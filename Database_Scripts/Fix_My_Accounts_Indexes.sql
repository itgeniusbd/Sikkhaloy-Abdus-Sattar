SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_PaymentRecord_School_Reg_PaidDate' AND object_id = OBJECT_ID(N'dbo.Income_PaymentRecord'))
    CREATE NONCLUSTERED INDEX IX_Income_PaymentRecord_School_Reg_PaidDate
    ON dbo.Income_PaymentRecord (SchoolID, RegistrationID, PaidDate)
    INCLUDE (PaidAmount, RoleID, AccountID, StudentID, StudentClassID, PayFor, MoneyReceiptID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Extra_Income_School_Reg_Date' AND object_id = OBJECT_ID(N'dbo.Extra_Income'))
    CREATE NONCLUSTERED INDEX IX_Extra_Income_School_Reg_Date
    ON dbo.Extra_Income (SchoolID, RegistrationID, Extra_IncomeDate)
    INCLUDE (Extra_IncomeAmount, Extra_IncomeCategoryID, AccountID, Extra_IncomeFor);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Expenditure_School_Reg_Date' AND object_id = OBJECT_ID(N'dbo.Expenditure'))
    CREATE NONCLUSTERED INDEX IX_Expenditure_School_Reg_Date
    ON dbo.Expenditure (SchoolID, RegistrationID, ExpenseDate)
    INCLUDE (Amount, ExpenseCategoryID, AccountID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employee_Payorder_Records_School_Reg_Date' AND object_id = OBJECT_ID(N'dbo.Employee_Payorder_Records'))
    CREATE NONCLUSTERED INDEX IX_Employee_Payorder_Records_School_Reg_Date
    ON dbo.Employee_Payorder_Records (SchoolID, RegistrationID, Paid_date)
    INCLUDE (Amount, Employee_PayorderID, AccountID, Paid_For);
GO

IF OBJECT_ID(N'dbo.CommitteeMoneyReceipt', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommitteeMoneyReceipt_School_Reg_Date' AND object_id = OBJECT_ID(N'dbo.CommitteeMoneyReceipt'))
    CREATE NONCLUSTERED INDEX IX_CommitteeMoneyReceipt_School_Reg_Date
    ON dbo.CommitteeMoneyReceipt (SchoolId, RegistrationId, PaidDate)
    INCLUDE (TotalAmount, AccountId, CommitteeMoneyReceiptSn);
GO
