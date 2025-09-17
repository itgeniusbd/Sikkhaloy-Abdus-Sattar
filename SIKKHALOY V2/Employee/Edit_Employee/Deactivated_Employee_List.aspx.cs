using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee.Edit_Employee
{
    public partial class Deactivated_Employee_List : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated | Session["SchoolID"] == null)
                Response.Redirect("~/Login.aspx");
        }

        protected void ActiveLinkButton_Command(object sender, CommandEventArgs e)
        {
            EmployeeSQL.UpdateParameters["EmployeeID"].DefaultValue = e.CommandName.ToString();
            EmployeeSQL.Update();
            EmployeeGridView.DataBind();
        }
    }
}