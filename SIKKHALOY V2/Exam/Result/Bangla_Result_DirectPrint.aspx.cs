using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Collections.Generic;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Bangla_Result_DirectPrint_New : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }
            }
            catch { }
        }

        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            ExamDropDownList.DataBind();
            view();
            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            ResultPanel.Visible = false;
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
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
                    LoadResultsData();
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "alert", "alert('Please select both Class and Exam');", true);
                }
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(typeof(Page), "error", "console.error('LoadResults Error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private void LoadResultsData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
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
                            'Imperial Ideal School & College' as SchoolName,
                            '761,Tulatulisohera Rd,Kalulkotil, Narayangonj' as Address,
                            '01906-265260, 01789-752002' as Phone
                        FROM Exam_Result_of_Student ers
                        INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                        INNER JOIN Student s ON sc.StudentID = s.StudentID
                        INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
                        INNER JOIN Exam_Name en ON ers.ExamID = en.ExamID
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

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@GroupID", GroupDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);

                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        ResultRepeater.DataSource = dt;
                        ResultRepeater.DataBind();
                        ResultPanel.Visible = true;
                    }
                    else
                    {
                        ResultPanel.Visible = false;
                        Page.ClientScript.RegisterStartupScript(typeof(Page), "nodata", "alert('No results found for the selected criteria');", true);
                    }
                }
                catch (Exception ex)
                {
                    ResultPanel.Visible = false;
                    Page.ClientScript.RegisterStartupScript(typeof(Page), "dberror", "console.error('Database Error: " + ex.Message.Replace("'", "\\'") + "');", true);
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
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                        SELECT DISTINCT Grades, MaxPercentage, MinPercentage, Point, Comments
                        FROM Exam_Grading_System 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
                        AND (ClassID = @ClassID OR ClassID IS NULL)
                        AND (ExamID = @ExamID OR ExamID IS NULL)
                        ORDER BY MaxPercentage DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"] ?? 1);
                    cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue != "0" ? ClassDropDownList.SelectedValue : "1");
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue != "0" ? ExamDropDownList.SelectedValue : "1");

                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        dt = GetDefaultGradingData();
                    }
                    return dt;
                }
                catch
                {
                    return GetDefaultGradingData();
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

        public string GenerateSubjectMarksTable(string studentResultID, string studentGrade, decimal studentPoint)
        {
            try
            {
                DataTable subjects = GetSubjectResults(studentResultID);
                string resultComment = GetResultStatus(studentGrade, studentPoint);

                if (subjects.Rows.Count == 0)
                    return "<p>No subject data found</p>";

                // Check if sub-exams exist for this exam
                bool hasSubExams = CheckIfSubExamsExist(studentResultID);

                if (hasSubExams)
                {
                    return GenerateSubExamTable(studentResultID, resultComment, subjects.Rows.Count);
                }
                else
                {
                    return GenerateSimpleSubjectTable(subjects, resultComment);
                }
            }
            catch
            {
                return "<p>Error loading subject table</p>";
            }
        }

        private bool CheckIfSubExamsExist(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    // Try different possible table names for sub-exam data
                    string[] possibleQueries = new string[]
                    {
                        @"SELECT COUNT(*) FROM Exam_Result_of_SubExam WHERE StudentResultID = @StudentResultID",
                        @"SELECT COUNT(*) FROM ExamResult_SubExam WHERE StudentResultID = @StudentResultID",
                        @"SELECT COUNT(*) FROM Exam_SubResult WHERE StudentResultID = @StudentResultID",
                        @"SELECT COUNT(*) FROM SubExam_Result WHERE StudentResultID = @StudentResultID"
                    };

                    foreach (string query in possibleQueries)
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                            object result = cmd.ExecuteScalar();
                            if (result != null && Convert.ToInt32(result) > 0)
                            {
                                return true;
                            }
                        }
                        catch
                        {
                            // Continue to next query if this table doesn't exist
                            continue;
                        }
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private string GenerateSubExamTable(string studentResultID, string resultComment, int subjectCount)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    // Try to find the correct sub-exam table structure
                    DataTable subExams = new DataTable();
                    DataTable results = new DataTable();

                    // Method 1: Try Exam_Result_of_SubExam table
                    try
                    {
                        string subExamQuery = @"
                            SELECT DISTINCT SubExamName, SubExamID 
                            FROM Exam_Result_of_SubExam 
                            WHERE StudentResultID = @StudentResultID
                            ORDER BY SubExamID";

                        SqlCommand subExamCmd = new SqlCommand(subExamQuery, con);
                        subExamCmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                        SqlDataAdapter subExamAdapter = new SqlDataAdapter(subExamCmd);
                        subExamAdapter.Fill(subExams);

                        if (subExams.Rows.Count > 0)
                        {
                            // Get detailed results
                            string detailQuery = @"
                                SELECT 
                                    ISNULL(sub.SubjectName, '') as SubjectName,
                                    sub.SubjectID,
                                    ISNULL(irs.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
                                    ISNULL(irs.TotalMark_ofSubject, 0) as TotalMark_ofSubject,
                                    ISNULL(irs.SubjectGrades, '') as SubjectGrades,
                                    ISNULL(irs.SubjectPoint, 0) as SubjectPoint,
                                    ISNULL(irs.PassStatus_Subject, 'Pass') as PassStatus_Subject,
                                    ISNULL(sub_exam.SubExamName, '') as SubExamName,
                                    ISNULL(sub_exam.ObtainedMark, 0) as SubExamObtainedMark,
                                    ISNULL(sub_exam.TotalMark, 0) as SubExamTotalMark
                                FROM Exam_Result_of_Subject irs
                                INNER JOIN Subject sub ON irs.SubjectID = sub.SubjectID
                                LEFT JOIN Exam_Result_of_SubExam sub_exam ON irs.StudentResultID = sub_exam.StudentResultID AND irs.SubjectID = sub_exam.SubjectID
                                WHERE irs.StudentResultID = @StudentResultID
                                AND ISNULL(irs.IS_Add_InExam, 1) = 1
                                ORDER BY ISNULL(sub.SN, 999), sub.SubjectName";

                            SqlCommand detailCmd = new SqlCommand(detailQuery, con);
                            detailCmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                            SqlDataAdapter detailAdapter = new SqlDataAdapter(detailCmd);
                            detailAdapter.Fill(results);
                        }
                    }
                    catch
                    {
                        // If sub-exam table doesn't exist or has different structure, fallback to simple table
                        return GenerateSimpleSubjectTable(GetSubjectResults(studentResultID), resultComment);
                    }

                    if (subExams.Rows.Count == 0 || results.Rows.Count == 0)
                    {
                        // No sub-exam data found, use simple table
                        return GenerateSimpleSubjectTable(GetSubjectResults(studentResultID), resultComment);
                    }

                    string tableSizeClass = GetTableCssClass(subjectCount);

                    string html = @"
                        <div class=""marks-heading"">[SUB_EXAM_HEADING]</div>
                        <table class=""marks-table " + tableSizeClass + @""">
                            <tr>
                                <th rowspan=""2"">[SUBJECT_HEADER]</th>";

                    // Add sub-exam headers
                    foreach (DataRow subExamRow in subExams.Rows)
                    {
                        html += "<th colspan=\"2\">" + subExamRow["SubExamName"].ToString() + "</th>";
                    }

                    html += @"
                                <th rowspan=""2"">[TOTAL_MARK_HEADER]</th>
                                <th rowspan=""2"">[GRADE_HEADER]</th>
                                <th rowspan=""2"">[POINT_HEADER]</th>
                                <th rowspan=""" + (subjectCount + 2) + @""" class=""vertical-text"">" + resultComment + @"</th>
                            </tr>
                            <tr>";

                    // Add sub-exam mark headers
                    foreach (DataRow subExamRow in subExams.Rows)
                    {
                        html += "<th>[OBTAINED_HEADER]</th><th>[TOTAL_HEADER]</th>";
                    }

                    html += "</tr>";

                    // Group results by subject using Dictionary
                    Dictionary<string, DataRow> subjectDict = new Dictionary<string, DataRow>();
                    Dictionary<string, List<DataRow>> subExamDict = new Dictionary<string, List<DataRow>>();

                    foreach (DataRow row in results.Rows)
                    {
                        string subjectName = row["SubjectName"].ToString();

                        if (!subjectDict.ContainsKey(subjectName))
                        {
                            subjectDict[subjectName] = row;
                            subExamDict[subjectName] = new List<DataRow>();
                        }

                        if (!string.IsNullOrEmpty(row["SubExamName"].ToString()))
                        {
                            subExamDict[subjectName].Add(row);
                        }
                    }

                    foreach (var kvp in subjectDict)
                    {
                        string subjectName = kvp.Key;
                        DataRow subjectRow = kvp.Value;

                        string passStatus = subjectRow["PassStatus_Subject"].ToString();
                        if (string.IsNullOrEmpty(passStatus)) passStatus = "Pass";
                        string rowClass = passStatus == "Fail" ? "failed-row" : "";

                        html += "<tr class=\"" + rowClass + "\">";
                        html += "<td style=\"text-align: left; padding-left: 12px;\">" + subjectName + "</td>";

                        // Add sub-exam marks for this subject
                        foreach (DataRow subExamRow in subExams.Rows)
                        {
                            string subExamName = subExamRow["SubExamName"].ToString();
                            DataRow foundRow = null;

                            foreach (DataRow sr in subExamDict[subjectName])
                            {
                                if (sr["SubExamName"].ToString() == subExamName)
                                {
                                    foundRow = sr;
                                    break;
                                }
                            }

                            if (foundRow != null)
                            {
                                html += "<td>" + foundRow["SubExamObtainedMark"].ToString() + "</td>";
                                html += "<td>" + foundRow["SubExamTotalMark"].ToString() + "</td>";
                            }
                            else
                            {
                                html += "<td>-</td><td>-</td>";
                            }
                        }

                        html += "<td>" + subjectRow["ObtainedMark_ofSubject"].ToString() + "/" + subjectRow["TotalMark_ofSubject"].ToString() + "</td>";
                        html += "<td>" + subjectRow["SubjectGrades"].ToString() + "</td>";
                        html += "<td>" + Convert.ToDecimal(subjectRow["SubjectPoint"]).ToString("F1") + "</td>";
                        html += "</tr>";
                    }

                    html += "</table>";

                    // Replace placeholders with actual Bengali text
                    html = html.Replace("[SUB_EXAM_HEADING]", "???????????? ????? (Sub-Exam Breakdown)");
                    html = html.Replace("[SUBJECT_HEADER]", "?????????");
                    html = html.Replace("[TOTAL_MARK_HEADER]", "??? ???????");
                    html = html.Replace("[GRADE_HEADER]", "?????");
                    html = html.Replace("[POINT_HEADER]", "???????");
                    html = html.Replace("[OBTAINED_HEADER]", "???????");
                    html = html.Replace("[TOTAL_HEADER]", "?????");

                    return html;
                }
                catch (Exception ex)
                {
                    // If anything fails, fallback to simple table
                    return GenerateSimpleSubjectTable(GetSubjectResults(studentResultID), resultComment);
                }
            }
        }

        private string GenerateSimpleSubjectTable(DataTable subjects, string resultComment)
        {
            string tableSizeClass = GetTableCssClass(subjects.Rows.Count);

            string html = @"
                <div class=""marks-heading"">[SUBJECT_TABLE_HEADING]</div>
                <table class=""marks-table " + tableSizeClass + @""">
                    <tr>
                        <th>[SUBJECT_HEADER]</th>
                        <th>[OBTAINED_MARK_HEADER]</th>
                        <th>[TOTAL_MARK_HEADER]</th>
                        <th>[GRADE_HEADER]</th>
                        <th>[POINT_HEADER]</th>
                        <th rowspan=""" + (subjects.Rows.Count + 1) + @""" class=""vertical-text"">" + resultComment + @"</th>
                    </tr>";

            foreach (DataRow row in subjects.Rows)
            {
                string subjectName = GetSafeColumnValue(row, "SubjectName");
                string obtainedMark = GetSafeColumnValue(row, "ObtainedMark_ofSubject");
                string fullMark = GetSafeColumnValue(row, "TotalMark_ofSubject");
                string subjectGrades = GetSafeColumnValue(row, "SubjectGrades");
                decimal subjectPoint = GetSafeDecimalValue(row, "SubjectPoint");
                string passStatus = GetSafeColumnValue(row, "PassStatus_Subject");

                if (passStatus == "") passStatus = "Pass";
                string rowClass = passStatus == "Fail" ? "failed-row" : "";

                html += @"
                    <tr class=""" + rowClass + @""">
                        <td style=""text-align: left; padding-left: 12px;"">" + subjectName + @"</td>
                        <td>" + obtainedMark + @"</td>
                        <td>" + fullMark + @"</td>
                        <td>" + subjectGrades + @"</td>
                        <td>" + subjectPoint.ToString("F1") + @"</td>
                    </tr>";
            }

            html += "</table>";

            // Replace placeholders with actual Bengali text
            html = html.Replace("[SUBJECT_TABLE_HEADING]", "???????????? ?????");
            html = html.Replace("[SUBJECT_HEADER]", "?????????");
            html = html.Replace("[OBTAINED_MARK_HEADER]", "??????? ???????");
            html = html.Replace("[TOTAL_MARK_HEADER]", "????? ???????");
            html = html.Replace("[GRADE_HEADER]", "?????");
            html = html.Replace("[POINT_HEADER]", "???????");

            return html;
        }

        private DataTable GetSubjectResults(string studentResultID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                try
                {
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

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandTimeout = 15;
                    cmd.Parameters.AddWithValue("@StudentResultID", studentResultID);

                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    return dt;
                }
                catch
                {
                    return new DataTable();
                }
            }
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

        public string GetResultStatusBengali(string studentGrade, decimal gpa)
        {
            if (string.IsNullOrEmpty(studentGrade))
                return "???";

            switch (studentGrade.ToUpper())
            {
                case "A+": return "??????";
                case "A": return "??????? ???";
                case "A-": return "???";
                case "B": return "?????????";
                case "C": return "???";
                case "D": return "????? ????";
                case "F": return "?????????";
                default: return gpa >= 4.0m ? "??????" : "???";
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
            return row["PassStatus_ofStudent"].ToString() == "Pass" ? "????????" : "??????????";
        }

        // Helper method to ensure proper Bengali text encoding
        private string EncodeBengaliText(string text)
        {
            return System.Web.HttpUtility.HtmlEncode(text);
        }

        // Helper method to generate dynamic info table row
        public string GenerateDynamicInfoRow(object dataItem)
        {
            DataRowView row = (DataRowView)dataItem;
            string className = row["ClassName"]?.ToString() ?? "";
            string groupName = row["GroupName"]?.ToString() ?? "";
            string sectionName = row["SectionName"]?.ToString() ?? "";
            
            string html = "<tr><td>?????:</td><td>" + className + "</td>";
            
            bool hasGroup = !string.IsNullOrEmpty(groupName);
            bool hasSection = !string.IsNullOrEmpty(sectionName);
            
            if (hasGroup && hasSection)
            {
                // Both Group and Section exist: ????? | Value | ????? | Value | ???? | Value (6 columns total)
                html += "<td>?????:</td><td>" + groupName + "</td>";
                html += "<td>????:</td><td>" + sectionName + "</td>";
            }
            else if (hasGroup && !hasSection)
            {
                // Only Group exists: ????? | Value | ????? | Value | colspan="2" (6 columns total)
                html += "<td>?????:</td><td>" + groupName + "</td>";
                html += "<td colspan=\"2\"></td>";
            }
            else if (!hasGroup && hasSection)
            {
                // Only Section exists: ????? | Value | ???? | Value | colspan="2" (6 columns total)
                html += "<td>????:</td><td>" + sectionName + "</td>";
                html += "<td colspan=\"2\"></td>";
            }
            else
            {
                // Neither Group nor Section exists: ????? | Value | colspan="4" (6 columns total)
                html += "<td colspan=\"4\"></td>";
            }
            
            html += "</tr>";
            return html;
        }
    }
}