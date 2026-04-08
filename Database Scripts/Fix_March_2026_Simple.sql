-- =============================================
-- SIMPLE FIX - March 2026 Student Count (Without Date Filter)
-- Count ALL active students in current education year
-- =============================================

PRINT '========================================='
PRINT 'REGENERATING MARCH 2026 STUDENT COUNT'
PRINT 'LOGIC: All active students in current year'
PRINT '========================================='
PRINT ''

-- Delete old March 2026 data
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
PRINT '? Cleared old March 2026 data'
PRINT ''

-- Variables
DECLARE @CurrentEducationYearID INT
DECLARE @MonthEnd DATE = '2026-03-31'

-- Get education year for March 2026
SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
FROM Education_Year 
WHERE @MonthEnd BETWEEN StartDate AND EndDate 
ORDER BY EducationYearID DESC

IF @CurrentEducationYearID IS NULL
BEGIN
    PRINT '?? No education year found for March 2026'
    PRINT 'Using current active year instead...'
    SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
    FROM Education_Year 
    WHERE GETDATE() BETWEEN StartDate AND EndDate 
    ORDER BY EducationYearID DESC
END

PRINT 'Education Year ID: ' + CAST(@CurrentEducationYearID AS NVARCHAR)
PRINT ''

-- Step 1: Class-wise count (ALL active students)
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
WHERE SC.EducationYearID = @CurrentEducationYearID
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) > 0

DECLARE @ClassCount INT = @@ROWCOUNT
PRINT '? Class-wise: ' + CAST(@ClassCount AS NVARCHAR) + ' classes'
PRINT ''

-- Step 2: School-wise total
INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SchoolID,
    @MonthEnd,
    SUM(Active_Student),
    SUM(Reject_Countable),
    SUM(Reject_Uncountable)
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
GROUP BY SchoolID
HAVING SUM(Active_Student) > 0

DECLARE @SchoolCount INT = @@ROWCOUNT
PRINT '? School-wise: ' + CAST(@SchoolCount AS NVARCHAR) + ' institutions'
PRINT ''

-- Verification
PRINT '========================================='
PRINT 'RESULTS - March 2026:'
PRINT '========================================='
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    ISNULL(SCM.StudentCount, 0) AS StudentCount,
    ISNULL(SCM.Active_Student, 0) AS ActiveStudents,
    CONVERT(VARCHAR(10), SCM.Month, 120) AS Month
FROM SchoolInfo SI
INNER JOIN AAP_Student_Count_Monthly SCM ON SI.SchoolID = SCM.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'COMPLETE!'
PRINT '========================================='
PRINT 'Classes: ' + CAST(@ClassCount AS NVARCHAR)
PRINT 'Institutions: ' + CAST(@SchoolCount AS NVARCHAR)
PRINT ''
PRINT 'Next: Refresh Create Invoice page ? Select Mar 2026'
