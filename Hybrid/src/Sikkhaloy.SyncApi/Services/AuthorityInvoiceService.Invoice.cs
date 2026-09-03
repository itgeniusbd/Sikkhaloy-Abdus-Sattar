using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityInvoiceService
{
    public async Task<AuthCreatePageDto> GetCreatePageAsync(
        SessionSnapshot session, string? month, int otherSchoolId, string? smsFrom, string? smsTo, string? smsQ, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthCreatePageDto
        {
            Categories = await LoadCategoriesAsync(ct),
            Schools = await LoadAllSchoolsAsync(ct),
            Job = await LoadJobStatusAsync(ct)
        };
        dto.Months = await LoadCountMonthsAsync(ct);
        dto.SelectedMonth = string.IsNullOrWhiteSpace(month)
            ? dto.Months.FirstOrDefault()?.Extra ?? ""
            : month.Trim();
        dto.ServiceRows = await LoadServiceRowsAsync(dto.SelectedMonth, ct);
        dto.SmsRows = await LoadSmsRowsAsync(smsFrom, smsTo, smsQ, ct);
        dto.OtherInvoices = otherSchoolId > 0
            ? await LoadSchoolInvoicesAsync(otherSchoolId, ct)
            : [];
        dto.GraceRows = await LoadGraceRowsAsync(ct);
        return dto;
    }

    public async Task<AuthorityResult> GenerateStudentCountAsync(
        SessionSnapshot session, AuthGenerateCountRequest? request, CancellationToken ct)
    {
        Guard(session);
        var month = ParseDate(request?.Month);
        if (month is null) return Fail("ai.needMonth");
        var monthEnd = MonthEnd(month.Value);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("sp_Generate_Monthly_Student_Count", con)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };
        cmd.Parameters.AddWithValue("@TargetMonth", monthEnd);
        var count = new SqlParameter("@GeneratedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var msg = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(count);
        cmd.Parameters.Add(msg);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
            var text = S(msg.Value);
            if (text.StartsWith("Success", StringComparison.OrdinalIgnoreCase) || text.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                return Ok(string.IsNullOrWhiteSpace(text) ? "ai.countOk" : text);
            return Fail(string.IsNullOrWhiteSpace(text) ? "ai.countFail" : text);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<AuthorityResult> AutoGenerateAsync(
        SessionSnapshot session, AuthGenerateCountRequest? request, CancellationToken ct)
    {
        Guard(session);
        var month = ParseDate(request?.Month);
        if (month is null) return Fail("ai.needMonth");
        var monthEnd = MonthEnd(month.Value);
        var count = await GenerateStudentCountAsync(session, request, ct);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("AAP_Auto_Generate_Monthly_Invoice", con)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 180
            };
            cmd.Parameters.AddWithValue("@TargetMonth", monthEnd);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            await cmd.ExecuteNonQueryAsync(ct);
            var extra = count.Succeeded ? (count.Message ?? "") : (count.Error ?? "");
            return Ok(string.IsNullOrWhiteSpace(extra) ? "ai.autoOk" : extra);
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<AuthorityResult> EnableJobAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(
                "EXEC msdb.dbo.sp_update_job @job_name = @JobName, @enabled = 1", con);
            cmd.Parameters.AddWithValue("@JobName", "Auto_Generate_Monthly_Invoice");
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ai.jobOn");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<AuthorityResult> CreateServiceInvoicesAsync(
        SessionSnapshot session, AuthCreateServiceRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthCreateServiceRequest();
        var month = ParseDate(request.Month);
        var issue = ParseDate(request.IssueDate);
        if (month is null) return Fail("ai.needMonth");
        if (issue is null) return Fail("ai.needIssue");
        var selected = request.Rows.Where(x => x.Selected).ToList();
        if (selected.Count == 0) return Fail("ai.needSelect");
        var monthEnd = MonthEnd(month.Value);
        var invoiceFor = monthEnd.ToString("MMM yyyy", CultureInfo.InvariantCulture);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        foreach (var row in selected)
        {
            var billable = row.StudentCount + row.CommitteeCount;
            var amount = row.Fixed > 0 ? row.Fixed : billable * row.PerStudent;
            object unitPrice = row.Fixed > 0 ? DBNull.Value : row.PerStudent;
            try
            {
                await using var cmd = new SqlCommand(
                    """
                    IF NOT EXISTS (
                        SELECT InvoiceID FROM AAP_Invoice
                        WHERE SchoolID = @SchoolID
                          AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'Service Charge')
                          AND EOMONTH(MonthName) = EOMONTH(@MonthName)
                          AND IsPaid = 0)
                    BEGIN
                        INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For,
                            TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice)
                        VALUES (@RegistrationID,
                            (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'Service Charge'),
                            @SchoolID, @IssuDate, @EndDate, @Invoice_For, @TotalAmount, @Discount, @MonthName,
                            dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice);
                        SELECT 1;
                    END
                    ELSE SELECT 0;
                    """, con);
                cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                cmd.Parameters.AddWithValue("@SchoolID", row.SchoolID);
                cmd.Parameters.AddWithValue("@IssuDate", issue.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", MonthPayDueDate(issue.Value.Date));
                cmd.Parameters.AddWithValue("@Invoice_For", invoiceFor);
                cmd.Parameters.AddWithValue("@TotalAmount", amount);
                cmd.Parameters.AddWithValue("@Discount", row.Discount);
                cmd.Parameters.AddWithValue("@MonthName", monthEnd);
                cmd.Parameters.AddWithValue("@Unit", billable);
                cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                var result = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0);
                if (result == 1) inserted++;
                else skipped++;
            }
            catch (Exception ex)
            {
                errors.Add($"{row.SchoolID}: {ex.Message}");
            }
        }

        var msg = $"Created: {inserted}";
        if (skipped > 0) msg += $"\nSkipped unpaid exists: {skipped}";
        if (errors.Count > 0) msg += "\n" + string.Join("\n", errors);
        return inserted == 0 && errors.Count > 0 ? Fail(msg) : Ok(msg);
    }

    public async Task<AuthorityResult> AddCategoryAsync(
        SessionSnapshot session, AuthAddCategoryRequest? request, CancellationToken ct)
    {
        Guard(session);
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0) return Fail("ai.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            """
            IF NOT EXISTS (SELECT 1 FROM AAP_Invoice_Category WHERE InvoiceCategory = @Name)
                INSERT INTO AAP_Invoice_Category (InvoiceCategory) VALUES (@Name)
            """, con);
        cmd.Parameters.AddWithValue("@Name", name);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ai.saved");
    }

    public async Task<AuthorityResult> CreateOtherInvoiceAsync(
        SessionSnapshot session, AuthCreateOtherRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthCreateOtherRequest();
        if (request.CategoryId <= 0 || request.SchoolID <= 0) return Fail("ai.needCatSchool");
        var issue = ParseDate(request.IssueDate);
        var end = ParseDate(request.EndDate);
        var month = ParseDate(request.MonthName);
        if (issue is null || end is null || month is null) return Fail("ai.needDates");
        if (string.IsNullOrWhiteSpace(request.InvoiceFor) || request.TotalAmount <= 0)
            return Fail("ai.needOther");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            """
            INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For,
                TotalAmount, Discount, Invoice_SN, Unit, UnitPrice, MonthName)
            VALUES (@RegistrationID, @InvoiceCategoryID, @SchoolID, @IssuDate, @EndDate, @Invoice_For,
                @TotalAmount, @Discount, dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice, @MonthName)
            """, con);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@InvoiceCategoryID", request.CategoryId);
        cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
        cmd.Parameters.AddWithValue("@IssuDate", issue.Value.Date);
        cmd.Parameters.AddWithValue("@EndDate", end.Value.Date);
        cmd.Parameters.AddWithValue("@Invoice_For", request.InvoiceFor.Trim());
        cmd.Parameters.AddWithValue("@TotalAmount", request.TotalAmount);
        cmd.Parameters.AddWithValue("@Discount", request.Discount);
        cmd.Parameters.AddWithValue("@Unit", request.Unit);
        cmd.Parameters.AddWithValue("@UnitPrice", request.UnitPrice);
        cmd.Parameters.AddWithValue("@MonthName", MonthEnd(month.Value));
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ai.otherOk");
    }

    public async Task<AuthorityResult> DeleteOtherInvoiceAsync(SessionSnapshot session, int invoiceId, CancellationToken ct)
    {
        Guard(session);
        if (invoiceId <= 0) return Fail("ai.needInvoice");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "DELETE FROM AAP_Invoice WHERE InvoiceID = @Id AND ISNULL(PaidAmount, 0) = 0", con);
        cmd.Parameters.AddWithValue("@Id", invoiceId);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0 ? Ok("ai.deleted") : Fail("ai.cannotDelete");
    }

    public async Task<AuthorityResult> SetGraceAsync(
        SessionSnapshot session, AuthGraceRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthGraceRequest();
        if (request.SchoolID <= 0) return Fail("ai.needSchool");
        var until = ParseDate(request.Until);
        if (until is null) return Fail("ai.needDate");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "UPDATE SchoolInfo SET AccessGraceUntil = @Until WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@Until", until.Value.Date);
        cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ai.graceOk");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<AuthorityResult> ClearGraceAsync(SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        Guard(session);
        if (schoolId <= 0) return Fail("ai.needSchool");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "UPDATE SchoolInfo SET AccessGraceUntil = NULL WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ai.graceOff");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<AuthPaidPageDto> GetPaidPageAsync(SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthPaidPageDto
        {
            SchoolID = schoolId,
            Schools = await LoadInvoiceSchoolsAsync(unpaidOnly: true, ct)
        };
        if (schoolId > 0)
        {
            dto.Invoices = await LoadUnpaidInvoicesAsync(schoolId, ct);
            foreach (var row in dto.Invoices)
                row.PayAmount = row.Due;
        }
        return dto;
    }

    public async Task<AuthorityResult> PayInvoicesAsync(
        SessionSnapshot session, AuthPayInvoiceRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthPayInvoiceRequest();
        if (request.SchoolID <= 0) return Fail("ai.needSchool");
        var paidDate = ParseDate(request.PaidDate) ?? DateTime.Today;
        var selected = request.Rows.Where(x => x.Selected && x.PayAmount > 0).ToList();
        if (selected.Count == 0) return Fail("ai.needPay");
        var total = selected.Sum(x => x.PayAmount);

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            int receiptId;
            await using (var rec = new SqlCommand(
                """
                INSERT INTO AAP_Invoice_Receipt (SchoolID, RegistrationID, TotalAmount, PaidDate, PaymentBy, Collected_By, Payment_Method, InvoiceReceipt_SN, PaidByUser)
                SELECT @SchoolID, @RegistrationID, @TotalAmount, @PaidDate, @PaymentBy, @Collected_By, @Payment_Method,
                       dbo.F_InvoiceReceipt_SN(), Registration.UserName
                FROM Registration WHERE RegistrationID = @RegistrationID;
                SELECT CAST(SCOPE_IDENTITY() AS int);
                """, con, tx))
            {
                rec.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                rec.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                rec.Parameters.AddWithValue("@TotalAmount", total);
                rec.Parameters.AddWithValue("@PaidDate", paidDate.Date);
                rec.Parameters.AddWithValue("@PaymentBy", request.PaymentBy ?? "");
                rec.Parameters.AddWithValue("@Collected_By", request.CollectedBy ?? "");
                rec.Parameters.AddWithValue("@Payment_Method", request.Method ?? "");
                receiptId = Convert.ToInt32(await rec.ExecuteScalarAsync(ct) ?? 0);
            }

            foreach (var row in selected)
            {
                await using (var upd = new SqlCommand(
                    """
                    UPDATE AAP_Invoice SET
                        NumberOfPayment = ISNULL(NumberOfPayment, 0) + 1,
                        LastPaidDate = GETDATE(),
                        Discount = CASE WHEN @Discount > TotalAmount THEN TotalAmount ELSE @Discount END,
                        PaidAmount = CASE
                            WHEN PaidAmount + @PaidAmount > TotalAmount - CASE WHEN @Discount > TotalAmount THEN TotalAmount ELSE @Discount END
                            THEN TotalAmount - CASE WHEN @Discount > TotalAmount THEN TotalAmount ELSE @Discount END
                            ELSE PaidAmount + @PaidAmount
                        END
                    WHERE InvoiceID = @InvoiceID
                    """, con, tx))
                {
                    upd.Parameters.AddWithValue("@PaidAmount", row.PayAmount);
                    upd.Parameters.AddWithValue("@Discount", row.Discount);
                    upd.Parameters.AddWithValue("@InvoiceID", row.InvoiceID);
                    await upd.ExecuteNonQueryAsync(ct);
                }

                await using (var pay = new SqlCommand(
                    """
                    INSERT INTO AAP_Invoice_Payment_Record (InvoiceReceiptID, InvoiceID, RegistrationID, SchoolID, Amount, PaidDate)
                    VALUES (@ReceiptID, @InvoiceID, @RegistrationID, @SchoolID, @Amount, @PaidDate)
                    """, con, tx))
                {
                    pay.Parameters.AddWithValue("@ReceiptID", receiptId);
                    pay.Parameters.AddWithValue("@InvoiceID", row.InvoiceID);
                    pay.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                    pay.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                    pay.Parameters.AddWithValue("@Amount", row.PayAmount);
                    pay.Parameters.AddWithValue("@PaidDate", paidDate.Date);
                    await pay.ExecuteNonQueryAsync(ct);
                }

                await MarkSmsPaidAsync(con, tx, row.InvoiceID, ct);
                await RecordCommissionAsync(con, tx, row.InvoiceID, ct);
            }

            await ClearGraceIfNoDueAsync(con, tx, request.SchoolID, ct);

            await tx.CommitAsync(ct);
            return Ok("ai.paidOk");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    public async Task<AuthPrintPageDto> GetPrintPageAsync(SessionSnapshot session, int schoolId, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthPrintPageDto
        {
            SchoolID = schoolId,
            Schools = await LoadInvoiceSchoolsAsync(unpaidOnly: false, ct)
        };
        if (schoolId <= 0) return dto;
        dto.Unpaid = await LoadUnpaidInvoicesAsync(schoolId, ct);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            """
            SELECT InvoiceReceiptID, InvoiceReceipt_SN, TotalAmount, PaidDate, PaymentBy, PaidByUser,
                   Collected_By, Payment_Method, SchoolID
            FROM AAP_Invoice_Receipt WHERE SchoolID = @SchoolID
            ORDER BY InvoiceReceiptID DESC
            """, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            dto.Receipts.Add(new AuthReceiptRowDto
            {
                ReceiptId = I(reader["InvoiceReceiptID"]),
                SchoolID = I(reader["SchoolID"]),
                ReceiptSn = S(reader["InvoiceReceipt_SN"]),
                TotalAmount = M(reader["TotalAmount"]),
                PaymentBy = S(reader["PaymentBy"]),
                PaidByUser = S(reader["PaidByUser"]),
                CollectedBy = S(reader["Collected_By"]),
                Method = S(reader["Payment_Method"]),
                PaidDate = Dt(reader["PaidDate"])
            });
        }
        return dto;
    }

    public async Task<AuthorityResult> DeleteUnpaidInvoiceAsync(SessionSnapshot session, int invoiceId, CancellationToken ct)
        => await DeleteOtherInvoiceAsync(session, invoiceId, ct);

    public async Task<AuthPayPrintDto> GetPayPrintAsync(SessionSnapshot session, int schoolId, string? ids, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthPayPrintDto();
        var idList = ParseIds(ids);
        if (schoolId <= 0 || idList.Count == 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var inSql = BuildIn(idList, out var pars);

        await using (var head = new SqlCommand(
            $"""
            SELECT SUM(i.TotalAmount - i.PaidAmount) AS GrandTotal, SUM(i.Discount) AS Discount,
                   SUM(i.Due) AS Due, s.SchoolName, s.Address, s.Phone
            FROM AAP_Invoice i
            INNER JOIN SchoolInfo s ON i.SchoolID = s.SchoolID
            WHERE i.SchoolID = @SchoolID AND i.IsPaid = 0 AND i.InvoiceID IN ({inSql})
            GROUP BY s.SchoolName, s.Address, s.Phone
            """, con))
        {
            head.Parameters.AddWithValue("@SchoolID", schoolId);
            foreach (var p in pars) head.Parameters.Add(p);
            await using var reader = await head.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Found = true;
                dto.SchoolName = S(reader["SchoolName"]);
                dto.Address = S(reader["Address"]);
                dto.Phone = S(reader["Phone"]);
                dto.GrandTotal = M(reader["GrandTotal"]);
                dto.Discount = M(reader["Discount"]);
                dto.Due = M(reader["Due"]);
            }
        }

        await using var lines = new SqlCommand(
            $"""
            SELECT i.Invoice_For, i.Unit, i.UnitPrice, i.TotalAmount, c.InvoiceCategory,
                   i.TotalAmount - i.PaidAmount AS Due
            FROM AAP_Invoice i
            INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
            WHERE i.SchoolID = @SchoolID AND i.IsPaid = 0 AND i.InvoiceID IN ({inSql})
            """, con);
        lines.Parameters.AddWithValue("@SchoolID", schoolId);
        foreach (var p in pars)
            lines.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
        await using var r2 = await lines.ExecuteReaderAsync(ct);
        while (await r2.ReadAsync(ct))
        {
            dto.Lines.Add(new AuthPayPrintLineDto
            {
                Category = S(r2["InvoiceCategory"]),
                InvoiceFor = S(r2["Invoice_For"]),
                Unit = M(r2["Unit"]),
                UnitPrice = M(r2["UnitPrice"]),
                TotalAmount = M(r2["TotalAmount"]),
                Due = M(r2["Due"])
            });
        }
        dto.Found |= dto.Lines.Count > 0;
        return dto;
    }

    public async Task<AuthReceiptPrintDto> GetReceiptPrintAsync(SessionSnapshot session, int receiptId, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthReceiptPrintDto();
        if (receiptId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var head = new SqlCommand(
            """
            SELECT r.InvoiceReceipt_SN, s.SchoolName, s.Address, s.Phone, r.TotalAmount AS Total_Paid,
                   r.PaidDate, r.PaymentBy, r.Collected_By, r.Payment_Method
            FROM AAP_Invoice_Receipt r
            INNER JOIN SchoolInfo s ON r.SchoolID = s.SchoolID
            WHERE r.InvoiceReceiptID = @Id
            """, con))
        {
            head.Parameters.AddWithValue("@Id", receiptId);
            await using var reader = await head.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return dto;
            dto.Found = true;
            dto.ReceiptSn = S(reader["InvoiceReceipt_SN"]);
            dto.SchoolName = S(reader["SchoolName"]);
            dto.Address = S(reader["Address"]);
            dto.Phone = S(reader["Phone"]);
            dto.TotalPaid = M(reader["Total_Paid"]);
            dto.PaidDate = Dt(reader["PaidDate"]);
            dto.PaymentBy = S(reader["PaymentBy"]);
            dto.CollectedBy = S(reader["Collected_By"]);
            dto.Method = S(reader["Payment_Method"]);
        }

        await using var lines = new SqlCommand(
            """
            SELECT i.Invoice_For, i.Unit, i.UnitPrice, i.TotalAmount, c.InvoiceCategory, p.Amount AS Paid,
                   i.Discount, i.Due
            FROM AAP_Invoice i
            INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
            INNER JOIN AAP_Invoice_Payment_Record p ON i.InvoiceID = p.InvoiceID
            WHERE p.InvoiceReceiptID = @Id
            """, con);
        lines.Parameters.AddWithValue("@Id", receiptId);
        await using var r2 = await lines.ExecuteReaderAsync(ct);
        while (await r2.ReadAsync(ct))
        {
            dto.Lines.Add(new AuthReceiptPrintLineDto
            {
                Category = S(r2["InvoiceCategory"]),
                InvoiceFor = S(r2["Invoice_For"]),
                Unit = M(r2["Unit"]),
                UnitPrice = M(r2["UnitPrice"]),
                TotalAmount = M(r2["TotalAmount"]),
                Paid = M(r2["Paid"])
            });
            dto.TotalAmount += M(r2["TotalAmount"]);
            dto.TotalDiscount += M(r2["Discount"]);
            dto.TotalDue += M(r2["Due"]);
        }
        return dto;
    }

    private async Task<List<AuthorityOptionDto>> LoadCountMonthsAsync(CancellationToken ct)
    {
        var list = new List<AuthorityOptionDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(
                """
                SELECT DISTINCT EOMONTH(Month) AS Date_N, FORMAT(EOMONTH(Month), 'MMM yyyy') AS Month
                FROM AAP_Student_Count_Monthly
                ORDER BY EOMONTH(Month) DESC
                """, con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var i = 0;
            while (await reader.ReadAsync(ct))
            {
                var date = Dt(reader["Date_N"]);
                list.Add(new AuthorityOptionDto
                {
                    Id = ++i,
                    Name = S(reader["Month"]),
                    Extra = date?.ToString("yyyy-MM-dd") ?? ""
                });
            }
        }
        catch
        {
        }
        return list;
    }

    private async Task<List<AuthServiceChargeRowDto>> LoadServiceRowsAsync(string monthIso, CancellationToken ct)
    {
        var list = new List<AuthServiceChargeRowDto>();
        if (string.IsNullOrWhiteSpace(monthIso)) return list;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var sqlWithFn = """
            SELECT s.SchoolName, s.Per_Student_Rate, s.IS_ServiceChargeActive, s.Discount, s.Fixed,
                   c.SchoolID, ISNULL(c.StudentCount, c.Active_Student) AS StudentCount,
                   c.Active_Student, c.Reject_Countable, c.Reject_Uncountable,
                   dbo.fn_GetBillableCommitteeCount(s.SchoolID) AS CommitteeCount
            FROM SchoolInfo s
            INNER JOIN AAP_Student_Count_Monthly c ON s.SchoolID = c.SchoolID
            WHERE EOMONTH(c.Month) = CAST(@Month AS date)
            ORDER BY s.SchoolName
            """;
        var sqlPlain = """
            SELECT s.SchoolName, s.Per_Student_Rate, s.IS_ServiceChargeActive, s.Discount, s.Fixed,
                   c.SchoolID, ISNULL(c.StudentCount, c.Active_Student) AS StudentCount,
                   c.Active_Student, c.Reject_Countable, c.Reject_Uncountable, 0 AS CommitteeCount
            FROM SchoolInfo s
            INNER JOIN AAP_Student_Count_Monthly c ON s.SchoolID = c.SchoolID
            WHERE EOMONTH(c.Month) = CAST(@Month AS date)
            ORDER BY s.SchoolName
            """;
        try
        {
            await ReadServiceRowsAsync(con, sqlWithFn, monthIso, list, ct);
        }
        catch
        {
            list.Clear();
            try { await ReadServiceRowsAsync(con, sqlPlain, monthIso, list, ct); } catch { }
        }
        return list;
    }

    private static async Task ReadServiceRowsAsync(
        SqlConnection con, string sql, string monthIso, List<AuthServiceChargeRowDto> list, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Month", monthIso);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var students = M(reader["StudentCount"]);
            var committee = M(reader["CommitteeCount"]);
            list.Add(new AuthServiceChargeRowDto
            {
                SchoolID = I(reader["SchoolID"]),
                SchoolName = S(reader["SchoolName"]),
                StudentCount = students,
                CommitteeCount = committee,
                Billable = students + committee,
                PerStudent = M(reader["Per_Student_Rate"]),
                RejectCountable = I(reader["Reject_Countable"]),
                RejectUncountable = I(reader["Reject_Uncountable"]),
                PaymentActive = B(reader["IS_ServiceChargeActive"]),
                ActiveStudent = I(reader["Active_Student"]),
                Discount = M(reader["Discount"]),
                Fixed = M(reader["Fixed"]),
                Selected = true
            });
        }
    }

    private async Task<List<AuthSmsInvoiceRowDto>> LoadSmsRowsAsync(
        string? from, string? to, string? q, CancellationToken ct)
    {
        var list = new List<AuthSmsInvoiceRowDto>();
        var fromDate = ParseDate(from) ?? new DateTime(1000, 1, 1);
        var toDate = ParseDate(to) ?? new DateTime(3000, 1, 1);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(
                """
                SELECT r.SMS_Recharge_RecordID, r.SchoolID, s.SchoolName, r.RechargeSMS, r.PerSMS_Price,
                       r.Total_Price, r.Date, g.UserName
                FROM SMS_Recharge_Record r
                INNER JOIN SchoolInfo s ON r.SchoolID = s.SchoolID
                LEFT JOIN Registration g ON r.RegistrationID = g.RegistrationID
                WHERE r.Total_Price > 0 AND r.Is_Paid = 0
                  AND r.Date BETWEEN @From AND @To
                  AND (@Q = '' OR s.SchoolName LIKE '%' + @Q + '%')
                ORDER BY r.Date DESC
                """, con);
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate.Date.AddDays(1).AddSeconds(-1));
            cmd.Parameters.AddWithValue("@Q", q?.Trim() ?? "");
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new AuthSmsInvoiceRowDto
                {
                    Id = I(reader["SMS_Recharge_RecordID"]),
                    SchoolID = I(reader["SchoolID"]),
                    SchoolName = S(reader["SchoolName"]),
                    RechargeSms = M(reader["RechargeSMS"]),
                    PerSms = M(reader["PerSMS_Price"]),
                    Total = M(reader["Total_Price"]),
                    Date = Dt(reader["Date"]),
                    UserName = S(reader["UserName"])
                });
            }
        }
        catch
        {
        }
        return list;
    }

    private async Task<List<AuthInvoiceLineDto>> LoadSchoolInvoicesAsync(int schoolId, CancellationToken ct)
    {
        var list = new List<AuthInvoiceLineDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            """
            SELECT i.InvoiceID, i.SchoolID, s.SchoolName, i.Invoice_SN, c.InvoiceCategory, i.Invoice_For,
                   i.MonthName, i.IssuDate, i.EndDate, i.Unit, i.UnitPrice, i.TotalAmount, i.Discount,
                   i.PaidAmount, i.Due
            FROM AAP_Invoice i
            INNER JOIN SchoolInfo s ON i.SchoolID = s.SchoolID
            INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
            WHERE i.SchoolID = @SchoolID
            ORDER BY i.InvoiceID DESC
            """, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(ReadInvoiceLine(reader));
        return list;
    }

    private async Task<List<AuthGraceRowDto>> LoadGraceRowsAsync(CancellationToken ct)
    {
        var list = new List<AuthGraceRowDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(
                "SELECT SchoolID, SchoolName, AccessGraceUntil FROM SchoolInfo WHERE AccessGraceUntil IS NOT NULL ORDER BY AccessGraceUntil DESC", con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var until = Dt(reader["AccessGraceUntil"]);
                list.Add(new AuthGraceRowDto
                {
                    SchoolID = I(reader["SchoolID"]),
                    SchoolName = S(reader["SchoolName"]),
                    Until = until,
                    Active = until is { } d && d.Date >= DateTime.Today
                });
            }
        }
        catch
        {
        }
        return list;
    }

    private async Task<AuthJobStatusDto> LoadJobStatusAsync(CancellationToken ct)
    {
        var dto = new AuthJobStatusDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(
                """
                SELECT j.name AS JobName, j.enabled AS Enabled,
                    CASE WHEN h.run_date IS NULL THEN NULL
                         ELSE CONVERT(datetime, STUFF(STUFF(CAST(h.run_date AS VARCHAR(8)),7,0,'-'),5,0,'-')
                              + ' ' + STUFF(STUFF(RIGHT('000000'+CAST(h.run_time AS VARCHAR(6)),6),5,0,':'),3,0,':'))
                    END AS LastRunDateTime,
                    h.run_status AS LastRunStatus,
                    CASE WHEN s.next_run_date = 0 THEN NULL
                         ELSE CONVERT(datetime, STUFF(STUFF(CAST(s.next_run_date AS VARCHAR(8)),7,0,'-'),5,0,'-')
                              + ' ' + STUFF(STUFF(RIGHT('000000'+CAST(s.next_run_time AS VARCHAR(6)),6),5,0,':'),3,0,':'))
                    END AS NextRunDateTime
                FROM msdb.dbo.sysjobs j
                LEFT JOIN (
                    SELECT job_id, run_date, run_time, run_status
                    FROM msdb.dbo.sysjobhistory
                    WHERE step_id = 0 AND instance_id = (
                        SELECT MAX(instance_id) FROM msdb.dbo.sysjobhistory h2
                        WHERE h2.job_id = (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = @JobName) AND h2.step_id = 0)
                ) h ON j.job_id = h.job_id
                LEFT JOIN (
                    SELECT job_id, next_run_date, next_run_time
                    FROM msdb.dbo.sysjobschedules
                    WHERE schedule_id = (
                        SELECT TOP 1 schedule_id FROM msdb.dbo.sysjobschedules js2
                        WHERE js2.job_id = (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = @JobName))
                ) s ON j.job_id = s.job_id
                WHERE j.name = @JobName
                """, con);
            cmd.Parameters.AddWithValue("@JobName", dto.Name);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Found = true;
                dto.Name = S(reader["JobName"]);
                dto.Enabled = B(reader["Enabled"]);
                dto.LastRun = Dt(reader["LastRunDateTime"])?.ToString("d MMM yyyy hh:mm tt") ?? "";
                dto.NextRun = Dt(reader["NextRunDateTime"])?.ToString("d MMM yyyy hh:mm tt") ?? "";
                dto.LastStatus = I(reader["LastRunStatus"]) switch
                {
                    1 => "Succeeded",
                    0 => "Failed",
                    3 => "Cancelled",
                    2 => "Retry",
                    _ => ""
                };
            }
        }
        catch (Exception ex)
        {
            dto.Error = ex.Message;
        }
        return dto;
    }

    private async Task<List<AuthorityOptionDto>> LoadInvoiceSchoolsAsync(bool unpaidOnly, CancellationToken ct)
    {
        var list = new List<AuthorityOptionDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var sql = unpaidOnly
            ? """
              SELECT DISTINCT s.SchoolID, CAST(s.SchoolID AS varchar(10)) + ' - ' + s.SchoolName AS Name
              FROM SchoolInfo s INNER JOIN AAP_Invoice i ON s.SchoolID = i.SchoolID
              WHERE s.Validation = N'Valid' AND i.IsPaid = 0
              ORDER BY Name
              """
            : """
              SELECT DISTINCT s.SchoolID, CAST(s.SchoolID AS varchar(10)) + ' - ' + s.SchoolName AS Name
              FROM SchoolInfo s INNER JOIN AAP_Invoice i ON s.SchoolID = i.SchoolID
              WHERE s.Validation = N'Valid'
              ORDER BY Name
              """;
        await using var cmd = new SqlCommand(sql, con);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(new AuthorityOptionDto { Id = I(reader["SchoolID"]), Name = S(reader["Name"]) });
        return list;
    }

    private async Task<List<AuthInvoiceLineDto>> LoadUnpaidInvoicesAsync(int schoolId, CancellationToken ct)
    {
        var list = new List<AuthInvoiceLineDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            """
            SELECT i.InvoiceID, i.SchoolID, s.SchoolName, i.Invoice_SN, c.InvoiceCategory, i.Invoice_For,
                   i.MonthName, i.IssuDate, i.EndDate, i.Unit, i.UnitPrice, i.TotalAmount, i.Discount,
                   i.PaidAmount, i.Due
            FROM AAP_Invoice i
            INNER JOIN SchoolInfo s ON i.SchoolID = s.SchoolID
            INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
            WHERE i.SchoolID = @SchoolID AND i.IsPaid = 0
            ORDER BY i.InvoiceID
            """, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(ReadInvoiceLine(reader));
        return list;
    }

    private static async Task MarkSmsPaidAsync(SqlConnection con, SqlTransaction tx, int invoiceId, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(
                """
                UPDATE SMS_Recharge_Record
                SET Is_Paid = 1
                WHERE Is_Paid = 0 AND SchoolID = (
                    SELECT i.SchoolID FROM AAP_Invoice i
                    INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
                    WHERE i.InvoiceID = @InvoiceID AND c.InvoiceCategory = N'SMS')
                  AND CONVERT(date, Date) = (
                    SELECT CONVERT(date, IssuDate) FROM AAP_Invoice WHERE InvoiceID = @InvoiceID)
                """, con, tx);
            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
        }
    }

    private static async Task RecordCommissionAsync(SqlConnection con, SqlTransaction tx, int invoiceId, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(
                """
                IF NOT EXISTS (SELECT 1 FROM AAP_Reference_Commission WHERE InvoiceID = @InvoiceID)
                BEGIN
                    INSERT INTO AAP_Reference_Commission
                        (ReferenceID, Reference_School_ID, InvoiceID, SchoolID,
                         Commission_Amount, Commission_Percentage, ServiceCharge_Amount, Commission_Date)
                    SELECT rs.ReferenceID, rs.Reference_School_ID, i.InvoiceID, i.SchoolID,
                           CAST(i.TotalAmount * rs.Percentage / 100.0 AS DECIMAL(18,2)),
                           rs.Percentage, i.TotalAmount, GETDATE()
                    FROM AAP_Invoice i
                    INNER JOIN AAP_Reference_School rs ON rs.SchoolID = i.SchoolID
                        AND (rs.End_Reference_Date IS NULL OR GETDATE() <= rs.End_Reference_Date)
                    INNER JOIN AAP_Invoice_Category cat ON i.InvoiceCategoryID = cat.InvoiceCategoryID
                        AND cat.InvoiceCategory = N'Service Charge'
                    WHERE i.InvoiceID = @InvoiceID
                END
                """, con, tx);
            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
        }
    }

    private static async Task ClearGraceIfNoDueAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(
            """
            IF NOT EXISTS (
                SELECT 1 FROM dbo.AAP_Invoice
                WHERE SchoolID = @SID AND IsPaid = 0
            )
                UPDATE dbo.SchoolInfo SET AccessGraceUntil = NULL
                WHERE SchoolID = @SID AND AccessGraceUntil IS NOT NULL
            """, con, tx);
        cmd.Parameters.AddWithValue("@SID", schoolId);
        try { await cmd.ExecuteNonQueryAsync(ct); } catch { }
    }

    private static DateTime MonthPayDueDate(DateTime issue)
    {
        var due = new DateTime(issue.Year, issue.Month, 15);
        return issue.Day <= 15 ? due : due.AddMonths(1);
    }

    private static List<int> ParseIds(string? ids)
    {
        var list = new List<int>();
        if (string.IsNullOrWhiteSpace(ids)) return list;
        foreach (var part in ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (int.TryParse(part, out var id) && id > 0)
                list.Add(id);
        return list;
    }

    private static string BuildIn(List<int> ids, out List<SqlParameter> pars)
    {
        pars = [];
        var names = new List<string>();
        for (var i = 0; i < ids.Count; i++)
        {
            var name = "@id" + i;
            names.Add(name);
            pars.Add(new SqlParameter(name, ids[i]));
        }
        return names.Count == 0 ? "NULL" : string.Join(",", names);
    }
}
