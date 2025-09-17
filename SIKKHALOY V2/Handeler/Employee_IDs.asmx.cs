using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace EDUCATION.COM.Handeler
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class Employee_IDs : WebService
    {
        [WebMethod(EnableSession = true)]
        public string GetEmployeeID(string ids)
        {
            List<string> Empid = new List<string>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT top(5) ID FROM Employee_Info WHERE SchoolID = @SchoolID AND Job_Status = 'Active' AND ID like @ID + '%'";
                    cmd.Parameters.AddWithValue("@ID", ids);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        Empid.Add(dr["ID"].ToString());
                    }
                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(Empid);
                    return json;
                }
            }
        }
    }
}
