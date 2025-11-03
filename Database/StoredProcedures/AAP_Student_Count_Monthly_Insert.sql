-- ==========================================
-- Stored Procedure: AAP_Student_Count_Monthly_Insert
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AAP_Student_Count_Monthly_Insert]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[AAP_Student_Count_Monthly_Insert]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[AAP_Student_Count_Monthly_Insert]
AS
BEGIN
	SET NOCOUNT ON;  
	 

INSERT INTO AAP_StudentClass_Count_Monthly
                         (SchoolID, EducationYearID, ClassID,Active_Student, Reject_Countable, Reject_Uncountable)
SELECT   SchoolID, EducationYearID, ClassID, ActiveStudent, Reject_Countable, Reject_Uncountable
FROM            VW_Payment_Monthly_StudentClass

INSERT INTO AAP_Student_Count_Monthly
                         (SchoolID,Active_Student, Reject_Countable, Reject_Uncountable)
SELECT      SchoolID, ActiveStudent, Reject_Countable, Reject_Uncountable
FROM            VW_Payment_Monthly_Stu

END


GO
