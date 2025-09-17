using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;

namespace EDUCATION.COM.ADMISSION_REGISTER
{
    public partial class Edit_Student_information : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["Student"]) | string.IsNullOrEmpty(Request.QueryString["Student_Class"]))
                    Response.Redirect("Find_Student.aspx");
            }
        }

        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count > 0)
            {
                Response.Redirect("Edit_Student_information.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
                StudentInfoFormView.DataBind();
            }
            else
            {
                Response.Redirect("Edit_Student_information.aspx?Student=0&Student_Class=0");
                StudentInfoFormView.DataBind();
            }
        }

        //Update Student Image
        [WebMethod]
        public static void Update_Student_Image(string StudentImageID, string Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE Student_Image SET Image = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') Where StudentImageID = @StudentImageID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@StudentImageID", StudentImageID);
                    cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        //Update Guardian Image
        [WebMethod]
        public static void Update_Guardian_Image(string StudentImageID, string Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE Student_Image SET Guardian_Photo = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') Where StudentImageID = @StudentImageID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@StudentImageID", StudentImageID);
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