using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class StudentSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly EduConnectionFactory _connections;
    private readonly ClassStructureService _classStructure;

    public StudentSyncService(EduConnectionFactory connections, ClassStructureService classStructure)
    {
        _connections = connections;
        _classStructure = classStructure;
    }

    public async Task<PushResponse> PushAsync(SessionSnapshot session, PushRequest? request, CancellationToken cancellationToken)
    {
        var response = new PushResponse();
        var changes = request?.Changes ?? [];
        var requestMap = new Dictionary<Guid, int>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        foreach (var change in changes)
        {
            if (EntityTypes.IsClassStructure(change.EntityType))
            {
                try
                {
                    response.Results.Add(await _classStructure.PushOneAsync(con, session, change, cancellationToken));
                }
                catch (InvalidOperationException ex)
                {
                    response.Results.Add(new PushItemResult
                    {
                        LocalId = change.LocalId,
                        Succeeded = false,
                        IsConflict = true,
                        Error = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    response.Results.Add(new PushItemResult
                    {
                        LocalId = change.LocalId,
                        Succeeded = false,
                        Error = ex.Message
                    });
                }

                continue;
            }

            if (!string.Equals(change.EntityType, EntityTypes.Student, StringComparison.OrdinalIgnoreCase))
            {
                response.Results.Add(new PushItemResult
                {
                    LocalId = change.LocalId,
                    Succeeded = false,
                    Error = $"Entity '{change.EntityType}' is not implemented yet."
                });
                continue;
            }

            try
            {
                var dto = JsonSerializer.Deserialize<StudentDto>(change.PayloadJson, JsonOptions)
                          ?? throw new InvalidOperationException("Student payload missing.");
                dto.LocalId = change.LocalId;
                dto.SchoolID = session.SchoolID;
                dto.EducationYearID = session.EducationYearID;
                dto.RegistrationID = session.RegistrationID;
                if (dto.ServerId is null or <= 0 && change.ServerId is > 0)
                    dto.ServerId = change.ServerId;

                var serverId = await UpsertStudentAsync(con, session, dto, request?.DeviceId ?? "", change.Operation, requestMap, cancellationToken);
                requestMap[change.LocalId] = serverId;
                response.Results.Add(new PushItemResult
                {
                    LocalId = change.LocalId,
                    Succeeded = true,
                    ServerId = serverId
                });
            }
            catch (InvalidOperationException ex)
            {
                response.Results.Add(new PushItemResult
                {
                    LocalId = change.LocalId,
                    Succeeded = false,
                    IsConflict = true,
                    Error = ex.Message
                });
            }
            catch (Exception ex)
            {
                response.Results.Add(new PushItemResult
                {
                    LocalId = change.LocalId,
                    Succeeded = false,
                    Error = ex.Message
                });
            }
        }

        return response;
    }

    public async Task<PullResponse> PullAsync(SessionSnapshot session, long since, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        // Phase 0: page by StudentID. Hybrid_ChangeLog is written on push for later incremental pull.
        return await PullSnapshotAsync(con, session, since, cancellationToken);
    }

    public async Task<StudentIdCheckResult> CheckStudentIdAsync(
        SessionSnapshot session,
        string? studentCode,
        int? exceptServerId,
        CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        var lastId = await GetLastEntryIdAsync(con, session.SchoolID, cancellationToken);
        var code = studentCode?.Trim() ?? "";
        if (code.Length == 0)
            return new StudentIdCheckResult { LastEntryId = lastId };

        const string sql = @"
SELECT COUNT(*)
FROM dbo.Student
WHERE SchoolID = @SchoolID
  AND ID = @ID
  AND (@Except = 0 OR StudentID <> @Except)";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@Except", exceptServerId ?? 0);
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        var suggestions = await SuggestIdsAsync(con, session.SchoolID, code, cancellationToken);
        return new StudentIdCheckResult
        {
            Exists = count > 0,
            LastEntryId = lastId,
            Suggestions = suggestions
        };
    }

    private static async Task<string?> GetLastEntryIdAsync(SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT TOP 1 ID FROM dbo.Student WHERE SchoolID = @SchoolID ORDER BY StudentID DESC";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToString(result);
    }

    private static async Task<List<string>> SuggestIdsAsync(
        SqlConnection con, int schoolId, string prefix, CancellationToken cancellationToken)
    {
        if (prefix.Length == 0)
            return [];

        const string sql = @"
SELECT TOP 10 ID
FROM dbo.Student
WHERE SchoolID = @SchoolID AND ID LIKE @Prefix + '%'
ORDER BY ID";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Prefix", prefix);
        var ids = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(Convert.ToString(reader.GetValue(0)) ?? "");
        return ids;
    }

    private async Task<int> UpsertStudentAsync(
        SqlConnection con,
        SessionSnapshot session,
        StudentDto dto,
        string deviceId,
        SyncOperation operation,
        Dictionary<Guid, int> requestMap,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.StudentCode) || string.IsNullOrWhiteSpace(dto.StudentsName))
            throw new InvalidOperationException("Student ID and name are required.");

        var mappedId = await TryGetMappedServerIdAsync(con, dto.LocalId, cancellationToken);
        int? existingId = null;
        if (requestMap.TryGetValue(dto.LocalId, out var fromBatch))
            existingId = fromBatch;
        existingId ??= mappedId;
        if (dto.ServerId is > 0)
            existingId ??= dto.ServerId;

        if (existingId is int knownId)
        {
            var owner = await GetStudentCodeAsync(con, knownId, session.SchoolID, cancellationToken);
            if (owner is null)
                existingId = null;
        }

        if (existingId is null && operation != SyncOperation.Update)
            existingId = await FindStudentIdByCodeAsync(con, session.SchoolID, dto.StudentCode, cancellationToken);

        if (existingId is int && mappedId is null && dto.ServerId is not > 0 && !requestMap.ContainsKey(dto.LocalId)
            && operation != SyncOperation.Update)
            throw new InvalidOperationException("sync.idExists");

        int serverId;
        if (existingId is null)
        {
            if (operation == SyncOperation.Update)
                throw new InvalidOperationException("Student to update was not found.");

            var taken = await FindStudentIdByCodeAsync(con, session.SchoolID, dto.StudentCode, cancellationToken);
            if (taken is int)
                throw new InvalidOperationException("sync.idExists");

            serverId = await InsertStudentAsync(con, dto, cancellationToken);
        }
        else
        {
            var ownerCode = await GetStudentCodeAsync(con, existingId.Value, session.SchoolID, cancellationToken);
            if (ownerCode is not null
                && !string.Equals(ownerCode, dto.StudentCode, StringComparison.OrdinalIgnoreCase)
                && await FindStudentIdByCodeAsync(con, session.SchoolID, dto.StudentCode, cancellationToken) is int otherId
                && otherId != existingId.Value)
            {
                throw new InvalidOperationException($"Student ID '{dto.StudentCode}' already exists in this school.");
            }

            await UpdateStudentAsync(con, existingId.Value, dto, cancellationToken);
            serverId = existingId.Value;
        }

        if (dto.ClassID is > 0)
            dto.StudentClassServerId = await UpsertStudentClassAsync(con, dto, serverId, cancellationToken);

        await TryMapAsync(con, dto.LocalId, serverId, session.SchoolID, deviceId, cancellationToken);
        await TryLogChangeAsync(con, session, dto.LocalId, serverId, existingId is null ? "Create" : "Update", deviceId, cancellationToken);
        return serverId;
    }

    private static async Task<int> InsertStudentAsync(SqlConnection con, StudentDto dto, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO dbo.Student
    (RegistrationID, SchoolID, ID, SMSPhoneNo, StudentsName, Gender, DateofBirth,
     FathersName, MothersName, BloodGroup, Religion, Status, AdmissionDate,
     StudentEmailAddress, Legal_Identity, StudentsLocalAddress, StudentPermanentAddress, OtherDetails,
     PrevSchoolName, PrevClass, PrevExamYear, PrevExamGrade,
     FatherOccupation, FatherPhoneNumber, MotherOccupation, MotherPhoneNumber,
     GuardianName, GuardianRelationshipwithStudent, GuardianPhoneNumber)
VALUES
    (@RegistrationID, @SchoolID, @ID, @SMSPhoneNo, @StudentsName, @Gender, @DateofBirth,
     @FathersName, @MothersName, @BloodGroup, @Religion, @Status, GETDATE(),
     @StudentEmailAddress, @LegalIdentity, @StudentsLocalAddress, @StudentPermanentAddress, @OtherDetails,
     @PrevSchoolName, @PrevClass, @PrevExamYear, @PrevExamGrade,
     @FatherOccupation, @FatherPhoneNumber, @MotherOccupation, @MotherPhoneNumber,
     @GuardianName, @GuardianRelationship, @GuardianPhoneNumber);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var cmd = new SqlCommand(sql, con);
        AddStudentParameters(cmd, dto);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static async Task UpdateStudentAsync(SqlConnection con, int studentId, StudentDto dto, CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE dbo.Student
SET ID = @ID,
    SMSPhoneNo = @SMSPhoneNo,
    StudentsName = @StudentsName,
    Gender = @Gender,
    DateofBirth = @DateofBirth,
    FathersName = @FathersName,
    MothersName = @MothersName,
    BloodGroup = @BloodGroup,
    Religion = @Religion,
    Status = @Status,
    StudentEmailAddress = @StudentEmailAddress,
    Legal_Identity = @LegalIdentity,
    StudentsLocalAddress = @StudentsLocalAddress,
    StudentPermanentAddress = @StudentPermanentAddress,
    OtherDetails = @OtherDetails,
    PrevSchoolName = @PrevSchoolName,
    PrevClass = @PrevClass,
    PrevExamYear = @PrevExamYear,
    PrevExamGrade = @PrevExamGrade,
    FatherOccupation = @FatherOccupation,
    FatherPhoneNumber = @FatherPhoneNumber,
    MotherOccupation = @MotherOccupation,
    MotherPhoneNumber = @MotherPhoneNumber,
    GuardianName = @GuardianName,
    GuardianRelationshipwithStudent = @GuardianRelationship,
    GuardianPhoneNumber = @GuardianPhoneNumber
WHERE StudentID = @StudentID
  AND SchoolID = @SchoolID";

        await using var cmd = new SqlCommand(sql, con);
        AddStudentParameters(cmd, dto);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (rows == 0)
            throw new InvalidOperationException("Student row was not updated for this school.");
    }

    private static async Task<int> UpsertStudentClassAsync(SqlConnection con, StudentDto dto, int studentId, CancellationToken cancellationToken)
    {
        const string findSql = @"
SELECT TOP 1 StudentClassID
FROM dbo.StudentsClass
WHERE StudentID = @StudentID
  AND SchoolID = @SchoolID
  AND EducationYearID = @EducationYearID";

        await using (var find = new SqlCommand(findSql, con))
        {
            find.Parameters.AddWithValue("@StudentID", studentId);
            find.Parameters.AddWithValue("@SchoolID", dto.SchoolID);
            find.Parameters.AddWithValue("@EducationYearID", dto.EducationYearID);
            var existing = await find.ExecuteScalarAsync(cancellationToken);
            if (existing is not null && existing is not DBNull)
            {
                const string updateSql = @"
UPDATE dbo.StudentsClass
SET ClassID = @ClassID,
    SectionID = @SectionID,
    ShiftID = @ShiftID,
    SubjectGroupID = @SubjectGroupID,
    RollNo = @RollNo
WHERE StudentClassID = @StudentClassID";
                await using var update = new SqlCommand(updateSql, con);
                update.Parameters.AddWithValue("@ClassID", dto.ClassID);
                update.Parameters.AddWithValue("@SectionID", (object?)dto.SectionID ?? DBNull.Value);
                update.Parameters.AddWithValue("@ShiftID", (object?)dto.ShiftID ?? DBNull.Value);
                update.Parameters.AddWithValue("@SubjectGroupID", (object?)dto.SubjectGroupID ?? DBNull.Value);
                update.Parameters.AddWithValue("@RollNo", (object?)dto.RollNo ?? DBNull.Value);
                update.Parameters.AddWithValue("@StudentClassID", Convert.ToInt32(existing));
                await update.ExecuteNonQueryAsync(cancellationToken);
                return Convert.ToInt32(existing);
            }
        }

        const string insertSql = @"
INSERT INTO dbo.StudentsClass
    (SchoolID, RegistrationID, StudentID, ClassID, SectionID, ShiftID, SubjectGroupID, RollNo, EducationYearID, Date, Is_New)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, GETDATE(), 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var insert = new SqlCommand(insertSql, con);
        insert.Parameters.AddWithValue("@SchoolID", dto.SchoolID);
        insert.Parameters.AddWithValue("@RegistrationID", dto.RegistrationID);
        insert.Parameters.AddWithValue("@StudentID", studentId);
        insert.Parameters.AddWithValue("@ClassID", dto.ClassID);
        insert.Parameters.AddWithValue("@SectionID", (object?)dto.SectionID ?? DBNull.Value);
        insert.Parameters.AddWithValue("@ShiftID", (object?)dto.ShiftID ?? DBNull.Value);
        insert.Parameters.AddWithValue("@SubjectGroupID", (object?)dto.SubjectGroupID ?? DBNull.Value);
        insert.Parameters.AddWithValue("@RollNo", (object?)dto.RollNo ?? DBNull.Value);
        insert.Parameters.AddWithValue("@EducationYearID", dto.EducationYearID);
        return Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
    }

    private static void AddStudentParameters(SqlCommand cmd, StudentDto dto)
    {
        cmd.Parameters.AddWithValue("@RegistrationID", dto.RegistrationID);
        cmd.Parameters.AddWithValue("@SchoolID", dto.SchoolID);
        cmd.Parameters.AddWithValue("@ID", dto.StudentCode.Trim());
        cmd.Parameters.AddWithValue("@SMSPhoneNo", (dto.SMSPhoneNo ?? "").Trim());
        cmd.Parameters.AddWithValue("@StudentsName", (dto.StudentsName ?? "").Trim());
        cmd.Parameters.AddWithValue("@Gender", (object?)dto.Gender ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DateofBirth", (object?)dto.DateofBirth ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FathersName", (object?)dto.FathersName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MothersName", (object?)dto.MothersName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BloodGroup", Db(dto.BloodGroup));
        cmd.Parameters.AddWithValue("@Religion", Db(dto.Religion));
        cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status);
        cmd.Parameters.AddWithValue("@StudentEmailAddress", Db(dto.StudentEmailAddress));
        cmd.Parameters.AddWithValue("@LegalIdentity", Db(dto.LegalIdentity));
        cmd.Parameters.AddWithValue("@StudentsLocalAddress", Db(dto.StudentsLocalAddress));
        cmd.Parameters.AddWithValue("@StudentPermanentAddress", Db(dto.StudentPermanentAddress));
        cmd.Parameters.AddWithValue("@OtherDetails", Db(dto.OtherDetails));
        cmd.Parameters.AddWithValue("@PrevSchoolName", Db(dto.PrevSchoolName));
        cmd.Parameters.AddWithValue("@PrevClass", Db(dto.PrevClass));
        cmd.Parameters.AddWithValue("@PrevExamYear", Db(dto.PrevExamYear));
        cmd.Parameters.AddWithValue("@PrevExamGrade", Db(dto.PrevExamGrade));
        cmd.Parameters.AddWithValue("@FatherOccupation", Db(dto.FatherOccupation));
        cmd.Parameters.AddWithValue("@FatherPhoneNumber", Db(dto.FatherPhoneNumber));
        cmd.Parameters.AddWithValue("@MotherOccupation", Db(dto.MotherOccupation));
        cmd.Parameters.AddWithValue("@MotherPhoneNumber", Db(dto.MotherPhoneNumber));
        cmd.Parameters.AddWithValue("@GuardianName", Db(dto.GuardianName));
        cmd.Parameters.AddWithValue("@GuardianRelationship", Db(dto.GuardianRelationshipwithStudent));
        cmd.Parameters.AddWithValue("@GuardianPhoneNumber", Db(dto.GuardianPhoneNumber));
    }

    private static object Db(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static async Task<int?> FindStudentIdByCodeAsync(SqlConnection con, int schoolId, string studentCode, CancellationToken cancellationToken)
    {
        const string sql = "SELECT TOP 1 StudentID FROM dbo.Student WHERE SchoolID = @SchoolID AND ID = @ID";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ID", studentCode.Trim());
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }

    private static async Task<string?> GetStudentCodeAsync(SqlConnection con, int studentId, int schoolId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT ID FROM dbo.Student WHERE StudentID = @StudentID AND SchoolID = @SchoolID";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToString(result);
    }

    private static async Task<int?> TryGetMappedServerIdAsync(SqlConnection con, Guid localId, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(con, "Hybrid_EntityMap", cancellationToken))
            return null;

        const string sql = @"
SELECT ServerId FROM dbo.Hybrid_EntityMap
WHERE LocalId = @LocalId AND EntityType = @EntityType";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@LocalId", localId);
        cmd.Parameters.AddWithValue("@EntityType", EntityTypes.Student);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }

    private static async Task TryMapAsync(
        SqlConnection con, Guid localId, int serverId, int schoolId, string deviceId, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(con, "Hybrid_EntityMap", cancellationToken))
            return;

        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Hybrid_EntityMap WHERE LocalId = @LocalId)
INSERT INTO dbo.Hybrid_EntityMap (LocalId, EntityType, ServerId, SchoolID, DeviceId)
VALUES (@LocalId, @EntityType, @ServerId, @SchoolID, @DeviceId)";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@LocalId", localId);
        cmd.Parameters.AddWithValue("@EntityType", EntityTypes.Student);
        cmd.Parameters.AddWithValue("@ServerId", serverId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task TryLogChangeAsync(
        SqlConnection con,
        SessionSnapshot session,
        Guid localId,
        int serverId,
        string operation,
        string deviceId,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(con, "Hybrid_ChangeLog", cancellationToken))
            return;

        const string sql = @"
INSERT INTO dbo.Hybrid_ChangeLog
    (SchoolID, EducationYearID, EntityType, ServerId, LocalId, Operation, OriginDeviceId)
VALUES
    (@SchoolID, @EducationYearID, @EntityType, @ServerId, @LocalId, @Operation, @DeviceId)";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@EntityType", EntityTypes.Student);
        cmd.Parameters.AddWithValue("@ServerId", serverId);
        cmd.Parameters.AddWithValue("@LocalId", localId);
        cmd.Parameters.AddWithValue("@Operation", operation);
        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<PullResponse> PullSnapshotAsync(
        SqlConnection con, SessionSnapshot session, long since, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 100
    s.StudentID, s.ID, s.StudentsName, s.SMSPhoneNo, s.Gender, s.DateofBirth,
    s.FathersName, s.MothersName, s.BloodGroup, s.Religion, s.AdmissionDate,
    s.Status, s.RegistrationID,
    s.StudentEmailAddress, s.Legal_Identity, s.StudentsLocalAddress, s.StudentPermanentAddress, s.OtherDetails,
    s.PrevSchoolName, s.PrevClass, s.PrevExamYear, s.PrevExamGrade,
    s.FatherOccupation, s.FatherPhoneNumber, s.MotherOccupation, s.MotherPhoneNumber,
    s.GuardianName, s.GuardianRelationshipwithStudent, s.GuardianPhoneNumber,
    sc.StudentClassID, sc.ClassID, sc.RollNo, sc.SectionID, sc.ShiftID, sc.SubjectGroupID,
    sc.EducationYearID, sc.Is_New,
    cc.Class, cs.Section, sh.Shift, sg.SubjectGroup
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s
    ON s.StudentID = sc.StudentID
   AND s.SchoolID = sc.SchoolID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.CreateSection AS cs ON cs.SectionID = sc.SectionID
LEFT JOIN dbo.CreateShift AS sh ON sh.ShiftID = sc.ShiftID
LEFT JOIN dbo.CreateSubjectGroup AS sg ON sg.SubjectGroupID = sc.SubjectGroupID
WHERE sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @EducationYearID
  AND s.StudentID > @Since
ORDER BY s.StudentID";

        var changes = new List<SyncChangeDto>();
        long watermark = since;
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Since", since);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var studentId = reader.GetInt32(0);
            watermark = studentId;
            var dto = new StudentDto
            {
                LocalId = Guid.NewGuid(),
                ServerId = studentId,
                StudentCode = ReadString(reader, "ID") ?? "",
                StudentsName = ReadString(reader, "StudentsName") ?? "",
                SMSPhoneNo = ReadString(reader, "SMSPhoneNo") ?? "",
                Gender = ReadString(reader, "Gender"),
                DateofBirth = ReadDate(reader, "DateofBirth"),
                FathersName = ReadString(reader, "FathersName"),
                MothersName = ReadString(reader, "MothersName"),
                BloodGroup = ReadString(reader, "BloodGroup"),
                Religion = ReadString(reader, "Religion"),
                AdmissionDate = ReadDate(reader, "AdmissionDate"),
                IsNew = ReadBool(reader, "Is_New"),
                Status = ReadString(reader, "Status") ?? "Active",
                RegistrationID = ReadInt(reader, "RegistrationID") ?? 0,
                SchoolID = session.SchoolID,
                EducationYearID = session.EducationYearID,
                StudentClassServerId = ReadInt(reader, "StudentClassID"),
                ClassID = ReadInt(reader, "ClassID"),
                RollNo = ReadString(reader, "RollNo"),
                SectionID = ReadInt(reader, "SectionID"),
                ShiftID = ReadInt(reader, "ShiftID"),
                SubjectGroupID = ReadInt(reader, "SubjectGroupID"),
                ClassName = ReadString(reader, "Class"),
                SectionName = ReadString(reader, "Section"),
                ShiftName = ReadString(reader, "Shift"),
                GroupName = ReadString(reader, "SubjectGroup"),
                StudentEmailAddress = ReadString(reader, "StudentEmailAddress"),
                LegalIdentity = ReadString(reader, "Legal_Identity"),
                StudentsLocalAddress = ReadString(reader, "StudentsLocalAddress"),
                StudentPermanentAddress = ReadString(reader, "StudentPermanentAddress"),
                OtherDetails = ReadString(reader, "OtherDetails"),
                PrevSchoolName = ReadString(reader, "PrevSchoolName"),
                PrevClass = ReadString(reader, "PrevClass"),
                PrevExamYear = ReadString(reader, "PrevExamYear"),
                PrevExamGrade = ReadString(reader, "PrevExamGrade"),
                FatherOccupation = ReadString(reader, "FatherOccupation"),
                FatherPhoneNumber = ReadString(reader, "FatherPhoneNumber"),
                MotherOccupation = ReadString(reader, "MotherOccupation"),
                MotherPhoneNumber = ReadString(reader, "MotherPhoneNumber"),
                GuardianName = ReadString(reader, "GuardianName"),
                GuardianRelationshipwithStudent = ReadString(reader, "GuardianRelationshipwithStudent"),
                GuardianPhoneNumber = ReadString(reader, "GuardianPhoneNumber"),
                UpdatedUtc = DateTime.UtcNow,
                SyncStatus = SyncStatus.Synced
            };

            changes.Add(new SyncChangeDto
            {
                LocalId = dto.LocalId,
                ServerId = dto.ServerId,
                EntityType = EntityTypes.Student,
                Operation = SyncOperation.Create,
                UpdatedUtc = dto.UpdatedUtc,
                PayloadJson = JsonSerializer.Serialize(dto)
            });
        }

        return new PullResponse
        {
            Watermark = watermark,
            HasMore = changes.Count == 100,
            Changes = changes
        };
    }

    private static async Task<bool> TableExistsAsync(SqlConnection con, string tableName, CancellationToken cancellationToken)
    {
        const string sql = "SELECT CASE WHEN OBJECT_ID(@Name, 'U') IS NULL THEN 0 ELSE 1 END";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Name", "dbo." + tableName);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    private static string? ReadString(SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull or null ? null : Convert.ToString(value);
    }

    private static int? ReadInt(SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull or null ? null : Convert.ToInt32(value);
    }

    private static DateTime? ReadDate(SqlDataReader reader, string column)
    {
        var value = reader[column];
        return value is DBNull or null ? null : Convert.ToDateTime(value);
    }

    private static bool? ReadBool(SqlDataReader reader, string column)
    {
        var value = reader[column];
        if (value is DBNull or null)
            return null;
        if (value is bool flag)
            return flag;
        return Convert.ToInt32(value) != 0;
    }
}
