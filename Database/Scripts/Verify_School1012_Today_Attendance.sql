/*
    School 1012 — verify today's attendance on SERVER after device sync.
    Run on Edu SQL Server. Compare with PC local SQLite.
*/

DECLARE @SchoolID INT = 1012;
DECLARE @Today    DATE = CAST(GETDATE() AS DATE);

PRINT 'Server today = ' + CONVERT(varchar(10), @Today, 120);

SELECT 'Student Attendance_Record' AS [Source], COUNT(*) AS Cnt
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
UNION ALL
SELECT 'Employee_Attendance_Record', COUNT(*)
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
UNION ALL
SELECT 'Employee WITH EntryTime (Current In)', COUNT(*)
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
  AND EntryTime IS NOT NULL AND Is_OUT = 0;

-- Detail: employees today
SELECT
    ear.Employee_Attendance_RecordID,
    e.DeviceID,
    e.Name,
    ear.Attendance_ScheduleID,
    ear.AttendanceStatus,
    ear.EntryTime,
    ear.ExitTime,
    ear.Is_OUT,
    ear.AttendanceDate
FROM dbo.Employee_Attendance_Record ear
INNER JOIN dbo.Employee_Info e ON ear.EmployeeID = e.EmployeeID
WHERE ear.SchoolID = @SchoolID
  AND ear.AttendanceDate = @Today
ORDER BY ear.EntryTime DESC, e.Name;

-- Students today (if any)
SELECT
    ar.AttendanceRecordID,
    s.DeviceID,
    s.StudentsName,
    ar.Attendance_ScheduleID,
    ar.Attendance,
    ar.EntryTime,
    ar.Is_OUT
FROM dbo.Attendance_Record ar
INNER JOIN dbo.VW_Attendance_Stus s ON ar.StudentID = s.StudentID AND ar.SchoolID = s.SchoolID
WHERE ar.SchoolID = @SchoolID
  AND ar.AttendanceDate = @Today
ORDER BY ar.EntryTime DESC;
