/*
    School 1012 — delete attendance for a specific DATE on SQL Server (Edu).
    Change @TargetDate below, then run in SSMS on Edu database.
*/

SET NOCOUNT ON;

DECLARE @SchoolID    INT  = 1012;
DECLARE @TargetDate  DATE = '2026-07-17';  -- 17 July

PRINT 'SchoolID=' + CAST(@SchoolID AS varchar(10)) + ', Date=' + CONVERT(varchar(10), @TargetDate, 120);

SELECT 'Attendance_Record (before)' AS [Table], COUNT(*) AS Cnt
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @TargetDate
UNION ALL
SELECT 'Employee_Attendance_Record (before)', COUNT(*)
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @TargetDate;

BEGIN TRANSACTION;

    DELETE FROM dbo.Attendance_SMS
    WHERE SchoolID = @SchoolID AND AttendanceDate = @TargetDate;

    DELETE FROM dbo.Attendance_Record
    WHERE SchoolID = @SchoolID AND AttendanceDate = @TargetDate;

    DELETE FROM dbo.Employee_Attendance_Record
    WHERE SchoolID = @SchoolID AND AttendanceDate = @TargetDate;

COMMIT TRANSACTION;

SELECT 'Attendance_Record (after)' AS [Table], COUNT(*) AS Cnt
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @TargetDate;
