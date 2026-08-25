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

    public async Task<ResetProgressDto> GetResetProgressAsync(
        SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        Guard(session);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureProgressTableAsync(con, ct);
        await using var cmd = new SqlCommand("""
SELECT Mode, EducationYearID, TotalRows, DeletedRows, Status, Message, UpdatedAt
FROM dbo.Institution_Reset_Progress WHERE SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return new ResetProgressDto { Ok = true, HasProgress = false };

        var deleted = L(reader["DeletedRows"]);
        var total = L(reader["TotalRows"]);
        var status = S(reader["Status"]);
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
            Mode = S(reader["Mode"]),
            DeletedRows = deleted,
            TotalRows = total,
            Status = status,
            Message = reader["Message"] is DBNull ? null : S(reader["Message"]),
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
        await UpsertProgressRunningAsync(con, schoolId, mode, educationYearId, totalRowsEstimate, CancellationToken.None);

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

        sw.Stop();
        if (string.IsNullOrEmpty(message) && msgParam.Value is not null and not DBNull)
            message = Convert.ToString(msgParam.Value) ?? "";

        var deleted = deletedFromResult
                      ?? (deletedParam.Value is DBNull or null ? 0 : Convert.ToInt32(deletedParam.Value));

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
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@DeletedRows", deletedRows);
            cmd.Parameters.AddWithValue("@Status", ok ? "Done" : "Error");
            cmd.Parameters.AddWithValue("@Message", (object?)message ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
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
