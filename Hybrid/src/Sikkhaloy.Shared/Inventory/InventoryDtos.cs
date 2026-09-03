using System.Text.Json.Serialization;

namespace Sikkhaloy.Shared.Inventory;

public sealed class InventoryResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int Id { get; set; }
    public bool Queued { get; set; }
}

public sealed class InventoryLookupsDto
{
    public List<InventoryCategoryDto> Categories { get; set; } = [];
    public List<InventoryItemDto> Items { get; set; } = [];
    public List<InventoryAccountDto> Accounts { get; set; } = [];
    public List<InventorySupplierDto> Suppliers { get; set; } = [];
}

public sealed class InventoryAccountDto
{
    public int AccountID { get; set; }
    public string AccountName { get; set; } = "";
    public decimal Balance { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class InventoryCategoryDto
{
    public int ItemCategoryID { get; set; }
    public string Name { get; set; } = "";
    public int ItemCount { get; set; }
}

public sealed class SaveInventoryCategoryRequest
{
    public int ItemCategoryID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class InventoryItemDto
{
    public int ItemID { get; set; }
    public int ItemCategoryID { get; set; }
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "pcs";
    public string? Sku { get; set; }
    [JsonPropertyName("minStock")]
    public decimal MinStock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Purchased { get; set; }
    public decimal Sold { get; set; }
    public decimal Stock { get; set; }
}

public sealed class SaveInventoryItemRequest
{
    public int ItemID { get; set; }
    public int ItemCategoryID { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "pcs";
    public string? Sku { get; set; }
    [JsonPropertyName("minStock")]
    public decimal MinStock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class InventoryLineDto
{
    public int ItemID { get; set; }
    public string ItemName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public sealed class InventoryDocDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string InvoiceNo { get; set; } = "";
    public string Party { get; set; } = "";
    public string? Note { get; set; }
    public int AccountID { get; set; }
    public string AccountName { get; set; } = "";
    public decimal Total { get; set; }
    public int LinkedAccountId { get; set; }
    public int SupplierID { get; set; }
    public int CustomerID { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string? UserName { get; set; }
    public List<InventoryLineDto> Lines { get; set; } = [];
}

public sealed class SaveInventoryDocRequest
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? InvoiceNo { get; set; }
    public string? Party { get; set; }
    public string? Note { get; set; }
    public int AccountID { get; set; }
    public int SupplierID { get; set; }
    public int CustomerID { get; set; }
    public decimal PayNow { get; set; } = -1;
    public bool SendSms { get; set; }
    public List<InventoryLineDto> Lines { get; set; } = [];
}

public sealed class InventoryDocListDto
{
    public decimal Total { get; set; }
    public int Count { get; set; }
    public List<InventoryDocDto> Items { get; set; } = [];
}

public sealed class InventoryStockRowDto
{
    public int ItemID { get; set; }
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Purchased { get; set; }
    public decimal Sold { get; set; }
    public decimal Stock { get; set; }
    public decimal MinStock { get; set; }
    public decimal AvgCost { get; set; }
    public decimal StockValue { get; set; }
    public decimal SalePrice { get; set; }
    public bool LowStock { get; set; }
}

public sealed class InventoryStockDto
{
    public int ItemCount { get; set; }
    public int LowCount { get; set; }
    public decimal TotalStock { get; set; }
    public decimal StockValue { get; set; }
    public decimal PurchaseTotal { get; set; }
    public decimal SaleTotal { get; set; }
    public List<InventoryStockRowDto> Rows { get; set; } = [];
}

public sealed class InventorySupplierDto
{
    public int SupplierID { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal Purchased { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
}

public sealed class SaveInventorySupplierRequest
{
    public int SupplierID { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public sealed class InventorySupplierDueDto
{
    public int PurchaseID { get; set; }
    public string InvoiceNo { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
}

public sealed class InventorySupplierLedgerDto
{
    public InventorySupplierDto Supplier { get; set; } = new();
    public List<InventorySupplierDueDto> Dues { get; set; } = [];
    public List<InventorySupplierPaymentDto> Payments { get; set; } = [];
}

public sealed class InventorySupplierPaymentDto
{
    public int PaymentID { get; set; }
    public int SupplierID { get; set; }
    public int PurchaseID { get; set; }
    public int AccountID { get; set; }
    public string AccountName { get; set; } = "";
    public string InvoiceNo { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public string? UserName { get; set; }
}

public sealed class SaveInventorySupplierPaymentRequest
{
    public int SupplierID { get; set; }
    public int PurchaseID { get; set; }
    public int AccountID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
}

public sealed class InventoryCustomerDto
{
    public int CustomerID { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public int StudentID { get; set; }
    public string? StudentCode { get; set; }
    public string? ClassName { get; set; }
    public decimal Due { get; set; }
}

public sealed class SaveInventoryCustomerRequest
{
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
}

public sealed class InventoryStudentHitDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
}
