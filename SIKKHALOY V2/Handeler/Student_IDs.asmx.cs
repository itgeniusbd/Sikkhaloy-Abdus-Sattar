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
    public class Student_IDs : WebService
    {
        [WebMethod(EnableSession = true)]
        public string GetStudentID(string ids)
        {
            List<string> StudentId = new List<string>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT top(5) Student.ID FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE Student.Status = 'Active' AND StudentsClass.SchoolID = @SchoolID AND StudentsClass.EducationYearID = @EducationYearID AND Student.ID like @ID + '%'";
                    cmd.Parameters.AddWithValue("@ID", ids);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        StudentId.Add(dr["ID"].ToString());
                    }
                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(StudentId);
                    return json;
                }
            }
        }
    }
}
