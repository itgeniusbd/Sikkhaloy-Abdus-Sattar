-- ==========================================
-- Stored Procedure: Student_Monthly_AttendanceFine
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Student_Monthly_AttendanceFine]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Student_Monthly_AttendanceFine]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Student_Monthly_AttendanceFine]
	 @SchoolID int,
	 @RegistrationID int,
	 @EducationYearID int,
	 @ClassID int,


	 @Get_date date,
	 @MonthName nvarchar(50)

AS
BEGIN
	SET NOCOUNT ON;  
	 DECLARE  @StartDate date = getdate();
	 DECLARE  @EndDate date = DATEADD(month, 1, GETDATE());


	  DECLARE  @Role nvarchar(50) = N'Monthly Attendance Fine'
	  DECLARE  @RoleID int 
	   
	  IF NOT EXISTS(SELECT RoleID FROM Income_Roles WHERE (SchoolID = @SchoolID) AND (Role = @Role))
      BEGIN
      INSERT INTO Income_Roles(SchoolID, RegistrationID, Role, NumberOfPay)VALUES(@SchoolID, @RegistrationID, @Role, 1)
      END
	   
	   SELECT @RoleID = RoleID FROM Income_Roles WHERE (SchoolID = @SchoolID) AND (Role = @Role)
   



	  DECLARE  @StudentClassID int 
	  DECLARE  @StudentID int 
	  DECLARE  @WorkingDays int
	  DECLARE  @TotalPresent int 
      DECLARE  @TotalAbsent int  
	  DECLARE  @TotalLeave int 
	  DECLARE  @TotalBunk int 
      DECLARE  @TotalLateAbs int
	  DECLARE  @TotalLate int
	  DECLARE   @FineAmount float
	  DECLARE   @PayOrderID int

	  DECLARE @From_Date date = DATEADD(mm, DATEDIFF(mm, 0, @Get_date), 0)
      DECLARE @To_Date date   = DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, @Get_date) + 1, 0))


SELECT StudentsClass.StudentID, Attendance_Record.StudentClassID, Attendance_Record.ClassID, COUNT(Attendance_Record.StudentClassID) AS WorkingDay, ISNULL(T_Pre.Pre, 0) AS Pre, ISNULL(T_Abs.Abs, 0) AS Abs,  ISNULL(T_Late.Late, 0) AS Late,ISNULL(T_Leave.Leave, 0) AS Leave, ISNULL(T_Bunk.Bunk, 0) AS Bunk, ISNULL(T_LateAbs.LateAbs, 0) AS LateAbs
 Into #Temp_Table  FROM Attendance_Record INNER JOIN
                         StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Bunk
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Bunk')
                               GROUP BY StudentClassID) AS T_Bunk ON Attendance_Record.StudentClassID = T_Bunk.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Abs
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Abs')
                               GROUP BY StudentClassID) AS T_Abs ON Attendance_Record.StudentClassID = T_Abs.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Pre
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Pre')
                               GROUP BY StudentClassID) AS T_Pre ON Attendance_Record.StudentClassID = T_Pre.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Late
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Late')
                               GROUP BY StudentClassID) AS T_Late ON Attendance_Record.StudentClassID = T_Late.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Leave
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Leave')
                               GROUP BY StudentClassID) AS T_Leave ON Attendance_Record.StudentClassID = T_Leave.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS LateAbs
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Late Abs')
                               GROUP BY StudentClassID) AS T_LateAbs ON Attendance_Record.StudentClassID = T_LateAbs.StudentClassID
WHERE        (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.ClassID = @ClassID  OR @ClassID = 0) AND (Attendance_Record.EducationYearID = @EducationYearID) AND (Attendance_Record.AttendanceDate BETWEEN 
                         @From_Date AND @To_Date)
GROUP BY Attendance_Record.StudentClassID,Attendance_Record.ClassID, T_Abs.Abs, T_Pre.Pre, T_Leave.Leave, T_Late.Late, StudentsClass.StudentID, T_Bunk.Bunk, T_LateAbs.LateAbs




While EXISTS(SELECT * From #Temp_Table)
Begin

  SELECT Top 1 @StudentClassID = StudentClassID,
               @StudentID = StudentID, 
			   @ClassID = ClassID, 
			   @WorkingDays = WorkingDay,
			   @TotalPresent = Pre,
			   @TotalLate =Late,
			   @TotalAbsent = Abs,  
			   @TotalLeave = Leave, 
			   @TotalBunk = Bunk,  
			   @TotalLateAbs= LateAbs
 From #Temp_Table


 IF NOT EXISTS(SELECT PayOrderID FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (StudentClassID = @StudentClassID) AND (ClassID = @ClassID) AND (PayFor = @MonthName) AND (RoleID = @RoleID))
BEGIN
 DECLARE @AbsFineAmount float
 DECLARE @LateFineAmount float
 DECLARE @BunkFineAmount float

SELECT @AbsFineAmount = ISNULL(FineAmount,0) FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (FineFor ='Abs')
SELECT @LateFineAmount = ISNULL(FineAmount,0) FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (FineFor ='Late')
SELECT @BunkFineAmount = ISNULL(FineAmount,0) FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (FineFor ='Bunk')

SET @FineAmount = ((ISNULL(@TotalAbsent,0) +  ISNULL(@TotalLateAbs,0)) * ISNULL(@AbsFineAmount,0)) + (ISNULL(@TotalLate,0) * ISNULL(@LateFineAmount,0)) + (ISNULL(@TotalBunk,0) * ISNULL(@BunkFineAmount,0))

IF(@FineAmount > 0)
BEGIN
INSERT INTO Income_PayOrder(SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, Amount, RoleID, PayFor, StartDate, EndDate, EducationYearID) 
VALUES(@SchoolID, @RegistrationID, @StudentID, @ClassID, @StudentClassID, @FineAmount, @RoleID, @MonthName, @StartDate, @EndDate, @EducationYearID)
      
SET @PayOrderID = (SELECT SCOPE_IDENTITY())

INSERT INTO  Attendance_Monthly_Report (SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, [MonthName], MonthStartDate, MonthEndDate, FineAmount, WorkingDays, TotalPresent, TotalAbsent, TotalLateAbs, TotalLate, TotalLeave, TotalBunk, PayOrderID)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @MonthName, @From_Date, @To_Date, @FineAmount, @WorkingDays, @TotalPresent, @TotalAbsent, @TotalLateAbs,@TotalLate, @TotalLeave, @TotalBunk, @PayOrderID)
END
END
   Delete #Temp_Table Where StudentClassID = @StudentClassID 
END
 DROP TABLE #Temp_Table

END


GO
