namespace Sikkhaloy.Shared.Institution;

public sealed class WebsiteHomeDto
{
    public WebsiteSchoolDto School { get; set; } = new();
    public WebsiteStatsDto Stats { get; set; } = new();
    public List<WebsitePersonDto> Teachers { get; set; } = [];
    public List<WebsitePersonDto> Staff { get; set; } = [];
    public List<WebsitePersonDto> Students { get; set; } = [];
    public List<WebsitePersonDto> Birthdays { get; set; } = [];
    public List<WebsiteNoticeDto> Notices { get; set; } = [];
}

public sealed class WebsiteNoticeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Body { get; set; }
    public string? Date { get; set; }
}

public sealed class WebsiteSchoolDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string? Dialog { get; set; }
    public string? Established { get; set; }
    public string? Principal { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
}

public sealed class WebsiteStatsDto
{
    public int Students { get; set; }
    public int Teachers { get; set; }
    public int Staff { get; set; }
    public string? EducationYear { get; set; }
    public List<WebsiteClassCountDto> Classes { get; set; } = [];
}

public sealed class WebsiteClassCountDto
{
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public sealed class WebsitePersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Title { get; set; }
    public string? Meta { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Kind { get; set; }
}

public sealed class WebsiteLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class WebsiteSubjectOptionDto
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
    public string SubjectType { get; set; } = "Compulsory";
    public bool Selected { get; set; }
}

public sealed class WebsiteFeeRoleDto
{
    public int RoleID { get; set; }
    public string Role { get; set; } = "";
    public int NumberOfPay { get; set; }
}

public sealed class WebsiteSuggestedFeeDto
{
    public int AssignRoleID { get; set; }
    public int RoleID { get; set; }
    public string Role { get; set; } = "";
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal LateFee { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class WebsiteAdmissionFormDto
{
    public string? LastStudentId { get; set; }
    public int ActiveEducationYearId { get; set; }
    public List<WebsiteLookupDto> Sessions { get; set; } = [];
    public List<WebsiteLookupDto> Classes { get; set; } = [];
    public List<WebsiteFeeRoleDto> FeeRoles { get; set; } = [];
}

public sealed class WebsiteAdmissionOptionsDto
{
    public string? LastRollNo { get; set; }
    public List<WebsiteLookupDto> Groups { get; set; } = [];
    public List<WebsiteLookupDto> Sections { get; set; } = [];
    public List<WebsiteLookupDto> Shifts { get; set; } = [];
    public List<WebsiteSubjectOptionDto> Subjects { get; set; } = [];
    public List<WebsiteSuggestedFeeDto> Fees { get; set; } = [];
}

public sealed class WebsiteAdmitSubject
{
    public int SubjectID { get; set; }
    public string? SubjectType { get; set; }
}

public sealed class WebsiteAdmitRequest
{
    public string StudentsName { get; set; } = "";
    public string SMSPhoneNo { get; set; } = "";
    public string Gender { get; set; } = "Male";
    public string FathersName { get; set; } = "";
    public int EducationYearID { get; set; }
    public int ClassID { get; set; }
    public int? SectionID { get; set; }
    public int? ShiftID { get; set; }
    public int? SubjectGroupID { get; set; }
    public string? RollNo { get; set; }
    public string? StudentEmailAddress { get; set; }
    public string? DateofBirth { get; set; }
    public string? LegalIdentity { get; set; }
    public string? BloodGroup { get; set; }
    public string? Religion { get; set; }
    public string? StudentsLocalAddress { get; set; }
    public string? StudentPermanentAddress { get; set; }
    public string? MothersName { get; set; }
    public string? MotherOccupation { get; set; }
    public string? MotherPhoneNumber { get; set; }
    public string? FatherOccupation { get; set; }
    public string? FatherPhoneNumber { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianRelationshipwithStudent { get; set; }
    public string? GuardianPhoneNumber { get; set; }
    public string? PrevSchoolName { get; set; }
    public string? PrevClass { get; set; }
    public string? PrevExamYear { get; set; }
    public string? PrevExamGrade { get; set; }
    public string? PhotoDataUrl { get; set; }
    public List<WebsiteAdmitSubject> Subjects { get; set; } = [];
    public bool SendSms { get; set; } = true;
    public List<WebsiteAdmitPayment> Payments { get; set; } = [];
}

public sealed class WebsiteAdmitPaymentsRequest
{
    public int StudentID { get; set; }
    public int EducationYearID { get; set; }
    public int ClassID { get; set; }
    public List<WebsiteAdmitPayment> Payments { get; set; } = [];
}

public sealed class WebsiteAdmitPayment
{
    public int RoleID { get; set; }
    public string? Role { get; set; }
    public string PayFor { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public int AssignRoleID { get; set; }
    public decimal LateFee { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class WebsiteAdmitResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string StudentCode { get; set; } = "";
    public string? LastStudentId { get; set; }
    public string? ClassName { get; set; }
    public string? RollNo { get; set; }
    public string? SchoolName { get; set; }
    public string? SchoolAddress { get; set; }
    public string? SchoolPhone { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public bool SmsSent { get; set; }
    public bool SmsLocal { get; set; }
    public string? SmsError { get; set; }
    public List<WebsiteAdmitPayment> Payments { get; set; } = [];
}
