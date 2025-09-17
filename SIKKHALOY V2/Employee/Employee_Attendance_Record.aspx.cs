using System;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Attendance_Record : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["SchoolID"] != null)
                {
                    FromDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                    ToDateTextBox.Text = DateTime.Now.ToString("d MMM yyyy");
                }
                else
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Login.aspx");
                }
            }
        }

        protected void EmployeeDropDownList_DataBound(object sender, EventArgs e)
        {
            EmployeeDropDownList.Items.Insert(0, new ListItem("[ All Employee ]", "0"));
        }

        protected void EmployeeAttendanceSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            RecordLabel.Text = "Total: " + e.AffectedRows.ToString() + " Record";
        }

        protected void EmployeeAttRecordGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (EmployeeAttRecordGridView.Rows.Count > 0)
            {
                EmployeeAttRecordGridView.UseAccessibleHeader = true;
                EmployeeAttRecordGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void Employee_Att_Summary_GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (Employee_Att_Summary_GridView.Rows.Count > 0)
            {
                Employee_Att_Summary_GridView.UseAccessibleHeader = true;
                Employee_Att_Summary_GridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }
    }
}