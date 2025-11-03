-- ==========================================
-- Stored Procedure: SP_Cumulative_Exam_Subject
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Cumulative_Exam_Subject]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_Cumulative_Exam_Subject]
END
GO


--9.
--CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Subject]
CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Subject]

-- Where condition parameters
	@SchoolID int,
	@RegistrationID int,
    @EducationYearID int,
	@ClassID int,
	@CumulativeNameID int,
	@Cumulative_SettingID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
---[[[[[[[[[[[[[[[[[[[[[[-----------------DELETE----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
DELETE FROM Exam_Cumulative_Subject WHERE (CumulativeNameID = @CumulativeNameID) and  (SchoolID = @SchoolID) and  (EducationYearID =@EducationYearID) and (ClassID = @ClassID)

---[[[[[[[[[[[[[[[[[[[[[[-----------------INSERT----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
--(SchoolID, RegistrationID, EducationYearID,ClassID, CumulativeNameID,StudentID, StudentClassID,  SubjectID, TotalMark_ofSubject, ObtainedMark_ofSubject)-------------

INSERT INTO Exam_Cumulative_Subject
                         (Cumulative_SettingID,SchoolID, RegistrationID, EducationYearID,ClassID, CumulativeNameID,StudentID, StudentClassID,  SubjectID, TotalMark_ofSubject, ObtainedMark_ofSubject)
SELECT      @Cumulative_SettingID,@SchoolID ,@RegistrationID,@EducationYearID,@ClassID ,@CumulativeNameID ,
                         Exam_Result_of_Subject.StudentID, Exam_Result_of_Subject.StudentClassID, Exam_Result_of_Subject.SubjectID, Exam_Cumulative_FullMarks.FullMarks, 
                         ROUND(SUM(Exam_Result_of_Subject.ObtainedMark_ofSubject * (Exam_Cumulative_ExamList.ExamAdd_Percentage / 100)) * Exam_Cumulative_FullMarks.FullMarks / SUM(Exam_Result_of_Subject.TotalMark_ofSubject * (Exam_Cumulative_ExamList.ExamAdd_Percentage / 100)), 2, 0) AS Obtained_Mark
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Cumulative_ExamList ON Exam_Result_of_Subject.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Subject.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND 
                         Exam_Result_of_Subject.SchoolID = Exam_Cumulative_ExamList.SchoolID AND Exam_Result_of_Subject.ClassID = Exam_Cumulative_ExamList.ClassID INNER JOIN
                         Exam_Cumulative_Name ON Exam_Cumulative_ExamList.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID INNER JOIN
                         Exam_Cumulative_FullMarks ON Exam_Cumulative_Name.CumulativeNameID = Exam_Cumulative_FullMarks.CumulativeNameID AND 
                         Exam_Result_of_Subject.SubjectID = Exam_Cumulative_FullMarks.SubjectID 
						 INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID  --  
						 INNER JOIN Student ON Exam_Result_of_Subject.StudentID = Student.StudentID   --without Reject Student 

WHERE        (Exam_Cumulative_ExamList.SchoolID = @SchoolID) AND (Exam_Cumulative_ExamList.EducationYearID = @EducationYearID) AND (Exam_Cumulative_ExamList.CumulativeNameID = @CumulativeNameID) 
                         AND (Exam_Cumulative_ExamList.ClassID = @ClassID) AND (Exam_Cumulative_FullMarks.Cumulative_SettingID = @Cumulative_SettingID)
						
						 AND (Student.Status = N'Active') AND (StudentsClass.Promotion_Demotion_Year IS NULL) --without Reject Student  & without Promotion Demotion Student

GROUP BY Exam_Result_of_Subject.StudentClassID, Exam_Result_of_Subject.SubjectID, Exam_Cumulative_FullMarks.FullMarks, Exam_Result_of_Subject.StudentID




---[[[[[[[[[[[[[[[[[[[[[[--------SubjectAbsenceStatus-------------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Cumulative_Subject
SET                SubjectAbsenceStatus = 'Present'
FROM            Exam_Cumulative_ExamList INNER JOIN
                         Exam_Result_of_Subject ON Exam_Cumulative_ExamList.ExamID = Exam_Result_of_Subject.ExamID AND Exam_Cumulative_ExamList.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Cumulative_ExamList.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Cumulative_ExamList.SchoolID = Exam_Result_of_Subject.SchoolID INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_ExamList.SchoolID = Exam_Cumulative_Subject.SchoolID AND Exam_Cumulative_ExamList.EducationYearID = Exam_Cumulative_Subject.EducationYearID AND
                          Exam_Cumulative_ExamList.CumulativeNameID = Exam_Cumulative_Subject.CumulativeNameID AND Exam_Cumulative_ExamList.ClassID = Exam_Cumulative_Subject.ClassID AND 
                         Exam_Result_of_Subject.StudentClassID = Exam_Cumulative_Subject.StudentClassID AND Exam_Result_of_Subject.SubjectID = Exam_Cumulative_Subject.SubjectID AND 
                         Exam_Result_of_Subject.StudentID = Exam_Cumulative_Subject.StudentID
WHERE        (Exam_Cumulative_ExamList.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_ExamList.SchoolID = @SchoolID) AND (Exam_Cumulative_ExamList.EducationYearID = @EducationYearID) 
                         AND (Exam_Cumulative_ExamList.ClassID = @ClassID) AND (Exam_Result_of_Subject.SubjectAbsenceStatus = N'PRESENT')


---[[[[[[[[[[[[[[[[[[[[[[-------Grade Point --OMark_ofSub_ConsiderOptional --SubjectPoint_ConsiderOptional---------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Cumulative_Subject
SET                SubjectGrades =Exam_Grading_System.Grades, SubjectPoint = Exam_Grading_System.Point ,OMark_ofSub_ConsiderOptional=ObtainedMark_ofSubject, SubjectPoint_ConsiderOptional = Exam_Grading_System.Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Grading_System.MinPercentage <= Exam_Cumulative_Subject.ObtainedPercentage_ofSubject AND 
                         Exam_Grading_System.MaxPercentage + 1 > Exam_Cumulative_Subject.ObtainedPercentage_ofSubject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID AND Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
WHERE        (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND 
                         (Exam_Cumulative_Subject.ClassID = @ClassID)

---[[[[[[[[[[[[[[[[[[[[[[--SubjectType--OMark_ofSub_ConsiderOptional --SubjectPoint_ConsiderOptional --Update --------]]]]]]]]]]]]]]]]]]]]]]]]]]---

-----Update to Compulsory 
UPDATE       Exam_Cumulative_Subject
SET               SubjectType = 'Compulsory'
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID AND Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID AND 
                         Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID INNER JOIN
                         StudentRecord ON Exam_Cumulative_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Cumulative_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Cumulative_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Cumulative_Subject.EducationYearID = StudentRecord.EducationYearID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (StudentRecord.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.SubjectType = N'Optional')

----Update to Optional 
UPDATE       Exam_Cumulative_Subject
SET   SubjectType = 'Optional',               
OMark_ofSub_ConsiderOptional = (CASE WHEN Exam_Cumulative_Subject.ObtainedPercentage_ofSubject < Exam_Cumulative_Setting.Optional_Percentage_Deduction THEN 0 ELSE ROUND(Exam_Cumulative_Subject.ObtainedMark_ofSubject - (Exam_Cumulative_Subject.TotalMark_ofSubject * Exam_Cumulative_Setting.Optional_Percentage_Deduction) / 100, 2, 0) END), 
SubjectPoint_ConsiderOptional = (CASE WHEN Exam_Grading_System.Point > Exam_Cumulative_Subject.SubjectPoint THEN 0 ELSE Exam_Cumulative_Subject.SubjectPoint - Exam_Grading_System.Point END)
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID AND 
                         Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                         Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID AND Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID INNER JOIN
                         Exam_Grading_System ON Exam_Cumulative_Setting.Optional_Percentage_Deduction >= Exam_Grading_System.MinPercentage AND 
                         Exam_Cumulative_Setting.Optional_Percentage_Deduction < Exam_Grading_System.MaxPercentage + 1 AND Exam_Cumulative_Setting.GradeNameID = Exam_Grading_System.GradeNameID INNER JOIN
                         StudentRecord ON Exam_Cumulative_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Cumulative_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Cumulative_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Cumulative_Subject.EducationYearID = StudentRecord.EducationYearID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (StudentRecord.SubjectType = N'Optional')







---[[[[[[[[[[[[[[[[[[[[[[--------PassPercentage_Subject---------PassMark_Subject-------PassStatus_Subject-----]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Cumulative_Subject
SET                PassPercentage_Subject = ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1, PassMark_Subject = ROUND(Exam_Cumulative_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) 
                         + 1) / 100, 2, 0), PassStatus_Subject = CASE WHEN ObtainedMark_ofSubject < ROUND(Exam_Cumulative_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1) 
                         / 100, 2, 0)  THEN 'F' ELSE 'P' END
FROM            Exam_Cumulative_Setting INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_Setting.Cumulative_SettingID = Exam_Cumulative_Subject.Cumulative_SettingID INNER JOIN
                         Exam_Grading_System ON Exam_Cumulative_Setting.GradeNameID = Exam_Grading_System.GradeNameID
WHERE        (Exam_Grading_System.Grades = 'F') AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Setting.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)


---[[[[[[[[[[[[[[[[[[[[[[-----Exam_Cumulative_ExamList.Exam_EnableFail------PassStatus_Subject--------Update To 'F'------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Cumulative_Subject
SET                PassStatus_Subject = Exam_Result_of_Subject.PassStatus_Subject
FROM            Exam_Cumulative_ExamList INNER JOIN
                         Exam_Result_of_Subject ON Exam_Cumulative_ExamList.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Cumulative_ExamList.ClassID = Exam_Result_of_Subject.ClassID AND 
                         Exam_Cumulative_ExamList.EducationYearID = Exam_Result_of_Subject.EducationYearID AND Exam_Cumulative_ExamList.ExamID = Exam_Result_of_Subject.ExamID INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_ExamList.SchoolID = Exam_Cumulative_Subject.SchoolID AND Exam_Cumulative_ExamList.EducationYearID = Exam_Cumulative_Subject.EducationYearID AND
                          Exam_Cumulative_ExamList.CumulativeNameID = Exam_Cumulative_Subject.CumulativeNameID AND Exam_Cumulative_ExamList.ClassID = Exam_Cumulative_Subject.ClassID AND 
                         Exam_Result_of_Subject.SubjectID = Exam_Cumulative_Subject.SubjectID AND Exam_Result_of_Subject.StudentClassID = Exam_Cumulative_Subject.StudentClassID AND 
                         Exam_Result_of_Subject.StudentID = Exam_Cumulative_Subject.StudentID
WHERE        (Exam_Cumulative_ExamList.SchoolID = @SchoolID) AND (Exam_Cumulative_ExamList.ClassID = @ClassID) AND (Exam_Cumulative_ExamList.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_ExamList.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_ExamList.Exam_EnableFail = 1) AND (Exam_Result_of_Subject.PassStatus_Subject = 'F')



---[[[[[[[[[[[[[[[[[[[[[[-------IS_Enable_Grade_as_it_is_if_Fail---Update----SubjectGrades,SubjectPoint----SubjectPoint_ConsiderOptional ---]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Subject
SET                SubjectGrades = 'F', SubjectPoint = 0, SubjectPoint_ConsiderOptional = 0
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                         Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID AND Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0) AND (Exam_Cumulative_Subject.PassStatus_Subject = 'F') AND (Exam_Cumulative_Subject.SubjectPoint <> 0)
END



GO
