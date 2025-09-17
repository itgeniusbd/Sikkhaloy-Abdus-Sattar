using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class Active_deactivate_Sub_Admin : System.Web.UI.Page
    {
       protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ApprovedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ApprovedCheckBox = (CheckBox)sender;
            GridViewRow Row = (GridViewRow)ApprovedCheckBox.Parent.Parent;

            MembershipUser usr = Membership.GetUser(SubAdminGV.DataKeys[Row.DataItemIndex]["UserName"].ToString());
            usr.IsApproved = ApprovedCheckBox.Checked;
            Membership.UpdateUser(usr);

        }

        protected void LockedOutCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox LockedOutCheckBox = (CheckBox)sender;
            GridViewRow Row = (GridViewRow)LockedOutCheckBox.Parent.Parent;

            MembershipUser usr = Membership.GetUser(SubAdminGV.DataKeys[Row.DataItemIndex]["UserName"].ToString());
            usr.UnlockUser();

            LockedOutCheckBox.Checked = usr.IsLockedOut;
        }
    }
}