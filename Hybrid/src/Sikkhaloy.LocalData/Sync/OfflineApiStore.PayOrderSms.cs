using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

internal sealed partial class OfflineApiStore
{
    private const string PayOrderMapKey = "offline:payorder-map";

    public async Task EnqueueOfficeSmsAsync(string phones, string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phones) || string.IsNullOrWhiteSpace(text))
            return;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.Outbox.Add(new OutboxEntry
        {
            LocalId = Guid.NewGuid(),
            EntityType = EntityTypes.PendingSms,
            Operation = SyncOperation.Create,
            PayloadJson = JsonSerializer.Serialize(new QueuedOfficeSms
            {
                Kind = "office",
                Phones = phones.Trim(),
                Text = text
            }, JsonOptions),
            CreatedUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OutboxEntry>> LoadPendingSmsAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Outbox
            .Where(x => x.EntityType == EntityTypes.PendingSms)
            .OrderBy(x => x.OutboxId)
            .Take(40)
            .ToListAsync(cancellationToken);
    }

    public static QueuedOfficeSms? ParseSms(OutboxEntry entry)
    {
        try
        {
            return JsonSerializer.Deserialize<QueuedOfficeSms>(entry.PayloadJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<string?> RemapCollectBodyAsync(string bodyJson, CancellationToken cancellationToken)
    {
        CollectPaymentRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<CollectPaymentRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return bodyJson;
        }

        if (request is null)
            return bodyJson;

        var student = await ResolveCollectStudentAsync(request, cancellationToken);
        if (student is null || student.ServerId is not > 0 || student.StudentClassServerId is not > 0)
            return null;

        request.StudentID = student.ServerId.Value;
        request.StudentClassID = student.StudentClassServerId.Value;
        request.StudentCode = student.StudentCode;

        var map = await ReadPayOrderMapAsync(cancellationToken);
        foreach (var item in request.Items)
        {
            if (item.PayOrderID >= 0)
                continue;
            var bound = map.FirstOrDefault(x => x.LocalId == item.PayOrderID && x.ServerId is > 0);
            if (bound?.ServerId is > 0)
            {
                item.PayOrderID = bound.ServerId.Value;
                continue;
            }
            return null;
        }

        return JsonSerializer.Serialize(request, JsonOptions);
    }

    public async Task<string?> RemapAddMoreBodyAsync(string bodyJson, CancellationToken cancellationToken)
    {
        AddMorePayOrderRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<AddMorePayOrderRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return bodyJson;
        }

        if (request is null)
            return bodyJson;
        if (request.StudentID > 0 && request.StudentClassID > 0)
            return JsonSerializer.Serialize(request, JsonOptions);

        var student = await FindLocalStudentAsync(request.StudentCode ?? "", cancellationToken);
        if (student?.ServerId is not > 0 || student.StudentClassServerId is not > 0)
            return null;

        request.StudentID = student.ServerId.Value;
        request.StudentClassID = student.StudentClassServerId.Value;
        request.ClassID = student.ClassID ?? request.ClassID;
        request.StudentCode = student.StudentCode;
        return JsonSerializer.Serialize(request, JsonOptions);
    }

    public async Task BindLocalPayOrdersAsync(IReadOnlyList<UnpaidPayOrderDto> serverUnpaid, CancellationToken cancellationToken)
    {
        var map = await ReadPayOrderMapAsync(cancellationToken);
        if (map.Count == 0)
            return;

        var used = new HashSet<int>();
        foreach (var hint in map.Where(x => x.ServerId is not > 0))
        {
            var match = serverUnpaid.FirstOrDefault(x =>
                !used.Contains(x.PayOrderID)
                && string.Equals(x.ID, hint.StudentCode, StringComparison.OrdinalIgnoreCase)
                && x.RoleID == hint.RoleID
                && string.Equals(x.PayFor, hint.PayFor, StringComparison.OrdinalIgnoreCase)
                && x.Amount == hint.Amount);
            if (match is null)
                continue;
            hint.ServerId = match.PayOrderID;
            used.Add(match.PayOrderID);
        }

        await SaveAsync(PayOrderMapKey, JsonSerializer.Serialize(map, JsonOptions), cancellationToken);
        await RewriteLocalPayOrderIdsAsync(map, serverUnpaid, cancellationToken);
        await RewriteQueuedCollectIdsAsync(map, cancellationToken);
    }

    public async Task<StudentReportDto?> StudentAccountsFromLocalAsync(string studentCode, CancellationToken cancellationToken)
    {
        var bundle = await BundleFromLocalAsync(studentCode, cancellationToken);
        if (bundle?.Student is null)
            return null;

        var dues = bundle.CurrentDues.Concat(bundle.OtherDues).Concat(bundle.InventoryDues).ToList();
        var payOrders = dues.Select(ToReportPay).ToList();
        var receipts = bundle.Receipts.Select(x => new StudentReportReceiptDto
        {
            ReceiptNo = x.ReceiptNo,
            PrintedReceiptNo = x.PrintedReceiptNo ?? x.ReceiptNo,
            PaidDate = x.PaidDate,
            Amount = x.TotalAmount
        }).ToList();

        return new StudentReportDto
        {
            Found = true,
            StudentID = bundle.Student.StudentID,
            StudentClassID = bundle.Student.StudentClassID,
            ClassID = bundle.Student.ClassID,
            Accounts = new StudentReportAccountsDto
            {
                TotalFee = payOrders.Sum(x => x.Amount),
                TotalDiscount = payOrders.Sum(x => x.Discount),
                TotalPaid = receipts.Sum(x => x.Amount),
                TotalDue = payOrders.Sum(x => x.Due),
                CurrentDueTotal = bundle.CurrentDue,
                Due = payOrders.Where(x => x.Due > 0).ToList(),
                CurrentDue = payOrders.Where(x => x.Due > 0).ToList(),
                Paid = payOrders.Where(x => x.PaidAmount > 0).ToList(),
                Receipts = receipts,
                AllPayOrders = payOrders
            }
        };
    }

    internal async Task ApplyPayOrderToCacheAsync(string bodyJson, CancellationToken cancellationToken)
    {
        CreatePayOrdersRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<CreatePayOrdersRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return;
        }

        if (request is null || request.Items.Count == 0)
            return;

        var codes = request.StudentIDs
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (codes.Count == 0)
            return;

        var roles = await ReadAsync<List<PaymentRoleDto>>("api/sync/accounts/roles", cancellationToken) ?? [];
        var unpaid = await ReadAsync<List<UnpaidPayOrderDto>>(UnpaidAllKey, cancellationToken) ?? [];
        var map = await ReadPayOrderMapAsync(cancellationToken);
        var nextId = NextLocalPayOrderId(unpaid, map);

        foreach (var code in codes)
        {
            var student = await FindLocalStudentAsync(code, cancellationToken);
            if (student is null)
                continue;
            foreach (var item in request.Items)
            {
                if (item.RoleID <= 0 || string.IsNullOrWhiteSpace(item.PayFor))
                    continue;
                var localId = nextId--;
                var roleName = roles.FirstOrDefault(x => x.RoleID == item.RoleID)?.Role ?? item.PayFor;
                unpaid.Add(new UnpaidPayOrderDto
                {
                    PayOrderID = localId,
                    RoleID = item.RoleID,
                    StudentID = student.ServerId ?? 0,
                    ClassID = student.ClassID ?? 0,
                    ID = student.StudentCode,
                    Name = student.StudentsName,
                    ClassName = student.ClassName,
                    Role = roleName,
                    PayFor = item.PayFor.Trim(),
                    Amount = item.Amount,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate
                });
                map.Add(new LocalPayOrderHint
                {
                    LocalId = localId,
                    StudentCode = student.StudentCode,
                    RoleID = item.RoleID,
                    PayFor = item.PayFor.Trim(),
                    Amount = item.Amount
                });
                await AppendDueToBundleAsync(student, localId, item, roleName, cancellationToken);
            }
        }

        await SaveAsync(UnpaidAllKey, JsonSerializer.Serialize(unpaid, JsonOptions), cancellationToken);
        await SaveAsync(PayOrderMapKey, JsonSerializer.Serialize(map, JsonOptions), cancellationToken);
    }

    internal async Task ApplyAddMoreToCacheAsync(string bodyJson, CancellationToken cancellationToken)
    {
        AddMorePayOrderRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<AddMorePayOrderRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return;
        }

        if (request is null || request.RoleID <= 0 || request.Amount <= 0)
            return;

        var student = await FindLocalStudentAsync(request.StudentCode ?? "", cancellationToken);
        if (student is null && request.StudentID > 0)
        {
            var scope = await CurrentScopeAsync(cancellationToken);
            if (scope is not null)
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                student = await db.Students.AsNoTracking().FirstOrDefaultAsync(
                    x => x.SchoolID == scope.Value.SchoolId && x.ServerId == request.StudentID,
                    cancellationToken);
            }
        }

        if (student is null)
            return;

        var payFor = string.IsNullOrWhiteSpace(request.PayFor) ? DateTime.Today.ToString("MMMM") : request.PayFor.Trim();
        var roles = await ReadAsync<List<PaymentRoleDto>>("api/sync/accounts/roles", cancellationToken) ?? [];
        var unpaid = await ReadAsync<List<UnpaidPayOrderDto>>(UnpaidAllKey, cancellationToken) ?? [];
        var map = await ReadPayOrderMapAsync(cancellationToken);
        var localId = NextLocalPayOrderId(unpaid, map);
        var roleName = roles.FirstOrDefault(x => x.RoleID == request.RoleID)?.Role ?? payFor;
        unpaid.Add(new UnpaidPayOrderDto
        {
            PayOrderID = localId,
            RoleID = request.RoleID,
            StudentID = student.ServerId ?? 0,
            ClassID = student.ClassID ?? request.ClassID,
            ID = student.StudentCode,
            Name = student.StudentsName,
            ClassName = student.ClassName,
            Role = roleName,
            PayFor = payFor,
            Amount = request.Amount
        });
        map.Add(new LocalPayOrderHint
        {
            LocalId = localId,
            StudentCode = student.StudentCode,
            RoleID = request.RoleID,
            PayFor = payFor,
            Amount = request.Amount
        });
        await AppendDueToBundleAsync(student, localId, new CreatePayOrderItem
        {
            RoleID = request.RoleID,
            PayFor = payFor,
            Amount = request.Amount,
            Discount = request.Discount,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today
        }, roleName, cancellationToken);
        await SaveAsync(UnpaidAllKey, JsonSerializer.Serialize(unpaid, JsonOptions), cancellationToken);
        await SaveAsync(PayOrderMapKey, JsonSerializer.Serialize(map, JsonOptions), cancellationToken);
    }

    private async Task<LocalStudent?> ResolveCollectStudentAsync(CollectPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.StudentCode))
        {
            var byCode = await FindLocalStudentAsync(request.StudentCode, cancellationToken);
            if (byCode is not null)
                return byCode;
        }

        if (request.StudentID <= 0)
            return null;
        var scope = await CurrentScopeAsync(cancellationToken);
        if (scope is null)
            return null;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Students.AsNoTracking().FirstOrDefaultAsync(
            x => x.SchoolID == scope.Value.SchoolId && x.ServerId == request.StudentID,
            cancellationToken);
    }

    private async Task<List<LocalPayOrderHint>> ReadPayOrderMapAsync(CancellationToken cancellationToken) =>
        await ReadAsync<List<LocalPayOrderHint>>(PayOrderMapKey, cancellationToken) ?? [];

    private static int NextLocalPayOrderId(List<UnpaidPayOrderDto> unpaid, List<LocalPayOrderHint> map)
    {
        var min = -1;
        if (unpaid.Count > 0)
            min = Math.Min(min, unpaid.Min(x => x.PayOrderID));
        if (map.Count > 0)
            min = Math.Min(min, map.Min(x => x.LocalId));
        return min > 0 ? -1 : min - 1;
    }

    private async Task AppendDueToBundleAsync(
        LocalStudent student, int localId, CreatePayOrderItem item, string roleName, CancellationToken cancellationToken)
    {
        var key = $"api/sync/accounts/students/bundle?id={Uri.EscapeDataString(student.StudentCode)}";
        var bundle = await ReadAsync<FeeStudentBundleDto>(key, cancellationToken)
                     ?? await BundleFromLocalAsync(student.StudentCode, cancellationToken)
                     ?? new FeeStudentBundleDto { Student = ToFeeStudent(student) };
        bundle.Student ??= ToFeeStudent(student);
        bundle.CurrentDues.Add(new DueRowDto
        {
            PayOrderID = localId,
            RoleID = item.RoleID,
            Role = roleName,
            PayFor = item.PayFor,
            ClassName = student.ClassName,
            Amount = item.Amount,
            Discount = item.Discount,
            Due = Math.Max(0, item.Amount - item.Discount),
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            CurrentYear = true
        });
        bundle.CurrentDue = bundle.CurrentDues.Sum(x => x.Due);
        await SaveAsync(key, JsonSerializer.Serialize(bundle, JsonOptions), cancellationToken);
    }

    private async Task RewriteLocalPayOrderIdsAsync(
        List<LocalPayOrderHint> map, IReadOnlyList<UnpaidPayOrderDto> serverUnpaid, CancellationToken cancellationToken)
    {
        var unpaid = await ReadAsync<List<UnpaidPayOrderDto>>(UnpaidAllKey, cancellationToken) ?? [];
        foreach (var hint in map.Where(x => x.ServerId is > 0))
        {
            foreach (var row in unpaid.Where(x => x.PayOrderID == hint.LocalId))
                row.PayOrderID = hint.ServerId!.Value;
        }

        var leftover = unpaid.Where(x => x.PayOrderID < 0).ToList();
        var merged = serverUnpaid.ToList();
        foreach (var row in leftover)
        {
            if (!merged.Any(x => x.PayOrderID == row.PayOrderID))
                merged.Add(row);
        }

        await SaveAsync(UnpaidAllKey, JsonSerializer.Serialize(merged, JsonOptions), cancellationToken);

        var codes = map.Select(x => x.StudentCode).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var code in codes)
        {
            var key = $"api/sync/accounts/students/bundle?id={Uri.EscapeDataString(code)}";
            var bundle = await ReadAsync<FeeStudentBundleDto>(key, cancellationToken);
            if (bundle is null)
                continue;
            foreach (var hint in map.Where(x => x.ServerId is > 0
                                                && string.Equals(x.StudentCode, code, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var row in bundle.CurrentDues.Concat(bundle.OtherDues).Concat(bundle.InventoryDues))
                {
                    if (row.PayOrderID == hint.LocalId)
                        row.PayOrderID = hint.ServerId!.Value;
                }
            }
            await SaveAsync(key, JsonSerializer.Serialize(bundle, JsonOptions), cancellationToken);
        }
    }

    private async Task RewriteQueuedCollectIdsAsync(List<LocalPayOrderHint> map, CancellationToken cancellationToken)
    {
        var bound = map.Where(x => x.ServerId is > 0).ToList();
        if (bound.Count == 0)
            return;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var rows = await db.Outbox.Where(x => x.EntityType == EntityTypes.ApiCall).ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var call = Parse(row);
            if (call is null || !string.Equals(call.Url, "api/sync/accounts/collect", StringComparison.OrdinalIgnoreCase))
                continue;
            CollectPaymentRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<CollectPaymentRequest>(call.BodyJson, JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (request is null)
                continue;
            var changed = false;
            foreach (var item in request.Items)
            {
                var hit = bound.FirstOrDefault(x => x.LocalId == item.PayOrderID);
                if (hit?.ServerId is not > 0)
                    continue;
                item.PayOrderID = hit.ServerId.Value;
                changed = true;
            }

            if (!changed)
                continue;
            call.BodyJson = JsonSerializer.Serialize(request, JsonOptions);
            row.PayloadJson = JsonSerializer.Serialize(call, JsonOptions);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static StudentReportPayOrderDto ToReportPay(DueRowDto x) => new()
    {
        ClassName = x.ClassName,
        Role = x.Role,
        PayFor = x.PayFor,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        Amount = x.Amount,
        Discount = x.Discount,
        PaidAmount = x.PaidAmount,
        Due = x.Due,
        LateFee = x.LateFee,
        LateFeeDiscount = x.LateFeeDiscount
    };
}
