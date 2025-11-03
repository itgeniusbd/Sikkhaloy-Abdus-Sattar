-- ==========================================
-- Stored Procedure: SP_Cumulative_Exam_Student
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Cumulative_Exam_Student]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_Cumulative_Exam_Student]
END
GO

--10.
--CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Student]

CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Student]

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
DELETE FROM Exam_Cumulative_Student WHERE (CumulativeNameID = @CumulativeNameID) and  (SchoolID = @SchoolID) and  (EducationYearID =@EducationYearID) and (ClassID = @ClassID)

---[[[[[[[[[[[[[[[[[[[[[[-----------------INSERT----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
INSERT INTO Exam_Cumulative_Student
            (Cumulative_SettingID,
			CumulativeNameID, 
			 SchoolID, 
			 RegistrationID, 
			 EducationYearID,
			 ClassID,

			 StudentID, 
			 StudentClassID, 
			 TotalSubjest_WithOptional,
			 TotalSubject,
			 TotalMark_ofStudent,
			 ObtainedMark_ofStudent,
			 PassPercentage_Student, 
			 TotalPoint)
SELECT 
       @Cumulative_SettingID,@CumulativeNameID,@SchoolID,@RegistrationID,@EducationYearID,@ClassID,       
       StudentID, 
	   StudentClassID,
       ROUND(COUNT(Cumulative_SubjectID), 2, 0) AS Total_Sub_WithOptional, 
	   ROUND(COUNT(Cumulative_SubjectID), 2, 0) AS TotalSubject,
       ROUND(SUM(TotalMark_ofSubject), 2, 0) AS TotalMark, 
	   ROUND(SUM(OMark_ofSub_ConsiderOptional), 2, 0) AS OMark, 
	   AVG(PassPercentage_Subject) AS PassPercentage, 
       ROUND(SUM(SubjectPoint_ConsiderOptional), 2, 0) AS  TotalPoint 

FROM Exam_Cumulative_Subject
WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)
GROUP BY StudentID, StudentClassID

---------[[[[[[[[[[[[Update TotalSubject,TotalMark_ofStudent]]]]]]]]]]]]]]]---------
UPDATE Exam_Cumulative_Student
SET  TotalMark_ofStudent = Stu_Sub.TotalMark, TotalSubject = Stu_Sub.Total_Sub
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT        Exam_Cumulative_Subject.SchoolID, Exam_Cumulative_Subject.EducationYearID, Exam_Cumulative_Subject.CumulativeNameID, Exam_Cumulative_Subject.ClassID, 
                                                         Exam_Cumulative_Subject.StudentID, COUNT(Exam_Cumulative_Subject.Cumulative_SubjectID) AS Total_Sub, Exam_Cumulative_Subject.StudentClassID, 
                                                         SUM(Exam_Cumulative_Subject.TotalMark_ofSubject) AS TotalMark
                               FROM            Exam_Cumulative_Subject INNER JOIN
                                                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID AND 
                                                         Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                                                         Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID INNER JOIN
                                                         StudentRecord ON Exam_Cumulative_Subject.StudentClassID = StudentRecord.StudentClassID AND Exam_Cumulative_Subject.SchoolID = StudentRecord.SchoolID AND 
                                                         Exam_Cumulative_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Cumulative_Subject.StudentID = StudentRecord.StudentID
                               WHERE        (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                                                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.IS_Add_Optional_Mark_In_FullMarks = 0) AND 
                                                         (StudentRecord.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)
                               GROUP BY Exam_Cumulative_Subject.SchoolID, Exam_Cumulative_Subject.EducationYearID, Exam_Cumulative_Subject.StudentID, Exam_Cumulative_Subject.StudentClassID, 
                                                         Exam_Cumulative_Subject.ClassID, Exam_Cumulative_Subject.CumulativeNameID) AS Stu_Sub ON Exam_Cumulative_Student.CumulativeNameID = Stu_Sub.CumulativeNameID AND 
                         Exam_Cumulative_Student.SchoolID = Stu_Sub.SchoolID AND Exam_Cumulative_Student.EducationYearID = Stu_Sub.EducationYearID AND Exam_Cumulative_Student.StudentID = Stu_Sub.StudentID AND 
                         Exam_Cumulative_Student.ClassID = Stu_Sub.ClassID AND Exam_Cumulative_Student.StudentClassID = Stu_Sub.StudentClassID

-----------------[[[[[[[[[[[[[[[[[if ObtainedMark_ofStudent > TotalMark_ofStudent]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET ObtainedMark_ofStudent = TotalMark_ofStudent
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (TotalMark_ofStudent < ObtainedMark_ofStudent)

-----------------[[[[[[[[[[[[[[[[[PassMark_Student]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET                PassMark_Student = ROUND(TotalMark_ofStudent * PassPercentage_Student / 100, 2, 0)
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)

-----------------[[[[[[[[[[[[[[[[[StudentAbsenceStatus]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET              StudentAbsenceStatus = 'Present'
FROM            Exam_Cumulative_Student INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_Student.SchoolID = Exam_Cumulative_Subject.SchoolID AND Exam_Cumulative_Student.EducationYearID = Exam_Cumulative_Subject.EducationYearID AND 
                         Exam_Cumulative_Student.ClassID = Exam_Cumulative_Subject.ClassID AND Exam_Cumulative_Student.StudentClassID = Exam_Cumulative_Subject.StudentClassID AND 
                         Exam_Cumulative_Student.StudentID = Exam_Cumulative_Subject.StudentID
WHERE        (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Subject.SubjectAbsenceStatus = N'PRESENT') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)

-----------------[[[[[[[[[[[[[[[[[PassStatus_InSubject]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET                PassStatus_InSubject ='F'
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID INNER JOIN
                         Exam_Cumulative_Student ON Exam_Cumulative_Subject.StudentID = Exam_Cumulative_Student.StudentID AND Exam_Cumulative_Subject.StudentClassID = Exam_Cumulative_Student.StudentClassID AND 
                         Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Student.Cumulative_SettingID
WHERE        (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.IS_Fail_Enable_Optional_Subject = 0) AND (Exam_Cumulative_Subject.PassStatus_Subject = 'F') AND 
                         (Exam_Cumulative_Subject.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)

UPDATE Exam_Cumulative_Student
SET                PassStatus_InSubject = 'F'
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID INNER JOIN
                         Exam_Cumulative_Student ON Exam_Cumulative_Subject.StudentID = Exam_Cumulative_Student.StudentID AND Exam_Cumulative_Subject.StudentClassID = Exam_Cumulative_Student.StudentClassID AND 
                         Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Student.Cumulative_SettingID
WHERE        (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.IS_Fail_Enable_Optional_Subject = 1) AND (Exam_Cumulative_Subject.PassStatus_Subject = 'F') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)

-----------------[[[[[[[[[[[[[[[[[Student_Point]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET                Student_Point = CASE WHEN Max_P.Max_Point < ROUND(Exam_Cumulative_Student.TotalPoint / Exam_Cumulative_Student.TotalSubject, 2, 0) 
                         THEN Max_P.Max_Point ELSE ROUND(Exam_Cumulative_Student.TotalPoint / Exam_Cumulative_Student.TotalSubject, 2, 0) END
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
WHERE        (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)
GROUP BY Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID) AS Max_P ON Exam_Cumulative_Student.SchoolID = Max_P.SchoolID AND Exam_Cumulative_Student.EducationYearID = Max_P.EducationYearID
WHERE        (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)



---[[[[[[[[[[[[[[[[[[[[[[-------IS_Enable_Grade_as_it_is_if_Fail---Update----Student_Grades,Student_Point-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Student
SET           Student_Point =0
FROM            Exam_Cumulative_Student INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Student.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Student.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                         Exam_Cumulative_Student.ClassID = Exam_Cumulative_Setting.ClassID AND Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0) AND (Exam_Cumulative_Student.Student_Point <> 0) AND 
                         (Exam_Cumulative_Student.PassStatus_InSubject = N'F')

-----------------[[[[[[[[[[[[[[[[[Student_Grade,Student_Comments]]]]]]]]]]]]]]]]-----------------------
declare @IS_Grade_BasePoint bit

 SELECT @IS_Grade_BasePoint = IS_Grade_BasePoint FROM  Exam_Cumulative_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)

 IF(@IS_Grade_BasePoint = 1)
 BEGIN
	UPDATE Exam_Cumulative_Student 
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC),
	Student_Comments = (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC)
	From Exam_Cumulative_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.CumulativeNameID = @CumulativeNameID)
END
 ELSE
 BEGIN
 	UPDATE Exam_Cumulative_Student 
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC),
	Student_Comments = (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC)
	From Exam_Cumulative_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.CumulativeNameID = @CumulativeNameID)
 END

---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
 UPDATE       Exam_Cumulative_Student
SET                NotGolden =0
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (Cumulative_StudentID NOT IN

(SELECT        Exam_Cumulative_Student.Cumulative_StudentID
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT        Exam_Cumulative_Subject.Cumulative_SettingID, Exam_Cumulative_Subject.StudentID, Exam_Cumulative_Subject.StudentClassID
                               FROM            Exam_Cumulative_Subject INNER JOIN
                                                             (SELECT Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                                                             FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
                                                             WHERE (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)
                                                             GROUP BY Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID) AS Max_P ON Exam_Cumulative_Subject.SchoolID = Max_P.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Max_P.EducationYearID AND 
                                                         Exam_Cumulative_Subject.SubjectPoint <> Max_P.Max_Point
                               WHERE        (Exam_Cumulative_Subject.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)
                               GROUP BY Exam_Cumulative_Subject.Cumulative_SettingID, Exam_Cumulative_Subject.StudentID, Exam_Cumulative_Subject.StudentClassID) AS TT ON 
                         Exam_Cumulative_Student.StudentID = TT.StudentID AND Exam_Cumulative_Student.StudentClassID = TT.StudentClassID AND Exam_Cumulative_Student.Cumulative_SettingID = TT.Cumulative_SettingID))

---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Student
SET                NotGolden = 1
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                                                             FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
                                                             WHERE (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)
                                                             GROUP BY Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID) AS Max_P ON Exam_Cumulative_Student.SchoolID = Max_P.SchoolID AND Exam_Cumulative_Student.EducationYearID = Max_P.EducationYearID AND 
                         Exam_Cumulative_Student.Student_Point <> Max_P.Max_Point
WHERE        (Exam_Cumulative_Student.NotGolden = 0) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
                         (Exam_Cumulative_Student.ClassID = @ClassID) AND (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)

---[[[[[[[[[[[[[[[[[[[[[[-----Update No optional--NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Student
SET                NotGolden =1
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (Cumulative_StudentID  NOT IN (SELECT Exam_Cumulative_Student.Cumulative_StudentID
FROM            Exam_Cumulative_Student INNER JOIN
                         Exam_Cumulative_Subject AS Sub_T ON Exam_Cumulative_Student.StudentID = Sub_T.StudentID AND Exam_Cumulative_Student.StudentClassID = Sub_T.StudentClassID AND 
                         Exam_Cumulative_Student.Cumulative_SettingID = Sub_T.Cumulative_SettingID
WHERE        (Sub_T.SubjectType = N'Optional') AND (Sub_T.IS_Add_InExam = 1) AND (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Student.ClassID = @ClassID) AND (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)
GROUP BY Exam_Cumulative_Student.Cumulative_StudentID))
END

GO
