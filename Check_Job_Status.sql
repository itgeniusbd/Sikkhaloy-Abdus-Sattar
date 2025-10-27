-- =========================================
-- Check if Job Exists and is Running
-- ?? query ??????? ????? job ??? ????
-- =========================================

USE msdb
GO

-- 1. Check SQL Server Agent Status
PRINT '========================================='
PRINT '1. SQL SERVER AGENT STATUS CHECK'
PRINT '========================================='

SELECT 
    servicename AS ServiceName,
    startup_type_desc AS StartupType,
    status_desc AS CurrentStatus,
    CASE 
        WHEN status_desc = 'Running' THEN '? Agent is Running'
        ELSE '? Agent is NOT Running'
    END AS Status
FROM sys.dm_server_services 
WHERE servicename LIKE 'SQL Server Agent%'
GO

PRINT ''
PRINT '========================================='
PRINT '2. CHECK IF OUR JOB EXISTS'
PRINT '========================================='

-- 2. Check if our specific job exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'Monthly Statistics Update - Edu Database')
BEGIN
    PRINT '? Job EXISTS in system'
    
    -- Show job details
    SELECT 
        j.name AS JobName,
        CASE j.enabled 
            WHEN 1 THEN '? Enabled' 
            ELSE '? Disabled' 
        END AS Status,
        j.description AS Description,
        CASE 
            WHEN js.next_run_date = 0 THEN 'Not Scheduled'
            ELSE CONVERT(VARCHAR(10), CONVERT(DATE, CAST(js.next_run_date AS VARCHAR(8)), 112), 120) + ' ' +
                 STUFF(STUFF(RIGHT('000000' + CAST(js.next_run_time AS VARCHAR(6)), 6), 5, 0, ':'), 3, 0, ':')
        END AS NextRunTime,
        s.name AS ScheduleName,
        CASE s.freq_type
            WHEN 16 THEN 'Monthly'
            WHEN 8 THEN 'Weekly'
            WHEN 4 THEN 'Daily'
            ELSE 'Other'
        END AS Frequency
    FROM msdb.dbo.sysjobs j
    LEFT JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
    LEFT JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
    WHERE j.name = N'Monthly Statistics Update - Edu Database'
END
ELSE
BEGIN
    PRINT '? Job does NOT exist in system'
    PRINT ''
    PRINT 'ACTION REQUIRED: Run Create_Automatic_Job.sql to create it'
END
GO

PRINT ''
PRINT '========================================='
PRINT '3. ALL EXISTING JOBS (If Any)'
PRINT '========================================='

-- 3. Show all jobs in system
SELECT 
    j.name AS JobName,
    CASE j.enabled 
        WHEN 1 THEN '? Enabled' 
        ELSE '? Disabled' 
    END AS Status,
    CASE 
        WHEN ja.run_requested_date IS NOT NULL THEN '? Running Now'
        WHEN j.enabled = 1 THEN '? Ready'
        ELSE '? Disabled'
    END AS CurrentState,
    CASE 
        WHEN js.next_run_date = 0 THEN 'Not Scheduled'
        ELSE CONVERT(VARCHAR(10), CONVERT(DATE, CAST(js.next_run_date AS VARCHAR(8)), 112), 120)
    END AS NextRunDate
FROM msdb.dbo.sysjobs j
LEFT JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
LEFT JOIN msdb.dbo.sysjobactivity ja ON j.job_id = ja.job_id 
    AND ja.run_requested_date IS NOT NULL
    AND ja.stop_execution_date IS NULL
ORDER BY j.name
GO

PRINT ''
PRINT '========================================='
PRINT '4. RECENT JOB HISTORY (Last 5 runs)'
PRINT '========================================='

-- 4. Check job execution history
SELECT TOP 5
    j.name AS JobName,
    CONVERT(VARCHAR(20), msdb.dbo.agent_datetime(jh.run_date, jh.run_time), 120) AS ExecutionTime,
    CASE jh.run_status
        WHEN 0 THEN '? Failed'
        WHEN 1 THEN '? Succeeded'
        WHEN 2 THEN '? Retry'
        WHEN 3 THEN '? Cancelled'
        ELSE 'Unknown'
    END AS Status,
    jh.message AS Message,
    STUFF(STUFF(RIGHT('000000' + CAST(jh.run_duration AS VARCHAR(6)), 6), 5, 0, ':'), 3, 0, ':') AS Duration
FROM msdb.dbo.sysjobs j
INNER JOIN msdb.dbo.sysjobhistory jh ON j.job_id = jh.job_id
WHERE j.name = N'Monthly Statistics Update - Edu Database'
    AND jh.step_id = 0  -- Overall job status
ORDER BY jh.run_date DESC, jh.run_time DESC
GO

PRINT ''
PRINT '========================================='
PRINT 'SUMMARY & RECOMMENDATIONS'
PRINT '========================================='

-- Final Summary
DECLARE @AgentRunning BIT = 0
DECLARE @JobExists BIT = 0
DECLARE @JobEnabled BIT = 0

-- Check Agent
IF EXISTS (SELECT * FROM sys.dm_server_services 
           WHERE servicename LIKE 'SQL Server Agent%' 
           AND status_desc = 'Running')
    SET @AgentRunning = 1

-- Check Job
IF EXISTS (SELECT * FROM msdb.dbo.sysjobs 
           WHERE name = N'Monthly Statistics Update - Edu Database')
BEGIN
    SET @JobExists = 1
    SELECT @JobEnabled = enabled 
    FROM msdb.dbo.sysjobs 
    WHERE name = N'Monthly Statistics Update - Edu Database'
END

PRINT ''
IF @AgentRunning = 1
    PRINT '? SQL Server Agent: Running'
ELSE
    PRINT '? SQL Server Agent: NOT Running (Start from Services.msc)'

IF @JobExists = 1
BEGIN
    PRINT '? Job: Created'
    IF @JobEnabled = 1
        PRINT '? Job Status: Enabled & Active'
    ELSE
        PRINT '? Job Status: Disabled (Enable it)'
END
ELSE
BEGIN
    PRINT '? Job: NOT Created'
    PRINT ''
    PRINT 'NEXT STEP: Run Create_Automatic_Job.sql'
END

PRINT ''
PRINT '========================================='
GO
