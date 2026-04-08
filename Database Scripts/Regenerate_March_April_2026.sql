-- =============================================
-- REGENERATE CORRECT DATA - March & April 2026
-- Using Payment Active Sessions Only
-- =============================================

PRINT '========================================'
PRINT 'REGENERATING MARCH & APRIL 2026'
PRINT 'Using PAYMENT ACTIVE sessions only'
PRINT '========================================'
PRINT ''

-- ============================================
-- MARCH 2026
-- ============================================
PRINT 'PART 1: Generating March 2026...'
PRINT ''

DECLARE @MonthEnd_Mar DATE = '2026-03-31'

-- Class-wise
INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID, SC.ClassID, SC.EducationYearID, @MonthEnd_Mar,
    SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END), 0, 0
FROM StudentsClass SC
INNER JOIN Student S ON SC.StudentID = S.StudentID
INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
WHERE S.Status = 'Active'
AND EY.IsActive = 1 -- Payment Active
AND SI.IS_ServiceChargeActive = 1 -- Active Institution
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

DECLARE @Mar_Classes INT = @@ROWCOUNT
PRINT 'March Classes: ' + CAST(@Mar_Classes AS VARCHAR)

-- School-wise
INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT SchoolID, @MonthEnd_Mar, SUM(Active_Student), 0, 0
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
GROUP BY SchoolID
HAVING SUM(Active_Student) > 0

DECLARE @Mar_Schools INT = @@ROWCOUNT
PRINT 'March Schools: ' + CAST(@Mar_Schools AS VARCHAR)
PRINT ''

-- ============================================
-- APRIL 2026
-- ============================================
PRINT 'PART 2: Generating April 2026...'
PRINT ''

DECLARE @MonthEnd_Apr DATE = '2026-04-30'

-- Class-wise
INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID, SC.ClassID, SC.EducationYearID, @MonthEnd_Apr,
    SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END), 0, 0
FROM StudentsClass SC
INNER JOIN Student S ON SC.StudentID = S.StudentID
INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
WHERE S.Status = 'Active'
AND EY.IsActive = 1 -- Payment Active
AND SI.IS_ServiceChargeActive = 1 -- Active Institution
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

DECLARE @Apr_Classes INT = @@ROWCOUNT
PRINT 'April Classes: ' + CAST(@Apr_Classes AS VARCHAR)

-- School-wise
INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT SchoolID, @MonthEnd_Apr, SUM(Active_Student), 0, 0
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
GROUP BY SchoolID
HAVING SUM(Active_Student) > 0

DECLARE @Apr_Schools INT = @@ROWCOUNT
PRINT 'April Schools: ' + CAST(@Apr_Schools AS VARCHAR)
PRINT ''

-- ============================================
-- VERIFICATION
-- ============================================
PRINT '========================================'
PRINT 'VERIFICATION:'
PRINT '========================================'
PRINT ''

PRINT 'March 2026 - Top 10 Institutions:'
SELECT TOP 10
    SI.SchoolID,
    SI.SchoolName,
    SCM.Active_Student AS Students
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

PRINT ''
PRINT 'April 2026 - Top 10 Institutions:'
SELECT TOP 10
    SI.SchoolID,
    SI.SchoolName,
    SCM.Active_Student AS Students
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 4 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================'
PRINT 'COMPLETE!'
PRINT '========================================'
PRINT 'March 2026: ' + CAST(@Mar_Schools AS VARCHAR) + ' schools, ' + CAST(@Mar_Classes AS VARCHAR) + ' classes'
PRINT 'April 2026: ' + CAST(@Apr_Schools AS VARCHAR) + ' schools, ' + CAST(@Apr_Classes AS VARCHAR) + ' classes'
PRINT ''
PRINT 'Using PAYMENT ACTIVE sessions only'
PRINT 'Refresh browser to verify counts!'
