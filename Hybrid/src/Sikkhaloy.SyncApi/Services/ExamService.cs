using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class ExamService
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

        if (kind is "seat" or "admit")
        {
            dto.Classes = await QueryOptionsAsync(con, """
SELECT ClassID AS Id, Class AS Name
FROM CreateClass
WHERE SchoolID = @SchoolID
ORDER BY SN, ClassID
""", session, ct, schoolOnly: true);
        }
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
        if (kind == "cumulative-publish")
        {
            dto.Classes = await QueryOptionsAsync(con, """
SELECT DISTINCT CreateClass.ClassID AS Id, CreateClass.Class AS Name
FROM Exam_Publish_Setting
INNER JOIN CreateClass ON Exam_Publish_Setting.ClassID = CreateClass.ClassID
WHERE Exam_Publish_Setting.SchoolID = @SchoolID AND Exam_Publish_Setting.EducationYearID = @EducationYearID
ORDER BY CreateClass.ClassID
""", session, ct);
        }
        else if (kind is "result" or "publish" or "cumulative-result")
        {
            dto.Classes = await QueryOptionsAsync(con, """
SELECT DISTINCT CreateClass.ClassID AS Id, CreateClass.Class AS Name
FROM Exam_Result_of_Student
INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID
WHERE Exam_Result_of_Student.SchoolID = @SchoolID AND Exam_Result_of_Student.EducationYearID = @EducationYearID
ORDER BY CreateClass.ClassID
""", session, ct);
        }
        if (kind is "publish" or "cumulative-publish")
        {
            dto.Schedules = await QueryOptionsAsync(con, """
SELECT ScheduleID AS Id, ScheduleName AS Name
FROM Attendance_Schedule WHERE SchoolID = @SchoolID ORDER BY ScheduleName
""", session, ct, schoolOnly: true);
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
        else if ((kind is "result" or "publish") && classId > 0)
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Name.ExamID AS Id, Exam_Name.ExamName AS Name
FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID
WHERE Exam_Name.EducationYearID = @EducationYearID AND Exam_Name.SchoolID = @SchoolID
  AND Exam_Result_of_Student.ClassID = @ClassID AND Exam_Result_of_Student.EducationYearID = @EducationYearID
ORDER BY Exam_Name.ExamID
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId));
        }
        else if (kind is "cumulative-publish" or "cumulative-result")
        {
            dto.CumulativeExams = await QueryOptionsAsync(con, """
SELECT CumulativeNameID AS Id, CumulativeResultName AS Name
FROM Exam_Cumulative_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
ORDER BY Date DESC, CumulativeResultName
""", session, ct);
            if (kind == "cumulative-result" && classId > 0)
            {
                dto.CumulativeExams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Cumulative_Name.CumulativeNameID AS Id, Exam_Cumulative_Name.CumulativeResultName AS Name
FROM Exam_Cumulative_Name
INNER JOIN Exam_Cumulative_Student ON Exam_Cumulative_Name.CumulativeNameID = Exam_Cumulative_Student.CumulativeNameID
WHERE Exam_Cumulative_Name.EducationYearID = @EducationYearID AND Exam_Cumulative_Name.SchoolID = @SchoolID
  AND Exam_Cumulative_Student.ClassID = @ClassID
ORDER BY Exam_Cumulative_Name.CumulativeNameID
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId));
            }
        }
        else if (kind == "delete")
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_Name.ExamID AS Id, Exam_Name.ExamName AS Name
FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID
WHERE Exam_Name.SchoolID = @SchoolID AND Exam_Name.EducationYearID = @EducationYearID
""", session, ct);
            if (examId > 0)
            {
                dto.Classes = await QueryOptionsAsync(con, """
SELECT DISTINCT CreateClass.ClassID AS Id, CreateClass.Class AS Name
FROM CreateClass INNER JOIN Exam_Result_of_Student ON CreateClass.ClassID = Exam_Result_of_Student.ClassID
WHERE CreateClass.SchoolID = @SchoolID AND Exam_Result_of_Student.EducationYearID = @EducationYearID AND Exam_Result_of_Student.ExamID = @ExamID
ORDER BY CreateClass.ClassID
""", session, ct, c => c.Parameters.AddWithValue("@ExamID", examId));
            }
        }
        else
        {
            dto.Exams = await QueryOptionsAsync(con, """
SELECT ExamID AS Id, ExamName AS Name FROM Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID ORDER BY ExamID
""", session, ct);
        }

        dto.Grades = await QueryOptionsAsync(con, "SELECT GradeNameID AS Id, GradeName AS Name FROM Exam_Grade_Name WHERE SchoolID = @SchoolID", session, ct, schoolOnly: true);

        if (classId > 0 && kind is not "cumulative-publish")
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

        if (kind == "result" && classId > 0 && examId > 0)
        {
            dto.Subjects = await QueryOptionsAsync(con, """
SELECT DISTINCT Subject.SubjectID AS Id, Subject.SubjectName AS Name
FROM Subject
INNER JOIN Exam_Result_of_Subject ON Subject.SubjectID = Exam_Result_of_Subject.SubjectID
INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
WHERE Exam_Result_of_Subject.ExamID = @ExamID AND Exam_Result_of_Subject.EducationYearID = @EducationYearID
  AND Exam_Result_of_Subject.SchoolID = @SchoolID AND Exam_Result_of_Subject.ClassID = @ClassID
  AND Exam_Result_of_Student.StudentPublishStatus = N'Pub'
ORDER BY Subject.SubjectName
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@ExamID", examId);
            });
        }

        if (kind == "delete" && classId > 0)
        {
            dto.Subjects = await QueryOptionsAsync(con, """
SELECT DISTINCT Subject.SubjectID AS Id, Subject.SubjectName AS Name
FROM Subject INNER JOIN Exam_Result_of_Subject ON Subject.SubjectID = Exam_Result_of_Subject.SubjectID
WHERE Exam_Result_of_Subject.ClassID = @ClassID AND Exam_Result_of_Subject.SchoolID = @SchoolID
  AND Exam_Result_of_Subject.EducationYearID = @EducationYearID
ORDER BY Subject.SubjectName
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId));
        }

        if (kind == "delete" && examId > 0 && classId > 0 && subjectId > 0)
        {
            dto.SubExams = await QueryOptionsAsync(con, """
SELECT DISTINCT Exam_SubExam_Name.SubExamID AS Id, Exam_SubExam_Name.SubExamName AS Name
FROM Exam_SubExam_Name INNER JOIN Exam_Obtain_Marks ON Exam_SubExam_Name.SubExamID = Exam_Obtain_Marks.SubExamID
WHERE Exam_SubExam_Name.SchoolID = @SchoolID AND Exam_Obtain_Marks.EducationYearID = @EducationYearID
  AND Exam_Obtain_Marks.ExamID = @ExamID AND Exam_Obtain_Marks.ClassID = @ClassID AND Exam_Obtain_Marks.SubjectID = @SubjectID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ExamID", examId);
                c.Parameters.AddWithValue("@ClassID", classId);
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
                    await FillObtainedAsync(con, session, student.StudentClassID, subjectId, examId, sub.SubExamID, sub, ct);
            }
            else
            {
                var box = new InputSubMarkDto();
                await FillObtainedAsync(con, session, student.StudentClassID, subjectId, examId, subExamId, box, ct);
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

    private static async Task FillObtainedAsync(SqlConnection con, SessionSnapshot session, int studentClassId, int subjectId, int examId, int subExamId, InputSubMarkDto target, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT MarksObtained, AbsenceStatus FROM Exam_Obtain_Marks
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
  AND StudentClassID = @StudentClassID AND SubjectID = @SubjectID AND ExamID = @ExamID
  AND (SubExamID = @SubExamID OR (@SubExamID = 0 AND SubExamID IS NULL))
""", con);
        AddSession(cmd, session);
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

    public async Task<ExamPublishSettingDto> GetPublishSettingAsync(SessionSnapshot session, int classId, int examId, CancellationToken ct)
    {
        var dto = new ExamPublishSettingDto();
        if (classId <= 0 || examId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT Exam_Position_Format, IS_Hide_Sec_Position, IS_Hide_Class_Position, IS_Hide_FullMark, IS_Hide_PassMark,
       Optional_Percentage_Deduction, IS_Fail_Enable_Optional_Subject, IS_Add_Optional_Mark_In_FullMarks,
       IS_Grade_BasePoint, IS_Enable_Grade_as_it_is_if_Fail, IS_Enable_Fail_if_fail_in_sub_Exam,
       Attendance_FromDate, Attendance_ToDate, Attendance_ScheduleID
FROM Exam_Publish_Setting
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var format = Text(reader["Exam_Position_Format"]);
                dto.PositionFormat = format.Length == 0 ? "Point" : format;
                dto.HideSecPosition = Flag(reader["IS_Hide_Sec_Position"]);
                dto.HideClassPosition = Flag(reader["IS_Hide_Class_Position"]);
                dto.HideFullMark = Flag(reader["IS_Hide_FullMark"]);
                dto.HidePassMark = Flag(reader["IS_Hide_PassMark"]);
                dto.OptionalPercent = ToDbl(reader["Optional_Percentage_Deduction"]);
                dto.OptionalMode = dto.OptionalPercent <= 0 ? 0 : dto.OptionalPercent >= 100 ? 2 : 1;
                dto.FailOptional = Flag(reader["IS_Fail_Enable_Optional_Subject"]);
                dto.AddOptionalInFull = Flag(reader["IS_Add_Optional_Mark_In_FullMarks"]);
                dto.GradeOnGpa = reader["IS_Grade_BasePoint"] is DBNull || Flag(reader["IS_Grade_BasePoint"]);
                dto.GradeAsItIs = Flag(reader["IS_Enable_Grade_as_it_is_if_Fail"]);
                dto.SubExamFail = Flag(reader["IS_Enable_Fail_if_fail_in_sub_Exam"]);
                dto.AttendanceFrom = Day(reader["Attendance_FromDate"]);
                dto.AttendanceTo = Day(reader["Attendance_ToDate"]);
                dto.ScheduleID = ToInt(reader["Attendance_ScheduleID"]);
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT DISTINCT Countable_Mark FROM Exam_Publish_Sub_Countable_Mark
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            var marks = new List<double>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) marks.Add(ToDbl(reader["Countable_Mark"]));
            if (marks.Count > 1) dto.SameCountable = false;
            else if (marks.Count == 1) { dto.SameCountable = true; dto.CountableMark = marks[0]; }
        }

        await using (var cmd = new SqlCommand("""
SELECT Exam_Result_of_Subject.SubjectID, Subject.SubjectName,
       MAX(Exam_Result_of_Subject.TotalMark_ofSubject) AS TotalMark_ofSubject, Exam_Result_of_Subject.IS_Add_InExam
FROM Exam_Result_of_Subject INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Result_of_Subject.SchoolID = @SchoolID AND Exam_Result_of_Subject.EducationYearID = @EducationYearID
  AND Exam_Result_of_Subject.ClassID = @ClassID AND Exam_Result_of_Subject.ExamID = @ExamID
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Exam_Result_of_Subject.IS_Add_InExam
ORDER BY Exam_Result_of_Subject.SubjectID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Subjects.Add(new ExamPublishSubjectDto
                {
                    SubjectID = ToInt(reader["SubjectID"]),
                    SubjectName = Text(reader["SubjectName"]),
                    AddInExam = reader["IS_Add_InExam"] is DBNull || Flag(reader["IS_Add_InExam"]),
                    CountableMark = ToDbl(reader["TotalMark_ofSubject"])
                });
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT SubjectID, SUM(FullMarks) AS FullMarks FROM Exam_Full_Marks
WHERE SchoolID = @SchoolID AND ExamID = @ExamID AND ClassID = @ClassID AND EducationYearID = @EducationYearID
GROUP BY SubjectID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            var dist = new Dictionary<int, double>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) dist[ToInt(reader["SubjectID"])] = ToDbl(reader["FullMarks"]);
            foreach (var row in dto.Subjects)
                if (dist.TryGetValue(row.SubjectID, out var fm)) row.DistFullMark = fm;
        }

        await using (var cmd = new SqlCommand("""
SELECT DISTINCT Exam_Obtain_Marks.SubjectID, Exam_Obtain_Marks.SubExamID, Subject.SubjectName, Exam_SubExam_Name.SubExamName,
       MAX(Exam_Obtain_Marks.AddPercentage) AS AddPercentage
FROM Exam_Obtain_Marks
INNER JOIN Subject ON Exam_Obtain_Marks.SubjectID = Subject.SubjectID
INNER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE Exam_Obtain_Marks.SchoolID = @SchoolID AND Exam_Obtain_Marks.EducationYearID = @EducationYearID
  AND Exam_Obtain_Marks.ClassID = @ClassID AND Exam_Obtain_Marks.ExamID = @ExamID
GROUP BY Exam_Obtain_Marks.SubjectID, Exam_Obtain_Marks.SubExamID, Exam_SubExam_Name.SubExamName, Subject.SubjectName
ORDER BY Exam_Obtain_Marks.SubjectID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.SubExams.Add(new ExamPublishSubExamDto
                {
                    SubjectID = ToInt(reader["SubjectID"]),
                    SubExamID = ToInt(reader["SubExamID"]),
                    SubjectName = Text(reader["SubjectName"]),
                    SubExamName = Text(reader["SubExamName"]),
                    AddPercentage = ToDbl(reader["AddPercentage"])
                });
            }
        }

        dto.EqualSubExam = true;
        return dto;
    }

    public async Task<ExamResult> PublishResultAsync(SessionSnapshot session, ExamPublishRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.ExamID <= 0) return Fail("exam.select");
        if (request.SameCountable && request.CountableMark < 1) return Fail("exam.needPublishMark");
        if (request.OptionalMode == 1 && (request.OptionalPercent <= 0 || request.OptionalPercent >= 100)) return Fail("exam.needOptionalPct");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await ExecAsync(con, """
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks LEFT OUTER JOIN Exam_Full_Marks ON Exam_Obtain_Marks.SchoolID = Exam_Full_Marks.SchoolID AND Exam_Obtain_Marks.SubjectID = Exam_Full_Marks.SubjectID AND Exam_Obtain_Marks.ExamID = Exam_Full_Marks.ExamID AND Exam_Obtain_Marks.ClassID = Exam_Full_Marks.ClassID AND ISNULL(Exam_Obtain_Marks.SubExamID, 0) = ISNULL(Exam_Full_Marks.SubExamID, 0) AND Exam_Obtain_Marks.EducationYearID = Exam_Full_Marks.EducationYearID
WHERE (Exam_Full_Marks.FullMarks IS NULL) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.ClassID = @ClassID)
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks LEFT OUTER JOIN StudentRecord ON Exam_Obtain_Marks.EducationYearID = StudentRecord.EducationYearID AND Exam_Obtain_Marks.SchoolID = StudentRecord.SchoolID AND Exam_Obtain_Marks.SubjectID = StudentRecord.SubjectID AND Exam_Obtain_Marks.StudentClassID = StudentRecord.StudentClassID
WHERE (StudentRecord.StudentID IS NULL) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.ClassID = @ClassID)
DELETE FROM Exam_Result_of_Subject FROM Exam_Result_of_Subject LEFT OUTER JOIN StudentRecord ON Exam_Result_of_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Result_of_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Result_of_Subject.EducationYearID = StudentRecord.EducationYearID AND Exam_Result_of_Subject.StudentClassID = StudentRecord.StudentClassID
WHERE (StudentRecord.StudentRecordID IS NULL) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.ClassID = @ClassID)
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks INNER JOIN StudentsClass ON Exam_Obtain_Marks.StudentClassID = StudentsClass.StudentClassID
WHERE (StudentsClass.New_StudentClassID IS NOT NULL) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID)
DELETE FROM Exam_Result_of_Student FROM StudentsClass INNER JOIN Exam_Result_of_Student ON StudentsClass.StudentClassID = Exam_Result_of_Student.StudentClassID
WHERE (StudentsClass.New_StudentClassID IS NOT NULL) AND (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID)
DELETE FROM Exam_Result_of_Subject FROM StudentsClass INNER JOIN Exam_Result_of_Subject ON StudentsClass.StudentClassID = Exam_Result_of_Subject.StudentClassID
WHERE (StudentsClass.New_StudentClassID IS NOT NULL) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID)
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks INNER JOIN Student ON Exam_Obtain_Marks.StudentID = Student.StudentID
WHERE (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Student.Status = N'Rejected')
DELETE FROM Exam_Result_of_Subject FROM Exam_Result_of_Subject INNER JOIN Student ON Exam_Result_of_Subject.StudentID = Student.StudentID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Student.Status = N'Rejected')
DELETE FROM Exam_Result_of_Student FROM Exam_Result_of_Student INNER JOIN Student ON Exam_Result_of_Student.StudentID = Student.StudentID
WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Student.Status = N'Rejected')
""", session, ct, c =>
        {
            c.Parameters.AddWithValue("@ClassID", request.ClassID);
            c.Parameters.AddWithValue("@ExamID", request.ExamID);
        });

        if (request.EqualSubExam)
        {
            await ExecAsync(con, """
UPDATE Exam_Obtain_Marks SET AddPercentage = 100
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", request.ClassID);
                c.Parameters.AddWithValue("@ExamID", request.ExamID);
            });
        }
        else
        {
            foreach (var row in request.SubExams)
            {
                await ExecAsync(con, """
UPDATE Exam_Obtain_Marks SET AddPercentage = @AddPercentage
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
  AND SubjectID = @SubjectID AND SubExamID = @SubExamID
""", session, ct, c =>
                {
                    c.Parameters.AddWithValue("@ClassID", request.ClassID);
                    c.Parameters.AddWithValue("@ExamID", request.ExamID);
                    c.Parameters.AddWithValue("@SubjectID", row.SubjectID);
                    c.Parameters.AddWithValue("@SubExamID", row.SubExamID);
                    c.Parameters.AddWithValue("@AddPercentage", row.AddPercentage);
                });
            }
        }

        var subjects = request.Subjects.Count > 0 ? request.Subjects : (await GetPublishSettingAsync(session, request.ClassID, request.ExamID, ct)).Subjects;
        foreach (var row in subjects)
        {
            var mark = request.SameCountable ? request.CountableMark : row.CountableMark;
            if (mark < 1) continue;
            await ExecAsync(con, """
IF NOT EXISTS(SELECT * FROM Exam_Publish_Sub_Countable_Mark WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID)
INSERT INTO Exam_Publish_Sub_Countable_Mark (SchoolID, RegistrationID, EducationYearID, SubjectID, ExamID, ClassID, Countable_Mark)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @SubjectID, @ExamID, @ClassID, @Countable_Mark)
ELSE
UPDATE Exam_Publish_Sub_Countable_Mark SET Countable_Mark = @Countable_Mark
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", request.ClassID);
                c.Parameters.AddWithValue("@ExamID", request.ExamID);
                c.Parameters.AddWithValue("@SubjectID", row.SubjectID);
                c.Parameters.AddWithValue("@Countable_Mark", mark);
            });
            if (!request.SameCountable)
            {
                await ExecAsync(con, """
UPDATE Exam_Result_of_Subject SET IS_Add_InExam = @IS_Add_InExam
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID
""", session, ct, c =>
                {
                    c.Parameters.AddWithValue("@ClassID", request.ClassID);
                    c.Parameters.AddWithValue("@ExamID", request.ExamID);
                    c.Parameters.AddWithValue("@SubjectID", row.SubjectID);
                    c.Parameters.AddWithValue("@IS_Add_InExam", row.AddInExam ? 1 : 0);
                });
            }
        }

        var optional = request.OptionalMode == 0 ? 0 : request.OptionalMode == 2 ? 100 : request.OptionalPercent;
        await ExecAsync(con, """
IF NOT EXISTS(SELECT * FROM Exam_Publish_Setting WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID)
INSERT INTO Exam_Publish_Setting
    (SchoolID, RegistrationID, EducationYearID, ClassID, ExamID, IS_Fail_Enable_Optional_Subject, IS_Add_Optional_Mark_In_FullMarks,
     IS_Enable_Grade_as_it_is_if_Fail, IS_Enable_Fail_if_fail_in_sub_Exam, Optional_Percentage_Deduction, IS_Published, Exam_Position_Format,
     IS_Hide_Sec_Position, IS_Hide_Class_Position, Attendance_FromDate, Attendance_ToDate, Attendance_ScheduleID, IS_Hide_FullMark, IS_Hide_PassMark, IS_Grade_BasePoint)
VALUES
    (@SchoolID, @RegistrationID, @EducationYearID, @ClassID, @ExamID, @FailOptional, @AddOptionalInFull, @GradeAsItIs, @SubExamFail,
     @OptionalPct, 1, @PositionFormat, @HideSec, @HideClass, @FromDate, @ToDate, @ScheduleID, @HideFull, @HidePass, @GradeOnGpa)
ELSE
UPDATE Exam_Publish_Setting SET
    IS_Fail_Enable_Optional_Subject = @FailOptional, IS_Add_Optional_Mark_In_FullMarks = @AddOptionalInFull,
    IS_Enable_Grade_as_it_is_if_Fail = @GradeAsItIs, IS_Enable_Fail_if_fail_in_sub_Exam = @SubExamFail,
    Optional_Percentage_Deduction = @OptionalPct, IS_PUBLISHED = 1, Exam_Position_Format = @PositionFormat, Last_Published_Date = GETDATE(),
    IS_Hide_Sec_Position = @HideSec, IS_Hide_Class_Position = @HideClass, Attendance_FromDate = @FromDate, Attendance_ToDate = @ToDate,
    Attendance_ScheduleID = @ScheduleID, IS_Hide_FullMark = @HideFull, IS_Hide_PassMark = @HidePass, IS_Grade_BasePoint = @GradeOnGpa
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID
""", session, ct, c =>
        {
            c.Parameters.AddWithValue("@ClassID", request.ClassID);
            c.Parameters.AddWithValue("@ExamID", request.ExamID);
            c.Parameters.AddWithValue("@FailOptional", request.FailOptional);
            c.Parameters.AddWithValue("@AddOptionalInFull", request.AddOptionalInFull);
            c.Parameters.AddWithValue("@GradeAsItIs", request.GradeAsItIs);
            c.Parameters.AddWithValue("@SubExamFail", request.SubExamFail);
            c.Parameters.AddWithValue("@OptionalPct", optional);
            c.Parameters.AddWithValue("@PositionFormat", string.IsNullOrWhiteSpace(request.PositionFormat) ? "Point" : request.PositionFormat);
            c.Parameters.AddWithValue("@HideSec", request.HideSecPosition);
            c.Parameters.AddWithValue("@HideClass", request.HideClassPosition);
            c.Parameters.AddWithValue("@FromDate", (object?)request.AttendanceFrom?.Date ?? DBNull.Value);
            c.Parameters.AddWithValue("@ToDate", (object?)request.AttendanceTo?.Date ?? DBNull.Value);
            c.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            c.Parameters.AddWithValue("@HideFull", request.HideFullMark);
            c.Parameters.AddWithValue("@HidePass", request.HidePassMark);
            c.Parameters.AddWithValue("@GradeOnGpa", request.GradeOnGpa);
        });

        var fromText = request.AttendanceFrom?.ToString("d MMM yyyy", CultureInfo.InvariantCulture) ?? "";
        var toText = request.AttendanceTo?.ToString("d MMM yyyy", CultureInfo.InvariantCulture) ?? "";
        var format = string.IsNullOrWhiteSpace(request.PositionFormat) ? "Point" : request.PositionFormat;
        await using (var cmd = new SqlCommand("""
EXEC [dbo].[SP_Exam_Subject] @SchoolID, @EducationYearID, @ClassID, @ExamID
EXEC [dbo].[SP_Exam_Student] @SchoolID, @EducationYearID, @ClassID, @ExamID
EXEC [dbo].[SP_Exam_Attendance] @SchoolID, @EducationYearID, @ClassID, @ExamID, @RegistrationID, @From_Date, @To_Date, @ScheduleID
EXEC [dbo].[HighestMark_Position] @SchoolID, @EducationYearID, @ClassID, @ExamID, @Exam_Position_Format
""", con))
        {
            cmd.CommandTimeout = 0;
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
            cmd.Parameters.AddWithValue("@ExamID", request.ExamID);
            cmd.Parameters.AddWithValue("@From_Date", string.IsNullOrWhiteSpace(fromText) ? DBNull.Value : fromText);
            cmd.Parameters.AddWithValue("@To_Date", string.IsNullOrWhiteSpace(toText) ? DBNull.Value : toText);
            cmd.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            cmd.Parameters.AddWithValue("@Exam_Position_Format", format);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        return Ok(request.ExamID);
    }

    public async Task<ExamResult> DeletePublishedResultAsync(SessionSnapshot session, ExamDeleteResultRequest? request, CancellationToken ct)
    {
        if (request is null || request.ExamID <= 0) return Fail("exam.selectExam");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        if (request.ClassID == 0)
        {
            await ExecAsync(con, """
DELETE FROM Exam_Result_of_Student WHERE ExamID = @ExamID AND EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", session, ct, c => c.Parameters.AddWithValue("@ExamID", request.ExamID));
        }
        else if (request.SubjectID == 0)
        {
            await ExecAsync(con, """
DELETE FROM Exam_Result_of_Student WHERE ExamID = @ExamID AND EducationYearID = @EducationYearID AND SchoolID = @SchoolID AND ClassID = @ClassID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ExamID", request.ExamID);
                c.Parameters.AddWithValue("@ClassID", request.ClassID);
            });
        }
        else if (request.SubExamID == 0)
        {
            await ExecAsync(con, """
DELETE FROM Exam_Result_of_Subject WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID
DELETE FROM Exam_Obtain_Marks WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ExamID", request.ExamID);
                c.Parameters.AddWithValue("@ClassID", request.ClassID);
                c.Parameters.AddWithValue("@SubjectID", request.SubjectID);
            });
        }
        else
        {
            await ExecAsync(con, """
DELETE FROM Exam_Obtain_Marks WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID AND SubExamID = @SubExamID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ExamID", request.ExamID);
                c.Parameters.AddWithValue("@ClassID", request.ClassID);
                c.Parameters.AddWithValue("@SubjectID", request.SubjectID);
                c.Parameters.AddWithValue("@SubExamID", request.SubExamID);
            });
            await using var check = new SqlCommand("""
SELECT COUNT(*) FROM Exam_Obtain_Marks eom
INNER JOIN Exam_SubExam_Name sen ON eom.SubExamID = sen.SubExamID
WHERE sen.SchoolID = @SchoolID AND eom.EducationYearID = @EducationYearID AND eom.ExamID = @ExamID AND eom.ClassID = @ClassID AND eom.SubjectID = @SubjectID
""", con);
            AddSession(check, session);
            check.Parameters.AddWithValue("@ExamID", request.ExamID);
            check.Parameters.AddWithValue("@ClassID", request.ClassID);
            check.Parameters.AddWithValue("@SubjectID", request.SubjectID);
            if (ToInt(await check.ExecuteScalarAsync(ct)) < 1)
            {
                await ExecAsync(con, """
DELETE FROM Exam_Result_of_Subject WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ExamID = @ExamID AND ClassID = @ClassID AND SubjectID = @SubjectID
""", session, ct, c =>
                {
                    c.Parameters.AddWithValue("@ExamID", request.ExamID);
                    c.Parameters.AddWithValue("@ClassID", request.ClassID);
                    c.Parameters.AddWithValue("@SubjectID", request.SubjectID);
                });
            }
        }

        return Ok(request.ExamID);
    }

    public async Task<ExamMeritListDto> GetMeritListAsync(SessionSnapshot session, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? passStatus, CancellationToken ct)
    {
        var dto = new ExamMeritListDto();
        if (classId <= 0 || examId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        dto.Title = await ScalarTextAsync(con, """
SELECT CreateClass.Class + ' - ' + Exam_Name.ExamName
FROM CreateClass, Exam_Name WHERE CreateClass.ClassID = @ClassID AND Exam_Name.ExamID = @ExamID
""", session, ct, c =>
        {
            c.Parameters.AddWithValue("@ClassID", classId);
            c.Parameters.AddWithValue("@ExamID", examId);
        });

        var rows = new Dictionary<int, ExamMeritRowDto>();
        await using (var cmd = new SqlCommand("""
SELECT StudentsClass.StudentClassID, Student.StudentID, Student.ID, StudentsClass.RollNo, Student.StudentsName,
       Exam_Result_of_Student.StudentResultID, Exam_Result_of_Student.ObtainedMark_ofStudent,
       Exam_Result_of_Student.Student_Grade, Exam_Result_of_Student.Student_Point,
       TRY_CAST(Exam_Result_of_Student.Position_InExam_Class AS int) AS Position_InExam_Class,
       TRY_CAST(Exam_Result_of_Student.Position_InExam_Subsection AS int) AS Position_InExam_Subsection,
       Student.SMSPhoneNo, Exam_Result_of_Student.PassStatus_InSubject, Exam_Result_of_Student.Average
FROM StudentsClass
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN Exam_Result_of_Student ON StudentsClass.StudentClassID = Exam_Result_of_Student.StudentClassID
WHERE StudentsClass.ClassID = @ClassID AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @SubjectGroupID AND StudentsClass.ShiftID LIKE @ShiftID
  AND StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.SchoolID = @SchoolID
  AND Exam_Result_of_Student.ExamID = @ExamID AND Exam_Result_of_Student.StudentPublishStatus = N'Pub'
  AND Exam_Result_of_Student.PassStatus_InSubject LIKE @PassStatus
ORDER BY Position_InExam_Class, CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS FLOAT) ELSE 0 END
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@SubjectGroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            cmd.Parameters.AddWithValue("@PassStatus", string.IsNullOrWhiteSpace(passStatus) ? "%" : passStatus);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentResultID"]);
                var row = new ExamMeritRowDto
                {
                    StudentResultID = id,
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    Phone = Text(reader["SMSPhoneNo"]),
                    RollNo = Text(reader["RollNo"]),
                    Total = ToDbl(reader["ObtainedMark_ofStudent"]),
                    Grade = Text(reader["Student_Grade"]),
                    Point = ToDbl(reader["Student_Point"]),
                    Average = Text(reader["Average"]),
                    PositionClass = Text(reader["Position_InExam_Class"]),
                    PositionSection = Text(reader["Position_InExam_Subsection"]),
                    PassStatus = Text(reader["PassStatus_InSubject"])
                };
                rows[id] = row;
                dto.Rows.Add(row);
            }
        }

        if (rows.Count == 0) return dto;
        await using (var cmd = new SqlCommand("""
SELECT Exam_Result_of_Subject.StudentResultID, Subject.SubjectName, Exam_Result_of_Subject.PassStatus_Subject,
       Exam_Result_of_Subject.SubjectType, Exam_Result_of_Subject.ObtainedMark_ofSubject AS Mark
FROM Exam_Result_of_Subject INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Result_of_Subject.SchoolID = @SchoolID AND Exam_Result_of_Subject.EducationYearID = @EducationYearID
  AND Exam_Result_of_Subject.ExamID = @ExamID AND Exam_Result_of_Subject.ClassID = @ClassID
ORDER BY ISNULL(Subject.SN, 999), Subject.SubjectID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentResultID"]);
                if (!rows.TryGetValue(id, out var row)) continue;
                var mark = reader["Mark"] is DBNull ? "" : ToDbl(reader["Mark"]).ToString("0.##");
                row.Marks.Add(new ExamMeritSubjectMarkDto
                {
                    Name = Text(reader["SubjectName"]),
                    Mark = mark,
                    PassStatus = Text(reader["PassStatus_Subject"]),
                    SubjectType = Text(reader["SubjectType"])
                });
            }
        }
        return dto;
    }

    public async Task<ExamMeritListDto> GetMeritSubjectAsync(SessionSnapshot session, int classId, int examId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken ct)
    {
        var dto = new ExamMeritListDto();
        if (classId <= 0 || examId <= 0 || subjectId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var rows = new Dictionary<int, ExamMeritRowDto>();
        await using (var cmd = new SqlCommand("""
SELECT StudentsClass.StudentClassID, Student.StudentID, Student.ID, Student.StudentsName, Student.SMSPhoneNo, StudentsClass.RollNo,
       Subject.SubjectName, Exam_Result_of_Subject.ObtainedMark_ofSubject, Exam_Result_of_Subject.SubjectGrades, Exam_Result_of_Subject.SubjectPoint,
       CAST(Exam_Result_of_Subject.Position_InSubject_Class AS int) AS Position_InSubject_Class,
       CAST(Exam_Result_of_Subject.Position_InSubject_Subsection AS int) AS Position_InSubject_Subsection,
       Exam_Result_of_Subject.PassStatus_Subject, Exam_Result_of_Subject.StudentResultID, Exam_Result_of_Subject.SubjectID
FROM Exam_Result_of_Student
INNER JOIN Exam_Result_of_Subject ON Exam_Result_of_Student.StudentResultID = Exam_Result_of_Subject.StudentResultID
INNER JOIN Subject ON Subject.SubjectID = Exam_Result_of_Subject.SubjectID
INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.ClassID = @ClassID AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @SubjectGroupID AND StudentsClass.ShiftID LIKE @ShiftID
  AND Student.Status = N'Active' AND StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.SchoolID = @SchoolID
  AND Exam_Result_of_Subject.ExamID = @ExamID AND Exam_Result_of_Subject.SubjectID = @SubjectID
  AND Exam_Result_of_Student.StudentPublishStatus = N'Pub'
ORDER BY StudentsClass.RollNo
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@SubjectGroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentResultID"]);
                var row = new ExamMeritRowDto
                {
                    StudentResultID = id,
                    SubjectID = subjectId,
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    Phone = Text(reader["SMSPhoneNo"]),
                    RollNo = Text(reader["RollNo"]),
                    Total = ToDbl(reader["ObtainedMark_ofSubject"]),
                    Grade = Text(reader["SubjectGrades"]),
                    Point = ToDbl(reader["SubjectPoint"]),
                    PositionClass = Text(reader["Position_InSubject_Class"]),
                    PositionSection = Text(reader["Position_InSubject_Subsection"]),
                    PassStatus = Text(reader["PassStatus_Subject"]),
                    SubjectName = Text(reader["SubjectName"])
                };
                rows[id] = row;
                dto.Rows.Add(row);
            }
        }

        if (rows.Count == 0) return dto;
        dto.Title = dto.Rows[0].SubjectName;
        await using (var cmd = new SqlCommand("""
SELECT Exam_Obtain_Marks.StudentResultID, Exam_SubExam_Name.SubExamName,
       ISNULL(CAST(Exam_Obtain_Marks.MarksObtained AS NVARCHAR(50)), 'A') AS MarksObtained
FROM Exam_Obtain_Marks LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE Exam_Obtain_Marks.SchoolID = @SchoolID AND Exam_Obtain_Marks.EducationYearID = @EducationYearID
  AND Exam_Obtain_Marks.ExamID = @ExamID AND Exam_Obtain_Marks.SubjectID = @SubjectID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentResultID"]);
                if (!rows.TryGetValue(id, out var row)) continue;
                row.Marks.Add(new ExamMeritSubjectMarkDto
                {
                    Name = Text(reader["SubExamName"]),
                    Mark = Text(reader["MarksObtained"])
                });
            }
        }
        return dto;
    }

    public async Task<ExamResultCardSheetDto> GetResultCardsAsync(SessionSnapshot session, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken ct)
    {
        var dto = new ExamResultCardSheetDto();
        if (classId <= 0 || examId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        try
        {
            await using (var cmd = new SqlCommand("SELECT SchoolName, Address, Phone, Teacher_Sign, Principal_Sign FROM SchoolInfo WHERE SchoolID = @SchoolID", con))
            {
                AddSchool(cmd, session);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    dto.SchoolName = Text(reader["SchoolName"]);
                    dto.Address = Text(reader["Address"]);
                    dto.Phone = Text(reader["Phone"]);
                    dto.TeacherSignDataUrl = ToDataUrl(reader["Teacher_Sign"] as byte[]);
                    dto.PrincipalSignDataUrl = ToDataUrl(reader["Principal_Sign"] as byte[]);
                }
            }
        }
        catch
        {
            await using var cmd = new SqlCommand("SELECT SchoolName, Address, Phone FROM SchoolInfo WHERE SchoolID = @SchoolID", con);
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.SchoolName = Text(reader["SchoolName"]);
                dto.Address = Text(reader["Address"]);
                dto.Phone = Text(reader["Phone"]);
            }
        }

        DateTime? attFrom = null, attTo = null;
        var attSchedule = 0;
        try
        {
            await using var cmd = new SqlCommand("""
SELECT IS_Hide_FullMark, IS_Hide_PassMark, IS_Hide_Class_Position, IS_Hide_Sec_Position,
       Attendance_FromDate, Attendance_ToDate, ISNULL(Attendance_ScheduleID, 0) AS Attendance_ScheduleID
FROM Exam_Publish_Setting
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.HideFullMark = Flag(reader["IS_Hide_FullMark"]);
                dto.HidePassMark = Flag(reader["IS_Hide_PassMark"]);
                dto.HideClassPosition = Flag(reader["IS_Hide_Class_Position"]);
                dto.HideSecPosition = Flag(reader["IS_Hide_Sec_Position"]);
                attFrom = Day(reader["Attendance_FromDate"]);
                attTo = Day(reader["Attendance_ToDate"]);
                attSchedule = ToInt(reader["Attendance_ScheduleID"]);
            }
        }
        catch
        {
        }

        try
        {
            await using var cmd = new SqlCommand("""
SELECT DISTINCT egs.MaxPercentage, egs.MinPercentage, egs.Grades, egs.Point, ISNULL(egs.Comments, '') AS Comments
FROM Exam_Grading_System egs
INNER JOIN Exam_Grading_Assign ega ON egs.GradeNameID = ega.GradeNameID AND egs.SchoolID = ega.SchoolID
WHERE ega.SchoolID = @SchoolID AND ega.EducationYearID = @EducationYearID AND ega.ClassID = @ClassID AND ega.ExamID = @ExamID
ORDER BY egs.MaxPercentage DESC
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Grades.Add(new ExamGradeBandViewDto
                {
                    Marks = $"{ToDbl(reader["MinPercentage"]):0}-{ToDbl(reader["MaxPercentage"]):0}",
                    Grade = Text(reader["Grades"]),
                    Point = ToDbl(reader["Point"]),
                    Comments = Text(reader["Comments"])
                });
            }
        }
        catch
        {
        }

        var ids = (studentIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0).Take(200).ToList();
        var sql = """
SELECT ers.StudentResultID, ers.ObtainedMark_ofStudent, ers.Student_Grade, ers.Student_Point, ers.Average,
       ers.ObtainedPercentage_ofStudent, ers.TotalMark_ofStudent, ers.Position_InExam_Class, ers.Position_InExam_Subsection,
       ers.PassStatus_Student, st.StudentsName, st.ID, st.StudentID, sc.RollNo, cc.Class AS ClassName,
       ISNULL(cs.Section, '') AS SectionName, ISNULL(csh.Shift, '') AS ShiftName, ISNULL(csg.SubjectGroup, '') AS GroupName,
       en.ExamName, sc.StudentClassID,
       ISNULL(TRY_CAST(REPLACE(REPLACE(sc.RollNo, '$', ''), ',', '') AS INT), 999999) AS RollNoSortNumber
FROM Exam_Result_of_Student ers
INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN Student st ON sc.StudentID = st.StudentID
INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
INNER JOIN Exam_Name en ON ers.ExamID = en.ExamID
LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID AND ers.ExamID = @ExamID AND sc.ClassID = @ClassID
  AND sc.SectionID LIKE @SectionID AND sc.SubjectGroupID LIKE @SubjectGroupID AND sc.ShiftID LIKE @ShiftID
""";
        if (ids.Count > 0) sql += " AND st.ID IN (" + string.Join(",", ids.Select((_, i) => "@Sid" + i)) + ")";
        sql += " ORDER BY RollNoSortNumber, sc.RollNo";

        var students = new Dictionary<int, ExamCardStudentDto>();
        await using (var cmd = new SqlCommand(sql, con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@SubjectGroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            for (var i = 0; i < ids.Count; i++) cmd.Parameters.AddWithValue("@Sid" + i, ids[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentResultID"]);
                var student = new ExamCardStudentDto
                {
                    StudentResultID = id,
                    StudentID = ToInt(reader["StudentID"]),
                    StudentClassID = ToInt(reader["StudentClassID"]),
                    StudentsName = Text(reader["StudentsName"]),
                    ID = Text(reader["ID"]),
                    RollNo = Text(reader["RollNo"]),
                    ClassName = Text(reader["ClassName"]),
                    SectionName = Text(reader["SectionName"]),
                    ShiftName = Text(reader["ShiftName"]),
                    GroupName = Text(reader["GroupName"]),
                    ExamName = Text(reader["ExamName"]),
                    Total = ToDbl(reader["ObtainedMark_ofStudent"]),
                    FullTotal = ToDbl(reader["TotalMark_ofStudent"]),
                    Grade = Text(reader["Student_Grade"]),
                    Point = ToDbl(reader["Student_Point"]),
                    Average = Text(reader["Average"]),
                    Percentage = Text(reader["ObtainedPercentage_ofStudent"]),
                    PositionClass = Text(reader["Position_InExam_Class"]),
                    PositionSection = Text(reader["Position_InExam_Subsection"]),
                    PassStatus = Text(reader["PassStatus_Student"]),
                    Comment = GradeComment(Text(reader["Student_Grade"]), dto.Grades)
                };
                students[id] = student;
                dto.Students.Add(student);
                if (!string.IsNullOrWhiteSpace(student.SectionName)) dto.HasSections = true;
            }
        }

        if (students.Count == 0) return dto;

        try
        {
            await FillCardPhotosAsync(con, session, dto.Students, ct);
        }
        catch
        {
        }

        try
        {
            await FillCardAttendanceAsync(con, session, classId, attFrom, attTo, attSchedule, dto.Students, ct);
        }
        catch
        {
        }

        var subNames = new List<string>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT DISTINCT Exam_SubExam_Name.SubExamName, Exam_SubExam_Name.Sub_ExamSN AS SN, Exam_SubExam_Name.SubExamID
FROM Exam_SubExam_Name
INNER JOIN Exam_Obtain_Marks ON Exam_SubExam_Name.SubExamID = Exam_Obtain_Marks.SubExamID
INNER JOIN Exam_Result_of_Student ON Exam_Obtain_Marks.StudentResultID = Exam_Result_of_Student.StudentResultID
INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
WHERE Exam_SubExam_Name.SchoolID = @SchoolID AND Exam_SubExam_Name.EducationYearID = @EducationYearID
  AND Exam_Result_of_Student.ExamID = @ExamID AND StudentsClass.ClassID = @ClassID
  AND Exam_Obtain_Marks.SchoolID = @SchoolID AND Exam_Obtain_Marks.EducationYearID = @EducationYearID
ORDER BY Exam_SubExam_Name.Sub_ExamSN, Exam_SubExam_Name.SubExamID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                subNames.Add(Text(reader["SubExamName"]));
        }
        catch
        {
        }
        dto.HasSubExams = subNames.Count > 0;
        dto.SubExamNames = subNames;

        var passBySubject = new Dictionary<int, double>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT SubjectID, SUM(ISNULL(Sub_PassMarks, FullMarks * 0.33)) AS PassMark
FROM Exam_Full_Marks
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
GROUP BY SubjectID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) passBySubject[ToInt(reader["SubjectID"])] = ToDbl(reader["PassMark"]);
        }
        catch
        {
        }

        var fullBySub = new Dictionary<(int Subject, int Sub), (double Full, double Pass)>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT SubjectID, ISNULL(SubExamID, 0) AS SubExamID, FullMarks, ISNULL(Sub_PassMarks, FullMarks * 0.33) AS PassMark
FROM Exam_Full_Marks
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                fullBySub[(ToInt(reader["SubjectID"]), ToInt(reader["SubExamID"]))] = (ToDbl(reader["FullMarks"]), ToDbl(reader["PassMark"]));
        }
        catch
        {
        }

        try
        {
            await using var cmd = new SqlCommand("""
SELECT ers.StudentResultID, sub.SubjectID,
       CASE WHEN ISNULL(sfg.SubjectType, '') = 'Optional' THEN ISNULL(sub.SubjectName, '') + ' *' ELSE ISNULL(sub.SubjectName, '') END AS SubjectName,
       ISNULL(ers.ObtainedMark_ofSubject, 0) AS ObtainedMark_ofSubject, ISNULL(ers.TotalMark_ofSubject, 0) AS TotalMark_ofSubject,
       ISNULL(ers.SubjectGrades, '') AS SubjectGrades, ISNULL(ers.SubjectPoint, 0) AS SubjectPoint,
       ISNULL(ers.PassStatus_Subject, 'Pass') AS PassStatus_Subject,
       ers.Position_InSubject_Class, ers.Position_InSubject_Subsection,
       ISNULL(ers.HighestMark_InSubject_Class, 0) AS HighestMark_InSubject_Class,
       ISNULL(ers.HighestMark_InSubject_Subsection, 0) AS HighestMark_InSubject_Subsection
FROM Exam_Result_of_Subject ers
INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
LEFT JOIN SubjectForGroup sfg ON sub.SubjectID = sfg.SubjectID AND sc.ClassID = sfg.ClassID AND sc.SubjectGroupID = sfg.SubjectGroupID AND ers.SchoolID = sfg.SchoolID
WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID AND ers.ExamID = @ExamID AND ers.ClassID = @ClassID
  AND ISNULL(ers.IS_Add_InExam, 1) = 1
ORDER BY ISNULL(sub.SN, 999), sub.SubjectName
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var sid = ToInt(reader["StudentResultID"]);
                if (!students.TryGetValue(sid, out var student)) continue;
                var obtained = ToDbl(reader["ObtainedMark_ofSubject"]);
                var grade = Text(reader["SubjectGrades"]);
                var passStatus = Text(reader["PassStatus_Subject"]);
                var absent = obtained == 0 && string.IsNullOrWhiteSpace(grade);
                var failed = grade.Equals("F", StringComparison.OrdinalIgnoreCase) || passStatus.Equals("Fail", StringComparison.OrdinalIgnoreCase) || passStatus.Equals("F", StringComparison.OrdinalIgnoreCase);
                var subjectId = ToInt(reader["SubjectID"]);
                var full = ToDbl(reader["TotalMark_ofSubject"]);
                var pass = passBySubject.TryGetValue(subjectId, out var pm) ? pm : full * 0.33;
                student.Subjects.Add(new ExamCardSubjectDto
                {
                    SubjectID = subjectId,
                    SubjectName = Text(reader["SubjectName"]),
                    Obtained = absent ? "Abs" : obtained.ToString(obtained % 1 == 0 ? "0" : "0.0"),
                    FullMark = full,
                    PassMark = pass,
                    Grade = string.IsNullOrWhiteSpace(grade) ? "-" : grade,
                    Point = ToDbl(reader["SubjectPoint"]),
                    PassStatus = passStatus,
                    PositionClass = Text(reader["Position_InSubject_Class"]),
                    PositionSection = Text(reader["Position_InSubject_Subsection"]),
                    HighestClass = ToDbl(reader["HighestMark_InSubject_Class"]) > 0 ? ToDbl(reader["HighestMark_InSubject_Class"]).ToString("0") : "-",
                    HighestSection = ToDbl(reader["HighestMark_InSubject_Subsection"]) > 0 ? ToDbl(reader["HighestMark_InSubject_Subsection"]).ToString("0") : "-",
                    Failed = failed
                });
            }
        }
        catch
        {
        }

        if (dto.HasSubExams)
        {
            try
            {
                await using var cmd = new SqlCommand("""
SELECT Exam_Obtain_Marks.StudentResultID, Exam_Obtain_Marks.SubjectID, ISNULL(Exam_Obtain_Marks.SubExamID, 0) AS SubExamID,
       Exam_SubExam_Name.SubExamName, Exam_Obtain_Marks.MarksObtained, Exam_Obtain_Marks.AbsenceStatus
FROM Exam_Obtain_Marks LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE Exam_Obtain_Marks.SchoolID = @SchoolID AND Exam_Obtain_Marks.EducationYearID = @EducationYearID
  AND Exam_Obtain_Marks.ExamID = @ExamID AND Exam_Obtain_Marks.ClassID = @ClassID
""", con);
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", classId);
                cmd.Parameters.AddWithValue("@ExamID", examId);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var sid = ToInt(reader["StudentResultID"]);
                    if (!students.TryGetValue(sid, out var student)) continue;
                    var subjectId = ToInt(reader["SubjectID"]);
                    var subject = student.Subjects.FirstOrDefault(x => x.SubjectID == subjectId);
                    if (subject is null) continue;
                    var subId = ToInt(reader["SubExamID"]);
                    var obtained = reader["MarksObtained"];
                    var absent = string.Equals(Text(reader["AbsenceStatus"]), "Absent", StringComparison.OrdinalIgnoreCase) || obtained is DBNull;
                    var pair = fullBySub.TryGetValue((subjectId, subId), out var fm) ? fm : (0, 0);
                    subject.Subs.Add(new ExamCardSubMarkDto
                    {
                        SubExamID = subId,
                        SubExamName = Text(reader["SubExamName"]),
                        Obtained = absent ? "A" : ToDbl(obtained).ToString("0.##"),
                        FullMark = pair.Full,
                        PassMark = pair.Pass
                    });
                }
            }
            catch
            {
            }
        }

        return dto;
    }

    public async Task<ExamAnalyticalDto> GetAnalyticalAsync(SessionSnapshot session, int classId, int examId, CancellationToken ct)
    {
        var dto = new ExamAnalyticalDto();
        if (classId <= 0 || examId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        dto.SchoolName = await ScalarTextAsync(con, "SELECT SchoolName FROM SchoolInfo WHERE SchoolID = @SchoolID", session, ct);
        dto.ClassName = await ScalarTextAsync(con, "SELECT Class FROM CreateClass WHERE ClassID = @ClassID", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId), schoolOnly: true);
        dto.ExamName = await ScalarTextAsync(con, "SELECT ExamName FROM Exam_Name WHERE ExamID = @ExamID AND SchoolID = @SchoolID", session, ct, c => c.Parameters.AddWithValue("@ExamID", examId), schoolOnly: true);

        await using (var cmd = new SqlCommand("""
SELECT COUNT(*) AS TotalStudents,
       SUM(CASE WHEN PassStatus_InSubject LIKE 'P%' THEN 1 ELSE 0 END) AS Passed,
       SUM(CASE WHEN PassStatus_InSubject LIKE 'F%' THEN 1 ELSE 0 END) AS Failed
FROM Exam_Result_of_Student
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.TotalStudents = ToInt(reader["TotalStudents"]);
                dto.Passed = ToInt(reader["Passed"]);
                dto.Failed = ToInt(reader["Failed"]);
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT Student_Grade AS Grade, COUNT(*) AS StudentCount
FROM Exam_Result_of_Student
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
  AND Student_Grade IS NOT NULL AND LTRIM(RTRIM(Student_Grade)) <> ''
GROUP BY Student_Grade
ORDER BY CASE Student_Grade WHEN 'A+' THEN 1 WHEN 'A' THEN 2 WHEN 'A-' THEN 3 WHEN 'B' THEN 4 WHEN 'C' THEN 5 WHEN 'D' THEN 6 WHEN 'F' THEN 7 ELSE 8 END
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var count = ToInt(reader["StudentCount"]);
                dto.GradeDistribution.Add(new ExamStatRowDto
                {
                    Label = Text(reader["Grade"]),
                    Count = count,
                    Percentage = dto.TotalStudents == 0 ? 0 : Math.Round(count * 100.0 / dto.TotalStudents, 2)
                });
            }
        }

        var subjects = new List<(int Id, string Name)>();
        await using (var cmd = new SqlCommand("""
SELECT DISTINCT s.SubjectID, s.SubjectName, ISNULL(s.SN, 999) AS SN
FROM Subject s INNER JOIN Exam_Result_of_Subject ers ON s.SubjectID = ers.SubjectID
WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID AND ers.ClassID = @ClassID AND ers.ExamID = @ExamID
ORDER BY SN, s.SubjectName
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) subjects.Add((ToInt(reader["SubjectID"]), Text(reader["SubjectName"])));
        }

        foreach (var subject in subjects)
        {
            var stat = new ExamSubjectStatDto { SubjectName = subject.Name };
            await using (var cmd = new SqlCommand("""
SELECT ers.SubjectGrades, COUNT(*) AS GradeCount
FROM Exam_Result_of_Subject ers
WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID AND ers.ClassID = @ClassID
  AND ers.ExamID = @ExamID AND ers.SubjectID = @SubjectID AND ers.SubjectGrades IS NOT NULL AND LTRIM(RTRIM(ers.SubjectGrades)) <> ''
GROUP BY ers.SubjectGrades
""", con))
            {
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", classId);
                cmd.Parameters.AddWithValue("@ExamID", examId);
                cmd.Parameters.AddWithValue("@SubjectID", subject.Id);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                    stat.Grades.Add(new ExamStatRowDto { Label = Text(reader["SubjectGrades"]), Count = ToInt(reader["GradeCount"]) });
            }
            dto.SubjectStats.Add(stat);

            await using (var fail = new SqlCommand("""
SELECT SUM(CASE WHEN PassStatus_Subject IN ('F','Fail') OR SubjectGrades = 'F' THEN 1 ELSE 0 END) AS FailCount
FROM Exam_Result_of_Subject
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID AND SubjectID = @SubjectID
""", con))
            {
                AddSession(fail, session);
                fail.Parameters.AddWithValue("@ClassID", classId);
                fail.Parameters.AddWithValue("@ExamID", examId);
                fail.Parameters.AddWithValue("@SubjectID", subject.Id);
                var failCount = ToInt(await fail.ExecuteScalarAsync(ct));
                if (failCount > 0)
                    dto.UnsuccessfulSummary.Add(new ExamStatRowDto { Label = subject.Name, Count = failCount });
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT st.ID, st.StudentsName, sc.RollNo,
       STUFF((SELECT ', ' + Subject.SubjectName
              FROM Exam_Result_of_Subject ers2 INNER JOIN Subject ON Subject.SubjectID = ers2.SubjectID
              WHERE ers2.StudentResultID = erst.StudentResultID AND (ers2.PassStatus_Subject IN ('F','Fail') OR ers2.SubjectGrades = 'F')
              FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS FailedSubjects,
       (SELECT COUNT(*)
        FROM Exam_Result_of_Subject ers2
        WHERE ers2.StudentResultID = erst.StudentResultID
          AND (ers2.PassStatus_Subject IN ('F','Fail') OR ers2.SubjectGrades = 'F')) AS FailCount
FROM Exam_Result_of_Student erst
INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
INNER JOIN Student st ON sc.StudentID = st.StudentID
WHERE erst.SchoolID = @SchoolID AND erst.EducationYearID = @EducationYearID AND erst.ClassID = @ClassID AND erst.ExamID = @ExamID
  AND erst.PassStatus_InSubject LIKE 'F%'
ORDER BY CASE WHEN ISNUMERIC(sc.RollNo) = 1 THEN CAST(REPLACE(REPLACE(sc.RollNo, '$', ''), ',', '') AS FLOAT) ELSE 0 END
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.UnsuccessfulStudents.Add(new ExamFailStudentDto
                {
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    RollNo = Text(reader["RollNo"]),
                    FailedSubjects = Text(reader["FailedSubjects"]),
                    FailCount = ToInt(reader["FailCount"])
                });
            }
        }

        return dto;
    }

    private static async Task ExecAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.CommandTimeout = 0;
        AddSession(cmd, session);
        extra?.Invoke(cmd);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<string> ScalarTextAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null, bool schoolOnly = false)
    {
        await using var cmd = new SqlCommand(sql, con);
        if (schoolOnly) AddSchool(cmd, session); else AddSession(cmd, session);
        extra?.Invoke(cmd);
        return Text(await cmd.ExecuteScalarAsync(ct));
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

    private static string GradeComment(string grade, List<ExamGradeBandViewDto> bands)
    {
        var match = bands.FirstOrDefault(g => string.Equals(g.Grade, grade, StringComparison.OrdinalIgnoreCase));
        return match?.Comments?.Trim() ?? "";
    }

    private static async Task FillCardPhotosAsync(
        SqlConnection con, SessionSnapshot session, List<ExamCardStudentDto> students, CancellationToken ct)
    {
        var ids = students.Select(s => s.StudentID).Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0) return;
        var inList = string.Join(",", ids.Select((_, i) => "@P" + i));
        await using var cmd = new SqlCommand($"""
SELECT s.StudentID, si.Image
FROM dbo.Student s
LEFT OUTER JOIN dbo.Student_Image si ON s.StudentImageID = si.StudentImageID
WHERE s.SchoolID = @SchoolID AND s.StudentID IN ({inList}) AND si.Image IS NOT NULL
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        for (var i = 0; i < ids.Count; i++)
            cmd.Parameters.AddWithValue("@P" + i, ids[i]);

        var byId = students.Where(s => s.StudentID > 0)
            .GroupBy(s => s.StudentID)
            .ToDictionary(g => g.Key, g => g.ToList());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = ToInt(reader["StudentID"]);
            if (!byId.TryGetValue(id, out var rows)) continue;
            if (reader["Image"] is not byte[] bytes || bytes.Length == 0) continue;
            var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
            var url = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
            foreach (var row in rows) row.PhotoDataUrl = url;
        }
    }

    private static async Task FillCardAttendanceAsync(
        SqlConnection con, SessionSnapshot session, int classId, DateTime? from, DateTime? to, int scheduleId,
        List<ExamCardStudentDto> students, CancellationToken ct)
    {
        if (from is null || to is null || students.Count == 0) return;
        var working = "";
        try
        {
            await using var cmd = new SqlCommand(
                "SELECT dbo.F_Stu_WorkingDay(@SchoolID, @EducationYearID, @ClassID, @From, @To)", con);
            AddSchoolYear(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@From", from.Value);
            cmd.Parameters.AddWithValue("@To", to.Value);
            working = Text(await cmd.ExecuteScalarAsync(ct));
        }
        catch
        {
        }

        var classIds = students.Select(s => s.StudentClassID).Where(id => id > 0).Distinct().ToList();
        var counts = new Dictionary<(int Scid, string Status), int>();
        if (classIds.Count > 0)
        {
            var inList = string.Join(",", classIds.Select((_, i) => "@Sc" + i));
            await using var cmd = new SqlCommand($"""
SELECT StudentClassID, Attendance, COUNT(*) AS Cnt
FROM Attendance_Record
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
  AND StudentClassID IN ({inList})
  AND CAST(AttendanceDate AS DATE) >= CAST(@From AS DATE)
  AND CAST(AttendanceDate AS DATE) <= CAST(@To AS DATE)
  AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
GROUP BY StudentClassID, Attendance
""", con);
            AddSchoolYear(cmd, session);
            cmd.Parameters.AddWithValue("@From", from.Value);
            cmd.Parameters.AddWithValue("@To", to.Value);
            cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
            for (var i = 0; i < classIds.Count; i++) cmd.Parameters.AddWithValue("@Sc" + i, classIds[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                counts[(ToInt(reader["StudentClassID"]), Text(reader["Attendance"]))] = ToInt(reader["Cnt"]);
        }

        static string CountOf(Dictionary<(int Scid, string Status), int> map, int scid, string status) =>
            map.TryGetValue((scid, status), out var n) && n > 0 ? n.ToString() : "";

        foreach (var student in students)
        {
            var days = (working ?? "").Trim();
            student.WorkingDays = days.Length == 0 || days == "0" ? "" : days;
            student.PresentDays = CountOf(counts, student.StudentClassID, "Pre");
            student.AbsentDays = CountOf(counts, student.StudentClassID, "Abs");
            student.LeaveDays = CountOf(counts, student.StudentClassID, "Leave");
            student.LateAbsDays = CountOf(counts, student.StudentClassID, "Late Abs");
            student.LateDays = CountOf(counts, student.StudentClassID, "Late");
        }
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
    private static bool Flag(object? value)
    {
        if (value is null or DBNull) return false;
        if (value is bool b) return b;
        if (value is IConvertible && value is not string)
        {
            try { return Convert.ToInt32(value) != 0; } catch { }
        }
        var text = value.ToString()?.Trim();
        return text == "1" || string.Equals(text, "True", StringComparison.OrdinalIgnoreCase);
    }
    private static ExamResult Fail(string error) => new() { Error = error };
    private static ExamResult Ok(int id = 0, int count = 0) => new() { Succeeded = true, Id = id, Count = count };
}
