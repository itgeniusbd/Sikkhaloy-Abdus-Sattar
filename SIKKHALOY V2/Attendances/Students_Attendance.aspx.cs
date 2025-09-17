using Education;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Students_Attendance : System.Web.UI.Page
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

        protected void FindButton_Click(object sender, EventArgs e)
        {
            AttendenceCheck();

            if (AttendanceDateTextBox.Text.Trim() != "")
            {
                StudentsAttendanceGridView.Visible = true;
                SMSRadioButtonList.SelectedIndex = 0;
            }
            else
                StudentsAttendanceGridView.Visible = false;
        }
        protected void AttendanceButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Msg = "";

            int SMSBalance = SMS.SMSBalance;

            foreach (GridViewRow Row in StudentsAttendanceGridView.Rows)
            {
                CheckBox SMSCheckBox = Row.FindControl("SMSCheckBox") as CheckBox;
                TextBox ReasonTextBox = (TextBox)Row.FindControl("ReasonTextBox");

                if (SMSCheckBox.Checked)
                {
                    PhoneNo = StudentsAttendanceGridView.DataKeys[Row.RowIndex]["SMSPhoneNo"].ToString();
                    Msg = ReasonTextBox.Text + " " + Session["School_Name"].ToString();

                    Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                    if (IsValid.Validation)
                    {
                        TotalSMS += SMS.SMS_Conut(Msg);
                    }
                }
            }


            if (SMSBalance >= TotalSMS)
            {
                if (SMS.SMS_GetBalance() >= TotalSMS)
                {
                    foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
                    {
                        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

                        RadioButtonList Attendance = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                        TextBox ReasonTextBox = (TextBox)row.FindControl("ReasonTextBox");
                        CheckBox SMSCheckBox = (CheckBox)row.FindControl("SMSCheckBox");
                        CheckBox Attendance_CheckBox = row.FindControl("Attendance_CheckBox") as CheckBox;

                        if (Attendance_CheckBox.Checked)
                        {
                            Attendance_RecordSQL.InsertParameters["StudentID"].DefaultValue = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString();
                            Attendance_RecordSQL.InsertParameters["StudentClassID"].DefaultValue = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["StudentClassID"].ToString();
                            Attendance_RecordSQL.InsertParameters["Attendance"].DefaultValue = Attendance.SelectedValue;
                            Attendance_RecordSQL.InsertParameters["AttendanceDate"].DefaultValue = AttendanceDateTextBox.Text.Trim();
                            Attendance_RecordSQL.InsertParameters["Reason"].DefaultValue = ReasonTextBox.Text;
                            Attendance_RecordSQL.Insert();

                            #region Send SMS
                            if (SMSCheckBox.Checked)
                            {
                                PhoneNo = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["SMSPhoneNo"].ToString();
                                
                                string studentID = StudentsAttendanceGridView.DataKeys[row.DataItemIndex]["ID"].ToString();



                                Msg = ReasonTextBox.Text + " " + Session["School_Name"].ToString();

                                Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                                if (IsValid.Validation)
                                {
                                    Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Attendance");

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
                }
                else
                {
                    ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                }
            }
            else
            {
                ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + SMSBalance;
            }

        }
        protected void AttendenceCheck()
        {
            if (AttendanceDateTextBox.Text.Trim() != "")
            {
                StudentsAttendanceGridView.DataBind();

                foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
                {
                    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                    string SchoolID = Session["SchoolID"].ToString();
                    string EducationYearID = Session["Edu_Year"].ToString();
                    DateTime AttendanceDate = Convert.ToDateTime(AttendanceDateTextBox.Text.Trim());

                    RadioButtonList Attendance = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                    TextBox ReasonTextBox = (TextBox)row.FindControl("ReasonTextBox");
                    Label AtDateLabel = (Label)row.FindControl("AtDateLabel");

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
                        row.CssClass = "Diable_Rows";
                        Attendance.SelectedValue = AttendanceCheck.ToString();
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
                        DateTime SDate = (DateTime)StartDateCmd.ExecuteScalar();
                        DateTime EDate = (DateTime)EndDateCmd.ExecuteScalar();
                        con.Close();

                        Attendance.SelectedIndex = 3;
                        ReasonTextBox.Enabled = true;
                        ReasonTextBox.CssClass = "Etextbox";

                        AtDateLabel.Text = "(From:" + SDate.ToString("d MMM yy") + " To " + EDate.ToString("d MMM yy") + ")";
                        ReasonTextBox.Text = LeaveCheck.ToString();
                    }
                }
            }
        }
    }
}