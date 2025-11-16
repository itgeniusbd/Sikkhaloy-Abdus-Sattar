using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Education;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class SubmitBalance0 : System.Web.UI.Page
    {
      // Control declarations
        protected Label CurrentBalanceLabel;
 protected Label TotalSubmissionLabel;
        protected Label TotalTransactionsLabel;
        protected Label ModalBalanceLabel;
        protected TextBox SubmissionAmountTextBox;
        protected TextBox SubmissionDateTextBox;
        protected TextBox ReceivedByTextBox;
        protected TextBox ReceiverPhoneTextBox;
    protected TextBox OTPTextBox;
        protected DropDownList PaymentMethodDropDown;
        protected TextBox RemarksTextBox;
        protected Button SubmitButton;
      protected Button SendOTPButton;
    protected Button ResendOTPButton;
        protected GridView SubmissionGridView;
 protected DropDownList UserDropDown;
      protected TextBox FromDateTextBox;
        protected TextBox ToDateTextBox;
      protected HtmlGenericControl otpSection;
        protected HtmlGenericControl ErrorMsg;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Set response encoding to UTF-8
          Response.Charset = "UTF-8";
      Response.ContentEncoding = System.Text.Encoding.UTF8;

            if (!IsPostBack)
 {
    LoadSummary();
 // Set today's date in dd/MM/yyyy format
          SubmissionDateTextBox.Text = DateTime.Now.ToString("dd/MM/yyyy");
      }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            LoadSummary();
  }

        protected void SendOTPButton_Click(object sender, EventArgs e)
        {
 try
            {
 string phoneNumber = ReceiverPhoneTextBox.Text.Trim();

  // Validate phone number
  if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 11 || !phoneNumber.StartsWith("01"))
            {
            ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
       "alert('Please enter valid 11 digit phone number starting with 01');", true);
        return;
                }

                // Check cooldown period (if last OTP was sent within 60 seconds)
                DateTime? lastOTPTime = Session["BalanceSubmissionOTPTime"] as DateTime?;
     if (lastOTPTime.HasValue && DateTime.Now.Subtract(lastOTPTime.Value).TotalSeconds < 60)
    {
      double remainingSeconds = 60 - DateTime.Now.Subtract(lastOTPTime.Value).TotalSeconds;
           ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
    $"alert('Please wait {Math.Ceiling(remainingSeconds)} seconds before requesting new OTP.');", true);
          return;
                }

              // Generate 6-digit OTP
         Random random = new Random();
    string otp = random.Next(100000, 999999).ToString();

// Store OTP in Session with timestamp
     Session["BalanceSubmissionOTP"] = otp;
   Session["BalanceSubmissionOTPPhone"] = phoneNumber;
         Session["BalanceSubmissionOTPTime"] = DateTime.Now;

 // Send OTP via SMS
     SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());
     string smsText = $"Your OTP for balance submission is: {otp}. Valid for 5 minutes. - {Session["SchoolName"]}";

      Get_Validation validation = SMS.SMS_Validation(phoneNumber, smsText);

       if (validation.Validation)
      {
          Guid smsSendId = SMS.SMS_Send(phoneNumber, smsText, "Balance Submission OTP");

        if (smsSendId != Guid.Empty)
      {
          otpSection.Visible = true;
          SendOTPButton.Enabled = false;
      SendOTPButton.Text = "OTP Sent";
               
// Start timer on client side
       ScriptManager.RegisterStartupScript(this, GetType(), "OTPSentSuccess",
   "alert('OTP sent successfully to " + phoneNumber + "'); otpSent();", true);
              }
     else
      {
       ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
          "alert('Failed to send OTP. Please try again.');", true);
  }
    }
         else
       {
   ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
      $"alert('{validation.Message}');", true);
        }
}
        catch (Exception ex)
       {
     ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
             $"alert('Error sending OTP: {ex.Message}');", true);
            }
        }

 protected void ResendOTPButton_Click(object sender, EventArgs e)
        {
            try
            {
        string phoneNumber = ReceiverPhoneTextBox.Text.Trim();

   // Validate phone number
      if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 11 || !phoneNumber.StartsWith("01"))
                {
          ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
          "alert('Please enter valid 11 digit phone number starting with 01');", true);
         return;
       }

    // Check cooldown period (60 seconds)
     DateTime? lastOTPTime = Session["BalanceSubmissionOTPTime"] as DateTime?;
         if (lastOTPTime.HasValue && DateTime.Now.Subtract(lastOTPTime.Value).TotalSeconds < 60)
                {
          double remainingSeconds = 60 - DateTime.Now.Subtract(lastOTPTime.Value).TotalSeconds;
      ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
     $"alert('Please wait {Math.Ceiling(remainingSeconds)} seconds before resending OTP.');", true);
              return;
          }

                // Generate new 6-digit OTP
                Random random = new Random();
       string otp = random.Next(100000, 999999).ToString();

 // Update OTP in Session with new timestamp
                Session["BalanceSubmissionOTP"] = otp;
     Session["BalanceSubmissionOTPPhone"] = phoneNumber;
                Session["BalanceSubmissionOTPTime"] = DateTime.Now;

         // Send OTP via SMS
        SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());
       string smsText = $"Your OTP for balance submission is: {otp}. Valid for 5 minutes. - {Session["SchoolName"]}";

    Get_Validation validation = SMS.SMS_Validation(phoneNumber, smsText);

      if (validation.Validation)
              {
          Guid smsSendId = SMS.SMS_Send(phoneNumber, smsText, "Balance Submission OTP (Resend)");

            if (smsSendId != Guid.Empty)
           {
        // Clear previous OTP from textbox
       OTPTextBox.Text = "";
      
   // Start timer again
             ScriptManager.RegisterStartupScript(this, GetType(), "ResendOTPSuccess",
                "alert('New OTP sent successfully to " + phoneNumber + "'); otpSent();", true);
     }
                else
          {
        ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
         "alert('Failed to resend OTP. Please try again.');", true);
              }
      }
      else
   {
                ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
       $"alert('{validation.Message}');", true);
         }
  }
      catch (Exception ex)
            {
             ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
   $"alert('Error resending OTP: {ex.Message}');", true);
    }
        }

        private bool ValidateOTP()
     {
      try
         {
  string enteredOTP = OTPTextBox.Text.Trim();
  string sessionOTP = Session["BalanceSubmissionOTP"]?.ToString();
  string sessionPhone = Session["BalanceSubmissionOTPPhone"]?.ToString();
      DateTime? otpTime = Session["BalanceSubmissionOTPTime"] as DateTime?;

  // Check if OTP exists
     if (string.IsNullOrEmpty(sessionOTP))
      {
       ErrorMsg.InnerText = "OTP not generated. Please click 'Send OTP' button.";
      return false;
   }

         // Check if phone number matches
 if (sessionPhone != ReceiverPhoneTextBox.Text.Trim())
  {
               ErrorMsg.InnerText = "Phone number changed. Please send OTP again.";
     return false;
    }

     // Check OTP expiration (5 minutes)
       if (otpTime.HasValue && DateTime.Now.Subtract(otpTime.Value).TotalMinutes > 5)
       {
      ErrorMsg.InnerText = "OTP expired. Please send a new OTP.";
      Session.Remove("BalanceSubmissionOTP");
  Session.Remove("BalanceSubmissionOTPTime");
        Session.Remove("BalanceSubmissionOTPPhone");
            return false;
         }

                // Verify OTP
        if (enteredOTP != sessionOTP)
       {
     ErrorMsg.InnerText = "Invalid OTP. Please enter correct OTP.";
      return false;
              }

return true;
          }
            catch
            {
       return false;
  }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
  {
            try
            {
      // First validate OTP
       if (!ValidateOTP())
              {
return; // Error message already set in ValidateOTP method
                }

       int schoolID = Convert.ToInt32(Session["SchoolID"]);
                int registrationID = Convert.ToInt32(Session["RegistrationID"]);
                decimal submissionAmount = Convert.ToDecimal(SubmissionAmountTextBox.Text);

       // Parse date with proper format
    DateTime submissionDate;
      if (!DateTime.TryParseExact(SubmissionDateTextBox.Text,
            new[] { "dd/MM/yyyy", "dd/mm/yyyy", "d/M/yyyy", "dd-MM-yyyy", "dd MMM yyyy" },
     System.Globalization.CultureInfo.InvariantCulture,
 System.Globalization.DateTimeStyles.None,
   out submissionDate))
      {
         ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
          "alert('Invalid date format! Please use dd/mm/yyyy format.');", true);
      return;
         }

       string receivedBy = ReceivedByTextBox.Text.Trim();
  string paymentMethod = PaymentMethodDropDown.SelectedValue;
    string remarks = RemarksTextBox.Text.Trim();
    string receiverPhone = ReceiverPhoneTextBox.Text.Trim();

           // Check if amount is valid
      decimal currentBalance = Convert.ToDecimal(CurrentBalanceLabel.Text.Replace(",", ""));
      if (submissionAmount > currentBalance)
   {
               ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
   "alert('Submission amount cannot exceed current balance!');", true);
        return;
           }

  string insertQuery = @"
   INSERT INTO User_Balance_Submission 
 (SchoolID, RegistrationID, SubmissionAmount, SubmissionDate, ReceivedBy, ReceiverPhone, PaymentMethod, Remarks, CreatedBy)
 VALUES 
   (@SchoolID, @RegistrationID, @SubmissionAmount, @SubmissionDate, @ReceivedBy, @ReceiverPhone, @PaymentMethod, @Remarks, @CreatedBy)";

 using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
 using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
        {
    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
         cmd.Parameters.AddWithValue("@RegistrationID", registrationID);
     cmd.Parameters.AddWithValue("@SubmissionAmount", submissionAmount);
       cmd.Parameters.AddWithValue("@SubmissionDate", submissionDate);
             cmd.Parameters.AddWithValue("@ReceivedBy", string.IsNullOrEmpty(receivedBy) ? (object)DBNull.Value : receivedBy);
               cmd.Parameters.AddWithValue("@ReceiverPhone", receiverPhone);
      cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
 cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
         cmd.Parameters.AddWithValue("@CreatedBy", registrationID);

          conn.Open();
       cmd.ExecuteNonQuery();
   }
        }

  // Clear OTP session after successful submission
       Session.Remove("BalanceSubmissionOTP");
    Session.Remove("BalanceSubmissionOTPTime");
             Session.Remove("BalanceSubmissionOTPPhone");

     // Clear form
      SubmissionAmountTextBox.Text = "";
        ReceivedByTextBox.Text = "";
      ReceiverPhoneTextBox.Text = "";
       OTPTextBox.Text = "";
       RemarksTextBox.Text = "";
         SubmissionDateTextBox.Text = DateTime.Now.ToString("dd/MM/yyyy");
                otpSection.Visible = false;
           SendOTPButton.Enabled = true;
         SendOTPButton.Text = "Send OTP";

       // Reload data
       LoadSummary();
                SubmissionGridView.DataBind();

          // Show success message
    ScriptManager.RegisterStartupScript(this, GetType(), "Success", "Success();", true);
}
            catch (Exception ex)
    {
      ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
$"alert('Error: {ex.Message}');", true);
          }
        }

        private void LoadSummary()
        {
            try
            {
    int schoolID = Convert.ToInt32(Session["SchoolID"]);
        int loggedInUserID = Convert.ToInt32(Session["RegistrationID"]);
         int selectedUserID = UserDropDown.SelectedValue != null ? Convert.ToInt32(UserDropDown.SelectedValue) : 0;
  string fromDate = FromDateTextBox.Text;
           string toDate = ToDateTextBox.Text;

        // Get the RegistrationID of the selected user, or use logged-in user's ID if none selected
              int userIDForBalance = selectedUserID > 0 ? selectedUserID : loggedInUserID;

        // Load selected user's balance
    string balanceQuery = @"
SELECT 
  ISNULL(Income, 0) - ISNULL(Expense, 0) - ISNULL(Submitted, 0) AS RemainingBalance
FROM 
(
    SELECT 
    (ISNULL(EX_In_T.Other_Income, 0) + ISNULL(Stu_P_T.Student_Income, 0) + ISNULL(Com_In_T.CommitteeDonation, 0)) AS Income,
(ISNULL(Ex_T.Expenditure, 0) + ISNULL(Emp_P_T.Employee_Paid, 0)) AS Expense,
    ISNULL(Sub_T.TotalSubmitted, 0) AS Submitted
    FROM 
Registration 
    LEFT OUTER JOIN 
     (SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income 
   FROM Extra_Income WHERE SchoolID = @SchoolID GROUP BY RegistrationID) AS EX_In_T 
    ON Registration.RegistrationID = EX_In_T.RegistrationID
  LEFT OUTER JOIN 
        (SELECT RegistrationId, ISNULL(SUM(TotalAmount), 0) AS CommitteeDonation 
       FROM CommitteeMoneyReceipt WHERE SchoolId = @SchoolID GROUP BY RegistrationId) AS Com_In_T 
    ON Registration.RegistrationID = Com_In_T.RegistrationId
    LEFT OUTER JOIN 
        (SELECT RegistrationID, ISNULL(SUM(PaidAmount), 0) AS Student_Income 
    FROM Income_PaymentRecord WHERE SchoolID = @SchoolID GROUP BY RegistrationID) AS Stu_P_T 
    ON Registration.RegistrationID = Stu_P_T.RegistrationID
    LEFT OUTER JOIN 
        (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Expenditure 
       FROM Expenditure WHERE SchoolID = @SchoolID GROUP BY RegistrationID) AS Ex_T 
  ON Registration.RegistrationID = Ex_T.RegistrationID
    LEFT OUTER JOIN 
   (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Employee_Paid 
  FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID GROUP BY RegistrationID) AS Emp_P_T 
    ON Registration.RegistrationID = Emp_P_T.RegistrationID
 LEFT OUTER JOIN 
   (SELECT RegistrationID, ISNULL(SUM(SubmissionAmount), 0) AS TotalSubmitted 
  FROM User_Balance_Submission WHERE SchoolID = @SchoolID GROUP BY RegistrationID) AS Sub_T 
    ON Registration.RegistrationID = Sub_T.RegistrationID
    WHERE 
        Registration.SchoolID = @SchoolID 
   AND Registration.RegistrationID = @RegistrationID
) AS T";

          // Load summary statistics
     string summaryQuery = @"
SELECT 
    ISNULL(SUM(SubmissionAmount), 0) AS TotalSubmission,
    COUNT(*) AS TotalTransactions
FROM User_Balance_Submission
WHERE SchoolID = @SchoolID
    AND (@SelectedUserID = 0 OR RegistrationID = @SelectedUserID)
    AND (SubmissionDate >= CASE WHEN NULLIF(@FromDate, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @FromDate, 103) END)
    AND (SubmissionDate <= CASE WHEN NULLIF(@ToDate, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @ToDate, 103) END)";

      using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
         {
  conn.Open();

  // Get selected user's balance
     using (SqlCommand cmd = new SqlCommand(balanceQuery, conn))
     {
   cmd.Parameters.AddWithValue("@SchoolID", schoolID);
   cmd.Parameters.AddWithValue("@RegistrationID", userIDForBalance);

    object result = cmd.ExecuteScalar();
          decimal balance = result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
     CurrentBalanceLabel.Text = balance.ToString("N0");
           ModalBalanceLabel.Text = balance.ToString("N0");
          }

           // Get summary
         using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
         {
         cmd.Parameters.AddWithValue("@SchoolID", schoolID);
 cmd.Parameters.AddWithValue("@SelectedUserID", selectedUserID);
      cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(fromDate) ? (object)DBNull.Value : fromDate);
            cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(toDate) ? (object)DBNull.Value : toDate);

       using (SqlDataReader reader = cmd.ExecuteReader())
          {
      if (reader.Read())
    {
      decimal totalSubmission = reader["TotalSubmission"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSubmission"]) : 0;
      int totalTransactions = reader["TotalTransactions"] != DBNull.Value ? Convert.ToInt32(reader["TotalTransactions"]) : 0;

       TotalSubmissionLabel.Text = totalSubmission.ToString("N0");
        TotalTransactionsLabel.Text = totalTransactions.ToString();
         }
            }
      }
         }
            }
         catch (Exception ex)
            {
     CurrentBalanceLabel.Text = "0";
    TotalSubmissionLabel.Text = "0";
    TotalTransactionsLabel.Text = "0";
            // Log error for debugging
     System.Diagnostics.Debug.WriteLine("LoadSummary Error: " + ex.Message);
      }
        }
    }
}
