-- =============================================
-- SQL Server Permission Setup Script
-- For IIS Application Pool Identity
-- =============================================

-- Instructions:
-- 1. Replace 'YourAppPoolName' with your actual Application Pool name (e.g., 'SIKKHALOY')
-- 2. Replace 'YourDatabaseName' with your actual database name (e.g., 'Edu')
-- 3. Execute this script in SQL Server Management Studio

-- =============================================
-- Configuration Variables
-- Change these values according to your setup
-- =============================================

USE [master]
GO

-- Application Pool name (change this)
DECLARE @AppPoolName NVARCHAR(128) = 'SIKKHALOY'
DECLARE @DatabaseName NVARCHAR(128) = 'Edu'
DECLARE @LoginName NVARCHAR(256) = 'IIS APPPOOL\' + @AppPoolName
DECLARE @SQL NVARCHAR(MAX)

PRINT '========================================='
PRINT 'SQL Server Permission Setup'
PRINT '========================================='
PRINT 'Application Pool: ' + @AppPoolName
PRINT 'Database: ' + @DatabaseName
PRINT 'Login: ' + @LoginName
PRINT '========================================='
PRINT ''

-- =============================================
-- Step 1: Create Login for IIS Application Pool
-- =============================================

PRINT 'Step 1: Creating server login...'

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = @LoginName)
BEGIN
    SET @SQL = 'CREATE LOGIN [' + @LoginName + '] FROM WINDOWS'
    EXEC sp_executesql @SQL
    PRINT '  ? Login created: ' + @LoginName
END
ELSE
BEGIN
    PRINT '  ? Login already exists: ' + @LoginName
END

GO

-- =============================================
-- Step 2: Create Database User
-- =============================================

-- Switch to target database
USE [Edu]  -- Change this to your database name
GO

PRINT ''
PRINT 'Step 2: Creating database user...'

DECLARE @AppPoolName NVARCHAR(128) = 'SIKKHALOY'  -- Change this
DECLARE @LoginName NVARCHAR(256) = 'IIS APPPOOL\' + @AppPoolName
DECLARE @SQL NVARCHAR(MAX)

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = @LoginName)
BEGIN
    SET @SQL = 'CREATE USER [' + @LoginName + '] FOR LOGIN [' + @LoginName + ']'
    EXEC sp_executesql @SQL
    PRINT '  ? User created: ' + @LoginName
END
ELSE
BEGIN
    PRINT '  ? User already exists: ' + @LoginName
END

-- =============================================
-- Step 3: Grant Database Permissions
-- =============================================

PRINT ''
PRINT 'Step 3: Granting permissions...'

-- Add user to db_owner role (full access)
SET @SQL = 'ALTER ROLE [db_owner] ADD MEMBER [' + @LoginName + ']'
EXEC sp_executesql @SQL
PRINT '  ? db_owner role granted'

GO

-- =============================================
-- Final Summary
-- =============================================

PRINT ''
PRINT '========================================='
PRINT 'SQL Permission Setup Completed!'
PRINT '========================================='
PRINT 'The IIS Application Pool can now access the database'
PRINT ''

-- =============================================
-- Verification Query
-- =============================================

PRINT 'Verification:'
PRINT ''

-- Show server login
SELECT 
    'Server Login' AS [Level],
    name AS [Principal],
    type_desc AS [Type],
    create_date AS [Created]
FROM sys.server_principals 
WHERE name LIKE 'IIS APPPOOL\%'

-- Show database user and roles
SELECT 
    'Database User' AS [Level],
    dp.name AS [Principal],
    dp.type_desc AS [Type],
    STRING_AGG(drm.name, ', ') AS [Roles]
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members rm ON dp.principal_id = rm.member_principal_id
LEFT JOIN sys.database_principals drm ON rm.role_principal_id = drm.principal_id
WHERE dp.name LIKE 'IIS APPPOOL\%'
GROUP BY dp.name, dp.type_desc

GO

-- =============================================
-- Alternative: Specific Permissions (More Secure)
-- If you don't want to use db_owner role
-- Uncomment the section below and comment out the db_owner line above
-- =============================================

/*
PRINT ''
PRINT 'Step 3: Granting specific permissions...'

DECLARE @AppPoolName NVARCHAR(128) = 'SIKKHALOY'
DECLARE @LoginName NVARCHAR(256) = 'IIS APPPOOL\' + @AppPoolName
DECLARE @SQL NVARCHAR(MAX)

-- Grant db_datareader role (read access)
SET @SQL = 'ALTER ROLE [db_datareader] ADD MEMBER [' + @LoginName + ']'
EXEC sp_executesql @SQL
PRINT '  ? db_datareader role granted'

-- Grant db_datawriter role (write access)
SET @SQL = 'ALTER ROLE [db_datawriter] ADD MEMBER [' + @LoginName + ']'
EXEC sp_executesql @SQL
PRINT '  ? db_datawriter role granted'

-- Grant EXECUTE permission (stored procedures)
SET @SQL = 'GRANT EXECUTE TO [' + @LoginName + ']'
EXEC sp_executesql @SQL
PRINT '  ? EXECUTE permission granted'

-- Grant VIEW DEFINITION permission
SET @SQL = 'GRANT VIEW DEFINITION TO [' + @LoginName + ']'
EXEC sp_executesql @SQL
PRINT '  ? VIEW DEFINITION permission granted'

PRINT ''
PRINT '========================================='
PRINT 'Specific Permissions Granted Successfully!'
PRINT '========================================='
*/
