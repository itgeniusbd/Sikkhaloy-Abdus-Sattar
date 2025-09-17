using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;

namespace EDUCATION.COM
{
    public partial class Login1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
                Response.Redirect("~/Profile_Redirect.aspx");
        }

        protected void UserLogin_LoginError(object sender, EventArgs e)
        {
            var usrInfo = Membership.GetUser(UserLogin2.UserName.Trim());
            if (usrInfo != null)
            {
                if (usrInfo.IsLockedOut)
                {
                    UserLogin2.FailureText = "Your account has been locked out because of too many invalid login attempts. Please contact the administrator to have your account unlocked.";
                }
                else if (!usrInfo.IsApproved)
                {
                    UserLogin2.FailureText = "Your account has not been approved. You cannot login until an administrator has approved your account.";
                }
            }
            else
            {
                UserLogin2.FailureText = "Your login attempt was not successful. Please try again.";
            }
        }

        protected void UserLogin_LoggedIn(object sender, EventArgs e)
        {
            var constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            if (Roles.IsUserInRole(UserLogin2.UserName.Trim(), "Authority") || Roles.IsUserInRole(UserLogin2.UserName.Trim(), "Sub-Authority"))//for Authority
            {
                object authorityRegistrationId;
                using (var con = new SqlConnection(constr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT Authority_Info.RegistrationID FROM Authority_Info INNER JOIN Registration ON Authority_Info.RegistrationID = Registration.RegistrationID WHERE (Registration.UserName = @UserName) AND (Registration.Validation = N'Valid')", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@UserName", UserLogin2.UserName.Trim());
                        authorityRegistrationId = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (authorityRegistrationId != null)
                {
                    Session["SchoolID"] = "Authority";
                    Session["RegistrationID"] = authorityRegistrationId;
                }
                else
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Default.aspx");
                }
            }
            else
            {
                var SchoolID = string.Empty;
                var SchoolName = string.Empty;
                var RegistrationID = string.Empty;
                var EducationYearID = string.Empty;

                using (var con = new SqlConnection(constr))
                {
                    using (var cmd = new SqlCommand("SELECT Registration.SchoolID, SchoolInfo.SchoolName, Registration.RegistrationID, Education_Year_User.EducationYearID FROM  Registration INNER JOIN SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year_User ON Registration.RegistrationID = Education_Year_User.RegistrationID WHERE (Registration.UserName = @UserName)", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@UserName", UserLogin2.UserName.Trim());
                        con.Open();
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    SchoolID = dr["SchoolID"].ToString();
                                    SchoolName = dr["SchoolName"].ToString();
                                    RegistrationID = dr["RegistrationID"].ToString();
                                    EducationYearID = dr["EducationYearID"].ToString();
                                }
                            }
                        }
                        con.Close();
                    }
                }

                Session["SchoolID"] = SchoolID;
                Session["School_Name"] = SchoolName;
                Session["RegistrationID"] = RegistrationID;
                Session["Edu_Year"] = EducationYearID;

                object oSutdentId;
                using (var con = new SqlConnection(constr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT StudentID FROM Student WHERE StudentRegistrationID = @StudentRegistrationID", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@StudentRegistrationID", RegistrationID);
                        oSutdentId = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (oSutdentId != null)
                {
                    Session["StudentID"] = oSutdentId;
                    using (var con = new SqlConnection(constr))
                    {
                        using (var cmd = new SqlCommand("SELECT StudentsClass.StudentClassID,StudentsClass.ClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE  (StudentsClass.EducationYearID = @EducationYearID) AND (Student.StudentRegistrationID = @StudentRegistrationID)", con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                            cmd.Parameters.AddWithValue("@StudentRegistrationID", RegistrationID);

                            con.Open();
                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        Session["ClassID"] = dr["ClassID"].ToString();
                                        Session["StudentClassID"] = dr["StudentClassID"].ToString();
                                    }
                                }
                            }
                            con.Close();
                        }
                    }
                }

                object oTeacherId;
                using (var con = new SqlConnection(constr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT TeacherID FROM Teacher WHERE TeacherRegistrationID = @TeacherRegistrationID", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@TeacherRegistrationID", RegistrationID);
                        oTeacherId = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (oTeacherId != null)
                {
                    Session["TeacherID"] = oTeacherId;
                }
            }
        }
    }
}