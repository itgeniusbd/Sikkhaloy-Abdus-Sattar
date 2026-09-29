using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Exam;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Teachers;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Authorize]
[Route("api/sync/teacher-portal")]
public sealed class TeacherPortalController : ControllerBase
{
    private readonly TeacherPortalService _portal;
    private readonly StudentFeedbackService _feedback;

    public TeacherPortalController(TeacherPortalService portal, StudentFeedbackService feedback)
    {
        _portal = portal;
        _feedback = feedback;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<TeacherPortalDashboardDto>> Dashboard(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetDashboardAsync(session, cancellationToken));
    }

    [HttpGet("details")]
    public async Task<ActionResult<TeacherPortalDetailsDto>> Details(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetDetailsAsync(session, cancellationToken));
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<EducationYearDto>>> Sessions(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetSessionsAsync(session, cancellationToken));
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<StudentPortalAttendanceDto>> Attendance(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetAttendanceAsync(session, cancellationToken));
    }

    [HttpGet("routine")]
    public async Task<ActionResult<List<TeacherPortalPeriodDto>>> Routine(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetRoutineAsync(session, cancellationToken));
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<List<TeacherPortalSubjectDto>>> Subjects(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetSubjectsAsync(session, cancellationToken));
    }

    [HttpGet("exams")]
    public async Task<ActionResult<List<ExamNameDto>>> Exams([FromQuery] int classId, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetExamsAsync(session, classId, cancellationToken));
    }

    [HttpGet("sub-exams")]
    public async Task<ActionResult<IReadOnlyList<SubExamDto>>> SubExams(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetSubExamsAsync(session, cancellationToken));
    }

    [HttpGet("marks-filters")]
    public async Task<ActionResult<ExamFilterDto>> MarksFilters(
        [FromQuery] int examId, [FromQuery] int classId, [FromQuery] int subjectId,
        [FromQuery] string? groupId, [FromQuery] string? sectionId, [FromQuery] string? shiftId,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetMarksFiltersAsync(session, examId, classId, subjectId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpGet("marks")]
    public async Task<ActionResult<InputSheetDto>> Marks(
        [FromQuery] int examId, [FromQuery] int classId, [FromQuery] int subjectId, [FromQuery] int subExamId,
        [FromQuery] string? groupId, [FromQuery] string? sectionId, [FromQuery] string? shiftId,
        CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetInputSheetAsync(session, examId, classId, subjectId, subExamId, groupId, sectionId, shiftId, cancellationToken));
    }

    [HttpPost("marks")]
    public async Task<ActionResult<ExamResult>> SaveMarks(
        [FromBody] SaveInputMarksRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SaveInputMarksAsync(session, request, cancellationToken));
    }

    [HttpGet("students")]
    public async Task<ActionResult<List<TeacherPortalStudentHitDto>>> Students(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SearchStudentsAsync(session, q, cancellationToken));
    }

    [HttpGet("students/photo")]
    public async Task<ActionResult<StudentPhotoDto>> StudentPhoto(
        [FromQuery] string? student, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetStudentPhotoAsync(session, student, cancellationToken));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<List<StudentPortalFaultReportDto>>> Reports(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? student, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetReportsAsync(session, from, to, student, cancellationToken));
    }

    [HttpPost("reports")]
    public async Task<ActionResult<StudentInfoResult>> SaveReport(
        [FromBody] SaveStudentFaultReportRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SaveReportAsync(session, request, cancellationToken));
    }

    [HttpPost("reports/bulk")]
    public async Task<ActionResult<StudentInfoResult>> SaveReportsBulk(
        [FromBody] SaveStudentFaultReportsBulkRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SaveReportsBulkAsync(session, request, cancellationToken));
    }

    [HttpPut("reports")]
    public async Task<ActionResult<StudentInfoResult>> UpdateReport(
        [FromBody] UpdateStudentFaultReportRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.UpdateReportAsync(session, request, cancellationToken));
    }

    [HttpDelete("reports/{id:int}")]
    public async Task<ActionResult<StudentInfoResult>> DeleteReport(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.DeleteReportAsync(session, id, cancellationToken));
    }

    [HttpGet("notices")]
    public async Task<ActionResult<IReadOnlyList<NoticeDto>>> Notices(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetNoticesAsync(session, cancellationToken));
    }

    [HttpPost("notices")]
    public async Task<ActionResult<StudentInfoResult>> SaveNotice(
        [FromBody] SaveNoticeRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SaveNoticeAsync(session, request, cancellationToken));
    }

    [HttpGet("classes")]
    public async Task<ActionResult<List<TeacherPortalOptionDto>>> Classes(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetClassesAsync(session, cancellationToken));
    }

    [HttpGet("student-attendance/meta")]
    public async Task<ActionResult<TeacherPortalAttendanceMetaDto>> StudentAttendanceMeta(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetStudentAttendanceMetaAsync(session, cancellationToken));
    }

    [HttpGet("student-attendance")]
    public async Task<ActionResult<IReadOnlyList<StudentManualRowDto>>> StudentAttendance(
        [FromQuery] int scheduleId, [FromQuery] int classId, [FromQuery] DateTime date, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetStudentAttendanceRowsAsync(session, scheduleId, classId, date, cancellationToken));
    }

    [HttpPost("student-attendance")]
    public async Task<ActionResult<AttendanceResult>> SaveStudentAttendance(
        [FromBody] SaveStudentManualRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SaveStudentAttendanceAsync(session, request, cancellationToken));
    }

    [HttpGet("cv")]
    public async Task<ActionResult<TeacherPortalCvDto>> Cv(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetCvAsync(session, cancellationToken));
    }

    [HttpPost("cv/objective")]
    public async Task<ActionResult<StudentInfoResult>> AddObjective(
        [FromBody] TeacherPortalCvTextRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.AddCvObjectiveAsync(session, request, cancellationToken));
    }

    [HttpPost("cv/education")]
    public async Task<ActionResult<StudentInfoResult>> AddEducation(
        [FromBody] TeacherPortalCvEducationRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.AddCvEducationAsync(session, request, cancellationToken));
    }

    [HttpPost("cv/job")]
    public async Task<ActionResult<StudentInfoResult>> AddJob(
        [FromBody] TeacherPortalCvJobRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.AddCvJobAsync(session, request, cancellationToken));
    }

    [HttpPost("cv/skill")]
    public async Task<ActionResult<StudentInfoResult>> AddSkill(
        [FromBody] TeacherPortalCvSkillRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.AddCvSkillAsync(session, request, cancellationToken));
    }

    [HttpPost("cv/language")]
    public async Task<ActionResult<StudentInfoResult>> AddLanguage(
        [FromBody] TeacherPortalCvLanguageRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.AddCvLanguageAsync(session, request, cancellationToken));
    }

    [HttpDelete("cv/{kind}/{id:int}")]
    public async Task<ActionResult<StudentInfoResult>> DeleteCv(string kind, int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.DeleteCvItemAsync(session, kind, id, cancellationToken));
    }

    [HttpGet("feedback")]
    public async Task<ActionResult<List<StudentFeedbackItemDto>>> Feedback(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ListOfficeAsync(session, cancellationToken));
    }

    [HttpGet("feedback/unread")]
    public async Task<ActionResult<StudentFeedbackUnreadDto>> FeedbackUnread(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.UnreadAsync(session, cancellationToken));
    }

    [HttpPut("feedback/{id:int}/status")]
    public async Task<ActionResult<StudentFeedbackResult>> FeedbackStatus(
        int id, [FromBody] StudentFeedbackStatusRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.SetStatusAsync(session, id, body?.Status, cancellationToken));
    }

    [HttpPut("feedback/{id:int}/read")]
    public async Task<ActionResult<StudentFeedbackResult>> FeedbackMarkRead(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.MarkReadAsync(session, id, cancellationToken));
    }

    [HttpPost("feedback/{id:int}/reply")]
    public async Task<ActionResult<StudentFeedbackResult>> FeedbackReply(
        int id, [FromBody] StudentFeedbackReplyRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ReplyAsync(session, id, body?.Body, cancellationToken));
    }

    [HttpPut("feedback/{id:int}/replies/read")]
    public async Task<ActionResult<StudentFeedbackResult>> FeedbackRepliesRead(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.MarkRepliesReadAsync(session, id, cancellationToken));
    }
}
