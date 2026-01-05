-- ==========================================
-- Diagnostic Script for User Active Sessions
-- Run this if sessions are still not being tracked
-- ==========================================

USE [Edu];
GO

PRINT '==========================================';
PRINT 'User Active Sessions Diagnostic Report';
PRINT '==========================================';
PRINT '';

-- 1. Check if table exists
PRINT '1. Checking if table exists...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'User_Active_Sessions')
    PRINT '   ? Table EXISTS';
ELSE
BEGIN
    PRINT '   ? Table DOES NOT EXIST!';
    PRINT '   ACTION: Run Fix_User_Active_Sessions_Table.sql';
END
PRINT '';

-- 2. Check table structure
PRINT '2. Checking table structure...';
SELECT 
    COLUMN_NAME,
    DATA_TYPE + 
    CASE 
        WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL 
        THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
        ELSE ''
    END as DataType,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'User_Active_Sessions'
ORDER BY ORDINAL_POSITION;
PRINT '';

-- 3. Check SchoolID allows NULL
PRINT '3. Checking SchoolID column...';
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'User_Active_Sessions' 
    AND COLUMN_NAME = 'SchoolID' 
    AND IS_NULLABLE = 'YES'
)
    PRINT '   ? SchoolID allows NULL (Good for Authority users)';
ELSE
BEGIN
    PRINT '   ? SchoolID does NOT allow NULL!';
    PRINT '   ACTION: Run Fix_User_Active_Sessions_Table.sql';
END
PRINT '';

-- 4. Check current user permissions
PRINT '4. Checking current user permissions...';
SELECT 
    USER_NAME() as CurrentUser,
    HAS_PERMS_BY_NAME('User_Active_Sessions', 'OBJECT', 'INSERT') as CanInsert,
    HAS_PERMS_BY_NAME('User_Active_Sessions', 'OBJECT', 'DELETE') as CanDelete,
    HAS_PERMS_BY_NAME('User_Active_Sessions', 'OBJECT', 'UPDATE') as CanUpdate,
    HAS_PERMS_BY_NAME('User_Active_Sessions', 'OBJECT', 'SELECT') as CanSelect;

IF HAS_PERMS_BY_NAME('User_Active_Sessions', 'OBJECT', 'INSERT') = 1
    PRINT '   ? INSERT permission granted';
ELSE
BEGIN
    PRINT '   ? INSERT permission DENIED!';
    PRINT '   ACTION: Grant INSERT permission to application user';
END
PRINT '';

-- 5. Check indexes
PRINT '5. Checking indexes...';
SELECT 
    i.name as IndexName,
    i.type_desc as IndexType,
    COL_NAME(ic.object_id, ic.column_id) as ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('User_Active_Sessions')
ORDER BY i.name, ic.key_ordinal;
PRINT '';

-- 6. Check current data
PRINT '6. Checking current data in table...';
DECLARE @RowCount INT;
SELECT @RowCount = COUNT(*) FROM User_Active_Sessions;

PRINT '   Total sessions: ' + CAST(@RowCount AS VARCHAR);

IF @RowCount = 0
BEGIN
    PRINT '   ??  No sessions found!';
    PRINT '   POSSIBLE CAUSES:';
    PRINT '   - Application not restarted after code changes';
    PRINT '   - Users not logged in yet';
    PRINT '   - Insert statement failing silently';
    PRINT '   - Check log file: App_Data/session_tracking_log.txt';
END
ELSE
BEGIN
    PRINT '   ? Sessions are being tracked!';
    PRINT '';
    PRINT '   Recent sessions:';
    SELECT TOP 5
        UserName,
        Category,
        SchoolID,
        LoginTime,
        LastActivity,
        DATEDIFF(MINUTE, LoginTime, GETDATE()) as MinutesAgo
    FROM User_Active_Sessions
    ORDER BY LoginTime DESC;
END
PRINT '';

-- 7. Check for old sessions
PRINT '7. Checking for old sessions (older than 30 minutes)...';
DECLARE @OldCount INT;
SELECT @OldCount = COUNT(*) 
FROM User_Active_Sessions
WHERE DATEDIFF(MINUTE, LastActivity, GETDATE()) > 30;

IF @OldCount > 0
BEGIN
    PRINT '   ??  Found ' + CAST(@OldCount AS VARCHAR) + ' old sessions';
    PRINT '   ACTION: Run cleanup query';
    PRINT '   DELETE FROM User_Active_Sessions WHERE DATEDIFF(MINUTE, LastActivity, GETDATE()) > 30;';
END
ELSE
    PRINT '   ? No old sessions found';
PRINT '';

-- 8. Test INSERT capability
PRINT '8. Testing INSERT capability...';
BEGIN TRY
    DECLARE @TestSession NVARCHAR(500) = 'diagnostic_test_' + CONVERT(NVARCHAR(50), NEWID());
    
    INSERT INTO User_Active_Sessions 
    (SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
    VALUES 
    (1012, 1, 'diagnostic_test', 'Admin', @TestSession, GETDATE(), GETDATE());
    
    IF @@ROWCOUNT > 0
    BEGIN
        PRINT '   ? INSERT test SUCCESSFUL!';
        
        -- Clean up test data
        DELETE FROM User_Active_Sessions WHERE SessionKey = @TestSession;
        PRINT '   ? Test data cleaned up';
    END
    ELSE
        PRINT '   ? INSERT test FAILED (no rows affected)';
END TRY
BEGIN CATCH
    PRINT '   ? INSERT test FAILED with error:';
    PRINT '   ERROR: ' + ERROR_MESSAGE();
    PRINT '   ACTION: Check error message above and fix';
END CATCH
PRINT '';

-- 9. Test Authority user INSERT (SchoolID = NULL)
PRINT '9. Testing Authority user INSERT (SchoolID = NULL)...';
BEGIN TRY
    DECLARE @TestSession2 NVARCHAR(500) = 'diagnostic_authority_' + CONVERT(NVARCHAR(50), NEWID());
    
    INSERT INTO User_Active_Sessions 
    (SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
    VALUES 
    (NULL, 1, 'diagnostic_authority', 'Authority', @TestSession2, GETDATE(), GETDATE());
    
    IF @@ROWCOUNT > 0
    BEGIN
        PRINT '   ? Authority INSERT test SUCCESSFUL!';
        
        -- Clean up test data
        DELETE FROM User_Active_Sessions WHERE SessionKey = @TestSession2;
        PRINT '   ? Test data cleaned up';
    END
    ELSE
        PRINT '   ? Authority INSERT test FAILED (no rows affected)';
END TRY
BEGIN CATCH
    PRINT '   ? Authority INSERT test FAILED with error:';
    PRINT '   ERROR: ' + ERROR_MESSAGE();
    PRINT '   ACTION: Run Fix_User_Active_Sessions_Table.sql to allow NULL SchoolID';
END CATCH
PRINT '';

-- 10. Summary and next steps
PRINT '==========================================';
PRINT 'DIAGNOSTIC SUMMARY';
PRINT '==========================================';
PRINT '';
PRINT 'If INSERT tests passed but no real sessions:';
PRINT '1. Stop Visual Studio debugging (Shift+F5)';
PRINT '2. Rebuild solution (Ctrl+Shift+B)';
PRINT '3. Start debugging again (F5)';
PRINT '4. Login with a user';
PRINT '5. Check log file: App_Data/session_tracking_log.txt';
PRINT '6. Run this query: SELECT * FROM User_Active_Sessions;';
PRINT '';
PRINT 'If INSERT tests failed:';
PRINT '1. Fix errors shown above';
PRINT '2. Run Fix_User_Active_Sessions_Table.sql';
PRINT '3. Grant necessary permissions';
PRINT '4. Run this diagnostic again';
PRINT '';
PRINT '==========================================';
PRINT 'For support, provide:';
PRINT '- Output of this diagnostic script';
PRINT '- Content of App_Data/session_tracking_log.txt';
PRINT '- Screenshots of any errors';
PRINT '==========================================';
