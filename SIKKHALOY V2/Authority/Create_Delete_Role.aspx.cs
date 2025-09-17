using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority
{
    public partial class Create_Delete_Role : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                DisplayRolesInGrid();
        }

        private void DisplayRolesInGrid()
        {
            RoleList.DataSource = Roles.GetAllRoles();
            RoleList.DataBind();
        }

        protected void CreateRoleButton_Click(object sender, EventArgs e)
        {
            string newRoleName = RoleName.Text.Trim();

            if (!Roles.RoleExists(newRoleName))
            {
                Roles.CreateRole(newRoleName);
                DisplayRolesInGrid();
            }

            RoleName.Text = string.Empty;
        }

        protected void RoleList_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                Label RoleNameLabel = RoleList.Rows[e.RowIndex].FindControl("RoleNameLabel") as Label;
                Roles.DeleteRole(RoleNameLabel.Text, true);

                DisplayRolesInGrid();
            }
            catch (ProviderException ex)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + ex.Message + "')", true);
            }
        }
    }
}