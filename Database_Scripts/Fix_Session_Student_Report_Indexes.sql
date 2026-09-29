SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Income_PayOrder_Session_Student' AND object_id = OBJECT_ID(N'dbo.Income_PayOrder'))
    CREATE NONCLUSTERED INDEX IX_Income_PayOrder_Session_Student
    ON dbo.Income_PayOrder (SchoolID, EducationYearID, Is_Active, EndDate)
    INCLUDE (StudentID, StudentClassID, ClassID, RoleID, Amount, LateFeeCountable, Discount, Total_Discount, PaidAmount, Receivable_Amount, PayFor, PayOrderID);
GO
