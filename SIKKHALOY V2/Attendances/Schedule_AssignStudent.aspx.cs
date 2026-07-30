using Education;
using System;
using System.Collections.Generic;
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
            if (ScheduleDropDownList.SelectedValue == "0" || ClassDropDownList.SelectedValue == "0")
                return;

            int schoolId = Convert.ToInt32(Session["SchoolID"]);
            int scheduleId = int.Parse(ScheduleDropDownList.SelectedValue);
            var errors = new List<string>();
            int savedCount = 0;

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                var addSchSelectCheckBox = (CheckBox)row.FindControl("AddSch_SelectCheckBox");
                var rfidCodeTextBox = row.FindControl("RFIDTextBox") as TextBox;
                var entrySelectCheckBox = (CheckBox)row.FindControl("Entry_SelectCheckBox");
                var exitSelectCheckBox = (CheckBox)row.FindControl("Exit_SelectCheckBox");
                var absSelectCheckBox = (CheckBox)row.FindControl("Abs_SelectCheckBox");
                var lateSelectCheckBox = (CheckBox)row.FindControl("Late_SelectCheckBox");
                string studentIdText = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                int studentId = int.Parse(studentIdText);
                string studentName = row.Cells[3].Text.Trim();

                if (addSchSelectCheckBox.Checked)
                {
                    string existingAssignId = GetHiddenAssignId(row, "IsNotAssign");
                    if (string.IsNullOrEmpty(existingAssignId))
                    {
                        ScheduleOverlapInfo overlap = ScheduleOverlapValidator.GetStudentOverlap(schoolId, studentId, scheduleId);
                        if (overlap != null)
                        {
                            errors.Add(studentName + ": " + overlap.Message);
                            ScheduleAssignSQL.UpdateParameters["StudentID"].DefaultValue = studentIdText;
                            ScheduleAssignSQL.UpdateParameters["RFID"].DefaultValue = rfidCodeTextBox.Text;
                            ScheduleAssignSQL.Update();
                            continue;
                        }
                    }

                    ScheduleAssignSQL.DeleteParameters["StudentID"].DefaultValue = studentIdText;
                    ScheduleAssignSQL.Delete();
                    ScheduleAssignSQL.InsertParameters["StudentID"].DefaultValue = studentIdText;
                    ScheduleAssignSQL.InsertParameters["Entry_Confirmation"].DefaultValue = entrySelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.InsertParameters["Exit_Confirmation"].DefaultValue = exitSelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.InsertParameters["Is_Abs_SMS"].DefaultValue = absSelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.InsertParameters["Is_Late_SMS"].DefaultValue = lateSelectCheckBox.Checked.ToString();
                    ScheduleAssignSQL.Insert();
                    savedCount++;
                }
                else
                {
                    ScheduleAssignSQL.DeleteParameters["StudentID"].DefaultValue = studentIdText;
                    ScheduleAssignSQL.Delete();
                }

                ScheduleAssignSQL.UpdateParameters["StudentID"].DefaultValue = studentIdText;
                ScheduleAssignSQL.UpdateParameters["RFID"].DefaultValue = rfidCodeTextBox.Text;
                ScheduleAssignSQL.Update();

                ConfSMS_UpdateSQL.UpdateParameters["StudentID"].DefaultValue = studentIdText;
                ConfSMS_UpdateSQL.UpdateParameters["Entry_Confirmation"].DefaultValue = entrySelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Exit_Confirmation"].DefaultValue = exitSelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Is_Abs_SMS"].DefaultValue = absSelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.UpdateParameters["Is_Late_SMS"].DefaultValue = lateSelectCheckBox.Checked.ToString();
                ConfSMS_UpdateSQL.Update();
            }

            if (savedCount > 0 || errors.Count == 0)
                Device_DataUpdateSQL.Insert();

            StudentsGridView.DataBind();
            ShowResultMessage(savedCount, errors, "Assign Successfully.");
        }

        protected void UnassignButton_Click(object sender, EventArgs e)
        {
            if (ScheduleDropDownList.SelectedValue == "0" || ClassDropDownList.SelectedValue == "0")
                return;

            int unassignedCount = 0;

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                var addSchSelectCheckBox = (CheckBox)row.FindControl("AddSch_SelectCheckBox");
                if (addSchSelectCheckBox == null || !addSchSelectCheckBox.Checked)
                    continue;

                string studentId = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                ScheduleAssignSQL.DeleteParameters["StudentID"].DefaultValue = studentId;
                ScheduleAssignSQL.Delete();
                unassignedCount++;
            }

            if (unassignedCount > 0)
            {
                Device_DataUpdateSQL.Insert();
                StudentsGridView.DataBind();

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "unassignMessage",
                    "alert('" + unassignedCount + " student(s) unassigned from this schedule.');", true);
            }
        }

        private static string GetHiddenAssignId(GridViewRow row, string cssClass)
        {
            foreach (Control control in row.Controls)
            {
                string value = FindHiddenAssignValue(control, cssClass);
                if (value != null)
                    return value;
            }

            return string.Empty;
        }

        private static string FindHiddenAssignValue(Control root, string cssClass)
        {
            if (root is System.Web.UI.HtmlControls.HtmlInputHidden hidden
                && hidden.Attributes["class"] == cssClass)
            {
                return hidden.Value ?? string.Empty;
            }

            foreach (Control child in root.Controls)
            {
                string value = FindHiddenAssignValue(child, cssClass);
                if (value != null)
                    return value;
            }

            return null;
        }

        private void ShowResultMessage(int savedCount, List<string> errors, string successMessage)
        {
            if (errors.Count > 0)
            {
                string message = string.Join("\\n", errors);
                if (savedCount > 0)
                    message = successMessage + "\\n\\nSome records were skipped:\\n" + message;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "assignErrors",
                    "alert('" + message.Replace("'", "\\'") + "');", true);
                return;
            }

            if (savedCount > 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage",
                    "alert('" + successMessage + "');", true);
            }
        }
    }
}