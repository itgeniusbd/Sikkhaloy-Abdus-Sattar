namespace Sikkhaloy.Shared.Authority;

public sealed class AuthorityResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int Id { get; set; }
}

public sealed class AuthorityOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Extra { get; set; } = "";
}

public sealed class SignupUserRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Email { get; set; } = "";
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

public sealed class SignupInstitutionRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string PasswordAnswer { get; set; } = "";
    public string PerStudentRate { get; set; } = "";
    public string SchoolName { get; set; } = "";
    public string Established { get; set; } = "";
    public string Principal { get; set; } = "";
    public string AcadamicStaff { get; set; } = "";
    public string Students { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string LocalArea { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Website { get; set; } = "";
    public string Address { get; set; } = "";
    public string? LogoBase64 { get; set; }
    public int ReferrerId { get; set; }
    public string Commission { get; set; } = "10";
    public int DurationYears { get; set; } = 2;
}

public sealed class SignupLookupsDto
{
    public List<AuthorityOptionDto> Referrers { get; set; } = [];
}

public sealed class UserInfoListDto
{
    public int Total { get; set; }
    public int Valid { get; set; }
    public int Invalid { get; set; }
    public List<UserInfoRowDto> Rows { get; set; } = [];
}

public sealed class UserInfoRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Validation { get; set; } = "";
    public string SessionNames { get; set; } = "";
}

public sealed class SchoolUserDto
{
    public int RegistrationID { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsApproved { get; set; }
    public bool IsLockedOut { get; set; }
    public string Email { get; set; } = "";
    public string Validation { get; set; } = "";
    public DateTime? CreateDate { get; set; }
}

public sealed class SetApprovedRequest
{
    public string UserName { get; set; } = "";
    public bool IsApproved { get; set; }
}

public sealed class UnlockUserRequest
{
    public string UserName { get; set; } = "";
}

public sealed class TestimonialRowDto
{
    public int TestimonialID { get; set; }
    public int ShowSn { get; set; }
    public string SchoolName { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime? InsertDate { get; set; }
    public bool IsShow { get; set; }
}

public sealed class SaveTestimonialRequest
{
    public int TestimonialID { get; set; }
    public string Text { get; set; } = "";
    public int ShowSn { get; set; }
}

public sealed class SetTestimonialShowRequest
{
    public int TestimonialID { get; set; }
    public bool IsShow { get; set; }
}

public sealed class ResetSchoolOptionDto
{
    public int SchoolID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ResetYearOptionDto
{
    public int EducationYearID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class ResetTableCountDto
{
    public string TableName { get; set; } = "";
    public long RowCnt { get; set; }
}

public sealed class ResetPreviewDto
{
    public bool Ok { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "";
    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = "";
    public string Mode { get; set; } = "";
    public int? EducationYearId { get; set; }
    public int ActiveUsers { get; set; }
    public long TotalRows { get; set; }
    public long Bytes { get; set; }
    public int SkippedStudents { get; set; }
    public List<ResetTableCountDto> Tables { get; set; } = [];
}

public sealed class ResetProgressDto
{
    public bool Ok { get; set; } = true;
    public bool HasProgress { get; set; }
    public string? Mode { get; set; }
    public long DeletedRows { get; set; }
    public long TotalRows { get; set; }
    public string Status { get; set; } = "Idle";
    public string? Message { get; set; }
    public int Percent { get; set; }
}

public sealed class ResetExecuteRequest
{
    public int SchoolId { get; set; }
    public int ConfirmSchoolId { get; set; }
    public string Mode { get; set; } = "";
    public int EducationYearId { get; set; }
    public string ConfirmWord { get; set; } = "";
    public long TotalRows { get; set; }
}

public sealed class ResetImageRequest
{
    public int SchoolId { get; set; }
    public int ConfirmSchoolId { get; set; }
    public List<int> EducationYearIds { get; set; } = [];
}

public sealed class AttSignupPageDto
{
    public List<AuthorityOptionDto> Available { get; set; } = [];
    public List<AttDeviceRowDto> Registered { get; set; } = [];
}

public sealed class AttDeviceRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class AttRegisterRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class AttPasswordRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class AttActiveRequest
{
    public int SchoolID { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SmsSettingPageDto
{
    public List<string> Providers { get; set; } = [];
    public string SmsProvider { get; set; } = "";
    public string SmsProviderMultiple { get; set; } = "";
    public int SmsSendInterval { get; set; }
    public int SmsProcessingUnit { get; set; }
    public int PendingSms { get; set; }
}

public sealed class SaveSmsSettingRequest
{
    public string SmsProvider { get; set; } = "";
    public string SmsProviderMultiple { get; set; } = "";
    public int SmsSendInterval { get; set; }
    public int SmsProcessingUnit { get; set; }
}

public sealed class SmsSenderRowDto
{
    public int Id { get; set; }
    public DateTime? AppStartTime { get; set; }
    public DateTime? AppCloseTime { get; set; }
    public int TotalEventCall { get; set; }
    public int TotalSmsSend { get; set; }
    public int TotalSmsFailed { get; set; }
}

public sealed class SmsFailedPageDto
{
    public int TotalFailed { get; set; }
    public int TodayFailed { get; set; }
    public int ThisWeekFailed { get; set; }
    public List<AuthorityOptionDto> Schools { get; set; } = [];
    public List<SmsFailedRowDto> Rows { get; set; } = [];
}

public sealed class SmsFailedRowDto
{
    public int Id { get; set; }
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string SmsText { get; set; } = "";
    public string MobileNo { get; set; } = "";
    public string AttendanceStatus { get; set; } = "";
    public string FailedReason { get; set; } = "";
    public string ScheduleTime { get; set; } = "";
    public string CreateTime { get; set; } = "";
    public string SentTime { get; set; } = "";
    public DateTime? AttendanceDate { get; set; }
    public DateTime? InsertDate { get; set; }
    public string SmsTimeOut { get; set; } = "";
}

public sealed class ClientSmsPageDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Deactive { get; set; }
    public int Balance { get; set; }
    public string Gateway { get; set; } = "";
    public bool LocalMode { get; set; }
    public List<ClientSmsRowDto> Rows { get; set; } = [];
}

public sealed class ClientSmsRowDto
{
    public int SchoolID { get; set; }
    public string SchoolName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Validation { get; set; } = "";
    public string StatusText { get; set; } = "";
    public int PhoneCount { get; set; }
    public DateTime? Date { get; set; }
}

public sealed class SendClientSmsRequest
{
    public string Text { get; set; } = "";
    public List<int> SchoolIds { get; set; } = [];
}

public sealed class SendClientSmsResult
{
    public bool Succeeded { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public int Balance { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public bool LocalMode { get; set; }
    public List<string> Details { get; set; } = [];
}

public sealed class AuthProfileDto
{
    public int AuthorityID { get; set; }
    public string Name { get; set; } = "";
    public string? FatherName { get; set; }
    public string? Gender { get; set; }
    public string? Designation { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? DateofBirth { get; set; }
    public string? PhotoDataUrl { get; set; }
}
