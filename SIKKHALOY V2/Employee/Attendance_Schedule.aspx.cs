using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Attendance_Schedule : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool IsAssign = false;
            foreach (GridViewRow row in EmployeeGridView.Rows)
            {
                var Emp_AddCheckBox = row.FindControl("AddCheckBox") as CheckBox;
                var Emp_LateCheckBox = row.FindControl("LateCheckBox") as CheckBox;
                var Emp_AbsCheckBox = row.FindControl("AbsCheckBox") as CheckBox;
                var RFIDTextBox = row.FindControl("RFIDTextBox") as TextBox;

                Schedule_AssignSQL.DeleteParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                Schedule_AssignSQL.Delete();

                if (Emp_AddCheckBox.Checked)
                {
                    Schedule_AssignSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    Schedule_AssignSQL.InsertParameters["Is_Abs_SMS"].DefaultValue = Emp_AbsCheckBox.Checked.ToString();
                    Schedule_AssignSQL.InsertParameters["Is_Late_SMS"].DefaultValue = Emp_LateCheckBox.Checked.ToString();
                    Schedule_AssignSQL.Insert();
                    IsAssign = true;
                }

                EmployeeSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                EmployeeSQL.UpdateParameters["RFID"].DefaultValue = RFIDTextBox.Text.Trim();
                EmployeeSQL.Update();
                IsAssign = true;
            }

            if (IsAssign)
            {
                Device_DataUpdateSQL.Insert();
                EmployeeGridView.DataBind();

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Inputted Successfully!!')", true);
            }
        }
    }
}