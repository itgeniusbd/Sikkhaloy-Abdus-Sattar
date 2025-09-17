using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission
{
    public partial class Student_Class_Info_update : System.Web.UI.Page
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

        //Group DDL
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ GROUP ]", "%"));
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
            SectionDropDownList.Items.Insert(0, new ListItem("[ SECTION ]", "%"));
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
            ShiftDropDownList.Items.Insert(0, new ListItem("[ SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }


        //Find
        protected void Find_Button_Click(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
        }


        protected void ChangeButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow Stu_row in StudentsGridView.Rows)
            {
                TextBox RollTextBox = (TextBox)Stu_row.FindControl("RollTextBox");

                StudentClassSQL.UpdateParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Stu_row.DataItemIndex]["StudentClassID"].ToString();
                StudentClassSQL.UpdateParameters["RollNo"].DefaultValue = RollTextBox.Text.Trim();
                StudentClassSQL.Update();

                bool Is_Subject_Checked = true;
                foreach (GridViewRow row in GroupGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        Is_Subject_Checked = false;
                    }
                }

                if (!Is_Subject_Checked)
                {
                    StudentSubjectRecordsSQL.DeleteParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Stu_row.DataItemIndex]["StudentClassID"].ToString();
                    StudentSubjectRecordsSQL.Delete();

                    foreach (GridViewRow row in GroupGridView.Rows)
                    {
                        CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                        RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                        if (SubjectCheckBox.Checked)
                        {
                            Is_Subject_Checked = false;
                            string SubjectID = GroupGridView.DataKeys[row.DataItemIndex].Value.ToString();
                            string Subject_Type = SubjectType.SelectedValue;

                            StudentSubjectRecordsSQL.InsertParameters["SubjectID"].DefaultValue = SubjectID;
                            StudentSubjectRecordsSQL.InsertParameters["SubjectType"].DefaultValue = Subject_Type;
                            StudentSubjectRecordsSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[Stu_row.DataItemIndex]["StudentID"].ToString();
                            StudentSubjectRecordsSQL.InsertParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Stu_row.DataItemIndex]["StudentClassID"].ToString();
                            StudentSubjectRecordsSQL.Insert();
                        }
                    }
                }
            }

            UpdateStudentRecordIDSQL.Update();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully updated!')", true);
        }
    }
}