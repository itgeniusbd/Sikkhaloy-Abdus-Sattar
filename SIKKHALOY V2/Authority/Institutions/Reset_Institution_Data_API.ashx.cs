using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace EDUCATION.COM.Authority.Institutions
{
    public class Reset_Institution_Data_API : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
    {
        private string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["RegistrationID"] == null)
            {
                WriteJson(context, new { ok = false, message = "Session expired. Please login again." });
                return;
            }

            if (!IsAuthorized())
            {
                context.Response.StatusCode = 403;
                WriteJson(context, new { ok = false, message = "You are not authorized." });
                return;
            }

            string action = (context.Request["action"] ?? "").Trim().ToLowerInvariant();
            string mode = (context.Request["mode"] ?? "").Trim().ToUpperInvariant();
            int schoolId;
            int confirmId;
            int educationYearId = 0;
            long totalRowsEstimate = 0;
            int.TryParse(context.Request["schoolId"], out schoolId);
            int.TryParse(context.Request["confirmSchoolId"], out confirmId);
            int.TryParse(context.Request["educationYearId"], out educationYearId);
            long.TryParse(context.Request["totalRows"], out totalRowsEstimate);

            if (schoolId <= 0)
            {
                WriteJson(context, new { ok = false, message = "Invalid SchoolID." });
                return;
            }

            try
            {
                if (action == "progress")
                {
                    Progress(context, schoolId);
                    return;
                }

                if (mode != "FULL" && mode != "SESSION" && mode != "PURGE")
                {
                    WriteJson(context, new { ok = false, message = "Mode must be FULL, SESSION or PURGE." });
                    return;
                }

                if (action == "preview")
                {
                    Preview(context, schoolId, mode, educationYearId);
                    return;
                }

                if (action == "execute")
                {
                    if (confirmId != schoolId)
                    {
                        WriteJson(context, new { ok = false, message = "Confirmation failed. Type the exact School ID (" + schoolId + ")." });
                        return;
                    }

                    if (mode == "PURGE")
                    {
                        string word = (context.Request["confirmWord"] ?? "").Trim();
                        if (!string.Equals(word, "DELETE", StringComparison.Ordinal))
                        {
                            WriteJson(context, new { ok = false, message = "Type DELETE (capital letters) to confirm permanent delete." });
                            return;
                        }
                    }

                    if (mode == "SESSION" && educationYearId <= 0)
                    {
                        WriteJson(context, new { ok = false, message = "Please select a session." });
                        return;
                    }

                    Execute(context, schoolId, mode, educationYearId > 0 ? (int?)educationYearId : null, confirmId, totalRowsEstimate);
                    return;
                }

                WriteJson(context, new { ok = false, message = "Unknown action. Use preview, progress or execute." });
            }
            catch (Exception ex)
            {
                WriteJson(context, new { ok = false, message = ex.Message });
            }
        }

        private void Progress(HttpContext context, int schoolId)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();
                EnsureProgressTable(con);

                using (SqlCommand cmd = new SqlCommand(@"
SELECT Mode, EducationYearID, TotalRows, DeletedRows, Status, Message, UpdatedAt
FROM dbo.Institution_Reset_Progress WHERE SchoolID = @SchoolID;", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            WriteJson(context, new
                            {
                                ok = true,
                                hasProgress = false,
                                deletedRows = 0,
                                totalRows = 0,
                                status = "Idle",
                                percent = 0
                            });
                            return;
                        }

                        long deleted = reader["DeletedRows"] == DBNull.Value ? 0 : Convert.ToInt64(reader["DeletedRows"]);
                        long total = reader["TotalRows"] == DBNull.Value ? 0 : Convert.ToInt64(reader["TotalRows"]);
                        string status = Convert.ToString(reader["Status"]) ?? "";
                        int percent = 0;
                        if (total > 0)
                        {
                            percent = (int)Math.Min(100, Math.Round(100.0 * deleted / total));
                            if (string.Equals(status, "Running", StringComparison.OrdinalIgnoreCase) && percent >= 100)
                                percent = 99;
                        }

                        WriteJson(context, new
                        {
                            ok = true,
                            hasProgress = true,
                            mode = reader["Mode"] == DBNull.Value ? null : Convert.ToString(reader["Mode"]),
                            deletedRows = deleted,
                            totalRows = total,
                            status = status,
                            message = reader["Message"] == DBNull.Value ? null : Convert.ToString(reader["Message"]),
                            percent = percent
                        });
                    }
                }
            }
        }

        private static void EnsureProgressTable(SqlConnection con)
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Institution_Reset_Progress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Institution_Reset_Progress
    (
        SchoolID         INT            NOT NULL CONSTRAINT PK_Institution_Reset_Progress PRIMARY KEY,
        Mode             VARCHAR(20)    NOT NULL,
        EducationYearID  INT            NULL,
        TotalRows        BIGINT         NOT NULL CONSTRAINT DF_InstResetProg_Total DEFAULT (0),
        DeletedRows      BIGINT         NOT NULL CONSTRAINT DF_InstResetProg_Deleted DEFAULT (0),
        Status           NVARCHAR(20)   NOT NULL,
        Message          NVARCHAR(500)  NULL,
        UpdatedAt        DATETIME2(0)   NOT NULL CONSTRAINT DF_InstResetProg_Updated DEFAULT (SYSUTCDATETIME())
    );
END", con))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void UpsertProgressRunning(SqlConnection con, int schoolId, string mode, int? educationYearId, long totalRows)
        {
            EnsureProgressTable(con);
            using (SqlCommand cmd = new SqlCommand(@"
MERGE dbo.Institution_Reset_Progress AS t
USING (SELECT @SchoolID AS SchoolID) AS s ON t.SchoolID = s.SchoolID
WHEN MATCHED THEN UPDATE SET
    Mode = @Mode,
    EducationYearID = @EducationYearID,
    TotalRows = @TotalRows,
    DeletedRows = 0,
    Status = N'Running',
    Message = NULL,
    UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT
    (SchoolID, Mode, EducationYearID, TotalRows, DeletedRows, Status, Message, UpdatedAt)
VALUES
    (@SchoolID, @Mode, @EducationYearID, @TotalRows, 0, N'Running', NULL, SYSUTCDATETIME());", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@Mode", mode);
                cmd.Parameters.AddWithValue("@EducationYearID", (object)educationYearId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalRows", totalRows < 0 ? 0 : totalRows);
                cmd.ExecuteNonQuery();
            }
        }

        private static void FinishProgress(SqlConnection con, int schoolId, long deletedRows, bool ok, string message)
        {
            try
            {
                EnsureProgressTable(con);
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Institution_Reset_Progress
SET DeletedRows = @DeletedRows,
    Status = @Status,
    Message = @Message,
    UpdatedAt = SYSUTCDATETIME()
WHERE SchoolID = @SchoolID;", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    cmd.Parameters.AddWithValue("@DeletedRows", deletedRows);
                    cmd.Parameters.AddWithValue("@Status", ok ? "Done" : "Error");
                    cmd.Parameters.AddWithValue("@Message", (object)message ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                /* ignore progress finish errors */
            }
        }

        private void Preview(HttpContext context, int schoolId, string mode, int educationYearId)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("dbo.sp_InstitutionData_Preview", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@Mode", mode);
                cmd.Parameters.AddWithValue("@EducationYearID", educationYearId > 0 ? (object)educationYearId : DBNull.Value);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        WriteJson(context, new { ok = false, message = "Preview returned no data." });
                        return;
                    }

                    var summary = new
                    {
                        status = Convert.ToString(reader["Status"]),
                        schoolId = Convert.ToInt32(reader["SchoolID"]),
                        schoolName = Convert.ToString(reader["SchoolName"]),
                        mode = Convert.ToString(reader["Mode"]),
                        educationYearId = reader["EducationYearID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EducationYearID"]),
                        activeUsers = Convert.ToInt32(reader["ActiveUsers"]),
                        totalRows = Convert.ToInt64(reader["TotalRows"])
                    };

                    var tables = new List<object>();
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            tables.Add(new
                            {
                                tableName = Convert.ToString(reader["TableName"]),
                                rowCnt = Convert.ToInt64(reader["RowCnt"])
                            });
                        }
                    }

                    WriteJson(context, new
                    {
                        ok = true,
                        summary = summary,
                        tables = tables
                    });
                }
            }
        }

        private void Execute(HttpContext context, int schoolId, string mode, int? educationYearId, int confirmId, long totalRowsEstimate)
        {
            var sw = Stopwatch.StartNew();

            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();

                // UI poll sees Running immediately (before long SP work)
                UpsertProgressRunning(con, schoolId, mode, educationYearId, totalRowsEstimate);

                using (SqlCommand cmd = new SqlCommand("dbo.sp_ResetInstitutionData", con))
                {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 900;
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@Mode", mode);
                cmd.Parameters.AddWithValue("@EducationYearID", (object)educationYearId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConfirmSchoolID", confirmId);
                cmd.Parameters.AddWithValue("@TotalRowsEstimate", totalRowsEstimate > 0 ? (object)totalRowsEstimate : DBNull.Value);

                var deletedParam = cmd.Parameters.Add("@DeletedRows", SqlDbType.Int);
                deletedParam.Direction = ParameterDirection.Output;
                var msgParam = cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 500);
                msgParam.Direction = ParameterDirection.Output;

                // Triggers/dynamic SQL may emit extra result sets before the final Status row.
                // Scan all sets for a row that has Status (do not use Fill first-table only).
                string status = "";
                string message = "";
                int? deletedFromResult = null;
                int errorLine = 0;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    do
                    {
                        if (!HasColumn(reader, "Status"))
                            continue;

                        if (!reader.Read())
                            continue;

                        status = Convert.ToString(reader["Status"]) ?? "";
                        if (HasColumn(reader, "Message") && reader["Message"] != DBNull.Value)
                            message = Convert.ToString(reader["Message"]) ?? "";
                        if (HasColumn(reader, "DeletedRows") && reader["DeletedRows"] != DBNull.Value)
                            deletedFromResult = Convert.ToInt32(reader["DeletedRows"]);
                        if (HasColumn(reader, "ErrorLine") && reader["ErrorLine"] != DBNull.Value)
                            errorLine = Convert.ToInt32(reader["ErrorLine"]);
                        break;
                    }
                    while (reader.NextResult());
                }

                sw.Stop();

                if (string.IsNullOrEmpty(message) && msgParam.Value != null && msgParam.Value != DBNull.Value)
                    message = Convert.ToString(msgParam.Value) ?? "";

                int deleted = deletedFromResult.HasValue
                    ? deletedFromResult.Value
                    : (deletedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(deletedParam.Value));

                if (string.IsNullOrEmpty(status))
                {
                    // SP finished but no Status result set found
                    status = deleted > 0 ? "Success" : "Error";
                    if (string.IsNullOrEmpty(message))
                        message = deleted > 0
                            ? "Completed (no status row returned)."
                            : "No status result returned from stored procedure.";
                }

                if (errorLine > 0 && !string.IsNullOrEmpty(message))
                    message = message + " (line " + errorLine + ")";

                bool ok = string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase);
                FinishProgress(con, schoolId, deleted, ok, message);
                WriteJson(context, new
                {
                    ok = ok,
                    status = status,
                    message = message,
                    deletedRows = deleted,
                    elapsedMs = sw.ElapsedMilliseconds,
                    elapsedText = FormatElapsed(sw.Elapsed)
                });
                } // SqlCommand
            } // SqlConnection
        }

        private static bool HasColumn(SqlDataReader reader, string name)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string FormatElapsed(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return string.Format("{0}h {1}m {2}s", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
            if (ts.TotalMinutes >= 1)
                return string.Format("{0}m {1}s", (int)ts.TotalMinutes, ts.Seconds);
            return string.Format("{0}.{1:000}s", ts.Seconds, ts.Milliseconds);
        }

        private static bool IsAuthorized()
        {
            return Roles.IsUserInRole("Authority") || Roles.IsUserInRole("Sub-Authority");
        }

        private static void WriteJson(HttpContext context, object data)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(data));
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
