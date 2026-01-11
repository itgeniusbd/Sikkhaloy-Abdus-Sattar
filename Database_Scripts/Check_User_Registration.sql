-- Find out what username is associated with different RegistrationIDs
-- This helps understand which user will be recorded when payment is made

SELECT 
    R.RegistrationID,
    R.UserName,
    R.Category,
    R.Validation,
    S.SchoolName,
    R.CreateDate
FROM Registration R
LEFT JOIN SchoolInfo S ON R.SchoolID = S.SchoolID
WHERE R.Category = 'Admin'  -- Authority users
ORDER BY R.CreateDate DESC
GO

-- If you want to check a specific user:
-- Replace 'abdus.satter' with the username you're logged in as
SELECT 
    RegistrationID,
    UserName,
    Category,
    SchoolID
FROM Registration
WHERE UserName = 'abdus.satter'  -- Your login username
GO
