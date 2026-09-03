using System.Text.Json;
using Microsoft.JSInterop;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.App.Services;

public sealed class SessionPersist : IDisposable
{
    private const string JsSave = "sikkhaloySessionSave";
    private const string JsLoad = "sikkhaloySessionLoad";
    private const string JsClear = "sikkhaloySessionClear";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly AppSession _session;
    private readonly IJSRuntime _js;
    private bool _watching;

    public SessionPersist(AppSession session, IJSRuntime js)
    {
        _session = session;
        _js = js;
    }

    public void StartWatching()
    {
        if (_watching)
            return;
        _watching = true;
        _session.Mutated += OnMutated;
    }

    public async Task TryRestoreAsync()
    {
        string json;
        try
        {
            json = await _js.InvokeAsync<string>(JsLoad);
        }
        catch (JSException)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(json))
            return;

        PersistedSession? stored;
        try
        {
            stored = JsonSerializer.Deserialize<PersistedSession>(json, JsonOptions);
        }
        catch (JsonException)
        {
            await ClearAsync();
            return;
        }

        if (stored?.Session is null || string.IsNullOrWhiteSpace(stored.AccessToken))
        {
            await ClearAsync();
            return;
        }

        if (stored.ExpiresAt != default && stored.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            await ClearAsync();
            return;
        }

        _session.Restore(new LoginResponse
        {
            Succeeded = true,
            AccessToken = stored.AccessToken,
            ExpiresAt = stored.ExpiresAt,
            Session = stored.Session
        }, stored.AuthorityReturn);
    }

    public async Task SaveAsync()
    {
        if (!_session.IsAuthenticated || string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            await ClearAsync();
            return;
        }

        if (_session.TokenExpiresAt != default && _session.TokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            await ClearAsync();
            return;
        }

        var payload = JsonSerializer.Serialize(new PersistedSession
        {
            AccessToken = _session.AccessToken,
            ExpiresAt = _session.TokenExpiresAt,
            Session = _session.Current,
            AuthorityReturn = _session.AuthorityReturn
        }, JsonOptions);

        try
        {
            await _js.InvokeVoidAsync(JsSave, payload);
        }
        catch (JSException)
        {
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _js.InvokeVoidAsync(JsClear);
        }
        catch (JSException)
        {
        }
    }

    private void OnMutated() => _ = SaveAsync();

    public void Dispose()
    {
        if (!_watching)
            return;
        _session.Mutated -= OnMutated;
        _watching = false;
    }

    private sealed class PersistedSession
    {
        public string AccessToken { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
        public SessionSnapshot? Session { get; set; }
        public LoginResponse? AuthorityReturn { get; set; }
    }
}
