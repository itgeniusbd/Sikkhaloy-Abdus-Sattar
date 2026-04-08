-- =============================================
-- FIX SCRIPT - Generate Missing Student Counts for March 2026
-- Run this to fix the issue where only 1 institution was counted
-- =============================================

PRINT '========================================='
PRINT 'FIXING MARCH 2026 STUDENT COUNT'
PRINT '========================================='
PRINT ''

-- First, let's see what we're fixing
PRINT 'BEFORE FIX:'
SELECT 
    'Currently counted: ' + CAST(COUNT(*) AS NVARCHAR) + ' institutions' AS Status
FROM AAP_Student_Count_Monthly
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026

PRINT ''
PRINT 'Starting fix process...'
PRINT ''

-- Delete existing March 2026 data (to regenerate clean)
-- UNCOMMENT THESE LINES to delete and regenerate ALL March 2026 data
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
PRINT '✅ Cleared existing March 2026 data'
PRINT ''

-- Get the current education year
DECLARE @CurrentEducationYearID INT
DECLARE @MonthEnd DATE = '2026-03-31'

SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
FROM Education_Year 
WHERE @MonthEnd BETWEEN StartDate AND EndDate 
ORDER BY EducationYearID DESC

IF @CurrentEducationYearID IS NULL
BEGIN
    -- If no exact match, get the most recent active year
    SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
    FROM Education_Year 
    WHERE GETDATE() BETWEEN StartDate AND EndDate 
    ORDER BY EducationYearID DESC
END

PRINT 'Using Education Year ID: ' + CAST(@CurrentEducationYearID AS NVARCHAR)
PRINT ''

-- Generate class-wise student count for ALL institutions
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
HAVING COUNT(S.StudentID) > 0

DECLARE @ClassCount INT = @@ROWCOUNT
PRINT '✅ Generated class-wise count: ' + CAST(@ClassCount AS NVARCHAR) + ' classes'
PRINT ''

-- Generate school-wise total student count for ALL institutions
INSERT INTO AAP_Student_Count_Monthly 
(SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SchoolID,
    @MonthEnd AS Month,
    SUM(Active_Student) AS Active_Student,
    SUM(Reject_Countable) AS Reject_Countable,
    SUM(Reject_Uncountable) AS Reject_Uncountable
FROM AAP_StudentClass_Count_Monthly
WHERE MONTH(Month) = 3 
AND YEAR(Month) = 2026
GROUP BY SchoolID

DECLARE @SchoolCount INT = @@ROWCOUNT
PRINT '✅ Generated school-wise count: ' + CAST(@SchoolCount AS NVARCHAR) + ' institutions'
PRINT ''

-- Show results
PRINT '========================================='
PRINT 'AFTER FIX:'
PRINT '========================================='

SELECT 
    SI.SchoolID,
    SI.SchoolName,
    ISNULL(SC.StudentCount, 0) AS TotalStudents,
    ISNULL(SC.Active_Student, 0) AS ActiveStudents,
    SC.Month
FROM SchoolInfo SI
LEFT JOIN AAP_Student_Count_Monthly SC ON SI.SchoolID = SC.SchoolID
    AND MONTH(SC.Month) = 3 AND YEAR(SC.Month) = 2026
WHERE EXISTS (
    SELECT 1 FROM StudentsClass 
    WHERE SchoolID = SI.SchoolID
)
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'FIX COMPLETE!'
PRINT '========================================='
PRINT ''
PRINT 'Summary:'
PRINT '  - Classes processed: ' + CAST(@ClassCount AS NVARCHAR)
PRINT '  - Institutions counted: ' + CAST(@SchoolCount AS NVARCHAR)
PRINT ''
PRINT 'Next steps:'
PRINT '  1. Refresh the Create Invoice page'
PRINT '  2. Select Mar 2026 from dropdown'
PRINT '  3. You should now see all institutions!'
