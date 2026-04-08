-- =============================================
-- Monthly Automatic Student Count & Invoice Generation
-- Author: Copilot
-- Create date: 2025
-- Description: Automatically generates student count and invoices monthly
-- =============================================

-- Step 0: Create Function to Get Billable Committee Count
IF OBJECT_ID('fn_GetBillableCommitteeCount', 'FN') IS NOT NULL
    DROP FUNCTION fn_GetBillableCommitteeCount
GO

CREATE FUNCTION fn_GetBillableCommitteeCount (@SchoolID INT)
RETURNS INT
AS
BEGIN
    DECLARE @CommitteeCount INT = 0
    
    -- Count active committee members from categories that are:
    -- 1. Included in billing (IsIncluded = 1)
    -- 2. Category is active (IsActive = 1)
    -- 3. Member status is 'Active'
    SELECT @CommitteeCount = COUNT(DISTINCT CM.CommitteeMemberId)
    FROM CommitteeMember CM
    INNER JOIN CommitteeMemberType CMT ON CM.CommitteeMemberTypeId = CMT.CommitteeMemberTypeId
    INNER JOIN CommitteeMember_Billing CMB ON CMT.CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
                                            AND CM.SchoolID = CMB.SchoolID
    WHERE CM.SchoolID = @SchoolID
    AND ISNULL(CM.Status, 'Active') = 'Active' -- Only active members
    AND CMB.IsIncluded = 1 -- Only categories included in billing
    AND CMB.IsActive = 1 -- Only active categories
    
    RETURN ISNULL(@CommitteeCount, 0)
END
GO

-- Create CommitteeMember_Billing table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
BEGIN
    CREATE TABLE CommitteeMember_Billing (
        BillingId INT IDENTITY(1,1) PRIMARY KEY,
        SchoolID INT NOT NULL,
        CommitteeMemberTypeId INT NOT NULL,
        IsIncluded BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETDATE(),
        UpdatedDate DATETIME DEFAULT GETDATE(),
        CONSTRAINT UC_School_Category UNIQUE (SchoolID, CommitteeMemberTypeId),
        CONSTRAINT FK_CommitteeBilling_School FOREIGN KEY (SchoolID) REFERENCES SchoolInfo(SchoolID),
        CONSTRAINT FK_CommitteeBilling_Type FOREIGN KEY (CommitteeMemberTypeId) REFERENCES CommitteeMemberType(CommitteeMemberTypeId)
    )
    
    PRINT 'CommitteeMember_Billing table created successfully'
END
ELSE
BEGIN
    -- Add IsActive column if table exists but column doesn't
    IF NOT EXISTS (SELECT * FROM sys.columns 
                   WHERE object_id = OBJECT_ID('CommitteeMember_Billing') 
                   AND name = 'IsActive')
    BEGIN
        ALTER TABLE CommitteeMember_Billing ADD IsActive BIT NOT NULL DEFAULT 1
        PRINT 'IsActive column added to CommitteeMember_Billing table'
    END
END
GO

-- Step 1: Create Stored Procedure for Student Count Generation
IF OBJECT_ID('sp_Generate_Monthly_Student_Count', 'P') IS NOT NULL
    DROP PROCEDURE sp_Generate_Monthly_Student_Count
GO

CREATE PROCEDURE sp_Generate_Monthly_Student_Count
    @TargetMonth DATE = NULL,
    @GeneratedCount INT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MonthEnd DATE
    DECLARE @ClassCount INT
    DECLARE @MonthStr NVARCHAR(20)
    
    BEGIN TRY
        -- If no month specified, use current month
        IF @TargetMonth IS NULL
            SET @TargetMonth = GETDATE()
        
        -- Get end of month
        SET @MonthEnd = EOMONTH(@TargetMonth)
        SET @MonthStr = CONVERT(NVARCHAR(20), @MonthEnd, 107) -- Format: MMM dd, yyyy
        
        -- Check if data already exists
        IF EXISTS (SELECT 1 FROM AAP_Student_Count_Monthly 
                   WHERE MONTH(Month) = MONTH(@MonthEnd) 
                   AND YEAR(Month) = YEAR(@MonthEnd))
        BEGIN
            SELECT @GeneratedCount = COUNT(*) 
            FROM AAP_Student_Count_Monthly
            WHERE MONTH(Month) = MONTH(@MonthEnd) 
            AND YEAR(Month) = YEAR(@MonthEnd)
            
            SET @ErrorMessage = 'Student count already exists for ' + @MonthStr + ' (' + CAST(@GeneratedCount AS NVARCHAR(10)) + ' institutions)'
            RETURN
        END
        
        -- Generate class-wise student count (PAYMENT ACTIVE sessions + ACTIVE institutions)
        INSERT INTO AAP_StudentClass_Count_Monthly 
        (SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
        SELECT 
            SC.SchoolID,
            SC.ClassID,
            SC.EducationYearID,
            @MonthEnd AS Month,
            COUNT(DISTINCT CASE 
                WHEN S.Status = 'Active' 
                THEN S.StudentID 
            END) AS Active_Student,
            0 AS Reject_Countable,
            0 AS Reject_Uncountable
        FROM StudentsClass SC
        INNER JOIN Student S ON SC.StudentID = S.StudentID
        INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
        INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
        WHERE S.Status = 'Active' -- Only active students
        AND EY.IsActive = 1 -- ONLY PAYMENT ACTIVE SESSIONS
        AND SI.IS_ServiceChargeActive = 1 -- ONLY ACTIVE INSTITUTIONS
        GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
        HAVING COUNT(DISTINCT CASE 
            WHEN S.Status = 'Active' 
            THEN S.StudentID 
        END) > 0
        
        SET @ClassCount = @@ROWCOUNT
        
        -- Generate school-wise total student count
        INSERT INTO AAP_Student_Count_Monthly 
        (SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
        SELECT 
            SchoolID,
            @MonthEnd AS Month,
            SUM(Active_Student) AS Active_Student,
            SUM(Reject_Countable) AS Reject_Countable,
            SUM(Reject_Uncountable) AS Reject_Uncountable
        FROM AAP_StudentClass_Count_Monthly
        WHERE MONTH(Month) = MONTH(@MonthEnd) 
        AND YEAR(Month) = YEAR(@MonthEnd)
        GROUP BY SchoolID
        HAVING SUM(Active_Student) > 0
        
        SET @GeneratedCount = @@ROWCOUNT
        SET @ErrorMessage = 'Success: Generated count for ' + CAST(@GeneratedCount AS NVARCHAR(10)) + ' institutions (' + CAST(@ClassCount AS NVARCHAR(10)) + ' classes)'
        
    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE()
        SET @GeneratedCount = 0
    END CATCH
END
GO

-- Step 2: Create Stored Procedure for Automatic Invoice Generation
IF OBJECT_ID('sp_Generate_Monthly_Invoices', 'P') IS NOT NULL
    DROP PROCEDURE sp_Generate_Monthly_Invoices
GO

CREATE PROCEDURE sp_Generate_Monthly_Invoices
    @TargetMonth DATE = NULL,
    @IssueDate DATE = NULL,
    @GeneratedCount INT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MonthEnd DATE
    DECLARE @ServiceChargeCategoryID INT
    DECLARE @EndDate DATE
    DECLARE @InvoiceFor NVARCHAR(50)
    DECLARE @MonthStr NVARCHAR(20)
    
    BEGIN TRY
        -- If no month specified, use current month
        IF @TargetMonth IS NULL
            SET @TargetMonth = GETDATE()
        
        -- If no issue date specified, use 1st of the month
        IF @IssueDate IS NULL
            SET @IssueDate = DATEFROMPARTS(YEAR(@TargetMonth), MONTH(@TargetMonth), 1)
        
        -- Get end of month
        SET @MonthEnd = EOMONTH(@TargetMonth)
        SET @EndDate = DATEADD(DAY, 15, @IssueDate) -- 15 days payment deadline
        SET @MonthStr = CONVERT(NVARCHAR(20), @MonthEnd, 107) -- Format: MMM dd, yyyy
        SET @InvoiceFor = LEFT(DATENAME(MONTH, @MonthEnd), 3) + ' ' + CAST(YEAR(@MonthEnd) AS NVARCHAR(4))
        
        -- Get Service Charge category ID
        SELECT @ServiceChargeCategoryID = InvoiceCategoryID 
        FROM AAP_Invoice_Category 
        WHERE InvoiceCategory = N'Service Charge'
        
        IF @ServiceChargeCategoryID IS NULL
        BEGIN
            SET @ErrorMessage = 'Service Charge category not found in AAP_Invoice_Category'
            SET @GeneratedCount = 0
            RETURN
        END
        
        -- Check if student count exists for this month
        IF NOT EXISTS (SELECT 1 FROM AAP_Student_Count_Monthly 
                       WHERE MONTH(Month) = MONTH(@MonthEnd) 
                       AND YEAR(Month) = YEAR(@MonthEnd))
        BEGIN
            SET @ErrorMessage = 'Student count not found for ' + @MonthStr + '. Please generate student count first.'
            SET @GeneratedCount = 0
            RETURN
        END
        
        -- Generate invoices for all institutions with student count
        INSERT INTO AAP_Invoice 
        (RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For, 
         TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice)
        SELECT 
            1 AS RegistrationID, -- System generated
            @ServiceChargeCategoryID,
            SC.SchoolID,
            @IssueDate,
            @EndDate,
            @InvoiceFor,
            CASE 
                WHEN ISNULL(SI.Fixed, 0) > 0 THEN SI.Fixed
                ELSE (ISNULL(SC.StudentCount, 0) + ISNULL(dbo.fn_GetBillableCommitteeCount(SC.SchoolID), 0)) * ISNULL(SI.Per_Student_Rate, 0)
            END AS TotalAmount,
            ISNULL(SI.Discount, 0) AS Discount,
            @MonthEnd AS MonthName,
            dbo.Invoice_SerialNumber(SC.SchoolID) AS Invoice_SN,
            ISNULL(SC.StudentCount, 0) + ISNULL(dbo.fn_GetBillableCommitteeCount(SC.SchoolID), 0) AS Unit,
            CASE WHEN ISNULL(SI.Fixed, 0) > 0 THEN NULL ELSE SI.Per_Student_Rate END AS UnitPrice
        FROM AAP_Student_Count_Monthly SC
        INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
        WHERE MONTH(SC.Month) = MONTH(@MonthEnd) 
        AND YEAR(SC.Month) = YEAR(@MonthEnd)
        AND ISNULL(SI.IS_ServiceChargeActive, 0) = 1 -- Only active institutions
        AND NOT EXISTS (
            SELECT 1 FROM AAP_Invoice 
            WHERE SchoolID = SC.SchoolID 
            AND InvoiceCategoryID = @ServiceChargeCategoryID
            AND MONTH(MonthName) = MONTH(@MonthEnd) 
            AND YEAR(MonthName) = YEAR(@MonthEnd)
        )
        
        SET @GeneratedCount = @@ROWCOUNT
        SET @ErrorMessage = 'Success: Generated ' + CAST(@GeneratedCount AS NVARCHAR(10)) + ' invoices for ' + @MonthStr
        
    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE()
        SET @GeneratedCount = 0
    END CATCH
END
GO

-- Step 3: Create Master Procedure that runs both processes
IF OBJECT_ID('sp_Monthly_Auto_Process', 'P') IS NOT NULL
    DROP PROCEDURE sp_Monthly_Auto_Process
GO

CREATE PROCEDURE sp_Monthly_Auto_Process
    @TargetMonth DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GeneratedCount INT
    DECLARE @ErrorMessage NVARCHAR(500)
    DECLARE @LogMessage NVARCHAR(MAX)
    DECLARE @MonthStr NVARCHAR(20)
    
    -- If no month specified, use current month
    IF @TargetMonth IS NULL
        SET @TargetMonth = GETDATE()
    
    SET @MonthStr = CONVERT(NVARCHAR(20), EOMONTH(@TargetMonth), 107)
    SET @LogMessage = 'Monthly Auto Process Started for ' + @MonthStr + CHAR(13) + CHAR(10)
    
    -- Step 1: Generate Student Count
    EXEC sp_Generate_Monthly_Student_Count 
        @TargetMonth = @TargetMonth,
        @GeneratedCount = @GeneratedCount OUTPUT,
        @ErrorMessage = @ErrorMessage OUTPUT
    
    SET @LogMessage = @LogMessage + 'Student Count: ' + @ErrorMessage + CHAR(13) + CHAR(10)
    
    -- Step 2: Generate Invoices (only if student count was successful or already exists)
    IF @GeneratedCount > 0 OR @ErrorMessage LIKE 'Student count already exists%'
    BEGIN
        WAITFOR DELAY '00:00:02' -- Wait 2 seconds
        
        EXEC sp_Generate_Monthly_Invoices 
            @TargetMonth = @TargetMonth,
            @IssueDate = NULL, -- Will use 1st of the month
            @GeneratedCount = @GeneratedCount OUTPUT,
            @ErrorMessage = @ErrorMessage OUTPUT
        
        SET @LogMessage = @LogMessage + 'Invoice Generation: ' + @ErrorMessage
    END
    ELSE
    BEGIN
        SET @LogMessage = @LogMessage + 'Invoice Generation: Skipped due to student count error'
    END
    
    -- Log the result
    PRINT @LogMessage
END
GO

-- Step 4: Create Optional Log Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AAP_Auto_Process_Log')
BEGIN
    CREATE TABLE AAP_Auto_Process_Log (
        LogID INT IDENTITY(1,1) PRIMARY KEY,
        ProcessDate DATETIME DEFAULT GETDATE(),
        ProcessMonth DATE,
        LogMessage NVARCHAR(MAX),
        ProcessType NVARCHAR(50)
    )
END
GO

-- Step 5: Update Master Procedure to include logging
ALTER PROCEDURE sp_Monthly_Auto_Process
    @TargetMonth DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GeneratedCount INT
    DECLARE @ErrorMessage NVARCHAR(500)
    DECLARE @LogMessage NVARCHAR(MAX)
    DECLARE @MonthEnd DATE
    DECLARE @MonthStr NVARCHAR(20)
    
    -- If no month specified, use current month
    IF @TargetMonth IS NULL
        SET @TargetMonth = GETDATE()
    
    SET @MonthEnd = EOMONTH(@TargetMonth)
    SET @MonthStr = CONVERT(NVARCHAR(20), @MonthEnd, 107)
    SET @LogMessage = 'Monthly Auto Process Started for ' + @MonthStr + CHAR(13) + CHAR(10)
    
    -- Step 1: Generate Student Count
    EXEC sp_Generate_Monthly_Student_Count 
        @TargetMonth = @TargetMonth,
        @GeneratedCount = @GeneratedCount OUTPUT,
        @ErrorMessage = @ErrorMessage OUTPUT
    
    SET @LogMessage = @LogMessage + 'Student Count: ' + @ErrorMessage + CHAR(13) + CHAR(10)
    
    -- Log student count result
    INSERT INTO AAP_Auto_Process_Log (ProcessMonth, LogMessage, ProcessType)
    VALUES (@MonthEnd, @ErrorMessage, 'Student Count')
    
    -- Step 2: Generate Invoices (only if student count was successful or already exists)
    IF @GeneratedCount > 0 OR @ErrorMessage LIKE 'Student count already exists%'
    BEGIN
        WAITFOR DELAY '00:00:02' -- Wait 2 seconds
        
        EXEC sp_Generate_Monthly_Invoices 
            @TargetMonth = @TargetMonth,
            @IssueDate = NULL, -- Will use 1st of the month
            @GeneratedCount = @GeneratedCount OUTPUT,
            @ErrorMessage = @ErrorMessage OUTPUT
        
        SET @LogMessage = @LogMessage + 'Invoice Generation: ' + @ErrorMessage
        
        -- Log invoice generation result
        INSERT INTO AAP_Auto_Process_Log (ProcessMonth, LogMessage, ProcessType)
        VALUES (@MonthEnd, @ErrorMessage, 'Invoice Generation')
    END
    ELSE
    BEGIN
        SET @LogMessage = @LogMessage + 'Invoice Generation: Skipped due to student count error'
        
        INSERT INTO AAP_Auto_Process_Log (ProcessMonth, LogMessage, ProcessType)
        VALUES (@MonthEnd, 'Skipped due to student count error', 'Invoice Generation')
    END
    
    -- Print final result
    PRINT @LogMessage
END
GO

-- =============================================
-- TESTING SECTION (Comment out in production)
-- =============================================

-- Test 1: Generate student count for current month
/*
DECLARE @Count INT, @Msg NVARCHAR(500)
EXEC sp_Generate_Monthly_Student_Count 
    @TargetMonth = NULL, -- Current month
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT
PRINT @Msg
*/

-- Test 2: Generate invoices for current month
/*
DECLARE @Count INT, @Msg NVARCHAR(500)
EXEC sp_Generate_Monthly_Invoices 
    @TargetMonth = NULL, -- Current month
    @IssueDate = NULL, -- Will use 1st of month
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT
PRINT @Msg
*/

-- Test 3: Run full auto process
/*
EXEC sp_Monthly_Auto_Process @TargetMonth = NULL -- Current month
*/

-- Test 4: Generate for specific month (e.g., March 2026)
/*
EXEC sp_Monthly_Auto_Process @TargetMonth = '2026-03-01'
*/

-- View logs
/*
SELECT TOP 20 * 
FROM AAP_Auto_Process_Log 
ORDER BY ProcessDate DESC
*/

-- Test 5: View committee billing configuration and count per institution
/*
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    CMT.CommitteeMemberType,
    CMB.IsIncluded,
    CMB.IsActive,
    COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END) as ActiveMembers,
    CASE 
        WHEN CMB.IsIncluded = 1 AND CMB.IsActive = 1 
        THEN COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END)
        ELSE 0
    END as BillableMembers
FROM SchoolInfo SI
LEFT JOIN CommitteeMemberType CMT ON SI.SchoolID = CMT.SchoolID
LEFT JOIN CommitteeMember CM ON CMT.CommitteeMemberTypeId = CM.CommitteeMemberTypeId AND SI.SchoolID = CM.SchoolID
LEFT JOIN CommitteeMember_Billing CMB ON CMT.CommitteeMemberTypeId = CMB.CommitteeMemberTypeId AND SI.SchoolID = CMB.SchoolID
WHERE SI.IS_ServiceChargeActive = 1
GROUP BY SI.SchoolID, SI.SchoolName, CMT.CommitteeMemberType, CMB.IsIncluded, CMB.IsActive
ORDER BY SI.SchoolName, CMT.CommitteeMemberType
*/

-- Test 6: View total billable committee count by institution
/*
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    dbo.fn_GetBillableCommitteeCount(SI.SchoolID) as TotalBillableCommitteeMembers
FROM SchoolInfo SI
WHERE SI.IS_ServiceChargeActive = 1
ORDER BY SI.SchoolName
*/

-- =============================================
-- SQL SERVER AGENT JOB CREATION SCRIPT
-- Run this to create automatic monthly job
-- =============================================
/*
USE msdb;
GO

-- Create job
EXEC dbo.sp_add_job
    @job_name = N'Monthly Student Count and Invoice Generation',
    @enabled = 1,
    @description = N'Automatically generates student count and invoices on 1st of every month';

-- Add job step
EXEC dbo.sp_add_jobstep
    @job_name = N'Monthly Student Count and Invoice Generation',
    @step_name = N'Run Auto Process',
    @subsystem = N'TSQL',
    @command = N'EXEC sp_Monthly_Auto_Process @TargetMonth = NULL',
    @database_name = N'YourDatabaseName', -- CHANGE THIS to your database name
    @retry_attempts = 3,
    @retry_interval = 5;

-- Schedule to run on 1st of every month at 2:00 AM
EXEC dbo.sp_add_schedule
    @schedule_name = N'Monthly on 1st at 2AM',
    @freq_type = 16, -- Monthly
    @freq_interval = 1, -- 1st day of month
    @active_start_time = 020000; -- 2:00 AM

-- Attach schedule to job
EXEC dbo.sp_attach_schedule
    @job_name = N'Monthly Student Count and Invoice Generation',
    @schedule_name = N'Monthly on 1st at 2AM';

-- Add job to local server
EXEC dbo.sp_add_jobserver
    @job_name = N'Monthly Student Count and Invoice Generation',
    @server_name = N'(local)';
*/

PRINT 'All stored procedures created successfully!'
PRINT ''
PRINT '========================================'
PRINT 'COMMITTEE BILLING FEATURE INCLUDED!'
PRINT '========================================'
PRINT 'The system now includes committee members in billing.'
PRINT ''
PRINT 'How Committee Billing Works:'
PRINT '1. Go to: Authority/Free_SMS.aspx'
PRINT '2. For each institution, you will see "Committee Member Bill" section'
PRINT '3. Check the committee categories you want to include in billing'
PRINT '4. Only ACTIVE members from CHECKED and ACTIVE categories will be billed'
PRINT '5. Click "Update All Changes" to save'
PRINT ''
PRINT 'The function fn_GetBillableCommitteeCount() will:'
PRINT '- Count only Active committee members'
PRINT '- From categories where IsIncluded = 1 (Checked)'
PRINT '- And categories where IsActive = 1 (Active)'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Configure committee billing in Authority/Free_SMS.aspx'
PRINT '2. Test the procedures manually (see testing section)'
PRINT '3. Create SQL Server Agent Job (see job creation script)'
PRINT '4. The manual button in UI will still work as backup'
PRINT ''
PRINT 'TEST QUERIES:'
PRINT '- Test 5: View committee billing config per institution'
PRINT '- Test 6: View total billable committee count'
