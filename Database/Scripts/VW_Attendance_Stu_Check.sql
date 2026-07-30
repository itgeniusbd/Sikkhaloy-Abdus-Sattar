-- Quick check: student attendance view name is VW_Attendance_Stu (no trailing "s").
-- API was querying VW_Attendance_Stus by mistake.

USE [Edu]
GO

SELECT TOP 5 *
FROM VW_Attendance_Stu
WHERE SchoolID = 1012;
GO
