-- Diagnose attendance SMS pipeline (run on Edu DB).
-- After Attendance_MultiSchedule_Trigger_Fix.sql:
--   Tr_Attendance_Record_SMS must NOT insert Attendance_SMS (API queues SMS).
USE [Edu];
GO

DECLARE @SchoolID int = 1012;
DECLARE @Today date = CONVERT(date, GETDATE());

PRINT '=== 1. Triggers on Attendance_Record ===';
SELECT
    t.name AS TriggerName,
    CASE t.is_instead_of_trigger WHEN 1 THEN 'INSTEAD OF' ELSE 'AFTER' END AS TriggerTiming,
    CASE
        WHEN OBJECTPROPERTY(t.object_id, 'ExecIsInsertTrigger') = 1 THEN 'INSERT'
        WHEN OBJECTPROPERTY(t.object_id, 'ExecIsUpdateTrigger') = 1 THEN 'UPDATE'
        WHEN OBJECTPROPERTY(t.object_id, 'ExecIsDeleteTrigger') = 1 THEN 'DELETE'
        ELSE 'OTHER'
    END AS TriggerEvent,
    CASE WHEN m.definition LIKE '%INSERT INTO Attendance_SMS%' THEN 'YES' ELSE 'NO' END AS InsertsAttendanceSms
FROM sys.triggers t
INNER JOIN sys.sql_modules m ON t.object_id = m.object_id
WHERE t.parent_id = OBJECT_ID(N'dbo.Attendance_Record')
ORDER BY t.name;
GO

DECLARE @SchoolID int = 1012;
DECLARE @Today date = CONVERT(date, GETDATE());

PRINT '=== 2. Attendance SMS device settings ===';
SELECT
    SchoolID,
    Is_All_SMS_On,
    Is_Student_All_SMS_Active,
    Is_Student_Entry_SMS_ON,
    Is_Student_Exit_SMS_ON,
    Is_Student_Abs_SMS_ON,
    Is_Student_Late_SMS_ON,
    SMS_TimeOut_Minute
FROM dbo.Attendance_Device_Setting
WHERE SchoolID = @SchoolID AND IsActive = 1;
GO

DECLARE @SchoolID int = 1012;
DECLARE @Today date = CONVERT(date, GETDATE());

PRINT '=== 3. Today attendance records (device sync) ===';
SELECT
    ar.AttendanceRecordID,
    s.DeviceID,
    s.StudentsName,
    sch.ScheduleName,
    ar.Attendance,
    ar.EntryTime,
    ar.AttendanceDate
FROM dbo.Attendance_Record ar
INNER JOIN dbo.Student s ON s.StudentID = ar.StudentID
LEFT JOIN dbo.Attendance_Schedule sch
    ON sch.ScheduleID = ar.Attendance_ScheduleID AND sch.SchoolID = ar.SchoolID
WHERE ar.SchoolID = @SchoolID
  AND ar.AttendanceDate = @Today
ORDER BY ar.EntryTime, s.StudentsName;
GO

DECLARE @SchoolID int = 1012;
DECLARE @Today date = CONVERT(date, GETDATE());

PRINT '=== 4. Attendance_SMS queue (today) ===';
SELECT
    Attendance_SMSID,
    SchoolID,
    StudentID,
    AttendanceStatus,
    LEFT(SMS_Text, 120) AS SMS_Text,
    MobileNo,
    AttendanceDate,
    ScheduleTime,
    CreateTime,
    Is_Send
FROM dbo.Attendance_SMS
WHERE SchoolID = @SchoolID
  AND AttendanceDate = @Today
ORDER BY Attendance_SMSID DESC;
GO

DECLARE @SchoolID int = 1012;
DECLARE @Today date = CONVERT(date, GETDATE());

PRINT '=== 5. Punched today but NO SMS queued (check Pre flag + phone) ===';
SELECT
    s.DeviceID,
    s.StudentsName,
    s.SMSPhoneNo,
    sch.ScheduleName,
    ar.Attendance,
    ar.EntryTime,
    ISNULL(v.Entry_Confirmation, 0) AS Entry_Confirmation,
    ISNULL(v.Is_Abs_SMS, 0) AS Is_Abs_SMS,
    ISNULL(v.Is_Late_SMS, 0) AS Is_Late_SMS
FROM dbo.Attendance_Record ar
INNER JOIN dbo.Student s ON s.StudentID = ar.StudentID
LEFT JOIN dbo.Attendance_Schedule sch
    ON sch.ScheduleID = ar.Attendance_ScheduleID AND sch.SchoolID = ar.SchoolID
LEFT JOIN dbo.VW_Attendance_Stu_Setting v
    ON v.StudentID = ar.StudentID
   AND v.ScheduleID = ISNULL(ar.Attendance_ScheduleID, 0)
   AND v.SchoolID = ar.SchoolID
WHERE ar.SchoolID = @SchoolID
  AND ar.AttendanceDate = @Today
  AND ar.IsFromDevice = 1
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Attendance_SMS sms
      WHERE sms.SchoolID = ar.SchoolID
        AND sms.StudentID = ar.StudentID
        AND sms.AttendanceDate = ar.AttendanceDate
        AND sms.AttendanceStatus = ar.Attendance
  )
ORDER BY s.StudentsName;
GO

DECLARE @SchoolID int = 1012;

PRINT '=== 6. All assign rows: Entry SMS off OR no phone (informational) ===';
SELECT
    s.DeviceID,
    s.StudentsName,
    s.SMSPhoneNo,
    ass.Entry_Confirmation,
    ass.Exit_Confirmation,
    ass.Is_Abs_SMS,
    ass.Is_Late_SMS,
    sch.ScheduleName
FROM dbo.Attendance_Schedule_AssignStudent ass
INNER JOIN dbo.Student s ON s.StudentID = ass.StudentID
INNER JOIN dbo.Attendance_Schedule sch ON sch.ScheduleID = ass.ScheduleID
WHERE ass.SchoolID = @SchoolID
  AND s.Status = N'Active'
  AND (ISNULL(s.SMSPhoneNo, N'') = N'' OR ass.Entry_Confirmation = 0)
ORDER BY s.StudentsName, sch.ScheduleName;
GO

PRINT '=== 7. Legacy UPDATE triggers that still insert Attendance_SMS ===';
DECLARE @dropSql nvarchar(max) = N'';

SELECT @dropSql = @dropSql
    + N'DROP TRIGGER ' + QUOTENAME(OBJECT_SCHEMA_NAME(t.parent_id)) + N'.' + QUOTENAME(t.name) + N';'
    + CHAR(13) + CHAR(10)
FROM sys.triggers t
INNER JOIN sys.sql_modules m ON t.object_id = m.object_id
WHERE t.parent_id = OBJECT_ID(N'dbo.Attendance_Record')
  AND t.name <> N'Tr_Attendance_Record_SMS'
  AND m.definition LIKE N'%INSERT INTO Attendance_SMS%';

IF LEN(@dropSql) > 0
BEGIN
    PRINT 'Found legacy triggers (run Attendance_MultiSchedule_Trigger_Fix.sql):';
    PRINT @dropSql;
END
ELSE
    PRINT 'OK: No legacy Attendance_Record UPDATE SMS triggers found.';
GO

PRINT '=== 8. Triggers on Attendance_SMS (queue table) ===';
SELECT
    t.name AS TriggerName,
    CASE t.is_instead_of_trigger WHEN 1 THEN 'INSTEAD OF' ELSE 'AFTER' END AS TriggerTiming,
    CASE WHEN m.definition LIKE '%DELETE%Attendance_SMS%' THEN 'YES' ELSE 'NO' END AS DeletesFromQueue
FROM sys.triggers t
INNER JOIN sys.sql_modules m ON t.object_id = m.object_id
WHERE t.parent_id = OBJECT_ID(N'dbo.Attendance_SMS')
ORDER BY t.name;
GO
