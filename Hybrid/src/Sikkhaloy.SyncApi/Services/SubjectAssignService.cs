using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Subjects;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SubjectAssignService
{
    private readonly EduConnectionFactory _connections;

    public SubjectAssignService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<ClassSubjectRowDto>> ListAsync(
        SessionSnapshot session, int classId, int groupId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT s.SubjectID, s.SubjectName, g.SubjectType
FROM dbo.Subject AS s
LEFT JOIN dbo.SubjectForGroup AS g
    ON g.SubjectID = s.SubjectID
   AND g.SchoolID = s.SchoolID
   AND g.ClassID = @ClassID
   AND g.SubjectGroupID = @SubjectGroupID
WHERE s.SchoolID = @SchoolID
ORDER BY ISNULL(s.SN, 9999), s.SubjectName
""";

        var items = new List<ClassSubjectRowDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SubjectGroupID", groupId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var type = reader["SubjectType"] is DBNull ? null : reader["SubjectType"]?.ToString();
            items.Add(new ClassSubjectRowDto
            {
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                SubjectName = reader["SubjectName"]?.ToString() ?? "",
                Selected = type is not null,
                SubjectType = string.IsNullOrWhiteSpace(type) ? null : type.Trim()
            });
        }

        return items;
    }

    public async Task<SubjectAssignResult> SaveAsync(
        SessionSnapshot session, SaveClassSubjectsRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ClassID <= 0)
            return Fail("asgn.needClass");

        var items = request.Items ?? [];
        if (items.Count == 0)
            return Fail("asgn.needSubject");

        foreach (var item in items)
        {
            if (item.SubjectID <= 0)
                return Fail("asgn.needSubject");
            if (!IsType(item.SubjectType))
                return Fail("asgn.needType");
        }

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsClassAsync(con, session.SchoolID, request.ClassID, cancellationToken))
            return Fail("asgn.needClass");

        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await DeleteGroupAsync(con, tx, session.SchoolID, request.ClassID, request.SubjectGroupID, cancellationToken);
            foreach (var item in items)
            {
                await using var cmd = new SqlCommand("""
INSERT INTO dbo.SubjectForGroup (SchoolID, RegistrationID, ClassID, SubjectID, SubjectGroupID, SubjectType, Date)
VALUES (@SchoolID, @RegistrationID, @ClassID, @SubjectID, @SubjectGroupID, @SubjectType, GETDATE())
""", con, tx);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@SubjectID", item.SubjectID);
                cmd.Parameters.AddWithValue("@SubjectGroupID", request.SubjectGroupID);
                cmd.Parameters.AddWithValue("@SubjectType", item.SubjectType.Trim());
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new SubjectAssignResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    public async Task<SubjectAssignResult> ClearAsync(
        SessionSnapshot session, int classId, int groupId, CancellationToken cancellationToken)
    {
        if (classId <= 0)
            return Fail("asgn.needClass");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsClassAsync(con, session.SchoolID, classId, cancellationToken))
            return Fail("asgn.needClass");

        await DeleteGroupAsync(con, null, session.SchoolID, classId, groupId, cancellationToken);
        return new SubjectAssignResult { Succeeded = true };
    }

    private static async Task DeleteGroupAsync(
        SqlConnection con, SqlTransaction? tx, int schoolId, int classId, int groupId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.SubjectForGroup
WHERE SchoolID = @SchoolID AND ClassID = @ClassID AND SubjectGroupID = @SubjectGroupID
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SubjectGroupID", groupId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> OwnsClassAsync(
        SqlConnection con, int schoolId, int classId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "SELECT 1 FROM dbo.CreateClass WHERE ClassID = @ClassID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static bool IsType(string? type) =>
        string.Equals(type, "Compulsory", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "Optional", StringComparison.OrdinalIgnoreCase);

    private static SubjectAssignResult Fail(string error) => new() { Succeeded = false, Error = error };
}
