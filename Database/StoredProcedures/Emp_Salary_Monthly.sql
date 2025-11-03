-- ==========================================
-- Stored Procedure: Emp_Salary_Monthly
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Emp_Salary_Monthly]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Emp_Salary_Monthly]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Emp_Salary_Monthly]
	 @SchoolID int,
	 @RegistrationID int,
	 @EducationYearID int,
	 @EmployeeID int,
	 @Employee_Payorder_NameID int,

	 @Get_date date,
	 @MonthName nvarchar(50),

     @GeT_Employee_PayorderID int out
AS
BEGIN
	SET NOCOUNT ON;  
	DECLARE  @PayorderAmount float
	DECLARE  @IS_Abs_Deducted bit
	DECLARE  @Abs_Deduction float
	DECLARE  @IS_Late_Count_As_Abs bit
	DECLARE  @Employee_PayorderID int 
	DECLARE  @Late_Days int 


IF NOT EXISTS(SELECT * FROM  Employee_Payorder_Monthly WHERE([MonthName] = @MonthName) AND (EmployeeID = @EmployeeID))
BEGIN

	SELECT @PayorderAmount = Salary, @IS_Abs_Deducted = IS_Abs_Deducted, @Abs_Deduction = Abs_Deduction,@IS_Late_Count_As_Abs = IS_Late_Count_As_Abs , @Late_Days =Late_Days FROM  Employee_Info WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)



	INSERT INTO Employee_Payorder
                         (SchoolID, RegistrationID, EducationYearID, EmployeeID, Employee_Payorder_NameID, PayorderAmount,  Employee_Payorder_SN)
                VALUES (@SchoolID, @RegistrationID,@EducationYearID,@EmployeeID,@Employee_Payorder_NameID, @PayorderAmount, [dbo].[Employee_Payorder_SN](@SchoolID))

--get the Employee_PayorderID
  set  @Employee_PayorderID = (SELECT SCOPE_IDENTITY())

--insert  Employee_Payorder_Monthly Table
  DECLARE @S_date date = DATEADD(mm, DATEDIFF(mm, 0, @Get_date), 0)
  DECLARE @E_date date = DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, @Get_date) + 1, 0))

  DECLARE  @Total_WorkingDays int


-- Total Working days of Employees
	SELECT @Total_WorkingDays = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
    WHERE (SchoolID = @SchoolID)  AND (EmployeeID = @EmployeeID) AND (AttendanceDate BETWEEN @S_date AND @E_date)

--Total Absent in Month
DECLARE  @Total_Abs int
DECLARE  @Total_Late int
DECLARE  @Total_Leave int
DECLARE  @Total_LateCount int
DECLARE  @Fine_Amount float
DECLARE  @FineCountDays int
DECLARE  @Total_Pre int

---Total Pre
SELECT @Total_Pre = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus ='Pre') AND (AttendanceDate BETWEEN @S_date AND @E_date)
--- Total Abs
SELECT @Total_Abs = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus ='Abs') AND (AttendanceDate BETWEEN @S_date AND @E_date)
--- Total Late and Late Abs 
SELECT @Total_Late = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus in('Late','Late Abs')) AND (AttendanceDate BETWEEN @S_date AND @E_date)
---Total Leave
SELECT @Total_Leave = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus = 'Leave') AND (AttendanceDate BETWEEN @S_date AND @E_date)

---------is Late Deducted--------------------
  IF(@IS_Late_Count_As_Abs = 1)
    BEGIN
	  set @Total_LateCount = @Total_Late / @Late_Days
   END
 ELSE
   BEGIN
      SET  @Total_LateCount = 0
   END

   SET @FineCountDays = @Total_Abs + @Total_LateCount

   SET @Fine_Amount =  @FineCountDays * @Abs_Deduction

  INSERT INTO Employee_Payorder_Monthly
                     (Employee_PayorderID, SchoolID, RegistrationID, EducationYearID, EmployeeID, [MonthName], MonthStartDate, MonthEndDate, Amount, WorkingDays,FineCountDays,FineAmount,LateDays, LeaveDays, AbsDays, PerDays)
            VALUES  (@Employee_PayorderID,@SchoolID,@RegistrationID,@EducationYearID,@EmployeeID, @MonthName,  @S_date,        @E_date, @PayorderAmount, @Total_WorkingDays,@FineCountDays,@Fine_Amount, @Total_Late, @Total_Leave, @Total_Abs, @Total_Pre)


--Employee_Allowance_Assign are insert to records
SELECT AllowanceAssignID, AllowanceID, AllowanceAmount, Fixed_Percetage  Into #Temp_Allowance_Assign  FROM  Employee_Allowance_Assign WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)
--loop start ------------------
	DECLARE  @AllowanceAssignID int
	DECLARE  @AllowanceID int 
	DECLARE  @Amount float
	DECLARE  @Fixed_Percetage nvarchar(50)
	DECLARE  @AllowanceAmount float

While EXISTS(SELECT * From #Temp_Allowance_Assign)
Begin
--get data row by row into variable 
    Select Top 1 @AllowanceAssignID = AllowanceAssignID,@AllowanceID =  AllowanceID,  @Amount = AllowanceAmount, @Fixed_Percetage = Fixed_Percetage   From #Temp_Allowance_Assign
  
  if(@Fixed_Percetage ='Fixed')
      set @AllowanceAmount = @Amount
  else
  set @AllowanceAmount = (@PayorderAmount *  @Amount)/100

   INSERT INTO Employee_Allowance_Records
       (SchoolID, RegistrationID, AllowanceID, EmployeeID, Employee_PayorderID, AllowanceAmount)
VALUES (@SchoolID, @RegistrationID, @AllowanceID, @EmployeeID, @Employee_PayorderID, @AllowanceAmount)
   
   Delete #Temp_Allowance_Assign Where AllowanceAssignID = @AllowanceAssignID
 END
 DROP TABLE #Temp_Allowance_Assign


--Employee_Deduction_Assign are insert to records
SELECT DeductionAssignID, DeductionID, DeductionAmount, Fixed_Percetage  Into #Temp_Employee_Deduction_Assign  FROM  Employee_Deduction_Assign WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)
--loop start ------------------
	DECLARE  @DeductionAssignID int
	DECLARE  @DeductionID int 
	DECLARE  @D_Amount float
	DECLARE  @D_Fixed_Percetage nvarchar(50)
	DECLARE  @DeductionAmount float

While EXISTS(SELECT * From #Temp_Employee_Deduction_Assign)
Begin
--get data row by row into variable 
    Select Top 1 @DeductionAssignID = DeductionAssignID,@DeductionID =  DeductionID,  @D_Amount = DeductionAmount, @D_Fixed_Percetage = Fixed_Percetage   From #Temp_Employee_Deduction_Assign
  
  if(@D_Fixed_Percetage ='Fixed')
      set @DeductionAmount = @D_Amount
  else
  set @DeductionAmount = (@PayorderAmount *  @D_Amount)/100

   INSERT INTO Employee_Deduction_Records
       (SchoolID, RegistrationID, DeductionID, EmployeeID, Employee_PayorderID, Deduction_Amount)
VALUES (@SchoolID, @RegistrationID, @DeductionID, @EmployeeID, @Employee_PayorderID, @DeductionAmount)
   
   Delete #Temp_Employee_Deduction_Assign Where DeductionAssignID = @DeductionAssignID
 END
 DROP TABLE #Temp_Employee_Deduction_Assign

 SET @GeT_Employee_PayorderID = @Employee_PayorderID

 RETURN @GeT_Employee_PayorderID
 END
END


GO
