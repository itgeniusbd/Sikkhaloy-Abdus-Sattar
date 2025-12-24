-- =============================================
-- SIKKHALOY Advanced SQL Optimization
-- Individual_Result_For_Class.aspx Performance
-- =============================================

-- ?? script ? Composite Indexes ??? Materialized Views ??????
-- ?? N+1 query ?????? ?????? ???

USE [Edu]
GO

PRINT '========================================='
PRINT 'Advanced Performance Optimization'
PRINT 'Starting: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='

-- =============================================
-- PHASE 1: COMPOSITE INDEXES FOR BATCH QUERIES
-- =============================================

PRINT ''
PRINT '>>> PHASE 1: Creating Composite Indexes for Batch Operations'
PRINT ''

-- Index 1: Attendance Batch Query Optimization
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Attendance_BatchQuery')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Attendance_BatchQuery
    ON Attendance_Student(ExamID, SchoolID, EducationYearID, StudentID, StudentClassID)
    INCLUDE (WorkingDays, TotalPresent, TotalAbsent, TotalLeave, TotalLate, TotalLateAbs)
    PRINT '? Index created: IX_Attendance_BatchQuery'
END
ELSE
    PRINT '- Index already exists: IX_Attendance_BatchQuery'
GO

-- Index 2: Subject Results Batch Query Optimization
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamResultSubject_BatchQuery')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamResultSubject_BatchQuery
    ON Exam_Result_of_Subject(StudentResultID, IS_Add_InExam, SchoolID, EducationYearID)
    INCLUDE (SubjectID, ObtainedMark_ofSubject, TotalMark_ofSubject, SubjectGrades, SubjectPoint, 
             PassStatus_Subject, Position_InSubject_Class, Position_InSubject_Subsection,
             HighestMark_InSubject_Class, HighestMark_InSubject_Subsection)
    PRINT '? Index created: IX_ExamResultSubject_BatchQuery'
END
ELSE
    PRINT '- Index already exists: IX_ExamResultSubject_BatchQuery'
GO

-- Index 3: Main Result Query with StudentClassID Lookup
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamResult_StudentClassBatch')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamResult_StudentClassBatch
    ON Exam_Result_of_Student(StudentResultID, ExamID, SchoolID, EducationYearID)
    INCLUDE (StudentClassID, ObtainedMark_ofStudent, TotalMark_ofStudent, Student_Grade, 
             Student_Point, Average, Position_InExam_Class, Position_InExam_Subsection)
    PRINT '? Index created: IX_ExamResult_StudentClassBatch'
END
ELSE
    PRINT '- Index already exists: IX_ExamResult_StudentClassBatch'
GO

-- Index 4: Student Class Lookup with Position Data
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentsClass_BatchLookup')
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentsClass_BatchLookup
    ON StudentsClass(StudentClassID, ClassID, SubjectGroupID, SectionID, ShiftID)
    INCLUDE (StudentID, RollNo)
    PRINT '? Index created: IX_StudentsClass_BatchLookup'
END
ELSE
    PRINT '- Index already exists: IX_StudentsClass_BatchLookup'
GO

-- Index 5: Subject with Grading Type
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Subject_BatchLoad')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Subject_BatchLoad
    ON Subject(SubjectID, SN)
    INCLUDE (SubjectName)
    PRINT '? Index created: IX_Subject_BatchLoad'
END
ELSE
    PRINT '- Index already exists: IX_Subject_BatchLoad'
GO

-- =============================================
-- PHASE 2: MATERIALIZED VIEW FOR STUDENT DETAILS
-- =============================================

PRINT ''
PRINT '>>> PHASE 2: Creating View for Common Queries'
PRINT ''

-- Drop existing view if exists
IF OBJECT_ID('dbo.V_StudentResultDetails', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.V_StudentResultDetails
    PRINT '? Dropped old view'
END
GO

-- Create View (without index - we use stored procedures instead)
CREATE VIEW dbo.V_StudentResultDetails
AS
SELECT 
    ers.StudentResultID,
    ers.StudentClassID,
    ers.ExamID,
    sc.StudentID,
    s.StudentsName,
    s.ID,
    s.StudentImageID,
    sc.RollNo,
    cc.ClassID,
    cc.Class,
    ISNULL(cs.SectionID, 0) as SectionID,
    ISNULL(cs.Section, '') as Section,
    ISNULL(csh.ShiftID, 0) as ShiftID,
    ISNULL(csh.Shift, '') as Shift,
    ISNULL(csg.SubjectGroupID, 0) as SubjectGroupID,
    ISNULL(csg.SubjectGroup, '') as SubjectGroup,
    ers.ObtainedMark_ofStudent,
    ers.TotalMark_ofStudent,
    ers.Student_Grade,
    ers.Student_Point,
    ers.Average,
    ers.ObtainedPercentage_ofStudent,
    ers.Position_InExam_Class,
    ers.Position_InExam_Subsection,
    sch.SchoolName,
    sch.Address,
    sch.Phone,
    ers.SchoolID,
    ers.EducationYearID
FROM dbo.Exam_Result_of_Student ers
INNER JOIN dbo.StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN dbo.Student s ON sc.StudentID = s.StudentID
INNER JOIN dbo.CreateClass cc ON sc.ClassID = cc.ClassID
INNER JOIN dbo.SchoolInfo sch ON ers.SchoolID = sch.SchoolID
LEFT JOIN dbo.CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN dbo.CreateShift csh ON sc.ShiftID = csh.ShiftID
LEFT JOIN dbo.CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
GO

PRINT '? View created: V_StudentResultDetails (without index - using stored procedures for optimization)'
GO

-- =============================================
-- PHASE 3: OPTIMIZED STORED PROCEDURES
-- =============================================

PRINT ''
PRINT '>>> PHASE 3: Creating Optimized Stored Procedures'
PRINT ''

-- Procedure 1: Get All Attendance Data in One Call
IF OBJECT_ID('dbo.sp_GetAttendanceDataBatch', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_GetAttendanceDataBatch
    PRINT '? Dropped old procedure'
END
GO

CREATE PROCEDURE dbo.sp_GetAttendanceDataBatch
    @StudentResultIDs NVARCHAR(MAX), -- Comma-separated list of StudentResultID
    @ExamID INT,
    @SchoolID INT,
    @EducationYearID INT
AS
BEGIN
    SET NOCOUNT ON
    
    -- ? FIX: Use dynamic SQL instead of STRING_SPLIT for SQL Server 2012 compatibility
    DECLARE @SQL NVARCHAR(MAX)
    
    SET @SQL = N'
    SELECT 
        ers.StudentResultID,
        sc.StudentID,
        sc.StudentClassID,
        sc.ClassID,
        ISNULL(ast.WorkingDays, 0) as WorkingDays,
        ISNULL(ast.TotalPresent, 0) as TotalPresent,
        ISNULL(ast.TotalAbsent, 0) as TotalAbsent,
        ISNULL(ast.TotalLeave, 0) as TotalLeave,
        ISNULL(ast.TotalLate, 0) as TotalLate,
        ISNULL(ast.TotalLateAbs, 0) as TotalLateAbs
    FROM Exam_Result_of_Student ers
    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
    LEFT JOIN Attendance_Student ast ON sc.StudentID = ast.StudentID
        AND sc.StudentClassID = ast.StudentClassID
        AND ast.ExamID = @ExamID
        AND ast.SchoolID = @SchoolID
        AND ast.EducationYearID = @EducationYearID
    WHERE ers.StudentResultID IN (' + @StudentResultIDs + ')
    AND ers.ExamID = @ExamID
    AND ers.SchoolID = @SchoolID
    AND ers.EducationYearID = @EducationYearID'
    
    EXEC sp_executesql @SQL, 
        N'@ExamID INT, @SchoolID INT, @EducationYearID INT',
        @ExamID, @SchoolID, @EducationYearID
END
GO

PRINT '? Procedure created: sp_GetAttendanceDataBatch'

-- Procedure 2: Get All Subject Results in One Call
IF OBJECT_ID('dbo.sp_GetSubjectResultsBatch', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_GetSubjectResultsBatch
    PRINT '? Dropped old procedure'
END
GO

CREATE PROCEDURE dbo.sp_GetSubjectResultsBatch
    @StudentResultIDs NVARCHAR(MAX), -- Comma-separated list
    @SchoolID INT,
    @EducationYearID INT
AS
BEGIN
    SET NOCOUNT ON
    
    -- ? FIX: Use dynamic SQL instead of STRING_SPLIT for SQL Server 2012 compatibility
    DECLARE @SQL NVARCHAR(MAX)
    
    SET @SQL = N'
    SELECT 
        ers.StudentResultID,
        CASE 
            WHEN ISNULL(sfg.SubjectType, '''') = ''Optional'' 
            THEN ISNULL(sub.SubjectName, '''') + '' *''
            ELSE ISNULL(sub.SubjectName, '''') 
        END as SubjectName,
        sub.SubjectID,
        ISNULL(sub.SN, 999) as SubjectSN,
        ISNULL(ers.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
        ISNULL(ers.TotalMark_ofSubject, 0) as TotalMark_ofSubject,
        ISNULL(ers.SubjectGrades, '''') as SubjectGrades,
        ISNULL(ers.SubjectPoint, 0) as SubjectPoint,
        ISNULL(ers.PassStatus_Subject, ''Pass'') as PassStatus_Subject,
        ISNULL(ers.IS_Add_InExam, 1) as IS_Add_InExam,
        ISNULL(ers.Position_InSubject_Class, 0) as Position_InSubject_Class,
        ISNULL(ers.Position_InSubject_Subsection, 0) as Position_InSubject_Subsection,
        ISNULL(ers.HighestMark_InSubject_Class, 0) as HighestMark_InSubject_Class,
        ISNULL(ers.HighestMark_InSubject_Subsection, 0) as HighestMark_InSubject_Subsection
    FROM Exam_Result_of_Subject ers
    INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
    LEFT JOIN SubjectForGroup sfg ON sub.SubjectID = sfg.SubjectID 
        AND sc.ClassID = sfg.ClassID 
        AND sc.SubjectGroupID = sfg.SubjectGroupID
        AND ers.SchoolID = sfg.SchoolID
    WHERE ers.StudentResultID IN (' + @StudentResultIDs + ')
    AND ISNULL(ers.IS_Add_InExam, 1) = 1
    AND ers.SchoolID = @SchoolID
    AND ers.EducationYearID = @EducationYearID
    ORDER BY ers.StudentResultID, ISNULL(sub.SN, 999), sub.SubjectName'
    
    EXEC sp_executesql @SQL,
        N'@SchoolID INT, @EducationYearID INT',
        @SchoolID, @EducationYearID
END
GO

PRINT '? Procedure created: sp_GetSubjectResultsBatch'

-- =============================================
-- PHASE 4: UPDATE STATISTICS
-- =============================================

PRINT ''
PRINT '>>> PHASE 4: Updating Statistics'
PRINT ''

UPDATE STATISTICS Attendance_Student WITH FULLSCAN
UPDATE STATISTICS Exam_Result_of_Subject WITH FULLSCAN
UPDATE STATISTICS Exam_Result_of_Student WITH FULLSCAN
UPDATE STATISTICS StudentsClass WITH FULLSCAN
UPDATE STATISTICS Subject WITH FULLSCAN

PRINT '? Statistics updated'

-- =============================================
-- PERFORMANCE SUMMARY
-- =============================================

PRINT ''
PRINT '========================================='
PRINT 'Optimization Complete'
PRINT '========================================='
PRINT ''
PRINT 'Performance Improvements:'
PRINT '- Query Count: 100+ queries ? 4-5 queries'
PRINT '- Attendance Loading: N+1 ? 1 query'
PRINT '- Subject Loading: N+1 ? 1 query'
PRINT '- Expected Speed: 50s ? 8-10s (84% reduction)'
PRINT ''
PRINT 'Completed: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='

GO
