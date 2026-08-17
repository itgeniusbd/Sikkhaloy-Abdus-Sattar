using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;

namespace Sikkhaloy.SyncApi.Services;

public sealed class ExamService
{
    private readonly EduConnectionFactory _connections;

    public ExamService(EduConnectionFactory connections) => _connections = connections;

    public async Task<IReadOnlyList<ExamNameDto>> ListExamsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<ExamNameDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT ExamID, ExamName, Period_StartDate, Period_EndDate
FROM Exam_Name WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
ORDER BY ExamID
""", con);
        AddSession(cmd, session);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new ExamNameDto
            {
                ExamID = ToInt(reader["ExamID"]),
                ExamName = Text(reader["ExamName"]),
                Period_StartDate = Day(reader["Period_StartDate"]),
                Period_EndDate = Day(reader["Period_EndDate"])
            });
        }
        return items;
    }

    public async Task<ExamResult> CreateExamAsync(SessionSnapshot session, SaveExamNameRequest? request, CancellationToken ct)
    {
        var name = (request?.ExamName ?? "").Trim();
        if (name.Length == 0) return Fail("exam.required");
        if (request?.StartDate is null || request.EndDate is null) return Fail("exam.dates");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var check = new SqlCommand("""
SELECT COUNT(*) FROM Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamName = @ExamName
""", con))
        {
            AddSession(check, session);
            check.Parameters.AddWithValue("@ExamName", name);
            if (ToInt(await check.ExecuteScalarAsync(ct)) > 0) return Fail("exam.exists");
        }
        await using var cmd = new SqlCommand("""
INSERT INTO Exam_Name(SchoolID, RegistrationID, EducationYearID, ExamName, Period_StartDate, Period_EndDate, Date)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @ExamName, @StartDate, @EndDate, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@ExamName", name);
        cmd.Parameters.AddWithValue("@StartDate", request.StartDate.Value.Date);
        cmd.Parameters.AddWithValue("@EndDate", request.EndDate.Value.Date);
        var id = await cmd.ExecuteScalarAsync(ct);
        return Ok(id is null or DBNull ? 0 : Convert.ToInt32(id));
    }

    public async Task<ExamResult> UpdateExamAsync(SessionSnapshot session, int examId, SaveExamNameRequest? request, CancellationToken ct)
    {
        var name = (request?.ExamName ?? "").Trim();
        if (examId <= 0 || name.Length == 0) return Fail("exam.required");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE Exam_Name SET ExamName = @ExamName, Period_StartDate = @StartDate, Period_EndDate = @EndDate
WHERE ExamID = @ExamID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        cmd.Parameters.AddWithValue("@ExamName", name);
        cmd.Parameters.AddWithValue("@StartDate", (object?)request?.StartDate?.Date ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EndDate", (object?)request?.EndDate?.Date ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok(examId);
    }

    public async Task<ExamResult> DeleteExamAsync(SessionSnapshot session, int examId, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
DELETE FROM Exam_Name WHERE ExamID = @ExamID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok(examId);
        }
        catch
        {
            return Fail("exam.inUse");
        }
    }

    public async Task<IReadOnlyList<SubExamDto>> ListSubExamsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<SubExamDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT SubExamID, SubExamName, Sub_ExamSN FROM Exam_SubExam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
ORDER BY Sub_ExamSN
""", con);
        AddSession(cmd, session);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new SubExamDto
            {
                SubExamID = ToInt(reader["SubExamID"]),
                SubExamName = Text(reader["SubExamName"]),
                Sub_ExamSN = ToInt(reader["Sub_ExamSN"])
            });
        }
        return items;
    }

    public async Task<ExamResult> CreateSubExamAsync(SessionSnapshot session, SaveSubExamRequest? request, CancellationToken ct)
    {
        var name = (request?.SubExamName ?? "").Trim();
        if (name.Length == 0) return Fail("exam.subRequired");
        if (request is null || request.Serial < 1) return Fail("exam.serial");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
INSERT INTO Exam_SubExam_Name(SchoolID, RegistrationID, EducationYearID, SubExamName, Sub_ExamSN)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @SubExamName, @Sub_ExamSN);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@SubExamName", name);
        cmd.Parameters.AddWithValue("@Sub_ExamSN", request.Serial);
        var id = await cmd.ExecuteScalarAsync(ct);
        return Ok(id is null or DBNull ? 0 : Convert.ToInt32(id));
    }

    public async Task<ExamResult> UpdateSubExamAsync(SessionSnapshot session, int id, SaveSubExamRequest? request, CancellationToken ct)
    {
        var name = (request?.SubExamName ?? "").Trim();
        if (id <= 0 || name.Length == 0) return Fail("exam.subRequired");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE Exam_SubExam_Name SET SubExamName = @SubExamName, Sub_ExamSN = @Sub_ExamSN
WHERE SubExamID = @SubExamID AND SchoolID = @SchoolID
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@SubExamID", id);
        cmd.Parameters.AddWithValue("@SubExamName", name);
        cmd.Parameters.AddWithValue("@Sub_ExamSN", request?.Serial ?? 1);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok(id);
    }

    public async Task<ExamResult> DeleteSubExamAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("DELETE FROM Exam_SubExam_Name WHERE SubExamID = @SubExamID AND SchoolID = @SchoolID", con);
            AddSchool(cmd, session);
            cmd.Parameters.AddWithValue("@SubExamID", id);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok(id);
        }
        catch
        {
            return Fail("exam.subInUse");
        }
    }

    public async Task<IReadOnlyList<GradeSystemDto>> ListGradingAsync(SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<GradeSystemDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("SELECT GradeNameID, GradeName FROM Exam_Grade_Name WHERE SchoolID = @SchoolID ORDER BY GradeNameID", con))
        {
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new GradeSystemDto
                {
                    GradeNameID = ToInt(reader["GradeNameID"]),
                    GradeName = Text(reader["GradeName"])
                });
            }
        }
        foreach (var item in items)
        {
            await using var cmd = new SqlCommand("""
SELECT GradingID, MaxPercentage, MinPercentage, Grades, Point, Comments
FROM Exam_Grading_System WHERE GradeNameID = @GradeNameID AND SchoolID = @SchoolID
ORDER BY Point DESC
""", con);
            AddSchool(cmd, session);
            cmd.Parameters.AddWithValue("@GradeNameID", item.GradeNameID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                item.Bands.Add(new GradeBandDto
                {
                    GradingID = ToInt(reader["GradingID"]),
                    MaxPercentage = ToDbl(reader["MaxPercentage"]),
                    MinPercentage = ToDbl(reader["MinPercentage"]),
                    Grades = Text(reader["Grades"]),
                    Point = ToDbl(reader["Point"]),
                    Comments = reader["Comments"] is DBNull ? null : Text(reader["Comments"])
                });
            }
        }
        return items;
    }

    public async Task<ExamResult> CreateGradingAsync(SessionSnapshot session, SaveGradeSystemRequest? request, CancellationToken ct)
    {
        var name = (request?.GradeName ?? "").Trim();
        var bands = request?.Bands ?? [];
        if (name.Length == 0) return Fail("exam.gradeName");
        if (bands.Count == 0) return Fail("exam.gradeBand");
        if (!bands.Any(x => Math.Abs(x.MaxPercentage - 100) < 0.001)) return Fail("exam.need100");
        if (!bands.Any(x => Math.Abs(x.MinPercentage) < 0.001)) return Fail("exam.need0");
        if (!bands.Any(x => string.Equals(x.Grades.Trim(), "F", StringComparison.OrdinalIgnoreCase))) return Fail("exam.needF");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            int gradeId;
            await using (var cmd = new SqlCommand("""
INSERT INTO Exam_Grade_Name (SchoolID, RegistrationID, GradeName)
VALUES (@SchoolID, @RegistrationID, @GradeName);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx))
            {
                AddSchool(cmd, session);
                cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                cmd.Parameters.AddWithValue("@GradeName", name);
                gradeId = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }
            foreach (var band in bands)
            {
                await using var cmd = new SqlCommand("""
INSERT INTO Exam_Grading_System(RegistrationID, SchoolID, EducationYearID, GradeNameID, Grades, MaxPercentage, MinPercentage, Comments, Point)
VALUES (@RegistrationID, @SchoolID, @EducationYearID, @GradeNameID, @Grades, @MaxPercentage, @MinPercentage, @Comments, @Point)
""", con, tx);
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@GradeNameID", gradeId);
                cmd.Parameters.AddWithValue("@Grades", band.Grades.Trim());
                cmd.Parameters.AddWithValue("@MaxPercentage", band.MaxPercentage);
                cmd.Parameters.AddWithValue("@MinPercentage", band.MinPercentage);
                cmd.Parameters.AddWithValue("@Comments", (object?)band.Comments ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Point", band.Point);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);
            return Ok(gradeId);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ExamResult> RenameGradingAsync(SessionSnapshot session, int id, SaveGradeSystemRequest? request, CancellationToken ct)
    {
        var name = (request?.GradeName ?? "").Trim();
        if (id <= 0 || name.Length == 0) return Fail("exam.gradeName");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
IF NOT EXISTS (SELECT GradeName FROM Exam_Grade_Name WHERE GradeName = @GradeName AND SchoolID = @SchoolID AND GradeNameID <> @GradeNameID)
UPDATE Exam_Grade_Name SET GradeName = @GradeName WHERE GradeNameID = @GradeNameID AND SchoolID = @SchoolID
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@GradeNameID", id);
        cmd.Parameters.AddWithValue("@GradeName", name);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok(id);
    }

    public async Task<ExamResult> UpdateGradeCommentAsync(SessionSnapshot session, int gradingId, string? comments, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("UPDATE Exam_Grading_System SET Comments = @Comments WHERE GradingID = @GradingID AND SchoolID = @SchoolID", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@GradingID", gradingId);
        cmd.Parameters.AddWithValue("@Comments", (object?)comments ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok(gradingId);
    }

    public async Task<ExamResult> DeleteGradingAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
IF NOT EXISTS (SELECT ExamGradeAssignID FROM Exam_Grading_Assign WHERE GradeNameID = @GradeNameID)
BEGIN
  IF NOT EXISTS (SELECT Cumulative_SettingID FROM Exam_Cumulative_Setting WHERE GradeNameID = @GradeNameID)
  BEGIN
    DELETE FROM Exam_Grading_System WHERE GradeNameID = @GradeNameID AND SchoolID = @SchoolID
    DELETE FROM Exam_Grade_Name WHERE GradeNameID = @GradeNameID AND SchoolID = @SchoolID
  END
END
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@GradeNameID", id);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok(id);
    }

    public async Task<ExamFilterDto> GetFiltersAsync(SessionSnapshot session, string? kind, int classId, int examId, string? groupId, string? sectionId, string? shiftId, int subjectId, CancellationToken ct)
    {
        var dto = new ExamFilterDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        kind = (kind ?? "").ToLowerInvariant();

        if (kind is "distribution" or "pass" or "check" or "control" or "copy")
        {
            dto.Classes = await QueryOptionsAsync(con, """
SELECT DISTINCT CreateClass.ClassID AS Id, CreateClass.Class AS Name
FROM CreateClass
INNER JOIN StudentsClass ON CreateClass.ClassID = StudentsClass.ClassID
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.SchoolID = @SchoolID AND Student.Status = N'Active'
ORDER BY CreateClass.ClassID
""", session, ct);
        }
        if (kind is "input" or "collect")
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT ExamID AS Id, ExamName AS Name FROM Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID ORDER BY ExamID
""", session, ct);
        }
        else if (kind == "pass" && classId > 0)
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Name.ExamID AS Id, Exam_Name.ExamName AS Name
FROM Exam_Name INNER JOIN Exam_Full_Marks ON Exam_Name.ExamID = Exam_Full_Marks.ExamID
WHERE Exam_Full_Marks.ClassID = @ClassID AND Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.SchoolID = @SchoolID
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId));
        }
        else if (kind == "copy" && classId > 0)
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Name.ExamID AS Id, Exam_Name.ExamName AS Name
FROM Exam_Name INNER JOIN Exam_Full_Marks ON Exam_Name.ExamID = Exam_Full_Marks.ExamID
WHERE Exam_Name.SchoolID = @SchoolID AND Exam_Name.EducationYearID = @EducationYearID AND Exam_Full_Marks.ClassID = @ClassID
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId));
            dto.CopyToExams = await QueryOptionsAsync(con, """
SELECT ExamID AS Id, ExamName AS Name FROM Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
  AND ExamID NOT IN (SELECT DISTINCT ExamID FROM Exam_Full_Marks WHERE ClassID = @ClassID AND EducationYearID = @EducationYearID AND SchoolID = @SchoolID)
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId));
        }
        else if (kind == "control")
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Name.ExamID AS Id, Exam_Name.ExamName AS Name
FROM Exam_Publish_Setting INNER JOIN Exam_Name ON Exam_Publish_Setting.ExamID = Exam_Name.ExamID
WHERE Exam_Publish_Setting.SchoolID = @SchoolID AND Exam_Publish_Setting.EducationYearID = @EducationYearID
ORDER BY Exam_Name.ExamName
""", session, ct);
            dto.CumulativeExams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Cumulative_Name.CumulativeNameID AS Id, Exam_Cumulative_Name.CumulativeResultName AS Name
FROM Exam_Cumulative_Name INNER JOIN Exam_Cumulative_Setting ON Exam_Cumulative_Name.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID
WHERE Exam_Cumulative_Setting.SchoolID = @SchoolID AND Exam_Cumulative_Setting.EducationYearID = @EducationYearID
""", session, ct);
        }
        else
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT ExamID AS Id, ExamName AS Name FROM Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID ORDER BY ExamID
""", session, ct);
        }

        dto.Grades = await QueryOptionsAsync(con, "SELECT GradeNameID AS Id, GradeName AS Name FROM Exam_Grade_Name WHERE SchoolID = @SchoolID", session, ct, schoolOnly: true);

        if (classId > 0)
        {
            dto.Groups = await QueryOptionsAsync(con, """
SELECT DISTINCT [Join].SubjectGroupID AS Id, CreateSubjectGroup.SubjectGroup AS Name
FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID
WHERE [Join].ClassID = @ClassID AND [Join].SectionID LIKE @SectionID AND [Join].ShiftID LIKE @ShiftID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@SectionID", Like(sectionId));
                c.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            }, schoolOnly: true);
            dto.Sections = await QueryOptionsAsync(con, """
SELECT DISTINCT [Join].SectionID AS Id, CreateSection.Section AS Name
FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID
WHERE [Join].ClassID = @ClassID AND [Join].SubjectGroupID LIKE @GroupID AND [Join].ShiftID LIKE @ShiftID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@GroupID", Like(groupId));
                c.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            }, schoolOnly: true);
            dto.Shifts = await QueryOptionsAsync(con, """
SELECT DISTINCT [Join].ShiftID AS Id, CreateShift.Shift AS Name
FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID
WHERE [Join].ClassID = @ClassID AND [Join].SubjectGroupID LIKE @GroupID AND [Join].SectionID LIKE @SectionID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@GroupID", Like(groupId));
                c.Parameters.AddWithValue("@SectionID", Like(sectionId));
            }, schoolOnly: true);
        }

        if (kind is "input" or "collect" && examId > 0)
        {
            dto.Classes = await QueryOptionsAsync(con, """
SELECT DISTINCT CreateClass.Class AS Name, CreateClass.ClassID AS Id, CreateClass.SN
FROM CreateClass INNER JOIN Exam_Full_Marks ON CreateClass.ClassID = Exam_Full_Marks.ClassID
WHERE CreateClass.SchoolID = @SchoolID AND Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.ExamID = @ExamID
ORDER BY CreateClass.SN
""", session, ct, c => c.Parameters.AddWithValue("@ExamID", examId));
        }

        if (kind is "input" or "collect" && classId > 0 && examId > 0)
        {
            dto.Subjects = await QueryOptionsAsync(con, """
SELECT DISTINCT Subject.SubjectID AS Id, Subject.SubjectName AS Name
FROM Subject INNER JOIN Exam_Full_Marks ON Subject.SubjectID = Exam_Full_Marks.SubjectID
WHERE Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.ClassID = @ClassID
  AND Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.ExamID = @ExamID
ORDER BY Subject.SubjectName
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@ExamID", examId);
            });
        }

        if (kind == "pass" && classId > 0 && examId > 0)
        {
            dto.SubExams = await QueryOptionsAsync(con, """
SELECT DISTINCT ISNULL(Exam_SubExam_Name.SubExamID, -1) AS Id, ISNULL(Exam_SubExam_Name.SubExamName, 'No Sub Exam') AS Name
FROM Exam_SubExam_Name RIGHT OUTER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID
WHERE Exam_Full_Marks.ClassID = @ClassID AND Exam_Full_Marks.ExamID = @ExamID
  AND Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.SchoolID = @SchoolID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@ExamID", examId);
            });
        }

        if (kind == "input" && classId > 0 && examId > 0 && subjectId > 0)
        {
            dto.SubExams = await QueryOptionsAsync(con, """
SELECT Exam_SubExam_Name.SubExamID AS Id, Exam_SubExam_Name.SubExamName AS Name
FROM Exam_SubExam_Name INNER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID
WHERE Exam_Full_Marks.ClassID = @ClassID AND Exam_SubExam_Name.SchoolID = @SchoolID
  AND Exam_Full_Marks.ExamID = @ExamID AND Exam_Full_Marks.EducationYearID = @EducationYearID
  AND Exam_Full_Marks.SubjectID = @SubjectID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@ExamID", examId);
                c.Parameters.AddWithValue("@SubjectID", subjectId);
            });
        }

        return dto;
    }

    public async Task<IReadOnlyList<PassMarkRowDto>> ListPassMarksAsync(SessionSnapshot session, int classId, int examId, int subExamId, CancellationToken ct)
    {
        var items = new List<PassMarkRowDto>();
        if (classId <= 0 || examId <= 0) return items;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT Exam_Full_Marks.ExamFullMarksID, Subject.SubjectName, Exam_SubExam_Name.SubExamName,
       Exam_Full_Marks.FullMarks, Exam_Full_Marks.Sub_PassMarks
FROM Exam_Full_Marks
INNER JOIN Subject ON Exam_Full_Marks.SubjectID = Subject.SubjectID
LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.ExamID = @ExamID
  AND Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.ClassID = @ClassID
  AND (ISNULL(Exam_Full_Marks.SubExamID, -1) = @SubExamID OR @SubExamID = 0)
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SubExamID", subExamId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new PassMarkRowDto
            {
                ExamFullMarksID = ToInt(reader["ExamFullMarksID"]),
                SubjectName = Text(reader["SubjectName"]),
                SubExamName = reader["SubExamName"] is DBNull ? null : Text(reader["SubExamName"]),
                FullMarks = ToDbl(reader["FullMarks"]),
                Sub_PassMarks = ToDbl(reader["Sub_PassMarks"])
            });
        }
        return items;
    }

    public async Task<ExamResult> SavePassMarksAsync(SessionSnapshot session, SavePassMarksRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.ExamID <= 0) return Fail("exam.select");
        foreach (var row in request.Rows)
        {
            if (row.Sub_PassMarks > row.FullMarks) return Fail("exam.passOver");
        }
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        foreach (var row in request.Rows)
        {
            await using var cmd = new SqlCommand("UPDATE Exam_Full_Marks SET Sub_PassMarks = @Sub_PassMarks WHERE ExamFullMarksID = @ExamFullMarksID AND SchoolID = @SchoolID", con);
            AddSchool(cmd, session);
            cmd.Parameters.AddWithValue("@Sub_PassMarks", row.Sub_PassMarks);
            cmd.Parameters.AddWithValue("@ExamFullMarksID", row.ExamFullMarksID);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await using (var sp = new SqlCommand("Exam_Mark_Re_Submit", con) { CommandType = CommandType.StoredProcedure })
        {
            AddSchoolYear(sp, session);
            sp.Parameters.AddWithValue("@ClassID", request.ClassID);
            sp.Parameters.AddWithValue("@ExamID", request.ExamID);
            await sp.ExecuteNonQueryAsync(ct);
        }
        return Ok(request.Rows.Count, request.Rows.Count);
    }

    public async Task<DistSheetDto> GetDistributionAsync(SessionSnapshot session, int classId, int examId, CancellationToken ct)
    {
        var dto = new DistSheetDto();
        if (classId <= 0 || examId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        dto.Grades = await QueryOptionsAsync(con, "SELECT GradeNameID AS Id, GradeName AS Name FROM Exam_Grade_Name WHERE SchoolID = @SchoolID", session, ct, schoolOnly: true);
        await using (var cmd = new SqlCommand("""
SELECT GradeNameID FROM Exam_Grading_Assign
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            var id = await cmd.ExecuteScalarAsync(ct);
            dto.GradeNameID = id is null or DBNull ? 0 : Convert.ToInt32(id);
        }
        var subs = await ListSubExamsAsync(session, ct);
        await using (var cmd = new SqlCommand("""
SELECT DISTINCT Subject.SubjectName, Subject.SubjectID
FROM Subject INNER JOIN StudentRecord ON Subject.SubjectID = StudentRecord.SubjectID
INNER JOIN StudentsClass ON StudentRecord.StudentClassID = StudentsClass.StudentClassID
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.ClassID = @ClassID
  AND StudentsClass.SchoolID = @SchoolID AND Student.Status = N'Active'
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Subjects.Add(new DistSubjectDto
                {
                    SubjectID = ToInt(reader["SubjectID"]),
                    SubjectName = Text(reader["SubjectName"]),
                    SubExams = subs.Select(s => new DistSubMarkDto { SubExamID = s.SubExamID, SubExamName = s.SubExamName }).ToList()
                });
            }
        }
        foreach (var subject in dto.Subjects)
        {
            await using var cmd = new SqlCommand("""
SELECT FullMarks, SubExamID FROM Exam_Full_Marks
WHERE SchoolID = @SchoolID AND ClassID = @ClassID AND ExamID = @ExamID AND SubjectID = @SubjectID AND EducationYearID = @EducationYearID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@SubjectID", subject.SubjectID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var subId = reader["SubExamID"] is DBNull ? 0 : ToInt(reader["SubExamID"]);
                var marks = ToDbl(reader["FullMarks"]);
                if (subId <= 0)
                {
                    subject.UseSubExam = false;
                    subject.FullMarks = marks;
                }
                else
                {
                    subject.UseSubExam = true;
                    var sub = subject.SubExams.FirstOrDefault(x => x.SubExamID == subId);
                    if (sub is not null)
                    {
                        sub.Selected = true;
                        sub.FullMarks = marks;
                    }
                }
            }
        }
        return dto;
    }

    public async Task<ExamResult> SaveDistributionAsync(SessionSnapshot session, SaveDistributionRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.ExamID <= 0) return Fail("exam.select");
        if (request.GradeNameID <= 0) return Fail("exam.noGrade");
        var count = 0;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await using (var del = new SqlCommand("""
DELETE FROM Exam_Full_Marks WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con, tx))
            {
                AddSession(del, session);
                del.Parameters.AddWithValue("@ClassID", request.ClassID);
                del.Parameters.AddWithValue("@ExamID", request.ExamID);
                await del.ExecuteNonQueryAsync(ct);
            }
            foreach (var subject in request.Subjects)
            {
                if (!subject.UseSubExam)
                {
                    if (subject.FullMarks is > 0)
                    {
                        await InsertFullMarkAsync(con, tx, session, request, subject.SubjectID, null, subject.FullMarks.Value, ct);
                        count++;
                    }
                    continue;
                }
                var any = false;
                foreach (var sub in subject.SubExams.Where(x => x.Selected && x.FullMarks is > 0))
                {
                    await InsertFullMarkAsync(con, tx, session, request, subject.SubjectID, sub.SubExamID, sub.FullMarks!.Value, ct);
                    any = true;
                }
                if (any) count++;
            }
            if (count == 0)
            {
                await tx.RollbackAsync(ct);
                return Fail("exam.noMarks");
            }
            await using (var grade = new SqlCommand("""
IF NOT EXISTS(SELECT GradeNameID FROM Exam_Grading_Assign WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID)
 INSERT INTO Exam_Grading_Assign (SchoolID, RegistrationID, EducationYearID, ClassID, ExamID, GradeNameID)
 VALUES (@SchoolID, @RegistrationID, @EducationYearID, @ClassID, @ExamID, @GradeNameID)
ELSE
 UPDATE Exam_Grading_Assign SET GradeNameID = @GradeNameID
 WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con, tx))
            {
                AddSession(grade, session);
                grade.Parameters.AddWithValue("@ClassID", request.ClassID);
                grade.Parameters.AddWithValue("@ExamID", request.ExamID);
                grade.Parameters.AddWithValue("@GradeNameID", request.GradeNameID);
                await grade.ExecuteNonQueryAsync(ct);
            }
            await using (var pass = new SqlCommand("""
UPDATE Exam_Full_Marks SET Sub_PassMarks = ROUND(Exam_Full_Marks.FullMarks * PM_T.PassMark / 100, 2, 0)
FROM Exam_Full_Marks INNER JOIN
(SELECT ROUND(Exam_Grading_System.MaxPercentage, 0, 1) + 1 AS PassMark, Exam_Grading_Assign.SchoolID
 FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
 WHERE Exam_Grading_System.Grades = 'F' AND Exam_Grading_Assign.ExamID = @ExamID AND Exam_Grading_Assign.ClassID = @ClassID
   AND Exam_Grading_Assign.EducationYearID = @EducationYearID AND Exam_Grading_Assign.SchoolID = @SchoolID) AS PM_T
ON Exam_Full_Marks.SchoolID = PM_T.SchoolID
WHERE Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.ExamID = @ExamID
  AND Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.ClassID = @ClassID
""", con, tx))
            {
                AddSession(pass, session);
                pass.Parameters.AddWithValue("@ClassID", request.ClassID);
                pass.Parameters.AddWithValue("@ExamID", request.ExamID);
                await pass.ExecuteNonQueryAsync(ct);
            }
            await using (var sp = new SqlCommand("Exam_Mark_Re_Submit", con, tx) { CommandType = CommandType.StoredProcedure })
            {
                AddSchoolYear(sp, session);
                sp.Parameters.AddWithValue("@ClassID", request.ClassID);
                sp.Parameters.AddWithValue("@ExamID", request.ExamID);
                await sp.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);
            return Ok(count, count);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ExamResult> CopyDistributionAsync(SessionSnapshot session, CopyDistributionRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.FromExamID <= 0 || request.ToExamID <= 0) return Fail("exam.select");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
DELETE FROM Exam_Grading_Assign WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @To_ExamID
DELETE FROM Exam_Full_Marks WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @To_ExamID
INSERT INTO Exam_Grading_Assign (SchoolID, RegistrationID, EducationYearID, ClassID, ExamID, GradeNameID)
SELECT @SchoolID, @RegistrationID, @EducationYearID, @ClassID, @To_ExamID, GradeNameID
FROM Exam_Grading_Assign WHERE SchoolID = @SchoolID AND ClassID = @ClassID AND ExamID = @From_ExamID AND EducationYearID = @EducationYearID
INSERT INTO Exam_Full_Marks (SchoolID, RegistrationID, SubjectID, ExamID, ClassID, SubExamID, EducationYearID, FullMarks, Date, Sub_PassMarks)
SELECT @SchoolID, @RegistrationID, SubjectID, @To_ExamID, @ClassID, SubExamID, @EducationYearID, FullMarks, GETDATE(), Sub_PassMarks
FROM Exam_Full_Marks WHERE ClassID = @ClassID AND EducationYearID = @EducationYearID AND SchoolID = @SchoolID AND ExamID = @From_ExamID
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
        cmd.Parameters.AddWithValue("@From_ExamID", request.FromExamID);
        cmd.Parameters.AddWithValue("@To_ExamID", request.ToExamID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok();
    }

    public async Task<CollectPaperDto> GetCollectPaperAsync(SessionSnapshot session, int examId, int classId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken ct)
    {
        var dto = new CollectPaperDto();
        if (examId <= 0 || classId <= 0 || subjectId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("""
SELECT Exam_Full_Marks.SubExamID, Exam_SubExam_Name.SubExamName
FROM Exam_Full_Marks
LEFT JOIN Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID AND Exam_SubExam_Name.SchoolID = @SchoolID
WHERE Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.ExamID = @ExamID AND Exam_Full_Marks.ClassID = @ClassID
  AND Exam_Full_Marks.EducationYearID = @EducationYearID AND Exam_Full_Marks.SubjectID = @SubjectID
  AND Exam_Full_Marks.FullMarks IS NOT NULL AND Exam_Full_Marks.FullMarks > 0
ORDER BY ISNULL(Exam_SubExam_Name.Sub_ExamSN, 999)
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var name = reader["SubExamName"] is DBNull ? "Marks" : Text(reader["SubExamName"]);
                dto.Columns.Add(name);
            }
        }
        dto.HasMarks = dto.Columns.Count > 0;
        if (!dto.HasMarks) return dto;
        await using (var cmd = new SqlCommand("""
SELECT Student.StudentsName, Student.FathersName, Student.ID, StudentsClass.RollNo
FROM StudentsClass
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID
WHERE StudentsClass.ClassID = @ClassID AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @GroupID AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentRecord.SubjectID = @SubjectID AND StudentsClass.ShiftID LIKE @ShiftID
  AND StudentsClass.SchoolID = @SchoolID AND Student.Status = N'Active'
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@GroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Students.Add(new CollectStudentDto
                {
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    FathersName = Text(reader["FathersName"]),
                    RollNo = Text(reader["RollNo"])
                });
            }
        }
        return dto;
    }

    public async Task<InputSheetDto> GetInputSheetAsync(SessionSnapshot session, int examId, int classId, int subjectId, int subExamId, string? groupId, string? sectionId, string? shiftId, CancellationToken ct)
    {
        var dto = new InputSheetDto();
        if (examId <= 0 || classId <= 0 || subjectId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("""
SELECT Exam_SubExam_Name.SubExamID, Exam_SubExam_Name.SubExamName, Exam_Full_Marks.FullMarks, Exam_Full_Marks.Sub_PassMarks,
       ROUND(Exam_Full_Marks.Sub_PassMarks * 100 / NULLIF(Exam_Full_Marks.FullMarks, 0), 2, 0) AS PassPercentage
FROM Exam_SubExam_Name INNER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID
WHERE Exam_Full_Marks.SubjectID = @SubjectID AND Exam_Full_Marks.ClassID = @ClassID AND Exam_SubExam_Name.SchoolID = @SchoolID
  AND Exam_Full_Marks.ExamID = @ExamID AND Exam_Full_Marks.EducationYearID = @EducationYearID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.SubHeaders.Add(new InputSubMarkDto
                {
                    SubExamID = ToInt(reader["SubExamID"]),
                    SubExamName = Text(reader["SubExamName"]),
                    FullMark = ToDbl(reader["FullMarks"]),
                    PassMark = ToDbl(reader["Sub_PassMarks"]),
                    PassPercentage = ToDbl(reader["PassPercentage"])
                });
            }
        }
        dto.HasSubExams = dto.SubHeaders.Count > 0;
        var lookupSub = subExamId > 0 ? subExamId : 0;
        await using (var cmd = new SqlCommand("""
SELECT Sub_PassMarks AS PassMark, FullMarks AS FullMark, ROUND(Sub_PassMarks * 100 / NULLIF(FullMarks, 0), 2, 0) AS PassPercentage
FROM Exam_Full_Marks
WHERE SchoolID = @SchoolID AND SubjectID = @SubjectID AND ExamID = @ExamID AND ClassID = @ClassID
  AND (SubExamID = @SubExamID OR @SubExamID = 0) AND EducationYearID = @EducationYearID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SubExamID", lookupSub);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.FullMark = ToDbl(reader["FullMark"]);
                dto.PassMark = ToDbl(reader["PassMark"]);
                dto.PassPercentage = ToDbl(reader["PassPercentage"]);
            }
        }
        await using (var cmd = new SqlCommand("""
SELECT Student.StudentsName, StudentsClass.StudentID, StudentsClass.StudentClassID, Student.ID, StudentsClass.RollNo
FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID
WHERE StudentsClass.ClassID = @ClassID AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @GroupID AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentRecord.SubjectID = @SubjectID AND StudentsClass.ShiftID LIKE @ShiftID
  AND StudentsClass.SchoolID = @SchoolID AND Student.Status = N'Active'
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@GroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Students.Add(new InputStudentDto
                {
                    StudentID = ToInt(reader["StudentID"]),
                    StudentClassID = ToInt(reader["StudentClassID"]),
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    RollNo = Text(reader["RollNo"]),
                    Subs = dto.SubHeaders.Select(s => new InputSubMarkDto
                    {
                        SubExamID = s.SubExamID,
                        SubExamName = s.SubExamName,
                        FullMark = s.FullMark,
                        PassMark = s.PassMark,
                        PassPercentage = s.PassPercentage
                    }).ToList()
                });
            }
        }
        foreach (var student in dto.Students)
        {
            if (dto.HasSubExams && subExamId <= 0)
            {
                foreach (var sub in student.Subs)
                    await FillObtainedAsync(con, student.StudentClassID, subjectId, examId, sub.SubExamID, sub, ct);
            }
            else
            {
                var box = new InputSubMarkDto();
                await FillObtainedAsync(con, student.StudentClassID, subjectId, examId, subExamId, box, ct);
                student.MarksObtained = box.MarksObtained;
                student.Absent = box.Absent;
            }
        }
        return dto;
    }

    public async Task<ExamResult> SaveInputMarksAsync(SessionSnapshot session, SaveInputMarksRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.ExamID <= 0 || request.SubjectID <= 0) return Fail("exam.select");
        var saved = 0;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        foreach (var student in request.Students)
        {
            if (request.AllSubExams && student.Subs.Count > 0)
            {
                foreach (var sub in student.Subs)
                {
                    if (!sub.Absent && sub.MarksObtained is null) continue;
                    if (sub.MarksObtained is > 0 && sub.MarksObtained > sub.FullMark) return Fail("exam.overMark");
                    await SubmitMarkAsync(con, session, request, student.StudentID, sub.SubExamID, sub.Absent ? null : sub.MarksObtained, sub.Absent, sub.FullMark, sub.PassMark, sub.PassPercentage, ct);
                    saved++;
                }
                continue;
            }
            if (!student.Absent && student.MarksObtained is null) continue;
            var full = 0d; var pass = 0d; var pct = 0d;
            await using (var cmd = new SqlCommand("""
SELECT FullMarks, Sub_PassMarks, ROUND(Sub_PassMarks * 100 / NULLIF(FullMarks, 0), 2, 0) AS PassPercentage
FROM Exam_Full_Marks WHERE SchoolID = @SchoolID AND SubjectID = @SubjectID AND ExamID = @ExamID AND ClassID = @ClassID
  AND (SubExamID = @SubExamID OR (@SubExamID = 0 AND SubExamID IS NULL)) AND EducationYearID = @EducationYearID
""", con))
            {
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@SubjectID", request.SubjectID);
                cmd.Parameters.AddWithValue("@ExamID", request.ExamID);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@SubExamID", request.SubExamID);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    full = ToDbl(reader["FullMarks"]);
                    pass = ToDbl(reader["Sub_PassMarks"]);
                    pct = ToDbl(reader["PassPercentage"]);
                }
            }
            if (student.MarksObtained is > 0 && student.MarksObtained > full) return Fail("exam.overMark");
            await SubmitMarkAsync(con, session, request, student.StudentID, request.SubExamID, student.Absent ? null : student.MarksObtained, student.Absent, full, pass, pct, ct);
            saved++;
        }
        if (saved == 0) return Fail("exam.needMark");
        return Ok(saved, saved);
    }

    public async Task<IReadOnlyList<MarksCheckRowDto>> GetMarksCheckAsync(SessionSnapshot session, int classId, int examId, CancellationToken ct)
    {
        var items = new List<MarksCheckRowDto>();
        if (classId <= 0 || examId <= 0) return items;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("""
SELECT DISTINCT Subject.SN, Subject.SubjectID, Subject.SubjectName, S_T.Total_Student
FROM Exam_Full_Marks INNER JOIN Subject ON Exam_Full_Marks.SubjectID = Subject.SubjectID
INNER JOIN (
  SELECT COUNT(Student.StudentID) AS Total_Student, StudentRecord.SubjectID
  FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
  INNER JOIN StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID
  WHERE Student.Status = 'Active' AND StudentsClass.ClassID = @ClassID
    AND StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.SchoolID = @SchoolID
  GROUP BY StudentRecord.SubjectID
) AS S_T ON Subject.SubjectID = S_T.SubjectID
WHERE Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.EducationYearID = @EducationYearID
  AND Exam_Full_Marks.ClassID = @ClassID AND Exam_Full_Marks.ExamID = @ExamID
ORDER BY Subject.SN
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new MarksCheckRowDto
                {
                    SubjectID = ToInt(reader["SubjectID"]),
                    SubjectName = Text(reader["SubjectName"]),
                    TotalStudent = ToInt(reader["Total_Student"])
                });
            }
        }
        foreach (var row in items)
        {
            await using var cmd = new SqlCommand("""
SELECT Sub_Exam_T.SubExamName, ISNULL(Total_Stu.Total_Student, 0) AS Total_Student
FROM (
  SELECT COUNT(Exam_Obtain_Marks.StudentID) AS Total_Student, Exam_Obtain_Marks.SubExamID, Exam_Obtain_Marks.SubjectID
  FROM Exam_Obtain_Marks INNER JOIN Student ON Exam_Obtain_Marks.StudentID = Student.StudentID
  WHERE Exam_Obtain_Marks.EducationYearID = @EducationYearID AND Exam_Obtain_Marks.SchoolID = @SchoolID
    AND Exam_Obtain_Marks.ClassID = @ClassID AND Exam_Obtain_Marks.ExamID = @ExamID
    AND Exam_Obtain_Marks.SubjectID = @SubjectID AND Student.Status = N'Active'
  GROUP BY Exam_Obtain_Marks.SubExamID, Exam_Obtain_Marks.SubjectID
) AS Total_Stu
RIGHT OUTER JOIN (
  SELECT Exam_SubExam_Name.SubExamName, Exam_Full_Marks.SubExamID, Exam_Full_Marks.SubjectID
  FROM Exam_Full_Marks LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID
  WHERE Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.EducationYearID = @EducationYearID
    AND Exam_Full_Marks.ExamID = @ExamID AND Exam_Full_Marks.ClassID = @ClassID AND Exam_Full_Marks.SubjectID = @SubjectID
) AS Sub_Exam_T ON Total_Stu.SubjectID = Sub_Exam_T.SubjectID AND ISNULL(Total_Stu.SubExamID, 0) = ISNULL(Sub_Exam_T.SubExamID, 0)
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@SubjectID", row.SubjectID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                row.Subs.Add(new MarksCheckSubDto
                {
                    SubExamName = reader["SubExamName"] is DBNull ? "Marks" : Text(reader["SubExamName"]),
                    TotalStudent = ToInt(reader["Total_Student"])
                });
            }
        }
        return items;
    }

    public async Task<IReadOnlyList<ExamControlRowDto>> GetControlAsync(SessionSnapshot session, int examId, bool cumulative, CancellationToken ct)
    {
        var items = new List<ExamControlRowDto>();
        if (examId <= 0) return items;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var sql = cumulative
            ? """
SELECT Exam_Cumulative_Setting.ClassID, CreateClass.Class, Exam_Cumulative_Setting.IS_Published, Exam_Cumulative_Setting.Last_Published_Date
FROM CreateClass INNER JOIN Exam_Cumulative_Setting ON CreateClass.ClassID = Exam_Cumulative_Setting.ClassID
WHERE Exam_Cumulative_Setting.SchoolID = @SchoolID AND Exam_Cumulative_Setting.EducationYearID = @EducationYearID
  AND Exam_Cumulative_Setting.CumulativeNameID = @ExamID
ORDER BY CreateClass.SN
"""
            : """
SELECT CreateClass.Class, Exam_Publish_Setting.Marks_Input_Locked, Exam_Publish_Setting.IS_Published,
       Exam_Publish_Setting.Last_Published_Date, Exam_Publish_Setting.ClassID
FROM Exam_Publish_Setting INNER JOIN CreateClass ON Exam_Publish_Setting.ClassID = CreateClass.ClassID
WHERE Exam_Publish_Setting.SchoolID = @SchoolID AND Exam_Publish_Setting.EducationYearID = @EducationYearID
  AND Exam_Publish_Setting.ExamID = @ExamID
ORDER BY CreateClass.SN
""";
        await using var cmd = new SqlCommand(sql, con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new ExamControlRowDto
            {
                ClassID = ToInt(reader["ClassID"]),
                ClassName = Text(reader["Class"]),
                LastPublished = Day(reader["Last_Published_Date"]),
                MarksLocked = !cumulative && reader["Marks_Input_Locked"] is not DBNull && Convert.ToBoolean(reader["Marks_Input_Locked"]),
                Published = reader["IS_Published"] is not DBNull && Convert.ToBoolean(reader["IS_Published"])
            });
        }
        return items;
    }

    public async Task<ExamResult> SaveControlAsync(SessionSnapshot session, SaveExamControlRequest? request, CancellationToken ct)
    {
        if (request is null || request.ExamID <= 0) return Fail("exam.select");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        foreach (var row in request.Rows)
        {
            var sql = request.Cumulative
                ? """
UPDATE Exam_Cumulative_Setting SET IS_Published = @IS_Published
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @ExamID
"""
                : """
UPDATE Exam_Publish_Setting SET IS_Published = @IS_Published, Marks_Input_Locked = @Marks_Input_Locked
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""";
            await using var cmd = new SqlCommand(sql, con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ExamID", request.ExamID);
            cmd.Parameters.AddWithValue("@ClassID", row.ClassID);
            cmd.Parameters.AddWithValue("@IS_Published", row.Published);
            if (!request.Cumulative)
                cmd.Parameters.AddWithValue("@Marks_Input_Locked", row.MarksLocked);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        return Ok(request.Rows.Count, request.Rows.Count);
    }

    private static async Task InsertFullMarkAsync(SqlConnection con, SqlTransaction tx, SessionSnapshot session, SaveDistributionRequest request, int subjectId, int? subExamId, double fullMarks, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO Exam_Full_Marks(SchoolID, RegistrationID, SubjectID, ExamID, ClassID, SubExamID, EducationYearID, FullMarks, Date)
VALUES (@SchoolID, @RegistrationID, @SubjectID, @ExamID, @ClassID, @SubExamID, @EducationYearID, @FullMarks, GETDATE())
""", con, tx);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@ExamID", request.ExamID);
        cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
        cmd.Parameters.AddWithValue("@SubExamID", (object?)subExamId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FullMarks", fullMarks);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task FillObtainedAsync(SqlConnection con, int studentClassId, int subjectId, int examId, int subExamId, InputSubMarkDto target, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT MarksObtained, AbsenceStatus FROM Exam_Obtain_Marks
WHERE StudentClassID = @StudentClassID AND SubjectID = @SubjectID AND ExamID = @ExamID
  AND (SubExamID = @SubExamID OR (@SubExamID = 0 AND SubExamID IS NULL))
""", con);
        cmd.Parameters.AddWithValue("@StudentClassID", studentClassId);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        cmd.Parameters.AddWithValue("@SubExamID", subExamId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return;
        target.MarksObtained = reader["MarksObtained"] is DBNull ? null : ToDbl(reader["MarksObtained"]);
        target.Absent = string.Equals(Text(reader["AbsenceStatus"]), "Absent", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task SubmitMarkAsync(SqlConnection con, SessionSnapshot session, SaveInputMarksRequest request, int studentId, int subExamId, double? marks, bool absent, double full, double pass, double pct, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("Exam_Mark_Submit", con) { CommandType = CommandType.StoredProcedure };
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
        cmd.Parameters.AddWithValue("@ExamID", request.ExamID);
        cmd.Parameters.AddWithValue("@SubjectID", request.SubjectID);
        cmd.Parameters.AddWithValue("@SubExamID", subExamId <= 0 ? DBNull.Value : subExamId);
        cmd.Parameters.AddWithValue("@MarksObtained", absent ? DBNull.Value : marks ?? 0);
        cmd.Parameters.AddWithValue("@AbsenceStatus", absent ? "Absent" : "Present");
        cmd.Parameters.AddWithValue("@FullMark", full);
        cmd.Parameters.AddWithValue("@PassPercentage", pct);
        cmd.Parameters.AddWithValue("@PassMark", pass);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<List<ExamOptionDto>> QueryOptionsAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null, bool schoolOnly = false)
    {
        var items = new List<ExamOptionDto>();
        await using var cmd = new SqlCommand(sql, con);
        if (schoolOnly) AddSchool(cmd, session); else AddSession(cmd, session);
        extra?.Invoke(cmd);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            items.Add(new ExamOptionDto { Id = ToInt(reader["Id"]), Name = Text(reader["Name"]) });
        return items;
    }

    private static void AddSchool(SqlCommand cmd, SessionSnapshot session)
    {
        if (!cmd.Parameters.Contains("@SchoolID"))
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        if (!cmd.Parameters.Contains("@RegistrationID") && cmd.CommandText.Contains("@RegistrationID", StringComparison.Ordinal))
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
    }

    private static void AddSchoolYear(SqlCommand cmd, SessionSnapshot session)
    {
        AddSchool(cmd, session);
        if (!cmd.Parameters.Contains("@EducationYearID"))
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
    }

    private static void AddSession(SqlCommand cmd, SessionSnapshot session)
    {
        AddSchool(cmd, session);
        if (!cmd.Parameters.Contains("@EducationYearID"))
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (!cmd.Parameters.Contains("@RegistrationID"))
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
    }

    private static string Like(string? value) => string.IsNullOrWhiteSpace(value) || value == "0" ? "%" : value.Trim();
    private static int ToInt(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static double ToDbl(object? value) => value is null or DBNull ? 0 : Convert.ToDouble(value);
    private static string Text(object? value) => value is null or DBNull ? "" : Convert.ToString(value) ?? "";
    private static DateTime? Day(object? value) => value is null or DBNull ? null : Convert.ToDateTime(value).Date;
    private static ExamResult Fail(string error) => new() { Error = error };
    private static ExamResult Ok(int id = 0, int count = 0) => new() { Succeeded = true, Id = id, Count = count };
}
