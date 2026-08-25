namespace Sikkhaloy.Shared.Invoice;

public sealed class InvoiceResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string? CheckoutUrl { get; set; }
    public bool LocalMode { get; set; }
}

public sealed class SubscriptionStatusDto
{
    public bool HasDue { get; set; }
    public bool IsBlocked { get; set; }
    public bool InGrace { get; set; }
    public int DaysUntilExpiry { get; set; } = -1;
    public int DueCount { get; set; }
    public decimal Due { get; set; }
}

public sealed class DueInvoiceLineDto
{
    public string InvoiceCategory { get; set; } = "";
    public string InvoiceFor { get; set; } = "";
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
}

public sealed class DueInvoiceDto
{
    public bool HasDue { get; set; }
    public bool LocalMode { get; set; }
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal GrandTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public decimal GatewayCharge { get; set; }
    public decimal TotalPayable { get; set; }
    public bool IsBlocked { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool ShowDiscount { get; set; }
    public bool ShowPaid { get; set; }
    public List<DueInvoiceLineDto> Lines { get; set; } = [];
}

public sealed class PaidInvoiceRowDto
{
    public int InvoiceReceiptId { get; set; }
    public int InvoiceReceiptSn { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal GatewayCharge { get; set; }
    public decimal CustomerPaid { get; set; }
    public string PaymentBy { get; set; } = "";
    public string PaidByUser { get; set; } = "";
    public string CollectedBy { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public DateTime? PaidDate { get; set; }
}

public sealed class PaidInvoiceListDto
{
    public List<PaidInvoiceRowDto> Rows { get; set; } = [];
}

public sealed class PaidInvoiceLineDto
{
    public string InvoiceCategory { get; set; } = "";
    public string InvoiceFor { get; set; } = "";
    public decimal Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
}

public sealed class PaidInvoiceReceiptDto
{
    public bool Found { get; set; }
    public int InvoiceReceiptId { get; set; }
    public int InvoiceReceiptSn { get; set; }
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string PaymentBy { get; set; } = "";
    public string CollectedBy { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public DateTime? PaidDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }
    public decimal GatewayCharge { get; set; }
    public decimal CustomerPaid { get; set; }
    public bool HasGatewayCharge { get; set; }
    public bool ShowDiscount { get; set; }
    public bool ShowPaid { get; set; }
    public bool ShowDue { get; set; }
    public List<PaidInvoiceLineDto> Lines { get; set; } = [];
}
