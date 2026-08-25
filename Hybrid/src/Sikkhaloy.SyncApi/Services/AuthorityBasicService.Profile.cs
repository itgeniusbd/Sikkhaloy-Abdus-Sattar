using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityBasicService
{
    public async Task<AuthProfileDto> GetProfileAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthProfileDto { Name = session.DisplayName };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT TOP 1 AuthorityID, Name, FatherName, Gender, Designation, City, Phone, Email, Address, DateofBirth, Image
FROM dbo.Authority_Info
WHERE RegistrationID = @RegistrationID
""", con);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return dto;

        dto.AuthorityID = I(reader["AuthorityID"]);
        var name = S(reader["Name"]).Trim();
        if (name.Length > 0)
            dto.Name = name;
        dto.FatherName = NullIfEmpty(S(reader["FatherName"]));
        dto.Gender = NullIfEmpty(S(reader["Gender"]));
        dto.Designation = NullIfEmpty(S(reader["Designation"]));
        dto.City = NullIfEmpty(S(reader["City"]));
        dto.Phone = NullIfEmpty(S(reader["Phone"]));
        dto.Email = NullIfEmpty(S(reader["Email"]));
        dto.Address = NullIfEmpty(S(reader["Address"]));
        dto.DateofBirth = Dt(reader["DateofBirth"]);
        if (reader["Image"] is byte[] bytes && bytes.Length > 0)
        {
            var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
            dto.PhotoDataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }
        return dto;
    }

    public async Task<ProfileResult> SaveProfileAsync(
        SessionSnapshot session, AuthProfileDto? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthProfileDto();
        var name = (request.Name ?? "").Trim();
        if (name.Length == 0)
            return new ProfileResult { Error = "profile.needName" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        var id = request.AuthorityID;
        if (id <= 0)
        {
            await using var find = new SqlCommand(
                "SELECT TOP 1 AuthorityID FROM dbo.Authority_Info WHERE RegistrationID = @RegistrationID", con);
            find.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            id = I(await find.ExecuteScalarAsync(ct));
        }
        if (id <= 0)
            return new ProfileResult { Error = "al.noProfile" };

        await using (var cmd = new SqlCommand("""
UPDATE dbo.Authority_Info
SET Name = @Name, FatherName = @FatherName, Gender = @Gender, Designation = @Designation,
    City = @City, Phone = @Phone, Email = @Email, Address = @Address, DateofBirth = @Dob
WHERE AuthorityID = @Id AND RegistrationID = @RegistrationID
""", con))
        {
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@FatherName", (object?)NullIfEmpty(request.FatherName) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (object?)NullIfEmpty(request.Gender) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Designation", (object?)NullIfEmpty(request.Designation) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object?)NullIfEmpty(request.City) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)NullIfEmpty(request.Phone) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)NullIfEmpty(request.Email) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Dob", request.DateofBirth.HasValue ? request.DateofBirth.Value.Date : DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            if (await cmd.ExecuteNonQueryAsync(ct) <= 0)
                return new ProfileResult { Error = "al.noProfile" };
        }

        if (!string.IsNullOrWhiteSpace(request.PhotoDataUrl))
        {
            var bytes = DecodeImage(request.PhotoDataUrl);
            if (bytes is { Length: > 0 })
            {
                await using var img = new SqlCommand("""
UPDATE dbo.Authority_Info SET Image = @Image
WHERE AuthorityID = @Id AND RegistrationID = @RegistrationID
""", con);
                var p = img.Parameters.Add("@Image", System.Data.SqlDbType.VarBinary, -1);
                p.Value = bytes;
                img.Parameters.AddWithValue("@Id", id);
                img.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                await img.ExecuteNonQueryAsync(ct);
            }
        }

        return new ProfileResult
        {
            Succeeded = true,
            DisplayName = name,
            PhotoDataUrl = request.PhotoDataUrl
        };
    }
}
