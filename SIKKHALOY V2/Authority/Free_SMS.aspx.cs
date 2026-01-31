using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;

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

        // NEW METHOD: Get Committee Categories for a school with billing status
        protected DataTable GetCommitteeCategories(object schoolId)
        {
            int schoolID = Convert.ToInt32(schoolId);
            DataTable dt = new DataTable();
            dt.Columns.Add("CommitteeMemberTypeId", typeof(int));
            dt.Columns.Add("CommitteeMemberType", typeof(string));
            dt.Columns.Add("MemberCount", typeof(int));
            dt.Columns.Add("ActiveMemberCount", typeof(int));
            dt.Columns.Add("InactiveMemberCount", typeof(int));
            dt.Columns.Add("IsIncluded", typeof(bool));
            dt.Columns.Add("IsActive", typeof(bool));

            string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get all committee member types for this school with member count, active/inactive count and billing status
                    string query = @"
                        SELECT 
                            CMT.CommitteeMemberTypeId,
                            CMT.CommitteeMemberType,
                            COUNT(CM.CommitteeMemberId) as MemberCount,
                            COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END) as ActiveMemberCount,
                            COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Inactive' THEN 1 END) as InactiveMemberCount,
                            ISNULL(CMB.IsIncluded, 0) as IsIncluded,
                            ISNULL(CMB.IsActive, 1) as IsActive
                        FROM CommitteeMemberType CMT
                        LEFT JOIN CommitteeMember CM ON CMT.CommitteeMemberTypeId = CM.CommitteeMemberTypeId AND CM.SchoolID = @SchoolID
                        LEFT JOIN CommitteeMember_Billing CMB ON CMT.CommitteeMemberTypeId = CMB.CommitteeMemberTypeId AND CMB.SchoolID = @SchoolID
                        WHERE CMT.SchoolID = @SchoolID
                        GROUP BY CMT.CommitteeMemberTypeId, CMT.CommitteeMemberType, CMB.IsIncluded, CMB.IsActive
                        ORDER BY CMT.CommitteeMemberType";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dt.Rows.Add(
                                    reader["CommitteeMemberTypeId"],
                                    reader["CommitteeMemberType"],
                                    reader["MemberCount"],
                                    reader["ActiveMemberCount"],
                                    reader["InactiveMemberCount"],
                                    reader["IsIncluded"],
                                    reader["IsActive"]
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine("Error fetching committee categories: " + ex.Message);
            }

            return dt;
        }

        // NEW METHOD: Committee Category Repeater ItemDataBound event
        protected void CommitteeCategoryRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Find the parent GridView row
                GridViewRow gridRow = (GridViewRow)((Repeater)sender).NamingContainer;
                
                // Calculate total active committee count for this school (only selected and active categories)
                int totalActiveCount = 0;
                Repeater repeater = (Repeater)gridRow.FindControl("CommitteeCategoryRepeater");
                if (repeater != null && repeater.DataSource is DataTable)
                {
                    DataTable dt = (DataTable)repeater.DataSource;
                    foreach (DataRow row in dt.Rows)
                    {
                        bool isIncluded = Convert.ToBoolean(row["IsIncluded"]);
                        bool isActive = Convert.ToBoolean(row["IsActive"]);
                        
                        // Only count if category is included in billing AND active
                        if (isIncluded && isActive)
                        {
                            totalActiveCount += Convert.ToInt32(row["ActiveMemberCount"]);
                        }
                    }
                }
                
                // Update total label
                Label totalLabel = (Label)gridRow.FindControl("TotalCommitteeCountLabel");
                if (totalLabel != null)
                {
                    totalLabel.Text = totalActiveCount.ToString();
                }
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

                int schoolID = Convert.ToInt32(SchoolGridView.DataKeys[row.RowIndex]["SchoolID"]);

                // Update SchoolInfo
                InstitutionSQL.UpdateParameters["SchoolID"].DefaultValue = schoolID.ToString();
                InstitutionSQL.UpdateParameters["Free_SMS"].DefaultValue = Free_SMS_TextBox.Text == "" ? "0" : Free_SMS_TextBox.Text;
                InstitutionSQL.UpdateParameters["Per_Student_Rate"].DefaultValue = Per_Student_Rate.Text == "" ? "0" : Per_Student_Rate.Text;
                InstitutionSQL.UpdateParameters["Discount"].DefaultValue = Discount_TextBox.Text == "" ? "0" : Discount_TextBox.Text;
                InstitutionSQL.UpdateParameters["Fixed"].DefaultValue = Fixed_TextBox.Text == "" ? "0" : Fixed_TextBox.Text;
                InstitutionSQL.UpdateParameters["IS_ServiceChargeActive"].DefaultValue = Payment_Active_CheckBox.Checked.ToString();
                InstitutionSQL.UpdateParameters["Validation"].DefaultValue = Validation_CheckBox.Checked ? "Valid" : "Invalid";
                InstitutionSQL.Update();

                // Update Device Active/Inactive
                DeviceActiveInactiveSQL.UpdateParameters["SchoolID"].DefaultValue = schoolID.ToString();
                DeviceActiveInactiveSQL.UpdateParameters["IsActive"].DefaultValue = Validation_CheckBox.Checked.ToString();
                DeviceActiveInactiveSQL.Update();

                // NEW: Save Committee Member Billing Selections
                SaveCommitteeBillingSelections(row, schoolID);
            }
            
            // Refresh the data after update
            LoadSchoolData();
            
            // Show success message
            System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "showalert", 
                "alert('All changes have been saved successfully including Committee billing settings!');", true);
        }

        // NEW METHOD: Save Committee Billing Selections
        private void SaveCommitteeBillingSelections(GridViewRow row, int schoolID)
        {
            Repeater committeeRepeater = (Repeater)row.FindControl("CommitteeCategoryRepeater");
            if (committeeRepeater == null) return;

            string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Create table if doesn't exist (with IsActive column)
                    string createTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
                        BEGIN
                            CREATE TABLE CommitteeMember_Billing (
                                BillingId INT IDENTITY(1,1) PRIMARY KEY,
                                SchoolID INT NOT NULL,
                                CommitteeMemberTypeId INT NOT NULL,
                                IsIncluded BIT NOT NULL DEFAULT 0,
                                IsActive BIT NOT NULL DEFAULT 1,
                                CreatedDate DATETIME DEFAULT GETDATE(),
                                UpdatedDate DATETIME DEFAULT GETDATE(),
                                CONSTRAINT UC_School_Category UNIQUE (SchoolID, CommitteeMemberTypeId)
                            )
                        END
                        
                        -- Add IsActive column if table exists but column doesn't
                        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
                           AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember_Billing') AND name = 'IsActive')
                        BEGIN
                            ALTER TABLE CommitteeMember_Billing ADD IsActive BIT NOT NULL DEFAULT 1
                        END";
                    
                    using (SqlCommand createCmd = new SqlCommand(createTableQuery, conn))
                    {
                        createCmd.ExecuteNonQuery();
                    }

                    // Process each category checkbox
                    foreach (RepeaterItem item in committeeRepeater.Items)
                    {
                        CheckBox categoryCheckBox = (CheckBox)item.FindControl("CategoryCheckBox");
                        CheckBox categoryActiveCheckBox = (CheckBox)item.FindControl("CategoryActiveCheckBox");
                        HiddenField categoryIdHF = (HiddenField)item.FindControl("CategoryIdHF");
                        
                        if (categoryCheckBox != null && categoryActiveCheckBox != null && categoryIdHF != null)
                        {
                            int categoryId = Convert.ToInt32(categoryIdHF.Value);
                            bool isIncluded = categoryCheckBox.Checked;
                            bool isActive = categoryActiveCheckBox.Checked;

                            // Insert or Update billing status
                            string upsertQuery = @"
                                IF EXISTS (SELECT 1 FROM CommitteeMember_Billing WHERE SchoolID = @SchoolID AND CommitteeMemberTypeId = @CategoryId)
                                BEGIN
                                    UPDATE CommitteeMember_Billing 
                                    SET IsIncluded = @IsIncluded, 
                                        IsActive = @IsActive,
                                        UpdatedDate = GETDATE()
                                    WHERE SchoolID = @SchoolID AND CommitteeMemberTypeId = @CategoryId
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO CommitteeMember_Billing (SchoolID, CommitteeMemberTypeId, IsIncluded, IsActive)
                                    VALUES (@SchoolID, @CategoryId, @IsIncluded, @IsActive)
                                END";

                            using (SqlCommand cmd = new SqlCommand(upsertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                                cmd.Parameters.AddWithValue("@IsIncluded", isIncluded);
                                cmd.Parameters.AddWithValue("@IsActive", isActive);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine("Error saving committee billing: " + ex.Message);
                System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), "showerror", 
                    "alert('Error saving committee billing settings: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }
    }
}