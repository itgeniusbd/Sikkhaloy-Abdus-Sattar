using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;

namespace EDUCATION.COM.ADMISSION_REGISTER
{
    public partial class Admission_New_Student : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null || Session["Edu_Year"] != null)
            {
                if (!this.IsPostBack)
                {
                    EducationYearDropDownList.SelectedValue = Session["Edu_Year"].ToString();

                    SqlCommand LastID = new SqlCommand("SELECT ID FROM Student WHERE (SchoolID = @SchoolID) ORDER BY StudentID DESC", con);
                    LastID.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                    con.Open();
                    LastIDLabel.Text = "Last Entry ID " + LastID.ExecuteScalar();
                    con.Close();
                }
            }
        }

        protected void StudentsInfoButton_Click(object sender, EventArgs e)
        {
            SqlCommand IDcmd = new SqlCommand("select ID from Student where SchoolID = @SchoolID and ID = @ID", con);
            IDcmd.Parameters.AddWithValue("@ID", IDTextBox.Text.Trim());
            IDcmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            object ID = IDcmd.ExecuteScalar();
            con.Close();

            if (ID == null)
            {
                StudentImageSQL.Insert();
                SqlCommand StudentImageIDcmd = new SqlCommand("SELECT IDENT_CURRENT('Student_Image')", con);

                con.Open();
                StudentInfoSQL.InsertParameters["StudentImageID"].DefaultValue = StudentImageIDcmd.ExecuteScalar().ToString();
                con.Close();

                StudentInfoSQL.Insert();
                SqlCommand Student = new SqlCommand("SELECT IDENT_CURRENT('Student')", con);

                con.Open();
                string StudentID = Student.ExecuteScalar().ToString();
                con.Close();

                Response.Cookies["StudentName"].Value = StudentNameTextBox.Text;
                Response.Cookies["ID"].Value = IDTextBox.Text;
                Response.Cookies["SMSPhone"].Value = SMSPhoneNoTextBox.Text;
                Response.Cookies["Admission_Year"].Value = EducationYearDropDownList.SelectedValue;

                Response.Redirect("Class_And_Subject.aspx?Student=" + StudentID + "&Name=" + StudentNameTextBox.Text + "&Fname=" + FatherNameTextBox.Text + "&ID=" + IDTextBox.Text);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + IDTextBox.Text + " ID Already exists. Enter an unique ID')", true);
            }

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
    }
}