-- Employee attendance diagnostic for multi-schedule (School 1012).
-- IMPORTANT: Web page date is usually 2026 (session year), NOT 2024.

USE [Edu]
GO

DECLARE @SchoolID INT = 1012;

PRINT '=== Schedules for school ===';
SELECT ScheduleID, ScheduleName
FROM Attendance_Schedule
WHERE SchoolID = @SchoolID
ORDER BY ScheduleID;
GO

PRINT '=== UI date: 2026-07-16 (use this, not 2024) ===';
SELECT Attendance_ScheduleID, COUNT(*) AS row_count
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
  AND CAST(AttendanceDate AS DATE) = '2026-07-16'
GROUP BY Attendance_ScheduleID
ORDER BY Attendance_ScheduleID;
GO

PRINT '=== Night schedule on UI date (2026-07-16) ===';
SELECT ear.EmployeeID, ear.Attendance_ScheduleID, ear.AttendanceStatus, ear.AttendanceDate, ear.CreatedDate
FROM Employee_Attendance_Record ear
WHERE ear.SchoolID = 1012
  AND CAST(ear.AttendanceDate AS DATE) = '2026-07-16'
  AND ear.Attendance_ScheduleID = (
      SELECT ScheduleID FROM Attendance_Schedule
      WHERE SchoolID = 1012 AND ScheduleName = N'নাইট'
  )
ORDER BY ear.EmployeeID;
GO

PRINT '=== Latest 20 employee attendance rows (any date) ===';
SELECT TOP 20
    Employee_Attendance_RecordID,
    EmployeeID,
    CAST(AttendanceDate AS DATE) AS AttDate,
    Attendance_ScheduleID,
    AttendanceStatus,
    CreatedDate
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
ORDER BY Employee_Attendance_RecordID DESC;
GO

PRINT '=== Indexes on Employee_Attendance_Record ===';
SELECT i.name, i.is_primary_key, i.is_unique,
       STUFF((
           SELECT ', ' + c.name
           FROM sys.index_columns ic
           INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
           WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
           ORDER BY ic.key_ordinal
           FOR XML PATH(''), TYPE
       ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS key_columns
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'[dbo].[Employee_Attendance_Record]')
  AND i.index_id > 0
ORDER BY i.is_primary_key DESC, i.name;
GO

PRINT '=== Triggers on Employee_Attendance_Record ===';
SELECT t.name AS trigger_name, t.is_disabled
FROM sys.triggers t
WHERE t.parent_id = OBJECT_ID(N'[dbo].[Employee_Attendance_Record]');
GO
