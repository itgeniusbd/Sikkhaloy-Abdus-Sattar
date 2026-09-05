using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.LocalData.Auth;

public sealed class OfflineAuthService
{
    private readonly IDbContextFactory<LocalDbContext> _dbFactory;

    public OfflineAuthService(IDbContextFactory<LocalDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task CacheOnlineLoginAsync(LoginResponse response, string password, CancellationToken cancellationToken = default)
    {
        if (response.Session is null || string.IsNullOrWhiteSpace(response.AccessToken))
            return;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var session = new CachedSession
        {
            UserName = response.Session.UserName,
            Role = response.Session.Role,
            SchoolID = response.Session.SchoolID,
            SchoolName = response.Session.SchoolName,
            RegistrationID = response.Session.RegistrationID,
            EducationYearID = response.Session.EducationYearID,
            DeviceId = response.Session.DeviceId,
            DisplayName = string.IsNullOrWhiteSpace(response.Session.DisplayName)
                ? response.Session.UserName
                : response.Session.DisplayName,
            StudentID = response.Session.StudentID,
            StudentClassID = response.Session.StudentClassID,
            ClassID = response.Session.ClassID,
            StudentCode = response.Session.StudentCode,
            ClassName = response.Session.ClassName,
            SectionName = response.Session.SectionName,
            AccessToken = response.AccessToken,
            TokenExpiresAt = response.ExpiresAt,
            VerifierSalt = salt,
            PasswordVerifier = HashPassword(password, salt),
            CachedUtc = DateTime.UtcNow
        };

        var existing = await db.Sessions.FindAsync(new object[] { session.UserName }, cancellationToken);
        if (existing is null)
            db.Sessions.Add(session);
        else
            db.Entry(existing).CurrentValues.SetValues(session);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCachedSessionAsync(LoginResponse response, CancellationToken cancellationToken = default)
    {
        if (response.Session is null)
            return;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.Sessions.FindAsync(new object[] { response.Session.UserName }, cancellationToken);
        if (existing is null)
            return;

        existing.EducationYearID = response.Session.EducationYearID;
        existing.SchoolName = response.Session.SchoolName;
        existing.DisplayName = string.IsNullOrWhiteSpace(response.Session.DisplayName)
            ? existing.DisplayName
            : response.Session.DisplayName;
        existing.StudentID = response.Session.StudentID;
        existing.StudentClassID = response.Session.StudentClassID;
        existing.ClassID = response.Session.ClassID;
        existing.StudentCode = response.Session.StudentCode;
        existing.ClassName = response.Session.ClassName;
        existing.SectionName = response.Session.SectionName;
        if (!string.IsNullOrWhiteSpace(response.AccessToken))
        {
            existing.AccessToken = response.AccessToken;
            existing.TokenExpiresAt = response.ExpiresAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCachedPasswordAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.Sessions.FindAsync(new object[] { userName }, cancellationToken);
        if (existing is null)
            return;

        existing.VerifierSalt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        existing.PasswordVerifier = HashPassword(password, existing.VerifierSalt);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<LoginResponse> TryOfflineLoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var cached = await db.Sessions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);

            if (cached is null)
            {
                return new LoginResponse
                {
                    Succeeded = false,
                    Error = "login.firstOnline"
                };
            }

            if (!FixedEquals(cached.PasswordVerifier, HashPassword(password, cached.VerifierSalt)))
            {
                return new LoginResponse
                {
                    Succeeded = false,
                    Error = "login.badPassword"
                };
            }

            return new LoginResponse
            {
                Succeeded = true,
                AccessToken = cached.AccessToken,
                ExpiresAt = cached.TokenExpiresAt,
                Session = new SessionSnapshot
                {
                    UserName = cached.UserName,
                    Role = cached.Role,
                    SchoolID = cached.SchoolID,
                    SchoolName = cached.SchoolName,
                    RegistrationID = cached.RegistrationID,
                    EducationYearID = cached.EducationYearID,
                    DeviceId = cached.DeviceId,
                    DisplayName = string.IsNullOrWhiteSpace(cached.DisplayName) ? cached.UserName : cached.DisplayName,
                    StudentID = cached.StudentID,
                    StudentClassID = cached.StudentClassID,
                    ClassID = cached.ClassID,
                    StudentCode = cached.StudentCode ?? "",
                    ClassName = cached.ClassName ?? "",
                    SectionName = cached.SectionName ?? ""
                }
            };
        }
        catch
        {
            return new LoginResponse
            {
                Succeeded = false,
                Error = "login.firstOnline"
            };
        }
    }

    public static string GetOrCreateDeviceId()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SIKKHALOY",
            "Hybrid");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "device-id.txt");
        if (File.Exists(path))
        {
            var existing = File.ReadAllText(path).Trim();
            if (!string.IsNullOrWhiteSpace(existing))
                return existing;
        }

        var id = Guid.NewGuid().ToString("N");
        File.WriteAllText(path, id);
        return id;
    }

    private static string HashPassword(string password, string salt)
    {
        var bytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Convert.FromBase64String(salt),
            100_000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(bytes);
    }

    private static bool FixedEquals(string left, string right)
    {
        var a = Convert.FromBase64String(left);
        var b = Convert.FromBase64String(right);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
