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
            try
            {
       using (SqlConnection con = new SqlConnection(_connectionString))
              {
              con.Open();

      // Check if TemplateCategory column exists
       SqlCommand checkCmd = new SqlCommand(@"
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = 'TemplateCategory')
  SELECT 1
 ELSE
      SELECT 0", con);

  int columnExists = (int)checkCmd.ExecuteScalar();

   string query = columnExists == 1
           ? @"SELECT TOP 1 MessageTemplate 
    FROM SMS_Template 
  WHERE SchoolID = @SchoolID 
             AND TemplateCategory = @Category 
         AND TemplateType = @TemplateType 
                AND IsActive = 1 
          ORDER BY CreatedDate DESC"
        : @"SELECT TOP 1 MessageTemplate 
     FROM SMS_Template 
WHERE SchoolID = @SchoolID 
             AND TemplateType = @TemplateType 
          AND IsActive = 1 
   ORDER BY CreatedDate DESC";

      SqlCommand cmd = new SqlCommand(query, con);
         cmd.Parameters.AddWithValue("@SchoolID", _schoolId);
             cmd.Parameters.AddWithValue("@TemplateType", templateType);

        if (columnExists == 1)
            {
            cmd.Parameters.AddWithValue("@Category", category);
     }

        object result = cmd.ExecuteScalar();
      return result != null ? result.ToString() : null;
        }
       }
      catch (Exception ex)
          {
      System.Diagnostics.Debug.WriteLine($"Error getting template: {ex.Message}");
      return null;
       }
     }

        /// <summary>
        /// Generate Attendance Entry SMS
        /// </summary>
public string GenerateEntrySMS(string studentName, string studentId, string schoolName, DateTime entryTime, DateTime date, string className = "", string roll = "")
        {
            string template = GetTemplate("Attendance", "Entry");

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
            string template = GetTemplate("Attendance", "Exit");

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
       string template = GetTemplate("Attendance", "Late");

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
 string template = GetTemplate("Attendance", "Absent");

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
          string template = GetTemplate("Attendance", "LateAbs");

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
         string template = GetTemplate("Attendance", "Present");

          if (string.IsNullOrEmpty(template))
     {
 // Default template
   template = "Respected Guardian, {StudentName} (ID: {ID}) is present at {SchoolName} today ({Date}).";
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
