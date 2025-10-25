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
                // শুধু স্কুল নাম লোড করবো, অন্য কিছু না
                UpdateClassExamLabel();
            }
            // Page_PreRender এ ডাইনামিক টেবিল জেনারেট হবে, এখানে আর দরকার নেই
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
            // Ensure the default "SELECT EXAM" item is always selected
            if (ExamDropDownList.Items.Count > 0)
            {
                ExamDropDownList.SelectedIndex = 0; // Select the first item which is "[ SELECT EXAM ]"
            }

            // শুধু লেবেল আপডেট করবো, অন্য কিছু না
            UpdateClassExamLabel();
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // এখানে সবাই ডাইনামিক টেবিল জেনারেট হবে
            UpdateClassExamLabel();

            // শুধু যখন ক্লাশ এবং এক্সাম দুটোই সিলেক্ট থাকবে এবং এক্সাম ভ্যালু 0 না হয় তখন ডেটা লোড করবো
            if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0 &&
                ExamDropDownList.SelectedValue != "0")
            {
                System.Diagnostics.Debug.WriteLine("🎯 Loading all report data after exam selection");
                LoadGradeChartData();
                // ডাইনামিক টেবিল Page_PreRender এ জেনারেট হবে
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Class or Exam not properly selected, skipping data load");
                // ডেটা ক্লিয়ার করি
                ClearReportData();
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ক্লাশ সিলেক্ট করলে শুধু লেবেল আপডেট করবো এবং এক্সাম ড্রপডাউন রিসেট করবো
            UpdateClassExamLabel();

            // Force exam dropdown to rebind by clearing selection first
            if (ExamDropDownList.Items.Count > 0)
            {
                ExamDropDownList.ClearSelection();
            }

            // রিপোর্ট ডেটা ক্লিয়ার করি কারণ এখনো এক্সাম সিলেক্ট হয়নি
            ClearReportData();

            System.Diagnostics.Debug.WriteLine($"🏫 Class changed to: {ClassDropDownList.SelectedItem?.Text}, Exam dropdown will be reset after rebind");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // শুধু যখন ক্লাশ এবং এক্সাম দুটোই সিলেক্ট থাকবে এবং এক্সাম ভ্যালু 0 না হয় তখন ডাইনামিক টেবিল জেনারেট করবো
            if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0 &&
                ExamDropDownList.SelectedValue != "0")
            {
                System.Diagnostics.Debug.WriteLine("🎯 Generating dynamic table in Page_PreRender");
                GenerateDynamicUnsuccessfulStudentsTable();
            }
        }

        // নতুন মেথড - রিপোর্ট ডেটা ক্লিয়ার করার জন্য
        private void ClearReportData()
        {
            try
            {
                // গ্রেড চার্ট ক্লিয়ার করি
                if (GradeChartLiteral != null)
                {
                    GradeChartLiteral.Text = "";
                }

                // ডাইনামিক টেবিল ক্লিয়ার করি
                Literal literalControl = FindControl("DynamicTableLiteral") as Literal;
                if (literalControl == null)
                {
                    literalControl = FindControlRecursive(this, "DynamicTableLiteral") as Literal;
                }

                if (literalControl != null)
                {
                    literalControl.Text = "";
                }

                System.Diagnostics.Debug.WriteLine("🧹 Report data cleared");
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
                System.Diagnostics.Debug.WriteLine($"📊 Grade chart loaded with {gradeData.Count} grades");
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

                // Get actual failed students with only their failed subjects
                var failedStudentsWithFailedSubjects = GetStudentsWithOnlyFailedSubjects();

                // Add debugging information
                System.Diagnostics.Debug.WriteLine($"🔍 Found {failedStudentsWithFailedSubjects.Count} students with failed subjects");

                foreach (var student in failedStudentsWithFailedSubjects)
                {
                    System.Diagnostics.Debug.WriteLine($"👨‍🎓 Student: {student.StudentName} (ID: {student.StudentID}) - Failed in {student.FailedSubjects.Count} subjects");
                    foreach (var failedSubject in student.FailedSubjects)
                    {
                        System.Diagnostics.Debug.WriteLine($"   📘 Failed Subject: {failedSubject.SubjectName}");
                    }
                }

                if (failedStudentsWithFailedSubjects.Count == 0)
                {
                    literalControl.Text = "<div class='no-data-message' style='text-align: center; padding: 40px; color: #28a745; font-size: 16px; font-weight: bold; background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 8px; margin: 20px;'>🎉 Great! No unsuccessful students found in this class and exam.</div>";
                    return;
                }

                // Get unique failed subjects across all students
                var uniqueFailedSubjects = GetUniqueFailedSubjects(failedStudentsWithFailedSubjects);

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
                GenerateFailedSubjectsTableHeaders(tableHtml, uniqueFailedSubjects);

                // Create data rows for each unsuccessful student
                tableHtml.Append("<tbody>");
                foreach (var student in failedStudentsWithFailedSubjects)
                {
                    GenerateFailedStudentRow(tableHtml, student, uniqueFailedSubjects);
                }
                tableHtml.Append("</tbody>");

                tableHtml.Append("</table>");
                tableHtml.Append("</div>");

                // Add enhanced summary info
                int summaryFontSize = uniqueFailedSubjects.Count <= 5 ? 11 : 10;
                int calculatedTotalColumns = uniqueFailedSubjects.Sum(s => s.SubExams.Count * 2) + 2;
                tableHtml.Append($"<div style='font-size: {summaryFontSize}px; color: #495057; margin-top: 8px; text-align: center; font-weight: 500'>");
                tableHtml.Append($"📊 Showing <strong>{uniqueFailedSubjects.Count}</strong> subjects with failed students | ");
                tableHtml.Append($"👥 <strong>{failedStudentsWithFailedSubjects.Count}</strong> unsuccessful students | ");
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

        private void GenerateFailedSubjectsTableHeaders(StringBuilder tableHtml, List<SubjectWithSubExams> subjects)
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
            tableHtml.AppendFormat("<th rowspan='3' style='border: 1px solid #34495e; padding: {0}; text-align: center; color: Black; font-weight: bold; font-size: {1}px; vertical-align: middle; min-width: {2}px;'> Student ID</th>",
                padding, headerFontSize, subjectCount <= 5 ? 40 : 30);

            int nameColumnWidth = subjectCount <= 3 ? 140 : (subjectCount <= 5 ? 120 : (subjectCount <= 8 ? 100 : 80));
            tableHtml.AppendFormat("<th rowspan='3' style='border: 1px solid #34495e; padding: {0}; text-align: center; color: Black; font-weight: bold; font-size: {1}px; vertical-align: middle; min-width: {2}px; max-width: {3}px;'>Student Name</th>",
                padding, headerFontSize, nameColumnWidth, nameColumnWidth + 20);

            foreach (var subject in subjects)
            {
                if (subject.SubExams.Count > 0)
                {
                    int totalCols = subject.SubExams.Count * 2;
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
                foreach (var subExam in subject.SubExams)
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
                foreach (var subExam in subject.SubExams)
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

        private void GenerateFailedStudentRow(StringBuilder tableHtml, FailedStudentData student, List<SubjectWithSubExams> subjects)
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
                nameLimit = 35;
            }
            else if (subjectCount <= 5)
            {
                dataFontSize = 11;
                nameFontSize = 12;
                padding = "1px 1px";
                nameLimit = 25;
            }
            else if (subjectCount <= 8)
            {
                dataFontSize = 10;
                nameFontSize = 11;
                padding = "1px 1px";
                nameLimit = 20;
            }
            else if (subjectCount <= 12)
            {
                dataFontSize = 9;
                nameFontSize = 10;
                padding = "1px 1px";
                nameLimit = 18;
            }
            else
            {
                dataFontSize = 9;
                nameFontSize = 10;
                padding = "0px";
                nameLimit = 15;
            }

            tableHtml.Append("<tr style='background-color: #ecf0f1;'>");

            // Student ID (ID field from Student table) - enhanced
            string displayID = !string.IsNullOrEmpty(student.StudentIDString) ? student.StudentIDString : student.StudentID.ToString();
            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; font-weight: bold; color: #2c3e50; background-color: #ffffff; font-size: {1}px;'>{2}</td>",
                padding, dataFontSize, displayID);

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

            // Subject data - only show data for subjects where student has actually failed
            foreach (var subject in subjects)
            {
                foreach (var subExam in subject.SubExams)
                {
                    // Find this student's failed subject data
                    var studentFailedSubject = student.FailedSubjects.FirstOrDefault(fs => fs.SubjectName == subject.SubjectName);

                    if (studentFailedSubject != null)
                    {
                        // Find the specific sub-exam data
                        var studentSubExam = studentFailedSubject.SubExams.FirstOrDefault(se => se.SubExamType == subExam.SubExamType);

                        if (studentSubExam != null)
                        {
                            string obtainedMarks = studentSubExam.ObtainedMarks;
                            decimal lackMarks = CalculateLack(obtainedMarks, studentSubExam.PassMarks);

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
                            // Student failed this subject but not this specific sub-exam - empty cells
                            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #e8f5e8;'></td>", padding);
                            tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #e8f5e8;'></td>", padding);
                        }
                    }
                    else
                    {
                        // Student didn't fail this subject - empty cells
                        tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #e8f5e8;'></td>", padding);
                        tableHtml.AppendFormat("<td style='border: 1px solid #bdc3c7; padding: {0}; text-align: center; background-color: #e8f5e8;'></td>", padding);
                    }
                }
            }

            tableHtml.Append("</tr>");
        }

        private List<FailedStudentData> GetStudentsWithOnlyFailedSubjects()
        {
            var failedStudents = new List<FailedStudentData>();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    // First, get students who failed in subjects with sub-exams (by sub-exam failure)
                    var studentsFailedBySubExam = GetStudentsFailedBySubExam(con);

                    // Second, get students who failed in subjects without sub-exams (by total marks)
                    var studentsFailedByTotalMarks = GetStudentsFailedByTotalMarks(con);

                    // Merge both results
                    var allFailedStudents = new Dictionary<int, FailedStudentData>();

                    // Add sub-exam failed students
                    foreach (var student in studentsFailedBySubExam)
                    {
                        if (!allFailedStudents.ContainsKey(student.StudentID))
                        {
                            allFailedStudents[student.StudentID] = student;
                        }
                        else
                        {
                            // Merge failed subjects
                            allFailedStudents[student.StudentID].FailedSubjects.AddRange(student.FailedSubjects);
                        }
                    }

                    // Add total marks failed students
                    foreach (var student in studentsFailedByTotalMarks)
                    {
                        if (!allFailedStudents.ContainsKey(student.StudentID))
                        {
                            allFailedStudents[student.StudentID] = student;
                        }
                        else
                        {
                            // Merge failed subjects
                            foreach (var failedSubject in student.FailedSubjects)
                            {
                                // Only add if not already exists
                                if (!allFailedStudents[student.StudentID].FailedSubjects.Any(fs => fs.SubjectID == failedSubject.SubjectID))
                                {
                                    allFailedStudents[student.StudentID].FailedSubjects.Add(failedSubject);
                                }
                            }
                        }
                    }

                    failedStudents = allFailedStudents.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting students with only failed subjects: " + ex.Message);
            }

            return failedStudents;
        }

        private List<FailedStudentData> GetStudentsFailedBySubExam(SqlConnection con)
        {
            var failedStudents = new List<FailedStudentData>();

            try
            {
                // Get students who failed in specific sub-exams - Modified to include s.ID
                string query = @"
                    SELECT DISTINCT
                        sc.StudentID,
                        s.StudentsName,
                        s.ID as StudentIDString,
                        sub.SubjectName,
                        sub.SubjectID
                    FROM Exam_Obtain_Marks eom
                    INNER JOIN Subject sub ON eom.SubjectID = sub.SubjectID
                    INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                    INNER JOIN Exam_Result_of_Student erst ON eom.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    INNER JOIN Student s ON sc.StudentID = s.StudentID
                    WHERE eom.SchoolID = @SchoolID 
                        AND eom.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID 
                        AND erst.ExamID = @ExamID
                        AND s.Status = 'Active'
                        AND esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID
                        AND (
                            UPPER(LTRIM(RTRIM(ISNULL(eom.AbsenceStatus, '')))) = 'ABSENT'
                            OR UPPER(LTRIM(RTRIM(ISNULL(eom.MarksObtained, '')))) IN ('A', 'ABS', 'ABSENT')
                            OR (
                                ISNUMERIC(ISNULL(eom.MarksObtained, '')) = 1 
                                AND LTRIM(RTRIM(ISNULL(eom.MarksObtained, ''))) NOT IN ('', 'A', 'ABS', 'ABSENT')
                                AND ISNUMERIC(ISNULL(eom.FullMark, '')) = 1
                                AND LTRIM(RTRIM(ISNULL(eom.FullMark, ''))) != ''
                                AND CAST(eom.FullMark AS DECIMAL(10,2)) > 0
                                AND CONVERT(FLOAT, LTRIM(RTRIM(eom.MarksObtained))) < (CAST(eom.FullMark AS DECIMAL(10,2)) * 0.33)
                            )
                        )
                    ORDER BY sc.StudentID, sub.SubjectName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                    cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                    cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                    cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var studentDict = new Dictionary<int, FailedStudentData>();

                        while (reader.Read())
                        {
                            int studentId = Convert.ToInt32(reader["StudentID"]);
                            string studentName = reader["StudentsName"]?.ToString() ?? "";
                            string studentIDString = reader["StudentIDString"]?.ToString() ?? studentId.ToString();
                            string subjectName = reader["SubjectName"]?.ToString() ?? "";
                            int subjectId = Convert.ToInt32(reader["SubjectID"]);

                            if (!studentDict.ContainsKey(studentId))
                            {
                                studentDict[studentId] = new FailedStudentData
                                {
                                    StudentID = studentId,
                                    StudentIDString = studentIDString,
                                    StudentName = studentName,
                                    FailedSubjects = new List<FailedSubjectData>()
                                };
                            }

                            // Check if this subject is already added
                            if (!studentDict[studentId].FailedSubjects.Any(fs => fs.SubjectID == subjectId))
                            {
                                var failedSubject = new FailedSubjectData
                                {
                                    SubjectID = subjectId,
                                    SubjectName = subjectName,
                                    TotalMarks = "",
                                    SubExams = new List<SubExamInfo>()
                                };

                                studentDict[studentId].FailedSubjects.Add(failedSubject);
                            }
                        }

                        failedStudents = studentDict.Values.ToList();
                    }
                }

                // Get sub-exam details for each failed subject
                foreach (var student in failedStudents)
                {
                    foreach (var failedSubject in student.FailedSubjects)
                    {
                        GetSubExamsForFailedSubject(failedSubject, student.StudentID, con);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting students failed by sub-exam: " + ex.Message);
            }

            return failedStudents;
        }

        private List<FailedStudentData> GetStudentsFailedByTotalMarks(SqlConnection con)
        {
            var failedStudents = new List<FailedStudentData>();

            try
            {
                // First, get all students with their total marks for subjects without sub-exams - Modified to include s.ID
                string query = @"
                    SELECT DISTINCT
                        sc.StudentID,
                        s.StudentsName,
                        s.ID as StudentIDString,
                        sub.SubjectName,
                        sub.SubjectID,
                        ers.ObtainedMark_ofSubject as TotalMarks,
                        ers.TotalMark_ofSubject as FullMarks
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    INNER JOIN Student s ON sc.StudentID = s.StudentID
                    INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
                    WHERE ers.SchoolID = @SchoolID 
                        AND ers.EducationYearID = @EducationYearID 
                        AND erst.ClassID = @ClassID 
                        AND erst.ExamID = @ExamID
                        AND s.Status = 'Active'
                        AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        -- Only include subjects that don't have sub-exam data
                        AND NOT EXISTS (
                            SELECT 1 FROM Exam_Obtain_Marks eom2 
                            INNER JOIN Exam_SubExam_Name esn2 ON eom2.SubExamID = esn2.SubExamID
                            WHERE eom2.StudentResultID = ers.StudentResultID 
                                AND eom2.SubjectID = ers.SubjectID
                                AND eom2.SchoolID = @SchoolID
                                AND eom2.EducationYearID = @EducationYearID
                                AND esn2.SchoolID = @SchoolID
                                AND esn2.EducationYearID = @EducationYearID
                        )
                    ORDER BY sc.StudentID, sub.SubjectName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                    cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                    cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                    cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var studentDict = new Dictionary<int, FailedStudentData>();

                        while (reader.Read())
                        {
                            int studentId = Convert.ToInt32(reader["StudentID"]);
                            string studentName = reader["StudentsName"]?.ToString() ?? "";
                            string studentIDString = reader["StudentIDString"]?.ToString() ?? studentId.ToString();
                            string subjectName = reader["SubjectName"]?.ToString() ?? "";
                            int subjectId = Convert.ToInt32(reader["SubjectID"]);
                            string totalMarks = reader["TotalMarks"]?.ToString() ?? "";
                            string fullMarks = reader["FullMarks"]?.ToString() ?? "";

                            // Calculate correct pass marks (33% of full marks)
                            decimal passMarks = 33; // default fallback
                            if (decimal.TryParse(fullMarks, out decimal fullMarksValue) && fullMarksValue > 0)
                            {
                                passMarks = Math.Round(fullMarksValue * 0.33m, 2);
                            }

                            // Check if student actually failed based on calculated pass marks
                            bool isFailed = false;
                            if (totalMarks?.ToUpper() == "A" || totalMarks?.ToUpper() == "ABS" || totalMarks?.ToUpper() == "ABSENT")
                            {
                                isFailed = true;
                            }
                            else if (string.IsNullOrEmpty(totalMarks) || totalMarks == "0")
                            {
                                isFailed = true;
                            }
                            else if (decimal.TryParse(totalMarks, out decimal obtainedMarks))
                            {
                                isFailed = obtainedMarks < passMarks;
                            }
                            else
                            {
                                // Check grade-based failure
                                string gradeQuery = @"
                                    SELECT ers.SubjectGrades, ers.PassStatus_Subject 
                                    FROM Exam_Result_of_Subject ers
                                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                                    WHERE sc.StudentID = @StudentID AND ers.SubjectID = @SubjectID
                                        AND ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID 
                                        AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID";

                                using (SqlConnection con2 = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                                {
                                    con2.Open();
                                    using (SqlCommand gradeCmd = new SqlCommand(gradeQuery, con2))
                                    {
                                        gradeCmd.Parameters.AddWithValue("@StudentID", studentId);
                                        gradeCmd.Parameters.AddWithValue("@SubjectID", subjectId);
                                        gradeCmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                                        gradeCmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                                        gradeCmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                                        gradeCmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                                        using (SqlDataReader gradeReader = gradeCmd.ExecuteReader())
                                        {
                                            if (gradeReader.Read())
                                            {
                                                string grade = gradeReader["SubjectGrades"]?.ToString()?.Trim().ToUpper() ?? "";
                                                string passStatus = gradeReader["PassStatus_Subject"]?.ToString()?.Trim().ToUpper() ?? "";

                                                isFailed = grade == "F" || passStatus == "FAIL" || passStatus == "F";
                                            }
                                        }
                                    }
                                }
                            }

                            // Only add if student actually failed
                            if (isFailed)
                            {
                                System.Diagnostics.Debug.WriteLine($"🔍 Student {studentName} failed in {subjectName}: ObtainedMarks={totalMarks}, PassMarks={passMarks} (33% of {fullMarks})");

                                if (!studentDict.ContainsKey(studentId))
                                {
                                    studentDict[studentId] = new FailedStudentData
                                    {
                                        StudentID = studentId,
                                        StudentIDString = studentIDString,
                                        StudentName = studentName,
                                        FailedSubjects = new List<FailedSubjectData>()
                                    };
                                }

                                // Add this failed subject
                                var failedSubject = new FailedSubjectData
                                {
                                    SubjectID = subjectId,
                                    SubjectName = subjectName,
                                    TotalMarks = totalMarks,
                                    SubExams = new List<SubExamInfo>
                                    {
                                        new SubExamInfo
                                        {
                                            SubExamType = "Total",
                                            SubExamID = 0,
                                            PassMarks = passMarks,
                                            ObtainedMarks = totalMarks
                                        }
                                    }
                                };

                                studentDict[studentId].FailedSubjects.Add(failedSubject);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"✅ Student {studentName} passed in {subjectName}: ObtainedMarks={totalMarks}, PassMarks={passMarks} (33% of {fullMarks})");
                            }
                        }

                        failedStudents = studentDict.Values.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting students failed by total marks: " + ex.Message);
            }

            return failedStudents;
        }

        private void GetSubExamsForFailedSubject(FailedSubjectData failedSubject, int studentId, SqlConnection con)
        {
            try
            {
                string subExamQuery = @"
                    SELECT DISTINCT 
                        esn.SubExamName,
                        esn.SubExamID,
                        esn.Sub_ExamSN,
                        eom.MarksObtained,
                        eom.FullMark,
                        ISNULL(eom.PassMark, 0) as PassMark,
                        ISNULL(eom.AbsenceStatus, 'Present') as AbsenceStatus
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
                        AND sc.StudentID = @StudentID
                        AND esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID
                    ORDER BY esn.Sub_ExamSN, esn.SubExamName";

                using (SqlCommand cmd = new SqlCommand(subExamQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", failedSubject.SubjectID);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
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
                            var marksObtainedValue = reader["MarksObtained"];
                            var fullMarkValue = reader["FullMark"];
                            var passMarkValue = reader["PassMark"];
                            string absenceStatus = reader["AbsenceStatus"]?.ToString() ?? "Present";

                            if (!string.IsNullOrEmpty(subExamName))
                            {
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

                                // Calculate pass marks as 33% of full marks
                                decimal passMark = 0;
                                if (fullMarkValue != null && fullMarkValue != DBNull.Value && decimal.TryParse(fullMarkValue.ToString(), out decimal fullMarkDecimal) && fullMarkDecimal > 0)
                                {
                                    passMark = Math.Round(fullMarkDecimal * 0.33m, 2);
                                }
                                else if (passMarkValue != null && passMarkValue != DBNull.Value)
                                {
                                    decimal.TryParse(passMarkValue.ToString(), out passMark);
                                }

                                // Use dynamic calculation for better accuracy if passMark still 0
                                if (passMark <= 0)
                                {
                                    passMark = GetDefaultPassMark(subExamName);
                                }

                                // Only add if this sub-exam is actually failed
                                if (IsFailingMark(marksObtained, passMark))
                                {
                                    failedSubject.SubExams.Add(new SubExamInfo
                                    {
                                        SubExamType = subExamName,
                                        SubExamID = Convert.ToInt32(reader["SubExamID"]),
                                        PassMarks = passMark,
                                        ObtainedMarks = marksObtained
                                    });
                                    hasSubExams = true;
                                }
                            }
                        }

                        // If no sub-exams found or no failed sub-exams, add total marks as failed
                        if (!hasSubExams)
                        {
                            // Use dynamic pass marks for total marks
                            decimal totalPassMarks = GetDynamicPassMarksForSubject(failedSubject.SubjectID, failedSubject.SubjectName);

                            failedSubject.SubExams.Add(new SubExamInfo
                            {
                                SubExamType = "Total",
                                SubExamID = 0,
                                PassMarks = totalPassMarks,
                                ObtainedMarks = failedSubject.TotalMarks
                            });

                            System.Diagnostics.Debug.WriteLine($"🎯 Added Total sub-exam for failed subject {failedSubject.SubjectName} with dynamic PassMarks={totalPassMarks}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting sub-exams for failed subject {failedSubject.SubjectID}: " + ex.Message);

                // Use dynamic pass marks even in error case
                decimal totalPassMarks = GetDynamicPassMarksForSubject(failedSubject.SubjectID, failedSubject.SubjectName);

                failedSubject.SubExams.Add(new SubExamInfo
                {
                    SubExamType = "Total",
                    SubExamID = 0,
                    PassMarks = totalPassMarks,
                    ObtainedMarks = failedSubject.TotalMarks
                });
            }
        }

        // Enhanced method to get accurate pass marks for subjects without sub-exams
        private decimal GetDynamicPassMarksForSubject(int subjectId, string subjectName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    // Get the maximum total marks for this subject to calculate pass marks (33%)
                    string query = @"
                        SELECT MAX(CASE 
                            WHEN ISNUMERIC(ers.TotalMark_ofSubject) = 1 
                            THEN CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2))
                            ELSE 0 
                        END) as MaxTotalMarks
                        FROM Exam_Result_of_Subject ers
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        WHERE ers.SubjectID = @SubjectID
                            AND ers.SchoolID = @SchoolID
                            AND ers.EducationYearID = @EducationYearID
                            AND erst.ClassID = @ClassID
                            AND erst.ExamID = @ExamID
                            AND ISNULL(ers.IS_Add_InExam, 1) = 1
                            AND ers.TotalMark_ofSubject IS NOT NULL 
                            AND ers.TotalMark_ofSubject != ''
                            AND ISNUMERIC(ers.TotalMark_ofSubject) = 1";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                        cmd.Parameters.AddWithValue("@SchoolID", Convert.ToInt32(Session["SchoolID"] ?? "1"));
                        cmd.Parameters.AddWithValue("@EducationYearID", Convert.ToInt32(Session["Edu_Year"] ?? "1"));
                        cmd.Parameters.AddWithValue("@ClassID", Convert.ToInt32(ClassDropDownList?.SelectedValue ?? "0"));
                        cmd.Parameters.AddWithValue("@ExamID", Convert.ToInt32(ExamDropDownList?.SelectedValue ?? "0"));

                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            decimal maxMarks = Convert.ToDecimal(result);
                            if (maxMarks > 0)
                            {
                                decimal passMarks = Math.Round(maxMarks * 0.33m, 2);
                                System.Diagnostics.Debug.WriteLine($"🎯 Dynamic PassMarks for {subjectName} (ID:{subjectId}): MaxMarks={maxMarks}, PassMarks={passMarks} (33%)");
                                return passMarks;
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting dynamic pass marks for {subjectName}: " + ex.Message);
            }

            // Fallback to subject-specific estimation (still using 33% logic)
            return GetEstimatedPassMarksForSubject(subjectName);
        }

        // Fallback method for estimating pass marks when database lookup fails
        private decimal GetEstimatedPassMarksForSubject(string subjectName)
        {
            // Enhanced fallback: estimate typical full marks and calculate 33%
            var lowerName = subjectName.ToLower();

            if (lowerName.Contains("drawing") || lowerName.Contains("art"))
                return Math.Round(50 * 0.33m, 2); // Typically 50 marks -> 16.5
            else if (lowerName.Contains("ict") || lowerName.Contains("computer"))
                return Math.Round(50 * 0.33m, 2); // 50 marks -> 16.5
            else if (lowerName.Contains("work") || lowerName.Contains("education"))
                return Math.Round(50 * 0.33m, 2); // 50 marks -> 16.5
            else if (lowerName.Contains("religion") || lowerName.Contains("islam") || lowerName.Contains("hindu") || lowerName.Contains("christian") || lowerName.Contains("buddhist"))
                return Math.Round(100 * 0.33m, 2); // 100 marks -> 33
            else if (lowerName.Contains("physical") || lowerName.Contains("sports"))
                return Math.Round(50 * 0.33m, 2); // 50 marks -> 16.5
            else
                return Math.Round(100 * 0.33m, 2); // Default major subjects (100 marks -> 33)
        }

        private decimal CalculateLack(string obtainedMarks, decimal passMarks)
        {
            if (obtainedMarks?.ToUpper() == "A" || obtainedMarks?.ToUpper() == "ABS" || obtainedMarks?.ToUpper() == "ABSENT")
            {
                return passMarks;
            }

            if (decimal.TryParse(obtainedMarks, out decimal marks))
            {
                decimal lack = Math.Max(0, passMarks - marks);

                // Debug output to see what's happening
                System.Diagnostics.Debug.WriteLine($"🔍 CalculateLack - Obtained: {obtainedMarks}, PassMarks: {passMarks}, Lack: {lack}");

                return lack;
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

        private List<SubjectWithSubExams> GetUniqueFailedSubjects(List<FailedStudentData> failedStudents)
        {
            var uniqueSubjects = new Dictionary<int, SubjectWithSubExams>();

            foreach (var student in failedStudents)
            {
                foreach (var failedSubject in student.FailedSubjects)
                {
                    if (!uniqueSubjects.ContainsKey(failedSubject.SubjectID))
                    {
                        uniqueSubjects[failedSubject.SubjectID] = new SubjectWithSubExams
                        {
                            SubjectID = failedSubject.SubjectID,
                            SubjectName = failedSubject.SubjectName,
                            SubExams = failedSubject.SubExams.ToList()
                        };
                    }
                    else
                    {
                        // Merge sub-exams if needed
                        var existingSubject = uniqueSubjects[failedSubject.SubjectID];
                        foreach (var subExam in failedSubject.SubExams)
                        {
                            if (!existingSubject.SubExams.Any(se => se.SubExamType == subExam.SubExamType))
                            {
                                existingSubject.SubExams.Add(subExam);
                            }
                        }
                    }
                }
            }

            return uniqueSubjects.Values.OrderBy(s => s.SubjectName).ToList();
        }

        private decimal GetDefaultPassMark(string subExamName)
        {
            if (string.IsNullOrEmpty(subExamName))
                return 33;

            var lowerName = subExamName.ToLower();

            // For sub-exams - estimate typical full marks and calculate 33%
            if (lowerName.Contains("creative"))
                return Math.Round(45 * 0.33m, 2); // Creative: typically 45 marks -> 14.85
            else if (lowerName.Contains("mcq"))
                return Math.Round(30 * 0.33m, 2); // MCQ: typically 30 marks -> 9.9
            else if (lowerName.Contains("cq"))
                return Math.Round(25 * 0.33m, 2); // CQ: typically 25 marks -> 8.25
            else if (lowerName.Contains("structured"))
                return Math.Round(60 * 0.33m, 2); // Structured: typically 60 marks -> 19.8
            else if (lowerName.Contains("practical"))
                return Math.Round(60 * 0.33m, 2); // Practical: typically 60 marks -> 19.8
            else if (lowerName.Contains("total"))
            {
                // For total marks, return default - will be overridden by dynamic calculation
                return Math.Round(100 * 0.33m, 2); // Default 100 marks -> 33
            }
            else
                return Math.Round(100 * 0.33m, 2); // Default 100 marks -> 33
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
            public string ObtainedMarks { get; set; } // Added for failed subject tracking
        }

        // New classes for precise failed student tracking
        [Serializable]
        public class FailedStudentData
        {
            public int StudentID { get; set; }
            public string StudentIDString { get; set; } // Added for ID field from Student table
            public string StudentName { get; set; }
            public List<FailedSubjectData> FailedSubjects { get; set; }
        }

        [Serializable]
        public class FailedSubjectData
        {
            public int SubjectID { get; set; }
            public string SubjectName { get; set; }
            public string TotalMarks { get; set; }
            public List<SubExamInfo> SubExams { get; set; }
        }
    }
}   
