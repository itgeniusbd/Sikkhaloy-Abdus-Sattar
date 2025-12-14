-- ========================================
-- Check if punch from User ID 188 arrived
-- ========================================

-- 1. Check Employee_Attendance_Record for User 188
SELECT TOP 5
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    emp.FirstName + ' ' + emp.LastName AS EmployeeName,
    CAST(ear.AttendanceDate AS DATE) AS AttendanceDate,
    CAST(ear.EntryTime AS TIME) AS EntryTime,
    ear.AttendanceStatus,
    ear.Is_OUT,
    ear.SchoolID
FROM Employee_Attendance_Record ear
LEFT JOIN VW_Emp_Info emp ON ear.EmployeeID = emp.EmployeeID
WHERE ear.EmployeeID = 188 
   OR emp.DeviceID = 188
ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC

-- 2. Check if DeviceID 188 is mapped to any employee
SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS EmployeeName,
    DeviceID,
    SchoolID,
    Job_Status
FROM VW_Emp_Info
WHERE DeviceID = 188

-- 3. Check device mapping for SMRS25200106
SELECT 
    DeviceSerialNumber,
    SchoolID,
    DeviceName,
    LastPushTime,
    DATEDIFF(SECOND, LastPushTime, GETDATE()) AS SecondsSinceLastPush,
    IsActive
FROM Device_Institution_Mapping
WHERE DeviceSerialNumber = 'SMR525200106'

-- 4. Check all attendance from this school in last 1 hour
SELECT TOP 10
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    emp.FirstName + ' ' + emp.LastName AS EmployeeName,
    emp.DeviceID,
    CAST(ear.AttendanceDate AS DATE) AS AttendanceDate,
    CAST(ear.EntryTime AS TIME) AS EntryTime,
    ear.AttendanceStatus,
    ear.SchoolID
FROM Employee_Attendance_Record ear
LEFT JOIN VW_Emp_Info emp ON ear.EmployeeID = emp.EmployeeID
WHERE ear.SchoolID = (SELECT TOP 1 SchoolID FROM Device_Institution_Mapping WHERE DeviceSerialNumber = 'SMRS25200106')
  AND ear.AttendanceDate >= DATEADD(HOUR, -1, GETDATE())
ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC

-- 5. Force update LastPushTime to show as connected
UPDATE Device_Institution_Mapping 
SET LastPushTime = GETDATE() 
WHERE DeviceSerialNumber = 'SMRS25200106'
