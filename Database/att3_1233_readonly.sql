/* =====================================================================================
   att3_1233_readonly.sql  -  READ-ONLY diagnostics (SELECT only, no writes)
   School 1233, dates 2026-09-28 (missing / "Not Current Data" punches) and 2026-09-29 (today)

   A  28-Sep: which device people have a row, which have NO row (lost punches), and which rows
      look incomplete (entry but no exit, or only an Abs row) - compare with the 12:08-13:05 window.
   B  Today: manual web rows (RegistrationID <> 0, no EntryTime) per schedule, and the display
      "In" count old formula vs new formula (new = counts manual Present rows too).
   C  Today: teacher rows. RegistrationID <> 0 on a row that has device times = the manual
      Employee_Attendance page saved over the device row (blank time box wiped EntryTime; the
      later exit punch put 8:58 / 10:27 back but RegistrationID stays set).
   D  Duplicate (person, date, schedule) rows.
   E  Attendance SMS queued for a past date (late SMS risk).
   Nothing here modifies data. Run with a read-only login if possible.
   ===================================================================================== */
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;   -- do not block the live app

DECLARE @SchoolID int  = 1233;
DECLARE @Prev     date = '20260928';
DECLARE @Today    date = '20260929';
DECLARE @WinFrom  time = '12:00', @WinTo time = '13:10';   -- the afternoon punches seen as "Not Current Data"

/* ---------- A1  28-Sep student rows by schedule / status / source ---------- */
SELECT 'A1_prev_student_mix' AS section, ar.Attendance_ScheduleID, ar.Attendance,
       CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 'real' ELSE 'NULL/00:00' END AS EntryKind,
       ar.Is_OUT, CASE WHEN ISNULL(ar.RegistrationID, 0) = 0 THEN 'device/API' ELSE 'manual' END AS Source,
       COUNT(*) AS Rows_, MIN(ar.EntryTime) AS MinEntry, MAX(ar.EntryTime) AS MaxEntry, MAX(ar.ExitTime) AS MaxExit,
       MIN(ar.InsertDate) AS FirstInsert, MAX(ar.InsertDate) AS LastInsert
FROM Attendance_Record ar
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Prev
GROUP BY ar.Attendance_ScheduleID, ar.Attendance,
         CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 'real' ELSE 'NULL/00:00' END,
         ar.Is_OUT, CASE WHEN ISNULL(ar.RegistrationID, 0) = 0 THEN 'device/API' ELSE 'manual' END
ORDER BY 2, 3;

/* A2  28-Sep rows touching the 12:00-13:10 window (should exist for the afternoon punches) */
SELECT 'A2_prev_student_window' AS section, ar.AttendanceRecordID, ar.Attendance_ScheduleID, s.ID AS StudentCode, s.StudentsName,
       s.DeviceID, ar.Attendance, ar.EntryTime, ar.ExitTime, ar.Is_OUT, ar.RegistrationID, ar.InsertDate
FROM Attendance_Record ar JOIN Student s ON s.StudentID = ar.StudentID
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Prev
  AND ((ar.EntryTime BETWEEN @WinFrom AND @WinTo) OR (ar.ExitTime BETWEEN @WinFrom AND @WinTo))
ORDER BY ISNULL(ar.ExitTime, ar.EntryTime);
SELECT 'A2_prev_employee_window' AS section, ear.Employee_Attendance_RecordID, ear.Attendance_ScheduleID, e.ID AS EmployeeCode,
       e.DeviceID, ear.AttendanceStatus, ear.EntryTime, ear.ExitTime, ear.Is_OUT, ear.RegistrationID, ear.CreatedDate
FROM Employee_Attendance_Record ear JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID
WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = @Prev
  AND ((ear.EntryTime BETWEEN @WinFrom AND @WinTo) OR (ear.ExitTime BETWEEN @WinFrom AND @WinTo))
ORDER BY ISNULL(ear.ExitTime, ear.EntryTime);

/* A3  Active students with a DeviceID who are assigned to a schedule that was ON on 28-Sep but have
       NO row or only an Abs / no-time row that day -> candidates for the lost 28-Sep punches */
SELECT 'A3_prev_student_missing_or_abs' AS section, ass.ScheduleID, sch.ScheduleName, s.StudentID, s.ID AS StudentCode,
       s.StudentsName, s.DeviceID, ar.AttendanceRecordID, ar.Attendance, ar.EntryTime, ar.ExitTime, ar.RegistrationID
FROM Attendance_Schedule_AssignStudent ass
JOIN Attendance_Schedule sch ON sch.ScheduleID = ass.ScheduleID
JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = ass.ScheduleID AND sd.SchoolID = ass.SchoolID
     AND sd.Day = DATENAME(dw, @Prev) AND ISNULL(sd.Is_OnDay, 1) = 1
JOIN Student s ON s.StudentID = ass.StudentID AND s.Status = N'Active' AND ISNULL(s.DeviceID, 0) <> 0
LEFT JOIN Attendance_Record ar ON ar.SchoolID = @SchoolID AND ar.StudentID = s.StudentID
     AND ar.AttendanceDate = @Prev AND ar.Attendance_ScheduleID = ass.ScheduleID
WHERE ass.SchoolID = @SchoolID
  AND (ar.AttendanceRecordID IS NULL OR ar.Attendance IN (N'Abs') OR ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time))
ORDER BY ass.ScheduleID, s.StudentsName;

/* A4  same for employees */
SELECT 'A4_prev_employee_missing_or_abs' AS section, eas.ScheduleID, sch.ScheduleName, e.EmployeeID, e.ID AS EmployeeCode,
       e.DeviceID, ear.Employee_Attendance_RecordID, ear.AttendanceStatus, ear.EntryTime, ear.ExitTime, ear.RegistrationID
FROM Employee_Attendance_Schedule_Assign eas
JOIN Attendance_Schedule sch ON sch.ScheduleID = eas.ScheduleID
JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = eas.ScheduleID AND sd.SchoolID = eas.SchoolID
     AND sd.Day = DATENAME(dw, @Prev) AND ISNULL(sd.Is_OnDay, 1) = 1
JOIN Employee_Info e ON e.EmployeeID = eas.EmployeeID AND e.Job_Status = N'Active' AND ISNULL(e.DeviceID, 0) <> 0
LEFT JOIN Employee_Attendance_Record ear ON ear.SchoolID = @SchoolID AND ear.EmployeeID = e.EmployeeID
     AND ear.AttendanceDate = @Prev AND ear.Attendance_ScheduleID = eas.ScheduleID
WHERE eas.SchoolID = @SchoolID
  AND (ear.Employee_Attendance_RecordID IS NULL OR ear.AttendanceStatus = N'Abs' OR ear.EntryTime IS NULL OR ear.EntryTime = CAST('00:00:00' AS time))
ORDER BY eas.ScheduleID, e.ID;

/* A5  rows with an entry but no exit on 28-Sep (afternoon exit punch possibly lost) */
SELECT 'A5_prev_no_exit' AS section, 'Student' AS Kind, ar.Attendance_ScheduleID, COUNT(*) AS Rows_
FROM Attendance_Record ar
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Prev AND ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)
  AND ISNULL(ar.Is_OUT, 0) = 0
GROUP BY ar.Attendance_ScheduleID
UNION ALL
SELECT 'A5_prev_no_exit', 'Employee', ear.Attendance_ScheduleID, COUNT(*)
FROM Employee_Attendance_Record ear
WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = @Prev AND ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)
  AND ISNULL(ear.Is_OUT, 0) = 0
GROUP BY ear.Attendance_ScheduleID;

/* ---------- B1  Today: display "In" old vs new, per schedule (students) ---------- */
SELECT 'B1_today_student_in' AS section, ar.Attendance_ScheduleID,
       SUM(CASE WHEN ar.Attendance = N'Pre' THEN 1 ELSE 0 END) AS Pre,
       SUM(CASE WHEN ar.Attendance = N'Late' THEN 1 ELSE 0 END) AS Late,
       SUM(CASE WHEN ar.Attendance = N'Late Abs' THEN 1 ELSE 0 END) AS Late_Abs,
       SUM(CASE WHEN ar.Attendance = N'Abs' THEN 1 ELSE 0 END) AS Abs_rows,
       SUM(CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) AND ar.Is_OUT = 0
                 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs') THEN 1 ELSE 0 END) AS In_old,
       SUM(CASE WHEN ISNULL(ar.Is_OUT, 0) = 0 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs')
                 AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0)
                THEN 1 ELSE 0 END) AS In_new,
       SUM(CASE WHEN ISNULL(ar.RegistrationID, 0) <> 0 AND (ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time))
                 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs') THEN 1 ELSE 0 END) AS Manual_present_no_time,
       SUM(CASE WHEN ISNULL(ar.RegistrationID, 0) = 0 AND (ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time))
                 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs') THEN 1 ELSE 0 END) AS Device_present_no_time_CHECK
FROM Attendance_Record ar
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Today
GROUP BY ar.Attendance_ScheduleID ORDER BY 2;

/* B2  the manual present rows themselves (who marked them, when) */
SELECT 'B2_today_manual_present' AS section, ar.AttendanceRecordID, ar.Attendance_ScheduleID, s.ID AS StudentCode, s.StudentsName,
       s.DeviceID, ar.Attendance, ar.EntryTime, ar.IsFromDevice, ar.RegistrationID, r.UserName AS MarkedBy, ar.InsertDate
FROM Attendance_Record ar JOIN Student s ON s.StudentID = ar.StudentID
LEFT JOIN Registration r ON r.RegistrationID = ar.RegistrationID
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Today AND ISNULL(ar.RegistrationID, 0) <> 0
  AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs')
ORDER BY ar.Attendance_ScheduleID, s.StudentsName;

/* ---------- C1  Today: all teacher / employee rows with source ---------- */
SELECT 'C1_today_employee_rows' AS section, ear.Employee_Attendance_RecordID, ear.Attendance_ScheduleID, e.ID AS EmployeeCode,
       LTRIM(ISNULL(v.FirstName, '') + ' ' + ISNULL(v.LastName, '')) AS EmployeeName, e.DeviceID,
       ear.AttendanceStatus, ear.EntryTime, ear.ExitTime, ear.Is_OUT, ear.IsFromDevice, ear.RegistrationID,
       r.UserName AS LastManualSaveBy, ear.CreatedDate,
       CASE WHEN ISNULL(ear.RegistrationID, 0) <> 0 AND ear.IsFromDevice = 1 THEN 'device row saved on manual page (C: overwrite)'
            WHEN ISNULL(ear.RegistrationID, 0) <> 0 THEN 'manual row'
            ELSE 'device/API' END AS Source,
       CASE WHEN ISNULL(ear.Is_OUT, 0) = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs')
                 AND ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time) THEN 1 ELSE 0 END AS In_old,
       CASE WHEN ISNULL(ear.Is_OUT, 0) = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs')
                 AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END AS In_new
FROM Employee_Attendance_Record ear
JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID
LEFT JOIN VW_Emp_Info v ON v.EmployeeID = e.EmployeeID
LEFT JOIN Registration r ON r.RegistrationID = ear.RegistrationID
WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = @Today
ORDER BY ear.Attendance_ScheduleID, e.ID;

/* C2  the teacher from the report: entry 08:58 / exit 10:27 (adjust times if needed) */
SELECT 'C2_teacher_0858_1027' AS section, ear.*, r.UserName AS LastManualSaveBy
FROM Employee_Attendance_Record ear LEFT JOIN Registration r ON r.RegistrationID = ear.RegistrationID
WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = @Today
  AND (ear.EntryTime BETWEEN '08:55' AND '09:02' OR ear.ExitTime BETWEEN '10:25' AND '10:30');

/* ---------- D  duplicates (person, date, schedule) for both days ---------- */
SELECT 'D_dup_student' AS section, ar.AttendanceDate, ar.StudentID, ISNULL(ar.Attendance_ScheduleID, 0) AS ScheduleID, COUNT(*) AS Rows_
FROM Attendance_Record ar WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate IN (@Prev, @Today)
GROUP BY ar.AttendanceDate, ar.StudentID, ISNULL(ar.Attendance_ScheduleID, 0) HAVING COUNT(*) > 1;
SELECT 'D_dup_employee' AS section, ear.AttendanceDate, ear.EmployeeID, ISNULL(ear.Attendance_ScheduleID, 0) AS ScheduleID, COUNT(*) AS Rows_
FROM Employee_Attendance_Record ear WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate IN (@Prev, @Today)
GROUP BY ear.AttendanceDate, ear.EmployeeID, ISNULL(ear.Attendance_ScheduleID, 0) HAVING COUNT(*) > 1;

/* ---------- E  SMS rows whose AttendanceDate is before the day they were inserted ---------- */
SELECT 'E_late_sms' AS section, sm.Attendance_SMSID, sm.StudentID, sm.EmployeeID, sm.AttendanceDate, sm.InsertDate,
       sm.AttendanceStatus, sm.Is_Send, sm.SentTime, sm.MobileNo
FROM Attendance_SMS sm
WHERE sm.SchoolID = @SchoolID AND sm.InsertDate >= DATEADD(day, -3, @Today)
  AND CONVERT(date, sm.AttendanceDate) < CONVERT(date, sm.InsertDate)
ORDER BY sm.InsertDate DESC;
