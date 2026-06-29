using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.SMS
{
    public partial class SMS_Template : System.Web.UI.Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string category = CurrentCategoryHiddenField.Value;
                if (string.IsNullOrEmpty(category) && ViewState["EditingCategory"] != null)
                    category = ViewState["EditingCategory"].ToString();
                if (string.IsNullOrEmpty(category))
                    category = GetCategoryFromActiveTab(ActiveTabHiddenField.Value);

                if (!string.IsNullOrEmpty(category))
                {
                    UpdateTemplateTypeOptions(category);
                    RestorePostedTemplateTypeSelection();
                }
            }
            else
            {
                UpdateTemplateTypeOptions("ExamResult");
            }
        }

        private static string GetCategoryFromActiveTab(string tabId)
        {
            switch (tabId)
            {
                case "exam-tab": return "ExamResult";
                case "payment-tab": return "Payment";
                case "due-tab": return "Due";
                case "donor-tab": return "Donor";
                case "attendance-tab": return "Attendance";
                case "admission-tab": return "Admission";
                default: return string.Empty;
            }
        }

        private string GetFormPostedValue(string uniqueId, string controlId)
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                string val = Request.Form[uniqueId];
                if (!string.IsNullOrWhiteSpace(val))
                    return val;
            }

            if (!string.IsNullOrEmpty(controlId))
            {
                foreach (string key in Request.Form.AllKeys)
                {
                    if (key == null)
                        continue;
                    if (key == controlId || key.EndsWith("$" + controlId, StringComparison.OrdinalIgnoreCase))
                    {
                        string val = Request.Form[key];
                        if (!string.IsNullOrWhiteSpace(val))
                            return val;
                    }
                }
            }

            return null;
        }

        private string GetResolvedTemplateType()
        {
            string templateType = GetFormPostedValue(TemplateTypeDropDownList.UniqueID, TemplateTypeDropDownList.ID);
            if (string.IsNullOrWhiteSpace(templateType))
                templateType = GetFormPostedValue(CurrentTemplateTypeHiddenField.UniqueID, CurrentTemplateTypeHiddenField.ID);
            if (string.IsNullOrWhiteSpace(templateType))
                templateType = TemplateTypeDropDownList.SelectedValue;
            if (string.IsNullOrWhiteSpace(templateType))
                templateType = CurrentTemplateTypeHiddenField.Value;
            return NormalizeLegacyTemplateType(templateType?.Trim());
        }

        private void RestorePostedTemplateTypeSelection()
        {
            string templateType = GetResolvedTemplateType();
            if (string.IsNullOrEmpty(templateType))
                return;

            SelectTemplateType(templateType);
            CurrentTemplateTypeHiddenField.Value = templateType;
        }

        private void SyncActiveTabWithCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return;

            string tabId;
            switch (category)
            {
                case "Payment": tabId = "payment-tab"; break;
                case "Due": tabId = "due-tab"; break;
                case "Donor": tabId = "donor-tab"; break;
                case "Attendance": tabId = "attendance-tab"; break;
                case "Admission": tabId = "admission-tab"; break;
                default: tabId = "exam-tab"; break;
            }

            ActiveTabHiddenField.Value = tabId;
        }

        private void SetOpenModalAfterPostback(bool open)
        {
            OpenModalAfterPostbackHiddenField.Value = open ? "1" : "0";
        }

        private void RegisterUiScript(string script, string key)
        {
            Page.ClientScript.RegisterStartupScript(GetType(), key, script, true);
        }

        private static int SafeToInt32(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            if (value is int i)
                return i;
            if (value is short s)
                return s;
            if (value is long l && l >= int.MinValue && l <= int.MaxValue)
                return (int)l;

            int result;
            return int.TryParse(value.ToString(), out result) ? result : defaultValue;
        }

        private static bool SafeToBoolean(object value, bool defaultValue = false)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            if (value is bool b)
                return b;

            string text = value.ToString().Trim();
            if (string.IsNullOrEmpty(text))
                return defaultValue;

            if (string.Equals(text, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(text, "0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "no", StringComparison.OrdinalIgnoreCase))
                return false;

            bool result;
            return bool.TryParse(text, out result) ? result : defaultValue;
        }

        private int GetSchoolId()
        {
            int schoolId = SafeToInt32(Session["SchoolID"], 0);
            if (schoolId <= 0)
                throw new InvalidOperationException("School session expired. Please login again.");
            return schoolId;
        }

        private int GetResolvedTemplateId()
        {
            string templateIdText = GetFormPostedValue(TemplateIDHiddenField.UniqueID, TemplateIDHiddenField.ID);
            if (string.IsNullOrWhiteSpace(templateIdText))
                templateIdText = TemplateIDHiddenField.Value;
            if (string.IsNullOrWhiteSpace(templateIdText))
                templateIdText = "0";

            int templateId = SafeToInt32(templateIdText, 0);
            if (templateId < 0)
                templateId = 0;

            TemplateIDHiddenField.Value = templateId.ToString();
            ViewState["EditingTemplateId"] = templateId;
            return templateId;
        }

        protected bool IsTemplateActive(object isActiveValue)
        {
            return SafeToBoolean(isActiveValue, false);
        }

        private void UpdateTemplateTypeOptions(string category)
        {
            TemplateTypeDropDownList.Items.Clear();

            switch (category)
            {
                case "ExamResult":
                    TemplateTypeDropDownList.Items.Add(new ListItem("Passed (পাস)", "Passed"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("Failed (ফেল)", "Failed"));
                    break;

                case "Payment":
                    TemplateTypeDropDownList.Items.Add(new ListItem("💰 Payment Receipt (পেমেন্ট রিসিট)", "Payment"));
                    break;

                case "Attendance":
                    TemplateTypeDropDownList.Items.Add(new ListItem("✅ Entry - স্কুলে প্রবেশ (সময়মতো আসা)", "Entry"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("🚪 Exit - স্কুল ত্যাগ (বের হওয়া)", "Exit"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("⏰ Late - দেরিতে আসা", "Late"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("❌ Absent - অনুপস্থিত", "Absent"));
                    break;

                case "Due":
                    TemplateTypeDropDownList.Items.Add(new ListItem("💸 Due Notification (বকেয়া নোটিফিকেশন)", "Due"));
                    break;

                case "Donor":
                    TemplateTypeDropDownList.Items.Add(new ListItem("💸 Donor Due - ডোনার বকেয়া (DonorDue)", "DonorDue"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("✅ Donor Payment - পেমেন্ট নিশ্চিতকরণ (DonorPayment)", "DonorPayment"));
                    break;

                case "Admission":
                    TemplateTypeDropDownList.Items.Add(new ListItem("🎓 Admission Confirmation (ভর্তি নিশ্চিতকরণ)", "AdmissionConfirm"));
                    break;

                default:
                    TemplateTypeDropDownList.Items.Add(new ListItem("Default", "Default"));
                    break;
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage("⚠️ Template Name (নাম) এবং Message (মেসেজ) অবশ্যই দিতে হবে।", "warning");
                SetOpenModalAfterPostback(true);
                return;
            }

            try
            {
                int templateId = GetResolvedTemplateId();
                string templateName = TemplateNameTextBox.Text.Trim();
                string templateCategory = GetFormPostedValue(CurrentCategoryHiddenField.UniqueID, CurrentCategoryHiddenField.ID);
                if (string.IsNullOrWhiteSpace(templateCategory))
                    templateCategory = CurrentCategoryHiddenField.Value;
                if (string.IsNullOrWhiteSpace(templateCategory) && ViewState["EditingCategory"] != null)
                    templateCategory = ViewState["EditingCategory"].ToString();
                if (string.IsNullOrWhiteSpace(templateCategory))
                    templateCategory = TemplateCategoryDropDownList.SelectedValue;

                if (string.IsNullOrWhiteSpace(templateCategory))
                {
                    ShowMessage("⚠️ Category পাওয়া যায়নি। Attendance tab থেকে '+ Add New' বাটন দিয়ে তৈরি করুন।", "warning");
                    SetOpenModalAfterPostback(true);
                    return;
                }

                string templateType = GetResolvedTemplateType();
                if (string.IsNullOrWhiteSpace(templateType))
                {
                    ShowMessage("⚠️ Template Type সিলেক্ট করুন (Passed / Failed / Payment ইত্যাদি)।", "warning");
                    SetOpenModalAfterPostback(true);
                    return;
                }
                string messageTemplate = MessageTemplateTextBox.Text.Trim();
                bool isActive = IsActiveCheckBox.Checked;
                int schoolId = GetSchoolId();

                if (templateCategory == "ExamResult")
                {
                    bool looksFailed = templateName.IndexOf("ফেল", StringComparison.OrdinalIgnoreCase) >= 0
                        || templateName.IndexOf("ফেইল", StringComparison.OrdinalIgnoreCase) >= 0
                        || messageTemplate.IndexOf("ফেল", StringComparison.OrdinalIgnoreCase) >= 0
                        || messageTemplate.IndexOf("ফেইল", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool looksPassed = templateName.IndexOf("পাস", StringComparison.OrdinalIgnoreCase) >= 0
                        || messageTemplate.IndexOf("পাস", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (templateType == "Passed" && looksFailed && !looksPassed)
                    {
                        ShowMessage("⚠️ এটি ফেল টেমপ্লেট — Template Type: Failed (ফেল) সিলেক্ট করুন।", "warning");
                        SetOpenModalAfterPostback(true);
                        return;
                    }
                    if (templateType == "Failed" && looksPassed && !looksFailed)
                    {
                        ShowMessage("⚠️ এটি পাস টেমপ্লেট — Template Type: Passed (পাস) সিলেক্ট করুন।", "warning");
                        SetOpenModalAfterPostback(true);
                        return;
                    }
                }

                if (templateCategory == "Donor")
                {
                    bool hasPaymentMarkers = messageTemplate.Contains("{Amount}") ||
                        messageTemplate.Contains("{ReceiptNo}") || messageTemplate.Contains("{PaymentDetails}");
                    bool hasDueMarkers = messageTemplate.Contains("{TotalDue}") || messageTemplate.Contains("{DueDetails}");

                    if (templateType == "DonorDue" && hasPaymentMarkers && !hasDueMarkers)
                    {
                        ShowMessage("⚠️ {Amount}/{ReceiptNo} প্লেসহোল্ডার Donor Payment টাইপের। Template Type: Donor Payment সিলেক্ট করুন।", "warning");
                        SetOpenModalAfterPostback(true);
                        return;
                    }
                    if (templateType == "DonorPayment" && hasDueMarkers && !hasPaymentMarkers)
                    {
                        ShowMessage("⚠️ {TotalDue}/{DueDetails} প্লেসহোল্ডার Donor Due টাইপের। Template Type: Donor Due সিলেক্ট করুন।", "warning");
                        SetOpenModalAfterPostback(true);
                        return;
                    }
                }

                CurrentTemplateTypeHiddenField.Value = templateType;

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    // Check if table has TemplateCategory column
                    SqlCommand checkColumnCmd = new SqlCommand(@"
            IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = 'TemplateCategory')
     SELECT 1
      ELSE
          SELECT 0", con);

                    int columnExists = 0;
                    try
                    {
                        columnExists = SafeToInt32(checkColumnCmd.ExecuteScalar(), 0);
                    }
                    catch { columnExists = 0; }

                    SqlCommand cmd;

                    if (templateId > 0)
                    {
                        // Update existing template
                        if (columnExists == 1)
                        {
                            cmd = new SqlCommand(@"UPDATE SMS_Template 
       SET TemplateName = @TemplateName, 
             TemplateCategory = @TemplateCategory,
           TemplateType = @TemplateType, 
 MessageTemplate = @MessageTemplate, 
          IsActive = @IsActive, 
        UpdatedDate = GETDATE() 
  WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID", con);
                            cmd.Parameters.AddWithValue("@TemplateCategory", templateCategory);
                        }
                        else
                        {
                            cmd = new SqlCommand(@"UPDATE SMS_Template 
     SET TemplateName = @TemplateName, 
           TemplateType = @TemplateType, 
    MessageTemplate = @MessageTemplate, 
      IsActive = @IsActive, 
        UpdatedDate = GETDATE() 
            WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID", con);
                        }
                        cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    }
                    else
                    {
                        // Insert new template
                        if (columnExists == 1)
                        {
                            cmd = new SqlCommand(@"INSERT INTO SMS_Template 
            (SchoolID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, IsActive, CreatedDate, UpdatedDate) 
      VALUES 
 (@SchoolID, @TemplateName, @TemplateCategory, @TemplateType, @MessageTemplate, @IsActive, GETDATE(), GETDATE())", con);
                            cmd.Parameters.AddWithValue("@TemplateCategory", templateCategory);
                        }
                        else
                        {
                            cmd = new SqlCommand(@"INSERT INTO SMS_Template 
         (SchoolID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate, UpdatedDate) 
      VALUES 
           (@SchoolID, @TemplateName, @TemplateType, @MessageTemplate, @IsActive, GETDATE(), GETDATE())", con);
                        }
                    }

                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    cmd.Parameters.AddWithValue("@TemplateName", templateName);
                    cmd.Parameters.AddWithValue("@TemplateType", templateType);
                    cmd.Parameters.AddWithValue("@MessageTemplate", messageTemplate);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        string successMsg = templateId > 0 ? "✅ Template updated successfully!" : "✅ Template created successfully!";
                        if (!isActive)
                            successMsg = "⚠️ Template saved but INACTIVE — SMS-এ default message যাবে। Active Template চেক করে আবার save করুন।";
                        ShowMessage(successMsg, isActive ? "success" : "warning");
                        SyncActiveTabWithCategory(templateCategory);
                        SetOpenModalAfterPostback(false);
                        ClearForm();
                        RefreshAllGrids();
                    }
                    else
                    {
                        ShowMessage("❌ Failed to save template. Please try again.", "danger");
                        SetOpenModalAfterPostback(true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, "danger");
                SetOpenModalAfterPostback(true);
                System.Diagnostics.Debug.WriteLine("Template Save Error: " + ex.Message);
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void TemplatesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandArgument == null || string.IsNullOrWhiteSpace(e.CommandArgument.ToString()))
                return;

            int templateId = SafeToInt32(e.CommandArgument, -1);
            if (templateId <= 0)
            {
                ShowMessage("❌ Invalid template selected.", "danger");
                return;
            }

            if (e.CommandName == "EditTemplate")
            {
                LoadTemplate(templateId);
                SetOpenModalAfterPostback(true);
            }
            else if (e.CommandName == "DeleteTemplate")
            {
                DeleteTemplate(templateId);
                RefreshAllGrids();
            }
        }

        protected void TemplatesGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Can be used for custom row formatting if needed
        }

        private void LoadTemplate(int templateId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    // Check if TemplateCategory column exists
                    SqlCommand checkCmd = new SqlCommand(@"
     IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = 'TemplateCategory')
    SELECT 1
       ELSE
   SELECT 0", con);

                    int columnExists = SafeToInt32(checkCmd.ExecuteScalar(), 0);

                    string selectQuery = columnExists == 1
                        ? "SELECT TemplateID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, IsActive FROM SMS_Template WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID"
                        : "SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive FROM SMS_Template WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID";

                    SqlCommand cmd = new SqlCommand(selectQuery, con);
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    cmd.Parameters.AddWithValue("@SchoolID", GetSchoolId());

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        TemplateIDHiddenField.Value = SafeToInt32(reader["TemplateID"], 0).ToString();
                        ViewState["EditingTemplateId"] = TemplateIDHiddenField.Value;
                        TemplateNameTextBox.Text = reader["TemplateName"].ToString();

                        if (columnExists == 1 && reader["TemplateCategory"] != DBNull.Value)
                        {
                            string category = reader["TemplateCategory"].ToString();
                            TemplateCategoryDropDownList.SelectedValue = category;
                            CurrentCategoryHiddenField.Value = category;
                            UpdateTemplateTypeOptions(category);
                        }

                        string templateType = reader["TemplateType"].ToString().Trim();
                        templateType = NormalizeLegacyTemplateType(templateType);
                        string categoryForInfer = columnExists == 1 && reader["TemplateCategory"] != DBNull.Value
                            ? reader["TemplateCategory"].ToString() : "ExamResult";
                        ViewState["EditingCategory"] = categoryForInfer;
                        string inferredType = InferTemplateTypeFromContent(
                            categoryForInfer,
                            reader["TemplateName"].ToString(),
                            reader["MessageTemplate"].ToString());
                        if (string.IsNullOrEmpty(templateType))
                            templateType = inferredType;

                        CurrentTemplateTypeHiddenField.Value = templateType;
                        SelectTemplateType(templateType);
                        MessageTemplateTextBox.Text = reader["MessageTemplate"].ToString();
                        IsActiveCheckBox.Checked = SafeToBoolean(reader["IsActive"], true);

                        SyncActiveTabWithCategory(categoryForInfer);

                        FormTitleLabel.Text = "Edit Template";
                        SaveButton.Text = "Update Template";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading template: " + ex.Message, "danger");
            }
        }

        private void DeleteTemplate(int templateId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM SMS_Template WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID", con);
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    cmd.Parameters.AddWithValue("@SchoolID", GetSchoolId());

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage("✅ Template deleted successfully!", "success");
                        // Refresh all grids instead of single GridView
                        RefreshAllGrids();
                    }
                    else
                    {
                        ShowMessage("❌ Failed to delete template.", "danger");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error deleting template: " + ex.Message, "danger");
                System.Diagnostics.Debug.WriteLine("Delete Template Error: " + ex.Message);
            }
        }

        private void SelectTemplateType(string templateType)
        {
            if (string.IsNullOrEmpty(templateType))
                return;

            ListItem item = TemplateTypeDropDownList.Items.FindByValue(templateType);
            if (item != null)
                TemplateTypeDropDownList.SelectedValue = templateType;
            else
            {
                TemplateTypeDropDownList.Items.Add(new ListItem(templateType, templateType));
                TemplateTypeDropDownList.SelectedValue = templateType;
            }
        }

        private static string InferTemplateTypeFromContent(string category, string templateName, string messageTemplate)
        {
            string combined = ((templateName ?? "") + " " + (messageTemplate ?? "")).ToLowerInvariant();

            if (category == "ExamResult")
            {
                if (combined.Contains("ফেল") || combined.Contains("ফেইল") || combined.Contains("fail") || combined.Contains("alas"))
                    return "Failed";
                if (combined.Contains("পাস") || combined.Contains("pass") || combined.Contains("congrat"))
                    return "Passed";
            }
            if (category == "Donor")
            {
                if ((messageTemplate ?? "").Contains("{ReceiptNo}") || (messageTemplate ?? "").Contains("{Amount}"))
                    return "DonorPayment";
                if ((messageTemplate ?? "").Contains("{TotalDue}"))
                    return "DonorDue";
            }
            if (category == "Attendance")
            {
                string msg = messageTemplate ?? "";
                if (msg.Contains("{ExitTime}"))
                    return "Exit";
                if (msg.Contains("{LateMinutes}"))
                    return "Late";
                if (msg.Contains("{EntryTime}"))
                    return "Entry";
                if (combined.Contains("absent") || combined.Contains("অনুপস্থিত"))
                    return "Absent";
            }

            return string.Empty;
        }

        private void ClearForm()
        {
            SetOpenModalAfterPostback(false);
            TemplateIDHiddenField.Value = "0";
            ViewState["EditingTemplateId"] = 0;
            ViewState["EditingCategory"] = null;
            CurrentCategoryHiddenField.Value = string.Empty;
            CurrentTemplateTypeHiddenField.Value = string.Empty;
            TemplateNameTextBox.Text = string.Empty;
            TemplateCategoryDropDownList.SelectedIndex = 0;
            TemplateCategoryDropDownList.Enabled = true;
            UpdateTemplateTypeOptions(TemplateCategoryDropDownList.SelectedValue);
            MessageTemplateTextBox.Text = string.Empty;
            IsActiveCheckBox.Checked = true;
            PreviewLabel.Text = "মেসেজ লিখলে এখানে দেখাবে...";

            FormTitleLabel.Text = "Create New Template";
            SaveButton.Text = "Save Template";
        }

        private void ShowMessage(string message, string type)
        {
            MessageLabel.Text = message;
            MessageLabel.CssClass = $"alert alert-{type}";
            MessageLabel.Visible = true;
        }

        // Helper method for GridView
        protected string GetCategoryIcon(string category)
        {
            switch (category)
            {
                case "ExamResult": return "📝";
                case "Payment": return "💰";
                case "Attendance": return "📅";
                case "Due": return "💸";
                default: return "📝";
            }
        }

        protected string GetTemplateTypeDisplayName(string category, string templateType)
        {
            templateType = NormalizeLegacyTemplateType(templateType);

            switch (category)
            {
                case "ExamResult":
                    switch (templateType)
                    {
                        case "Passed": return "✅ পাস (Passed)";
                        case "Failed": return "❌ ফেল (Failed)";
                        default: return templateType;
                    }
                case "Payment":
                    return "💰 পেমেন্ট রিসিট (Payment)";
                case "Attendance":
                    switch (templateType)
                    {
                        case "Entry": return "✅ প্রবেশ (Entry)";
                        case "Exit": return "🚪 প্রস্থান (Exit)";
                        case "Late": return "⏰ দেরি (Late)";
                        case "LateAbs": return "⏰ দেরি (Late)";
                        case "Absent": return "❌ অনুপস্থিত (Absent)";
                        default: return templateType;
                    }
                case "Due":
                    switch (templateType)
                    {
                        case "Due": return "💸 বকেয়া নোটিফিকেশন (Due)";
                        default: return templateType;
                    }
                case "Donor":
                    switch (templateType)
                    {
                        case "DonorDue": return "💸 ডোনার বকেয়া (Donor Due)";
                        case "DonorPayment": return "✅ ডোনার পেমেন্ট (Donor Payment)";
                        default: return templateType;
                    }
                case "Admission":
                    return "🎓 ভর্তি নিশ্চিতকরণ (Confirm)";
                default:
                    return templateType;
            }
        }

        protected void AddNewTemplate_Click(object sender, EventArgs e)
{
    Button btn = (Button)sender;
    string category = btn.CommandArgument;

    // Clear form
    ClearForm();

    // Set category
    TemplateCategoryDropDownList.SelectedValue = category;
    CurrentCategoryHiddenField.Value = category;
    UpdateTemplateTypeOptions(category);
    SyncActiveTabWithCategory(category);
    ViewState["EditingCategory"] = category;
    ViewState["EditingTemplateId"] = 0;
    TemplateIDHiddenField.Value = "0";

 FormTitleLabel.Text = "Create New Template";
    SaveButton.Text = "Save Template";

    SetOpenModalAfterPostback(true);
}

private void RefreshAllGrids()
{
    ExamTemplatesGridView.DataBind();
    PaymentTemplatesGridView.DataBind();
    AttendanceTemplatesGridView.DataBind();
    DueTemplatesGridView.DataBind();
    DonorTemplatesGridView.DataBind();
    AdmissionTemplatesGridView.DataBind();
}

        private static string NormalizeLegacyTemplateType(string templateType)
        {
            switch (templateType)
            {
                case "Present": return "Entry";
                case "PaymentReminder": return "Payment";
                case "DueReminder": return "Due";
                case "DonorReminder": return "DonorDue";
                case "DonorThankYou": return "DonorPayment";
                case "AdmissionWelcome": return "AdmissionConfirm";
                default: return templateType;
            }
        }
    }
}