-- Auto monthly invoice: previous month count + Service Charge invoices.
-- Schedule stays on SQL Agent job Auto_Generate_Monthly_Invoice (1st of month, 12:01 AM).
-- Run this once on the Edu database. Recreating the job is not required if it is already Enabled.

IF OBJECT_ID(N'dbo.AAP_Auto_Generate_Monthly_Invoice', N'P') IS NULL
BEGIN
    RAISERROR(N'AAP_Auto_Generate_Monthly_Invoice was not found.', 16, 1);
    RETURN;
END
GO

ALTER PROCEDURE [dbo].[AAP_Auto_Generate_Monthly_Invoice]
    @TargetMonth DATE = NULL,
    @RegistrationID INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MonthName NVARCHAR(50);
    DECLARE @MonthDate DATE;
    DECLARE @IssueDate DATE;
    DECLARE @EndDate DATE;

    IF @TargetMonth IS NULL
        SET @MonthDate = EOMONTH(DATEADD(MONTH, -1, GETDATE()));
    ELSE
        SET @MonthDate = EOMONTH(@TargetMonth);

    SET @MonthName = FORMAT(@MonthDate, 'MMM yyyy');
    SET @IssueDate = DATEADD(DAY, 1, @MonthDate);
    SET @EndDate = DATEFROMPARTS(YEAR(@IssueDate), MONTH(@IssueDate), 15);
    IF @EndDate < @IssueDate
        SET @EndDate = DATEADD(MONTH, 1, @EndDate);

    DECLARE @SchoolID INT;
    DECLARE @SchoolName NVARCHAR(200);
    DECLARE @StudentCount INT;
    DECLARE @ActiveStudent INT;
    DECLARE @CommitteeCount INT;
    DECLARE @BillableCount INT;
    DECLARE @PerStudentRate FLOAT;
    DECLARE @Discount FLOAT;
    DECLARE @Fixed FLOAT;
    DECLARE @IS_ServiceChargeActive BIT;
    DECLARE @TotalAmount FLOAT;
    DECLARE @InvoiceCategoryID INT;
    DECLARE @InvoiceExists INT;

    SELECT @InvoiceCategoryID = InvoiceCategoryID
    FROM AAP_Invoice_Category
    WHERE InvoiceCategory = N'Service Charge';

    IF @InvoiceCategoryID IS NULL
    BEGIN
        PRINT 'Error: Service Charge category not found!';
        RETURN;
    END

    DECLARE school_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        si.SchoolID,
        si.SchoolName,
        si.Per_Student_Rate,
        si.IS_ServiceChargeActive,
        ISNULL(si.Discount, 0) AS Discount,
        ISNULL(si.Fixed, 0) AS Fixed,
        ISNULL(scm.StudentCount, scm.Active_Student) AS StudentCount,
        scm.Active_Student
    FROM SchoolInfo si
    INNER JOIN AAP_Student_Count_Monthly scm ON si.SchoolID = scm.SchoolID
    WHERE EOMONTH(scm.Month) = @MonthDate
      AND si.IS_ServiceChargeActive = 1;

    OPEN school_cursor;
    FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;

    DECLARE @SuccessCount INT = 0;
    DECLARE @SkipCount INT = 0;
    DECLARE @ErrorCount INT = 0;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            IF OBJECT_ID(N'dbo.fn_GetBillableCommitteeCount', N'FN') IS NOT NULL
                SET @CommitteeCount = dbo.fn_GetBillableCommitteeCount(@SchoolID);
            ELSE
                SET @CommitteeCount = 0;
            SET @BillableCount = ISNULL(@StudentCount, 0) + ISNULL(@CommitteeCount, 0);

            SELECT @InvoiceExists = COUNT(*)
            FROM AAP_Invoice
            WHERE SchoolID = @SchoolID
              AND InvoiceCategoryID = @InvoiceCategoryID
              AND EOMONTH(MonthName) = @MonthDate;

            IF @InvoiceExists > 0
            BEGIN
                SET @SkipCount = @SkipCount + 1;
            END
            ELSE
            BEGIN
                IF @Fixed > 0
                    SET @TotalAmount = @Fixed;
                ELSE
                    SET @TotalAmount = @BillableCount * @PerStudentRate;

                INSERT INTO AAP_Invoice (
                    RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate,
                    Invoice_For, TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice
                )
                VALUES (
                    @RegistrationID,
                    @InvoiceCategoryID,
                    @SchoolID,
                    @IssueDate,
                    @EndDate,
                    @MonthName,
                    @TotalAmount,
                    @Discount,
                    @MonthDate,
                    dbo.Invoice_SerialNumber(@SchoolID),
                    @BillableCount,
                    CASE WHEN @Fixed > 0 THEN NULL ELSE @PerStudentRate END
                );

                SET @SuccessCount = @SuccessCount + 1;
            END
        END TRY
        BEGIN CATCH
            SET @ErrorCount = @ErrorCount + 1;
            PRINT 'Error for School ID: ' + CAST(@SchoolID AS NVARCHAR(10)) + ' - ' + ERROR_MESSAGE();
        END CATCH

        FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;
    END

    CLOSE school_cursor;
    DEALLOCATE school_cursor;

    PRINT 'Invoice Generation Summary for ' + @MonthName;
    PRINT 'Created: ' + CAST(@SuccessCount AS NVARCHAR(10));
    PRINT 'Skipped (already exists): ' + CAST(@SkipCount AS NVARCHAR(10));
    PRINT 'Errors: ' + CAST(@ErrorCount AS NVARCHAR(10));
END
GO

-- Keep the existing SQL Agent job. Only refresh the step so invoices still generate
-- when last-month student count is already in the table.
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'Auto_Generate_Monthly_Invoice')
BEGIN
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

EXEC dbo.sp_Generate_Monthly_Student_Count
    @TargetMonth = @BillingMonth,
    @GeneratedCount = @GeneratedCount OUTPUT,
    @ErrorMessage = @ErrorMessage OUTPUT;
PRINT ''Student Count: '' + ISNULL(@ErrorMessage, ''(no message)'');

IF @GeneratedCount > 0
   OR @ErrorMessage LIKE ''Student count already exists%''
   OR EXISTS (SELECT 1 FROM dbo.AAP_Student_Count_Monthly WHERE EOMONTH(Month) = @BillingMonth)
BEGIN
    WAITFOR DELAY ''00:00:02'';
    EXEC dbo.AAP_Auto_Generate_Monthly_Invoice @TargetMonth = @BillingMonth;
    PRINT ''Invoice generation completed for '' + @MonthLabel;
END
ELSE
BEGIN
    RAISERROR(''Auto invoice skipped: student count was not generated.'', 16, 1);
END';

    PRINT 'SQL Agent job Auto_Generate_Monthly_Invoice is enabled and the step was updated.';
END
ELSE
BEGIN
    PRINT 'Job Auto_Generate_Monthly_Invoice was not found. Create it from Database/Jobs/Auto_Generate_Monthly_Invoice_Job.sql';
END
GO
