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

namespace EDUCATION.COM.Student.Exam
{
    public partial class Cumulative_Exam : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

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

        private bool HasSections
        {
            get { return ViewState["HasSections"] != null && (bool)ViewState["HasSections"]; }
            set { ViewState["HasSections"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadSignatures();
        }

        protected void Cum_ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cum_ExamDropDownList.SelectedValue == "0")
            {
                ResultPanel.Visible = false;
                return;
            }
            LoadStudentResult();
        }

        private void LoadStudentResult()
        {
            try
            {
                if (Session["StudentClassID"] == null || Session["SchoolID"] == null || Session["Edu_Year"] == null)
                    return;

                int studentClassID = Convert.ToInt32(Session["StudentClassID"]);
                int cumulativeNameID = Convert.ToInt32(Cum_ExamDropDownList.SelectedValue);
                int classID = Convert.ToInt32(Session["ClassID"]);

                LoadPublishSettings(classID, cumulativeNameID);
                DetermineIfHasSections(classID, cumulativeNameID);

                var resultData = GetStudentResultData(studentClassID, cumulativeNameID);
                if (resultData == null || resultData.Rows.Count == 0)
                {
                    ResultPanel.Visible = false;
                    return;
                }

                ResultRepeater.DataSource = resultData;
                ResultRepeater.DataBind();
                ResultPanel.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading cumulative result: " + ex.Message);
                ResultPanel.Visible = false;
            }
        }

        private void LoadPublishSettings(int classID, int cumulativeNameID)
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT TOP 1
                            ISNULL(IS_Hide_Sec_Position, 0) AS IS_Hide_Sec_Position,
                            ISNULL(IS_Hide_Class_Position, 0) AS IS_Hide_Class_Position,
                            ISNULL(IS_Grade_BasePoint, 0) AS IS_Grade_BasePoint
                        FROM Exam_Cumulative_Setting
                        WHERE SchoolID = @SchoolID
                        AND EducationYearID = @EducationYearID
                        AND ClassID = @ClassID
                        AND CumulativeNameID = @CumulativeNameID";

                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                        cmd.Parameters.AddWithValue("@ClassID", classID);
                        cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                IS_Hide_Sec_Position = Convert.ToBoolean(reader["IS_Hide_Sec_Position"]);
                                IS_Hide_Class_Position = Convert.ToBoolean(reader["IS_Hide_Class_Position"]);
                            }
                            else
                            {
                                IS_Hide_Sec_Position = false;
                                IS_Hide_Class_Position = false;
                            }
                        }
                    }
                }
                IS_Hide_FullMark = false;
                IS_Hide_PassMark = false;
            }
            catch
            {
                IS_Hide_Sec_Position = false;
                IS_Hide_Class_Position = false;
                IS_Hide_FullMark = false;
                IS_Hide_PassMark = false;
            }
        }

        private void DetermineIfHasSections(int classID, int cumulativeNameID)
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
                        command.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        command.Parameters.AddWithValue("@ClassID", classID);
                        HasSections = Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                HasSections = false;
            }
        }

        private DataTable GetStudentResultData(int studentClassID, int cumulativeNameID)
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT 
                            SchoolInfo.SchoolName, 
                            Student.ID, 
                            Student.StudentsName, 
                            CreateClass.Class, 
                            StudentsClass.RollNo, 
                            Exam_Cumulative_Student.TotalMark_ofStudent AS Cu_Stu_TM,
                            Exam_Cumulative_Student.ObtainedMark_ofStudent AS Cu_Stu_OM,
                            Exam_Cumulative_Student.Student_Grade,
                            Exam_Cumulative_Student.Student_Point,
                            Exam_Cumulative_Student.HighestMark_InExam_Class,
                            Exam_Cumulative_Student.HighestMark_InExam_Subsection,
                            Exam_Cumulative_Student.Position_InExam_Class,
                            Exam_Cumulative_Student.Position_InExam_Subsection,
                            Exam_Cumulative_Student.Student_Comments,
                            Exam_Cumulative_Name.CumulativeResultName AS ExamName,
                            SchoolInfo.Address,
                            SchoolInfo.Phone,
                            CreateSection.Section,
                            CreateSubjectGroup.SubjectGroup,
                            CreateShift.Shift,
                            Student.StudentImageID,
                            Exam_Cumulative_Student.Average,
                            Exam_Cumulative_Setting.IS_Hide_SubExam,
                            Exam_Cumulative_Setting.IS_Hide_Sec_Position,
                            Exam_Cumulative_Setting.IS_Hide_Class_Position,
                            Attendance_Student.WorkingDays,
                            Attendance_Student.TotalPresent,
                            Attendance_Student.TotalAbsent,
                            Attendance_Student.TotalLate,
                            Attendance_Student.TotalLeave,
                            Exam_Cumulative_Student.ObtainedPercentage_ofStudent,
                            Exam_Cumulative_Student.StudentClassID,
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
                            LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
                            LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
                            LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
                            LEFT OUTER JOIN Attendance_Student ON Exam_Cumulative_Student.CumulativeNameID = Attendance_Student.CumulativeNameID 
                                AND Exam_Cumulative_Student.StudentID = Attendance_Student.StudentID 
                                AND Exam_Cumulative_Student.StudentClassID = Attendance_Student.StudentClassID
                        WHERE 
                            Exam_Cumulative_Student.StudentClassID = @StudentClassID
                            AND Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentClassID", studentClassID);
                        command.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetStudentResultData: " + ex.Message);
            }
            return dataTable;
        }

        protected void ResultRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var gradingRepeater = (Repeater)e.Item.FindControl("GradingSystemRepeater");
                if (gradingRepeater != null)
                    LoadGradingSystem(gradingRepeater);
            }
        }

        private void LoadGradingSystem(Repeater gradingRepeater)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = @"SELECT MARKS, Grades, Point FROM Exam_Grading_System WHERE SchoolID = @SchoolID ORDER BY Point DESC";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dt = new DataTable();
                            adapter.Fill(dt);
                            gradingRepeater.DataSource = dt;
                            gradingRepeater.DataBind();
                        }
                    }
                }
            }
            catch
            {
                var dt = CreateDefaultGradingSystem();
                gradingRepeater.DataSource = dt;
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

        private void LoadSignatures()
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            CASE WHEN Teacher_Sign IS NOT NULL AND DATALENGTH(Teacher_Sign) > 0 THEN 1 ELSE 0 END as HasTeacherSign,
                            CASE WHEN Principal_Sign IS NOT NULL AND DATALENGTH(Principal_Sign) > 0 THEN 1 ELSE 0 END as HasPrincipalSign
                        FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string ts = DateTime.Now.Ticks.ToString();
                                HiddenTeacherSign.Value = Convert.ToBoolean(reader["HasTeacherSign"]) ?
                                    $"/Handeler/SignatureHandler.ashx?type=teacher&schoolId={Session["SchoolID"]}&t={ts}" : "";
                                HiddenPrincipalSign.Value = Convert.ToBoolean(reader["HasPrincipalSign"]) ?
                                    $"/Handeler/SignatureHandler.ashx?type=principal&schoolId={Session["SchoolID"]}&t={ts}" : "";
                            }
                        }
                    }
                }
            }
            catch
            {
                HiddenTeacherSign.Value = "";
                HiddenPrincipalSign.Value = "";
            }
        }

        #region Helper Methods

        protected string GetDynamicInfoRow(object dataItem)
        {
            try
            {
                var row = (DataRowView)dataItem;
                var html = new StringBuilder();
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
                var workingDays = row["WorkingDays"] != DBNull.Value ? row["WorkingDays"].ToString() : "";
                var totalPresent = row["TotalPresent"] != DBNull.Value ? row["TotalPresent"].ToString() : "";
                var totalAbsent = row["TotalAbsent"] != DBNull.Value ? row["TotalAbsent"].ToString() : "";
                var totalLeave = row["TotalLeave"] != DBNull.Value ? row["TotalLeave"].ToString() : "";
                var totalMarks = row["Cu_Stu_TM"] != DBNull.Value ? row["Cu_Stu_TM"].ToString() : "";
                var obtainedMarks = row["Cu_Stu_OM"] != DBNull.Value ? row["Cu_Stu_OM"].ToString() : "";
                var percentage = row["ObtainedPercentage_ofStudent"] != DBNull.Value ?
                    decimal.Parse(row["ObtainedPercentage_ofStudent"].ToString()).ToString("F2") + "%" : "0.00%";
                var average = row["Average"] != DBNull.Value ?
                    decimal.Parse(row["Average"].ToString()).ToString("F2") : "0.00";
                var grade = row["Student_Grade"] != DBNull.Value ? row["Student_Grade"].ToString() : "A+";
                var gpa = row["Student_Point"] != DBNull.Value ?
                    decimal.Parse(row["Student_Point"].ToString()).ToString("F2") : "5.00";
                var positionClass = row["Position_InExam_Class"] != DBNull.Value && row["Position_InExam_Class"].ToString() != "0" ?
                    row["Position_InExam_Class"].ToString() : "-";
                var positionSection = row["Position_InExam_Subsection"] != DBNull.Value && row["Position_InExam_Subsection"].ToString() != "0" ?
                    row["Position_InExam_Subsection"].ToString() : "-";
                var comments = row["Student_Comments"] != DBNull.Value ? row["Student_Comments"].ToString() : "";

                var html = new StringBuilder();
                html.Append("<table class='summary-table' style='margin-top: 10px; width: 100%; border-collapse: collapse;'>");
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
                if (!IS_Hide_Class_Position)
                    html.Append("<td style='padding: 0px; text-align: center; border: 1px solid #ddd;'><strong>PC</strong></td>");
                if (HasSections && !IS_Hide_Sec_Position)
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
                if (!IS_Hide_Class_Position)
                    html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #228b22; color: white;'><strong>{positionClass}</strong></td>");
                if (HasSections && !IS_Hide_Sec_Position)
                    html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #228b22; color: white;'><strong>{positionSection}</strong></td>");
                html.Append($"<td style='padding: 8px; text-align: center; border: 1px solid #ddd; background-color: #800080; color: white;'><strong>{comments}</strong></td>");
                html.Append("</tr>");
                html.Append("</table>");
                return html.ToString();
            }
            catch
            {
                return "<table class='summary-table' style='margin-top: 10px;'><tr><td>Attendance data not available</td></tr></table>";
            }
        }

        protected string GenerateSubjectMarksTable(string studentClassID, string studentGrade, object studentPoint)
        {
            try
            {
                int cumulativeNameID = Convert.ToInt32(Cum_ExamDropDownList.SelectedValue);
                int classID = Convert.ToInt32(Session["ClassID"]);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var examList = GetCumulativeExamList(connection, studentClassID, classID, cumulativeNameID);
                    if (examList.Count == 0)
                        return "<div class='error'>No exam data found.</div>";

                    var cumulativeFMs = GetCumulativeFullMarks(connection, cumulativeNameID);
                    var subjectData = GetSubjectExamWiseMarks(connection, studentClassID, classID, cumulativeNameID, examList);

                    var html = new StringBuilder();
                    html.Append("<table class='marks-table'>");
                    html.Append(BuildExamHeaderRows(examList));
                    html.Append(BuildSubjectRows(subjectData, examList, cumulativeFMs));
                    html.Append(BuildCumulativeResultRow(connection, studentClassID, classID, cumulativeNameID, studentGrade, studentPoint, examList.Count));
                    html.Append("</table>");
                    return html.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"<div class='error'>Error loading subject marks: {ex.Message}</div>";
            }
        }

        private Dictionary<int, string> GetCumulativeFullMarks(SqlConnection connection, int cumulativeNameID)
        {
            var cumulativeFMs = new Dictionary<int, string>();
            string query = @"SELECT SubjectID, FullMarks FROM Exam_Cumulative_FullMarks
                WHERE CumulativeNameID = @CumulativeNameID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        cumulativeFMs[Convert.ToInt32(reader["SubjectID"])] = reader["FullMarks"].ToString();
                }
            }
            return cumulativeFMs;
        }

        private List<ExamInfo> GetCumulativeExamList(SqlConnection connection, string studentClassID, int classID, int cumulativeNameID)
        {
            var examList = new List<ExamInfo>();
            int latestSettingID = GetLatestSettingID(connection, classID, cumulativeNameID);
            if (latestSettingID == 0) return examList;

            string query = @"
                SELECT DISTINCT en.ExamName, cel.ExamID, cel.ExamAdd_Percentage, en.Period_StartDate
                FROM Exam_Cumulative_ExamList cel
                INNER JOIN Exam_Name en ON cel.ExamID = en.ExamID
                WHERE cel.Cumulative_SettingID = @Cumulative_SettingID
                AND cel.CumulativeNameID = @CumulativeNameID
                AND cel.SchoolID = @SchoolID
                AND cel.EducationYearID = @EducationYearID
                AND cel.ClassID = @ClassID
                ORDER BY en.Period_StartDate, cel.ExamID";

            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Cumulative_SettingID", latestSettingID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                cmd.Parameters.AddWithValue("@ClassID", classID);
                using (var reader = cmd.ExecuteReader())
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

        private int GetLatestSettingID(SqlConnection connection, int classID, int cumulativeNameID)
        {
            string query = @"
                SELECT TOP 1 Cumulative_SettingID FROM Exam_Cumulative_Setting
                WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
                AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
                ORDER BY Cumulative_SettingID DESC";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                cmd.Parameters.AddWithValue("@ClassID", classID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                var result = cmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
            }
        }

        private string BuildExamHeaderRows(List<ExamInfo> examList)
        {
            var html = new StringBuilder();
            html.Append("<tr>");
            html.Append("<th rowspan='2' style='background-color: #E6E6FA; width: 90px; min-width: 90px; max-width: 90px;'>SUBJECTS</th>");
            foreach (var exam in examList)
            {
                int examColspan = 1;
                if (!IS_Hide_FullMark) examColspan++;
                if (!IS_Hide_PassMark) examColspan++;
                html.Append($"<th colspan='{examColspan}' style='white-space: nowrap;'>{exam.ExamName} ({exam.Percentage}%)</th>");
            }
            int cumulativeColspan = 4;
            if (!IS_Hide_Class_Position) cumulativeColspan++;
            if (HasSections && !IS_Hide_Sec_Position) cumulativeColspan++;
            cumulativeColspan++;
            if (HasSections && !IS_Hide_Sec_Position) cumulativeColspan++;
            html.Append($"<th colspan='{cumulativeColspan}' style='background-color: #E6E6FA; white-space: nowrap;'>Cumulative Result</th>");
            html.Append("</tr>");
            html.Append("<tr>");
            foreach (var exam in examList)
            {
                if (!IS_Hide_FullMark) html.Append("<th style='width: 30px; min-width: 30px;'>FM</th>");
                if (!IS_Hide_PassMark) html.Append("<th style='width: 30px; min-width: 30px;'>PM</th>");
                html.Append("<th style='width: 35px; min-width: 35px;'>OM</th>");
            }
            html.Append("<th style='background-color: #E6E6FA; width: 35px; min-width: 35px;'>FM</th>");
            html.Append("<th style='background-color: #E6E6FA; width: 45px; min-width: 45px;'>OM</th>");
            html.Append("<th style='background-color: #E6E6FA; width: 45px; min-width: 45px;'>GRADE</th>");
            html.Append("<th style='background-color: #E6E6FA; width: 35px; min-width: 35px;'>GPA</th>");
            if (!IS_Hide_Class_Position) html.Append("<th style='background-color: #E6E6FA; width: 30px; min-width: 30px;'>PC</th>");
            if (HasSections && !IS_Hide_Sec_Position) html.Append("<th style='background-color: #E6E6FA; width: 30px; min-width: 30px;'>PS</th>");
            html.Append("<th style='background-color: #E6E6FA; width: 45px; min-width: 45px;'>HMC</th>");
            if (HasSections && !IS_Hide_Sec_Position) html.Append("<th style='background-color: #E6E6FA; width: 45px; min-width: 45px;'>HMS</th>");
            html.Append("</tr>");
            return html.ToString();
        }

        private DataTable GetSubjectExamWiseMarks(SqlConnection connection, string studentClassID, int classID, int cumulativeNameID, List<ExamInfo> examList)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("SubjectName", typeof(string));
            dataTable.Columns.Add("SubjectID", typeof(int));
            dataTable.Columns.Add("SN", typeof(int));
            dataTable.Columns.Add("SubjectType", typeof(string));
            foreach (var exam in examList)
            {
                if (!IS_Hide_FullMark) dataTable.Columns.Add($"FM_Exam{exam.ExamID}", typeof(string));
                if (!IS_Hide_PassMark) dataTable.Columns.Add($"PM_Exam{exam.ExamID}", typeof(string));
                dataTable.Columns.Add($"OM_Exam{exam.ExamID}", typeof(string));
            }
            dataTable.Columns.Add("Cu_Sub_TM", typeof(string));
            dataTable.Columns.Add("Cu_Sub_OM", typeof(string));
            dataTable.Columns.Add("Cu_Sub_Grades", typeof(string));
            dataTable.Columns.Add("Cu_Sub_Point", typeof(string));
            dataTable.Columns.Add("Position_InSubject_Class", typeof(string));
            dataTable.Columns.Add("Position_InSubject_Subsection", typeof(string));
            dataTable.Columns.Add("HighestMark_InSubject_Class", typeof(string));
            dataTable.Columns.Add("HighestMark_InSubject_Subsection", typeof(string));

            int latestSettingID = GetLatestSettingID(connection, classID, cumulativeNameID);

            string cumulativeQuery = @"
                SELECT Subject.SubjectName, Subject.SN, Exam_Cumulative_Subject.SubjectID,
                    Exam_Cumulative_Subject.TotalMark_ofSubject AS Cu_Sub_TM,
                    Exam_Cumulative_Subject.ObtainedMark_ofSubject AS Cu_Sub_OM,
                    Exam_Cumulative_Subject.SubjectGrades AS Cu_Sub_Grades,
                    Exam_Cumulative_Subject.SubjectPoint AS Cu_Sub_Point,
                    Exam_Cumulative_Subject.Position_InSubject_Class,
                    Exam_Cumulative_Subject.Position_InSubject_Subsection,
                    Exam_Cumulative_Subject.HighestMark_InSubject_Class,
                    Exam_Cumulative_Subject.HighestMark_InSubject_Subsection,
                    ISNULL(Exam_Cumulative_Subject.SubjectType, 'Compulsory') AS SubjectType
                FROM Exam_Cumulative_Subject 
                INNER JOIN Subject ON Exam_Cumulative_Subject.SubjectID = Subject.SubjectID
                WHERE Exam_Cumulative_Subject.StudentClassID = @StudentClassID
                AND Exam_Cumulative_Subject.SchoolID = @SchoolID
                AND Exam_Cumulative_Subject.EducationYearID = @EducationYearID
                AND Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID
                AND Exam_Cumulative_Subject.Cumulative_SettingID = @Cumulative_SettingID
                AND Exam_Cumulative_Subject.IS_Add_InExam = 1
                ORDER BY ISNULL(Subject.SN, 9999), Subject.SubjectName";

            using (var cmd = new SqlCommand(cumulativeQuery, connection))
            {
                cmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                cmd.Parameters.AddWithValue("@Cumulative_SettingID", latestSettingID);

                using (var reader = cmd.ExecuteReader())
                {
                    var subjectsData = new Dictionary<int, DataRow>();
                    while (reader.Read())
                    {
                        var row = dataTable.NewRow();
                        string subjectName = reader["SubjectName"].ToString();
                        string subjectType = reader["SubjectType"]?.ToString() ?? "Compulsory";
                        if (subjectType.Equals("Optional", StringComparison.OrdinalIgnoreCase))
                            subjectName = subjectName + " *";
                        row["SubjectName"] = subjectName;
                        row["SubjectID"] = reader["SubjectID"];
                        row["SN"] = reader["SN"] == DBNull.Value ? 9999 : Convert.ToInt32(reader["SN"]);
                        row["SubjectType"] = subjectType;
                        row["Cu_Sub_TM"] = FormatMarks(reader["Cu_Sub_TM"]);
                        row["Cu_Sub_OM"] = FormatMarks(reader["Cu_Sub_OM"]);
                        row["Cu_Sub_Grades"] = reader["Cu_Sub_Grades"] == DBNull.Value ? "-" : reader["Cu_Sub_Grades"].ToString();
                        row["Cu_Sub_Point"] = FormatPoint(reader["Cu_Sub_Point"]);
                        row["Position_InSubject_Class"] = FormatPosition(reader["Position_InSubject_Class"]);
                        row["Position_InSubject_Subsection"] = FormatPosition(reader["Position_InSubject_Subsection"]);
                        row["HighestMark_InSubject_Class"] = FormatMarks(reader["HighestMark_InSubject_Class"]);
                        row["HighestMark_InSubject_Subsection"] = FormatMarks(reader["HighestMark_InSubject_Subsection"]);
                        foreach (var exam in examList)
                        {
                            if (!IS_Hide_FullMark) row[$"FM_Exam{exam.ExamID}"] = "-";
                            if (!IS_Hide_PassMark) row[$"PM_Exam{exam.ExamID}"] = "-";
                            row[$"OM_Exam{exam.ExamID}"] = "-";
                        }
                        subjectsData[Convert.ToInt32(reader["SubjectID"])] = row;
                    }
                    reader.Close();

                    foreach (var subjectID in subjectsData.Keys.ToList())
                    {
                        var row = subjectsData[subjectID];
                        foreach (var exam in examList)
                        {
                            string examMarksQuery = @"
                                SELECT ers.TotalMark_ofSubject AS E_Subject_TM,
                                    ers.ObtainedMark_ofSubject AS E_Subject_OM,
                                    ers.SubjectAbsenceStatus AS E_Subject_Abs
                                FROM Exam_Result_of_Subject ers
                                INNER JOIN Exam_Result_of_Student erstu ON ers.StudentResultID = erstu.StudentResultID
                                WHERE erstu.StudentClassID = @StudentClassID
                                AND ers.SubjectID = @SubjectID
                                AND erstu.ExamID = @ExamID
                                AND erstu.SchoolID = @SchoolID
                                AND erstu.EducationYearID = @EducationYearID";

                            using (var examCmd = new SqlCommand(examMarksQuery, connection))
                            {
                                examCmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
                                examCmd.Parameters.AddWithValue("@SubjectID", subjectID);
                                examCmd.Parameters.AddWithValue("@ExamID", exam.ExamID);
                                examCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                                examCmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                                using (var examReader = examCmd.ExecuteReader())
                                {
                                    if (examReader.Read())
                                    {
                                        var fm = examReader["E_Subject_TM"];
                                        var om = examReader["E_Subject_OM"];
                                        var abs = examReader["E_Subject_Abs"]?.ToString() ?? "";
                                        if (!IS_Hide_FullMark) row[$"FM_Exam{exam.ExamID}"] = FormatMarks(fm);
                                        if (!IS_Hide_PassMark)
                                        {
                                            if (fm != DBNull.Value && decimal.TryParse(fm.ToString(), out decimal fmVal))
                                                row[$"PM_Exam{exam.ExamID}"] = FormatMarks(fmVal * 0.33m);
                                            else
                                                row[$"PM_Exam{exam.ExamID}"] = "-";
                                        }
                                        row[$"OM_Exam{exam.ExamID}"] = (abs == "Absent" || abs == "A") ? "Abs" : FormatMarks(om);
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

        private string BuildSubjectRows(DataTable subjectData, List<ExamInfo> examList, Dictionary<int, string> cumulativeFMs)
        {
            var html = new StringBuilder();
            foreach (DataRow row in subjectData.Rows)
            {
                html.Append("<tr>");
                html.Append($"<td class='subject-name' style='background-color: #F5F5F5; text-align: left;'>{row["SubjectName"]}</td>");
                foreach (var exam in examList)
                {
                    if (!IS_Hide_FullMark) html.Append($"<td>{row[$"FM_Exam{exam.ExamID}"]}</td>");
                    if (!IS_Hide_PassMark) html.Append($"<td>{row[$"PM_Exam{exam.ExamID}"]}</td>");
                    html.Append($"<td class='total-marks-cell'>{row[$"OM_Exam{exam.ExamID}"]}</td>");
                }
                int subjectID = Convert.ToInt32(row["SubjectID"]);
                string cuFM = cumulativeFMs.ContainsKey(subjectID) ? cumulativeFMs[subjectID] : row["Cu_Sub_TM"].ToString();
                html.Append($"<td style='background-color: #E6E6FA;'>{cuFM}</td>");
                html.Append($"<td class='total-marks-cell' style='background-color: #E6E6FA;'>{row["Cu_Sub_OM"]}</td>");
                html.Append($"<td class='grade-cell' style='background-color: #E6E6FA;'>{row["Cu_Sub_Grades"]}</td>");
                html.Append($"<td style='background-color: #E6E6FA;'>{row["Cu_Sub_Point"]}</td>");
                if (!IS_Hide_Class_Position) html.Append($"<td style='background-color: #E6E6FA;'>{row["Position_InSubject_Class"]}</td>");
                if (HasSections && !IS_Hide_Sec_Position) html.Append($"<td style='background-color: #E6E6FA;'>{row["Position_InSubject_Subsection"]}</td>");
                html.Append($"<td style='background-color: #E6E6FA;'>{row["HighestMark_InSubject_Class"]}</td>");
                if (HasSections && !IS_Hide_Sec_Position) html.Append($"<td style='background-color: #E6E6FA;'>{row["HighestMark_InSubject_Subsection"]}</td>");
                html.Append("</tr>");
            }
            return html.ToString();
        }

        private string BuildCumulativeResultRow(SqlConnection connection, string studentClassID, int classID, int cumulativeNameID, string studentGrade, object studentPoint, int examCount)
        {
            string totalMarks = "0", obtainedMarks = "0", positionClass = "-", positionSection = "-", highestClass = "-", highestSection = "-";
            int latestSettingID = GetLatestSettingID(connection, classID, cumulativeNameID);

            string query = @"
                SELECT TOP 1 TotalMark_ofStudent, ObtainedMark_ofStudent,
                    Position_InExam_Class, Position_InExam_Subsection,
                    HighestMark_InExam_Class, HighestMark_InExam_Subsection
                FROM Exam_Cumulative_Student
                WHERE StudentClassID = @StudentClassID
                AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
                AND CumulativeNameID = @CumulativeNameID
                AND Cumulative_SettingID = @Cumulative_SettingID
                ORDER BY Cumulative_StudentID DESC";

            try
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentClassID", studentClassID);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                    cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameID);
                    cmd.Parameters.AddWithValue("@Cumulative_SettingID", latestSettingID);
                    using (var reader = cmd.ExecuteReader())
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
            }
            catch { }

            var html = new StringBuilder();
            html.Append("<tr class='total-row' style='background-color: #D3D3D3; font-weight: bold;'>");
            html.Append("<td style='background-color: #D3D3D3;'><strong>Overall Result</strong></td>");
            int cellsPerExam = 1;
            if (!IS_Hide_FullMark) cellsPerExam++;
            if (!IS_Hide_PassMark) cellsPerExam++;
            for (int i = 0; i < examCount; i++)
                for (int j = 0; j < cellsPerExam; j++)
                    html.Append("<td style='background-color: #D3D3D3;'></td>");
            html.Append($"<td style='background-color: #E6E6FA;'><strong>{totalMarks}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA;'><strong>{obtainedMarks}</strong></td>");
            html.Append($"<td class='grade-cell' style='background-color: #E6E6FA;'><strong>{studentGrade}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA;'><strong>{FormatPoint(studentPoint)}</strong></td>");
            if (!IS_Hide_Class_Position) html.Append($"<td style='background-color: #E6E6FA;'><strong>{positionClass}</strong></td>");
            if (HasSections && !IS_Hide_Sec_Position) html.Append($"<td style='background-color: #E6E6FA;'><strong>{positionSection}</strong></td>");
            html.Append($"<td style='background-color: #E6E6FA;'><strong>{highestClass}</strong></td>");
            if (HasSections && !IS_Hide_Sec_Position) html.Append($"<td style='background-color: #E6E6FA;'><strong>{highestSection}</strong></td>");
            html.Append("</tr>");
            return html.ToString();
        }

        private class ExamInfo
        {
            public int ExamID { get; set; }
            public string ExamName { get; set; }
            public decimal Percentage { get; set; }
        }

        private string FormatMarks(object marks)
        {
            if (marks == null || marks == DBNull.Value) return "-";
            var marksStr = marks.ToString();
            if (marksStr == "0") return "0";
            if (decimal.TryParse(marksStr, out decimal marksValue))
                return marksValue == Math.Floor(marksValue) ? marksValue.ToString("0") : marksValue.ToString("0.##");
            return marksStr;
        }

        private string FormatPoint(object point)
        {
            if (point == null || point == DBNull.Value) return "0.00";
            if (decimal.TryParse(point.ToString(), out decimal pointValue))
                return pointValue.ToString("0.00");
            return "0.00";
        }

        private string FormatPosition(object position)
        {
            if (position == null || position == DBNull.Value) return "-";
            var posStr = position.ToString();
            return (posStr == "0" || string.IsNullOrEmpty(posStr)) ? "-" : posStr;
        }

        #endregion
    }
}
