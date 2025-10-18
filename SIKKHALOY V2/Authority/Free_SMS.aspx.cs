using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority
{
    public partial class Free_SMS : System.Web.UI.Page
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
            PaymentActiveFilter.SelectedValue = "";
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
                whereClause.Append("(SchoolName LIKE @SearchText OR UserName LIKE @SearchText OR Phone LIKE @SearchText OR CAST(SchoolID AS VARCHAR) LIKE @SearchText)");
                hasCondition = true;
            }

            // Validation filter condition
            if (!string.IsNullOrEmpty(ValidationFilter.SelectedValue))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");
                
                whereClause.Append("Validation = @ValidationStatus");
                hasCondition = true;
            }

            // Payment Active filter condition
            if (!string.IsNullOrEmpty(PaymentActiveFilter.SelectedValue))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");

                if (PaymentActiveFilter.SelectedValue == "Active")
                {
                    whereClause.Append("IS_ServiceChargeActive = 1");
                }
                else if (PaymentActiveFilter.SelectedValue == "Inactive")
                {
                    whereClause.Append("IS_ServiceChargeActive = 0");
                }
                hasCondition = true;
            }

            // Build the complete SQL query
            string baseQuery = "SELECT Per_Student_Rate, School_SN, SchoolID, SchoolName, Date, Address, Phone, Free_SMS, Fixed, Discount, IS_ServiceChargeActive, CAST(CASE WHEN Validation = 'Valid' THEN 1 ELSE 0 END AS BIT) AS Validation, UserName FROM SchoolInfo AS Sch";
            string orderBy = " ORDER BY School_SN";
            
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
                    // Count total results
                    string countQuery = @"SELECT COUNT(*) as TotalCount, 
                                                 SUM(CASE WHEN Validation = 'Valid' THEN 1 ELSE 0 END) as ValidCount, 
                                                 SUM(CASE WHEN Validation = 'Invalid' THEN 1 ELSE 0 END) as InvalidCount,
                                                 SUM(CASE WHEN IS_ServiceChargeActive = 1 THEN 1 ELSE 0 END) as PaymentActiveCount
                                          FROM SchoolInfo AS Sch";

                    // Add WHERE clause if conditions exist
                    if (query.Contains("WHERE"))
                    {
                        string whereSection = query.Substring(query.IndexOf("WHERE"));
                        whereSection = whereSection.Replace("ORDER BY School_SN", "").Trim();
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
                                int paymentActiveCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                                // Update summary labels
                                TotalCountLabel.Text = totalCount.ToString();
                                ValidCountLabel.Text = validCount.ToString();
                                InvalidCountLabel.Text = invalidCount.ToString();
                                PaymentActiveCountLabel.Text = paymentActiveCount.ToString();

                                // Show/hide summary based on search activity
                                bool hasSearch = !string.IsNullOrEmpty(SearchTextBox.Text.Trim()) || 
                                               !string.IsNullOrEmpty(ValidationFilter.SelectedValue) ||
                                               !string.IsNullOrEmpty(PaymentActiveFilter.SelectedValue);
                                searchSummary.Visible = hasSearch || totalCount > 0;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error or handle exception
                // For now, hide summary on error and show default counts
                searchSummary.Visible = false;
                TotalCountLabel.Text = "0";
                ValidCountLabel.Text = "0";
                InvalidCountLabel.Text = "0";
                PaymentActiveCountLabel.Text = "0";
            }
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in SchoolGridView.Rows)
            {
                TextBox Free_SMS_TextBox = (TextBox)row.FindControl("Free_SMS_TextBox");
                TextBox Discount_TextBox = (TextBox)row.FindControl("Discount_TextBox");
                TextBox Fixed_TextBox = (TextBox)row.FindControl("Fixed_TextBox");
                TextBox Per_Student_Rate = (TextBox)row.FindControl("Per_Student_TextBox");
                CheckBox Validation_CheckBox = (CheckBox)row.FindControl("Validation_CheckBox");
                CheckBox Payment_Active_CheckBox = (CheckBox)row.FindControl("Payment_Active_CheckBox");

                InstitutionSQL.UpdateParameters["SchoolID"].DefaultValue = SchoolGridView.DataKeys[row.RowIndex]["SchoolID"].ToString();
                InstitutionSQL.UpdateParameters["Free_SMS"].DefaultValue = Free_SMS_TextBox.Text == "" ? "0" : Free_SMS_TextBox.Text;
                InstitutionSQL.UpdateParameters["Per_Student_Rate"].DefaultValue = Per_Student_Rate.Text == "" ? "0" : Per_Student_Rate.Text;
                InstitutionSQL.UpdateParameters["Discount"].DefaultValue = Discount_TextBox.Text == "" ? "0" : Discount_TextBox.Text;
                InstitutionSQL.UpdateParameters["Fixed"].DefaultValue = Fixed_TextBox.Text == "" ? "0" : Fixed_TextBox.Text;
                InstitutionSQL.UpdateParameters["IS_ServiceChargeActive"].DefaultValue = Payment_Active_CheckBox.Checked.ToString();
                InstitutionSQL.UpdateParameters["Validation"].DefaultValue = Validation_CheckBox.Checked ? "Valid" : "Invalid";
                InstitutionSQL.Update();

                DeviceActiveInactiveSQL.UpdateParameters["SchoolID"].DefaultValue = SchoolGridView.DataKeys[row.RowIndex]["SchoolID"].ToString();
                DeviceActiveInactiveSQL.UpdateParameters["IsActive"].DefaultValue = Validation_CheckBox.Checked.ToString();
                DeviceActiveInactiveSQL.Update();
            }
            
            // Refresh the data after update
            LoadSchoolData();
        }
    }
}