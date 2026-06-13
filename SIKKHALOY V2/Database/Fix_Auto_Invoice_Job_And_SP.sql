-- One-time fix: enable job + update step
-- Also run: Database/StoredProcedures/AAP_Auto_Generate_Monthly_Invoice.sql

USE [msdb]
GO

EXEC msdb.dbo.sp_update_job
    @job_name = N'Auto_Generate_Monthly_Invoice',
    @enabled = 1;

EXEC msdb.dbo.sp_update_jobstep
    @job_name = N'Auto_Generate_Monthly_Invoice',
    @step_id = 1,
    @command = N'DECLARE @BillingMonth DATE = EOMONTH(DATEADD(MONTH, -1, GETDATE()));
DECLARE @MonthLabel NVARCHAR(50) = FORMAT(@BillingMonth, ''MMM yyyy'');
DECLARE @GeneratedCount INT;
DECLARE @ErrorMessage NVARCHAR(500);

PRINT ''=== Auto Monthly Process for '' + @MonthLabel + '' ==='';

EXEC Edu.dbo.sp_Generate_Monthly_Student_Count
    @TargetMonth = @BillingMonth,
    @GeneratedCount = @GeneratedCount OUTPUT,
    @ErrorMessage = @ErrorMessage OUTPUT;
PRINT ''Student Count: '' + ISNULL(@ErrorMessage, ''(no message)'');

IF @GeneratedCount > 0 OR @ErrorMessage LIKE ''Student count already exists%''
BEGIN
    WAITFOR DELAY ''00:00:02'';
    EXEC Edu.dbo.AAP_Auto_Generate_Monthly_Invoice @TargetMonth = @BillingMonth;
    PRINT ''Invoice generation completed for '' + @MonthLabel;
END
ELSE
BEGIN
    RAISERROR(''Auto invoice skipped: student count was not generated.'', 16, 1);
END';

PRINT 'Job enabled and step updated.';
GO
