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
            if (!IsPostBack)
            {
                UpdateTemplateTypeOptions("ExamResult");
            }
        }

        protected void TemplateCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTemplateTypeOptions(TemplateCategoryDropDownList.SelectedValue);
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
                    TemplateTypeDropDownList.Items.Add(new ListItem("Payment Receipt (পেমেন্ট রিসিট)", "Payment"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("Payment Reminder (পেমেন্ট রিমাইডার)", "PaymentReminder"));
                    break;

                case "Attendance":
                    TemplateTypeDropDownList.Items.Add(new ListItem("✅ Entry - School Entry (স্কুলে প্রবেশ)", "Entry"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("🚪 Exit - School Exit (স্কুল ত্যাগ)", "Exit"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("⏰ Late - Late Entry (দেরিতে আসা)", "Late"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("❌ Absent - Absent from School (অনুপস্থিত)", "Absent"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("⚠️ Late Abs - Late Absent (দেরিতে + অনুপস্থিত)", "LateAbs"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("✔️ Present - Regular Present (উপস্থিত)", "Present"));
                    break;

                case "Due":
                    TemplateTypeDropDownList.Items.Add(new ListItem("Due Notification (বকেয়া নোটিফিকেশন)", "Due"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("Due Reminder (বকেয়া রিমাইন্ডার)", "DueReminder"));
                    break;

                case "Admission":
                    TemplateTypeDropDownList.Items.Add(new ListItem("Admission Confirmation (ভর্তি নিশ্চিতকরণ)", "AdmissionConfirm"));
                    TemplateTypeDropDownList.Items.Add(new ListItem("Admission Welcome (স্বাগতম বার্তা)", "AdmissionWelcome"));
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
                return;
            }

            try
            {
                int templateId = Convert.ToInt32(TemplateIDHiddenField.Value);
                string templateName = TemplateNameTextBox.Text.Trim();
                string templateCategory = TemplateCategoryDropDownList.SelectedValue;
                string templateType = TemplateTypeDropDownList.SelectedValue;
                string messageTemplate = MessageTemplateTextBox.Text.Trim();
                bool isActive = IsActiveCheckBox.Checked;
                int schoolId = Convert.ToInt32(Session["SchoolID"]);

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
                        columnExists = (int)checkColumnCmd.ExecuteScalar();
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
                        ShowMessage(templateId > 0 ? "✅ Template updated successfully!" : "✅ Template created successfully!", "success");
                        ClearForm();
                        RefreshAllGrids();
                        
                        // Close modal and show success message - NO PAGE RELOAD
   string script = @"
 $('#templateModal').modal('hide');
    $('.alert-success').fadeIn();
        setTimeout(function() { 
         $('.alert-success').fadeOut(); 
     }, 3000);
          ";
    Page.ClientScript.RegisterStartupScript(this.GetType(), "CloseModalSuccess", script, true);
                    }
                    else
                    {
                        ShowMessage("❌ Failed to save template. Please try again.", "danger");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, "danger");
                System.Diagnostics.Debug.WriteLine("Template Save Error: " + ex.Message);
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void TemplatesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int templateId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditTemplate")
            {
                LoadTemplate(templateId);
                Page.ClientScript.RegisterStartupScript(this.GetType(), "ShowModal", "showTemplateModal();", true);
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

                    int columnExists = (int)checkCmd.ExecuteScalar();

                    string selectQuery = columnExists == 1
                        ? "SELECT TemplateID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, IsActive FROM SMS_Template WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID"
                        : "SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive FROM SMS_Template WHERE TemplateID = @TemplateID AND SchoolID = @SchoolID";

                    SqlCommand cmd = new SqlCommand(selectQuery, con);
                    cmd.Parameters.AddWithValue("@TemplateID", templateId);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        TemplateIDHiddenField.Value = reader["TemplateID"].ToString();
                        TemplateNameTextBox.Text = reader["TemplateName"].ToString();

                        if (columnExists == 1 && reader["TemplateCategory"] != DBNull.Value)
                        {
                            string category = reader["TemplateCategory"].ToString();
                            TemplateCategoryDropDownList.SelectedValue = category;
                            UpdateTemplateTypeOptions(category);
                        }

                        TemplateTypeDropDownList.SelectedValue = reader["TemplateType"].ToString();
                        MessageTemplateTextBox.Text = reader["MessageTemplate"].ToString();
                        IsActiveCheckBox.Checked = Convert.ToBoolean(reader["IsActive"]);

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
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

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

        private void ClearForm()
        {
            TemplateIDHiddenField.Value = "0";
            TemplateNameTextBox.Text = string.Empty;
            TemplateCategoryDropDownList.SelectedIndex = 0;
            UpdateTemplateTypeOptions(TemplateCategoryDropDownList.SelectedValue);
            MessageTemplateTextBox.Text = string.Empty;
            IsActiveCheckBox.Checked = true;
            PreviewLabel.Text = "Template preview will appear here...";

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

 FormTitleLabel.Text = "Create New Template";
    SaveButton.Text = "Save Template";

    // Show modal
    Page.ClientScript.RegisterStartupScript(this.GetType(), "ShowModal", "showTemplateModal();", true);
}

private void RefreshAllGrids()
{
    ExamTemplatesGridView.DataBind();
    PaymentTemplatesGridView.DataBind();
    AttendanceTemplatesGridView.DataBind();
    DueTemplatesGridView.DataBind();
    AdmissionTemplatesGridView.DataBind();
}
    }
}