-- Quick Test: School 1024 (????????? ???? ?????? ????? ???? ????)

PRINT '========================================='
PRINT 'TESTING: School ID 1024'
PRINT '========================================='
PRINT ''

-- Show all sessions and their students
PRINT 'ALL SESSIONS (Payment Active status):'
SELECT 
    EY.EducationYear,
    EY.IsActive AS PaymentActive,
    COUNT(DISTINCT S.StudentID) AS TotalStudents
FROM Education_Year EY
LEFT JOIN StudentsClass SC ON EY.EducationYearID = SC.EducationYearID
LEFT JOIN Student S ON SC.StudentID = S.StudentID AND S.Status = 'Active'
WHERE EY.SchoolID = 1024
GROUP BY EY.EducationYear, EY.IsActive
ORDER BY EY.EducationYear DESC

PRINT ''
PRINT '----------------------------------------'
PRINT 'PAYMENT ACTIVE SESSION ONLY:'

-- Show only payment active session
SELECT 
    EY.EducationYear,
    COUNT(DISTINCT S.StudentID) AS ActiveStudents,
    'THIS SHOULD BE BILLED' AS Note
FROM Education_Year EY
INNER JOIN StudentsClass SC ON EY.EducationYearID = SC.EducationYearID
INNER JOIN Student S ON SC.StudentID = S.StudentID
WHERE EY.SchoolID = 1024
AND EY.IsActive = 1 -- Payment Active
AND S.Status = 'Active'
GROUP BY EY.EducationYear

PRINT ''
PRINT '========================================='
PRINT 'EXPECTED RESULT:'
PRINT 'Should show 532 students (from 2026 session only)'
PRINT 'NOT 1983 (which was sum of all sessions)'
PRINT '========================================='
