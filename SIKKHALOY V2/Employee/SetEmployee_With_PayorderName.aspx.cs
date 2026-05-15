using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class SetEmployee_With_PayorderName : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindSubCategoryDropDown(EmpTypeDropDownList.SelectedValue);
            }
        }

        private void BindSubCategoryDropDown(string empType)
        {
            SubCategoryDropDownList.Items.Clear();
            SubCategoryDropDownList.Items.Add(new ListItem("-- সকল --", "0"));

            if (empType == "%" || string.IsNullOrEmpty(empType)) return;

            string cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand("SELECT SubCategoryID, SubCategoryName FROM Employee_SubCategory WHERE SchoolID=@SchoolID AND EmployeeType=@EmpType ORDER BY SubCategoryName", con))
            {
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@EmpType", empType);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read())
                        SubCategoryDropDownList.Items.Add(new ListItem(dr["SubCategoryName"].ToString(), dr["SubCategoryID"].ToString()));
            }
        }

        protected void EmpTypeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubCategoryDropDownList.SelectedIndex = 0;
            BindSubCategoryDropDown(EmpTypeDropDownList.SelectedValue);
            EmployeeListGridView.DataBind();
        }
        protected void PayorderNameUpdateButton_Click(object sender, EventArgs e)
        {

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            string payordernameId = PayorderNameDropDownList.SelectedValue;

            con.Open();
            
            CheckBox SingleCheckBox = new CheckBox();
            string employeeId = "";

            foreach (GridViewRow Row in EmployeeListGridView.Rows)
            {
                SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;
                employeeId = EmployeeListGridView.DataKeys[Row.DataItemIndex]["EmployeeID"].ToString();
                Session["empID"] = employeeId;
                if (SingleCheckBox.Checked)
                {
                    EmployeeListSQL.UpdateParameters["EmployeeID"].DefaultValue = employeeId; //EmployeeID_DR["EmployeeID"].ToString();
                    
                    EmployeeListSQL.Update();
                }
            }
            con.Close();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully!!')", true);
            EmployeeListGridView.DataBind();














            //SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            ////bool Up = false;

            //foreach (GridViewRow rows in EmployeeListGridView.Rows)
            //{

            //    TextBox SalaryTextBox = (TextBox)rows.FindControl("SalaryTextBox");




            //    //Update Salary
            //    if (SalaryTextBox.Text != "")
            //    {
            //        SalaryUpdateSQL.UpdateParameters["Salary"].DefaultValue = SalaryTextBox.Text;
            //        SalaryUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeListGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
            //        SalaryUpdateSQL.Update();
            //    }
            //}
            ////if (Up)
            ////ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully !!'');" +
            //// "window.location='Payorder_Monthly.aspx';", true);
            //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully!!')", true);


        }
    }
}