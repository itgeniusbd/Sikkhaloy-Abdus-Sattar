using System;

namespace EDUCATION.COM.Profile.Support
{
    public partial class Support_Ticket : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            SupportSQL.Insert();
            Response.Redirect(Request.Url.AbsoluteUri);
        }
    }
}