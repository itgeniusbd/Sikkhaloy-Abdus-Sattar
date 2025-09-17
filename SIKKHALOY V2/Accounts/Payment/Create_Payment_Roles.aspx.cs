using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public partial class Create_Payment_Roles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void AddButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            SqlCommand cmd = new SqlCommand("Select RoleID from Income_Roles where Role = @Role And SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@Role", RoleNameTextBox.Text.Trim());
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            object CheckPayCategory = cmd.ExecuteScalar();
            con.Close();

            if (CheckPayCategory == null)
            {
                RoleSQL.Insert();

                Response.Redirect(Request.Url.AbsoluteUri);
                PayCategoryGridView.DataBind();
                RoleNameTextBox.Text = string.Empty;
            }
            else
            {
                MsgLabel.Text = RoleNameTextBox.Text.Trim() + ". Role Already exist";
            }
        }

        protected void PayCategoryGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            if (e.Exception != null)
            {
                MsgLabel.Text = "Sorry This Record used another option. (Record not deleted)";
                e.ExceptionHandled = true;
            }
        }
    }
}