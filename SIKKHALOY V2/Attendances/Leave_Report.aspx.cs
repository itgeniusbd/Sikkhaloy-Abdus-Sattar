using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Leave_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Default: current month start to today
                FromDateTextBox.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("d MMM yyyy");
                ToDateTextBox.Text   = DateTime.Now.ToString("d MMM yyyy");
                LoadLeaveData();
            }
        }

        protected void FilterButton_Click(object sender, EventArgs e)
        {
            LoadLeaveData();
        }

        private void LoadLeaveData()
        {
            string type     = TypeDropDownList.SelectedValue;
            string fromDate = FromDateTextBox.Text.Trim();
            string toDate   = ToDateTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fromDate) || string.IsNullOrWhiteSpace(toDate))
            {
                NoSearchPanel.Visible = true;
                ResultPanel.Visible   = false;
                return;
            }

            int schoolId = 0;
            if (Session["SchoolID"] == null || !int.TryParse(Session["SchoolID"].ToString(), out schoolId))
                return;

            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            DataTable dt   = new DataTable();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd;

                if (type == "Student")
                {
                    // Student leave from Attendance_Leave table
                    string sql = @"
                        SELECT
                            ROW_NUMBER() OVER (ORDER BY al.StartDate DESC) AS SL,
                            al.StudentLeaveID                      AS LeaveID,
                            'Student'                              AS [Type],
                            s.ID,
                            s.StudentsName                         AS [Name],
                            ISNULL(cc.Class, '')                   AS ClassName,
                            ISNULL(al.LeaveType, '')               AS LeaveType,
                            al.StartDate,
                            al.EndDate,
                            DATEDIFF(DAY, al.StartDate, al.EndDate) + 1 AS [Days],
                            ISNULL(al.GuardianName, '')            AS GuardianName,
                            ISNULL(al.Description, '')             AS Description
                        FROM Attendance_Leave al
                        INNER JOIN Student s ON al.StudentID = s.StudentID
                        LEFT JOIN StudentsClass sc ON s.StudentID = sc.StudentID
                            AND sc.SchoolID = @SchoolID
                            AND sc.EducationYearID = @EduYear
                        LEFT JOIN CreateClass cc ON sc.ClassID = cc.ClassID
                        WHERE al.SchoolID = @SchoolID
                          AND CAST(al.StartDate AS DATE) >= CAST(@FromDate AS DATE)
                          AND CAST(al.StartDate AS DATE) <= CAST(@ToDate AS DATE)
                        ORDER BY al.StartDate DESC";

                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@SchoolID",  schoolId);
                    cmd.Parameters.AddWithValue("@EduYear",   Session["Edu_Year"] ?? 0);
                    cmd.Parameters.AddWithValue("@FromDate",  fromDate);
                    cmd.Parameters.AddWithValue("@ToDate",    toDate);
                }
                else
                {
                    // Teacher leave — tries Teacher_Leave table; shows empty set if table doesn't exist
                    string sql = @"
                        IF OBJECT_ID(N'dbo.Teacher_Leave', N'U') IS NOT NULL
                        BEGIN
                            SELECT
                                ROW_NUMBER() OVER (ORDER BY tl.StartDate DESC) AS SL,
                                'Teacher'                              AS [Type],
                                ISNULL(t.TeacherID, 0)                 AS ID,
                                ISNULL(t.Name, '')                     AS [Name],
                                ISNULL(t.Designation, '')              AS ClassName,
                                ISNULL(tl.LeaveType, '')               AS LeaveType,
                                tl.StartDate,
                                tl.EndDate,
                                DATEDIFF(DAY, tl.StartDate, tl.EndDate) + 1 AS [Days],
                                ''                                     AS GuardianName,
                                ISNULL(tl.Description, '')             AS Description
                            FROM Teacher_Leave tl
                            INNER JOIN Teacher t ON tl.TeacherID = t.TeacherID
                            WHERE tl.SchoolID = @SchoolID
                              AND CAST(tl.StartDate AS DATE) >= CAST(@FromDate AS DATE)
                              AND CAST(tl.StartDate AS DATE) <= CAST(@ToDate AS DATE)
                            ORDER BY tl.StartDate DESC
                        END
                        ELSE
                        BEGIN
                            SELECT 0 AS SL, '' AS [Type], '' AS ID, '' AS [Name],
                                   '' AS ClassName, '' AS LeaveType,
                                   CAST(GETDATE() AS DATE) AS StartDate,
                                   CAST(GETDATE() AS DATE) AS EndDate,
                                   0 AS [Days], '' AS GuardianName, '' AS Description
                            WHERE 1 = 0
                        END";

                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@SchoolID",  schoolId);
                    cmd.Parameters.AddWithValue("@FromDate",  fromDate);
                    cmd.Parameters.AddWithValue("@ToDate",    toDate);
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            LeaveGridView.DataSource = dt;
            LeaveGridView.PageIndex  = 0;
            LeaveGridView.DataBind();

            TotalLabel.Text  = dt.Rows.Count.ToString();
            summaryText.InnerText = string.Format(" | {0} থেকে {1}", fromDate, toDate);

            NoSearchPanel.Visible = false;
            ResultPanel.Visible   = true;
        }

        protected void LeaveGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            LeaveGridView.PageIndex = e.NewPageIndex;
            LoadLeaveData();
        }

        protected void ActionButton_Click(object sender, EventArgs e)
        {
            string action  = hfAction.Value;
            string leaveID = hfLeaveID.Value;
            if (string.IsNullOrEmpty(leaveID)) return;

            int schoolId = 0;
            if (Session["SchoolID"] == null || !int.TryParse(Session["SchoolID"].ToString(), out schoolId))
                return;

            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                if (action == "Delete")
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Attendance_Leave WHERE StudentLeaveID = @ID AND SchoolID = @SchoolID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ID",       leaveID);
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else if (action == "Edit")
                {
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Attendance_Leave SET
                            StartDate    = @StartDate,
                            EndDate      = @EndDate,
                            LeaveType    = @LeaveType,
                            GuardianName = @GuardianName,
                            Description  = @Description
                          WHERE StudentLeaveID = @ID AND SchoolID = @SchoolID", conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate",    hfStartDate.Value);
                        cmd.Parameters.AddWithValue("@EndDate",      hfEndDate.Value);
                        cmd.Parameters.AddWithValue("@LeaveType",    hfLeaveType.Value);
                        cmd.Parameters.AddWithValue("@GuardianName", hfGuardianName.Value);
                        cmd.Parameters.AddWithValue("@Description",  hfDescription.Value);
                        cmd.Parameters.AddWithValue("@ID",           leaveID);
                        cmd.Parameters.AddWithValue("@SchoolID",     schoolId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            LoadLeaveData();
        }
    }
}
