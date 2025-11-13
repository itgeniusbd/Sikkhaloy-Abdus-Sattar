using Education;
using Microsoft.Ajax.Utilities;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Money_Receipt : Page
    {
        private string CurrentMoneyReceiptID
        {
            get { return ViewState["MoneyReceiptID"]?.ToString(); }
            set { ViewState["MoneyReceiptID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["s_icD"]) || string.IsNullOrEmpty(Request.QueryString["mN_R"]))
            {
                Response.Redirect("Payment_Collection.aspx");
            }

            if (IsPostBack) return;

            try
            {
                string decryptedMoneyReceiptID = Decrypt(HttpUtility.UrlDecode(Request.QueryString["mN_R"]));
                CurrentMoneyReceiptID = decryptedMoneyReceiptID;

                StudentInfoSQL.SelectParameters["ID"].DefaultValue = Decrypt(HttpUtility.UrlDecode(Request.QueryString["s_icD"]));
                ID_DueDetailsODS.SelectParameters["ID"].DefaultValue = Decrypt(HttpUtility.UrlDecode(Request.QueryString["s_icD"]));
                MoneyRSQL.SelectParameters["MoneyReceiptID"].DefaultValue = decryptedMoneyReceiptID;
                PaidDetailsSQL.SelectParameters["MoneyReceiptID"].DefaultValue = decryptedMoneyReceiptID;
                ReceivedBySQL.SelectParameters["MoneyReceiptID"].DefaultValue = decryptedMoneyReceiptID;
            }
            catch { ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Something went wrong!')", true); }
        }

        private string Decrypt(string cipherText)
        {
            const string encryptionKey = "MAKV2SPBNI99212";
            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);

            using (var encryption = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryption.Key = pdb.GetBytes(32);
                encryption.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryption.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            var msg = "Congrats! ";
            var isSentSMS = false;

            decimal currentDue = 0.0m;
            if (DueDetailsGridView.Rows.Count > 0)
            {
                foreach (GridViewRow row in DueDetailsGridView.Rows)
                {
                    var numberLabel = row.FindControl("DueLabel") as Label;
                    if (numberLabel != null)
                    {
                        {
                            currentDue += Convert.ToDecimal(numberLabel.Text);
                        }

                    }
                }
            }
            if (StudentInfoFormView.CurrentMode == FormViewMode.ReadOnly)
            {
                var phoneNo = StudentInfoFormView.DataKey["SMSPhoneNo"].ToString();
                var studentId = StudentInfoFormView.DataKey["ID"].ToString();
                var paid = ReceiptFormView.DataKey["TotalAmount"].ToString();
                var studentName = (StudentInfoFormView.Row.FindControl("StudentsNameLabel") as Label)?.Text;
                var receiptNo = (ReceiptFormView.Row.FindControl("MoneyReceiptIDLabel") as Label)?.Text;



                if (CurrentDueCheckBox.Checked)
                {
                    msg += $"(ID: {studentId}) {studentName}. You've Paid: {paid} Tk. And Current Due: {currentDue} Tk.  Receipt No: {receiptNo}";
                }
                else
                {
                    msg += $"(ID: {studentId}) {studentName}. You've Paid: {paid} Tk. Receipt No: {receiptNo}";
                }



                if (RoleCheckBox.Checked)
                {
                    foreach (GridViewRow row in PaidDetailsGridView.Rows)
                    {
                        var role = PaidDetailsGridView.DataKeys[row.DataItemIndex]?["Role"];
                        var payFor = PaidDetailsGridView.DataKeys[row.DataItemIndex]?["PayFor"];

                        msg += $", {role}: {payFor}";
                    }
                }
                msg += ". Regards, " + Session["School_Name"];

                var sms = new SMS_Class(Session["SchoolID"].ToString());
                var smsBalance = sms.SMSBalance;
                var totalSMS = sms.SMS_Conut(msg);

                if (smsBalance >= totalSMS)
                {
                    if (sms.SMS_GetBalance() >= totalSMS)
                    {
                        var isValid = sms.SMS_Validation(phoneNo, msg);

                        if (isValid.Validation)
                        {
                            var smsSendId = sms.SMS_Send(phoneNo, msg, "Payment Collection");
                            if (smsSendId != Guid.Empty)
                            {
                                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue =
                                    Session["SchoolID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue =
                                    Session["Edu_Year"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue =
                                    StudentInfoFormView.DataKey["StudentID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";
                                SMS_OtherInfoSQL.Insert();

                            }
                            isSentSMS = true;
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
                    ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + smsBalance;
                }
            }

            if (isSentSMS)
            {
                Response.Redirect("Payment_Collection.aspx");
            }
        }

        protected void UpdatePrintedReceiptButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentMoneyReceiptID))
            {
                return;
            }

            // Get button and find the FormView
            Button btn = (Button)sender;
            FormView formView = (FormView)btn.NamingContainer;

            if (formView == null)
            {
                return;
            }

            // Get controls from FormView ItemTemplate
            TextBox printedReceiptNoTextBox = (TextBox)formView.FindControl("PrintedReceiptNoTextBox");
            Label updateMessageLabel = (Label)formView.FindControl("UpdateMessageLabel");

            if (printedReceiptNoTextBox == null || updateMessageLabel == null)
            {
                return;
            }

            string printedReceiptNo = printedReceiptNoTextBox.Text.Trim();

            if (string.IsNullOrEmpty(printedReceiptNo))
            {
                updateMessageLabel.Text = "Please enter a number!";
                updateMessageLabel.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                // Check if this Printed Receipt Number already exists for another receipt
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string checkQuery = @"SELECT COUNT(*) FROM Income_MoneyReceipt 
                                        WHERE PrintedReceiptNo = @PrintedReceiptNo 
                                        AND MoneyReceiptID != @CurrentMoneyReceiptID 
                                        AND SchoolID = @SchoolID";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@PrintedReceiptNo", printedReceiptNo);
                    checkCmd.Parameters.AddWithValue("@CurrentMoneyReceiptID", CurrentMoneyReceiptID);
                    checkCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);

                    con.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    con.Close();

                    if (count > 0)
                    {
                        updateMessageLabel.Text = "❌ This number already exists! Use a unique number.";
                        updateMessageLabel.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                }

                // If unique, proceed with update
                MoneyRSQL.UpdateParameters["PrintedReceiptNo"].DefaultValue = printedReceiptNo;
                MoneyRSQL.UpdateParameters["MoneyReceiptID"].DefaultValue = CurrentMoneyReceiptID;
                MoneyRSQL.Update();

                updateMessageLabel.Text = "✓ Updated!";
                updateMessageLabel.ForeColor = System.Drawing.Color.Green;

                // Refresh to show updated value
                ReceiptFormView.DataBind();

                // Hide message after 3 seconds
                ScriptManager.RegisterStartupScript(this, GetType(), "HideMsg",
    "setTimeout(function(){ var label = document.getElementById('" + updateMessageLabel.ClientID + "'); if(label) label.style.display='none'; }, 3000);", true);
            }
            catch (Exception ex)
            {
                updateMessageLabel.Text = "Error: " + ex.Message;
                updateMessageLabel.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}