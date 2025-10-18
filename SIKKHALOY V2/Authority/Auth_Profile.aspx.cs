using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Globalization;

namespace EDUCATION.COM.Authority
{
    public partial class Auth_Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSchoolData();
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            NoticeSQL.Insert();
        }

        protected void FIndButton_Click(object sender, EventArgs e)
        {
            LoadSchoolData();
        }

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            SearchTextBox.Text = "";
            ValidationFilter.SelectedValue = "";
            StartDateTextBox.Text = "";
            EndDateTextBox.Text = "";
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

            // Date filter conditions
            if (!string.IsNullOrEmpty(StartDateTextBox.Text.Trim()))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");
                
                whereClause.Append("Date >= @StartDate");
                hasCondition = true;
            }

            if (!string.IsNullOrEmpty(EndDateTextBox.Text.Trim()))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");
                
                whereClause.Append("Date <= @EndDate");
                hasCondition = true;
            }

            // Build the complete SQL query
            string baseQuery = "SELECT SchoolID, SchoolName, Phone, Validation, Date, UserName FROM SchoolInfo AS Sch";
            string orderBy = " ORDER BY Date DESC, SchoolID";
            
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

            if (!string.IsNullOrEmpty(StartDateTextBox.Text.Trim()))
            {
                DateTime startDate;
                if (DateTime.TryParseExact(StartDateTextBox.Text.Trim(), new string[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                {
                    InstitutionSQL.SelectParameters.Add("StartDate", TypeCode.DateTime, startDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    // If parsing fails, try to remove the WHERE condition for StartDate
                    finalQuery = finalQuery.Replace("Date >= @StartDate", "1=1");
                    if (finalQuery.Contains(" AND 1=1"))
                        finalQuery = finalQuery.Replace(" AND 1=1", "");
                    if (finalQuery.Contains("WHERE 1=1 AND "))
                        finalQuery = finalQuery.Replace("WHERE 1=1 AND ", "WHERE ");
                    if (finalQuery.Contains("WHERE 1=1"))
                        finalQuery = finalQuery.Replace("WHERE 1=1", "");
                }
            }

            if (!string.IsNullOrEmpty(EndDateTextBox.Text.Trim()))
            {
                DateTime endDate;
                if (DateTime.TryParseExact(EndDateTextBox.Text.Trim(), new string[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                {
                    // Add 23:59:59 to include the entire end date
                    endDate = endDate.AddDays(1).AddSeconds(-1);
                    InstitutionSQL.SelectParameters.Add("EndDate", TypeCode.DateTime, endDate.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    // If parsing fails, try to remove the WHERE condition for EndDate
                    finalQuery = finalQuery.Replace("Date <= @EndDate", "1=1");
                    if (finalQuery.Contains(" AND 1=1"))
                        finalQuery = finalQuery.Replace(" AND 1=1", "");
                    if (finalQuery.Contains("WHERE 1=1 AND "))
                        finalQuery = finalQuery.Replace("WHERE 1=1 AND ", "WHERE ");
                    if (finalQuery.Contains("WHERE 1=1"))
                        finalQuery = finalQuery.Replace("WHERE 1=1", "");
                }
            }

            // Update the final query after parameter validation
            InstitutionSQL.SelectCommand = finalQuery;

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
                    string countQuery = query.Replace("SELECT SchoolID, SchoolName, Phone, Validation, Date, UserName FROM", "SELECT COUNT(*) as TotalCount, SUM(CASE WHEN Validation = 'Valid' THEN 1 ELSE 0 END) as ValidCount, SUM(CASE WHEN Validation = 'Invalid' THEN 1 ELSE 0 END) as InvalidCount FROM");
                    // Remove ORDER BY clause for count query
                    countQuery = countQuery.Replace(" ORDER BY Date DESC, SchoolID", "");

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

                        if (!string.IsNullOrEmpty(StartDateTextBox.Text.Trim()) && countQuery.Contains("@StartDate"))
                        {
                            DateTime startDate;
                            if (DateTime.TryParseExact(StartDateTextBox.Text.Trim(), new string[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                            {
                                command.Parameters.AddWithValue("@StartDate", startDate);
                            }
                        }

                        if (!string.IsNullOrEmpty(EndDateTextBox.Text.Trim()) && countQuery.Contains("@EndDate"))
                        {
                            DateTime endDate;
                            if (DateTime.TryParseExact(EndDateTextBox.Text.Trim(), new string[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                            {
                                endDate = endDate.AddDays(1).AddSeconds(-1);
                                command.Parameters.AddWithValue("@EndDate", endDate);
                            }
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

                                // Update date range label
                                UpdateDateRangeLabel();

                                // Show/hide summary based on search activity
                                bool hasSearch = !string.IsNullOrEmpty(SearchTextBox.Text.Trim()) || 
                                               !string.IsNullOrEmpty(ValidationFilter.SelectedValue) ||
                                               !string.IsNullOrEmpty(StartDateTextBox.Text.Trim()) ||
                                               !string.IsNullOrEmpty(EndDateTextBox.Text.Trim());
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
                DateRangeLabel.Text = "Error occurred";
            }
        }

        private void UpdateDateRangeLabel()
        {
            string dateRangeText = "All Time";
            
            bool hasStartDate = !string.IsNullOrEmpty(StartDateTextBox.Text.Trim());
            bool hasEndDate = !string.IsNullOrEmpty(EndDateTextBox.Text.Trim());

            if (hasStartDate && hasEndDate)
            {
                dateRangeText = $"{StartDateTextBox.Text} to {EndDateTextBox.Text}";
            }
            else if (hasStartDate)
            {
                dateRangeText = $"From {StartDateTextBox.Text}";
            }
            else if (hasEndDate)
            {
                dateRangeText = $"Up to {EndDateTextBox.Text}";
            }

            DateRangeLabel.Text = dateRangeText;
        }
    }
}