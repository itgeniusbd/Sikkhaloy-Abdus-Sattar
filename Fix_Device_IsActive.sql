-- Fix: Set IsActive = 1 for the device
-- এই script run করো device activate করার জন্য

USE [Edu];
GO

-- Step 1: Check if device exists with serial SMR5252200106
IF EXISTS (SELECT 1 FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMR5252200106')
BEGIN
    -- Update existing device
    UPDATE Device_Institution_Mapping
    SET IsActive = 1,
        LastPushTime = GETDATE()
    WHERE DeviceSerialNumber = 'SMR5252200106';
    
    PRINT '✅ Device activated successfully';
END
ELSE
BEGIN
    -- Insert new device if not exists
    INSERT INTO Device_Institution_Mapping 
    (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation, IsActive)
    VALUES 
    (1012, 'SMR5252200106', 'Main Gate Device', 'School Main Entrance', 1);
    
    PRINT '✅ Device inserted and activated successfully';
END
GO

-- Step 2: Verify the update
PRINT '';
PRINT '--- Verification ---';
SELECT 
    MappingID,
    SchoolID,
    DeviceSerialNumber,
    DeviceName,
    DeviceLocation,
    IsActive,
    LastPushTime,
    CreatedDate
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMR5252200106';
GO
