-- ==========================================
-- Stored Procedure: SP_Exam_Subject
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Exam_Subject]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_Exam_Subject]
END
GO

CREATE PROCEDURE [dbo].[SP_Exam_Subject]

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
	
---[[[[[[[[[[[[---- TotalExamFullMark_ofSubject , 
--	          TotalExamObtainedMark_ofSubject, 
--		      ObtainedMark_ofSubject,
--			  ObtainedPercentage_ofSubject ,
--			  TotalMark_ofSubject
--reset----SubjectAbsenceStatus
--reset----PassStatus_InSubExam-------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Result_of_Subject
SET               
TotalExamFullMark_ofSubject =U_T.TotalExamFullMark_ofSubject, 
TotalExamObtainedMark_ofSubject =U_T.TotalExamObtainedMark_ofSubject, 
ObtainedPercentage_ofSubject =U_T.ObtainedPercentage_ofSubject, 
ObtainedMark_ofSubject =U_T.ObtainedMark_ofSubject, 
TotalMark_ofSubject =U_T.Countable_Mark,
SubjectAbsenceStatus = 'Absent',
PassStatus_InSubExam = 'P'

FROM            Exam_Result_of_Subject INNER JOIN
                             (SELECT        Exam_Obtain_Marks.StudentResultID, Exam_Obtain_Marks.SubjectID, SUM(Exam_Obtain_Marks.FullMark) AS TotalExamFullMark_ofSubject, SUM(ISNULL(Exam_Obtain_Marks.MarksObtained, 0)) 
                                                         AS TotalExamObtainedMark_ofSubject, ROUND(SUM(ISNULL(Exam_Obtain_Marks.MarksObtained, 0) * Exam_Obtain_Marks.AddPercentage / 100) 
                                                         * Exam_Publish_Sub_Countable_Mark.Countable_Mark / SUM(Exam_Obtain_Marks.FullMark * Exam_Obtain_Marks.AddPercentage / 100) 
                                                         * 100 / Exam_Publish_Sub_Countable_Mark.Countable_Mark, 2, 0) AS ObtainedPercentage_ofSubject, ROUND(SUM(ISNULL(Exam_Obtain_Marks.MarksObtained, 0) 
                                                         * Exam_Obtain_Marks.AddPercentage / 100) * Exam_Publish_Sub_Countable_Mark.Countable_Mark / SUM(Exam_Obtain_Marks.FullMark * Exam_Obtain_Marks.AddPercentage / 100), 2, 0) 
                                                         AS ObtainedMark_ofSubject, Exam_Publish_Sub_Countable_Mark.Countable_Mark
                               FROM            Exam_Obtain_Marks INNER JOIN
                                                         Exam_Publish_Sub_Countable_Mark ON Exam_Obtain_Marks.SchoolID = Exam_Publish_Sub_Countable_Mark.SchoolID AND 
                                                         Exam_Obtain_Marks.EducationYearID = Exam_Publish_Sub_Countable_Mark.EducationYearID AND Exam_Obtain_Marks.SubjectID = Exam_Publish_Sub_Countable_Mark.SubjectID AND 
                                                         Exam_Obtain_Marks.ExamID = Exam_Publish_Sub_Countable_Mark.ExamID AND Exam_Obtain_Marks.ClassID = Exam_Publish_Sub_Countable_Mark.ClassID
                               WHERE        (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND 
                                                         (Exam_Obtain_Marks.ClassID = @ClassID)
                               GROUP BY Exam_Obtain_Marks.StudentResultID, Exam_Obtain_Marks.SubjectID, Exam_Publish_Sub_Countable_Mark.Countable_Mark) AS U_T ON 
                         Exam_Result_of_Subject.StudentResultID = U_T.StudentResultID AND Exam_Result_of_Subject.SubjectID = U_T.SubjectID



---[[[[[[[[[[[[[[[[[[[[[[-----Up By Condition---SubjectAbsenceStatus-------------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE     Exam_Result_of_Subject
SET        SubjectAbsenceStatus ='Present'
FROM            Exam_Obtain_Marks INNER JOIN
                         Exam_Result_of_Subject ON Exam_Obtain_Marks.StudentResultID = Exam_Result_of_Subject.StudentResultID AND Exam_Obtain_Marks.SubjectID = Exam_Result_of_Subject.SubjectID
WHERE        (Exam_Obtain_Marks.AbsenceStatus = 'Present') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
                         (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID)



---[[[[[[[[[[[[[[[[[[[[[[-----Up By Condition--- PassStatus_InSubExam-------------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Result_of_Subject
SET                PassStatus_InSubExam = 'F'
FROM            Exam_Obtain_Marks INNER JOIN
                     Exam_Result_of_Subject ON Exam_Obtain_Marks.StudentResultID = Exam_Result_of_Subject.StudentResultID AND Exam_Obtain_Marks.SubjectID = Exam_Result_of_Subject.SubjectID
WHERE        (Exam_Obtain_Marks.PassStatus = 'F') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
                        (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID)




---[[[[[[[[[[[[[[[[[[[[[[--------PassPercentage_Subject---------PassMark_Subject-------PassStatus_Subject-----]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Result_of_Subject
SET                PassPercentage_Subject = ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1, PassMark_Subject = ROUND(Exam_Result_of_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) 
                         + 1) / 100, 2, 0), PassStatus_Subject = CASE WHEN Exam_Result_of_Subject.ObtainedMark_ofSubject < ROUND(Exam_Result_of_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1) 
                         / 100, 2, 0)  THEN 'F' ELSE 'P' END
FROM            Exam_Grading_Assign INNER JOIN
                         Exam_Result_of_Subject ON Exam_Grading_Assign.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Grading_Assign.ExamID = Exam_Result_of_Subject.ExamID AND 
                         Exam_Grading_Assign.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Grading_Assign.EducationYearID = Exam_Result_of_Subject.EducationYearID INNER JOIN
                         Exam_Grading_System ON Exam_Grading_Assign.GradeNameID = Exam_Grading_System.GradeNameID AND Exam_Grading_Assign.SchoolID = Exam_Grading_System.SchoolID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Grading_System.Grades = 'F')


---[[[[[[[[[[[[[[[[[[[[[[------Update--PassStatus_Subject-----if~~~IS_Enable_Fail_if_fail_in_sub_Exam--------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Result_of_Subject
SET                PassStatus_Subject ='F'
FROM            Exam_Publish_Setting INNER JOIN
                         Exam_Result_of_Subject ON Exam_Publish_Setting.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Publish_Setting.ExamID = Exam_Result_of_Subject.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND 
                         (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Result_of_Subject.PassStatus_InSubExam = N'F') AND (Exam_Publish_Setting.IS_Enable_Fail_if_fail_in_sub_Exam = 1)  AND 
                         (Exam_Result_of_Subject.PassStatus_Subject <> 'F')



---[[[[[[[[[[[[[[[[[[[[[[-------GradingID, 
--	          SubjectGrades, 
--	          SubjectPoint,
--			  SubjectPoint_ConsiderOptional,---------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE        Exam_Result_of_Subject
SET             GradingID =Exam_Grading_System.GradingID,  SubjectGrades =Exam_Grading_System.Grades, SubjectPoint = Exam_Grading_System.Point ,OMark_ofSub_ConsiderOptional=ObtainedMark_ofSubject, SubjectPoint_ConsiderOptional = Exam_Grading_System.Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Result_of_Subject ON Exam_Grading_System.MinPercentage <= Exam_Result_of_Subject.ObtainedPercentage_ofSubject AND 
                         Exam_Grading_System.MaxPercentage + 1 > Exam_Result_of_Subject.ObtainedPercentage_ofSubject INNER JOIN
                         Exam_Grading_Assign ON Exam_Result_of_Subject.SchoolID = Exam_Grading_Assign.SchoolID AND Exam_Result_of_Subject.EducationYearID = Exam_Grading_Assign.EducationYearID AND 
                         Exam_Result_of_Subject.ClassID = Exam_Grading_Assign.ClassID AND Exam_Result_of_Subject.ExamID = Exam_Grading_Assign.ExamID AND 
                         Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
WHERE        (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID)



---[[[[[[[[[[[[[[[[[[[[[[----OMark_ofSub_ConsiderOptional --SubjectPoint_ConsiderOptional --Update --------]]]]]]]]]]]]]]]]]]]]]]]]]]---

---update Optional to comuplsory 
UPDATE       Exam_Result_of_Subject
SET                SubjectType = 'Compulsory'
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Subject.ExamID = Exam_Publish_Setting.ExamID AND Exam_Result_of_Subject.ClassID = Exam_Publish_Setting.ClassID AND 
                         Exam_Result_of_Subject.SchoolID = Exam_Publish_Setting.SchoolID AND Exam_Result_of_Subject.EducationYearID = Exam_Publish_Setting.EducationYearID INNER JOIN
                         StudentRecord ON Exam_Result_of_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Result_of_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Result_of_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Result_of_Subject.EducationYearID = StudentRecord.EducationYearID
WHERE        (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Publish_Setting.ClassID = @ClassID) AND (StudentRecord.SubjectType = N'Compulsory') AND (Exam_Result_of_Subject.SubjectType = N'Optional')

---update Optional 

UPDATE       Exam_Result_of_Subject
SET   SubjectType = 'Optional',               
OMark_ofSub_ConsiderOptional = (CASE WHEN Exam_Result_of_Subject.ObtainedPercentage_ofSubject < Exam_Publish_Setting.Optional_Percentage_Deduction THEN 0 ELSE ROUND(Exam_Result_of_Subject.ObtainedMark_ofSubject - (Exam_Result_of_Subject.TotalMark_ofSubject * Exam_Publish_Setting.Optional_Percentage_Deduction) / 100, 2, 0) END), 
SubjectPoint_ConsiderOptional = (CASE WHEN Exam_Grading_System.Point > Exam_Result_of_Subject.SubjectPoint THEN 0 ELSE Exam_Result_of_Subject.SubjectPoint - Exam_Grading_System.Point END)
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Subject.ExamID = Exam_Publish_Setting.ExamID AND Exam_Result_of_Subject.ClassID = Exam_Publish_Setting.ClassID AND 
                         Exam_Result_of_Subject.SchoolID = Exam_Publish_Setting.SchoolID AND Exam_Result_of_Subject.EducationYearID = Exam_Publish_Setting.EducationYearID INNER JOIN
                         Exam_Grading_System ON Exam_Publish_Setting.Optional_Percentage_Deduction >= Exam_Grading_System.MinPercentage AND 
                         Exam_Publish_Setting.Optional_Percentage_Deduction < Exam_Grading_System.MaxPercentage + 1 INNER JOIN
                         StudentRecord ON Exam_Result_of_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Result_of_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Result_of_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Result_of_Subject.EducationYearID = StudentRecord.EducationYearID INNER JOIN
                         Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID AND Exam_Grading_System.SchoolID = Exam_Grading_Assign.SchoolID AND 
                         Exam_Publish_Setting.SchoolID = Exam_Grading_Assign.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Grading_Assign.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Grading_Assign.ClassID AND Exam_Publish_Setting.ExamID = Exam_Grading_Assign.ExamID
WHERE        (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND 
                         (StudentRecord.SubjectType = N'Optional')


--Update  Exam_Result_of_Subject---  Sub Exam Fail Enable------------

UPDATE Exam_Result_of_Subject SET SubjectGrades = N'F', SubjectPoint = 0 ,SubjectPoint_ConsiderOptional = 0
FROM            Exam_Publish_Setting INNER JOIN
                         Exam_Result_of_Subject ON Exam_Publish_Setting.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Publish_Setting.ExamID = Exam_Result_of_Subject.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND 
                         (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Result_of_Subject.PassStatus_Subject = 'F') AND (Exam_Publish_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0)
END

GO
