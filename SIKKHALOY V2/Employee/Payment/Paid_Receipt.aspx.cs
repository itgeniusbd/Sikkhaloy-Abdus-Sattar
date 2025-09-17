using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee.Payment
{
    public partial class Paid_Receipt : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
                Response.Redirect("~/Login.aspx");

            if (string.IsNullOrEmpty(Request.QueryString["EmployeeID"]))
            {
                Response.Redirect("Salary_Payment.aspx");
            }
        }
    }
}