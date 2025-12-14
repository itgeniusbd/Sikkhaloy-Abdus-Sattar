-- ========================================
-- ALL-IN-ONE FIX SCRIPT
-- Run this to fix everything at once
-- ========================================

PRINT '=========================================='
PRINT 'ZKTeco Device Setup & Fix Script'
PRINT 'Device Serial: SMRS25200106'
PRINT 'Time: ' + CONVERT(VARCHAR(20), GETDATE(), 120)
PRINT '=========================================='
PRINT ''

-- ==========================================
-- STEP 1: Add/Update Device in Database
-- ==========================================
PRINT 'STEP 1: Checking Device Registration...'

IF NOT EXISTS (SELECT 1 FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMRS25200106')
BEGIN
    PRINT '  ? Device NOT found. Adding now...'
    
    INSERT INTO Device_Institution_Mapping 
    (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation, IsActive, CreatedDate, LastPushTime)
    VALUES 
    (1012, 'SMRS25200106', 'Main Gate Device', 'School Main Entrance', 1, GETDATE(), GETDATE())
    
    PRINT '  ? Device added successfully!'
END
ELSE
BEGIN
    PRINT '  ? Device found. Updating LastPushTime...'
    
    UPDATE Device_Institution_Mapping 
    SET LastPushTime = GETDATE(),
        IsActive = 1
    WHERE DeviceSerialNumber = 'SMRS25200106'
    
    PRINT '  ? Device updated successfully!'
END

-- Show device status
SELECT 
    '  Device Status:' AS Info,
    DeviceSerialNumber,
    SchoolID,
    IsActive,
    LastPushTime,
    DATEDIFF(SECOND, LastPushTime, GETDATE()) AS SecondsSinceLastPush
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMRS25200106'

PRINT ''

-- ==========================================
-- STEP 2: Check Employee with DeviceID 188
-- ==========================================
PRINT 'STEP 2: Checking Employee DeviceID Mapping...'

IF EXISTS (SELECT 1 FROM VW_Emp_Info WHERE DeviceID = 188)
BEGIN
    PRINT '  ? Employee with DeviceID 188 found:'
    
    SELECT 
        '  Employee Info:' AS Info,
        EmployeeID,
        FirstName + ' ' + LastName AS Name,
        DeviceID,
        SchoolID,
        Job_Status
    FROM VW_Emp_Info
    WHERE DeviceID = 188
END
ELSE
BEGIN
    PRINT '  ? NO employee found with DeviceID 188'
    PRINT '  ACTION REQUIRED: You need to:'
    PRINT '    1. Find employee "MUHAMMAD KAZI ABDUS SATTI" in database'
    PRINT '    2. Run this query:'
    PRINT ''
    PRINT '    UPDATE Employee_Info'
    PRINT '    SET DeviceID = 188'
    PRINT '    WHERE EmployeeID = [ACTUAL_EMPLOYEE_ID]'
    PRINT ''
    PRINT '  OR create a new employee with DeviceID = 188'
    
    -- Show employees without DeviceID
    PRINT ''
    PRINT '  Employees in SchoolID 1012 WITHOUT DeviceID:'
    
    SELECT TOP 5
        EmployeeID,
        FirstName + ' ' + LastName AS Name,
        DeviceID,
        Job_Status
    FROM VW_Emp_Info
    WHERE SchoolID = 1012
      AND Job_Status = 'Active'
      AND DeviceID IS NULL
    ORDER BY EmployeeID
END

PRINT ''

-- ==========================================
-- STEP 3: Test Data - Insert Sample Attendance
-- ==========================================
PRINT 'STEP 3: Checking Recent Attendance...'

-- Check if any attendance exists in last hour
IF EXISTS (
    SELECT 1 
    FROM Employee_Attendance_Record 
    WHERE SchoolID = 1012 
      AND AttendanceDate >= DATEADD(HOUR, -1, GETDATE())
)
BEGIN
    PRINT '  ? Recent attendance found:'
    
    SELECT TOP 3
        EmployeeID,
        CAST(AttendanceDate AS DATE) AS Date,
        CAST(EntryTime AS TIME) AS Time,
        AttendanceStatus
    FROM Employee_Attendance_Record
    WHERE SchoolID = 1012
      AND AttendanceDate >= DATEADD(HOUR, -1, GETDATE())
    ORDER BY AttendanceDate DESC, EntryTime DESC
END
ELSE
BEGIN
    PRINT '  ? No attendance records in last hour'
    PRINT '  This means device is NOT uploading data to API'
END

PRINT ''

-- ==========================================
-- STEP 4: Configuration Summary
-- ==========================================
PRINT '=========================================='
PRINT 'CONFIGURATION SUMMARY'
PRINT '=========================================='

DECLARE @DeviceExists BIT = 0
DECLARE @EmployeeExists BIT = 0
DECLARE @AttendanceExists BIT = 0

IF EXISTS (SELECT 1 FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMRS25200106' AND IsActive = 1)
    SET @DeviceExists = 1

IF EXISTS (SELECT 1 FROM VW_Emp_Info WHERE DeviceID = 188 AND SchoolID = 1012)
    SET @EmployeeExists = 1

IF EXISTS (SELECT 1 FROM Employee_Attendance_Record WHERE SchoolID = 1012 AND AttendanceDate >= DATEADD(HOUR, -1, GETDATE()))
    SET @AttendanceExists = 1

PRINT ''
PRINT 'Status:'
PRINT '  Device Registered: ' + CASE WHEN @DeviceExists = 1 THEN '? YES' ELSE '? NO' END
PRINT '  Employee Mapped:   ' + CASE WHEN @EmployeeExists = 1 THEN '? YES' ELSE '? NO' END
PRINT '  Data Uploading:    ' + CASE WHEN @AttendanceExists = 1 THEN '? YES' ELSE '? NO' END
PRINT ''

-- ==========================================
-- STEP 5: Next Steps
-- ==========================================
PRINT '=========================================='
PRINT 'NEXT STEPS'
PRINT '=========================================='
PRINT ''

IF @DeviceExists = 1 AND @EmployeeExists = 1 AND @AttendanceExists = 1
BEGIN
    PRINT '? Everything is configured correctly!'
    PRINT '? Dashboard should show device as CONNECTED now'
    PRINT ''
    PRINT 'Go to: https://pushapi.sikkhaloy.com/'
    PRINT 'Refresh the page (F5)'
END
ELSE
BEGIN
    IF @DeviceExists = 0
    BEGIN
        PRINT '1. ? Device NOT registered'
        PRINT '   ? Run this script again to add device'
    END
    ELSE
        PRINT '1. ? Device registered'
    
    PRINT ''
    
    IF @EmployeeExists = 0
    BEGIN
        PRINT '2. ? Employee DeviceID NOT configured'
        PRINT '   ? Find employee and assign DeviceID = 188'
        PRINT '   ? Or use script: Assign_DeviceID_To_Employee_188.sql'
    END
    ELSE
        PRINT '2. ? Employee DeviceID configured'
    
    PRINT ''
    
    IF @AttendanceExists = 0
    BEGIN
        PRINT '3. ? Device NOT uploading data'
        PRINT '   ? Check device ADMS configuration:'
        PRINT '     MENU ? System ? Communications ? ADMS Settings'
        PRINT '     Server: pushapi.sikkhaloy.com'
        PRINT '     Port: 4370 or 80'
        PRINT '     Enable: YES'
        PRINT '     Upload: 30 seconds'
        PRINT ''
        PRINT '   ? Reboot device: MENU ? System ? Power ? Reboot'
        PRINT '   ? Wait 2-3 minutes'
        PRINT '   ? Punch again'
    END
    ELSE
        PRINT '3. ? Device uploading data'
END

PRINT ''
PRINT '=========================================='
PRINT 'Script completed!'
PRINT '=========================================='
