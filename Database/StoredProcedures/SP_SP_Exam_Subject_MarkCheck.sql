-- ==========================================
-- Stored Procedure: SP_SP_Exam_Subject_MarkCheck
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_SP_Exam_Subject_MarkCheck]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_SP_Exam_Subject_MarkCheck]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_SP_Exam_Subject_MarkCheck]

-- Where condition parameters
@SchoolID NVARCHAR(10),
@ClassID NVARCHAR(10),
@EducationYearID NVARCHAR(10),
@SectionID nvarchar(10),
@SubjectGroupID nvarchar(10),
@ShiftID nvarchar(10),
@SubjectID nvarchar(10),
@ExamID nvarchar(10)


AS
BEGIN
SET NOCOUNT ON;
 DECLARE @PivotColumnHeaders NVARCHAR(MAX)

 DECLARE @PivotTableSQL NVARCHAR(MAX)


 declare @Status nvarchar(50) ='Active'



SELECT @PivotColumnHeaders = COALESCE(@PivotColumnHeaders +',[' + ISNULL(Exam_SubExam_Name.SubExamName,'Marks') + ']','[' + ISNULL(Exam_SubExam_Name.SubExamName,'Marks') + ']')
FROM  Exam_Full_Marks LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.SchoolID = @SchoolID) AND (Exam_Full_Marks.SubjectID = @SubjectID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ClassID = @ClassID)

ORDER BY Exam_SubExam_Name.Sub_ExamSN

SET @PivotTableSQL = N'SELECT ID, StudentsName as Name, RollNo as Roll, '+ @PivotColumnHeaders + N'
FROM (SELECT StudentsClass.StudentID, StudentsClass.StudentClassID, Student.ID, Student.StudentsName, StudentsClass.RollNo, ISNULL(Exam_SubExam_Name.SubExamName,''Marks'')AS SubExamName, 
 Exam_Obtain_Marks.MarksObtained FROM StudentsClass INNER JOIN
                                                    Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN
                                                    Exam_Obtain_Marks ON StudentsClass.StudentClassID = Exam_Obtain_Marks.StudentClassID AND StudentsClass.SchoolID = Exam_Obtain_Marks.SchoolID AND 
                                                    StudentsClass.StudentID = Exam_Obtain_Marks.StudentID AND StudentsClass.EducationYearID = Exam_Obtain_Marks.EducationYearID LEFT OUTER JOIN
                                                    Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID
                          WHERE  StudentsClass.ClassID = '+ @ClassID + ' AND 
						         StudentsClass.SectionID LIKE '''+ @SectionID + ''' AND 
								 StudentsClass.SubjectGroupID LIKE '''+ @SubjectGroupID + ''' AND 
                                 StudentsClass.EducationYearID = '+ @EducationYearID + ' AND 
								 StudentsClass.ShiftID LIKE '''+ @ShiftID + ''' AND 
								 StudentsClass.SchoolID = '+ @SchoolID + ' AND 
								 Student.Status = ''Active'' AND 
                                 Exam_Obtain_Marks.SubjectID = '+ @SubjectID + ' AND
								 Exam_Obtain_Marks.ExamID = '+ @ExamID + '
								 ) AS PivotData PIVOT (MAX(MarksObtained) FOR [SubExamName] IN ( ' + @PivotColumnHeaders + ' )) AS PivotTable ORDER BY CASE WHEN ISNUMERIC(RollNo) = 1 THEN CAST(REPLACE(REPLACE(RollNo , ''$'' , '''') , '','' , '''') AS INT) ELSE 0 END'

EXECUTE(@PivotTableSQL)
END

GO
