-- ==========================================
-- Stored Procedure: Attendance_Students_API
-- Multi-schedule aware: resolves schedule by punch time
-- and stores Attendance_ScheduleID per record.
-- ==========================================

USE [Edu]
GO

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

    DECLARE @Attendance_Date date
    DECLARE @EntryTime time(7)
    DECLARE @ScheduleID int
    DECLARE @StartTime time(7)
    DECLARE @EndTime time(7)
    DECLARE @LateEntryTime time(7)
    DECLARE @AttendanceStatus nvarchar(50)
    DECLARE @ClassID int
    DECLARE @StudentClassID int
    DECLARE @Day nvarchar(50)
    DECLARE @EducationYearID int

    SET @Attendance_Date = CONVERT(date, @Entry_DateTime)
    SET @EntryTime = CAST(@Entry_DateTime AS time)
    SET @Day = DATENAME(dw, @Entry_DateTime)

    SELECT @EducationYearID = EducationYearID
    FROM Education_Year
    WHERE Status = N'True' AND SchoolID = @SchoolID

    -- Resolve active schedule for this punch (multi-schedule aware)
    SELECT TOP 1
        @ScheduleID = sd.ScheduleID,
        @StartTime = sd.StartTime,
        @EndTime = sd.EndTime,
        @LateEntryTime = sd.LateEntryTime
    FROM Attendance_Schedule_AssignStudent ass
    INNER JOIN Attendance_Schedule_Day sd ON ass.ScheduleID = sd.ScheduleID
    WHERE ass.SchoolID = @SchoolID
      AND ass.StudentID = @StudentID
      AND sd.SchoolID = @SchoolID
      AND sd.Day = @Day
      AND sd.Is_OnDay = 1
    ORDER BY
        CASE WHEN @EntryTime >= sd.StartTime AND @EntryTime <= sd.EndTime THEN 0 ELSE 1 END,
        ABS(DATEDIFF(MINUTE, CAST(sd.StartTime AS datetime), CAST(@EntryTime AS datetime)))

    IF @ScheduleID IS NULL
        RETURN

    -- Mark absent/leave for students in this schedule after late entry time
    IF (@LateEntryTime < @EntryTime)
    BEGIN
        SELECT
            ass.Schedule_AssignStuID,
            ass.StudentID
        INTO #Temp_Attendance_Assign
        FROM Attendance_Schedule_AssignStudent ass
        INNER JOIN Student s ON ass.StudentID = s.StudentID
        WHERE ass.SchoolID = @SchoolID
          AND ass.ScheduleID = @ScheduleID
          AND s.Status = N'Active'

        DECLARE @Schedule_AssignStuID int
        DECLARE @Loop_StudentID int

        WHILE EXISTS (SELECT 1 FROM #Temp_Attendance_Assign)
        BEGIN
            SELECT TOP 1
                @Schedule_AssignStuID = Schedule_AssignStuID,
                @Loop_StudentID = StudentID
            FROM #Temp_Attendance_Assign

            SELECT
                @StudentClassID = StudentClassID,
                @ClassID = ClassID
            FROM StudentsClass
            WHERE SchoolID = @SchoolID
              AND EducationYearID = @EducationYearID
              AND StudentID = @Loop_StudentID

            IF EXISTS (
                SELECT 1
                FROM Attendance_Leave
                WHERE SchoolID = @SchoolID
                  AND StudentID = @Loop_StudentID
                  AND @Attendance_Date BETWEEN StartDate AND EndDate
            )
            BEGIN
                SET @AttendanceStatus = 'Leave'

                IF NOT EXISTS (
                    SELECT 1
                    FROM Attendance_Record
                    WHERE SchoolID = @SchoolID
                      AND StudentID = @Loop_StudentID
                      AND AttendanceDate = @Attendance_Date
                      AND EducationYearID = @EducationYearID
                      AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@ScheduleID, 0)
                )
                BEGIN
                    INSERT INTO Attendance_Record (
                        SchoolID, RegistrationID, EducationYearID, StudentID, ClassID,
                        StudentClassID, Attendance_ScheduleID, Attendance, AttendanceDate
                    )
                    VALUES (
                        @SchoolID, 0, @EducationYearID, @Loop_StudentID, @ClassID, @StudentClassID,
                        @ScheduleID, @AttendanceStatus, @Attendance_Date
                    )
                END
            END
            ELSE
            BEGIN
                SET @AttendanceStatus = 'Abs'

                IF NOT EXISTS (
                    SELECT 1
                    FROM Attendance_Record
                    WHERE SchoolID = @SchoolID
                      AND StudentID = @Loop_StudentID
                      AND AttendanceDate = @Attendance_Date
                      AND EducationYearID = @EducationYearID
                      AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@ScheduleID, 0)
                )
                BEGIN
                    INSERT INTO Attendance_Record (
                        SchoolID, RegistrationID, EducationYearID, StudentID, ClassID,
                        StudentClassID, Attendance_ScheduleID, Attendance, AttendanceDate
                    )
                    VALUES (
                        @SchoolID, 0, @EducationYearID, @Loop_StudentID, @ClassID, @StudentClassID,
                        @ScheduleID, @AttendanceStatus, @Attendance_Date
                    )
                END
            END

            DELETE #Temp_Attendance_Assign WHERE Schedule_AssignStuID = @Schedule_AssignStuID
        END

        DROP TABLE #Temp_Attendance_Assign
    END

    -- Insert or update attendance for the punching student
    IF NOT EXISTS (
        SELECT 1
        FROM Attendance_Record
        WHERE SchoolID = @SchoolID
          AND StudentID = @StudentID
          AND AttendanceDate = @Attendance_Date
          AND EducationYearID = @EducationYearID
          AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@ScheduleID, 0)
    )
    BEGIN
        IF (@StartTime >= @EntryTime)
            SET @AttendanceStatus = 'Pre'

        IF ((@StartTime < @EntryTime) AND (@EntryTime <= @LateEntryTime))
            SET @AttendanceStatus = 'Late'

        IF (@EntryTime < @EndTime)
        BEGIN
            SELECT
                @StudentClassID = StudentClassID,
                @ClassID = ClassID
            FROM StudentsClass
            WHERE SchoolID = @SchoolID
              AND EducationYearID = @EducationYearID
              AND StudentID = @StudentID

            INSERT INTO Attendance_Record (
                SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID,
                Attendance_ScheduleID, Attendance, AttendanceDate, EntryTime
            )
            VALUES (
                @SchoolID, 0, @EducationYearID, @StudentID, @ClassID, @StudentClassID,
                @ScheduleID, @AttendanceStatus, @Attendance_Date, @EntryTime
            )
        END
    END
    ELSE
    BEGIN
        IF ((@LateEntryTime < @EntryTime) AND (@EntryTime < @EndTime))
        BEGIN
            SET @AttendanceStatus = 'Late Abs'

            UPDATE Attendance_Record
            SET EntryTime = @EntryTime,
                Attendance = @AttendanceStatus
            WHERE SchoolID = @SchoolID
              AND StudentID = @StudentID
              AND AttendanceDate = @Attendance_Date
              AND EducationYearID = @EducationYearID
              AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@ScheduleID, 0)
              AND Attendance = 'Abs'
        END

        IF (@EndTime <= @EntryTime)
        BEGIN
            UPDATE Attendance_Record
            SET ExitTime = @EntryTime,
                ExitStatus = N'Out',
                Is_OUT = 1
            WHERE SchoolID = @SchoolID
              AND StudentID = @StudentID
              AND AttendanceDate = @Attendance_Date
              AND EducationYearID = @EducationYearID
              AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@ScheduleID, 0)
        END
    END
END
GO
