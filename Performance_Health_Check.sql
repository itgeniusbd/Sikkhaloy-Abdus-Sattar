-- =========================================
-- Performance Health Check
-- ?????? ???? ??????? ????? system ??? ??? ????
-- =========================================

USE Edu
GO

PRINT '========================================='
PRINT 'SIKKHALOY Performance Health Check'
PRINT 'Time: ' + CONVERT(VARCHAR(20), GETDATE(), 120)
PRINT '========================================='
PRINT ''

-- 1. Check Index Status
PRINT '1. INDEX STATUS CHECK:'
PRINT '-------------------'
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    CASE 
        WHEN i.name IS NOT NULL THEN '? Present'
        ELSE '? Missing'
    END AS Status
FROM sys.tables t
LEFT JOIN sys.indexes i ON t.object_id = i.object_id AND i.name LIKE 'IX_%Performance%'
WHERE t.name IN ('Exam_Result_of_Student', 'StudentsClass', 'Student', 
                 'Exam_Result_of_Subject', 'Attendance_Student', 
                 'Exam_Obtain_Marks', 'Subject')
ORDER BY t.name
GO

PRINT ''
PRINT '2. STATISTICS AGE CHECK:'
PRINT '-------------------'

-- 2. Check Statistics Age
SELECT 
    OBJECT_NAME(s.object_id) AS TableName,
    s.name AS StatisticsName,
    STATS_DATE(s.object_id, s.stats_id) AS LastUpdated,
    DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) AS DaysOld,
    CASE 
        WHEN DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) < 30 THEN '? Fresh'
        WHEN DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) < 60 THEN '? OK'
        ELSE '? Needs Update'
    END AS HealthStatus,
    CASE 
        WHEN DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) >= 60 THEN 'RUN: Monthly_Statistics_Update.sql'
        ELSE 'No action needed'
    END AS Recommendation
FROM sys.stats s
INNER JOIN sys.tables t ON s.object_id = t.object_id
WHERE t.name IN ('Exam_Result_of_Student', 'StudentsClass', 'Student', 
                 'Exam_Result_of_Subject', 'Attendance_Student', 
                 'Exam_Obtain_Marks', 'Subject')
    AND s.name LIKE 'IX_%Performance%'
ORDER BY DaysOld DESC
GO

PRINT ''
PRINT '3. INDEX USAGE STATISTICS:'
PRINT '-------------------'

-- 3. Check if indexes are being used
SELECT 
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks AS Seeks,
    s.user_scans AS Scans,
    s.user_lookups AS Lookups,
    s.user_seeks + s.user_scans + s.user_lookups AS TotalUsage,
    CASE 
        WHEN s.user_seeks + s.user_scans + s.user_lookups > 1000 THEN '? Heavily Used'
        WHEN s.user_seeks + s.user_scans + s.user_lookups > 100 THEN '? Used'
        WHEN s.user_seeks + s.user_scans + s.user_lookups > 0 THEN '? Lightly Used'
        ELSE '? Not Used Yet'
    END AS UsageStatus
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE database_id = DB_ID('Edu')
    AND i.name LIKE 'IX_%Performance%'
ORDER BY TotalUsage DESC
GO

PRINT ''
PRINT '4. TABLE SIZE & ROW COUNT:'
PRINT '-------------------'

-- 4. Check table sizes
SELECT 
    t.name AS TableName,
    p.rows AS RowCount,
    SUM(a.total_pages) * 8 / 1024 AS TotalSpaceMB,
    SUM(a.used_pages) * 8 / 1024 AS UsedSpaceMB,
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 / 1024 AS UnusedSpaceMB,
    CASE 
        WHEN p.rows < 1000 THEN '? Small'
        WHEN p.rows < 10000 THEN '? Medium'
        WHEN p.rows < 50000 THEN '? Large'
        ELSE '? Very Large'
    END AS SizeCategory
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name IN ('Exam_Result_of_Student', 'StudentsClass', 'Student', 
                 'Exam_Result_of_Subject', 'Attendance_Student', 
                 'Exam_Obtain_Marks', 'Subject')
    AND i.index_id <= 1
GROUP BY t.name, p.rows
ORDER BY p.rows DESC
GO

PRINT ''
PRINT '5. QUERY PERFORMANCE (Last 10 slowest queries):'
PRINT '-------------------'

-- 5. Find slow queries related to Result page
SELECT TOP 10
    CAST(qs.total_elapsed_time / 1000000.0 AS DECIMAL(10,2)) AS TotalTimeSec,
    qs.execution_count AS ExecCount,
    CAST((qs.total_elapsed_time / 1000000.0) / qs.execution_count AS DECIMAL(10,3)) AS AvgTimeSec,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1, 
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS QueryText,
    CASE 
        WHEN (qs.total_elapsed_time / 1000000.0) / qs.execution_count < 1 THEN '? Fast'
        WHEN (qs.total_elapsed_time / 1000000.0) / qs.execution_count < 5 THEN '? OK'
        ELSE '? Slow'
    END AS PerformanceStatus
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
WHERE qt.text LIKE '%Exam_Result_of_Student%'
    OR qt.text LIKE '%StudentsClass%'
ORDER BY qs.total_elapsed_time DESC
GO

PRINT ''
PRINT '========================================='
PRINT 'OVERALL HEALTH SUMMARY:'
PRINT '========================================='
PRINT ''

-- Overall Summary
DECLARE @IndexCount INT, @OldStatsCount INT, @TotalTables INT

SELECT @TotalTables = 7 -- We have 7 tables

SELECT @IndexCount = COUNT(*)
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name LIKE 'IX_%Performance%'
    AND t.name IN ('Exam_Result_of_Student', 'StudentsClass', 'Student', 
                   'Exam_Result_of_Subject', 'Attendance_Student', 
                   'Exam_Obtain_Marks', 'Subject')

SELECT @OldStatsCount = COUNT(*)
FROM sys.stats s
INNER JOIN sys.tables t ON s.object_id = t.object_id
WHERE DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) >= 60
    AND t.name IN ('Exam_Result_of_Student', 'StudentsClass', 'Student', 
                   'Exam_Result_of_Subject', 'Attendance_Student', 
                   'Exam_Obtain_Marks', 'Subject')

PRINT 'Indexes Created: ' + CAST(@IndexCount AS VARCHAR(10)) + ' / ' + CAST(@TotalTables AS VARCHAR(10))
PRINT 'Old Statistics: ' + CAST(@OldStatsCount AS VARCHAR(10))
PRINT ''

IF @IndexCount = @TotalTables AND @OldStatsCount = 0
BEGIN
    PRINT '??? EXCELLENT - System is running optimally!'
    PRINT 'No action needed at this time.'
END
ELSE IF @IndexCount = @TotalTables AND @OldStatsCount > 0
BEGIN
    PRINT '?? GOOD but Statistics need update'
    PRINT 'ACTION: Run Monthly_Statistics_Update.sql'
END
ELSE IF @IndexCount < @TotalTables
BEGIN
    PRINT '?? CRITICAL - Indexes are missing!'
    PRINT 'ACTION: Run Performance_Optimization.sql immediately'
END

PRINT ''
PRINT '========================================='
PRINT 'Health Check Complete!'
PRINT '========================================='
GO
