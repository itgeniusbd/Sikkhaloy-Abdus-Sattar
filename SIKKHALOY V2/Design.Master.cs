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
            
            // Write initial log to verify event is firing
            WriteLog($"[Design.Master] UserLogin_LoggedIn event fired for user: {UserLogin.UserName.Trim()}", false);

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
                    
                    WriteLog($"[Design.Master] Authority user detected, calling TrackUserLogin", false);
                    
                    // Track Authority login - SchoolID is 0 for Authority
                    TrackUserLogin(constr, 0, Convert.ToInt32(Authority_RegistrationID), UserLogin.UserName.Trim(), "Authority");
                }
                else
                {
                    WriteLog($"[Design.Master] Authority_RegistrationID is null, signing out", true);
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

                WriteLog($"[Design.Master] School user detected: SchoolID={SchoolID}, RegID={RegistrationID}", false);

                // Track School login - properly convert SchoolID
                if (!string.IsNullOrEmpty(RegistrationID) && !string.IsNullOrEmpty(SchoolID))
                {
                    int schoolIdInt = 0;
                    if (int.TryParse(SchoolID, out schoolIdInt))
                    {
                        string category = "Admin";
                        if (Roles.IsUserInRole(UserLogin.UserName.Trim(), "Teacher"))
                            category = "Teacher";
                        else if (Roles.IsUserInRole(UserLogin.UserName.Trim(), "Student"))
                            category = "Student";
                        
                        WriteLog($"[Design.Master] Calling TrackUserLogin with category={category}", false);
                        
                        TrackUserLogin(constr, schoolIdInt, Convert.ToInt32(RegistrationID), UserLogin.UserName.Trim(), category);
                    }
                    else
                    {
                        WriteLog($"[Design.Master] Failed to parse SchoolID: {SchoolID}", true);
                    }
                }
                else
                {
                    WriteLog($"[Design.Master] RegistrationID or SchoolID is empty", true);
                }

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
            
            WriteLog($"[Design.Master] UserLogin_LoggedIn completed for user: {UserLogin.UserName.Trim()}", false);
        }
        
        private void TrackUserLogin(string connectionString, int schoolId, int registrationId, string userName, string category)
        {
            string logMessage = "";
            try
            {
                logMessage += $"[Design.Master] Starting tracking for user: {userName}, Category: {category}\n";
                
                string sessionKey = Session.SessionID + "_" + DateTime.Now.Ticks;
                Session["SessionKey"] = sessionKey;
                
                logMessage += $"Session Key: {sessionKey}\n";
                logMessage += $"SchoolID: {schoolId}, RegID: {registrationId}\n";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    logMessage += "Database connection opened\n";

                    // Delete old sessions for this user
                    string deleteOldQuery = "DELETE FROM User_Active_Sessions WHERE RegistrationID = @RegistrationID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteOldQuery, connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                        int deletedRows = deleteCmd.ExecuteNonQuery();
                        logMessage += $"Deleted {deletedRows} old session(s)\n";
                    }

                    // Insert new session
                    string query = @"
                        INSERT INTO User_Active_Sessions 
                        (SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
                        VALUES (@SchoolID, @RegistrationID, @UserName, @Category, @SessionKey, GETDATE(), GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Use NULL for SchoolID if it's 0 (Authority users)
                        if (schoolId == 0)
                        {
                            command.Parameters.AddWithValue("@SchoolID", DBNull.Value);
                            logMessage += "Using NULL for SchoolID (Authority user)\n";
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@SchoolID", schoolId);
                            logMessage += $"Using SchoolID: {schoolId}\n";
                        }
                        
                        command.Parameters.AddWithValue("@RegistrationID", registrationId);
                        command.Parameters.AddWithValue("@UserName", userName);
                        command.Parameters.AddWithValue("@Category", category);
                        command.Parameters.AddWithValue("@SessionKey", sessionKey);

                        int rowsAffected = command.ExecuteNonQuery();
                        logMessage += $"Rows inserted: {rowsAffected}\n";
                        logMessage += "SUCCESS: Session tracked successfully from Design.Master!\n";
                    }
                }
                
                // Write success log
                WriteLog(logMessage, false);
            }
            catch (Exception ex)
            {
                logMessage += $"ERROR: {ex.Message}\n";
                logMessage += $"Stack Trace: {ex.StackTrace}\n";
                if (ex.InnerException != null)
                {
                    logMessage += $"Inner Exception: {ex.InnerException.Message}\n";
                }
                
                // Write error log
                WriteLog(logMessage, true);
                
                // Don't throw - let login continue
                System.Diagnostics.Debug.WriteLine("TrackUserLogin Error: " + ex.Message);
            }
        }
        
        private void WriteLog(string message, bool isError)
        {
            try
            {
                string logPath = Server.MapPath("~/App_Data/session_tracking_log.txt");
                string logDir = System.IO.Path.GetDirectoryName(logPath);
                
                if (!System.IO.Directory.Exists(logDir))
                {
                    System.IO.Directory.CreateDirectory(logDir);
                }
                
                string logEntry = $"\n========== {DateTime.Now:yyyy-MM-dd HH:mm:ss} {(isError ? "ERROR" : "SUCCESS")} ==========\n";
                logEntry += message;
                logEntry += "=".PadRight(60, '=') + "\n";
                
                System.IO.File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignore log writing errors
            }
        }
    }
}