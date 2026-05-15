-- Fix: AAP_Auto_Generate_Monthly_Invoice - Add committee billing count
-- Problem: Auto invoice was not including committee members in billing calculation
-- The manual process (Monthly_Button_Click) correctly adds committee count,
-- but this stored procedure was missing it.

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
    SET @EndDate = DATEADD(DAY, 15, @IssueDate);

    PRINT 'Generating invoices for: ' + @MonthName;
    PRINT 'Issue Date: ' + CONVERT(NVARCHAR, @IssueDate, 106);
    PRINT 'End Date: ' + CONVERT(NVARCHAR, @EndDate, 106);

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

    DECLARE school_cursor CURSOR FOR
    SELECT 
        si.SchoolID,
        si.SchoolName,
        si.Per_Student_Rate,
        si.IS_ServiceChargeActive,
        ISNULL(si.Discount, 0) AS Discount,
        ISNULL(si.Fixed, 0) AS Fixed,
        scm.StudentCount,
        scm.Active_Student
    FROM SchoolInfo si
    INNER JOIN AAP_Student_Count_Monthly scm ON si.SchoolID = scm.SchoolID
    WHERE FORMAT(scm.Month, 'MMM yyyy') = @MonthName
        AND si.IS_ServiceChargeActive = 1;

    OPEN school_cursor;
    FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;

    DECLARE @SuccessCount INT = 0;
    DECLARE @SkipCount INT = 0;
    DECLARE @ErrorCount INT = 0;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            -- Get committee billable count for this school (if committee billing is enabled)
            SET @CommitteeCount = dbo.fn_GetBillableCommitteeCount(@SchoolID);
            -- Total billable = students + committee members (committee = 0 if option disabled)
            SET @BillableCount = @StudentCount + @CommitteeCount;

            SELECT @InvoiceExists = COUNT(*)
            FROM AAP_Invoice
            WHERE SchoolID = @SchoolID
                AND InvoiceCategoryID = @InvoiceCategoryID
                AND FORMAT(MonthName, 'MMM yyyy') = @MonthName;

            IF @InvoiceExists > 0
            BEGIN
                PRINT 'Invoice already exists for School ID: ' + CAST(@SchoolID AS NVARCHAR(10)) + ' (' + @SchoolName + ')';
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
                PRINT 'Invoice created for School ID: ' + CAST(@SchoolID AS NVARCHAR(10)) + 
                      ' (' + @SchoolName + ') - Amount: ' + CAST(@TotalAmount AS NVARCHAR(20)) +
                      ' (Students: ' + CAST(@StudentCount AS NVARCHAR(10)) + 
                      ', Committee: ' + CAST(@CommitteeCount AS NVARCHAR(10)) + ')';
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

    PRINT '';
    PRINT '========================================';
    PRINT 'Invoice Generation Summary for ' + @MonthName;
    PRINT '========================================';
    PRINT 'Successfully Created: ' + CAST(@SuccessCount AS NVARCHAR(10));
    PRINT 'Already Exists (Skipped): ' + CAST(@SkipCount AS NVARCHAR(10));
    PRINT 'Errors: ' + CAST(@ErrorCount AS NVARCHAR(10));
    PRINT '========================================';
END
GO
