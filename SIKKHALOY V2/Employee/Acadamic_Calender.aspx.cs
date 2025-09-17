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
    public partial class Acadamic_Calender : System.Web.UI.Page
    {
        SqlDataAdapter Holiday_DA;
        DataSet ds = new DataSet();
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Holiday_DA = new SqlDataAdapter("Select * FROM Employee_Holiday Where SchoolID = @SchoolID", con);
                Holiday_DA.SelectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Holiday_DA.Fill(ds, "Table");
            }
            catch
            {
                Response.Redirect("~/Login.aspx");
            }
        }
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

                        if (dtEvent.Equals(e.Day.Date))
                        {
                            e.Cell.CssClass = "Evnt_Date";

                            lbl.Text = "<br />";
                            lbl.Text += dr["HolidayName"].ToString();
                            lbl.CssClass = "Appointment";

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