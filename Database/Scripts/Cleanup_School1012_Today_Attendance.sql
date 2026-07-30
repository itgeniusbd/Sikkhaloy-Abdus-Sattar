/*
    School 1012 — delete ALL attendance for TODAY (manual + device).
    Run on: Edu / Education SQL Server (LOOPS-IT-VM-1 or production).

    BEFORE RUN:
    - Close AttendanceDevice app on school PC(s) before local cleanup.
    - Take backup if needed.

    AFTER RUN:
    - Re-open AttendanceDevice, Settings → User Info → Download
    - Settings → Schedule (refresh), then test punch on Display.
*/

SET NOCOUNT ON;

DECLARE @SchoolID INT = 1012;
DECLARE @Today    DATE = CAST(GETDATE() AS DATE);  -- change to '2026-07-16' if server date differs

PRINT 'SchoolID=' + CAST(@SchoolID AS varchar(10)) + ', Today=' + CONVERT(varchar(10), @Today, 120);

-- Preview counts
SELECT 'Attendance_Record (before)' AS [Table], COUNT(*) AS Cnt
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
UNION ALL
SELECT 'Employee_Attendance_Record (before)', COUNT(*)
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
UNION ALL
SELECT 'Attendance_SMS (before)', COUNT(*)
FROM dbo.Attendance_SMS
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today;

BEGIN TRANSACTION;

    DELETE FROM dbo.Attendance_SMS
    WHERE SchoolID = @SchoolID
      AND AttendanceDate = @Today;

    PRINT 'Deleted Attendance_SMS: ' + CAST(@@ROWCOUNT AS varchar(20));

    DELETE FROM dbo.Attendance_Record
    WHERE SchoolID = @SchoolID
      AND AttendanceDate = @Today;

    PRINT 'Deleted Attendance_Record: ' + CAST(@@ROWCOUNT AS varchar(20));

    DELETE FROM dbo.Employee_Attendance_Record
    WHERE SchoolID = @SchoolID
      AND AttendanceDate = @Today;

    PRINT 'Deleted Employee_Attendance_Record: ' + CAST(@@ROWCOUNT AS varchar(20));

COMMIT TRANSACTION;

-- Verify
SELECT 'Attendance_Record (after)' AS [Table], COUNT(*) AS Cnt
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
UNION ALL
SELECT 'Employee_Attendance_Record (after)', COUNT(*)
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today
UNION ALL
SELECT 'Attendance_SMS (after)', COUNT(*)
FROM dbo.Attendance_SMS
WHERE SchoolID = @SchoolID AND AttendanceDate = @Today;

PRINT 'Done.';
