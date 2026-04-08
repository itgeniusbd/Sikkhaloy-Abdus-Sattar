-- =============================================
-- ADVANCED SCRIPT - March 2026 Count Based on Attendance
-- Students who were present at least 5 days in March 2026
-- ?? Use this ONLY if you have attendance data
-- =============================================

PRINT '========================================='
PRINT 'ATTENDANCE-BASED COUNT FOR MARCH 2026'
PRINT 'Students with minimum 5 days attendance'
PRINT '========================================='
PRINT ''

-- Check if attendance table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attendance')
BEGIN
    PRINT '? ERROR: Attendance table not found!'
    PRINT 'Please use Fix_March_2026_v2.sql instead (enrollment-based)'
    RETURN
END

-- Delete old data
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
PRINT '? Cleared old data'
PRINT ''

DECLARE @CurrentEducationYearID INT
DECLARE @MonthEnd DATE = '2026-03-31'
DECLARE @MonthStart DATE = '2026-03-01'
DECLARE @MinDays INT = 5 -- Minimum attendance days required

-- Get education year
SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
FROM Education_Year 
WHERE @MonthEnd BETWEEN StartDate AND EndDate 
ORDER BY EducationYearID DESC

IF @CurrentEducationYearID IS NULL
BEGIN
    SELECT TOP 1 @CurrentEducationYearID = EducationYearID 
    FROM Education_Year 
    WHERE GETDATE() BETWEEN StartDate AND EndDate 
END

PRINT 'Education Year: ' + CAST(@CurrentEducationYearID AS NVARCHAR)
PRINT 'Period: Mar 1-31, 2026'
PRINT 'Minimum days: ' + CAST(@MinDays AS NVARCHAR)
PRINT ''

-- Temporary table for eligible students
IF OBJECT_ID('tempdb..#EligibleStudents') IS NOT NULL DROP TABLE #EligibleStudents
CREATE TABLE #EligibleStudents (
    StudentID INT,
    SchoolID INT,
    AttendanceDays INT
)

-- Get students with minimum attendance
INSERT INTO #EligibleStudents
SELECT 
    S.StudentID,
    SC.SchoolID,
    COUNT(DISTINCT A.AttendanceDate) AS AttendanceDays
FROM Student S
INNER JOIN StudentsClass SC ON S.StudentID = SC.StudentID
LEFT JOIN Attendance A ON S.StudentID = A.StudentID
    AND A.AttendanceDate BETWEEN @MonthStart AND @MonthEnd
    AND A.Attendance IN ('Pre', 'Present', 'Late') -- Counted as present
WHERE SC.EducationYearID = @CurrentEducationYearID
AND S.Status = 'Active'
GROUP BY S.StudentID, SC.SchoolID
HAVING COUNT(DISTINCT A.AttendanceDate) >= @MinDays

PRINT 'Found ' + CAST((SELECT COUNT(*) FROM #EligibleStudents) AS NVARCHAR) + ' eligible students'
PRINT ''

-- Class-wise count
INSERT INTO AAP_StudentClass_Count_Monthly 
(SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
SELECT 
    SC.SchoolID,
    SC.ClassID,
    SC.EducationYearID,
    @MonthEnd,
    COUNT(ES.StudentID),
    0,
    0
FROM StudentsClass SC
INNER JOIN #EligibleStudents ES ON SC.StudentID = ES.StudentID AND SC.SchoolID = ES.SchoolID
WHERE SC.EducationYearID = @CurrentEducationYearID
GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID

DECLARE @ClassCount INT = @@ROWCOUNT

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

DECLARE @SchoolCount INT = @@ROWCOUNT

PRINT '? Classes: ' + CAST(@ClassCount AS NVARCHAR)
PRINT '? Schools: ' + CAST(@SchoolCount AS NVARCHAR)
PRINT ''

-- Results
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    SCM.StudentCount,
    SCM.Active_Student,
    SCM.Month
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

DROP TABLE #EligibleStudents
PRINT ''
PRINT 'COMPLETE!'
