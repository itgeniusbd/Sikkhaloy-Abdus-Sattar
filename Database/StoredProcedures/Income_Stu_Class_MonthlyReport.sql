-- ==========================================
-- Stored Procedure: Income_Stu_Class_MonthlyReport
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Income_Stu_Class_MonthlyReport]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Income_Stu_Class_MonthlyReport]
END
GO

CREATE PROCEDURE [dbo].[Income_Stu_Class_MonthlyReport]
 @SchoolID int ,
 @EducationYearID int,
 @SectionID nvarchar(50),
 @ClassID int,
 @RoleIDs nvarchar(Max)
AS
BEGIN
	SET NOCOUNT ON;
IF(@RoleIDs is not null)
BEGIN
SELECT StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8) AS Month, SUM(Income_PayOrder.PaidAmount) AS Amount
FROM  Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) 
 AND Student.Status='Active' AND Income_PayOrder.RoleID IN (Select id from dbo.In_Function_Parameter(@RoleIDs))
GROUP BY StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8)
ORDER BY MAX(Income_PayOrder.EndDate) ,CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
END
ELSE
BEGIN
SELECT StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8) AS Month, SUM(Income_PayOrder.PaidAmount) AS Amount
FROM Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE(Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND Student.Status='Active' AND (StudentsClass.SectionID LIKE @SectionID)
GROUP BY StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8)
ORDER BY MAX(Income_PayOrder.EndDate) ,CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
END
END;

GO
