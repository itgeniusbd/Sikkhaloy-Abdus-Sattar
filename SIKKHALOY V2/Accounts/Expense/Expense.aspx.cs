using System;
using System.Configuration;
using System.Data;
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

        // Handle RowCommand for ExCategoryGridView (including Manage Sub-Category)
        protected void ExCategoryGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ManageSub")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                if (parts.Length >= 2)
                {
                    SelectedCategoryIDHidden.Value = parts[0];
                    SubCatTitleLabel.Text = parts[1];
                    SubCategoryGridView.DataBind();
                    SubPanelOpenFlag.Value = "1";
                }
            }
        }

        // Close sub-category panel
        protected void CloseSubPanelBtn_Click(object sender, EventArgs e)
        {
            SubCategoryNameTextBox.Text = string.Empty;
            SubPanelOpenFlag.Value = "2";
        }

        // Add a new sub-category
        protected void AddSubCategoryButton_Click(object sender, EventArgs e)
        {
            string subCatName = SubCategoryNameTextBox.Text.Trim();
            string categoryId = SelectedCategoryIDHidden.Value;
            if (string.IsNullOrEmpty(subCatName) || string.IsNullOrEmpty(categoryId)) return;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    string sql = @"IF NOT EXISTS (SELECT * FROM Expense_SubCategory WHERE ExpenseCategoryID=@CatID AND SchoolID=@SchoolID AND SubCategoryName=@Name)
                                   INSERT INTO Expense_SubCategory(ExpenseCategoryID, SubCategoryName, SchoolID, RegistrationID)
                                   VALUES(@CatID, LTRIM(RTRIM(@Name)), @SchoolID, @RegID)";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@CatID", int.Parse(categoryId));
                    cmd.Parameters.AddWithValue("@Name", subCatName);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@RegID", Session["RegistrationID"].ToString());
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                SubCategoryNameTextBox.Text = string.Empty;
                SubCategoryGridView.DataBind();
                SubPanelOpenFlag.Value = "1";
            }
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Error adding sub-category')", true);
            }
        }

        protected void SubCategoryGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            if (e.Exception != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Sub-Category has been already Used!')", true);
                e.ExceptionHandled = true;
            }
        }

        // When category changes in the filter area, reload sub-category filter dropdown
        protected void FindCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFilterSubCategories();
        }

        private void LoadFilterSubCategories()
        {
            FindSubCategoryDropDownList.Items.Clear();
            FindSubCategoryDropDownList.Items.Add(new ListItem("[ All Sub-Category ]", "%"));

            string catId = FindCategoryDropDownList.SelectedValue;
            if (catId == "%") return;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(
                        "SELECT ExpenseSubCategoryID, SubCategoryName FROM Expense_SubCategory WHERE ExpenseCategoryID=@CatID AND SchoolID=@SchoolID ORDER BY ExpenseSubCategoryID", con);
                    cmd.Parameters.AddWithValue("@CatID", catId);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                        FindSubCategoryDropDownList.Items.Add(new ListItem(dr["SubCategoryName"].ToString(), dr["ExpenseSubCategoryID"].ToString()));
                }
            }
            catch { }
        }

        // When category changes in Add Expense modal, reload sub-category dropdown
        protected void ExCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEntrySubCategories();
        }

        private void LoadEntrySubCategories()
        {
            ExSubCategoryDropDownList.Items.Clear();
            ExSubCategoryDropDownList.Items.Add(new ListItem("[ No Sub-Category ]", ""));

            string catId = ExCategoryDropDownList.SelectedValue;
            if (catId == "0") return;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(
                        "SELECT ExpenseSubCategoryID, SubCategoryName FROM Expense_SubCategory WHERE ExpenseCategoryID=@CatID AND SchoolID=@SchoolID ORDER BY ExpenseSubCategoryID", con);
                    cmd.Parameters.AddWithValue("@CatID", catId);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                        ExSubCategoryDropDownList.Items.Add(new ListItem(dr["SubCategoryName"].ToString(), dr["ExpenseSubCategoryID"].ToString()));
                }
            }
            catch { }
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
                LoadEntrySubCategories();
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
