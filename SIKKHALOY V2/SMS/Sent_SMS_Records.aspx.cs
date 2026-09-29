using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.SMS
{
    public partial class Sent_SMS_Records : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrWhiteSpace(FromDateTextBox.Text))
                    FromDateTextBox.Text = DateTime.Today.ToString("dd MMM yyyy");
                if (string.IsNullOrWhiteSpace(ToDateTextBox.Text))
                    ToDateTextBox.Text = DateTime.Today.ToString("dd MMM yyyy");
            }
        }

        protected void RecordsSQL_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            e.Command.CommandTimeout = 60;
        }

    }
}