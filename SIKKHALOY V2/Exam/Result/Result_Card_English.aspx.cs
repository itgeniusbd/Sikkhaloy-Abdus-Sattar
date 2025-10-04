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

        // Whether the current class/result set has sections
        private bool HasSections
        {
            get { return ViewState["HasSections"] != null && (bool)ViewState["HasSections"]; }
            set { ViewState["HasSections"] = value; }
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
                    // Original query for normal search - Fix column name inconsistencies
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
                SafeRegisterStartupScript("sqlerror", $"console.error('Database Error: {safeErrorMessage}');");
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
                obtainedMarks = row["TotalExamObtainedMark_ofStudent"]?.ToString() ?? "0";
                totalMarks = row["TotalMark_ofStudent"]?.ToString() ?? "0";
                percentage = row["ObtainedPercentage_ofStudent"] == DBNull.Value ? "0.00" : string.Format("{0:F2}", row["ObtainedPercentage_ofStudent"]);
                average = row["Average"] == DBNull.Value ? "0.00" : string.Format("{0:F2}", row["Average"]);
                grade = row["Student_Grade"] == DBNull.Value ? "F" : row["Student_Grade"].ToString();
                gpa = row["Student_Point"] == DBNull.Value ? "0.0" : string.Format("{0:F1}", row["Student_Point"]);

                int posClassInt = row["Position_InExam_Class"] == DBNull.Value ? 0 : Convert.ToInt32(row["Position_InExam_Class"]);
                int posSectionInt = row["Position_InExam_Subsection"] == DBNull.Value ? 0 : Convert.ToInt32(row["Position_InExam_Subsection"]);
                positionClass = posClassInt > 0 ? ToOrdinal(posClassInt) : "-";
                positionSection = posSectionInt > 0 ? ToOrdinal(posSectionInt) : "-";

                decimal studentPoint = row["Student_Point"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Student_Point"]);
                comment = GetResultStatus(grade, studentPoint);
            }
            catch { }

            string studentResultID = row["StudentResultID"]?.ToString() ?? string.Empty;
            int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
            var attendanceInfo = GetAttendanceData(studentResultID, examID);

            string psHeader = HasSections ? "<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #339f03; color: #fff; min-width: 25px;\">PS</td>" : string.Empty;
            string psData = HasSections ? $"<td style=\"border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;\" title=\"{positionSection}\">{positionSection}</td>" : string.Empty;

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
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #e19511; color: #fff; min-width: 25px;"" title=""Position In Class"">PC</td>
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
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 75px;"" title=""{obtainedMarks}/{totalMarks}"">{obtainedMarks}/{totalMarks}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 30px;"">{percentage}%</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 45px;"">{average}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 35px;"">{grade}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 30px;"">{gpa}</td>
                        <td style=""border: 1px solid #000; padding: 4px 6px; text-align: center; font-weight: bold; background-color: #fff; color: #000; min-width: 25px;"" title=""{positionClass}"">{positionClass}</td>
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

                // Better JavaScript error handling with proper escaping
                string safeErrorMessage = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradingTableAdapterError",
                    $"console.error('TableAdapter error: {safeErrorMessage}');", true);
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
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "gradeFromChart",
                            $"console.log('Found comment from TableAdapter: Grade: {gradeFromChart}, Comment: {commentFromChart}');", true);

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

                // Better JavaScript error handling with proper escaping
                string safeErrorMessage = ex.Message.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
                Page.ClientScript.RegisterStartupScript(typeof(Page), "gradeChartError",
                    $"console.error('GetCommentFromGradingChart error: {safeErrorMessage}');", true);
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
        // Renders Class (always) and optional Section/Group/Shift in compact rows (max 3 pairs per row)
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

                // If nothing to show, return empty string
                if (pairs.Count == 0)
                    return string.Empty;

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
            catch
            {
                return string.Empty;
            }
        }

        // Generate the subject marks table HTML (used by ASPX markup)
        public string GenerateSubjectMarksTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            try
            {
                DataTable subjects = GetSubjectResults(studentResultID);
                string resultComment = GetResultStatus(studentGrade, studentPoint);

                if (subjects.Rows.Count == 0)
                    return "<p>No subject data found</p>";

                string tableSizeClass = GetTableCssClass(subjects.Rows.Count);

                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                bool hasSubExams = HasSubExams(examID);
                string subExamHeader = "";
                string subExamSecondHeader = "";
                int subExamCount = 0;

                // Consistent font sizing
                string fontSize = "11px";
                string cellPadding = "3px";

                string tableContainerStyle = $"font-size: {fontSize} !important; font-family: Arial, sans-serif !important; border-collapse: collapse; width: 100%; table-layout: auto; overflow-x: auto;";
                string standardCellStyle = $"font-size: {fontSize} !important; font-family: Arial, sans-serif !important; border: 1px solid #000; padding: {cellPadding}; text-align: center; white-space: nowrap; min-width: 30px; max-width: 60px; overflow: hidden; text-overflow: ellipsis;";

                if (hasSubExams)
                {
                    subExamCount = GetSubExamCount(examID);
                    if (subExamCount > 0)
                    {
                        var subExamHeaders = GetSubExamHeadersWithStructure(studentResultID);
                        subExamHeader = subExamHeaders.FirstRowHeader;
                        subExamSecondHeader = subExamHeaders.SecondRowHeader;
                    }
                }

                // Calculate total columns for responsive layout hints
                int totalColumns = 1; // SUBJECTS
                if (hasSubExams && subExamCount > 0)
                {
                    totalColumns += (subExamCount * 3); // FM, PM, OM for each
                }
                else
                {
                    totalColumns += 3; // FM, PM, OM
                }
                int positionColumns = HasSections ? 4 : 2; // PC,(PS),HMC,(HMS)
                totalColumns += (3 + positionColumns); // MARKS, GRADE, GPA + positions
                totalColumns += 5; // buffer

                string html = $@"<div style=""overflow-x: auto; width: 100;""><table class=""marks-table {tableSizeClass} sub-exam-{subExamCount}"" style=""{tableContainerStyle}"" data-total-columns=""{totalColumns}"">";

                if (hasSubExams && subExamCount > 0 && !string.IsNullOrEmpty(subExamHeader))
                {
                    html += $@"<tr style=""background-color: #ffb3ba;""> <th rowspan=""2"" style=""{standardCellStyle}; text-align: left; min-width: 80px; max-width: 120px; font-weight: bold;"">SUBJECTS</th> {subExamHeader}<th rowspan=""2"" style=""{standardCellStyle}; min-width: 60px; font-weight: bold;"">MARKS</th><th rowspan=""2"" style=""{standardCellStyle}; min-width: 40px; font-weight: bold;"">GRADE</th><th rowspan=""2"" style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">GPA</th><th rowspan=""2"" class=""pc-column"" style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PC</th>";
                    if (HasSections)
                    {
                        html += $@"<th rowspan=""2"" class=""ps-column"" style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PS</th>";
                    }
                    html += $@"<th rowspan=""2"" class=""hmc-column"" style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMC</th>";
                    if (HasSections)
                    {
                        html += $@"<th rowspan=""2"" class=""hms-column"" style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMS</th>";
                    }
                    html += "</tr>";
                    html += $@"<tr style=""background-color: #ffb3ba;"">{subExamSecondHeader}</tr>";
                }
                else
                {
                    html += $@"<tr style=""background-color: #ffb3ba;""> <th style=""{standardCellStyle}; text-align: left; min-width: 80px; max-width: 120px; font-weight: bold;"">SUBJECTS</th><th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">FM</th><th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">PM</th><th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">OM</th><th style=""{standardCellStyle}; min-width: 60px; font-weight: bold;"">MARKS</th><th style=""{standardCellStyle}; min-width: 40px; font-weight: bold;"">GRADE</th><th style=""{standardCellStyle}; min-width: 35px; font-weight: bold;"">GPA</th><th class=""pc-column"" style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PC</th>";
                    if (HasSections)
                    {
                        html += $@"<th class=""ps-column"" style=""{standardCellStyle}; min-width: 35px; background-color: #e8f4fd; font-weight: bold;"">PS</th>";
                    }
                    html += $@"<th class=""hmc-column"" style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMC</th>";
                    if (HasSections)
                    {
                        html += $@"<th class=""hms-column"" style=""{standardCellStyle}; min-width: 40px; background-color: #e8f4fd; font-weight: bold;"">HMS</th>";
                    }
                    html += "</tr>";
                }

                // Build body rows
                foreach (DataRow srow in subjects.Rows)
                {
                    string subjectName = GetSafeColumnValue(srow, "SubjectName");
                    string obtainedMark = GetSafeColumnValue(srow, "ObtainedMark_ofSubject");
                    string fullMark = GetSafeColumnValue(srow, "TotalMark_ofSubject");
                    string subjectGrades = GetSafeColumnValue(srow, "SubjectGrades");
                    decimal subjectPoint = GetSafeDecimalValue(srow, "SubjectPoint");
                    string passStatus = GetSafeColumnValue(srow, "PassStatus_Subject");
                    int subjectID = 0; int.TryParse(GetSafeColumnValue(srow, "SubjectID"), out subjectID);

                    var positionData = GetSubjectPositionDataForTable(studentResultID, subjectID);

                    if (string.IsNullOrWhiteSpace(passStatus)) passStatus = "Pass";
                    string rowClass = string.Equals(passStatus, "Fail", StringComparison.OrdinalIgnoreCase) ? "failed-row" : "";

                    bool isSubjectAbsent = (
                        string.Equals(obtainedMark, "A", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(subjectGrades, "F", StringComparison.OrdinalIgnoreCase)
                    ) || (obtainedMark == "0" && string.Equals(subjectGrades, "F", StringComparison.OrdinalIgnoreCase) && subjectPoint == 0.0m);

                    string displayMark = isSubjectAbsent ? "Abs" : obtainedMark;
                    string marksDisplay = $"{displayMark}/{fullMark}";
                    string marksColumnStyle = isSubjectAbsent ?
                        $"{standardCellStyle}; background-color: #ffcccc; color: #d32f2f; font-weight: bold;" :
                        standardCellStyle;

                    string subjectCellStyle = $"font-size: {fontSize} !important; font-family: Arial, sans-serif !important; border: 1px solid #000; padding: {cellPadding}; text-align: left; padding-left: 4px; min-width: 80px; max-width: 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

                    if (hasSubExams && subExamCount > 0)
                    {
                        // Ensure we have ordered SubExamIDs in ViewState (populated by header generator)
                        // If not populated yet (edge case), populate now
                        if (ViewState["OrderedSubExamIDs"] == null)
                        {
                            var _ = GetSubExamHeadersWithStructure(studentResultID);
                        }

                        // Build FM/PM/OM cells per sub-exam in header order
                        string subExamData = GetSubExamCellsHtml(studentResultID, subjectID, examID, standardCellStyle);

                        html += $@"<tr class=""{rowClass}"">
    <td style=""{subjectCellStyle}"" title=""{subjectName}"">{subjectName}</td>
    {subExamData}
    <td style=""{marksColumnStyle}"">{marksDisplay}</td>
    <td class=""grade-cell"" style=""{standardCellStyle}"">{subjectGrades}</td>
    <td style=""{standardCellStyle}"">{subjectPoint.ToString("F1")}</td>
    <td class=""pc-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionClass}"">{positionData.PositionClass}</td>";

                        if (HasSections)
                        {
                            html += $@"<td class=""ps-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionSection}"">{positionData.PositionSection}</td>";
                        }

                        html += $@"<td class=""hmc-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksClass}"">{positionData.HighestMarksClass}</td>";

                        if (HasSections)
                        {
                            html += $@"<td class=""hms-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksSection}"">{positionData.HighestMarksSection}</td>";
                        }

                        html += "</tr>";
                    }
                    else
                    {
                        // No sub-exams - use the new method for correct pass marks
                        var passMarkData = GetMainExamPassMark(subjectID, examID);
                        string omDisplayMark = isSubjectAbsent ? "Abs" : obtainedMark;
                        string omCellStyle = isSubjectAbsent ?
                            $"{standardCellStyle}; background-color: #ffcccc; color: #d32f2f; font-weight: bold;" :
                            standardCellStyle;

                        html += $@"<tr class=""{rowClass}"">
    <td style=""{subjectCellStyle}"" title=""{subjectName}"">{subjectName}</td>
    <td style=""{standardCellStyle}"">{fullMark}</td>
    <td style=""{standardCellStyle}"">{passMarkData}</td>
    <td style=""{omCellStyle}"">{omDisplayMark}</td>
    <td style=""{marksColumnStyle}"">{marksDisplay}</td>
    <td class=""grade-cell"" style=""{standardCellStyle}"">{subjectGrades}</td>
    <td style=""{standardCellStyle}"">{subjectPoint.ToString("F1")}</td>
    <td class=""pc-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionClass}"">{positionData.PositionClass}</td>";

                        if (HasSections)
                        {
                            html += $@"<td class=""ps-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.PositionSection}"">{positionData.PositionSection}</td>";
                        }

                        html += $@"<td class=""hmc-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksClass}"">{positionData.HighestMarksClass}</td>";

                        if (HasSections)
                        {
                            html += $@"<td class=""hms-column"" style=""{standardCellStyle}; background-color: #e8f4fd;"" title=""{positionData.HighestMarksSection}"">{positionData.HighestMarksSection}</td>";
                        }

                        html += "</tr>";
                    }
                }

                html += "</table></div>";
                return html;
            }
            catch (Exception ex)
            {
                return "<p>Error loading subject table: " + ex.Message + "</p>";
            }
        }

        // Return sub-exam cells for a specific subject in header order
        private string GetSubExamCellsHtml(string studentResultID, int subjectID, int examID, string standardCellStyle)
        {
            // Build list of available SubExamIDs for this exam/class
            List<int> subExamIDs = GetAvailableSubExamIDs(examID);

            // Ensure ordered header list available
            var ordered = ViewState["OrderedSubExamIDs"] as List<int>;
            var loopIds = (ordered != null && ordered.Count > 0)
                ? ordered.Where(id => subExamIDs.Contains(id)).ToList()
                : subExamIDs;

            // If none available, still generate the correct number of dash cells
            if (loopIds.Count == 0)
            {
                int count = GetSubExamCount(examID);
                return GenerateDashCellsForSubExams(count, standardCellStyle);
            }

            return GetSubExamMarksForSpecificSubject(studentResultID, subjectID, loopIds, standardCellStyle);
        }

        private string GenerateDashCellsForSubExams(int subExamCount, string standardCellStyle)
        {
            string dashCells = "";
            for (int i = 0; i < subExamCount; i++)
            {
                dashCells += $@"<td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td>";
            }
            return dashCells;
        }

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
                    AND eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID
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

        // Subject position data for subject table
        public class SubjectPositionInfo
        {
            public string PositionClass { get; set; } = "-";
            public string PositionSection { get; set; } = "-";
            public string HighestMarksClass { get; set; } = "-";
            public string HighestMarksSection { get; set; } = "-";
        }
        private SubjectPositionInfo GetSubjectPositionDataForTable(string studentResultID, int subjectID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

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
                    AND ers.SubjectID = @SubjectID
                    AND ers.SchoolID = @SchoolID
                    AND ers.EducationYearID = @EducationYearID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SubjectPositionInfo
                            {
                                PositionClass = reader["Position_InSubject_Class"]?.ToString() ?? "-",
                                PositionSection = reader["Position_InSubject_Subsection"]?.ToString() ?? "-",
                                HighestMarksClass = reader["HighestMark_InSubject_Class"]?.ToString() ?? "-",
                                HighestMarksSection = reader["HighestMark_InSubject_Subsection"]?.ToString() ?? "-"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubjectPositionDataForTable: {ex.Message}");
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return new SubjectPositionInfo();
        }

        [System.Web.Services.WebMethod]
        public static object SaveSignature(string signatureType, string imageData)
        {
            try
            {
                var context = HttpContext.Current;
                var schoolId = context.Session["SchoolID"];

                if ( schoolId == null)
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
                    if (con != null && con.State == System.Data.ConnectionState.Open)
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

        // Attendance DTO and minimal data provider
        public class AttendanceData
        {
            public string WorkingDays { get; set; } = "0";
            public string PresentDays { get; set; } = "0";
            public string AbsentDays { get; set; } = "0";
            public string LeaveDays { get; set; } = "0";
            public string LateAbsDays { get; set; } = "0";
            public string LateDays { get; set; } = "0";
        }
        
        private AttendanceData GetAttendanceData(string studentResultID, int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                // First, get the StudentClassID from the result
                string getStudentClassQuery = @"
                    SELECT sc.StudentID, ers.StudentClassID, sc.ClassID
                    FROM Exam_Result_of_Student ers
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    WHERE ers.StudentResultID = @StudentResultID";

                int studentID = 0;
                int studentClassID = 0;
                int classID = 0;

                using (SqlCommand cmd = new SqlCommand(getStudentClassQuery, con))
                {
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            studentID = Convert.ToInt32(reader["StudentID"]);
                            studentClassID = Convert.ToInt32(reader["StudentClassID"]);
                            classID = Convert.ToInt32(reader["ClassID"]);
                        }
                        else
                        {
                            return new AttendanceData(); // Return default if no student found
                        }
                    }
                }

                // Now get attendance data from Attendance_Student table
                string attendanceQuery = @"
                    SELECT 
                        ISNULL(WorkingDays, 0) as WorkingDays,
                        ISNULL(TotalPresent, 0) as TotalPresent,
                        ISNULL(TotalAbsent, 0) as TotalAbsent,
                        ISNULL(TotalLeave, 0) as TotalLeave,
                        ISNULL(TotalLate, 0) as TotalLate,
                        ISNULL(TotalLateAbs, 0) as TotalLateAbs
                    FROM Attendance_Student 
                    WHERE StudentID = @StudentID
                    AND ExamID = @ExamID
                    AND ClassID = @ClassID
                    AND StudentClassID = @StudentClassID
                    AND SchoolID = @SchoolID
                    AND EducationYearID = @EducationYearID";

                using (SqlCommand cmd = new SqlCommand(attendanceQuery, con))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Parameters.AddWithValue("@ExamID", examID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    cmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
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

        // Ordinal helpers - ensuring proper method signatures and no hidden characters
        private static string ToOrdinal(int number)
        {
            if (number <= 0) return "-";
            int lastTwo = number % 100;
            if (lastTwo >= 11 && lastTwo <= 13) return number + "th";
            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }

        // Helper method to parse Student IDs from comma-separated input
        private List<string> ParseStudentIDs(string input)
        {
            var studentIDs = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return studentIDs;

            // Split by comma and parse each ID
            string[] idStrings = input.Split(new char[] { ',', '、' }, StringSplitOptions.RemoveEmptyEntries);

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

        // Check if exam has sub-exams
        private bool HasSubExams(int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT COUNT(DISTINCT eom.SubExamID) 
                    FROM Exam_SubExam_Name esn
                    INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                    INNER JOIN Exam_Result_of_Student ers ON eom.StudentResultID = ers.StudentResultID
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    WHERE esn.SchoolID = @SchoolID 
                    AND eom.SchoolID = @SchoolID 
                    AND eom.EducationYearID = @EducationYearID
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

        // Count sub-exams
        private int GetSubExamCount(int examID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT COUNT(DISTINCT esn.SubExamID) 
                    FROM Exam_SubExam_Name esn
                    INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                    INNER JOIN Exam_Result_of_Student ers ON eom.StudentResultID = ers.StudentResultID
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    WHERE esn.SchoolID = @SchoolID
                    AND eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID
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

                    return Convert.ToInt32(cmd.ExecuteScalar());
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

        // Header structure for sub-exam table
        public class SubExamHeaderStructure
        {
            public string FirstRowHeader { get; set; } = string.Empty;
            public string SecondRowHeader { get; set; } = string.Empty;
        }
        
        private SubExamHeaderStructure GetSubExamHeadersWithStructure(string studentResultID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                int subExamCount = GetSubExamCount(examID);

                string fontSize = subExamCount >= 4 ? "9px" : (subExamCount >= 3 ? "10px" : "11px");
                string cellPadding = subExamCount >= 4 ? "2px" : "3px";
                string standardCellStyle = $"font-size: {fontSize}; font-family: Arial, sans-serif; border: 1px solid #000; padding: {cellPadding}; text-align: center; white-space: nowrap; min-width: 25px; max-width: 35px; overflow: hidden; text-overflow: ellipsis;";

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
                    foreach (DataRow row in dt.Rows)
                    {
                        string subExamName = row["SubExamName"].ToString();
                        if (subExamName.Length > 8 && subExamCount >= 3)
                        {
                            subExamName = subExamName.Substring(0, 6) + "..";
                        }
                        result.FirstRowHeader += $@"<th colspan=""3"" style=""{standardCellStyle}; min-width: 75px; max-width: 100px;"" title=""{row["SubExamName"]}"">{subExamName}</th>";
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        result.SecondRowHeader += $@"<th style=""{standardCellStyle}"">FM</th><th style=""{standardCellStyle}"">PM</th><th style=""{standardCellStyle}"">OM</th>";
                    }

                    ViewState["OrderedSubExamIDs"] = dt.AsEnumerable().Select(r => Convert.ToInt32(r["SubExamID"])).ToList();
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

        private string GetSubExamMarksForSpecificSubject(string studentResultID, int subjectID, List<int> availableSubExamIDs, string standardCellStyle)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string cellsHtml = "";

                // Use ordered sub-exam IDs if present to keep column alignment consistent with header
                var ordered = ViewState["OrderedSubExamIDs"] as List<int>;
                var loopIds = (ordered != null && ordered.Count > 0)
                    ? ordered.Where(id => availableSubExamIDs.Contains(id)).ToList()
                    : availableSubExamIDs;

                // Get sub-exam data for each available sub-exam in order
                foreach (int subExamID in loopIds)
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

                                // Check if this subject has data for this sub-exam
                                if (obtainedMarkValue == DBNull.Value || obtainedMarkValue == null)
                                {
                                    // No data for this sub-exam, show dashes
                                    cellsHtml += $@"<td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td><td style=""{standardCellStyle}"">-</td>";
                                }
                                else
                                {
                                    string obtainedMark = obtainedMarkValue.ToString();

                                    // Determine absence ONLY if DB marks indicates absence and final grade is fail OR explicit absence status
                                    bool isAbsent =
                                        string.Equals(absenceStatus, "Absent", StringComparison.OrdinalIgnoreCase) ||
                                        (string.Equals(obtainedMark, "A", StringComparison.OrdinalIgnoreCase));

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
    }
}