using System.Security.Cryptography;
using System.Text;

namespace Sikkhaloy.SyncApi.Services;

/// <summary>
/// Matches System.Web.Security.SqlMembershipProvider.EncodePassword
/// (passwordFormat=Hashed, default hashAlgorithmType=SHA1).
/// </summary>
public static class MembershipPasswordVerifier
{
    public static bool Verify(string password, string storedHash, string storedSalt, int passwordFormat)
    {
        if (passwordFormat == 0)
            return string.Equals(password, storedHash, StringComparison.Ordinal);

        if (passwordFormat != 1)
            return false;

        byte[] expected;
        byte[] saltBytes;
        try
        {
            expected = Convert.FromBase64String(storedHash);
            saltBytes = Convert.FromBase64String(storedSalt);
        }
        catch (FormatException)
        {
            return false;
        }

        var passwordBytes = Encoding.Unicode.GetBytes(password);

        // Default .NET 4 membership: SHA1(salt + unicode password)
        if (Matches(expected, Sha1SaltPlusPassword(saltBytes, passwordBytes)))
            return true;

        // Fallback used when hashAlgorithmType is HMACSHA1
        if (Matches(expected, HmacSha1(saltBytes, passwordBytes)))
            return true;

        return false;
    }

    private static byte[] Sha1SaltPlusPassword(byte[] saltBytes, byte[] passwordBytes)
    {
        var combined = new byte[saltBytes.Length + passwordBytes.Length];
        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);
        return SHA1.HashData(combined);
    }

    private static byte[] HmacSha1(byte[] saltBytes, byte[] passwordBytes)
    {
        using var hmac = new HMACSHA1(BuildKeyedHashKey(saltBytes, 64));
        return hmac.ComputeHash(passwordBytes);
    }

    private static byte[] BuildKeyedHashKey(byte[] saltBytes, int keyLength)
    {
        if (saltBytes.Length == keyLength)
            return saltBytes;

        if (saltBytes.Length > keyLength)
        {
            var truncated = new byte[keyLength];
            Buffer.BlockCopy(saltBytes, 0, truncated, 0, keyLength);
            return truncated;
        }

        var key = new byte[keyLength];
        var offset = 0;
        while (offset < keyLength)
        {
            var copy = Math.Min(saltBytes.Length, keyLength - offset);
            Buffer.BlockCopy(saltBytes, 0, key, offset, copy);
            offset += copy;
        }

        return key;
    }

    private static bool Matches(byte[] expected, byte[] actual)
    {
        if (expected.Length != actual.Length)
            return false;
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public static string NewSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    public static string Hash(string value, string saltBase64)
    {
        var saltBytes = Convert.FromBase64String(saltBase64);
        var valueBytes = Encoding.Unicode.GetBytes(value ?? "");
        return Convert.ToBase64String(Sha1SaltPlusPassword(saltBytes, valueBytes));
    }
}
