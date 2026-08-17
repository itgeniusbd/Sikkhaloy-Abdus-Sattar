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
    private readonly PaymentSmsService _sms;
    private readonly ExamService _exams;
    private readonly DashboardService _dashboard;

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
        PaymentSmsService sms,
        ExamService exams,
        DashboardService dashboard)
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
        _sms = sms;
        _exams = exams;
        _dashboard = dashboard;
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
        [FromQuery] string? id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _studentInfo.GetReportAsync(session, id, cancellationToken));
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
        return Ok(await _accounts.GetReceiptAsync(session, no, cancellationToken));
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
    public async Task<ActionResult<ExpenseListDto>> AccExpense(int categoryId, int subCategoryId, DateTime? from, DateTime? to, string? receiptNo, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var (items, total) = await _accounts.ListExpenseAsync(session, categoryId, subCategoryId, from, to, receiptNo, cancellationToken);
        return Ok(new ExpenseListDto { Items = items.ToList(), Total = total });
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
    public async Task<ActionResult<SessionPaidDueDto>> AccSessionPaidDue(string? status, string? classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _reports.GetSessionPaidDueAsync(session, status, classId, sectionId, roleId, payFor, from, to, cancellationToken));
    }

    [HttpGet("dashboard/overview")]
    public async Task<ActionResult<DashboardOverviewDto>> DashboardOverview(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _dashboard.GetOverviewAsync(session, cancellationToken));
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
}
