-- ==========================================
-- Stored Procedure: Result_of_Cumulative_Full_Class
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Result_of_Cumulative_Full_Class]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Result_of_Cumulative_Full_Class]
END
GO

CREATE PROCEDURE [dbo].[Result_of_Cumulative_Full_Class]
@ClassID int,
@SchoolID int,
@EducationYearID int
AS
BEGIN
SELECT Exam_Result_of_Student.StudentID, 
Student.ID, 
Student.StudentsName,
Student.SMSPhoneNo, 
ROUND(SUM(Exam_Result_of_Student.TotalMark_ofStudent),2) AS TotalMark,
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent),2) AS ObtainedMark, 
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*),2) AS Avarage,
ROUND(AVG(Exam_Result_of_Student.Student_Point),2) AS Point,
(SELECT Grades FROM Exam_Grading_System WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND ( round (((SUM(Exam_Result_of_Student.ObtainedMark_ofStudent)/SUM(Exam_Result_of_Student.TotalMark_ofStudent))*100),0) BETWEEN MinPercentage AND MaxPercentage)) as Grade, 
DENSE_RANK() OVER (ORDER BY SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*) DESC) AS Position

FROM Exam_Result_of_Student INNER JOIN
Exam_Cumulative_ExamList ON Exam_Result_of_Student.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Student.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND 
Exam_Result_of_Student.ClassID = Exam_Cumulative_ExamList.ClassID INNER JOIN
Student ON Exam_Result_of_Student.StudentID = Student.StudentID

WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) 
AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) 
AND (Exam_Result_of_Student.ClassID = @ClassID)
GROUP BY Exam_Result_of_Student.StudentID, Student.ID, Student.SMSPhoneNo, Student.StudentsName
END

GO
