using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class ExamService
{
    public async Task<IReadOnlyList<ExamOptionDto>> ListCumulativeNamesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryOptionsAsync(con, """
SELECT CumulativeNameID AS Id, CumulativeResultName AS Name
FROM Exam_Cumulative_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
ORDER BY Date DESC, CumulativeResultName
""", session, ct);
    }

    public async Task<ExamResult> CreateCumulativeNameAsync(SessionSnapshot session, SaveCumulativeNameRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0) return Fail("exam.needCuName");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var check = new SqlCommand("""
SELECT COUNT(*) FROM Exam_Cumulative_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND CumulativeResultName = @Name
""", con))
        {
            AddSession(check, session);
            check.Parameters.AddWithValue("@Name", name);
            if (ToInt(await check.ExecuteScalarAsync(ct)) > 0) return Fail("exam.cuExists");
        }

        await using var cmd = new SqlCommand("""
INSERT INTO Exam_Cumulative_Name (SchoolID, RegistrationID, EducationYearID, CumulativeResultName)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@Name", name);
        return Ok(ToInt(await cmd.ExecuteScalarAsync(ct)));
    }

    public async Task<ExamResult> UpdateCumulativeNameAsync(SessionSnapshot session, int id, SaveCumulativeNameRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (id <= 0 || name.Length == 0) return Fail("exam.needCuName");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var check = new SqlCommand("""
SELECT COUNT(*) FROM Exam_Cumulative_Name
WHERE SchoolID = @SchoolID AND CumulativeResultName = @Name AND CumulativeNameID <> @Id
""", con))
        {
            AddSchool(check, session);
            check.Parameters.AddWithValue("@Name", name);
            check.Parameters.AddWithValue("@Id", id);
            if (ToInt(await check.ExecuteScalarAsync(ct)) > 0) return Fail("exam.cuExists");
        }

        await using var cmd = new SqlCommand("""
UPDATE Exam_Cumulative_Name SET CumulativeResultName = @Name
WHERE CumulativeNameID = @Id AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
        AddSession(cmd, session);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok(id);
    }

    public async Task<CumulativePublishSettingDto> GetCumulativePublishSettingAsync(SessionSnapshot session, int classId, int cumulativeNameId, CancellationToken ct)
    {
        var dto = new CumulativePublishSettingDto();
        if (classId <= 0 || cumulativeNameId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        var selected = new Dictionary<int, (double Pct, bool Fail)>();
        await using (var cmd = new SqlCommand("""
SELECT ExamID, ExamAdd_Percentage, Exam_EnableFail
FROM Exam_Cumulative_ExamList
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                selected[ToInt(reader["ExamID"])] = (ToDbl(reader["ExamAdd_Percentage"]), Flag(reader["Exam_EnableFail"]));
        }

        await using (var cmd = new SqlCommand("""
SELECT Exam_Name.ExamID, Exam_Name.ExamName
FROM Exam_Publish_Setting
INNER JOIN Exam_Name ON Exam_Publish_Setting.ExamID = Exam_Name.ExamID
WHERE Exam_Publish_Setting.SchoolID = @SchoolID AND Exam_Publish_Setting.EducationYearID = @EducationYearID
  AND Exam_Publish_Setting.ClassID = @ClassID
ORDER BY Exam_Name.ExamID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var examId = ToInt(reader["ExamID"]);
                selected.TryGetValue(examId, out var row);
                dto.Exams.Add(new CumulativeExamChoiceDto
                {
                    ExamID = examId,
                    ExamName = Text(reader["ExamName"]),
                    Selected = selected.ContainsKey(examId),
                    Percentage = row.Pct,
                    EnableFail = row.Fail
                });
            }
        }

        var marks = new Dictionary<int, (double Full, bool Add)>();
        await using (var cmd = new SqlCommand("""
SELECT DISTINCT Exam_Cumulative_FullMarks.SubjectID, Exam_Cumulative_FullMarks.FullMarks, Exam_Cumulative_Subject.IS_Add_InExam
FROM Exam_Cumulative_FullMarks
INNER JOIN Exam_Cumulative_Subject ON Exam_Cumulative_FullMarks.SubjectID = Exam_Cumulative_Subject.SubjectID
  AND Exam_Cumulative_FullMarks.Cumulative_SettingID = Exam_Cumulative_Subject.Cumulative_SettingID
WHERE Exam_Cumulative_FullMarks.CumulativeNameID = @CumulativeNameID AND Exam_Cumulative_FullMarks.SchoolID = @SchoolID
  AND Exam_Cumulative_FullMarks.EducationYearID = @EducationYearID AND Exam_Cumulative_FullMarks.ClassID = @ClassID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                marks[ToInt(reader["SubjectID"])] = (ToDbl(reader["FullMarks"]), Flag(reader["IS_Add_InExam"]));
        }

        await using (var cmd = new SqlCommand("""
SELECT DISTINCT Subject.SubjectID, Subject.SubjectName, Subject.SN
FROM Exam_Full_Marks
INNER JOIN Subject ON Exam_Full_Marks.SubjectID = Subject.SubjectID
INNER JOIN Exam_Publish_Setting ON Exam_Full_Marks.SchoolID = Exam_Publish_Setting.SchoolID
  AND Exam_Full_Marks.EducationYearID = Exam_Publish_Setting.EducationYearID
  AND Exam_Full_Marks.ClassID = Exam_Publish_Setting.ClassID
  AND Exam_Full_Marks.ExamID = Exam_Publish_Setting.ExamID
WHERE Exam_Full_Marks.SchoolID = @SchoolID AND Exam_Full_Marks.EducationYearID = @EducationYearID
  AND Exam_Full_Marks.ClassID = @ClassID
ORDER BY Subject.SN
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var subjectId = ToInt(reader["SubjectID"]);
                marks.TryGetValue(subjectId, out var row);
                dto.Subjects.Add(new ExamPublishSubjectDto
                {
                    SubjectID = subjectId,
                    SubjectName = Text(reader["SubjectName"]),
                    AddInExam = !marks.ContainsKey(subjectId) || row.Add,
                    CountableMark = row.Full,
                    DistFullMark = row.Full
                });
            }
        }

        var distinctMarks = dto.Subjects.Select(x => x.CountableMark).Where(x => x > 0).Distinct().ToList();
        dto.SameCountable = distinctMarks.Count <= 1;
        if (dto.SameCountable && distinctMarks.Count == 1) dto.CountableMark = distinctMarks[0];

        await using (var cmd = new SqlCommand("""
SELECT TOP 1 * FROM Exam_Cumulative_Setting
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
ORDER BY Cumulative_SettingID DESC
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var optional = ToDbl(reader["Optional_Percentage_Deduction"]);
                dto.OptionalMode = optional == 0 ? 0 : optional >= 100 ? 2 : 1;
                dto.OptionalPercent = dto.OptionalMode == 1 ? optional : 0;
                dto.FailOptional = Flag(reader["IS_Fail_Enable_Optional_Subject"]);
                dto.AddOptionalInFull = Flag(reader["IS_Add_Optional_Mark_In_FullMarks"]);
                dto.GradeAsItIs = Flag(reader["IS_Enable_Grade_as_it_is_if_Fail"]);
                dto.HideSubExam = Flag(reader["IS_Hide_SubExam"]);
                dto.HideSecPosition = Flag(reader["IS_Hide_Sec_Position"]);
                dto.HideClassPosition = Flag(reader["IS_Hide_Class_Position"]);
                dto.GradeOnGpa = Flag(reader["IS_Grade_BasePoint"]);
                dto.PositionFormat = Text(reader["Exam_Position_Format"]);
                if (string.IsNullOrWhiteSpace(dto.PositionFormat)) dto.PositionFormat = "Point";
                dto.AttendanceFrom = Day(reader["Attendance_FromDate"]);
                dto.AttendanceTo = Day(reader["Attendance_ToDate"]);
                try { dto.ScheduleID = ToInt(reader["Attendance_ScheduleID"]); } catch { }
                dto.GradeNameID = ToInt(reader["GradeNameID"]);
            }
        }

        if (dto.GradeNameID <= 0)
        {
            dto.GradeNameID = ToInt(await ScalarTextAsync(con,
                "SELECT TOP 1 GradeNameID FROM Exam_Grade_Name WHERE SchoolID = @SchoolID", session, ct, schoolOnly: true));
        }

        return dto;
    }

    public async Task<ExamResult> PublishCumulativeResultAsync(SessionSnapshot session, CumulativePublishRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.CumulativeNameID <= 0) return Fail("exam.select");
        if (!request.Exams.Any(x => x.Selected)) return Fail("exam.needCuExam");
        if (request.SameCountable && request.CountableMark < 1) return Fail("exam.needPublishMark");
        if (request.OptionalMode == 1 && (request.OptionalPercent <= 0 || request.OptionalPercent >= 100)) return Fail("exam.needOptionalPct");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        try
        {
            await ExecSqlAsync(con, """
DELETE FROM Exam_Cumulative_ExamList
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
DELETE FROM Exam_Cumulative_FullMarks
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
DELETE FROM Exam_Cumulative_Subject
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
DELETE FROM Exam_Cumulative_Student
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
DELETE FROM Exam_Cumulative_Setting
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
""", session, ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", request.ClassID);
                c.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
            });

            var optional = request.OptionalMode == 0 ? 0 : request.OptionalMode == 2 ? 100 : request.OptionalPercent;
            var gradeId = request.GradeNameID;
            if (gradeId <= 0)
            {
                await using var gradeCmd = new SqlCommand("SELECT TOP 1 GradeNameID FROM Exam_Grade_Name WHERE SchoolID = @SchoolID", con);
                AddSchool(gradeCmd, session);
                gradeId = ToInt(await gradeCmd.ExecuteScalarAsync(ct));
            }

            int settingId;
            await using (var cmd = new SqlCommand("""
INSERT INTO Exam_Cumulative_Setting
    (CumulativeNameID, SchoolID, RegistrationID, EducationYearID, ClassID, IS_Fail_Enable_Optional_Subject,
     IS_Add_Optional_Mark_In_FullMarks, IS_Enable_Grade_as_it_is_if_Fail, Optional_Percentage_Deduction, Exam_Position_Format,
     IS_Hide_SubExam, IS_Hide_Sec_Position, IS_Hide_Class_Position, Attendance_FromDate, Attendance_ToDate,
     Attendance_ScheduleID, GradeNameID, IS_Grade_BasePoint)
VALUES
    (@CumulativeNameID, @SchoolID, @RegistrationID, @EducationYearID, @ClassID, @FailOptional, @AddOptionalInFull,
     @GradeAsItIs, @OptionalPct, @PositionFormat, @HideSub, @HideSec, @HideClass, @FromDate, @ToDate, @ScheduleID,
     @GradeNameID, @GradeOnGpa);
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con))
            {
                cmd.CommandTimeout = 0;
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                cmd.Parameters.AddWithValue("@FailOptional", request.FailOptional);
                cmd.Parameters.AddWithValue("@AddOptionalInFull", request.AddOptionalInFull);
                cmd.Parameters.AddWithValue("@GradeAsItIs", request.GradeAsItIs);
                cmd.Parameters.AddWithValue("@OptionalPct", optional);
                cmd.Parameters.AddWithValue("@PositionFormat", string.IsNullOrWhiteSpace(request.PositionFormat) ? "Point" : request.PositionFormat);
                cmd.Parameters.AddWithValue("@HideSub", request.HideSubExam);
                cmd.Parameters.AddWithValue("@HideSec", request.HideSecPosition);
                cmd.Parameters.AddWithValue("@HideClass", request.HideClassPosition);
                cmd.Parameters.AddWithValue("@FromDate", (object?)request.AttendanceFrom?.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object?)request.AttendanceTo?.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
                cmd.Parameters.AddWithValue("@GradeNameID", gradeId);
                cmd.Parameters.AddWithValue("@GradeOnGpa", request.GradeOnGpa);
                settingId = ToInt(await cmd.ExecuteScalarAsync(ct));
            }

            foreach (var exam in request.Exams.Where(x => x.Selected))
            {
                await using var cmd = new SqlCommand("""
INSERT INTO Exam_Cumulative_ExamList
    (SchoolID, RegistrationID, CumulativeNameID, EducationYearID, ExamID, ClassID, ExamAdd_Percentage, Exam_EnableFail, Cumulative_SettingID, Publish_SettingID)
SELECT @SchoolID, @RegistrationID, @CumulativeNameID, @EducationYearID, @ExamID, @ClassID, @ExamAdd_Percentage, @Exam_EnableFail, @Cumulative_SettingID,
       ISNULL((SELECT TOP 1 Publish_SettingID FROM Exam_Publish_Setting
               WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID), 0)
""", con);
                cmd.CommandTimeout = 0;
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                cmd.Parameters.AddWithValue("@ExamID", exam.ExamID);
                cmd.Parameters.AddWithValue("@ExamAdd_Percentage", exam.Percentage);
                cmd.Parameters.AddWithValue("@Exam_EnableFail", exam.EnableFail);
                cmd.Parameters.AddWithValue("@Cumulative_SettingID", settingId);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            var subjects = request.Subjects;
            await using (var cmd = new SqlCommand("""
INSERT INTO Exam_Cumulative_FullMarks
    (CumulativeNameID, SchoolID, RegistrationID, SubjectID, ClassID, EducationYearID, FullMarks, Cumulative_SettingID)
SELECT @CumulativeNameID, @SchoolID, @RegistrationID, v.SubjectID, @ClassID, @EducationYearID, v.FullMarks, @Cumulative_SettingID
FROM (VALUES {0}) AS v(SubjectID, FullMarks)
""", con))
            {
                var marks = subjects
                    .Select(row => (row.SubjectID, Mark: request.SameCountable ? request.CountableMark : row.CountableMark))
                    .Where(x => x.Mark >= 1)
                    .ToList();
                if (marks.Count > 0)
                {
                    cmd.CommandTimeout = 0;
                    AddSession(cmd, session);
                    cmd.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                    cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                    cmd.Parameters.AddWithValue("@Cumulative_SettingID", settingId);
                    var values = new List<string>(marks.Count);
                    for (var i = 0; i < marks.Count; i++)
                    {
                        values.Add($"(@SubjectID{i}, @FullMarks{i})");
                        cmd.Parameters.AddWithValue($"@SubjectID{i}", marks[i].SubjectID);
                        cmd.Parameters.AddWithValue($"@FullMarks{i}", marks[i].Mark);
                    }
                    cmd.CommandText = string.Format(cmd.CommandText, string.Join(",", values));
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }

            await using (var cmd = new SqlCommand("EXEC [dbo].[SP_Cumulative_Exam_Subject] @SchoolID, @RegistrationID, @EducationYearID, @ClassID, @CumulativeNameID, @Cumulative_SettingID", con))
            {
                cmd.CommandTimeout = 0;
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                cmd.Parameters.AddWithValue("@Cumulative_SettingID", settingId);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            if (request.SameCountable || subjects.Count == 0)
            {
                await ExecSqlAsync(con, """
UPDATE Exam_Cumulative_Subject SET IS_Add_InExam = 1
WHERE CumulativeNameID = @CumulativeNameID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
""", session, ct, c =>
                {
                    c.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                    c.Parameters.AddWithValue("@ClassID", request.ClassID);
                });
            }
            else
            {
                var addIds = subjects.Where(x => x.AddInExam).Select(x => x.SubjectID).Distinct().ToList();
                if (addIds.Count == 0)
                {
                    await ExecSqlAsync(con, """
UPDATE Exam_Cumulative_Subject SET IS_Add_InExam = 0
WHERE CumulativeNameID = @CumulativeNameID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
""", session, ct, c =>
                    {
                        c.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                        c.Parameters.AddWithValue("@ClassID", request.ClassID);
                    });
                }
                else
                {
                    var names = addIds.Select((_, i) => "@Add" + i).ToArray();
                    await ExecSqlAsync(con, $"""
UPDATE Exam_Cumulative_Subject SET IS_Add_InExam = CASE WHEN SubjectID IN ({string.Join(",", names)}) THEN 1 ELSE 0 END
WHERE CumulativeNameID = @CumulativeNameID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
""", session, ct, c =>
                    {
                        for (var i = 0; i < addIds.Count; i++)
                            c.Parameters.AddWithValue(names[i], addIds[i]);
                        c.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                        c.Parameters.AddWithValue("@ClassID", request.ClassID);
                    });
                }
            }

            var format = string.IsNullOrWhiteSpace(request.PositionFormat) ? "Point" : request.PositionFormat;
            await using (var cmd = new SqlCommand("""
EXEC [dbo].[SP_Cumulative_Exam_Student] @SchoolID, @RegistrationID, @EducationYearID, @ClassID, @CumulativeNameID, @Cumulative_SettingID
EXEC [dbo].[SP_Cumulative_HighestMark_Position] @SchoolID, @EducationYearID, @ClassID, @CumulativeNameID, @Exam_Position_Format
""", con))
            {
                cmd.CommandTimeout = 0;
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                cmd.Parameters.AddWithValue("@Cumulative_SettingID", settingId);
                cmd.Parameters.AddWithValue("@Exam_Position_Format", format);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            if (request.AttendanceFrom is not null && request.AttendanceTo is not null)
            {
                await using var cmd = new SqlCommand("""
DELETE FROM Attendance_Student
WHERE SchoolID = @SchoolID AND ClassID = @ClassID AND EducationYearID = @EducationYearID AND CumulativeNameID = @CumulativeNameID;

INSERT INTO Attendance_Student
    (SchoolID, RegistrationID, EducationYearID, CumulativeNameID, ClassID, StudentID, StudentClassID,
     WorkingDays, TotalPresent, TotalAbsent, TotalLate, TotalLeave, TotalBunk, TotalLateAbs)
SELECT
    @SchoolID, @RegistrationID, @EducationYearID, @CumulativeNameID, @ClassID,
    StudentsClass.StudentID,
    Attendance_Record.StudentClassID,
    COUNT(*) AS WorkingDays,
    SUM(CASE WHEN Attendance_Record.Attendance = N'Pre' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Attendance_Record.Attendance = N'Abs' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Attendance_Record.Attendance = N'Late' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Attendance_Record.Attendance = N'Leave' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Attendance_Record.Attendance = N'Bunk' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Attendance_Record.Attendance = N'Late Abs' THEN 1 ELSE 0 END)
FROM Attendance_Record
INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID
WHERE Attendance_Record.SchoolID = @SchoolID
  AND Attendance_Record.ClassID = @ClassID
  AND Attendance_Record.EducationYearID = @EducationYearID
  AND Attendance_Record.AttendanceDate BETWEEN @From_Date AND @To_Date
  AND (@ScheduleID = 0 OR ISNULL(Attendance_Record.Attendance_ScheduleID, 0) = @ScheduleID)
GROUP BY Attendance_Record.StudentClassID, StudentsClass.StudentID;
""", con);
                cmd.CommandTimeout = 0;
                AddSession(cmd, session);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@CumulativeNameID", request.CumulativeNameID);
                cmd.Parameters.AddWithValue("@From_Date", request.AttendanceFrom.Value.Date);
                cmd.Parameters.AddWithValue("@To_Date", request.AttendanceTo.Value.Date);
                cmd.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            return Ok(settingId);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<ExamMeritListDto> GetCumulativeMeritAsync(SessionSnapshot session, int classId, int cumulativeNameId, string? groupId, string? sectionId, string? shiftId, CancellationToken ct)
    {
        var dto = new ExamMeritListDto();
        if (classId <= 0 || cumulativeNameId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        dto.Title = await ScalarTextAsync(con, """
SELECT CreateClass.Class + ' - ' + Exam_Cumulative_Name.CumulativeResultName
FROM CreateClass, Exam_Cumulative_Name
WHERE CreateClass.ClassID = @ClassID AND Exam_Cumulative_Name.CumulativeNameID = @CumulativeNameID
""", session, ct, c =>
        {
            c.Parameters.AddWithValue("@ClassID", classId);
            c.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
        });

        var rows = new Dictionary<int, ExamMeritRowDto>();
        await using (var cmd = new SqlCommand("""
SELECT StudentsClass.StudentClassID, Student.StudentID, Student.ID, StudentsClass.RollNo, Student.StudentsName,
       Exam_Cumulative_Student.Cumulative_StudentID, Exam_Cumulative_Student.ObtainedMark_ofStudent,
       Exam_Cumulative_Student.Student_Grade, Exam_Cumulative_Student.Student_Point,
       TRY_CAST(Exam_Cumulative_Student.Position_InExam_Class AS int) AS Position_InExam_Class,
       TRY_CAST(Exam_Cumulative_Student.Position_InExam_Subsection AS int) AS Position_InExam_Subsection,
       Student.SMSPhoneNo, Exam_Cumulative_Student.PassStatus_InSubject, Exam_Cumulative_Student.Average,
       Exam_Cumulative_Student.Cumulative_SettingID
FROM StudentsClass
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN Exam_Cumulative_Student ON StudentsClass.StudentClassID = Exam_Cumulative_Student.StudentClassID
INNER JOIN Exam_Cumulative_Setting ON Exam_Cumulative_Student.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID
WHERE StudentsClass.ClassID = @ClassID AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @SubjectGroupID AND StudentsClass.ShiftID LIKE @ShiftID
  AND Student.Status = N'Active' AND StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.SchoolID = @SchoolID
  AND Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID AND Exam_Cumulative_Setting.IS_Published = 1
ORDER BY Position_InExam_Class, CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS FLOAT) ELSE 0 END
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@SubjectGroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentClassID"]);
                var row = new ExamMeritRowDto
                {
                    StudentResultID = ToInt(reader["Cumulative_StudentID"]),
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
SELECT Exam_Cumulative_Subject.StudentClassID, Subject.SubjectName, Exam_Cumulative_Subject.PassStatus_Subject,
       Exam_Cumulative_Subject.SubjectType, Exam_Cumulative_Subject.ObtainedMark_ofSubject AS Mark
FROM Exam_Cumulative_Subject INNER JOIN Subject ON Exam_Cumulative_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Cumulative_Subject.SchoolID = @SchoolID AND Exam_Cumulative_Subject.EducationYearID = @EducationYearID
  AND Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID AND Exam_Cumulative_Subject.ClassID = @ClassID
ORDER BY ISNULL(Subject.SN, 999), Subject.SubjectID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["StudentClassID"]);
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

    public async Task<CumulativeResultCardSheetDto> GetCumulativeResultCardsAsync(
        SessionSnapshot session, int classId, int cumulativeNameId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken ct)
    {
        var dto = new CumulativeResultCardSheetDto();
        if (classId <= 0 || cumulativeNameId <= 0) return dto;
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

        var settingId = 0;
        DateTime? attFrom = null, attTo = null;
        var attSchedule = 0;
        var gradeNameId = 0;
        await using (var cmd = new SqlCommand("""
SELECT TOP 1 Cumulative_SettingID, ISNULL(IS_Hide_Sec_Position, 0) AS IS_Hide_Sec_Position,
       ISNULL(IS_Hide_Class_Position, 0) AS IS_Hide_Class_Position, Attendance_FromDate, Attendance_ToDate,
       ISNULL(Attendance_ScheduleID, 0) AS Attendance_ScheduleID, ISNULL(GradeNameID, 0) AS GradeNameID
FROM Exam_Cumulative_Setting
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND CumulativeNameID = @CumulativeNameID
ORDER BY Cumulative_SettingID DESC
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                settingId = ToInt(reader["Cumulative_SettingID"]);
                dto.HideSecPosition = Flag(reader["IS_Hide_Sec_Position"]);
                dto.HideClassPosition = Flag(reader["IS_Hide_Class_Position"]);
                attFrom = Day(reader["Attendance_FromDate"]);
                attTo = Day(reader["Attendance_ToDate"]);
                attSchedule = ToInt(reader["Attendance_ScheduleID"]);
                gradeNameId = ToInt(reader["GradeNameID"]);
            }
        }

        if (settingId <= 0) return dto;

        await using (var cmd = new SqlCommand("""
SELECT DISTINCT en.ExamName, cel.ExamID, cel.ExamAdd_Percentage, en.Period_StartDate
FROM Exam_Cumulative_ExamList cel
INNER JOIN Exam_Name en ON cel.ExamID = en.ExamID
WHERE cel.Cumulative_SettingID = @SettingID AND cel.CumulativeNameID = @CumulativeNameID
  AND cel.SchoolID = @SchoolID AND cel.EducationYearID = @EducationYearID AND cel.ClassID = @ClassID
ORDER BY en.Period_StartDate, cel.ExamID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@SettingID", settingId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Exams.Add(new CumulativeExamColDto
                {
                    ExamID = ToInt(reader["ExamID"]),
                    ExamName = Text(reader["ExamName"]),
                    Percentage = ToDbl(reader["ExamAdd_Percentage"]).ToString("0.##")
                });
            }
        }

        if (gradeNameId > 0)
        {
            await using var cmd = new SqlCommand("""
SELECT MaxPercentage, MinPercentage, Grades, Point, ISNULL(Comments, '') AS Comments
FROM Exam_Grading_System
WHERE SchoolID = @SchoolID AND GradeNameID = @GradeNameID
ORDER BY MaxPercentage DESC
""", con);
            AddSchool(cmd, session);
            cmd.Parameters.AddWithValue("@GradeNameID", gradeNameId);
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

        var ids = (studentIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0).Take(200).ToList();
        var sql = """
SELECT ecs.StudentClassID, ecs.StudentID, st.ID, st.StudentsName, sc.RollNo, cc.Class AS ClassName,
       ISNULL(cs.Section, '') AS SectionName, ISNULL(csh.Shift, '') AS ShiftName, ISNULL(csg.SubjectGroup, '') AS GroupName,
       n.CumulativeResultName AS ExamName, ecs.TotalMark_ofStudent, ecs.ObtainedMark_ofStudent, ecs.Student_Grade,
       ecs.Student_Point, ecs.Average, ecs.ObtainedPercentage_ofStudent, ecs.Position_InExam_Class,
       ecs.Position_InExam_Subsection, ecs.PassStatus_InSubject, ecs.HighestMark_InExam_Class, ecs.HighestMark_InExam_Subsection,
       ISNULL(TRY_CAST(REPLACE(REPLACE(sc.RollNo, '$', ''), ',', '') AS INT), 999999) AS RollNoSortNumber
FROM Exam_Cumulative_Student ecs
INNER JOIN StudentsClass sc ON ecs.StudentClassID = sc.StudentClassID
INNER JOIN Student st ON sc.StudentID = st.StudentID
INNER JOIN CreateClass cc ON ecs.ClassID = cc.ClassID
INNER JOIN Exam_Cumulative_Name n ON ecs.CumulativeNameID = n.CumulativeNameID
LEFT JOIN CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN CreateShift csh ON sc.ShiftID = csh.ShiftID
LEFT JOIN CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
WHERE ecs.SchoolID = @SchoolID AND ecs.EducationYearID = @EducationYearID AND ecs.ClassID = @ClassID
  AND ecs.CumulativeNameID = @CumulativeNameID AND ecs.Cumulative_SettingID = @SettingID
  AND sc.SectionID LIKE @SectionID AND sc.SubjectGroupID LIKE @SubjectGroupID AND sc.ShiftID LIKE @ShiftID
  AND st.Status = N'Active'
""";
        if (ids.Count > 0) sql += " AND st.ID IN (" + string.Join(",", ids.Select((_, i) => "@Sid" + i)) + ")";
        sql += " ORDER BY RollNoSortNumber, sc.RollNo";

        var students = new Dictionary<int, CumulativeCardStudentDto>();
        await using (var cmd = new SqlCommand(sql, con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            cmd.Parameters.AddWithValue("@SettingID", settingId);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@SubjectGroupID", Like(groupId));
            cmd.Parameters.AddWithValue("@ShiftID", Like(shiftId));
            for (var i = 0; i < ids.Count; i++) cmd.Parameters.AddWithValue("@Sid" + i, ids[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var scid = ToInt(reader["StudentClassID"]);
                var student = new CumulativeCardStudentDto
                {
                    StudentClassID = scid,
                    StudentID = ToInt(reader["StudentID"]),
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
                    PositionClass = PosText(reader["Position_InExam_Class"]),
                    PositionSection = PosText(reader["Position_InExam_Subsection"]),
                    PassStatus = Text(reader["PassStatus_InSubject"]),
                    HighestClass = MarkText(reader["HighestMark_InExam_Class"]),
                    HighestSection = MarkText(reader["HighestMark_InExam_Subsection"]),
                    Comment = GradeComment(Text(reader["Student_Grade"]), dto.Grades)
                };
                students[scid] = student;
                dto.Students.Add(student);
                if (!string.IsNullOrWhiteSpace(student.SectionName)) dto.HasSections = true;
            }
        }

        if (students.Count == 0) return dto;

        try
        {
            await FillCardAttendanceAsync(con, session, classId, attFrom, attTo, attSchedule, dto.Students.Cast<ExamCardStudentDto>().ToList(), ct);
        }
        catch
        {
        }

        try
        {
            await using var cmd = new SqlCommand("""
SELECT WorkingDays, TotalPresent, TotalAbsent, TotalLate, TotalLeave, TotalLateAbs, StudentClassID
FROM Attendance_Student
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND CumulativeNameID = @CumulativeNameID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (!students.TryGetValue(ToInt(reader["StudentClassID"]), out var student)) continue;
                student.WorkingDays = Text(reader["WorkingDays"]);
                student.PresentDays = Text(reader["TotalPresent"]);
                student.AbsentDays = Text(reader["TotalAbsent"]);
                student.LateDays = Text(reader["TotalLate"]);
                student.LeaveDays = Text(reader["TotalLeave"]);
                student.LateAbsDays = Text(reader["TotalLateAbs"]);
            }
        }
        catch
        {
        }

        var fullMarks = new Dictionary<int, string>();
        await using (var cmd = new SqlCommand("""
SELECT SubjectID, FullMarks FROM Exam_Cumulative_FullMarks
WHERE CumulativeNameID = @CumulativeNameID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                fullMarks[ToInt(reader["SubjectID"])] = MarkText(reader["FullMarks"]);
        }

        var subjects = new Dictionary<(int Scid, int SubjectId), CumulativeCardSubjectDto>();
        await using (var cmd = new SqlCommand("""
SELECT Exam_Cumulative_Subject.StudentClassID, Subject.SubjectName, Subject.SN, Exam_Cumulative_Subject.SubjectID,
       Exam_Cumulative_Subject.TotalMark_ofSubject, Exam_Cumulative_Subject.ObtainedMark_ofSubject,
       Exam_Cumulative_Subject.SubjectGrades, Exam_Cumulative_Subject.SubjectPoint,
       Exam_Cumulative_Subject.Position_InSubject_Class, Exam_Cumulative_Subject.Position_InSubject_Subsection,
       Exam_Cumulative_Subject.HighestMark_InSubject_Class, Exam_Cumulative_Subject.HighestMark_InSubject_Subsection,
       ISNULL(Exam_Cumulative_Subject.SubjectType, 'Compulsory') AS SubjectType
FROM Exam_Cumulative_Subject
INNER JOIN Subject ON Exam_Cumulative_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Cumulative_Subject.SchoolID = @SchoolID AND Exam_Cumulative_Subject.EducationYearID = @EducationYearID
  AND Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID AND Exam_Cumulative_Subject.ClassID = @ClassID
  AND Exam_Cumulative_Subject.Cumulative_SettingID = @SettingID AND Exam_Cumulative_Subject.IS_Add_InExam = 1
ORDER BY ISNULL(Subject.SN, 9999), Subject.SubjectName
""", con))
        {
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@CumulativeNameID", cumulativeNameId);
            cmd.Parameters.AddWithValue("@SettingID", settingId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var scid = ToInt(reader["StudentClassID"]);
                if (!students.TryGetValue(scid, out var student)) continue;
                var subjectId = ToInt(reader["SubjectID"]);
                var name = Text(reader["SubjectName"]);
                if (string.Equals(Text(reader["SubjectType"]), "Optional", StringComparison.OrdinalIgnoreCase))
                    name += " *";
                var subject = new CumulativeCardSubjectDto
                {
                    SubjectID = subjectId,
                    SubjectName = name,
                    CuFull = fullMarks.TryGetValue(subjectId, out var fm) ? fm : MarkText(reader["TotalMark_ofSubject"]),
                    CuObtained = MarkText(reader["ObtainedMark_ofSubject"]),
                    Grade = Text(reader["SubjectGrades"]),
                    Point = PointText(reader["SubjectPoint"]),
                    PositionClass = PosText(reader["Position_InSubject_Class"]),
                    PositionSection = PosText(reader["Position_InSubject_Subsection"]),
                    HighestClass = MarkText(reader["HighestMark_InSubject_Class"]),
                    HighestSection = MarkText(reader["HighestMark_InSubject_Subsection"])
                };
                foreach (var exam in dto.Exams)
                    subject.Exams.Add(new CumulativeCardExamMarkDto { ExamID = exam.ExamID });
                student.CuSubjects.Add(subject);
                subjects[(scid, subjectId)] = subject;
            }
        }

        if (dto.Exams.Count > 0 && subjects.Count > 0)
        {
            await using var cmd = new SqlCommand("""
SELECT erstu.StudentClassID, ers.SubjectID, erstu.ExamID, ers.TotalMark_ofSubject, ers.ObtainedMark_ofSubject, ers.SubjectAbsenceStatus
FROM Exam_Result_of_Subject ers
INNER JOIN Exam_Result_of_Student erstu ON ers.StudentResultID = erstu.StudentResultID
WHERE erstu.SchoolID = @SchoolID AND erstu.EducationYearID = @EducationYearID AND erstu.ClassID = @ClassID
""", con);
            AddSession(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (!subjects.TryGetValue((ToInt(reader["StudentClassID"]), ToInt(reader["SubjectID"])), out var subject)) continue;
                var examId = ToInt(reader["ExamID"]);
                var cell = subject.Exams.FirstOrDefault(x => x.ExamID == examId);
                if (cell is null) continue;
                var abs = Text(reader["SubjectAbsenceStatus"]);
                var fm = ToDbl(reader["TotalMark_ofSubject"]);
                cell.FullMark = MarkText(reader["TotalMark_ofSubject"]);
                cell.PassMark = fm > 0 ? MarkText(fm * 0.33) : "-";
                cell.Obtained = abs is "Absent" or "A" ? "Abs" : MarkText(reader["ObtainedMark_ofSubject"]);
            }
        }

        try
        {
            await FillCardPhotosAsync(con, session, dto.Students.Cast<ExamCardStudentDto>().ToList(), ct);
        }
        catch
        {
        }

        return dto;
    }

    private static Task ExecTxAsync(SqlConnection con, SqlTransaction tx, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null) =>
        ExecSqlAsync(con, sql, session, ct, extra, tx);

    private static async Task ExecSqlAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null, SqlTransaction? tx = null)
    {
        await using var cmd = tx is null ? new SqlCommand(sql, con) : new SqlCommand(sql, con, tx);
        cmd.CommandTimeout = 0;
        AddSession(cmd, session);
        extra?.Invoke(cmd);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static string MarkText(object? value)
    {
        if (value is null or DBNull) return "-";
        var n = Convert.ToDouble(value);
        return n % 1 == 0 ? n.ToString("0") : n.ToString("0.##");
    }

    private static string PointText(object? value)
    {
        if (value is null or DBNull) return "-";
        return ToDbl(value).ToString("0.00");
    }

    private static string PosText(object? value)
    {
        if (value is null or DBNull) return "-";
        var text = Convert.ToString(value)?.Trim() ?? "";
        if (text.Length == 0 || text == "0") return "-";
        return text;
    }
}
