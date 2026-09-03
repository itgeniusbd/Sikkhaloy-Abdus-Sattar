namespace Sikkhaloy.Shared.Auth;

public sealed class OfficeProfileDto
{
    public int AdminID { get; set; }
    public string DisplayName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? PhotoBase64 { get; set; }
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? LogoBase64 { get; set; }
    public string? NameLogoBase64 { get; set; }
    public bool ClearNameLogo { get; set; }
    public string? HeaderColor { get; set; }
}

public sealed class AdminInfoDto
{
    public int AdminID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? FatherName { get; set; }
    public string? Gender { get; set; }
    public string? Designation { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PhotoDataUrl { get; set; }
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

public sealed class ProfileResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? DisplayName { get; set; }
    public string? PhotoDataUrl { get; set; }
}

public sealed class HeaderColorRequest
{
    public string Color { get; set; } = "";
}

public sealed class SwitchYearRequest
{
    public int EducationYearID { get; set; }
}
