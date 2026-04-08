-- =============================================
-- Diagnostic Queries - ??? ???? ??? Institution Count ???
-- =============================================

PRINT '========================================='
PRINT 'DIAGNOSTIC REPORT - Student Count Issue'
PRINT '========================================='
PRINT ''

-- 1. Current Education Year Check
PRINT '1. CURRENT EDUCATION YEAR:'
SELECT 
    EducationYearID,
    EducationYear,
    StartDate,
    EndDate,
    CASE 
        WHEN GETDATE() BETWEEN StartDate AND EndDate THEN '? ACTIVE'
        ELSE '? INACTIVE'
    END AS Status
FROM Education_Year
WHERE GETDATE() BETWEEN DATEADD(YEAR, -1, StartDate) AND DATEADD(YEAR, 1, EndDate)
ORDER BY StartDate DESC

PRINT ''
PRINT '========================================='

-- 2. How many institutions have students?
PRINT '2. INSTITUTIONS WITH STUDENTS:'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    SI.IS_ServiceChargeActive,
    COUNT(DISTINCT S.StudentID) AS TotalStudents,
    SUM(CASE WHEN S.Status = 'Active' THEN 1 ELSE 0 END) AS ActiveStudents,
    COUNT(DISTINCT SC.ClassID) AS TotalClasses
FROM SchoolInfo SI
LEFT JOIN StudentsClass SC ON SI.SchoolID = SC.SchoolID
LEFT JOIN Student S ON SC.StudentID = S.StudentID
WHERE SC.EducationYearID IN (
    SELECT EducationYearID 
    FROM Education_Year 
    WHERE GETDATE() BETWEEN StartDate AND EndDate
)
GROUP BY SI.SchoolID, SI.SchoolName, SI.IS_ServiceChargeActive
HAVING COUNT(DISTINCT S.StudentID) > 0
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='

-- 3. Which institutions got counted for March 2026?
PRINT '3. MARCH 2026 - COUNTED INSTITUTIONS:'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    ISNULL(SC.StudentCount, 0) AS StudentCount,
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

-- 4. Missing institutions (have students but not counted)
PRINT '4. MISSING INSTITUTIONS (Have students but NOT counted):'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    COUNT(DISTINCT S.StudentID) AS StudentsInDB,
    'NOT COUNTED in March 2026' AS Issue
FROM SchoolInfo SI
INNER JOIN StudentsClass SC ON SI.SchoolID = SC.SchoolID
INNER JOIN Student S ON SC.StudentID = S.StudentID
WHERE SC.EducationYearID IN (
    SELECT EducationYearID 
    FROM Education_Year 
    WHERE GETDATE() BETWEEN StartDate AND EndDate
)
AND NOT EXISTS (
    SELECT 1 FROM AAP_Student_Count_Monthly
    WHERE SchoolID = SI.SchoolID
    AND MONTH(Month) = 3 AND YEAR(Month) = 2026
)
GROUP BY SI.SchoolID, SI.SchoolName
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='

-- 5. Class-wise breakdown for March 2026
PRINT '5. CLASS-WISE COUNT FOR MARCH 2026:'
SELECT 
    SI.SchoolName,
    CC.Class,
    ISNULL(ASCCM.StudentCount, 0) AS ClassStudentCount,
    ISNULL(ASCCM.Active_Student, 0) AS ActiveStudents
FROM AAP_StudentClass_Count_Monthly ASCCM
INNER JOIN SchoolInfo SI ON ASCCM.SchoolID = SI.SchoolID
INNER JOIN CreateClass CC ON ASCCM.ClassID = CC.ClassID
WHERE MONTH(ASCCM.Month) = 3 AND YEAR(ASCCM.Month) = 2026
ORDER BY SI.SchoolID, CC.Class

PRINT ''
PRINT '========================================='

-- 6. Data quality check
PRINT '6. DATA QUALITY CHECK:'

-- Check 1: Education Year
IF EXISTS (SELECT 1 FROM Education_Year WHERE GETDATE() BETWEEN StartDate AND EndDate)
    PRINT '? Active Education Year exists'
ELSE
    PRINT '? No active Education Year found'

-- Check 2: Students
DECLARE @TotalStudents INT
SELECT @TotalStudents = COUNT(*) FROM Student WHERE Status = 'Active'
IF @TotalStudents > 0
    PRINT '? Active students exist: ' + CAST(@TotalStudents AS NVARCHAR)
ELSE
    PRINT '? No active students found'

-- Check 3: StudentsClass mapping
DECLARE @TotalMappings INT
SELECT @TotalMappings = COUNT(*) FROM StudentsClass
IF @TotalMappings > 0
    PRINT '? Student-Class mappings exist: ' + CAST(@TotalMappings AS NVARCHAR)
ELSE
    PRINT '? No Student-Class mappings found'

-- Check 4: Multiple schools
DECLARE @SchoolCount INT
SELECT @SchoolCount = COUNT(DISTINCT SchoolID) FROM StudentsClass
IF @SchoolCount > 1
    PRINT '? Multiple institutions have students: ' + CAST(@SchoolCount AS NVARCHAR)
ELSE
    PRINT '?? Only ' + CAST(@SchoolCount AS NVARCHAR) + ' institution has students'

PRINT ''
PRINT '========================================='
PRINT 'DIAGNOSIS COMPLETE'
PRINT '========================================='
