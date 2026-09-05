using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Sms;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SmsOfficeService
{
    private const decimal PerSmsRate = 0.36m;
    private const string ShurjoPayBase = "https://engine.shurjopayment.com";
    private const string ShurjoPayUser = "sikkhaloy";
    private const string ShurjoPayPassword = "sikkp22tmxq3499z";
    private const string ShurjoPayPrefix = "SIK";
    private const string RechargeReturnUrl = "https://sikkhaloy.com/Profile/Invoice/ShurjoPayCallback.aspx";
    private const string RechargeCancelUrl = "https://sikkhaloy.com/SMS/SMS_Recharge.aspx?cancelled=1";

    private readonly EduConnectionFactory _connections;
    private readonly LocalOfficeMode _local;
    private readonly OfficeSmsGateway _gateway;

    public SmsOfficeService(EduConnectionFactory connections, LocalOfficeMode local, OfficeSmsGateway gateway)
    {
        _connections = connections;
        _local = local;
        _gateway = gateway;
    }

    public bool IsLocal => _local.IsLocal;

    public async Task<(string? Body, string? Error, bool Local)> TrySendAsync(
        string phone, string text, CancellationToken ct)
    {
        if (_local.IsLocal)
            return ("Localhost - not sent to mobile", null, true);
        var call = await _gateway.SendAsync(phone, text, ct);
        return (call.Body, call.Error, false);
    }

    public async Task<SmsBalanceDto> GetBalanceAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return new SmsBalanceDto
        {
            Balance = await ReadBalanceAsync(con, session.SchoolID, ct),
            LocalMode = _local.IsLocal
        };
    }

    public async Task<IReadOnlyList<SmsStudentDto>> GetStudentsAsync(
        SessionSnapshot session, int classId, int groupId, int sectionId, int shiftId, string? ids, CancellationToken ct)
    {
        var code = (ids ?? "").Trim();
        var idList = SplitIds(code);
        if (idList.Count == 0 && classId <= 0)
            return [];

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand { Connection = con };
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        var sql = """
SELECT Student.StudentID, Student.ID, StudentsClass.RollNo, CreateClass.Class,
       Student.StudentsName, Student.Gender, Student.Religion, Student.SMSPhoneNo
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
INNER JOIN dbo.CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE Student.Status = N'Active'
  AND StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
""";
        if (idList.Count > 0)
        {
            var names = new List<string>();
            for (var i = 0; i < idList.Count; i++)
            {
                var name = "@ID" + i;
                names.Add(name);
                cmd.Parameters.AddWithValue(name, idList[i]);
            }
            sql += $" AND Student.ID IN ({string.Join(",", names)})";
        }
        else
        {
            sql += """
  AND StudentsClass.ClassID = @ClassID
  AND (@SectionID = 0 OR ISNULL(StudentsClass.SectionID, 0) = @SectionID)
  AND (@SubjectGroupID = 0 OR ISNULL(StudentsClass.SubjectGroupID, 0) = @SubjectGroupID)
  AND (@ShiftID = 0 OR ISNULL(StudentsClass.ShiftID, 0) = @ShiftID)
""";
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SectionID", sectionId);
            cmd.Parameters.AddWithValue("@SubjectGroupID", groupId);
            cmd.Parameters.AddWithValue("@ShiftID", shiftId);
        }
        sql += """
 ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1
              THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS FLOAT) ELSE 0 END
""";
        cmd.CommandText = sql;
        var rows = new List<SmsStudentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new SmsStudentDto
            {
                StudentID = Convert.ToInt32(reader["StudentID"]),
                ID = reader["ID"]?.ToString() ?? "",
                RollNo = reader["RollNo"]?.ToString(),
                ClassName = reader["Class"]?.ToString(),
                Name = reader["StudentsName"]?.ToString() ?? "",
                Gender = reader["Gender"]?.ToString(),
                Religion = reader["Religion"]?.ToString(),
                Phone = reader["SMSPhoneNo"]?.ToString()
            });
        }
        return rows;
    }

    public async Task<IReadOnlyList<SmsTeacherDto>> GetTeachersAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT EmployeeID, ID, FirstName + N' ' + LastName AS Name, Designation, Phone
FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID AND Job_Status = N'Active'
ORDER BY FirstName, LastName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = new List<SmsTeacherDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new SmsTeacherDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = (reader["Name"]?.ToString() ?? "").Trim(),
                Designation = reader["Designation"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString()
            });
        }
        return rows;
    }

    public async Task<SmsResult> SendAsync(SessionSnapshot session, SendOfficeSmsRequest? request, CancellationToken ct)
    {
        try
        {
            var text = (request?.Text ?? "").Trim();
            if (text.Length == 0)
                return Fail("sms.needText");

            var mode = (request?.Mode ?? "selected").Trim().ToLowerInvariant();
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            var jobs = await BuildJobsAsync(con, session, mode, request!, ct);
            if (jobs.Count == 0)
                return Fail("sms.needSelect");

            var valid = jobs.Where(x => x.Count > 0).ToList();
            if (valid.Count == 0)
                return Fail("sms.needPhone");

            var needed = valid.Sum(x => x.Count);
            var balance = await ReadBalanceAsync(con, session.SchoolID, ct);
            if (balance < needed)
                return new SmsResult { Error = "sms.low", Balance = balance };

            var sent = 0;
            var failed = 0;
            string? lastError = null;
            foreach (var job in jobs)
            {
                if (job.Count <= 0 || string.IsNullOrWhiteSpace(job.Phone))
                {
                    failed++;
                    continue;
                }
                var call = _local.IsLocal
                    ? new OfficeSmsGateway.GatewayCall("Localhost - not sent to mobile", null)
                    : await _gateway.SendAsync(job.Phone, text, ct);
                if (string.IsNullOrWhiteSpace(call.Body))
                {
                    failed++;
                    lastError = call.Error;
                    continue;
                }
                try
                {
                    var smsId = Guid.NewGuid();
                    await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, @Purpose, @Status, GETDATE(), @Resp)
""", con))
                    {
                        ins.Parameters.AddWithValue("@ID", smsId);
                        ins.Parameters.AddWithValue("@Phone", job.Phone);
                        ins.Parameters.AddWithValue("@Text", text);
                        ins.Parameters.AddWithValue("@Len", text.Length);
                        ins.Parameters.AddWithValue("@Count", job.Count);
                        ins.Parameters.AddWithValue("@Purpose", job.Purpose);
                        ins.Parameters.AddWithValue("@Status", _local.IsLocal ? "Local" : "Sent");
                        ins.Parameters.AddWithValue("@Resp", call.Body);
                        await ins.ExecuteNonQueryAsync(ct);
                    }
                    if (job.ContactId > 0)
                    {
                        await using var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, EducationYearID, SMS_NumberID)
VALUES (@ID, @SchoolID, @YearID, @NID)
""", con);
                        other.Parameters.AddWithValue("@ID", smsId);
                        other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                        other.Parameters.AddWithValue("@YearID", session.EducationYearID);
                        other.Parameters.AddWithValue("@NID", job.ContactId);
                        await other.ExecuteNonQueryAsync(ct);
                    }
                    else
                    {
                        await using var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID)
VALUES (@ID, @SchoolID, @SID, @TID, @YearID)
""", con);
                        other.Parameters.AddWithValue("@ID", smsId);
                        other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                        other.Parameters.AddWithValue("@SID", job.StudentId > 0 ? job.StudentId : DBNull.Value);
                        other.Parameters.AddWithValue("@TID", job.TeacherId > 0 ? job.TeacherId : DBNull.Value);
                        other.Parameters.AddWithValue("@YearID", session.EducationYearID);
                        await other.ExecuteNonQueryAsync(ct);
                    }
                    sent++;
                }
                catch (Exception ex)
                {
                    failed++;
                    lastError = ex.Message;
                }
            }

            return new SmsResult
            {
                Succeeded = sent > 0,
                Sent = sent,
                Failed = failed,
                Balance = await ReadBalanceAsync(con, session.SchoolID, ct),
                Error = sent > 0 ? null : (lastError ?? "sms.fail"),
                Message = sent > 0 && _local.IsLocal ? "sms.localSent" : lastError,
                LocalMode = _local.IsLocal
            };
        }
        catch (Exception ex)
        {
            return new SmsResult { Error = ex.Message, LocalMode = _local.IsLocal };
        }
    }

    public async Task<SmsResult> SendStudentLoginSmsAsync(
        SessionSnapshot session, StudentLoginSmsRequest? request, CancellationToken ct)
    {
        try
        {
            var ids = (request?.StudentIDs ?? []).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0)
                return Fail("sms.needSelect");

            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            var school = await SessionSchool.ResolveNameAsync(session, con, ct);
            var website = "www.sikkhaloy.com";
            await using (var webCmd = new SqlCommand(
                "SELECT LTRIM(RTRIM(ISNULL(Website, N''))) FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con))
            {
                webCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                var value = await webCmd.ExecuteScalarAsync(ct);
                var site = (value as string ?? "").Trim();
                if (site.Length > 0) website = site;
            }

            var names = ids.Select((_, i) => "@S" + i).ToArray();
            await using var cmd = new SqlCommand($"""
SELECT Student.StudentID, Student.StudentsName, ISNULL(Student.SMSPhoneNo, N'') AS SMSPhoneNo,
       ISNULL(Registration.UserName, N'') AS UserName, ISNULL(AST.Password, N'') AS Password
FROM dbo.Student
INNER JOIN dbo.Registration ON Registration.RegistrationID = Student.StudentRegistrationID
LEFT JOIN dbo.AST ON AST.RegistrationID = Student.StudentRegistrationID
WHERE Student.SchoolID = @SchoolID AND Student.StudentID IN ({string.Join(",", names)})
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            for (var i = 0; i < ids.Count; i++)
                cmd.Parameters.AddWithValue(names[i], ids[i]);

            var jobs = new List<(int StudentId, string Phone, string Text, int Count)>();
            await using (var reader = await cmd.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var name = reader["StudentsName"]?.ToString() ?? "";
                    var phone = reader["SMSPhoneNo"]?.ToString() ?? "";
                    var user = reader["UserName"]?.ToString() ?? "";
                    var pass = reader["Password"]?.ToString() ?? "";
                    var text = $"Dear, {name} Your Login Username Is: {user} And Password Is: {pass} Visit: {website} and Login. Regards: {school}";
                    jobs.Add((Convert.ToInt32(reader["StudentID"]), phone, text, IsValidBdMobile(phone) ? SmsCount(text) : 0));
                }
            }

            var valid = jobs.Where(x => x.Count > 0).ToList();
            if (valid.Count == 0)
                return Fail("sms.needPhone");
            var needed = valid.Sum(x => x.Count);
            var balance = await ReadBalanceAsync(con, session.SchoolID, ct);
            if (balance < needed)
                return new SmsResult { Error = "sms.low", Balance = balance };

            var sent = 0;
            var failed = 0;
            string? lastError = null;
            foreach (var job in jobs)
            {
                if (job.Count <= 0 || string.IsNullOrWhiteSpace(job.Phone))
                {
                    failed++;
                    continue;
                }
                var call = _local.IsLocal
                    ? new OfficeSmsGateway.GatewayCall("Localhost - not sent to mobile", null)
                    : await _gateway.SendAsync(job.Phone, job.Text, ct);
                if (string.IsNullOrWhiteSpace(call.Body))
                {
                    failed++;
                    lastError = call.Error;
                    continue;
                }
                try
                {
                    var smsId = Guid.NewGuid();
                    await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, N'Student User & Password', @Status, GETDATE(), @Resp)
""", con))
                    {
                        ins.Parameters.AddWithValue("@ID", smsId);
                        ins.Parameters.AddWithValue("@Phone", job.Phone);
                        ins.Parameters.AddWithValue("@Text", job.Text);
                        ins.Parameters.AddWithValue("@Len", job.Text.Length);
                        ins.Parameters.AddWithValue("@Count", job.Count);
                        ins.Parameters.AddWithValue("@Status", _local.IsLocal ? "Local" : "Sent");
                        ins.Parameters.AddWithValue("@Resp", call.Body);
                        await ins.ExecuteNonQueryAsync(ct);
                    }
                    await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID)
VALUES (@ID, @SchoolID, @SID, NULL, @YearID)
""", con))
                    {
                        other.Parameters.AddWithValue("@ID", smsId);
                        other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                        other.Parameters.AddWithValue("@SID", job.StudentId);
                        other.Parameters.AddWithValue("@YearID", session.EducationYearID);
                        await other.ExecuteNonQueryAsync(ct);
                    }
                    sent++;
                }
                catch (Exception ex)
                {
                    failed++;
                    lastError = ex.Message;
                }
            }

            return new SmsResult
            {
                Succeeded = sent > 0,
                Sent = sent,
                Failed = failed,
                Balance = await ReadBalanceAsync(con, session.SchoolID, ct),
                Error = sent > 0 ? null : (lastError ?? "sms.fail"),
                Message = sent > 0 && _local.IsLocal ? "sms.localSent" : lastError,
                LocalMode = _local.IsLocal
            };
        }
        catch (Exception ex)
        {
            return new SmsResult { Error = ex.Message, LocalMode = _local.IsLocal };
        }
    }

    public async Task<SmsResult> SendBirthdaySmsAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);

            await using (var check = new SqlCommand("""
SELECT TOP (1) SMS_Send_Record.SMS_Send_ID
FROM dbo.SMS_Send_Record
INNER JOIN dbo.SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID
WHERE SMS_OtherInfo.SchoolID = @SchoolID
  AND SMS_Send_Record.PurposeOfSMS = N'Birthday'
  AND CONVERT(date, SMS_Send_Record.Date) = CONVERT(date, GETDATE())
""", con))
            {
                check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                if (await check.ExecuteScalarAsync(ct) is not null)
                    return Fail("dash.bdayAlready");
            }

            var school = await SessionSchool.ResolveNameAsync(session, con, ct);
            await using var cmd = new SqlCommand("""
SELECT Student.StudentID, Student.StudentsName, ISNULL(Student.SMSPhoneNo, N'') AS SMSPhoneNo
FROM dbo.Student
INNER JOIN dbo.StudentsClass ON Student.StudentID = StudentsClass.StudentID
WHERE Student.DateofBirth IS NOT NULL
  AND MONTH(Student.DateofBirth) = MONTH(GETDATE())
  AND DAY(Student.DateofBirth) = DAY(GETDATE())
  AND Student.Status = N'Active'
  AND StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);

            var jobs = new List<(int StudentId, string Phone, string Text, int Count)>();
            await using (var reader = await cmd.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var name = (reader["StudentsName"]?.ToString() ?? "").Trim();
                    var phone = reader["SMSPhoneNo"]?.ToString() ?? "";
                    var text = "Happy birthday to you, " + name
                        + ". I wish you a successful future. Study hard and don't forget your ambitions in life. You'll certainly go places. Regards, "
                        + school;
                    jobs.Add((
                        Convert.ToInt32(reader["StudentID"]),
                        phone,
                        text,
                        IsValidBdMobile(phone) ? SmsCount(text) : 0));
                }
            }

            if (jobs.Count == 0)
                return Fail("dash.noBirthday");

            var valid = jobs.Where(x => x.Count > 0).ToList();
            if (valid.Count == 0)
                return Fail("sms.needPhone");

            var needed = valid.Sum(x => x.Count);
            var balance = await ReadBalanceAsync(con, session.SchoolID, ct);
            if (balance < needed)
                return new SmsResult { Error = "sms.low", Balance = balance };

            var sent = 0;
            var failed = 0;
            string? lastError = null;
            foreach (var job in jobs)
            {
                if (job.Count <= 0 || string.IsNullOrWhiteSpace(job.Phone))
                {
                    failed++;
                    continue;
                }

                var call = _local.IsLocal
                    ? new OfficeSmsGateway.GatewayCall("Localhost - not sent to mobile", null)
                    : await _gateway.SendAsync(job.Phone, job.Text, ct);
                if (string.IsNullOrWhiteSpace(call.Body))
                {
                    failed++;
                    lastError = call.Error;
                    continue;
                }

                try
                {
                    var smsId = Guid.NewGuid();
                    await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, N'Birthday', @Status, GETDATE(), @Resp)
""", con))
                    {
                        ins.Parameters.AddWithValue("@ID", smsId);
                        ins.Parameters.AddWithValue("@Phone", job.Phone);
                        ins.Parameters.AddWithValue("@Text", job.Text);
                        ins.Parameters.AddWithValue("@Len", job.Text.Length);
                        ins.Parameters.AddWithValue("@Count", job.Count);
                        ins.Parameters.AddWithValue("@Status", _local.IsLocal ? "Local" : "Sent");
                        ins.Parameters.AddWithValue("@Resp", call.Body);
                        await ins.ExecuteNonQueryAsync(ct);
                    }

                    await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID)
VALUES (@ID, @SchoolID, @SID, NULL, @YearID)
""", con))
                    {
                        other.Parameters.AddWithValue("@ID", smsId);
                        other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                        other.Parameters.AddWithValue("@SID", job.StudentId);
                        other.Parameters.AddWithValue("@YearID", session.EducationYearID);
                        await other.ExecuteNonQueryAsync(ct);
                    }

                    sent++;
                }
                catch (Exception ex)
                {
                    failed++;
                    lastError = ex.Message;
                }
            }

            return new SmsResult
            {
                Succeeded = sent > 0,
                Sent = sent,
                Failed = failed,
                Balance = await ReadBalanceAsync(con, session.SchoolID, ct),
                Error = sent > 0 ? null : (lastError ?? "sms.fail"),
                Message = sent > 0 && _local.IsLocal ? "sms.localSent" : lastError,
                LocalMode = _local.IsLocal
            };
        }
        catch (Exception ex)
        {
            return new SmsResult { Error = ex.Message, LocalMode = _local.IsLocal };
        }
    }

    public async Task<IReadOnlyList<SmsGroupDto>> GetGroupsAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT SMS_GroupID, GroupName FROM dbo.SMS_Group_Name
WHERE SchoolID = @SchoolID ORDER BY GroupName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = new List<SmsGroupDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new SmsGroupDto
            {
                SMS_GroupID = Convert.ToInt32(reader["SMS_GroupID"]),
                GroupName = reader["GroupName"]?.ToString() ?? ""
            });
        }
        return rows;
    }

    public async Task<SmsResult> SaveGroupAsync(SessionSnapshot session, SaveSmsGroupRequest? request, CancellationToken ct)
    {
        var name = (request?.GroupName ?? "").Trim();
        if (name.Length == 0)
            return Fail("sms.needGroup");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request!.SMS_GroupID > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.SMS_Group_Name SET GroupName = @Name
WHERE SMS_GroupID = @ID AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@Name", name);
            upd.Parameters.AddWithValue("@ID", request.SMS_GroupID);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(ct);
            return new SmsResult { Succeeded = true, Count = request.SMS_GroupID };
        }
        await using var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Group_Name (RegistrationID, SchoolID, GroupName)
VALUES (@RID, @SchoolID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con);
        ins.Parameters.AddWithValue("@RID", session.RegistrationID);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@Name", name);
        var id = Convert.ToInt32(await ins.ExecuteScalarAsync(ct));
        return new SmsResult { Succeeded = true, Count = id };
    }

    public async Task<SmsResult> DeleteGroupAsync(SessionSnapshot session, int groupId, CancellationToken ct)
    {
        if (groupId <= 0)
            return Fail("sms.needGroup");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var delNum = new SqlCommand("""
DELETE FROM dbo.SMS_Group_Phone_Number WHERE SMS_GroupID = @ID AND SchoolID = @SchoolID
""", con))
        {
            delNum.Parameters.AddWithValue("@ID", groupId);
            delNum.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await delNum.ExecuteNonQueryAsync(ct);
        }
        await using var del = new SqlCommand("""
DELETE FROM dbo.SMS_Group_Name WHERE SMS_GroupID = @ID AND SchoolID = @SchoolID
""", con);
        del.Parameters.AddWithValue("@ID", groupId);
        del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await del.ExecuteNonQueryAsync(ct);
        return new SmsResult { Succeeded = true };
    }

    public async Task<IReadOnlyList<SmsContactDto>> GetContactsAsync(
        SessionSnapshot session, int groupId, string? search, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT n.SMS_NumberID, n.SMS_GroupID, ISNULL(n.Name, N'') AS Name, n.MobileNo, n.Add_Date, n.Address,
       g.GroupName
FROM dbo.SMS_Group_Phone_Number AS n
INNER JOIN dbo.SMS_Group_Name AS g ON n.SMS_GroupID = g.SMS_GroupID
WHERE n.SchoolID = @SchoolID
  AND (n.SMS_GroupID = @GID OR @GID = 0)
  AND (@Q = N'' OR n.MobileNo LIKE N'%' + @Q + N'%' OR n.Name LIKE N'%' + @Q + N'%')
ORDER BY n.SMS_GroupID, n.Name
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@GID", groupId);
        cmd.Parameters.AddWithValue("@Q", (search ?? "").Trim());
        var rows = new List<SmsContactDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new SmsContactDto
            {
                SMS_NumberID = Convert.ToInt32(reader["SMS_NumberID"]),
                SMS_GroupID = Convert.ToInt32(reader["SMS_GroupID"]),
                GroupName = reader["GroupName"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                MobileNo = reader["MobileNo"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString(),
                Add_Date = reader["Add_Date"] is DBNull ? null : Convert.ToDateTime(reader["Add_Date"])
            });
        }
        return rows;
    }

    public async Task<SmsResult> SaveContactAsync(SessionSnapshot session, SaveSmsContactRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        var mobile = (request?.MobileNo ?? "").Trim();
        if (request is null || request.SMS_GroupID <= 0)
            return Fail("sms.needGroup");
        if (name.Length == 0)
            return Fail("sms.needName");
        if (!IsValidBdMobile(mobile))
            return Fail("sms.badMobile");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request.SMS_NumberID > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.SMS_Group_Phone_Number
SET Name = @Name, MobileNo = @Mobile, Address = @Address
WHERE SMS_NumberID = @ID AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@Name", name);
            upd.Parameters.AddWithValue("@Mobile", mobile);
            upd.Parameters.AddWithValue("@Address", (object?)request.Address ?? DBNull.Value);
            upd.Parameters.AddWithValue("@ID", request.SMS_NumberID);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(ct);
            return new SmsResult { Succeeded = true, Message = "sms.contactUpdated" };
        }
        await using var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Group_Phone_Number(SchoolID, RegistrationID, SMS_GroupID, Name, MobileNo, Address, Add_Date)
VALUES (@SchoolID, @RID, @GID, @Name, @Mobile, @Address, GETDATE())
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@RID", session.RegistrationID);
        ins.Parameters.AddWithValue("@GID", request.SMS_GroupID);
        ins.Parameters.AddWithValue("@Name", name);
        ins.Parameters.AddWithValue("@Mobile", mobile);
        ins.Parameters.AddWithValue("@Address", (object?)request.Address ?? DBNull.Value);
        await ins.ExecuteNonQueryAsync(ct);
        return new SmsResult { Succeeded = true, Message = "sms.contactAdded" };
    }

    public async Task<SmsResult> DeleteContactAsync(SessionSnapshot session, int numberId, CancellationToken ct)
    {
        if (numberId <= 0)
            return Fail("sms.needContact");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.SMS_Group_Phone_Number WHERE SMS_NumberID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", numberId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(ct);
        return new SmsResult { Succeeded = true };
    }

    public async Task<SmsRecordsDto> GetRecordsAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, string? search, string? kind, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 50);
        var q = (search ?? "").Trim();
        if (q.Length > 80) q = q[..80];
        var like = q.Length == 0 ? "" : "%" + EscapeLike(q) + "%";
        var kindKey = (kind ?? "").Trim().ToLowerInvariant();
        if (kindKey is not ("group" or "individual" or "system")) kindKey = "";
        var fromDt = from?.Date ?? new DateTime(2000, 1, 1);
        var toEx = (to?.Date ?? DateTime.Today).AddDays(1);
        var skip = (page - 1) * pageSize;

        const string Filter = """
FROM dbo.SMS_Send_Record AS r
INNER JOIN dbo.SMS_OtherInfo AS o ON r.SMS_Send_ID = o.SMS_Send_ID
WHERE o.SchoolID = @SchoolID
  AND r.Date >= @From AND r.Date < @ToEx
  AND (@Q = N'' OR r.PhoneNumber LIKE @Like OR LEFT(r.PurposeOfSMS, 120) LIKE @Like)
  AND (
        @Kind = N''
        OR CASE
            WHEN o.SMS_NumberID IS NOT NULL THEN N'group'
            WHEN o.StudentID IS NOT NULL OR o.TeacherID IS NOT NULL OR o.CommitteeMemberId IS NOT NULL THEN N'individual'
            ELSE N'system'
           END = @Kind
      )
""";

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new SmsRecordsDto
        {
            Balance = await ReadBalanceAsync(con, session.SchoolID, ct),
            LocalMode = _local.IsLocal,
            PerSmsRate = PerSmsRate,
            Page = page,
            PageSize = pageSize
        };

        await using (var stats = new SqlCommand($"""
SELECT COUNT_BIG(*) AS Recipients,
       ISNULL(SUM(r.SMSCount), 0) AS TotalSent,
       ISNULL(SUM(CASE WHEN ISNULL(r.Status, N'') IN (N'Sent', N'Local') THEN r.SMSCount ELSE 0 END), 0) AS Successful,
       ISNULL(SUM(CASE WHEN ISNULL(r.Status, N'') IN (N'Sent', N'Local') THEN 0 ELSE ISNULL(r.SMSCount, 0) END), 0) AS Failed
{Filter}
""", con))
        {
            stats.CommandTimeout = 45;
            BindRecordFilter(stats, session.SchoolID, fromDt, toEx, q, like, kindKey);
            await using var reader = await stats.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.TotalRecipients = ToInt(reader["Recipients"]);
                dto.TotalSent = ToInt(reader["TotalSent"]);
                dto.DistinctRecipients = dto.TotalRecipients;
                dto.Successful = ToInt(reader["Successful"]);
                dto.Failed = ToInt(reader["Failed"]);
            }
        }

        dto.RowCount = dto.TotalRecipients;
        dto.TotalCost = Math.Round(dto.TotalSent * PerSmsRate, 2);
        dto.TotalPages = dto.RowCount <= 0 ? 1 : (int)Math.Ceiling(dto.RowCount / (double)pageSize);
        if (page > dto.TotalPages)
        {
            page = dto.TotalPages;
            dto.Page = page;
            skip = (page - 1) * pageSize;
        }

        await using var cmd = new SqlCommand($"""
SELECT r.SMS_Send_ID, r.PhoneNumber, r.TextSMS, r.TextCount, r.SMSCount, r.PurposeOfSMS, r.Date, r.Status,
       r.Kind, r.RecipientName, r.RecipientCode
FROM (
    SELECT r.SMS_Send_ID, r.PhoneNumber,
           ISNULL(r.TextSMS, N'') AS TextSMS,
           r.TextCount, r.SMSCount,
           LEFT(ISNULL(r.PurposeOfSMS, N''), 80) AS PurposeOfSMS,
           r.Date, ISNULL(r.Status, N'') AS Status,
           CASE
             WHEN o.SMS_NumberID IS NOT NULL THEN N'group'
             WHEN o.StudentID IS NOT NULL OR o.TeacherID IS NOT NULL OR o.CommitteeMemberId IS NOT NULL THEN N'individual'
             ELSE N'system'
           END AS Kind,
           COALESCE(
             NULLIF(LTRIM(RTRIM(st.StudentsName)), N''),
             NULLIF(LTRIM(RTRIM(ISNULL(t.FirstName, N'') + N' ' + ISNULL(t.LastName, N''))), N''),
             NULLIF(LTRIM(RTRIM(ISNULL(sf.FirstName, N'') + N' ' + ISNULL(sf.LastName, N''))), N''),
             NULLIF(LTRIM(RTRIM(cm.MemberName)), N''),
             NULLIF(LTRIM(RTRIM(cn.Name)), N''),
             r.PhoneNumber
           ) AS RecipientName,
           ISNULL(st.ID, N'') AS RecipientCode
    FROM (
        SELECT r.SMS_Send_ID
        {Filter}
        ORDER BY r.Date DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
    ) AS ids
    INNER JOIN dbo.SMS_Send_Record AS r ON r.SMS_Send_ID = ids.SMS_Send_ID
    INNER JOIN dbo.SMS_OtherInfo AS o ON o.SMS_Send_ID = ids.SMS_Send_ID
    LEFT JOIN dbo.Student AS st ON st.StudentID = o.StudentID
    OUTER APPLY (SELECT TOP 1 FirstName, LastName FROM dbo.Teacher WHERE TeacherID = o.TeacherID OR EmployeeID = o.TeacherID) AS t
    OUTER APPLY (SELECT TOP 1 FirstName, LastName FROM dbo.Staff_Info WHERE EmployeeID = o.TeacherID) AS sf
    LEFT JOIN dbo.CommitteeMember AS cm ON cm.CommitteeMemberId = o.CommitteeMemberId
    LEFT JOIN dbo.SMS_Group_Phone_Number AS cn ON cn.SMS_NumberID = o.SMS_NumberID
) AS r
ORDER BY r.Date DESC
""", con);
        cmd.CommandTimeout = 45;
        BindRecordFilter(cmd, session.SchoolID, fromDt, toEx, q, like, kindKey);
        cmd.Parameters.AddWithValue("@Skip", skip);
        cmd.Parameters.AddWithValue("@Take", pageSize);
        await using var rows = await cmd.ExecuteReaderAsync(ct);
        while (await rows.ReadAsync(ct))
        {
            dto.Rows.Add(new SmsRecordDto
            {
                SMS_Send_ID = rows["SMS_Send_ID"] is Guid g ? g : Guid.Parse(rows["SMS_Send_ID"].ToString()!),
                PhoneNumber = rows["PhoneNumber"]?.ToString() ?? "",
                RecipientName = rows["RecipientName"]?.ToString() ?? "",
                RecipientCode = rows["RecipientCode"]?.ToString() ?? "",
                TextSMS = rows["TextSMS"]?.ToString() ?? "",
                TextCount = rows["TextCount"] is DBNull ? 0 : Convert.ToInt32(rows["TextCount"]),
                SMSCount = rows["SMSCount"] is DBNull ? 0 : Convert.ToInt32(rows["SMSCount"]),
                PurposeOfSMS = rows["PurposeOfSMS"]?.ToString() ?? "",
                Kind = rows["Kind"]?.ToString() ?? "",
                Status = rows["Status"]?.ToString() ?? "",
                Date = rows["Date"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(rows["Date"])
            });
        }
        return dto;
    }

    private static void BindRecordFilter(
        SqlCommand cmd, int schoolId, DateTime fromDt, DateTime toEx, string q, string like, string kind)
    {
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@From", fromDt);
        cmd.Parameters.AddWithValue("@ToEx", toEx);
        cmd.Parameters.AddWithValue("@Q", q);
        cmd.Parameters.AddWithValue("@Like", like);
        cmd.Parameters.AddWithValue("@Kind", kind);
    }

    private static string EscapeLike(string value) =>
        value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    private static int ToInt(object value) =>
        value is DBNull ? 0 : Convert.ToInt32(Convert.ToDecimal(value));

    public async Task<SmsRechargePageDto> GetRechargeAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new SmsRechargePageDto
        {
            Balance = await ReadBalanceAsync(con, session.SchoolID, ct),
            PerSmsRate = PerSmsRate,
            LocalMode = _local.IsLocal
        };
        await using (var hist = new SqlCommand("""
SELECT TOP 100 r.SMS_Recharge_RecordID, r.RechargeSMS, r.PerSMS_Price, r.Total_Price, r.Date,
       ISNULL(r.Is_Paid, 0) AS Is_Paid, reg.UserName
FROM dbo.SMS_Recharge_Record AS r
LEFT JOIN dbo.Registration AS reg ON reg.RegistrationID = r.RegistrationID
WHERE r.SchoolID = @SchoolID
ORDER BY r.Date DESC
""", con))
        {
            hist.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await hist.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.History.Add(new SmsRechargeRowDto
                {
                    SMS_Recharge_RecordID = Convert.ToInt32(reader["SMS_Recharge_RecordID"]),
                    RechargeSMS = reader["RechargeSMS"] is DBNull ? 0 : Convert.ToInt32(reader["RechargeSMS"]),
                    PerSMS_Price = reader["PerSMS_Price"] is DBNull ? 0 : Convert.ToDecimal(reader["PerSMS_Price"]),
                    Total_Price = reader["Total_Price"] is DBNull ? 0 : Convert.ToDecimal(reader["Total_Price"]),
                    Date = Convert.ToDateTime(reader["Date"]),
                    UserName = reader["UserName"]?.ToString(),
                    Is_Paid = reader["Is_Paid"] is not DBNull && Convert.ToBoolean(reader["Is_Paid"])
                });
            }
        }
        try
        {
            await using var due = new SqlCommand("""
SELECT i.Invoice_SN, i.Invoice_For, i.Unit, i.UnitPrice, i.TotalAmount, i.PaidAmount,
       (i.TotalAmount - i.PaidAmount - ISNULL(i.Discount, 0)) AS Due, i.IssuDate, i.EndDate
FROM dbo.AAP_Invoice AS i
INNER JOIN dbo.AAP_Invoice_Category AS c ON i.InvoiceCategoryID = c.InvoiceCategoryID
WHERE i.SchoolID = @SchoolID AND i.IsPaid = 0 AND c.InvoiceCategory = N'SMS'
ORDER BY i.IssuDate DESC
""", con);
            due.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await due.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.DueInvoices.Add(new SmsDueInvoiceDto
                {
                    Invoice_SN = reader["Invoice_SN"]?.ToString() ?? "",
                    Invoice_For = reader["Invoice_For"]?.ToString() ?? "",
                    Unit = reader["Unit"] is DBNull ? 0 : Convert.ToDecimal(reader["Unit"]),
                    UnitPrice = reader["UnitPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["UnitPrice"]),
                    TotalAmount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                    PaidAmount = reader["PaidAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["PaidAmount"]),
                    Due = reader["Due"] is DBNull ? 0 : Convert.ToDecimal(reader["Due"]),
                    IssuDate = reader["IssuDate"] is DBNull ? null : Convert.ToDateTime(reader["IssuDate"]),
                    EndDate = reader["EndDate"] is DBNull ? null : Convert.ToDateTime(reader["EndDate"])
                });
            }
        }
        catch
        {
        }
        return dto;
    }

    public async Task<SmsResult> StartRechargeAsync(SessionSnapshot session, SmsRechargeRequest? request, CancellationToken ct)
    {
        var qty = request?.Quantity ?? 0;
        if (qty <= 0)
            return Fail("sms.needQty");
        if (_local.IsLocal)
            return new SmsResult { Succeeded = true, Count = qty, LocalMode = true, Message = "sms.localPay" };

        var invoice = qty * PerSmsRate;
        var gateway = Math.Round(invoice / 1000m * 19m, 2);
        var total = invoice + gateway;

        string schoolName = session.SchoolName, phone = "01700000000", email = "info@school.com", address = "Dhaka";
        await using (var con = _connections.Create())
        {
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT TOP 1 SchoolName, Phone, Email, Address FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                schoolName = reader["SchoolName"]?.ToString() ?? schoolName;
                phone = FirstPhone(reader["Phone"]?.ToString()) ?? phone;
                email = string.IsNullOrWhiteSpace(reader["Email"]?.ToString()) ? email : reader["Email"]!.ToString()!;
                address = string.IsNullOrWhiteSpace(reader["Address"]?.ToString()) ? address : reader["Address"]!.ToString()!;
            }
        }
        if (schoolName.Length > 50)
            schoolName = schoolName[..50];

        var note = "SMS_RECHARGE|" + qty + "|" + session.RegistrationID + "|" + session.SchoolID;
        try
        {
            var url = await CreateShurjoPayOrderAsync(session.SchoolID, total, invoice, schoolName, phone, email, address, note, ct);
            if (string.IsNullOrWhiteSpace(url))
                return Fail("sms.payFail");
            return new SmsResult { Succeeded = true, CheckoutUrl = url, Count = qty };
        }
        catch (Exception ex)
        {
            return new SmsResult { Error = ex.Message };
        }
    }

    private async Task<List<SmsJob>> BuildJobsAsync(
        SqlConnection con, SessionSnapshot session, string mode, SendOfficeSmsRequest request, CancellationToken ct)
    {
        var jobs = new List<SmsJob>();
        var count = SmsCount(request.Text);
        if (mode is "all")
        {
            await using var cmd = new SqlCommand("""
SELECT Student.StudentID, Student.SMSPhoneNo
FROM dbo.Student
INNER JOIN dbo.StudentsClass ON Student.StudentID = StudentsClass.StudentID
WHERE StudentsClass.EducationYearID = @YearID AND Student.Status = N'Active' AND StudentsClass.SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var phone = reader["SMSPhoneNo"]?.ToString() ?? "";
                jobs.Add(new SmsJob(phone, IsValidBdMobile(phone) ? count : 0, Convert.ToInt32(reader["StudentID"]), 0, 0, "SMS Service"));
            }
            return jobs;
        }
        if (mode is "numbers")
        {
            foreach (var raw in (request.Phones ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var phone = raw.Trim();
                if (phone.Length == 0) continue;
                jobs.Add(new SmsJob(phone, IsValidBdMobile(phone) ? count : 0, 0, 0, 0, "SMS Service"));
            }
            return jobs;
        }
        if (mode is "teachers")
        {
            var ids = (request.TeacherIds ?? []).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0) return jobs;
            await using var cmd = new SqlCommand($"""
SELECT EmployeeID, Phone FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID AND EmployeeID IN ({string.Join(",", ids.Select((_, i) => "@T" + i))})
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            for (var i = 0; i < ids.Count; i++)
                cmd.Parameters.AddWithValue("@T" + i, ids[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var phone = reader["Phone"]?.ToString() ?? "";
                jobs.Add(new SmsJob(phone, IsValidBdMobile(phone) ? count : 0, 0, Convert.ToInt32(reader["EmployeeID"]), 0, "SMS Service"));
            }
            return jobs;
        }
        if (mode is "committee")
        {
            var ids = (request.CommitteeMemberIds ?? []).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0) return jobs;
            await using var cmd = new SqlCommand($"""
SELECT CommitteeMemberId, SmsNumber FROM dbo.CommitteeMember
WHERE SchoolID = @SchoolID AND CommitteeMemberId IN ({string.Join(",", ids.Select((_, i) => "@M" + i))})
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            for (var i = 0; i < ids.Count; i++)
                cmd.Parameters.AddWithValue("@M" + i, ids[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var phone = reader["SmsNumber"]?.ToString() ?? "";
                jobs.Add(new SmsJob(phone, IsValidBdMobile(phone) ? count : 0, 0, 0, 0, "Committee SMS"));
            }
            return jobs;
        }
        if (mode is "contacts")
        {
            var ids = (request.ContactIds ?? []).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0) return jobs;
            await using var cmd = new SqlCommand($"""
SELECT SMS_NumberID, MobileNo FROM dbo.SMS_Group_Phone_Number
WHERE SchoolID = @SchoolID AND SMS_NumberID IN ({string.Join(",", ids.Select((_, i) => "@C" + i))})
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            for (var i = 0; i < ids.Count; i++)
                cmd.Parameters.AddWithValue("@C" + i, ids[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var phone = reader["MobileNo"]?.ToString() ?? "";
                jobs.Add(new SmsJob(phone, IsValidBdMobile(phone) ? count : 0, 0, 0, Convert.ToInt32(reader["SMS_NumberID"]), "Others SMS"));
            }
            return jobs;
        }

        var studentIds = (request.StudentIds ?? []).Where(x => x > 0).Distinct().ToList();
        if (studentIds.Count == 0) return jobs;
        await using (var cmd = new SqlCommand($"""
SELECT StudentID, SMSPhoneNo FROM dbo.Student
WHERE SchoolID = @SchoolID AND StudentID IN ({string.Join(",", studentIds.Select((_, i) => "@S" + i))})
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            for (var i = 0; i < studentIds.Count; i++)
                cmd.Parameters.AddWithValue("@S" + i, studentIds[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var phone = reader["SMSPhoneNo"]?.ToString() ?? "";
                jobs.Add(new SmsJob(phone, IsValidBdMobile(phone) ? count : 0, Convert.ToInt32(reader["StudentID"]), 0, 0, "SMS Service"));
            }
        }
        return jobs;
    }

    private static async Task<int> ReadBalanceAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task<string?> CreateShurjoPayOrderAsync(
        int schoolId, decimal amount, decimal invoiceAmt, string name, string phone, string email, string address,
        string note, CancellationToken ct)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using (var tokenBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = ShurjoPayUser,
            ["password"] = ShurjoPayPassword
        }))
        using (var tokenRes = await http.PostAsync(ShurjoPayBase + "/api/get_token", tokenBody, ct))
        {
            var tokenJson = await tokenRes.Content.ReadAsStringAsync(ct);
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var root = tokenDoc.RootElement;
            var token = JsonText(root, "token");
            var storeId = JsonText(root, "store_id") ?? "";
            var execute = JsonText(root, "execute_url");
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("ShurjoPay token নেওয়া সম্ভব হয়নি।");

            var orderId = "SMSR_" + schoolId + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var payUrl = string.IsNullOrWhiteSpace(execute) ? ShurjoPayBase + "/api/secret-pay" : execute;
            using var payReq = new HttpRequestMessage(HttpMethod.Post, payUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["token"] = token!,
                    ["store_id"] = storeId ?? "",
                    ["prefix"] = ShurjoPayPrefix,
                    ["currency"] = "BDT",
                    ["return_url"] = RechargeReturnUrl,
                    ["cancel_url"] = RechargeCancelUrl,
                    ["amount"] = amount.ToString("F2"),
                    ["order_id"] = orderId,
                    ["discount_amount"] = "0",
                    ["disc_percent"] = "0",
                    ["client_ip"] = "127.0.0.1",
                    ["customer_name"] = name,
                    ["customer_phone"] = phone,
                    ["customer_email"] = email,
                    ["customer_address"] = address,
                    ["customer_city"] = "Dhaka",
                    ["customer_state"] = "Dhaka",
                    ["customer_postcode"] = "1200",
                    ["customer_country"] = "Bangladesh",
                    ["value1"] = schoolId.ToString(),
                    ["value2"] = note.Length > 250 ? note[..250] : note,
                    ["value3"] = invoiceAmt.ToString("F2"),
                    ["value4"] = ""
                })
            };
            payReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var payRes = await http.SendAsync(payReq, ct);
            var payJson = await payRes.Content.ReadAsStringAsync(ct);
            var trimmed = payJson.Trim();
            if (!trimmed.StartsWith('{') && !trimmed.StartsWith('['))
                throw new InvalidOperationException("ShurjoPay গেটওয়ে একটি অকার্যকর রেসপন্স দিয়েছে।");
            using var payDoc = JsonDocument.Parse(trimmed);
            var pay = payDoc.RootElement;
            var checkout = JsonText(pay, "checkout_url") ?? JsonText(pay, "payment_url");
            if (string.IsNullOrWhiteSpace(checkout))
            {
                var msg = JsonText(pay, "message")
                          ?? JsonText(pay, "sp_massage")
                          ?? "checkout_url পাওয়া যায়নি।";
                throw new InvalidOperationException("ShurjoPay গেটওয়ে এরর: " + msg);
            }
            return checkout;
        }
    }

    private static List<string> SplitIds(string text) =>
        text.Split([',', ' ', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(400)
            .ToList();

    private static string? FirstPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Split([',', '/', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
    }

    private static int SmsCount(string text)
    {
        var unicode = text.Any(ch => ch > 127);
        var size = unicode ? 70 : 160;
        return Math.Max(1, (int)Math.Ceiling(text.Length / (double)size));
    }

    private static bool IsValidBdMobile(string phone)
    {
        var digits = new string((phone ?? "").Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits.StartsWith("01", StringComparison.Ordinal)
               || digits.Length == 13 && digits.StartsWith("8801", StringComparison.Ordinal);
    }

    private static string? JsonText(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? JsonText(value) : null;

    private static string? JsonText(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => value.GetRawText()
    };

    private static string TrimErr(string text)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= 240 ? trimmed : trimmed[..240];
    }

    private static SmsResult Fail(string error) => new() { Error = error };

    private readonly record struct SmsJob(string Phone, int Count, int StudentId, int TeacherId, int ContactId, string Purpose);
}
