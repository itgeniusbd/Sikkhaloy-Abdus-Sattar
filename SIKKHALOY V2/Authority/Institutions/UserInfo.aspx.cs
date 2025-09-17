using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class UserInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Ins_LinkButton_Command(object sender, CommandEventArgs e)
        {
            UserSQL.SelectParameters["SchoolID"].DefaultValue = e.CommandName.ToString();
            Institution_Label.Text = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
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
    }
}