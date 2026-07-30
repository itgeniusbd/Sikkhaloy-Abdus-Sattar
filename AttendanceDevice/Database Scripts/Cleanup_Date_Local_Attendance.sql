/*
    Delete local attendance for a specific date (PC SQLite).
    Replace @DateText with your date string as stored in the app (dd-MMM-yy).
    Examples: 17-Jul-26, 17-Jul-24

    PowerShell:
      powershell -ExecutionPolicy Bypass -File "Run-LocalCleanup.ps1" -Script Date -AttendanceDate "17-Jul-26"
*/

-- CHANGE THIS:
-- 17-Jul-26

DELETE FROM Attendance_Record
WHERE AttendanceDate = '17-Jul-26';

DELETE FROM Attendance_Log_Backup
WHERE Entry_Date = '17-Jul-26';

SELECT 'Attendance_Record left' AS Info, COUNT(*) AS Cnt FROM Attendance_Record;
SELECT 'Pending sync' AS Info, COUNT(*) AS Cnt
FROM Attendance_Record
WHERE Is_Sent = 0 OR Is_Updated = 0;
