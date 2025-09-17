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
    public partial class Manage_Sub_Admin_Access : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void UserListDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            con.Open();
            LinkGridView.DataBind();
            foreach (GridViewRow row in LinkGridView.Rows)
            {
                CheckBox LinkCheckBox = (CheckBox)row.FindControl("LinkCheckBox");

                SqlCommand LinkCmd = new SqlCommand("select LinkID from Link_Users where UserName = @UserName and LinkID = @LinkID",con);
                LinkCmd.Parameters.AddWithValue("@UserName", UserListDropDownList.SelectedValue);
                LinkCmd.Parameters.AddWithValue("@LinkID", LinkGridView.DataKeys[row.DataItemIndex].Value.ToString());

                object CheckPages = LinkCmd.ExecuteScalar();

                if (CheckPages != null)
                {
                    LinkCheckBox.Checked = true;
                    row.CssClass = "selected";
                }
            }
            con.Close();
        }

        protected void LinkGridView_DataBound(object sender, EventArgs e)
        {
            int RowSpan = 2;
            for (int i = LinkGridView.Rows.Count - 2; i >= 0; i--)
            {
                GridViewRow currRow = LinkGridView.Rows[i];
                GridViewRow prevRow = LinkGridView.Rows[i + 1];

                if (currRow.Cells[0].Text == prevRow.Cells[0].Text && currRow.Cells[1].Text == prevRow.Cells[1].Text)
                {
                    currRow.Cells[0].RowSpan = RowSpan;
                    prevRow.Cells[0].Visible = false;

                    currRow.Cells[1].RowSpan = RowSpan;
                    prevRow.Cells[1].Visible = false;
                    RowSpan += 1;
                }
                else
                    RowSpan = 2;

            }
        }     

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            SqlCommand RegIDCmd = new SqlCommand("select RegistrationID from Registration where UserName = @UserName and SchoolID = @SchoolID", con);
            RegIDCmd.Parameters.AddWithValue("@UserName", UserListDropDownList.SelectedValue);
            RegIDCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            string RegistrationID = RegIDCmd.ExecuteScalar().ToString();
            con.Close();

            foreach (GridViewRow row in LinkGridView.Rows)
            {
                CheckBox LinkCheckBox = (CheckBox)row.FindControl("LinkCheckBox");

                if (LinkCheckBox.Checked)
                {
                    UpdateLinkSQL.InsertParameters["LinkID"].DefaultValue = LinkGridView.DataKeys[row.DataItemIndex]["LinkID"].ToString();
                    UpdateLinkSQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
                    UpdateLinkSQL.Insert();

                    if (Roles.RoleExists(LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString()))
                    {
                        if (!Roles.IsUserInRole(UserListDropDownList.SelectedValue, LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString()))
                        {
                            Roles.AddUserToRole(UserListDropDownList.SelectedValue, LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString());
                        }
                    }
                }
                else
                {
                    UpdateLinkSQL.DeleteParameters["LinkID"].DefaultValue = LinkGridView.DataKeys[row.DataItemIndex]["LinkID"].ToString();
                    UpdateLinkSQL.Delete();

                    if (Roles.RoleExists(LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString()))
                    {
                        if (Roles.IsUserInRole(UserListDropDownList.SelectedValue, LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString()))
                        {
                            Roles.RemoveUserFromRole(UserListDropDownList.SelectedValue, LinkGridView.DataKeys[row.DataItemIndex]["RoleName"].ToString());
                        }
                    }
                }
            }
        }
    }
}