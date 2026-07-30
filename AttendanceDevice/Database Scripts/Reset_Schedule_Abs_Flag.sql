/*
    Reset Abs auto-count flag so device can insert Abs again (e.g. night schedule had no User_Schedule assign).
    Run while AttendanceDevice is CLOSED.

    DB: %LOCALAPPDATA%\SIKKHALOY\AttendanceDevice\SikkhaloyAppDB.db
    Or: powershell -ExecutionPolicy Bypass -File Run-LocalCleanup.ps1 won't do this — use DB Browser or below.
*/

-- All schedules today
-- UPDATE Schedule_Day SET Is_Abs_Count = 0;

-- Night student schedule only (change ScheduleID if needed)
UPDATE Schedule_Day SET Is_Abs_Count = 0 WHERE ScheduleID = 2799;

SELECT ScheduleID, Day, LateEntryTime, Is_Abs_Count
FROM Schedule_Day
WHERE ScheduleID = 2799;
