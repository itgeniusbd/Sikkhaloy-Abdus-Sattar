/*
================================================================================
  Enable ALL user-table triggers in current database (Edu)
  Use after sp_ResetInstitutionData hang / server restart left triggers DISABLED.

  Safe to re-run. Does not change data — only ENABLE TRIGGER.
================================================================================
*/
USE [Edu];
GO

SET NOCOUNT ON;

PRINT '=== BEFORE: disabled triggers ===';
SELECT
    OBJECT_SCHEMA_NAME(t.parent_id) AS SchemaName,
    OBJECT_NAME(t.parent_id) AS TableName,
    t.name AS TriggerName,
    t.is_disabled
FROM sys.triggers AS t
WHERE t.parent_class_desc = N'OBJECT_OR_COLUMN'
  AND t.is_ms_shipped = 0
  AND t.is_disabled = 1
ORDER BY TableName, TriggerName;

DECLARE @sql nvarchar(max) = N'';

SELECT @sql = @sql
    + N'ENABLE TRIGGER ' + QUOTENAME(t.name)
    + N' ON ' + QUOTENAME(OBJECT_SCHEMA_NAME(t.parent_id))
    + N'.' + QUOTENAME(OBJECT_NAME(t.parent_id)) + N';' + CHAR(13)
FROM sys.triggers AS t
WHERE t.parent_class_desc = N'OBJECT_OR_COLUMN'
  AND t.is_ms_shipped = 0
  AND t.is_disabled = 1;

IF LEN(@sql) = 0
BEGIN
    PRINT 'No disabled triggers found. All OK.';
END
ELSE
BEGIN
    PRINT 'Enabling disabled triggers...';
    PRINT @sql;
    EXEC sys.sp_executesql @sql;
END

PRINT '=== AFTER: any still disabled? (should be empty) ===';
SELECT
    OBJECT_NAME(t.parent_id) AS TableName,
    t.name AS TriggerName,
    t.is_disabled
FROM sys.triggers AS t
WHERE t.parent_class_desc = N'OBJECT_OR_COLUMN'
  AND t.is_ms_shipped = 0
  AND t.is_disabled = 1
ORDER BY TableName, TriggerName;

SELECT
    COUNT(*) AS TotalTriggers,
    SUM(CASE WHEN t.is_disabled = 1 THEN 1 ELSE 0 END) AS StillDisabled
FROM sys.triggers AS t
WHERE t.parent_class_desc = N'OBJECT_OR_COLUMN'
  AND t.is_ms_shipped = 0;

PRINT '=== DONE ===';
GO
