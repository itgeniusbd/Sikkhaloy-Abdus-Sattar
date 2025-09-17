using Education;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class SendSMS : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            var committeeMemberTypeIds = new StringBuilder();
            var isTypeSelected = false;

            foreach (GridViewRow row in MemberTypeGridView.Rows)
            {
                var selectCheckBox = row.FindControl("SelectCheckBox") as CheckBox;

                if (selectCheckBox.Checked)
                {
                    committeeMemberTypeIds.Append(MemberTypeGridView.DataKeys[row.DataItemIndex]["CommitteeMemberTypeId"].ToString());
                    committeeMemberTypeIds.Append(',');
                    isTypeSelected = true;
                }
            }

            if (isTypeSelected)
            {
                var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                var sms = new SMS_Class(Session["SchoolID"].ToString());

                var msg = SMSTextBox.Text;     
                var totalSms = 0;
                var sentMgsConfirm = false;
                var sentMsgCont = 0;
                var smsBalance = sms.SMSBalance;

                #region Send SMS to All member

                con.Open();
                var smsCommand = new SqlCommand("SELECT CommitteeMemberId, SmsNumber FROM CommitteeMember WHERE (SchoolID = @SchoolID) AND (CommitteeMemberTypeId IN (select id from [dbo].[In_Function_Parameter](@CommitteeMemberTypeIds)))",con);
                smsCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                smsCommand.Parameters.AddWithValue("@CommitteeMemberTypeIds", committeeMemberTypeIds.ToString());

                var smsDr = smsCommand.ExecuteReader();

                string phoneNo;
                while (smsDr.Read())
                {
                    phoneNo = smsDr["SmsNumber"].ToString();

                    var isValid = sms.SMS_Validation(phoneNo, msg);
                    if (isValid.Validation)
                    {
                        totalSms += sms.SMS_Conut(msg);
                    }
                }

                con.Close();

                if (smsBalance >= totalSms)
                {
                    if (sms.SMS_GetBalance() >= totalSms)
                    {
                        con.Open();
                        smsDr = smsCommand.ExecuteReader();

                        while (smsDr.Read())
                        {
                            phoneNo = smsDr["SmsNumber"].ToString();
                            var isValid = sms.SMS_Validation(phoneNo, msg);

                            if (isValid.Validation)
                            {
                                var smsSendId = sms.SMS_Send(phoneNo, msg, "SMS Service");
                                if (smsSendId != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["CommitteeMemberId"].DefaultValue = smsDr["CommitteeMemberId"].ToString();

                                    SMS_OtherInfoSQL.Insert();
                                    sentMsgCont++;
                                    sentMgsConfirm = true;
                                }
                            }
                        }

                        con.Close();

                        if (sentMgsConfirm)
                        {
                            SMSTextBox.Text = string.Empty;
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage","alert('You Have Successfully Sent " + sentMsgCont.ToString() + " SMS.')", true);
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
            else
            {
                ErrorLabel.Text = "Please Select any member type";
            }

            #endregion
        }
    }
}