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
            

            if (StartDate.Text != string.Empty && EndDate.Text != string.Empty)
            {
                LeaveSQL.InsertParameters["StudentID"].DefaultValue = StudentDetailsView.DataKey["StudentID"].ToString();
                LeaveSQL.InsertParameters["StartDate"].DefaultValue = StartDate.Text.Trim();
                LeaveSQL.InsertParameters["EndDate"].DefaultValue = EndDate.Text.Trim();
                LeaveSQL.InsertParameters["Description"].DefaultValue = DescriptionTextBox.Text;
                LeaveSQL.Insert();

                StartDate.Text = string.Empty;
                EndDate.Text = string.Empty;
                DescriptionTextBox.Text = string.Empty;
                DurationHF.Value = string.Empty;

                StudentDetailsView.DataBind();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Leave record successfully inserted.')", true);
            }
        }
    }
}