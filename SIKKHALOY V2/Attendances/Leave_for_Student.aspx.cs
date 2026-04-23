using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Leave_for_Student : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            StudentDetailsView.DataBind();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            TextBox StartDate = (TextBox)StudentDetailsView.FindControl("StartDateTextBox");
            TextBox EndDate = (TextBox)StudentDetailsView.FindControl("EndDateTextBox");
            TextBox DescriptionTextBox = (TextBox)StudentDetailsView.FindControl("DescriptionTextBox");
            HiddenField DurationHF = (HiddenField)StudentDetailsView.FindControl("DurationHF");
            DropDownList LeaveTypeDropDownList = (DropDownList)StudentDetailsView.FindControl("LeaveTypeDropDownList");
            TextBox GuardianNameTextBox = (TextBox)StudentDetailsView.FindControl("GuardianNameTextBox");

            if (StartDate.Text != string.Empty && EndDate.Text != string.Empty)
            {
                LeaveSQL.InsertParameters["StudentID"].DefaultValue = StudentDetailsView.DataKey["StudentID"].ToString();
                LeaveSQL.InsertParameters["StartDate"].DefaultValue = StartDate.Text.Trim();
                LeaveSQL.InsertParameters["EndDate"].DefaultValue = EndDate.Text.Trim();
                LeaveSQL.InsertParameters["Description"].DefaultValue = DescriptionTextBox.Text;
                LeaveSQL.InsertParameters["LeaveType"].DefaultValue = LeaveTypeDropDownList != null ? LeaveTypeDropDownList.SelectedValue : string.Empty;
                LeaveSQL.InsertParameters["GuardianName"].DefaultValue = GuardianNameTextBox != null ? GuardianNameTextBox.Text.Trim() : string.Empty;
                LeaveSQL.Insert();

                // Retrieve the newly inserted StudentLeaveID
                int newLeaveID = GetLastInsertedLeaveID();

                StartDate.Text = string.Empty;
                EndDate.Text = string.Empty;
                DescriptionTextBox.Text = string.Empty;
                DurationHF.Value = string.Empty;

                StudentDetailsView.DataBind();

                if (newLeaveID > 0)
                {
                    Response.Redirect("Leave_Print.aspx?lid=" + newLeaveID);
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Leave record successfully inserted.')", true);
                }
            }
        }

        private int GetLastInsertedLeaveID()
        {
            try
            {
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connStr))
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(
                    "SELECT TOP 1 StudentLeaveID FROM Attendance_Leave WHERE SchoolID = @SchoolID AND StudentID = @StudentID ORDER BY StudentLeaveID DESC", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@StudentID", StudentDetailsView.DataKey["StudentID"]);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}