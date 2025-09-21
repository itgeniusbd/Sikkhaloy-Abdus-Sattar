using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
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
                        s.StudentID as ID,
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
                        ResultRepeater.DataSource = dt;
                        ResultRepeater.DataBind();
                        ResultPanel.Visible = true;
                    }
                    else
                    {
                        ResultPanel.Visible = false;
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
                Page.ClientScript.RegisterStartupScript(typeof(Page), "sqlerror",
                    "console.error('Database Error: " + sqlEx.Message.Replace("'", "\\'") + "');", true);
            }
            catch (Exception ex)
            {
                ResultPanel.Visible = false;
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

        public DataTable GetGradingSystemData()
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();

                string query = @"
                    SELECT DISTINCT Grades, MaxPercentage, MinPercentage, Point, Comments
                    FROM Exam_Grading_System 
                    WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
                    AND (ClassID = @ClassID OR ClassID IS NULL)
                    AND (ExamID = @ExamID OR ExamID IS NULL)
                    ORDER BY MaxPercentage DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue != "0" ? ClassDropDownList.SelectedValue : "1");
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue != "0" ? ExamDropDownList.SelectedValue : "1");

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.SelectCommand.CommandTimeout = 15;
                        adapter.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        dt = GetDefaultGradingData();
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
                return GetDefaultGradingData();
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

        private DataTable GetDefaultGradingData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Grades", typeof(string));
            dt.Columns.Add("MaxPercentage", typeof(decimal));
            dt.Columns.Add("MinPercentage", typeof(decimal));
            dt.Columns.Add("Point", typeof(decimal));
            dt.Columns.Add("Comments", typeof(string));

            dt.Rows.Add("A+", 100, 80, 5.00, "Outstanding");
            dt.Rows.Add("A", 79, 70, 4.00, "Excellent");
            dt.Rows.Add("A-", 69, 60, 3.50, "Very Good");
            dt.Rows.Add("B", 59, 50, 3.00, "Good");
            dt.Rows.Add("C", 49, 40, 2.00, "Satisfactory");
            dt.Rows.Add("D", 39, 33, 1.00, "Acceptable");
            dt.Rows.Add("F", 32, 0, 0.00, "Fail");

            return dt;
        }

        public string GetResultStatus(string studentGrade, decimal gpa)
        {
            if (string.IsNullOrEmpty(studentGrade))
                return "Good";

            switch (studentGrade.ToUpper())
            {
                case "A+": return "Excellent";
                case "A": return "Very Good";
                case "A-": return "Good";
                case "B": return "Satisfactory";
                case "C": return "Average";
                case "D": return "Below Average";
                case "F": return "Fail";
                default: return gpa >= 4.0m ? "Excellent" : "Good";
            }
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
                        ISNULL(ers.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
                        ISNULL(ers.TotalMark_ofSubject, 0) as TotalMark_ofSubject,
                        ISNULL(ers.SubjectGrades, '') as SubjectGrades,
                        ISNULL(ers.SubjectPoint, 0) as SubjectPoint,
                        ISNULL(ers.PassStatus_Subject, 'Pass') as PassStatus_Subject,
                        ISNULL(ers.IS_Add_InExam, 1) as IS_Add_InExam
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
                
                string query = @"
                    SELECT COUNT(*) 
                    FROM Exam_Obtain_Marks eom
                    INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                    WHERE eom.SchoolID = @SchoolID 
                    AND eom.EducationYearID = @EducationYearID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch
            {
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
                
                // Debug: First check if we have any Exam_Obtain_Marks records at all
                string debugQuery = @"SELECT COUNT(*) FROM Exam_Obtain_Marks WHERE StudentResultID = @StudentResultID";
                using (SqlCommand debugCmd = new SqlCommand(debugQuery, con))
                {
                    debugCmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    int totalRecords = Convert.ToInt32(debugCmd.ExecuteScalar());
                    System.Diagnostics.Debug.WriteLine($"Total Exam_Obtain_Marks records for StudentResultID {studentResultID}: {totalRecords}");
                }
                
                // Use the exact same query structure as ExamPosition_WithSub.aspx
                string query = @"
                    SELECT 
                        Exam_SubExam_Name.SubExamName, 
                        ISNULL(CAST(Exam_Obtain_Marks.MarksObtained AS char(10)), 'A') as ObtainedMark_ofSubExam 
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

                    System.Diagnostics.Debug.WriteLine($"Found {dt.Rows.Count} sub-exam records for Subject {subjectID}");
                    foreach (DataRow row in dt.Rows)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {row["SubExamName"]}: {row["ObtainedMark_ofSubExam"]}");
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

        // Method to get sub-exam header names - only if there are meaningful sub-exam marks
        private string GetSubExamHeaderNames(string studentResultID)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString);
                con.Open();
                
                // First check if we actually have meaningful sub-exam data
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM Exam_Obtain_Marks eom
                    INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                    WHERE eom.StudentResultID = @StudentResultID
                    AND eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID
                    AND ISNULL(CAST(eom.MarksObtained AS VARCHAR(10)), '') != ''
                    AND ISNULL(CAST(eom.MarksObtained AS VARCHAR(10)), '') != 'A'
                    AND ISNUMERIC(CAST(eom.MarksObtained AS VARCHAR(10))) = 1
                    AND CAST(eom.MarksObtained AS DECIMAL) > 0";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    checkCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    checkCmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                    int meaningfulSubExamCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    System.Diagnostics.Debug.WriteLine($"Meaningful sub-exam records found: {meaningfulSubExamCount}");
                    
                    if (meaningfulSubExamCount == 0)
                    {
                        return ""; // No meaningful sub-exam data
                    }
                }
                
                // If we have meaningful data, get the header names
                string query = @"
                    SELECT DISTINCT esn.SubExamName, esn.Sub_ExamSN
                    FROM Exam_Obtain_Marks eom
                    INNER JOIN Exam_SubExam_Name esn ON eom.SubExamID = esn.SubExamID
                    WHERE eom.StudentResultID = @StudentResultID
                    AND eom.SchoolID = @SchoolID
                    AND eom.EducationYearID = @EducationYearID
                    ORDER BY esn.Sub_ExamSN";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    string headerHtml = "";
                    foreach (DataRow row in dt.Rows)
                    {
                        headerHtml += $"<th>{row["SubExamName"]}</th>";
                    }

                    System.Diagnostics.Debug.WriteLine($"Generated header HTML: {headerHtml}");
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

        // Method to get sub-exam marks formatted for display in separate cells
        private (string SubExamMarksCells, string TotalMarks) GetSubExamMarksForDisplay(string studentResultID, int subjectID, string originalObtainedMark)
        {
            try
            {
                DataTable subExams = GetSubExamMarks(studentResultID, subjectID, 0); // examID not needed
                
                System.Diagnostics.Debug.WriteLine($"Subject {subjectID}: Found {subExams.Rows.Count} sub-exam rows");
                
                if (subExams.Rows.Count > 0)
                {
                    string cellsHtml = "";
                    decimal totalMarks = 0;
                    bool hasValidSubExamMarks = false;
                    int validMarkCount = 0;
                    
                    foreach (DataRow row in subExams.Rows)
                    {
                        string markValue = row["ObtainedMark_ofSubExam"].ToString().Trim();
                        System.Diagnostics.Debug.WriteLine($"  Sub-exam mark: '{markValue}'");
                        
                        cellsHtml += $"<td>{markValue}</td>";
                        
                        // Calculate total if it's numeric and not 'A' (absent) and not empty
                        if (!string.IsNullOrEmpty(markValue) && 
                            markValue != "A" && 
                            decimal.TryParse(markValue, out decimal mark) && 
                            mark > 0)
                        {
                            totalMarks += mark;
                            hasValidSubExamMarks = true;
                            validMarkCount++;
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"  Valid mark count: {validMarkCount}, Total: {totalMarks}");
                    
                    // If we have at least one valid sub-exam mark, show sub-exam breakdown
                    if (hasValidSubExamMarks)
                    {
                        string totalMarksCellHtml = $"<td class=\"total-marks-cell\">{totalMarks}</td>";
                        return (cellsHtml, totalMarksCellHtml);
                    }
                    else
                    {
                        // Sub-exams exist but no meaningful marks - treat as non-sub-exam subject
                        return ("<td>-</td><td>-</td>", $"<td class=\"total-marks-cell\">{originalObtainedMark}</td>");
                    }
                }
                else
                {
                    // No sub-exams at all - show empty cells and original mark
                    System.Diagnostics.Debug.WriteLine($"  No sub-exams found, using original mark: {originalObtainedMark}");
                    return ("<td>-</td><td>-</td>", $"<td class=\"total-marks-cell\">{originalObtainedMark}</td>");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetSubExamMarksForDisplay: {ex.Message}");
                return ("<td>-</td><td>-</td>", $"<td class=\"total-marks-cell\">{originalObtainedMark}</td>");
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

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Generating table for StudentResultID: {studentResultID}");

                // Check if we have sub-exams to determine table structure
                int examID = Convert.ToInt32(ExamDropDownList.SelectedValue);
                bool hasSubExams = HasSubExams(examID);
                string subExamHeader = "";

                if (hasSubExams)
                {
                    // Get sub-exam names for header
                    subExamHeader = GetSubExamHeaderNames(studentResultID);
                }

                string html = @"
                    
                    <table class=""marks-table " + tableSizeClass + @""">";

                // Create header row
                if (hasSubExams && !string.IsNullOrEmpty(subExamHeader))
                {
                    html += @"
                        <tr>
                            <th rowspan=""2"">বিষয়</th>
                            <th colspan=""2"">প্রাপ্ত নম্বর</th>
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

                    // Debug output
                    System.Diagnostics.Debug.WriteLine($"Processing subject: {subjectName} (ID: {subjectID})");

                    if (passStatus == "") passStatus = "Pass";
                    string rowClass = passStatus == "Fail" ? "failed-row" : "";

                    if (hasSubExams)
                    {
                        // Get sub-exam marks for this subject (pass original obtained mark)
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
                        html += @"
                            <tr class=""" + rowClass + @""">
                                <td style=""text-align: left; padding-left: 12px;"">" + subjectName + @"</td>
                                <td>" + obtainedMark + @"</td>
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
                System.Diagnostics.Debug.WriteLine($"Error in GenerateSubjectMarksTable: {ex.Message}");
                return "<p>Error loading subject table: " + ex.Message + "</p>";
            }
        }
    }
}