using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Acadamic_Calender : System.Web.UI.Page
    {
      SqlDataAdapter Holiday_DA;
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        
        protected void Page_Load(object sender, EventArgs e)
   {
            try
   {
         // Check if Session is valid
  if (Session["SchoolID"] == null || string.IsNullOrEmpty(Session["SchoolID"].ToString()))
    {
          Response.Redirect("~/Login.aspx");
     return;
        }

             Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = @SchoolID", con);
        Holiday_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
    Holiday_DA.Fill(ds, "Table");
   }
            catch (Exception ex)
            {
   // Log the error for debugging
     System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
     Response.Redirect("~/Login.aspx");
    }
        }
        
        protected void HolidayCalendar_DayRender(object sender, DayRenderEventArgs e)
        {  
     // Clear default text
   e.Cell.Text = "";
         
// If the month is CurrentMonth
            if (!e.Day.IsOtherMonth)
            {
   // Create container div for multi-language dates
      HtmlGenericControl dateContainer = new HtmlGenericControl("div");
dateContainer.Attributes["class"] = "calendar-date-container";
      
    // English Date (Biggest & Bold)
                Label englishDate = new Label();
          englishDate.Text = e.Day.Date.Day.ToString();
      englishDate.CssClass = "calendar-english-date";
                dateContainer.Controls.Add(englishDate);
   
            // Bangla Date (Green box)
 Label banglaDate = new Label();
     banglaDate.Text = GetBanglaDate(e.Day.Date);
       banglaDate.CssClass = "calendar-bangla-date";
      dateContainer.Controls.Add(banglaDate);
          
     // Arabic/Hijri Date (Purple box)
      Label hijriDate = new Label();
         hijriDate.Text = GetHijriDate(e.Day.Date);
      hijriDate.CssClass = "calendar-hijri-date";
      dateContainer.Controls.Add(hijriDate);
             
  // Add "TODAY" badge if it's today
     if (e.Day.Date == DateTime.Today)
                {
       Label todayBadge = new Label();
todayBadge.Text = "TODAY";
        todayBadge.CssClass = "calendar-today-badge";
              dateContainer.Controls.Add(todayBadge);
     }
          
        e.Cell.Controls.Add(dateContainer);
          
        // Check for holidays
        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
     {
   foreach (DataRow dr in ds.Tables[0].Rows)
        {
                if (dr["HolidayDate"] != DBNull.Value)
            {
       DateTime dtEvent = (DateTime)dr["HolidayDate"];
   
    if (dtEvent.Equals(e.Day.Date))
     {
      e.Cell.CssClass = "Evnt_Date";
          
   Label lbl = new Label();
          lbl.Text = dr["HolidayName"].ToString();
    lbl.CssClass = "Appointment";
    
        e.Cell.Controls.Add(lbl);
       }
             }
     }
          }
}
            //If the month is not CurrentMonth then make it semi-transparent
            else
    {
          e.Cell.Text = e.Day.Date.Day.ToString();
      e.Cell.ForeColor = System.Drawing.Color.Silver;
        }
        }
        
        // Convert English date to Bangla date
        private string GetBanglaDate(DateTime date)
        {
    try
            {
         // Bangla calendar months
        string[] banglaMonths = { 
        "বৈশাখ", "জ্যৈষ্ঠ", "আষাঢ়", "শ্রাবণ", 
            "ভাদ্র", "আশ্বিন", "কার্তিক", "অগ্রহায়ণ", 
     "পৌষ", "মাঘ", "ফাল্গুন", "চৈত্র" 
     };
     
        // Bangla digits
 string[] banglaDigits = { "০", "১", "২", "৩", "৪", "৫", "৬", "৭", "৮", "৯" };
                
     // Approximate Bangla date calculation
     // Bengali New Year starts around April 14-15
   int dayOfYear = date.DayOfYear;
  int bengaliYear = date.Year - 593;
     
                // Adjust for Bengali calendar starting around mid-April
    if (dayOfYear < 105) // Before April 15
    {
               bengaliYear--;
      dayOfYear += 260; // Adjust day count
    }
              else
       {
     dayOfYear -= 105;
          }
         
           // Calculate month and day (approximate)
    int bengaliMonth = (dayOfYear / 30);
         int bengaliDay = (dayOfYear % 30) + 1;
    
    if (bengaliMonth >= 12) bengaliMonth = 11;
       
       // Convert day to Bangla digits
          string banglaDay = "";
             foreach (char c in bengaliDay.ToString())
  {
    banglaDay += banglaDigits[int.Parse(c.ToString())];
            }
        
   return banglaDay + " " + banglaMonths[bengaliMonth];
          }
            catch
  {
        return "তারিখ";
  }
  }
        
        // Convert to Hijri date
        private string GetHijriDate(DateTime date)
      {
       try
            {
      // Use Hijri calendar
            System.Globalization.HijriCalendar hijri = new System.Globalization.HijriCalendar();
        
  // Hijri month names in Arabic
      string[] hijriMonths = { 
          "محرم", "صفر", "ربيع الأول", "ربيع الثاني", 
          "جمادى الأولى", "جمادى الآخرة", "رجب", "شعبان", 
     "رمضان", "شوال", "ذو القعدة", "ذو الحجة" 
          };
                
      // Arabic-Indic digits
    string[] arabicDigits = { "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩" };
          
            int day = hijri.GetDayOfMonth(date);
         int month = hijri.GetMonth(date);
     
     // Convert day to Arabic digits
    string arabicDay = "";
           foreach (char c in day.ToString())
    {
             arabicDay += arabicDigits[int.Parse(c.ToString())];
          }
      
         return arabicDay + " " + hijriMonths[month - 1];
            }
            catch
         {
     return "تاريخ";
            }
  }
    }
}