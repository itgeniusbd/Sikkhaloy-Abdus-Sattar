namespace Sikkhaloy.Shared.Accounts;

public sealed class AccountsIdRequest
{
    public int Id { get; set; }
}

public sealed class AccountsResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Id { get; set; }
    public int Count { get; set; }
    public int Saved { get; set; }
    public int Failed { get; set; }
    public string? ReceiptNo { get; set; }
}

public sealed class PaymentRoleDto
{
    public int RoleID { get; set; }
    public string Role { get; set; } = "";
    public int NumberOfPay { get; set; }
    public string? Description { get; set; }
}

public sealed class SavePaymentRoleRequest
{
    public string Role { get; set; } = "";
    public int NumberOfPay { get; set; }
    public string? Description { get; set; }
}

public sealed class AssignedRoleDto
{
    public int AssignRoleID { get; set; }
    public int RoleID { get; set; }
    public string Role { get; set; } = "";
    public int NumberOfPay { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Selected { get; set; }
}

public sealed class BulkAssignItem
{
    public int RoleID { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class BulkAssignRoleRequest
{
    public List<int> ClassIDs { get; set; } = [];
    public List<BulkAssignItem> Items { get; set; } = [];
}

public sealed class AssignedPayForDto
{
    public string PayFor { get; set; } = "";
    public int ClassCount { get; set; }
}

public sealed class RoleAssignStatusDto
{
    public int RoleID { get; set; }
    public string Role { get; set; } = "";
    public int NumberOfPay { get; set; }
    public string? Description { get; set; }
    public int MaxRemaining { get; set; }
    public int ClassesNeeding { get; set; }
    public int SelectedClassCount { get; set; }
    public List<AssignedPayForDto> Assigned { get; set; } = [];
}

public sealed class AssignableRolesDto
{
    public List<RoleAssignStatusDto> OneTime { get; set; } = [];
    public List<RoleAssignStatusDto> Multi { get; set; } = [];
}

public sealed class SaveAssignedRoleRequest
{
    public int ClassID { get; set; }
    public int RoleID { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class UpdateAssignedRoleRequest
{
    public int AssignRoleID { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class PayOrderStudentDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? RollNo { get; set; }
    public bool IsNew { get; set; }
    public bool Selected { get; set; }
}

public sealed class CreatePayOrdersRequest
{
    public List<int> StudentClassIDs { get; set; } = [];
    public List<CreatePayOrderItem> Items { get; set; } = [];
}

public sealed class CreatePayOrderItem
{
    public int RoleID { get; set; }
    public int AssignRoleID { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public decimal Discount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class RemovePayOrderRequest
{
    public List<int> PayOrderIDs { get; set; } = [];
}

public sealed class ChangePayOrderDateRequest
{
    public int AssignRoleID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class UnpaidPayOrderDto
{
    public int PayOrderID { get; set; }
    public int RoleID { get; set; }
    public int StudentID { get; set; }
    public int ClassID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Selected { get; set; }
}

public sealed class CashAccountDto
{
    public int AccountID { get; set; }
    public string AccountName { get; set; } = "";
    public decimal Balance { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class SaveCashAccountRequest
{
    public string AccountName { get; set; } = "";
}

public sealed class AccountMoveRequest
{
    public int AccountID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Details { get; set; }
}

public sealed class AccountMoveDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Details { get; set; }
}

public sealed class FeeStudentDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public int EducationYearID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public string? FathersName { get; set; }
    public string? Section { get; set; }
    public string? Shift { get; set; }
    public string? Status { get; set; }
    public string? EducationYear { get; set; }
}

public sealed class FeeSuggestDto
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
}

public sealed class DueRowDto
{
    public int PayOrderID { get; set; }
    public int RoleID { get; set; }
    public int EducationYearID { get; set; }
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public string? YearName { get; set; }
    public string? ClassName { get; set; }
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public decimal StoredLateFee { get; set; }
    public decimal Discount { get; set; }
    public decimal LateFeeDiscount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public decimal PayNow { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Overdue { get; set; }
    public bool CurrentYear { get; set; }
    public bool Selected { get; set; }
}

public sealed class FeeStudentBundleDto
{
    public FeeStudentDto? Student { get; set; }
    public decimal CurrentDue { get; set; }
    public List<DueRowDto> CurrentDues { get; set; } = [];
    public List<DueRowDto> OtherDues { get; set; } = [];
    public List<ReceiptListDto> Receipts { get; set; } = [];
    public List<ReceiptListDto> PreviousReceipts { get; set; } = [];
}

public sealed class CollectPaymentRequest
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int EducationYearID { get; set; }
    public int AccountID { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool SendSms { get; set; }
    public List<CollectPaymentItem> Items { get; set; } = [];
}

public sealed class CollectPaymentItem
{
    public int PayOrderID { get; set; }
    public decimal PaidAmount { get; set; }
}

public sealed class AddMorePayOrderRequest
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public int RoleID { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
}

public sealed class SaveConcessionRequest
{
    public List<ConcessionItem> Items { get; set; } = [];
    public string? Reason { get; set; }
}

public sealed class ConcessionItem
{
    public int PayOrderID { get; set; }
    public decimal Discount { get; set; }
    public decimal LateFee { get; set; }
    public decimal LateFeeDiscount { get; set; }
    public bool SetLateFee { get; set; }
}

public sealed class ReceiptListDto
{
    public int MoneyReceiptID { get; set; }
    public string ReceiptNo { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateTime PaidDate { get; set; }
    public DateTime CollectionDate { get; set; }
    public int EducationYearID { get; set; }
    public string? YearName { get; set; }
    public string? PrintedReceiptNo { get; set; }
    public string? ReceivedBy { get; set; }
    public string? PayFor { get; set; }
}

public sealed class ReceiptDetailDto
{
    public int MoneyReceiptID { get; set; }
    public string ReceiptNo { get; set; } = "";
    public DateTime PaidDate { get; set; }
    public DateTime CollectionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ReceivedBy { get; set; }
    public string? AccountName { get; set; }
    public string? PrintedReceiptNo { get; set; }
    public FeeStudentDto? Student { get; set; }
    public List<ReceiptLineDto> Lines { get; set; } = [];
    public List<ReceiptDueLineDto> RemainingDues { get; set; } = [];
}

public sealed class ReceiptLineDto
{
    public int PayOrderID { get; set; }
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public string? YearName { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
}

public sealed class ReceiptDueLineDto
{
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public string? YearName { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
}

public sealed class PaymentSmsSettingDto
{
    public bool Active { get; set; }
    public int Balance { get; set; }
}

public sealed class PrintedReceiptRequest
{
    public int MoneyReceiptID { get; set; }
    public string? PrintedReceiptNo { get; set; }
}

public sealed class ExtraIncomeCategoryDto
{
    public int ExtraIncomeCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ExtraIncomeDto
{
    public int ExtraIncomeID { get; set; }
    public int ExtraIncomeCategoryID { get; set; }
    public string Category { get; set; } = "";
    public string? Details { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? ReceivedBy { get; set; }
}

public sealed class ExtraIncomeListDto
{
    public List<ExtraIncomeDto> Items { get; set; } = [];
    public decimal Total { get; set; }
}

public sealed class SaveExtraCategoryRequest
{
    public int ExtraIncomeCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class SaveExtraIncomeRequest
{
    public int ExtraIncomeID { get; set; }
    public int ExtraIncomeCategoryID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Details { get; set; }
    public int AccountID { get; set; }
}

public sealed class ExpenseCategoryDto
{
    public int ExpenseCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ExpenseSubCategoryDto
{
    public int ExpenseSubCategoryID { get; set; }
    public int ExpenseCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ExpenseDto
{
    public int ExpenseID { get; set; }
    public int ExpenseCategoryID { get; set; }
    public int? ExpenseSubCategoryID { get; set; }
    public string Category { get; set; } = "";
    public string? SubCategory { get; set; }
    public string? Details { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? ReceivedBy { get; set; }
}

public sealed class ExpenseListDto
{
    public List<ExpenseDto> Items { get; set; } = [];
    public decimal Total { get; set; }
}

public sealed class SaveExpenseCategoryRequest
{
    public int ExpenseCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class SaveExpenseSubCategoryRequest
{
    public int ExpenseSubCategoryID { get; set; }
    public int ExpenseCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class SaveExpenseRequest
{
    public int ExpenseID { get; set; }
    public int ExpenseCategoryID { get; set; }
    public int? ExpenseSubCategoryID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Details { get; set; }
    public int AccountID { get; set; }
}
