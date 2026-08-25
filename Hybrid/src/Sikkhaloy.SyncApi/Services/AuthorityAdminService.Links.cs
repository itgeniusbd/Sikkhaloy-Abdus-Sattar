using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityAdminService
{
    public async Task<AuthLinkTreeDto> GetLinksAsync(
        SessionSnapshot session, int categoryId, int subId, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthLinkTreeDto { CategoryId = categoryId, SubCategoryId = subId };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT LinkCategoryID, Category, Ascending
FROM dbo.Link_Category
ORDER BY Ascending, Category
""", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var row = new AuthLinkCategoryRowDto
                {
                    LinkCategoryID = I(reader["LinkCategoryID"]),
                    Category = S(reader["Category"]),
                    Ascending = I(reader["Ascending"])
                };
                dto.Categories.Add(row);
                if (row.LinkCategoryID == categoryId)
                    dto.CategoryName = row.Category;
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT CAST(r.RoleId AS nvarchar(50)) AS RoleId, r.RoleName
FROM dbo.aspnet_Roles AS r
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = r.ApplicationId AND a.LoweredApplicationName = N'/'
ORDER BY r.RoleName
""", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Roles.Add(new AuthRoleOptionDto
                {
                    RoleId = S(reader["RoleId"]),
                    RoleName = S(reader["RoleName"])
                });
            }
        }

        if (categoryId <= 0)
            return dto;

        await using (var cmd = new SqlCommand("""
SELECT SubCategoryID, LinkCategoryID, Ascending, SubCategory
FROM dbo.Link_SubCategory
WHERE LinkCategoryID = @Id
ORDER BY Ascending, SubCategory
""", con))
        {
            cmd.Parameters.AddWithValue("@Id", categoryId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var row = new AuthLinkSubRowDto
                {
                    SubCategoryID = I(reader["SubCategoryID"]),
                    LinkCategoryID = I(reader["LinkCategoryID"]),
                    Ascending = I(reader["Ascending"]),
                    SubCategory = S(reader["SubCategory"])
                };
                dto.Subs.Add(row);
                if (row.SubCategoryID == subId)
                    dto.SubCategoryName = row.SubCategory;
            }
        }

        var pageSql = subId > 0
            ? """
SELECT p.LinkID, p.LinkCategoryID, ISNULL(p.SubCategoryID, 0) AS SubCategoryID, p.Ascending, p.PageURL, p.PageTitle,
       ISNULL(c.Category, N'') AS Category, ISNULL(sc.SubCategory, N'') AS SubCategory,
       ISNULL(CAST(p.RoleId AS nvarchar(50)), N'') AS RoleId, ISNULL(r.RoleName, N'') AS RoleName
FROM dbo.Link_Pages AS p
LEFT JOIN dbo.Link_Category AS c ON c.LinkCategoryID = p.LinkCategoryID
LEFT JOIN dbo.Link_SubCategory AS sc ON sc.SubCategoryID = p.SubCategoryID
LEFT JOIN dbo.aspnet_Roles AS r ON r.RoleId = p.RoleId
WHERE p.LinkCategoryID = @Cat AND p.SubCategoryID = @Sub
ORDER BY p.Ascending, p.PageTitle
"""
            : """
SELECT p.LinkID, p.LinkCategoryID, ISNULL(p.SubCategoryID, 0) AS SubCategoryID, p.Ascending, p.PageURL, p.PageTitle,
       ISNULL(c.Category, N'') AS Category, ISNULL(sc.SubCategory, N'') AS SubCategory,
       ISNULL(CAST(p.RoleId AS nvarchar(50)), N'') AS RoleId, ISNULL(r.RoleName, N'') AS RoleName
FROM dbo.Link_Pages AS p
LEFT JOIN dbo.Link_Category AS c ON c.LinkCategoryID = p.LinkCategoryID
LEFT JOIN dbo.Link_SubCategory AS sc ON sc.SubCategoryID = p.SubCategoryID
LEFT JOIN dbo.aspnet_Roles AS r ON r.RoleId = p.RoleId
WHERE p.LinkCategoryID = @Cat AND p.SubCategoryID IS NULL
ORDER BY p.Ascending, p.PageTitle
""";
        await using (var cmd = new SqlCommand(pageSql, con))
        {
            cmd.Parameters.AddWithValue("@Cat", categoryId);
            if (subId > 0)
                cmd.Parameters.AddWithValue("@Sub", subId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Pages.Add(new AuthLinkPageRowDto
                {
                    LinkID = I(reader["LinkID"]),
                    LinkCategoryID = I(reader["LinkCategoryID"]),
                    SubCategoryID = I(reader["SubCategoryID"]),
                    Ascending = I(reader["Ascending"]),
                    PageURL = S(reader["PageURL"]),
                    PageTitle = S(reader["PageTitle"]),
                    Category = S(reader["Category"]),
                    SubCategory = S(reader["SubCategory"]),
                    RoleId = S(reader["RoleId"]),
                    RoleName = S(reader["RoleName"])
                });
            }
        }

        return dto;
    }

    public async Task<AuthorityResult> SaveCategoryAsync(
        SessionSnapshot session, AuthLinkNameSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthLinkNameSaveRequest();
        var name = (request.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("al.catName");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request.Id > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Link_Category SET Ascending = @Ascending, Category = @Name WHERE LinkCategoryID = @Id
""", con);
            cmd.Parameters.AddWithValue("@Ascending", request.Ascending);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Id", request.Id);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ab.saved", request.Id);
        }

        await using (var cmd = new SqlCommand("""
INSERT INTO dbo.Link_Category (Ascending, Category) VALUES (@Ascending, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con))
        {
            cmd.Parameters.AddWithValue("@Ascending", request.Ascending);
            cmd.Parameters.AddWithValue("@Name", name);
            return Ok("ab.saved", I(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    public async Task<AuthorityResult> DeleteCategoryAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        Guard(session);
        if (id <= 0)
            return Fail("al.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("DELETE FROM dbo.Link_Category WHERE LinkCategoryID = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("al.deleted");
        }
        catch (SqlException ex) when (ex.Number is 547)
        {
            return Fail("al.linkInUse");
        }
    }

    public async Task<AuthorityResult> SaveSubAsync(
        SessionSnapshot session, AuthLinkNameSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthLinkNameSaveRequest();
        var name = (request.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("al.subName");
        if (request.ParentId <= 0 && request.Id <= 0)
            return Fail("al.needCat");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request.Id > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Link_SubCategory
SET LinkCategoryID = CASE WHEN @ParentId > 0 THEN @ParentId ELSE LinkCategoryID END,
    Ascending = @Ascending, SubCategory = @Name
WHERE SubCategoryID = @Id
""", con);
            cmd.Parameters.AddWithValue("@ParentId", request.ParentId);
            cmd.Parameters.AddWithValue("@Ascending", request.Ascending);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Id", request.Id);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ab.saved", request.Id);
        }

        await using (var cmd = new SqlCommand("""
INSERT INTO dbo.Link_SubCategory (LinkCategoryID, Ascending, SubCategory)
VALUES (@ParentId, @Ascending, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con))
        {
            cmd.Parameters.AddWithValue("@ParentId", request.ParentId);
            cmd.Parameters.AddWithValue("@Ascending", request.Ascending);
            cmd.Parameters.AddWithValue("@Name", name);
            return Ok("ab.saved", I(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    public async Task<AuthorityResult> DeleteSubAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        Guard(session);
        if (id <= 0)
            return Fail("al.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("DELETE FROM dbo.Link_SubCategory WHERE SubCategoryID = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("al.deleted");
        }
        catch (SqlException ex) when (ex.Number is 547)
        {
            return Fail("al.linkInUse");
        }
    }

    public async Task<AuthorityResult> SavePageAsync(
        SessionSnapshot session, AuthLinkPageSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthLinkPageSaveRequest();
        var title = (request.PageTitle ?? "").Trim();
        var url = (request.PageURL ?? "").Trim();
        if (title.Length == 0 || url.Length == 0)
            return Fail("al.pageRequired");
        if (request.LinkCategoryID <= 0)
            return Fail("al.needCat");

        var role = RoleIdValue(request.RoleId);
        var sub = request.SubCategoryID > 0 ? request.SubCategoryID : (object)DBNull.Value;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request.LinkID > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Link_Pages
SET Ascending = @Ascending, PageURL = @Url, PageTitle = @Title,
    LinkCategoryID = @Cat, SubCategoryID = @Sub, RoleId = @RoleId
WHERE LinkID = @Id
""", con);
            cmd.Parameters.AddWithValue("@Ascending", request.Ascending);
            cmd.Parameters.AddWithValue("@Url", url);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Cat", request.LinkCategoryID);
            cmd.Parameters.AddWithValue("@Sub", sub);
            cmd.Parameters.AddWithValue("@RoleId", role);
            cmd.Parameters.AddWithValue("@Id", request.LinkID);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ab.saved", request.LinkID);
        }

        await using (var cmd = new SqlCommand("""
INSERT INTO dbo.Link_Pages (LinkCategoryID, Ascending, PageURL, PageTitle, SubCategoryID, RoleId)
VALUES (@Cat, @Ascending, @Url, @Title, @Sub, @RoleId);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con))
        {
            cmd.Parameters.AddWithValue("@Cat", request.LinkCategoryID);
            cmd.Parameters.AddWithValue("@Ascending", request.Ascending);
            cmd.Parameters.AddWithValue("@Url", url);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Sub", sub);
            cmd.Parameters.AddWithValue("@RoleId", role);
            return Ok("ab.saved", I(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    public async Task<AuthorityResult> DeletePageAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        Guard(session);
        if (id <= 0)
            return Fail("al.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await using (var cmd = new SqlCommand("DELETE FROM dbo.Link_Users WHERE LinkID = @Id", con, tx))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            await using (var cmd = new SqlCommand("DELETE FROM dbo.Link_Pages WHERE LinkID = @Id", con, tx))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);
            return Ok("al.deleted");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    private static object RoleIdValue(string? roleId)
    {
        if (Guid.TryParse(roleId, out var g) && g != Guid.Empty)
            return g;
        return DBNull.Value;
    }
}
