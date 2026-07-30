using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Employee_Attendance : System.Web.UI.Page
    {
        private const string GridShownSessionKey = "EmployeeAttendanceGridShown";
        private const string AttendanceDateSessionKey = "EmployeeAttendanceDate";
        private Dictionary<string, DataRow> _existingAttendance;
        private DateTime _attendanceCheckDate;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["EmployeeAttendanceSchedule"] != null)
                ScheduleDropDownList.SelectedValue = Session["EmployeeAttendanceSchedule"].ToString();
            else if (IsPostBack && ScheduleDropDownList.SelectedValue != "0")
                Session["EmployeeAttendanceSchedule"] = ScheduleDropDownList.SelectedValue;

            if (string.IsNullOrWhiteSpace(AttendanceDateTextBox.Text) && Session[AttendanceDateSessionKey] != null)
                AttendanceDateTextBox.Text = Session[AttendanceDateSessionKey].ToString();

            if (ShouldShowEmployeeGrid())
                EmployeeGridView.Visible = true;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (ShouldShowEmployeeGrid())
                AttendenceCheck();
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            if (ScheduleDropDownList.SelectedValue == "0")
            {
                EmployeeGridView.Visible = false;
                Session[GridShownSessionKey] = false;
                return;
            }

            Session["EmployeeAttendanceSchedule"] = ScheduleDropDownList.SelectedValue;

            if (string.IsNullOrWhiteSpace(AttendanceDateTextBox.Text))
            {
                EmployeeGridView.Visible = false;
                Session[GridShownSessionKey] = false;
                return;
            }

            Session[AttendanceDateSessionKey] = AttendanceDateTextBox.Text.Trim();
            Session[GridShownSessionKey] = true;
            EmployeeGridView.Visible = true;
            AttendenceCheck();
        }

        protected void ScheduleDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["EmployeeAttendanceSchedule"] = ScheduleDropDownList.SelectedValue;

            if (ShouldShowEmployeeGrid())
                EmployeeGridView.Visible = true;
        }

        protected void AttendanceButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            if (!TryParseAttendanceDate(out DateTime attendanceDate))
            {
                ErrorLabel.Text = "Invalid attendance date.";
                return;
            }

            if (ScheduleDropDownList.SelectedValue == "0")
            {
                ErrorLabel.Text = "Select a schedule.";
                return;
            }

            if (!ShouldShowEmployeeGrid())
            {
                ErrorLabel.Text = "প্রথমে Find বাটনে ক্লিক করে কর্মী তালিকা লোড করুন।";
                return;
            }

            int scheduleId = int.Parse(ScheduleDropDownList.SelectedValue);
            Session["EmployeeAttendanceSchedule"] = scheduleId.ToString();
            Session[AttendanceDateSessionKey] = AttendanceDateTextBox.Text.Trim();

            var postedRows = BuildPostedAttendanceRows(scheduleId);
            if (postedRows.Count == 0)
            {
                ErrorLabel.Text = "কোনো কর্মী পাওয়া যায়নি। Find চাপুন এবং শিডিউলে কর্মী assign আছে কিনা দেখুন।";
                return;
            }

            int savedCount = 0;
            foreach (PostedEmployeeAttendance row in postedRows)
            {
                if (TrySaveAttendanceRecord(row.EmployeeId.ToString(CultureInfo.InvariantCulture), scheduleId, row.AttendanceStatus, attendanceDate, row.EntryTime, row.ExitTime, out string saveError))
                    savedCount++;
                else
                {
                    ErrorLabel.Text = saveError;
                    break;
                }
            }

            if (savedCount > 0)
            {
                AttendenceCheck();

                string scheduleName = ScheduleDropDownList.SelectedItem.Text;
                ScriptManager.RegisterStartupScript(this, GetType(), "alertMessage",
                    "alert('হাজিরা সফলভাবে সংরক্ষণ হয়েছে।\\nশিডিউল: " + scheduleName.Replace("'", "\\'") + "\\nমোট: " + savedCount + " জন');", true);
            }
            else if (string.IsNullOrEmpty(ErrorLabel.Text))
            {
                ErrorLabel.Text = "হাজিরা সংরক্ষণ ব্যর্থ হয়েছে।";
            }
        }

        private bool IsSubmitPostBack()
        {
            return Request.Form[AttendanceButton.UniqueID] != null;
        }

        private bool ShouldShowEmployeeGrid()
        {
            return Session[GridShownSessionKey] as bool? == true
                && !string.IsNullOrWhiteSpace(GetAttendanceDateText())
                && ScheduleDropDownList.SelectedValue != "0";
        }

        private string GetAttendanceDateText()
        {
            if (!string.IsNullOrWhiteSpace(AttendanceDateTextBox.Text))
                return AttendanceDateTextBox.Text.Trim();

            return Session[AttendanceDateSessionKey] as string ?? string.Empty;
        }

        private bool TryParseAttendanceDate(out DateTime attendanceDate)
        {
            attendanceDate = DateTime.MinValue;
            string text = GetAttendanceDateText();
            if (string.IsNullOrEmpty(text))
                return false;

            string[] formats =
            {
                "dd MMM yyyy",
                "dd M yyyy",
                "d MMM yyyy",
                "d M yyyy"
            };

            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out attendanceDate))
                return true;

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out attendanceDate))
                return true;

            return DateTime.TryParse(text, out attendanceDate);
        }

        private List<PostedEmployeeAttendance> BuildPostedAttendanceRows(int scheduleId)
        {
            var rows = new List<PostedEmployeeAttendance>();

            if (EmployeeGridView.Rows.Count > 0)
            {
                foreach (GridViewRow row in EmployeeGridView.Rows)
                {
                    if (row.RowType != DataControlRowType.DataRow)
                        continue;

                    string employeeId = EmployeeGridView.DataKeys[row.RowIndex]["EmployeeID"].ToString();
                    RadioButtonList attendance = row.FindControl("AttendenceRadioButtonList") as RadioButtonList;
                    TextBox startTimeTextBox = row.FindControl("StartTimeTextBox") as TextBox;
                    TextBox endTimeTextBox = row.FindControl("EndTimeTextBox") as TextBox;

                    string status = attendance != null && !string.IsNullOrWhiteSpace(attendance.SelectedValue)
                        ? attendance.SelectedValue
                        : "Pre";

                    rows.Add(new PostedEmployeeAttendance
                    {
                        EmployeeId = int.Parse(employeeId, CultureInfo.InvariantCulture),
                        AttendanceStatus = status,
                        EntryTime = startTimeTextBox != null ? startTimeTextBox.Text : string.Empty,
                        ExitTime = endTimeTextBox != null ? endTimeTextBox.Text : string.Empty
                    });
                }
            }

            if (rows.Count > 0)
                return rows;

            for (int i = 0; i < 500; i++)
            {
                string employeeIdKey = FindGridFormKey(i, "EmployeeIDHidden");
                if (employeeIdKey == null)
                    break;

                string employeeId = Request.Form[employeeIdKey];
                if (string.IsNullOrWhiteSpace(employeeId))
                    continue;

                string attendanceKey = FindGridFormKey(i, "AttendenceRadioButtonList");
                string entryKey = FindGridFormKey(i, "StartTimeTextBox");
                string exitKey = FindGridFormKey(i, "EndTimeTextBox");

                string status = attendanceKey != null ? Request.Form[attendanceKey] : null;
                if (string.IsNullOrWhiteSpace(status))
                    status = "Pre";

                rows.Add(new PostedEmployeeAttendance
                {
                    EmployeeId = int.Parse(employeeId, CultureInfo.InvariantCulture),
                    AttendanceStatus = status,
                    EntryTime = entryKey != null ? Request.Form[entryKey] : string.Empty,
                    ExitTime = exitKey != null ? Request.Form[exitKey] : string.Empty
                });
            }

            if (rows.Count > 0)
                return rows;

            var employees = LoadEmployeesForSchedule(scheduleId);
            for (int i = 0; i < employees.Count; i++)
            {
                string attendanceKey = FindGridFormKey(i, "AttendenceRadioButtonList");
                string entryKey = FindGridFormKey(i, "StartTimeTextBox");
                string exitKey = FindGridFormKey(i, "EndTimeTextBox");

                string status = attendanceKey != null ? Request.Form[attendanceKey] : null;
                if (string.IsNullOrWhiteSpace(status))
                    status = "Pre";

                rows.Add(new PostedEmployeeAttendance
                {
                    EmployeeId = employees[i],
                    AttendanceStatus = status,
                    EntryTime = entryKey != null ? Request.Form[entryKey] : string.Empty,
                    ExitTime = exitKey != null ? Request.Form[exitKey] : string.Empty
                });
            }

            return rows;
        }

        private string FindGridFormKey(int dataRowIndex, string controlName)
        {
            string marker = "$ctl" + (dataRowIndex + 2).ToString("00", CultureInfo.InvariantCulture) + "$" + controlName;
            foreach (string key in Request.Form.AllKeys)
            {
                if (key != null && key.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                    return key;
            }

            return null;
        }

        private List<int> LoadEmployeesForSchedule(int scheduleId)
        {
            var employees = new List<int>();
            const string sql = @"SELECT VW_Emp_Info.EmployeeID
FROM VW_Emp_Info
INNER JOIN Employee_Attendance_Schedule_Assign
    ON VW_Emp_Info.EmployeeID = Employee_Attendance_Schedule_Assign.EmployeeID
   AND VW_Emp_Info.SchoolID = Employee_Attendance_Schedule_Assign.SchoolID
WHERE VW_Emp_Info.SchoolID = @SchoolID
  AND VW_Emp_Info.Job_Status = N'Active'
  AND VW_Emp_Info.EmployeeType LIKE @EmployeeType
  AND Employee_Attendance_Schedule_Assign.ScheduleID = @ScheduleID
ORDER BY VW_Emp_Info.ID";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = Convert.ToInt32(Session["SchoolID"]);
                cmd.Parameters.Add("@EmployeeType", SqlDbType.NVarChar, 20).Value = EmpTypeRadioButtonList.SelectedValue;
                cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        employees.Add(reader.GetInt32(0));
                }
            }

            return employees;
        }

        private bool TrySaveAttendanceRecord(string employeeId, int scheduleId, string attendanceStatus, DateTime attendanceDate, string entryTime, string exitTime, out string errorMessage)
        {
            errorMessage = null;
            const string upsertSql = @"
IF EXISTS (
    SELECT 1 FROM Employee_Attendance_Record
    WHERE EmployeeID = @EmployeeID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND Attendance_ScheduleID = @ScheduleID
)
BEGIN
    UPDATE Employee_Attendance_Record
    SET AttendanceStatus = @AttendanceStatus,
        EntryTime = @EntryTime,
        ExitTime = @ExitTime
    WHERE EmployeeID = @EmployeeID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND Attendance_ScheduleID = @ScheduleID
END
ELSE IF EXISTS (
    SELECT 1 FROM Employee_Attendance_Record
    WHERE EmployeeID = @EmployeeID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND Attendance_ScheduleID IS NULL
)
BEGIN
    UPDATE Employee_Attendance_Record
    SET AttendanceStatus = @AttendanceStatus,
        EntryTime = @EntryTime,
        ExitTime = @ExitTime,
        Attendance_ScheduleID = @ScheduleID
    WHERE EmployeeID = @EmployeeID
      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
      AND SchoolID = @SchoolID
      AND Attendance_ScheduleID IS NULL
END
ELSE
BEGIN
    INSERT INTO Employee_Attendance_Record
        (SchoolID, RegistrationID, EmployeeID, Attendance_ScheduleID, AttendanceStatus, AttendanceDate, EntryTime, ExitTime)
    VALUES
        (@SchoolID, @RegistrationID, @EmployeeID, @ScheduleID, @AttendanceStatus, @AttendanceDate, @EntryTime, @ExitTime)
END";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(upsertSql, con))
                    {
                        cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = Convert.ToInt32(Session["SchoolID"]);
                        cmd.Parameters.Add("@RegistrationID", SqlDbType.Int).Value = Convert.ToInt32(Session["RegistrationID"]);
                        cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = Convert.ToInt32(employeeId);
                        cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;
                        cmd.Parameters.Add("@AttendanceStatus", SqlDbType.NVarChar, 20).Value = attendanceStatus;
                        cmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate.Date;
                        cmd.Parameters.Add("@EntryTime", SqlDbType.Time).Value = ParseTimeOrDbNull(entryTime);
                        cmd.Parameters.Add("@ExitTime", SqlDbType.Time).Value = ParseTimeOrDbNull(exitTime);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    errorMessage = "ডুপ্লিকেট রেকর্ড। Database/Scripts/Employee_MultiSchedule_Step2_Index.sql আবার চালান।";
                else if (ex.Message.IndexOf("Attendance_ScheduleID", StringComparison.OrdinalIgnoreCase) >= 0)
                    errorMessage = "ডাটাবেসে Employee Attendance_ScheduleID কলাম নেই। Database/Scripts/Employee_MultiSchedule_Step1_Column.sql চালান।";
                else
                    errorMessage = "হাজিরা সংরক্ষণ ব্যর্থ: " + ex.Message;

                return false;
            }
        }

        private static object ParseTimeOrDbNull(string timeText)
        {
            if (string.IsNullOrWhiteSpace(timeText))
                return DBNull.Value;

            if (TimeSpan.TryParse(timeText, out TimeSpan time))
                return time;

            if (DateTime.TryParse(timeText, out DateTime dt))
                return dt.TimeOfDay;

            return DBNull.Value;
        }

        protected void AttendenceCheck()
        {
            if (!ShouldShowEmployeeGrid())
                return;

            if (!TryParseAttendanceDate(out DateTime attendanceDate))
                return;

            int scheduleId = int.Parse(ScheduleDropDownList.SelectedValue);
            _attendanceCheckDate = attendanceDate.Date;
            _existingAttendance = LoadExistingAttendance(_attendanceCheckDate, scheduleId);
            EmployeeGridView.DataBind();
        }

        protected void EmployeeGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow || _existingAttendance == null)
                return;

            RadioButtonList attendance = (RadioButtonList)e.Row.FindControl("AttendenceRadioButtonList");
            Label atDateLabel = (Label)e.Row.FindControl("AtDateLabel");
            TextBox startTimeTextBox = (TextBox)e.Row.FindControl("StartTimeTextBox");
            TextBox endTimeTextBox = (TextBox)e.Row.FindControl("EndTimeTextBox");
            if (attendance == null)
                return;

            string employeeId = EmployeeGridView.DataKeys[e.Row.RowIndex]["EmployeeID"].ToString();

            if (_existingAttendance.TryGetValue(employeeId, out DataRow attendanceRow))
            {
                e.Row.CssClass = "Diable_Rows";
                foreach (TableCell cell in e.Row.Cells)
                    cell.CssClass = "Diable_Rows";

                ApplyAttendanceSelection(attendance, attendanceRow["AttendanceStatus"].ToString().Trim());

                if (attendanceRow["EntryTime"] != DBNull.Value)
                    startTimeTextBox.Text = ((TimeSpan)attendanceRow["EntryTime"]).ToString(@"hh\:mm");
                if (attendanceRow["ExitTime"] != DBNull.Value)
                    endTimeTextBox.Text = ((TimeSpan)attendanceRow["ExitTime"]).ToString(@"hh\:mm");

                string status = attendanceRow["AttendanceStatus"].ToString();
                if (status == "Leave" || status == "Abs")
                {
                    startTimeTextBox.Enabled = false;
                    endTimeTextBox.Enabled = false;
                }

                return;
            }

            ApplyLeaveStatus(_attendanceCheckDate, employeeId, attendance, atDateLabel, startTimeTextBox, endTimeTextBox);

            if (attendance.SelectedIndex < 0)
            {
                ListItem preItem = attendance.Items.FindByValue("Pre");
                if (preItem != null)
                    preItem.Selected = true;
            }
        }

        private Dictionary<string, DataRow> LoadExistingAttendance(DateTime attendanceDate, int scheduleId)
        {
            var result = new Dictionary<string, DataRow>();

            const string sql = @"SELECT EmployeeID, AttendanceStatus, EntryTime, ExitTime
FROM Employee_Attendance_Record
WHERE CAST(AttendanceDate AS DATE) = @AttendanceDate
  AND SchoolID = @SchoolID
  AND Attendance_ScheduleID = @ScheduleID";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate.Date;
                cmd.Parameters.Add("@SchoolID", SqlDbType.Int).Value = Convert.ToInt32(Session["SchoolID"]);
                cmd.Parameters.Add("@ScheduleID", SqlDbType.Int).Value = scheduleId;

                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                    result[Convert.ToInt32(row["EmployeeID"]).ToString()] = row;
            }

            return result;
        }

        private void ApplyLeaveStatus(DateTime attendanceDate, string employeeId, RadioButtonList attendance, Label atDateLabel, TextBox startTimeTextBox, TextBox endTimeTextBox)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            using (SqlCommand leaveCmd = new SqlCommand("SELECT LeaveStartDate, LeaveEndDate FROM Employee_Leave WHERE EmployeeID = @EmployeeID AND LeaveStartDate <= @AttendanceDate AND LeaveEndDate >= @AttendanceDate AND SchoolID = @SchoolID", con))
            {
                leaveCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                leaveCmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate;
                leaveCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                con.Open();
                using (SqlDataReader reader = leaveCmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return;

                    attendance.ClearSelection();
                    attendance.Items[4].Selected = true;
                    atDateLabel.Text = "(From:" + ((DateTime)reader["LeaveStartDate"]).ToString("d MMM yy") + " To " + ((DateTime)reader["LeaveEndDate"]).ToString("d MMM yy") + ")";
                    startTimeTextBox.Enabled = false;
                    endTimeTextBox.Enabled = false;
                }
            }
        }

        private static void ApplyAttendanceSelection(RadioButtonList attendance, string attendanceValue)
        {
            if (string.IsNullOrWhiteSpace(attendanceValue))
                return;

            if (attendanceValue.IndexOf(',') >= 0)
                attendanceValue = attendanceValue.Split(',')[0].Trim();

            ListItem item = attendance.Items.FindByValue(attendanceValue) ?? attendance.Items.FindByText(attendanceValue);
            if (item == null)
            {
                string[] values = { "Pre", "Abs", "Late", "Late Abs", "Leave" };
                int index = Array.IndexOf(values, attendanceValue);
                if (index >= 0 && index < attendance.Items.Count)
                    item = attendance.Items[index];
            }

            if (item == null)
                return;

            attendance.ClearSelection();
            item.Selected = true;
        }

        protected void EmpTypeRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ShouldShowEmployeeGrid())
                EmployeeGridView.Visible = true;
        }

        private sealed class PostedEmployeeAttendance
        {
            public int EmployeeId { get; set; }
            public string AttendanceStatus { get; set; }
            public string EntryTime { get; set; }
            public string ExitTime { get; set; }
        }
    }
}
