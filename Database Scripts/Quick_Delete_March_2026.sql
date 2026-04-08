-- =============================================
-- QUICK DELETE: March 2026 Unpaid Invoices
-- One-click solution
-- =============================================

-- Show before
PRINT 'BEFORE DELETE:'
SELECT COUNT(*) AS TotalInvoices
FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')

-- Delete unpaid
DELETE FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
AND ISNULL(IsPaid, 0) = 0

PRINT 'Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' unpaid invoices'

-- Show after
PRINT 'AFTER DELETE:'
SELECT COUNT(*) AS RemainingInvoices
FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')

PRINT 'DONE!'
