using System;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class Change_Password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["RegistrationID"] == null)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void ChangePassword1_ChangedPassword(object sender, EventArgs e)
        {
            TextBox newPassword = (TextBox)ChangePassword.ChangePasswordTemplateContainer.FindControl("NewPassword");
            LITQL.UpdateParameters["Password"].DefaultValue = newPassword.Text;
            LITQL.Update();
        }
    }
}
