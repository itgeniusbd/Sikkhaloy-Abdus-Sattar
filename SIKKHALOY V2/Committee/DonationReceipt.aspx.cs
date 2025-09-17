using Education;
using System;
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
                if (string.IsNullOrEmpty(id)) Response.Redirect("./DonationAdd.aspx");
            }
        }


        //send sms
        protected void SMSButton_Click(object sender, EventArgs e)
        {
            if (InfoFormView.CurrentMode == FormViewMode.ReadOnly)
            {
                ErrorLabel.Text = "";
                var isSentSMS = false;

                var memberName = InfoFormView.DataKey["MemberName"].ToString();
                var mobileNumber = InfoFormView.DataKey["SmsNumber"].ToString();
                var paidAmount = InfoFormView.DataKey["TotalAmount"].ToString();
                var receiptNo = InfoFormView.DataKey["CommitteeMoneyReceiptSn"].ToString();

                var message = $"Congrats! {memberName}. You've paid: {paidAmount} tk, receipt No: {receiptNo}. Regards, {Session["School_Name"]}";

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
                            var smsSendId = sms.SMS_Send(mobileNumber, message, "donation");
                            if (smsSendId != Guid.Empty)
                            {
                                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue =
                                    Session["SchoolID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue =
                                    Session["Edu_Year"].ToString();
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
                    ErrorLabel.Text = $"You don't have sufficient SMS balance, Your Current Balance is ${smsBalance}";
                }

                if (isSentSMS)
                {
                    Response.Redirect("Donations.aspx");
                }
            }


        }
    }
}