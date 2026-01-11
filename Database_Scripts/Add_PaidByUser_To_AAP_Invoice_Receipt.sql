-- Add PaidByUser column to AAP_Invoice_Receipt table
-- This will track the username of the user who processed the payment

-- Step 1: Check if column exists
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'dbo.AAP_Invoice_Receipt') 
               AND name = 'PaidByUser')
BEGIN
    ALTER TABLE AAP_Invoice_Receipt
    ADD PaidByUser NVARCHAR(256) NULL
    
    PRINT '? PaidByUser column added successfully to AAP_Invoice_Receipt table'
END
ELSE
BEGIN
    PRINT '? PaidByUser column already exists in AAP_Invoice_Receipt table'
END
GO

-- Step 2: Update existing NULL records with 'System' or logged user
UPDATE AAP_Invoice_Receipt 
SET PaidByUser = CASE 
    WHEN PaidByUser IS NULL THEN 'System' 
    ELSE PaidByUser 
END
WHERE PaidByUser IS NULL
GO

-- Step 3: Verify the changes
SELECT TOP 10 
    InvoiceReceipt_SN,
    TotalAmount,
    PaymentBy,
    PaidByUser,
    PaidDate
FROM AAP_Invoice_Receipt
ORDER BY PaidDate DESC
GO

PRINT '? Script execution completed successfully!'
PRINT 'Please check the results above to verify PaidByUser column values.'
GO
