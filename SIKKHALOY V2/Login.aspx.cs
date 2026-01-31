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
            string errorMessage = "";
            
            var usrInfo = Membership.GetUser(UserLogin2.UserName.Trim());
            if (usrInfo != null)
            {
                if (usrInfo.IsLockedOut)
                {
                    errorMessage = "<i class='fa fa-exclamation-triangle'></i> আপনার একাউন্ট লক হয়ে গেছে। অনেকবার ভুল পাসওয়ার্ড দেওয়ার কারণে আপনার একাউন্ট সাময়িকভাবে বন্ধ করা হয়েছে। অনুগ্রহ করে প্রশাসকের সাথে যোগাযোগ করুন।<br/><br/>Your account has been locked out because of too many invalid login attempts. Please contact the administrator to have your account unlocked.";
                }
                else if (!usrInfo.IsApproved)
                {
                    errorMessage = "<i class='fa fa-exclamation-triangle'></i> আপনার একাউন্ট এখনও অনুমোদিত হয়নি। প্রশাসক অনুমোদন না করা পর্যন্ত আপনি লগইন করতে পারবেন না।<br/><br/>Your account has not been approved. You cannot login until an administrator has approved your account.";
                }
                else
                {
                    errorMessage = "<i class='fa fa-times-circle'></i> ভুল পাসওয়ার্ড! অনুগ্রহ করে সঠিক পাসওয়ার্ড দিয়ে আবার চেষ্টা করুন।<br/><br/>Wrong password! Please try again with the correct password.";
                }
            }
            else
            {
                errorMessage = "<i class='fa fa-user-times'></i> এই ইউজারনেম পাওয়া যায়নি। অনুগ্রহ করে সঠিক ইউজারনেম দিয়ে আবার চেষ্টা করুন।<br/><br/>Username not found. Please try again with correct username.";
            }
            
            InvalidErrorLabel.Text = errorMessage;
            InvalidErrorLabel.CssClass = "error-message";
            InvalidErrorLabel.Visible = true;
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
                    
                    // Track Authority login - SchoolID is 0 for Authority
                    TrackUserLogin(constr, 0, Convert.ToInt32(authorityRegistrationId), UserLogin2.UserName.Trim(), "Authority");
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

                // Track School login - properly convert SchoolID
                if (!string.IsNullOrEmpty(RegistrationID) && !string.IsNullOrEmpty(SchoolID))
                {
                    int schoolIdInt = 0;
                    if (int.TryParse(SchoolID, out schoolIdInt))
                    {
                        string category = "Admin";
                        if (Roles.IsUserInRole(UserLogin2.UserName.Trim(), "Teacher"))
                            category = "Teacher";
                        else if (Roles.IsUserInRole(UserLogin2.UserName.Trim(), "Student"))
                            category = "Student";
                        
                        TrackUserLogin(constr, schoolIdInt, Convert.ToInt32(RegistrationID), UserLogin2.UserName.Trim(), category);
                    }
                }

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

                object oCommitteeMemberId;
                using (var con = new SqlConnection(constr))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(@"SELECT CM.CommitteeMemberId FROM CommitteeMember CM 
                                                      INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.UserName = CM.SmsNumber 
                                                      WHERE R.RegistrationID = @RegistrationID", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@RegistrationID", RegistrationID);
                        oCommitteeMemberId = cmd.ExecuteScalar();
                    }
                    con.Close();
                }

                if (oCommitteeMemberId != null)
                {
                    Session["CommitteeMemberID"] = oCommitteeMemberId;
                }
            }
        }

        private void TrackUserLogin(string connectionString, int schoolId, int registrationId, string userName, string category)
        {
            string logMessage = "";
            try
            {
                logMessage += $"Starting tracking for user: {userName}, Category: {category}\n";
                
                string sessionKey = Session.SessionID + "_" + DateTime.Now.Ticks;
                Session["SessionKey"] = sessionKey;
                
                logMessage += $"Session Key: {sessionKey}\n";
                logMessage += $"SchoolID: {schoolId}, RegID: {registrationId}\n";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // First, check if table exists
                    string checkTableQuery = @"
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'User_Active_Sessions')
                            SELECT 1
                        ELSE
                            SELECT 0";
                    
                    connection.Open();
                    
                    using (SqlCommand checkCmd = new SqlCommand(checkTableQuery, connection))
                    {
                        int tableExists = (int)checkCmd.ExecuteScalar();
                        logMessage += $"Table exists: {tableExists}\n";
                        
                        if (tableExists == 0)
                        {
                            logMessage += "ERROR: User_Active_Sessions table does not exist!\n";
                            throw new Exception("User_Active_Sessions table not found");
                        }
                    }

                    // Check if user already has an active session - delete old ones
                    string deleteOldQuery = @"
                        DELETE FROM User_Active_Sessions 
                        WHERE RegistrationID = @RegistrationID";
                    
                    using (SqlCommand deleteCmd = new SqlCommand(deleteOldQuery, connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                        int deletedRows = deleteCmd.ExecuteNonQuery();
                        logMessage += $"Deleted {deletedRows} old session(s)\n";
                    }

                    // Now insert - use NULL for SchoolID if it's 0 (Authority users)
                    string query = @"
                        INSERT INTO User_Active_Sessions 
                        (SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
                        VALUES (@SchoolID, @RegistrationID, @UserName, @Category, @SessionKey, GETDATE(), GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Use NULL for SchoolID if it's 0 (Authority users don't have a SchoolID)
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
                        logMessage += "SUCCESS: Session tracked successfully!\n";
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