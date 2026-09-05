using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Access;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Calendar;
using Sikkhaloy.Shared.Classes;
using Sikkhaloy.Shared.Employees;
using Sikkhaloy.Shared.Exam;
using Sikkhaloy.Shared.Institution;
using Sikkhaloy.Shared.Menu;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Routine;
using Sikkhaloy.Shared.Committee;
using Sikkhaloy.Shared.Inventory;
using Sikkhaloy.Shared.Invoice;
using Sikkhaloy.Shared.Support;
using Sikkhaloy.Shared.Sms;
using Sikkhaloy.Shared.Authority;
using Sikkhaloy.Shared.Subjects;
using Sikkhaloy.Shared.Sync;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Authorize]
[Route("api/sync")]
public sealed class SyncController : ControllerBase
{
    private readonly StudentSyncService _students;
    private readonly MasterDataService _masters;
    private readonly ClassStructureService _classStructure;
    private readonly PageAccessService _pageAccess;
    private readonly SubAdminService _subAdmins;
    private readonly SubjectService _subjectsCatalog;
    private readonly SubjectAssignService _subjectAssign;
    private readonly InstitutionService _institution;
    private readonly HolidayService _holidays;
    private readonly EducationYearService _years;
    private readonly EmployeeService _employees;
    private readonly SalaryService _salary;
    private readonly StudentInfoService _studentInfo;
    private readonly StudentManagementService _studentMgmt;
    private readonly AttendanceService _attendance;
    private readonly AccountsService _accounts;
    private readonly ReportsService _reports;
    private readonly BalanceSubmissionService _balanceSubmit;
    private readonly PaymentSmsService _sms;
    private readonly ExamService _exams;
    private readonly DashboardService _dashboard;
    private readonly SmsOfficeService _officeSms;
    private readonly SmsTemplateService _smsTemplates;
    private readonly RoutineService _routines;
    private readonly CommitteeService _committee;
    private readonly InventoryService _inventory;
    private readonly PlatformInvoiceService _invoice;
    private readonly SupportService _support;
    private readonly AuthorityService _authority;
    private readonly AuthorityBasicService _authorityBasic;
    private readonly AuthorityInvoiceService _authorityInvoice;
    private readonly AuthorityAdminService _authorityAdmin;

    public SyncController(
        StudentSyncService students,
        MasterDataService masters,
        ClassStructureService classStructure,
        PageAccessService pageAccess,
        SubAdminService subAdmins,
        SubjectService subjectsCatalog,
        SubjectAssignService subjectAssign,
        InstitutionService institution,
        HolidayService holidays,
        EducationYearService years,
        EmployeeService employees,
        SalaryService salary,
        StudentInfoService studentInfo,
        StudentManagementService studentMgmt,
        AttendanceService attendance,
        AccountsService accounts,
        ReportsService reports,
        BalanceSubmissionService balanceSubmit,
        PaymentSmsService sms,
        ExamService exams,
        DashboardService dashboard,
        SmsOfficeService officeSms,
        SmsTemplateService smsTemplates,
        RoutineService routines,
        CommitteeService committee,
        InventoryService inventory,
        PlatformInvoiceService invoice,
        SupportService support,
        AuthorityService authority,
        AuthorityBasicService authorityBasic,
        AuthorityInvoiceService authorityInvoice,
        AuthorityAdminService authorityAdmin)
    {
        _students = students;
        _masters = masters;
        _classStructure = classStructure;
        _pageAccess = pageAccess;
        _subAdmins = subAdmins;
        _subjectsCatalog = subjectsCatalog;
        _subjectAssign = subjectAssign;
        _institution = institution;
        _holidays = holidays;
        _years = years;
        _employees = employees;
        _salary = salary;
        _studentInfo = studentInfo;
        _studentMgmt = studentMgmt;
        _attendance = attendance;
        _accounts = accounts;
        _reports = reports;
        _balanceSubmit = balanceSubmit;
        _sms = sms;
        _exams = exams;
        _dashboard = dashboard;
        _officeSms = officeSms;
        _smsTemplates = smsTemplates;
        _routines = routines;
        _committee = committee;
        _inventory = inventory;
        _invoice = invoice;
        _support = support;
        _authority = authority;
        _authorityBasic = authorityBasic;
        _authorityInvoice = authorityInvoice;
        _authorityAdmin = authorityAdmin;
    }

    [HttpPost("push")]
    public async Task<ActionResult<PushResponse>> Push([FromBody] PushRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var session = JwtTokenService.FromPrincipal(User);
            return Ok(await _students.PushAsync(session, request, cancellationToken));
        }
        catch (Exception ex)
        {
            return Ok(new PushResponse
            {
                Results =
                [
                    new PushItemResult
                    {
                        LocalId = request?.Changes?.FirstOrDefault()?.LocalId ?? Guid.Empty,
                        Succeeded = false,
                        Error = ex.Message
                    }
                ]
            });
        }
    }

    [HttpGet("pull")]
    public async Task<ActionResult<PullResponse>> Pull([FromQuery] long since = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.PullAsync(session, since, cancellationToken));
    }

    [HttpGet("student-id")]
    public async Task<ActionResult<StudentIdCheckResult>> StudentId(
        [FromQuery] string? code,
        [FromQuery] int? exceptServerId,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.CheckStudentIdAsync(session, code, exceptServerId, cancellationToken));
    }

    [HttpGet("classes")]
    public async Task<ActionResult<IReadOnlyList<SchoolClassDto>>> Classes(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.GetClassesAsync(session, cancellationToken));
    }

    [HttpGet("class-structure")]
    public async Task<ActionResult<ClassStructureDto>> ClassStructure(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _classStructure.GetAsync(session, cancellationToken));
    }

    [HttpGet("sub-admins")]
    public async Task<ActionResult<IReadOnlyList<SubAdminDto>>> SubAdmins(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _pageAccess.ListSubAdminsAsync(session, cancellationToken));
    }

    [HttpGet("page-access")]
    public async Task<ActionResult<PageAccessDto>> PageAccess([FromQuery] string userName, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _pageAccess.GetAsync(session, userName ?? "", cancellationToken));
    }

    [HttpPost("page-access")]
    public async Task<ActionResult<SavePageAccessResult>> SavePageAccess([FromBody] SavePageAccessRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _pageAccess.SaveAsync(session, request, cancellationToken));
    }

    [HttpPost("sub-admins")]
    public async Task<ActionResult<CreateSubAdminResult>> CreateSubAdmin([FromBody] CreateSubAdminRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _subAdmins.CreateAsync(session, request, cancellationToken));
    }

    [HttpGet("sub-admin-accounts")]
    public async Task<ActionResult<IReadOnlyList<SubAdminAccountDto>>> SubAdminAccounts(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _subAdmins.ListAccountsAsync(session, cancellationToken));
    }

    [HttpPost("sub-admins/approved")]
    public async Task<ActionResult<SubAdminStatusResult>> SetSubAdminApproved(
        [FromBody] SetSubAdminApprovedRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _subAdmins.SetApprovedAsync(session, request, cancellationToken));
    }

    [HttpPost("sub-admins/unlock")]
    public async Task<ActionResult<SubAdminStatusResult>> UnlockSubAdmin(
        [FromBody] UnlockSubAdminRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!IsAdmin(session))
            return Forbid();
        return Ok(await _subAdmins.UnlockAsync(session, request, cancellationToken));
    }

    private static bool IsAdmin(SessionSnapshot session) =>
        string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase);

    [HttpGet("subjects")]
    public async Task<ActionResult<IReadOnlyList<SubjectDto>>> Subjects(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectsCatalog.ListAsync(session, cancellationToken));
    }

    [HttpPost("subjects")]
    public async Task<ActionResult<SubjectResult>> CreateSubject(
        [FromBody] SaveSubjectRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectsCatalog.CreateAsync(session, request, cancellationToken));
    }

    [HttpPut("subjects/{subjectId:int}")]
    public async Task<ActionResult<SubjectResult>> UpdateSubject(
        int subjectId, [FromBody] SaveSubjectRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectsCatalog.UpdateAsync(session, subjectId, request, cancellationToken));
    }

    [HttpDelete("subjects/{subjectId:int}")]
    public async Task<ActionResult<SubjectResult>> DeleteSubject(int subjectId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectsCatalog.DeleteAsync(session, subjectId, cancellationToken));
    }

    [HttpPost("subjects/serials")]
    public async Task<ActionResult<SubjectResult>> SaveSubjectSerials(
        [FromBody] SaveSubjectSerialsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectsCatalog.SaveSerialsAsync(session, request, cancellationToken));
    }

    [HttpGet("class-subjects")]
    public async Task<ActionResult<IReadOnlyList<ClassSubjectRowDto>>> ClassSubjects(
        [FromQuery] int classId, [FromQuery] int groupId = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectAssign.ListAsync(session, classId, groupId, cancellationToken));
    }

    [HttpPost("class-subjects")]
    public async Task<ActionResult<SubjectAssignResult>> SaveClassSubjects(
        [FromBody] SaveClassSubjectsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectAssign.SaveAsync(session, request, cancellationToken));
    }

    [HttpDelete("class-subjects")]
    public async Task<ActionResult<SubjectAssignResult>> ClearClassSubjects(
        [FromQuery] int classId, [FromQuery] int groupId = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _subjectAssign.ClearAsync(session, classId, groupId, cancellationToken));
    }

    [HttpGet("institution")]
    public async Task<ActionResult<InstitutionDto>> Institution(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _institution.GetAsync(session, cancellationToken));
    }

    [HttpPut("institution")]
    public async Task<ActionResult<InstitutionResult>> SaveInstitution(
        [FromBody] InstitutionDto request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _institution.SaveAsync(session, request, cancellationToken));
    }

    [HttpGet("holidays")]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> Holidays(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _holidays.ListAsync(session, cancellationToken));
    }

    [HttpPost("holidays/weekly")]
    public async Task<ActionResult<HolidayResult>> AddWeeklyHolidays(
        [FromBody] WeeklyHolidayRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _holidays.AddWeeklyAsync(session, request, cancellationToken));
    }

    [HttpPost("holidays/range")]
    public async Task<ActionResult<HolidayResult>> AddRangeHolidays(
        [FromBody] RangeHolidayRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _holidays.AddRangeAsync(session, request, cancellationToken));
    }

    [HttpPost("holidays")]
    public async Task<ActionResult<HolidayResult>> AddHoliday(
        [FromBody] SaveHolidayRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _holidays.AddOneAsync(session, request, cancellationToken));
    }

    [HttpPut("holidays/{holidayId:int}")]
    public async Task<ActionResult<HolidayResult>> UpdateHoliday(
        int holidayId, [FromBody] SaveHolidayRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _holidays.UpdateAsync(session, holidayId, request, cancellationToken));
    }

    [HttpDelete("holidays/{holidayId:int}")]
    public async Task<ActionResult<HolidayResult>> DeleteHoliday(int holidayId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _holidays.DeleteAsync(session, holidayId, cancellationToken));
    }

    [HttpGet("years")]
    public async Task<ActionResult<IReadOnlyList<EducationYearDto>>> Years(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.GetYearsAsync(session, cancellationToken));
    }

    [HttpPost("years")]
    public async Task<ActionResult<EducationYearResult>> CreateYear(
        [FromBody] SaveEducationYearRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _years.CreateAsync(session, request, cancellationToken));
    }

    [HttpPut("years/{yearId:int}")]
    public async Task<ActionResult<EducationYearResult>> UpdateYear(
        int yearId, [FromBody] SaveEducationYearRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _years.UpdateAsync(session, yearId, request, cancellationToken));
    }

    [HttpDelete("years/{yearId:int}")]
    public async Task<ActionResult<EducationYearResult>> DeleteYear(int yearId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _years.DeleteAsync(session, yearId, cancellationToken));
    }

    [HttpGet("profile")]
    public async Task<ActionResult<OfficeProfileDto>> Profile(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.GetProfileAsync(session, cancellationToken));
    }

    [HttpPost("profile/header-color")]
    public async Task<ActionResult<ProfileResult>> SaveHeaderColor(
        [FromBody] HeaderColorRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.SaveHeaderColorAsync(session, request, cancellationToken));
    }

    [HttpGet("profile/admin")]
    public async Task<ActionResult<AdminInfoDto?>> AdminInfo(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.GetAdminAsync(session, cancellationToken));
    }

    [HttpPost("profile/admin")]
    public async Task<ActionResult<ProfileResult>> SaveAdminInfo(
        [FromBody] AdminInfoDto request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.SaveAdminAsync(session, request, cancellationToken));
    }

    [HttpPost("profile/password")]
    public async Task<ActionResult<ProfileResult>> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.ChangePasswordAsync(session, request, cancellationToken));
    }

    [HttpGet("menu")]
    public async Task<ActionResult<MenuTreeDto>> Menu(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _masters.GetMenuAsync(session, cancellationToken));
    }

    [HttpGet("re-admission/candidates")]
    public async Task<ActionResult<IReadOnlyList<ReAdmissionCandidateDto>>> ReAdmissionCandidates(
        [FromQuery] int yearId,
        [FromQuery] int classId,
        [FromQuery] int sectionId = 0,
        [FromQuery] int groupId = 0,
        [FromQuery] int shiftId = 0,
        CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.ListReAdmissionCandidatesAsync(
            session, yearId, classId, sectionId, groupId, shiftId, cancellationToken));
    }

    [HttpGet("re-admission/assign")]
    public async Task<ActionResult<ReAdmissionAssignDto>> ReAdmissionAssign(
        [FromQuery] int studentId,
        [FromQuery] int fromYearId,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.GetReAdmissionAssignAsync(session, studentId, fromYearId, cancellationToken));
    }

    [HttpGet("re-admission/subjects")]
    public async Task<ActionResult<IReadOnlyList<ReAdmissionSubjectDto>>> ReAdmissionSubjects(
        [FromQuery] int classId,
        [FromQuery] int groupId = 0,
        CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.ListReAdmissionSubjectsAsync(session, classId, groupId, cancellationToken));
    }

    [HttpPost("re-admission")]
    public async Task<ActionResult<ReAdmissionResult>> FinishReAdmission(
        [FromBody] ReAdmissionRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.FinishReAdmissionAsync(session, request, cancellationToken));
    }

    [HttpGet("re-admission/exams")]
    public async Task<ActionResult<IReadOnlyList<ReAdmissionExamDto>>> ReAdmissionExams(
        [FromQuery] int yearId,
        [FromQuery] int classId,
        [FromQuery] bool cumulative = true,
        CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.ListReAdmissionExamsAsync(session, yearId, classId, cumulative, cancellationToken));
    }

    [HttpGet("re-admission/positions")]
    public async Task<ActionResult<IReadOnlyList<ReAdmissionPositionDto>>> ReAdmissionPositions(
        [FromQuery] int yearId,
        [FromQuery] int classId,
        [FromQuery] int examId,
        [FromQuery] bool cumulative = true,
        [FromQuery] bool sectionWise = false,
        CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.ListReAdmissionPositionsAsync(
            session, yearId, classId, examId, cumulative, sectionWise, cancellationToken));
    }

    [HttpPost("re-admission/bulk")]
    public async Task<ActionResult<BulkReAdmissionResult>> FinishBulkReAdmission(
        [FromBody] BulkReAdmissionRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _students.FinishBulkReAdmissionAsync(session, request, cancellationToken));
    }

    [HttpGet("employees")]
    public async Task<ActionResult<IReadOnlyList<EmployeeListDto>>> Employees(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.ListAsync(session, type, status, q, cancellationToken));
    }

    [HttpPost("employees/teachers")]
    public async Task<ActionResult<EmployeeResult>> CreateTeacher(
        [FromBody] CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.CreateTeacherAsync(session, request, cancellationToken));
    }

    [HttpPost("employees/staff")]
    public async Task<ActionResult<EmployeeResult>> CreateStaff(
        [FromBody] CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.CreateStaffAsync(session, request, cancellationToken));
    }

    [HttpPut("employees/{employeeId:int}")]
    public async Task<ActionResult<EmployeeResult>> UpdateEmployee(
        int employeeId, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.UpdateAsync(session, employeeId, request, cancellationToken));
    }

    [HttpPost("employees/{employeeId:int}/status")]
    public async Task<ActionResult<EmployeeResult>> SetEmployeeStatus(
        int employeeId, [FromBody] SetJobStatusRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.SetJobStatusAsync(session, employeeId, request, cancellationToken));
    }

    [HttpGet("employees/{employeeId:int}")]
    public async Task<ActionResult<EmployeeEditDto>> EmployeeDetail(
        int employeeId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var data = await _employees.GetAsync(session, employeeId, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPut("employees/{employeeId:int}/detail")]
    public async Task<ActionResult<EmployeeResult>> SaveEmployeeDetail(
        int employeeId, [FromBody] EmployeeEditDto request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.SaveDetailAsync(session, employeeId, request, cancellationToken));
    }

    [HttpPost("employees/{employeeId:int}/photo")]
    public async Task<ActionResult<EmployeeResult>> SaveEmployeePhoto(
        int employeeId, [FromBody] EmployeePhotoRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.SavePhotoAsync(session, employeeId, request, cancellationToken));
    }

    [HttpGet("employees/id-cards")]
    public async Task<ActionResult<IReadOnlyList<EmployeeIdCardDto>>> EmployeeIdCards(
        [FromQuery] string? type,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.ListIdCardsAsync(session, type, q, cancellationToken));
    }

    [HttpGet("teachers/accounts")]
    public async Task<ActionResult<IReadOnlyList<TeacherAccountDto>>> TeacherAccounts(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.ListTeacherAccountsAsync(session, q, cancellationToken));
    }

    [HttpPost("teachers/approved")]
    public async Task<ActionResult<TeacherAccountResult>> SetTeacherApproved(
        [FromBody] SetTeacherApprovedRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.SetApprovedAsync(session, request, cancellationToken));
    }

    [HttpPost("teachers/unlock")]
    public async Task<ActionResult<TeacherAccountResult>> UnlockTeacher(
        [FromBody] UnlockTeacherRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.UnlockAsync(session, request, cancellationToken));
    }

    [HttpGet("teachers")]
    public async Task<ActionResult<IReadOnlyList<TeacherPickDto>>> ActiveTeachers(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.ListActiveTeachersAsync(session, cancellationToken));
    }

    [HttpGet("teachers/{teacherId:int}/subjects")]
    public async Task<ActionResult<IReadOnlyList<TeacherSubjectRowDto>>> TeacherSubjects(
        int teacherId, [FromQuery] int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.ListTeacherSubjectsAsync(session, teacherId, classId, cancellationToken));
    }

    [HttpPost("teachers/{teacherId:int}/subjects")]
    public async Task<ActionResult<EmployeeResult>> ToggleTeacherSubject(
        int teacherId, [FromBody] ToggleTeacherSubjectRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _employees.ToggleTeacherSubjectAsync(session, teacherId, request, cancellationToken));
    }

    [HttpGet("salary/names")]
    public async Task<ActionResult<IReadOnlyList<SalaryNameDto>>> SalaryNames(
        [FromQuery] string kind, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListNamesAsync(session, kind, cancellationToken));
    }

    [HttpPost("salary/names")]
    public async Task<ActionResult<SalaryResult>> CreateSalaryName(
        [FromQuery] string kind, [FromBody] SaveSalaryNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.CreateNameAsync(session, kind, request, cancellationToken));
    }

    [HttpPut("salary/names/{id:int}")]
    public async Task<ActionResult<SalaryResult>> UpdateSalaryName(
        int id, [FromQuery] string kind, [FromBody] SaveSalaryNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.UpdateNameAsync(session, kind, id, request, cancellationToken));
    }

    [HttpDelete("salary/names/{id:int}")]
    public async Task<ActionResult<SalaryResult>> DeleteSalaryName(
        int id, [FromQuery] string kind, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.DeleteNameAsync(session, kind, id, cancellationToken));
    }

    [HttpGet("salary/assign")]
    public async Task<ActionResult<IReadOnlyList<SalaryAssignRowDto>>> SalaryAssign(
        [FromQuery] string kind, [FromQuery] int nameId, [FromQuery] string? type, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListAssignAsync(session, kind, nameId, type, cancellationToken));
    }

    [HttpPost("salary/assign")]
    public async Task<ActionResult<SalaryResult>> SaveSalaryAssign(
        [FromQuery] string kind, [FromBody] SaveSalaryAssignRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.SaveAssignAsync(session, kind, request, cancellationToken));
    }

    [HttpGet("salary/payorder-employees")]
    public async Task<ActionResult<IReadOnlyList<PayorderEmployeeDto>>> PayorderEmployees(
        [FromQuery] string? type, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListPayorderEmployeesAsync(session, type, cancellationToken));
    }

    [HttpPost("salary/payorder-employees")]
    public async Task<ActionResult<SalaryResult>> AssignPayorder(
        [FromBody] AssignPayorderRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.AssignPayorderAsync(session, request, cancellationToken));
    }

    [HttpGet("salary/months")]
    public async Task<ActionResult<IReadOnlyList<SalaryMonthDto>>> SalaryMonths(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListMonthsAsync(session, cancellationToken));
    }

    [HttpPost("salary/generate")]
    public async Task<ActionResult<SalaryResult>> GenerateSalary(
        [FromBody] GenerateSalaryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.GenerateAsync(session, request, cancellationToken));
    }

    [HttpGet("salary/monthly")]
    public async Task<ActionResult<IReadOnlyList<MonthlyPayorderDto>>> MonthlyPayorders(
        [FromQuery] int payorderNameId, [FromQuery] string monthName, [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListMonthlyAsync(session, payorderNameId, monthName ?? "", type, cancellationToken));
    }

    [HttpPost("salary/bonus-fine")]
    public async Task<ActionResult<SalaryResult>> UpdateBonusFine(
        [FromBody] UpdateBonusFineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.UpdateBonusFineAsync(session, request, cancellationToken));
    }

    [HttpDelete("salary/monthly/{employeePayorderId:int}")]
    public async Task<ActionResult<SalaryResult>> DeleteMonthlyPayorder(
        int employeePayorderId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.DeletePayorderAsync(session, employeePayorderId, cancellationToken));
    }

    [HttpPost("salary/monthly/delete")]
    public async Task<ActionResult<SalaryResult>> DeleteMonthlyPayorders(
        [FromBody] DeleteMonthlyPayordersRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.DeletePayordersAsync(session, request, cancellationToken));
    }

    [HttpGet("salary/accounts")]
    public async Task<ActionResult<IReadOnlyList<AccountOptionDto>>> SalaryAccounts(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListAccountsAsync(session, cancellationToken));
    }

    [HttpPost("salary/pay")]
    public async Task<ActionResult<SalaryResult>> PaySalary(
        [FromBody] PaySalaryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.PayAsync(session, request, cancellationToken));
    }

    [HttpGet("salary/paid-records")]
    public async Task<ActionResult<IReadOnlyList<PaidRecordDto>>> PaidRecords(
        [FromQuery] int employeeId, [FromQuery] int employeePayorderId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.ListPaidRecordsAsync(session, employeeId, employeePayorderId, cancellationToken));
    }

    [HttpDelete("salary/paid-records/{recordId:int}")]
    public async Task<ActionResult<SalaryResult>> DeletePaidRecord(int recordId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _salary.DeletePaidRecordAsync(session, recordId, cancellationToken));
    }

    [HttpGet("salary/paid-due")]
    public async Task<ActionResult<IReadOnlyList<PaidDueRowDto>>> PaidDue(
        [FromQuery] string? ids, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var list = string.IsNullOrWhiteSpace(ids)
            ? []
            : ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var n) ? n : 0)
                .Where(x => x > 0)
                .ToList();
        return Ok(await _salary.ListPaidDueAsync(session, list, cancellationToken));
    }

    [HttpGet("student-info/signup")]
    public async Task<ActionResult<StudentSignupListsDto>> StudentSignup(
        [FromQuery] int classId, [FromQuery] int groupId, [FromQuery] int sectionId, [FromQuery] int shiftId,
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.ListSignupAsync(session, classId, groupId, sectionId, shiftId, id, cancellationToken));
    }

    [HttpPost("student-info/signup")]
    public async Task<ActionResult<StudentInfoResult>> CreateStudentUsers(
        [FromBody] CreateStudentUsersRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.CreateUsersAsync(session, request, cancellationToken));
    }

    [HttpPost("student-info/signup/sms")]
    public async Task<ActionResult<SmsResult>> StudentLoginSms(
        [FromBody] StudentLoginSmsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.SendStudentLoginSmsAsync(session, request, cancellationToken));
    }

    [HttpGet("student-info/accounts")]
    public async Task<ActionResult<IReadOnlyList<StudentAccountDto>>> StudentAccounts(
        [FromQuery] int classId, [FromQuery] int groupId, [FromQuery] int sectionId, [FromQuery] int shiftId,
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.ListAccountsAsync(session, classId, groupId, sectionId, shiftId, id, cancellationToken));
    }

    [HttpPost("student-info/accounts/approved")]
    public async Task<ActionResult<StudentAccountResult>> SetStudentApproved(
        [FromBody] SetStudentApprovedRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.SetApprovedAsync(session, request, cancellationToken));
    }

    [HttpPost("student-info/accounts/unlock")]
    public async Task<ActionResult<StudentAccountResult>> UnlockStudent(
        [FromBody] UnlockStudentRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.UnlockAsync(session, request, cancellationToken));
    }

    [HttpPost("student-info/accounts/delete")]
    public async Task<ActionResult<StudentInfoResult>> DeleteStudentAccount(
        [FromBody] DeleteStudentAccountRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.DeleteAccountAsync(session, request, cancellationToken));
    }

    [HttpGet("student-info/id-cards")]
    public async Task<ActionResult<IReadOnlyList<StudentIdCardDto>>> StudentIdCards(
        [FromQuery] int classId, [FromQuery] int groupId, [FromQuery] int sectionId, [FromQuery] int shiftId,
        [FromQuery] string? ids, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.ListIdCardsAsync(session, classId, groupId, sectionId, shiftId, ids, cancellationToken));
    }

    [HttpGet("student-info/photos")]
    public async Task<ActionResult<IReadOnlyList<StudentPhotoDto>>> StudentPhotos(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.ListPhotosAsync(session, cancellationToken));
    }

    [HttpGet("student-info/report")]
    public async Task<ActionResult<StudentReportDto>> StudentReport(
        [FromQuery] string? id, [FromQuery] string? part, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.GetReportAsync(session, id, part, cancellationToken));
    }

    [HttpGet("student-info/fault-reports")]
    public async Task<ActionResult<List<StudentPortalFaultReportDto>>> StudentFaultReports(
        [FromQuery] string? id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.ListFaultReportsAsync(session, id, from, to, cancellationToken));
    }

    [HttpPost("student-info/fault-reports")]
    public async Task<ActionResult<StudentInfoResult>> SaveStudentFaultReport(
        [FromBody] SaveStudentFaultReportRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.SaveFaultReportAsync(session, request, cancellationToken));
    }

    [HttpPost("student-info/fault-reports/bulk")]
    public async Task<ActionResult<StudentInfoResult>> SaveStudentFaultReportsBulk(
        [FromBody] SaveStudentFaultReportsBulkRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.SaveFaultReportsBulkAsync(session, request, cancellationToken));
    }

    [HttpPut("student-info/fault-reports")]
    public async Task<ActionResult<StudentInfoResult>> UpdateStudentFaultReport(
        [FromBody] UpdateStudentFaultReportRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.UpdateFaultReportAsync(session, request, cancellationToken));
    }

    [HttpDelete("student-info/fault-reports/{id:int}")]
    public async Task<ActionResult<StudentInfoResult>> DeleteStudentFaultReport(
        int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.DeleteFaultReportAsync(session, id, cancellationToken));
    }

    [HttpGet("student-info/placement")]
    public async Task<ActionResult<StudentPlacementDto?>> StudentPlacement(
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.FindPlacementAsync(session, id, cancellationToken));
    }

    [HttpPost("student-info/placement")]
    public async Task<ActionResult<StudentInfoResult>> SaveStudentPlacement(
        [FromBody] SaveStudentPlacementRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.SavePlacementAsync(session, request, cancellationToken));
    }

    [HttpGet("student-info/subjects")]
    public async Task<ActionResult<StudentSubjectsDto>> StudentSubjects(
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.GetSubjectsAsync(session, id, cancellationToken));
    }

    [HttpPost("student-info/subjects")]
    public async Task<ActionResult<StudentInfoResult>> SaveStudentSubjects(
        [FromBody] SaveStudentSubjectsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.SaveSubjectsAsync(session, request, cancellationToken));
    }

    [HttpGet("student-info/certificate")]
    public async Task<ActionResult<StudentPlacementDto?>> StudentCertificate(
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.FindCertificateAsync(session, id, cancellationToken));
    }

    [HttpGet("student-mgmt/students")]
    public async Task<ActionResult<IReadOnlyList<SmStudentRowDto>>> SmStudents(
        [FromQuery] int classId, [FromQuery] int groupId, [FromQuery] int sectionId, [FromQuery] int shiftId,
        [FromQuery] string? id, [FromQuery] int? subjectId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.ListClassStudentsAsync(
            session, classId, groupId, sectionId, shiftId, id, subjectId, cancellationToken));
    }

    [HttpGet("student-mgmt/class-change")]
    public async Task<ActionResult<StudentPlacementDto?>> SmClassChangeStudent(
        [FromQuery] int studentId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.FindStudentAsync(session, studentId, cancellationToken));
    }

    [HttpPost("student-mgmt/class-change")]
    public async Task<ActionResult<StudentInfoResult>> SmChangeClass(
        [FromBody] ChangeClassRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.ChangeClassAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/class-change/bulk")]
    public async Task<ActionResult<StudentInfoResult>> SmBulkChangeClass(
        [FromBody] BulkChangeClassRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.BulkChangeClassAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/group-section-shift")]
    public async Task<ActionResult<StudentInfoResult>> SmBulkPlacement(
        [FromBody] BulkPlacementRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.BulkPlacementAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/subject-students")]
    public async Task<ActionResult<StudentInfoResult>> SmSaveOneSubject(
        [FromBody] SaveOneSubjectRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.SaveOneSubjectAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/class-subjects")]
    public async Task<ActionResult<StudentInfoResult>> SmReplaceClassSubjects(
        [FromBody] ReplaceClassSubjectsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.ReplaceClassSubjectsAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/rolls")]
    public async Task<ActionResult<StudentInfoResult>> SmSaveRolls(
        [FromBody] SaveRollSeatRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.SaveRollsAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/photo")]
    public async Task<ActionResult<StudentInfoResult>> SmSavePhoto(
        [FromBody] SaveStudentPhotoRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.SaveStudentPhotoAsync(session, request, cancellationToken));
    }

    [HttpGet("student-mgmt/tc")]
    public async Task<ActionResult<TcStudentDto?>> SmFindTc(
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.FindTcAsync(session, id, cancellationToken));
    }

    [HttpGet("student-mgmt/tc/list")]
    public async Task<ActionResult<IReadOnlyList<TcStudentDto>>> SmListTc(
        [FromQuery] int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.ListTcAsync(session, classId, cancellationToken));
    }

    [HttpPost("student-mgmt/tc/give")]
    public async Task<ActionResult<StudentInfoResult>> SmGiveTc(
        [FromBody] GiveTcRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.GiveTcAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/tc/activate")]
    public async Task<ActionResult<StudentInfoResult>> SmActivateTc(
        [FromBody] ActivateTcRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.ActivateTcAsync(session, request, cancellationToken));
    }

    [HttpGet("student-mgmt/notices")]
    public async Task<ActionResult<IReadOnlyList<NoticeDto>>> SmNotices(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.ListNoticesAsync(session, cancellationToken));
    }

    [HttpPost("student-mgmt/notices")]
    public async Task<ActionResult<StudentInfoResult>> SmSaveNotice(
        [FromBody] SaveNoticeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.SaveNoticeAsync(session, request, cancellationToken));
    }

    [HttpPost("student-mgmt/notices/delete")]
    public async Task<ActionResult<StudentInfoResult>> SmDeleteNotices(
        [FromBody] DeleteNoticesRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentMgmt.DeleteNoticesAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/schedules")]
    public async Task<ActionResult<IReadOnlyList<AttendanceScheduleDto>>> AttSchedules(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListSchedulesAsync(session, cancellationToken));
    }

    [HttpPost("attendance/schedules")]
    public async Task<ActionResult<AttendanceResult>> AttCreateSchedule(
        [FromBody] SaveScheduleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.CreateScheduleAsync(session, request, cancellationToken));
    }

    [HttpPost("attendance/schedules/{id:int}")]
    public async Task<ActionResult<AttendanceResult>> AttRenameSchedule(
        int id, [FromBody] SaveScheduleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.RenameScheduleAsync(session, id, request, cancellationToken));
    }

    [HttpPost("attendance/schedules/{id:int}/delete")]
    public async Task<ActionResult<AttendanceResult>> AttDeleteSchedule(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.DeleteScheduleAsync(session, id, cancellationToken));
    }

    [HttpPost("attendance/schedules/days")]
    public async Task<ActionResult<AttendanceResult>> AttSaveDays(
        [FromBody] SaveScheduleDaysRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveScheduleDaysAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/settings")]
    public async Task<ActionResult<AttendanceSettingsDto>> AttSettings(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.GetSettingsAsync(session, cancellationToken));
    }

    [HttpPost("attendance/settings")]
    public async Task<ActionResult<AttendanceResult>> AttSaveSettings(
        [FromBody] AttendanceSettingsDto request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveSettingsAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/settings/app")]
    public IActionResult AttDownloadApp()
    {
        var file = _attendance.FindLatestInstaller();
        if (file is null)
            return NotFound();
        return PhysicalFile(file.FullName, "application/octet-stream", file.Name);
    }

    [HttpGet("attendance/settings/users.csv")]
    public async Task<IActionResult> AttDownloadUsers(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var csv = await _attendance.ExportUsersCsvAsync(session, cancellationToken);
        return File(csv, "text/csv; charset=utf-8", "AttendanceUsers.csv");
    }

    [HttpGet("attendance/settings/photos.zip")]
    public async Task<IActionResult> AttDownloadPhotos(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var zip = await _attendance.ExportPhotosZipAsync(session, cancellationToken);
        if (zip is null || zip.Length == 0)
            return NotFound();
        return File(zip, "application/zip", "Attendance_Photo.zip");
    }

    [HttpGet("attendance/student/rfid")]
    public async Task<ActionResult<IReadOnlyList<StudentRfidRowDto>>> AttStudentRfid(
        int scheduleId, int classId, int groupId, int sectionId, int shiftId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListStudentRfidAsync(session, scheduleId, classId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpPost("attendance/student/rfid")]
    public async Task<ActionResult<AttendanceResult>> AttSaveStudentRfid(
        [FromBody] SaveStudentRfidRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveStudentRfidAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/employee/rfid")]
    public async Task<ActionResult<IReadOnlyList<EmployeeRfidRowDto>>> AttEmployeeRfid(
        int scheduleId, string? type, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListEmployeeRfidAsync(session, scheduleId, type, cancellationToken));
    }

    [HttpPost("attendance/employee/rfid")]
    public async Task<ActionResult<AttendanceResult>> AttSaveEmployeeRfid(
        [FromBody] SaveEmployeeRfidRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveEmployeeRfidAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/student/manual")]
    public async Task<ActionResult<IReadOnlyList<StudentManualRowDto>>> AttStudentManual(
        int scheduleId, int classId, int groupId, int sectionId, int shiftId, DateTime date,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListStudentManualAsync(
            session, scheduleId, classId, groupId, sectionId, shiftId, date, cancellationToken));
    }

    [HttpPost("attendance/student/manual")]
    public async Task<ActionResult<AttendanceResult>> AttSaveStudentManual(
        [FromBody] SaveStudentManualRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveStudentManualAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/employee/manual")]
    public async Task<ActionResult<IReadOnlyList<EmployeeManualRowDto>>> AttEmployeeManual(
        int scheduleId, string? type, DateTime date, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListEmployeeManualAsync(session, scheduleId, type, date, cancellationToken));
    }

    [HttpPost("attendance/employee/manual")]
    public async Task<ActionResult<AttendanceResult>> AttSaveEmployeeManual(
        [FromBody] SaveEmployeeManualRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveEmployeeManualAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/student/records")]
    public async Task<ActionResult<IReadOnlyList<StudentAttendanceRecordDto>>> AttStudentRecords(
        string? status, int classId, int groupId, int sectionId, int shiftId, int scheduleId,
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListStudentRecordsAsync(
            session, status, classId, groupId, sectionId, shiftId, scheduleId, from, to, cancellationToken));
    }

    [HttpGet("attendance/student/summary")]
    public async Task<ActionResult<IReadOnlyList<StudentAttendanceSummaryDto>>> AttStudentSummary(
        int classId, int groupId, int sectionId, int shiftId, int scheduleId,
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListStudentSummaryAsync(
            session, classId, groupId, sectionId, shiftId, scheduleId, from, to, cancellationToken));
    }

    [HttpGet("attendance/employee/records")]
    public async Task<ActionResult<IReadOnlyList<EmployeeAttendanceRecordDto>>> AttEmployeeRecords(
        string? type, string? status, int scheduleId, int employeeId, DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListEmployeeRecordsAsync(
            session, type, status, scheduleId, employeeId, from, to, cancellationToken));
    }

    [HttpGet("attendance/employee/summary")]
    public async Task<ActionResult<IReadOnlyList<EmployeeAttendanceSummaryDto>>> AttEmployeeSummary(
        string? type, int scheduleId, int employeeId, DateTime from, DateTime to,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListEmployeeSummaryAsync(
            session, type, scheduleId, employeeId, from, to, cancellationToken));
    }

    [HttpGet("attendance/leave-types")]
    public async Task<ActionResult<IReadOnlyList<string>>> AttLeaveTypes(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListLeaveTypesAsync(session, cancellationToken));
    }

    [HttpGet("attendance/leave-types/rows")]
    public async Task<ActionResult<IReadOnlyList<AttendanceLeaveTypeDto>>> AttLeaveTypeRows(
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListLeaveTypeRowsAsync(session, cancellationToken));
    }

    [HttpPost("attendance/leave-types")]
    public async Task<ActionResult<AttendanceResult>> AttAddLeaveType(
        [FromBody] SaveLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.AddLeaveTypeAsync(session, request, cancellationToken));
    }

    [HttpPost("attendance/leave-types/{id:int}/delete")]
    public async Task<ActionResult<AttendanceResult>> AttDeleteLeaveType(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.DeleteLeaveTypeAsync(session, id, cancellationToken));
    }

    [HttpGet("attendance/student/leave/suggest")]
    public async Task<ActionResult<IReadOnlyList<StudentLeaveSuggestDto>>> AttSuggestStudentLeave(
        string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SuggestStudentLeaveAsync(session, q, cancellationToken));
    }

    [HttpGet("attendance/student/leave/find")]
    public async Task<ActionResult<StudentLeavePersonDto?>> AttFindStudentLeave(
        string id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.FindStudentLeaveAsync(session, id, cancellationToken));
    }

    [HttpGet("attendance/student/leave/print/{id:int}")]
    public async Task<ActionResult<StudentLeavePrintDto?>> AttStudentLeavePrint(
        int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.GetStudentLeavePrintAsync(session, id, cancellationToken));
    }

    [HttpGet("attendance/student/leave")]
    public async Task<ActionResult<IReadOnlyList<StudentLeaveRowDto>>> AttStudentLeaves(
        int studentId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListStudentLeavesAsync(session, studentId, cancellationToken));
    }

    [HttpPost("attendance/student/leave")]
    public async Task<ActionResult<AttendanceResult>> AttSaveStudentLeave(
        [FromBody] SaveStudentLeaveRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveStudentLeaveAsync(session, request, cancellationToken));
    }

    [HttpPost("attendance/student/leave/{id:int}/delete")]
    public async Task<ActionResult<AttendanceResult>> AttDeleteStudentLeave(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.DeleteStudentLeaveAsync(session, id, cancellationToken));
    }

    [HttpGet("attendance/employee/leave/picks")]
    public async Task<ActionResult<IReadOnlyList<EmployeeLeavePickDto>>> AttEmployeeLeavePicks(
        string? type, string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListEmployeeLeavePicksAsync(session, type, q, cancellationToken));
    }

    [HttpPost("attendance/employee/leave")]
    public async Task<ActionResult<AttendanceResult>> AttSaveEmployeeLeave(
        [FromBody] SaveEmployeeLeaveRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.SaveEmployeeLeaveAsync(session, request, cancellationToken));
    }

    [HttpGet("attendance/leave-report")]
    public async Task<ActionResult<IReadOnlyList<LeaveReportRowDto>>> AttLeaveReport(
        string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListLeaveReportAsync(session, type, from, to, cancellationToken));
    }

    [HttpGet("attendance/fine/months")]
    public async Task<ActionResult<IReadOnlyList<AttendanceMonthDto>>> AttFineMonths(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.ListFineMonthsAsync(session, cancellationToken));
    }

    [HttpPost("attendance/fine")]
    public async Task<ActionResult<IReadOnlyList<AttendanceFineRowDto>>> AttGenerateFine(
        [FromBody] GenerateFineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _attendance.GenerateFineAsync(session, request, cancellationToken));
    }

    [HttpGet("accounts/roles")]
    public async Task<ActionResult<IReadOnlyList<PaymentRoleDto>>> AccRoles(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListRolesAsync(session, cancellationToken));
    }

    [HttpPost("accounts/roles")]
    public async Task<ActionResult<AccountsResult>> AccCreateRole([FromBody] SavePaymentRoleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateRoleAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/roles/{id:int}")]
    public async Task<ActionResult<AccountsResult>> AccUpdateRole(int id, [FromBody] SavePaymentRoleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateRoleAsync(session, id, request, cancellationToken));
    }

    [HttpPost("accounts/roles/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteRoleById(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteRoleAsync(session, id, cancellationToken));
    }

    [HttpPost("accounts/roles/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteRole([FromBody] AccountsIdRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteRoleAsync(session, request.Id, cancellationToken));
    }

    [HttpGet("accounts/assigned")]
    public async Task<ActionResult<IReadOnlyList<AssignedRoleDto>>> AccAssigned(int classId, int roleId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListAssignedAsync(session, classId, roleId, cancellationToken));
    }

    [HttpGet("accounts/assigned/available")]
    public async Task<ActionResult<AssignableRolesDto>> AccAssignable(string? classIds, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var ids = (classIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var n) ? n : 0)
            .Where(x => x > 0)
            .ToList();
        return Ok(await _accounts.ListAssignableAsync(session, ids, cancellationToken));
    }

    [HttpPost("accounts/assigned")]
    public async Task<ActionResult<AccountsResult>> AccAssign([FromBody] SaveAssignedRoleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.AssignRoleAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/assigned/bulk")]
    public async Task<ActionResult<AccountsResult>> AccBulkAssign([FromBody] BulkAssignRoleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.BulkAssignAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/assigned/update")]
    public async Task<ActionResult<AccountsResult>> AccUpdateAssigned([FromBody] UpdateAssignedRoleRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateAssignedAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/assigned/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteAssigned(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteAssignedAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/payorder/students")]
    public async Task<ActionResult<IReadOnlyList<PayOrderStudentDto>>> AccPayOrderStudents(int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListPayOrderStudentsAsync(session, classId, cancellationToken));
    }

    [HttpPost("accounts/payorder")]
    public async Task<ActionResult<AccountsResult>> AccCreatePayOrders([FromBody] CreatePayOrdersRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreatePayOrdersAsync(session, request, cancellationToken));
    }

    [HttpGet("accounts/payorder/unpaid")]
    public async Task<ActionResult<IReadOnlyList<UnpaidPayOrderDto>>> AccUnpaid(int classId, int roleId, DateTime? endDate, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListUnpaidAsync(session, classId, roleId, endDate, cancellationToken));
    }

    [HttpPost("accounts/payorder/remove")]
    public async Task<ActionResult<AccountsResult>> AccRemovePayOrders([FromBody] RemovePayOrderRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.RemovePayOrdersAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/payorder/dates")]
    public async Task<ActionResult<AccountsResult>> AccChangeDates([FromBody] ChangePayOrderDateRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ChangePayOrderDateAsync(session, request, cancellationToken));
    }

    [HttpGet("accounts/cash")]
    public async Task<ActionResult<IReadOnlyList<CashAccountDto>>> AccCash(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListCashAccountsAsync(session, cancellationToken));
    }

    [HttpPost("accounts/cash")]
    public async Task<ActionResult<AccountsResult>> AccCreateCash([FromBody] SaveCashAccountRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateCashAccountAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/cash/{id:int}/default")]
    public async Task<ActionResult<AccountsResult>> AccDefaultCash(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.SetDefaultAccountAsync(session, id, cancellationToken));
    }

    [HttpPost("accounts/cash/deposit")]
    public async Task<ActionResult<AccountsResult>> AccDeposit([FromBody] AccountMoveRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DepositAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/cash/withdraw")]
    public async Task<ActionResult<AccountsResult>> AccWithdraw([FromBody] AccountMoveRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.WithdrawAsync(session, request, cancellationToken));
    }

    [HttpGet("accounts/cash/{id:int}/deposits")]
    public async Task<ActionResult<IReadOnlyList<AccountMoveDto>>> AccDeposits(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListDepositsAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/cash/{id:int}/withdraws")]
    public async Task<ActionResult<IReadOnlyList<AccountMoveDto>>> AccWithdraws(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListWithdrawsAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/students/suggest")]
    public async Task<ActionResult<IReadOnlyList<FeeSuggestDto>>> AccSuggest(string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.SuggestStudentsAsync(session, q, cancellationToken));
    }

    [HttpGet("accounts/students/bundle")]
    public async Task<ActionResult<FeeStudentBundleDto>> AccBundle(string id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.GetStudentBundleAsync(session, id, cancellationToken));
    }

    [HttpPost("accounts/collect")]
    public async Task<ActionResult<AccountsResult>> AccCollect([FromBody] CollectPaymentRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CollectAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/add-more")]
    public async Task<ActionResult<AccountsResult>> AccAddMore([FromBody] AddMorePayOrderRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.AddMoreAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/concession")]
    public async Task<ActionResult<AccountsResult>> AccConcession([FromBody] SaveConcessionRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.SaveConcessionAsync(session, request, cancellationToken));
    }

    [HttpGet("accounts/receipt")]
    public async Task<ActionResult<ReceiptDetailDto?>> AccReceipt(string no, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var dto = await _accounts.GetReceiptAsync(session, no, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("accounts/sms")]
    public async Task<ActionResult<PaymentSmsSettingDto>> AccSms(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _sms.GetSettingAsync(session, cancellationToken));
    }

    [HttpPost("accounts/sms")]
    public async Task<ActionResult<AccountsResult>> AccSmsSave([FromBody] PaymentSmsSettingDto request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        await _sms.SaveSettingAsync(session, request.Active, cancellationToken);
        return Ok(new AccountsResult { Succeeded = true });
    }

    [HttpPost("accounts/receipt/{id:int}/sms")]
    public async Task<ActionResult<AccountsResult>> AccReceiptSms(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _sms.SendReceiptAsync(session, id, cancellationToken));
    }

    [HttpPost("accounts/receipt/printed")]
    public async Task<ActionResult<AccountsResult>> AccPrinted([FromBody] PrintedReceiptRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdatePrintedReceiptAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/receipt/{id:int}/unpaid")]
    public async Task<ActionResult<AccountsResult>> AccUnpaid(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UnpaidReceiptAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/extra/categories")]
    public async Task<ActionResult<IReadOnlyList<ExtraIncomeCategoryDto>>> AccExtraCats(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListExtraCategoriesAsync(session, cancellationToken));
    }

    [HttpPost("accounts/extra/categories")]
    public async Task<ActionResult<AccountsResult>> AccCreateExtraCat([FromBody] SaveExtraCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateExtraCategoryAsync(session, request.Name, cancellationToken));
    }

    [HttpGet("accounts/extra")]
    public async Task<ActionResult<ExtraIncomeListDto>> AccExtra(int categoryId, DateTime? from, DateTime? to, string? receiptNo, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var (items, total) = await _accounts.ListExtraIncomeAsync(session, categoryId, from, to, receiptNo, cancellationToken);
        return Ok(new ExtraIncomeListDto { Items = items.ToList(), Total = total });
    }

    [HttpPost("accounts/extra")]
    public async Task<ActionResult<AccountsResult>> AccCreateExtra([FromBody] SaveExtraIncomeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateExtraIncomeAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/extra/update")]
    public async Task<ActionResult<AccountsResult>> AccUpdateExtra([FromBody] SaveExtraIncomeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateExtraIncomeAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/extra/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteExtra(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteExtraIncomeAsync(session, id, cancellationToken));
    }

    [HttpPost("accounts/extra/categories/update")]
    public async Task<ActionResult<AccountsResult>> AccUpdateExtraCat([FromBody] SaveExtraCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateExtraCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/extra/categories/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteExtraCat(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteExtraCategoryAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/extra/{id:int}")]
    public async Task<ActionResult<ExtraIncomeDto?>> AccExtraOne(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.GetExtraIncomeAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/expense/categories")]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryDto>>> AccExpenseCats(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListExpenseCategoriesAsync(session, cancellationToken));
    }

    [HttpPost("accounts/expense/categories")]
    public async Task<ActionResult<AccountsResult>> AccCreateExpenseCat([FromBody] SaveExpenseCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateExpenseCategoryAsync(session, request.Name, cancellationToken));
    }

    [HttpPost("accounts/expense/categories/update")]
    public async Task<ActionResult<AccountsResult>> AccUpdateExpenseCat([FromBody] SaveExpenseCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateExpenseCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/expense/categories/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteExpenseCat(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteExpenseCategoryAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/expense/subcategories")]
    public async Task<ActionResult<IReadOnlyList<ExpenseSubCategoryDto>>> AccExpenseSubs(int categoryId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.ListExpenseSubCategoriesAsync(session, categoryId, cancellationToken));
    }

    [HttpPost("accounts/expense/subcategories")]
    public async Task<ActionResult<AccountsResult>> AccCreateExpenseSub([FromBody] SaveExpenseSubCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateExpenseSubCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/expense/subcategories/update")]
    public async Task<ActionResult<AccountsResult>> AccUpdateExpenseSub([FromBody] SaveExpenseSubCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateExpenseSubCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/expense/subcategories/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteExpenseSub(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteExpenseSubCategoryAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/expense")]
    public async Task<ActionResult<ExpenseListDto>> AccExpense(int categoryId, int subCategoryId, DateTime? from, DateTime? to, string? receiptNo, int page = 1, int pageSize = 80, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var (items, total, totalCount) = await _accounts.ListExpenseAsync(session, categoryId, subCategoryId, from, to, receiptNo, page, pageSize, cancellationToken);
        return Ok(new ExpenseListDto { Items = items.ToList(), Total = total, TotalCount = totalCount });
    }

    [HttpPost("accounts/expense")]
    public async Task<ActionResult<AccountsResult>> AccCreateExpense([FromBody] SaveExpenseRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.CreateExpenseAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/expense/update")]
    public async Task<ActionResult<AccountsResult>> AccUpdateExpense([FromBody] SaveExpenseRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.UpdateExpenseAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/expense/{id:int}/delete")]
    public async Task<ActionResult<AccountsResult>> AccDeleteExpense(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.DeleteExpenseAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/expense/{id:int}")]
    public async Task<ActionResult<ExpenseDto?>> AccExpenseOne(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _accounts.GetExpenseAsync(session, id, cancellationToken));
    }

    [HttpGet("accounts/reports/summary")]
    public async Task<ActionResult<AccountsSummaryDto>> AccReportSummary(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSummaryAsync(session, cancellationToken));
    }

    [HttpGet("accounts/reports/month")]
    public async Task<ActionResult<MonthBasedDto>> AccReportMonth(
        DateTime? from, DateTime? to, int classId, string? sectionId, string? roleIds,
        bool students = false, bool money = true, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetMonthBasedAsync(session, from, to, classId, sectionId, roleIds, students, money, cancellationToken));
    }

    [HttpGet("accounts/reports/month-roles")]
    public async Task<ActionResult<IReadOnlyList<NameAmountDto>>> AccReportMonthRoles(int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.ListMonthRolesAsync(session, classId, cancellationToken));
    }

    [HttpGet("accounts/reports/income")]
    public async Task<ActionResult<IncomeExpenseReportDto>> AccReportIncome(DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetIncomeReportAsync(session, from, to, category, cancellationToken));
    }

    [HttpGet("accounts/reports/expense")]
    public async Task<ActionResult<IncomeExpenseReportDto>> AccReportExpense(DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetExpenseReportAsync(session, from, to, category, cancellationToken));
    }

    [HttpGet("accounts/reports/net")]
    public async Task<ActionResult<NetReportDto>> AccReportNet(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetNetAsync(session, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/due")]
    public async Task<ActionResult<CurrentDueDto>> AccReportDue(int classId, string? sectionId, string? roleId, string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetCurrentDueAsync(session, classId, sectionId, roleId, id, cancellationToken));
    }

    [HttpGet("accounts/reports/due-roles")]
    public async Task<ActionResult<IReadOnlyList<NameAmountDto>>> AccReportDueRoles(int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.ListDueRolesAsync(session, classId, cancellationToken));
    }

    [HttpGet("accounts/reports/due-details")]
    public async Task<ActionResult<CurrentDueStudentDetailDto>> AccReportDueDetails(string id, string? roleId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetDueDetailsAsync(session, id ?? "", roleId, cancellationToken));
    }

    [HttpPost("accounts/reports/due-sms")]
    public async Task<ActionResult<AccountsResult>> AccReportDueSms([FromBody] DueSmsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _sms.SendDueSmsAsync(session, request ?? new DueSmsRequest(), cancellationToken));
    }

    [HttpGet("accounts/reports/payorder")]
    public async Task<ActionResult<PayorderReportDto>> AccReportPayorder(DateTime? from, DateTime? to, int roleId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetPayorderAsync(session, from, to, roleId, cancellationToken));
    }

    [HttpGet("accounts/reports/paid")]
    public async Task<ActionResult<PaidDetailsDto>> AccReportPaid(string? yearId, int classId, string? groupId, string? sectionId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetPaidAsync(session, yearId, classId, groupId, sectionId, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/my")]
    public async Task<ActionResult<MyAccountsDto>> AccReportMy(int regId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetMyAccountsAsync(session, regId, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/my/remaining")]
    public async Task<ActionResult<BalanceRemainingDto>> AccReportMyRemaining(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _balanceSubmit.GetRemainingAsync(session, from, to, cancellationToken));
    }

    [HttpPost("accounts/reports/my/submit-otp")]
    public async Task<ActionResult<AccountsResult>> AccReportMySubmitOtp([FromBody] BalanceSubmitOtpRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _balanceSubmit.SendOtpAsync(session, request, cancellationToken));
    }

    [HttpPost("accounts/reports/my/submit")]
    public async Task<ActionResult<AccountsResult>> AccReportMySubmit([FromBody] BalanceSubmitRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _balanceSubmit.SubmitAsync(session, request, cancellationToken));
    }

    [HttpGet("accounts/reports/account")]
    public async Task<ActionResult<List<AccountDetailDto>>> AccReportAccount(string? accountId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetAccountDetailsAsync(session, accountId, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/log")]
    public async Task<ActionResult<AccountsLogDto>> AccReportLog(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetLogAsync(session, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/income-categories")]
    public async Task<ActionResult<IReadOnlyList<NameAmountDto>>> AccReportIncomeCats(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.ListIncomeCategoriesAsync(session, cancellationToken));
    }

    [HttpGet("accounts/reports/expense-categories")]
    public async Task<ActionResult<IReadOnlyList<NameAmountDto>>> AccReportExpenseCats(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.ListExpenseCategoriesAsync(session, cancellationToken));
    }

    [HttpGet("accounts/reports/sections")]
    public async Task<ActionResult<IReadOnlyList<NameAmountDto>>> AccReportSections(int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.ListSectionsAsync(session, classId, cancellationToken));
    }

    [HttpGet("accounts/reports/groups")]
    public async Task<ActionResult<IReadOnlyList<NameAmountDto>>> AccReportGroups(int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.ListGroupsAsync(session, classId, cancellationToken));
    }

    [HttpGet("accounts/reports/session/filters")]
    public async Task<ActionResult<SessionFilterDto>> AccSessionFilters(int yearId, int classId, string? roleId, string? kind, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionFiltersAsync(session, yearId, classId, roleId, kind, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/session/class")]
    public async Task<ActionResult<SessionClassReportDto>> AccSessionClass(int yearId, DateTime? from, DateTime? to, int classId, int roleId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionClassAsync(session, yearId, from, to, classId, roleId, cancellationToken));
    }

    [HttpGet("accounts/reports/session/students")]
    public async Task<ActionResult<SessionStudentReportDto>> AccSessionStudents(int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionStudentsAsync(session, yearId, classId, sectionId, roleId, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/session/paid")]
    public async Task<ActionResult<SessionStudentReportDto>> AccSessionPaid(int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionPaidAsync(session, yearId, classId, sectionId, roleId, payFor, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/session/due")]
    public async Task<ActionResult<SessionStudentReportDto>> AccSessionDue(int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionDueAsync(session, yearId, classId, sectionId, roleId, payFor, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/session/concession")]
    public async Task<ActionResult<SessionStudentReportDto>> AccSessionConcession(int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionConcessionAsync(session, yearId, classId, sectionId, roleId, from, to, cancellationToken));
    }

    [HttpGet("accounts/reports/session/paid-due")]
    public async Task<ActionResult<SessionPaidDueDto>> AccSessionPaidDue(string? status, string? classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionPaidDueAsync(session, status, classId, sectionId, roleId, payFor, from, to, page, pageSize, cancellationToken));
    }

    [HttpGet("dashboard/overview")]
    public async Task<ActionResult<DashboardOverviewDto>> DashboardOverview(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _dashboard.GetOverviewAsync(session, cancellationToken));
    }

    [HttpPost("dashboard/birthday-sms")]
    public async Task<ActionResult<SmsResult>> DashboardBirthdaySms(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.SendBirthdaySmsAsync(session, cancellationToken));
    }

    [HttpGet("exam/filters")]
    public async Task<ActionResult<ExamFilterDto>> ExamFilters(string? kind, int classId, int examId, string? groupId, string? sectionId, string? shiftId, int subjectId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetFiltersAsync(session, kind, classId, examId, groupId, sectionId, shiftId, subjectId, cancellationToken));
    }

    [HttpGet("exam/names")]
    public async Task<ActionResult<IReadOnlyList<ExamNameDto>>> ExamNames(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.ListExamsAsync(session, cancellationToken));
    }

    [HttpPost("exam/names")]
    public async Task<ActionResult<ExamResult>> ExamCreateName([FromBody] SaveExamNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.CreateExamAsync(session, request, cancellationToken));
    }

    [HttpPost("exam/names/{id:int}")]
    public async Task<ActionResult<ExamResult>> ExamUpdateName(int id, [FromBody] SaveExamNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.UpdateExamAsync(session, id, request, cancellationToken));
    }

    [HttpPost("exam/names/{id:int}/delete")]
    public async Task<ActionResult<ExamResult>> ExamDeleteName(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.DeleteExamAsync(session, id, cancellationToken));
    }

    [HttpGet("exam/sub-exams")]
    public async Task<ActionResult<IReadOnlyList<SubExamDto>>> ExamSubExams(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.ListSubExamsAsync(session, cancellationToken));
    }

    [HttpPost("exam/sub-exams")]
    public async Task<ActionResult<ExamResult>> ExamCreateSub([FromBody] SaveSubExamRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.CreateSubExamAsync(session, request, cancellationToken));
    }

    [HttpPost("exam/sub-exams/{id:int}")]
    public async Task<ActionResult<ExamResult>> ExamUpdateSub(int id, [FromBody] SaveSubExamRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.UpdateSubExamAsync(session, id, request, cancellationToken));
    }

    [HttpPost("exam/sub-exams/{id:int}/delete")]
    public async Task<ActionResult<ExamResult>> ExamDeleteSub(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.DeleteSubExamAsync(session, id, cancellationToken));
    }

    [HttpGet("exam/grading")]
    public async Task<ActionResult<IReadOnlyList<GradeSystemDto>>> ExamGrading(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.ListGradingAsync(session, cancellationToken));
    }

    [HttpPost("exam/grading")]
    public async Task<ActionResult<ExamResult>> ExamCreateGrading([FromBody] SaveGradeSystemRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.CreateGradingAsync(session, request, cancellationToken));
    }

    [HttpPost("exam/grading/{id:int}")]
    public async Task<ActionResult<ExamResult>> ExamRenameGrading(int id, [FromBody] SaveGradeSystemRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.RenameGradingAsync(session, id, request, cancellationToken));
    }

    [HttpPost("exam/grading/{id:int}/comment")]
    public async Task<ActionResult<ExamResult>> ExamGradeComment(int id, [FromBody] SaveGradeCommentRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.UpdateGradeCommentAsync(session, id, request.Comments, cancellationToken));
    }

    [HttpPost("exam/grading/{id:int}/delete")]
    public async Task<ActionResult<ExamResult>> ExamDeleteGrading(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.DeleteGradingAsync(session, id, cancellationToken));
    }

    [HttpGet("exam/pass-marks")]
    public async Task<ActionResult<IReadOnlyList<PassMarkRowDto>>> ExamPassMarks(int classId, int examId, int subExamId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.ListPassMarksAsync(session, classId, examId, subExamId, cancellationToken));
    }

    [HttpPost("exam/pass-marks")]
    public async Task<ActionResult<ExamResult>> ExamSavePassMarks([FromBody] SavePassMarksRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.SavePassMarksAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/distribution")]
    public async Task<ActionResult<DistSheetDto>> ExamDistribution(int classId, int examId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetDistributionAsync(session, classId, examId, cancellationToken));
    }

    [HttpPost("exam/distribution")]
    public async Task<ActionResult<ExamResult>> ExamSaveDistribution([FromBody] SaveDistributionRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.SaveDistributionAsync(session, request, cancellationToken));
    }

    [HttpPost("exam/distribution/copy")]
    public async Task<ActionResult<ExamResult>> ExamCopyDistribution([FromBody] CopyDistributionRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.CopyDistributionAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/collect-paper")]
    public async Task<ActionResult<CollectPaperDto>> ExamCollectPaper(int examId, int classId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetCollectPaperAsync(session, examId, classId, subjectId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpGet("exam/input")]
    public async Task<ActionResult<InputSheetDto>> ExamInputSheet(int examId, int classId, int subjectId, int subExamId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetInputSheetAsync(session, examId, classId, subjectId, subExamId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpPost("exam/input")]
    public async Task<ActionResult<ExamResult>> ExamSaveInput([FromBody] SaveInputMarksRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.SaveInputMarksAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/marks-check")]
    public async Task<ActionResult<IReadOnlyList<MarksCheckRowDto>>> ExamMarksCheck(int classId, int examId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetMarksCheckAsync(session, classId, examId, cancellationToken));
    }

    [HttpGet("exam/control")]
    public async Task<ActionResult<IReadOnlyList<ExamControlRowDto>>> ExamControl(int examId, bool cumulative, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetControlAsync(session, examId, cumulative, cancellationToken));
    }

    [HttpPost("exam/control")]
    public async Task<ActionResult<ExamResult>> ExamSaveControl([FromBody] SaveExamControlRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.SaveControlAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/publish")]
    public async Task<ActionResult<ExamPublishSettingDto>> ExamPublishSetting(int classId, int examId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetPublishSettingAsync(session, classId, examId, cancellationToken));
    }

    [HttpPost("exam/publish")]
    public async Task<ActionResult<ExamResult>> ExamPublish([FromBody] ExamPublishRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.PublishResultAsync(session, request, cancellationToken));
    }

    [HttpPost("exam/delete-result")]
    public async Task<ActionResult<ExamResult>> ExamDeleteResult([FromBody] ExamDeleteResultRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.DeletePublishedResultAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/merit")]
    public async Task<ActionResult<ExamMeritListDto>> ExamMerit(int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? passStatus, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetMeritListAsync(session, classId, examId, groupId, sectionId, shiftId, passStatus, cancellationToken));
    }

    [HttpGet("exam/merit-subject")]
    public async Task<ActionResult<ExamMeritListDto>> ExamMeritSubject(int classId, int examId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetMeritSubjectAsync(session, classId, examId, subjectId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpGet("exam/result-cards")]
    public async Task<ActionResult<ExamResultCardSheetDto>> ExamResultCards(int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetResultCardsAsync(session, classId, examId, groupId, sectionId, shiftId, studentIds, cancellationToken));
    }

    [HttpGet("exam/analytical")]
    public async Task<ActionResult<ExamAnalyticalDto>> ExamAnalytical(int classId, int examId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetAnalyticalAsync(session, classId, examId, cancellationToken));
    }

    [HttpGet("exam/cumulative/names")]
    public async Task<ActionResult<IReadOnlyList<ExamOptionDto>>> ExamCumulativeNames(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.ListCumulativeNamesAsync(session, cancellationToken));
    }

    [HttpPost("exam/cumulative/names")]
    public async Task<ActionResult<ExamResult>> ExamCreateCumulativeName([FromBody] SaveCumulativeNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.CreateCumulativeNameAsync(session, request, cancellationToken));
    }

    [HttpPost("exam/cumulative/names/{id:int}")]
    public async Task<ActionResult<ExamResult>> ExamUpdateCumulativeName(int id, [FromBody] SaveCumulativeNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.UpdateCumulativeNameAsync(session, id, request, cancellationToken));
    }

    [HttpGet("exam/cumulative/publish")]
    public async Task<ActionResult<CumulativePublishSettingDto>> ExamCumulativePublishSetting(int classId, int examId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetCumulativePublishSettingAsync(session, classId, examId, cancellationToken));
    }

    [HttpPost("exam/cumulative/publish")]
    public async Task<ActionResult<ExamResult>> ExamCumulativePublish([FromBody] CumulativePublishRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.PublishCumulativeResultAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/cumulative/merit")]
    public async Task<ActionResult<ExamMeritListDto>> ExamCumulativeMerit(int classId, int examId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetCumulativeMeritAsync(session, classId, examId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpGet("exam/cumulative/result-cards")]
    public async Task<ActionResult<CumulativeResultCardSheetDto>> ExamCumulativeResultCards(int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetCumulativeResultCardsAsync(session, classId, examId, groupId, sectionId, shiftId, studentIds, cancellationToken));
    }

    [HttpGet("exam/seat-plan")]
    public async Task<ActionResult<ExamSeatPlanSheetDto>> ExamSeatPlan(int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? classIds, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetSeatPlanAsync(session, classId, examId, groupId, sectionId, shiftId, studentIds, classIds, cancellationToken));
    }

    [HttpPost("exam/seat-plan/random")]
    public async Task<ActionResult<ExamResult>> ExamSeatPlanRandom([FromBody] RandomSeatRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.RandomizeSeatNumbersAsync(session, request, cancellationToken));
    }

    [HttpGet("exam/admit-cards")]
    public async Task<ActionResult<ExamAdmitCardSheetDto>> ExamAdmitCards(int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? paymentStatus, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.GetAdmitCardsAsync(session, classId, examId, groupId, sectionId, shiftId, studentIds, paymentStatus, cancellationToken));
    }

    [HttpPost("exam/admit-sign")]
    public async Task<ActionResult<ExamResult>> ExamAdmitSign([FromBody] SaveExamSignRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _exams.SaveExamSignAsync(session, request, cancellationToken));
    }

    [HttpGet("sms/balance")]
    public async Task<ActionResult<SmsBalanceDto>> SmsBalance(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetBalanceAsync(session, cancellationToken));
    }

    [HttpGet("sms/students")]
    public async Task<ActionResult<IReadOnlyList<SmsStudentDto>>> SmsStudents(
        int classId, int groupId, int sectionId, int shiftId, string? ids, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetStudentsAsync(session, classId, groupId, sectionId, shiftId, ids, cancellationToken));
    }

    [HttpGet("sms/teachers")]
    public async Task<ActionResult<IReadOnlyList<SmsTeacherDto>>> SmsTeachers(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetTeachersAsync(session, cancellationToken));
    }

    [HttpPost("sms/send")]
    public async Task<ActionResult<SmsResult>> SmsSend([FromBody] SendOfficeSmsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.SendAsync(session, request, cancellationToken));
    }

    [HttpGet("sms/groups")]
    public async Task<ActionResult<IReadOnlyList<SmsGroupDto>>> SmsGroups(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetGroupsAsync(session, cancellationToken));
    }

    [HttpPost("sms/groups")]
    public async Task<ActionResult<SmsResult>> SmsSaveGroup([FromBody] SaveSmsGroupRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.SaveGroupAsync(session, request, cancellationToken));
    }

    [HttpPost("sms/groups/{id:int}/delete")]
    public async Task<ActionResult<SmsResult>> SmsDeleteGroup(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.DeleteGroupAsync(session, id, cancellationToken));
    }

    [HttpGet("sms/contacts")]
    public async Task<ActionResult<IReadOnlyList<SmsContactDto>>> SmsContacts(int groupId, string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetContactsAsync(session, groupId, q, cancellationToken));
    }

    [HttpPost("sms/contacts")]
    public async Task<ActionResult<SmsResult>> SmsSaveContact([FromBody] SaveSmsContactRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.SaveContactAsync(session, request, cancellationToken));
    }

    [HttpPost("sms/contacts/{id:int}/delete")]
    public async Task<ActionResult<SmsResult>> SmsDeleteContact(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.DeleteContactAsync(session, id, cancellationToken));
    }

    [HttpGet("sms/records")]
    public async Task<ActionResult<SmsRecordsDto>> SmsRecords(
        DateTime? from, DateTime? to, string? q, string? kind, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetRecordsAsync(session, from, to, q, kind, page, pageSize, cancellationToken));
    }

    [HttpGet("sms/recharge")]
    public async Task<ActionResult<SmsRechargePageDto>> SmsRecharge(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.GetRechargeAsync(session, cancellationToken));
    }

    [HttpPost("sms/recharge")]
    public async Task<ActionResult<SmsResult>> SmsStartRecharge([FromBody] SmsRechargeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _officeSms.StartRechargeAsync(session, request, cancellationToken));
    }

    [HttpGet("sms/templates")]
    public async Task<ActionResult<IReadOnlyList<SmsTemplateDto>>> SmsTemplates(string? category, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _smsTemplates.ListAsync(session, category, cancellationToken));
    }

    [HttpGet("sms/templates/committee-payment-lang")]
    public async Task<ActionResult<CommitteePaymentSmsLangDto>> CommitteePaymentSmsLang(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _smsTemplates.GetDonorPaymentLangAsync(session, cancellationToken));
    }

    [HttpPost("sms/templates/committee-payment-lang")]
    public async Task<ActionResult<SmsTemplateResult>> CommitteePaymentSmsLangSave(
        [FromBody] CommitteePaymentSmsLangDto request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _smsTemplates.SaveDonorPaymentLangAsync(session, request, cancellationToken));
    }

    [HttpGet("sms/templates/{id:int}")]
    public async Task<ActionResult<SmsTemplateDto>> SmsTemplate(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var dto = await _smsTemplates.GetAsync(session, id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("sms/templates")]
    public async Task<ActionResult<SmsTemplateResult>> SmsTemplateSave([FromBody] SaveSmsTemplateRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _smsTemplates.SaveAsync(session, request, cancellationToken));
    }

    [HttpPost("sms/templates/{id:int}/delete")]
    public async Task<ActionResult<SmsTemplateResult>> SmsTemplateDelete(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _smsTemplates.DeleteAsync(session, id, cancellationToken));
    }

    [HttpGet("routine/names")]
    public async Task<ActionResult<IReadOnlyList<RoutineNameDto>>> RoutineNames(bool unusedOnly, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.GetNamesAsync(session, unusedOnly, cancellationToken));
    }

    [HttpPost("routine/names")]
    public async Task<ActionResult<RoutineResult>> RoutineSaveName([FromBody] SaveRoutineNameRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.SaveNameAsync(session, request, cancellationToken));
    }

    [HttpPost("routine/names/{id:int}/delete")]
    public async Task<ActionResult<RoutineResult>> RoutineDeleteName(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.DeleteNameAsync(session, id, cancellationToken));
    }

    [HttpPost("routine/create")]
    public async Task<ActionResult<RoutineResult>> RoutineCreate([FromBody] CreateClassRoutineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.CreateAsync(session, request, cancellationToken));
    }

    [HttpGet("routine/assign")]
    public async Task<ActionResult<ClassRoutineSheetDto>> RoutineAssign(
        int classId, int groupId, int sectionId, int shiftId, int routineInfoId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.GetAssignSheetAsync(session, classId, groupId, sectionId, shiftId, routineInfoId, cancellationToken));
    }

    [HttpGet("routine/teachers")]
    public async Task<ActionResult<IReadOnlyList<RoutineOptionDto>>> RoutineTeachers(
        int classId, int subjectId, string day, string start, string end, int exceptRoutineInfoId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.GetTeachersAsync(session, classId, subjectId, day ?? "", start ?? "", end ?? "", exceptRoutineInfoId, cancellationToken));
    }

    [HttpPost("routine/assign")]
    public async Task<ActionResult<RoutineResult>> RoutineAssignSave([FromBody] AssignRoutineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.AssignAsync(session, request, cancellationToken));
    }

    [HttpGet("routine/view")]
    public async Task<ActionResult<ClassRoutineSheetDto>> RoutineView(
        int classId, int groupId, int sectionId, int shiftId, int routineInfoId, bool edit, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.GetViewSheetAsync(session, classId, groupId, sectionId, shiftId, routineInfoId, edit, cancellationToken));
    }

    [HttpPost("routine/update")]
    public async Task<ActionResult<RoutineResult>> RoutineUpdate([FromBody] AssignRoutineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.UpdateAsync(session, request, cancellationToken));
    }

    [HttpPost("routine/delete-class")]
    public async Task<ActionResult<RoutineResult>> RoutineDeleteClass([FromBody] AssignRoutineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.DeleteClassRoutineAsync(session, request, cancellationToken));
    }

    [HttpGet("routine/exam")]
    public async Task<ActionResult<ExamRoutineSheetDto>> RoutineExam(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.GetExamAsync(session, id, cancellationToken));
    }

    [HttpGet("routine/exam/subjects")]
    public async Task<ActionResult<IReadOnlyList<RoutineOptionDto>>> RoutineExamSubjects(int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.GetExamSubjectsAsync(session, classId, cancellationToken));
    }

    [HttpPost("routine/exam")]
    public async Task<ActionResult<RoutineResult>> RoutineExamSave([FromBody] SaveExamRoutineRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.SaveExamAsync(session, request, cancellationToken));
    }

    [HttpPost("routine/exam/{id:int}/delete")]
    public async Task<ActionResult<RoutineResult>> RoutineExamDelete(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _routines.DeleteExamAsync(session, id, cancellationToken));
    }

    [HttpGet("committee/lookups")]
    public async Task<ActionResult<CommitteeLookupsDto>> CommitteeLookups(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetLookupsAsync(session, cancellationToken));
    }

    [HttpGet("committee/types")]
    public async Task<ActionResult<IReadOnlyList<CommitteeMemberTypeDto>>> CommitteeTypes(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetMemberTypesAsync(session, cancellationToken));
    }

    [HttpPost("committee/types")]
    public async Task<ActionResult<CommitteeResult>> CommitteeSaveType([FromBody] SaveCommitteeMemberTypeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SaveMemberTypeAsync(session, request, cancellationToken));
    }

    [HttpPost("committee/types/{id:int}/delete")]
    public async Task<ActionResult<CommitteeResult>> CommitteeDeleteType(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.DeleteMemberTypeAsync(session, id, cancellationToken));
    }

    [HttpGet("committee/members")]
    public async Task<ActionResult<IReadOnlyList<CommitteeMemberDto>>> CommitteeMembers(int typeId, string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetMembersAsync(session, typeId, q, cancellationToken));
    }

    [HttpPost("committee/members")]
    public async Task<ActionResult<CommitteeResult>> CommitteeSaveMember([FromBody] SaveCommitteeMemberRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SaveMemberAsync(session, request, cancellationToken));
    }

    [HttpGet("committee/categories")]
    public async Task<ActionResult<IReadOnlyList<DonationCategoryDto>>> CommitteeCategories(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetCategoriesAsync(session, cancellationToken));
    }

    [HttpPost("committee/categories")]
    public async Task<ActionResult<CommitteeResult>> CommitteeSaveCategory([FromBody] SaveDonationCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SaveCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("committee/categories/{id:int}/delete")]
    public async Task<ActionResult<CommitteeResult>> CommitteeDeleteCategory(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.DeleteCategoryAsync(session, id, cancellationToken));
    }

    [HttpGet("committee/donors")]
    public async Task<ActionResult<IReadOnlyList<DonorSuggestDto>>> CommitteeDonors(string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SuggestDonorsAsync(session, q, cancellationToken));
    }

    [HttpPost("committee/donations")]
    public async Task<ActionResult<CommitteeResult>> CommitteeAddDonation([FromBody] AddDonationRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.AddDonationAsync(session, request, cancellationToken));
    }

    [HttpGet("committee/donations")]
    public async Task<ActionResult<DonationListDto>> CommitteeDonations(int memberId, int categoryId, string? paid, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonationsAsync(session, memberId, categoryId, paid, cancellationToken));
    }

    [HttpPost("committee/donations/update")]
    public async Task<ActionResult<CommitteeResult>> CommitteeUpdateDonation([FromBody] UpdateDonationRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.UpdateDonationAsync(session, request, cancellationToken));
    }

    [HttpPost("committee/donations/{id:int}/delete")]
    public async Task<ActionResult<CommitteeResult>> CommitteeDeleteDonation(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.DeleteDonationAsync(session, id, cancellationToken));
    }

    [HttpGet("committee/members/{id:int}/photo")]
    public async Task<ActionResult<CommitteeMemberDto>> CommitteeMemberPhoto(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(new CommitteeMemberDto { PhotoDataUrl = await _committee.GetMemberPhotoAsync(session, id, cancellationToken) });
    }

    [HttpGet("committee/collect")]
    public async Task<ActionResult<CollectPageDto>> CommitteeCollect(int memberId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetCollectAsync(session, memberId, cancellationToken));
    }

    [HttpPost("committee/collect")]
    public async Task<ActionResult<CommitteeResult>> CommitteeCollectSave([FromBody] CollectDonationRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var result = await _committee.CollectAsync(session, request, cancellationToken);
        if (result.Succeeded && request.SendSms && result.ReceiptId > 0)
            _ = _committee.SendDonorReceiptSmsAsync(session, result.ReceiptId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("committee/payments")]
    public async Task<ActionResult<PaymentRecordListDto>> CommitteePayments(
        int yearId, int categoryId, int memberId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetPaymentsAsync(session, yearId, categoryId, memberId, from, to, cancellationToken));
    }

    [HttpGet("committee/unpaid")]
    public async Task<ActionResult<UnpaidReceiptDto>> CommitteeUnpaid(string? sn, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetUnpaidAsync(session, sn, cancellationToken));
    }

    [HttpPost("committee/unpaid")]
    public async Task<ActionResult<CommitteeResult>> CommitteeUnpaidSave([FromBody] UnpaidReceiptRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.UnpaidAsync(session, request?.Sn, cancellationToken));
    }

    [HttpGet("committee/receipt/{id:int}")]
    public async Task<ActionResult<DonationReceiptDto>> CommitteeReceipt(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var dto = await _committee.GetReceiptAsync(session, id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("committee/receipt/{id:int}/sms")]
    public async Task<ActionResult<AccountsResult>> CommitteeReceiptSms(int id, [FromBody] DonorReceiptSmsRequest? request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SendDonorReceiptSmsAsync(session, id, request, cancellationToken));
    }

    [HttpGet("committee/donation-pay-order/template")]
    public async Task<ActionResult<decimal?>> CommitteeDonationTemplate(int typeId, int categoryId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonationTemplateAmountAsync(session, typeId, categoryId, cancellationToken));
    }

    [HttpGet("committee/donation-pay-order/months")]
    public async Task<ActionResult<IReadOnlyList<DonationPayOrderMonthDto>>> CommitteePayOrderMonths(string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetPayOrderMonthsAsync(session, q, cancellationToken));
    }

    [HttpPost("committee/donation-pay-order")]
    public async Task<ActionResult<DonationPayOrderResult>> CommitteeCreatePayOrders([FromBody] CreateDonationPayOrdersRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.CreateDonationPayOrdersAsync(session, request, cancellationToken));
    }

    [HttpGet("committee/donation-bulk-edit")]
    public async Task<ActionResult<DonationBulkEditListDto>> CommitteeDonationBulkEdit(
        int typeId, int memberId, string? name, string? phone, int categoryId, string? status, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonationBulkEditAsync(session, typeId, memberId, name, phone, categoryId, status, cancellationToken));
    }

    [HttpGet("committee/donation-bulk-edit/donors")]
    public async Task<ActionResult<IReadOnlyList<DonorSuggestDto>>> CommitteeBulkEditDonors(string? name, string? phone, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SearchDonorsBulkAsync(session, name, phone, cancellationToken));
    }

    [HttpPost("committee/donation-bulk-edit/update")]
    public async Task<ActionResult<DonationBulkEditResult>> CommitteeBulkUpdateDonations([FromBody] BulkEditDonationsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.BulkUpdateDonationsAsync(session, request, cancellationToken));
    }

    [HttpPost("committee/donation-bulk-edit/delete")]
    public async Task<ActionResult<DonationBulkEditResult>> CommitteeBulkDeleteDonations([FromBody] BulkDeleteDonationsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.BulkDeleteDonationsAsync(session, request, cancellationToken));
    }

    [HttpGet("committee/donor-due/summary")]
    public async Task<ActionResult<DonorDueSummaryDto>> CommitteeDonorDueSummary(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonorDueSummaryAsync(session, cancellationToken));
    }

    [HttpGet("committee/donor-due/categories")]
    public async Task<ActionResult<IReadOnlyList<CommitteeOptionDto>>> CommitteeDonorDueCategories(int typeId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonorDueCategoriesAsync(session, typeId, cancellationToken));
    }

    [HttpGet("committee/donor-due/by-type")]
    public async Task<ActionResult<DonorDueByTypeListDto>> CommitteeDonorDueByType(int typeId, int categoryId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonorDueByTypeAsync(session, typeId, categoryId, cancellationToken));
    }

    [HttpGet("committee/donor-due/by-name")]
    public async Task<ActionResult<DonorDueMemberDetailDto>> CommitteeDonorDueByName(string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonorDueByNameAsync(session, q, cancellationToken));
    }

    [HttpPost("committee/donor-due/view")]
    public async Task<ActionResult<IReadOnlyList<DonorDueViewBlockDto>>> CommitteeDonorDueView([FromBody] DonorDueViewRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonorDueViewAsync(session, request, cancellationToken));
    }

    [HttpPost("committee/donor-due/sms")]
    public async Task<ActionResult<DonorDueSmsResult>> CommitteeDonorDueSms([FromBody] DonorDueSmsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SendDonorDueSmsAsync(session, request, cancellationToken));
    }

    [HttpGet("committee/donor-login")]
    public async Task<ActionResult<DonorLoginPageDto>> CommitteeDonorLogin(int typeId, string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.GetDonorLoginPageAsync(session, typeId, q, cancellationToken));
    }

    [HttpPost("committee/donor-login/create")]
    public async Task<ActionResult<DonorLoginCreateResult>> CommitteeDonorLoginCreate([FromBody] DonorLoginCreateRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.CreateDonorLoginsAsync(session, request, cancellationToken));
    }

    [HttpPost("committee/donor-login/sms")]
    public async Task<ActionResult<DonorLoginSmsResult>> CommitteeDonorLoginSms([FromBody] DonorLoginSmsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _committee.SendDonorLoginSmsAsync(session, request, cancellationToken));
    }

    [HttpGet("inventory/lookups")]
    public async Task<ActionResult<InventoryLookupsDto>> InvLookups(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.GetLookupsAsync(session, cancellationToken));
    }

    [HttpGet("inventory/categories")]
    public async Task<ActionResult<IReadOnlyList<InventoryCategoryDto>>> InvCategories(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.ListCategoriesAsync(session, cancellationToken));
    }

    [HttpPost("inventory/categories")]
    public async Task<ActionResult<InventoryResult>> InvSaveCategory([FromBody] SaveInventoryCategoryRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SaveCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("inventory/categories/{id:int}/delete")]
    public async Task<ActionResult<InventoryResult>> InvDeleteCategory(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.DeleteCategoryAsync(session, id, cancellationToken));
    }

    [HttpGet("inventory/items")]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> InvItems(int categoryId = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.ListItemsAsync(session, categoryId, cancellationToken));
    }

    [HttpPost("inventory/items")]
    public async Task<ActionResult<InventoryResult>> InvSaveItem([FromBody] SaveInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SaveItemAsync(session, request, cancellationToken));
    }

    [HttpPost("inventory/items/{id:int}/delete")]
    public async Task<ActionResult<InventoryResult>> InvDeleteItem(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.DeleteItemAsync(session, id, cancellationToken));
    }

    [HttpGet("inventory/suppliers")]
    public async Task<ActionResult<IReadOnlyList<InventorySupplierDto>>> InvSuppliers(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.ListSuppliersAsync(session, cancellationToken));
    }

    [HttpPost("inventory/suppliers")]
    public async Task<ActionResult<InventoryResult>> InvSaveSupplier([FromBody] SaveInventorySupplierRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SaveSupplierAsync(session, request, cancellationToken));
    }

    [HttpPost("inventory/suppliers/{id:int}/delete")]
    public async Task<ActionResult<InventoryResult>> InvDeleteSupplier(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.DeleteSupplierAsync(session, id, cancellationToken));
    }

    [HttpGet("inventory/suppliers/{id:int}/ledger")]
    public async Task<ActionResult<InventorySupplierLedgerDto>> InvSupplierLedger(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.GetSupplierLedgerAsync(session, id, cancellationToken));
    }

    [HttpPost("inventory/supplier-payments")]
    public async Task<ActionResult<InventoryResult>> InvSaveSupplierPayment([FromBody] SaveInventorySupplierPaymentRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SaveSupplierPaymentAsync(session, request, cancellationToken));
    }

    [HttpGet("inventory/customers/students")]
    public async Task<ActionResult<IReadOnlyList<InventoryStudentHitDto>>> InvSaleStudents(string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SuggestSaleStudentsAsync(session, q, cancellationToken));
    }

    [HttpGet("inventory/customers/from-student")]
    public async Task<ActionResult<InventoryCustomerDto>> InvCustomerFromStudent(string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.CustomerFromStudentAsync(session, id, cancellationToken) ?? new InventoryCustomerDto());
    }

    [HttpGet("inventory/customers")]
    public async Task<ActionResult<IReadOnlyList<InventoryCustomerDto>>> InvCustomers(string? name, string? phone, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SearchCustomersAsync(session, name, phone, cancellationToken));
    }

    [HttpPost("inventory/customers")]
    public async Task<ActionResult<InventoryResult>> InvSaveCustomer([FromBody] SaveInventoryCustomerRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SaveWalkInCustomerAsync(session, request, cancellationToken));
    }

    [HttpGet("inventory/purchases")]
    public async Task<ActionResult<InventoryDocListDto>> InvPurchases(DateTime? from, DateTime? to, int itemId = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.ListPurchasesAsync(session, from, to, itemId, cancellationToken));
    }

    [HttpGet("inventory/purchases/{id:int}")]
    public async Task<ActionResult<InventoryDocDto>> InvPurchase(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.GetPurchaseAsync(session, id, cancellationToken) ?? new InventoryDocDto());
    }

    [HttpPost("inventory/purchases")]
    public async Task<ActionResult<InventoryResult>> InvSavePurchase([FromBody] SaveInventoryDocRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.SavePurchaseAsync(session, request, cancellationToken));
    }

    [HttpPost("inventory/purchases/{id:int}/delete")]
    public async Task<ActionResult<InventoryResult>> InvDeletePurchase(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.DeletePurchaseAsync(session, id, cancellationToken));
    }

    [HttpGet("inventory/sales")]
    public async Task<ActionResult<InventoryDocListDto>> InvSales(DateTime? from, DateTime? to, int itemId = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.ListSalesAsync(session, from, to, itemId, cancellationToken));
    }

    [HttpGet("inventory/sales/{id:int}")]
    public async Task<ActionResult<InventoryDocDto>> InvSale(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.GetSaleAsync(session, id, cancellationToken) ?? new InventoryDocDto());
    }

    [HttpPost("inventory/sales")]
    public async Task<ActionResult<InventoryResult>> InvSaveSale([FromBody] SaveInventoryDocRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var result = await _inventory.SaveSaleAsync(session, request, cancellationToken);
        if (result.Succeeded && request.SendSms && result.Id > 0)
            _ = _sms.SendInventorySaleAsync(session, result.Id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("inventory/sales/{id:int}/delete")]
    public async Task<ActionResult<InventoryResult>> InvDeleteSale(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.DeleteSaleAsync(session, id, cancellationToken));
    }

    [HttpPost("inventory/sales/{id:int}/sms")]
    public async Task<ActionResult<AccountsResult>> InvSaleSms(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _sms.SendInventorySaleAsync(session, id, cancellationToken));
    }

    [HttpGet("inventory/stock")]
    public async Task<ActionResult<InventoryStockDto>> InvStock(int categoryId = 0, CancellationToken cancellationToken = default)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _inventory.GetStockAsync(session, categoryId, cancellationToken));
    }

    [HttpGet("support")]
    public async Task<ActionResult<SupportPageDto>> SupportPage(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _support.GetPageAsync(session, cancellationToken));
    }

    [HttpPost("support")]
    public async Task<ActionResult<SupportResult>> SubmitSupport(
        [FromBody] SubmitSupportRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _support.SubmitAsync(session, request, cancellationToken));
    }

    [HttpGet("invoice/status")]
    public async Task<ActionResult<SubscriptionStatusDto>> InvoiceStatus(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _invoice.GetStatusAsync(session, cancellationToken));
    }

    [HttpGet("invoice/due")]
    public async Task<ActionResult<DueInvoiceDto>> InvoiceDue(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _invoice.GetDueAsync(session, cancellationToken));
    }

    [HttpPost("invoice/pay")]
    public async Task<ActionResult<InvoiceResult>> InvoicePay(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _invoice.PayDueAsync(session, cancellationToken));
    }

    [HttpGet("invoice/paid")]
    public async Task<ActionResult<PaidInvoiceListDto>> InvoicePaid(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _invoice.GetPaidAsync(session, cancellationToken));
    }

    [HttpGet("invoice/receipt/{id:int}")]
    public async Task<ActionResult<PaidInvoiceReceiptDto>> InvoiceReceipt(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _invoice.GetReceiptAsync(session, id, cancellationToken));
    }

    [HttpGet("authority/dashboard")]
    public async Task<ActionResult<AuthorityDashboardDto>> AuthorityDashboard(
        [FromQuery] string? q,
        [FromQuery] string? validation,
        [FromQuery] string? live,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!session.IsAuthority)
            return Forbid();
        return Ok(await _authority.GetDashboardAsync(session, cancellationToken));
    }

    [HttpGet("authority/institutions")]
    public async Task<ActionResult<AuthorityDashboardDto>> AuthorityInstitutions(
        [FromQuery] string? q,
        [FromQuery] string? validation,
        [FromQuery] string? live,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!session.IsAuthority)
            return Forbid();
        return Ok(await _authority.GetInstitutionsAsync(session, q, validation, live, from, to, cancellationToken));
    }

    [HttpGet("authority/institutions/{id:int}")]
    public async Task<ActionResult<InstitutionDetailsDto>> AuthorityInstitutionDetails(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!session.IsAuthority)
            return Forbid();
        return Ok(await _authority.GetInstitutionDetailsAsync(session, id, cancellationToken));
    }

    [HttpPost("authority/institutions/years")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveYears([FromBody] SaveInstitutionYearsRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        if (!session.IsAuthority)
            return Forbid();
        return Ok(await _authority.SaveInstitutionYearsAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/institutions/sms-recharge")]
    public async Task<ActionResult<AuthorityResult>> AuthorityInstSmsRecharge(
        [FromBody] InstSmsRechargeRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.RechargeInstitutionSmsAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/institutions/due-notice")]
    public async Task<ActionResult<AuthorityResult>> AuthorityInstDueNotice(
        [FromBody] InstDueNoticeRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.SaveDueNoticeAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/institutions/{id:int}/student")]
    public async Task<ActionResult<InstStudentFindDto>> AuthorityInstStudent(int id, [FromQuery] string? q, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.FindStudentAsync(session, id, q, cancellationToken));
    }

    [HttpPost("authority/institutions/delete-student")]
    public async Task<ActionResult<AuthorityResult>> AuthorityInstDeleteStudent(
        [FromBody] InstIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.DeleteStudentIdAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/institutions/change-student-id")]
    public async Task<ActionResult<AuthorityResult>> AuthorityInstChangeId(
        [FromBody] InstChangeIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.ChangeStudentIdAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/institutions/{id:int}/receipt")]
    public async Task<ActionResult<InstReceiptDto>> AuthorityInstReceipt(int id, [FromQuery] string? sn, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.FindReceiptAsync(session, id, sn, cancellationToken));
    }

    [HttpPost("authority/institutions/delete-receipt")]
    public async Task<ActionResult<AuthorityResult>> AuthorityInstDeleteReceipt(
        [FromBody] InstReceiptRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.DeleteReceiptAsync(session, request, cancellationToken));
    }

    private ActionResult? AuthorityOrForbid(out SessionSnapshot session)
    {
        session = JwtTokenService.FromPrincipal(User);
        return session.IsAuthority ? null : Forbid();
    }

    [HttpGet("authority/signup/lookups")]
    public async Task<ActionResult<SignupLookupsDto>> AuthoritySignupLookups(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetSignupLookupsAsync(session, cancellationToken));
    }

    [HttpPost("authority/signup/user")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySignupUser(
        [FromBody] SignupUserRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.CreateSignupUserAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/signup/institution")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySignupInstitution(
        [FromBody] SignupInstitutionRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.CreateInstitutionAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/user-info")]
    public async Task<ActionResult<UserInfoListDto>> AuthorityUserInfo(
        [FromQuery] string? q, [FromQuery] string? validation, [FromQuery] string? password, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetUserInfoAsync(session, q, validation, password, cancellationToken));
    }

    [HttpGet("authority/user-info/users")]
    public async Task<ActionResult<IReadOnlyList<SchoolUserDto>>> AuthoritySchoolUsers(
        [FromQuery] int schoolId, [FromQuery] string? category, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetSchoolUsersAsync(session, schoolId, category, cancellationToken));
    }

    [HttpPost("authority/user-info/approve")]
    public async Task<ActionResult<AuthorityResult>> AuthorityApprove(
        [FromBody] SetApprovedRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SetApprovedAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/user-info/unlock")]
    public async Task<ActionResult<AuthorityResult>> AuthorityUnlock(
        [FromBody] UnlockUserRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.UnlockUserAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/testimonials")]
    public async Task<ActionResult<IReadOnlyList<TestimonialRowDto>>> AuthorityTestimonials(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetTestimonialsAsync(session, cancellationToken));
    }

    [HttpPost("authority/testimonials")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveTestimonial(
        [FromBody] SaveTestimonialRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SaveTestimonialAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/testimonials/show")]
    public async Task<ActionResult<AuthorityResult>> AuthorityShowTestimonial(
        [FromBody] SetTestimonialShowRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SetTestimonialShowAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/reset/schools")]
    public async Task<ActionResult<IReadOnlyList<ResetSchoolOptionDto>>> AuthorityResetSchools(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetResetSchoolsAsync(session, cancellationToken));
    }

    [HttpGet("authority/reset/years")]
    public async Task<ActionResult<IReadOnlyList<ResetYearOptionDto>>> AuthorityResetYears(
        [FromQuery] int schoolId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetResetYearsAsync(session, schoolId, cancellationToken));
    }

    [HttpGet("authority/reset/preview")]
    public async Task<ActionResult<ResetPreviewDto>> AuthorityResetPreview(
        [FromQuery] int schoolId, [FromQuery] string? mode, [FromQuery] int educationYearId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.PreviewResetAsync(session, schoolId, mode ?? "", educationYearId, cancellationToken));
    }

    [HttpGet("authority/reset/progress")]
    public async Task<ActionResult<ResetProgressDto>> AuthorityResetProgress(
        [FromQuery] int schoolId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetResetProgressAsync(session, schoolId, cancellationToken));
    }

    [HttpPost("authority/reset/execute")]
    public async Task<ActionResult<AuthorityResult>> AuthorityResetExecute(
        [FromBody] ResetExecuteRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.StartResetAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/reset/image-preview")]
    public async Task<ActionResult<ResetPreviewDto>> AuthorityResetImagePreview(
        [FromQuery] int schoolId, [FromQuery] string? yearIds, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.PreviewResetImagesAsync(session, schoolId, ParseIds(yearIds), cancellationToken));
    }

    [HttpPost("authority/reset/delete-images")]
    public async Task<ActionResult<ResetPreviewDto>> AuthorityResetDeleteImages(
        [FromBody] ResetImageRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.DeleteResetImagesAsync(session, request, cancellationToken));
    }

    private static List<int> ParseIds(string? raw)
    {
        var ids = new List<int>();
        if (string.IsNullOrWhiteSpace(raw))
            return ids;
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var id) && id > 0)
                ids.Add(id);
        }
        return ids;
    }

    [HttpGet("authority/attendance")]
    public async Task<ActionResult<AttSignupPageDto>> AuthorityAttendance(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetAttendanceSignupAsync(session, cancellationToken));
    }

    [HttpPost("authority/attendance/register")]
    public async Task<ActionResult<AuthorityResult>> AuthorityAttendanceRegister(
        [FromBody] AttRegisterRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.RegisterAttendanceAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/attendance/password")]
    public async Task<ActionResult<AuthorityResult>> AuthorityAttendancePassword(
        [FromBody] AttPasswordRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SetAttendancePasswordAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/attendance/active")]
    public async Task<ActionResult<AuthorityResult>> AuthorityAttendanceActive(
        [FromBody] AttActiveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SetAttendanceActiveAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/sms-setting")]
    public async Task<ActionResult<SmsSettingPageDto>> AuthoritySmsSetting(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetSmsSettingAsync(session, cancellationToken));
    }

    [HttpPost("authority/sms-setting")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveSmsSetting(
        [FromBody] SaveSmsSettingRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SaveSmsSettingAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/sms-setting/records")]
    public async Task<ActionResult<IReadOnlyList<SmsSenderRowDto>>> AuthoritySmsRecords(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetSmsSenderRecordsAsync(session, from, to, cancellationToken));
    }

    [HttpGet("authority/sms-setting/failed")]
    public async Task<ActionResult<SmsFailedPageDto>> AuthoritySmsFailed(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? reason, [FromQuery] int schoolId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetFailedSmsAsync(session, from, to, reason, schoolId, cancellationToken));
    }

    [HttpGet("authority/client-sms")]
    public async Task<ActionResult<ClientSmsPageDto>> AuthorityClientSms(
        [FromQuery] string? q, [FromQuery] string? validation, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetClientSmsAsync(session, q, validation, cancellationToken));
    }

    [HttpPost("authority/client-sms")]
    public async Task<ActionResult<SendClientSmsResult>> AuthoritySendClientSms(
        [FromBody] SendClientSmsRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SendClientSmsAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/accounts")]
    public async Task<ActionResult<AuthAccountsPageDto>> AuthorityAccounts(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetAccountsAsync(session, cancellationToken));
    }

    [HttpGet("authority/progress")]
    public async Task<ActionResult<AuthProgressPageDto>> AuthorityProgress([FromQuery] string? filter, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetProgressAsync(session, filter, cancellationToken));
    }

    [HttpGet("authority/collection")]
    public async Task<ActionResult<AuthCollectPageDto>> AuthorityCollection(
        [FromQuery] int categoryId, [FromQuery] string? month, [FromQuery] string? detail, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetCollectAsync(session, categoryId, month, detail, cancellationToken));
    }

    [HttpGet("authority/manage")]
    public async Task<ActionResult<AuthManagePageDto>> AuthorityManage(
        [FromQuery] string? q, [FromQuery] string? validation, [FromQuery] string? payment, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetManageAsync(session, q, validation, payment, cancellationToken));
    }

    [HttpPost("authority/manage")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveManage(
        [FromBody] AuthManageSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.SaveManageAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/invoice/create")]
    public async Task<ActionResult<AuthCreatePageDto>> AuthorityCreatePage(
        [FromQuery] string? month, [FromQuery] int otherSchoolId, [FromQuery] string? smsFrom, [FromQuery] string? smsTo, [FromQuery] string? smsQ, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetCreatePageAsync(session, month, otherSchoolId, smsFrom, smsTo, smsQ, cancellationToken));
    }

    [HttpPost("authority/invoice/generate-count")]
    public async Task<ActionResult<AuthorityResult>> AuthorityGenerateCount(
        [FromBody] AuthGenerateCountRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GenerateStudentCountAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/invoice/auto-generate")]
    public async Task<ActionResult<AuthorityResult>> AuthorityAutoGenerate(
        [FromBody] AuthGenerateCountRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.AutoGenerateAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/invoice/enable-job")]
    public async Task<ActionResult<AuthorityResult>> AuthorityEnableJob(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.EnableJobAsync(session, cancellationToken));
    }

    [HttpPost("authority/invoice/service")]
    public async Task<ActionResult<AuthorityResult>> AuthorityCreateService(
        [FromBody] AuthCreateServiceRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.CreateServiceInvoicesAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/invoice/category")]
    public async Task<ActionResult<AuthorityResult>> AuthorityAddCategory(
        [FromBody] AuthAddCategoryRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.AddCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/invoice/other")]
    public async Task<ActionResult<AuthorityResult>> AuthorityCreateOther(
        [FromBody] AuthCreateOtherRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.CreateOtherInvoiceAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/invoice/other/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteOther(
        [FromBody] AuthIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.DeleteOtherInvoiceAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpPost("authority/invoice/grace")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySetGrace(
        [FromBody] AuthGraceRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.SetGraceAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/invoice/grace/clear")]
    public async Task<ActionResult<AuthorityResult>> AuthorityClearGrace(
        [FromBody] AuthGraceRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.ClearGraceAsync(session, request?.SchoolID ?? 0, cancellationToken));
    }

    [HttpGet("authority/invoice/paid")]
    public async Task<ActionResult<AuthPaidPageDto>> AuthorityPaidPage([FromQuery] int schoolId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetPaidPageAsync(session, schoolId, cancellationToken));
    }

    [HttpPost("authority/invoice/pay")]
    public async Task<ActionResult<AuthorityResult>> AuthorityPay(
        [FromBody] AuthPayInvoiceRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.PayInvoicesAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/invoice/print")]
    public async Task<ActionResult<AuthPrintPageDto>> AuthorityPrintPage([FromQuery] int schoolId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetPrintPageAsync(session, schoolId, cancellationToken));
    }

    [HttpPost("authority/invoice/print/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeletePrintInvoice(
        [FromBody] AuthIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.DeleteUnpaidInvoiceAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpGet("authority/invoice/print/pay")]
    public async Task<ActionResult<AuthPayPrintDto>> AuthorityPayPrint(
        [FromQuery] int schoolId, [FromQuery] string? ids, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetPayPrintAsync(session, schoolId, ids, cancellationToken));
    }

    [HttpGet("authority/invoice/print/receipt")]
    public async Task<ActionResult<AuthReceiptPrintDto>> AuthorityReceiptPrint(
        [FromQuery] int receiptId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetReceiptPrintAsync(session, receiptId, cancellationToken));
    }

    [HttpGet("authority/online-pay")]
    public async Task<ActionResult<AuthOnlinePayPageDto>> AuthorityOnlinePay(
        [FromQuery] string? type, [FromQuery] int schoolId, [FromQuery] string? method,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityInvoice.GetOnlinePayAsync(session, type, schoolId, method, from, to, cancellationToken));
    }

    [HttpGet("authority/links")]
    public async Task<ActionResult<AuthLinkTreeDto>> AuthorityLinks(
        [FromQuery] int categoryId, [FromQuery] int subId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.GetLinksAsync(session, categoryId, subId, cancellationToken));
    }

    [HttpPost("authority/links/category")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveCategory(
        [FromBody] AuthLinkNameSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.SaveCategoryAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/links/category/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteCategory(
        [FromBody] AuthIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.DeleteCategoryAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpPost("authority/links/sub")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveSub(
        [FromBody] AuthLinkNameSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.SaveSubAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/links/sub/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteSub(
        [FromBody] AuthIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.DeleteSubAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpPost("authority/links/page")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySavePage(
        [FromBody] AuthLinkPageSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.SavePageAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/links/page/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeletePage(
        [FromBody] AuthIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.DeletePageAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpGet("authority/roles")]
    public async Task<ActionResult<AuthRoleListDto>> AuthorityRoles(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.GetRolesAsync(session, cancellationToken));
    }

    [HttpPost("authority/roles")]
    public async Task<ActionResult<AuthorityResult>> AuthorityCreateRole(
        [FromBody] AuthRoleSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.CreateRoleAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/roles/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteRole(
        [FromBody] AuthRoleSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.DeleteRoleAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/reference")]
    public async Task<ActionResult<AuthReferralPageDto>> AuthorityReference(
        [FromQuery] int id, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.GetReferralAsync(session, id, cancellationToken));
    }

    [HttpPost("authority/reference")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveReferrer(
        [FromBody] AuthReferrerSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.SaveReferrerAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/reference/schools")]
    public async Task<ActionResult<AuthSchoolSearchPageDto>> AuthoritySearchSchools(
        [FromQuery] string? q, [FromQuery] int refId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.SearchSchoolsAsync(session, q, refId, cancellationToken));
    }

    [HttpPost("authority/reference/assign")]
    public async Task<ActionResult<AuthorityResult>> AuthorityAssignSchool(
        [FromBody] AuthAssignSchoolRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.AssignSchoolAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/reference/assign/update")]
    public async Task<ActionResult<AuthorityResult>> AuthorityUpdateAssign(
        [FromBody] AuthAssignUpdateRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.UpdateAssignAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/reference/assign/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteAssign(
        [FromBody] AuthIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.DeleteAssignAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpGet("authority/commission")]
    public async Task<ActionResult<AuthCommissionPageDto>> AuthorityCommission(
        [FromQuery] int refId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? status, [FromQuery] int detailId, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.GetCommissionAsync(
            session, refId, from ?? default, to ?? default, status, detailId, cancellationToken));
    }

    [HttpPost("authority/commission/pay")]
    public async Task<ActionResult<AuthorityResult>> AuthorityPayCommission(
        [FromBody] AuthCommissionPayRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.PayCommissionAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/sub-authority")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySignupSub(
        [FromBody] AuthSubSignupRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.CreateSubAuthorityAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/page-access")]
    public async Task<ActionResult<AuthAccessPageDto>> AuthorityPageAccess(
        [FromQuery] string? userName, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.GetPageAccessAsync(session, userName, cancellationToken));
    }

    [HttpPost("authority/page-access")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySavePageAccess(
        [FromBody] AuthAccessSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityAdmin.SavePageAccessAsync(session, request, cancellationToken));
    }

    [HttpGet("authority/profile")]
    public async Task<ActionResult<AuthProfileDto>> AuthorityProfile(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.GetProfileAsync(session, cancellationToken));
    }

    [HttpPost("authority/profile")]
    public async Task<ActionResult<ProfileResult>> AuthoritySaveProfile(
        [FromBody] AuthProfileDto? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authorityBasic.SaveProfileAsync(session, request, cancellationToken));
    }

    [HttpGet("admin-notices")]
    public async Task<ActionResult<List<AuthNoticeDto>>> AdminNotices(CancellationToken cancellationToken) =>
        Ok(await _authority.ListActiveNoticesAsync(cancellationToken));

    [HttpGet("authority/notices")]
    public async Task<ActionResult<List<AuthNoticeDto>>> AuthorityNotices(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.ListNoticesAsync(session, cancellationToken));
    }

    [HttpPost("authority/notices")]
    public async Task<ActionResult<AuthorityResult>> AuthoritySaveNotice(
        [FromBody] AuthNoticeSaveRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.SaveNoticeAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/notices/delete")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteNotice(
        [FromBody] AuthNoticeIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.DeleteNoticeAsync(session, request?.Id ?? 0, cancellationToken));
    }

    [HttpGet("authority/messages/unread")]
    public async Task<ActionResult<AuthUnreadDto>> AuthorityUnread(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.GetUnreadAsync(session, cancellationToken));
    }

    [HttpGet("authority/messages")]
    public async Task<ActionResult<AuthMessagePageDto>> AuthorityMessages(CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.GetMessagesAsync(session, cancellationToken));
    }

    [HttpPost("authority/messages/read")]
    public async Task<ActionResult<AuthorityResult>> AuthorityReadMessage(
        [FromBody] AuthMessageReadRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.MarkMessageReadAsync(session, request, cancellationToken));
    }

    [HttpPost("authority/messages/delete-contact")]
    public async Task<ActionResult<AuthorityResult>> AuthorityDeleteContact(
        [FromBody] AuthNoticeIdRequest? request, CancellationToken cancellationToken)
    {
        if (AuthorityOrForbid(out var session) is { } deny) return deny;
        return Ok(await _authority.DeleteContactAsync(session, request?.Id ?? 0, cancellationToken));
    }
}
