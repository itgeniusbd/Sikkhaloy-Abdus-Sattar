﻿using Education;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Profile
{
    public partial class Admin : System.Web.UI.Page
    {
        SqlDataAdapter Holiday_DA;
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null)
            {
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = " + Session["SchoolID"].ToString(), con);
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


        //Session wise Student data
        [WebMethod]
        public static List<object> Get_Session_Student()
        {
            List<object> chartData = new List<object>();
            List<string> EducationYear = new List<string>();
            List<string> Total_Student = new List<string>();

            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            string query = "SELECT Education_Year.EducationYear, COUNT(StudentsClass.StudentClassID) AS Total_Student FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID WHERE(Student.Status = N'Active') AND (StudentsClass.SchoolID = @SchoolID) GROUP BY Education_Year.SN, Education_Year.EducationYear, Education_Year.EducationYearID ORDER BY Education_Year.SN";

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
                            Total_Student.Add(sdr["Total_Student"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(EducationYear);
                    chartData.Add(Total_Student);

                    return chartData;
                }
            }
        }

        //Student gender data
        [WebMethod]
        public static List<object> Get_Gender()
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
                    cmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
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

        //SMS
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

        //Employee
        [WebMethod]
        public static List<object> Get_Employee()
        {
            List<object> chartData = new List<object>();
            List<string> EmployeeType = new List<string>();
            List<string> Total = new List<string>();


            string query = "SELECT EmployeeType, COUNT(EmployeeID) AS Total FROM Employee_Info WHERE(SchoolID = @SchoolID) AND (Job_Status = N'Active') GROUP BY EmployeeType";
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
                            EmployeeType.Add(sdr["EmployeeType"].ToString());
                            Total.Add(sdr["Total"].ToString());
                        }
                    }
                    con.Close();

                    chartData.Add(EmployeeType);
                    chartData.Add(Total);

                    return chartData;
                }
            }
        }

        //Is Birthday sms Sent
        public bool IsSMSsent()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            SqlCommand IsSmsCmd = new SqlCommand("SELECT TOP(1) SMS_Send_Record.SMS_Send_ID FROM SMS_Send_Record INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID WHERE (SMS_OtherInfo.SchoolID = @SchoolID) AND (SMS_Send_Record.PurposeOfSMS ='Birthday') AND CONVERT(date,SMS_Send_Record.Date) = CONVERT(date, GETDATE())", con);
            IsSmsCmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());

            con.Open();
            object IsSent = IsSmsCmd.ExecuteScalar();
            con.Close();

            return IsSent == null;
        }

        protected void SendButton_Click(object sender, EventArgs e)
        {
            if (IsSMSsent())
            {
                SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());
                int TotalSMS = 0;
                string PhoneNo = "";
                string Msg = "";
                int SMSBalance = SMS.SMSBalance;

                #region Count SMS
                foreach (RepeaterItem item in TodayBirthdayRepeater.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var StudentsName = (HiddenField)item.FindControl("StudentsName");
                        var SMSPhoneNo = (HiddenField)item.FindControl("SMSPhoneNo");
                        PhoneNo = SMSPhoneNo.Value;

                        Msg = "Happy birthday to you, " + StudentsName.Value + ". I wish you a successful future. Study hard and don't forget your ambitions in life. You'll certainly go places. Regards, " + Session["School_Name"].ToString();

                        Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                        if (IsValid.Validation)
                        {
                            TotalSMS += SMS.SMS_Conut(Msg);
                        }
                    }
                }
                #endregion Count SMS

                #region Send SMS
                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        foreach (RepeaterItem item in TodayBirthdayRepeater.Items)
                        {
                            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                            {
                                var StudentsName = (HiddenField)item.FindControl("StudentsName");
                                var SMSPhoneNo = (HiddenField)item.FindControl("SMSPhoneNo");
                                var StudentID = (HiddenField)item.FindControl("StudentID");

                                PhoneNo = SMSPhoneNo.Value;

                                Msg = "Happy birthday to you," + StudentsName.Value + ". I wish you a successful future. Study hard and don't forget your ambitions in life. You'll certainly go places. Regards, " + Session["School_Name"].ToString();

                                TotalSMS = SMS.SMS_Conut(Msg);

                                Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                                if (IsValid.Validation)
                                {
                                    Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Birthday");
                                    if (SMS_Send_ID != Guid.Empty)
                                    {
                                        SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                        SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = StudentID.Value;
                                        SMS_OtherInfoSQL.Insert();

                                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('SMS Sent Successfully.')", true);
                                    }
                                    else
                                    {
                                        ErrorLabel.Text = IsValid.Message;
                                    }
                                }
                                else
                                {
                                    ErrorLabel.Text = IsValid.Message;
                                }
                            }

                        }
                    }
                    else
                    {
                        ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                    }
                }
                else
                {
                    ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + SMSBalance;
                }
                #endregion Send SMS
            }
            else
            {
                SendButton.Enabled = false;
                SendButton.Text = "SMS ALREADY SENT";
            }
        }
    }
}
