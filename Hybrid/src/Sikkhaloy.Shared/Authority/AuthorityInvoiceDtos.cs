namespace Sikkhaloy.Shared.Authority;

public sealed class AuthAccountsPageDto
{
    public AuthCollectSummaryDto Summary { get; set; } = new();
    public int UnpaidSchools { get; set; }
    public int UnpaidInvoices { get; set; }
    public List<AuthInvoiceLineDto> RecentUnpaid { get; set; } = [];
}

public sealed class AuthProgressPageDto
{
    public string Filter { get; set; } = "%";
    public int Institutions { get; set; }
    public int ActiveStudent { get; set; }
    public int RejectCountable { get; set; }
    public int RejectUncountable { get; set; }
    public int TotalCountable { get; set; }
    public decimal ServiceCharge { get; set; }
    public string? Error { get; set; }
    public List<AuthProgressRowDto> Rows { get; set; } = [];
}

public sealed class AuthProgressRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public int ActiveStudent { get; set; }
    public int RejectCountable { get; set; }
    public int RejectUncountable { get; set; }
    public int FreeSms { get; set; }
    public int Countable { get; set; }
    public decimal PerStudent { get; set; }
    public decimal Fixed { get; set; }
    public decimal Discount { get; set; }
    public decimal ServiceCharge { get; set; }
    public bool PaymentActive { get; set; }
}

public sealed class AuthCollectPageDto
{
    public int CategoryId { get; set; }
    public List<AuthorityOptionDto> Categories { get; set; } = [];
    public AuthCollectSummaryDto Summary { get; set; } = new();
    public List<AuthCollectMonthDto> Months { get; set; } = [];
    public List<string> DetailMonths { get; set; } = [];
    public List<AuthInvoiceLineDto> PaidRows { get; set; } = [];
    public List<AuthInvoiceLineDto> DueRows { get; set; } = [];
}

public sealed class AuthCollectSummaryDto
{
    public int InvoiceCount { get; set; }
    public decimal UnitCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Receivable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public decimal CollectPercent { get; set; }
}

public sealed class AuthCollectMonthDto
{
    public int Year { get; set; }
    public int MonthNo { get; set; }
    public string Month { get; set; } = "";
    public int InvoiceCount { get; set; }
    public decimal UnitCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Receivable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public decimal CollectPercent { get; set; }
}

public sealed class AuthInvoiceLineDto
{
    public int InvoiceID { get; set; }
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string InvoiceSn { get; set; } = "";
    public string Category { get; set; } = "";
    public string InvoiceFor { get; set; } = "";
    public DateTime? MonthName { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public bool Selected { get; set; }
    public decimal PayAmount { get; set; }
}

public sealed class AuthManagePageDto
{
    public int Total { get; set; }
    public int Valid { get; set; }
    public int Invalid { get; set; }
    public int PaymentActive { get; set; }
    public List<AuthManageRowDto> Rows { get; set; } = [];
}

public sealed class AuthManageRowDto
{
    public int SchoolID { get; set; }
    public int SchoolSn { get; set; }
    public string SchoolName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime? Date { get; set; }
    public int FreeSms { get; set; }
    public decimal PerStudent { get; set; }
    public decimal Discount { get; set; }
    public decimal Fixed { get; set; }
    public bool PaymentActive { get; set; }
    public bool Valid { get; set; }
    public int CommitteeTotal { get; set; }
    public List<AuthCommitteeBillDto> Committee { get; set; } = [];
}

public sealed class AuthCommitteeBillDto
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public int MemberCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public bool Included { get; set; }
    public bool Active { get; set; }
}

public sealed class AuthManageSaveRequest
{
    public List<AuthManageRowDto> Rows { get; set; } = [];
}

public sealed class AuthCreatePageDto
{
    public List<AuthorityOptionDto> Months { get; set; } = [];
    public string SelectedMonth { get; set; } = "";
    public List<AuthServiceChargeRowDto> ServiceRows { get; set; } = [];
    public List<AuthSmsInvoiceRowDto> SmsRows { get; set; } = [];
    public List<AuthorityOptionDto> Categories { get; set; } = [];
    public List<AuthorityOptionDto> Schools { get; set; } = [];
    public List<AuthInvoiceLineDto> OtherInvoices { get; set; } = [];
    public List<AuthGraceRowDto> GraceRows { get; set; } = [];
    public AuthJobStatusDto Job { get; set; } = new();
}

public sealed class AuthServiceChargeRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public decimal StudentCount { get; set; }
    public decimal CommitteeCount { get; set; }
    public decimal Billable { get; set; }
    public decimal PerStudent { get; set; }
    public int RejectCountable { get; set; }
    public int RejectUncountable { get; set; }
    public bool PaymentActive { get; set; }
    public int ActiveStudent { get; set; }
    public decimal Discount { get; set; }
    public decimal Fixed { get; set; }
    public bool Selected { get; set; }
}

public sealed class AuthSmsInvoiceRowDto
{
    public int Id { get; set; }
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public decimal RechargeSms { get; set; }
    public decimal PerSms { get; set; }
    public decimal Total { get; set; }
    public DateTime? Date { get; set; }
    public string UserName { get; set; } = "";
}

public sealed class AuthGraceRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public DateTime? Until { get; set; }
    public bool Active { get; set; }
}

public sealed class AuthJobStatusDto
{
    public string Name { get; set; } = "Auto_Generate_Monthly_Invoice";
    public bool Found { get; set; }
    public bool Enabled { get; set; }
    public string LastRun { get; set; } = "";
    public string LastStatus { get; set; } = "";
    public string NextRun { get; set; } = "";
    public string Error { get; set; } = "";
}

public sealed class AuthGenerateCountRequest
{
    public string Month { get; set; } = "";
}

public sealed class AuthCreateServiceRequest
{
    public string Month { get; set; } = "";
    public string IssueDate { get; set; } = "";
    public List<AuthServiceChargeRowDto> Rows { get; set; } = [];
}

public sealed class AuthCreateOtherRequest
{
    public int CategoryId { get; set; }
    public int SchoolID { get; set; }
    public string MonthName { get; set; } = "";
    public string IssueDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public string InvoiceFor { get; set; } = "";
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
}

public sealed class AuthAddCategoryRequest
{
    public string Name { get; set; } = "";
}

public sealed class AuthIdRequest
{
    public int Id { get; set; }
}

public sealed class AuthGraceRequest
{
    public int SchoolID { get; set; }
    public string Until { get; set; } = "";
}

public sealed class AuthPaidPageDto
{
    public int SchoolID { get; set; }
    public List<AuthorityOptionDto> Schools { get; set; } = [];
    public List<AuthInvoiceLineDto> Invoices { get; set; } = [];
}

public sealed class AuthPayInvoiceRequest
{
    public int SchoolID { get; set; }
    public string PaidDate { get; set; } = "";
    public string PaymentBy { get; set; } = "";
    public string CollectedBy { get; set; } = "";
    public string Method { get; set; } = "";
    public List<AuthInvoiceLineDto> Rows { get; set; } = [];
}

public sealed class AuthPrintPageDto
{
    public int SchoolID { get; set; }
    public List<AuthorityOptionDto> Schools { get; set; } = [];
    public List<AuthInvoiceLineDto> Unpaid { get; set; } = [];
    public List<AuthReceiptRowDto> Receipts { get; set; } = [];
}

public sealed class AuthReceiptRowDto
{
    public int ReceiptId { get; set; }
    public int SchoolID { get; set; }
    public string ReceiptSn { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string PaymentBy { get; set; } = "";
    public string PaidByUser { get; set; } = "";
    public string CollectedBy { get; set; } = "";
    public string Method { get; set; } = "";
    public DateTime? PaidDate { get; set; }
}

public sealed class AuthPayPrintDto
{
    public bool Found { get; set; }
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public decimal GrandTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Due { get; set; }
    public List<AuthPayPrintLineDto> Lines { get; set; } = [];
}

public sealed class AuthPayPrintLineDto
{
    public string Category { get; set; } = "";
    public string InvoiceFor { get; set; } = "";
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Due { get; set; }
}

public sealed class AuthReceiptPrintDto
{
    public bool Found { get; set; }
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ReceiptSn { get; set; } = "";
    public string PaymentBy { get; set; } = "";
    public string CollectedBy { get; set; } = "";
    public string Method { get; set; } = "";
    public DateTime? PaidDate { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalDue { get; set; }
    public List<AuthReceiptPrintLineDto> Lines { get; set; } = [];
}

public sealed class AuthReceiptPrintLineDto
{
    public string Category { get; set; } = "";
    public string InvoiceFor { get; set; } = "";
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Paid { get; set; }
}

public sealed class AuthOnlinePayPageDto
{
    public string Type { get; set; } = "All";
    public int SchoolID { get; set; }
    public string Method { get; set; } = "";
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<AuthorityOptionDto> Schools { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public decimal OnlineAmount { get; set; }
    public decimal OfflineAmount { get; set; }
    public int TotalCount { get; set; }
    public int OnlineCount { get; set; }
    public int OfflineCount { get; set; }
    public int InstitutionCount { get; set; }
    public List<AuthOnlinePayRowDto> Rows { get; set; } = [];
}

public sealed class AuthOnlinePayRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public decimal Amount { get; set; }
    public string Method { get; set; } = "";
    public string Type { get; set; } = "";
    public string CollectedBy { get; set; } = "";
    public string Reference { get; set; } = "";
    public DateTime? PaymentDate { get; set; }
}
