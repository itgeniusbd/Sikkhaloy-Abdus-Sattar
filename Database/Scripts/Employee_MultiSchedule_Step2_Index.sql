-- Step 2: Allow multiple employee attendance rows per day (one per schedule).
-- Run AFTER Employee_MultiSchedule_Step1_Column.sql
-- Safe to re-run: skips steps already done.

USE [Edu]
GO

SET NOCOUNT ON;
GO

-- Back-fill single-schedule employees first
UPDATE ear
SET ear.Attendance_ScheduleID = ea.ScheduleID
FROM [dbo].[Employee_Attendance_Record] ear
INNER JOIN (
    SELECT SchoolID, EmployeeID, MIN(ScheduleID) AS ScheduleID
    FROM [dbo].[Employee_Attendance_Schedule_Assign]
    GROUP BY SchoolID, EmployeeID
    HAVING COUNT(DISTINCT ScheduleID) = 1
) ea
    ON ear.EmployeeID = ea.EmployeeID
   AND ear.SchoolID = ea.SchoolID
WHERE ear.Attendance_ScheduleID IS NULL;

PRINT 'Back-filled Attendance_ScheduleID for single-schedule employees. Rows: ' + CAST(@@ROWCOUNT AS VARCHAR(20));
GO

-- Remove duplicates that would block the new unique index
;WITH CTE AS (
    SELECT Employee_Attendance_RecordID,
           ROW_NUMBER() OVER (
               PARTITION BY SchoolID, EmployeeID, CAST(AttendanceDate AS DATE), ISNULL(Attendance_ScheduleID, 0)
               ORDER BY Employee_Attendance_RecordID DESC
           ) AS RowNum
    FROM [dbo].[Employee_Attendance_Record]
)
DELETE FROM [dbo].[Employee_Attendance_Record]
WHERE Employee_Attendance_RecordID IN (SELECT Employee_Attendance_RecordID FROM CTE WHERE RowNum > 1);

PRINT 'Removed duplicate Employee_Attendance_Record rows. Rows: ' + CAST(@@ROWCOUNT AS VARCHAR(20));
GO

-- Show indexes/constraints BEFORE change (for log)
PRINT '--- Indexes on Employee_Attendance_Record (before) ---';
SELECT i.name AS index_name,
       i.is_primary_key,
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

-- Drop UNIQUE constraints (NOT primary key) that do not include Attendance_ScheduleID
DECLARE @dropConstraintSql NVARCHAR(MAX) = N'';

SELECT @dropConstraintSql = @dropConstraintSql
    + N'ALTER TABLE [dbo].[Employee_Attendance_Record] DROP CONSTRAINT ' + QUOTENAME(kc.name) + N';' + CHAR(13)
FROM sys.key_constraints kc
WHERE kc.parent_object_id = OBJECT_ID(N'[dbo].[Employee_Attendance_Record]')
  AND kc.type = 'UQ'
  AND NOT EXISTS (
      SELECT 1
      FROM sys.index_columns ic
      INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
      INNER JOIN sys.indexes ix ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
      WHERE ix.object_id = kc.parent_object_id
        AND ix.name = kc.name
        AND c.name = 'Attendance_ScheduleID'
  );

IF LEN(@dropConstraintSql) > 0
BEGIN
    PRINT 'Dropping old UNIQUE constraints (not PK):';
    PRINT @dropConstraintSql;
    EXEC sp_executesql @dropConstraintSql;
END
ELSE
    PRINT 'No old UNIQUE constraint found without Attendance_ScheduleID.';
GO

-- Drop non-PK unique indexes that do not include Attendance_ScheduleID
DECLARE @dropIndexSql NVARCHAR(MAX) = N'';

SELECT @dropIndexSql = @dropIndexSql
    + N'DROP INDEX ' + QUOTENAME(i.name) + N' ON [dbo].[Employee_Attendance_Record];' + CHAR(13)
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'[dbo].[Employee_Attendance_Record]')
  AND i.is_unique = 1
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0
  AND i.name IS NOT NULL
  AND i.name <> 'UQ_Employee_Attendance_Record_Employee_Date_Schedule'
  AND NOT EXISTS (
      SELECT 1
      FROM sys.index_columns ic
      INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
      WHERE ic.object_id = i.object_id
        AND ic.index_id = i.index_id
        AND c.name = 'Attendance_ScheduleID'
  );

IF LEN(@dropIndexSql) > 0
BEGIN
    PRINT 'Dropping old unique indexes (not PK):';
    PRINT @dropIndexSql;
    EXEC sp_executesql @dropIndexSql;
END
ELSE
    PRINT 'No old unique index found without Attendance_ScheduleID.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[dbo].[Employee_Attendance_Record]')
      AND name = 'UQ_Employee_Attendance_Record_Employee_Date_Schedule'
)
BEGIN
    SET QUOTED_IDENTIFIER ON;

    CREATE UNIQUE INDEX UQ_Employee_Attendance_Record_Employee_Date_Schedule
    ON [dbo].[Employee_Attendance_Record] (SchoolID, EmployeeID, AttendanceDate, Attendance_ScheduleID)
    WHERE Attendance_ScheduleID IS NOT NULL;

    PRINT 'Created filtered unique index UQ_Employee_Attendance_Record_Employee_Date_Schedule.';
END
ELSE
    PRINT 'Index UQ_Employee_Attendance_Record_Employee_Date_Schedule already exists.';
GO

PRINT '--- Verify schedule counts for School 1012 on 2026-07-16 ---';
SELECT Attendance_ScheduleID, COUNT(*) AS row_count
FROM Employee_Attendance_Record
WHERE SchoolID = 1012
  AND CAST(AttendanceDate AS DATE) = '2026-07-16'
GROUP BY Attendance_ScheduleID
ORDER BY Attendance_ScheduleID;

GO
