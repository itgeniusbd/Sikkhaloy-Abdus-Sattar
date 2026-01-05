-- ==========================================
-- Test User Active Sessions Tracking
-- Use this to verify session tracking is working
-- ==========================================

USE [Edu];
GO

-- 1. Check current active sessions
PRINT '==========================================';
PRINT '1. Current Active Sessions';
PRINT '==========================================';
SELECT 
    SessionID,
    SchoolID,
    RegistrationID,
    UserName,
    Category,
    LEFT(SessionKey, 50) + '...' as SessionKey,
    LastActivity,
    LoginTime,
    DATEDIFF(MINUTE, LastActivity, GETDATE()) as MinutesInactive
FROM User_Active_Sessions
ORDER BY LastActivity DESC;
GO

-- 2. Session statistics by category
PRINT '';
PRINT '==========================================';
PRINT '2. Session Statistics by Category';
PRINT '==========================================';
SELECT 
    Category,
    COUNT(*) as TotalSessions,
    COUNT(CASE WHEN DATEDIFF(MINUTE, LastActivity, GETDATE()) <= 5 THEN 1 END) as OnlineNow,
    COUNT(CASE WHEN DATEDIFF(MINUTE, LastActivity, GETDATE()) <= 15 THEN 1 END) as ActiveUsers,
    COUNT(CASE WHEN DATEDIFF(MINUTE, LastActivity, GETDATE()) <= 60 THEN 1 END) as LastHour,
    COUNT(CASE WHEN CAST(LoginTime AS DATE) = CAST(GETDATE() AS DATE) THEN 1 END) as TodayLogins
FROM User_Active_Sessions
GROUP BY Category;
GO

-- 3. Check for duplicate sessions (same user logged in multiple times)
PRINT '';
PRINT '==========================================';
PRINT '3. Users with Multiple Active Sessions';
PRINT '==========================================';
SELECT 
    RegistrationID,
    UserName,
    Category,
    COUNT(*) as SessionCount
FROM User_Active_Sessions
GROUP BY RegistrationID, UserName, Category
HAVING COUNT(*) > 1;
GO

-- 4. Old sessions (older than 30 minutes)
PRINT '';
PRINT '==========================================';
PRINT '4. Old Sessions (Older than 30 minutes)';
PRINT '==========================================';
SELECT COUNT(*) as OldSessionsCount
FROM User_Active_Sessions
WHERE DATEDIFF(MINUTE, LastActivity, GETDATE()) > 30;
GO

-- 5. Clean up old sessions
PRINT '';
PRINT '==========================================';
PRINT '5. Cleanup Old Sessions';
PRINT '==========================================';
DELETE FROM User_Active_Sessions 
WHERE LastActivity < DATEADD(MINUTE, -30, GETDATE());
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' old sessions deleted';
GO

-- 6. Test manual insert for a test user
PRINT '';
PRINT '==========================================';
PRINT '6. Test Manual Insert';
PRINT '==========================================';
DECLARE @TestSessionKey NVARCHAR(500) = 'test_session_' + CONVERT(NVARCHAR(50), GETDATE(), 121);

-- Try to insert a test session
INSERT INTO User_Active_Sessions 
(SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
VALUES 
(1012, 3188, 'test_user', 'Admin', @TestSessionKey, GETDATE(), GETDATE());

IF @@ROWCOUNT > 0
    PRINT '? Test insert successful! Session tracking is working.';
ELSE
    PRINT '? Test insert failed!';

-- Clean up test data
DELETE FROM User_Active_Sessions WHERE SessionKey = @TestSessionKey;
PRINT 'Test data cleaned up.';
GO

-- 7. Test Authority user (SchoolID = NULL)
PRINT '';
PRINT '==========================================';
PRINT '7. Test Authority User Insert';
PRINT '==========================================';
DECLARE @TestSessionKey2 NVARCHAR(500) = 'test_authority_' + CONVERT(NVARCHAR(50), GETDATE(), 121);

-- Try to insert a test Authority session (SchoolID = NULL)
INSERT INTO User_Active_Sessions 
(SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
VALUES 
(NULL, 3188, 'test_authority', 'Authority', @TestSessionKey2, GETDATE(), GETDATE());

IF @@ROWCOUNT > 0
    PRINT '? Authority user insert successful! SchoolID NULL is allowed.';
ELSE
    PRINT '? Authority user insert failed!';

-- Clean up test data
DELETE FROM User_Active_Sessions WHERE SessionKey = @TestSessionKey2;
PRINT 'Test data cleaned up.';
GO

PRINT '';
PRINT '==========================================';
PRINT '? All tests complete!';
PRINT '==========================================';
PRINT 'Next steps:';
PRINT '1. Login to your application';
PRINT '2. Run: SELECT * FROM User_Active_Sessions';
PRINT '3. Check the log file at: App_Data/session_tracking_log.txt';
PRINT '==========================================';
