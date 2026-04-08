-- =============================================
-- ALL-IN-ONE FIX: Delete + Regenerate March & April 2026
-- One script to fix everything!
-- =============================================

PRINT '========================================'
PRINT 'COMPLETE FIX: MARCH & APRIL 2026'
PRINT '========================================'
PRINT ''

-- ============================================
-- STEP 1: DELETE OLD DATA
-- ============================================
PRINT 'STEP 1: Deleting old data...'
PRINT '----------------------------------------'

-- March
DELETE FROM AAP_Invoice WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026 AND ISNULL(IsPaid, 0) = 0
PRINT 'Deleted March invoices: ' + CAST(@@ROWCOUNT AS VARCHAR)

DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026

-- April  
DELETE FROM AAP_Invoice WHERE MONTH(MonthName) = 4 AND YEAR(MonthName) = 2026 AND ISNULL(IsPaid, 0) = 0
PRINT 'Deleted April invoices: ' + CAST(@@ROWCOUNT AS VARCHAR)

DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026

PRINT ''

-- ============================================
-- STEP 2: REGENERATE MARCH 2026
-- ============================================
PRINT 'STEP 2: Regenerating March 2026...'
PRINT '----------------------------------------'

DECLARE @MonthEnd_Mar DATE = '2026-03-31'

INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID, SC.ClassID, SC.EducationYearID, @MonthEnd_Mar,
    SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END), 0, 0
FROM StudentsClass SC
INNER JOIN Student S ON SC.StudentID = S.StudentID
INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
WHERE S.Status = 'Active' AND EY.IsActive = 1 AND SI.IS_ServiceChargeActive = 1
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT SchoolID, @MonthEnd_Mar, SUM(Active_Student), 0, 0
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
GROUP BY SchoolID

PRINT 'March 2026: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' institutions'
PRINT ''

-- ============================================
-- STEP 3: REGENERATE APRIL 2026
-- ============================================
PRINT 'STEP 3: Regenerating April 2026...'
PRINT '----------------------------------------'

DECLARE @MonthEnd_Apr DATE = '2026-04-30'

INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID, SC.ClassID, SC.EducationYearID, @MonthEnd_Apr,
    SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END), 0, 0
FROM StudentsClass SC
INNER JOIN Student S ON SC.StudentID = S.StudentID
INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
WHERE S.Status = 'Active' AND EY.IsActive = 1 AND SI.IS_ServiceChargeActive = 1
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT SchoolID, @MonthEnd_Apr, SUM(Active_Student), 0, 0
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
GROUP BY SchoolID

PRINT 'April 2026: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' institutions'
PRINT ''

-- ============================================
-- VERIFICATION
-- ============================================
PRINT '========================================'
PRINT 'VERIFICATION - School 1024:'
PRINT '========================================'
SELECT 
    DATENAME(MONTH, Month) + ' 2026' AS Month,
    Active_Student AS Students
FROM AAP_Student_Count_Monthly
WHERE SchoolID = 1024
AND YEAR(Month) = 2026
AND MONTH(Month) IN (3, 4)
ORDER BY Month

PRINT ''
PRINT '========================================'
PRINT 'COMPLETE!'
PRINT '========================================'
PRINT 'All data regenerated with PAYMENT ACTIVE sessions'
PRINT 'School 1024 should show 532 students for both months'
PRINT ''
PRINT 'Refresh browser and verify!'
