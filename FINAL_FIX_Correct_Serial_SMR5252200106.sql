-- ========================================
-- CORRECT FIX: Delete WRONG and keep CORRECT
-- Correct Serial: SMR5252200106 (from device screen)
-- Wrong Serial:   SMRS25200106 (typo in previous scripts)
-- ========================================

PRINT '=========================================='
PRINT 'Fixing Device Serial Number'
PRINT 'Correct Serial: SMR5252200106'
PRINT '=========================================='
PRINT ''

-- Show all current device entries
PRINT 'Current device entries in database:'
SELECT 
    MappingID,
    DeviceSerialNumber,
    DeviceName,
    SchoolID,
    LastPushTime,
    IsActive,
    CASE 
        WHEN DeviceSerialNumber = 'SMR5252200106' THEN '? CORRECT'
        ELSE '? WRONG'
    END AS Validity
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber LIKE '%252%200106%'
   OR DeviceSerialNumber LIKE '%25200106%'
ORDER BY DeviceSerialNumber

PRINT ''
PRINT '-------------------------------------------'
PRINT 'Deleting WRONG entry: SMRS25200106'
PRINT '-------------------------------------------'

DELETE FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMRS25200106'

PRINT '? Deleted'
PRINT ''

-- Ensure CORRECT entry exists
PRINT '-------------------------------------------'
PRINT 'Ensuring CORRECT entry: SMR5252200106'
PRINT '-------------------------------------------'

IF EXISTS (SELECT 1 FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMR5252200106')
BEGIN
    -- Update existing correct entry
    UPDATE Device_Institution_Mapping
    SET IsActive = 1,
        LastPushTime = GETDATE(),
        DeviceName = 'Main Gate Device',
        DeviceLocation = 'School Main Entrance'
    WHERE DeviceSerialNumber = 'SMR5252200106'
    
    PRINT '? Updated existing entry'
END
ELSE
BEGIN
    -- Insert new correct entry
    INSERT INTO Device_Institution_Mapping 
    (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation, IsActive, CreatedDate, LastPushTime)
    VALUES 
    (1012, 'SMR5252200106', 'Main Gate Device', 'School Main Entrance', 1, GETDATE(), GETDATE())
    
    PRINT '? Inserted new entry'
END

PRINT ''
PRINT '=========================================='
PRINT 'FINAL RESULT:'
PRINT '=========================================='

SELECT 
    MappingID,
    DeviceSerialNumber AS Serial,
    DeviceName,
    SchoolID,
    DeviceLocation AS Location,
    IsActive,
    LastPushTime,
    DATEDIFF(SECOND, LastPushTime, GETDATE()) AS SecondsSinceLastPush,
    CASE 
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 5 THEN '?? Connected (within 5 min)'
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 30 THEN '?? Recent (within 30 min)'
        ELSE '?? Disconnected (>30 min)'
    END AS Status
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMR5252200106'

PRINT ''
PRINT '? DONE!'
PRINT ''
PRINT 'Now:'
PRINT '1. Refresh dashboard: https://pushapi.sikkhaloy.com/'
PRINT '2. Should show: Connected Devices: 1'
PRINT '3. Only one device: SMR5252200106'
PRINT ''
PRINT '=========================================='
