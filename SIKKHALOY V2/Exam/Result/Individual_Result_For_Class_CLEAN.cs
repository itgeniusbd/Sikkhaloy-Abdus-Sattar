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

        // Cache grading data to avoid repeated queries
        private static Dictionary<string, DataTable> gradingCache = new Dictionary<string, DataTable>();

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

        // OPTIMIZATION: Don't store DataTable in ViewState - use Session instead for better performance
        private DataTable AllResultsData
        {
            get { return Session["AllResultsData"] as DataTable; }
            set { Session["AllResultsData"] = value; }
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

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // OPTIMIZATION: Disable ViewState for repeater to reduce page size
                ResultRepeater.EnableViewState = false;

                Session["Group"] = GroupDropDownList.SelectedValue;
                Session["Shift"] = ShiftDropDownList.SelectedValue;
                Session["Section"] = SectionDropDownList.SelectedValue;

                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                    CurrentPageIndex = 0;
                    HasSections = false; // default
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

                // reset section flag
                HasSections = false;

                // Hide print button when class changes
                SafeRegisterStartupScript("hidePrintOnClassChange", "var btn = document.getElementById('PrintButton'); if(btn) btn.style.display = 'none';");

                // Reset page title when class changes
                SafeRegisterStartupScript("resetTitleClassChange", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");
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

                        // Load publish settings before loading results
                        LoadPublishSettings();
                        LoadResultsData();
                    }
                    else
                    {
                        SafeRegisterStartupScript("alert", "alert('For Student ID search, please select both Class and Exam');");
                    }
                }
                else
                {
                    // For normal search, Class and Exam are required
                    if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0")
                    {
                        SafeRegisterStartupScript("debug1",
                            $"console.log('Loading results for Exam ID: {ExamDropDownList.SelectedValue}, Class ID: {ClassDropDownList.SelectedValue}');");

                        // Load publish settings before loading results
                        LoadPublishSettings();
                        LoadResultsData();
                    }
                    else
                    {
                        SafeRegisterStartupScript("alert", "alert('Please select both Class and Exam');");
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
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "invalidID",
                            "alert('Please enter valid Student IDs');", true);
                        return;
                    }

                    // Create IN clause for student IDs - they are already quoted in ParseStudentIDs
                    string idInClause = string.Join(",", studentIDs);

                    // OPTIMIZED query with WITH (NOLOCK) hints for faster reads
                    query = @"
                        SELECT DISTINCT
                            ers.StudentResultID,
                            ers.ObtainedMark_ofStudent,
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
                        FROM Exam_Result_of_Student ers WITH (NOLOCK)
                        INNER JOIN StudentsClass sc WITH (NOLOCK) ON ers.StudentClassID = sc.StudentClassID
                        INNER JOIN Student s WITH (NOLOCK) ON sc.StudentID = s.StudentID
                        INNER JOIN CreateClass cc WITH (NOLOCK) ON sc.ClassID = cc.ClassID
                        INNER JOIN Exam_Name en WITH (NOLOCK) ON ers.ExamID = en.ExamID
                        INNER JOIN SchoolInfo sch WITH (NOLOCK) ON ers.SchoolID = sch.SchoolID
                        LEFT JOIN CreateSection cs WITH (NOLOCK) ON sc.SectionID = cs.SectionID
                        LEFT JOIN CreateShift csh WITH (NOLOCK) ON sc.ShiftID = csh.ShiftID
                        LEFT JOIN CreateSubjectGroup csg WITH (NOLOCK) ON sc.SubjectGroupID = csg.SubjectGroupID
                        WHERE ers.ExamID = @ExamID
                        AND sc.ClassID = @ClassID
                        AND s.ID IN (" + idInClause + @")
                        AND ers.SchoolID = @SchoolID
                        AND ers.EducationYearID = @EducationYearID
                        ORDER BY s.ID";
                }
                else
                {
                    // OPTIMIZED query with WITH (NOLOCK) hints for faster reads
                    query = @"
                        SELECT DISTINCT
                            ers.StudentResultID,
                            ers.ObtainedMark_ofStudent,
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
                        FROM Exam_Result_of_Student ers WITH (NOLOCK)
                        INNER JOIN StudentsClass sc WITH (NOLOCK) ON ers.StudentClassID = sc.StudentClassID
                        INNER JOIN Student s WITH (NOLOCK) ON sc.StudentID = s.StudentID
                        INNER JOIN CreateClass cc WITH (NOLOCK) ON sc.ClassID = cc.ClassID
                        INNER JOIN Exam_Name en WITH (NOLOCK) ON ers.ExamID = en.ExamID
                        INNER JOIN SchoolInfo sch WITH (NOLOCK) ON ers.SchoolID = sch.SchoolID
                        LEFT JOIN CreateSection cs WITH (NOLOCK) ON sc.SectionID = cs.SectionID
                        LEFT JOIN CreateShift csh WITH (NOLOCK) ON sc.ShiftID = csh.ShiftID
                        LEFT JOIN CreateSubjectGroup csg WITH (NOLOCK) ON sc.SubjectGroupID = csg.SubjectGroupID
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
                    // OPTIMIZATION: Increase timeout to 60 seconds for large datasets
                    cmd.CommandTimeout = 60;
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
                        adapter.SelectCommand.CommandTimeout = 60;
                        adapter.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        // Determine if this class has any sections in the loaded data
                        HasSections = dt.AsEnumerable().Any(r => !string.IsNullOrWhiteSpace(r.Field<string>("SectionName")));

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
                        string searchMethod = isSearchingByID ? "ID Search" : "General Search";
                        string dynamicTitle = EscapeForJavaScript($"English Result Card - Total Students ( {studentCount} ) - {searchMethod}");

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
                        SafeRegisterStartupScript("resetTitle", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");

                        string noResultsMessage = isSearchingByID ?
                            "No results found for the specified Student IDs" :
                            "No results found for the selected criteria";

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
                SafeRegisterStartupScript("resetTitleError", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");

                // Use safe JavaScript registration for SQL errors
                string safeErrorMessage = EscapeForJavaScript(sqlEx.Message);
                SafeRegisterStartupScript("sqlerror", $"console.error('Database Error: {safeErrorMessage}'); alert('Database timeout. Please try with smaller selection or specific Student IDs.');");
            }
            catch (Exception ex)
            {
                ResultPanel.Visible = false;

                // Reset title on error
                SafeRegisterStartupScript("resetTitleError2", "document.getElementById('pageTitle').innerHTML = 'English Result Card';");

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

                            System.Diagnostics.Debug.WriteLine($"Publish Settings Loaded: HideSec={IS_Hide_Sec_Position}, HideClass={IS_Hide_Class_Position}, HideFM={IS_Hide_FullMark}, HidePM={IS_Hide_PassMark}, GradeBase={IS_Grade_BasePoint}");
                        }
                        else
                        {
                            // Default values if no settings found
                            IS_Hide_Sec_Position = false;
                            IS_Hide_Class_Position = false;
                            IS_Hide_FullMark = false;
                            IS_Hide_PassMark = false;
                            IS_Grade_BasePoint = false;
                            System.Diagnostics.Debug.WriteLine("No publish settings found - using defaults");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading publish settings: {ex.Message}");
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

        // OPTIMIZATION: Use connection pooling and reduce query complexity
        private AttendanceData GetAttendanceData(string studentResultID, int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                // OPTIMIZED: Single query to get both student info and attendance
                string combinedQuery = @"
                    SELECT 
                        ISNULL(att.WorkingDays, 0) as WorkingDays,
                        ISNULL(att.TotalPresent, 0) as TotalPresent,
                        ISNULL(att.TotalAbsent, 0) as TotalAbsent,
                        ISNULL(att.TotalLeave, 0) as TotalLeave,
                        ISNULL(att.TotalLate, 0) as TotalLate,
                        ISNULL(att.TotalLateAbs, 0) as TotalLateAbs
                    FROM Exam_Result_of_Student ers WITH (NOLOCK)
                    INNER JOIN StudentsClass sc WITH (NOLOCK) ON ers.StudentClassID = sc.StudentClassID
                    LEFT JOIN Attendance_Student att WITH (NOLOCK) ON 
                        sc.StudentID = att.StudentID
                        AND att.ExamID = @ExamID
                        AND att.ClassID = sc.ClassID
                        AND att.StudentClassID = sc.StudentClassID
                        AND att.SchoolID = @SchoolID
                        AND att.EducationYearID = @EducationYearID
                    WHERE ers.StudentResultID = @StudentResultID
                    AND ers.SchoolID = @SchoolID
                    AND ers.EducationYearID = @EducationYearID";

                using (SqlCommand cmd = new SqlCommand(combinedQuery, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AttendanceData
                            {
                                WorkingDays = reader["WorkingDays"]?.ToString() ?? "0",
                                PresentDays = reader["TotalPresent"]?.ToString() ?? "0",
                                AbsentDays = reader["TotalAbsent"]?.ToString() ?? "0",
                                LeaveDays = reader["TotalLeave"]?.ToString() ?? "0",
                                LateAbsDays = reader["TotalLateAbs"]?.ToString() ?? "0",
                                LateDays = reader["TotalLate"]?.ToString() ?? "0"
                            };
                        }
                    }
                }

                // If no specific attendance record found, return default values
                return new AttendanceData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAttendanceData: {ex.Message}");
                // Return default values if error occurs
                return new AttendanceData();
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

        // OPTIMIZATION: Reduce database calls in sub-exam data retrieval
        private string GetSubExamMarksForSpecificSubject(string studentResultID, int subjectID, List<int> availableSubExamIDs, string standardCellStyle)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                // OPTIMIZED: Get all sub-exam marks in one query instead of looping
                string subExamIds = string.Join(",", availableSubExamIDs);
                
                string query = @"
                    SELECT 
                        esn.SubExamID,
                        esn.SubExamName,
                        esn.Sub_ExamSN,
                        eom.MarksObtained as ObtainedMarks,
                        ISNULL(eom.FullMark, 0) as FullMark,
                        ISNULL(eom.PassMark, 0) as PassMark,
                        ISNULL(eom.AbsenceStatus, 'Present') as AbsenceStatus
                    FROM Exam_SubExam_Name esn WITH (NOLOCK)
                    LEFT JOIN Exam_Obtain_Marks eom WITH (NOLOCK) ON esn.SubExamID = eom.SubExamID 
                        AND eom.StudentResultID = @StudentResultID 
                        AND eom.SubjectID = @SubjectID
                        AND eom.SchoolID = @SchoolID
                        AND eom.EducationYearID = @EducationYearID
                    WHERE esn.SubExamID IN (" + subExamIds + @")
                    AND esn.SchoolID = @SchoolID
                    AND esn.EducationYearID = @EducationYearID
                    ORDER BY esn.Sub_ExamSN";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);

                    DataTable subExamData = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(subExamData);
                    }

                    // Build HTML from cached data
                    StringBuilder cellsHtml = new StringBuilder();

                    foreach (DataRow row in subExamData.Rows)
                    {
                        string fullMark = row["FullMark"]?.ToString() ?? "0";
                        string passMark = row["PassMark"]?.ToString() ?? "0";
                        var obtainedMarkValue = row["ObtainedMarks"];
                        string absenceStatus = row["AbsenceStatus"]?.ToString() ?? "Present";

                        // Check if this subject has data for this sub-exam
                        if (obtainedMarkValue == DBNull.Value || obtainedMarkValue == null)
                        {
                            // No data for this sub-exam, show dashes based on settings
                            if (!IS_Hide_FullMark)
                            {
                                cellsHtml.Append($@"<td style=""{standardCellStyle}"">-</td>");
                            }
                            if (!IS_Hide_PassMark)
                            {
                                cellsHtml.Append($@"<td style=""{standardCellStyle}"">-</td>");
                            }
                            // Always show OM
                            cellsHtml.Append($@"<td style=""{standardCellStyle}"">-</td>");
                        }
                        else
                        {
                            string obtainedMark = obtainedMarkValue.ToString();

                            // Determine absence ONLY if DB marks indicates absence and final grade is fail OR explicit absence status
                            bool isAbsent =
                                string.Equals(absenceStatus, "Absent", StringComparison.OrdinalIgnoreCase) ||
                                (string.Equals(obtainedMark, "A", StringComparison.OrdinalIgnoreCase));

                            // If fullMark or passMark is 0, show dash for those
                            fullMark = (fullMark == "0") ? "-" : fullMark;
                            passMark = (passMark == "0") ? "-" : passMark;

                            // Check if student failed in this sub-exam
                            bool isFailedInSubExam = false;
                            if (!isAbsent && passMark != "-" && obtainedMark != "-")
                            {
                                decimal om = 0, pm = 0;
                                if (decimal.TryParse(obtainedMark, out om) && decimal.TryParse(passMark, out pm))
                                {
                                    if (om < pm && pm > 0)
                                    {
                                        isFailedInSubExam = true;
                                    }
                                }
                            }

                            // Style for absent or failed OM cell (red background)
                            string omCellStyle = (isAbsent || isFailedInSubExam) ?
                                $"{standardCellStyle}; background-color: #ffcccc !important; color: #d32f2f; font-weight: bold;" :
                                standardCellStyle;

                            // Show actual marks in FM and PM, but Abs in OM if absent
                            string displayObtainedMark = isAbsent ? "Abs" : obtainedMark;

                            // Build cells based on settings
                            if (!IS_Hide_FullMark)
                            {
                                cellsHtml.Append($@"<td style=""{standardCellStyle}"" title=""Full Mark: {fullMark}"">{fullMark}</td>");
                            }
                            if (!IS_Hide_PassMark)
                            {
                                cellsHtml.Append($@"<td style=""{standardCellStyle}"" title=""Pass Mark: {passMark}"">{passMark}</td>");
                            }
                            // Always show OM
                            cellsHtml.Append($@"<td style=""{omCellStyle}"" title=""Obtained Mark: {displayObtainedMark}"">{displayObtainedMark}</td>");
                        }
                    }

                    return cellsHtml.ToString();
                }
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
                // Get values from the row with proper null checking
                obtainedMarks = row["ObtainedMark_ofStudent"] == DBNull.Value ? "0" :
                    Convert.ToDecimal(row["ObtainedMark_ofStudent"]).ToString("F1");

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
            var attendanceInfo = GetAttendanceData(studentResultID, examID);

            // Create proper marks display (obtained/total)
            string marksDisplay = $"{obtainedMarks}/{totalMarks}";

            // Build conditional PC header and data based on IS_Hide_Class_Position
            string pcHeader = !IS_Hide_Class_Position ?
                "<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #e19511; color: #fff; min-width: 25px;\" title=\"Position In Class\">PC</td>" :
                string.Empty;
            string pcData = !IS_Hide_Class_Position ?
                $"<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;\" title=\"{positionClass}\">{positionClass}</td>" :
                string.Empty;

            // Build conditional PS header and data based on HasSections AND IS_Hide_Sec_Position
            string psHeader = (HasSections && !IS_Hide_Sec_Position) ?
                "<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #339f03; color: #fff; min-width: 25px;\">PS</td>" :
                string.Empty;
            string psData = (HasSections && !IS_Hide_Sec_Position) ?
                $"<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;\" title=\"{positionSection}\">{positionSection}</td>" :
                string.Empty;
            string html = $@"<table class=""attendance-summary-combined"" style=""border-collapse: collapse; width: 100%; margin: 8px 0; font-size: 11px; font-family: Arial, sans-serif;"">
                    <tr>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ffd966; color: #000; min-width: 25px;"">WD</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ffd966; color: #000; min-width: 25px;"">Pre</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ffd966; color: #000; min-width: 25px;"">Abs</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ffd966; color: #000; min-width: 35px;"">L Abs</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ffd966; color: #000; min-width: 35px;"">Leave</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ffd966; color: #000; min-width: 35px;"">Late</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #4285f4; color: #fff; min-width: 75px;"">Obtained Marks</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ea4335; color: #fff; min-width: 30px;"">%</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #009974; color: #fff; min-width: 45px;"">Average</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #ed4695; color: #fff; min-width: 35px;"">Grade</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #805ddd; color: #fff; min-width: 30px;"">GPA</td>
                      
                        {pcHeader}
                        {psHeader}
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #a71a5f; color: #fff; min-width: 60px;"" title=""Comment based on Grade and GPA"">Comment</td>
                    </tr>
                    <tr>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;"">{attendanceInfo.WorkingDays}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;"">{attendanceInfo.PresentDays}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;"">{attendanceInfo.AbsentDays}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 35px;"">{attendanceInfo.LateAbsDays}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 35px;"">{attendanceInfo.LeaveDays}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 35px;"">{attendanceInfo.LateDays}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 75px;"" title=""{marksDisplay}"">{marksDisplay}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 30px;"">{percentage}%</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 45px;"">{average}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 35px;"">{grade}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 30px;"">{gpa}</td>
                       
                        {pcData}
                        {psData}
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 60px;"">{comment}</td>
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

            // Remove unnecessary columns for performance
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    // Keep only essential columns
                    if (!new[] { "StudentResultID", "ObtainedMark_ofStudent", "Student_Grade", "Student_Point", "Average", "ObtainedPercentage_ofStudent", "TotalMark_ofStudent", "Position_InExam_Class", "Position_InExam_Subsection", "StudentsName", "ID", "StudentImageID", "RollNo", "ClassName", "SectionName", "ShiftName", "GroupName", "ExamName", "SchoolID", "SchoolName", "Address", "Phone" }.Contains(col.ColumnName))
                    {
                        row[col] = DBNull.Value;
                    }
                }
            }

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

        // OPTIMIZATION: Cache grading data per exam/class combination
        public DataTable GetGradingSystemData()
        {
            try
            {
                int schoolID = Convert.ToInt32(Session["SchoolID"] ?? 1);
                int classID = Convert.ToInt32(ClassDropDownList.SelectedValue != "0" ? ClassDropDownList.SelectedValue : "1");
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue != "0" ? ExamDropDownList.SelectedValue : "1");
                int educationYearID = Convert.ToInt32(Session["Edu_Year"] ?? 1);

                // Create cache key
                string cacheKey = $"{schoolID}_{classID}_{examID}_{educationYearID}";

                // Check cache first
                if (gradingCache.ContainsKey(cacheKey))
                {
                    return gradingCache[cacheKey];
                }

                // Use the exact same TableAdapter that BanglaResult.aspx uses
                var tableAdapter = new EDUCATION.COM.Exam_ResultTableAdapters.Exam_Grading_SystemTableAdapter();

                // Call the same method that BanglaResult.aspx ObjectDataSource calls
                var gradingData = tableAdapter.GetData(schoolID, classID, examID, educationYearID);

                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradingTableAdapter",
                    $"console.log('TableAdapter returned {gradingData.Rows.Count} grading rows');", true);

                // If we got data from TableAdapter, cache and return it
                if (gradingData.Rows.Count > 0)
                {
                    // Cache the data
                    gradingCache[cacheKey] = gradingData;

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

                // Better JavaScript error handling with proper escaping
                string safeErrorMessage = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradingTableAdapterError",
                    $"console.error('TableAdapter error: {safeErrorMessage}');", true);
            }

            // Fallback to default grading data if TableAdapter fails
            return GetDefaultGradingData();
        }

        // Helper method to generate attendance + summary table for a student
    }
}
