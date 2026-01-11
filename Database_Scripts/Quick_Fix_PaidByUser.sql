-- Quick Fix: Direct SQL Commands
-- Copy and paste these commands one by one in SQL Server Management Studio

-- 1. Add column if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.AAP_Invoice_Receipt') AND name = 'PaidByUser')
BEGIN
    ALTER TABLE AAP_Invoice_Receipt ADD PaidByUser NVARCHAR(256) NULL
    PRINT 'Column added'
END
GO

-- 2. Update NULL values to 'System'
UPDATE AAP_Invoice_Receipt SET PaidByUser = 'System' WHERE PaidByUser IS NULL
GO

-- 3. Check the results
SELECT TOP 20 
    InvoiceReceipt_SN AS 'Receipt #',
    PaymentBy AS 'Payment By', 
    PaidByUser AS 'Paid By User',
    PaidDate AS 'Date'
FROM AAP_Invoice_Receipt 
ORDER BY PaidDate DESC
GO
