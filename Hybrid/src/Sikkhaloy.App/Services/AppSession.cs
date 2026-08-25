using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Invoice;

namespace Sikkhaloy.App.Services;

public sealed class AppSession
{
    public SessionSnapshot? Current { get; private set; }
    public string AccessToken { get; private set; } = "";
    public DateTimeOffset TokenExpiresAt { get; private set; }
    public bool IsAuthenticated => Current is not null;
    public bool IsAuthority => Current?.IsAuthority == true;
    public bool IsImpersonating => AuthorityReturn is not null;
    public LoginResponse? AuthorityReturn { get; private set; }
    public string HomePath => IsAuthority ? "/authority" : "/dashboard";
    public string HomeHref => IsAuthority ? "authority" : "dashboard";

    public event Action? Changed;

    public void Set(LoginResponse response, bool notify = true)
    {
        Current = response.Session;
        AccessToken = response.AccessToken ?? "";
        TokenExpiresAt = response.ExpiresAt;
        if (notify)
            Changed?.Invoke();
    }

    public void EnterSchool(LoginResponse office)
    {
        if (Current is not null && IsAuthority)
        {
            AuthorityReturn = new LoginResponse
            {
                Succeeded = true,
                AccessToken = AccessToken,
                ExpiresAt = TokenExpiresAt,
                Session = Clone(Current)
            };
        }

        Set(office);
    }

    public bool ReturnToAuthority()
    {
        if (AuthorityReturn?.Session is null)
            return false;

        var back = AuthorityReturn;
        AuthorityReturn = null;
        Set(back);
        return true;
    }

    private static SessionSnapshot Clone(SessionSnapshot source) => new()
    {
        UserName = source.UserName,
        Role = source.Role,
        SchoolID = source.SchoolID,
        SchoolName = source.SchoolName,
        RegistrationID = source.RegistrationID,
        EducationYearID = source.EducationYearID,
        DeviceId = source.DeviceId,
        DisplayName = source.DisplayName
    };

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
        AuthorityReturn = null;
        Access = new();
        Changed?.Invoke();
    }

    public SubscriptionStatusDto Access { get; private set; } = new();

    public void SetAccess(SubscriptionStatusDto status)
    {
        Access = status ?? new();
    }
}
