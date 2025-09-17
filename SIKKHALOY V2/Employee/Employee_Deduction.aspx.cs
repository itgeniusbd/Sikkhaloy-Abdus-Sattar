using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Deduction : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddDeductionButton_Click(object sender, EventArgs e)
        {
            DeductionNameSQL.Insert();
            DeductionNameTextBox.Text = string.Empty;
            AllownceNameGridView.DataBind();
        }

        protected void AssignButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in EmployeeGridView.Rows)
            {
                CheckBox AddCheckBox = row.FindControl("AddCheckBox") as CheckBox;
                TextBox DeductionAmountTextBox = row.FindControl("DeductionAmountTextBox") as TextBox;
                RadioButtonList Fixed_PercetageRBList = row.FindControl("Fixed_PercetageRadioButtonList") as RadioButtonList;

                if (AddCheckBox.Checked)
                {
                    Deduction_AssignSQl.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    Deduction_AssignSQl.InsertParameters["DeductionAmount"].DefaultValue = DeductionAmountTextBox.Text.Trim();
                    Deduction_AssignSQl.InsertParameters["Fixed_Percetage"].DefaultValue = Fixed_PercetageRBList.SelectedValue;
                    Deduction_AssignSQl.Insert();
                }
                else
                {
                    Deduction_AssignSQl.DeleteParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    Deduction_AssignSQl.Delete();
                }
            }
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Saved')", true);
        }


        protected void DeductionDropDownList_DataBound(object sender, EventArgs e)
        {
            DeductionDropDownList.Items.Insert(0, new ListItem("[ SELECT DEDUCTION ]", "0"));
        }
    }
}