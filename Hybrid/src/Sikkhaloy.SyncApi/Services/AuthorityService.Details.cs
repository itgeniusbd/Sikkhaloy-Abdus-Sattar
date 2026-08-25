using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityService
{
    public async Task<AuthorityResult> RechargeInstitutionSmsAsync(
        SessionSnapshot session, InstSmsRechargeRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority) return Fail("auth.forbidden");
        request ??= new InstSmsRechargeRequest();
        if (request.SchoolID <= 0) return Fail("auth.noSchool");
        if (request.Quantity <= 0 || request.PerSms < 0) return Fail("id.needSms");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await using (var ins = new SqlCommand("""
INSERT INTO SMS_Recharge_Record(SchoolID, RechargeSMS, PerSMS_Price, Date, Is_Paid, RegistrationID)
VALUES (@SchoolID, @Qty, @Price, GETDATE(), 0, @RegistrationID);
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con, tx))
            {
                ins.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                ins.Parameters.AddWithValue("@Qty", request.Quantity);
                ins.Parameters.AddWithValue("@Price", request.PerSms);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                _ = await ins.ExecuteScalarAsync(ct);
            }

            var total = request.Quantity * request.PerSms;
            var catId = 0;
            await using (var cat = new SqlCommand(
                "SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'SMS'", con, tx))
            {
                var v = await cat.ExecuteScalarAsync(ct);
                if (v is not null and not DBNull)
                    catId = Convert.ToInt32(v);
            }

            if (catId > 0 && total > 0)
            {
                var now = DateTime.Now;
                await using var inv = new SqlCommand("""
INSERT INTO AAP_Invoice(RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate,
    Invoice_For, TotalAmount, MonthName, Invoice_SN, Unit, UnitPrice)
VALUES (@RegistrationID, @InvoiceCategoryID, @SchoolID, @IssuDate, @EndDate,
    @Invoice_For, @TotalAmount, @MonthName, dbo.Invoice_SerialNumber(@SchoolID), @Unit, @UnitPrice)
""", con, tx);
                inv.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                inv.Parameters.AddWithValue("@InvoiceCategoryID", catId);
                inv.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                inv.Parameters.AddWithValue("@IssuDate", now.Date);
                inv.Parameters.AddWithValue("@EndDate", now.Date.AddDays(15));
                inv.Parameters.AddWithValue("@Invoice_For", "Recharged: " + now.ToString("d MMM yyyy"));
                inv.Parameters.AddWithValue("@TotalAmount", total);
                inv.Parameters.AddWithValue("@MonthName", now.Date);
                inv.Parameters.AddWithValue("@Unit", request.Quantity);
                inv.Parameters.AddWithValue("@UnitPrice", request.PerSms);
                await inv.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return Ok("id.smsOk");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    public async Task<AuthorityResult> SaveDueNoticeAsync(
        SessionSnapshot session, InstDueNoticeRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority) return Fail("auth.forbidden");
        request ??= new InstDueNoticeRequest();
        if (request.SchoolID <= 0) return Fail("auth.noSchool");

        DateTime? hide = null;
        if (!string.IsNullOrWhiteSpace(request.HideUntil) && DateTime.TryParse(request.HideUntil, out var parsed))
            hide = parsed.Date;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using (var off = new SqlCommand(
                "UPDATE SchoolInfo_DueNoticeSettings SET IsEnabled = 0 WHERE SchoolID = @SchoolID", con))
            {
                off.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                await off.ExecuteNonQueryAsync(ct);
            }

            if (!request.Enabled)
                return Ok("id.dueOff");

            await using var ins = new SqlCommand("""
INSERT INTO SchoolInfo_DueNoticeSettings (SchoolID, IsEnabled, HideUntilDate, Reason, CreatedDate, CreatedBy)
VALUES (@SchoolID, 1, @HideUntilDate, @Reason, GETDATE(), @CreatedBy)
""", con);
            ins.Parameters.AddWithValue("@SchoolID", request.SchoolID);
            ins.Parameters.AddWithValue("@HideUntilDate", hide is { } d ? d : DBNull.Value);
            ins.Parameters.AddWithValue("@Reason", string.IsNullOrWhiteSpace(request.Reason) ? DBNull.Value : request.Reason.Trim());
            ins.Parameters.AddWithValue("@CreatedBy", session.RegistrationID);
            await ins.ExecuteNonQueryAsync(ct);
            return Ok("id.dueOn");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<InstStudentFindDto> FindStudentAsync(
        SessionSnapshot session, int schoolId, string? id, CancellationToken ct)
    {
        var dto = new InstStudentFindDto();
        if (!session.IsAuthority || schoolId <= 0 || string.IsNullOrWhiteSpace(id))
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT TOP 1 s.StudentID, s.ID, s.StudentsName, s.Status, ISNULL(cc.Class, '') AS Class, ISNULL(sc.RollNo, '') AS RollNo
FROM Student s
LEFT JOIN StudentsClass sc ON sc.StudentID = s.StudentID AND sc.SchoolID = s.SchoolID
LEFT JOIN CreateClass cc ON cc.ClassID = sc.ClassID
WHERE s.SchoolID = @SchoolID AND s.ID = @ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ID", id.Trim());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return dto;
        dto.Found = true;
        dto.StudentID = I(reader["StudentID"]);
        dto.Id = S(reader["ID"]);
        dto.StudentsName = S(reader["StudentsName"]);
        dto.Class = S(reader["Class"]);
        dto.RollNo = S(reader["RollNo"]);
        dto.Status = S(reader["Status"]);
        return dto;
    }

    public async Task<AuthorityResult> DeleteStudentIdAsync(
        SessionSnapshot session, InstIdRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority) return Fail("auth.forbidden");
        request ??= new InstIdRequest();
        if (request.SchoolID <= 0) return Fail("auth.noSchool");
        var id = (request.Id ?? "").Trim();
        if (id.Length == 0) return Fail("id.needId");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
IF NOT EXISTS (
    SELECT StudentsClass.StudentID
    FROM StudentsClass
    INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
    WHERE Student.SchoolID = @SchoolID AND Student.ID = @ID)
BEGIN
    DELETE FROM Student_Image
    FROM Student_Image INNER JOIN Student ON Student_Image.StudentImageID = Student.StudentImageID
    WHERE Student.SchoolID = @SchoolID AND Student.ID = @ID;
    DELETE FROM Student WHERE SchoolID = @SchoolID AND ID = @ID;
    SELECT CAST(@@ROWCOUNT AS int);
END
ELSE
    SELECT CAST(-1 AS int);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
        cmd.Parameters.AddWithValue("@ID", id);
        var n = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0);
        if (n < 0) return Fail("id.stillClass");
        return n > 0 ? Ok("id.idDeleted") : Fail("id.noStudent");
    }

    public async Task<AuthorityResult> ChangeStudentIdAsync(
        SessionSnapshot session, InstChangeIdRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority) return Fail("auth.forbidden");
        request ??= new InstChangeIdRequest();
        if (request.SchoolID <= 0) return Fail("auth.noSchool");
        var oldId = (request.OldId ?? "").Trim();
        var newId = (request.NewId ?? "").Trim();
        if (oldId.Length == 0 || newId.Length == 0) return Fail("id.needId");
        if (oldId.Equals(newId, StringComparison.OrdinalIgnoreCase)) return Fail("id.sameId");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
IF NOT EXISTS (
    SELECT ID FROM Employee_Info WHERE SchoolID = @SchoolID AND ID = @ID
    UNION
    SELECT ID FROM Student WHERE SchoolID = @SchoolID AND ID = @ID)
BEGIN
    UPDATE Student SET ID = @ID WHERE SchoolID = @SchoolID AND ID = @OldId;
    SELECT CAST(@@ROWCOUNT AS int);
END
ELSE
    SELECT CAST(-1 AS int);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
        cmd.Parameters.AddWithValue("@OldId", oldId);
        cmd.Parameters.AddWithValue("@ID", newId);
        var n = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0);
        if (n < 0) return Fail("id.idTaken");
        if (n <= 0) return Fail("id.noStudent");

        try
        {
            await using var dev = new SqlCommand("""
IF NOT EXISTS (SELECT DateUpdateID FROM Attendance_Device_DataUpdateList WHERE SchoolID = @SchoolID AND UpdateType = @UpdateType)
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription)
VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
""", con);
            dev.Parameters.AddWithValue("@SchoolID", request.SchoolID);
            dev.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            dev.Parameters.AddWithValue("@UpdateType", "Student ID Change");
            dev.Parameters.AddWithValue("@UpdateDescription", "Student ID Change by authority");
            await dev.ExecuteNonQueryAsync(ct);
        }
        catch
        {
        }

        return Ok("id.idChanged");
    }

    public async Task<InstReceiptDto> FindReceiptAsync(
        SessionSnapshot session, int schoolId, string? sn, CancellationToken ct)
    {
        var dto = new InstReceiptDto { ReceiptSn = sn ?? "" };
        if (!session.IsAuthority || schoolId <= 0 || string.IsNullOrWhiteSpace(sn))
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var info = new SqlCommand("""
SELECT TOP 1 s.ID, s.StudentsName, cc.Class, r.MoneyReceipt_SN, r.TotalAmount, r.PaidDate
FROM Income_MoneyReceipt r
INNER JOIN StudentsClass sc ON sc.StudentClassID = r.StudentClassID
INNER JOIN Student s ON s.StudentID = sc.StudentID
INNER JOIN CreateClass cc ON cc.ClassID = sc.ClassID
WHERE s.SchoolID = @SchoolID AND r.MoneyReceipt_SN = @SN
""", con))
        {
            info.Parameters.AddWithValue("@SchoolID", schoolId);
            info.Parameters.AddWithValue("@SN", sn.Trim());
            await using var reader = await info.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return dto;
            dto.Found = true;
            dto.StudentId = S(reader["ID"]);
            dto.StudentsName = S(reader["StudentsName"]);
            dto.Class = S(reader["Class"]);
            dto.ReceiptSn = S(reader["MoneyReceipt_SN"]);
            dto.TotalAmount = M(reader["TotalAmount"]);
            dto.PaidDate = Dt(reader["PaidDate"]);
        }

        await using var lines = new SqlCommand("""
SELECT ir.Role, pr.PayFor, pr.PaidAmount, po.Receivable_Amount AS Due, po.Amount
FROM Income_PaymentRecord pr
INNER JOIN Income_Roles ir ON pr.RoleID = ir.RoleID
INNER JOIN Income_PayOrder po ON pr.PayOrderID = po.PayOrderID
INNER JOIN Income_MoneyReceipt r ON pr.MoneyReceiptID = r.MoneyReceiptID
WHERE pr.SchoolID = @SchoolID AND r.MoneyReceipt_SN = @SN
""", con);
        lines.Parameters.AddWithValue("@SchoolID", schoolId);
        lines.Parameters.AddWithValue("@SN", sn.Trim());
        await using var lineReader = await lines.ExecuteReaderAsync(ct);
        while (await lineReader.ReadAsync(ct))
        {
            dto.Lines.Add(new InstReceiptLineDto
            {
                Role = S(lineReader["Role"]),
                PayFor = S(lineReader["PayFor"]),
                Amount = M(lineReader["Amount"]),
                PaidAmount = M(lineReader["PaidAmount"]),
                Due = M(lineReader["Due"])
            });
        }
        return dto;
    }

    public async Task<AuthorityResult> DeleteReceiptAsync(
        SessionSnapshot session, InstReceiptRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority) return Fail("auth.forbidden");
        request ??= new InstReceiptRequest();
        if (request.SchoolID <= 0) return Fail("auth.noSchool");
        var sn = (request.ReceiptSn ?? "").Trim();
        if (sn.Length == 0) return Fail("id.needReceipt");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await using (var upd = new SqlCommand("""
UPDATE Income_PayOrder
SET PaidAmount = Income_PayOrder.PaidAmount - Income_PaymentRecord.PaidAmount,
    NumberOfPayment = 0, LastPaidDate = NULL
FROM Income_MoneyReceipt
INNER JOIN Income_PaymentRecord ON Income_MoneyReceipt.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID
INNER JOIN Income_PayOrder ON Income_PaymentRecord.PayOrderID = Income_PayOrder.PayOrderID
WHERE Income_MoneyReceipt.SchoolID = @SchoolID AND Income_MoneyReceipt.MoneyReceipt_SN = @SN
""", con, tx))
            {
                upd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                upd.Parameters.AddWithValue("@SN", sn);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await using (var pay = new SqlCommand("""
DELETE FROM Income_PaymentRecord
FROM Income_MoneyReceipt
INNER JOIN Income_PaymentRecord ON Income_MoneyReceipt.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID
WHERE Income_MoneyReceipt.SchoolID = @SchoolID AND Income_MoneyReceipt.MoneyReceipt_SN = @SN
""", con, tx))
            {
                pay.Parameters.AddWithValue("@SchoolID", request.SchoolID);
                pay.Parameters.AddWithValue("@SN", sn);
                await pay.ExecuteNonQueryAsync(ct);
            }

            await using var rec = new SqlCommand("""
DELETE FROM Income_MoneyReceipt WHERE SchoolID = @SchoolID AND MoneyReceipt_SN = @SN
""", con, tx);
            rec.Parameters.AddWithValue("@SchoolID", request.SchoolID);
            rec.Parameters.AddWithValue("@SN", sn);
            var n = await rec.ExecuteNonQueryAsync(ct);
            await tx.CommitAsync(ct);
            return n > 0 ? Ok("id.receiptDeleted") : Fail("id.noReceipt");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    private static async Task LoadSmsAsync(SqlConnection con, InstitutionDetailsDto dto, CancellationToken ct)
    {
        try
        {
            await using var bal = new SqlCommand("SELECT TOP 1 SMS_Balance FROM SMS WHERE SchoolID = @SchoolID", con);
            bal.Parameters.AddWithValue("@SchoolID", dto.SchoolID);
            var v = await bal.ExecuteScalarAsync(ct);
            dto.SmsBalance = v is null or DBNull ? 0 : Convert.ToDecimal(v);
        }
        catch
        {
        }

        try
        {
            await using var hist = new SqlCommand("""
SELECT r.SMS_Recharge_RecordID, r.RechargeSMS, r.PerSMS_Price, r.Total_Price, r.Date, r.Is_Paid, ISNULL(g.UserName, '') AS UserName
FROM SMS_Recharge_Record r
LEFT JOIN Registration g ON r.RegistrationID = g.RegistrationID
WHERE r.SchoolID = @SchoolID
ORDER BY r.Date DESC
""", con);
            hist.Parameters.AddWithValue("@SchoolID", dto.SchoolID);
            await using var reader = await hist.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.SmsHistory.Add(new InstSmsRowDto
                {
                    Id = I(reader["SMS_Recharge_RecordID"]),
                    RechargeSms = M(reader["RechargeSMS"]),
                    PerSms = M(reader["PerSMS_Price"]),
                    Total = M(reader["Total_Price"]),
                    Date = Dt(reader["Date"]),
                    IsPaid = reader["Is_Paid"] is not DBNull && Convert.ToBoolean(reader["Is_Paid"]),
                    UserName = S(reader["UserName"])
                });
            }
        }
        catch
        {
        }
    }

    private static async Task LoadDueNoticeAsync(SqlConnection con, InstitutionDetailsDto dto, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 IsEnabled, HideUntilDate, Reason, CreatedDate
FROM SchoolInfo_DueNoticeSettings
WHERE SchoolID = @SchoolID AND IsEnabled = 1
ORDER BY CreatedDate DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", dto.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return;
            dto.DueNotice.Enabled = reader["IsEnabled"] is not DBNull && Convert.ToBoolean(reader["IsEnabled"]);
            dto.DueNotice.HideUntil = Dt(reader["HideUntilDate"]);
            dto.DueNotice.Reason = S(reader["Reason"]);
            dto.DueNotice.CreatedDate = Dt(reader["CreatedDate"]);
        }
        catch
        {
        }
    }

    private static AuthorityResult Fail(string error) => new() { Error = error };
    private static AuthorityResult Ok(string message) => new() { Succeeded = true, Message = message };
}
