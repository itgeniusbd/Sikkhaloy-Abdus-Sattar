/*
    Reset local sync flags so pending records re-send to server.
    Run in SQLite (DB Browser) while AttendanceDevice is CLOSED.
    Or use the PowerShell block below.
*/

UPDATE Attendance_Record
SET Is_Sent = 0, Is_Updated = 0
WHERE AttendanceStatus <> 'Abs'
   OR (EntryTime IS NOT NULL AND EntryTime <> '');

-- Optional: delete auto-Abs rows without punch time (noise)
DELETE FROM Attendance_Record
WHERE (EntryTime IS NULL OR EntryTime = '')
  AND AttendanceStatus = 'Abs';

SELECT 'Pending after reset' AS Info,
       COUNT(*) AS Cnt
FROM Attendance_Record
WHERE Is_Sent = 0 OR Is_Updated = 0;
