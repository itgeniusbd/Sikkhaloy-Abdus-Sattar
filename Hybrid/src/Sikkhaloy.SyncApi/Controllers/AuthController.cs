using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JwtTokenService _tokens;

    public AuthController(AuthService auth, JwtTokenService tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auth.LoginAsync(request, cancellationToken);
            if (!result.Succeeded || result.Session is null)
                return Ok(result);

            var (token, expires) = _tokens.Create(result.Session);
            result.AccessToken = token;
            result.ExpiresAt = expires;
            return Ok(result);
        }
        catch (Exception)
        {
            return Ok(new LoginResponse { Succeeded = false, Error = "login.failed" });
        }
    }

    [HttpPost("enter-school")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<LoginResponse>> EnterSchool([FromBody] Sikkhaloy.Shared.Authority.EnterSchoolRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var result = await _auth.EnterSchoolAsync(session, request.SchoolID, request.EducationYearID, cancellationToken);
        if (!result.Succeeded || result.Session is null)
            return Ok(result);

        var (token, expires) = _tokens.Create(result.Session);
        result.AccessToken = token;
        result.ExpiresAt = expires;
        return Ok(result);
    }

    [HttpPost("switch-year")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<LoginResponse>> SwitchYear([FromBody] SwitchYearRequest request, CancellationToken cancellationToken)
    {
        var session = JwtTokenService.FromPrincipal(User);
        var result = await _auth.SwitchYearAsync(session, request.EducationYearID, cancellationToken);
        if (!result.Succeeded || result.Session is null)
            return Ok(result);

        var (token, expires) = _tokens.Create(result.Session);
        result.AccessToken = token;
        result.ExpiresAt = expires;
        return Ok(result);
    }
}
