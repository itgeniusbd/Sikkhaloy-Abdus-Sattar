using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public partial class Remove_Pay_order : System.Web.UI.Page
    {
        string deleteFilter = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (Session["SchoolID"] != null)
                    Session_DropDownList.SelectedValue = Session["Edu_Year"].ToString();
            }
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DueGridView.DataBind();
            IDTextBox.Text = "";
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
                CheckBox SingleCheckBox = row.FindControl("SingleCheckBox") as CheckBox;
                SingleCheckBox.Checked = true;
            }
        }
      
        protected void Role_Find_Button_Click(object sender, EventArgs e)
        {
            string Filtering = "";

            if (IDTextBox.Text != "")
            {
                string SIDs = "";
                bool S_check = false;
                foreach (GridViewRow Student_Row in StudentsGridView.Rows)
                {
                    CheckBox SingleCheckBox = Student_Row.FindControl("SingleCheckBox") as CheckBox;
                    if (SingleCheckBox.Checked)
                    {
                        SIDs += StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentID"].ToString() + ",";
                        S_check = true;
                    }
                }
                if (S_check)
                {
                    Filtering += "StudentID in(" + SIDs.TrimEnd(',') + ")";
                }
                else
                {
                    Filtering += "StudentID in(0)";
                }


                string RIDs = "";
                bool R_check = false;
                foreach (GridViewRow New_Role_Row in AddNewRoleGridView.Rows)
                {
                    CheckBox AddCheckBox = New_Role_Row.FindControl("AddCheckBox") as CheckBox;

                    if (AddCheckBox.Checked)
                    {
                        RIDs += AddNewRoleGridView.DataKeys[New_Role_Row.RowIndex]["RoleID"].ToString() + ",";
                        R_check = true;
                    }
                }

                if (R_check)
                {
                    Filtering += "and RoleID in(" + RIDs.TrimEnd(',') + ")";
                }
                else
                {
                    Filtering += "and RoleID in(0)";
                }

                deleteFilter = Filtering;
                DueSQL.FilterExpression = Filtering;
                DueGridView.DataSource = DueSQL;
                DueGridView.DataBind();
            }
            else
            {
                if (ClassDropDownList.SelectedValue != "-1")
                {
                    Filtering += "ClassID =" + ClassDropDownList.SelectedValue;

                    string SIDs = "";
                    bool S_check = false;
                    foreach (GridViewRow Student_Row in StudentsGridView.Rows)
                    {
                        CheckBox SingleCheckBox = Student_Row.FindControl("SingleCheckBox") as CheckBox;
                        if (SingleCheckBox.Checked)
                        {
                            SIDs += StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentID"].ToString() + ",";
                            S_check = true;
                        }
                    }
                    if (S_check)
                    {
                        Filtering += "and StudentID in(" + SIDs.TrimEnd(',') + ")";
                    }
                    else
                    {
                        Filtering += "and StudentID in(0)";
                    }


                    string RIDs = "";
                    bool R_check = false;
                    foreach (GridViewRow New_Role_Row in AddNewRoleGridView.Rows)
                    {
                        CheckBox AddCheckBox = New_Role_Row.FindControl("AddCheckBox") as CheckBox;

                        if (AddCheckBox.Checked)
                        {
                            RIDs += AddNewRoleGridView.DataKeys[New_Role_Row.RowIndex]["RoleID"].ToString() + ",";
                            R_check = true;
                        }
                    }
                    if (R_check)
                    {
                        Filtering += "and RoleID in(" + RIDs.TrimEnd(',') + ")";
                    }
                    else
                    {
                        Filtering += "and RoleID in(0)";
                    }

                    deleteFilter = Filtering;
                    DueSQL.FilterExpression = Filtering;
                    DueGridView.DataSource = DueSQL;
                    DueGridView.DataBind();
                }
                else
                {
                    string RIDs = "";
                    bool R_check = false;

                    foreach (GridViewRow New_Role_Row in AddNewRoleGridView.Rows)
                    {
                        CheckBox AddCheckBox = New_Role_Row.FindControl("AddCheckBox") as CheckBox;

                        if (AddCheckBox.Checked)
                        {
                            RIDs += AddNewRoleGridView.DataKeys[New_Role_Row.RowIndex]["RoleID"].ToString() + ",";
                            R_check = true;
                        }
                    }
                    if (R_check)
                    {
                        Filtering += "RoleID in(" + RIDs.TrimEnd(',') + ")";

                    }
                    else
                    {
                        Filtering += "RoleID in(0)";
                    }

                    deleteFilter = Filtering;
                    DueSQL.FilterExpression = Filtering;
                    DueGridView.DataSource = DueSQL;
                    DueGridView.DataBind();
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        protected void RemoveOrderButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (GridViewRow Row in DueGridView.Rows)
                {
                    CheckBox AddCheckBox = Row.FindControl("AddCheckBox") as CheckBox;
                    if (AddCheckBox.Checked)
                    {
                        RemovePayOrderSQL.DeleteParameters["PayOrderID"].DefaultValue = DueGridView.DataKeys[Row.RowIndex]["PayOrderID"].ToString();
                        RemovePayOrderSQL.Delete();
                    }
                }

                Role_Find_Button_Click(null, null);
            }
            catch { }
        }

    }
}