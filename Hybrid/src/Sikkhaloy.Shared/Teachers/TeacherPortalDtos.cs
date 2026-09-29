namespace Sikkhaloy.Shared.Teachers;

public sealed class TeacherPortalDashboardDto
{
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
    public string SchoolName { get; set; } = "";
    public string SchoolAddress { get; set; } = "";
    public string YearName { get; set; } = "";
    public string? PhotoDataUrl { get; set; }
    public int SubjectCount { get; set; }
    public int TodayPeriodCount { get; set; }
    public List<TeacherPortalPeriodDto> TodayRoutine { get; set; } = [];
    public string TodayStatus { get; set; } = "";
    public string? TodayEntry { get; set; }
    public string? TodayExit { get; set; }
    public string? TodayHoliday { get; set; }
    public int MonthPresent { get; set; }
    public int MonthAbsent { get; set; }
    public int MonthLate { get; set; }
    public int MonthLeave { get; set; }
    public int MonthTotal { get; set; }
    public decimal MonthPct { get; set; }
}

public sealed class TeacherPortalDetailsDto
{
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Gender { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public string Religion { get; set; } = "";
    public string FatherName { get; set; } = "";
    public string MothersName { get; set; } = "";
    public string Address { get; set; } = "";
    public string PermanentAddress { get; set; } = "";
    public string Nationality { get; set; } = "";
    public string? PhotoDataUrl { get; set; }
}

public sealed class TeacherPortalPeriodDto
{
    public string Day { get; set; } = "";
    public string Period { get; set; } = "";
    public string Subject { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string Section { get; set; } = "";
    public string Shift { get; set; } = "";
    public string Group { get; set; } = "";
    public string Time { get; set; } = "";
}

public sealed class TeacherPortalSubjectDto
{
    public int ClassID { get; set; }
    public string ClassName { get; set; } = "";
    public int SubjectID { get; set; }
    public string SubjectName { get; set; } = "";
}

public sealed class TeacherPortalOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class TeacherPortalStudentHitDto
{
    public int StudentID { get; set; }
    public int StudentClassID { get; set; }
    public string StudentCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string RollNo { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string ShiftName { get; set; } = "";
}

public sealed class TeacherPortalAttendanceMetaDto
{
    public bool AllowBackDate { get; set; }
    public List<TeacherPortalOptionDto> Classes { get; set; } = [];
    public List<TeacherPortalOptionDto> Schedules { get; set; } = [];
}

public sealed class TeacherPortalCvDto
{
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string? PhotoDataUrl { get; set; }
    public List<TeacherPortalCvLineDto> Objectives { get; set; } = [];
    public List<TeacherPortalCvEducationDto> Education { get; set; } = [];
    public List<TeacherPortalCvJobDto> Jobs { get; set; } = [];
    public List<TeacherPortalCvSkillDto> Skills { get; set; } = [];
    public List<TeacherPortalCvLanguageDto> Languages { get; set; } = [];
}

public sealed class TeacherPortalCvLineDto
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
}

public sealed class TeacherPortalCvEducationDto
{
    public int Id { get; set; }
    public string InstitutionName { get; set; } = "";
    public string ExamName { get; set; } = "";
    public string ExamYear { get; set; } = "";
    public string Result { get; set; } = "";
}

public sealed class TeacherPortalCvJobDto
{
    public int Id { get; set; }
    public string InstitutionName { get; set; } = "";
    public string Position { get; set; } = "";
    public string JobType { get; set; } = "";
    public string JobYear { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class TeacherPortalCvSkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class TeacherPortalCvLanguageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Level { get; set; } = "";
}

public sealed class TeacherPortalCvTextRequest
{
    public string Text { get; set; } = "";
}

public sealed class TeacherPortalCvEducationRequest
{
    public string InstitutionName { get; set; } = "";
    public string ExamName { get; set; } = "";
    public string ExamYear { get; set; } = "";
    public string Result { get; set; } = "";
}

public sealed class TeacherPortalCvJobRequest
{
    public string InstitutionName { get; set; } = "";
    public string Position { get; set; } = "";
    public string JobType { get; set; } = "";
    public string JobYear { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}

public sealed class TeacherPortalCvSkillRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class TeacherPortalCvLanguageRequest
{
    public string Name { get; set; } = "";
    public string Level { get; set; } = "";
}
