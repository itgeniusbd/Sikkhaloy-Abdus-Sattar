SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Account_Log_School_Acc_Date' AND object_id = OBJECT_ID(N'dbo.Account_Log'))
    CREATE NONCLUSTERED INDEX IX_Account_Log_School_Acc_Date
    ON dbo.Account_Log (SchoolID, AccountID, Insert_Date)
    INCLUDE (Amount, Add_Subtraction, In_Ex_type, Insert_Up_De, SubCategory, Balance_Before, Balance_After, Insert_Time, ClassOrOtherCategory);
GO
