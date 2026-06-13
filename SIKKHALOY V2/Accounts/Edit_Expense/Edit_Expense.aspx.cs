using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Edit_Expense
{
    public partial class Edit_Expense : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                SelectedAccount();
            }
        }

        protected void AddCategoryButton_Click(object sender, EventArgs e)
        {
            CategorySQL.Insert();
            ExCategoryGridView.DataBind();
            CategoryNameTextBox.Text = string.Empty;
            FindCategoryDropDownList.DataBind();
        }

        protected void ExCategoryGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            if (e.Exception != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Category has been already Used!')", true);
                e.ExceptionHandled = true;
            }
        }

        // Handle RowCommand for ExCategoryGridView (Manage Sub-Category)
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

        // Close sub-category modal
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
                    string sql = @"SET context_info @RegID
                                   IF NOT EXISTS (SELECT * FROM Expense_SubCategory WHERE ExpenseCategoryID=@CatID AND SchoolID=@SchoolID AND SubCategoryName=@Name)
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

        // Filter sub-category dropdown when category changes in search bar
        protected void FindCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFilterSubCategories();
        }

        private void LoadFilterSubCategories()
        {
            FindSubCategoryDropDownList.Items.Clear();
            FindSubCategoryDropDownList.Items.Add(new System.Web.UI.WebControls.ListItem("[ All Sub-Category ]", "%"));

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
                        FindSubCategoryDropDownList.Items.Add(new System.Web.UI.WebControls.ListItem(dr["SubCategoryName"].ToString(), dr["ExpenseSubCategoryID"].ToString()));
                }
            }
            catch { }
        }

        // Load sub-categories in Add Expense modal when category changes
        protected void ExCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEntrySubCategories();
        }

        private void LoadEntrySubCategories()
        {
            ExSubCategoryDropDownList.Items.Clear();
            ExSubCategoryDropDownList.Items.Add(new System.Web.UI.WebControls.ListItem("[ No Sub-Category ]", ""));

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
                        ExSubCategoryDropDownList.Items.Add(new System.Web.UI.WebControls.ListItem(dr["SubCategoryName"].ToString(), dr["ExpenseSubCategoryID"].ToString()));
                }
            }
            catch { }
        }

        // Load sub-categories in GridView edit row when category changes
        protected void GridCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView gv = ExpenseGridView;
            if (gv.EditIndex < 0) return;

            GridViewRow row = gv.Rows[gv.EditIndex];
            DropDownList catDdl = row.FindControl("ExCategoryDropDownList") as DropDownList;
            DropDownList subDdl = row.FindControl("EditSubCategoryDropDownList") as DropDownList;
            if (catDdl == null || subDdl == null) return;

            subDdl.Items.Clear();
            subDdl.Items.Add(new System.Web.UI.WebControls.ListItem("[ None ]", ""));

            string catId = catDdl.SelectedValue;
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
                        subDdl.Items.Add(new System.Web.UI.WebControls.ListItem(dr["SubCategoryName"].ToString(), dr["ExpenseSubCategoryID"].ToString()));
                }
            }
            catch { }
        }

        // Load sub-categories when edit row opens
        protected void ExpenseGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow || (e.Row.RowState & DataControlRowState.Edit) == 0)
                return;

            DropDownList subDdl = e.Row.FindControl("EditSubCategoryDropDownList") as DropDownList;
            DropDownList catDdl = e.Row.FindControl("ExCategoryDropDownList") as DropDownList;
            if (subDdl == null || catDdl == null) return;

            string catId = catDdl.SelectedValue;
            object currentSubId = DataBinder.Eval(e.Row.DataItem, "ExpenseSubCategoryID");

            subDdl.Items.Clear();
            subDdl.Items.Add(new System.Web.UI.WebControls.ListItem("[ None ]", ""));

            if (string.IsNullOrEmpty(catId) || catId == "0") return;

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
                        subDdl.Items.Add(new System.Web.UI.WebControls.ListItem(dr["SubCategoryName"].ToString(), dr["ExpenseSubCategoryID"].ToString()));
                }
                if (currentSubId != null && currentSubId != DBNull.Value)
                {
                    System.Web.UI.WebControls.ListItem li = subDdl.Items.FindByValue(currentSubId.ToString());
                    if (li != null) li.Selected = true;
                }
            }
            catch { }
        }

        protected void ExpenseGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            e.Cancel = true;

            try
            {
                int expenseId = Convert.ToInt32(ExpenseGridView.DataKeys[e.RowIndex].Value);
                int registrationId = Convert.ToInt32(Session["RegistrationID"]);

                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SET context_info @RegistrationID; DELETE FROM [Expenditure] WHERE [ExpenseID] = @ExpenseID", con))
                {
                    cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                    cmd.Parameters.AddWithValue("@ExpenseID", expenseId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ExpenseGridView.DataBind();
                Total_FormView.DataBind();
            }
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage",
                    "alert('Could not delete expense. It may already be in use.')", true);
            }
        }

        // On Update: read EditSubCategoryDropDownList value and pass to SQL
        protected void ExpenseGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = ExpenseGridView.Rows[e.RowIndex];
            DropDownList subDdl = row.FindControl("EditSubCategoryDropDownList") as DropDownList;
            e.NewValues["ExpenseSubCategoryID"] = subDdl != null ? subDdl.SelectedValue : "";
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            try
            {
                ExpenseSQL.Insert();

                ExpenseDateTextBox.Text = string.Empty;
                AmountTextBox.Text = string.Empty;
                ExpenseReasonTextBox.Text = string.Empty;

                AccountDropDownList.DataBind();
                SelectedAccount();
                LoadEntrySubCategories();
                ExpenseGridView.DataBind();

                ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
            }
            catch { ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Paid Amount Greater than Account Balance')", true); }
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
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Error')", true);
            }
        }
    }
}