namespace Sikkhaloy.Shared.Auth;

public sealed class LoginRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string DeviceId { get; set; } = "";
}

public sealed class LoginResponse
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? AccessToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public SessionSnapshot? Session { get; set; }
}

public sealed class SessionSnapshot
{
    public string UserName { get; set; } = "";
    public string Role { get; set; } = "";
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public int RegistrationID { get; set; }
    public int EducationYearID { get; set; }
    public string DeviceId { get; set; } = "";
    public string DisplayName { get; set; } = "";
}
