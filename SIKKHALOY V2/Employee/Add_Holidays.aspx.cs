using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Add_Holidays : System.Web.UI.Page
    {
        SqlDataAdapter Holiday_DA;
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.BindGrid();
            }

            if (!User.Identity.IsAuthenticated | Session["SchoolID"] == null)
            {
                Response.Redirect("~/Login.aspx");
            }
            try
            {
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = " + Session["SchoolID"].ToString() + "", con);
                Holiday_DA.Fill(ds, "Table");
            }
            catch { Response.Redirect("~/Login.aspx"); }

        }

        //Calender
        protected void HolidayCalendar_DayRender(object sender, DayRenderEventArgs e)
        {  // If the month is CurrentMonth
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

        private void BindGrid()
        {
            DataTable dt = new DataTable();
            DataColumn HolidayName = new DataColumn("HolidayName");
            DataColumn Date = new DataColumn("Date");

            dt.Columns.Add(HolidayName);
            dt.Columns.Add(Date);
            // Add ten rows.
            DataRow row;

            row = dt.NewRow();
            row["HolidayName"] = "";
            row["Date"] = "";
            dt.Rows.Add(row);

            HolidaysGridview.DataSource = dt;
            HolidaysGridview.DataBind();
        }
        protected void WeeklyButton_Click(object sender, EventArgs e)
        {
            DateTime StartDate = DateTime.Parse(StartDateTextBox.Text);
            DateTime EndDate = DateTime.Parse(EndDateTextBox.Text);

            foreach (DateTime day in EachDay(StartDate, EndDate))
            {
                foreach (ListItem item in WeekCheckBoxList.Items)
                {
                    if (item.Selected)
                    {
                        if (day.DayOfWeek.ToString() == item.Value)
                        {
                            HolidaySQL.InsertParameters["HolidayName"].DefaultValue = "Weekly Holiday";
                            HolidaySQL.InsertParameters["HolidayDate"].DefaultValue = day.ToShortDateString();
                            HolidaySQL.Insert();
                        }
                    }
                }
            }
        }
     
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.Form["HolidayName"]) && !string.IsNullOrEmpty(Request.Form["Date"]))
            {
                string[] HolidayName = Request.Form["HolidayName"].Split(',');
                string[] HolidayDate = Request.Form["Date"].Split(',');


                for (int i = 0; i < HolidayName.Length; i++)
                {
                    HolidaySQL.InsertParameters["HolidayName"].DefaultValue = HolidayName[i];
                    HolidaySQL.InsertParameters["HolidayDate"].DefaultValue = HolidayDate[i];
                    HolidaySQL.Insert();
                }

                Response.Redirect(Request.Url.AbsoluteUri);
            }
        }

        protected void Multi_HolidayButton_Click(object sender, EventArgs e)
        {
            DateTime StartDate = Convert.ToDateTime(Multi_FromDateTextBox.Text);
            DateTime EndDate = Convert.ToDateTime(Multi_ToDateTextBox.Text);

            foreach (DateTime Date in EachDay(StartDate, EndDate))
            {
                HolidaySQL.InsertParameters["HolidayName"].DefaultValue = Multi_HoliNameTextBox.Text;
                HolidaySQL.InsertParameters["HolidayDate"].DefaultValue = Date.ToShortDateString();
                HolidaySQL.Insert();  
            }

            Response.Redirect(Request.Url.AbsoluteUri);

        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime To)
        {
            for (var day = from.Date; day.Date <= To.Date; day = day.AddDays(1))
                yield return day;
        }

    }
}