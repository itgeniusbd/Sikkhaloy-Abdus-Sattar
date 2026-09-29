using System.Net;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Exam;
using Sikkhaloy.Shared.Sms;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class SmsOfficeService
{
    public async Task<SmsResult> SendExamMeritSmsAsync(SessionSnapshot session, ExamMeritSmsRequest? request, CancellationToken ct)
    {
        try
        {
            var ids = (request?.StudentResultIDs ?? []).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0)
                return Fail("exam.smsNeedSelect");
            if (request is null || request.ExamID <= 0)
                return Fail("exam.selectExam");

            await using var con = _connections.Create();
            await con.OpenAsync(ct);

            var examName = await ScalarTextAsync(con, """
SELECT ExamName FROM dbo.Exam_Name
WHERE ExamID = @ExamID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", session, request.ExamID, ct) ?? "";
            if (string.IsNullOrWhiteSpace(examName))
                return Fail("exam.selectExam");

            var school = await SessionSchool.ResolveNameAsync(session, con, ct);
            var passedTemplate = await ReadExamResultTemplateAsync(con, session.SchoolID, "Passed", ct);
            var failedTemplate = await ReadExamResultTemplateAsync(con, session.SchoolID, "Failed", ct);

            var rows = await LoadMeritSmsRowsAsync(con, session, request.ExamID, ids, ct);
            if (rows.Count == 0)
                return Fail("exam.smsNeedSelect");

            var jobs = new List<(int StudentId, string Phone, string Text, int Count)>();
            foreach (var row in rows)
            {
                var passed = string.Equals(row.PassStatus, "P", StringComparison.OrdinalIgnoreCase);
                var text = passed
                    ? BuildMeritMessage(passedTemplate, row, examName, school, passed: true)
                    : BuildMeritMessage(failedTemplate, row, examName, school, passed: false);
                var phone = row.Phone ?? "";
                jobs.Add((row.StudentID, phone, text, IsValidBdMobile(phone) ? SmsCount(text) : 0));
            }

            var valid = jobs.Where(x => x.Count > 0).ToList();
            if (valid.Count == 0)
                return Fail("sms.needPhone");

            var needed = valid.Sum(x => x.Count);
            var balance = await ReadBalanceAsync(con, session.SchoolID, ct);
            if (balance < needed)
                return new SmsResult { Error = "sms.low", Balance = balance, Message = balance.ToString() };

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
    (@ID, @Phone, @Text, @Len, @Count, N'Position Result', @Status, GETDATE(), @Resp)
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
                Message = sent > 0 ? null : lastError,
                LocalMode = _local.IsLocal
            };
        }
        catch (Exception ex)
        {
            return new SmsResult { Error = ex.Message, LocalMode = _local.IsLocal };
        }
    }

    public async Task<SmsResult> SendExamCumulativeMeritSmsAsync(SessionSnapshot session, ExamMeritSmsRequest? request, CancellationToken ct)
    {
        try
        {
            var ids = (request?.StudentResultIDs ?? []).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0)
                return Fail("exam.smsNeedSelect");
            if (request is null || request.ExamID <= 0)
                return Fail("exam.selectExam");

            await using var con = _connections.Create();
            await con.OpenAsync(ct);

            var examName = await ScalarTextAsync(con, """
SELECT CumulativeResultName FROM dbo.Exam_Cumulative_Name
WHERE CumulativeNameID = @ExamID AND SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", session, request.ExamID, ct) ?? "";
            if (string.IsNullOrWhiteSpace(examName))
                return Fail("exam.selectExam");

            var school = await SessionSchool.ResolveNameAsync(session, con, ct);
            var passedTemplate = await ReadExamResultTemplateAsync(con, session.SchoolID, "Passed", ct);
            var failedTemplate = await ReadExamResultTemplateAsync(con, session.SchoolID, "Failed", ct);

            var rows = await LoadCumulativeMeritSmsRowsAsync(con, session, request.ExamID, ids, ct);
            if (rows.Count == 0)
                return Fail("exam.smsNeedSelect");

            var jobs = new List<(int StudentId, string Phone, string Text, int Count)>();
            foreach (var row in rows)
            {
                var passed = string.Equals(row.PassStatus, "P", StringComparison.OrdinalIgnoreCase);
                var text = passed
                    ? BuildMeritMessage(passedTemplate, row, examName, school, passed: true)
                    : BuildMeritMessage(failedTemplate, row, examName, school, passed: false);
                var phone = row.Phone ?? "";
                jobs.Add((row.StudentID, phone, text, IsValidBdMobile(phone) ? SmsCount(text) : 0));
            }

            var valid = jobs.Where(x => x.Count > 0).ToList();
            if (valid.Count == 0)
                return Fail("sms.needPhone");

            var needed = valid.Sum(x => x.Count);
            var balance = await ReadBalanceAsync(con, session.SchoolID, ct);
            if (balance < needed)
                return new SmsResult { Error = "sms.low", Balance = balance, Message = balance.ToString() };

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
    (@ID, @Phone, @Text, @Len, @Count, N'Cumulative Position Result', @Status, GETDATE(), @Resp)
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
                Message = sent > 0 ? null : lastError,
                LocalMode = _local.IsLocal
            };
        }
        catch (Exception ex)
        {
            return new SmsResult { Error = ex.Message, LocalMode = _local.IsLocal };
        }
    }

    private static async Task<List<MeritSmsRow>> LoadCumulativeMeritSmsRowsAsync(
        SqlConnection con, SessionSnapshot session, int cumulativeNameId, List<int> resultIds, CancellationToken ct)
    {
        var sql = $"""
SELECT ecs.Cumulative_StudentID AS StudentResultID, sc.StudentID, ISNULL(s.ID, N'') AS ID, ISNULL(s.StudentsName, N'') AS StudentsName,
       ISNULL(s.SMSPhoneNo, N'') AS SMSPhoneNo, ISNULL(ecs.ObtainedMark_ofStudent, 0) AS Total,
       ISNULL(ecs.Student_Grade, N'') AS Grade, ISNULL(ecs.Student_Point, 0) AS Point,
       ISNULL(CAST(ecs.Position_InExam_Class AS nvarchar(20)), N'') AS PositionClass,
       ISNULL(CAST(ecs.Position_InExam_Subsection AS nvarchar(20)), N'') AS PositionSection,
       ISNULL(ecs.PassStatus_InSubject, N'') AS PassStatus
FROM dbo.Exam_Cumulative_Student AS ecs
INNER JOIN dbo.StudentsClass AS sc ON ecs.StudentClassID = sc.StudentClassID
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
INNER JOIN dbo.Exam_Cumulative_Setting AS setg ON ecs.Cumulative_SettingID = setg.Cumulative_SettingID
WHERE ecs.SchoolID = @SchoolID AND ecs.EducationYearID = @EducationYearID AND ecs.CumulativeNameID = @ExamID
  AND ISNULL(setg.IS_Published, 0) = 1
  AND ecs.Cumulative_StudentID IN ({string.Join(",", resultIds.Select((_, i) => "@R" + i))})
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ExamID", cumulativeNameId);
        for (var i = 0; i < resultIds.Count; i++)
            cmd.Parameters.AddWithValue("@R" + i, resultIds[i]);

        var rows = new List<MeritSmsRow>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new MeritSmsRow(
                Convert.ToInt32(reader["StudentResultID"]),
                Convert.ToInt32(reader["StudentID"]),
                reader["ID"]?.ToString() ?? "",
                reader["StudentsName"]?.ToString() ?? "",
                reader["SMSPhoneNo"]?.ToString() ?? "",
                Convert.ToDecimal(reader["Total"]),
                WebUtility.HtmlDecode(reader["Grade"]?.ToString() ?? ""),
                Convert.ToDecimal(reader["Point"]),
                reader["PositionClass"]?.ToString() ?? "",
                reader["PositionSection"]?.ToString() ?? "",
                reader["PassStatus"]?.ToString() ?? ""));
        }
        return rows;
    }
    private static async Task<List<MeritSmsRow>> LoadMeritSmsRowsAsync(
        SqlConnection con, SessionSnapshot session, int examId, List<int> resultIds, CancellationToken ct)
    {
        var sql = $"""
SELECT ers.StudentResultID, sc.StudentID, ISNULL(s.ID, N'') AS ID, ISNULL(s.StudentsName, N'') AS StudentsName,
       ISNULL(s.SMSPhoneNo, N'') AS SMSPhoneNo, ISNULL(ers.ObtainedMark_ofStudent, 0) AS Total,
       ISNULL(ers.Student_Grade, N'') AS Grade, ISNULL(ers.Student_Point, 0) AS Point,
       ISNULL(CAST(ers.Position_InExam_Class AS nvarchar(20)), N'') AS PositionClass,
       ISNULL(CAST(ers.Position_InExam_Subsection AS nvarchar(20)), N'') AS PositionSection,
       ISNULL(ers.PassStatus_InSubject, N'') AS PassStatus
FROM dbo.Exam_Result_of_Student AS ers
INNER JOIN dbo.StudentsClass AS sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
WHERE ers.SchoolID = @SchoolID AND ers.EducationYearID = @EducationYearID AND ers.ExamID = @ExamID
  AND ers.StudentPublishStatus = N'Pub'
  AND ers.StudentResultID IN ({string.Join(",", resultIds.Select((_, i) => "@R" + i))})
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        for (var i = 0; i < resultIds.Count; i++)
            cmd.Parameters.AddWithValue("@R" + i, resultIds[i]);

        var rows = new List<MeritSmsRow>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new MeritSmsRow(
                Convert.ToInt32(reader["StudentResultID"]),
                Convert.ToInt32(reader["StudentID"]),
                reader["ID"]?.ToString() ?? "",
                reader["StudentsName"]?.ToString() ?? "",
                reader["SMSPhoneNo"]?.ToString() ?? "",
                Convert.ToDecimal(reader["Total"]),
                WebUtility.HtmlDecode(reader["Grade"]?.ToString() ?? ""),
                Convert.ToDecimal(reader["Point"]),
                reader["PositionClass"]?.ToString() ?? "",
                reader["PositionSection"]?.ToString() ?? "",
                reader["PassStatus"]?.ToString() ?? ""));
        }
        return rows;
    }

    private static async Task<string?> ReadExamResultTemplateAsync(SqlConnection con, int schoolId, string type, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID
  AND ISNULL(IsActive, 1) = 1
  AND (
        TemplateType = @Type
        OR TemplateType LIKE N'%' + @Type + N'%'
        OR (ISNULL(TemplateCategory, N'') = N'ExamResult' AND TemplateName LIKE N'%' + @Type + N'%')
      )
ORDER BY TemplateID DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@Type", type);
            var value = await cmd.ExecuteScalarAsync(ct);
            var text = value?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildMeritMessage(string? template, MeritSmsRow row, string examName, string school, bool passed)
    {
        var total = row.Total.ToString("0.00");
        var point = row.Point.ToString("0.00");
        if (!string.IsNullOrWhiteSpace(template))
        {
            return template
                .Replace("{StudentName}", row.Name, StringComparison.OrdinalIgnoreCase)
                .Replace("{ID}", row.ID, StringComparison.OrdinalIgnoreCase)
                .Replace("{ExamName}", examName, StringComparison.OrdinalIgnoreCase)
                .Replace("{TotalMarks}", total, StringComparison.OrdinalIgnoreCase)
                .Replace("{Grade}", row.Grade, StringComparison.OrdinalIgnoreCase)
                .Replace("{Point}", point, StringComparison.OrdinalIgnoreCase)
                .Replace("{ClassPosition}", row.PositionClass, StringComparison.OrdinalIgnoreCase)
                .Replace("{SectionPosition}", row.PositionSection, StringComparison.OrdinalIgnoreCase)
                .Replace("{SchoolName}", school, StringComparison.OrdinalIgnoreCase);
        }

        if (passed)
        {
            return "Congratulation! " + row.Name
                + "(ID " + row.ID + ")"
                + " You have successfully Passed " + examName + ". Your "
                + "Total Marks: " + total
                + ", Grade: " + row.Grade
                + ", Point: " + point
                + ". Regards: " + school;
        }

        return "Alas!! " + row.Name
            + "(ID " + row.ID + ")"
            + " You have failed " + examName
            + ". Your Total Marks Is : " + total
            + ". Regards: " + school;
    }

    private static async Task<string?> ScalarTextAsync(
        SqlConnection con, string sql, SessionSnapshot session, int examId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ExamID", examId);
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null or DBNull ? null : value.ToString();
    }

    private sealed record MeritSmsRow(
        int StudentResultID,
        int StudentID,
        string ID,
        string Name,
        string Phone,
        decimal Total,
        string Grade,
        decimal Point,
        string PositionClass,
        string PositionSection,
        string PassStatus);
}
