SET NOCOUNT ON;
IF DB_ID(N'ReceiptSnTest') IS NOT NULL BEGIN ALTER DATABASE ReceiptSnTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ReceiptSnTest; END
GO
CREATE DATABASE ReceiptSnTest;
GO
USE ReceiptSnTest;
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;
GO
CREATE TABLE dbo.Income_MoneyReceipt (
    MoneyReceiptID   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Income_MoneyReceipt PRIMARY KEY,
    SchoolID INT NULL, StudentID INT NULL, RegistrationID INT NULL, StudentClassID INT NULL,
    PaidDate DATETIME NULL, TotalAmount FLOAT NULL, EducationYearID INT NULL, PaymentBy NVARCHAR(128) NULL,
    BankName NVARCHAR(128) NULL, BranchName NVARCHAR(128) NULL, TransactionID INT NULL,
    MoneyReceipt_SN INT NULL, PrintedReceiptNo NVARCHAR(50) NULL, CollectionDate DATETIME NULL);
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
/* seed: school 1208 up to 101900, school 7 up to 100005, school 55 has a DUPLICATE (100010 twice), school 99 has old small numbers (<100000) */
DECLARE @i int = 1;
WHILE @i <= 1900 BEGIN INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (1208, @i, 100000 + @i, GETDATE()); SET @i += 1; END
SET @i = 1; WHILE @i <= 5 BEGIN INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (7, @i, 100000 + @i, GETDATE()); SET @i += 1; END
SET @i = 1; WHILE @i <= 10 BEGIN INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (55, @i, 100000 + @i, GETDATE()); SET @i += 1; END
INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (55, 11, 100010, GETDATE());
INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (99, 1, 17, GETDATE());
INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (99, 2, NULL, GETDATE());
INSERT dbo.Income_MoneyReceipt(SchoolID, StudentID, MoneyReceipt_SN, PaidDate) VALUES (99, 3, NULL, GETDATE());
SELECT SchoolID, COUNT(*) n, MAX(MoneyReceipt_SN) mx FROM dbo.Income_MoneyReceipt GROUP BY SchoolID;
GO
