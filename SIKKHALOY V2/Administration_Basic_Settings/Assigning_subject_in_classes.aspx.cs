using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASIC_SETTING
{
    public partial class Assigning_subject_in_classes : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GroupNameDropDownList.DataBind();
                GroupNameDropDownList.Visible = false;
                GroupValidator.Enabled = false;
            }
        }

        protected void ClassnameDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupNameDropDownList.DataBind();
                GroupNameDropDownList.Visible = false;
                GroupValidator.Enabled = false;
            }
            else
            {
                GroupGridView.DataBind();
                GroupNameDropDownList.Visible = true;
                GroupValidator.Enabled = true;
            }


            con.Open();
            foreach (GridViewRow row in GroupGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                SqlCommand SubjectTypecmd = new SqlCommand("select SubjectType from SubjectForGroup where ClassID = @ClassID and SubjectID = @SubjectID and SubjectGroupID = @SubjectGroupID", con);
                SubjectTypecmd.Parameters.AddWithValue("@ClassID", ClassnameDropDownList.SelectedValue);
                SubjectTypecmd.Parameters.AddWithValue("@SubjectID", GroupGridView.DataKeys[row.DataItemIndex].Value.ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SubjectGroupID", GroupNameDropDownList.SelectedValue);

                Object CheckSub = SubjectTypecmd.ExecuteScalar();

                if (CheckSub != null)
                {
                    SubjectCheckBox.Checked = true;
                    SubjectType.SelectedValue = SubjectTypecmd.ExecuteScalar().ToString();
                    row.CssClass = "selected";
                }
                else
                {
                    SubjectCheckBox.Checked = false;
                    SubjectType.SelectedIndex = -1;
                    row.CssClass = "Deselected";
                }
            }
            con.Close();
        }

        protected void GroupNameDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            con.Open();
            foreach (GridViewRow row in GroupGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                SqlCommand SubjectTypecmd = new SqlCommand("select SubjectType from SubjectForGroup where ClassID = @ClassID and SubjectID = @SubjectID and SubjectGroupID = @SubjectGroupID", con);
                SubjectTypecmd.Parameters.AddWithValue("@ClassID", ClassnameDropDownList.SelectedValue);
                SubjectTypecmd.Parameters.AddWithValue("@SubjectID", GroupGridView.DataKeys[row.DataItemIndex].Value.ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SubjectGroupID", GroupNameDropDownList.SelectedValue);

                Object CheckSub = SubjectTypecmd.ExecuteScalar();

                if (CheckSub != null)
                {
                    SubjectCheckBox.Checked = true;
                    SubjectType.SelectedValue = SubjectTypecmd.ExecuteScalar().ToString();
                    row.CssClass = "selected";
                }
                else
                {
                    SubjectCheckBox.Checked = false;
                    SubjectType.SelectedIndex = -1;
                    row.CssClass = "Deselected";
                }
            }
            con.Close();
        }

        protected void GroupNameDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupNameDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "0"));
        }

        protected void AddSubJectGroupButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool Is_Subject_Checked = true;
                bool Is_SubjectType_Checked = true;
                foreach (GridViewRow row in GroupGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        Is_Subject_Checked = false;

                        //If SubjectType not Selected
                        if (SubjectType.SelectedIndex == -1)
                        {
                            Is_SubjectType_Checked = false;
                        }
                    }
                }

                if (Is_Subject_Checked)
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Select Subject.')", true);
                }
                else
                {
                    if (Is_SubjectType_Checked)
                    {
                        CreateGroupSQL.Delete();
                        foreach (GridViewRow row in GroupGridView.Rows)
                        {
                            CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                            RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
                            if (SubjectCheckBox.Checked)
                            {
                                Is_Subject_Checked = false;
                                string SubjectID = GroupGridView.DataKeys[row.DataItemIndex].Value.ToString();
                                string Subject_Type = SubjectType.SelectedValue;

                                CreateGroupSQL.InsertParameters["SubjectID"].DefaultValue = SubjectID;
                                CreateGroupSQL.InsertParameters["SubjectType"].DefaultValue = Subject_Type;

                                CreateGroupSQL.Insert();
                                row.CssClass = "selected";
                            }
                            else
                            {
                                SubjectType.SelectedIndex = -1;
                                row.CssClass = "Deselected";
                            }
                        }

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Subjects Assigned Successfully.')", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Please Select Subject Type.')", true);
                    }
                }
            }
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('System error.')", true);
            }
        }

        protected void DeletButton_Click(object sender, EventArgs e)
        {
            if (ClassnameDropDownList.SelectedIndex > 0)
            {
                CreateGroupSQL.Delete();
                GroupGridView.DataBind();
            }
        }
    }
}