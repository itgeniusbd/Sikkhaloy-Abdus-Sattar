using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed class StudentManagementService
{
    private readonly EduConnectionFactory _connections;

    public StudentManagementService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<SmStudentRowDto>> ListClassStudentsAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, string? studentCode,
        int? subjectId, CancellationToken cancellationToken)
    {
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0 && classId <= 0)
            return [];

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var sql = code.Length > 0 ? ClassStudentByIdSql : ClassStudentSql;
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (code.Length > 0)
            cmd.Parameters.AddWithValue("@ID", code);
        else
            AddClassFilters(cmd, classId, groupId, sectionId, shiftId);

        var items = new List<SmStudentRowDto>();
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                items.Add(ReadRow(reader));
        }

        if (subjectId is > 0)
        {
            foreach (var row in items)
            {
                await using var sub = new SqlCommand("""
SELECT SubjectType FROM dbo.StudentRecord
WHERE SubjectID = @SubjectID AND StudentClassID = @StudentClassID
  AND EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
                sub.Parameters.AddWithValue("@SubjectID", subjectId.Value);
                sub.Parameters.AddWithValue("@StudentClassID", row.StudentClassID);
                sub.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                sub.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                var type = await sub.ExecuteScalarAsync(cancellationToken);
                if (type is not null and not DBNull)
                {
                    row.HasSubject = true;
                    row.Selected = true;
                    row.SubjectType = type.ToString() ?? "Compulsory";
                }
            }
        }

        await AttachPhotosAsync(con, session.SchoolID, items, cancellationToken);
        return items;
    }

    public async Task<StudentPlacementDto?> FindStudentAsync(
        SessionSnapshot session, int studentId, CancellationToken cancellationToken)
    {
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
WHERE StudentsClass.StudentID = @StudentID
  AND StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.Class_Status IS NULL
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new StudentPlacementDto
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
    }

    public async Task<StudentInfoResult> ChangeClassAsync(
        SessionSnapshot session, ChangeClassRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0 || request.OldStudentClassID <= 0 || request.ClassID <= 0)
            return Fail("sm.needStudent");
        var items = request.Subjects.Where(x => x.SubjectID > 0).ToList();
        if (items.Count == 0)
            return Fail("si.needSubject");

        var status = string.Equals(request.ClassStatus, "Demotion", StringComparison.OrdinalIgnoreCase)
            ? "Demotion"
            : "Promotion";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var check = new SqlCommand("""
SELECT COUNT(*) FROM dbo.StudentsClass
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
  AND EducationYearID = @EducationYearID AND ClassID = @ClassID
""", con, tx))
            {
                check.Parameters.AddWithValue("@StudentID", request.StudentID);
                check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                check.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                check.Parameters.AddWithValue("@ClassID", request.ClassID);
                if (ToInt(await check.ExecuteScalarAsync(cancellationToken)) > 0)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("sm.classExists");
                }
            }

            int newId;
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.StudentsClass
    (SchoolID, RegistrationID, StudentID, ClassID, SectionID, ShiftID, SubjectGroupID, RollNo, EducationYearID, Date)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx))
            {
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@StudentID", request.StudentID);
                ins.Parameters.AddWithValue("@ClassID", request.ClassID);
                AddNullableId(ins, "@SectionID", request.SectionID);
                AddNullableId(ins, "@ShiftID", request.ShiftID);
                AddNullableId(ins, "@SubjectGroupID", request.SubjectGroupID);
                ins.Parameters.AddWithValue("@RollNo", (object?)request.RollNo ?? DBNull.Value);
                ins.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                newId = ToInt(await ins.ExecuteScalarAsync(cancellationToken));
            }

            foreach (var item in items)
            {
                var type = string.Equals(item.SubjectType, "Optional", StringComparison.OrdinalIgnoreCase)
                    ? "Optional" : "Compulsory";
                await using var rec = new SqlCommand("""
INSERT INTO dbo.StudentRecord
    (SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)
""", con, tx);
                rec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                rec.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                rec.Parameters.AddWithValue("@StudentID", request.StudentID);
                rec.Parameters.AddWithValue("@StudentClassID", newId);
                rec.Parameters.AddWithValue("@SubjectID", item.SubjectID);
                rec.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                rec.Parameters.AddWithValue("@SubjectType", type);
                await rec.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var upd = new SqlCommand("""
UPDATE dbo.StudentsClass
SET EducationYearID = 0,
    New_StudentClassID = @New_StudentClassID,
    Promotion_Demotion_Year = @Promotion_Demotion_Year,
    Class_Status = @Class_Status
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID
""", con, tx))
            {
                upd.Parameters.AddWithValue("@New_StudentClassID", newId);
                upd.Parameters.AddWithValue("@Promotion_Demotion_Year", session.EducationYearID);
                upd.Parameters.AddWithValue("@Class_Status", status);
                upd.Parameters.AddWithValue("@StudentClassID", request.OldStudentClassID);
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await upd.ExecuteNonQueryAsync(cancellationToken);
            }

            if (!request.KeepPayOrder)
            {
                await using var delPay = new SqlCommand("""
DELETE FROM dbo.Income_PayOrder
WHERE SchoolID = @SchoolID AND StudentClassID = @StudentClassID AND PaidAmount = 0
""", con, tx);
                delPay.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                delPay.Parameters.AddWithValue("@StudentClassID", request.OldStudentClassID);
                await delPay.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using var keep = new SqlCommand("""
UPDATE dbo.Income_PayOrder
SET StudentClassID = @NewId, ClassID = @ClassID
WHERE SchoolID = @SchoolID
  AND EducationYearID = @EducationYearID
  AND StudentID = @StudentID
  AND StudentClassID = @OldId
  AND ISNULL(PaidAmount, 0) = 0
""", con, tx);
                keep.Parameters.AddWithValue("@NewId", newId);
                keep.Parameters.AddWithValue("@ClassID", request.ClassID);
                keep.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                keep.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                keep.Parameters.AddWithValue("@StudentID", request.StudentID);
                keep.Parameters.AddWithValue("@OldId", request.OldStudentClassID);
                await keep.ExecuteNonQueryAsync(cancellationToken);

                foreach (var table in new[]
                {
                    "Income_Discount_Record", "Income_LateFee_Change_Record", "Income_LateFee_Discount_Record"
                })
                {
                    await using var move = new SqlCommand($"""
UPDATE dbo.[{table}] SET StudentClassID = @NewId WHERE StudentClassID = @OldId
""", con, tx);
                    move.Parameters.AddWithValue("@NewId", newId);
                    move.Parameters.AddWithValue("@OldId", request.OldStudentClassID);
                    try
                    {
                        await move.ExecuteNonQueryAsync(cancellationToken);
                    }
                    catch (SqlException)
                    {
                    }
                }
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

    public async Task<StudentInfoResult> BulkChangeClassAsync(
        SessionSnapshot session, BulkChangeClassRequest? request, CancellationToken cancellationToken)
    {
        var students = (request?.Students ?? [])
            .Where(x => x.StudentID > 0 && x.OldStudentClassID > 0)
            .GroupBy(x => x.StudentID)
            .Select(g => g.First())
            .ToList();
        if (students.Count == 0)
            return Fail("sm.needStudents");
        if (request!.ClassID <= 0)
            return Fail("sm.needNewClass");
        var subjects = request.Subjects.Where(x => x.SubjectID > 0).ToList();
        if (subjects.Count == 0)
            return Fail("si.needSubject");

        var ok = 0;
        var failed = new List<string>();
        foreach (var stu in students)
        {
            var one = await ChangeClassAsync(session, new ChangeClassRequest
            {
                StudentID = stu.StudentID,
                OldStudentClassID = stu.OldStudentClassID,
                ClassID = request.ClassID,
                SubjectGroupID = request.SubjectGroupID,
                SectionID = request.SectionID,
                ShiftID = request.ShiftID,
                RollNo = stu.RollNo,
                ClassStatus = request.ClassStatus,
                KeepPayOrder = request.KeepPayOrder,
                Subjects = subjects
            }, cancellationToken);
            if (one.Succeeded)
                ok++;
            else
                failed.Add(string.IsNullOrWhiteSpace(stu.ID) ? stu.StudentsName : $"{stu.ID} {stu.StudentsName}".Trim());
        }

        if (ok == 0)
        {
            return new StudentInfoResult
            {
                Succeeded = false,
                Error = failed.Count > 0 ? "sm.classExistsSome" : "sm.needStudents",
                Detail = failed.Count > 0 ? string.Join(", ", failed) : null,
                Count = 0
            };
        }

        return new StudentInfoResult
        {
            Succeeded = true,
            Count = ok,
            Error = failed.Count > 0 ? "sm.classExistsSome" : null,
            Detail = failed.Count > 0 ? string.Join(", ", failed) : null
        };
    }

    public async Task<StudentInfoResult> BulkPlacementAsync(
        SessionSnapshot session, BulkPlacementRequest? request, CancellationToken cancellationToken)
    {
        var ids = (request?.StudentClassIDs ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return Fail("sm.needStudents");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var n = 0;
        foreach (var id in ids)
        {
            var sets = new List<string>();
            if (request!.UpdateSection)
                sets.Add("SectionID = @SectionID");
            if (request.UpdateShift)
                sets.Add("ShiftID = @ShiftID");
            if (request.UpdateGroup)
                sets.Add("SubjectGroupID = @SubjectGroupID");
            if (sets.Count == 0)
                continue;

            await using var cmd = new SqlCommand($"""
UPDATE dbo.StudentsClass
SET {string.Join(", ", sets)}
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
            if (request.UpdateSection)
                AddNullableId(cmd, "@SectionID", request.SectionID);
            if (request.UpdateShift)
                AddNullableId(cmd, "@ShiftID", request.ShiftID);
            if (request.UpdateGroup)
                AddNullableId(cmd, "@SubjectGroupID", request.SubjectGroupID);
            cmd.Parameters.AddWithValue("@StudentClassID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            n += await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return n > 0
            ? new StudentInfoResult { Succeeded = true, Count = n }
            : Fail("sm.needStudents");
    }

    public async Task<StudentInfoResult> SaveOneSubjectAsync(
        SessionSnapshot session, SaveOneSubjectRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.SubjectID <= 0 || request.Items.Count == 0)
            return Fail("si.needSubject");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        foreach (var item in request.Items)
        {
            if (item.Selected)
            {
                var type = string.Equals(item.SubjectType, "Optional", StringComparison.OrdinalIgnoreCase)
                    ? "Optional" : "Compulsory";
                await using var ins = new SqlCommand("""
IF NOT EXISTS (
    SELECT 1 FROM dbo.StudentRecord
    WHERE StudentClassID = @StudentClassID AND SubjectID = @SubjectID AND SchoolID = @SchoolID)
INSERT INTO dbo.StudentRecord
    (StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date)
VALUES
    (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())
ELSE
UPDATE dbo.StudentRecord SET SubjectType = @SubjectType
WHERE StudentClassID = @StudentClassID AND SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con);
                ins.Parameters.AddWithValue("@StudentID", item.StudentID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@StudentClassID", item.StudentClassID);
                ins.Parameters.AddWithValue("@SubjectID", request.SubjectID);
                ins.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                ins.Parameters.AddWithValue("@SubjectType", type);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using var del = new SqlCommand("""
DELETE FROM dbo.StudentRecord
WHERE StudentClassID = @StudentClassID AND SubjectID = @SubjectID AND SchoolID = @SchoolID
""", con);
                del.Parameters.AddWithValue("@StudentClassID", item.StudentClassID);
                del.Parameters.AddWithValue("@SubjectID", request.SubjectID);
                del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await del.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        return new StudentInfoResult { Succeeded = true, Count = request.Items.Count };
    }

    public async Task<StudentInfoResult> ReplaceClassSubjectsAsync(
        SessionSnapshot session, ReplaceClassSubjectsRequest? request, CancellationToken cancellationToken)
    {
        var items = (request?.Items ?? []).Where(x => x.SubjectID > 0).ToList();
        if (request is null || request.ClassID <= 0)
            return Fail("si.needClass");
        if (items.Count == 0)
            return Fail("si.needSubject");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var students = new List<(int StudentID, int StudentClassID)>();
            await using (var list = new SqlCommand("""
SELECT Student.StudentID, StudentsClass.StudentClassID
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
  AND Student.Status = N'Active'
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
""", con, tx))
            {
                list.Parameters.AddWithValue("@ClassID", request.ClassID);
                list.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                list.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                list.Parameters.AddWithValue("@SubjectGroupID", request.SubjectGroupID);
                list.Parameters.AddWithValue("@SectionID", request.SectionID);
                list.Parameters.AddWithValue("@ShiftID", request.ShiftID);
                await using var reader = await list.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    students.Add((ToInt(reader["StudentID"]), ToInt(reader["StudentClassID"])));
            }

            foreach (var student in students)
            {
                await using var del = new SqlCommand("""
DELETE FROM dbo.StudentRecord WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID
""", con, tx);
                del.Parameters.AddWithValue("@StudentClassID", student.StudentClassID);
                del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await del.ExecuteNonQueryAsync(cancellationToken);

                foreach (var item in items)
                {
                    var type = string.Equals(item.SubjectType, "Optional", StringComparison.OrdinalIgnoreCase)
                        ? "Optional" : "Compulsory";
                    await using var ins = new SqlCommand("""
INSERT INTO dbo.StudentRecord
    (StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date)
VALUES
    (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())
""", con, tx);
                    ins.Parameters.AddWithValue("@StudentID", student.StudentID);
                    ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                    ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    ins.Parameters.AddWithValue("@StudentClassID", student.StudentClassID);
                    ins.Parameters.AddWithValue("@SubjectID", item.SubjectID);
                    ins.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                    ins.Parameters.AddWithValue("@SubjectType", type);
                    await ins.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            var groupId = request.SubjectGroupID > 0 ? request.SubjectGroupID : 0;
            await using (var clear = new SqlCommand("""
DELETE FROM dbo.SubjectForGroup
WHERE ClassID = @ClassID AND SchoolID = @SchoolID AND SubjectGroupID = @SubjectGroupID
""", con, tx))
            {
                clear.Parameters.AddWithValue("@ClassID", request.ClassID);
                clear.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                clear.Parameters.AddWithValue("@SubjectGroupID", groupId);
                await clear.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var item in items)
            {
                var type = string.Equals(item.SubjectType, "Optional", StringComparison.OrdinalIgnoreCase)
                    ? "Optional" : "Compulsory";
                await using var g = new SqlCommand("""
INSERT INTO dbo.SubjectForGroup (SchoolID, RegistrationID, ClassID, SubjectGroupID, SubjectID, SubjectType)
VALUES (@SchoolID, @RegistrationID, @ClassID, @SubjectGroupID, @SubjectID, @SubjectType)
""", con, tx);
                g.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                g.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                g.Parameters.AddWithValue("@ClassID", request.ClassID);
                g.Parameters.AddWithValue("@SubjectGroupID", groupId);
                g.Parameters.AddWithValue("@SubjectID", item.SubjectID);
                g.Parameters.AddWithValue("@SubjectType", type);
                await g.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new StudentInfoResult { Succeeded = true, Count = students.Count };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<StudentInfoResult> SaveRollsAsync(
        SessionSnapshot session, SaveRollSeatRequest? request, CancellationToken cancellationToken)
    {
        var items = (request?.Items ?? []).Where(x => x.StudentClassID > 0).ToList();
        if (items.Count == 0)
            return Fail("sm.needStudents");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var n = 0;
        foreach (var item in items)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.StudentsClass
SET RollNo = @RollNo, SeatNo = @SeatNo
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
            cmd.Parameters.AddWithValue("@RollNo", (object?)item.RollNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SeatNo", (object?)item.SeatNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StudentClassID", item.StudentClassID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            n += await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return new StudentInfoResult { Succeeded = true, Count = n };
    }

    public async Task<TcStudentDto?> FindTcAsync(
        SessionSnapshot session, string? studentCode, CancellationToken cancellationToken)
    {
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0)
            return null;

        const string sql = """
SELECT TOP 1 Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID,
       Student.StudentsName, Student.FathersName, Student.Gender, CreateClass.Class,
       CreateSubjectGroup.SubjectGroup, CreateSection.Section, CreateShift.Shift,
       StudentsClass.RollNo, Student.SMSPhoneNo, Student.Status, Education_Year.EducationYear,
       Student.DateofBirth, Student.RejectedDate, Student.StudentsLocalAddress, SchoolInfo.SchoolName
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
INNER JOIN dbo.SchoolInfo ON Student.SchoolID = SchoolInfo.SchoolID
LEFT OUTER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT OUTER JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT OUTER JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT OUTER JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
WHERE Student.ID = @ID AND StudentsClass.SchoolID = @SchoolID AND StudentsClass.Class_Status IS NULL
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
        return ReadTc(reader);
    }

    public async Task<IReadOnlyList<TcStudentDto>> ListTcAsync(
        SessionSnapshot session, int classId, CancellationToken cancellationToken)
    {
        var sql = """
SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID,
       Student.StudentsName, Student.FathersName, Student.Gender, CreateClass.Class,
       ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup,
       ISNULL(CreateSection.Section, N'No section') AS Section, CreateShift.Shift,
       StudentsClass.RollNo, Student.SMSPhoneNo, Student.Status, CAST(NULL AS nvarchar(50)) AS EducationYear,
       Student.DateofBirth, Student.RejectedDate, Student.StudentPermanentAddress AS StudentsLocalAddress,
       CAST(N'' AS nvarchar(200)) AS SchoolName
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT OUTER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT OUTER JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT OUTER JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT OUTER JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
WHERE Student.Status = N'Rejected' AND StudentsClass.SchoolID = @SchoolID
""";
        if (classId > 0)
            sql += " AND StudentsClass.ClassID = @ClassID";
        sql += " ORDER BY Student.RejectedDate DESC";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        if (classId > 0)
            cmd.Parameters.AddWithValue("@ClassID", classId);
        var items = new List<TcStudentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(ReadTc(reader));
        return items;
    }

    public async Task<StudentInfoResult> GiveTcAsync(
        SessionSnapshot session, GiveTcRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0 || string.IsNullOrWhiteSpace(request.ID))
            return Fail("sm.needStudent");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var upd = new SqlCommand("""
UPDATE dbo.Student
SET Status = N'Rejected', RejectedDate = GETDATE(), StudentRegistrationID = NULL, DeactivateTime = GETDATE()
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
""", con, tx))
            {
                upd.Parameters.AddWithValue("@StudentID", request.StudentID);
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                if (await upd.ExecuteNonQueryAsync(cancellationToken) == 0)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("sm.needStudent");
                }
            }

            await using (var log = new SqlCommand("""
IF OBJECT_ID(N'dbo.Student_Act_Deactivate_Log', N'U') IS NOT NULL
INSERT INTO dbo.Student_Act_Deactivate_Log (SchoolID, RegistrationID, StudentClassID, StudentID, Status, Act_Deact_Time)
VALUES (@SchoolID, @RegistrationID, @StudentClassID, @StudentID, N'Active', GETDATE())
""", con, tx))
            {
                log.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                log.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                log.Parameters.AddWithValue("@StudentClassID", request.StudentClassID);
                log.Parameters.AddWithValue("@StudentID", request.StudentID);
                await log.ExecuteNonQueryAsync(cancellationToken);
            }

            var userName = session.SchoolID + request.ID.Trim();
            await using (var ast = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
DELETE FROM dbo.AST WHERE UserName = @UserName AND SchoolID = @SchoolID
""", con, tx))
            {
                ast.Parameters.AddWithValue("@UserName", userName);
                ast.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await ast.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var reg = new SqlCommand("""
DELETE FROM dbo.Registration WHERE UserName = @UserName AND SchoolID = @SchoolID
""", con, tx))
            {
                reg.Parameters.AddWithValue("@UserName", userName);
                reg.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await reg.ExecuteNonQueryAsync(cancellationToken);
            }

            if (request.DeleteAllPayorder)
            {
                await using var pay = new SqlCommand("""
DELETE FROM dbo.Income_PayOrder
FROM dbo.Income_PayOrder
INNER JOIN dbo.Student ON Income_PayOrder.StudentID = Student.StudentID
WHERE Income_PayOrder.PaidAmount <= 0 AND Income_PayOrder.SchoolID = @SchoolID AND Student.ID = @ID
""", con, tx);
                pay.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                pay.Parameters.AddWithValue("@ID", request.ID.Trim());
                await pay.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using (var pay = new SqlCommand("""
DELETE FROM dbo.Income_PayOrder
FROM dbo.Income_PayOrder
INNER JOIN dbo.Student ON Income_PayOrder.StudentID = Student.StudentID
WHERE Income_PayOrder.PaidAmount <= 0
  AND Income_PayOrder.SchoolID = @SchoolID
  AND Income_PayOrder.EndDate >= @EndDate
  AND Student.ID = @ID
""", con, tx))
                {
                    pay.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    pay.Parameters.AddWithValue("@EndDate", DateTime.Today);
                    pay.Parameters.AddWithValue("@ID", request.ID.Trim());
                    await pay.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (var keep = new SqlCommand("""
UPDATE dbo.Income_PayOrder
SET Is_Active = 1
FROM dbo.Income_PayOrder
INNER JOIN dbo.Student ON Income_PayOrder.StudentID = Student.StudentID
WHERE Income_PayOrder.PaidAmount <= 0
  AND Income_PayOrder.SchoolID = @SchoolID
  AND Income_PayOrder.EndDate <= GETDATE()
  AND Student.ID = @ID
""", con, tx))
                {
                    keep.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    keep.Parameters.AddWithValue("@ID", request.ID.Trim());
                    await keep.ExecuteNonQueryAsync(cancellationToken);
                }
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

    public async Task<StudentInfoResult> SaveStudentPhotoAsync(
        SessionSnapshot session, SaveStudentPhotoRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0)
            return Fail("sm.needStudent");

        byte[] bytes;
        try
        {
            bytes = DecodePhoto(request.ImageBase64);
        }
        catch
        {
            return Fail("sm.badPhoto");
        }

        if (bytes.Length == 0)
            return Fail("sm.needPhoto");
        if (bytes.Length > 80_000)
            return Fail("sm.photoTooBig");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            int imageId;
            await using (var find = new SqlCommand("""
SELECT StudentImageID FROM dbo.Student
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
""", con, tx))
            {
                find.Parameters.AddWithValue("@StudentID", request.StudentID);
                find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                var val = await find.ExecuteScalarAsync(cancellationToken);
                if (val is null or DBNull)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("sm.needStudent");
                }

                imageId = ToInt(val);
            }

            var exists = false;
            if (imageId > 0)
            {
                await using var chk = new SqlCommand(
                    "SELECT 1 FROM dbo.Student_Image WHERE StudentImageID = @Id", con, tx);
                chk.Parameters.AddWithValue("@Id", imageId);
                exists = await chk.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
            }

            if (exists)
            {
                var sql = request.IsGuardian
                    ? "UPDATE dbo.Student_Image SET Guardian_Photo = @Image WHERE StudentImageID = @Id"
                    : "UPDATE dbo.Student_Image SET Image = @Image WHERE StudentImageID = @Id";
                await using var upd = new SqlCommand(sql, con, tx);
                AddImageParam(upd, bytes);
                upd.Parameters.AddWithValue("@Id", imageId);
                await upd.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                var insSql = request.IsGuardian
                    ? """
INSERT INTO dbo.Student_Image (Image, Guardian_Photo) VALUES (NULL, @Image);
SELECT CAST(SCOPE_IDENTITY() AS INT);
"""
                    : """
INSERT INTO dbo.Student_Image (Image, Guardian_Photo) VALUES (@Image, NULL);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""";
                await using var ins = new SqlCommand(insSql, con, tx);
                AddImageParam(ins, bytes);
                imageId = ToInt(await ins.ExecuteScalarAsync(cancellationToken));

                await using var link = new SqlCommand("""
UPDATE dbo.Student SET StudentImageID = @ImageId
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
""", con, tx);
                link.Parameters.AddWithValue("@ImageId", imageId);
                link.Parameters.AddWithValue("@StudentID", request.StudentID);
                link.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await link.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return new StudentInfoResult { Succeeded = true, Count = imageId };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<StudentInfoResult> ActivateTcAsync(
        SessionSnapshot session, ActivateTcRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0 || request.ClassID <= 0 || request.EducationYearID <= 0)
            return Fail("sm.needClass");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var act = new SqlCommand("""
UPDATE dbo.Student SET Status = N'Active', ActiveTime = GETDATE(), ActiveDate = GETDATE()
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
""", con, tx))
            {
                act.Parameters.AddWithValue("@StudentID", request.StudentID);
                act.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await act.ExecuteNonQueryAsync(cancellationToken);
            }

            int? existingId = null;
            await using (var find = new SqlCommand("""
SELECT TOP 1 StudentClassID
FROM dbo.StudentsClass
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
  AND EducationYearID = @EducationYearID AND Class_Status IS NULL
""", con, tx))
            {
                find.Parameters.AddWithValue("@StudentID", request.StudentID);
                find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                find.Parameters.AddWithValue("@EducationYearID", request.EducationYearID);
                existingId = ToInt(await find.ExecuteScalarAsync(cancellationToken));
                if (existingId == 0)
                    existingId = null;
            }

            if (existingId is > 0)
            {
                await using var upd = new SqlCommand("""
UPDATE dbo.StudentsClass
SET ClassID = @ClassID, SectionID = @SectionID, SubjectGroupID = @SubjectGroupID, ShiftID = @ShiftID
WHERE StudentClassID = @StudentClassID
""", con, tx);
                upd.Parameters.AddWithValue("@ClassID", request.ClassID);
                AddNullableId(upd, "@SectionID", request.SectionID);
                AddNullableId(upd, "@SubjectGroupID", request.SubjectGroupID);
                AddNullableId(upd, "@ShiftID", request.ShiftID);
                upd.Parameters.AddWithValue("@StudentClassID", existingId.Value);
                await upd.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using (var mark = new SqlCommand("""
UPDATE dbo.StudentsClass SET Class_Status = N'Re-Admitted'
WHERE StudentID = @StudentID AND SchoolID = @SchoolID
  AND EducationYearID <> @EducationYearID AND Class_Status IS NULL
""", con, tx))
                {
                    mark.Parameters.AddWithValue("@StudentID", request.StudentID);
                    mark.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    mark.Parameters.AddWithValue("@EducationYearID", request.EducationYearID);
                    await mark.ExecuteNonQueryAsync(cancellationToken);
                }

                await using var ins = new SqlCommand("""
INSERT INTO dbo.StudentsClass
    (SchoolID, RegistrationID, StudentID, ClassID, SectionID, SubjectGroupID, ShiftID, EducationYearID, Date, Class_Status)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID, @EducationYearID, GETDATE(), NULL)
""", con, tx);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@StudentID", request.StudentID);
                ins.Parameters.AddWithValue("@ClassID", request.ClassID);
                AddNullableId(ins, "@SectionID", request.SectionID);
                AddNullableId(ins, "@SubjectGroupID", request.SubjectGroupID);
                AddNullableId(ins, "@ShiftID", request.ShiftID);
                ins.Parameters.AddWithValue("@EducationYearID", request.EducationYearID);
                await ins.ExecuteNonQueryAsync(cancellationToken);
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

    public async Task<IReadOnlyList<NoticeDto>> ListNoticesAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT n.StudentNoticeId, n.NoticeTitle, n.Notice, n.Notice_file, n.IsHomeWork, n.InsertDate, r.UserName,
       STUFF((
           SELECT N', ' + c.Class
           FROM dbo.StudentNoticeClass AS nc
           INNER JOIN dbo.CreateClass AS c ON c.ClassID = nc.ClassId
           WHERE nc.StudentNoticeId = n.StudentNoticeId
           ORDER BY c.SN
           FOR XML PATH(N''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N'') AS Classes
FROM dbo.StudentNotice AS n
INNER JOIN dbo.Registration AS r ON n.RegistrationId = r.RegistrationID
WHERE n.SchoolId = @SchoolID AND n.EducationYearId = @EducationYearID
ORDER BY n.InsertDate DESC
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        var items = new List<NoticeDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new NoticeDto
            {
                StudentNoticeId = ToInt(reader["StudentNoticeId"]),
                NoticeTitle = NullString(reader["NoticeTitle"]) ?? "",
                Notice = NullString(reader["Notice"]) ?? "",
                NoticeFile = NullString(reader["Notice_file"]),
                IsHomeWork = ReadBool(reader["IsHomeWork"]),
                InsertDate = ReadDate(reader["InsertDate"]),
                UserName = NullString(reader["UserName"]),
                Classes = NullString(reader["Classes"]) ?? ""
            });
        }

        return items;
    }

    public async Task<StudentInfoResult> SaveNoticeAsync(
        SessionSnapshot session, SaveNoticeRequest? request, CancellationToken cancellationToken)
    {
        var title = (request?.NoticeTitle ?? "").Trim();
        var classIds = (request?.ClassIDs ?? []).Where(x => x > 0).Distinct().ToList();
        if (title.Length == 0)
            return Fail("sm.needTitle");
        if (classIds.Count == 0)
            return Fail("sm.needClass");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            int id;
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.StudentNotice (RegistrationId, SchoolId, EducationYearId, NoticeTitle, Notice, Notice_file, IsHomeWork)
VALUES (@RegistrationId, @SchoolId, @EducationYearId, @NoticeTitle, @Notice, N'', @IsHomeWork);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx))
            {
                ins.Parameters.AddWithValue("@RegistrationId", session.RegistrationID);
                ins.Parameters.AddWithValue("@SchoolId", session.SchoolID);
                ins.Parameters.AddWithValue("@EducationYearId", session.EducationYearID);
                ins.Parameters.AddWithValue("@NoticeTitle", title);
                ins.Parameters.AddWithValue("@Notice", request!.Notice ?? "");
                ins.Parameters.AddWithValue("@IsHomeWork", request.IsHomeWork ? 1 : 0);
                id = ToInt(await ins.ExecuteScalarAsync(cancellationToken));
            }

            foreach (var classId in classIds)
            {
                await using var cls = new SqlCommand("""
INSERT INTO dbo.StudentNoticeClass (StudentNoticeId, ClassId) VALUES (@StudentNoticeId, @ClassId)
""", con, tx);
                cls.Parameters.AddWithValue("@StudentNoticeId", id);
                cls.Parameters.AddWithValue("@ClassId", classId);
                await cls.ExecuteNonQueryAsync(cancellationToken);
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

    public async Task<StudentInfoResult> DeleteNoticesAsync(
        SessionSnapshot session, DeleteNoticesRequest? request, CancellationToken cancellationToken)
    {
        var ids = (request?.NoticeIDs ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return Fail("sm.needNotice");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        foreach (var id in ids)
        {
            await using var cls = new SqlCommand("""
DELETE FROM dbo.StudentNoticeClass WHERE StudentNoticeId = @Id
""", con);
            cls.Parameters.AddWithValue("@Id", id);
            await cls.ExecuteNonQueryAsync(cancellationToken);

            await using var n = new SqlCommand("""
DELETE FROM dbo.StudentNotice
WHERE StudentNoticeId = @Id AND SchoolId = @SchoolID AND EducationYearId = @EducationYearID
""", con);
            n.Parameters.AddWithValue("@Id", id);
            n.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            n.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            await n.ExecuteNonQueryAsync(cancellationToken);
        }

        return new StudentInfoResult { Succeeded = true, Count = ids.Count };
    }

    private const string ClassStudentSql = """
SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID, Student.StudentsName,
       Student.FathersName, StudentsClass.RollNo, StudentsClass.SeatNo, Student.SMSPhoneNo,
       Student.FatherPhoneNumber, Student.MotherPhoneNumber, Student.GuardianPhoneNumber,
       ISNULL(Student.StudentImageID, 0) AS StudentImageID,
       ISNULL(StudentsClass.SubjectGroupID, 0) AS SubjectGroupID,
       ISNULL(StudentsClass.SectionID, 0) AS SectionID,
       ISNULL(StudentsClass.ShiftID, 0) AS ShiftID,
       CreateClass.Class, CreateSubjectGroup.SubjectGroup, CreateSection.Section, CreateShift.Shift
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT OUTER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT OUTER JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT OUTER JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT OUTER JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
WHERE StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
  AND Student.Status = N'Active'
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS FLOAT) ELSE 0 END
""";

    private const string ClassStudentByIdSql = """
SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID, Student.StudentsName,
       Student.FathersName, StudentsClass.RollNo, StudentsClass.SeatNo, Student.SMSPhoneNo,
       Student.FatherPhoneNumber, Student.MotherPhoneNumber, Student.GuardianPhoneNumber,
       ISNULL(Student.StudentImageID, 0) AS StudentImageID,
       ISNULL(StudentsClass.SubjectGroupID, 0) AS SubjectGroupID,
       ISNULL(StudentsClass.SectionID, 0) AS SectionID,
       ISNULL(StudentsClass.ShiftID, 0) AS ShiftID,
       CreateClass.Class, CreateSubjectGroup.SubjectGroup, CreateSection.Section, CreateShift.Shift
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT OUTER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT OUTER JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT OUTER JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT OUTER JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
WHERE Student.ID = @ID
  AND Student.Status = N'Active'
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
""";

    private static SmStudentRowDto ReadRow(SqlDataReader reader) => new()
    {
        StudentID = ToInt(reader["StudentID"]),
        StudentClassID = ToInt(reader["StudentClassID"]),
        ClassID = ToInt(reader["ClassID"]),
        ID = NullString(reader["ID"]) ?? "",
        StudentsName = NullString(reader["StudentsName"]) ?? "",
        FathersName = NullString(reader["FathersName"]),
        RollNo = NullString(reader["RollNo"]),
        SeatNo = NullString(reader["SeatNo"]),
        Phone = NullString(reader["SMSPhoneNo"]),
        FatherPhone = NullString(reader["FatherPhoneNumber"]),
        MotherPhone = NullString(reader["MotherPhoneNumber"]),
        GuardianPhone = NullString(reader["GuardianPhoneNumber"]),
        ClassName = NullString(reader["Class"]),
        SubjectGroupID = ToInt(reader["SubjectGroupID"]),
        SectionID = ToInt(reader["SectionID"]),
        ShiftID = ToInt(reader["ShiftID"]),
        GroupName = NullString(reader["SubjectGroup"]),
        SectionName = NullString(reader["Section"]),
        ShiftName = NullString(reader["Shift"]),
        StudentImageID = ToInt(reader["StudentImageID"])
    };

    private static TcStudentDto ReadTc(SqlDataReader reader) => new()
    {
        StudentID = ToInt(reader["StudentID"]),
        StudentClassID = ToInt(reader["StudentClassID"]),
        ClassID = ToInt(reader["ClassID"]),
        ID = NullString(reader["ID"]) ?? "",
        StudentsName = NullString(reader["StudentsName"]) ?? "",
        FathersName = NullString(reader["FathersName"]),
        Gender = NullString(reader["Gender"]),
        ClassName = NullString(reader["Class"]),
        GroupName = NullString(reader["SubjectGroup"]),
        SectionName = NullString(reader["Section"]),
        ShiftName = NullString(reader["Shift"]),
        RollNo = NullString(reader["RollNo"]),
        Phone = NullString(reader["SMSPhoneNo"]),
        Status = NullString(reader["Status"]) ?? "",
        EducationYear = NullString(reader["EducationYear"]),
        DateofBirth = ReadDate(reader["DateofBirth"]),
        RejectedDate = ReadDate(reader["RejectedDate"]),
        Address = NullString(reader["StudentsLocalAddress"]),
        SchoolName = NullString(reader["SchoolName"]) ?? ""
    };

    private static void AddClassFilters(SqlCommand cmd, int classId, int groupId, int sectionId, int shiftId)
    {
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SubjectGroupID", groupId);
        cmd.Parameters.AddWithValue("@SectionID", sectionId);
        cmd.Parameters.AddWithValue("@ShiftID", shiftId);
    }

    private static async Task AttachPhotosAsync(
        SqlConnection con, int schoolId, List<SmStudentRowDto> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return;

        var names = items.Select((_, i) => "@p" + i).ToArray();
        await using var cmd = new SqlCommand($"""
SELECT Student.StudentID, ISNULL(Student.StudentImageID, 0) AS StudentImageID,
       Student_Image.Image, Student_Image.Guardian_Photo
FROM dbo.Student
LEFT OUTER JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE Student.SchoolID = @SchoolID AND Student.StudentID IN ({string.Join(",", names)})
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        for (var i = 0; i < items.Count; i++)
            cmd.Parameters.AddWithValue(names[i], items[i].StudentID);

        var byId = items.ToDictionary(x => x.StudentID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!byId.TryGetValue(ToInt(reader["StudentID"]), out var row))
                continue;
            row.StudentImageID = ToInt(reader["StudentImageID"]);
            row.PhotoDataUrl = ToPhotoDataUrl(reader["Image"] as byte[]);
            row.GuardianPhotoDataUrl = ToPhotoDataUrl(reader["Guardian_Photo"] as byte[]);
        }
    }

    private static string? ToPhotoDataUrl(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 and <= 150_000 })
            return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    private static byte[] DecodePhoto(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];
        raw = raw.Trim();
        var comma = raw.IndexOf(',');
        if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > 0)
            raw = raw[(comma + 1)..];
        return Convert.FromBase64String(raw);
    }

    private static void AddImageParam(SqlCommand cmd, byte[] bytes)
    {
        var p = cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1);
        p.Value = bytes;
    }

    private static void AddNullableId(SqlCommand cmd, string name, int id) =>
        cmd.Parameters.AddWithValue(name, id > 0 ? id : 0);

    private static StudentInfoResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static string? NullString(object? value)
    {
        var text = value is null or DBNull ? null : value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool ReadBool(object? value)
    {
        if (value is bool b)
            return b;
        if (value is byte by)
            return by != 0;
        return ToInt(value) == 1;
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
}
