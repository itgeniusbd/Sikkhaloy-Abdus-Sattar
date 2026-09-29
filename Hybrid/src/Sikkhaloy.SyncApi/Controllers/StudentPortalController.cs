using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Authorize]
[Route("api/sync/student-portal")]
public sealed class StudentPortalController : ControllerBase
{
    private readonly StudentPortalService _portal;
    private readonly StudentFeedbackService _feedback;

    public StudentPortalController(StudentPortalService portal, StudentFeedbackService feedback)
    {
        _portal = portal;
        _feedback = feedback;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<StudentPortalDashboardDto>> Dashboard(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetDashboardAsync(session, cancellationToken));
    }

    [HttpGet("details")]
    public async Task<ActionResult<StudentPortalDetailsDto>> Details(CancellationToken cancellationToken)
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

    [HttpGet("sms")]
    public async Task<ActionResult<List<StudentPortalSmsDto>>> Sms(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetSmsAsync(session, from, to, cancellationToken));
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<StudentPortalAccountsBundleDto>> Accounts(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetAccountsBundleAsync(session, cancellationToken));
    }

    [HttpGet("accounts/receipt/{id:int}")]
    public async Task<ActionResult<List<StudentPortalReceiptLineDto>>> Receipt(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetReceiptLinesAsync(session, id, cancellationToken));
    }

    [HttpPost("accounts/pay")]
    public async Task<ActionResult<StudentPortalPayStartResult>> StartPay(
        [FromBody] StudentPortalPayStartRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.StartOnlinePaymentAsync(session, request, cancellationToken));
    }

    [HttpGet("pay/callback")]
    [HttpPost("pay/callback")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PayCallback(CancellationToken cancellationToken)
    {
        var recordId = First("opt_b");
        var returnUrl = First("opt_a");
        var result = await _portal.CompleteOnlinePaymentAsync(recordId, returnUrl, cancellationToken);
        var dest = string.IsNullOrWhiteSpace(returnUrl) ? "http://localhost:5288/student/accounts" : returnUrl;
        dest += dest.Contains('?') ? "&" : "?";
        dest += result.Succeeded ? "paid=1" : "pay=fail";
        return Redirect(dest);
    }

    private string? First(string name)
    {
        if (Request.Query.TryGetValue(name, out var q) && !string.IsNullOrWhiteSpace(q))
            return q.ToString();
        if (Request.HasFormContentType && Request.Form.TryGetValue(name, out var f) && !string.IsNullOrWhiteSpace(f))
            return f.ToString();
        return null;
    }

    [HttpGet("notices")]
    public async Task<ActionResult<List<StudentPortalNoticeDto>>> Notices(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetNoticesAsync(session, cancellationToken));
    }

    [HttpGet("exams")]
    public async Task<ActionResult<List<StudentPortalExamDto>>> Exams(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetExamsAsync(session, cancellationToken));
    }

    [HttpGet("exams/subjects")]
    public async Task<ActionResult<List<StudentPortalExamSubjectDto>>> ExamSubjects(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetExamSubjectsAsync(session, cancellationToken));
    }

    [HttpGet("cumulative")]
    public async Task<ActionResult<List<StudentPortalExamDto>>> Cumulative(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetCumulativeAsync(session, cancellationToken));
    }

    [HttpGet("cumulative/subjects")]
    public async Task<ActionResult<List<StudentPortalExamSubjectDto>>> CumulativeSubjects(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetCumulativeSubjectsAsync(session, cancellationToken));
    }

    [HttpGet("routine")]
    public async Task<ActionResult<List<StudentPortalPeriodDto>>> Routine(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetRoutineAsync(session, cancellationToken));
    }

    [HttpGet("upcoming-exams")]
    public async Task<ActionResult<List<StudentPortalExamDto>>> Upcoming(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetUpcomingExamsAsync(session, cancellationToken));
    }

    [HttpGet("report")]
    public async Task<ActionResult<List<StudentPortalFaultReportDto>>> Report(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetFaultReportsAsync(session, from, to, cancellationToken));
    }

    [HttpGet("inbox-reads")]
    public async Task<ActionResult<StudentPortalInboxReadsDto>> InboxReads(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.GetInboxReadsAsync(session, cancellationToken));
    }

    [HttpPut("inbox-reads")]
    public async Task<ActionResult<StudentPortalInboxReadsDto>> SaveInboxReads(
        [FromBody] StudentPortalInboxReadsDto body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _portal.SaveInboxReadsAsync(session, body, cancellationToken));
    }

    [HttpGet("feedback/recipients")]
    public async Task<ActionResult<List<StudentFeedbackRecipientDto>>> FeedbackRecipients(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ListRecipientsAsync(session, cancellationToken));
    }

    [HttpGet("feedback")]
    public async Task<ActionResult<List<StudentFeedbackItemDto>>> Feedback(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ListMineAsync(session, cancellationToken));
    }

    [HttpPost("feedback")]
    public async Task<ActionResult<StudentFeedbackResult>> SubmitFeedback(
        [FromBody] SubmitStudentFeedbackRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.SubmitAsync(session, body, cancellationToken));
    }

    [HttpPost("feedback/{id:int}/reply")]
    public async Task<ActionResult<StudentFeedbackResult>> ReplyFeedback(
        int id, [FromBody] StudentFeedbackReplyRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ReplyAsync(session, id, body?.Body, cancellationToken));
    }

    [HttpPut("feedback/{id:int}/replies/read")]
    public async Task<ActionResult<StudentFeedbackResult>> MarkFeedbackRepliesRead(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.MarkRepliesReadAsync(session, id, cancellationToken));
    }
}
