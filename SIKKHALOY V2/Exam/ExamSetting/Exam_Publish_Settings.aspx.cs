using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.ExamSetting
{
    public partial class Exam_Publish_Settings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in ClassGridView.Rows)
            {
                var ClassID = ClassGridView.DataKeys[row.DataItemIndex]["ClassID"].ToString();
                var PublishedCheckBox = row.FindControl("PublishedCheckBox") as CheckBox;
                var LockedCheckBox = row.FindControl("LockedCheckBox") as CheckBox;

                if (ClassID != null)
                {
                    ClassSQL.UpdateParameters["ClassID"].DefaultValue = ClassID;
                    ClassSQL.UpdateParameters["IS_Published"].DefaultValue = PublishedCheckBox.Checked.ToString();
                    ClassSQL.UpdateParameters["Marks_Input_Locked"].DefaultValue = LockedCheckBox.Checked.ToString();
                    ClassSQL.Update();
                }
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
        }

        protected void CumulativeUpdate_Button_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in CumulativeClassGridView.Rows)
            {
                var ClassID = CumulativeClassGridView.DataKeys[row.DataItemIndex]["ClassID"].ToString();
                var PublishedCheckBox = row.FindControl("PublishedCheckBox") as CheckBox;

                if (ClassID != null)
                {
                    CumulativeClassSQL.UpdateParameters["ClassID"].DefaultValue = ClassID;
                    CumulativeClassSQL.UpdateParameters["IS_Published"].DefaultValue = PublishedCheckBox.Checked.ToString();
                    CumulativeClassSQL.Update();
                }
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
        }
    }
}