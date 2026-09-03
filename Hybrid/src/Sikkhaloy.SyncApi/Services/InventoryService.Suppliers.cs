using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Inventory;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class InventoryService
{
    public async Task<IReadOnlyList<InventorySupplierDto>> ListSuppliersAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        return await LoadSuppliersAsync(con, session.SchoolID, ct);
    }

    public async Task<InventoryResult> SaveSupplierAsync(SessionSnapshot session, SaveInventorySupplierRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("inv.needSupplier");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        if (request!.SupplierID > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Inv_Supplier SET Name = @Name, Phone = @Phone, Address = @Address
WHERE SupplierID = @ID AND SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Phone", (object?)NullIfEmpty(request.Phone) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ID", request.SupplierID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(ct);
            if (n <= 0) return Fail("inv.empty");
            return Ok(request.SupplierID, "inv.supplierUpdated");
        }

        var exists = await ScalarAsync(con, null, """
SELECT COUNT(1) FROM dbo.Inv_Supplier WHERE SchoolID = @SchoolID AND Name = @Name
""", p => { p.AddWithValue("@SchoolID", session.SchoolID); p.AddWithValue("@Name", name); }, ct);
        if (exists > 0) return Fail("inv.supplierExists");

        var id = await ScalarAsync(con, null, """
INSERT INTO dbo.Inv_Supplier (SchoolID, Name, Phone, Address)
VALUES (@SchoolID, @Name, @Phone, @Address);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
        {
            p.AddWithValue("@SchoolID", session.SchoolID);
            p.AddWithValue("@Name", name);
            p.AddWithValue("@Phone", (object?)NullIfEmpty(request.Phone) ?? DBNull.Value);
            p.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
        }, ct);
        return Ok(id, "inv.supplierSaved");
    }

    public async Task<InventoryResult> DeleteSupplierAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("inv.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var used = await ScalarAsync(con, null, """
SELECT COUNT(1) FROM dbo.Inv_Purchase WHERE SchoolID = @SchoolID AND SupplierID = @ID
""", p => { p.AddWithValue("@SchoolID", session.SchoolID); p.AddWithValue("@ID", id); }, ct);
        if (used > 0) return Fail("inv.supplierUsed");

        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Inv_Supplier WHERE SupplierID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n <= 0) return Fail("inv.empty");
        return Ok(id, "inv.deleted");
    }

    public async Task<InventorySupplierLedgerDto> GetSupplierLedgerAsync(SessionSnapshot session, int supplierId, CancellationToken ct)
    {
        var dto = new InventorySupplierLedgerDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var suppliers = await LoadSuppliersAsync(con, session.SchoolID, ct);
        dto.Supplier = suppliers.FirstOrDefault(x => x.SupplierID == supplierId) ?? new InventorySupplierDto { SupplierID = supplierId };
        dto.Dues = await LoadSupplierDuesAsync(con, session.SchoolID, supplierId, dueOnly: false, ct);
        await using var cmd = new SqlCommand("""
SELECT p.PaymentID, p.SupplierID, ISNULL(p.PurchaseID, 0) AS PurchaseID, p.AccountID,
       ISNULL(acc.AccountName, N'') AS AccountName, ISNULL(d.InvoiceNo, N'') AS InvoiceNo,
       p.Amount, p.DocDate, p.Note, ISNULL(reg.UserName, N'') AS UserName
FROM dbo.Inv_SupplierPayment AS p
LEFT JOIN dbo.Account AS acc ON acc.AccountID = p.AccountID
LEFT JOIN dbo.Inv_Purchase AS d ON d.PurchaseID = p.PurchaseID
LEFT JOIN dbo.Registration AS reg ON reg.RegistrationID = p.RegistrationID
WHERE p.SchoolID = @SchoolID AND p.SupplierID = @ID
ORDER BY p.DocDate DESC, p.PaymentID DESC
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ID", supplierId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            dto.Payments.Add(new InventorySupplierPaymentDto
            {
                PaymentID = I(reader["PaymentID"]),
                SupplierID = I(reader["SupplierID"]),
                PurchaseID = I(reader["PurchaseID"]),
                AccountID = I(reader["AccountID"]),
                AccountName = S(reader["AccountName"]),
                InvoiceNo = S(reader["InvoiceNo"]),
                Amount = Dec(reader["Amount"]),
                Date = Convert.ToDateTime(reader["DocDate"]).Date,
                Note = NullS(reader["Note"]),
                UserName = S(reader["UserName"])
            });
        }
        return dto;
    }

    public async Task<InventoryResult> SaveSupplierPaymentAsync(
        SessionSnapshot session, SaveInventorySupplierPaymentRequest? request, CancellationToken ct)
    {
        if (request is null || request.SupplierID <= 0)
            return Fail("inv.needSupplier");
        if (request.AccountID <= 0)
            return Fail("acc.needPay");
        if (request.Amount <= 0)
            return Fail("inv.needPayAmount");
        var date = request.Date == default ? DateTime.Today : request.Date.Date;
        if (date > DateTime.Today)
            return Fail("acc.futureDate");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var supplierName = await GetSupplierNameAsync(con, session.SchoolID, request.SupplierID, ct);
        if (string.IsNullOrWhiteSpace(supplierName))
            return Fail("inv.needSupplier");
        var dues = await LoadSupplierDuesAsync(con, session.SchoolID, request.SupplierID, dueOnly: true, ct);
        if (request.PurchaseID > 0)
            dues = dues.Where(x => x.PurchaseID == request.PurchaseID).ToList();
        var dueTotal = dues.Sum(x => x.Due);
        if (dueTotal <= 0)
            return Fail("inv.noDue");
        var amount = Math.Round(request.Amount, 2);
        if (amount > dueTotal)
            return Fail("inv.payExceedsDue");

        var cats = await EnsureAccountCategoriesAsync(con, session, ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await SetContextAsync(con, tx, session.RegistrationID, ct);
            var balErr = await EnsurePurchaseBalanceAsync(con, tx, session.SchoolID, request.AccountID, 0, amount, ct);
            if (balErr is not null) { await tx.RollbackAsync(ct); return balErr; }

            var details = $"Supplier Payment: {supplierName}. {amount:0.##} Tk.";
            var expenseId = await ScalarAsync(con, tx, """
INSERT INTO dbo.Expenditure
    (RegistrationID, ExpenseCategoryID, Amount, ExpenseFor, ExpenseDate, SchoolID, EducationYearID, AccountID)
VALUES
    (@RegistrationID, @Cat, @Amount, @Details, @Date, @SchoolID, @YearID, @AccountID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
            {
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@Cat", cats.ExpenseCat);
                p.AddWithValue("@Amount", (double)amount);
                p.AddWithValue("@Details", details);
                p.AddWithValue("@Date", date);
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@YearID", session.EducationYearID);
                p.AddWithValue("@AccountID", request.AccountID);
            }, ct);

            var remaining = amount;
            var lastId = 0;
            foreach (var due in dues)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, due.Due);
                if (take <= 0) continue;
                lastId = await ScalarAsync(con, tx, """
INSERT INTO dbo.Inv_SupplierPayment
    (SchoolID, SupplierID, PurchaseID, AccountID, Amount, DocDate, Note, ExpenseID, RegistrationID)
VALUES
    (@SchoolID, @SupplierID, @PurchaseID, @AccountID, @Amount, @Date, @Note, @ExpenseID, @RegistrationID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
                {
                    p.AddWithValue("@SchoolID", session.SchoolID);
                    p.AddWithValue("@SupplierID", request.SupplierID);
                    p.AddWithValue("@PurchaseID", due.PurchaseID);
                    p.AddWithValue("@AccountID", request.AccountID);
                    p.AddWithValue("@Amount", take);
                    p.AddWithValue("@Date", date);
                    p.AddWithValue("@Note", (object?)NullIfEmpty(request.Note) ?? DBNull.Value);
                    p.AddWithValue("@ExpenseID", expenseId);
                    p.AddWithValue("@RegistrationID", session.RegistrationID);
                }, ct);
                await ExecAsync(con, tx, """
UPDATE dbo.Inv_Purchase
SET PaidAmount = ISNULL(PaidAmount, 0) + @Take,
    AccountID = CASE WHEN AccountID IS NULL OR AccountID = 0 THEN @AccountID ELSE AccountID END
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Take", take);
                    p.AddWithValue("@AccountID", request.AccountID);
                    p.AddWithValue("@ID", due.PurchaseID);
                    p.AddWithValue("@SchoolID", session.SchoolID);
                }, ct);
                remaining -= take;
            }

            await tx.CommitAsync(ct);
            return Ok(lastId, "inv.paySaved");
        }
        catch (SqlException)
        {
            await tx.RollbackAsync(ct);
            return Fail("acc.overBalance");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static async Task<List<InventorySupplierDto>> LoadSuppliersAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        var rows = new List<InventorySupplierDto>();
        await using var cmd = new SqlCommand("""
SELECT s.SupplierID, s.Name, s.Phone, s.Address,
       ISNULL(p.Purchased, 0) AS Purchased, ISNULL(p.Paid, 0) AS Paid
FROM dbo.Inv_Supplier AS s
LEFT JOIN (
    SELECT SupplierID,
           SUM(Total) AS Purchased,
           SUM(ISNULL(PaidAmount, CASE WHEN ISNULL(ExpenseID, 0) > 0 THEN Total ELSE 0 END)) AS Paid
    FROM dbo.Inv_Purchase
    WHERE SchoolID = @SchoolID
    GROUP BY SupplierID
) AS p ON p.SupplierID = s.SupplierID
WHERE s.SchoolID = @SchoolID
ORDER BY s.Name
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var purchased = Dec(reader["Purchased"]);
            var paid = Dec(reader["Paid"]);
            rows.Add(new InventorySupplierDto
            {
                SupplierID = I(reader["SupplierID"]),
                Name = S(reader["Name"]),
                Phone = NullS(reader["Phone"]),
                Address = NullS(reader["Address"]),
                Purchased = purchased,
                Paid = paid,
                Due = Math.Max(0, purchased - paid)
            });
        }
        return rows;
    }

    private static async Task<string?> GetSupplierNameAsync(
        SqlConnection con, int schoolId, int supplierId, CancellationToken ct, SqlTransaction? tx = null)
    {
        if (supplierId <= 0) return null;
        await using var cmd = tx is null
            ? new SqlCommand("""
SELECT Name FROM dbo.Inv_Supplier WHERE SupplierID = @ID AND SchoolID = @SchoolID
""", con)
            : new SqlCommand("""
SELECT Name FROM dbo.Inv_Supplier WHERE SupplierID = @ID AND SchoolID = @SchoolID
""", con, tx);
        cmd.Parameters.AddWithValue("@ID", supplierId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        return NullIfEmpty(S(await cmd.ExecuteScalarAsync(ct)));
    }

    public Task PrepareSchemaAsync(SqlConnection con, CancellationToken ct) => EnsureSchemaAsync(con, ct);

    public async Task<InventoryResult> SyncExpenseInventoryInTxAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session,
        int expenseId, decimal newAmount, bool reapply, CancellationToken ct)
    {
        newAmount = Math.Round(newAmount, 2);
        var oldAmount = await ScalarDecAsync(con, tx, """
SELECT ISNULL(Amount, 0) FROM dbo.Expenditure WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", expenseId); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);

        var pays = new List<(int PurchaseID, int SupplierID, int AccountID, decimal Amount, DateTime Date, string? Note)>();
        await using (var cmd = new SqlCommand("""
SELECT ISNULL(PurchaseID, 0), SupplierID, AccountID, Amount, DocDate, Note
FROM dbo.Inv_SupplierPayment
WHERE ExpenseID = @ID AND SchoolID = @SchoolID
ORDER BY PaymentID
""", con, tx))
        {
            cmd.Parameters.AddWithValue("@ID", expenseId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                pays.Add((
                    I(reader[0]),
                    I(reader[1]),
                    I(reader[2]),
                    Dec(reader[3]),
                    Convert.ToDateTime(reader[4]).Date,
                    NullIfEmpty(S(reader[5]))));
            }
        }

        foreach (var pay in pays)
        {
            if (pay.PurchaseID <= 0 || pay.Amount <= 0) continue;
            await ExecAsync(con, tx, """
UPDATE dbo.Inv_Purchase
SET PaidAmount = CASE
    WHEN ISNULL(PaidAmount, 0) > @Amt THEN ISNULL(PaidAmount, 0) - @Amt
    ELSE 0 END
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p =>
            {
                p.AddWithValue("@Amt", pay.Amount);
                p.AddWithValue("@ID", pay.PurchaseID);
                p.AddWithValue("@SchoolID", session.SchoolID);
            }, ct);
        }

        if (pays.Count > 0)
        {
            await ExecAsync(con, tx, """
DELETE FROM dbo.Inv_SupplierPayment WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", expenseId); p.AddWithValue("@SchoolID", session.SchoolID); }, ct);
        }

        if (reapply && newAmount > 0 && pays.Count > 0)
        {
            var supplierId = pays[0].SupplierID;
            var accountId = pays[0].AccountID;
            var date = pays[0].Date;
            var note = pays[0].Note;
            var dues = await LoadSupplierDuesAsync(con, session.SchoolID, supplierId, true, ct, tx);
            var dueTotal = dues.Sum(d => d.Due);
            if (newAmount > dueTotal + 0.009m)
                return Fail("inv.payExceedsDue");

            var remaining = newAmount;
            foreach (var due in dues)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, due.Due);
                if (take <= 0) continue;
                await ScalarAsync(con, tx, """
INSERT INTO dbo.Inv_SupplierPayment
    (SchoolID, SupplierID, PurchaseID, AccountID, Amount, DocDate, Note, ExpenseID, RegistrationID)
VALUES
    (@SchoolID, @SupplierID, @PurchaseID, @AccountID, @Amount, @Date, @Note, @ExpenseID, @RegistrationID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
                {
                    p.AddWithValue("@SchoolID", session.SchoolID);
                    p.AddWithValue("@SupplierID", supplierId);
                    p.AddWithValue("@PurchaseID", due.PurchaseID);
                    p.AddWithValue("@AccountID", accountId);
                    p.AddWithValue("@Amount", take);
                    p.AddWithValue("@Date", date);
                    p.AddWithValue("@Note", (object?)note ?? DBNull.Value);
                    p.AddWithValue("@ExpenseID", expenseId);
                    p.AddWithValue("@RegistrationID", session.RegistrationID);
                }, ct);
                await ExecAsync(con, tx, """
UPDATE dbo.Inv_Purchase
SET PaidAmount = ISNULL(PaidAmount, 0) + @Take
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Take", take);
                    p.AddWithValue("@ID", due.PurchaseID);
                    p.AddWithValue("@SchoolID", session.SchoolID);
                }, ct);
                remaining -= take;
            }
        }

        var purchaseIds = new List<(int PurchaseID, decimal Total, decimal Paid)>();
        await using (var cmd = new SqlCommand("""
SELECT PurchaseID, Total, ISNULL(PaidAmount, 0)
FROM dbo.Inv_Purchase
WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", con, tx))
        {
            cmd.Parameters.AddWithValue("@ID", expenseId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                purchaseIds.Add((I(reader[0]), Dec(reader[1]), Dec(reader[2])));
        }

        foreach (var row in purchaseIds)
        {
            var otherPaid = Math.Max(0, row.Paid - oldAmount);
            if (reapply && newAmount > 0)
            {
                var maxForThis = Math.Max(0, row.Total - otherPaid);
                if (newAmount > maxForThis + 0.009m)
                    return Fail("inv.payExceedsDue");
                await ExecAsync(con, tx, """
UPDATE dbo.Inv_Purchase
SET PaidAmount = @Paid
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Paid", otherPaid + newAmount);
                    p.AddWithValue("@ID", row.PurchaseID);
                    p.AddWithValue("@SchoolID", session.SchoolID);
                }, ct);
            }
            else
            {
                await ExecAsync(con, tx, """
UPDATE dbo.Inv_Purchase
SET PaidAmount = @Paid, ExpenseID = NULL
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Paid", otherPaid);
                    p.AddWithValue("@ID", row.PurchaseID);
                    p.AddWithValue("@SchoolID", session.SchoolID);
                }, ct);
            }
        }

        return Ok(expenseId, "");
    }

    private static async Task<List<InventorySupplierDueDto>> LoadSupplierDuesAsync(
        SqlConnection con, int schoolId, int supplierId, bool dueOnly, CancellationToken ct,
        SqlTransaction? tx = null)
    {
        var rows = new List<InventorySupplierDueDto>();
        await using var cmd = tx is null
            ? new SqlCommand("", con)
            : new SqlCommand("", con, tx);
        cmd.CommandText = $"""
SELECT PurchaseID, ISNULL(InvoiceNo, N'') AS InvoiceNo, DocDate, Total,
       ISNULL(PaidAmount, CASE WHEN ISNULL(ExpenseID, 0) > 0 THEN Total ELSE 0 END) AS Paid
FROM dbo.Inv_Purchase
WHERE SchoolID = @SchoolID AND SupplierID = @ID
{(dueOnly ? "AND Total > ISNULL(PaidAmount, CASE WHEN ISNULL(ExpenseID, 0) > 0 THEN Total ELSE 0 END)" : "")}
ORDER BY DocDate, PurchaseID
""";
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ID", supplierId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var total = Dec(reader["Total"]);
            var paid = Dec(reader["Paid"]);
            rows.Add(new InventorySupplierDueDto
            {
                PurchaseID = I(reader["PurchaseID"]),
                InvoiceNo = S(reader["InvoiceNo"]),
                Date = Convert.ToDateTime(reader["DocDate"]).Date,
                Total = total,
                Paid = paid,
                Due = Math.Max(0, total - paid)
            });
        }
        return rows;
    }

    private static async Task ReverseSupplierPaymentsAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int purchaseId, CancellationToken ct)
    {
        var expenses = new HashSet<int>();
        await using (var cmd = new SqlCommand("""
SELECT ISNULL(ExpenseID, 0) FROM dbo.Inv_SupplierPayment
WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", con, tx))
        {
            cmd.Parameters.AddWithValue("@ID", purchaseId);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var expenseId = I(reader[0]);
                if (expenseId > 0) expenses.Add(expenseId);
            }
        }

        await ExecAsync(con, tx, """
DELETE FROM dbo.Inv_SupplierPayment WHERE PurchaseID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", purchaseId); p.AddWithValue("@SchoolID", schoolId); }, ct);

        foreach (var expenseId in expenses)
        {
            var leftover = await ScalarDecAsync(con, tx, """
SELECT ISNULL(SUM(Amount), 0) FROM dbo.Inv_SupplierPayment WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", expenseId); p.AddWithValue("@SchoolID", schoolId); }, ct);
            if (leftover <= 0)
            {
                await ExecAsync(con, tx, """
DELETE FROM dbo.Expenditure WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", p => { p.AddWithValue("@ID", expenseId); p.AddWithValue("@SchoolID", schoolId); }, ct);
            }
            else
            {
                await ExecAsync(con, tx, """
UPDATE dbo.Expenditure SET Amount = @Amount WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", p =>
                {
                    p.AddWithValue("@Amount", (double)leftover);
                    p.AddWithValue("@ID", expenseId);
                    p.AddWithValue("@SchoolID", schoolId);
                }, ct);
            }
        }
    }

    private static async Task EnsureSupplierSchemaAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.Inv_Supplier', N'U') IS NULL
CREATE TABLE dbo.Inv_Supplier (
    SupplierID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(400) NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Supplier_Insert DEFAULT (GETDATE())
);
IF OBJECT_ID(N'dbo.Inv_SupplierPayment', N'U') IS NULL
CREATE TABLE dbo.Inv_SupplierPayment (
    PaymentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    SupplierID INT NOT NULL,
    PurchaseID INT NULL,
    AccountID INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    DocDate DATE NOT NULL,
    Note NVARCHAR(500) NULL,
    ExpenseID INT NULL,
    RegistrationID INT NOT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_SupplierPayment_Insert DEFAULT (GETDATE())
);
IF COL_LENGTH(N'dbo.Inv_Purchase', N'SupplierID') IS NULL
    ALTER TABLE dbo.Inv_Purchase ADD SupplierID INT NULL;
IF COL_LENGTH(N'dbo.Inv_Purchase', N'PaidAmount') IS NULL
    ALTER TABLE dbo.Inv_Purchase ADD PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Purchase_Paid DEFAULT (0);
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inv_Purchase') AND name = N'AccountID' AND is_nullable = 0)
    ALTER TABLE dbo.Inv_Purchase ALTER COLUMN AccountID INT NULL;
IF COL_LENGTH(N'dbo.Inv_Purchase', N'PaidAmount') IS NOT NULL
    EXEC(N'UPDATE dbo.Inv_Purchase SET PaidAmount = Total WHERE ExpenseID IS NOT NULL AND ISNULL(PaidAmount, 0) = 0 AND Total > 0');
IF OBJECT_ID(N'dbo.Inv_SupplierPayment', N'U') IS NOT NULL
    EXEC(N'
UPDATE p
SET p.PaidAmount = CASE
    WHEN ISNULL(p.PaidAmount, 0) > x.Amt THEN ISNULL(p.PaidAmount, 0) - x.Amt
    ELSE 0 END
FROM dbo.Inv_Purchase AS p
INNER JOIN (
    SELECT sp.PurchaseID, SUM(sp.Amount) AS Amt
    FROM dbo.Inv_SupplierPayment AS sp
    WHERE ISNULL(sp.ExpenseID, 0) > 0
      AND NOT EXISTS (
          SELECT 1 FROM dbo.Expenditure AS e
          WHERE e.ExpenseID = sp.ExpenseID AND e.SchoolID = sp.SchoolID)
    GROUP BY sp.PurchaseID
) AS x ON x.PurchaseID = p.PurchaseID;
DELETE sp
FROM dbo.Inv_SupplierPayment AS sp
WHERE ISNULL(sp.ExpenseID, 0) > 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.Expenditure AS e
      WHERE e.ExpenseID = sp.ExpenseID AND e.SchoolID = sp.SchoolID);
');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Supplier_School' AND object_id = OBJECT_ID(N'dbo.Inv_Supplier'))
    CREATE INDEX IX_Inv_Supplier_School ON dbo.Inv_Supplier (SchoolID, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Purchase_Supplier' AND object_id = OBJECT_ID(N'dbo.Inv_Purchase'))
    CREATE INDEX IX_Inv_Purchase_Supplier ON dbo.Inv_Purchase (SchoolID, SupplierID, DocDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_SupplierPayment_School' AND object_id = OBJECT_ID(N'dbo.Inv_SupplierPayment'))
    CREATE INDEX IX_Inv_SupplierPayment_School ON dbo.Inv_SupplierPayment (SchoolID, SupplierID, DocDate);
""", con);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
