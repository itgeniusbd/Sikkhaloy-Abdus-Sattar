using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public partial class Remove_Pay_order : System.Web.UI.Page
    {
        private string DeleteFilter
        {
            get { return ViewState["DeleteFilter"] as string ?? string.Empty; }
            set { ViewState["DeleteFilter"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["SchoolID"] != null)
                Session_DropDownList.SelectedValue = Session["Edu_Year"].ToString();
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DueGridView.DataBind();
            IDTextBox.Text = string.Empty;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
        }

        protected void Find_ID_Button_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedValue = "0";
            StudentsGridView.DataBind();

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                CheckBox singleCheckBox = row.FindControl("SingleCheckBox") as CheckBox;
                if (singleCheckBox != null)
                    singleCheckBox.Checked = true;
            }
        }

        protected void Role_Find_Button_Click(object sender, EventArgs e)
        {
            string filtering = BuildDueFilter();
            DeleteFilter = filtering;
            DueSQL.FilterExpression = filtering;
            DueGridView.DataSource = DueSQL;
            DueGridView.DataBind();
            ScriptManager.RegisterStartupScript(this, GetType(), "Pop", "openModal();", true);
        }

        protected void RefreshDueGridButton_Click(object sender, EventArgs e)
        {
            RebindDueGridView();
            ScriptManager.RegisterStartupScript(this, GetType(), "Pop", "openModal();", true);
        }

        private string BuildDueFilter()
        {
            string filtering;

            if (!string.IsNullOrWhiteSpace(IDTextBox.Text))
            {
                filtering = "StudentID in(" + GetCheckedStudentIds() + ")";
                filtering += "and RoleID in(" + GetCheckedRoleIds() + ")";
            }
            else if (ClassDropDownList.SelectedValue != "-1")
            {
                filtering = "ClassID =" + ClassDropDownList.SelectedValue;
                filtering += "and StudentID in(" + GetCheckedStudentIds() + ")";
                filtering += "and RoleID in(" + GetCheckedRoleIds() + ")";
            }
            else
            {
                filtering = "RoleID in(" + GetCheckedRoleIds() + ")";
            }

            string payForFilter = GetCheckedPayForFilter();
            if (!string.IsNullOrEmpty(payForFilter))
                filtering += payForFilter;

            return filtering;
        }

        private string GetCheckedPayForFilter()
        {
            List<string> selectedPayFors = new List<string>();

            foreach (GridViewRow payForRow in PayForGridView.Rows)
            {
                CheckBox payForCheckBox = payForRow.FindControl("PayForCheckBox") as CheckBox;
                if (payForCheckBox != null && payForCheckBox.Checked)
                {
                    string payFor = PayForGridView.DataKeys[payForRow.RowIndex]["PayFor"].ToString()
                        .Replace("'", "''");
                    if (!string.IsNullOrWhiteSpace(payFor))
                        selectedPayFors.Add(payFor);
                }
            }

            if (selectedPayFors.Count == 0)
                return string.Empty;

            return "and PayFor in ('" + string.Join("','", selectedPayFors) + "')";
        }

        private string GetCheckedStudentIds()
        {
            string sIds = string.Empty;
            bool hasSelection = false;

            foreach (GridViewRow studentRow in StudentsGridView.Rows)
            {
                CheckBox singleCheckBox = studentRow.FindControl("SingleCheckBox") as CheckBox;
                if (singleCheckBox != null && singleCheckBox.Checked)
                {
                    sIds += StudentsGridView.DataKeys[studentRow.RowIndex]["StudentID"] + ",";
                    hasSelection = true;
                }
            }

            return hasSelection ? sIds.TrimEnd(',') : "0";
        }

        private string GetCheckedRoleIds()
        {
            string rIds = string.Empty;
            bool hasSelection = false;

            foreach (GridViewRow roleRow in AddNewRoleGridView.Rows)
            {
                CheckBox addCheckBox = roleRow.FindControl("AddCheckBox") as CheckBox;
                if (addCheckBox != null && addCheckBox.Checked)
                {
                    rIds += AddNewRoleGridView.DataKeys[roleRow.RowIndex]["RoleID"] + ",";
                    hasSelection = true;
                }
            }

            return hasSelection ? rIds.TrimEnd(',') : "0";
        }

        private void RebindDueGridView()
        {
            if (string.IsNullOrWhiteSpace(DeleteFilter))
                return;

            DueSQL.FilterExpression = DeleteFilter;
            DueGridView.DataSource = DueSQL;
            DueGridView.DataBind();
        }
    }
}
