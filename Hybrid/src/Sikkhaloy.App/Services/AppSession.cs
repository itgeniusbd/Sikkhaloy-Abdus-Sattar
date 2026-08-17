using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.App.Services;

public sealed class AppSession
{
    public SessionSnapshot? Current { get; private set; }
    public string AccessToken { get; private set; } = "";
    public DateTimeOffset TokenExpiresAt { get; private set; }
    public bool IsAuthenticated => Current is not null;

    public event Action? Changed;

    public void Set(LoginResponse response, bool notify = true)
    {
        Current = response.Session;
        AccessToken = response.AccessToken ?? "";
        TokenExpiresAt = response.ExpiresAt;
        if (notify)
            Changed?.Invoke();
    }

    public void UpdateYear(int educationYearId, bool notify = true)
    {
        if (Current is null)
            return;
        Current.EducationYearID = educationYearId;
        if (notify)
            Changed?.Invoke();
    }

    public void NotifyChanged() => Changed?.Invoke();

    public void Clear()
    {
        Current = null;
        AccessToken = "";
        TokenExpiresAt = default;
        Changed?.Invoke();
    }
}
