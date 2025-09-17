using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace EDUCATION.COM.Handeler
{
    public class Student_Photo : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            var cmd = new SqlCommand("SELECT Image from Student_Image where StudentImageID = @StudentImageID", con);
            cmd.Parameters.AddWithValue("@StudentImageID", context.Request.QueryString["SID"]);
            con.Open();
            var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            if (reader.Read())
            {
                if (reader.GetValue(0) != DBNull.Value)
                {
                    if (((byte[])reader.GetValue(0)).Length != 0)
                    {
                        context.Response.BinaryWrite((byte[])reader.GetValue(0));
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

        public bool IsReusable => false;
    }
}