-- ============================================================
-- Step 1: Add CollectionDate column to Income_MoneyReceipt
-- Run this ENTIRE file in SSMS (database: Edu)
-- ============================================================

USE Edu
GO

-- 1) Add column (safe - skips if already exists)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Income_MoneyReceipt' AND COLUMN_NAME = 'CollectionDate'
)
BEGIN
    ALTER TABLE Income_MoneyReceipt
    ADD CollectionDate DATETIME NULL
END
GO

-- 2) Add default constraint (safe - skips if already exists)
IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('Income_MoneyReceipt')
      AND name = 'DF_Income_MoneyReceipt_CollectionDate'
)
BEGIN
    ALTER TABLE Income_MoneyReceipt
    ADD CONSTRAINT DF_Income_MoneyReceipt_CollectionDate
    DEFAULT GETDATE() FOR CollectionDate
END
GO

-- Note: Old rows keep CollectionDate NULL (no backfill).
-- App shows ISNULL(CollectionDate, PaidDate) on screen.

-- ============================================================
-- Step 2: Update MoneyReceipt stored procedure
-- ============================================================

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[MoneyReceipt]') AND type IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[MoneyReceipt]
GO

CREATE PROCEDURE [dbo].[MoneyReceipt]
    @StudentID        INT,
    @RegistrationID   INT,
    @StudentClassID   INT,
    @EducationYearID  INT,
    @PaymentBy        NVARCHAR(128),
    @PaidDate         DATETIME,
    @SchoolID         INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MoneyReceipt_SN INT
    SET @MoneyReceipt_SN = [dbo].[F_MoneyReceipt_SN](@SchoolID)

    INSERT INTO Income_MoneyReceipt
        (StudentID, RegistrationID, StudentClassID, PaidDate, EducationYearID, PaymentBy, SchoolID, MoneyReceipt_SN, CollectionDate)
    VALUES
        (@StudentID, @RegistrationID, @StudentClassID, @PaidDate, @EducationYearID, @PaymentBy, @SchoolID, @MoneyReceipt_SN, GETDATE())

    SELECT SCOPE_IDENTITY()
END
GO

PRINT 'Done: CollectionDate column + MoneyReceipt procedure updated successfully.'
GO
