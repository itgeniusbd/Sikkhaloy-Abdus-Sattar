-- =============================================
-- COMPLETE DATABASE SETUP FOR COMMITTEE BILLING
-- Run this script to setup everything at once
-- Created: 2025-01-29
-- =============================================

USE [Edu]  -- Change to your database name
GO

PRINT '========================================='
PRINT 'Starting Committee Billing Setup...'
PRINT '========================================='
GO

-- =============================================
-- STEP 1: Create CommitteeMember_Billing Table
-- =============================================
PRINT ''
PRINT 'STEP 1: Creating CommitteeMember_Billing table...'
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
BEGIN
    PRINT 'Creating CommitteeMember_Billing table...'
    
    CREATE TABLE [dbo].[CommitteeMember_Billing] (
        [BillingId] INT IDENTITY(1,1) PRIMARY KEY,
        [SchoolID] INT NOT NULL,
        [CommitteeMemberTypeId] INT NOT NULL,
        [IsIncluded] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [UC_School_Category] UNIQUE ([SchoolID], [CommitteeMemberTypeId])
    )
    
    PRINT '? CommitteeMember_Billing table created!'
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_Billing_SchoolID] ON [dbo].[CommitteeMember_Billing] ([SchoolID])
    CREATE NONCLUSTERED INDEX [IX_Billing_CategoryID] ON [dbo].[CommitteeMember_Billing] ([CommitteeMemberTypeId])
    CREATE NONCLUSTERED INDEX [IX_Billing_IsIncluded] ON [dbo].[CommitteeMember_Billing] ([IsIncluded])
    CREATE NONCLUSTERED INDEX [IX_Billing_IsActive] ON [dbo].[CommitteeMember_Billing] ([IsActive])
    
    PRINT '? Indexes created!'
END
ELSE
BEGIN
    PRINT '? CommitteeMember_Billing table already exists.'
    
    -- Check and add IsActive column if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember_Billing') AND name = 'IsActive')
    BEGIN
        PRINT 'Adding IsActive column...'
        ALTER TABLE [dbo].[CommitteeMember_Billing] ADD [IsActive] BIT NOT NULL DEFAULT 1
        CREATE NONCLUSTERED INDEX [IX_Billing_IsActive] ON [dbo].[CommitteeMember_Billing] ([IsActive])
        PRINT '? IsActive column added!'
    END
END
GO

-- =============================================
-- STEP 2: Add Status Column to CommitteeMember
-- =============================================
PRINT ''
PRINT 'STEP 2: Adding Status column to CommitteeMember table...'
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember') AND name = 'Status')
BEGIN
    PRINT 'Adding Status column...'
    ALTER TABLE [dbo].[CommitteeMember] ADD [Status] NVARCHAR(20) NOT NULL DEFAULT 'Active'
    CREATE NONCLUSTERED INDEX [IX_CommitteeMember_Status] ON [dbo].[CommitteeMember] ([Status])
    PRINT '? Status column added to CommitteeMember table!'
END
ELSE
BEGIN
    PRINT '? Status column already exists in CommitteeMember table.'
END
GO

-- =============================================
-- STEP 3: Create fn_GetBillableCommitteeCount Function
-- =============================================
PRINT ''
PRINT 'STEP 3: Creating fn_GetBillableCommitteeCount function...'
GO

-- Drop function if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_GetBillableCommitteeCount]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
    DROP FUNCTION [dbo].[fn_GetBillableCommitteeCount]
    PRINT 'Dropped existing function.'
END
GO

CREATE FUNCTION [dbo].[fn_GetBillableCommitteeCount]
(
    @SchoolID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT = 0
    
    -- Calculate billable committee member count
    -- Only count members where:
    -- 1. Category is included in billing (IsIncluded = 1)
    -- 2. Category is active (IsActive = 1)  
    -- 3. Member is active (Status = 'Active')
    
    SELECT @Count = ISNULL(SUM(MemberCount), 0)
    FROM (
        SELECT COUNT(CM.CommitteeMemberId) as MemberCount
        FROM CommitteeMember_Billing CMB
        INNER JOIN CommitteeMember CM 
            ON CMB.CommitteeMemberTypeId = CM.CommitteeMemberTypeId 
            AND CMB.SchoolID = CM.SchoolID
        WHERE CMB.SchoolID = @SchoolID
          AND CMB.IsIncluded = 1
          AND CMB.IsActive = 1
          AND ISNULL(CM.Status, 'Active') = 'Active'
        GROUP BY CMB.CommitteeMemberTypeId
    ) AS CategoryCounts
    
    RETURN @Count
END
GO

PRINT '? Function fn_GetBillableCommitteeCount created successfully!'
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'Verification:'
PRINT '========================================='
GO

-- Check tables
PRINT ''
PRINT 'Tables:'
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
    PRINT '? CommitteeMember_Billing table exists'
ELSE
    PRINT '? CommitteeMember_Billing table MISSING'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember')
    PRINT '? CommitteeMember table exists'
ELSE
    PRINT '? CommitteeMember table MISSING'

-- Check columns
PRINT ''
PRINT 'Columns:'
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember_Billing') AND name = 'IsActive')
    PRINT '? CommitteeMember_Billing.IsActive column exists'
ELSE
    PRINT '? CommitteeMember_Billing.IsActive column MISSING'

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember') AND name = 'Status')
    PRINT '? CommitteeMember.Status column exists'
ELSE
    PRINT '? CommitteeMember.Status column MISSING'

-- Check function
PRINT ''
PRINT 'Functions:'
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'fn_GetBillableCommitteeCount') AND type IN (N'FN'))
    PRINT '? fn_GetBillableCommitteeCount function exists'
ELSE
    PRINT '? fn_GetBillableCommitteeCount function MISSING'

-- Test function
PRINT ''
PRINT 'Testing function...'
DECLARE @TestSchoolID INT = 1
DECLARE @TestResult INT

SELECT @TestResult = dbo.fn_GetBillableCommitteeCount(@TestSchoolID)
PRINT '? Function test successful! Result for SchoolID ' + CAST(@TestSchoolID AS VARCHAR) + ': ' + CAST(@TestResult AS VARCHAR) + ' members'

PRINT ''
PRINT '========================================='
PRINT 'Setup Complete!'
PRINT '========================================='
PRINT ''
PRINT 'Next Steps:'
PRINT '1. ? Database setup complete'
PRINT '2. ? Build your application'
PRINT '3. ? Test Authority ? Free SMS page'
PRINT '4. ? Test Authority ? Invoice ? Create Invoice'
PRINT ''
GO
