using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class ExamService
{
    public async Task<ExamSeatPlanSheetDto> GetSeatPlanAsync(
        SessionSnapshot session, int classId, int examId, string? groupId, string? sectionId, string? shiftId,
        string? studentIds, string? classIds, CancellationToken ct)
    {
        var dto = new ExamSeatPlanSheetDto();
        var ids = ParseIds(studentIds);
        var classes = ParseClassIds(classIds, classId);
        if (classes.Count == 0 && ids.Count == 0) return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await FillSchoolHeaderAsync(con, session, dto, ct);
        dto.ExamName = await LoadExamNameAsync(con, session, examId, ct);
        dto.Students = await QueryPrintStudentsAsync(con, session, classes, groupId, sectionId, shiftId, ids, null, ct);
        return dto;
    }

    public async Task<ExamResult> RandomizeSeatNumbersAsync(
        SessionSnapshot session, RandomSeatRequest? request, CancellationToken ct)
    {
        var classIds = (request?.ClassIds ?? []).Where(x => x > 0).Distinct().Take(80).ToList();
        if (classIds.Count == 0) return Fail("exam.needSeatClass");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var students = await QueryPrintStudentsAsync(
            con, session, classIds, request?.GroupId, request?.SectionId, request?.ShiftId, [], null, ct);
        if (students.Count == 0) return Fail("exam.empty");

        var mixed = MixClasses(students);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            var n = 0;
            for (var i = 0; i < mixed.Count; i++)
            {
                await using var cmd = new SqlCommand("""
UPDATE StudentsClass SET SeatNo = @SeatNo
WHERE StudentClassID = @StudentClassID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con, tx);
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@SeatNo", (i + 1).ToString());
                cmd.Parameters.AddWithValue("@StudentClassID", mixed[i].StudentClassID);
                n += await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return Ok(count: n);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return Fail("exam.failed");
        }
    }

    public async Task<ExamAdmitCardSheetDto> GetAdmitCardsAsync(
        SessionSnapshot session, int classId, int examId, string? groupId, string? sectionId, string? shiftId,
        string? studentIds, string? paymentStatus, CancellationToken ct)
    {
        var dto = new ExamAdmitCardSheetDto();
        var ids = ParseIds(studentIds);
        if (classId <= 0 && ids.Count == 0) return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await FillSchoolHeaderAsync(con, session, dto, ct);
        dto.ExamName = await LoadExamNameAsync(con, session, examId, ct);
        dto.EducationYear = await ScalarTextAsync(con, """
SELECT EducationYear FROM Education_Year
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", session, ct);
        dto.Students = await QueryPrintStudentsAsync(
            con, session, classId > 0 ? [classId] : [], groupId, sectionId, shiftId, ids, paymentStatus, ct);
        foreach (var student in dto.Students)
            student.EducationYear = dto.EducationYear;
        return dto;
    }

    public async Task<ExamResult> SaveExamSignAsync(SessionSnapshot session, SaveExamSignRequest? request, CancellationToken ct)
    {
        var kind = (request?.Kind ?? "").Trim().ToLowerInvariant();
        var column = kind switch
        {
            "teacher" => "Teacher_Sign",
            "principal" or "head" => "Principal_Sign",
            _ => ""
        };
        if (column.Length == 0) return Fail("exam.signKind");
        var bytes = DecodeImage(request?.Image);
        if (bytes is null || bytes.Length == 0) return Fail("exam.signImage");

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand($"UPDATE SchoolInfo SET {column} = @Image WHERE SchoolID = @SchoolID", con);
            AddSchool(cmd, session);
            cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = bytes;
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok();
        }
        catch
        {
            return Fail("exam.signFailed");
        }
    }

    private async Task FillSchoolHeaderAsync(SqlConnection con, SessionSnapshot session, ExamSeatPlanSheetDto dto, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT SchoolName FROM SchoolInfo WHERE SchoolID = @SchoolID", con);
        AddSchool(cmd, session);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            dto.SchoolName = Text(reader["SchoolName"]);
    }

    private async Task FillSchoolHeaderAsync(SqlConnection con, SessionSnapshot session, ExamAdmitCardSheetDto dto, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT SchoolName, Address, Phone, Teacher_Sign, Principal_Sign
FROM SchoolInfo WHERE SchoolID = @SchoolID
""", con);
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
            return;
        }
        catch
        {
        }

        await using var fallback = new SqlCommand("SELECT SchoolName, Address, Phone FROM SchoolInfo WHERE SchoolID = @SchoolID", con);
        AddSchool(fallback, session);
        await using var reader2 = await fallback.ExecuteReaderAsync(ct);
        if (await reader2.ReadAsync(ct))
        {
            dto.SchoolName = Text(reader2["SchoolName"]);
            dto.Address = Text(reader2["Address"]);
            dto.Phone = Text(reader2["Phone"]);
        }
    }

    private async Task<string> LoadExamNameAsync(SqlConnection con, SessionSnapshot session, int examId, CancellationToken ct)
    {
        if (examId <= 0) return "";
        return await ScalarTextAsync(con, """
SELECT ExamName FROM Exam_Name
WHERE ExamID = @ExamID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", session, ct, c => c.Parameters.AddWithValue("@ExamID", examId));
    }

    private static async Task<List<ExamPrintStudentDto>> QueryPrintStudentsAsync(
        SqlConnection con, SessionSnapshot session, IReadOnlyList<int> classIds, string? groupId, string? sectionId, string? shiftId,
        List<string> ids, string? paymentStatus, CancellationToken ct)
    {
        var sql = """
SELECT sc.StudentClassID, sc.ClassID, st.StudentsName, st.ID, st.StudentID, sc.RollNo, ISNULL(sc.SeatNo, '') AS SeatNo,
       cc.Class AS ClassName, ISNULL(cs.Section, '') AS SectionName,
       ISNULL(csh.Shift, '') AS ShiftName, ISNULL(csg.SubjectGroup, '') AS GroupName,
       ISNULL(TRY_CAST(REPLACE(REPLACE(sc.RollNo, '$', ''), ',', '') AS INT), 999999) AS RollNoSortNumber
FROM StudentsClass sc
INNER JOIN Student st ON sc.StudentID = st.StudentID
INNER JOIN CreateClass cc ON sc.ClassID = cc.ClassID
LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
WHERE sc.SchoolID = @SchoolID AND sc.EducationYearID = @EducationYearID AND st.Status = N'Active'
""";
        if (ids.Count > 0)
            sql += " AND st.ID IN (" + string.Join(",", ids.Select((_, i) => "@Sid" + i)) + ")";
        else if (classIds.Count > 0)
        {
            sql += " AND sc.ClassID IN (" + string.Join(",", classIds.Select((_, i) => "@Cid" + i)) + ")";
            if (classIds.Count == 1)
            {
                sql += """
 AND sc.SectionID LIKE @SectionID AND sc.SubjectGroupID LIKE @SubjectGroupID AND sc.ShiftID LIKE @ShiftID
""";
                var paid = (paymentStatus ?? "").Trim();
                if (paid.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    sql += """
 AND sc.StudentID IN (
    SELECT StudentID FROM Income_PayOrder
    WHERE SchoolID = @SchoolID AND ClassID = @ClassID AND EndDate < GETDATE()
    GROUP BY StudentID HAVING SUM(Receivable_Amount) = 0)
""";
                }
                else if (paid.Equals("Due", StringComparison.OrdinalIgnoreCase))
                {
                    sql += """
 AND sc.StudentID IN (
    SELECT StudentID FROM Income_PayOrder
    WHERE SchoolID = @SchoolID AND ClassID = @ClassID AND EndDate < GETDATE()
    GROUP BY StudentID HAVING SUM(Receivable_Amount) <> 0)
""";
                }
            }
        }
        else
            return [];

        sql += """
 ORDER BY CASE WHEN TRY_CAST(sc.SeatNo AS INT) IS NULL THEN 1 ELSE 0 END,
          TRY_CAST(sc.SeatNo AS INT), RollNoSortNumber, sc.RollNo
""";

        var students = new List<ExamPrintStudentDto>();
        await using var cmd = new SqlCommand(sql, con);
        AddSession(cmd, session);
        if (ids.Count == 0)
        {
            for (var i = 0; i < classIds.Count; i++)
                cmd.Parameters.AddWithValue("@Cid" + i, classIds[i]);
            if (classIds.Count == 1)
            {
                cmd.Parameters.AddWithValue("@ClassID", classIds[0]);
                cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
                cmd.Parameters.AddWithValue("@SubjectGroupID", Like(groupId));
                cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            }
        }
        for (var i = 0; i < ids.Count; i++)
            cmd.Parameters.AddWithValue("@Sid" + i, ids[i]);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            students.Add(new ExamPrintStudentDto
            {
                StudentClassID = ToInt(reader["StudentClassID"]),
                ClassID = ToInt(reader["ClassID"]),
                StudentID = ToInt(reader["StudentID"]),
                ID = Text(reader["ID"]),
                StudentsName = Text(reader["StudentsName"]),
                ClassName = Text(reader["ClassName"]),
                SectionName = Text(reader["SectionName"]),
                ShiftName = Text(reader["ShiftName"]),
                GroupName = Text(reader["GroupName"]),
                RollNo = Text(reader["RollNo"]),
                SeatNo = Text(reader["SeatNo"])
            });
        }
        return students;
    }

    private static List<int> ParseClassIds(string? classIds, int fallback)
    {
        var ids = (classIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .Take(80)
            .ToList();
        if (ids.Count == 0 && fallback > 0)
            ids.Add(fallback);
        return ids;
    }

    private static List<ExamPrintStudentDto> MixClasses(List<ExamPrintStudentDto> students)
    {
        var queues = students
            .GroupBy(x => x.ClassID)
            .Select(g =>
            {
                var list = g.ToList();
                Shuffle(list);
                return new Queue<ExamPrintStudentDto>(list);
            })
            .ToList();

        var mixed = new List<ExamPrintStudentDto>(students.Count);
        var lastClass = int.MinValue;
        while (queues.Count > 0)
        {
            var pick = queues
                .Where(q => q.Peek().ClassID != lastClass)
                .OrderByDescending(q => q.Count)
                .FirstOrDefault()
                ?? queues.OrderByDescending(q => q.Count).First();
            var item = pick.Dequeue();
            lastClass = item.ClassID;
            mixed.Add(item);
            if (pick.Count == 0)
                queues.Remove(pick);
        }
        return mixed;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static List<string> ParseIds(string? studentIds) =>
        (studentIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0).Take(200).ToList();

    private static byte[]? DecodeImage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var text = raw.Trim();
        var comma = text.IndexOf(',');
        if (comma >= 0 && text.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            text = text[(comma + 1)..];
        try
        {
            var bytes = Convert.FromBase64String(text);
            return bytes.Length is > 0 and <= 400_000 ? bytes : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? ToDataUrl(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0) return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
