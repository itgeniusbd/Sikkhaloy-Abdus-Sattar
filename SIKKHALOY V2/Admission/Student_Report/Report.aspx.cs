using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Student_Rerport
{
    public partial class Report : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        SqlDataAdapter Attendance_Calendar_DA;
        SqlDataAdapter Holiday_DA;

        DataSet Atten_DS = new DataSet();
        string StudentID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null)
            {
                if (!this.IsPostBack)
                {
                    StudentID = Request.QueryString["Student"];
                    if (string.IsNullOrEmpty(StudentID))
                        Response.Redirect("Students_List.aspx");
                }

                //Attendance
                Attendance_Calendar_DA = new SqlDataAdapter("SELECT Attendance, AttendanceDate, CONVERT(varchar(15), EntryTime, 100) AS EntryTime,CONVERT(varchar(15), ExitTime, 100) AS ExitTime FROM Attendance_Record Where StudentClassID = @StudentClassID and SchoolID = @SchoolID", con);
                Attendance_Calendar_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Attendance_Calendar_DA.SelectCommand.Parameters.AddWithValue("@StudentClassID", Request.QueryString["Student_Class"]);
                Attendance_Calendar_DA.Fill(Atten_DS, "Table");

                //Holidays
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = @SchoolID", con);
                Holiday_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Holiday_DA.Fill(Atten_DS, "HolidaysTable");
            }
        }
        //find by ID
        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count > 0)
            {
                Response.Redirect("Report.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
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
                            lbl.Text += " (" + dr["Attendance"].ToString() + ")" + "<br />" + dr["EntryTime"].ToString();
                            if (dr["ExitTime"].ToString() != "")
                            {
                                lbl.Text += " - " + dr["ExitTime"].ToString();
                            }

                            lbl.CssClass = "Appointment";

                            if (dr["Attendance"].ToString() == "Pre")
                            {
                                e.Cell.CssClass = "Pre";
                            }

                            if (dr["Attendance"].ToString() == "Abs")
                            {
                                e.Cell.CssClass = "Abs";
                            }

                            if (dr["Attendance"].ToString() == "Late")
                            {
                                e.Cell.CssClass = "Late";
                            }

                            if (dr["Attendance"].ToString() == "Late Abs")
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

        //Accounts
        protected void MreceiptLinkButton_Command(object sender, CommandEventArgs e)
        {
            AllPayRecordSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        //Individual Exam
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResultReportViewer.LocalReport.Refresh();
            this.ResultReportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(ResultReportViewer_SubreportProcessing);
        }
        void ResultReportViewer_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            e.DataSources.Add(new ReportDataSource("DataSet1", GradingSystemODS));
            e.DataSources.Add(new ReportDataSource("SchoolDS", SchoolInfoODS));
        }

        //Cumulative Result
        protected void Cum_ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cu_ResultReportViewer.LocalReport.Refresh();
            this.Cu_ResultReportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(Cum_ResultReportViewer_SubreportProcessing);
        }
        void Cum_ResultReportViewer_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            e.DataSources.Add(new ReportDataSource("DataSet1", Cu_GradingSystemODS));
        }

        protected void Fault_Add_Button_Click(object sender, EventArgs e)
        {
            FaultSQL.Insert();
            Fault_Title_TextBox.Text = "";
            Fault_TextBox.Text = "";
            Fault_Date_TextBox.Text = "";
            Fault_Gridview.DataBind();

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

        //Exam chart data
        [WebMethod]
        public static List<object> Get_Exam_GradePoint(string EducationYearID, string StudentID)
        {
            List<object> chartData = new List<object>();
            List<string> ExamName = new List<string>();
            List<string> Point = new List<string>();
            List<string> Grade = new List<string>();

            string query = "SELECT Exam_Name.ExamName, Exam_Result_of_Student.Student_Point,Exam_Result_of_Student.Student_Grade FROM Exam_Result_of_Student INNER JOIN Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID WHERE(Exam_Result_of_Student.StudentID = @StudentID) AND(Exam_Result_of_Student.StudentPublishStatus = N'Pub') AND(Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY Exam_Name.Period_StartDate";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", StudentID);
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            ExamName.Add(sdr["ExamName"].ToString());
                            Point.Add(sdr["Student_Point"].ToString());
                            Grade.Add(sdr["Student_Grade"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(ExamName);
                    chartData.Add(Point);
                    chartData.Add(Grade);

                    return chartData;
                }
            }
        }

        [WebMethod]
        public static List<object> Get_CumilativeExam(string EducationYearID, string StudentID)
        {
            List<object> chartData = new List<object>();
            List<string> ExamName = new List<string>();
            List<string> Point = new List<string>();
            List<string> Grade = new List<string>();

            string query = "SELECT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Student.Student_Point, Exam_Cumulative_Student.Student_Grade FROM Exam_Cumulative_Student INNER JOIN Exam_Cumulative_Name ON Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID WHERE(Exam_Cumulative_Student.StudentID = @StudentID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID)";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", StudentID);
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            ExamName.Add(sdr["CumulativeResultName"].ToString());
                            Point.Add(sdr["Student_Point"].ToString());
                            Grade.Add(sdr["Student_Grade"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(ExamName);
                    chartData.Add(Point);
                    chartData.Add(Grade);

                    return chartData;
                }
            }
        }
    }
}