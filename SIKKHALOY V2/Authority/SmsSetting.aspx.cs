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

                    // Set default dates for search
                    RecordsStartDateTextBox.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                    RecordsEndDateTextBox.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    
                    FailedStartDateTextBox.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                    FailedEndDateTextBox.Text = DateTime.Now.ToString("yyyy-MM-dd");

                    // Load initial data
                    SmsSenderGridView.DataBind();
                    SmsFailGridView.DataBind();
                    FailedStatsFormView.DataBind();
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
            try
            {
                SmsSettingSQL.Insert();
                
                // Show success message
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", 
                    "alert('SMS Settings updated successfully!');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", 
                    $"alert('Failed to update settings: {ex.Message}');", true);
            }
        }

        protected void SearchRecordsButton_Click(object sender, EventArgs e)
        {
            try
            {
                SmsSenderGridView.DataBind();
                
                ScriptManager.RegisterStartupScript(this, GetType(), "showRecordsTab", 
                    "$('#records-tab').tab('show');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", 
                    $"alert('Search failed: {ex.Message}');", true);
            }
        }

        protected void ClearRecordsFilterButton_Click(object sender, EventArgs e)
        {
            RecordsStartDateTextBox.Text = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            RecordsEndDateTextBox.Text = DateTime.Now.ToString("yyyy-MM-dd");
            SmsSenderGridView.DataBind();
        }

        protected void SearchFailedButton_Click(object sender, EventArgs e)
        {
            try
            {
                SmsFailGridView.DataBind();
                FailedStatsFormView.DataBind();
                
                ScriptManager.RegisterStartupScript(this, GetType(), "showFailedTab", 
                    "$('#failed-tab').tab('show');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", 
                    $"alert('Search failed: {ex.Message}');", true);
            }
        }

        protected void ClearFailedFilterButton_Click(object sender, EventArgs e)
        {
            FailedStartDateTextBox.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            FailedEndDateTextBox.Text = DateTime.Now.ToString("yyyy-MM-dd");
            FailedReasonDropDown.SelectedIndex = 0;
            InstitutionDropDown.SelectedIndex = 0;
            SmsFailGridView.DataBind();
            FailedStatsFormView.DataBind();
        }
    }
}