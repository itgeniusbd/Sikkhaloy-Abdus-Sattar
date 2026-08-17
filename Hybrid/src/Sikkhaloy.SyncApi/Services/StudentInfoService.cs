using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed class StudentInfoService
{
    private readonly EduConnectionFactory _connections;

    public StudentInfoService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<StudentSignupListsDto> ListSignupAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, string? studentCode,
        CancellationToken cancellationToken)
    {
        var code = (studentCode ?? "").Trim();
        var result = new StudentSignupListsDto();
        if (code.Length == 0 && classId <= 0)
            return result;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        result.WithoutLogin.AddRange(await QuerySignupWithoutLoginAsync(
            con, session, classId, groupId, sectionId, shiftId, code, cancellationToken));
        result.Created.AddRange(await QueryCreatedUsersAsync(
            con, session, classId, groupId, sectionId, shiftId, code, cancellationToken));
        return result;
    }

    public async Task<StudentInfoResult> CreateUsersAsync(
        SessionSnapshot session, CreateStudentUsersRequest? request, CancellationToken cancellationToken)
    {
        var ids = (request?.StudentIDs ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return Fail("si.needStudents");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var created = 0;
        foreach (var studentId in ids)
        {
            await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
            try
            {
                var info = await ReadCreateTargetAsync(con, tx, session, studentId, cancellationToken);
                if (info is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    continue;
                }

                var userName = session.SchoolID + info.Value.Code;
                if (await UserExistsAsync(con, tx, userName, cancellationToken))
                {
                    await tx.RollbackAsync(cancellationToken);
                    continue;
                }

                var password = Random.Shared.Next(100000, 999999).ToString();
                var email = string.IsNullOrWhiteSpace(info.Value.Email)
                    ? $"{userName}@sikkhaloy.local"
                    : info.Value.Email.Trim();
                await InsertMembershipManuallyAsync(
                    con, tx, Guid.NewGuid(), userName, password, email, info.Value.Code, cancellationToken);
                await AddToRoleAsync(con, tx, userName, "Student", cancellationToken);
                var registrationId = await InsertRegistrationAsync(con, tx, session.SchoolID, userName, cancellationToken);
                await using (var upd = new SqlCommand("""
UPDATE dbo.Student
SET StudentRegistrationID = @RegistrationID
WHERE StudentID = @StudentID AND SchoolID = @SchoolID AND StudentRegistrationID IS NULL
""", con, tx))
                {
                    upd.Parameters.AddWithValue("@RegistrationID", registrationId);
                    upd.Parameters.AddWithValue("@StudentID", studentId);
                    upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    if (await upd.ExecuteNonQueryAsync(cancellationToken) == 0)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        continue;
                    }
                }

                await InsertAstAsync(con, tx, registrationId, session.SchoolID, userName, password, info.Value.Code, cancellationToken);
                await InsertEducationYearAsync(con, tx, registrationId, session.SchoolID, session.EducationYearID, cancellationToken);
                await tx.CommitAsync(cancellationToken);
                created++;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
            }
        }

        if (created == 0)
            return Fail("si.notCreated");
        return new StudentInfoResult { Succeeded = true, Count = created };
    }

    public async Task<IReadOnlyList<StudentAccountDto>> ListAccountsAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, string? studentCode,
        CancellationToken cancellationToken)
    {
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0 && classId <= 0)
            return [];

        const string byClass = """
SELECT Student.StudentID, Registration.RegistrationID, Student.ID, Student.StudentsName,
       Student.SMSPhoneNo, aspnet_Membership.Email, Registration.UserName, Registration.Validation,
       aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut,
       aspnet_Membership.CreateDate, aspnet_Membership.LastLoginDate
FROM dbo.aspnet_Users
INNER JOIN dbo.aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId
INNER JOIN dbo.Registration ON aspnet_Users.UserName = Registration.UserName
INNER JOIN dbo.Student ON Registration.RegistrationID = Student.StudentRegistrationID
INNER JOIN dbo.StudentsClass ON Student.StudentID = StudentsClass.StudentID
WHERE Registration.Category = N'Student'
  AND Registration.SchoolID = @SchoolID
  AND Student.Status = N'Active'
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""";
        const string byId = """
SELECT Student.StudentID, Registration.RegistrationID, Student.ID, Student.StudentsName,
       Student.SMSPhoneNo, aspnet_Membership.Email, Registration.UserName, Registration.Validation,
       aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut,
       aspnet_Membership.CreateDate, aspnet_Membership.LastLoginDate
FROM dbo.aspnet_Users
INNER JOIN dbo.aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId
INNER JOIN dbo.Registration ON aspnet_Users.UserName = Registration.UserName
INNER JOIN dbo.Student ON Registration.RegistrationID = Student.StudentRegistrationID
WHERE Registration.Category = N'Student'
  AND Registration.SchoolID = @SchoolID
  AND Student.Status = N'Active'
  AND Student.ID = @ID
""";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(code.Length > 0 ? byId : byClass, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        if (code.Length > 0)
        {
            cmd.Parameters.AddWithValue("@ID", code);
        }
        else
        {
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            AddClassFilters(cmd, classId, groupId, sectionId, shiftId);
        }

        var items = new List<StudentAccountDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentAccountDto
            {
                StudentID = ToInt(reader["StudentID"]),
                RegistrationID = ToInt(reader["RegistrationID"]),
                ID = NullString(reader["ID"]) ?? "",
                StudentsName = NullString(reader["StudentsName"]) ?? "",
                Phone = NullString(reader["SMSPhoneNo"]),
                Email = NullString(reader["Email"]),
                UserName = NullString(reader["UserName"]) ?? "",
                Validation = NullString(reader["Validation"]) ?? "",
                IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"]),
                CreateDate = ReadDate(reader["CreateDate"]),
                LastLoginDate = ReadDate(reader["LastLoginDate"])
            });
        }

        return items;
    }

    public async Task<StudentAccountResult> SetApprovedAsync(
        SessionSnapshot session, SetStudentApprovedRequest? request, CancellationToken cancellationToken)
    {
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0)
            return AccountFail("si.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var userId = await FindStudentUserIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (userId is null)
            return AccountFail("si.needUser");

        await using var cmd = new SqlCommand("""
UPDATE dbo.aspnet_Membership SET IsApproved = @IsApproved WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@IsApproved", request!.IsApproved);
        cmd.Parameters.AddWithValue("@UserId", userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return await ReadAccountStatusAsync(con, userId.Value, cancellationToken);
    }

    public async Task<StudentAccountResult> UnlockAsync(
        SessionSnapshot session, UnlockStudentRequest? request, CancellationToken cancellationToken)
    {
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0)
            return AccountFail("si.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var userId = await FindStudentUserIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (userId is null)
            return AccountFail("si.needUser");

        await using var cmd = new SqlCommand("""
UPDATE dbo.aspnet_Membership
SET IsLockedOut = 0,
    FailedPasswordAttemptCount = 0,
    FailedPasswordAttemptWindowStart = CONVERT(datetime, '17540101', 112),
    FailedPasswordAnswerAttemptCount = 0,
    FailedPasswordAnswerAttemptWindowStart = CONVERT(datetime, '17540101', 112),
    LastLockoutDate = CONVERT(datetime, '17540101', 112)
WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@UserId", userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return await ReadAccountStatusAsync(con, userId.Value, cancellationToken);
    }

    public async Task<StudentInfoResult> DeleteAccountAsync(
        SessionSnapshot session, DeleteStudentAccountRequest? request, CancellationToken cancellationToken)
    {
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0 || request is null || request.RegistrationID <= 0)
            return Fail("si.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var userId = await FindStudentUserIdAsync(con, session.SchoolID, userName, cancellationToken, tx);
            if (userId is null)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("si.needUser");
            }

            await using (var ast = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
DELETE FROM dbo.AST WHERE UserName = @UserName AND SchoolID = @SchoolID
""", con, tx))
            {
                ast.Parameters.AddWithValue("@UserName", userName);
                ast.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await ast.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var unlink = new SqlCommand("""
UPDATE dbo.Student SET StudentRegistrationID = NULL
WHERE StudentRegistrationID = @RegistrationID AND SchoolID = @SchoolID
""", con, tx))
            {
                unlink.Parameters.AddWithValue("@RegistrationID", request.RegistrationID);
                unlink.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await unlink.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var year = new SqlCommand("""
DELETE FROM dbo.Education_Year_User
WHERE RegistrationID = @RegistrationID AND SchoolID = @SchoolID
""", con, tx))
            {
                year.Parameters.AddWithValue("@RegistrationID", request.RegistrationID);
                year.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await year.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var reg = new SqlCommand("""
DELETE FROM dbo.Registration
WHERE RegistrationID = @RegistrationID AND SchoolID = @SchoolID AND Category = N'Student'
""", con, tx))
            {
                reg.Parameters.AddWithValue("@RegistrationID", request.RegistrationID);
                reg.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                if (await reg.ExecuteNonQueryAsync(cancellationToken) == 0)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("si.needUser");
                }
            }

            await using (var roles = new SqlCommand("DELETE FROM dbo.aspnet_UsersInRoles WHERE UserId = @UserId", con, tx))
            {
                roles.Parameters.AddWithValue("@UserId", userId.Value);
                await roles.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var mem = new SqlCommand("DELETE FROM dbo.aspnet_Membership WHERE UserId = @UserId", con, tx))
            {
                mem.Parameters.AddWithValue("@UserId", userId.Value);
                await mem.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var users = new SqlCommand("DELETE FROM dbo.aspnet_Users WHERE UserId = @UserId", con, tx))
            {
                users.Parameters.AddWithValue("@UserId", userId.Value);
                await users.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new StudentInfoResult { Succeeded = true, Count = 1 };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<StudentIdCardDto>> ListIdCardsAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, string? ids,
        CancellationToken cancellationToken)
    {
        var parsed = ParseIds(ids);
        if (parsed.Count == 0 && classId <= 0)
            return [];

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        string sql;
        if (parsed.Count > 0)
        {
            var names = parsed.Select((_, i) => "@id" + i).ToArray();
            sql = $"""
SELECT Student.StudentID, Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class,
       StudentsClass.RollNo, Student.SMSPhoneNo, Student.BloodGroup, Student.DateofBirth,
       Student.StudentPermanentAddress, Student.StudentsLocalAddress,
       SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Institution_Dialog,
       SchoolInfo.SchoolLogo, Student_Image.Image
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
INNER JOIN dbo.SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID
LEFT OUTER JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE StudentsClass.EducationYearID = @EducationYearID
  AND Student.Status = N'Active'
  AND SchoolInfo.SchoolID = @SchoolID
  AND Student.ID IN ({string.Join(",", names)})
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""";
        }
        else
        {
            sql = """
SELECT Student.StudentID, Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class,
       StudentsClass.RollNo, Student.SMSPhoneNo, Student.BloodGroup, Student.DateofBirth,
       Student.StudentPermanentAddress, Student.StudentsLocalAddress,
       SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Institution_Dialog,
       SchoolInfo.SchoolLogo, Student_Image.Image
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
INNER JOIN dbo.SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID
LEFT OUTER JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND StudentsClass.EducationYearID = @EducationYearID
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
  AND Student.Status = N'Active'
  AND SchoolInfo.SchoolID = @SchoolID
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""";
        }

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        if (parsed.Count > 0)
        {
            for (var i = 0; i < parsed.Count; i++)
                cmd.Parameters.AddWithValue("@id" + i, parsed[i]);
        }
        else
        {
            AddClassFilters(cmd, classId, groupId, sectionId, shiftId);
        }

        var items = new List<StudentIdCardDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentIdCardDto
            {
                StudentID = ToInt(reader["StudentID"]),
                ID = NullString(reader["ID"]) ?? "",
                StudentsName = NullString(reader["StudentsName"]) ?? "",
                FathersName = NullString(reader["FathersName"]),
                ClassName = NullString(reader["Class"]),
                RollNo = NullString(reader["RollNo"]),
                Phone = NullString(reader["SMSPhoneNo"]),
                BloodGroup = NullString(reader["BloodGroup"]),
                PermanentAddress = NullString(reader["StudentPermanentAddress"]),
                LocalAddress = NullString(reader["StudentsLocalAddress"]),
                DateofBirth = ReadDate(reader["DateofBirth"]),
                SchoolName = NullString(reader["SchoolName"]) ?? "",
                SchoolAddress = NullString(reader["Address"]),
                InstitutionDialog = NullString(reader["Institution_Dialog"]),
                LogoDataUrl = ToDataUrl(reader["SchoolLogo"] as byte[]),
                PhotoDataUrl = ToDataUrl(reader["Image"] as byte[])
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<StudentPhotoDto>> ListPhotosAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT s.StudentID, si.Image
FROM dbo.Student AS s
INNER JOIN dbo.StudentsClass AS sc ON sc.StudentID = s.StudentID
LEFT OUTER JOIN dbo.Student_Image AS si ON s.StudentImageID = si.StudentImageID
WHERE sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @EducationYearID
  AND s.Status = N'Active'
  AND si.Image IS NOT NULL
""", con);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);

        var map = new Dictionary<int, StudentPhotoDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = ToInt(reader["StudentID"]);
            if (id <= 0 || map.ContainsKey(id))
                continue;
            if (reader["Image"] is not byte[] bytes || bytes.Length == 0)
                continue;
            var url = ToDataUrl(bytes);
            if (string.IsNullOrWhiteSpace(url))
                continue;
            map[id] = new StudentPhotoDto { StudentID = id, PhotoDataUrl = url };
        }

        return map.Values.ToList();
    }

    public async Task<StudentPlacementDto?> FindPlacementAsync(
        SessionSnapshot session, string? studentCode, CancellationToken cancellationToken)
    {
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0)
            return null;

        const string sql = """
SELECT TOP 1 Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID,
       Student.StudentsName, Student.FathersName, Student.MothersName, CreateClass.Class,
       CreateSubjectGroup.SubjectGroup, CreateSection.Section, CreateShift.Shift,
       StudentsClass.RollNo, Student.SMSPhoneNo, Student.Gender, Student.DateofBirth, Student.BloodGroup,
       ISNULL(StudentsClass.SubjectGroupID, 0) AS SubjectGroupID,
       ISNULL(StudentsClass.SectionID, 0) AS SectionID,
       ISNULL(StudentsClass.ShiftID, 0) AS ShiftID,
       Student.StudentPermanentAddress, SchoolInfo.SchoolName, SchoolInfo.Address,
       Education_Year.EducationYear
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.SchoolInfo ON Student.SchoolID = SchoolInfo.SchoolID
INNER JOIN dbo.Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
LEFT OUTER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT OUTER JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT OUTER JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT OUTER JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
WHERE StudentsClass.EducationYearID = @EducationYearID
  AND Student.ID = @ID
  AND StudentsClass.SchoolID = @SchoolID
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return ReadPlacement(reader);
    }

    public async Task<StudentInfoResult> SavePlacementAsync(
        SessionSnapshot session, SaveStudentPlacementRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentClassID <= 0)
            return Fail("si.needStudent");

        var sets = new List<string> { "RollNo = @RollNo" };
        if (request.UpdateGroup)
            sets.Add("SubjectGroupID = @SubjectGroupID");
        if (request.UpdateSection)
            sets.Add("SectionID = @SectionID");
        if (request.UpdateShift)
            sets.Add("ShiftID = @ShiftID");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand($"""
UPDATE dbo.StudentsClass
SET {string.Join(", ", sets)}
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
        cmd.Parameters.AddWithValue("@RollNo", (object?)request.RollNo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StudentClassID", request.StudentClassID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (request.UpdateGroup)
            AddNullableId(cmd, "@SubjectGroupID", request.SubjectGroupID);
        if (request.UpdateSection)
            AddNullableId(cmd, "@SectionID", request.SectionID);
        if (request.UpdateShift)
            AddNullableId(cmd, "@ShiftID", request.ShiftID);

        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0
            ? new StudentInfoResult { Succeeded = true, Count = n }
            : Fail("si.needStudent");
    }

    public async Task<StudentSubjectsDto> GetSubjectsAsync(
        SessionSnapshot session, string? studentCode, CancellationToken cancellationToken)
    {
        var dto = new StudentSubjectsDto
        {
            Student = await FindPlacementAsync(session, studentCode, cancellationToken)
        };
        if (dto.Student is null)
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var assigned = new Dictionary<int, string>();
        await using (var rec = new SqlCommand("""
SELECT SubjectID, SubjectType
FROM dbo.StudentRecord
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID
""", con))
        {
            rec.Parameters.AddWithValue("@StudentClassID", dto.Student.StudentClassID);
            rec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await rec.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                assigned[ToInt(reader["SubjectID"])] = NullString(reader["SubjectType"]) ?? "Compulsory";
        }

        await using (var sub = new SqlCommand("""
SELECT SubjectID, SubjectName
FROM dbo.Subject
WHERE SchoolID = @SchoolID
ORDER BY SubjectName
""", con))
        {
            sub.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await sub.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = ToInt(reader["SubjectID"]);
                dto.Subjects.Add(new StudentSubjectRowDto
                {
                    SubjectID = id,
                    SubjectName = NullString(reader["SubjectName"]) ?? "",
                    Selected = assigned.ContainsKey(id),
                    SubjectType = assigned.TryGetValue(id, out var type) ? type : "Compulsory"
                });
            }
        }

        return dto;
    }

    public async Task<StudentInfoResult> SaveSubjectsAsync(
        SessionSnapshot session, SaveStudentSubjectsRequest? request, CancellationToken cancellationToken)
    {
        var items = (request?.Items ?? []).Where(x => x.SubjectID > 0).ToList();
        if (request is null || request.StudentID <= 0 || request.StudentClassID <= 0)
            return Fail("si.needStudent");
        if (items.Count == 0)
            return Fail("si.needSubject");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var del = new SqlCommand("""
DELETE FROM dbo.StudentRecord
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID
""", con, tx))
            {
                del.Parameters.AddWithValue("@StudentClassID", request.StudentClassID);
                del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await del.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var item in items)
            {
                var type = string.Equals(item.SubjectType, "Optional", StringComparison.OrdinalIgnoreCase)
                    ? "Optional"
                    : "Compulsory";
                await using var ins = new SqlCommand("""
INSERT INTO dbo.StudentRecord
    (StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date)
VALUES
    (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())
""", con, tx);
                ins.Parameters.AddWithValue("@StudentID", request.StudentID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@StudentClassID", request.StudentClassID);
                ins.Parameters.AddWithValue("@SubjectID", item.SubjectID);
                ins.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                ins.Parameters.AddWithValue("@SubjectType", type);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var marks = new SqlCommand("""
UPDATE Exam_Obtain_Marks
SET Exam_Obtain_Marks.StudentRecordID = StudentRecord.StudentRecordID
FROM Exam_Obtain_Marks
INNER JOIN StudentRecord
    ON StudentRecord.EducationYearID = Exam_Obtain_Marks.EducationYearID
   AND StudentRecord.SubjectID = Exam_Obtain_Marks.SubjectID
   AND StudentRecord.StudentID = Exam_Obtain_Marks.StudentID
   AND StudentRecord.SchoolID = Exam_Obtain_Marks.SchoolID
WHERE StudentRecord.StudentClassID = @StudentClassID
""", con, tx))
            {
                marks.Parameters.AddWithValue("@StudentClassID", request.StudentClassID);
                await marks.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var result = new SqlCommand("""
UPDATE Exam_Result_of_Subject
SET Exam_Result_of_Subject.StudentRecordID = StudentRecord.StudentRecordID
FROM StudentRecord
INNER JOIN Exam_Result_of_Subject
    ON StudentRecord.StudentID = Exam_Result_of_Subject.StudentID
   AND StudentRecord.EducationYearID = Exam_Result_of_Subject.EducationYearID
   AND StudentRecord.SubjectID = Exam_Result_of_Subject.SubjectID
   AND StudentRecord.SchoolID = Exam_Result_of_Subject.SchoolID
WHERE StudentRecord.StudentClassID = @StudentClassID
""", con, tx))
            {
                result.Parameters.AddWithValue("@StudentClassID", request.StudentClassID);
                await result.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new StudentInfoResult { Succeeded = true, Count = items.Count };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<StudentPlacementDto?> FindCertificateAsync(
        SessionSnapshot session, string? studentCode, CancellationToken cancellationToken)
    {
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0)
            return null;

        const string sql = """
SELECT TOP 1 Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID,
       Student.StudentsName, Student.FathersName, Student.MothersName, CreateClass.Class,
       CAST(NULL AS nvarchar(50)) AS SubjectGroup, CAST(NULL AS nvarchar(50)) AS Section,
       CAST(NULL AS nvarchar(50)) AS Shift, StudentsClass.RollNo, Student.SMSPhoneNo, Student.Gender,
       Student.DateofBirth, Student.BloodGroup, 0 AS SubjectGroupID, 0 AS SectionID, 0 AS ShiftID,
       Student.StudentPermanentAddress, SchoolInfo.SchoolName, SchoolInfo.Address,
       Education_Year.EducationYear
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
INNER JOIN dbo.SchoolInfo ON Student.SchoolID = SchoolInfo.SchoolID
LEFT OUTER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE Student.ID = @ID
  AND Student.SchoolID = @SchoolID
  AND StudentsClass.Class_Status IS NULL
ORDER BY StudentsClass.EducationYearID DESC
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return ReadPlacement(reader);
    }

    private async Task<List<StudentSignupRowDto>> QuerySignupWithoutLoginAsync(
        SqlConnection con, SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId,
        string code, CancellationToken cancellationToken)
    {
        const string byClass = """
SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.FathersName,
       StudentsClass.RollNo, Student.SMSPhoneNo, StudentsClass.StudentClassID, StudentsClass.StudentID
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
  AND Student.Status = N'Active'
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
  AND Student.StudentRegistrationID IS NULL
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""";
        const string byId = """
SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.FathersName,
       StudentsClass.RollNo, Student.SMSPhoneNo, StudentsClass.StudentClassID, StudentsClass.StudentID
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
WHERE Student.ID = @ID
  AND Student.Status = N'Active'
  AND StudentsClass.SchoolID = @SchoolID
  AND Student.StudentRegistrationID IS NULL
  AND StudentsClass.EducationYearID = @EducationYearID
""";
        await using var cmd = new SqlCommand(code.Length > 0 ? byId : byClass, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (code.Length > 0)
            cmd.Parameters.AddWithValue("@ID", code);
        else
            AddClassFilters(cmd, classId, groupId, sectionId, shiftId);

        var items = new List<StudentSignupRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentSignupRowDto
            {
                StudentID = ToInt(reader["StudentID"]),
                StudentClassID = ToInt(reader["StudentClassID"]),
                ID = NullString(reader["ID"]) ?? "",
                StudentsName = NullString(reader["StudentsName"]) ?? "",
                FathersName = NullString(reader["FathersName"]),
                RollNo = NullString(reader["RollNo"]),
                Phone = NullString(reader["SMSPhoneNo"]),
                Address = NullString(reader["StudentsLocalAddress"])
            });
        }

        return items;
    }

    private async Task<List<StudentCreatedUserDto>> QueryCreatedUsersAsync(
        SqlConnection con, SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId,
        string code, CancellationToken cancellationToken)
    {
        const string byClass = """
SELECT Student.ID, Student.StudentsName, Registration.UserName, Student.SMSPhoneNo, AST.Password,
       Registration.CreateDate, Student.StudentID, StudentsClass.RollNo
FROM dbo.aspnet_Users
INNER JOIN dbo.Registration ON aspnet_Users.UserName = Registration.UserName
INNER JOIN dbo.Student ON Registration.RegistrationID = Student.StudentRegistrationID
INNER JOIN dbo.StudentsClass ON Student.StudentID = StudentsClass.StudentID
LEFT OUTER JOIN dbo.AST ON Student.StudentRegistrationID = AST.RegistrationID
WHERE Registration.Category = N'Student'
  AND Registration.SchoolID = @SchoolID
  AND Student.Status = N'Active'
  AND StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
  AND StudentsClass.EducationYearID = @EducationYearID
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""";
        const string byId = """
SELECT Student.ID, Student.StudentsName, Registration.UserName, Student.SMSPhoneNo, AST.Password,
       Registration.CreateDate, Student.StudentID, StudentsClass.RollNo
FROM dbo.aspnet_Users
INNER JOIN dbo.Registration ON aspnet_Users.UserName = Registration.UserName
INNER JOIN dbo.Student ON Registration.RegistrationID = Student.StudentRegistrationID
INNER JOIN dbo.StudentsClass ON Student.StudentID = StudentsClass.StudentID
LEFT OUTER JOIN dbo.AST ON Student.StudentRegistrationID = AST.RegistrationID
WHERE Registration.Category = N'Student'
  AND Registration.SchoolID = @SchoolID
  AND Student.Status = N'Active'
  AND Student.ID = @ID
  AND StudentsClass.EducationYearID = @EducationYearID
""";
        await using var cmd = new SqlCommand(code.Length > 0 ? byId : byClass, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (code.Length > 0)
            cmd.Parameters.AddWithValue("@ID", code);
        else
            AddClassFilters(cmd, classId, groupId, sectionId, shiftId);

        var items = new List<StudentCreatedUserDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentCreatedUserDto
            {
                StudentID = ToInt(reader["StudentID"]),
                ID = NullString(reader["ID"]) ?? "",
                StudentsName = NullString(reader["StudentsName"]) ?? "",
                RollNo = NullString(reader["RollNo"]),
                Phone = NullString(reader["SMSPhoneNo"]),
                UserName = NullString(reader["UserName"]) ?? "",
                Password = NullString(reader["Password"]) ?? "",
                CreateDate = ReadDate(reader["CreateDate"])
            });
        }

        return items;
    }

    private static async Task<(string Code, string? Email)?> ReadCreateTargetAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, int studentId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 Student.ID, Student.StudentEmailAddress
FROM dbo.Student
INNER JOIN dbo.StudentsClass ON Student.StudentID = StudentsClass.StudentID
WHERE Student.StudentID = @StudentID
  AND Student.SchoolID = @SchoolID
  AND Student.Status = N'Active'
  AND Student.StudentRegistrationID IS NULL
  AND StudentsClass.EducationYearID = @EducationYearID
""", con, tx);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return (NullString(reader["ID"]) ?? "", NullString(reader["StudentEmailAddress"]));
    }

    private static async Task<bool> UserExistsAsync(
        SqlConnection con, SqlTransaction tx, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT 1
WHERE EXISTS (
    SELECT 1 FROM dbo.aspnet_Users AS u
    INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
    WHERE u.LoweredUserName = LOWER(@UserName))
   OR EXISTS (SELECT 1 FROM dbo.Registration WHERE UserName = @UserName)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        return await cmd.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
    }

    private static async Task InsertMembershipManuallyAsync(
        SqlConnection con, SqlTransaction tx, Guid userId, string userName, string password,
        string email, string answer, CancellationToken cancellationToken)
    {
        var salt = MembershipPasswordVerifier.NewSalt();
        var hashedPassword = MembershipPasswordVerifier.Hash(password, salt);
        var hashedAnswer = MembershipPasswordVerifier.Hash(answer, salt);
        var utcNow = DateTime.UtcNow;
        var lockout = new DateTime(1754, 1, 1);

        await using var appCmd = new SqlCommand(
            "SELECT ApplicationId FROM dbo.aspnet_Applications WHERE LoweredApplicationName = N'/'", con, tx);
        var appIdObj = await appCmd.ExecuteScalarAsync(cancellationToken);
        if (appIdObj is null or DBNull)
            throw new InvalidOperationException("Membership application '/' was not found.");
        var appId = (Guid)appIdObj;

        await using var userCmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
VALUES (@ApplicationId, @UserId, @UserName, LOWER(@UserName), 0, @Now)
""", con, tx);
        userCmd.Parameters.AddWithValue("@ApplicationId", appId);
        userCmd.Parameters.AddWithValue("@UserId", userId);
        userCmd.Parameters.AddWithValue("@UserName", userName);
        userCmd.Parameters.AddWithValue("@Now", utcNow);
        await userCmd.ExecuteNonQueryAsync(cancellationToken);

        await using var memCmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Membership
    (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, Email, LoweredEmail,
     PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate,
     LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
     FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)
VALUES
    (@ApplicationId, @UserId, @Password, 1, @PasswordSalt, @Email, LOWER(@Email),
     @PasswordQuestion, @PasswordAnswer, 1, 0, @Now, @Now,
     @Now, @Lockout, 0, @Lockout,
     0, @Lockout)
""", con, tx);
        memCmd.Parameters.AddWithValue("@ApplicationId", appId);
        memCmd.Parameters.AddWithValue("@UserId", userId);
        memCmd.Parameters.AddWithValue("@Password", hashedPassword);
        memCmd.Parameters.AddWithValue("@PasswordSalt", salt);
        memCmd.Parameters.AddWithValue("@Email", email);
        memCmd.Parameters.AddWithValue("@PasswordQuestion", "Student ID");
        memCmd.Parameters.AddWithValue("@PasswordAnswer", hashedAnswer);
        memCmd.Parameters.AddWithValue("@Now", utcNow);
        memCmd.Parameters.AddWithValue("@Lockout", lockout);
        await memCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task AddToRoleAsync(
        SqlConnection con, SqlTransaction tx, string userName, string roleName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.aspnet_Roles AS r ON r.ApplicationId = a.ApplicationId AND r.LoweredRoleName = LOWER(@RoleName)
WHERE u.LoweredUserName = LOWER(@UserName)
  AND NOT EXISTS (
      SELECT 1 FROM dbo.aspnet_UsersInRoles AS ur
      WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@RoleName", roleName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> InsertRegistrationAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Registration (SchoolID, UserName, Validation, Category, CreateDate)
VALUES (@SchoolID, @UserName, N'Valid', N'Student', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task InsertAstAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, string userName,
        string password, string answer, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
INSERT INTO dbo.AST (RegistrationID, SchoolID, UserName, Category, Password, PasswordAnswer)
VALUES (@RegistrationID, @SchoolID, @UserName, N'Student', @Password, @PasswordAnswer)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@Password", password);
        cmd.Parameters.AddWithValue("@PasswordAnswer", answer);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertEducationYearAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, int educationYearId,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Education_Year_User (RegistrationID, EducationYearID, SchoolID)
VALUES (@RegistrationID, @EducationYearID, @SchoolID)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Guid?> FindStudentUserIdAsync(
        SqlConnection con, int schoolId, string userName, CancellationToken cancellationToken, SqlTransaction? tx = null)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 u.UserId
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Applications AS a
    ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.Registration AS r ON r.UserName = u.UserName
WHERE u.LoweredUserName = LOWER(@UserName)
  AND r.SchoolID = @SchoolID
  AND r.Category = N'Student'
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is Guid id ? id : value is null or DBNull ? null : (Guid?)value;
    }

    private static async Task<StudentAccountResult> ReadAccountStatusAsync(
        SqlConnection con, Guid userId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT IsApproved, IsLockedOut FROM dbo.aspnet_Membership WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return AccountFail("si.needUser");
        return new StudentAccountResult
        {
            Succeeded = true,
            IsApproved = Convert.ToBoolean(reader["IsApproved"]),
            IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"])
        };
    }

    private static StudentPlacementDto ReadPlacement(SqlDataReader reader) => new()
    {
        StudentID = ToInt(reader["StudentID"]),
        StudentClassID = ToInt(reader["StudentClassID"]),
        ClassID = ToInt(reader["ClassID"]),
        ID = NullString(reader["ID"]) ?? "",
        StudentsName = NullString(reader["StudentsName"]) ?? "",
        FathersName = NullString(reader["FathersName"]),
        MothersName = NullString(reader["MothersName"]),
        ClassName = NullString(reader["Class"]),
        GroupName = NullString(reader["SubjectGroup"]),
        SectionName = NullString(reader["Section"]),
        ShiftName = NullString(reader["Shift"]),
        RollNo = NullString(reader["RollNo"]),
        Phone = NullString(reader["SMSPhoneNo"]),
        Gender = NullString(reader["Gender"]),
        DateofBirth = ReadDate(reader["DateofBirth"]),
        BloodGroup = NullString(reader["BloodGroup"]),
        SubjectGroupID = ToInt(reader["SubjectGroupID"]),
        SectionID = ToInt(reader["SectionID"]),
        ShiftID = ToInt(reader["ShiftID"]),
        PermanentAddress = NullString(reader["StudentPermanentAddress"]),
        SchoolName = NullString(reader["SchoolName"]) ?? "",
        SchoolAddress = NullString(reader["Address"]),
        EducationYear = NullString(reader["EducationYear"])
    };

    public async Task<StudentReportDto> GetReportAsync(
        SessionSnapshot session, string? studentCode, CancellationToken cancellationToken)
    {
        var dto = new StudentReportDto();
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0)
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        await using (var find = new SqlCommand("""
SELECT TOP 1 Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
  AND Student.ID = @ID
""", con))
        {
            find.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            find.Parameters.AddWithValue("@ID", code);
            await using var reader = await find.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return dto;
            dto.Found = true;
            dto.StudentID = ToInt(reader["StudentID"]);
            dto.StudentClassID = ToInt(reader["StudentClassID"]);
            dto.ClassID = ToInt(reader["ClassID"]);
        }

        dto.PhotoDataUrl = await QueryPhotoAsync(con, dto.StudentID, cancellationToken);
        await FillResultAsync(con, session, dto, cancellationToken);
        await FillAttendanceAsync(con, session, dto, cancellationToken);
        await FillSubjectsAsync(con, session, dto, cancellationToken);
        await FillAccountsAsync(con, session, dto, cancellationToken);
        return dto;
    }

    private static async Task<string?> QueryPhotoAsync(
        SqlConnection con, int studentId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT si.Image
FROM dbo.Student AS s
LEFT OUTER JOIN dbo.Student_Image AS si ON s.StudentImageID = si.StudentImageID
WHERE s.StudentID = @StudentID
""", con);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            return value is byte[] bytes && bytes.Length > 0 ? ToDataUrl(bytes) : null;
        }
        catch (SqlException)
        {
            return null;
        }
    }

    private static async Task FillResultAsync(
        SqlConnection con, SessionSnapshot session, StudentReportDto dto, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(con, "Exam_Result_of_Student", cancellationToken)
            || !await TableExistsAsync(con, "Exam_Result_of_Subject", cancellationToken))
            return;

        const string pub = "N'Pub'";
        try
        {
            await using var best = new SqlCommand($"""
SELECT TOP (1) ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Top_Sub,
       Subject.SubjectName
FROM Exam_Result_of_Subject
INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Result_of_Subject.StudentID = @StudentID
  AND Exam_Result_of_Student.StudentPublishStatus = {pub}
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN
ORDER BY Top_Sub DESC
""", con);
            best.Parameters.AddWithValue("@StudentID", dto.StudentID);
            await using var reader = await best.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                dto.Result.BestSubject = NullString(reader["SubjectName"]);
                dto.Result.BestAvg = ToDecN(reader["Top_Sub"]);
            }
        }
        catch (SqlException)
        {
        }

        try
        {
            await using var worst = new SqlCommand($"""
SELECT TOP (1) ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Worst_Sub,
       Subject.SubjectName
FROM Exam_Result_of_Subject
INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Result_of_Subject.StudentID = @StudentID
  AND Exam_Result_of_Student.StudentPublishStatus = {pub}
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN
ORDER BY Worst_Sub ASC
""", con);
            worst.Parameters.AddWithValue("@StudentID", dto.StudentID);
            await using var reader = await worst.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                dto.Result.WorstSubject = NullString(reader["SubjectName"]);
                dto.Result.WorstAvg = ToDecN(reader["Worst_Sub"]);
            }
        }
        catch (SqlException)
        {
        }

        try
        {
            await using var avgs = new SqlCommand($"""
SELECT Subject.SubjectName,
       ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Sub_Avg,
       AVG(CAST(Exam_Result_of_Subject.Position_InSubject_Class AS int)) AS Sub_Position
FROM Exam_Result_of_Subject
INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Result_of_Subject.StudentID = @StudentID
  AND Exam_Result_of_Student.StudentPublishStatus = {pub}
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN
ORDER BY Sub_Avg DESC
""", con);
            avgs.Parameters.AddWithValue("@StudentID", dto.StudentID);
            await using var reader = await avgs.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Result.SubjectAvgs.Add(new StudentReportSubjectAvgDto
                {
                    SubjectName = NullString(reader["SubjectName"]) ?? "",
                    AvgMark = ToDec(reader["Sub_Avg"]),
                    Position = ToDec(reader["Sub_Position"])
                });
            }
        }
        catch (SqlException)
        {
        }

        var hasCumulative = await TableExistsAsync(con, "Exam_Cumulative_Student", cancellationToken);
        var passSql = hasCumulative
            ? """
(SELECT ROUND(100 * SUM(CASE WHEN t.PassStatus_Student = 'P' THEN 1 ELSE 0 END) / NULLIF(COUNT(t.StudentID), 0), 2, 0)
 FROM (
   SELECT StudentID, PassStatus_Student FROM Exam_Result_of_Student
   WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID
   UNION ALL
   SELECT StudentID, PassStatus_Student FROM Exam_Cumulative_Student WHERE StudentID = @StudentID
 ) AS t)
"""
            : """
(SELECT ROUND(100 * SUM(CASE WHEN PassStatus_Student = 'P' THEN 1 ELSE 0 END) / NULLIF(COUNT(StudentID), 0), 2, 0)
 FROM Exam_Result_of_Student
 WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID)
""";
        try
        {
            await using var summary = new SqlCommand($"""
SELECT AVG(CAST(Position_InExam_Class AS int)) AS Average_Position_Class,
       ROUND(AVG(Student_Point), 2, 0) AS Average_Point,
       (SELECT ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0)
        FROM Exam_Result_of_Subject
        INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
        WHERE Exam_Result_of_Student.StudentPublishStatus = N'Pub'
          AND Exam_Result_of_Student.StudentID = @StudentID) AS Average_ObtainedMarkofSubject,
       {passSql} AS Success_Percentage
FROM Exam_Result_of_Student
WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID
""", con);
            summary.Parameters.AddWithValue("@StudentID", dto.StudentID);
            await using var reader = await summary.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                dto.Result.AvgPosition = ToDecN(reader["Average_Position_Class"]);
                dto.Result.AvgPoint = ToDecN(reader["Average_Point"]);
                dto.Result.AvgMark = ToDecN(reader["Average_ObtainedMarkofSubject"]);
                dto.Result.PassPercent = ToDecN(reader["Success_Percentage"]);
            }
        }
        catch (SqlException)
        {
        }

        try
        {
            var sessionPass = hasCumulative
                ? """
(SELECT EducationYearID,
        ROUND(100 * SUM(CASE WHEN t.PassStatus_Student = 'P' THEN 1 ELSE 0 END) / NULLIF(COUNT(StudentID), 0), 2, 0) AS Success_Percentage
 FROM (
   SELECT EducationYearID, StudentID, PassStatus_Student FROM Exam_Result_of_Student
   WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID
   UNION ALL
   SELECT EducationYearID, StudentID, PassStatus_Student FROM Exam_Cumulative_Student WHERE StudentID = @StudentID
 ) AS t
 GROUP BY EducationYearID)
"""
                : """
(SELECT EducationYearID,
        ROUND(100 * SUM(CASE WHEN PassStatus_Student = 'P' THEN 1 ELSE 0 END) / NULLIF(COUNT(StudentID), 0), 2, 0) AS Success_Percentage
 FROM Exam_Result_of_Student
 WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID
 GROUP BY EducationYearID)
""";
            await using var sessions = new SqlCommand($"""
SELECT Education_Year.EducationYear,
       T_AP.Average_Position_Class,
       T_AP.Average_Point,
       T_S.Success_Percentage,
       T_B.Average_ObtainedMarkofSubject
FROM (
  SELECT EducationYearID,
         AVG(CAST(Position_InExam_Class AS int)) AS Average_Position_Class,
         ROUND(AVG(Student_Point), 2, 0) AS Average_Point
  FROM Exam_Result_of_Student
  WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID
  GROUP BY EducationYearID
) AS T_AP
INNER JOIN {sessionPass} AS T_S ON T_AP.EducationYearID = T_S.EducationYearID
INNER JOIN (
  SELECT Exam_Result_of_Student.EducationYearID,
         ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Average_ObtainedMarkofSubject
  FROM Exam_Result_of_Subject
  INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
  WHERE Exam_Result_of_Student.StudentPublishStatus = N'Pub'
    AND Exam_Result_of_Student.StudentID = @StudentID
  GROUP BY Exam_Result_of_Student.EducationYearID
) AS T_B ON T_AP.EducationYearID = T_B.EducationYearID
INNER JOIN Education_Year ON T_AP.EducationYearID = Education_Year.EducationYearID
ORDER BY Education_Year.StartDate
""", con);
            sessions.Parameters.AddWithValue("@StudentID", dto.StudentID);
            await using var reader = await sessions.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Result.Sessions.Add(new StudentReportSessionDto
                {
                    EducationYear = NullString(reader["EducationYear"]) ?? "",
                    AvgPosition = ToDecN(reader["Average_Position_Class"]),
                    AvgPoint = ToDecN(reader["Average_Point"]),
                    PassPercent = ToDecN(reader["Success_Percentage"]),
                    AvgMark = ToDecN(reader["Average_ObtainedMarkofSubject"])
                });
            }
        }
        catch (SqlException)
        {
        }

        try
        {
            await using var exams = new SqlCommand("""
SELECT Exam_Name.ExamName,
       Exam_Result_of_Student.Student_Grade,
       Exam_Result_of_Student.Student_Point,
       CAST(Exam_Result_of_Student.Position_InExam_Class AS int) AS Position_InExam_Class,
       Exam_Result_of_Student.ObtainedMark_ofStudent,
       Exam_Result_of_Student.TotalMark_ofStudent,
       Exam_Result_of_Student.ObtainedPercentage_ofStudent,
       Exam_Result_of_Student.PassStatus_Student
FROM Exam_Result_of_Student
INNER JOIN Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID
WHERE Exam_Result_of_Student.StudentID = @StudentID
  AND Exam_Result_of_Student.StudentPublishStatus = N'Pub'
  AND Exam_Result_of_Student.EducationYearID = @EducationYearID
ORDER BY Exam_Name.ExamID
""", con);
            exams.Parameters.AddWithValue("@StudentID", dto.StudentID);
            exams.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            await using var reader = await exams.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Result.Exams.Add(new StudentReportExamDto
                {
                    ExamName = NullString(reader["ExamName"]) ?? "",
                    Grade = NullString(reader["Student_Grade"]),
                    Point = ToDecN(reader["Student_Point"]),
                    Position = ToIntN(reader["Position_InExam_Class"]),
                    Obtained = ToDecN(reader["ObtainedMark_ofStudent"]),
                    Total = ToDecN(reader["TotalMark_ofStudent"]),
                    Percent = ToDecN(reader["ObtainedPercentage_ofStudent"]),
                    PassStatus = NullString(reader["PassStatus_Student"])
                });
            }
        }
        catch (SqlException)
        {
        }
    }

    private static async Task FillAttendanceAsync(
        SqlConnection con, SessionSnapshot session, StudentReportDto dto, CancellationToken cancellationToken)
    {
        var filled = false;
        if (await ObjectExistsAsync(con, "dbo.F_Stu_WorkingDay", cancellationToken)
            && await ObjectExistsAsync(con, "dbo.F_Stu_Attendance_Summary", cancellationToken))
        {
            try
            {
                await using var cmd = new SqlCommand("""
SELECT dbo.F_Stu_WorkingDay(@SchoolID, @EducationYearID, @ClassID, NULL, NULL) AS WorkingDay,
       dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Pre', NULL, NULL) AS Pre,
       dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Abs', NULL, NULL) AS Abs,
       dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Late', NULL, NULL) AS Late,
       dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Leave', NULL, NULL) AS Leave,
       dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Bunk', NULL, NULL) AS Bunk,
       dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Late Abs', NULL, NULL) AS LateAbs
""", con);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                cmd.Parameters.AddWithValue("@ClassID", dto.ClassID);
                cmd.Parameters.AddWithValue("@StudentClassID", dto.StudentClassID);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    dto.Attendance.WorkingDays = ToInt(reader["WorkingDay"]);
                    dto.Attendance.Present = ToInt(reader["Pre"]);
                    dto.Attendance.Absent = ToInt(reader["Abs"]);
                    dto.Attendance.Late = ToInt(reader["Late"]);
                    dto.Attendance.Leave = ToInt(reader["Leave"]);
                    dto.Attendance.Bunk = ToInt(reader["Bunk"]);
                    dto.Attendance.LateAbsent = ToInt(reader["LateAbs"]);
                    filled = true;
                }
            }
            catch (SqlException)
            {
            }
        }

        if (!filled && await TableExistsAsync(con, "Attendance_Record", cancellationToken))
        {
            try
            {
                await using var work = new SqlCommand("""
SELECT COUNT(DISTINCT CAST(AttendanceDate AS date))
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
""", con);
                work.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                work.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                work.Parameters.AddWithValue("@ClassID", dto.ClassID);
                dto.Attendance.WorkingDays = ToInt(await work.ExecuteScalarAsync(cancellationToken));

                await using var counts = new SqlCommand("""
SELECT LTRIM(RTRIM(ISNULL(Attendance, N''))) AS Attendance, COUNT(*) AS Cnt
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID
GROUP BY LTRIM(RTRIM(ISNULL(Attendance, N'')))
""", con);
                counts.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                counts.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                counts.Parameters.AddWithValue("@StudentClassID", dto.StudentClassID);
                await using var reader = await counts.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var key = (NullString(reader["Attendance"]) ?? "").Trim();
                    var n = ToInt(reader["Cnt"]);
                    if (key.Equals("Pre", StringComparison.OrdinalIgnoreCase))
                        dto.Attendance.Present = n;
                    else if (key.Equals("Abs", StringComparison.OrdinalIgnoreCase))
                        dto.Attendance.Absent = n;
                    else if (key.Equals("Late", StringComparison.OrdinalIgnoreCase))
                        dto.Attendance.Late = n;
                    else if (key.Equals("Leave", StringComparison.OrdinalIgnoreCase))
                        dto.Attendance.Leave = n;
                    else if (key.Equals("Bunk", StringComparison.OrdinalIgnoreCase))
                        dto.Attendance.Bunk = n;
                    else if (key.Equals("Late Abs", StringComparison.OrdinalIgnoreCase)
                             || key.Equals("LateAbs", StringComparison.OrdinalIgnoreCase)
                             || key.Equals("Late_Abs", StringComparison.OrdinalIgnoreCase))
                        dto.Attendance.LateAbsent = n;
                }
            }
            catch (SqlException)
            {
            }
        }

        if (!await TableExistsAsync(con, "Attendance_Record", cancellationToken))
            return;

        try
        {
            await using var days = new SqlCommand("""
SELECT TOP 400 CAST(AttendanceDate AS date) AS AttendanceDate, Attendance, EntryTime
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID
ORDER BY AttendanceDate DESC
""", con);
            days.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            days.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            days.Parameters.AddWithValue("@StudentClassID", dto.StudentClassID);
            await using var reader = await days.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var date = ReadDate(reader["AttendanceDate"]);
                if (date is null)
                    continue;
                dto.Attendance.Days.Add(new StudentReportAttendanceDayDto
                {
                    Date = date.Value,
                    Attendance = NullString(reader["Attendance"]) ?? "",
                    EntryTime = ReadTime(reader["EntryTime"])
                });
            }
        }
        catch (SqlException)
        {
        }
    }

    private static async Task FillSubjectsAsync(
        SqlConnection con, SessionSnapshot session, StudentReportDto dto, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(con, "StudentRecord", cancellationToken))
            return;
        try
        {
            await using var cmd = new SqlCommand("""
SELECT Subject.SubjectName, StudentRecord.SubjectType
FROM dbo.StudentRecord
INNER JOIN dbo.Subject ON StudentRecord.SubjectID = Subject.SubjectID
WHERE StudentRecord.EducationYearID = @EducationYearID
  AND StudentRecord.SchoolID = @SchoolID
  AND StudentRecord.StudentClassID = @StudentClassID
ORDER BY Subject.SN, Subject.SubjectName
""", con);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@StudentClassID", dto.StudentClassID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Subjects.Add(new StudentReportSubjectDto
                {
                    SubjectName = NullString(reader["SubjectName"]) ?? "",
                    SubjectType = NullString(reader["SubjectType"]) ?? "Compulsory"
                });
            }
        }
        catch (SqlException)
        {
        }
    }

    private static async Task FillAccountsAsync(
        SqlConnection con, SessionSnapshot session, StudentReportDto dto, CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(con, "Income_PayOrder", cancellationToken))
            return;

        const string dueExpr = """
CASE WHEN Income_PayOrder.EndDate < GETDATE() - 1
     THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0)
          - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0)
          - ISNULL(Income_PayOrder.LateFee_Discount, 0)
     ELSE ISNULL(Income_PayOrder.Amount, 0) - ISNULL(Income_PayOrder.Discount, 0)
          - ISNULL(Income_PayOrder.PaidAmount, 0)
END
""";
        const string payCols = $"""
Education_Year.EducationYear,
CreateClass.Class,
Income_Roles.Role,
Income_PayOrder.PayFor,
Income_PayOrder.StartDate,
Income_PayOrder.EndDate,
ISNULL(Income_PayOrder.Amount, 0) AS Amount,
ISNULL(Income_PayOrder.Discount, 0) AS Discount,
ISNULL(Income_PayOrder.PaidAmount, 0) AS PaidAmount,
{dueExpr} AS Due,
ISNULL(Income_PayOrder.LateFee, 0) AS LateFee,
ISNULL(Income_PayOrder.LateFee_Discount, 0) AS LateFeeDiscount,
Income_PayOrder.LastPaidDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
LEFT OUTER JOIN dbo.Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID
LEFT OUTER JOIN dbo.CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID
""";

        dto.Accounts.Due = await QueryPayOrdersAsync(con, $"""
SELECT {payCols}
INNER JOIN dbo.Student ON Income_PayOrder.StudentID = Student.StudentID
WHERE Income_PayOrder.Status = N'Due'
  AND Income_PayOrder.StudentID = @StudentID
  AND Student.Status = N'Active'
  AND Income_PayOrder.SchoolID = @SchoolID
ORDER BY Income_PayOrder.EndDate
""", session.SchoolID, dto.StudentID, 0, 0, cancellationToken);

        dto.Accounts.CurrentDue = await QueryPayOrdersAsync(con, $"""
SELECT {payCols}
WHERE Income_PayOrder.Status = N'Due'
  AND Income_PayOrder.EndDate < GETDATE()
  AND Income_PayOrder.StudentID = @StudentID
ORDER BY Income_PayOrder.EndDate
""", session.SchoolID, dto.StudentID, 0, 0, cancellationToken);

        dto.Accounts.Paid = await QueryPayOrdersAsync(con, $"""
SELECT {payCols}
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.StudentClassID = @StudentClassID
  AND Income_PayOrder.PaidAmount <> 0
ORDER BY Income_PayOrder.LastPaidDate DESC
""", session.SchoolID, dto.StudentID, session.EducationYearID, dto.StudentClassID, cancellationToken);

        dto.Accounts.AllPayOrders = await QueryPayOrdersAsync(con, $"""
SELECT {payCols}
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.StudentClassID = @StudentClassID
ORDER BY Income_PayOrder.StartDate
""", session.SchoolID, dto.StudentID, session.EducationYearID, dto.StudentClassID, cancellationToken);

        dto.Accounts.TotalDue = dto.Accounts.Due.Sum(x => x.Due);
        dto.Accounts.CurrentDueTotal = dto.Accounts.CurrentDue.Sum(x => x.Due);
        dto.Accounts.TotalPaid = dto.Accounts.Paid.Sum(x => x.PaidAmount);
        dto.Accounts.TotalFee = dto.Accounts.AllPayOrders.Sum(x => x.Amount);
        dto.Accounts.TotalDiscount = dto.Accounts.AllPayOrders.Sum(x => x.Discount);
        dto.Accounts.TotalLateFee = dto.Accounts.AllPayOrders.Sum(x => x.LateFee);

        try
        {
            await using var rec = new SqlCommand("""
SELECT MoneyReceipt_SN, PaidDate, TotalAmount
FROM dbo.Income_MoneyReceipt
WHERE StudentID = @StudentID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID
ORDER BY PaidDate DESC
""", con);
            rec.Parameters.AddWithValue("@StudentID", dto.StudentID);
            rec.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            rec.Parameters.AddWithValue("@StudentClassID", dto.StudentClassID);
            await using var reader = await rec.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Accounts.Receipts.Add(new StudentReportReceiptDto
                {
                    ReceiptNo = NullString(reader["MoneyReceipt_SN"]) ?? "",
                    PaidDate = ReadDate(reader["PaidDate"]),
                    Amount = ToDec(reader["TotalAmount"])
                });
            }
        }
        catch (SqlException)
        {
        }

        try
        {
            await using var conc = new SqlCommand("""
SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.Amount, Income_PayOrder.LateFee,
       Income_PayOrder.Total_Discount, Income_PayOrder.StartDate, Income_PayOrder.EndDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.StudentClassID = @StudentClassID
  AND Income_PayOrder.Total_Discount <> 0
ORDER BY Income_PayOrder.StartDate
""", con);
            conc.Parameters.AddWithValue("@StudentID", dto.StudentID);
            conc.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            conc.Parameters.AddWithValue("@StudentClassID", dto.StudentClassID);
            await using var reader = await conc.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Accounts.Concession.Add(new StudentReportConcessionDto
                {
                    Role = NullString(reader["Role"]) ?? "",
                    PayFor = NullString(reader["PayFor"]) ?? "",
                    Amount = ToDec(reader["Amount"]),
                    LateFee = ToDec(reader["LateFee"]),
                    Discount = ToDec(reader["Total_Discount"]),
                    StartDate = ReadDate(reader["StartDate"]),
                    EndDate = ReadDate(reader["EndDate"])
                });
            }
        }
        catch (SqlException)
        {
        }
    }

    private static async Task<List<StudentReportPayOrderDto>> QueryPayOrdersAsync(
        SqlConnection con, string sql, int schoolId, int studentId, int yearId, int studentClassId,
        CancellationToken cancellationToken)
    {
        var rows = new List<StudentReportPayOrderDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            cmd.Parameters.AddWithValue("@EducationYearID", yearId);
            cmd.Parameters.AddWithValue("@StudentClassID", studentClassId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new StudentReportPayOrderDto
                {
                    Session = NullString(reader["EducationYear"]),
                    ClassName = NullString(reader["Class"]),
                    Role = NullString(reader["Role"]) ?? "",
                    PayFor = NullString(reader["PayFor"]) ?? "",
                    StartDate = ReadDate(reader["StartDate"]),
                    EndDate = ReadDate(reader["EndDate"]),
                    Amount = ToDec(reader["Amount"]),
                    Discount = ToDec(reader["Discount"]),
                    PaidAmount = ToDec(reader["PaidAmount"]),
                    Due = ToDec(reader["Due"]),
                    LateFee = ToDec(reader["LateFee"]),
                    LateFeeDiscount = ToDec(reader["LateFeeDiscount"]),
                    LastPaidDate = ReadDate(reader["LastPaidDate"])
                });
            }
        }
        catch (SqlException)
        {
        }

        return rows;
    }

    private static void AddClassFilters(SqlCommand cmd, int classId, int groupId, int sectionId, int shiftId)
    {
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SubjectGroupID", groupId);
        cmd.Parameters.AddWithValue("@SectionID", sectionId);
        cmd.Parameters.AddWithValue("@ShiftID", shiftId);
    }

    private static void AddNullableId(SqlCommand cmd, string name, int id) =>
        cmd.Parameters.AddWithValue(name, id > 0 ? id : DBNull.Value);

    private static List<string> ParseIds(string? ids) =>
        (ids ?? "")
            .Split([',', ' ', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static StudentInfoResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static StudentAccountResult AccountFail(string error) => new() { Succeeded = false, Error = error };

    private static string? NullString(object? value)
    {
        var text = value is null or DBNull ? null : value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static DateTime? ReadDate(object? value)
    {
        if (value is null or DBNull)
            return null;
        var date = Convert.ToDateTime(value);
        return date.Year < 1900 ? null : date;
    }

    private static int ToInt(object? value)
    {
        if (value is null or DBNull)
            return 0;
        if (value is int i)
            return i;
        if (value is long l)
            return (int)l;
        if (value is decimal d)
            return (int)d;
        return int.TryParse(Convert.ToString(value), out var n) ? n : 0;
    }

    private static int? ToIntN(object? value) => value is null or DBNull ? null : ToInt(value);

    private static decimal ToDec(object? value) =>
        value is null or DBNull ? 0 : Convert.ToDecimal(value);

    private static decimal? ToDecN(object? value) =>
        value is null or DBNull ? null : Convert.ToDecimal(value);

    private static string? ReadTime(object? value)
    {
        if (value is null or DBNull)
            return null;
        if (value is TimeSpan span)
            return span.ToString(@"hh\:mm");
        if (value is DateTime date)
            return date.ToString("HH:mm");
        var text = Convert.ToString(value);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static async Task<bool> TableExistsAsync(
        SqlConnection con, string tableName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(@Name, 'U') IS NULL THEN 0 ELSE 1 END", con);
        cmd.Parameters.AddWithValue("@Name", "dbo." + tableName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    private static async Task<bool> ObjectExistsAsync(
        SqlConnection con, string name, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(@Name) IS NULL THEN 0 ELSE 1 END", con);
        cmd.Parameters.AddWithValue("@Name", name);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    private static string? ToDataUrl(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
