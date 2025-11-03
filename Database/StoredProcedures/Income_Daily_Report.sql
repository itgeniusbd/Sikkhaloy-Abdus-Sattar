-- ==========================================
-- Stored Procedure: Income_Daily_Report
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Income_Daily_Report]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Income_Daily_Report]
END
GO


CREATE PROCEDURE [dbo].[Income_Daily_Report]
 @SchoolID int ,
 @From_Date date,
 @To_Date date

AS
BEGIN
	SET NOCOUNT ON;

SELECT * FROM(SELECT CAST(Income_PaymentRecord.PaidDate AS DATE) AS In_Date, Income_Roles.Role AS Category, Income_PaymentRecord.PaidAmount AS Amount
FROM  Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE(Income_PaymentRecord.SchoolID = @SchoolID) AND 
(CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))

union all

SELECT  Extra_Income.Extra_IncomeDate AS In_Date, Extra_IncomeCategory.Extra_Income_CategoryName AS Category, Extra_Income.Extra_IncomeAmount AS Amount
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) AND Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')

union all

SELECT CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS In_Date, CommitteeDonationCategory.DonationCategory AS Category, CommitteePaymentRecord.PaidAmount AS Amount
FROM  CommitteePaymentRecord INNER JOIN
                         CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN
                         CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN
                         CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId  
WHERE(CommitteeMoneyReceipt.SchoolID = @SchoolID) AND 
(CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))

) AS T
ORDER BY T.In_Date,T.Category
END;

GO
