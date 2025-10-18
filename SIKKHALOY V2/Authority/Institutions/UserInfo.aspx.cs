using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class UserInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSchoolData();
            }
        }

        protected void FIndButton_Click(object sender, EventArgs e)
        {
            LoadSchoolData();
        }

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            SearchTextBox.Text = "";
            ValidationFilter.SelectedValue = "";
            PasswordFilter.SelectedValue = "";
            LoadSchoolData();
        }

        private void LoadSchoolData()
        {
            StringBuilder whereClause = new StringBuilder();
            bool hasCondition = false;

            // Text search condition
            if (!string.IsNullOrEmpty(SearchTextBox.Text.Trim()))
            {
                string searchText = SearchTextBox.Text.Trim();
                whereClause.Append("(Sch.SchoolName LIKE @SearchText OR AdminUser.UserName LIKE @SearchText OR Sch.Phone LIKE @SearchText OR CAST(Sch.SchoolID AS VARCHAR) LIKE @SearchText)");
                hasCondition = true;
            }

            // Validation filter condition
            if (!string.IsNullOrEmpty(ValidationFilter.SelectedValue))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");
                
                whereClause.Append("Sch.Validation = @ValidationStatus");
                hasCondition = true;
            }

            // Password filter condition
            if (!string.IsNullOrEmpty(PasswordFilter.SelectedValue))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");

                if (PasswordFilter.SelectedValue == "HasPassword")
                {
                    whereClause.Append("(AdminUser.Password IS NOT NULL AND AdminUser.Password != '')");
                }
                else if (PasswordFilter.SelectedValue == "NoPassword")
                {
                    whereClause.Append("(AdminUser.Password IS NULL OR AdminUser.Password = '')");
                }
                hasCondition = true;
            }

            // Build the complete SQL query with proper JOINs
            string baseQuery = @"SELECT Sch.SchoolID, Sch.SchoolName, AdminUser.UserName, AdminUser.Password, Sch.Phone, Sch.Validation, Sch.Date 
                                FROM SchoolInfo AS Sch 
                                LEFT JOIN AST AS AdminUser ON AdminUser.SchoolID = Sch.SchoolID AND AdminUser.Category = N'admin'";
            string orderBy = " ORDER BY Sch.SchoolID";
            
            string finalQuery = baseQuery;
            if (hasCondition)
            {
                finalQuery += " WHERE " + whereClause.ToString();
            }
            finalQuery += orderBy;

            // Update the SqlDataSource
            InstitutionSQL.SelectCommand = finalQuery;
            InstitutionSQL.SelectParameters.Clear();

            if (!string.IsNullOrEmpty(SearchTextBox.Text.Trim()))
            {
                InstitutionSQL.SelectParameters.Add("SearchText", TypeCode.String, "%" + SearchTextBox.Text.Trim() + "%");
            }

            if (!string.IsNullOrEmpty(ValidationFilter.SelectedValue))
            {
                InstitutionSQL.SelectParameters.Add("ValidationStatus", TypeCode.String, ValidationFilter.SelectedValue);
            }

            // Rebind the GridView
            SchoolGridView.DataBind();

            // Calculate and display summary
            CalculateAndDisplaySummary(finalQuery);
        }

        private void CalculateAndDisplaySummary(string query)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Count total results - use a simpler count query
                    string countQuery = @"SELECT COUNT(*) as TotalCount, 
                                                 SUM(CASE WHEN Sch.Validation = 'Valid' THEN 1 ELSE 0 END) as ValidCount, 
                                                 SUM(CASE WHEN Sch.Validation = 'Invalid' THEN 1 ELSE 0 END) as InvalidCount 
                                          FROM SchoolInfo AS Sch 
                                          LEFT JOIN AST AS AdminUser ON AdminUser.SchoolID = Sch.SchoolID AND AdminUser.Category = N'admin'";

                    // Add WHERE clause if conditions exist
                    if (query.Contains("WHERE"))
                    {
                        string whereSection = query.Substring(query.IndexOf("WHERE"));
                        whereSection = whereSection.Replace("ORDER BY Sch.SchoolID", "").Trim();
                        countQuery += " " + whereSection;
                    }

                    using (SqlCommand command = new SqlCommand(countQuery, connection))
                    {
                        // Add parameters if they exist
                        if (!string.IsNullOrEmpty(SearchTextBox.Text.Trim()))
                        {
                            command.Parameters.AddWithValue("@SearchText", "%" + SearchTextBox.Text.Trim() + "%");
                        }

                        if (!string.IsNullOrEmpty(ValidationFilter.SelectedValue))
                        {
                            command.Parameters.AddWithValue("@ValidationStatus", ValidationFilter.SelectedValue);
                        }

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Handle potential null values
                                int totalCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                int validCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                int invalidCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                                // Update summary labels
                                TotalCountLabel.Text = totalCount.ToString();
                                ValidCountLabel.Text = validCount.ToString();
                                InvalidCountLabel.Text = invalidCount.ToString();

                                // Show/hide summary based on search activity
                                bool hasSearch = !string.IsNullOrEmpty(SearchTextBox.Text.Trim()) || 
                                               !string.IsNullOrEmpty(ValidationFilter.SelectedValue) ||
                                               !string.IsNullOrEmpty(PasswordFilter.SelectedValue);
                                searchSummary.Visible = hasSearch || totalCount > 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                // For now, hide summary on error and show default counts
                searchSummary.Visible = false;
                TotalCountLabel.Text = "0";
                ValidCountLabel.Text = "0";
                InvalidCountLabel.Text = "0";
                
                // You can log the actual error for debugging
                System.Diagnostics.Debug.WriteLine("Error in CalculateAndDisplaySummary: " + ex.Message);
            }
        }

        protected void Ins_LinkButton_Command(object sender, CommandEventArgs e)
        {
            UserSQL.SelectParameters["SchoolID"].DefaultValue = e.CommandName.ToString();
            Institution_Label.Text = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        //Approved/unlock User
        protected void ISApprovedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GridViewRow row = (sender as CheckBox).Parent.Parent as GridViewRow;

            string userName = UserGridView.DataKeys[row.RowIndex]["UserName"].ToString();
            MembershipUser usr = Membership.GetUser(userName, false);
            usr.IsApproved = (sender as CheckBox).Checked;
            Membership.UpdateUser(usr);

            UpdateRegSQL.UpdateParameters["UserName"].DefaultValue = userName;

            if (!(sender as CheckBox).Checked)
            {
                UpdateRegSQL.UpdateParameters["Validation"].DefaultValue = "Invalid";
                UpdateRegSQL.Update();
            }
            else
            {
                UpdateRegSQL.UpdateParameters["Validation"].DefaultValue = "Valid";
                UpdateRegSQL.Update();
            }

            UserGridView.DataBind();
        }

        protected void IsLockedOutCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GridViewRow row = (sender as CheckBox).Parent.Parent as GridViewRow;

            string userName = UserGridView.DataKeys[row.RowIndex]["UserName"].ToString();
            MembershipUser usr = Membership.GetUser(userName);
            usr.UnlockUser();
            UserGridView.DataBind();
        }
    }
}