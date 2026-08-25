using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Attendance_API.Helpers
{
    internal sealed class AttendanceSmsTemplateHelper
    {
        private readonly string _connectionString;
        private readonly int _schoolId;

        public AttendanceSmsTemplateHelper(int schoolId)
        {
            _schoolId = schoolId;
            _connectionString = ConfigurationManager.ConnectionStrings["EduConnection"].ConnectionString;
        }

        public string GetScheduleName(int? scheduleId, int studentId = 0)
        {
            if (scheduleId.HasValue && scheduleId.Value > 0)
            {
                var name = QueryScheduleName(scheduleId.Value);
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            if (studentId <= 0)
                return string.Empty;

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 ISNULL(NULLIF(LTRIM(RTRIM(sch.ScheduleName)), N''),
                           N'Schedule ' + CAST(ass.ScheduleID AS nvarchar(20)))
                    FROM Attendance_Schedule_AssignStudent ass
                    INNER JOIN Attendance_Schedule sch
                        ON ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = sch.SchoolID
                    INNER JOIN Education_Year ey ON ass.EducationYearID = ey.EducationYearID
                    WHERE ass.SchoolID = @SchoolID
                      AND ass.StudentID = @StudentID
                      AND ey.Status = N'True'
                    ORDER BY sch.StartTime, ass.ScheduleID", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value ? string.Empty : result.ToString().Trim();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string QueryScheduleName(int scheduleId)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
                    @"SELECT TOP 1 ISNULL(NULLIF(LTRIM(RTRIM(ScheduleName)), N''), N'Schedule ' + CAST(@ScheduleID AS nvarchar(20)))
                      FROM Attendance_Schedule
                      WHERE SchoolID = @SchoolID AND ScheduleID = @ScheduleID", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                    cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value ? string.Empty : result.ToString().Trim();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public (string className, string roll, string displayId) GetStudentClassInfo(int studentId)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 CreateClass.Class, StudentsClass.RollNo, Student.ID
                    FROM StudentsClass
                    INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
                    INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
                    INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
                    WHERE StudentsClass.StudentID = @StudentID
                      AND StudentsClass.SchoolID = @SchoolID
                      AND Education_Year.Status = N'True'", con))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return (string.Empty, string.Empty, string.Empty);

                        return (
                            reader["Class"]?.ToString() ?? string.Empty,
                            reader["RollNo"]?.ToString() ?? string.Empty,
                            reader["ID"]?.ToString() ?? string.Empty);
                    }
                }
            }
            catch
            {
                return (string.Empty, string.Empty, string.Empty);
            }
        }

        public string GetStudentDisplayId(int studentId)
        {
            var classInfo = GetStudentClassInfo(studentId);
            if (!string.IsNullOrWhiteSpace(classInfo.displayId))
                return classInfo.displayId.Trim();

            return QueryStudentIdColumn(studentId);
        }

        private string QueryStudentIdColumn(int studentId)
        {
            if (studentId <= 0)
                return string.Empty;

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 1 ID
                    FROM Student
                    WHERE StudentID = @StudentID
                      AND SchoolID = @SchoolID", con))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value
                        ? string.Empty
                        : result.ToString().Trim();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public string BuildMessage(
            string attendanceType,
            string studentName,
            string displayId,
            string schoolName,
            DateTime attendanceDate,
            TimeSpan? entryTime,
            TimeSpan? exitTime,
            TimeSpan startTime,
            string className,
            string roll,
            string scheduleName,
            bool isEnglish)
        {
            var normalizedType = NormalizeAttendanceType(attendanceType);
            var template = ResolveAttendanceTemplate(normalizedType);
            if (string.IsNullOrWhiteSpace(template))
                template = GetDefaultTemplate(normalizedType, isEnglish);

            return ApplyMessagePlaceholders(
                template,
                studentName,
                displayId,
                schoolName,
                attendanceDate,
                entryTime,
                exitTime,
                startTime,
                className,
                roll,
                scheduleName,
                attendanceType);
        }

        public string BuildEmployeeMessage(
            string attendanceType,
            string employeeName,
            string schoolName,
            DateTime attendanceDate,
            TimeSpan? entryTime,
            TimeSpan startTime,
            string scheduleName,
            bool isEnglish,
            bool toOwnNumber)
        {
            var normalizedType = NormalizeAttendanceType(attendanceType);
            var template = GetEmployeeDefaultTemplate(normalizedType, isEnglish, toOwnNumber);

            return ApplyMessagePlaceholders(
                template,
                employeeName,
                string.Empty,
                schoolName,
                attendanceDate,
                entryTime,
                null,
                startTime,
                string.Empty,
                string.Empty,
                scheduleName,
                attendanceType);
        }

        private static string ApplyMessagePlaceholders(
            string template,
            string personName,
            string displayId,
            string schoolName,
            DateTime attendanceDate,
            TimeSpan? entryTime,
            TimeSpan? exitTime,
            TimeSpan startTime,
            string className,
            string roll,
            string scheduleName,
            string attendanceType)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            var entryDateTime = entryTime.HasValue
                ? attendanceDate.Date.Add(entryTime.Value)
                : attendanceDate;
            var exitDateTime = exitTime.HasValue
                ? attendanceDate.Date.Add(exitTime.Value)
                : attendanceDate;
            var lateMinutes = entryTime.HasValue
                ? Math.Max(0, (int)(entryTime.Value - startTime).TotalMinutes)
                : 0;

            return template
                .Replace("{StudentName}", personName ?? string.Empty)
                .Replace("{EmployeeName}", personName ?? string.Empty)
                .Replace("{ID}", displayId ?? string.Empty)
                .Replace("{SchoolName}", schoolName ?? string.Empty)
                .Replace("{Date}", attendanceDate.ToString("d MMM yy"))
                .Replace("{Class}", className ?? string.Empty)
                .Replace("{Roll}", roll ?? string.Empty)
                .Replace("{ScheduleName}", scheduleName ?? string.Empty)
                .Replace("{Status}", attendanceType ?? string.Empty)
                .Replace("{EntryTime}", entryTime.HasValue ? entryDateTime.ToString("h:mm tt") : string.Empty)
                .Replace("{ExitTime}", exitTime.HasValue ? exitDateTime.ToString("h:mm tt") : string.Empty)
                .Replace("{LateMinutes}", lateMinutes.ToString());
        }

        private string GetDefaultTemplate(string attendanceType, bool isEnglish)
        {
            switch (NormalizeAttendanceType(attendanceType))
            {
                case "Pre":
                case "Entry":
                    return isEnglish
                        ? "Respected Guardian, {StudentName} has safely entered in {SchoolName} at {EntryTime}. Schedule: {ScheduleName}. Date: {Date}"
                        : "সম্মানিত অভিভাবক, {StudentName} নিরাপদে {SchoolName} এ {EntryTime} এ প্রবেশ করেছে। শিডিউল: {ScheduleName}। তারিখ: {Date}";
                case "Late":
                    return isEnglish
                        ? "Respected guardian, {StudentName} today({Date}) late {LateMinutes} min from schedule \"{ScheduleName}\", entry time {EntryTime}. {SchoolName}"
                        : "সম্মানিত অভিভাবক, {StudentName} আজ({Date}) \"{ScheduleName}\" শিডিউলে {LateMinutes} মি. বিলম্বে প্রবেশ করেছে। প্রবেশ সময় {EntryTime}। {SchoolName}";
                case "LateAbs":
                    return isEnglish
                        ? "Respected guardian, {StudentName} today({Date}) late absent from schedule \"{ScheduleName}\", entry time {EntryTime}. {SchoolName}"
                        : "সম্মানিত অভিভাবক, {StudentName} আজ({Date}) \"{ScheduleName}\" শিডিউলে বিলম্বে অনুপস্থিত। প্রবেশ সময় {EntryTime}। {SchoolName}";
                case "Absent":
                    return isEnglish
                        ? "Respected guardian, {StudentName} today({Date}) absent from schedule \"{ScheduleName}\", please send to class regularly. {SchoolName}"
                        : "সম্মানিত অভিভাবক, {StudentName} আজ({Date}) \"{ScheduleName}\" শিডিউলে অনুপস্থিত, অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান। {SchoolName}";
                case "Exit":
                    return isEnglish
                        ? "Respected guardian, {StudentName} has exited today({Date}) from {SchoolName} at {ExitTime}. Schedule: {ScheduleName}"
                        : "সম্মানিত অভিভাবক, {StudentName} আজ({Date}) {SchoolName} থেকে {ExitTime} এ প্রস্থান করেছে। শিডিউল: {ScheduleName}";
                default:
                    return string.Empty;
            }
        }

        private static string GetEmployeeDefaultTemplate(string attendanceType, bool isEnglish, bool toOwnNumber)
        {
            if (isEnglish)
            {
                if (toOwnNumber)
                {
                    switch (attendanceType)
                    {
                        case "LateAbs":
                            return "Dear {EmployeeName}, you are marked late absent today ({Date}) for schedule \"{ScheduleName}\". Entry time {EntryTime}. {SchoolName}";
                        case "Absent":
                            return "Dear {EmployeeName}, you are marked absent today ({Date}) for schedule \"{ScheduleName}\". {SchoolName}";
                        case "Late":
                            return "Dear {EmployeeName}, you are {LateMinutes} min late today ({Date}) for schedule \"{ScheduleName}\". Entry time {EntryTime}. {SchoolName}";
                        default:
                            return string.Empty;
                    }
                }

                switch (attendanceType)
                {
                    case "LateAbs":
                        return "Attendance notice: {EmployeeName} is late absent today ({Date}) from schedule \"{ScheduleName}\", entry time {EntryTime}. {SchoolName}";
                    case "Absent":
                        return "Attendance notice: {EmployeeName} is absent today ({Date}) from schedule \"{ScheduleName}\". {SchoolName}";
                    case "Late":
                        return "Attendance notice: {EmployeeName} is {LateMinutes} min late today ({Date}) from schedule \"{ScheduleName}\", entry time {EntryTime}. {SchoolName}";
                    default:
                        return string.Empty;
                }
            }

            if (toOwnNumber)
            {
                switch (attendanceType)
                {
                    case "LateAbs":
                        return "{EmployeeName}, আপনি আজ({Date}) \"{ScheduleName}\" শিডিউলে বিলম্বে অনুপস্থিত। প্রবেশ সময় {EntryTime}। {SchoolName}";
                    case "Absent":
                        return "{EmployeeName}, আপনি আজ({Date}) \"{ScheduleName}\" শিডিউলে অনুপস্থিত। {SchoolName}";
                    case "Late":
                        return "{EmployeeName}, আপনি আজ({Date}) \"{ScheduleName}\" শিডিউলে {LateMinutes} মি. বিলম্বে প্রবেশ করেছেন। প্রবেশ সময় {EntryTime}। {SchoolName}";
                    default:
                        return string.Empty;
                }
            }

            switch (attendanceType)
            {
                case "LateAbs":
                    return "হাজিরা বিজ্ঞপ্তি: {EmployeeName} আজ({Date}) \"{ScheduleName}\" শিডিউলে বিলম্বে অনুপস্থিত। প্রবেশ সময় {EntryTime}। {SchoolName}";
                case "Absent":
                    return "হাজিরা বিজ্ঞপ্তি: {EmployeeName} আজ({Date}) \"{ScheduleName}\" শিডিউলে অনুপস্থিত। {SchoolName}";
                case "Late":
                    return "হাজিরা বিজ্ঞপ্তি: {EmployeeName} আজ({Date}) \"{ScheduleName}\" শিডিউলে {LateMinutes} মি. বিলম্বে প্রবেশ করেছেন। প্রবেশ সময় {EntryTime}। {SchoolName}";
                default:
                    return string.Empty;
            }
        }

        private string ResolveAttendanceTemplate(string attendanceType)
        {
            if (string.IsNullOrWhiteSpace(attendanceType))
                return null;

            var normalizedType = NormalizeAttendanceType(attendanceType);

            foreach (var activeOnly in new[] { true, false })
            {
                var result = QueryTemplate("Attendance", normalizedType, activeOnly);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;

                foreach (var legacyType in GetLegacyTypes(normalizedType))
                {
                    result = QueryTemplate("Attendance", legacyType, activeOnly);
                    if (!string.IsNullOrWhiteSpace(result))
                        return result;
                }

                result = FindByMarkers(normalizedType, activeOnly);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }

            return null;
        }

        private static string NormalizeAttendanceType(string attendanceType)
        {
            if (string.Equals(attendanceType, "Pre", StringComparison.OrdinalIgnoreCase))
                return "Entry";
            if (string.Equals(attendanceType, "Late Abs", StringComparison.OrdinalIgnoreCase))
                return "LateAbs";
            if (string.Equals(attendanceType, "Abs", StringComparison.OrdinalIgnoreCase))
                return "Absent";
            return attendanceType;
        }

        private static string[] GetLegacyTypes(string attendanceType)
        {
            switch (attendanceType)
            {
                case "Entry": return new[] { "Present" };
                case "LateAbs": return new string[0];
                default: return new string[0];
            }
        }

        private string FindByMarkers(string attendanceType, bool activeOnly)
        {
            string[] preferTypes;
            string[] markers;

            switch (attendanceType)
            {
                case "Exit":
                    preferTypes = new[] { "Exit" };
                    markers = new[] { "{ExitTime}" };
                    break;
                case "Late":
                    preferTypes = new[] { "Late" };
                    markers = new[] { "{LateMinutes}" };
                    break;
                case "LateAbs":
                    preferTypes = new[] { "LateAbs", "Late" };
                    markers = new[] { "{LateMinutes}" };
                    break;
                case "Entry":
                    preferTypes = new[] { "Entry", "Present" };
                    markers = new[] { "{EntryTime}" };
                    break;
                case "Absent":
                    preferTypes = new[] { "Absent" };
                    markers = new[] { "{ScheduleName}" };
                    break;
                default:
                    return null;
            }

            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    if (!TableExists(con))
                        return null;

                    var sql = @"
                        SELECT TOP 1 MessageTemplate FROM SMS_Template
                        WHERE SchoolID = @SchoolID
                          AND TemplateCategory = 'Attendance'
                          AND TemplateType IN (" + string.Join(",", preferTypes.Select((_, i) => "@PT" + i)) + @")
                          AND (" + string.Join(" OR ", markers.Select((_, i) => "MessageTemplate LIKE @M" + i)) + @")";
                    if (activeOnly)
                        sql += " AND IsActive = 1";
                    sql += " ORDER BY CreatedDate DESC";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                        for (var i = 0; i < preferTypes.Length; i++)
                            cmd.Parameters.AddWithValue("@PT" + i, preferTypes[i]);
                        for (var i = 0; i < markers.Length; i++)
                            cmd.Parameters.AddWithValue("@M" + i, "%" + markers[i] + "%");

                        var result = cmd.ExecuteScalar();
                        return result == null || result == DBNull.Value ? null : result.ToString().Trim();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private string QueryTemplate(string category, string templateType, bool activeOnly)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    if (!TableExists(con))
                        return null;

                    var sql = @"
                        SELECT TOP 1 MessageTemplate FROM SMS_Template
                        WHERE SchoolID = @SchoolID AND TemplateType = @TemplateType";
                    if (!string.IsNullOrWhiteSpace(category))
                        sql += " AND TemplateCategory = @Category";
                    if (activeOnly)
                        sql += " AND IsActive = 1";
                    sql += " ORDER BY CreatedDate DESC";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                        cmd.Parameters.AddWithValue("@TemplateType", templateType);
                        if (!string.IsNullOrWhiteSpace(category))
                            cmd.Parameters.AddWithValue("@Category", category);

                        var result = cmd.ExecuteScalar();
                        return result == null || result == DBNull.Value ? null : result.ToString().Trim();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool TableExists(SqlConnection con)
        {
            using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SMS_Template')
                    SELECT 1 ELSE SELECT 0", con))
            {
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }
    }
}
