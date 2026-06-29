-- Add CollectionDate column only (no backfill - old rows stay NULL, app uses PaidDate as fallback)
USE Edu
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Income_MoneyReceipt' AND COLUMN_NAME = 'CollectionDate'
)
BEGIN
    ALTER TABLE Income_MoneyReceipt
    ADD CollectionDate DATETIME NULL
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('Income_MoneyReceipt')
      AND name = 'DF_Income_MoneyReceipt_CollectionDate'
)
BEGIN
    ALTER TABLE Income_MoneyReceipt
    ADD CONSTRAINT DF_Income_MoneyReceipt_CollectionDate
    DEFAULT GETDATE() FOR CollectionDate
END
GO
