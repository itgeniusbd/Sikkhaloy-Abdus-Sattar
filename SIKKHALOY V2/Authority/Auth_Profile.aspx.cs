using System;

namespace EDUCATION.COM.Authority
{
    public partial class Auth_Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            NoticeSQL.Insert();
        }
    }
}