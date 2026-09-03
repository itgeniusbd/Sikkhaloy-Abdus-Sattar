using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;
using Sikkhaloy.Shared.Exam;

namespace Sikkhaloy.LocalData.Sync;

internal sealed partial class OfflineApiStore
{
    public async Task<ExamFilterDto> ExamFiltersFromLocalAsync(
        string? kind, int classId, int examId, string? groupId, string? sectionId, string? shiftId, int subjectId,
        CancellationToken cancellationToken)
    {
        var dto = await ReadAsync<ExamFilterDto>(
            FilterKey(kind, 0, 0, 0), cancellationToken) ?? new ExamFilterDto();

        if (classId > 0)
        {
            var classFilter = await ReadAsync<ExamFilterDto>(
                FilterKey(kind, classId, 0, 0), cancellationToken);
            MergeFilter(dto, classFilter);
            if (examId > 0)
            {
                var examFilter = await ReadAsync<ExamFilterDto>(
                    FilterKey(kind, classId, examId, subjectId), cancellationToken);
                MergeFilter(dto, examFilter);
            }
        }

        if (dto.Exams.Count == 0)
        {
            var names = await ReadAsync<List<ExamNameDto>>("api/sync/exam/names", cancellationToken) ?? [];
            dto.Exams = names.Select(x => new ExamOptionDto { Id = x.ExamID, Name = x.ExamName }).ToList();
        }

        if (dto.SubExams.Count == 0)
        {
            var subs = await ReadAsync<List<SubExamDto>>("api/sync/exam/sub-exams", cancellationToken) ?? [];
            dto.SubExams = subs.Select(x => new ExamOptionDto { Id = x.SubExamID, Name = x.SubExamName }).ToList();
        }

        if (dto.Grades.Count == 0)
        {
            var grades = await ReadAsync<List<GradeSystemDto>>("api/sync/exam/grading", cancellationToken) ?? [];
            dto.Grades = grades.Select(x => new ExamOptionDto { Id = x.GradeNameID, Name = x.GradeName }).ToList();
        }

        if (dto.Classes.Count == 0)
        {
            var structure = await ReadClassStructureAsync(cancellationToken);
            dto.Classes = structure.Classes
                .OrderBy(x => x.SortOrder)
                .Select(x => new ExamOptionDto { Id = x.ClassID, Name = x.Name })
                .ToList();
        }

        if (classId > 0)
        {
            var structure = await ReadClassStructureAsync(cancellationToken);
            var parts = structure.AssignedParts(classId);
            if (dto.Groups.Count == 0)
                dto.Groups = parts.Groups.Select(x => new ExamOptionDto { Id = x.ServerId, Name = x.Name }).ToList();
            if (dto.Sections.Count == 0)
                dto.Sections = parts.Sections.Select(x => new ExamOptionDto { Id = x.ServerId, Name = x.Name }).ToList();
            if (dto.Shifts.Count == 0)
                dto.Shifts = parts.Shifts.Select(x => new ExamOptionDto { Id = x.ServerId, Name = x.Name }).ToList();

            if (dto.Subjects.Count == 0 && examId > 0)
            {
                var dist = await ReadAsync<DistSheetDto>(
                    $"api/sync/exam/distribution?classId={classId}&examId={examId}", cancellationToken);
                if (dist is not null)
                    dto.Subjects = dist.Subjects.Select(x => new ExamOptionDto { Id = x.SubjectID, Name = x.SubjectName }).ToList();
            }
        }

        _ = groupId;
        _ = sectionId;
        _ = shiftId;
        return dto;
    }

    public async Task<InputSheetDto> InputSheetFromLocalAsync(
        int examId, int classId, int subjectId, int subExamId, string? groupId, string? sectionId, string? shiftId,
        CancellationToken cancellationToken)
    {
        var cached = await ReadAsync<InputSheetDto>(
            $"api/sync/exam/input?examId={examId}&classId={classId}&subjectId={subjectId}&subExamId={subExamId}",
            cancellationToken);
        var students = await LocalExamStudentsAsync(classId, groupId, sectionId, shiftId, cancellationToken);
        var dist = await ReadAsync<DistSheetDto>(
            $"api/sync/exam/distribution?classId={classId}&examId={examId}", cancellationToken);
        var subject = dist?.Subjects.FirstOrDefault(x => x.SubjectID == subjectId);
        var subs = await ReadAsync<List<SubExamDto>>("api/sync/exam/sub-exams", cancellationToken) ?? [];
        var headers = BuildInputHeaders(subject, subs, subExamId);
        var sheet = cached ?? new InputSheetDto();
        sheet.HasSubExams = headers.Count > 0 && subExamId <= 0 && (subject?.UseSubExam ?? headers.Count > 1);
        sheet.SubHeaders = headers;
        sheet.FullMark = subject?.FullMarks ?? headers.Sum(x => x.FullMark);
        sheet.PassMark = sheet.FullMark > 0 ? Math.Round(sheet.FullMark * 0.33, 2) : 0;
        if (sheet.Students.Count == 0)
        {
            sheet.Students = students.Select(x => new InputStudentDto
            {
                StudentID = x.ServerId ?? 0,
                StudentClassID = x.StudentClassServerId ?? 0,
                ID = x.StudentCode,
                Name = x.StudentsName,
                RollNo = x.RollNo ?? "",
                Subs = headers.Select(h => new InputSubMarkDto
                {
                    SubExamID = h.SubExamID,
                    SubExamName = h.SubExamName,
                    FullMark = h.FullMark,
                    PassMark = h.PassMark
                }).ToList()
            }).ToList();
        }

        return sheet;
    }

    public async Task<CollectPaperDto> CollectPaperFromLocalAsync(
        int examId, int classId, int subjectId, string? groupId, string? sectionId, string? shiftId,
        CancellationToken cancellationToken)
    {
        var students = await LocalExamStudentsAsync(classId, groupId, sectionId, shiftId, cancellationToken);
        var names = await ReadAsync<List<ExamNameDto>>("api/sync/exam/names", cancellationToken) ?? [];
        var dist = await ReadAsync<DistSheetDto>(
            $"api/sync/exam/distribution?classId={classId}&examId={examId}", cancellationToken);
        var subject = dist?.Subjects.FirstOrDefault(x => x.SubjectID == subjectId);
        var columns = subject?.UseSubExam == true
            ? subject.SubExams.Where(x => x.Selected).Select(x => x.SubExamName).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
            : [];
        if (columns.Count == 0)
        {
            var subs = await ReadAsync<List<SubExamDto>>("api/sync/exam/sub-exams", cancellationToken) ?? [];
            columns = subs.Select(x => x.SubExamName).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
        if (columns.Count == 0)
            columns = ["Marks"];

        var structure = await ReadClassStructureAsync(cancellationToken);
        return new CollectPaperDto
        {
            HasMarks = true,
            ExamName = names.FirstOrDefault(x => x.ExamID == examId)?.ExamName ?? "",
            ClassName = structure.Classes.FirstOrDefault(x => x.ClassID == classId)?.Name ?? "",
            SubjectName = subject?.SubjectName ?? "",
            Columns = columns,
            Students = students.Select(x => new CollectStudentDto
            {
                ID = x.StudentCode,
                Name = x.StudentsName,
                FathersName = x.FathersName ?? "",
                RollNo = x.RollNo ?? ""
            }).ToList()
        };
    }

    public async Task<ExamAdmitCardSheetDto> AdmitCardsFromLocalAsync(
        int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds,
        CancellationToken cancellationToken)
    {
        var print = await PrintStudentsFromLocalAsync(classId, examId, groupId, sectionId, shiftId, studentIds, null, cancellationToken);
        var session = await CurrentSessionAsync(cancellationToken);
        return new ExamAdmitCardSheetDto
        {
            SchoolName = session?.SchoolName ?? "",
            ExamName = print.ExamName,
            EducationYear = print.Year,
            Students = print.Students
        };
    }

    public async Task<ExamSeatPlanSheetDto> SeatPlanFromLocalAsync(
        int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? classIds,
        CancellationToken cancellationToken)
    {
        var print = await PrintStudentsFromLocalAsync(classId, examId, groupId, sectionId, shiftId, studentIds, classIds, cancellationToken);
        var session = await CurrentSessionAsync(cancellationToken);
        return new ExamSeatPlanSheetDto
        {
            SchoolName = session?.SchoolName ?? "",
            ExamName = print.ExamName,
            Students = print.Students
        };
    }

    public async Task<DistSheetDto> DistributionFromLocalAsync(int classId, int examId, CancellationToken cancellationToken)
    {
        var cached = await ReadAsync<DistSheetDto>(
            $"api/sync/exam/distribution?classId={classId}&examId={examId}", cancellationToken);
        if (cached is not null && cached.Subjects.Count > 0)
            return cached;

        var sheet = cached ?? new DistSheetDto();
        var grades = await ReadAsync<List<GradeSystemDto>>("api/sync/exam/grading", cancellationToken) ?? [];
        sheet.Grades = grades.Select(x => new ExamOptionDto { Id = x.GradeNameID, Name = x.GradeName }).ToList();
        if (sheet.GradeNameID <= 0)
            sheet.GradeNameID = sheet.Grades.FirstOrDefault()?.Id ?? 0;

        var filter = await ReadAsync<ExamFilterDto>(FilterKey("distribution", classId, examId, 0), cancellationToken)
            ?? await ReadAsync<ExamFilterDto>(FilterKey("input", classId, examId, 0), cancellationToken);
        var subs = await ReadAsync<List<SubExamDto>>("api/sync/exam/sub-exams", cancellationToken) ?? [];
        if (sheet.Subjects.Count == 0 && filter is not null)
        {
            sheet.Subjects = filter.Subjects.Select(s => new DistSubjectDto
            {
                SubjectID = s.Id,
                SubjectName = s.Name,
                SubExams = subs.Select(x => new DistSubMarkDto
                {
                    SubExamID = x.SubExamID,
                    SubExamName = x.SubExamName
                }).ToList()
            }).ToList();
        }

        return sheet;
    }

    internal async Task<int> ApplyExamWriteToCacheAsync(string url, string bodyJson, CancellationToken cancellationToken)
    {
        if (string.Equals(url, "api/sync/exam/names", StringComparison.OrdinalIgnoreCase))
            return await UpsertExamNameCacheAsync(0, bodyJson, cancellationToken);
        if (url.StartsWith("api/sync/exam/names/", StringComparison.OrdinalIgnoreCase))
        {
            var id = ParseExamPathId(url, "api/sync/exam/names/");
            if (url.EndsWith("/delete", StringComparison.OrdinalIgnoreCase))
            {
                await RemoveExamNameCacheAsync(id, cancellationToken);
                return id;
            }
            return await UpsertExamNameCacheAsync(id, bodyJson, cancellationToken);
        }

        if (string.Equals(url, "api/sync/exam/sub-exams", StringComparison.OrdinalIgnoreCase))
            return await UpsertSubExamCacheAsync(0, bodyJson, cancellationToken);
        if (url.StartsWith("api/sync/exam/sub-exams/", StringComparison.OrdinalIgnoreCase))
        {
            var id = ParseExamPathId(url, "api/sync/exam/sub-exams/");
            if (url.EndsWith("/delete", StringComparison.OrdinalIgnoreCase))
            {
                await RemoveSubExamCacheAsync(id, cancellationToken);
                return id;
            }
            return await UpsertSubExamCacheAsync(id, bodyJson, cancellationToken);
        }

        if (string.Equals(url, "api/sync/exam/input", StringComparison.OrdinalIgnoreCase))
            return await ApplyInputMarksCacheAsync(bodyJson, cancellationToken);
        if (string.Equals(url, "api/sync/exam/distribution", StringComparison.OrdinalIgnoreCase))
            return await ApplyDistributionCacheAsync(bodyJson, cancellationToken);

        return 0;
    }

    private async Task<(string ExamName, string Year, List<ExamPrintStudentDto> Students)> PrintStudentsFromLocalAsync(
        int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? classIds,
        CancellationToken cancellationToken)
    {
        var ids = ParseIdList(classIds);
        if (classId > 0 && ids.Count == 0)
            ids.Add(classId);
        List<LocalStudent> students;
        if (ids.Count == 0)
            students = await LocalStudentsAsync(null, cancellationToken);
        else
        {
            students = [];
            foreach (var id in ids.Distinct())
                students.AddRange(await LocalExamStudentsAsync(id, groupId, sectionId, shiftId, cancellationToken));
        }

        var wanted = ParseCodeList(studentIds);
        if (wanted.Count > 0)
        {
            students = students
                .Where(x => wanted.Contains(x.StudentCode) || (x.ServerId is int sid && wanted.Contains(sid.ToString())))
                .ToList();
        }

        var names = await ReadAsync<List<ExamNameDto>>("api/sync/exam/names", cancellationToken) ?? [];
        var session = await CurrentSessionAsync(cancellationToken);
        var examName = names.FirstOrDefault(x => x.ExamID == examId)?.ExamName ?? "";
        var year = session?.EducationYearID > 0 ? session.EducationYearID.ToString() : "";
        var rows = students
            .OrderBy(x => x.ClassName)
            .ThenBy(x => x.RollNo)
            .ThenBy(x => x.StudentsName)
            .Select((x, i) => new ExamPrintStudentDto
            {
                StudentClassID = x.StudentClassServerId ?? 0,
                ClassID = x.ClassID ?? 0,
                StudentID = x.ServerId ?? 0,
                ID = x.StudentCode,
                StudentsName = x.StudentsName,
                ClassName = x.ClassName ?? "",
                SectionName = x.SectionName ?? "",
                ShiftName = x.ShiftName ?? "",
                GroupName = x.GroupName ?? "",
                RollNo = x.RollNo ?? "",
                SeatNo = (i + 1).ToString(),
                EducationYear = year
            })
            .ToList();
        return (examName, year, rows);
    }

    private async Task<List<LocalStudent>> LocalExamStudentsAsync(
        int classId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken)
    {
        var rows = await LocalStudentsAsync(classId > 0 ? classId : null, cancellationToken);
        return rows
            .Where(x => MatchPart(x.SubjectGroupID, groupId))
            .Where(x => MatchPart(x.SectionID, sectionId))
            .Where(x => MatchPart(x.ShiftID, shiftId))
            .OrderBy(x => x.RollNo)
            .ThenBy(x => x.StudentsName)
            .ToList();
    }

    private async Task<int> UpsertExamNameCacheAsync(int examId, string bodyJson, CancellationToken cancellationToken)
    {
        SaveExamNameRequest? request;
        try { request = JsonSerializer.Deserialize<SaveExamNameRequest>(bodyJson, JsonOptions); }
        catch (JsonException) { return examId; }
        if (request is null) return examId;
        var rows = await ReadAsync<List<ExamNameDto>>("api/sync/exam/names", cancellationToken) ?? [];
        if (examId <= 0)
            examId = TempId();
        var row = rows.FirstOrDefault(x => x.ExamID == examId);
        if (row is null)
        {
            row = new ExamNameDto { ExamID = examId };
            rows.Add(row);
        }
        row.ExamName = request.ExamName;
        row.Period_StartDate = request.StartDate;
        row.Period_EndDate = request.EndDate;
        await SaveAsync("api/sync/exam/names", JsonSerializer.Serialize(rows, JsonOptions), cancellationToken);
        return examId;
    }

    private async Task RemoveExamNameCacheAsync(int examId, CancellationToken cancellationToken)
    {
        var rows = await ReadAsync<List<ExamNameDto>>("api/sync/exam/names", cancellationToken);
        if (rows is null) return;
        rows.RemoveAll(x => x.ExamID == examId);
        await SaveAsync("api/sync/exam/names", JsonSerializer.Serialize(rows, JsonOptions), cancellationToken);
    }

    private async Task<int> UpsertSubExamCacheAsync(int id, string bodyJson, CancellationToken cancellationToken)
    {
        SaveSubExamRequest? request;
        try { request = JsonSerializer.Deserialize<SaveSubExamRequest>(bodyJson, JsonOptions); }
        catch (JsonException) { return id; }
        if (request is null) return id;
        var rows = await ReadAsync<List<SubExamDto>>("api/sync/exam/sub-exams", cancellationToken) ?? [];
        if (id <= 0)
            id = TempId();
        var row = rows.FirstOrDefault(x => x.SubExamID == id);
        if (row is null)
        {
            row = new SubExamDto { SubExamID = id };
            rows.Add(row);
        }
        row.SubExamName = request.SubExamName;
        row.Sub_ExamSN = request.Serial;
        await SaveAsync("api/sync/exam/sub-exams", JsonSerializer.Serialize(rows, JsonOptions), cancellationToken);
        return id;
    }

    private async Task RemoveSubExamCacheAsync(int id, CancellationToken cancellationToken)
    {
        var rows = await ReadAsync<List<SubExamDto>>("api/sync/exam/sub-exams", cancellationToken);
        if (rows is null) return;
        rows.RemoveAll(x => x.SubExamID == id);
        await SaveAsync("api/sync/exam/sub-exams", JsonSerializer.Serialize(rows, JsonOptions), cancellationToken);
    }

    private async Task<int> ApplyInputMarksCacheAsync(string bodyJson, CancellationToken cancellationToken)
    {
        SaveInputMarksRequest? request;
        try { request = JsonSerializer.Deserialize<SaveInputMarksRequest>(bodyJson, JsonOptions); }
        catch (JsonException) { return 0; }
        if (request is null) return 0;
        var sheet = new InputSheetDto
        {
            HasSubExams = request.AllSubExams || request.Students.Any(x => x.Subs.Count > 0),
            Students = request.Students,
            SubHeaders = request.Students.FirstOrDefault()?.Subs
                .Select(x => new InputSubMarkDto
                {
                    SubExamID = x.SubExamID,
                    SubExamName = x.SubExamName,
                    FullMark = x.FullMark,
                    PassMark = x.PassMark
                }).ToList() ?? []
        };
        var key = $"api/sync/exam/input?examId={request.ExamID}&classId={request.ClassID}&subjectId={request.SubjectID}&subExamId={request.SubExamID}";
        await SaveAsync(key, JsonSerializer.Serialize(sheet, JsonOptions), cancellationToken);
        return request.ExamID;
    }

    private async Task<int> ApplyDistributionCacheAsync(string bodyJson, CancellationToken cancellationToken)
    {
        SaveDistributionRequest? request;
        try { request = JsonSerializer.Deserialize<SaveDistributionRequest>(bodyJson, JsonOptions); }
        catch (JsonException) { return 0; }
        if (request is null) return 0;
        var grades = await ReadAsync<List<GradeSystemDto>>("api/sync/exam/grading", cancellationToken) ?? [];
        var sheet = new DistSheetDto
        {
            GradeNameID = request.GradeNameID,
            Grades = grades.Select(x => new ExamOptionDto { Id = x.GradeNameID, Name = x.GradeName }).ToList(),
            Subjects = request.Subjects
        };
        await SaveAsync(
            $"api/sync/exam/distribution?classId={request.ClassID}&examId={request.ExamID}",
            JsonSerializer.Serialize(sheet, JsonOptions),
            cancellationToken);
        return request.ExamID;
    }

    private async Task<CachedSession?> CurrentSessionAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Sessions.AsNoTracking()
            .OrderByDescending(x => x.CachedUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static List<InputSubMarkDto> BuildInputHeaders(DistSubjectDto? subject, List<SubExamDto> subs, int subExamId)
    {
        if (subject?.UseSubExam == true)
        {
            var selected = subject.SubExams.Where(x => x.Selected && (subExamId <= 0 || x.SubExamID == subExamId)).ToList();
            if (selected.Count > 0)
            {
                return selected.Select(x => new InputSubMarkDto
                {
                    SubExamID = x.SubExamID,
                    SubExamName = x.SubExamName,
                    FullMark = x.FullMarks ?? 0,
                    PassMark = (x.FullMarks ?? 0) * 0.33
                }).ToList();
            }
        }

        if (subExamId > 0)
        {
            var one = subs.FirstOrDefault(x => x.SubExamID == subExamId);
            if (one is not null)
            {
                return
                [
                    new InputSubMarkDto
                    {
                        SubExamID = one.SubExamID,
                        SubExamName = one.SubExamName,
                        FullMark = subject?.FullMarks ?? 0
                    }
                ];
            }
        }

        return [];
    }

    private static void MergeFilter(ExamFilterDto target, ExamFilterDto? extra)
    {
        if (extra is null) return;
        if (extra.Classes.Count > 0) target.Classes = extra.Classes;
        if (extra.Exams.Count > 0) target.Exams = extra.Exams;
        if (extra.SubExams.Count > 0) target.SubExams = extra.SubExams;
        if (extra.Groups.Count > 0) target.Groups = extra.Groups;
        if (extra.Sections.Count > 0) target.Sections = extra.Sections;
        if (extra.Shifts.Count > 0) target.Shifts = extra.Shifts;
        if (extra.Subjects.Count > 0) target.Subjects = extra.Subjects;
        if (extra.Grades.Count > 0) target.Grades = extra.Grades;
        if (extra.CopyToExams.Count > 0) target.CopyToExams = extra.CopyToExams;
        if (extra.CumulativeExams.Count > 0) target.CumulativeExams = extra.CumulativeExams;
        if (extra.Schedules.Count > 0) target.Schedules = extra.Schedules;
    }

    private static string FilterKey(string? kind, int classId, int examId, int subjectId) =>
        $"api/sync/exam/filters?kind={Uri.EscapeDataString(kind ?? "")}&classId={classId}&examId={examId}&subjectId={subjectId}";

    private static bool MatchPart(int? value, string? filter)
    {
        if (!int.TryParse(filter, out var id) || id <= 0)
            return true;
        return value == id;
    }

    private static int ParseExamPathId(string url, string prefix)
    {
        var rest = url[prefix.Length..];
        var slash = rest.IndexOf('/');
        var token = slash < 0 ? rest : rest[..slash];
        return int.TryParse(token, out var id) ? id : 0;
    }

    private static List<int> ParseIdList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var id) ? id : 0)
            .Where(x => x > 0)
            .ToList();
    }

    private static HashSet<string> ParseCodeList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static int TempId()
    {
        var id = unchecked((int)(DateTime.UtcNow.Ticks % 1_000_000_000L));
        return id == 0 ? 1 : -Math.Abs(id);
    }
}
