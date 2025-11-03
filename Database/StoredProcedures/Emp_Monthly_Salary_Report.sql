-- ==========================================
-- Stored Procedure: Emp_Monthly_Salary_Report
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Emp_Monthly_Salary_Report]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Emp_Monthly_Salary_Report]
END
GO


CREATE PROCEDURE [dbo].[Emp_Monthly_Salary_Report]
 @SchoolID int ,
 @EducationYearID int,
 @RoleIDs nvarchar(Max) = null
AS
BEGIN
	SET NOCOUNT ON;
IF(@RoleIDs is not null)
BEGIN
SELECT        Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, Employee_Payorder_Monthly.MonthName, SUM(Employee_Payorder.PaidAmount) AS Paid, SUM(Employee_Payorder.Due) AS Due
FROM            Employee_Payorder INNER JOIN
                         VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID INNER JOIN
                         Employee_Payorder_Monthly ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Monthly.Employee_PayorderID
WHERE        (Employee_Payorder.SchoolID = @SchoolID) AND (Employee_Payorder.EducationYearID = @EducationYearID) AND Employee_Payorder.Employee_Payorder_NameID IN (Select id from dbo.In_Function_Parameter(@RoleIDs))
GROUP BY Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName, Employee_Payorder_Monthly.MonthName, Employee_Payorder.PaidAmount, 
                         Employee_Payorder_Monthly.MonthStartDate
ORDER BY Employee_Payorder_Monthly.MonthStartDate, VW_Emp_Info.ID
END
ELSE
BEGIN
SELECT        Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, Employee_Payorder_Monthly.MonthName, SUM(Employee_Payorder.PaidAmount) AS Paid , SUM(Employee_Payorder.Due) AS Due
FROM            Employee_Payorder INNER JOIN
                         VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID INNER JOIN
                         Employee_Payorder_Monthly ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Monthly.Employee_PayorderID
WHERE        (Employee_Payorder.SchoolID = @SchoolID) AND (Employee_Payorder.EducationYearID = @EducationYearID) 
GROUP BY Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName, Employee_Payorder_Monthly.MonthName, Employee_Payorder.PaidAmount, 
                         Employee_Payorder_Monthly.MonthStartDate
ORDER BY Employee_Payorder_Monthly.MonthStartDate, VW_Emp_Info.ID
END
END;

GO
