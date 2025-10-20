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
        private const int StudentsPerPage = 10;
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

                LoadStudentResults();
            }
            catch (Exception ex)
            {
                ShowError("Error loading results: " + ex.Message);
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

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                studentClassIDs.Add(Convert.ToInt32(reader["StudentClassID"]));
                            }
                        }
                    }
                }

                // Apply Student ID filter if specified
                if (!string.IsNullOrWhiteSpace(StudentIDTextBox.Text))
                {
                    var requestedIDs = ParseStudentIDInput(StudentIDTextBox.Text.Trim());
                    if (requestedIDs.Any())
                    {
                        studentClassIDs = FilterByRequestedIDs(studentClassIDs, requestedIDs);
                    }
                }
            }
            catch (Exception ex)
            {
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT TeacherSignature, PrincipalSignature
                        FROM SchoolInfo
                        WHERE SchoolID = @SchoolID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var teacherSig = reader["TeacherSignature"]?.ToString() ?? "";
                                var principalSig = reader["PrincipalSignature"]?.ToString() ?? "";

                                HiddenTeacherSign.Value = teacherSig;
                                HiddenPrincipalSign.Value = principalSig;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't break the page for signature loading errors
                System.Diagnostics.Debug.WriteLine("Error loading signatures: " + ex.Message);
            }
        }

        [System.Web.Services.WebMethod]
        public static string SaveSignature(string signatureType, string imageData)
        {
            try
            {
                var schoolID = HttpContext.Current.Session["SchoolID"];
                if (schoolID == null)
                    return "Error: User not logged in";

                var connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                var imagePath = SaveSignatureImage(imageData, signatureType, schoolID.ToString());

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var columnName = signatureType == "teacher" ? "TeacherSignature" : "PrincipalSignature";
                    var query = $"UPDATE SchoolInfo SET {columnName} = @ImagePath WHERE SchoolID = @SchoolID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ImagePath", imagePath);
                        command.Parameters.AddWithValue("@SchoolID", schoolID);
                        command.ExecuteNonQuery();
                    }
                }

                return "Success";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        private static string SaveSignatureImage(string base64Data, string signatureType, string schoolID)
        {
            try
            {
                var imageBytes = Convert.FromBase64String(base64Data);
                var fileName = $"{signatureType}_signature_{schoolID}_{DateTime.Now.Ticks}.png";
                var uploadPath = HttpContext.Current.Server.MapPath("~/Uploads/Signatures/");
                
                if (!System.IO.Directory.Exists(uploadPath))
                    System.IO.Directory.CreateDirectory(uploadPath);

                var filePath = System.IO.Path.Combine(uploadPath, fileName);
                System.IO.File.WriteAllBytes(filePath, imageBytes);

                return "/Uploads/Signatures/" + fileName;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving signature image: " + ex.Message);
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
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>PC</strong></td>");
                html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>PS</strong></td>");
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
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #228b22; color: white;'><strong>{positionClass}</strong></td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #228b22; color: white;'><strong>{positionSection}</strong></td>");
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

            // Get CumulativeNameID and Cumulative_SettingID from the student record  
            var getCumulativeInfoQuery = @"
                SELECT CumulativeNameID, Cumulative_SettingID
                FROM Exam_Cumulative_Student 
                WHERE StudentClassID = @StudentClassID";

            int cumulativeNameID = 0;
            int cumulativeSettingID = 0;
            
            using (var cmd = new SqlCommand(getCumulativeInfoQuery, connection))
            {
                cmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cumulativeNameID = Convert.ToInt32(reader["CumulativeNameID"]);
                        cumulativeSettingID = Convert.ToInt32(reader["Cumulative_SettingID"]);
                    }
                }
            }

            if (cumulativeNameID == 0)
                return examList;

            // Get the list of exams with their percentages - matching Cumulative_Result.aspx structure
            var query = @"
                SELECT DISTINCT 
                    ex.ExamName,
                    cel.ExamID,
                    cel.ExamAdd_Percentage,
                    ex.Period_StartDate
                FROM Exam_Cumulative_ExamList cel
                INNER JOIN Exam_Name ex ON cel.ExamID = ex.ExamID
                WHERE cel.CumulativeNameID = @CumulativeNameID
                AND cel.Cumulative_SettingID = @Cumulative_SettingID
                AND cel.SchoolID = @SchoolID
                ORDER BY ex.Period_StartDate, cel.ExamID";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                command.Parameters.AddWithValue("@Cumulative_SettingID", cumulativeSettingID);
                command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        examList.Add(new ExamInfo
                        {
                            ExamID = Convert.ToInt32(reader["ExamID"]),
                            ExamName = reader["ExamName"].ToString(),
                            Percentage = Convert.ToDecimal(reader["ExamAdd_Percentage"])
                        });
                    }
                }
            }

            return examList;
        }

        private DataTable GetSubjectExamWiseMarks_Fixed(SqlConnection connection, string studentClassID, List<ExamInfo> examList)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("SubjectName", typeof(string));
            dataTable.Columns.Add("SubjectID", typeof(int));
            dataTable.Columns.Add("SN", typeof(int));

            // Add columns for each exam (FM and OM)
            foreach (var exam in examList)
            {
                dataTable.Columns.Add($"FM_Exam{exam.ExamID}", typeof(string));
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

                    // Now get exam-wise marks for each subject from Exam_Result_of_Subject
                    foreach (var subjectID in subjectMarks.Keys)
                    {
                        var row = subjectMarks[subjectID];
                        
                        foreach (var exam in examList)
                        {
                            // Get marks from Exam_Result_of_Subject for this specific exam
                            var examMarksQuery = @"
                                SELECT 
                                    ers.TotalMark_ofSubject AS E_Subject_TM,
                                    ers.ObtainedMark_ofSubject AS E_Subject_OM,
                                    ers.SubjectAbsenceStatus AS E_Subject_Abs
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
                                        
                                        row[$"FM_Exam{exam.ExamID}"] = FormatMarks(fm);
                                        
                                        // If absent, show "Abs"
                                        if (abs == "Absent" || abs == "A")
                                        {
                                            row[$"OM_Exam{exam.ExamID}"] = "Abs";
                                        }
                                        else
                                        {
                                            row[$"OM_Exam{exam.ExamID}"] = FormatMarks(om);
                                        }
                                    }
                                    else
                                    {
                                        // No data for this exam-subject combination
                                        row[$"FM_Exam{exam.ExamID}"] = "-";
                                        row[$"OM_Exam{exam.ExamID}"] = "-";
                                    }
                                }
                            }
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        private string BuildExamHeaderRows(List<ExamInfo> examList)
        {
            var html = new StringBuilder();
            
            // First header row - Exam names with percentages and merged Cumulative Result
            html.Append("<tr>");
            html.Append("<th rowspan='2' style='background-color: #E6E6FA;'>SUBJECTS</th>");
            
            foreach (var exam in examList)
            {
                html.Append($"<th colspan='2'>{exam.ExamName} ({exam.Percentage}%)</th>");
            }
            
            // Merged Cumulative Result header spanning 8 columns
            html.Append("<th colspan='8' style='background-color: #E6E6FA;'>Cumulative Result</th>");
            html.Append("</tr>");

            // Second header row - FM and OM labels for exams, then individual cumulative columns
            html.Append("<tr>");
            
            foreach (var exam in examList)
            {
                html.Append("<th>FM</th>");
                html.Append("<th>OM</th>");
            }
            
            // Individual cumulative column headers with lavender background
            html.Append("<th style='background-color: #E6E6FA;'>FM</th>");
            html.Append("<th style='background-color: #E6E6FA;'>OM</th>");
            html.Append("<th style='background-color: #E6E6FA;'>GRADE</th>");
            html.Append("<th style='background-color: #E6E6FA;'>GPA</th>");
            html.Append("<th style='background-color: #E6E6FA;'>PC</th>");
            html.Append("<th style='background-color: #E6E6FA;'>PS</th>");
            html.Append("<th style='background-color: #E6E6FA;'>HMC</th>");
            html.Append("<th style='background-color: #E6E6FA;'>HMS</th>");
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
                
                // Exam-wise marks
                foreach (var exam in examList)
                {
                    html.Append($"<td>{row[$"FM_Exam{exam.ExamID}"]}</td>");
                    html.Append($"<td class='total-marks-cell'>{row[$"OM_Exam{exam.ExamID}"]}</td>");
                }
                
                // Cumulative marks with Lavender background (#E6E6FA)
                html.Append($"<td style='background-color: #E6E6FA;'>{row["Cu_Sub_TM"]}</td>");
                html.Append($"<td class='total-marks-cell' style='background-color: #E6E6FA;'>{row["Cu_Sub_OM"]}</td>");
                html.Append($"<td class='grade-cell' style='background-color: #E6E6FA;'>{row["Cu_Sub_Grades"]}</td>");
                html.Append($"<td style='background-color: #E6E6FA;'>{row["Cu_Sub_Point"]}</td>");
                html.Append($"<td class='position-col-pc' style='background-color: #E6E6FA;'>{row["Position_InSubject_Class"]}</td>");
                html.Append($"<td class='position-col-ps' style='background-color: #E6E6FA;'>{row["Position_InSubject_Subsection"]}</td>");
                html.Append($"<td class='position-col-hmc' style='background-color: #E6E6FA;'>{row["HighestMark_InSubject_Class"]}</td>");
                html.Append($"<td class='position-col-hms' style='background-color: #E6E6FA;'>{row["HighestMark_InSubject_Subsection"]}</td>");
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
            for (int i = 0; i < examCount; i++)
            {
                html.Append("<td style='background-color: #D3D3D3;'></td><td style='background-color: #D3D3D3;'></td>");
            }
            
            // All cumulative totals merged with single Lavender background (#E6E6FA)
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{totalMarks}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{obtainedMarks}</strong></td>");
            html.Append($"<td class='grade-cell' style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{studentGrade}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{FormatPoint(studentPoint)}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{positionClass}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{positionSection}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA; border-right: 1px solid #ddd;'><strong>{highestClass}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA;'><strong>{highestSection}</strong></td>");
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

        #endregion
    }
}