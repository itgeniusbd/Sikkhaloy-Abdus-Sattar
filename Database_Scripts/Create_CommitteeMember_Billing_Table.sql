-- =============================================
-- Committee Member Billing Table Creation Script
-- Purpose: Track which committee member categories are included in billing
-- Created: 2025-01-29
-- Updated: 2025-01-29 - Added Active/Inactive status
-- =============================================

USE [Edu]  -- Change to your database name if different
GO

-- Create CommitteeMember_Billing table if it doesn't exist
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
        
        -- Unique constraint to prevent duplicate entries
        CONSTRAINT [UC_School_Category] UNIQUE ([SchoolID], [CommitteeMemberTypeId]),
        
        -- Foreign key constraints (optional, add if you have these tables)
        -- CONSTRAINT [FK_Billing_School] FOREIGN KEY ([SchoolID]) REFERENCES [dbo].[SchoolInfo]([SchoolID]) ON DELETE CASCADE,
        -- CONSTRAINT [FK_Billing_Type] FOREIGN KEY ([CommitteeMemberTypeId]) REFERENCES [dbo].[CommitteeMemberType]([CommitteeMemberTypeId]) ON DELETE CASCADE
    )
    
    PRINT 'CommitteeMember_Billing table created successfully!'
    
    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX [IX_Billing_SchoolID] ON [dbo].[CommitteeMember_Billing] ([SchoolID])
    CREATE NONCLUSTERED INDEX [IX_Billing_CategoryID] ON [dbo].[CommitteeMember_Billing] ([CommitteeMemberTypeId])
    CREATE NONCLUSTERED INDEX [IX_Billing_IsIncluded] ON [dbo].[CommitteeMember_Billing] ([IsIncluded])
    CREATE NONCLUSTERED INDEX [IX_Billing_IsActive] ON [dbo].[CommitteeMember_Billing] ([IsActive])
    
    PRINT 'Indexes created successfully!'
END
ELSE
BEGIN
    PRINT 'CommitteeMember_Billing table already exists.'
    
    -- Check if IsActive column exists, if not add it
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember_Billing') AND name = 'IsActive')
    BEGIN
        PRINT 'Adding IsActive column to existing table...'
        ALTER TABLE [dbo].[CommitteeMember_Billing]
        ADD [IsActive] BIT NOT NULL DEFAULT 1
        
        -- Create index for the new column
        CREATE NONCLUSTERED INDEX [IX_Billing_IsActive] ON [dbo].[CommitteeMember_Billing] ([IsActive])
        
        PRINT 'IsActive column added successfully!'
    END
    ELSE
    BEGIN
        PRINT 'IsActive column already exists.'
    END
END
GO

-- Add Status column to CommitteeMember table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember') AND name = 'Status')
BEGIN
    PRINT 'Adding Status column to CommitteeMember table...'
    ALTER TABLE [dbo].[CommitteeMember]
    ADD [Status] NVARCHAR(20) NOT NULL DEFAULT 'Active'
    
    -- Create index for Status column
    CREATE NONCLUSTERED INDEX [IX_CommitteeMember_Status] ON [dbo].[CommitteeMember] ([Status])
    
    PRINT 'Status column added to CommitteeMember table successfully!'
END
ELSE
BEGIN
    PRINT 'Status column already exists in CommitteeMember table.'
END
GO

-- Sample query to view billing settings with active/inactive status
-- SELECT 
--     SI.SchoolName,
--     CMT.CommitteeMemberType,
--     COUNT(CASE WHEN CM.Status = 'Active' THEN 1 END) as ActiveMemberCount,
--     COUNT(CASE WHEN CM.Status = 'Inactive' THEN 1 END) as InactiveMemberCount,
--     COUNT(CM.CommitteeMemberId) as TotalMembers,
--     ISNULL(CMB.IsIncluded, 0) as IsIncludedInBilling,
--     ISNULL(CMB.IsActive, 1) as IsCategoryActive
-- FROM SchoolInfo SI
-- INNER JOIN CommitteeMemberType CMT ON SI.SchoolID = CMT.SchoolID
-- LEFT JOIN CommitteeMember CM ON CMT.CommitteeMemberTypeId = CM.CommitteeMemberTypeId
-- LEFT JOIN CommitteeMember_Billing CMB ON SI.SchoolID = CMB.SchoolID AND CMT.CommitteeMemberTypeId = CMB.CommitteeMemberTypeId
-- GROUP BY SI.SchoolName, CMT.CommitteeMemberType, CMB.IsIncluded, CMB.IsActive
-- ORDER BY SI.SchoolName, CMT.CommitteeMemberType

PRINT 'Script completed successfully!'
GO
