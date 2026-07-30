/*
    Local PC (AttendanceDevice SQLite) — delete TODAY's attendance + backup logs.
    Run while AttendanceDevice is CLOSED.

    DB path (v4 Inno):
      %LOCALAPPDATA%\SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db

    PowerShell:
      powershell -ExecutionPolicy Bypass -File "...\Database Scripts\Run-LocalCleanup.ps1" -Script Today
*/

DELETE FROM Attendance_Record
WHERE AttendanceDate = strftime('%d-%b-%y', 'now', 'localtime');

DELETE FROM Attendance_Log_Backup
WHERE Entry_Date = strftime('%d-%b-%y', 'now', 'localtime');

UPDATE Schedule_Day SET Is_Abs_Count = 0;

SELECT 'Attendance_Record left' AS Info, COUNT(*) AS Cnt FROM Attendance_Record;
SELECT 'Pending sync (Is_Sent=0 or Is_Updated=0)' AS Info, COUNT(*) AS Cnt
FROM Attendance_Record
WHERE Is_Sent = 0 OR Is_Updated = 0;
