-- =============================================
-- FIXED SCRIPT - Proper Student Count for March 2026
-- Students who were enrolled at least 5 days before month end
-- Logic: Student must be enrolled before March 27, 2026 (to have 5 days in March)
-- =============================================

PRINT '========================================='
PRINT 'REGENERATING MARCH 2026 STUDENT COUNT'
PRINT 'LOGIC: Students enrolled at least 5 days before month end'
PRINT '       (Enrolled before March 27, 2026)'
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
DECLARE @MinDaysInMonth INT = 5
DECLARE @CutoffDate DATE = DATEADD(DAY, -@MinDaysInMonth, @MonthEnd) -- March 26, 2026

PRINT 'Month: March 2026 (2026-03-01 to 2026-03-31)'
PRINT 'Minimum days required in month: ' + CAST(@MinDaysInMonth AS NVARCHAR)
PRINT 'Cutoff date for enrollment: ' + CAST(@CutoffDate AS NVARCHAR) + ' or earlier'
PRINT 'Students enrolled AFTER ' + CAST(@CutoffDate AS NVARCHAR) + ' will NOT be counted'
PRINT ''

-- Get education year for March 2026
SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
FROM Education_Year 
WHERE @MonthEnd BETWEEN StartDate AND EndDate 
ORDER BY EducationYearID DESC

IF @CurrentEducationYearID IS NULL
BEGIN
    SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
    FROM Education_Year 
    WHERE GETDATE() BETWEEN StartDate AND EndDate 
    ORDER BY EducationYearID DESC
END

PRINT 'Education Year ID: ' + CAST(@CurrentEducationYearID AS NVARCHAR)
PRINT ''

-- Step 1: Class-wise count (students enrolled before cutoff date)
INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID,
    SC.ClassID,
    SC.EducationYearID,
    @MonthEnd AS Month,
    COUNT(DISTINCT CASE 
        WHEN S.Status = 'Active' 
        AND S.AdmissionDate <= @CutoffDate 
        THEN S.StudentID 
    END) AS Active_Student,
    0 AS Reject_Countable,
    0 AS Reject_Uncountable
FROM StudentsClass SC
INNER JOIN Student S ON SC.StudentID = S.StudentID
WHERE SC.EducationYearID = @CurrentEducationYearID
AND S.AdmissionDate <= @CutoffDate -- Only students who joined at least 5 days before month end
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
HAVING COUNT(DISTINCT CASE 
    WHEN S.Status = 'Active' 
    AND S.AdmissionDate <= @CutoffDate 
    THEN S.StudentID 
END) > 0

DECLARE @ClassCount INT = @@ROWCOUNT
PRINT '? Class-wise: ' + CAST(@ClassCount AS NVARCHAR) + ' classes with eligible students'
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
PRINT 'VERIFICATION - March 2026 Data:'
PRINT '========================================='
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    ISNULL(SCM.StudentCount, 0) AS StudentCount,
    ISNULL(SCM.Active_Student, 0) AS ActiveStudents,
    CONVERT(VARCHAR(10), SCM.Month, 120) AS Month
FROM SchoolInfo SI
LEFT JOIN AAP_Student_Count_Monthly SCM ON SI.SchoolID = SCM.SchoolID
    AND MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
WHERE SI.SchoolID IN (
    SELECT DISTINCT SchoolID FROM StudentsClass
)
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'BREAKDOWN BY INSTITUTION:'
PRINT '========================================='

-- Detailed breakdown showing enrollment dates
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    COUNT(DISTINCT S.StudentID) AS TotalStudentsInDB,
    COUNT(DISTINCT CASE WHEN S.AdmissionDate <= @CutoffDate THEN S.StudentID END) AS EligibleForMarch,
    COUNT(DISTINCT CASE WHEN S.AdmissionDate > @CutoffDate THEN S.StudentID END) AS TooNew,
    MIN(S.AdmissionDate) AS EarliestAdmission,
    MAX(S.AdmissionDate) AS LatestAdmission
FROM SchoolInfo SI
LEFT JOIN StudentsClass SC ON SI.SchoolID = SC.SchoolID
LEFT JOIN Student S ON SC.StudentID = S.StudentID
WHERE SC.EducationYearID = @CurrentEducationYearID
AND S.Status = 'Active'
GROUP BY SI.SchoolID, SI.SchoolName
HAVING COUNT(DISTINCT S.StudentID) > 0
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'COMPLETE!'
PRINT '========================================='
PRINT 'Summary:'
PRINT '  Classes processed: ' + CAST(@ClassCount AS NVARCHAR)
PRINT '  Institutions counted: ' + CAST(@SchoolCount AS NVARCHAR)
PRINT '  Cutoff date: ' + CAST(@CutoffDate AS NVARCHAR)
PRINT ''
PRINT 'Students enrolled AFTER ' + CAST(@CutoffDate AS NVARCHAR) + ' are NOT counted'
PRINT 'This ensures students had at least ' + CAST(@MinDaysInMonth AS NVARCHAR) + ' days in March 2026'
PRINT ''
PRINT 'Next: Refresh Create Invoice page and select Mar 2026'
