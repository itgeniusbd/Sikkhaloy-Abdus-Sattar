/*
    School 1012 — find why Employee_Attendance_Record insert fails.
    Run on Edu SQL Server (LOOPS-IT-VM-1).
*/

USE [Edu];
GO

DECLARE @SchoolID INT = 1012;

-- 1) Which INSTEAD OF INSERT triggers exist on this table?
SELECT
    t.name AS TriggerName,
    t.is_disabled,
    t.create_date,
    t.modify_date
FROM sys.triggers t
WHERE t.parent_id = OBJECT_ID(N'dbo.Employee_Attendance_Record')
ORDER BY t.name;

-- 2) Device 212 on server?
SELECT EmployeeID, ID, FirstName, LastName, DeviceID, Job_Status
FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID AND DeviceID = 212;

-- 3) Today count before test
SELECT COUNT(*) AS TodayCount
FROM dbo.Employee_Attendance_Record
WHERE SchoolID = @SchoolID AND AttendanceDate = CAST(GETDATE() AS DATE);

-- 4) Direct insert test (bypasses API — tests trigger only)
DECLARE @EmployeeID INT =
(
    SELECT TOP 1 EmployeeID
    FROM dbo.VW_Emp_Info
    WHERE SchoolID = @SchoolID AND DeviceID = 212
);

IF @EmployeeID IS NULL
BEGIN
    PRINT 'DeviceID 212 not found in VW_Emp_Info — fix Employee_Info.DeviceID first.';
END
ELSE IF EXISTS (
    SELECT 1
    FROM dbo.Employee_Attendance_Record
    WHERE SchoolID = @SchoolID
      AND EmployeeID = @EmployeeID
      AND AttendanceDate = CAST(GETDATE() AS DATE)
      AND Attendance_ScheduleID = 2793
)
BEGIN
    PRINT 'Test row already exists for schedule 2793 today — skip insert test.';
END
ELSE
BEGIN
    BEGIN TRY
        INSERT INTO dbo.Employee_Attendance_Record
        (
            SchoolID, RegistrationID, EmployeeID, Attendance_ScheduleID,
            AttendanceStatus, AttendanceDate, EntryTime, ExitTime,
            ExitStatus, Is_OUT, IsFromDevice
        )
        VALUES
        (
            @SchoolID, 0, @EmployeeID, 2793,
            'Late Abs', CAST(GETDATE() AS DATE), '21:17:00', '21:54:00',
            'Out', 1, 1
        );

        PRINT 'Direct INSERT succeeded — trigger OK. Problem is likely Attendance_API not published.';
    END TRY
    BEGIN CATCH
        PRINT 'Direct INSERT FAILED:';
        PRINT ERROR_MESSAGE();
    END CATCH
END

-- 5) After test
SELECT
    ear.AttendanceStatus,
    ear.EntryTime,
    ear.ExitTime,
    ear.Is_OUT,
    ear.Attendance_ScheduleID
FROM dbo.Employee_Attendance_Record ear
INNER JOIN dbo.VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID
WHERE ear.SchoolID = @SchoolID
  AND ear.AttendanceDate = CAST(GETDATE() AS DATE)
  AND e.DeviceID = 212;
