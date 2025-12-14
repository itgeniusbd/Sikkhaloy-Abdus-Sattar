-- Employee Attendance Data Check ???? ???? Query
-- ?? script run ??? ???? data ????? ????

USE [Edu];
GO

PRINT '====================================================';
PRINT 'Employee Attendance Monitoring Script';
PRINT 'Ali Akbar Academy (SchoolID: 1012)';
PRINT '====================================================';
PRINT '';

-- ========================================
-- Part 1: ????? ?? Employee Attendance
-- ========================================
PRINT '========================================';
PRINT '????? ?? Employee Attendance Records';
PRINT '========================================';
PRINT '';

SELECT 
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    e.Name AS EmployeeName,
    e.EmployeeCode,
    e.DeviceID AS EmployeeDeviceID,
    ear.AttendanceDate,
    CONVERT(VARCHAR(8), ear.EntryTime, 108) AS EntryTime,
    CONVERT(VARCHAR(8), ear.ExitTime, 108) AS ExitTime,
    ear.AttendanceStatus,
    ear.ExitStatus,
    CASE WHEN ear.Is_OUT = 1 THEN 'Yes' ELSE 'No' END AS Is_OUT,
    s.SchoolName
FROM Employee_Attendance_Record ear
INNER JOIN Employee e ON ear.EmployeeID = e.EmployeeID
INNER JOIN SchoolInfo s ON ear.SchoolID = s.SchoolID
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012
ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC;

PRINT '';
PRINT '========================================';
PRINT '??????? 20 ?? Employee Attendance (?? school)';
PRINT '========================================';
PRINT '';

-- ========================================
-- Part 2: ??????? 20 ?? Records (?????? date)
-- ========================================
SELECT TOP 20
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    e.Name AS EmployeeName,
    e.EmployeeCode,
    e.DeviceID AS EmployeeDeviceID,
    ear.AttendanceDate,
    CONVERT(VARCHAR(8), ear.EntryTime, 108) AS EntryTime,
    CONVERT(VARCHAR(8), ear.ExitTime, 108) AS ExitTime,
    ear.AttendanceStatus,
    ear.ExitStatus,
    CASE WHEN ear.Is_OUT = 1 THEN 'Yes' ELSE 'No' END AS Is_OUT,
    s.SchoolName
FROM Employee_Attendance_Record ear
INNER JOIN Employee e ON ear.EmployeeID = e.EmployeeID
INNER JOIN SchoolInfo s ON ear.SchoolID = s.SchoolID
ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC;

PRINT '';
PRINT '========================================';
PRINT 'Summary Report - ????? Statistics';
PRINT '========================================';
PRINT '';

-- ========================================
-- Part 3: Summary Statistics
-- ========================================
SELECT 
    'Total Records Today' AS Description,
    COUNT(*) AS Count
FROM Employee_Attendance_Record ear
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012

UNION ALL

SELECT 
    'Unique Employees' AS Description,
    COUNT(DISTINCT ear.EmployeeID) AS Count
FROM Employee_Attendance_Record ear
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012

UNION ALL

SELECT 
    'Entry Records (Not Out)' AS Description,
    SUM(CASE WHEN ear.Is_OUT = 0 THEN 1 ELSE 0 END) AS Count
FROM Employee_Attendance_Record ear
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012

UNION ALL

SELECT 
    'Exit Records (Out)' AS Description,
    SUM(CASE WHEN ear.Is_OUT = 1 THEN 1 ELSE 0 END) AS Count
FROM Employee_Attendance_Record ear
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012

UNION ALL

SELECT 
    'Present' AS Description,
    COUNT(*) AS Count
FROM Employee_Attendance_Record ear
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012
    AND ear.AttendanceStatus = 'Pre'

UNION ALL

SELECT 
    'Late' AS Description,
    COUNT(*) AS Count
FROM Employee_Attendance_Record ear
WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
    AND ear.SchoolID = 1012
    AND ear.AttendanceStatus = 'Late';

PRINT '';
PRINT '====================================================';
PRINT 'Script Completed Successfully!';
PRINT '====================================================';

GO
