using System;
using System.Web.UI;

namespace EDUCATION.COM.Committee
{
    public partial class Donor_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["RegistrationID"] == null)
            {
                Response.Redirect("~/Default.aspx");
            }
        }
    }
}
