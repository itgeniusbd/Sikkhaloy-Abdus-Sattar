-- Verify employee multi-schedule attendance for one school/date.
-- IMPORTANT: Use the same date shown in the web page (e.g. 2026-07-16, not 2024).

USE [Edu]
GO

DECLARE @SchoolID INT = 1012;          -- change if needed
DECLARE @AttendanceDate DATE = '2026-07-16';  -- must match UI date

-- How many schedules have saved records?
SELECT Attendance_ScheduleID, COUNT(*) AS row_count
FROM Employee_Attendance_Record
WHERE SchoolID = @SchoolID
  AND CAST(AttendanceDate AS DATE) = @AttendanceDate
GROUP BY Attendance_ScheduleID
ORDER BY Attendance_ScheduleID;

-- Same employee with multiple schedules on same day (should exist after Step2)
SELECT EmployeeID, CAST(AttendanceDate AS DATE) AS AttDate,
       Attendance_ScheduleID, AttendanceStatus
FROM Employee_Attendance_Record
WHERE SchoolID = @SchoolID
  AND CAST(AttendanceDate AS DATE) = @AttendanceDate
ORDER BY EmployeeID, Attendance_ScheduleID;

-- Legacy rows still missing schedule (should be 0 after Step2 back-fill)
SELECT COUNT(*) AS null_schedule_rows
FROM Employee_Attendance_Record
WHERE SchoolID = @SchoolID
  AND Attendance_ScheduleID IS NULL;

GO
