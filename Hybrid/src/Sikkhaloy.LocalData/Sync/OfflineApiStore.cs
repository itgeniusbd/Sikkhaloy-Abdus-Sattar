using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Classes;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

internal sealed class QueuedApiPost
{
    public string Url { get; set; } = "";
    public string Kind { get; set; } = "accounts";
    public string BodyJson { get; set; } = "{}";
}

internal sealed class OfflineQueueResult
{
    public string? ReceiptNo { get; set; }
    public int Id { get; set; }
}

internal sealed class QueuedOfficeSms
{
    public string Kind { get; set; } = "office";
    public string? Phones { get; set; }
    public string? Text { get; set; }
}

internal sealed class LocalPayOrderHint
{
    public int LocalId { get; set; }
    public string StudentCode { get; set; } = "";
    public int RoleID { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public int? ServerId { get; set; }
}

internal sealed partial class OfflineApiStore
{
    public const string UnpaidAllKey = "api/sync/accounts/payorder/unpaid?classId=0&roleId=0";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDbContextFactory<Sikkhaloy.LocalData.LocalDbContext> _dbFactory;

    public OfflineApiStore(IDbContextFactory<Sikkhaloy.LocalData.LocalDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public static bool IsOffline(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is HttpRequestException http
                && http.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return false;
            if (current is SocketException)
                return true;
            if (current is HttpRequestException { StatusCode: null })
                return true;
            if (current is TaskCanceledException or TimeoutException)
                return true;
            var msg = current.Message ?? "";
            if (msg.Contains("No such host", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("Failed to connect", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("A connection attempt failed", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("network is unreachable", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("The remote name could not be resolved", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool CanQueue(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        if (url.Contains("authority/", StringComparison.OrdinalIgnoreCase))
            return false;
        if (url.Contains("/sms", StringComparison.OrdinalIgnoreCase))
            return false;
        return url.StartsWith("api/sync/", StringComparison.OrdinalIgnoreCase);
    }

    public async Task SaveAsync(string key, string json, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key) || json is null)
            return;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.ApiSnapshots.FindAsync([key], cancellationToken);
        if (row is null)
        {
            db.ApiSnapshots.Add(new CachedApiSnapshot
            {
                CacheKey = key,
                PayloadJson = json,
                PulledUtc = DateTime.UtcNow
            });
        }
        else
        {
            row.PayloadJson = json;
            row.PulledUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> ReadRawAsync(string key, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.ApiSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken);
        return row?.PayloadJson;
    }

    public async Task<T?> ReadAsync<T>(string key, CancellationToken cancellationToken)
    {
        var json = await ReadRawAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task<OfflineQueueResult> EnqueueAsync(string url, string kind, string bodyJson, CancellationToken cancellationToken)
    {
        var result = new OfflineQueueResult();
        if (string.Equals(url, "api/sync/accounts/collect", StringComparison.OrdinalIgnoreCase))
            result.ReceiptNo = await ApplyCollectToCacheAsync(bodyJson, cancellationToken);
        else if (string.Equals(url, "api/sync/accounts/payorder", StringComparison.OrdinalIgnoreCase))
            await ApplyPayOrderToCacheAsync(bodyJson, cancellationToken);
        else if (string.Equals(url, "api/sync/accounts/add-more", StringComparison.OrdinalIgnoreCase))
            await ApplyAddMoreToCacheAsync(bodyJson, cancellationToken);
        else if (string.Equals(url, "api/sync/inventory/sales", StringComparison.OrdinalIgnoreCase))
            result.Id = await ApplyInventorySaleToCacheAsync(bodyJson, cancellationToken);
        else if (kind == "exam")
            result.Id = await ApplyExamWriteToCacheAsync(url, bodyJson, cancellationToken);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.Outbox.Add(new OutboxEntry
        {
            LocalId = Guid.NewGuid(),
            EntityType = EntityTypes.ApiCall,
            Operation = SyncOperation.Create,
            PayloadJson = JsonSerializer.Serialize(new QueuedApiPost
            {
                Url = url,
                Kind = kind,
                BodyJson = bodyJson
            }, JsonOptions),
            CreatedUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<List<OutboxEntry>> LoadQueuedAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Outbox
            .Where(x => x.EntityType == EntityTypes.ApiCall)
            .OrderBy(x => x.OutboxId)
            .Take(40)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(long outboxId, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Outbox.FirstOrDefaultAsync(x => x.OutboxId == outboxId, cancellationToken);
        if (row is null)
            return;
        db.Outbox.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkErrorAsync(long outboxId, string? error, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Outbox.FirstOrDefaultAsync(x => x.OutboxId == outboxId, cancellationToken);
        if (row is null)
            return;
        row.AttemptCount++;
        row.LastError = error;
        await db.SaveChangesAsync(cancellationToken);
    }

    public static QueuedApiPost? Parse(OutboxEntry entry)
    {
        try
        {
            return JsonSerializer.Deserialize<QueuedApiPost>(entry.PayloadJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<(int SchoolId, int YearId)?> CurrentScopeAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Sessions.AsNoTracking()
            .OrderByDescending(x => x.CachedUtc)
            .FirstOrDefaultAsync(cancellationToken);
        return row is null || row.SchoolID <= 0 ? null : (row.SchoolID, row.EducationYearID);
    }

    public async Task<ClassStructureDto> ReadClassStructureAsync(CancellationToken cancellationToken)
    {
        var scope = await CurrentScopeAsync(cancellationToken);
        if (scope is null)
            return new ClassStructureDto();

        var schoolId = scope.Value.SchoolId;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return new ClassStructureDto
        {
            Classes = await db.Classes.AsNoTracking()
                .Where(x => x.SchoolID == schoolId && x.ClassID > 0)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new SchoolClassDto
                {
                    LocalId = x.LocalId,
                    ClassID = x.ClassID,
                    Name = x.Name,
                    SortOrder = x.SortOrder,
                    SyncStatus = x.SyncStatus
                })
                .ToListAsync(cancellationToken),
            Groups = await db.ClassGroups.AsNoTracking()
                .Where(x => x.SchoolID == schoolId)
                .OrderBy(x => x.Name)
                .Select(x => new ClassPartDto
                {
                    LocalId = x.LocalId,
                    ServerId = x.SubjectGroupID,
                    ClassID = x.ClassID,
                    Name = x.Name,
                    SyncStatus = x.SyncStatus
                })
                .ToListAsync(cancellationToken),
            Sections = await db.ClassSections.AsNoTracking()
                .Where(x => x.SchoolID == schoolId)
                .OrderBy(x => x.Name)
                .Select(x => new ClassPartDto
                {
                    LocalId = x.LocalId,
                    ServerId = x.SectionID,
                    ClassID = x.ClassID,
                    Name = x.Name,
                    SyncStatus = x.SyncStatus
                })
                .ToListAsync(cancellationToken),
            Shifts = await db.ClassShifts.AsNoTracking()
                .Where(x => x.SchoolID == schoolId)
                .OrderBy(x => x.Name)
                .Select(x => new ClassPartDto
                {
                    LocalId = x.LocalId,
                    ServerId = x.ShiftID,
                    ClassID = x.ClassID,
                    Name = x.Name,
                    SyncStatus = x.SyncStatus
                })
                .ToListAsync(cancellationToken),
            Joins = await db.ClassJoins.AsNoTracking()
                .Where(x => x.SchoolID == schoolId)
                .OrderBy(x => x.JoinID)
                .Select(x => new ClassJoinDto
                {
                    LocalId = x.LocalId,
                    JoinID = x.JoinID,
                    ClassID = x.ClassID,
                    SubjectGroupID = x.SubjectGroupID,
                    SectionID = x.SectionID,
                    ShiftID = x.ShiftID,
                    GroupName = x.GroupName,
                    SectionName = x.SectionName,
                    ShiftName = x.ShiftName,
                    SyncStatus = x.SyncStatus
                })
                .ToListAsync(cancellationToken)
        };
    }

    public async Task<List<int>> LocalClassIdsAsync(CancellationToken cancellationToken)
    {
        var scope = await CurrentScopeAsync(cancellationToken);
        if (scope is null)
            return [];
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Classes.AsNoTracking()
            .Where(x => x.SchoolID == scope.Value.SchoolId && x.ClassID > 0)
            .Select(x => x.ClassID)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LocalStudent>> LocalStudentsAsync(int? classId, CancellationToken cancellationToken)
    {
        var scope = await CurrentScopeAsync(cancellationToken);
        if (scope is null)
            return [];
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.Students.AsNoTracking()
            .Where(x => x.SchoolID == scope.Value.SchoolId && x.EducationYearID == scope.Value.YearId);
        if (classId is > 0)
            query = query.Where(x => x.ClassID == classId);
        return await query.OrderBy(x => x.StudentsName).ToListAsync(cancellationToken);
    }

    public async Task<LocalStudent?> FindLocalStudentAsync(string code, CancellationToken cancellationToken)
    {
        var trimmed = (code ?? "").Trim();
        if (trimmed.Length == 0)
            return null;
        var scope = await CurrentScopeAsync(cancellationToken);
        if (scope is null)
            return null;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Students.AsNoTracking().FirstOrDefaultAsync(
            x => x.SchoolID == scope.Value.SchoolId
                 && x.EducationYearID == scope.Value.YearId
                 && x.StudentCode.ToLower() == trimmed.ToLower(),
            cancellationToken);
    }

    public async Task<List<PayOrderStudentDto>> LocalPayOrderStudentsAsync(int classId, CancellationToken cancellationToken)
    {
        var rows = await LocalStudentsAsync(classId > 0 ? classId : null, cancellationToken);
        return rows.Select(ToPayOrder).ToList();
    }

    public async Task<List<FeeSuggestDto>> SuggestLocalAsync(string query, CancellationToken cancellationToken)
    {
        var q = (query ?? "").Trim().ToLower();
        if (q.Length == 0)
            return [];
        var rows = await LocalStudentsAsync(null, cancellationToken);
        return rows
            .Where(x => x.StudentCode.ToLower().Contains(q) || x.StudentsName.ToLower().Contains(q))
            .Take(20)
            .Select(x => new FeeSuggestDto
            {
                ID = x.StudentCode,
                Name = x.StudentsName,
                ClassName = x.ClassName
            })
            .ToList();
    }

    public async Task<FeeStudentBundleDto?> BundleFromLocalAsync(string id, CancellationToken cancellationToken)
    {
        var student = await FindLocalStudentAsync(id, cancellationToken);
        if (student is null)
            return null;

        var unpaid = await ReadAsync<List<UnpaidPayOrderDto>>(UnpaidAllKey, cancellationToken) ?? [];
        var dues = unpaid
            .Where(x => string.Equals(x.ID, student.StudentCode, StringComparison.OrdinalIgnoreCase)
                        || (student.ServerId is > 0 && x.StudentID == student.ServerId))
            .Select(ToDue)
            .ToList();

        return new FeeStudentBundleDto
        {
            Student = ToFeeStudent(student),
            CurrentDues = dues,
            CurrentDue = dues.Sum(x => x.Due)
        };
    }

    public async Task<List<StudentManualRowDto>> LocalManualRowsAsync(int classId, int groupId, int sectionId, int shiftId, CancellationToken cancellationToken)
    {
        var rows = await LocalStudentsAsync(classId, cancellationToken);
        return rows
            .Where(x => groupId <= 0 || x.SubjectGroupID == groupId)
            .Where(x => sectionId <= 0 || x.SectionID == sectionId)
            .Where(x => shiftId <= 0 || x.ShiftID == shiftId)
            .Select(x => new StudentManualRowDto
            {
                StudentID = x.ServerId ?? 0,
                StudentClassID = x.StudentClassServerId ?? 0,
                ClassID = x.ClassID ?? classId,
                ID = x.StudentCode,
                Name = x.StudentsName,
                RollNo = x.RollNo,
                Phone = x.SMSPhoneNo,
                Attendance = "Pre"
            })
            .ToList();
    }

    public StudentLeavePersonDto ToLeavePerson(LocalStudent student) => new()
    {
        StudentID = student.ServerId ?? 0,
        ID = student.StudentCode,
        Name = student.StudentsName,
        ClassName = student.ClassName,
        FathersName = student.FathersName,
        Phone = student.SMSPhoneNo,
        Gender = student.Gender,
        Section = student.SectionName,
        GroupName = student.GroupName,
        Shift = student.ShiftName
    };

    public async Task<string?> RemapPayOrderBodyAsync(string bodyJson, CancellationToken cancellationToken)
    {
        CreatePayOrdersRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<CreatePayOrdersRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return bodyJson;
        }

        if (request is null)
            return bodyJson;

        var codes = request.StudentIDs
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var classIds = request.StudentClassIDs.Where(x => x > 0).Distinct().ToList();
        if (codes.Count == 0)
            return classIds.Count > 0 ? JsonSerializer.Serialize(request, JsonOptions) : null;

        var resolved = new List<int>();
        foreach (var code in codes)
        {
            var student = await FindLocalStudentAsync(code, cancellationToken);
            if (student?.StudentClassServerId is > 0)
            {
                resolved.Add(student.StudentClassServerId.Value);
                continue;
            }

            return null;
        }

        request.StudentClassIDs = resolved.Distinct().ToList();
        return JsonSerializer.Serialize(request, JsonOptions);
    }

    private async Task<string?> ApplyCollectToCacheAsync(string bodyJson, CancellationToken cancellationToken)
    {
        CollectPaymentRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<CollectPaymentRequest>(bodyJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (request is null || request.Items.Count == 0)
            return null;

        var paid = request.Items.ToDictionary(x => x.PayOrderID, x => x.PaidAmount);
        var paidTotal = request.Items.Sum(x => x.PaidAmount);
        var now = DateTime.Now;
        var paidDate = request.PaidDate ?? now;
        var receiptNo = $"OFF-{now:yyyyMMdd-HHmmssfff}";
        var receiptId = unchecked((int)(now.Ticks & 0x7FFFFFFF));
        if (receiptId == 0) receiptId = 1;
        receiptId = -receiptId;

        var unpaid = await ReadAsync<List<UnpaidPayOrderDto>>(UnpaidAllKey, cancellationToken);
        var scope = await CurrentScopeAsync(cancellationToken);
        LocalStudent? student = null;
        if (scope is not null && request.StudentID > 0)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            student = await db.Students.AsNoTracking().FirstOrDefaultAsync(
                x => x.SchoolID == scope.Value.SchoolId && x.ServerId == request.StudentID,
                cancellationToken);
        }

        if (student is null && !string.IsNullOrWhiteSpace(request.StudentCode))
            student = await FindLocalStudentAsync(request.StudentCode, cancellationToken);

        var code = student?.StudentCode
            ?? (!string.IsNullOrWhiteSpace(request.StudentCode) ? request.StudentCode.Trim() : null)
            ?? unpaid?.FirstOrDefault(x => x.StudentID == request.StudentID)?.ID
            ?? "";
        var key = string.IsNullOrWhiteSpace(code)
            ? null
            : $"api/sync/accounts/students/bundle?id={Uri.EscapeDataString(code)}";
        var bundle = key is null ? null : await ReadAsync<FeeStudentBundleDto>(key, cancellationToken);
        if (bundle is null && !string.IsNullOrWhiteSpace(code))
            bundle = await BundleFromLocalAsync(code, cancellationToken);

        var allDues = bundle is null
            ? []
            : bundle.CurrentDues.Concat(bundle.OtherDues).Concat(bundle.InventoryDues).ToList();

        var cash = await ReadAsync<List<CashAccountDto>>("api/sync/accounts/cash", cancellationToken);
        var accountName = cash?.FirstOrDefault(x => x.AccountID == request.AccountID)?.AccountName;
        string? receivedBy;
        await using (var sessionDb = await _dbFactory.CreateDbContextAsync(cancellationToken))
        {
            var sess = await sessionDb.Sessions.AsNoTracking()
                .OrderByDescending(x => x.CachedUtc)
                .FirstOrDefaultAsync(cancellationToken);
            receivedBy = string.IsNullOrWhiteSpace(sess?.DisplayName) ? sess?.UserName : sess.DisplayName;
        }

        var lines = new List<ReceiptLineDto>();
        foreach (var row in allDues)
        {
            if (!paid.TryGetValue(row.PayOrderID, out var amount))
                continue;
            lines.Add(new ReceiptLineDto
            {
                PayOrderID = row.PayOrderID,
                Role = row.Role,
                PayFor = row.PayFor,
                YearName = row.YearName,
                Amount = row.Amount,
                Discount = row.Discount + row.LateFeeDiscount,
                PaidAmount = amount,
                Due = Math.Max(0, row.Due - amount)
            });
        }

        if (lines.Count == 0)
        {
            foreach (var item in request.Items)
            {
                var src = unpaid?.FirstOrDefault(x => x.PayOrderID == item.PayOrderID);
                lines.Add(new ReceiptLineDto
                {
                    PayOrderID = item.PayOrderID,
                    Role = src?.Role ?? "",
                    PayFor = src?.PayFor ?? "",
                    Amount = src?.Amount ?? item.PaidAmount,
                    PaidAmount = item.PaidAmount,
                    Due = Math.Max(0, (src?.Amount ?? item.PaidAmount) - item.PaidAmount)
                });
            }
        }

        var remainingDues = new List<ReceiptDueLineDto>();
        foreach (var row in allDues)
        {
            var due = row.Due;
            var extraPaid = 0m;
            if (paid.TryGetValue(row.PayOrderID, out var amount))
            {
                due = Math.Max(0, due - amount);
                extraPaid = amount;
            }
            if (due <= 0)
                continue;
            remainingDues.Add(new ReceiptDueLineDto
            {
                Role = row.Role,
                PayFor = row.PayFor,
                YearName = row.YearName,
                EndDate = row.EndDate,
                PaidAmount = row.PaidAmount + extraPaid,
                Due = due
            });
        }

        var studentDto = bundle?.Student ?? (student is null ? null : ToFeeStudent(student));
        if (studentDto is null)
        {
            var src = unpaid?.FirstOrDefault(x => x.StudentID == request.StudentID);
            if (src is not null)
            {
                studentDto = new FeeStudentDto
                {
                    StudentID = src.StudentID,
                    ID = src.ID,
                    Name = src.Name,
                    ClassName = src.ClassName
                };
            }
        }

        var detail = new ReceiptDetailDto
        {
            MoneyReceiptID = receiptId,
            ReceiptNo = receiptNo,
            PaidDate = paidDate,
            CollectionDate = now,
            TotalAmount = paidTotal,
            ReceivedBy = receivedBy,
            AccountName = accountName,
            Student = studentDto,
            Lines = lines,
            RemainingDues = remainingDues
        };
        await SaveAsync(
            $"api/sync/accounts/receipt?no={Uri.EscapeDataString(receiptNo)}",
            JsonSerializer.Serialize(detail, JsonOptions),
            cancellationToken);

        if (unpaid is not null)
        {
            unpaid.RemoveAll(x => paid.ContainsKey(x.PayOrderID));
            await SaveAsync(UnpaidAllKey, JsonSerializer.Serialize(unpaid, JsonOptions), cancellationToken);
        }

        if (bundle is not null && key is not null)
        {
            foreach (var row in bundle.CurrentDues.Concat(bundle.OtherDues).Concat(bundle.InventoryDues))
            {
                if (!paid.TryGetValue(row.PayOrderID, out var amount))
                    continue;
                row.PaidAmount += amount;
                row.Due = Math.Max(0, row.Due - amount);
            }

            bundle.CurrentDues.RemoveAll(x => x.Due <= 0);
            bundle.OtherDues.RemoveAll(x => x.Due <= 0);
            bundle.InventoryDues.RemoveAll(x => x.Due <= 0);
            foreach (var row in bundle.CurrentDues.Concat(bundle.OtherDues).Concat(bundle.InventoryDues))
            {
                row.PayNow = 0;
                row.Selected = false;
            }

            bundle.CurrentDue = bundle.CurrentDues.Sum(x => x.Due);
            bundle.Receipts.Insert(0, new ReceiptListDto
            {
                MoneyReceiptID = receiptId,
                ReceiptNo = receiptNo,
                TotalAmount = paidTotal,
                PaidDate = paidDate,
                CollectionDate = now,
                EducationYearID = request.EducationYearID,
                ReceivedBy = receivedBy
            });
            await SaveAsync(key, JsonSerializer.Serialize(bundle, JsonOptions), cancellationToken);
        }

        return receiptNo;
    }

    private static PayOrderStudentDto ToPayOrder(LocalStudent x) => new()
    {
        StudentID = x.ServerId ?? 0,
        StudentClassID = x.StudentClassServerId ?? 0,
        ClassID = x.ClassID ?? 0,
        ID = x.StudentCode,
        Name = x.StudentsName,
        RollNo = x.RollNo,
        IsNew = x.IsNew == true
    };

    private static FeeStudentDto ToFeeStudent(LocalStudent x) => new()
    {
        StudentID = x.ServerId ?? 0,
        StudentClassID = x.StudentClassServerId ?? 0,
        ClassID = x.ClassID ?? 0,
        EducationYearID = x.EducationYearID,
        ID = x.StudentCode,
        Name = x.StudentsName,
        ClassName = x.ClassName,
        RollNo = x.RollNo,
        Phone = x.SMSPhoneNo,
        FathersName = x.FathersName,
        Section = x.SectionName,
        Shift = x.ShiftName,
        Status = x.Status
    };

    private static DueRowDto ToDue(UnpaidPayOrderDto x)
    {
        var due = x.Amount;
        return new DueRowDto
        {
            PayOrderID = x.PayOrderID,
            RoleID = x.RoleID,
            Role = x.Role,
            PayFor = x.PayFor,
            ClassName = x.ClassName,
            Amount = x.Amount,
            Due = due,
            PayNow = 0,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Overdue = x.EndDate.Date < DateTime.Today,
            CurrentYear = true,
            Selected = false
        };
    }
}
