using System;

namespace EDUCATION.COM
{
    public partial class Basic_Donor : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["RegistrationID"] == null || Session["SchoolID"] == null)
            {
                Response.Redirect("~/Default.aspx");
            }
        }
    }
}
