using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class AttendanceService
{
    private static readonly string[] WeekDays =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] DefaultLeaveTypes =
    [
        "অসুস্থতার জন্য", "ব্যাক্তিগত কারনে", "ফ্যামেলি প্রয়োজনে",
        "মেডিক্যাল", "সাময়িক", "সাপ্তাহিক", "মাসিক", "অন্যান্ন"
    ];

    private readonly EduConnectionFactory _connections;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly PaymentSmsService _sms;

    public AttendanceService(
        EduConnectionFactory connections,
        IWebHostEnvironment env,
        IConfiguration config,
        PaymentSmsService sms)
    {
        _connections = connections;
        _env = env;
        _config = config;
        _sms = sms;
    }

    public async Task<IReadOnlyList<AttendanceScheduleDto>> ListSchedulesAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT ScheduleID, ScheduleName,
       CONVERT(varchar(8), StartTime, 108) AS StartTime,
       CONVERT(varchar(8), LateEntryTime, 108) AS LateEntryTime,
       CONVERT(varchar(8), EndTime, 108) AS EndTime
FROM dbo.Attendance_Schedule
WHERE SchoolID = @SchoolID
ORDER BY ScheduleName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<AttendanceScheduleDto>();
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new AttendanceScheduleDto
                {
                    ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                    ScheduleName = reader["ScheduleName"]?.ToString() ?? "",
                    StartTime = TrimTime(reader["StartTime"]),
                    LateEntryTime = TrimTime(reader["LateEntryTime"]),
                    EndTime = TrimTime(reader["EndTime"])
                });
            }
        }

        foreach (var item in items)
            item.Days.AddRange(await ListDaysAsync(con, session.SchoolID, item.ScheduleID, cancellationToken));
        return items;
    }

    public async Task<AttendanceResult> CreateScheduleAsync(
        SessionSnapshot session, SaveScheduleRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.ScheduleName ?? "").Trim();
        if (name.Length == 0 || request is null)
            return Fail("att.scheduleName");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Schedule (SchoolID, RegistrationID, ScheduleName, LateEntryTime, StartTime, EndTime, Date)
VALUES (@SchoolID, @RegistrationID, @ScheduleName, @Late, @Start, @End, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@ScheduleName", name);
        cmd.Parameters.AddWithValue("@Late", ParseTime(request.LateEntryTime));
        cmd.Parameters.AddWithValue("@Start", ParseTime(request.StartTime));
        cmd.Parameters.AddWithValue("@End", ParseTime(request.EndTime));
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        await EnsureDaysAsync(con, session, id, request, cancellationToken);
        return new AttendanceResult { Succeeded = true, Saved = 1 };
    }

    public async Task<AttendanceResult> RenameScheduleAsync(
        SessionSnapshot session, int scheduleId, SaveScheduleRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.ScheduleName ?? "").Trim();
        if (scheduleId <= 0 || name.Length == 0)
            return Fail("att.scheduleName");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Attendance_Schedule SET ScheduleName = @Name
WHERE ScheduleID = @ScheduleID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<AttendanceResult> DeleteScheduleAsync(
        SessionSnapshot session, int scheduleId, CancellationToken cancellationToken)
    {
        if (scheduleId <= 0)
            return Fail("att.needSchedule");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Employee_Attendance_Schedule_Assign WHERE SchoolID = @SchoolID AND ScheduleID = @ScheduleID;
DELETE FROM dbo.Attendance_Schedule_AssignStudent WHERE SchoolID = @SchoolID AND ScheduleID = @ScheduleID;
DELETE FROM dbo.Attendance_Schedule_Day WHERE SchoolID = @SchoolID AND ScheduleID = @ScheduleID;
DELETE FROM dbo.Attendance_Schedule WHERE SchoolID = @SchoolID AND ScheduleID = @ScheduleID;
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<AttendanceResult> SaveScheduleDaysAsync(
        SessionSnapshot session, SaveScheduleDaysRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ScheduleID <= 0)
            return Fail("att.needSchedule");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        foreach (var day in request.Days)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Attendance_Schedule_Day
SET LateEntryTime = @Late, StartTime = @Start, EndTime = @End, Is_OnDay = @On
WHERE ScheduleID = @ScheduleID AND SchoolID = @SchoolID AND Day = @Day
""", con);
            cmd.Parameters.AddWithValue("@Late", ParseTime(day.LateEntryTime));
            cmd.Parameters.AddWithValue("@Start", ParseTime(day.StartTime));
            cmd.Parameters.AddWithValue("@End", ParseTime(day.EndTime));
            cmd.Parameters.AddWithValue("@On", day.IsOnDay);
            cmd.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Day", day.Day);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        return Ok();
    }

    public async Task<AttendanceSettingsDto> GetSettingsAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT TOP 1 AttendanceSettingID, SettingKey, SMS_TimeOut_Minute,
       Is_Device_Attendance_Enable, Is_All_SMS_On, Is_Holiday_As_Offday, Is_English_SMS,
       Is_Student_Attendance_Enable, Is_Student_All_SMS_Active, Is_Student_Entry_SMS_ON,
       Is_Student_Exit_SMS_ON, Is_Student_Abs_SMS_ON, Is_Student_Late_SMS_ON,
       Is_Employee_Attendance_Enable, Is_Employee_SMS_Active, Is_Employee_Abs_SMS_ON,
       Is_Employee_Late_SMS_ON, Is_Employee_SMS_OwnNumber, Employee_SMS_Number
FROM dbo.Attendance_Device_Setting
WHERE SchoolID = @SchoolID AND IsActive = 1
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new AttendanceSettingsDto { DeviceAttendance = true, StudentAttendance = true, EmployeeAttendance = true, SmsTimeoutMinute = 30, EnglishSms = true };
        return new AttendanceSettingsDto
        {
            AttendanceSettingID = Convert.ToInt32(reader["AttendanceSettingID"]),
            SettingKey = reader["SettingKey"]?.ToString() ?? "",
            SmsTimeoutMinute = ToInt(reader["SMS_TimeOut_Minute"], 30),
            DeviceAttendance = ToBool(reader["Is_Device_Attendance_Enable"]),
            AllSms = ToBool(reader["Is_All_SMS_On"]),
            HolidayAsOffday = ToBool(reader["Is_Holiday_As_Offday"]),
            EnglishSms = ToBool(reader["Is_English_SMS"]),
            StudentAttendance = ToBool(reader["Is_Student_Attendance_Enable"]),
            StudentAllSms = ToBool(reader["Is_Student_All_SMS_Active"]),
            StudentEntrySms = ToBool(reader["Is_Student_Entry_SMS_ON"]),
            StudentExitSms = ToBool(reader["Is_Student_Exit_SMS_ON"]),
            StudentAbsSms = ToBool(reader["Is_Student_Abs_SMS_ON"]),
            StudentLateSms = ToBool(reader["Is_Student_Late_SMS_ON"]),
            EmployeeAttendance = ToBool(reader["Is_Employee_Attendance_Enable"]),
            EmployeeSms = ToBool(reader["Is_Employee_SMS_Active"]),
            EmployeeAbsSms = ToBool(reader["Is_Employee_Abs_SMS_ON"]),
            EmployeeLateSms = ToBool(reader["Is_Employee_Late_SMS_ON"]),
            EmployeeSmsOwnNumber = ToBool(reader["Is_Employee_SMS_OwnNumber"]),
            EmployeeSmsNumber = NullString(reader["Employee_SMS_Number"])
        };
    }

    public async Task<AttendanceResult> SaveSettingsAsync(
        SessionSnapshot session, AttendanceSettingsDto? request, CancellationToken cancellationToken)
    {
        if (request is null || request.AttendanceSettingID <= 0)
            return Fail("att.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Attendance_Device_Setting SET
    SettingKey = @SettingKey, Is_Device_Attendance_Enable = @Device, Is_All_SMS_On = @AllSms,
    Is_Holiday_As_Offday = @Holiday, Is_English_SMS = @English, SMS_TimeOut_Minute = @Timeout,
    Is_Student_Attendance_Enable = @StuAtt, Is_Student_All_SMS_Active = @StuSms,
    Is_Student_Entry_SMS_ON = @Entry, Is_Student_Exit_SMS_ON = @Exit,
    Is_Student_Abs_SMS_ON = @Abs, Is_Student_Late_SMS_ON = @Late,
    Is_Employee_Attendance_Enable = @EmpAtt, Is_Employee_SMS_Active = @EmpSms,
    Is_Employee_Abs_SMS_ON = @EmpAbs, Is_Employee_Late_SMS_ON = @EmpLate,
    Is_Employee_SMS_OwnNumber = @Own, Employee_SMS_Number = @EmpNo
WHERE AttendanceSettingID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SettingKey", request.SettingKey ?? "");
        cmd.Parameters.AddWithValue("@Device", request.DeviceAttendance);
        cmd.Parameters.AddWithValue("@AllSms", request.AllSms);
        cmd.Parameters.AddWithValue("@Holiday", request.HolidayAsOffday);
        cmd.Parameters.AddWithValue("@English", request.EnglishSms);
        cmd.Parameters.AddWithValue("@Timeout", request.SmsTimeoutMinute);
        cmd.Parameters.AddWithValue("@StuAtt", request.StudentAttendance);
        cmd.Parameters.AddWithValue("@StuSms", request.StudentAllSms);
        cmd.Parameters.AddWithValue("@Entry", request.StudentEntrySms);
        cmd.Parameters.AddWithValue("@Exit", request.StudentExitSms);
        cmd.Parameters.AddWithValue("@Abs", request.StudentAbsSms);
        cmd.Parameters.AddWithValue("@Late", request.StudentLateSms);
        cmd.Parameters.AddWithValue("@EmpAtt", request.EmployeeAttendance);
        cmd.Parameters.AddWithValue("@EmpSms", request.EmployeeSms);
        cmd.Parameters.AddWithValue("@EmpAbs", request.EmployeeAbsSms);
        cmd.Parameters.AddWithValue("@EmpLate", request.EmployeeLateSms);
        cmd.Parameters.AddWithValue("@Own", request.EmployeeSmsOwnNumber);
        cmd.Parameters.AddWithValue("@EmpNo", (object?)request.EmployeeSmsNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID", request.AttendanceSettingID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public FileInfo? FindLatestInstaller()
    {
        var folders = new List<string>();
        var configured = _config["Attendance:InstallerFolder"];
        if (!string.IsNullOrWhiteSpace(configured))
            folders.Add(configured);

        var root = _env.ContentRootPath;
        folders.Add(Path.Combine(root, "App_For_Download"));
        folders.Add(Path.GetFullPath(Path.Combine(root, "..", "..", "..", "SIKKHALOY V2", "Attendances", "App_For_Download")));
        folders.Add(Path.GetFullPath(Path.Combine(root, "..", "..", "..", "AttendanceDevice", "Installer", "Output")));

        return folders
            .Where(Directory.Exists)
            .SelectMany(dir => new DirectoryInfo(dir).GetFiles("*.exe", SearchOption.TopDirectoryOnly))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();
    }

    public async Task<byte[]> ExportUsersCsvAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT DeviceID, ScheduleID, ID, RFID, Name, Designation, Is_Student
FROM dbo.VW_Attendance_Users
WHERE SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var csv = new StringBuilder();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.IsDBNull(i) ? "" : reader.GetValue(i)?.ToString() ?? "";
                csv.Append(value.Replace(",", ";"));
                csv.Append(',');
            }
            csv.Append("\r\n");
        }
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var body = utf8.GetBytes(csv.ToString());
        var bom = utf8.GetPreamble();
        var bytes = new byte[bom.Length + body.Length];
        Buffer.BlockCopy(bom, 0, bytes, 0, bom.Length);
        Buffer.BlockCopy(body, 0, bytes, bom.Length, body.Length);
        return bytes;
    }

    public async Task<byte[]?> ExportPhotosZipAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT ID, Image
FROM dbo.VW_Attendance_Users_Image
WHERE SchoolID = @SchoolID AND Image IS NOT NULL
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        using var output = new MemoryStream();
        var count = 0;
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader["ID"]?.ToString();
                if (string.IsNullOrWhiteSpace(id) || reader["Image"] is not byte[] bytes || bytes.Length == 0)
                    continue;
                var entry = zip.CreateEntry(SafeFileName(id) + ".jpg", CompressionLevel.Fastest);
                await using var stream = entry.Open();
                await stream.WriteAsync(bytes, cancellationToken);
                count++;
            }
        }
        return count == 0 ? null : output.ToArray();
    }

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        var name = new string(chars).Trim();
        return name.Length == 0 ? "photo" : name;
    }

    public async Task<IReadOnlyList<StudentRfidRowDto>> ListStudentRfidAsync(
        SessionSnapshot session, int scheduleId, int classId, int groupId, int sectionId, int shiftId,
        CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT Student.StudentID, StudentsClass.StudentClassID, Student.DeviceID, Student.ID, Student.RFID,
       Student.StudentsName, StudentsClass.RollNo,
       ISNULL(a.Entry_Confirmation, 0) AS Entry_Confirmation,
       ISNULL(a.Exit_Confirmation, 0) AS Exit_Confirmation,
       ISNULL(a.Is_Abs_SMS, 0) AS Is_Abs_SMS, ISNULL(a.Is_Late_SMS, 0) AS Is_Late_SMS,
       CAST(CASE WHEN a.StudentID IS NULL THEN 0 ELSE 1 END AS BIT) AS Assigned
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.Attendance_Schedule_AssignStudent AS a
    ON a.StudentID = Student.StudentID AND a.SchoolID = @SchoolID AND a.ScheduleID = @ScheduleID
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.ClassID = @ClassID
  AND Student.Status = N'Active'
  AND (@GroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @GroupID)
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
""", con);
        AddClassParams(cmd, session, scheduleId, classId, groupId, sectionId, shiftId);
        var items = new List<StudentRfidRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentRfidRowDto
            {
                StudentID = Convert.ToInt32(reader["StudentID"]),
                StudentClassID = Convert.ToInt32(reader["StudentClassID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                RollNo = NullString(reader["RollNo"]),
                DeviceID = NullString(reader["DeviceID"]),
                RFID = NullString(reader["RFID"]),
                Assigned = ToBool(reader["Assigned"]),
                PreSms = ToBool(reader["Entry_Confirmation"]),
                LateSms = ToBool(reader["Is_Late_SMS"]),
                AbsSms = ToBool(reader["Is_Abs_SMS"]),
                ExitSms = ToBool(reader["Exit_Confirmation"])
            });
        }
        return items;
    }

    public async Task<AttendanceResult> SaveStudentRfidAsync(
        SessionSnapshot session, SaveStudentRfidRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ScheduleID <= 0)
            return Fail("att.needSchedule");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        var errors = new List<string>();
        foreach (var row in request.Rows)
        {
            await using var rfid = new SqlCommand(
                "UPDATE dbo.Student SET RFID = @RFID WHERE StudentID = @StudentID AND SchoolID = @SchoolID", con);
            rfid.Parameters.AddWithValue("@RFID", (object?)row.RFID ?? DBNull.Value);
            rfid.Parameters.AddWithValue("@StudentID", row.StudentID);
            rfid.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await rfid.ExecuteNonQueryAsync(cancellationToken);

            if (row.Assigned)
            {
                var alreadyOnSchedule = await IsStudentAssignedAsync(
                    con, session.SchoolID, row.StudentID, request.ScheduleID, cancellationToken);
                if (!alreadyOnSchedule)
                {
                    var overlap = await GetScheduleOverlapAsync(
                        con, session.SchoolID, row.StudentID, request.ScheduleID,
                        "Attendance_Schedule_AssignStudent", "StudentID", cancellationToken);
                    if (overlap is not null)
                    {
                        errors.Add(overlap);
                        continue;
                    }
                }
            }

            await using var del = new SqlCommand("""
DELETE FROM dbo.Attendance_Schedule_AssignStudent
WHERE SchoolID = @SchoolID AND StudentID = @StudentID AND ScheduleID = @ScheduleID
""", con);
            del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            del.Parameters.AddWithValue("@StudentID", row.StudentID);
            del.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            await del.ExecuteNonQueryAsync(cancellationToken);

            if (row.Assigned)
            {
                await using var ins = new SqlCommand("""
INSERT INTO dbo.Attendance_Schedule_AssignStudent
    (SchoolID, RegistrationID, ScheduleID, StudentID, Entry_Confirmation, Exit_Confirmation, Is_Abs_SMS, Is_Late_SMS, Date)
VALUES (@SchoolID, @RegistrationID, @ScheduleID, @StudentID, @Pre, @Exit, @Abs, @Late, GETDATE())
""", con);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
                ins.Parameters.AddWithValue("@StudentID", row.StudentID);
                ins.Parameters.AddWithValue("@Pre", row.PreSms);
                ins.Parameters.AddWithValue("@Exit", row.ExitSms);
                ins.Parameters.AddWithValue("@Abs", row.AbsSms);
                ins.Parameters.AddWithValue("@Late", row.LateSms);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }
            saved++;
        }
        return BuildAssignResult(saved, errors);
    }

    public async Task<IReadOnlyList<EmployeeRfidRowDto>> ListEmployeeRfidAsync(
        SessionSnapshot session, int scheduleId, string? type, CancellationToken cancellationToken)
    {
        var employeeType = string.IsNullOrWhiteSpace(type) || type == "%" ? "%" : type.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT v.EmployeeID, v.ID, v.DeviceID, v.EmployeeType, v.Designation, v.Phone, v.RFID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(a.Is_Abs_SMS, 0) AS Is_Abs_SMS, ISNULL(a.Is_Late_SMS, 0) AS Is_Late_SMS,
       CAST(CASE WHEN a.EmployeeID IS NULL THEN 0 ELSE 1 END AS BIT) AS Assigned
FROM dbo.VW_Emp_Info AS v
LEFT JOIN dbo.Employee_Attendance_Schedule_Assign AS a
    ON a.EmployeeID = v.EmployeeID AND a.SchoolID = @SchoolID AND a.ScheduleID = @ScheduleID
WHERE v.SchoolID = @SchoolID AND v.Job_Status = N'Active' AND v.EmployeeType LIKE @Type
ORDER BY v.ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        cmd.Parameters.AddWithValue("@Type", employeeType);
        var items = new List<EmployeeRfidRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EmployeeRfidRowDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                Phone = NullString(reader["Phone"]),
                DeviceID = NullString(reader["DeviceID"]),
                RFID = NullString(reader["RFID"]),
                Assigned = ToBool(reader["Assigned"]),
                AbsSms = ToBool(reader["Is_Abs_SMS"]),
                LateSms = ToBool(reader["Is_Late_SMS"])
            });
        }
        return items;
    }

    public async Task<AttendanceResult> SaveEmployeeRfidAsync(
        SessionSnapshot session, SaveEmployeeRfidRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ScheduleID <= 0)
            return Fail("att.needSchedule");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        var errors = new List<string>();
        foreach (var row in request.Rows)
        {
            await using var rfid = new SqlCommand(
                "UPDATE dbo.Employee_Info SET RFID = @RFID WHERE EmployeeID = @EmployeeID AND SchoolID = @SchoolID", con);
            rfid.Parameters.AddWithValue("@RFID", (object?)row.RFID ?? DBNull.Value);
            rfid.Parameters.AddWithValue("@EmployeeID", row.EmployeeID);
            rfid.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await rfid.ExecuteNonQueryAsync(cancellationToken);

            if (row.Assigned)
            {
                var alreadyOnSchedule = await IsEmployeeAssignedAsync(
                    con, session.SchoolID, row.EmployeeID, request.ScheduleID, cancellationToken);
                if (!alreadyOnSchedule)
                {
                    var overlap = await GetScheduleOverlapAsync(
                        con, session.SchoolID, row.EmployeeID, request.ScheduleID,
                        "Employee_Attendance_Schedule_Assign", "EmployeeID", cancellationToken);
                    if (overlap is not null)
                    {
                        errors.Add(overlap);
                        continue;
                    }
                }
            }

            await using var del = new SqlCommand("""
DELETE FROM dbo.Employee_Attendance_Schedule_Assign
WHERE SchoolID = @SchoolID AND EmployeeID = @EmployeeID AND ScheduleID = @ScheduleID
""", con);
            del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            del.Parameters.AddWithValue("@EmployeeID", row.EmployeeID);
            del.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            await del.ExecuteNonQueryAsync(cancellationToken);

            if (row.Assigned)
            {
                await using var ins = new SqlCommand("""
INSERT INTO dbo.Employee_Attendance_Schedule_Assign
    (SchoolID, RegistrationID, EmployeeID, ScheduleID, Is_Abs_SMS, Is_Late_SMS)
VALUES (@SchoolID, @RegistrationID, @EmployeeID, @ScheduleID, @Abs, @Late)
""", con);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@EmployeeID", row.EmployeeID);
                ins.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
                ins.Parameters.AddWithValue("@Abs", row.AbsSms);
                ins.Parameters.AddWithValue("@Late", row.LateSms);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }
            saved++;
        }
        return BuildAssignResult(saved, errors);
    }

    public async Task<IReadOnlyList<StudentManualRowDto>> ListStudentManualAsync(
        SessionSnapshot session, int scheduleId, int classId, int groupId, int sectionId, int shiftId,
        DateTime date, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID, Student.StudentsName,
       StudentsClass.RollNo, Student.SMSPhoneNo,
       ar.Attendance, ar.Reason,
       CAST(CASE WHEN ar.StudentClassID IS NULL THEN 0 ELSE 1 END AS BIT) AS HasRecord,
       COALESCE(
           NULLIF(LTRIM(RTRIM(ISNULL(adm.FirstName, N'') + N' ' + ISNULL(adm.LastName, N''))), N''),
           NULLIF(LTRIM(RTRIM(ISNULL(t.FirstName, N'') + N' ' + ISNULL(t.LastName, N''))), N''),
           r.UserName
       ) AS UpdatedByName,
       al.StartDate AS LeaveStart, al.EndDate AS LeaveEnd, al.Description AS LeaveDescription
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.Attendance_Schedule_AssignStudent AS ass
    ON ass.SchoolID = StudentsClass.SchoolID AND ass.StudentID = Student.StudentID AND ass.ScheduleID = @ScheduleID
LEFT JOIN dbo.Attendance_Record AS ar
    ON ar.StudentClassID = StudentsClass.StudentClassID
   AND ar.SchoolID = @SchoolID AND ar.EducationYearID = @EducationYearID
   AND CAST(ar.AttendanceDate AS DATE) = @Date
   AND ISNULL(ar.Attendance_ScheduleID, 0) = @ScheduleID
LEFT JOIN dbo.Registration AS r ON ar.RegistrationID = r.RegistrationID AND ar.RegistrationID > 0
LEFT JOIN dbo.Admin AS adm ON ar.RegistrationID = adm.RegistrationID AND ar.SchoolID = adm.SchoolID
LEFT JOIN dbo.Teacher AS t ON ar.RegistrationID = t.TeacherRegistrationID AND ar.SchoolID = t.SchoolID
LEFT JOIN dbo.Attendance_Leave AS al
    ON al.StudentID = Student.StudentID AND al.SchoolID = @SchoolID
   AND al.EducationYearID = @EducationYearID
   AND CAST(al.StartDate AS DATE) <= @Date AND CAST(al.EndDate AS DATE) >= @Date
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.ClassID = @ClassID
  AND Student.Status = N'Active'
  AND (@GroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @GroupID)
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
""", con);
        AddClassParams(cmd, session, scheduleId, classId, groupId, sectionId, shiftId);
        cmd.Parameters.AddWithValue("@Date", date.Date);
        try
        {
            return await ReadManualRowsAsync(cmd, cancellationToken);
        }
        catch (SqlException)
        {
            await using var fallback = new SqlCommand("""
SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID, Student.StudentsName,
       StudentsClass.RollNo, Student.SMSPhoneNo,
       ar.Attendance, ar.Reason,
       CAST(CASE WHEN ar.StudentClassID IS NULL THEN 0 ELSE 1 END AS BIT) AS HasRecord,
       CAST(NULL AS nvarchar(100)) AS UpdatedByName,
       CAST(NULL AS datetime) AS LeaveStart, CAST(NULL AS datetime) AS LeaveEnd,
       CAST(NULL AS nvarchar(400)) AS LeaveDescription
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.Attendance_Schedule_AssignStudent AS ass
    ON ass.SchoolID = StudentsClass.SchoolID AND ass.StudentID = Student.StudentID AND ass.ScheduleID = @ScheduleID
LEFT JOIN dbo.Attendance_Record AS ar
    ON ar.StudentClassID = StudentsClass.StudentClassID
   AND ar.SchoolID = @SchoolID AND ar.EducationYearID = @EducationYearID
   AND CAST(ar.AttendanceDate AS DATE) = @Date
   AND ISNULL(ar.Attendance_ScheduleID, 0) = @ScheduleID
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.ClassID = @ClassID
  AND Student.Status = N'Active'
  AND (@GroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @GroupID)
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
""", con);
            AddClassParams(fallback, session, scheduleId, classId, groupId, sectionId, shiftId);
            fallback.Parameters.AddWithValue("@Date", date.Date);
            return await ReadManualRowsAsync(fallback, cancellationToken);
        }
    }

    private static async Task<IReadOnlyList<StudentManualRowDto>> ReadManualRowsAsync(
        SqlCommand cmd, CancellationToken cancellationToken)
    {
        var items = new List<StudentManualRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var hasRecord = ToBool(reader["HasRecord"]);
            var attendance = hasRecord ? (reader["Attendance"]?.ToString() ?? "Pre") : "Pre";
            if (string.IsNullOrWhiteSpace(attendance))
                attendance = "Pre";
            var reason = hasRecord ? NullString(reader["Reason"]) : null;
            string? leaveRange = null;
            if (!hasRecord && reader["LeaveStart"] is not DBNull && reader["LeaveStart"] is not null)
            {
                attendance = "Leave";
                reason = NullString(reader["LeaveDescription"]);
                var start = Convert.ToDateTime(reader["LeaveStart"]).ToString("d MMM yy");
                var end = Convert.ToDateTime(reader["LeaveEnd"]).ToString("d MMM yy");
                leaveRange = $"(From:{start} To {end})";
            }

            items.Add(new StudentManualRowDto
            {
                StudentID = Convert.ToInt32(reader["StudentID"]),
                StudentClassID = Convert.ToInt32(reader["StudentClassID"]),
                ClassID = Convert.ToInt32(reader["ClassID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                RollNo = NullString(reader["RollNo"]),
                Phone = NullString(reader["SMSPhoneNo"]),
                Attendance = attendance,
                Reason = reason,
                TakenBy = NullString(reader["UpdatedByName"]),
                LeaveRange = leaveRange,
                HasRecord = hasRecord,
                SendSms = attendance is "Abs" or "Late" or "Bunk",
                Selected = hasRecord
            });
        }
        return items;
    }

    public async Task<AttendanceResult> SaveStudentManualAsync(
        SessionSnapshot session, SaveStudentManualRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ScheduleID <= 0)
            return Fail("att.needSchedule");
        if (request.Rows.Count == 0)
            return Fail("att.needRows");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        foreach (var row in request.Rows)
        {
            await using var cmd = new SqlCommand("""
IF NOT EXISTS (
    SELECT 1 FROM dbo.Attendance_Record
    WHERE StudentClassID = @StudentClassID AND CAST(AttendanceDate AS DATE) = @Date
      AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
      AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
    INSERT INTO dbo.Attendance_Record
        (SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID,
         Attendance_ScheduleID, Attendance, AttendanceDate, Reason)
    VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @ClassID, @StudentClassID,
            @ScheduleID, @Attendance, @Date, @Reason);
ELSE
    UPDATE dbo.Attendance_Record
    SET Attendance = @Attendance, Reason = @Reason, RegistrationID = @RegistrationID
    WHERE StudentClassID = @StudentClassID AND CAST(AttendanceDate AS DATE) = @Date
      AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
      AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID;
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@StudentID", row.StudentID);
            cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
            cmd.Parameters.AddWithValue("@StudentClassID", row.StudentClassID);
            cmd.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            cmd.Parameters.AddWithValue("@Attendance", row.Attendance);
            cmd.Parameters.AddWithValue("@Date", request.AttendanceDate.Date);
            cmd.Parameters.AddWithValue("@Reason", (object?)row.Reason ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            saved++;
        }
        if (request.Rows.Any(x => x.SendSms))
            _ = _sms.TrySendManualAttendanceAsync(session, request, CancellationToken.None);
        return new AttendanceResult { Succeeded = true, Saved = saved };
    }

    public async Task<IReadOnlyList<EmployeeManualRowDto>> ListEmployeeManualAsync(
        SessionSnapshot session, int scheduleId, string? type, DateTime date, CancellationToken cancellationToken)
    {
        var employeeType = string.IsNullOrWhiteSpace(type) || type == "%" ? "%" : type.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT v.EmployeeID, v.ID, v.Designation, v.EmployeeType,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(r.AttendanceStatus, N'Pre') AS Attendance,
       CONVERT(varchar(8), r.EntryTime, 108) AS EntryTime,
       CONVERT(varchar(8), r.ExitTime, 108) AS ExitTime,
       CAST(CASE WHEN r.EmployeeID IS NULL THEN 0 ELSE 1 END AS BIT) AS HasRecord
FROM dbo.VW_Emp_Info AS v
INNER JOIN dbo.Employee_Attendance_Schedule_Assign AS a
    ON a.EmployeeID = v.EmployeeID AND a.SchoolID = v.SchoolID AND a.ScheduleID = @ScheduleID
LEFT JOIN dbo.Employee_Attendance_Record AS r
    ON r.EmployeeID = v.EmployeeID AND r.SchoolID = @SchoolID
   AND CAST(r.AttendanceDate AS DATE) = @Date
   AND ISNULL(r.Attendance_ScheduleID, 0) = @ScheduleID
WHERE v.SchoolID = @SchoolID AND v.Job_Status = N'Active' AND v.EmployeeType LIKE @Type
ORDER BY v.ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        cmd.Parameters.AddWithValue("@Type", employeeType);
        cmd.Parameters.AddWithValue("@Date", date.Date);
        var items = new List<EmployeeManualRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var hasRecord = ToBool(reader["HasRecord"]);
            items.Add(new EmployeeManualRowDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                Attendance = hasRecord ? (reader["Attendance"]?.ToString() ?? "Pre") : "",
                EntryTime = TrimTime(reader["EntryTime"]),
                ExitTime = TrimTime(reader["ExitTime"]),
                Selected = hasRecord,
                HasRecord = hasRecord
            });
        }
        return items;
    }

    public async Task<AttendanceResult> SaveEmployeeManualAsync(
        SessionSnapshot session, SaveEmployeeManualRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ScheduleID <= 0)
            return Fail("att.needSchedule");
        if (request.Rows.Count == 0)
            return Fail("att.needRows");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        foreach (var row in request.Rows)
        {
            await using var cmd = new SqlCommand("""
IF EXISTS (
    SELECT 1 FROM dbo.Employee_Attendance_Record
    WHERE EmployeeID = @EmployeeID AND CAST(AttendanceDate AS DATE) = @Date
      AND SchoolID = @SchoolID AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
    UPDATE dbo.Employee_Attendance_Record
    SET AttendanceStatus = @Status, EntryTime = @Entry, ExitTime = @Exit, Attendance_ScheduleID = @ScheduleID
    WHERE EmployeeID = @EmployeeID AND CAST(AttendanceDate AS DATE) = @Date
      AND SchoolID = @SchoolID AND ISNULL(Attendance_ScheduleID, 0) = @ScheduleID;
ELSE
    INSERT INTO dbo.Employee_Attendance_Record
        (SchoolID, RegistrationID, EmployeeID, Attendance_ScheduleID, AttendanceStatus, AttendanceDate, EntryTime, ExitTime)
    VALUES (@SchoolID, @RegistrationID, @EmployeeID, @ScheduleID, @Status, @Date, @Entry, @Exit);
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@EmployeeID", row.EmployeeID);
            cmd.Parameters.AddWithValue("@ScheduleID", request.ScheduleID);
            cmd.Parameters.AddWithValue("@Status", row.Attendance);
            cmd.Parameters.AddWithValue("@Date", request.AttendanceDate.Date);
            cmd.Parameters.AddWithValue("@Entry", ParseTimeOrNull(row.EntryTime));
            cmd.Parameters.AddWithValue("@Exit", ParseTimeOrNull(row.ExitTime));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            saved++;
        }
        return new AttendanceResult { Succeeded = true, Saved = saved };
    }

    public async Task<IReadOnlyList<StudentAttendanceRecordDto>> ListStudentRecordsAsync(
        SessionSnapshot session, string? status, int classId, int groupId, int sectionId, int shiftId,
        int scheduleId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var like = string.IsNullOrWhiteSpace(status) || status == "%" ? "%" : status.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT CAST(ar.AttendanceDate AS DATE) AS AttendanceDate, ar.Attendance, ar.Reason,
       CONVERT(varchar(8), ar.EntryTime, 108) AS EntryTime,
       CONVERT(varchar(8), ar.ExitTime, 108) AS ExitTime,
       s.ID, s.StudentsName, sc.RollNo, cc.Class
FROM dbo.Attendance_Record AS ar
INNER JOIN dbo.Student AS s ON ar.StudentID = s.StudentID
INNER JOIN dbo.StudentsClass AS sc ON ar.StudentClassID = sc.StudentClassID
INNER JOIN dbo.CreateClass AS cc ON ar.ClassID = cc.ClassID
WHERE ar.SchoolID = @SchoolID AND ar.EducationYearID = @EducationYearID
  AND s.Status = N'Active'
  AND ar.Attendance LIKE @Status
  AND CAST(ar.AttendanceDate AS DATE) >= @FromDate
  AND CAST(ar.AttendanceDate AS DATE) <= @ToDate
  AND (@ClassID = 0 OR ar.ClassID = @ClassID)
  AND (@ScheduleID = 0 OR ISNULL(ar.Attendance_ScheduleID, 0) = @ScheduleID)
  AND (@GroupID = 0 OR ISNULL(sc.SubjectGroupID, 0) = @GroupID)
  AND (@SectionID = 0 OR ISNULL(sc.SectionID, 0) = @SectionID)
  AND (@ShiftID = 0 OR ISNULL(sc.ShiftID, 0) = @ShiftID)
ORDER BY ar.AttendanceDate DESC, s.ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Status", like);
        cmd.Parameters.AddWithValue("@FromDate", from.Date);
        cmd.Parameters.AddWithValue("@ToDate", to.Date);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        cmd.Parameters.AddWithValue("@GroupID", groupId);
        cmd.Parameters.AddWithValue("@SectionID", sectionId);
        cmd.Parameters.AddWithValue("@ShiftID", shiftId);
        var items = new List<StudentAttendanceRecordDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentAttendanceRecordDto
            {
                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]).Date,
                Attendance = reader["Attendance"]?.ToString() ?? "",
                Reason = NullString(reader["Reason"]),
                EntryTime = TrimTime(reader["EntryTime"]),
                ExitTime = TrimTime(reader["ExitTime"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                RollNo = NullString(reader["RollNo"]),
                ClassName = NullString(reader["Class"])
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<StudentAttendanceSummaryDto>> ListStudentSummaryAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId,
        int scheduleId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var records = await ListStudentRecordsAsync(
            session, "%", classId, groupId, sectionId, shiftId, scheduleId, from, to, cancellationToken);
        return records
            .GroupBy(x => new { x.ID, x.Name, x.RollNo })
            .Select(g => new StudentAttendanceSummaryDto
            {
                ID = g.Key.ID,
                Name = g.Key.Name,
                RollNo = g.Key.RollNo,
                Present = g.Count(x => x.Attendance == "Pre"),
                Absent = g.Count(x => x.Attendance == "Abs"),
                Late = g.Count(x => x.Attendance == "Late"),
                LateAbs = g.Count(x => x.Attendance == "Late Abs"),
                Leave = g.Count(x => x.Attendance == "Leave"),
                Bunk = g.Count(x => x.Attendance == "Bunk")
            })
            .OrderBy(x => x.ID)
            .ToList();
    }

    public async Task<IReadOnlyList<EmployeeAttendanceRecordDto>> ListEmployeeRecordsAsync(
        SessionSnapshot session, string? type, string? status, int scheduleId, int employeeId,
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var employeeType = string.IsNullOrWhiteSpace(type) || type == "%" ? "%" : type.Trim();
        var like = string.IsNullOrWhiteSpace(status) || status == "%" ? "%" : status.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT CAST(r.AttendanceDate AS DATE) AS AttendanceDate, r.AttendanceStatus,
       CONVERT(varchar(8), r.EntryTime, 108) AS EntryTime,
       CONVERT(varchar(8), r.ExitTime, 108) AS ExitTime,
       v.ID, v.Designation, v.EmployeeType,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name
FROM dbo.Employee_Attendance_Record AS r
INNER JOIN dbo.VW_Emp_Info AS v ON r.EmployeeID = v.EmployeeID
WHERE r.SchoolID = @SchoolID
  AND v.EmployeeType LIKE @Type
  AND r.AttendanceStatus LIKE @Status
  AND CAST(r.AttendanceDate AS DATE) >= @FromDate
  AND CAST(r.AttendanceDate AS DATE) <= @ToDate
  AND (@ScheduleID = 0 OR ISNULL(r.Attendance_ScheduleID, 0) = @ScheduleID)
  AND (@EmployeeID = 0 OR r.EmployeeID = @EmployeeID)
ORDER BY r.AttendanceDate DESC, v.ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Type", employeeType);
        cmd.Parameters.AddWithValue("@Status", like);
        cmd.Parameters.AddWithValue("@FromDate", from.Date);
        cmd.Parameters.AddWithValue("@ToDate", to.Date);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        var items = new List<EmployeeAttendanceRecordDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EmployeeAttendanceRecordDto
            {
                AttendanceDate = Convert.ToDateTime(reader["AttendanceDate"]).Date,
                Attendance = reader["AttendanceStatus"]?.ToString() ?? "",
                EntryTime = TrimTime(reader["EntryTime"]),
                ExitTime = TrimTime(reader["ExitTime"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? ""
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<EmployeeAttendanceSummaryDto>> ListEmployeeSummaryAsync(
        SessionSnapshot session, string? type, int scheduleId, int employeeId,
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var records = await ListEmployeeRecordsAsync(
            session, type, "%", scheduleId, employeeId, from, to, cancellationToken);
        return records
            .GroupBy(x => new { x.ID, x.Name, x.Designation })
            .Select(g => new EmployeeAttendanceSummaryDto
            {
                ID = g.Key.ID,
                Name = g.Key.Name,
                Designation = g.Key.Designation,
                Present = g.Count(x => x.Attendance == "Pre"),
                Absent = g.Count(x => x.Attendance == "Abs"),
                Late = g.Count(x => x.Attendance == "Late"),
                LateAbs = g.Count(x => x.Attendance == "Late Abs"),
                Leave = g.Count(x => x.Attendance == "Leave")
            })
            .OrderBy(x => x.ID)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> ListLeaveTypesAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand("""
SELECT LeaveTypeName FROM dbo.Attendance_Leave_Type
WHERE SchoolID = @SchoolID AND IsActive = 1
ORDER BY SortOrder, LeaveTypeName
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var items = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var name = reader["LeaveTypeName"]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    items.Add(name);
            }
            return items.Count == 0 ? DefaultLeaveTypes : items;
        }
        catch (SqlException)
        {
            return DefaultLeaveTypes;
        }
    }

    public async Task<IReadOnlyList<AttendanceLeaveTypeDto>> ListLeaveTypeRowsAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand("""
SELECT LeaveTypeID, LeaveTypeName
FROM dbo.Attendance_Leave_Type
WHERE SchoolID = @SchoolID AND IsActive = 1
ORDER BY SortOrder, LeaveTypeName
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            return await ReadLeaveTypeRowsAsync(cmd, cancellationToken);
        }
        catch (SqlException)
        {
            try
            {
                await using var con = _connections.Create();
                await con.OpenAsync(cancellationToken);
                await using var cmd = new SqlCommand("""
SELECT LeaveTypeID, LeaveTypeName
FROM dbo.Attendance_Leave_Type
WHERE SchoolID = @SchoolID
ORDER BY LeaveTypeName
""", con);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                return await ReadLeaveTypeRowsAsync(cmd, cancellationToken);
            }
            catch (SqlException)
            {
                return [];
            }
        }
    }

    public async Task<AttendanceResult> AddLeaveTypeAsync(
        SessionSnapshot session, SaveLeaveTypeRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("att.leaveTypeNeedName");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var exists = new SqlCommand("""
SELECT TOP 1 LeaveTypeID FROM dbo.Attendance_Leave_Type
WHERE SchoolID = @SchoolID AND LeaveTypeName = @Name
""", con);
            exists.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            exists.Parameters.AddWithValue("@Name", name);
            if (await exists.ExecuteScalarAsync(cancellationToken) is not null and not DBNull)
                return Fail("att.leaveTypeExists");
        }
        catch (SqlException)
        {
            return Fail("att.failed");
        }

        try
        {
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Leave_Type (SchoolID, LeaveTypeName, SortOrder, IsActive)
SELECT @SchoolID, @Name, ISNULL(MAX(SortOrder), 0) + 1, 1
FROM dbo.Attendance_Leave_Type
WHERE SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Name", name);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return Ok(1);
        }
        catch (SqlException)
        {
            try
            {
                await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Leave_Type (SchoolID, LeaveTypeName, SortOrder)
SELECT @SchoolID, @Name, ISNULL(MAX(SortOrder), 0) + 1
FROM dbo.Attendance_Leave_Type
WHERE SchoolID = @SchoolID
""", con);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                cmd.Parameters.AddWithValue("@Name", name);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                return Ok(1);
            }
            catch (SqlException)
            {
                return Fail("att.failed");
            }
        }
    }

    public async Task<AttendanceResult> DeleteLeaveTypeAsync(
        SessionSnapshot session, int leaveTypeId, CancellationToken cancellationToken)
    {
        if (leaveTypeId <= 0)
            return Fail("att.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.Attendance_Leave_Type WHERE LeaveTypeID = @ID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@ID", leaveTypeId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    private static async Task<List<AttendanceLeaveTypeDto>> ReadLeaveTypeRowsAsync(
        SqlCommand cmd, CancellationToken cancellationToken)
    {
        var items = new List<AttendanceLeaveTypeDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader["LeaveTypeName"]?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;
            items.Add(new AttendanceLeaveTypeDto
            {
                LeaveTypeID = Convert.ToInt32(reader["LeaveTypeID"]),
                Name = name
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<StudentLeaveSuggestDto>> SuggestStudentLeaveAsync(
        SessionSnapshot session, string? query, CancellationToken cancellationToken)
    {
        var code = (query ?? "").Trim();
        if (code.Length == 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT TOP 8 Student.ID, Student.StudentsName, ISNULL(CreateClass.Class, N'') AS ClassName
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE Student.Status = N'Active'
  AND StudentsClass.SchoolID = @SchoolID AND StudentsClass.EducationYearID = @EducationYearID
  AND Student.ID LIKE @ID + N'%'
ORDER BY Student.ID
""", con);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        var items = new List<StudentLeaveSuggestDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentLeaveSuggestDto
            {
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                ClassName = NullString(reader["ClassName"])
            });
        }
        return items;
    }

    public async Task<StudentLeavePersonDto?> FindStudentLeaveAsync(
        SessionSnapshot session, string id, CancellationToken cancellationToken)
    {
        var code = (id ?? "").Trim();
        if (code.Length == 0)
            return null;
        var unpadded = code.TrimStart('0');
        if (unpadded.Length == 0)
            unpadded = "0";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 Student.StudentID, Student.ID, Student.StudentsName, Student.FathersName, Student.SMSPhoneNo,
       Student.Gender, ISNULL(CreateClass.Class, N'') AS ClassName,
       ISNULL(CreateSection.Section, N'') AS Section,
       ISNULL(CreateSubjectGroup.SubjectGroup, N'') AS GroupName,
       ISNULL(CreateShift.Shift, N'') AS Shift, Student_Image.Image
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT JOIN dbo.CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT JOIN dbo.CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID
LEFT JOIN dbo.CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID
LEFT JOIN dbo.Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE Student.Status = N'Active'
  AND StudentsClass.SchoolID = @SchoolID AND StudentsClass.EducationYearID = @EducationYearID
  AND (Student.ID = @ID OR LTRIM(RTRIM(Student.ID)) = @ID OR LTRIM(RTRIM(Student.ID)) = @Unpadded)
""", con);
            AddFindParams(cmd, session, code, unpadded);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            return ReadStudentLeavePerson(reader, true);
        }
        catch (SqlException)
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 Student.StudentID, Student.ID, Student.StudentsName, Student.FathersName, Student.SMSPhoneNo, CreateClass.Class
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE Student.Status = N'Active'
  AND StudentsClass.SchoolID = @SchoolID AND StudentsClass.EducationYearID = @EducationYearID
  AND (Student.ID = @ID OR LTRIM(RTRIM(Student.ID)) = @ID OR LTRIM(RTRIM(Student.ID)) = @Unpadded)
""", con);
            AddFindParams(cmd, session, code, unpadded);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            return ReadStudentLeavePerson(reader, false);
        }
    }

    public async Task<IReadOnlyList<StudentLeaveRowDto>> ListStudentLeavesAsync(
        SessionSnapshot session, int studentId, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT StudentLeaveID, StartDate, EndDate, Description, LeaveType, GuardianName
FROM dbo.Attendance_Leave
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND StudentID = @StudentID
ORDER BY StartDate DESC
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        var items = new List<StudentLeaveRowDto>();
        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                items.Add(ReadStudentLeave(reader));
        }
        catch (SqlException)
        {
            await using var fallback = new SqlCommand("""
SELECT StudentLeaveID, StartDate, EndDate, Description
FROM dbo.Attendance_Leave
WHERE SchoolID = @SchoolID AND StudentID = @StudentID
ORDER BY StartDate DESC
""", con);
            fallback.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            fallback.Parameters.AddWithValue("@StudentID", studentId);
            await using var reader = await fallback.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                items.Add(ReadStudentLeave(reader));
        }
        return items;
    }

    public async Task<AttendanceResult> SaveStudentLeaveAsync(
        SessionSnapshot session, SaveStudentLeaveRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0)
            return Fail("att.needStudent");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Leave
    (SchoolID, RegistrationID, StudentID, StartDate, EndDate, Description, EducationYearID, LeaveType, GuardianName)
VALUES (@SchoolID, @RegistrationID, @StudentID, @Start, @End, @Desc, @EducationYearID, @Type, @Guardian);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@StudentID", request.StudentID);
            cmd.Parameters.AddWithValue("@Start", request.StartDate.Date);
            cmd.Parameters.AddWithValue("@End", request.EndDate.Date);
            cmd.Parameters.AddWithValue("@Desc", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@Type", (object?)request.LeaveType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Guardian", (object?)request.GuardianName ?? DBNull.Value);
            var id = ToInt(await cmd.ExecuteScalarAsync(cancellationToken), 0);
            return Ok(1, id);
        }
        catch (SqlException)
        {
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Leave
    (SchoolID, RegistrationID, StudentID, StartDate, EndDate, Description, EducationYearID)
VALUES (@SchoolID, @RegistrationID, @StudentID, @Start, @End, @Desc, @EducationYearID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@StudentID", request.StudentID);
            cmd.Parameters.AddWithValue("@Start", request.StartDate.Date);
            cmd.Parameters.AddWithValue("@End", request.EndDate.Date);
            cmd.Parameters.AddWithValue("@Desc", (object?)request.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            var id = ToInt(await cmd.ExecuteScalarAsync(cancellationToken), 0);
            return Ok(1, id);
        }
    }

    public async Task<AttendanceResult> DeleteStudentLeaveAsync(
        SessionSnapshot session, int leaveId, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.Attendance_Leave WHERE StudentLeaveID = @ID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@ID", leaveId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<IReadOnlyList<EmployeeLeavePickDto>> ListEmployeeLeavePicksAsync(
        SessionSnapshot session, string? type, string? query, CancellationToken cancellationToken)
    {
        var employeeType = string.IsNullOrWhiteSpace(type) || type == "%" ? "%" : type.Trim();
        var search = (query ?? "").Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT EmployeeID, ID, Designation, EmployeeType, Phone,
       LTRIM(RTRIM(ISNULL(FirstName, N'') + N' ' + ISNULL(LastName, N''))) AS Name
FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID AND Job_Status = N'Active' AND EmployeeType LIKE @Type
ORDER BY ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Type", employeeType);
        var items = new List<EmployeeLeavePickDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new EmployeeLeavePickDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                Phone = NullString(reader["Phone"])
            };
            if (search.Length == 0
                || Contains(row.ID, search) || Contains(row.Name, search)
                || Contains(row.Designation, search) || Contains(row.Phone, search))
                items.Add(row);
        }
        return items;
    }

    public async Task<AttendanceResult> SaveEmployeeLeaveAsync(
        SessionSnapshot session, SaveEmployeeLeaveRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.EmployeeIDs.Count == 0)
            return Fail("att.needRows");
        var reason = (request.Reason ?? "").Trim();
        if (reason.Length == 0)
            return Fail("att.reason");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        foreach (var id in request.EmployeeIDs.Distinct())
        {
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Employee_Leave
    (SchoolID, RegistrationID, EducationYearID, EmployeeID, LeaveStartDate, LeaveEndDate, LeaveReason, ApproveStatus, ApprovedBy_RegistrationID)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @EmployeeID, @Start, @End, @Reason, N'Approved', @RegistrationID)
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@EmployeeID", id);
            cmd.Parameters.AddWithValue("@Start", request.StartDate.Date);
            cmd.Parameters.AddWithValue("@End", request.EndDate.Date);
            cmd.Parameters.AddWithValue("@Reason", reason);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            saved++;
        }
        return new AttendanceResult { Succeeded = true, Saved = saved };
    }

    public async Task<StudentLeavePrintDto?> GetStudentLeavePrintAsync(
        SessionSnapshot session, int leaveId, CancellationToken cancellationToken)
    {
        if (leaveId <= 0)
            return null;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("""
SELECT al.StudentLeaveID, al.StartDate, al.EndDate, al.Description,
       DATEDIFF(DAY, al.StartDate, al.EndDate) + 1 AS LeaveDays,
       ISNULL(al.LeaveType, N'') AS LeaveType, ISNULL(al.GuardianName, N'') AS GuardianName,
       s.StudentsName, s.FathersName, s.ID AS StudentDisplayID,
       si.SchoolName,
       ISNULL(si.Address, N'') + ISNULL(N', ' + si.City, N'') + ISNULL(N', ' + si.State, N'') AS SchoolAddress,
       ISNULL(si.Phone, N'') AS SchoolPhone,
       ISNULL(cc.Class, N'') AS ClassName, ISNULL(csg.SubjectGroup, N'') AS GroupName
FROM dbo.Attendance_Leave AS al
INNER JOIN dbo.Student AS s ON al.StudentID = s.StudentID
INNER JOIN dbo.SchoolInfo AS si ON al.SchoolID = si.SchoolID
LEFT JOIN dbo.StudentsClass AS sc
    ON sc.StudentID = s.StudentID AND sc.EducationYearID = al.EducationYearID AND sc.SchoolID = al.SchoolID
LEFT JOIN dbo.CreateClass AS cc ON sc.ClassID = cc.ClassID
LEFT JOIN dbo.CreateSubjectGroup AS csg ON sc.SubjectGroupID = csg.SubjectGroupID
WHERE al.StudentLeaveID = @ID AND al.SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@ID", leaveId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            return ReadStudentLeavePrint(reader, session);
        }
        catch (SqlException)
        {
            await using var cmd = new SqlCommand("""
SELECT al.StudentLeaveID, al.StartDate, al.EndDate, al.Description,
       DATEDIFF(DAY, al.StartDate, al.EndDate) + 1 AS LeaveDays,
       s.StudentsName, s.FathersName, s.ID AS StudentDisplayID,
       si.SchoolName, ISNULL(si.Address, N'') AS SchoolAddress, ISNULL(si.Phone, N'') AS SchoolPhone,
       ISNULL(cc.Class, N'') AS ClassName
FROM dbo.Attendance_Leave AS al
INNER JOIN dbo.Student AS s ON al.StudentID = s.StudentID
INNER JOIN dbo.SchoolInfo AS si ON al.SchoolID = si.SchoolID
LEFT JOIN dbo.StudentsClass AS sc
    ON sc.StudentID = s.StudentID AND sc.EducationYearID = al.EducationYearID AND sc.SchoolID = al.SchoolID
LEFT JOIN dbo.CreateClass AS cc ON sc.ClassID = cc.ClassID
WHERE al.StudentLeaveID = @ID AND al.SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@ID", leaveId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            return ReadStudentLeavePrint(reader, session);
        }
    }

    public async Task<IReadOnlyList<LeaveReportRowDto>> ListLeaveReportAsync(
        SessionSnapshot session, string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var kind = (type ?? "Student").Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (kind.Equals("Employee", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await using var cmd = new SqlCommand("""
SELECT el.Employee_LeaveID AS EmployeeLeaveID, v.ID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(v.Designation, N'') AS Designation,
       el.LeaveStartDate, el.LeaveEndDate, ISNULL(el.LeaveReason, N'') AS LeaveReason
FROM dbo.Employee_Leave AS el
INNER JOIN dbo.VW_Emp_Info AS v
    ON v.EmployeeID = el.EmployeeID AND v.SchoolID = el.SchoolID
WHERE el.SchoolID = @SchoolID
  AND (@FromDate IS NULL OR CAST(el.LeaveStartDate AS DATE) >= @FromDate)
  AND (@ToDate IS NULL OR CAST(el.LeaveStartDate AS DATE) <= @ToDate)
ORDER BY el.LeaveStartDate DESC
""", con);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)from?.Date ?? DBNull.Value;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)to?.Date ?? DBNull.Value;
                return await ReadEmployeeLeaveReportAsync(cmd, cancellationToken);
            }
            catch (SqlException)
            {
                await using var fallback = new SqlCommand("""
SELECT el.Employee_LeaveID AS EmployeeLeaveID, CAST(el.EmployeeID AS nvarchar(50)) AS ID,
       CAST(el.EmployeeID AS nvarchar(50)) AS Name,
       N'' AS Designation,
       el.LeaveStartDate, el.LeaveEndDate, ISNULL(el.LeaveReason, N'') AS LeaveReason
FROM dbo.Employee_Leave AS el
WHERE el.SchoolID = @SchoolID
  AND (@FromDate IS NULL OR CAST(el.LeaveStartDate AS DATE) >= @FromDate)
  AND (@ToDate IS NULL OR CAST(el.LeaveStartDate AS DATE) <= @ToDate)
ORDER BY el.LeaveStartDate DESC
""", con);
                fallback.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                fallback.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)from?.Date ?? DBNull.Value;
                fallback.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)to?.Date ?? DBNull.Value;
                return await ReadEmployeeLeaveReportAsync(fallback, cancellationToken);
            }
        }

        await using var stu = new SqlCommand("""
SELECT al.StudentLeaveID, s.ID, s.StudentsName, ISNULL(cc.Class, N'') AS ClassName,
       al.StartDate, al.EndDate, al.Description, al.LeaveType
FROM dbo.Attendance_Leave AS al
INNER JOIN dbo.Student AS s ON al.StudentID = s.StudentID
LEFT JOIN dbo.StudentsClass AS sc
    ON s.StudentID = sc.StudentID AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @EducationYearID
LEFT JOIN dbo.CreateClass AS cc ON sc.ClassID = cc.ClassID
WHERE al.SchoolID = @SchoolID
  AND (@FromDate IS NULL OR CAST(al.StartDate AS DATE) >= @FromDate)
  AND (@ToDate IS NULL OR CAST(al.StartDate AS DATE) <= @ToDate)
ORDER BY al.StartDate DESC
""", con);
        stu.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        stu.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        stu.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)from?.Date ?? DBNull.Value;
        stu.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)to?.Date ?? DBNull.Value;
        var rows = new List<LeaveReportRowDto>();
        try
        {
            await using var reader = await stu.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                rows.Add(ReadLeaveReport(reader, true));
        }
        catch (SqlException)
        {
            await using var fallback = new SqlCommand("""
SELECT al.StudentLeaveID, s.ID, s.StudentsName, N'' AS ClassName,
       al.StartDate, al.EndDate, al.Description
FROM dbo.Attendance_Leave AS al
INNER JOIN dbo.Student AS s ON al.StudentID = s.StudentID
WHERE al.SchoolID = @SchoolID
  AND (@FromDate IS NULL OR CAST(al.StartDate AS DATE) >= @FromDate)
  AND (@ToDate IS NULL OR CAST(al.StartDate AS DATE) <= @ToDate)
ORDER BY al.StartDate DESC
""", con);
            fallback.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            fallback.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)from?.Date ?? DBNull.Value;
            fallback.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)to?.Date ?? DBNull.Value;
            await using var reader = await fallback.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                rows.Add(ReadLeaveReport(reader, false));
        }
        return rows;
    }

    public async Task<IReadOnlyList<AttendanceMonthDto>> ListFineMonthsAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT StartDate, EndDate FROM dbo.Education_Year
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return [];
        var start = Convert.ToDateTime(reader["StartDate"]).Date;
        var end = Convert.ToDateTime(reader["EndDate"]).Date;
        var months = new List<AttendanceMonthDto>();
        var cursor = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);
        while (cursor <= last)
        {
            months.Add(new AttendanceMonthDto { Date = cursor, Name = cursor.ToString("MMM yyyy") });
            cursor = cursor.AddMonths(1);
        }
        return months;
    }

    public async Task<IReadOnlyList<AttendanceFineRowDto>> GenerateFineAsync(
        SessionSnapshot session, GenerateFineRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ClassID <= 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var proc = new SqlCommand("dbo.Student_Monthly_AttendanceFine", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        proc.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        proc.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        proc.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        proc.Parameters.AddWithValue("@ClassID", request.ClassID);
        proc.Parameters.AddWithValue("@Get_date", request.MonthDate.Date);
        proc.Parameters.AddWithValue("@MonthName", request.MonthName);
        await proc.ExecuteNonQueryAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
SELECT s.ID, s.StudentsName, r.MonthName, r.FineAmount, r.WorkingDays, r.TotalPresent, r.TotalAbsent,
       r.TotalLateAbs, r.Abs_Count, r.TotalLate, r.TotalLeave, r.TotalBunk
FROM dbo.Attendance_Monthly_Report AS r
INNER JOIN dbo.Student AS s ON r.StudentID = s.StudentID
WHERE r.SchoolID = @SchoolID AND r.ClassID = @ClassID AND r.MonthName = @MonthName
ORDER BY s.ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
        cmd.Parameters.AddWithValue("@MonthName", request.MonthName);
        var items = new List<AttendanceFineRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AttendanceFineRowDto
            {
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                MonthName = NullString(reader["MonthName"]),
                FineAmount = reader["FineAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["FineAmount"]),
                WorkingDays = ToInt(reader["WorkingDays"], 0),
                Present = ToInt(reader["TotalPresent"], 0),
                Absent = ToInt(reader["TotalAbsent"], 0),
                LateAbs = ToInt(reader["TotalLateAbs"], 0),
                AbsCount = ToInt(reader["Abs_Count"], 0),
                Late = ToInt(reader["TotalLate"], 0),
                Leave = ToInt(reader["TotalLeave"], 0),
                Bunk = ToInt(reader["TotalBunk"], 0)
            });
        }
        return items;
    }

    private static async Task<List<AttendanceScheduleDayDto>> ListDaysAsync(
        SqlConnection con, int schoolId, int scheduleId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT ScheduleDayID, Day, CONVERT(varchar(8), StartTime, 108) AS StartTime,
       CONVERT(varchar(8), LateEntryTime, 108) AS LateEntryTime,
       CONVERT(varchar(8), EndTime, 108) AS EndTime, Is_OnDay
FROM dbo.Attendance_Schedule_Day
WHERE SchoolID = @SchoolID AND ScheduleID = @ScheduleID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        var items = new List<AttendanceScheduleDayDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AttendanceScheduleDayDto
            {
                ScheduleDayID = Convert.ToInt32(reader["ScheduleDayID"]),
                Day = reader["Day"]?.ToString() ?? "",
                StartTime = TrimTime(reader["StartTime"]),
                LateEntryTime = TrimTime(reader["LateEntryTime"]),
                EndTime = TrimTime(reader["EndTime"]),
                IsOnDay = ToBool(reader["Is_OnDay"])
            });
        }
        return items;
    }

    private static async Task EnsureDaysAsync(
        SqlConnection con, SessionSnapshot session, int scheduleId, SaveScheduleRequest request,
        CancellationToken cancellationToken)
    {
        await using var check = new SqlCommand(
            "SELECT COUNT(*) FROM dbo.Attendance_Schedule_Day WHERE ScheduleID = @ID AND SchoolID = @SchoolID", con);
        check.Parameters.AddWithValue("@ID", scheduleId);
        check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var count = Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken));
        if (count > 0)
            return;
        foreach (var day in WeekDays)
        {
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Schedule_Day
    (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime, Insert_Date, Is_OnDay)
VALUES (@ScheduleID, @SchoolID, @RegistrationID, @Day, @Late, @Start, @End, GETDATE(), @On)
""", con);
            cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@Late", ParseTime(request.LateEntryTime));
            cmd.Parameters.AddWithValue("@Start", ParseTime(request.StartTime));
            cmd.Parameters.AddWithValue("@End", ParseTime(request.EndTime));
            cmd.Parameters.AddWithValue("@On", day is not "Friday");
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static void AddClassParams(
        SqlCommand cmd, SessionSnapshot session, int scheduleId, int classId, int groupId, int sectionId, int shiftId)
    {
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@GroupID", groupId);
        cmd.Parameters.AddWithValue("@SectionID", sectionId);
        cmd.Parameters.AddWithValue("@ShiftID", shiftId);
    }

    private static StudentLeaveRowDto ReadStudentLeave(SqlDataReader reader) => new()
    {
        StudentLeaveID = Convert.ToInt32(reader["StudentLeaveID"]),
        StartDate = Convert.ToDateTime(reader["StartDate"]).Date,
        EndDate = Convert.ToDateTime(reader["EndDate"]).Date,
        Description = NullString(reader["Description"]),
        LeaveType = HasColumn(reader, "LeaveType") ? NullString(reader["LeaveType"]) : null,
        GuardianName = HasColumn(reader, "GuardianName") ? NullString(reader["GuardianName"]) : null
    };

    private static async Task<IReadOnlyList<LeaveReportRowDto>> ReadEmployeeLeaveReportAsync(
        SqlCommand cmd, CancellationToken cancellationToken)
    {
        var items = new List<LeaveReportRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var start = Convert.ToDateTime(reader["LeaveStartDate"]).Date;
            var end = Convert.ToDateTime(reader["LeaveEndDate"]).Date;
            items.Add(new LeaveReportRowDto
            {
                LeaveID = Convert.ToInt32(reader["EmployeeLeaveID"]),
                Type = "Employee",
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                ClassOrDesignation = NullString(reader["Designation"]),
                StartDate = start,
                EndDate = end,
                Days = (end - start).Days + 1,
                Description = NullString(reader["LeaveReason"])
            });
        }
        return items;
    }

    private static LeaveReportRowDto ReadLeaveReport(SqlDataReader reader, bool withType)
    {
        var start = Convert.ToDateTime(reader["StartDate"]).Date;
        var end = Convert.ToDateTime(reader["EndDate"]).Date;
        return new LeaveReportRowDto
        {
            LeaveID = Convert.ToInt32(reader["StudentLeaveID"]),
            Type = "Student",
            ID = reader["ID"]?.ToString() ?? "",
            Name = reader["StudentsName"]?.ToString() ?? "",
            ClassOrDesignation = NullString(reader["ClassName"]),
            LeaveType = withType && HasColumn(reader, "LeaveType") ? NullString(reader["LeaveType"]) : null,
            StartDate = start,
            EndDate = end,
            Days = (end - start).Days + 1,
            Description = NullString(reader["Description"])
        };
    }

    private static StudentLeavePrintDto ReadStudentLeavePrint(SqlDataReader reader, SessionSnapshot session)
    {
        var start = Convert.ToDateTime(reader["StartDate"]).Date;
        var end = Convert.ToDateTime(reader["EndDate"]).Date;
        var className = NullString(reader["ClassName"]);
        var groupName = HasColumn(reader, "GroupName") ? NullString(reader["GroupName"]) : null;
        return new StudentLeavePrintDto
        {
            StudentLeaveID = Convert.ToInt32(reader["StudentLeaveID"]),
            ID = reader["StudentDisplayID"]?.ToString() ?? "",
            Name = reader["StudentsName"]?.ToString() ?? "",
            FathersName = NullString(reader["FathersName"]),
            ClassName = string.IsNullOrWhiteSpace(groupName) ? className : $"{className} ({groupName})",
            GroupName = groupName,
            StartDate = start,
            EndDate = end,
            Days = ToInt(reader["LeaveDays"], (end - start).Days + 1),
            LeaveType = HasColumn(reader, "LeaveType") ? NullString(reader["LeaveType"]) : null,
            GuardianName = HasColumn(reader, "GuardianName") ? NullString(reader["GuardianName"]) : null,
            Description = NullString(reader["Description"]),
            ApproverName = session.DisplayName,
            ApprovedOn = DateTime.Now,
            SchoolName = reader["SchoolName"]?.ToString() ?? session.SchoolName,
            SchoolAddress = NullString(reader["SchoolAddress"]),
            SchoolPhone = NullString(reader["SchoolPhone"])
        };
    }

    private static bool HasColumn(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static object ParseTime(string? value)
    {
        if (TimeSpan.TryParse(value, out var span))
            return span;
        if (DateTime.TryParse(value, out var date))
            return date.TimeOfDay;
        return TimeSpan.FromHours(8);
    }

    private static object ParseTimeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DBNull.Value;
        if (TimeSpan.TryParse(value, out var span))
            return span;
        if (DateTime.TryParse(value, out var date))
            return date.TimeOfDay;
        return DBNull.Value;
    }

    private static string TrimTime(object value)
    {
        var text = value is DBNull ? null : value?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return "";
        return text.Length >= 5 ? text[..5] : text;
    }

    private static string? NullString(object value)
    {
        var text = value is DBNull ? null : value?.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool ToBool(object value) =>
        value is not DBNull && Convert.ToBoolean(value);

    private static int ToInt(object value, int fallback)
    {
        if (value is null or DBNull)
            return fallback;
        return Convert.ToInt32(value);
    }

    private static bool Contains(string? source, string search) =>
        !string.IsNullOrWhiteSpace(source)
        && source.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static void AddFindParams(SqlCommand cmd, SessionSnapshot session, string code, string unpadded)
    {
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@Unpadded", unpadded);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
    }

    private static StudentLeavePersonDto ReadStudentLeavePerson(SqlDataReader reader, bool extra)
    {
        return new StudentLeavePersonDto
        {
            StudentID = Convert.ToInt32(reader["StudentID"]),
            ID = reader["ID"]?.ToString() ?? "",
            Name = reader["StudentsName"]?.ToString() ?? "",
            ClassName = extra ? NullString(reader["ClassName"]) : NullString(reader["Class"]),
            FathersName = NullString(reader["FathersName"]),
            Phone = NullString(reader["SMSPhoneNo"]),
            Gender = extra && HasColumn(reader, "Gender") ? NullString(reader["Gender"]) : null,
            Section = extra && HasColumn(reader, "Section") ? NullString(reader["Section"]) : null,
            GroupName = extra && HasColumn(reader, "GroupName") ? NullString(reader["GroupName"]) : null,
            Shift = extra && HasColumn(reader, "Shift") ? NullString(reader["Shift"]) : null,
            PhotoDataUrl = extra && HasColumn(reader, "Image") ? ToDataUrl(reader["Image"] as byte[]) : null
        };
    }

    private static string? ToDataUrl(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    private static AttendanceResult Fail(string error) => new() { Succeeded = false, Error = error };
    private static AttendanceResult Ok(int saved = 0, int id = 0) =>
        new() { Succeeded = true, Saved = saved, Id = id };

    private static AttendanceResult BuildAssignResult(int saved, List<string> errors)
    {
        if (errors.Count == 0)
            return Ok(saved);

        // One short schedule-pair line only (never list every person).
        var detail = errors
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault() ?? "";

        if (saved > 0)
        {
            return new AttendanceResult
            {
                Succeeded = true,
                Saved = saved,
                Error = "att.overlapPartial",
                Message = detail
            };
        }

        return new AttendanceResult
        {
            Succeeded = false,
            Error = "att.overlap",
            Message = detail,
            Saved = 0
        };
    }

    private static async Task<bool> IsStudentAssignedAsync(
        SqlConnection con, int schoolId, int studentId, int scheduleId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 1
FROM dbo.Attendance_Schedule_AssignStudent
WHERE SchoolID = @SchoolID AND StudentID = @PersonID AND ScheduleID = @ScheduleID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@PersonID", studentId);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null && result is not DBNull;
    }

    private static async Task<bool> IsEmployeeAssignedAsync(
        SqlConnection con, int schoolId, int employeeId, int scheduleId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 1
FROM dbo.Employee_Attendance_Schedule_Assign
WHERE SchoolID = @SchoolID AND EmployeeID = @PersonID AND ScheduleID = @ScheduleID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@PersonID", employeeId);
        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null && result is not DBNull;
    }

    /// <summary>
    /// Same rule as V2 ScheduleOverlapValidator: any overlap of schedule Start/End windows
    /// when the person is already on another schedule.
    /// </summary>
    private static async Task<string?> GetScheduleOverlapAsync(
        SqlConnection con, int schoolId, int personId, int targetScheduleId,
        string assignTable, string personColumn, CancellationToken cancellationToken)
    {
        // Table/column names are fixed callers only — never user input.
        await using var cmd = new SqlCommand($"""
SELECT TOP 1
    existing.ScheduleName AS ExistingScheduleName,
    existing.StartTime AS ExistingStart,
    existing.EndTime AS ExistingEnd,
    target.ScheduleName AS TargetScheduleName,
    target.StartTime AS TargetStart,
    target.EndTime AS TargetEnd
FROM dbo.{assignTable} AS assignTbl
INNER JOIN dbo.Attendance_Schedule AS existing
    ON existing.ScheduleID = assignTbl.ScheduleID AND existing.SchoolID = assignTbl.SchoolID
INNER JOIN dbo.Attendance_Schedule AS target
    ON target.ScheduleID = @TargetScheduleID AND target.SchoolID = @SchoolID
WHERE assignTbl.SchoolID = @SchoolID
  AND assignTbl.{personColumn} = @PersonID
  AND assignTbl.ScheduleID <> @TargetScheduleID
  AND existing.StartTime IS NOT NULL
  AND existing.EndTime IS NOT NULL
  AND target.StartTime IS NOT NULL
  AND target.EndTime IS NOT NULL
  AND existing.StartTime < target.EndTime
  AND target.StartTime < existing.EndTime
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@PersonID", personId);
        cmd.Parameters.AddWithValue("@TargetScheduleID", targetScheduleId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var existing = reader["ExistingScheduleName"]?.ToString() ?? "";
        var target = reader["TargetScheduleName"]?.ToString() ?? "";
        var eStart = FormatClock12(reader["ExistingStart"]);
        var eEnd = FormatClock12(reader["ExistingEnd"]);
        var tStart = FormatClock12(reader["TargetStart"]);
        var tEnd = FormatClock12(reader["TargetEnd"]);
        return $"'{existing}' ({eStart}-{eEnd}) / '{target}' ({tStart}-{tEnd})";
    }

    private static string FormatClock12(object? value)
    {
        if (value is null or DBNull)
            return "";
        var span = value switch
        {
            TimeSpan ts => ts,
            DateTime dt => dt.TimeOfDay,
            _ => TimeSpan.TryParse(value.ToString(), out var parsed) ? parsed : TimeSpan.Zero
        };
        return DateTime.Today.Add(span).ToString("h:mm tt", CultureInfo.InvariantCulture);
    }
}
