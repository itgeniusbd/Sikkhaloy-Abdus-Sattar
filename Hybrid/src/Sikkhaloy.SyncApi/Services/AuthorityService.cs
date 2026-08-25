using System.Text;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityService
{
    private readonly EduConnectionFactory _connections;

    public AuthorityService(EduConnectionFactory connections) => _connections = connections;

    public async Task<AuthorityDashboardDto> GetDashboardAsync(SessionSnapshot session, CancellationToken ct)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await CleanSessionsAsync(con, ct);

        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var lastStart = monthStart.AddMonths(-1);
        var nextStart = monthStart.AddMonths(1);

        var dto = new AuthorityDashboardDto
        {
            AllInstitutions = await ScalarIntAsync(con, "SELECT COUNT(*) FROM dbo.SchoolInfo", ct),
            Active15m = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND LastActivity >= DATEADD(MINUTE, -15, GETDATE())
""", ct),
            Today = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND CAST(LoginTime AS DATE) = CAST(GETDATE() AS DATE)
""", ct),
            LastHour = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND LastActivity >= DATEADD(HOUR, -1, GETDATE())
""", ct),
            Online5m = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND LastActivity >= DATEADD(MINUTE, -5, GETDATE())
""", ct),
            TotalUsers = await ScalarIntAsync(con, """
SELECT COUNT(*) FROM dbo.Registration
WHERE Category IN (N'Admin', N'Sub-Admin', N'Teacher', N'Student')
""", ct),
            ActiveUsers = await ScalarIntAsync(con, """
SELECT COUNT(*) FROM dbo.User_Active_Sessions
WHERE LastActivity >= DATEADD(MINUTE, -15, GETDATE())
""", ct)
        };

        await using (var count = new SqlCommand("""
SELECT COUNT(*) AS TotalCount,
       SUM(CASE WHEN Validation = N'Valid' THEN 1 ELSE 0 END) AS ValidCount,
       SUM(CASE WHEN Validation = N'Invalid' THEN 1 ELSE 0 END) AS InvalidCount,
       SUM(CASE WHEN Date IS NOT NULL AND YEAR(Date) = YEAR(GETDATE()) THEN 1 ELSE 0 END) AS NewYear
FROM dbo.SchoolInfo
""", con))
        {
            await using var reader = await count.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Total = I(reader["TotalCount"]);
                dto.Valid = I(reader["ValidCount"]);
                dto.Invalid = I(reader["InvalidCount"]);
                dto.NewThisYear = I(reader["NewYear"]);
            }
        }

        dto.AllInstitutions = dto.Total;
        dto.Yearly = await LoadYearlyAsync(con, now.Year, ct);
        dto.MonthPaid = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(Amount), 0) FROM dbo.AAP_Invoice_Payment_Record
WHERE PaidDate >= @Start AND PaidDate < @End
""", monthStart, nextStart, ct);
        dto.LastMonthPaid = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(Amount), 0) FROM dbo.AAP_Invoice_Payment_Record
WHERE PaidDate >= @Start AND PaidDate < @End
""", lastStart, monthStart, ct);
        dto.MonthPayable = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(ISNULL(TotalAmount, 0) - ISNULL(Discount, 0)), 0) FROM dbo.AAP_Invoice
WHERE IssuDate >= @Start AND IssuDate < @End
""", monthStart, nextStart, ct);
        dto.LastMonthPayable = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(ISNULL(TotalAmount, 0) - ISNULL(Discount, 0)), 0) FROM dbo.AAP_Invoice
WHERE IssuDate >= @Start AND IssuDate < @End
""", lastStart, monthStart, ct);
        dto.MonthDue = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0)), 0) FROM dbo.AAP_Invoice
WHERE IssuDate >= @Start AND IssuDate < @End
  AND ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0) > 0
""", monthStart, nextStart, ct);
        dto.LastMonthDue = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0)), 0) FROM dbo.AAP_Invoice
WHERE IssuDate >= @Start AND IssuDate < @End
  AND ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0) > 0
""", lastStart, monthStart, ct);
        dto.OutstandingDue = await ScalarDecAsync(con, """
SELECT ISNULL(SUM(ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0)), 0) FROM dbo.AAP_Invoice
WHERE ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0) > 0
""", null, null, ct);
        dto.DueInstitutions = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.AAP_Invoice
WHERE ISNULL(TotalAmount, 0) - ISNULL(PaidAmount, 0) - ISNULL(Discount, 0) > 0
""", ct);
        dto.TopPaid = await LoadTopPaidAsync(con, monthStart, nextStart, ct);
        dto.Rows = await LoadRowsAsync(con, 15, ct);
        return dto;
    }

    public async Task<AuthorityDashboardDto> GetInstitutionsAsync(
        SessionSnapshot session,
        string? q,
        string? validation,
        string? live,
        DateTime? from,
        DateTime? to,
        CancellationToken ct)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await CleanSessionsAsync(con, ct);

        var dto = new AuthorityDashboardDto
        {
            AllInstitutions = await ScalarIntAsync(con, "SELECT COUNT(*) FROM dbo.SchoolInfo", ct),
            Active15m = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND LastActivity >= DATEADD(MINUTE, -15, GETDATE())
""", ct),
            Today = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND CAST(LoginTime AS DATE) = CAST(GETDATE() AS DATE)
""", ct),
            LastHour = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND LastActivity >= DATEADD(HOUR, -1, GETDATE())
""", ct),
            Online5m = await ScalarIntAsync(con, """
SELECT COUNT(DISTINCT SchoolID) FROM dbo.User_Active_Sessions
WHERE SchoolID IS NOT NULL AND LastActivity >= DATEADD(MINUTE, -5, GETDATE())
""", ct)
        };

        var where = new StringBuilder();
        await using var cmd = new SqlCommand { Connection = con };
        AppendFilters(where, cmd, q, validation, live, from, to);
        var whereSql = where.Length == 0 ? "" : " WHERE " + where;

        await using (var count = new SqlCommand($"""
SELECT COUNT(*) AS TotalCount,
       SUM(CASE WHEN Sch.Validation = N'Valid' THEN 1 ELSE 0 END) AS ValidCount,
       SUM(CASE WHEN Sch.Validation = N'Invalid' THEN 1 ELSE 0 END) AS InvalidCount
FROM dbo.SchoolInfo AS Sch
{whereSql}
""", con))
        {
            CopyParameters(cmd, count);
            await using var reader = await count.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Total = I(reader["TotalCount"]);
                dto.Valid = I(reader["ValidCount"]);
                dto.Invalid = I(reader["InvalidCount"]);
            }
        }

        dto.Rows = await LoadRowsAsync(con, 0, ct, whereSql, cmd);
        return dto;
    }

    public async Task<InstitutionDetailsDto> GetInstitutionDetailsAsync(SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");

        var dto = new InstitutionDetailsDto { SchoolID = schoolId };
        if (schoolId <= 0)
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT SchoolID, SchoolName, Principal, Phone, Email, Address, UserName, Validation
FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return dto;

            dto.SchoolID = I(reader["SchoolID"]);
            dto.SchoolName = S(reader["SchoolName"]);
            dto.Principal = S(reader["Principal"]);
            dto.Phone = S(reader["Phone"]);
            dto.Email = S(reader["Email"]);
            dto.Address = S(reader["Address"]);
            dto.UserName = S(reader["UserName"]);
            dto.Validation = S(reader["Validation"]);
        }

        await using (var years = new SqlCommand("""
SELECT ey.EducationYearID, ISNULL(ey.SN, 0) AS SN, CAST(ISNULL(ey.IsActive, 0) AS bit) AS IsActive,
       ey.EducationYear, COUNT(s.StudentID) AS TotalStudent
FROM dbo.Education_Year ey
LEFT JOIN dbo.StudentsClass sc ON sc.EducationYearID = ey.EducationYearID AND sc.SchoolID = ey.SchoolID
LEFT JOIN dbo.Student s ON s.StudentID = sc.StudentID AND s.Status = N'Active'
WHERE ey.SchoolID = @SchoolID
GROUP BY ey.EducationYearID, ey.SN, ey.IsActive, ey.EducationYear
ORDER BY ey.EducationYearID
""", con))
        {
            years.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await years.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Years.Add(new InstitutionYearRowDto
                {
                    EducationYearID = I(reader["EducationYearID"]),
                    SN = I(reader["SN"]),
                    IsActive = reader["IsActive"] is not DBNull && Convert.ToBoolean(reader["IsActive"]),
                    EducationYear = S(reader["EducationYear"]),
                    TotalStudent = I(reader["TotalStudent"])
                });
            }
        }

        await LoadSmsAsync(con, dto, ct);
        await LoadDueNoticeAsync(con, dto, ct);
        return dto;
    }

    public async Task<AuthorityResult> SaveInstitutionYearsAsync(SessionSnapshot session, SaveInstitutionYearsRequest request, CancellationToken ct)
    {
        if (!session.IsAuthority)
            return new AuthorityResult { Error = "auth.forbidden" };
        if (request.SchoolID <= 0)
            return new AuthorityResult { Error = "auth.noSchool" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            foreach (var year in request.Years)
            {
                await using var cmd = new SqlCommand("""
UPDATE dbo.Education_Year
SET IsActive = @IsActive, SN = @SN
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con, tx);
                cmd.Parameters.AddWithValue("@IsActive", year.IsActive);
                cmd.Parameters.AddWithValue("@SN", year.SN);
                cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                cmd.Parameters.AddWithValue("@EducationYearID", year.EducationYearID);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return new AuthorityResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return new AuthorityResult { Error = ex.Message };
        }
    }

    private static async Task CleanSessionsAsync(SqlConnection con, CancellationToken ct)
    {
        try
        {
            await using var clean = new SqlCommand(
                "DELETE FROM dbo.User_Active_Sessions WHERE LastActivity < DATEADD(MINUTE, -30, GETDATE())", con);
            await clean.ExecuteNonQueryAsync(ct);
        }
        catch
        {
        }
    }

    private static async Task<List<AuthorityYearCountDto>> LoadYearlyAsync(SqlConnection con, int toYear, CancellationToken ct)
    {
        var map = new Dictionary<int, int>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT YEAR(Date) AS Y, COUNT(*) AS C
FROM dbo.SchoolInfo
WHERE Date IS NOT NULL AND YEAR(Date) BETWEEN 1990 AND @To
GROUP BY YEAR(Date)
""", con);
            cmd.Parameters.AddWithValue("@To", toYear);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                map[I(reader["Y"])] = I(reader["C"]);
        }
        catch
        {
        }

        if (map.Count == 0)
            return [];

        var fromYear = map.Keys.Min();
        var list = new List<AuthorityYearCountDto>();
        for (var y = fromYear; y <= toYear; y++)
            list.Add(new AuthorityYearCountDto { Year = y, Count = map.GetValueOrDefault(y) });
        return list;
    }

    private static async Task<List<AuthorityTopPaidDto>> LoadTopPaidAsync(SqlConnection con, DateTime start, DateTime end, CancellationToken ct)
    {
        var list = new List<AuthorityTopPaidDto>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 5 s.SchoolID, s.SchoolName, SUM(pr.Amount) AS Paid
FROM dbo.AAP_Invoice_Payment_Record pr
INNER JOIN dbo.SchoolInfo s ON s.SchoolID = pr.SchoolID
WHERE pr.PaidDate >= @Start AND pr.PaidDate < @End
GROUP BY s.SchoolID, s.SchoolName
ORDER BY SUM(pr.Amount) DESC
""", con);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new AuthorityTopPaidDto
                {
                    SchoolID = I(reader["SchoolID"]),
                    SchoolName = S(reader["SchoolName"]),
                    Paid = M(reader["Paid"])
                });
            }
        }
        catch
        {
        }

        return list;
    }

    private static async Task<List<AuthorityInstitutionRowDto>> LoadRowsAsync(
        SqlConnection con, int top, CancellationToken ct, string? whereSql = null, SqlCommand? paramSource = null)
    {
        var list = new List<AuthorityInstitutionRowDto>();
        var limit = top > 0 ? $"TOP ({top}) " : "";
        var where = whereSql ?? "";
        await using var cmd = new SqlCommand($"""
SELECT {limit}Sch.SchoolID, Sch.SchoolName, Sch.Phone, Sch.Validation, Sch.Date, Sch.UserName,
       ses.LoggedInUser, ses.LoginRole, ses.LoginTime, ses.LastActivity,
       ISNULL(STUFF((
           SELECT N', ' + EducationYear
           FROM dbo.Education_Year ey
           WHERE ey.SchoolID = Sch.SchoolID AND ey.IsActive = 1
           FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N''), N'') AS SessionNames
FROM dbo.SchoolInfo AS Sch
OUTER APPLY (
    SELECT TOP 1 u.UserName AS LoggedInUser, u.Category AS LoginRole, u.LoginTime, u.LastActivity
    FROM dbo.User_Active_Sessions u
    WHERE u.SchoolID = Sch.SchoolID
      AND (u.LastActivity >= DATEADD(HOUR, -1, GETDATE()) OR CAST(u.LoginTime AS DATE) = CAST(GETDATE() AS DATE))
    ORDER BY u.LastActivity DESC
) ses
{where}
ORDER BY {(top > 0 ? "Sch.Date DESC, Sch.SchoolID DESC" : "ses.LastActivity DESC, Sch.Date DESC, Sch.SchoolID")}
""", con);
        if (paramSource is not null)
            CopyParameters(paramSource, cmd);

        var now = DateTime.Now;
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var last = Dt(reader["LastActivity"]);
            var online = "";
            if (last is DateTime lastAt)
            {
                if (lastAt >= now.AddMinutes(-5))
                    online = "now";
                else if (lastAt >= now.AddMinutes(-15))
                    online = "active";
            }

            list.Add(new AuthorityInstitutionRowDto
            {
                SchoolID = I(reader["SchoolID"]),
                SchoolName = S(reader["SchoolName"]),
                UserName = S(reader["UserName"]),
                Phone = S(reader["Phone"]),
                Validation = S(reader["Validation"]),
                OnlineStatus = online,
                LoggedInUser = S(reader["LoggedInUser"]),
                LoginRole = S(reader["LoginRole"]),
                LoginTime = Dt(reader["LoginTime"]),
                LastActivity = last,
                Registered = Dt(reader["Date"]),
                SessionNames = S(reader["SessionNames"])
            });
        }

        return list;
    }

    private static void AppendFilters(
        StringBuilder where, SqlCommand cmd, string? q, string? validation, string? live, DateTime? from, DateTime? to)
    {
        if (!string.IsNullOrWhiteSpace(q))
        {
            And(where);
            where.Append("(Sch.SchoolName LIKE @q OR Sch.UserName LIKE @q OR Sch.Phone LIKE @q OR CAST(Sch.SchoolID AS VARCHAR(20)) LIKE @q)");
            cmd.Parameters.AddWithValue("@q", "%" + q.Trim() + "%");
        }

        if (string.Equals(validation, "Valid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(validation, "Invalid", StringComparison.OrdinalIgnoreCase))
        {
            And(where);
            where.Append("Sch.Validation = @val");
            cmd.Parameters.AddWithValue("@val", validation!.Trim());
        }

        switch (live?.Trim())
        {
            case "LiveNow":
                And(where);
                where.Append("EXISTS (SELECT 1 FROM dbo.User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND u.LastActivity >= DATEADD(MINUTE, -5, GETDATE()))");
                break;
            case "LoggedIn":
                And(where);
                where.Append("EXISTS (SELECT 1 FROM dbo.User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND u.LastActivity >= DATEADD(MINUTE, -15, GETDATE()))");
                break;
            case "LastHour":
                And(where);
                where.Append("EXISTS (SELECT 1 FROM dbo.User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND u.LastActivity >= DATEADD(HOUR, -1, GETDATE()))");
                break;
            case "Today":
                And(where);
                where.Append("EXISTS (SELECT 1 FROM dbo.User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND CAST(u.LoginTime AS DATE) = CAST(GETDATE() AS DATE))");
                break;
        }

        if (from is DateTime start)
        {
            And(where);
            where.Append("Sch.Date >= @from");
            cmd.Parameters.AddWithValue("@from", start.Date);
        }

        if (to is DateTime end)
        {
            And(where);
            where.Append("Sch.Date < @to");
            cmd.Parameters.AddWithValue("@to", end.Date.AddDays(1));
        }
    }

    private static void CopyParameters(SqlCommand source, SqlCommand target)
    {
        foreach (SqlParameter p in source.Parameters)
            target.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
    }

    private static void And(StringBuilder where)
    {
        if (where.Length > 0)
            where.Append(" AND ");
    }

    private static async Task<int> ScalarIntAsync(SqlConnection con, string sql, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            var value = await cmd.ExecuteScalarAsync(ct);
            return value is null or DBNull ? 0 : Convert.ToInt32(value);
        }
        catch
        {
            return 0;
        }
    }

    private static async Task<decimal> ScalarDecAsync(
        SqlConnection con, string sql, DateTime? start, DateTime? end, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            if (start is not null)
                cmd.Parameters.AddWithValue("@Start", start.Value);
            if (end is not null)
                cmd.Parameters.AddWithValue("@End", end.Value);
            var value = await cmd.ExecuteScalarAsync(ct);
            return value is null or DBNull ? 0 : Convert.ToDecimal(value);
        }
        catch
        {
            return 0;
        }
    }

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static decimal M(object? value) => value is null or DBNull ? 0 : Convert.ToDecimal(value);
    private static DateTime? Dt(object? value) =>
        value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);
}
