-- ========================================
-- Check Device Connection Status RIGHT NOW
-- ========================================

-- 1. Check device LastPushTime
SELECT 
    DeviceSerialNumber,
    DeviceName,
    SchoolID,
    LastPushTime,
    DATEDIFF(MINUTE, LastPushTime, GETDATE()) AS MinutesSinceLastPush,
    CASE 
        WHEN LastPushTime IS NULL THEN 'Never Connected'
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 5 THEN '?? Connected (5min)'
        WHEN DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 30 THEN '?? Recent (30min)'
        ELSE '?? Disconnected'
    END AS ConnectionStatus,
    IsActive
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMRS25200106'

-- 2. Manually update LastPushTime to NOW (for testing)
-- Uncomment this line to force update:
-- UPDATE Device_Institution_Mapping SET LastPushTime = GETDATE() WHERE DeviceSerialNumber = 'SMRS25200106'

-- 3. Check recent attendance from this device
SELECT TOP 5
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    CAST(ear.AttendanceDate AS DATE) AS AttendanceDate,
    CAST(ear.EntryTime AS TIME) AS EntryTime,
    ear.AttendanceStatus,
    ear.Is_OUT
FROM Employee_Attendance_Record ear
WHERE ear.SchoolID = (SELECT SchoolID FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMRS25200106')
ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC
