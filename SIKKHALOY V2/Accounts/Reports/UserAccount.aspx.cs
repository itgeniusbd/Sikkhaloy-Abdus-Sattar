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
        }
    }
}