using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Attendances.Online_Display
{
    public partial class Attendance_Slider : System.Web.UI.Page
    {
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
            Employee_FormView.DataBind();
            EmployeeIN_Repeater.DataBind();
            EmployeeOUT_Repeater.DataBind();

            Student_FormView.DataBind();
            StudentINRepeater.DataBind();
            StudentOUTRepeater.DataBind();
        }
    }
}