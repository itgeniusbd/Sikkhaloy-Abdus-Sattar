/* LocalDB ONLY. After 30_bootstrap_live_dups.sql: add one NEW duplicate above the reviewed cutoff. */
USE ReceiptSnDupTest; SET NOCOUNT ON;
IF DB_NAME() = N'ReceiptSnDupTest'
    INSERT dbo.Income_MoneyReceipt (SchoolID, StudentID, MoneyReceipt_SN, PaidDate, PaymentBy)
    SELECT SchoolID, StudentID, MoneyReceipt_SN, PaidDate, N'NEW-dup' FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID = 1169990;
SELECT SchoolID, MoneyReceipt_SN, MoneyReceiptID FROM dbo.Income_MoneyReceipt WHERE PaymentBy = N'NEW-dup' OR MoneyReceiptID = 1169990;
