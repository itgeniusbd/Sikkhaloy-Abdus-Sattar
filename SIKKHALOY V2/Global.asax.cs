using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;

namespace EDUCATION.COM
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
        }

        void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Create unique session key
            string sessionKey = Session.SessionID + "_" + DateTime.Now.Ticks;
            Session["SessionKey"] = sessionKey;
            
            // Note: We'll track login in the Login page, not here
            // because Session_Start happens before authentication
        }

        void Session_End(object sender, EventArgs e)
        {
            try
            {
                // Remove session from tracking table
                string sessionKey = Session["SessionKey"] as string;
                
                if (!string.IsNullOrEmpty(sessionKey))
                {
                    RemoveSession(sessionKey);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                System.Diagnostics.Debug.WriteLine("Session_End Error: " + ex.Message);
            }
        }

        private void RemoveSession(string sessionKey)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM User_Active_Sessions WHERE SessionKey = @SessionKey";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SessionKey", sessionKey);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RemoveSession Error: " + ex.Message);
            }
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            // This runs after authentication, good place to track authenticated users
            if (User != null && User.Identity.IsAuthenticated)
            {
                TrackUserSession();
            }
        }

        private void TrackUserSession()
        {
            try
            {
                if (Session["SessionTracked"] != null)
                    return; // Already tracked this session

                string sessionKey = Session["SessionKey"] as string;
                if (string.IsNullOrEmpty(sessionKey))
                {
                    sessionKey = Session.SessionID + "_" + DateTime.Now.Ticks;
                    Session["SessionKey"] = sessionKey;
                }

                string userName = User.Identity.Name;
                
                // Get user details from database
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get user info
                    string userQuery = @"
                        SELECT TOP 1 SchoolID, RegistrationID, Category 
                        FROM Registration 
                        WHERE UserName = @UserName AND Validation = 'Valid'";

                    int? schoolId = null;
                    int? registrationId = null;
                    string category = null;

                    using (SqlCommand cmd = new SqlCommand(userQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserName", userName);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                    schoolId = reader.GetInt32(0);
                                
                                if (!reader.IsDBNull(1))
                                    registrationId = reader.GetInt32(1);
                                
                                if (!reader.IsDBNull(2))
                                    category = reader.GetString(2);
                            }
                        }
                    }

                    if (registrationId.HasValue)
                    {
                        // Insert or update session tracking
                        string insertQuery = @"
                            IF EXISTS (SELECT 1 FROM User_Active_Sessions WHERE SessionKey = @SessionKey)
                                UPDATE User_Active_Sessions 
                                SET LastActivity = GETDATE()
                                WHERE SessionKey = @SessionKey
                            ELSE
                                INSERT INTO User_Active_Sessions 
                                (SchoolID, RegistrationID, UserName, Category, SessionKey, LastActivity, LoginTime)
                                VALUES (@SchoolID, @RegistrationID, @UserName, @Category, @SessionKey, GETDATE(), GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@SchoolID", (object)schoolId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RegistrationID", registrationId.Value);
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            cmd.Parameters.AddWithValue("@Category", category ?? "");
                            cmd.Parameters.AddWithValue("@SessionKey", sessionKey);
                            
                            cmd.ExecuteNonQuery();
                        }

                        Session["SessionTracked"] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TrackUserSession Error: " + ex.Message);
            }
        }
    }
}
