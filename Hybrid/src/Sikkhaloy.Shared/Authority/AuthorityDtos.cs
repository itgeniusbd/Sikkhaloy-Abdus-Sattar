namespace Sikkhaloy.Shared.Authority;

public sealed class AuthorityDashboardDto
{
    public int Total { get; set; }
    public int Valid { get; set; }
    public int Invalid { get; set; }
    public int NewThisYear { get; set; }
    public int AllInstitutions { get; set; }
    public int Active15m { get; set; }
    public int Today { get; set; }
    public int LastHour { get; set; }
    public int Online5m { get; set; }
    public int DueInstitutions { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public decimal MonthPaid { get; set; }
    public decimal MonthDue { get; set; }
    public decimal MonthPayable { get; set; }
    public decimal LastMonthPaid { get; set; }
    public decimal LastMonthDue { get; set; }
    public decimal LastMonthPayable { get; set; }
    public decimal OutstandingDue { get; set; }
    public List<AuthorityYearCountDto> Yearly { get; set; } = [];
    public List<AuthorityTopPaidDto> TopPaid { get; set; } = [];
    public List<AuthorityInstitutionRowDto> Rows { get; set; } = [];
}

public sealed class AuthorityYearCountDto
{
    public int Year { get; set; }
    public int Count { get; set; }
}

public sealed class AuthorityTopPaidDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public decimal Paid { get; set; }
}

public sealed class EnterSchoolRequest
{
    public int SchoolID { get; set; }
    public int EducationYearID { get; set; }
}

public sealed class InstitutionDetailsDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string Principal { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Validation { get; set; } = "";
    public List<InstitutionYearRowDto> Years { get; set; } = [];
    public decimal SmsBalance { get; set; }
    public List<InstSmsRowDto> SmsHistory { get; set; } = [];
    public InstDueNoticeDto DueNotice { get; set; } = new();
}

public sealed class InstSmsRowDto
{
    public int Id { get; set; }
    public decimal RechargeSms { get; set; }
    public decimal PerSms { get; set; }
    public decimal Total { get; set; }
    public DateTime? Date { get; set; }
    public string UserName { get; set; } = "";
    public bool IsPaid { get; set; }
}

public sealed class InstSmsRechargeRequest
{
    public int SchoolID { get; set; }
    public int Quantity { get; set; }
    public decimal PerSms { get; set; }
}

public sealed class InstDueNoticeDto
{
    public bool Enabled { get; set; }
    public DateTime? HideUntil { get; set; }
    public string Reason { get; set; } = "";
    public DateTime? CreatedDate { get; set; }
}

public sealed class InstDueNoticeRequest
{
    public int SchoolID { get; set; }
    public bool Enabled { get; set; }
    public string? HideUntil { get; set; }
    public string? Reason { get; set; }
}

public sealed class InstStudentFindDto
{
    public bool Found { get; set; }
    public int StudentID { get; set; }
    public string Id { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string Class { get; set; } = "";
    public string RollNo { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed class InstChangeIdRequest
{
    public int SchoolID { get; set; }
    public string OldId { get; set; } = "";
    public string NewId { get; set; } = "";
}

public sealed class InstIdRequest
{
    public int SchoolID { get; set; }
    public string Id { get; set; } = "";
}

public sealed class InstReceiptDto
{
    public bool Found { get; set; }
    public string StudentId { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string Class { get; set; } = "";
    public string ReceiptSn { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public List<InstReceiptLineDto> Lines { get; set; } = [];
}

public sealed class InstReceiptLineDto
{
    public string PayFor { get; set; } = "";
    public string Role { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
}

public sealed class InstReceiptRequest
{
    public int SchoolID { get; set; }
    public string ReceiptSn { get; set; } = "";
}

public sealed class InstitutionYearRowDto
{
    public int EducationYearID { get; set; }
    public int SN { get; set; }
    public bool IsActive { get; set; }
    public string EducationYear { get; set; } = "";
    public int TotalStudent { get; set; }
}

public sealed class SaveInstitutionYearsRequest
{
    public int SchoolID { get; set; }
    public List<InstitutionYearRowDto> Years { get; set; } = [];
}

public sealed class AuthorityInstitutionRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Validation { get; set; } = "";
    public string OnlineStatus { get; set; } = "";
    public string LoggedInUser { get; set; } = "";
    public string LoginRole { get; set; } = "";
    public DateTime? LoginTime { get; set; }
    public DateTime? LastActivity { get; set; }
    public DateTime? Registered { get; set; }
    public string SessionNames { get; set; } = "";
}
