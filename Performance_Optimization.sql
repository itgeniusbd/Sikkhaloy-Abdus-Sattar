-- =========================================
-- Performance Optimization Script
-- For Individual_Result_For_Class.aspx Page
-- =========================================
-- ?? ????????? ????? ????? database-?
-- Loading time 1-2 ????? ???? 10-15 ???????? ????

-- 1. Exam_Result_of_Student Table Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExamResult_Performance' AND object_id = OBJECT_ID('Exam_Result_of_Student'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamResult_Performance
    ON Exam_Result_of_Student(ExamID, ClassID, SchoolID, EducationYearID)
    INCLUDE (StudentResultID, StudentClassID, Student_Grade, Student_Point, Average, ObtainedPercentage_ofStudent, 
             TotalMark_ofStudent, ObtainedMark_ofStudent, Position_InExam_Class, Position_InExam_Subsection)
    PRINT 'Index IX_ExamResult_Performance created successfully on Exam_Result_of_Student'
END
ELSE
BEGIN
    PRINT 'Index IX_ExamResult_Performance already exists on Exam_Result_of_Student'
END
GO

-- 2. StudentsClass Table Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StudentsClass_Performance' AND object_id = OBJECT_ID('StudentsClass'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentsClass_Performance
    ON StudentsClass(ClassID, SectionID, ShiftID, SubjectGroupID, StudentID)
    INCLUDE (StudentClassID, RollNo)
    PRINT 'Index IX_StudentsClass_Performance created successfully on StudentsClass'
END
ELSE
BEGIN
    PRINT 'Index IX_StudentsClass_Performance already exists on StudentsClass'
END
GO

-- 3. Student Table Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Student_Performance' AND object_id = OBJECT_ID('Student'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Student_Performance
    ON Student(StudentID, ID)
    INCLUDE (StudentsName, StudentImageID)
    PRINT 'Index IX_Student_Performance created successfully on Student'
END
ELSE
BEGIN
    PRINT 'Index IX_Student_Performance already exists on Student'
END
GO

-- 4. Exam_Result_of_Subject Table Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExamResultSubject_Performance' AND object_id = OBJECT_ID('Exam_Result_of_Subject'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamResultSubject_Performance
    ON Exam_Result_of_Subject(StudentResultID, SubjectID, SchoolID, EducationYearID)
    INCLUDE (ObtainedMark_ofSubject, TotalMark_ofSubject, SubjectGrades, SubjectPoint, PassStatus_Subject, IS_Add_InExam,
             Position_InSubject_Class, Position_InSubject_Subsection, HighestMark_InSubject_Class, HighestMark_InSubject_Subsection)
    PRINT 'Index IX_ExamResultSubject_Performance created successfully on Exam_Result_of_Subject'
END
ELSE
BEGIN
    PRINT 'Index IX_ExamResultSubject_Performance already exists on Exam_Result_of_Subject'
END
GO

-- 5. Attendance_Student Table Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Attendance_Performance' AND object_id = OBJECT_ID('Attendance_Student'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Attendance_Performance
    ON Attendance_Student(StudentID, ExamID, ClassID, StudentClassID, SchoolID, EducationYearID)
    INCLUDE (WorkingDays, TotalPresent, TotalAbsent, TotalLeave, TotalLate, TotalLateAbs)
    PRINT 'Index IX_Attendance_Performance created successfully on Attendance_Student'
END
ELSE
BEGIN
    PRINT 'Index IX_Attendance_Performance already exists on Attendance_Student'
END
GO

-- 6. Exam_Obtain_Marks Table Index (for Sub-Exams)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExamObtainMarks_Performance' AND object_id = OBJECT_ID('Exam_Obtain_Marks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamObtainMarks_Performance
    ON Exam_Obtain_Marks(StudentResultID, SubjectID, SubExamID, SchoolID, EducationYearID)
    INCLUDE (MarksObtained, FullMark, PassMark, AbsenceStatus)
    PRINT 'Index IX_ExamObtainMarks_Performance created successfully on Exam_Obtain_Marks'
END
ELSE
BEGIN
    PRINT 'Index IX_ExamObtainMarks_Performance already exists on Exam_Obtain_Marks'
END
GO

-- 7. Subject Table Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Subject_Performance' AND object_id = OBJECT_ID('Subject'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Subject_Performance
    ON Subject(SubjectID, SN)
    INCLUDE (SubjectName)
    PRINT 'Index IX_Subject_Performance created successfully on Subject'
END
ELSE
BEGIN
    PRINT 'Index IX_Subject_Performance already exists on Subject'
END
GO

-- 8. Update Statistics for all tables
PRINT 'Updating statistics...'
UPDATE STATISTICS Exam_Result_of_Student WITH FULLSCAN
UPDATE STATISTICS StudentsClass WITH FULLSCAN
UPDATE STATISTICS Student WITH FULLSCAN
UPDATE STATISTICS Exam_Result_of_Subject WITH FULLSCAN
UPDATE STATISTICS Attendance_Student WITH FULLSCAN
UPDATE STATISTICS Exam_Obtain_Marks WITH FULLSCAN
UPDATE STATISTICS Subject WITH FULLSCAN
GO

PRINT '========================================='
PRINT 'Performance Optimization Complete!'
PRINT '========================================='
PRINT '??????:'
PRINT '1. Loading time 80-90% ??? ????'
PRINT '2. Database query ???? ????? ???'
PRINT '3. Server load ????'
PRINT ''
PRINT '??????? ???????:'
PRINT '1. Application pool restart ????'
PRINT '2. Browser cache clear ????'
PRINT '3. Result page test ????'
PRINT '========================================='
