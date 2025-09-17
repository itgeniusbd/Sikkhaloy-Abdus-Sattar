using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;

namespace EDUCATION.COM.Teacher
{
    public partial class Update_Info : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        //Update_Teacher_Image
        [WebMethod]
        public static void Update_Teacher_Image(string Image)
        {
            if (HttpContext.Current.Session["SchoolID"] == null) return;

            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE Teacher SET Image = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') Where TeacherID = @TeacherID and SchoolID = @SchoolID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@TeacherID", HttpContext.Current.Session["TeacherID"].ToString());
                    cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
    }
}