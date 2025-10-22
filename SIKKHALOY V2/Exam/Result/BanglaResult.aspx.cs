﻿using System;
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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Bangla_Result_DirectPrint : System.Web.UI.Page
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

                // Hide print button when class changes using safe JavaScript
                SafeRegisterStartupScript("hidePrintOnClassChange", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';");

                // Reset page title when class changes using safe JavaScript
                SafeRegisterStartupScript("resetTitleClassChange", "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';");
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Use safe JavaScript registration for class selection errors
                string safeErrorMessage = EscapeForJavaScript(ex.Message);
                SafeRegisterStartupScript("error", $"console.error('Class selection error: {safeErrorMessage}');");
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
                        // Use safe JavaScript registration
                        string escapedStudentIDText = EscapeForJavaScript(studentIDText);

                        SafeRegisterStartupScript("debug1",
                            $"console.log('Loading results for Student IDs: {escapedStudentIDText}, Exam ID: {ExamDropDownList.SelectedValue}, Class ID: {ClassDropDownList.SelectedValue}');");

                        LoadResultsData();
                    }
                    else
                    {
                        SafeRegisterStartupScript("alert", "alert('Student ID অনুসন্ধানের জন্য, দয়া করে ক্লাস এবং পরীক্ষা উভয়ই নির্বাচন করুন');");
                    }
                }
                else
                {
                    // For normal search, Class and Exam are required
                    if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0")
                    {
                        SafeRegisterStartupScript("debug1",
                            $"console.log('Loading results for Exam ID: {ExamDropDownList.SelectedValue}, Class ID: {ClassDropDownList.SelectedValue}');");

                        LoadResultsData();
                    }
                    else
                    {
                        SafeRegisterStartupScript("alert", "alert('দয়া করে ক্লাস এবং পরীক্ষা উভয়ই নির্বাচন করুন');");
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Use safe JavaScript registration for errors
                string safeErrorMessage = EscapeForJavaScript(ex.Message);
                SafeRegisterStartupScript("error", $"console.error('LoadResults Error: {safeErrorMessage}');");
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
                        SafeRegisterStartupScript("invalidID", "alert('দয়া করে বৈধ Student ID প্রদান করুন');");
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
                        SafeRegisterStartupScript("showPrintButton", "document.getElementById('PrintButton').style.display = 'inline-block';");

                        // Update page title with dynamic student count - use safe JavaScript
                        int studentCount = dt.Rows.Count;
                        string searchMethod = isSearchingByID ? "আইডি সার্চ" : "সাধারণ সার্চ";
                        string dynamicTitle = EscapeForJavaScript($"বাংলা রেজাল্ট কার্ড - মোট শিক্ষার্থী ( {studentCount} ) - {searchMethod}");

                        // Update page title using JavaScript
                        SafeRegisterStartupScript("updateTitle", $"document.getElementById('pageTitle').innerHTML = '{dynamicTitle}';");
                    }
                    else
                    {
                        AllResultsData = null;
                        TotalRecords = 0;
                        CurrentPageIndex = 0;
                        ResultPanel.Visible = false;

                        // Hide print button when no results
                        SafeRegisterStartupScript("hidePrintButton", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';");

                        // Reset page title when no results
                        SafeRegisterStartupScript("resetTitle", "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';");

                        string noResultsMessage = isSearchingByID ?
                            "নির্দিষ্ট Student ID এর জন্য কোন ফলাফল পাওয়া যায়নি" :
                            "নির্বাচিত শর্তের জন্য কোন ফলাফল পাওয়া যায়নি";

                        SafeRegisterStartupScript("nodata", $"alert('{EscapeForJavaScript(noResultsMessage)}');");
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
                SafeRegisterStartupScript("resetTitleError", "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';");

                // Use safe JavaScript registration for SQL errors
                string safeErrorMessage = EscapeForJavaScript(sqlEx.Message);
                SafeRegisterStartupScript("sqlerror", $"console.error('Database Error: {safeErrorMessage}');");
            }
            catch (Exception ex)
            {
                ResultPanel.Visible = false;

                // Reset title on error
                SafeRegisterStartupScript("resetTitleError2", "document.getElementById('pageTitle').innerHTML = 'বাংলা রেজাল্ট কার্ড';");

                // Use safe JavaScript registration for general errors
                string safeErrorMessage = EscapeForJavaScript(ex.Message);
                SafeRegisterStartupScript("dberror", $"console.error('Error: {safeErrorMessage}');");
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

                // Hide print button when no results using safe JavaScript
                SafeRegisterStartupScript("hidePrintButton", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';");
                return;
            }

            int totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
            int currentPage = CurrentPageIndex + 1;
            int startRecord = (CurrentPageIndex * PageSize) + 1;
            int endRecord = Math.Min(startRecord + PageSize - 1, TotalRecords);

            // Update info labels
            PaginationInfoLabel.Text = $"লোড হয়েছে {startRecord} থেকে {endRecord} জন। মোট {TotalRecords} জন শিক্ষার্থী থেকে";
            PageInfoLabel.Text = $"Page {currentPage} of {totalPages}";

            // Enable/disable buttons
            FirstPageButton.Enabled = CurrentPageIndex > 0;
            PrevPageButton.Enabled = CurrentPageIndex > 0;
            NextPageButton.Enabled = CurrentPageIndex < (totalPages - 1);
            LastPageButton.Enabled = CurrentPageIndex < (totalPages - 1);

            // Show print button when there are results using safe JavaScript
            SafeRegisterStartupScript("showPrintButton", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'inline-block';");
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
                // Bind grading system data
                Repeater gradingSystemRepeater = (Repeater)e.Item.FindControl("GradingSystemRepeater");
                if (gradingSystemRepeater != null)
                {
                    DataTable gradingData = GetGradingSystemData();
                    gradingSystemRepeater.DataSource = gradingData;
                    gradingSystemRepeater.DataBind();
                }

                // Handle dynamic header display based on School Name Logo
                if (Session["SchoolID"] != null)
                {
                    int schoolId = Convert.ToInt32(Session["SchoolID"]);
                    bool hasSchoolNameLogo = CheckSchoolNameLogoExists(schoolId);

                    System.Diagnostics.Debug.WriteLine($"BanglaResult ItemDataBound - SchoolID: {schoolId}, HasLogo: {hasSchoolNameLogo}");

                    var schoolNameLogoPanel = e.Item.FindControl("SchoolNameLogoHeaderPanel") as Panel;
                    var traditionalHeaderPanel = e.Item.FindControl("TraditionalHeaderPanel") as Panel;

                    if (schoolNameLogoPanel != null && traditionalHeaderPanel != null)
                    {
                        if (hasSchoolNameLogo)
                        {
                            System.Diagnostics.Debug.WriteLine("BanglaResult - Showing School Name Logo, Hiding Traditional Header");

                            // Show school name logo panel - full width center
                            schoolNameLogoPanel.CssClass = "show-panel";
                            schoolNameLogoPanel.Style.Clear();
                            schoolNameLogoPanel.Style.Add("display", "block");
                            schoolNameLogoPanel.Style.Add("visibility", "visible");
                            schoolNameLogoPanel.Style.Add("width", "100%");

                            // Set the school name logo image
                            var schoolNameLogoImage = e.Item.FindControl("SchoolNameLogoImage") as System.Web.UI.HtmlControls.HtmlImage;
                            if (schoolNameLogoImage != null)
                            {
                                schoolNameLogoImage.Src = string.Format("/Handeler/SchoolNameLogo.ashx?SchoolID={0}&t={1}", schoolId, DateTime.Now.Ticks);
                                System.Diagnostics.Debug.WriteLine($"BanglaResult - School Name Logo Image URL: {schoolNameLogoImage.Src}");
                            }

                            // Completely hide traditional header
                            traditionalHeaderPanel.CssClass = "hide-panel";
                            traditionalHeaderPanel.Style.Clear();
                            traditionalHeaderPanel.Style.Add("display", "none");
                            traditionalHeaderPanel.Style.Add("visibility", "hidden");
                            traditionalHeaderPanel.Style.Add("position", "absolute");
                            traditionalHeaderPanel.Style.Add("left", "-9999px");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("BanglaResult - Showing Traditional Header, Hiding School Name Logo");

                            // Completely hide school name logo panel
                            schoolNameLogoPanel.CssClass = "hide-panel";
                            schoolNameLogoPanel.Style.Clear();
                            schoolNameLogoPanel.Style.Add("display", "none");
                            schoolNameLogoPanel.Style.Add("visibility", "hidden");
                            schoolNameLogoPanel.Style.Add("position", "absolute");
                            schoolNameLogoPanel.Style.Add("left", "-9999px");

                            // Show traditional header
                            traditionalHeaderPanel.CssClass = "show-panel";
                            traditionalHeaderPanel.Style.Clear();
                            traditionalHeaderPanel.Style.Add("display", "block");
                            traditionalHeaderPanel.Style.Add("visibility", "visible");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"BanglaResult - Panel controls not found. SchoolNameLogoPanel: {schoolNameLogoPanel != null}, TraditionalHeaderPanel: {traditionalHeaderPanel != null}");
                    }
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

                SafeRegisterStartupScript("gradingTableAdapter", $"console.log('TableAdapter returned {gradingData.Rows.Count} grading rows');");

                // If we got data from TableAdapter, return it
                if (gradingData.Rows.Count > 0)
                {
                    // Log what we found
                    foreach (System.Data.DataRow row in gradingData.Rows)
                    {
                        string grade = EscapeForJavaScript(row["Grades"]?.ToString() ?? "");
                        string comment = EscapeForJavaScript(row["Comments"]?.ToString() ?? "");
                        string marks = EscapeForJavaScript(row["MARKS"]?.ToString() ?? "");
                        SafeRegisterStartupScript($"gradingRow{grade}", $"console.log('TableAdapter Grade: {grade}, Comment: {comment}, Marks: {marks}');");
                    }

                    return gradingData;
                }
                else
                {
                    SafeRegisterStartupScript("noGradingData", "console.log('No grading data from TableAdapter, using default');");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TableAdapter GetGradingSystemData error: {ex.Message}");
                string safeErrorMessage = EscapeForJavaScript(ex.Message);
                SafeRegisterStartupScript("gradingTableAdapterError", $"console.error('TableAdapter error: {safeErrorMessage}');");
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
                default: return gpa >= 4.0m ? "চিত্তাকর্ষক" : "ভালো";
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
                        string safeGrade = EscapeForJavaScript(gradeFromChart);
                        string safeComment = EscapeForJavaScript(commentFromChart);
                        SafeRegisterStartupScript("gradeFromChart", $"console.log('Found comment from TableAdapter: Grade={safeGrade}, Comment={safeComment}');");

                        if (!string.IsNullOrEmpty(commentFromChart))
                        {
                            return commentFromChart;
                        }
                    }
                }

                string safeStudentGrade = EscapeForJavaScript(studentGrade);
                SafeRegisterStartupScript("noGradeFromChart", $"console.log('No comment found in TableAdapter data for grade: {safeStudentGrade}');");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCommentFromGradingChart error: {ex.Message}");
                string safeErrorMessage = EscapeForJavaScript(ex.Message);
                SafeRegisterStartupScript("gradeChartError", $"console.error('GetCommentFromGradingChart error: {safeErrorMessage}');");
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

            return passStatus.ToString() == "Pass" ? "উত্তীর্ণ" : "অনুত্তীর্ণ";
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

        // Method to generate the complete subject marks table with sub-exams
        public string GenerateSubjectMarksTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            try
            {
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                
                // Get subject results
                DataTable subjectResults = GetSubjectResults(studentResultID);
                
                if (subjectResults.Rows.Count == 0)
                {
                    return "<p>No subject results found</p>";
                }
                
                // Check if this exam has sub-exams
                bool hasSubExams = HasSubExams(examID);
                int subExamCount = hasSubExams ? GetSubExamCount(examID) : 0;
                
                // Build the table
                StringBuilder tableHtml = new StringBuilder();
                
                // Get CSS class based on subject count
                string tableCssClass = GetTableCssClass(subjectResults.Rows.Count);
                
                tableHtml.Append($"<table class=\"marks-table {tableCssClass}\">");
                
                // Table header
                tableHtml.Append("<thead><tr>");
                tableHtml.Append("<th>বিষয়</th>");
                
                if (hasSubExams && subExamCount > 0)
                {
                    // Add sub-exam headers
                    tableHtml.Append(GetSubExamHeaderNames(studentResultID));
                }
                
                tableHtml.Append("<th>প্রাপ্ত নম্বর</th>");
                tableHtml.Append("<th>পূর্ণ মান</th>");
                tableHtml.Append("<th>গ্রেড</th>");
                tableHtml.Append("<th>পয়েন্ট</th>");
                tableHtml.Append("</tr></thead>");
                
                // Table body
                tableHtml.Append("<tbody>");
                
                foreach (DataRow row in subjectResults.Rows)
                {
                    string subjectName = GetSafeColumnValue(row, "SubjectName");
                    int subjectID = Convert.ToInt32(row["SubjectID"]);
                    string obtainedMark = GetSafeColumnValue(row, "ObtainedMark_ofSubject");
                    string totalMark = GetSafeColumnValue(row, "TotalMark_ofSubject");
                    string subjectGrade = GetSafeColumnValue(row, "SubjectGrades");
                    decimal subjectPoint = GetSafeDecimalValue(row, "SubjectPoint");
                    string passStatus = GetSafeColumnValue(row, "PassStatus_Subject");
                    
                    // Check if student is absent (A or 0)
                    bool isAbsent = (obtainedMark == "A" || obtainedMark == "0");
                    string displayObtainedMark = isAbsent ? "অনুপস্থিত" : obtainedMark;
                    
                    // Determine row class based on pass status
                    string rowClass = (passStatus == "Fail") ? "failed-subject" : "";
                    
                    tableHtml.Append($"<tr class=\"{rowClass}\">");
                    tableHtml.Append($"<td>{subjectName}</td>");
                    
                    if (hasSubExams && subExamCount > 0)
                    {
                        // Get sub-exam marks for this subject
                        var subExamResult = GetSubExamMarksForDisplay(studentResultID, subjectID, obtainedMark);
                        tableHtml.Append(subExamResult.SubExamMarksCells);
                        tableHtml.Append(subExamResult.TotalMarks);
                    }
                    else
                    {
                        // No sub-exams, just show obtained mark
                        tableHtml.Append($"<td>{displayObtainedMark}</td>");
                    }
                    
                    tableHtml.Append($"<td>{totalMark}</td>");
                    tableHtml.Append($"<td>{subjectGrade}</td>");
                    tableHtml.Append($"<td>{subjectPoint:F1}</td>");
                    tableHtml.Append("</tr>");
                }
                
                tableHtml.Append("</tbody>");
                tableHtml.Append("</table>");
                
                // Result status is now shown in summary table, not here
                
                return tableHtml.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateSubjectMarksTable: {ex.Message}");
                return "<p>Error generating subject marks table</p>";
            }
        }

        // Method to check if School Name Logo exists for dynamic header display
        private bool CheckSchoolNameLogoExists(int schoolId)
        {
            SqlConnection con = null;
            try
            {
                var constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                con = new SqlConnection(constr);
                con.Open();

                // First check if column exists
                using (var checkCmd = new SqlCommand(
                    @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo]') AND name = 'SchoolNameLogo')
                      SELECT 1 ELSE SELECT 0", con))
                {
                    int columnExists = (int)checkCmd.ExecuteScalar();

                    System.Diagnostics.Debug.WriteLine($"BanglaResult - SchoolNameLogo column exists: {columnExists == 1}");

                    if (columnExists == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("BanglaResult - Column does not exist, returning false");
                        return false;
                    }
                }

                // If column exists, check if logo exists for this school
                using (var cmd = new SqlCommand("SELECT SchoolNameLogo FROM SchoolInfo WHERE SchoolID = @SchoolID", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                    var result = cmd.ExecuteScalar();

                    System.Diagnostics.Debug.WriteLine($"BanglaResult - Query result: {(result == null ? "NULL" : result == DBNull.Value ? "DBNull" : "Has Data")}");

                    // Check if logo exists and is not null/empty
                    if (result != null && result != DBNull.Value)
                    {
                        byte[] logoData = result as byte[];
                        var hasData = logoData != null && logoData.Length > 0;

                        System.Diagnostics.Debug.WriteLine($"BanglaResult - Logo data length: {(logoData != null ? logoData.Length : 0)} bytes");
                        System.Diagnostics.Debug.WriteLine($"BanglaResult - Returning: {hasData}");

                        return hasData;
                    }

                    System.Diagnostics.Debug.WriteLine("BanglaResult - No logo data found, returning false");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log the error if needed
                System.Diagnostics.Debug.WriteLine("BanglaResult - Error checking school name logo: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("BanglaResult - Stack trace: " + ex.StackTrace);
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

        // Enhanced method to safely register JavaScript with proper error handling
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

        // Helper method to parse Student IDs from comma-separated input
        private List<string> ParseStudentIDs(string input)
        {
            var studentIDs = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return studentIDs;

            // Split by comma and parse each ID
            string[] idStrings = input.Split(new char[] { ',', '।' }, StringSplitOptions.RemoveEmptyEntries);

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

        // Method to get sub-exam marks formatted for display in separate cells
        private (string SubExamMarksCells, string TotalMarks) GetSubExamMarksForDisplay(string studentResultID, int subjectID, string originalObtainedMark)
        {
            try
            {
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                DataTable subExamMarks = GetSubExamMarks(studentResultID, subjectID, examID);

                SqlConnection con2 = null;
                try
                {
                    con2 = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                    con2.Open();

                    string headerQuery = @"
                        SELECT DISTINCT esn.SubExamName, esn.Sub_ExamSN, 
                               eom.FullMark as FM, eom.PassMark as PM
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

                        string studentMarksQuery = @"
                            SELECT DISTINCT esn.SubExamName, esn.Sub_ExamSN,
                                   ISNULL(CAST(eom.MarksObtained AS varchar(10)), '-') as MarksObtained,
                                   eom.FullMark as FM, eom.PassMark as PM
                            FROM Exam_SubExam_Name esn
                            INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                            WHERE eom.SchoolID = @SchoolID
                            AND eom.EducationYearID = @EducationYearID
                            AND eom.StudentResultID = @StudentResultID
                            AND eom.SubjectID = @SubjectID
                            ORDER BY esn.Sub_ExamSN";

                        using (SqlCommand studentCmd = new SqlCommand(studentMarksQuery, con2))
                        {
                            studentCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                            studentCmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                            studentCmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                            studentCmd.Parameters.AddWithValue("@SubjectID", subjectID);

                            DataTable studentMarks = new DataTable();
                            using (SqlDataAdapter adapter = new SqlDataAdapter(studentCmd))
                            {
                                adapter.Fill(studentMarks);
                            }

                            Dictionary<string, (string marks, decimal pm, decimal fm)> marksDict = new Dictionary<string, (string, decimal, decimal)>();
                            foreach (DataRow markRow in studentMarks.Rows)
                            {
                                string subExamName = markRow["SubExamName"].ToString();
                                string markValue = markRow["MarksObtained"]?.ToString() ?? "-";
                                decimal pm = markRow["PM"] != DBNull.Value ? Convert.ToDecimal(markRow["PM"]) : 0;
                                decimal fm = markRow["FM"] != DBNull.Value ? Convert.ToDecimal(markRow["FM"]) : 0;
                                marksDict[subExamName] = (markValue, pm, fm);
                            }

                            string cellsHtml = "";
                            decimal totalMarks = 0;
                            bool hasValidMarks = false;
                            bool hasAbsentMarks = false;

                            foreach (DataRow headerRow in subExamHeaders.Rows)
                            {
                                string subExamName = headerRow["SubExamName"].ToString();
                                string markValue = "-";
                                decimal passMark = 0;

                                if (marksDict.ContainsKey(subExamName))
                                {
                                    markValue = marksDict[subExamName].marks;
                                    passMark = marksDict[subExamName].pm;
                                }

                                bool isAbsent = false;
                                if (markValue == "A")
                                {
                                    markValue = "অনুপস্থিত";
                                    hasAbsentMarks = true;
                                    isAbsent = true;
                                }
                                else if (markValue == "0")
                                {
                                    markValue = "অনুপাতিত";
                                    hasAbsentMarks = true;
                                    isAbsent = true;
                                }

                                string cellClass = "";
                                if (!isAbsent && markValue != "-")
                                {
                                    decimal obtainedMark = 0;
                                    if (decimal.TryParse(markValue, out obtainedMark))
                                    {
                                        if (obtainedMark < passMark && passMark > 0)
                                        {
                                            cellClass = " class=\"failed-mark-bg\"";
                                        }
                                        totalMarks += obtainedMark;
                                        hasValidMarks = true;
                                    }
                                }

                                cellsHtml += $"<td{cellClass}>{markValue}</td>";
                            }

                            string totalCell;
                            if (hasAbsentMarks)
                            {
                                totalCell = $"<td class=\"total-marks-cell\">-</td>";
                            }
                            else if (hasValidMarks)
                            {
                                decimal overallPassMark = 0;
                                SqlConnection con3 = null;
                                try
                                {
                                    con3 = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                                    con3.Open();

                                    string pmQuery = @"SELECT TOP 1 TotalMark_ofSubject FROM Exam_Result_of_Subject 
                                                       WHERE StudentResultID = @StudentResultID AND SubjectID = @SubjectID";
                                    using (SqlCommand pmCmd = new SqlCommand(pmQuery, con3))
                                    {
                                        pmCmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                                        pmCmd.Parameters.AddWithValue("@SubjectID", subjectID);
                                        var result = pmCmd.ExecuteScalar();
                                        if (result != null && result != DBNull.Value)
                                        {
                                            decimal totalFullMark = Convert.ToDecimal(result);
                                            overallPassMark = totalFullMark * 0.33m;
                                        }
                                    }
                                }
                                finally
                                {
                                    if (con3 != null && con3.State == ConnectionState.Open)
                                    {
                                        con3.Close();
                                        con3.Dispose();
                                    }
                                }

                                string totalCellClass = "";
                                if (totalMarks < overallPassMark && overallPassMark > 0)
                                {
                                    totalCellClass = " class=\"failed-mark-bg\"";
                                }

                                totalCell = $"<td{totalCellClass} class=\"total-marks-cell\">{totalMarks}</td>";
                            }
                            else
                            {
                                string displayMark = (originalObtainedMark == "A" || originalObtainedMark == "0") ? "-" : originalObtainedMark;
                                totalCell = $"<td class=\"total-marks-cell\">{displayMark}</td>";
                            }

                            return (cellsHtml, totalCell);
                        }
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
                string displayMark = (originalObtainedMark == "A" || originalObtainedMark == "0") ? "-" : originalObtainedMark;
                string errorTotalCell = $"<td class=\"total-marks-cell\">{displayMark}</td>";
                return ("", errorTotalCell);
            }
        }

        // Helper methods for class/section/group
        protected bool IsSectionSelected()
        {
            string sectionValue = SectionDropDownList.SelectedValue;
            return !string.IsNullOrEmpty(sectionValue) &&
                   sectionValue != "%" &&
                   sectionValue.Trim() != "" &&
                   SectionDropDownList.Visible;
        }

        protected string GetSectionColumnHeader()
        {
            return IsSectionSelected() ? "<td>শাখা মেধা</td>" : "";
        }

        protected string GetSectionColumnData(object dataItem)
        {
            if (!IsSectionSelected()) return "";

            DataRowView row = (DataRowView)dataItem;
            var positionValue = row["Position_InExam_Subsection"];

            if (positionValue == DBNull.Value || positionValue == null)
            {
                return "<td>N/A</td>";
            }

            return "<td>" + positionValue.ToString() + "</td>";
        }

        protected bool HasGroupsForClass()
        {
            return GroupDropDownList.Visible;
        }

        protected bool HasSectionsForClass()
        {
            return SectionDropDownList.Visible;
        }

        protected string GetDynamicInfoRow(object dataItem)
        {
            DataRowView row = (DataRowView)dataItem;

            string className = row["ClassName"]?.ToString() ?? "";
            string groupName = row["GroupName"]?.ToString() ?? "";
            string sectionName = row["SectionName"]?.ToString() ?? "";

            bool hasGroups = HasGroupsForClass();
            bool hasSections = HasSectionsForClass();

            if (hasGroups)
            {
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
                return @"
                    <tr>
                        <td>ক্লাস:</td>
                        <td>" + className + @"</td>
                        <td colspan=""4""></td>
                    </tr>";
            }
        }

        // Method to get result status column data for summary table
        protected string GetResultStatusColumn(object dataItem)
        {
            try
            {
                DataRowView row = (DataRowView)dataItem;
                string studentGrade = row["Student_Grade"]?.ToString() ?? "";
                decimal studentPoint = row["Student_Point"] != DBNull.Value ? Convert.ToDecimal(row["Student_Point"]) : 0m;

                string resultStatus = GetResultStatus(studentGrade, studentPoint);
                
                // Add CSS class based on pass/fail
                string cssClass = (studentGrade.ToUpper() == "F") ? "result-fail" : "result-pass";
                
                return $"<td class=\"{cssClass}\">{resultStatus}</td>";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetResultStatusColumn: {ex.Message}");
                return "<td>N/A</td>";
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

        protected void DownloadPDFButton_Click(object sender, EventArgs e)
        {
            try
            {
                SafeRegisterStartupScript("printPage", "window.open(window.location.href + '?print=1', '_blank');");
            }
            catch (Exception ex)
            {
                string safeErrorMessage = EscapeForJavaScript(ex.Message);
                SafeRegisterStartupScript("error", $"alert('Error: {safeErrorMessage}');");
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (Request.QueryString["print"] == "1")
            {
                SafeRegisterStartupScript("autoPrint", @"
                    document.addEventListener('DOMContentLoaded', function() {
                        var buttons = document.querySelectorAll('.btn, .form-control');
                        buttons.forEach(function(btn) { btn.style.display = 'none'; });
                        
                        setTimeout(function() {
                            window.print();
                        }, 1000);
                    });
                ");
            }
        }
    }
}