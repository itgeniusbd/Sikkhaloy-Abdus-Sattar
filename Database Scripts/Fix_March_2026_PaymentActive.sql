-- =============================================
-- FINAL FIX - March 2026 (Payment Active Session Only)
-- Count students from PAYMENT ACTIVE sessions only
-- =============================================

PRINT '========================================='
PRINT 'FIXING MARCH 2026 - PAYMENT ACTIVE SESSIONS ONLY'
PRINT '========================================='
PRINT ''

-- Step 1: Clear ALL existing March 2026 data
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
PRINT '? Cleared all existing March 2026 data'
PRINT ''

DECLARE @MonthEnd DATE = '2026-03-31'

-- Step 2: Generate class-wise count (PAYMENT ACTIVE sessions only + ACTIVE institutions)
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
WHERE S.Status = 'Active' -- Only active students
AND EY.IsActive = 1 -- ONLY PAYMENT ACTIVE SESSIONS
AND SI.IS_ServiceChargeActive = 1 -- ONLY ACTIVE INSTITUTIONS
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

DECLARE @ClassCount INT = @@ROWCOUNT
PRINT '? Class-wise: ' + CAST(@ClassCount AS NVARCHAR) + ' classes (from payment active sessions)'

-- Step 3: Generate school-wise total
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
PRINT '? School-wise: ' + CAST(@SchoolCount AS NVARCHAR) + ' institutions'
PRINT ''

-- Step 4: Verification
PRINT '========================================='
PRINT 'VERIFICATION:'
PRINT '========================================='
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    SCM.StudentCount,
    SCM.Active_Student,
    EY.EducationYear AS PaymentActiveSession,
    CONVERT(VARCHAR(10), SCM.Month, 120) AS Month
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
LEFT JOIN (
    -- Get the payment active session for each school
    SELECT SchoolID, EducationYear
    FROM Education_Year
    WHERE IsActive = 1
) EY ON SCM.SchoolID = EY.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'COMPLETE!'
PRINT '========================================='
PRINT 'Counted students from PAYMENT ACTIVE sessions only'
PRINT 'Classes: ' + CAST(@ClassCount AS NVARCHAR)
PRINT 'Institutions: ' + CAST(@SchoolCount AS NVARCHAR)
PRINT ''
PRINT '? Refresh browser to see corrected counts'
