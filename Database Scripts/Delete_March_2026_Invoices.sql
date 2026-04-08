-- =============================================
-- DELETE March 2026 Invoices (Generated with wrong count)
-- Safe deletion with verification
-- =============================================

PRINT '========================================='
PRINT 'DELETING MARCH 2026 INVOICES'
PRINT 'These invoices were generated with WRONG student counts'
PRINT '========================================='
PRINT ''

-- Step 1: Check what will be deleted
PRINT 'STEP 1: INVOICES TO BE DELETED (March 2026):'
PRINT '----------------------------------------'
SELECT 
    InvoiceID,
    SchoolID,
    SI.SchoolName,
    Invoice_SN,
    Invoice_For,
    CONVERT(VARCHAR(11), IssuDate, 106) AS IssueDate,
    DATENAME(MONTH, MonthName) + ' ' + CAST(YEAR(MonthName) AS VARCHAR) AS BillingMonth,
    Unit AS Students,
    UnitPrice,
    TotalAmount,
    CASE WHEN IsPaid = 1 THEN 'PAID' ELSE 'Unpaid' END AS PaymentStatus,
    CASE WHEN IsPaid = 1 THEN 'CANNOT DELETE' ELSE 'Can Delete' END AS DeleteStatus
FROM AAP_Invoice I
INNER JOIN SchoolInfo SI ON I.SchoolID = SI.SchoolID
WHERE MONTH(MonthName) = 3 
AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
ORDER BY SchoolID

PRINT ''
PRINT '----------------------------------------'

-- Count unpaid vs paid
DECLARE @TotalInvoices INT
DECLARE @PaidInvoices INT
DECLARE @UnpaidInvoices INT

SELECT 
    @TotalInvoices = COUNT(*),
    @PaidInvoices = SUM(CASE WHEN IsPaid = 1 THEN 1 ELSE 0 END),
    @UnpaidInvoices = SUM(CASE WHEN IsPaid = 0 OR IsPaid IS NULL THEN 1 ELSE 0 END)
FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')

PRINT 'Summary:'
PRINT '  Total Invoices: ' + CAST(@TotalInvoices AS NVARCHAR)
PRINT '  Paid Invoices: ' + CAST(@PaidInvoices AS NVARCHAR) + ' ?? WILL NOT DELETE'
PRINT '  Unpaid Invoices: ' + CAST(@UnpaidInvoices AS NVARCHAR) + ' ? WILL DELETE'
PRINT ''

IF @PaidInvoices > 0
BEGIN
    PRINT '?? WARNING: Some invoices are already PAID!'
    PRINT 'Paid invoices will NOT be deleted.'
    PRINT 'You may need to manually reverse payments if needed.'
    PRINT ''
END

PRINT '========================================='
PRINT 'STEP 2: DELETING UNPAID INVOICES'
PRINT '========================================='
PRINT ''

-- Step 2: Delete ONLY UNPAID invoices
DELETE FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
AND (IsPaid = 0 OR IsPaid IS NULL) -- Only delete unpaid invoices

DECLARE @DeletedCount INT = @@ROWCOUNT
PRINT '? Deleted ' + CAST(@DeletedCount AS NVARCHAR) + ' unpaid invoices'
PRINT ''

-- Step 3: Verification
PRINT '========================================='
PRINT 'STEP 3: VERIFICATION'
PRINT '========================================='
PRINT ''

IF EXISTS (
    SELECT 1 FROM AAP_Invoice 
    WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
    AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
)
BEGIN
    PRINT '?? REMAINING INVOICES (Paid ones that were not deleted):'
    SELECT 
        InvoiceID,
        SchoolID,
        Invoice_SN,
        Invoice_For,
        TotalAmount,
        PaidAmount,
        'PAID - NOT DELETED' AS Status
    FROM AAP_Invoice
    WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
    AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
    ORDER BY SchoolID
END
ELSE
BEGIN
    PRINT '? All March 2026 unpaid invoices deleted successfully!'
    PRINT '? No invoices remaining for March 2026'
END

PRINT ''
PRINT '========================================='
PRINT 'COMPLETE!'
PRINT '========================================='
PRINT ''
PRINT 'Summary of actions:'
PRINT '  - Deleted: ' + CAST(@DeletedCount AS NVARCHAR) + ' unpaid invoices'
IF @PaidInvoices > 0
    PRINT '  - Kept: ' + CAST(@PaidInvoices AS NVARCHAR) + ' paid invoices (manual review needed)'
PRINT ''
PRINT 'Next steps:'
PRINT '  1. Run Fix_March_2026_PaymentActive.sql to regenerate correct counts'
PRINT '  2. Generate new invoices with correct student counts'
PRINT '  3. Verify counts match Payment Active sessions'
