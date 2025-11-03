-- ==========================================
-- Stored Procedure: MoneyReceipt
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoneyReceipt]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[MoneyReceipt]
END
GO

CREATE PROCEDURE [dbo].[MoneyReceipt]
@StudentID int,
@RegistrationID int,
@StudentClassID int,
@EducationYearID int,
@PaymentBy nvarchar(128),
@PaidDate datetime,
@SchoolID int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @MoneyReceipt_SN int
	SET @MoneyReceipt_SN = [dbo].[F_MoneyReceipt_SN](@SchoolID)

INSERT INTO Income_MoneyReceipt
                         (StudentID, RegistrationID, StudentClassID, PaidDate, EducationYearID, PaymentBy, SchoolID, MoneyReceipt_SN)
VALUES        (@StudentID,@RegistrationID,@StudentClassID,@PaidDate,@EducationYearID,@PaymentBy,@SchoolID,@MoneyReceipt_SN)

Select SCOPE_IDENTITY();
END;

GO
