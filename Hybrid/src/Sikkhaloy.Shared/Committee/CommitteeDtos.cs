namespace Sikkhaloy.Shared.Committee;

public sealed class CommitteeResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int Id { get; set; }
    public int ReceiptId { get; set; }
}

public sealed class CommitteeOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class CommitteeLookupsDto
{
    public List<CommitteeOptionDto> Types { get; set; } = [];
    public List<CommitteeOptionDto> Categories { get; set; } = [];
    public List<CommitteeOptionDto> Members { get; set; } = [];
    public List<CommitteeAccountDto> Accounts { get; set; } = [];
    public List<CommitteeOptionDto> Years { get; set; } = [];
}

public sealed class CommitteeAccountDto
{
    public int AccountID { get; set; }
    public string AccountName { get; set; } = "";
    public decimal Balance { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class CommitteeMemberTypeDto
{
    public int CommitteeMemberTypeId { get; set; }
    public string CommitteeMemberType { get; set; } = "";
    public DateTime? InsertDate { get; set; }
}

public sealed class SaveCommitteeMemberTypeRequest
{
    public int CommitteeMemberTypeId { get; set; }
    public string Name { get; set; } = "";
}

public sealed class CommitteeMemberDto
{
    public int CommitteeMemberId { get; set; }
    public int CommitteeMemberTypeId { get; set; }
    public string MemberName { get; set; } = "";
    public string MemberType { get; set; } = "";
    public string ReferenceBy { get; set; } = "";
    public string SmsNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string Status { get; set; } = "Active";
    public decimal TotalDonation { get; set; }
    public decimal PaidDonation { get; set; }
    public decimal DueDonation { get; set; }
    public bool Selected { get; set; }
}

public sealed class SaveCommitteeMemberRequest
{
    public int CommitteeMemberId { get; set; }
    public int CommitteeMemberTypeId { get; set; }
    public string MemberName { get; set; } = "";
    public string? ReferenceBy { get; set; }
    public string SmsNumber { get; set; } = "";
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class DonationCategoryDto
{
    public int CommitteeDonationCategoryId { get; set; }
    public string DonationCategory { get; set; } = "";
    public DateTime? InsertDate { get; set; }
}

public sealed class SaveDonationCategoryRequest
{
    public int CommitteeDonationCategoryId { get; set; }
    public string Name { get; set; } = "";
}

public sealed class DonorSuggestDto
{
    public int CommitteeMemberId { get; set; }
    public string MemberName { get; set; } = "";
    public string SmsNumber { get; set; } = "";
}

public sealed class AddDonationRequest
{
    public int CommitteeMemberId { get; set; }
    public int CommitteeDonationCategoryId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? PromiseDate { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public int AccountId { get; set; }
}

public sealed class DonationListDto
{
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
    public List<DonationRowDto> Rows { get; set; } = [];
}

public sealed class DonationRowDto
{
    public int CommitteeDonationId { get; set; }
    public int CommitteeDonationCategoryId { get; set; }
    public int CommitteeMemberId { get; set; }
    public string MemberName { get; set; } = "";
    public string MemberType { get; set; } = "";
    public string SmsNumber { get; set; } = "";
    public string DonationCategory { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public string Description { get; set; } = "";
    public DateTime? InsertDate { get; set; }
    public DateTime? PromiseDate { get; set; }
    public bool CanDelete { get; set; }
}

public sealed class UpdateDonationRequest
{
    public int CommitteeDonationId { get; set; }
    public int CommitteeDonationCategoryId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public sealed class CollectPageDto
{
    public CommitteeMemberDto? Member { get; set; }
    public List<DonationDueDto> Dues { get; set; } = [];
    public List<MemberReceiptDto> Receipts { get; set; } = [];
}

public sealed class DonationDueDto
{
    public int CommitteeDonationId { get; set; }
    public string DonationCategory { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public bool Selected { get; set; }
    public decimal CollectAmount { get; set; }
}

public sealed class MemberReceiptDto
{
    public int CommitteeMoneyReceiptId { get; set; }
    public int CommitteeMoneyReceiptSn { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? PaidDate { get; set; }
}

public sealed class CollectDonationRequest
{
    public int CommitteeMemberId { get; set; }
    public int AccountId { get; set; }
    public DateTime? PaidDate { get; set; }
    public List<CollectLineRequest> Lines { get; set; } = [];
}

public sealed class CollectLineRequest
{
    public int CommitteeDonationId { get; set; }
    public decimal PaidAmount { get; set; }
}

public sealed class PaymentRecordListDto
{
    public decimal Total { get; set; }
    public List<PaymentRecordRowDto> Rows { get; set; } = [];
}

public sealed class PaymentRecordRowDto
{
    public int CommitteeMoneyReceiptId { get; set; }
    public int CommitteeMoneyReceiptSn { get; set; }
    public string MemberName { get; set; } = "";
    public string MemberType { get; set; } = "";
    public string SmsNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string Details { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateTime? PaidDate { get; set; }
}

public sealed class UnpaidReceiptDto
{
    public bool Found { get; set; }
    public int CommitteeMoneyReceiptId { get; set; }
    public int CommitteeMoneyReceiptSn { get; set; }
    public string MemberName { get; set; } = "";
    public string MemberType { get; set; } = "";
    public string SmsNumber { get; set; } = "";
    public string Address { get; set; } = "";
    public decimal TotalDonation { get; set; }
    public decimal PaidDonation { get; set; }
    public string AccountName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string ReceivedBy { get; set; } = "";
    public List<ReceiptLineDto> Lines { get; set; } = [];
}

public sealed class ReceiptLineDto
{
    public string DonationCategory { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
}

public sealed class UnpaidReceiptRequest
{
    public string Sn { get; set; } = "";
}

public sealed class DonationReceiptDto
{
    public int CommitteeMoneyReceiptId { get; set; }
    public int CommitteeMoneyReceiptSn { get; set; }
    public string MemberName { get; set; } = "";
    public string MemberType { get; set; } = "";
    public string SmsNumber { get; set; } = "";
    public string Address { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string EducationYear { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string ReceivedBy { get; set; } = "";
    public List<ReceiptLineDto> Lines { get; set; } = [];
    public List<ReceiptLineDto> CurrentDues { get; set; } = [];
}
