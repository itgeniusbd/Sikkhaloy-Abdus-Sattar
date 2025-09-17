using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Teacher
{
    public partial class Attendance : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        SqlDataAdapter Attendance_Calendar_DA;
        SqlDataAdapter Holiday_DA;

        DataSet Atten_DS = new DataSet();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null)
            {
                //Attendance
                Attendance_Calendar_DA = new SqlDataAdapter("SELECT CONVERT(varchar(15), Employee_Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT(varchar(15), Employee_Attendance_Record.ExitTime, 100) AS ExitTime, Employee_Attendance_Record.AttendanceDate, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN Teacher ON Employee_Attendance_Record.EmployeeID = Teacher.EmployeeID WHERE (Teacher.TeacherID = @TeacherID) AND (Employee_Attendance_Record.SchoolID = @SchoolID)", con);
                Attendance_Calendar_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Attendance_Calendar_DA.SelectCommand.Parameters.AddWithValue("@TeacherID", Session["TeacherID"].ToString());
                Attendance_Calendar_DA.Fill(Atten_DS, "Table");

                //Holiday
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = @SchoolID", con);
                Holiday_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Holiday_DA.Fill(Atten_DS, "HolidaysTable");
            }
        }

        //AttendanceCalendar
        protected void AttendanceCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            // If the month is CurrentMonth
            if (!e.Day.IsOtherMonth)
            {
                foreach (DataRow dr in Atten_DS.Tables[0].Rows)
                {
                    if ((dr["AttendanceDate"].ToString() != DBNull.Value.ToString()))
                    {
                        DateTime dtEvent = (DateTime)dr["AttendanceDate"];
                        Label lbl = new Label();

                        if (dtEvent.Equals(e.Day.Date))
                        {
                            lbl.Text += " (" + dr["AttendanceStatus"].ToString() + ")" + "<br />" + dr["EntryTime"].ToString();
                            if (dr["ExitTime"].ToString() != "")
                            {
                                lbl.Text += " - " + dr["ExitTime"].ToString();
                            }

                            lbl.CssClass = "Appointment";

                            if (dr["AttendanceStatus"].ToString() == "Pre")
                            {
                                e.Cell.CssClass = "Pre";
                            }

                            if (dr["AttendanceStatus"].ToString() == "Abs")
                            {
                                e.Cell.CssClass = "Abs";
                            }

                            if (dr["AttendanceStatus"].ToString() == "Late")
                            {
                                e.Cell.CssClass = "Late";
                            }

                            if (dr["AttendanceStatus"].ToString() == "Late Abs")
                            {
                                e.Cell.CssClass = "Late_Abs";
                            }

                            e.Cell.Controls.Add(lbl);
                        }
                    }
                }

                //Holidays
                foreach (DataRow dr in Atten_DS.Tables[1].Rows)
                {
                    if ((dr["HolidayDate"].ToString() != DBNull.Value.ToString()))
                    {
                        DateTime dtEvent = (DateTime)dr["HolidayDate"];
                        Label lbl = new Label();
                        lbl.CssClass = "Appointment";

                        if (dtEvent.Equals(e.Day.Date))
                        {
                            e.Cell.CssClass = "Att_Holidays";

                            lbl.Text = "<br />";
                            lbl.Text += dr["HolidayName"].ToString();
                            e.Cell.Controls.Add(lbl);
                        }
                    }
                }
            }
            //If the month is not CurrentMonth then hide the Dates
            else
            {
                e.Cell.Text = "";
            }
        }
    }
}