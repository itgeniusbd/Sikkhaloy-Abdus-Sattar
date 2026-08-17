using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Subjects;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SubjectService
{
    private readonly EduConnectionFactory _connections;

    public SubjectService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<SubjectDto>> ListAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT SubjectID, SubjectName, SN
FROM dbo.Subject
WHERE SchoolID = @SchoolID
ORDER BY ISNULL(SN, 9999), SubjectName
""";

        var items = new List<SubjectDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SubjectDto
            {
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                SubjectName = reader["SubjectName"]?.ToString() ?? "",
                SN = reader["SN"] is DBNull ? null : Convert.ToInt32(reader["SN"])
            });
        }

        return items;
    }

    public async Task<SubjectResult> CreateAsync(
        SessionSnapshot session, SaveSubjectRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.SubjectName ?? "").Trim();
        if (name.Length == 0)
            return Fail("subj.required");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await NameExistsAsync(con, session.SchoolID, name, null, cancellationToken))
            return Fail("subj.exists");

        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Subject (SubjectName, Date, RegistrationID, SchoolID)
VALUES (@SubjectName, GETDATE(), @RegistrationID, @SchoolID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SubjectName", name);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var id = await cmd.ExecuteScalarAsync(cancellationToken);
        return new SubjectResult
        {
            Succeeded = true,
            SubjectID = id is null or DBNull ? 0 : Convert.ToInt32(id)
        };
    }

    public async Task<SubjectResult> UpdateAsync(
        SessionSnapshot session, int subjectId, SaveSubjectRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.SubjectName ?? "").Trim();
        if (subjectId <= 0)
            return Fail("subj.needSubject");
        if (name.Length == 0)
            return Fail("subj.required");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsAsync(con, session.SchoolID, subjectId, cancellationToken))
            return Fail("subj.needSubject");
        if (await NameExistsAsync(con, session.SchoolID, name, subjectId, cancellationToken))
            return Fail("subj.exists");

        await using var cmd = new SqlCommand("""
UPDATE dbo.Subject
SET SubjectName = @SubjectName
WHERE SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SubjectName", name);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return new SubjectResult { Succeeded = true, SubjectID = subjectId };
    }

    public async Task<SubjectResult> DeleteAsync(SessionSnapshot session, int subjectId, CancellationToken cancellationToken)
    {
        if (subjectId <= 0)
            return Fail("subj.needSubject");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsAsync(con, session.SchoolID, subjectId, cancellationToken))
            return Fail("subj.needSubject");
        if (await IsInUseAsync(con, session.SchoolID, subjectId, cancellationToken))
            return Fail("subj.inUse");

        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Subject
WHERE SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return new SubjectResult { Succeeded = true, SubjectID = subjectId };
    }

    public async Task<SubjectResult> SaveSerialsAsync(
        SessionSnapshot session, SaveSubjectSerialsRequest? request, CancellationToken cancellationToken)
    {
        var items = request?.Items ?? [];
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var item in items)
            {
                if (item.SubjectID <= 0)
                    continue;
                await using var cmd = new SqlCommand("""
UPDATE dbo.Subject
SET SN = @SN
WHERE SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con, tx);
                cmd.Parameters.AddWithValue("@SubjectID", item.SubjectID);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                cmd.Parameters.AddWithValue("@SN", item.SN.HasValue ? item.SN.Value : DBNull.Value);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new SubjectResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    private static SubjectResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<bool> OwnsAsync(
        SqlConnection con, int schoolId, int subjectId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT 1 FROM dbo.Subject WHERE SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static async Task<bool> NameExistsAsync(
        SqlConnection con, int schoolId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        var sql = exceptId is null
            ? "SELECT 1 FROM dbo.Subject WHERE SchoolID = @SchoolID AND SubjectName = @SubjectName"
            : "SELECT 1 FROM dbo.Subject WHERE SchoolID = @SchoolID AND SubjectName = @SubjectName AND SubjectID <> @SubjectID";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@SubjectName", name);
        if (exceptId is not null)
            cmd.Parameters.AddWithValue("@SubjectID", exceptId.Value);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static async Task<bool> IsInUseAsync(
        SqlConnection con, int schoolId, int subjectId, CancellationToken cancellationToken)
    {
        await using var exists = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(N'dbo.StudentRecord', N'U') IS NULL THEN 0 ELSE 1 END", con);
        var hasTable = Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) == 1;
        if (!hasTable)
            return false;

        await using var cmd = new SqlCommand("""
SELECT TOP 1 1
FROM dbo.StudentRecord
WHERE SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }
}
