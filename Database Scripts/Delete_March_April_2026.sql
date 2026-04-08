-- =============================================
-- DELETE BOTH March & April 2026 Data
-- Invoices + Student Counts
-- =============================================

PRINT '========================================'
PRINT 'DELETING MARCH & APRIL 2026 DATA'
PRINT '========================================'
PRINT ''

-- ============================================
-- PART 1: DELETE MARCH 2026
-- ============================================
PRINT 'PART 1: Deleting March 2026...'
PRINT ''

-- March Invoices
DELETE FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
AND ISNULL(IsPaid, 0) = 0

PRINT 'March Invoices Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- March Student Counts
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
PRINT 'March School Counts Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
PRINT 'March Class Counts Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

PRINT ''

-- ============================================
-- PART 2: DELETE APRIL 2026
-- ============================================
PRINT 'PART 2: Deleting April 2026...'
PRINT ''

-- April Invoices
DELETE FROM AAP_Invoice
WHERE MONTH(MonthName) = 4 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
AND ISNULL(IsPaid, 0) = 0

PRINT 'April Invoices Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- April Student Counts
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
PRINT 'April School Counts Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
PRINT 'April Class Counts Deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

PRINT ''

-- ============================================
-- VERIFICATION
-- ============================================
PRINT '========================================'
PRINT 'VERIFICATION:'
PRINT '========================================'
PRINT ''

PRINT 'Remaining March 2026:'
SELECT COUNT(*) AS MarchInvoices FROM AAP_Invoice WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
SELECT COUNT(*) AS MarchCounts FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026

PRINT ''
PRINT 'Remaining April 2026:'
SELECT COUNT(*) AS AprilInvoices FROM AAP_Invoice WHERE MONTH(MonthName) = 4 AND YEAR(MonthName) = 2026
SELECT COUNT(*) AS AprilCounts FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026

PRINT ''
PRINT '========================================'
PRINT 'COMPLETE!'
PRINT '========================================'
PRINT 'All wrong March & April 2026 data deleted'
PRINT ''
PRINT 'Next: Run Fix_March_2026_PaymentActive.sql to regenerate correct March data'
