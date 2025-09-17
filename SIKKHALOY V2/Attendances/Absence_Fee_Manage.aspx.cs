using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Absence_Fee_Manage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Add_Schedule_Button_Click(object sender, EventArgs e)
        {
            ScheduleSQL.Insert();

            ScheduleTextBox.Text = "";
            StartTimeTextBox.Text = "";
            LateTimeTextBox.Text = "";
            EndTimeTextBox.Text = "";

            AddSuccessLabel.Text = "Schedule Added Successfully!";
        }

        protected void Add_Fine_Button_Click(object sender, EventArgs e)
        {
            AttendanceFineSQL.Insert();

            AmountTextBox.Text = string.Empty;
            FineForDropDownList.SelectedIndex = 0;
            AFineGridView.DataBind();
        }

        protected void Day_Time_UpdateButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in ModifyScheduleGridView.Rows)
            {
                var EStartTimeTextBox = (TextBox)row.FindControl("EStartTimeTextBox");
                var ELateEntryTimeTextBox = (TextBox)row.FindControl("ELateEntryTimeTextBox");
                var EEndTimeTextBox = (TextBox)row.FindControl("EEndTimeTextBox");
                var DayCheckBox = (CheckBox)row.FindControl("DayCheckBox");

                Schedule_Day_SQL.UpdateParameters["Day"].DefaultValue = ModifyScheduleGridView.DataKeys[row.DataItemIndex]["Day"].ToString();
                Schedule_Day_SQL.UpdateParameters["StartTime"].DefaultValue = EStartTimeTextBox.Text;
                Schedule_Day_SQL.UpdateParameters["LateEntryTime"].DefaultValue = ELateEntryTimeTextBox.Text;
                Schedule_Day_SQL.UpdateParameters["EndTime"].DefaultValue = EEndTimeTextBox.Text;
                Schedule_Day_SQL.UpdateParameters["Is_OnDay"].DefaultValue = DayCheckBox.Checked.ToString();
                Schedule_Day_SQL.Update();
            }

            ScheduleGridView.DataBind();
            ErrLabel.Text = "Successfully Updated";
        }

        protected void ScheduleGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "DayScheduleModal();", true);
        }
    }
}