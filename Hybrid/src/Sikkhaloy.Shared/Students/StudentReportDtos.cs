namespace Sikkhaloy.Shared.Students;

public sealed class StudentReportDto
{
    public bool Found { get; set; }
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public string? PhotoDataUrl { get; set; }
    public StudentReportResultDto Result { get; set; } = new();
    public StudentReportAttendanceDto Attendance { get; set; } = new();
    public List<StudentReportSubjectDto> Subjects { get; set; } = [];
    public StudentReportAccountsDto Accounts { get; set; } = new();
}

public sealed class StudentReportResultDto
{
    public string? BestSubject { get; set; }
    public decimal? BestAvg { get; set; }
    public string? WorstSubject { get; set; }
    public decimal? WorstAvg { get; set; }
    public decimal? AvgPosition { get; set; }
    public decimal? AvgPoint { get; set; }
    public decimal? AvgMark { get; set; }
    public decimal? PassPercent { get; set; }
    public List<StudentReportSubjectAvgDto> SubjectAvgs { get; set; } = [];
    public List<StudentReportSessionDto> Sessions { get; set; } = [];
    public List<StudentReportExamDto> Exams { get; set; } = [];
}

public sealed class StudentReportSubjectAvgDto
{
    public string SubjectName { get; set; } = "";
    public decimal AvgMark { get; set; }
    public decimal Position { get; set; }
}

public sealed class StudentReportSessionDto
{
    public string EducationYear { get; set; } = "";
    public decimal? AvgPosition { get; set; }
    public decimal? AvgPoint { get; set; }
    public decimal? PassPercent { get; set; }
    public decimal? AvgMark { get; set; }
}

public sealed class StudentReportExamDto
{
    public string ExamName { get; set; } = "";
    public string? Grade { get; set; }
    public decimal? Point { get; set; }
    public int? Position { get; set; }
    public decimal? Obtained { get; set; }
    public decimal? Total { get; set; }
    public decimal? Percent { get; set; }
    public string? PassStatus { get; set; }
}

public sealed class StudentReportAttendanceDto
{
    public int WorkingDays { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int LateAbsent { get; set; }
    public int Leave { get; set; }
    public int Bunk { get; set; }
    public List<StudentReportAttendanceDayDto> Days { get; set; } = [];
    public List<StudentReportHolidayDto> Holidays { get; set; } = [];
    public List<StudentReportLeaveDto> Leaves { get; set; } = [];
}

public sealed class StudentReportAttendanceDayDto
{
    public DateTime Date { get; set; }
    public string Attendance { get; set; } = "";
    public string? EntryTime { get; set; }
    public string? ExitTime { get; set; }
}

public sealed class StudentReportHolidayDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = "";
}

public sealed class StudentReportLeaveDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? LeaveType { get; set; }
    public string? Description { get; set; }
}

public sealed class StudentReportSubjectDto
{
    public string SubjectName { get; set; } = "";
    public string SubjectType { get; set; } = "Compulsory";
}

public sealed class StudentReportAccountsDto
{
    public decimal TotalFee { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalLateFee { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }
    public decimal CurrentDueTotal { get; set; }
    public List<StudentReportPayOrderDto> Due { get; set; } = [];
    public List<StudentReportPayOrderDto> CurrentDue { get; set; } = [];
    public List<StudentReportPayOrderDto> Paid { get; set; } = [];
    public List<StudentReportReceiptDto> Receipts { get; set; } = [];
    public List<StudentReportConcessionDto> Concession { get; set; } = [];
    public List<StudentReportPayOrderDto> AllPayOrders { get; set; } = [];
}

public sealed class StudentReportPayOrderDto
{
    public string? Session { get; set; }
    public string? ClassName { get; set; }
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public decimal LateFee { get; set; }
    public decimal LateFeeDiscount { get; set; }
    public DateTime? LastPaidDate { get; set; }
}

public sealed class StudentReportReceiptDto
{
    public string ReceiptNo { get; set; } = "";
    public string PrintedReceiptNo { get; set; } = "";
    public DateTime? PaidDate { get; set; }
    public decimal Amount { get; set; }
}

public sealed class StudentReportConcessionDto
{
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public decimal Discount { get; set; }
}
