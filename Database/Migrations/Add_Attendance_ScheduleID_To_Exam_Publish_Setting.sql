-- Add Attendance_ScheduleID to exam publish settings (0 = all schedules)
-- Run on Edu database before deploying Publish Result schedule filter.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Publish_Setting]')
      AND name = 'Attendance_ScheduleID'
)
BEGIN
    ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD Attendance_ScheduleID INT NOT NULL CONSTRAINT DF_Exam_Publish_Setting_Attendance_ScheduleID DEFAULT (0);
    PRINT 'Added Attendance_ScheduleID to Exam_Publish_Setting.';
END
ELSE
    PRINT 'Attendance_ScheduleID already exists on Exam_Publish_Setting.';

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Cumulative_Setting]')
      AND name = 'Attendance_ScheduleID'
)
BEGIN
    ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD Attendance_ScheduleID INT NOT NULL CONSTRAINT DF_Exam_Cumulative_Setting_Attendance_ScheduleID DEFAULT (0);
    PRINT 'Added Attendance_ScheduleID to Exam_Cumulative_Setting.';
END
ELSE
    PRINT 'Attendance_ScheduleID already exists on Exam_Cumulative_Setting.';

GO
