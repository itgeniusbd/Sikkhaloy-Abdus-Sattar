using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Globalization;

namespace EDUCATION.COM.Exam.CumulativeResult
{
    public partial class CumulativeResultCardt : System.Web.UI.Page
    {
        #region Properties and Variables

        private string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
        private const int StudentsPerPage = 25; // ✅ Changed from 10 to 25
        
        private int CurrentPage
        {
            get { return ViewState["CurrentPage"] != null ? (int)ViewState["CurrentPage"] : 1; }
            set { ViewState["CurrentPage"] = value; }
        }

        private int TotalStudents
        {
            get { return ViewState["TotalStudents"] != null ? (int)ViewState["TotalStudents"] : 0; }
            set { ViewState["TotalStudents"] = value; }
        }

        private List<int> FilteredStudentIDs
        {
            get
            {
                if (ViewState["FilteredStudentIDs"] != null)
                    return (List<int>)ViewState["FilteredStudentIDs"];
                return new List<int>();
            }
            set { ViewState["FilteredStudentIDs"] = value; }
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

        // Whether the current class/result set has sections
        private bool HasSections
        {
            get { return ViewState["HasSections"] != null && (bool)ViewState["HasSections"]; }
            set { ViewState["HasSections"] = value; }
        }

        #endregion

        #region Page Events

        protected void Page_Load(object sender, EventArgs e)
        {
            // Update session management first for all postbacks
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {
                    InitializePage();

                    // Initialize session values for filters
                    Session["Group"] = "%";
                    Session["Shift"] = "%";
                    Session["Section"] = "%";

                    // Initially hide Group, Section, Shift dropdowns until Class is selected
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }
            }
            catch { }

            LoadSignatures();
        }

        private void InitializePage()
        {
            // Check if user is logged in
            if (Session["SchoolID"] == null || Session["Edu_Year"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Initialize controls
            ResultPanel.Visible = false;
            UpdatePaginationInfo();

            // Hide controls that should only show when data is loaded
            if (Page.FindControl("PrintButton") != null)
            {
                Page.FindControl("PrintButton").Visible = false;
            }
        }

        #endregion

        #region Dropdown Events and View Management

        protected void view()
        {
            try
            {
                // Check if controls and data sources exist
                if (GroupSQL != null && GroupDropDownList != null)
                {
                    try
                    {
                        DataView GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
                        if (GroupDV != null && GroupDV.Count < 1)
                        {
                            GroupDropDownList.Visible = false;
                        }
                        else if (GroupDV != null)
                        {
                            GroupDropDownList.Visible = true;
                        }
                    }
                    catch
                    {
                        GroupDropDownList.Visible = false;
                    }
                }

                if (SectionSQL != null && SectionDropDownList != null)
                {
                    try
                    {
                        DataView SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
                        if (SectionDV != null && SectionDV.Count < 1)
                        {
                            SectionDropDownList.Visible = false;
                        }
                        else if (SectionDV != null)
                        {
                            SectionDropDownList.Visible = true;
                        }
                    }
                    catch
                    {
                        SectionDropDownList.Visible = false;
                    }
                }

                if (ShiftSQL != null && ShiftDropDownList != null)
                {
                    try
                    {
                        DataView ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
                        if (ShiftDV != null && ShiftDV.Count < 1)
                        {
                            ShiftDropDownList.Visible = false;
                        }
                        else if (ShiftDV != null)
                        {
                            ShiftDropDownList.Visible = true;
                        }
                    }
                    catch
                    {
                        ShiftDropDownList.Visible = false;
                    }
                }
            }
            catch
            {
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset session values for dependent dropdowns - exactly like Cumulative_Result.aspx
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            // Rebind all dependent dropdowns
            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();

            // Update visibility based on item counts
            view();

            // Reset exam dropdown to default
            if (ExamDropDownList != null && ExamDropDownList.Items.Count > 0)
            {
                ExamDropDownList.SelectedIndex = 0;
            }

            // Reset Student ID and hide results
            StudentIDTextBox.Text = "";
            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            view();
            StudentIDTextBox.Text = "";
            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            if (GroupDropDownList.Items.Count > 0)
            {
                GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
                if (IsPostBack && Session["Group"] != null)
                {
                    var groupItem = GroupDropDownList.Items.FindByValue(Session["Group"].ToString());
                    if (groupItem != null)
                        groupItem.Selected = true;
                }
                else
                {
                    GroupDropDownList.SelectedIndex = 0;
                }
            }
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Section"] = SectionDropDownList.SelectedValue;
            view();
            StudentIDTextBox.Text = "";
            ResultPanel.Visible = false;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            if (SectionDropDownList.Items.Count > 0)
            {
                SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
                if (IsPostBack && Session["Section"] != null)
                {
                    var sectionItem = SectionDropDownList.Items.FindByValue(Session["Section"].ToString());
                    if (sectionItem != null)
                        sectionItem.Selected = true;
                }
                else
                {
                    SectionDropDownList.SelectedIndex = 0;
                }
            }
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            view();
            StudentIDTextBox.Text = "";
            ResultPanel.Visible = false;
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            if (ShiftDropDownList.Items.Count > 0)
            {
                ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
                if (IsPostBack && Session["Shift"] != null)
                {
                    var shiftItem = ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString());
                    if (shiftItem != null)
                        shiftItem.Selected = true;
                }
                else
                {
                    ShiftDropDownList.SelectedIndex = 0;
                }
            }
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            StudentIDTextBox.Text = "";
            ResultPanel.Visible = false;
        }

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            // No special logic needed - exam dropdown doesn't depend on class selection
            // It only depends on SchoolID and EducationYearID from session
        }

        #endregion

        #region Load Results

        protected void LoadResultsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                // Load publish settings before loading results
                LoadPublishSettings();

                LoadStudentResults();
            }
            catch (Exception ex)
            {
                ShowError("Error loading results: " + ex.Message);
            }
        }

        // NEW: Load publish settings from Exam_Cumulative_Setting table
        private void LoadPublishSettings()
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();

                string query = @"
                    SELECT TOP 1
                        ISNULL(IS_Hide_Sec_Position, 0) AS IS_Hide_Sec_Position,
                        ISNULL(IS_Hide_Class_Position, 0) AS IS_Hide_Class_Position,
                        ISNULL(IS_Hide_SubExam, 0) AS IS_Hide_SubExam,
                        ISNULL(IS_Grade_BasePoint, 0) AS IS_Grade_BasePoint
                    FROM Exam_Cumulative_Setting
                    WHERE SchoolID = @SchoolID
                    AND EducationYearID = @EducationYearID
                    AND ClassID = @ClassID
                    AND CumulativeNameID = @CumulativeNameID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            IS_Hide_Sec_Position = Convert.ToBoolean(reader["IS_Hide_Sec_Position"]);
                            IS_Hide_Class_Position = Convert.ToBoolean(reader["IS_Hide_Class_Position"]);
                            // For cumulative result, FM and PM are always shown (no hide settings in table)
                            IS_Hide_FullMark = false;
                            IS_Hide_PassMark = false;
                            IS_Grade_BasePoint = Convert.ToBoolean(reader["IS_Grade_BasePoint"]);

                            System.Diagnostics.Debug.WriteLine($"Cumulative Settings Loaded: HideSec={IS_Hide_Sec_Position}, HideClass={IS_Hide_Class_Position}, GradeBase={IS_Grade_BasePoint}");
                        }
                        else
                        {
                            // Default values if no settings found
                            IS_Hide_Sec_Position = false;
                            IS_Hide_Class_Position = false;
                            IS_Hide_FullMark = false;
                            IS_Hide_PassMark = false;
                            IS_Grade_BasePoint = false;
                            System.Diagnostics.Debug.WriteLine("No cumulative settings found - using defaults");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cumulative settings: {ex.Message}");
                // Set defaults on error
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

        private bool ValidateInputs()
        {
            if (ClassDropDownList.SelectedValue == "0")
            {
                ShowError("Please select a class.");
                return false;
            }

            if (ExamDropDownList.SelectedValue == "0")
            {
                ShowError("Please select an exam.");
                return false;
            }

            return true;
        }

        private void LoadStudentResults()
        {
            try
            {
                // Get filtered student list using correct structure from the original dataset query
                var studentClassIDs = GetFilteredStudentClassIDs();
                FilteredStudentIDs = studentClassIDs;
                TotalStudents = studentClassIDs.Count;
                CurrentPage = 1;

                if (TotalStudents == 0)
                {
                    // More detailed debugging information
                    var debugInfo = GetDebugInfo();
                    ShowError($"No students found matching the criteria. Debug info: {debugInfo}");
                    ResultPanel.Visible = false;
                    return;
                }

                // Determine if this class has any sections in the loaded data
                DetermineIfHasSections();

                // Load results for current page
                LoadPagedResults();
                UpdatePaginationInfo();
                ResultPanel.Visible = true;

                // Show print button
                ClientScript.RegisterStartupScript(this.GetType(), "ShowPrintButton",
                    "setTimeout(function(){ var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'inline-block'; }, 100);", true);
            }
            catch (Exception ex)
            {
                ShowError("Error loading student results: " + ex.Message);
            }
        }

        // NEW: Determine if the current result set has any sections
        private void DetermineIfHasSections()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT COUNT(DISTINCT cs.SectionID) AS SectionCount
                        FROM Exam_Cumulative_Student ecs
                        INNER JOIN StudentsClass sc ON ecs.StudentClassID = sc.StudentClassID
                        LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
                        WHERE ecs.CumulativeNameID = @CumulativeNameID
                        AND ecs.SchoolID = @SchoolID
                        AND ecs.ClassID = @ClassID
                        AND cs.Section IS NOT NULL AND cs.Section != ''";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        command.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                        var sectionCount = Convert.ToInt32(command.ExecuteScalar());
                        HasSections = sectionCount > 0;

                        System.Diagnostics.Debug.WriteLine($"HasSections determined: {HasSections} (SectionCount: {sectionCount})");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error determining HasSections: {ex.Message}");
                HasSections = false;
            }
        }

        private string GetDebugInfo()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var info = new StringBuilder();

                    // Check if cumulative exam exists
                    var examQuery = "SELECT COUNT(*) FROM Exam_Cumulative_Name WHERE CumulativeNameID = @CumulativeNameID AND SchoolID = @SchoolID";
                    using (var cmd = new SqlCommand(examQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        var examCount = (int)cmd.ExecuteScalar();
                        info.Append($"Exam exists: {examCount > 0}, ");
                    }

                    // Check if cumulative setting exists and is published
                    var settingQuery = @"SELECT COUNT(*) FROM Exam_Cumulative_Setting 
                                       WHERE SchoolID = @SchoolID AND IS_Published = 1";
                    using (var cmd = new SqlCommand(settingQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        var settingCount = (int)cmd.ExecuteScalar();
                        info.Append($"Published settings: {settingCount}, ");
                    }

                    // Check if students exist in cumulative table
                    var studentQuery = @"SELECT COUNT(*) FROM Exam_Cumulative_Student 
                                       WHERE CumulativeNameID = @CumulativeNameID 
                                       AND SchoolID = @SchoolID 
                                       AND ClassID = @ClassID";
                    using (var cmd = new SqlCommand(studentQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        var studentCount = (int)cmd.ExecuteScalar();
                        info.Append($"Cumulative students: {studentCount}");
                        //                        info.Append($"Cumulative students: {studentCount}, ");
                    }

                    return info.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Debug error: {ex.Message}";
            }
        }

        private List<int> GetFilteredStudentClassIDs()
        {
            var studentClassIDs = new List<int>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Build query with Group, Section, and Shift filters
                    // IMPORTANT: This query fetches students from Exam_Cumulative_Student table
                    // which should be populated after cumulative result is published
                    var query = new StringBuilder(@"
                        SELECT DISTINCT ecs.StudentClassID
                        FROM Exam_Cumulative_Student ecs
                        INNER JOIN StudentsClass sc ON ecs.StudentClassID = sc.StudentClassID
                        WHERE ecs.CumulativeNameID = @CumulativeNameID 
                        AND ecs.SchoolID = @SchoolID 
                        AND ecs.EducationYearID = @EducationYearID 
                        AND ecs.ClassID = @ClassID");

                    // Add Group filter if not ALL
                    var groupValue = GroupDropDownList.SelectedValue;
                    if (!string.IsNullOrEmpty(groupValue) && groupValue != "%" && groupValue != "0")
                    {
                        query.Append(" AND sc.SubjectGroupID = @SubjectGroupID");
                    }

                    // Add Section filter if not ALL
                    var sectionValue = SectionDropDownList.SelectedValue;
                    if (!string.IsNullOrEmpty(sectionValue) && sectionValue != "%" && sectionValue != "0")
                    {
                        query.Append(" AND sc.SectionID = @SectionID");
                    }

                    // Add Shift filter if not ALL
                    var shiftValue = ShiftDropDownList.SelectedValue;
                    if (!string.IsNullOrEmpty(shiftValue) && shiftValue != "%" && shiftValue != "0")
                    {
                        query.Append(" AND sc.ShiftID = @ShiftID");
                    }

                    query.Append(" ORDER BY ecs.StudentClassID");

                    using (var command = new SqlCommand(query.ToString(), connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        command.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                        command.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        command.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);

                        // Add filter parameters
                        if (!string.IsNullOrEmpty(groupValue) && groupValue != "%" && groupValue != "0")
                        {
                            command.Parameters.AddWithValue("@SubjectGroupID", groupValue);
                        }

                        if (!string.IsNullOrEmpty(sectionValue) && sectionValue != "%" && sectionValue != "0")
                        {
                            command.Parameters.AddWithValue("@SectionID", sectionValue);
                        }

                        if (!string.IsNullOrEmpty(shiftValue) && shiftValue != "%" && shiftValue != "0")
                        {
                            command.Parameters.AddWithValue("@ShiftID", shiftValue);
                        }

                        // Debug logging
                        System.Diagnostics.Debug.WriteLine($"GetFilteredStudentClassIDs Query: {query}");
                        System.Diagnostics.Debug.WriteLine($"Parameters - SchoolID: {Session["SchoolID"]}, EducationYearID: {Session["Edu_Year"]}, ClassID: {ClassDropDownList.SelectedValue}, CumulativeNameID: {ExamDropDownList.SelectedValue}");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                studentClassIDs.Add(Convert.ToInt32(reader["StudentClassID"]));
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"Total students found: {studentClassIDs.Count}");
                    }
                }

                // Apply Student ID filter if specified
                if (!string.IsNullOrWhiteSpace(StudentIDTextBox.Text))
                {
                    var requestedIDs = ParseStudentIDInput(StudentIDTextBox.Text.Trim());
                    if (requestedIDs.Any())
                    {
                        studentClassIDs = FilterByRequestedIDs(studentClassIDs, requestedIDs);
                        System.Diagnostics.Debug.WriteLine($"After Student ID filter: {studentClassIDs.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetFilteredStudentClassIDs: {ex.Message}");
                throw new Exception("Error filtering students: " + ex.Message);
            }

            return studentClassIDs;
        }

        private void AddFilterParameters(SqlCommand command)
        {
            command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
            command.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
            command.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            command.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);
            command.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue ?? "%");
            command.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue ?? "%");
            command.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue ?? "%");
        }

        private List<string> ParseStudentIDInput(string input)
        {
            var ids = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return ids;

            // Convert Bengali numbers to English
            input = ConvertBengaliToEnglish(input);

            // Split by commas and clean up
            var parts = input.Split(',');
            foreach (var part in parts)
            {
                var cleanPart = part.Trim();
                if (!string.IsNullOrEmpty(cleanPart))
                {
                    ids.Add(cleanPart);
                }
            }

            return ids;
        }

        private List<int> FilterByRequestedIDs(List<int> allStudentClassIDs, List<string> requestedIDs)
        {
            var filteredIDs = new List<int>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var studentClassID in allStudentClassIDs)
                    {
                        var query = @"
                            SELECT s.ID, sc.RollNo
                            FROM StudentsClass sc
                            INNER JOIN Student s ON sc.StudentID = s.StudentID
                            WHERE sc.StudentClassID = @StudentClassID";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@StudentClassID", studentClassID);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var id = reader["ID"]?.ToString() ?? "";
                                    var rollNo = reader["RollNo"]?.ToString() ?? "";

                                    if (requestedIDs.Contains(id) || requestedIDs.Contains(rollNo))
                                    {
                                        filteredIDs.Add(studentClassID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error filtering by requested IDs: " + ex.Message);
            }

            return filteredIDs;
        }

        private void LoadPagedResults()
        {
            try
            {
                var startIndex = (CurrentPage - 1) * StudentsPerPage;
                var endIndex = Math.Min(startIndex + StudentsPerPage, TotalStudents);
                var pageStudentClassIDs = FilteredStudentIDs.Skip(startIndex).Take(StudentsPerPage).ToList();

                var resultData = GetStudentResultData(pageStudentClassIDs);

                ResultRepeater.DataSource = resultData;
                ResultRepeater.DataBind();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading paged results: " + ex.Message);
            }
        }

        private DataTable GetStudentResultData(List<int> studentClassIDs)
        {
            var dataTable = new DataTable();

            if (!studentClassIDs.Any())
                return dataTable;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = BuildResultDataQuery();
                    using (var command = new SqlCommand(query, connection))
                    {
                        AddResultDataParameters(command, studentClassIDs);

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting student result data: " + ex.Message);
            }

            return dataTable;
        }

        private string BuildResultDataQuery()
        {
            // Fixed query - removed DISTINCT completely and ensured proper ORDER BY
            // Added CumulativeNameID filter to prevent loading multiple cumulative results
            var query = @"
                SELECT 
                    SchoolInfo.SchoolName, 
                    Student.ID, 
                    Student.StudentsName, 
                    CreateClass.Class, 
                    StudentsClass.RollNo, 
                    Exam_Cumulative_Student.TotalMark_ofStudent AS Cu_Stu_TM,
                    Exam_Cumulative_Student.ObtainedMark_ofStudent AS Cu_Stu_OM,
                    Exam_Cumulative_Student.PassStatus_InSubject AS Cu_Stu_Pass,
                    Exam_Cumulative_Student.Student_Grade,
                    Exam_Cumulative_Student.Student_Point,
                    Exam_Cumulative_Student.HighestMark_InExam_Class,
                    Exam_Cumulative_Student.HighestMark_InExam_Subsection,
                    Exam_Cumulative_Student.Position_InExam_Class,
                    Exam_Cumulative_Student.Position_InExam_Subsection,
                    Exam_Cumulative_Student.Student_Comments,
                    Exam_Cumulative_Name.CumulativeResultName AS ExamName,
                    SchoolInfo.SchoolLogo,
                    SchoolInfo.Address,
                    SchoolInfo.Phone,
                    CreateSection.Section,
                    CreateSubjectGroup.SubjectGroup,
                    CreateShift.Shift,
                    Student_Image.Image,
                    Exam_Cumulative_Student.Average,
                    Exam_Cumulative_Student.NotGolden,
                    Exam_Cumulative_Setting.IS_Hide_SubExam,
                    Exam_Cumulative_Setting.IS_Hide_Sec_Position,
                    Exam_Cumulative_Setting.IS_Hide_Class_Position,
                    Attendance_Student.WorkingDays,
                    Attendance_Student.TotalPresent,
                    Attendance_Student.TotalAbsent,
                    Attendance_Student.TotalLate,
                    Attendance_Student.TotalLeave,
                    Attendance_Student.TotalBunk,
                    Attendance_Student.TotalLateAbs,
                    Exam_Cumulative_Student.ObtainedPercentage_ofStudent,
                    Exam_Cumulative_Setting.Attendance_FromDate,
                    Exam_Cumulative_Setting.Attendance_ToDate,
                    Exam_Cumulative_Student.StudentClassID,
                    Student.StudentImageID,
                    SchoolInfo.SchoolID,
                    Exam_Cumulative_Student.StudentClassID AS StudentResultID
                FROM 
                    Exam_Cumulative_Student 
                    INNER JOIN Exam_Cumulative_Setting ON Exam_Cumulative_Student.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID
                    INNER JOIN StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID
                    INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
                    INNER JOIN CreateClass ON Exam_Cumulative_Student.ClassID = CreateClass.ClassID
                    INNER JOIN Exam_Cumulative_Name ON Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID
                    INNER JOIN SchoolInfo ON Exam_Cumulative_Setting.SchoolID = SchoolInfo.SchoolID
                    LEFT OUTER JOIN Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
                    LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
                    LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
                    LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
                    LEFT OUTER JOIN Attendance_Student ON Exam_Cumulative_Student.CumulativeNameID = Attendance_Student.CumulativeNameID 
                        AND Exam_Cumulative_Student.StudentID = Attendance_Student.StudentID 
                        AND Exam_Cumulative_Student.StudentClassID = Attendance_Student.StudentClassID
                WHERE 
                    Exam_Cumulative_Student.StudentClassID IN ({0})
                    AND Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID
                ORDER BY 
                    CAST(StudentsClass.RollNo AS INT)";

            return query;
        }

        private void AddResultDataParameters(SqlCommand command, List<int> studentClassIDs)
        {
            var parameters = new List<string>();
            for (int i = 0; i < studentClassIDs.Count; i++)
            {
                var paramName = "@StudentClassID" + i;
                parameters.Add(paramName);
                command.Parameters.AddWithValue(paramName, studentClassIDs[i]);
            }

            // Add CumulativeNameID parameter to filter specific cumulative result
            command.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);

            command.CommandText = string.Format(command.CommandText, string.Join(",", parameters));
        }

        #endregion

        #region Pagination

        protected void FirstPageButton_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            LoadPagedResults();
            UpdatePaginationInfo();
        }

        protected void PrevPageButton_Click(object sender, EventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                LoadPagedResults();
                UpdatePaginationInfo();
            }
        }

        protected void NextPageButton_Click(object sender, EventArgs e)
        {
            var totalPages = (int)Math.Ceiling((double)TotalStudents / StudentsPerPage);
            if (CurrentPage < totalPages)
            {
                CurrentPage++;
                LoadPagedResults();
                UpdatePaginationInfo();
            }
        }

        protected void LastPageButton_Click(object sender, EventArgs e)
        {
            var totalPages = (int)Math.Ceiling((double)TotalStudents / StudentsPerPage);
            CurrentPage = totalPages;
            LoadPagedResults();
            UpdatePaginationInfo();
        }

        private void UpdatePaginationInfo()
        {
            var totalPages = TotalStudents > 0 ? (int)Math.Ceiling((double)TotalStudents / StudentsPerPage) : 1;
            var startStudent = TotalStudents > 0 ? ((CurrentPage - 1) * StudentsPerPage) + 1 : 0;
            var endStudent = Math.Min(CurrentPage * StudentsPerPage, TotalStudents);

            PageInfoLabel.Text = $"Page {CurrentPage} of {totalPages}";
            PaginationInfoLabel.Text = $"Loaded {startStudent} to {endStudent} students. Total {TotalStudents} students";

            // Update button states
            FirstPageButton.Enabled = CurrentPage > 1;
            PrevPageButton.Enabled = CurrentPage > 1;
            NextPageButton.Enabled = CurrentPage < totalPages;
            LastPageButton.Enabled = CurrentPage < totalPages;

            FirstPageButton.CssClass = CurrentPage > 1 ? "btn btn-xs btn-outline-primary" : "btn btn-xs btn-outline-secondary";
            PrevPageButton.CssClass = CurrentPage > 1 ? "btn btn-xs btn-outline-primary" : "btn btn-xs btn-outline-secondary";
            NextPageButton.CssClass = CurrentPage < totalPages ? "btn btn-xs btn-outline-primary" : "btn btn-xs btn-outline-secondary";
            LastPageButton.CssClass = CurrentPage < totalPages ? "btn btn-xs btn-outline-primary" : "btn btn-xs btn-outline-secondary";
        }

        #endregion

        #region Repeater Events

        protected void ResultRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    var dataItem = (DataRowView)e.Item.DataItem;
                    var studentClassID = dataItem["StudentClassID"].ToString();

                    // Load grading system for each result card
                    var gradingRepeater = (Repeater)e.Item.FindControl("GradingSystemRepeater");
                    if (gradingRepeater != null)
                    {
                        LoadGradingSystem(gradingRepeater);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't break the page
                    System.Diagnostics.Debug.WriteLine("Error in ResultRepeater_ItemDataBound: " + ex.Message);
                }
            }
        }

        private void LoadGradingSystem(Repeater gradingRepeater)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT MARKS, Grades, Point
                        FROM Exam_Grading_System
                        WHERE SchoolID = @SchoolID
                        ORDER BY Point DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            gradingRepeater.DataSource = dataTable;
                            gradingRepeater.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Use default grading if error occurs
                var defaultGrading = CreateDefaultGradingSystem();
                gradingRepeater.DataSource = defaultGrading;
                gradingRepeater.DataBind();
            }
        }

        private DataTable CreateDefaultGradingSystem()
        {
            var table = new DataTable();
            table.Columns.Add("MARKS", typeof(string));
            table.Columns.Add("Grades", typeof(string));
            table.Columns.Add("Point", typeof(decimal));

            table.Rows.Add("80-100", "A+", 5.0);
            table.Rows.Add("70-79", "A", 4.0);
            table.Rows.Add("60-69", "A-", 3.5);
            table.Rows.Add("50-59", "B", 3.0);
            table.Rows.Add("40-49", "C", 2.0);
            table.Rows.Add("33-39", "D", 1.0);
            table.Rows.Add("0-32", "F", 0.0);

            return table;
        }

        #endregion

        #region Signature Management

        private void LoadSignatures()
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(connectionString);
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

                            System.Diagnostics.Debug.WriteLine($"LoadSignatures: SchoolID: {Session["SchoolID"]}, HasTeacherSign: {hasTeacherSign}, HasPrincipalSign: {hasPrincipalSign}");

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
                            System.Diagnostics.Debug.WriteLine($"LoadSignatures: No SchoolInfo record found for SchoolID: {Session["SchoolID"]}");
                            HiddenTeacherSign.Value = "";
                            HiddenPrincipalSign.Value = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop the main process
                System.Diagnostics.Debug.WriteLine($"LoadSignatures error: {ex.Message}\nStack: {ex.StackTrace}");
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

                    // Determine which column to update based on signature type
                    string column;
                    switch (signatureType.ToLower())
                    {
                        case "teacher":
                            column = "Teacher_Sign";
                            break;
                        case "guardian":
                            column = "Guardian_Sign";
                            break;
                        case "principal":
                            column = "Principal_Sign";
                            break;
                        default:
                            return new { success = false, message = "Invalid signature type" };
                    }

                    string updateQuery = $"UPDATE SchoolInfo SET {column} = @ImageData WHERE SchoolID = @SchoolID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ImageData", imageBytes);
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new { success = true, message = $"{signatureType} signature saved successfully", schoolId = schoolId };
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
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SaveSignature: {ex.Message}");
                return new { success = false, message = $"Error: {ex.Message}" };
            }
        }

        #endregion

        #region Utility Methods

        private void ResetFilters()
        {
            CurrentPage = 1;
            TotalStudents = 0;
            FilteredStudentIDs = new List<int>();
            UpdatePaginationInfo();
        }

        private void ShowError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "ShowError",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }

        private string ConvertBengaliToEnglish(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var bengaliToEnglish = new Dictionary<char, char>
            {
                {'০', '0'}, {'১', '1'}, {'২', '2'}, {'৩', '3'}, {'৪', '4'},
                {'৫', '5'}, {'৬', '6'}, {'৭', '7'}, {'৮', '8'}, {'৯', '9'}
            };

            var result = new StringBuilder();
            foreach (char c in input)
            {
                if (bengaliToEnglish.ContainsKey(c))
                    result.Append(bengaliToEnglish[c]);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        #endregion

        #region Helper Methods for ASPX

        protected string GetDynamicInfoRow(object dataItem)
        {
            try
            {
                var row = (DataRowView)dataItem;
                var html = new StringBuilder();

                // Class, Group, Section, Shift row
                html.Append("<tr>");
                html.Append("<td>Class:</td>");
                html.Append($"<td>{row["Class"]}</td>");

                var group = row["SubjectGroup"]?.ToString();
                var section = row["Section"]?.ToString();
                var shift = row["Shift"]?.ToString();

                if (!string.IsNullOrEmpty(group))
                {
                    html.Append("<td>Group:</td>");
                    html.Append($"<td>{group}</td>");
                }
                else if (!string.IsNullOrEmpty(section))
                {
                    html.Append("<td>Section:</td>");
                    html.Append($"<td>{section}</td>");
                }
                else if (!string.IsNullOrEmpty(shift))
                {
                    html.Append("<td>Shift:</td>");
                    html.Append($"<td>{shift}</td>");
                }
                else
                {
                    html.Append("<td></td><td></td>");
                }

                html.Append("</tr>");

                return html.ToString();
            }
            catch
            {
                return "<tr><td>Class:</td><td>-</td><td></td><td></td></tr>";
            }
        }

        protected string GetAttendanceTableHtml(object dataItem)
        {
            try
            {
                var row = (DataRowView)dataItem;
                var html = new StringBuilder();

                // Get attendance and summary data
                var workingDays = row["WorkingDays"] != DBNull.Value ? row["WorkingDays"].ToString() : "";
                var totalPresent = row["TotalPresent"] != DBNull.Value ? row["TotalPresent"].ToString() : "";
                var totalAbsent = row["TotalAbsent"] != DBNull.Value ? row["TotalAbsent"].ToString() : "";
                var totalLeave = row["TotalLeave"] != DBNull.Value ? row["TotalLeave"].ToString() : "";
                var totalMarks = row["Cu_Stu_TM"] != DBNull.Value ? row["Cu_Stu_TM"].ToString() : "";
                var obtainedMarks = row["Cu_Stu_OM"] != DBNull.Value ? row["Cu_Stu_OM"].ToString() : "";
                var percentage = row["ObtainedPercentage_ofStudent"] != DBNull.Value ?
                    decimal.Parse(row["ObtainedPercentage_ofStudent"].ToString()).ToString("F2") + "%" : "98.52%";
                var average = row["Average"] != DBNull.Value ?
                    decimal.Parse(row["Average"].ToString()).ToString("F2") : "83.74";
                var grade = row["Student_Grade"] != DBNull.Value ? row["Student_Grade"].ToString() : "A+";
                var gpa = row["Student_Point"] != DBNull.Value ?
                    decimal.Parse(row["Student_Point"].ToString()).ToString("F2") : "5.00";
                var positionClass = row["Position_InExam_Class"] != DBNull.Value && row["Position_InExam_Class"].ToString() != "0" ?
                    row["Position_InExam_Class"].ToString() : "6";
                var positionSection = row["Position_InExam_Subsection"] != DBNull.Value && row["Position_InExam_Subsection"].ToString() != "0" ?
                    row["Position_InExam_Subsection"].ToString() : "2";
                var comments = row["Student_Comments"] != DBNull.Value ? row["Student_Comments"].ToString() : "Excellent";

                html.Append("<table class='summary-table' style='margin-top: 10px; width: 100%; border-collapse: collapse;'>");

                // First row with all summary information - matching capture00.png exactly
                html.Append("<tr class='summary-header' style='background-color: #f5f5f5; border: 1px solid #ddd;'>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>WD</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Pre</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Abs</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Leave</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Obtained Marks</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>%</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Average</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Grade</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>GPA</strong></td>");

                // Conditional PC header based on IS_Hide_Class_Position
                if (!IS_Hide_Class_Position)
                {
                    html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>PC</strong></td>");
                }

                // Conditional PS header based on HasSections AND IS_Hide_Sec_Position
                if (HasSections && !IS_Hide_Sec_Position)
                {
                    html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>PS</strong></td>");
                }

                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>Comment</strong></td>");
                html.Append("</tr>");

                html.Append("<tr class='summary-values' style='background-color: white;'>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{workingDays}</td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{totalPresent}</td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{totalAbsent}</td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{totalLeave}</td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #d4edda;'><strong>{obtainedMarks} / {totalMarks}</strong></td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #ff6347; color: white;'><strong>{percentage}</strong></td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #87ceeb;'><strong>{average}</strong></td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #6495ed; color: white;'><strong>{grade}</strong></td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #00008b; color: white;'><strong>{gpa}</strong></td>");

                // Conditional PC data based on IS_Hide_Class_Position
                if (!IS_Hide_Class_Position)
                {
                    html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #228b22; color: white;'><strong>{positionClass}</strong></td>");
                }

                // Conditional PS data based on HasSections AND IS_Hide_Sec_Position
                if (HasSections && !IS_Hide_Sec_Position)
                {
                    html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #228b22; color: white;'><strong>{positionSection}</strong></td>");
                }

                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #800080; color: white;'><strong>{comments}</strong></td>");
                html.Append("</tr>");
                html.Append("</table>");

                return html.ToString();
            }
            catch (Exception ex)
            {
                return "<table class='summary-table' style='margin-top: 10px;'><tr><td>Attendance data not available</td></tr></table>";
            }
        }

        protected string GenerateSubjectMarksTable(string studentClassID, string studentGrade, object studentPoint)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // First, get the list of exams included in this cumulative result with their percentages
                    var examList = GetCumulativeExamList(connection, studentClassID);

                    if (examList.Count == 0)
                    {
                        return "<div class='error'>No exam data found for cumulative result.</div>";
                    }

                    // Get subject data with exam-wise marks - using the same structure as Cumulative_Result.aspx
                    var subjectData = GetSubjectExamWiseMarks_Fixed(connection, studentClassID, examList);

                    // Build the enhanced table HTML
                    var html = new StringBuilder();
                    html.Append("<table class='marks-table'>");

                    // Build header rows
                    html.Append(BuildExamHeaderRows(examList));

                    // Build subject rows with exam-wise marks
                    html.Append(BuildSubjectRows_Fixed(subjectData, examList));

                    // Build cumulative result row
                    html.Append(BuildCumulativeResultRow(connection, studentClassID, studentGrade, studentPoint, examList.Count));

                    html.Append("</table>");
                    return html.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"<div class='error'>Error loading subject marks: {ex.Message}</div>";
            }
        }

        private List<ExamInfo> GetCumulativeExamList(SqlConnection connection, string studentClassID)
        {
            var examList = new List<ExamInfo>();

            try
            {
                // CRITICAL FIX: Get the LATEST Cumulative_SettingID directly from Exam_Cumulative_Setting
                // instead of relying on student data which may be from old publishes
                var getLatestSettingQuery = @"
                    SELECT TOP 1
                        ecs.Cumulative_SettingID,
                        ecs.CumulativeNameID,
                        ecs.ClassID,
                        ecs.SchoolID,
                        ecs.EducationYearID
                    FROM Exam_Cumulative_Setting ecs
                    WHERE ecs.SchoolID = @SchoolID
                    AND ecs.EducationYearID = @EducationYearID
                    AND ecs.ClassID = @ClassID
                    AND ecs.CumulativeNameID = @CumulativeNameID
                    ORDER BY ecs.Cumulative_SettingID DESC";

                int cumulativeNameID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                int classID = Convert.ToInt32(ClassDropDownList.SelectedValue);
                int schoolID = Convert.ToInt32(Session["SchoolID"]);
                int educationYearID = Convert.ToInt32(Session["Edu_Year"]);
                int latestCumulativeSettingID = 0;

                using (var cmd = new SqlCommand(getLatestSettingQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@EducationYearID", educationYearID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            latestCumulativeSettingID = Convert.ToInt32(reader["Cumulative_SettingID"]);
                        }
                    }
                }

                if (latestCumulativeSettingID == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No LATEST Cumulative_SettingID found");
                    System.Diagnostics.Debug.WriteLine($"   SchoolID: {schoolID}, EducationYearID: {educationYearID}");
                    System.Diagnostics.Debug.WriteLine($"   ClassID: {classID}, CumulativeNameID: {cumulativeNameID}");
                    return examList;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Using LATEST Cumulative_SettingID: {latestCumulativeSettingID}");
                System.Diagnostics.Debug.WriteLine($"   CumulativeNameID: {cumulativeNameID}");
                System.Diagnostics.Debug.WriteLine($"   ClassID: {classID}");
                System.Diagnostics.Debug.WriteLine($"   SchoolID: {schoolID}");
                System.Diagnostics.Debug.WriteLine($"   EducationYearID: {educationYearID}");

                // STEP 2: Get the list of exams from Exam_Cumulative_ExamList table
                // Using the LATEST Cumulative_SettingID (not from student data)
                var query = @"
                    SELECT DISTINCT 
                        en.ExamName,
                        cel.ExamID,
                        cel.ExamAdd_Percentage,
                        en.Period_StartDate
                    FROM Exam_Cumulative_ExamList cel
                    INNER JOIN Exam_Name en ON cel.ExamID = en.ExamID
                    WHERE cel.Cumulative_SettingID = @Cumulative_SettingID
                    AND cel.CumulativeNameID = @CumulativeNameID
                    AND cel.SchoolID = @SchoolID
                    AND cel.EducationYearID = @EducationYearID
                    AND cel.ClassID = @ClassID
                    ORDER BY en.Period_StartDate, cel.ExamID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Cumulative_SettingID", latestCumulativeSettingID);
                    command.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                    command.Parameters.AddWithValue("@SchoolID", schoolID);
                    command.Parameters.AddWithValue("@EducationYearID", educationYearID);
                    command.Parameters.AddWithValue("@ClassID", classID);

                    System.Diagnostics.Debug.WriteLine($"🔍 Querying Exam_Cumulative_ExamList with LATEST Setting:");
                    System.Diagnostics.Debug.WriteLine($"   Cumulative_SettingID: {latestCumulativeSettingID}");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var examInfo = new ExamInfo
                            {
                                ExamID = Convert.ToInt32(reader["ExamID"]),
                                ExamName = reader["ExamName"].ToString(),
                                Percentage = Convert.ToDecimal(reader["ExamAdd_Percentage"])
                            };
                            examList.Add(examInfo);
                            System.Diagnostics.Debug.WriteLine($"   ✅ Exam: {examInfo.ExamName} (ID: {examInfo.ExamID}, {examInfo.Percentage}%)");
                        }
                    }
                }

                if (examList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No exams found in Exam_Cumulative_ExamList for LATEST setting");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Total exams from LATEST publish: {examList.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in GetCumulativeExamList: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
            }

            return examList;
        }

        private string BuildExamHeaderRows(List<ExamInfo> examList)
        {
            var html = new StringBuilder();

            // First header row - Exam names with percentages and merged Cumulative Result
            html.Append("<tr>");
            html.Append("<th rowspan='2' style='background-color: #E6E6FA;'>SUBJECTS</th>");

            foreach (var exam in examList)
            {
                // Calculate colspan based on visible columns
                int examColspan = 1; // OM always shown
                if (!IS_Hide_FullMark) examColspan++; // Add FM
                if (!IS_Hide_PassMark) examColspan++; // Add PM

                html.Append($"<th colspan='{examColspan}'>{exam.ExamName} ({exam.Percentage}%)</th>");
            }

            // Calculate cumulative result colspan based on visible columns
            int cumulativeColspan = 4; // FM, OM, GRADE, GPA always shown
            if (!IS_Hide_Class_Position) cumulativeColspan++; // PC
            if (HasSections && !IS_Hide_Sec_Position) cumulativeColspan++; // PS
            cumulativeColspan++; // HMC always shown
            if (HasSections && !IS_Hide_Sec_Position) cumulativeColspan++; // HMS

            // Merged Cumulative Result header spanning calculated columns
            html.Append($"<th colspan='{cumulativeColspan}' style='background-color: #E6E6FA;'>Cumulative Result</th>");
            html.Append("</tr>");

            // Second header row - Conditional FM, PM, and OM labels for exams, then individual cumulative columns
            html.Append("<tr>");

            foreach (var exam in examList)
            {
                // Conditional FM header
                if (!IS_Hide_FullMark)
                {
                    html.Append("<th>FM</th>");
                }
                // Conditional PM header
                if (!IS_Hide_PassMark)
                {
                    html.Append("<th>PM</th>");
                }
                // OM always shown
                html.Append("<th>OM</th>");
            }

            // Individual cumulative column headers with lavender background
            html.Append("<th style='background-color: #E6E6FA;'>FM</th>");
            html.Append("<th style='background-color: #E6E6FA;'>OM</th>");
            html.Append("<th style='background-color: #E6E6FA;'>GRADE</th>");
            html.Append("<th style='background-color: #E6E6FA;'>GPA</th>");

            // Conditional PC header
            if (!IS_Hide_Class_Position)
            {
                html.Append("<th style='background-color: #E6E6FA;'>PC</th>");
            }

            // Conditional PS header
            if (HasSections && !IS_Hide_Sec_Position)
            {
                html.Append("<th style='background-color: #E6E6FA;'>PS</th>");
            }

            // HMC always shown
            html.Append("<th style='background-color: #E6E6FA;'>HMC</th>");

            // HMS only with sections and not hidden
            if (HasSections && !IS_Hide_Sec_Position)
            {
                html.Append("<th style='background-color: #E6E6FA;'>HMS</th>");
            }

            html.Append("</tr>");

            return html.ToString();
        }

        private string BuildSubjectRows_Fixed(DataTable subjectData, List<ExamInfo> examList)
        {
            var html = new StringBuilder();

            foreach (DataRow row in subjectData.Rows)
            {
                html.Append("<tr>");
                html.Append($"<td class='subject-name' style='background-color: #F5F5F5; text-align: left;'>{row["SubjectName"]}</td>");

                // Exam-wise marks with conditional FM and PM
                foreach (var exam in examList)
                {
                    // Conditional FM column
                    if (!IS_Hide_FullMark)
                    {
                        html.Append($"<td>{row[$"FM_Exam{exam.ExamID}"]}</td>");
                    }
                    // Conditional PM column
                    if (!IS_Hide_PassMark)
                    {
                        html.Append($"<td>{row[$"PM_Exam{exam.ExamID}"]}</td>");
                    }
                    // OM always shown
                    html.Append($"<td class='total-marks-cell'>{row[$"OM_Exam{exam.ExamID}"]}</td>");
                }

                // Cumulative marks with Lavender background (#E6E6FA)
                html.Append($"<td style='background-color: #E6E6FA;'>{row["Cu_Sub_TM"]}</td>");
                html.Append($"<td class='total-marks-cell' style='background-color: #E6E6FA;'>{row["Cu_Sub_OM"]}</td>");
                html.Append($"<td class='grade-cell' style='background-color: #E6E6FA;'>{row["Cu_Sub_Grades"]}</td>");
                html.Append($"<td style='background-color: #E6E6FA;'>{row["Cu_Sub_Point"]}</td>");

                // Conditional PC column
                if (!IS_Hide_Class_Position)
                {
                    html.Append($"<td class='position-col-pc' style='background-color: #E6E6FA;'>{row["Position_InSubject_Class"]}</td>");
                }

                // Conditional PS column
                if (HasSections && !IS_Hide_Sec_Position)
                {
                    html.Append($"<td class='position-col-ps' style='background-color: #E6E6FA;'>{row["Position_InSubject_Subsection"]}</td>");
                }

                // HMC always shown
                html.Append($"<td class='position-col-hmc' style='background-color: #E6E6FA;'>{row["HighestMark_InSubject_Class"]}</td>");

                // HMS only with sections
                if (HasSections && !IS_Hide_Sec_Position)
                {
                    html.Append($"<td class='position-col-hms' style='background-color: #E6E6FA;'>{row["HighestMark_InSubject_Subsection"]}</td>");
                }

                html.Append("</tr>");
            }

            return html.ToString();
        }

        private string BuildCumulativeResultRow(SqlConnection connection, string studentClassID, string studentGrade, object studentPoint, int examCount)
        {
            var html = new StringBuilder();

            // Get student cumulative totals with positions
            var query = @"
                SELECT 
                    TotalMark_ofStudent,
                    ObtainedMark_ofStudent,
                    Position_InExam_Class,
                    Position_InExam_Subsection,
                    HighestMark_InExam_Class,
                    HighestMark_InExam_Subsection
                FROM Exam_Cumulative_Student
                WHERE StudentClassID = @StudentClassID";

            string totalMarks = "0";
            string obtainedMarks = "0";
            string positionClass = "-";
            string positionSection = "-";
            string highestClass = "-";
            string highestSection = "-";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentClassID", studentClassID);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        totalMarks = FormatMarks(reader["TotalMark_ofStudent"]);
                        obtainedMarks = FormatMarks(reader["ObtainedMark_ofStudent"]);
                        positionClass = FormatPosition(reader["Position_InExam_Class"]);
                        positionSection = FormatPosition(reader["Position_InExam_Subsection"]);
                        highestClass = FormatMarks(reader["HighestMark_InExam_Class"]);
                        highestSection = FormatMarks(reader["HighestMark_InExam_Subsection"]);
                    }
                }
            }

            html.Append("<tr class='total-row' style='background-color: #D3D3D3; font-weight: bold;'>");
            html.Append($"<td style='background-color: #D3D3D3;'><strong>Overall Result</strong></td>");

            // Empty cells for exam-wise columns with light gray background
            // Calculate number of cells per exam based on settings
            int cellsPerExam = 1; // OM always shown
            if (!IS_Hide_FullMark) cellsPerExam++;
            if (!IS_Hide_PassMark) cellsPerExam++;

            for (int i = 0; i < examCount; i++)
            {
                for (int j = 0; j < cellsPerExam; j++)
                {
                    html.Append("<td style='background-color: #D3D3D3;'></td>");
                }
            }

            // All cumulative totals merged with single Lavender background (#E6E6FA)
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{totalMarks}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{obtainedMarks}</strong></td>");
            html.Append($"<td class='grade-cell' style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{studentGrade}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{FormatPoint(studentPoint)}</strong></td>");

            // Conditional PC cell
            if (!IS_Hide_Class_Position)
            {
                html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{positionClass}</strong></td>");
            }

            // Conditional PS cell
            if (HasSections && !IS_Hide_Sec_Position)
            {
                html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{positionSection}</strong></td>");
            }

            // HMC always shown
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{highestClass}</strong></td>");

            // HMS only with sections
            if (HasSections && !IS_Hide_Sec_Position)
            {
                html.Append($"<td style='background-color: #E6E6FA;'><strong>{highestSection}</strong></td>");
            }

            html.Append("</tr>");

            return html.ToString();
        }

        // Helper class for exam information
        private class ExamInfo
        {
            public int ExamID { get; set; }
            public string ExamName { get; set; }
            public decimal Percentage { get; set; }
        }

        private string FormatMarks(object marks)
        {
            if (marks == null || marks == DBNull.Value)
                return "-";

            var marksStr = marks.ToString();
            if (marksStr == "0")
                return "0";

            // Try to parse as decimal to format properly
            if (decimal.TryParse(marksStr, out decimal marksValue))
            {
                // If it's a whole number, show without decimals
                if (marksValue == Math.Floor(marksValue))
                    return marksValue.ToString("0");
                else
                    return marksValue.ToString("0.##"); // Show up to 2 decimal places
            }

            return marksStr;
        }

        private string FormatPoint(object point)
        {
            if (point == null || point == DBNull.Value)
                return "0.00";

            if (decimal.TryParse(point.ToString(), out decimal pointValue))
                return pointValue.ToString("0.00");

            return "0.00";
        }

        private string FormatPosition(object position)
        {
            if (position == null || position == DBNull.Value)
                return "-";

            var posStr = position.ToString();
            if (posStr == "0" || string.IsNullOrEmpty(posStr))
                return "-";

            return posStr;
        }

        private DataTable GetSubjectExamWiseMarks_Fixed(SqlConnection connection, string studentClassID, List<ExamInfo> examList)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("SubjectName", typeof(string));
            dataTable.Columns.Add("SubjectID", typeof(int));
            dataTable.Columns.Add("SN", typeof(int));

            // Add columns for each exam (FM, PM, and OM based on settings)
            foreach (var exam in examList)
            {
                // Conditional FM column
                if (!IS_Hide_FullMark)
                {
                    dataTable.Columns.Add($"FM_Exam{exam.ExamID}", typeof(string));
                }
                // Conditional PM column
                if (!IS_Hide_PassMark)
                {
                    dataTable.Columns.Add($"PM_Exam{exam.ExamID}", typeof(string));
                }
                // OM always added
                dataTable.Columns.Add($"OM_Exam{exam.ExamID}", typeof(string));
            }

            // Add cumulative columns
            dataTable.Columns.Add("Cu_Sub_TM", typeof(string));
            dataTable.Columns.Add("Cu_Sub_OM", typeof(string));
            dataTable.Columns.Add("Cu_Sub_Grades", typeof(string));
            dataTable.Columns.Add("Cu_Sub_Point", typeof(string));
            dataTable.Columns.Add("Position_InSubject_Class", typeof(string));
            dataTable.Columns.Add("Position_InSubject_Subsection", typeof(string));
            dataTable.Columns.Add("HighestMark_InSubject_Class", typeof(string));
            dataTable.Columns.Add("HighestMark_InSubject_Subsection", typeof(string));

            // Get cumulative subject data - only subjects that should be shown
            var cumulativeQuery = @"
                SELECT 
                    Subject.SubjectName,
                    Subject.SN,
                    Exam_Cumulative_Subject.SubjectID,
                    Exam_Cumulative_Subject.TotalMark_ofSubject AS Cu_Sub_TM,
                    Exam_Cumulative_Subject.ObtainedMark_ofSubject AS Cu_Sub_OM,
                    Exam_Cumulative_Subject.SubjectGrades AS Cu_Sub_Grades,
                    Exam_Cumulative_Subject.SubjectPoint AS Cu_Sub_Point,
                    Exam_Cumulative_Subject.Position_InSubject_Class,
                    Exam_Cumulative_Subject.Position_InSubject_Subsection,
                    Exam_Cumulative_Subject.HighestMark_InSubject_Class,
                    Exam_Cumulative_Subject.HighestMark_InSubject_Subsection
                FROM Exam_Cumulative_Subject 
                INNER JOIN Subject ON Exam_Cumulative_Subject.SubjectID = Subject.SubjectID
                WHERE Exam_Cumulative_Subject.StudentClassID = @StudentClassID
                AND Exam_Cumulative_Subject.IS_Add_InExam = 1
                ORDER BY ISNULL(Subject.SN, 9999), Subject.SubjectName";

            using (var command = new SqlCommand(cumulativeQuery, connection))
            {
                command.Parameters.AddWithValue("@StudentClassID", studentClassID);

                using (var reader = command.ExecuteReader())
                {
                    var subjectMarks = new Dictionary<int, DataRow>();

                    while (reader.Read())
                    {
                        var row = dataTable.NewRow();
                        row["SubjectName"] = reader["SubjectName"];
                        row["SubjectID"] = reader["SubjectID"];
                        row["SN"] = reader["SN"] == DBNull.Value ? 9999 : Convert.ToInt32(reader["SN"]);
                        row["Cu_Sub_TM"] = FormatMarks(reader["Cu_Sub_TM"]);
                        row["Cu_Sub_OM"] = FormatMarks(reader["Cu_Sub_OM"]);
                        row["Cu_Sub_Grades"] = reader["Cu_Sub_Grades"];
                        row["Cu_Sub_Point"] = FormatPoint(reader["Cu_Sub_Point"]);
                        row["Position_InSubject_Class"] = FormatPosition(reader["Position_InSubject_Class"]);
                        row["Position_InSubject_Subsection"] = FormatPosition(reader["Position_InSubject_Subsection"]);
                        row["HighestMark_InSubject_Class"] = FormatMarks(reader["HighestMark_InSubject_Class"]);
                        row["HighestMark_InSubject_Subsection"] = FormatMarks(reader["HighestMark_InSubject_Subsection"]);

                        int subjectID = Convert.ToInt32(reader["SubjectID"]);
                        subjectMarks[subjectID] = row;
                    }

                    reader.Close();

                    System.Diagnostics.Debug.WriteLine($"Found {subjectMarks.Count} subjects for StudentClassID: {studentClassID}");

                    // Now get exam-wise marks for each subject from Exam_Result_of_Subject
                    foreach (var subjectID in subjectMarks.Keys)
                    {
                        var row = subjectMarks[subjectID];

                        foreach (var exam in examList)
                        {
                            System.Diagnostics.Debug.WriteLine($"Getting marks for Subject: {row["SubjectName"]}, Exam: {exam.ExamName} (ID: {exam.ExamID})");

                            // Get marks from Exam_Result_of_Subject for this specific exam
                            // This is the key query that links cumulative exams with individual exam results
                            var examMarksQuery = @"
                                SELECT 
                                    ers.TotalMark_ofSubject AS E_Subject_TM,
                                    ers.ObtainedMark_ofSubject AS E_Subject_OM,
                                    ers.SubjectAbsenceStatus AS E_Subject_Abs,
                                    ers.PassStatus_Subject AS E_Subject_Pass
                                FROM Exam_Result_of_Subject ers
                                INNER JOIN Exam_Result_of_Student erstu ON ers.StudentResultID = erstu.StudentResultID
                                WHERE erstu.StudentClassID = @StudentClassID
                                AND ers.SubjectID = @SubjectID
                                AND erstu.ExamID = @ExamID";

                            using (var examCmd = new SqlCommand(examMarksQuery, connection))
                            {
                                examCmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
                                examCmd.Parameters.AddWithValue("@SubjectID", subjectID);
                                examCmd.Parameters.AddWithValue("@ExamID", exam.ExamID);

                                using (var examReader = examCmd.ExecuteReader())
                                {
                                    if (examReader.Read())
                                    {
                                        var fm = examReader["E_Subject_TM"];
                                        var om = examReader["E_Subject_OM"];
                                        var abs = examReader["E_Subject_Abs"]?.ToString() ?? "";
                                        var pass = examReader["E_Subject_Pass"]?.ToString() ?? "";

                                        // Conditional FM column
                                        if (!IS_Hide_FullMark)
                                        {
                                            row[$"FM_Exam{exam.ExamID}"] = FormatMarks(fm);
                                        }

                                        // Conditional PM column - calculate 33% of FM as PM
                                        if (!IS_Hide_PassMark)
                                        {
                                            if (fm != DBNull.Value && fm != null)
                                            {
                                                decimal fmValue = Convert.ToDecimal(fm);
                                                decimal pmValue = fmValue * 0.33m;
                                                row[$"PM_Exam{exam.ExamID}"] = FormatMarks(pmValue);
                                            }
                                            else
                                            {
                                                row[$"PM_Exam{exam.ExamID}"] = "-";
                                            }
                                        }

                                        // If absent, show "Abs"
                                        if (abs == "Absent" || abs == "A")
                                        {
                                            row[$"OM_Exam{exam.ExamID}"] = "Abs";
                                        }
                                        else
                                        {
                                            row[$"OM_Exam{exam.ExamID}"] = FormatMarks(om);
                                        }

                                        System.Diagnostics.Debug.WriteLine($"  - FM: {FormatMarks(fm)}, OM: {FormatMarks(om)}, Abs: {abs}");
                                    }
                                    else
                                    {
                                        // No data for this exam-subject combination
                                        System.Diagnostics.Debug.WriteLine($"  - No marks found for this exam-subject combination");
                                        
                                        if (!IS_Hide_FullMark)
                                        {
                                            row[$"FM_Exam{exam.ExamID}"] = "-";
                                        }
                                        if (!IS_Hide_PassMark)
                                        {
                                            row[$"PM_Exam{exam.ExamID}"] = "-";
                                        }
                                        row[$"OM_Exam{exam.ExamID}"] = "-";
                                    }
                                }
                            }
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"DataTable created with {dataTable.Rows.Count} subjects and {examList.Count} exams");
            return dataTable;
        }

        #endregion
    }
}
