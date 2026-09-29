using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AttendanceService
{
    public async Task<AttendanceDisplayDto> GetDisplayAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        // Every open display polls every 20s; share one result per school/year for a few seconds
        // so N screens (or N browser tabs) of the same school cost one set of queries.
        var cacheKey = (session.SchoolID, session.EducationYearID);
        if (DisplayCache.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow - cached.At < DisplayCacheTtl)
            return cached.Dto;

        // The six queries are independent (each opens its own pooled connection): run them in parallel.
        var studentSchedules = ListStudentScheduleSummariesAsync(session, cancellationToken);
        var employeeSchedules = ListEmployeeScheduleSummariesAsync(session, cancellationToken);
        var studentIn = ListStudentDisplayCardsAsync(session, isOut: false, cancellationToken);
        var studentOut = ListStudentDisplayCardsAsync(session, isOut: true, cancellationToken);
        var employeeIn = ListEmployeeDisplayCardsAsync(session, isOut: false, cancellationToken);
        var employeeOut = ListEmployeeDisplayCardsAsync(session, isOut: true, cancellationToken);
        await Task.WhenAll(studentSchedules, employeeSchedules, studentIn, studentOut, employeeIn, employeeOut);

        var result = new AttendanceDisplayDto
        {
            StudentSchedules = (await studentSchedules).ToList(),
            EmployeeSchedules = (await employeeSchedules).ToList(),
            StudentIn = (await studentIn).ToList(),
            StudentOut = (await studentOut).ToList(),
            EmployeeIn = (await employeeIn).ToList(),
            EmployeeOut = (await employeeOut).ToList()
        };
        DisplayCache[cacheKey] = (DateTime.UtcNow, result);
        if (DisplayCache.Count > 500)
        {
            foreach (var kv in DisplayCache)
                if (DateTime.UtcNow - kv.Value.At > DisplayCacheTtl)
                    DisplayCache.TryRemove(kv.Key, out _);
        }
        return result;
    }

    private static readonly TimeSpan DisplayCacheTtl = TimeSpan.FromSeconds(10);
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(int SchoolID, int EducationYearID), (DateTime At, AttendanceDisplayDto Dto)> DisplayCache = new();

    private async Task<IReadOnlyList<AttendanceScheduleSummaryDto>> ListStudentScheduleSummariesAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT sch.ScheduleID,
       sch.ScheduleName,
       LTRIM(RIGHT(CONVERT(varchar(20), sd.StartTime, 100), 7)) + N' - ' + LTRIM(RIGHT(CONVERT(varchar(20), sd.EndTime, 100), 7)) AS TimeRange,
       ISNULL(u.Total_User, 0) AS Total_User,
       ISNULL(r.Current_IN, 0) AS Current_IN,
       ISNULL(r.Total_Out, 0) AS Total_Out,
       ISNULL(r.Total_Present, 0) AS Total_Present,
       ISNULL(r.Total_Late, 0) AS Total_Late,
       ISNULL(r.Total_Late_Absent, 0) AS Total_Late_Absent,
       ISNULL(r.Total_Absent, 0) AS Total_Absent,
       ISNULL(r.Total_Leave_Recorded, 0) + ISNULL(u.Virtual_Leave, 0) AS Total_Leave
FROM dbo.Attendance_Schedule sch
INNER JOIN dbo.Attendance_Schedule_Day sd ON sch.ScheduleID = sd.ScheduleID AND sd.SchoolID = sch.SchoolID
    AND sd.Day = DATENAME(dw, GETDATE()) AND sd.Is_OnDay = 1
LEFT JOIN (
    SELECT st.ScheduleID,
           COUNT(DISTINCT st.StudentID) AS Total_User,
           COUNT(DISTINCT CASE WHEN st.HasRow = 0 AND st.OnLeave = 1 THEN st.StudentID END) AS Virtual_Leave
    FROM (
        SELECT ass.ScheduleID, ass.StudentID,
               CASE WHEN EXISTS (SELECT 1 FROM dbo.Attendance_Record ar2
                                 WHERE ar2.SchoolID = @SchoolID AND ar2.StudentID = ass.StudentID
                                   AND ar2.AttendanceDate = CONVERT(date, GETDATE())
                                   AND ar2.Attendance_ScheduleID = ass.ScheduleID) THEN 1 ELSE 0 END AS HasRow,
               CASE WHEN EXISTS (SELECT 1 FROM dbo.Attendance_Leave al
                                 WHERE al.SchoolID = @SchoolID AND al.StudentID = ass.StudentID
                                   AND CONVERT(date, GETDATE()) BETWEEN al.StartDate AND al.EndDate) THEN 1 ELSE 0 END AS OnLeave
        FROM dbo.Attendance_Schedule_AssignStudent ass
        INNER JOIN dbo.Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
        INNER JOIN dbo.StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ass.SchoolID = @SchoolID
    ) st
    GROUP BY st.ScheduleID
) u ON u.ScheduleID = sch.ScheduleID
LEFT JOIN (
    SELECT ar.Attendance_ScheduleID AS ScheduleID,
           SUM(CASE WHEN ISNULL(ar.Is_OUT, 0) = 0 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs') AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Current_IN,
           SUM(CASE WHEN ar.ExitTime IS NOT NULL AND ar.Is_OUT = 1 THEN 1 ELSE 0 END) AS Total_Out,
           SUM(CASE WHEN ar.Attendance = N'Pre' AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Total_Present,
           SUM(CASE WHEN ar.Attendance = N'Late' THEN 1 ELSE 0 END) AS Total_Late,
           SUM(CASE WHEN ar.Attendance = N'Late Abs' THEN 1 ELSE 0 END) AS Total_Late_Absent,
           SUM(CASE WHEN ar.Attendance = N'Abs' OR (ar.Attendance = N'Pre' AND ISNULL(ar.RegistrationID, 0) = 0 AND (ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time))) THEN 1 ELSE 0 END) AS Total_Absent,
           SUM(CASE WHEN ar.Attendance = N'Leave' THEN 1 ELSE 0 END) AS Total_Leave_Recorded
    FROM dbo.Attendance_Record ar
    INNER JOIN dbo.Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
    WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())
      AND ar.Attendance_ScheduleID IS NOT NULL
    GROUP BY ar.Attendance_ScheduleID
) r ON r.ScheduleID = sch.ScheduleID
WHERE sch.SchoolID = @SchoolID
  AND EXISTS (
      SELECT 1 FROM dbo.Attendance_Schedule_AssignStudent ass
      INNER JOIN dbo.Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
      WHERE ass.SchoolID = @SchoolID AND ass.ScheduleID = sch.ScheduleID)
ORDER BY sd.StartTime, sch.ScheduleName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        return await ReadSummariesAsync(cmd, cancellationToken);
    }

    private async Task<IReadOnlyList<AttendanceScheduleSummaryDto>> ListEmployeeScheduleSummariesAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT sch.ScheduleID,
       sch.ScheduleName,
       LTRIM(RIGHT(CONVERT(varchar(20), sd.StartTime, 100), 7)) + N' - ' + LTRIM(RIGHT(CONVERT(varchar(20), sd.EndTime, 100), 7)) AS TimeRange,
       u.Total_User,
       ISNULL(r.Current_IN, 0) AS Current_IN,
       ISNULL(r.Total_Out, 0) AS Total_Out,
       ISNULL(r.Total_Present, 0) AS Total_Present,
       ISNULL(r.Total_Late, 0) AS Total_Late,
       ISNULL(r.Total_Late_Absent, 0) AS Total_Late_Absent,
       ISNULL(r.Total_Absent, 0) AS Total_Absent,
       ISNULL(r.Total_Leave_Recorded, 0) + u.Virtual_Leave AS Total_Leave
FROM dbo.Attendance_Schedule sch
INNER JOIN dbo.Attendance_Schedule_Day sd ON sch.ScheduleID = sd.ScheduleID AND sd.SchoolID = sch.SchoolID
    AND sd.Day = DATENAME(dw, GETDATE()) AND sd.Is_OnDay = 1
INNER JOIN (
    SELECT et.ScheduleID,
           COUNT(DISTINCT et.EmployeeID) AS Total_User,
           COUNT(DISTINCT CASE WHEN et.HasRow = 0 AND et.OnLeave = 1 THEN et.EmployeeID END) AS Virtual_Leave
    FROM (
        SELECT ass.ScheduleID, ass.EmployeeID,
               CASE WHEN EXISTS (SELECT 1 FROM dbo.Employee_Attendance_Record ear2
                                 WHERE ear2.SchoolID = @SchoolID AND ear2.EmployeeID = ass.EmployeeID
                                   AND ear2.AttendanceDate = CONVERT(date, GETDATE())
                                   AND ear2.Attendance_ScheduleID = ass.ScheduleID) THEN 1 ELSE 0 END AS HasRow,
               CASE WHEN EXISTS (SELECT 1 FROM dbo.Employee_Leave el
                                 WHERE el.SchoolID = @SchoolID AND el.EmployeeID = ass.EmployeeID
                                   AND CONVERT(date, GETDATE()) BETWEEN el.LeaveStartDate AND el.LeaveEndDate) THEN 1 ELSE 0 END AS OnLeave
        FROM dbo.Employee_Attendance_Schedule_Assign ass
        INNER JOIN dbo.VW_Emp_Info e ON ass.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
        WHERE ass.SchoolID = @SchoolID
    ) et
    GROUP BY et.ScheduleID
) u ON u.ScheduleID = sch.ScheduleID
LEFT JOIN (
    SELECT ear.Attendance_ScheduleID AS ScheduleID,
           SUM(CASE WHEN ISNULL(ear.Is_OUT, 0) = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs') AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Current_IN,
           SUM(CASE WHEN ear.ExitTime IS NOT NULL AND ear.ExitTime <> CAST('00:00:00' AS time) AND ISNULL(ear.Is_OUT, 0) = 1 THEN 1 ELSE 0 END) AS Total_Out,
           SUM(CASE WHEN ear.AttendanceStatus = N'Pre' AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Total_Present,
           SUM(CASE WHEN ear.AttendanceStatus = N'Late' THEN 1 ELSE 0 END) AS Total_Late,
           SUM(CASE WHEN ear.AttendanceStatus = N'Late Abs' THEN 1 ELSE 0 END) AS Total_Late_Absent,
           SUM(CASE WHEN ear.AttendanceStatus = N'Abs' OR (ear.AttendanceStatus = N'Pre' AND ISNULL(ear.RegistrationID, 0) = 0 AND (ear.EntryTime IS NULL OR ear.EntryTime = CAST('00:00:00' AS time))) THEN 1 ELSE 0 END) AS Total_Absent,
           SUM(CASE WHEN ear.AttendanceStatus = N'Leave' THEN 1 ELSE 0 END) AS Total_Leave_Recorded
    FROM dbo.Employee_Attendance_Record ear
    INNER JOIN dbo.VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
    WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())
      AND ear.Attendance_ScheduleID IS NOT NULL
    GROUP BY ear.Attendance_ScheduleID
) r ON r.ScheduleID = sch.ScheduleID
WHERE sch.SchoolID = @SchoolID
ORDER BY sd.StartTime, sch.ScheduleName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return await ReadSummariesAsync(cmd, cancellationToken);
    }

    private static async Task<IReadOnlyList<AttendanceScheduleSummaryDto>> ReadSummariesAsync(
        SqlCommand cmd, CancellationToken cancellationToken)
    {
        var items = new List<AttendanceScheduleSummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AttendanceScheduleSummaryDto
            {
                ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                ScheduleName = reader["ScheduleName"]?.ToString() ?? "",
                TimeRange = reader["TimeRange"]?.ToString() ?? "",
                TotalUser = Convert.ToInt32(reader["Total_User"]),
                CurrentIn = Convert.ToInt32(reader["Current_IN"]),
                TotalOut = Convert.ToInt32(reader["Total_Out"]),
                TotalPresent = Convert.ToInt32(reader["Total_Present"]),
                TotalLate = Convert.ToInt32(reader["Total_Late"]),
                TotalLateAbsent = Convert.ToInt32(reader["Total_Late_Absent"]),
                TotalAbsent = Convert.ToInt32(reader["Total_Absent"]),
                TotalLeave = Convert.ToInt32(reader["Total_Leave"])
            });
        }
        return items;
    }

    private async Task<IReadOnlyList<AttendanceDisplayCardDto>> ListStudentDisplayCardsAsync(
        SessionSnapshot session, bool isOut, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var sql = isOut ? """
SELECT ar.StudentID AS PersonID, s.StudentsName AS Name,
       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
       ar.Attendance, CONVERT(varchar(15), ar.EntryTime, 100) AS EntryTime, CONVERT(varchar(15), ar.ExitTime, 100) AS ExitTime,
       ISNULL(cc.Class, N'') AS ClassOrDesignation
FROM dbo.Attendance_Record ar
INNER JOIN dbo.Student s ON ar.StudentID = s.StudentID AND s.Status = N'Active'
INNER JOIN dbo.StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
LEFT JOIN dbo.CreateClass cc ON scEy.ClassID = cc.ClassID
LEFT JOIN dbo.Attendance_Schedule sch ON ar.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ar.SchoolID
WHERE ar.AttendanceDate = CONVERT(date, GETDATE())
  AND ar.ExitTime IS NOT NULL AND ar.ExitTime <> CAST('00:00:00' AS time) AND ar.Is_OUT = 1
  AND ar.SchoolID = @SchoolID
ORDER BY ar.ExitTime DESC
""" : """
SELECT ar.StudentID AS PersonID, s.StudentsName AS Name,
       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
       ar.Attendance, CONVERT(varchar(15), ar.EntryTime, 100) AS EntryTime, CONVERT(varchar(15), ar.ExitTime, 100) AS ExitTime,
       ISNULL(cc.Class, N'') AS ClassOrDesignation
FROM dbo.Attendance_Record ar
INNER JOIN dbo.Student s ON ar.StudentID = s.StudentID AND s.Status = N'Active'
INNER JOIN dbo.StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
LEFT JOIN dbo.CreateClass cc ON scEy.ClassID = cc.ClassID
LEFT JOIN dbo.Attendance_Schedule sch ON ar.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ar.SchoolID
WHERE ar.AttendanceDate = CONVERT(date, GETDATE())
  AND ISNULL(ar.Is_OUT, 0) = 0 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs')
  AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0)
  AND ar.SchoolID = @SchoolID
ORDER BY CASE WHEN ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time) THEN 1 ELSE 0 END, ar.EntryTime DESC
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        return await ReadCardsAsync(cmd, isEmployee: false, cancellationToken);
    }

    private async Task<IReadOnlyList<AttendanceDisplayCardDto>> ListEmployeeDisplayCardsAsync(
        SessionSnapshot session, bool isOut, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var sql = isOut ? """
SELECT ear.EmployeeID AS PersonID, LTRIM(RTRIM(ISNULL(e.FirstName, N'') + N' ' + ISNULL(e.LastName, N''))) AS Name, ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
       ear.AttendanceStatus AS Attendance, CONVERT(varchar(15), ear.EntryTime, 100) AS EntryTime, CONVERT(varchar(15), ear.ExitTime, 100) AS ExitTime,
       ISNULL(e.Designation, N'') AS ClassOrDesignation
FROM dbo.Employee_Attendance_Record ear
INNER JOIN dbo.VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
LEFT JOIN dbo.Attendance_Schedule sch ON ear.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ear.SchoolID
WHERE ear.AttendanceDate = CONVERT(date, GETDATE())
  AND ear.ExitTime IS NOT NULL AND ear.ExitTime <> CAST('00:00:00' AS time) AND ISNULL(ear.Is_OUT, 0) = 1
  AND ear.SchoolID = @SchoolID
ORDER BY ear.ExitTime DESC
""" : """
SELECT ear.EmployeeID AS PersonID, LTRIM(RTRIM(ISNULL(e.FirstName, N'') + N' ' + ISNULL(e.LastName, N''))) AS Name, ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
       ear.AttendanceStatus AS Attendance, CONVERT(varchar(15), ear.EntryTime, 100) AS EntryTime, CONVERT(varchar(15), ear.ExitTime, 100) AS ExitTime,
       ISNULL(e.Designation, N'') AS ClassOrDesignation
FROM dbo.Employee_Attendance_Record ear
INNER JOIN dbo.VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
LEFT JOIN dbo.Attendance_Schedule sch ON ear.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ear.SchoolID
WHERE ear.AttendanceDate = CONVERT(date, GETDATE())
  AND ISNULL(ear.Is_OUT, 0) = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs')
  AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0)
  AND ear.SchoolID = @SchoolID
ORDER BY CASE WHEN ear.EntryTime IS NULL OR ear.EntryTime = CAST('00:00:00' AS time) THEN 1 ELSE 0 END, ear.EntryTime DESC
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return await ReadCardsAsync(cmd, isEmployee: true, cancellationToken);
    }

    private static async Task<IReadOnlyList<AttendanceDisplayCardDto>> ReadCardsAsync(
        SqlCommand cmd, bool isEmployee, CancellationToken cancellationToken)
    {
        var items = new List<AttendanceDisplayCardDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AttendanceDisplayCardDto
            {
                PersonID = Convert.ToInt32(reader["PersonID"]),
                Name = reader["Name"]?.ToString() ?? "",
                ScheduleName = reader["ScheduleName"]?.ToString(),
                Attendance = reader["Attendance"]?.ToString() ?? "",
                EntryTime = reader["EntryTime"]?.ToString()?.Trim(),
                ExitTime = reader["ExitTime"]?.ToString()?.Trim(),
                ClassOrDesignation = reader["ClassOrDesignation"]?.ToString(),
                IsEmployee = isEmployee
            });
        }
        return items;
    }
}