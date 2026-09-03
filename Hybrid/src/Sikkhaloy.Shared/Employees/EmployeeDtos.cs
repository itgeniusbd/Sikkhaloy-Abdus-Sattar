namespace Sikkhaloy.Shared.Employees;

public sealed class EmployeeListDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string? FatherName { get; set; }
    public string? Phone { get; set; }
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string JobStatus { get; set; } = "";
    public string? PermanentTemporary { get; set; }
    public decimal Salary { get; set; }
    public string? BankAccNo { get; set; }
    public string? DeviceID { get; set; }
    public string? SubCategoryName { get; set; }
    public string? PhotoDataUrl { get; set; }
}

public sealed class CreateTeacherRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? FatherName { get; set; }
    public string Designation { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Gender { get; set; } = "";
    public string? Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Religion { get; set; }
    public string? Nationality { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string PermanentTemporary { get; set; } = "Permanent";
    public decimal Salary { get; set; }
    public bool AbsDeducted { get; set; }
    public decimal AbsDeduction { get; set; }
    public bool LateCountAsAbs { get; set; }
    public int? LateDays { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Email { get; set; } = "";
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

public sealed class CreateStaffRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Gender { get; set; } = "";
    public string? FatherName { get; set; }
    public string Designation { get; set; } = "";
    public string? Religion { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string PermanentTemporary { get; set; } = "Permanent";
    public decimal Salary { get; set; }
    public bool AbsDeducted { get; set; }
    public decimal AbsDeduction { get; set; }
    public bool LateCountAsAbs { get; set; }
    public int? LateDays { get; set; }
}

public sealed class EmployeeResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int EmployeeID { get; set; }
    public int PersonID { get; set; }
    public string? UserName { get; set; }
}

public sealed class UpdateEmployeeRequest
{
    public string? ID { get; set; }
    public string? Name { get; set; }
    public string? FatherName { get; set; }
    public string? Phone { get; set; }
    public string? Designation { get; set; }
    public string? EmployeeType { get; set; }
    public decimal? Salary { get; set; }
    public string? BankAccNo { get; set; }
}

public sealed class SetJobStatusRequest
{
    public string JobStatus { get; set; } = "Active";
}

public sealed class EmployeePhotoRequest
{
    public string EmployeeType { get; set; } = "";
    public string Image { get; set; } = "";
}

public sealed class EmployeeEditDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? FatherName { get; set; }
    public string Gender { get; set; } = "Male";
    public string? Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Religion { get; set; }
    public string? Nationality { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Designation { get; set; } = "";
    public string PermanentTemporary { get; set; } = "Permanent";
    public decimal Salary { get; set; }
    public bool AbsDeducted { get; set; }
    public decimal AbsDeduction { get; set; }
    public bool LateCountAsAbs { get; set; }
    public int? LateDays { get; set; }
    public string? BankAccNo { get; set; }
    public string? PhotoDataUrl { get; set; }
    public bool IsTeacher =>
        string.Equals(EmployeeType, "Teacher", StringComparison.OrdinalIgnoreCase);
}

public sealed class TeacherAccountDto
{
    public int TeacherID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public sealed class TeacherPickDto
{
    public int TeacherID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class TeacherSubjectRowDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public bool Assigned { get; set; }
}

public sealed class ToggleTeacherSubjectRequest
{
    public int ClassID { get; set; }
    public int SubjectID { get; set; }
    public bool Assigned { get; set; }
}

public sealed class SetTeacherApprovedRequest
{
    public string UserName { get; set; } = "";
    public bool IsApproved { get; set; }
}

public sealed class UnlockTeacherRequest
{
    public string UserName { get; set; } = "";
}

public sealed class TeacherAccountResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
}

public sealed class EmployeeIdCardDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? DateofBirth { get; set; }
    public string SchoolName { get; set; } = "";
    public string? Address { get; set; }
    public string? InstitutionDialog { get; set; }
    public string? LogoDataUrl { get; set; }
    public string? PhotoDataUrl { get; set; }
}
