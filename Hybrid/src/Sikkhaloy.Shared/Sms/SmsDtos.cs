namespace Sikkhaloy.Shared.Sms;

public sealed class SmsResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Balance { get; set; }
    public int Count { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? Message { get; set; }
    public bool LocalMode { get; set; }
}

public sealed class SmsBalanceDto
{
    public int Balance { get; set; }
    public bool LocalMode { get; set; }
}

public sealed class SmsStudentDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string? RollNo { get; set; }
    public string? ClassName { get; set; }
    public string Name { get; set; } = "";
    public string? Gender { get; set; }
    public string? Religion { get; set; }
    public string? Phone { get; set; }
    public bool Selected { get; set; }
}

public sealed class SmsTeacherDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string? Phone { get; set; }
    public bool Selected { get; set; }
}

public sealed class SendOfficeSmsRequest
{
    public string Mode { get; set; } = "selected";
    public string Text { get; set; } = "";
    public List<int> StudentIds { get; set; } = [];
    public List<int> TeacherIds { get; set; } = [];
    public List<int> ContactIds { get; set; } = [];
    public List<int> CommitteeMemberIds { get; set; } = [];
    public string? Phones { get; set; }
}

public sealed class SmsGroupDto
{
    public int SMS_GroupID { get; set; }
    public string GroupName { get; set; } = "";
}

public sealed class SmsContactDto
{
    public int SMS_NumberID { get; set; }
    public int SMS_GroupID { get; set; }
    public string GroupName { get; set; } = "";
    public string Name { get; set; } = "";
    public string MobileNo { get; set; } = "";
    public string? Address { get; set; }
    public DateTime? Add_Date { get; set; }
    public bool Selected { get; set; }
}

public sealed class SaveSmsGroupRequest
{
    public int SMS_GroupID { get; set; }
    public string GroupName { get; set; } = "";
}

public sealed class SaveSmsContactRequest
{
    public int SMS_NumberID { get; set; }
    public int SMS_GroupID { get; set; }
    public string Name { get; set; } = "";
    public string MobileNo { get; set; } = "";
    public string? Address { get; set; }
}

public sealed class SmsRecordDto
{
    public Guid SMS_Send_ID { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public string RecipientCode { get; set; } = "";
    public string TextSMS { get; set; } = "";
    public int TextCount { get; set; }
    public int SMSCount { get; set; }
    public string PurposeOfSMS { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime Date { get; set; }
}

public sealed class SmsRecordsDto
{
    public int Balance { get; set; }
    public int TotalSent { get; set; }
    public int TotalRecipients { get; set; }
    public int DistinctRecipients { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public decimal TotalCost { get; set; }
    public decimal PerSmsRate { get; set; } = 0.36m;
    public int RowCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; } = 1;
    public bool LocalMode { get; set; }
    public List<SmsRecordDto> Rows { get; set; } = [];
}

public sealed class SmsRechargeRowDto
{
    public int SMS_Recharge_RecordID { get; set; }
    public int RechargeSMS { get; set; }
    public decimal PerSMS_Price { get; set; }
    public decimal Total_Price { get; set; }
    public DateTime Date { get; set; }
    public string? UserName { get; set; }
    public bool Is_Paid { get; set; }
}

public sealed class SmsDueInvoiceDto
{
    public string Invoice_SN { get; set; } = "";
    public string Invoice_For { get; set; } = "";
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public DateTime? IssuDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class SmsRechargePageDto
{
    public int Balance { get; set; }
    public decimal PerSmsRate { get; set; } = 0.36m;
    public bool LocalMode { get; set; }
    public List<SmsRechargeRowDto> History { get; set; } = [];
    public List<SmsDueInvoiceDto> DueInvoices { get; set; } = [];
}

public sealed class SmsRechargeRequest
{
    public int Quantity { get; set; }
}

public sealed class SmsTemplateDto
{
    public int TemplateID { get; set; }
    public string TemplateName { get; set; } = "";
    public string TemplateCategory { get; set; } = "";
    public string TemplateType { get; set; } = "";
    public string MessageTemplate { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedDate { get; set; }
}

public sealed class SaveSmsTemplateRequest
{
    public int TemplateID { get; set; }
    public string TemplateName { get; set; } = "";
    public string TemplateCategory { get; set; } = "";
    public string TemplateType { get; set; } = "";
    public string MessageTemplate { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class SmsTemplateResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int TemplateID { get; set; }
}

public sealed class CommitteePaymentSmsLangDto
{
    public string Lang { get; set; } = "bn";
}

public sealed class DonorReceiptSmsRequest
{
    public string? Lang { get; set; }
}
