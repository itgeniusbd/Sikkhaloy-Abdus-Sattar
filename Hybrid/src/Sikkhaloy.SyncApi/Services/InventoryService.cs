using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Inventory;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class InventoryService
{
    public const string PurchaseCategory = "Inventory Purchase";
    public const string SaleCategory = "Inventory Sale";

    private readonly EduConnectionFactory _connections;

    public InventoryService(EduConnectionFactory connections) => _connections = connections;

    public async Task<InventoryLookupsDto> GetLookupsAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var dto = new InventoryLookupsDto
        {
            Categories = await LoadCategoriesAsync(con, session.SchoolID, ct),
            Items = await LoadItemsAsync(con, session.SchoolID, 0, ct),
            Suppliers = await LoadSuppliersAsync(con, session.SchoolID, ct)
        };
        await using (var cmd = new SqlCommand("""
SELECT AccountID, AccountName, ISNULL(AccountBalance, 0) AS AccountBalance, ISNULL(Default_Status, N'') AS Default_Status
FROM dbo.Account WHERE SchoolID = @SchoolID
ORDER BY CASE WHEN Default_Status = N'True' THEN 0 ELSE 1 END, AccountName
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Accounts.Add(new InventoryAccountDto
                {
                    AccountID = I(reader["AccountID"]),
                    AccountName = S(reader["AccountName"]),
                    Balance = Dec(reader["AccountBalance"]),
                    IsDefault = string.Equals(S(reader["Default_Status"]), "True", StringComparison.OrdinalIgnoreCase)
                });
            }
        }
        return dto;
    }

    public async Task<IReadOnlyList<InventoryCategoryDto>> ListCategoriesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        return await LoadCategoriesAsync(con, session.SchoolID, ct);
    }

    public async Task<InventoryResult> SaveCategoryAsync(SessionSnapshot session, SaveInventoryCategoryRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("inv.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        if (request!.ItemCategoryID > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Inv_ItemCategory SET Name = @Name
WHERE ItemCategoryID = @ID AND SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@ID", request.ItemCategoryID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(ct);
            if (n <= 0) return Fail("inv.empty");
            return Ok(request.ItemCategoryID, "inv.categoryUpdated");
        }

        await using (var dup = new SqlCommand("""
SELECT COUNT(1) FROM dbo.Inv_ItemCategory WHERE SchoolID = @SchoolID AND Name = @Name
""", con))
        {
            dup.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            dup.Parameters.AddWithValue("@Name", name);
            if (I(await dup.ExecuteScalarAsync(ct)) > 0)
                return Fail("inv.categoryExists");
        }

        await using var ins = new SqlCommand("""
INSERT INTO dbo.Inv_ItemCategory (SchoolID, Name) VALUES (@SchoolID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@Name", name);
        var id = I(await ins.ExecuteScalarAsync(ct));
        return Ok(id, "inv.categoryAdded");
    }

    public async Task<InventoryResult> DeleteCategoryAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("inv.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        await using (var used = new SqlCommand("""
SELECT COUNT(1) FROM dbo.Inv_Item WHERE ItemCategoryID = @ID AND SchoolID = @SchoolID
""", con))
        {
            used.Parameters.AddWithValue("@ID", id);
            used.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            if (I(await used.ExecuteScalarAsync(ct)) > 0)
                return Fail("inv.categoryUsed");
        }

        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Inv_ItemCategory WHERE ItemCategoryID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n <= 0) return Fail("inv.empty");
        return Ok(id, "inv.deleted");
    }

    public async Task<IReadOnlyList<InventoryItemDto>> ListItemsAsync(SessionSnapshot session, int categoryId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        return await LoadItemsAsync(con, session.SchoolID, categoryId, ct);
    }

    public async Task<InventoryResult> SaveItemAsync(SessionSnapshot session, SaveInventoryItemRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (request is null || request.ItemCategoryID <= 0 || name.Length == 0)
            return Fail("inv.needItem");
        var unit = string.IsNullOrWhiteSpace(request.Unit) ? "pcs" : request.Unit.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        await using (var cat = new SqlCommand("""
SELECT COUNT(1) FROM dbo.Inv_ItemCategory WHERE ItemCategoryID = @ID AND SchoolID = @SchoolID
""", con))
        {
            cat.Parameters.AddWithValue("@ID", request.ItemCategoryID);
            cat.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            if (I(await cat.ExecuteScalarAsync(ct)) <= 0)
                return Fail("inv.needCategory");
        }

        if (request.ItemID > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Inv_Item
SET ItemCategoryID = @Cat, Name = @Name, Unit = @Unit, Sku = @Sku, MinStock = @MinStock,
    PurchasePrice = @Purchase, SalePrice = @Sale, IsActive = @Active
WHERE ItemID = @ID AND SchoolID = @SchoolID
""", con);
            BindItem(cmd, session, request, name, unit);
            cmd.Parameters.AddWithValue("@ID", request.ItemID);
            var n = await cmd.ExecuteNonQueryAsync(ct);
            if (n <= 0) return Fail("inv.empty");
            return Ok(request.ItemID, "inv.itemUpdated");
        }

        await using var ins = new SqlCommand("""
INSERT INTO dbo.Inv_Item
    (SchoolID, ItemCategoryID, Name, Unit, Sku, MinStock, PurchasePrice, SalePrice, IsActive)
VALUES
    (@SchoolID, @Cat, @Name, @Unit, @Sku, @MinStock, @Purchase, @Sale, @Active);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        BindItem(ins, session, request, name, unit);
        var id = I(await ins.ExecuteScalarAsync(ct));
        return Ok(id, "inv.itemAdded");
    }

    public async Task<InventoryResult> DeleteItemAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("inv.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        await using (var used = new SqlCommand("""
SELECT
  (SELECT COUNT(1) FROM dbo.Inv_PurchaseLine WHERE ItemID = @ID)
+ (SELECT COUNT(1) FROM dbo.Inv_SaleLine WHERE ItemID = @ID)
""", con))
        {
            used.Parameters.AddWithValue("@ID", id);
            if (I(await used.ExecuteScalarAsync(ct)) > 0)
                return Fail("inv.itemUsed");
        }

        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Inv_Item WHERE ItemID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n <= 0) return Fail("inv.empty");
        return Ok(id, "inv.deleted");
    }

    public Task<InventoryDocListDto> ListPurchasesAsync(SessionSnapshot session, DateTime? from, DateTime? to, int itemId, CancellationToken ct) =>
        ListDocsAsync(session, isPurchase: true, from, to, itemId, ct);

    public Task<InventoryDocDto?> GetPurchaseAsync(SessionSnapshot session, int id, CancellationToken ct) =>
        GetDocAsync(session, isPurchase: true, id, ct);

    public Task<InventoryResult> SavePurchaseAsync(SessionSnapshot session, SaveInventoryDocRequest? request, CancellationToken ct) =>
        SaveDocAsync(session, isPurchase: true, request, ct);

    public Task<InventoryResult> DeletePurchaseAsync(SessionSnapshot session, int id, CancellationToken ct) =>
        DeleteDocAsync(session, isPurchase: true, id, ct);

    public Task<InventoryDocListDto> ListSalesAsync(SessionSnapshot session, DateTime? from, DateTime? to, int itemId, CancellationToken ct) =>
        ListDocsAsync(session, isPurchase: false, from, to, itemId, ct);

    public Task<InventoryDocDto?> GetSaleAsync(SessionSnapshot session, int id, CancellationToken ct) =>
        GetDocAsync(session, isPurchase: false, id, ct);

    public Task<InventoryResult> SaveSaleAsync(SessionSnapshot session, SaveInventoryDocRequest? request, CancellationToken ct) =>
        SaveDocAsync(session, isPurchase: false, request, ct);

    public Task<InventoryResult> DeleteSaleAsync(SessionSnapshot session, int id, CancellationToken ct) =>
        DeleteDocAsync(session, isPurchase: false, id, ct);

    public async Task<InventoryStockDto> GetStockAsync(SessionSnapshot session, int categoryId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var dto = new InventoryStockDto();
        await using var cmd = new SqlCommand("""
SELECT i.ItemID, c.Name AS Category, i.Name, i.Unit, i.MinStock, i.SalePrice,
       ISNULL(p.Qty, 0) AS Purchased, ISNULL(s.Qty, 0) AS Sold,
       ISNULL(p.Amount, 0) AS PurchaseAmount, ISNULL(sa.Amount, 0) AS SaleAmount
FROM dbo.Inv_Item AS i
INNER JOIN dbo.Inv_ItemCategory AS c ON c.ItemCategoryID = i.ItemCategoryID
LEFT JOIN (
    SELECT l.ItemID, SUM(l.Qty) AS Qty, SUM(l.Amount) AS Amount
    FROM dbo.Inv_PurchaseLine AS l
    INNER JOIN dbo.Inv_Purchase AS d ON d.PurchaseID = l.PurchaseID
    WHERE d.SchoolID = @SchoolID
    GROUP BY l.ItemID
) AS p ON p.ItemID = i.ItemID
LEFT JOIN (
    SELECT l.ItemID, SUM(l.Qty) AS Qty
    FROM dbo.Inv_SaleLine AS l
    INNER JOIN dbo.Inv_Sale AS d ON d.SaleID = l.SaleID
    WHERE d.SchoolID = @SchoolID
    GROUP BY l.ItemID
) AS s ON s.ItemID = i.ItemID
LEFT JOIN (
    SELECT l.ItemID, SUM(l.Amount) AS Amount
    FROM dbo.Inv_SaleLine AS l
    INNER JOIN dbo.Inv_Sale AS d ON d.SaleID = l.SaleID
    WHERE d.SchoolID = @SchoolID
    GROUP BY l.ItemID
) AS sa ON sa.ItemID = i.ItemID
WHERE i.SchoolID = @SchoolID AND (@Cat = 0 OR i.ItemCategoryID = @Cat)
ORDER BY c.Name, i.Name
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Cat", categoryId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var purchased = Dec(reader["Purchased"]);
            var sold = Dec(reader["Sold"]);
            var stock = purchased - sold;
            var purchaseAmount = Dec(reader["PurchaseAmount"]);
            var avg = purchased > 0 ? Math.Round(purchaseAmount / purchased, 2) : 0;
            var min = Dec(reader["MinStock"]);
            var row = new InventoryStockRowDto
            {
                ItemID = I(reader["ItemID"]),
                Category = S(reader["Category"]),
                Name = S(reader["Name"]),
                Unit = S(reader["Unit"]),
                Purchased = purchased,
                Sold = sold,
                Stock = stock,
                MinStock = min,
                AvgCost = avg,
                StockValue = Math.Round(avg * stock, 2),
                SalePrice = Dec(reader["SalePrice"]),
                LowStock = stock <= min
            };
            dto.Rows.Add(row);
            dto.ItemCount++;
            if (row.LowStock) dto.LowCount++;
            dto.TotalStock += stock;
            dto.StockValue += row.StockValue;
            dto.PurchaseTotal += purchaseAmount;
            dto.SaleTotal += Dec(reader["SaleAmount"]);
        }
        return dto;
    }

    private async Task<InventoryDocListDto> ListDocsAsync(
        SessionSnapshot session, bool isPurchase, DateTime? from, DateTime? to, int itemId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var table = isPurchase ? "Inv_Purchase" : "Inv_Sale";
        var idCol = isPurchase ? "PurchaseID" : "SaleID";
        var partyCol = isPurchase ? "Supplier" : "Customer";
        var lineTable = isPurchase ? "Inv_PurchaseLine" : "Inv_SaleLine";
        var lineFk = isPurchase ? "PurchaseID" : "SaleID";
        var partySelect = isPurchase
            ? "ISNULL(NULLIF(LTRIM(RTRIM(sup.Name)), N''), ISNULL(d.Supplier, N''))"
            : "ISNULL(NULLIF(LTRIM(RTRIM(cust.Name)), N''), ISNULL(d.Customer, N''))";
        var extra = isPurchase
            ? ", ISNULL(d.SupplierID, 0) AS SupplierID, ISNULL(d.PaidAmount, CASE WHEN ISNULL(d.ExpenseID, 0) > 0 THEN d.Total ELSE 0 END) AS PaidAmount"
            : ", ISNULL(d.CustomerID, 0) AS CustomerID, ISNULL(d.PaidAmount, CASE WHEN ISNULL(d.ExtraIncomeID, 0) > 0 THEN d.Total ELSE 0 END) AS PaidAmount";
        var partyJoin = isPurchase
            ? "LEFT JOIN dbo.Inv_Supplier AS sup ON sup.SupplierID = d.SupplierID"
            : "LEFT JOIN dbo.Inv_Customer AS cust ON cust.CustomerID = d.CustomerID";
        var sql = $"""
SELECT d.{idCol} AS Id, d.DocDate, ISNULL(d.InvoiceNo, N'') AS InvoiceNo, {partySelect} AS Party,
       d.Note, d.AccountID, ISNULL(acc.AccountName, N'') AS AccountName, d.Total{extra},
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS UserName
FROM dbo.{table} AS d
LEFT JOIN dbo.Account AS acc ON acc.AccountID = d.AccountID
LEFT JOIN dbo.Admin AS a ON a.RegistrationID = d.RegistrationID
{partyJoin}
WHERE d.SchoolID = @SchoolID AND d.EducationYearID = @YearID
  AND d.DocDate >= @From AND d.DocDate <= @To
  AND (@ItemID = 0 OR EXISTS (
        SELECT 1 FROM dbo.{lineTable} AS l WHERE l.{lineFk} = d.{idCol} AND l.ItemID = @ItemID))
ORDER BY d.DocDate DESC, d.{idCol} DESC
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@From", (from ?? DateTime.Today.AddMonths(-1)).Date);
        cmd.Parameters.AddWithValue("@To", (to ?? DateTime.Today).Date);
        cmd.Parameters.AddWithValue("@ItemID", itemId);
        var dto = new InventoryDocListDto();
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var row = ReadDoc(reader);
                dto.Items.Add(row);
                dto.Total += row.Total;
            }
        }
        dto.Count = dto.Items.Count;
        if (dto.Items.Count > 0)
            await FillDocLinesAsync(con, dto.Items, lineTable, lineFk, ct);
        return dto;
    }

    private static async Task FillDocLinesAsync(
        SqlConnection con, List<InventoryDocDto> docs, string lineTable, string lineFk, CancellationToken ct)
    {
        var ids = docs.Select(x => x.Id).Distinct().ToList();
        if (ids.Count == 0) return;
        var map = docs.ToLookup(x => x.Id);
        var names = string.Join(",", ids.Select((_, i) => $"@L{i}"));
        await using var cmd = new SqlCommand($"""
SELECT l.{lineFk} AS DocID, l.ItemID, i.Name AS ItemName, i.Unit, l.Qty, l.UnitPrice, l.Amount
FROM dbo.{lineTable} AS l
INNER JOIN dbo.Inv_Item AS i ON i.ItemID = l.ItemID
WHERE l.{lineFk} IN ({names})
ORDER BY l.{lineFk}, i.Name
""", con);
        for (var i = 0; i < ids.Count; i++)
            cmd.Parameters.AddWithValue($"@L{i}", ids[i]);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var docId = I(reader["DocID"]);
            var line = new InventoryLineDto
            {
                ItemID = I(reader["ItemID"]),
                ItemName = S(reader["ItemName"]),
                Unit = S(reader["Unit"]),
                Qty = Dec(reader["Qty"]),
                UnitPrice = Dec(reader["UnitPrice"]),
                Amount = Dec(reader["Amount"])
            };
            foreach (var doc in map[docId])
                doc.Lines.Add(line);
        }
    }

    private async Task<InventoryDocDto?> GetDocAsync(SessionSnapshot session, bool isPurchase, int id, CancellationToken ct)
    {
        if (id <= 0) return null;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var table = isPurchase ? "Inv_Purchase" : "Inv_Sale";
        var idCol = isPurchase ? "PurchaseID" : "SaleID";
        var partyCol = isPurchase ? "Supplier" : "Customer";
        var linkCol = isPurchase ? "ExpenseID" : "ExtraIncomeID";
        var lineTable = isPurchase ? "Inv_PurchaseLine" : "Inv_SaleLine";
        var lineFk = isPurchase ? "PurchaseID" : "SaleID";
        InventoryDocDto? doc = null;
        var partySelect = isPurchase
            ? "ISNULL(NULLIF(LTRIM(RTRIM(sup.Name)), N''), ISNULL(d.Supplier, N''))"
            : "ISNULL(NULLIF(LTRIM(RTRIM(cust.Name)), N''), ISNULL(d.Customer, N''))";
        var extra = isPurchase
            ? ", ISNULL(d.SupplierID, 0) AS SupplierID, ISNULL(d.PaidAmount, CASE WHEN ISNULL(d.ExpenseID, 0) > 0 THEN d.Total ELSE 0 END) AS PaidAmount"
            : ", ISNULL(d.CustomerID, 0) AS CustomerID, ISNULL(d.PaidAmount, CASE WHEN ISNULL(d.ExtraIncomeID, 0) > 0 THEN d.Total ELSE 0 END) AS PaidAmount";
        var partyJoin = isPurchase
            ? "LEFT JOIN dbo.Inv_Supplier AS sup ON sup.SupplierID = d.SupplierID"
            : "LEFT JOIN dbo.Inv_Customer AS cust ON cust.CustomerID = d.CustomerID";
        await using (var cmd = new SqlCommand($"""
SELECT d.{idCol} AS Id, d.DocDate, ISNULL(d.InvoiceNo, N'') AS InvoiceNo, {partySelect} AS Party,
       d.Note, d.AccountID, ISNULL(acc.AccountName, N'') AS AccountName, d.Total{extra},
       ISNULL(d.{linkCol}, 0) AS LinkedAccountId,
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS UserName
FROM dbo.{table} AS d
LEFT JOIN dbo.Account AS acc ON acc.AccountID = d.AccountID
LEFT JOIN dbo.Admin AS a ON a.RegistrationID = d.RegistrationID
{partyJoin}
WHERE d.{idCol} = @ID AND d.SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                doc = ReadDoc(reader);
        }
        if (doc is null) return null;
        await using (var lines = new SqlCommand($"""
SELECT l.ItemID, i.Name AS ItemName, i.Unit, l.Qty, l.UnitPrice, l.Amount
FROM dbo.{lineTable} AS l
INNER JOIN dbo.Inv_Item AS i ON i.ItemID = l.ItemID
WHERE l.{lineFk} = @ID
ORDER BY l.{lineFk}
""", con))
        {
            lines.Parameters.AddWithValue("@ID", id);
            await using var reader = await lines.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                doc.Lines.Add(new InventoryLineDto
                {
                    ItemID = I(reader["ItemID"]),
                    ItemName = S(reader["ItemName"]),
                    Unit = S(reader["Unit"]),
                    Qty = Dec(reader["Qty"]),
                    UnitPrice = Dec(reader["UnitPrice"]),
                    Amount = Dec(reader["Amount"])
                });
            }
        }
        return doc;
    }

    private async Task<InventoryResult> SaveDocAsync(
        SessionSnapshot session, bool isPurchase, SaveInventoryDocRequest? request, CancellationToken ct)
    {
        var check = ValidateDoc(request, isPurchase);
        if (check is not null) return check;
        var paid = request!.Date == default ? DateTime.Today : request.Date.Date;
        if (paid > DateTime.Today) return Fail("acc.futureDate");
        NormalizeLines(request.Lines);
        if (request.Lines.Count == 0) return Fail("inv.needLines");
        var total = request.Lines.Sum(x => x.Amount);
        var payNow = request.Id > 0
            ? -1
            : (request.PayNow < 0 ? total : Math.Clamp(request.PayNow, 0, total));
        if (request.Id == 0 && payNow > 0 && request.AccountID <= 0)
            return Fail("acc.needPay");
        if (!isPurchase && request.Id == 0 && payNow < total && request.CustomerID <= 0)
            return Fail("inv.needCustomer");
        if (isPurchase && request.Id == 0 && payNow < total && request.SupplierID <= 0)
            return Fail("inv.needSupplier");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        if (isPurchase && request.SupplierID > 0)
        {
            var supplierName = await GetSupplierNameAsync(con, session.SchoolID, request.SupplierID, ct);
            if (string.IsNullOrWhiteSpace(supplierName))
                return Fail("inv.needSupplier");
            request.Party = supplierName;
        }
        else if (request.CustomerID > 0)
        {
            var customerName = await GetCustomerNameAsync(con, session.SchoolID, request.CustomerID, ct);
            if (!string.IsNullOrWhiteSpace(customerName))
                request.Party = customerName;
            if (!isPurchase && request.Id == 0 && payNow < total)
            {
                var studentId = await GetCustomerStudentIdAsync(con, session.SchoolID, request.CustomerID, ct);
                if (studentId <= 0) return Fail("inv.cashOnlyCustomer");
            }
        }
        var cats = await EnsureAccountCategoriesAsync(con, session, ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await SetContextAsync(con, tx, session.RegistrationID, ct);
            var cash = payNow;
            if (isPurchase)
            {
                var stockErr = await EnsurePurchaseStockAsync(con, tx, session.SchoolID, request.Id, request.Lines, ct);
                if (stockErr is not null) { await tx.RollbackAsync(ct); return stockErr; }
                if (request.Id > 0)
                    cash = await ScalarDecAsync(con, tx, """
SELECT ISNULL(PaidAmount, 0) FROM dbo.Inv_Purchase WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", request.Id); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
                if (request.Id > 0 && total < cash)
                {
                    await tx.RollbackAsync(ct);
                    return Fail("inv.paidExceeds");
                }
                if (request.Id == 0 && cash > 0)
                {
                    var balErr = await EnsurePurchaseBalanceAsync(con, tx, session.SchoolID, request.AccountID, 0, cash, ct);
                    if (balErr is not null) { await tx.RollbackAsync(ct); return balErr; }
                }
            }
            else
            {
                var stockErr = await EnsureSaleStockAsync(con, tx, session.SchoolID, request.Id, request.Lines, ct);
                if (stockErr is not null) { await tx.RollbackAsync(ct); return stockErr; }
                if (request.Id > 0)
                    cash = await ScalarDecAsync(con, tx, """
SELECT ISNULL(PaidAmount, 0) FROM dbo.Inv_Sale WHERE SaleID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", request.Id); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
            }

            var id = request.Id;
            if (id > 0)
            {
                var exists = await ScalarAsync(con, tx, isPurchase
                    ? "SELECT COUNT(1) FROM dbo.Inv_Purchase WHERE PurchaseID = @ID AND SchoolID = @SchoolID"
                    : "SELECT COUNT(1) FROM dbo.Inv_Sale WHERE SaleID = @ID AND SchoolID = @SchoolID",
                    p => { p.AddWithValue("@ID", id); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
                if (exists <= 0) { await tx.RollbackAsync(ct); return Fail("inv.empty"); }

                await ExecAsync(con, tx, isPurchase
                    ? """
UPDATE dbo.Inv_Purchase
SET DocDate = @Date, InvoiceNo = @Invoice, Supplier = @Party, Note = @Note, AccountID = @AccountID,
    Total = @Total, SupplierID = @SupplierID
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
"""
                    : """
UPDATE dbo.Inv_Sale
SET DocDate = @Date, InvoiceNo = @Invoice, Customer = @Party, Note = @Note, AccountID = @AccountID,
    Total = @Total, CustomerID = @CustomerID
WHERE SaleID = @ID AND SchoolID = @SchoolID
""", p => BindDoc(p, session, request, paid, total, id), ct);

                await ExecAsync(con, tx, isPurchase
                    ? "DELETE FROM dbo.Inv_PurchaseLine WHERE PurchaseID = @ID"
                    : "DELETE FROM dbo.Inv_SaleLine WHERE SaleID = @ID",
                    p => p.AddWithValue("@ID", id), ct);
            }
            else
            {
                id = await ScalarAsync(con, tx, isPurchase
                    ? """
INSERT INTO dbo.Inv_Purchase
    (SchoolID, EducationYearID, RegistrationID, AccountID, DocDate, InvoiceNo, Supplier, Note, Total, SupplierID, PaidAmount)
VALUES
    (@SchoolID, @YearID, @RegistrationID, @AccountID, @Date, @Invoice, @Party, @Note, @Total, @SupplierID, @Paid);
SELECT CAST(SCOPE_IDENTITY() AS INT);
"""
                    : """
INSERT INTO dbo.Inv_Sale
    (SchoolID, EducationYearID, RegistrationID, AccountID, DocDate, InvoiceNo, Customer, Note, Total, CustomerID, PaidAmount)
VALUES
    (@SchoolID, @YearID, @RegistrationID, @AccountID, @Date, @Invoice, @Party, @Note, @Total, @CustomerID, @Paid);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p => BindDoc(p, session, request, paid, total, 0, payNow), ct);
            }

            foreach (var line in request.Lines)
            {
                await ExecAsync(con, tx, isPurchase
                    ? """
INSERT INTO dbo.Inv_PurchaseLine (PurchaseID, ItemID, Qty, UnitPrice, Amount)
VALUES (@DocID, @ItemID, @Qty, @Price, @Amount)
"""
                    : """
INSERT INTO dbo.Inv_SaleLine (SaleID, ItemID, Qty, UnitPrice, Amount)
VALUES (@DocID, @ItemID, @Qty, @Price, @Amount)
""", p =>
                {
                    p.AddWithValue("@DocID", id);
                    p.AddWithValue("@ItemID", line.ItemID);
                    p.AddWithValue("@Qty", line.Qty);
                    p.AddWithValue("@Price", line.UnitPrice);
                    p.AddWithValue("@Amount", line.Amount);
                }, ct);

                if (isPurchase && line.UnitPrice > 0)
                {
                    await ExecAsync(con, tx, """
UPDATE dbo.Inv_Item SET PurchasePrice = @Price
WHERE ItemID = @ItemID AND SchoolID = @SchoolID
""", p =>
                    {
                        p.AddWithValue("@Price", line.UnitPrice);
                        p.AddWithValue("@ItemID", line.ItemID);
                        p.AddWithValue("@SchoolID", session.SchoolID);
                    }, ct);
                }
                else if (!isPurchase && line.UnitPrice > 0)
                {
                    await ExecAsync(con, tx, """
UPDATE dbo.Inv_Item SET SalePrice = @Price
WHERE ItemID = @ItemID AND SchoolID = @SchoolID
""", p =>
                    {
                        p.AddWithValue("@Price", line.UnitPrice);
                        p.AddWithValue("@ItemID", line.ItemID);
                        p.AddWithValue("@SchoolID", session.SchoolID);
                    }, ct);
                }
            }

            var invoice = string.IsNullOrWhiteSpace(request.InvoiceNo)
                ? (isPurchase ? $"P-{id}" : $"S-{id}")
                : request.InvoiceNo!.Trim();
            if (string.IsNullOrWhiteSpace(request.InvoiceNo))
            {
                await ExecAsync(con, tx, isPurchase
                    ? "UPDATE dbo.Inv_Purchase SET InvoiceNo = @Invoice WHERE PurchaseID = @ID"
                    : "UPDATE dbo.Inv_Sale SET InvoiceNo = @Invoice WHERE SaleID = @ID",
                    p => { p.AddWithValue("@Invoice", invoice); p.AddWithValue("@ID", id); }, ct);
            }

            var details = BuildAccountDetails(isPurchase, invoice, request.Party, request.Lines, total);
            var cashAmount = request.Id > 0 ? cash : payNow;
            var linked = 0;
            if (cashAmount > 0)
            {
                linked = await SyncAccountsAsync(con, tx, session, isPurchase, id, request.AccountID, paid, cashAmount, details, cats, ct);
            }
            else
            {
                var oldLinked = await ScalarAsync(con, tx, isPurchase
                    ? "SELECT ISNULL(ExpenseID, 0) FROM dbo.Inv_Purchase WHERE PurchaseID = @ID"
                    : "SELECT ISNULL(ExtraIncomeID, 0) FROM dbo.Inv_Sale WHERE SaleID = @ID",
                    p => p.AddWithValue("@ID", id), ct);
                if (oldLinked > 0)
                {
                    await ExecAsync(con, tx, isPurchase
                        ? "DELETE FROM dbo.Expenditure WHERE ExpenseID = @ID AND SchoolID = @SchoolID"
                        : "DELETE FROM dbo.Extra_Income WHERE Extra_IncomeID = @ID AND SchoolID = @SchoolID",
                        p => { p.AddWithValue("@ID", oldLinked); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
                }
            }
            await ExecAsync(con, tx, isPurchase
                ? "UPDATE dbo.Inv_Purchase SET ExpenseID = @Linked WHERE PurchaseID = @ID"
                : "UPDATE dbo.Inv_Sale SET ExtraIncomeID = @Linked WHERE SaleID = @ID",
                p => { p.AddWithValue("@Linked", linked > 0 ? (object)linked : DBNull.Value); p.AddWithValue("@ID", id); }, ct);

            if (!isPurchase && request.Id == 0 && payNow < total)
            {
                var feeErr = await AttachSaleFeePayOrderAsync(
                    con, tx, session, id, request.CustomerID, total - payNow, invoice, ct);
                if (feeErr is not null)
                {
                    await tx.RollbackAsync(ct);
                    return feeErr;
                }
            }
            await tx.CommitAsync(ct);
            return Ok(id, request.Id > 0
                ? (isPurchase ? "inv.purchaseUpdated" : "inv.saleUpdated")
                : (isPurchase ? "inv.purchaseSaved" : "inv.saleSaved"));
        }
        catch (SqlException)
        {
            await tx.RollbackAsync(ct);
            return Fail(isPurchase ? "acc.overBalance" : "inv.failed");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<InventoryResult> DeleteDocAsync(SessionSnapshot session, bool isPurchase, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("inv.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await SetContextAsync(con, tx, session.RegistrationID, ct);
            int linked = 0, feePayOrderId = 0;
            await using (var cmd = new SqlCommand(isPurchase
                ? "SELECT ISNULL(ExpenseID, 0), AccountID, ISNULL(InvoiceNo, N'') FROM dbo.Inv_Purchase WHERE PurchaseID = @ID AND SchoolID = @SchoolID"
                : "SELECT ISNULL(ExtraIncomeID, 0), AccountID, ISNULL(InvoiceNo, N''), ISNULL(FeePayOrderID, 0) FROM dbo.Inv_Sale WHERE SaleID = @ID AND SchoolID = @SchoolID", con, tx))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (!await reader.ReadAsync(ct))
                {
                    await tx.RollbackAsync(ct);
                    return Fail("inv.empty");
                }
                linked = I(reader[0]);
                if (!isPurchase && reader.FieldCount > 3) feePayOrderId = I(reader[3]);
            }

            if (isPurchase)
            {
                var stockErr = await EnsurePurchaseDeleteStockAsync(con, tx, session.SchoolID, id, ct);
                if (stockErr is not null) { await tx.RollbackAsync(ct); return stockErr; }
                await ReverseSupplierPaymentsAsync(con, tx, session.SchoolID, id, ct);
            }

            if (linked > 0)
            {
                await ExecAsync(con, tx, isPurchase
                    ? "DELETE FROM dbo.Expenditure WHERE ExpenseID = @ID AND SchoolID = @SchoolID"
                    : "DELETE FROM dbo.Extra_Income WHERE Extra_IncomeID = @ID AND SchoolID = @SchoolID",
                    p => { p.AddWithValue("@ID", linked); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
            }

            if (!isPurchase && feePayOrderId > 0)
            {
                await ExecAsync(con, tx, """
DELETE FROM dbo.Income_PayOrder
WHERE PayOrderID = @ID AND SchoolID = @SchoolID AND ISNULL(PaidAmount, 0) = 0
""", p => { p.AddWithValue("@ID", feePayOrderId); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
            }

            await ExecAsync(con, tx, isPurchase
                ? "DELETE FROM dbo.Inv_PurchaseLine WHERE PurchaseID = @ID"
                : "DELETE FROM dbo.Inv_SaleLine WHERE SaleID = @ID",
                p => p.AddWithValue("@ID", id), ct);
            await ExecAsync(con, tx, isPurchase
                ? "DELETE FROM dbo.Inv_Purchase WHERE PurchaseID = @ID AND SchoolID = @SchoolID"
                : "DELETE FROM dbo.Inv_Sale WHERE SaleID = @ID AND SchoolID = @SchoolID",
                p => { p.AddWithValue("@ID", id); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);

            await tx.CommitAsync(ct);
            return Ok(id, "inv.deleted");
        }
        catch (SqlException)
        {
            await tx.RollbackAsync(ct);
            return Fail("inv.failed");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static InventoryResult? ValidateDoc(SaveInventoryDocRequest? request, bool isPurchase)
    {
        if (request is null) return Fail("inv.needLines");
        if (request.Lines is null || request.Lines.Count == 0)
            return Fail("inv.needLines");
        if (!isPurchase && request.AccountID <= 0 && request.PayNow != 0)
            return Fail("acc.needPay");
        return null;
    }

    private static void NormalizeLines(List<InventoryLineDto> lines)
    {
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (line.ItemID <= 0 || line.Qty <= 0 || line.UnitPrice < 0)
            {
                lines.RemoveAt(i);
                continue;
            }
            line.Amount = Math.Round(line.Qty * line.UnitPrice, 2);
        }
        var merged = new Dictionary<int, InventoryLineDto>();
        foreach (var line in lines)
        {
            if (merged.TryGetValue(line.ItemID, out var existing))
            {
                existing.Qty += line.Qty;
                existing.Amount = Math.Round(existing.Qty * existing.UnitPrice, 2);
            }
            else
                merged[line.ItemID] = line;
        }
        lines.Clear();
        lines.AddRange(merged.Values);
    }

    private static async Task<InventoryResult?> EnsureSaleStockAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int saleId, List<InventoryLineDto> lines, CancellationToken ct)
    {
        foreach (var line in lines)
        {
            var stock = await ItemStockAsync(con, tx, schoolId, line.ItemID, ct);
            if (saleId > 0)
                stock += await ScalarDecAsync(con, tx, """
SELECT ISNULL(SUM(Qty), 0) FROM dbo.Inv_SaleLine WHERE SaleID = @ID AND ItemID = @ItemID
""", p => { p.AddWithValue("@ID", saleId); p.AddWithValue("@ItemID", line.ItemID); }, ct);
            if (stock < line.Qty)
                return Fail("inv.notEnoughStock");
        }
        return null;
    }

    private static async Task<InventoryResult?> EnsurePurchaseStockAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int purchaseId, List<InventoryLineDto> lines, CancellationToken ct)
    {
        if (purchaseId <= 0) return null;
        foreach (var line in lines)
        {
            var oldQty = await ScalarDecAsync(con, tx, """
SELECT ISNULL(SUM(Qty), 0) FROM dbo.Inv_PurchaseLine WHERE PurchaseID = @ID AND ItemID = @ItemID
""", p => { p.AddWithValue("@ID", purchaseId); p.AddWithValue("@ItemID", line.ItemID); }, ct);
            if (line.Qty >= oldQty) continue;
            var stock = await ItemStockAsync(con, tx, schoolId, line.ItemID, ct);
            if (stock - (oldQty - line.Qty) < 0)
                return Fail("inv.stockLocked");
        }
        return null;
    }

    private static async Task<InventoryResult?> EnsurePurchaseDeleteStockAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int purchaseId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT ItemID, Qty FROM dbo.Inv_PurchaseLine WHERE PurchaseID = @ID
""", con, tx);
        cmd.Parameters.AddWithValue("@ID", purchaseId);
        var lines = new List<(int ItemId, decimal Qty)>();
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
                lines.Add((I(reader["ItemID"]), Dec(reader["Qty"])));
        }
        foreach (var (itemId, qty) in lines)
        {
            var stock = await ItemStockAsync(con, tx, schoolId, itemId, ct);
            if (stock - qty < 0)
                return Fail("inv.stockLocked");
        }
        return null;
    }

    private static async Task<decimal> ItemStockAsync(SqlConnection con, SqlTransaction tx, int schoolId, int itemId, CancellationToken ct) =>
        await ScalarDecAsync(con, tx, """
SELECT ISNULL((
    SELECT SUM(l.Qty) FROM dbo.Inv_PurchaseLine AS l
    INNER JOIN dbo.Inv_Purchase AS d ON d.PurchaseID = l.PurchaseID
    WHERE d.SchoolID = @SchoolID AND l.ItemID = @ItemID), 0)
 - ISNULL((
    SELECT SUM(l.Qty) FROM dbo.Inv_SaleLine AS l
    INNER JOIN dbo.Inv_Sale AS d ON d.SaleID = l.SaleID
    WHERE d.SchoolID = @SchoolID AND l.ItemID = @ItemID), 0)
""", p => { p.AddWithValue("@SchoolID", schoolId); p.AddWithValue("@ItemID", itemId); }, ct);

    private static async Task<InventoryResult?> EnsurePurchaseBalanceAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int accountId, int purchaseId, decimal total, CancellationToken ct)
    {
        var balance = await ScalarDecAsync(con, tx, """
SELECT ISNULL(AccountBalance, 0) FROM dbo.Account WHERE AccountID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", accountId); p.AddWithValue("@SchoolID", schoolId); }, ct);
        var credit = 0m;
        if (purchaseId > 0)
        {
            await using var old = new SqlCommand("""
SELECT ISNULL(Total, 0), AccountID FROM dbo.Inv_Purchase WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", con, tx);
            old.Parameters.AddWithValue("@ID", purchaseId);
            old.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await old.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct) && I(reader[1]) == accountId)
                credit = Dec(reader[0]);
        }
        if (balance + credit < total)
            return Fail("acc.overBalance");
        return null;
    }

    private static async Task<int> SyncAccountsAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, bool isPurchase,
        int docId, int accountId, DateTime date, decimal total, string details,
        (int ExpenseCat, int IncomeCat) cats, CancellationToken ct)
    {
        var linkedCol = isPurchase ? "ExpenseID" : "ExtraIncomeID";
        var table = isPurchase ? "Inv_Purchase" : "Inv_Sale";
        var idCol = isPurchase ? "PurchaseID" : "SaleID";
        var linked = await ScalarAsync(con, tx, $"SELECT ISNULL({linkedCol}, 0) FROM dbo.{table} WHERE {idCol} = @ID",
            p => p.AddWithValue("@ID", docId), ct);

        if (linked > 0)
        {
            if (isPurchase)
            {
                await ExecAsync(con, tx, """
UPDATE dbo.Expenditure
SET Amount = @Amount, ExpenseFor = @Details, ExpenseDate = @Date, AccountID = @AccountID,
    ExpenseCategoryID = @Cat
WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Amount", (double)total);
                    p.AddWithValue("@Details", details);
                    p.AddWithValue("@Date", date);
                    p.AddWithValue("@AccountID", accountId);
                    p.AddWithValue("@Cat", cats.ExpenseCat);
                    p.AddWithValue("@ID", linked);
                    p.AddWithValue("@SchoolID", session.SchoolID);
                }, ct);
            }
            else
            {
                await ExecAsync(con, tx, """
UPDATE dbo.Extra_Income
SET Extra_IncomeAmount = @Amount, Extra_IncomeFor = @Details, Extra_IncomeDate = @Date,
    AccountID = @AccountID, Extra_IncomeCategoryID = @Cat
WHERE Extra_IncomeID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Amount", (double)total);
                    p.AddWithValue("@Details", details);
                    p.AddWithValue("@Date", date);
                    p.AddWithValue("@AccountID", accountId);
                    p.AddWithValue("@Cat", cats.IncomeCat);
                    p.AddWithValue("@ID", linked);
                    p.AddWithValue("@SchoolID", session.SchoolID);
                }, ct);
            }
            return linked;
        }

        if (isPurchase)
        {
            return await ScalarAsync(con, tx, """
INSERT INTO dbo.Expenditure
    (RegistrationID, ExpenseCategoryID, Amount, ExpenseFor, ExpenseDate, SchoolID, EducationYearID, AccountID)
VALUES
    (@RegistrationID, @Cat, @Amount, @Details, @Date, @SchoolID, @YearID, @AccountID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
            {
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@Cat", cats.ExpenseCat);
                p.AddWithValue("@Amount", (double)total);
                p.AddWithValue("@Details", details);
                p.AddWithValue("@Date", date);
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@YearID", session.EducationYearID);
                p.AddWithValue("@AccountID", accountId);
            }, ct);
        }

        return await ScalarAsync(con, tx, """
INSERT INTO dbo.Extra_Income
    (SchoolID, RegistrationID, Extra_IncomeCategoryID, Extra_IncomeAmount, Extra_IncomeFor, AccountID, EducationYearID, Extra_IncomeDate)
VALUES
    (@SchoolID, @RegistrationID, @Cat, @Amount, @Details, @AccountID, @YearID, @Date);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
        {
            p.AddWithValue("@SchoolID", session.SchoolID);
            p.AddWithValue("@RegistrationID", session.RegistrationID);
            p.AddWithValue("@Cat", cats.IncomeCat);
            p.AddWithValue("@Amount", (double)total);
            p.AddWithValue("@Details", details);
            p.AddWithValue("@AccountID", accountId);
            p.AddWithValue("@YearID", session.EducationYearID);
            p.AddWithValue("@Date", date);
        }, ct);
    }

    private static string BuildAccountDetails(bool isPurchase, string invoice, string? party, List<InventoryLineDto> lines, decimal total)
    {
        var kind = isPurchase ? "Inventory Purchase" : "Inventory Sale";
        var who = string.IsNullOrWhiteSpace(party) ? "" : (isPurchase ? $" Supplier: {party.Trim()}" : $" Customer: {party.Trim()}");
        var items = string.Join(", ", lines.Take(6).Select(x =>
            $"{(string.IsNullOrWhiteSpace(x.ItemName) ? "#" + x.ItemID : x.ItemName)} {x.Qty:0.##}"));
        if (lines.Count > 6) items += ", …";
        return $"{kind} {invoice}.{who} Items: {items}. Total {total:0.##} Tk.";
    }

    private async Task<(int ExpenseCat, int IncomeCat)> EnsureAccountCategoriesAsync(
        SqlConnection con, SessionSnapshot session, CancellationToken ct)
    {
        var expense = await ScalarAsync(con, null, """
SELECT TOP 1 ExpenseCategoryID FROM dbo.Expense_CategoryName
WHERE SchoolID = @SchoolID AND CategoryName = @Name
""", p => { p.AddWithValue("@SchoolID", session.SchoolID); p.AddWithValue("@Name", PurchaseCategory); }, ct);
        if (expense <= 0)
        {
            expense = await ScalarAsync(con, null, """
INSERT INTO dbo.Expense_CategoryName (RegistrationID, SchoolID, CategoryName)
VALUES (@RegistrationID, @SchoolID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
            {
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@Name", PurchaseCategory);
            }, ct);
        }

        var income = await ScalarAsync(con, null, """
SELECT TOP 1 Extra_IncomeCategoryID FROM dbo.Extra_IncomeCategory
WHERE SchoolID = @SchoolID AND Extra_Income_CategoryName = @Name
""", p => { p.AddWithValue("@SchoolID", session.SchoolID); p.AddWithValue("@Name", SaleCategory); }, ct);
        if (income <= 0)
        {
            income = await ScalarAsync(con, null, """
INSERT INTO dbo.Extra_IncomeCategory (SchoolID, RegistrationID, Extra_Income_CategoryName)
VALUES (@SchoolID, @RegistrationID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
            {
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@Name", SaleCategory);
            }, ct);
        }
        return (expense, income);
    }

    private static async Task<List<InventoryCategoryDto>> LoadCategoriesAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        var rows = new List<InventoryCategoryDto>();
        await using var cmd = new SqlCommand("""
SELECT c.ItemCategoryID, c.Name, COUNT(i.ItemID) AS ItemCount
FROM dbo.Inv_ItemCategory AS c
LEFT JOIN dbo.Inv_Item AS i ON i.ItemCategoryID = c.ItemCategoryID
WHERE c.SchoolID = @SchoolID
GROUP BY c.ItemCategoryID, c.Name
ORDER BY c.Name
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new InventoryCategoryDto
            {
                ItemCategoryID = I(reader["ItemCategoryID"]),
                Name = S(reader["Name"]),
                ItemCount = I(reader["ItemCount"])
            });
        }
        return rows;
    }

    private static async Task<List<InventoryItemDto>> LoadItemsAsync(SqlConnection con, int schoolId, int categoryId, CancellationToken ct)
    {
        var rows = new List<InventoryItemDto>();
        await using var cmd = new SqlCommand("""
SELECT i.ItemID, i.ItemCategoryID, c.Name AS Category, i.Name, i.Unit, i.Sku, i.MinStock,
       i.PurchasePrice, i.SalePrice, i.IsActive,
       ISNULL(p.Qty, 0) AS Purchased, ISNULL(s.Qty, 0) AS Sold
FROM dbo.Inv_Item AS i
INNER JOIN dbo.Inv_ItemCategory AS c ON c.ItemCategoryID = i.ItemCategoryID
LEFT JOIN (
    SELECT l.ItemID, SUM(l.Qty) AS Qty
    FROM dbo.Inv_PurchaseLine AS l
    INNER JOIN dbo.Inv_Purchase AS d ON d.PurchaseID = l.PurchaseID
    WHERE d.SchoolID = @SchoolID GROUP BY l.ItemID
) AS p ON p.ItemID = i.ItemID
LEFT JOIN (
    SELECT l.ItemID, SUM(l.Qty) AS Qty
    FROM dbo.Inv_SaleLine AS l
    INNER JOIN dbo.Inv_Sale AS d ON d.SaleID = l.SaleID
    WHERE d.SchoolID = @SchoolID GROUP BY l.ItemID
) AS s ON s.ItemID = i.ItemID
WHERE i.SchoolID = @SchoolID AND (@Cat = 0 OR i.ItemCategoryID = @Cat)
ORDER BY c.Name, i.Name
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Cat", categoryId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var purchased = Dec(reader["Purchased"]);
            var sold = Dec(reader["Sold"]);
            rows.Add(new InventoryItemDto
            {
                ItemID = I(reader["ItemID"]),
                ItemCategoryID = I(reader["ItemCategoryID"]),
                Category = S(reader["Category"]),
                Name = S(reader["Name"]),
                Unit = S(reader["Unit"]),
                Sku = NullS(reader["Sku"]),
                MinStock = Dec(reader["MinStock"]),
                PurchasePrice = Dec(reader["PurchasePrice"]),
                SalePrice = Dec(reader["SalePrice"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                Purchased = purchased,
                Sold = sold,
                Stock = purchased - sold
            });
        }
        return rows;
    }

    private static InventoryDocDto ReadDoc(SqlDataReader reader)
    {
        var total = Dec(reader["Total"]);
        var paidAmt = Has(reader, "PaidAmount") ? Dec(reader["PaidAmount"]) : total;
        return new InventoryDocDto
        {
            Id = I(reader["Id"]),
            Date = Convert.ToDateTime(reader["DocDate"]).Date,
            InvoiceNo = S(reader["InvoiceNo"]),
            Party = S(reader["Party"]),
            Note = NullS(reader["Note"]),
            AccountID = I(reader["AccountID"]),
            AccountName = S(reader["AccountName"]),
            Total = total,
            SupplierID = Has(reader, "SupplierID") ? I(reader["SupplierID"]) : 0,
            CustomerID = Has(reader, "CustomerID") ? I(reader["CustomerID"]) : 0,
            PaidAmount = paidAmt,
            DueAmount = Math.Max(0, total - paidAmt),
            LinkedAccountId = Has(reader, "LinkedAccountId") ? I(reader["LinkedAccountId"]) : 0,
            UserName = Has(reader, "UserName") ? S(reader["UserName"]).Trim() : ""
        };
    }

    private static void BindItem(SqlCommand cmd, SessionSnapshot session, SaveInventoryItemRequest request, string name, string unit)
    {
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Cat", request.ItemCategoryID);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Unit", unit);
        cmd.Parameters.AddWithValue("@Sku", (object?)NullIfEmpty(request.Sku) ?? DBNull.Value);
        var min = cmd.Parameters.Add("@MinStock", SqlDbType.Decimal);
        min.Precision = 18;
        min.Scale = 3;
        min.Value = request.MinStock;
        cmd.Parameters.AddWithValue("@Purchase", request.PurchasePrice);
        cmd.Parameters.AddWithValue("@Sale", request.SalePrice);
        cmd.Parameters.AddWithValue("@Active", request.IsActive);
    }

    private static void BindDoc(SqlParameterCollection p, SessionSnapshot session, SaveInventoryDocRequest request, DateTime date, decimal total, int id, decimal paidAmount = -1)
    {
        p.AddWithValue("@SchoolID", session.SchoolID);
        p.AddWithValue("@YearID", session.EducationYearID);
        p.AddWithValue("@RegistrationID", session.RegistrationID);
        p.AddWithValue("@AccountID", request.AccountID > 0 ? (object)request.AccountID : DBNull.Value);
        p.AddWithValue("@Date", date);
        p.AddWithValue("@Invoice", (object?)NullIfEmpty(request.InvoiceNo) ?? DBNull.Value);
        p.AddWithValue("@Party", (object?)NullIfEmpty(request.Party) ?? DBNull.Value);
        p.AddWithValue("@Note", (object?)NullIfEmpty(request.Note) ?? DBNull.Value);
        p.AddWithValue("@Total", total);
        p.AddWithValue("@SupplierID", request.SupplierID > 0 ? (object)request.SupplierID : DBNull.Value);
        p.AddWithValue("@CustomerID", request.CustomerID > 0 ? (object)request.CustomerID : DBNull.Value);
        if (paidAmount >= 0) p.AddWithValue("@Paid", paidAmount);
        if (id > 0) p.AddWithValue("@ID", id);
    }

    private static async Task EnsureSchemaAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.Inv_ItemCategory', N'U') IS NULL
CREATE TABLE dbo.Inv_ItemCategory (
    ItemCategoryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_ItemCategory_Insert DEFAULT (GETDATE())
);
IF OBJECT_ID(N'dbo.Inv_Item', N'U') IS NULL
CREATE TABLE dbo.Inv_Item (
    ItemID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    ItemCategoryID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Unit NVARCHAR(40) NOT NULL CONSTRAINT DF_Inv_Item_Unit DEFAULT (N'pcs'),
    Sku NVARCHAR(80) NULL,
    MinStock DECIMAL(18,3) NOT NULL CONSTRAINT DF_Inv_Item_Min DEFAULT (0),
    PurchasePrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Item_PPrice DEFAULT (0),
    SalePrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Item_SPrice DEFAULT (0),
    IsActive BIT NOT NULL CONSTRAINT DF_Inv_Item_Active DEFAULT (1),
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Item_Insert DEFAULT (GETDATE())
);
IF OBJECT_ID(N'dbo.Inv_Purchase', N'U') IS NULL
CREATE TABLE dbo.Inv_Purchase (
    PurchaseID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    EducationYearID INT NOT NULL,
    RegistrationID INT NOT NULL,
    AccountID INT NULL,
    DocDate DATE NOT NULL,
    InvoiceNo NVARCHAR(80) NULL,
    Supplier NVARCHAR(200) NULL,
    SupplierID INT NULL,
    Note NVARCHAR(500) NULL,
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Purchase_Total DEFAULT (0),
    PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Purchase_Paid DEFAULT (0),
    ExpenseID INT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Purchase_Insert DEFAULT (GETDATE())
);
IF OBJECT_ID(N'dbo.Inv_PurchaseLine', N'U') IS NULL
CREATE TABLE dbo.Inv_PurchaseLine (
    PurchaseLineID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PurchaseID INT NOT NULL,
    ItemID INT NOT NULL,
    Qty DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL
);
IF OBJECT_ID(N'dbo.Inv_Sale', N'U') IS NULL
CREATE TABLE dbo.Inv_Sale (
    SaleID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    EducationYearID INT NOT NULL,
    RegistrationID INT NOT NULL,
    AccountID INT NULL,
    DocDate DATE NOT NULL,
    InvoiceNo NVARCHAR(80) NULL,
    Customer NVARCHAR(200) NULL,
    CustomerID INT NULL,
    Note NVARCHAR(500) NULL,
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Sale_Total DEFAULT (0),
    PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Sale_Paid DEFAULT (0),
    ExtraIncomeID INT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Sale_Insert DEFAULT (GETDATE())
);
IF OBJECT_ID(N'dbo.Inv_SaleLine', N'U') IS NULL
CREATE TABLE dbo.Inv_SaleLine (
    SaleLineID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SaleID INT NOT NULL,
    ItemID INT NOT NULL,
    Qty DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL
);
IF OBJECT_ID(N'dbo.Inv_Log', N'U') IS NOT NULL
    DROP TABLE dbo.Inv_Log;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Item_School' AND object_id = OBJECT_ID(N'dbo.Inv_Item'))
    CREATE INDEX IX_Inv_Item_School ON dbo.Inv_Item (SchoolID, ItemCategoryID, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Purchase_School' AND object_id = OBJECT_ID(N'dbo.Inv_Purchase'))
    CREATE INDEX IX_Inv_Purchase_School ON dbo.Inv_Purchase (SchoolID, EducationYearID, DocDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Sale_School' AND object_id = OBJECT_ID(N'dbo.Inv_Sale'))
    CREATE INDEX IX_Inv_Sale_School ON dbo.Inv_Sale (SchoolID, EducationYearID, DocDate);
""", con);
        await cmd.ExecuteNonQueryAsync(ct);
        await using (var extra = new SqlCommand("""
IF COL_LENGTH(N'dbo.Inv_Item', N'MinStock') IS NULL
    ALTER TABLE dbo.Inv_Item ADD MinStock DECIMAL(18,3) NOT NULL CONSTRAINT DF_Inv_Item_Min DEFAULT (0);
IF COL_LENGTH(N'dbo.Inv_Item', N'Sku') IS NULL
    ALTER TABLE dbo.Inv_Item ADD Sku NVARCHAR(80) NULL;
""", con))
            await extra.ExecuteNonQueryAsync(ct);
        await EnsureSupplierSchemaAsync(con, ct);
        await EnsureCustomerSchemaAsync(con, ct);
    }

    private static async Task<int> GetCustomerStudentIdAsync(SqlConnection con, int schoolId, int customerId, CancellationToken ct)
    {
        if (customerId <= 0) return 0;
        return await ScalarAsync(con, null, """
SELECT ISNULL(StudentID, 0) FROM dbo.Inv_Customer WHERE CustomerID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", customerId); p.AddWithValue("@SchoolID", schoolId); }, ct);
    }

    private static async Task<InventoryResult?> AttachSaleFeePayOrderAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, int saleId, int customerId, decimal due,
        string invoice, CancellationToken ct)
    {
        if (due <= 0 || customerId <= 0) return Fail("inv.cashOnlyCustomer");
        var studentId = await ScalarAsync(con, tx, """
SELECT ISNULL(StudentID, 0) FROM dbo.Inv_Customer WHERE CustomerID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", customerId); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
        if (studentId <= 0) return Fail("inv.cashOnlyCustomer");

        var roleId = await ScalarAsync(con, tx, """
SELECT TOP 1 RoleID FROM dbo.Income_Roles WHERE SchoolID = @SchoolID AND Role = @Role
""", p => { p.AddWithValue("@SchoolID", session.SchoolID); p.AddWithValue("@Role", SaleCategory); }, ct);
        if (roleId <= 0)
        {
            roleId = await ScalarAsync(con, tx, """
INSERT INTO dbo.Income_Roles (SchoolID, RegistrationID, Role, NumberOfPay, Description, Date)
VALUES (@SchoolID, @RegistrationID, @Role, 1, N'Inventory due', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
            {
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@Role", SaleCategory);
            }, ct);
        }

        var classId = 0;
        var studentClassId = 0;
        await using (var cls = new SqlCommand("""
SELECT TOP 1 sc.StudentClassID, sc.ClassID
FROM dbo.StudentsClass AS sc
WHERE sc.StudentID = @SID AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @YearID
  AND sc.Class_Status IS NULL
""", con, tx))
        {
            cls.Parameters.AddWithValue("@SID", studentId);
            cls.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cls.Parameters.AddWithValue("@YearID", session.EducationYearID);
            await using var reader = await cls.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                studentClassId = I(reader[0]);
                classId = I(reader[1]);
            }
        }

        var payOrderId = await ScalarAsync(con, tx, """
INSERT INTO dbo.Income_PayOrder
    (SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, EducationYearID,
     Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor,
     StartDate, EndDate, CreatedDate, NumberOfPayment, Is_Active, Is_LateFeeAdded)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SCID, @YearID,
     @Amount, 0, 0, 0, 0, @RoleID, @PayFor,
     @Date, @Date, GETDATE(), 0, 1, 0);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
        {
            p.AddWithValue("@SchoolID", session.SchoolID);
            p.AddWithValue("@RegistrationID", session.RegistrationID);
            p.AddWithValue("@StudentID", studentId);
            p.AddWithValue("@ClassID", classId > 0 ? classId : DBNull.Value);
            p.AddWithValue("@SCID", studentClassId > 0 ? studentClassId : DBNull.Value);
            p.AddWithValue("@YearID", session.EducationYearID);
            p.AddWithValue("@Amount", (double)due);
            p.AddWithValue("@RoleID", roleId);
            p.AddWithValue("@PayFor", invoice);
            p.AddWithValue("@Date", DateTime.Today);
        }, ct);
        if (payOrderId <= 0) return Fail("inv.failed");
        await ExecAsync(con, tx, "UPDATE dbo.Inv_Sale SET FeePayOrderID = @PID WHERE SaleID = @ID",
            p => { p.AddWithValue("@PID", payOrderId); p.AddWithValue("@ID", saleId); }, ct);
        return null;
    }

    private static async Task SetContextAsync(SqlConnection con, SqlTransaction tx, int registrationId, CancellationToken ct)
    {
        var bytes = new byte[128];
        BitConverter.GetBytes(registrationId).CopyTo(bytes, 0);
        await using var cmd = new SqlCommand("SET CONTEXT_INFO @Ctx", con, tx);
        cmd.Parameters.Add("@Ctx", SqlDbType.Binary, 128).Value = bytes;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task ExecAsync(SqlConnection con, SqlTransaction tx, string sql, Action<SqlParameterCollection> bind, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, con, tx);
        bind(cmd.Parameters);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> ScalarAsync(SqlConnection con, SqlTransaction? tx, string sql, Action<SqlParameterCollection> bind, CancellationToken ct)
    {
        await using var cmd = tx is null ? new SqlCommand(sql, con) : new SqlCommand(sql, con, tx);
        bind(cmd.Parameters);
        return I(await cmd.ExecuteScalarAsync(ct));
    }

    private static async Task<decimal> ScalarDecAsync(SqlConnection con, SqlTransaction? tx, string sql, Action<SqlParameterCollection> bind, CancellationToken ct)
    {
        await using var cmd = tx is null ? new SqlCommand(sql, con) : new SqlCommand(sql, con, tx);
        bind(cmd.Parameters);
        return Dec(await cmd.ExecuteScalarAsync(ct));
    }

    private static InventoryResult Ok(int id, string message) => new() { Succeeded = true, Id = id, Message = message };
    private static InventoryResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static string? NullS(object? value) => value is null or DBNull ? null : value.ToString();
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static decimal Dec(object? value) => value is null or DBNull ? 0 : Convert.ToDecimal(value);
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool Has(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
