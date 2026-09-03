using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.Shared.Inventory;

namespace Sikkhaloy.LocalData.Sync;

internal sealed partial class OfflineApiStore
{
    public const string InventoryLookupsKey = "api/sync/inventory/lookups";

    internal async Task<int> ApplyInventorySaleToCacheAsync(string bodyJson, CancellationToken cancellationToken)
    {
        SaveInventoryDocRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<SaveInventoryDocRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return 0;
        }

        if (request is null || request.Lines.Count == 0)
            return 0;

        var now = DateTime.Now;
        var id = unchecked((int)(2_000_000_000L + (now.Ticks % 99_999_999L)));
        if (id == 0) id = 2_000_000_001;
        var invoiceNo = string.IsNullOrWhiteSpace(request.InvoiceNo)
            ? $"OFF-{now:yyyyMMdd-HHmmssfff}"
            : request.InvoiceNo.Trim();
        var total = request.Lines.Sum(x => x.Amount);
        var paid = request.PayNow < 0 ? total : Math.Clamp(request.PayNow, 0, total);

        string? accountName = null;
        string? userName = null;
        var lookups = await ReadAsync<InventoryLookupsDto>(InventoryLookupsKey, cancellationToken);
        accountName = lookups?.Accounts.FirstOrDefault(x => x.AccountID == request.AccountID)?.AccountName;
        await using (var db = await _dbFactory.CreateDbContextAsync(cancellationToken))
        {
            var sess = await db.Sessions.AsNoTracking()
                .OrderByDescending(x => x.CachedUtc)
                .FirstOrDefaultAsync(cancellationToken);
            userName = string.IsNullOrWhiteSpace(sess?.DisplayName) ? sess?.UserName : sess.DisplayName;
        }

        if (lookups is not null)
        {
            foreach (var line in request.Lines)
            {
                var item = lookups.Items.FirstOrDefault(x => x.ItemID == line.ItemID);
                if (item is null) continue;
                item.Sold += line.Qty;
                item.Stock = Math.Max(0, item.Stock - line.Qty);
            }
            var acc = lookups.Accounts.FirstOrDefault(x => x.AccountID == request.AccountID);
            if (acc is not null)
                acc.Balance += paid;
            await SaveAsync(InventoryLookupsKey, JsonSerializer.Serialize(lookups, JsonOptions), cancellationToken);
        }

        var units = lookups?.Items.ToDictionary(x => x.ItemID, x => x.Unit) ?? [];
        var doc = new InventoryDocDto
        {
            Id = id,
            Date = request.Date == default ? DateTime.Today : request.Date,
            InvoiceNo = invoiceNo,
            Party = request.Party ?? "",
            Note = request.Note,
            AccountID = request.AccountID,
            AccountName = accountName ?? "",
            Total = total,
            CustomerID = request.CustomerID,
            PaidAmount = paid,
            DueAmount = Math.Max(0, total - paid),
            UserName = userName,
            Lines = request.Lines.Select(x => new InventoryLineDto
            {
                ItemID = x.ItemID,
                ItemName = x.ItemName,
                Unit = string.IsNullOrWhiteSpace(x.Unit) && units.TryGetValue(x.ItemID, out var u) ? u : x.Unit,
                Qty = x.Qty,
                UnitPrice = x.UnitPrice,
                Amount = x.Amount
            }).ToList()
        };
        await SaveAsync($"api/sync/inventory/sales/{id}", JsonSerializer.Serialize(doc, JsonOptions), cancellationToken);
        return id;
    }
}
