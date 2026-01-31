-- =============================================
-- Add Email Column to CommitteeMember Table
-- =============================================
-- Date: 2025-01-29
-- Purpose: Add Email field for Donor members
-- =============================================

USE [Edu]
GO

-- Check if Email column already exists
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'CommitteeMember' 
    AND COLUMN_NAME = 'Email'
)
BEGIN
    -- Add Email column
    ALTER TABLE [dbo].[CommitteeMember]
    ADD [Email] NVARCHAR(100) NULL;
    
    PRINT 'Email column added successfully to CommitteeMember table!'
END
ELSE
BEGIN
    PRINT 'Email column already exists in CommitteeMember table.'
END
GO

-- Optional: Add index for Email column for faster search
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CommitteeMember_Email')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CommitteeMember_Email]
    ON [dbo].[CommitteeMember] ([Email] ASC)
    WHERE [Email] IS NOT NULL
    
    PRINT 'Index created on Email column!'
END
GO

-- Verify the column was added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CommitteeMember' 
AND COLUMN_NAME = 'Email';

PRINT ''
PRINT '=== Email Column Added Successfully ==='
PRINT 'Column Name: Email'
PRINT 'Data Type: NVARCHAR(100)'
PRINT 'Nullable: YES'
PRINT ''
GO
