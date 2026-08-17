using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Access;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class PageAccessService
{
    private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Sub-Admin"
    };

    private readonly EduConnectionFactory _connections;

    public PageAccessService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<SubAdminDto>> ListSubAdminsAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT Registration.RegistrationID,
       Registration.UserName,
       LTRIM(RTRIM(ISNULL(Admin.FirstName, N'') + N' ' + ISNULL(Admin.LastName, N'')))
           + N'(' + Registration.UserName + N')' AS Name
FROM dbo.Admin
INNER JOIN dbo.Registration ON Admin.RegistrationID = Registration.RegistrationID
WHERE Admin.SchoolID = @SchoolID
  AND Registration.Category = N'Sub-Admin'
  AND Registration.Validation = N'Valid'
ORDER BY Name
""";

        var items = new List<SubAdminDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SubAdminDto
            {
                RegistrationID = Convert.ToInt32(reader["RegistrationID"]),
                UserName = reader["UserName"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? ""
            });
        }

        return items;
    }

    public async Task<PageAccessDto> GetAsync(SessionSnapshot session, string userName, CancellationToken cancellationToken)
    {
        userName = (userName ?? "").Trim();
        var result = new PageAccessDto { UserName = userName };
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        if (userName.Length > 0)
            result.RegistrationID = await GetRegistrationIdAsync(con, session.SchoolID, userName, cancellationToken);

        await using var cmd = new SqlCommand("""
SELECT p.LinkID,
       LTRIM(RTRIM(ISNULL(c.Category, N''))) AS Category,
       LTRIM(RTRIM(ISNULL(sc.SubCategory, N''))) AS SubCategory,
       LTRIM(RTRIM(ISNULL(p.PageTitle, N''))) AS PageTitle,
       r.RoleName,
       CAST(CASE WHEN u.LinkID IS NULL THEN 0 ELSE 1 END AS BIT) AS Allowed
FROM dbo.Link_Pages AS p
LEFT JOIN dbo.Link_Category AS c ON c.LinkCategoryID = p.LinkCategoryID
LEFT JOIN dbo.Link_SubCategory AS sc ON sc.SubCategoryID = p.SubCategoryID
LEFT JOIN dbo.aspnet_Roles AS r ON r.RoleId = p.RoleId
LEFT JOIN dbo.Link_Users AS u ON u.LinkID = p.LinkID AND u.UserName = @UserName
ORDER BY ISNULL(c.Ascending, 0), ISNULL(sc.Ascending, 0), ISNULL(p.Ascending, 0), p.LinkID
""", con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Pages.Add(new PageAccessRowDto
            {
                LinkID = Convert.ToInt32(reader["LinkID"]),
                Category = reader["Category"]?.ToString() ?? "",
                SubCategory = reader["SubCategory"]?.ToString() ?? "",
                PageTitle = reader["PageTitle"]?.ToString() ?? "",
                RoleName = reader["RoleName"] as string,
                Allowed = Convert.ToBoolean(reader["Allowed"])
            });
        }

        return result;
    }

    public async Task<SavePageAccessResult> SaveAsync(SessionSnapshot session, SavePageAccessRequest? request, CancellationToken cancellationToken)
    {
        request ??= new SavePageAccessRequest();
        var userName = (request.UserName ?? "").Trim();
        if (userName.Length == 0)
            return Fail("Select Sub-Admin");

        var selected = (request.LinkIDs ?? []).Distinct().ToHashSet();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        var registrationId = await GetRegistrationIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (registrationId <= 0)
            return Fail("Select Sub-Admin");

        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var pages = await LoadPagesAsync(con, tx, cancellationToken);
            var current = await LoadGrantedAsync(con, tx, userName, cancellationToken);

            foreach (var page in pages)
            {
                var want = selected.Contains(page.LinkID);
                var have = current.Contains(page.LinkID);
                if (want && !have)
                    await InsertLinkAsync(con, tx, session.SchoolID, registrationId, page.LinkID, userName, cancellationToken);
                else if (!want && have)
                    await DeleteLinkAsync(con, tx, page.LinkID, userName, cancellationToken);
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
                await AddToRoleAsync(con, tx, userName, role, cancellationToken);
            foreach (var role in unselectedRoles)
            {
                if (ProtectedRoles.Contains(role))
                    continue;
                await RemoveFromRoleAsync(con, tx, userName, role, cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new SavePageAccessResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    private static SavePageAccessResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<int> GetRegistrationIdAsync(
        SqlConnection con,
        int schoolId,
        string userName,
        CancellationToken cancellationToken,
        SqlTransaction? tx = null)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 RegistrationID
FROM dbo.Registration
WHERE UserName = @UserName AND SchoolID = @SchoolID AND Category = N'Sub-Admin'
""", con);
        if (tx is not null)
            cmd.Transaction = tx;
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task<List<PageAccessRowDto>> LoadPagesAsync(SqlConnection con, SqlTransaction tx, CancellationToken cancellationToken)
    {
        var pages = new List<PageAccessRowDto>();
        await using var cmd = new SqlCommand("""
SELECT p.LinkID, r.RoleName
FROM dbo.Link_Pages AS p
LEFT JOIN dbo.aspnet_Roles AS r ON r.RoleId = p.RoleId
""", con, tx);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pages.Add(new PageAccessRowDto
            {
                LinkID = Convert.ToInt32(reader["LinkID"]),
                RoleName = reader["RoleName"] as string
            });
        }

        return pages;
    }

    private static async Task<HashSet<int>> LoadGrantedAsync(SqlConnection con, SqlTransaction tx, string userName, CancellationToken cancellationToken)
    {
        var ids = new HashSet<int>();
        await using var cmd = new SqlCommand("SELECT LinkID FROM dbo.Link_Users WHERE UserName = @UserName", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(Convert.ToInt32(reader["LinkID"]));
        return ids;
    }

    private static async Task InsertLinkAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int registrationId, int linkId, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF NOT EXISTS (SELECT 1 FROM dbo.Link_Users WHERE LinkID = @LinkID AND UserName = @UserName)
INSERT INTO dbo.Link_Users (SchoolID, RegistrationID, LinkID, UserName)
VALUES (@SchoolID, @RegistrationID, @LinkID, @UserName)
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@LinkID", linkId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task DeleteLinkAsync(SqlConnection con, SqlTransaction tx, int linkId, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.Link_Users WHERE LinkID = @LinkID AND UserName = @UserName", con, tx);
        cmd.Parameters.AddWithValue("@LinkID", linkId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task AddToRoleAsync(SqlConnection con, SqlTransaction tx, string userName, string roleName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.aspnet_Roles AS r ON r.ApplicationId = a.ApplicationId AND r.LoweredRoleName = LOWER(@RoleName)
WHERE u.LoweredUserName = LOWER(@UserName)
  AND NOT EXISTS (
      SELECT 1 FROM dbo.aspnet_UsersInRoles AS ur
      WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@RoleName", roleName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task RemoveFromRoleAsync(SqlConnection con, SqlTransaction tx, string userName, string roleName, CancellationToken cancellationToken)
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
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
