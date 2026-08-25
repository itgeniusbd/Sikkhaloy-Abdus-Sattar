using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityAdminService
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "Authority", "Sub-Authority", "Sub-Admin", "Teacher", "Student"
    };

    private readonly EduConnectionFactory _connections;

    public AuthorityAdminService(EduConnectionFactory connections) => _connections = connections;

    private static void Guard(SessionSnapshot session)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");
    }

    private static AuthorityResult Fail(string error) => new() { Succeeded = false, Error = error };
    private static AuthorityResult Ok(string? message = null, int id = 0) =>
        new() { Succeeded = true, Message = message, Id = id };

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static decimal M(object? value) =>
        value is null or DBNull ? 0m : Convert.ToDecimal(value);
    private static bool B(object? value) =>
        value is bool b ? b : value is null or DBNull ? false : Convert.ToBoolean(value);
    private static DateTime? Dt(object? value) =>
        value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);
    private static object Db(object? value) => value is null ? DBNull.Value : value;
    private static object DbDate(DateTime? value) =>
        value.HasValue ? value.Value.Date : DBNull.Value;

    public async Task<AuthRoleListDto> GetRolesAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthRoleListDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT r.RoleName
FROM dbo.aspnet_Roles AS r
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = r.ApplicationId AND a.LoweredApplicationName = N'/'
ORDER BY r.RoleName
""", con);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            dto.Roles.Add(S(reader["RoleName"]));
        return dto;
    }

    public async Task<AuthorityResult> CreateRoleAsync(SessionSnapshot session, AuthRoleSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        var name = (request?.Name ?? "").Trim();
        if (name.Length < 2)
            return Fail("al.roleName");
        if (name.Length > 256)
            return Fail("al.roleName");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var exists = new SqlCommand("""
SELECT 1
FROM dbo.aspnet_Roles AS r
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = r.ApplicationId AND a.LoweredApplicationName = N'/'
WHERE r.LoweredRoleName = LOWER(@Name)
""", con);
        exists.Parameters.AddWithValue("@Name", name);
        if (await exists.ExecuteScalarAsync(ct) is not null and not DBNull)
            return Fail("al.roleExists");

        await using var cmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Roles (ApplicationId, RoleId, RoleName, LoweredRoleName, Description)
SELECT a.ApplicationId, @RoleId, @Name, LOWER(@Name), NULL
FROM dbo.aspnet_Applications AS a
WHERE a.LoweredApplicationName = N'/'
""", con);
        cmd.Parameters.AddWithValue("@RoleId", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@Name", name);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0 ? Ok("al.roleCreated") : Fail("al.failed");
    }

    public async Task<AuthorityResult> DeleteRoleAsync(SessionSnapshot session, AuthRoleSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("al.roleName");
        if (ProtectedRoles.Contains(name))
            return Fail("al.roleProtected");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var count = new SqlCommand("""
SELECT COUNT(*)
FROM dbo.aspnet_UsersInRoles AS ur
INNER JOIN dbo.aspnet_Roles AS r ON r.RoleId = ur.RoleId
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = r.ApplicationId AND a.LoweredApplicationName = N'/'
WHERE r.LoweredRoleName = LOWER(@Name)
""", con);
        count.Parameters.AddWithValue("@Name", name);
        var users = I(await count.ExecuteScalarAsync(ct));
        if (users > 0)
            return Fail("al.roleInUse");

        await using var cmd = new SqlCommand("""
DELETE r
FROM dbo.aspnet_Roles AS r
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = r.ApplicationId AND a.LoweredApplicationName = N'/'
WHERE r.LoweredRoleName = LOWER(@Name)
""", con);
        cmd.Parameters.AddWithValue("@Name", name);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0 ? Ok("al.roleDeleted") : Fail("al.failed");
    }

    public async Task<AuthorityResult> CreateSubAuthorityAsync(
        SessionSnapshot session, AuthSubSignupRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthSubSignupRequest();
        var name = (request.Name ?? "").Trim();
        var designation = (request.Designation ?? "").Trim();
        var userName = (request.UserName ?? "").Trim();
        var password = request.Password ?? "";
        var confirm = request.ConfirmPassword ?? "";
        var email = (request.Email ?? "").Trim();
        var question = (request.Question ?? "").Trim();
        var answer = (request.Answer ?? "").Trim();

        if (name.Length == 0 || designation.Length == 0 || userName.Length == 0 || password.Length == 0
            || email.Length == 0 || question.Length == 0 || answer.Length == 0)
            return Fail("ab.required");
        if (userName.Any(char.IsWhiteSpace))
            return Fail("ab.userSpace");
        if (userName.Length is < 8 or > 30)
            return Fail("ab.userLen");
        if (password.Length is < 8 or > 30)
            return Fail("ab.passLen");
        if (!string.Equals(password, confirm, StringComparison.Ordinal))
            return Fail("ab.passMatch");
        if (!EmailPattern.IsMatch(email))
            return Fail("ab.emailInvalid");
        if (string.Equals(question, "Select your security question", StringComparison.OrdinalIgnoreCase))
            return Fail("ab.required");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            if (await AuthorityBasicService.UserExistsAsync(con, tx, userName, ct))
            {
                await tx.RollbackAsync(ct);
                return Fail("ab.userExists");
            }

            var created = await AuthorityBasicService.CreateMembershipUserAsync(
                con, tx, userName, password, email, question, answer, ct);
            if (!created.Succeeded)
            {
                await tx.RollbackAsync(ct);
                return created;
            }

            await AuthorityBasicService.AddToRoleAsync(con, tx, userName, "Sub-Authority", ct);

            int registrationId;
            await using (var cmd = new SqlCommand("""
INSERT INTO dbo.Registration (SchoolID, UserName, Validation, Category)
VALUES (0, @UserName, N'Valid', N'Sub-Authority');
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx))
            {
                cmd.Parameters.AddWithValue("@UserName", userName);
                registrationId = I(await cmd.ExecuteScalarAsync(ct));
            }

            await using (var cmd = new SqlCommand("""
INSERT INTO dbo.AST (RegistrationID, SchoolID, UserName, Category, Password, PasswordAnswer)
VALUES (@RegistrationID, 0, @UserName, N'Sub-Authority', @Password, @Answer)
""", con, tx))
            {
                cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Answer", answer);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await using (var cmd = new SqlCommand("""
INSERT INTO dbo.Authority_Info (RegistrationID, Name, Designation, Gender)
VALUES (@RegistrationID, @Name, @Designation, N'Male')
""", con, tx))
            {
                cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Designation", designation);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return Ok("al.subCreated", registrationId);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }
}
