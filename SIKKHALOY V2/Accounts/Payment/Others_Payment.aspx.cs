using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Others_Payment : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Amount();

                SqlCommand AccountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND Default_Status = 'True'", con);
                AccountCmd.Parameters.AddWithValue("@SchoolID",Session["SchoolID"].ToString());
              
                con.Open();
                object AccountID = AccountCmd.ExecuteScalar();
                con.Close();

                if (AccountID != null)
                    AccountDropDownList.SelectedValue = AccountID.ToString();
            }

        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            NewCategorySQL.Insert();
            NewCategoryNameTextBox.Text = string.Empty;
            AllCategory.DataBind();
            CategoryDropDownList.DataBind();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            ExtraIncomeSQL.Insert();
            ExtraIncomeGridView.DataBind();
            Amount();
            AmountTextBox.Text = string.Empty;
            IncomeForTextBox.Text = string.Empty;

            ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
        }

        protected void Amount()
        {
            try
            {
                DataView dv = (DataView)ViewIncomeSQL.Select(DataSourceSelectArguments.Empty);
                double reorderedProducts = (double)dv.Table.Rows[0][0];
                if (reorderedProducts > 0)
                {
                    AmountLabel.Text = "Total : " + reorderedProducts + " Tk";
                }
                else
                {
                    AmountLabel.Text = "No Income Added";
                }
            }
            catch { Response.Redirect("~/Login.aspx"); }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            Amount();
            ExtraIncomeGridView.DataBind();
        }

        protected void CategoryDropDownList_DataBound(object sender, EventArgs e)
        {
            CategoryDropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }
    }
}