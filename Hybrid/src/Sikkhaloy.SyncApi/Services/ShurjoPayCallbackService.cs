using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Sikkhaloy.SyncApi.Services;

public sealed class ShurjoPayCallbackService
{
    private const string DefaultShurjoPayBase = "https://engine.shurjopayment.com";
    private const string DefaultHybridAppResultUrl = "https://hybrid.sikkhaloy.com/payment-result";

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> OrderLocks = new();

    private readonly EduConnectionFactory _connections;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public ShurjoPayCallbackService(EduConnectionFactory connections, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _connections = connections;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    // ShurjoPay merchant login comes from configuration (ShurjoPay:Username / ShurjoPay:Password in the
    // gitignored appsettings.Secrets.json or environment variables ShurjoPay__Username / ShurjoPay__Password),
    // never from source code.
    private string ShurjoPayBase => (string.IsNullOrWhiteSpace(_config["ShurjoPay:BaseUrl"]) ? DefaultShurjoPayBase : _config["ShurjoPay:BaseUrl"]!).TrimEnd('/');
    private string ShurjoPayUser => _config["ShurjoPay:Username"] ?? "";
    private string ShurjoPayPassword => _config["ShurjoPay:Password"] ?? "";
    private string HybridAppResultUrl => string.IsNullOrWhiteSpace(_config["ShurjoPay:HybridResultUrl"]) ? DefaultHybridAppResultUrl : _config["ShurjoPay:HybridResultUrl"]!;

    public async Task<ShurjoPayCallbackResult> CompleteAsync(string? spOrderId, CancellationToken ct)
    {
        spOrderId = (spOrderId ?? "").Trim();
        if (spOrderId.Length == 0)
        {
            return new ShurjoPayCallbackResult
            {
                Status = "failed",
                Title = "Payment failed",
                Kind = "invoice",
                Message = "Order id missing."
            };
        }

        var gate = OrderLocks.GetOrAdd(spOrderId, _ => new SemaphoreSlim(1, 1));
        if (!await gate.WaitAsync(TimeSpan.FromSeconds(30), ct))
        {
            return new ShurjoPayCallbackResult
            {
                Status = "failed",
                Title = "Please wait",
                Kind = "invoice",
                Message = "Another request is still processing this payment."
            };
        }

        try
        {
            var verify = await VerifyPaymentAsync(spOrderId, ct);
            if (verify is null)
            {
                return new ShurjoPayCallbackResult
                {
                    Status = "failed",
                    Title = "Payment failed",
                    Kind = "invoice",
                    OrderId = spOrderId,
                    Message = "Could not verify payment with ShurjoPay."
                };
            }

            var ourOrderId = verify.order_id ?? spOrderId;
            var schoolId = 0;
            if (!string.IsNullOrWhiteSpace(verify.value1))
                int.TryParse(verify.value1, out schoolId);

            var invoiceNote = verify.value2 ?? "";
            var isSms = invoiceNote.StartsWith("SMS_RECHARGE|", StringComparison.OrdinalIgnoreCase)
                        || ourOrderId.StartsWith("SMSR_", StringComparison.OrdinalIgnoreCase)
                        || spOrderId.StartsWith("SMSR_", StringComparison.OrdinalIgnoreCase);

            var result = new ShurjoPayCallbackResult
            {
                OrderId = ourOrderId,
                Amount = verify.recv_amt ?? verify.amount ?? "",
                Trx = verify.bank_trx_id ?? "",
                Kind = isSms ? "sms" : "invoice"
            };

            if (!IsSuccessStatus(verify.sp_code, verify.bank_status, verify.transaction_status))
            {
                result.Status = "failed";
                result.Title = "Payment failed";
                result.Message = verify.bank_status ?? verify.transaction_status ?? "Failed";
                return result;
            }

            bool saved = isSms
                ? await SaveSmsAsync(spOrderId, ourOrderId, verify, schoolId, invoiceNote, ct)
                : await MarkInvoiceAsync(spOrderId, ourOrderId, verify, schoolId, invoiceNote, ct);

            if (saved)
            {
                result.Status = "success";
                result.Title = "Payment successful";
                result.Message = isSms ? "SMS recharge saved." : "Invoice payment saved.";
            }
            else
            {
                result.Status = "failed";
                result.Title = "Payment failed";
                result.Message = "Verified with gateway but could not update school records.";
            }
            return result;
        }
        catch (Exception ex)
        {
            return new ShurjoPayCallbackResult
            {
                Status = "failed",
                Title = "Payment failed",
                Kind = "invoice",
                OrderId = spOrderId,
                Message = ex.Message
            };
        }
        finally
        {
            gate.Release();
            if (gate.CurrentCount == 1)
                OrderLocks.TryRemove(spOrderId, out _);
        }
    }

    public string BuildRedirectUrl(ShurjoPayCallbackResult result)
    {
        var url = HybridAppResultUrl
            + "?status=" + Uri.EscapeDataString(result.Status ?? "failed")
            + "&kind=" + Uri.EscapeDataString(result.Kind ?? "invoice")
            + "&order=" + Uri.EscapeDataString(result.OrderId ?? "")
            + "&amount=" + Uri.EscapeDataString(result.Amount ?? "")
            + "&trx=" + Uri.EscapeDataString(result.Trx ?? "")
            + "&msg=" + Uri.EscapeDataString(result.Title ?? "");
        return url;
    }

    private async Task<ShurjoPayVerifyDto?> VerifyPaymentAsync(string spOrderId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ShurjoPayUser) || string.IsNullOrWhiteSpace(ShurjoPayPassword))
            throw new InvalidOperationException("ShurjoPay credentials are not configured (ShurjoPay:Username / ShurjoPay:Password).");

        var http = _httpClientFactory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(30);

        using var tokenBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = ShurjoPayUser,
            ["password"] = ShurjoPayPassword
        });
        using var tokenRes = await http.PostAsync(ShurjoPayBase + "/api/get_token", tokenBody, ct);
        var tokenJson = await tokenRes.Content.ReadAsStringAsync(ct);
        using var tokenDoc = JsonDocument.Parse(tokenJson);
        var token = JsonText(tokenDoc.RootElement, "token");
        if (string.IsNullOrWhiteSpace(token))
            return null;

        using var verifyReq = new HttpRequestMessage(HttpMethod.Post, ShurjoPayBase + "/api/verification")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["order_id"] = spOrderId
            })
        };
        verifyReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var verifyRes = await http.SendAsync(verifyReq, ct);
        var raw = (await verifyRes.Content.ReadAsStringAsync(ct)).Trim();
        if (raw.Length == 0)
            return null;

        JsonElement root;
        using var doc = JsonDocument.Parse(raw);
        root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            if (root.GetArrayLength() == 0) return null;
            root = root[0];
        }

        return new ShurjoPayVerifyDto
        {
            order_id = JsonText(root, "order_id"),
            amount = JsonText(root, "amount"),
            payable_amount = JsonText(root, "payable_amount"),
            method = JsonText(root, "method"),
            sp_code = JsonText(root, "sp_code"),
            sp_message = JsonText(root, "sp_message"),
            value1 = JsonText(root, "value1"),
            value2 = JsonText(root, "value2"),
            value3 = JsonText(root, "value3"),
            value4 = JsonText(root, "value4"),
            transaction_status = JsonText(root, "transaction_status"),
            bank_trx_id = JsonText(root, "bank_trx_id"),
            recv_amt = JsonText(root, "recv_amt"),
            recv_dt = JsonText(root, "recv_dt"),
            bank_status = JsonText(root, "bank_status")
        };
    }

    private static string? JsonText(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p)) return null;
        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),
            JsonValueKind.Number => p.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => p.GetRawText()
        };
    }


    private static bool IsSuccessStatus(string? spCode, string? bankStatus, string? transStatus)
    {
        if (!string.IsNullOrEmpty(spCode) && (spCode == "1000" || spCode == "200"))
            return true;
        if (!string.IsNullOrEmpty(bankStatus) &&
            (bankStatus.Equals("Success", StringComparison.OrdinalIgnoreCase) ||
             bankStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
            return true;
        if (!string.IsNullOrEmpty(transStatus) &&
            (transStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) ||
             transStatus.Equals("Success", StringComparison.OrdinalIgnoreCase) ||
             transStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
            return true;
        return false;
    }

    private async Task<bool> SaveSmsAsync(string spOrderId, string orderId, ShurjoPayVerifyDto verify, int schoolId, string invoiceNote, CancellationToken ct)
    {
        // invoiceNote = "SMS_RECHARGE|smsQty|registrationID|schoolID"
        int smsQty         = 0;
        int registrationID = 0;

        if (invoiceNote.StartsWith("SMS_RECHARGE|"))
        {
            // Format: SMS_RECHARGE|smsQty|registrationID|schoolID
            // Split gives: [0]=SMS_RECHARGE [1]=smsQty [2]=registrationID [3]=schoolID
            var parts = invoiceNote.Split('|');
            if (parts.Length >= 2) int.TryParse(parts[1], out smsQty);
            if (parts.Length >= 3) int.TryParse(parts[2], out registrationID);
            if (parts.Length >= 4 && schoolId <= 0) int.TryParse(parts[3], out schoolId);
        }

        // Debug info — screen-এ দেখাও
        string debugInfo = "schoolId=" + schoolId + " | smsQty=" + smsQty
                         + " | regID=" + registrationID
                         + " | orderId=" + orderId
                         + " | value1=" + (verify.value1 ?? "null")
                         + " | value2=" + (verify.value2 ?? "null")
                         + " | sp_code=" + (verify.sp_code ?? "null")
                         + " | bank_status=" + (verify.bank_status ?? "null")
                         + " | recv_amt=" + (verify.recv_amt ?? "null");

        if (smsQty <= 0 || schoolId <= 0)
        {
            return false;
        }

        double  totalAmount   = smsQty * 0.36;
        decimal invoiceDueAmt = (decimal)totalAmount;

        // Amounts from ShurjoPay verify response
        decimal customerPaidAmt = 0m;
        decimal recvAmtSms      = 0m;
        decimal spAmtSms        = 0m;

        decimal.TryParse(verify.payable_amount ?? "", out customerPaidAmt);
        decimal.TryParse(verify.recv_amt       ?? "", out recvAmtSms);
        decimal.TryParse(verify.amount         ?? "", out spAmtSms);

        if (invoiceDueAmt <= 0m)
            invoiceDueAmt = recvAmtSms > 0m ? recvAmtSms : spAmtSms;
        if (customerPaidAmt <= 0m)
            customerPaidAmt = spAmtSms > 0m ? spAmtSms : invoiceDueAmt;

        decimal paidAmt = recvAmtSms > 0m ? recvAmtSms : invoiceDueAmt;

        // Gateway charge calculation (same dual-mode logic)
        decimal gatewayCharge = 0m;
        if (customerPaidAmt > invoiceDueAmt + 0.01m)
        {
            gatewayCharge = customerPaidAmt - invoiceDueAmt;
        }
        else if (recvAmtSms > 0m && spAmtSms > recvAmtSms + 0.01m)
        {
            gatewayCharge   = spAmtSms - recvAmtSms;
            customerPaidAmt = spAmtSms;
        }
        if (gatewayCharge < 0m) gatewayCharge = 0m;

        // ShurjoPay verify API-এ payable_amount সবসময় original amount return করে।
        // তাই প্রতি হাজারে ১৯ টাকা হারে manually gateway charge calculate করা হচ্ছে।
        bool isSimulated = false;
            if (gatewayCharge == 0m && invoiceDueAmt > 0m)
            {
                gatewayCharge   = Math.Round(invoiceDueAmt / 1000m * 19m, 2);
                customerPaidAmt = invoiceDueAmt + gatewayCharge;
            }

        string  method      = verify.method ?? "ShurjoPay";
        string  trxId       = verify.bank_trx_id ?? orderId;
        string  paymentByName = await GetUserDisplayNameAsync(registrationID, schoolId, ct);

        try
        {
await using (var conn = _connections.Create())
            {
                await conn.OpenAsync(ct);
                await using var tran = (SqlTransaction)await conn.BeginTransactionAsync(ct);
                try
                {
                    // ─── PRIMARY Duplicate check ───────────────────────────────────────────
                    // SP_OrderID (ShurjoPay unique ID) দিয়ে SMS_Balance update হয়েছে কিনা
                    // check করি — এটাই সবচেয়ে নির্ভরযোগ্য guard
                    // SP_OrderID column exist করে কিনা আগে check করি
                    bool spOrderIdColumnExists = false;
                    await using (var colChk = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SMS_Recharge_Record') AND name='SP_OrderID'", conn, tran))
                    {
                        spOrderIdColumnExists = (int)await colChk.ExecuteScalarAsync(ct) > 0;
                    }

                    if (spOrderIdColumnExists)
                    {
                        // SP_OrderID দিয়ে ইতিমধ্যে balance দেওয়া হয়েছে কিনা — UPDLOCK দিয়ে lock নাও
                        await using (var chkSms = new SqlCommand(
                            "SELECT COUNT(*) FROM SMS_Recharge_Record WITH (UPDLOCK,HOLDLOCK) WHERE SP_OrderID=@OID", conn, tran))
                        {
                            chkSms.Parameters.AddWithValue("@OID", spOrderId);
                            if ((int)await chkSms.ExecuteScalarAsync(ct) > 0)
                            {
                                await tran.RollbackAsync(ct);
                                return true;
                            }
                        }
                    }

                    // ─── SECONDARY Duplicate check ─────────────────────────────────────────
                    // AAP_Invoice_OnlinePayment table-এও check করি (UPDLOCK দিয়ে)
                    await using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM AAP_Invoice_OnlinePayment WITH (UPDLOCK,HOLDLOCK) WHERE SP_OrderID=@OID", conn, tran))
                    {
                        chk.Parameters.AddWithValue("@OID", spOrderId);
                        if ((int)await chk.ExecuteScalarAsync(ct) > 0)
                        {
                            await tran.RollbackAsync(ct);
                            return true;
                        }
                    }

                    // 1. SMS_Recharge_Record — SP_OrderID সহ insert (Total_Price computed column, তাই বাদ)
                    string insertSql = spOrderIdColumnExists
                        ? "INSERT INTO SMS_Recharge_Record(SchoolID,RechargeSMS,PerSMS_Price,Date,Is_Paid,RegistrationID,SP_OrderID)"
                          + " VALUES(@SchoolID,@RechargeSMS,@PerSMS_Price,GETDATE(),1,@RegistrationID,@OID)"
                        : "INSERT INTO SMS_Recharge_Record(SchoolID,RechargeSMS,PerSMS_Price,Date,Is_Paid,RegistrationID)"
                          + " VALUES(@SchoolID,@RechargeSMS,@PerSMS_Price,GETDATE(),1,@RegistrationID)";
                    await using (var cmd = new SqlCommand(insertSql, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                        cmd.Parameters.AddWithValue("@RechargeSMS",    smsQty);
                        cmd.Parameters.AddWithValue("@PerSMS_Price",   0.36);
                        cmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                        if (spOrderIdColumnExists)
                            cmd.Parameters.AddWithValue("@OID",        spOrderId);
                        await cmd.ExecuteNonQueryAsync(ct);
                    }

                    // NOTE: SMS_Recharge_Record table-এ Tr_SMS_Recharge_InsertUpdate trigger আছে
                    // যা INSERT হলে স্বয়ংক্রিয়ভাবে SMS.SMS_Balance update করে।
                    // তাই এখানে আলাদাভাবে balance update করার দরকার নেই।

                    // 3. Invoice category
                    int invoiceCategoryID = 0;
                    await using (var catCmd = new SqlCommand(
                        "SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory=N'SMS'", conn, tran))
                    {
                        object catResult = await catCmd.ExecuteScalarAsync(ct);
                        if (catResult != null) invoiceCategoryID = Convert.ToInt32(catResult);
                    }

                    int newInvoiceID = 0;
                    if (invoiceCategoryID > 0)
                    {
                        DateTime issueDate = DateTime.Now;
                        string   invoiceFor = "SMS Recharge: " + smsQty + " SMS @ 0.36 (" + issueDate.ToString("d MMM yyyy") + ")";
                        string   monthName  = issueDate.ToString("MMM yyyy");

                        // IsPaid is a computed column — insert without it, then UPDATE afterwards
                        await using (var invCmd = new SqlCommand(
                            "INSERT INTO AAP_Invoice(RegistrationID,InvoiceCategoryID,SchoolID,IssuDate,EndDate,"
                            + "Invoice_For,TotalAmount,MonthName,Invoice_SN,Unit,UnitPrice,PaidAmount)"
                            + " VALUES(@RegistrationID,@InvoiceCategoryID,@SchoolID,@IssuDate,@IssuDate,"
                            + "@Invoice_For,@TotalAmount,@MonthName,dbo.Invoice_SerialNumber(@SchoolID),@Unit,@UnitPrice,@TotalAmount);"
                            + " SELECT SCOPE_IDENTITY();", conn, tran))
                        {
                            invCmd.Parameters.AddWithValue("@RegistrationID",    registrationID > 0 ? (object)registrationID : DBNull.Value);
                            invCmd.Parameters.AddWithValue("@InvoiceCategoryID", invoiceCategoryID);
                            invCmd.Parameters.AddWithValue("@SchoolID",          schoolId);
                            invCmd.Parameters.AddWithValue("@IssuDate",          issueDate);
                            invCmd.Parameters.AddWithValue("@Invoice_For",       invoiceFor);
                            invCmd.Parameters.AddWithValue("@TotalAmount",       totalAmount);
                            invCmd.Parameters.AddWithValue("@MonthName",         monthName);
                            invCmd.Parameters.AddWithValue("@Unit",              smsQty);
                            invCmd.Parameters.AddWithValue("@UnitPrice",         0.36);
                            object idResult = await invCmd.ExecuteScalarAsync(ct);
                            if (idResult != null) newInvoiceID = Convert.ToInt32(idResult);
                        }

                        // UPDATE NumberOfPayment and LastPaidDate (these are regular columns)
                        if (newInvoiceID > 0)
                        {
                            await using (var updInv = new SqlCommand(
                                "UPDATE AAP_Invoice SET NumberOfPayment=1, LastPaidDate=GETDATE() WHERE InvoiceID=@InvoiceID", conn, tran))
                            {
                                updInv.Parameters.AddWithValue("@InvoiceID", newInvoiceID);
                                await updInv.ExecuteNonQueryAsync(ct);
                            }
                        }
                    }

                    // 4. AAP_Invoice_Receipt
                    int receiptID = 0;
                    await using (var rcpCmd = new SqlCommand(
                        "INSERT INTO AAP_Invoice_Receipt(SchoolID,RegistrationID,InvoiceReceipt_SN,TotalAmount,PaidDate,PaymentBy,Collected_By,Payment_Method,PaidByUser)"
                        + " VALUES(@SchoolID,@RegistrationID,dbo.F_InvoiceReceipt_SN(),@Amt,GETDATE(),@PayBy,@ColBy,@Method,@PaidByUser);"
                        + " SELECT SCOPE_IDENTITY();", conn, tran))
                    {
                        rcpCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                        rcpCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                        rcpCmd.Parameters.AddWithValue("@Amt",            paidAmt);
                        rcpCmd.Parameters.AddWithValue("@PayBy",          paymentByName);
                        rcpCmd.Parameters.AddWithValue("@ColBy",          "Sikkhaloy Hybrid (ShurjoPay)");
                        rcpCmd.Parameters.AddWithValue("@Method",         method);
                        rcpCmd.Parameters.AddWithValue("@PaidByUser",     paymentByName);
                        object ridResult = await rcpCmd.ExecuteScalarAsync(ct);
                        if (ridResult != null) receiptID = Convert.ToInt32(ridResult);
                    }

                    // 5. AAP_Invoice_Payment_Record
                    if (newInvoiceID > 0 && receiptID > 0)
                    {
                        await using (var prCmd = new SqlCommand(
                            "INSERT INTO AAP_Invoice_Payment_Record(InvoiceID,InvoiceReceiptID,RegistrationID,SchoolID,Amount,PaidDate)"
                            + " VALUES(@InvoiceID,@ReceiptID,@RegistrationID,@SchoolID,@Amount,GETDATE())", conn, tran))
                        {
                            prCmd.Parameters.AddWithValue("@InvoiceID",      newInvoiceID);
                            prCmd.Parameters.AddWithValue("@ReceiptID",      receiptID);
                            prCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                            prCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                            prCmd.Parameters.AddWithValue("@Amount",         paidAmt);
                            await prCmd.ExecuteNonQueryAsync(ct);
                        }
                    }

                    // 6. Online payment log — ReceiptID + gateway charge info
                    string smsChargeInfo = string.Format(
                        "ReceiptID:{0} | Invoice: {1:F2} | GatewayCharge: {2:F2} | CustomerPaid: {3:F2} | SMS: {4}",
                        receiptID, invoiceDueAmt, gatewayCharge, customerPaidAmt, smsQty);

                    await using (var logCmd = new SqlCommand(
                        "INSERT INTO AAP_Invoice_OnlinePayment"
                        + "(SchoolID,SP_OrderID,SP_TrxID,SP_Method,Amount,SP_Code,SP_Message,PaymentDate,CreatedDate)"
                        + " VALUES(@SchoolID,@OID,@TrxID,@Method,@Amt,@Code,@Msg,@PDate,GETDATE())", conn, tran))
                    {
                        logCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        logCmd.Parameters.AddWithValue("@OID",      spOrderId);
                        logCmd.Parameters.AddWithValue("@TrxID",    trxId);
                        logCmd.Parameters.AddWithValue("@Method",   method);
                        logCmd.Parameters.AddWithValue("@Amt",      customerPaidAmt > 0m ? customerPaidAmt : paidAmt);
                        logCmd.Parameters.AddWithValue("@Code",     verify.sp_code ?? "");
                        logCmd.Parameters.AddWithValue("@Msg",      smsChargeInfo.Length > 500 ? smsChargeInfo.Substring(0, 500) : smsChargeInfo);
                        logCmd.Parameters.AddWithValue("@PDate",    DateTime.Now);
                        await logCmd.ExecuteNonQueryAsync(ct);
                    }

                    
                    // Keep grace while other invoices are still unpaid (SMS recharge must not lock the school).
                    using (var graceCmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM AAP_Invoice WHERE SchoolID = @SID AND IsPaid = 0)
    UPDATE SchoolInfo SET AccessGraceUntil = NULL
    WHERE SchoolID = @SID AND AccessGraceUntil IS NOT NULL", conn, tran))
                    {
                        graceCmd.Parameters.AddWithValue("@SID", schoolId);
                        graceCmd.ExecuteNonQuery();
                    }

                    await tran.CommitAsync(ct);

                    // Success message with gateway charge breakdown
                    string smsSuccessMsg = string.Format(
                        "<i class='fa fa-check-circle'></i> SMS রিচার্জ সফল! <b>{0} SMS</b> যোগ হয়েছে।", smsQty);
                    if (gatewayCharge > 0m)
                    {
                        if (isSimulated)
                            smsSuccessMsg += string.Format(
                                "<br/><small class='text-info'>🧪 [Sandbox] বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ: <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                invoiceDueAmt, gatewayCharge, customerPaidAmt);
                        else
                            smsSuccessMsg += string.Format(
                                "<br/><small>বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ: <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                invoiceDueAmt, gatewayCharge, customerPaidAmt);
                    }
                    return true;
                }
                catch (Exception)
                {
                    await tran.RollbackAsync(ct);
                    return false;
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<string> GetUserDisplayNameAsync(int registrationID, int schoolId, CancellationToken ct)
    {
        try
        {
await using (var conn = _connections.Create())
            {
                await conn.OpenAsync(ct);
                if (registrationID > 0)
                {
                    await using (var cmd = new SqlCommand(@"
SELECT TOP 1 Name FROM (
    SELECT LTRIM(RTRIM(FirstName + ISNULL(' ' + NULLIF(LastName,''), ''))) AS Name
    FROM Admin WHERE RegistrationID = @RID
    UNION ALL
    SELECT LTRIM(RTRIM(FirstName + ISNULL(' ' + NULLIF(LastName,''), '')))
    FROM Teacher WHERE TeacherRegistrationID = @RID
    UNION ALL
    SELECT LTRIM(RTRIM(FirstName + ISNULL(' ' + NULLIF(LastName,''), '')))
    FROM Staff_Info WHERE RegistrationID = @RID
) n
WHERE Name IS NOT NULL AND Name <> ''", conn))
                    {
                        cmd.Parameters.AddWithValue("@RID", registrationID);
                        object result = await cmd.ExecuteScalarAsync(ct);
                        if (result != null && result != DBNull.Value)
                        {
                            string name = result.ToString().Trim();
                            if (!string.IsNullOrEmpty(name)) return name;
                        }
                    }
                    await using (var cmd2 = new SqlCommand(
                        "SELECT TOP 1 UserName FROM Registration WHERE RegistrationID=@RID", conn))
                    {
                        cmd2.Parameters.AddWithValue("@RID", registrationID);
                        object result2 = await cmd2.ExecuteScalarAsync(ct);
                        if (result2 != null && result2 != DBNull.Value)
                        {
                            string uname = result2.ToString().Trim();
                            if (!string.IsNullOrEmpty(uname)) return uname;
                        }
                    }
                }
                // 3. SchoolID দিয়ে SchoolName return করি fallback হিসেবে
                if (schoolId > 0)
                {
                    await using (var cmd3 = new SqlCommand(
                        "SELECT TOP 1 SchoolName FROM SchoolInfo WHERE SchoolID=@SID", conn))
                    {
                        cmd3.Parameters.AddWithValue("@SID", schoolId);
                        object result3 = cmd3.ExecuteScalar();
                        if (result3 != null && result3 != DBNull.Value)
                        {
                            string sname = result3.ToString().Trim();
                            if (!string.IsNullOrEmpty(sname)) return sname;
                        }
                    }
                }
            }
        }
        catch { }
        return "ShurjoPay Online";
    }

    private async Task<bool> MarkInvoiceAsync(string spOrderId, string ourOrderId, ShurjoPayVerifyDto verify, int schoolId, string invoiceNote, CancellationToken ct)
    {
        try
        {
            // Invoice due amount (value3 থেকে) — আমরা ShurjoPay-এ পাঠিয়েছিলাম
            decimal invoiceDueAmount = 0m;
            if (!string.IsNullOrEmpty(verify.value3))
                decimal.TryParse(verify.value3, out invoiceDueAmount);

            // Amounts from ShurjoPay verify response
            decimal customerPaidAmount = 0m;
            decimal recvAmt            = 0m;
            decimal spAmount           = 0m;

            decimal.TryParse(verify.payable_amount ?? "", out customerPaidAmount);
            decimal.TryParse(verify.recv_amt       ?? "", out recvAmt);
            decimal.TryParse(verify.amount         ?? "", out spAmount);

            // Invoice fallback
            if (invoiceDueAmount <= 0m)
                invoiceDueAmount = recvAmt > 0m ? recvAmt : spAmount;

            // Customer paid: payable_amount > amount means customer-bearing charge
            // Otherwise use spAmount (what customer paid = invoice amount)
            if (customerPaidAmount <= 0m)
                customerPaidAmount = spAmount > 0m ? spAmount : invoiceDueAmount;

            // Gateway charge:
            // 1) Customer-bearing: payable_amount - invoiceDue
            // 2) Merchant-bearing fallback: spAmount - recv_amt (ShurjoPay deducted from merchant)
            decimal gatewayCharge = 0m;
            if (customerPaidAmount > invoiceDueAmount + 0.01m)
            {
                // customer paid more → customer-bearing
                gatewayCharge = customerPaidAmount - invoiceDueAmount;
            }
            else if (recvAmt > 0m && spAmount > recvAmt + 0.01m)
            {
                // merchant received less → merchant-bearing (show as info)
                gatewayCharge     = spAmount - recvAmt;
                customerPaidAmount = spAmount; // customer paid spAmount
            }
            if (gatewayCharge < 0m) gatewayCharge = 0m;

            // ShurjoPay verify API-এ payable_amount সবসময় original amount return করে।
                    // তাই 2.038% হারে manually gateway charge calculate করা হচ্ছে।
                    bool isSimulated = false;
                if (gatewayCharge == 0m && invoiceDueAmount > 0m)
                {
                    gatewayCharge      = Math.Round(invoiceDueAmount / 1000m * 10m, 2);
                    customerPaidAmount = invoiceDueAmount + gatewayCharge;
                }

            string  method  = verify.method ?? "ShurjoPay";
            string  trxId   = verify.bank_trx_id ?? ourOrderId;

            int registrationID = await ResolvePayerRegistrationIdAsync(verify, schoolId, invoiceNote, ct);
            string paymentByName = await GetUserDisplayNameAsync(registrationID, schoolId, ct);
await using (var conn = _connections.Create())
            {
                await conn.OpenAsync(ct);
                await using var tran = (SqlTransaction)await conn.BeginTransactionAsync(ct);
                try
                {
                    // Duplicate check — spOrderId দিয়ে
                    await using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM AAP_Invoice_OnlinePayment WITH (UPDLOCK,HOLDLOCK) WHERE SP_OrderID=@OID", conn, tran))
                    {
                        chk.Parameters.AddWithValue("@OID", spOrderId);
                        if ((int)await chk.ExecuteScalarAsync(ct) > 0)
                        {
                            await tran.RollbackAsync(ct);
                            return true;
                        }
                    }

                    // 1. AAP_Invoice_Receipt insert (receipt amount = invoice due, not customer paid)
                    int receiptID = 0;
                    await using (var rcpCmd = new SqlCommand(
                        "INSERT INTO AAP_Invoice_Receipt(SchoolID,RegistrationID,InvoiceReceipt_SN,TotalAmount,PaidDate,PaymentBy,Collected_By,Payment_Method,PaidByUser)"
                        + " VALUES(@SchoolID,@RegistrationID,dbo.F_InvoiceReceipt_SN(),@Amt,GETDATE(),@PayBy,@ColBy,@Method,@PaidByUser);"
                        + " SELECT SCOPE_IDENTITY();", conn, tran))
                    {
                        rcpCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                        rcpCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                        rcpCmd.Parameters.AddWithValue("@Amt",            invoiceDueAmount);
                        rcpCmd.Parameters.AddWithValue("@PayBy",          paymentByName);
                        rcpCmd.Parameters.AddWithValue("@ColBy", "Sikkhaloy Hybrid (ShurjoPay)");
                        rcpCmd.Parameters.AddWithValue("@Method",         method);
                        rcpCmd.Parameters.AddWithValue("@PaidByUser",     paymentByName);
                        object ridResult = await rcpCmd.ExecuteScalarAsync(ct);
                        if (ridResult != null) receiptID = Convert.ToInt32(ridResult);
                    }

                    // 2. Unpaid invoices গুলো update করি এবং প্রতিটির জন্য Payment_Record insert করি
                    // NOTE: IsPaid computed column — PaidAmount আপডেট করলে স্বয়ংক্রিয়ভাবে IsPaid=1 হয়
                    if (schoolId > 0)
                    {
                        // আগে unpaid invoice IDs সংগ্রহ করি
                        var invoiceIds = new System.Collections.Generic.List<int>();
                        var invoiceAmounts = new System.Collections.Generic.Dictionary<int, decimal>();
                        await using (var selCmd = new SqlCommand(
                            @"SELECT InvoiceID, TotalAmount - ISNULL(Discount,0) - ISNULL(PaidAmount,0) AS DueAmt
                              FROM AAP_Invoice
                              WHERE SchoolID=@SID AND ISNULL(PaidAmount,0) < TotalAmount - ISNULL(Discount,0)", conn, tran))
                        {
                            selCmd.Parameters.AddWithValue("@SID", schoolId);
                            await using (var rdr = await selCmd.ExecuteReaderAsync(ct))
                            {
                                while (await rdr.ReadAsync(ct))
                                {
                                    int iid = Convert.ToInt32(rdr[0]);
                                    decimal due = Convert.ToDecimal(rdr[1]);
                                    invoiceIds.Add(iid);
                                    invoiceAmounts[iid] = due;
                                }
                            }
                        }

                        foreach (int invoiceId in invoiceIds)
                        {
                            // Update PaidAmount
                            await using (var updCmd = new SqlCommand(
                                @"UPDATE AAP_Invoice
                                  SET PaidAmount = TotalAmount - ISNULL(Discount,0),
                                      NumberOfPayment = ISNULL(NumberOfPayment,0) + 1,
                                      LastPaidDate = GETDATE()
                                  WHERE InvoiceID=@IID", conn, tran))
                            {
                                updCmd.Parameters.AddWithValue("@IID", invoiceId);
                                await updCmd.ExecuteNonQueryAsync(ct);
                            }

                            // Insert Payment_Record
                            if (receiptID > 0)
                            {
                                await using (var prCmd = new SqlCommand(
                                    "INSERT INTO AAP_Invoice_Payment_Record(InvoiceID,InvoiceReceiptID,RegistrationID,SchoolID,Amount,PaidDate)"
                                    + " VALUES(@InvoiceID,@ReceiptID,@RegistrationID,@SchoolID,@Amount,GETDATE())", conn, tran))
                                {
                                    prCmd.Parameters.AddWithValue("@InvoiceID",      invoiceId);
                                    prCmd.Parameters.AddWithValue("@ReceiptID",      receiptID);
                                    prCmd.Parameters.AddWithValue("@RegistrationID", registrationID > 0 ? (object)registrationID : DBNull.Value);
                                    prCmd.Parameters.AddWithValue("@SchoolID",       schoolId);
                                    prCmd.Parameters.AddWithValue("@Amount",         invoiceAmounts[invoiceId]);
                                    await prCmd.ExecuteNonQueryAsync(ct);
                                }
                            }
                        }
                    }

                    // 3. Online payment log — SP_Message-এ gateway charge + ReceiptID store
                    string chargeInfo = string.Format(
                        "ReceiptID:{0} | Invoice: {1:F2} | GatewayCharge: {2:F2} | CustomerPaid: {3:F2}",
                        receiptID, invoiceDueAmount, gatewayCharge, customerPaidAmount);

                    // AAP_Invoice_OnlinePayment table exist check
                    bool logTableExists = false;
                    await using (var chkTbl = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.tables WHERE name='AAP_Invoice_OnlinePayment'", conn, tran))
                    {
                        logTableExists = (int)await chkTbl.ExecuteScalarAsync(ct) > 0;
                    }

                    if (logTableExists)
                    {
                        await using (var logCmd = new SqlCommand(
                            "INSERT INTO AAP_Invoice_OnlinePayment"
                            + "(SchoolID,SP_OrderID,SP_TrxID,SP_Method,Amount,SP_Code,SP_Message,PaymentDate,CreatedDate)"
                            + " VALUES(@SchoolID,@OID,@TrxID,@Method,@Amt,@Code,@Msg,@PDate,GETDATE())", conn, tran))
                        {
                            logCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                            logCmd.Parameters.AddWithValue("@OID",      spOrderId);
                            logCmd.Parameters.AddWithValue("@TrxID",    trxId);
                            logCmd.Parameters.AddWithValue("@Method",   method);
                            logCmd.Parameters.AddWithValue("@Amt",      customerPaidAmount > 0m ? customerPaidAmount : invoiceDueAmount);
                            logCmd.Parameters.AddWithValue("@Code",     verify.sp_code ?? "");
                            logCmd.Parameters.AddWithValue("@Msg",      chargeInfo.Length > 500 ? chargeInfo.Substring(0, 500) : chargeInfo);
                            logCmd.Parameters.AddWithValue("@PDate",    DateTime.Now);
                            await logCmd.ExecuteNonQueryAsync(ct);
                        }
                    }

                    await tran.CommitAsync(ct);

                    // Success message-এ charge + log status দেখানো
                    string successMsg = "<i class='fa fa-check-circle'></i> পেমেন্ট সফলভাবে সম্পন্ন হয়েছে! ধন্যবাদ।";
                    if (gatewayCharge > 0m)
                    {
                        if (isSimulated)
                        {
                            successMsg += string.Format(
                                                "<br/><small class='text-info'>🧪 [Sandbox] বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ: <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                                invoiceDueAmount, gatewayCharge, customerPaidAmount);
                                        }
                                        else
                                        {
                                            successMsg += string.Format(
                                                "<br/><small>বিলের পরিমাণ: <b>{0:F2} ৳</b> | গেটওয়ে চার্জ: <b>{1:F2} ৳</b> | মোট পরিশোধিত: <b>{2:F2} ৳</b></small>",
                                                invoiceDueAmount, gatewayCharge, customerPaidAmount);
                        }
                    }
                    if (!logTableExists)
                    {
                        successMsg += "<br/><small class='text-warning'>⚠️ AAP_Invoice_OnlinePayment table পাওয়া যায়নি — log সংরক্ষিত হয়নি।</small>";
                    }

                    return true;
                }
                catch (Exception)
                {
                    await tran.RollbackAsync(ct);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("MarkInvoiceAsPaid error: " + ex.Message);
            return false;
        }
    }

    private async Task<int> ResolvePayerRegistrationIdAsync(ShurjoPayVerifyDto verify, int schoolId, string invoiceNote, CancellationToken ct)
    {
        int registrationID = 0;

        if (verify != null && !string.IsNullOrEmpty(verify.value4))
            int.TryParse(verify.value4, out registrationID);

        if (registrationID <= 0)
            registrationID = ParseRegIdFromNote(invoiceNote);

        if (registrationID <= 0)
            registrationID = await GetSchoolRegistrationIDAsync(schoolId, ct);

        return registrationID;
    }

    private static int ParseRegIdFromNote(string invoiceNote)
    {
        if (string.IsNullOrEmpty(invoiceNote)) return 0;
        const string marker = "RegID:";
        int idx = invoiceNote.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return 0;
        string rest = invoiceNote.Substring(idx + marker.Length);
        int end = 0;
        while (end < rest.Length && char.IsDigit(rest[end])) end++;
        if (end == 0) return 0;
        int registrationID;
        return int.TryParse(rest.Substring(0, end), out registrationID) ? registrationID : 0;
    }

    private async Task<int> GetSchoolRegistrationIDAsync(int schoolId, CancellationToken ct)
    {
        if (schoolId <= 0) return 0;
        try
        {
            await using (var conn = _connections.Create())
            {
                await conn.OpenAsync(ct);
                // 1. Registration table থেকে school-এর primary user নাও
                await using (var cmd = new SqlCommand(
                    "SELECT TOP 1 RegistrationID FROM Registration WHERE SchoolID=@SID ORDER BY RegistrationID", conn))
                {
                    cmd.Parameters.AddWithValue("@SID", schoolId);
                    object result = await cmd.ExecuteScalarAsync(ct);
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
                // 2. Admin table থেকে যেকোনো admin
                await using (var cmd2 = new SqlCommand(
                    "SELECT TOP 1 RegistrationID FROM Admin WHERE SchoolID=@SID ORDER BY RegistrationID", conn))
                {
                    cmd2.Parameters.AddWithValue("@SID", schoolId);
                    object result2 = await cmd2.ExecuteScalarAsync(ct);
                    if (result2 != null && result2 != DBNull.Value)
                        return Convert.ToInt32(result2);
                }
            }
        }
        catch { }
        return 0;
    }

}

public sealed class ShurjoPayCallbackResult
{
    public string? Status { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Kind { get; set; }
    public string? OrderId { get; set; }
    public string? Amount { get; set; }
    public string? Trx { get; set; }
}

internal sealed class ShurjoPayVerifyDto
{
    public string? order_id { get; set; }
    public string? amount { get; set; }
    public string? payable_amount { get; set; }
    public string? method { get; set; }
    public string? sp_code { get; set; }
    public string? sp_message { get; set; }
    public string? value1 { get; set; }
    public string? value2 { get; set; }
    public string? value3 { get; set; }
    public string? value4 { get; set; }
    public string? transaction_status { get; set; }
    public string? bank_trx_id { get; set; }
    public string? recv_amt { get; set; }
    public string? recv_dt { get; set; }
    public string? bank_status { get; set; }
}