using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Salary_Sheet : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void EmployeeGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (EmployeeGridView.Rows.Count > 0)
            {
                EmployeeGridView.UseAccessibleHeader = true;
                EmployeeGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }
    }
}