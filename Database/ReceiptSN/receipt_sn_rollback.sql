/* =====================================================================
   receipt_sn_rollback.sql  — undo receipt_sn_setup.sql
   - dbo.MoneyReceipt back to the original MAX+1 body (F_MoneyReceipt_SN)
   - drops usp_NextMoneyReceiptSN, usp_SkipMoneyReceiptSN,
     every dbo.Seq_MoneyReceipt_<SchoolID>, and UX_Income_MoneyReceipt_School_SN
   Changes NO receipt rows. Published app code keeps working: V2 calls
   dbo.MoneyReceipt, Hybrid falls back to MAX+1 when the procedure is gone.
   NOTE: after rollback numbers are MAX+1 again, i.e. they can be reused
   after a crash / deletion of the newest receipt (the original problem).
   The lookup index IX_Income_MoneyReceipt_School_SN is left in place
   (it may have existed before the setup script, and it keeps the MAX+1
   lookup of F_MoneyReceipt_SN fast). The unique index is dropped whether it
   was created plain or with the MoneyReceiptID cutoff filter.
   ===================================================================== */
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE [dbo].[MoneyReceipt]
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
DROP PROCEDURE IF EXISTS dbo.usp_NextMoneyReceiptSN;
DROP PROCEDURE IF EXISTS dbo.usp_SkipMoneyReceiptSN;
GO
DECLARE @sql nvarchar(max) = N'', @n int;
SELECT @sql += N'DROP SEQUENCE dbo.' + QUOTENAME(name) + N';' + CHAR(10), @n = ISNULL(@n, 0) + 1
FROM sys.sequences
WHERE schema_id = SCHEMA_ID(N'dbo') AND name LIKE N'Seq[_]MoneyReceipt[_]%';
PRINT N'dropping ' + CONVERT(nvarchar(11), ISNULL(@n, 0)) + N' sequences';
EXEC (@sql);
GO
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND name = N'UX_Income_MoneyReceipt_School_SN')
    DROP INDEX UX_Income_MoneyReceipt_School_SN ON dbo.Income_MoneyReceipt;
GO
SELECT OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN') AS usp_Next_should_be_NULL,
       (SELECT COUNT(*) FROM sys.sequences WHERE name LIKE N'Seq[_]MoneyReceipt[_]%') AS sequences_should_be_0,
       CASE WHEN OBJECT_DEFINITION(OBJECT_ID(N'dbo.MoneyReceipt')) LIKE N'%usp_NextMoneyReceiptSN%' THEN 'STILL NEW' ELSE 'ORIGINAL' END AS MoneyReceipt_body,
       (SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND name = N'UX_Income_MoneyReceipt_School_SN') AS unique_index_should_be_0;
GO
