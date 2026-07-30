-- School 1012: find employee attendance on ANY date (date mismatch check)
DECLARE @SchoolID INT = 1012;

SELECT GETDATE() AS ServerNow;

SELECT AttendanceDate, COUNT(*) AS Cnt
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID
GROUP BY AttendanceDate
ORDER BY AttendanceDate DESC;

-- DeviceID 261 = MD SHAJALAL (API sync uses VW_Emp_Info, not Employee_Info.Name)
SELECT
    EmployeeID,
    ID,
    FirstName,
    LastName,
    DeviceID,
    Job_Status
FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID AND DeviceID = 261;

-- Today only
SELECT COUNT(*) AS Today_Employee_Records
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = CAST(GETDATE() AS DATE);

SELECT TOP 20
    ear.AttendanceDate,
    v.DeviceID,
    v.FirstName + ' ' + v.LastName AS EmployeeName,
    ear.AttendanceStatus,
    ear.EntryTime,
    ear.Attendance_ScheduleID,
    ear.Is_OUT
FROM dbo.Employee_Attendance_Record ear
INNER JOIN dbo.VW_Emp_Info v ON ear.EmployeeID = v.EmployeeID AND ear.SchoolID = v.SchoolID
WHERE ear.SchoolID = @SchoolID
  AND ear.AttendanceDate = CAST(GETDATE() AS DATE)
ORDER BY ear.EntryTime DESC;
