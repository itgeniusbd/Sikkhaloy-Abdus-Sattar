namespace Sikkhaloy.Shared.Students;

public sealed class ReAdmissionCandidateDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int EducationYearID { get; set; }
    public string EducationYear { get; set; } = "";
    public string StudentCode { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? FathersName { get; set; }
    public string? SMSPhoneNo { get; set; }
    public string? FatherPhoneNumber { get; set; }
    public string? MotherPhoneNumber { get; set; }
    public string? GuardianPhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? RollNo { get; set; }
    public int? ClassID { get; set; }
    public string? ClassName { get; set; }
    public int? SectionID { get; set; }
    public string? SectionName { get; set; }
    public int? ShiftID { get; set; }
    public string? ShiftName { get; set; }
    public int? SubjectGroupID { get; set; }
    public string? GroupName { get; set; }
}

public sealed class ReAdmissionAssignDto
{
    public ReAdmissionCandidateDto? Student { get; set; }
    public List<EducationYearDto> TargetYears { get; set; } = [];
    public string? Error { get; set; }
}

public sealed class ReAdmissionSubjectDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public string SubjectType { get; set; } = "Compulsory";
    public bool Selected { get; set; }
}

public sealed class ReAdmissionSubjectChoice
{
    public int SubjectID { get; set; }
    public string SubjectType { get; set; } = "Compulsory";
}

public sealed class ReAdmissionRequest
{
    public int StudentID { get; set; }
    public int FromStudentClassID { get; set; }
    public int ToEducationYearID { get; set; }
    public int ClassID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public int SubjectGroupID { get; set; }
    public string? RollNo { get; set; }
    public List<ReAdmissionSubjectChoice> Subjects { get; set; } = [];
}

public sealed class ReAdmissionResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public StudentDto? Student { get; set; }
}

public sealed class ReAdmissionExamDto
{
    public int ExamID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ReAdmissionPositionDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string? Position { get; set; }
}

public sealed class BulkReAdmissionStudent
{
    public int StudentID { get; set; }
    public int FromStudentClassID { get; set; }
    public string? RollNo { get; set; }
}

public sealed class BulkReAdmissionRequest
{
    public int ToEducationYearID { get; set; }
    public int ClassID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public int SubjectGroupID { get; set; }
    public List<ReAdmissionSubjectChoice> Subjects { get; set; } = [];
    public List<BulkReAdmissionStudent> Students { get; set; } = [];
}

public sealed class BulkReAdmissionResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Admitted { get; set; }
    public int Skipped { get; set; }
    public List<StudentDto> Students { get; set; } = [];
}
