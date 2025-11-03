-- ==========================================
-- Stored Procedure: HighestMark_Position
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HighestMark_Position]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[HighestMark_Position]
END
GO

--15.
--ALTER PROCEDURE [dbo].[HighestMark_Position]
CREATE PROCEDURE [dbo].[HighestMark_Position]
    @SchoolID int,
	@EducationYearID int,
	@ClassID int,
	@ExamID int,
	@Exam_Position_Format nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    

----------------------------------------------------------------------------------------------------------------------------------------------------
---Position_InExam_Class --------HighestMark_InExam_Class---------------Position_InExam_Subsection

declare @HighestMark_InExam_Class float
--for HighestMark_InExam_Class -----
SELECT @HighestMark_InExam_Class = MAX(ObtainedMark_ofStudent) FROM Exam_Result_of_Student WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)


if(@Exam_Position_Format = 'Point')
BEGIN
  UPDATE  Exam_Result_of_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Result_of_Student INNER JOIN
  (
   SELECT DENSE_RANK() OVER (Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.NotGolden, Exam_Result_of_Student.Student_Point DESC,Exam_Result_of_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.NotGolden, Exam_Result_of_Student.Student_Point DESC,Exam_Result_of_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Result_of_Student.Student_Point,Exam_Result_of_Student.ObtainedMark_ofStudent,Exam_Result_of_Student.StudentResultID 
   FROM Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
   WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
   (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
   (Exam_Result_of_Student.ClassID = @ClassID) AND 
   (Exam_Result_of_Student.ExamID = @ExamID) 
  ) as a
  ON Exam_Result_of_Student.StudentResultID = a.StudentResultID  
 END
ELSE
 BEGIN
   UPDATE  Exam_Result_of_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Result_of_Student INNER JOIN
   (
    SELECT DENSE_RANK() OVER (Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Result_of_Student.ObtainedMark_ofStudent,Exam_Result_of_Student.StudentResultID 
    FROM Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
    WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
    (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
    (Exam_Result_of_Student.ClassID = @ClassID) AND 
    (Exam_Result_of_Student.ExamID = @ExamID) 
   ) as a
  ON Exam_Result_of_Student.StudentResultID = a.StudentResultID  
END


----------------------------------------------------------------------------------------------------------------------------------------------
-----------HighestMark_InExam_Subsection

UPDATE  Exam_Result_of_Student
SET       HighestMark_InExam_Subsection = a.HighestMark_InExam_Subsection
FROM  Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Result_of_Student.ObtainedMark_ofStudent)as HighestMark_InExam_Subsection ,
StudentsClass.SectionID,StudentsClass.ShiftID,StudentsClass.SubjectGroupID 

FROM Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
(Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Student.ClassID = @ClassID) AND 
(Exam_Result_of_Student.ExamID = @ExamID)
group by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a

ON StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
(Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Student.ClassID = @ClassID) AND 
(Exam_Result_of_Student.ExamID = @ExamID)



--------------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------

-----------HighestMark_InSubject_Class

UPDATE Exam_Result_of_Subject
SET HighestMark_InSubject_Class = a.HighestMark_InSubject_Class
FROM Exam_Result_of_Subject INNER JOIN
 (SELECT MAX(ObtainedMark_ofSubject) AS HighestMark_InSubject_Class, SubjectID, SchoolID, EducationYearID, ClassID, ExamID
  FROM  Exam_Result_of_Subject GROUP BY SubjectID, SchoolID, EducationYearID, ClassID, ExamID) AS a ON Exam_Result_of_Subject.SubjectID = a.SubjectID AND Exam_Result_of_Subject.SchoolID = a.SchoolID AND 
  Exam_Result_of_Subject.EducationYearID = a.EducationYearID AND Exam_Result_of_Subject.ClassID = a.ClassID AND Exam_Result_of_Subject.ExamID = a.ExamID
  WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND 
       (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
       (Exam_Result_of_Subject.ClassID = @ClassID) AND 
       (Exam_Result_of_Subject.ExamID = @ExamID)

-------------------------------------------------------------------------------------------------------------------------------------

--For HighestMark_InSubject_Subsection-------------------------

UPDATE  Exam_Result_of_Subject
SET       HighestMark_InSubject_Subsection = a.Mark_ofSubject
FROM  Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Result_of_Subject.ObtainedMark_ofSubject) as Mark_ofSubject,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND 
(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Subject.ClassID = @ClassID) AND 
(Exam_Result_of_Subject.ExamID = @ExamID) 
group by Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a 
ON Exam_Result_of_Subject.SubjectID = a.SubjectID and StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND 
(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Subject.ClassID = @ClassID) AND 
(Exam_Result_of_Subject.ExamID = @ExamID) 


---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

--For Position_InSubject_Class-------- Position_InSubject_Subsection-------------------------


if(@Exam_Position_Format = 'Point')
BEGIN
	UPDATE  Exam_Result_of_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Result_of_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY SubjectPoint DESC, ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Result_of_Subject.SubjectPoint DESC, Exam_Result_of_Subject.ObtainedMark_ofSubject DESC,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Result_of_Subject.SubjectPoint,Exam_Result_of_Subject.ObtainedMark_ofSubject,Exam_Result_of_Subject.SubjectResultID ,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
	FROM Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
	WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND
	(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
	(Exam_Result_of_Subject.ClassID = @ClassID) AND 
	(Exam_Result_of_Subject.ExamID = @ExamID)) as a
	ON Exam_Result_of_Subject.SubjectResultID = a.SubjectResultID
 END
ELSE
 BEGIN


	UPDATE  Exam_Result_of_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Result_of_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Result_of_Subject.ObtainedMark_ofSubject DESC,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Result_of_Subject.ObtainedMark_ofSubject,Exam_Result_of_Subject.SubjectResultID ,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
	FROM Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
	WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND
	(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
	(Exam_Result_of_Subject.ClassID = @ClassID) AND 
	(Exam_Result_of_Subject.ExamID = @ExamID)) as a
	ON Exam_Result_of_Subject.SubjectResultID = a.SubjectResultID

END


------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


END



GO
