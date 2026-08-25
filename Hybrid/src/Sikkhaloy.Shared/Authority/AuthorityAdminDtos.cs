using Sikkhaloy.Shared.Access;

namespace Sikkhaloy.Shared.Authority;

public sealed class AuthRoleOptionDto
{
    public string RoleId { get; set; } = "";
    public string RoleName { get; set; } = "";
}

public sealed class AuthRoleListDto
{
    public List<string> Roles { get; set; } = [];
}

public sealed class AuthRoleSaveRequest
{
    public string Name { get; set; } = "";
}

public sealed class AuthLinkCategoryRowDto
{
    public int LinkCategoryID { get; set; }
    public int Ascending { get; set; }
    public string Category { get; set; } = "";
}

public sealed class AuthLinkSubRowDto
{
    public int SubCategoryID { get; set; }
    public int LinkCategoryID { get; set; }
    public int Ascending { get; set; }
    public string SubCategory { get; set; } = "";
}

public sealed class AuthLinkPageRowDto
{
    public int LinkID { get; set; }
    public int LinkCategoryID { get; set; }
    public int SubCategoryID { get; set; }
    public int Ascending { get; set; }
    public string PageURL { get; set; } = "";
    public string PageTitle { get; set; } = "";
    public string Category { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string RoleId { get; set; } = "";
    public string RoleName { get; set; } = "";
}

public sealed class AuthLinkTreeDto
{
    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string SubCategoryName { get; set; } = "";
    public List<AuthLinkCategoryRowDto> Categories { get; set; } = [];
    public List<AuthLinkSubRowDto> Subs { get; set; } = [];
    public List<AuthLinkPageRowDto> Pages { get; set; } = [];
    public List<AuthRoleOptionDto> Roles { get; set; } = [];
}

public sealed class AuthLinkNameSaveRequest
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int Ascending { get; set; }
    public string Name { get; set; } = "";
}

public sealed class AuthLinkPageSaveRequest
{
    public int LinkID { get; set; }
    public int LinkCategoryID { get; set; }
    public int SubCategoryID { get; set; }
    public int Ascending { get; set; }
    public string PageURL { get; set; } = "";
    public string PageTitle { get; set; } = "";
    public string RoleId { get; set; } = "";
}

public sealed class AuthReferrerRowDto
{
    public int ReferenceID { get; set; }
    public int ReferenceSN { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public int TotalSchools { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
}

public sealed class AuthReferrerSaveRequest
{
    public int ReferenceID { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime? StartDate { get; set; }
}

public sealed class AuthAssignedSchoolDto
{
    public int ReferenceSchoolID { get; set; }
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string Phone { get; set; } = "";
    public decimal Percentage { get; set; }
    public DateTime? SignupDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal PaidCommission { get; set; }
    public bool Expired { get; set; }
}

public sealed class AuthReferralPageDto
{
    public int ReferenceID { get; set; }
    public string ReferenceName { get; set; } = "";
    public List<AuthReferrerRowDto> Referrers { get; set; } = [];
    public List<AuthAssignedSchoolDto> Assigned { get; set; } = [];
}

public sealed class AuthSchoolSearchDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool HasInvoice { get; set; }
}

public sealed class AuthSchoolSearchPageDto
{
    public List<AuthSchoolSearchDto> Items { get; set; } = [];
}

public sealed class AuthAssignSchoolRequest
{
    public int ReferenceID { get; set; }
    public int SchoolID { get; set; }
    public decimal Percentage { get; set; }
    public DateTime? SignupDate { get; set; }
    public DateTime? ExpireDate { get; set; }
}

public sealed class AuthAssignUpdateRequest
{
    public int ReferenceSchoolID { get; set; }
    public decimal Percentage { get; set; }
    public DateTime? SignupDate { get; set; }
    public DateTime? ExpireDate { get; set; }
}

public sealed class AuthCommissionRowDto
{
    public int ReferenceID { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public int TotalSchools { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
}

public sealed class AuthCommissionSchoolDto
{
    public int ReferenceSchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public decimal Percentage { get; set; }
    public DateTime? SignupDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public decimal TotalServiceCharge { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
}

public sealed class AuthPayHistoryDto
{
    public int Id { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal Amount { get; set; }
    public string PaidBy { get; set; } = "";
    public string Method { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class AuthCommissionPageDto
{
    public decimal TotalCommission { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }
    public int TotalRef { get; set; }
    public int DetailRefId { get; set; }
    public string DetailRefName { get; set; } = "";
    public List<AuthorityOptionDto> Referrers { get; set; } = [];
    public List<AuthCommissionRowDto> Rows { get; set; } = [];
    public List<AuthCommissionSchoolDto> Schools { get; set; } = [];
    public List<AuthPayHistoryDto> History { get; set; } = [];
}

public sealed class AuthCommissionPayRequest
{
    public int ReferenceID { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string PaidBy { get; set; } = "";
    public string Method { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class AuthSubSignupRequest
{
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Email { get; set; } = "";
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

public sealed class AuthAccessPageDto
{
    public string UserName { get; set; } = "";
    public List<AuthAccessUserDto> Users { get; set; } = [];
    public List<PageAccessRowDto> Pages { get; set; } = [];
}

public sealed class AuthAccessUserDto
{
    public int RegistrationID { get; set; }
    public string UserName { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class AuthAccessSaveRequest
{
    public string UserName { get; set; } = "";
    public List<int> LinkIDs { get; set; } = [];
}
