-- =========================================
-- CREATE AUTOMATIC SQL SERVER AGENT JOB
-- ???? ????? ????? - ????? automatic ????
-- =========================================
-- Prerequisites:
-- 1. SQL Server Agent service ???? ????? ???
-- 2. SQL Server Standard ?? Enterprise edition ?????
-- 3. sysadmin permission ?????
-- =========================================

USE msdb
GO

-- Check if SQL Server Agent is running
IF (SELECT status FROM sys.dm_server_services WHERE servicename LIKE 'SQL Server Agent%') = 4
BEGIN
    PRINT '? SQL Server Agent is running'
END
ELSE
BEGIN
    PRINT '? SQL Server Agent is NOT running'
    PRINT 'Please start SQL Server Agent service first'
    PRINT 'Services.msc -> SQL Server Agent -> Start'
    -- RETURN -- uncomment this if you want to stop execution
END
GO

-- Step 1: Create Job Category (if not exists)
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name = N'Database Maintenance' AND category_class = 1)
BEGIN
    EXEC msdb.dbo.sp_add_category
        @class = N'JOB',
        @type = N'LOCAL',
        @name = N'Database Maintenance'
END
GO

-- Step 2: Delete existing job if exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'Monthly Statistics Update - Edu Database')
BEGIN
    PRINT 'Deleting existing job...'
    EXEC msdb.dbo.sp_delete_job @job_name = N'Monthly Statistics Update - Edu Database', @delete_unused_schedule = 1
END
GO

-- Step 3: Create the Job
DECLARE @jobId BINARY(16)
EXEC msdb.dbo.sp_add_job
    @job_name = N'Monthly Statistics Update - Edu Database',
    @enabled = 1,
    @description = N'Updates statistics for Edu database tables monthly to maintain optimal performance',
    @category_name = N'Database Maintenance',
    @owner_login_name = N'sa', -- Change this to your login if needed
    @job_id = @jobId OUTPUT

PRINT 'Job created with ID: ' + CAST(@jobId AS VARCHAR(50))
GO

-- Step 4: Add Job Step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'Monthly Statistics Update - Edu Database',
    @step_name = N'Update Statistics',
    @step_id = 1,
    @cmdexec_success_code = 0,
    @on_success_action = 1, -- Quit with success
    @on_fail_action = 2, -- Quit with failure
    @retry_attempts = 2,
    @retry_interval = 5,
    @database_name = N'Edu',
    @command = N'
-- Update Statistics for all important tables
PRINT ''Starting statistics update...''

UPDATE STATISTICS Exam_Result_of_Student WITH FULLSCAN
PRINT ''? Exam_Result_of_Student updated''

UPDATE STATISTICS Exam_Result_of_Subject WITH FULLSCAN
PRINT ''? Exam_Result_of_Subject updated''

UPDATE STATISTICS Exam_Obtain_Marks WITH FULLSCAN
PRINT ''? Exam_Obtain_Marks updated''

UPDATE STATISTICS Student WITH FULLSCAN
PRINT ''? Student updated''

UPDATE STATISTICS StudentsClass WITH FULLSCAN
PRINT ''? StudentsClass updated''

UPDATE STATISTICS Attendance_Student WITH FULLSCAN
PRINT ''? Attendance_Student updated''

UPDATE STATISTICS Subject WITH FULLSCAN
PRINT ''? Subject updated''

PRINT ''Statistics update completed successfully!''
'
GO

-- Step 5: Set Job Server
EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'Monthly Statistics Update - Edu Database',
    @server_name = N'(local)'
GO

-- Step 6: Create Monthly Schedule
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Monthly - First Sunday 2AM',
    @enabled = 1,
    @freq_type = 16, -- Monthly
    @freq_interval = 1, -- First
    @freq_subday_type = 1, -- At the specified time
    @freq_subday_interval = 0,
    @freq_relative_interval = 1, -- First
    @freq_recurrence_factor = 1, -- Every month
    @active_start_date = 20250101, -- Start from 2025
    @active_start_time = 020000 -- 2:00 AM
GO

-- Step 7: Attach Schedule to Job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'Monthly Statistics Update - Edu Database',
    @schedule_name = N'Monthly - First Sunday 2AM'
GO

PRINT '========================================='
PRINT '? SQL Server Agent Job Created Successfully!'
PRINT '========================================='
PRINT 'Job Name: Monthly Statistics Update - Edu Database'
PRINT 'Schedule: First Sunday of every month at 2:00 AM'
PRINT 'Status: Enabled'
PRINT '========================================='
PRINT ''
PRINT 'To manually run the job now:'
PRINT 'EXEC msdb.dbo.sp_start_job @job_name = ''Monthly Statistics Update - Edu Database'''
PRINT ''
PRINT 'To view job history:'
PRINT 'EXEC msdb.dbo.sp_help_jobhistory @job_name = ''Monthly Statistics Update - Edu Database'''
PRINT ''
PRINT 'To disable the job:'
PRINT 'EXEC msdb.dbo.sp_update_job @job_name = ''Monthly Statistics Update - Edu Database'', @enabled = 0'
PRINT '========================================='
GO

-- Step 8: Verify Job Creation
SELECT 
    j.name AS JobName,
    j.enabled AS IsEnabled,
    j.description AS Description,
    s.name AS ScheduleName,
    CASE s.freq_type
        WHEN 16 THEN 'Monthly'
        WHEN 8 THEN 'Weekly'
        WHEN 4 THEN 'Daily'
    END AS Frequency,
    CASE 
        WHEN j.enabled = 1 THEN '? Active'
        ELSE '? Disabled'
    END AS Status
FROM msdb.dbo.sysjobs j
LEFT JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
LEFT JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
WHERE j.name = N'Monthly Statistics Update - Edu Database'
GO

PRINT ''
PRINT 'Job setup complete! It will run automatically.'
GO
