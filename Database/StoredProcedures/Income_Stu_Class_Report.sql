-- ==========================================
-- Stored Procedure: Income_Stu_Class_Report
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Income_Stu_Class_Report]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Income_Stu_Class_Report]
END
GO

create PROCEDURE [dbo].[Income_Stu_Class_Report]
 @SchoolID int ,
 @EducationYearID int,
 @From_Date date,
 @To_Date date,
 @SectionID nvarchar(50),
 @ClassID int,
 @RoleID nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

SELECT StudentsClass.ClassID, Income_PaymentRecord.RoleID, CreateClass.Class, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, Income_Roles.Role, 
      Income_PaymentRecord.PaidAmount,Income_PaymentRecord.PaidDate, RIGHT(CONVERT(VARCHAR(11), Income_PaymentRecord.PaidDate, 106), 8) AS Month
FROM  StudentsClass INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN
                         Income_PaymentRecord INNER JOIN
                         Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID ON StudentsClass.StudentClassID = Income_PaymentRecord.StudentClassID INNER JOIN
                         Student ON Income_PaymentRecord.StudentID = Student.StudentID LEFT OUTER JOIN
                         CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_PaymentRecord.EducationYearID = @EducationYearID) AND (CAST(Income_PaymentRecord.PaidDate AS date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (StudentsClass.SectionID like @SectionID) AND (StudentsClass.ClassID = @ClassID) AND (Income_PaymentRecord.RoleID LIKE @RoleID)
ORDER BY CreateClass.ClassID
END;

GO
