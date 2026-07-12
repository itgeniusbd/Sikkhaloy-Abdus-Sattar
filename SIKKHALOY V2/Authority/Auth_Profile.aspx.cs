using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority
{
    public partial class Auth_Profile : System.Web.UI.Page
    {
        private const string InstitutionBaseQuery = @"
SELECT Sch.SchoolID, Sch.SchoolName, Sch.Phone, Sch.Validation, Sch.Date, Sch.UserName,
    ses.LoggedInUser, ses.LoginRole, ses.LoginTime, ses.LastActivity
FROM SchoolInfo AS Sch
OUTER APPLY (
    SELECT TOP 1 u.UserName AS LoggedInUser, u.Category AS LoginRole, u.LoginTime, u.LastActivity
    FROM User_Active_Sessions u
    WHERE u.SchoolID = Sch.SchoolID
      AND (u.LastActivity >= DATEADD(HOUR, -1, GETDATE()) OR CAST(u.LoginTime AS DATE) = CAST(GETDATE() AS DATE))
    ORDER BY u.LastActivity DESC
) ses";

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadSchoolData();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            LoadLoggedInUsersCount();
            UpdateLiveFilterButtons();
        }

        protected void FilterAllBtn_Click(object sender, EventArgs e)
        {
            ApplyLiveFilter("");
        }

        protected void FilterActiveBtn_Click(object sender, EventArgs e)
        {
            ApplyLiveFilter("LoggedIn");
        }

        protected void FilterTodayBtn_Click(object sender, EventArgs e)
        {
            ApplyLiveFilter("Today");
        }

        protected void FilterLastHourBtn_Click(object sender, EventArgs e)
        {
            ApplyLiveFilter("LastHour");
        }

        protected void FilterLiveNowBtn_Click(object sender, EventArgs e)
        {
            ApplyLiveFilter("LiveNow");
        }

        private void ApplyLiveFilter(string filterKey)
        {
            OnlineFilterValue.Value = filterKey ?? "";
            LoadSchoolData();
            UpdateLiveFilterButtons();
        }

        private void UpdateLiveFilterButtons()
        {
            var current = OnlineFilterValue.Value ?? "";

            FilterAllPanel.CssClass = "live-filter-item live-filter-all" + (current == "" ? " selected" : "");
            FilterActivePanel.CssClass = "live-filter-item live-filter-active" + (current == "LoggedIn" ? " selected" : "");
            FilterTodayPanel.CssClass = "live-filter-item live-filter-today" + (current == "Today" ? " selected" : "");
            FilterLastHourPanel.CssClass = "live-filter-item live-filter-hour" + (current == "LastHour" ? " selected" : "");
            FilterLiveNowPanel.CssClass = "live-filter-item live-filter-live" + (current == "LiveNow" ? " selected" : "");

            switch (current)
            {
                case "LoggedIn": ActiveFilterLabel.Text = "Currently Active (15 min)"; break;
                case "Today": ActiveFilterLabel.Text = "Today"; break;
                case "LastHour": ActiveFilterLabel.Text = "Last Hour"; break;
                case "LiveNow": ActiveFilterLabel.Text = "Online Now (5 min)"; break;
                default: ActiveFilterLabel.Text = "All"; break;
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
            OnlineFilterValue.Value = "";
            StartDateTextBox.Text = "";
            EndDateTextBox.Text = "";
            LoadSchoolData();
            UpdateLiveFilterButtons();
        }

        protected void SchoolGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var lastActivityObj = DataBinder.Eval(e.Row.DataItem, "LastActivity");
            if (lastActivityObj == null || lastActivityObj == DBNull.Value)
                return;

            var lastActivity = Convert.ToDateTime(lastActivityObj);
            if (lastActivity >= DateTime.Now.AddMinutes(-5))
                e.Row.CssClass = "online-now-row";
            else if (lastActivity >= DateTime.Now.AddMinutes(-15))
                e.Row.CssClass = "online-active-row";
        }

        protected string GetOnlineStatusBadge(object lastActivityObj)
        {
            if (lastActivityObj == null || lastActivityObj == DBNull.Value)
                return string.Empty;

            var lastActivity = Convert.ToDateTime(lastActivityObj);

            if (lastActivity >= DateTime.Now.AddMinutes(-5))
                return "<span class=\"online-badge online-now\"><i class=\"fa fa-circle\"></i> Online</span>";

            if (lastActivity >= DateTime.Now.AddMinutes(-15))
                return "<span class=\"online-badge online-active\"><i class=\"fa fa-circle\"></i> Active</span>";

            return string.Empty;
        }

        private void LoadSchoolData()
        {
            StringBuilder whereClause = new StringBuilder();
            bool hasCondition = false;

            if (!string.IsNullOrEmpty(SearchTextBox.Text.Trim()))
            {
                whereClause.Append("(Sch.SchoolName LIKE @SearchText OR Sch.UserName LIKE @SearchText OR Sch.Phone LIKE @SearchText OR CAST(Sch.SchoolID AS VARCHAR) LIKE @SearchText)");
                hasCondition = true;
            }

            if (!string.IsNullOrEmpty(ValidationFilter.SelectedValue))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");

                whereClause.Append("Sch.Validation = @ValidationStatus");
                hasCondition = true;
            }

            AppendOnlineFilter(whereClause, ref hasCondition);

            if (!string.IsNullOrEmpty(StartDateTextBox.Text.Trim()))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");

                whereClause.Append("Sch.Date >= @StartDate");
                hasCondition = true;
            }

            if (!string.IsNullOrEmpty(EndDateTextBox.Text.Trim()))
            {
                if (hasCondition)
                    whereClause.Append(" AND ");

                whereClause.Append("Sch.Date <= @EndDate");
                hasCondition = true;
            }

            string finalQuery = InstitutionBaseQuery;
            if (hasCondition)
            {
                finalQuery += " WHERE " + whereClause;
            }
            finalQuery += " ORDER BY ses.LastActivity DESC, Sch.Date DESC, Sch.SchoolID";

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
                if (DateTime.TryParseExact(StartDateTextBox.Text.Trim(), new[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                {
                    InstitutionSQL.SelectParameters.Add("StartDate", TypeCode.DateTime, startDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    finalQuery = finalQuery.Replace("Sch.Date >= @StartDate", "1=1");
                    CleanupPlaceholderConditions(ref finalQuery);
                }
            }

            if (!string.IsNullOrEmpty(EndDateTextBox.Text.Trim()))
            {
                DateTime endDate;
                if (DateTime.TryParseExact(EndDateTextBox.Text.Trim(), new[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                {
                    endDate = endDate.AddDays(1).AddSeconds(-1);
                    InstitutionSQL.SelectParameters.Add("EndDate", TypeCode.DateTime, endDate.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    finalQuery = finalQuery.Replace("Sch.Date <= @EndDate", "1=1");
                    CleanupPlaceholderConditions(ref finalQuery);
                }
            }

            InstitutionSQL.SelectCommand = finalQuery;
            SchoolGridView.DataBind();
            CalculateAndDisplaySummary(finalQuery);
        }

        private static void CleanupPlaceholderConditions(ref string finalQuery)
        {
            if (finalQuery.Contains(" AND 1=1"))
                finalQuery = finalQuery.Replace(" AND 1=1", "");
            if (finalQuery.Contains("WHERE 1=1 AND "))
                finalQuery = finalQuery.Replace("WHERE 1=1 AND ", "WHERE ");
            if (finalQuery.Contains("WHERE 1=1"))
                finalQuery = finalQuery.Replace("WHERE 1=1", "");
        }

        private void AppendOnlineFilter(StringBuilder whereClause, ref bool hasCondition)
        {
            var filter = OnlineFilterValue.Value ?? "";
            if (string.IsNullOrEmpty(filter))
                return;

            if (hasCondition)
                whereClause.Append(" AND ");

            switch (filter)
            {
                case "LiveNow":
                    whereClause.Append("EXISTS (SELECT 1 FROM User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND u.LastActivity >= DATEADD(MINUTE, -5, GETDATE()))");
                    break;
                case "LoggedIn":
                    whereClause.Append("EXISTS (SELECT 1 FROM User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND u.LastActivity >= DATEADD(MINUTE, -15, GETDATE()))");
                    break;
                case "LastHour":
                    whereClause.Append("EXISTS (SELECT 1 FROM User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND u.LastActivity >= DATEADD(HOUR, -1, GETDATE()))");
                    break;
                case "Today":
                    whereClause.Append("EXISTS (SELECT 1 FROM User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND u.SchoolID IS NOT NULL AND CAST(u.LoginTime AS DATE) = CAST(GETDATE() AS DATE))");
                    break;
            }

            hasCondition = true;
        }

        private void CalculateAndDisplaySummary(string query)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    int fromIndex = query.IndexOf("FROM SchoolInfo", StringComparison.OrdinalIgnoreCase);
                    string countQuery = "SELECT COUNT(*) as TotalCount, SUM(CASE WHEN Sch.Validation = 'Valid' THEN 1 ELSE 0 END) as ValidCount, SUM(CASE WHEN Sch.Validation = 'Invalid' THEN 1 ELSE 0 END) as InvalidCount " +
                                        query.Substring(fromIndex);
                    countQuery = countQuery.Replace(" ORDER BY ses.LastActivity DESC, Sch.Date DESC, Sch.SchoolID", "");

                    using (SqlCommand command = new SqlCommand(countQuery, connection))
                    {
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
                            if (DateTime.TryParseExact(StartDateTextBox.Text.Trim(), new[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                            {
                                command.Parameters.AddWithValue("@StartDate", startDate);
                            }
                        }

                        if (!string.IsNullOrEmpty(EndDateTextBox.Text.Trim()) && countQuery.Contains("@EndDate"))
                        {
                            DateTime endDate;
                            if (DateTime.TryParseExact(EndDateTextBox.Text.Trim(), new[] { "dd M yyyy", "d M yyyy", "dd MMM yyyy", "d MMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
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
                                int totalCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                int validCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                int invalidCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                                TotalCountLabel.Text = totalCount.ToString();
                                ValidCountLabel.Text = validCount.ToString();
                                InvalidCountLabel.Text = invalidCount.ToString();
                                UpdateDateRangeLabel();

                                bool hasSearch = !string.IsNullOrEmpty(SearchTextBox.Text.Trim()) ||
                                                 !string.IsNullOrEmpty(ValidationFilter.SelectedValue) ||
                                                 !string.IsNullOrEmpty(OnlineFilterValue.Value) ||
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
                dateRangeText = $"{StartDateTextBox.Text} to {EndDateTextBox.Text}";
            else if (hasStartDate)
                dateRangeText = $"From {StartDateTextBox.Text}";
            else if (hasEndDate)
                dateRangeText = $"Up to {EndDateTextBox.Text}";

            DateRangeLabel.Text = dateRangeText;
        }

        private void LoadLoggedInUsersCount()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string cleanupQuery = @"
                        DELETE FROM User_Active_Sessions 
                        WHERE LastActivity < DATEADD(MINUTE, -30, GETDATE())";

                    using (SqlCommand cleanupCmd = new SqlCommand(cleanupQuery, connection))
                    {
                        cleanupCmd.ExecuteNonQuery();
                    }

                    string activeUsersQuery = @"
                        SELECT COUNT(DISTINCT u.SchoolID)
                        FROM User_Active_Sessions u
                        WHERE u.SchoolID IS NOT NULL
                          AND u.LastActivity >= DATEADD(MINUTE, -15, GETDATE())";

                    string todayLoginsQuery = @"
                        SELECT COUNT(DISTINCT u.SchoolID)
                        FROM User_Active_Sessions u
                        WHERE u.SchoolID IS NOT NULL
                          AND CAST(u.LoginTime AS DATE) = CAST(GETDATE() AS DATE)";

                    string lastHourQuery = @"
                        SELECT COUNT(DISTINCT u.SchoolID)
                        FROM User_Active_Sessions u
                        WHERE u.SchoolID IS NOT NULL
                          AND u.LastActivity >= DATEADD(HOUR, -1, GETDATE())";

                    string onlineNowQuery = @"
                        SELECT COUNT(DISTINCT u.SchoolID)
                        FROM User_Active_Sessions u
                        WHERE u.SchoolID IS NOT NULL
                          AND u.LastActivity >= DATEADD(MINUTE, -5, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(activeUsersQuery, connection))
                    {
                        object result = cmd.ExecuteScalar();
                        LoggedInUsersCountLabel.Text = result != null ? result.ToString() : "0";
                    }

                    using (SqlCommand cmd = new SqlCommand(todayLoginsQuery, connection))
                    {
                        object result = cmd.ExecuteScalar();
                        TodayLoginsLabel.Text = result != null ? result.ToString() : "0";
                    }

                    using (SqlCommand cmd = new SqlCommand(lastHourQuery, connection))
                    {
                        object result = cmd.ExecuteScalar();
                        LastHourLoginsLabel.Text = result != null ? result.ToString() : "0";
                    }

                    using (SqlCommand cmd = new SqlCommand(onlineNowQuery, connection))
                    {
                        object result = cmd.ExecuteScalar();
                        OnlineNowLabel.Text = result != null ? result.ToString() : "0";
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SchoolInfo", connection))
                    {
                        object result = cmd.ExecuteScalar();
                        AllInstitutionCountLabel.Text = result != null ? result.ToString() : "0";
                    }
                }
            }
            catch (Exception ex)
            {
                LoggedInUsersCountLabel.Text = "0";
                TodayLoginsLabel.Text = "0";
                LastHourLoginsLabel.Text = "0";
                OnlineNowLabel.Text = "0";
                AllInstitutionCountLabel.Text = "0";
                System.Diagnostics.Debug.WriteLine("Error loading active users: " + ex.Message);

                try
                {
                    string logPath = Server.MapPath("~/App_Data/session_tracking_errors.txt");
                    string logDir = System.IO.Path.GetDirectoryName(logPath);
                    if (!System.IO.Directory.Exists(logDir))
                    {
                        System.IO.Directory.CreateDirectory(logDir);
                    }
                    System.IO.File.AppendAllText(logPath,
                        $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\n"
                    );
                }
                catch { }
            }
        }
    }
}
