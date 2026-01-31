using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;

namespace EDUCATION.COM
{
    public partial class Profile_Redirect : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated || Session["SchoolID"] == null)
            {
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
            }

            //for Authority
            if (Roles.IsUserInRole(User.Identity.Name.Trim(), "Authority") || Roles.IsUserInRole(User.Identity.Name.Trim(), "Sub-Authority"))
            {
                Response.Redirect("~/Authority/Auth_Profile.aspx");
            }
            else//for Others
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

                SqlCommand School_Validity = new SqlCommand("SELECT Validation FROM SchoolInfo WHERE SchoolID = @SchoolID", con);
                School_Validity.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                con.Open();
                string Validation = School_Validity.ExecuteScalar().ToString();
                con.Close();

                if (Validation == "Valid") //Is School Invalid
                {
                    SqlCommand User_Validity = new SqlCommand("SELECT Validation FROM Registration WHERE UserName = @UserName and Validation = 'Valid'", con);
                    User_Validity.Parameters.AddWithValue("@UserName", User.Identity.Name.Trim());

                    con.Open();
                    string User_Validation = User_Validity.ExecuteScalar().ToString();
                    con.Close();

                    if (User_Validation == "Valid")  //Is User Invalid
                    {
                        if (Roles.IsUserInRole(User.Identity.Name.Trim(), "Admin"))
                            Response.Redirect("~/Profile/Admin.aspx");

                        if (Roles.IsUserInRole(User.Identity.Name.Trim(), "Sub-Admin"))
                            Response.Redirect("~/Profile/Admin.aspx");

                        if (Roles.IsUserInRole(User.Identity.Name.Trim(), "Teacher"))
                            Response.Redirect("~/Teacher/Teacher_profile.aspx");

                        if (Roles.IsUserInRole(User.Identity.Name.Trim(), "Student"))
                            Response.Redirect("~/Student/Student_Profile.aspx");

                        if (Roles.IsUserInRole(User.Identity.Name.Trim(), "Donor"))
                            Response.Redirect("~/Committee/Donor_Dashboard.aspx");
                    }
                    else
                    {
                        FormsAuthentication.SignOut();
                        Response.Redirect("~/Default.aspx?Invalid=User Locked by Admin");
                    }
                }
                else
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Default.aspx?Invalid=Institution Invalid by Authourity");
                }
            }
        }
    }
}
