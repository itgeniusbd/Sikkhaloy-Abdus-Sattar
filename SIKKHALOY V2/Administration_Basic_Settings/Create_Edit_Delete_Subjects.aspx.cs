using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASIC_SETTING
{
    public partial class Create_Edit_Delete_Subjects : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void AddSubjectButton_Click(object sender, EventArgs e)
        {
            SubjectSQL.Insert();
            SubjectGridView.DataBind();
            SubjectNameTextBox.Text = string.Empty;
        }

        protected void SN_Button_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in SubjectGridView.Rows)
            {
                TextBox SN_TextBox = (TextBox)row.FindControl("SN_TextBox");

                SNSQL.UpdateParameters["SN"].DefaultValue = SN_TextBox.Text.Trim();
                SNSQL.UpdateParameters["SubjectID"].DefaultValue = SubjectGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                SNSQL.Update();
            }
            SubjectGridView.DataBind();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Subject Serial Updated')", true);
        }
    }
}