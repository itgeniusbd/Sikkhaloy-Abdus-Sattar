using SmsService;
using System;
using System.Data;
using System.Web.UI;

namespace EDUCATION.COM.Authority
{
    public partial class SmsSetting : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {


                if (!Page.IsPostBack)
                {
                    var Sms = Enum.GetValues(typeof(ProviderEnum));
                    foreach (var item in Sms)
                    {
                        SmsProviderRadioButtonList.Items.Add(item.ToString());
                        SmsProviderMultipleRadioButtonList.Items.Add(item.ToString());
                    }

                    var dv = (DataView)SmsSettingSQL.Select(DataSourceSelectArguments.Empty);

                    SmsProviderRadioButtonList.Items.FindByValue(dv[0]["SmsProvider"].ToString()).Selected = true;
                    SmsProviderMultipleRadioButtonList.Items.FindByValue(dv[0]["SmsProviderMultiple"].ToString()).Selected = true;
                    SMSSendingIntervalTextBox.Text = dv[0]["SmsSendInterval"].ToString();
                    SMSProcessingUnitTextBox.Text = dv[0]["SmsProcessingUnit"].ToString();
                }
            }
            catch (Exception exception)
            {

            }
        }

        protected void SmsProviderRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SmsSettingSQL.Update();
        }

        protected void SMSSettingUpdateButton_Click(object sender, EventArgs e)
        {
            SmsSettingSQL.Insert();
        }
    }
}