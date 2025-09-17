using Education;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Teacher
{
    public partial class StudentAttendance : System.Web.UI.Page
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

        protected void ToggleDropdowns()
        {
            DataView GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }

            AttendenceCheck();
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

            ToggleDropdowns();
        }
        //Group DDL
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToggleDropdowns();
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
            ToggleDropdowns();
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
            ToggleDropdowns();
        }
        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        //submit attendance
        protected void AttendanceButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int totalSmsCount = 0;
            int availableSms = SMS.SMSBalance;
            string mobileNumber;
            string message;

            foreach (GridViewRow Row in StudentsAttendanceGridView.Rows)
            {
                CheckBox SMSCheckBox = Row.FindControl("SMSCheckBox") as CheckBox;
                TextBox ReasonTextBox = (TextBox)Row.FindControl("ReasonTextBox");

                if (SMSCheckBox.Checked)
                {
                    mobileNumber = StudentsAttendanceGridView.DataKeys[Row.RowIndex]["SMSPhoneNo"].ToString();
                    message = ReasonTextBox.Text + " " + Session["School_Name"].ToString();

                    Get_Validation IsValid = SMS.SMS_Validation(mobileNumber, message);

                    if (IsValid.Validation)
                    {
                        totalSmsCount += SMS.SMS_Conut(message);
                    }
                }
            }

            if (availableSms >= totalSmsCount)
            {
                if (SMS.SMS_GetBalance() >= totalSmsCount)
                {
                    foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
                    {
                        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

                        var Attendance_CheckBox = row.FindControl("Attendance_CheckBox") as CheckBox;
                        var Attendance = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                        var ReasonTextBox = (TextBox)row.FindControl("ReasonTextBox");
                        var SMSCheckBox = (CheckBox)row.FindControl("SMSCheckBox");

                        if (Attendance_CheckBox.Checked)
                        {
                            Attendance_RecordSQL.InsertParameters["StudentID"].DefaultValue = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString();
                            Attendance_RecordSQL.InsertParameters["StudentClassID"].DefaultValue = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentClassID"].ToString();
                            Attendance_RecordSQL.InsertParameters["Attendance"].DefaultValue = Attendance.SelectedValue;
                            Attendance_RecordSQL.InsertParameters["AttendanceDate"].DefaultValue = DateTime.Now.ToString("dd MMM yyy");
                            Attendance_RecordSQL.InsertParameters["Reason"].DefaultValue = ReasonTextBox.Text;
                            Attendance_RecordSQL.Insert();

                            #region Send SMS
                            if (SMSCheckBox.Checked)
                            {
                                mobileNumber = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["SMSPhoneNo"].ToString();
                                message = ReasonTextBox.Text + " " + Session["School_Name"].ToString();

                                Get_Validation IsValid = SMS.SMS_Validation(mobileNumber, message);

                                if (IsValid.Validation)
                                {
                                    Guid SMS_Send_ID = SMS.SMS_Send(mobileNumber, message, "Attendance");

                                    if (SMS_Send_ID != Guid.Empty)
                                    {
                                        SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                        SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                        SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                        SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString();
                                        SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";
                                        SMS_OtherInfoSQL.Insert();
                                    }
                                    else
                                    {
                                        ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                                    }
                                }
                                else
                                {
                                    ErrorLabel.Text = IsValid.Message;
                                    row.BackColor = System.Drawing.Color.Red;
                                }
                            }
                            #endregion Send SMS
                        }
                    }

                    AttendenceCheck();

                    SuccessLabel.Text = "Attendance successfully added!";
                }
                else
                {
                    ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                }
            }
            else
            {
                ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + availableSms;
            }
        }

        protected void AttendenceCheck()
        {
            StudentsAttendanceGridView.DataBind();

            foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

                string SchoolID = Session["SchoolID"].ToString();
                string EducationYearID = Session["Edu_Year"].ToString();

                var AttendanceDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var AttendanceRadioButton = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                var ReasonTextBox = (TextBox)row.FindControl("ReasonTextBox");
                var AtDateLabel = (Label)row.FindControl("AtDateLabel");

                SqlCommand AttendanceRecordIDcmd = new SqlCommand("SELECT Attendance from Attendance_Record Where StudentClassID = @StudentClassID and AttendanceDate = @AttendanceDate and SchoolID = @SchoolID and EducationYearID = @EducationYearID", con);
                AttendanceRecordIDcmd.Parameters.AddWithValue("@StudentClassID", StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentClassID"].ToString());
                AttendanceRecordIDcmd.Parameters.AddWithValue("@AttendanceDate", AttendanceDate);
                AttendanceRecordIDcmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                AttendanceRecordIDcmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);

                con.Open();
                object AttendanceCheck = AttendanceRecordIDcmd.ExecuteScalar();
                con.Close();

                if (AttendanceCheck != null)
                {
                    row.CssClass = "active-attendance";
                    AttendanceRadioButton.SelectedValue = AttendanceCheck.ToString();
                }


                SqlCommand Leavecmd = new SqlCommand("SELECT Description from Attendance_Leave where StudentID = @StudentID and StartDate <= @StartDate and EndDate >= @EndDate and SchoolID = @SchoolID and EducationYearID = @EducationYearID", con);
                Leavecmd.Parameters.AddWithValue("@StudentID", StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString());
                Leavecmd.Parameters.AddWithValue("@StartDate", AttendanceDate);
                Leavecmd.Parameters.AddWithValue("@EndDate", AttendanceDate);
                Leavecmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                Leavecmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);

                con.Open();
                object LeaveCheck = Leavecmd.ExecuteScalar();
                con.Close();

                if (LeaveCheck != null)
                {
                    SqlCommand StartDateCmd = new SqlCommand("SELECT StartDate from Attendance_Leave where StudentID = @StudentID and StartDate <= @StartDate and EndDate >= @EndDate and SchoolID = @SchoolID and EducationYearID = @EducationYearID", con);
                    StartDateCmd.Parameters.AddWithValue("@StudentID", StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString());
                    StartDateCmd.Parameters.AddWithValue("@StartDate", AttendanceDate);
                    StartDateCmd.Parameters.AddWithValue("@EndDate", AttendanceDate);
                    StartDateCmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                    StartDateCmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);

                    SqlCommand EndDateCmd = new SqlCommand("SELECT EndDate from Attendance_Leave where StudentID = @StudentID and StartDate <= @StartDate and EndDate >= @EndDate and SchoolID = @SchoolID and EducationYearID = @EducationYearID", con);
                    EndDateCmd.Parameters.AddWithValue("@StudentID", StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString());
                    EndDateCmd.Parameters.AddWithValue("@StartDate", AttendanceDate);
                    EndDateCmd.Parameters.AddWithValue("@EndDate", AttendanceDate);
                    EndDateCmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                    EndDateCmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);

                    con.Open();
                    var SDate = (DateTime)StartDateCmd.ExecuteScalar();
                    var EDate = (DateTime)EndDateCmd.ExecuteScalar();
                    con.Close();

                    AttendanceRadioButton.SelectedIndex = 3;
                    ReasonTextBox.Enabled = true;

                    AtDateLabel.Text = "(From:" + SDate.ToString("d MMM yy") + " To " + EDate.ToString("d MMM yy") + ")";
                    ReasonTextBox.Text = LeaveCheck.ToString();
                }
            }
        }
    }
}