using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Security;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class Institution_Details : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["SchoolID"]))
            {
                Response.Redirect("InstitutionList.aspx");
            }

            if (!IsPostBack)
            {
                LoadDueNoticeSettings();
            }
        }

        // Due Notice Settings Methods
        private void LoadDueNoticeSettings()
        {
            try
            {
                string schoolID = Request.QueryString["SchoolID"];
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TOP 1 * FROM SchoolInfo_DueNoticeSettings 
                                    WHERE SchoolID = @SchoolID AND IsEnabled = 1 
                                    ORDER BY CreatedDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", schoolID);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                EnableDueNoticeCheckBox.Checked = true;
                                HideDatePanel.Visible = true;
                                DueNoticeStatusFormView.Visible = true;

                                if (!reader.IsDBNull(reader.GetOrdinal("HideUntilDate")))
                                {
                                    DateTime hideUntilDate = reader.GetDateTime(reader.GetOrdinal("HideUntilDate"));
                                    HideUntilDateTextBox.Text = hideUntilDate.ToString("dd MMM yyyy");
                                }

                                if (!reader.IsDBNull(reader.GetOrdinal("Reason")))
                                {
                                    HideReasonTextBox.Text = reader.GetString(reader.GetOrdinal("Reason"));
                                }
                            }
                            else
                            {
                                EnableDueNoticeCheckBox.Checked = false;
                                HideDatePanel.Visible = false;
                                DueNoticeStatusFormView.Visible = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DueSettingsMessageLabel.ForeColor = System.Drawing.Color.Red;
                DueSettingsMessageLabel.Text = "Error loading settings: " + ex.Message;
            }
        }

        protected void EnableDueNoticeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            HideDatePanel.Visible = EnableDueNoticeCheckBox.Checked;
            
            if (!EnableDueNoticeCheckBox.Checked)
            {
                // Uncheck করলে সাথে সাথে database থেকে disable করে দিবে
                DisableDueNotice();
            }
            else
            {
                DueNoticeStatusFormView.Visible = false;
                DueSettingsMessageLabel.Text = "";
            }
        }

        protected void SaveDueSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                string schoolID = Request.QueryString["SchoolID"];
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                DateTime? hideUntilDate = null;
                if (!string.IsNullOrEmpty(HideUntilDateTextBox.Text.Trim()))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParseExact(HideUntilDateTextBox.Text.Trim(), 
                        new string[] { "dd MMM yyyy", "d MMM yyyy", "dd M yyyy", "d M yyyy" }, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        hideUntilDate = parsedDate;
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // First, deactivate all previous settings for this school
                    string deactivateQuery = "UPDATE SchoolInfo_DueNoticeSettings SET IsEnabled = 0 WHERE SchoolID = @SchoolID";
                    using (SqlCommand deactivateCmd = new SqlCommand(deactivateQuery, connection))
                    {
                        deactivateCmd.Parameters.AddWithValue("@SchoolID", schoolID);
                        connection.Open();
                        deactivateCmd.ExecuteNonQuery();
                        connection.Close();
                    }

                    // Insert new setting with IsEnabled = 1
                    string insertQuery = @"INSERT INTO SchoolInfo_DueNoticeSettings 
                                         (SchoolID, IsEnabled, HideUntilDate, Reason, CreatedDate, CreatedBy) 
                                         VALUES (@SchoolID, 1, @HideUntilDate, @Reason, GETDATE(), @CreatedBy)";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@SchoolID", schoolID);
                        insertCmd.Parameters.AddWithValue("@HideUntilDate", 
                            hideUntilDate.HasValue ? (object)hideUntilDate.Value : DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Reason", 
                            string.IsNullOrEmpty(HideReasonTextBox.Text.Trim()) ? (object)DBNull.Value : HideReasonTextBox.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@CreatedBy", 
                            Session["RegistrationID"] != null ? Session["RegistrationID"] : (object)DBNull.Value);

                        connection.Open();
                        insertCmd.ExecuteNonQuery();
                    }
                }

                DueSettingsMessageLabel.ForeColor = System.Drawing.Color.Green;
                DueSettingsMessageLabel.Text = "✓ সেটিংস সফলভাবে সংরক্ষিত হয়েছে!";
                
                DueNoticeStatusFormView.DataBind();
                DueNoticeStatusFormView.Visible = true;
            }
            catch (Exception ex)
            {
                DueSettingsMessageLabel.ForeColor = System.Drawing.Color.Red;
                DueSettingsMessageLabel.Text = "Error: " + ex.Message;
            }
        }

        private void DisableDueNotice()
        {
            try
            {
                string schoolID = Request.QueryString["SchoolID"];
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE SchoolInfo_DueNoticeSettings SET IsEnabled = 0 WHERE SchoolID = @SchoolID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", schoolID);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                HideUntilDateTextBox.Text = "";
                HideReasonTextBox.Text = "";
                DueNoticeStatusFormView.Visible = false;
                
                DueSettingsMessageLabel.ForeColor = System.Drawing.Color.Green;
                DueSettingsMessageLabel.Text = "✓ বকেয়া নোটিশ বন্ধ করা হয়েছে!";
            }
            catch (Exception ex)
            {
                DueSettingsMessageLabel.ForeColor = System.Drawing.Color.Red;
                DueSettingsMessageLabel.Text = "Error: " + ex.Message;
            }
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

        protected void SMSRechargeButton_Click(object sender, EventArgs e)
        {
            SMS_SQL.Insert();
            Response.Redirect(Request.Url.AbsoluteUri);
        }

        protected void DeleteIDButton_Click(object sender, EventArgs e)
        {
            DeleteStudentSQL.Delete();
            IDerrorLabel.Text = "ID Deleted Successfully";
        }

        protected void DeleteReceiptButton_Click(object sender, EventArgs e)
        {
            MoneyRSQL.Delete();
            StudentInfoFormView.DataBind();
            ReceiptFormView.DataBind();
            PaidDetailsGridView.DataBind();
        }


        protected void SNButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in Total_StudentGridView.Rows)
            {
                TextBox SessionSNTextBox = (TextBox)row.FindControl("SessionSNTextBox");
                CheckBox IsActiveCheckbox = (CheckBox)row.FindControl("IsActiveCheckbox");

                Total_StudentSQL.UpdateParameters["IsActive"].DefaultValue = IsActiveCheckbox.Checked.ToString();
                Total_StudentSQL.UpdateParameters["SN"].DefaultValue = SessionSNTextBox.Text;
                Total_StudentSQL.UpdateParameters["EducationYearID"].DefaultValue = Total_StudentGridView.DataKeys[row.RowIndex]["EducationYearID"].ToString();

                Total_StudentSQL.Update();
            }
        }

        protected void Login_Button_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as Button).Parent.Parent as GridViewRow;

            Session["Edu_Year"] = Total_StudentGridView.DataKeys[row.DataItemIndex]["EducationYearID"].ToString();
            Session["SchoolID"] = Request.QueryString["SchoolID"];
            Session["School_Name"] = SchoolFormView.DataKey["SchoolName"].ToString();

            Response.Redirect("~/Profile/Admin.aspx");
        }

        protected void IDChangeInfoFV_ItemUpdated(object sender, FormViewUpdatedEventArgs e)
        {
            Device_DataUpdateSQL.Insert();
        }
    }
}