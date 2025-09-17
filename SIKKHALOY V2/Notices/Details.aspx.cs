using System;

namespace EDUCATION.COM.Notices
{
    public partial class Details : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx");
            }
        }
    }
}