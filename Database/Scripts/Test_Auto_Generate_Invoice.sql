-- ==========================================
-- Manual Test Script: Generate December 2025 Invoices
-- Purpose: Test invoice generation for December 2025
-- ==========================================

-- Test: Generate invoices for December 2025
PRINT 'Testing invoice generation for December 2025...';
PRINT '';

EXEC AAP_Auto_Generate_Monthly_Invoice @TargetMonth = '2025-12-31';

PRINT '';
PRINT '========================================';
PRINT 'Verification: Check Created Invoices';
PRINT '========================================';

-- Show created invoices
SELECT 
    i.InvoiceID,
    i.Invoice_SN,
    i.SchoolID,
    s.SchoolName,
    FORMAT(i.MonthName, 'MMM yyyy') AS InvoiceMonth,
    i.TotalAmount,
    i.Discount,
    i.Unit AS Students,
    i.UnitPrice AS PerStudentRate,
    FORMAT(i.IssuDate, 'dd MMM yyyy') AS IssueDate,
    FORMAT(i.EndDate, 'dd MMM yyyy') AS EndDate,
    i.Due,
    i.IsPaid
FROM AAP_Invoice i
INNER JOIN SchoolInfo s ON i.SchoolID = s.SchoolID
WHERE FORMAT(i.MonthName, 'MMM yyyy') = 'Dec 2025'
ORDER BY i.Invoice_SN;

PRINT '';
PRINT 'Total invoices created: ';
SELECT COUNT(*) AS TotalInvoices
FROM AAP_Invoice
WHERE FORMAT(MonthName, 'MMM yyyy') = 'Dec 2025';
GO
