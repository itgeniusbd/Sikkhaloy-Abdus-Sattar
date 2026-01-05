-- ==========================================
-- Stored Procedure: AAP_Auto_Generate_Monthly_Invoice
-- Purpose: Automatically generate monthly invoices for all schools
-- Schedule: Runs on 1st of every month to create previous month's invoices
-- ==========================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AAP_Auto_Generate_Monthly_Invoice]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[AAP_Auto_Generate_Monthly_Invoice]
END
GO

CREATE PROCEDURE [dbo].[AAP_Auto_Generate_Monthly_Invoice]
    @TargetMonth DATE = NULL,  -- Optional: specify month, else uses previous month
    @RegistrationID INT = 1    -- Default registration ID
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MonthName NVARCHAR(50);
    DECLARE @MonthDate DATE;
    DECLARE @IssueDate DATE;
    DECLARE @EndDate DATE;
    
    -- If no target month specified, use previous month
    IF @TargetMonth IS NULL
    BEGIN
        SET @MonthDate = EOMONTH(DATEADD(MONTH, -1, GETDATE()));
    END
    ELSE
    BEGIN
        SET @MonthDate = EOMONTH(@TargetMonth);
    END
    
    SET @MonthName = FORMAT(@MonthDate, 'MMM yyyy');
    SET @IssueDate = DATEADD(DAY, 1, @MonthDate); -- Next month ?? ? ?????
    SET @EndDate = DATEADD(DAY, 15, @IssueDate);  -- ?? ??? ???
    
    PRINT 'Generating invoices for: ' + @MonthName;
    PRINT 'Issue Date: ' + CONVERT(NVARCHAR, @IssueDate, 106);
    PRINT 'End Date: ' + CONVERT(NVARCHAR, @EndDate, 106);
    
    -- Cursor to loop through all schools with student count data
    DECLARE @SchoolID INT;
    DECLARE @SchoolName NVARCHAR(200);
    DECLARE @StudentCount INT;
    DECLARE @ActiveStudent INT;
    DECLARE @PerStudentRate FLOAT;
    DECLARE @Discount FLOAT;
    DECLARE @Fixed FLOAT;
    DECLARE @IS_ServiceChargeActive BIT;
    DECLARE @TotalAmount FLOAT;
    DECLARE @InvoiceCategoryID INT;
    DECLARE @InvoiceExists INT;
    
    -- Get Service Charge category ID
    SELECT @InvoiceCategoryID = InvoiceCategoryID 
    FROM AAP_Invoice_Category 
    WHERE InvoiceCategory = N'Service Charge';
    
    IF @InvoiceCategoryID IS NULL
    BEGIN
        PRINT 'Error: Service Charge category not found!';
        RETURN;
    END
    
    -- Cursor for schools
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
        AND si.IS_ServiceChargeActive = 1;  -- Only active schools
    
    OPEN school_cursor;
    FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;
    
    DECLARE @SuccessCount INT = 0;
    DECLARE @SkipCount INT = 0;
    DECLARE @ErrorCount INT = 0;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            -- Check if invoice already exists
            SELECT @InvoiceExists = COUNT(*)
            FROM AAP_Invoice
            WHERE SchoolID = @SchoolID
                AND InvoiceCategoryID = @InvoiceCategoryID
                AND FORMAT(MonthName, 'MMM yyyy') = @MonthName;
            
            IF @InvoiceExists > 0
            BEGIN
                PRINT 'Invoice already exists for School ID: ' + CAST(@SchoolID AS NVARCHAR) + ' (' + @SchoolName + ')';
                SET @SkipCount = @SkipCount + 1;
            END
            ELSE
            BEGIN
                -- Calculate total amount
                IF @Fixed > 0
                BEGIN
                    SET @TotalAmount = @Fixed;
                END
                ELSE
                BEGIN
                    SET @TotalAmount = @StudentCount * @PerStudentRate;
                END
                
                -- Insert invoice
                INSERT INTO AAP_Invoice (
                    RegistrationID,
                    InvoiceCategoryID,
                    SchoolID,
                    IssuDate,
                    EndDate,
                    Invoice_For,
                    TotalAmount,
                    Discount,
                    MonthName,
                    Invoice_SN,
                    Unit,
                    UnitPrice
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
                    @StudentCount,
                    CASE WHEN @Fixed > 0 THEN NULL ELSE @PerStudentRate END
                );
                
                SET @SuccessCount = @SuccessCount + 1;
                PRINT 'Invoice created for School ID: ' + CAST(@SchoolID AS NVARCHAR) + ' (' + @SchoolName + ') - Amount: ' + CAST(@TotalAmount AS NVARCHAR);
            END
        END TRY
        BEGIN CATCH
            SET @ErrorCount = @ErrorCount + 1;
            PRINT 'Error creating invoice for School ID: ' + CAST(@SchoolID AS NVARCHAR) + ' - ' + ERROR_MESSAGE();
        END CATCH
        
        FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;
    END
    
    CLOSE school_cursor;
    DEALLOCATE school_cursor;
    
    -- Summary
    PRINT '';
    PRINT '========================================';
    PRINT 'Invoice Generation Summary for ' + @MonthName;
    PRINT '========================================';
    PRINT 'Successfully Created: ' + CAST(@SuccessCount AS NVARCHAR);
    PRINT 'Already Exists (Skipped): ' + CAST(@SkipCount AS NVARCHAR);
    PRINT 'Errors: ' + CAST(@ErrorCount AS NVARCHAR);
    PRINT '========================================';
END
GO

PRINT 'Stored Procedure AAP_Auto_Generate_Monthly_Invoice created successfully!';
GO
