using EDUCATION.COM.PaymentDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Student_Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        //chart data
        [WebMethod]
        public static List<object> Get_Exam_GradePoint(string EducationYearID)
        {
            List<object> chartData = new List<object>();
            List<string> ExamName = new List<string>();
            List<string> Point = new List<string>();
            List<string> Grade = new List<string>();

            string query = "SELECT Exam_Name.ExamName, Exam_Result_of_Student.Student_Point, Exam_Result_of_Student.Student_Grade FROM Exam_Result_of_Student INNER JOIN  Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID INNER JOIN  Exam_Publish_Setting ON Exam_Result_of_Student.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID WHERE(Exam_Result_of_Student.StudentID = @StudentID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND(Exam_Publish_Setting.IS_Published = 1) ORDER BY Exam_Name.Period_StartDate";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", HttpContext.Current.Session["StudentID"].ToString());
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
        public static List<object> Get_CumilativeExam(string EducationYearID)
        {
            List<object> chartData = new List<object>();
            List<string> ExamName = new List<string>();
            List<string> Point = new List<string>();
            List<string> Grade = new List<string>();

            string query = "SELECT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Student.Student_Point, Exam_Cumulative_Student.Student_Grade FROM Exam_Cumulative_Student INNER JOIN Exam_Cumulative_Name ON Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID INNER JOIN Exam_Cumulative_Setting ON Exam_Cumulative_Student.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID WHERE(Exam_Cumulative_Student.StudentID = @StudentID) AND(Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND(Exam_Cumulative_Setting.IS_Published = 1)";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", HttpContext.Current.Session["StudentID"].ToString());
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

        [WebMethod]
        public static List<object> Get_Attendance()
        {
            List<object> chartData = new List<object>();
            List<string> Attendance = new List<string>();
            List<string> Days = new List<string>();

            string query = "SELECT Attendance,COUNT(StudentClassID) AS [Days] FROM Attendance_Record WHERE (SchoolID = @SchoolID) AND (StudentClassID = @StudentClassID) AND (AttendanceDate BETWEEN DATEADD(DAY, -30,GETDATE()) AND GETDATE()) GROUP BY Attendance";
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@StudentClassID", HttpContext.Current.Session["StudentClassID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            Attendance.Add(sdr["Attendance"].ToString());
                            Days.Add(sdr["Days"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(Attendance);
                    chartData.Add(Days);

                    return chartData;
                }
            }
        }

        [WebMethod]
        public static bool IsOnlinePaymentApplicable()
        {
            bool isOnlinePaymentApplicable = false;

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT OnlinePaymentEnable FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            int onlinePaymentEnable = Int32.Parse(sdr["OnlinePaymentEnable"].ToString());
                            isOnlinePaymentApplicable = onlinePaymentEnable == 1;
                        }
                    }
                    conn.Close();

                }
            }

            return isOnlinePaymentApplicable;

        }
    }
}