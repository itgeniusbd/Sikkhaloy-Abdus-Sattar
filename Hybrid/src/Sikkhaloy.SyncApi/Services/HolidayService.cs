using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Calendar;

namespace Sikkhaloy.SyncApi.Services;

public sealed class HolidayService
{
    private static readonly HashSet<string> WeekDays = new(StringComparer.OrdinalIgnoreCase)
    {
        "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"
    };

    private readonly EduConnectionFactory _connections;

    public HolidayService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<HolidayDto>> ListAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT HolidayID, HolidayName, HolidayDate
FROM dbo.Employee_Holiday
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
ORDER BY HolidayDate, HolidayID
""";

        var items = new List<HolidayDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader["HolidayName"]?.ToString() ?? "";
            items.Add(new HolidayDto
            {
                HolidayID = Convert.ToInt32(reader["HolidayID"]),
                HolidayName = name,
                HolidayDate = Convert.ToDateTime(reader["HolidayDate"]).Date,
                IsWeekly = string.Equals(name, "Weekly Holiday", StringComparison.OrdinalIgnoreCase)
            });
        }

        return items;
    }

    public async Task<HolidayResult> AddWeeklyAsync(
        SessionSnapshot session, WeeklyHolidayRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
            return Fail("cal.needRange");
        if (request.EndDate.Date < request.StartDate.Date)
            return Fail("cal.needRange");

        var days = (request.Days ?? [])
            .Select(x => x.Trim())
            .Where(x => WeekDays.Contains(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (days.Count == 0)
            return Fail("cal.needDay");

        var added = 0;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        for (var day = request.StartDate.Date; day <= request.EndDate.Date; day = day.AddDays(1))
        {
            if (!days.Contains(day.DayOfWeek.ToString()))
                continue;
            if (await InsertIfMissingAsync(con, session, "Weekly Holiday", day, cancellationToken))
                added++;
        }

        return new HolidayResult { Succeeded = true, Added = added };
    }

    public async Task<HolidayResult> AddRangeAsync(
        SessionSnapshot session, RangeHolidayRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.HolidayName ?? "").Trim();
        if (name.Length == 0)
            return Fail("cal.needName");
        if (request is null || request.EndDate.Date < request.StartDate.Date)
            return Fail("cal.needRange");

        var added = 0;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        for (var day = request.StartDate.Date; day <= request.EndDate.Date; day = day.AddDays(1))
        {
            if (await InsertIfMissingAsync(con, session, name, day, cancellationToken))
                added++;
        }

        return new HolidayResult { Succeeded = true, Added = added };
    }

    public async Task<HolidayResult> AddOneAsync(
        SessionSnapshot session, SaveHolidayRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.HolidayName ?? "").Trim();
        if (name.Length == 0)
            return Fail("cal.needName");
        if (request is null || request.HolidayDate == default)
            return Fail("cal.needDate");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var added = await InsertIfMissingAsync(con, session, name, request.HolidayDate.Date, cancellationToken) ? 1 : 0;
        return new HolidayResult { Succeeded = true, Added = added };
    }

    public async Task<HolidayResult> UpdateAsync(
        SessionSnapshot session, int holidayId, SaveHolidayRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.HolidayName ?? "").Trim();
        if (holidayId <= 0)
            return Fail("cal.needHoliday");
        if (name.Length == 0)
            return Fail("cal.needName");
        if (request is null || request.HolidayDate == default)
            return Fail("cal.needDate");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Employee_Holiday
SET HolidayName = @HolidayName, HolidayDate = @HolidayDate
WHERE HolidayID = @HolidayID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
        cmd.Parameters.AddWithValue("@HolidayName", name);
        cmd.Parameters.AddWithValue("@HolidayDate", request.HolidayDate.Date);
        cmd.Parameters.AddWithValue("@HolidayID", holidayId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0
            ? new HolidayResult { Succeeded = true }
            : Fail("cal.needHoliday");
    }

    public async Task<HolidayResult> DeleteAsync(SessionSnapshot session, int holidayId, CancellationToken cancellationToken)
    {
        if (holidayId <= 0)
            return Fail("cal.needHoliday");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Employee_Holiday
WHERE HolidayID = @HolidayID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
        cmd.Parameters.AddWithValue("@HolidayID", holidayId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0
            ? new HolidayResult { Succeeded = true }
            : Fail("cal.needHoliday");
    }

    private static async Task<bool> InsertIfMissingAsync(
        SqlConnection con, SessionSnapshot session, string name, DateTime date, CancellationToken cancellationToken)
    {
        await using var exists = new SqlCommand("""
SELECT 1 FROM dbo.Employee_Holiday
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND HolidayDate = @HolidayDate
""", con);
        exists.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        exists.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        exists.Parameters.AddWithValue("@HolidayDate", date);
        if (await exists.ExecuteScalarAsync(cancellationToken) is not null and not DBNull)
            return false;

        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Employee_Holiday (SchoolID, RegistrationID, EducationYearID, HolidayName, HolidayDate)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @HolidayName, @HolidayDate)
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@HolidayName", name);
        cmd.Parameters.AddWithValue("@HolidayDate", date);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    private static HolidayResult Fail(string error) => new() { Succeeded = false, Error = error };
}
