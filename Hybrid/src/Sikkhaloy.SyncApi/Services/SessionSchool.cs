using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

internal static class SessionSchool
{
    public static async Task<string> ResolveNameAsync(SessionSnapshot session, SqlConnection con, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(session.SchoolName))
            return session.SchoolName.Trim();

        await using var cmd = new SqlCommand(
            "SELECT LTRIM(RTRIM(ISNULL(SchoolName, N''))) FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var value = await cmd.ExecuteScalarAsync(ct);
        var name = value as string ?? "";
        if (!string.IsNullOrWhiteSpace(name))
            session.SchoolName = name;
        return name;
    }
}
