using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class JwtTokenService
{
    public const string SchoolIdClaim = "school_id";
    public const string EducationYearIdClaim = "education_year_id";
    public const string RegistrationIdClaim = "registration_id";
    public const string RoleClaim = "office_role";
    public const string DeviceIdClaim = "device_id";

    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTimeOffset ExpiresAt) Create(SessionSnapshot session)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = _configuration["Jwt:Issuer"] ?? "sikkhaloy-hybrid";
        var hours = int.TryParse(_configuration["Jwt:LifetimeHours"], out var value) ? value : 12;
        var expires = DateTimeOffset.UtcNow.AddHours(hours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, session.UserName),
            new Claim(ClaimTypes.Name, session.UserName),
            new Claim(SchoolIdClaim, session.SchoolID.ToString()),
            new Claim(EducationYearIdClaim, session.EducationYearID.ToString()),
            new Claim(RegistrationIdClaim, session.RegistrationID.ToString()),
            new Claim(RoleClaim, session.Role),
            new Claim(DeviceIdClaim, session.DeviceId)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            issuer,
            claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public static SessionSnapshot FromPrincipal(ClaimsPrincipal user)
    {
        return new SessionSnapshot
        {
            UserName = user.Identity?.Name ?? "",
            Role = user.FindFirstValue(RoleClaim) ?? "",
            SchoolID = ParseInt(user.FindFirstValue(SchoolIdClaim)),
            RegistrationID = ParseInt(user.FindFirstValue(RegistrationIdClaim)),
            EducationYearID = ParseInt(user.FindFirstValue(EducationYearIdClaim)),
            DeviceId = user.FindFirstValue(DeviceIdClaim) ?? ""
        };
    }

    private static int ParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : 0;
}
