/*
    School 1012 — why student download/sync fails.
    Run on Edu (LOOPS-IT-VM-1).
*/

USE [Edu];
GO

DECLARE @SchoolID INT = 1012;

-- 1) Students in display view
SELECT COUNT(*) AS Students_In_VW_Attendance_Users
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID AND Is_Student = 1;

SELECT COUNT(*) AS Students_With_DeviceID
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID AND Is_Student = 1 AND DeviceID > 0;

-- 2) Schedule assign (required for punch + User_Schedule sync)
SELECT COUNT(*) AS Students_Assigned_To_Schedule
FROM dbo.Student s
INNER JOIN dbo.Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
WHERE s.SchoolID = @SchoolID AND s.Status = 'Active' AND ass.SchoolID = @SchoolID;

SELECT TOP 15 s.DeviceID, s.ID, s.StudentsName, ass.ScheduleID
FROM dbo.Student s
INNER JOIN dbo.Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
WHERE s.SchoolID = @SchoolID AND s.Status = 'Active' AND s.DeviceID > 0
ORDER BY s.DeviceID;

-- 3) Students missing DeviceID (cannot sync from device)
SELECT COUNT(*) AS Active_Students_No_DeviceID
FROM dbo.Student
WHERE SchoolID = @SchoolID AND Status = 'Active' AND (DeviceID IS NULL OR DeviceID = 0);

-- 4) Simulate latest api/Users download query
SELECT COUNT(*) AS ApiUsers_Would_Return
FROM dbo.VW_Attendance_Users u
OUTER APPLY (
    SELECT TOP 1 ass.ScheduleID
    FROM dbo.Student s
    INNER JOIN dbo.Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
    WHERE s.SchoolID = @SchoolID AND s.DeviceID = u.DeviceID AND s.Status = 'Active' AND ass.SchoolID = @SchoolID
    ORDER BY ass.ScheduleID
) stuAss
OUTER APPLY (
    SELECT TOP 1 eas.ScheduleID
    FROM dbo.Employee_Info e
    INNER JOIN dbo.Employee_Attendance_Schedule_Assign eas ON e.EmployeeID = eas.EmployeeID
    WHERE e.SchoolID = @SchoolID AND e.DeviceID = u.DeviceID AND e.Job_Status = 'Active' AND eas.SchoolID = @SchoolID
    ORDER BY eas.ScheduleID
) empAss
WHERE u.SchoolID = @SchoolID
  AND u.DeviceID > 0
  AND COALESCE(stuAss.ScheduleID, empAss.ScheduleID, NULLIF(u.ScheduleID, 0)) IS NOT NULL;

SELECT TOP 15
    u.DeviceID,
    u.Name,
    u.Is_Student,
    COALESCE(NULLIF(u.ScheduleID, 0), stuAss.ScheduleID, empAss.ScheduleID) AS ResolvedScheduleID
FROM dbo.VW_Attendance_Users u
OUTER APPLY (
    SELECT TOP 1 ass.ScheduleID
    FROM dbo.Student s
    INNER JOIN dbo.Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
    WHERE s.SchoolID = @SchoolID AND s.DeviceID = u.DeviceID AND s.Status = 'Active' AND ass.SchoolID = @SchoolID
    ORDER BY ass.ScheduleID
) stuAss
OUTER APPLY (
    SELECT TOP 1 eas.ScheduleID
    FROM dbo.Employee_Info e
    INNER JOIN dbo.Employee_Attendance_Schedule_Assign eas ON e.EmployeeID = eas.EmployeeID
    WHERE e.SchoolID = @SchoolID AND e.DeviceID = u.DeviceID AND e.Job_Status = 'Active' AND eas.SchoolID = @SchoolID
    ORDER BY eas.ScheduleID
) empAss
WHERE u.SchoolID = @SchoolID
  AND u.DeviceID > 0
  AND ISNULL(u.Is_Student, 0) = 1
ORDER BY u.DeviceID;

-- 5) Student sync view (API POST join)
SELECT COUNT(*) AS Students_In_VW_Attendance_Stu
FROM dbo.VW_Attendance_Stu
WHERE SchoolID = @SchoolID;

-- 6) Today student attendance on server
SELECT COUNT(*) AS Today_Student_Records
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = CAST(GETDATE() AS DATE);
