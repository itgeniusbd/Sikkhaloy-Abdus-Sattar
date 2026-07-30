-- Fix Tr_Attendance_Record_SMS for multi-schedule manual attendance.
-- The INSTEAD OF INSERT trigger previously ignored Attendance_ScheduleID and
-- blocked a second schedule insert for the same student/day.

USE [Edu]
GO

IF OBJECT_ID(N'[dbo].[Tr_Attendance_Record_SMS]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[Tr_Attendance_Record_SMS];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[Tr_Attendance_Record_SMS] ON [dbo].[Attendance_Record]
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    DECLARE @StudentID int
    DECLARE @SchoolID int
    DECLARE @RegistrationID int
    DECLARE @ClassID int
    DECLARE @StudentClassID int
    DECLARE @EducationYearID int
    DECLARE @Attendance nvarchar(50)
    DECLARE @AttendanceDate date
    DECLARE @Reason nvarchar(500)
    DECLARE @EntryTime time(7)
    DECLARE @ExitTime time(7)
    DECLARE @ExitStatus nvarchar(50)
    DECLARE @Is_OUT bit
    DECLARE @IsFromDevice bit
    DECLARE @Attendance_ScheduleID int

    DECLARE @StudentsName nvarchar(128)
    DECLARE @SMSPhoneNo nvarchar(50)
    DECLARE @Entry_Confirmation bit
    DECLARE @Exit_Confirmation bit
    DECLARE @Is_Abs_SMS bit
    DECLARE @Is_Late_SMS bit
    DECLARE @LateEntryTime time(7)
    DECLARE @StartTime time(7)
    DECLARE @EndTime time(7)

    DECLARE @ScheduleTime time(7)
    DECLARE @SMS_Text nvarchar(500)
    DECLARE @ScheduleName nvarchar(128)

    SELECT *
    INTO #Temp_Table_Attendance_Record
    FROM INSERTED

    SELECT TOP 1 @SchoolID = SchoolID
    FROM #Temp_Table_Attendance_Record

    DECLARE @Is_All_SMS_On bit
    DECLARE @Is_Student_All_SMS_Active bit
    DECLARE @Is_English_SMS bit
    DECLARE @SMS_TimeOut_Minute int
    DECLARE @Is_Student_Abs_SMS_ON bit
    DECLARE @Is_Student_Entry_SMS_ON bit
    DECLARE @Is_Student_Late_SMS_ON bit
    DECLARE @Is_Student_Exit_SMS_ON bit

    SELECT
        @Is_All_SMS_On = Is_All_SMS_On,
        @Is_Student_All_SMS_Active = Is_Student_All_SMS_Active,
        @Is_English_SMS = Is_English_SMS,
        @SMS_TimeOut_Minute = SMS_TimeOut_Minute,
        @Is_Student_Abs_SMS_ON = Is_Student_Abs_SMS_ON,
        @Is_Student_Entry_SMS_ON = Is_Student_Entry_SMS_ON,
        @Is_Student_Late_SMS_ON = Is_Student_Late_SMS_ON,
        @Is_Student_Exit_SMS_ON = Is_Student_Exit_SMS_ON
    FROM Attendance_Device_Setting
    WHERE SchoolID = @SchoolID

    DECLARE @SchoolName nvarchar(128)
    SELECT @SchoolName = SchoolName
    FROM SchoolInfo
    WHERE SchoolID = @SchoolID

    WHILE EXISTS (SELECT 1 FROM #Temp_Table_Attendance_Record)
    BEGIN
        SELECT TOP 1
            @StudentID = StudentID,
            @RegistrationID = RegistrationID,
            @SchoolID = SchoolID,
            @ClassID = ClassID,
            @StudentClassID = StudentClassID,
            @EducationYearID = EducationYearID,
            @Attendance = Attendance,
            @AttendanceDate = AttendanceDate,
            @Reason = Reason,
            @EntryTime = EntryTime,
            @ExitTime = ExitTime,
            @ExitStatus = ExitStatus,
            @Is_OUT = Is_OUT,
            @IsFromDevice = IsFromDevice,
            @Attendance_ScheduleID = Attendance_ScheduleID
        FROM #Temp_Table_Attendance_Record

        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[Attendance_Record]
            WHERE SchoolID = @SchoolID
              AND StudentClassID = @StudentClassID
              AND AttendanceDate = @AttendanceDate
              AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@Attendance_ScheduleID, 0)
        )
        BEGIN
            INSERT INTO Attendance_Record
            (
                StudentID, RegistrationID, SchoolID, ClassID, StudentClassID, EducationYearID,
                Attendance, AttendanceDate, Reason, EntryTime, ExitTime, ExitStatus, Is_OUT, IsFromDevice,
                Attendance_ScheduleID
            )
            VALUES
            (
                @StudentID, @RegistrationID, @SchoolID, @ClassID, @StudentClassID, @EducationYearID,
                @Attendance, @AttendanceDate, @Reason, @EntryTime, @ExitTime, @ExitStatus, @Is_OUT, @IsFromDevice,
                @Attendance_ScheduleID
            )

            -- Attendance SMS is queued by Attendance_API using SMS_Template (not hardcoded here).
        END

        DELETE TOP (1) FROM #Temp_Table_Attendance_Record
    END

    DROP TABLE #Temp_Table_Attendance_Record

    SELECT SCOPE_IDENTITY() AS AttendanceRecordID
END
GO

PRINT 'Tr_Attendance_Record_SMS updated for multi-schedule support.';
GO

-- Remove legacy UPDATE triggers that sent hardcoded SMS without SMS_Template support.
IF OBJECT_ID(N'[dbo].[Tr_Attendance_Record_Update]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[Tr_Attendance_Record_Update];
GO

IF OBJECT_ID(N'[dbo].[Tr_Attendance_Record_Update_SMS]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[Tr_Attendance_Record_Update_SMS];
GO

IF OBJECT_ID(N'[dbo].[Tr_Attendance_Record_SMS_Update]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[Tr_Attendance_Record_SMS_Update];
GO

-- Drop any other Attendance_Record trigger that still inserts Attendance_SMS.
DECLARE @dropSmsTriggerSql nvarchar(max) = N'';
SELECT @dropSmsTriggerSql = @dropSmsTriggerSql
    + N'DROP TRIGGER ' + QUOTENAME(OBJECT_SCHEMA_NAME(t.parent_id)) + N'.' + QUOTENAME(t.name) + N';' + CHAR(10)
FROM sys.triggers t
INNER JOIN sys.sql_modules m ON t.object_id = m.object_id
WHERE t.parent_id = OBJECT_ID(N'dbo.Attendance_Record')
  AND t.name <> N'Tr_Attendance_Record_SMS'
  AND m.definition LIKE N'%INSERT INTO Attendance_SMS%';
IF LEN(@dropSmsTriggerSql) > 0
    EXEC sp_executesql @dropSmsTriggerSql;
GO
