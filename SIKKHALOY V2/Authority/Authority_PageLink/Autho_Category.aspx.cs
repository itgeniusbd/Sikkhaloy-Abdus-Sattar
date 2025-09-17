using System;

namespace EDUCATION.COM.Authority.Authority_PageLink
{
    public partial class Autho_Category : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            CategorySQL.Insert();
        }
    }
}