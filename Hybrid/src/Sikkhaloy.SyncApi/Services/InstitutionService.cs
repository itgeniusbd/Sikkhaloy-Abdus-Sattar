using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Institution;

namespace Sikkhaloy.SyncApi.Services;

public sealed class InstitutionService
{
    private const int MaxLogoBytes = 1_500_000;
    private readonly EduConnectionFactory _connections;

    public InstitutionService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<PublicInstituteDto>> ListPublicAsync(CancellationToken cancellationToken)
    {
        const string sql = """
SELECT SchoolID, SchoolName,
       CASE WHEN SchoolLogo IS NULL OR DATALENGTH(SchoolLogo) = 0 THEN 0 ELSE 1 END AS HasLogo
FROM dbo.SchoolInfo
WHERE LTRIM(RTRIM(ISNULL(SchoolName, N''))) <> N''
ORDER BY SchoolName
""";

        var list = new List<PublicInstituteDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new PublicInstituteDto
            {
                SchoolID = Convert.ToInt32(reader["SchoolID"]),
                SchoolName = reader["SchoolName"]?.ToString()?.Trim() ?? "",
                HasLogo = Convert.ToInt32(reader["HasLogo"]) == 1
            });
        }

        return list;
    }

    public async Task<PublicStatsDto> GetPublicStatsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
SELECT
    (SELECT COUNT(SchoolID) FROM dbo.SchoolInfo) AS Total_Institution,
    (SELECT COUNT(StudentID) FROM dbo.Student) AS Total_Student,
    (SELECT COUNT(EmployeeID) FROM dbo.Employee_Info) AS Total_Teacher
""";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new PublicStatsDto();

        return new PublicStatsDto
        {
            Institutions = Convert.ToInt32(reader["Total_Institution"]),
            Students = Convert.ToInt32(reader["Total_Student"]),
            Teachers = Convert.ToInt32(reader["Total_Teacher"])
        };
    }

    public async Task<PublicContactResult> SendPublicContactAsync(PublicContactRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        var mobile = (request?.MobileNo ?? "").Trim();
        var message = (request?.Message ?? "").Trim();
        var subject = (request?.Subject ?? "").Trim();
        var email = (request?.Email ?? "").Trim();
        if (name.Length == 0 || mobile.Length < 8 || message.Length == 0)
            return new PublicContactResult { Succeeded = false, Error = "home.pop.need" };
        if (name.Length > 120 || mobile.Length > 30 || subject.Length > 200 || message.Length > 4000 || email.Length > 120)
            return new PublicContactResult { Succeeded = false, Error = "home.pop.need" };
        if (subject.Length == 0)
            subject = "Website inquiry";

        const string sql = """
INSERT INTO dbo.Public_Contact_US (Name, Email, MobileNo, Subject, Message)
VALUES (@Name, @Email, @MobileNo, @Subject, @Message)
""";

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@MobileNo", mobile);
            cmd.Parameters.AddWithValue("@Subject", subject);
            cmd.Parameters.AddWithValue("@Message", message);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return new PublicContactResult { Succeeded = true };
        }
        catch (Exception)
        {
            return new PublicContactResult { Succeeded = false, Error = "home.pop.fail" };
        }
    }

    public async Task<byte[]?> GetPublicLogoAsync(int schoolId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT SchoolLogo
FROM dbo.SchoolInfo
WHERE SchoolID = @SchoolID
""";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is byte[] bytes && bytes.Length > 0 ? bytes : null;
    }

    public async Task<InstitutionDto> GetAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT SchoolID, SchoolName, Institution_Dialog, Established, Principal, AcadamicStaff, Students,
       Address, City, State, LocalArea, PostalCode, Phone, Email, Website, SchoolLogo
FROM dbo.SchoolInfo
WHERE SchoolID = @SchoolID
""";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new InstitutionDto { SchoolID = session.SchoolID, SchoolName = session.SchoolName };

        var dto = new InstitutionDto
        {
            SchoolID = Convert.ToInt32(reader["SchoolID"]),
            SchoolName = reader["SchoolName"]?.ToString() ?? "",
            InstitutionDialog = ReadString(reader, "Institution_Dialog"),
            Established = ReadString(reader, "Established"),
            Principal = ReadString(reader, "Principal"),
            AcadamicStaff = ReadString(reader, "AcadamicStaff"),
            Students = ReadString(reader, "Students"),
            Address = ReadString(reader, "Address"),
            City = ReadString(reader, "City"),
            State = ReadString(reader, "State"),
            LocalArea = ReadString(reader, "LocalArea"),
            PostalCode = ReadString(reader, "PostalCode"),
            Phone = ReadString(reader, "Phone"),
            Email = ReadString(reader, "Email"),
            Website = ReadString(reader, "Website"),
            LogoDataUrl = ToDataUrl(reader["SchoolLogo"] as byte[])
        };
        await reader.CloseAsync();
        dto.NameLogoDataUrl = await ReadNameLogoAsync(con, session.SchoolID, cancellationToken);
        return dto;
    }

    public async Task<InstitutionResult> SaveAsync(
        SessionSnapshot session, InstitutionDto? request, CancellationToken cancellationToken)
    {
        if (request is null)
            return Fail("inst.failed");

        var name = (request.SchoolName ?? "").Trim();
        if (name.Length == 0)
            return Fail("inst.needName");

        byte[]? logo = null;
        byte[]? nameLogo = null;
        if (!string.IsNullOrWhiteSpace(request.LogoBase64))
        {
            logo = DecodeImage(request.LogoBase64);
            if (logo is null)
                return Fail("inst.badLogo");
        }

        if (!string.IsNullOrWhiteSpace(request.NameLogoBase64))
        {
            nameLogo = DecodeImage(request.NameLogoBase64);
            if (nameLogo is null)
                return Fail("inst.badLogo");
        }

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.SchoolInfo
SET SchoolName = @SchoolName,
    Institution_Dialog = @Institution_Dialog,
    Established = @Established,
    Principal = @Principal,
    AcadamicStaff = @AcadamicStaff,
    Students = @Students,
    Address = @Address,
    City = @City,
    State = @State,
    LocalArea = @LocalArea,
    PostalCode = @PostalCode,
    Phone = @Phone,
    Email = @Email,
    Website = @Website
WHERE SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@SchoolName", name);
        cmd.Parameters.AddWithValue("@Institution_Dialog", (object?)NullIfEmpty(request.InstitutionDialog) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Established", (object?)NullIfEmpty(request.Established) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Principal", (object?)NullIfEmpty(request.Principal) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AcadamicStaff", (object?)NullIfEmpty(request.AcadamicStaff) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Students", (object?)NullIfEmpty(request.Students) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", (object?)NullIfEmpty(request.City) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@State", (object?)NullIfEmpty(request.State) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LocalArea", (object?)NullIfEmpty(request.LocalArea) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PostalCode", (object?)NullIfEmpty(request.PostalCode) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Phone", (object?)NullIfEmpty(request.Phone) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)NullIfEmpty(request.Email) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Website", (object?)NullIfEmpty(request.Website) ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (logo is not null)
            await UpdateImageAsync(con, session.SchoolID, "SchoolLogo", logo, cancellationToken);

        await EnsureNameLogoColumnAsync(con, cancellationToken);
        if (request.ClearNameLogo)
            await UpdateImageAsync(con, session.SchoolID, "SchoolNameLogo", null, cancellationToken);
        else if (nameLogo is not null)
            await UpdateImageAsync(con, session.SchoolID, "SchoolNameLogo", nameLogo, cancellationToken);

        return new InstitutionResult
        {
            Succeeded = true,
            Data = await GetAsync(session, cancellationToken)
        };
    }

    private static async Task UpdateImageAsync(
        SqlConnection con, int schoolId, string column, byte[]? image, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand($"UPDATE dbo.SchoolInfo SET {column} = @Image WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var p = cmd.Parameters.Add("@Image", System.Data.SqlDbType.VarBinary);
        p.Value = image is null ? DBNull.Value : image;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureNameLogoColumnAsync(SqlConnection con, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF COL_LENGTH(N'dbo.SchoolInfo', N'SchoolNameLogo') IS NULL
    ALTER TABLE dbo.SchoolInfo ADD SchoolNameLogo VARBINARY(MAX)
""", con);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string?> ReadNameLogoAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        await using var exists = new SqlCommand(
            "SELECT CASE WHEN COL_LENGTH(N'dbo.SchoolInfo', N'SchoolNameLogo') IS NULL THEN 0 ELSE 1 END", con);
        if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) == 0)
            return null;

        await using var cmd = new SqlCommand(
            "SELECT SchoolNameLogo FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return ToDataUrl(value as byte[]);
    }

    private static byte[]? DecodeImage(string raw)
    {
        var comma = raw.IndexOf(',');
        var payload = comma >= 0 && raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? raw[(comma + 1)..]
            : raw;
        try
        {
            var bytes = Convert.FromBase64String(payload);
            if (bytes.Length == 0 || bytes.Length > MaxLogoBytes)
                return null;
            return bytes;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? ToDataUrl(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    private static string? ReadString(SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull ? null : value?.ToString();
    }

    private static string? NullIfEmpty(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static InstitutionResult Fail(string error) => new() { Succeeded = false, Error = error };
}
