-- ==========================================
-- Stored Procedure: Exam_Mark_Submit
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Mark_Submit]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[Exam_Mark_Submit]
END
GO


CREATE PROCEDURE [dbo].[Exam_Mark_Submit]

    @SchoolID int,
	@RegistrationID int,
	@EducationYearID int,
	@StudentID int,
	@ClassID int,
	@ExamID int,
	@SubjectID int,
	@SubExamID int,

	@MarksObtained float,
	@AbsenceStatus nvarchar(50),
	@FullMark float,
	@PassPercentage float,
	@PassMark float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @StudentResultID int
    DECLARE @StudentClassID int
	DECLARE @StudentRecordID int
	DECLARE @ObtainedPercentage float

	DECLARE @GradingID int
	DECLARE @Grades nvarchar(50)
	DECLARE @Point float


	SELECT @StudentClassID = StudentClassID FROM StudentsClass WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SchoolID = @SchoolID)

	SELECT @StudentRecordID = StudentRecordID FROM  StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SubjectID = @SubjectID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID)



 IF NOT EXISTS (SELECT StudentResultID FROM Exam_Result_of_Student WHERE SchoolID =@SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND ExamID = @ExamID)
 BEGIN
	INSERT INTO Exam_Result_of_Student
           (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, ClassID, ExamID,Date)
    VALUES (@SchoolID,@RegistrationID,@EducationYearID,@StudentID,@StudentClassID,@ClassID,@ExamID,GETDATE())

	set @StudentResultID = SCOPE_IDENTITY();
 END

ELSE
 BEGIN
    SELECT @StudentResultID =  StudentResultID FROM Exam_Result_of_Student WHERE SchoolID =@SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND ExamID = @ExamID
 END

 IF NOT EXISTS (SELECT * FROM Exam_Result_of_Subject WHERE SchoolID =@SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND ExamID = @ExamID AND SubjectID = @SubjectID AND StudentResultID = @StudentResultID)
 BEGIN
 INSERT INTO Exam_Result_of_Subject
         (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, ClassID, ExamID, StudentRecordID, SubjectID, StudentResultID, Date)
 VALUES  (@SchoolID,@RegistrationID,@EducationYearID,@StudentID,@StudentClassID,@ClassID,@ExamID,@StudentRecordID,@SubjectID,@StudentResultID,GETDATE())
 END


 SET @ObtainedPercentage = (ISNULL(@MarksObtained,0) * 100)/@FullMark

 SELECT TOP (1) @GradingID = Exam_Grading_System.GradingID, 
                @Grades = Exam_Grading_System.Grades,
			    @Point  = Exam_Grading_System.Point
FROM   Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_System.MinPercentage <= @ObtainedPercentage) AND (Exam_Grading_Assign.SchoolID = @SchoolID) AND (Exam_Grading_Assign.EducationYearID = @EducationYearID) AND 
                         (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
ORDER BY Exam_Grading_System.Point DESC



  IF NOT EXISTS (SELECT * From Exam_Obtain_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID) AND (ExamID = @ExamID) AND (SubjectID = @SubjectID) AND (StudentResultID = @StudentResultID) AND (SubExamID = @SubExamID OR SubExamID IS NULL))
 BEGIN
 INSERT INTO Exam_Obtain_Marks
         (SchoolID, RegistrationID, StudentID, SubjectID, ClassID, ExamID, SubExamID, StudentClassID, EducationYearID, StudentRecordID, StudentResultID, 
		 MarksObtained, AbsenceStatus, FullMark, ObtainedPercentage, PassPercentage, Date,GradingID,ObtainedGrades,ObtainedPoint,PassMark)
 VALUES  (@SchoolID,@RegistrationID,@StudentID,@SubjectID,@ClassID,@ExamID,@SubExamID,@StudentClassID,@EducationYearID,@StudentRecordID,@StudentResultID,
         @MarksObtained,@AbsenceStatus,@FullMark,@ObtainedPercentage,@PassPercentage,GETDATE(),@GradingID,@Grades,@Point,@PassMark)
 END
   ELSE
   BEGIN
    update Exam_Obtain_Marks set MarksObtained = @MarksObtained , SubExamID = @SubExamID ,AbsenceStatus = @AbsenceStatus,FullMark = @FullMark,ObtainedPercentage = @ObtainedPercentage, PassPercentage = @PassPercentage,GradingID = @GradingID,ObtainedGrades = @Grades,ObtainedPoint = @Point, PassMark = @PassMark
	 Where StudentClassID = @StudentClassID and SubjectID = @SubjectID and ExamID = @ExamID and (SubExamID = @SubExamID or SubExamID is null)
   END

END

----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--9. ALTER PROCEDURE [dbo].[SP_Exam_Subject]

GO
