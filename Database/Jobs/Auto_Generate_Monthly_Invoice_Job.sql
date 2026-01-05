-- ==========================================
-- SQL Server Agent Job: Auto Generate Monthly Invoices
-- Purpose: Automatically generate invoices for all schools
-- Schedule: Runs on 1st of every month at 12:01 AM
-- ==========================================

USE [msdb]
GO

-- Delete existing job if exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'Auto_Generate_Monthly_Invoice')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name=N'Auto_Generate_Monthly_Invoice', @delete_unused_schedule=1;
END
GO

-- Create the job
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

-- Create Job Category if not exists
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
    EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
    IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
END

-- Create the Job
DECLARE @jobId BINARY(16)
EXEC @ReturnCode = msdb.dbo.sp_add_job 
    @job_name=N'Auto_Generate_Monthly_Invoice', 
    @enabled=1, 
    @notify_level_eventlog=0, 
    @notify_level_email=0, 
    @notify_level_netsend=0, 
    @notify_level_page=0, 
    @delete_level=0, 
    @description=N'Automatically generates monthly invoices for all schools on the 1st of every month', 
    @category_name=N'[Uncategorized (Local)]', 
    @owner_login_name=N'sa', 
    @job_id = @jobId OUTPUT

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Job Step
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep 
    @job_id=@jobId, 
    @step_name=N'Generate Previous Month Invoices', 
    @step_id=1, 
    @cmdexec_success_code=0, 
    @on_success_action=1, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=2, 
    @retry_interval=5, 
    @os_run_priority=0, 
    @subsystem=N'TSQL', 
    @command=N'-- Generate invoices for previous month
DECLARE @PreviousMonth DATE = EOMONTH(DATEADD(MONTH, -1, GETDATE()));
DECLARE @MonthName NVARCHAR(50) = FORMAT(@PreviousMonth, ''MMM yyyy'');

PRINT ''Starting invoice generation for: '' + @MonthName;

-- Execute the stored procedure
EXEC AAP_Auto_Generate_Monthly_Invoice @TargetMonth = @PreviousMonth;

PRINT ''Invoice generation completed for: '' + @MonthName;', 
    @database_name=N'Edu', 
    @flags=0

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Create Schedule: Runs on 1st of every month at 12:01 AM
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule 
    @job_id=@jobId, 
    @name=N'Monthly on 1st at 12:01 AM', 
    @enabled=1, 
    @freq_type=16,  -- Monthly
    @freq_interval=1,  -- On day 1 of month
    @freq_subday_type=1, 
    @freq_subday_interval=0, 
    @freq_relative_interval=0, 
    @freq_recurrence_factor=1, 
    @active_start_date=20260101, 
    @active_end_date=99991231, 
    @active_start_time=000100,  -- 12:01:00 AM
    @active_end_time=235959

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add job to local server
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave

QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO

PRINT '';
PRINT '========================================';
PRINT 'SQL Server Agent Job Created Successfully!';
PRINT '========================================';
PRINT 'Job Name: Auto_Generate_Monthly_Invoice';
PRINT 'Schedule: Runs on the 1st of every month at 12:01 AM';
PRINT 'Action: Generates invoices for previous month automatically';
PRINT '';
PRINT 'Examples:';
PRINT '  - January 1, 2026 ??? 12:01 ? ???? December 2025 ?? invoice generate ???';
PRINT '  - February 1, 2026 ??? 12:01 ? ???? January 2026 ?? invoice generate ???';
PRINT '  - January 1, 2027 ??? 12:01 ? ???? December 2026 ?? invoice generate ???';
PRINT '========================================';
GO
