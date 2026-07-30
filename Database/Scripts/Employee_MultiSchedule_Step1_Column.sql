-- Step 1: Add Attendance_ScheduleID to Employee_Attendance_Record
-- for multi-schedule manual/device employee attendance.

USE [Edu]
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'Employee_Attendance_Record'
      AND COLUMN_NAME = 'Attendance_ScheduleID'
)
BEGIN
    ALTER TABLE [dbo].[Employee_Attendance_Record]
    ADD Attendance_ScheduleID INT NULL;

    PRINT 'Attendance_ScheduleID column added to Employee_Attendance_Record.';
END
ELSE
BEGIN
    PRINT 'Attendance_ScheduleID column already exists on Employee_Attendance_Record.';
END
GO
