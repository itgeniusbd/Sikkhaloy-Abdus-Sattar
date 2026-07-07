using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Student_Rerport
{
    public partial class Report : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        SqlDataAdapter Attendance_Calendar_DA;
        SqlDataAdapter Holiday_DA;
        SqlDataAdapter Leave_DA;

        DataSet Atten_DS = new DataSet();
        string StudentID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null)
            {
                if (!this.IsPostBack)
                {
                    StudentID = Request.QueryString["Student"];
                    if (string.IsNullOrEmpty(StudentID))
                        Response.Redirect("Students_List.aspx");
                }

                //Attendance
                Attendance_Calendar_DA = new SqlDataAdapter("SELECT Attendance, AttendanceDate, CONVERT(varchar(15), EntryTime, 100) AS EntryTime,CONVERT(varchar(15), ExitTime, 100) AS ExitTime FROM Attendance_Record Where StudentClassID = @StudentClassID and SchoolID = @SchoolID", con);
                Attendance_Calendar_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Attendance_Calendar_DA.SelectCommand.Parameters.AddWithValue("@StudentClassID", Request.QueryString["Student_Class"]);
                Attendance_Calendar_DA.Fill(Atten_DS, "Table");

                //Holidays
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = @SchoolID", con);
                Holiday_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Holiday_DA.Fill(Atten_DS, "HolidaysTable");

                // Student Leave
                Leave_DA = new SqlDataAdapter(
                    "SELECT StartDate, EndDate, ISNULL(LeaveType,'') AS LeaveType, ISNULL(Description,'') AS Description " +
                    "FROM Attendance_Leave " +
                    "WHERE SchoolID = @SchoolID AND StudentID = (SELECT StudentID FROM StudentsClass WHERE StudentClassID = @StudentClassID)", con);
                Leave_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Leave_DA.SelectCommand.Parameters.AddWithValue("@StudentClassID", Request.QueryString["Student_Class"]);
                Leave_DA.Fill(Atten_DS, "LeaveTable");
            }
        }
        //find by ID
        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count > 0)
            {
                Response.Redirect("Report.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
            }
        }

        //AttendanceCalendar
        protected void AttendanceCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            // If the month is CurrentMonth
            if (!e.Day.IsOtherMonth)
            {
                foreach (DataRow dr in Atten_DS.Tables[0].Rows)
                {
                    if ((dr["AttendanceDate"].ToString() != DBNull.Value.ToString()))
                    {
                        DateTime dtEvent = (DateTime)dr["AttendanceDate"];
                        Label lbl = new Label();

                        if (dtEvent.Equals(e.Day.Date))
                        {
                            lbl.Text += " (" + dr["Attendance"].ToString() + ")" + "<br />" + dr["EntryTime"].ToString();
                            if (dr["ExitTime"].ToString() != "")
                            {
                                lbl.Text += " - " + dr["ExitTime"].ToString();
                            }

                            lbl.CssClass = "Appointment";

                            string baseClass = e.Cell.CssClass.Contains("myCalendarToday") ? "myCalendarDay myCalendarToday" : "myCalendarDay";

                            if (dr["Attendance"].ToString() == "Pre")
                            {
                                e.Cell.CssClass = baseClass + " Pre";
                                e.Cell.BackColor = System.Drawing.Color.Empty;
                            }

                            if (dr["Attendance"].ToString() == "Abs")
                            {
                                e.Cell.CssClass = baseClass + " Abs";
                                e.Cell.BackColor = System.Drawing.Color.Empty;
                            }

                            if (dr["Attendance"].ToString() == "Late")
                            {
                                e.Cell.CssClass = baseClass + " Late";
                                e.Cell.BackColor = System.Drawing.Color.Empty;
                            }

                            if (dr["Attendance"].ToString() == "Late Abs")
                            {
                                e.Cell.CssClass = baseClass + " Late_Abs";
                                e.Cell.BackColor = System.Drawing.Color.Empty;
                            }

                            e.Cell.Controls.Add(lbl);
                        }
                    }
                }

                //Holidays
                foreach (DataRow dr in Atten_DS.Tables[1].Rows)
                {
                    if ((dr["HolidayDate"].ToString() != DBNull.Value.ToString()))
                    {
                        DateTime dtEvent = (DateTime)dr["HolidayDate"];
                        Label lbl = new Label();
                        lbl.CssClass = "Appointment";

                        if (dtEvent.Equals(e.Day.Date))
                        {
                            string baseClass = e.Cell.CssClass.Contains("myCalendarToday") ? "myCalendarDay myCalendarToday" : "myCalendarDay";
                            e.Cell.CssClass = baseClass + " Att_Holidays";
                            e.Cell.BackColor = System.Drawing.Color.Empty;

                            lbl.Text = "<br />";
                            lbl.Text += dr["HolidayName"].ToString();
                            e.Cell.Controls.Add(lbl);
                        }
                    }
                }

                // Student Leave
                if (Atten_DS.Tables.Contains("LeaveTable"))
                {
                    foreach (DataRow dr in Atten_DS.Tables["LeaveTable"].Rows)
                    {
                        if (dr["StartDate"] != DBNull.Value && dr["EndDate"] != DBNull.Value)
                        {
                            DateTime startDate = Convert.ToDateTime(dr["StartDate"]);
                            DateTime endDate   = Convert.ToDateTime(dr["EndDate"]);

                            if (e.Day.Date >= startDate && e.Day.Date <= endDate)
                            {
                            // Only apply leave color if not already colored by attendance
                                if (string.IsNullOrEmpty(e.Cell.CssClass) || e.Cell.CssClass == "myCalendarDay" || e.Cell.CssClass == "myCalendarDay myCalendarToday")
                                {
                                    string baseClass = e.Cell.CssClass.Contains("myCalendarToday") ? "myCalendarDay myCalendarToday" : "myCalendarDay";
                                    e.Cell.CssClass = baseClass + " Student_Leave";
                                    e.Cell.BackColor = System.Drawing.Color.Empty;
                                }

                                Label lbl = new Label();
                                lbl.CssClass = "Appointment";
                                string leaveType = dr["LeaveType"].ToString();
                                string desc      = dr["Description"].ToString();
                                lbl.Text = "ছুটি";
                                if (!string.IsNullOrWhiteSpace(leaveType))
                                    lbl.Text += ": " + leaveType;
                                if (!string.IsNullOrWhiteSpace(desc))
                                    lbl.Text += "<br/>" + desc;
                                e.Cell.Controls.Add(lbl);
                                break;
                            }
                        }
                    }
                }
            }
            //If the month is not CurrentMonth then hide the Dates
            else
            {
                e.Cell.Text = "";
            }
        }

        //Accounts
        protected void MreceiptLinkButton_Command(object sender, CommandEventArgs e)
        {
            AllPayRecordSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        //Individual Exam
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int examID = 0;
            int.TryParse(ExamDropDownList.SelectedValue, out examID);
            Panel panel = UpdatePanel2.FindControl("IndividualResultPanel") as Panel;
            if (panel != null)
            {
                panel.Visible = examID > 0;
                if (examID > 0) panel.DataBind();
            }
        }

        // ── HTML Result Card generation ──────────────────────────────────────
        public string RenderIndividualResultCard()
        {
            int examID = 0;
            int.TryParse(ExamDropDownList.SelectedValue, out examID);
            if (examID == 0) return string.Empty;

            string studentClassID = Request.QueryString["Student_Class"];
            string studentID      = Request.QueryString["Student"];
            if (string.IsNullOrEmpty(studentClassID) || string.IsNullOrEmpty(studentID))
                return string.Empty;

            DataRow student = LoadSingleStudentResult(studentClassID, examID);
            if (student == null) return "<p class='alert alert-warning'>No published result found for this exam.</p>";

            string studentResultID = student["StudentResultID"].ToString();
            DataTable grading      = GetGradingData(examID);

            var sb = new StringBuilder();
            sb.Append("<div class='sr-result-card'>");

            // Header
            sb.AppendFormat("<div class='src-header'>");
            sb.AppendFormat("<img src='/Handeler/SchoolLogo.ashx?SLogo={0}' class='src-logo' onerror=\"this.style.display='none';\" alt='' />", Session["SchoolID"]);
            sb.AppendFormat("<div class='src-school-info'><div class='src-school-name'>{0}</div><div class='src-school-addr'>{1}</div></div>", SafeStr(student, "SchoolName"), SafeStr(student, "Address"));
            sb.AppendFormat("<img src='/Handeler/Student_Photo.ashx?SID={0}' class='src-photo' onerror=\"this.style.display='none';\" alt='' />", SafeStr(student, "StudentImageID"));
            sb.Append("</div>");

            // Exam title
            sb.AppendFormat("<div class='src-exam-title'>{0}</div>", SafeStr(student, "ExamName"));

            // Student summary row
            string obtMark  = FormatMark(student, "TotalExamObtainedMark_ofStudent");
            string totMark  = FormatMark(student, "TotalMark_ofStudent");
            string pct      = SafeDecStr(student, "ObtainedPercentage_ofStudent", "F2") + "%";
            string avg      = SafeDecStr(student, "Average", "F2");
            string grade    = SafeStr(student, "Student_Grade");
            string gpa      = SafeDecStr(student, "Student_Point", "F2");
            int    posClsInt= SafeInt(student, "Position_InExam_Class");
            string posCls   = posClsInt > 0 ? ToOrdinal(posClsInt) : "-";
            string comment  = GetComment(grade, grading);
            string attHtml  = BuildAttendanceRow(studentResultID, studentClassID, examID);

            sb.Append("<div class='src-top'>");

            // Left: Student info table
            sb.Append("<div class='src-info-box'>");
            sb.Append("<table class='src-info-table'>");
            sb.AppendFormat("<tr><td>Name:</td><td colspan='3'><b>{0}</b></td></tr>", SafeStr(student, "StudentsName"));
            sb.AppendFormat("<tr><td>Class:</td><td><b>{0}</b></td><td>Shift:</td><td><b>{1}</b></td></tr>", SafeStr(student, "ClassName"), SafeStr(student, "ShiftName"));
            sb.AppendFormat("<tr><td>Roll:</td><td><b>{0}</b></td><td>ID:</td><td><b>{1}</b></td></tr>", SafeStr(student, "RollNo"), SafeStr(student, "ID"));
            string sectionName = SafeStr(student, "SectionName");
            string groupName   = SafeStr(student, "GroupName");
            if (!string.IsNullOrWhiteSpace(sectionName) || !string.IsNullOrWhiteSpace(groupName))
                sb.AppendFormat("<tr><td>Section:</td><td><b>{0}</b></td><td>Group:</td><td><b>{1}</b></td></tr>", sectionName, groupName);
            sb.Append("</table>");
            // Summary row spanning full width of info box
            sb.Append(attHtml);
            sb.Append("</div>");

            // Right: Grading chart
            sb.Append("<div class='src-grade-chart'><table>");
            sb.Append("<tr><th>Marks %</th><th>Grade</th><th>Point</th></tr>");
            foreach (DataRow gr in grading.Rows)
            {
                string gMarks  = (gr.Table.Columns.Contains("MARKS")  && gr["MARKS"]  != DBNull.Value) ? gr["MARKS"].ToString()  : "";
                string gGrades = (gr.Table.Columns.Contains("Grades") && gr["Grades"] != DBNull.Value) ? gr["Grades"].ToString() : "";
                decimal gPoint = (gr.Table.Columns.Contains("Point")  && gr["Point"]  != DBNull.Value) ? SafeDec(gr["Point"]) : 0m;
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", gMarks, gGrades, gPoint.ToString("F1"));
            }
            sb.Append("</table></div>");

            sb.Append("</div>"); // src-top

            // Subject marks table
            sb.Append(BuildSubjectTable(studentResultID, examID));

            // Footer note
            sb.Append("<p class='src-note'>WD=Working Days &nbsp;|&nbsp; PM=Pass Marks &nbsp;|&nbsp; FM=Full Marks &nbsp;|&nbsp; OM=Obtained Marks &nbsp;|&nbsp; PC=Position in Class &nbsp;|&nbsp; HMC=Highest Marks in Class</p>");
            sb.Append("</div>"); // sr-result-card

            return sb.ToString();
        }

        private DataRow LoadSingleStudentResult(string studentClassID, int examID)
        {
            try
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    string sql = @"SELECT TOP 1
                        ers.StudentResultID,
                        ers.ObtainedMark_ofStudent  AS TotalExamObtainedMark_ofStudent,
                        ers.Student_Grade, ers.Student_Point, ers.Average,
                        ers.ObtainedPercentage_ofStudent, ers.TotalMark_ofStudent,
                        ers.Position_InExam_Class, ers.Position_InExam_Subsection,
                        ers.PassStatus_Student AS PassStatus_ofStudent,
                        st.StudentsName, st.ID,
                        ISNULL(st.StudentImageID,0) AS StudentImageID,
                        sc.RollNo,
                        cc.Class AS ClassName,
                        ISNULL(css.Section,'')       AS SectionName,
                        ISNULL(csh.Shift,'')         AS ShiftName,
                        ISNULL(csg.SubjectGroup,'')  AS GroupName,
                        en.ExamName,
                        si.SchoolName, si.Address, si.Phone
                    FROM Exam_Result_of_Student ers
                    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
                    INNER JOIN Student       st ON sc.StudentID = st.StudentID
                    INNER JOIN CreateClass   cc ON sc.ClassID   = cc.ClassID
                    INNER JOIN Exam_Name     en ON ers.ExamID   = en.ExamID
                    INNER JOIN SchoolInfo    si ON ers.SchoolID = si.SchoolID
                    LEFT  JOIN CreateSection    css ON sc.SectionID      = css.SectionID
                    LEFT  JOIN CreateShift      csh ON sc.ShiftID        = csh.ShiftID
                    LEFT  JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
                    WHERE ers.StudentClassID = @SCID
                      AND ers.ExamID        = @ExamID
                      AND ers.SchoolID      = @SchoolID
                      AND ers.StudentPublishStatus = 'Pub'";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@SCID",     studentClassID);
                        cmd.Parameters.AddWithValue("@ExamID",   examID);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"] ?? 1);
                        var dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                    }
                }
            }
            catch { return null; }
        }

        private DataTable GetGradingData(int examID)
        {
            try
            {
                var ta = new EDUCATION.COM.Exam_ResultTableAdapters.Exam_Grading_SystemTableAdapter();
                string studentClassID = Request.QueryString["Student_Class"];
                int classID = 0;
                // get classID from StudentsClass
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT ClassID FROM StudentsClass WHERE StudentClassID=@SCID", con))
                    {
                        cmd.Parameters.AddWithValue("@SCID", studentClassID);
                        var v = cmd.ExecuteScalar();
                        if (v != null) classID = Convert.ToInt32(v);
                    }
                }
                var dt = ta.GetData(Convert.ToInt32(Session["SchoolID"]), classID, examID, Convert.ToInt32(Session["Edu_Year"]));
                if (dt.Rows.Count > 0) return dt;
            }
            catch { }
            // fallback
            var fb = new DataTable();
            fb.Columns.Add("Grades"); fb.Columns.Add("MARKS"); fb.Columns.Add("Point", typeof(decimal));
            fb.Rows.Add("A+","80-100",5m); fb.Rows.Add("A","70-79",4m); fb.Rows.Add("A-","60-69",3.5m);
            fb.Rows.Add("B","50-59",3m);   fb.Rows.Add("C","40-49",2m); fb.Rows.Add("D","33-39",1m); fb.Rows.Add("F","0-32",0m);
            return fb;
        }

        private string BuildAttendanceRow(string studentResultID, string studentClassID, int examID)
        {
            string wd="-", pre="-", abs="-", late="-", lateAbs="-", leave="-";
            string obtMark="-", totMark="-", pct="-", avg="-", grade="-", gpa="-", posCls="-", comment="-";
            try
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    con.Open();
                    // Get student result summary
                    using (var cmd = new SqlCommand(@"SELECT ers.ObtainedMark_ofStudent,ers.TotalMark_ofStudent,
                        ers.ObtainedPercentage_ofStudent,ers.Average,ers.Student_Grade,ers.Student_Point,ers.Position_InExam_Class
                        FROM Exam_Result_of_Student ers WHERE ers.StudentResultID=@SID", con))
                    {
                        cmd.Parameters.AddWithValue("@SID", studentResultID);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                decimal om = r["ObtainedMark_ofStudent"]==DBNull.Value?0:Convert.ToDecimal(r["ObtainedMark_ofStudent"]);
                                decimal tm = r["TotalMark_ofStudent"]==DBNull.Value?0:Convert.ToDecimal(r["TotalMark_ofStudent"]);
                                obtMark = om%1==0?om.ToString("F0"):om.ToString("F1");
                                totMark = tm.ToString("F0");
                                pct     = r["ObtainedPercentage_ofStudent"]==DBNull.Value?"-":Convert.ToDecimal(r["ObtainedPercentage_ofStudent"]).ToString("F2")+"%";
                                avg     = r["Average"]==DBNull.Value?"-":Convert.ToDecimal(r["Average"]).ToString("F2");
                                grade   = r["Student_Grade"]==DBNull.Value?"-":r["Student_Grade"].ToString();
                                gpa     = r["Student_Point"]==DBNull.Value?"-":Convert.ToDecimal(r["Student_Point"]).ToString("F2");
                                int posInt = r["Position_InExam_Class"]==DBNull.Value?0:Convert.ToInt32(r["Position_InExam_Class"]);
                                posCls  = posInt>0?ToOrdinal(posInt):"-";
                                // Calculate comment from grade
                                switch ((grade??"").ToUpper()) {
                                    case "A+": comment = "Outstanding"; break;
                                    case "A":  comment = "Excellent";   break;
                                    case "A-": comment = "Very Good";   break;
                                    case "B":  comment = "Good";        break;
                                    case "C":  comment = "Satisfactory"; break;
                                    case "D":  comment = "Acceptable";  break;
                                    case "F":  comment = "Fail";        break;
                                    default:   comment = "-";           break;
                                }
                            }
                        }
                    }
                    // Attendance from Exam_Publish_Setting dates
                    string fromDate=null, toDate=null;
                    int classID=0;
                    using (var cmd2 = new SqlCommand("SELECT ClassID FROM StudentsClass WHERE StudentClassID=@SCID",con))
                    {
                        cmd2.Parameters.AddWithValue("@SCID",studentClassID);
                        var v=cmd2.ExecuteScalar(); if(v!=null) classID=Convert.ToInt32(v);
                    }
                    using (var cmd3 = new SqlCommand(@"SELECT Attendance_FromDate,Attendance_ToDate FROM Exam_Publish_Setting WHERE SchoolID=@SID AND EducationYearID=@EY AND ExamID=@EID AND ClassID=@CID",con))
                    {
                        cmd3.Parameters.AddWithValue("@SID",Session["SchoolID"]??1);
                        cmd3.Parameters.AddWithValue("@EY",Session["Edu_Year"]??1);
                        cmd3.Parameters.AddWithValue("@EID",examID);
                        cmd3.Parameters.AddWithValue("@CID",classID);
                        using (var r3=cmd3.ExecuteReader())
                        {
                            if(r3.Read()){
                                fromDate=r3["Attendance_FromDate"]==DBNull.Value?null:r3["Attendance_FromDate"].ToString();
                                toDate  =r3["Attendance_ToDate"]  ==DBNull.Value?null:r3["Attendance_ToDate"].ToString();
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(fromDate) && !string.IsNullOrWhiteSpace(toDate))
                    {
                        using (var cmd4 = new SqlCommand(@"SELECT
                            dbo.F_Stu_WorkingDay(@SID,@EY,@CID,@F,@T) AS WD,
                            dbo.F_Stu_Attendance_Summary(@SID,@EY,@SCID,'Pre',@F,@T)      AS Pre,
                            dbo.F_Stu_Attendance_Summary(@SID,@EY,@SCID,'Abs',@F,@T)      AS Abs,
                            dbo.F_Stu_Attendance_Summary(@SID,@EY,@SCID,'Late',@F,@T)     AS Late,
                            dbo.F_Stu_Attendance_Summary(@SID,@EY,@SCID,'Late Abs',@F,@T) AS LateAbs,
                            dbo.F_Stu_Attendance_Summary(@SID,@EY,@SCID,'Leave',@F,@T)    AS Leave",con))
                        {
                            cmd4.Parameters.AddWithValue("@SID",Session["SchoolID"]??1);
                            cmd4.Parameters.AddWithValue("@EY",Session["Edu_Year"]??1);
                            cmd4.Parameters.AddWithValue("@CID",classID);
                            cmd4.Parameters.AddWithValue("@SCID",studentClassID);
                            cmd4.Parameters.AddWithValue("@F",fromDate);
                            cmd4.Parameters.AddWithValue("@T",toDate);
                            using(var r4=cmd4.ExecuteReader())
                            {
                                if(r4.Read()){
                                    wd      =r4["WD"]==DBNull.Value?"-":r4["WD"].ToString();
                                    pre     =r4["Pre"]==DBNull.Value?"-":r4["Pre"].ToString();
                                    abs     =r4["Abs"]==DBNull.Value?"-":r4["Abs"].ToString();
                                    late    =r4["Late"]==DBNull.Value?"-":r4["Late"].ToString();
                                    lateAbs =r4["LateAbs"]==DBNull.Value?"-":r4["LateAbs"].ToString();
                                    leave   =r4["Leave"]==DBNull.Value?"-":r4["Leave"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex){ System.Diagnostics.Debug.WriteLine("BuildAttendanceRow: "+ex.Message); }

            var sb = new StringBuilder();
            sb.Append("<table class='src-summary'><tr class='src-sum-hdr'>");
            sb.Append("<td>WD</td><td>Pre</td><td>Abs</td><td>L.Abs</td><td>Leave</td><td>Late</td>");
            sb.Append("<td>Obtained Marks</td><td>%</td><td>Average</td><td>Grade</td><td>GPA</td><td>PC</td><td>Comment</td>");
            sb.Append("</tr><tr class='src-sum-val'>");
            sb.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td>",wd,pre,abs,lateAbs,leave,late);
            sb.AppendFormat("<td>{0}/{1}</td><td>{2}</td><td>{3}</td>",obtMark,totMark,pct,avg);
            string gradeBg = grade=="A+"?"#4caf50":grade=="F"?"#f44336":"#2196f3";
            sb.AppendFormat("<td style='background:{1};color:#fff;font-weight:700'>{0}</td>",grade,gradeBg);
            sb.AppendFormat("<td style='background:#ff9800;color:#fff;font-weight:700'>{0}</td>",gpa);
            sb.AppendFormat("<td style='background:#9c27b0;color:#fff;font-weight:700'>{0}</td>",posCls);
            sb.AppendFormat("<td style='background:#388e3c;color:#fff;font-weight:700'>{0}</td>",comment);
            sb.Append("</tr></table>");
            return sb.ToString();
        }

        private string BuildSubjectTable(string studentResultID, int examID)
        {
            try
            {
                string conStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                // 1. Load subjects — same query as Individual_Result_For_Class, with IS_Add_InExam filter
                DataTable subjects = new DataTable();
                using (var con = new SqlConnection(conStr))
                {
                    con.Open();
                    string sql = @"SELECT
                        sub.SubjectID,
                        CASE WHEN ISNULL(sfg.SubjectType,'')='Optional' THEN ISNULL(sub.SubjectName,'')+' *' ELSE ISNULL(sub.SubjectName,'') END AS SubjectName,
                        ISNULL(sub.SN,999)                               AS SN,
                        ISNULL(ers.ObtainedMark_ofSubject, 0)            AS ObtainedMark_ofSubject,
                        ISNULL(ers.TotalMark_ofSubject,   0)             AS TotalMark_ofSubject,
                        ISNULL(ers.SubjectGrades,         '')            AS SubjectGrades,
                        ISNULL(ers.SubjectPoint,          0)             AS SubjectPoint,
                        ISNULL(ers.PassStatus_Subject,    'Pass')        AS PassStatus_Subject,
                        ISNULL(ers.HighestMark_InSubject_Class, 0)       AS HighestMark_InSubject_Class,
                        ISNULL(ers.Position_InSubject_Class,    0)       AS Position_InSubject_Class
                    FROM Exam_Result_of_Subject ers
                    INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
                    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                    LEFT JOIN SubjectForGroup sfg ON sub.SubjectID = sfg.SubjectID
                        AND sc.ClassID = sfg.ClassID
                        AND sc.SubjectGroupID = sfg.SubjectGroupID
                        AND ers.SchoolID = sfg.SchoolID
                    WHERE ers.StudentResultID = @SRID
                      AND ISNULL(ers.IS_Add_InExam, 1) = 1
                    ORDER BY ISNULL(sub.SN,999), sub.SubjectName";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandTimeout = 15;
                        cmd.Parameters.AddWithValue("@SRID", studentResultID);
                        new SqlDataAdapter(cmd).Fill(subjects);
                    }
                }
                if (subjects.Rows.Count == 0) return "<p class='alert alert-warning'>No subject data found.</p>";

                // 2. Check sub-exams
                bool hasSub = false;
                List<Tuple<int, string>> subExams = new List<Tuple<int, string>>();
                Dictionary<int, Dictionary<int, Tuple<string,string,string>>> allSubMarks
                    = new Dictionary<int, Dictionary<int, Tuple<string,string,string>>>();

                using (var con = new SqlConnection(conStr))
                {
                    con.Open();
                    object chkVal = null;
                    using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM Exam_Obtain_Marks WHERE ExamID=@EID AND SchoolID=@SID AND StudentResultID=@SRID", con))
                    {
                        chk.Parameters.AddWithValue("@EID", examID);
                        chk.Parameters.AddWithValue("@SID", Session["SchoolID"] ?? 1);
                        chk.Parameters.AddWithValue("@SRID", studentResultID);
                        chkVal = chk.ExecuteScalar();
                    }
                    hasSub = (chkVal != null && chkVal != DBNull.Value && Convert.ToInt32(chkVal) > 0);

                    if (hasSub)
                    {
                        string seSql = @"SELECT DISTINCT esn.SubExamID, esn.SubExamName, esn.Sub_ExamSN
                            FROM Exam_SubExam_Name esn
                            INNER JOIN Exam_Obtain_Marks eom ON esn.SubExamID = eom.SubExamID
                            WHERE esn.SchoolID=@SID AND esn.EducationYearID=@EY AND eom.ExamID=@EID
                              AND eom.StudentResultID=@SRID
                            ORDER BY esn.Sub_ExamSN";
                        using (var cmd = new SqlCommand(seSql, con))
                        {
                            cmd.Parameters.AddWithValue("@SID", Session["SchoolID"] ?? 1);
                            cmd.Parameters.AddWithValue("@EY",  Session["Edu_Year"]  ?? 1);
                            cmd.Parameters.AddWithValue("@EID", examID);
                            cmd.Parameters.AddWithValue("@SRID", studentResultID);
                            var dt2 = new DataTable();
                            new SqlDataAdapter(cmd).Fill(dt2);
                            foreach (DataRow r in dt2.Rows)
                            {
                                int seID2 = (r["SubExamID"] == DBNull.Value) ? 0 : Convert.ToInt32(r["SubExamID"]);
                                string seName = (r["SubExamName"] == DBNull.Value) ? "" : r["SubExamName"].ToString();
                                if (seID2 > 0) subExams.Add(Tuple.Create(seID2, seName));
                            }
                        }

                        string markSql = @"SELECT SubExamID, SubjectID,
                            ISNULL(CAST(FullMark      AS NVARCHAR(20)),'') AS FullMark,
                            ISNULL(CAST(PassMark      AS NVARCHAR(20)),'') AS PassMark,
                            ISNULL(CAST(MarksObtained AS NVARCHAR(20)),'') AS MarksObtained
                            FROM Exam_Obtain_Marks
                            WHERE StudentResultID=@SRID AND ExamID=@EID AND SchoolID=@SID";
                        using (var cmd = new SqlCommand(markSql, con))
                        {
                            cmd.Parameters.AddWithValue("@SRID", studentResultID);
                            cmd.Parameters.AddWithValue("@EID",  examID);
                            cmd.Parameters.AddWithValue("@SID",  Session["SchoolID"] ?? 1);
                            var dt3 = new DataTable();
                            new SqlDataAdapter(cmd).Fill(dt3);
                            foreach (DataRow r in dt3.Rows)
                            {
                                if (r["SubjectID"] == DBNull.Value || r["SubExamID"] == DBNull.Value) continue;
                                int subID2 = Convert.ToInt32(r["SubjectID"]);
                                int seID   = Convert.ToInt32(r["SubExamID"]);
                                string fm2   = FormatDecStr(r["FullMark"]);
                                string pm2   = FormatDecStr(r["PassMark"]);
                                string omRaw = (r["MarksObtained"] == DBNull.Value) ? "" : r["MarksObtained"].ToString();
                                string om2   = string.IsNullOrWhiteSpace(omRaw) ? "Abs" : FormatDecStr(omRaw);
                                if (!allSubMarks.ContainsKey(subID2))
                                    allSubMarks[subID2] = new Dictionary<int, Tuple<string,string,string>>();
                                allSubMarks[subID2][seID] = Tuple.Create(fm2, pm2, om2);
                            }
                        }
                    }
                }

                // 3. Build HTML
                var sb = new StringBuilder();
                string hBg  = "#c8e6c9";
                string cellSty = "border:1px solid #0072bc;padding:4px 6px;text-align:center;font-weight:bold;font-size:11px;font-family:Arial,sans-serif;";
                string hSty = cellSty + "background:" + hBg + ";";
                string dSty = "border:1px solid #0072bc;padding:4px 6px;text-align:center;font-size:11px;font-family:Arial,sans-serif;background:#fff;color:#000;";
                string dStyL= "border:1px solid #0072bc;padding:4px 12px;text-align:left;font-size:11px;font-family:Arial,sans-serif;background:#fff;color:#000;";
                string posBg= "#e8f4fd";
                string hmcBg= "#e8f4fd";

                sb.Append("<div style='overflow-x:auto;width:100%'><table class='src-marks-table' style='font-size:11px;font-family:Arial,sans-serif;border-collapse:collapse;width:100%'>");

                if (hasSub && subExams.Count > 0)
                {
                    sb.AppendFormat("<tr style='background:{0}'>", hBg);
                    sb.AppendFormat("<th rowspan='2' style='{0}min-width:130px;text-align:left;'>SUBJECTS</th>", hSty);
                    foreach (var se in subExams)
                        sb.AppendFormat("<th colspan='3' style='{0}'>{1}</th>", hSty, se.Item2);
                    sb.AppendFormat("<th rowspan='2' style='{0}min-width:70px;'>MARKS</th>", hSty);
                    sb.AppendFormat("<th rowspan='2' style='{0}'>GRADE</th>", hSty);
                    sb.AppendFormat("<th rowspan='2' style='{0}'>GPA</th>", hSty);
                    sb.AppendFormat("<th rowspan='2' style='{0}background:{1};'>PC</th>", cellSty, posBg);
                    sb.AppendFormat("<th rowspan='2' style='{0}background:{1};'>HMC</th>", cellSty, hmcBg);
                    sb.Append("</tr>");
                    sb.AppendFormat("<tr style='background:{0}'>", hBg);
                    foreach (var se in subExams)
                    {
                        sb.AppendFormat("<th style='{0}'>FM</th>", hSty);
                        sb.AppendFormat("<th style='{0}'>PM</th>", hSty);
                        sb.AppendFormat("<th style='{0}'>OM</th>", hSty);
                    }
                    sb.Append("</tr>");

                    foreach (DataRow row in subjects.Rows)
                    {
                        int     subID  = (row["SubjectID"] == DBNull.Value) ? 0 : Convert.ToInt32(row["SubjectID"]);
                        string  subNm  = (row["SubjectName"] == DBNull.Value) ? "" : row["SubjectName"].ToString();
                        string  sg     = (row["SubjectGrades"] == DBNull.Value) ? "" : row["SubjectGrades"].ToString();
                        decimal sp     = SafeDec(row["SubjectPoint"]);
                        decimal om     = SafeDec(row["ObtainedMark_ofSubject"]);
                        decimal fm     = SafeDec(row["TotalMark_ofSubject"]);
                        decimal hmc    = SafeDec(row["HighestMark_InSubject_Class"]);
                        string  posStr = (row["Position_InSubject_Class"] == DBNull.Value) ? "" : row["Position_InSubject_Class"].ToString();
                        string  posCls = BuildOrdinal(posStr);
                        bool    failed = string.Equals(sg, "F", StringComparison.OrdinalIgnoreCase);
                        string  rowBg  = failed ? "#fff0f0" : "#fff";

                        sb.AppendFormat("<tr style='background:{0}'>", rowBg);
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dStyL, subNm);
                        var subMarksDict = allSubMarks.ContainsKey(subID) ? allSubMarks[subID] : new Dictionary<int, Tuple<string,string,string>>();
                        foreach (var se in subExams)
                        {
                            if (subMarksDict.ContainsKey(se.Item1))
                            {
                                var m = subMarksDict[se.Item1];
                                sb.AppendFormat("<td style='{0}'>{1}</td><td style='{0}'>{2}</td><td style='{0}'>{3}</td>", dSty, m.Item1, m.Item2, m.Item3);
                            }
                            else
                            {
                                sb.AppendFormat("<td style='{0}color:#aaa'>-</td><td style='{0}color:#aaa'>-</td><td style='{0}color:#aaa'>-</td>", dSty);
                            }
                        }
                        string omDisp = (om == 0 && string.IsNullOrWhiteSpace(sg)) ? "Abs" : (om % 1 != 0 ? om.ToString("F1") : om.ToString("F0"));
                        sb.AppendFormat("<td style='{0}'>{1}/{2}</td>", dSty, omDisp, fm.ToString("F0"));
                        string gBg = failed ? "background:#f44336;color:#fff;font-weight:700;" : "";
                        sb.AppendFormat("<td style='{0}{1}'>{2}</td>", dSty, gBg, string.IsNullOrEmpty(sg) ? "-" : sg);
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dSty, sp.ToString("F2"));
                        sb.AppendFormat("<td style='{0}background:{1};'>{2}</td>", dSty, posBg, posCls);
                        sb.AppendFormat("<td style='{0}background:{1};'>{2}</td>", dSty, hmcBg, hmc > 0 ? hmc.ToString("F0") : "-");
                        sb.Append("</tr>");
                    }
                }
                else
                {
                    // Simple table — matches Individual_Result_For_Class layout
                    sb.AppendFormat("<tr style='background:{0}'>", hBg);
                    sb.AppendFormat("<th style='{0}min-width:160px;text-align:left;'>Subject</th>", hSty);
                    sb.AppendFormat("<th style='{0}min-width:35px;'>OM</th>",    hSty);
                    sb.AppendFormat("<th style='{0}min-width:35px;'>FM</th>",    hSty);
                    sb.AppendFormat("<th style='{0}min-width:35px;'>PM</th>",    hSty);
                    sb.AppendFormat("<th style='{0}min-width:35px;'>Grade</th>", hSty);
                    sb.AppendFormat("<th style='{0}min-width:40px;'>Point</th>", hSty);
                    sb.AppendFormat("<th style='{0}min-width:35px;background:{1};'>PC</th>",  cellSty, posBg);
                    sb.AppendFormat("<th style='{0}min-width:40px;background:{1};'>HMC</th>", cellSty, hmcBg);
                    sb.Append("</tr>");

                    foreach (DataRow row in subjects.Rows)
                    {
                        string  subNm  = (row["SubjectName"] == DBNull.Value) ? "" : row["SubjectName"].ToString();
                        string  sg     = (row["SubjectGrades"] == DBNull.Value) ? "" : row["SubjectGrades"].ToString();
                        decimal sp     = SafeDec(row["SubjectPoint"]);
                        decimal om     = SafeDec(row["ObtainedMark_ofSubject"]);
                        decimal fm     = SafeDec(row["TotalMark_ofSubject"]);
                        decimal pm     = Math.Floor(fm * 0.33m);
                        decimal hmc    = SafeDec(row["HighestMark_InSubject_Class"]);
                        string  posStr = (row["Position_InSubject_Class"] == DBNull.Value) ? "" : row["Position_InSubject_Class"].ToString();
                        string  posCls = BuildOrdinal(posStr);
                        bool    isAbs  = (om == 0 && string.IsNullOrWhiteSpace(sg));
                        string  dispOM = isAbs ? "Abs" : (om % 1 != 0 ? om.ToString("F1") : om.ToString("F0"));
                        bool    failed = string.Equals(sg, "F", StringComparison.OrdinalIgnoreCase);
                        string  rowBg  = failed ? "#fff0f0" : "#fff";
                        string  dispGrade = string.IsNullOrWhiteSpace(sg) ? "-" : sg;

                        sb.AppendFormat("<tr style='border:1px solid #0072bc;'>", rowBg);
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dStyL, subNm);
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dSty, dispOM);
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dSty, fm.ToString("F0"));
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dSty, pm.ToString("F0"));
                        string gBg = failed ? "background:#f44336;color:#fff;font-weight:700;" : "";
                        sb.AppendFormat("<td style='{0}{1}'>{2}</td>", dSty, gBg, dispGrade);
                        sb.AppendFormat("<td style='{0}'>{1}</td>", dSty, sp.ToString("F2"));
                        sb.AppendFormat("<td style='{0}background:{1};'>{2}</td>", dSty, posBg, posCls);
                        sb.AppendFormat("<td style='{0}background:{1};'>{2}</td>", dSty, hmcBg, hmc > 0 ? hmc.ToString("F0") : "-");
                        sb.Append("</tr>");
                    }
                }
                sb.Append("</table></div>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "<p class='alert alert-danger' style='color:red;padding:8px;'>Error loading subject table: " + ex.Message + " | " + ex.StackTrace + "</p>";
            }
        }

        // Convert stored position string/int to ordinal
        private string BuildOrdinal(string posStr)
        {
            if (string.IsNullOrWhiteSpace(posStr) || posStr == "0") return "-";
            int p = 0;
            if (int.TryParse(posStr, out p) && p > 0) return ToOrdinal(p);
            return posStr; // already ordinal string
        }

        private string GetComment(string grade, DataTable grading)
        {
            foreach (DataRow r in grading.Rows)
                if (string.Equals(r["Grades"].ToString(), grade, StringComparison.OrdinalIgnoreCase))
                {
                    if (grading.Columns.Contains("Comments") && r["Comments"]!=DBNull.Value && !string.IsNullOrEmpty(r["Comments"].ToString()))
                        return r["Comments"].ToString();
                }
            switch ((grade??"").ToUpper())
            {
                case "A+": return "Outstanding"; case "A": return "Excellent"; case "A-": return "Very Good";
                case "B":  return "Good";        case "C": return "Satisfactory"; case "D": return "Acceptable";
                default:   return "Fail";
            }
        }

        private string ToOrdinal(int n)
        {
            if(n<=0) return n.ToString();
            switch(n%100){case 11:case 12:case 13:return n+"th";}
            switch(n%10){case 1:return n+"st";case 2:return n+"nd";case 3:return n+"rd";default:return n+"th";}
        }
        private string SafeStr(DataRow r, string col) => r.Table.Columns.Contains(col)&&r[col]!=DBNull.Value?r[col].ToString():"";
        private int    SafeInt(DataRow r, string col) { int v=0; if(r.Table.Columns.Contains(col)&&r[col]!=DBNull.Value) int.TryParse(r[col].ToString(),out v); return v; }
        private decimal SafeDec(object val) { if(val==null||val==DBNull.Value) return 0m; decimal v; decimal.TryParse(val.ToString(), out v); return v; }
        private string SafeDecStr(DataRow r, string col, string fmt="F2") { if(r.Table.Columns.Contains(col)&&r[col]!=DBNull.Value){decimal v;if(decimal.TryParse(r[col].ToString(),out v))return v.ToString(fmt);} return "-"; }
        private string FormatMark(DataRow r, string col) { if(r.Table.Columns.Contains(col)&&r[col]!=DBNull.Value){decimal v;if(decimal.TryParse(r[col].ToString(),out v))return v%1==0?v.ToString("F0"):v.ToString("F1");} return "-"; }
        private string FormatDecStr(object val) { if(val==null||val==DBNull.Value) return "-"; decimal v; if(decimal.TryParse(val.ToString(),out v)) return v%1==0?v.ToString("F0"):v.ToString("F1"); return val.ToString(); }

        //Cumulative Result
        protected void Cum_ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cu_ResultReportViewer.LocalReport.Refresh();
            this.Cu_ResultReportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(Cum_ResultReportViewer_SubreportProcessing);
        }
        void Cum_ResultReportViewer_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            e.DataSources.Add(new ReportDataSource("DataSet1", Cu_GradingSystemODS));
        }

        protected void Fault_Add_Button_Click(object sender, EventArgs e)
        {
            FaultSQL.Insert();
            Fault_Title_TextBox.Text = "";
            Fault_TextBox.Text = "";
            Fault_Date_TextBox.Text = "";
            Fault_Gridview.DataBind();

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

        //Exam chart data
        [WebMethod]
        public static List<object> Get_Exam_GradePoint(string EducationYearID, string StudentID)
        {
            List<object> chartData = new List<object>();
            List<string> ExamName = new List<string>();
            List<string> Point = new List<string>();
            List<string> Grade = new List<string>();

            string query = "SELECT Exam_Name.ExamName, Exam_Result_of_Student.Student_Point,Exam_Result_of_Student.Student_Grade FROM Exam_Result_of_Student INNER JOIN Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID WHERE(Exam_Result_of_Student.StudentID = @StudentID) AND(Exam_Result_of_Student.StudentPublishStatus = N'Pub') AND(Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY Exam_Name.Period_StartDate";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", StudentID);
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            ExamName.Add(sdr["ExamName"].ToString());
                            Point.Add(sdr["Student_Point"].ToString());
                            Grade.Add(sdr["Student_Grade"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(ExamName);
                    chartData.Add(Point);
                    chartData.Add(Grade);

                    return chartData;
                }
            }
        }

        [WebMethod]
        public static List<object> Get_CumilativeExam(string EducationYearID, string StudentID)
        {
            List<object> chartData = new List<object>();
            List<string> ExamName = new List<string>();
            List<string> Point = new List<string>();
            List<string> Grade = new List<string>();

            string query = "SELECT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Student.Student_Point, Exam_Cumulative_Student.Student_Grade FROM Exam_Cumulative_Student INNER JOIN Exam_Cumulative_Name ON Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID WHERE(Exam_Cumulative_Student.StudentID = @StudentID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID)";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", StudentID);
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            ExamName.Add(sdr["CumulativeResultName"].ToString());
                            Point.Add(sdr["Student_Point"].ToString());
                            Grade.Add(sdr["Student_Grade"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(ExamName);
                    chartData.Add(Point);
                    chartData.Add(Grade);

                    return chartData;
                }
            }
        }
    }
}