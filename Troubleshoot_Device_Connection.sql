-- Device Connection Troubleshooting Steps
-- Check device configuration and connectivity

-- 1. Check if device exists in mapping table
SELECT 
    'Device Mapping Status' AS CheckType,
    MappingID,
    SchoolID,
    DeviceSerialNumber,
    DeviceName,
    DeviceLocation,
    IsActive,
    LastPushTime,
    CASE 
        WHEN LastPushTime IS NULL THEN 'Never Connected'
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 5 THEN 'Connected (Active)'
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 60 THEN 'Recently Disconnected'
        ELSE 'Disconnected (Inactive)'
    END AS ConnectionStatus,
    DATEDIFF(MINUTE, LastPushTime, GETDATE()) AS MinutesSinceLastPush
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMRS25200106' -- Replace with your device serial
ORDER BY LastPushTime DESC

-- 2. Check if SchoolID exists
SELECT 'School Check' AS CheckType, 
       SchoolID, 
       SchoolName, 
       Phone 
FROM SchoolInfo 
WHERE SchoolID = 1012 -- Replace with your SchoolID

-- 3. Check if there are any attendance records today
SELECT 'Today Attendance' AS CheckType,
       COUNT(*) AS TotalRecords,
       MIN(AttendanceDate) AS FirstRecord,
       MAX(AttendanceDate) AS LastRecord
FROM Employee_Attendance_Record
WHERE CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)

-- 4. Check recent device activity in logs (if you have log table)
-- This checks the last 10 activities
SELECT TOP 10 
    'Recent Activity' AS CheckType,
    *
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
ORDER BY AttendanceDate DESC, EntryTime DESC

GO

-- Quick Fix Queries

-- Fix 1: Reset LastPushTime to force device to connect
-- UPDATE Device_Institution_Mapping 
-- SET LastPushTime = NULL 
-- WHERE DeviceSerialNumber = 'YOUR_SERIAL'

-- Fix 2: Activate device if inactive
-- UPDATE Device_Institution_Mapping 
-- SET IsActive = 1 
-- WHERE DeviceSerialNumber = 'YOUR_SERIAL'

-- Fix 3: Check device push settings
PRINT '========================================='
PRINT 'DEVICE PUSH CONFIGURATION REQUIRED:'
PRINT '========================================='
PRINT 'Server Address: pushapi.sikkhaloy.com'
PRINT 'Server Port: 443 (HTTPS) or 80 (HTTP)'
PRINT 'Push URL: /iclock/cdata'
PRINT 'Protocol: PUSH'
PRINT '========================================='
