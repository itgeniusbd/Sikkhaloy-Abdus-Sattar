using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Section_Shift_Group_Change_ofStudent
{
    public partial class Change_Section_Shift_Group : System.Web.UI.Page
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
        }

        //Class DDL
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

        //Group DDL
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        //Section DDL
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //Shift DDL
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        protected void ChangeButton_Click(object sender, EventArgs e)
        {
            string GroupID = "";
            string SectionID = "";
            string ShiftID = "";

            if (GroupDropDownList.SelectedValue == "%")
            {
                GroupID = "0";
            }
            else
            {
                GroupID = GroupDropDownList.SelectedValue;
            }

            if (SectionDropDownList.SelectedValue == "%")
            {
                SectionID = "0";
            }
            else
            {
                SectionID = SectionDropDownList.SelectedValue;
            }

            if (ShiftDropDownList.SelectedValue == "%")
            {
                ShiftID = "0";
            }
            else
            {
                ShiftID = ShiftDropDownList.SelectedValue;
            }

            UpdateStudentClassSQL.UpdateParameters["SubjectGroupID"].DefaultValue = GroupID;
            UpdateStudentClassSQL.UpdateParameters["SectionID"].DefaultValue = SectionID;
            UpdateStudentClassSQL.UpdateParameters["ShiftID"].DefaultValue = ShiftID;

            bool IsUpdate = false;
            foreach (GridViewRow Allrow in StudentsGridView.Rows)
            {
                CheckBox SelectCheckBox = (CheckBox)Allrow.FindControl("SelectCheckBox");

                if (SelectCheckBox.Checked)
                {
                    UpdateStudentClassSQL.UpdateParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Allrow.RowIndex]["StudentClassID"].ToString();
                    UpdateStudentClassSQL.Update();
                    IsUpdate = true;
                }
            }

            if (IsUpdate)
            {
                StudentsGridView.DataBind();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Changed Successfully')", true);
            }
        }
    }
}