using System;

namespace EDUCATION.COM.Committee
{
    public partial class Donor_Payment_History : System.Web.UI.Page
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
