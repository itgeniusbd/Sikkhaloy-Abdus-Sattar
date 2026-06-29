using Education;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority
{
    public partial class Bulk_Client_SMS : Page
    {
        private const string SmsPurpose = "Authority Client SMS";
        private const string PageMenuUrl = "~/Authority/Bulk_Client_SMS.aspx";
        private const string PageMenuTitle = "Client SMS";

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureMenuLink();
            DevModePanel.Visible = SMS_Class.IsDevModeActive();

            if (!IsPostBack)
            {
                LoadSchoolData();
                LoadSmsBalance();
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            LoadSchoolData();
        }

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            SearchTextBox.Text = "";
            ValidationFilter.SelectedValue = "";
            LoadSchoolData();
        }

        protected void SelectAllButton_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(true);
        }

        protected void ClearSelectionButton_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(false);
        }

        protected void SchoolGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var validation = DataBinder.Eval(e.Row.DataItem, "Validation")?.ToString();
            if (validation == "Invalid")
                e.Row.CssClass = "Invaid_Ins";
            else
                e.Row.CssClass = "Valid_Ins";

            var statusLabel = (Label)e.Row.FindControl("StatusLabel");
            if (statusLabel != null)
            {
                statusLabel.CssClass = validation == "Valid" ? "status-valid" : "status-invalid";
            }
        }

        protected void SendButton_Click(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;

            string message = MessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                ShowResult("মেসেজ লিখুন।", false);
                return;
            }

            var selectedSchools = GetSelectedSchools();
            if (selectedSchools.Count == 0)
            {
                ShowResult("কমপক্ষে একটি প্রতিষ্ঠান নির্বাচন করুন।", false);
                return;
            }

            SMS_Class sms;
            try
            {
                sms = new SMS_Class(GetPlatformSmsSchoolId());
            }
            catch (Exception ex)
            {
                ShowResult("SMS সার্ভিস লোড করা যায়নি: " + ex.Message, false);
                return;
            }

            int sentCount = 0;
            int failedCount = 0;
            int skippedCount = 0;
            var failedDetails = new StringBuilder();
            var successDetails = new StringBuilder();

            foreach (var school in selectedSchools)
            {
                var phones = ParsePhones(school.Phone);
                if (phones.Count == 0)
                {
                    skippedCount++;
                    failedDetails.AppendLine(school.SchoolName + " — কোনো ফোন নম্বর নেই।");
                    continue;
                }

                foreach (var phone in phones)
                {
                    var validation = sms.SMS_Validation(phone, message);
                    if (!validation.Validation)
                    {
                        failedCount++;
                        failedDetails.AppendLine(school.SchoolName + " (" + phone + ") — " + validation.Message);
                        continue;
                    }

                    Guid sendId = sms.SMS_Send(phone, message, SmsPurpose);
                    if (sendId != Guid.Empty)
                    {
                        sentCount++;
                        var gatewayInfo = SMS_Class.GetGatewayResponse(sendId);
                        if (!string.IsNullOrEmpty(gatewayInfo))
                            successDetails.AppendLine(gatewayInfo);
                    }
                    else
                    {
                        failedCount++;
                        failedDetails.AppendLine(school.SchoolName + " (" + phone + ") — " +
                            (string.IsNullOrWhiteSpace(sms.LastSendError) ? "পাঠানো যায়নি।" : sms.LastSendError));
                    }
                }
            }

            LoadSmsBalance();

            var summary = new StringBuilder();
            if (SMS_Class.IsDevModeActive())
                summary.Append("<strong>Dev Mode:</strong> SMS লগ হয়েছে, মোবাইলে পাঠানো হয়নি। ");
            summary.AppendFormat("পাঠানো হয়েছে: {0} | ব্যর্থ: {1}", sentCount, failedCount);
            if (skippedCount > 0)
                summary.AppendFormat(" | ফোন ছাড়া প্রতিষ্ঠান: {0}", skippedCount);

            if (successDetails.Length > 0)
            {
                summary.Append("<br/><small><strong>Gateway Response:</strong><br/>")
                    .Append(Server.HtmlEncode(successDetails.ToString()).Replace("\n", "<br/>"))
                    .Append("</small>");
            }

            if (failedDetails.Length > 0)
            {
                summary.Append("<br/><small>").Append(Server.HtmlEncode(failedDetails.ToString()).Replace("\n", "<br/>")).Append("</small>");
                ShowResult(summary.ToString(), sentCount > 0);
            }
            else
            {
                ShowResult(summary.ToString(), true);
            }
        }

        private void LoadSchoolData()
        {
            var dt = new DataTable();
            dt.Columns.Add("SchoolID", typeof(int));
            dt.Columns.Add("SchoolName", typeof(string));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("Validation", typeof(string));
            dt.Columns.Add("StatusText", typeof(string));
            dt.Columns.Add("PhoneCount", typeof(int));
            dt.Columns.Add("Date", typeof(DateTime));

            string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            var query = new StringBuilder(@"
                SELECT SchoolID, SchoolName, UserName, Phone, Validation, Date
                FROM SchoolInfo
                WHERE 1=1");

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;

                if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    query.Append(" AND (SchoolName LIKE @SearchText OR UserName LIKE @SearchText OR Phone LIKE @SearchText OR CAST(SchoolID AS VARCHAR) LIKE @SearchText)");
                    command.Parameters.AddWithValue("@SearchText", "%" + SearchTextBox.Text.Trim() + "%");
                }

                if (!string.IsNullOrEmpty(ValidationFilter.SelectedValue))
                {
                    query.Append(" AND Validation = @Validation");
                    command.Parameters.AddWithValue("@Validation", ValidationFilter.SelectedValue);
                }

                query.Append(" ORDER BY SchoolID DESC");
                command.CommandText = query.ToString();

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string phone = reader["Phone"]?.ToString() ?? "";
                        string validation = reader["Validation"]?.ToString() ?? "";
                        dt.Rows.Add(
                            Convert.ToInt32(reader["SchoolID"]),
                            reader["SchoolName"]?.ToString(),
                            reader["UserName"]?.ToString(),
                            phone,
                            validation,
                            validation == "Valid" ? "Active" : "Deactive",
                            ParsePhones(phone).Count,
                            reader["Date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Date"])
                        );
                    }
                }
            }

            SchoolGridView.DataSource = dt;
            SchoolGridView.DataBind();

            int active = dt.AsEnumerable().Count(r => r.Field<string>("Validation") == "Valid");
            int deactive = dt.Rows.Count - active;
            TotalCountLabel.Text = dt.Rows.Count.ToString();
            ActiveCountLabel.Text = active.ToString();
            DeactiveCountLabel.Text = deactive.ToString();
        }

        private void LoadSmsBalance()
        {
            ActiveProviderLabel.Text = SMS_Class.GetActiveProviderSummary();

            try
            {
                var sms = new SMS_Class(GetPlatformSmsSchoolId());
                SmsBalanceLabel.Text = sms.SMS_GetBalance().ToString();
            }
            catch
            {
                SmsBalanceLabel.Text = "N/A";
            }
        }

        private List<SchoolSmsRow> GetSelectedSchools()
        {
            var list = new List<SchoolSmsRow>();

            foreach (GridViewRow row in SchoolGridView.Rows)
            {
                var chk = row.FindControl("chkSelect") as CheckBox;
                if (chk == null || !chk.Checked)
                    continue;

                list.Add(new SchoolSmsRow
                {
                    SchoolID = Convert.ToInt32(SchoolGridView.DataKeys[row.RowIndex]["SchoolID"]),
                    SchoolName = ((HiddenField)row.FindControl("hidSchoolName"))?.Value ?? "",
                    Phone = SchoolGridView.DataKeys[row.RowIndex]["Phone"]?.ToString() ?? ""
                });
            }

            return list;
        }

        private void SetAllCheckboxes(bool selected)
        {
            foreach (GridViewRow row in SchoolGridView.Rows)
            {
                var chk = row.FindControl("chkSelect") as CheckBox;
                if (chk != null)
                    chk.Checked = selected;
            }
        }

        private void ShowResult(string message, bool isSuccess)
        {
            ResultPanel.Visible = true;
            ResultPanel.CssClass = "alert-msg " + (isSuccess ? "alert-success" : "alert-error");
            ResultLabel.Text = message;
        }

        private static List<string> ParsePhones(string phoneField)
        {
            var phones = new List<string>();
            if (string.IsNullOrWhiteSpace(phoneField))
                return phones;

            foreach (var part in phoneField.Split(new[] { ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var number = part.Trim();
                if (!string.IsNullOrEmpty(number) && !phones.Contains(number))
                    phones.Add(number);
            }

            return phones;
        }

        private static string GetPlatformSmsSchoolId()
        {
            var configured = ConfigurationManager.AppSettings["AuthorityPlatformSmsSchoolId"];
            if (!string.IsNullOrWhiteSpace(configured))
                return configured.Trim();

            string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("SELECT TOP 1 CAST(SchoolID AS VARCHAR(20)) FROM SMS ORDER BY SchoolID", connection))
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return result.ToString();
            }

            return "1";
        }

        private void EnsureMenuLink()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int linkCategoryId = 0;
                    object subCategoryId = DBNull.Value;
                    int ascending = 1;

                    // Match SMS Setting placement exactly (usually Basic Option, SubCategoryID NULL).
                    using (var refCmd = new SqlCommand(@"
                        SELECT TOP 1 LinkCategoryID, SubCategoryID, Ascending
                        FROM Authority_Link_Pages
                        WHERE PageURL LIKE '%SmsSetting%'", connection))
                    using (var reader = refCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            linkCategoryId = reader.GetInt32(0);
                            subCategoryId = reader.IsDBNull(1) ? (object)DBNull.Value : reader.GetInt32(1);
                            ascending = reader.GetInt32(2) + 1;
                        }
                    }

                    if (linkCategoryId == 0)
                    {
                        using (var fallbackCmd = new SqlCommand(
                            "SELECT TOP 1 LinkCategoryID FROM Authority_Link_Category WHERE Category LIKE '%Basic%'", connection))
                        {
                            var result = fallbackCmd.ExecuteScalar();
                            if (result == null || result == DBNull.Value)
                                return;
                            linkCategoryId = Convert.ToInt32(result);
                        }

                        using (var ascCmd = new SqlCommand(@"
                            SELECT ISNULL(MAX(Ascending), 0) + 1
                            FROM Authority_Link_Pages
                            WHERE LinkCategoryID = @LinkCategoryID AND SubCategoryID IS NULL", connection))
                        {
                            ascCmd.Parameters.AddWithValue("@LinkCategoryID", linkCategoryId);
                            ascending = Convert.ToInt32(ascCmd.ExecuteScalar());
                        }
                    }

                    object existingLinkId;
                    using (var existsCmd = new SqlCommand(
                        "SELECT LinkID FROM Authority_Link_Pages WHERE PageURL LIKE '%Bulk_Client_SMS%'", connection))
                    {
                        existingLinkId = existsCmd.ExecuteScalar();
                    }

                    if (existingLinkId != null && existingLinkId != DBNull.Value)
                    {
                        using (var updateCmd = new SqlCommand(@"
                            UPDATE Authority_Link_Pages
                            SET LinkCategoryID = @LinkCategoryID,
                                SubCategoryID = @SubCategoryID,
                                PageTitle = @PageTitle,
                                PageURL = @PageURL,
                                Ascending = @Ascending
                            WHERE LinkID = @LinkID", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@LinkCategoryID", linkCategoryId);
                            updateCmd.Parameters.AddWithValue("@SubCategoryID", subCategoryId);
                            updateCmd.Parameters.AddWithValue("@PageTitle", PageMenuTitle);
                            updateCmd.Parameters.AddWithValue("@PageURL", PageMenuUrl);
                            updateCmd.Parameters.AddWithValue("@Ascending", ascending);
                            updateCmd.Parameters.AddWithValue("@LinkID", Convert.ToInt32(existingLinkId));
                            updateCmd.ExecuteNonQuery();
                        }
                        return;
                    }

                    using (var insertCmd = new SqlCommand(@"
                        INSERT INTO Authority_Link_Pages (LinkCategoryID, SubCategoryID, PageTitle, PageURL, Ascending)
                        VALUES (@LinkCategoryID, @SubCategoryID, @PageTitle, @PageURL, @Ascending)", connection))
                    {
                        insertCmd.Parameters.AddWithValue("@LinkCategoryID", linkCategoryId);
                        insertCmd.Parameters.AddWithValue("@SubCategoryID", subCategoryId);
                        insertCmd.Parameters.AddWithValue("@PageTitle", PageMenuTitle);
                        insertCmd.Parameters.AddWithValue("@PageURL", PageMenuUrl);
                        insertCmd.Parameters.AddWithValue("@Ascending", ascending);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Sidebar registration is best-effort; page remains usable via direct URL.
            }
        }

        private class SchoolSmsRow
        {
            public int SchoolID { get; set; }
            public string SchoolName { get; set; }
            public string Phone { get; set; }
        }
    }
}
