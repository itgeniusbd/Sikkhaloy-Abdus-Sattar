using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class StudentPortalService
{
    private const string DueExpr = """
CASE WHEN Income_PayOrder.EndDate < GETDATE() - 1
     THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0)
        - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0)
        - ISNULL(Income_PayOrder.LateFee_Discount, 0)
     ELSE ISNULL(Income_PayOrder.Amount, 0) - ISNULL(Income_PayOrder.Discount, 0)
        - ISNULL(Income_PayOrder.PaidAmount, 0) END
""";

    public async Task<StudentPortalAccountsBundleDto> GetAccountsBundleAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalAccountsBundleDto();
        if (!IsPortal(session))
            return dto;

        var dueTask = LoadDueRowsAsync(session, currentOnly: false, ct);
        var currentTask = LoadDueRowsAsync(session, currentOnly: true, ct);
        var paidTask = LoadPaidRowsAsync(session, ct);
        var receiptTask = LoadReceiptsAsync(session, ct);
        var concessionTask = LoadConcessionRowsAsync(session, ct);
        var lateConcTask = LoadLateFeeRowsAsync(session, "Income_LateFee_Discount_Record", ct);
        var lateChargeTask = LoadLateFeeRowsAsync(session, "Income_LateFee_Change_Record", ct);
        var allTask = LoadAllPayOrdersAsync(session, ct);
        var summaryTask = LoadPayOrderSummaryAsync(session, ct);
        var enabledTask = IsOnlinePaymentEnabledAsync(session, ct);

        await Task.WhenAll(dueTask, currentTask, paidTask, receiptTask, concessionTask,
            lateConcTask, lateChargeTask, allTask, summaryTask, enabledTask);

        dto.TotalDues = dueTask.Result;
        dto.CurrentDues = currentTask.Result;
        dto.Paid = paidTask.Result;
        dto.Receipts = receiptTask.Result;
        dto.Concessions = concessionTask.Result;
        dto.LateFeeConcessions = lateConcTask.Result;
        dto.LateFeeCharges = lateChargeTask.Result;
        dto.AllPayOrders = allTask.Result;
        dto.Summary = summaryTask.Result;
        dto.OnlinePaymentEnabled = enabledTask.Result;
        dto.TotalDue = dto.TotalDues.Sum(x => x.Due);
        dto.CurrentDue = dto.CurrentDues.Sum(x => x.Due);
        dto.TotalPaid = dto.Paid.Sum(x => x.Paid);
        dto.TotalConcession = dto.Concessions.Sum(x => x.Discount);
        return dto;
    }

    public Task<List<StudentPortalReceiptLineDto>> GetReceiptLinesAsync(
        SessionSnapshot session, int moneyReceiptId, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Income_Roles.Role, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PaidDate
FROM dbo.Income_PaymentRecord
INNER JOIN dbo.Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
INNER JOIN dbo.Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID
WHERE Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID
  AND Income_MoneyReceipt.StudentID = @StudentID
  AND Income_MoneyReceipt.SchoolID = @SchoolID
""", ct, r => new StudentPortalReceiptLineDto
        {
            Role = Text(r["Role"]),
            PayFor = Text(r["PayFor"]),
            Paid = Dec(r["PaidAmount"]),
            PaidDate = Day(r["PaidDate"])
        }, extra => extra.Parameters.AddWithValue("@MoneyReceiptID", moneyReceiptId));

    public async Task<StudentPortalPayStartResult> StartOnlinePaymentAsync(
        SessionSnapshot session, StudentPortalPayStartRequest? request, CancellationToken ct)
    {
        if (!IsPortal(session))
            return FailPay("auth.forbidden");
        if (!await IsOnlinePaymentEnabledAsync(session, ct))
            return FailPay("stu.payOff");

        var ids = request?.PayOrderIDs.Where(x => x > 0).Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return FailPay("stu.payPick");

        var dues = await LoadDueRowsByIdsAsync(session, ids, ct);
        if (dues.Count == 0 || dues.Any(x => x.Due <= 0))
            return FailPay("stu.payPick");

        var accountId = await ScalarIntAsync(session, """
SELECT TOP 1 AccountID FROM dbo.Account
WHERE SchoolID = @SchoolID AND AccountName = N'Online Payment'
""", ct);
        if (accountId <= 0)
            return FailPay("stu.payAccount");

        var student = await LoadPayCustomerAsync(session, ct);
        if (string.IsNullOrWhiteSpace(student.Email))
            return FailPay("stu.payEmail");

        var paymentRecordId = DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + session.StudentID;
        await InsertTempRecordsAsync(session, dues, paymentRecordId, accountId, ct);

        var creds = await LoadAmarPayAsync(session, ct);
        if (string.IsNullOrWhiteSpace(creds.StoreId) || string.IsNullOrWhiteSpace(creds.SignatureKey))
            return FailPay("stu.payStore");

        var returnUrl = SafeReturnUrl(request?.ReturnUrl);
        var callback = CallbackBase() + "/api/sync/student-portal/pay/callback";
        var payload = new Dictionary<string, object?>
        {
            ["store_id"] = creds.StoreId,
            ["signature_key"] = creds.SignatureKey,
            ["tran_id"] = RandomTran(),
            ["amount"] = dues.Sum(x => x.Due),
            ["currency"] = "BDT",
            ["desc"] = "Pay Fee",
            ["cus_name"] = student.Name,
            ["cus_email"] = student.Email,
            ["cus_phone"] = student.Phone,
            ["cus_add1"] = "N/A",
            ["cus_add2"] = "N/A",
            ["cus_city"] = "Dhaka",
            ["cus_state"] = "Dhaka",
            ["cus_postcode"] = "1200",
            ["cus_country"] = "Bangladesh",
            ["type"] = "json",
            ["success_url"] = callback,
            ["fail_url"] = returnUrl + (returnUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?") + "pay=fail",
            ["cancel_url"] = returnUrl + (returnUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?") + "pay=cancel",
            ["opt_a"] = returnUrl,
            ["opt_b"] = paymentRecordId
        };

        try
        {
            using var client = _http.CreateClient();
            using var response = await client.PostAsJsonAsync(creds.Gateway + "/jsonpost.php", payload, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            if (doc.RootElement.TryGetProperty("payment_url", out var url) && url.ValueKind == JsonValueKind.String)
            {
                var paymentUrl = url.GetString();
                if (!string.IsNullOrWhiteSpace(paymentUrl))
                    return new StudentPortalPayStartResult { Succeeded = true, PaymentUrl = paymentUrl };
            }

            var error = body;
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var name in new[] { "error", "mess", "message" })
                {
                    if (doc.RootElement.TryGetProperty(name, out var msg) && msg.ValueKind == JsonValueKind.String)
                        error = msg.GetString();
                }
            }
            return FailPay(string.IsNullOrWhiteSpace(error) ? "stu.payFailed" : error);
        }
        catch (Exception)
        {
            return FailPay("stu.payFailed");
        }
    }

    public async Task<StudentPortalPayCompleteResult> CompleteOnlinePaymentAsync(string? paymentRecordId, string? returnUrl, CancellationToken ct)
    {
        var id = (paymentRecordId ?? "").Trim();
        if (id.Length == 0)
            return new StudentPortalPayCompleteResult { Succeeded = false, Error = "stu.payFailed" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var temps = new List<(int StudentID, int RoleID, int PayOrderID, int YearID, decimal Paid, string PayFor, int AccountID)>();
        await using (var cmd = new SqlCommand("""
SELECT StudentID, RoleID, PayOrderID, PayOrderEduYearID, PaidAmount, PayFor, AccountID
FROM dbo.Temp_Online_PaymentRecord
WHERE PaymentRecordID = @ID
""", con))
        {
            cmd.Parameters.AddWithValue("@ID", id);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                temps.Add((
                    ToInt(reader["StudentID"]),
                    ToInt(reader["RoleID"]),
                    ToInt(reader["PayOrderID"]),
                    ToInt(reader["PayOrderEduYearID"]),
                    Dec(reader["PaidAmount"]),
                    Text(reader["PayFor"]),
                    ToInt(reader["AccountID"])));
            }
        }

        if (temps.Count == 0)
            return new StudentPortalPayCompleteResult { Succeeded = true };

        var studentId = temps[0].StudentID;
        var schoolId = await ScalarIntByStudentAsync(con, studentId, ct);
        var classId = 0;
        var yearId = temps[0].YearID;
        await using (var find = new SqlCommand("""
SELECT TOP 1 StudentsClass.StudentClassID, StudentsClass.EducationYearID, StudentsClass.SchoolID
FROM dbo.StudentsClass
WHERE StudentsClass.StudentID = @StudentID
ORDER BY StudentsClass.EducationYearID DESC
""", con))
        {
            find.Parameters.AddWithValue("@StudentID", studentId);
            await using var reader = await find.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                classId = ToInt(reader["StudentClassID"]);
                if (yearId <= 0)
                    yearId = ToInt(reader["EducationYearID"]);
                if (schoolId <= 0)
                    schoolId = ToInt(reader["SchoolID"]);
            }
        }

        var adminReg = await ScalarIntAsync(new SessionSnapshot { SchoolID = schoolId }, """
SELECT TOP 1 RegistrationID FROM dbo.Registration
WHERE SchoolID = @SchoolID AND Validation = N'Valid' AND Category = N'Admin'
ORDER BY RegistrationID
""", ct);
        var collect = await _accounts.CollectAsync(new SessionSnapshot
        {
            SchoolID = schoolId,
            EducationYearID = yearId,
            RegistrationID = adminReg
        }, new CollectPaymentRequest
        {
            StudentID = studentId,
            StudentClassID = classId,
            EducationYearID = yearId,
            AccountID = temps[0].AccountID,
            PaidDate = DateTime.Now,
            SendSms = true,
            Items = temps.Select(x => new CollectPaymentItem
            {
                PayOrderID = x.PayOrderID,
                PaidAmount = x.Paid
            }).ToList()
        }, ct);

        if (!collect.Succeeded)
            return new StudentPortalPayCompleteResult { Succeeded = false, Error = collect.Error ?? "stu.payFailed" };

        await using (var del = new SqlCommand("DELETE FROM dbo.Temp_Online_PaymentRecord WHERE PaymentRecordID = @ID", con))
        {
            del.Parameters.AddWithValue("@ID", id);
            await del.ExecuteNonQueryAsync(ct);
        }

        return new StudentPortalPayCompleteResult { Succeeded = true, ReceiptNo = collect.ReceiptNo };
    }

    private async Task<List<StudentPortalDueRowDto>> LoadDueRowsAsync(SessionSnapshot session, bool currentOnly, CancellationToken ct)
    {
        var filter = currentOnly ? "AND Income_PayOrder.EndDate < GETDATE()" : "";
        return await QueryListAsync(session, $"""
SELECT Income_PayOrder.PayOrderID, Income_PayOrder.RoleID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID,
       Income_Roles.Role, Income_PayOrder.PayFor, Education_Year.EducationYear, CreateClass.Class,
       Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount,
       Income_PayOrder.PaidAmount, {DueExpr} AS Due, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.LastPaidDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
INNER JOIN dbo.Student ON Income_PayOrder.StudentID = Student.StudentID
INNER JOIN dbo.Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID
INNER JOIN dbo.CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID
WHERE Income_PayOrder.Status = N'Due'
  AND Income_PayOrder.StudentID = @StudentID
  AND Student.Status = N'Active'
  AND Income_PayOrder.SchoolID = @SchoolID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  {filter}
ORDER BY Income_PayOrder.EndDate
""", ct, MapDue);
    }

    private Task<List<StudentPortalDueRowDto>> LoadPaidRowsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, $"""
SELECT Income_PayOrder.PayOrderID, Income_PayOrder.RoleID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID,
       Income_Roles.Role, Income_PayOrder.PayFor, N'' AS EducationYear, N'' AS Class,
       Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount,
       Income_PayOrder.PaidAmount, {DueExpr} AS Due, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.LastPaidDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.Status = N'Paid'
  AND Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.StudentClassID = @StudentClassID
  AND Income_PayOrder.PaidAmount <> 0
ORDER BY Income_PayOrder.LastPaidDate DESC
""", ct, MapDue);

    private Task<List<StudentPortalReceiptDto>> LoadReceiptsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT MoneyReceiptID, MoneyReceipt_SN, PaidDate, TotalAmount, PaymentBy
FROM dbo.Income_MoneyReceipt
WHERE StudentID = @StudentID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID
ORDER BY PaidDate DESC
""", ct, r => new StudentPortalReceiptDto
        {
            MoneyReceiptID = ToInt(r["MoneyReceiptID"]),
            ReceiptNo = Text(r["MoneyReceipt_SN"]),
            PaidDate = Day(r["PaidDate"]),
            TotalAmount = Dec(r["TotalAmount"]),
            PaymentBy = Text(r["PaymentBy"])
        });

    private Task<List<StudentPortalDueRowDto>> LoadConcessionRowsAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, """
SELECT Income_PayOrder.PayOrderID, Income_PayOrder.RoleID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID,
       Income_Roles.Role, Income_PayOrder.PayFor, N'' AS EducationYear, N'' AS Class,
       Income_PayOrder.Amount, Income_PayOrder.Total_Discount AS Discount, Income_PayOrder.LateFee, 0 AS LateFee_Discount,
       0 AS PaidAmount, 0 AS Due, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.LastPaidDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.StudentClassID = @StudentClassID
  AND Income_PayOrder.Total_Discount <> 0
ORDER BY Income_PayOrder.StartDate
""", ct, MapDue);

    private Task<List<StudentPortalLateFeeDto>> LoadLateFeeRowsAsync(SessionSnapshot session, string table, CancellationToken ct) =>
        QueryListAsync(session, $"""
SELECT PreviousAmount, PostAmount, {(table.Contains("Discount", StringComparison.Ordinal) ? "Reason" : "N'' AS Reason")}, Date
FROM dbo.{table}
WHERE StudentID = @StudentID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID
ORDER BY Date DESC
""", ct, r => new StudentPortalLateFeeDto
        {
            PreviousAmount = Dec(r["PreviousAmount"]),
            PostAmount = Dec(r["PostAmount"]),
            Reason = Text(r["Reason"]),
            Date = Day(r["Date"])
        });

    private Task<List<StudentPortalDueRowDto>> LoadAllPayOrdersAsync(SessionSnapshot session, CancellationToken ct) =>
        QueryListAsync(session, $"""
SELECT Income_PayOrder.PayOrderID, Income_PayOrder.RoleID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID,
       Income_Roles.Role, Income_PayOrder.PayFor, N'' AS EducationYear, N'' AS Class,
       Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount,
       Income_PayOrder.PaidAmount, {DueExpr} AS Due, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.LastPaidDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.StudentClassID = @StudentClassID
ORDER BY Income_PayOrder.StartDate
""", ct, MapDue);

    private async Task<StudentPortalPayOrderSummaryDto> LoadPayOrderSummaryAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentPortalPayOrderSummaryDto();
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT SUM(Amount) AS TotalFee, SUM(LateFeeCountable) AS TotalLateFee, SUM(Total_Discount) AS TotalDiscount,
       SUM(ISNULL(PaidAmount, 0)) AS TotalPaid, SUM(Receivable_Amount) AS Unpaid
FROM dbo.Income_PayOrder
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID
""", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.TotalFee = Dec(reader["TotalFee"]);
                dto.TotalLateFee = Dec(reader["TotalLateFee"]);
                dto.TotalDiscount = Dec(reader["TotalDiscount"]);
                dto.TotalPaid = Dec(reader["TotalPaid"]);
                dto.Unpaid = Dec(reader["Unpaid"]);
            }
        }
        catch (SqlException)
        {
        }
        return dto;
    }

    private async Task<List<StudentPortalDueRowDto>> LoadDueRowsByIdsAsync(
        SessionSnapshot session, IReadOnlyList<int> ids, CancellationToken ct)
    {
        var names = ids.Select((_, i) => "@p" + i).ToList();
        return await QueryListAsync(session, $"""
SELECT Income_PayOrder.PayOrderID, Income_PayOrder.RoleID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID,
       Income_Roles.Role, Income_PayOrder.PayFor, N'' AS EducationYear, N'' AS Class,
       Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount,
       Income_PayOrder.PaidAmount, {DueExpr} AS Due, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.LastPaidDate
FROM dbo.Income_PayOrder
INNER JOIN dbo.Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.StudentID = @StudentID
  AND Income_PayOrder.SchoolID = @SchoolID
  AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Status = N'Due'
  AND Income_PayOrder.PayOrderID IN ({string.Join(", ", names)})
""", ct, MapDue, extra =>
        {
            for (var i = 0; i < ids.Count; i++)
                extra.Parameters.AddWithValue(names[i], ids[i]);
        });
    }

    private async Task InsertTempRecordsAsync(
        SessionSnapshot session, IReadOnlyList<StudentPortalDueRowDto> dues, string paymentRecordId, int accountId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Temp_Online_PaymentRecord
    (PaymentRecordID, StudentID, RoleID, PayOrderID, PayOrderEduYearID, PaidAmount, PayFor, PaidDate, AccountID)
VALUES
    (@PaymentRecordID, @StudentID, @RoleID, @PayOrderID, @PayOrderEduYearID, @PaidAmount, @PayFor, @PaidDate, @AccountID)
""", con, tx);
        cmd.Parameters.AddWithValue("@PaymentRecordID", paymentRecordId);
        cmd.Parameters.AddWithValue("@StudentID", session.StudentID);
        cmd.Parameters.Add("@RoleID", System.Data.SqlDbType.Int);
        cmd.Parameters.Add("@PayOrderID", System.Data.SqlDbType.Int);
        cmd.Parameters.Add("@PayOrderEduYearID", System.Data.SqlDbType.Int);
        cmd.Parameters.Add("@PaidAmount", System.Data.SqlDbType.Decimal);
        cmd.Parameters.Add("@PayFor", System.Data.SqlDbType.NVarChar, 200);
        cmd.Parameters.AddWithValue("@PaidDate", DateTime.Now);
        cmd.Parameters.AddWithValue("@AccountID", accountId);
        foreach (var row in dues)
        {
            cmd.Parameters["@RoleID"].Value = row.RoleID;
            cmd.Parameters["@PayOrderID"].Value = row.PayOrderID;
            cmd.Parameters["@PayOrderEduYearID"].Value = row.EducationYearID;
            cmd.Parameters["@PaidAmount"].Value = row.Due;
            cmd.Parameters["@PayFor"].Value = row.PayFor;
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct);
    }

    private async Task<bool> IsOnlinePaymentEnabledAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            return await ScalarIntAsync(session, "SELECT ISNULL(OnlinePaymentEnable, 0) FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", ct) == 1;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private async Task<(string StoreId, string SignatureKey, string Gateway)> LoadAmarPayAsync(SessionSnapshot session, CancellationToken ct)
    {
        var sandbox = string.Equals(_config["StudentPay:Sandbox"], "true", StringComparison.OrdinalIgnoreCase);
        if (sandbox)
        {
            return ("aamarpaytest", "dbb74894e82415a2f7ff0ec3a97e4183", "https://sandbox.aamarpay.com");
        }

        var store = "";
        var key = "";
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("SELECT StoreId, SignatureKey FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                store = Text(reader["StoreId"]);
                key = Text(reader["SignatureKey"]);
            }
        }
        catch (SqlException)
        {
        }
        return (store, key, "https://secure.aamarpay.com");
    }

    private async Task<(string Name, string Email, string Phone)> LoadPayCustomerAsync(SessionSnapshot session, CancellationToken ct)
    {
        var name = session.DisplayName;
        var email = "";
        var phone = "";
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT Student.StudentsName, Student.StudentEmailAddress, Student.SMSPhoneNo, SchoolInfo.Email
FROM dbo.Student
INNER JOIN dbo.SchoolInfo ON SchoolInfo.SchoolID = Student.SchoolID
WHERE Student.StudentID = @StudentID AND Student.SchoolID = @SchoolID
""", con);
            AddStudent(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                name = Text(reader["StudentsName"]);
                email = Text(reader["StudentEmailAddress"]);
                phone = Text(reader["SMSPhoneNo"]);
                if (string.IsNullOrWhiteSpace(email))
                    email = Text(reader["Email"]);
            }
        }
        catch (SqlException)
        {
        }
        return (name, email, phone);
    }

    private async Task<int> ScalarIntAsync(SessionSnapshot session, string sql, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, con);
        AddStudent(cmd, session);
        return ToInt(await cmd.ExecuteScalarAsync(ct));
    }

    private static async Task<int> ScalarIntByStudentAsync(SqlConnection con, int studentId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT TOP 1 SchoolID FROM dbo.Student WHERE StudentID = @StudentID", con);
        cmd.Parameters.AddWithValue("@StudentID", studentId);
        return ToInt(await cmd.ExecuteScalarAsync(ct));
    }

    private string CallbackBase()
    {
        var configured = _config["StudentPay:CallbackBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured.TrimEnd('/');
        var req = _httpContext.HttpContext?.Request;
        if (req is null)
            return "http://127.0.0.1:5135";
        return $"{req.Scheme}://{req.Host.Value}";
    }

    private static string SafeReturnUrl(string? raw)
    {
        if (Uri.TryCreate(raw, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return uri.GetLeftPart(UriPartial.Path);
        }
        return "http://localhost:5288/student/accounts";
    }

    private static string RandomTran()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 10).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    private static StudentPortalPayStartResult FailPay(string error) =>
        new() { Succeeded = false, Error = error };

    private static StudentPortalDueRowDto MapDue(SqlDataReader r) => new()
    {
        PayOrderID = ToInt(r["PayOrderID"]),
        RoleID = ToInt(r["RoleID"]),
        EducationYearID = ToInt(r["EducationYearID"]),
        StudentClassID = ToInt(r["StudentClassID"]),
        Role = Text(r["Role"]),
        PayFor = Text(r["PayFor"]),
        YearName = Text(r["EducationYear"]),
        ClassName = Text(r["Class"]),
        Amount = Dec(r["Amount"]),
        Discount = Dec(r["Discount"]),
        LateFee = Dec(r["LateFee"]),
        LateFeeDiscount = Dec(r["LateFee_Discount"]),
        Paid = Dec(r["PaidAmount"]),
        Due = Dec(r["Due"]),
        StartDate = Day(r["StartDate"]),
        EndDate = Day(r["EndDate"]),
        LastPaidDate = Day(r["LastPaidDate"])
    };

    private async Task<List<T>> QueryListAsync<T>(
        SessionSnapshot session, string sql, CancellationToken ct, Func<SqlDataReader, T> map, Action<SqlCommand>? extra)
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
            extra?.Invoke(cmd);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                items.Add(map(reader));
        }
        catch (SqlException)
        {
        }
        return items;
    }
}
