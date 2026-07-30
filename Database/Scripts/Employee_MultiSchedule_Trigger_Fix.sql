-- Fix Tr_Employee_Attendance_Record_SMS for multi-schedule manual attendance.
-- The INSTEAD OF INSERT trigger previously ignored Attendance_ScheduleID and
-- blocked a second schedule insert for the same employee/day.

USE [Edu]
GO

IF OBJECT_ID(N'[dbo].[Tr_Employee_Attendance_Record_SMS]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[Tr_Employee_Attendance_Record_SMS];
GO

-- Legacy/alternate name seen on some servers
IF OBJECT_ID(N'[dbo].[Tr_Employee_Attendance_Record_NMR]', N'TR') IS NOT NULL
    DROP TRIGGER [dbo].[Tr_Employee_Attendance_Record_NMR];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[Tr_Employee_Attendance_Record_SMS] ON [dbo].[Employee_Attendance_Record]
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    DECLARE @EmployeeID int
    DECLARE @SchoolID int
    DECLARE @RegistrationID int
    DECLARE @AttendanceStatus nvarchar(50)
    DECLARE @AttendanceDate date
    DECLARE @EntryTime time(7)
    DECLARE @ExitTime time(7)
    DECLARE @ExitStatus nvarchar(50)
    DECLARE @Is_OUT bit
    DECLARE @IsFromDevice bit
    DECLARE @Attendance_ScheduleID int

    DECLARE @EmployeesName nvarchar(128)
    DECLARE @EmployeePhoneNumber nvarchar(50)
    DECLARE @SMSPhoneNo nvarchar(50)
    DECLARE @Is_Abs_SMS bit
    DECLARE @Is_Late_SMS bit
    DECLARE @LateEntryTime time(7)
    DECLARE @StartTime time(7)
    DECLARE @EndTime time(7)
    DECLARE @ScheduleTime time(7)
    DECLARE @SMS_Text nvarchar(500)

    SELECT *
    INTO #Temp_Table_Attendance
    FROM INSERTED

    SELECT TOP 1 @SchoolID = SchoolID
    FROM #Temp_Table_Attendance

    DECLARE @Is_All_SMS_On bit
    DECLARE @Is_Employee_All_SMS_Active bit
    DECLARE @Is_English_SMS bit
    DECLARE @SMS_TimeOut_Minute int
    DECLARE @Is_Employee_Abs_SMS_ON bit
    DECLARE @Is_Employee_Late_SMS_ON bit
    DECLARE @Is_Employee_SMS_OwnNumber bit
    DECLARE @Employee_SMS_Number nvarchar(50)

    SELECT
        @Is_All_SMS_On = Is_All_SMS_On,
        @Is_Employee_All_SMS_Active = Is_Employee_SMS_Active,
        @Is_English_SMS = Is_English_SMS,
        @SMS_TimeOut_Minute = SMS_TimeOut_Minute,
        @Is_Employee_Abs_SMS_ON = Is_Employee_Abs_SMS_ON,
        @Is_Employee_Late_SMS_ON = Is_Employee_Late_SMS_ON,
        @Is_Employee_SMS_OwnNumber = Is_Employee_SMS_OwnNumber,
        @Employee_SMS_Number = Employee_SMS_Number
    FROM Attendance_Device_Setting
    WHERE SchoolID = @SchoolID

    WHILE EXISTS (SELECT 1 FROM #Temp_Table_Attendance)
    BEGIN
        SELECT TOP 1
            @EmployeeID = EmployeeID,
            @RegistrationID = RegistrationID,
            @SchoolID = SchoolID,
            @AttendanceStatus = AttendanceStatus,
            @AttendanceDate = AttendanceDate,
            @EntryTime = EntryTime,
            @ExitTime = ExitTime,
            @ExitStatus = ExitStatus,
            @Is_OUT = Is_OUT,
            @IsFromDevice = IsFromDevice,
            @Attendance_ScheduleID = Attendance_ScheduleID
        FROM #Temp_Table_Attendance

        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[Employee_Attendance_Record]
            WHERE SchoolID = @SchoolID
              AND EmployeeID = @EmployeeID
              AND AttendanceDate = @AttendanceDate
              AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@Attendance_ScheduleID, 0)
        )
        BEGIN
            INSERT INTO Employee_Attendance_Record
            (
                SchoolID, RegistrationID, EmployeeID, Attendance_ScheduleID,
                AttendanceStatus, AttendanceDate, EntryTime, ExitTime,
                ExitStatus, Is_OUT, IsFromDevice
            )
            VALUES
            (
                @SchoolID, @RegistrationID, @EmployeeID, @Attendance_ScheduleID,
                @AttendanceStatus, @AttendanceDate, @EntryTime, @ExitTime,
                @ExitStatus, @Is_OUT, @IsFromDevice
            )

            -- Employee attendance SMS is queued by Attendance_API (not hardcoded here).
        END

        DELETE TOP (1) FROM #Temp_Table_Attendance
    END

    DROP TABLE #Temp_Table_Attendance

    SELECT SCOPE_IDENTITY() AS Employee_Attendance_RecordID
END
GO

PRINT 'Tr_Employee_Attendance_Record_SMS updated for multi-schedule support.';
GO
