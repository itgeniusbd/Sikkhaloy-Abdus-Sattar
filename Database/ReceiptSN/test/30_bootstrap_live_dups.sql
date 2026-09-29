/* LocalDB ONLY (throwaway database ReceiptSnDupTest). Never run on Edu.
   Recreates Income_MoneyReceipt with the live duplicate pattern of 28 Sep 2026:
   120,000 receipts, MoneyReceiptID 1,050,001..1,170,000, 205 schools (1000..1204) + SchoolID 0,
   and the six historical duplicate pairs (largest MoneyReceiptID involved = 1166495). */
SET NOCOUNT ON;
IF DB_ID(N'ReceiptSnDupTest') IS NOT NULL BEGIN ALTER DATABASE ReceiptSnDupTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ReceiptSnDupTest; END
GO
CREATE DATABASE ReceiptSnDupTest;
GO
USE ReceiptSnDupTest;
GO
IF DB_NAME() <> N'ReceiptSnDupTest' BEGIN RAISERROR(N'not in ReceiptSnDupTest - stopping', 16, 1); SET NOEXEC ON; END
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE dbo.Income_MoneyReceipt (
    MoneyReceiptID   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Income_MoneyReceipt PRIMARY KEY,
    SchoolID INT NULL, StudentID INT NULL, RegistrationID INT NULL, StudentClassID INT NULL,
    PaidDate DATETIME NULL, TotalAmount FLOAT NULL, EducationYearID INT NULL, PaymentBy NVARCHAR(128) NULL,
    BankName NVARCHAR(128) NULL, BranchName NVARCHAR(128) NULL, TransactionID INT NULL,
    MoneyReceipt_SN INT NULL, PrintedReceiptNo NVARCHAR(50) NULL, CollectionDate DATETIME NULL);
/* the two PrintedReceiptNo indexes that exist on live */
CREATE NONCLUSTERED INDEX IX_Income_MoneyReceipt_PrintedReceiptNo ON dbo.Income_MoneyReceipt(PrintedReceiptNo) WHERE PrintedReceiptNo IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UQ_PrintedReceiptNo_SchoolID ON dbo.Income_MoneyReceipt(PrintedReceiptNo, SchoolID) WHERE PrintedReceiptNo IS NOT NULL;
CREATE TABLE dbo.Income_PaymentRecord (PaymentRecordID INT IDENTITY PRIMARY KEY, MoneyReceiptID INT, PaidAmount FLOAT);
CREATE TABLE dbo.Student (StudentID INT PRIMARY KEY, ID NVARCHAR(50), StudentsName NVARCHAR(50));
CREATE TABLE dbo.Admin (AdminID INT IDENTITY PRIMARY KEY, RegistrationID INT, SchoolID INT, FirstName NVARCHAR(128), LastName NVARCHAR(128));
GO
CREATE FUNCTION [dbo].[F_MoneyReceipt_SN](@SchoolID int) RETURNS int AS
BEGIN
    DECLARE @ret int; DECLARE @SN int;
    SELECT @SN = ISNULL(MAX(MoneyReceipt_SN), 0) FROM [Income_MoneyReceipt] where SchoolID=@SchoolID
    if(@SN<100000) SET @ret = 100000 ELSE SET @ret=@SN
    RETURN @ret + 1;
END;
GO
CREATE PROCEDURE [dbo].[MoneyReceipt]
    @StudentID INT, @RegistrationID INT, @StudentClassID INT, @EducationYearID INT,
    @PaymentBy NVARCHAR(128), @PaidDate DATETIME, @SchoolID INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MoneyReceipt_SN INT
    SET @MoneyReceipt_SN = [dbo].[F_MoneyReceipt_SN](@SchoolID)
    INSERT INTO Income_MoneyReceipt (StudentID, RegistrationID, StudentClassID, PaidDate, EducationYearID, PaymentBy, SchoolID, MoneyReceipt_SN, CollectionDate)
    VALUES (@StudentID,@RegistrationID,@StudentClassID,@PaidDate,@EducationYearID,@PaymentBy,@SchoolID,@MoneyReceipt_SN, GETDATE())
    SELECT SCOPE_IDENTITY()
END
GO
SET IDENTITY_INSERT dbo.Income_MoneyReceipt ON;
;WITH n AS (SELECT TOP (120000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i
            FROM sys.all_objects a CROSS JOIN sys.all_objects b)
INSERT dbo.Income_MoneyReceipt (MoneyReceiptID, SchoolID, StudentID, MoneyReceipt_SN, PaidDate, CollectionDate, PaymentBy)
SELECT 1050000 + i, 1000 + (i % 205), i, 100000 + i, '2026-01-01', '2026-01-01', N'seed' FROM n;
SET IDENTITY_INSERT dbo.Income_MoneyReceipt OFF;
/* school 0 (Authority session) receipts 100001..100008 */
UPDATE dbo.Income_MoneyReceipt SET SchoolID = 0, MoneyReceipt_SN = 100000 + (MoneyReceiptID - 1160000)
WHERE MoneyReceiptID BETWEEN 1160001 AND 1160008;
/* the six live duplicate pairs (SchoolID, SN, first id, second id) */
DECLARE @d TABLE (SchoolID int, SN int, a int, b int);
INSERT @d VALUES (0,100009,1166494,1166495),(1144,166376,1070906,1070907),(1144,166556,1071353,1071354),
                 (1144,167809,1101950,1101951),(1162,109026,1142050,1142051),(1185,112344,1064658,1064659);
/* make sure the chosen SN is not used by another row of that school */
UPDATE m SET MoneyReceipt_SN = MoneyReceipt_SN + 500000
FROM dbo.Income_MoneyReceipt m JOIN @d d ON d.SchoolID = m.SchoolID AND d.SN = m.MoneyReceipt_SN
WHERE m.MoneyReceiptID NOT IN (d.a, d.b);
UPDATE m SET SchoolID = d.SchoolID, MoneyReceipt_SN = d.SN, PaymentBy = N'historical-dup'
FROM dbo.Income_MoneyReceipt m JOIN @d d ON m.MoneyReceiptID IN (d.a, d.b);
GO
SELECT COUNT(*) AS receipts, COUNT(DISTINCT SchoolID) AS schools, MAX(MoneyReceiptID) AS max_id FROM dbo.Income_MoneyReceipt;
SELECT SchoolID, MoneyReceipt_SN, COUNT(*) AS copies, MIN(MoneyReceiptID) AS first_id, MAX(MoneyReceiptID) AS last_id
FROM dbo.Income_MoneyReceipt GROUP BY SchoolID, MoneyReceipt_SN HAVING COUNT(*) > 1 ORDER BY SchoolID, MoneyReceipt_SN;
GO
