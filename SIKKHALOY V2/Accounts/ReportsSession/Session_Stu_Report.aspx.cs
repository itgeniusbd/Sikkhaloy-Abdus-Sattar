using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.ReportsSession
{
    public partial class Session_Stu_Report : System.Web.UI.Page
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

        private string QueryString()
        {
            string Q = "";

            Q += "?Year=" + Session_DropDownList.SelectedValue;

            if (FromDateTextBox.Text.Trim() != "")
            {
                Q += "&From=" + FromDateTextBox.Text.Trim();
            }

            if (ToDateTextBox.Text.Trim() != "")
            {
                Q += "&To=" + ToDateTextBox.Text.Trim();
            }

            if (ClassDropDownList.SelectedValue != "0")
            {
                Q += "&Class=" + ClassDropDownList.SelectedValue;
            }

            if (RoleDropDownList.SelectedValue != "%")
            {
                Q += "&Role=" + RoleDropDownList.SelectedValue;
            }

            return Q;
        }

        protected void ConcessionLinkButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("Concession_Report.aspx" + QueryString());
        }
        protected void Paid_LB_Click(object sender, EventArgs e)
        {
            Response.Redirect("Session_Paid_Report.aspx" + QueryString());
        }

        protected void Unpaid_LB_Click(object sender, EventArgs e)
        {
            Response.Redirect("Session_Due_Report.aspx" + QueryString());
        }


    }
}