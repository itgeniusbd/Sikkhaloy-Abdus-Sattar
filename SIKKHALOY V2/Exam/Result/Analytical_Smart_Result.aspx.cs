using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Analytical_Smart_Result : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSchoolName();
                UpdateClassExamLabel();
            }
        }

        private void LoadSchoolName()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT SchoolName FROM School WHERE SchoolID = @SchoolID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        var schoolName = cmd.ExecuteScalar()?.ToString();
                        SchoolNameLabel.Text = !string.IsNullOrEmpty(schoolName) ? schoolName : "School Name";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading school name: " + ex.Message);
                SchoolNameLabel.Text = Session["SchoolName"]?.ToString() ?? "School Name";
            }
        }

        private void UpdateClassExamLabel()
        {
            try
            {
                string className = ClassDropDownList.SelectedIndex > 0 ? ClassDropDownList.SelectedItem.Text : "";
                string examName = ExamDropDownList.SelectedIndex > 0 ? ExamDropDownList.SelectedItem.Text : "";
                ClassExamLabel.Text = !string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(examName) 
                    ? $"Class: {className}, Exam: {examName}" : "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating label: " + ex.Message);
            }
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            if (ExamDropDownList.Items.Count > 0)
                ExamDropDownList.SelectedIndex = 0;
            UpdateClassExamLabel();
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassExamLabel();
            if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedValue != "0")
            {
                LoadGradeChartData();
                GenerateSubjectWiseGradeDistribution();
            }
            else
            {
                ClearReportData();
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassExamLabel();
            if (ExamDropDownList.Items.Count > 0)
                ExamDropDownList.ClearSelection();
            ClearReportData();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedValue != "0")
            {
                GenerateDynamicUnsuccessfulStudentsTable();
            }
        }

        private void ClearReportData()
        {
            try
            {
                if (GradeChartLiteral != null) GradeChartLiteral.Text = "";
                if (SubjectWiseGradeLiteral != null) SubjectWiseGradeLiteral.Text = "";
                Literal literalControl = FindControl("DynamicTableLiteral") as Literal ?? FindControlRecursive(this, "DynamicTableLiteral") as Literal;
                if (literalControl != null) literalControl.Text = "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error clearing report data: " + ex.Message);
            }
        }

        private void LoadGradeChartData()
        {
            try
            {
                var gradeData = GetGradeDistribution();
                StringBuilder chartHtml = new StringBuilder();
                foreach (var grade in gradeData)
                {
                    chartHtml.AppendFormat("<div class='grade-chart'><div class='grade-count'>{0}</div><div class='grade-label'>Grade {1}</div></div>", 
                        grade.Value, grade.Key);
                }
                GradeChartLiteral.Text = chartHtml.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading grade chart: " + ex.Message);
            }
        }

        private Dictionary<string, int> GetGradeDistribution()
        {
            var gradeData = new Dictionary<string, int>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT Student_Grade, COUNT(*) as StudentCount FROM Exam_Result_of_Student 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
                        GROUP BY Student_Grade
                        ORDER BY CASE Student_Grade WHEN 'A+' THEN 1 WHEN 'A' THEN 2 WHEN 'A-' THEN 3 WHEN 'B' THEN 4 WHEN 'C' THEN 5 WHEN 'D' THEN 6 WHEN 'F' THEN 7 ELSE 8 END";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                        cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                        cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                gradeData[reader["Student_Grade"]?.ToString() ?? "N/A"] = Convert.ToInt32(reader["StudentCount"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting grade distribution: " + ex.Message);
            }
            return gradeData;
        }

        // NEW METHOD: Generate Subject Wise Grade Distribution
        private void GenerateSubjectWiseGradeDistribution()
        {
            try
            {
                var literalControl = SubjectWiseGradeLiteral;
                if (literalControl == null) return;

                var institutionGrades = GetInstitutionGrades();
                if (institutionGrades.Count == 0)
                {
                    // Get same parameters used in GetInstitutionGrades for display
                    int schoolID = Convert.ToInt32(Session["SchoolID"] ?? "1");
                    int educationYearID = Convert.ToInt32(Session["Edu_Year"] ?? "1");
                    int classID = Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0");
                    int examID = Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0");
                    
                    literalControl.Text = $@"<div class='alert alert-warning' style='margin-top: 15px;'>
                        <strong>?? No grading system found for this institution.</strong><br/>
                        <small>Debug Info: SchoolID={schoolID}, EducationYearID={educationYearID}, ClassID={classID}, ExamID={examID}</small><br/>
                        <small>Please check if grades are recorded in the database for the selected class and exam.</small>
                    </div>";
                    return;
                }

                var subjectGradeData = GetSubjectWiseGradeData(institutionGrades);
                if (subjectGradeData.Count == 0)
                {
                    literalControl.Text = "<div class='alert alert-info' style='margin-top: 15px;'>?? No subject grade data available for the selected class and exam.</div>";
                    return;
                }

                StringBuilder tableHtml = new StringBuilder();
                tableHtml.Append(@"<style>
                    .subject-grade-table {
                        width: 100%;
                        border-collapse: collapse;
                        font-family: 'Arial', sans-serif;
                        background-color: white;
                        font-size: 12px;
                        margin-top: 15px;
                    }
                    .subject-grade-table th, .subject-grade-table td {
                        border: 1px solid #dee2e6;
                        padding: 8px;
                        text-align: center;
                        vertical-align: middle;
                    }
                    .subject-grade-table thead {
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        font-weight: bold;
                    }
                    .subject-grade-table tbody tr:nth-child(even) {
                        background-color: #f8f9fa;
                    }
                    .subject-grade-table tbody tr:hover {
                        background-color: #e3f2fd;
                        transition: background-color 0.2s ease;
                    }
                    .subject-grade-table td:first-child {
                        text-align: left;
                        font-weight: 600;
                        color: #2c3e50;
                    }
                    .grade-cell-high {
                        background-color: #d4edda !important;
                        color: #155724;
                        font-weight: bold;
                    }
                    .grade-cell-medium {
                        background-color: #fff3cd !important;
                        color: #856404;
                    }
                    .grade-cell-low {
                        background-color: #f8d7da !important;
                        color: #721c24;
                        font-weight: bold;
                    }
                    @media print {
                        .subject-grade-table {
                            font-size: 10px;
                        }
                        .subject-grade-table th, .subject-grade-table td {
                            padding: 4px;
                        }
                    }
                </style>");

                tableHtml.Append("<div class='table-responsive'><table class='subject-grade-table'>");
                tableHtml.Append("<thead><tr><th style='min-width: 150px;'>Subject Name</th>");
                foreach (var grade in institutionGrades)
                {
                    tableHtml.AppendFormat("<th>Grade {0}</th>", grade);
                }
                tableHtml.Append("<th style='background-color: #28a745;'>Total Students</th></tr></thead>");

                tableHtml.Append("<tbody>");
                foreach (var subjectData in subjectGradeData)
                {
                    tableHtml.Append("<tr>");
                    tableHtml.AppendFormat("<td>{0}</td>", subjectData.SubjectName);
                    int totalStudents = 0;
                    foreach (var grade in institutionGrades)
                    {
                        int count = subjectData.GradeCounts.ContainsKey(grade) ? subjectData.GradeCounts[grade] : 0;
                        totalStudents += count;
                        string cellClass = count > 20 ? "grade-cell-high" : (count > 5 ? "grade-cell-medium" : (count > 0 && grade == "F" ? "grade-cell-low" : ""));
                        tableHtml.AppendFormat("<td class='{0}'>{1}</td>", cellClass, count > 0 ? count.ToString() : "-");
                    }
                    tableHtml.AppendFormat("<td style='font-weight: bold; background-color: #e9ecef;'>{0}</td></tr>", totalStudents);
                }
                tableHtml.Append("</tbody></table></div>");
                tableHtml.AppendFormat("<div style='margin-top: 10px; text-align: center; color: #6c757d; font-size: 11px;'>");
                tableHtml.AppendFormat("?? Showing grade distribution for <strong>{0}</strong> subjects across <strong>{1}</strong> grade levels</div>", 
                    subjectGradeData.Count, institutionGrades.Count);
                literalControl.Text = tableHtml.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateSubjectWiseGradeDistribution: {ex.Message}");
                if (SubjectWiseGradeLiteral != null)
                {
                    SubjectWiseGradeLiteral.Text = $"<div class='alert alert-danger' style='margin-top: 15px;'>?? Error loading subject wise grade distribution: {ex.Message}</div>";
                }
            }
        }

        private List<string> GetInstitutionGrades()
        {
            var grades = new List<string>();
            try
            {
                int schoolID = Convert.ToInt32(Session["SchoolID"] ?? "1");
                int educationYearID = Convert.ToInt32(Session["Edu_Year"] ?? "1");
                int classID = Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0");
                int examID = Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0");

                System.Diagnostics.Debug.WriteLine($"?? GetInstitutionGrades Parameters: SchoolID={schoolID}, EduYear={educationYearID}, ClassID={classID}, ExamID={examID}");

                // Use the EXACT same query as GetGradeDistribution() to ensure we get the same grades
                var gradeDistribution = GetGradeDistribution();
                
                if (gradeDistribution != null && gradeDistribution.Count > 0)
                {
                    grades = gradeDistribution.Keys.ToList();
                    System.Diagnostics.Debug.WriteLine($"? Got {grades.Count} grades from GetGradeDistribution(): {string.Join(", ", grades)}");
                    return grades;
                }
                
                System.Diagnostics.Debug.WriteLine("?? GetGradeDistribution() returned no grades, trying direct query");

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    
                    // Exact same query as GetGradeDistribution()
                    string query = @"SELECT DISTINCT Student_Grade as Grade FROM Exam_Result_of_Student 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID 
                        AND ClassID = @ClassID AND ExamID = @ExamID
                        AND Student_Grade IS NOT NULL AND LTRIM(RTRIM(Student_Grade)) != ''
                        ORDER BY CASE Student_Grade 
                            WHEN 'A+' THEN 1 WHEN 'A' THEN 2 WHEN 'A-' THEN 3 
                            WHEN 'B' THEN 4 WHEN 'C' THEN 5 WHEN 'D' THEN 6 WHEN 'F' THEN 7 ELSE 8 END";
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                        cmd.Parameters.AddWithValue("@EducationYearID", educationYearID);
                        cmd.Parameters.AddWithValue("@ClassID", classID);
                        cmd.Parameters.AddWithValue("@ExamID", examID);
                        
                        System.Diagnostics.Debug.WriteLine($"?? Executing direct query for grades");
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string grade = reader["Grade"]?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(grade))
                                {
                                    grades.Add(grade);
                                    System.Diagnostics.Debug.WriteLine($"? Found grade: {grade}");
                                }
                            }
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"? Total Found {grades.Count} grades: {string.Join(", ", grades)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error getting institution grades: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            return grades;
        }

        private List<SubjectGradeDistribution> GetSubjectWiseGradeData(List<string> institutionGrades)
        {
            var subjectGradeData = new List<SubjectGradeDistribution>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    
                    // Get all subjects for this exam
                    string subjectQuery = @"SELECT DISTINCT s.SubjectID, s.SubjectName, s.SN FROM Subject s
                        INNER JOIN Exam_Result_of_Subject ers ON s.SubjectID = ers.SubjectID
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                        AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        ORDER BY s.SN, s.SubjectName";
                        
                    using (SqlCommand cmd = new SqlCommand(subjectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                        cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                        cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subjectGradeData.Add(new SubjectGradeDistribution
                                {
                                    SubjectID = Convert.ToInt32(reader["SubjectID"]),
                                    SubjectName = reader["SubjectName"]?.ToString() ?? "",
                                    GradeCounts = new Dictionary<string, int>()
                                });
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Found {subjectGradeData.Count} subjects");

                    // For each subject, get grade counts
                    foreach (var subjectData in subjectGradeData)
                    {
                        // First try: Use SubjectGrades column if available
                        string gradeCountQuery = @"SELECT ers.SubjectGrades, COUNT(*) as GradeCount 
                            FROM Exam_Result_of_Subject ers
                            INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                            WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                            AND ers.SubjectID = @SubjectID AND ISNULL(ers.IS_Add_InExam, 1) = 1 
                            AND ers.SubjectGrades IS NOT NULL AND LTRIM(RTRIM(ers.SubjectGrades)) != ''
                            GROUP BY ers.SubjectGrades";
                        
                        using (SqlCommand cmd = new SqlCommand(gradeCountQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                            cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                            cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                            cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                            cmd.Parameters.AddWithValue("@SubjectID", subjectData.SubjectID);
                            
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string grade = reader["SubjectGrades"]?.ToString()?.Trim();
                                    if (!string.IsNullOrEmpty(grade))
                                    {
                                        subjectData.GradeCounts[grade] = Convert.ToInt32(reader["GradeCount"]);
                                        System.Diagnostics.Debug.WriteLine($"Subject {subjectData.SubjectName}: Grade {grade} = {reader["GradeCount"]}");
                                    }
                                }
                            }
                        }
                        
                        // If no data found in SubjectGrades, calculate from marks using GradeRange
                        if (subjectData.GradeCounts.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"?? No SubjectGrades found for {subjectData.SubjectName}, calculating from marks");
                            
                            // Get all students' marks for this subject and calculate grade based on GradeRange
                            string marksQuery = @"SELECT ers.TotalMarks
                                FROM Exam_Result_of_Subject ers
                                INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                                WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                                AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                                AND ers.SubjectID = @SubjectID AND ISNULL(ers.IS_Add_InExam, 1) = 1
                                AND ers.TotalMarks IS NOT NULL";
                            
                            var studentMarks = new List<decimal>();
                            using (SqlCommand cmd = new SqlCommand(marksQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                                cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                                cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                                cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                                cmd.Parameters.AddWithValue("@SubjectID", subjectData.SubjectID);
                                
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        studentMarks.Add(Convert.ToDecimal(reader["TotalMarks"]));
                                    }
                                }
                            }
                            
                            // Get GradeRange configuration
                            var gradeRanges = GetGradeRanges();
                            
                            // Calculate grade for each student's marks
                            foreach (var marks in studentMarks)
                            {
                                string calculatedGrade = CalculateGradeFromMarks(marks, gradeRanges);
                                if (!string.IsNullOrEmpty(calculatedGrade))
                                {
                                    if (subjectData.GradeCounts.ContainsKey(calculatedGrade))
                                        subjectData.GradeCounts[calculatedGrade]++;
                                    else
                                        subjectData.GradeCounts[calculatedGrade] = 1;
                                }
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Calculated grades for {subjectData.SubjectName}: {string.Join(", ", subjectData.GradeCounts.Select(x => $"{x.Key}={x.Value}"))}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error getting subject wise grade data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            return subjectGradeData;
        }

        // Helper method to get grade ranges from database
        private List<GradeRangeInfo> GetGradeRanges()
        {
            var gradeRanges = new List<GradeRangeInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT Grade, MinMarks, MaxMarks FROM GradeRange 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
                        ORDER BY MinMarks DESC";
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                gradeRanges.Add(new GradeRangeInfo
                                {
                                    Grade = reader["Grade"]?.ToString() ?? "",
                                    MinMarks = Convert.ToDecimal(reader["MinMarks"]),
                                    MaxMarks = Convert.ToDecimal(reader["MaxMarks"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting grade ranges: {ex.Message}");
            }
            return gradeRanges;
        }

        // Helper method to calculate grade from marks
        private string CalculateGradeFromMarks(decimal marks, List<GradeRangeInfo> gradeRanges)
        {
            foreach (var range in gradeRanges)
            {
                if (marks >= range.MinMarks && marks <= range.MaxMarks)
                {
                    return range.Grade;
                }
            }
            return "F"; // Default to F if no range matches
        }

        // Simplified version - You can add full implementation later if needed
        private void GenerateDynamicUnsuccessfulStudentsTable()
        {
            try
            {
                Literal literalControl = FindControl("DynamicTableLiteral") as Literal ?? FindControlRecursive(this, "DynamicTableLiteral") as Literal;
                if (literalControl == null) return;
                
                literalControl.Text = "<div class='alert alert-info'>?? Unsuccessful students table will be shown here.</div>";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error generating unsuccessful students table: " + ex.Message);
            }
        }

        private Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id) return root;
            foreach (Control control in root.Controls)
            {
                Control found = FindControlRecursive(control, id);
                if (found != null) return found;
            }
            return null;
        }

        // Helper Classes
        [Serializable]
        public class SubjectGradeDistribution
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; }
            public Dictionary<string, int> GradeCounts { get; set; }
        }

        [Serializable]
        public class GradeRangeInfo
        {
            public string Grade { get; set; }
            public decimal MinMarks { get; set; }
            public decimal MaxMarks { get; set; }
        }
    }
}
