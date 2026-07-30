using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Education
{
    public class ScheduleOverlapInfo
    {
        public string ExistingScheduleName { get; set; }
        public TimeSpan ExistingStart { get; set; }
        public TimeSpan ExistingEnd { get; set; }
        public string TargetScheduleName { get; set; }
        public TimeSpan TargetStart { get; set; }
        public TimeSpan TargetEnd { get; set; }

        public string Message
        {
            get
            {
                return "Cannot assign: time overlaps with existing schedule '"
                    + ExistingScheduleName + "' ("
                    + FormatTime(ExistingStart) + " - " + FormatTime(ExistingEnd)
                    + ") and target schedule '"
                    + TargetScheduleName + "' ("
                    + FormatTime(TargetStart) + " - " + FormatTime(TargetEnd) + ").";
            }
        }

        private static string FormatTime(TimeSpan time)
        {
            return DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }
    }

    public static class ScheduleOverlapValidator
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();

        private const string OverlapSql = @"
SELECT TOP 1
    existing.ScheduleName AS ExistingScheduleName,
    existing.StartTime AS ExistingStartTime,
    existing.EndTime AS ExistingEndTime,
    target.ScheduleName AS TargetScheduleName,
    target.StartTime AS TargetStartTime,
    target.EndTime AS TargetEndTime
FROM {AssignTable} assignTbl
INNER JOIN Attendance_Schedule existing ON existing.ScheduleID = assignTbl.ScheduleID AND existing.SchoolID = assignTbl.SchoolID
INNER JOIN Attendance_Schedule target ON target.ScheduleID = @TargetScheduleID AND target.SchoolID = @SchoolID
WHERE assignTbl.SchoolID = @SchoolID
  AND assignTbl.{PersonColumn} = @PersonID
  AND assignTbl.ScheduleID <> @TargetScheduleID
  AND existing.StartTime IS NOT NULL
  AND existing.EndTime IS NOT NULL
  AND target.StartTime IS NOT NULL
  AND target.EndTime IS NOT NULL
  AND existing.StartTime < target.EndTime
  AND target.StartTime < existing.EndTime";

        public static ScheduleOverlapInfo GetEmployeeOverlap(int schoolId, int employeeId, int targetScheduleId)
        {
            string sql = OverlapSql
                .Replace("{AssignTable}", "Employee_Attendance_Schedule_Assign")
                .Replace("{PersonColumn}", "EmployeeID");

            return GetOverlap(sql, schoolId, employeeId, targetScheduleId);
        }

        public static ScheduleOverlapInfo GetStudentOverlap(int schoolId, int studentId, int targetScheduleId)
        {
            string sql = OverlapSql
                .Replace("{AssignTable}", "Attendance_Schedule_AssignStudent")
                .Replace("{PersonColumn}", "StudentID");

            return GetOverlap(sql, schoolId, studentId, targetScheduleId);
        }

        private static ScheduleOverlapInfo GetOverlap(string sql, int schoolId, int personId, int targetScheduleId)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = schoolId;
                cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = personId;
                cmd.Parameters.Add("@TargetScheduleID", SqlDbType.Int).Value = targetScheduleId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new ScheduleOverlapInfo
                    {
                        ExistingScheduleName = reader["ExistingScheduleName"].ToString(),
                        ExistingStart = (TimeSpan)reader["ExistingStartTime"],
                        ExistingEnd = (TimeSpan)reader["ExistingEndTime"],
                        TargetScheduleName = reader["TargetScheduleName"].ToString(),
                        TargetStart = (TimeSpan)reader["TargetStartTime"],
                        TargetEnd = (TimeSpan)reader["TargetEndTime"]
                    };
                }
            }
        }
    }
}
