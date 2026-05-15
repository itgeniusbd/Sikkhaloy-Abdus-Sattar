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
                BindSubCategoryDropDown(EmpTypeRadioButtonList.SelectedValue);
                UpdateCount();
            }
        }

        // Populate sub-category dropdown filtered by employee type
        private void BindSubCategoryDropDown(string empType)
        {
            SubCategoryDropDownList.Items.Clear();
            SubCategoryDropDownList.Items.Add(new ListItem("-- সকল --", "0"));

            if (empType == "%" || empType == null) return; // All Employee - no sub-cat filter

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

        // Populate sub-category assign dropdown in each grid row
        protected void EmployeeGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var ddl = (DropDownList)e.Row.FindControl("SubCatAssignDDL");
            if (ddl == null) return;

            string empType = DataBinder.Eval(e.Row.DataItem, "EmployeeType")?.ToString();
            object subCatId = DataBinder.Eval(e.Row.DataItem, "SubCategoryID");

            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- নেই --", "0"));

            if (!string.IsNullOrEmpty(empType))
            {
                string cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(cs))
                using (SqlCommand cmd = new SqlCommand("SELECT SubCategoryID, SubCategoryName FROM Employee_SubCategory WHERE SchoolID=@SchoolID AND EmployeeType=@EmpType ORDER BY SubCategoryName", con))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd.Parameters.AddWithValue("@EmpType", empType);
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        while (dr.Read())
                            ddl.Items.Add(new ListItem(dr["SubCategoryName"].ToString(), dr["SubCategoryID"].ToString()));
                }
            }
            // Set current value
            if (subCatId != null && subCatId != DBNull.Value)
            {
                ListItem li = ddl.Items.FindByValue(subCatId.ToString());
                if (li != null) li.Selected = true;
            }
        }

        private void UpdateCount()
        {
            DataView dv = (DataView)EmployeeSQL.Select(DataSourceSelectArguments.Empty);
            CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
        }

        protected void EditLinkButton_Command(object sender, CommandEventArgs e)
        {
            if (e.CommandArgument.ToString() == "Teacher")
                Response.Redirect("Edit_Employee/Employee.aspx?Emp=" + e.CommandName.ToString());
            else
                Response.Redirect("Edit_Employee/Staff.aspx?Emp=" + e.CommandName.ToString());
        }

        protected void FindButton_Click(object sender, EventArgs e) { UpdateCount(); }

        protected void EmpTypeRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindSubCategoryDropDown(EmpTypeRadioButtonList.SelectedValue);
            UpdateCount();
        }

        protected void SubCategoryDropDownList_SelectedIndexChanged(object sender, EventArgs e) { UpdateCount(); }

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
                    con.Open(); cmd.ExecuteNonQuery();
                }
            }
        }

        // AJAX: Assign sub-category directly from Employee List grid
        [WebMethod]
        public static string AssignSubCategory(int employeeID, int subCategoryID)
        {
            string cs = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand("UPDATE Employee_Info SET SubCategoryID = CASE WHEN @SubCategoryID=0 THEN NULL ELSE @SubCategoryID END WHERE EmployeeID=@EmployeeID", con))
            {
                cmd.Parameters.AddWithValue("@SubCategoryID", subCategoryID);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                con.Open(); cmd.ExecuteNonQuery();
            }
            return "ok";
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

                if (AccNoTextBox.Text != "")
                {
                    Bank_AccNoUpdateSQL.UpdateParameters["Bank_AccNo"].DefaultValue = AccNoTextBox.Text;
                    Bank_AccNoUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    Bank_AccNoUpdateSQL.Update();
                }
                if (SalaryTextBox.Text != "")
                {
                    SalaryUpdateSQL.UpdateParameters["Salary"].DefaultValue = SalaryTextBox.Text;
                    SalaryUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    SalaryUpdateSQL.Update();
                }
                if (EmployeeTypeTextBox.Text != "")
                {
                    EmployeeSQL.InsertParameters["EmployeeType"].DefaultValue = EmployeeTypeTextBox.Text;
                    EmployeeSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    EmployeeSQL.Insert();
                }
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
