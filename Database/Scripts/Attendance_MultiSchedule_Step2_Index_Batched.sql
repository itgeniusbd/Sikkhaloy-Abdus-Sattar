-- Step 2 (batched): Back-fill + dedupe + unique index.
-- Run during off-peak. Disable triggers first to avoid SMS firing on every row.
-- Estimated time: 30-90 minutes on ~8M rows (with triggers disabled).

USE [Edu]
GO

SET NOCOUNT ON;
GO

PRINT '=== Step 2 started at ' + CONVERT(varchar(30), GETDATE(), 120) + ' ===';
GO

-- 1. Disable triggers (critical — prevents "SMS Student setting off" per row)
DISABLE TRIGGER ALL ON [dbo].[Attendance_Record];
PRINT 'Triggers disabled on Attendance_Record.';
GO

-- 2. Back-fill Attendance_ScheduleID in batches
DECLARE @batch INT = 500000;
DECLARE @rows INT = 1;
DECLARE @total INT = 0;

WHILE @rows > 0
BEGIN
    UPDATE TOP (@batch) ar
    SET ar.Attendance_ScheduleID = ass.ScheduleID
    FROM [dbo].[Attendance_Record] ar
    INNER JOIN [dbo].[Attendance_Schedule_AssignStudent] ass
        ON ar.StudentID = ass.StudentID
        AND ar.SchoolID = ass.SchoolID
    WHERE ar.Attendance_ScheduleID IS NULL
      AND ass.ScheduleID IS NOT NULL;

    SET @rows = @@ROWCOUNT;
    SET @total = @total + @rows;

    IF @rows > 0
    BEGIN
        PRINT 'Back-filled ' + CAST(@rows AS varchar(20)) + ' rows (total so far: ' + CAST(@total AS varchar(20)) + ') at ' + CONVERT(varchar(30), GETDATE(), 120);
        WAITFOR DELAY '00:00:02';
    END
END

PRINT 'Back-fill complete. Total rows updated: ' + CAST(@total AS varchar(20));
GO

-- 3. Remove duplicates for new unique index
;WITH CTE AS (
    SELECT AttendanceRecordID,
           ROW_NUMBER() OVER (
               PARTITION BY SchoolID, StudentID, AttendanceDateKey, ISNULL(Attendance_ScheduleID, 0)
               ORDER BY AttendanceRecordID DESC
           ) AS RowNum
    FROM [dbo].[Attendance_Record]
)
DELETE FROM CTE WHERE RowNum > 1;

PRINT 'Removed ' + CAST(@@ROWCOUNT AS varchar(20)) + ' duplicate Attendance_Record rows.';
GO

-- 4. Drop old unique index if it exists
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('[dbo].[Attendance_Record]')
      AND name = 'UQ_Attendance_Record_Student_Date'
)
BEGIN
    DROP INDEX UQ_Attendance_Record_Student_Date ON [dbo].[Attendance_Record];
    PRINT 'Dropped old unique index UQ_Attendance_Record_Student_Date.';
END
GO

-- 5. Drop new index if partial run left it
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('[dbo].[Attendance_Record]')
      AND name = 'UQ_Attendance_Record_Student_Date_Schedule'
)
BEGIN
    DROP INDEX UQ_Attendance_Record_Student_Date_Schedule ON [dbo].[Attendance_Record];
    PRINT 'Dropped existing UQ_Attendance_Record_Student_Date_Schedule.';
END
GO

-- 6. Create unique index (may take 20-60 minutes — do NOT cancel)
PRINT 'Creating unique index... started at ' + CONVERT(varchar(30), GETDATE(), 120);
GO

CREATE UNIQUE INDEX UQ_Attendance_Record_Student_Date_Schedule
ON [dbo].[Attendance_Record] (SchoolID, StudentID, AttendanceDateKey, Attendance_ScheduleID);

PRINT 'Created unique index UQ_Attendance_Record_Student_Date_Schedule at ' + CONVERT(varchar(30), GETDATE(), 120);
GO

-- 7. Re-enable triggers (Step 4 will replace Tr_Attendance_Record_SMS)
ENABLE TRIGGER ALL ON [dbo].[Attendance_Record];
PRINT 'Triggers re-enabled on Attendance_Record.';
GO

-- 8. Verification
SELECT
    COUNT(*) AS total,
    SUM(CASE WHEN Attendance_ScheduleID IS NOT NULL THEN 1 ELSE 0 END) AS filled,
    SUM(CASE WHEN Attendance_ScheduleID IS NULL THEN 1 ELSE 0 END) AS still_null
FROM [dbo].[Attendance_Record];

SELECT name, is_unique
FROM sys.indexes
WHERE object_id = OBJECT_ID('[dbo].[Attendance_Record]')
  AND is_unique = 1;

PRINT '=== Step 2 finished at ' + CONVERT(varchar(30), GETDATE(), 120) + ' ===';
GO
