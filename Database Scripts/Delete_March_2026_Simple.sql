-- =============================================
-- SIMPLE & SAFE: Delete March 2026 Invoices
-- Compatible with all SQL Server versions
-- =============================================

PRINT '========================================'
PRINT 'DELETING MARCH 2026 INVOICES'
PRINT '========================================'
PRINT ''

-- Step 1: Show what will be deleted
PRINT 'Step 1: Checking invoices...'
PRINT ''

SELECT 
    I.InvoiceID,
    I.SchoolID,
    SI.SchoolName,
    I.Invoice_SN,
    I.Invoice_For,
    I.Unit AS Students,
    I.TotalAmount,
    CASE WHEN I.IsPaid = 1 THEN 'PAID' ELSE 'UNPAID' END AS Status
FROM AAP_Invoice I
INNER JOIN SchoolInfo SI ON I.SchoolID = SI.SchoolID
WHERE MONTH(I.MonthName) = 3 
AND YEAR(I.MonthName) = 2026
AND I.InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
ORDER BY I.SchoolID

-- Count
DECLARE @Total INT = 0
DECLARE @Paid INT = 0
DECLARE @Unpaid INT = 0

SELECT 
    @Total = COUNT(*),
    @Paid = SUM(CASE WHEN IsPaid = 1 THEN 1 ELSE 0 END),
    @Unpaid = SUM(CASE WHEN ISNULL(IsPaid, 0) = 0 THEN 1 ELSE 0 END)
FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 
AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')

PRINT ''
PRINT 'Summary:'
PRINT '  Total: ' + CAST(@Total AS VARCHAR)
PRINT '  Paid: ' + CAST(@Paid AS VARCHAR) + ' (will NOT delete)'
PRINT '  Unpaid: ' + CAST(@Unpaid AS VARCHAR) + ' (WILL delete)'
PRINT ''

IF @Paid > 0
BEGIN
    PRINT 'WARNING: Some invoices are PAID!'
    PRINT 'Paid invoices will be kept.'
    PRINT ''
END

-- Step 2: Delete unpaid invoices
PRINT '========================================'
PRINT 'Step 2: Deleting UNPAID invoices...'
PRINT '========================================'
PRINT ''

DELETE FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 
AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
AND ISNULL(IsPaid, 0) = 0

DECLARE @Deleted INT = @@ROWCOUNT
PRINT 'Deleted: ' + CAST(@Deleted AS VARCHAR) + ' invoices'
PRINT ''

-- Step 3: Verify
PRINT '========================================'
PRINT 'Step 3: Verification'
PRINT '========================================'
PRINT ''

IF EXISTS (
    SELECT 1 FROM AAP_Invoice 
    WHERE MONTH(MonthName) = 3 
    AND YEAR(MonthName) = 2026
    AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
)
BEGIN
    PRINT 'REMAINING INVOICES (Paid - not deleted):'
    SELECT 
        InvoiceID,
        SchoolID,
        Invoice_SN,
        TotalAmount,
        PaidAmount,
        'PAID' AS Status
    FROM AAP_Invoice
    WHERE MONTH(MonthName) = 3 
    AND YEAR(MonthName) = 2026
    AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
END
ELSE
BEGIN
    PRINT 'SUCCESS: All unpaid invoices deleted!'
    PRINT 'No March 2026 invoices remaining.'
END

PRINT ''
PRINT '========================================'
PRINT 'COMPLETE!'
PRINT '========================================'
PRINT 'Deleted: ' + CAST(@Deleted AS VARCHAR) + ' invoices'
IF @Paid > 0
    PRINT 'Kept: ' + CAST(@Paid AS VARCHAR) + ' paid invoices'
PRINT ''
PRINT 'Next: Run Fix_March_2026_PaymentActive.sql'
