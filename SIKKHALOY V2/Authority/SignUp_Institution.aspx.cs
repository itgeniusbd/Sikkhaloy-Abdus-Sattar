using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;

namespace EDUCATION.COM.Authority
{
    public partial class SignUp_Institution : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void InstitutionCW_CreatedUser(object sender, EventArgs e)
        {
            string[] userName = { InstitutionCW.UserName };
            string[] role = { "Admin" };

            Roles.AddUsersToRoles(userName, role);
            ViewState["Password"] = InstitutionCW.Password;
            ViewState["PasswordAnswer"] = InstitutionCW.Answer;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            SchoolInfoSQL.InsertParameters["UserName"].DefaultValue = InstitutionCW.UserName;
            SchoolInfoSQL.Insert();

            RegistrationSQL.InsertParameters["UserName"].DefaultValue = InstitutionCW.UserName;
            RegistrationSQL.Insert();

            AdminSQL.Insert();

            var schoolInfoCmd = new SqlCommand("Select IDENT_CURRENT('SchoolInfo')", con);
            var registrationIdCmd = new SqlCommand("Select IDENT_CURRENT('Registration')", con);

            con.Open();
            var schoolId = schoolInfoCmd.ExecuteScalar().ToString();
            var registrationId = registrationIdCmd.ExecuteScalar().ToString();
            con.Close();

            LIT_SQL.InsertParameters["SchoolID"].DefaultValue = schoolId;
            LIT_SQL.InsertParameters["RegistrationID"].DefaultValue = registrationId;
            LIT_SQL.InsertParameters["UserName"].DefaultValue = InstitutionCW.UserName;
            LIT_SQL.InsertParameters["Password"].DefaultValue = ViewState["Password"].ToString();
            LIT_SQL.InsertParameters["PasswordAnswer"].DefaultValue = ViewState["PasswordAnswer"].ToString();
            LIT_SQL.Insert();

            Edu_YearSQL.InsertParameters["SchoolID"].DefaultValue = schoolId;
            Edu_YearSQL.InsertParameters["RegistrationID"].DefaultValue = registrationId;
            Edu_YearSQL.Insert();

            SMS_SQL.InsertParameters["SchoolID"].DefaultValue = schoolId;
            SMS_SQL.Insert();


            InstitutionCW.ActiveStepIndex = 2;
        }
    }
}