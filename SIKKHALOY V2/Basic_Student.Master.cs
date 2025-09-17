using System;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM
{
    public partial class Basic_Student : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated || Session["SchoolID"] == null)
            {
                string[] myCookies = Request.Cookies.AllKeys;
                foreach (string cookie in myCookies)
                {
                    Response.Cookies[cookie].Expires = DateTime.Now;
                }

                Roles.DeleteCookie();
                Session.Clear();
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
            }

            if (!this.IsPostBack)
            {
                if (Session["Edu_Year"] != null)
                {
                    Session_DropDownList.SelectedValue = Session["Edu_Year"].ToString();
                }
            }
        }

        protected void LoginStatus1_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now;
            }
            Roles.DeleteCookie();
            Session.Clear();
            FormsAuthentication.SignOut();
            Session.Abandon();
        }
    }
}