using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.ReportsSession
{
    public partial class SessionPayorder : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["EducationYearID"]))
                {
                    Session_DropDownList.SelectedValue = Request.QueryString["EducationYearID"];
                }
                else
                {
                    if (Session["Edu_Year"] != null)
                    Session_DropDownList.SelectedValue = Session["Edu_Year"].ToString();
                }
            }
        }
        protected void Class_LB_Command(object sender, CommandEventArgs e)
        {
            Class_Name_Label.Text = e.CommandArgument.ToString();
        }
        protected void Role_LB_Command(object sender, CommandEventArgs e)
        {
            Role_Label.Text = e.CommandArgument.ToString();
        }



        //Link page
        protected void Student_LB_Click(object sender, EventArgs e)
        {
            Response.Redirect("Session_Stu_Report.aspx" + QueryString());
        }

        protected void Concession_LB_Click(object sender, EventArgs e)
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


      
        protected void Class_Command(object sender, CommandEventArgs e)
        {
            Response.Redirect(e.CommandName + QueryString() + "&Class=" + e.CommandArgument);
        }

        protected void Role_Command(object sender, CommandEventArgs e)
        {
            Response.Redirect(e.CommandName + QueryString() + "&Class=" + ClassGridView.SelectedValue + "&Role=" + e.CommandArgument);
        }

        private string QueryString()
        {
            string Q = "";

            Q += "?Year=" + Session_DropDownList.SelectedValue;

            if (From_Date_TextBox.Text.Trim() != "")
            {
                Q += "&From=" + From_Date_TextBox.Text.Trim();
            }

            if (To_Date_TextBox.Text.Trim() != "")
            {
                Q += "&To=" + To_Date_TextBox.Text.Trim();
            }

            return Q;
        }

    }
}