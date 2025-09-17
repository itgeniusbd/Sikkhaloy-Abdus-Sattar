using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions.Device
{
    public partial class InstitutionRegister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Register_Button_Click(object sender, EventArgs e)
        {
            if (School_DropDownList.SelectedIndex > 0)
            {
                UserSQL.Insert();

                Password_TextBox.Text = "";
                School_DropDownList.SelectedIndex = 0;
                ErrorLabel.Text = "User registration Successfully!";
                ErrorLabel.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                ErrorLabel.Text = "server error";
                ErrorLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void ActiveCheckBox_OnCheckedChanged(object sender, EventArgs e)
        {
            var activeCheckBox = (CheckBox)sender;
            var row = (GridViewRow)activeCheckBox.Parent.Parent;

            var schoolId = UsersGridView.DataKeys[row.DataItemIndex]?["SchoolID"].ToString();
            DeviceActiveInactiveSQL.UpdateParameters["SchoolID"].DefaultValue = schoolId;
            DeviceActiveInactiveSQL.UpdateParameters["IsActive"].DefaultValue = activeCheckBox.Checked.ToString();
            DeviceActiveInactiveSQL.Update();
        }


        //update device password
        [WebMethod]
        public static void UpdatePassword(string userName, string password)
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            var command = new SqlCommand("UPDATE Attendance_Device_Setting SET Password = @Password WHERE (UserName = @UserName)", con);
            command.Parameters.AddWithValue("@UserName", userName);
            command.Parameters.AddWithValue("@Password", password);

            con.Open();
            command.ExecuteNonQuery();
            con.Close();
        }
    }
}