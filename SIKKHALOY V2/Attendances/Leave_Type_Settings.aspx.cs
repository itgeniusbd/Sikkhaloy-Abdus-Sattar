using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using Education;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Leave_Type_Settings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindGrid();
        }

        protected void AddButton_Click(object sender, EventArgs e)
        {
            int schoolId = GetSchoolId();
            if (schoolId <= 0) return;

            string leaveTypeName = LeaveTypeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(leaveTypeName))
            {
                MessageLabel.Text = "ছুটির ধরণ লিখুন।";
                return;
            }

            var helper = new LeaveType_Helper();
            if (helper.AddLeaveType(schoolId, leaveTypeName, 0))
            {
                LeaveTypeTextBox.Text = string.Empty;
                MessageLabel.Text = string.Empty;
                BindGrid();
            }
            else
            {
                MessageLabel.Text = "ছুটির ধরণ যোগ করা যায়নি। হয়তো আগে থেকেই আছে বা টেবিল তৈরি হয়নি।";
            }
        }

        protected void LeaveTypeGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int schoolId = GetSchoolId();
            if (schoolId <= 0) return;

            int leaveTypeId = Convert.ToInt32(LeaveTypeGridView.DataKeys[e.RowIndex].Value);
            var helper = new LeaveType_Helper();
            helper.DeleteLeaveType(schoolId, leaveTypeId);
            BindGrid();
        }

        private void BindGrid()
        {
            int schoolId = GetSchoolId();
            if (schoolId <= 0) return;

            DataTable dt = new DataTable();
            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT LeaveTypeID, LeaveTypeName, SortOrder
                      FROM Attendance_Leave_Type
                      WHERE SchoolID = @SchoolID AND IsActive = 1
                      ORDER BY SortOrder, LeaveTypeName", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);
                }
            }
            catch { }

            LeaveTypeGridView.DataSource = dt;
            LeaveTypeGridView.DataBind();
        }

        private int GetSchoolId()
        {
            int schoolId;
            return Session["SchoolID"] != null && int.TryParse(Session["SchoolID"].ToString(), out schoolId)
                ? schoolId
                : 0;
        }
    }
}
