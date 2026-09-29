using System.Text.Json;
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
        var schoolTask = LoadSchoolAsync(session, ct);
        var photoTask = LoadPhotoAsync(session, ct);
        var statsTask = LoadStatsAsync(session, ct);
        var subjectsTask = LoadSubjectsAsync(session, ct);
        var attendanceTask = LoadAttendanceCountsAsync(session, ct);
        var dueTask = LoadDueAsync(session, ct);
        var examsTask = LoadExamRoutineAsync(session, false, ct);
        var routineTask = LoadTodayRoutineAsync(session, ct);
        var noticesTask = LoadNoticesAsync(session, ct, 3);
        var classSizeTask = LoadClassSizeAsync(session, ct);

        await Task.WhenAll(headerTask, schoolTask, photoTask, statsTask, subjectsTask, attendanceTask,
            dueTask, examsTask, routineTask, noticesTask, classSizeTask);

        var header = headerTask.Result;
        var school = schoolTask.Result;
        dto.SchoolName = string.IsNullOrWhiteSpace(school.Name) ? dto.SchoolName : school.Name;
        dto.SchoolAddress = school.Address;
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
        dto.UpcomingExams = UpcomingOnly(examsTask.Result);
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

    public async Task<List<StudentPortalSmsDto>> GetSmsAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var items = new List<StudentPortalSmsDto>();
        if (!IsPortal(session))
            return items;

        var fromDate = from?.Date ?? new DateTime(1753, 1, 1);
        var toDate = to?.Date ?? new DateTime(9999, 12, 31);

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP (400) SMS_Send_Record.SMS_Send_ID, SMS_Send_Record.PhoneNumber, SMS_Send_Record.TextSMS,
       SMS_Send_Record.PurposeOfSMS, SMS_Send_Record.Date
FROM dbo.SMS_Send_Record
INNER JOIN dbo.SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID
WHERE SMS_OtherInfo.SchoolID = @SchoolID
  AND SMS_OtherInfo.StudentID = @StudentID
  AND SMS_OtherInfo.EducationYearID = @EducationYearID
  AND CAST(SMS_Send_Record.Date AS date) BETWEEN @FromDate AND @ToDate
ORDER BY SMS_Send_Record.Date DESC
""", con) { CommandTimeout = 20 };
            AddStudent(cmd, session);
            cmd.Parameters.AddWithValue("@FromDate", fromDate);
            cmd.Parameters.AddWithValue("@ToDate", toDate);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new StudentPortalSmsDto
                {
                    SmsSendId = ToInt(reader["SMS_Send_ID"]),
                    Phone = Text(reader["PhoneNumber"]),
                    Text = Text(reader["TextSMS"]),
                    Purpose = Text(reader["PurposeOfSMS"]),
                    Date = Day(reader["Date"])
                });
            }
        }
        catch (SqlException)
        {
        }

        return items;
    }

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

    public async Task<StudentPortalInboxReadsDto> GetInboxReadsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalInboxReadsDto();
        if (!IsPortal(session))
            return dto;

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureInboxTableAsync(con, ct);
            await using var cmd = new SqlCommand("""
SELECT NoticeReads, SmsReads, ReportReads
FROM dbo.Student_Portal_Inbox_Read
WHERE SchoolID = @SchoolID AND StudentID = @StudentID AND EducationYearID = @EducationYearID
""", con) { CommandTimeout = 20 };
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Notices = ParseKeys(reader["NoticeReads"]);
                dto.Sms = ParseKeys(reader["SmsReads"]);
                dto.Reports = ParseKeys(reader["ReportReads"]);
            }
        }
        catch (SqlException)
        {
        }

        return dto;
    }

    public async Task<StudentPortalInboxReadsDto> SaveInboxReadsAsync(
        SessionSnapshot session, StudentPortalInboxReadsDto incoming, CancellationToken ct)
    {
        incoming ??= new StudentPortalInboxReadsDto();
        var current = await GetInboxReadsAsync(session, ct);
        var merged = new StudentPortalInboxReadsDto
        {
            Notices = UnionKeys(current.Notices, incoming.Notices),
            Sms = UnionKeys(current.Sms, incoming.Sms),
            Reports = UnionKeys(current.Reports, incoming.Reports)
        };
        if (!IsPortal(session))
            return merged;

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureInboxTableAsync(con, ct);
            await using var cmd = new SqlCommand("""
UPDATE dbo.Student_Portal_Inbox_Read
SET NoticeReads = @NoticeReads, SmsReads = @SmsReads, ReportReads = @ReportReads, UpdatedAt = GETDATE()
WHERE SchoolID = @SchoolID AND StudentID = @StudentID AND EducationYearID = @EducationYearID;
IF @@ROWCOUNT = 0
    INSERT INTO dbo.Student_Portal_Inbox_Read
        (SchoolID, StudentID, EducationYearID, NoticeReads, SmsReads, ReportReads, UpdatedAt)
    VALUES
        (@SchoolID, @StudentID, @EducationYearID, @NoticeReads, @SmsReads, @ReportReads, GETDATE());
""", con) { CommandTimeout = 20 };
            AddStudent(cmd, session);
            cmd.Parameters.AddWithValue("@NoticeReads", JsonSerializer.Serialize(merged.Notices));
            cmd.Parameters.AddWithValue("@SmsReads", JsonSerializer.Serialize(merged.Sms));
            cmd.Parameters.AddWithValue("@ReportReads", JsonSerializer.Serialize(merged.Reports));
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException)
        {
        }

        return merged;
    }

    public Task<List<StudentPortalExamDto>> GetExamsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT ers.ExamID, en.ExamName, ers.Student_Point, ers.Student_Grade, en.Period_StartDate,
       ISNULL(ers.TotalExamObtainedMark_ofStudent, ers.ObtainedMark_ofStudent) AS ObtainedMarks,
       ISNULL(ers.TotalMark_ofStudent, 0) AS TotalMarks,
       ISNULL(ers.ObtainedPercentage_ofStudent, 0) AS Percentage,
       ISNULL(ers.Average, 0) AS Average,
       ISNULL(ers.Position_InExam_Class, N'') AS PositionClass,
       ISNULL(ers.Position_InExam_Subsection, N'') AS PositionSection,
       ISNULL(NULLIF(LTRIM(RTRIM(ers.Student_Comments)), N''), ISNULL(g.Comments, N'')) AS Comment
FROM dbo.Exam_Result_of_Student ers
INNER JOIN dbo.Exam_Name en ON ers.ExamID = en.ExamID
INNER JOIN dbo.Exam_Publish_Setting ON ers.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID
OUTER APPLY (
    SELECT TOP 1 egs.Comments
    FROM dbo.Exam_Grading_Assign ega
    INNER JOIN dbo.Exam_Grading_System egs
        ON egs.GradeNameID = ega.GradeNameID AND egs.SchoolID = ega.SchoolID
    WHERE ega.SchoolID = @SchoolID
      AND ega.EducationYearID = @EducationYearID
      AND ega.ClassID = ers.ClassID
      AND ega.ExamID = ers.ExamID
      AND egs.Grades = ers.Student_Grade
) g
WHERE ers.StudentID = @StudentID
  AND ers.EducationYearID = @EducationYearID
  AND Exam_Publish_Setting.IS_Published = 1
ORDER BY en.Period_StartDate
""", ct, r => new StudentPortalExamDto
        {
            Name = Text(r["ExamName"]),
            Point = Dec(r["Student_Point"]),
            Grade = Text(r["Student_Grade"]),
            Date = Day(r["Period_StartDate"]),
            ObtainedMarks = Dec(r["ObtainedMarks"]),
            TotalMarks = Dec(r["TotalMarks"]),
            Percentage = Dec(r["Percentage"]),
            Average = Dec(r["Average"]),
            PositionClass = Text(r["PositionClass"]),
            PositionSection = Text(r["PositionSection"]),
            Comment = Text(r["Comment"]),
            ExamID = ToInt(r["ExamID"])
        });

    public Task<List<StudentPortalExamSubjectDto>> GetExamSubjectsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT ers.ExamID, ISNULL(s.SubjectName, N'') AS SubjectName,
       ISNULL(sub.TotalExamObtainedMark_ofSubject, sub.ObtainedMark_ofSubject) AS ObtainedMarks,
       ISNULL(sub.TotalMark_ofSubject, 0) AS TotalMarks,
       ISNULL(sub.SubjectGrades, N'') AS Grade,
       ISNULL(sub.SubjectPoint, 0) AS Point,
       ISNULL(sub.SubjectType, N'') AS SubjectType
FROM dbo.Exam_Result_of_Subject sub
INNER JOIN dbo.Exam_Result_of_Student ers ON sub.StudentResultID = ers.StudentResultID
INNER JOIN dbo.Subject s ON sub.SubjectID = s.SubjectID
INNER JOIN dbo.Exam_Publish_Setting ps ON ers.Publish_SettingID = ps.Publish_SettingID
WHERE ers.StudentID = @StudentID
  AND ers.EducationYearID = @EducationYearID
  AND ps.IS_Published = 1
ORDER BY ers.ExamID, s.SubjectID
""", ct, r => new StudentPortalExamSubjectDto
        {
            ExamID = ToInt(r["ExamID"]),
            Subject = Text(r["SubjectName"]),
            ObtainedMarks = Dec(r["ObtainedMarks"]),
            TotalMarks = Dec(r["TotalMarks"]),
            Grade = Text(r["Grade"]),
            Point = Dec(r["Point"]),
            SubjectType = Text(r["SubjectType"])
        });

    public Task<List<StudentPortalExamDto>> GetCumulativeAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT ecs.CumulativeNameID AS ExamID, n.CumulativeResultName, ecs.Student_Point, ecs.Student_Grade,
       ISNULL(ecs.ObtainedMark_ofStudent, 0) AS ObtainedMarks,
       ISNULL(ecs.TotalMark_ofStudent, 0) AS TotalMarks,
       ISNULL(ecs.ObtainedPercentage_ofStudent, 0) AS Percentage,
       ISNULL(ecs.Average, 0) AS Average,
       ISNULL(ecs.Position_InExam_Class, N'') AS PositionClass,
       ISNULL(ecs.Position_InExam_Subsection, N'') AS PositionSection,
       ISNULL(NULLIF(LTRIM(RTRIM(ecs.Student_Comments)), N''), ISNULL(g.Comments, N'')) AS Comment
FROM dbo.Exam_Cumulative_Student ecs
INNER JOIN dbo.Exam_Cumulative_Name n ON ecs.CumulativeNameID = n.CumulativeNameID
INNER JOIN dbo.Exam_Cumulative_Setting cs ON ecs.Cumulative_SettingID = cs.Cumulative_SettingID
OUTER APPLY (
    SELECT TOP 1 egs.Comments
    FROM dbo.Exam_Grading_System egs
    WHERE egs.SchoolID = @SchoolID
      AND egs.GradeNameID = cs.GradeNameID
      AND egs.Grades = ecs.Student_Grade
) g
WHERE ecs.StudentID = @StudentID
  AND ecs.EducationYearID = @EducationYearID
  AND cs.IS_Published = 1
ORDER BY n.CumulativeNameID
""", ct, r => new StudentPortalExamDto
        {
            Name = Text(r["CumulativeResultName"]),
            Point = Dec(r["Student_Point"]),
            Grade = Text(r["Student_Grade"]),
            ObtainedMarks = Dec(r["ObtainedMarks"]),
            TotalMarks = Dec(r["TotalMarks"]),
            Percentage = Dec(r["Percentage"]),
            Average = Dec(r["Average"]),
            PositionClass = Text(r["PositionClass"]),
            PositionSection = Text(r["PositionSection"]),
            Comment = Text(r["Comment"]),
            ExamID = ToInt(r["ExamID"])
        });

    public Task<List<StudentPortalExamSubjectDto>> GetCumulativeSubjectsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT sub.CumulativeNameID AS ExamID, ISNULL(s.SubjectName, N'') AS SubjectName,
       ISNULL(sub.ObtainedMark_ofSubject, 0) AS ObtainedMarks,
       ISNULL(sub.TotalMark_ofSubject, 0) AS TotalMarks,
       ISNULL(sub.SubjectGrades, N'') AS Grade,
       ISNULL(sub.SubjectPoint, 0) AS Point,
       ISNULL(sub.SubjectType, N'') AS SubjectType
FROM dbo.Exam_Cumulative_Subject sub
INNER JOIN dbo.Subject s ON sub.SubjectID = s.SubjectID
INNER JOIN dbo.Exam_Cumulative_Setting cs ON sub.Cumulative_SettingID = cs.Cumulative_SettingID
WHERE sub.StudentID = @StudentID
  AND sub.EducationYearID = @EducationYearID
  AND cs.IS_Published = 1
ORDER BY sub.CumulativeNameID, s.SubjectID
""", ct, r => new StudentPortalExamSubjectDto
        {
            ExamID = ToInt(r["ExamID"]),
            Subject = Text(r["SubjectName"]),
            ObtainedMarks = Dec(r["ObtainedMarks"]),
            TotalMarks = Dec(r["TotalMarks"]),
            Grade = Text(r["Grade"]),
            Point = Dec(r["Point"]),
            SubjectType = Text(r["SubjectType"])
        });

    public Task<List<StudentPortalPeriodDto>> GetRoutineAsync(SessionSnapshot session, CancellationToken ct) =>
        LoadRoutineAsync(session, false, ct);

    public async Task<List<StudentPortalExamDto>> GetUpcomingExamsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var rows = await LoadExamRoutineAsync(session, false, ct);
        if (rows.Count == 0)
            rows = await LoadNamedExamsAsync(session, ct);
        return rows;
    }

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
SELECT sf.StudentFaultID, sf.Fault_Title, sf.Fault, sf.Fault_Date,
       COALESCE(NULLIF(t.Name, N''), NULLIF(a.Name, N''), ISNULL(r.UserName, N'')) AS UserName
FROM dbo.Student_Fault AS sf
LEFT JOIN dbo.Registration AS r ON sf.RegistrationId = r.RegistrationID
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

    private async Task<(string Name, string Address)> LoadSchoolAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 ISNULL(SchoolName, N'') AS SchoolName, ISNULL(Address, N'') AS Address
FROM dbo.SchoolInfo
WHERE SchoolID = @SchoolID
""", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return (session.SchoolName, "");
            return (Text(reader["SchoolName"]), Text(reader["Address"]));
        }
        catch (SqlException)
        {
            return (session.SchoolName, "");
        }
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

    private static List<StudentPortalExamDto> UpcomingOnly(List<StudentPortalExamDto> rows)
    {
        var today = DateTime.Today;
        return rows
            .Where(x => x.Date is null || x.Date.Value.Date >= today)
            .Take(8)
            .ToList();
    }

    private async Task<List<StudentPortalExamDto>> LoadExamRoutineAsync(SessionSnapshot session, bool upcomingOnly, CancellationToken ct)
    {
        var items = await QueryExamRoutineAsync(session, true, ct);
        if (items.Count == 0)
            items = await QueryExamRoutineAsync(session, false, ct);
        if (items.Count == 0)
            items = await LoadNamedExamsAsync(session, ct);
        return upcomingOnly ? UpcomingOnly(items) : items;
    }

    private Task<List<StudentPortalExamDto>> LoadNamedExamsAsync(SessionSnapshot session, CancellationToken ct) =>
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

    private async Task<List<StudentPortalExamDto>> QueryExamRoutineAsync(
        SessionSnapshot session, bool yearFilter, CancellationToken ct)
    {
        var items = new List<StudentPortalExamDto>();
        if (!IsPortal(session) || session.ClassID <= 0)
            return items;
        var yearSql = yearFilter
            ? " AND (d.EducationYearID = @EducationYearID OR d.EducationYearID IS NULL)"
            : "";
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand($"""
SELECT d.RoutineName, dates.examDate AS ExamDate, r.DayName, r.StartTime, r.EndTime, r.Duration, r.ExamTime,
       ISNULL(s.SubjectName, N'') AS SubjectName,
       ISNULL(cell.SubjectText, N'') AS ExtraInfo,
       ISNULL(cell.TimeText, N'') AS TimeText
FROM dbo.Exam_Routine_SavedData d
INNER JOIN dbo.Exam_Routine_ClassColumns col ON col.RoutineID = d.RoutineID AND col.ClassID = @ClassID
INNER JOIN dbo.Exam_Routine_Rows r ON r.RoutineID = d.RoutineID
LEFT JOIN dbo.Exam_Routine_CellData cell
    ON cell.RoutineID = d.RoutineID AND cell.RowIndex = r.RowIndex AND cell.ColumnIndex = col.ColumnIndex
LEFT JOIN dbo.Subject s ON s.SubjectID = cell.SubjectID
CROSS APPLY (
    SELECT COALESCE(
        TRY_CONVERT(date, r.ExamDate, 103),
        TRY_CONVERT(date, r.ExamDate, 101),
        TRY_CONVERT(date, r.ExamDate)) AS examDate
) dates
WHERE d.SchoolID = @SchoolID
  AND ISNULL(d.IsActive, 1) = 1
  AND (
        NULLIF(LTRIM(RTRIM(cell.SubjectText)), N'') IS NOT NULL
        OR ISNULL(cell.SubjectID, 0) > 0
      )
{yearSql}
ORDER BY examDate, r.RowIndex
""", con) { CommandTimeout = 20 };
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var subject = Text(reader["SubjectName"]);
                var extra = Text(reader["ExtraInfo"]);
                if (string.Equals(extra, subject, StringComparison.OrdinalIgnoreCase))
                    extra = "";
                var examName = Text(reader["RoutineName"]);
                var start = Text(reader["StartTime"]);
                var end = Text(reader["EndTime"]);
                var slot = Text(reader["ExamTime"]);
                if (slot.Length == 0 && (start.Length > 0 || end.Length > 0))
                    slot = (start + " - " + end).Trim(' ', '-');
                items.Add(new StudentPortalExamDto
                {
                    Name = examName.Length > 0 ? examName : subject,
                    Subject = subject,
                    ExtraInfo = extra,
                    CellTime = Text(reader["TimeText"]),
                    Comment = slot,
                    DayName = Text(reader["DayName"]),
                    Duration = Text(reader["Duration"]),
                    Date = Day(reader["ExamDate"])
                });
            }
        }
        catch (SqlException)
        {
        }
        return items;
    }

    private Task<List<StudentPortalPeriodDto>> LoadTodayRoutineAsync(SessionSnapshot session, CancellationToken ct) =>
        LoadRoutineAsync(session, true, ct);

    private async Task<List<StudentPortalPeriodDto>> LoadRoutineAsync(SessionSnapshot session, bool todayOnly, CancellationToken ct)
    {
        var items = await QueryRoutineAsync(session, true, ct);
        if (items.Count == 0)
            items = await QueryRoutineAsync(session, false, ct);
        if (todayOnly)
        {
            var today = TodayRoutineDay();
            items = items.Where(x => string.Equals(x.Day, today, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return items;
    }

    private async Task<List<StudentPortalPeriodDto>> QueryRoutineAsync(SessionSnapshot session, bool yearFilter, CancellationToken ct)
    {
        var items = new List<StudentPortalPeriodDto>();
        if (!IsPortal(session))
            return items;
        var yearSql = yearFilter
            ? " AND (c.EducationYearID = @EducationYearID OR c.EducationYearID = sc.EducationYearID)"
            : "";
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand($"""
SELECT c.Day,
       t.RoutinePeriod,
       t.StartTime,
       t.EndTime,
       ISNULL(s.SubjectName, t.RoutinePeriod) AS SubjectName,
       LTRIM(RTRIM(ISNULL(te.FirstName, N'') + N' ' + ISNULL(te.LastName, N''))) AS TeacherName
FROM dbo.RoutineForClass c
INNER JOIN dbo.StudentsClass sc ON sc.StudentClassID = @StudentClassID
  AND c.ClassID = sc.ClassID
  AND (ISNULL(c.SectionID, 0) IN (0, sc.SectionID) OR ISNULL(sc.SectionID, 0) = 0)
  AND (ISNULL(c.ShiftID, 0) IN (0, sc.ShiftID) OR ISNULL(sc.ShiftID, 0) = 0)
  AND (ISNULL(c.SubjectGroupID, 0) IN (0, sc.SubjectGroupID) OR ISNULL(sc.SubjectGroupID, 0) = 0)
INNER JOIN dbo.RoutineTime t ON t.RoutineTimeID = c.RoutineTimeID
  AND t.SchoolID = c.SchoolID AND t.RoutineInfoID = c.RoutineInfoID
LEFT JOIN dbo.Subject s ON s.SubjectID = c.SubjectID
LEFT JOIN dbo.Teacher te ON te.TeacherID = c.TeacherID
WHERE c.SchoolID = @SchoolID
{yearSql}
ORDER BY t.StartTime
""", con) { CommandTimeout = 20 };
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new StudentPortalPeriodDto
                {
                    Day = NormalizeRoutineDay(Text(reader["Day"])),
                    Period = Text(reader["RoutinePeriod"]),
                    Subject = Text(reader["SubjectName"]),
                    Teacher = Text(reader["TeacherName"]),
                    Time = FormatRoutineSlot(reader["StartTime"], reader["EndTime"])
                });
            }
        }
        catch (SqlException)
        {
        }
        return items;
    }

    private static string FormatRoutineSlot(object? start, object? end)
    {
        var a = FormatRoutineTime(start);
        var b = FormatRoutineTime(end);
        if (a.Length == 0 && b.Length == 0) return "";
        return a + " - " + b;
    }

    private static string FormatRoutineTime(object? value)
    {
        if (value is null or DBNull) return "";
        if (value is TimeSpan ts)
            return DateTime.Today.Add(ts).ToString("h:mm tt");
        if (DateTime.TryParse(value.ToString(), out var dt))
            return dt.ToString("h:mm tt");
        return value.ToString() ?? "";
    }

    private static string TodayRoutineDay() => NormalizeRoutineDay(DateTime.Today.DayOfWeek.ToString());

    private static string NormalizeRoutineDay(string? raw)
    {
        var text = (raw ?? "").Trim();
        if (text.Length >= 3)
        {
            var key = text[..3];
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

    private Task<List<StudentPortalNoticeDto>> LoadNoticesAsync(SessionSnapshot session, CancellationToken ct, int take) =>
        QueryListAsync(session, $"""
SELECT TOP ({take}) StudentNotice.StudentNoticeId, StudentNotice.NoticeTitle, StudentNotice.Notice,
       StudentNotice.InsertDate, StudentNotice.IsHomeWork
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
            NoticeId = ToInt(r["StudentNoticeId"]),
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

    private static async Task EnsureInboxTableAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.Student_Portal_Inbox_Read', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Inbox_Read (
        SchoolID int NOT NULL,
        StudentID int NOT NULL,
        EducationYearID int NOT NULL,
        NoticeReads nvarchar(max) NOT NULL CONSTRAINT DF_StudentPortalInbox_Notice DEFAULT (N'[]'),
        SmsReads nvarchar(max) NOT NULL CONSTRAINT DF_StudentPortalInbox_Sms DEFAULT (N'[]'),
        ReportReads nvarchar(max) NOT NULL CONSTRAINT DF_StudentPortalInbox_Report DEFAULT (N'[]'),
        UpdatedAt datetime NOT NULL CONSTRAINT DF_StudentPortalInbox_Updated DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Inbox_Read PRIMARY KEY (SchoolID, StudentID, EducationYearID)
    );
END
""", con) { CommandTimeout = 20 };
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static List<string> ParseKeys(object? value)
    {
        var text = Text(value);
        if (string.IsNullOrWhiteSpace(text))
            return [];
        try
        {
            return (JsonSerializer.Deserialize<List<string>>(text) ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<string> UnionKeys(IEnumerable<string>? left, IEnumerable<string>? right) =>
        (left ?? []).Concat(right ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .Take(4000)
            .ToList();

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
