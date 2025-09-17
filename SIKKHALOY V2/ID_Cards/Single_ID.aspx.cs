using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ID_Cards
{
    public partial class Single_ID : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated | Session["SchoolID"] == null)
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Login.aspx");
                }
            }
            catch { }
        }
    }
}