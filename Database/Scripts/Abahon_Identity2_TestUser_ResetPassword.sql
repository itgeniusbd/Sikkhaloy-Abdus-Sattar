/*
  AbahonDB / AspNetUsers — ASP.NET Identity 2.x
  ------------------------------------------------
  Your existing hash decodes to 49 bytes, version byte 0x00 => Identity 2.x (NOT Core 3+).

  Option A: Reset password on EXISTING user (abahon09@yahoo.com)
  Login password after run: Test@123

  Option B: Insert a NEW test user
  Login: testuser@local.com / Test@123
*/

USE AbahonDB;
GO

-- ============================================================
-- OPTION A: Reset existing user password
-- ============================================================
UPDATE dbo.AspNetUsers
SET PasswordHash = 'AFrh2U9Fyjnm0t4pByPBNqxGnkqanb9YqAgie2uY3w20xmczk3C/J4BfNekJXchHPg==',  -- Test@123
    SecurityStamp = CONVERT(NVARCHAR(128), NEWID())
WHERE Id = '6f51b6e3-4e26-46bd-9831-7efdd7043059';
GO

-- Verify
SELECT Id, UserName, PasswordHash, SecurityStamp
FROM dbo.AspNetUsers
WHERE Id = '6f51b6e3-4e26-46bd-9831-7efdd7043059';
GO


-- ============================================================
-- OPTION B: Create NEW test user (run only if you want a separate account)
-- ============================================================
/*
DECLARE @UserId NVARCHAR(128) = LOWER(CONVERT(NVARCHAR(36), NEWID()));
DECLARE @UserName NVARCHAR(256) = N'testuser@local.com';
DECLARE @PasswordHash NVARCHAR(MAX) = N'AFrh2U9Fyjnm0t4pByPBNqxGnkqanb9YqAgie2uY3w20xmczk3C/J4BfNekJXchHPg=='; -- Test@123
DECLARE @SecurityStamp NVARCHAR(MAX) = LOWER(CONVERT(NVARCHAR(36), NEWID()));

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE UserName = @UserName)
BEGIN
    INSERT INTO dbo.AspNetUsers
    (
        Id,
        UserName,
        PasswordHash,
        SecurityStamp,
        Email,
        EmailConfirmed,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnabled,
        AccessFailedCount
    )
    VALUES
    (
        @UserId,
        @UserName,
        @PasswordHash,
        @SecurityStamp,
        @UserName,          -- Email same as username (optional)
        0,                  -- EmailConfirmed
        0,                  -- PhoneNumberConfirmed
        0,                  -- TwoFactorEnabled
        0,                  -- LockoutEnabled
        0                   -- AccessFailedCount
    );

    PRINT 'Test user created: ' + @UserName + ' / Test@123';
END
ELSE
BEGIN
    PRINT 'User already exists: ' + @UserName;
END
GO
*/
