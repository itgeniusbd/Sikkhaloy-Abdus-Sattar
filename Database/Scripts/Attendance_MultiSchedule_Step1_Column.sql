-- Step 1: Add Attendance_ScheduleID and AttendanceDateKey columns quickly.
-- Run this first. The unique index creation is in a separate script and
-- may take a long time on large tables, so run it during off-peak hours.

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

-- 2. Add a persisted computed column for the DATE portion
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
