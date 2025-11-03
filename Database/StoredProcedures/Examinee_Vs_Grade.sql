-- ==========================================
-- Stored Procedure: Examinee_Vs_Grade
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Examinee_Vs_Grade]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Examinee_Vs_Grade]
END
GO

--24.
--ALTER PROCEDURE [dbo].[Examinee_Vs_Grade]
CREATE PROCEDURE [dbo].[Examinee_Vs_Grade]
 @SchoolID int, 
 @EducationYearID int ,
 @ClassID int ,
 @ExamID int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select *,count(*) as NoOfStudent,100.0 * COUNT(*)/(SELECT count(*) FROM Exam_Result_of_Student
WHERE(SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID))as Percentage from (SELECT Student_Grade FROM Exam_Result_of_Student
WHERE(SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)) as Exam_Result_of_Student_1

GROUP BY Exam_Result_of_Student_1.Student_Grade
END

GO
