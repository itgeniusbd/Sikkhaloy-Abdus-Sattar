using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Result_Card_English : System.Web.UI.Page
    {
        // Attendance DTO and minimal data provider
        public class AttendanceData
        {
            public string WorkingDays { get; set; } = "";
            public string PresentDays { get; set; } = "";
            public string AbsentDays { get; set; } = "";
            public string LeaveDays { get; set; } = "";
            public string LateAbsDays { get; set; } = "";
            public string LateDays { get; set; } = "";
        }

        // Pagination properties
        private const int PageSize = 25; // 25 records per page

        private int CurrentPageIndex
        {
            get { return ViewState["CurrentPageIndex"] != null ? (int)ViewState["CurrentPageIndex"] : 0; }
            set { ViewState["CurrentPageIndex"] = value; }
        }

        private int TotalRecords
        {
            get { return ViewState["TotalRecords"] != null ? (int)ViewState["TotalRecords"] : 0; }
            set { ViewState["TotalRecords"] = value; }
        }

        private DataTable AllResultsData
        {
            get { return ViewState["AllResultsData"] as DataTable; }
            set { ViewState["AllResultsData"] = value; }
        }

        // Whether the current class/result set has sections
        private bool HasSections
        {
            get { return ViewState["HasSections"] != null && (bool)ViewState["HasSections"]; }
            set { ViewState["HasSections"] = value; }
        }

        // NEW: Publish Settings Properties
        private bool IS_Hide_Sec_Position
        {
            get { return ViewState["IS_Hide_Sec_Position"] != null && (bool)ViewState["IS_Hide_Sec_Position"]; }
            set { ViewState["IS_Hide_Sec_Position"] = value; }
        }

        private bool IS_Hide_Class_Position
        {
            get { return ViewState["IS_Hide_Class_Position"] != null && (bool)ViewState["IS_Hide_Class_Position"]; }
            set { ViewState["IS_Hide_Class_Position"] = value; }
        }

        private bool IS_Hide_FullMark
        {
            get { return ViewState["IS_Hide_FullMark"] != null && (bool)ViewState["IS_Hide_FullMark"]; }
            set { ViewState["IS_Hide_FullMark"] = value; }
        }

        private bool IS_Hide_PassMark
        {
            get { return ViewState["IS_Hide_PassMark"] != null && (bool)ViewState["IS_Hide_PassMark"]; }
            set { ViewState["IS_Hide_PassMark"] = value; }
        }

        private bool IS_Grade_BasePoint
        {
            get { return ViewState["IS_Grade_BasePoint"] != null && (bool)ViewState["IS_Grade_BasePoint"]; }
            set { ViewState["IS_Grade_BasePoint"] = value; }
        }

        // Remove problematic method calls that cause errors
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Simple page load initialization
                LoadPublishSettings();
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
                // Hide print button and reset title when class changes
                // SafeRegisterStartupScript("hidePrintOnClassChange", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';");

                // SafeRegisterStartupScript("resetTitleClassChange", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");

                // Clear results when class changes
                ResultPanel.Visible = false;
            }
            catch (Exception ex)
            {
                // Simple error handling
                // string safeErrorMessage = EscapeForJavaScript(ex.Message);
                // SafeRegisterStartupScript("error", $"console.error('Class selection error: {safeErrorMessage}');");
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
                ResultPanel.Visible = false;

                if (ClassDropDownList.SelectedIndex <= 0 || ExamDropDownList.SelectedIndex <= 0)
                {
                    string studentIDText = StudentIDTextBox.Text.Trim();

                    if (!string.IsNullOrEmpty(studentIDText))
                    {
                        // For student ID search, we need class and exam
                        return;
                    }
                    else
                    {
                        // For class-wise search, we need both class and exam
                        return;
                    }
                }

                LoadResultsData();
            }
            catch (Exception ex)
            {
                // Simple error handling
                System.Diagnostics.Debug.WriteLine($"LoadResults Error: {ex.Message}");
            }
        }

        private void LoadResultsData()
        {
            try
            {
                string studentIDText = StudentIDTextBox.Text.Trim();
                DataTable dt;

                if (!string.IsNullOrEmpty(studentIDText))
                {
                    var studentIDs = studentIDText.Split(',').Select(id => id.Trim()).Where(id => !string.IsNullOrEmpty(id)).ToList();
                    dt = LoadStudentsByIDs(studentIDs);
                }
                else
                {
                    dt = LoadStudentsByClass();
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    // Determine if this class has any sections in the loaded data
                    HasSections = dt.AsEnumerable().Any(r => !string.IsNullOrWhiteSpace(r.Field<string>("SectionName")));

                    // Store all data for pagination
                    AllResultsData = dt;
                    TotalRecords = dt.Rows.Count;
                    CurrentPageIndex = 0; // Reset to first page

                    // Load publish settings BEFORE loading signatures
                    LoadPublishSettings();

                    // Load signatures separately
                    LoadSignatureImages();

                    // Bind paginated data
                    BindResultsToRepeater(dt);
                    ResultPanel.Visible = true;

                    // Show simple print button when results are available
                    SafeRegisterStartupScript("showPrintButton", "document.getElementById('PrintButton').style.display = 'inline-block';");

                    // Update page title with dynamic student count - use safe JavaScript
                    int studentCount = dt.Rows.Count;
                    string sectionInfo = HasSections ? "With Sections" : "No Sections";
                    string dynamicTitle = EscapeForJavaScript($"English Result Card - Total Students ( {studentCount} ) - {sectionInfo}");

                    // Update page title using JavaScript
                    SafeRegisterStartupScript("updateTitle", $"document.getElementById('pageTitle').innerHTML = '{dynamicTitle}';");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No data found - showing message");
                    ResultPanel.Visible = false;
                    SafeRegisterStartupScript("hidePrintButton", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';");

                    SafeRegisterStartupScript("resetTitle", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");

                    string noResultsMessage = !string.IsNullOrEmpty(studentIDText) ?
                        $"No published results found for Student IDs: {studentIDText}" :
                        "No published results found for the selected criteria. Please check:\n1. Results are published\n2. Class and Exam are correctly selected";

                    SafeRegisterStartupScript("nodata", $"alert('{EscapeForJavaScript(noResultsMessage)}');");
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error: {sqlEx.Message}");
                ResultPanel.Visible = false;
                SafeRegisterStartupScript("resetTitleError", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");
                SafeRegisterStartupScript("sqlerror", $"alert('Database Error: Please check if result is published for this exam.');");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}\nStack: {ex.StackTrace}");
                ResultPanel.Visible = false;
                SafeRegisterStartupScript("resetTitleError2", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");
                SafeRegisterStartupScript("dberror", $"alert('Error loading results. Please try again.');");
            }
        }

        // NEW: Load publish settings from Exam_Publish_Setting table
        private void LoadPublishSettings()
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT TOP 1
                        ISNULL(IS_Hide_Sec_Position, 0) AS IS_Hide_Sec_Position,
                        ISNULL(IS_Hide_Class_Position, 0) AS IS_Hide_Class_Position,
                        ISNULL(IS_Hide_FullMark, 0) AS IS_Hide_FullMark,
                        ISNULL(IS_Hide_PassMark, 0) AS IS_Hide_PassMark,
                        ISNULL(IS_Grade_BasePoint, 0) AS IS_Grade_BasePoint
                    FROM Exam_Publish_Setting
                    WHERE SchoolID = @SchoolID
                    AND EducationYearID = @EducationYearID
                    AND ClassID = @ClassID
                    AND ExamID = @ExamID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            IS_Hide_Sec_Position = Convert.ToBoolean(reader["IS_Hide_Sec_Position"]);
                            IS_Hide_Class_Position = Convert.ToBoolean(reader["IS_Hide_Class_Position"]);
                            IS_Hide_FullMark = Convert.ToBoolean(reader["IS_Hide_FullMark"]);
                            IS_Hide_PassMark = Convert.ToBoolean(reader["IS_Hide_PassMark"]);
                            IS_Grade_BasePoint = Convert.ToBoolean(reader["IS_Grade_BasePoint"]);
                        }
                        else
                        {
                            IS_Hide_Sec_Position = false;
                            IS_Hide_Class_Position = false;
                            IS_Hide_FullMark = false;
                            IS_Hide_PassMark = false;
                            IS_Grade_BasePoint = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IS_Hide_Sec_Position = false;
                IS_Hide_Class_Position = false;
                IS_Hide_FullMark = false;
                IS_Hide_PassMark = false;
                IS_Grade_BasePoint = false;
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

        // Helper method to generate attendance + summary table for a student
        public string GetAttendanceTableHtml(object dataItem)
        {
            var row = (DataRowView)dataItem;

            string obtainedMarks = "0";
            string totalMarks = "0";
            string percentage = "0.00";
            string average = "0.00";
            string grade = "F";
            string gpa = "0.0";
            string positionClass = "-";
            string positionSection = "-";
            string comment = "Good";

            try
            {
                // Get values from the row with proper null checking using correct field names
                obtainedMarks = row["TotalExamObtainedMark_ofStudent"] == DBNull.Value ? "0" :
                    Convert.ToDecimal(row["TotalExamObtainedMark_ofStudent"]).ToString("F1");

                totalMarks = row["TotalMark_ofStudent"] == DBNull.Value ? "0" :
                    Convert.ToDecimal(row["TotalMark_ofStudent"]).ToString("F0");

                percentage = row["ObtainedPercentage_ofStudent"] == DBNull.Value ? "0.00" :
                    Convert.ToDecimal(row["ObtainedPercentage_ofStudent"]).ToString("F2");

                average = row["Average"] == DBNull.Value ? "0.00" :
                    Convert.ToDecimal(row["Average"]).ToString("F2");

                grade = row["Student_Grade"] == DBNull.Value ? "F" : row["Student_Grade"].ToString();

                gpa = row["Student_Point"] == DBNull.Value ? "0.0" :
                    Convert.ToDecimal(row["Student_Point"]).ToString("F1");

                // Position calculations
                int posClassInt = row["Position_InExam_Class"] == DBNull.Value ? 0 :
                    Convert.ToInt32(row["Position_InExam_Class"]);
                int posSectionInt = row["Position_InExam_Subsection"] == DBNull.Value ? 0 :
                    Convert.ToInt32(row["Position_InExam_Subsection"]);

                positionClass = posClassInt > 0 ? ToOrdinal(posClassInt) : "-";
                positionSection = posSectionInt > 0 ? ToOrdinal(posSectionInt) : "-";

                // Get comment based on grade and GPA
                decimal studentPoint = row["Student_Point"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Student_Point"]);
                comment = GetResultStatus(grade, studentPoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing student data: {ex.Message}");
            }

            string studentResultID = row["StudentResultID"]?.ToString() ?? string.Empty;
            int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);

            // Get real attendance data
            var attendanceInfo = GetAttendanceData(studentResultID, examID);

            // Create proper marks display (obtained/total)
            string marksDisplay = $"{obtainedMarks}/{totalMarks}";

            // Build conditional PC header and data based on IS_Hide_Class_Position
            string pcHeader = !IS_Hide_Class_Position ?
                "<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #e19511; color: #000; min-width: 25px;\" title=\"Position In Class\">PC</td>" :
                string.Empty;
            string pcData = !IS_Hide_Class_Position ?
                $"<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #0; color: #000; min-width: 25px;\" title=\"{positionClass}\">{positionClass}</td>" :
                string.Empty;

            // Build conditional PS header and data based on HasSections AND IS_Hide_Sec_Position
            string psHeader = (HasSections && !IS_Hide_Sec_Position) ?
                "<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #339f03; color: #000; min-width: 25px;\">PS</td>" :
                string.Empty;
            string psData = (HasSections && !IS_Hide_Sec_Position) ?
                $"<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;\" title=\"{positionSection}\">{positionSection}</td>" :
                string.Empty;

            string html = $@"<table class=""summary"" style=""width: 100%; border-collapse: collapse; margin-top: 5px; font-family: Arial, sans-serif;"">
                    <tr class=""summary-header"">
                        <td>WD</td>
                        <td>Pre</td>
                        <td>Abs</td>
                        <td>L Abs</td>
                        <td>Leave</td>
                        <td>Late</td>
                        <td>Obtained Marks</td>
                        <td>%</td>
                        <td>Average</td>
                        <td>Grade</td>
                        <td>GPA</td>
                        {pcHeader}
                        {psHeader}
                        <td>Comment</td>
                    </tr>
                    <tr class=""summary-values"">
                        <td>{attendanceInfo.WorkingDays}</td>
                        <td>{attendanceInfo.PresentDays}</td>
                        <td>{attendanceInfo.AbsentDays}</td>
                        <td>{attendanceInfo.LateAbsDays}</td>
                        <td>{attendanceInfo.LeaveDays}</td>
                        <td>{attendanceInfo.LateDays}</td>
                        <td>{marksDisplay}</td>
                        <td>{percentage}%</td>
                        <td>{average}</td>
                        <td>{grade}</td>
                        <td>{gpa}</td>
                        {pcData}
                        {psData}
                        <td>{comment}</td>
                    </tr>
                </table>";
            return html;
        }

        private void BindResultsToRepeater(DataTable dt
        )
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                ResultRepeater.DataSource = null;
                ResultRepeater.DataBind();
                UpdatePaginationControls();
                return;
            }

            // Calculate pagination
            int startIndex = CurrentPageIndex * PageSize;
            int endIndex = Math.Min(startIndex + PageSize, dt.Rows.Count);

            // Create a new DataTable with only the current page data
            DataTable pageData = dt.Clone();
            for (int i = startIndex; i < endIndex; i++)
            {
                pageData.ImportRow(dt.Rows[i]);
            }

            // Bind to repeater
            ResultRepeater.DataSource = pageData;
            ResultRepeater.DataBind();

            // Update pagination controls
            UpdatePaginationControls();
        }

        private void UpdatePaginationControls()
        {
            if (TotalRecords == 0)
            {
                PaginationInfoLabel.Text = "No students found";
                PageInfoLabel.Text = "Page 0 of 0";

                FirstPageButton.Enabled = false;
                PrevPageButton.Enabled = false;
                NextPageButton.Enabled = false;
                LastPageButton.Enabled = false;

                // Hide print button when no results
                Page.ClientScript.RegisterStartupScript(typeof(Page), "hidePrintButton",
                    "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';", true);
                return;
            }

            int totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
            int currentPage = CurrentPageIndex + 1;
            int startRecord = (CurrentPageIndex * PageSize) + 1;
            int endRecord = Math.Min(startRecord + PageSize - 1, TotalRecords);

            // Update info labels - using English text for English Result Card
            PaginationInfoLabel.Text = $"Loaded {startRecord} to {endRecord} students. Total {TotalRecords} students";
            PageInfoLabel.Text = $"Page {currentPage} of {totalPages}";

            // Enable/disable buttons
            FirstPageButton.Enabled = CurrentPageIndex > 0;
            PrevPageButton.Enabled = CurrentPageIndex > 0;
            NextPageButton.Enabled = CurrentPageIndex < (totalPages - 1);
            LastPageButton.Enabled = CurrentPageIndex < (totalPages - 1);

            // Show print button when there are results
            Page.ClientScript.RegisterStartupScript(typeof(Page), "showPrintButton",
                "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'inline-block';", true);
        }

        protected void NextPageButton_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
            if (CurrentPageIndex < totalPages - 1)
            {
                CurrentPageIndex++;
                BindResultsToRepeater(AllResultsData);
            }
        }

        protected void PrevPageButton_Click(object sender, EventArgs e)
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                BindResultsToRepeater(AllResultsData);
            }
        }

        protected void FirstPageButton_Click(object sender, EventArgs e)
        {
            CurrentPageIndex = 0;
            BindResultsToRepeater(AllResultsData);
        }

        protected void LastPageButton_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
            CurrentPageIndex = Math.Max(0, totalPages - 1);
            BindResultsToRepeater(AllResultsData);
        }

        private void LoadSignatureImages()
        {
            SqlConnection con = null;
            try
            {
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

                            string timestamp = DateTime.Now.Ticks.ToString();

                            HiddenTeacherSign.Value = hasTeacherSign ?
                                $"/Handeler/SignatureHandler.ashx?type=teacher&schoolId={Session["SchoolID"]}&t={timestamp}" : "";
                            HiddenPrincipalSign.Value = hasPrincipalSign ?
                                $"/Handeler/SignatureHandler.ashx?type=principal&schoolId={Session["SchoolID"]}&t={timestamp}" : "";
                        }
                        else
                        {
                            HiddenTeacherSign.Value = "";
                            HiddenPrincipalSign.Value = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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

                // If we got data from TableAdapter, return it
                if (gradingData.Rows.Count > 0)
                {
                    // Log what we found
                    foreach (System.Data.DataRow row in gradingData.Rows)
                    {
                        string grade = row["Grades"]?.ToString() ?? "";
                        string comment = row["Comments"]?.ToString() ?? "";
                        string marks = row["MARKS"]?.ToString() ?? "";
                    }

                    return gradingData;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TableAdapter GetGradingSystemData error: {ex.Message}");
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

        // UPDATED: to fetch dynamic comments from database based on student's grade, school, exam settings
        public string GetResultStatus(string studentGrade, decimal gpa)
        {
            if (string.IsNullOrEmpty(studentGrade))
                return "Good";

            // First try to get comment from the TableAdapter grading data we already have
            string gradeChartComment = GetCommentFromGradingChart(studentGrade);
            if (!string.IsNullOrEmpty(gradeChartComment))
            {
                return gradeChartComment;
            }

            // Fallback to static comments based on your school's system - using English for English Result Card
            switch (studentGrade.ToUpper())
            {
                case "A+": return "Outstanding";
                case "A": return "Excellent";
                case "A-": return "Very Good";
                case "B": return "Good";
                case "C": return "Satisfactory";
                case "D": return "Acceptable";
                case "F": return "Fail";
                default: return gpa >= 4.0m ? "Outstanding" : "Good";
            }
        }

        private string GetCommentFromGradingChart(string studentGrade)
        {
            try
            {
                // Get the grading data that we use for the chart (which now comes from TableAdapter)
                DataTable gradingData = GetGradingSystemData();

                foreach (DataRow row in gradingData.Rows)
                {
                    string gradeFromChart = row["Grades"]?.ToString() ?? "";
                    string commentFromChart = row["Comments"]?.ToString() ?? "";

                    if (string.Equals(gradeFromChart, studentGrade, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(commentFromChart))
                        {
                            return commentFromChart;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCommentFromGradingChart error: {ex.Message}");
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
            if (row.Table.Columns.Contains(columnName) &&
                row[columnName] != DBNull.Value &&
                row[columnName] != null)
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
            var passStatus = row["PassStatus_ofStudent"];

            // Handle DBNull values
            if (passStatus == DBNull.Value || passStatus == null)
            {
                return "N/A";
            }

            return passStatus.ToString() == "Pass" ? "Pass" : "Fail";
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
                        CASE 
                            WHEN ISNULL(sfg.SubjectType, '') = 'Optional' 
                            THEN ISNULL(sub.SubjectName, '') + ' *'
                            ELSE ISNULL(sub.SubjectName, '') 
                        END as SubjectName,
                        sub.SubjectID,
                        ISNULL(sub.SN, 999) as SubjectSN,
                        ISNULL(ERS.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
                        ISNULL(ERS.TotalMark_ofSubject, 0) as TotalMark_ofSubject,
                        ISNULL(ERS.SubjectGrades, '') as SubjectGrades,
                        ISNULL(ERS.SubjectPoint, 0) as SubjectPoint,
                        ISNULL(ERS.PassStatus_Subject, 'Pass') as PassStatus_Subject,
                        ISNULL(ERS.IS_Add_InExam, 1) as IS_Add_InExam,
                        ISNULL(ERS.HighestMark_InSubject_Class, 0) as HighestMark_InSubject_Class,
                        ISNULL(ERS.HighestMark_InSubject_Subsection, 0) as HighestMark_InSubject_Subsection
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    LEFT JOIN SubjectForGroup sfg ON sub.SubjectID = sfg.SubjectID 
                        AND sc.ClassID = sfg.ClassID 
                        AND sc.SubjectGroupID = sfg.SubjectGroupID
                        AND ers.SchoolID = sfg.SchoolID
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
            catch (Exception ex)
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

        // NEW: Get pass mark for subjects without sub-exams
        private string GetMainExamPassMark(int subjectID, int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT TOP 1 
                        CASE 
                            WHEN TotalMark_ofSubject > 0 THEN CAST(TotalMark_ofSubject * 0.33 AS INT)
                            ELSE 33
                        END as CalculatedPassMark
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    WHERE ers.SubjectID = @SubjectID 
                    AND erst.ExamID = @ExamID 
                    AND sc.ClassID = @ClassID
                    AND ers.SchoolID = @SchoolID 
                    AND ers.EducationYearID = @EducationYearID
                    AND ers.TotalMark_ofSubject > 0
                    ORDER BY ers.TotalMark_ofSubject DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "33";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetMainExamPassMark: {ex.Message}");
                return "33";
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

        // NEW: Helper to generate dynamic info rows for the Student Info table
        // Renders Class (always) and optional Section/Group/Shift in compact rows (max 2 pairs per row)
        public string GetDynamicInfoRow(object dataItem)
        {
            try
            {
                var row = dataItem as DataRowView;
                if (row == null) return string.Empty;

                string className = row.Row.Table.Columns.Contains("ClassName") ? row["ClassName"]?.ToString() ?? string.Empty : string.Empty;
                string sectionName = row.Row.Table.Columns.Contains("SectionName") ? row["SectionName"]?.ToString() ?? string.Empty : string.Empty;
                string groupName = row.Row.Table.Columns.Contains("GroupName") ? row["GroupName"]?.ToString() ?? string.Empty : string.Empty;
                string shiftName = row.Row.Table.Columns.Contains("ShiftName") ? row["ShiftName"]?.ToString() ?? string.Empty : string.Empty;

                // Build label-value pairs
                var pairs = new List<Tuple<string, string>>();
                if (!string.IsNullOrWhiteSpace(className))
                    pairs.Add(Tuple.Create("Class:", className));
                if (!string.IsNullOrWhiteSpace(sectionName))
                    pairs.Add(Tuple.Create("Section:", sectionName));
                if (!string.IsNullOrWhiteSpace(groupName))
                    pairs.Add(Tuple.Create("Group:", groupName));
                if (!string.IsNullOrWhiteSpace(shiftName))
                    pairs.Add(Tuple.Create("Shift:", shiftName));

                // If nothing to show, add just class row
                if (pairs.Count == 0)
                    return "<tr><td>Class:</td><td colspan=\"3\"><b>-</b></td></tr>";

                var sb = new StringBuilder();
                int i = 0;
                while (i < pairs.Count)
                {
                    sb.Append("<tr>");

                    // Add first pair
                    string label1 = pairs[i].Item1;
                    string value1 = HttpUtility.HtmlEncode(pairs[i].Item2);
                    sb.AppendFormat("<td>{0}</td><td><b>{1}</b></td>", label1, value1);
                    i++;

                    // Add second pair if it exists
                    if (i < pairs.Count)
                    {
                        string label2 = pairs[i].Item1;
                        string value2 = HttpUtility.HtmlEncode(pairs[i].Item2);
                        sb.AppendFormat("<td>{0}</td><td><b>{1}</b></td>", label2, value2);
                        i++;
                    }
                    else
                    {
                        // If only one pair in the row, add empty cells to complete the 4-column layout
                        sb.Append("<td></td><td></td>");
                    }

                    sb.Append("</tr>");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDynamicInfoRow: {ex.Message}");
                return "<tr><td>Class:</td><td colspan=\"3\"><b>-</b></td></tr>";
            }
        }

        // Generate the subject marks table HTML (used by ASPX markup)
        public string GenerateSubjectMarksTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            try
            {
                DataTable subjects = GetSubjectResults(studentResultID);
                if (subjects.Rows.Count == 0)
                    return "<p>No subject data found</p>";

                string html = @"<div style=""overflow-x: auto; width: 100%;""><table class=""marks-table"" style=""font-size: 11px; font-family: Arial, sans-serif; border-collapse: collapse; width: 100%;"">";

                // Check if we have sub-exam structure by looking at first subject
                bool hasSubExamStructure = false;
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);

                // Simple check - do we have sub-exam data?
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string checkQuery = @"SELECT COUNT(*) FROM Exam_Obtain_Marks WHERE ExamID = @ExamID AND SchoolID = @SchoolID";
                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ExamID", examID);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                        hasSubExamStructure = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }

                if (hasSubExamStructure)
                {
                    // Complex table with sub-exams
                    html += BuildSubExamTable(subjects, studentResultID, examID);
                    html += @"</table></div>";
                }
                else
                {
                    // No sub-exams - simple table structure
                    string tableSizeClass = GetTableCssClass(subjects.Rows.Count);
                    string standardCellStyle = "border: 1px solid #0072bc; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; font-size: 11px; font-family: Arial, sans-serif;";

                    html += $@"<tr class=""marks-header"" style=""background-color: #e8f4fd;"">
                            <th style=""{standardCellStyle}; min-width: 120px; background-color: #e8f4fd; font-weight: bold;"">Subject</th>
                            <th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">OM</th>";

                    // Add Full Mark column conditionally
                    if (!IS_Hide_FullMark)
                    {
                        html += $@"<th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">FM</th>";
                    }

                    // Add Pass Mark column conditionally  
                    if (!IS_Hide_PassMark)
                    {
                        html += $@"<th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PM</th>";
                    }

                    html += $@"<th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">Grade</td>
                            <th style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">Point</th>";

                    // Position columns - Only show PC if not hidden
                    if (!IS_Hide_Class_Position)
                    {
                        html += $@"<th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PC</th>";
                    }

                    // PS column - Only show if HasSections is true AND IS_Hide_Sec_Position is false
                    if (HasSections && !IS_Hide_Sec_Position)
                    {
                        html += $@"<th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PS</th>";
                    }

                    html += $@"<th style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMC</th>";

                    // HMS column - Only show if HasSections is true AND IS_Hide_Sec_Position is false  
                    if (HasSections && !IS_Hide_Sec_Position)
                    {
                        html += $@"<th style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMS</th>";
                    }

                    html += @"</tr>";

                    // Data rows
                    foreach (DataRow row in subjects.Rows)
                    {
                        string subjectName = GetSafeColumnValue(row, "SubjectName");
                        decimal obtainedMark = GetSafeDecimalValue(row, "ObtainedMark_ofSubject");
                        decimal fullMark = GetSafeDecimalValue(row, "TotalMark_ofSubject");
                        string subjectGrades = GetSafeColumnValue(row, "SubjectGrades");
                        decimal subjectPointVal = GetSafeDecimalValue(row, "SubjectPoint");
                        string passStatus = GetSafeColumnValue(row, "PassStatus_Subject");

                        if (passStatus == "") passStatus = "Pass";
                        bool isFailed = (subjectGrades.ToUpper() == "F" || passStatus == "Fail");
                        string rowClass = isFailed ? "failed-row" : "";

                        // Student is absent ONLY if marks are 0 AND no grade is assigned
                        bool isAbsent = (obtainedMark == 0 && string.IsNullOrWhiteSpace(subjectGrades));
                        
                        // FIXED: Show decimal for obtained marks if it has fractional part
                        string displayMark = isAbsent ? "Abs" : 
                            (obtainedMark % 1 != 0 ? obtainedMark.ToString("F1") : obtainedMark.ToString("F0"));

                        // Show actual grade from database
                        string displayGrade = string.IsNullOrWhiteSpace(subjectGrades) ? "-" : subjectGrades;

                        // Get position data
                        int subjectID = 0;
                        int.TryParse(GetSafeColumnValue(row, "SubjectID"), out subjectID);
                        var positionData = GetSubjectPositionDataForResult(studentResultID, subjectID);
                        string positionClass = positionData.PositionClass;
                        string positionSection = positionData.PositionSection;

                        // Get highest marks data
                        decimal hmcValue = GetSafeDecimalValue(row, "HighestMark_InSubject_Class");
                        decimal hmsValue = GetSafeDecimalValue(row, "HighestMark_InSubject_Subsection");

                        html += $@"<tr class=""{rowClass}"" style=""border: 1px solid #0072bc;"">
                                <td style=""{standardCellStyle}; text-align: left; padding-left: 12px;"">{subjectName}</td>
                                <td style=""{standardCellStyle}"">{displayMark}</td>";

                        // Full Mark column
                        if (!IS_Hide_FullMark)
                        {
                            html += $@"<td style=""{standardCellStyle}"">{fullMark:F0}</td>";
                        }

                        // Pass Mark column
                        if (!IS_Hide_PassMark)
                        {
                            decimal passMark = fullMark * 0.33m;
                            html += $@"<td style=""{standardCellStyle}"">{passMark:F0}</td>";
                        }

                        html += $@"<td style=""{standardCellStyle}"">{displayGrade}</td>
                                <td style=""{standardCellStyle}"">{subjectPointVal:F1}</td>";

                        // Position columns
                        if (!IS_Hide_Class_Position)
                        {
                            html += $@"<td style=""{standardCellStyle}"">{positionClass}</td>";
                        }

                        if (HasSections && !IS_Hide_Sec_Position)
                        {
                            html += $@"<td style=""{standardCellStyle}"">{positionSection}</td>";
                        }

                        html += $@"<td style=""{standardCellStyle}"">{(hmcValue > 0 ? hmcValue.ToString("F0") : "-")}</td>";

                        if (HasSections && !IS_Hide_Sec_Position)
                        {
                            html += $@"<td style=""{standardCellStyle}"">{(hmsValue > 0 ? hmsValue.ToString("F0") : "-")}</td>";
                        }

                        html += @"</tr>";
                    }

                    html += @"</table></div>";
                }

                return html;
            }
            catch (Exception ex)
            {
                return "<p>Error loading subject table: " + ex.Message + "</p>";
            }
        }

        private string BuildSubExamTable(DataTable subjects, string studentResultID, int examID)
        {
            string html = "";
            string cellStyle = "border: 1px solid #000; padding: 3px; text-align: center; font-size: 11px; min-width: 35px; max-width: 45px;";

            // Get sub-exam names dynamically from database
            List<string> subExamNames = GetSubExamNames(examID);
            int subExamCount = subExamNames.Count;

            // Build header - Only add PC header if not hidden
            html += @"<tr style=""background-color: #c8e6c9;"">
                <th rowspan=""2"" style=""" + cellStyle + @"text-align: left; min-width: 80px; max-width: 120px; background-color: #c8e6c9;"">SUBJECTS</th>";

            // Add dynamic sub-exam headers
            foreach (string subExamName in subExamNames)
            {
                html += $@"<th colspan=""3"" style=""{cellStyle}background-color: #c8e6c9;"">{subExamName}</th>";
            }

            html += @"<th rowspan=""2"" style=""" + cellStyle + @"background-color: #c8e6c9; min-width: 70px;"">MARKS</th>
                <th rowspan=""2"" style=""" + cellStyle + @"background-color: #c8e6c9;"">GRADE</th>
                <th rowspan=""2"" style=""" + cellStyle + @"background-color: #c8e6c9;"">GPA</th>";

            // Add PC header conditionally
            if (!IS_Hide_Class_Position)
            {
                html += @"<th rowspan=""2"" style=""" + cellStyle + @"background-color: #e8f4fd; font-weight: bold;"">PC</th>";
            }

            // Add PS header conditionally based on HasSections AND IS_Hide_Sec_Position
            if (HasSections && !IS_Hide_Sec_Position)
            {
                html += @"<th rowspan=""2"" style=""" + cellStyle + @"background-color: #e8f4fd; font-weight: bold;"">PS</th>";
            }

            // HMC header - always show
            html += @"<th rowspan=""2"" style=""" + cellStyle + @"background-color: #e8f4fd; font-weight: bold;"">HMC</th>";

            // Add HMS header conditionally based on HasSections AND IS_Hide_Sec_Position
            if (HasSections && !IS_Hide_Sec_Position)
            {
                html += @"<th rowspan=""2"" style=""" + cellStyle + @"background-color: #e8f4fd; font-weight: bold;"">HMS</th>";
            }

            html += @"</tr>";

            // Second header row - FM, PM, OM for each sub-exam
            html += @"<tr style=""background-color: #c8e6c9;"">";
            for (int i = 0; i < subExamCount; i++)
            {
                html += $@"<th style=""{cellStyle}background-color: #c8e6c9;"">FM</th>
                           <th style=""{cellStyle}background-color: #c8e6c9;"">PM</th>
                           <th style=""{cellStyle}background-color: #c8e6c9;"">OM</th>";
            }
            html += @"</tr>";

            // Build rows
            foreach (DataRow srow in subjects.Rows)
            {
                string subjectName = GetSafeColumnValue(srow, "SubjectName");
                decimal obtainedMarkDecimal = GetSafeDecimalValue(srow, "ObtainedMark_ofSubject");
                decimal fullMarkDecimal = GetSafeDecimalValue(srow, "TotalMark_ofSubject");
                string subjectGrades = GetSafeColumnValue(srow, "SubjectGrades");
                decimal subjectPoint = GetSafeDecimalValue(srow, "SubjectPoint");
                int subjectID = 0;
                int.TryParse(GetSafeColumnValue(srow, "SubjectID"), out subjectID);

                // Check if this subject has sub-exam data
                bool hasSubExamData = CheckSubjectHasSubExamData(studentResultID, subjectID, examID);

                // FIXED: Format marks with decimal if needed
                bool isAbsent = (obtainedMarkDecimal == 0 && string.IsNullOrWhiteSpace(subjectGrades));
                
                string displayObtainedMark = isAbsent ? "Abs" : 
                    (obtainedMarkDecimal % 1 != 0 ? obtainedMarkDecimal.ToString("F1") : obtainedMarkDecimal.ToString("F0"));
                
                string displayFullMark = fullMarkDecimal % 1 != 0 ? fullMarkDecimal.ToString("F1") : fullMarkDecimal.ToString("F0");

                // Get position data from database
                var positionData = GetSubjectPositionDataForResult(studentResultID, subjectID);
                string positionClass = positionData.PositionClass;
                string positionSection = positionData.PositionSection;

                // Get highest marks data from DataRow
                decimal hmcValue = GetSafeDecimalValue(srow, "HighestMark_InSubject_Class");
                decimal hmsValue = GetSafeDecimalValue(srow, "HighestMark_InSubject_Subsection");

                html += $@"<tr>
                    <td style=""{cellStyle}text-align: left; padding-left: 8px; min-width: 80px; max-width: 120px;"">{subjectName}</td>
                    ";

                if (hasSubExamData)
                {
                    // Show actual sub-exam marks
                    var subExamMarks = GetSubExamMarksForSubject(studentResultID, subjectID, examID, subExamCount);
                    html += subExamMarks;
                }
                else
                {
                    // Show dashes for all sub-exam columns for subjects without sub-exam data
                    for (int i = 0; i < subExamCount; i++)
                    {
                        html += $@"<td style=""{cellStyle}color: #666;"" class=""no-sub-exam-data"">-</td>
                                   <td style=""{cellStyle}color: #666;"" class=""no-sub-exam-data"">-</td>
                                   <td style=""{cellStyle}color: #666;"" class=""no-sub-exam-data"">-</td>";
                    }
                }

                // FIXED: Use decimal-formatted marks in MARKS column
                html += $@"<td style=""{cellStyle}min-width: 70px;"">{displayObtainedMark}/{displayFullMark}</td>
                    <td style=""{cellStyle}"">{subjectGrades}</td>
                    <td style=""{cellStyle}"">{subjectPoint:F1}</td>";

                // Add PC data conditionally
                if (!IS_Hide_Class_Position)
                {
                    html += $@"<td style=""{cellStyle}background-color: #e8f4fd;"">{positionClass}</td>";
                }

                // Add PS data conditionally
                if (HasSections && !IS_Hide_Sec_Position)
                {
                    html += $@"<td style=""{cellStyle}background-color: #e8f4fd;"">{positionSection}</td>";
                }

                // HMC data - always show
                html += $@"<td style=""{cellStyle}background-color: #e8f4fd;"">{(hmcValue > 0 ? hmcValue.ToString("F0") : "-")}</td>";

                // Add HMS data conditionally
                if (HasSections && !IS_Hide_Sec_Position)
                {
                    html += $@"<td style=""{cellStyle}background-color: #e8f4fd;"">{(hmsValue > 0 ? hmsValue.ToString("F0") : "-")}</td>";
                }

                html += @"</tr>";
            }

            return html;
        }

        // New method to get sub-exam names from database
        private List<string> GetSubExamNames(int examID)
        {
            List<string> subExamNames = new List<string>();
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

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

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subExamNames.Add(reader["SubExamName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sub-exam names: {ex.Message}");
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }

            return subExamNames;
        }

        private string GetSubExamMarksForSubject(string studentResultID, int subjectID, int examID, int expectedSubExamCount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT esn.SubExamName, esn.Sub_ExamSN, eom.FullMark, eom.PassMark, eom.MarksObtained 
                                   FROM Exam_Obtain_Marks eom
                                   INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                                   WHERE eom.StudentResultID = @StudentResultID AND eom.SubjectID = @SubjectID 
                                   AND eom.ExamID = @ExamID AND eom.SchoolID = @SchoolID
                                   ORDER BY esn.Sub_ExamSN";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                        cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                        cmd.Parameters.AddWithValue("@ExamID", examID);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);

                        string cellStyle = "border: 1px solid #000; padding: 3px; text-align: center; font-size: 11px; min-width: 35px; max-width: 45px;";
                        string result = "";
                        int actualSubExamCount = 0;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read() && actualSubExamCount < expectedSubExamCount)
                            {
                                string fm = reader["FullMark"]?.ToString() ?? "-";
                                string pm = reader["PassMark"]?.ToString() ?? "-";
                                string om = reader["MarksObtained"]?.ToString() ?? "";

                                // Format Full Mark to remove decimal if it's a whole number
                                if (decimal.TryParse(fm, out decimal fmValue))
                                {
                                    fm = fmValue % 1 == 0 ? fmValue.ToString("F0") : fmValue.ToString("F1");
                                }

                                // Format Pass Mark to remove decimal if it's a whole number
                                if (decimal.TryParse(pm, out decimal pmValue))
                                {
                                    pm = pmValue % 1 == 0 ? pmValue.ToString("F0") : pmValue.ToString("F1");
                                }

                                // FIXED: Format Obtained Mark - show decimal only when needed (e.g., 79.5 should stay 79.5, not 80)
                                if (string.IsNullOrWhiteSpace(om))
                                {
                                    om = "Abs";
                                }
                                else if (string.Equals(om, "A", StringComparison.OrdinalIgnoreCase) || 
                                         string.Equals(om, "ABS", StringComparison.OrdinalIgnoreCase) || 
                                         string.Equals(om, "ABSENT", StringComparison.OrdinalIgnoreCase))
                                {
                                    om = "Abs";
                                }
                                else if (decimal.TryParse(om, out decimal omValue))
                                {
                                    // CRITICAL FIX: Show decimal if it has fractional part (e.g., 79.5), otherwise show whole number (e.g., 20)
                                    if (omValue % 1 != 0)
                                    {
                                        // Has decimal part - show one decimal place
                                        om = omValue.ToString("F1");
                                    }
                                    else
                                    {
                                        // Whole number - show without decimal
                                        om = omValue.ToString("F0");
                                    }
                                }
                                // else: keep as is (might be already formatted)

                                result += $@"<td style=""{cellStyle}"">{fm}</td>
                                           <td style=""{cellStyle}"">{pm}</td>
                                           <td style=""{cellStyle}"">{om}</td>";
                                actualSubExamCount++;
                            }
                        }

                        // Fill missing sub-exam slots with dashes
                        while (actualSubExamCount < expectedSubExamCount)
                        {
                            result += $@"<td style=""{cellStyle}color: #666;"" class=""no-sub-exam-data"">-</td>
                                       <td style=""{cellStyle}color: #666;"" class=""no-sub-exam-data"">-</td>
                                       <td style=""{cellStyle}color: #666;"" class=""no-sub-exam-data"">-</td>";
                            actualSubExamCount++;
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSubExamMarksForSubject error: {ex.Message}");
                string cellStyle = "border: 1px solid #000; padding: 3px; text-align: center; font-size: 11px; min-width: 35px; max-width: 45px;";
                string result = "";
                for (int i = 0; i < expectedSubExamCount; i++)
                {
                    result += $@"<td style=""{cellStyle}color: #666;"">-</td>
                               <td style=""{cellStyle}color: #666;"">-</td>
                               <td style=""{cellStyle}color: #666;"">-</td>";
                }
                return result;
            }
        }
    }

    public class AttendanceData
    {
        public string WorkingDays { get; set; }
        public string PresentDays { get; set; }
        public string AbsentDays { get; set; }
        public string LeaveDays { get; set; }
        public string LateAbsDays { get; set; }
        public string LateDays { get; set; }
    }

    // Helper class for position data
    public class SubjectPositionData
    {
        public string PositionClass { get; set; }
        public string PositionSection { get; set; }
    }
}

// Add missing helper methods as a partial class
namespace EDUCATION.COM.Exam.Result
{
    public partial class Result_Card_English
    {
        // Helper method to safely register startup script
        private void SafeRegisterStartupScript(string key, string script)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(script))
                    return;

                string safeScript = "try { " + script + " } catch (e) { console.error('JavaScript error in " + key + ":', e); }";
                Page.ClientScript.RegisterStartupScript(typeof(Page), key, safeScript, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering JavaScript for key '{key}': {ex.Message}");
            }
        }

        // Helper method to safely escape strings for JavaScript output
        private string EscapeForJavaScript(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("\\", "\\\\")  // Escape backslashes first
                .Replace("'", "\\'")    // Escape single quotes
                .Replace("\"", "\\\"")  // Escape double quotes
                .Replace("\n", "\\n")   // Escape newlines
                .Replace("\r", "\\r")   // Escape carriage returns
                .Replace("\t", "\\t")   // Escape tabs
                .Replace("\b", "\\b")   // Escape backspace
                .Replace("\f", "\\f")   // Escape form feed
                .Replace("\v", "\\v")   // Escape vertical tab
                .Replace("\0", "\\0");  // Escape null character
        }

        // Helper method to parse Student IDs from comma-separated input
        private List<string> ParseStudentIDs(string input)
        {
            var studentIDs = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return studentIDs;

            // Split by comma and parse each ID
            string[] idStrings = input.Split(new char[] { ',', '۔' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string idString in idStrings)
            {
                string cleanId = idString.Trim();

                // Convert Bengali numbers to English if needed
                cleanId = ConvertBengaliToEnglish(cleanId);

                // Check if it's a valid ID (can be numeric or alphanumeric)
                if (!string.IsNullOrEmpty(cleanId) && cleanId.Length > 0)
                {
                    // Add quotes around the ID for SQL IN clause
                    string quotedId = $"'{cleanId}'";
                    if (!studentIDs.Contains(quotedId))
                    {
                        studentIDs.Add(quotedId);
                    }
                }
            }

            return studentIDs;
        }

        // Helper method to convert Bengali numbers to English
        private string ConvertBengaliToEnglish(string bengaliText)
        {
            if (string.IsNullOrEmpty(bengaliText))
                return bengaliText;

            var bengaliToEnglish = new Dictionary<char, char>
            {
                {'০', '0'}, {'১', '1'}, {'২', '2'}, {'৩', '3'}, {'৪', '4'},
                {'৫', '5'}, {'৬', '6'}, {'৭', '7'}, {'৮', '8'}, {'৯', '9'}
            };

            var result = new StringBuilder();
            foreach (char c in bengaliText)
            {
                if (bengaliToEnglish.ContainsKey(c))
                {
                    result.Append(bengaliToEnglish[c]);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        // Helper method to convert number to ordinal (1st, 2nd, 3rd, etc.)
        private string ToOrdinal(int number)
        {
            if (number <= 0) return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            switch (number % 10)
            {
                case 1:
                    return number + "st";
                case 2:
                    return number + "nd";
                case 3:
                    return number + "rd";
                default:
                    return number + "th";
            }
        }

        // Helper method to check if subject has sub-exam data
        private bool CheckSubjectHasSubExamData(string studentResultID, int subjectID, int examID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT COUNT(*) FROM Exam_Obtain_Marks 
                                   WHERE StudentResultID = @StudentResultID 
                                   AND SubjectID = @SubjectID 
                                   AND ExamID = @ExamID 
                                   AND SchoolID = @SchoolID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                        cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                        cmd.Parameters.AddWithValue("@ExamID", examID);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);

                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // Helper method to get subject position data
        private SubjectPositionData GetSubjectPositionDataForResult(string studentResultID, int subjectID)
        {
            SubjectPositionData positionData = new SubjectPositionData
            {
                PositionClass = "-",
                PositionSection = "-"
            };

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT Position_InSubject_Class, Position_InSubject_Subsection 
                                   FROM Exam_Result_of_Subject 
                                   WHERE StudentResultID = @StudentResultID 
                                   AND SubjectID = @SubjectID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                        cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int posClass = 0;
                                int posSection = 0;

                                if (reader["Position_InSubject_Class"] != DBNull.Value)
                                {
                                    int.TryParse(reader["Position_InSubject_Class"].ToString(), out posClass);
                                }

                                if (reader["Position_InSubject_Subsection"] != DBNull.Value)
                                {
                                    int.TryParse(reader["Position_InSubject_Subsection"].ToString(), out posSection);
                                }

                                positionData.PositionClass = posClass > 0 ? ToOrdinal(posClass) : "-";
                                positionData.PositionSection = posSection > 0 ? ToOrdinal(posSection) : "-";
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return default values on error
            }

            return positionData;
        }

        // Helper method to get attendance data
        private AttendanceData GetAttendanceData(string studentResultID, int examID)
        {
            AttendanceData data = new AttendanceData
            {
                WorkingDays = "",
                PresentDays = "",
                AbsentDays = "",
                LeaveDays = "",
                LateAbsDays = "",
                LateDays = ""
            };

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT WorkingDays, PresentDays, AbsentDays, LeaveDays, LateAbsDays, LateDays 
                                   FROM Exam_Result_of_Student 
                                   WHERE StudentResultID = @StudentResultID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                data.WorkingDays = reader["WorkingDays"]?.ToString() ?? "";
                                data.PresentDays = reader["PresentDays"]?.ToString() ?? "";
                                data.AbsentDays = reader["AbsentDays"]?.ToString() ?? "";
                                data.LeaveDays = reader["LeaveDays"]?.ToString() ?? "";
                                data.LateAbsDays = reader["LateAbsDays"]?.ToString() ?? "";
                                data.LateDays = reader["LateDays"]?.ToString() ?? "";
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return default values on error
            }

            return data;
        }

        // Helper method to load students by IDs
        private DataTable LoadStudentsByIDs(List<string> studentIDs)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    string idList = string.Join(",", studentIDs);

                    string query = $@"
                        SELECT DISTINCT
                            ers.StudentResultID,
                            ers.ObtainedMark_ofStudent as TotalExamObtainedMark_ofStudent,
                            ers.Student_Grade,
                            ers.Student_Point,
                            ers.Average,
                            ers.ObtainedPercentage_ofStudent,
                            ers.TotalMark_ofStudent,
                            ers.Position_InExam_Class,
                            ers.Position_InExam_Subsection,
                            ers.PassStatus_Student as PassStatus_ofStudent,
                            st.StudentsName,
                            st.ID,
                            ISNULL(st.StudentImageID, 0) as StudentImageID,
                            sc.RollNo,
                            cc.Class as ClassName,
                            ISNULL(cs.Section, '') as SectionName,
                            ISNULL(csh.Shift, '') as ShiftName,
                            ISNULL(csg.SubjectGroup, '') as GroupName,
                            en.ExamName,
                            ers.SchoolID,
                            si.SchoolName,
                            si.Address,
                            si.Phone,
                            CASE WHEN ISNUMERIC(sc.RollNo) = 1 
                                THEN CAST(sc.RollNo AS INT) 
                                ELSE 999999 END as RollNoSortNumber
                        FROM Exam_Result_of_Student ers
                        INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                        INNER JOIN Student st ON sc.StudentID = st.StudentID
                        INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
                        INNER JOIN Exam_Name en ON ers.ExamID = en.ExamID
                        INNER JOIN SchoolInfo si ON ers.SchoolID = si.SchoolID
                        LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
                        LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
                        LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
                        WHERE ers.SchoolID = @SchoolID
                        AND ers.EducationYearID = @EducationYearID
                        AND ers.ExamID = @ExamID
                        AND sc.ClassID = @ClassID
                        AND st.ID IN ({idList})
                        ORDER BY RollNoSortNumber, sc.RollNo";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandTimeout = 60;
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                        cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                        cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStudentsByIDs error: {ex.Message}");
            }

            return dt;
        }

        // Helper method to load students by class
        private DataTable LoadStudentsByClass()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();

                    StringBuilder whereClause = new StringBuilder();
                    whereClause.Append("ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID AND ers.ExamID = @ExamID");

                    if (ClassDropDownList.SelectedValue != "0")
                        whereClause.Append(" AND sc.ClassID = @ClassID");

                    if (GroupDropDownList.Items.Count > 1 && GroupDropDownList.SelectedValue != "%")
                        whereClause.Append(" AND sc.SubjectGroupID = @GroupID");

                    if (SectionDropDownList.Items.Count > 1 && SectionDropDownList.SelectedValue != "%")
                        whereClause.Append(" AND sc.SectionID = @SectionID");

                    if (ShiftDropDownList.Items.Count > 1 && ShiftDropDownList.SelectedValue != "%")
                        whereClause.Append(" AND sc.ShiftID = @ShiftID");

                    string query = $@"
                        SELECT DISTINCT
                            ers.StudentResultID,
                            ers.ObtainedMark_ofStudent as TotalExamObtainedMark_ofStudent,
                            ers.Student_Grade,
                            ers.Student_Point,
                            ers.Average,
                            ers.ObtainedPercentage_ofStudent,
                            ers.TotalMark_ofStudent,
                            ers.Position_InExam_Class,
                            ers.Position_InExam_Subsection,
                            ers.PassStatus_Student as PassStatus_ofStudent,
                            st.StudentsName,
                            st.ID,
                            ISNULL(st.StudentImageID, 0) as StudentImageID,
                            sc.RollNo,
                            cc.Class as ClassName,
                            ISNULL(cs.Section, '') as SectionName,
                            ISNULL(csh.Shift, '') as ShiftName,
                            ISNULL(csg.SubjectGroup, '') as GroupName,
                            en.ExamName,
                            ers.SchoolID,
                            si.SchoolName,
                            si.Address,
                            si.Phone,
                            CASE WHEN ISNUMERIC(sc.RollNo) = 1 
                                THEN CAST(sc.RollNo AS INT) 
                                ELSE 999999 END as RollNoSortNumber
                        FROM Exam_Result_of_Student ers
                        INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                        INNER JOIN Student st ON sc.StudentID = st.StudentID
                        INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
                        INNER JOIN Exam_Name en ON ers.ExamID = en.ExamID
                        INNER JOIN SchoolInfo si ON ers.SchoolID = si.SchoolID
                        LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
                        LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
                        LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
                        WHERE {whereClause}
                        ORDER BY RollNoSortNumber, sc.RollNo";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandTimeout = 60;
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                        cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                        cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);

                        if (ClassDropDownList.SelectedValue != "0")
                        {
                            cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        }

                        if (GroupDropDownList.Items.Count > 1 && GroupDropDownList.SelectedValue != "%")
                        {
                            cmd.Parameters.AddWithValue("@GroupID", GroupDropDownList.SelectedValue);
                        }

                        if (SectionDropDownList.Items.Count > 1 && SectionDropDownList.SelectedValue != "%")
                        {
                            cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                        }

                        if (ShiftDropDownList.Items.Count > 1 && ShiftDropDownList.SelectedValue != "%")
                        {
                            cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStudentsByClass error: {ex.Message}\nStack: {ex.StackTrace}");
            }

            return dt;
        }
    }
}