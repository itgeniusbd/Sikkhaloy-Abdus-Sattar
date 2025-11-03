-- ==========================================
-- Stored Procedure: Result_of_Cumulative
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Result_of_Cumulative]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Result_of_Cumulative]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[Result_of_Cumulative]

-- Where condition parameters

	@ClassID int,
	@SchoolID int,
    @EducationYearID int

	
AS
BEGIN
select * from (SELECT Exam_Result_of_Student.StudentID, 
ROUND(SUM(Exam_Result_of_Student.TotalMark_ofStudent),2) AS TotalMark,
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent),2) AS ObtainedMark, 
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*),2) AS Avarage,
ROUND(AVG(Exam_Result_of_Student.Student_Point),2) AS Point,
(SELECT Grades FROM Exam_Grading_System WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND ( round (((SUM(Exam_Result_of_Student.ObtainedMark_ofStudent)/SUM(Exam_Result_of_Student.TotalMark_ofStudent))*100),0) BETWEEN MinPercentage AND MaxPercentage)) as Grade, 
DENSE_RANK() OVER (ORDER BY SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*) DESC) AS Position

FROM Exam_Result_of_Student INNER JOIN Exam_Cumulative_ExamList ON Exam_Result_of_Student.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Student.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND  Exam_Result_of_Student.ClassID = Exam_Cumulative_ExamList.ClassID

WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) 
AND   (Exam_Result_of_Student.EducationYearID = @EducationYearID) 
AND   (Exam_Result_of_Student.ClassID = @ClassID) 
GROUP BY Exam_Result_of_Student.StudentID) as CU 	

INNER JOIN

(SELECT Exam_Result_of_Student.StudentID,
Student.ID, 
 Student.StudentsName, 
 CreateClass.Class, 
 Exam_Name.ExamName,
 Exam_Name.ExamID,
 ROUND(Exam_Result_of_Student.TotalMark_ofStudent, 2) AS exam_TM, 
 ROUND(Exam_Result_of_Student.ObtainedMark_ofStudent, 2) AS exam_OM,
 ROUND(Exam_Result_of_Student.Student_Point, 2) AS Exam_Point, 
 Exam_Result_of_Student.Student_Grade AS Exam_Grade

FROM Exam_Result_of_Student INNER JOIN
Exam_Cumulative_ExamList ON Exam_Result_of_Student.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Student.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND 
Exam_Result_of_Student.ClassID = Exam_Cumulative_ExamList.ClassID INNER JOIN
Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID INNER JOIN
Student ON Exam_Result_of_Student.StudentID = Student.StudentID INNER JOIN
CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID

WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) 
AND   (Exam_Result_of_Student.EducationYearID = @EducationYearID) 
AND   (Exam_Result_of_Student.ClassID = @ClassID)) as Exam ON cu.StudentID = Exam.StudentID 
order by ID,ExamID
END
GO
