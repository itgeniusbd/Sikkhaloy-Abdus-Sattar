using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Attendances.Online_Display
{
    public partial class DeviceDisplay : Page
    {
        private const int DisplaySqlTimeoutSeconds = 45;

        protected void Page_Init(object sender, EventArgs e)
        {
            StudentScheduleSummarySQL.SelectCommand = StudentSummarySql;
            EmployeeScheduleSummarySQL.SelectCommand = EmployeeSummarySql;
            Student_Entry_LogSQL.SelectCommand = StudentEntrySql;
            Student_Exit_LogSQL.SelectCommand = StudentExitSql;
            EmployeeEntryLogSQL.SelectCommand = EmployeeEntrySql;
            EmployeeExitLogSQL.SelectCommand = EmployeeExitSql;

            WireDisplaySource(StudentScheduleSummarySQL);
            WireDisplaySource(EmployeeScheduleSummarySQL);
            WireDisplaySource(Student_Entry_LogSQL);
            WireDisplaySource(Student_Exit_LogSQL);
            WireDisplaySource(EmployeeEntryLogSQL);
            WireDisplaySource(EmployeeExitLogSQL);

            var schoolId = ResolveSchoolId();
            if (schoolId <= 0)
                return;

            var educationYearId = ResolveEducationYearId(schoolId);
            ApplyEducationYearParameter(StudentScheduleSummarySQL, educationYearId);
            ApplyEducationYearParameter(Student_Entry_LogSQL, educationYearId);
            ApplyEducationYearParameter(Student_Exit_LogSQL, educationYearId);
        }

        private void WireDisplaySource(SqlDataSource source)
        {
            // Several screens of the same school (and the device app's WebView2) hit this page
            // repeatedly; share each result for 15s (cache key = SQL + SchoolID/EducationYearID).
            source.EnableCaching = true;
            source.CacheDuration = 15;
            source.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
            source.Selecting -= DisplaySql_Selecting;
            source.Selected -= DisplaySql_Selected;
            source.Selecting += DisplaySql_Selecting;
            source.Selected += DisplaySql_Selected;
        }

        protected void DisplaySql_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            e.Command.CommandTimeout = DisplaySqlTimeoutSeconds;
            var sql = e.Command.CommandText ?? string.Empty;
            if (sql.IndexOf("READ UNCOMMITTED", StringComparison.OrdinalIgnoreCase) < 0)
            {
                e.Command.CommandText =
                    "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " + sql;
            }
        }

        protected void DisplaySql_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            if (e.Exception != null)
                e.ExceptionHandled = true;
        }

        private int ResolveSchoolId()
        {
            int schoolId;
            if (int.TryParse(Request.QueryString["SchoolID"], out schoolId) && schoolId > 0)
                return schoolId;

            return 0;
        }

        private int ResolveEducationYearId(int schoolId)
        {
            int fromQuery;
            if (int.TryParse(Request.QueryString["EducationYearID"], out fromQuery) && fromQuery > 0)
                return fromQuery;

            const string sql = @"
SELECT TOP 1 EducationYearID
FROM Education_Year
WHERE SchoolID = @SchoolID
  AND (
        Status IN (N'True', N'true', N'1')
        OR IsActive = 1
      )
  AND (StartDate IS NULL OR StartDate <= CONVERT(date, GETDATE()))
  AND (EndDate IS NULL OR EndDate >= CONVERT(date, GETDATE()))
ORDER BY SN DESC, EducationYearID DESC;

SELECT TOP 1 EducationYearID
FROM Education_Year
WHERE SchoolID = @SchoolID
  AND (Status IN (N'True', N'true', N'1') OR IsActive = 1)
ORDER BY SN DESC, EducationYearID DESC;

SELECT TOP 1 EducationYearID
FROM Education_Year
WHERE SchoolID = @SchoolID
ORDER BY CASE WHEN Status IN (N'True', N'true', N'1') THEN 0 ELSE 1 END,
         CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
         SN DESC, EducationYearID DESC;";

            var connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                return 0;

            try
            {
                using (var con = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 8;
                    cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = schoolId;
                    con.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader[0] != DBNull.Value)
                            return Convert.ToInt32(reader[0]);

                        if (reader.NextResult() && reader.Read() && reader[0] != DBNull.Value)
                            return Convert.ToInt32(reader[0]);

                        if (reader.NextResult() && reader.Read() && reader[0] != DBNull.Value)
                            return Convert.ToInt32(reader[0]);
                    }
                }
            }
            catch
            {
                return 0;
            }

            return 0;
        }

        private static void ApplyEducationYearParameter(SqlDataSource source, int educationYearId)
        {
            if (source == null)
                return;

            var value = Math.Max(educationYearId, 0).ToString();
            var param = source.SelectParameters["EducationYearID"];
            if (param == null)
            {
                source.SelectParameters.Add("EducationYearID", DbType.Int32, value);
                return;
            }

            param.DefaultValue = value;
        }

        private const string StudentSummarySql = @"
SELECT sch.ScheduleID,
       sch.ScheduleName,
       LTRIM(RIGHT(CONVERT(varchar(20), sd.StartTime, 100), 7)) + N' - ' + LTRIM(RIGHT(CONVERT(varchar(20), sd.EndTime, 100), 7)) AS TimeRange,
       ISNULL(u.Total_User, 0) AS Total_User,
       ISNULL(a.Current_IN, 0) AS Current_IN,
       ISNULL(a.Total_Out, 0) AS Total_Out,
       ISNULL(a.Total_Present, 0) AS Total_Present,
       ISNULL(a.Total_Late, 0) AS Total_Late,
       ISNULL(a.Total_Late_Absent, 0) AS Total_Late_Absent,
       ISNULL(a.Total_Abs_Recorded, 0) + ISNULL(v.Virtual_Absent, 0) AS Total_Absent,
       ISNULL(a.Total_Leave_Recorded, 0) + ISNULL(lv.Virtual_Leave, 0) AS Total_Leave
FROM Attendance_Schedule sch
INNER JOIN Attendance_Schedule_Day sd
    ON sch.ScheduleID = sd.ScheduleID AND sd.SchoolID = sch.SchoolID
INNER JOIN (
    SELECT ass.ScheduleID, COUNT(DISTINCT ass.StudentID) AS Total_User
    FROM Attendance_Schedule_AssignStudent ass
    INNER JOIN Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
    INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID
    WHERE ass.SchoolID = @SchoolID
    GROUP BY ass.ScheduleID
) u ON u.ScheduleID = sch.ScheduleID
LEFT JOIN (
    SELECT ar.Attendance_ScheduleID AS ScheduleID,
           SUM(CASE WHEN ISNULL(ar.Is_OUT, 0) = 0 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs') AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Current_IN,
           SUM(CASE WHEN ar.ExitTime IS NOT NULL AND ar.Is_OUT = 1 THEN 1 ELSE 0 END) AS Total_Out,
           SUM(CASE WHEN ar.Attendance = N'Pre' AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Total_Present,
           SUM(CASE WHEN ar.Attendance = N'Late' THEN 1 ELSE 0 END) AS Total_Late,
           SUM(CASE WHEN ar.Attendance = N'Late Abs' THEN 1 ELSE 0 END) AS Total_Late_Absent,
           SUM(CASE WHEN (ar.Attendance = N'Abs' OR (ar.Attendance = N'Pre' AND ISNULL(ar.RegistrationID, 0) = 0 AND (ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time)))) THEN 1 ELSE 0 END) AS Total_Abs_Recorded,
           SUM(CASE WHEN ar.Attendance = N'Leave' THEN 1 ELSE 0 END) AS Total_Leave_Recorded
    FROM Attendance_Record ar
    INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
    INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
    INNER JOIN Attendance_Schedule_AssignStudent ass
        ON ass.StudentID = ar.StudentID AND ass.ScheduleID = ar.Attendance_ScheduleID AND ass.SchoolID = @SchoolID
    WHERE ar.SchoolID = @SchoolID
      AND ar.AttendanceDate = CONVERT(date, GETDATE())
      AND ar.Attendance_ScheduleID IS NOT NULL
    GROUP BY ar.Attendance_ScheduleID
) a ON a.ScheduleID = sch.ScheduleID
LEFT JOIN (
    SELECT ass.ScheduleID, COUNT(DISTINCT ass.StudentID) AS Virtual_Absent
    FROM Attendance_Schedule_AssignStudent ass
    INNER JOIN Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
    INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID
    INNER JOIN Attendance_Schedule_Day sd2
        ON sd2.ScheduleID = ass.ScheduleID AND sd2.SchoolID = ass.SchoolID AND sd2.Day = DATENAME(dw, GETDATE())
    LEFT JOIN Attendance_Record ar2
        ON ar2.SchoolID = @SchoolID
       AND ar2.StudentID = ass.StudentID
       AND ar2.AttendanceDate = CONVERT(date, GETDATE())
       AND ar2.Attendance_ScheduleID = ass.ScheduleID
    LEFT JOIN Attendance_Leave al
        ON al.SchoolID = @SchoolID
       AND al.StudentID = ass.StudentID
       AND CONVERT(date, GETDATE()) BETWEEN al.StartDate AND al.EndDate
    WHERE ass.SchoolID = @SchoolID
      AND CAST(GETDATE() AS time) > sd2.LateEntryTime
      AND ar2.StudentID IS NULL
      AND al.StudentID IS NULL
    GROUP BY ass.ScheduleID
) v ON v.ScheduleID = sch.ScheduleID
LEFT JOIN (
    -- On approved leave (Attendance_Leave) with no row for this schedule today: excluded from Abs above, counted as Leave.
    SELECT ass.ScheduleID, COUNT(DISTINCT ass.StudentID) AS Virtual_Leave
    FROM Attendance_Schedule_AssignStudent ass
    INNER JOIN Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
    INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID
    WHERE ass.SchoolID = @SchoolID
      AND NOT EXISTS (
          SELECT 1 FROM Attendance_Record ar3
          WHERE ar3.SchoolID = @SchoolID AND ar3.StudentID = ass.StudentID
            AND ar3.AttendanceDate = CONVERT(date, GETDATE()) AND ar3.Attendance_ScheduleID = ass.ScheduleID)
      AND EXISTS (
          SELECT 1 FROM Attendance_Leave al3
          WHERE al3.SchoolID = @SchoolID AND al3.StudentID = ass.StudentID
            AND CONVERT(date, GETDATE()) BETWEEN al3.StartDate AND al3.EndDate)
    GROUP BY ass.ScheduleID
) lv ON lv.ScheduleID = sch.ScheduleID
WHERE sch.SchoolID = @SchoolID
  AND sd.Day = DATENAME(dw, GETDATE())
  AND ISNULL(sd.Is_OnDay, 1) = 1
ORDER BY sd.StartTime";

        private const string EmployeeSummarySql = @"
SELECT sch.ScheduleID,
       sch.ScheduleName,
       LTRIM(RIGHT(CONVERT(varchar(20), sd.StartTime, 100), 7)) + N' - ' + LTRIM(RIGHT(CONVERT(varchar(20), sd.EndTime, 100), 7)) AS TimeRange,
       ISNULL(u.Total_User, 0) AS Total_User,
       ISNULL(a.Current_IN, 0) AS Current_IN,
       ISNULL(a.Total_Out, 0) AS Total_Out,
       ISNULL(a.Total_Present, 0) AS Total_Present,
       ISNULL(a.Total_Late, 0) AS Total_Late,
       ISNULL(a.Total_Late_Absent, 0) AS Total_Late_Absent,
       ISNULL(a.Total_Abs_Recorded, 0) + ISNULL(v.Virtual_Absent, 0) AS Total_Absent,
       ISNULL(a.Total_Leave_Recorded, 0) + ISNULL(lv.Virtual_Leave, 0) AS Total_Leave
FROM Attendance_Schedule sch
INNER JOIN Attendance_Schedule_Day sd
    ON sch.ScheduleID = sd.ScheduleID AND sd.SchoolID = sch.SchoolID
INNER JOIN (
    SELECT eas.ScheduleID, COUNT(DISTINCT eas.EmployeeID) AS Total_User
    FROM Employee_Attendance_Schedule_Assign eas
    INNER JOIN Employee_Info e ON eas.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
    WHERE eas.SchoolID = @SchoolID
    GROUP BY eas.ScheduleID
) u ON u.ScheduleID = sch.ScheduleID
LEFT JOIN (
    SELECT ear.Attendance_ScheduleID AS ScheduleID,
           SUM(CASE WHEN ISNULL(ear.Is_OUT, 0) = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs') AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Current_IN,
           SUM(CASE WHEN ear.ExitTime IS NOT NULL AND ear.Is_OUT = 1 THEN 1 ELSE 0 END) AS Total_Out,
           SUM(CASE WHEN ear.AttendanceStatus = N'Pre' AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0) THEN 1 ELSE 0 END) AS Total_Present,
           SUM(CASE WHEN ear.AttendanceStatus = N'Late' THEN 1 ELSE 0 END) AS Total_Late,
           SUM(CASE WHEN ear.AttendanceStatus = N'Late Abs' THEN 1 ELSE 0 END) AS Total_Late_Absent,
           SUM(CASE WHEN (ear.AttendanceStatus = N'Abs' OR (ear.AttendanceStatus = N'Pre' AND ISNULL(ear.RegistrationID, 0) = 0 AND (ear.EntryTime IS NULL OR ear.EntryTime = CAST('00:00:00' AS time)))) THEN 1 ELSE 0 END) AS Total_Abs_Recorded,
           SUM(CASE WHEN ear.AttendanceStatus = N'Leave' THEN 1 ELSE 0 END) AS Total_Leave_Recorded
    FROM Employee_Attendance_Record ear
    INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
    INNER JOIN Employee_Attendance_Schedule_Assign eas
        ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = ear.Attendance_ScheduleID AND eas.SchoolID = @SchoolID
    WHERE ear.SchoolID = @SchoolID
      AND ear.AttendanceDate = CONVERT(date, GETDATE())
      AND ear.Attendance_ScheduleID IS NOT NULL
    GROUP BY ear.Attendance_ScheduleID
) a ON a.ScheduleID = sch.ScheduleID
LEFT JOIN (
    SELECT eas.ScheduleID, COUNT(DISTINCT eas.EmployeeID) AS Virtual_Absent
    FROM Employee_Attendance_Schedule_Assign eas
    INNER JOIN Employee_Info e ON eas.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
    INNER JOIN Attendance_Schedule_Day sd2
        ON sd2.ScheduleID = eas.ScheduleID AND sd2.SchoolID = eas.SchoolID AND sd2.Day = DATENAME(dw, GETDATE())
    LEFT JOIN Employee_Attendance_Record ear2
        ON ear2.SchoolID = @SchoolID
       AND ear2.EmployeeID = eas.EmployeeID
       AND ear2.AttendanceDate = CONVERT(date, GETDATE())
       AND ear2.Attendance_ScheduleID = eas.ScheduleID
    LEFT JOIN Employee_Leave el
        ON el.SchoolID = @SchoolID
       AND el.EmployeeID = eas.EmployeeID
       AND CONVERT(date, GETDATE()) BETWEEN el.LeaveStartDate AND el.LeaveEndDate
    WHERE eas.SchoolID = @SchoolID
      AND CAST(GETDATE() AS time) > sd2.LateEntryTime
      AND ear2.EmployeeID IS NULL
      AND el.EmployeeID IS NULL
    GROUP BY eas.ScheduleID
) v ON v.ScheduleID = sch.ScheduleID
LEFT JOIN (
    -- On leave (Employee_Leave) with no row for this schedule today: excluded from Abs above, counted as Leave.
    SELECT eas.ScheduleID, COUNT(DISTINCT eas.EmployeeID) AS Virtual_Leave
    FROM Employee_Attendance_Schedule_Assign eas
    INNER JOIN Employee_Info e ON eas.EmployeeID = e.EmployeeID AND e.Job_Status = N'Active'
    WHERE eas.SchoolID = @SchoolID
      AND NOT EXISTS (
          SELECT 1 FROM Employee_Attendance_Record ear3
          WHERE ear3.SchoolID = @SchoolID AND ear3.EmployeeID = eas.EmployeeID
            AND ear3.AttendanceDate = CONVERT(date, GETDATE()) AND ear3.Attendance_ScheduleID = eas.ScheduleID)
      AND EXISTS (
          SELECT 1 FROM Employee_Leave el3
          WHERE el3.SchoolID = @SchoolID AND el3.EmployeeID = eas.EmployeeID
            AND CONVERT(date, GETDATE()) BETWEEN el3.LeaveStartDate AND el3.LeaveEndDate)
    GROUP BY eas.ScheduleID
) lv ON lv.ScheduleID = sch.ScheduleID
WHERE sch.SchoolID = @SchoolID
  AND sd.Day = DATENAME(dw, GETDATE())
  AND ISNULL(sd.Is_OnDay, 1) = 1
ORDER BY sd.StartTime";

        private const string StudentEntrySql = @"
SELECT ar.StudentID,
       s.StudentsName,
       ar.Attendance_ScheduleID AS ScheduleID,
       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
       ar.Attendance,
       ISNULL(CONVERT(varchar(15), NULLIF(ar.EntryTime, CAST('00:00:00' AS time)), 100), N'Manual') AS EntryTime
FROM Attendance_Record ar
INNER JOIN Student s ON ar.StudentID = s.StudentID AND s.Status = N'Active'
INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
LEFT JOIN Attendance_Schedule sch ON ar.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ar.SchoolID
WHERE ar.AttendanceDate = CONVERT(date, GETDATE())
  AND ISNULL(ar.Is_OUT, 0) = 0 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs') AND ((ar.EntryTime IS NOT NULL AND ar.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ar.RegistrationID, 0) <> 0)
  AND ar.SchoolID = @SchoolID
  AND ISNULL(ar.Attendance_ScheduleID, 0) > 0
ORDER BY CASE WHEN ar.EntryTime IS NULL OR ar.EntryTime = CAST('00:00:00' AS time) THEN 1 ELSE 0 END, ar.EntryTime DESC";

        private const string StudentExitSql = @"
SELECT StudentID, StudentsName, ScheduleID, ScheduleName, Attendance, EntryTime, ExitTime
FROM (
    SELECT ar.StudentID,
           s.StudentsName,
           ar.Attendance_ScheduleID AS ScheduleID,
           ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
           ar.Attendance,
           CONVERT(varchar(15), ar.EntryTime, 100) AS EntryTime,
           CONVERT(varchar(15), ar.ExitTime, 100) AS ExitTime,
           ROW_NUMBER() OVER (
               PARTITION BY ar.StudentID, ISNULL(ar.Attendance_ScheduleID, 0)
               ORDER BY CASE ar.Attendance WHEN N'Abs' THEN 2 ELSE 1 END,
                        ar.ExitTime DESC, ar.EntryTime DESC
           ) AS RowNum
    FROM Attendance_Record ar
    INNER JOIN Student s ON ar.StudentID = s.StudentID AND s.Status = N'Active'
    INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
    LEFT JOIN Attendance_Schedule sch ON ar.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ar.SchoolID
    WHERE ar.AttendanceDate = CONVERT(date, GETDATE())
      AND (ar.Is_OUT = 1 OR ar.Attendance = N'Abs')
      AND ar.SchoolID = @SchoolID
      AND ISNULL(ar.Attendance_ScheduleID, 0) > 0

    UNION ALL

    SELECT ass.StudentID,
           s.StudentsName,
           sch.ScheduleID,
           ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
           N'Abs' AS Attendance,
           CONVERT(varchar(15), CAST(NULL AS time), 100) AS EntryTime,
           CONVERT(varchar(15), CAST(NULL AS time), 100) AS ExitTime,
           1 AS RowNum
    FROM Attendance_Schedule_AssignStudent ass
    INNER JOIN Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
    INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
    INNER JOIN Attendance_Schedule sch ON sch.ScheduleID = ass.ScheduleID AND sch.SchoolID = ass.SchoolID
    INNER JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = sch.ScheduleID AND sd.SchoolID = sch.SchoolID
         AND sd.Day = DATENAME(dw, GETDATE()) AND ISNULL(sd.Is_OnDay, 1) = 1
    WHERE ass.SchoolID = @SchoolID
      AND CAST(GETDATE() AS time) > sd.LateEntryTime
      AND NOT EXISTS (
          SELECT 1 FROM Attendance_Record ar2
          WHERE ar2.SchoolID = @SchoolID
            AND ar2.StudentID = ass.StudentID
            AND ar2.AttendanceDate = CONVERT(date, GETDATE())
            AND ar2.Attendance_ScheduleID = sch.ScheduleID
      )
      AND NOT EXISTS (
          SELECT 1 FROM Attendance_Leave al
          WHERE al.SchoolID = @SchoolID
            AND al.StudentID = ass.StudentID
            AND CONVERT(date, GETDATE()) BETWEEN al.StartDate AND al.EndDate
      )
) ranked
WHERE RowNum = 1
ORDER BY ExitTime DESC, EntryTime DESC";

        private const string EmployeeEntrySql = @"
SELECT ear.EmployeeID,
       e.FirstName + N' ' + e.LastName AS Name,
       ear.Attendance_ScheduleID AS ScheduleID,
       ISNULL(schInfo.ScheduleName, N'Schedule') AS ScheduleName,
       ear.AttendanceStatus,
       ISNULL(CONVERT(varchar(15), NULLIF(ear.EntryTime, CAST('00:00:00' AS time)), 100), N'Manual') AS EntryTime
FROM Employee_Attendance_Record ear
INNER JOIN VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID
LEFT JOIN Attendance_Schedule schInfo
    ON schInfo.SchoolID = ear.SchoolID AND schInfo.ScheduleID = ear.Attendance_ScheduleID
INNER JOIN Attendance_Schedule_Day sdCard
    ON sdCard.ScheduleID = ear.Attendance_ScheduleID AND sdCard.SchoolID = ear.SchoolID AND sdCard.Day = DATENAME(dw, GETDATE())
WHERE ear.AttendanceDate = CONVERT(date, GETDATE())
  AND ISNULL(ear.Is_OUT, 0) = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs') AND ((ear.EntryTime IS NOT NULL AND ear.EntryTime <> CAST('00:00:00' AS time)) OR ISNULL(ear.RegistrationID, 0) <> 0)
  AND ear.SchoolID = @SchoolID
  AND ear.Attendance_ScheduleID IS NOT NULL
ORDER BY sdCard.StartTime DESC, CASE WHEN ear.EntryTime IS NULL OR ear.EntryTime = CAST('00:00:00' AS time) THEN 1 ELSE 0 END, ear.EntryTime DESC";

        private const string EmployeeExitSql = @"
SELECT EmployeeID, Name, ScheduleID, ScheduleName, AttendanceStatus, EntryTime, ExitTime
FROM (
    SELECT ear.EmployeeID,
           e.FirstName + N' ' + e.LastName AS Name,
           ear.Attendance_ScheduleID AS ScheduleID,
           ISNULL(schInfo.ScheduleName, N'Schedule') AS ScheduleName,
           ear.AttendanceStatus,
           CONVERT(varchar(15), ear.EntryTime, 100) AS EntryTime,
           CONVERT(varchar(15), ear.ExitTime, 100) AS ExitTime,
           sdCard.StartTime AS SortStart,
           ROW_NUMBER() OVER (
               PARTITION BY ear.EmployeeID, ear.Attendance_ScheduleID
               ORDER BY CASE ear.AttendanceStatus WHEN N'Abs' THEN 2 ELSE 1 END,
                        CASE WHEN ear.ExitTime IS NULL THEN 1 ELSE 0 END,
                        ear.ExitTime DESC, ear.EntryTime DESC
           ) AS RowNum
    FROM Employee_Attendance_Record ear
    INNER JOIN VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID
    LEFT JOIN Attendance_Schedule schInfo
        ON schInfo.SchoolID = ear.SchoolID AND schInfo.ScheduleID = ear.Attendance_ScheduleID
    INNER JOIN Attendance_Schedule_Day sdCard
        ON sdCard.ScheduleID = ear.Attendance_ScheduleID AND sdCard.SchoolID = ear.SchoolID AND sdCard.Day = DATENAME(dw, GETDATE())
    WHERE ear.AttendanceDate = CONVERT(date, GETDATE())
      AND (ear.Is_OUT = 1 OR ear.AttendanceStatus = N'Abs')
      AND ear.SchoolID = @SchoolID
      AND ear.Attendance_ScheduleID IS NOT NULL

    UNION ALL

    SELECT eas.EmployeeID,
           e.FirstName + N' ' + e.LastName AS Name,
           sch.ScheduleID,
           ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,
           N'Abs' AS AttendanceStatus,
           CONVERT(varchar(15), CAST(NULL AS time), 100) AS EntryTime,
           CONVERT(varchar(15), CAST(NULL AS time), 100) AS ExitTime,
           sd.StartTime AS SortStart,
           1 AS RowNum
    FROM Employee_Attendance_Schedule_Assign eas
    INNER JOIN VW_Emp_Info e ON eas.EmployeeID = e.EmployeeID
    INNER JOIN Attendance_Schedule sch ON sch.ScheduleID = eas.ScheduleID AND sch.SchoolID = eas.SchoolID
    INNER JOIN Attendance_Schedule_Day sd ON sd.ScheduleID = sch.ScheduleID AND sd.SchoolID = sch.SchoolID
         AND sd.Day = DATENAME(dw, GETDATE()) AND ISNULL(sd.Is_OnDay, 1) = 1
    WHERE eas.SchoolID = @SchoolID
      AND CAST(GETDATE() AS time) > sd.LateEntryTime
      AND NOT EXISTS (
          SELECT 1 FROM Employee_Attendance_Record ear2
          WHERE ear2.SchoolID = @SchoolID
            AND ear2.EmployeeID = eas.EmployeeID
            AND ear2.AttendanceDate = CONVERT(date, GETDATE())
            AND ear2.Attendance_ScheduleID = sch.ScheduleID
      )
) ranked
WHERE RowNum = 1
ORDER BY SortStart, Name";

}
}
