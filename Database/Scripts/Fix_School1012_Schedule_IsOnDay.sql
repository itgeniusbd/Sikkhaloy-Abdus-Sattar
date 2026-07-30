/*
    Optional: enable schedule days for school 1012 (if Is_OnDay = 0 in SQL Server).
    Run on Edu database BEFORE device punch test.
*/

DECLARE @SchoolID INT = 1012;

SELECT ScheduleID, Day, StartTime, EndTime, Is_OnDay
FROM dbo.Attendance_Schedule_Day
WHERE SchoolID = @SchoolID
ORDER BY ScheduleID, ScheduleDayID;

UPDATE dbo.Attendance_Schedule_Day
SET Is_OnDay = 1
WHERE SchoolID = @SchoolID
  AND Is_OnDay = 0;

PRINT 'Updated rows: ' + CAST(@@ROWCOUNT AS varchar(20));

SELECT ScheduleID, Day, StartTime, EndTime, Is_OnDay
FROM dbo.Attendance_Schedule_Day
WHERE SchoolID = @SchoolID
ORDER BY ScheduleID, ScheduleDayID;
