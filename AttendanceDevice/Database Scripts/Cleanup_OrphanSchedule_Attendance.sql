/*
    LOCAL PC (AttendanceDevice SQLite)
    Delete attendance rows whose ScheduleID no longer exists in Schedule_Day.

    Run while AttendanceDevice is CLOSED.

    DB path (v4 Inno install):
      %LOCALAPPDATA%\SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db

    Tool: DB Browser for SQLite → Open DB → Execute SQL
    Or PowerShell (after closing app):
      sqlite3 "$env:LOCALAPPDATA\SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db" ".read Cleanup_OrphanSchedule_Attendance.sql"
*/

-- Preview orphan attendance
SELECT 'Orphan Attendance_Record' AS Info, COUNT(*) AS Cnt
FROM Attendance_Record
WHERE ScheduleID NOT IN (SELECT ScheduleID FROM Schedule_Day);

SELECT RecordID, DeviceID, ScheduleID, AttendanceDate, AttendanceStatus, EntryTime, ExitTime, Is_Sent
FROM Attendance_Record
WHERE ScheduleID NOT IN (SELECT ScheduleID FROM Schedule_Day)
ORDER BY RecordID DESC
LIMIT 50;

-- Preview orphan user-schedule links
SELECT 'Orphan User_Schedule' AS Info, COUNT(*) AS Cnt
FROM User_Schedule
WHERE ScheduleID NOT IN (SELECT ScheduleID FROM Schedule_Day);

-- Delete orphan rows
DELETE FROM Attendance_Record
WHERE ScheduleID NOT IN (SELECT ScheduleID FROM Schedule_Day);

DELETE FROM User_Schedule
WHERE ScheduleID NOT IN (SELECT ScheduleID FROM Schedule_Day);

-- Optional: only today's orphan rows (uncomment instead of full delete above)
/*
DELETE FROM Attendance_Record
WHERE ScheduleID NOT IN (SELECT ScheduleID FROM Schedule_Day)
  AND AttendanceDate = strftime('%d-%b-%y', 'now', 'localtime');
*/

UPDATE Schedule_Day SET Is_Abs_Count = 0;

SELECT 'Attendance_Record left' AS Info, COUNT(*) AS Cnt FROM Attendance_Record;
SELECT 'User_Schedule left' AS Info, COUNT(*) AS Cnt FROM User_Schedule;
