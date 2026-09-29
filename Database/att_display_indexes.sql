/* =====================================================================================
   Attendance display (Online_Display/Attendance_Slider.aspx, DeviceDisplay.aspx,
   Hybrid /attendance/display) - supporting indexes.            2026-09-29
   -------------------------------------------------------------------------------------
   WHY: every display query filters "this school, today" but no index on
        Attendance_Record / Employee_Attendance_Record leads with (SchoolID, AttendanceDate),
        so each query read the school's whole attendance history (100k - 59M logical reads).
        VW_Emp_Info joins Teacher / Staff_Info on EmployeeID with no index (full scans,
        also on every Employee_Image.ashx photo request).
   SAFE: CREATE INDEX only (no data change). Idempotent (skips existing). ONLINE=ON is
        used automatically on Enterprise/Developer/Azure so the tables stay usable.
   RUN : off-peak, in SSMS or: sqlcmd -S <server> -d <db> -E -I -i att_display_indexes.sql
        (-I = QUOTED_IDENTIFIER ON, required because of filtered indexes on these tables).
        Largest one (Attendance_Record, ~8.7M rows) needs roughly 350-500 MB and a few minutes.
   ROLLBACK: see the commented section at the end.
   ===================================================================================== */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @online nvarchar(40) =
    CASE WHEN CAST(SERVERPROPERTY('EngineEdition') AS int) IN (3, 5, 8) THEN N'ONLINE = ON, ' ELSE N'' END;
DECLARE @with nvarchar(200) = N' WITH (' + @online + N'SORT_IN_TEMPDB = ON)';
PRINT 'Index options:' + @with;

DECLARE @ix TABLE (seq int, tbl sysname, name sysname, ddl nvarchar(max));
INSERT @ix VALUES
(1, N'Attendance_Record', N'IX_Attendance_Record_School_Date',
    N'CREATE NONCLUSTERED INDEX IX_Attendance_Record_School_Date ON dbo.Attendance_Record (SchoolID, AttendanceDate) INCLUDE (StudentID, Attendance_ScheduleID, Attendance, EntryTime, ExitTime, Is_OUT, RegistrationID)'),
(2, N'Employee_Attendance_Record', N'IX_Employee_Attendance_Record_School_Date',
    N'CREATE NONCLUSTERED INDEX IX_Employee_Attendance_Record_School_Date ON dbo.Employee_Attendance_Record (SchoolID, AttendanceDate) INCLUDE (EmployeeID, Attendance_ScheduleID, AttendanceStatus, EntryTime, ExitTime, Is_OUT, RegistrationID)'),
(3, N'Attendance_Schedule_AssignStudent', N'IX_Attendance_Schedule_AssignStudent_School_Schedule',
    N'CREATE NONCLUSTERED INDEX IX_Attendance_Schedule_AssignStudent_School_Schedule ON dbo.Attendance_Schedule_AssignStudent (SchoolID, ScheduleID) INCLUDE (StudentID)'),
(4, N'Employee_Attendance_Schedule_Assign', N'IX_Employee_Attendance_Schedule_Assign_School_Schedule',
    N'CREATE NONCLUSTERED INDEX IX_Employee_Attendance_Schedule_Assign_School_Schedule ON dbo.Employee_Attendance_Schedule_Assign (SchoolID, ScheduleID) INCLUDE (EmployeeID)'),
(5, N'Teacher', N'IX_Teacher_EmployeeID',
    N'CREATE NONCLUSTERED INDEX IX_Teacher_EmployeeID ON dbo.Teacher (EmployeeID)'),
(6, N'Staff_Info', N'IX_Staff_Info_EmployeeID',
    N'CREATE NONCLUSTERED INDEX IX_Staff_Info_EmployeeID ON dbo.Staff_Info (EmployeeID)'),
(7, N'StudentsClass', N'IX_StudentsClass_School_Year_Student',
    N'CREATE NONCLUSTERED INDEX IX_StudentsClass_School_Year_Student ON dbo.StudentsClass (SchoolID, EducationYearID, StudentID) INCLUDE (ClassID)'),
(8, N'Attendance_Leave', N'IX_Attendance_Leave_School_Student',
    N'CREATE NONCLUSTERED INDEX IX_Attendance_Leave_School_Student ON dbo.Attendance_Leave (SchoolID, StudentID) INCLUDE (StartDate, EndDate)');

DECLARE @seq int = 0, @tbl sysname, @name sysname, @ddl nvarchar(max), @t0 datetime2;
WHILE 1 = 1
BEGIN
    SELECT TOP 1 @seq = seq, @tbl = tbl, @name = name, @ddl = ddl FROM @ix WHERE seq > @seq ORDER BY seq;
    IF @@ROWCOUNT = 0 BREAK;
    IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NULL
    BEGIN
        PRINT 'SKIP  ' + @name + ' (table dbo.' + @tbl + ' not found)';
        CONTINUE;
    END
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.' + @tbl) AND name = @name)
    BEGIN
        PRINT 'EXISTS ' + @name;
        CONTINUE;
    END
    SET @t0 = SYSDATETIME();
    BEGIN TRY
        SET @ddl = @ddl + @with;
        EXEC sys.sp_executesql @ddl;
        PRINT 'CREATED ' + @name + ' in ' + CAST(DATEDIFF(ms, @t0, SYSDATETIME()) AS varchar(20)) + ' ms';
    END TRY
    BEGIN CATCH
        PRINT 'FAILED ' + @name + ': ' + ERROR_MESSAGE();
    END CATCH
END

/* Verify */
SELECT t.name AS TableName, i.name AS IndexName,
       CAST(SUM(ps.used_page_count) * 8 / 1024.0 AS decimal(10,1)) AS SizeMB
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.dm_db_partition_stats ps ON ps.object_id = i.object_id AND ps.index_id = i.index_id
WHERE i.name IN (SELECT name FROM @ix)
GROUP BY t.name, i.name
ORDER BY t.name, i.name;
GO

/* ------------------------------ ROLLBACK (only if needed) ------------------------------
DROP INDEX IF EXISTS IX_Attendance_Record_School_Date ON dbo.Attendance_Record;
DROP INDEX IF EXISTS IX_Employee_Attendance_Record_School_Date ON dbo.Employee_Attendance_Record;
DROP INDEX IF EXISTS IX_Attendance_Schedule_AssignStudent_School_Schedule ON dbo.Attendance_Schedule_AssignStudent;
DROP INDEX IF EXISTS IX_Employee_Attendance_Schedule_Assign_School_Schedule ON dbo.Employee_Attendance_Schedule_Assign;
DROP INDEX IF EXISTS IX_Teacher_EmployeeID ON dbo.Teacher;
DROP INDEX IF EXISTS IX_Staff_Info_EmployeeID ON dbo.Staff_Info;
DROP INDEX IF EXISTS IX_StudentsClass_School_Year_Student ON dbo.StudentsClass;
DROP INDEX IF EXISTS IX_Attendance_Leave_School_Student ON dbo.Attendance_Leave;
--------------------------------------------------------------------------------------- */
