using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Teachers;

namespace Sikkhaloy.SyncApi.Services;

public sealed class TeacherPortalService
{
    private readonly EduConnectionFactory _connections;
    private readonly ExamService _exams;
    private readonly AttendanceService _attendance;
    private readonly StudentInfoService _studentInfo;
    private readonly StudentManagementService _notices;

    public TeacherPortalService(
        EduConnectionFactory connections,
        ExamService exams,
        AttendanceService attendance,
        StudentInfoService studentInfo,
        StudentManagementService notices)
    {
        _connections = connections;
        _exams = exams;
        _attendance = attendance;
        _studentInfo = studentInfo;
        _notices = notices;
    }

    public async Task<TeacherPortalDashboardDto> GetDashboardAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new TeacherPortalDashboardDto
        {
            Name = session.DisplayName,
            SchoolName = session.SchoolName
        };
        if (!IsPortal(session))
            return dto;

        var details = await GetDetailsAsync(session, ct);
        dto.Name = string.IsNullOrWhiteSpace(details.Name) ? dto.Name : details.Name;
        dto.Designation = details.Designation;
        dto.Phone = details.Phone;
        dto.Address = details.Address;
        dto.Email = details.Email;
        dto.PhotoDataUrl = details.PhotoDataUrl;

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using (var cmd = new SqlCommand("""
SELECT TOP 1 ISNULL(SchoolName, N'') AS SchoolName, ISNULL(Address, N'') AS Address
FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID
""", con))
            {
                AddTeacher(cmd, session);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    var name = Text(reader["SchoolName"]);
                    if (name.Length > 0)
                        dto.SchoolName = name;
                    dto.SchoolAddress = Text(reader["Address"]);
                }
            }

            await using (var year = new SqlCommand("""
SELECT TOP 1 ISNULL(EducationYear, N'') FROM dbo.Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con))
            {
                AddTeacher(year, session);
                dto.YearName = Text(await year.ExecuteScalarAsync(ct));
            }
        }
        catch (SqlException)
        {
        }

        dto.TodayRoutine = (await GetRoutineAsync(session, ct))
            .Where(x => string.Equals(x.Day, TodayKey(), StringComparison.OrdinalIgnoreCase))
            .ToList();
        dto.TodayPeriodCount = dto.TodayRoutine.Count;
        dto.SubjectCount = (await GetSubjectsAsync(session, ct)).Count;
        ApplyAttendanceSummary(dto, await GetAttendanceAsync(session, ct));
        return dto;
    }

    public async Task<TeacherPortalDetailsDto> GetDetailsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new TeacherPortalDetailsDto { Name = session.DisplayName };
        if (!IsPortal(session))
            return dto;

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1
    LTRIM(RTRIM(ISNULL(FirstName, N'') + N' ' + ISNULL(LastName, N''))) AS Name,
    ISNULL(Designation, N'') AS Designation,
    ISNULL(Phone, N'') AS Phone,
    ISNULL(Email, N'') AS Email,
    ISNULL(Gender, N'') AS Gender,
    ISNULL(DateofBirth, N'') AS DateofBirth,
    ISNULL(Religion, N'') AS Religion,
    ISNULL(FatherName, N'') AS FatherName,
    ISNULL(MothersName, N'') AS MothersName,
    ISNULL(Address, N'') AS Address,
    ISNULL(PermanentAddress, N'') AS PermanentAddress,
    ISNULL(Nationality, N'') AS Nationality,
    Image
FROM dbo.Teacher
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID
""", con);
            AddTeacher(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return dto;

            dto.Name = Text(reader["Name"]);
            dto.Designation = Text(reader["Designation"]);
            dto.Phone = Text(reader["Phone"]);
            dto.Email = Text(reader["Email"]);
            dto.Gender = Text(reader["Gender"]);
            dto.DateOfBirth = Text(reader["DateofBirth"]);
            dto.Religion = Text(reader["Religion"]);
            dto.FatherName = Text(reader["FatherName"]);
            dto.MothersName = Text(reader["MothersName"]);
            dto.Address = Text(reader["Address"]);
            dto.PermanentAddress = Text(reader["PermanentAddress"]);
            dto.Nationality = Text(reader["Nationality"]);
            dto.PhotoDataUrl = ToDataUrl(reader["Image"] as byte[]);
        }
        catch (SqlException)
        {
        }

        return dto;
    }

    public async Task<List<EducationYearDto>> GetSessionsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<EducationYearDto>();
        if (!IsPortal(session))
            return items;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT EducationYearID, EducationYear, StartDate, EndDate
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID
ORDER BY StartDate DESC, EducationYearID DESC
""", con);
            AddTeacher(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["EducationYearID"]);
                items.Add(new EducationYearDto
                {
                    EducationYearID = id,
                    Name = Text(reader["EducationYear"]),
                    StartDate = Day(reader["StartDate"]),
                    EndDate = Day(reader["EndDate"]),
                    IsCurrent = id == session.EducationYearID
                });
            }
        }
        catch (SqlException)
        {
        }

        return items;
    }

    public async Task<StudentPortalAttendanceDto> GetAttendanceAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalAttendanceDto();
        if (!IsPortal(session) || session.EmployeeID <= 0)
            return dto;

        dto.Days = await QueryListAsync(session, """
SELECT CAST(r.AttendanceDate AS date) AS AttendanceDate,
       ISNULL(r.AttendanceStatus, N'') AS Attendance,
       r.EntryTime, r.ExitTime
FROM dbo.Employee_Attendance_Record AS r
INNER JOIN dbo.Teacher AS t ON t.EmployeeID = r.EmployeeID AND t.SchoolID = r.SchoolID
WHERE t.TeacherID = @TeacherID AND r.SchoolID = @SchoolID
ORDER BY r.AttendanceDate
""", ct, r => new StudentPortalAttendanceDayDto
        {
            Date = Day(r["AttendanceDate"]) ?? DateTime.MinValue,
            Status = Text(r["Attendance"]),
            EntryTime = TimeText(r["EntryTime"]),
            ExitTime = TimeText(r["ExitTime"])
        });

        foreach (var day in dto.Days)
        {
            var key = day.Status.Trim();
            if (key.Equals("Pre", StringComparison.OrdinalIgnoreCase) || key.Equals("Present", StringComparison.OrdinalIgnoreCase))
                dto.Present++;
            else if (key.Equals("Abs", StringComparison.OrdinalIgnoreCase))
                dto.Absent++;
            else if (key.Equals("Late", StringComparison.OrdinalIgnoreCase))
                dto.Late++;
            else if (key.Equals("Leave", StringComparison.OrdinalIgnoreCase))
                dto.Leave++;
            else if (key.Contains("Late", StringComparison.OrdinalIgnoreCase))
                dto.LateAbsent++;
        }

        dto.Holidays = await QueryListAsync(session, """
SELECT CAST(HolidayDate AS date) AS HolidayDate, HolidayName
FROM dbo.Employee_Holiday
WHERE SchoolID = @SchoolID
""", ct, r => new StudentPortalHolidayDto
        {
            Date = Day(r["HolidayDate"]) ?? DateTime.MinValue,
            Name = Text(r["HolidayName"])
        });

        return dto;
    }

    public async Task<List<TeacherPortalPeriodDto>> GetRoutineAsync(SessionSnapshot session, CancellationToken ct)
    {
        var items = await QueryRoutineAsync(session, yearFilter: true, ct);
        if (items.Count == 0)
            items = await QueryRoutineAsync(session, yearFilter: false, ct);
        return items;
    }

    private Task<List<TeacherPortalPeriodDto>> QueryRoutineAsync(SessionSnapshot session, bool yearFilter, CancellationToken ct)
    {
        var yearSql = yearFilter ? " AND RoutineForClass.EducationYearID = @EducationYearID" : "";
        return QueryListAsync(session, $"""
SELECT RoutineForClass.Day,
       ISNULL(RoutineTime.RoutinePeriod, N'') AS RoutinePeriod,
       ISNULL(Subject.SubjectName, N'') AS SubjectName,
       ISNULL(CreateClass.Class, N'') AS ClassName,
       ISNULL(CreateSection.Section, N'') AS SectionName,
       ISNULL(CreateShift.Shift, N'') AS ShiftName,
       ISNULL(CreateSubjectGroup.SubjectGroup, N'') AS GroupName,
       RoutineTime.StartTime, RoutineTime.EndTime
FROM dbo.RoutineForClass
LEFT JOIN dbo.RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID
LEFT JOIN dbo.Subject ON RoutineForClass.SubjectID = Subject.SubjectID
LEFT JOIN dbo.CreateClass ON RoutineForClass.ClassID = CreateClass.ClassID
LEFT JOIN dbo.CreateSection ON RoutineForClass.SectionID = CreateSection.SectionID
LEFT JOIN dbo.CreateShift ON RoutineForClass.ShiftID = CreateShift.ShiftID
LEFT JOIN dbo.CreateSubjectGroup ON RoutineForClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
WHERE RoutineForClass.SchoolID = @SchoolID
  AND RoutineForClass.TeacherID = @TeacherID
{yearSql}
ORDER BY RoutineTime.StartTime
""", ct, r => new TeacherPortalPeriodDto
        {
            Day = NormalizeDay(Text(r["Day"])),
            Period = Text(r["RoutinePeriod"]),
            Subject = Text(r["SubjectName"]),
            ClassName = Text(r["ClassName"]),
            Section = Text(r["SectionName"]),
            Shift = Text(r["ShiftName"]),
            Group = Text(r["GroupName"]),
            Time = FormatRange(r["StartTime"], r["EndTime"])
        });
    }

    public async Task<List<TeacherPortalSubjectDto>> GetSubjectsAsync(SessionSnapshot session, CancellationToken ct) =>
        await QueryListAsync(session, """
SELECT DISTINCT CreateClass.ClassID, ISNULL(CreateClass.Class, N'') AS ClassName,
       Subject.SubjectID, ISNULL(Subject.SubjectName, N'') AS SubjectName
FROM dbo.TecherSubject
INNER JOIN dbo.CreateClass ON CreateClass.ClassID = TecherSubject.ClassID
INNER JOIN dbo.Subject ON Subject.SubjectID = TecherSubject.SubjectID
WHERE TecherSubject.SchoolID = @SchoolID AND TecherSubject.TeacherID = @TeacherID
ORDER BY CreateClass.Class, Subject.SubjectName
""", ct, r => new TeacherPortalSubjectDto
        {
            ClassID = ToInt(r["ClassID"]),
            ClassName = Text(r["ClassName"]),
            SubjectID = ToInt(r["SubjectID"]),
            SubjectName = Text(r["SubjectName"])
        });

    public async Task<List<ExamNameDto>> GetExamsAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        var items = new List<ExamNameDto>();
        if (!IsPortal(session) || classId <= 0)
            return items;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName, Exam_Name.Period_StartDate, Exam_Name.Period_EndDate
FROM dbo.Exam_Name
INNER JOIN dbo.Exam_Full_Marks ON Exam_Name.ExamID = Exam_Full_Marks.ExamID
INNER JOIN dbo.TecherSubject ON TecherSubject.ClassID = Exam_Full_Marks.ClassID
    AND TecherSubject.SchoolID = Exam_Name.SchoolID
WHERE Exam_Name.SchoolID = @SchoolID
  AND Exam_Name.EducationYearID = @EducationYearID
  AND Exam_Full_Marks.ClassID = @ClassID
  AND TecherSubject.TeacherID = @TeacherID
ORDER BY Exam_Name.ExamName
""", con);
            AddTeacher(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
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
        }
        catch (SqlException)
        {
        }

        return items;
    }

    public Task<IReadOnlyList<SubExamDto>> GetSubExamsAsync(SessionSnapshot session, CancellationToken ct) =>
        _exams.ListSubExamsAsync(session, ct);

    public async Task<ExamFilterDto> GetMarksFiltersAsync(
        SessionSnapshot session, int examId, int classId, int subjectId,
        string? groupId, string? sectionId, string? shiftId, CancellationToken ct)
    {
        var dto = new ExamFilterDto();
        if (!IsPortal(session))
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        dto.Exams = await QueryMarksOptionsAsync(con, session, """
SELECT ExamID AS Id, ExamName AS Name
FROM dbo.Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
ORDER BY ExamID
""", ct);

        if (examId > 0)
        {
            dto.Classes = await QueryMarksOptionsAsync(con, session, """
SELECT DISTINCT CreateClass.ClassID AS Id, CreateClass.Class AS Name, CreateClass.SN
FROM dbo.CreateClass
INNER JOIN dbo.Exam_Full_Marks ON CreateClass.ClassID = Exam_Full_Marks.ClassID
INNER JOIN dbo.TecherSubject ON CreateClass.ClassID = TecherSubject.ClassID
    AND TecherSubject.SchoolID = CreateClass.SchoolID
WHERE CreateClass.SchoolID = @SchoolID
  AND Exam_Full_Marks.EducationYearID = @EducationYearID
  AND Exam_Full_Marks.ExamID = @ExamID
  AND TecherSubject.TeacherID = @TeacherID
ORDER BY CreateClass.SN
""", ct, c => c.Parameters.AddWithValue("@ExamID", examId));
        }

        if (classId > 0)
        {
            dto.Groups = await QueryMarksOptionsAsync(con, session, """
SELECT DISTINCT [Join].SubjectGroupID AS Id, CreateSubjectGroup.SubjectGroup AS Name
FROM [Join] INNER JOIN dbo.CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID
WHERE [Join].ClassID = @ClassID
  AND (@SectionID IS NULL OR [Join].SectionID = @SectionID)
  AND (@ShiftID IS NULL OR [Join].ShiftID = @ShiftID)
""", ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@SectionID", FilterId(sectionId));
                c.Parameters.AddWithValue("@ShiftID", FilterId(shiftId));
            });
            dto.Sections = await QueryMarksOptionsAsync(con, session, """
SELECT DISTINCT [Join].SectionID AS Id, CreateSection.Section AS Name
FROM [Join] INNER JOIN dbo.CreateSection ON [Join].SectionID = CreateSection.SectionID
WHERE [Join].ClassID = @ClassID
  AND (@GroupID IS NULL OR [Join].SubjectGroupID = @GroupID)
  AND (@ShiftID IS NULL OR [Join].ShiftID = @ShiftID)
""", ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@GroupID", FilterId(groupId));
                c.Parameters.AddWithValue("@ShiftID", FilterId(shiftId));
            });
            dto.Shifts = await QueryMarksOptionsAsync(con, session, """
SELECT DISTINCT [Join].ShiftID AS Id, CreateShift.Shift AS Name
FROM [Join] INNER JOIN dbo.CreateShift ON [Join].ShiftID = CreateShift.ShiftID
WHERE [Join].ClassID = @ClassID
  AND (@GroupID IS NULL OR [Join].SubjectGroupID = @GroupID)
  AND (@SectionID IS NULL OR [Join].SectionID = @SectionID)
""", ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@GroupID", FilterId(groupId));
                c.Parameters.AddWithValue("@SectionID", FilterId(sectionId));
            });
        }

        if (examId > 0 && classId > 0)
        {
            dto.Subjects = await QueryMarksOptionsAsync(con, session, """
SELECT DISTINCT Subject.SubjectID AS Id, Subject.SubjectName AS Name
FROM dbo.Subject
INNER JOIN dbo.Exam_Full_Marks ON Subject.SubjectID = Exam_Full_Marks.SubjectID
INNER JOIN dbo.TecherSubject ON TecherSubject.SubjectID = Subject.SubjectID
    AND TecherSubject.ClassID = Exam_Full_Marks.ClassID
    AND TecherSubject.SchoolID = Exam_Full_Marks.SchoolID
WHERE Exam_Full_Marks.EducationYearID = @EducationYearID
  AND Exam_Full_Marks.ClassID = @ClassID
  AND Exam_Full_Marks.SchoolID = @SchoolID
  AND Exam_Full_Marks.ExamID = @ExamID
  AND TecherSubject.TeacherID = @TeacherID
ORDER BY Subject.SubjectName
""", ct, c =>
            {
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@ExamID", examId);
            });
        }

        if (examId > 0 && classId > 0 && subjectId > 0)
        {
            dto.SubExams = await QueryMarksOptionsAsync(con, session, """
SELECT Exam_SubExam_Name.SubExamID AS Id, Exam_SubExam_Name.SubExamName AS Name
FROM dbo.Exam_SubExam_Name
INNER JOIN dbo.Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID
WHERE Exam_Full_Marks.SubjectID = @SubjectID
  AND Exam_Full_Marks.ClassID = @ClassID
  AND Exam_SubExam_Name.SchoolID = @SchoolID
  AND Exam_Full_Marks.ExamID = @ExamID
  AND Exam_Full_Marks.EducationYearID = @EducationYearID
""", ct, c =>
            {
                c.Parameters.AddWithValue("@SubjectID", subjectId);
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@ExamID", examId);
            });
        }

        return dto;
    }

    public async Task<InputSheetDto> GetInputSheetAsync(
        SessionSnapshot session, int examId, int classId, int subjectId, int subExamId,
        string? groupId, string? sectionId, string? shiftId, CancellationToken ct)
    {
        if (!await OwnsSubjectAsync(session, classId, subjectId, ct))
            return new InputSheetDto();
        return await _exams.GetInputSheetAsync(session, examId, classId, subjectId, subExamId, groupId, sectionId, shiftId, ct);
    }

    public async Task<ExamResult> SaveInputMarksAsync(SessionSnapshot session, SaveInputMarksRequest? request, CancellationToken ct)
    {
        if (request is null || !await OwnsSubjectAsync(session, request.ClassID, request.SubjectID, ct))
            return new ExamResult { Succeeded = false, Error = "auth.forbidden" };
        if (await IsMarksLockedAsync(session, request.ClassID, request.ExamID, ct))
            return new ExamResult { Succeeded = false, Error = "tch.marksLocked" };
        return await _exams.SaveInputMarksAsync(session, request, ct);
    }

    public async Task<List<TeacherPortalStudentHitDto>> SearchStudentsAsync(SessionSnapshot session, string? query, CancellationToken ct)
    {
        var items = new List<TeacherPortalStudentHitDto>();
        if (!IsPortal(session))
            return items;
        var q = (query ?? "").Trim();
        if (q.Length < 1)
            return items;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP (25)
    Student.StudentID, StudentsClass.StudentClassID, Student.ID,
    Student.StudentsName, ISNULL(CreateClass.Class, N'') AS ClassName,
    ISNULL(StudentsClass.RollNo, N'') AS RollNo,
    ISNULL(CreateSection.Section, N'') AS SectionName,
    ISNULL(CreateSubjectGroup.SubjectGroup, N'') AS GroupName,
    ISNULL(CreateShift.Shift, N'') AS ShiftName
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON Student.StudentID = StudentsClass.StudentID
LEFT JOIN dbo.CreateClass ON CreateClass.ClassID = StudentsClass.ClassID
LEFT JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND Student.Status = N'Active'
  AND (Student.ID LIKE @Q OR Student.StudentsName LIKE @Q)
ORDER BY CreateClass.SN, Student.StudentsName
""", con);
            AddTeacher(cmd, session);
            cmd.Parameters.AddWithValue("@Q", "%" + q + "%");
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new TeacherPortalStudentHitDto
                {
                    StudentID = ToInt(reader["StudentID"]),
                    StudentClassID = ToInt(reader["StudentClassID"]),
                    StudentCode = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    ClassName = Text(reader["ClassName"]),
                    RollNo = Text(reader["RollNo"]),
                    SectionName = Text(reader["SectionName"]),
                    GroupName = Text(reader["GroupName"]),
                    ShiftName = Text(reader["ShiftName"])
                });
            }
        }
        catch (SqlException)
        {
        }

        return items;
    }

    public async Task<StudentPhotoDto> GetStudentPhotoAsync(SessionSnapshot session, string? studentCode, CancellationToken ct)
    {
        var dto = new StudentPhotoDto();
        var code = (studentCode ?? "").Trim();
        if (!IsPortal(session) || code.Length == 0)
            return dto;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 Student.StudentID, Student_Image.Image
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON Student.StudentID = StudentsClass.StudentID
LEFT JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND Student.ID = @ID
""", con);
            AddTeacher(cmd, session);
            cmd.Parameters.AddWithValue("@ID", code);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.StudentID = ToInt(reader["StudentID"]);
                dto.PhotoDataUrl = ToDataUrl(reader["Image"] as byte[]);
            }
        }
        catch (SqlException)
        {
        }

        return dto;
    }

    public async Task<List<StudentPortalFaultReportDto>> GetReportsAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, string? studentCode, CancellationToken ct)
    {
        var code = (studentCode ?? "").Trim();
        if (code.Length > 0)
            return await _studentInfo.ListFaultReportsAsync(session, code, from, to, ct);

        var items = new List<StudentPortalFaultReportDto>();
        if (!IsPortal(session))
            return items;
        var fromDate = from?.Date ?? new DateTime(1753, 1, 1);
        var toDate = to?.Date ?? new DateTime(9999, 12, 31);
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP (80) sf.StudentFaultID, sf.Fault_Title, sf.Fault, sf.Fault_Date,
       COALESCE(NULLIF(t.Name, N''), NULLIF(a.Name, N''), ISNULL(r.UserName, N'')) AS UserName,
       ISNULL(s.ID, N'') AS StudentCode,
       ISNULL(s.StudentsName, N'') AS StudentsName
FROM dbo.Student_Fault AS sf
LEFT JOIN dbo.Registration AS r ON sf.RegistrationID = r.RegistrationID
LEFT JOIN dbo.Student AS s ON sf.StudentID = s.StudentID
OUTER APPLY (
    SELECT TOP 1 LTRIM(RTRIM(ISNULL(FirstName, N'') + N' ' + ISNULL(LastName, N''))) AS Name
    FROM dbo.Teacher
    WHERE SchoolID = sf.SchoolID
      AND (TeacherRegistrationID = r.RegistrationID OR RegistrationID = r.RegistrationID)
) AS t
OUTER APPLY (
    SELECT TOP 1 LTRIM(RTRIM(ISNULL(FirstName, N'') + N' ' + ISNULL(LastName, N''))) AS Name
    FROM dbo.Admin
    WHERE SchoolID = sf.SchoolID AND RegistrationID = r.RegistrationID
) AS a
WHERE sf.SchoolID = @SchoolID
  AND sf.EducationYearID = @EducationYearID
  AND sf.RegistrationID = @RegistrationID
  AND CAST(sf.Fault_Date AS date) BETWEEN @FromDate AND @ToDate
ORDER BY sf.Fault_Date DESC, sf.StudentFaultID DESC
""", con) { CommandTimeout = 20 };
            AddTeacher(cmd, session);
            cmd.Parameters.AddWithValue("@FromDate", fromDate);
            cmd.Parameters.AddWithValue("@ToDate", toDate);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new StudentPortalFaultReportDto
                {
                    StudentFaultID = ToInt(reader["StudentFaultID"]),
                    Title = Text(reader["Fault_Title"]),
                    Body = Text(reader["Fault"]),
                    Date = Day(reader["Fault_Date"]),
                    PostBy = Text(reader["UserName"]),
                    StudentCode = Text(reader["StudentCode"]),
                    StudentsName = Text(reader["StudentsName"])
                });
            }
        }
        catch (SqlException)
        {
        }

        return items;
    }

    public Task<StudentInfoResult> SaveReportAsync(
        SessionSnapshot session, SaveStudentFaultReportRequest request, CancellationToken ct) =>
        _studentInfo.SaveFaultReportAsync(session, request, ct);

    public Task<StudentInfoResult> SaveReportsBulkAsync(
        SessionSnapshot session, SaveStudentFaultReportsBulkRequest request, CancellationToken ct) =>
        _studentInfo.SaveFaultReportsBulkAsync(session, request, ct);

    public Task<StudentInfoResult> UpdateReportAsync(
        SessionSnapshot session, UpdateStudentFaultReportRequest request, CancellationToken ct) =>
        _studentInfo.UpdateFaultReportAsync(session, request, ct);

    public Task<StudentInfoResult> DeleteReportAsync(
        SessionSnapshot session, int studentFaultId, CancellationToken ct) =>
        _studentInfo.DeleteFaultReportAsync(session, studentFaultId, ct);

    public async Task<IReadOnlyList<NoticeDto>> GetNoticesAsync(SessionSnapshot session, CancellationToken ct)
    {
        var all = await _notices.ListNoticesAsync(session, ct);
        return all.Where(x => string.Equals(x.UserName, session.UserName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public Task<StudentInfoResult> SaveNoticeAsync(SessionSnapshot session, SaveNoticeRequest request, CancellationToken ct) =>
        _notices.SaveNoticeAsync(session, request, ct);

    public async Task<List<TeacherPortalOptionDto>> GetClassesAsync(SessionSnapshot session, CancellationToken ct) =>
        await QueryListAsync(session, """
SELECT ClassID, ISNULL(Class, N'') AS ClassName
FROM dbo.CreateClass
WHERE SchoolID = @SchoolID
ORDER BY SN, Class
""", ct, r => new TeacherPortalOptionDto
        {
            Id = ToInt(r["ClassID"]),
            Name = Text(r["ClassName"])
        });

    public async Task<TeacherPortalAttendanceMetaDto> GetStudentAttendanceMetaAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new TeacherPortalAttendanceMetaDto
        {
            Classes = await GetClassesAsync(session, ct)
        };
        if (!IsPortal(session))
            return dto;

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using (var cmd = new SqlCommand("""
SELECT TOP 1 Teacher_BackDate_Attendance FROM dbo.Account WHERE SchoolID = @SchoolID
""", con))
            {
                AddTeacher(cmd, session);
                var value = await cmd.ExecuteScalarAsync(ct);
                dto.AllowBackDate = value is true or 1 or (byte)1;
            }

            await using (var cmd = new SqlCommand("""
SELECT ScheduleID, ISNULL(ScheduleName, N'') AS ScheduleName
FROM dbo.Attendance_Schedule
WHERE SchoolID = @SchoolID
ORDER BY ScheduleName
""", con))
            {
                AddTeacher(cmd, session);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    dto.Schedules.Add(new TeacherPortalOptionDto
                    {
                        Id = ToInt(reader["ScheduleID"]),
                        Name = Text(reader["ScheduleName"])
                    });
                }
            }
        }
        catch (SqlException)
        {
        }

        return dto;
    }

    public async Task<IReadOnlyList<StudentManualRowDto>> GetStudentAttendanceRowsAsync(
        SessionSnapshot session, int scheduleId, int classId, DateTime date, CancellationToken ct)
    {
        if (!IsPortal(session) || scheduleId <= 0 || classId <= 0)
            return [];
        if (!await CanTakeAttendanceOnAsync(session, date, ct))
            return [];
        return await _attendance.ListStudentManualAsync(session, scheduleId, classId, 0, 0, 0, date, ct);
    }

    public async Task<AttendanceResult> SaveStudentAttendanceAsync(
        SessionSnapshot session, SaveStudentManualRequest? request, CancellationToken ct)
    {
        if (!IsPortal(session) || request is null)
            return new AttendanceResult { Succeeded = false, Error = "auth.forbidden" };
        if (!await CanTakeAttendanceOnAsync(session, request.AttendanceDate, ct))
            return new AttendanceResult { Succeeded = false, Error = "tch.attTodayOnly" };
        return await _attendance.SaveStudentManualAsync(session, request, ct);
    }

    public async Task<TeacherPortalCvDto> GetCvAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new TeacherPortalCvDto();
        var details = await GetDetailsAsync(session, ct);
        dto.Name = details.Name;
        dto.Designation = details.Designation;
        dto.Phone = details.Phone;
        dto.Email = details.Email;
        dto.Address = details.Address;
        dto.PhotoDataUrl = details.PhotoDataUrl;
        if (!IsPortal(session))
            return dto;

        dto.Objectives = await QueryListAsync(session, """
SELECT CareerObjectiveID, ISNULL(CareerObjective, N'') AS CareerObjective
FROM dbo.Teacher_Career_Objective
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID
""", ct, r => new TeacherPortalCvLineDto { Id = ToInt(r["CareerObjectiveID"]), Text = Text(r["CareerObjective"]) });

        dto.Education = await QueryListAsync(session, """
SELECT TeacherEducationID, ISNULL(InstitutionName, N'') AS InstitutionName,
       ISNULL(ExamName, N'') AS ExamName, ISNULL(ExamYear, N'') AS ExamYear, ISNULL(Result, N'') AS Result
FROM dbo.Teacher_EducationInfo
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID
""", ct, r => new TeacherPortalCvEducationDto
        {
            Id = ToInt(r["TeacherEducationID"]),
            InstitutionName = Text(r["InstitutionName"]),
            ExamName = Text(r["ExamName"]),
            ExamYear = Text(r["ExamYear"]),
            Result = Text(r["Result"])
        });

        dto.Jobs = await QueryListAsync(session, """
SELECT TeacherJobID, ISNULL(InstitutionName, N'') AS InstitutionName, ISNULL(Position, N'') AS Position,
       ISNULL(JobType, N'') AS JobType, ISNULL(JobYear, N'') AS JobYear, ISNULL(Address, N'') AS Address,
       ISNULL(Phone, N'') AS Phone, ISNULL(Email, N'') AS Email
FROM dbo.Teacher_JobInfo
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID
""", ct, r => new TeacherPortalCvJobDto
        {
            Id = ToInt(r["TeacherJobID"]),
            InstitutionName = Text(r["InstitutionName"]),
            Position = Text(r["Position"]),
            JobType = Text(r["JobType"]),
            JobYear = Text(r["JobYear"]),
            Address = Text(r["Address"]),
            Phone = Text(r["Phone"]),
            Email = Text(r["Email"])
        });

        dto.Skills = await QueryListAsync(session, """
SELECT TeacherSkillID, ISNULL(SkilName, N'') AS SkilName, ISNULL(Description, N'') AS Description
FROM dbo.Teacher_Skill
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID
""", ct, r => new TeacherPortalCvSkillDto
        {
            Id = ToInt(r["TeacherSkillID"]),
            Name = Text(r["SkilName"]),
            Description = Text(r["Description"])
        });

        dto.Languages = await QueryListAsync(session, """
SELECT TeacherLanguageID, ISNULL(LanguageName, N'') AS LanguageName, ISNULL(Level, N'') AS Level
FROM dbo.Teacher_Language
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID
""", ct, r => new TeacherPortalCvLanguageDto
        {
            Id = ToInt(r["TeacherLanguageID"]),
            Name = Text(r["LanguageName"]),
            Level = Text(r["Level"])
        });

        return dto;
    }

    public Task<StudentInfoResult> AddCvObjectiveAsync(SessionSnapshot session, TeacherPortalCvTextRequest? request, CancellationToken ct) =>
        ExecInsertAsync(session, """
INSERT INTO dbo.Teacher_Career_Objective (SchoolID, TeacherID, CareerObjective)
VALUES (@SchoolID, @TeacherID, @Text)
""", ct, cmd => cmd.Parameters.AddWithValue("@Text", (request?.Text ?? "").Trim()), (request?.Text ?? "").Trim());

    public Task<StudentInfoResult> AddCvEducationAsync(SessionSnapshot session, TeacherPortalCvEducationRequest? request, CancellationToken ct) =>
        ExecInsertAsync(session, """
INSERT INTO dbo.Teacher_EducationInfo (SchoolID, TeacherID, InstitutionName, ExamName, ExamYear, Result)
VALUES (@SchoolID, @TeacherID, @InstitutionName, @ExamName, @ExamYear, @Result)
""", ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@InstitutionName", (request?.InstitutionName ?? "").Trim());
            cmd.Parameters.AddWithValue("@ExamName", (request?.ExamName ?? "").Trim());
            cmd.Parameters.AddWithValue("@ExamYear", (request?.ExamYear ?? "").Trim());
            cmd.Parameters.AddWithValue("@Result", (request?.Result ?? "").Trim());
        }, request?.InstitutionName);

    public Task<StudentInfoResult> AddCvJobAsync(SessionSnapshot session, TeacherPortalCvJobRequest? request, CancellationToken ct) =>
        ExecInsertAsync(session, """
INSERT INTO dbo.Teacher_JobInfo (SchoolID, TeacherID, InstitutionName, Position, JobType, JobYear, Address, Phone, Email)
VALUES (@SchoolID, @TeacherID, @InstitutionName, @Position, @JobType, @JobYear, @Address, @Phone, @Email)
""", ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@InstitutionName", (request?.InstitutionName ?? "").Trim());
            cmd.Parameters.AddWithValue("@Position", (request?.Position ?? "").Trim());
            cmd.Parameters.AddWithValue("@JobType", (request?.JobType ?? "").Trim());
            cmd.Parameters.AddWithValue("@JobYear", (request?.JobYear ?? "").Trim());
            cmd.Parameters.AddWithValue("@Address", (request?.Address ?? "").Trim());
            cmd.Parameters.AddWithValue("@Phone", (request?.Phone ?? "").Trim());
            cmd.Parameters.AddWithValue("@Email", (request?.Email ?? "").Trim());
        }, request?.InstitutionName);

    public Task<StudentInfoResult> AddCvSkillAsync(SessionSnapshot session, TeacherPortalCvSkillRequest? request, CancellationToken ct) =>
        ExecInsertAsync(session, """
INSERT INTO dbo.Teacher_Skill (SchoolID, TeacherID, SkilName, Description)
VALUES (@SchoolID, @TeacherID, @Name, @Description)
""", ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@Name", (request?.Name ?? "").Trim());
            cmd.Parameters.AddWithValue("@Description", (request?.Description ?? "").Trim());
        }, request?.Name);

    public Task<StudentInfoResult> AddCvLanguageAsync(SessionSnapshot session, TeacherPortalCvLanguageRequest? request, CancellationToken ct) =>
        ExecInsertAsync(session, """
INSERT INTO dbo.Teacher_Language (SchoolID, TeacherID, LanguageName, Level)
VALUES (@SchoolID, @TeacherID, @Name, @Level)
""", ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@Name", (request?.Name ?? "").Trim());
            cmd.Parameters.AddWithValue("@Level", (request?.Level ?? "").Trim());
        }, request?.Name);

    public Task<StudentInfoResult> DeleteCvItemAsync(SessionSnapshot session, string kind, int id, CancellationToken ct)
    {
        var sql = kind switch
        {
            "objective" => "DELETE FROM dbo.Teacher_Career_Objective WHERE CareerObjectiveID = @Id AND SchoolID = @SchoolID AND TeacherID = @TeacherID",
            "education" => "DELETE FROM dbo.Teacher_EducationInfo WHERE TeacherEducationID = @Id AND SchoolID = @SchoolID AND TeacherID = @TeacherID",
            "job" => "DELETE FROM dbo.Teacher_JobInfo WHERE TeacherJobID = @Id AND SchoolID = @SchoolID AND TeacherID = @TeacherID",
            "skill" => "DELETE FROM dbo.Teacher_Skill WHERE TeacherSkillID = @Id AND SchoolID = @SchoolID AND TeacherID = @TeacherID",
            "language" => "DELETE FROM dbo.Teacher_Language WHERE TeacherLanguageID = @Id AND SchoolID = @SchoolID AND TeacherID = @TeacherID",
            _ => ""
        };
        if (sql.Length == 0 || id <= 0)
            return Task.FromResult(new StudentInfoResult { Succeeded = false, Error = "tch.cvNeed" });
        return ExecInsertAsync(session, sql, ct, cmd => cmd.Parameters.AddWithValue("@Id", id), "ok");
    }

    private async Task<bool> IsMarksLockedAsync(SessionSnapshot session, int classId, int examId, CancellationToken ct)
    {
        if (!IsPortal(session) || classId <= 0 || examId <= 0)
            return false;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT Marks_Input_Locked FROM dbo.Exam_Publish_Setting
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
""", con);
            AddTeacher(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@ExamID", examId);
            var locked = await cmd.ExecuteScalarAsync(ct);
            return locked is true or 1 or (byte)1;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private async Task<List<ExamOptionDto>> QueryMarksOptionsAsync(
        SqlConnection con, SessionSnapshot session, string sql, CancellationToken ct, Action<SqlCommand>? extra = null)
    {
        var items = new List<ExamOptionDto>();
        await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 20 };
        AddTeacher(cmd, session);
        extra?.Invoke(cmd);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            items.Add(new ExamOptionDto { Id = ToInt(reader["Id"]), Name = Text(reader["Name"]) });
        return items;
    }

    private static object FilterId(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "0" ? DBNull.Value : value.Trim();

    private async Task<bool> OwnsSubjectAsync(SessionSnapshot session, int classId, int subjectId, CancellationToken ct)
    {
        if (!IsPortal(session) || classId <= 0 || subjectId <= 0)
            return false;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 1 FROM dbo.TecherSubject
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID AND ClassID = @ClassID AND SubjectID = @SubjectID
""", con);
            AddTeacher(cmd, session);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SubjectID", subjectId);
            return await cmd.ExecuteScalarAsync(ct) is not null and not DBNull;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private async Task<bool> CanTakeAttendanceOnAsync(SessionSnapshot session, DateTime date, CancellationToken ct)
    {
        if (date.Date == DateTime.Today)
            return true;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 Teacher_BackDate_Attendance FROM dbo.Account WHERE SchoolID = @SchoolID
""", con);
            AddTeacher(cmd, session);
            var value = await cmd.ExecuteScalarAsync(ct);
            return value is true or 1 or (byte)1;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private async Task<StudentInfoResult> ExecInsertAsync(
        SessionSnapshot session, string sql, CancellationToken ct, Action<SqlCommand> bind, string? required)
    {
        if (!IsPortal(session))
            return new StudentInfoResult { Succeeded = false, Error = "auth.forbidden" };
        if (string.IsNullOrWhiteSpace(required))
            return new StudentInfoResult { Succeeded = false, Error = "tch.cvNeed" };
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, con);
            AddTeacher(cmd, session);
            bind(cmd);
            await cmd.ExecuteNonQueryAsync(ct);
            return new StudentInfoResult { Succeeded = true };
        }
        catch (SqlException)
        {
            return new StudentInfoResult { Succeeded = false, Error = "tch.cvFailed" };
        }
    }

    private async Task<List<T>> QueryListAsync<T>(
        SessionSnapshot session, string sql, CancellationToken ct, Func<SqlDataReader, T> map)
    {
        var items = new List<T>();
        if (!IsPortal(session))
            return items;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 20 };
            AddTeacher(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                items.Add(map(reader));
        }
        catch (SqlException)
        {
        }

        return items;
    }

    private static bool IsPortal(SessionSnapshot session) =>
        session.IsTeacher && session.SchoolID > 0 && session.TeacherID > 0;

    private static void AddTeacher(SqlCommand cmd, SessionSnapshot session)
    {
        if (cmd.CommandText.Contains("@SchoolID", StringComparison.OrdinalIgnoreCase)
            && !cmd.Parameters.Contains("@SchoolID"))
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        if (cmd.CommandText.Contains("@EducationYearID", StringComparison.OrdinalIgnoreCase)
            && !cmd.Parameters.Contains("@EducationYearID"))
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (cmd.CommandText.Contains("@TeacherID", StringComparison.OrdinalIgnoreCase)
            && !cmd.Parameters.Contains("@TeacherID"))
            cmd.Parameters.AddWithValue("@TeacherID", session.TeacherID);
        if (cmd.CommandText.Contains("@RegistrationID", StringComparison.OrdinalIgnoreCase)
            && !cmd.Parameters.Contains("@RegistrationID"))
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        if (cmd.CommandText.Contains("@EmployeeID", StringComparison.OrdinalIgnoreCase)
            && !cmd.Parameters.Contains("@EmployeeID"))
            cmd.Parameters.AddWithValue("@EmployeeID", session.EmployeeID);
    }

    private static string Text(object? value) => Convert.ToString(value)?.Trim() ?? "";

    private static int ToInt(object? value)
    {
        if (value is null or DBNull)
            return 0;
        return Convert.ToInt32(value);
    }

    private static DateTime? Day(object? value)
    {
        if (value is null or DBNull)
            return null;
        if (value is DateTime date)
            return date.Date;
        return DateTime.TryParse(Convert.ToString(value), out var parsed) ? parsed.Date : null;
    }

    private static string? TimeText(object? value)
    {
        if (value is null or DBNull)
            return null;
        if (value is TimeSpan span)
            return DateTime.Today.Add(span).ToString("hh:mm tt");
        if (value is DateTime date)
            return date.ToString("hh:mm tt");
        var text = Convert.ToString(value)?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string FormatRange(object? start, object? end)
    {
        var a = TimeText(start);
        var b = TimeText(end);
        if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b))
            return "";
        if (string.IsNullOrWhiteSpace(b))
            return a ?? "";
        if (string.IsNullOrWhiteSpace(a))
            return b ?? "";
        return $"{a} - {b}";
    }

    private static void ApplyAttendanceSummary(TeacherPortalDashboardDto dto, StudentPortalAttendanceDto att)
    {
        var today = DhakaToday();
        var holiday = att.Holidays.FirstOrDefault(x => x.Date.Date == today);
        if (holiday is not null)
            dto.TodayHoliday = holiday.Name;

        var day = att.Days
            .Where(x => x.Date.Date == today)
            .LastOrDefault();
        if (day is not null)
        {
            dto.TodayStatus = day.Status;
            dto.TodayEntry = day.EntryTime;
            dto.TodayExit = day.ExitTime;
        }

        foreach (var row in att.Days.Where(x => x.Date.Year == today.Year && x.Date.Month == today.Month))
        {
            var key = (row.Status ?? "").Trim();
            if (key.Equals("Pre", StringComparison.OrdinalIgnoreCase) || key.Equals("Present", StringComparison.OrdinalIgnoreCase))
                dto.MonthPresent++;
            else if (key.Equals("Late", StringComparison.OrdinalIgnoreCase))
                dto.MonthLate++;
            else if (key.Equals("Leave", StringComparison.OrdinalIgnoreCase))
                dto.MonthLeave++;
            else if (key.Length > 0)
                dto.MonthAbsent++;
        }

        dto.MonthTotal = dto.MonthPresent + dto.MonthLate + dto.MonthAbsent + dto.MonthLeave;
        var counted = dto.MonthPresent + dto.MonthLate + dto.MonthAbsent;
        dto.MonthPct = counted <= 0
            ? 0
            : Math.Round(100m * (dto.MonthPresent + dto.MonthLate) / counted, 1);
    }

    private static DateTime DhakaToday()
    {
        foreach (var id in new[] { "Bangladesh Standard Time", "Asia/Dhaka" })
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(id)).Date;
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }
        return DateTime.Today;
    }

    private static string TodayKey() => NormalizeDay(DhakaToday().DayOfWeek.ToString());

    private static string NormalizeDay(string? raw)
    {
        var text = (raw ?? "").Trim();
        if (text.Length >= 3)
        {
            var key = char.ToUpperInvariant(text[0]) + text[1..3].ToLowerInvariant();
            if (key is "Sat" or "Sun" or "Mon" or "Tue" or "Wed" or "Thu" or "Fri")
                return key;
        }
        return text.ToLowerInvariant() switch
        {
            "saturday" or "শনি" or "শনিবার" => "Sat",
            "sunday" or "রবি" or "রবিবার" => "Sun",
            "monday" or "সোম" or "সোমবার" => "Mon",
            "tuesday" or "মঙ্গল" or "মঙ্গলবার" => "Tue",
            "wednesday" or "বুধ" or "বুধবার" => "Wed",
            "thursday" or "বৃহ" or "বৃহস্পতি" or "বৃহস্পতিবার" => "Thu",
            "friday" or "শুক্র" or "শুক্রবার" => "Fri",
            _ => text
        };
    }

    private static string? ToDataUrl(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 ? "image/png"
            : bytes.Length >= 6 && bytes[0] == 0x47 && bytes[1] == 0x49 ? "image/gif"
            : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
