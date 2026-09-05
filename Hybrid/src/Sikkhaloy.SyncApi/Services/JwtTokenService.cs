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
    public const string SchoolNameClaim = "school_name";
    public const string DisplayNameClaim = "display_name";
    public const string StudentIdClaim = "student_id";
    public const string StudentClassIdClaim = "student_class_id";
    public const string ClassIdClaim = "class_id";
    public const string StudentCodeClaim = "student_code";
    public const string ClassNameClaim = "class_name";
    public const string SectionNameClaim = "section_name";

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
            new Claim(DeviceIdClaim, session.DeviceId),
            new Claim(SchoolNameClaim, session.SchoolName ?? ""),
            new Claim(DisplayNameClaim, session.DisplayName ?? ""),
            new Claim(StudentIdClaim, session.StudentID.ToString()),
            new Claim(StudentClassIdClaim, session.StudentClassID.ToString()),
            new Claim(ClassIdClaim, session.ClassID.ToString()),
            new Claim(StudentCodeClaim, session.StudentCode ?? ""),
            new Claim(ClassNameClaim, session.ClassName ?? ""),
            new Claim(SectionNameClaim, session.SectionName ?? "")
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
            DeviceId = user.FindFirstValue(DeviceIdClaim) ?? "",
            SchoolName = user.FindFirstValue(SchoolNameClaim) ?? "",
            DisplayName = user.FindFirstValue(DisplayNameClaim) ?? "",
            StudentID = ParseInt(user.FindFirstValue(StudentIdClaim)),
            StudentClassID = ParseInt(user.FindFirstValue(StudentClassIdClaim)),
            ClassID = ParseInt(user.FindFirstValue(ClassIdClaim)),
            StudentCode = user.FindFirstValue(StudentCodeClaim) ?? "",
            ClassName = user.FindFirstValue(ClassNameClaim) ?? "",
            SectionName = user.FindFirstValue(SectionNameClaim) ?? ""
        };
    }

    private static int ParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : 0;
}
