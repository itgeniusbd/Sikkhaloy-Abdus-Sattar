using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM
{
    public partial class Design : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["Invalid"]))
            {
                InvalidErrorLabel.Text = Request.QueryString["Invalid"].ToString();
                Button LoginButton = (Button)UserLogin.FindControl("LoginButton");
                LoginButton.Enabled = false;
            }


            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                if (Session["SchoolID"] == null)
                {
                    string[] myCookies = Request.Cookies.AllKeys;
                    foreach (string cookie in myCookies)
                    {
                        Response.Cookies[cookie].Expires = DateTime.Now;
                    }

                    Roles.DeleteCookie();
                    Session.Clear();
                    FormsAuthentication.SignOut();
                }
            }

        }

        protected void LoginStatus1_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now;
            }

            Roles.DeleteCookie();
            Session.Clear();
            FormsAuthentication.SignOut();
        }

        protected void UserLogin_LoginError(object sender, EventArgs e)
        {
            MembershipUser usrInfo = Membership.GetUser(UserLogin.UserName.Trim());
            if (usrInfo != null)
            {
                if (usrInfo.IsLockedOut)
                {
                    UserLogin.FailureText = "Your account has been locked out because of too many invalid login attempts. Please contact the administrator to have your account unlocked.";
                }
                else if (!usrInfo.IsApproved)
                {
                    UserLogin.FailureText = "Your account has not been approved. You cannot login until an administrator has approved your account.";
                }
            }
            else
            {
                UserLogin.FailureText = "Your login attempt was not successful. Please try again.";
            }
        }

        protected void UserLogin_LoggedIn(object sender, EventArgs e)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            if (Roles.IsUserInRole(UserLogin.UserName.Trim(), "Authority") || Roles.IsUserInRole(UserLogin.UserName.Trim(), "Sub-Authority"))//for Authority
            {
                object Authority_RegistrationID;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Authority_Info.RegistrationID FROM Authority_Info INNER JOIN Registration ON Authority_Info.RegistrationID = Registration.RegistrationID WHERE (Registration.UserName = @UserName) AND (Registration.Validation = N'Valid')", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@UserName", UserLogin.UserName.Trim());
                        Authority_RegistrationID = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (Authority_RegistrationID != null)
                {
                    Session["SchoolID"] = "Authority";
                    Session["RegistrationID"] = Authority_RegistrationID;
                }
                else
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Default.aspx");
                }
            }
            else
            {
                string SchoolID = string.Empty;
                string SchoolName = string.Empty;
                string RegistrationID = string.Empty;
                string EducationYearID = string.Empty;

                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Registration.SchoolID, SchoolInfo.SchoolName, Registration.RegistrationID, Education_Year_User.EducationYearID FROM  Registration INNER JOIN SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year_User ON Registration.RegistrationID = Education_Year_User.RegistrationID WHERE (Registration.UserName = @UserName)", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@UserName", UserLogin.UserName.Trim());
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
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

                object O_SutdentID;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT StudentID FROM Student WHERE StudentRegistrationID = @StudentRegistrationID", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@StudentRegistrationID", RegistrationID);
                        O_SutdentID = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (O_SutdentID != null)
                {
                    Session["StudentID"] = O_SutdentID;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT StudentsClass.StudentClassID,StudentsClass.ClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE  (StudentsClass.EducationYearID = @EducationYearID) AND (Student.StudentRegistrationID = @StudentRegistrationID)", con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                            cmd.Parameters.AddWithValue("@StudentRegistrationID", RegistrationID);

                            con.Open();
                            using (SqlDataReader dr = cmd.ExecuteReader())
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

                object O_TeacherID;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT TeacherID FROM Teacher WHERE TeacherRegistrationID = @TeacherRegistrationID", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@TeacherRegistrationID", RegistrationID);
                        O_TeacherID = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (O_TeacherID != null)
                {
                    Session["TeacherID"] = O_TeacherID;
                }
            }
        }
    }
}