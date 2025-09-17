using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.ReportsSession
{
    public partial class Session_Paid_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["Year"]))
                {
                    Session_DropDownList.SelectedValue = Request.QueryString["Year"];
                }
                else
                {
                    if (Session["Edu_Year"] != null)
                        Session_DropDownList.SelectedValue = Session["Edu_Year"].ToString();
                }


                if (!string.IsNullOrEmpty(Request.QueryString["From"]))
                {
                    FromDateTextBox.Text = Request.QueryString["From"];
                }

                if (!string.IsNullOrEmpty(Request.QueryString["To"]))
                {
                    ToDateTextBox.Text = Request.QueryString["To"];
                }
            }
        }

        protected void ClassDropDownList_DataBound(object sender, EventArgs e)
        {
            ClassDropDownList.Items.Insert(0, new ListItem("[ ALL CLASS ]", "0"));
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["Class"]))
                {
                    ClassDropDownList.SelectedValue = Request.QueryString["Class"];
                }
            }
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }

        protected void RoleDropDownList_DataBound(object sender, EventArgs e)
        {
            RoleDropDownList.Items.Insert(0, new ListItem("[ ALL ROLE ]", "%"));
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["Role"]))
                {
                    RoleDropDownList.SelectedValue = Request.QueryString["Role"];
                }
            }
        }

        protected void PayForDropDownList_DataBound(object sender, EventArgs e)
        {
            PayForDropDownList.Items.Insert(0, new ListItem("[ PAY FOR ]", "%"));
        }
    }
}