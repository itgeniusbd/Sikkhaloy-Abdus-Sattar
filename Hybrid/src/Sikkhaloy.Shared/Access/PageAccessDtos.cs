namespace Sikkhaloy.Shared.Access;

public sealed class SubAdminDto
{
    public int RegistrationID { get; set; }
    public string UserName { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class PageAccessRowDto
{
    public int LinkID { get; set; }
    public string Category { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string PageTitle { get; set; } = "";
    public string? RoleName { get; set; }
    public bool Allowed { get; set; }
}

public sealed class PageAccessDto
{
    public string UserName { get; set; } = "";
    public int RegistrationID { get; set; }
    public List<PageAccessRowDto> Pages { get; set; } = [];
}

public sealed class SavePageAccessRequest
{
    public string UserName { get; set; } = "";
    public List<int> LinkIDs { get; set; } = [];
}

public sealed class SavePageAccessResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
}

public sealed class CreateSubAdminRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Designation { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Email { get; set; } = "";
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

public sealed class CreateSubAdminResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string UserName { get; set; } = "";
    public int RegistrationID { get; set; }
}

public sealed class SubAdminAccountDto
{
    public int RegistrationID { get; set; }
    public string UserName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Validation { get; set; } = "";
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastPasswordChangedDate { get; set; }
}

public sealed class SetSubAdminApprovedRequest
{
    public string UserName { get; set; } = "";
    public bool IsApproved { get; set; }
}

public sealed class UnlockSubAdminRequest
{
    public string UserName { get; set; } = "";
}

public sealed class SubAdminStatusResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
}
