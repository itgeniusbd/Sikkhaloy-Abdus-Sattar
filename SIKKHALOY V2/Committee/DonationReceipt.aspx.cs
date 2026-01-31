using Education;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationReceipt : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var id = Request.QueryString["id"];
                if (string.IsNullOrEmpty(id)) 
                    Response.Redirect("./DonationAdd.aspx");
                else
                {
                    // Explicitly bind data to controls
                    InfoFormView.DataBind();
                    PaymentGridView.DataBind();
                    SMSFormView.DataBind();
                    FooterFormView.DataBind();
                    UnpaidGridView.DataBind();
                }
            }
        }


        //send sms
        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            var isSentSMS = false;

            try
            {
                var committeeMoneyReceiptId = Request.QueryString["id"];

                if (string.IsNullOrEmpty(committeeMoneyReceiptId))
                {
                    ErrorLabel.Text = "Receipt ID is missing from URL.";
                    return;
                }

                // Get all donor and receipt information directly from database
                string memberName = "";
                string mobileNumber = "";
                string paidAmount = "";
                string receiptNo = "";
                string committeeMemberId = "";

                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                    {
                        string query = @"SELECT CommitteeMember.CommitteeMemberId, CommitteeMember.MemberName, CommitteeMember.SmsNumber, 
                                              CommitteeMoneyReceipt.TotalAmount, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn
                                       FROM CommitteeMoneyReceipt 
                                       INNER JOIN CommitteeMember ON CommitteeMoneyReceipt.CommitteeMemberId = CommitteeMember.CommitteeMemberId
                                       WHERE CommitteeMoneyReceipt.SchoolId = @SchoolId 
                                       AND CommitteeMoneyReceipt.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@SchoolId", Session["SchoolID"]);
                        cmd.Parameters.AddWithValue("@CommitteeMoneyReceiptId", committeeMoneyReceiptId);

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            committeeMemberId = reader["CommitteeMemberId"]?.ToString() ?? "";
                            memberName = reader["MemberName"]?.ToString() ?? "";
                            mobileNumber = reader["SmsNumber"]?.ToString() ?? "";
                            paidAmount = reader["TotalAmount"]?.ToString() ?? "";
                            receiptNo = reader["CommitteeMoneyReceiptSn"]?.ToString() ?? "";
                        }
                        reader.Close();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ErrorLabel.Text = "Error retrieving receipt information: " + ex.Message;
                    return;
                }

                // Validate required data
                if (string.IsNullOrEmpty(committeeMemberId))
                {
                    ErrorLabel.Text = "Donor ID is missing.";
                    return;
                }
                if (string.IsNullOrEmpty(memberName))
                {
                    ErrorLabel.Text = "Donor name is missing.";
                    return;
                }
                if (string.IsNullOrEmpty(mobileNumber))
                {
                    ErrorLabel.Text = "Mobile number is missing.";
                    return;
                }
                if (string.IsNullOrEmpty(paidAmount))
                {
                    ErrorLabel.Text = "Payment amount is missing.";
                    return;
                }
                if (string.IsNullOrEmpty(receiptNo))
                {
                    ErrorLabel.Text = "Receipt number is missing.";
                    return;
                }

                // Build payment details - get directly from database
                string paymentDetails = "";
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                    {
                        string query = @"SELECT CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description 
                                       FROM CommitteePaymentRecord 
                                       INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
                                       INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId 
                                       WHERE CommitteePaymentRecord.SchoolId = @SchoolId 
                                       AND CommitteePaymentRecord.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@SchoolId", Session["SchoolID"]);
                        cmd.Parameters.AddWithValue("@CommitteeMoneyReceiptId", committeeMoneyReceiptId);

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var category = reader["DonationCategory"]?.ToString() ?? "";
                            var description = reader["Description"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(category))
                            {
                                paymentDetails += $", {category}";
                                if (!string.IsNullOrEmpty(description))
                                {
                                    paymentDetails += $": {description}";
                                }
                            }
                        }
                        reader.Close();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error getting payment details: " + ex.Message);
                    // Continue without payment details
                }

                // Get current due for this donor
                decimal currentDue = GetDonorCurrentDue(memberName, mobileNumber);

                // Try to get Donor Payment SMS Template from database
                string donorPaymentTemplate = GetSMSTemplate("Donor", "DonorPayment");

                string message = "";
                if (!string.IsNullOrEmpty(donorPaymentTemplate))
                {
                    // Use template and replace placeholders
                    message = BuildDonorPaymentMessage(donorPaymentTemplate, memberName, Convert.ToDouble(paidAmount), 
                                                      receiptNo, paymentDetails, currentDue);
                }
                else
                {
                    // Default message if no template found
                    message = $"অভিনন্দন! {memberName}. আপনি: {paidAmount} টাকা পরিশোধ করেছেন, রিসিট নম্বর: {receiptNo}";
                    if (!string.IsNullOrEmpty(paymentDetails))
                    {
                        message += paymentDetails;
                    }
                    if (currentDue > 0)
                    {
                        message += $". বর্তমান বকেয়া: {currentDue:0.00} টাকা";
                    }
                    message += $". ধন্যবাদ, {Session["School_Name"]}";
                }

                System.Diagnostics.Debug.WriteLine($"Final Message: {message}");

                var sms = new SMS_Class(Session["SchoolID"].ToString());
                var smsBalance = sms.SMSBalance;
                var totalSMS = sms.SMS_Conut(message);

                if (smsBalance >= totalSMS)
                {
                    if (sms.SMS_GetBalance() >= totalSMS)
                    {
                        var isValid = sms.SMS_Validation(mobileNumber, message);

                        if (isValid.Validation)
                        {
                            var smsSendId = sms.SMS_Send(mobileNumber, message, "Donor Payment");
                            if (smsSendId != Guid.Empty)
                            {
                                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = null;
                                SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = null;
                                SMS_OtherInfoSQL.InsertParameters["CommitteeMemberId"].DefaultValue = committeeMemberId;
                                SMS_OtherInfoSQL.Insert();

                                isSentSMS = true;
                            }
                        }
                        else
                        {
                            ErrorLabel.Text = isValid.Message;
                        }
                    }
                    else
                    {
                        ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                    }
                }
                else
                {
                    ErrorLabel.Text = $"You don't have sufficient SMS balance, Your Current Balance is {smsBalance}";
                }

                if (isSentSMS)
                {
                    Response.Redirect("Donations.aspx");
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "Error sending SMS: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("SMS Send Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Get current due amount for a donor
        /// </summary>
        private decimal GetDonorCurrentDue(string memberName, string smsNumber)
        {
            decimal currentDue = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    string query = @"SELECT ISNULL(SUM(Due), 0) AS TotalDue 
                                   FROM CommitteeDonation 
                                   INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId 
                                   WHERE CommitteeMember.SchoolID = @SchoolID 
                                   AND CommitteeMember.MemberName = @MemberName 
                                   AND CommitteeMember.SmsNumber = @SmsNumber 
                                   AND CommitteeDonation.Due > 0
                                   AND CommitteeDonation.PromiseDate < CAST(GETDATE() AS DATE)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@MemberName", memberName);
                    cmd.Parameters.AddWithValue("@SmsNumber", smsNumber);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        currentDue = Convert.ToDecimal(result);
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine("Error getting donor current due: " + ex.Message);
            }
            return currentDue;
        }

        /// <summary>
        /// Get SMS Template from database by category and type
        /// </summary>
        private string GetSMSTemplate(string category, string templateType)
        {
            try
            {
                using (SqlConnection tempCon = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    tempCon.Open();

                    // First check if SMS_Template table exists
                    SqlCommand checkTableCmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                                   WHERE TABLE_NAME = 'SMS_Template')
                            SELECT 1
                        ELSE
                            SELECT 0", tempCon);

                    int tableExists = (int)checkTableCmd.ExecuteScalar();

                    if (tableExists == 0)
                    {
                        return string.Empty;
                    }

                    // Check if TemplateCategory column exists
                    SqlCommand checkColumnCmd = new SqlCommand(@"
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                   WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = 'TemplateCategory')
                            SELECT 1
                        ELSE
                            SELECT 0", tempCon);

                    int columnExists = (int)checkColumnCmd.ExecuteScalar();

                    string selectQuery;
                    if (columnExists == 1)
                    {
                        selectQuery = @"SELECT TOP 1 MessageTemplate 
                                      FROM SMS_Template 
                                      WHERE SchoolID = @SchoolID 
                                      AND TemplateCategory = @TemplateCategory
                                      AND TemplateType = @TemplateType 
                                      AND IsActive = 1 
                                      ORDER BY CreatedDate DESC";
                    }
                    else
                    {
                        selectQuery = @"SELECT TOP 1 MessageTemplate 
                                      FROM SMS_Template 
                                      WHERE SchoolID = @SchoolID 
                                      AND TemplateType = @TemplateType 
                                      AND IsActive = 1 
                                      ORDER BY CreatedDate DESC";
                    }

                    SqlCommand cmd = new SqlCommand(selectQuery, tempCon);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    if (columnExists == 1)
                    {
                        cmd.Parameters.AddWithValue("@TemplateCategory", category);
                    }
                    cmd.Parameters.AddWithValue("@TemplateType", templateType);

                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Build Donor Payment SMS message from template by replacing placeholders
        /// </summary>
        private string BuildDonorPaymentMessage(string template, string donorName, double amount, string receiptNo, 
                                               string paymentDetails, decimal currentDue)
        {
            string message = template;

            // Replace all donor payment-related placeholders
            message = message.Replace("{DonorName}", donorName);
            message = message.Replace("{Amount}", amount.ToString("0.00"));
            message = message.Replace("{ReceiptNo}", receiptNo);
            message = message.Replace("{CurrentDue}", currentDue.ToString("0.00"));
            
            // Clean up payment details
            if (!string.IsNullOrEmpty(paymentDetails))
            {
                paymentDetails = paymentDetails.TrimStart(',', ' ');
                message = message.Replace("{PaymentDetails}", paymentDetails);
            }
            else
            {
                message = message.Replace(", {PaymentDetails}", "")
                                .Replace("{PaymentDetails}", "");
            }
            
            message = message.Replace("{SchoolName}", Session["School_Name"].ToString());

            return message;
        }
    }
}