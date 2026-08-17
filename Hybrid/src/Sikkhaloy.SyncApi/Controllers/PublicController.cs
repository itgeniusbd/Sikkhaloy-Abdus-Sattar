using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Institution;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController : ControllerBase
{
    private readonly InstitutionService _institution;

    public PublicController(InstitutionService institution)
    {
        _institution = institution;
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
