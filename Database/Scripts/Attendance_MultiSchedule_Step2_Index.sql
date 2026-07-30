-- Step 2: Back-fill and create unique index.
-- Run this during off-peak hours because it can take a long time
-- on large Attendance_Record tables.

USE [Edu]
GO

-- Back-fill Attendance_ScheduleID from Attendance_Schedule_AssignStudent
UPDATE ar
SET ar.Attendance_ScheduleID = ass.ScheduleID
FROM [dbo].[Attendance_Record] ar
INNER JOIN [dbo].[Attendance_Schedule_AssignStudent] ass
    ON ar.StudentID = ass.StudentID
    AND ar.SchoolID = ass.SchoolID
WHERE ar.Attendance_ScheduleID IS NULL
  AND ass.ScheduleID IS NOT NULL;

PRINT 'Back-filled Attendance_ScheduleID from Attendance_Schedule_AssignStudent.';
GO

-- Remove duplicate records that would violate the new unique index
WITH CTE AS (
    SELECT *,
           ROW_NUMBER() OVER (
               PARTITION BY SchoolID, StudentID, AttendanceDateKey, ISNULL(Attendance_ScheduleID, 0)
               ORDER BY AttendanceRecordID DESC
           ) AS RowNum
    FROM [dbo].[Attendance_Record]
)
DELETE FROM CTE
WHERE RowNum > 1;

PRINT 'Removed duplicate Attendance_Record rows for unique index.';
GO

-- Drop the old unique index if it exists
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('[dbo].[Attendance_Record]')
      AND name = 'UQ_Attendance_Record_Student_Date'
)
BEGIN
    DROP INDEX UQ_Attendance_Record_Student_Date ON [dbo].[Attendance_Record];
    PRINT 'Dropped old unique index UQ_Attendance_Record_Student_Date.';
END
GO

-- Drop the new index if it already exists
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('[dbo].[Attendance_Record]')
      AND name = 'UQ_Attendance_Record_Student_Date_Schedule'
)
BEGIN
    DROP INDEX UQ_Attendance_Record_Student_Date_Schedule ON [dbo].[Attendance_Record];
    PRINT 'Dropped existing unique index UQ_Attendance_Record_Student_Date_Schedule.';
END
GO

-- Create unique index (may take a long time on large tables)
CREATE UNIQUE INDEX UQ_Attendance_Record_Student_Date_Schedule
ON [dbo].[Attendance_Record] (SchoolID, StudentID, AttendanceDateKey, Attendance_ScheduleID);

PRINT 'Created unique index UQ_Attendance_Record_Student_Date_Schedule.';
GO
