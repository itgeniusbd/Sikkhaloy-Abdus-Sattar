using Education;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

                // Due Invoice চেক করে পপআপ নোটিশ দেখানো
                if (!IsPostBack)
                {
                    CheckAndShowDueInvoiceNotification();
                }
            }
        }

        // Due Invoice এর টোটাল চেক করে নোটিফিকেশন দেখানোর মেথড
        private void CheckAndShowDueInvoiceNotification()
        {
            try
            {
                // প্রথমে চেক করি notification enable করা আছে কিনা
                if (!IsDueNoticeEnabled())
                {
                    return; // Enable না থাকলে notification দেখাবে না
                }

                string query = @"SELECT COUNT(*) AS DueRecordCount, 
                                       ISNULL(SUM(AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount), 0) AS TotalDue
                                FROM AAP_Invoice 
                                WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0)";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int dueRecordCount = Convert.ToInt32(reader["DueRecordCount"]);
                                decimal totalDue = Convert.ToDecimal(reader["TotalDue"]);

                                if (dueRecordCount > 0 && totalDue > 0)
                                {
                                    // Modal পপআপ দেখানোর জন্য JavaScript কোড
                                    string script = $@"
                                        $(document).ready(function() {{
                                            $('#dueRecordCount').text('{dueRecordCount}');
                                            $('#totalDueAmount').text('{totalDue:N2}');
                                            $('#dueInvoiceModal').modal('show');
                                        }});
                                    ";
                                    
                                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowDueInvoiceModal", 
                                        script, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Error handling - লগ করা যেতে পারে কিন্তু ইউজারকে দেখানো হবে না
                System.Diagnostics.Debug.WriteLine("Due Invoice notification error: " + ex.Message);
            }
        }

        // চেক করে যে Due Notice Enable করা আছে কিনা (নতুন লজিক - ডিফল্ট বন্ধ)
        private bool IsDueNoticeEnabled()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TOP 1 IsEnabled, HideUntilDate 
                                    FROM SchoolInfo_DueNoticeSettings 
                                    WHERE SchoolID = @SchoolID 
                                    ORDER BY CreatedDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isEnabled = reader.GetBoolean(reader.GetOrdinal("IsEnabled"));
                                
                                if (!isEnabled)
                                {
                                    return false; // Disabled থাকলে নোটিশ দেখাবে না
                                }
                                
                                // Enable থাকলে চেক করি HideUntilDate আছে কিনা
                                if (!reader.IsDBNull(reader.GetOrdinal("HideUntilDate")))
                                {
                                    DateTime hideUntilDate = reader.GetDateTime(reader.GetOrdinal("HideUntilDate"));
                                    if (DateTime.Now <= hideUntilDate)
                                    {
                                        return false; // Hide period চলছে
                                    }
                                }
                                
                                return true; // Enable আছে এবং Hide period শেষ
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error checking due notice enabled status: " + ex.Message);
            }

            return false; // ডিফল্ট - কোনো setting না থাকলে নোটিশ দেখাবে না
        }

        // Acadamic calendar with Arabic, Bangla and English dates
        protected void HolidayCalendar_DayRender(object sender, DayRenderEventArgs e)
        {
            // Clear default content
            e.Cell.Controls.Clear();

            // If the month is CurrentMonth
            if (!e.Day.IsOtherMonth)
            {
                // Create container for multi-language dates
                Panel dateContainer = new Panel();
                dateContainer.CssClass = "calendar-date-container";

                // 1. English Date (First Line - Largest)
                Label englishDate = new Label();
                englishDate.CssClass = "calendar-english-date";
                englishDate.Text = e.Day.Date.Day.ToString();
                dateContainer.Controls.Add(englishDate);

                // 2. Bangla Date (Second Line)
                Label banglaDate = new Label();
                banglaDate.CssClass = "calendar-bangla-date";
                banglaDate.Text = "🇧🇩 " + GetBanglaDate(e.Day.Date);
                dateContainer.Controls.Add(banglaDate);

                // 3. Arabic/Hijri Date (Third Line)
                Label hijriDate = new Label();
                hijriDate.CssClass = "calendar-hijri-date";
                hijriDate.Text = GetHijriDate(e.Day.Date) + " 🕌";
                dateContainer.Controls.Add(hijriDate);

                // Check if today
                if (e.Day.Date == DateTime.Today)
                {
                    e.Cell.CssClass = "myCalendarToday";
                    Label todayBadge = new Label();
                    todayBadge.CssClass = "calendar-today-badge";
                    todayBadge.Text = "TODAY";
                    dateContainer.Controls.Add(todayBadge);
                }

                e.Cell.Controls.Add(dateContainer);

                // Check for holidays/events
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if ((dr["HolidayDate"].ToString() != DBNull.Value.ToString()))
                    {
                        DateTime dtEvent = (DateTime)dr["HolidayDate"];
                        
                        if (dtEvent.Equals(e.Day.Date))
                        {
                            e.Cell.CssClass += " Evnt_Date";
                            Label lbl = new Label();
                            lbl.CssClass = "Appointment";
                            lbl.Text = "📅 " + dr["HolidayName"].ToString();
                            e.Cell.Controls.Add(lbl);
                        }
                    }
                }
            }
            //If the month is not CurrentMonth then hide the Dates
            else
            {
                e.Cell.Text = "";
                e.Cell.Enabled = false;
            }
        }

        // Get Bangla Date
        private string GetBanglaDate(DateTime date)
        {
            try
            {
                // Bangla month names
                string[] banglaMonths = { "বৈশাখ", "জ্যৈেষ্ঠ", "আষাঢ়", "শ্রাবণ", "ভাদ্র", "আশ্বিন",
      "কার্তিক", "অগ্রহায়ণ", "পৌষ", "মাঘ", "ফাল্গুন", "চৈত্র" };

                // Bangla digits
                string[] banglaDigits = { "০", "১", "২", "৩", "৪", "৫", "৬", "৭", "৮", "৯" };

                // Bengali calendar epoch: 593 years behind Gregorian
                int gYear = date.Year;
                int gMonth = date.Month;
                int gDay = date.Day;

                // Determine if leap year in Gregorian calendar
                bool isLeapYear = DateTime.IsLeapYear(gYear);

                // Bengali New Year starts on April 14 (or April 15 in leap years)
                int pohela = isLeapYear ? 15 : 14;

                int banglaYear, banglaMonth, banglaDay;

                if (gMonth < 4 || (gMonth == 4 && gDay < pohela))
                {
                    // Before Bengali New Year - previous Bengali year
                    banglaYear = gYear - 594;
                    DateTime bengaliNewYear = new DateTime(gYear - 1, 4, pohela);
                    TimeSpan diff = date - bengaliNewYear;
                    int totalDays = diff.Days + 1;
                    CalculateBengaliMonthDay(totalDays, out banglaMonth, out banglaDay);
                }
                else
                {
                    // After or on Bengali New Year - current Bengali year
                    banglaYear = gYear - 593;
                    DateTime bengaliNewYear = new DateTime(gYear, 4, pohela);
                    TimeSpan diff = date - bengaliNewYear;
                    int totalDays = diff.Days + 1;
                    CalculateBengaliMonthDay(totalDays, out banglaMonth, out banglaDay);
                }

                // Convert day to Bangla digits
                string banglaDayStr = "";
                foreach (char digit in banglaDay.ToString())
                {
                    banglaDayStr += banglaDigits[int.Parse(digit.ToString())];
                }

                return string.Format("{0} {1}", banglaDayStr, banglaMonths[banglaMonth - 1]);
            }
            catch
            {
                return "";
            }
        }

        // Calculate Bengali month and day from total days
        private void CalculateBengaliMonthDay(int totalDays, out int month, out int day)
        {
            int[] monthDays = { 31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 30 };
            month = 1;
            day = totalDays;

            for (int i = 0; i < 12; i++)
            {
                if (day <= monthDays[i])
                {
                    month = i + 1;
                    break;
                }
                day -= monthDays[i];
            }

            // Handle edge case
            if (day > monthDays[month - 1])
            {
                day = 1;
                month++;
                if (month > 12)
                {
                    month = 1;
                }
            }
        }

        // Get Hijri/Arabic Date
        private string GetHijriDate(DateTime date)
        {
            try
            {
                // Create Hijri calendar
                HijriCalendar hijriCalendar = new HijriCalendar();
    
                // Arabic month names
                string[] arabicMonths = { "محرم", "صفر", "ربيع الأول", "ربيع الثاني", "جمادى الأولى", "جمادى الثانية",
            "رجب", "شعبان", "رمضان", "شوال", "ذو القعدة", "ذو الحجة" };
    
              // Get Hijri date components
              int hijriDay = hijriCalendar.GetDayOfMonth(date);
                int hijriMonth = hijriCalendar.GetMonth(date);
               int hijriYear = hijriCalendar.GetYear(date);
       
  // Arabic digits (Eastern Arabic numerals)
     string[] arabicDigits = { "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩" };
      
             // Convert day to Arabic digits
        string arabicDayStr = "";
                foreach (char digit in hijriDay.ToString())
                {
         arabicDayStr += arabicDigits[int.Parse(digit.ToString())];
    }
     
                return string.Format("{0} {1}", arabicDayStr, arabicMonths[hijriMonth - 1]);
      }
            catch
            {
  return "";
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
