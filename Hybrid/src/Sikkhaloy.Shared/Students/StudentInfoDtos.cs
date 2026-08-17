namespace Sikkhaloy.Shared.Students;

public sealed class StudentSignupRowDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? FathersName { get; set; }
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool Selected { get; set; }
}

public sealed class StudentCreatedUserDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public DateTime? CreateDate { get; set; }
}

public sealed class StudentSignupListsDto
{
    public List<StudentSignupRowDto> WithoutLogin { get; set; } = [];
    public List<StudentCreatedUserDto> Created { get; set; } = [];
}

public sealed class CreateStudentUsersRequest
{
    public List<int> StudentIDs { get; set; } = [];
}

public sealed class StudentAccountDto
{
    public int StudentID { get; set; }
    public int RegistrationID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string UserName { get; set; } = "";
    public string Validation { get; set; } = "";
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public sealed class SetStudentApprovedRequest
{
    public string UserName { get; set; } = "";
    public bool IsApproved { get; set; }
}

public sealed class UnlockStudentRequest
{
    public string UserName { get; set; } = "";
}

public sealed class DeleteStudentAccountRequest
{
    public string UserName { get; set; } = "";
    public int RegistrationID { get; set; }
}

public sealed class StudentAccountResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
}

public sealed class StudentIdCardDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? FathersName { get; set; }
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string? BloodGroup { get; set; }
    public string? PermanentAddress { get; set; }
    public string? LocalAddress { get; set; }
    public DateTime? DateofBirth { get; set; }
    public string SchoolName { get; set; } = "";
    public string? SchoolAddress { get; set; }
    public string? InstitutionDialog { get; set; }
    public string? LogoDataUrl { get; set; }
    public string? PhotoDataUrl { get; set; }
}

public sealed class StudentPhotoDto
{
    public int StudentID { get; set; }
    public string? PhotoDataUrl { get; set; }
}

public sealed class StudentPlacementDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? FathersName { get; set; }
    public string? MothersName { get; set; }
    public string? ClassName { get; set; }
    public string? GroupName { get; set; }
    public string? SectionName { get; set; }
    public string? ShiftName { get; set; }
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateofBirth { get; set; }
    public string? BloodGroup { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public string? PermanentAddress { get; set; }
    public string SchoolName { get; set; } = "";
    public string? SchoolAddress { get; set; }
    public string? EducationYear { get; set; }
}

public sealed class SaveStudentPlacementRequest
{
    public int StudentClassID { get; set; }
    public string? RollNo { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public bool UpdateGroup { get; set; }
    public bool UpdateSection { get; set; }
    public bool UpdateShift { get; set; }
}

public sealed class StudentSubjectRowDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public bool Selected { get; set; }
    public string SubjectType { get; set; } = "Compulsory";
}

public sealed class StudentSubjectsDto
{
    public StudentPlacementDto? Student { get; set; }
    public List<StudentSubjectRowDto> Subjects { get; set; } = [];
}

public sealed class SaveStudentSubjectItem
{
    public int SubjectID { get; set; }
    public string SubjectType { get; set; } = "Compulsory";
}

public sealed class SaveStudentSubjectsRequest
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public List<SaveStudentSubjectItem> Items { get; set; } = [];
}

public sealed class StudentInfoResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Count { get; set; }
}
