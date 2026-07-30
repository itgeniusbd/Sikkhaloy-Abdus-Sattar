using Education;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ATTENDANCES
{
    public partial class Students_Attendance : System.Web.UI.Page
    {
        private System.Collections.Generic.Dictionary<string, DataRow> _existingAttendance;
        private DateTime _attendanceCheckDate;
        private int _attendanceCheckScheduleId;
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

                if (Session["AttendanceSchedule"] != null)
                    ScheduleDropDownList.SelectedValue = Session["AttendanceSchedule"].ToString();

                if (Session["ManualAttendanceDate"] != null)
                    AttendanceDateTextBox.Text = Session["ManualAttendanceDate"].ToString();
            }
            else
            {
                if (Session["ManualAttendanceGridVisible"] as bool? == true)
                    StudentsAttendanceGridView.Visible = true;

                if (ScheduleDropDownList.SelectedValue != "0")
                    Session["AttendanceSchedule"] = ScheduleDropDownList.SelectedValue;
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

            RefreshAttendanceGridIfReady();
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
            ErrorLabel.Text = "";

            if (ClassDropDownList.SelectedValue == "0")
            {
                ErrorLabel.Text = "Select class.";
                StudentsAttendanceGridView.Visible = false;
                Session["ManualAttendanceGridVisible"] = false;
                return;
            }

            if (ScheduleDropDownList.SelectedValue == "0")
            {
                ErrorLabel.Text = "Select schedule.";
                StudentsAttendanceGridView.Visible = false;
                Session["ManualAttendanceGridVisible"] = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(AttendanceDateTextBox.Text.Trim()))
            {
                ErrorLabel.Text = "Select attendance date.";
                StudentsAttendanceGridView.Visible = false;
                Session["ManualAttendanceGridVisible"] = false;
                return;
            }

            if (!TryParseAttendanceDate(out _))
            {
                ErrorLabel.Text = "Invalid attendance date.";
                StudentsAttendanceGridView.Visible = false;
                Session["ManualAttendanceGridVisible"] = false;
                return;
            }

            Session["AttendanceSchedule"] = ScheduleDropDownList.SelectedValue;
            Session["ManualAttendanceDate"] = AttendanceDateTextBox.Text.Trim();
            Session["ManualAttendanceGridVisible"] = true;

            StudentsAttendanceGridView.Visible = true;
            SMSRadioButtonList.SelectedIndex = 0;
            AttendenceCheck();
        }
        protected void AttendanceButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            if (!TryParseAttendanceDate(out DateTime attendanceDate))
            {
                ErrorLabel.Text = "Invalid attendance date.";
                return;
            }

            int scheduleId = int.Parse(ScheduleDropDownList.SelectedValue);
            Session["AttendanceSchedule"] = scheduleId.ToString();
            int savedCount = 0;
            string saveError = null;

            foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
            {
                CheckBox attendanceCheckBox = row.FindControl("Attendance_CheckBox") as CheckBox;
                if (attendanceCheckBox == null || !attendanceCheckBox.Checked)
                    continue;

                RadioButtonList attendance = (RadioButtonList)row.FindControl("AttendenceRadioButtonList");
                TextBox reasonTextBox = (TextBox)row.FindControl("ReasonTextBox");

                string studentId = StudentsAttendanceGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                string studentClassId = StudentsAttendanceGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();

                if (TrySaveAttendanceRecord(studentClassId, studentId, scheduleId, attendance.SelectedValue, attendanceDate, reasonTextBox.Text, out saveError))
                    savedCount++;
                else
                {
                    if (!string.IsNullOrEmpty(saveError))
                        ErrorLabel.Text = saveError;
                    break;
                }
            }

            if (savedCount > 0)
            {
                int smsSent = SendAttendanceSms();
                Session["ManualAttendanceDate"] = AttendanceDateTextBox.Text.Trim();
                Session["ManualAttendanceGridVisible"] = true;
                StudentsAttendanceGridView.Visible = true;
                AttendenceCheck();

                string alertMsg = "হাজিরা সফলভাবে সংরক্ষণ হয়েছে। (" + savedCount + " জন শিক্ষার্থী)";
                if (smsSent > 0)
                    alertMsg += "\\nSMS পাঠানো হয়েছে: " + smsSent + " টি।";
                else if (!string.IsNullOrEmpty(ErrorLabel.Text))
                    alertMsg += "\\nSMS: " + ErrorLabel.Text.Replace("'", "\\'");

                ScriptManager.RegisterClientScriptBlock(this, GetType(), "attendanceSaved",
                    "alert('" + alertMsg + "');", true);
            }
            else if (string.IsNullOrEmpty(ErrorLabel.Text))
            {
                ErrorLabel.Text = saveError ?? "কোনো শিক্ষার্থী নির্বাচন করা হয়নি বা সংরক্ষণ ব্যর্থ হয়েছে।";
            }
        }

        private int SendAttendanceSms()
        {
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());
            int totalSMS = 0;
            int smsSent = 0;

            foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
            {
                CheckBox attendanceCheckBox = row.FindControl("Attendance_CheckBox") as CheckBox;
                CheckBox smsCheckBox = row.FindControl("SMSCheckBox") as CheckBox;
                TextBox reasonTextBox = (TextBox)row.FindControl("ReasonTextBox");

                if (attendanceCheckBox == null || !attendanceCheckBox.Checked || smsCheckBox == null || !smsCheckBox.Checked)
                    continue;

                string phoneNo = StudentsAttendanceGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();
                string msg = reasonTextBox.Text + " " + Session["School_Name"].ToString();
                Get_Validation isValid = SMS.SMS_Validation(phoneNo, msg);

                if (isValid.Validation)
                    totalSMS += SMS.SMS_Conut(msg);
            }

            if (totalSMS == 0)
                return 0;

            int smsBalance = SMS.SMSBalance;
            if (smsBalance < totalSMS)
            {
                ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + smsBalance;
                return 0;
            }

            try
            {
                if (SMS.SMS_GetBalance() < totalSMS)
                {
                    ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                    return 0;
                }
            }
            catch
            {
                ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                return 0;
            }

            foreach (GridViewRow row in StudentsAttendanceGridView.Rows)
            {
                CheckBox attendanceCheckBox = row.FindControl("Attendance_CheckBox") as CheckBox;
                CheckBox smsCheckBox = row.FindControl("SMSCheckBox") as CheckBox;
                TextBox reasonTextBox = (TextBox)row.FindControl("ReasonTextBox");

                if (attendanceCheckBox == null || !attendanceCheckBox.Checked || smsCheckBox == null || !smsCheckBox.Checked)
                    continue;

                string phoneNo = StudentsAttendanceGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();
                string msg = reasonTextBox.Text + " " + Session["School_Name"].ToString();
                Get_Validation isValid = SMS.SMS_Validation(phoneNo, msg);

                if (!isValid.Validation)
                {
                    ErrorLabel.Text = isValid.Message;
                    row.BackColor = System.Drawing.Color.Red;
                    continue;
                }

                Guid smsSendId = SMS.SMS_Send(phoneNo, msg, "Attendance");
                if (smsSendId == Guid.Empty)
                {
                    ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                    continue;
                }

                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = StudentsAttendanceGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";
                SMS_OtherInfoSQL.Insert();
                smsSent++;
            }

            return smsSent;
        }

        private bool TryParseAttendanceDate(out DateTime attendanceDate)
        {
            attendanceDate = DateTime.MinValue;
            string text = AttendanceDateTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return false;

            if (DateTime.TryParseExact(text, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out attendanceDate))
                return true;

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out attendanceDate))
                return true;

            return DateTime.TryParse(text, out attendanceDate);
        }

        private bool TrySaveAttendanceRecord(string studentClassId, string studentId, int scheduleId, string attendance, DateTime attendanceDate, string reason, out string errorMessage)
        {
            errorMessage = null;
            const string upsertSql = @"
IF EXISTS (
    SELECT 1 FROM Attendance_Record
    WHERE StudentClassID = @StudentClassID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID
      AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID
)
BEGIN
    UPDATE Attendance_Record
    SET Attendance = @Attendance, Reason = @Reason, RegistrationID = @RegistrationID
    WHERE StudentClassID = @StudentClassID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID
      AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID
END
ELSE IF EXISTS (
    SELECT 1 FROM Attendance_Record
    WHERE StudentClassID = @StudentClassID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID
      AND Attendance_ScheduleID IS NULL
)
BEGIN
    UPDATE Attendance_Record
    SET Attendance = @Attendance, Reason = @Reason, Attendance_ScheduleID = @ScheduleID, RegistrationID = @RegistrationID
    WHERE StudentClassID = @StudentClassID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID
      AND Attendance_ScheduleID IS NULL
END
ELSE
BEGIN
    INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, Attendance_ScheduleID, Attendance, AttendanceDate, Reason)
    VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @ScheduleID, @Attendance, @AttendanceDate, @Reason)
END";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                {
                    con.Open();
                    using (SqlCommand setCmd = new SqlCommand("SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;", con))
                        setCmd.ExecuteNonQuery();

                    using (SqlCommand cmd = new SqlCommand(upsertSql, con))
                    {
                        cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = Convert.ToInt32(Session["SchoolID"]);
                        cmd.Parameters.Add("@RegistrationID", SqlDbType.Int).Value = Convert.ToInt32(Session["RegistrationID"]);
                        cmd.Parameters.Add("@EducationYearID", SqlDbType.Int).Value = Convert.ToInt32(Session["Edu_Year"]);
                        cmd.Parameters.Add("@ClassID", SqlDbType.Int).Value = Convert.ToInt32(ClassDropDownList.SelectedValue);
                        cmd.Parameters.Add("@StudentID", SqlDbType.Int).Value = Convert.ToInt32(studentId);
                        cmd.Parameters.Add("@StudentClassID", SqlDbType.Int).Value = Convert.ToInt32(studentClassId);
                        cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;
                        cmd.Parameters.Add("@Attendance", SqlDbType.NVarChar, 10).Value = attendance;
                        cmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate.Date;
                        cmd.Parameters.Add("@Reason", SqlDbType.NVarChar).Value = (object)reason ?? DBNull.Value;

                        cmd.ExecuteNonQuery();

                        using (SqlCommand verifyCmd = new SqlCommand(@"SELECT COUNT(1) FROM Attendance_Record
WHERE StudentClassID = @StudentClassID
  AND CAST(AttendanceDate AS DATE) = @AttendanceDate
  AND SchoolID = @SchoolID
  AND EducationYearID = @EducationYearID
  AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID", con))
                        {
                            verifyCmd.Parameters.Add("@StudentClassID", SqlDbType.Int).Value = Convert.ToInt32(studentClassId);
                            verifyCmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate.Date;
                            verifyCmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = Convert.ToInt32(Session["SchoolID"]);
                            verifyCmd.Parameters.Add("@EducationYearID", SqlDbType.Int).Value = Convert.ToInt32(Session["Edu_Year"]);
                            verifyCmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;

                            if (Convert.ToInt32(verifyCmd.ExecuteScalar()) == 0)
                            {
                                errorMessage = "হাজিরা সংরক্ষণ হয়নি। Database/Scripts/Attendance_MultiSchedule_Trigger_Fix.sql স্ক্রিপ্ট চালান।";
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Message.IndexOf("Attendance_ScheduleID", StringComparison.OrdinalIgnoreCase) >= 0)
                    errorMessage = "ডাটাবেসে Attendance_ScheduleID কলাম নেই। Database/Scripts/Attendance_MultiSchedule_Step1_Column.sql স্ক্রিপ্ট চালান।";
                else
                    errorMessage = "হাজিরা সংরক্ষণ ব্যর্থ: " + ex.Message;

                return false;
            }
        }

        protected void StudentsAttendanceGridView_DataBinding(object sender, EventArgs e)
        {
            if (!StudentsAttendanceGridView.Visible)
            {
                _existingAttendance = null;
                return;
            }

            PrepareAttendanceMarks();
        }

        private void PrepareAttendanceMarks()
        {
            _existingAttendance = new System.Collections.Generic.Dictionary<string, DataRow>();

            if (!TryParseAttendanceDate(out DateTime attendanceDate))
                return;

            if (ScheduleDropDownList.SelectedValue == "0")
                return;

            _attendanceCheckDate = attendanceDate.Date;
            _attendanceCheckScheduleId = int.Parse(ScheduleDropDownList.SelectedValue);
            _existingAttendance = LoadExistingAttendance(_attendanceCheckDate, _attendanceCheckScheduleId);
        }

        protected void ScheduleDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["AttendanceSchedule"] = ScheduleDropDownList.SelectedValue;
            RefreshAttendanceGridIfReady();
        }

        private void RefreshAttendanceGridIfReady()
        {
            if (ScheduleDropDownList.SelectedValue == "0"
                || string.IsNullOrWhiteSpace(AttendanceDateTextBox.Text)
                || !TryParseAttendanceDate(out _))
                return;

            Session["ManualAttendanceDate"] = AttendanceDateTextBox.Text.Trim();
            Session["ManualAttendanceGridVisible"] = true;
            StudentsAttendanceGridView.Visible = true;
            AttendenceCheck();
        }

        protected void AttendenceCheck()
        {
            if (ScheduleDropDownList.SelectedValue == "0"
                || string.IsNullOrWhiteSpace(AttendanceDateTextBox.Text)
                || !TryParseAttendanceDate(out _))
            {
                _existingAttendance = null;
                return;
            }

            PrepareAttendanceMarks();
            StudentsAttendanceGridView.DataBind();
        }

        protected void StudentsAttendanceGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow || _existingAttendance == null)
                return;

            RadioButtonList attendance = (RadioButtonList)e.Row.FindControl("AttendenceRadioButtonList");
            TextBox reasonTextBox = (TextBox)e.Row.FindControl("ReasonTextBox");
            Label atDateLabel = (Label)e.Row.FindControl("AtDateLabel");
            Label updatedByLabel = (Label)e.Row.FindControl("UpdatedByLabel");
            CheckBox attendanceCheckBox = (CheckBox)e.Row.FindControl("Attendance_CheckBox");
            if (attendance == null)
                return;

            string studentClassId = StudentsAttendanceGridView.DataKeys[e.Row.RowIndex]["StudentClassID"].ToString();
            string studentId = StudentsAttendanceGridView.DataKeys[e.Row.RowIndex]["StudentID"].ToString();
            bool hasAttendanceRecord = false;

            if (_existingAttendance.TryGetValue(studentClassId, out DataRow attendanceRow))
            {
                hasAttendanceRecord = true;
                e.Row.CssClass = "Diable_Rows";
                ApplyAttendanceSelection(attendance, attendanceRow["Attendance"].ToString().Trim());

                if (attendanceCheckBox != null)
                    attendanceCheckBox.Checked = true;

                if (attendanceRow["Reason"] != DBNull.Value && !string.IsNullOrWhiteSpace(attendanceRow["Reason"].ToString()))
                {
                    reasonTextBox.Text = attendanceRow["Reason"].ToString();
                    reasonTextBox.Enabled = true;
                    reasonTextBox.CssClass = "Etextbox";
                }

                if (updatedByLabel != null && attendanceRow["UpdatedByName"] != DBNull.Value)
                {
                    string updatedByName = attendanceRow["UpdatedByName"].ToString().Trim();
                    if (!string.IsNullOrEmpty(updatedByName))
                        updatedByLabel.Text = updatedByName;
                }
            }

            if (!hasAttendanceRecord)
                ApplyLeaveStatus(studentId, attendance, reasonTextBox, atDateLabel);
        }

        private static void ApplyAttendanceSelection(RadioButtonList attendance, string attendanceValue)
        {
            if (string.IsNullOrWhiteSpace(attendanceValue))
                return;

            ListItem attendanceItem = attendance.Items.FindByValue(attendanceValue);
            if (attendanceItem == null)
                attendanceItem = attendance.Items.FindByText(attendanceValue);
            if (attendanceItem == null)
            {
                string[] values = { "Pre", "Abs", "Late", "Leave", "Bunk" };
                int index = Array.IndexOf(values, attendanceValue);
                if (index >= 0 && index < attendance.Items.Count)
                    attendanceItem = attendance.Items[index];
            }
            if (attendanceItem == null)
                return;

            attendance.ClearSelection();
            attendanceItem.Selected = true;
        }

        private void ApplyLeaveStatus(string studentId, RadioButtonList attendance, TextBox reasonTextBox, Label atDateLabel)
        {
            string schoolId = Session["SchoolID"].ToString();
            string educationYearId = Session["Edu_Year"].ToString();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            using (SqlCommand leaveCmd = new SqlCommand("SELECT Description, StartDate, EndDate FROM Attendance_Leave WHERE StudentID = @StudentID AND StartDate <= @AttendanceDate AND EndDate >= @AttendanceDate AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID", con))
            {
                leaveCmd.Parameters.AddWithValue("@StudentID", studentId);
                leaveCmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = _attendanceCheckDate;
                leaveCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                leaveCmd.Parameters.AddWithValue("@EducationYearID", educationYearId);

                con.Open();
                using (SqlDataReader reader = leaveCmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return;

                    attendance.ClearSelection();
                    attendance.Items[3].Selected = true;
                    reasonTextBox.Enabled = true;
                    reasonTextBox.CssClass = "Etextbox";
                    atDateLabel.Text = "(From:" + ((DateTime)reader["StartDate"]).ToString("d MMM yy") + " To " + ((DateTime)reader["EndDate"]).ToString("d MMM yy") + ")";
                    reasonTextBox.Text = reader["Description"].ToString();
                }
            }
        }

        private System.Collections.Generic.Dictionary<string, DataRow> LoadExistingAttendance(DateTime attendanceDate, int scheduleId)
        {
            var result = new System.Collections.Generic.Dictionary<string, DataRow>();

            const string sql = @"SELECT ar.StudentClassID, ar.Attendance, ar.Reason,
    COALESCE(
        NULLIF(LTRIM(RTRIM(ISNULL(a.FirstName, '') + ' ' + ISNULL(a.LastName, ''))), ''),
        NULLIF(LTRIM(RTRIM(ISNULL(t.FirstName, '') + ' ' + ISNULL(t.LastName, ''))), ''),
        r.UserName
    ) AS UpdatedByName
FROM Attendance_Record ar
LEFT JOIN Registration r ON ar.RegistrationID = r.RegistrationID AND ar.RegistrationID > 0
LEFT JOIN Admin a ON ar.RegistrationID = a.RegistrationID AND ar.SchoolID = a.SchoolID
LEFT JOIN Teacher t ON ar.RegistrationID = t.TeacherRegistrationID AND ar.SchoolID = t.SchoolID
WHERE CAST(ar.AttendanceDate AS DATE) = @AttendanceDate
  AND ar.SchoolID = @SchoolID
  AND ar.EducationYearID = @EducationYearID
  AND ar.ClassID = @ClassID
  AND (
        ISNULL(ar.Attendance_ScheduleID, 0) = @ScheduleID
        OR (
            ar.Attendance_ScheduleID IS NULL
            AND NOT EXISTS (
                SELECT 1
                FROM Attendance_Record scheduled
                WHERE scheduled.StudentClassID = ar.StudentClassID
                  AND CAST(scheduled.AttendanceDate AS DATE) = @AttendanceDate
                  AND scheduled.SchoolID = @SchoolID
                  AND scheduled.EducationYearID = @EducationYearID
                  AND ISNULL(scheduled.Attendance_ScheduleID, 0) = @ScheduleID
            )
        )
      )";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate.Date;
                cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = Convert.ToInt32(Session["SchoolID"]);
                cmd.Parameters.Add("@EducationYearID", SqlDbType.Int).Value = Convert.ToInt32(Session["Edu_Year"]);
                cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;
                cmd.Parameters.Add("@ClassID", SqlDbType.Int).Value = Convert.ToInt32(ClassDropDownList.SelectedValue);

                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                    result[row["StudentClassID"].ToString()] = row;
            }

            return result;
        }

        private System.Web.UI.Control FindControlRecursive(System.Web.UI.Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (System.Web.UI.Control c in root.Controls)
            {
                var t = FindControlRecursive(c, id);
                if (t != null) return t;
            }

            return null;
        }
    }
}