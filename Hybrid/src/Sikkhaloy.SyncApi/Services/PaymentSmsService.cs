using System.Data;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class PaymentSmsService
{
    private const string GatewayUrl = "http://loopsitbd.powersms.net.bd/httpapi/sendsms";
    private const string GatewayUser = "Sikkhaloy";
    private const string GatewayPassword = "Sikkhaloy@SMS_345";

    private readonly EduConnectionFactory _connections;
    private readonly ReportsService _reports;

    public PaymentSmsService(EduConnectionFactory connections, ReportsService reports)
    {
        _connections = connections;
        _reports = reports;
    }

    public async Task<PaymentSmsSettingDto> GetSettingAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        var dto = new PaymentSmsSettingDto();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var cmd = new SqlCommand(
                         "SELECT TOP 1 PAY_Buttton_SMS_Enable_Disable FROM dbo.Account WHERE SchoolID = @SchoolID", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            dto.Active = value is not null and not DBNull && Convert.ToInt32(value) == 1;
        }
        await using (var bal = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con))
        {
            bal.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var value = await bal.ExecuteScalarAsync(cancellationToken);
            dto.Balance = value is null or DBNull ? 0 : Convert.ToInt32(value);
        }
        return dto;
    }

    public async Task SaveSettingAsync(SessionSnapshot session, bool active, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Account SET PAY_Buttton_SMS_Enable_Disable = @V WHERE SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@V", active ? 1 : 0);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<AccountsResult> SendReceiptAsync(SessionSnapshot session, int moneyReceiptId, CancellationToken cancellationToken)
    {
        if (moneyReceiptId <= 0)
            return new AccountsResult { Error = "acc.needReceipt" };
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        string phone = "", studentCode = "", studentName = "", receiptNo = "";
        int studentId = 0;
        decimal amount = 0;
        var details = new StringBuilder();
        await using (var cmd = new SqlCommand("""
SELECT mr.MoneyReceiptID, CAST(mr.MoneyReceipt_SN AS nvarchar(20)) AS ReceiptNo, ISNULL(mr.TotalAmount, 0) AS TotalAmount,
       mr.StudentID, ISNULL(s.ID, N'') AS StudentCode, ISNULL(s.StudentsName, N'') AS StudentName,
       ISNULL(s.SMSPhoneNo, N'') AS Phone
FROM dbo.Income_MoneyReceipt AS mr
INNER JOIN dbo.Student AS s ON mr.StudentID = s.StudentID
WHERE mr.MoneyReceiptID = @MID AND mr.SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@MID", moneyReceiptId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return new AccountsResult { Error = "acc.empty" };
            receiptNo = reader["ReceiptNo"]?.ToString() ?? "";
            amount = reader["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(reader["TotalAmount"]);
            studentId = Convert.ToInt32(reader["StudentID"]);
            studentCode = reader["StudentCode"]?.ToString() ?? "";
            studentName = reader["StudentName"]?.ToString() ?? "";
            phone = reader["Phone"]?.ToString() ?? "";
        }
        await using (var lines = new SqlCommand("""
SELECT r.Role, pr.PayFor
FROM dbo.Income_PaymentRecord AS pr
INNER JOIN dbo.Income_Roles AS r ON pr.RoleID = r.RoleID
WHERE pr.MoneyReceiptID = @MID AND pr.SchoolID = @SchoolID
ORDER BY pr.PayOrderID
""", con))
        {
            lines.Parameters.AddWithValue("@MID", moneyReceiptId);
            lines.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await lines.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                details.Append(", ").Append(reader["Role"]).Append(": ").Append(reader["PayFor"]);
        }

        return await SendCoreAsync(session, studentId, studentCode, studentName, phone, amount, receiptNo,
            details.ToString(), cancellationToken);
    }

    public async Task<AccountsResult> SendDueSmsAsync(SessionSnapshot session, DueSmsRequest request, CancellationToken cancellationToken)
    {
        var ids = (request.Ids ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ids.Count == 0)
            return new AccountsResult { Error = "rpt.needSelect" };

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var template = await ReadDueTemplateAsync(con, session.SchoolID, cancellationToken);
        var balance = 0;
        await using (var bal = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con))
        {
            bal.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var value = await bal.ExecuteScalarAsync(cancellationToken);
            balance = value is null or DBNull ? 0 : Convert.ToInt32(value);
        }

        var jobs = new List<(int StudentId, string Phone, string Message, int Count)>();
        foreach (var id in ids)
        {
            var detail = await _reports.GetDueDetailsAsync(session, id, request.RoleId, cancellationToken);
            if (detail.StudentID <= 0)
                detail = await LookupStudentAsync(con, session.SchoolID, id, cancellationToken);
            if (detail.StudentID <= 0)
            {
                jobs.Add((0, "", "", 0));
                continue;
            }
            var phone = (detail.Phone ?? "").Trim();
            var dueDetails = string.Join(", ", detail.Lines.Select(x =>
                $"{x.Role}: {x.PayFor} - {Fmt(x.Due)} Tk"));
            var msg = BuildDueMessage(template, detail.Name, detail.ID, detail.Due, dueDetails, session.SchoolName);
            if (!IsValidBdMobile(phone))
            {
                jobs.Add((detail.StudentID, phone, msg, 0));
                continue;
            }
            jobs.Add((detail.StudentID, phone, msg, SmsCount(msg)));
        }

        var needed = jobs.Sum(x => x.Count);
        if (needed == 0)
            return new AccountsResult { Succeeded = false, Failed = ids.Count, Error = "acc.smsNoPhone" };
        if (balance < needed)
            return new AccountsResult { Error = "acc.smsLow", Count = balance };

        var sent = 0;
        var failed = 0;
        foreach (var job in jobs)
        {
            if (job.Count <= 0 || string.IsNullOrWhiteSpace(job.Phone))
            {
                failed++;
                continue;
            }
            var response = await PostGatewayAsync(job.Phone, job.Message, cancellationToken);
            if (string.IsNullOrWhiteSpace(response))
            {
                failed++;
                continue;
            }
            var smsId = Guid.NewGuid();
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, N'Due SMS', N'Sent', GETDATE(), @Resp)
""", con))
            {
                ins.Parameters.AddWithValue("@ID", smsId);
                ins.Parameters.AddWithValue("@Phone", job.Phone);
                ins.Parameters.AddWithValue("@Text", job.Message);
                ins.Parameters.AddWithValue("@Len", job.Message.Length);
                ins.Parameters.AddWithValue("@Count", job.Count);
                ins.Parameters.AddWithValue("@Resp", response);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }
            await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID)
VALUES (@ID, @SchoolID, @SID, @YearID)
""", con))
            {
                other.Parameters.AddWithValue("@ID", smsId);
                other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                other.Parameters.AddWithValue("@SID", job.StudentId);
                other.Parameters.AddWithValue("@YearID", session.EducationYearID);
                await other.ExecuteNonQueryAsync(cancellationToken);
            }
            sent++;
        }

        return new AccountsResult { Succeeded = sent > 0, Saved = sent, Failed = failed, Count = sent };
    }

    public async Task TrySendAfterCollectAsync(
        SessionSnapshot session, int studentId, string studentCode, string studentName, string? phone,
        decimal amount, string receiptNo, string details, CancellationToken cancellationToken)
    {
        try
        {
            await SendCoreAsync(session, studentId, studentCode, studentName, phone ?? "", amount, receiptNo, details, cancellationToken);
        }
        catch
        {
        }
    }

    private async Task<AccountsResult> SendCoreAsync(
        SessionSnapshot session, int studentId, string studentCode, string studentName, string phone,
        decimal amount, string receiptNo, string details, CancellationToken cancellationToken)
    {
        phone = (phone ?? "").Trim();
        if (!IsValidBdMobile(phone))
            return new AccountsResult { Error = "acc.smsNoPhone" };

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var template = await ReadTemplateAsync(con, session.SchoolID, cancellationToken);
        var due = await CurrentDueAsync(con, studentCode, session.SchoolID, cancellationToken);
        var msg = BuildMessage(template, studentName, studentCode, amount, receiptNo, details, due, session.SchoolName);
        var count = SmsCount(msg);
        var balance = 0;
        await using (var bal = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con))
        {
            bal.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var value = await bal.ExecuteScalarAsync(cancellationToken);
            balance = value is null or DBNull ? 0 : Convert.ToInt32(value);
        }
        if (balance < count)
            return new AccountsResult { Error = "acc.smsLow" };

        var response = await PostGatewayAsync(phone, msg, cancellationToken);
        if (string.IsNullOrWhiteSpace(response))
            return new AccountsResult { Error = "acc.smsFail" };

        var smsId = Guid.NewGuid();
        await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, N'Payment Collection', N'Sent', GETDATE(), @Resp)
""", con))
        {
            ins.Parameters.AddWithValue("@ID", smsId);
            ins.Parameters.AddWithValue("@Phone", phone);
            ins.Parameters.AddWithValue("@Text", msg);
            ins.Parameters.AddWithValue("@Len", msg.Length);
            ins.Parameters.AddWithValue("@Count", count);
            ins.Parameters.AddWithValue("@Resp", response);
            await ins.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID)
VALUES (@ID, @SchoolID, @SID, @YearID)
""", con))
        {
            other.Parameters.AddWithValue("@ID", smsId);
            other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            other.Parameters.AddWithValue("@SID", studentId);
            other.Parameters.AddWithValue("@YearID", session.EducationYearID);
            await other.ExecuteNonQueryAsync(cancellationToken);
        }
        return new AccountsResult { Succeeded = true, Saved = count };
    }

    private static async Task<CurrentDueStudentDetailDto> LookupStudentAsync(
        SqlConnection con, int schoolId, string studentCode, CancellationToken cancellationToken)
    {
        var dto = new CurrentDueStudentDetailDto { ID = studentCode };
        await using var cmd = new SqlCommand("""
SELECT TOP 1 StudentID, ID, StudentsName, SMSPhoneNo
FROM dbo.Student
WHERE SchoolID = @SchoolID AND ID = @ID AND Status = N'Active'
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ID", studentCode);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return dto;
        dto.StudentID = Convert.ToInt32(reader["StudentID"]);
        dto.ID = reader["ID"]?.ToString() ?? studentCode;
        dto.Name = reader["StudentsName"]?.ToString() ?? "";
        dto.Phone = reader["SMSPhoneNo"]?.ToString() ?? "";
        return dto;
    }

    private static async Task<string?> ReadDueTemplateAsync(SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID
  AND (
        TemplateType = N'Due'
        OR TemplateType LIKE N'%Due%'
        OR ISNULL(TemplateCategory, N'') = N'Due'
        OR ISNULL(TemplateCategory, N'') LIKE N'%Due%'
      )
ORDER BY CASE WHEN ISNULL(IsActive, 1) = 1 THEN 0 ELSE 1 END, CreatedDate DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            return (await cmd.ExecuteScalarAsync(cancellationToken))?.ToString();
        }
        catch
        {
            try
            {
                await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND TemplateType LIKE N'%Due%'
""", con);
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                return (await cmd.ExecuteScalarAsync(cancellationToken))?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }

    private static string BuildDueMessage(
        string? template, string name, string id, decimal totalDue, string dueDetails, string school)
    {
        var dueText = Fmt(totalDue);
        if (!string.IsNullOrWhiteSpace(template))
        {
            var details = (dueDetails ?? "").Trim().TrimStart(',').TrimEnd(',').Trim();
            var msg = template
                .Replace("{StudentName}", name)
                .Replace("{ID}", id)
                .Replace("{TotalDue}", dueText)
                .Replace("{SchoolName}", school);
            if (!string.IsNullOrWhiteSpace(details))
                msg = msg.Replace("{DueDetails}", details);
            else
                msg = msg.Replace(", {DueDetails}", "").Replace("{DueDetails}", "");
            return msg;
        }
        return $"Dear, {name}, ID:{id}. You've Due Payment: {dueText} Tk. Regards, {school}";
    }

    private static string Fmt(decimal amount) =>
        amount == decimal.Truncate(amount)
            ? decimal.Truncate(amount).ToString("0")
            : amount.ToString("0.00");

    private static async Task<string?> ReadTemplateAsync(SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND TemplateType = N'Payment' AND ISNULL(IsActive, 1) = 1
ORDER BY CreatedDate DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            return (await cmd.ExecuteScalarAsync(cancellationToken))?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static async Task<decimal> CurrentDueAsync(SqlConnection con, string studentCode, int schoolId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT ISNULL(SUM(
    ISNULL(po.Amount,0)+ISNULL(po.LateFee,0)-ISNULL(po.Discount,0)-ISNULL(po.PaidAmount,0)-ISNULL(po.LateFee_Discount,0)
), 0)
FROM dbo.Income_PayOrder AS po
INNER JOIN dbo.Student AS st ON po.StudentID = st.StudentID
WHERE st.ID = @ID AND st.SchoolID = @SchID AND po.SchoolID = @SchID
  AND po.Status = N'Due' AND po.EndDate < GETDATE()
""", con);
        cmd.Parameters.AddWithValue("@ID", studentCode);
        cmd.Parameters.AddWithValue("@SchID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? 0 : Convert.ToDecimal(value);
    }

    private static string BuildMessage(
        string? template, string name, string id, decimal amount, string receiptNo, string details, decimal due, string school)
    {
        var paid = amount == decimal.Truncate(amount) ? decimal.Truncate(amount).ToString("0") : amount.ToString("0.00");
        var dueText = due == decimal.Truncate(due) ? decimal.Truncate(due).ToString("0") : due.ToString("0.00");
        var payFor = (details ?? "").Trim().TrimStart(',').Trim();
        if (!string.IsNullOrWhiteSpace(template))
        {
            return template
                .Replace("{StudentName}", name)
                .Replace("{ID}", id)
                .Replace("{Amount}", paid)
                .Replace("{ReceiptNo}", receiptNo)
                .Replace("{CurrentDue}", dueText)
                .Replace("{PaymentDetails}", payFor)
                .Replace("{Session}", "")
                .Replace("{SchoolName}", school);
        }
        return $"Congrats! (ID: {id}) {name}. You've Paid: {paid} Tk. Receipt No: {receiptNo}. Regards, {school}";
    }

    private static int SmsCount(string text)
    {
        var unicode = text.Any(ch => ch > 127);
        var size = unicode ? 70 : 160;
        return Math.Max(1, (int)Math.Ceiling(text.Length / (double)size));
    }

    private static bool IsValidBdMobile(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits.StartsWith("01", StringComparison.Ordinal)
               || digits.Length == 13 && digits.StartsWith("8801", StringComparison.Ordinal);
    }

    private static async Task<string?> PostGatewayAsync(string number, string text, CancellationToken cancellationToken)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var safe = text.Replace("A+", "A Plus", StringComparison.OrdinalIgnoreCase).Replace("+", " Plus ");
            var body = "userId=" + Uri.EscapeDataString(GatewayUser)
                       + "&password=" + Uri.EscapeDataString(GatewayPassword)
                       + "&smsText=" + Uri.EscapeDataString(safe)
                       + "&commaSeperatedReceiverNumbers=" + Uri.EscapeDataString(number);
            using var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            using var response = await http.PostAsync(GatewayUrl, content, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode || json.Contains("\"isError\":true", StringComparison.OrdinalIgnoreCase))
                return null;
            return string.IsNullOrWhiteSpace(json) ? "Sent" : json;
        }
        catch
        {
            return null;
        }
    }
}
