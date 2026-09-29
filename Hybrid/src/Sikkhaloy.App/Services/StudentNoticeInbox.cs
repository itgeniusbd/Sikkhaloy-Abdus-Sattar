using System.Text.Json;
using Microsoft.JSInterop;
using Sikkhaloy.LocalData.Sync;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.App.Services;

public sealed class StudentNoticeInbox : IDisposable
{
    private readonly AppSession _session;
    private readonly ISyncApiClient _api;
    private readonly IJSRuntime _js;
    private readonly HashSet<string> _noticeReads = new(StringComparer.Ordinal);
    private readonly HashSet<string> _smsReads = new(StringComparer.Ordinal);
    private readonly HashSet<string> _reportReads = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private int _refreshSeq;

    public StudentNoticeInbox(AppSession session, ISyncApiClient api, IJSRuntime js)
    {
        _session = session;
        _api = api;
        _js = js;
        _session.Changed += OnSessionChanged;
    }

    public event Action? Changed;
    public IReadOnlyList<StudentPortalNoticeDto> Notices { get; private set; } = [];
    public IReadOnlyList<StudentPortalSmsDto> SmsItems { get; private set; } = [];
    public IReadOnlyList<StudentPortalFaultReportDto> Reports { get; private set; } = [];
    public int NoticeUnread { get; private set; }
    public int SmsUnread { get; private set; }
    public int ReportUnread { get; private set; }
    public int UnreadCount => NoticeUnread + SmsUnread + ReportUnread;

    public string BadgeText => BadgeOf(UnreadCount);
    public string NoticeBadgeText => BadgeOf(NoticeUnread);
    public string SmsBadgeText => BadgeOf(SmsUnread);
    public string ReportBadgeText => BadgeOf(ReportUnread);

    public bool IsRead(StudentPortalNoticeDto notice) =>
        Contains(_noticeReads, NoticeKey(notice), NoticeLegacyKey(notice));

    public bool IsSmsRead(StudentPortalSmsDto sms) =>
        Contains(_smsReads, SmsKey(sms), SmsLegacyKey(sms));

    public bool IsReportRead(StudentPortalFaultReportDto report) =>
        Contains(_reportReads, ReportKey(report), ReportLegacyKey(report));

    public async Task RefreshAsync()
    {
        var seq = Interlocked.Increment(ref _refreshSeq);
        if (!_session.IsStudent || string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            Notices = [];
            SmsItems = [];
            Reports = [];
            NoticeUnread = SmsUnread = ReportUnread = 0;
            Changed?.Invoke();
            return;
        }

        var token = _session.AccessToken;
        var noticeTask = SafeList(() => _api.GetStudentPortalNoticesAsync(token));
        var smsTask = SafeList(() => _api.GetStudentPortalSmsAsync(token));
        var reportTask = SafeList(() => _api.GetStudentPortalReportAsync(token));
        var serverTask = SafeReads(() => _api.GetStudentPortalInboxReadsAsync(token));
        Notices = await noticeTask;
        SmsItems = await smsTask;
        Reports = await reportTask;
        var server = await serverTask;
        if (seq != _refreshSeq)
            return;

        var before = SnapshotCount();
        MergeKeys(_noticeReads, server.Notices);
        MergeKeys(_smsReads, server.Sms);
        MergeKeys(_reportReads, server.Reports);
        await MergeLocalReadsAsync(_noticeReads, StoreKey("noticeRead"));
        await MergeLocalReadsAsync(_smsReads, StoreKey("smsRead"));
        await MergeLocalReadsAsync(_reportReads, StoreKey("reportRead"));
        UpgradeKeys();
        if (seq != _refreshSeq)
            return;

        Recount();
        if (SnapshotCount() != before)
            await PersistAsync();
    }

    public Task MarkReadAsync(StudentPortalNoticeDto notice) =>
        MarkAsync(_noticeReads, NoticeKey(notice), NoticeLegacyKey(notice));

    public Task MarkSmsReadAsync(StudentPortalSmsDto sms) =>
        MarkAsync(_smsReads, SmsKey(sms), SmsLegacyKey(sms));

    public Task MarkReportReadAsync(StudentPortalFaultReportDto report) =>
        MarkAsync(_reportReads, ReportKey(report), ReportLegacyKey(report));

    public Task MarkAllSmsAsync(IEnumerable<StudentPortalSmsDto>? extra = null) =>
        MarkManyAsync(_smsReads, SmsItems.Concat(extra ?? []).Select(x => (SmsKey(x), SmsLegacyKey(x))));

    public Task MarkAllReportsAsync(IEnumerable<StudentPortalFaultReportDto>? extra = null) =>
        MarkManyAsync(_reportReads, Reports.Concat(extra ?? []).Select(x => (ReportKey(x), ReportLegacyKey(x))));

    public Task MarkAllNoticesAsync(bool? homework = null) =>
        MarkManyAsync(_noticeReads, Notices
            .Where(x => homework is null || x.IsHomeWork == homework)
            .Select(x => (NoticeKey(x), NoticeLegacyKey(x))));

    private async Task MarkManyAsync(HashSet<string> reads, IEnumerable<(string Key, string Legacy)> keys)
    {
        var added = false;
        foreach (var (key, legacy) in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
                added |= reads.Add(key);
            if (!string.IsNullOrWhiteSpace(legacy))
                added |= reads.Add(legacy);
        }
        if (!added)
            return;
        await PersistAsync();
        Recount();
    }

    private async Task MarkAsync(HashSet<string> reads, string key, string legacy)
    {
        var added = false;
        if (!string.IsNullOrWhiteSpace(key))
            added |= reads.Add(key);
        if (!string.IsNullOrWhiteSpace(legacy))
            added |= reads.Add(legacy);
        if (!added)
            return;
        await PersistAsync();
        Recount();
    }

    private async Task PersistAsync()
    {
        await _saveLock.WaitAsync();
        try
        {
            await SaveReadsAsync(_noticeReads, StoreKey("noticeRead"));
            await SaveReadsAsync(_smsReads, StoreKey("smsRead"));
            await SaveReadsAsync(_reportReads, StoreKey("reportRead"));
            await SaveServerReadsAsync();
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private async Task SaveServerReadsAsync()
    {
        if (string.IsNullOrWhiteSpace(_session.AccessToken))
            return;

        try
        {
            var saved = await _api.SaveStudentPortalInboxReadsAsync(_session.AccessToken, CurrentReads());
            MergeKeys(_noticeReads, saved.Notices);
            MergeKeys(_smsReads, saved.Sms);
            MergeKeys(_reportReads, saved.Reports);
        }
        catch
        {
        }
    }

    private StudentPortalInboxReadsDto CurrentReads() => new()
    {
        Notices = _noticeReads.ToList(),
        Sms = _smsReads.ToList(),
        Reports = _reportReads.ToList()
    };

    private void UpgradeKeys()
    {
        foreach (var notice in Notices)
            Promote(_noticeReads, NoticeKey(notice), NoticeLegacyKey(notice));
        foreach (var sms in SmsItems)
            Promote(_smsReads, SmsKey(sms), SmsLegacyKey(sms));
        foreach (var report in Reports)
            Promote(_reportReads, ReportKey(report), ReportLegacyKey(report));
    }

    private async Task MergeLocalReadsAsync(HashSet<string> reads, string storeKey)
    {
        if (string.IsNullOrWhiteSpace(storeKey))
            return;

        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", storeKey);
            if (string.IsNullOrWhiteSpace(json))
                return;
            var items = JsonSerializer.Deserialize<List<string>>(json);
            MergeKeys(reads, items);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
        }
    }

    private async Task SaveReadsAsync(HashSet<string> reads, string storeKey)
    {
        if (string.IsNullOrWhiteSpace(storeKey))
            return;

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", storeKey, JsonSerializer.Serialize(reads.ToList()));
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
        }
    }

    private void Recount()
    {
        NoticeUnread = Notices.Count(x => !IsRead(x));
        SmsUnread = SmsItems.Count(x => !IsSmsRead(x));
        ReportUnread = Reports.Count(x => !IsReportRead(x));
        Changed?.Invoke();
    }

    private string StoreKey(string kind)
    {
        var snap = _session.Current;
        if (snap is null || snap.StudentID <= 0)
            return "";
        return $"sikkhaloy.{kind}.{snap.SchoolID}.{snap.StudentID}.{snap.EducationYearID}";
    }

    private static string NoticeKey(StudentPortalNoticeDto notice) =>
        notice.NoticeId > 0 ? $"n:{notice.NoticeId}" : NoticeLegacyKey(notice);

    private static string NoticeLegacyKey(StudentPortalNoticeDto notice)
    {
        var day = notice.Date?.ToString("yyyy-MM-dd") ?? "";
        return $"{(notice.IsHomeWork ? "1" : "0")}|{day}|{notice.Title.Trim()}|{notice.Body.Trim()}";
    }

    private static string SmsKey(StudentPortalSmsDto sms) =>
        sms.SmsSendId > 0 ? $"s:{sms.SmsSendId}" : SmsLegacyKey(sms);

    private static string SmsLegacyKey(StudentPortalSmsDto sms)
    {
        var stamp = sms.Date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        return $"{stamp}|{sms.Phone.Trim()}|{sms.Purpose.Trim()}|{sms.Text.Trim()}";
    }

    private static string ReportKey(StudentPortalFaultReportDto report) =>
        report.StudentFaultID > 0
            ? report.StudentFaultID.ToString()
            : ReportLegacyKey(report);

    private static string ReportLegacyKey(StudentPortalFaultReportDto report) =>
        $"{report.Date:yyyy-MM-dd}|{report.Title.Trim()}|{report.Body.Trim()}";

    private static bool Contains(HashSet<string> reads, string key, string legacy) =>
        (!string.IsNullOrWhiteSpace(key) && reads.Contains(key))
        || (!string.IsNullOrWhiteSpace(legacy) && reads.Contains(legacy));

    private static void Promote(HashSet<string> reads, string key, string legacy)
    {
        if (string.IsNullOrWhiteSpace(key) || string.Equals(key, legacy, StringComparison.Ordinal))
            return;
        if (reads.Contains(legacy))
            reads.Add(key);
    }

    private static void MergeKeys(HashSet<string> reads, IEnumerable<string>? items)
    {
        if (items is null)
            return;
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item))
                reads.Add(item);
        }
    }

    private int SnapshotCount() => _noticeReads.Count + _smsReads.Count + _reportReads.Count;

    private static string BadgeOf(int count) => count > 99 ? "99+" : count.ToString();

    private static async Task<IReadOnlyList<T>> SafeList<T>(Func<Task<IReadOnlyList<T>>> load)
    {
        try { return await load(); }
        catch { return []; }
    }

    private static async Task<StudentPortalInboxReadsDto> SafeReads(Func<Task<StudentPortalInboxReadsDto>> load)
    {
        try { return await load(); }
        catch { return new StudentPortalInboxReadsDto(); }
    }

    private void OnSessionChanged() => _ = RefreshAsync();

    public void Dispose()
    {
        _session.Changed -= OnSessionChanged;
        _saveLock.Dispose();
    }
}
