using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace EDUCATION.COM
{
    public partial class W_Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [WebMethod]
        public static string Get_Image_file()
        {
            List<UserImage> Imagedata = new List<UserImage>();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT SchoolID,ID,Image FROM VW_Attendance_Users_Image where SchoolID = @SchoolID";
                    cmd.Parameters.AddWithValue("@SchoolID", 1017); //HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        Imagedata.Add(new UserImage
                        {
                            ID = (string)dr["ID"],
                            SchoolID = dr["SchoolID"].ToString(),
                            Image = Convert.ToBase64String((byte[])dr["Image"])
                        });
                    }
                    con.Close();

                    var JS = new JavaScriptSerializer();
                    JS.MaxJsonLength = Int32.MaxValue;

                    var json = JS.Serialize(Imagedata);
                    return json;
                }
            }
        }

        class UserImage
        {
            public string ID { get; set; }
            public string SchoolID { get; set; }
            public string Image { get; set; }
        }
    }
}