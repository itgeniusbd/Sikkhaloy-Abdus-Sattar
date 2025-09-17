using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Change_Password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void ChangePassword1_ChangedPassword(object sender, EventArgs e)
        {
            LITQL.UpdateParameters["Password"].DefaultValue = ChangePassword.NewPassword;
            LITQL.Update();
        }
    }
}