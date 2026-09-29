using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Institution;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class InstitutionService
{
    public async Task<WebsiteApiKeyDto> GetWebsiteApiKeyAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await EnsureWebsiteApiKeyTableAsync(cancellationToken);
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        var schoolName = "";
        await using (var nameCmd = new SqlCommand("SELECT TOP 1 SchoolName FROM dbo.SchoolInfo WHERE SchoolID = @id", con))
        {
            nameCmd.Parameters.AddWithValue("@id", session.SchoolID);
            var raw = await nameCmd.ExecuteScalarAsync(cancellationToken);
            schoolName = raw?.ToString()?.Trim() ?? "";
        }

        await using var cmd = new SqlCommand("""
SELECT ApiKey, AllowedHosts, UpdatedAt
FROM dbo.SchoolWebsiteApiKey
WHERE SchoolID = @id
""", con);
        cmd.Parameters.AddWithValue("@id", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new WebsiteApiKeyDto
            {
                SchoolID = session.SchoolID,
                SchoolName = schoolName,
                HasKey = false
            };
        }

        var key = reader["ApiKey"]?.ToString() ?? "";
        return new WebsiteApiKeyDto
        {
            SchoolID = session.SchoolID,
            SchoolName = schoolName,
            HasKey = !string.IsNullOrWhiteSpace(key),
            ApiKey = key,
            AllowedHosts = reader["AllowedHosts"]?.ToString(),
            UpdatedAtUtc = reader["UpdatedAt"] is DateTime dt ? dt.ToUniversalTime().ToString("O") : null
        };
    }

    public async Task<WebsiteApiKeyResult> SaveWebsiteApiKeyAsync(
        SessionSnapshot session, WebsiteApiKeySaveRequest? request, CancellationToken cancellationToken)
    {
        request ??= new WebsiteApiKeySaveRequest();
        await EnsureWebsiteApiKeyTableAsync(cancellationToken);
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        string? existing = null;
        await using (var find = new SqlCommand("SELECT ApiKey FROM dbo.SchoolWebsiteApiKey WHERE SchoolID = @id", con))
        {
            find.Parameters.AddWithValue("@id", session.SchoolID);
            existing = (await find.ExecuteScalarAsync(cancellationToken))?.ToString();
        }

        var key = existing;
        if (request.Regenerate || string.IsNullOrWhiteSpace(key))
            key = CreateWebsiteApiKey();

        var hosts = string.IsNullOrWhiteSpace(request.AllowedHosts) ? null : request.AllowedHosts.Trim();
        await using var upsert = new SqlCommand("""
MERGE dbo.SchoolWebsiteApiKey AS t
USING (SELECT @id AS SchoolID) AS s ON t.SchoolID = s.SchoolID
WHEN MATCHED THEN UPDATE SET
    ApiKey = @key,
    AllowedHosts = @hosts,
    UpdatedAt = SYSUTCDATETIME(),
    UpdatedBy = @by
WHEN NOT MATCHED THEN INSERT (SchoolID, ApiKey, AllowedHosts, UpdatedAt, UpdatedBy)
VALUES (@id, @key, @hosts, SYSUTCDATETIME(), @by);
""", con);
        upsert.Parameters.AddWithValue("@id", session.SchoolID);
        upsert.Parameters.AddWithValue("@key", key!);
        upsert.Parameters.AddWithValue("@hosts", (object?)hosts ?? DBNull.Value);
        upsert.Parameters.AddWithValue("@by", session.UserName ?? "");
        await upsert.ExecuteNonQueryAsync(cancellationToken);

        var data = await GetWebsiteApiKeyAsync(session, cancellationToken);
        return new WebsiteApiKeyResult { Succeeded = true, Data = data };
    }

    public async Task<string?> GetStoredWebsiteApiKeyAsync(int schoolId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0) return null;
        await EnsureWebsiteApiKeyTableAsync(cancellationToken);
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("SELECT ApiKey FROM dbo.SchoolWebsiteApiKey WHERE SchoolID = @id", con);
        cmd.Parameters.AddWithValue("@id", schoolId);
        var raw = await cmd.ExecuteScalarAsync(cancellationToken);
        var key = raw?.ToString()?.Trim();
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }

    private async Task EnsureWebsiteApiKeyTableAsync(CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.SchoolWebsiteApiKey', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchoolWebsiteApiKey
    (
        SchoolID INT NOT NULL CONSTRAINT PK_SchoolWebsiteApiKey PRIMARY KEY,
        ApiKey NVARCHAR(128) NOT NULL,
        AllowedHosts NVARCHAR(500) NULL,
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_SchoolWebsiteApiKey_UpdatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy NVARCHAR(100) NULL
    );
END
""", con);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string CreateWebsiteApiKey()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
