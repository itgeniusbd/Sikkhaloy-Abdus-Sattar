using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions.Paid_Due
{
    public partial class PaidDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void MonthDropDownList_DataBound(object sender, EventArgs e)
        {
            MonthDropDownList.Items.Insert(0, new ListItem("[ SELECT MONTHS ]", "0"));
        }
    }
}