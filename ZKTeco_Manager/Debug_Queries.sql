-- Check all attendance records in database (any date)
SELECT TOP 100
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    CASE 
        WHEN vw.FirstName IS NOT NULL 
        THEN vw.FirstName + ' ' + ISNULL(vw.LastName, '')
        ELSE 'Employee ID: ' + CAST(ear.EmployeeID AS VARCHAR(10))
    END AS EmployeeName,
    vw.DeviceID AS EmployeeDeviceID,
    ear.AttendanceDate,
    FORMAT(ear.AttendanceDate, 'yyyy-MM-dd') AS FormattedDate,
    CAST(ear.EntryTime AS VARCHAR(8)) AS EntryTime,
    CAST(ear.ExitTime AS VARCHAR(8)) AS ExitTime,
    ear.AttendanceStatus,
    ear.Is_OUT,
    DATEDIFF(day, ear.AttendanceDate, GETDATE()) AS DaysAgo
FROM Employee_Attendance_Record ear
LEFT JOIN VW_Emp_Info vw ON ear.EmployeeID = vw.EmployeeID
WHERE ear.SchoolID = 1012
ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC

-- Check today's records specifically
SELECT 
    COUNT(*) AS TodayRecordCount,
    MIN(AttendanceDate) AS FirstDate,
    MAX(AttendanceDate) AS LastDate,
    CAST(GETDATE() AS DATE) AS TodayDate
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
    AND CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)

-- Check last 7 days
SELECT 
    CAST(AttendanceDate AS DATE) AS Date,
    COUNT(*) AS RecordCount
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
    AND AttendanceDate >= DATEADD(day, -7, GETDATE())
GROUP BY CAST(AttendanceDate AS DATE)
ORDER BY Date DESC

-- Check Device_Institution_Mapping
SELECT * FROM Device_Institution_Mapping WHERE SchoolID = 1012
