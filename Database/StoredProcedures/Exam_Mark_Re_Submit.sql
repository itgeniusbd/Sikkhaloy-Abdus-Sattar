-- ==========================================
-- Stored Procedure: Exam_Mark_Re_Submit
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Mark_Re_Submit]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Exam_Mark_Re_Submit]
END
GO

--7. ALTER PROCEDURE [dbo].[Exam_Mark_Re_Submit]


CREATE PROCEDURE [dbo].[Exam_Mark_Re_Submit]
    @SchoolID int,
	@EducationYearID int,
	@ClassID int,
	@ExamID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

--  UPDATE FullMark,PassMark,ObtainedPercentage,PassPercentage

 UPDATE       Exam_Obtain_Marks
    SET            FullMark =Exam_Full_Marks.FullMarks, 
                   PassMark =Exam_Full_Marks.Sub_PassMarks, 
				     ObtainedPercentage = ROUND((ISNULL(Exam_Obtain_Marks.MarksObtained ,0) * 100)/Exam_Full_Marks.FullMarks, 2, 0) , 
				     PassPercentage = ROUND((Exam_Full_Marks.Sub_PassMarks * 100 ) /Exam_Full_Marks.FullMarks, 2, 0)
FROM            Exam_Obtain_Marks INNER JOIN
                         Exam_Full_Marks ON Exam_Obtain_Marks.SchoolID = Exam_Full_Marks.SchoolID AND Exam_Obtain_Marks.ExamID = Exam_Full_Marks.ExamID AND Exam_Obtain_Marks.ClassID = Exam_Full_Marks.ClassID AND 
                         Exam_Obtain_Marks.SubjectID = Exam_Full_Marks.SubjectID AND ISNULL(Exam_Obtain_Marks.SubExamID, 0) = ISNULL(Exam_Full_Marks.SubExamID, 0) AND 
                         Exam_Obtain_Marks.EducationYearID = Exam_Full_Marks.EducationYearID
WHERE        (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ExamID = @ExamID)

--  UPDATE GradingID,ObtainedGrades,ObtainedPoint

 UPDATE       Exam_Obtain_Marks
    SET      	   GradingID = Exam_Grading_System.GradingID, 
 				   ObtainedGrades = Exam_Grading_System.Grades, 
 				   ObtainedPoint = Exam_Grading_System.Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Obtain_Marks ON Exam_Grading_System.MinPercentage <= Exam_Obtain_Marks.ObtainedPercentage AND 
                         Exam_Grading_System.MaxPercentage + 1 > Exam_Obtain_Marks.ObtainedPercentage INNER JOIN
                         Exam_Grading_Assign ON Exam_Obtain_Marks.SchoolID = Exam_Grading_Assign.SchoolID AND Exam_Obtain_Marks.EducationYearID = Exam_Grading_Assign.EducationYearID AND 
                         Exam_Obtain_Marks.ClassID = Exam_Grading_Assign.ClassID AND Exam_Obtain_Marks.ExamID = Exam_Grading_Assign.ExamID AND Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID AND 
                         Exam_Grading_System.SchoolID = Exam_Grading_Assign.SchoolID
WHERE        (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ExamID = @ExamID)


END


---------------------------------------------------------------------------------------------------------------------------------------------
--8. ALTER PROCEDURE [dbo].[Exam_Mark_Submit]

GO
