using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityBasicService
{
    public async Task<IReadOnlyList<ResetSchoolOptionDto>> GetResetSchoolsAsync(
        SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var items = new List<ResetSchoolOptionDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT SchoolID, CAST(SchoolID AS NVARCHAR(20)) + N' - ' + SchoolName AS DisplayText
FROM dbo.SchoolInfo
ORDER BY SchoolID DESC
""", con);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new ResetSchoolOptionDto
            {
                SchoolID = I(reader["SchoolID"]),
                Name = S(reader["DisplayText"])
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<ResetYearOptionDto>> GetResetYearsAsync(
        SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        Guard(session);
        var items = new List<ResetYearOptionDto>();
        if (schoolId <= 0)
            return items;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT EducationYearID,
       CAST(EducationYear AS NVARCHAR(20)) + N' (ID: ' + CAST(EducationYearID AS NVARCHAR(20)) + N')' AS DisplayText
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID
ORDER BY EducationYearID DESC
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new ResetYearOptionDto
            {
                EducationYearID = I(reader["EducationYearID"]),
                Name = S(reader["DisplayText"])
            });
        }
        return items;
    }

    public async Task<ResetPreviewDto> PreviewResetAsync(
        SessionSnapshot session, int schoolId, string mode, int educationYearId, CancellationToken ct)
    {
        Guard(session);
        mode = (mode ?? "").Trim().ToUpperInvariant();
        if (schoolId <= 0)
            return new ResetPreviewDto { Ok = false, Message = "Invalid SchoolID." };
        if (mode is not ("FULL" or "SESSION" or "PURGE"))
            return new ResetPreviewDto { Ok = false, Message = "Mode must be FULL, SESSION or PURGE." };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            return await PreviewResetFromProcAsync(con, schoolId, mode, educationYearId, ct);
        }
        catch (SqlException ex) when (IsExecuteDenied(ex))
        {
            return await PreviewResetInlineAsync(con, schoolId, mode, educationYearId, ct);
        }
    }

    private static async Task<ResetPreviewDto> PreviewResetFromProcAsync(
        SqlConnection con, int schoolId, string mode, int educationYearId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("dbo.sp_InstitutionData_Preview", con)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Mode", mode);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId > 0 ? educationYearId : DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return new ResetPreviewDto { Ok = false, Message = "Preview returned no data." };

        var dto = new ResetPreviewDto
        {
            Ok = true,
            Status = S(reader["Status"]),
            SchoolId = I(reader["SchoolID"]),
            SchoolName = S(reader["SchoolName"]),
            Mode = S(reader["Mode"]),
            EducationYearId = reader["EducationYearID"] is DBNull ? null : I(reader["EducationYearID"]),
            ActiveUsers = I(reader["ActiveUsers"]),
            TotalRows = L(reader["TotalRows"])
        };

        if (await reader.NextResultAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Tables.Add(new ResetTableCountDto
                {
                    TableName = S(reader["TableName"]),
                    RowCnt = L(reader["RowCnt"])
                });
            }
        }
        return dto;
    }

    private static async Task<ResetPreviewDto> PreviewResetInlineAsync(
        SqlConnection con, int schoolId, string mode, int educationYearId, CancellationToken ct)
    {
        var dto = new ResetPreviewDto { Ok = true, SchoolId = schoolId, Mode = mode, Status = "Preview" };
        await using (var name = new SqlCommand("SELECT SchoolName FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con))
        {
            name.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await name.ExecuteScalarAsync(ct);
            dto.SchoolName = value is string s ? s : "";
            if (string.IsNullOrWhiteSpace(dto.SchoolName))
                return new ResetPreviewDto { Ok = false, Message = "Institution not found." };
        }

        string[] tables =
        [
            "StudentsClass", "Student", "StudentRecord",
            "Attendance_Record", "Attendance_Student", "Attendance_Leave",
            "Income_PayOrder", "Income_PaymentRecord", "Income_MoneyReceipt",
            "Exam_Obtain_Marks", "Exam_Result_of_Student", "Exam_Result_of_Subject",
            "Exam_Name", "Teacher", "Subject", "CreateClass",
            "Account_Log", "AccountIN_Record", "AccountOUT_Record",
            "Expenditure", "Extra_Income",
            "SMS_Send_Record", "SMS_OtherInfo",
            "Employee_Info", "Employee_Payorder",
            "Education_Year", "Registration", "AST", "SMS"
        ];
        dto.Tables.AddRange(await LoadResetTableCountsAsync(con, schoolId, mode, educationYearId, tables, nolock: false, ct));

        dto.Tables.Sort((a, b) => b.RowCnt.CompareTo(a.RowCnt));
        dto.TotalRows = dto.Tables.Sum(x => x.RowCnt);
        return dto;
    }

    private static async Task<(long Remaining, int TablesCounted)> CountResetRemainingAsync(
        SqlConnection con, int schoolId, string mode, int educationYearId, CancellationToken ct)
    {
        string[] tables =
        [
            "StudentsClass", "Student", "StudentRecord",
            "Attendance_Record", "Attendance_Student", "Attendance_Leave",
            "Income_PayOrder", "Income_PaymentRecord", "Income_MoneyReceipt",
            "Exam_Obtain_Marks", "Exam_Result_of_Student", "Exam_Result_of_Subject",
            "Exam_Name", "Teacher", "Subject", "CreateClass",
            "Account_Log", "AccountIN_Record", "AccountOUT_Record",
            "Expenditure", "Extra_Income",
            "SMS_Send_Record", "SMS_OtherInfo",
            "Employee_Info", "Employee_Payorder",
            "Education_Year", "Registration", "AST", "SMS"
        ];
        var rows = await LoadResetTableCountsAsync(con, schoolId, mode, educationYearId, tables, nolock: true, ct);
        return (rows.Sum(x => x.RowCnt), rows.Count);
    }

    private static async Task<List<ResetTableCountDto>> LoadResetTableCountsAsync(
        SqlConnection con, int schoolId, string mode, int educationYearId, string[] tables, bool nolock, CancellationToken ct)
    {
        string[] sessionSkip = ["SchoolInfo", "Registration", "AST", "SMS", "Teacher", "Subject", "CreateClass", "Student"];
        var hint = nolock ? " WITH (NOLOCK)" : "";
        var items = new List<ResetTableCountDto>();
        foreach (var table in tables)
        {
            if (mode == "FULL" && table == "SchoolInfo")
                continue;
            if (nolock && mode == "FULL" && table is
                "Education_Year" or "Registration" or "AST" or "SMS"
                or "Teacher" or "Subject" or "CreateClass" or "Employee_Info")
                continue;
            if (!await TableHasColumnAsync(con, table, "SchoolID", ct))
                continue;
            var useYear = mode == "SESSION"
                          && await TableHasColumnAsync(con, table, "EducationYearID", ct)
                          && !sessionSkip.Contains(table, StringComparer.OrdinalIgnoreCase);
            var sql = useYear
                ? $"SELECT COUNT_BIG(*) FROM dbo.[{table}]{hint} WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID"
                : mode is "FULL" or "PURGE" or "SESSION"
                    ? $"SELECT COUNT_BIG(*) FROM dbo.[{table}]{hint} WHERE SchoolID = @SchoolID"
                    : null;
            if (sql is null)
                continue;
            if (mode == "SESSION" && !useYear)
                continue;
            try
            {
                await using var cmd = new SqlCommand(sql, con);
                cmd.CommandTimeout = 4;
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                if (useYear)
                    cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
                var value = await cmd.ExecuteScalarAsync(ct);
                var count = value is long l ? l : Convert.ToInt64(value ?? 0);
                if (count > 0)
                    items.Add(new ResetTableCountDto { TableName = table, RowCnt = count });
            }
            catch (SqlException)
            {
            }
        }
        items.Sort((a, b) => b.RowCnt.CompareTo(a.RowCnt));
        return items;
    }

    private static async Task<bool> TableHasColumnAsync(SqlConnection con, string table, string column, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(
            "SELECT CASE WHEN COL_LENGTH(N'dbo.' + @Table, @Column) IS NULL THEN 0 ELSE 1 END", con);
        cmd.Parameters.AddWithValue("@Table", table);
        cmd.Parameters.AddWithValue("@Column", column);
        var value = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(value) == 1;
    }

    private static bool IsExecuteDenied(SqlException ex) =>
        ex.Number == 229 || ex.Message.Contains("EXECUTE permission was denied", StringComparison.OrdinalIgnoreCase);

    public async Task<ResetProgressDto> GetResetProgressAsync(
        SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        Guard(session);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureProgressTableAsync(con, ct);
        await using var cmd = new SqlCommand("""
SELECT Mode, EducationYearID, TotalRows, DeletedRows, Status, Message, UpdatedAt
FROM dbo.Institution_Reset_Progress WITH (NOLOCK)
WHERE SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        string mode;
        long deleted;
        long total;
        string status;
        string? message;
        int yearId;
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct))
                return new ResetProgressDto { Ok = true, HasProgress = false };

            deleted = L(reader["DeletedRows"]);
            total = L(reader["TotalRows"]);
            status = S(reader["Status"]);
            mode = S(reader["Mode"]);
            yearId = reader["EducationYearID"] is DBNull ? 0 : I(reader["EducationYearID"]);
            message = reader["Message"] is DBNull ? null : S(reader["Message"]);
        }

        if (string.Equals(status, "Done", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase))
        {
            if (total > 0)
                deleted = total;
        }
        else if (string.Equals(status, "Running", StringComparison.OrdinalIgnoreCase) && total > 0)
        {
            if (deleted >= total)
            {
                await TryEnableLedgerTriggersAsync(con, ct);
                await FinishProgressAsync(con, schoolId, total, true,
                    "Institution reset to new-signup state successfully.", ct);
                status = "Done";
                deleted = total;
                message = "Institution reset to new-signup state successfully.";
            }
            else
            {
                var rawDeleted = deleted;
                long remaining = -1;
                try
                {
                    (remaining, _) = await CountResetRemainingAsync(con, schoolId, mode, yearId, ct);
                }
                catch
                {
                    remaining = -1;
                }
                if (remaining >= 0)
                {
                    var estimated = Math.Max(0, total - remaining);
                    if (estimated > deleted)
                        deleted = estimated;
                }

                if (deleted > total)
                    deleted = total;

                var leftoverOk = remaining >= 0 && remaining <= Math.Max(80, total / 20);
                var mostlyGone = remaining == 0
                                 || (leftoverOk && (rawDeleted >= total || deleted >= total * 85 / 100))
                                 || deleted >= total;
                if (mostlyGone)
                {
                    await TryEnableLedgerTriggersAsync(con, ct);
                    await FinishProgressAsync(con, schoolId, total, true,
                        "Institution reset to new-signup state successfully.", ct);
                    status = "Done";
                    deleted = total;
                    message = "Institution reset to new-signup state successfully.";
                }
            }
        }

        if (total > 0 && deleted > total)
            deleted = total;

        var percent = 0;
        if (total > 0)
        {
            percent = (int)Math.Min(100, Math.Round(100.0 * deleted / total));
            if (string.Equals(status, "Running", StringComparison.OrdinalIgnoreCase) && percent >= 100)
                percent = 99;
        }

        return new ResetProgressDto
        {
            Ok = true,
            HasProgress = true,
            Mode = mode,
            DeletedRows = deleted,
            TotalRows = total,
            Status = status,
            Message = message,
            Percent = percent
        };
    }

    public async Task<AuthorityResult> StartResetAsync(
        SessionSnapshot session, ResetExecuteRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new ResetExecuteRequest();
        var mode = (request.Mode ?? "").Trim().ToUpperInvariant();
        var schoolId = request.SchoolId;
        if (schoolId <= 0)
            return Fail("Invalid SchoolID.");
        if (mode is not ("FULL" or "SESSION" or "PURGE"))
            return Fail("Mode must be FULL, SESSION or PURGE.");
        if (request.ConfirmSchoolId != schoolId)
            return Fail("Confirmation failed. Type the exact School ID (" + schoolId + ").");
        if (mode == "PURGE" && !string.Equals((request.ConfirmWord ?? "").Trim(), "DELETE", StringComparison.Ordinal))
            return Fail("Type DELETE (capital letters) to confirm permanent delete.");
        if (mode == "SESSION" && request.EducationYearId <= 0)
            return Fail("Please select a session.");

        await using (var con = _connections.Create())
        {
            await con.OpenAsync(ct);
            await UpsertProgressRunningAsync(con, schoolId, mode,
                request.EducationYearId > 0 ? request.EducationYearId : null,
                request.TotalRows, ct);
        }

        var yearId = request.EducationYearId;
        var confirmId = request.ConfirmSchoolId;
        var totalRows = request.TotalRows;
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _scopes.CreateAsyncScope();
                var inner = scope.ServiceProvider.GetRequiredService<AuthorityBasicService>();
                await inner.ExecuteResetCoreAsync(schoolId, mode, yearId > 0 ? yearId : null, confirmId, totalRows);
            }
            catch (Exception ex)
            {
                try
                {
                    await using var con = _connections.Create();
                    await con.OpenAsync();
                    await FinishProgressAsync(con, schoolId, 0, false, ex.Message, CancellationToken.None);
                }
                catch
                {
                }
            }
        });

        return Ok("started");
    }

    internal async Task ExecuteResetCoreAsync(
        int schoolId, string mode, int? educationYearId, int confirmId, long totalRowsEstimate)
    {
        var sw = Stopwatch.StartNew();
        await using var con = _connections.Create();
        await con.OpenAsync();
        await EnsureProgressTableAsync(con, CancellationToken.None);

        await using var cmd = new SqlCommand("dbo.sp_ResetInstitutionData", con)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 900
        };
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Mode", mode);
        cmd.Parameters.AddWithValue("@EducationYearID", (object?)educationYearId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ConfirmSchoolID", confirmId);
        cmd.Parameters.AddWithValue("@TotalRowsEstimate", totalRowsEstimate > 0 ? totalRowsEstimate : DBNull.Value);
        var deletedParam = cmd.Parameters.Add("@DeletedRows", SqlDbType.Int);
        deletedParam.Direction = ParameterDirection.Output;
        var msgParam = cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 500);
        msgParam.Direction = ParameterDirection.Output;

        var status = "";
        var message = "";
        int? deletedFromResult = null;
        var errorLine = 0;

        try
        {
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                do
                {
                    if (!HasColumn(reader, "Status"))
                        continue;
                    if (!await reader.ReadAsync())
                        continue;
                    status = S(reader["Status"]);
                    if (HasColumn(reader, "Message") && reader["Message"] is not DBNull)
                        message = S(reader["Message"]);
                    if (HasColumn(reader, "DeletedRows") && reader["DeletedRows"] is not DBNull)
                        deletedFromResult = Convert.ToInt32(reader["DeletedRows"]);
                    if (HasColumn(reader, "ErrorLine") && reader["ErrorLine"] is not DBNull)
                        errorLine = Convert.ToInt32(reader["ErrorLine"]);
                    break;
                }
                while (await reader.NextResultAsync());
            }
        }
        catch (SqlException ex) when (IsExecuteDenied(ex))
        {
            await FinishProgressAsync(con, schoolId, 0, false,
                "EXECUTE permission denied on sp_ResetInstitutionData. Run Database_Scripts/Grant_InstitutionReset_Execute.sql on the Edu database.",
                CancellationToken.None);
            return;
        }

        sw.Stop();
        if (string.IsNullOrEmpty(message) && msgParam.Value is not null and not DBNull)
            message = Convert.ToString(msgParam.Value) ?? "";

        var deleted = deletedFromResult
                      ?? (deletedParam.Value is DBNull or null ? 0 : Convert.ToInt32(deletedParam.Value));
        if (deleted < 0) deleted = 0;

        if (string.IsNullOrEmpty(status))
        {
            status = deleted > 0 ? "Success" : "Error";
            if (string.IsNullOrEmpty(message))
            {
                message = deleted > 0
                    ? "Completed (no status row returned)."
                    : "No status result returned from stored procedure.";
            }
        }

        if (errorLine > 0 && !string.IsNullOrEmpty(message))
            message += " (line " + errorLine + ")";

        var ok = string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase);
        if (totalRowsEstimate > 0)
        {
            if (deleted > totalRowsEstimate)
                deleted = (int)Math.Min(int.MaxValue, totalRowsEstimate);
            else if (ok && deleted < totalRowsEstimate)
                deleted = (int)Math.Min(int.MaxValue, totalRowsEstimate);
        }
        if (ok && string.IsNullOrEmpty(message))
            message = "Done in " + FormatElapsed(sw.Elapsed) + ". Deleted rows: " + deleted;
        await FinishProgressAsync(con, schoolId, deleted, ok, message, CancellationToken.None);
    }

    private static async Task EnsureProgressTableAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
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
END
""", con);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpsertProgressRunningAsync(
        SqlConnection con, int schoolId, string mode, int? educationYearId, long totalRows, CancellationToken ct)
    {
        await EnsureProgressTableAsync(con, ct);
        await using var cmd = new SqlCommand("""
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
    (@SchoolID, @Mode, @EducationYearID, @TotalRows, 0, N'Running', NULL, SYSUTCDATETIME());
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Mode", mode);
        cmd.Parameters.AddWithValue("@EducationYearID", (object?)educationYearId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TotalRows", totalRows < 0 ? 0 : totalRows);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task FinishProgressAsync(
        SqlConnection con, int schoolId, long deletedRows, bool ok, string message, CancellationToken ct)
    {
        try
        {
            await EnsureProgressTableAsync(con, ct);
            await using var cmd = new SqlCommand("""
UPDATE dbo.Institution_Reset_Progress
SET DeletedRows = @DeletedRows,
    Status = @Status,
    Message = @Message,
    UpdatedAt = SYSUTCDATETIME()
WHERE SchoolID = @SchoolID
  AND (@Ok = 1 OR Status = N'Running')
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@DeletedRows", deletedRows);
            cmd.Parameters.AddWithValue("@Status", ok ? "Done" : "Error");
            cmd.Parameters.AddWithValue("@Message", (object?)message ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Ok", ok ? 1 : 0);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
        }
    }

    private static async Task TryEnableLedgerTriggersAsync(SqlConnection con, CancellationToken ct)
    {
        string[] tables =
        [
            "AccountIN_Record", "AccountOUT_Record", "Income_PaymentRecord", "Income_PayOrder",
            "Income_MoneyReceipt", "Expenditure", "Extra_Income"
        ];
        foreach (var table in tables)
        {
            try
            {
                await using var cmd = new SqlCommand(
                    "IF OBJECT_ID(N'dbo.' + @Table, N'U') IS NOT NULL EXEC(N'ENABLE TRIGGER ALL ON dbo.' + QUOTENAME(@Table));",
                    con);
                cmd.Parameters.AddWithValue("@Table", table);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            catch
            {
            }
        }
    }

    private static bool HasColumn(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string FormatElapsed(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
        return $"{ts.Seconds}.{ts.Milliseconds:000}s";
    }

    public async Task<ResetPreviewDto> PreviewResetImagesAsync(
        SessionSnapshot session, int schoolId, IReadOnlyList<int> yearIds, CancellationToken ct)
    {
        Guard(session);
        var years = CleanYearIds(yearIds);
        if (schoolId <= 0)
            return new ResetPreviewDto { Ok = false, Message = "Invalid SchoolID." };
        if (years.Count == 0)
            return new ResetPreviewDto { Ok = false, Message = "ab.selectSession" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await LoadImagePreviewAsync(con, schoolId, years, ct);
    }

    public async Task<ResetPreviewDto> DeleteResetImagesAsync(
        SessionSnapshot session, ResetImageRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new ResetImageRequest();
        var years = CleanYearIds(request.EducationYearIds);
        if (request.SchoolId <= 0)
            return new ResetPreviewDto { Ok = false, Message = "Invalid SchoolID." };
        if (request.ConfirmSchoolId != request.SchoolId)
            return new ResetPreviewDto { Ok = false, Message = "Confirmation failed. Type the exact School ID (" + request.SchoolId + ")." };
        if (years.Count == 0)
            return new ResetPreviewDto { Ok = false, Message = "ab.selectSession" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var preview = await LoadImagePreviewAsync(con, request.SchoolId, years, ct);
        if (!preview.Ok)
            return preview;

        await using (var cmd = new SqlCommand())
        {
            cmd.Connection = con;
            cmd.CommandTimeout = 180;
            BindImageSql(cmd, request.SchoolId, years, """
UPDATE si
SET si.Image = NULL, si.Guardian_Photo = NULL
FROM dbo.Student_Image si
INNER JOIN ImageTargets t ON t.StudentImageID = si.StudentImageID
WHERE ISNULL(DATALENGTH(si.Image), 0) > 0 OR ISNULL(DATALENGTH(si.Guardian_Photo), 0) > 0
""");
            var n = await cmd.ExecuteNonQueryAsync(ct);
            preview.TotalRows = n < 0 ? preview.TotalRows : n;
        }

        preview.Message = "Cleared student/guardian photos. Records stay; only image bytes were removed.";
        return preview;
    }

    private static async Task<ResetPreviewDto> LoadImagePreviewAsync(
        SqlConnection con, int schoolId, List<int> years, CancellationToken ct)
    {
        var dto = new ResetPreviewDto { Ok = true, SchoolId = schoolId, Mode = "IMAGES" };
        await using (var name = new SqlCommand("SELECT SchoolName FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con))
        {
            name.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await name.ExecuteScalarAsync(ct);
            dto.SchoolName = value is string s ? s : "";
            if (string.IsNullOrWhiteSpace(dto.SchoolName))
                return new ResetPreviewDto { Ok = false, Message = "Institution not found." };
        }

        await using (var cmd = new SqlCommand())
        {
            cmd.Connection = con;
            BindImageSql(cmd, schoolId, years, """
SELECT
  COUNT(*) AS ImageRows,
  SUM(CASE WHEN ISNULL(DATALENGTH(si.Image), 0) > 0 THEN 1 ELSE 0 END) AS Photos,
  SUM(CASE WHEN ISNULL(DATALENGTH(si.Guardian_Photo), 0) > 0 THEN 1 ELSE 0 END) AS Guardians,
  ISNULL(SUM(CONVERT(bigint, ISNULL(DATALENGTH(si.Image), 0)) + CONVERT(bigint, ISNULL(DATALENGTH(si.Guardian_Photo), 0))), 0) AS Bytes
FROM dbo.Student_Image si
INNER JOIN ImageTargets t ON t.StudentImageID = si.StudentImageID
""");
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var photos = L(reader["Photos"]);
                var guardians = L(reader["Guardians"]);
                dto.Bytes = L(reader["Bytes"]);
                dto.TotalRows = photos + guardians;
                dto.Tables.Add(new ResetTableCountDto { TableName = "Student photos", RowCnt = photos });
                dto.Tables.Add(new ResetTableCountDto { TableName = "Guardian photos", RowCnt = guardians });
                dto.Tables.Add(new ResetTableCountDto { TableName = "Bytes", RowCnt = dto.Bytes });
            }
        }

        await using (var skip = new SqlCommand())
        {
            skip.Connection = con;
            BindYearParams(skip, schoolId, years);
            skip.CommandText = $"""
SELECT COUNT(DISTINCT s.StudentID)
FROM dbo.Student s
INNER JOIN dbo.StudentsClass sc ON sc.StudentID = s.StudentID AND sc.SchoolID = s.SchoolID
WHERE s.SchoolID = @SchoolID
  AND ISNULL(s.StudentImageID, 0) > 0
  AND sc.EducationYearID IN ({YearParams(years)})
  AND EXISTS (
      SELECT 1 FROM dbo.StudentsClass o
      WHERE o.StudentID = s.StudentID AND o.SchoolID = @SchoolID
        AND o.EducationYearID NOT IN ({YearParams(years)})
  )
""";
            var value = await skip.ExecuteScalarAsync(ct);
            dto.SkippedStudents = value is int i ? i : Convert.ToInt32(value ?? 0);
        }

        return dto;
    }

    private static void BindImageSql(SqlCommand cmd, int schoolId, List<int> years, string body)
    {
        BindYearParams(cmd, schoolId, years);
        cmd.CommandText = $"""
;WITH ImageTargets AS (
    SELECT DISTINCT s.StudentImageID
    FROM dbo.Student s
    INNER JOIN dbo.StudentsClass sc ON sc.StudentID = s.StudentID AND sc.SchoolID = s.SchoolID
    WHERE s.SchoolID = @SchoolID
      AND ISNULL(s.StudentImageID, 0) > 0
      AND sc.EducationYearID IN ({YearParams(years)})
      AND NOT EXISTS (
          SELECT 1 FROM dbo.StudentsClass o
          WHERE o.StudentID = s.StudentID AND o.SchoolID = @SchoolID
            AND o.EducationYearID NOT IN ({YearParams(years)})
      )
)
{body}
""";
    }

    private static void BindYearParams(SqlCommand cmd, int schoolId, List<int> years)
    {
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        for (var i = 0; i < years.Count; i++)
            cmd.Parameters.AddWithValue("@Y" + i, years[i]);
    }

    private static string YearParams(List<int> years) =>
        string.Join(",", years.Select((_, i) => "@Y" + i));

    private static List<int> CleanYearIds(IReadOnlyList<int>? yearIds) =>
        (yearIds ?? []).Where(x => x > 0).Distinct().Take(40).ToList();
}
