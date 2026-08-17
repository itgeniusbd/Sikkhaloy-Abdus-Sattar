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
        var dto = new DashboardOverviewDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await TryAsync(async () =>
        {
            await using var cmd = new SqlCommand("SELECT TOP 1 SMS_Balance FROM SMS WHERE SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var value = await cmd.ExecuteScalarAsync(ct);
            dto.SmsRemaining = ToInt(value);
        });

        dto.SmsByYear = await QueryCountsAsync(con, """
SELECT Education_Year.EducationYear AS Name, ISNULL(SUM(SMS_Send_Record.SMSCount), 0) AS Total
FROM SMS_Send_Record
INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID
INNER JOIN Education_Year ON SMS_OtherInfo.EducationYearID = Education_Year.EducationYearID
WHERE SMS_OtherInfo.SchoolID = @SchoolID
GROUP BY Education_Year.EducationYear, Education_Year.EducationYearID
ORDER BY Education_Year.EducationYearID
""", session.SchoolID, ct);
        dto.SmsSent = dto.SmsByYear.Sum(x => x.Count);

        dto.Employees = await QueryCountsAsync(con, """
SELECT EmployeeType AS Name, COUNT(EmployeeID) AS Total
FROM Employee_Info
WHERE SchoolID = @SchoolID AND Job_Status = N'Active'
GROUP BY EmployeeType
""", session.SchoolID, ct);
        dto.EmployeeCount = dto.Employees.Sum(x => x.Count);

        dto.AttendanceToday = await QueryCountsAsync(con, """
SELECT Attendance AS Name, COUNT(*) AS Total
FROM Attendance_Record
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
  AND CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY Attendance
ORDER BY Attendance DESC
""", session.SchoolID, ct, session.EducationYearID);

        dto.Sessions = await QueryCountsAsync(con, """
SELECT Education_Year.EducationYear AS Name, COUNT(StudentsClass.StudentClassID) AS Total
FROM Student
INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID
INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
WHERE Student.Status = N'Active' AND StudentsClass.SchoolID = @SchoolID
GROUP BY Education_Year.SN, Education_Year.EducationYear, Education_Year.EducationYearID
ORDER BY Education_Year.SN
""", session.SchoolID, ct);

        return dto;
    }

    private static async Task<List<DashboardNamedCountDto>> QueryCountsAsync(
        SqlConnection con, string sql, int schoolId, CancellationToken ct, int? yearId = null)
    {
        var items = new List<DashboardNamedCountDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
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

    private static async Task TryAsync(Func<Task> work)
    {
        try { await work(); }
        catch (SqlException) { }
    }

    private static int ToInt(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static string Text(object? value) => value is null or DBNull ? "" : Convert.ToString(value)?.Trim() ?? "";
}
