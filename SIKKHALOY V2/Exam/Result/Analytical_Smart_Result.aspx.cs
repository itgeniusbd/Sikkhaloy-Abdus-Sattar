using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
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
            else
            {
                // Regenerate dynamic table on postback
                if (ClassDropDownList.SelectedIndex != 0 && ExamDropDownList.SelectedIndex != 0)
                {
                    GenerateDynamicUnsuccessfulStudentsTable();
                }
            }
        }

        private void LoadSchoolName()
        {
            try
            {
                // Load school name from database dynamically
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT SchoolName 
                        FROM School 
                        WHERE SchoolID = @SchoolID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));

                        var schoolName = cmd.ExecuteScalar()?.ToString();
                        if (!string.IsNullOrEmpty(schoolName))
                        {
                            SchoolNameLabel.Text = schoolName;
                        }
                        else
                        {
                            SchoolNameLabel.Text = "School Name"; // Fallback
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading school name: " + ex.Message);
                // Fallback to session or default
                SchoolNameLabel.Text = Session["SchoolName"]?.ToString() ?? "School Name";
            }
        }

        private void UpdateClassExamLabel()
        {
            try
            {
                string className = ClassDropDownList.SelectedIndex > 0 ? ClassDropDownList.SelectedItem.Text : "";
                string examName = ExamDropDownList.SelectedIndex > 0 ? ExamDropDownList.SelectedItem.Text : "";

                if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(examName))
                {
                    ClassExamLabel.Text = $"Class: {className}, Exam: {examName}";
                }
                else
                {
                    ClassExamLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating label: " + ex.Message);
            }
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            // Auto-select first exam if available
            if (ExamDropDownList.Items.Count > 1)
            {
                ExamDropDownList.SelectedIndex = 1;
                UpdateClassExamLabel();

                if (ClassDropDownList.SelectedIndex != 0)
                {
                    LoadGradeChartData();
                    GenerateDynamicUnsuccessfulStudentsTable();
                }
            }
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassExamLabel();
            if (ClassDropDownList.SelectedIndex != 0 && ExamDropDownList.SelectedIndex != 0)
            {
                LoadGradeChartData();
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassExamLabel();
            if (ClassDropDownList.SelectedIndex != 0 && ExamDropDownList.SelectedIndex != 0)
            {
                LoadGradeChartData();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Generate dynamic table after all controls are loaded
            if (ClassDropDownList.SelectedIndex != 0 && ExamDropDownList.SelectedIndex != 0)
            {
                GenerateDynamicUnsuccessfulStudentsTable();
            }
        }

        private void LoadGradeChartData()
        {
            try
            {
                // Generate grade chart visualization
                var gradeData = GetGradeDistribution();
                StringBuilder chartHtml = new StringBuilder();

                foreach (var grade in gradeData)
                {
                    chartHtml.AppendFormat(@"
                        <div class='grade-chart'>
                            <div class='grade-count'>{0}</div>
                            <div class='grade-label'>Grade {1}</div>
                        </div>",
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
                    string query = @"
                        SELECT Student_Grade, COUNT(*) as StudentCount
                        FROM Exam_Result_of_Student 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID 
                        AND ClassID = @ClassID AND ExamID = @ExamID
                        GROUP BY Student_Grade
                        ORDER BY 
                            CASE Student_Grade 
                                WHEN 'A+' THEN 1 WHEN 'A' THEN 2 WHEN 'A-' THEN 3 
                                WHEN 'B' THEN 4 WHEN 'C' THEN 5 WHEN 'D' THEN 6 
                                WHEN 'F' THEN 7 ELSE 8 
                            END";

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
                                string grade = reader["Student_Grade"]?.ToString() ?? "N/A";
                                int count = Convert.ToInt32(reader["StudentCount"]);
                                gradeData[grade] = count;
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

        // New method to generate the dynamic table for unsuccessful students
        private void GenerateDynamicUnsuccessfulStudentsTable()
        {
            try
            {
                // Find the literal control first - try different approaches
                Literal literalControl = null;

                // Try direct FindControl
                literalControl = FindControl("DynamicTableLiteral") as Literal;

                // If not found, try recursive search
                if (literalControl == null)
                {
                    literalControl = FindControlRecursive(this, "DynamicTableLiteral") as Literal;
                }

                if (literalControl == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Could not find DynamicTableLiteral control");
                    return;
                }

                // Get all subjects and their sub-exams
                var subjectsWithSubExams = GetAllSubjectsWithSubExams();

                // Get unsuccessful students data with sub-exam details
                var unsuccessfulStudents = GetUnsuccessfulStudentsDataWithSubExams();

                if (subjectsWithSubExams.Count == 0 || unsuccessfulStudents.Count == 0)
                {
                    literalControl.Text = "<div class='no-data-message' style='text-align: center; padding: 40px; color: #28a745; font-size: 16px; font-weight: bold; background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 8px; margin: 20px;'>🎉 Great! No unsuccessful students found in this class and exam.</div>";
                    return;
                }

                // Generate enhanced table with smaller text and padding
                StringBuilder tableHtml = new StringBuilder();

                // Add custom CSS for compact styling
                tableHtml.Append(@"
                <style>
                    .enhanced-table-wrapper {
                        overflow-x: auto;
                        margin: 10px 0;
                        border-radius: 4px;
                        box-shadow: 0 1px 6px rgba(0,0,0,0.1);
                        background: white;
                    }
                    .dynamic-unsuccessful-table {
                        width: 100%;
                        border-collapse: collapse;
                        font-family: 'Arial', sans-serif;
                        background-color: white;
                        min-width: 700px;
                        font-size: 10px;
                    }
                    .dynamic-unsuccessful-table th,
                    .dynamic-unsuccessful-table td {
                        border: 1px solid #dee2e6;
                        text-align: center;
                        vertical-align: middle;
                        white-space: nowrap;
                        padding: 3px 2px;
                    }
                    .dynamic-unsuccessful-table tbody tr:nth-child(even) {
                        background-color: #f8f9fa;
                    }
                    .dynamic-unsuccessful-table tbody tr:hover {
                        background-color: #e3f2fd;
                        transition: background-color 0.2s ease;
                    }
                    
                    /* Enhanced student name column */
                    .dynamic-unsuccessful-table td:nth-child(2) {
                        text-align: left !important;
                        font-weight: 600 !important;
                        white-space: normal !important;
                        word-wrap: break-word !important;
                        font-size: 11px !important;
                        min-width: 120px !important;
                    }
                    
                    /* Enhanced student ID column */
                    .dynamic-unsuccessful-table td:nth-child(1) {
                        font-weight: bold !important;
                        min-width: 40px !important;
                    }
                    
                    @media print {
                        .enhanced-table-wrapper {
                            overflow-x: visible;
                            box-shadow: none;
                            margin: 3px 0;
                        }
                        .dynamic-unsuccessful-table {
                            min-width: auto;
                            font-size: 12px !important;
                            width: 100% !important;
                        }
                        .dynamic-unsuccessful-table th,
                        .dynamic-unsuccessful-table td {
                            font-size: 12px !important;
                            padding: 4px 3px !important;
                        }
                        
                        /* Much better print visibility for student names */
                        .dynamic-unsuccessful-table td:nth-child(2) {
                            font-size: 12px !important;
                            font-weight: bold !important;
                            text-align: left !important;
                            min-width: 150px !important;
                            max-width: none !important;
                            white-space: normal !important;
                            word-wrap: break-word !important;
                        }
                        
                        .dynamic-unsuccessful-table td:nth-child(1) {
                            font-size: 11px !important;
                            font-weight: bold !important;
                            min-width: 45px !important;
                        }
                        
                        @page {
                            margin: 0.4in;
                            size: A4 landscape;
                        }
                    }
                    
                    @media (max-width: 1200px) {
                        .dynamic-unsuccessful-table {
                            font-size: 9px;
                        }
                        .dynamic-unsuccessful-table td:nth-child(2) {
                            font-size: 10px !important;
                        }
                    }
                </style>");

                // Table wrapper for horizontal scroll
                tableHtml.Append("<div class='enhanced-table-wrapper'>");
                tableHtml.Append("<table class='dynamic-unsuccessful-table'>");

                // Create responsive headers
                GenerateCompactTableHeaders(tableHtml, subjectsWithSubExams);

                // Create data rows for each unsuccessful student
                tableHtml.Append("<tbody>");
                foreach (var student in unsuccessfulStudents)
                {
                    GenerateCompactStudentRow(tableHtml, student, subjectsWithSubExams);
                }
                tableHtml.Append("</tbody>");

                tableHtml.Append("</table>");
                tableHtml.Append("</div>");

                // Add enhanced summary info with proper variable scope
                int summaryFontSize = subjectsWithSubExams.Count <= 5 ? 11 : 10;
                int calculatedTotalColumns = subjectsWithSubExams.Sum(s => GetFailedSubExamsForSubject(s).Count * 2) + 2;
                tableHtml.Append($"<div style='font-size: {summaryFontSize}px; color: #495057; margin-top: 8px; text-align: center; font-weight: 500'>");
                tableHtml.Append($"📊 Showing <strong>{subjectsWithSubExams.Count}</strong> subjects with failed students | ");
                tableHtml.Append($"👥 <strong>{unsuccessfulStudents.Count}</strong> unsuccessful students | ");
                tableHtml.Append($"📋 Total <strong>{calculatedTotalColumns}</strong> data columns");
                tableHtml.Append("</div>");

                literalControl.Text = tableHtml.ToString();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error in GenerateDynamicUnsuccessfulStudentsTable: " + ex.Message);

                var literalControl = FindControl("DynamicTableLiteral") as Literal;
                if (literalControl == null)
                {
                    literalControl = FindControlRecursive(this, "DynamicTableLiteral") as Literal;
                }

                if (literalControl != null)
                {
                    literalControl.Text = "<div style='background-color: #f8d7da; padding: 15px; margin: 15px; border: 1px solid #f5c6cb; border-radius: 6px; color: #721c24;'>" +
                                  "<h5 style='margin: 0 0 8px 0; font-size: 14px;'>⚠️ Error loading unsuccessful students data</h5>" +
                                  "<p style='margin: 0; font-size: 12px;'>Please try refreshing the page or contact system administrator.</p>" +
                                  "</div>";
                }
            }
        }

        private void GenerateCompactTableHeaders(StringBuilder tableHtml, List<SubjectWithSubExams> subjects)
        {
            // Calculate enhanced font sizes based on number of subjects
            int subjectCount = subjects.Count;
            int headerFontSize, subHeaderFontSize, omLackFontSize;
            string padding;

            if (subjectCount <= 3)
            {
                headerFontSize = 12;
                subHeaderFontSize = 11;
                omLackFontSize = 10;
                padding = "6px 4px";
            }
            else if (subjectCount <= 5)
            {
                headerFontSize = 11;
                subHeaderFontSize = 10;
                omLackFontSize = 9;
                padding = "5px 3px";
            }
            else if (subjectCount <= 8)
            {
                headerFontSize = 10;
                subHeaderFontSize = 9;
                omLackFontSize = 8;
                padding = "4px 2px";
            }
            else if (subjectCount <= 12)
            {
                headerFontSize = 9;
                subHeaderFontSize = 8;
                omLackFontSize = 7;
                padding = "3px 2px";
            }
            else
            {
                headerFontSize = 8;
                subHeaderFontSize = 7;
                omLackFontSize = 6;
                padding = "2px 1px";
            }

            tableHtml.Append("<thead>");

            // Subject names row - enhanced sizing
            tableHtml.Append("<tr style='background-color: #2c3e50; color: white;'>");
            tableHtml.AppendFormat("<th rowspan='3' style='border: 1px solid #34495e; padding: {0}; text-align: center; color: white; font-weight: bold; font-size: {1}px; vertical-align: middle; min-width: {2}px;'>SL", 
                padding, headerFontSize, subjectCount <= 5 ? 40 : 30);
            
            int nameColumnWidth = subjectCount <= 3 ? 140 : (subjectCount <= 5 ? 120 : (subjectCount <= 8 ? 100 : 80));
            tableHtml.AppendFormat("<th rowspan='3' style='border: 1px solid #34495e; padding: {0}; text-align: center; color: white; font-weight: bold; font-size: {1}px; vertical-align: middle; min-width: {2}px; max-width: {3}px;'>Student Name</th>", 
                padding, headerFontSize, nameColumnWidth, nameColumnWidth + 20);

            foreach (var subject in subjects)
            {
                var failedSubExams = GetFailedSubExamsForSubject(subject);
                if (failedSubExams.Count > 0)
                {
                    int totalCols = failedSubExams.Count * 2;
                    int subjectNameLength = subjectCount <= 5 ? 15 : (subjectCount <= 8 ? 12 : 10);
                    string subjectName = subject.SubjectName.Length > subjectNameLength ? subject.SubjectName.Substring(0, subjectNameLength) + ".." : subject.SubjectName;
                    
                    tableHtml.AppendFormat("<th colspan='{0}' style='border: 1px solid #34495e; padding: {1}; text-align: center; background-color: #e74c3c; color: white; font-weight: bold; font-size: {2}px;' title='{3}'>{4}</th>", 
                        totalCols, padding, headerFontSize, subject.SubjectName, subjectName);
                }
            }
            tableHtml.Append("</tr>");

            // Sub-exam names row
            tableHtml.Append("<tr style='background-color: #34495e; color: white;'>");
            foreach (var subject in subjects)
            {
                var failedSubExams = GetFailedSubExamsForSubject(subject);
                foreach (var subExam in failedSubExams)
                {
                    int subExamNameLength = subjectCount <= 5 ? 12 : (subjectCount <= 8 ? 10 : 8);
                    string subExamName = subExam.SubExamType.Length > subExamNameLength ? subExam.SubExamType.Substring(0, subExamNameLength) + ".." : subExam.SubExamType;
                    
                    tableHtml.AppendFormat("<th colspan='2' style='border: 1px solid #34495e; padding: {0}; text-align: center; background-color: #3498db; color: white; font-weight: bold; font-size: {1}px;' title='{2}'>{3}</th>", 
                        padding, subHeaderFontSize, subExam.SubExamType, subExamName);
                }
            }
            tableHtml.Append("</tr>");

            // OM and Lack row
            tableHtml.Append("<tr style='background-color: #95a5a6; color: white;'>");
            foreach (var subject in subjects)
            {
                var failedSubExams = GetFailedSubExamsForSubject(subject);
                foreach (var subExam in failedSubExams)
                {
                    int minColWidth = subjectCount <= 5 ? 30 : (subjectCount <= 8 ? 25 : 20);
                    tableHtml.AppendFormat("<th style='border: 1px solid #34495e; padding: {0}; text-align: center; background-color: #f39c12; color: white; font-weight: bold; font-size: {1}px; min-width: {2}px;'>OM</th>", 
                        padding, omLackFontSize, minColWidth);
                    tableHtml.AppendFormat("<th style='border: 1px solid #34495e; padding: {0}; text-align: center; background-color: #e67e22; color: white; font-weight: bold; font-size: {1}px; min-width: {2}px;'>Lack</th>", 
                        padding, omLackFontSize, minColWidth);
                }
            }
            tableHtml.Append("</tr>");

            tableHtml.Append("</thead>");
        }

        private void GenerateCompactStudentRow(StringBuilder tableHtml, UnsuccessfulStudentDataEnhanced student, List<SubjectWithSubExams> subjects)
        {
            // Calculate enhanced responsive sizing
            int subjectCount = subjects.Count;
            int dataFontSize, nameFontSize;
            string padding;
            int nameLimit;

            if (subjectCount <= 3)
            {
                dataFontSize = 11;
                nameFontSize = 12;
                padding = "4px 3px";
                nameLimit = 25; // Much longer names for few subjects
            }
            else if (subjectCount <= 5)
            {
                dataFontSize = 10;
                nameFontSize = 11;
                padding = "3px 2px";
                nameLimit = 20;
            }
            else if (subjectCount <= 8)
            {
                dataFontSize = 9;
                nameFontSize = 10;
                padding = "2px 1px";
                nameLimit = 15;
            }
            else if (subjectCount <= 12)
            {
                dataFontSize = 8;
                nameFontSize = 9;
                padding = "2px 1px";
                nameLimit = 12;
            }
            else
            {
                dataFontSize = 7;
                nameFontSize = 8;
                padding = "1px";
                nameLimit = 10;
            }

            tableHtml.Append("<tr style='background-color: #ecf0f1;'>");

            // Student ID - enhanced
            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; font-weight: bold; color: #2c3e50; background-color: #ffffff; font-size: {1}px;'>{2}</td>", 
                padding, dataFontSize, student.StudentID);

            // Student Name - much better visibility
            string displayName;
            if (student.StudentName.Length > nameLimit)
            {
                displayName = student.StudentName.Substring(0, nameLimit) + "..";
            }
            else
            {
                displayName = student.StudentName;
            }

            int nameColumnWidth = subjectCount <= 3 ? 140 : (subjectCount <= 5 ? 120 : (subjectCount <= 8 ? 100 : 80));
            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: left; font-weight: 600; color: #2c3e50; background-color: #ffffff; font-size: {1}px; min-width: {2}px; max-width: {3}px; word-wrap: break-word; overflow: hidden;' title='{4}'>{5}</td>", 
                padding, nameFontSize, nameColumnWidth, nameColumnWidth + 20, student.StudentName, displayName);

            // Subject data with enhanced visibility
            foreach (var subject in subjects)
            {
                var failedSubExams = GetFailedSubExamsForSubject(subject);

                foreach (var subExam in failedSubExams)
                {
                    if (student.SubjectSubExamData.ContainsKey(subject.SubjectName))
                    {
                        var subjectData = student.SubjectSubExamData[subject.SubjectName];
                        SubExamMarks marksData = GetSubExamMarks(subjectData, subExam.SubExamType);

                        if (marksData != null && IsFailingMark(marksData.ObtainedMarks, subExam.PassMarks))
                        {
                            string obtainedMarks = marksData.ObtainedMarks;
                            decimal lackMarks = CalculateLack(obtainedMarks, subExam.PassMarks);

                            // OM Column - enhanced
                            string omCellColor = obtainedMarks?.ToUpper() == "A" ? "#e74c3c" : "#dc3545";
                            string omBackgroundColor = obtainedMarks?.ToUpper() == "A" ? "#ffebee" : "#fff5f5";
                            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; color: {1}; font-weight: bold; font-size: {2}px; background-color: {3};'>{4}</td>", 
                                padding, omCellColor, dataFontSize, omBackgroundColor, obtainedMarks);

                            // Lack Column - enhanced
                            string lackCellColor = lackMarks > 0 ? "#d32f2f" : "#388e3c";
                            string lackBackgroundColor = lackMarks > 0 ? "#ffcdd2" : "#c8e6c9";
                            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; color: {1}; font-weight: bold; font-size: {2}px; background-color: {3};'>{4}</td>", 
                                padding, lackCellColor, dataFontSize, lackBackgroundColor, lackMarks > 0 ? lackMarks.ToString("0") : "");
                        }
                        else
                        {
                            // Pass - empty cells
                            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #e8f5e8;'></td>", padding);
                            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #e8f5e8;'></td>", padding);
                        }
                    }
                    else
                    {
                        // No data - empty cells
                        tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #f5f5f5;'></td>", padding);
                        tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #f5f5f5;'></td>", padding);
                    }
                }
            }

            tableHtml.Append("</tr>");
        }

        // All other methods remain the same...
        private List<SubjectWithSubExams> GetAllSubjectsWithSubExams()
        {
            var subjects = new List<SubjectWithSubExams>();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    string subjectQuery = @"
                        SELECT DISTINCT s.SubjectID, s.SubjectName, ISNULL(s.SN, 999) as SortOrder
                        FROM Subject s
                        INNER JOIN Exam_Result_of_Subject ers ON s.SubjectID = ers.SubjectID
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        WHERE ers.SchoolID = @SchoolID 
                            AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID 
                            AND erst.ExamID = @ExamID
                            AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        ORDER BY SortOrder, s.SubjectName";

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
                                var subject = new SubjectWithSubExams
                                {
                                    SubjectID = Convert.ToInt32(reader["SubjectID"]),
                                    SubjectName = reader["SubjectName"]?.ToString() ?? "",
                                    SubExams = new List<SubExamInfo>()
                                };

                                subjects.Add(subject);
                            }
                        }
                    }

                    foreach (var subject in subjects)
                    {
                        GetSubExamsForSubjectFromDatabase(subject, con);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting subjects with sub-exams: " + ex.Message);
            }

            return subjects;
        }

        private void GetSubExamsForSubjectFromDatabase(SubjectWithSubExams subject, SqlConnection con)
        {
            try
            {
                string subExamQuery = @"
                    SELECT DISTINCT 
                        esn.SubExamName,
                        esn.SubExamID,
                        esn.Sub_ExamSN,
                        AVG(CAST(ISNULL(eom.PassMark, 33) AS DECIMAL)) as PassMark
                    FROM Exam_SubExam_Name esn
                    INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                    INNER JOIN Exam_Result_of_Student ers ON eom.StudentResultID = ers.StudentResultID
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    WHERE esn.SchoolID = @SchoolID
                        AND eom.SchoolID = @SchoolID
                        AND eom.EducationYearID = @EducationYearID
                        AND ers.ExamID = @ExamID
                        AND sc.ClassID = @ClassID
                        AND eom.SubjectID = @SubjectID
                        AND esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID
                    GROUP BY esn.SubExamName, esn.SubExamID, esn.Sub_ExamSN
                    ORDER BY esn.Sub_ExamSN, esn.SubExamName";

                using (SqlCommand cmd = new SqlCommand(subExamQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subject.SubjectID);
                    cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                    cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                    cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                    cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool hasSubExams = false;
                        while (reader.Read())
                        {
                            var subExamName = reader["SubExamName"]?.ToString();
                            var passMarkValue = reader["PassMark"];

                            if (!string.IsNullOrEmpty(subExamName))
                            {
                                decimal passMark = 0;
                                if (passMarkValue != null && passMarkValue != DBNull.Value)
                                {
                                    decimal.TryParse(passMarkValue.ToString(), out passMark);
                                }

                                subject.SubExams.Add(new SubExamInfo
                                {
                                    SubExamType = subExamName,
                                    SubExamID = Convert.ToInt32(reader["SubExamID"]),
                                    PassMarks = passMark > 0 ? passMark : GetDefaultPassMark(subExamName)
                                });
                                hasSubExams = true;
                            }
                        }

                        if (!hasSubExams)
                        {
                            CheckForTotalMarksOnly(subject, con);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting sub-exams from database for subject {subject.SubjectID}: " + ex.Message);
                subject.SubExams.Add(new SubExamInfo
                {
                    SubExamType = "Total",
                    SubExamID = 0,
                    PassMarks = 33
                });
            }
        }

        private void CheckForTotalMarksOnly(SubjectWithSubExams subject, SqlConnection con)
        {
            try
            {
                string totalMarksQuery = @"
                    SELECT COUNT(*) as HasData
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                    WHERE ers.SubjectID = @SubjectID
                        AND ers.SchoolID = @SchoolID 
                        AND ers.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID 
                        AND erst.ExamID = @ExamID
                        AND ISNULL(ers.IS_Add_InExam, 1) = 1";

                using (SqlCommand cmd = new SqlCommand(totalMarksQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subject.SubjectID);
                    cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                    cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                    cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                    cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                    var hasData = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                    if (hasData > 0)
                    {
                        subject.SubExams.Add(new SubExamInfo
                        {
                            SubExamType = "Total",
                            SubExamID = 0,
                            PassMarks = 33
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error checking total marks for {subject.SubjectName}: " + ex.Message);
                subject.SubExams.Add(new SubExamInfo
                {
                    SubExamType = "Total",
                    SubExamID = 0,
                    PassMarks = 33
                });
            }
        }

        private decimal GetDefaultPassMark(string subExamName)
        {
            if (string.IsNullOrEmpty(subExamName))
                return 33;

            var lowerName = subExamName.ToLower();

            if (lowerName.Contains("creative"))
                return 15;
            else if (lowerName.Contains("mcq"))
                return 10;
            else if (lowerName.Contains("cq"))
                return 8;
            else if (lowerName.Contains("structured"))
                return 20;
            else if (lowerName.Contains("practical"))
                return 20;
            else
                return 33;
        }

        private List<UnsuccessfulStudentDataEnhanced> GetUnsuccessfulStudentsDataWithSubExams()
        {
            var students = new List<UnsuccessfulStudentDataEnhanced>();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT DISTINCT
                            sc.StudentID,
                            s.StudentsName
                        FROM Exam_Result_of_Subject ers
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                        INNER JOIN Student s ON sc.StudentID = s.StudentID
                        WHERE ers.SchoolID = @SchoolID 
                            AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID 
                            AND erst.ExamID = @ExamID
                            AND s.Status = 'Active'
                            AND (
                                UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F'
                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                                OR (
                                    ISNUMERIC(ISNULL(ERS.ObtainedMark_ofSubject, '')) = 1 
                                    AND LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) NOT IN ('', 'A', 'ABS', 'ABSENT')
                                    AND CONVERT(FLOAT, LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) < 33
                                )
                            )
                        ORDER BY sc.StudentID";

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
                                var student = new UnsuccessfulStudentDataEnhanced
                                {
                                    StudentID = Convert.ToInt32(reader["StudentID"]),
                                    StudentName = reader["StudentsName"]?.ToString() ?? "",
                                    SubjectSubExamData = new Dictionary<string, SubExamMarksData>()
                                };

                                students.Add(student);
                            }
                        }
                    }

                    foreach (var student in students)
                    {
                        GetSubExamDetailsForStudentFromDatabase(student, con);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting unsuccessful students with sub-exams: " + ex.Message);
            }

            return students;
        }

        private void GetSubExamDetailsForStudentFromDatabase(UnsuccessfulStudentDataEnhanced student, SqlConnection con)
        {
            try
            {
                string query = @"
                    SELECT 
                        s.SubjectName,
                        esn.SubExamName,
                        eom.MarksObtained,
                        ISNULL(eom.PassMark, 33) as PassMark,
                        ISNULL(eom.AbsenceStatus, 'Present') as AbsenceStatus,
                        ers.ObtainedMark_ofSubject as TotalMarks
                    FROM Exam_Obtain_Marks eom
                    INNER JOIN Subject s ON eom.SubjectID = s.SubjectID
                    INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                    INNER JOIN Exam_Result_of_Student erst ON eom.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    INNER JOIN Exam_Result_of_Subject ers ON ers.StudentResultID = erst.StudentResultID AND ers.SubjectID = s.SubjectID
                    WHERE sc.StudentID = @StudentID
                        AND eom.SchoolID = @SchoolID 
                        AND eom.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID 
                        AND erst.ExamID = @ExamID
                        AND esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID
                        AND (
                            UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F'
                            OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                            OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                            OR (
                                ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                AND LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) NOT IN ('', 'A', 'ABS', 'ABSENT')
                                AND CONVERT(FLOAT, LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) < 33
                            )
                        )
                    ORDER BY s.SubjectName, esn.Sub_ExamSN";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@StudentID", student.StudentID);
                    cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                    cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                    cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                    cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var subjectData = new Dictionary<string, Dictionary<string, SubExamMarks>>();

                        while (reader.Read())
                        {
                            string subjectName = reader["SubjectName"]?.ToString() ?? "";
                            string subExamName = reader["SubExamName"]?.ToString() ?? "";
                            var marksObtainedValue = reader["MarksObtained"];
                            decimal passMark = Convert.ToDecimal(reader["PassMark"] ?? 33);
                            string absenceStatus = reader["AbsenceStatus"]?.ToString() ?? "Present";

                            string marksObtained = "0";
                            if (marksObtainedValue != null && marksObtainedValue != DBNull.Value)
                            {
                                marksObtained = marksObtainedValue.ToString();
                            }

                            bool isAbsent = string.Equals(absenceStatus, "Absent", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(marksObtained, "A", StringComparison.OrdinalIgnoreCase);

                            if (isAbsent)
                            {
                                marksObtained = "A";
                            }

                            if (!subjectData.ContainsKey(subjectName))
                            {
                                subjectData[subjectName] = new Dictionary<string, SubExamMarks>();
                            }

                            subjectData[subjectName][subExamName] = new SubExamMarks
                            {
                                ObtainedMarks = marksObtained,
                                PassMarks = passMark
                            };
                        }

                        foreach (var kvp in subjectData)
                        {
                            var subExamMarksData = new SubExamMarksData();

                            foreach (var subExamKvp in kvp.Value)
                            {
                                string subExamName = subExamKvp.Key;
                                SubExamMarks marksData = subExamKvp.Value;

                                var lowerName = subExamName.ToLower();
                                if (lowerName.Contains("creative"))
                                {
                                    subExamMarksData.Creative = marksData;
                                }
                                else if (lowerName.Contains("mcq"))
                                {
                                    subExamMarksData.MCQ = marksData;
                                }
                                else if (lowerName.Contains("cq"))
                                {
                                    subExamMarksData.CQ = marksData;
                                }
                                else if (lowerName.Contains("structured"))
                                {
                                    subExamMarksData.Structured = marksData;
                                }
                                else if (lowerName.Contains("practical"))
                                {
                                    subExamMarksData.Practical = marksData;
                                }
                                else
                                {
                                    subExamMarksData.Total = marksData;
                                }
                            }

                            student.SubjectSubExamData[kvp.Key] = subExamMarksData;
                        }

                        if (subjectData.Count == 0)
                        {
                            GetTotalMarksForFailedStudent(student, con);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sub-exam details from database for student {student.StudentID}: " + ex.Message);
                GetTotalMarksForFailedStudent(student, con);
            }
        }

        private void GetTotalMarksForFailedStudent(UnsuccessfulStudentDataEnhanced student, SqlConnection con)
        {
            try
            {
                string totalQuery = @"
                    SELECT 
                        s.SubjectName,
                        ers.ObtainedMark_ofSubject as TotalMarks
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Subject s ON ers.SubjectID = s.SubjectID
                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    WHERE sc.StudentID = @StudentID
                        AND ers.SchoolID = @SchoolID 
                        AND ers.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID 
                        AND erst.ExamID = @ExamID
                        AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        AND (
                            UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F'
                            OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                            OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                            OR (
                                ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                AND LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) NOT IN ('', 'A', 'ABS', 'ABSENT')
                                AND CONVERT(FLOAT, LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) < 33
                            )
                        )
                    ORDER BY s.SubjectName";

                using (SqlCommand cmd = new SqlCommand(totalQuery, con))
                {
                    cmd.Parameters.AddWithValue("@StudentID", student.StudentID);
                    cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                    cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                    cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                    cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subjectName = reader["SubjectName"]?.ToString() ?? "";
                            string totalMarks = reader["TotalMarks"]?.ToString() ?? "0";

                            var subExamMarksData = new SubExamMarksData
                            {
                                Total = new SubExamMarks
                                {
                                    ObtainedMarks = totalMarks,
                                    PassMarks = 33
                                }
                            };

                            student.SubjectSubExamData[subjectName] = subExamMarksData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting total marks for student {student.StudentID}: " + ex.Message);
            }
        }

        private SubExamMarks GetSubExamMarks(SubExamMarksData data, string subExamType)
        {
            if (data != null)
            {
                var lowerSubExamType = subExamType.ToLower();

                if (subExamType == "Total" && data.Total != null)
                    return data.Total;

                if (lowerSubExamType.Contains("creative") && data.Creative != null)
                    return data.Creative;
                else if (lowerSubExamType.Contains("mcq") && data.MCQ != null)
                    return data.MCQ;
                else if (lowerSubExamType.Contains("cq") && data.CQ != null)
                    return data.CQ;
                else if (lowerSubExamType.Contains("structured") && data.Structured != null)
                    return data.Structured;
                else if (lowerSubExamType.Contains("practical") && data.Practical != null)
                    return data.Practical;

                if (data.Creative != null) return data.Creative;
                if (data.MCQ != null) return data.MCQ;
                if (data.CQ != null) return data.CQ;
                if (data.Structured != null) return data.Structured;
                if (data.Practical != null) return data.Practical;
                if (data.Total != null) return data.Total;
            }

            return null;
        }

        private List<SubExamInfo> GetFailedSubExamsForSubject(SubjectWithSubExams subject)
        {
            return subject.SubExams; // Simplified - return all sub-exams
        }

        private decimal CalculateLack(string obtainedMarks, decimal passMarks)
        {
            if (obtainedMarks?.ToUpper() == "A" || obtainedMarks?.ToUpper() == "ABS" || obtainedMarks?.ToUpper() == "ABSENT")
            {
                return passMarks;
            }

            if (decimal.TryParse(obtainedMarks, out decimal marks))
            {
                return Math.Max(0, passMarks - marks);
            }

            return passMarks;
        }

        private bool IsFailingMark(string obtainedMarks, decimal passMarks)
        {
            if (obtainedMarks?.ToUpper() == "A" || obtainedMarks?.ToUpper() == "ABS" || obtainedMarks?.ToUpper() == "ABSENT")
            {
                return true;
            }

            if (decimal.TryParse(obtainedMarks, out decimal marks))
            {
                return marks < passMarks;
            }

            return true;
        }

        private Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control control in root.Controls)
            {
                Control found = FindControlRecursive(control, id);
                if (found != null)
                    return found;
            }

            return null;
        }
    }

    // Helper Classes
    [Serializable]
    public class SubjectWithSubExams
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public List<SubExamInfo> SubExams { get; set; }
    }

    [Serializable]
    public class SubExamInfo
    {
        public string SubExamType { get; set; }
        public int SubExamID { get; set; }
        public decimal PassMarks { get; set; }
    }

    [Serializable]
    public class UnsuccessfulStudentDataEnhanced
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public Dictionary<string, SubExamMarksData> SubjectSubExamData { get; set; }
    }

    [Serializable]
    public class SubExamMarksData
    {
        public SubExamMarks Creative { get; set; }
        public SubExamMarks MCQ { get; set; }
        public SubExamMarks CQ { get; set; }
        public SubExamMarks Structured { get; set; }
        public SubExamMarks Practical { get; set; }
        public SubExamMarks Total { get; set; }
    }

    [Serializable]
    public class SubExamMarks
    {
        public string ObtainedMarks { get; set; }
        public decimal PassMarks { get; set; }
    }
}