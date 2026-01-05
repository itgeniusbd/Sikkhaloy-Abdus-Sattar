-- ==========================================
-- Fix User_Active_Sessions Table
-- This script ensures the table exists with correct structure
-- ==========================================

USE [Edu];
GO

-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'User_Active_Sessions')
BEGIN
    PRINT 'Creating User_Active_Sessions table...';
    
    CREATE TABLE [dbo].[User_Active_Sessions](
        [SessionID] [int] IDENTITY(1,1) NOT NULL,
        [SchoolID] [int] NULL,
        [RegistrationID] [int] NOT NULL,
        [UserName] [nvarchar](256) NOT NULL,
        [Category] [nvarchar](50) NULL,
        [SessionKey] [nvarchar](500) NOT NULL,
        [LastActivity] [datetime] NOT NULL,
        [LoginTime] [datetime] NOT NULL,
     CONSTRAINT [PK_User_Active_Sessions] PRIMARY KEY CLUSTERED 
    (
        [SessionID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    
    PRINT '? Table created successfully!';
END
ELSE
BEGIN
    PRINT 'Table already exists. Checking structure...';
    
    -- Check if SchoolID column allows NULL (should allow NULL for Authority users)
    IF EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'User_Active_Sessions' 
        AND COLUMN_NAME = 'SchoolID' 
        AND IS_NULLABLE = 'NO'
    )
    BEGIN
        PRINT 'Altering SchoolID column to allow NULL...';
        ALTER TABLE User_Active_Sessions ALTER COLUMN SchoolID INT NULL;
        PRINT '? SchoolID column updated to allow NULL';
    END
    
    PRINT '? Table structure is correct!';
END
GO

-- Create index on SessionKey for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Active_Sessions_SessionKey')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_User_Active_Sessions_SessionKey]
    ON [dbo].[User_Active_Sessions] ([SessionKey])
    INCLUDE ([LastActivity]);
    PRINT '? Index on SessionKey created';
END
GO

-- Create index on RegistrationID for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Active_Sessions_RegistrationID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_User_Active_Sessions_RegistrationID]
    ON [dbo].[User_Active_Sessions] ([RegistrationID])
    INCLUDE ([SchoolID], [LastActivity], [LoginTime]);
    PRINT '? Index on RegistrationID created';
END
GO

-- Create index on LastActivity for cleanup queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Active_Sessions_LastActivity')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_User_Active_Sessions_LastActivity]
    ON [dbo].[User_Active_Sessions] ([LastActivity])
    INCLUDE ([SchoolID], [RegistrationID]);
    PRINT '? Index on LastActivity created';
END
GO

-- Verify the table structure
PRINT '';
PRINT '--- Table Structure ---';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'User_Active_Sessions'
ORDER BY ORDINAL_POSITION;
GO

-- Show existing data (if any)
PRINT '';
PRINT '--- Existing Sessions ---';
SELECT COUNT(*) as TotalSessions FROM User_Active_Sessions;
SELECT TOP 10 * FROM User_Active_Sessions ORDER BY LoginTime DESC;
GO

PRINT '';
PRINT '==========================================';
PRINT '? Setup complete! You can now test login.';
PRINT '==========================================';
