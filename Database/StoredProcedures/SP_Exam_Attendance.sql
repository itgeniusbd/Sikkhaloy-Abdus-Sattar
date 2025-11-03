-- ==========================================
-- Stored Procedure: SP_Exam_Attendance
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Exam_Attendance]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[SP_Exam_Attendance]
END
GO

--22.

--CREATE PROCEDURE [dbo].[SP_Exam_Attendance]
CREATE PROCEDURE [dbo].[SP_Exam_Attendance]

-- Where condition parameters
	@SchoolID int,
    @EducationYearID int,
	@ClassID int,
	@ExamID int,
	@RegistrationID int,
    @From_Date date,
    @To_Date date 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--------[[[[[[[Delete]]]]]]]-------------------
DELETE FROM Attendance_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID)

IF(@From_Date <> '' and @To_Date  <> '')
BEGIN 
--------[[[[[[[INSERT]]]]]]]-------------------
INSERT INTO Attendance_Student
                         (SchoolID, RegistrationID, EducationYearID,ExamID, ClassID, StudentID,  StudentClassID, WorkingDays, TotalPresent, TotalAbsent, TotalLate, TotalLeave, TotalBunk,TotalLateAbs)

SELECT       @SchoolID, @RegistrationID, @EducationYearID, @ExamID, @ClassID,  StudentsClass.StudentID, Attendance_Record.StudentClassID, COUNT(Attendance_Record.StudentClassID) AS WorkingDay, ISNULL(T_Pre.Pre, 0) AS Pre, ISNULL(T_Abs.Abs, 0) AS Abs,  ISNULL(T_Late.Late, 0) AS Late,ISNULL(T_Leave.Leave, 0) AS Leave, ISNULL(T_Bunk.Bunk, 0) AS Bunk, ISNULL(T_LateAbs.LateAbs, 0) AS LateAbs
FROM            Attendance_Record INNER JOIN
                         StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Bunk
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Bunk')
                               GROUP BY StudentClassID) AS T_Bunk ON Attendance_Record.StudentClassID = T_Bunk.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Abs
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Abs')
                               GROUP BY StudentClassID) AS T_Abs ON Attendance_Record.StudentClassID = T_Abs.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Pre
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Pre')
                               GROUP BY StudentClassID) AS T_Pre ON Attendance_Record.StudentClassID = T_Pre.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Late
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Late')
                               GROUP BY StudentClassID) AS T_Late ON Attendance_Record.StudentClassID = T_Late.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Leave
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Leave')
                               GROUP BY StudentClassID) AS T_Leave ON Attendance_Record.StudentClassID = T_Leave.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS LateAbs
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Late Abs')
                               GROUP BY StudentClassID) AS T_LateAbs ON Attendance_Record.StudentClassID = T_LateAbs.StudentClassID
WHERE        (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.ClassID = @ClassID) AND (Attendance_Record.EducationYearID = @EducationYearID) AND (Attendance_Record.AttendanceDate BETWEEN 
                         @From_Date AND @To_Date)
GROUP BY Attendance_Record.StudentClassID, T_Abs.Abs, T_Pre.Pre, T_Leave.Leave, T_Late.Late, StudentsClass.StudentID, T_Bunk.Bunk, T_LateAbs.LateAbs
END
END

GO
