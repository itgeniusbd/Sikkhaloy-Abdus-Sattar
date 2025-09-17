using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.ContactUs
{
    public partial class Message_List : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ContactLinkButton_Command(object sender, CommandEventArgs e)
        {
            MsgSQL.UpdateParameters["ContactUsID"].DefaultValue = e.CommandName.ToString();
            MsgSQL.Update();
        }

        protected void SupportLinkButton_Command(object sender, CommandEventArgs e)
        {
            SupportSQl.UpdateParameters["SupportID"].DefaultValue = e.CommandName.ToString();
            SupportSQl.Update();
        }
    }
}