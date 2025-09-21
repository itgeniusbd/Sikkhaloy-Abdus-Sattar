using System;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;

namespace EDUCATION.COM.Handeler
{
    /// <summary>
    /// Handler for retrieving teacher and principal signatures from database
    /// </summary>
    public class SignatureHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // Set response type to plain text initially for better error debugging
            context.Response.ContentType = "text/plain";
            
            try
            {
                string signType = context.Request.QueryString["type"]; // "teacher" or "principal"
                string schoolId = context.Request.QueryString["schoolId"];
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"SignatureHandler called with type: {signType}, schoolId: {schoolId}");
                
                if (string.IsNullOrEmpty(signType) || string.IsNullOrEmpty(schoolId))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("Missing required parameters: type or schoolId");
                    System.Diagnostics.Debug.WriteLine("SignatureHandler: Missing parameters");
                    return;
                }
                
                // Add cache control headers to ensure fresh images
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
                context.Response.Cache.SetNoStore();
                
                SqlConnection con = null;
                try
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"]?.ConnectionString;
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new Exception("Connection string 'EducationConnectionString' not found");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"SignatureHandler: Connection string found");
                    
                    con = new SqlConnection(connectionString);
                    con.Open();
                    
                    System.Diagnostics.Debug.WriteLine($"SignatureHandler: Database connection opened successfully");
                    
                    string column = signType.ToLower() == "teacher" ? "Teacher_Sign" : "Principal_Sign";
                    string query = $"SELECT {column} FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    
                    System.Diagnostics.Debug.WriteLine($"SignatureHandler: Executing query for column {column}, SchoolID: {schoolId}");
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        
                        object result = cmd.ExecuteScalar();
                        
                        System.Diagnostics.Debug.WriteLine($"SignatureHandler: Query executed, result type: {result?.GetType().Name ?? "null"}");
                        
                        if (result != null && result != DBNull.Value)
                        {
                            byte[] imageData = (byte[])result;
                            
                            System.Diagnostics.Debug.WriteLine($"SignatureHandler: Found signature data, length: {imageData.Length}");
                            
                            if (imageData.Length > 0)
                            {
                                // Change content type to image for successful response
                                context.Response.ContentType = "image/jpeg";
                                context.Response.BinaryWrite(imageData);
                                System.Diagnostics.Debug.WriteLine("SignatureHandler: Successfully returned signature image");
                                return;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("SignatureHandler: Image data length is 0");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"SignatureHandler: No signature data found for {signType}, SchoolID: {schoolId}");
                        }
                        
                        // No signature found - return 204 No Content
                        context.Response.StatusCode = 204; // No Content
                        context.Response.Write($"No signature available for {signType} at school {schoolId}");
                    }
                }
                catch (SqlException sqlEx)
                {
                    string errorMsg = $"SignatureHandler SQL Error: {sqlEx.Message} (Number: {sqlEx.Number})";
                    System.Diagnostics.Debug.WriteLine(errorMsg);
                    context.Response.StatusCode = 500;
                    context.Response.Write($"Database Error: {sqlEx.Message}");
                }
                finally
                {
                    if (con != null && con.State == ConnectionState.Open)
                    {
                        con.Close();
                        con.Dispose();
                        System.Diagnostics.Debug.WriteLine("SignatureHandler: Database connection closed");
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"SignatureHandler General Error: {ex.Message}\nStack Trace: {ex.StackTrace}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                context.Response.StatusCode = 500;
                context.Response.Write($"Error: {ex.Message}");
            }
        }
        
        public bool IsReusable
        {
            get { return false; }
        }
    }
}