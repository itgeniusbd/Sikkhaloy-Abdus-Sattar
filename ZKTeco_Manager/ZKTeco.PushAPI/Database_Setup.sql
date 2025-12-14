-- =====================================================
-- ZKTeco PushAPI - Database Setup Script
-- =====================================================
-- ?? script run ???? ??? ??????? ???? ??:
-- 1. Sikkhaloy database already exists
-- 2. Required tables already exist
-- =====================================================

USE [YOUR_DATABASE_NAME]
GO

-- =====================================================
-- 1. Check if Device table exists
-- =====================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Device')
BEGIN
    PRINT 'Error: Device table does not exist!'
    PRINT 'Please create the Device table first.'
END
GO

-- =====================================================
-- 2. Sample Device Configuration
-- =====================================================
-- ????? device information ????? replace ????
/*
INSERT INTO Device (DeviceSerial, SchoolID, DeviceName, DeviceIP, Port, DeviceStatus)
VALUES 
('DEVICE001', 1, 'Main Gate', '192.168.0.201', 4370, 'Active'),
('DEVICE002', 1, 'Back Gate', '192.168.0.202', 4370, 'Active')
*/

-- =====================================================
-- 3. Check if Attendance_Device_Settings exists
-- =====================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Attendance_Device_Settings')
BEGIN
    PRINT 'Creating Attendance_Device_Settings table...'
    
    CREATE TABLE Attendance_Device_Settings (
        SettingID INT PRIMARY KEY IDENTITY(1,1),
        SchoolID INT NOT NULL,
        Is_Device_Attendance_Enable BIT DEFAULT 1,
        Is_Student_Attendance_Enable BIT DEFAULT 1,
        Is_Employee_Attendance_Enable BIT DEFAULT 1,
        Is_Holiday_As_Offday BIT DEFAULT 1,
        CONSTRAINT FK_AttendanceSettings_School FOREIGN KEY (SchoolID) 
            REFERENCES SchoolInfo(SchoolID)
    )
    
    PRINT 'Attendance_Device_Settings table created successfully!'
END
GO

-- =====================================================
-- 4. Enable Attendance Settings (Sample)
-- =====================================================
-- ????? SchoolID ????? replace ????
/*
IF NOT EXISTS (SELECT * FROM Attendance_Device_Settings WHERE SchoolID = 1)
BEGIN
    INSERT INTO Attendance_Device_Settings 
    (SchoolID, Is_Device_Attendance_Enable, Is_Student_Attendance_Enable, 
     Is_Employee_Attendance_Enable, Is_Holiday_As_Offday)
    VALUES 
    (1, 1, 1, 1, 1)
    
    PRINT 'Attendance settings enabled for SchoolID = 1'
END
*/
GO

-- =====================================================
-- 5. Sample Schedule Configuration
-- =====================================================
-- ????? schedule information ????? replace ????
/*
-- First, insert schedule in Attendance_Schedule table
INSERT INTO Attendance_Schedule (ScheduleID, SchoolID, ScheduleName, EducationYearID)
VALUES (1, 1, 'Default Schedule', 1)

-- Then, insert schedule days
INSERT INTO Attendance_Schedule_Day 
(ScheduleID, SchoolID, Day, StartTime, LateEntryTime, EndTime, Is_OnDay)
VALUES 
(1, 1, 'Sunday', '08:00:00', '08:15:00', '15:00:00', 1),
(1, 1, 'Monday', '08:00:00', '08:15:00', '15:00:00', 1),
(1, 1, 'Tuesday', '08:00:00', '08:15:00', '15:00:00', 1),
(1, 1, 'Wednesday', '08:00:00', '08:15:00', '15:00:00', 1),
(1, 1, 'Thursday', '08:00:00', '08:15:00', '15:00:00', 1),
(1, 1, 'Friday', '08:00:00', '08:15:00', '13:00:00', 0),  -- Off day
(1, 1, 'Saturday', '08:00:00', '08:15:00', '15:00:00', 0) -- Off day

PRINT 'Schedule configuration completed!'
*/
GO

-- =====================================================
-- 6. Verify Setup - Run these queries to check
-- =====================================================

-- Check if all required tables exist
SELECT 'Table Existence Check' as CheckType, * FROM (
    SELECT 
        'Attendance_Record' as TableName,
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Attendance_Record') 
            THEN 'EXISTS' ELSE 'MISSING' END as Status
    UNION ALL
    SELECT 
        'Employee_Attendance_Record',
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Employee_Attendance_Record') 
            THEN 'EXISTS' ELSE 'MISSING' END
    UNION ALL
    SELECT 
        'Device',
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Device') 
            THEN 'EXISTS' ELSE 'MISSING' END
    UNION ALL
    SELECT 
        'Student',
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Student') 
            THEN 'EXISTS' ELSE 'MISSING' END
    UNION ALL
    SELECT 
        'Employee',
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Employee') 
            THEN 'EXISTS' ELSE 'MISSING' END
    UNION ALL
    SELECT 
        'Attendance_Schedule_Day',
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Attendance_Schedule_Day') 
            THEN 'EXISTS' ELSE 'MISSING' END
    UNION ALL
    SELECT 
        'Attendance_Device_Settings',
        CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Attendance_Device_Settings') 
            THEN 'EXISTS' ELSE 'MISSING' END
) AS TableCheck
GO

-- Check device configuration
SELECT 'Device Configuration' as CheckType, * FROM Device
GO

-- Check attendance settings
SELECT 'Attendance Settings' as CheckType, * FROM Attendance_Device_Settings
GO

-- Check schedule configuration
SELECT 'Schedule Configuration' as CheckType, * FROM Attendance_Schedule_Day
ORDER BY 
    CASE Day 
        WHEN 'Sunday' THEN 1
        WHEN 'Monday' THEN 2
        WHEN 'Tuesday' THEN 3
        WHEN 'Wednesday' THEN 4
        WHEN 'Thursday' THEN 5
        WHEN 'Friday' THEN 6
        WHEN 'Saturday' THEN 7
    END
GO

-- =====================================================
-- 7. Query to check students with DeviceID
-- =====================================================
SELECT 
    s.StudentID,
    s.DeviceID,
    s.StudentsName,
    s.Status,
    sc.EducationYearID,
    sc.ScheduleID
FROM Student s
INNER JOIN Student_Class sc ON s.StudentID = sc.StudentID
INNER JOIN Education_Year ey ON sc.EducationYearID = ey.EducationYearID
WHERE s.DeviceID IS NOT NULL
    AND s.Status = 'Active'
    AND ey.Status = 'True'
ORDER BY s.DeviceID
GO

-- =====================================================
-- 8. Query to check employees with DeviceID
-- =====================================================
SELECT 
    e.EmployeeID,
    e.DeviceID,
    e.Name,
    e.Status,
    eas.EducationYearID,
    eas.ScheduleID
FROM Employee e
LEFT JOIN Employee_Attendance_Schedule eas ON e.EmployeeID = eas.EmployeeID
WHERE e.DeviceID IS NOT NULL
    AND e.Status = 'Active'
ORDER BY e.DeviceID
GO

-- =====================================================
-- 9. View to check today's attendance
-- =====================================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_TodayAttendance')
    DROP VIEW VW_TodayAttendance
GO

CREATE VIEW VW_TodayAttendance AS
SELECT 
    'Student' as UserType,
    s.DeviceID,
    s.StudentsName as UserName,
    ar.Attendance,
    ar.AttendanceDate,
    ar.EntryTime,
    ar.ExitTime,
    ar.Is_OUT
FROM Attendance_Record ar
INNER JOIN Student s ON ar.StudentID = s.StudentID
WHERE ar.AttendanceDate = CAST(GETDATE() AS DATE)

UNION ALL

SELECT 
    'Employee' as UserType,
    e.DeviceID,
    e.Name as UserName,
    ear.AttendanceStatus as Attendance,
    ear.AttendanceDate,
    ear.EntryTime,
    ear.ExitTime,
    ear.Is_OUT
FROM Employee_Attendance_Record ear
INNER JOIN Employee e ON ear.EmployeeID = e.EmployeeID
WHERE ear.AttendanceDate = CAST(GETDATE() AS DATE)
GO

-- View today's attendance
SELECT * FROM VW_TodayAttendance
ORDER BY AttendanceDate DESC, EntryTime DESC
GO

-- =====================================================
-- 10. Stored Procedure to test attendance save
-- =====================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_TestAttendanceSave')
    DROP PROCEDURE SP_TestAttendanceSave
GO

CREATE PROCEDURE SP_TestAttendanceSave
    @DeviceID INT,
    @AttendanceTime DATETIME,
    @IsStudent BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SchoolID INT = 1  -- Change to your SchoolID
    DECLARE @UserID INT
    DECLARE @ScheduleID INT
    DECLARE @DayName NVARCHAR(20) = DATENAME(WEEKDAY, @AttendanceTime)
    
    IF @IsStudent = 1
    BEGIN
        -- Get student info
        SELECT TOP 1 
            @UserID = s.StudentID,
            @ScheduleID = sc.ScheduleID
        FROM Student s
        INNER JOIN Student_Class sc ON s.StudentID = sc.StudentID
        WHERE s.DeviceID = @DeviceID 
            AND s.SchoolID = @SchoolID
            AND s.Status = 'Active'
        
        IF @UserID IS NOT NULL
        BEGIN
            PRINT 'Student found: ' + CAST(@UserID AS VARCHAR)
            PRINT 'ScheduleID: ' + CAST(@ScheduleID AS VARCHAR)
            PRINT 'Day: ' + @DayName
        END
        ELSE
        BEGIN
            PRINT 'Student not found for DeviceID: ' + CAST(@DeviceID AS VARCHAR)
        END
    END
    ELSE
    BEGIN
        -- Get employee info
        SELECT TOP 1 
            @UserID = e.EmployeeID,
            @ScheduleID = eas.ScheduleID
        FROM Employee e
        LEFT JOIN Employee_Attendance_Schedule eas ON e.EmployeeID = eas.EmployeeID
        WHERE e.DeviceID = @DeviceID 
            AND e.SchoolID = @SchoolID
            AND e.Status = 'Active'
        
        IF @UserID IS NOT NULL
        BEGIN
            PRINT 'Employee found: ' + CAST(@UserID AS VARCHAR)
            PRINT 'ScheduleID: ' + CAST(@ScheduleID AS VARCHAR)
            PRINT 'Day: ' + @DayName
        END
        ELSE
        BEGIN
            PRINT 'Employee not found for DeviceID: ' + CAST(@DeviceID AS VARCHAR)
        END
    END
    
    -- Check schedule
    IF @ScheduleID IS NOT NULL
    BEGIN
        SELECT 
            'Schedule Info' as InfoType,
            Day,
            CONVERT(VARCHAR(8), StartTime, 108) as StartTime,
            CONVERT(VARCHAR(8), LateEntryTime, 108) as LateEntryTime,
            CONVERT(VARCHAR(8), EndTime, 108) as EndTime,
            Is_OnDay
        FROM Attendance_Schedule_Day
        WHERE ScheduleID = @ScheduleID AND Day = @DayName
    END
END
GO

-- Test the procedure
-- EXEC SP_TestAttendanceSave @DeviceID = 1, @AttendanceTime = '2024-12-10 09:30:00', @IsStudent = 1
GO

-- =====================================================
-- SETUP COMPLETE
-- =====================================================
PRINT ''
PRINT '====================================================='
PRINT 'ZKTeco PushAPI Database Setup Script Completed!'
PRINT '====================================================='
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Update Device table with your device information'
PRINT '2. Enable Attendance_Device_Settings for your school'
PRINT '3. Configure Attendance_Schedule_Day'
PRINT '4. Update Web.config connection string'
PRINT '5. Deploy API and test'
PRINT ''
PRINT 'Run verification queries above to check configuration'
PRINT '====================================================='
GO
