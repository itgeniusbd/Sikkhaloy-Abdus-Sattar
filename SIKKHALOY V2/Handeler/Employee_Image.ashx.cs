using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace EDUCATION.COM.Handeler
{
    public class Employee_Image : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // Display screens (DeviceDisplay / Attendance_Slider) request photos with cache=1 and
            // re-render every refresh; let the browser/WebView2 reuse the photo for 30 minutes
            // instead of one DB round-trip per card per refresh. Other callers are unchanged.
            if (context.Request.QueryString["cache"] == "1")
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetMaxAge(TimeSpan.FromMinutes(30));
                context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(30));
            }

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT Image FROM VW_Emp_Info WHERE EmployeeID = @EmployeeID", con);
            cmd.Parameters.AddWithValue("@EmployeeID", context.Request.QueryString["Img"]);
            SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            if (reader.Read())
            {
                if (reader.GetValue(0) != DBNull.Value)
                {
                    if (((Byte[])reader.GetValue(0)).Length != 0)
                    {
                        context.Response.BinaryWrite((Byte[])reader.GetValue(0));
                    }
                    else
                    {
                        context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("Default/Male.png")));
                    }
                }
                else
                {
                    context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("Default/Male.png")));
                }
            }
            else
                context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("Default/Male.png")));

            reader.Close();
            con.Close();
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}