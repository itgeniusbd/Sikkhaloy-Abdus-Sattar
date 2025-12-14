-- ========================================
-- Delete WRONG device entry and keep CORRECT one
-- ========================================

PRINT '=========================================='
PRINT 'Cleaning up duplicate/wrong device entries'
PRINT '=========================================='
PRINT ''

-- Show current devices
PRINT 'Current devices in database:'
SELECT 
    MappingID,
    DeviceSerialNumber,
    DeviceName,
    SchoolID,
    LastPushTime,
    IsActive
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber LIKE '%25200106%'
ORDER BY DeviceSerialNumber

PRINT ''
PRINT 'Correct Serial: SMRS25200106'
PRINT 'Wrong Serial:   SMR5252200106 (has extra 5 and 2)'
PRINT ''

-- Delete the WRONG entry
DELETE FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMR5252200106'

PRINT '? Deleted wrong entry: SMR5252200106'
PRINT ''

-- Ensure correct entry exists and is active
IF EXISTS (SELECT 1 FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMRS25200106')
BEGIN
    UPDATE Device_Institution_Mapping
    SET IsActive = 1,
        LastPushTime = GETDATE()
    WHERE DeviceSerialNumber = 'SMRS25200106'
    
    PRINT '? Updated correct entry: SMRS25200106'
    PRINT '  - IsActive = 1'
    PRINT '  - LastPushTime = NOW'
END
ELSE
BEGIN
    -- Insert correct entry if not exists
    INSERT INTO Device_Institution_Mapping 
    (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation, IsActive, CreatedDate, LastPushTime)
    VALUES 
    (1012, 'SMRS25200106', 'Main Gate Device', 'School Main Entrance', 1, GETDATE(), GETDATE())
    
    PRINT '? Inserted correct entry: SMRS25200106'
END

PRINT ''
PRINT '=========================================='
PRINT 'Final result:'
PRINT '=========================================='

-- Show final state
SELECT 
    MappingID,
    DeviceSerialNumber,
    DeviceName,
    SchoolID,
    DeviceLocation,
    IsActive,
    LastPushTime,
    DATEDIFF(SECOND, LastPushTime, GETDATE()) AS SecondsSinceLastPush,
    CASE 
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 5 THEN '?? Connected'
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 30 THEN '?? Recent'
        ELSE '?? Disconnected'
    END AS Status
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMRS25200106'

PRINT ''
PRINT '? Done! Now you should see only 1 device in dashboard.'
PRINT ''
