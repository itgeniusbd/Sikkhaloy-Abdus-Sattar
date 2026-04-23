using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class UserAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["RegID"]))
                Response.Redirect(Request.Url.AbsoluteUri + "?RegID=" + Session["RegistrationID"].ToString());

            if (!IsPostBack)
            {
                // Default: current month
                DateTime today = DateTime.Today;
                From_Date_TextBox.Text = new DateTime(today.Year, today.Month, 1).ToString("dd/MM/yyyy");
                To_Date_TextBox.Text = today.ToString("dd/MM/yyyy");
            }
        }
    }
}