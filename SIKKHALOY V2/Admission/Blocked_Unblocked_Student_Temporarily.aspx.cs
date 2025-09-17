using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace EDUCATION.COM.ADMISSION_REGISTER
{
    public partial class Blocked_Unblocked_Student_Temporarily : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
        }


        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }
            StudentGridView.DataSource = StudentSQL;
            StudentGridView.DataBind();
            IDTextBox.Text = string.Empty;

        }

        //---------------------------------------Class DDL-------------------------------------------
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
        }


        //---------------------------------------Group DDL-------------------------------------------
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }


        //---------------------------------------Section DDL-------------------------------------------
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //---------------------------------------Shift DDL-------------------------------------------
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        //---------------------------------------End DD-------------------------------------------

        protected void ApprovedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ApprovedCheckBox = (CheckBox)sender;
            GridViewRow Row = (GridViewRow)ApprovedCheckBox.Parent.Parent;

            MembershipUser usr = Membership.GetUser(StudentGridView.DataKeys[Row.DataItemIndex]["UserName"].ToString());
            usr.IsApproved = ApprovedCheckBox.Checked;
            Membership.UpdateUser(usr);

        }

        protected void LockedOutCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox LockedOutCheckBox = (CheckBox)sender;
            GridViewRow Row = (GridViewRow)LockedOutCheckBox.Parent.Parent;

            MembershipUser usr = Membership.GetUser(StudentGridView.DataKeys[Row.DataItemIndex]["UserName"].ToString());
            usr.UnlockUser();

            LockedOutCheckBox.Checked = usr.IsLockedOut;
        }

        protected void DeleteLinkButton_Command(object sender, CommandEventArgs e)
        {
            Membership.DeleteUser(e.CommandName.ToString());

            LITSQL.DeleteParameters["UserName"].DefaultValue = e.CommandName.ToString();
            LITSQL.DeleteParameters["RegistrationID"].DefaultValue = e.CommandArgument.ToString();
            LITSQL.Delete();

            LinkButton DeleteLinkButton = (LinkButton)sender;
            GridViewRow Row = (GridViewRow)DeleteLinkButton.Parent.Parent;
            Row.Visible = false;
        }

        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedIndex = 0;

            GroupDropDownList.Visible = false;
            SectionDropDownList.Visible = false;
            ShiftDropDownList.Visible = false;

            StudentGridView.DataSource = Student_ID_SQL;
            StudentGridView.DataBind();
        }
    }
}