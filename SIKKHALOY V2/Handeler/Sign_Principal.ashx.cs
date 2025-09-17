using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace EDUCATION.COM.Handeler
{
    /// <summary>
    /// Summary description for Sign_Principal
    /// </summary>
    public class Sign_Principal : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            con.Open();
            SqlCommand cmd = new SqlCommand("select Principal_Sign from SchoolInfo where SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@SchoolID", context.Request.QueryString["sign"]);
            SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            if (reader.Read())
            {
                if (reader.GetValue(0) != DBNull.Value)
                    context.Response.BinaryWrite((Byte[])reader.GetValue(0));
                else
                    context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("Default/Default_Sign.jpg")));
            }
            else
                context.Response.BinaryWrite(File.ReadAllBytes(context.Server.MapPath("Default/Default_Sign.jpg")));

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