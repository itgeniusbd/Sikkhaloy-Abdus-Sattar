-- Run this FIRST to see which index blocks multi-schedule employee attendance.
USE [Edu]
GO

SELECT i.name AS index_name,
       i.type_desc,
       i.is_primary_key,
       i.is_unique,
       i.is_unique_constraint,
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

-- How many schedules saved today? (use UI date: 2026-07-16)
SELECT Attendance_ScheduleID, COUNT(*) AS row_count
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
  AND CAST(AttendanceDate AS DATE) = '2026-07-16'
GROUP BY Attendance_ScheduleID
ORDER BY Attendance_ScheduleID;
GO
