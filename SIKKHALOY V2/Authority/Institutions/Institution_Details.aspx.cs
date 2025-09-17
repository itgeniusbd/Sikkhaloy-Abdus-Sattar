using System;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class Institution_Details : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["SchoolID"]))
            {
                Response.Redirect("InstitutionList.aspx");
            }
        }

        //Approved/unlock User
        protected void ISApprovedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GridViewRow row = (sender as CheckBox).Parent.Parent as GridViewRow;

            string userName = UserGridView.DataKeys[row.RowIndex]["UserName"].ToString();
            MembershipUser usr = Membership.GetUser(userName, false);
            usr.IsApproved = (sender as CheckBox).Checked;
            Membership.UpdateUser(usr);

            UpdateRegSQL.UpdateParameters["UserName"].DefaultValue = userName;

            if (!(sender as CheckBox).Checked)
            {
                UpdateRegSQL.UpdateParameters["Validation"].DefaultValue = "Invalid";
                UpdateRegSQL.Update();
            }
            else
            {
                UpdateRegSQL.UpdateParameters["Validation"].DefaultValue = "Valid";
                UpdateRegSQL.Update();
            }

            UserGridView.DataBind();

        }

        protected void IsLockedOutCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GridViewRow row = (sender as CheckBox).Parent.Parent as GridViewRow;

            string userName = UserGridView.DataKeys[row.RowIndex]["UserName"].ToString();
            MembershipUser usr = Membership.GetUser(userName);
            usr.UnlockUser();
            UserGridView.DataBind();
        }

        protected void SMSRechargeButton_Click(object sender, EventArgs e)
        {
            SMS_SQL.Insert();
            Response.Redirect(Request.Url.AbsoluteUri);
        }

        protected void DeleteIDButton_Click(object sender, EventArgs e)
        {
            DeleteStudentSQL.Delete();
            IDerrorLabel.Text = "ID Deleted Successfully";
        }

        protected void DeleteReceiptButton_Click(object sender, EventArgs e)
        {
            MoneyRSQL.Delete();
            StudentInfoFormView.DataBind();
            ReceiptFormView.DataBind();
            PaidDetailsGridView.DataBind();
        }


        protected void SNButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in Total_StudentGridView.Rows)
            {
                TextBox SessionSNTextBox = (TextBox)row.FindControl("SessionSNTextBox");
                CheckBox IsActiveCheckbox = (CheckBox)row.FindControl("IsActiveCheckbox");

                Total_StudentSQL.UpdateParameters["IsActive"].DefaultValue = IsActiveCheckbox.Checked.ToString();
                Total_StudentSQL.UpdateParameters["SN"].DefaultValue = SessionSNTextBox.Text;
                Total_StudentSQL.UpdateParameters["EducationYearID"].DefaultValue = Total_StudentGridView.DataKeys[row.RowIndex]["EducationYearID"].ToString();

                Total_StudentSQL.Update();
            }
        }

        protected void Login_Button_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as Button).Parent.Parent as GridViewRow;

            Session["Edu_Year"] = Total_StudentGridView.DataKeys[row.DataItemIndex]["EducationYearID"].ToString();
            Session["SchoolID"] = Request.QueryString["SchoolID"];
            Session["School_Name"] = SchoolFormView.DataKey["SchoolName"].ToString();

            Response.Redirect("~/Profile/Admin.aspx");
        }

        protected void IDChangeInfoFV_ItemUpdated(object sender, FormViewUpdatedEventArgs e)
        {
            Device_DataUpdateSQL.Insert();
        }
    }
}