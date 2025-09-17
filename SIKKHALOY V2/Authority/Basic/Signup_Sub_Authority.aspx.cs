using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Basic
{
    public partial class Signup_Sub_Authority : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void SubAdminCreateUserWizard_CreatedUser(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            Roles.AddUserToRole(SubAdminCreateUserWizard.UserName, "Sub-Authority");

            SqlCommand Insert_Reg_cmd = new SqlCommand("INSERT INTO [Registration] ([SchoolID], [UserName], [Validation], [Category]) VALUES (@SchoolID,@UserName, 'Valid' ,'Sub-Authority')", con);
            Insert_Reg_cmd.Parameters.AddWithValue("@SchoolID", 0);
            Insert_Reg_cmd.Parameters.AddWithValue("@UserName", SubAdminCreateUserWizard.UserName);

            con.Open();
            Insert_Reg_cmd.ExecuteNonQuery();
            SqlCommand RegistrationID_Cmd = new SqlCommand("Select IDENT_CURRENT('Registration')", con);

            string RegistrationID = RegistrationID_Cmd.ExecuteScalar().ToString();
            con.Close();

            LITQL.InsertParameters["SchoolID"].DefaultValue = "0";
            LITQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            LITQL.InsertParameters["UserName"].DefaultValue = SubAdminCreateUserWizard.UserName;
            LITQL.InsertParameters["Password"].DefaultValue = SubAdminCreateUserWizard.Password;
            LITQL.InsertParameters["PasswordAnswer"].DefaultValue = SubAdminCreateUserWizard.Answer;
            LITQL.Insert();


            SqlDataSource AdminSQL = (SqlDataSource)SubAdminCreateUserWizard.CreateUserStep.ContentTemplateContainer.FindControl("AdminSQL");
            AdminSQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            AdminSQL.Insert();
        }
    }
}