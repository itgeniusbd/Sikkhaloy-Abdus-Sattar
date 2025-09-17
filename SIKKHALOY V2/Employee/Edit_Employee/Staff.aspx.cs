using System;

namespace EDUCATION.COM.Employee.Edit_Employee
{
    public partial class Staff : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated | Session["SchoolID"] == null)
            {
                Response.Redirect("~/Login.aspx?Invalid=Timeout");
            }

            if (string.IsNullOrEmpty(Request.QueryString["Emp"]))
            {
                Response.Redirect("~/Employee/Staff.aspx");
            }
        }

        protected void DeactivateButton_Click(object sender, EventArgs e)
        {
            DeactivateEmpSQL.Update();
            Device_DataUpdateSQL.Insert();
            Response.Redirect("Deactivated_Employee_List.aspx");
        }
       
    }
}