-- ==========================================
-- Stored Procedure: SP_Cumulative_HighestMark_Position
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Cumulative_HighestMark_Position]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_Cumulative_HighestMark_Position]
END
GO


--11.
--CREATE PROCEDURE [dbo].[SP_Cumulative_HighestMark_Position]
CREATE PROCEDURE [dbo].[SP_Cumulative_HighestMark_Position]
    @SchoolID int,
	@EducationYearID int,
	@ClassID int,
	@CumulativeNameID int,
	@Exam_Position_Format nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
---Position_InExam_Class --------HighestMark_InExam_Class---------------Position_InExam_Subsection

declare @HighestMark_InExam_Class float
--for HighestMark_InExam_Class -----
SELECT @HighestMark_InExam_Class = MAX(ObtainedMark_ofStudent) FROM Exam_Cumulative_Student WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)


if(@Exam_Position_Format = 'Point')
BEGIN
  UPDATE  Exam_Cumulative_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Cumulative_Student INNER JOIN
  (
   SELECT DENSE_RANK() OVER (Order by Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.NotGolden, Exam_Cumulative_Student.Student_Point DESC,Exam_Cumulative_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.NotGolden, Exam_Cumulative_Student.Student_Point DESC,Exam_Cumulative_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Cumulative_Student.Student_Point,Exam_Cumulative_Student.ObtainedMark_ofStudent,Exam_Cumulative_Student.Cumulative_StudentID 
           FROM   Exam_Cumulative_Student INNER JOIN
                 StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                 Student ON Exam_Cumulative_Student.StudentID = Student.StudentID
           WHERE        (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
		                (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
						(Exam_Cumulative_Student.ClassID = @ClassID) AND 
                        (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID) AND 
						(Student.Status = N'Active')) as a
  ON Exam_Cumulative_Student.Cumulative_StudentID = a.Cumulative_StudentID  
 END
ELSE
 BEGIN
   UPDATE  Exam_Cumulative_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Cumulative_Student INNER JOIN
   (
    SELECT DENSE_RANK() OVER (Order by Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by  Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Cumulative_Student.ObtainedMark_ofStudent,Exam_Cumulative_Student.Cumulative_StudentID 
FROM            Exam_Cumulative_Student INNER JOIN
                         StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         Student ON Exam_Cumulative_Student.StudentID = Student.StudentID
WHERE        (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID) AND (Student.Status = N'Active')) as a
  ON Exam_Cumulative_Student.Cumulative_StudentID = a.Cumulative_StudentID  
END


----------------------------------------------------------------------------------------------------------------------------------------------
-----------HighestMark_InExam_Subsection

UPDATE  Exam_Cumulative_Student
SET       HighestMark_InExam_Subsection = a.HighestMark_InExam_Subsection
FROM  Exam_Cumulative_Student INNER JOIN StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Cumulative_Student.ObtainedMark_ofStudent)as HighestMark_InExam_Subsection ,
StudentsClass.SectionID,StudentsClass.ShiftID,StudentsClass.SubjectGroupID 

FROM Exam_Cumulative_Student INNER JOIN StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Student.ClassID = @ClassID) AND 
(Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)
group by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a

ON StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Student.ClassID = @ClassID) AND 
(Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)



--------------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------

-----------HighestMark_InSubject_Class

UPDATE Exam_Cumulative_Subject
SET HighestMark_InSubject_Class = a.HighestMark_InSubject_Class
FROM Exam_Cumulative_Subject INNER JOIN
 (SELECT MAX(ObtainedMark_ofSubject) AS HighestMark_InSubject_Class, SubjectID, SchoolID, EducationYearID, ClassID, CumulativeNameID
  FROM  Exam_Cumulative_Subject GROUP BY SubjectID, SchoolID, EducationYearID, ClassID, CumulativeNameID) AS a ON Exam_Cumulative_Subject.SubjectID = a.SubjectID AND Exam_Cumulative_Subject.SchoolID = a.SchoolID AND 
  Exam_Cumulative_Subject.EducationYearID = a.EducationYearID AND Exam_Cumulative_Subject.ClassID = a.ClassID AND Exam_Cumulative_Subject.CumulativeNameID = a.CumulativeNameID
  WHERE (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND 
       (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND 
       (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
       (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID)

-------------------------------------------------------------------------------------------------------------------------------------

--For HighestMark_InSubject_Subsection-------------------------

UPDATE  Exam_Cumulative_Subject
SET       HighestMark_InSubject_Subsection = a.Mark_ofSubject
FROM  Exam_Cumulative_Subject INNER JOIN StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Cumulative_Subject.ObtainedMark_ofSubject) as Mark_ofSubject,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM Exam_Cumulative_Subject INNER JOIN StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Subject.ClassID = @ClassID) AND 
(Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) 
group by Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a 
ON Exam_Cumulative_Subject.SubjectID = a.SubjectID and StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Subject.ClassID = @ClassID) AND 
(Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) 


---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

--For Position_InSubject_Class-------- Position_InSubject_Subsection-------------------------


if(@Exam_Position_Format = 'Point')
BEGIN
	UPDATE  Exam_Cumulative_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Cumulative_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY SubjectPoint DESC, ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Cumulative_Subject.SubjectPoint DESC, Exam_Cumulative_Subject.ObtainedMark_ofSubject DESC,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Cumulative_Subject.SubjectPoint,Exam_Cumulative_Subject.ObtainedMark_ofSubject,Exam_Cumulative_Subject.Cumulative_SubjectID ,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM            Exam_Cumulative_Subject INNER JOIN
                         StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID
WHERE        (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Student.Status = N'Active')) as a
	ON Exam_Cumulative_Subject.Cumulative_SubjectID = a.Cumulative_SubjectID
 END
ELSE
 BEGIN


	UPDATE  Exam_Cumulative_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Cumulative_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Cumulative_Subject.ObtainedMark_ofSubject DESC,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Cumulative_Subject.ObtainedMark_ofSubject,Exam_Cumulative_Subject.Cumulative_SubjectID ,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM            Exam_Cumulative_Subject INNER JOIN
                         StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID
WHERE        (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Student.Status = N'Active')) as a
	ON Exam_Cumulative_Subject.Cumulative_SubjectID = a.Cumulative_SubjectID

END
END



GO
