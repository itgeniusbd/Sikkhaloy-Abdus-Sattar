using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace EDUCATION.COM
{
    public class RejectedIDSearch : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            string prefixText = context.Request.QueryString["q"];
            string SchoolID = context.Session["SchoolID"].ToString();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT top(3) ID from Student where SchoolID = @SchoolID and ID like @ID + '%'";
                    cmd.Parameters.AddWithValue("@ID", prefixText);
                    cmd.Parameters.AddWithValue("@SchoolID", SchoolID);

                    cmd.Connection = conn;
                    StringBuilder sb = new StringBuilder();
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            sb.Append(sdr["ID"]).Append(Environment.NewLine);
                        }
                    }
                    conn.Close();

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        context.Response.Write(sb.ToString());
                    }
                    else
                    {
                        context.Response.Write(" ");
                    }
                }
            }
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