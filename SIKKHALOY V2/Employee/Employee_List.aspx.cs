using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_List : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
                CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
            }
        }


        protected void EditLinkButton_Command(object sender, CommandEventArgs e)
        {
            if (e.CommandArgument.ToString() == "Teacher")
            {
                Response.Redirect("Edit_Employee/Employee.aspx?Emp=" + e.CommandName.ToString());
            }
            else
            {
                Response.Redirect("Edit_Employee/Staff.aspx?Emp=" + e.CommandName.ToString());
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
            CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
        }

        protected void EmpTypeRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
            CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
        }

        //Update Employee Image via AJAX
        [WebMethod]
        public static void UpdateEmployeeImage(string EmployeeID, string EmployeeType, string Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string tableName = EmployeeType == "Teacher" ? "Teacher" : "Staff_Info";

                using (SqlCommand cmd = new SqlCommand($"UPDATE {tableName} SET Image = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') WHERE EmployeeID = @EmployeeID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                    cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            bool Up = false;

            foreach (GridViewRow rows in EmployeeGridView.Rows)
            {
                TextBox Emp_ID_TextBox = (TextBox)rows.FindControl("Emp_ID_TextBox");
                TextBox EmployeeTypeTextBox = (TextBox)rows.FindControl("EmployeeTypeTextBox");
                TextBox SalaryTextBox = (TextBox)rows.FindControl("SalaryTextBox");
                TextBox AccNoTextBox = (TextBox)rows.FindControl("AccNoTextBox");


                //Update Acc No
                if (AccNoTextBox.Text != "")
                {
                    Bank_AccNoUpdateSQL.UpdateParameters["Bank_AccNo"].DefaultValue = AccNoTextBox.Text;
                    Bank_AccNoUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    Bank_AccNoUpdateSQL.Update();
                }
                //Update Salary
                if (SalaryTextBox.Text != "")
                {
                    SalaryUpdateSQL.UpdateParameters["Salary"].DefaultValue = SalaryTextBox.Text;
                    SalaryUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    SalaryUpdateSQL.Update();
                }

                //Update EmployeeType
                if (EmployeeTypeTextBox.Text != "")
                {
                    EmployeeSQL.InsertParameters["EmployeeType"].DefaultValue = EmployeeTypeTextBox.Text;
                    EmployeeSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    EmployeeSQL.Insert();
                }


                //Update Employee ID
                if (Emp_ID_TextBox.Text != "")
                {
                    EmployeeSQL.UpdateParameters["ID"].DefaultValue = Emp_ID_TextBox.Text;
                    EmployeeSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    EmployeeSQL.Update();

                    Device_DataUpdateSQL.Insert();
                    Up = true;
                }
            }

            if (Up)
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully!!')", true);
        }
    }
}