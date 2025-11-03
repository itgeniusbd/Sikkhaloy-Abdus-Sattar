-- ==========================================
-- Stored Procedure: Attendance_Students_API
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance_Students_API]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Attendance_Students_API]
END
GO


CREATE PROCEDURE [dbo].[Attendance_Students_API]
	 @SchoolID int,
	 @Entry_DateTime datetime, 
	 @StudentID int
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE  @Attendance_Date date
	DECLARE  @EntryTime time(7)
    DECLARE  @ScheduleID int
	DECLARE  @StartTime time(7)
	DECLARE  @EndTime time(7)
    DECLARE  @LateEntryTime time(7)
	DECLARE  @AttendanceStatus nvarchar(50)
	DECLARE  @ClassID int
	DECLARE  @StudentClassID int
	DECLARE  @Day nvarchar(50)

	 set @Attendance_Date	=  CONVERT(date, @Entry_DateTime)
     set @EntryTime = cast(@Entry_DateTime as time) 
	 set @Day = datename(dw,@Entry_DateTime) 

DECLARE @EducationYearID int
SELECT @EducationYearID = EducationYearID FROM  Education_Year WHERE  (Status = N'True') AND (SchoolID = @SchoolID)

SELECT @ScheduleID =  Attendance_Schedule_AssignStudent.ScheduleID,@StartTime = Attendance_Schedule_Day.StartTime,@EndTime = Attendance_Schedule_Day.EndTime,@LateEntryTime = Attendance_Schedule_Day.LateEntryTime
FROM   Attendance_Schedule_Day INNER JOIN Attendance_Schedule_AssignStudent ON Attendance_Schedule_Day.ScheduleID = Attendance_Schedule_AssignStudent.ScheduleID
WHERE  (Attendance_Schedule_AssignStudent.SchoolID = @SchoolID) AND (Attendance_Schedule_AssignStudent.StudentID = @StudentID) AND (Attendance_Schedule_Day.EducationYearID = @EducationYearID) AND (Attendance_Schedule_Day.Day = @Day)


if(@LateEntryTime < @EntryTime)
BEGIN
SELECT Attendance_Schedule_AssignStudent.Schedule_AssignStuID, Attendance_Schedule_AssignStudent.StudentID Into #Temp_Attendance_Assign
FROM Attendance_Schedule_AssignStudent INNER JOIN Student ON Attendance_Schedule_AssignStudent.StudentID = Student.StudentID
WHERE (Attendance_Schedule_AssignStudent.SchoolID = @SchoolID) AND (Attendance_Schedule_AssignStudent.EducationYearID = @EducationYearID) AND (Attendance_Schedule_AssignStudent.ScheduleID = @ScheduleID) AND (Student.Status = N'Active')


--loop start ------------------
	DECLARE  @Schedule_AssignStuID int
	DECLARE  @Loop_StudentID int 


While EXISTS(SELECT * From #Temp_Attendance_Assign)
Begin
--get data row by row into variable 
  SELECT Top 1 @Schedule_AssignStuID = Schedule_AssignStuID , @Loop_StudentID = StudentID From #Temp_Attendance_Assign

  SELECT @StudentClassID = StudentClassID,@ClassID = ClassID FROM StudentsClass WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentID = @Loop_StudentID)
  
  IF EXISTS (SELECT * FROM  Attendance_Leave WHERE (SchoolID = @SchoolID) AND (StudentID = @Loop_StudentID) AND (@Attendance_Date BETWEEN StartDate AND EndDate))
 BEGIN
  Set @AttendanceStatus='Leave'

  IF NOT EXISTS (SELECT * FROM Attendance_Record WHERE(SchoolID = @SchoolID) AND (StudentID = @Loop_StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID))
   BEGIN
     INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, Attendance, AttendanceDate, ExitConfirmed_Status)
     VALUES(@SchoolID,0, @EducationYearID, @Loop_StudentID, @ClassID, @StudentClassID, @AttendanceStatus, @Attendance_Date, 'Leave')
   END
 END 
 ELSE
BEGIN
Set @AttendanceStatus='Abs'

  IF NOT EXISTS (SELECT * FROM Attendance_Record WHERE(SchoolID = @SchoolID) AND (StudentID = @Loop_StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID))
   BEGIN
     INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID,ClassID, StudentClassID, Attendance, AttendanceDate, ExitConfirmed_Status)
     VALUES(@SchoolID,0, @EducationYearID, @Loop_StudentID, @ClassID, @StudentClassID,@AttendanceStatus, @Attendance_Date, 'Abs')
   END
END  
    Delete #Temp_Attendance_Assign Where Schedule_AssignStuID = @Schedule_AssignStuID
 END
DROP TABLE #Temp_Attendance_Assign
END



IF NOT EXISTS (SELECT * FROM  Attendance_Record WHERE(SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID))
 BEGIN
 IF(@StartTime >= @EntryTime)
  Set @AttendanceStatus='Pre'

 IF((@StartTime < @EntryTime) AND (@EntryTime <= @LateEntryTime))
   Set @AttendanceStatus='Late'

if(@EntryTime < @EndTime)
BEGIN
  SELECT @StudentClassID = StudentClassID,@ClassID = ClassID FROM StudentsClass WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentID = @StudentID)
  INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID,ClassID, StudentClassID, Attendance, AttendanceDate, EntryTime, ExitConfirmed_Status)
  VALUES(@SchoolID,0, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @AttendanceStatus, @Attendance_Date, @EntryTime,'No')
END
 END
ELSE
 BEGIN
 --If Employee Entry After Late Entry Time 
   IF((@LateEntryTime < @EntryTime) AND(@EntryTime < @EndTime))
    BEGIN
     Set @AttendanceStatus='Late Abs'
	 UPDATE Attendance_Record SET ExitConfirmed_Status = 'No', EntryTime = @EntryTime, Attendance = @AttendanceStatus WHERE(SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID) AND (Attendance = 'Abs')
	END 

  IF(@EndTime <= @EntryTime)
   BEGIN
    UPDATE Attendance_Record SET ExitTime = @EntryTime, ExitConfirmed_Status = 'Yes' WHERE(SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID)
   END
 END
END


GO
