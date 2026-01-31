-- Script to add CommitteeMemberId column to Registration table if it doesn't exist
-- Run this script first before using the new Donor Login system

-- Check and add CommitteeMemberId to Registration table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Registration') AND name = 'CommitteeMemberId')
BEGIN
    ALTER TABLE Registration
    ADD CommitteeMemberId INT NULL;
    
    PRINT 'CommitteeMemberId column added to Registration table';
END
ELSE
BEGIN
    PRINT 'CommitteeMemberId column already exists in Registration table';
END

-- Check and add SmsNumber to AST table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AST') AND name = 'SmsNumber')
BEGIN
    ALTER TABLE AST
    ADD SmsNumber VARCHAR(20) NULL;
    
    PRINT 'SmsNumber column added to AST table';
END
ELSE
BEGIN
    PRINT 'SmsNumber column already exists in AST table';
END

-- Create index for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Registration_CommitteeMemberId')
BEGIN
    CREATE INDEX IX_Registration_CommitteeMemberId ON Registration(CommitteeMemberId);
    PRINT 'Index created on Registration.CommitteeMemberId';
END

PRINT 'Database schema update completed successfully!';
