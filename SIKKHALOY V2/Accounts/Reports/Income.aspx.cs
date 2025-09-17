using System;
using System.Web.UI;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class Income : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

                if (string.IsNullOrEmpty(Request.QueryString["Category"]) && string.IsNullOrEmpty(Request.QueryString["All"]))
                {
                    From_Date_TextBox.Text = firstDayOfMonth.ToString("d MMM yyyy");
                    To_Date_TextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                }

                if (Request.QueryString["Category"] != "")
                {
                    CategoryDropDownList.SelectedValue = Request.QueryString["Category"];
                }

                if (!string.IsNullOrEmpty(Request.QueryString["f"]))
                {
                    From_Date_TextBox.Text = Request.QueryString["f"];
                    To_Date_TextBox.Text = Request.QueryString["t"];
                }
               
            }
        }

    }
}