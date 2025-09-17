using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class Create_delete_Sub_Admin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubAdminCreateUserWizard_CreatedUser(object sender, EventArgs e)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            Roles.AddUserToRole(SubAdminCreateUserWizard.UserName, "Sub-Admin");

            var insertRegCmd = new SqlCommand("INSERT INTO [Registration] ([SchoolID], [UserName], [Validation], [Category], [CreateDate]) VALUES (@SchoolID,@UserName, 'Valid' ,'Sub-Admin', getdate())", con);
            insertRegCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            insertRegCmd.Parameters.AddWithValue("@UserName", SubAdminCreateUserWizard.UserName);

            var registrationIdCmd = new SqlCommand("Select IDENT_CURRENT('Registration')", con);

            con.Open();
            insertRegCmd.ExecuteNonQuery();
            string RegistrationID = registrationIdCmd.ExecuteScalar().ToString();
            con.Close();

            LITQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            LITQL.InsertParameters["UserName"].DefaultValue = SubAdminCreateUserWizard.UserName;
            LITQL.InsertParameters["Password"].DefaultValue = SubAdminCreateUserWizard.Password;
            LITQL.InsertParameters["PasswordAnswer"].DefaultValue = SubAdminCreateUserWizard.Answer;
            LITQL.Insert();

            Edu_YearSQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            Edu_YearSQL.Insert();

            var adminSql = (SqlDataSource)SubAdminCreateUserWizard.CreateUserStep.ContentTemplateContainer.FindControl("AdminSQL");
            adminSql.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            adminSql.Insert();

            ViewState["RegistrationID"] = RegistrationID;
            SubAdminCreateUserWizard.ActiveStepIndex = 1;
        }

        protected void LinkAssignButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in LinkGridView.Rows)
            {
                CheckBox LinkCheckbox = row.FindControl("LinkCheckBox") as CheckBox;
                if (LinkCheckbox.Checked)
                {
                    if (Roles.RoleExists(LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString()))
                    {
                        if (!Roles.IsUserInRole(SubAdminCreateUserWizard.UserName, LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString()))
                        {
                            Roles.AddUserToRole(SubAdminCreateUserWizard.UserName, LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString());
                        }
                    }
                    Link_UserSQL.InsertParameters["RegistrationID"].DefaultValue = ViewState["RegistrationID"].ToString();
                    Link_UserSQL.InsertParameters["LinkID"].DefaultValue = LinkGridView.DataKeys[row.DataItemIndex]["LinkID"].ToString();
                    Link_UserSQL.InsertParameters["UserName"].DefaultValue = SubAdminCreateUserWizard.UserName;
                    Link_UserSQL.Insert();

                    SubAdminCreateUserWizard.ActiveStepIndex = 2;
                }
            }
        }

        protected void LinkGridView_DataBound(object sender, EventArgs e)
        {
            int RowSpan = 2;
            for (int i = LinkGridView.Rows.Count - 2; i >= 0; i--)
            {
                GridViewRow currRow = LinkGridView.Rows[i];
                GridViewRow prevRow = LinkGridView.Rows[i + 1];

                if (currRow.Cells[0].Text == prevRow.Cells[0].Text && currRow.Cells[1].Text == prevRow.Cells[1].Text)
                {
                    currRow.Cells[0].RowSpan = RowSpan;
                    prevRow.Cells[0].Visible = false;

                    currRow.Cells[1].RowSpan = RowSpan;
                    prevRow.Cells[1].Visible = false;
                    RowSpan += 1;
                }
                else
                    RowSpan = 2;

            }
        }
    }
}