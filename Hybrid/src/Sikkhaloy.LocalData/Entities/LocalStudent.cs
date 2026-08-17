using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Entities;

public sealed class LocalStudent
{
    public Guid LocalId { get; set; }
    public int? ServerId { get; set; }
    public int? StudentClassServerId { get; set; }
    public int SchoolID { get; set; }
    public int EducationYearID { get; set; }
    public int RegistrationID { get; set; }
    public string StudentCode { get; set; } = "";
    public string StudentsName { get; set; } = "";
    public string SMSPhoneNo { get; set; } = "";
    public string? Gender { get; set; }
    public DateTime? DateofBirth { get; set; }
    public string? FathersName { get; set; }
    public string? MothersName { get; set; }
    public string? BloodGroup { get; set; }
    public string? Religion { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public bool? IsNew { get; set; }
    public string Status { get; set; } = "Active";
    public int? ClassID { get; set; }
    public string? RollNo { get; set; }
    public int? SectionID { get; set; }
    public int? ShiftID { get; set; }
    public int? SubjectGroupID { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? ShiftName { get; set; }
    public string? GroupName { get; set; }
    public string? StudentEmailAddress { get; set; }
    public string? LegalIdentity { get; set; }
    public string? StudentsLocalAddress { get; set; }
    public string? StudentPermanentAddress { get; set; }
    public string? OtherDetails { get; set; }
    public string? PrevSchoolName { get; set; }
    public string? PrevClass { get; set; }
    public string? PrevExamYear { get; set; }
    public string? PrevExamGrade { get; set; }
    public string? FatherOccupation { get; set; }
    public string? FatherPhoneNumber { get; set; }
    public string? MotherOccupation { get; set; }
    public string? MotherPhoneNumber { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianRelationshipwithStudent { get; set; }
    public string? GuardianPhoneNumber { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string OriginDeviceId { get; set; } = "";
}
