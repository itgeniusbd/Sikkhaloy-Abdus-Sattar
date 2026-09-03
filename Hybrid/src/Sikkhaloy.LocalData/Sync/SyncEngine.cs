using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Classes;
using Sikkhaloy.Shared.Menu;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

public sealed partial class SyncEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDbContextFactory<LocalDbContext> _dbFactory;
    private readonly ISyncApiClient _api;
    private readonly object _gate = new();
    private readonly SemaphoreSlim _runLock = new(1, 1);
    private int _failureCount;
    private int _pendingCount;
    private DateTime _nextAttemptUtc = DateTime.MinValue;
    private bool _online;

    public SyncEngine(IDbContextFactory<LocalDbContext> dbFactory, ISyncApiClient api)
    {
        _dbFactory = dbFactory;
        _api = api;
        _api.OfflineQueueChanged += () => _ = NotifyQueueAsync();
    }

    private async Task NotifyQueueAsync()
    {
        try
        {
            await RefreshPendingAsync(CancellationToken.None);
            StateChanged?.Invoke();
        }
        catch
        {
        }
    }

    public bool IsOnline => _online;
    public int PendingCount => _pendingCount;
    public string? LastError { get; private set; }
    public event Action? StateChanged;

    public async Task RunOnceAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken = default)
        => await RunOnceAsync(session, accessToken, force: false, cancellationToken);

    public async Task RunOnceAsync(SessionSnapshot session, string accessToken, bool force, CancellationToken cancellationToken = default)
    {
        await _runLock.WaitAsync(cancellationToken);
        try
        {
            await RunOnceCoreAsync(session, accessToken, cancellationToken);
        }
        finally
        {
            _runLock.Release();
        }
    }

    private async Task RunOnceCoreAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        LastError = null;
        if (session.IsAuthority || session.SchoolID <= 0)
        {
            _online = await _api.PingAsync(cancellationToken);
            await RefreshPendingAsync(cancellationToken);
            StateChanged?.Invoke();
            return;
        }

        _online = await _api.PingAsync(cancellationToken);
        if (!_online)
        {
            LastError = "sync.needOnline";
            RegisterFailure();
            await RefreshPendingAsync(cancellationToken);
            StateChanged?.Invoke();
            return;
        }

        try
        {
            await PushAsync(session, accessToken, cancellationToken);
            await PullAsync(session, accessToken, cancellationToken);
            await PullClassStructureAsync(session, accessToken, cancellationToken);
            await PullYearsAsync(session, accessToken, cancellationToken);
            await PullProfileAsync(session, accessToken, cancellationToken);
            await PullMenuAsync(session, accessToken, cancellationToken);
            await _api.FlushQueuedWritesAsync(accessToken, cancellationToken);
            await _api.WarmOfflineCacheAsync(accessToken, cancellationToken);
            _failureCount = 0;
            _nextAttemptUtc = DateTime.MinValue;
            LastError = null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            LastError = "login.expired";
            RegisterFailure();
        }
        catch (Exception ex)
        {
            LastError = string.IsNullOrWhiteSpace(ex.Message) ? "sync.failed" : ex.Message;
            RegisterFailure();
        }

        await RefreshPendingAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(LastError))
            LastError = await ReadOutboxErrorAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    public async Task MergeStudentPlacementsAsync(
        int schoolId,
        int educationYearId,
        IEnumerable<SmStudentRowDto> rows,
        CancellationToken cancellationToken = default)
    {
        var items = rows.Where(x => x.StudentID > 0 || x.StudentClassID > 0).ToList();
        if (items.Count == 0)
            return;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var students = await db.Students
            .Where(x => x.SchoolID == schoolId && x.EducationYearID == educationYearId)
            .ToListAsync(cancellationToken);
        if (students.Count == 0)
            return;

        var byClassId = students
            .Where(x => x.StudentClassServerId is > 0)
            .GroupBy(x => x.StudentClassServerId!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        var byStudentId = students
            .Where(x => x.ServerId is > 0)
            .GroupBy(x => x.ServerId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var changed = false;
        foreach (var item in items)
        {
            LocalStudent? row = null;
            if (item.StudentClassID > 0)
                byClassId.TryGetValue(item.StudentClassID, out row);
            if (row is null && item.StudentID > 0)
                byStudentId.TryGetValue(item.StudentID, out row);
            if (row is null)
                continue;

            var groupId = item.SubjectGroupID > 0 ? item.SubjectGroupID : (int?)null;
            var sectionId = item.SectionID > 0 ? item.SectionID : (int?)null;
            var shiftId = item.ShiftID > 0 ? item.ShiftID : (int?)null;
            var groupName = string.IsNullOrWhiteSpace(item.GroupName) ? null : item.GroupName;
            var sectionName = string.IsNullOrWhiteSpace(item.SectionName) ? null : item.SectionName;
            var shiftName = string.IsNullOrWhiteSpace(item.ShiftName) ? null : item.ShiftName;
            if (row.SubjectGroupID == groupId
                && row.SectionID == sectionId
                && row.ShiftID == shiftId
                && string.Equals(row.GroupName, groupName, StringComparison.Ordinal)
                && string.Equals(row.SectionName, sectionName, StringComparison.Ordinal)
                && string.Equals(row.ShiftName, shiftName, StringComparison.Ordinal))
                continue;

            row.SubjectGroupID = groupId;
            row.SectionID = sectionId;
            row.ShiftID = shiftId;
            row.GroupName = groupName;
            row.SectionName = sectionName;
            row.ShiftName = shiftName;
            if (item.StudentClassID > 0)
                row.StudentClassServerId = item.StudentClassID;
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveStudentAsync(StudentDto dto, SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTime.UtcNow;
        dto.SchoolID = session.SchoolID;
        dto.EducationYearID = session.EducationYearID;
        dto.RegistrationID = session.RegistrationID;
        dto.UpdatedUtc = now;

        LocalStudent? existing = null;
        if (dto.LocalId != Guid.Empty)
            existing = await db.Students.FindAsync(new object[] { dto.LocalId }, cancellationToken);
        if (existing is null && dto.ServerId is > 0)
        {
            existing = await db.Students.FirstOrDefaultAsync(
                x => x.SchoolID == session.SchoolID && x.ServerId == dto.ServerId,
                cancellationToken);
            if (existing is not null)
                dto.LocalId = existing.LocalId;
        }

        if (dto.LocalId == Guid.Empty)
            dto.LocalId = Guid.NewGuid();

        if (existing is not null)
        {
            if (dto.ServerId is null or <= 0)
                dto.ServerId = existing.ServerId;
            if (dto.StudentClassServerId is null or <= 0)
                dto.StudentClassServerId = existing.StudentClassServerId;
        }

        var operation = existing is null ? SyncOperation.Create : SyncOperation.Update;
        dto.SyncStatus = existing is null ? SyncStatus.PendingCreate : SyncStatus.PendingUpdate;

        if (existing is null)
        {
            var taken = await db.Students.AnyAsync(
                x => x.SchoolID == session.SchoolID
                     && x.StudentCode.ToLower() == dto.StudentCode.Trim().ToLower(),
                cancellationToken);
            if (taken)
                throw new InvalidOperationException("edit.id.exists");

            dto.IsNew ??= true;
            dto.AdmissionDate ??= DateTime.Today;
            db.Students.Add(Map(dto, session.DeviceId));
        }
        else
        {
            var taken = await db.Students.AnyAsync(
                x => x.SchoolID == session.SchoolID
                     && x.LocalId != existing.LocalId
                     && x.StudentCode.ToLower() == dto.StudentCode.Trim().ToLower(),
                cancellationToken);
            if (taken)
                throw new InvalidOperationException("edit.id.exists");
            Copy(dto, existing, session.DeviceId);
        }

        var payload = JsonSerializer.Serialize(dto, JsonOptions);
        var pendingCreate = await db.Outbox.FirstOrDefaultAsync(
            x => x.LocalId == dto.LocalId
                 && x.EntityType == EntityTypes.Student
                 && x.Operation == SyncOperation.Create,
            cancellationToken);
        if (pendingCreate is not null)
        {
            pendingCreate.PayloadJson = payload;
            pendingCreate.CreatedUtc = now;
        }
        else
        {
            db.Outbox.Add(new OutboxEntry
            {
                LocalId = dto.LocalId,
                EntityType = EntityTypes.Student,
                Operation = operation,
                PayloadJson = payload,
                CreatedUtc = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    public async Task<IReadOnlyList<StudentDto>> ListStudentsAsync(int schoolId, int educationYearId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var rows = await db.Students.AsNoTracking()
            .Where(x => x.SchoolID == schoolId && x.EducationYearID == educationYearId)
            .OrderBy(x => x.StudentsName)
            .ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task<StudentDto?> GetStudentAsync(Guid localId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LocalId == localId, cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task<StudentDto?> FindStudentByCodeAsync(
        int schoolId, int educationYearId, string studentCode, CancellationToken cancellationToken = default)
    {
        var code = studentCode.Trim();
        if (code.Length == 0)
            return null;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Students.AsNoTracking().FirstOrDefaultAsync(
            x => x.SchoolID == schoolId
                 && x.EducationYearID == educationYearId
                 && x.StudentCode.ToLower() == code.ToLower(),
            cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task<StudentIdCheckResult> CheckStudentIdAsync(
        int schoolId,
        string studentCode,
        Guid? exceptLocalId,
        int? exceptServerId,
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        var result = new StudentIdCheckResult();
        var code = studentCode.Trim();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        result.LastEntryId = await db.Students.AsNoTracking()
            .Where(x => x.SchoolID == schoolId && x.StudentCode != "")
            .OrderByDescending(x => x.ServerId ?? 0)
            .ThenByDescending(x => x.UpdatedUtc)
            .Select(x => x.StudentCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (code.Length > 0)
        {
            var except = exceptLocalId ?? Guid.Empty;
            result.Exists = await db.Students.AsNoTracking().AnyAsync(
                x => x.SchoolID == schoolId
                     && x.LocalId != except
                     && x.StudentCode.ToLower() == code.ToLower(),
                cancellationToken);

            result.Suggestions = await db.Students.AsNoTracking()
                .Where(x => x.SchoolID == schoolId && x.StudentCode.ToLower().StartsWith(code.ToLower()))
                .Select(x => x.StudentCode)
                .Distinct()
                .OrderBy(x => x)
                .Take(10)
                .ToListAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            try
            {
                var remote = await _api.CheckStudentIdAsync(accessToken, code, exceptServerId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(remote.LastEntryId))
                    result.LastEntryId = remote.LastEntryId;
                if (code.Length > 0)
                    result.Exists = result.Exists || remote.Exists;
                if (remote.Suggestions.Count > 0)
                {
                    result.Suggestions = result.Suggestions
                        .Concat(remote.Suggestions)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x)
                        .Take(10)
                        .ToList();
                }
            }
            catch (Exception)
            {
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<SchoolClassDto>> ListClassesAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Classes.AsNoTracking()
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
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassPartDto>> ListSectionsAsync(int schoolId, int classId, CancellationToken cancellationToken = default)
    {
        if (classId <= 0)
            return [];
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.ClassSections.AsNoTracking()
            .Where(x => x.SchoolID == schoolId && x.ClassID == classId)
            .OrderBy(x => x.Name)
            .Select(x => new ClassPartDto { ServerId = x.SectionID, ClassID = x.ClassID, Name = x.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassPartDto>> ListShiftsAsync(int schoolId, int classId, CancellationToken cancellationToken = default)
    {
        if (classId <= 0)
            return [];
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.ClassShifts.AsNoTracking()
            .Where(x => x.SchoolID == schoolId && x.ClassID == classId)
            .OrderBy(x => x.Name)
            .Select(x => new ClassPartDto { ServerId = x.ShiftID, ClassID = x.ClassID, Name = x.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassPartDto>> ListGroupsAsync(int schoolId, int classId, CancellationToken cancellationToken = default)
    {
        if (classId <= 0)
            return [];
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.ClassGroups.AsNoTracking()
            .Where(x => x.SchoolID == schoolId && x.ClassID == classId)
            .OrderBy(x => x.Name)
            .Select(x => new ClassPartDto { ServerId = x.SubjectGroupID, ClassID = x.ClassID, Name = x.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task RefreshClassesAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return;
        try
        {
            await PullClassStructureAsync(session, accessToken, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    public async Task<IReadOnlyList<EducationYearDto>> ListYearsAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.EducationYears.AsNoTracking()
            .Where(x => x.SchoolID == schoolId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new EducationYearDto
            {
                EducationYearID = x.EducationYearID,
                Name = x.Name,
                SortOrder = x.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task RefreshOfficeAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return;
        try
        {
            await PullYearsAsync(session, accessToken, cancellationToken);
            await PullProfileAsync(session, accessToken, cancellationToken);
            await PullMenuAsync(session, accessToken, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    public async Task SwitchYearLocalAsync(SessionSnapshot session, int educationYearId, CancellationToken cancellationToken = default)
    {
        session.EducationYearID = educationYearId;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var cached = await db.Sessions.FirstOrDefaultAsync(x => x.UserName == session.UserName, cancellationToken);
        if (cached is null)
            return;
        cached.EducationYearID = educationYearId;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetYearPullAsync(SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var watermark = await db.YearWatermarks.FindAsync(
            new object[] { session.SchoolID, session.EducationYearID }, cancellationToken);
        if (watermark is not null)
            watermark.LastChangeId = 0;

        var stale = await db.Students
            .Where(x => x.SchoolID == session.SchoolID
                && x.EducationYearID == session.EducationYearID
                && x.SyncStatus == SyncStatus.Synced)
            .ToListAsync(cancellationToken);
        db.Students.RemoveRange(stale);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task PullYearsAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        var items = await _api.GetYearsAsync(accessToken, cancellationToken);
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.EducationYears.Where(x => x.SchoolID == session.SchoolID).ToListAsync(cancellationToken);
        db.EducationYears.RemoveRange(existing);
        foreach (var item in items)
        {
            db.EducationYears.Add(new LocalEducationYear
            {
                EducationYearID = item.EducationYearID,
                SchoolID = session.SchoolID,
                Name = item.Name,
                SortOrder = item.SortOrder
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task PullProfileAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        var profile = await _api.GetProfileAsync(accessToken, cancellationToken);
        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
            session.DisplayName = profile.DisplayName;
        if (!string.IsNullOrWhiteSpace(profile.SchoolName))
            session.SchoolName = profile.SchoolName;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var cached = await db.Sessions.FirstOrDefaultAsync(x => x.UserName == session.UserName, cancellationToken);
        if (cached is not null)
        {
            if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                cached.DisplayName = profile.DisplayName;
            if (!string.IsNullOrWhiteSpace(profile.SchoolName))
                cached.SchoolName = profile.SchoolName;
            await db.SaveChangesAsync(cancellationToken);
        }

        SaveSchoolHeader(session.SchoolID, profile);

        if (string.IsNullOrWhiteSpace(profile.PhotoBase64))
            return;

        var path = ProfilePhotoPath(session.RegistrationID);
        await File.WriteAllBytesAsync(path, Convert.FromBase64String(profile.PhotoBase64), cancellationToken);
    }

    public string? GetProfilePhotoDataUrl(int registrationId)
    {
        return ReadImageDataUrl(ProfilePhotoPath(registrationId));
    }

    public void SaveProfilePhoto(int registrationId, string? dataUrl)
    {
        if (registrationId <= 0 || string.IsNullOrWhiteSpace(dataUrl))
            return;
        try
        {
            var bytes = DecodeImageBytes(dataUrl);
            if (bytes.Length == 0)
                return;
            File.WriteAllBytes(ProfilePhotoPath(registrationId), bytes);
        }
        catch
        {
        }
    }

    public string? GetSchoolLogoDataUrl(int schoolId) =>
        ReadImageDataUrl(SchoolLogoPath(schoolId));

    public string? GetSchoolNameLogoDataUrl(int schoolId) =>
        ReadImageDataUrl(SchoolNameLogoPath(schoolId));

    public OfficeProfileDto GetSchoolHeader(int schoolId) => ReadSchoolHeader(schoolId);

    private static OfficeProfileDto ReadSchoolHeader(int schoolId)
    {
        var path = SchoolHeaderPath(schoolId);
        if (!File.Exists(path))
            return new OfficeProfileDto { SchoolID = schoolId };

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<OfficeProfileDto>(json, JsonOptions)
                   ?? new OfficeProfileDto { SchoolID = schoolId };
        }
        catch (Exception)
        {
            return new OfficeProfileDto { SchoolID = schoolId };
        }
    }

    public async Task<MenuTreeDto> GetMenuAsync(string userName, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Menus.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
        if (row is null || string.IsNullOrWhiteSpace(row.PayloadJson))
            return new MenuTreeDto();

        var tree = JsonSerializer.Deserialize<MenuTreeDto>(row.PayloadJson, JsonOptions) ?? new MenuTreeDto();
        foreach (var category in tree.Categories)
        {
            foreach (var link in category.Links)
                HybridMenuRoutes.Apply(link);
            foreach (var sub in category.Subs)
            {
                foreach (var link in sub.Links)
                    HybridMenuRoutes.Apply(link);
            }
        }

        HybridMenuRoutes.Deduplicate(tree);
        return tree;
    }

    public async Task<MenuLinkDto?> FindMenuLinkAsync(string userName, int linkId, CancellationToken cancellationToken = default)
    {
        var tree = await GetMenuAsync(userName, cancellationToken);
        foreach (var category in tree.Categories)
        {
            var direct = category.Links.FirstOrDefault(x => x.LinkID == linkId);
            if (direct is not null)
                return direct;
            foreach (var sub in category.Subs)
            {
                var nested = sub.Links.FirstOrDefault(x => x.LinkID == linkId);
                if (nested is not null)
                    return nested;
            }
        }

        return null;
    }

    private async Task PullMenuAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        var tree = await _api.GetMenuAsync(accessToken, cancellationToken);
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.Menus.FindAsync(new object[] { session.UserName }, cancellationToken);
        var json = JsonSerializer.Serialize(tree);
        if (existing is null)
        {
            db.Menus.Add(new CachedMenu
            {
                UserName = session.UserName,
                PayloadJson = json,
                PulledUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.PayloadJson = json;
            existing.PulledUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string ProfilePhotoPath(int registrationId) =>
        Path.Combine(HybridFolder(), $"admin-{registrationId}.jpg");

    private static string SchoolLogoPath(int schoolId) =>
        Path.Combine(HybridFolder(), $"school-{schoolId}.img");

    private static string SchoolNameLogoPath(int schoolId) =>
        Path.Combine(HybridFolder(), $"school-{schoolId}-name.img");

    private static string SchoolHeaderPath(int schoolId) =>
        Path.Combine(HybridFolder(), $"school-{schoolId}.json");

    private static string HybridFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SIKKHALOY",
            "Hybrid");
        Directory.CreateDirectory(folder);
        return folder;
    }

    public void RefreshSchoolHeader(OfficeProfileDto profile)
    {
        if (profile.SchoolID <= 0)
            return;
        SaveSchoolHeader(profile.SchoolID, profile);
    }

    public void SaveHeaderColor(int schoolId, string color)
    {
        if (schoolId <= 0)
            return;
        var header = GetSchoolHeader(schoolId);
        header.HeaderColor = color;
        File.WriteAllText(SchoolHeaderPath(schoolId), JsonSerializer.Serialize(header));
    }

    private static void SaveSchoolHeader(int schoolId, OfficeProfileDto profile)
    {
        var existingColor = ReadSchoolHeader(schoolId).HeaderColor;
        var header = new OfficeProfileDto
        {
            SchoolID = schoolId,
            SchoolName = profile.SchoolName ?? "",
            Address = profile.Address ?? "",
            Phone = profile.Phone ?? "",
            Email = profile.Email ?? "",
            HeaderColor = string.IsNullOrWhiteSpace(profile.HeaderColor) ? existingColor : profile.HeaderColor
        };
        File.WriteAllText(SchoolHeaderPath(schoolId), JsonSerializer.Serialize(header));

        if (!string.IsNullOrWhiteSpace(profile.LogoBase64))
        {
            try
            {
                File.WriteAllBytes(SchoolLogoPath(schoolId), DecodeImageBytes(profile.LogoBase64));
            }
            catch (FormatException)
            {
            }
        }

        var namePath = SchoolNameLogoPath(schoolId);
        if (profile.ClearNameLogo)
        {
            if (File.Exists(namePath))
                File.Delete(namePath);
            return;
        }
        if (string.IsNullOrWhiteSpace(profile.NameLogoBase64))
            return;
        try
        {
            File.WriteAllBytes(namePath, DecodeImageBytes(profile.NameLogoBase64));
        }
        catch (FormatException)
        {
        }
    }

    private static string? ReadImageDataUrl(string path)
    {
        if (!File.Exists(path))
            return null;
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
            return null;
        return $"data:{ImageMime(bytes)};base64,{Convert.ToBase64String(bytes)}";
    }

    private static byte[] DecodeImageBytes(string raw)
    {
        var comma = raw.IndexOf(',');
        var payload = comma >= 0 && raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? raw[(comma + 1)..]
            : raw;
        return Convert.FromBase64String(payload);
    }

    private static string ImageMime(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50)
            return "image/png";
        if (bytes.Length >= 6 && bytes[0] == 0x47 && bytes[1] == 0x49)
            return "image/gif";
        return "image/jpeg";
    }

    public async Task<DashboardStats> GetDashboardAsync(int schoolId, int educationYearId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.Students.AsNoTracking()
            .Where(x => x.SchoolID == schoolId && x.EducationYearID == educationYearId)
            .Where(x => string.IsNullOrWhiteSpace(x.Status)
                || x.Status == "Active");

        var students = await query.ToListAsync(cancellationToken);
        var today = DateTime.Today;
        return new DashboardStats
        {
            TotalStudents = students.Count,
            MaleCount = students.Count(IsMale),
            FemaleCount = students.Count(IsFemale),
            NewCount = students.Count(x => x.IsNew == true),
            OldCount = students.Count(x => x.IsNew != true),
            ClassCount = students
                .Select(x => x.ClassID ?? 0)
                .Distinct()
                .Count(x => x > 0),
            PendingSync = await db.Outbox.CountAsync(cancellationToken),
            Classes = students
                .GroupBy(x => new { x.ClassID, Name = string.IsNullOrWhiteSpace(x.ClassName) ? "—" : x.ClassName.Trim() })
                .OrderBy(g => g.Key.ClassID ?? int.MaxValue)
                .ThenBy(g => g.Key.Name)
                .Select(g => new DashboardClassRowDto
                {
                    ClassName = g.Key.Name,
                    NewCount = g.Count(x => x.IsNew == true),
                    OldCount = g.Count(x => x.IsNew != true)
                })
                .ToList(),
            BloodGroups = students
                .Select(x => (x.BloodGroup ?? "").Trim())
                .Where(x => x.Length > 0)
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => new DashboardNamedCountDto { Name = g.First(), Count = g.Count() })
                .ToList(),
            BirthdaysToday = students
                .Where(x => IsBirthdayOn(x.DateofBirth, today))
                .OrderBy(x => AgeYears(x.DateofBirth, today))
                .ThenBy(x => x.StudentsName)
                .Select(Map)
                .ToList(),
            BirthdaysUpcoming = students
                .Where(x => IsUpcomingBirthday(x.DateofBirth, today, 7))
                .OrderBy(x => NextBirthday(x.DateofBirth, today))
                .ThenBy(x => x.StudentsName)
                .Select(Map)
                .ToList(),
            Recent = students
                .OrderByDescending(x => x.UpdatedUtc)
                .Take(8)
                .Select(Map)
                .ToList()
        };
    }

    private async Task PushAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var pending = await db.Outbox
            .Where(x => x.EntityType != EntityTypes.ApiCall && x.EntityType != EntityTypes.PendingSms)
            .OrderBy(x => x.OutboxId)
            .Take(80)
            .ToListAsync(cancellationToken);
        if (pending.Count == 0)
            return;

        var classCreates = pending
            .Where(x => x.EntityType == EntityTypes.Class && x.Operation == SyncOperation.Create)
            .ToList();
        if (classCreates.Count > 0)
        {
            await PushBatchAsync(db, session, accessToken, classCreates, cancellationToken);
            pending = await db.Outbox
                .Where(x => x.EntityType != EntityTypes.ApiCall && x.EntityType != EntityTypes.PendingSms)
                .OrderBy(x => x.OutboxId)
                .Take(80)
                .ToListAsync(cancellationToken);
        }

        var parts = pending
            .Where(x => x.EntityType is EntityTypes.ClassGroup or EntityTypes.ClassSection or EntityTypes.ClassShift)
            .ToList();
        if (parts.Count > 0)
        {
            await PushBatchAsync(db, session, accessToken, parts, cancellationToken);
            pending = await db.Outbox
                .Where(x => x.EntityType != EntityTypes.ApiCall && x.EntityType != EntityTypes.PendingSms)
                .OrderBy(x => x.OutboxId)
                .Take(80)
                .ToListAsync(cancellationToken);
        }

        if (pending.Count > 0)
            await PushBatchAsync(db, session, accessToken, pending, cancellationToken);
    }

    private async Task PushBatchAsync(
        LocalDbContext db,
        SessionSnapshot session,
        string accessToken,
        List<OutboxEntry> batch,
        CancellationToken cancellationToken)
    {
        var studentIds = batch.Select(x => x.LocalId).ToList();
        var serverIds = await db.Students.AsNoTracking()
            .Where(x => studentIds.Contains(x.LocalId) && x.ServerId != null)
            .ToDictionaryAsync(x => x.LocalId, x => x.ServerId, cancellationToken);
        var request = new PushRequest
        {
            DeviceId = session.DeviceId,
            Changes = batch.Select(x => new SyncChangeDto
            {
                LocalId = x.LocalId,
                EntityType = x.EntityType,
                Operation = x.Operation,
                ServerId = serverIds.GetValueOrDefault(x.LocalId),
                UpdatedUtc = x.CreatedUtc,
                PayloadJson = x.PayloadJson
            }).ToList()
        };

        var response = await _api.PushAsync(accessToken, request, cancellationToken);
        foreach (var result in response.Results)
        {
            var entry = batch.FirstOrDefault(x => x.LocalId == result.LocalId);
            if (entry is null)
                continue;

            var student = await db.Students.FirstOrDefaultAsync(x => x.LocalId == result.LocalId, cancellationToken);
            if (result.Succeeded)
            {
                if (student is not null)
                {
                    student.ServerId = result.ServerId;
                    student.SyncStatus = SyncStatus.Synced;
                    student.UpdatedUtc = DateTime.UtcNow;
                }

                await ApplyClassStructurePushAsync(db, entry, result, cancellationToken);
                db.Outbox.Remove(entry);
            }
            else
            {
                entry.AttemptCount++;
                entry.LastError = result.Error;
                if (student is not null && result.IsConflict)
                    student.SyncStatus = SyncStatus.Conflict;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task PullAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        var pages = 0;
        bool hasMore;
        do
        {
            hasMore = await PullPageAsync(session, accessToken, cancellationToken);
            pages++;
        } while (hasMore && pages < 200);
    }

    private async Task<bool> PullPageAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var watermark = await db.YearWatermarks.FindAsync(new object[] { session.SchoolID, session.EducationYearID }, cancellationToken);
        var since = watermark?.LastChangeId ?? 0;
        var pull = await _api.PullAsync(accessToken, since, cancellationToken);

        foreach (var change in pull.Changes)
        {
            if (!string.Equals(change.EntityType, EntityTypes.Student, StringComparison.OrdinalIgnoreCase))
                continue;

            var dto = JsonSerializer.Deserialize<StudentDto>(change.PayloadJson, JsonOptions);
            if (dto is null)
                continue;

            var local = await db.Students.FirstOrDefaultAsync(
                x => x.LocalId == dto.LocalId
                     || (dto.ServerId != null && x.ServerId == dto.ServerId && x.EducationYearID == dto.EducationYearID)
                     || (x.SchoolID == dto.SchoolID
                         && x.EducationYearID == dto.EducationYearID
                         && x.StudentCode.ToLower() == dto.StudentCode.ToLower()),
                cancellationToken);

            if (local is not null && local.SyncStatus is SyncStatus.PendingCreate or SyncStatus.PendingUpdate)
            {
                if (local.UpdatedUtc > dto.UpdatedUtc)
                    continue;
            }

            if (local is null)
            {
                if (dto.LocalId == Guid.Empty)
                    dto.LocalId = Guid.NewGuid();
                dto.SyncStatus = SyncStatus.Synced;
                db.Students.Add(Map(dto, session.DeviceId));
            }
            else
            {
                dto.LocalId = local.LocalId;
                dto.SyncStatus = SyncStatus.Synced;
                Copy(dto, local, session.DeviceId);
            }
        }

        if (watermark is null)
        {
            db.YearWatermarks.Add(new YearWatermark
            {
                SchoolID = session.SchoolID,
                EducationYearID = session.EducationYearID,
                LastChangeId = pull.Watermark,
                PulledUtc = DateTime.UtcNow
            });
        }
        else
        {
            watermark.LastChangeId = pull.Watermark;
            watermark.PulledUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return pull.HasMore;
    }

    private static bool IsMale(LocalStudent student) =>
        string.Equals(student.Gender, "Male", StringComparison.OrdinalIgnoreCase)
        || string.Equals(student.Gender, "Boy", StringComparison.OrdinalIgnoreCase)
        || student.Gender == "ছেলে";

    private static bool IsFemale(LocalStudent student) =>
        string.Equals(student.Gender, "Female", StringComparison.OrdinalIgnoreCase)
        || string.Equals(student.Gender, "Girl", StringComparison.OrdinalIgnoreCase)
        || student.Gender == "মেয়ে";

    private static bool IsBirthdayOn(DateTime? dob, DateTime day) =>
        dob is DateTime d && d.Month == day.Month && d.Day == day.Day;

    private static bool IsUpcomingBirthday(DateTime? dob, DateTime today, int days)
    {
        if (dob is null) return false;
        for (var i = 1; i <= days; i++)
        {
            if (IsBirthdayOn(dob, today.AddDays(i)))
                return true;
        }
        return false;
    }

    private static DateTime NextBirthday(DateTime? dob, DateTime today)
    {
        if (dob is null) return today.AddYears(10);
        var year = today.Year;
        if (!DateTime.IsLeapYear(year) && dob.Value.Month == 2 && dob.Value.Day == 29)
            return new DateTime(year, 2, 28);
        var next = new DateTime(year, dob.Value.Month, Math.Min(dob.Value.Day, DateTime.DaysInMonth(year, dob.Value.Month)));
        return next >= today ? next : next.AddYears(1);
    }

    private static int AgeYears(DateTime? dob, DateTime today)
    {
        if (dob is null) return 0;
        var age = today.Year - dob.Value.Year;
        if (dob.Value.Date > today.AddYears(-age)) age--;
        return Math.Max(age, 0);
    }

    private async Task RefreshPendingAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        _pendingCount = await db.Outbox.CountAsync(cancellationToken);
    }

    private async Task<string?> ReadOutboxErrorAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Outbox
            .Where(x => x.LastError != null && x.LastError != "")
            .OrderByDescending(x => x.OutboxId)
            .Select(x => x.LastError)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void RegisterFailure()
    {
        lock (_gate)
        {
            _failureCount++;
            var seconds = Math.Min(30 * Math.Pow(2, Math.Min(_failureCount, 4)), 300);
            _nextAttemptUtc = DateTime.UtcNow.AddSeconds(seconds);
        }
    }

    private static LocalStudent Map(StudentDto dto, string deviceId)
    {
        var row = new LocalStudent { OriginDeviceId = deviceId };
        Copy(dto, row, deviceId);
        return row;
    }

    private static void Copy(StudentDto dto, LocalStudent row, string deviceId)
    {
        row.LocalId = dto.LocalId;
        row.ServerId = dto.ServerId;
        row.StudentClassServerId = dto.StudentClassServerId;
        row.SchoolID = dto.SchoolID;
        row.EducationYearID = dto.EducationYearID;
        row.RegistrationID = dto.RegistrationID;
        row.StudentCode = dto.StudentCode.Trim();
        row.StudentsName = dto.StudentsName.Trim();
        row.SMSPhoneNo = dto.SMSPhoneNo.Trim();
        row.Gender = dto.Gender;
        row.DateofBirth = dto.DateofBirth;
        row.FathersName = dto.FathersName;
        row.MothersName = dto.MothersName;
        row.BloodGroup = dto.BloodGroup;
        row.Religion = dto.Religion;
        row.AdmissionDate = dto.AdmissionDate;
        row.IsNew = dto.IsNew;
        row.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status;
        row.ClassID = dto.ClassID;
        row.RollNo = dto.RollNo;
        row.SectionID = dto.SectionID;
        row.ShiftID = dto.ShiftID;
        row.SubjectGroupID = dto.SubjectGroupID;
        row.ClassName = dto.ClassName;
        row.SectionName = dto.SectionName;
        row.ShiftName = dto.ShiftName;
        row.GroupName = dto.GroupName;
        row.StudentEmailAddress = TrimOrNull(dto.StudentEmailAddress);
        row.LegalIdentity = TrimOrNull(dto.LegalIdentity);
        row.StudentsLocalAddress = TrimOrNull(dto.StudentsLocalAddress);
        row.StudentPermanentAddress = TrimOrNull(dto.StudentPermanentAddress);
        row.OtherDetails = TrimOrNull(dto.OtherDetails);
        row.PrevSchoolName = TrimOrNull(dto.PrevSchoolName);
        row.PrevClass = TrimOrNull(dto.PrevClass);
        row.PrevExamYear = TrimOrNull(dto.PrevExamYear);
        row.PrevExamGrade = TrimOrNull(dto.PrevExamGrade);
        row.FatherOccupation = TrimOrNull(dto.FatherOccupation);
        row.FatherPhoneNumber = TrimOrNull(dto.FatherPhoneNumber);
        row.MotherOccupation = TrimOrNull(dto.MotherOccupation);
        row.MotherPhoneNumber = TrimOrNull(dto.MotherPhoneNumber);
        row.GuardianName = TrimOrNull(dto.GuardianName);
        row.GuardianRelationshipwithStudent = TrimOrNull(dto.GuardianRelationshipwithStudent);
        row.GuardianPhoneNumber = TrimOrNull(dto.GuardianPhoneNumber);
        row.UpdatedUtc = dto.UpdatedUtc;
        row.SyncStatus = dto.SyncStatus;
        row.OriginDeviceId = deviceId;
    }

    private static StudentDto Map(LocalStudent row)
    {
        return new StudentDto
        {
            LocalId = row.LocalId,
            ServerId = row.ServerId,
            StudentClassServerId = row.StudentClassServerId,
            SchoolID = row.SchoolID,
            EducationYearID = row.EducationYearID,
            RegistrationID = row.RegistrationID,
            StudentCode = row.StudentCode,
            StudentsName = row.StudentsName,
            SMSPhoneNo = row.SMSPhoneNo,
            Gender = row.Gender,
            DateofBirth = row.DateofBirth,
            FathersName = row.FathersName,
            MothersName = row.MothersName,
            BloodGroup = row.BloodGroup,
            Religion = row.Religion,
            AdmissionDate = row.AdmissionDate,
            IsNew = row.IsNew,
            Status = row.Status,
            ClassID = row.ClassID,
            RollNo = row.RollNo,
            SectionID = row.SectionID,
            ShiftID = row.ShiftID,
            SubjectGroupID = row.SubjectGroupID,
            ClassName = row.ClassName,
            SectionName = row.SectionName,
            ShiftName = row.ShiftName,
            GroupName = row.GroupName,
            StudentEmailAddress = row.StudentEmailAddress,
            LegalIdentity = row.LegalIdentity,
            StudentsLocalAddress = row.StudentsLocalAddress,
            StudentPermanentAddress = row.StudentPermanentAddress,
            OtherDetails = row.OtherDetails,
            PrevSchoolName = row.PrevSchoolName,
            PrevClass = row.PrevClass,
            PrevExamYear = row.PrevExamYear,
            PrevExamGrade = row.PrevExamGrade,
            FatherOccupation = row.FatherOccupation,
            FatherPhoneNumber = row.FatherPhoneNumber,
            MotherOccupation = row.MotherOccupation,
            MotherPhoneNumber = row.MotherPhoneNumber,
            GuardianName = row.GuardianName,
            GuardianRelationshipwithStudent = row.GuardianRelationshipwithStudent,
            GuardianPhoneNumber = row.GuardianPhoneNumber,
            UpdatedUtc = row.UpdatedUtc,
            SyncStatus = row.SyncStatus
        };
    }

    private static string? TrimOrNull(string? value)
    {
        var text = value?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
