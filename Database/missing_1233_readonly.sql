/* =====================================================================================
   missing_1233_readonly.sql  -  READ-ONLY diagnostics (SELECT only, no writes)
   School 1233 (Luxmipur Khatune Zannat Mohila Madrasha), attendance date 2026-09-28

   Why the device display (Online_Display/DeviceDisplay.aspx, which the AttendanceDevice
   app shows inside its WebView) does not add up:
     Total          = assigned + Active + has StudentsClass in the resolved EducationYear
     Pre (display)  = Attendance='Pre' AND EntryTime is a real time (not NULL, not 00:00)
     Late / L.Abs / Abs = status only;  Abs also adds "virtual absent" = assigned, no row
                      for that schedule today, not on Attendance_Leave, now > LateEntryTime
     => a person with a row that is 'Pre' + NULL/00:00 EntryTime, 'Leave', 'Bunk' or any other
        status (or no row but on leave) is in Total but in NO chip.
   Web pages that count "Pre" (Attendance_Records.aspx with the Pre filter, the Summary tab,
   Display_User/Device_Attendance_Report.aspx) do NOT check EntryTime, so 76 vs 68 = the
   8 'Pre' rows without a real EntryTime.

   Row source hints:  device/API writes (Attendance_API, ZKTeco PushAPI, Attendance_Students_API proc)
                      -> RegistrationID = 0 (or NULL). V2 / Hybrid manual insert OR manual UPDATE of a
                      device row -> RegistrationID = login user. IsFromDevice is NOT reliable: a device
                      Abs row later changed to Pre on the manual page keeps IsFromDevice = 1.
                      Manual pages never set EntryTime for students.
   Run on the live DB with a read-only login if possible. Nothing here modifies data.
   ===================================================================================== */
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;   -- avoid blocking the live app; read-only anyway

DECLARE @SchoolID int  = 1233;
DECLARE @Date     date = '20260928';
DECLARE @DayName  nvarchar(30) = DATENAME(dw, @Date);           -- same function the display uses
DECLARE @AsOf     time = CASE WHEN @Date = CONVERT(date, GETDATE()) THEN CAST(GETDATE() AS time) ELSE '23:59:59' END;
DECLARE @EY int;

/* EducationYear resolved exactly like DeviceDisplay.aspx.cs ResolveEducationYearId (lines 74-137) */
SELECT TOP 1 @EY = EducationYearID FROM Education_Year
 WHERE SchoolID = @SchoolID AND (Status IN (N'True', N'true', N'1') OR IsActive = 1)
   AND (StartDate IS NULL OR StartDate <= @Date) AND (EndDate IS NULL OR EndDate >= @Date)
 ORDER BY SN DESC, EducationYearID DESC;
IF @EY IS NULL
  SELECT TOP 1 @EY = EducationYearID FROM Education_Year
   WHERE SchoolID = @SchoolID AND (Status IN (N'True', N'true', N'1') OR IsActive = 1)
   ORDER BY SN DESC, EducationYearID DESC;
IF @EY IS NULL
  SELECT TOP 1 @EY = EducationYearID FROM Education_Year WHERE SchoolID = @SchoolID
   ORDER BY CASE WHEN Status IN (N'True', N'true', N'1') THEN 0 ELSE 1 END,
            CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, SN DESC, EducationYearID DESC;

SELECT '00_params' AS section, @SchoolID AS SchoolID, @Date AS AttendanceDate, @DayName AS DayName,
       @AsOf AS AsOfTime, @EY AS ResolvedEducationYearID,
       (SELECT EducationYear FROM Education_Year WHERE EducationYearID = @EY) AS EducationYear,
       (SELECT COUNT(*) FROM Employee_Holiday h WHERE h.SchoolID = @SchoolID AND h.HolidayDate = @Date) AS HolidayRowsThatDay,
       (SELECT TOP 1 HolidayName FROM Employee_Holiday h WHERE h.SchoolID = @SchoolID AND h.HolidayDate = @Date) AS HolidayName;

/* ---------- 01  Schedules for that weekday ---------- */
SELECT '01_schedules' AS section, sch.ScheduleID, sch.ScheduleName, sd.Day, sd.StartTime, sd.LateEntryTime, sd.EndTime,
       sd.Is_OnDay,
       (SELECT COUNT(*) FROM Attendance_Schedule_AssignStudent a WHERE a.SchoolID = @SchoolID AND a.ScheduleID = sch.ScheduleID) AS StudentAssignRows,
       (SELECT COUNT(*) FROM Employee_Attendance_Schedule_Assign e WHERE e.SchoolID = @SchoolID AND e.ScheduleID = sch.ScheduleID) AS EmployeeAssignRows
FROM Attendance_Schedule sch
JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = sch.ScheduleID AND sd.SchoolID = sch.SchoolID
WHERE sch.SchoolID = @SchoolID AND sd.Day = @DayName
ORDER BY sd.StartTime;

/* ---------- 02  STUDENTS: one row per assigned student per schedule with the display bucket ---------- */
IF OBJECT_ID('tempdb..#stu') IS NOT NULL DROP TABLE #stu;
SELECT
    ass.ScheduleID, sch.ScheduleName, s.StudentID, s.ID AS StudentCode, s.StudentsName, s.Status AS StudentStatus,
    s.DeviceID, s.RFID,
    (SELECT COUNT(*) FROM StudentsClass sc WHERE sc.StudentID = s.StudentID AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @EY) AS StudentsClassRowsInEY,
    (SELECT TOP 1 cc.Class FROM StudentsClass sc JOIN CreateClass cc ON cc.ClassID = sc.ClassID
       WHERE sc.StudentID = s.StudentID AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @EY) AS Class,
    (SELECT TOP 1 sc.Class_Status FROM StudentsClass sc WHERE sc.StudentID = s.StudentID AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @EY) AS Class_Status,
    CASE WHEN EXISTS (SELECT 1 FROM Attendance_Leave al WHERE al.SchoolID = @SchoolID AND al.StudentID = s.StudentID
                      AND @Date BETWEEN al.StartDate AND al.EndDate) THEN 1 ELSE 0 END AS OnLeave,
    (SELECT COUNT(*) FROM Attendance_Record x WHERE x.SchoolID = @SchoolID AND x.StudentID = s.StudentID AND x.AttendanceDate = @Date
       AND x.Attendance_ScheduleID = ass.ScheduleID) AS RowsThisSchedule,
    (SELECT COUNT(*) FROM Attendance_Record x WHERE x.SchoolID = @SchoolID AND x.StudentID = s.StudentID AND x.AttendanceDate = @Date
       AND ISNULL(x.Attendance_ScheduleID, 0) <> ass.ScheduleID) AS RowsOtherOrNullSchedule,
    ar.AttendanceRecordID, ar.Attendance, ar.EntryTime, ar.ExitTime, ar.ExitStatus, ar.Is_OUT, ar.IsFromDevice,
    ar.RegistrationID AS RowRegistrationID, r.UserName AS RowInsertedByUser, r.Category AS RowInsertedByCategory,
    ar.InsertDate, ar.Reason, ar.EducationYearID AS RowEducationYearID, ar.StudentClassID AS RowStudentClassID,
    CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 1 ELSE 0 END AS HasRealEntryTime,
    CASE
        WHEN ar.AttendanceRecordID IS NULL AND EXISTS (SELECT 1 FROM Attendance_Leave al WHERE al.SchoolID = @SchoolID AND al.StudentID = s.StudentID AND @Date BETWEEN al.StartDate AND al.EndDate)
             THEN 'UNCOUNTED: no row + on leave (no Leave chip)'
        WHEN ar.AttendanceRecordID IS NULL AND @AsOf > sd.LateEntryTime THEN 'Abs (virtual, no row)'
        WHEN ar.AttendanceRecordID IS NULL THEN 'UNCOUNTED: no row, before LateEntryTime'
        WHEN ar.Attendance = N'Pre' AND ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 'Pre'
        WHEN ar.Attendance = N'Pre' THEN 'UNCOUNTED: Pre with NULL/00:00 EntryTime'
        WHEN ar.Attendance IN (N'Late', N'Late Abs', N'Abs') THEN ar.Attendance
        ELSE 'UNCOUNTED: status=' + ISNULL(ar.Attendance, 'NULL')
    END AS DisplayBucket,
    CASE WHEN ar.AttendanceRecordID IS NULL THEN NULL
         WHEN ISNULL(ar.RegistrationID, 0) = 0 THEN 'device app / API (RegistrationID 0)'
         WHEN ar.IsFromDevice = 1 THEN 'device row later edited on a manual page'
         ELSE 'manual insert (V2/Hybrid attendance page)' END AS LikelySource
INTO #stu
FROM Attendance_Schedule_AssignStudent ass
JOIN Attendance_Schedule sch ON sch.ScheduleID = ass.ScheduleID
JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = ass.ScheduleID AND sd.SchoolID = ass.SchoolID AND sd.Day = @DayName
JOIN Student s ON s.StudentID = ass.StudentID AND s.Status = N'Active'
LEFT JOIN Attendance_Record ar ON ar.SchoolID = @SchoolID AND ar.StudentID = ass.StudentID
      AND ar.AttendanceDate = @Date AND ar.Attendance_ScheduleID = ass.ScheduleID
LEFT JOIN Registration r ON r.RegistrationID = ar.RegistrationID
WHERE ass.SchoolID = @SchoolID
  AND EXISTS (SELECT 1 FROM StudentsClass sc WHERE sc.StudentID = s.StudentID AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @EY);

/* 02a  per-schedule reproduction of the display chips + web-style Pre count */
SELECT '02a_student_counts' AS section, ScheduleID, ScheduleName,
       COUNT(DISTINCT StudentID) AS Total_like_display,
       COUNT(DISTINCT CASE WHEN HasRealEntryTime = 1 AND Is_OUT = 0 AND Attendance IN (N'Pre', N'Late', N'Late Abs') THEN StudentID END) AS [In],
       COUNT(DISTINCT CASE WHEN ExitTime IS NOT NULL AND Is_OUT = 1 THEN StudentID END) AS [Out],
       COUNT(DISTINCT CASE WHEN DisplayBucket = 'Pre' THEN StudentID END) AS Pre_display,
       COUNT(DISTINCT CASE WHEN Attendance = N'Pre' THEN StudentID END) AS Pre_web_any_entrytime,
       COUNT(DISTINCT CASE WHEN Attendance = N'Late' THEN StudentID END) AS Late,
       COUNT(DISTINCT CASE WHEN Attendance = N'Late Abs' THEN StudentID END) AS Late_Abs,
       COUNT(DISTINCT CASE WHEN DisplayBucket IN ('Abs', 'Abs (virtual, no row)') THEN StudentID END) AS Abs_incl_virtual,
       COUNT(DISTINCT CASE WHEN DisplayBucket LIKE 'UNCOUNTED%' THEN StudentID END) AS Uncounted,
       COUNT(DISTINCT AttendanceRecordID) AS RowsWithRecord,
       COUNT(*) - COUNT(DISTINCT CAST(StudentID AS bigint) * 100000000 + ISNULL(AttendanceRecordID, 0)) AS ExtraRowsFromDuplicateAssignments
FROM #stu GROUP BY ScheduleID, ScheduleName ORDER BY ScheduleID;

/* 02b  THE ANSWER: students in Total but in no chip (expect 8 for the Class schedule) */
SELECT '02b_student_uncounted' AS section, * FROM #stu
WHERE DisplayBucket LIKE 'UNCOUNTED%'
ORDER BY ScheduleID, Class, StudentsName;

/* 02c  all 'Pre' rows for that date (web shows 76) with insert time / source; DisplayCountsAsPre = 0 are the missing ones */
SELECT '02c_student_pre_rows' AS section, ar.AttendanceRecordID, ar.Attendance_ScheduleID, s.ID AS StudentCode, s.StudentsName,
       ar.EntryTime, ar.ExitTime, ar.Is_OUT, ar.ExitStatus, ar.IsFromDevice, ar.RegistrationID, r.UserName AS InsertedByUser,
       r.Category AS InsertedByCategory, ar.InsertDate, ar.Reason, s.DeviceID, s.Status AS StudentStatus,
       CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 1 ELSE 0 END AS DisplayCountsAsPre,
       CASE WHEN EXISTS (SELECT 1 FROM Attendance_Schedule_AssignStudent a WHERE a.SchoolID = @SchoolID AND a.StudentID = ar.StudentID AND a.ScheduleID = ar.Attendance_ScheduleID) THEN 1 ELSE 0 END AS AssignedToThatSchedule
FROM Attendance_Record ar
JOIN Student s ON s.StudentID = ar.StudentID
LEFT JOIN Registration r ON r.RegistrationID = ar.RegistrationID
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Date AND ar.Attendance = N'Pre'
ORDER BY DisplayCountsAsPre, ar.InsertDate;

/* 02d  status distribution (all rows for the date, any schedule) */
SELECT '02d_student_status_mix' AS section, ISNULL(CAST(ar.Attendance_ScheduleID AS varchar(12)), 'NULL') AS ScheduleID, ar.Attendance,
       CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 'real' ELSE 'NULL/00:00' END AS EntryTimeKind,
       ar.Is_OUT, ar.IsFromDevice, CASE WHEN ar.RegistrationID = 0 THEN 'API(0)' ELSE 'user' END AS RegKind,
       COUNT(*) AS Rows_, MIN(ar.InsertDate) AS FirstInsert, MAX(ar.InsertDate) AS LastInsert
FROM Attendance_Record ar
WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Date
GROUP BY ar.Attendance_ScheduleID, ar.Attendance,
         CASE WHEN ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time) THEN 'real' ELSE 'NULL/00:00' END,
         ar.Is_OUT, ar.IsFromDevice, CASE WHEN ar.RegistrationID = 0 THEN 'API(0)' ELSE 'user' END
ORDER BY 2, 3, 4;

/* 02e  duplicates that can distort counts */
SELECT '02e_dup_rows_same_schedule' AS section, ar.StudentID, ISNULL(ar.Attendance_ScheduleID, 0) AS ScheduleID, COUNT(*) AS Rows_
FROM Attendance_Record ar WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = @Date
GROUP BY ar.StudentID, ISNULL(ar.Attendance_ScheduleID, 0) HAVING COUNT(*) > 1;
SELECT '02e_dup_assignment_rows' AS section, a.ScheduleID, a.StudentID, COUNT(*) AS AssignRows   -- duplicates inflate the display's In/Out/Pre/Late sums
FROM Attendance_Schedule_AssignStudent a WHERE a.SchoolID = @SchoolID
GROUP BY a.ScheduleID, a.StudentID HAVING COUNT(*) > 1;
SELECT '02e_dup_employee_assignment_rows' AS section, e.ScheduleID, e.EmployeeID, COUNT(*) AS AssignRows
FROM Employee_Attendance_Schedule_Assign e WHERE e.SchoolID = @SchoolID
GROUP BY e.ScheduleID, e.EmployeeID HAVING COUNT(*) > 1;
SELECT '02e_multi_StudentsClass_in_EY' AS section, sc.StudentID, COUNT(*) AS StudentsClassRows
FROM StudentsClass sc
WHERE sc.SchoolID = @SchoolID AND sc.EducationYearID = @EY
  AND EXISTS (SELECT 1 FROM Attendance_Schedule_AssignStudent a WHERE a.StudentID = sc.StudentID AND a.SchoolID = @SchoolID)
GROUP BY sc.StudentID HAVING COUNT(*) > 1;

/* 02f  device-id sanity for uncounted students (DeviceID missing / shared) */
SELECT '02f_deviceid_check' AS section, u.StudentID, u.StudentsName, u.DeviceID,
       (SELECT COUNT(*) FROM Student s2 WHERE s2.SchoolID = @SchoolID AND s2.DeviceID = u.DeviceID AND s2.Status = N'Active') AS ActiveStudentsWithSameDeviceID,
       (SELECT COUNT(*) FROM Employee_Info e2 WHERE e2.SchoolID = @SchoolID AND e2.DeviceID = u.DeviceID AND e2.Job_Status = N'Active') AS ActiveEmployeesWithSameDeviceID
FROM #stu u WHERE u.DisplayBucket LIKE 'UNCOUNTED%';

/* 02g  raw device punches on the server, if the legacy table exists and is used */
IF OBJECT_ID('dbo.Attendance_Record_Device') IS NOT NULL
    EXEC sp_executesql N'
    SELECT ''02g_raw_device_punches'' AS section, d.Category, d.Stu_Emp_ID, d.DeviceID, d.EntryDateTime, d.ServerUp_Status, d.Insert_Date
    FROM dbo.Attendance_Record_Device d
    WHERE d.SchoolID = @SchoolID AND d.EntryDate = @Date
      AND d.DeviceID IN (SELECT DeviceID FROM #stu WHERE DisplayBucket LIKE ''UNCOUNTED%'' AND DeviceID IS NOT NULL)
    ORDER BY d.EntryDateTime;', N'@SchoolID int, @Date date', @SchoolID, @Date;

/* ---------- 03  TEACHERS / EMPLOYEES ---------- */
IF OBJECT_ID('tempdb..#emp') IS NOT NULL DROP TABLE #emp;
SELECT
    eas.ScheduleID, sch.ScheduleName, e.EmployeeID, e.ID AS EmployeeCode,
    LTRIM(ISNULL(v.FirstName, '') + ' ' + ISNULL(v.LastName, '')) AS EmployeeName, v.Designation,
    e.Job_Status, e.DeviceID, e.RFID,
    CASE WHEN EXISTS (SELECT 1 FROM Employee_Leave el WHERE el.SchoolID = @SchoolID AND el.EmployeeID = e.EmployeeID
                      AND @Date BETWEEN el.LeaveStartDate AND el.LeaveEndDate) THEN 1 ELSE 0 END AS OnLeave,
    (SELECT TOP 1 el.ApproveStatus FROM Employee_Leave el WHERE el.SchoolID = @SchoolID AND el.EmployeeID = e.EmployeeID
       AND @Date BETWEEN el.LeaveStartDate AND el.LeaveEndDate) AS LeaveApproveStatus,
    (SELECT COUNT(*) FROM Employee_Attendance_Record x WHERE x.SchoolID = @SchoolID AND x.EmployeeID = e.EmployeeID AND x.AttendanceDate = @Date
       AND ISNULL(x.Attendance_ScheduleID, 0) <> eas.ScheduleID) AS RowsOtherOrNullSchedule,
    ear.Employee_Attendance_RecordID, ear.AttendanceStatus, ear.EntryTime, ear.ExitTime, ear.ExitStatus, ear.Is_OUT, ear.IsFromDevice,
    ear.RegistrationID AS RowRegistrationID, r.UserName AS RowInsertedByUser, ear.CreatedDate,
    CASE
        WHEN ear.Employee_Attendance_RecordID IS NULL AND EXISTS (SELECT 1 FROM Employee_Leave el WHERE el.SchoolID = @SchoolID AND el.EmployeeID = e.EmployeeID AND @Date BETWEEN el.LeaveStartDate AND el.LeaveEndDate)
             THEN 'UNCOUNTED: no row + on leave (no Leave chip)'
        WHEN ear.Employee_Attendance_RecordID IS NULL AND @AsOf > sd.LateEntryTime THEN 'Abs (virtual, no row)'
        WHEN ear.Employee_Attendance_RecordID IS NULL THEN 'UNCOUNTED: no row, before LateEntryTime'
        WHEN ear.AttendanceStatus = N'Pre' AND ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time) THEN 'Pre'
        WHEN ear.AttendanceStatus = N'Pre' THEN 'UNCOUNTED: Pre with NULL/00:00 EntryTime'
        WHEN ear.AttendanceStatus IN (N'Late', N'Late Abs', N'Abs') THEN ear.AttendanceStatus
        ELSE 'UNCOUNTED: status=' + ISNULL(ear.AttendanceStatus, 'NULL')
    END AS DisplayBucket
INTO #emp
FROM Employee_Attendance_Schedule_Assign eas
JOIN Attendance_Schedule sch ON sch.ScheduleID = eas.ScheduleID
JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = eas.ScheduleID AND sd.SchoolID = eas.SchoolID AND sd.Day = @DayName
JOIN Employee_Info e ON e.EmployeeID = eas.EmployeeID AND e.Job_Status = N'Active'
LEFT JOIN VW_Emp_Info v ON v.EmployeeID = e.EmployeeID
LEFT JOIN Employee_Attendance_Record ear ON ear.SchoolID = @SchoolID AND ear.EmployeeID = eas.EmployeeID
      AND ear.AttendanceDate = @Date AND ear.Attendance_ScheduleID = eas.ScheduleID
LEFT JOIN Registration r ON r.RegistrationID = ear.RegistrationID
WHERE eas.SchoolID = @SchoolID;

SELECT '03a_employee_all' AS section, * FROM #emp ORDER BY ScheduleID, EmployeeName;
SELECT '03b_employee_uncounted' AS section, * FROM #emp WHERE DisplayBucket LIKE 'UNCOUNTED%';
SELECT '03c_employee_rows_any_schedule' AS section, ear.*, r.UserName AS InsertedByUser
FROM Employee_Attendance_Record ear LEFT JOIN Registration r ON r.RegistrationID = ear.RegistrationID
WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = @Date
ORDER BY ear.EmployeeID, ear.Attendance_ScheduleID;

/* ---------- 04  device-data / sync trail (there is no server-side absent job; Abs rows come from the
              device app's local Abs_Insert and are pushed through Attendance_API) ---------- */
SELECT TOP 50 '04_device_update_list' AS section, * FROM Attendance_Device_DataUpdateList
WHERE SchoolID = @SchoolID ORDER BY DateUpdateID DESC;
SELECT '04_device_settings' AS section, Is_Device_Attendance_Enable, Is_Student_Attendance_Enable, Is_Employee_Attendance_Enable,
       Is_Holiday_As_Offday, IsActive
FROM Attendance_Device_Setting WHERE SchoolID = @SchoolID;
