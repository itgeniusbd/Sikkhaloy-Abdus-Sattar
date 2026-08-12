using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Leave_Print : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "utf-8";
            if (!IsPostBack)
            {
                if (Session["RegistrationID"] != null)
                    _regIdHidden.Value = Session["RegistrationID"].ToString();
                int leaveID = 0;
                if (Request.QueryString["lid"] != null && int.TryParse(Request.QueryString["lid"], out leaveID))
                    RenderGatePass(leaveID);
                else
                    PrintLiteral.Text = "<div style='text-align:center;padding:30px;color:red;'>ছুটির রেকর্ড পাওয়া যায়নি।</div>";
            }
        }

        private void RenderGatePass(int studentLeaveID)
        {
            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            string sql = "SELECT al.StudentLeaveID, al.StartDate, al.EndDate, al.Description, " +
                "DATEDIFF(DAY, al.StartDate, al.EndDate) + 1 AS LeaveDays, " +
                "ISNULL(al.LeaveType,'') AS LeaveType, ISNULL(al.GuardianName,'') AS GuardianName, " +
                "s.StudentsName, s.FathersName, s.ID AS StudentDisplayID, si.SchoolName, " +
                "ISNULL(si.Address,'') + ISNULL(', ' + si.City,'') + ISNULL(', ' + si.State,'') AS SchoolAddress, " +
                "ISNULL(si.Phone,'') AS SchoolPhone, " +
                "ISNULL(cc.Class,'') AS ClassName, ISNULL(csg.SubjectGroup,'') AS GroupName, ey.EducationYear " +
                "FROM Attendance_Leave al " +
                "INNER JOIN Student s ON al.StudentID = s.StudentID " +
                "INNER JOIN SchoolInfo si ON al.SchoolID = si.SchoolID " +
                "LEFT JOIN StudentsClass sc ON sc.StudentID = s.StudentID AND sc.EducationYearID = al.EducationYearID " +
                "LEFT JOIN CreateClass cc ON sc.ClassID = cc.ClassID " +
                "LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID " +
                "LEFT JOIN Education_Year ey ON al.EducationYearID = ey.EducationYearID " +
                "WHERE al.StudentLeaveID = @StudentLeaveID";

            string approverName = GetApproverName(connStr);

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@StudentLeaveID", studentLeaveID);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string schoolName    = dr["SchoolName"].ToString();
                        string schoolAddress = dr["SchoolAddress"].ToString();
                        string schoolPhone   = dr["SchoolPhone"].ToString();
                        string studentName   = dr["StudentsName"].ToString();
                        string fathersName   = dr["FathersName"].ToString();
                        string studentDisplayID = dr["StudentDisplayID"].ToString();
                        string className     = dr["ClassName"].ToString();
                        string groupName     = dr["GroupName"].ToString();
                        string eduYear       = dr["EducationYear"].ToString();
                        string description   = dr["Description"].ToString();
                        string leaveType     = dr["LeaveType"].ToString();
                        string guardianName  = dr["GuardianName"].ToString();
                        int    leaveDays     = Convert.ToInt32(dr["LeaveDays"]);
                        DateTime startDate   = Convert.ToDateTime(dr["StartDate"]);
                        DateTime endDate     = Convert.ToDateTime(dr["EndDate"]);
                        string classInfo     = string.IsNullOrWhiteSpace(groupName) ? className : className + " (" + groupName + ")";
                        PrintLiteral.Text = BuildGatePassHtml(schoolName, schoolAddress, schoolPhone,
                            studentName, fathersName, studentDisplayID, classInfo, eduYear,
                            description, leaveType, guardianName, leaveDays,
                            startDate, endDate, DateTime.Now, studentLeaveID, approverName);
                    }
                    else
                        PrintLiteral.Text = "<div style='text-align:center;padding:30px;color:red;'>ছুটির রেকর্ড পাওয়া যায়নি।</div>";
                }
            }
        }

        private string GetApproverName(string connStr)
        {
            // Try to get name from session RegistrationID
            if (Session["RegistrationID"] == null) return "";
            string regId = Session["RegistrationID"].ToString();
            if (string.IsNullOrWhiteSpace(regId)) return "";

            // Try Admin/Sub-Admin name first, then Teacher name
            string name = "";
            string sql = @"SELECT TOP 1 ISNULL(a.FirstName + ' ' + ISNULL(a.LastName,''), '') AS FullName
                           FROM Admin a
                           WHERE a.RegistrationID = @RegID AND LEN(ISNULL(a.FirstName,'')) > 0
                           UNION ALL
                           SELECT TOP 1 ISNULL(t.FirstName + ' ' + ISNULL(t.LastName,''), '')
                           FROM Teacher t
                           WHERE t.RegistrationID = @RegID AND LEN(ISNULL(t.FirstName,'')) > 0";

            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@RegID", regId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        name = result.ToString().Trim();
                }
            }
            catch { }

            return name;
        }

        private string BuildGatePassHtml(string schoolName, string schoolAddress, string schoolPhone,
            string studentName, string fathersName, string studentDisplayID, string classInfo,
            string eduYear, string description, string leaveType, string guardianName,
            int leaveDays, DateTime startDate, DateTime endDate, DateTime approvedOn, int leaveNo,
            string approverName = "")
        {
            string sd = startDate.ToString("dd/MM/yyyy");
            string ed = endDate.ToString("dd/MM/yyyy");
            string ad = approvedOn.ToString("dd/MM/yyyy");
            string at = approvedOn.ToString("h:mm:ss tt");

            string leaveLabel = !string.IsNullOrWhiteSpace(leaveType)
                ? Encode(leaveType)
                : "মাসিক ছুটি";

            string guardianLabel = !string.IsNullOrWhiteSpace(guardianName)
                ? Encode(guardianName)
                : "বাবা";

            var sb = new StringBuilder();
            sb.Append("<div class=\"page-wrapper\">");
            sb.Append(CopySection(schoolName, schoolAddress, schoolPhone, studentName, fathersName,
                studentDisplayID, classInfo, leaveLabel, leaveNo, ad, sd, ed, at, leaveDays, description, ad, guardianLabel,
                "শিক্ষার্থীর কপি", approverName));
            sb.Append("<div class=\"scissor-divider\">&#9988; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -</div>");
            sb.Append(OfficeSection(studentName, fathersName, studentDisplayID, classInfo, leaveNo, sd, ed, leaveDays, leaveLabel, approverName));
            sb.Append("</div>");
            return sb.ToString();
        }

        private string CopySection(string sn, string sa, string sp, string stuN, string fatN,
            string stuID, string cls, string leaveL, int leaveNo, string issueDate,
            string sd, string ed, string at, int days, string desc, string apDate,
            string guardianLabel, string copyTitle, string approverName = "")
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"gp-card\">");

            // Header
            sb.Append("<div class=\"gp-header\">");
            sb.Append("<img class=\"logo\" src=\"/Handeler/School_Logo.ashx\" onerror=\"this.style.display='none'\" />");
            sb.Append("<div class=\"school-info\">");
            sb.AppendFormat("<div class=\"school-name\">{0}</div>", Encode(sn));
            sb.AppendFormat("<div class=\"school-addr\">{0}</div>", Encode(sa));
            sb.AppendFormat("<div class=\"school-phone\">ফোন: {0}</div>", Encode(sp));
            sb.Append("</div></div>");

            // Title band
            sb.Append("<div class=\"gp-title-band\">");
            sb.AppendFormat("<div class=\"band-left\">পাস নং : {0}</div>", leaveNo);
            sb.Append("<div class=\"band-title\">গেইট পাস</div>");
            sb.AppendFormat("<div class=\"band-right\">{0}</div>", copyTitle);
            sb.Append("</div>");

            // Info grid
            sb.Append("<div class=\"gp-info\">");
            sb.Append("<div class=\"gp-info-col\">");
            sb.Append(IR("নাম", Encode(stuN)));
            sb.Append(IR("পিতা", Encode(fatN)));
            sb.Append(IR("শ্রেণি", Encode(cls)));
            sb.Append("</div>");
            sb.Append("<div class=\"gp-info-col\">");
            sb.Append(IR("আইডি", Encode(stuID)));
            sb.Append(IR("তারিখ", issueDate));
            sb.Append(IR("ছুটির ধরণ", leaveL));
            sb.Append("</div>");
            sb.Append("</div>");

            // Date/time table
            sb.Append("<table class=\"gp-table\"><thead><tr>");
            sb.Append("<th style=\"width:22%;\">&nbsp;</th>");
            sb.Append("<th>প্রস্থান</th><th>আগমন</th><th>মেয়াদ</th>");
            sb.Append("</tr></thead><tbody>");
            sb.AppendFormat("<tr><td class=\"row-lbl\">তারিখ</td><td>{0}</td><td>{1}</td><td>{2} দিন</td></tr>", sd, ed, days);
            sb.AppendFormat("<tr><td class=\"row-lbl\">সময়</td><td>{0}</td><td>--</td><td>&nbsp;</td></tr>", Encode(at));
            sb.Append("</tbody></table>");

            // Remarks
            sb.Append("<div class=\"gp-remarks\">");
            sb.Append("<span class=\"lbl\">ছুটির কারন :</span>");
            sb.Append(Encode(desc));
            sb.Append("</div>");

            // Footer — show approver name instead of plain date
            string approverHtml = string.IsNullOrWhiteSpace(approverName)
                ? string.Format("<div class=\"issue-date\">তারিখ : {0}</div>", Encode(apDate))
                : string.Format("<div class=\"issue-date\"><strong>অনুমতি দাতা :</strong> {0}<br/>{1}</div>", Encode(approverName), Encode(apDate));

            sb.Append("<div class=\"gp-footer\">");
            sb.AppendFormat("<div class=\"guardian\"><strong>অভিভাবক</strong> : {0}</div>", guardianLabel);
            sb.Append("<div class=\"approval\">✔ অনুমতি দেওয়া হলো</div>");
            sb.Append(approverHtml);
            sb.Append("</div>");

            sb.Append("</div>"); // gp-card
            return sb.ToString();
        }

        private string OfficeSection(string stuN, string fatN, string stuID, string cls,
            int leaveNo, string sd, string ed, int days, string leaveType, string approverName)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"gp-office\">");
            sb.Append("<div class=\"gp-office-header\">অফিস কপি</div>");
            sb.Append("<div class=\"gp-office-body\">");
            sb.Append("<div class=\"gp-info\">");
            sb.Append("<div class=\"gp-info-col\">");
            sb.Append(IR("নাম", Encode(stuN)));
            sb.Append(IR("পিতা", Encode(fatN)));
            sb.Append(IR("শ্রেণি", Encode(cls)));
            sb.Append("</div>");
            sb.Append("<div class=\"gp-info-col\">");
            sb.Append(IR("পাস নং", leaveNo.ToString()));
            sb.Append(IR("আইডি", Encode(stuID)));
            sb.Append(IR("ছুটির ধরণ", leaveType));
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("<table class=\"gp-office-table\"><tbody><tr>");
            sb.AppendFormat("<td> &nbsp; <strong>{0}</strong> হতে</td>", sd);
            sb.AppendFormat("<td> &nbsp; <strong>{0}</strong> পর্যন্ত</td>", ed);
            sb.AppendFormat("<td><strong>{0}</strong> দিন</td>", days);
            sb.Append("</tr></tbody></table>");
            sb.Append("<div class=\"gp-sign-row\">");
            sb.Append("<span class=\"sign-left\">অভিভাবকের স্বাক্ষর :</span>");
            if (!string.IsNullOrWhiteSpace(approverName))
                sb.AppendFormat("<span class=\"sign-right\"><strong>অনুমতি দাতা :</strong> {0}</span>", Encode(approverName));
            else
                sb.Append("<span class=\"sign-right\">&nbsp;</span>");
            sb.Append("</div>");
            sb.Append("</div></div>");
            return sb.ToString();
        }

        private static string IR(string label, string value)
        {
            return string.Format("<div class=\"info-row\"><span class=\"info-label\">{0}</span><span class=\"info-sep\">:</span><span class=\"info-value\">{1}</span></div>", label, value);
        }

        private static string Encode(string s)
        {
            return System.Web.HttpUtility.HtmlEncode(s ?? string.Empty);
        }
    }
}
