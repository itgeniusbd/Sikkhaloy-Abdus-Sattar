using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Authorize]
[Route("api/sync/feedback")]
public sealed class FeedbackController : ControllerBase
{
    private readonly StudentFeedbackService _feedback;

    public FeedbackController(StudentFeedbackService feedback) => _feedback = feedback;

    [HttpGet]
    public async Task<ActionResult<List<StudentFeedbackItemDto>>> List(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var items = await _feedback.ListOfficeAsync(session, cancellationToken);
        Response.Headers["X-Feedback-School"] = session.SchoolID.ToString();
        Response.Headers["X-Feedback-Role"] = session.Role ?? "";
        Response.Headers["X-Feedback-Reg"] = session.RegistrationID.ToString();
        Response.Headers["X-Feedback-Count"] = items.Count.ToString();
        Response.Headers["X-Feedback-SeeAll"] =
            (string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "1" : "0");
        return Ok(items);
    }

    [HttpGet("recipients")]
    public async Task<ActionResult<List<StudentFeedbackRecipientDto>>> Recipients(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ListAssignRecipientsAsync(session, cancellationToken));
    }

    [HttpGet("unread")]
    public async Task<ActionResult<StudentFeedbackUnreadDto>> Unread(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.UnreadAsync(session, cancellationToken));
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<StudentFeedbackResult>> Status(
        int id, [FromBody] StudentFeedbackStatusRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.SetStatusAsync(session, id, body?.Status, cancellationToken));
    }

    [HttpPut("{id:int}/assign")]
    public async Task<ActionResult<StudentFeedbackResult>> Assign(
        int id, [FromBody] AssignStudentFeedbackRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.AssignAsync(session, id, body?.ToRegistrationID ?? 0, cancellationToken));
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<StudentFeedbackResult>> MarkRead(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.MarkReadAsync(session, id, cancellationToken));
    }

    [HttpPost("{id:int}/reply")]
    public async Task<ActionResult<StudentFeedbackResult>> Reply(
        int id, [FromBody] StudentFeedbackReplyRequest body, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.ReplyAsync(session, id, body?.Body, cancellationToken));
    }

    [HttpPut("{id:int}/replies/read")]
    public async Task<ActionResult<StudentFeedbackResult>> MarkRepliesRead(int id, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.MarkRepliesReadAsync(session, id, cancellationToken));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<StudentFeedbackResult>> MarkAll(CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        return Ok(await _feedback.MarkAllReadAsync(session, cancellationToken));
    }
}
