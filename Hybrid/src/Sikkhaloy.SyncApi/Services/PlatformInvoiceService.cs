using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Invoice;

namespace Sikkhaloy.SyncApi.Services;

public sealed class PlatformInvoiceService
{
    private const string ShurjoPayBase = "https://engine.shurjopayment.com";
    private const string ShurjoPayUser = "sikkhaloy";
    private const string ShurjoPayPassword = "sikkp22tmxq3499z";
    private const string ShurjoPayPrefix = "SIK";
    private const string ReturnUrl = "https://sikkhaloy.com/Profile/Invoice/ShurjoPayCallback.aspx";
    private const string CancelUrl = "https://sikkhaloy.com/Profile/Invoice/Due_Invoice.aspx";

    private readonly EduConnectionFactory _connections;
    private readonly LocalOfficeMode _local;

    public PlatformInvoiceService(EduConnectionFactory connections, LocalOfficeMode local)
    {
        _connections = connections;
        _local = local;
    }

    public async Task<DueInvoiceDto> GetDueAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new DueInvoiceDto { LocalMode = _local.IsLocal };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT SchoolName, Address, Phone, Email FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.SchoolName = S(reader["SchoolName"]);
                dto.Address = S(reader["Address"]);
                dto.Phone = S(reader["Phone"]);
                dto.Email = S(reader["Email"]);
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT i.Invoice_For, i.Unit, i.UnitPrice, i.TotalAmount, i.Discount, i.PaidAmount,
       c.InvoiceCategory, (i.TotalAmount - i.PaidAmount - i.Discount) AS Due
FROM dbo.AAP_Invoice i
INNER JOIN dbo.AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
WHERE i.SchoolID = @SchoolID AND i.IsPaid = 0
ORDER BY i.InvoiceID
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var line = new DueInvoiceLineDto
                {
                    InvoiceCategory = S(reader["InvoiceCategory"]),
                    InvoiceFor = S(reader["Invoice_For"]),
                    Unit = Dec(reader["Unit"]),
                    UnitPrice = Dec(reader["UnitPrice"]),
                    TotalAmount = Dec(reader["TotalAmount"]),
                    Discount = Dec(reader["Discount"]),
                    PaidAmount = Dec(reader["PaidAmount"]),
                    Due = Dec(reader["Due"])
                };
                dto.Lines.Add(line);
                dto.GrandTotal += line.TotalAmount;
                dto.Discount += line.Discount;
                dto.PaidAmount += line.PaidAmount;
                dto.Due += line.Due;
            }
        }

        dto.HasDue = dto.Lines.Count > 0;
        dto.ShowDiscount = dto.Lines.Any(x => x.Discount > 0);
        dto.ShowPaid = dto.Lines.Any(x => x.PaidAmount > 0);
        dto.GatewayCharge = Math.Round(dto.Due / 1000m * 10m, 2);
        dto.TotalPayable = dto.Due + dto.GatewayCharge;

        var status = await ReadStatusAsync(con, session.SchoolID, ct);
        dto.IsBlocked = status.IsBlocked;
        dto.DaysUntilExpiry = status.DaysUntilExpiry;
        return dto;
    }

    public async Task<SubscriptionStatusDto> GetStatusAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await ReadStatusAsync(con, session.SchoolID, ct);
    }

    public async Task<InvoiceResult> PayDueAsync(SessionSnapshot session, CancellationToken ct)
    {
        var due = await GetDueAsync(session, ct);
        if (!due.HasDue || due.Due <= 0)
            return Fail("inv.noDue");
        if (_local.IsLocal)
            return new InvoiceResult { Succeeded = true, LocalMode = true, Message = "inv.localNote" };

        var name = string.IsNullOrWhiteSpace(due.SchoolName) ? "School" : due.SchoolName;
        if (name.Length > 50) name = name[..50];
        var phone = FirstPhone(due.Phone) ?? "01700000000";
        var email = string.IsNullOrWhiteSpace(due.Email) ? "info@school.com" : due.Email;
        var address = string.IsNullOrWhiteSpace(due.Address) ? "Dhaka" : due.Address;
        var note = "Sikkhaloy Invoice - SchoolID:" + session.SchoolID;
        try
        {
            var url = await CreateShurjoPayOrderAsync(
                session.SchoolID, due.TotalPayable, due.Due, name, phone, email, address, note, ct);
            if (string.IsNullOrWhiteSpace(url))
                return Fail("inv.payFail");
            return new InvoiceResult { Succeeded = true, CheckoutUrl = url };
        }
        catch (Exception ex)
        {
            return new InvoiceResult { Error = ex.Message };
        }
    }

    public async Task<PaidInvoiceListDto> GetPaidAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new PaidInvoiceListDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT r.InvoiceReceiptID, r.InvoiceReceipt_SN, r.TotalAmount, r.PaidDate,
       r.PaymentBy, r.PaidByUser, r.Collected_By, r.Payment_Method,
       op.SP_Message, op.Amount AS OpAmount
FROM dbo.AAP_Invoice_Receipt r
OUTER APPLY (
    SELECT TOP 1 Amount, SP_Message
    FROM dbo.AAP_Invoice_OnlinePayment
    WHERE SP_Message LIKE 'ReceiptID:' + CAST(r.InvoiceReceiptID AS varchar(20)) + ' |%'
    ORDER BY CreatedDate DESC
) op
WHERE r.SchoolID = @SchoolID
ORDER BY r.InvoiceReceiptID DESC
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var total = Dec(reader["TotalAmount"]);
            ParseGateway(S(reader["SP_Message"]), Dec(reader["OpAmount"]), total, out var charge, out var customer);
            dto.Rows.Add(new PaidInvoiceRowDto
            {
                InvoiceReceiptId = I(reader["InvoiceReceiptID"]),
                InvoiceReceiptSn = I(reader["InvoiceReceipt_SN"]),
                TotalAmount = total,
                GatewayCharge = charge,
                CustomerPaid = customer,
                PaymentBy = S(reader["PaymentBy"]),
                PaidByUser = S(reader["PaidByUser"]),
                CollectedBy = S(reader["Collected_By"]),
                PaymentMethod = S(reader["Payment_Method"]),
                PaidDate = Dt(reader["PaidDate"])
            });
        }
        return dto;
    }

    public async Task<PaidInvoiceReceiptDto> GetReceiptAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        var dto = new PaidInvoiceReceiptDto { InvoiceReceiptId = id };
        if (id <= 0) return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("""
SELECT r.InvoiceReceipt_SN, s.SchoolName, s.Address, s.Phone, s.Email,
       r.TotalAmount AS Total_Paid, t.Total_Due, t.Total_Discount, t.Total_Amount,
       r.PaidDate, r.PaymentBy, r.Collected_By, r.Payment_Method
FROM dbo.AAP_Invoice_Receipt r
INNER JOIN dbo.SchoolInfo s ON r.SchoolID = s.SchoolID
INNER JOIN (
    SELECT pr.InvoiceReceiptID,
           SUM(ISNULL(i.TotalAmount - i.PaidAmount - i.Discount, 0)) AS Total_Due,
           SUM(ISNULL(i.Discount, 0)) AS Total_Discount,
           SUM(ISNULL(i.TotalAmount, 0)) AS Total_Amount
    FROM dbo.AAP_Invoice_Payment_Record pr
    INNER JOIN dbo.AAP_Invoice i ON pr.InvoiceID = i.InvoiceID
    GROUP BY pr.InvoiceReceiptID
) t ON r.InvoiceReceiptID = t.InvoiceReceiptID
WHERE r.InvoiceReceiptID = @RID AND r.SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@RID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return dto;
            dto.Found = true;
            dto.InvoiceReceiptSn = I(reader["InvoiceReceipt_SN"]);
            dto.SchoolName = S(reader["SchoolName"]);
            dto.Address = S(reader["Address"]);
            dto.Phone = S(reader["Phone"]);
            dto.Email = S(reader["Email"]);
            dto.PaymentBy = S(reader["PaymentBy"]);
            dto.CollectedBy = S(reader["Collected_By"]);
            dto.PaymentMethod = S(reader["Payment_Method"]);
            dto.PaidDate = Dt(reader["PaidDate"]);
            dto.TotalPaid = Dec(reader["Total_Paid"]);
            dto.TotalDue = Dec(reader["Total_Due"]);
            dto.TotalDiscount = Dec(reader["Total_Discount"]);
            dto.TotalAmount = Dec(reader["Total_Amount"]);
        }

        await using (var cmd = new SqlCommand("""
SELECT i.Invoice_For, i.Unit, i.UnitPrice, i.TotalAmount, i.Discount,
       c.InvoiceCategory, pr.Amount AS Paid,
       (i.TotalAmount - i.PaidAmount - i.Discount) AS Due
FROM dbo.AAP_Invoice i
INNER JOIN dbo.AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
INNER JOIN dbo.AAP_Invoice_Payment_Record pr ON i.InvoiceID = pr.InvoiceID
WHERE pr.InvoiceReceiptID = @RID AND i.SchoolID = @SchoolID
""", con))
        {
            cmd.Parameters.AddWithValue("@RID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Lines.Add(new PaidInvoiceLineDto
                {
                    InvoiceCategory = S(reader["InvoiceCategory"]),
                    InvoiceFor = S(reader["Invoice_For"]),
                    Unit = Dec(reader["Unit"]),
                    UnitPrice = Dec(reader["UnitPrice"]),
                    TotalAmount = Dec(reader["TotalAmount"]),
                    Discount = Dec(reader["Discount"]),
                    Paid = Dec(reader["Paid"]),
                    Due = Dec(reader["Due"])
                });
            }
        }

        dto.ShowDiscount = dto.Lines.Any(x => x.Discount > 0) || dto.TotalDiscount > 0;
        dto.ShowPaid = dto.Lines.Any(x => x.Paid > 0 && x.Paid < x.TotalAmount);
        dto.ShowDue = dto.Lines.Any(x => x.Due > 0) || dto.TotalDue > 0;

        await using (var cmd = new SqlCommand("""
SELECT TOP 1 Amount, SP_Message
FROM dbo.AAP_Invoice_OnlinePayment
WHERE SP_Message LIKE @Pattern
ORDER BY CreatedDate DESC
""", con))
        {
            cmd.Parameters.AddWithValue("@Pattern", "ReceiptID:" + id + " |%");
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                ParseGateway(S(reader["SP_Message"]), Dec(reader["Amount"]), dto.TotalPaid, out var charge, out var customer);
                if (charge > 0)
                {
                    dto.GatewayCharge = charge;
                    dto.CustomerPaid = customer;
                    dto.HasGatewayCharge = true;
                }
            }
        }

        return dto;
    }

    private static async Task<SubscriptionStatusDto> ReadStatusAsync(
        SqlConnection con, int schoolId, CancellationToken ct)
    {
        var dto = new SubscriptionStatusDto();
        await using (var cmd = new SqlCommand("""
SELECT COUNT(*) AS DueCount,
       ISNULL(SUM(TotalAmount - PaidAmount - Discount), 0) AS Due
FROM dbo.AAP_Invoice
WHERE SchoolID = @SID AND IsPaid = 0
""", con))
        {
            cmd.Parameters.AddWithValue("@SID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.DueCount = I(reader["DueCount"]);
                dto.Due = Dec(reader["Due"]);
            }
        }
        dto.HasDue = dto.DueCount > 0 && dto.Due > 0;

        var graceUntil = await ReadGraceUntilAsync(con, schoolId, ct);
        if (graceUntil is DateTime grace && grace.Date >= DateTime.Today)
        {
            dto.InGrace = true;
            dto.DaysUntilExpiry = (int)(grace.Date - DateTime.Today).TotalDays;
        }

        await using (var cmd = new SqlCommand("""
SELECT COUNT(*) FROM dbo.AAP_Invoice
WHERE SchoolID = @SID AND IsPaid = 0
  AND EndDate IS NOT NULL AND CAST(EndDate AS DATE) < CAST(GETDATE() AS DATE)
""", con))
        {
            cmd.Parameters.AddWithValue("@SID", schoolId);
            var expired = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0);
            if (expired > 0 && dto.HasDue && !dto.InGrace)
            {
                dto.IsBlocked = true;
                dto.DaysUntilExpiry = 0;
                return dto;
            }
        }

        if (dto.InGrace)
            return dto;

        await using (var cmd = new SqlCommand("""
SELECT MIN(CAST(EndDate AS DATE)) FROM dbo.AAP_Invoice
WHERE SchoolID = @SID AND IsPaid = 0
  AND EndDate IS NOT NULL AND CAST(EndDate AS DATE) >= CAST(GETDATE() AS DATE)
""", con))
        {
            cmd.Parameters.AddWithValue("@SID", schoolId);
            var future = await cmd.ExecuteScalarAsync(ct);
            if (future is DateTime nearest)
                dto.DaysUntilExpiry = (int)(nearest.Date - DateTime.Today).TotalDays;
            else if (future is not null && future is not DBNull)
                dto.DaysUntilExpiry = (int)(Convert.ToDateTime(future).Date - DateTime.Today).TotalDays;
        }

        return dto;
    }

    private static async Task<DateTime?> ReadGraceUntilAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(
                "SELECT AccessGraceUntil FROM dbo.SchoolInfo WHERE SchoolID = @SID", con);
            cmd.Parameters.AddWithValue("@SID", schoolId);
            var value = await cmd.ExecuteScalarAsync(ct);
            if (value is DateTime date)
                return date;
            if (value is not null && value is not DBNull)
                return Convert.ToDateTime(value);
        }
        catch (SqlException)
        {
        }
        return null;
    }

    private static async Task<string?> CreateShurjoPayOrderAsync(
        int schoolId, decimal amount, decimal invoiceAmt, string name, string phone, string email, string address,
        string note, CancellationToken ct)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var tokenBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = ShurjoPayUser,
            ["password"] = ShurjoPayPassword
        });
        using var tokenRes = await http.PostAsync(ShurjoPayBase + "/api/get_token", tokenBody, ct);
        var tokenJson = await tokenRes.Content.ReadAsStringAsync(ct);
        using var tokenDoc = JsonDocument.Parse(tokenJson);
        var root = tokenDoc.RootElement;
        var token = JsonText(root, "token");
        var storeId = JsonText(root, "store_id") ?? "";
        var execute = JsonText(root, "execute_url");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("ShurjoPay token নেওয়া সম্ভব হয়নি।");

        var orderId = ShurjoPayPrefix + "_" + schoolId + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        var payUrl = string.IsNullOrWhiteSpace(execute) ? ShurjoPayBase + "/api/secret-pay" : execute;
        using var payReq = new HttpRequestMessage(HttpMethod.Post, payUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token!,
                ["store_id"] = storeId,
                ["prefix"] = ShurjoPayPrefix,
                ["currency"] = "BDT",
                ["return_url"] = ReturnUrl,
                ["cancel_url"] = CancelUrl,
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
            var msg = JsonText(pay, "message") ?? JsonText(pay, "sp_massage") ?? "checkout_url পাওয়া যায়নি।";
            throw new InvalidOperationException("ShurjoPay গেটওয়ে এরর: " + msg);
        }
        return checkout;
    }

    private static void ParseGateway(string message, decimal opAmount, decimal total, out decimal charge, out decimal customer)
    {
        charge = 0;
        customer = opAmount > 0 ? opAmount : total;
        foreach (var part in (message ?? "").Split('|'))
        {
            var kv = part.Trim().Split(':', 2);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim();
            var val = kv[1].Trim();
            if (key.Equals("GatewayCharge", StringComparison.OrdinalIgnoreCase))
                decimal.TryParse(val, out charge);
            if (key.Equals("CustomerPaid", StringComparison.OrdinalIgnoreCase))
                decimal.TryParse(val, out customer);
        }
        if (charge <= 0)
            customer = 0;
        else if (customer <= 0)
            customer = total + charge;
    }

    private static InvoiceResult Fail(string error) => new() { Error = error };

    private static string? FirstPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Split([',', '/', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
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

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static decimal Dec(object? value) => value is null or DBNull ? 0 : Convert.ToDecimal(value);
    private static DateTime? Dt(object? value) => value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);
}
