SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_PayOrder_School_Active_Year' AND object_id = OBJECT_ID(N'dbo.Income_PayOrder'))
    CREATE NONCLUSTERED INDEX IX_Income_PayOrder_School_Active_Year
    ON dbo.Income_PayOrder (SchoolID, Is_Active, EducationYearID)
    INCLUDE (Amount, LateFeeCountable, Total_Discount, PaidAmount, Receivable_Amount, StartDate, EndDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_PaymentRecord_School_PaidDate' AND object_id = OBJECT_ID(N'dbo.Income_PaymentRecord'))
    CREATE NONCLUSTERED INDEX IX_Income_PaymentRecord_School_PaidDate
    ON dbo.Income_PaymentRecord (SchoolID, PaidDate)
    INCLUDE (PaidAmount, RegistrationID, EducationYearID, RoleID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Extra_Income_School_Date' AND object_id = OBJECT_ID(N'dbo.Extra_Income'))
    CREATE NONCLUSTERED INDEX IX_Extra_Income_School_Date
    ON dbo.Extra_Income (SchoolID, Extra_IncomeDate)
    INCLUDE (Extra_IncomeAmount, RegistrationID, EducationYearID, Extra_IncomeCategoryID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Expenditure_School_Date' AND object_id = OBJECT_ID(N'dbo.Expenditure'))
    CREATE NONCLUSTERED INDEX IX_Expenditure_School_Date
    ON dbo.Expenditure (SchoolID, ExpenseDate)
    INCLUDE (Amount, RegistrationID, EducationYearID, ExpenseCategoryID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employee_Payorder_Records_School_Date' AND object_id = OBJECT_ID(N'dbo.Employee_Payorder_Records'))
    CREATE NONCLUSTERED INDEX IX_Employee_Payorder_Records_School_Date
    ON dbo.Employee_Payorder_Records (SchoolID, Paid_date)
    INCLUDE (Amount, RegistrationID, EducationYearID, Employee_PayorderID);
GO

IF OBJECT_ID(N'dbo.CommitteeMoneyReceipt', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommitteeMoneyReceipt_School_Date' AND object_id = OBJECT_ID(N'dbo.CommitteeMoneyReceipt'))
    CREATE NONCLUSTERED INDEX IX_CommitteeMoneyReceipt_School_Date
    ON dbo.CommitteeMoneyReceipt (SchoolId, PaidDate)
    INCLUDE (TotalAmount, RegistrationID, EducationYearId);
GO

IF OBJECT_ID(N'dbo.CommitteePaymentRecord', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommitteePaymentRecord_School' AND object_id = OBJECT_ID(N'dbo.CommitteePaymentRecord'))
    CREATE NONCLUSTERED INDEX IX_CommitteePaymentRecord_School
    ON dbo.CommitteePaymentRecord (SchoolId)
    INCLUDE (PaidAmount, CommitteeDonationId);
GO
