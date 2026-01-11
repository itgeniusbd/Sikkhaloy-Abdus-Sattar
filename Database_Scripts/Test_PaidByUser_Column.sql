-- Test Query: Check if PaidByUser column exists and has data
-- Run this query to diagnose the issue

-- Check 1: Does the column exist?
SELECT 
    'Column Exists' AS CheckType,
    CASE 
        WHEN EXISTS (
            SELECT * FROM sys.columns 
            WHERE object_id = OBJECT_ID(N'dbo.AAP_Invoice_Receipt') 
            AND name = 'PaidByUser'
        ) THEN '? YES' 
        ELSE '? NO - Need to add column' 
    END AS Result
GO

-- Check 2: What does the table structure look like?
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AAP_Invoice_Receipt'
ORDER BY ORDINAL_POSITION
GO

-- Check 3: Show sample data
SELECT TOP 5
    InvoiceReceipt_SN,
    PaymentBy,
    PaidByUser,
    PaidDate
FROM AAP_Invoice_Receipt
ORDER BY PaidDate DESC
GO
