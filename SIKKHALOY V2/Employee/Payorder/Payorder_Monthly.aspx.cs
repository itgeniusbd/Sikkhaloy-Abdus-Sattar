using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee.Payorder
{
    public partial class Payorder_Monthly : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //DataView dv = (DataView)EmployeeListSQL.Select(DataSourceSelectArguments.Empty);
                //CountLabel.Text = "Total: " + dv.Count.ToString() + " Employee(s)";
            }
        }
        protected void AddPayorderButton_Click(object sender, EventArgs e)
        {
            PayorderNameSQL.Insert();
            Response.Redirect(Request.Url.AbsoluteUri);
        }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            SqlCommand EmployeeID_Cmd = new SqlCommand("SELECT EmployeeID FROM VW_Emp_Info WHERE (Job_Status = N'Active') AND (Time_Basis_Type = N'Monthly') AND (SchoolID = @SchoolID) AND ( EmployeeID NOT IN(SELECT Employee_Payorder_Monthly.EmployeeID FROM  Employee_Payorder_Monthly INNER JOIN Employee_Payorder ON Employee_Payorder_Monthly.Employee_PayorderID = Employee_Payorder.Employee_PayorderID WHERE (Employee_Payorder_Monthly.SchoolID = @SchoolID) AND (Employee_Payorder_Monthly.MonthName = @MonthName) AND (Employee_Payorder.Employee_Payorder_NameID = @Employee_Payorder_NameID)))", con);
            EmployeeID_Cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            EmployeeID_Cmd.Parameters.AddWithValue("@MonthName", MonthNameDropDownList.SelectedItem.Text);
            EmployeeID_Cmd.Parameters.AddWithValue("@Employee_Payorder_NameID", PayorderNameDropDownList.SelectedValue);
            con.Open();
            SqlDataReader EmployeeID_DR;
            EmployeeID_DR = EmployeeID_Cmd.ExecuteReader();
            CheckBox SingleCheckBox = new CheckBox();
            string employeeId = "";
            
            foreach (GridViewRow Row in EmployeeListGridView.Rows)
            {                
                SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;
                employeeId = EmployeeListGridView.DataKeys[Row.DataItemIndex]["EmployeeID"].ToString();
                Session["empID"] = employeeId;
                if (SingleCheckBox.Checked)
                {
                    PayorderSQL.InsertParameters["EmployeeID"].DefaultValue = employeeId; //EmployeeID_DR["EmployeeID"].ToString();
                    PayorderSQL.Insert();
                }
            }
            con.Close();
            PayOrderGridView.DataBind();
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
        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in PayOrderGridView.Rows)
            {
                Repeater BonusRepeater = (Repeater)row.FindControl("BonusRepeater");
                HiddenField Employee_PayorderID = (HiddenField)row.FindControl("Employee_PayorderID");
                var AttendanceFineTextBox = (TextBox)row.FindControl("AttendanceFineTextBox");

                foreach (RepeaterItem item in BonusRepeater.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var BonusID = (HiddenField)item.FindControl("BonusID");
                        var BonusTextBox = (TextBox)item.FindControl("BonusTextBox");

                        EmplyoeePayOrderSQL.InsertParameters["EmployeeID"].DefaultValue = PayOrderGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                        EmplyoeePayOrderSQL.InsertParameters["Bonus_Amount"].DefaultValue = BonusTextBox.Text.Trim();
                        EmplyoeePayOrderSQL.InsertParameters["BonusID"].DefaultValue = BonusID.Value;
                        EmplyoeePayOrderSQL.InsertParameters["Employee_PayorderID"].DefaultValue = Employee_PayorderID.Value;
                        EmplyoeePayOrderSQL.Insert();
                    }
                }


                Repeater FineRepeater = (Repeater)row.FindControl("FineRepeater");
                foreach (RepeaterItem item in FineRepeater.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var FineID = (HiddenField)item.FindControl("FineID");
                        var FineTextBox = (TextBox)item.FindControl("FineTextBox");

                        EmplyoeePayOrderSQL.UpdateParameters["EmployeeID"].DefaultValue = PayOrderGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                        EmplyoeePayOrderSQL.UpdateParameters["Fine_Amount"].DefaultValue = FineTextBox.Text.Trim();
                        EmplyoeePayOrderSQL.UpdateParameters["FineID"].DefaultValue = FineID.Value;
                        EmplyoeePayOrderSQL.UpdateParameters["Employee_PayorderID"].DefaultValue = Employee_PayorderID.Value;
                        EmplyoeePayOrderSQL.Update();
                    }
                }

                AttendanceFineUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = PayOrderGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                AttendanceFineUpdateSQL.UpdateParameters["FineAmount"].DefaultValue = AttendanceFineTextBox.Text.Trim();
                AttendanceFineUpdateSQL.UpdateParameters["Employee_PayorderID"].DefaultValue = Employee_PayorderID.Value;
                AttendanceFineUpdateSQL.Update();
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfull !!')", true);
        }
        
        protected void SalaryUpdateButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            //bool Up = false;

            foreach (GridViewRow rows in EmployeeListGridView.Rows)
            {       
                
                TextBox SalaryTextBox = (TextBox)rows.FindControl("SalaryTextBox");
                


                
                //Update Salary
                if (SalaryTextBox.Text != "")
                {
                    SalaryUpdateSQL.UpdateParameters["Salary"].DefaultValue = SalaryTextBox.Text;
                    SalaryUpdateSQL.UpdateParameters["EmployeeID"].DefaultValue = EmployeeListGridView.DataKeys[rows.DataItemIndex]["EmployeeID"].ToString();
                    SalaryUpdateSQL.Update();
                }                
            }
            //if (Up)
                //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully !!'');" +
                // "window.location='Payorder_Monthly.aspx';", true);
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Update Successfully!!')", true);


        }
    }
}