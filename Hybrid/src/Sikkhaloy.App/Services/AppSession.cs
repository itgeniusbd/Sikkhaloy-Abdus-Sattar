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
    public bool IsStudent => Current?.IsStudent == true;
    public bool IsImpersonating => AuthorityReturn is not null;
    public LoginResponse? AuthorityReturn { get; private set; }
    public string HomePath => IsAuthority ? "/authority" : IsStudent ? "/student" : "/dashboard";
    public string HomeHref => IsAuthority ? "authority" : IsStudent ? "student" : "dashboard";

    public event Action? Changed;
    public event Action? Mutated;
    public bool AwaitInitialSync { get; private set; }
    private bool _initialSyncFinished;
    private bool _syncOverlaySeen;

    public void Set(LoginResponse response, bool notify = true)
    {
        Current = response.Session;
        AccessToken = response.AccessToken ?? "";
        TokenExpiresAt = response.ExpiresAt;
        AwaitInitialSync = Current is not null && !IsAuthority && !IsStudent;
        _initialSyncFinished = false;
        _syncOverlaySeen = false;
        Mutated?.Invoke();
        if (notify)
            Changed?.Invoke();
    }

    public void ApplyServerLogin(LoginResponse response)
    {
        if (response.Session is null)
            return;
        Current = response.Session;
        if (!string.IsNullOrWhiteSpace(response.AccessToken))
            AccessToken = response.AccessToken;
        TokenExpiresAt = response.ExpiresAt;
        Mutated?.Invoke();
    }

    public void FinishInitialSync()
    {
        _initialSyncFinished = true;
        TryClearInitialSync();
    }

    public void MarkSyncOverlaySeen()
    {
        _syncOverlaySeen = true;
        TryClearInitialSync();
    }

    private void TryClearInitialSync()
    {
        if (!AwaitInitialSync || !_initialSyncFinished || !_syncOverlaySeen)
            return;
        AwaitInitialSync = false;
    }

    public void Restore(LoginResponse current, LoginResponse? authorityReturn)
    {
        AuthorityReturn = authorityReturn;
        Current = current.Session;
        AccessToken = current.AccessToken ?? "";
        TokenExpiresAt = current.ExpiresAt;
        AwaitInitialSync = false;
        _initialSyncFinished = true;
        _syncOverlaySeen = true;
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
        DisplayName = source.DisplayName,
        StudentID = source.StudentID,
        StudentClassID = source.StudentClassID,
        ClassID = source.ClassID,
        StudentCode = source.StudentCode,
        ClassName = source.ClassName,
        SectionName = source.SectionName
    };

    public void UpdateYear(int educationYearId, bool notify = true)
    {
        if (Current is null)
            return;
        Current.EducationYearID = educationYearId;
        Mutated?.Invoke();
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
        AwaitInitialSync = false;
        _initialSyncFinished = true;
        _syncOverlaySeen = true;
        Access = new();
        Mutated?.Invoke();
        Changed?.Invoke();
    }

    public SubscriptionStatusDto Access { get; private set; } = new();

    public void SetAccess(SubscriptionStatusDto status)
    {
        Access = status ?? new();
    }
}
