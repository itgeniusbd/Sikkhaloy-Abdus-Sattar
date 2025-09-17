using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings.AllCertificate
{
    public partial class Testimonial_English : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        [WebMethod]
        public static string GetAllID(string ids)
        {
            List<string> StudentId = new List<string>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT top(3) ID FROM Student WHERE SchoolID = @SchoolID AND ID like @ID + '%'";
                    cmd.Parameters.AddWithValue("@ID", ids);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
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
        //Export To Word
        protected void ExportWordButton_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=Certificate.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";

            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Admission/CSS/print-tc.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }
            sr.Close();

            Panel Data_Panel = (Panel)FormView1.FindControl("Data_Panel");

            Data_Panel.RenderControl(htmlWrite);


            Response.Write("<html><head><style type='text/css'>" + sb.ToString() + "</style></head><body>" + stringWrite.ToString() + "</body></html>");
            Response.Write(stringWrite.ToString());
            Response.End();
        }
    }
}