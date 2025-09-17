using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Attendances
{
    public partial class Schedule_AssignStudent : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }
            }

            catch { }
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

            string name = "";

            name += " For Class: " + ClassDropDownList.SelectedItem.Text;

            if (SectionDropDownList.SelectedIndex != 0)
            {
                name += ", Section: " + SectionDropDownList.SelectedItem.Text;
            }
            if (GroupDropDownList.SelectedIndex != 0)
            {
                name += ", Group: " + GroupDropDownList.SelectedItem.Text;
            }
            if (ShiftDropDownList.SelectedIndex != 0)
            {
                name += ", Shift: " + ShiftDropDownList.SelectedItem.Text;
            }
            CGSSLabel.Text = name;
        }
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
        //End DDL

        protected void AssignButton_Click(object sender, EventArgs e)
        {
            bool IsAssign = false;
            foreach (GridViewRow ROW in StudentsGridView.Rows)
            {
                var AddSch_SelectCheckBox = (CheckBox)ROW.FindControl("AddSch_SelectCheckBox");
                var RFIDCodeTextBox = StudentsGridView.Rows[ROW.RowIndex].FindControl("RFIDTextBox") as TextBox;

                var Entry_SelectCheckBox = (CheckBox)ROW.FindControl("Entry_SelectCheckBox");
                var Exit_SelectCheckBox = (CheckBox)ROW.FindControl("Exit_SelectCheckBox");

                var Abs_SelectCheckBox = (CheckBox)ROW.FindControl("Abs_SelectCheckBox");
                var Late_SelectCheckBox = (CheckBox)ROW.FindControl("Late_SelectCheckBox");


                ScheduleAssignSQL.DeleteParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[ROW.DataItemIndex]["StudentID"].ToString();
                ScheduleAssignSQL.Delete();

                if (AddSch_SelectCheckBox.Checked)
                {
                    ScheduleAssignSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[ROW.DataItemIndex]["StudentID"].ToString();
                    ScheduleAssignSQL.InsertParameters["Entry_Confirmation"].DefaultValue = Entry_SelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.InsertParameters["Exit_Confirmation"].DefaultValue = Exit_SelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.InsertParameters["Is_Abs_SMS"].DefaultValue = Abs_SelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.InsertParameters["Is_Late_SMS"].DefaultValue = Late_SelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.Insert();
                    IsAssign = true;
                }


                ScheduleAssignSQL.UpdateParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[ROW.DataItemIndex]["StudentID"].ToString();
                ScheduleAssignSQL.UpdateParameters["RFID"].DefaultValue = RFIDCodeTextBox.Text;
                ScheduleAssignSQL.Update();

                ConfSMS_UpdateSQL.UpdateParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[ROW.DataItemIndex]["StudentID"].ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Entry_Confirmation"].DefaultValue = Entry_SelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Exit_Confirmation"].DefaultValue = Exit_SelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Is_Abs_SMS"].DefaultValue = Abs_SelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Is_Late_SMS"].DefaultValue = Late_SelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.Update();

                IsAssign = true;
            }

            if (IsAssign)
            {
                Device_DataUpdateSQL.Insert();
                StudentsGridView.DataBind();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Assign Successfully.')", true);
            }
        }
    }
}