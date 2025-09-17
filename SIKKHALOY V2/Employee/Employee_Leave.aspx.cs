using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Leave : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool isLeave = false;
            foreach (GridViewRow row in EmployeeGridView.Rows)
            {
                CheckBox Emp_AddCheckBox = row.FindControl("AddCheckBox") as CheckBox;
                if (Emp_AddCheckBox.Checked)
                {
                    EmployyeLeaveSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    EmployyeLeaveSQL.Insert();
                    isLeave = true;
                }
            }

            if(isLeave)
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Leave Inputted Successfully!!')", true);
        }
    }
}