using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Profile
{
    public partial class Sub_Admin : System.Web.UI.Page
    {
        SqlDataAdapter Holiday_DA;
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null)
            {
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = " + Session["SchoolID"].ToString() + "", con);
                Holiday_DA.Fill(ds, "Table");
            }
        }

        //Acadamic calendar
        protected void HolidayCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            // If the month is CurrentMonth
            if (!e.Day.IsOtherMonth)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if ((dr["HolidayDate"].ToString() != DBNull.Value.ToString()))
                    {
                        DateTime dtEvent = (DateTime)dr["HolidayDate"];
                        Label lbl = new Label();
                        lbl.CssClass = "Appointment";

                        if (dtEvent.Equals(e.Day.Date))
                        {
                            e.Cell.CssClass = "Evnt_Date";

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


        //chart data
        [WebMethod]
        public static List<object> Get_Class_Student(string EducationYearID)
        {
            List<object> chartData = new List<object>();

            List<string> Class = new List<string>();
            List<string> Total_Student = new List<string>();


            string query = "SELECT CreateClass.Class, COUNT(CreateClass.Class) AS Total_Student FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.Status = N'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.SchoolID = @SchoolID) GROUP BY CreateClass.Class, StudentsClass.ClassID ORDER BY StudentsClass.ClassID";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Class.Add(sdr["Class"].ToString());
                            Total_Student.Add(sdr["Total_Student"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(Class);
                    chartData.Add(Total_Student);

                    return chartData;
                }
            }
        }

        //Student data
        [WebMethod]
        public static List<object> Get_Gender(string EducationYearID)
        {
            List<object> chartData = new List<object>();
            List<string> Label = new List<string>();
            List<string> Value = new List<string>();

            string query = "SELECT Student.Gender, COUNT(StudentsClass.StudentClassID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE(StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') GROUP BY Student.Gender";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Label.Add(sdr["Gender"].ToString());
                            Value.Add(sdr["Total"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(Label);
                    chartData.Add(Value);

                    return chartData;
                }
            }
        }

        [WebMethod]
        public static List<object> Get_SentSMS()
        {
            List<object> chartData = new List<object>();

            List<string> EducationYear = new List<string>();
            List<string> Session_sent = new List<string>();


            string query = "SELECT Education_Year.EducationYearID, Education_Year.EducationYear, ISNULL(SUM(SMS_Send_Record.SMSCount),0) AS Session_sent FROM SMS_Send_Record INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID INNER JOIN Education_Year ON SMS_OtherInfo.EducationYearID = Education_Year.EducationYearID WHERE (SMS_OtherInfo.SchoolID = @SchoolID) GROUP BY Education_Year.EducationYear, Education_Year.EducationYearID ORDER BY Education_Year.EducationYearID";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            EducationYear.Add(sdr["EducationYear"].ToString());
                            Session_sent.Add(sdr["Session_sent"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(EducationYear);
                    chartData.Add(Session_sent);

                    return chartData;
                }
            }
        }
    }
}