/*
  Copy login users + one school from Edu -> EduHybrid (same SQL Server instance).
  No student/fee/attendance rows. Safe to re-run (skips existing IDs).

  1. Change @SchoolID to the test institution (Object Explorer: Edu > SchoolInfo).
  2. Keep @CopyAuthority = 1 to also copy Authority / Sub-Authority logins.
  3. Execute while connected to this server (both databases must exist).
  4. Login to Hybrid with that school's Admin user (same password as live).
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_ID(N'Edu') IS NULL OR DB_ID(N'EduHybrid') IS NULL
BEGIN
    RAISERROR(N'Both Edu and EduHybrid must exist on this SQL Server.', 16, 1);
    RETURN;
END;

DECLARE @SchoolID int = 1012;       -- <<< change this
DECLARE @CopyAuthority bit = 1;     -- 1 = also copy Authority users

IF NOT EXISTS (SELECT 1 FROM Edu.dbo.SchoolInfo WHERE SchoolID = @SchoolID)
BEGIN
    RAISERROR(N'SchoolID not found in Edu.SchoolInfo. Change @SchoolID.', 16, 1);
    RETURN;
END;

CREATE TABLE #UserName (UserName nvarchar(256) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY);

INSERT INTO #UserName (UserName)
SELECT DISTINCT RTRIM(UserName)
FROM Edu.dbo.Registration
WHERE SchoolID = @SchoolID
  AND Validation = N'Valid'
  AND NULLIF(RTRIM(UserName), N'') IS NOT NULL;

INSERT INTO #UserName (UserName)
SELECT RTRIM(s.UserName)
FROM Edu.dbo.SchoolInfo AS s
WHERE s.SchoolID = @SchoolID
  AND NULLIF(RTRIM(s.UserName), N'') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM #UserName u WHERE u.UserName = RTRIM(s.UserName));

IF @CopyAuthority = 1
BEGIN
    INSERT INTO #UserName (UserName)
    SELECT DISTINCT RTRIM(r.UserName)
    FROM Edu.dbo.Registration AS r
    WHERE r.SchoolID = 0
      AND r.Validation = N'Valid'
      AND r.Category IN (N'Authority', N'Sub-Authority')
      AND NULLIF(RTRIM(r.UserName), N'') IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM #UserName u WHERE u.UserName = RTRIM(r.UserName));
END;

BEGIN TRAN;

INSERT INTO EduHybrid.dbo.aspnet_Applications (ApplicationName, LoweredApplicationName, ApplicationId, Description)
SELECT s.ApplicationName, s.LoweredApplicationName, s.ApplicationId, s.Description
FROM Edu.dbo.aspnet_Applications AS s
WHERE NOT EXISTS (
    SELECT 1 FROM EduHybrid.dbo.aspnet_Applications t WHERE t.ApplicationId = s.ApplicationId);

INSERT INTO EduHybrid.dbo.aspnet_SchemaVersions (Feature, CompatibleSchemaVersion, IsCurrentVersion)
SELECT s.Feature, s.CompatibleSchemaVersion, s.IsCurrentVersion
FROM Edu.dbo.aspnet_SchemaVersions AS s
WHERE NOT EXISTS (
    SELECT 1 FROM EduHybrid.dbo.aspnet_SchemaVersions t
    WHERE t.Feature = s.Feature AND t.CompatibleSchemaVersion = s.CompatibleSchemaVersion);

INSERT INTO EduHybrid.dbo.aspnet_Roles (ApplicationId, RoleId, RoleName, LoweredRoleName, Description)
SELECT s.ApplicationId, s.RoleId, s.RoleName, s.LoweredRoleName, s.Description
FROM Edu.dbo.aspnet_Roles AS s
WHERE NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.aspnet_Roles t WHERE t.RoleId = s.RoleId);

INSERT INTO EduHybrid.dbo.aspnet_Users
    (ApplicationId, UserId, UserName, LoweredUserName, MobileAlias, IsAnonymous, LastActivityDate)
SELECT s.ApplicationId, s.UserId, s.UserName, s.LoweredUserName, s.MobileAlias, s.IsAnonymous, s.LastActivityDate
FROM Edu.dbo.aspnet_Users AS s
INNER JOIN Edu.dbo.aspnet_Applications AS a ON a.ApplicationId = s.ApplicationId
INNER JOIN #UserName AS n ON n.UserName = s.UserName
WHERE a.LoweredApplicationName = N'/'
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.aspnet_Users t WHERE t.UserId = s.UserId);

INSERT INTO EduHybrid.dbo.aspnet_Membership
    (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, MobilePIN, Email, LoweredEmail,
     PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate,
     LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
     FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart, Comment)
SELECT s.ApplicationId, s.UserId, s.Password, s.PasswordFormat, s.PasswordSalt, s.MobilePIN, s.Email, s.LoweredEmail,
       s.PasswordQuestion, s.PasswordAnswer, s.IsApproved, s.IsLockedOut, s.CreateDate, s.LastLoginDate,
       s.LastPasswordChangedDate, s.LastLockoutDate, s.FailedPasswordAttemptCount, s.FailedPasswordAttemptWindowStart,
       s.FailedPasswordAnswerAttemptCount, s.FailedPasswordAnswerAttemptWindowStart, s.Comment
FROM Edu.dbo.aspnet_Membership AS s
WHERE EXISTS (SELECT 1 FROM EduHybrid.dbo.aspnet_Users u WHERE u.UserId = s.UserId)
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.aspnet_Membership t WHERE t.UserId = s.UserId);

INSERT INTO EduHybrid.dbo.aspnet_UsersInRoles (UserId, RoleId)
SELECT s.UserId, s.RoleId
FROM Edu.dbo.aspnet_UsersInRoles AS s
WHERE EXISTS (SELECT 1 FROM EduHybrid.dbo.aspnet_Users u WHERE u.UserId = s.UserId)
  AND EXISTS (SELECT 1 FROM EduHybrid.dbo.aspnet_Roles r WHERE r.RoleId = s.RoleId)
  AND NOT EXISTS (
      SELECT 1 FROM EduHybrid.dbo.aspnet_UsersInRoles t
      WHERE t.UserId = s.UserId AND t.RoleId = s.RoleId);

SET IDENTITY_INSERT EduHybrid.dbo.SchoolInfo ON;
INSERT INTO EduHybrid.dbo.SchoolInfo (
    SchoolID, SchoolName, SchoolLogo, Institution_Dialog, Established, Principal, AcadamicStaff, Students,
    Address, City, State, LocalArea, PostalCode, Phone, Email, Website, UserName, Validation, Date,
    School_SN, Per_Student_Rate, Device_SN, IS_ServiceChargeActive, Discount, Fixed, Free_SMS,
    Principal_Sign, Teacher_Sign, OnlinePaymentEnable, StoreId, SignatureKey, SchoolNameLogo, AccessGraceUntil)
SELECT
    s.SchoolID, s.SchoolName, s.SchoolLogo, s.Institution_Dialog, s.Established, s.Principal, s.AcadamicStaff, s.Students,
    s.Address, s.City, s.State, s.LocalArea, s.PostalCode, s.Phone, s.Email, s.Website, s.UserName, s.Validation, s.Date,
    s.School_SN, s.Per_Student_Rate, s.Device_SN, s.IS_ServiceChargeActive, s.Discount, s.Fixed, s.Free_SMS,
    s.Principal_Sign, s.Teacher_Sign, ISNULL(s.OnlinePaymentEnable, 0), s.StoreId, s.SignatureKey, s.SchoolNameLogo, s.AccessGraceUntil
FROM Edu.dbo.SchoolInfo AS s
WHERE s.SchoolID = @SchoolID
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.SchoolInfo t WHERE t.SchoolID = s.SchoolID);
SET IDENTITY_INSERT EduHybrid.dbo.SchoolInfo OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Registration ON;
INSERT INTO EduHybrid.dbo.Registration (
    RegistrationID, SchoolID, UserName, Validation, Category, CreateDate, ExpireDate, CommitteeMemberId)
SELECT s.RegistrationID, s.SchoolID, s.UserName, s.Validation, s.Category, s.CreateDate, s.ExpireDate, s.CommitteeMemberId
FROM Edu.dbo.Registration AS s
INNER JOIN #UserName AS n ON n.UserName = RTRIM(s.UserName)
WHERE (s.SchoolID = @SchoolID OR (@CopyAuthority = 1 AND s.SchoolID = 0))
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Registration t WHERE t.RegistrationID = s.RegistrationID);
SET IDENTITY_INSERT EduHybrid.dbo.Registration OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Education_Year ON;
INSERT INTO EduHybrid.dbo.Education_Year (
    EducationYearID, SchoolID, RegistrationID, EducationYear, Status, StartDate, EndDate, IsActive, SN)
SELECT s.EducationYearID, s.SchoolID, s.RegistrationID, s.EducationYear, s.Status, s.StartDate, s.EndDate, s.IsActive, s.SN
FROM Edu.dbo.Education_Year AS s
WHERE s.SchoolID = @SchoolID
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Education_Year t WHERE t.EducationYearID = s.EducationYearID);
SET IDENTITY_INSERT EduHybrid.dbo.Education_Year OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Education_Year_User ON;
INSERT INTO EduHybrid.dbo.Education_Year_User (
    EducationYear_UserID, RegistrationID, EducationYearID, SchoolID)
SELECT s.EducationYear_UserID, s.RegistrationID, s.EducationYearID, s.SchoolID
FROM Edu.dbo.Education_Year_User AS s
WHERE s.SchoolID = @SchoolID
  AND EXISTS (SELECT 1 FROM EduHybrid.dbo.Registration r WHERE r.RegistrationID = s.RegistrationID)
  AND EXISTS (SELECT 1 FROM EduHybrid.dbo.Education_Year y WHERE y.EducationYearID = s.EducationYearID)
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Education_Year_User t WHERE t.EducationYear_UserID = s.EducationYear_UserID);
SET IDENTITY_INSERT EduHybrid.dbo.Education_Year_User OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Admin ON;
INSERT INTO EduHybrid.dbo.Admin (
    AdminID, RegistrationID, SchoolID, FirstName, LastName, FatherName, Gender, Age, Designation,
    DateofBirth, Nationality, NationalIDorPassportNO, Address, City, PostalCode, State, Phone, Email, Date, Image)
SELECT
    s.AdminID, s.RegistrationID, s.SchoolID, s.FirstName, s.LastName, s.FatherName, s.Gender, s.Age, s.Designation,
    s.DateofBirth, s.Nationality, s.NationalIDorPassportNO, s.Address, s.City, s.PostalCode, s.State, s.Phone, s.Email, s.Date, s.Image
FROM Edu.dbo.Admin AS s
WHERE s.SchoolID = @SchoolID
  AND EXISTS (SELECT 1 FROM EduHybrid.dbo.Registration r WHERE r.RegistrationID = s.RegistrationID)
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Admin t WHERE t.AdminID = s.AdminID);
SET IDENTITY_INSERT EduHybrid.dbo.Admin OFF;

IF @CopyAuthority = 1
BEGIN
    SET IDENTITY_INSERT EduHybrid.dbo.Authority_Info ON;
    INSERT INTO EduHybrid.dbo.Authority_Info (
        AuthorityID, RegistrationID, Name, FatherName, Gender, Age, Designation, DateofBirth,
        Nationality, NationalIDorPassportNO, Address, City, Phone, Email, JoiningDate, Image, Insert_Date)
    SELECT
        s.AuthorityID, s.RegistrationID, s.Name, s.FatherName, s.Gender, s.Age, s.Designation, s.DateofBirth,
        s.Nationality, s.NationalIDorPassportNO, s.Address, s.City, s.Phone, s.Email, s.JoiningDate, s.Image, s.Insert_Date
    FROM Edu.dbo.Authority_Info AS s
    WHERE EXISTS (SELECT 1 FROM EduHybrid.dbo.Registration r WHERE r.RegistrationID = s.RegistrationID AND r.SchoolID = 0)
      AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Authority_Info t WHERE t.AuthorityID = s.AuthorityID);
    SET IDENTITY_INSERT EduHybrid.dbo.Authority_Info OFF;
END;

SET IDENTITY_INSERT EduHybrid.dbo.Link_Category ON;
INSERT INTO EduHybrid.dbo.Link_Category (LinkCategoryID, Category, Ascending)
SELECT s.LinkCategoryID, s.Category, s.Ascending
FROM Edu.dbo.Link_Category AS s
WHERE NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Link_Category t WHERE t.LinkCategoryID = s.LinkCategoryID);
SET IDENTITY_INSERT EduHybrid.dbo.Link_Category OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Link_SubCategory ON;
INSERT INTO EduHybrid.dbo.Link_SubCategory (SubCategoryID, LinkCategoryID, SubCategory, Ascending)
SELECT s.SubCategoryID, s.LinkCategoryID, s.SubCategory, s.Ascending
FROM Edu.dbo.Link_SubCategory AS s
WHERE NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Link_SubCategory t WHERE t.SubCategoryID = s.SubCategoryID);
SET IDENTITY_INSERT EduHybrid.dbo.Link_SubCategory OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Link_Pages ON;
INSERT INTO EduHybrid.dbo.Link_Pages (LinkID, LinkCategoryID, SubCategoryID, RoleId, PageURL, PageTitle, Ascending)
SELECT s.LinkID, s.LinkCategoryID, s.SubCategoryID, s.RoleId, s.PageURL, s.PageTitle, s.Ascending
FROM Edu.dbo.Link_Pages AS s
WHERE NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Link_Pages t WHERE t.LinkID = s.LinkID);
SET IDENTITY_INSERT EduHybrid.dbo.Link_Pages OFF;

SET IDENTITY_INSERT EduHybrid.dbo.Link_Users ON;
INSERT INTO EduHybrid.dbo.Link_Users (LinkUserID, SchoolID, RegistrationID, LinkID, UserName)
SELECT s.LinkUserID, s.SchoolID, s.RegistrationID, s.LinkID, s.UserName
FROM Edu.dbo.Link_Users AS s
INNER JOIN #UserName AS n ON n.UserName = RTRIM(s.UserName)
WHERE NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.Link_Users t WHERE t.LinkUserID = s.LinkUserID);
SET IDENTITY_INSERT EduHybrid.dbo.Link_Users OFF;

SET IDENTITY_INSERT EduHybrid.dbo.SMS ON;
INSERT INTO EduHybrid.dbo.SMS (SMSID, SchoolID, SMS_Balance, Masking, Date)
SELECT s.SMSID, s.SchoolID, s.SMS_Balance, s.Masking, s.Date
FROM Edu.dbo.SMS AS s
WHERE s.SchoolID = @SchoolID
  AND NOT EXISTS (SELECT 1 FROM EduHybrid.dbo.SMS t WHERE t.SMSID = s.SMSID);
SET IDENTITY_INSERT EduHybrid.dbo.SMS OFF;

COMMIT TRAN;

PRINT N'Copied school ' + CONVERT(varchar(20), @SchoolID);
SELECT UserName FROM #UserName ORDER BY UserName;
SELECT
    (SELECT COUNT(*) FROM EduHybrid.dbo.aspnet_Users) AS Users,
    (SELECT COUNT(*) FROM EduHybrid.dbo.Registration) AS Registration,
    (SELECT COUNT(*) FROM EduHybrid.dbo.SchoolInfo) AS Schools,
    (SELECT COUNT(*) FROM EduHybrid.dbo.Education_Year) AS Years;

DROP TABLE #UserName;
GO
