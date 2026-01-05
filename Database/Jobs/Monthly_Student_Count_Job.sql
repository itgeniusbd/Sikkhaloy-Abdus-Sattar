-- ==========================================
-- SQL Server Agent Job: Monthly Student Count Auto Insert
-- Purpose: Automatically insert current month's student count data
-- Schedule: Runs on the 28th of every month at 11:00 PM
-- ==========================================

USE [msdb]
GO

-- Delete existing job if exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'Monthly_Student_Count_Auto_Insert')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name=N'Monthly_Student_Count_Auto_Insert', @delete_unused_schedule=1;
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
    @job_name=N'Monthly_Student_Count_Auto_Insert', 
    @enabled=1, 
    @notify_level_eventlog=0, 
    @notify_level_email=0, 
    @notify_level_netsend=0, 
    @notify_level_page=0, 
    @delete_level=0, 
    @description=N'Automatically inserts current month student count data on the 28th of every month', 
    @category_name=N'[Uncategorized (Local)]', 
    @owner_login_name=N'sa', 
    @job_id = @jobId OUTPUT

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Job Step
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep 
    @job_id=@jobId, 
    @step_name=N'Insert Current Month Data', 
    @step_id=1, 
    @cmdexec_success_code=0, 
    @on_success_action=1, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=0, 
    @retry_interval=0, 
    @os_run_priority=0, 
    @subsystem=N'TSQL', 
    @command=N'-- Insert current month data (28th ?????? ????, ??? ????? data insert ????)
EXEC AAP_Student_Count_Monthly_Insert;

PRINT ''Data inserted successfully for '' + FORMAT(GETDATE(), ''MMMM yyyy'');', 
    @database_name=N'Edu', 
    @flags=0

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Create Schedule: Runs on 28th of every month at 11:00 PM
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule 
    @job_id=@jobId, 
    @name=N'Monthly on 28th at 11PM', 
    @enabled=1, 
    @freq_type=16,  -- Monthly
    @freq_interval=28,  -- On day 28 of month
    @freq_subday_type=1, 
    @freq_subday_interval=0, 
    @freq_relative_interval=0, 
    @freq_recurrence_factor=1, 
    @active_start_date=20260101, 
    @active_end_date=99991231, 
    @active_start_time=230000,  -- 11:00:00 PM
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

PRINT 'SQL Server Agent Job created successfully!'
PRINT 'Job Name: Monthly_Student_Count_Auto_Insert'
PRINT 'Schedule: Runs on the 28th of every month at 11:00 PM'
PRINT 'Action: Automatically inserts CURRENT month student count data'
PRINT ''
PRINT 'Example: January 28, 2026 ??? ?????? ???? January 2026 ?? data insert ???'
PRINT 'Example: December 28, 2026 ??? ?????? ???? December 2026 ?? data insert ???'
GO
