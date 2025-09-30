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
                    CurrentPageIndex = 0;
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

                // Reset pagination
                CurrentPageIndex = 0;
                AllResultsData = null;
                TotalRecords = 0;

                // Hide print button when class changes
                Page.ClientScript.RegisterStartupScript(typeof(Page), "hidePrintOnClassChange",
                    "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';", true);

                // Reset page title when class changes
                Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitleClassChange",
                    "document.getElementById('pageTitle').innerHTML = 'English Result Card';", true);
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
                // Check if Student ID search is being used
                string studentIDText = StudentIDTextBox.Text.Trim();
                bool isSearchingByID = !string.IsNullOrEmpty(studentIDText);

                if (isSearchingByID)
                {
                    // For Student ID search, only Class and Exam are required
                    if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0")
                    {
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "debug1",
                            $"console.log('Loading results for Student IDs: {studentIDText}, Exam ID: {ExamDropDownList.SelectedValue}, Class ID: {ClassDropDownList.SelectedValue}');", true);

                        LoadResultsData();
                    }
                    else
                    {
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "alert",
                            "alert('For Student ID search, please select both Class and Exam');", true);
                    }
                }
                else
                {
                    // For normal search, Class and Exam are required
                    if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0")
                    {
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "debug1",
                            $"console.log('Loading results for Exam ID: {ExamDropDownList.SelectedValue}, Class ID: {ClassDropDownList.SelectedValue}');", true);

                        LoadResultsData();
                    }
                    else
                    {
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "alert", "alert('Please select both Class and Exam');", true);
                    }
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

                // Check if Student ID search is being used
                string studentIDText = StudentIDTextBox.Text.Trim();
                bool isSearchingByID = !string.IsNullOrEmpty(studentIDText);

                string query;

                if (isSearchingByID)
                {
                    // Parse student IDs from textbox
                    var studentIDs = ParseStudentIDs(studentIDText);
                    if (studentIDs.Count == 0)
                    {
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "invalidID",
                            "alert('Please enter valid Student IDs');", true);
                        return;
                    }

                    // Create IN clause for student IDs - they are already quoted in ParseStudentIDs
                    string idInClause = string.Join(",", studentIDs);

                    // Modified query for Student ID search - using 'ID' field instead of 'StudentID'
                    query = @"
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
                            s.ID,
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
                        AND s.ID IN (" + idInClause + @")
                        AND ers.SchoolID = @SchoolID
                        AND ers.EducationYearID = @EducationYearID
                        ORDER BY s.ID";
                }
                else
                {
                    // Original query for normal search
                    query = @"
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
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 30;
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);

                    // Only add these parameters for normal search (not ID search)
                    if (!isSearchingByID)
                    {
                        cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                        cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                        cmd.Parameters.AddWithValue("@GroupID", GroupDropDownList.SelectedValue);
                    }

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.SelectCommand.CommandTimeout = 30;
                        adapter.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        // Store all data for pagination
                        AllResultsData = dt;
                        TotalRecords = dt.Rows.Count;
                        CurrentPageIndex = 0; // Reset to first page

                        // Load signatures separately
                        LoadSignatureImages();

                        // Bind paginated data
                        BindResultsToRepeater(dt);
                        ResultPanel.Visible = true;

                        // Show simple print button when results are available
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "showPrintButton",
                            "document.getElementById('PrintButton').style.display = 'inline-block';", true);

                        // Update page title with dynamic student count
                        int studentCount = dt.Rows.Count;
                        string searchMethod = isSearchingByID ? "ID Search" : "General Search";
                        string dynamicTitle = $"English Result Card - Total Students ( {studentCount} ) - {searchMethod}";

                        // Update page title using JavaScript
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "updateTitle",
                            $"document.getElementById('pageTitle').innerHTML = '{dynamicTitle}';", true);
                    }
                    else
                    {
                        AllResultsData = null;
                        TotalRecords = 0;
                        CurrentPageIndex = 0;
                        ResultPanel.Visible = false;

                        // Hide print button when no results
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "hidePrintButton",
                            "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';", true);

                        // Reset page title when no results
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitle",
                            "document.getElementById('pageTitle').innerHTML = 'English Result Card';", true);

                        string noResultsMessage = isSearchingByID ?
                            "No results found for the specified Student IDs" :
                            "No results found for the selected criteria";

                        Page.ClientScript.RegisterStartupScript(typeof(Page), "nodata",
                            $"alert('{noResultsMessage}');", true);
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
                    "document.getElementById('pageTitle').innerHTML = 'English Result Card';", true);

                Page.ClientScript.RegisterStartupScript(typeof(Page), "sqlerror",
                    "console.error('Database Error: " + sqlEx.Message.Replace("'", "\\'") + "');", true);
            }
            catch (Exception ex)
            {
                ResultPanel.Visible = false;

                // Reset title on error
                Page.ClientScript.RegisterStartupScript(typeof(Page), "resetTitleError2",
                    "document.getElementById('pageTitle').innerHTML = 'English Result Card';", true);

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

        private void BindResultsToRepeater(DataTable dt)
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

        // Helper class for sub-exam header structure
        public class SubExamHeaderStructure
        {
            public string FirstRowHeader { get; set; } = "";
            public string SecondRowHeader { get; set; } = "";
        }

        // Method to get sub-exam headers with proper structure (like the image)
        private SubExamHeaderStructure GetSubExamHeadersWithStructure(string studentResultID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                int subExamCount = GetSubExamCount(examID);
                
                // Dynamic styling based on sub-exam count
                string fontSize = "11px";
                string cellPadding = "3px";
                
                if (subExamCount >= 4)
                {
                    fontSize = "9px";
                    cellPadding = "2px";
                }
                else if (subExamCount >= 3)
                {
                    fontSize = "10px";
                    cellPadding = "2px";
                }

                string standardCellStyle = $"font-size: {fontSize}; font-family: Arial, sans-serif; border: 1px solid #000; padding: {cellPadding}; text-align: center; white-space: nowrap; min-width: 25px; max-width: 35px; overflow: hidden; text-overflow: ellipsis;";

                // Get sub-exam names that have actual data for this class and exam
                string query = @"
                    SELECT DISTINCT esn.SubExamName, esn.Sub_ExamSN, esn.SubExamID
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

                    var result = new SubExamHeaderStructure();
                    
                    // First row: Sub-exam names with colspan=3 for each (FM, PM, OM)
                    foreach (DataRow row in dt.Rows)
                    {
                        string subExamName = row["SubExamName"].ToString();
                        // Truncate long sub-exam names if needed
                        if (subExamName.Length > 8 && subExamCount >= 3)
                        {
                            subExamName = subExamName.Substring(0, 6) + "..";
                        }
                        result.FirstRowHeader += $@"<th colspan=""3"" style=""{standardCellStyle}; min-width: 75px; max-width: 100px;"" title=""{row["SubExamName"]}"">{subExamName}</th>";
                    }

                    // Second row: FM, PM, OM for each sub-exam
                    foreach (DataRow row in dt.Rows)
                    {
                        result.SecondRowHeader += $@"<th style=""{standardCellStyle}"">FM</th><th style=""{standardCellStyle}"">PM</th><th style=""{standardCellStyle}"">OM</th>";
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamHeadersWithStructure: {ex.Message}");
                return new SubExamHeaderStructure();
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

        // Method to get sub-exam marks with full marks structure (FM, PM, OM for each sub-exam)
        private string GetSubExamMarksWithFullMarks(string studentResultID, int subjectID, string originalObtainedMark)
        {
            try
            {
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                int subExamCount = GetSubExamCount(examID);
                
                // Dynamic styling based on sub-exam count
                string fontSize = "11px";
                string cellPadding = "3px";
                
                if (subExamCount >= 4)
                {
                    fontSize = "9px";
                    cellPadding = "2px";
                }
                else if (subExamCount >= 3)
                {
                    fontSize = "10px";
                    cellPadding = "2px";
                }

                string standardCellStyle = $"font-size: {fontSize}; font-family: Arial, sans-serif; border: 1px solid #000; padding: {cellPadding}; text-align: center; white-space: nowrap; min-width: 25px; max-width: 35px; overflow: hidden; text-overflow: ellipsis;";

                // Get available sub-exam IDs
                List<int> availableSubExamIDs = GetAvailableSubExamIDs(examID);
                
                // Check if this subject has any sub-exam data
                if (SubjectHasAnySubExamData(studentResultID, subjectID, availableSubExamIDs))
                {
                    return GetSubExamMarksForSpecificSubject(studentResultID, subjectID, availableSubExamIDs, standardCellStyle);
                }
                else
                {
                    return GenerateDashCellsForSubExams(subExamCount, standardCellStyle);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamMarksWithFullMarks: {ex.Message}");
                return "";
            }
        }

        private string GenerateDashCellsForSubExams(int subExamCount, string standardCellStyle)
        {
            string dashCells = "";
            
            // Generate dash cells for each sub-exam (3 columns per sub-exam: FM, PM, OM)
            for (int i = 0; i < subExamCount; i++)
            {
                dashCells += $@"<td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td>";
            }
            
            return dashCells;
        }

        // Method to get all available sub-exam IDs for this exam and class
        private List<int> GetAvailableSubExamIDs(int examID)
        {
            SqlConnection con = null;
            List<int> subExamIDs = new List<int>();
            
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT DISTINCT esn.SubExamID
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
                    ORDER BY esn.SubExamID";

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
                            subExamIDs.Add(Convert.ToInt32(reader["SubExamID"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAvailableSubExamIDs: {ex.Message}");
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }

            return subExamIDs;
        }

        // Method to check if a subject has ANY sub-exam data for the available sub-exams
        private bool SubjectHasAnySubExamData(string studentResultID, int subjectID, List<int> availableSubExamIDs)
        {
            if (availableSubExamIDs.Count == 0) return false;

            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string subExamIDsInClause = string.Join(",", availableSubExamIDs);
                string query = $@"
                    SELECT COUNT(*) 
                    FROM Exam_Obtain_Marks eom
                    WHERE eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID
                    AND eom.StudentResultID = @StudentResultID
                    AND eom.SubjectID = @SubjectID
                    AND eom.SubExamID IN ({subExamIDsInClause})";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SubjectHasAnySubExamData: {ex.Message}");
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

        // Method to get sub-exam marks for a specific subject in the correct order
        private string GetSubExamMarksForSpecificSubject(string studentResultID, int subjectID, List<int> availableSubExamIDs, string standardCellStyle)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string cellsHtml = "";

                // Get sub-exam data for each available sub-exam in order
                foreach (int subExamID in availableSubExamIDs)
                {
                    string query = @"
                        SELECT 
                            esn.SubExamName,
                            eom.MarksObtained as ObtainedMarks,
                            ISNULL(eom.FullMark, 0) as FullMark,
                            ISNULL(eom.PassMark, 0) as PassMark,
                            ISNULL(eom.AbsenceStatus, 'Present') as AbsenceStatus
                        FROM Exam_SubExam_Name esn
                        LEFT JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID 
                            AND eom.StudentResultID = @StudentResultID 
                            AND eom.SubjectID = @SubjectID
                            AND eom.SchoolID = @SchoolID
                            AND eom.EducationYearID = @EducationYearID
                        WHERE esn.SubExamID = @SubExamID
                        AND esn.SchoolID = @SchoolID
                        AND esn.EducationYearID = @EducationYearID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                        cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                        cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                        cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                        cmd.Parameters.AddWithValue("@SubExamID", subExamID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fullMark = reader["FullMark"]?.ToString() ?? "0";
                                string passMark = reader["PassMark"]?.ToString() ?? "0";
                                var obtainedMarkValue = reader["ObtainedMarks"];
                                string absenceStatus = reader["AbsenceStatus"]?.ToString() ?? "Present";

                                // Check if this subject actually has data for this sub-exam
                                if (obtainedMarkValue == DBNull.Value || obtainedMarkValue == null)
                                {
                                    // No data for this sub-exam, show dashes
                                    cellsHtml += $@"<td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td>";
                                }
                                else
                                {
                                    string obtainedMark = obtainedMarkValue.ToString();
                                    
                                    // Check if student is absent - only show Abs in OM column
                                    bool isAbsent = (absenceStatus == "Absent" || obtainedMark == "A" || 
                                                    (obtainedMark == "0" && absenceStatus == "Absent"));
                                    
                                    // If fullMark or passMark is 0, show dash for those
                                    if (fullMark == "0") fullMark = "-";
                                    if (passMark == "0") passMark = "-";

                                    // Style for absent OM cell (red background)
                                    string omCellStyle = isAbsent ? 
                                        $"{standardCellStyle}; background-color: #ffcccc; color: #d32f2f; font-weight: bold;" : 
                                        standardCellStyle;

                                    // Show actual marks in FM and PM, but Abs in OM if absent
                                    string displayObtainedMark = isAbsent ? "Abs" : obtainedMark;

                                    // Add FM, PM, OM cells for this sub-exam
                                    cellsHtml += $@"<td style=""{standardCellStyle}"" title=""Full Mark: {fullMark}"">{fullMark}</td><td style=""{standardCellStyle}"" title=""Pass Mark: {passMark}"">{passMark}</td><td style=""{omCellStyle}"" title=""Obtained Mark: {displayObtainedMark}"">{displayObtainedMark}</td>";
                                }
                            }
                            else
                            {
                                // Sub-exam not found, show dashes
                                cellsHtml += $@"<td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td>";
                            }
                        }
                    }
                }

                return cellsHtml;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamMarksForSpecificSubject: {ex.Message}");
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
                string subExamSecondHeader = "";
                int subExamCount = 0;
                
                // Get list of all sub-exam IDs that exist for this exam and class
                List<int> availableSubExamIDs = GetAvailableSubExamIDs(examID);

                // FORCE CONSISTENT font size - NO variation
                string fontSize = "11px";
                string cellPadding = "3px";
                
                // UNIFORM styling for ALL table elements - NO exceptions
                string tableContainerStyle = $"font-size: {fontSize} !important; font-family: Arial, sans-serif !important; border-collapse: collapse; width: 100%; table-layout: auto; overflow-x: auto;";
                string standardCellStyle = $"font-size: {fontSize} !important; font-family: Arial, sans-serif !important; border: 1px solid #000; padding: {cellPadding}; text-align: center; white-space: nowrap; min-width: 30px; max-width: 60px; overflow: hidden; text-overflow: ellipsis;";
                
                if (hasSubExams)
                {
                    subExamCount = GetSubExamCount(examID);
                }

                // Calculate total columns dynamically based on actual structure
                int totalColumns = 1; // SUBJECTS column
                if (hasSubExams && subExamCount > 0)
                {
                    totalColumns += (subExamCount * 3); // Each sub-exam has 3 columns (FM, PM, OM)
                }
                else
                {
                    totalColumns += 3; // FM, PM, OM columns for non-sub-exam tables
                }
                totalColumns += 7; // MARKS, GRADE, GPA, PC, PS, HMC, HMS

                string html = $@"<div style=""overflow-x: auto; width: 100%;""><table class=""marks-table {tableSizeClass} sub-exam-{subExamCount}"" style=""{tableContainerStyle}"" data-total-columns=""{totalColumns}"">";

                if (hasSubExams)
                {
                    // Get actual sub-exam count and header names
                    if (subExamCount > 0)
                    {
                        var subExamHeaders = GetSubExamHeadersWithStructure(studentResultID);
                        subExamHeader = subExamHeaders.FirstRowHeader;
                        subExamSecondHeader = subExamHeaders.SecondRowHeader;
                    }
                }

                // Create header row - Dynamic structure based on actual sub-exam count
                if (hasSubExams && subExamCount > 0 && !string.IsNullOrEmpty(subExamHeader))
                {
                    html += $@"
                        <tr style=""background-color: #ffb3ba;"">
                            <th rowspan=""2"" style=""{standardCellStyle}; text-align: left; min-width: 80px; max-width: 120px; font-weight: bold;"">SUBJECTS</th>
                            {subExamHeader}
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 60px; font-weight: bold;"">MARKS</th>
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 40px; font-weight: bold;"">GRADE</th>
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">GPA</th>
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PC</th>
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PS</th>
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMC</th>
                            <th rowspan=""2"" style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMS</th>
                        </tr>
                        <tr style=""background-color: #ffb3ba;"">
                            {subExamSecondHeader}
                        </tr>";
                }
                else
                {
                    // No sub-exams - Simple table structure with position columns
                    html += $@"
                        <tr style=""background-color: #ffb3ba;"">
                            <th style=""{standardCellStyle}; text-align: left; min-width: 80px; max-width: 120px; font-weight: bold;"">SUBJECTS</th>
                            <th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">FM</th>
                            <th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">PM</th>
                            <th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">OM</th>
                            <th style=""{standardCellStyle}; min-width: 60px; font-weight: bold;"">MARKS</th>
                            <th style=""{standardCellStyle}; min-width: 40px; font-weight: bold;"">GRADE</th>
                            <th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">GPA</th>
                            <th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PC</th>
                            <th style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PS</th>
                            <th style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMC</th>
                            <th style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMS</th>
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

                    // Get subject position data from database
                    var positionData = GetSubjectPositionData(studentResultID, subjectID);

                    if (passStatus == "") passStatus = "Pass";
                    string rowClass = passStatus == "Fail" ? "failed-row" : "";

                    // Check if subject is absent - more refined logic
                    bool isSubjectAbsent = (obtainedMark == "A" || (obtainedMark == "0" && subjectGrades == "F" && subjectPoint == 0.0m));

                    // For marks display in MARKS column
                    string displayMark = isSubjectAbsent ? "Abs" : obtainedMark;
                    string marksDisplay = $"{displayMark}/{fullMark}";

                    // Style for marks display if absent
                    string marksColumnStyle = isSubjectAbsent ? 
                        $"{standardCellStyle}; background-color: #ffcccc; color: #d32f2f; font-weight: bold;" : 
                        standardCellStyle;

                    // FORCE same font size for subject name cell
                    string subjectCellStyle = $"font-size: {fontSize} !important; font-family: Arial, sans-serif !important; border: 1px solid #000; padding: {cellPadding}; text-align: left; padding-left: 4px; min-width: 80px; max-width: 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

                    if (hasSubExams && subExamCount > 0)
                    {
                        string subExamData = "";
                        
                        // Check if this subject has ANY sub-exam data
                        if (SubjectHasAnySubExamData(studentResultID, subjectID, availableSubExamIDs))
                        {
                            // Get actual sub-exam marks for this specific subject
                            subExamData = GetSubExamMarksForSpecificSubject(studentResultID, subjectID, availableSubExamIDs, standardCellStyle);
                        }
                        else
                        {
                            // Fill with dashes for subjects without any sub-exam data
                            subExamData = GenerateDashCellsForSubExams(subExamCount, standardCellStyle);
                        }

                        html += $@"
                            <tr class=""{rowClass}"">
                                <td style=""{subjectCellStyle}"" title=""{subjectName}"">{subjectName}</td>
                                {subExamData}
                                <td style=""{marksColumnStyle}"">{marksDisplay}</td>
                                <td style=""{standardCellStyle}"">{subjectGrades}</td>
                                <td style=""{standardCellStyle}"">{subjectPoint.ToString("F1")}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionClass}"">{positionData.PositionClass}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionSection}"">{positionData.PositionSection}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksClass}"">{positionData.HighestMarksClass}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksSection}"">{positionData.HighestMarksSection}</td>
                            </tr>";
                    }
                    else
                    {
                        // No sub-exams - Simple row structure with FM, PM, OM columns
                        var passMarkData = GetSubjectPassMark(subjectID, examID);
                        
                        // For simple structure, show actual marks in FM, PM but Abs only in OM if absent
                        string omDisplayMark = isSubjectAbsent ? "Abs" : obtainedMark;
                        string omCellStyle = isSubjectAbsent ? 
                            $"{standardCellStyle}; background-color: #ffcccc; color: #d32f2f; font-weight: bold;" : 
                            standardCellStyle;
                        
                        html += $@"
                            <tr class=""{rowClass}"">
                                <td style=""{subjectCellStyle}"" title=""{subjectName}"">{subjectName}</td>
                                <td style=""{standardCellStyle}"">{fullMark}</td>
                                <td style=""{standardCellStyle}"">{passMarkData}</td>
                                <td style=""{omCellStyle}"">{omDisplayMark}</td>
                                <td style=""{marksColumnStyle}"">{marksDisplay}</td>
                                <td style=""{standardCellStyle}"">{subjectGrades}</td>
                                <td style=""{standardCellStyle}"">{subjectPoint.ToString("F1")}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionClass}"">{positionData.PositionClass}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionSection}"">{positionData.PositionSection}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksClass}"">{positionData.HighestMarksClass}</td>
                                <td style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksSection}"">{positionData.HighestMarksSection}</td>
                            </tr>";
                    }
                }

                html += "</table></div>";
                return html.ToString();
            }
            catch (Exception ex)
            {
                return "<p>Error loading subject table: " + ex.Message + "</p>";
            }
        }

        // Helper class for subject position data
        public class SubjectPositionData
        {
            public string PositionClass { get; set; } = "-";
            public string PositionSection { get; set; } = "-";
            public string HighestMarksClass { get; set; } = "-";
            public string HighestMarksSection { get; set; } = "-";
        }

        // Method to get subject position data from database
        private SubjectPositionData GetSubjectPositionData(string studentResultID, int subjectID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                // Query to get subject position data similar to RTable TableAdapter
                string query = @"
                    SELECT 
                        ISNULL(CAST(ers.Position_InSubject_Class AS VARCHAR(10)) + 
                            CASE WHEN ers.Position_InSubject_Class % 100 IN (11, 12, 13) 
                            THEN 'th' WHEN ers.Position_InSubject_Class % 10 = 1 THEN 'st' 
                            WHEN ers.Position_InSubject_Class % 10 = 2 THEN 'nd' 
                            WHEN ers.Position_InSubject_Class % 10 = 3 THEN 'rd' 
                            ELSE 'th' END, '-') AS Position_InSubject_Class,
                        ISNULL(CAST(ers.Position_InSubject_Subsection AS VARCHAR(10)) + 
                            CASE WHEN ers.Position_InSubject_Subsection % 100 IN (11, 12, 13) 
                            THEN 'th' WHEN ers.Position_InSubject_Subsection % 10 = 1 THEN 'st' 
                            WHEN ers.Position_InSubject_Subsection % 10 = 2 THEN 'nd' 
                            WHEN ers.Position_InSubject_Subsection % 10 = 3 THEN 'rd' 
                            ELSE 'th' END, '-') AS Position_InSubject_Subsection,
                        ISNULL(CAST(ers.HighestMark_InSubject_Class AS VARCHAR(10)), '-') AS HighestMark_InSubject_Class,
                        ISNULL(CAST(ers.HighestMark_InSubject_Subsection AS VARCHAR(10)), '-') AS HighestMark_InSubject_Subsection
                    FROM Exam_Result_of_Subject ers
                    WHERE ers.StudentResultID = @StudentResultID 
                    AND ers.SubjectID = @SubjectID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SubjectPositionData
                            {
                                PositionClass = reader["Position_InSubject_Class"]?.ToString() ?? "-",
                                PositionSection = reader["Position_InSubject_Subsection"]?.ToString() ?? "-",
                                HighestMarksClass = reader["HighestMark_InSubject_Class"]?.ToString() ?? "-",
                                HighestMarksSection = reader["HighestMark_InSubject_Subsection"]?.ToString() ?? "-"
                            };
                        }
                    }
                }

                return new SubjectPositionData(); // Return default values if no data found
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubjectPositionData: {ex.Message}");
                return new SubjectPositionData(); // Return default values on error
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

        // Method to get subject pass mark
        private string GetSubjectPassMark(int subjectID, int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT ISNULL(AVG(PassMark), 0) as AveragePassMark
                    FROM Exam_Full_Marks 
                    WHERE SubjectID = @SubjectID 
                    AND ExamID = @ExamID 
                    AND SchoolID = @SchoolID 
                    AND EducationYearID = @EducationYearID
                    AND ClassID = @ClassID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                    var result = cmd.ExecuteScalar();
                    decimal passMark = Convert.ToDecimal(result ?? 0);
                    return passMark.ToString("F1");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubjectPassMark: {ex.Message}");
                return "0";
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

        // Helper method to parse Student IDs from comma-separated input
        private List<string> ParseStudentIDs(string input)
        {
            var studentIDs = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return studentIDs;

            // Split by comma and parse each ID
            string[] idStrings = input.Split(new char[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries);

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
            return IsSectionSelected() ? "<td>Section Position</td>" : "";
        }

        protected string GetSectionColumnData(object dataItem)
        {
            if (!IsSectionSelected()) return "";

            DataRowView row = (DataRowView)dataItem;
            var positionValue = row["Position_InExam_Subsection"];

            // Handle DBNull values
            if (positionValue == DBNull.Value || positionValue == null)
            {
                return "<td>N/A</td>";
            }

            return "<td>" + positionValue.ToString() + "</td>";
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
                        <td>Class:</td>
                        <td>" + className + @"</td>
                        <td>Group:</td>
                        <td>" + groupName + @"</td>
                        <td>Section:</td>
                        <td>" + sectionName + @"</td>
                    </tr>";
            }
            else if (hasSections)
            {
                // Show Class and Section only
                return @"
                    <tr>
                        <td>Class:</td>
                        <td>" + className + @"</td>
                        <td>Section:</td>
                        <td>" + sectionName + @"</td>
                        <td colspan=""2""></td>
                    </tr>";
            }
            else
            {
                // Show Class only
                return @"
                    <tr>
                        <td>Class:</td>
                        <td>" + className + @"</td>
                        <td colspan=""4""></td>
                    </tr>";
            }
        }
    }
}