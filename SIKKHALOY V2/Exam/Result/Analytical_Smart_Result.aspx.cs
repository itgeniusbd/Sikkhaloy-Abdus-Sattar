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
            // Remove duplicates if any exist
            var uniqueItems = new Dictionary<string, ListItem>();
            var itemsToRemove = new List<ListItem>();
            
            foreach (ListItem item in ExamDropDownList.Items)
            {
                if (uniqueItems.ContainsKey(item.Value))
                {
                    // Mark duplicate for removal
                    itemsToRemove.Add(item);
                }
                else
                {
                    uniqueItems[item.Value] = item;
                }
            }
            
            // Remove duplicates
            foreach (var item in itemsToRemove)
            {
                ExamDropDownList.Items.Remove(item);
            }
            
            // Select first item if available
            if (ExamDropDownList.Items.Count > 0)
                ExamDropDownList.SelectedIndex = 0;
                
            UpdateClassExamLabel();
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassExamLabel();
            if (ClassDropDownList.SelectedIndex <= 0 || ExamDropDownList.SelectedIndex <= 0 || ExamDropDownList.SelectedValue == "0")
            {
                ClearReportData();
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassExamLabel();
            
            // Clear exam dropdown to prevent data duplication
            ExamDropDownList.Items.Clear();
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
            ExamDropDownList.DataBind();
            
            ClearReportData();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedValue != "0")
            {
                LoadGradeChartData();
                GenerateSubjectWiseGradeDistribution();
                GenerateDynamicUnsuccessfulStudentsTable();
            }
        }

        private void ClearReportData()
        {
            try
            {
                if (GradeChartLiteral != null) GradeChartLiteral.Text = "";
                if (SubjectWiseGradeLiteral != null) SubjectWiseGradeLiteral.Text = "";
                if (DynamicTableLiteral != null) DynamicTableLiteral.Text = "";
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
                var institutionGrades = GetInstitutionGrades();
                if (institutionGrades.Count == 0 && gradeData.Count > 0)
                    institutionGrades = SortGrades(gradeData.Keys.ToList());

                StringBuilder chartHtml = new StringBuilder();
                foreach (var grade in institutionGrades)
                {
                    int count = GetGradeCount(gradeData, grade);
                    chartHtml.AppendFormat("<div class='grade-chart'><div class='grade-count'>{0}</div><div class='grade-label'>Grade {1}</div></div>",
                        count, grade);
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
                        AND Student_Grade IS NOT NULL AND LTRIM(RTRIM(Student_Grade)) <> ''
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
                        <strong>???? ????? ???? ?????? ???????</strong><br/>
                        <small>????????? ????? ? ???????? ???? Exam_Result_of_Student ?? Exam_Result_of_Subject ?????? ????? ??????? ??? ???? ??????</small><br/>
                        <small>Debug: SchoolID={schoolID}, EducationYearID={educationYearID}, ClassID={classID}, ExamID={examID}</small>
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
                    .total-cell {
                        font-weight: bold;
                        background-color: #e9ecef;
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
               
                tableHtml.Append("<th style='background-color: #17a2b8; color: white;'>Appeared</th>");
                tableHtml.Append("<th style='background-color: #28a745; color: white;'>Total Students</th></tr></thead>");

                tableHtml.Append("<tbody>");
                
                int totalClassStudents = GetTotalClassStudents();
                
                foreach (var subjectData in subjectGradeData)
                {
                    tableHtml.Append("<tr>");
                    tableHtml.AppendFormat("<td>{0}</td>", subjectData.SubjectName);
                    foreach (var grade in institutionGrades)
                    {
                        int count = GetGradeCount(subjectData.GradeCounts, grade);
                        string cellClass = count > 20 ? "grade-cell-high" : (count > 5 ? "grade-cell-medium" : (count > 0 && grade == "F" ? "grade-cell-low" : ""));
                        tableHtml.AppendFormat("<td class='{0}'>{1}</td>", cellClass, count > 0 ? count.ToString() : "-");
                    }
                    
                    int appearedCount = subjectData.GradeCounts.Values.Sum();
                    tableHtml.AppendFormat("<td style='font-weight:bold; background-color:#d1ecf1;'>{0}</td>", appearedCount);
                    tableHtml.AppendFormat("<td class='total-cell'>{0}</td>", subjectData.TotalEnrolled);
                    tableHtml.Append("</tr>");
                }
                tableHtml.Append("</tbody></table></div>");
                tableHtml.AppendFormat("<div style='margin-top: 10px; text-align: center; color: #6c757d; font-size: 11px;'>");
                tableHtml.AppendFormat("?? Showing grade distribution for <strong>{0}</strong> subjects across <strong>{1}</strong> grade levels. ", 
                    subjectGradeData.Count, institutionGrades.Count);
                tableHtml.AppendFormat("Total students in class: <strong>{0}</strong></div>", totalClassStudents);
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
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                int schoolID = Convert.ToInt32(Session["SchoolID"] ?? "1");
                int educationYearID = Convert.ToInt32(Session["Edu_Year"] ?? "1");
                int classID = Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0");
                int examID = Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0");

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    // 1. Institution grading system ? full grade scale for this exam (A+ through F)
                    MergeGrades(grades, seen, LoadDistinctGrades(con, @"SELECT DISTINCT gs.Grades AS Grade
                        FROM Exam_Grading_System gs
                        INNER JOIN Exam_Grading_Assign ga ON gs.GradeNameID = ga.GradeNameID AND gs.SchoolID = ga.SchoolID
                        WHERE ga.SchoolID = @SchoolID AND ga.EducationYearID = @EducationYearID
                        AND ga.ClassID = @ClassID AND ga.ExamID = @ExamID
                        ORDER BY gs.Point DESC",
                        schoolID, educationYearID, classID, examID));

                    // 2. Subject-level grades ? includes B,C,D that may not appear in overall Student_Grade
                    MergeGrades(grades, seen, LoadDistinctGrades(con, @"SELECT DISTINCT ers.SubjectGrades AS Grade
                        FROM Exam_Result_of_Subject ers
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID
                        AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                        AND ers.SubjectGrades IS NOT NULL AND LTRIM(RTRIM(ers.SubjectGrades)) <> ''",
                        schoolID, educationYearID, classID, examID));

                    // 3. Overall student grades for this exam
                    MergeGrades(grades, seen, LoadDistinctGrades(con, @"SELECT DISTINCT Student_Grade AS Grade
                        FROM Exam_Result_of_Student
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
                        AND ClassID = @ClassID AND ExamID = @ExamID
                        AND Student_Grade IS NOT NULL AND LTRIM(RTRIM(Student_Grade)) <> ''",
                        schoolID, educationYearID, classID, examID));
                }

                grades = SortGrades(grades);
                System.Diagnostics.Debug.WriteLine($"? GetInstitutionGrades: {string.Join(", ", grades)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error getting institution grades: {ex.Message}");
            }
            return grades;
        }

        private static void MergeGrades(List<string> target, HashSet<string> seen, IEnumerable<string> newGrades)
        {
            foreach (var grade in newGrades)
            {
                if (!string.IsNullOrWhiteSpace(grade) && seen.Add(grade.Trim()))
                    target.Add(grade.Trim());
            }
        }

        private static List<string> SortGrades(List<string> grades)
        {
            return grades.OrderBy(g => GetGradeSortOrder(g)).ThenBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static int GetGradeSortOrder(string grade)
        {
            switch (grade?.Trim().ToUpperInvariant())
            {
                case "A+": return 1;
                case "A": return 2;
                case "A-": return 3;
                case "B": return 4;
                case "C": return 5;
                case "D": return 6;
                case "F": return 7;
                default: return 8;
            }
        }

        private static int GetGradeCount(Dictionary<string, int> gradeCounts, string grade)
        {
            if (gradeCounts == null || string.IsNullOrEmpty(grade))
                return 0;
            if (gradeCounts.TryGetValue(grade, out int count))
                return count;
            foreach (var kvp in gradeCounts)
            {
                if (string.Equals(kvp.Key, grade, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            return 0;
        }

        private static List<string> LoadDistinctGrades(SqlConnection con, string query, int schoolID, int educationYearID, int classID, int examID)
        {
            var result = new List<string>();
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@EducationYearID", educationYearID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    cmd.Parameters.AddWithValue("@ExamID", examID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string grade = reader["Grade"]?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(grade))
                                result.Add(grade);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? LoadDistinctGrades failed: {ex.Message}");
            }
            return result;
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

                    // For each subject, get grade counts using BOTH SubjectGrades and Student overall grade
                    foreach (var subjectData in subjectGradeData)
                    {
                        // Strategy 1: Try to use SubjectGrades column first
                        string gradeCountQuery = @"SELECT ers.SubjectGrades, COUNT(*) as GradeCount 
                            FROM Exam_Result_of_Subject ers
                            INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                            WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                            AND ers.SubjectID = @SubjectID
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
                                    }
                                }
                            }
                        }

                        int countFromSubjectGrades = subjectData.GradeCounts.Values.Sum();
                        System.Diagnostics.Debug.WriteLine($"Subject {subjectData.SubjectName}: Found {countFromSubjectGrades} from SubjectGrades");
                        
                        // Strategy 2: Get total student count for this subject to compare
                        string totalStudentQuery = @"SELECT COUNT(DISTINCT erst.StudentID) as TotalStudents
                            FROM Exam_Result_of_Subject ers
                            INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                            WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                            AND ers.SubjectID = @SubjectID";
                        
                        int totalStudents = 0;
                        using (SqlCommand cmd = new SqlCommand(totalStudentQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                            cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                            cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                            cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                            cmd.Parameters.AddWithValue("@SubjectID", subjectData.SubjectID);
                            
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                totalStudents = Convert.ToInt32(result);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Subject {subjectData.SubjectName}: Total students in database = {totalStudents}");

                        // Strategy 3: If SubjectGrades doesn't cover all students, use Student_Grade as fallback for missing students
                        if (countFromSubjectGrades < totalStudents)
                        {
                            System.Diagnostics.Debug.WriteLine($"?? Subject {subjectData.SubjectName}: Missing {totalStudents - countFromSubjectGrades} students. Using Student_Grade for missing records.");
                            
                            // Get grades for students who don't have SubjectGrades
                            string missingGradesQuery = @"SELECT erst.Student_Grade, COUNT(*) as GradeCount
                                FROM Exam_Result_of_Subject ers
                                INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                                WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                                AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
                                AND ers.SubjectID = @SubjectID
                                AND (ers.SubjectGrades IS NULL OR LTRIM(RTRIM(ers.SubjectGrades)) = '')
                                AND erst.Student_Grade IS NOT NULL AND LTRIM(RTRIM(erst.Student_Grade)) != ''
                                GROUP BY erst.Student_Grade";
                            
                            using (SqlCommand cmd = new SqlCommand(missingGradesQuery, con))
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
                                        string grade = reader["Student_Grade"]?.ToString()?.Trim();
                                        int count = Convert.ToInt32(reader["GradeCount"]);
                                        
                                        if (!string.IsNullOrEmpty(grade))
                                        {
                                            if (subjectData.GradeCounts.ContainsKey(grade))
                                                subjectData.GradeCounts[grade] += count;
                                            else
                                                subjectData.GradeCounts[grade] = count;
                                            
                                            System.Diagnostics.Debug.WriteLine($"  Added {count} students with grade {grade} from Student_Grade");
                                        }
                                    }
                                }
                            }
                        }

                        int finalCount = subjectData.GradeCounts.Values.Sum();
                        subjectData.TotalEnrolled = totalStudents;
                        System.Diagnostics.Debug.WriteLine($"? Subject {subjectData.SubjectName}: Final total = {finalCount} (Enrolled: {totalStudents})");
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

        // Helper method to get total students in the selected class for this exam
        private int GetTotalClassStudents()
        {
            int totalStudents = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT COUNT(DISTINCT erst.StudentID) as TotalStudents
                        FROM Exam_Result_of_Student erst
                        WHERE erst.SchoolID = @SchoolID 
                        AND erst.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID 
                        AND erst.ExamID = @ExamID";
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                        cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                        cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                        
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            totalStudents = Convert.ToInt32(result);
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine($"?? Total class students: {totalStudents}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting total class students: {ex.Message}");
            }
            return totalStudents;
        }

        // Simplified version - You can add full implementation later if needed
        private void GenerateDynamicUnsuccessfulStudentsTable()
        {
            try
            {
                if (DynamicTableLiteral == null)
                {
                    System.Diagnostics.Debug.WriteLine("? DynamicTableLiteral control not found!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("?? Starting GenerateDynamicUnsuccessfulStudentsTable...");

                // Get all subjects for this exam
                var subjects = GetSubjectsForExam();
                if (subjects.Count == 0)
                {
                    DynamicTableLiteral.Text = "<div class='alert alert-info'>?? No subjects found for the selected exam.</div>";
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"?? Found {subjects.Count} subjects");

                // Get unsuccessful students with their failed subjects
                var unsuccessfulStudents = GetUnsuccessfulStudentsWithFailedSubjects(subjects);
                if (unsuccessfulStudents.Count == 0)
                {
                    DynamicTableLiteral.Text = "<div class='alert alert-success' style='margin-top: 15px;'><strong>? Great News!</strong><br/>All students have passed in all subjects. No unsuccessful students found.</div>";
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"?? Found {unsuccessfulStudents.Count} unsuccessful students");

                // Build the dynamic table HTML
                StringBuilder tableHtml = new StringBuilder();

                tableHtml.Append(@"<div class='unsuccessful-table-wrapper table-wrapper'>
                    <div class='unsuccessful-legend d-print-none'>
                        <span class='legend-item'><span class='legend-swatch legend-pass'></span> Pass</span>
                        <span class='legend-item'><span class='legend-swatch legend-fail'></span> Fail</span>
                        <span class='legend-item'><span class='legend-swatch legend-absent'></span> Absent</span>
                        <span class='legend-item legend-note'><strong>OM</strong> = Obtained Marks &nbsp;|&nbsp; <strong>Lack</strong> = Shortage from pass marks (33%)</span>
                    </div>
                    <table class='dynamic-unsuccessful-table'>");

                // Table header
                tableHtml.Append("<thead>");
                tableHtml.Append("<tr class='header-row'>");
                tableHtml.Append("<th rowspan='2' class='fixed-col col-sl'>SL</th>");
                tableHtml.Append("<th rowspan='2' class='fixed-col col-id'>ID</th>");
                tableHtml.Append("<th rowspan='2' class='fixed-col col-name'>Name</th>");
                tableHtml.Append("<th rowspan='2' class='fixed-col col-roll'>Roll</th>");
                tableHtml.Append("<th rowspan='2' class='fixed-col col-grade'>Grd</th>");
                tableHtml.Append("<th rowspan='2' class='fixed-col failed-col' title='Failed Subjects'>Fail</th>");

                for (int i = 0; i < subjects.Count; i++)
                {
                    string groupClass = "subject-group-" + (i % 5);
                    string subjectTitle = System.Web.HttpUtility.HtmlEncode(subjects[i].SubjectName);
                    string shortLabel = GetCompactSubjectLabel(subjects[i].SubjectName);
                    tableHtml.AppendFormat("<th colspan='2' class='subject-group-header {0}' title='{1}'>{2}</th>",
                        groupClass, subjectTitle, System.Web.HttpUtility.HtmlEncode(shortLabel));
                }
                tableHtml.Append("</tr>");

                tableHtml.Append("<tr class='om-lack-header-row'>");
                for (int i = 0; i < subjects.Count; i++)
                {
                    string groupClass = "subject-group-" + (i % 5);
                    tableHtml.AppendFormat("<th class='subject-om-header subject-group-start {0}'>OM</th>", groupClass);
                    tableHtml.AppendFormat("<th class='subject-lack-header {0}'>Lack</th>", groupClass);
                }
                tableHtml.Append("</tr></thead>");

                // Table body
                tableHtml.Append("<tbody>");
                int serialNo = 1;

                foreach (var student in unsuccessfulStudents)
                {
                    tableHtml.Append("<tr>");
                    tableHtml.AppendFormat("<td class='fixed-col'>{0}</td>", serialNo++);
                    tableHtml.AppendFormat("<td class='fixed-col student-id-cell'>{0}</td>", student.StudentID);
                    tableHtml.AppendFormat("<td class='fixed-col student-name-cell' title='{0}'>{0}</td>",
                        System.Web.HttpUtility.HtmlEncode(student.StudentName));
                    tableHtml.AppendFormat("<td class='fixed-col roll-col'>{0}</td>", student.Roll);
                    tableHtml.AppendFormat("<td class='fixed-col grade-fail-cell'>{0}</td>", student.Grade);
                    tableHtml.AppendFormat("<td class='fixed-col failed-count-cell'>{0}</td>", student.FailedCount);

                    for (int i = 0; i < subjects.Count; i++)
                    {
                        var subject = subjects[i];
                        string groupClass = "subject-group-" + (i % 5);
                        string omClass = "subject-om-cell subject-group-start " + groupClass;
                        string lackClass = "subject-lack-cell " + groupClass;

                        if (student.SubjectResults.ContainsKey(subject.SubjectID))
                        {
                            var result = student.SubjectResults[subject.SubjectID];

                            if (result.IsAbsent)
                            {
                                tableHtml.AppendFormat("<td class='{0} absent-cell'>ABS</td>", omClass);
                                tableHtml.AppendFormat("<td class='{0} absent-cell'>-</td>", lackClass);
                            }
                            else if (result.IsFailed)
                            {
                                tableHtml.AppendFormat("<td class='{0} om-cell fail-cell'>{1}</td>", omClass, result.ObtainedMarks);
                                tableHtml.AppendFormat("<td class='{0} lack-cell fail-cell'>{1}</td>", lackClass, result.Shortage);
                            }
                            else
                            {
                                tableHtml.AppendFormat("<td class='{0} pass-cell'>{1}</td>", omClass, result.ObtainedMarks);
                                tableHtml.AppendFormat("<td class='{0} pass-cell'>-</td>", lackClass);
                            }
                        }
                        else
                        {
                            tableHtml.AppendFormat("<td class='{0} no-data-cell'>-</td>", omClass);
                            tableHtml.AppendFormat("<td class='{0} no-data-cell'>-</td>", lackClass);
                        }
                    }

                    tableHtml.Append("</tr>");
                }

                tableHtml.Append("</tbody></table></div>");

                // Summary footer
                tableHtml.AppendFormat("<div class='unsuccessful-table-footer'>");
                tableHtml.AppendFormat("Showing <strong>{0}</strong> unsuccessful students across <strong>{1}</strong> subjects.",
                    unsuccessfulStudents.Count, subjects.Count);
                tableHtml.Append("</div>");

                DynamicTableLiteral.Text = tableHtml.ToString();
                System.Diagnostics.Debug.WriteLine("? Unsuccessful students table generated successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error generating unsuccessful students table: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (DynamicTableLiteral != null)
                {
                    DynamicTableLiteral.Text = $"<div class='alert alert-danger' style='margin-top: 15px;'>?? Error loading unsuccessful students: {ex.Message}</div>";
                }
            }
        }

        private static string GetCompactSubjectLabel(string subjectName)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
                return "";

            string name = subjectName.Trim().ToUpperInvariant();

            if (name.Contains("BANGLA") && (name.Contains("1") || name.Contains("FIRST")))
                return "BN-1";
            if (name.Contains("BANGLA") && (name.Contains("2") || name.Contains("SECOND")))
                return "BN-2";
            if (name.Contains("ENGLISH") && (name.Contains("1") || name.Contains("FIRST")))
                return "EN-1";
            if (name.Contains("ENGLISH") && (name.Contains("2") || name.Contains("SECOND")))
                return "EN-2";
            if (name.Contains("MATHEMATIC"))
                return "MATH";
            if (name == "G.K" || name.Contains("G.K") || name.Contains("GENERAL KNOWLEDGE"))
                return "G.K";
            if (name.Contains("RELIGION") || name.Contains("ISLAM") || name.Contains("HINDU"))
                return "REL";
            if (name.Contains("SCIENCE") && !name.Contains("SOCIAL"))
                return "SCI";
            if (name.Contains("SOCIAL"))
                return "SOC";
            if (name.Contains("BANGLADESH") && name.Contains("GLOBAL"))
                return "BGS";
            if (name.Contains("ICT") || name.Contains("COMPUTER"))
                return "ICT";
            if (name.Contains("PHYSICS"))
                return "PHY";
            if (name.Contains("CHEMISTRY"))
                return "CHM";
            if (name.Contains("BIOLOGY"))
                return "BIO";

            if (name.Length <= 7)
                return name;

            var words = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2)
            {
                return string.Join("-", words.Select(w => w.Length > 3 ? w.Substring(0, 3) : w));
            }

            return name.Substring(0, 7);
        }

        private List<SubjectInfo> GetSubjectsForExam()
        {
            var subjects = new List<SubjectInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT DISTINCT s.SubjectID, s.SubjectName, s.SN 
                        FROM Subject s
                        INNER JOIN Exam_Result_of_Subject ers ON s.SubjectID = ers.SubjectID
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        WHERE ers.SchoolID = @SchoolID 
                            AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID 
                            AND erst.ExamID = @ExamID
                            AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        ORDER BY s.SN, s.SubjectName";
                    
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
                                subjects.Add(new SubjectInfo
                                {
                                    SubjectID = Convert.ToInt32(reader["SubjectID"]),
                                    SubjectName = reader["SubjectName"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error getting subjects: {ex.Message}");
            }
            return subjects;
        }

        private List<UnsuccessfulStudentInfo> GetUnsuccessfulStudentsWithFailedSubjects(List<SubjectInfo> subjects)
        {
            var unsuccessfulStudents = new List<UnsuccessfulStudentInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    
                    // Get all students who have failed in at least one subject
                    string studentQuery = @"
                        SELECT DISTINCT 
                            s.ID,
                            s.StudentsName,
                            ISNULL(sc.RollNo, '') as Roll,
                            erst.Student_Grade,
                            erst.StudentResultID
                        FROM Exam_Result_of_Student erst
                        INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                        INNER JOIN Student s ON sc.StudentID = s.StudentID
                        WHERE erst.SchoolID = @SchoolID 
                            AND erst.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID 
                            AND erst.ExamID = @ExamID
                            
                            AND EXISTS (
                                SELECT 1 FROM Exam_Result_of_Subject ers
                                WHERE ers.StudentResultID = erst.StudentResultID
                                AND ISNULL(ers.IS_Add_InExam, 1) = 1
                                AND (
                                    -- Check if failed based on SubjectGrades
                                    UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F'
                                    OR 
                                    -- Check if failed based on PassStatus_Subject
                                    UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                    OR 
                                    -- Check if absent
                                    UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                                    OR 
                                    -- Check if marks are zero or empty
                                    ers.ObtainedMark_ofSubject = '0'
                                    OR 
                                    LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) = ''
                                    OR 
                                    -- Check if marks are below 33% (dynamic pass marks)
                                    (
                                        ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                        AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                        AND LEN(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) > 0
                                        AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                        AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                        AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                        AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                    )
                                )
                            )
                        ORDER BY s.StudentsName";
                    
                    System.Diagnostics.Debug.WriteLine($"?? Querying unsuccessful students with parameters:");
                    System.Diagnostics.Debug.WriteLine($"   SchoolID: {Session["SchoolID"]}");
                    System.Diagnostics.Debug.WriteLine($"   EducationYearID: {Session["Edu_Year"]}");
                    System.Diagnostics.Debug.WriteLine($"   ClassID: {ClassDropDownList?.SelectedValue}");
                    System.Diagnostics.Debug.WriteLine($"   ExamID: {ExamDropDownList?.SelectedValue}");
                    
                    using (SqlCommand cmd = new SqlCommand(studentQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                        cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                        cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var student = new UnsuccessfulStudentInfo
                                {
                                    StudentID = Convert.ToInt32(reader["ID"]),
                                    StudentName = reader["StudentsName"]?.ToString() ?? "",
                                    Roll = reader["Roll"]?.ToString() ?? "",
                                    Grade = reader["Student_Grade"]?.ToString() ?? "",
                                    StudentResultID = Convert.ToInt32(reader["StudentResultID"]),
                                    FailedCount = 0,
                                    SubjectResults = new Dictionary<int, SubjectResultInfo>()
                                };
                                unsuccessfulStudents.Add(student);
                                System.Diagnostics.Debug.WriteLine($"   ? Found unsuccessful student: {student.StudentName} (ID: {student.StudentID})");
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"?? Total unsuccessful students found: {unsuccessfulStudents.Count}");

                    // For each student, get their subject results
                    foreach (var student in unsuccessfulStudents)
                    {
                        string subjectResultQuery = @"
                            SELECT 
                                ers.SubjectID,
                                ers.ObtainedMark_ofSubject,
                                ers.TotalMark_ofSubject,
                                ers.SubjectGrades,
                                ers.PassStatus_Subject
                            FROM Exam_Result_of_Subject ers
                            WHERE ers.StudentResultID = @StudentResultID
                                AND ISNULL(ers.IS_Add_InExam, 1) = 1";
                        
                        using (SqlCommand cmd = new SqlCommand(subjectResultQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@StudentResultID", student.StudentResultID);
                            
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int subjectID = Convert.ToInt32(reader["SubjectID"]);
                                    string obtainedMarkStr = reader["ObtainedMark_ofSubject"]?.ToString()?.Trim() ?? "";
                                    string totalMarkStr = reader["TotalMark_ofSubject"]?.ToString()?.Trim() ?? "";
                                    string grade = reader["SubjectGrades"]?.ToString()?.Trim() ?? "";
                                    string passStatus = reader["PassStatus_Subject"]?.ToString()?.Trim()?.ToUpper() ?? "";

                                    var subjectResult = new SubjectResultInfo();
                                    
                                    // Check if absent
                                    if (string.IsNullOrEmpty(obtainedMarkStr) || 
                                        obtainedMarkStr.ToUpper() == "A" || 
                                        obtainedMarkStr.ToUpper() == "ABS" || 
                                        obtainedMarkStr.ToUpper() == "ABSENT")
                                    {
                                        subjectResult.IsAbsent = true;
                                        subjectResult.IsFailed = true;
                                        subjectResult.ObtainedMarks = "ABS";
                                        subjectResult.Shortage = "-";
                                        student.FailedCount++;
                                    }
                                    else if (decimal.TryParse(obtainedMarkStr, out decimal obtainedMarks) && 
                                             decimal.TryParse(totalMarkStr, out decimal totalMarks) && 
                                             totalMarks > 0)
                                    {
                                        decimal passMarks = totalMarks * 0.33m;
                                        
                                        // Check if failed
                                        bool isFailed = grade == "F" || 
                                                       passStatus == "FAIL" || 
                                                       passStatus == "F" || 
                                                       obtainedMarks < passMarks;
                                        
                                        subjectResult.IsFailed = isFailed;
                                        subjectResult.ObtainedMarks = obtainedMarks.ToString("F0");
                                        
                                        if (isFailed)
                                        {
                                            decimal shortage = passMarks - obtainedMarks;
                                            subjectResult.Shortage = shortage > 0 ? shortage.ToString("F0") : "0";
                                            student.FailedCount++;
                                        }
                                        else
                                        {
                                            subjectResult.Shortage = "-";
                                        }
                                    }
                                    else
                                    {
                                        // Unable to parse marks - treat as failed
                                        subjectResult.IsFailed = true;
                                        subjectResult.ObtainedMarks = obtainedMarkStr.Length > 0 ? obtainedMarkStr : "N/A";
                                        subjectResult.Shortage = "-";
                                        student.FailedCount++;
                                    }
                                    
                                    student.SubjectResults[subjectID] = subjectResult;
                                }
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"   Student {student.StudentName}: {student.FailedCount} failed subjects");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error getting unsuccessful students: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            return unsuccessfulStudents;
        }

        // Helper Classes
        [Serializable]
        public class SubjectGradeDistribution
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; }
            public Dictionary<string, int> GradeCounts { get; set; }
            public int TotalEnrolled { get; set; }
        }

        [Serializable]
        public class GradeRangeInfo
        {
            public string Grade { get; set; }
            public decimal MinMarks { get; set; }
            public decimal MaxMarks { get; set; }
        }

        [Serializable]
        public class SubjectInfo
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; }
        }

        [Serializable]
        public class UnsuccessfulStudentInfo
        {
            public int StudentID { get; set; }
            public string StudentName { get; set; }
            public string Roll { get; set; }
            public string Grade { get; set; }
            public int StudentResultID { get; set; }
            public int FailedCount { get; set; }
            public Dictionary<int, SubjectResultInfo> SubjectResults { get; set; }
        }

        [Serializable]
        public class SubjectResultInfo
        {
            public bool IsFailed { get; set; }
            public bool IsAbsent { get; set; }
            public string ObtainedMarks { get; set; }
            public string Shortage { get; set; }
        }

        // Helper method to recursively find control
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
    }
}
