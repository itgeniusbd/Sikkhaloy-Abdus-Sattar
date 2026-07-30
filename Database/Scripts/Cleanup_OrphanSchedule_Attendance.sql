/*
    SERVER (Education SQL Server)
    Delete attendance rows whose schedule no longer exists (deleted schedule orphan data).

    When a schedule is deleted from web (Absence Fee / Schedule page), only
    Attendance_Schedule + assign tables are removed — Attendance_Record and
    Employee_Attendance_Record rows with that ScheduleID remain and still show
    on DeviceDisplay / Attendance_Slider IN-OUT lists.

    BEFORE RUN:
    - Close AttendanceDevice on school PC(s).
    - Set @SchoolID (and optional filters below).
    - Run PREVIEW section first, then uncomment DELETE section.

    AFTER RUN:
    - Device: Settings → Atten. Display → download schedule again.
    - Optional: run local Cleanup_OrphanSchedule_Attendance.sql on PC.
*/

SET NOCOUNT ON;

DECLARE @SchoolID INT = 1012;          -- change
DECLARE @ScheduleID INT = NULL;        -- e.g. 2791 for one deleted schedule; NULL = all orphan schedules
DECLARE @TodayOnly BIT = 1;            -- 1 = today only, 0 = all dates
DECLARE @AttendanceDate DATE = CAST(GETDATE() AS DATE);

PRINT 'SchoolID=' + CAST(@SchoolID AS varchar(10))
    + ', ScheduleID=' + ISNULL(CAST(@ScheduleID AS varchar(10)), 'ALL-ORPHAN')
    + ', DateFilter=' + CASE WHEN @TodayOnly = 1 THEN CONVERT(varchar(10), @AttendanceDate, 120) ELSE 'ALL-DATES' END;

/* ===================== PREVIEW ===================== */
SELECT 'Student orphan rows' AS Info, COUNT(*) AS Cnt
FROM dbo.Attendance_Record ar
WHERE ar.SchoolID = @SchoolID
  AND ISNULL(ar.Attendance_ScheduleID, 0) <> 0
  AND (@ScheduleID IS NULL OR ar.Attendance_ScheduleID = @ScheduleID)
  AND (@TodayOnly = 0 OR ar.AttendanceDate = @AttendanceDate)
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Attendance_Schedule sch
      WHERE sch.SchoolID = ar.SchoolID
        AND sch.ScheduleID = ar.Attendance_ScheduleID
  );

SELECT TOP 50
    ar.Attendance_RecordID,
    ar.StudentID,
    ar.Attendance_ScheduleID AS ScheduleID,
    ar.Attendance,
    ar.AttendanceDate,
    ar.EntryTime,
    ar.ExitTime
FROM dbo.Attendance_Record ar
WHERE ar.SchoolID = @SchoolID
  AND ISNULL(ar.Attendance_ScheduleID, 0) <> 0
  AND (@ScheduleID IS NULL OR ar.Attendance_ScheduleID = @ScheduleID)
  AND (@TodayOnly = 0 OR ar.AttendanceDate = @AttendanceDate)
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Attendance_Schedule sch
      WHERE sch.SchoolID = ar.SchoolID
        AND sch.ScheduleID = ar.Attendance_ScheduleID
  )
ORDER BY ar.AttendanceDate DESC, ar.Attendance_RecordID DESC;

SELECT 'Employee orphan rows' AS Info, COUNT(*) AS Cnt
FROM dbo.Employee_Attendance_Record ear
WHERE ear.SchoolID = @SchoolID
  AND ISNULL(ear.Attendance_ScheduleID, 0) <> 0
  AND (@ScheduleID IS NULL OR ear.Attendance_ScheduleID = @ScheduleID)
  AND (@TodayOnly = 0 OR ear.AttendanceDate = @AttendanceDate)
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Attendance_Schedule sch
      WHERE sch.SchoolID = ear.SchoolID
        AND sch.ScheduleID = ear.Attendance_ScheduleID
  );

SELECT TOP 50
    ear.Employee_Attendance_RecordID,
    ear.EmployeeID,
    ear.Attendance_ScheduleID AS ScheduleID,
    ear.AttendanceStatus,
    ear.AttendanceDate,
    ear.EntryTime,
    ear.ExitTime
FROM dbo.Employee_Attendance_Record ear
WHERE ear.SchoolID = @SchoolID
  AND ISNULL(ear.Attendance_ScheduleID, 0) <> 0
  AND (@ScheduleID IS NULL OR ear.Attendance_ScheduleID = @ScheduleID)
  AND (@TodayOnly = 0 OR ear.AttendanceDate = @AttendanceDate)
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Attendance_Schedule sch
      WHERE sch.SchoolID = ear.SchoolID
        AND sch.ScheduleID = ear.Attendance_ScheduleID
  )
ORDER BY ear.AttendanceDate DESC, ear.Employee_Attendance_RecordID DESC;

/* ===================== DELETE (uncomment after preview) ===================== */
/*
BEGIN TRANSACTION;

    DELETE sms
    FROM dbo.Attendance_SMS sms
    WHERE sms.SchoolID = @SchoolID
      AND (@TodayOnly = 0 OR sms.AttendanceDate = @AttendanceDate)
      AND (
          EXISTS (
              SELECT 1
              FROM dbo.Attendance_Record ar
              WHERE ar.SchoolID = sms.SchoolID
                AND ar.StudentID = sms.StudentID
                AND ar.AttendanceDate = sms.AttendanceDate
                AND ISNULL(ar.Attendance_ScheduleID, 0) <> 0
                AND (@ScheduleID IS NULL OR ar.Attendance_ScheduleID = @ScheduleID)
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.Attendance_Schedule sch
                    WHERE sch.SchoolID = ar.SchoolID AND sch.ScheduleID = ar.Attendance_ScheduleID
                )
          )
          OR EXISTS (
              SELECT 1
              FROM dbo.Employee_Attendance_Record ear
              WHERE ear.SchoolID = sms.SchoolID
                AND ear.EmployeeID = sms.EmployeeID
                AND ear.AttendanceDate = sms.AttendanceDate
                AND ISNULL(ear.Attendance_ScheduleID, 0) <> 0
                AND (@ScheduleID IS NULL OR ear.Attendance_ScheduleID = @ScheduleID)
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.Attendance_Schedule sch
                    WHERE sch.SchoolID = ear.SchoolID AND sch.ScheduleID = ear.Attendance_ScheduleID
                )
          )
      );

    PRINT 'Deleted Attendance_SMS: ' + CAST(@@ROWCOUNT AS varchar(20));

    DELETE ar
    FROM dbo.Attendance_Record ar
    WHERE ar.SchoolID = @SchoolID
      AND ISNULL(ar.Attendance_ScheduleID, 0) <> 0
      AND (@ScheduleID IS NULL OR ar.Attendance_ScheduleID = @ScheduleID)
      AND (@TodayOnly = 0 OR ar.AttendanceDate = @AttendanceDate)
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.Attendance_Schedule sch
          WHERE sch.SchoolID = ar.SchoolID
            AND sch.ScheduleID = ar.Attendance_ScheduleID
      );

    PRINT 'Deleted Attendance_Record: ' + CAST(@@ROWCOUNT AS varchar(20));

    DELETE ear
    FROM dbo.Employee_Attendance_Record ear
    WHERE ear.SchoolID = @SchoolID
      AND ISNULL(ear.Attendance_ScheduleID, 0) <> 0
      AND (@ScheduleID IS NULL OR ear.Attendance_ScheduleID = @ScheduleID)
      AND (@TodayOnly = 0 OR ear.AttendanceDate = @AttendanceDate)
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.Attendance_Schedule sch
          WHERE sch.SchoolID = ear.SchoolID
            AND sch.ScheduleID = ear.Attendance_ScheduleID
      );

    PRINT 'Deleted Employee_Attendance_Record: ' + CAST(@@ROWCOUNT AS varchar(20));

COMMIT TRANSACTION;
PRINT 'Done.';
*/
