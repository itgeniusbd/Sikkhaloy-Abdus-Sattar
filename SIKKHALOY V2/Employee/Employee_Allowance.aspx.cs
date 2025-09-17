using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Allowance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddAllowanceButton_Click(object sender, EventArgs e)
        {
            AllowanceNameSQL.Insert();
            AllowanceNameTextBox.Text = string.Empty;
            AllownceNameGridView.DataBind();
        }

        protected void AssignButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in EmployeeGridView.Rows)
            {
                CheckBox AddCheckBox = row.FindControl("AddCheckBox") as CheckBox;
                TextBox AllowanceAmountTextBox = row.FindControl("AllowanceAmountTextBox") as TextBox;
                RadioButtonList Fixed_PercetageRBList = row.FindControl("Fixed_PercetageRadioButtonList") as RadioButtonList;

                if (AddCheckBox.Checked)
                {
                    Allowance_AssignSQl.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    Allowance_AssignSQl.InsertParameters["AllowanceAmount"].DefaultValue = AllowanceAmountTextBox.Text;
                    Allowance_AssignSQl.InsertParameters["Fixed_Percetage"].DefaultValue = Fixed_PercetageRBList.SelectedValue;
                    Allowance_AssignSQl.Insert();
                }
                else
                {
                    Allowance_AssignSQl.DeleteParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    Allowance_AssignSQl.Delete();
                }
            }
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Saved')", true);
        }

        protected void AllowanceDropDownList_DataBound(object sender, EventArgs e)
        {
            AllowanceDropDownList.Items.Insert(0, new ListItem("[ SELECT ALLOWANCE ]", "0"));
        }
    }
}