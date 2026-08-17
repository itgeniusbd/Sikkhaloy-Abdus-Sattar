using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Menu;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed class MasterDataService
{
    private readonly EduConnectionFactory _connections;

    public MasterDataService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<SchoolClassDto>> GetClassesAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT ClassID, Class, ISNULL(SN, ClassID) AS SortOrder
FROM dbo.CreateClass
WHERE SchoolID = @SchoolID
ORDER BY SN, ClassID";

        var items = new List<SchoolClassDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SchoolClassDto
            {
                ClassID = Convert.ToInt32(reader["ClassID"]),
                Name = reader["Class"]?.ToString() ?? "",
                SortOrder = Convert.ToInt32(reader["SortOrder"])
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<EducationYearDto>> GetYearsAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT EducationYearID, EducationYear, ISNULL(SN, EducationYearID) AS SortOrder, StartDate, EndDate
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID
ORDER BY SN, EducationYearID";

        var items = new List<EducationYearDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EducationYearDto
            {
                EducationYearID = Convert.ToInt32(reader["EducationYearID"]),
                Name = reader["EducationYear"]?.ToString() ?? "",
                SortOrder = Convert.ToInt32(reader["SortOrder"]),
                StartDate = reader["StartDate"] is DBNull ? null : Convert.ToDateTime(reader["StartDate"]),
                EndDate = reader["EndDate"] is DBNull ? null : Convert.ToDateTime(reader["EndDate"]),
                IsCurrent = Convert.ToInt32(reader["EducationYearID"]) == session.EducationYearID
            });
        }

        return items;
    }

    public async Task<OfficeProfileDto> GetProfileAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1
    Admin.AdminID,
    LTRIM(RTRIM(ISNULL(Admin.FirstName, N'') + N' ' + ISNULL(Admin.LastName, N''))) AS DisplayName,
    Admin.Image
FROM dbo.Admin
WHERE Admin.SchoolID = @SchoolID
  AND Admin.RegistrationID = @RegistrationID";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var profile = new OfficeProfileDto
        {
            DisplayName = session.UserName,
            Role = session.Role,
            SchoolID = session.SchoolID,
            SchoolName = session.SchoolName
        };

        if (await reader.ReadAsync(cancellationToken))
        {
            profile.AdminID = reader["AdminID"] is DBNull ? 0 : Convert.ToInt32(reader["AdminID"]);
            var name = (reader["DisplayName"]?.ToString() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(name))
                profile.DisplayName = name;
            if (reader["Image"] is byte[] bytes && bytes.Length > 0)
                profile.PhotoBase64 = Convert.ToBase64String(bytes);
        }

        await reader.CloseAsync();
        await FillSchoolAsync(con, profile, cancellationToken);
        return profile;
    }

    private static async Task FillSchoolAsync(
        SqlConnection con, OfficeProfileDto profile, CancellationToken cancellationToken)
    {
        await using (var cmd = new SqlCommand("""
SELECT SchoolID, SchoolName, Address, Phone, Email, SchoolLogo
FROM dbo.SchoolInfo
WHERE SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", profile.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return;

            profile.SchoolID = reader["SchoolID"] is DBNull ? profile.SchoolID : Convert.ToInt32(reader["SchoolID"]);
            var schoolName = (reader["SchoolName"]?.ToString() ?? "").Trim();
            if (schoolName.Length > 0)
                profile.SchoolName = schoolName;
            profile.Address = reader["Address"]?.ToString()?.Trim() ?? "";
            profile.Phone = reader["Phone"]?.ToString()?.Trim() ?? "";
            profile.Email = reader["Email"]?.ToString()?.Trim() ?? "";
            if (reader["SchoolLogo"] is byte[] logo && logo.Length > 0)
                profile.LogoBase64 = Convert.ToBase64String(logo);
        }

        await using var exists = new SqlCommand(
            "SELECT CASE WHEN COL_LENGTH(N'dbo.SchoolInfo', N'SchoolNameLogo') IS NULL THEN 0 ELSE 1 END", con);
        if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) == 0)
        {
            profile.ClearNameLogo = true;
            return;
        }

        await using var nameCmd = new SqlCommand(
            "SELECT SchoolNameLogo FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con);
        nameCmd.Parameters.AddWithValue("@SchoolID", profile.SchoolID);
        var nameLogo = await nameCmd.ExecuteScalarAsync(cancellationToken) as byte[];
        if (nameLogo is { Length: > 0 })
            profile.NameLogoBase64 = Convert.ToBase64String(nameLogo);
        else
            profile.ClearNameLogo = true;
    }

    public async Task<MenuTreeDto> GetMenuAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        var isAdmin = string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var sql = isAdmin
            ? """
SELECT c.LinkCategoryID, LTRIM(RTRIM(c.Category)) AS Category, c.Ascending AS CatSort,
       sc.SubCategoryID, LTRIM(RTRIM(sc.SubCategory)) AS SubCategory, ISNULL(sc.Ascending, 0) AS SubSort,
       p.LinkID, LTRIM(RTRIM(p.PageTitle)) AS PageTitle, p.PageURL, ISNULL(p.Ascending, 0) AS PageSort
FROM dbo.Link_Pages AS p
INNER JOIN dbo.Link_Category AS c ON c.LinkCategoryID = p.LinkCategoryID
LEFT JOIN dbo.Link_SubCategory AS sc ON sc.SubCategoryID = p.SubCategoryID
ORDER BY c.Ascending, ISNULL(sc.Ascending, 0), ISNULL(p.Ascending, 0), p.LinkID
"""
            : """
SELECT c.LinkCategoryID, LTRIM(RTRIM(c.Category)) AS Category, c.Ascending AS CatSort,
       sc.SubCategoryID, LTRIM(RTRIM(sc.SubCategory)) AS SubCategory, ISNULL(sc.Ascending, 0) AS SubSort,
       p.LinkID, LTRIM(RTRIM(p.PageTitle)) AS PageTitle, p.PageURL, ISNULL(p.Ascending, 0) AS PageSort
FROM dbo.Link_Users AS u
INNER JOIN dbo.Link_Pages AS p ON p.LinkID = u.LinkID
INNER JOIN dbo.Link_Category AS c ON c.LinkCategoryID = p.LinkCategoryID
LEFT JOIN dbo.Link_SubCategory AS sc ON sc.SubCategoryID = p.SubCategoryID
WHERE u.RegistrationID = @RegistrationID
ORDER BY c.Ascending, ISNULL(sc.Ascending, 0), ISNULL(p.Ascending, 0), p.LinkID
""";

        var categories = new Dictionary<int, MenuCategoryDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        if (!isAdmin)
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var categoryId = Convert.ToInt32(reader["LinkCategoryID"]);
            if (!categories.TryGetValue(categoryId, out var category))
            {
                category = new MenuCategoryDto
                {
                    CategoryID = categoryId,
                    Name = reader["Category"]?.ToString() ?? "",
                    Sort = Convert.ToInt32(reader["CatSort"])
                };
                categories[categoryId] = category;
            }

            var link = new MenuLinkDto
            {
                LinkID = Convert.ToInt32(reader["LinkID"]),
                Title = reader["PageTitle"]?.ToString() ?? "",
                PageUrl = reader["PageURL"]?.ToString() ?? "",
                Sort = Convert.ToInt32(reader["PageSort"])
            };
            HybridMenuRoutes.Apply(link);

            var subIdObj = reader["SubCategoryID"];
            if (subIdObj is DBNull or null)
            {
                category.Links.Add(link);
                continue;
            }

            var subId = Convert.ToInt32(subIdObj);
            var sub = category.Subs.FirstOrDefault(x => x.SubCategoryID == subId);
            if (sub is null)
            {
                sub = new MenuSubDto
                {
                    SubCategoryID = subId,
                    Name = reader["SubCategory"]?.ToString() ?? "",
                    Sort = Convert.ToInt32(reader["SubSort"])
                };
                category.Subs.Add(sub);
            }

            sub.Links.Add(link);
        }

        var tree = new MenuTreeDto
        {
            Categories = categories.Values
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name)
                .ToList()
        };
        HybridMenuRoutes.Deduplicate(tree);
        return tree;
    }

    public async Task<AdminInfoDto?> GetAdminAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT TOP 1 AdminID, FirstName, LastName, FatherName, Gender, Designation, City, PostalCode,
       Phone, Email, Address, Image
FROM dbo.Admin
WHERE SchoolID = @SchoolID AND RegistrationID = @RegistrationID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var dto = new AdminInfoDto
        {
            AdminID = reader["AdminID"] is DBNull ? 0 : Convert.ToInt32(reader["AdminID"]),
            FirstName = reader["FirstName"]?.ToString()?.Trim() ?? "",
            LastName = reader["LastName"]?.ToString()?.Trim() ?? "",
            FatherName = NullText(reader["FatherName"]),
            Gender = NullText(reader["Gender"]),
            Designation = NullText(reader["Designation"]),
            City = NullText(reader["City"]),
            PostalCode = NullText(reader["PostalCode"]),
            Phone = NullText(reader["Phone"]),
            Email = NullText(reader["Email"]),
            Address = NullText(reader["Address"])
        };
        if (reader["Image"] is byte[] bytes && bytes.Length > 0)
        {
            var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
            dto.PhotoDataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        return dto;
    }

    public async Task<ProfileResult> SaveAdminAsync(
        SessionSnapshot session, AdminInfoDto? request, CancellationToken cancellationToken)
    {
        if (request is null || request.AdminID <= 0)
            return new ProfileResult { Error = "profile.noAdmin" };
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return new ProfileResult { Error = "profile.needName" };

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Admin
SET FirstName = @FirstName, LastName = @LastName, FatherName = @FatherName, Gender = @Gender,
    Designation = @Designation, Address = @Address, City = @City, PostalCode = @PostalCode,
    Phone = @Phone, Email = @Email
WHERE AdminID = @AdminID AND SchoolID = @SchoolID AND RegistrationID = @RegistrationID
""", con);
        cmd.Parameters.AddWithValue("@FirstName", request.FirstName.Trim());
        cmd.Parameters.AddWithValue("@LastName", (request.LastName ?? "").Trim());
        cmd.Parameters.AddWithValue("@FatherName", (object?)request.FatherName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Gender", (object?)request.Gender ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Designation", (object?)request.Designation ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Address", (object?)request.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", (object?)request.City ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PostalCode", (object?)request.PostalCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AdminID", request.AdminID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (n <= 0)
            return new ProfileResult { Error = "profile.noAdmin" };

        if (!string.IsNullOrWhiteSpace(request.PhotoDataUrl))
        {
            var bytes = DecodeImage(request.PhotoDataUrl);
            if (bytes.Length > 0)
            {
                await using var img = new SqlCommand("""
UPDATE dbo.Admin SET Image = @Image
WHERE AdminID = @AdminID AND SchoolID = @SchoolID AND RegistrationID = @RegistrationID
""", con);
                var p = img.Parameters.Add("@Image", System.Data.SqlDbType.VarBinary, -1);
                p.Value = bytes;
                img.Parameters.AddWithValue("@AdminID", request.AdminID);
                img.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                img.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                await img.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        var display = (request.FirstName.Trim() + " " + (request.LastName ?? "").Trim()).Trim();
        return new ProfileResult
        {
            Succeeded = true,
            DisplayName = display,
            PhotoDataUrl = request.PhotoDataUrl
        };
    }

    public async Task<ProfileResult> ChangePasswordAsync(
        SessionSnapshot session, ChangePasswordRequest? request, CancellationToken cancellationToken)
    {
        var current = request?.CurrentPassword ?? "";
        var next = request?.NewPassword ?? "";
        var confirm = request?.ConfirmPassword ?? "";
        if (current.Length == 0 || next.Length == 0)
            return new ProfileResult { Error = "profile.pwRequired" };
        if (next.Length is < 8 or > 30)
            return new ProfileResult { Error = "profile.pwShort" };
        if (!string.Equals(next, confirm, StringComparison.Ordinal))
            return new ProfileResult { Error = "profile.pwMismatch" };

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var find = new SqlCommand("""
SELECT m.Password, m.PasswordSalt, m.PasswordFormat, u.UserId
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Membership AS m ON u.UserId = m.UserId
INNER JOIN dbo.aspnet_Applications AS a ON u.ApplicationId = a.ApplicationId
WHERE u.LoweredUserName = LOWER(@UserName)
  AND a.LoweredApplicationName = N'/'
""", con);
        find.Parameters.AddWithValue("@UserName", session.UserName);
        Guid userId;
        string storedHash;
        string storedSalt;
        int format;
        await using (var reader = await find.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
                return new ProfileResult { Error = "login.userNotFound" };
            storedHash = reader.GetString(0);
            storedSalt = reader.GetString(1);
            format = reader.GetInt32(2);
            userId = reader.GetGuid(3);
        }

        if (!MembershipPasswordVerifier.Verify(current, storedHash, storedSalt, format))
            return new ProfileResult { Error = "login.badPassword" };

        var salt = MembershipPasswordVerifier.NewSalt();
        var hashed = format == 0 ? next : MembershipPasswordVerifier.Hash(next, salt);
        await using (var upd = new SqlCommand("""
UPDATE dbo.aspnet_Membership
SET Password = @Password, PasswordSalt = @Salt, LastPasswordChangedDate = GETUTCDATE()
WHERE UserId = @UserId
""", con))
        {
            upd.Parameters.AddWithValue("@Password", hashed);
            upd.Parameters.AddWithValue("@Salt", format == 0 ? storedSalt : salt);
            upd.Parameters.AddWithValue("@UserId", userId);
            await upd.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            await using var ast = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
    UPDATE dbo.AST SET Password = @Password WHERE RegistrationID = @RegistrationID
""", con);
            ast.Parameters.AddWithValue("@Password", next);
            ast.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            await ast.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException)
        {
        }

        return new ProfileResult { Succeeded = true };
    }

    private static string? NullText(object? value)
    {
        var text = value is null or DBNull ? null : value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static byte[] DecodeImage(string raw)
    {
        raw = raw.Trim();
        var comma = raw.IndexOf(',');
        if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > 0)
            raw = raw[(comma + 1)..];
        try
        {
            return Convert.FromBase64String(raw);
        }
        catch (FormatException)
        {
            return [];
        }
    }
}
