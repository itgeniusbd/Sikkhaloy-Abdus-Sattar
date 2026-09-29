/* LocalDB ONLY. After 30_bootstrap_live_dups.sql: remove the 2nd copies -> database without duplicates. */
USE ReceiptSnDupTest; SET NOCOUNT ON;
IF DB_NAME() = N'ReceiptSnDupTest'
    DELETE dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (1166495,1070907,1071354,1101951,1142051,1064659);
