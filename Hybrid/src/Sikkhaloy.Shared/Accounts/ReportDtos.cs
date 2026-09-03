namespace Sikkhaloy.Shared.Accounts;

public sealed class NameAmountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Amount2 { get; set; }
}

public sealed class ReportLineDto
{
    public int Sn { get; set; }
    public string UserName { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string Category { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string Details { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public DateTime? ActivityDate { get; set; }
    public string? Time { get; set; }
    public string? Session { get; set; }
}

public sealed class ReportGroupDto
{
    public string Category { get; set; } = "";
    public decimal Total { get; set; }
    public List<ReportLineDto> Lines { get; set; } = [];
}

public sealed class AccountsSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Net { get; set; }
    public decimal Payorder { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public decimal PresentDue { get; set; }
    public decimal Advance { get; set; }
    public decimal AccountTotal { get; set; }
    public List<NameAmountDto> Users { get; set; } = [];
    public List<NameAmountDto> Accounts { get; set; } = [];
    public List<NameAmountDto> IncomeCategories { get; set; } = [];
    public List<NameAmountDto> ExpenseCategories { get; set; } = [];
    public List<SessionReportDto> Sessions { get; set; } = [];
}

public sealed class SessionReportDto
{
    public int EducationYearID { get; set; }
    public string YearName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
    public decimal Payorder { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public List<NameAmountDto> Months { get; set; } = [];
}

public sealed class MonthBasedDto
{
    public string SchoolName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public List<string> Months { get; set; } = [];
    public List<MonthStudentRowDto> Students { get; set; } = [];
    public Dictionary<string, decimal> MonthTotals { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal GrandTotal { get; set; }
    public MonthMatrixDto IncomeDaily { get; set; } = new();
    public MonthMatrixDto IncomeMonthly { get; set; } = new();
    public MonthMatrixDto ExpenseDaily { get; set; } = new();
    public MonthMatrixDto ExpenseMonthly { get; set; } = new();
}

public sealed class MonthStudentRowDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public Dictionary<string, decimal> Months { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal Total { get; set; }
}

public sealed class MonthMatrixDto
{
    public List<string> Columns { get; set; } = [];
    public List<MonthRoleRowDto> Rows { get; set; } = [];
    public Dictionary<string, decimal> ColumnTotals { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal GrandTotal { get; set; }
}

public sealed class MonthRoleRowDto
{
    public string Role { get; set; } = "";
    public Dictionary<string, decimal> Months { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal Total { get; set; }
}

public sealed class IncomeExpenseReportDto
{
    public decimal Total { get; set; }
    public List<ReportGroupDto> Groups { get; set; } = [];
}

public sealed class NetReportDto
{
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Online { get; set; }
    public decimal CashInHand { get; set; }
    public List<NameAmountDto> IncomeCategories { get; set; } = [];
    public List<NameAmountDto> ClassIncome { get; set; } = [];
    public List<NameAmountDto> Donations { get; set; } = [];
    public List<NameAmountDto> ExpenseCategories { get; set; } = [];
    public List<ReportGroupDto> IncomeDetails { get; set; } = [];
    public List<ReportGroupDto> ExpenseDetails { get; set; } = [];
}

public sealed class CurrentDueDto
{
    public decimal InstitutionDue { get; set; }
    public List<CurrentDueRowDto> Students { get; set; } = [];
}

public sealed class CurrentDueRowDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string? ClassName { get; set; }
    public decimal Due { get; set; }
}

public sealed class CurrentDueLineDto
{
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class CurrentDueStudentDetailDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string? ClassName { get; set; }
    public decimal Due { get; set; }
    public List<CurrentDueLineDto> Lines { get; set; } = [];
}

public sealed class DueSmsRequest
{
    public List<string> Ids { get; set; } = [];
    public string? RoleId { get; set; }
}

public sealed class PayorderReportDto
{
    public decimal Payorder { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public List<PayorderRoleDto> Roles { get; set; } = [];
}

public sealed class PayorderRoleDto
{
    public int RoleID { get; set; }
    public string Role { get; set; } = "";
    public decimal Fee { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public List<PayorderRoleDto> PayFors { get; set; } = [];
}

public sealed class PaidDetailsDto
{
    public decimal Total { get; set; }
    public List<PaidReceiptDto> Receipts { get; set; } = [];
}

public sealed class PaidReceiptDto
{
    public int MoneyReceiptID { get; set; }
    public string ReceiptNo { get; set; } = "";
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public string? ClassName { get; set; }
    public string? Section { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaidDate { get; set; }
    public List<string> Details { get; set; } = [];
}

public sealed class MyAccountsDto
{
    public string UserName { get; set; } = "";
    public string? Designation { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
    public decimal Submitted { get; set; }
    public decimal Remaining { get; set; }
    public List<ReportGroupDto> IncomeGroups { get; set; } = [];
    public List<ReportGroupDto> ExpenseGroups { get; set; } = [];
}

public sealed class BalanceRemainingDto
{
    public decimal Remaining { get; set; }
}

public sealed class BalanceSubmitOtpRequest
{
    public string Phone { get; set; } = "";
}

public sealed class BalanceSubmitRequest
{
    public decimal Amount { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string Phone { get; set; } = "";
    public string Otp { get; set; } = "";
    public string PaymentMethod { get; set; } = "Cash";
    public string? Remarks { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
}

public sealed class AccountDetailCatDto
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    /// <summary>unpaid, deleted, adjust, or empty for regular rows.</summary>
    public string Badge { get; set; } = "";
}

public sealed class AccountDetailDto
{
    public int AccountID { get; set; }
    public string AccountName { get; set; } = "";
    public decimal Balance { get; set; }
    public decimal TotalAdd { get; set; }
    public decimal TotalSub { get; set; }
    public decimal Opening { get; set; }
    public decimal Closing { get; set; }
    public List<AccountDetailCatDto> Adds { get; set; } = [];
    public List<AccountDetailCatDto> AddAdjust { get; set; } = [];
    public List<AccountDetailCatDto> Subs { get; set; } = [];
    public List<AccountDetailCatDto> SubAdjust { get; set; } = [];
}

public sealed class AccountsLogDto
{
    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal AdjustTotal { get; set; }
    public List<ReportGroupDto> Income { get; set; } = [];
    public List<ReportGroupDto> Expense { get; set; } = [];
    public List<ReportGroupDto> Adjust { get; set; } = [];
}

public sealed class SessionClassReportDto
{
    public string YearName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Students { get; set; }
    public decimal Payorder { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public List<SessionClassRowDto> Classes { get; set; } = [];
    public List<SessionClassRowDto> Roles { get; set; } = [];
    public List<SessionClassRowDto> PayFors { get; set; } = [];
}

public sealed class SessionClassRowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Students { get; set; }
    public decimal Fee { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
}

public sealed class SessionStudentReportDto
{
    public int Students { get; set; }
    public int PayorderCount { get; set; }
    public decimal Payorder { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public decimal Percentage { get; set; }
    public List<SessionStudentRowDto> Rows { get; set; } = [];
}

public sealed class SessionStudentRowDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public int PayorderCount { get; set; }
    public decimal Fee { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public decimal Percentage { get; set; }
}

public sealed class SessionPaidDueDto
{
    public int Students { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalPages { get; set; } = 1;
    public decimal Fee { get; set; }
    public decimal LateFee { get; set; }
    public decimal Concession { get; set; }
    public decimal Receivable { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public List<SessionPaidDueStudentDto> Rows { get; set; } = [];
}

public sealed class SessionPaidDueStudentDto
{
    public int StudentClassID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public decimal Paid { get; set; }
    public decimal Unpaid { get; set; }
    public List<SessionPaidDueLineDto> Lines { get; set; } = [];
}

public sealed class SessionPaidDueLineDto
{
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Concession { get; set; }
    public decimal LateFee { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
    public string Status { get; set; } = "";
}

public sealed class SessionFilterDto
{
    public List<NameAmountDto> Classes { get; set; } = [];
    public List<NameAmountDto> Sections { get; set; } = [];
    public List<NameAmountDto> Roles { get; set; } = [];
    public List<NameAmountDto> PayFors { get; set; } = [];
}
