using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.SyncApi.Services;

public sealed class DashboardService
{
    private readonly EduConnectionFactory _connections;

    public DashboardService(EduConnectionFactory connections) => _connections = connections;

    public async Task<DashboardOverviewDto> GetOverviewAsync(SessionSnapshot session, CancellationToken ct)
    {
        var schoolId = session.SchoolID;
        var yearId = session.EducationYearID;
        var smsBalanceTask = ScalarIntAsync(
            "SELECT TOP 1 SMS_Balance FROM SMS WHERE SchoolID = @SchoolID",
            schoolId, ct);
        var smsTask = QuerySmsByYearAsync(schoolId, ct);
        var empTask = QueryCountsAsync("""
SELECT EmployeeType AS Name, COUNT(EmployeeID) AS Total
FROM Employee_Info
WHERE SchoolID = @SchoolID AND Job_Status = N'Active'
GROUP BY EmployeeType
""", schoolId, ct);
        var attTask = QueryCountsAsync("""
SELECT Attendance AS Name, COUNT(*) AS Total
FROM Attendance_Record
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
  AND CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY Attendance
ORDER BY Attendance DESC
""", schoolId, ct, yearId);
        var sessTask = QueryCountsAsync("""
SELECT Education_Year.EducationYear AS Name, COUNT(*) AS Total
FROM StudentsClass
INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.SchoolID = @SchoolID AND Student.Status = N'Active'
GROUP BY Education_Year.SN, Education_Year.EducationYear, Education_Year.EducationYearID
ORDER BY Education_Year.SN
""", schoolId, ct);
        var bdaySmsTask = ScalarIntAsync("""
SELECT TOP 1 1
FROM SMS_Send_Record
INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID
WHERE SMS_OtherInfo.SchoolID = @SchoolID
  AND SMS_Send_Record.PurposeOfSMS = N'Birthday'
  AND SMS_Send_Record.Date >= CAST(CAST(GETDATE() AS date) AS datetime)
  AND SMS_Send_Record.Date < DATEADD(day, 1, CAST(CAST(GETDATE() AS date) AS datetime))
""", schoolId, ct);

        await Task.WhenAll(smsBalanceTask, smsTask, empTask, attTask, sessTask, bdaySmsTask);

        var dto = new DashboardOverviewDto
        {
            SmsRemaining = smsBalanceTask.Result,
            SmsByYear = smsTask.Result,
            Employees = empTask.Result,
            AttendanceToday = attTask.Result,
            Sessions = sessTask.Result,
            BirthdaySmsSent = bdaySmsTask.Result > 0
        };
        dto.SmsSent = dto.SmsByYear.Sum(x => x.Count);
        dto.EmployeeCount = dto.Employees.Sum(x => x.Count);
        return dto;
    }

    private async Task<List<DashboardNamedCountDto>> QuerySmsByYearAsync(int schoolId, CancellationToken ct)
    {
        var bySession = await QueryCountsAsync("""
SELECT COALESCE(NULLIF(LTRIM(RTRIM(ey.EducationYear)), N''), N'—') AS Name,
       SUM(ISNULL(r.SMSCount, 0)) AS Total
FROM dbo.SMS_OtherInfo AS o
INNER JOIN dbo.SMS_Send_Record AS r ON r.SMS_Send_ID = o.SMS_Send_ID
LEFT JOIN dbo.Education_Year AS ey ON ey.EducationYearID = o.EducationYearID
WHERE o.SchoolID = @SchoolID
GROUP BY ey.EducationYearID, ey.EducationYear
ORDER BY ey.EducationYearID
""", schoolId, ct, timeoutSeconds: 60);
        if (bySession.Count > 0)
            return bySession;

        return await QueryCountsAsync("""
SELECT CAST(YEAR(r.Date) AS nvarchar(12)) AS Name,
       SUM(ISNULL(r.SMSCount, 0)) AS Total
FROM dbo.SMS_Send_Record AS r
INNER JOIN dbo.SMS_OtherInfo AS o ON o.SMS_Send_ID = r.SMS_Send_ID
WHERE o.SchoolID = @SchoolID
GROUP BY YEAR(r.Date)
ORDER BY YEAR(r.Date)
""", schoolId, ct, timeoutSeconds: 60);
    }

    private async Task<int> ScalarIntAsync(string sql, int schoolId, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            return ToInt(await cmd.ExecuteScalarAsync(ct));
        }
        catch (SqlException)
        {
            return 0;
        }
    }

    private async Task<List<DashboardNamedCountDto>> QueryCountsAsync(
        string sql, int schoolId, CancellationToken ct, int? yearId = null, int timeoutSeconds = 20)
    {
        var items = new List<DashboardNamedCountDto>();
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, con) { CommandTimeout = timeoutSeconds };
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            if (yearId is int year)
                cmd.Parameters.AddWithValue("@EducationYearID", year);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var name = Text(reader["Name"]);
                if (string.IsNullOrWhiteSpace(name)) continue;
                items.Add(new DashboardNamedCountDto { Name = name, Count = ToInt(reader["Total"]) });
            }
        }
        catch (SqlException)
        {
        }
        return items;
    }

    private static int ToInt(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static string Text(object? value) => value is null or DBNull ? "" : Convert.ToString(value)?.Trim() ?? "";
}
