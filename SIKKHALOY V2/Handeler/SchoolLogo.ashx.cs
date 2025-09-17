using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace Education.Handeler
{
    /// <summary>
    /// Summary description for SchoolLogo
    /// </summary>
    public class SchoolLogo : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            con.Open();
            SqlCommand cmd = new SqlCommand("select SchoolLogo from SchoolInfo where SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@SchoolID", context.Request.QueryString["SLogo"]);
            SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            if (reader.Read())
            {
                if (reader.GetValue(0) != DBNull.Value)
                    context.Response.BinaryWrite((Byte[])reader.GetValue(0));
                else
                    context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("~/CSS/Image/Defualt_image.png")));
            }
            else
                context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("~/CSS/Image/Defualt_image.png")));

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