using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class UserAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["RegID"]))
          {
    Response.Redirect(Request.Url.AbsoluteUri + "?RegID=" + Session["RegistrationID"].ToString(), false);
           return;
            }

       if (!IsPostBack)
      {
  // Enable ViewState only where needed
    IncomeRepeater.EnableViewState = false;
          ExpenseRepeater.EnableViewState = false;
     IncomeFormView.EnableViewState = false;
          
          // Set output cache
  Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
   Response.Cache.SetExpires(DateTime.Now.AddMinutes(5));
         Response.Cache.SetValidUntilExpires(true);
   }
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            // Disable ViewState for the entire page if not needed
  this.EnableViewState = false;
        }
    }
}