namespace Sikkhaloy.Shared.Students;

public sealed class SmStudentRowDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? FathersName { get; set; }
    public string? RollNo { get; set; }
    public string? SeatNo { get; set; }
    public string? Phone { get; set; }
    public string? FatherPhone { get; set; }
    public string? MotherPhone { get; set; }
    public string? GuardianPhone { get; set; }
    public string? ClassName { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public string? GroupName { get; set; }
    public string? SectionName { get; set; }
    public string? ShiftName { get; set; }
    public int StudentImageID { get; set; }
    public string? PhotoDataUrl { get; set; }
    public string? GuardianPhotoDataUrl { get; set; }
    public bool Selected { get; set; }
    public bool HasSubject { get; set; }
    public string SubjectType { get; set; } = "Compulsory";
}

public sealed class ChangeClassRequest
{
    public int StudentID { get; set; }
    public int OldStudentClassID { get; set; }
    public int ClassID { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public string? RollNo { get; set; }
    public string ClassStatus { get; set; } = "Promotion";
    public bool KeepPayOrder { get; set; }
    public List<SaveStudentSubjectItem> Subjects { get; set; } = [];
}

public sealed class BulkChangeClassItem
{
    public int StudentID { get; set; }
    public int OldStudentClassID { get; set; }
    public string? RollNo { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
}

public sealed class BulkChangeClassRequest
{
    public int ClassID { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public string ClassStatus { get; set; } = "Promotion";
    public bool KeepPayOrder { get; set; }
    public List<BulkChangeClassItem> Students { get; set; } = [];
    public List<SaveStudentSubjectItem> Subjects { get; set; } = [];
}

public sealed class BulkPlacementRequest
{
    public List<int> StudentClassIDs { get; set; } = [];
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public bool UpdateGroup { get; set; } = true;
    public bool UpdateSection { get; set; } = true;
    public bool UpdateShift { get; set; } = true;
}

public sealed class SaveOneSubjectRequest
{
    public int SubjectID { get; set; }
    public List<SaveOneSubjectItem> Items { get; set; } = [];
}

public sealed class SaveOneSubjectItem
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public bool Selected { get; set; }
    public string SubjectType { get; set; } = "Compulsory";
}

public sealed class ReplaceClassSubjectsRequest
{
    public int ClassID { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public List<SaveStudentSubjectItem> Items { get; set; } = [];
}

public sealed class SaveRollSeatItem
{
    public int StudentClassID { get; set; }
    public string? RollNo { get; set; }
    public string? SeatNo { get; set; }
}

public sealed class SaveRollSeatRequest
{
    public List<SaveRollSeatItem> Items { get; set; } = [];
}

public sealed class SaveStudentPhotoRequest
{
    public int StudentID { get; set; }
    public string ImageBase64 { get; set; } = "";
    public bool IsGuardian { get; set; }
}

public sealed class TcStudentDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public int ClassID { get; set; }
    public string ID { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string? FathersName { get; set; }
    public string? Gender { get; set; }
    public string? ClassName { get; set; }
    public string? GroupName { get; set; }
    public string? SectionName { get; set; }
    public string? ShiftName { get; set; }
    public string? RollNo { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "";
    public string? EducationYear { get; set; }
    public DateTime? DateofBirth { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? Address { get; set; }
    public string SchoolName { get; set; } = "";
}

public sealed class GiveTcRequest
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string ID { get; set; } = "";
    public bool DeleteAllPayorder { get; set; } = true;
}

public sealed class ActivateTcRequest
{
    public int StudentID { get; set; }
    public int ClassID { get; set; }
    public int EducationYearID { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
}

public sealed class NoticeDto
{
    public int StudentNoticeId { get; set; }
    public string NoticeTitle { get; set; } = "";
    public string Notice { get; set; } = "";
    public string? NoticeFile { get; set; }
    public bool IsHomeWork { get; set; }
    public DateTime? InsertDate { get; set; }
    public string? UserName { get; set; }
    public string Classes { get; set; } = "";
    public bool Selected { get; set; }
}

public sealed class SaveNoticeRequest
{
    public string NoticeTitle { get; set; } = "";
    public string Notice { get; set; } = "";
    public bool IsHomeWork { get; set; }
    public List<int> ClassIDs { get; set; } = [];
}

public sealed class DeleteNoticesRequest
{
    public List<int> NoticeIDs { get; set; } = [];
}
