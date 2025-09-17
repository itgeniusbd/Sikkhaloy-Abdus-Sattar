using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Reports
{
    public partial class PayOrderReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LinkButton1_Command(object sender, CommandEventArgs e)
        {
            Role_Label.Text = e.CommandArgument.ToString();
        }
    }
}