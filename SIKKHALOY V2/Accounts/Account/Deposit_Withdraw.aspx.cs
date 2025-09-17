using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Account
{
    public partial class Deposit_Withdraw : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddAccountButton_Click(object sender, EventArgs e)
        {
            AccountNameSQL.Insert();
            AccountNameGridView.DataBind();
            ErrLabel.Text = string.Empty;
            AccountNameTextBox.Text = string.Empty;
        }
        protected void DStatusCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            int selRowIndex = ((GridViewRow)(((CheckBox)sender).Parent.Parent)).RowIndex;

            DefaultAccountSQL.UpdateParameters["AccountID"].DefaultValue = AccountNameGridView.DataKeys[selRowIndex]["AccountID"].ToString();
            DefaultAccountSQL.Update();
            AccountNameGridView.DataBind();
        }
        protected void AccountNameSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ErrLabel.Text = e.Command.Parameters["@ERROR"].Value.ToString();
        }

        protected void DepositButton_Click(object sender, EventArgs e)
        {
            try
            {
                DELabel.Text = "";

                DepositSQL.Insert();
                AccountIN_AmountTextBox.Text = string.Empty;
                IN_DetailsTextBox.Text = string.Empty;
                Deposit_Date_TextBox.Text = string.Empty;

                AccountNameGridView.DataBind();
                ABFormView.DataBind();
            }
            catch { DELabel.Text = "Something Went Wrong!"; }
        }
        protected void WithdrawButton_Click(object sender, EventArgs e)
        {
            if (Account_Balance())
            {
                WELabel.Text = "";
                WithdrawSQL.Insert();
                AccountOUT_AmountTextBox.Text = string.Empty;
                Out_DetailsTextBox.Text = string.Empty;
                Withdraw_Date_TextBox.Text = string.Empty;

                AccountNameGridView.DataBind();
                ABFormView.DataBind();
            }
            else { WELabel.Text = "Withdraw Amount Greater Than Current Balance"; }
        }

        private bool Account_Balance()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            SqlCommand AccountBalance_Cmd = new SqlCommand("Select AccountBalance from Account where SchoolID = @SchoolID AND AccountID = @AccountID", con);
            AccountBalance_Cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            AccountBalance_Cmd.Parameters.AddWithValue("@AccountID", AccountDropDownList.SelectedValue);

            con.Open();
            object AccountBalance = AccountBalance_Cmd.ExecuteScalar();
            con.Close();

            if (AccountBalance != null)
            {
                double Paid_Amount = Convert.ToDouble(AccountOUT_AmountTextBox.Text.Trim());
                double Balance = Convert.ToDouble(AccountBalance);

                if (Paid_Amount > Balance)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        protected void AccountDropDownList_DataBound(object sender, EventArgs e)
        {
            AccountDropDownList.Items.Insert(0, new ListItem("Select Account", "0"));
        }
    }
}