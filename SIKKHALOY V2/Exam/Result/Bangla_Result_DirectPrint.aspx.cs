using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.Services;
using System.Threading;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Bangla_Result_DirectPrint : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Session["Group"] = GroupDropDownList.SelectedValue;
                Session["Shift"] = ShiftDropDownList.SelectedValue;
                Session["Section"] = SectionDropDownList.SelectedValue;

                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }
            }
            catch (ThreadAbortException)
            {
                // Do not catch ThreadAbortException
                throw;
            }
            catch (Exception ex)
            {
                // Log the error but don't show to user during page load
                System.Diagnostics.Debug.WriteLine("Page_Load Error: " + ex.Message);
            }
        }

        protected void UpdateDropdownVisibility()
        {
            try
            {
                DataView GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
                GroupDropDownList.Visible = GroupDV.Count > 0;

                DataView SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
                SectionDropDownList.Visible = SectionDV.Count > 0;

                DataView ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
                ShiftDropDownList.Visible = ShiftDV.Count > 0;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateDropdownVisibility Error: " + ex.Message);
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Session["Group"] = "%";
                Session["Shift"] = "%";
                Session["Section"] = "%";
                GroupDropDownList.DataBind();
                ShiftDropDownList.DataBind();
                SectionDropDownList.DataBind();
                ExamDropDownList.DataBind();
                UpdateDropdownVisibility();
                ResultPanel.Visible = false;
                
                // Reset page title when class changes
                Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitleClassChange",
                    "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';", true);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(typeof(Page), "error",
                    "console.error('Class selection error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDropdownVisibility();
            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
            {
                var groupValue = Session["Group"]?.ToString();
                if (!string.IsNullOrEmpty(groupValue) && GroupDropDownList.Items.FindByValue(groupValue) != null)
                    GroupDropDownList.Items.FindByValue(groupValue).Selected = true;
            }
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDropdownVisibility();
            ResultPanel.Visible = false;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
            {
                var sectionValue = Session["Section"]?.ToString();
                if (!string.IsNullOrEmpty(sectionValue) && SectionDropDownList.Items.FindByValue(sectionValue) != null)
                    SectionDropDownList.Items.FindByValue(sectionValue).Selected = true;
            }
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDropdownVisibility();
            ResultPanel.Visible = false;
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
            {
                var shiftValue = Session["Shift"]?.ToString();
                if (!string.IsNullOrEmpty(shiftValue) && ShiftDropDownList.Items.FindByValue(shiftValue) != null)
                    ShiftDropDownList.Items.FindByValue(shiftValue).Selected = true;
            }
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDropdownVisibility();
            ResultPanel.Visible = false;
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }

        protected void LoadResultsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0")
                {
                    // Add client-side debug information
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "debug1",
                        $"console.log('Loading results for Exam ID: {ExamDropDownList.SelectedValue}, Class ID: {ClassDropDownList.SelectedValue}');", true);

                    LoadResultsData();
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "alert", "alert('Please select both Class and Exam');", true);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(typeof(Page), "error",
                    "console.error('LoadResults Error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private void LoadResultsData()
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT DISTINCT
                        ers.StudentResultID,
                        ers.TotalExamObtainedMark_ofStudent,
                        ers.Student_Grade,
                        ers.Student_Point,
                        ers.Average,
                        ers.ObtainedPercentage_ofStudent,
                        ers.TotalMark_ofStudent,
                        ers.Position_InExam_Class,
                        ers.Position_InExam_Subsection,
                        CASE WHEN ers.Student_Grade = 'F' THEN 'Fail' ELSE 'Pass' END as PassStatus_ofStudent,
                        s.StudentsName,
                        ISNULL(s.ID, '') as ID,
                        ISNULL(s.StudentImageID, 0) as StudentImageID,
                        sc.RollNo,
                        cc.Class as ClassName,
                        ISNULL(cs.Section, '') as SectionName,
                        ISNULL(csh.Shift, '') as ShiftName,
                        ISNULL(csg.SubjectGroup, '') as GroupName,
                        en.ExamName,
                        ers.SchoolID,
                        sch.SchoolName,
                        sch.Address,
                        sch.Phone
                    FROM Exam_Result_of_Student ers
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    INNER JOIN Student s ON sc.StudentID = s.StudentID
                    INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
                    INNER JOIN Exam_Name en ON ers.ExamID = en.ExamID
                    INNER JOIN SchoolInfo sch ON ers.SchoolID = sch.SchoolID
                    LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
                    LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
                    LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
                    WHERE ers.ExamID = @ExamID
                    AND sc.ClassID = @ClassID
                    AND sc.SectionID LIKE @SectionID
                    AND sc.ShiftID LIKE @ShiftID
                    AND sc.SubjectGroupID LIKE @GroupID
                    AND ers.SchoolID = @SchoolID
                    AND ers.EducationYearID = @EducationYearID
                    ORDER BY sc.RollNo";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 30; // Increase timeout
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@GroupID", GroupDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.SelectCommand.CommandTimeout = 30;
                        adapter.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        // Load signatures separately
                        LoadSignatureImages();
                        
                        ResultRepeater.DataSource = dt;
                        ResultRepeater.DataBind();
                        ResultPanel.Visible = true;

                        // Update page title with dynamic student count
                        int studentCount = dt.Rows.Count;
                        string dynamicTitle = $"বাংলা রেজাল্ট কার্ড - মোট শিক্ষার্থী ( {studentCount} )";
                        
                        // Update page title using JavaScript
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "updateTitle",
                            $"document.getElementById('pageTitle').innerHTML = '{dynamicTitle}';", true);
                    }
                    else
                    {
                        ResultPanel.Visible = false;
                        
                        // Reset page title when no results
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitle",
                            "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';", true);
                        
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "nodata",
                            "alert('No results found for the selected criteria');", true);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (SqlException sqlEx)
            {
                ResultPanel.Visible = false;
                
                // Reset title on error
                Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitleError",
                    "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';", true);
                
                Page.ClientScript.RegisterStartupScript(typeof(Page), "sqlerror",
                    "console.error('Database Error: " + sqlEx.Message.Replace("'", "\\'") + "');", true);
            }
            catch (Exception ex)
            {
                ResultPanel.Visible = false;
                
                // Reset title on error
                Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitleError2",
                    "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';", true);
                
                Page.ClientScript.RegisterStartupScript(typeof(Page), "dberror",
                    "console.error('Error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        private void LoadSignatureImages()
        {
            SqlConnection con = null;
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadSignatureImages: Starting for SchoolID: {Session["SchoolID"]}");
                
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string signatureQuery = @"
                    SELECT 
                        CASE WHEN Teacher_Sign IS NOT NULL AND DATALENGTH(Teacher_Sign) > 0 THEN 1 ELSE 0 END as HasTeacherSign,
                        CASE WHEN Principal_Sign IS NOT NULL AND DATALENGTH(Principal_Sign) > 0 THEN 1 ELSE 0 END as HasPrincipalSign
                    FROM SchoolInfo 
                    WHERE SchoolID = @SchoolID";

                using (SqlCommand cmd = new SqlCommand(signatureQuery, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool hasTeacherSign = Convert.ToBoolean(reader["HasTeacherSign"]);
                            bool hasPrincipalSign = Convert.ToBoolean(reader["HasPrincipalSign"]);
                            
                            System.Diagnostics.Debug.WriteLine($"LoadSignatureImages: SchoolID: {Session["SchoolID"]}, HasTeacherSign: {hasTeacherSign}, HasPrincipalSign: {hasPrincipalSign}");
                            
                            // Add timestamp to avoid caching issues
                            string timestamp = DateTime.Now.Ticks.ToString();
                            
                            // Set paths to signature handler if signatures exist
                            HiddenTeacherSign.Value = hasTeacherSign ? 
                                $"/Handeler/SignatureHandler.ashx?type=teacher&schoolId={Session["SchoolID"]}&t={timestamp}" : "";
                            HiddenPrincipalSign.Value = hasPrincipalSign ? 
                                $"/Handeler/SignatureHandler.ashx?type=principal&schoolId={Session["SchoolID"]}&t={timestamp}" : "";
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"LoadSignatureImages: No SchoolInfo record found for SchoolID: {Session["SchoolID"]}");
                            // Set empty values if no school record found
                            HiddenTeacherSign.Value = "";
                            HiddenPrincipalSign.Value = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop the main process
                System.Diagnostics.Debug.WriteLine($"LoadSignatureImages error: {ex.Message}\nStack: {ex.StackTrace}");
                // Set empty values if error occurs
                HiddenTeacherSign.Value = "";
                HiddenPrincipalSign.Value = "";
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        protected void ResultRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Repeater gradingSystemRepeater = (Repeater)e.Item.FindControl("GradingSystemRepeater");
                if (gradingSystemRepeater != null)
                {
                    DataTable gradingData = GetGradingSystemData();
                    gradingSystemRepeater.DataSource = gradingData;
                    gradingSystemRepeater.DataBind();
                }
            }
        }

        // Update GetGradingSystemData to use the exact same TableAdapter as the official BanglaResult.aspx
        public DataTable GetGradingSystemData()
        {
            try
            {
                // Use the exact same TableAdapter that BanglaResult.aspx uses
                var tableAdapter = new EDUCATION.COM.Exam_ResultTableAdapters.Exam_Grading_SystemTableAdapter();
                
                int schoolID = Convert.ToInt32(Session["SchoolID"] ?? 1);
                int classID = Convert.ToInt32(ClassDropDownList.SelectedValue != "0" ? ClassDropDownList.SelectedValue : "1");
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue != "0" ? ExamDropDownList.SelectedValue : "1");
                int educationYearID = Convert.ToInt32(Session["Edu_Year"] ?? 1);

                // Call the same method that BanglaResult.aspx ObjectDataSource calls
                var gradingData = tableAdapter.GetData(schoolID, classID, examID, educationYearID);
                
                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradingTableAdapter", 
                    $"console.log('TableAdapter returned {gradingData.Rows.Count} grading rows');", true);

                // If we got data from TableAdapter, return it
                if (gradingData.Rows.Count > 0)
                {
                    // Log what we found
                    foreach (System.Data.DataRow row in gradingData.Rows)
                    {
                        string grade = row["Grades"]?.ToString() ?? "";
                        string comment = row["Comments"]?.ToString() ?? "";
                        string marks = row["MARKS"]?.ToString() ?? "";
                        Page.ClientScript.RegisterStartupScript(typeof(Page), $"gradingRow{grade}", 
                            $"console.log('TableAdapter Grade: {grade}, Comment: {comment}, Marks: {marks}');", true);
                    }
                    
                    return gradingData;
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "noGradingData", 
                        $"console.log('No grading data from TableAdapter, using default');", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TableAdapter GetGradingSystemData error: {ex.Message}");
                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradingTableAdapterError", 
                    $"console.error('TableAdapter error: {ex.Message}');", true);
            }

            // Fallback to default grading data if TableAdapter fails
            return GetDefaultGradingData();
        }

        private DataTable GetDefaultGradingData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Grades", typeof(string));
            dt.Columns.Add("MARKS", typeof(string));  // Changed to match TableAdapter format
            dt.Columns.Add("Comments", typeof(string));
            dt.Columns.Add("Point", typeof(decimal));

            dt.Rows.Add("A+", "80% - 100%", "Outstanding", 5.00);
            dt.Rows.Add("A", "70% - 79%", "Excellent", 4.00);
            dt.Rows.Add("A-", "60% - 69%", "Very Good", 3.50);
            dt.Rows.Add("B", "50% - 59%", "Good", 3.00);
            dt.Rows.Add("C", "40% - 49%", "Satisfactory", 2.00);
            dt.Rows.Add("D", "33% - 39%", "Acceptable", 1.00);
            dt.Rows.Add("F", "0% - 32%", "Fail", 0.00);

            return dt;
        }

        // Updated to fetch dynamic comments from database based on student's grade, school, exam settings
        public string GetResultStatus(string studentGrade, decimal gpa)
        {
            if (string.IsNullOrEmpty(studentGrade))
                return "ভালো";

            // First try to get comment from the TableAdapter grading data we already have
            string gradeChartComment = GetCommentFromGradingChart(studentGrade);
            if (!string.IsNullOrEmpty(gradeChartComment))
            {
                return gradeChartComment;
            }

            // Fallback to static comments based on your school's system
            switch (studentGrade.ToUpper())
            {
                case "A+": return "চমৎকার";
                case "A": return "ভালো"; 
                case "A-": return "মোটামুটি ভালো";
                case "B": return "বেশি ভালো নয়";
                case "C": return "সন্তোষজনক নয়";
                case "D": return "খুব খারাপ";
                case "F": return "অকৃতকার্য";
                default: return gpa >= 4.0m ? "চমৎকার" : "ভালো";
            }
        }

        private string GetCommentFromGradingChart(string studentGrade)
        {
            try
            {
                // Get the same grading data that we use for the chart (which now comes from TableAdapter)
                DataTable gradingData = GetGradingSystemData();
                
                foreach (DataRow row in gradingData.Rows)
                {
                    string gradeFromChart = row["Grades"]?.ToString()?.Trim() ?? "";
                    string commentFromChart = row["Comments"]?.ToString()?.Trim() ?? "";
                    
                    if (string.Equals(gradeFromChart, studentGrade, StringComparison.OrdinalIgnoreCase))
                    {
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "gradeFromChart", 
                            $"console.log('Found comment from TableAdapter: Grade={gradeFromChart}, Comment={commentFromChart}');", true);
                        
                        if (!string.IsNullOrEmpty(commentFromChart))
                        {
                            return commentFromChart;
                        }
                    }
                }
                
                Page.ClientScript.RegisterStartupScript(typeof(Page), "noGradeFromChart", 
                    $"console.log('No comment found in TableAdapter data for grade: {studentGrade}');", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCommentFromGradingChart error: {ex.Message}");
                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradeChartError", 
                    $"console.error('GetCommentFromGradingChart error: {ex.Message}');", true);
            }
            
            return string.Empty;
        }

        private string GetTableCssClass(int subjectCount)
        {
            if (subjectCount <= 6) return "";
            else if (subjectCount >= 7 && subjectCount <= 10) return "medium-subjects";
            else return "small-subjects";
        }

        private string GetSafeColumnValue(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                return row[columnName].ToString();
            return string.Empty;
        }

        private decimal GetSafeDecimalValue(DataRow row, string columnName)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                decimal value;
                if (decimal.TryParse(row[columnName].ToString(), out value))
                    return value;
            }
            return 0m;
        }

        public string GetSchoolName() => "Imperial Ideal School & College";
        public string GetSchoolAddress() => "761,Tulatulisohera Rd,Kalulkotil, Narayangonj | Phone: 01906-265260, 01789-752002 | Idealedu8@gmail.com";
        public string GetExamName() => ExamDropDownList.SelectedItem?.Text + " - " + DateTime.Now.Year;
        public string GetResult(object dataItem)
        {
            DataRowView row = (DataRowView)dataItem;
            return row["PassStatus_ofStudent"].ToString() == "Pass" ? "উত্তীর্ণ" : "অনুত্তীর্ণ";
        }

        private DataTable GetSubjectResults(string studentResultID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                string query = @"
                    SELECT 
                        ISNULL(sub.SubjectName, '') as SubjectName,
                        sub.SubjectID,
                        ISNULL(sub.SN, 999) as SubjectSN,
                        ISNULL(ERS.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
                        ISNULL(ERS.TotalMark_ofSubject, 0) as TotalMark_ofSubject,
                        ISNULL(ERS.SubjectGrades, '') as SubjectGrades,
                        ISNULL(ERS.SubjectPoint, 0) as SubjectPoint,
                        ISNULL(ERS.PassStatus_Subject, 'Pass') as PassStatus_Subject,
                        ISNULL(ERS.IS_Add_InExam, 1) as IS_Add_InExam
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
                    WHERE ers.StudentResultID = @StudentResultID
                    AND ISNULL(ers.IS_Add_InExam, 1) = 1
                    ORDER BY ISNULL(sub.SN, 999), sub.SubjectName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.SelectCommand.CommandTimeout = 15;
                        adapter.Fill(dt);
                    }

                    return dt;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch
            {
                return new DataTable();
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        // Method to check if the current exam has sub-exams
        private bool HasSubExams(int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                // Check if there are any sub-exam marks for this school, education year, class, and exam
                string query = @"
                    SELECT COUNT(DISTINCT eom.SubExamID) 
                    FROM Exam_Obtain_Marks eom
                    INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                    INNER JOIN StudentsClass sc ON eom.StudentResultID IN (
                        SELECT ers.StudentResultID 
                        FROM Exam_Result_of_Student ers 
                        INNER JOIN StudentsClass sc2 ON ers.StudentClassID = sc2.StudentClassID
                        WHERE ers.ExamID = @ExamID AND sc2.ClassID = @ClassID
                    )
                    WHERE eom.SchoolID = @SchoolID 
                    AND eom.EducationYearID = @EducationYearID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    System.Diagnostics.Debug.WriteLine($"HasSubExams: Found {count} sub-exam types for ClassID: {ClassDropDownList.SelectedValue}, ExamID: {examID}");
                    
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HasSubExams: {ex.Message}");
                return false;
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        // New method to get sub-exam marks for a subject using Exam_Obtain_Marks
        private DataTable GetSubExamMarks(string studentResultID, int subjectID, int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                // Use the exact same query as ExamPosition_WithSub.aspx without ExamID filter first
                string query = @"
                    SELECT 
                        Exam_SubExam_Name.SubExamName, 
                        ISNULL(CAST(Exam_Obtain_Marks.MarksObtained AS char(10)), 'A') as MarksObtained 
                    FROM Exam_Obtain_Marks 
                    INNER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID 
                    WHERE (Exam_Obtain_Marks.SchoolID = @SchoolID) 
                    AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) 
                    AND (Exam_Obtain_Marks.StudentResultID = @StudentResultID) 
                    AND (Exam_Obtain_Marks.SubjectID = @SubjectID)
                    ORDER BY Exam_SubExam_Name.Sub_ExamSN";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.SelectCommand.CommandTimeout = 15;
                        adapter.Fill(dt);
                    }

                    System.Diagnostics.Debug.WriteLine($"GetSubExamMarks: Subject {subjectID}: Found {dt.Rows.Count} sub-exam records for StudentResultID {studentResultID}");
                    
                    // Log the sub-exam details for debugging
                    foreach (DataRow row in dt.Rows)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Sub-exam: {row["SubExamName"]} = {row["MarksObtained"]}");
                    }

                    return dt;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamMarks: {ex.Message}");
                return new DataTable();
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        // Method to get the actual number of sub-exams for a specific class and exam
        private int GetSubExamCount(int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                // Count only sub-exams that have actual data for this class and exam
                string query = @"
                    SELECT COUNT(DISTINCT esn.SubExamID) 
                    FROM Exam_SubExam_Name esn
                    INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                    INNER JOIN Exam_Result_of_Student ers ON eom.StudentResultID = ers.StudentResultID
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    WHERE esn.SchoolID = @SchoolID
                    AND esn.EducationYearID = @EducationYearID
                    AND ers.ExamID = @ExamID
                    AND sc.ClassID = @ClassID
                    AND eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    System.Diagnostics.Debug.WriteLine($"GetSubExamCount: Found {count} active sub-exams for ClassID: {ClassDropDownList.SelectedValue}, ExamID: {examID}");
                    
                    return count;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamCount: {ex.Message}");
                return 0;
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        // Method to get sub-exam header names - only for sub-exams with actual data for this class
        private string GetSubExamHeaderNames(string studentResultID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                
                // Get only sub-exam names that have actual data for this class and exam
                string query = @"
                    SELECT DISTINCT esn.SubExamName, esn.Sub_ExamSN
                    FROM Exam_SubExam_Name esn
                    INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                    INNER JOIN Exam_Result_of_Student ers ON eom.StudentResultID = ers.StudentResultID
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    WHERE esn.SchoolID = @SchoolID
                    AND esn.EducationYearID = @EducationYearID
                    AND ers.ExamID = @ExamID
                    AND sc.ClassID = @ClassID
                    AND eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID
                    ORDER BY esn.Sub_ExamSN";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    System.Diagnostics.Debug.WriteLine($"GetSubExamHeaderNames: Found {dt.Rows.Count} active sub-exams for ClassDropDownList.SelectedValue: {ClassDropDownList.SelectedValue}");

                    string headerHtml = "";
                    foreach (DataRow row in dt.Rows)
                    {
                        headerHtml += $"<th>{row["SubExamName"]}</th>";
                        System.Diagnostics.Debug.WriteLine($"  Adding header: {row["SubExamName"]}");
                    }

                    return headerHtml;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamHeaderNames: {ex.Message}");
                return "";
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        // Method to get sub-exam marks formatted for display in separate cells - Only active sub-exams for this class
        private (string SubExamMarksCells, string TotalMarks) GetSubExamMarksForDisplay(string studentResultID, int subjectID, string originalObtainedMark)
        {
            try
            {
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                
                System.Diagnostics.Debug.WriteLine($"GetSubExamMarksForDisplay: Processing StudentResultID={studentResultID}, SubjectID={subjectID}, ExamID={examID}");
                
                // Get sub-exam marks for this specific subject and student
                DataTable subExamMarks = GetSubExamMarks(studentResultID, subjectID, examID);
                
                System.Diagnostics.Debug.WriteLine($"GetSubExamMarksForDisplay: Found {subExamMarks.Rows.Count} sub-exam marks");
                
                // Get the ordered sub-exam names for header consistency - only active ones for this class
                SqlConnection con2 = null;
                try
                {
                    con2 = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                    con2.Open();
                    
                    // Get only sub-exam names that have actual data for this class and exam
                    string headerQuery = @"
                        SELECT DISTINCT esn.SubExamName, esn.Sub_ExamSN
                        FROM Exam_SubExam_Name esn
                        INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                        INNER JOIN Exam_Result_of_Student ers ON eom.StudentResultID = ers.StudentResultID
                        INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                        WHERE esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID
                        AND ers.ExamID = @ExamID
                        AND sc.ClassID = @ClassID
                        AND eom.SchoolID = @SchoolID
                        AND eom.EducationYearID = @EducationYearID
                        ORDER BY esn.Sub_ExamSN";

                    using (SqlCommand cmd2 = new SqlCommand(headerQuery, con2))
                    {
                        cmd2.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                        cmd2.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                        cmd2.Parameters.AddWithValue("@ExamID", examID);
                        cmd2.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                        DataTable subExamHeaders = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd2))
                        {
                            adapter.Fill(subExamHeaders);
                        }

                        System.Diagnostics.Debug.WriteLine($"GetSubExamMarksForDisplay: Found {subExamHeaders.Rows.Count} active header names for ClassID: {ClassDropDownList.SelectedValue}");

                        // Create dictionary of actual marks
                        Dictionary<string, string> marksDict = new Dictionary<string, string>();
                        foreach (DataRow markRow in subExamMarks.Rows)
                        {
                            string subExamName = markRow["SubExamName"].ToString();
                            string markValue = markRow["MarksObtained"].ToString();
                            marksDict[subExamName] = markValue;
                            System.Diagnostics.Debug.WriteLine($"  Added to dict: {subExamName} = {markValue}");
                        }

                        // Generate cells in header order
                        string cellsHtml = "";
                        decimal totalMarks = 0;
                        bool hasValidMarks = false;
                        bool hasAbsentMarks = false;
                        foreach (DataRow headerRow in subExamHeaders.Rows)
                        {
                            string subExamName = headerRow["SubExamName"].ToString();
                            string markValue = marksDict.ContainsKey(subExamName) ? marksDict[subExamName] : "-";
                            
                            // Check if this is an absent mark (A) or 0 for absent student
                            if (markValue == "A")
                            {
                                markValue = "অনুপস্থিত";
                                hasAbsentMarks = true;
                            }
                            else if (markValue == "0" && IsStudentAbsent(studentResultID, subjectID))
                            {
                                markValue = "অনুপস্থিত";
                                hasAbsentMarks = true;
                            }
                            
                            cellsHtml += $"<td>{markValue}</td>";
                            System.Diagnostics.Debug.WriteLine($"  Generated cell for {subExamName}: {markValue}");
                            
                            // Calculate total if it's a valid numeric mark (not absent)
                            if (!string.IsNullOrEmpty(markValue) && 
                                markValue != "A" && 
                                markValue != "অনুপস্থিত" &&
                                markValue != "-" &&
                                decimal.TryParse(markValue, out decimal mark))
                            {
                                totalMarks += mark;
                                hasValidMarks = true;
                            }
                        }
                        
                        // Format the total marks display
                        string totalCell;
                        
                        // If student has absent marks in sub-exams, show "-" in total
                        if (hasAbsentMarks || IsStudentAbsent(studentResultID, subjectID))
                        {
                            totalCell = $"<td class=\"total-marks-cell\">-</td>";
                        }
                        else if (hasValidMarks)
                        {
                            totalCell = $"<td class=\"total-marks-cell\">{totalMarks}</td>";
                        }
                        else
                        {
                            // Format original marks - show "অনুপস্থিত" or "-" for absent
                            string formattedOriginalMark = FormatMarksDisplay(originalObtainedMark, studentResultID, subjectID);
                            if (formattedOriginalMark == "অনুপস্থিত")
                            {
                                totalCell = $"<td class=\"total-marks-cell\">-</td>";
                            }
                            else
                            {
                                totalCell = $"<td class=\"total-marks-cell\">{formattedOriginalMark}</td>";
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"GetSubExamMarksForDisplay: HasValidMarks: {hasValidMarks}, HasAbsentMarks: {hasAbsentMarks}, Total: {(hasValidMarks ? totalMarks.ToString() : "N/A")}");
                        System.Diagnostics.Debug.WriteLine($"GetSubExamMarksForDisplay: CellsHtml: {cellsHtml}");
                        
                        return (cellsHtml, totalCell);
                    }
                }
                finally
                {
                    if (con2 != null && con2.State == ConnectionState.Open)
                    {
                        con2.Close();
                        con2.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamMarksForDisplay: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // For error case, check if student is absent and show appropriate total
                string formattedOriginalMark = FormatMarksDisplay(originalObtainedMark, studentResultID, subjectID);
                string errorTotalCell = formattedOriginalMark == "অনুপস্থিত" ? 
                    $"<td class=\"total-marks-cell\">-</td>" : 
                    $"<td class=\"total-marks-cell\">{formattedOriginalMark}</td>";
                    
                return ("", errorTotalCell);
            }
        }

        public string GenerateSubjectMarksTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            try
            {
                DataTable subjects = GetSubjectResults(studentResultID);
                string resultComment = GetResultStatus(studentGrade, studentPoint);

                if (subjects.Rows.Count == 0)
                    return "<p>No subject data found</p>";

                string tableSizeClass = GetTableCssClass(subjects.Rows.Count);

                // Check if we have sub-exams to determine table structure
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                bool hasSubExams = HasSubExams(examID);
                string subExamHeader = "";
                int subExamCount = 0;

                string html = @"<table class=""marks-table " + tableSizeClass + @""">";

                if (hasSubExams)
                {
                    // Get actual sub-exam count and header names
                    subExamCount = GetSubExamCount(examID);
                    if (subExamCount > 0)
                    {
                        subExamHeader = GetSubExamHeaderNames(studentResultID);
                    }
                }

                // Create header row - Dynamic structure based on actual sub-exam count
                if (hasSubExams && subExamCount > 0 && !string.IsNullOrEmpty(subExamHeader))
                {
                    html += @"
                        <tr>
                            <th rowspan=""2"">বিষয়</th>
                            <th colspan=""" + subExamCount + @""">প্রাপ্ত নম্বর</th>
                            <th rowspan=""2"">মোট নম্বর</th>
                            <th rowspan=""2"">পূর্ণ নম্বর</th>
                            <th rowspan=""2"">গ্রেড</th>
                            <th rowspan=""2"">পয়েন্ট</th>
                            <th rowspan=""" + (subjects.Rows.Count + 2) + @""" class=""vertical-text"">" + resultComment + @"</th>
                        </tr>
                        <tr>" + subExamHeader + @"</tr>";
                }
                else
                {
                    // No sub-exams - Simple table structure
                    html += @"
                        <tr>
                            <th>বিষয়</th>
                            <th>প্রাপ্ত নম্বর</th>
                            <th>পূর্ণ নম্বর</th>
                            <th>গ্রেড</th>
                            <th>পয়েন্ট</th>
                            <th rowspan=""" + (subjects.Rows.Count + 1) + @""" class=""vertical-text"">" + resultComment + @"</th>
                        </tr>";
                }

                foreach (DataRow row in subjects.Rows)
                {
                    string subjectName = GetSafeColumnValue(row, "SubjectName");
                    string obtainedMark = GetSafeColumnValue(row, "ObtainedMark_ofSubject");
                    string fullMark = GetSafeColumnValue(row, "TotalMark_ofSubject");
                    string subjectGrades = GetSafeColumnValue(row, "SubjectGrades");
                    decimal subjectPoint = GetSafeDecimalValue(row, "SubjectPoint");
                    string passStatus = GetSafeColumnValue(row, "PassStatus_Subject");
                    int subjectID = Convert.ToInt32(GetSafeColumnValue(row, "SubjectID"));

                    if (passStatus == "") passStatus = "Pass";
                    string rowClass = passStatus == "Fail" ? "failed-row" : "";

                    // Format marks display - show 'অনুপস্থিত' for absent students instead of 0
                    string displayMark = FormatMarksDisplay(obtainedMark, studentResultID, subjectID);

                    if (hasSubExams && subExamCount > 0)
                    {
                        // Get dynamic sub-exam marks for this subject
                        var subExamData = GetSubExamMarksForDisplay(studentResultID, subjectID, obtainedMark);
                        
                        html += @"
                            <tr class=""" + rowClass + @""">
                                <td style=""text-align: left; padding-left: 12px;"">" + subjectName + @"</td>
                                " + subExamData.SubExamMarksCells + @"
                                " + subExamData.TotalMarks + @"
                                <td>" + fullMark + @"</td>
                                <td>" + subjectGrades + @"</td>
                                <td>" + subjectPoint.ToString("F1") + @"</td>
                            </tr>";
                    }
                    else
                    {
                        // No sub-exams - Simple row structure with formatted marks
                        html += @"
                            <tr class=""" + rowClass + @""">
                                <td style=""text-align: left; padding-left: 12px;"">" + subjectName + @"</td>
                                <td>" + displayMark + @"</td>
                                <td>" + fullMark + @"</td>
                                <td>" + subjectGrades + @"</td>
                                <td>" + subjectPoint.ToString("F1") + @"</td>
                            </tr>";
                    }
                }

                html += "</table>";
                return html;
            }
            catch (Exception ex)
            {
                return "<p>Error loading subject table: " + ex.Message + "</p>";
            }
        }

        [System.Web.Services.WebMethod]
        public static object SaveSignature(string signatureType, string imageData)
        {
            try
            {
                var context = HttpContext.Current;
                var schoolId = context.Session["SchoolID"];
                
                if (schoolId == null)
                {
                    return new { success = false, message = "School ID not found in session" };
                }

                // Convert base64 to byte array
                byte[] imageBytes = Convert.FromBase64String(imageData);

                SqlConnection con = null;
                try
                {
                    con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                    con.Open();

                    string column = signatureType.ToLower() == "teacher" ? "Teacher_Sign" : "Principal_Sign";
                    string updateQuery = $"UPDATE SchoolInfo SET {column} = @ImageData WHERE SchoolID = @SchoolID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ImageData", imageBytes);
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new { success = true, message = "Signature saved successfully", schoolId = schoolId };
                        }
                        else
                        {
                            return new { success = false, message = "No rows updated" };
                        }
                    }
                }
                finally
                {
                    if (con != null && con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        // Helper method to check if a specific section is selected (not ALL sections)
        protected bool IsSectionSelected()
        {
            string sectionValue = SectionDropDownList.SelectedValue;
            // Return true only if a specific section is selected (not % or empty)
            return !string.IsNullOrEmpty(sectionValue) && 
                   sectionValue != "%" && 
                   sectionValue.Trim() != "" &&
                   SectionDropDownList.Visible; // Also check if section dropdown is visible
        }

        // Helper method to get section column header and data for dynamic display
        protected string GetSectionColumnHeader()
        {
            return IsSectionSelected() ? "<td>শাখা মেধা</td>" : "";
        }

        protected string GetSectionColumnData(object dataItem)
        {
            if (!IsSectionSelected()) return "";
            
            DataRowView row = (DataRowView)dataItem;
            return "<td>" + row["Position_InExam_Subsection"] + "</td>";
        }

        // Helper method to check if the selected class has groups available
        protected bool HasGroupsForClass()
        {
            // Check if Group dropdown is visible (like summary table approach)
            return GroupDropDownList.Visible;
        }

        // Helper method to check if the selected class has sections available - based on dropdown visibility  
        protected bool HasSectionsForClass()
        {
            // Check if Section dropdown is visible (like summary table approach)
            return SectionDropDownList.Visible;
        }

        // Helper methods to get group and section row HTML for dynamic display
        protected string GetGroupRowHtml(object dataItem)
        {
            if (!HasGroupsForClass()) return "";
            
            DataRowView row = (DataRowView)dataItem;
            string groupName = row["GroupName"]?.ToString() ?? "";
            
            return @"
                <tr>
                    <td>গ্রুপ:</td>
                    <td>" + groupName + @"</td>
                    <td>শাখা:</td>
                    <td>" + row["SectionName"] + @"</td>
                </tr>";
        }

        protected string GetSectionOnlyRowHtml(object dataItem)
        {
            // This method is for when there's no group but there are sections
            if (HasGroupsForClass() || !HasSectionsForClass()) return "";
            
            DataRowView row = (DataRowView)dataItem;
            return @"
                <tr>
                    <td>শাখা:</td>
                    <td>" + row["SectionName"] + @"</td>
                    <td colspan=""2""></td>
                </tr>";
        }

        protected string GetNoGroupSectionRowHtml()
        {
            // This method is for when there's neither group nor section
            if (HasGroupsForClass() || HasSectionsForClass()) return "";
            
            return @"
                <tr style=""display: none;"">
                    <td colspan=""4""></td>
                </tr>";
        }

        // Helper method to generate dynamic info row based on class configuration
        protected string GetDynamicInfoRow(object dataItem)
        {
            DataRowView row = (DataRowView)dataItem;
            
            string className = row["ClassName"]?.ToString() ?? "";
            string groupName = row["GroupName"]?.ToString() ?? "";
            string sectionName = row["SectionName"]?.ToString() ?? "";
            
            // Check if this class has groups and sections
            bool hasGroups = HasGroupsForClass();
            bool hasSections = HasSectionsForClass();
            
            if (hasGroups)
            {
                // Show Class, Group, and Section
                return @"
                    <tr>
                        <td>ক্লাস:</td>
                        <td>" + className + @"</td>
                        <td>গ্রুপ:</td>
                        <td>" + groupName + @"</td>
                        <td>শাখা:</td>
                        <td>" + sectionName + @"</td>
                    </tr>";
            }
            else if (hasSections)
            {
                // Show Class and Section only
                return @"
                    <tr>
                        <td>ক্লাস:</td>
                        <td>" + className + @"</td>
                        <td>শাখা:</td>
                        <td>" + sectionName + @"</td>
                        <td colspan=""2""></td>
                    </tr>";
            }
            else
            {
                // Show Class only
                return @"
                    <tr>
                        <td>ক্লাস:</td>
                        <td>" + className + @"</td>
                        <td colspan=""4""></td>
                    </tr>";
            }
        }

        // Enhanced method to check if student was absent - with debugging
        private bool IsStudentAbsent(string studentResultID, int subjectID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                System.Diagnostics.Debug.WriteLine($"Checking absence for StudentResultID: {studentResultID}, SubjectID: {subjectID}");
                
                // Get the obtained marks for this subject
                string marksQuery = @"
                    SELECT ISNULL(ObtainedMark_ofSubject, '0') as ObtainedMarks
                    FROM Exam_Result_of_Subject 
                    WHERE StudentResultID = @StudentResultID 
                    AND SubjectID = @SubjectID";

                using (SqlCommand cmd = new SqlCommand(marksQuery, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                    object result = cmd.ExecuteScalar();
                    
                    if (result == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"No record found for StudentResultID: {studentResultID}, SubjectID: {subjectID} - marking as absent");
                        return true; // No record = absent
                    }
                    
                    string obtainedMarks = result.ToString().Trim();
                    System.Diagnostics.Debug.WriteLine($"Found marks: '{obtainedMarks}' for StudentResultID: {studentResultID}, SubjectID: {subjectID}");
                    
                    // Check for explicit absent markers
                    if (obtainedMarks.ToUpper() == "A" || 
                        obtainedMarks.ToUpper() == "AB" || 
                        obtainedMarks.ToLower() == "absent")
                    {
                        System.Diagnostics.Debug.WriteLine($"Explicit absent marker found: '{obtainedMarks}'");
                        return true;
                    }
                    
                    // If marks is 0, we need to determine if this is real 0 or absent
                    if (obtainedMarks == "0")
                    {
                        System.Diagnostics.Debug.WriteLine($"Found 0 marks for StudentResultID: {studentResultID}, SubjectID: {subjectID} - checking participation");
                        
                        // First check if this exam has sub-exams at all
                        int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                        bool examHasSubExams = HasSubExams(examID);
                        
                        System.Diagnostics.Debug.WriteLine($"Exam {examID} has sub-exams: {examHasSubExams}");
                        
                        if (examHasSubExams)
                        {
                            // Check if there are any sub-exam marks for this subject
                            string subExamQuery = @"
                                SELECT COUNT(*) 
                                FROM Exam_Obtain_Marks eom
                                WHERE eom.StudentResultID = @StudentResultID 
                                AND eom.SubjectID = @SubjectID
                                AND eom.SchoolID = @SchoolID
                                AND eom.EducationYearID = @EducationYearID
                                AND ISNULL(eom.MarksObtained, '') != ''
                                AND ISNULL(eom.MarksObtained, '') != 'A'";

                            using (SqlCommand cmd2 = new SqlCommand(subExamQuery, con))
                            {
                                cmd2.Parameters.AddWithValue("@StudentResultID", studentResultID);
                                cmd2.Parameters.AddWithValue("@SubjectID", subjectID);
                                cmd2.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                                cmd2.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                                int participationCount = Convert.ToInt32(cmd2.ExecuteScalar());
                                System.Diagnostics.Debug.WriteLine($"Sub-exam participation count: {participationCount}");
                                
                                // If no sub-exam participation and total is 0, consider absent
                                if (participationCount == 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"No sub-exam participation found - marking as absent");
                                    return true;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Sub-exam participation found - marking as real 0");
                                    return false; // Student participated but got 0
                                }
                            }
                        }
                        else
                        {
                            // No sub-exams in this exam, so 0 could mean absent
                            // Let's assume 0 without sub-exams means absent unless proven otherwise
                            System.Diagnostics.Debug.WriteLine($"No sub-exams in this exam - assuming 0 marks means absent");
                            return true;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Student not absent for StudentResultID: {studentResultID}, SubjectID: {subjectID}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in IsStudentAbsent: {ex.Message}");
                // In case of error, don't assume absent
                return false;
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        // Enhanced method that combines both approaches
        private bool IsStudentReallyAbsent(string studentResultID, int subjectID)
        {
            // Try primary method first
            bool absentPrimary = IsStudentAbsent(studentResultID, subjectID);
            
            // If primary method says absent, verify with alternative
            if (absentPrimary)
            {
                return IsStudentAbsent(studentResultID, subjectID);
            }
            
            // Also check if marks is exactly 0 and no participation
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                string zeroMarksQuery = @"
                    SELECT ObtainedMark_ofSubject 
                    FROM Exam_Result_of_Subject 
                    WHERE StudentResultID = @StudentResultID 
                    AND SubjectID = @SubjectID";

                using (SqlCommand cmd = new SqlCommand(zeroMarksQuery, con))
                {
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result.ToString().Trim() == "0")
                    {
                        // Check if this 0 is due to absence (no participation in any sub-exam)
                        return IsStudentAbsent(studentResultID, subjectID);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in IsStudentReallyAbsent: {ex.Message}");
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
            
            return false;
        }

        // Helper method to format marks display - show 'অনুপস্থিত' for absent, actual marks for others
        private string FormatMarksDisplay(string marks, string studentResultID, int subjectID)
        {
            // Check for explicit absent markers first
            if (!string.IsNullOrEmpty(marks))
            {
                string trimmedMarks = marks.Trim().ToUpper();
                if (trimmedMarks == "A" || trimmedMarks == "AB" || trimmedMarks == "ABSENT")
                {
                    return "অনুপস্থিত";
                }
            }
            
            // If marks is 0, check if student was really absent
            if (marks == "0")
            {
                if (IsStudentAbsent(studentResultID, subjectID))
                {
                    return "অনুপস্থিত";
                }
            }
            
            return marks;
        }
    }}