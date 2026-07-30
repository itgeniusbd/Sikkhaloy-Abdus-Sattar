-- =====================================================
-- Multi-Schedule Attendance Migration Script
-- Adds Attendance_ScheduleID to Attendance_Record so a
-- student can have multiple attendance records per day
-- (e.g. Class schedule + Coaching schedule).
-- =====================================================

USE [Edu]
GO

-- 1. Add Attendance_ScheduleID column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'Attendance_Record'
      AND COLUMN_NAME = 'Attendance_ScheduleID'
)
BEGIN
    ALTER TABLE [dbo].[Attendance_Record]
    ADD Attendance_ScheduleID INT NULL;

    PRINT 'Attendance_ScheduleID column added to Attendance_Record.';
END
ELSE
BEGIN
    PRINT 'Attendance_ScheduleID column already exists.';
END
GO

-- 2. Back-fill Attendance_ScheduleID from Attendance_Schedule_AssignStudent
--    for existing records where the value is currently NULL.
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

-- 3. Remove duplicate records that would violate the new unique index
--    Group by the DATE portion of AttendanceDate (the column is DateTime)
--    and keep the record with the latest AttendanceRecordID for each group.
WITH CTE AS (
    SELECT *,
           ROW_NUMBER() OVER (
               PARTITION BY SchoolID, StudentID, CAST(AttendanceDate AS DATE), ISNULL(Attendance_ScheduleID, 0)
               ORDER BY AttendanceRecordID DESC
           ) AS RowNum
    FROM [dbo].[Attendance_Record]
)
DELETE FROM CTE
WHERE RowNum > 1;

PRINT 'Removed duplicate Attendance_Record rows for unique index.';
GO

-- 4. Drop the old unique constraint/index that only looks at
--    Student + Date if it exists (optional, depends on DB schema).
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

-- 5. Add a persisted computed column for the DATE portion if it does not exist,
--    then create the unique index on that computed column.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'Attendance_Record'
      AND COLUMN_NAME = 'AttendanceDateKey'
)
BEGIN
    ALTER TABLE [dbo].[Attendance_Record]
    ADD AttendanceDateKey AS CAST(AttendanceDate AS DATE) PERSISTED;

    PRINT 'Added persisted computed column AttendanceDateKey.';
END
ELSE
BEGIN
    PRINT 'AttendanceDateKey column already exists.';
END
GO

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

CREATE UNIQUE INDEX UQ_Attendance_Record_Student_Date_Schedule
ON [dbo].[Attendance_Record] (SchoolID, StudentID, AttendanceDateKey, Attendance_ScheduleID);

PRINT 'Created unique index UQ_Attendance_Record_Student_Date_Schedule.';
GO
