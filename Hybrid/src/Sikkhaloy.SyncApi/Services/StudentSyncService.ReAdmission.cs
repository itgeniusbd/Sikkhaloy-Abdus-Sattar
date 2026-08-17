using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class StudentSyncService
{
    public async Task<IReadOnlyList<ReAdmissionCandidateDto>> ListReAdmissionCandidatesAsync(
        SessionSnapshot session,
        int educationYearId,
        int classId,
        int sectionId,
        int subjectGroupId,
        int shiftId,
        CancellationToken cancellationToken)
    {
        if (educationYearId <= 0 || classId <= 0)
            return [];

        const string sql = @"
SELECT
    s.StudentID, s.ID, s.StudentsName, s.Gender, s.FathersName,
    s.SMSPhoneNo, s.FatherPhoneNumber, s.MotherPhoneNumber, s.GuardianPhoneNumber,
    sc.RollNo, sc.StudentClassID, sc.EducationYearID, sc.ClassID,
    sc.SectionID, sc.ShiftID, sc.SubjectGroupID,
    cc.Class, cs.Section, sh.Shift, sg.SubjectGroup, ey.EducationYear
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s
    ON s.StudentID = sc.StudentID
   AND s.SchoolID = sc.SchoolID
INNER JOIN dbo.Education_Year AS ey
    ON ey.EducationYearID = sc.EducationYearID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.CreateSection AS cs ON cs.SectionID = sc.SectionID
LEFT JOIN dbo.CreateShift AS sh ON sh.ShiftID = sc.ShiftID
LEFT JOIN dbo.CreateSubjectGroup AS sg ON sg.SubjectGroupID = sc.SubjectGroupID
WHERE sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @EducationYearID
  AND sc.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(sc.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(sc.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(sc.ShiftID, 0) = @ShiftID)
  AND s.Status = N'Active'
  AND sc.Class_Status IS NULL
ORDER BY TRY_CAST(REPLACE(REPLACE(ISNULL(sc.RollNo, N''), N'$', N''), N',', N'') AS INT), sc.RollNo";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SectionID", sectionId);
        cmd.Parameters.AddWithValue("@SubjectGroupID", subjectGroupId);
        cmd.Parameters.AddWithValue("@ShiftID", shiftId);

        var items = new List<ReAdmissionCandidateDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(ReadCandidate(reader));
        return items;
    }

    public async Task<ReAdmissionAssignDto> GetReAdmissionAssignAsync(
        SessionSnapshot session, int studentId, int fromYearId, CancellationToken cancellationToken)
    {
        if (studentId <= 0)
            return new ReAdmissionAssignDto { Error = "readm.notFound" };

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        var student = await ReadCandidateAsync(con, session.SchoolID, studentId, fromYearId, cancellationToken);
        if (student is null)
            return new ReAdmissionAssignDto { Error = "readm.notFound" };

        return new ReAdmissionAssignDto
        {
            Student = student,
            TargetYears = await ListEligibleYearsAsync(con, session.SchoolID, studentId, cancellationToken)
        };
    }

    public async Task<IReadOnlyList<ReAdmissionSubjectDto>> ListReAdmissionSubjectsAsync(
        SessionSnapshot session, int classId, int subjectGroupId, CancellationToken cancellationToken)
    {
        if (classId <= 0)
            return [];

        const string sql = @"
SELECT Subject.SubjectID, Subject.SubjectName, SubjectForGroup.SubjectType,
       CAST(CASE WHEN SubjectForGroup.SubjectType = N'Compulsory' THEN 1 ELSE 0 END AS BIT) AS Selected
FROM dbo.Subject
INNER JOIN dbo.SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID
WHERE Subject.SchoolID = @SchoolID
  AND SubjectForGroup.ClassID = @ClassID
  AND SubjectForGroup.SubjectGroupID = @SubjectGroupID
ORDER BY SubjectForGroup.SubjectType, Subject.SubjectName";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SubjectGroupID", subjectGroupId < 0 ? 0 : subjectGroupId);

        var items = new List<ReAdmissionSubjectDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ReAdmissionSubjectDto
            {
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                SubjectName = ReadString(reader, "SubjectName") ?? "",
                SubjectType = ReadString(reader, "SubjectType") ?? "Compulsory",
                Selected = ReadBool(reader, "Selected") == true
            });
        }

        return items;
    }

    public async Task<ReAdmissionResult> FinishReAdmissionAsync(
        SessionSnapshot session, ReAdmissionRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0 || request.FromStudentClassID <= 0)
            return Fail("readm.notFound");
        if (request.ToEducationYearID <= 0)
            return Fail("readm.needTarget");
        if (request.ClassID <= 0)
            return Fail("readm.needClass");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!await OwnsStudentClassAsync(con, tx, session.SchoolID, request.StudentID, request.FromStudentClassID, cancellationToken))
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("readm.notFound");
            }

            if (await StudentClassExistsAsync(con, tx, session.SchoolID, request.StudentID, request.ToEducationYearID, cancellationToken))
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("readm.already");
            }

            var sectionId = request.SectionID > 0 ? request.SectionID : 0;
            var shiftId = request.ShiftID > 0 ? request.ShiftID : 0;
            var groupId = request.SubjectGroupID > 0 ? request.SubjectGroupID : 0;
            var rollNo = string.IsNullOrWhiteSpace(request.RollNo) ? (object)DBNull.Value : request.RollNo.Trim();

            const string insertSql = @"
INSERT INTO dbo.StudentsClass
    (SchoolID, RegistrationID, StudentID, ClassID, SectionID, ShiftID, SubjectGroupID, RollNo, EducationYearID, Date, Is_New)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, GETDATE(), 0);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int studentClassId;
            await using (var insert = new SqlCommand(insertSql, con, tx))
            {
                insert.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                insert.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                insert.Parameters.AddWithValue("@StudentID", request.StudentID);
                insert.Parameters.AddWithValue("@ClassID", request.ClassID);
                insert.Parameters.AddWithValue("@SectionID", sectionId);
                insert.Parameters.AddWithValue("@ShiftID", shiftId);
                insert.Parameters.AddWithValue("@SubjectGroupID", groupId);
                insert.Parameters.AddWithValue("@RollNo", rollNo);
                insert.Parameters.AddWithValue("@EducationYearID", request.ToEducationYearID);
                studentClassId = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
            }

            foreach (var subject in request.Subjects ?? [])
            {
                if (subject.SubjectID <= 0)
                    continue;
                const string recordSql = @"
IF NOT EXISTS (
    SELECT 1 FROM dbo.StudentRecord
    WHERE StudentID = @StudentID AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID
      AND StudentClassID = @StudentClassID AND SubjectID = @SubjectID)
INSERT INTO dbo.StudentRecord
    (SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)";
                await using var record = new SqlCommand(recordSql, con, tx);
                record.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                record.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                record.Parameters.AddWithValue("@StudentID", request.StudentID);
                record.Parameters.AddWithValue("@StudentClassID", studentClassId);
                record.Parameters.AddWithValue("@SubjectID", subject.SubjectID);
                record.Parameters.AddWithValue("@EducationYearID", request.ToEducationYearID);
                record.Parameters.AddWithValue("@SubjectType",
                    string.IsNullOrWhiteSpace(subject.SubjectType) ? "Compulsory" : subject.SubjectType.Trim());
                await record.ExecuteNonQueryAsync(cancellationToken);
            }

            const string statusSql = @"
UPDATE dbo.StudentsClass
SET Class_Status = N'Re-Admitted'
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID AND StudentID = @StudentID";
            await using (var status = new SqlCommand(statusSql, con, tx))
            {
                status.Parameters.AddWithValue("@StudentClassID", request.FromStudentClassID);
                status.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                status.Parameters.AddWithValue("@StudentID", request.StudentID);
                await status.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail("readm.failed");
        }

        StudentDto? student = null;
        try
        {
            student = await LoadStudentDtoAsync(con, session, request.StudentID, request.ToEducationYearID, cancellationToken);
            if (student is not null)
                await TryLogChangeAsync(con, session, student.LocalId, request.StudentID, "Update", session.DeviceId, cancellationToken);
        }
        catch (Exception)
        {
        }

        return new ReAdmissionResult { Succeeded = true, Student = student };
    }

    private static ReAdmissionResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<bool> OwnsStudentClassAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int studentId, int studentClassId, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1 StudentClassID
FROM dbo.StudentsClass
WHERE StudentClassID = @StudentClassID AND StudentID = @StudentID AND SchoolID = @SchoolID";
        await using var cmd = new SqlCommand(sql, con, tx);
        cmd.Parameters.AddWithValue("@StudentClassID", studentClassId);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null and not DBNull;
    }

    private static async Task<bool> StudentClassExistsAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int studentId, int educationYearId, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1 StudentClassID
FROM dbo.StudentsClass
WHERE StudentID = @StudentID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID";
        await using var cmd = new SqlCommand(sql, con, tx);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null and not DBNull;
    }

    private static async Task<List<EducationYearDto>> ListEligibleYearsAsync(
        SqlConnection con, int schoolId, int studentId, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT EducationYearID, EducationYear, ISNULL(SN, EducationYearID) AS SortOrder
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID
  AND EducationYearID NOT IN (
        SELECT DISTINCT EducationYearID
        FROM dbo.StudentsClass
        WHERE StudentID = @StudentID AND SchoolID = @SchoolID)
  AND ISNULL(SN, EducationYearID) > ISNULL((
        SELECT MAX(ISNULL(ey.SN, ey.EducationYearID))
        FROM dbo.StudentsClass AS sc
        INNER JOIN dbo.Education_Year AS ey ON sc.EducationYearID = ey.EducationYearID
        WHERE sc.StudentID = @StudentID AND sc.SchoolID = @SchoolID
      ), 0)
ORDER BY ISNULL(SN, EducationYearID), EducationYearID";

        var items = new List<EducationYearDto>();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EducationYearDto
            {
                EducationYearID = Convert.ToInt32(reader["EducationYearID"]),
                Name = reader["EducationYear"]?.ToString() ?? "",
                SortOrder = Convert.ToInt32(reader["SortOrder"])
            });
        }

        return items;
    }

    private static async Task<ReAdmissionCandidateDto?> ReadCandidateAsync(
        SqlConnection con, int schoolId, int studentId, int educationYearId, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    s.StudentID, s.ID, s.StudentsName, s.Gender, s.FathersName,
    s.SMSPhoneNo, s.FatherPhoneNumber, s.MotherPhoneNumber, s.GuardianPhoneNumber,
    sc.RollNo, sc.StudentClassID, sc.EducationYearID, sc.ClassID,
    sc.SectionID, sc.ShiftID, sc.SubjectGroupID,
    cc.Class, cs.Section, sh.Shift, sg.SubjectGroup, ey.EducationYear
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s
    ON s.StudentID = sc.StudentID
   AND s.SchoolID = sc.SchoolID
INNER JOIN dbo.Education_Year AS ey
    ON ey.EducationYearID = sc.EducationYearID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.CreateSection AS cs ON cs.SectionID = sc.SectionID
LEFT JOIN dbo.CreateShift AS sh ON sh.ShiftID = sc.ShiftID
LEFT JOIN dbo.CreateSubjectGroup AS sg ON sg.SubjectGroupID = sc.SubjectGroupID
WHERE sc.SchoolID = @SchoolID
  AND sc.StudentID = @StudentID
  AND (@EducationYearID = 0 OR sc.EducationYearID = @EducationYearID)";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return ReadCandidate(reader);
    }

    private static async Task<StudentDto?> LoadStudentDtoAsync(
        SqlConnection con, SessionSnapshot session, int studentId, int educationYearId, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1
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
  AND s.StudentID = @StudentID";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new StudentDto
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
            RegistrationID = Convert.ToInt32(reader["RegistrationID"]),
            SchoolID = session.SchoolID,
            EducationYearID = educationYearId,
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
    }

    private static ReAdmissionCandidateDto ReadCandidate(SqlDataReader reader) => new()
    {
        StudentID = Convert.ToInt32(reader["StudentID"]),
        StudentClassID = Convert.ToInt32(reader["StudentClassID"]),
        EducationYearID = Convert.ToInt32(reader["EducationYearID"]),
        EducationYear = ReadString(reader, "EducationYear") ?? "",
        StudentCode = ReadString(reader, "ID") ?? "",
        StudentsName = ReadString(reader, "StudentsName") ?? "",
        Gender = ReadString(reader, "Gender"),
        FathersName = ReadString(reader, "FathersName"),
        SMSPhoneNo = ReadString(reader, "SMSPhoneNo"),
        FatherPhoneNumber = ReadString(reader, "FatherPhoneNumber"),
        MotherPhoneNumber = ReadString(reader, "MotherPhoneNumber"),
        GuardianPhoneNumber = ReadString(reader, "GuardianPhoneNumber"),
        RollNo = ReadString(reader, "RollNo"),
        ClassID = ReadInt(reader, "ClassID"),
        ClassName = ReadString(reader, "Class"),
        SectionID = ReadInt(reader, "SectionID"),
        SectionName = ReadString(reader, "Section"),
        ShiftID = ReadInt(reader, "ShiftID"),
        ShiftName = ReadString(reader, "Shift"),
        SubjectGroupID = ReadInt(reader, "SubjectGroupID"),
        GroupName = ReadString(reader, "SubjectGroup")
    };
}
