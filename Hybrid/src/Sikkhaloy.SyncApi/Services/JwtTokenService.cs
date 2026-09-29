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
    public const string TeacherIdClaim = "teacher_id";
    public const string EmployeeIdClaim = "employee_id";

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
            new Claim(SectionNameClaim, session.SectionName ?? ""),
            new Claim(TeacherIdClaim, session.TeacherID.ToString()),
            new Claim(EmployeeIdClaim, session.EmployeeID.ToString())
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
        var role = ClaimValue(user, RoleClaim, ClaimTypes.Role, "role",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        // Some hosts map custom claims; also accept Category-style Admin values.
        if (string.IsNullOrWhiteSpace(role))
            role = ClaimValue(user, "Category", "category");

        return new SessionSnapshot
        {
            UserName = FirstNonEmpty(
                user.Identity?.Name,
                ClaimValue(user, ClaimTypes.Name, JwtRegisteredClaimNames.UniqueName, "unique_name", "name",
                    JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier)),
            Role = role,
            SchoolID = FirstInt(user, SchoolIdClaim, "schoolid", "SchoolID"),
            RegistrationID = FirstInt(user, RegistrationIdClaim, "registrationid", "RegistrationID"),
            EducationYearID = FirstInt(user, EducationYearIdClaim, "educationyearid", "EducationYearID"),
            DeviceId = ClaimValue(user, DeviceIdClaim),
            SchoolName = ClaimValue(user, SchoolNameClaim),
            DisplayName = ClaimValue(user, DisplayNameClaim),
            StudentID = FirstInt(user, StudentIdClaim),
            StudentClassID = FirstInt(user, StudentClassIdClaim),
            ClassID = FirstInt(user, ClassIdClaim),
            StudentCode = ClaimValue(user, StudentCodeClaim),
            ClassName = ClaimValue(user, ClassNameClaim),
            SectionName = ClaimValue(user, SectionNameClaim),
            TeacherID = FirstInt(user, TeacherIdClaim),
            EmployeeID = FirstInt(user, EmployeeIdClaim)
        };
    }

    private static int ParseInt(string? value) => int.TryParse(value, out var parsed) ? parsed : 0;

    private static int FirstInt(ClaimsPrincipal user, params string[] names)
    {
        var text = ClaimValue(user, names);
        if (ParseInt(text) > 0)
            return ParseInt(text);

        foreach (var claim in user.Claims)
        {
            var type = claim.Type ?? "";
            foreach (var name in names)
            {
                if (type.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    var n = ParseInt(claim.Value);
                    if (n > 0)
                        return n;
                }
            }
        }

        return 0;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            var text = (value ?? "").Trim();
            if (text.Length > 0)
                return text;
        }

        return "";
    }

    private static string ClaimValue(ClaimsPrincipal user, params string[] names)
    {
        foreach (var name in names)
        {
            var value = user.FindFirstValue(name);
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        foreach (var claim in user.Claims)
        {
            var type = claim.Type ?? "";
            foreach (var name in names)
            {
                if (type.Equals(name, StringComparison.OrdinalIgnoreCase)
                    || type.EndsWith("/" + name, StringComparison.OrdinalIgnoreCase)
                    || type.EndsWith(":" + name, StringComparison.OrdinalIgnoreCase))
                {
                    var value = (claim.Value ?? "").Trim();
                    if (value.Length > 0)
                        return value;
                }
            }
        }

        return "";
    }
}
