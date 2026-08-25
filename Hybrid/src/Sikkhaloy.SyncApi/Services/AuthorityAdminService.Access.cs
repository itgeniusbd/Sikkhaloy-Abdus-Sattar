using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Access;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityAdminService
{
    public async Task<AuthAccessPageDto> GetPageAccessAsync(SessionSnapshot session, string? userName, CancellationToken ct)
    {
        Guard(session);
        userName = (userName ?? "").Trim();
        var dto = new AuthAccessPageDto { UserName = userName };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT Registration.RegistrationID, Registration.UserName, Authority_Info.Name
FROM dbo.Registration
INNER JOIN dbo.Authority_Info ON Registration.RegistrationID = Authority_Info.RegistrationID
WHERE Registration.Validation = N'Valid'
  AND Registration.UserName <> N'sikkhaloy_admin'
ORDER BY Authority_Info.Name
""", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Users.Add(new AuthAccessUserDto
                {
                    RegistrationID = I(reader["RegistrationID"]),
                    UserName = S(reader["UserName"]),
                    Name = S(reader["Name"]) + " (" + S(reader["UserName"]) + ")"
                });
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT p.LinkID,
       LTRIM(RTRIM(ISNULL(c.Category, N''))) AS Category,
       LTRIM(RTRIM(ISNULL(sc.SubCategory, N''))) AS SubCategory,
       LTRIM(RTRIM(ISNULL(p.PageTitle, N''))) AS PageTitle,
       r.RoleName,
       CAST(CASE WHEN u.LinkID IS NULL THEN 0 ELSE 1 END AS BIT) AS Allowed
FROM dbo.Authority_Link_Pages AS p
LEFT JOIN dbo.Authority_Link_Category AS c ON c.LinkCategoryID = p.LinkCategoryID
LEFT JOIN dbo.Authority_Link_SubCategory AS sc ON sc.SubCategoryID = p.SubCategoryID
LEFT JOIN dbo.aspnet_Roles AS r ON r.RoleId = p.RoleId
LEFT JOIN dbo.Authority_Link_Users AS u ON u.LinkID = p.LinkID AND u.UserName = @UserName
ORDER BY ISNULL(c.Ascending, 0), ISNULL(sc.Ascending, 0), ISNULL(p.Ascending, 0), p.LinkID
""", con))
        {
            cmd.Parameters.AddWithValue("@UserName", userName);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Pages.Add(new PageAccessRowDto
                {
                    LinkID = I(reader["LinkID"]),
                    Category = S(reader["Category"]),
                    SubCategory = S(reader["SubCategory"]),
                    PageTitle = S(reader["PageTitle"]),
                    RoleName = reader["RoleName"] as string,
                    Allowed = B(reader["Allowed"])
                });
            }
        }

        return dto;
    }

    public async Task<AuthorityResult> SavePageAccessAsync(
        SessionSnapshot session, AuthAccessSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthAccessSaveRequest();
        var userName = (request.UserName ?? "").Trim();
        if (userName.Length == 0)
            return Fail("al.needUser");

        var selected = (request.LinkIDs ?? []).Distinct().ToHashSet();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        int registrationId;
        await using (var cmd = new SqlCommand("""
SELECT TOP 1 RegistrationID FROM dbo.Registration WHERE UserName = @UserName AND SchoolID = 0
""", con))
        {
            cmd.Parameters.AddWithValue("@UserName", userName);
            registrationId = I(await cmd.ExecuteScalarAsync(ct));
        }
        if (registrationId <= 0)
            return Fail("al.needUser");

        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            var pages = new List<(int LinkID, string? RoleName)>();
            await using (var cmd = new SqlCommand("""
SELECT p.LinkID, r.RoleName
FROM dbo.Authority_Link_Pages AS p
LEFT JOIN dbo.aspnet_Roles AS r ON r.RoleId = p.RoleId
""", con, tx))
            await using (var reader = await cmd.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                    pages.Add((I(reader["LinkID"]), reader["RoleName"] as string));
            }

            var current = new HashSet<int>();
            await using (var cmd = new SqlCommand(
                "SELECT LinkID FROM dbo.Authority_Link_Users WHERE UserName = @UserName", con, tx))
            {
                cmd.Parameters.AddWithValue("@UserName", userName);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                    current.Add(I(reader["LinkID"]));
            }

            foreach (var page in pages)
            {
                var want = selected.Contains(page.LinkID);
                var have = current.Contains(page.LinkID);
                if (want && !have)
                {
                    await using var cmd = new SqlCommand("""
IF NOT EXISTS (SELECT 1 FROM dbo.Authority_Link_Users WHERE LinkID = @LinkID AND UserName = @UserName)
INSERT INTO dbo.Authority_Link_Users (SchoolID, RegistrationID, LinkID, UserName)
VALUES (0, @RegistrationID, @LinkID, @UserName)
""", con, tx);
                    cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                    cmd.Parameters.AddWithValue("@LinkID", page.LinkID);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                else if (!want && have)
                {
                    await using var cmd = new SqlCommand(
                        "DELETE FROM dbo.Authority_Link_Users WHERE LinkID = @LinkID AND UserName = @UserName", con, tx);
                    cmd.Parameters.AddWithValue("@LinkID", page.LinkID);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }

            var selectedRoles = pages
                .Where(x => selected.Contains(x.LinkID) && !string.IsNullOrWhiteSpace(x.RoleName))
                .Select(x => x.RoleName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var unselectedRoles = pages
                .Where(x => !selected.Contains(x.LinkID) && !string.IsNullOrWhiteSpace(x.RoleName))
                .Select(x => x.RoleName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(role => selectedRoles.All(keep => !string.Equals(keep, role, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var role in selectedRoles)
                await AuthorityBasicService.AddToRoleAsync(con, tx, userName, role, ct);
            foreach (var role in unselectedRoles)
            {
                if (ProtectedRoles.Contains(role))
                    continue;
                await RemoveFromRoleAsync(con, tx, userName, role, ct);
            }

            await tx.CommitAsync(ct);
            return Ok("al.accessSaved");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    private static async Task RemoveFromRoleAsync(
        SqlConnection con, SqlTransaction tx, string userName, string roleName, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
DELETE ur
FROM dbo.aspnet_UsersInRoles AS ur
INNER JOIN dbo.aspnet_Users AS u ON u.UserId = ur.UserId
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.aspnet_Roles AS r ON r.RoleId = ur.RoleId AND r.ApplicationId = a.ApplicationId
WHERE u.LoweredUserName = LOWER(@UserName)
  AND r.LoweredRoleName = LOWER(@RoleName)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@RoleName", roleName);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
