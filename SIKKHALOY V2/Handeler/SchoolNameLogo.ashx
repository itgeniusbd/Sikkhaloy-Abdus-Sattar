<%@ WebHandler Language="C#" Class="SchoolNameLogo" %>

using System;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

public class SchoolNameLogo : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        string SchoolID = context.Request.QueryString["SchoolID"];
        
        if(string.IsNullOrEmpty(SchoolID))
        {
            context.Response.ContentType = "image/png";
            context.Response.StatusCode = 404;
            context.Response.End();
            return;
        }
        
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        
        try
        {
            con.Open();
            
            // First check if column exists
            SqlCommand checkCmd = new SqlCommand(
                @"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo]') AND name = 'SchoolNameLogo')
                  SELECT 1 ELSE SELECT 0", con);
            int columnExists = (int)checkCmd.ExecuteScalar();
            
            if (columnExists == 1)
            {
                SqlCommand cmd = new SqlCommand("SELECT SchoolNameLogo FROM SchoolInfo WHERE SchoolID = @SchoolID", con);
                cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                
                SqlDataReader dr = cmd.ExecuteReader();
                
                if (dr.Read())
                {
                    if (dr["SchoolNameLogo"] != DBNull.Value)
                    {
                        byte[] imageData = (byte[])dr["SchoolNameLogo"];
                        
                        if (imageData != null && imageData.Length > 0)
                        {
                            // Detect content type from image data
                            string contentType = GetImageContentType(imageData);
                            context.Response.ContentType = contentType;
                            context.Response.BinaryWrite(imageData);
                            context.Response.StatusCode = 200;
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
                dr.Close();
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }
        catch(Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine("Error loading school name logo: " + ex.Message);
            context.Response.StatusCode = 500;
        }
        finally
        {
            if (con.State == ConnectionState.Open)
                con.Close();
        }
        
        context.Response.End();
    }
    
    private string GetImageContentType(byte[] imageData)
    {
        if (imageData == null || imageData.Length < 4)
            return "image/png";
            
        // Check PNG signature
        if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
            return "image/png";
            
        // Check JPEG signature
        if (imageData[0] == 0xFF && imageData[1] == 0xD8)
            return "image/jpeg";
            
        // Check GIF signature
        if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
            return "image/gif";
            
        // Default to PNG
        return "image/png";
    }

    public bool IsReusable {
        get {
            return false;
        }
    }
}
