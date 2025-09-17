using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission
{
    public partial class Change_Student_Subjects : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            StudentInfoFormView.DataBind();
            con.Open();
            foreach (GridViewRow row in GroupGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                SqlCommand SubjectTypecmd = new SqlCommand("SELECT SubjectType FROM StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SchoolID = @SchoolID) AND (SubjectID = @SubjectID)", con);
                SubjectTypecmd.Parameters.AddWithValue("@StudentClassID", StudentInfoFormView.DataKey["StudentClassID"].ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SubjectID", GroupGridView.DataKeys[row.DataItemIndex].Value.ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                object CheckSub = SubjectTypecmd.ExecuteScalar();

                if (CheckSub != null)
                {
                    SubjectCheckBox.Checked = true;
                    SubjectType.SelectedValue = SubjectTypecmd.ExecuteScalar().ToString();
                    row.CssClass = "selected";
                }
                else
                {
                    SubjectCheckBox.Checked = false;
                    SubjectType.SelectedIndex = 0;
                }
            }
            con.Close();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool Is_Subject_Checked = true;
            foreach (GridViewRow row in GroupGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                if (SubjectCheckBox.Checked)
                {
                    Is_Subject_Checked = false;
                }
            }

            if (Is_Subject_Checked)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Please Select Subject.')", true);
            }
            else
            {
                StudentSubjectRecordsSQL.DeleteParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
                StudentSubjectRecordsSQL.Delete();

                foreach (GridViewRow row in GroupGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        Is_Subject_Checked = false;
                        string SubjectID = GroupGridView.DataKeys[row.DataItemIndex].Value.ToString();
                        string Subject_Type = SubjectType.SelectedValue;

                        StudentSubjectRecordsSQL.InsertParameters["SubjectID"].DefaultValue = SubjectID;
                        StudentSubjectRecordsSQL.InsertParameters["SubjectType"].DefaultValue = Subject_Type;
                        StudentSubjectRecordsSQL.InsertParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
                        StudentSubjectRecordsSQL.InsertParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
                        StudentSubjectRecordsSQL.Insert();
                    }
                }
                UpdateStudentRecordIDSQL.UpdateParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
                UpdateStudentRecordIDSQL.Update();

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Subjects Assigned Successfully.')", true);
            }
        }
    }
}