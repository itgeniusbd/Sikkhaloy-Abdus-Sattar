using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace EDUCATION.COM.Student
{
 public partial class Exam_Routine : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
  {
if (!IsPostBack)
      {
          LoadExamRoutines();
          }
   }

  // Helper method to format time compactly (e.g., "10:00 AM - 1:00 PM" → "10:00-1:00")
  private string FormatTimeCompact(string examTime)
   {
            if (string.IsNullOrEmpty(examTime))
      return "";

 // Pattern: "10:00 AM - 1:00 PM" → "10:00-1:00"
     // Remove AM/PM and extra spaces
            var compactTime = Regex.Replace(examTime, @"\s*(AM|PM|am|pm)\s*", "", RegexOptions.IgnoreCase);
    compactTime = compactTime.Replace(" - ", "-").Replace("  ", " ").Trim();
   
            return compactTime;
     }

  // Helper method to calculate duration from time range (e.g., "10:00 AM - 1:00 PM" → "৩ ঘন্টা")
  private string CalculateDuration(string examTime)
    {
        if (string.IsNullOrEmpty(examTime) || !examTime.Contains("-"))
   return "";

        try
 {
            // Parse start and end time
     var parts = examTime.Split(new[] { '-' }, 2);
   if (parts.Length != 2)
    return "";

          string startTimeStr = parts[0].Trim();
       string endTimeStr = parts[1].Trim();

      // Parse to DateTime
          DateTime startTime = DateTime.ParseExact(startTimeStr, 
             new[] { "h:mm tt", "hh:mm tt", "H:mm", "HH:mm" },
System.Globalization.CultureInfo.InvariantCulture,
           System.Globalization.DateTimeStyles.None);

 DateTime endTime = DateTime.ParseExact(endTimeStr,
   new[] { "h:mm tt", "hh:mm tt", "H:mm", "HH:mm" },
          System.Globalization.CultureInfo.InvariantCulture,
      System.Globalization.DateTimeStyles.None);

            // Calculate duration
          TimeSpan duration = endTime - startTime;
        
  // Handle negative duration (crossing midnight)
        if (duration.TotalMinutes < 0)
           duration = duration.Add(TimeSpan.FromHours(24));

   // Format in Bengali
            int hours = (int)duration.TotalHours;
  int minutes = duration.Minutes;

            string[] bengaliDigits = { "০", "১", "২", "৩", "৪", "৫", "৬", "৭", "৮", "৯" };
        
       string result = "";
            if (hours > 0)
       {
   string hourStr = ConvertToBengaliNumber(hours, bengaliDigits);
          result = hourStr + " ঘন্টা";
 }
            
         if (minutes > 0)
   {
    string minStr = ConvertToBengaliNumber(minutes, bengaliDigits);
           if (!string.IsNullOrEmpty(result))
    result += " ";
  result += minStr + " মিনিট";
        }

     return result;
        }
     catch
     {
            return "";
        }
    }

    // Helper to convert number to Bengali
    private string ConvertToBengaliNumber(int number, string[] bengaliDigits)
    {
 string numStr = number.ToString();
        string result = "";
   foreach (char c in numStr)
        {
          result += bengaliDigits[c - '0'];
        }
        return result;
    }

  private void LoadExamRoutines()
{
    string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
 StringBuilder html = new StringBuilder();

  using (SqlConnection con = new SqlConnection(connString))
          {
       // Get all routines for the student's class and CURRENT education year
     string routineQuery = @"
SELECT DISTINCT 
    ers.RoutineID, 
    ers.RoutineName,
    cc.Class,
 ey.EducationYear,
    ers.CreatedDate
FROM Exam_Routine_SavedData ers
INNER JOIN Exam_Routine_ClassColumns erc ON ers.RoutineID = erc.RoutineID
INNER JOIN StudentsClass sc ON erc.ClassID = sc.ClassID
INNER JOIN CreateClass cc ON erc.ClassID = cc.ClassID
INNER JOIN Education_Year ey ON sc.EducationYearID = ey.EducationYearID
WHERE ers.SchoolID = @SchoolID 
    AND sc.StudentClassID = @StudentClassID
    AND sc.EducationYearID = @EducationYearID
    AND (ers.EducationYearID = @EducationYearID OR ers.EducationYearID IS NULL)
ORDER BY ers.CreatedDate DESC";

      SqlCommand cmd = new SqlCommand(routineQuery, con);
      cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
   cmd.Parameters.AddWithValue("@StudentClassID", Session["StudentClassID"]);
          cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);

         con.Open();
    SqlDataReader reader = cmd.ExecuteReader();

bool hasRoutines = false;
   int routineCount = 0;

while (reader.Read())
       {
   hasRoutines = true;
routineCount++;
        
  int routineId = Convert.ToInt32(reader["RoutineID"]);
  string routineName = reader["RoutineName"].ToString();
  string className = reader["Class"].ToString();
  string educationYear = reader["EducationYear"].ToString();

 // Generate HTML for this routine
 html.Append("<div class='print-button'>");
    html.Append("<button type='button' class='btn btn-primary' onclick='printRoutine(" + routineCount + ")'>");
      html.Append("<i class='fa fa-print'></i> প্রিন্ট করুন - " + routineName + "");
     html.Append("</button>");
   html.Append("</div>");

  html.Append("<div id='printableArea" + routineCount + "' class='routine-section'>");

        // Routine header INSIDE printableArea
      html.Append("<div class='routine-header'>");
   html.Append("<div class='routine-title'>" + routineName + "</div>");
     html.Append("<div class='class-info'>শ্রেণী: " + className + " | শিক্ষাবর্ষ: " + educationYear + "</div>");
     html.Append("</div>");
     
   html.Append(GenerateRoutineTable(routineId, con));
 html.Append("</div>");
 }
  reader.Close();

      if (!hasRoutines)
 {
    NoRoutinePanel.Visible = true;
  RoutineDisplayLiteral.Text = "";
      }
     else
  {
   NoRoutinePanel.Visible = false;
     RoutineDisplayLiteral.Text = html.ToString();
           }
  }
    }

  private string GenerateRoutineTable(int routineId, SqlConnection con)
   {
   StringBuilder html = new StringBuilder();

 // Get class columns
        string classQuery = @"
   SELECT ColumnIndex, cc.Class 
   FROM Exam_Routine_ClassColumns erc
   INNER JOIN CreateClass cc ON erc.ClassID = cc.ClassID
WHERE erc.RoutineID = @RoutineID
ORDER BY ColumnIndex";

  SqlCommand classCmd = new SqlCommand(classQuery, con);
   classCmd.Parameters.AddWithValue("@RoutineID", routineId);

  List<string> classNames = new List<string>();
  SqlDataReader classReader = classCmd.ExecuteReader();
     while (classReader.Read())
   {
     classNames.Add(classReader["Class"].ToString());
   }
     classReader.Close();

 // Start table
  html.Append("<table class='routine-table'>");

// Table header
      html.Append("<thead><tr>");
   html.Append("<th>তারিখ</th>");
        html.Append("<th>বার</th>");
   html.Append("<th>সময়<br/><small>(মোট ঘন্টা)</small></th>");
 foreach (string className in classNames)
    {
         html.Append("<th>শ্রেণী: " + className + "</th>");
 }
html.Append("</tr></thead>");

      // Table body
 html.Append("<tbody>");

    // Get rows
   string rowQuery = @"
     SELECT RowIndex, ExamDate, DayName, ExamTime 
    FROM Exam_Routine_Rows 
        WHERE RoutineID = @RoutineID 
    ORDER BY RowIndex";

     SqlCommand rowCmd = new SqlCommand(rowQuery, con);
     rowCmd.Parameters.AddWithValue("@RoutineID", routineId);

   SqlDataReader rowReader = rowCmd.ExecuteReader();
  while (rowReader.Read())
        {
         int rowIndex = Convert.ToInt32(rowReader["RowIndex"]);
    DateTime examDate = rowReader["ExamDate"] != DBNull.Value 
      ? Convert.ToDateTime(rowReader["ExamDate"]) 
  : DateTime.MinValue;
  string dayName = rowReader["DayName"].ToString();
   string examTime = rowReader["ExamTime"].ToString();
  
        // Format time compactly and calculate duration
     string compactTime = FormatTimeCompact(examTime);
        string duration = CalculateDuration(examTime);
 
        string timeDisplay = compactTime;
        if (!string.IsNullOrEmpty(duration))
        {
            timeDisplay += "<br/><small style='color: #d32f2f;'>(" + duration + ")</small>";
        }

html.Append("<tr>");
   html.Append("<td class='date-cell'>");
      html.Append(examDate != DateTime.MinValue ? examDate.ToString("dd/MM/yyyy") : "");
   html.Append("</td>");
 html.Append("<td class='day-cell'>" + dayName + "</td>");
 html.Append("<td class='time-cell'>" + timeDisplay + "</td>");

   // Add cell data
html.Append(GenerateRowCells(routineId, rowIndex, classNames.Count, con));

 html.Append("</tr>");
 }
     rowReader.Close();

  html.Append("</tbody>");
        html.Append("</table>");

     return html.ToString();
  }

 private string GenerateRowCells(int routineId, int rowIndex, int columnCount, SqlConnection con)
  {
   StringBuilder html = new StringBuilder();

    // Get cell data
     string cellQuery = @"
   SELECT ecd.ColumnIndex, s.SubjectName, ecd.SubjectText, ecd.TimeText
     FROM Exam_Routine_CellData ecd
  LEFT JOIN Subject s ON ecd.SubjectID = s.SubjectID
        WHERE ecd.RoutineID = @RoutineID AND ecd.RowIndex = @RowIndex
  ORDER BY ecd.ColumnIndex";

      SqlCommand cellCmd = new SqlCommand(cellQuery, con);
      cellCmd.Parameters.AddWithValue("@RoutineID", routineId);
    cellCmd.Parameters.AddWithValue("@RowIndex", rowIndex);

 Dictionary<int, Tuple<string, string, string>> cellData = new Dictionary<int, Tuple<string, string, string>>();
   SqlDataReader cellReader = cellCmd.ExecuteReader();

   while (cellReader.Read())
 {
  int colIndex = Convert.ToInt32(cellReader["ColumnIndex"]);
   string subjectName = cellReader["SubjectName"]?.ToString() ?? "";
          string subjectText = cellReader["SubjectText"]?.ToString() ?? "";
 string timeText = cellReader["TimeText"]?.ToString() ?? "";
     cellData[colIndex] = new Tuple<string, string, string>(subjectName, subjectText, timeText);
     }
 cellReader.Close();

   // Generate cells
  for (int i = 1; i <= columnCount; i++)
   {
   html.Append("<td>");

   if (cellData.ContainsKey(i))
    {
    var data = cellData[i];

     if (!string.IsNullOrEmpty(data.Item1))
      {
     html.Append("<div style='font-weight: bold; margin-bottom: 5px;'>");
   html.Append(data.Item1);
     html.Append("</div>");
     }

  if (!string.IsNullOrEmpty(data.Item2))
    {
        html.Append("<div style='font-size: 12px; color: #666;'>");
    html.Append(data.Item2);
      html.Append("</div>");
       }

     if (!string.IsNullOrEmpty(data.Item3))
    {
     html.Append("<div style='font-size: 11px; color: #d32f2f; font-weight: bold; margin-top: 3px;'>");
    html.Append(data.Item3);
   html.Append("</div>");
      }
     }

   html.Append("</td>");
        }

  return html.ToString();
     }
    }
}