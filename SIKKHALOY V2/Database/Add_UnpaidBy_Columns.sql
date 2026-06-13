-- Add columns to track who unpaid the receipt and when
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Income_MoneyReceipt' AND COLUMN_NAME = 'DeletedByRegistrationID'
)
BEGIN
    ALTER TABLE Income_MoneyReceipt
    ADD DeletedByRegistrationID BIGINT NULL
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Income_MoneyReceipt' AND COLUMN_NAME = 'DeletedDate'
)
BEGIN
    ALTER TABLE Income_MoneyReceipt
    ADD DeletedDate DATETIME NULL
END
