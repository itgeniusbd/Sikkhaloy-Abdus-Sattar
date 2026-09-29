using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Institution;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController : ControllerBase
{
    private readonly InstitutionService _institution;
    private readonly WebsitePublicService _website;
    private readonly IConfiguration _config;

    public PublicController(InstitutionService institution, WebsitePublicService website, IConfiguration config)
    {
        _institution = institution;
        _website = website;
        _config = config;
    }

    [HttpGet("institutes")]
    public async Task<ActionResult<IReadOnlyList<PublicInstituteDto>>> Institutes(CancellationToken cancellationToken)
        => Ok(await _institution.ListPublicAsync(cancellationToken));

    [HttpGet("stats")]
    public async Task<ActionResult<PublicStatsDto>> Stats(CancellationToken cancellationToken)
        => Ok(await _institution.GetPublicStatsAsync(cancellationToken));

    [HttpPost("contact")]
    public async Task<ActionResult<PublicContactResult>> Contact([FromBody] PublicContactRequest request, CancellationToken cancellationToken)
    {
        var result = await _institution.SendPublicContactAsync(request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("institutes/{schoolId:int}/logo")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Logo(int schoolId, CancellationToken cancellationToken)
    {
        var bytes = await _institution.GetPublicLogoAsync(schoolId, cancellationToken);
        if (bytes is null || bytes.Length == 0)
            return NotFound();

        return File(bytes, DetectMime(bytes));
    }

    [HttpGet("institutes/{schoolId:int}")]
    public async Task<ActionResult<WebsiteHomeDto>> Institute(int schoolId, CancellationToken cancellationToken)
    {
        var data = await _website.GetHomeAsync(schoolId, cancellationToken);
        if (data is null)
            return NotFound();
        return Ok(data);
    }

    [HttpGet("website/{schoolId:int}")]
    public async Task<ActionResult<WebsiteHomeDto>> Website(int schoolId, CancellationToken cancellationToken)
    {
        var data = await _website.GetHomeAsync(schoolId, cancellationToken);
        if (data is null)
            return NotFound();
        return Ok(data);
    }

    [HttpGet("website/{schoolId:int}/students/{studentId:int}/photo")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> StudentPhoto(int schoolId, int studentId, CancellationToken cancellationToken)
    {
        var bytes = await _website.GetStudentPhotoAsync(schoolId, studentId, cancellationToken);
        if (bytes is null || bytes.Length == 0)
            return NotFound();
        return File(bytes, DetectMime(bytes));
    }

    [HttpGet("website/{schoolId:int}/teachers/{employeeId:int}/photo")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> TeacherPhoto(int schoolId, int employeeId, CancellationToken cancellationToken)
    {
        var bytes = await _website.GetTeacherPhotoAsync(schoolId, employeeId, cancellationToken);
        if (bytes is null || bytes.Length == 0)
            return NotFound();
        return File(bytes, DetectMime(bytes));
    }

    [HttpGet("website/{schoolId:int}/staff/{employeeId:int}/photo")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> StaffPhoto(int schoolId, int employeeId, CancellationToken cancellationToken)
    {
        var bytes = await _website.GetStaffPhotoAsync(schoolId, employeeId, cancellationToken);
        if (bytes is null || bytes.Length == 0)
            return NotFound();
        return File(bytes, DetectMime(bytes));
    }

    [HttpGet("website/{schoolId:int}/admission")]
    public async Task<ActionResult<WebsiteAdmissionFormDto>> AdmissionForm(int schoolId, CancellationToken cancellationToken)
    {
        var data = await _website.GetAdmissionFormAsync(schoolId, cancellationToken);
        if (data is null)
            return NotFound();
        return Ok(data);
    }

    [HttpGet("website/{schoolId:int}/admission/options")]
    public async Task<ActionResult<WebsiteAdmissionOptionsDto>> AdmissionOptions(
        int schoolId,
        [FromQuery] int classId,
        [FromQuery] int educationYearId = 0,
        [FromQuery] int groupId = 0,
        [FromQuery] int sectionId = 0,
        [FromQuery] int shiftId = 0,
        CancellationToken cancellationToken = default)
    {
        var data = await _website.GetAdmissionOptionsAsync(
            schoolId, classId, educationYearId, groupId, sectionId, shiftId, cancellationToken);
        if (data is null)
            return NotFound();
        return Ok(data);
    }

    [HttpPost("website/{schoolId:int}/admit")]
    public async Task<ActionResult<WebsiteAdmitResult>> Admit(
        int schoolId, [FromBody] WebsiteAdmitRequest request, CancellationToken cancellationToken)
    {
        if (!await AdmitKeyOkAsync(schoolId, cancellationToken))
            return Unauthorized(new WebsiteAdmitResult { Error = "Website admit key is required." });

        var result = await _website.AdmitStudentAsync(schoolId, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("website/{schoolId:int}/admit-payments")]
    public async Task<ActionResult<WebsiteAdmitResult>> AdmitPayments(
        int schoolId, [FromBody] WebsiteAdmitPaymentsRequest request, CancellationToken cancellationToken)
    {
        if (!await AdmitKeyOkAsync(schoolId, cancellationToken))
            return Unauthorized(new WebsiteAdmitResult { Error = "Website admit key is required." });

        var result = await _website.AddAdmissionPaymentsAsync(schoolId, request, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    private async Task<bool> AdmitKeyOkAsync(int schoolId, CancellationToken cancellationToken)
    {
        var header = Request.Headers["X-Website-Key"].ToString();
        var schoolKey = await _institution.GetStoredWebsiteApiKeyAsync(schoolId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(schoolKey))
            return string.Equals(schoolKey, header, StringComparison.Ordinal);

        var globalKey = _config["Website:AdmitKey"];
        if (string.IsNullOrWhiteSpace(globalKey))
            return true;
        return string.Equals(globalKey, header, StringComparison.Ordinal);
    }

    private static string DetectMime(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50)
            return "image/png";
        if (bytes.Length >= 3 && bytes[0] == 0x47 && bytes[1] == 0x49)
            return "image/gif";
        if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D)
            return "image/bmp";
        return "image/jpeg";
    }
}
