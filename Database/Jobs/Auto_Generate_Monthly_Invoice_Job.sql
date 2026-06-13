-- ==========================================
-- SQL Server Agent Job: Auto Generate Monthly Invoices
-- Schedule: 1st of every month at 12:01 AM
-- Action: Previous month student count + Service Charge invoices
-- Example: 1 Jun 2026 00:01 → May 2026 count + May 2026 invoices
-- ==========================================

USE [msdb]
GO

IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = N'Auto_Generate_Monthly_Invoice')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name=N'Auto_Generate_Monthly_Invoice', @delete_unused_schedule=1;
END
GO

BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
    EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
    IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode = msdb.dbo.sp_add_job 
    @job_name=N'Auto_Generate_Monthly_Invoice', 
    @enabled=1, 
    @notify_level_eventlog=0, 
    @notify_level_email=0, 
    @notify_level_netsend=0, 
    @notify_level_page=0, 
    @delete_level=0, 
    @description=N'Generates previous month student count and Service Charge invoices on the 1st of each month', 
    @category_name=N'[Uncategorized (Local)]', 
    @owner_login_name=N'sa', 
    @job_id = @jobId OUTPUT

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep 
    @job_id=@jobId, 
    @step_name=N'Generate Previous Month Count + Invoices', 
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
    @command=N'DECLARE @BillingMonth DATE = EOMONTH(DATEADD(MONTH, -1, GETDATE()));
DECLARE @MonthLabel NVARCHAR(50) = FORMAT(@BillingMonth, ''MMM yyyy'');
DECLARE @GeneratedCount INT;
DECLARE @ErrorMessage NVARCHAR(500);

PRINT ''=== Auto Monthly Process for '' + @MonthLabel + '' ==='';

EXEC sp_Generate_Monthly_Student_Count
    @TargetMonth = @BillingMonth,
    @GeneratedCount = @GeneratedCount OUTPUT,
    @ErrorMessage = @ErrorMessage OUTPUT;
PRINT ''Student Count: '' + ISNULL(@ErrorMessage, ''(no message)'');

IF @GeneratedCount > 0 OR @ErrorMessage LIKE ''Student count already exists%''
BEGIN
    WAITFOR DELAY ''00:00:02'';
    EXEC AAP_Auto_Generate_Monthly_Invoice @TargetMonth = @BillingMonth;
    PRINT ''Invoice generation completed for '' + @MonthLabel;
END
ELSE
BEGIN
    RAISERROR(''Auto invoice skipped: student count was not generated.'', 16, 1);
END', 
    @database_name=N'Edu', 
    @flags=0

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule 
    @job_id=@jobId, 
    @name=N'Monthly on 1st at 12:01 AM', 
    @enabled=1, 
    @freq_type=16,
    @freq_interval=1,
    @freq_subday_type=1, 
    @freq_subday_interval=0, 
    @freq_relative_interval=0, 
    @freq_recurrence_factor=1, 
    @active_start_date=20260101, 
    @active_end_date=99991231, 
    @active_start_time=100,
    @active_end_time=235959

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave

QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO

PRINT 'Job Auto_Generate_Monthly_Invoice created/enabled. Runs 1st of each month at 12:01 AM.';
GO
