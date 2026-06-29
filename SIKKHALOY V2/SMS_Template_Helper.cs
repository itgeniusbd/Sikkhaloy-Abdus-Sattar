using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Education
{
    /// <summary>
    /// SMS Template Helper - Handles all SMS template operations for Attendance, Payment, Exam, Due, Admission
    /// </summary>
    public class SMS_Template_Helper
    {
     private readonly string _connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
        private readonly int _schoolId;

        public SMS_Template_Helper(int schoolId)
        {
  _schoolId = schoolId;
        }

 /// <summary>
        /// Get SMS template by category and type
      /// </summary>
     public string GetTemplate(string category, string templateType)
        {
            return GetTemplate(category, templateType, activeOnly: true);
        }

        public string GetTemplate(string category, string templateType, bool activeOnly)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    if (!TableExists(con))
                        return null;

                    bool columnExists = ColumnExists(con, "TemplateCategory");

                    string result = QueryTemplate(con, columnExists, category, templateType, activeOnly);
                    if (string.IsNullOrWhiteSpace(result) && columnExists)
                        result = QueryTemplate(con, columnExists, null, templateType, activeOnly);

                    if (string.IsNullOrWhiteSpace(result))
                    {
                        foreach (string legacyType in GetLegacyTypeFallbacks(templateType))
                        {
                            result = QueryTemplate(con, columnExists, category, legacyType, activeOnly);
                            if (string.IsNullOrWhiteSpace(result) && columnExists)
                                result = QueryTemplate(con, columnExists, null, legacyType, activeOnly);
                            if (!string.IsNullOrWhiteSpace(result))
                                break;
                        }
                    }

                    return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting template: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Donor payment receipt SMS — handles wrong type saved (e.g. DonorDue with {Amount}).
        /// </summary>
        public static string GetDonorPaymentTemplate(int schoolId)
        {
            var helper = new SMS_Template_Helper(schoolId);
            foreach (bool activeOnly in new[] { true, false })
            {
                string result = helper.GetTemplate("Donor", "DonorPayment", activeOnly);
                if (!string.IsNullOrEmpty(result)) return result;

                result = helper.FindCategoryTemplateByMarkers(
                    "Donor", activeOnly,
                    new[] { "DonorPayment", "DonorThankYou" },
                    "{ReceiptNo}", "{Amount}", "{PaymentDetails}");
                if (!string.IsNullOrEmpty(result)) return result;
            }
            return null;
        }

        /// <summary>
        /// Donor due notice SMS.
        /// </summary>
        public static string GetDonorDueTemplate(int schoolId)
        {
            var helper = new SMS_Template_Helper(schoolId);
            foreach (bool activeOnly in new[] { true, false })
            {
                string result = helper.GetTemplate("Donor", "DonorDue", activeOnly);
                if (!string.IsNullOrEmpty(result)) return result;

                result = helper.FindCategoryTemplateByMarkers(
                    "Donor", activeOnly,
                    new[] { "DonorDue", "DonorReminder" },
                    "{TotalDue}", "{DueDetails}");
                if (!string.IsNullOrEmpty(result)) return result;
            }
            return null;
        }

        /// <summary>
        /// Student due notice SMS (Present_Due page).
        /// </summary>
        public static string GetStudentDueTemplate(int schoolId)
        {
            var helper = new SMS_Template_Helper(schoolId);
            foreach (bool activeOnly in new[] { true, false })
            {
                string result = helper.GetTemplate("Due", "Due", activeOnly);
                if (!string.IsNullOrEmpty(result)) return result;

                result = helper.FindCategoryTemplateByMarkers(
                    "Due", activeOnly,
                    new[] { "Due", "DueReminder" },
                    "{TotalDue}", "{DueDetails}", "{StudentName}");
                if (!string.IsNullOrEmpty(result)) return result;
            }
            return null;
        }

        /// <summary>
        /// Exam result SMS — Passed or Failed (ExamPosition page).
        /// </summary>
        public static string GetExamResultTemplate(int schoolId, string resultType)
        {
            if (resultType != "Passed" && resultType != "Failed")
                return null;

            var helper = new SMS_Template_Helper(schoolId);
            foreach (bool activeOnly in new[] { true, false })
            {
                string result = helper.GetTemplate("ExamResult", resultType, activeOnly);
                if (!string.IsNullOrEmpty(result)) return result;

                result = helper.ResolveExamTemplateWithBlankType(resultType, activeOnly);
                if (!string.IsNullOrEmpty(result)) return result;
            }
            return null;
        }

        /// <summary>
        /// Attendance SMS — Entry, Exit, Late, LateAbs, Absent (device attendance API).
        /// </summary>
        private string ResolveAttendanceTemplate(string attendanceType)
        {
            if (string.IsNullOrWhiteSpace(attendanceType))
                return null;

            foreach (bool activeOnly in new[] { true, false })
            {
                string result = GetTemplate("Attendance", attendanceType, activeOnly);
                if (!string.IsNullOrEmpty(result))
                    return result;

                string[] preferTypes = GetAttendancePreferTypes(attendanceType);
                string[] markers = GetAttendanceMarkers(attendanceType);
                if (markers != null && markers.Length > 0)
                {
                    result = FindCategoryTemplateByMarkers("Attendance", activeOnly, preferTypes, markers);
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }

                result = ResolveAttendanceTemplateWithBlankType(attendanceType, activeOnly);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
            return null;
        }

        private static string[] GetAttendancePreferTypes(string attendanceType)
        {
            switch (attendanceType)
            {
                case "Entry": return new[] { "Entry", "Present" };
                case "Exit": return new[] { "Exit" };
                case "Late": return new[] { "Late" };
                case "LateAbs": return new[] { "LateAbs", "Late" };
                case "Absent": return new[] { "Absent" };
                default: return new[] { attendanceType };
            }
        }

        private static string[] GetAttendanceMarkers(string attendanceType)
        {
            switch (attendanceType)
            {
                case "Exit": return new[] { "{ExitTime}" };
                case "Late":
                case "LateAbs": return new[] { "{LateMinutes}" };
                case "Entry": return new[] { "{EntryTime}" };
                default: return null;
            }
        }

        private string ResolveAttendanceTemplateWithBlankType(string attendanceType, bool activeOnly)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    if (!TableExists(con))
                        return null;

                    string sql = @"SELECT TemplateName, MessageTemplate FROM SMS_Template
                        WHERE SchoolID = @SchoolID
                        AND TemplateCategory = 'Attendance'
                        AND LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''";
                    if (activeOnly)
                        sql += " AND IsActive = 1";
                    sql += " ORDER BY CreatedDate DESC";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader["TemplateName"]?.ToString() ?? "";
                                string message = reader["MessageTemplate"]?.ToString() ?? "";
                                if (MatchesAttendanceTypeHint(name, message, attendanceType))
                                    return message.Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResolveAttendanceTemplateWithBlankType: {ex.Message}");
            }
            return null;
        }

        private static bool MatchesAttendanceTypeHint(string templateName, string message, string attendanceType)
        {
            string combined = ((templateName ?? "") + " " + (message ?? "")).ToLowerInvariant();
            string msg = message ?? "";

            switch (attendanceType)
            {
                case "Exit":
                    return msg.Contains("{ExitTime}");
                case "Late":
                    return msg.Contains("{LateMinutes}")
                        && !combined.Contains("absent") && !combined.Contains("অনুপস্থিত");
                case "LateAbs":
                    return msg.Contains("{LateMinutes}")
                        && (combined.Contains("absent") || combined.Contains("অনুপস্থিত") || combined.Contains("late abs"));
                case "Absent":
                    return !msg.Contains("{EntryTime}") && !msg.Contains("{ExitTime}") && !msg.Contains("{LateMinutes}")
                        && (combined.Contains("absent") || combined.Contains("অনুপস্থিত") || combined.Contains("অনু"));
                case "Entry":
                    return msg.Contains("{EntryTime}") && !msg.Contains("{LateMinutes}") && !msg.Contains("{ExitTime}");
                default:
                    return false;
            }
        }

        private string ResolveExamTemplateWithBlankType(string resultType, bool activeOnly)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    if (!TableExists(con))
                        return null;

                    string sql = @"SELECT TemplateName, MessageTemplate FROM SMS_Template
                        WHERE SchoolID = @SchoolID
                        AND (TemplateCategory = 'ExamResult' OR TemplateCategory IS NULL)
                        AND LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''";
                    if (activeOnly)
                        sql += " AND IsActive = 1";
                    sql += " ORDER BY CreatedDate DESC";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader["TemplateName"]?.ToString() ?? "";
                                string message = reader["MessageTemplate"]?.ToString() ?? "";
                                if (MatchesExamResultHint(name, message, resultType))
                                    return message.Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResolveExamTemplateWithBlankType: {ex.Message}");
            }
            return null;
        }

        private static bool MatchesExamResultHint(string templateName, string message, string resultType)
        {
            string combined = ((templateName ?? "") + " " + (message ?? "")).ToLowerInvariant();
            if (resultType == "Failed")
            {
                return combined.Contains("ফেল") || combined.Contains("ফেইল")
                    || combined.Contains("fail") || combined.Contains("alas");
            }
            if (resultType == "Passed")
            {
                return combined.Contains("পাস") || combined.Contains("pass")
                    || combined.Contains("congrat") || combined.Contains("success");
            }
            return false;
        }

        private string FindCategoryTemplateByMarkers(string category, bool activeOnly, string[] preferTypes, params string[] markers)
        {
            if (markers == null || markers.Length == 0)
                return null;

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    if (!TableExists(con) || !ColumnExists(con, "TemplateCategory"))
                        return null;

                    var sql = new System.Text.StringBuilder(@"
                        SELECT TOP 1 MessageTemplate FROM SMS_Template
                        WHERE SchoolID = @SchoolID AND TemplateCategory = @Category");
                    if (activeOnly)
                        sql.Append(" AND IsActive = 1");

                    if (preferTypes != null && preferTypes.Length > 0)
                    {
                        sql.Append(" AND (");
                        for (int i = 0; i < preferTypes.Length; i++)
                        {
                            if (i > 0) sql.Append(" OR ");
                            sql.Append("TemplateType = @PT").Append(i);
                        }
                        sql.Append(" OR LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''");
                        sql.Append(")");
                    }

                    sql.Append(" AND (");
                    for (int i = 0; i < markers.Length; i++)
                    {
                        if (i > 0) sql.Append(" OR ");
                        sql.Append("MessageTemplate LIKE @M").Append(i);
                    }
                    sql.Append(") ORDER BY CASE TemplateType ");
                    for (int i = 0; i < preferTypes.Length; i++)
                        sql.Append("WHEN @PT").Append(i).Append(" THEN ").Append(i).Append(' ');
                    sql.Append("ELSE 99 END, CreatedDate DESC");

                    using (var cmd = new SqlCommand(sql.ToString(), con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                        cmd.Parameters.AddWithValue("@Category", category);
                        for (int i = 0; i < markers.Length; i++)
                            cmd.Parameters.AddWithValue("@M" + i, "%" + markers[i] + "%");
                        for (int i = 0; i < preferTypes.Length; i++)
                            cmd.Parameters.AddWithValue("@PT" + i, preferTypes[i]);

                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindCategoryTemplateByMarkers: {ex.Message}");
                return null;
            }
        }

        private static string[] GetLegacyTypeFallbacks(string templateType)
        {
            switch (templateType)
            {
                case "DonorDue": return new[] { "DonorReminder" };
                case "DonorPayment": return new[] { "DonorThankYou" };
                case "Payment": return new[] { "PaymentReminder" };
                case "Due": return new[] { "DueReminder" };
                case "Entry": return new[] { "Present" };
                default: return new string[0];
            }
        }

        private static bool TableExists(SqlConnection con)
        {
            using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SMS_Template')
                    SELECT 1 ELSE SELECT 0", con))
            {
                return (int)cmd.ExecuteScalar() == 1;
            }
        }

        private static bool ColumnExists(SqlConnection con, string columnName)
        {
            using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = @Column)
                    SELECT 1 ELSE SELECT 0", con))
            {
                cmd.Parameters.AddWithValue("@Column", columnName);
                return (int)cmd.ExecuteScalar() == 1;
            }
        }

        private string QueryTemplate(SqlConnection con, bool hasCategoryColumn, string category, string templateType, bool activeOnly)
        {
            var sql = new System.Text.StringBuilder(@"
                SELECT TOP 1 MessageTemplate FROM SMS_Template
                WHERE SchoolID = @SchoolID AND TemplateType = @TemplateType");
            if (activeOnly)
                sql.Append(" AND IsActive = 1");
            if (hasCategoryColumn && !string.IsNullOrEmpty(category))
                sql.Append(" AND TemplateCategory = @Category");
            sql.Append(" ORDER BY CreatedDate DESC");

            using (var cmd = new SqlCommand(sql.ToString(), con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
                cmd.Parameters.AddWithValue("@TemplateType", templateType);
                if (hasCategoryColumn && !string.IsNullOrEmpty(category))
                    cmd.Parameters.AddWithValue("@Category", category);

                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }

        public static string GetTemplateForSchool(int schoolId, string category, string templateType)
        {
            return new SMS_Template_Helper(schoolId).GetTemplate(category, templateType);
        }

        /// <summary>
        /// Current due only (EndDate passed) — excludes future/upcoming fees. Same as Payment Collection banner.
        /// </summary>
        public static decimal GetStudentCurrentDue(string studentId, int schoolId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(@"SELECT ISNULL(SUM(
                    ISNULL(po.Amount,0)+ISNULL(po.LateFee,0)-ISNULL(po.Discount,0)-ISNULL(po.PaidAmount,0)-ISNULL(po.LateFee_Discount,0)
                    ),0)
                    FROM Income_PayOrder po
                    INNER JOIN Student st ON po.StudentID = st.StudentID AND st.ID = @ID AND st.SchoolID = @SchID
                    WHERE po.SchoolID = @SchID AND po.Status = 'Due' AND po.EndDate <= GETDATE()", con))
                {
                    cmd.Parameters.AddWithValue("@ID", studentId);
                    cmd.Parameters.AddWithValue("@SchID", schoolId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Payment SMS amounts: whole numbers without decimals (1200 not 1200.00).
        /// </summary>
        public static string FormatPaymentAmount(decimal amount)
        {
            return amount == decimal.Truncate(amount)
                ? decimal.Truncate(amount).ToString("0")
                : amount.ToString("0.00");
        }

        public static string FormatPaymentAmount(double amount)
        {
            return FormatPaymentAmount(Convert.ToDecimal(amount));
        }

        /// <summary>
        /// Generate Attendance Entry SMS
        /// </summary>
public string GenerateEntrySMS(string studentName, string studentId, string schoolName, DateTime entryTime, DateTime date, string className = "", string roll = "")
        {
            string template = ResolveAttendanceTemplate("Entry");

 if (string.IsNullOrEmpty(template))
            {
                // Default template if no custom template found
       template = "Respected Guardian, {StudentName} has safely entered in {SchoolName} at {EntryTime}. Date: {Date}";
            }

   return template
                .Replace("{StudentName}", studentName)
       .Replace("{ID}", studentId)
   .Replace("{SchoolName}", schoolName)
        .Replace("{EntryTime}", entryTime.ToString("h:mm tt"))
    .Replace("{Date}", date.ToString("d MMM yyyy"))
    .Replace("{Class}", className)
     .Replace("{Roll}", roll);
        }

        /// <summary>
        /// Generate Attendance Exit SMS
        /// </summary>
 public string GenerateExitSMS(string studentName, string studentId, string schoolName, DateTime exitTime, DateTime date, string className = "", string roll = "")
    {
            string template = ResolveAttendanceTemplate("Exit");

  if (string.IsNullOrEmpty(template))
            {
      // Default template
            template = "Respected Guardian, {StudentName} has left {SchoolName} at {ExitTime}. Date: {Date}";
  }

        return template
                .Replace("{StudentName}", studentName)
    .Replace("{ID}", studentId)
 .Replace("{SchoolName}", schoolName)
          .Replace("{ExitTime}", exitTime.ToString("h:mm tt"))
        .Replace("{Date}", date.ToString("d MMM yyyy"))
  .Replace("{Class}", className)
      .Replace("{Roll}", roll);
        }

        /// <summary>
        /// Generate Late Entry SMS
   /// </summary>
        public string GenerateLateSMS(string studentName, string studentId, string schoolName, DateTime entryTime, int lateMinutes, DateTime date, string className = "", string roll = "")
        {
       string template = ResolveAttendanceTemplate("Late");

      if (string.IsNullOrEmpty(template))
   {
      // Default template
   template = "Respected Guardian, {StudentName} (ID: {ID}) arrived {LateMinutes} minutes late at {SchoolName}. Entry Time: {EntryTime}. Date: {Date}";
   }

            return template
         .Replace("{StudentName}", studentName)
           .Replace("{ID}", studentId)
            .Replace("{SchoolName}", schoolName)
           .Replace("{EntryTime}", entryTime.ToString("h:mm tt"))
           .Replace("{LateMinutes}", lateMinutes.ToString())
                .Replace("{Date}", date.ToString("d MMM yyyy"))
     .Replace("{Class}", className)
     .Replace("{Roll}", roll);
        }

        /// <summary>
        /// Generate Absent SMS
   /// </summary>
        public string GenerateAbsentSMS(string studentName, string studentId, string schoolName, DateTime date, string className = "", string roll = "")
        {
 string template = ResolveAttendanceTemplate("Absent");

            if (string.IsNullOrEmpty(template))
    {
     // Default template
           template = "Respected Guardian, {StudentName} (ID: {ID}, Class: {Class}, Roll: {Roll}) is absent from {SchoolName} today ({Date}). Please send regularly.";
}

       return template
            .Replace("{StudentName}", studentName)
     .Replace("{ID}", studentId)
      .Replace("{SchoolName}", schoolName)
            .Replace("{Date}", date.ToString("d MMM yyyy"))
           .Replace("{Class}", className)
          .Replace("{Roll}", roll);
        }

        /// <summary>
     /// Generate Late Absent SMS (Late + counted as absent)
        /// </summary>
   public string GenerateLateAbsSMS(string studentName, string studentId, string schoolName, DateTime entryTime, int lateMinutes, DateTime date, string className = "", string roll = "")
        {
          string template = ResolveAttendanceTemplate("LateAbs");

            if (string.IsNullOrEmpty(template))
  {
      // Default template
        template = "Respected Guardian, {StudentName} arrived {LateMinutes} min late (counted as Absent) at {SchoolName}. Entry: {EntryTime}. Date: {Date}";
            }

         return template
         .Replace("{StudentName}", studentName)
          .Replace("{ID}", studentId)
                .Replace("{SchoolName}", schoolName)
        .Replace("{EntryTime}", entryTime.ToString("h:mm tt"))
  .Replace("{LateMinutes}", lateMinutes.ToString())
    .Replace("{Date}", date.ToString("d MMM yyyy"))
          .Replace("{Class}", className)
       .Replace("{Roll}", roll);
        }

    /// <summary>
        /// Generate Present SMS (Regular attendance confirmation)
        /// </summary>
        public string GeneratePresentSMS(string studentName, string studentId, string schoolName, DateTime date, string className = "", string roll = "")
        {
         string template = ResolveAttendanceTemplate("Present");
         if (string.IsNullOrEmpty(template))
            template = ResolveAttendanceTemplate("Entry");

          if (string.IsNullOrEmpty(template))
     {
   template = "Respected Guardian, {StudentName} (ID: {ID}) has entered {SchoolName} today ({Date}).";
            }

            return template
     .Replace("{StudentName}", studentName)
       .Replace("{ID}", studentId)
  .Replace("{SchoolName}", schoolName)
         .Replace("{Date}", date.ToString("d MMM yyyy"))
  .Replace("{Class}", className)
       .Replace("{Roll}", roll);
        }

        /// <summary>
        /// Get student additional info (Class, Roll) for SMS
      /// </summary>
        public (string className, string roll) GetStudentClassInfo(int studentId)
      {
            try
            {
            using (SqlConnection con = new SqlConnection(_connectionString))
     {
   con.Open();
             SqlCommand cmd = new SqlCommand(@"
        SELECT CreateClass.Class, StudentsClass.RollNo
            FROM StudentsClass 
       INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
     INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
         WHERE StudentsClass.StudentID = @StudentID 
       AND StudentsClass.SchoolID = @SchoolID
     AND Education_Year.Status = 'True'", con);

          cmd.Parameters.AddWithValue("@StudentID", studentId);
          cmd.Parameters.AddWithValue("@SchoolID", _schoolId);

                    SqlDataReader reader = cmd.ExecuteReader();
    if (reader.Read())
     {
            return (reader["Class"].ToString(), reader["RollNo"].ToString());
           }
   }
   }
 catch (Exception ex)
       {
            System.Diagnostics.Debug.WriteLine($"Error getting student class info: {ex.Message}");
            }

       return ("", "");
        }
    }
}
