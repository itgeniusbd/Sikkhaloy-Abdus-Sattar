using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASIC_SETTING
{
    public partial class Education_Year : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            EduYearSQL.Insert();
        }

        protected void EduYearGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (EduYearGridView.DataKeys[e.Row.RowIndex]["Check_ID"].ToString() == "TRUE")
                {
                    CheckBox cb = e.Row.FindControl("ActiveCheckBox") as CheckBox;
                    cb.Checked = true;
                    cb.Enabled = false;
                    (e.Row.FindControl("DeleteLinkButton") as LinkButton).Visible = false;
                }
            }
        }
        protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            int selRowIndex = ((GridViewRow)(((CheckBox)sender).Parent.Parent)).RowIndex;
            SqlCommand cmd = new SqlCommand("UPDATE Education_Year_User SET EducationYearID = @EducationYearID WHERE (SchoolID = @SchoolID) AND (RegistrationID = @RegistrationID)", con);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@RegistrationID", Session["RegistrationID"].ToString());
            cmd.Parameters.AddWithValue("@EducationYearID", EduYearGridView.DataKeys[selRowIndex]["EducationYearID"].ToString());

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            Session["Edu_Year"] = EduYearGridView.DataKeys[selRowIndex]["EducationYearID"].ToString();
            Response.Redirect(Request.RawUrl);
        }
    }
}