using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.Expense
{
    public partial class Expense : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SelectedAccount();
            }
        }

        //add category
        protected void AddCategoryButton_Click(object sender, EventArgs e)
        {
            CategorySQL.Insert();
            ExCategoryGridView.DataBind();
            CategoryNameTextBox.Text = string.Empty;
        }

        protected void ExCategoryGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            if (e.Exception != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Category has been already Used!')", true);
                e.ExceptionHandled = true;
            }
        }

        //add expense
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            try
            {
                ExpenseSQL.Insert();

                AmountTextBox.Text = string.Empty;
                ExpenseReasonTextBox.Text = string.Empty;

                AccountDropDownList.DataBind();
                SelectedAccount();
                ExpenseGridView.DataBind();

                ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
            }
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Paid Amount Greater than Account Balance')", true);
            }
        }
        
        protected void SelectedAccount()
        {
            try
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                SqlCommand AccountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND AccountBalance <> 0 AND Default_Status = 'True'", con);
                AccountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                con.Open();
                object AccountID = AccountCmd.ExecuteScalar();
                con.Close();

                if (AccountID != null)
                    AccountDropDownList.SelectedValue = AccountID.ToString();
            }
            catch { ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('System Error')", true); }
        }
    }
}