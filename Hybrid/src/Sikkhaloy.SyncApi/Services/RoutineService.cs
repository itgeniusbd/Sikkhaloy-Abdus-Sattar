using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Routine;

namespace Sikkhaloy.SyncApi.Services;

public sealed class RoutineService
{
    private static readonly string[] WeekDays = ["Sat", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri"];

    private readonly EduConnectionFactory _connections;

    public RoutineService(EduConnectionFactory connections) => _connections = connections;

    public async Task<IReadOnlyList<RoutineNameDto>> GetNamesAsync(SessionSnapshot session, bool unusedOnly, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var sql = unusedOnly
            ? """
SELECT RoutineInfoID, RoutineSpecification
FROM dbo.RoutineInfo
WHERE SchoolID = @SchoolID
  AND RoutineInfoID NOT IN (SELECT RoutineInfoID FROM dbo.RoutineDay WHERE SchoolID = @SchoolID)
ORDER BY RoutineSpecification
"""
            : """
SELECT i.RoutineInfoID, i.RoutineSpecification,
       CASE WHEN EXISTS (SELECT 1 FROM dbo.RoutineDay d WHERE d.RoutineInfoID = i.RoutineInfoID AND d.SchoolID = @SchoolID)
            THEN 1 ELSE 0 END AS Used
FROM dbo.RoutineInfo i
WHERE i.SchoolID = @SchoolID
ORDER BY i.RoutineSpecification
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = new List<RoutineNameDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new RoutineNameDto
            {
                RoutineInfoID = Convert.ToInt32(reader["RoutineInfoID"]),
                Name = reader["RoutineSpecification"]?.ToString() ?? "",
                Used = unusedOnly || Convert.ToInt32(reader["Used"]) == 1
            });
        }
        return rows;
    }

    public async Task<RoutineResult> SaveNameAsync(SessionSnapshot session, SaveRoutineNameRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("rt.needName");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request!.RoutineInfoID > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.RoutineInfo SET RoutineSpecification = @Name
WHERE RoutineInfoID = @ID AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@Name", name);
            upd.Parameters.AddWithValue("@ID", request.RoutineInfoID);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(ct);
            return Ok("rt.nameUpdated");
        }
        await using var ins = new SqlCommand("""
INSERT INTO dbo.RoutineInfo (SchoolID, RegistrationID, RoutineSpecification, Date)
VALUES (@SchoolID, @RegID, @Name, GETDATE())
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@RegID", session.RegistrationID);
        ins.Parameters.AddWithValue("@Name", name);
        await ins.ExecuteNonQueryAsync(ct);
        return Ok("rt.nameAdded");
    }

    public async Task<RoutineResult> DeleteNameAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("rt.needRoutine");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        foreach (var sql in new[]
        {
            "DELETE FROM dbo.RoutineDay WHERE SchoolID = @SchoolID AND RoutineInfoID = @ID",
            "DELETE FROM dbo.RoutineForClass WHERE SchoolID = @SchoolID AND RoutineInfoID = @ID",
            "DELETE FROM dbo.RoutineTime WHERE SchoolID = @SchoolID AND RoutineInfoID = @ID",
            "DELETE FROM dbo.RoutineInfo WHERE SchoolID = @SchoolID AND RoutineInfoID = @ID"
        })
        {
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@ID", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        return Ok("rt.nameDeleted");
    }

    public async Task<RoutineResult> CreateAsync(SessionSnapshot session, CreateClassRoutineRequest? request, CancellationToken ct)
    {
        if (request is null || request.RoutineInfoID <= 0)
            return Fail("rt.needRoutine");
        var days = request.Days.Where(d => WeekDays.Contains(d)).Distinct().ToList();
        if (days.Count == 0)
            return Fail("rt.needDay");
        if (request.Periods.Count == 0)
            return Fail("rt.needPeriod");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            foreach (var day in days)
            {
                await using var dayCmd = new SqlCommand("""
INSERT INTO dbo.RoutineDay (RoutineInfoID, SchoolID, RegistrationID, Day)
VALUES (@RID, @SchoolID, @RegID, @Day)
""", con, tx);
                dayCmd.Parameters.AddWithValue("@RID", request.RoutineInfoID);
                dayCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                dayCmd.Parameters.AddWithValue("@RegID", session.RegistrationID);
                dayCmd.Parameters.AddWithValue("@Day", day);
                await dayCmd.ExecuteNonQueryAsync(ct);
            }
            foreach (var period in request.Periods)
            {
                await using var timeCmd = new SqlCommand("""
INSERT INTO dbo.RoutineTime (RoutineInfoID, SchoolID, RegistrationID, RoutinePeriod, StartTime, EndTime, Duration, Is_OffTime)
VALUES (@RID, @SchoolID, @RegID, @Period, @Start, @End, @Duration, @Off)
""", con, tx);
                timeCmd.Parameters.AddWithValue("@RID", request.RoutineInfoID);
                timeCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                timeCmd.Parameters.AddWithValue("@RegID", session.RegistrationID);
                timeCmd.Parameters.AddWithValue("@Period", period.Period);
                timeCmd.Parameters.AddWithValue("@Start", period.StartTime);
                timeCmd.Parameters.AddWithValue("@End", period.EndTime);
                timeCmd.Parameters.AddWithValue("@Duration", period.Duration);
                timeCmd.Parameters.AddWithValue("@Off", period.OffTime);
                await timeCmd.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);
            return Ok("rt.created");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return new RoutineResult { Error = ex.Message };
        }
    }

    public async Task<ClassRoutineSheetDto> GetAssignSheetAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, int routineInfoId, CancellationToken ct)
    {
        var sheet = new ClassRoutineSheetDto
        {
            Routines = (await GetNamesAsync(session, false, ct)).ToList()
        };
        if (classId <= 0 || routineInfoId <= 0)
            return sheet;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        sheet.Subjects = await LoadSubjectsAsync(con, session.SchoolID, classId, groupId, ct);
        sheet.Periods = await LoadPeriodsAsync(con, session.SchoolID, routineInfoId, true, ct);
        var likeGroup = Like(groupId);
        var likeSection = Like(sectionId);
        var likeShift = Like(shiftId);
        await using (var cmd = new SqlCommand("""
SELECT RoutineDayID, Day FROM dbo.RoutineDay
WHERE RoutineInfoID = @RID AND SchoolID = @SchoolID
  AND Day NOT IN (
      SELECT Day FROM dbo.RoutineForClass
      WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID = @ClassID
        AND SectionID LIKE @SectionID AND ShiftID LIKE @ShiftID
        AND SubjectGroupID LIKE @GroupID AND RoutineInfoID LIKE @RID)
""", con))
        {
            AddFilter(cmd, session, classId, likeGroup, likeSection, likeShift, routineInfoId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var day = new RoutineDayRowDto
                {
                    RoutineDayID = Convert.ToInt32(reader["RoutineDayID"]),
                    Day = reader["Day"]?.ToString() ?? ""
                };
                foreach (var period in sheet.Periods)
                {
                    day.Cells.Add(new RoutineCellDto
                    {
                        RoutineTimeID = period.RoutineTimeID,
                        StartTime = period.StartTime,
                        EndTime = period.EndTime
                    });
                }
                sheet.Days.Add(day);
            }
        }
        return sheet;
    }

    public async Task<IReadOnlyList<RoutineOptionDto>> GetTeachersAsync(
        SessionSnapshot session, int classId, int subjectId, string day, string start, string end, int exceptRoutineInfoId, CancellationToken ct)
    {
        if (classId <= 0 || subjectId <= 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await LoadTeachersAsync(con, session, classId, subjectId, day, start, end, exceptRoutineInfoId, 0, ct);
    }

    public async Task<RoutineResult> AssignAsync(SessionSnapshot session, AssignRoutineRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.RoutineInfoID <= 0)
            return Fail("rt.needClassRoutine");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var likeGroup = Like(request.GroupID);
        var likeSection = Like(request.SectionID);
        var likeShift = Like(request.ShiftID);
        await using (var check = new SqlCommand("""
SELECT TOP 1 RoutineForClassID FROM dbo.RoutineForClass
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID = @ClassID
  AND RoutineInfoID = @RID AND SubjectGroupID LIKE @GroupID
  AND ShiftID LIKE @ShiftID AND SectionID LIKE @SectionID
""", con))
        {
            AddFilter(check, session, request.ClassID, likeGroup, likeSection, likeShift, request.RoutineInfoID);
            var exists = await check.ExecuteScalarAsync(ct);
            if (exists is not null and not DBNull)
                return Fail("rt.already");
        }

        var conflict = await FindTeacherConflictAsync(con, session, request, false, ct);
        if (conflict is not null)
            return new RoutineResult { Error = conflict };

        var groupStore = Store(request.GroupID);
        var sectionStore = Store(request.SectionID);
        var shiftStore = Store(request.ShiftID);
        foreach (var cell in request.Cells)
        {
            await using var ins = new SqlCommand("""
INSERT INTO dbo.RoutineForClass
    (RoutineInfoID, RoutineTimeID, SubjectID, Day, SchoolID, RegistrationID, TeacherID,
     ClassID, SectionID, SubjectGroupID, EducationYearID, Date, ShiftID)
VALUES
    (@RID, @TID, @SID, @Day, @SchoolID, @RegID, @TeacherID,
     @ClassID, @SectionID, @GroupID, @YearID, GETDATE(), @ShiftID)
""", con);
            ins.Parameters.AddWithValue("@RID", request.RoutineInfoID);
            ins.Parameters.AddWithValue("@TID", cell.RoutineTimeID);
            ins.Parameters.AddWithValue("@SID", cell.SubjectID);
            ins.Parameters.AddWithValue("@Day", cell.Day);
            ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            ins.Parameters.AddWithValue("@RegID", session.RegistrationID);
            ins.Parameters.AddWithValue("@TeacherID", cell.TeacherID);
            ins.Parameters.AddWithValue("@ClassID", request.ClassID);
            ins.Parameters.AddWithValue("@SectionID", sectionStore);
            ins.Parameters.AddWithValue("@GroupID", groupStore);
            ins.Parameters.AddWithValue("@YearID", session.EducationYearID);
            ins.Parameters.AddWithValue("@ShiftID", shiftStore);
            await ins.ExecuteNonQueryAsync(ct);
        }
        return Ok("rt.assigned");
    }

    public async Task<ClassRoutineSheetDto> GetViewSheetAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, int routineInfoId, bool assignedOnly, CancellationToken ct)
    {
        var sheet = new ClassRoutineSheetDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        sheet.Routines = await LoadClassRoutinesAsync(con, session.SchoolID, classId, ct);
        if (classId <= 0 || routineInfoId <= 0)
            return sheet;

        sheet.Periods = await LoadPeriodsAsync(con, session.SchoolID, routineInfoId, false, ct);
        sheet.Subjects = await LoadSubjectsAsync(con, session.SchoolID, classId, groupId, ct);
        var likeGroup = assignedOnly && groupId == 0 ? "0" : Like(groupId);
        var likeSection = assignedOnly && sectionId == 0 ? "0" : Like(sectionId);
        var likeShift = assignedOnly && shiftId == 0 ? "0" : Like(shiftId);
        if (!assignedOnly)
        {
            likeGroup = Like(groupId);
            likeSection = Like(sectionId);
            likeShift = Like(shiftId);
        }

        await using (var cmd = new SqlCommand("""
SELECT RoutineDayID, Day FROM dbo.RoutineDay
WHERE RoutineInfoID = @RID AND SchoolID = @SchoolID
  AND Day IN (
      SELECT Day FROM dbo.RoutineForClass
      WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID = @ClassID
        AND SectionID LIKE @SectionID AND ShiftID LIKE @ShiftID
        AND SubjectGroupID LIKE @GroupID AND RoutineInfoID LIKE @RID)
""", con))
        {
            AddFilter(cmd, session, classId, likeGroup, likeSection, likeShift, routineInfoId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                sheet.Days.Add(new RoutineDayRowDto
                {
                    RoutineDayID = Convert.ToInt32(reader["RoutineDayID"]),
                    Day = reader["Day"]?.ToString() ?? ""
                });
            }
        }

        foreach (var day in sheet.Days)
        {
            day.Cells = await LoadDayCellsAsync(con, session, classId, likeGroup, likeSection, likeShift, routineInfoId, day.Day, assignedOnly, ct);
            if (assignedOnly)
            {
                foreach (var cell in day.Cells.Where(x => x.SubjectID > 0))
                {
                    cell.Teachers = await LoadTeachersAsync(con, session, classId, cell.SubjectID, day.Day, cell.StartTime, cell.EndTime, routineInfoId, cell.TeacherID, ct);
                }
            }
        }
        return sheet;
    }

    public async Task<RoutineResult> UpdateAsync(SessionSnapshot session, AssignRoutineRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.RoutineInfoID <= 0)
            return Fail("rt.needClassRoutine");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var conflict = await FindTeacherConflictAsync(con, session, request, true, ct);
        if (conflict is not null)
            return new RoutineResult { Error = conflict };

        foreach (var cell in request.Cells)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.RoutineForClass
SET SubjectID = @SID, TeacherID = @TeacherID
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND RoutineInfoID = @RID
  AND RoutineTimeID = @TID AND ClassID = @ClassID AND SectionID = @SectionID
  AND ShiftID = @ShiftID AND SubjectGroupID = @GroupID AND Day = @Day
""", con);
            upd.Parameters.AddWithValue("@SID", cell.SubjectID);
            upd.Parameters.AddWithValue("@TeacherID", cell.TeacherID);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            upd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            upd.Parameters.AddWithValue("@RID", request.RoutineInfoID);
            upd.Parameters.AddWithValue("@TID", cell.RoutineTimeID);
            upd.Parameters.AddWithValue("@ClassID", request.ClassID);
            upd.Parameters.AddWithValue("@SectionID", Store(request.SectionID));
            upd.Parameters.AddWithValue("@ShiftID", Store(request.ShiftID));
            upd.Parameters.AddWithValue("@GroupID", Store(request.GroupID));
            upd.Parameters.AddWithValue("@Day", cell.Day);
            await upd.ExecuteNonQueryAsync(ct);
        }
        return Ok("rt.updated");
    }

    public async Task<RoutineResult> DeleteClassRoutineAsync(SessionSnapshot session, AssignRoutineRequest? request, CancellationToken ct)
    {
        if (request is null || request.ClassID <= 0 || request.RoutineInfoID <= 0)
            return Fail("rt.needClassRoutine");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.RoutineForClass
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID = @ClassID
  AND SectionID LIKE @SectionID AND ShiftID LIKE @ShiftID
  AND SubjectGroupID LIKE @GroupID AND RoutineInfoID = @RID
""", con);
        AddFilter(cmd, session, request.ClassID, Like(request.GroupID), Like(request.SectionID), Like(request.ShiftID), request.RoutineInfoID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("rt.deleted");
    }

    public async Task<ExamRoutineSheetDto> GetExamAsync(SessionSnapshot session, int routineId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new ExamRoutineSheetDto
        {
            Saved = await LoadExamListAsync(con, session.SchoolID, ct),
            Classes = await LoadClassesAsync(con, session.SchoolID, ct)
        };
        if (routineId <= 0)
        {
            dto.Columns.Add(new ExamRoutineColumnDto { ColumnIndex = 0 });
            dto.Rows.Add(new ExamRoutineRowDto { Duration = "3 hours" });
            return dto;
        }

        await using (var cmd = new SqlCommand("""
SELECT RoutineID, RoutineName, ClassColumnCount, [RowCount]
FROM dbo.Exam_Routine_SavedData
WHERE RoutineID = @ID AND SchoolID = @SchoolID AND ISNULL(IsActive, 1) = 1
""", con))
        {
            cmd.Parameters.AddWithValue("@ID", routineId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return dto;
            dto.RoutineID = Convert.ToInt32(reader["RoutineID"]);
            dto.RoutineName = reader["RoutineName"]?.ToString() ?? "";
        }

        await using (var col = new SqlCommand("""
SELECT ColumnIndex, ClassID FROM dbo.Exam_Routine_ClassColumns WHERE RoutineID = @ID ORDER BY ColumnIndex
""", con))
        {
            col.Parameters.AddWithValue("@ID", routineId);
            await using var reader = await col.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var classId = Convert.ToInt32(reader["ClassID"]);
                dto.Columns.Add(new ExamRoutineColumnDto
                {
                    ColumnIndex = Convert.ToInt32(reader["ColumnIndex"]),
                    ClassID = classId,
                    ClassName = dto.Classes.FirstOrDefault(x => x.Id == classId)?.Name ?? ""
                });
            }
        }
        if (dto.Columns.Count == 0)
            dto.Columns.Add(new ExamRoutineColumnDto { ColumnIndex = 0 });

        foreach (var column in dto.Columns)
            column.Subjects = await LoadExamSubjectsAsync(con, session.SchoolID, column.ClassID, ct);

        await using (var row = new SqlCommand("""
SELECT RowIndex, ExamDate, DayName, StartTime, EndTime, Duration, ExamTime
FROM dbo.Exam_Routine_Rows WHERE RoutineID = @ID ORDER BY RowIndex
""", con))
        {
            row.Parameters.AddWithValue("@ID", routineId);
            await using var reader = await row.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Rows.Add(new ExamRoutineRowDto
                {
                    ExamDate = FormatExamDate(reader["ExamDate"]),
                    DayName = reader["DayName"]?.ToString() ?? "",
                    StartTime = reader["StartTime"]?.ToString() ?? "",
                    EndTime = reader["EndTime"]?.ToString() ?? "",
                    Duration = reader["Duration"]?.ToString() ?? reader["ExamTime"]?.ToString() ?? ""
                });
            }
        }
        if (dto.Rows.Count == 0)
            dto.Rows.Add(new ExamRoutineRowDto { Duration = "3 hours" });

        var cells = new List<(int Row, int Col, int SubjectId, string Text)>();
        await using (var cell = new SqlCommand("""
SELECT RowIndex, ColumnIndex, SubjectID, SubjectText
FROM dbo.Exam_Routine_CellData WHERE RoutineID = @ID
""", con))
        {
            cell.Parameters.AddWithValue("@ID", routineId);
            await using var reader = await cell.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                cells.Add((
                    Convert.ToInt32(reader["RowIndex"]),
                    Convert.ToInt32(reader["ColumnIndex"]),
                    reader["SubjectID"] is DBNull ? 0 : Convert.ToInt32(reader["SubjectID"]),
                    reader["SubjectText"]?.ToString() ?? ""));
            }
        }
        for (var r = 0; r < dto.Rows.Count; r++)
        {
            foreach (var column in dto.Columns)
            {
                var match = cells.FirstOrDefault(x => x.Row == r && x.Col == column.ColumnIndex);
                dto.Rows[r].Cells.Add(new ExamRoutineCellDto
                {
                    ColumnIndex = column.ColumnIndex,
                    SubjectID = match.SubjectId,
                    SubjectText = match.Text
                });
            }
        }
        return dto;
    }

    public async Task<IReadOnlyList<RoutineOptionDto>> GetExamSubjectsAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await LoadExamSubjectsAsync(con, session.SchoolID, classId, ct);
    }

    public async Task<RoutineResult> SaveExamAsync(SessionSnapshot session, SaveExamRoutineRequest? request, CancellationToken ct)
    {
        var name = (request?.RoutineName ?? "").Trim();
        if (name.Length == 0)
            return Fail("rt.needExamName");
        var columns = request!.Columns;
        var rows = request.Rows;
        if (columns.Count == 0 || rows.Count == 0)
            return Fail("rt.needExamGrid");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            int id;
            if (request.RoutineID > 0)
            {
                id = request.RoutineID;
                await using var upd = new SqlCommand("""
UPDATE dbo.Exam_Routine_SavedData
SET RoutineName = @Name, ClassColumnCount = @Cols, [RowCount] = @Rows,
    EducationYearID = @YearID, ModifiedDate = GETDATE()
WHERE RoutineID = @ID AND SchoolID = @SchoolID
""", con, tx);
                upd.Parameters.AddWithValue("@Name", name);
                upd.Parameters.AddWithValue("@Cols", columns.Count);
                upd.Parameters.AddWithValue("@Rows", rows.Count);
                upd.Parameters.AddWithValue("@YearID", session.EducationYearID);
                upd.Parameters.AddWithValue("@ID", id);
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await upd.ExecuteNonQueryAsync(ct);
                foreach (var sql in new[]
                {
                    "DELETE FROM dbo.Exam_Routine_CellData WHERE RoutineID = @ID",
                    "DELETE FROM dbo.Exam_Routine_Rows WHERE RoutineID = @ID",
                    "DELETE FROM dbo.Exam_Routine_ClassColumns WHERE RoutineID = @ID"
                })
                {
                    await using var del = new SqlCommand(sql, con, tx);
                    del.Parameters.AddWithValue("@ID", id);
                    await del.ExecuteNonQueryAsync(ct);
                }
            }
            else
            {
                await using var ins = new SqlCommand("""
INSERT INTO dbo.Exam_Routine_SavedData (RoutineName, ClassColumnCount, [RowCount], SchoolID, EducationYearID, CreatedDate)
VALUES (@Name, @Cols, @Rows, @SchoolID, @YearID, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con, tx);
                ins.Parameters.AddWithValue("@Name", name);
                ins.Parameters.AddWithValue("@Cols", columns.Count);
                ins.Parameters.AddWithValue("@Rows", rows.Count);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@YearID", session.EducationYearID);
                id = Convert.ToInt32(await ins.ExecuteScalarAsync(ct));
            }

            for (var i = 0; i < columns.Count; i++)
            {
                await using var col = new SqlCommand("""
INSERT INTO dbo.Exam_Routine_ClassColumns (RoutineID, ColumnIndex, ClassID)
VALUES (@ID, @Idx, @ClassID)
""", con, tx);
                col.Parameters.AddWithValue("@ID", id);
                col.Parameters.AddWithValue("@Idx", i);
                col.Parameters.AddWithValue("@ClassID", columns[i].ClassID);
                await col.ExecuteNonQueryAsync(ct);
            }

            for (var r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                await using var rowCmd = new SqlCommand("""
INSERT INTO dbo.Exam_Routine_Rows (RoutineID, RowIndex, ExamDate, DayName, StartTime, EndTime, Duration, ExamTime)
VALUES (@ID, @Idx, @Date, @Day, @Start, @End, @Duration, @ExamTime)
""", con, tx);
                rowCmd.Parameters.AddWithValue("@ID", id);
                rowCmd.Parameters.AddWithValue("@Idx", r);
                rowCmd.Parameters.AddWithValue("@Date", ParseExamDate(row.ExamDate));
                rowCmd.Parameters.AddWithValue("@Day", (object?)row.DayName ?? DBNull.Value);
                rowCmd.Parameters.AddWithValue("@Start", (object?)row.StartTime ?? DBNull.Value);
                rowCmd.Parameters.AddWithValue("@End", (object?)row.EndTime ?? DBNull.Value);
                rowCmd.Parameters.AddWithValue("@Duration", (object?)row.Duration ?? DBNull.Value);
                rowCmd.Parameters.AddWithValue("@ExamTime", string.IsNullOrWhiteSpace(row.StartTime) ? DBNull.Value : row.StartTime + " - " + row.EndTime);
                await rowCmd.ExecuteNonQueryAsync(ct);

                foreach (var cell in row.Cells)
                {
                    await using var cellCmd = new SqlCommand("""
INSERT INTO dbo.Exam_Routine_CellData (RoutineID, RowIndex, ColumnIndex, SubjectID, SubjectText)
VALUES (@ID, @Row, @Col, @SID, @Text)
""", con, tx);
                    cellCmd.Parameters.AddWithValue("@ID", id);
                    cellCmd.Parameters.AddWithValue("@Row", r);
                    cellCmd.Parameters.AddWithValue("@Col", cell.ColumnIndex);
                    cellCmd.Parameters.AddWithValue("@SID", cell.SubjectID > 0 ? cell.SubjectID : DBNull.Value);
                    cellCmd.Parameters.AddWithValue("@Text", (object?)cell.SubjectText ?? DBNull.Value);
                    await cellCmd.ExecuteNonQueryAsync(ct);
                }
            }

            await tx.CommitAsync(ct);
            return new RoutineResult { Succeeded = true, Count = id, Message = "rt.examSaved" };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return new RoutineResult { Error = ex.Message };
        }
    }

    public async Task<RoutineResult> DeleteExamAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("rt.needExam");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        foreach (var sql in new[]
        {
            "DELETE FROM dbo.Exam_Routine_CellData WHERE RoutineID = @ID",
            "DELETE FROM dbo.Exam_Routine_Rows WHERE RoutineID = @ID",
            "DELETE FROM dbo.Exam_Routine_ClassColumns WHERE RoutineID = @ID",
            "DELETE FROM dbo.Exam_Routine_SavedData WHERE RoutineID = @ID AND SchoolID = @SchoolID"
        })
        {
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        return Ok("rt.examDeleted");
    }

    private async Task<string?> FindTeacherConflictAsync(
        SqlConnection con, SessionSnapshot session, AssignRoutineRequest request, bool excludeSameRoutine, CancellationToken ct)
    {
        foreach (var cell in request.Cells.Where(x => x.TeacherID > 0))
        {
            var sql = """
SELECT TOP 1 t.RoutinePeriod
FROM dbo.RoutineForClass c
INNER JOIN dbo.RoutineTime t ON c.RoutineTimeID = t.RoutineTimeID
WHERE c.SchoolID = @SchoolID AND c.TeacherID = @TeacherID AND c.TeacherID <> 0 AND c.Day = @Day
""" + (excludeSameRoutine ? " AND c.RoutineInfoID <> @RID" : "") + """
  AND (((t.StartTime <= @Start) AND (t.EndTime > @Start))
    OR ((t.StartTime < @End) AND (t.EndTime >= @End))
    OR ((t.StartTime >= @Start) AND (t.EndTime <= @End)))
""";
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@TeacherID", cell.TeacherID);
            cmd.Parameters.AddWithValue("@Day", cell.Day);
            cmd.Parameters.AddWithValue("@Start", cell.StartTime ?? "");
            cmd.Parameters.AddWithValue("@End", cell.EndTime ?? "");
            if (excludeSameRoutine)
                cmd.Parameters.AddWithValue("@RID", request.RoutineInfoID);
            var period = await cmd.ExecuteScalarAsync(ct);
            if (period is not null and not DBNull)
                return "On [" + cell.Day + "] day in [" + period + "] teacher already assigned in another class";
        }
        return null;
    }

    private static async Task<List<RoutinePeriodDto>> LoadPeriodsAsync(
        SqlConnection con, int schoolId, int routineInfoId, bool skipOff, CancellationToken ct)
    {
        var sql = """
SELECT RoutineTimeID, RoutinePeriod, StartTime, EndTime, Duration, ISNULL(Is_OffTime, 0) AS Is_OffTime
FROM dbo.RoutineTime
WHERE RoutineInfoID = @RID AND SchoolID = @SchoolID
""" + (skipOff ? " AND ISNULL(Is_OffTime, 0) = 0" : "") + " ORDER BY StartTime";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@RID", routineInfoId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var rows = new List<RoutinePeriodDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var start = FormatTime(reader["StartTime"]);
            var end = FormatTime(reader["EndTime"]);
            rows.Add(new RoutinePeriodDto
            {
                RoutineTimeID = Convert.ToInt32(reader["RoutineTimeID"]),
                Period = reader["RoutinePeriod"]?.ToString() ?? "",
                StartTime = start,
                EndTime = end,
                TimeRange = start + " - " + end,
                Duration = reader["Duration"]?.ToString() ?? "",
                OffTime = Convert.ToBoolean(reader["Is_OffTime"])
            });
        }
        return rows;
    }

    private static async Task<List<RoutineCellDto>> LoadDayCellsAsync(
        SqlConnection con, SessionSnapshot session, int classId, string group, string section, string shift,
        int routineInfoId, string day, bool forEdit, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT t.RoutineTimeID, t.RoutinePeriod, t.StartTime, t.EndTime, t.Duration, ISNULL(t.Is_OffTime, 0) AS Is_OffTime,
       c.RoutineForClassID, ISNULL(c.SubjectID, 0) AS SubjectID, ISNULL(c.TeacherID, 0) AS TeacherID,
       ISNULL(s.SubjectName, CASE WHEN ISNULL(t.Is_OffTime, 0) = 1 THEN t.RoutinePeriod ELSE N'' END) AS SubjectName,
       ISNULL(te.FirstName + N' ' + te.LastName, CASE WHEN ISNULL(t.Is_OffTime, 0) = 1 THEN N'Off Time' ELSE N'' END) AS TeacherName
FROM dbo.RoutineTime t
LEFT JOIN dbo.RoutineForClass c
    ON t.RoutineTimeID = c.RoutineTimeID AND t.SchoolID = c.SchoolID AND t.RoutineInfoID = c.RoutineInfoID
   AND c.Day = @Day AND c.ClassID = @ClassID AND c.EducationYearID = @YearID
   AND c.SectionID LIKE @SectionID AND c.ShiftID LIKE @ShiftID AND c.SubjectGroupID LIKE @GroupID
LEFT JOIN dbo.Subject s ON c.SubjectID = s.SubjectID
LEFT JOIN dbo.Teacher te ON c.TeacherID = te.TeacherID
WHERE t.RoutineInfoID = @RID AND t.SchoolID = @SchoolID
""" + (forEdit ? " AND ISNULL(t.Is_OffTime, 0) = 0" : "") + """
ORDER BY t.StartTime
""", con);
        AddFilter(cmd, session, classId, group, section, shift, routineInfoId);
        cmd.Parameters.AddWithValue("@Day", day);
        var cells = new List<RoutineCellDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            cells.Add(new RoutineCellDto
            {
                RoutineTimeID = Convert.ToInt32(reader["RoutineTimeID"]),
                RoutineForClassID = reader["RoutineForClassID"] is DBNull ? 0 : Convert.ToInt32(reader["RoutineForClassID"]),
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                TeacherID = Convert.ToInt32(reader["TeacherID"]),
                SubjectName = reader["SubjectName"]?.ToString() ?? "",
                TeacherName = reader["TeacherName"]?.ToString() ?? "",
                StartTime = FormatTime(reader["StartTime"]),
                EndTime = FormatTime(reader["EndTime"]),
                OffTime = Convert.ToBoolean(reader["Is_OffTime"])
            });
        }
        return cells;
    }

    private static async Task<List<RoutineOptionDto>> LoadSubjectsAsync(
        SqlConnection con, int schoolId, int classId, int groupId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT DISTINCT s.SubjectID, s.SubjectName
FROM dbo.SubjectForGroup g
INNER JOIN dbo.Subject s ON g.SubjectID = s.SubjectID
WHERE g.ClassID = @ClassID AND (g.SubjectGroupID LIKE @GroupID)
ORDER BY s.SubjectName
""", con);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@GroupID", Like(groupId));
        var rows = new List<RoutineOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new RoutineOptionDto
            {
                Id = Convert.ToInt32(reader["SubjectID"]),
                Name = reader["SubjectName"]?.ToString() ?? ""
            });
        }
        return rows;
    }

    private static async Task<List<RoutineOptionDto>> LoadTeachersAsync(
        SqlConnection con, SessionSnapshot session, int classId, int subjectId, string day, string start, string end,
        int exceptRoutineInfoId, int keepTeacherId, CancellationToken ct)
    {
        var sql = """
SELECT Teacher.FirstName + N' ' + Teacher.LastName AS Name, ts.TeacherID
FROM dbo.TecherSubject ts
INNER JOIN dbo.Teacher ON ts.TeacherID = Teacher.TeacherID
WHERE ts.SubjectID = @SubjectID AND ts.ClassID = @ClassID
  AND ts.TeacherID NOT IN (
      SELECT c.TeacherID FROM dbo.RoutineForClass c
      INNER JOIN dbo.RoutineTime t ON c.RoutineTimeID = t.RoutineTimeID
      WHERE c.SchoolID = @SchoolID AND c.EducationYearID = @YearID AND c.Day = @Day AND c.TeacherID <> 0
""" + (exceptRoutineInfoId > 0 ? " AND c.RoutineInfoID <> @RID" : "") + """
        AND (((t.StartTime <= @Start) AND (t.EndTime > @Start))
          OR ((t.StartTime < @End) AND (t.EndTime >= @End))
          OR ((t.StartTime >= @Start) AND (t.EndTime <= @End))))
""";
        if (keepTeacherId > 0)
        {
            sql += """
UNION
SELECT Teacher.FirstName + N' ' + Teacher.LastName AS Name, Teacher.TeacherID
FROM dbo.Teacher WHERE TeacherID = @Keep
""";
        }
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SubjectID", subjectId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Day", day);
        cmd.Parameters.AddWithValue("@Start", start);
        cmd.Parameters.AddWithValue("@End", end);
        if (exceptRoutineInfoId > 0)
            cmd.Parameters.AddWithValue("@RID", exceptRoutineInfoId);
        if (keepTeacherId > 0)
            cmd.Parameters.AddWithValue("@Keep", keepTeacherId);
        var rows = new List<RoutineOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new RoutineOptionDto
            {
                Id = Convert.ToInt32(reader["TeacherID"]),
                Name = (reader["Name"]?.ToString() ?? "").Trim()
            });
        }
        return rows.GroupBy(x => x.Id).Select(g => g.First()).OrderBy(x => x.Name).ToList();
    }

    private static async Task<List<RoutineNameDto>> LoadClassRoutinesAsync(SqlConnection con, int schoolId, int classId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT DISTINCT i.RoutineInfoID, i.RoutineSpecification
FROM dbo.RoutineInfo i
INNER JOIN dbo.RoutineForClass c ON i.RoutineInfoID = c.RoutineInfoID
WHERE i.SchoolID = @SchoolID AND c.ClassID = @ClassID
ORDER BY i.RoutineSpecification
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        var rows = new List<RoutineNameDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new RoutineNameDto
            {
                RoutineInfoID = Convert.ToInt32(reader["RoutineInfoID"]),
                Name = reader["RoutineSpecification"]?.ToString() ?? "",
                Used = true
            });
        }
        return rows;
    }

    private static async Task<List<ExamRoutineListItemDto>> LoadExamListAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT RoutineID, RoutineName, CreatedDate
FROM dbo.Exam_Routine_SavedData
WHERE SchoolID = @SchoolID AND ISNULL(IsActive, 1) = 1
ORDER BY CreatedDate DESC
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var rows = new List<ExamRoutineListItemDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new ExamRoutineListItemDto
            {
                RoutineID = Convert.ToInt32(reader["RoutineID"]),
                Name = reader["RoutineName"]?.ToString() ?? "",
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
            });
        }
        return rows;
    }

    private static async Task<List<RoutineOptionDto>> LoadClassesAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT ClassID, Class FROM dbo.CreateClass WHERE SchoolID = @SchoolID ORDER BY SN
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var rows = new List<RoutineOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new RoutineOptionDto
            {
                Id = Convert.ToInt32(reader["ClassID"]),
                Name = reader["Class"]?.ToString() ?? ""
            });
        }
        return rows;
    }

    private static async Task<List<RoutineOptionDto>> LoadExamSubjectsAsync(SqlConnection con, int schoolId, int classId, CancellationToken ct)
    {
        if (classId <= 0) return [];
        await using var cmd = new SqlCommand("""
SELECT DISTINCT s.SubjectID, s.SubjectName
FROM dbo.Subject s
INNER JOIN dbo.SubjectForGroup g ON s.SubjectID = g.SubjectID
WHERE g.ClassID = @ClassID AND g.SchoolID = @SchoolID
ORDER BY s.SubjectName
""", con);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var rows = new List<RoutineOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new RoutineOptionDto
            {
                Id = Convert.ToInt32(reader["SubjectID"]),
                Name = reader["SubjectName"]?.ToString() ?? ""
            });
        }
        return rows;
    }

    private static void AddFilter(
        SqlCommand cmd, SessionSnapshot session, int classId, string group, string section, string shift, int routineInfoId)
    {
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@GroupID", group);
        cmd.Parameters.AddWithValue("@SectionID", section);
        cmd.Parameters.AddWithValue("@ShiftID", shift);
        cmd.Parameters.AddWithValue("@RID", routineInfoId);
    }

    private static string Like(int id) => id <= 0 ? "%" : id.ToString();
    private static int Store(int id) => id <= 0 ? 0 : id;

    private static string FormatTime(object? value)
    {
        if (value is null or DBNull) return "";
        if (DateTime.TryParse(value.ToString(), out var dt))
            return dt.ToString("h:mm tt");
        if (value is TimeSpan ts)
            return DateTime.Today.Add(ts).ToString("h:mm tt");
        return value.ToString() ?? "";
    }

    private static string FormatExamDate(object? value)
    {
        if (value is null or DBNull) return "";
        if (value is DateTime dt) return dt.ToString("dd/MM/yyyy");
        if (DateTime.TryParse(value.ToString(), out var parsed))
            return parsed.ToString("dd/MM/yyyy");
        return value.ToString() ?? "";
    }

    private static object ParseExamDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return DBNull.Value;
        if (DateTime.TryParseExact(text.Trim(), ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"],
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
            return dt.Date;
        if (DateTime.TryParse(text, out var any))
            return any.Date;
        return DBNull.Value;
    }

    private static RoutineResult Fail(string error) => new() { Error = error };
    private static RoutineResult Ok(string message) => new() { Succeeded = true, Message = message };
}
