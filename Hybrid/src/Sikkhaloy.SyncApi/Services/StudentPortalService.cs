using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class StudentPortalService
{
    private readonly EduConnectionFactory _connections;
    private readonly AccountsService _accounts;
    private readonly IHttpClientFactory _http;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IConfiguration _config;

    public StudentPortalService(
        EduConnectionFactory connections,
        AccountsService accounts,
        IHttpClientFactory http,
        IHttpContextAccessor httpContext,
        IConfiguration config)
    {
        _connections = connections;
        _accounts = accounts;
        _http = http;
        _httpContext = httpContext;
        _config = config;
    }

    public async Task<StudentPortalDashboardDto> GetDashboardAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalDashboardDto
        {
            StudentsName = session.DisplayName,
            StudentCode = session.StudentCode,
            ClassName = session.ClassName,
            SectionName = session.SectionName,
            SchoolName = session.SchoolName
        };
        if (!IsPortal(session))
            return dto;

        var headerTask = LoadHeaderAsync(session, ct);
        var photoTask = LoadPhotoAsync(session, ct);
        var statsTask = LoadStatsAsync(session, ct);
        var subjectsTask = LoadSubjectsAsync(session, ct);
        var attendanceTask = LoadAttendanceCountsAsync(session, ct);
        var dueTask = LoadDueAsync(session, ct);
        var examsTask = LoadUpcomingExamsAsync(session, ct);
        var routineTask = LoadTodayRoutineAsync(session, ct);
        var noticesTask = LoadNoticesAsync(session, ct, 3);
        var classSizeTask = LoadClassSizeAsync(session, ct);

        await Task.WhenAll(headerTask, photoTask, statsTask, subjectsTask, attendanceTask,
            dueTask, examsTask, routineTask, noticesTask, classSizeTask);

        var header = headerTask.Result;
        dto.StudentsName = string.IsNullOrWhiteSpace(header.Name) ? dto.StudentsName : header.Name;
        dto.StudentCode = string.IsNullOrWhiteSpace(header.Code) ? dto.StudentCode : header.Code;
        dto.ClassName = string.IsNullOrWhiteSpace(header.ClassName) ? dto.ClassName : header.ClassName;
        dto.SectionName = string.IsNullOrWhiteSpace(header.SectionName) ? dto.SectionName : header.SectionName;
        dto.YearName = header.YearName;
        dto.PhotoDataUrl = photoTask.Result;

        var stats = statsTask.Result;
        dto.AvgMarks = stats.AvgMarks;
        dto.AvgPoint = stats.AvgPoint;
        dto.AvgPosition = stats.AvgPosition;
        dto.PassPct = stats.PassPct;

        dto.Subjects = subjectsTask.Result;
        dto.Attendance = attendanceTask.Result;
        dto.PresentDays = dto.Attendance.Where(IsPresent).Sum(x => x.Count);
        dto.AbsentDays = dto.Attendance.Where(x => !IsPresent(x)).Sum(x => x.Count);
        dto.AttendanceTotal = dto.PresentDays + dto.AbsentDays;
        dto.AttendancePct = dto.AttendanceTotal <= 0
            ? 0
            : Math.Round(100m * dto.PresentDays / dto.AttendanceTotal, 2);
        dto.CurrentDue = dueTask.Result;
        dto.UpcomingExams = examsTask.Result;
        dto.TodayRoutine = routineTask.Result;
        dto.Notices = noticesTask.Result;
        dto.ClassSize = classSizeTask.Result;
        return dto;
    }

    public async Task<StudentPortalDetailsDto> GetDetailsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalDetailsDto();
        if (!IsPortal(session))
            return dto;

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1
    Student.StudentsName, Student.ID, Student.Gender, Student.SMSPhoneNo, Student.DateofBirth,
    Student.BloodGroup, Student.Religion, Student.StudentPermanentAddress, Student.StudentsLocalAddress,
    Student.StudentEmailAddress, Student.FathersName, Student.MothersName, Student.GuardianName,
    Student.GuardianRelationshipwithStudent, Student_Image.Image
FROM dbo.Student
LEFT JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE Student.StudentID = @StudentID AND Student.SchoolID = @SchoolID
""", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return dto;

            dto.StudentsName = Text(reader["StudentsName"]);
            dto.StudentCode = Text(reader["ID"]);
            dto.Gender = Text(reader["Gender"]);
            dto.Phone = Text(reader["SMSPhoneNo"]);
            dto.DateOfBirth = Day(reader["DateofBirth"]);
            dto.BloodGroup = Text(reader["BloodGroup"]);
            dto.Religion = Text(reader["Religion"]);
            dto.PermanentAddress = Text(reader["StudentPermanentAddress"]);
            dto.PresentAddress = Text(reader["StudentsLocalAddress"]);
            dto.Email = Text(reader["StudentEmailAddress"]);
            dto.FathersName = Text(reader["FathersName"]);
            dto.MothersName = Text(reader["MothersName"]);
            dto.GuardianName = Text(reader["GuardianName"]);
            dto.GuardianRelation = Text(reader["GuardianRelationshipwithStudent"]);
            dto.PhotoDataUrl = ToDataUrl(reader["Image"] as byte[]);
        }
        catch (SqlException)
        {
        }

        return dto;
    }

    public async Task<List<EducationYearDto>> GetSessionsAsync(SessionSnapshot session, CancellationToken ct) =>
        await QueryListAsync(session, """
SELECT Education_Year.EducationYearID, Education_Year.EducationYear, Education_Year.StartDate, Education_Year.EndDate
FROM dbo.StudentsClass
INNER JOIN dbo.Education_Year ON Education_Year.EducationYearID = StudentsClass.EducationYearID
WHERE StudentsClass.StudentID = @StudentID
  AND StudentsClass.SchoolID = @SchoolID
GROUP BY Education_Year.EducationYearID, Education_Year.EducationYear, Education_Year.StartDate, Education_Year.EndDate
ORDER BY Education_Year.StartDate DESC
""", ct, r => new EducationYearDto
        {
            EducationYearID = ToInt(r["EducationYearID"]),
            Name = Text(r["EducationYear"]),
            StartDate = Day(r["StartDate"]),
            EndDate = Day(r["EndDate"]),
            IsCurrent = ToInt(r["EducationYearID"]) == session.EducationYearID
        });

    public async Task<StudentPortalAttendanceDto> GetAttendanceAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalAttendanceDto();
        if (!IsPortal(session))
            return dto;

        dto.Days = await QueryListAsync(session, """
SELECT CAST(AttendanceDate AS date) AS AttendanceDate, Attendance, EntryTime, ExitTime
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID
  AND EducationYearID = @EducationYearID
  AND StudentClassID = @StudentClassID
ORDER BY AttendanceDate
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

        dto.Leaves = await QueryListAsync(session, """
SELECT CAST(StartDate AS date) AS StartDate, CAST(EndDate AS date) AS EndDate,
       ISNULL(LeaveType, N'') AS LeaveType, ISNULL(Description, N'') AS Description
FROM dbo.Attendance_Leave
WHERE SchoolID = @SchoolID AND StudentID = @StudentID
""", ct, r => new StudentPortalLeaveDto
        {
            StartDate = Day(r["StartDate"]) ?? DateTime.MinValue,
            EndDate = Day(r["EndDate"]) ?? DateTime.MinValue,
            LeaveType = Text(r["LeaveType"]),
            Description = Text(r["Description"])
        });

        return dto;
    }

    public Task<List<StudentPortalSmsDto>> GetSmsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT TOP 80 SMS_Send_Record.PhoneNumber, SMS_Send_Record.TextSMS, SMS_Send_Record.PurposeOfSMS, SMS_Send_Record.Date
FROM dbo.SMS_Send_Record
INNER JOIN dbo.SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID
WHERE SMS_OtherInfo.SchoolID = @SchoolID
  AND SMS_OtherInfo.StudentID = @StudentID
  AND SMS_OtherInfo.EducationYearID = @EducationYearID
ORDER BY SMS_Send_Record.Date DESC
""", ct, r => new StudentPortalSmsDto
        {
            Phone = Text(r["PhoneNumber"]),
            Text = Text(r["TextSMS"]),
            Purpose = Text(r["PurposeOfSMS"]),
            Date = Day(r["Date"])
        });

    public Task<List<StudentPortalAccountRowDto>> GetAccountsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.Amount, Income_PayOrder.PaidAmount,
       CASE WHEN Income_PayOrder.EndDate < GETDATE() - 1
            THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0)
               - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0)
               - ISNULL(Income_PayOrder.LateFee_Discount, 0)
            ELSE ISNULL(Income_PayOrder.Amount, 0) - ISNULL(Income_PayOrder.Discount, 0)
               - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due,
       Income_PayOrder.EndDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Is_Active = 1
ORDER BY Income_PayOrder.EndDate DESC
""", ct, r => new StudentPortalAccountRowDto
        {
            Role = Text(r["Role"]),
            PayFor = Text(r["PayFor"]),
            Amount = Dec(r["Amount"]),
            Paid = Dec(r["PaidAmount"]),
            Due = Dec(r["Due"]),
            EndDate = Day(r["EndDate"])
        });

    public Task<List<StudentPortalNoticeDto>> GetNoticesAsync(SessionSnapshot session, CancellationToken ct) =>
        LoadNoticesAsync(session, ct, 80);

    public Task<List<StudentPortalExamDto>> GetExamsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Exam_Name.ExamName, Exam_Result_of_Student.Student_Point, Exam_Result_of_Student.Student_Grade,
       Exam_Name.Period_StartDate
FROM dbo.Exam_Result_of_Student
INNER JOIN dbo.Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID
INNER JOIN dbo.Exam_Publish_Setting ON Exam_Result_of_Student.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID
WHERE Exam_Result_of_Student.StudentID = @StudentID
  AND Exam_Result_of_Student.EducationYearID = @EducationYearID
  AND Exam_Publish_Setting.IS_Published = 1
ORDER BY Exam_Name.Period_StartDate
""", ct, r => new StudentPortalExamDto
        {
            Name = Text(r["ExamName"]),
            Point = Dec(r["Student_Point"]),
            Grade = Text(r["Student_Grade"]),
            Date = Day(r["Period_StartDate"])
        });

    public Task<List<StudentPortalExamDto>> GetCumulativeAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Student.Student_Point,
       Exam_Cumulative_Student.Student_Grade
FROM dbo.Exam_Cumulative_Student
INNER JOIN dbo.Exam_Cumulative_Name ON Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID
INNER JOIN dbo.Exam_Cumulative_Setting ON Exam_Cumulative_Student.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID
WHERE Exam_Cumulative_Student.StudentID = @StudentID
  AND Exam_Cumulative_Student.EducationYearID = @EducationYearID
  AND Exam_Cumulative_Setting.IS_Published = 1
""", ct, r => new StudentPortalExamDto
        {
            Name = Text(r["CumulativeResultName"]),
            Point = Dec(r["Student_Point"]),
            Grade = Text(r["Student_Grade"])
        });

    public Task<List<StudentPortalPeriodDto>> GetRoutineAsync(SessionSnapshot session, CancellationToken ct) =>
        LoadTodayRoutineAsync(session, ct);

    public Task<List<StudentPortalExamDto>> GetUpcomingExamsAsync(SessionSnapshot session, CancellationToken ct) =>
        LoadUpcomingExamsAsync(session, ct);

    public async Task<List<StudentPortalFaultReportDto>> GetFaultReportsAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
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
SELECT sf.StudentFaultID, sf.Fault_Title, sf.Fault, sf.Fault_Date, ISNULL(r.UserName, N'') AS UserName
FROM dbo.Student_Fault AS sf
LEFT JOIN dbo.Registration AS r ON sf.RegistrationId = r.RegistrationID
WHERE sf.SchoolID = @SchoolID
  AND sf.EducationYearID = @EducationYearID
  AND sf.StudentClassID = @StudentClassID
  AND CAST(sf.Fault_Date AS date) BETWEEN @FromDate AND @ToDate
ORDER BY sf.Fault_Date DESC, sf.StudentFaultID DESC
""", con) { CommandTimeout = 20 };
            AddStudent(cmd, session);
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
                    PostBy = Text(reader["UserName"])
                });
            }
        }
        catch (SqlException)
        {
        }

        return items;
    }

    private async Task<(string Name, string Code, string ClassName, string SectionName, string YearName)> LoadHeaderAsync(
        SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 Student.StudentsName, Student.ID, ISNULL(CreateClass.Class, N'') AS ClassName,
       ISNULL(CreateSection.Section, N'') AS SectionName, ISNULL(Education_Year.EducationYear, N'') AS YearName
FROM dbo.Student
INNER JOIN dbo.StudentsClass ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.CreateClass ON CreateClass.ClassID = StudentsClass.ClassID
LEFT JOIN dbo.CreateSection ON CreateSection.SectionID = StudentsClass.SectionID
LEFT JOIN dbo.Education_Year ON Education_Year.EducationYearID = StudentsClass.EducationYearID
WHERE Student.StudentID = @StudentID AND StudentsClass.StudentClassID = @StudentClassID
""", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return (session.DisplayName, session.StudentCode, session.ClassName, session.SectionName, "");
            return (Text(reader["StudentsName"]), Text(reader["ID"]), Text(reader["ClassName"]),
                Text(reader["SectionName"]), Text(reader["YearName"]));
        }
        catch (SqlException)
        {
            return (session.DisplayName, session.StudentCode, session.ClassName, session.SectionName, "");
        }
    }

    private async Task<string?> LoadPhotoAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 Student_Image.Image
FROM dbo.Student
LEFT JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE Student.StudentID = @StudentID AND Student.SchoolID = @SchoolID
""", con);
            AddStudent(cmd, session);
            var value = await cmd.ExecuteScalarAsync(ct);
            return ToDataUrl(value as byte[]);
        }
        catch (SqlException)
        {
            return null;
        }
    }

    private async Task<(decimal AvgMarks, decimal AvgPoint, decimal AvgPosition, decimal PassPct)> LoadStatsAsync(
        SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT AVG(CAST(Position_InExam_Class AS int)) AS Average_Position_Class,
       ROUND(AVG(Student_Point), 2, 0) AS Average_Point,
       (SELECT ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0)
        FROM dbo.Exam_Result_of_Subject
        INNER JOIN dbo.Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
        WHERE Exam_Result_of_Student.StudentPublishStatus = N'Pub'
          AND Exam_Result_of_Student.StudentID = @StudentID
          AND Exam_Result_of_Student.EducationYearID = @EducationYearID) AS Average_ObtainedMarkofSubject,
       (SELECT ROUND(100 * SUM(CASE WHEN t.PassStatus_Student = 'P' THEN 1 ELSE 0 END) / NULLIF(COUNT(t.StudentID), 0), 2, 0)
        FROM (
            SELECT StudentID, PassStatus_Student FROM dbo.Exam_Result_of_Student
            WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID AND EducationYearID = @EducationYearID
            UNION ALL
            SELECT StudentID, PassStatus_Student FROM dbo.Exam_Cumulative_Student
            WHERE StudentID = @StudentID AND EducationYearID = @EducationYearID
        ) AS t) AS Success_Percentage
FROM dbo.Exam_Result_of_Student
WHERE StudentPublishStatus = N'Pub' AND StudentID = @StudentID AND EducationYearID = @EducationYearID
""", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return (0, 0, 0, 0);
            return (Dec(reader["Average_ObtainedMarkofSubject"]), Dec(reader["Average_Point"]),
                Dec(reader["Average_Position_Class"]), Dec(reader["Success_Percentage"]));
        }
        catch (SqlException)
        {
            return (0, 0, 0, 0);
        }
    }

    private Task<List<StudentPortalSubjectDto>> LoadSubjectsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Subject.SubjectName,
       ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Sub_Avg
FROM dbo.Exam_Result_of_Subject
INNER JOIN dbo.Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID
INNER JOIN dbo.Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE Exam_Result_of_Subject.StudentID = @StudentID
  AND Exam_Result_of_Student.StudentPublishStatus = N'Pub'
  AND Exam_Result_of_Student.EducationYearID = @EducationYearID
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN
ORDER BY Sub_Avg DESC
""", ct, r =>
        {
            var avg = Dec(r["Sub_Avg"]);
            return new StudentPortalSubjectDto
            {
                Name = Text(r["SubjectName"]),
                Avg = avg,
                Grade = GradeFromPct(avg)
            };
        });

    private Task<List<StudentPortalCountDto>> LoadAttendanceCountsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Attendance AS Name, COUNT(StudentClassID) AS Total
FROM dbo.Attendance_Record
WHERE SchoolID = @SchoolID AND StudentClassID = @StudentClassID
  AND EducationYearID = @EducationYearID
GROUP BY Attendance
""", ct, r => new StudentPortalCountDto
        {
            Name = Text(r["Name"]),
            Count = ToInt(r["Total"])
        });

    private async Task<decimal> LoadDueAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT SUM(ISNULL(Receivable_Amount, 0))
FROM dbo.Income_PayOrder
WHERE EndDate < GETDATE() AND StudentID = @StudentID AND Is_Active = 1
  AND EducationYearID = @EducationYearID
""", con);
            AddStudent(cmd, session);
            return Dec(await cmd.ExecuteScalarAsync(ct));
        }
        catch (SqlException)
        {
            return 0;
        }
    }

    private Task<List<StudentPortalExamDto>> LoadUpcomingExamsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT TOP 8 ExamName, Period_StartDate
FROM dbo.Exam_Name
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
  AND Period_StartDate >= CAST(GETDATE() AS DATE)
ORDER BY Period_StartDate
""", ct, r => new StudentPortalExamDto
        {
            Name = Text(r["ExamName"]),
            Date = Day(r["Period_StartDate"])
        });

    private Task<List<StudentPortalPeriodDto>> LoadTodayRoutineAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT RoutineTime.RoutinePeriod,
       ISNULL(Subject.SubjectName, RoutineTime.RoutinePeriod) AS SubjectName,
       CONVERT(varchar(15), CAST(RoutineTime.StartTime AS TIME), 100) + N' - ' +
       CONVERT(varchar(15), CAST(RoutineTime.EndTime AS TIME), 100) AS TimeSlot
FROM dbo.RoutineForClass
INNER JOIN dbo.StudentsClass ON RoutineForClass.ClassID = StudentsClass.ClassID
  AND RoutineForClass.SectionID = StudentsClass.SectionID
  AND RoutineForClass.ShiftID = StudentsClass.ShiftID
  AND RoutineForClass.SubjectGroupID = StudentsClass.SubjectGroupID
INNER JOIN dbo.RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID
LEFT JOIN dbo.Subject ON RoutineForClass.SubjectID = Subject.SubjectID
WHERE StudentsClass.StudentClassID = @StudentClassID
  AND RoutineForClass.SchoolID = @SchoolID
  AND RoutineForClass.EducationYearID = @EducationYearID
  AND RoutineForClass.Day = DATENAME(WEEKDAY, GETDATE())
ORDER BY RoutineTime.StartTime
""", ct, r => new StudentPortalPeriodDto
        {
            Period = Text(r["RoutinePeriod"]),
            Subject = Text(r["SubjectName"]),
            Time = Text(r["TimeSlot"])
        });

    private Task<List<StudentPortalNoticeDto>> LoadNoticesAsync(SessionSnapshot session, CancellationToken ct, int take) =>
        QueryListAsync(session, $"""
SELECT TOP ({take}) StudentNotice.NoticeTitle, StudentNotice.Notice, StudentNotice.InsertDate, StudentNotice.IsHomeWork
FROM dbo.StudentNoticeClass
INNER JOIN dbo.StudentNotice ON StudentNoticeClass.StudentNoticeId = StudentNotice.StudentNoticeId
INNER JOIN dbo.StudentsClass ON StudentNoticeClass.ClassId = StudentsClass.ClassID
  AND StudentNotice.EducationYearId = StudentsClass.EducationYearID
WHERE StudentNotice.EducationYearId = @EducationYearID
  AND StudentNotice.SchoolId = @SchoolID
  AND StudentsClass.StudentClassID = @StudentClassID
ORDER BY StudentNotice.InsertDate DESC
""", ct, r => new StudentPortalNoticeDto
        {
            Title = Text(r["NoticeTitle"]),
            Body = Text(r["Notice"]),
            Date = Day(r["InsertDate"]),
            IsHomeWork = AsBool(r["IsHomeWork"])
        });

    private async Task<int> LoadClassSizeAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT COUNT(*)
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON Student.StudentID = StudentsClass.StudentID
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.ClassID = @ClassID
  AND Student.Status = N'Active'
""", con);
            AddStudent(cmd, session);
            return ToInt(await cmd.ExecuteScalarAsync(ct));
        }
        catch (SqlException)
        {
            return 0;
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
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                items.Add(map(reader));
        }
        catch (SqlException)
        {
        }
        return items;
    }

    private static void AddStudent(SqlCommand cmd, SessionSnapshot session)
    {
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@StudentID", session.StudentID);
        cmd.Parameters.AddWithValue("@StudentClassID", session.StudentClassID);
        cmd.Parameters.AddWithValue("@ClassID", session.ClassID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
    }

    private static bool IsPortal(SessionSnapshot session) =>
        session.IsStudent && session.StudentID > 0 && session.SchoolID > 0;

    private static bool IsPresent(StudentPortalCountDto row)
    {
        var name = row.Name.Trim();
        return name.Equals("Pre", StringComparison.OrdinalIgnoreCase)
               || name.Equals("Present", StringComparison.OrdinalIgnoreCase)
               || name.Equals("Late", StringComparison.OrdinalIgnoreCase);
    }

    private static string GradeFromPct(decimal pct) =>
        pct >= 80 ? "A+" : pct >= 70 ? "A" : pct >= 60 ? "A-" : pct >= 50 ? "B" : pct >= 40 ? "C" : pct >= 33 ? "D" : "F";

    private static int ToInt(object? value)
    {
        if (value is null or DBNull) return 0;
        if (value is bool flag) return flag ? 1 : 0;
        return Convert.ToInt32(value);
    }

    private static bool AsBool(object? value) =>
        value is bool flag ? flag : ToInt(value) == 1;

    private static decimal Dec(object? value) => value is null or DBNull ? 0 : Convert.ToDecimal(value);
    private static string Text(object? value) => value is null or DBNull ? "" : Convert.ToString(value)?.Trim() ?? "";
    private static DateTime? Day(object? value) => value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);

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
