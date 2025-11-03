-- ==========================================
-- Stored Procedure: SP_Exam_Student
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Exam_Student]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_Exam_Student]
END
GO

CREATE PROCEDURE [dbo].[SP_Exam_Student]

-- Where condition parameters
	@SchoolID int,
    @EducationYearID int,
	@ClassID int,
	@ExamID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	

---------[[[[[[[[[[[[  Update
--				   TotalSubjest_WithOptional,
--				   TotalSubject,
--                 TotalExamObtainedMark_ofStudent, 
--				   ObtainedMark_ofStudent, 
--				   TotalExamFullMark_ofStudent,
--                 TotalMark_ofStudent,
--				   PassPercentage_Student,
--				   TotalPoint 
--reset----- PassStatus_InSubject ]]]]]]]]]]]]]]]---------

UPDATE       Exam_Result_of_Student
SET         
TotalSubjest_WithOptional = S_T.TotalSubjest_WithOptional,
TotalSubject = S_T.TotalSubject,
TotalExamObtainedMark_ofStudent = ROUND(S_T.TotalExamObtainedMark_ofStudent, 2, 0),
ObtainedMark_ofStudent = ROUND(S_T.ObtainedMark_ofStudent, 2, 0), 
TotalExamFullMark_ofStudent = S_T.TotalExamFullMark_ofStudent,
TotalMark_ofStudent = S_T.TotalMark_ofStudent,
PassPercentage_Student = S_T.PassPercentage_Student,
TotalPoint = S_T.TotalPoint,
PassStatus_InSubject = 'P' 
      
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT        COUNT(SubjectResultID) AS TotalSubjest_WithOptional, COUNT(SubjectResultID) AS TotalSubject, SUM(TotalExamObtainedMark_ofSubject) AS TotalExamObtainedMark_ofStudent, 
                                                         SUM(OMark_ofSub_ConsiderOptional) AS ObtainedMark_ofStudent, SUM(TotalExamFullMark_ofSubject) AS TotalExamFullMark_ofStudent, SUM(TotalMark_ofSubject) AS TotalMark_ofStudent, 
                                                         AVG(PassPercentage_Subject) AS PassPercentage_Student, SUM(SubjectPoint_ConsiderOptional) AS TotalPoint, StudentResultID
                               FROM            Exam_Result_of_Subject
                               WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (IS_Add_InExam = 1)
                               GROUP BY StudentResultID) AS S_T ON Exam_Result_of_Student.StudentResultID = S_T.StudentResultID


---------[[[[[[[[[[[[Update TotalSubject,TotalMark_ofStudent]]]]]]]]]]]]]]]---------
UPDATE       Exam_Result_of_Student
SET                TotalSubject = Sub_T.Total_Sub, TotalMark_ofStudent =Sub_T.TotalMark
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT        Exam_Result_of_Subject.StudentResultID, COUNT(Exam_Result_of_Subject.SubjectID) AS Total_Sub, SUM(Exam_Result_of_Subject.TotalMark_ofSubject) AS TotalMark
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Subject.ExamID = Exam_Publish_Setting.ExamID AND Exam_Result_of_Subject.SchoolID = Exam_Publish_Setting.SchoolID AND 
                         Exam_Result_of_Subject.EducationYearID = Exam_Publish_Setting.EducationYearID AND Exam_Result_of_Subject.ClassID = Exam_Publish_Setting.ClassID
WHERE        (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND 
                         (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Publish_Setting.IS_Add_Optional_Mark_In_FullMarks = 0) AND (Exam_Result_of_Subject.SubjectType = N'Compulsory') AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
GROUP BY Exam_Result_of_Subject.StudentResultID) AS Sub_T ON Exam_Result_of_Student.StudentResultID = Sub_T.StudentResultID




-----------------[[[[[[[[[[[[[[[[[if ObtainedMark_ofStudent > TotalMark_ofStudent]]]]]]]]]]]]]]]]--------------------

UPDATE Exam_Result_of_Student
SET ObtainedMark_ofStudent = TotalMark_ofStudent
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (TotalMark_ofStudent < ObtainedMark_ofStudent)


-----------------[[[[[[[[[[[[[[[[[PassMark_Student------ObtainedPercentage_ofStudent-----PassStatus_Student]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Result_of_Student
SET                PassMark_Student = ROUND(TotalMark_ofStudent * PassPercentage_Student / 100, 2, 0),
                   ObtainedPercentage_ofStudent =  ROUND((ObtainedMark_ofStudent * 100)/  TotalMark_ofStudent, 2, 0),
				   PassStatus_Student =  (case when ROUND((ObtainedMark_ofStudent * 100)/  TotalMark_ofStudent, 2, 0)>= PassPercentage_Student then 'P' else 'F' end)
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)


-----------------[[[[[[[[[[[[[[[[[Student_Point]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Result_of_Student
SET                Student_Point = CASE WHEN Max_P.Max_Point < ROUND(Exam_Result_of_Student.TotalPoint / Exam_Result_of_Student.TotalSubject, 2, 0) 
                         THEN Max_P.Max_Point ELSE ROUND(Exam_Result_of_Student.TotalPoint / Exam_Result_of_Student.TotalSubject, 2, 0) END
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                               FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
                               WHERE (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
                               GROUP BY Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID) AS Max_P ON Exam_Result_of_Student.SchoolID = Max_P.SchoolID AND Exam_Result_of_Student.EducationYearID = Max_P.EducationYearID
WHERE        (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND 
                         (Exam_Result_of_Student.ExamID = @ExamID)


-----------------[[[[[[[[[[[[[[[[[StudentAbsenceStatus]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Result_of_Student
SET              StudentAbsenceStatus = 'Present'
FROM            Exam_Result_of_Student INNER JOIN
                         Exam_Result_of_Subject ON Exam_Result_of_Student.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Result_of_Student.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Result_of_Student.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Result_of_Student.StudentClassID = Exam_Result_of_Subject.StudentClassID AND 
                         Exam_Result_of_Student.StudentID = Exam_Result_of_Subject.StudentID
WHERE        (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND 
                         (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Subject.SubjectAbsenceStatus = N'PRESENT') AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
						 

-----------------[[[[[[[[[[[[[[[[[Publish_SettingID]]]]]]]]]]]]]]]]-----------------------
UPDATE  Exam_Result_of_Student
SET   Publish_SettingID = Exam_Publish_Setting.Publish_SettingID,
StudentPublishStatus = 'Pub'
FROM Exam_Publish_Setting INNER JOIN
                         Exam_Result_of_Student ON Exam_Publish_Setting.SchoolID = Exam_Result_of_Student.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Result_of_Student.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Result_of_Student.ClassID AND Exam_Publish_Setting.ExamID = Exam_Result_of_Student.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND  (Exam_Publish_Setting.ExamID = @ExamID)


-----------------[[[[[[[[[[[[[[[[[Up --by Condition---------PassStatus_InSubject]]]]]]]]]]]]]]]]-----------------------
UPDATE  Exam_Result_of_Student
SET     PassStatus_InSubject = 'F'
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Result_of_Student ON Exam_Result_of_Subject.StudentID = Exam_Result_of_Student.StudentID AND Exam_Result_of_Subject.StudentClassID = Exam_Result_of_Student.StudentClassID AND 
                         Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Student.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID
WHERE        (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.PassStatus_Subject = 'F') AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
AND (((Exam_Publish_Setting.IS_Fail_Enable_Optional_Subject = 0) AND (Exam_Result_of_Subject.SubjectType = N'Compulsory')) OR (Exam_Publish_Setting.IS_Fail_Enable_Optional_Subject = 1))




---[[[[[[[[[[[[[[[[[[[[[[-------IS_Enable_Grade_as_it_is_if_Fail---Update----Student_Grades,Student_Point-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Result_of_Student
SET                Student_Point = 0 
FROM            Exam_Result_of_Student INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Student.SchoolID = Exam_Publish_Setting.SchoolID AND Exam_Result_of_Student.EducationYearID = Exam_Publish_Setting.EducationYearID AND 
                         Exam_Result_of_Student.ClassID = Exam_Publish_Setting.ClassID AND Exam_Result_of_Student.ExamID = Exam_Publish_Setting.ExamID
WHERE        (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Publish_Setting.ClassID = @ClassID) AND (Exam_Publish_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0) AND (Exam_Result_of_Student.Student_Point <> 0) AND 
                         (Exam_Result_of_Student.PassStatus_InSubject = N'F')


						 
-----------------[[[[[[[[[[[[[[[[[Student_Grade,Student_Comments]]]]]]]]]]]]]]]]-----------------------

declare @IS_Grade_BasePoint bit

 SELECT @IS_Grade_BasePoint = IS_Grade_BasePoint FROM  Exam_Publish_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)

 IF(@IS_Grade_BasePoint = 1)
 BEGIN
	 UPDATE Exam_Result_of_Student
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC),
	Student_Comments = (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC)
	From Exam_Result_of_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.ExamID = @ExamID)
 END
 ELSE
 BEGIN
 	 UPDATE Exam_Result_of_Student
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC),
	Student_Comments =  (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC)
	From Exam_Result_of_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.ExamID = @ExamID)
 END


---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Result_of_Student
SET                NotGolden =0
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (StudentResultID NOT IN
                             (SELECT        Exam_Result_of_Subject.StudentResultID
                               FROM            Exam_Result_of_Subject INNER JOIN
                                                             (SELECT Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                                                             FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
                                                             WHERE (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
                                                             GROUP BY Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID) AS Max_P ON Exam_Result_of_Subject.SchoolID = Max_P.SchoolID AND Exam_Result_of_Subject.EducationYearID = Max_P.EducationYearID AND 
                                                         Exam_Result_of_Subject.SubjectPoint <> Max_P.Max_Point
                               WHERE        (Exam_Result_of_Subject.SubjectType = N'Compulsory') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
                                                         (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
                               GROUP BY  Exam_Result_of_Subject.StudentResultID))

---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Result_of_Student
SET                NotGolden = 1
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                               FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
                               WHERE (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
                               GROUP BY Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID) AS Max_P ON Exam_Result_of_Student.SchoolID = Max_P.SchoolID AND Exam_Result_of_Student.EducationYearID = Max_P.EducationYearID AND 
                         Exam_Result_of_Student.Student_Point <> Max_P.Max_Point
WHERE        (Exam_Result_of_Student.NotGolden = 0) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
                         (Exam_Result_of_Student.ClassID = @ClassID) AND (Exam_Result_of_Student.ExamID = @ExamID)

---[[[[[[[[[[[[[[[[[[[[[[-----Update No optional--NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Result_of_Student
SET                NotGolden =1
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (StudentResultID NOT IN (SELECT StudentResultID
FROM            Exam_Result_of_Subject
WHERE        (SubjectType = N'Optional') AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (IS_Add_InExam = 1)
GROUP BY StudentResultID))

END
GO
