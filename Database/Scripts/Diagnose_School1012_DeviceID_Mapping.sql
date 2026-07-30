/*
    School 1012 — why student/employee DeviceID sync fails.
    Run on Edu (LOOPS-IT-VM-1).
*/

DECLARE @SchoolID INT = 1012;

-- 1) Students in view but ScheduleID NULL (download was skipping these)
SELECT COUNT(*) AS Students_In_View
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID AND Is_Student = 1;

SELECT COUNT(*) AS Students_With_ViewSchedule
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID AND Is_Student = 1 AND ScheduleID IS NOT NULL;

SELECT COUNT(*) AS Students_With_AssignSchedule
FROM dbo.Student s
INNER JOIN dbo.Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
WHERE s.SchoolID = @SchoolID AND s.Status = 'Active' AND ass.SchoolID = @SchoolID;

-- 2) Students available for API sync (VW_Attendance_Stu)
SELECT COUNT(*) AS Students_In_Sync_View
FROM dbo.VW_Attendance_Stu
WHERE SchoolID = @SchoolID;

SELECT TOP 10 DeviceID, StudentID, EducationYearID
FROM dbo.VW_Attendance_Stu
WHERE SchoolID = @SchoolID
ORDER BY DeviceID;

-- 3) Students assigned to a schedule? (required for PC download + API sync)
SELECT COUNT(*) AS Students_With_Schedule
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID AND Is_Student = 1 AND ScheduleID IS NOT NULL;

SELECT TOP 20 DeviceID, ID, Name, ScheduleID, Is_Student
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID AND Is_Student = 1 AND ScheduleID IS NOT NULL
ORDER BY DeviceID;

-- B) Employees on server vs PC device list
SELECT DeviceID, ID, FirstName, LastName, Job_Status
FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID AND DeviceID IS NOT NULL AND DeviceID > 0
ORDER BY DeviceID;

-- C) PC has these DeviceIDs but server VW_Emp_Info may not (update Employee_Info.DeviceID)
/*
261 MD SHAJALAL
281 Toma Mirja
282 Fatima
283 Sohanur Rahman
284 Rana Shikder
305 Md Abdullah Siyam
315 Rana Shikder
320 Umayer
321 Umu
323 (Bengali name)
329 Abdullah
330 Abdulllah
*/

-- Example: set missing employee DeviceID on server (adjust EmployeeID from Employee_Info)
-- UPDATE dbo.Employee_Info SET DeviceID = 261 WHERE SchoolID = 1012 AND ID = 'ET???';

-- D) Today after sync
SELECT COUNT(*) AS Today_Employee
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = CAST(GETDATE() AS DATE);

SELECT COUNT(*) AS Today_Student
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = CAST(GETDATE() AS DATE);
