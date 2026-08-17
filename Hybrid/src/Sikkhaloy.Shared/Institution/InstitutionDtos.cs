namespace Sikkhaloy.Shared.Institution;

public sealed class InstitutionDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string? InstitutionDialog { get; set; }
    public string? Established { get; set; }
    public string? Principal { get; set; }
    public string? AcadamicStaff { get; set; }
    public string? Students { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? LocalArea { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoDataUrl { get; set; }
    public string? NameLogoDataUrl { get; set; }
    public string? LogoBase64 { get; set; }
    public string? NameLogoBase64 { get; set; }
    public bool ClearNameLogo { get; set; }
}

public sealed class PublicInstituteDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public bool HasLogo { get; set; }
    public string? LogoUrl { get; set; }
}

public sealed class PublicStatsDto
{
    public int Institutions { get; set; }
    public int Students { get; set; }
    public int Teachers { get; set; }
}

public sealed class PublicContactRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string MobileNo { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
}

public sealed class PublicContactResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
}

public sealed class InstitutionResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public InstitutionDto? Data { get; set; }
}
