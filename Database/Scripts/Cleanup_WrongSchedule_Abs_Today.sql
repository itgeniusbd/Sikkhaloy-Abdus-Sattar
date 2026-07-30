-- Remove Abs records for students not assigned to that schedule (wrong multi-schedule marks).
-- Run on Edu DB after fixing AttendanceDevice User_Schedule seeding bug.

DECLARE @SchoolID int = 1012; -- change as needed
DECLARE @Today date = CONVERT(date, GETDATE());

DELETE ar
FROM Attendance_Record ar
WHERE ar.SchoolID = @SchoolID
  AND ar.AttendanceDate = @Today
  AND ar.Attendance = N'Abs'
  AND NOT EXISTS (
      SELECT 1
      FROM Attendance_Schedule_AssignStudent ass
      INNER JOIN Student s ON ass.StudentID = s.StudentID
      WHERE ass.SchoolID = @SchoolID
        AND ass.ScheduleID = ar.Attendance_ScheduleID
        AND ass.StudentID = ar.StudentID
        AND s.Status = N'Active'
  );

SELECT @@ROWCOUNT AS DeletedWrongAbsRows;
