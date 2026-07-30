using Education;

using System;

using System.Collections.Generic;

using System.Web.UI;

using System.Web.UI.WebControls;



namespace EDUCATION.COM.Employee

{

    public partial class Attendance_Schedule : System.Web.UI.Page

    {

        protected void Page_Load(object sender, EventArgs e)

        {



        }



        protected void SubmitButton_Click(object sender, EventArgs e)

        {

            if (ScheduleDropDownList.SelectedValue == "0")

                return;



            int schoolId = Convert.ToInt32(Session["SchoolID"]);

            int scheduleId = int.Parse(ScheduleDropDownList.SelectedValue);

            var errors = new List<string>();

            int savedCount = 0;



            foreach (GridViewRow row in EmployeeGridView.Rows)

            {

                var empAddCheckBox = row.FindControl("AddCheckBox") as CheckBox;

                var empLateCheckBox = row.FindControl("LateCheckBox") as CheckBox;

                var empAbsCheckBox = row.FindControl("AbsCheckBox") as CheckBox;

                var rfidTextBox = row.FindControl("RFIDTextBox") as TextBox;

                string employeeIdText = EmployeeGridView.DataKeys[row.RowIndex]["EmployeeID"].ToString();

                int employeeId = int.Parse(employeeIdText);

                string employeeName = row.Cells[3].Text.Trim();



                if (empAddCheckBox.Checked)

                {

                    string existingAssignId = GetHiddenAssignId(row, "IsNotAssign");

                    if (string.IsNullOrEmpty(existingAssignId))

                    {

                        ScheduleOverlapInfo overlap = ScheduleOverlapValidator.GetEmployeeOverlap(schoolId, employeeId, scheduleId);

                        if (overlap != null)

                        {

                            errors.Add(employeeName + ": " + overlap.Message);

                            EmployeeSQL.UpdateParameters["EmployeeID"].DefaultValue = employeeIdText;

                            EmployeeSQL.UpdateParameters["RFID"].DefaultValue = rfidTextBox.Text.Trim();

                            EmployeeSQL.Update();

                            continue;

                        }

                    }



                    Schedule_AssignSQL.DeleteParameters["EmployeeID"].DefaultValue = employeeIdText;

                    Schedule_AssignSQL.Delete();

                    Schedule_AssignSQL.InsertParameters["EmployeeID"].DefaultValue = employeeIdText;

                    Schedule_AssignSQL.InsertParameters["Is_Abs_SMS"].DefaultValue = empAbsCheckBox.Checked.ToString();

                    Schedule_AssignSQL.InsertParameters["Is_Late_SMS"].DefaultValue = empLateCheckBox.Checked.ToString();

                    Schedule_AssignSQL.Insert();

                    savedCount++;

                }

                else

                {

                    Schedule_AssignSQL.DeleteParameters["EmployeeID"].DefaultValue = employeeIdText;

                    Schedule_AssignSQL.Delete();

                }



                EmployeeSQL.UpdateParameters["EmployeeID"].DefaultValue = employeeIdText;

                EmployeeSQL.UpdateParameters["RFID"].DefaultValue = rfidTextBox.Text.Trim();

                EmployeeSQL.Update();

            }



            if (savedCount > 0 || errors.Count == 0)

                Device_DataUpdateSQL.Insert();



            EmployeeGridView.DataBind();

            ShowResultMessage(savedCount, errors, "Inputted Successfully!!");

        }



        protected void UnassignButton_Click(object sender, EventArgs e)

        {

            if (ScheduleDropDownList.SelectedValue == "0")

                return;



            int unassignedCount = 0;



            foreach (GridViewRow row in EmployeeGridView.Rows)

            {

                var addCheckBox = row.FindControl("AddCheckBox") as CheckBox;

                if (addCheckBox == null || !addCheckBox.Checked)

                    continue;



                string employeeId = EmployeeGridView.DataKeys[row.RowIndex]["EmployeeID"].ToString();

                Schedule_AssignSQL.DeleteParameters["EmployeeID"].DefaultValue = employeeId;

                Schedule_AssignSQL.Delete();

                unassignedCount++;

            }



            if (unassignedCount > 0)

            {

                Device_DataUpdateSQL.Insert();

                EmployeeGridView.DataBind();



                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "unassignMessage",

                    "alert('" + unassignedCount + " employee(s) unassigned from this schedule.');", true);

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


