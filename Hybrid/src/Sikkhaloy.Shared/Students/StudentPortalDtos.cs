namespace Sikkhaloy.Shared.Students;

public sealed class StudentPortalDashboardDto
{
    public string StudentsName { get; set; } = "";
    public string StudentCode { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string SchoolName { get; set; } = "";
    public string YearName { get; set; } = "";
    public string? PhotoDataUrl { get; set; }
    public decimal AttendancePct { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int AttendanceTotal { get; set; }
    public decimal AvgMarks { get; set; }
    public decimal AvgPoint { get; set; }
    public decimal AvgPosition { get; set; }
    public int ClassSize { get; set; }
    public decimal PassPct { get; set; }
    public decimal CurrentDue { get; set; }
    public List<StudentPortalSubjectDto> Subjects { get; set; } = [];
    public List<StudentPortalCountDto> Attendance { get; set; } = [];
    public List<StudentPortalExamDto> UpcomingExams { get; set; } = [];
    public List<StudentPortalPeriodDto> TodayRoutine { get; set; } = [];
    public List<StudentPortalNoticeDto> Notices { get; set; } = [];
}

public sealed class StudentPortalSubjectDto
{
    public string Name { get; set; } = "";
    public decimal Avg { get; set; }
    public string Grade { get; set; } = "";
}

public sealed class StudentPortalCountDto
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public sealed class StudentPortalExamDto
{
    public string Name { get; set; } = "";
    public string Subject { get; set; } = "";
    public DateTime? Date { get; set; }
    public decimal Point { get; set; }
    public string Grade { get; set; } = "";
}

public sealed class StudentPortalPeriodDto
{
    public string Period { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Time { get; set; } = "";
}

public sealed class StudentPortalNoticeDto
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime? Date { get; set; }
    public bool IsHomeWork { get; set; }
}

public sealed class StudentPortalDetailsDto
{
    public string StudentsName { get; set; } = "";
    public string StudentCode { get; set; } = "";
    public string Gender { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string BloodGroup { get; set; } = "";
    public string Religion { get; set; } = "";
    public string PermanentAddress { get; set; } = "";
    public string PresentAddress { get; set; } = "";
    public string Email { get; set; } = "";
    public string FathersName { get; set; } = "";
    public string MothersName { get; set; } = "";
    public string GuardianName { get; set; } = "";
    public string GuardianRelation { get; set; } = "";
    public string? PhotoDataUrl { get; set; }
}

public sealed class StudentPortalAttendanceDayDto
{
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
    public string? EntryTime { get; set; }
    public string? ExitTime { get; set; }
}

public sealed class StudentPortalHolidayDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = "";
}

public sealed class StudentPortalLeaveDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class StudentPortalAttendanceDto
{
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int LateAbsent { get; set; }
    public int Leave { get; set; }
    public List<StudentPortalAttendanceDayDto> Days { get; set; } = [];
    public List<StudentPortalHolidayDto> Holidays { get; set; } = [];
    public List<StudentPortalLeaveDto> Leaves { get; set; } = [];
}

public sealed class StudentPortalSmsDto
{
    public string Phone { get; set; } = "";
    public string Text { get; set; } = "";
    public string Purpose { get; set; } = "";
    public DateTime? Date { get; set; }
}

public sealed class StudentPortalAccountRowDto
{
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class StudentPortalAccountsBundleDto
{
    public bool OnlinePaymentEnabled { get; set; }
    public decimal TotalDue { get; set; }
    public decimal CurrentDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalConcession { get; set; }
    public StudentPortalPayOrderSummaryDto Summary { get; set; } = new();
    public List<StudentPortalDueRowDto> TotalDues { get; set; } = [];
    public List<StudentPortalDueRowDto> CurrentDues { get; set; } = [];
    public List<StudentPortalDueRowDto> Paid { get; set; } = [];
    public List<StudentPortalReceiptDto> Receipts { get; set; } = [];
    public List<StudentPortalDueRowDto> Concessions { get; set; } = [];
    public List<StudentPortalLateFeeDto> LateFeeConcessions { get; set; } = [];
    public List<StudentPortalLateFeeDto> LateFeeCharges { get; set; } = [];
    public List<StudentPortalDueRowDto> AllPayOrders { get; set; } = [];
}

public sealed class StudentPortalPayOrderSummaryDto
{
    public decimal TotalFee { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalLateFee { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Unpaid { get; set; }
}

public sealed class StudentPortalDueRowDto
{
    public int PayOrderID { get; set; }
    public int RoleID { get; set; }
    public int EducationYearID { get; set; }
    public int StudentClassID { get; set; }
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public string YearName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public decimal LateFee { get; set; }
    public decimal LateFeeDiscount { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? LastPaidDate { get; set; }
}

public sealed class StudentPortalReceiptDto
{
    public int MoneyReceiptID { get; set; }
    public string ReceiptNo { get; set; } = "";
    public DateTime? PaidDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentBy { get; set; } = "";
}

public sealed class StudentPortalReceiptLineDto
{
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public decimal Paid { get; set; }
    public DateTime? PaidDate { get; set; }
}

public sealed class StudentPortalLateFeeDto
{
    public decimal PreviousAmount { get; set; }
    public decimal PostAmount { get; set; }
    public string Reason { get; set; } = "";
    public DateTime? Date { get; set; }
}

public sealed class StudentPortalPayStartRequest
{
    public List<int> PayOrderIDs { get; set; } = [];
    public string ReturnUrl { get; set; } = "";
}

public sealed class StudentPortalPayStartResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? PaymentUrl { get; set; }
}

public sealed class StudentPortalPayCompleteResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? ReceiptNo { get; set; }
}

public sealed class StudentPortalSessionReportDto
{
    public string YearName { get; set; } = "";
    public decimal AvgPosition { get; set; }
    public decimal AvgPoint { get; set; }
    public decimal PassPct { get; set; }
    public decimal AvgMarks { get; set; }
}

public sealed class StudentPortalFaultReportDto
{
    public int StudentFaultID { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime? Date { get; set; }
    public string PostBy { get; set; } = "";
}
