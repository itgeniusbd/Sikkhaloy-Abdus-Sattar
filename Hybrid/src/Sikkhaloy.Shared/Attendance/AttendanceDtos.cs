namespace Sikkhaloy.Shared.Attendance;

public sealed class AttendanceResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    /// <summary>Free-form details (e.g. schedule overlap skips). Prefer this over Error when set.</summary>
    public string? Message { get; set; }
    public int Saved { get; set; }
    public int Id { get; set; }
    public bool Queued { get; set; }
}

public sealed class AttendanceLeaveTypeDto
{
    public int LeaveTypeID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class SaveLeaveTypeRequest
{
    public string Name { get; set; } = "";
}

public sealed class AttendanceScheduleDto
{
    public int ScheduleID { get; set; }
    public string ScheduleName { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string LateEntryTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public List<AttendanceScheduleDayDto> Days { get; set; } = [];
}

public sealed class AttendanceScheduleDayDto
{
    public int ScheduleDayID { get; set; }
    public string Day { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string LateEntryTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public bool IsOnDay { get; set; }
}

public sealed class SaveScheduleRequest
{
    public string ScheduleName { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string LateEntryTime { get; set; } = "";
    public string EndTime { get; set; } = "";
}

public sealed class SaveScheduleDaysRequest
{
    public int ScheduleID { get; set; }
    public List<AttendanceScheduleDayDto> Days { get; set; } = [];
}

public sealed class AttendanceSettingsDto
{
    public int AttendanceSettingID { get; set; }
    public bool DeviceAttendance { get; set; }
    public bool AllSms { get; set; }
    public bool HolidayAsOffday { get; set; }
    public string SettingKey { get; set; } = "";
    public bool EnglishSms { get; set; }
    public int SmsTimeoutMinute { get; set; }
    public bool StudentAttendance { get; set; }
    public bool StudentAllSms { get; set; }
    public bool StudentEntrySms { get; set; }
    public bool StudentExitSms { get; set; }
    public bool StudentAbsSms { get; set; }
    public bool StudentLateSms { get; set; }
    public bool EmployeeAttendance { get; set; }
    public bool EmployeeSms { get; set; }
    public bool EmployeeAbsSms { get; set; }
    public bool EmployeeLateSms { get; set; }
    public bool EmployeeSmsOwnNumber { get; set; }
    public string? EmployeeSmsNumber { get; set; }
}

public sealed class AttendanceDownloadResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = [];
}

public sealed class StudentRfidRowDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public string? DeviceID { get; set; }
    public string? RFID { get; set; }
    public bool Assigned { get; set; }
    public bool PreSms { get; set; }
    public bool LateSms { get; set; }
    public bool AbsSms { get; set; }
    public bool ExitSms { get; set; }
}

public sealed class SaveStudentRfidRequest
{
    public int ScheduleID { get; set; }
    public List<StudentRfidSaveRow> Rows { get; set; } = [];
}

public sealed class StudentRfidSaveRow
{
    public int StudentID { get; set; }
    public string? Name { get; set; }
    public string? RFID { get; set; }
    public bool Assigned { get; set; }
    public bool PreSms { get; set; }
    public bool LateSms { get; set; }
    public bool AbsSms { get; set; }
    public bool ExitSms { get; set; }
}

public sealed class EmployeeRfidRowDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string? Phone { get; set; }
    public string? DeviceID { get; set; }
    public string? RFID { get; set; }
    public bool Assigned { get; set; }
    public bool AbsSms { get; set; }
    public bool LateSms { get; set; }
}

public sealed class SaveEmployeeRfidRequest
{
    public int ScheduleID { get; set; }
    public List<EmployeeRfidSaveRow> Rows { get; set; } = [];
}

public sealed class EmployeeRfidSaveRow
{
    public int EmployeeID { get; set; }
    public string? Name { get; set; }
    public string? RFID { get; set; }
    public bool Assigned { get; set; }
    public bool AbsSms { get; set; }
    public bool LateSms { get; set; }
}

public sealed class StudentManualRowDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string Attendance { get; set; } = "Pre";
    public string? Reason { get; set; }
    public string? TakenBy { get; set; }
    public string? LeaveRange { get; set; }
    public bool HasRecord { get; set; }
    public bool SendSms { get; set; }
    public bool Selected { get; set; }
}

public sealed class SaveStudentManualRequest
{
    public int ScheduleID { get; set; }
    public int ClassID { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<StudentManualSaveRow> Rows { get; set; } = [];
}

public sealed class StudentManualSaveRow
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string Attendance { get; set; } = "Pre";
    public string? Reason { get; set; }
    public bool SendSms { get; set; }
    public string? ID { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
}

public sealed class EmployeeManualRowDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string Attendance { get; set; } = "";
    public string? EntryTime { get; set; }
    public string? ExitTime { get; set; }
    public bool Selected { get; set; }
    public bool HasRecord { get; set; }
}

public sealed class SaveEmployeeManualRequest
{
    public int ScheduleID { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<EmployeeManualSaveRow> Rows { get; set; } = [];
}

public sealed class EmployeeManualSaveRow
{
    public int EmployeeID { get; set; }
    public string Attendance { get; set; } = "Pre";
    public string? EntryTime { get; set; }
    public string? ExitTime { get; set; }
}

public sealed class StudentAttendanceRecordDto
{
    public DateTime AttendanceDate { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public string Attendance { get; set; } = "";
    public string? Reason { get; set; }
    public string? EntryTime { get; set; }
    public string? ExitTime { get; set; }
}

public sealed class StudentAttendanceSummaryDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int LateAbs { get; set; }
    public int Leave { get; set; }
    public int Bunk { get; set; }
}

public sealed class EmployeeAttendanceRecordDto
{
    public DateTime AttendanceDate { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string Attendance { get; set; } = "";
    public string? EntryTime { get; set; }
    public string? ExitTime { get; set; }
}

public sealed class EmployeeAttendanceSummaryDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int LateAbs { get; set; }
    public int Leave { get; set; }
}

public sealed class StudentLeavePersonDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string? FathersName { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Section { get; set; }
    public string? GroupName { get; set; }
    public string? Shift { get; set; }
    public string? PhotoDataUrl { get; set; }
}

public sealed class StudentLeaveSuggestDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
}

public sealed class StudentLeavePrintDto
{
    public int StudentLeaveID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? FathersName { get; set; }
    public string? ClassName { get; set; }
    public string? GroupName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string? LeaveType { get; set; }
    public string? GuardianName { get; set; }
    public string? Description { get; set; }
    public string? ApproverName { get; set; }
    public DateTime ApprovedOn { get; set; }
    public string SchoolName { get; set; } = "";
    public string? SchoolAddress { get; set; }
    public string? SchoolPhone { get; set; }
}

public sealed class StudentLeaveRowDto
{
    public int StudentLeaveID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? LeaveType { get; set; }
    public string? GuardianName { get; set; }
    public string? Description { get; set; }
}

public sealed class SaveStudentLeaveRequest
{
    public int StudentID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? LeaveType { get; set; }
    public string? GuardianName { get; set; }
    public string? Description { get; set; }
}

public sealed class EmployeeLeavePickDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string? Phone { get; set; }
}

public sealed class SaveEmployeeLeaveRequest
{
    public List<int> EmployeeIDs { get; set; } = [];
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = "";
}

public sealed class LeaveReportRowDto
{
    public int LeaveID { get; set; }
    public string Type { get; set; } = "";
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassOrDesignation { get; set; }
    public string? LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }
    public string? Description { get; set; }
}

public sealed class AttendanceFineRowDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? MonthName { get; set; }
    public decimal FineAmount { get; set; }
    public int WorkingDays { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int LateAbs { get; set; }
    public int AbsCount { get; set; }
    public int Late { get; set; }
    public int Leave { get; set; }
    public int Bunk { get; set; }
}

public sealed class AttendanceMonthDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = "";
}

public sealed class GenerateFineRequest
{
    public int ClassID { get; set; }
    public DateTime MonthDate { get; set; }
    public string MonthName { get; set; } = "";
}
