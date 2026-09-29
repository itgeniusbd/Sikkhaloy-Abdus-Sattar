SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Account_Log_School_SN' AND object_id = OBJECT_ID(N'dbo.Account_Log'))
    CREATE NONCLUSTERED INDEX IX_Account_Log_School_SN
    ON dbo.Account_Log (SchoolID, Log_SN DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_MoneyReceipt_School_SN' AND object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt'))
    CREATE NONCLUSTERED INDEX IX_Income_MoneyReceipt_School_SN
    ON dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_MoneyReceipt_School_Student_Year' AND object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt'))
    CREATE NONCLUSTERED INDEX IX_Income_MoneyReceipt_School_Student_Year
    ON dbo.Income_MoneyReceipt (SchoolID, StudentID, EducationYearID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_PayOrder_School_Student_Status' AND object_id = OBJECT_ID(N'dbo.Income_PayOrder'))
    CREATE NONCLUSTERED INDEX IX_Income_PayOrder_School_Student_Status
    ON dbo.Income_PayOrder (SchoolID, StudentID, Status, EndDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_PaymentRecord_School_Receipt' AND object_id = OBJECT_ID(N'dbo.Income_PaymentRecord'))
    CREATE NONCLUSTERED INDEX IX_Income_PaymentRecord_School_Receipt
    ON dbo.Income_PaymentRecord (SchoolID, MoneyReceiptID);
GO
