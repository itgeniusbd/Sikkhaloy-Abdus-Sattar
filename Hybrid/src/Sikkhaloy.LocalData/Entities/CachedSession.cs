namespace Sikkhaloy.LocalData.Entities;

public sealed class CachedSession
{
    public string UserName { get; set; } = "";
    public string Role { get; set; } = "";
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public int RegistrationID { get; set; }
    public int EducationYearID { get; set; }
    public string? DisplayName { get; set; }
    public string DeviceId { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public DateTimeOffset TokenExpiresAt { get; set; }
    public string PasswordVerifier { get; set; } = "";
    public string VerifierSalt { get; set; } = "";
    public DateTime CachedUtc { get; set; }
}
