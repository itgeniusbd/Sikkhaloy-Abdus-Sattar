using Sikkhaloy.LocalData.Sync;

namespace Sikkhaloy.App.Services;

public sealed class TeacherFeedbackInbox : IDisposable
{
    private readonly AppSession _session;
    private readonly ISyncApiClient _api;
    private int _seq;

    public TeacherFeedbackInbox(AppSession session, ISyncApiClient api)
    {
        _session = session;
        _api = api;
        _session.Changed += OnSessionChanged;
    }

    public event Action? Changed;
    public int UnreadCount { get; private set; }
    public string BadgeText => UnreadCount > 99 ? "99+" : UnreadCount.ToString();
    public bool Visible =>
        _session.IsAuthenticated
        && _session.IsTeacher
        && !string.IsNullOrWhiteSpace(_session.AccessToken);

    public async Task RefreshAsync()
    {
        var seq = Interlocked.Increment(ref _seq);
        if (!Visible)
        {
            UnreadCount = 0;
            Changed?.Invoke();
            return;
        }

        try
        {
            var dto = await _api.GetTeacherFeedbackUnreadAsync(_session.AccessToken);
            if (seq != _seq)
                return;
            UnreadCount = dto.Count < 0 ? 0 : dto.Count;
        }
        catch
        {
            if (seq != _seq)
                return;
        }

        Changed?.Invoke();
    }

    public void SetUnread(int count)
    {
        UnreadCount = count < 0 ? 0 : count;
        Changed?.Invoke();
    }

    private void OnSessionChanged() => _ = RefreshAsync();

    public void Dispose() => _session.Changed -= OnSessionChanged;
}
