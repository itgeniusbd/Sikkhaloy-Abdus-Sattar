using System;
using System.Web.UI;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class AccountDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DateTime now = DateTime.Now;
                From_Date_TextBox.Text = new DateTime(now.Year, now.Month, 1).ToString("d MMM yyyy");
                To_Date_TextBox.Text = now.ToString("d MMM yyyy");

                if (!string.IsNullOrEmpty(Request.QueryString["acc"]))
                    AccountDropDownList.SelectedValue = Request.QueryString["acc"];

            }
        }
    }
}