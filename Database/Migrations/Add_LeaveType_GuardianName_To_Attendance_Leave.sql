-- =============================================
-- Migration: Add LeaveType and GuardianName columns
-- Table: Attendance_Leave
-- Date: 2025
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Attendance_Leave]') 
    AND name = 'LeaveType'
)
BEGIN
    ALTER TABLE [dbo].[Attendance_Leave]
    ADD [LeaveType] NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Attendance_Leave]') 
    AND name = 'GuardianName'
)
BEGIN
    ALTER TABLE [dbo].[Attendance_Leave]
    ADD [GuardianName] NVARCHAR(200) NULL;
END
GO
