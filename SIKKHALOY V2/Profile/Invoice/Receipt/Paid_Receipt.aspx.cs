using System;

namespace EDUCATION.COM.Profile.Invoice
{
    public partial class Paid_Invoice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["SID"]))
            {
                Response.Redirect("Invoice_List.aspx");
            }
        }
    }
}