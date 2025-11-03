-- ==========================================
-- Stored Procedure: Attendance_Employee_API
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance_Employee_API]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Attendance_Employee_API]
END
GO


CREATE PROCEDURE [dbo].[Attendance_Employee_API]
	 @SchoolID int,
	 @Entry_DateTime datetime, 
	 @EmployeeID int
	 
AS
BEGIN

	SET NOCOUNT ON;
	DECLARE  @Attendance_Date date
	DECLARE  @EntryTime time(7)
    DECLARE  @Employee_Attendance_ScheduleID int
	DECLARE  @LateEntryTime time(7)
	DECLARE  @StartTime time(7)
	DECLARE  @EndTime time(7)

	DECLARE  @AttendanceStatus nvarchar(50)


 set @Attendance_Date	=  CONVERT(date, @Entry_DateTime)
 set @EntryTime = cast(@Entry_DateTime as time) 

DECLARE @EducationYearID int
SELECT @EducationYearID = EducationYearID FROM  Education_Year WHERE  (Status = N'True') AND (SchoolID = @SchoolID)


SELECT @Employee_Attendance_ScheduleID = Employee_Attendance_Schedule_Assign.Employee_Attendance_ScheduleID,
 @LateEntryTime = Employee_Attendance_Schedule.LateEntryTime,
 @StartTime= Employee_Attendance_Schedule.StartTime,
 @EndTime = Employee_Attendance_Schedule.EndTime

FROM Employee_Attendance_Schedule_Assign INNER JOIN Employee_Attendance_Schedule ON Employee_Attendance_Schedule_Assign.Employee_Attendance_ScheduleID = Employee_Attendance_Schedule.Employee_Attendance_ScheduleID
WHERE (Employee_Attendance_Schedule_Assign.SchoolID = @SchoolID) AND (Employee_Attendance_Schedule_Assign.EmployeeID = @EmployeeID)




if( @LateEntryTime < @EntryTime)
BEGIN

SELECT Employee_Attendance_Schedule_Assign.Employee_Schedule_AssignID, Employee_Attendance_Schedule_Assign.EmployeeID Into #Temp_Attendance_Assign FROM  dbo.Employee_Attendance_Schedule_Assign INNER JOIN
dbo.Employee_Info ON dbo.Employee_Attendance_Schedule_Assign.EmployeeID = dbo.Employee_Info.EmployeeID
WHERE (dbo.Employee_Attendance_Schedule_Assign.SchoolID = @SchoolID) AND (dbo.Employee_Attendance_Schedule_Assign.Employee_Attendance_ScheduleID = @Employee_Attendance_ScheduleID) AND (dbo.Employee_Info.Job_Status = N'Active')


--loop start ------------------
	DECLARE  @Employee_Schedule_AssignID int
	DECLARE  @Loop_EmployeeID int 


While EXISTS(SELECT * From #Temp_Attendance_Assign)
Begin
--get data row by row into variable 
    Select Top 1 @Employee_Schedule_AssignID = Employee_Schedule_AssignID , @Loop_EmployeeID = EmployeeID  From #Temp_Attendance_Assign
  
  
  IF EXISTS (SELECT * FROM  Employee_Leave WHERE (SchoolID = @SchoolID) AND (EmployeeID = @Loop_EmployeeID) AND (@Attendance_Date BETWEEN LeaveStartDate AND LeaveEndDate))
 BEGIN
  Set @AttendanceStatus='Leave'

  IF NOT EXISTS (SELECT * FROM  Employee_Attendance_Record WHERE(SchoolID = @SchoolID) AND (EmployeeID = @Loop_EmployeeID) AND (AttendanceDate = @Attendance_Date))
   BEGIN
     INSERT INTO Employee_Attendance_Record (SchoolID, RegistrationID, EducationYearID, EmployeeID, AttendanceStatus, AttendanceDate,  ExitConfirmed_Status)
                                       VALUES(@SchoolID,0, @EducationYearID, @Loop_EmployeeID, @AttendanceStatus, @Attendance_Date, 'Leave')
   END
 END 
 ELSE
BEGIN
Set @AttendanceStatus='Abs'

  IF NOT EXISTS (SELECT * FROM  Employee_Attendance_Record WHERE(SchoolID = @SchoolID) AND (EmployeeID = @Loop_EmployeeID) AND (AttendanceDate = @Attendance_Date))
   BEGIN
     INSERT INTO Employee_Attendance_Record (SchoolID, RegistrationID, EducationYearID, EmployeeID, AttendanceStatus, AttendanceDate,  ExitConfirmed_Status)
                                       VALUES(@SchoolID,0, @EducationYearID, @Loop_EmployeeID, @AttendanceStatus, @Attendance_Date, 'Abs')
   END
END  
    Delete #Temp_Attendance_Assign Where Employee_Schedule_AssignID = @Employee_Schedule_AssignID
 END
DROP TABLE #Temp_Attendance_Assign
END




IF NOT EXISTS (SELECT * FROM  Employee_Attendance_Record WHERE(SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @Attendance_Date))
 BEGIN
 IF(@StartTime >= @EntryTime)
  Set @AttendanceStatus='Pre'

 IF((@StartTime < @EntryTime) AND (@EntryTime <= @LateEntryTime))
   Set @AttendanceStatus='Late'

 IF(@EntryTime < @EndTime)
  INSERT INTO Employee_Attendance_Record (SchoolID, RegistrationID, EducationYearID, EmployeeID, AttendanceStatus, AttendanceDate, EntryTime, ExitConfirmed_Status)
  VALUES(@SchoolID,0, @EducationYearID, @EmployeeID, @AttendanceStatus, @Attendance_Date, @EntryTime,'No')
 END
ELSE
 BEGIN
 --If Employee Entry After Late Entry Time 
   IF((@LateEntryTime < @EntryTime) AND(@EntryTime < @EndTime))
    BEGIN
     Set @AttendanceStatus='Late Abs'
	 UPDATE Employee_Attendance_Record SET  ExitConfirmed_Status = 'No', EntryTime = @EntryTime, AttendanceStatus = @AttendanceStatus WHERE(SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @Attendance_Date) AND (ExitTime IS NULL) AND (AttendanceStatus='Abs')
	END 

  IF(@EndTime <= @EntryTime)
   BEGIN
    UPDATE Employee_Attendance_Record SET ExitTime = @EntryTime, ExitConfirmed_Status = 'Yes' WHERE(SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @Attendance_Date)
   END
 END
END




GO
