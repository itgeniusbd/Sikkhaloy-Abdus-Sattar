using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.AccountsAnalysis
{
    public partial class Paid_And_Due_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }

        protected void RoleDropDownList_DataBound(object sender, EventArgs e)
        {
            RoleDropDownList.Items.Insert(0, new ListItem("[ ALL ROLE ]", "%"));
        }

        protected void PayForDropDownList_DataBound(object sender, EventArgs e)
        {
            PayForDropDownList.Items.Insert(0, new ListItem("[ ALL PAY FOR ]", "%"));
        }
    }
}