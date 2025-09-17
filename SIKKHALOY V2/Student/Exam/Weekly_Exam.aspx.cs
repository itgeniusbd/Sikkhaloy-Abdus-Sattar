using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student.Exam
{
    public partial class Weekly_Exam : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void WeeklyExamGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[8].Text == "1st")
                {
                    e.Row.Cells[8].CssClass = "First";
                }

                if (e.Row.Cells[8].Text == "2nd")
                {
                    e.Row.Cells[8].CssClass = "Second";
                }

                if (e.Row.Cells[8].Text == "3rd")
                {
                    e.Row.Cells[8].CssClass = "Third";
                }
            }
        }
    }
}