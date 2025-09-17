using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace EDUCATION.COM.Employee
{
    public partial class Staff_Info : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddStaffButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            bool isAmount = false;
            if (Abs_DeductedRadioButtonList.SelectedIndex == 0)
            {
                if (Abs_DeductedAmount_TextBox.Text.Trim() != "")
                {
                    isAmount = true;
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Input Deduction Amount')", true);
                }
            }
            else
            {
                isAmount = true;
            }


            if (isAmount)
            {
                Staff_InfoSQL.Insert();
                SqlCommand Staff_Info_Cmd = new SqlCommand("Select IDENT_CURRENT('Staff_Info')", con);

                con.Open();
                Employee_InfoSQL.InsertParameters["StaffID"].DefaultValue = Staff_Info_Cmd.ExecuteScalar().ToString();
                con.Close();

                Employee_InfoSQL.Insert();
                Device_DataUpdateSQL.Insert();

                Response.Redirect(Request.Url.AbsoluteUri);
            }
        }
    }
}