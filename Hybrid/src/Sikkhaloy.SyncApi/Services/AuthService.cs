using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class AuthService
{
    private static readonly HashSet<string> OfficeRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Sub-Admin"
    };

    private static readonly HashSet<string> AuthorityRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authority",
        "Sub-Authority"
    };

    private readonly EduConnectionFactory _connections;

    public AuthService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Fail("login.required");
        }

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        var membership = await ReadMembershipAsync(con, userName, cancellationToken);
        if (membership is null)
            return Fail("login.userNotFound");
        if (!membership.Value.IsApproved)
            return Fail("login.notApproved");
        if (membership.Value.IsLockedOut)
            return Fail("login.locked");
        if (!MembershipPasswordVerifier.Verify(request.Password, membership.Value.Password, membership.Value.Salt, membership.Value.Format))
            return Fail("login.badPassword");

        var role = await ReadRoleAsync(con, userName, cancellationToken);
        if (role is null || (!OfficeRoles.Contains(role) && !AuthorityRoles.Contains(role)))
            return Fail("login.role");

        var deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
            ? Guid.NewGuid().ToString("N")
            : request.DeviceId;

        if (AuthorityRoles.Contains(role))
        {
            var authority = await ReadAuthorityProfileAsync(con, userName, cancellationToken);
            if (authority is null)
                return Fail("login.noAuthority");

            return new LoginResponse
            {
                Succeeded = true,
                Session = new SessionSnapshot
                {
                    UserName = userName,
                    Role = role,
                    SchoolID = 0,
                    SchoolName = "Sikkhaloy.com",
                    RegistrationID = authority.Value.RegistrationID,
                    EducationYearID = 0,
                    DeviceId = deviceId,
                    DisplayName = string.IsNullOrWhiteSpace(authority.Value.DisplayName)
                        ? userName
                        : authority.Value.DisplayName
                }
            };
        }

        var profile = await ReadProfileAsync(con, userName, cancellationToken);
        if (profile is null)
            return Fail("login.noYear");

        return new LoginResponse
        {
            Succeeded = true,
            Session = new SessionSnapshot
            {
                UserName = userName,
                Role = role,
                SchoolID = profile.Value.SchoolID,
                SchoolName = profile.Value.SchoolName,
                RegistrationID = profile.Value.RegistrationID,
                EducationYearID = profile.Value.EducationYearID,
                DeviceId = deviceId,
                DisplayName = string.IsNullOrWhiteSpace(profile.Value.DisplayName)
                    ? userName
                    : profile.Value.DisplayName
            }
        };
    }

    private static LoginResponse Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<(string Password, string Salt, int Format, bool IsApproved, bool IsLockedOut)?> ReadMembershipAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT m.Password, m.PasswordSalt, m.PasswordFormat, m.IsApproved, m.IsLockedOut
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Membership AS m ON u.UserId = m.UserId
INNER JOIN dbo.aspnet_Applications AS a ON u.ApplicationId = a.ApplicationId
WHERE u.LoweredUserName = LOWER(@UserName)
  AND a.LoweredApplicationName = N'/'";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return (
            reader.GetString(0),
            reader.GetString(1),
            reader.GetInt32(2),
            reader.GetBoolean(3),
            reader.GetBoolean(4));
    }

    private static async Task<string?> ReadRoleAsync(SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT r.RoleName
FROM dbo.aspnet_UsersInRoles AS ur
INNER JOIN dbo.aspnet_Roles AS r ON ur.RoleId = r.RoleId
INNER JOIN dbo.aspnet_Users AS u ON ur.UserId = u.UserId
WHERE u.LoweredUserName = LOWER(@UserName)";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        string? best = null;
        var bestRank = 99;
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            var rank = RoleRank(name);
            if (rank < bestRank)
            {
                best = name;
                bestRank = rank;
            }
        }

        return best;
    }

    private static int RoleRank(string name)
    {
        if (string.Equals(name, "Authority", StringComparison.OrdinalIgnoreCase)) return 0;
        if (string.Equals(name, "Sub-Authority", StringComparison.OrdinalIgnoreCase)) return 1;
        if (string.Equals(name, "Admin", StringComparison.OrdinalIgnoreCase)) return 2;
        if (string.Equals(name, "Sub-Admin", StringComparison.OrdinalIgnoreCase)) return 3;
        return 99;
    }

    private static async Task<(int SchoolID, string SchoolName, int RegistrationID, int EducationYearID, string DisplayName)?> ReadProfileAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT Registration.SchoolID, SchoolInfo.SchoolName, Registration.RegistrationID, Education_Year_User.EducationYearID,
       LTRIM(RTRIM(ISNULL(Admin.FirstName, N'') + N' ' + ISNULL(Admin.LastName, N''))) AS DisplayName
FROM dbo.Registration
INNER JOIN dbo.SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID
INNER JOIN dbo.Education_Year_User ON Registration.RegistrationID = Education_Year_User.RegistrationID
LEFT JOIN dbo.Admin ON Admin.RegistrationID = Registration.RegistrationID AND Admin.SchoolID = Registration.SchoolID
WHERE Registration.UserName = @UserName
  AND Registration.Validation = N'Valid'";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return (
            Convert.ToInt32(reader["SchoolID"]),
            reader["SchoolName"]?.ToString() ?? "",
            Convert.ToInt32(reader["RegistrationID"]),
            Convert.ToInt32(reader["EducationYearID"]),
            (reader["DisplayName"]?.ToString() ?? "").Trim());
    }

    private static async Task<(int RegistrationID, string DisplayName)?> ReadAuthorityProfileAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1 Registration.RegistrationID,
       LTRIM(RTRIM(ISNULL(Authority_Info.Name, N''))) AS DisplayName
FROM dbo.Authority_Info
INNER JOIN dbo.Registration ON Authority_Info.RegistrationID = Registration.RegistrationID
WHERE Registration.UserName = @UserName
  AND Registration.Validation = N'Valid'";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return (
            Convert.ToInt32(reader["RegistrationID"]),
            (reader["DisplayName"]?.ToString() ?? "").Trim());
    }

    public async Task<LoginResponse> EnterSchoolAsync(SessionSnapshot authority, int schoolId, int educationYearId, CancellationToken cancellationToken)
    {
        if (!authority.IsAuthority)
            return Fail("auth.forbidden");
        if (schoolId <= 0)
            return Fail("auth.noSchool");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        string? userName = null;
        await using (var find = new SqlCommand("""
SELECT TOP 1 r.UserName
FROM dbo.SchoolInfo s
INNER JOIN dbo.Registration r ON r.SchoolID = s.SchoolID AND r.UserName = s.UserName
WHERE s.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
  AND r.Category IN (N'Admin', N'Sub-Admin')
ORDER BY CASE WHEN r.Category = N'Admin' THEN 0 ELSE 1 END, r.RegistrationID
""", con))
        {
            find.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await find.ExecuteScalarAsync(cancellationToken);
            userName = value is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            await using var fallback = new SqlCommand("""
SELECT TOP 1 r.UserName
FROM dbo.Registration r
WHERE r.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
  AND r.Category = N'Admin'
ORDER BY r.RegistrationID
""", con);
            fallback.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await fallback.ExecuteScalarAsync(cancellationToken);
            userName = value is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
        }

        if (string.IsNullOrWhiteSpace(userName))
            return Fail("auth.noAdmin");

        var role = await ReadRoleAsync(con, userName, cancellationToken);
        if (role is null || (!OfficeRoles.Contains(role) && !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(role, "Sub-Admin", StringComparison.OrdinalIgnoreCase)))
        {
            role = "Admin";
        }

        var profile = await ReadProfileAsync(con, userName, cancellationToken);
        if (profile is null || profile.Value.SchoolID != schoolId)
            return Fail("login.noYear");

        var yearId = profile.Value.EducationYearID;
        if (educationYearId > 0)
        {
            await using var year = new SqlCommand("""
SELECT EducationYearID FROM dbo.Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
            year.Parameters.AddWithValue("@SchoolID", schoolId);
            year.Parameters.AddWithValue("@EducationYearID", educationYearId);
            var found = await year.ExecuteScalarAsync(cancellationToken);
            if (found is null or DBNull)
                return Fail("login.noYear");
            yearId = educationYearId;
        }

        var deviceId = string.IsNullOrWhiteSpace(authority.DeviceId)
            ? Guid.NewGuid().ToString("N")
            : authority.DeviceId;

        return new LoginResponse
        {
            Succeeded = true,
            Session = new SessionSnapshot
            {
                UserName = userName,
                Role = OfficeRoles.Contains(role) ? role : "Admin",
                SchoolID = profile.Value.SchoolID,
                SchoolName = profile.Value.SchoolName,
                RegistrationID = profile.Value.RegistrationID,
                EducationYearID = yearId,
                DeviceId = deviceId,
                DisplayName = string.IsNullOrWhiteSpace(profile.Value.DisplayName)
                    ? userName
                    : profile.Value.DisplayName
            }
        };
    }

    public async Task<LoginResponse> SwitchYearAsync(SessionSnapshot session, int educationYearId, CancellationToken cancellationToken)
    {
        if (educationYearId <= 0)
            return Fail("login.noYear");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        const string existsSql = @"
SELECT EducationYear
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID";
        await using (var exists = new SqlCommand(existsSql, con))
        {
            exists.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            exists.Parameters.AddWithValue("@EducationYearID", educationYearId);
            var name = await exists.ExecuteScalarAsync(cancellationToken);
            if (name is null or DBNull)
                return Fail("login.noYear");
        }

        const string updateSql = @"
UPDATE dbo.Education_Year_User
SET EducationYearID = @EducationYearID
WHERE SchoolID = @SchoolID AND RegistrationID = @RegistrationID";
        await using (var update = new SqlCommand(updateSql, con))
        {
            update.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            update.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            update.Parameters.AddWithValue("@EducationYearID", educationYearId);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        var profile = await ReadProfileAsync(con, session.UserName, cancellationToken);
        if (profile is null)
            return Fail("login.noYear");

        return new LoginResponse
        {
            Succeeded = true,
            Session = new SessionSnapshot
            {
                UserName = session.UserName,
                Role = session.Role,
                SchoolID = profile.Value.SchoolID,
                SchoolName = profile.Value.SchoolName,
                RegistrationID = profile.Value.RegistrationID,
                EducationYearID = educationYearId,
                DeviceId = session.DeviceId,
                DisplayName = string.IsNullOrWhiteSpace(profile.Value.DisplayName)
                    ? session.UserName
                    : profile.Value.DisplayName
            }
        };
    }
}
