using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Change_Payorder_Date : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool isChange = false;
            foreach (GridViewRow row in AssignedGridView.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    TextBox StartDateTextBox = (TextBox)AssignedGridView.Rows[row.RowIndex].FindControl("EStDateTextBox");
                    TextBox EndDateTextBox = (TextBox)AssignedGridView.Rows[row.RowIndex].FindControl("EEdateTextBox");
                    CheckBox SingleCheckBox = (CheckBox)AssignedGridView.Rows[row.RowIndex].FindControl("SingleCheckBox");

                    if (SingleCheckBox.Checked)
                    {
                        AssignedRoleSQL.UpdateParameters["StartDate"].DefaultValue = StartDateTextBox.Text.Trim();
                        AssignedRoleSQL.UpdateParameters["EndDate"].DefaultValue = EndDateTextBox.Text.Trim();
                        AssignedRoleSQL.UpdateParameters["PayFor"].DefaultValue = AssignedGridView.DataKeys[row.DataItemIndex]["PayFor"].ToString();
                        AssignedRoleSQL.UpdateParameters["AssignRoleID"].DefaultValue = AssignedGridView.DataKeys[row.DataItemIndex]["AssignRoleID"].ToString();
                        AssignedRoleSQL.Update();
                        isChange = true;
                    }
                }
            }

            if (isChange)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
            }
        }
    }
}