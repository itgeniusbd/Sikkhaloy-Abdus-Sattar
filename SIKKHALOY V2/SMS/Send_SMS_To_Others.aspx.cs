using Education;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.SMS
{
    public partial class Send_SMS_To_Others : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void GroupNameDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupNameDropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }
        protected void SelectGroupDropDownList_DataBound(object sender, EventArgs e)
        {
            SelectGroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "0"));
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            AddGroupSQL.Insert();
            GroupGridView.DataBind();
            GroupName.Text = string.Empty;
        }
        protected void AddButton_Click(object sender, EventArgs e)
        {
            Group_Phone_NumberSQL.Insert();
            PersonNameTextBox.Text = string.Empty;
            MobileNumberTextBox.Text = string.Empty;
            AddressTextBox.Text = string.Empty;
            MsgLabel.Text = "Contact Successfully Added";
            SelectGroupDropDownList.SelectedIndex = 0;
        }


        protected void SendSMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            string Msg = SMSTextBox.Text;
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            bool SentMgsConfirm = false;
            int SentMsgCont = 0;
            int FailedMsgCont = 0;

            int SMSBalance = SMS.SMSBalance;

            foreach (GridViewRow ROW in ContactListGridView.Rows)
            {
                CheckBox SelectCheckbox = (CheckBox)ROW.FindControl("SelectCheckBox");
                Label MobileNoLabel = (Label)ROW.FindControl("MobileNoLabel");
                if (SelectCheckbox.Checked)
                {
                    PhoneNo = MobileNoLabel.Text;

                    Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                    if (IsValid.Validation)
                    {
                        TotalSMS += SMS.SMS_Conut(Msg);
                    }
                }
            }

            if (SMSBalance >= TotalSMS)
            {
                if (SMS.SMS_GetBalance() >= TotalSMS)
                {
                    foreach (GridViewRow ROW in ContactListGridView.Rows)
                    {
                        CheckBox SelectCheckbox = (CheckBox)ROW.FindControl("SelectCheckBox");
                        Label MobileNoLabel = (Label)ROW.FindControl("MobileNoLabel");
                        if (SelectCheckbox.Checked)
                        {

                            PhoneNo = MobileNoLabel.Text;

                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Others SMS");
                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SMS_NumberID"].DefaultValue = ContactListGridView.DataKeys[ROW.DataItemIndex % ContactListGridView.PageSize]["SMS_NumberID"].ToString();

                                    SMS_OtherInfoSQL.Insert();
                                    SentMsgCont++;
                                    SentMgsConfirm = true;
                                }
                                else
                                {
                                    ROW.BackColor = System.Drawing.Color.Red;
                                    FailedMsgCont++;
                                }
                            }
                            else
                            {
                                ROW.BackColor = System.Drawing.Color.Red;
                                FailedMsgCont++;
                            }
                        }
                    }

                    if (SentMgsConfirm)
                    {
                        SMSTextBox.Text = string.Empty;
                        SMSFormView.DataBind();
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('You Have Successfully Sent " + SentMsgCont.ToString() + " SMS And Failed " + FailedMsgCont.ToString() + ".')", true);
                    }
                }
                else
                {
                    ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                }
            }
            else
            {
                ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + SMSBalance;
            }
        }

        protected void Group_Phone_NumberSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            IteamCountLabel.Text = "Total: " + e.AffectedRows.ToString() + " Contact.";
        }
    }
}