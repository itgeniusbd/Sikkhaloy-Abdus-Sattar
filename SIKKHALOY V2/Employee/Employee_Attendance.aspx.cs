using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Attendance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            AttendenceCheck();

            if (AttendanceDateTextBox.Text.Trim() != "")
                EmployeeGridView.Visible = true;
            else
                EmployeeGridView.Visible = false;
        }

        protected void AttendanceButton_Click(object sender, EventArgs e)
        {
            bool Done = false;
            foreach (GridViewRow row in EmployeeGridView.Rows)
            {
                RadioButtonList Attendance = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                TextBox StartTimeTextBox = row.FindControl("StartTimeTextBox") as TextBox;
                TextBox EndTimeTextBox = row.FindControl("EndTimeTextBox") as TextBox;
                CheckBox Attendance_CheckBox = row.FindControl("Attendance_CheckBox") as CheckBox;

                if (Attendance_CheckBox.Checked)
                {
                    Attendance_RecordSQL.InsertParameters["EmployeeID"].DefaultValue = EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString();
                    Attendance_RecordSQL.InsertParameters["AttendanceStatus"].DefaultValue = Attendance.SelectedValue;
                    Attendance_RecordSQL.InsertParameters["AttendanceDate"].DefaultValue = AttendanceDateTextBox.Text.Trim();
                    Attendance_RecordSQL.InsertParameters["EntryTime"].DefaultValue = StartTimeTextBox.Text;
                    Attendance_RecordSQL.InsertParameters["ExitTime"].DefaultValue = EndTimeTextBox.Text;

                    Attendance_RecordSQL.Insert();
                    Done = true;
                }
            }

            if (Done)
            {
                AttendenceCheck();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Done!!')", true);
            }
        }

        protected void AttendenceCheck()
        {
            if (AttendanceDateTextBox.Text.Trim() != "")
            {
                EmployeeGridView.DataBind();

                foreach (GridViewRow row in EmployeeGridView.Rows)
                {
                    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                    string SchoolID = Session["SchoolID"].ToString();

                    DateTime AttendanceDate = Convert.ToDateTime(AttendanceDateTextBox.Text.Trim());
                    RadioButtonList Attendance = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                    Label AtDateLabel = (Label)row.FindControl("AtDateLabel");
                    TextBox StartTimeTextBox = row.FindControl("StartTimeTextBox") as TextBox;
                    TextBox EndTimeTextBox = row.FindControl("EndTimeTextBox") as TextBox;

                    SqlCommand AttendanceRecordIDcmd = new SqlCommand("SELECT * from Employee_Attendance_Record Where EmployeeID = @EmployeeID and AttendanceDate = @AttendanceDate and SchoolID = @SchoolID", con);
                    AttendanceRecordIDcmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString());
                    AttendanceRecordIDcmd.Parameters.AddWithValue("@AttendanceDate", AttendanceDate);
                    AttendanceRecordIDcmd.Parameters.AddWithValue("@SchoolID", SchoolID);


                    con.Open();
                    SqlDataReader rdr = AttendanceRecordIDcmd.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(rdr);
                    con.Close();


                    if (dataTable.Rows.Count > 0)
                    {
                        row.CssClass = "Diable_Rows";

                        Attendance.SelectedValue = dataTable.Rows[0]["AttendanceStatus"].ToString();
                        StartTimeTextBox.Text = dataTable.Rows[0]["EntryTime"].ToString();
                        EndTimeTextBox.Text = dataTable.Rows[0]["ExitTime"].ToString();

                        if (dataTable.Rows[0]["AttendanceStatus"].ToString() == "Leave" || dataTable.Rows[0]["AttendanceStatus"].ToString() == "Abs")
                        {
                            StartTimeTextBox.Enabled = false;
                            EndTimeTextBox.Enabled = false;
                        }
                    }


                    SqlCommand Leavecmd = new SqlCommand("SELECT LeaveReason from Employee_Leave where EmployeeID = @EmployeeID and LeaveStartDate <= @StartDate and LeaveEndDate >= @EndDate and SchoolID = @SchoolID", con);
                    Leavecmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString());
                    Leavecmd.Parameters.AddWithValue("@StartDate", AttendanceDate);
                    Leavecmd.Parameters.AddWithValue("@EndDate", AttendanceDate);
                    Leavecmd.Parameters.AddWithValue("@SchoolID", SchoolID);

                    con.Open();
                    object LeaveCheck = Leavecmd.ExecuteScalar();
                    con.Close();

                    if (LeaveCheck != null)
                    {
                        SqlCommand StartDateCmd = new SqlCommand("SELECT LeaveStartDate from Employee_Leave where EmployeeID = @EmployeeID and LeaveStartDate <= @StartDate and LeaveEndDate >= @EndDate and SchoolID = @SchoolID", con);
                        StartDateCmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString());
                        StartDateCmd.Parameters.AddWithValue("@StartDate", AttendanceDate);
                        StartDateCmd.Parameters.AddWithValue("@EndDate", AttendanceDate);
                        StartDateCmd.Parameters.AddWithValue("@SchoolID", SchoolID);


                        SqlCommand EndDateCmd = new SqlCommand("SELECT LeaveEndDate from Employee_Leave where EmployeeID = @EmployeeID and LeaveStartDate <= @StartDate and LeaveEndDate >= @EndDate and SchoolID = @SchoolID", con);
                        EndDateCmd.Parameters.AddWithValue("@EmployeeID", EmployeeGridView.DataKeys[row.DataItemIndex]["EmployeeID"].ToString());
                        EndDateCmd.Parameters.AddWithValue("@StartDate", AttendanceDate);
                        EndDateCmd.Parameters.AddWithValue("@EndDate", AttendanceDate);
                        EndDateCmd.Parameters.AddWithValue("@SchoolID", SchoolID);

                        con.Open();
                        DateTime SDate = (DateTime)StartDateCmd.ExecuteScalar();
                        DateTime EDate = (DateTime)EndDateCmd.ExecuteScalar();
                        con.Close();

                        Attendance.SelectedIndex = 4;
                        AtDateLabel.Text = "(From:" + SDate.ToString("d MMM yy") + " To " + EDate.ToString("d MMM yy") + ")";

                        StartTimeTextBox.Enabled = false;
                        EndTimeTextBox.Enabled = false;
                    }
                }
            }
        }
        protected void EmpTypeRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            AttendenceCheck();
        }
    }
}