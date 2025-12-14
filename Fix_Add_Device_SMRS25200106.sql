-- ========================================
-- Find ALL devices in database
-- ========================================

-- 1. Check ALL devices in Device_Institution_Mapping
SELECT 
    MappingID,
    DeviceSerialNumber,
    DeviceName,
    SchoolID,
    DeviceLocation,
    IsActive,
    LastPushTime,
    DATEDIFF(MINUTE, LastPushTime, GETDATE()) AS MinutesSinceLastPush,
    CreatedDate
FROM Device_Institution_Mapping
ORDER BY CreatedDate DESC

-- 2. Check if serial exists with slight variation
SELECT 
    DeviceSerialNumber,
    LEN(DeviceSerialNumber) AS SerialLength,
    SchoolID,
    IsActive
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber LIKE '%SMRS%'
   OR DeviceSerialNumber LIKE '%25200106%'

-- 3. Insert device if not exists
IF NOT EXISTS (SELECT 1 FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMRS25200106')
BEGIN
    INSERT INTO Device_Institution_Mapping 
    (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation, IsActive, CreatedDate, LastPushTime)
    VALUES 
    (1012, 'SMRS25200106', 'Main Gate Device', 'School Main Entrance', 1, GETDATE(), GETDATE())
    
    PRINT '? Device added successfully'
END
ELSE
BEGIN
    -- Update LastPushTime to NOW
    UPDATE Device_Institution_Mapping 
    SET LastPushTime = GETDATE(),
        IsActive = 1
    WHERE DeviceSerialNumber = 'SMRS25200106'
    
    PRINT '? Device LastPushTime updated to NOW'
END

-- 4. Verify insertion
SELECT 
    DeviceSerialNumber,
    SchoolID,
    IsActive,
    LastPushTime,
    DATEDIFF(SECOND, LastPushTime, GETDATE()) AS SecondsSinceLastPush
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMRS25200106'
