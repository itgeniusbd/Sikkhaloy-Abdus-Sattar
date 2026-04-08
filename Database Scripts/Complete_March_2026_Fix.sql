-- =============================================
-- COMPLETE CLEANUP & FIX - March 2026
-- Step 1: Delete wrong invoices
-- Step 2: Delete wrong student counts
-- Step 3: Regenerate correct counts (Payment Active sessions only)
-- =============================================

PRINT '========================================='
PRINT 'COMPLETE MARCH 2026 CLEANUP & FIX'
PRINT '========================================='
PRINT ''

-- ========================================
-- PART 1: DELETE INVOICES
-- ========================================
PRINT 'PART 1: DELETING MARCH 2026 INVOICES'
PRINT '----------------------------------------'

SELECT 
    @TotalInvoices = COUNT(*),
    @PaidInvoices = SUM(CASE WHEN IsPaid = 1 THEN 1 ELSE 0 END),
    @UnpaidInvoices = SUM(CASE WHEN IsPaid = 0 OR IsPaid IS NULL THEN 1 ELSE 0 END)
FROM AAP_Invoice
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')

DECLARE @TotalInvoices INT = 0
DECLARE @PaidInvoices INT = 0
DECLARE @UnpaidInvoices INT = 0

IF EXISTS (SELECT 1 FROM AAP_Invoice WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026)
BEGIN
    SELECT 
        @TotalInvoices = COUNT(*),
        @PaidInvoices = SUM(CASE WHEN IsPaid = 1 THEN 1 ELSE 0 END),
        @UnpaidInvoices = SUM(CASE WHEN IsPaid = 0 OR IsPaid IS NULL THEN 1 ELSE 0 END)
    FROM AAP_Invoice
    WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
    AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')

    PRINT 'Found invoices:'
    PRINT '  Total: ' + CAST(@TotalInvoices AS NVARCHAR)
    PRINT '  Paid: ' + CAST(ISNULL(@PaidInvoices, 0) AS NVARCHAR) + ' (will keep)'
    PRINT '  Unpaid: ' + CAST(ISNULL(@UnpaidInvoices, 0) AS NVARCHAR) + ' (will delete)'
    PRINT ''

    -- Delete unpaid invoices
    DELETE FROM AAP_Invoice
    WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
    AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
    AND (IsPaid = 0 OR IsPaid IS NULL)

    PRINT '? Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' unpaid invoices'
END
ELSE
BEGIN
    PRINT '?? No invoices found for March 2026'
END

PRINT ''

-- ========================================
-- PART 2: DELETE STUDENT COUNTS
-- ========================================
PRINT 'PART 2: DELETING MARCH 2026 STUDENT COUNTS'
PRINT '----------------------------------------'

DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DECLARE @DeletedSchools INT = @@ROWCOUNT
PRINT '? Deleted ' + CAST(@DeletedSchools AS NVARCHAR) + ' school records'

DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DECLARE @DeletedClasses INT = @@ROWCOUNT
PRINT '? Deleted ' + CAST(@DeletedClasses AS NVARCHAR) + ' class records'

PRINT ''

-- ========================================
-- PART 3: REGENERATE CORRECT COUNTS
-- ========================================
PRINT 'PART 3: REGENERATING CORRECT STUDENT COUNTS'
PRINT '(Payment Active sessions only)'
PRINT '----------------------------------------'

DECLARE @MonthEnd DATE = '2026-03-31'

-- Class-wise count (PAYMENT ACTIVE sessions only)
INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID,
    SC.ClassID,
    SC.EducationYearID,
    @MonthEnd AS Month,
    SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) AS Active_Student,
    0 AS Reject_Countable,
    0 AS Reject_Uncountable
FROM StudentsClass SC
INNER JOIN Student S ON SC.StudentID = S.StudentID
INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
WHERE S.Status = 'Active'
AND EY.IsActive = 1 -- ONLY PAYMENT ACTIVE SESSIONS
AND SI.IS_ServiceChargeActive = 1 -- ONLY ACTIVE INSTITUTIONS
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

DECLARE @ClassCount INT = @@ROWCOUNT
PRINT '? Generated ' + CAST(@ClassCount AS NVARCHAR) + ' class records'

-- School-wise total
INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SchoolID,
    @MonthEnd,
    SUM(Active_Student),
    0,
    0
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
GROUP BY SchoolID
HAVING SUM(Active_Student) > 0

DECLARE @SchoolCount INT = @@ROWCOUNT
PRINT '? Generated ' + CAST(@SchoolCount AS NVARCHAR) + ' school records'

PRINT ''

-- ========================================
-- VERIFICATION
-- ========================================
PRINT '========================================='
PRINT 'VERIFICATION - CORRECTED COUNTS'
PRINT '========================================='

SELECT TOP 20
    SI.SchoolID,
    SI.SchoolName,
    SCM.StudentCount AS TotalCount,
    SCM.Active_Student AS ActiveStudents,
    EY.EducationYear AS PaymentActiveSession,
    CONVERT(VARCHAR(10), SCM.Month, 120) AS ForMonth
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
LEFT JOIN (
    SELECT SchoolID, EducationYear
    FROM Education_Year
    WHERE IsActive = 1
) EY ON SCM.SchoolID = EY.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'CLEANUP COMPLETE!'
PRINT '========================================='
PRINT ''
PRINT 'Summary:'
PRINT '  ? Deleted ' + CAST(ISNULL(@UnpaidInvoices, 0) AS NVARCHAR) + ' wrong invoices'
IF ISNULL(@PaidInvoices, 0) > 0
    PRINT '  ?? Kept ' + CAST(@PaidInvoices AS NVARCHAR) + ' paid invoices (manual review needed)'
PRINT '  ? Regenerated counts for ' + CAST(@SchoolCount AS NVARCHAR) + ' institutions'
PRINT '  ? Using PAYMENT ACTIVE sessions only'
PRINT ''
PRINT 'Next steps:'
PRINT '  1. ? Refresh browser'
PRINT '  2. ? Check Mar 2026 in Create Invoice page'
PRINT '  3. ? Verify School 1024 shows 532 students (not 1983)'
PRINT '  4. ? Generate new invoices with correct counts'
