using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Attendances.Online_Display
{
    public partial class Attendance_Slider : System.Web.UI.Page
    {
        private const int DisplaySqlTimeoutSeconds = 45;

        protected void Page_Init(object sender, EventArgs e)
        {
            // Same protection DeviceDisplay uses: display reads must not queue behind device-push
            // inserts (READ UNCOMMITTED), get a longer timeout than the 30s default, and a failing
            // source must not crash the whole page with "The wait operation timed out".
            WireDisplaySource(StudentScheduleSummarySQL);
            WireDisplaySource(EmployeeScheduleSummarySQL);
            WireDisplaySource(Student_Entry_LogSQL);
            WireDisplaySource(Student_Exit_LogSQL);
            WireDisplaySource(Emp_INSQL);
            WireDisplaySource(Emp_OUTSQL);
        }

        private void WireDisplaySource(SqlDataSource source)
        {
            source.Selecting -= DisplaySql_Selecting;
            source.Selected -= DisplaySql_Selected;
            source.Selecting += DisplaySql_Selecting;
            source.Selected += DisplaySql_Selected;
        }

        protected void DisplaySql_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            e.Command.CommandTimeout = DisplaySqlTimeoutSeconds;
            var sql = e.Command.CommandText ?? string.Empty;
            if (sql.IndexOf("READ UNCOMMITTED", StringComparison.OrdinalIgnoreCase) < 0)
            {
                e.Command.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " + sql;
            }
        }

        protected void DisplaySql_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            if (e.Exception != null)
                e.ExceptionHandled = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] == null || Session["Edu_Year"] == null)
                Response.Redirect("~/Default.aspx");
        }

        protected void Employee_CheckBoxList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filter = "";
            foreach (ListItem item in Employee_CheckBoxList.Items)
            {
                if (item.Selected)
                {
                    filter += "'" + item.Value + "',";
                }
            }
            if (filter != "")
            {
                filter.TrimEnd(',');
                Emp_INSQL.FilterExpression = "AttendanceStatus in (" + filter + ")";
                Emp_OUTSQL.FilterExpression = "AttendanceStatus in (" + filter + ")";
            }
            else
            {
                Emp_INSQL.FilterExpression = "";
                Emp_OUTSQL.FilterExpression = "";
                EmployeeIN_Repeater.DataBind();
                EmployeeOUT_Repeater.DataBind();
            }
        }

        protected void Student_CheckBoxList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Filter = "";
            foreach (ListItem item in Student_CheckBoxList.Items)
            {
                if (item.Selected)
                {
                    Filter += "'" + item.Value + "',";
                }
            }
            if (Filter != "")
            {
                Filter.TrimEnd(',');
                Student_Entry_LogSQL.FilterExpression = "Attendance in (" + Filter + ")";
                Student_Exit_LogSQL.FilterExpression = "Attendance in (" + Filter + ")";

            }
            else
            {
                Student_Entry_LogSQL.FilterExpression = "";
                Student_Exit_LogSQL.FilterExpression = "";
                StudentINRepeater.DataBind();
                StudentOUTRepeater.DataBind();
            }
        }

        protected void Reload_LinkButton_Click(object sender, EventArgs e)
        {
            EmployeeScheduleSummaryRepeater.DataBind();
            EmployeeIN_Repeater.DataBind();
            EmployeeOUT_Repeater.DataBind();

            StudentScheduleSummaryRepeater.DataBind();
            StudentINRepeater.DataBind();
            StudentOUTRepeater.DataBind();
        }
    }
}