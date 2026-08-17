using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Classes;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;
using System.Text.Json;

namespace Sikkhaloy.SyncApi.Services;

public sealed class ClassStructureService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly EduConnectionFactory _connections;

    public ClassStructureService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<ClassStructureDto> GetAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        var result = new ClassStructureDto();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        await using (var cmd = new SqlCommand("""
            SELECT ClassID, Class, ISNULL(SN, ClassID) AS SortOrder
            FROM dbo.CreateClass
            WHERE SchoolID = @SchoolID
            ORDER BY SN, ClassID
            """, con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Classes.Add(new SchoolClassDto
                {
                    ClassID = Convert.ToInt32(reader["ClassID"]),
                    Name = reader["Class"]?.ToString() ?? "",
                    SortOrder = Convert.ToInt32(reader["SortOrder"]),
                    SyncStatus = SyncStatus.Synced
                });
            }
        }

        await ReadPartsAsync(con, session.SchoolID, """
            SELECT SubjectGroupID AS ServerId, ClassID, SubjectGroup AS Name
            FROM dbo.CreateSubjectGroup
            WHERE SchoolID = @SchoolID
            ORDER BY SubjectGroupID
            """, result.Groups, cancellationToken);

        await ReadPartsAsync(con, session.SchoolID, """
            SELECT SectionID AS ServerId, ClassID, Section AS Name
            FROM dbo.CreateSection
            WHERE SchoolID = @SchoolID
            ORDER BY SectionID
            """, result.Sections, cancellationToken);

        await ReadPartsAsync(con, session.SchoolID, """
            SELECT ShiftID AS ServerId, ClassID, Shift AS Name
            FROM dbo.CreateShift
            WHERE SchoolID = @SchoolID
            ORDER BY ShiftID
            """, result.Shifts, cancellationToken);

        await using (var cmd = new SqlCommand("""
            SELECT j.JoinID, j.ClassID,
                   ISNULL(j.SubjectGroupID, 0) AS SubjectGroupID,
                   ISNULL(j.SectionID, 0) AS SectionID,
                   ISNULL(j.ShiftID, 0) AS ShiftID,
                   ISNULL(g.SubjectGroup, N'') AS GroupName,
                   ISNULL(s.Section, N'') AS SectionName,
                   ISNULL(sh.Shift, N'') AS ShiftName
            FROM dbo.[Join] AS j
            LEFT JOIN dbo.CreateSubjectGroup AS g ON g.SubjectGroupID = j.SubjectGroupID
            LEFT JOIN dbo.CreateSection AS s ON s.SectionID = j.SectionID
            LEFT JOIN dbo.CreateShift AS sh ON sh.ShiftID = j.ShiftID
            WHERE j.SchoolID = @SchoolID
               OR j.ClassID IN (SELECT ClassID FROM dbo.CreateClass WHERE SchoolID = @SchoolID)
            ORDER BY j.JoinID
            """, con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Joins.Add(new ClassJoinDto
                {
                    JoinID = Convert.ToInt32(reader["JoinID"]),
                    ClassID = Convert.ToInt32(reader["ClassID"]),
                    SubjectGroupID = Convert.ToInt32(reader["SubjectGroupID"]),
                    SectionID = Convert.ToInt32(reader["SectionID"]),
                    ShiftID = Convert.ToInt32(reader["ShiftID"]),
                    GroupName = reader["GroupName"]?.ToString() ?? "",
                    SectionName = reader["SectionName"]?.ToString() ?? "",
                    ShiftName = reader["ShiftName"]?.ToString() ?? "",
                    SyncStatus = SyncStatus.Synced
                });
            }
        }

        return result;
    }

    public async Task<PushItemResult> PushOneAsync(
        SqlConnection con,
        SessionSnapshot session,
        SyncChangeDto change,
        CancellationToken cancellationToken)
    {
        if (change.EntityType == EntityTypes.ClassJoin)
        {
            var join = JsonSerializer.Deserialize<ClassJoinDto>(change.PayloadJson, JsonOptions)
                       ?? throw new InvalidOperationException("Join payload missing.");
            join.LocalId = change.LocalId;
            var joinId = change.Operation == SyncOperation.Delete
                ? await DeleteJoinAsync(con, session, join, cancellationToken)
                : await InsertJoinAsync(con, session, join, cancellationToken);
            return new PushItemResult
            {
                LocalId = change.LocalId,
                Succeeded = true,
                ServerId = joinId
            };
        }

        var dto = JsonSerializer.Deserialize<ClassMutationDto>(change.PayloadJson, JsonOptions)
                  ?? throw new InvalidOperationException("Class structure payload missing.");
        dto.LocalId = change.LocalId;

        var serverId = change.EntityType switch
        {
            EntityTypes.Class when change.Operation == SyncOperation.Delete =>
                await DeleteClassAsync(con, session, dto, cancellationToken),
            EntityTypes.Class =>
                await UpsertClassAsync(con, session, dto, cancellationToken),
            EntityTypes.ClassGroup when change.Operation == SyncOperation.Delete =>
                await DeletePartAsync(con, session, dto, "group", cancellationToken),
            EntityTypes.ClassGroup =>
                await UpsertPartAsync(con, session, dto, "group", cancellationToken),
            EntityTypes.ClassSection when change.Operation == SyncOperation.Delete =>
                await DeletePartAsync(con, session, dto, "section", cancellationToken),
            EntityTypes.ClassSection =>
                await UpsertPartAsync(con, session, dto, "section", cancellationToken),
            EntityTypes.ClassShift when change.Operation == SyncOperation.Delete =>
                await DeletePartAsync(con, session, dto, "shift", cancellationToken),
            EntityTypes.ClassShift =>
                await UpsertPartAsync(con, session, dto, "shift", cancellationToken),
            _ => throw new InvalidOperationException($"Unknown class entity '{change.EntityType}'.")
        };

        return new PushItemResult
        {
            LocalId = change.LocalId,
            Succeeded = true,
            ServerId = serverId
        };
    }

    private static async Task ReadPartsAsync(
        SqlConnection con,
        int schoolId,
        string sql,
        List<ClassPartDto> target,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            target.Add(new ClassPartDto
            {
                ServerId = Convert.ToInt32(reader["ServerId"]),
                ClassID = Convert.ToInt32(reader["ClassID"]),
                Name = reader["Name"]?.ToString() ?? "",
                SyncStatus = SyncStatus.Synced
            });
        }
    }

    private static async Task<int> UpsertClassAsync(
        SqlConnection con,
        SessionSnapshot session,
        ClassMutationDto dto,
        CancellationToken cancellationToken)
    {
        var name = (dto.Name ?? "").Trim();
        if (name.Length == 0)
            throw new InvalidOperationException("Enter Class");

        var existingId = dto.ServerId > 0 ? dto.ServerId : (dto.ClassID > 0 ? dto.ClassID : 0);
        if (existingId > 0)
        {
            await using var update = new SqlCommand("""
                IF NOT EXISTS (
                    SELECT 1 FROM dbo.CreateClass
                    WHERE SchoolID = @SchoolID AND Class = @Class AND ClassID <> @ClassID)
                UPDATE dbo.CreateClass
                SET Class = @Class, SN = @SN
                WHERE ClassID = @ClassID AND SchoolID = @SchoolID;
                """, con);
            update.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            update.Parameters.AddWithValue("@Class", name);
            update.Parameters.AddWithValue("@ClassID", existingId);
            update.Parameters.AddWithValue("@SN", dto.SortOrder);
            await update.ExecuteNonQueryAsync(cancellationToken);
            return existingId;
        }

        await using var insert = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.CreateClass WHERE SchoolID = @SchoolID AND Class = @Class)
            BEGIN
                INSERT INTO dbo.CreateClass (SchoolID, RegistrationID, Class, SN)
                VALUES (@SchoolID, @RegistrationID, @Class, @SN);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            END
            ELSE
                SELECT ClassID FROM dbo.CreateClass WHERE SchoolID = @SchoolID AND Class = @Class;
            """, con);
        insert.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        insert.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        insert.Parameters.AddWithValue("@Class", name);
        insert.Parameters.AddWithValue("@SN", dto.SortOrder);
        var idObj = await insert.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(idObj);
    }

    private static async Task<int> DeleteClassAsync(
        SqlConnection con,
        SessionSnapshot session,
        ClassMutationDto dto,
        CancellationToken cancellationToken)
    {
        var id = dto.ServerId > 0 ? dto.ServerId : dto.ClassID;
        if (id <= 0)
            return 0;

        try
        {
            await using var cmd = new SqlCommand(
                "DELETE FROM dbo.CreateClass WHERE ClassID = @ClassID AND SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@ClassID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return id;
        }
        catch (SqlException)
        {
            throw new InvalidOperationException("This Class already used!");
        }
    }

    private static async Task<int> UpsertPartAsync(
        SqlConnection con,
        SessionSnapshot session,
        ClassMutationDto dto,
        string kind,
        CancellationToken cancellationToken)
    {
        var name = (dto.Name ?? "").Trim();
        if (name.Length == 0)
            throw new InvalidOperationException(kind == "group" ? "Enter Group" : kind == "section" ? "Enter Section" : "Enter Shift");
        if (dto.ClassID <= 0)
            throw new InvalidOperationException("Select Class");

        if (dto.ServerId > 0)
        {
            var (table, idCol, nameCol) = Table(kind);
            await using var update = new SqlCommand(
                $"UPDATE dbo.[{table}] SET [{nameCol}] = @Name WHERE [{idCol}] = @Id AND SchoolID = @SchoolID", con);
            update.Parameters.AddWithValue("@Name", name);
            update.Parameters.AddWithValue("@Id", dto.ServerId);
            update.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await update.ExecuteNonQueryAsync(cancellationToken);
            return dto.ServerId;
        }

        var insertSql = kind switch
        {
            "group" => """
                INSERT INTO dbo.CreateSubjectGroup (RegistrationID, SchoolID, ClassID, SubjectGroup)
                VALUES (@RegistrationID, @SchoolID, @ClassID, @Name);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """,
            "section" => """
                INSERT INTO dbo.CreateSection (SchoolID, RegistrationID, ClassID, Section)
                VALUES (@SchoolID, @RegistrationID, @ClassID, @Name);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """,
            _ => """
                INSERT INTO dbo.CreateShift (RegistrationID, SchoolID, ClassID, Shift)
                VALUES (@RegistrationID, @SchoolID, @ClassID, @Name);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """
        };

        await using var insert = new SqlCommand(insertSql, con);
        insert.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        insert.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        insert.Parameters.AddWithValue("@ClassID", dto.ClassID);
        insert.Parameters.AddWithValue("@Name", name);
        var newId = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));

        var sectionId = kind == "section" ? newId : 0;
        var groupId = kind == "group" ? newId : 0;
        var shiftId = kind == "shift" ? newId : 0;
        await using var join = new SqlCommand("""
            INSERT INTO dbo.[Join] (RegistrationID, SchoolID, ClassID, SectionID, SubjectGroupID, ShiftID)
            VALUES (@RegistrationID, @SchoolID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID)
            """, con);
        join.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        join.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        join.Parameters.AddWithValue("@ClassID", dto.ClassID);
        join.Parameters.AddWithValue("@SectionID", sectionId);
        join.Parameters.AddWithValue("@SubjectGroupID", groupId);
        join.Parameters.AddWithValue("@ShiftID", shiftId);
        await join.ExecuteNonQueryAsync(cancellationToken);
        return newId;
    }

    private static async Task<int> DeletePartAsync(
        SqlConnection con,
        SessionSnapshot session,
        ClassMutationDto dto,
        string kind,
        CancellationToken cancellationToken)
    {
        if (dto.ServerId <= 0)
            return 0;

        var sql = kind switch
        {
            "group" => """
                UPDATE dbo.StudentsClass SET SubjectGroupID = 0 WHERE SubjectGroupID = @Id;
                DELETE FROM dbo.CreateSubjectGroup WHERE SubjectGroupID = @Id AND SchoolID = @SchoolID;
                DELETE FROM dbo.[Join] WHERE SubjectGroupID = @Id;
                IF OBJECT_ID(N'dbo.SubjectForGroup', N'U') IS NOT NULL
                    DELETE FROM dbo.SubjectForGroup WHERE ClassID = @ClassID AND SubjectGroupID = @Id;
                """,
            "section" => """
                UPDATE dbo.StudentsClass SET SectionID = 0 WHERE SectionID = @Id;
                DELETE FROM dbo.CreateSection WHERE SectionID = @Id AND SchoolID = @SchoolID;
                DELETE FROM dbo.[Join] WHERE SectionID = @Id;
                """,
            _ => """
                UPDATE dbo.StudentsClass SET ShiftID = 0 WHERE ShiftID = @Id;
                DELETE FROM dbo.CreateShift WHERE ShiftID = @Id AND SchoolID = @SchoolID;
                DELETE FROM dbo.[Join] WHERE ShiftID = @Id;
                """
        };

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Id", dto.ServerId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ClassID", dto.ClassID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return dto.ServerId;
    }

    private static async Task<int> InsertJoinAsync(
        SqlConnection con,
        SessionSnapshot session,
        ClassJoinDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.ClassID <= 0)
            throw new InvalidOperationException("Select class");

        await using (var find = new SqlCommand(JoinExistsSql(dto), con))
        {
            AddJoinParams(find, session, dto);
            var existing = await find.ExecuteScalarAsync(cancellationToken);
            if (existing is not null && existing is not DBNull)
                return Convert.ToInt32(existing);
        }

        await using var insert = new SqlCommand("""
            INSERT INTO dbo.[Join] (RegistrationID, SchoolID, ClassID, SectionID, SubjectGroupID, ShiftID)
            VALUES (@RegistrationID, @SchoolID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, con);
        AddJoinParams(insert, session, dto);
        return Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task<int> DeleteJoinAsync(
        SqlConnection con,
        SessionSnapshot session,
        ClassJoinDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.JoinID <= 0)
            return 0;

        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.[Join] WHERE JoinID = @JoinID AND (SchoolID = @SchoolID OR ClassID = @ClassID)", con);
        cmd.Parameters.AddWithValue("@JoinID", dto.JoinID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ClassID", dto.ClassID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return dto.JoinID;
    }

    private static string JoinExistsSql(ClassJoinDto dto)
    {
        if (dto.SubjectGroupID <= 0)
            return "SELECT TOP 1 JoinID FROM dbo.[Join] WHERE ClassID = @ClassID AND ShiftID = @ShiftID AND SectionID = @SectionID";
        if (dto.SectionID <= 0)
            return "SELECT TOP 1 JoinID FROM dbo.[Join] WHERE ClassID = @ClassID AND SubjectGroupID = @SubjectGroupID AND ShiftID = @ShiftID";
        if (dto.ShiftID <= 0)
            return "SELECT TOP 1 JoinID FROM dbo.[Join] WHERE ClassID = @ClassID AND SubjectGroupID = @SubjectGroupID AND SectionID = @SectionID";
        return "SELECT TOP 1 JoinID FROM dbo.[Join] WHERE ClassID = @ClassID AND SubjectGroupID = @SubjectGroupID AND SectionID = @SectionID AND ShiftID = @ShiftID";
    }

    private static void AddJoinParams(SqlCommand cmd, SessionSnapshot session, ClassJoinDto dto)
    {
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ClassID", dto.ClassID);
        cmd.Parameters.AddWithValue("@SectionID", dto.SectionID);
        cmd.Parameters.AddWithValue("@SubjectGroupID", dto.SubjectGroupID);
        cmd.Parameters.AddWithValue("@ShiftID", dto.ShiftID);
    }

    private static (string Table, string IdCol, string NameCol) Table(string kind) => kind switch
    {
        "group" => ("CreateSubjectGroup", "SubjectGroupID", "SubjectGroup"),
        "section" => ("CreateSection", "SectionID", "Section"),
        _ => ("CreateShift", "ShiftID", "Shift")
    };
}
