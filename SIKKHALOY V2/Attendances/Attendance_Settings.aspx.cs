using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace EDUCATION.COM.Attendances
{
    public partial class Attendance_Settings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //Download user info
        protected void InfoDownload_Click(object sender, EventArgs e)
        {
            InfoLinkButton.Enabled = false;
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT DeviceID,ScheduleID,ID,RFID,Name,Designation,Is_Student FROM VW_Attendance_Users WHERE (SchoolID = @SchoolID)"))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);

                            //Build the CSV file data as a Comma separated string.
                            string csv = string.Empty;

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    //Add the Data rows.
                                    csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';
                                }

                                //Add new line.
                                csv += "\r\n";
                            }

                            //Download the CSV file.
                            Response.Clear();
                            Response.Buffer = true;
                            Response.AddHeader("content-disposition", "attachment;filename=AttendanceUsers.csv");
                            Response.Charset = "";
                            Response.ContentType = "application/text";
                            Response.Output.Write(csv);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }

            InfoLinkButton.Enabled = true;
        }

        //Download Photo
        [WebMethod]
        public static string Get_Image_file()
        {
            var imageData = new List<UserImage>();

            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT ID,Image FROM VW_Attendance_Users_Image where SchoolID = @SchoolID";
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        imageData.Add(new UserImage
                        {
                            ID = (string)dr["ID"],
                            Image = Convert.ToBase64String((byte[])dr["Image"])
                        });
                    }
                    con.Close();

                    var js = new JavaScriptSerializer {MaxJsonLength = int.MaxValue};

                    var json = js.Serialize(imageData);
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