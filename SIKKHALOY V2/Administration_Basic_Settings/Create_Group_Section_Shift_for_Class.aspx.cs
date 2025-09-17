using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASICSETTING
{
    public partial class Create_Group_Section_Shift_for_Class : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            ClErrorLabel.Text = "";

            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["Msg"]))
                {
                    QMsgLabel.Text = Request.QueryString["Msg"];
                }
            }
            else { QMsgLabel.Text = ""; }
        }

        //Add Class
        protected void ClassAddButton_Click(object sender, EventArgs e)
        {
            CreateClassSQL.Insert();
            ClassnameTextBox.Text = string.Empty;
            ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
            ClassGridView.DataBind();
        }
        protected void ClassGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            if (e.Exception != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Class already used!')", true);
                e.ExceptionHandled = true;
            }
        }

        //Add Group,Section,Shift
        protected void GroupButton_Click(object sender, EventArgs e)
        {
            if (ClassDropDownList.SelectedIndex > 0)
            {
                SubjectGroupSQL.Insert();
                SqlCommand SubjectGroup = new SqlCommand("Select IDENT_CURRENT('CreateSubjectGroup')", con);

                con.Open();
                GroupJoinSQL.InsertParameters["SubjectGroupID"].DefaultValue = SubjectGroup.ExecuteScalar().ToString();
                con.Close();

                GroupJoinSQL.Insert();
                GroupGridView.DataBind();

                SubjectGroupTextBox.Text = string.Empty;
            }
            else
                ClErrorLabel.Text = "Select Class";
        }
        protected void SectionButton_Click(object sender, EventArgs e)
        {
            if (ClassDropDownList.SelectedIndex > 0)
            {
                SectionSQL.Insert();
                SqlCommand SectionID = new SqlCommand("Select IDENT_CURRENT('CreateSection')", con);

                con.Open();
                SectionJoinSQL.InsertParameters["SectionID"].DefaultValue = SectionID.ExecuteScalar().ToString();
                con.Close();

                SectionJoinSQL.Insert();
                SectionGridView.DataBind();

                SectionTextBox.Text = string.Empty;
            }
            else
                ClErrorLabel.Text = "Select Class";
        }
        protected void ShiftButton_Click(object sender, EventArgs e)
        {
            if (ClassDropDownList.SelectedIndex > 0)
            {
                ShiftSQL.Insert();
                SqlCommand SectionID = new SqlCommand("Select IDENT_CURRENT('CreateShift')", con);

                con.Open();
                ShiftJoinSQL.InsertParameters["ShiftID"].DefaultValue = SectionID.ExecuteScalar().ToString();
                con.Close();

                ShiftJoinSQL.Insert();
                ShiftGridView.DataBind();

                ShiftTextBox.Text = string.Empty;
            }
            else
                ClErrorLabel.Text = "Select Class";
        }

        protected void SN_Button_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in ClassGridView.Rows)
            {
                TextBox SN_TextBox = (TextBox)row.FindControl("SN_TextBox");

                UpdateSN_SQL.UpdateParameters["SN"].DefaultValue = SN_TextBox.Text.Trim();
                UpdateSN_SQL.UpdateParameters["ClassID"].DefaultValue = ClassGridView.DataKeys[row.DataItemIndex]["ClassID"].ToString();
                UpdateSN_SQL.Update();
            }
            ClassGridView.DataBind();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Class Serial Updated')", true);
        }
    }
}