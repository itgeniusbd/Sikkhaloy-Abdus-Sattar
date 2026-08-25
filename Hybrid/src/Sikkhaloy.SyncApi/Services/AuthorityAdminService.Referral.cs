using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityAdminService
{
    public async Task<AuthReferralPageDto> GetReferralAsync(SessionSnapshot session, int referenceId, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthReferralPageDto { ReferenceID = referenceId };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT
    r.ReferenceID,
    ISNULL(r.Reference_SN, 0) AS Reference_SN,
    r.Reference_Name,
    ISNULL(r.Reference_Phone, N'') AS Reference_Phone,
    ISNULL(r.Address, N'') AS Address,
    r.Marketing_StartDate,
    COUNT(DISTINCT rs.Reference_School_ID) AS TotalSchools,
    ISNULL((SELECT SUM(rc.Commission_Amount) FROM dbo.AAP_Reference_Commission rc WHERE rc.ReferenceID = r.ReferenceID), 0) AS TotalCommission,
    ISNULL((SELECT SUM(pr.Amount) FROM dbo.AAP_Reference_PaymentRecord pr WHERE pr.ReferenceID = r.ReferenceID), 0) AS PaidAmount
FROM dbo.AAP_Reference r
LEFT JOIN dbo.AAP_Reference_School rs ON r.ReferenceID = rs.ReferenceID
GROUP BY r.ReferenceID, r.Reference_SN, r.Reference_Name, r.Reference_Phone, r.Address, r.Marketing_StartDate
ORDER BY r.Reference_SN, r.Reference_Name
""", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var comm = M(reader["TotalCommission"]);
                var paid = M(reader["PaidAmount"]);
                var row = new AuthReferrerRowDto
                {
                    ReferenceID = I(reader["ReferenceID"]),
                    ReferenceSN = I(reader["Reference_SN"]),
                    Name = S(reader["Reference_Name"]),
                    Phone = S(reader["Reference_Phone"]),
                    Address = S(reader["Address"]),
                    StartDate = Dt(reader["Marketing_StartDate"]),
                    TotalSchools = I(reader["TotalSchools"]),
                    TotalCommission = comm,
                    PaidAmount = paid,
                    DueAmount = comm - paid
                };
                dto.Referrers.Add(row);
                if (row.ReferenceID == referenceId)
                    dto.ReferenceName = row.Name;
            }
        }

        if (referenceId > 0)
            dto.Assigned.AddRange(await LoadAssignedAsync(con, referenceId, ct));

        return dto;
    }

    public async Task<AuthorityResult> SaveReferrerAsync(
        SessionSnapshot session, AuthReferrerSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthReferrerSaveRequest();
        var name = (request.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("al.refName");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request.ReferenceID > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.AAP_Reference
SET Reference_Name = @Name, Reference_Phone = @Phone, Address = @Address, Marketing_StartDate = @Start
WHERE ReferenceID = @Id
""", con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Phone", (request.Phone ?? "").Trim());
            cmd.Parameters.AddWithValue("@Address", (request.Address ?? "").Trim());
            cmd.Parameters.AddWithValue("@Start", DbDate(request.StartDate));
            cmd.Parameters.AddWithValue("@Id", request.ReferenceID);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("ab.saved", request.ReferenceID);
        }

        await using (var cmd = new SqlCommand("""
INSERT INTO dbo.AAP_Reference (Reference_Name, Reference_Phone, Address, Marketing_StartDate)
VALUES (@Name, @Phone, @Address, @Start);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con))
        {
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Phone", (request.Phone ?? "").Trim());
            cmd.Parameters.AddWithValue("@Address", (request.Address ?? "").Trim());
            cmd.Parameters.AddWithValue("@Start", DbDate(request.StartDate));
            return Ok("al.refAdded", I(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    public async Task<AuthSchoolSearchPageDto> SearchSchoolsAsync(
        SessionSnapshot session, string? q, int referenceId, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthSchoolSearchPageDto();
        var keyword = (q ?? "").Trim();
        if (keyword.Length == 0)
            return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT TOP 50
    s.SchoolID,
    s.SchoolName,
    ISNULL(s.Phone, N'') AS Phone,
    CASE WHEN EXISTS (SELECT 1 FROM dbo.AAP_Invoice i WHERE i.SchoolID = s.SchoolID) THEN 1 ELSE 0 END AS HasInvoice
FROM dbo.SchoolInfo s
WHERE (
        s.SchoolName LIKE @Keyword
     OR ISNULL(s.Phone, N'') LIKE @Keyword
     OR ISNULL(s.UserName, N'') LIKE @Keyword
     OR CAST(s.SchoolID AS nvarchar(20)) LIKE @Keyword
     OR CAST(ISNULL(s.School_SN, 0) AS nvarchar(20)) LIKE @Keyword
  )
  AND (@RefID = 0 OR s.SchoolID NOT IN (
        SELECT SchoolID FROM dbo.AAP_Reference_School WHERE ReferenceID = @RefID
  ))
ORDER BY CASE WHEN s.SchoolName LIKE @StartsWith THEN 0 ELSE 1 END, s.SchoolID DESC
""", con);
        cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
        cmd.Parameters.AddWithValue("@StartsWith", keyword + "%");
        cmd.Parameters.AddWithValue("@RefID", referenceId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            dto.Items.Add(new AuthSchoolSearchDto
            {
                SchoolID = I(reader["SchoolID"]),
                SchoolName = S(reader["SchoolName"]),
                Phone = S(reader["Phone"]),
                HasInvoice = I(reader["HasInvoice"]) == 1
            });
        }
        return dto;
    }

    public async Task<AuthorityResult> AssignSchoolAsync(
        SessionSnapshot session, AuthAssignSchoolRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthAssignSchoolRequest();
        if (request.ReferenceID <= 0 || request.SchoolID <= 0)
            return Fail("al.needRefSchool");
        if (request.Percentage <= 0)
            return Fail("al.pct");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var chk = new SqlCommand("""
SELECT COUNT(*) FROM dbo.AAP_Reference_School WHERE SchoolID = @SchoolID AND ReferenceID = @RefID
""", con))
        {
            chk.Parameters.AddWithValue("@SchoolID", request.SchoolID);
            chk.Parameters.AddWithValue("@RefID", request.ReferenceID);
            if (I(await chk.ExecuteScalarAsync(ct)) > 0)
                return Fail("al.alreadyAssigned");
        }

        await using var cmd = new SqlCommand("""
INSERT INTO dbo.AAP_Reference_School (SchoolID, ReferenceID, Percentage, School_SignUp_Date, End_Reference_Date)
VALUES (@SchoolID, @RefID, @Pct, @Signup, @Expire)
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
        cmd.Parameters.AddWithValue("@RefID", request.ReferenceID);
        cmd.Parameters.AddWithValue("@Pct", request.Percentage);
        cmd.Parameters.AddWithValue("@Signup", DbDate(request.SignupDate));
        cmd.Parameters.AddWithValue("@Expire", DbDate(request.ExpireDate));
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("al.assigned");
    }

    public async Task<AuthorityResult> UpdateAssignAsync(
        SessionSnapshot session, AuthAssignUpdateRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthAssignUpdateRequest();
        if (request.ReferenceSchoolID <= 0)
            return Fail("al.failed");
        if (request.Percentage <= 0)
            return Fail("al.pct");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE dbo.AAP_Reference_School
SET Percentage = @Pct, School_SignUp_Date = @Signup, End_Reference_Date = @Expire
WHERE Reference_School_ID = @Id
""", con);
        cmd.Parameters.AddWithValue("@Pct", request.Percentage);
        cmd.Parameters.AddWithValue("@Signup", DbDate(request.SignupDate));
        cmd.Parameters.AddWithValue("@Expire", DbDate(request.ExpireDate));
        cmd.Parameters.AddWithValue("@Id", request.ReferenceSchoolID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.saved");
    }

    public async Task<AuthorityResult> DeleteAssignAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        Guard(session);
        if (id <= 0)
            return Fail("al.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("DELETE FROM dbo.AAP_Reference_School WHERE Reference_School_ID = @Id", con);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("al.deleted");
    }

    public async Task<AuthCommissionPageDto> GetCommissionAsync(
        SessionSnapshot session, int refId, DateTime from, DateTime to, string? status, int detailId, CancellationToken ct)
    {
        Guard(session);
        if (from == default) from = new DateTime(DateTime.Today.Year, 1, 1);
        if (to == default) to = DateTime.Today;
        to = to.Date.AddDays(1).AddTicks(-1);
        status = (status ?? "").Trim().ToLowerInvariant();

        var dto = new AuthCommissionPageDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand(
            "SELECT ReferenceID, Reference_Name FROM dbo.AAP_Reference ORDER BY Reference_Name", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Referrers.Add(new AuthorityOptionDto
                {
                    Id = I(reader["ReferenceID"]),
                    Name = S(reader["Reference_Name"])
                });
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT
    r.ReferenceID,
    r.Reference_Name,
    ISNULL(r.Reference_Phone, N'') AS Reference_Phone,
    COUNT(DISTINCT rs.Reference_School_ID) AS TotalSchools,
    ISNULL((SELECT SUM(rc.Commission_Amount) FROM dbo.AAP_Reference_Commission rc WHERE rc.ReferenceID = r.ReferenceID AND rc.Commission_Date BETWEEN @From AND @To), 0) AS TotalCommission,
    ISNULL((SELECT SUM(Amount) FROM dbo.AAP_Reference_PaymentRecord pr WHERE pr.ReferenceID = r.ReferenceID AND pr.PaidDate BETWEEN @From AND @To), 0) AS PaidAmount
FROM dbo.AAP_Reference r
LEFT JOIN dbo.AAP_Reference_School rs ON r.ReferenceID = rs.ReferenceID
WHERE (@RefID = 0 OR r.ReferenceID = @RefID)
GROUP BY r.ReferenceID, r.Reference_Name, r.Reference_Phone
ORDER BY r.Reference_Name
""", con))
        {
            cmd.Parameters.AddWithValue("@From", from.Date);
            cmd.Parameters.AddWithValue("@To", to);
            cmd.Parameters.AddWithValue("@RefID", refId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var comm = M(reader["TotalCommission"]);
                var paid = M(reader["PaidAmount"]);
                var due = comm - paid;
                if (status == "due" && due <= 0)
                    continue;
                if (status == "paid" && !(paid > 0 && due <= 0))
                    continue;
                dto.Rows.Add(new AuthCommissionRowDto
                {
                    ReferenceID = I(reader["ReferenceID"]),
                    Name = S(reader["Reference_Name"]),
                    Phone = S(reader["Reference_Phone"]),
                    TotalSchools = I(reader["TotalSchools"]),
                    TotalCommission = comm,
                    PaidAmount = paid,
                    DueAmount = due
                });
            }
        }

        dto.TotalCommission = dto.Rows.Sum(x => x.TotalCommission);
        dto.TotalPaid = dto.Rows.Sum(x => x.PaidAmount);
        dto.TotalDue = dto.Rows.Sum(x => x.DueAmount);
        dto.TotalRef = dto.Rows.Count;

        var detail = detailId > 0 ? detailId : 0;
        if (detail > 0)
        {
            dto.DetailRefId = detail;
            dto.DetailRefName = dto.Rows.FirstOrDefault(x => x.ReferenceID == detail)?.Name
                ?? dto.Referrers.FirstOrDefault(x => x.Id == detail)?.Name
                ?? "";

            await using (var cmd = new SqlCommand("""
SELECT
    rs.Reference_School_ID,
    s.SchoolName,
    rs.Percentage,
    rs.School_SignUp_Date,
    rs.End_Reference_Date,
    ISNULL((SELECT SUM(rc.ServiceCharge_Amount) FROM dbo.AAP_Reference_Commission rc WHERE rc.Reference_School_ID = rs.Reference_School_ID AND rc.Commission_Date BETWEEN @From AND @To), 0) AS TotalServiceCharge,
    ISNULL((SELECT SUM(rc.Commission_Amount) FROM dbo.AAP_Reference_Commission rc WHERE rc.Reference_School_ID = rs.Reference_School_ID AND rc.Commission_Date BETWEEN @From AND @To), 0) AS CommissionAmount,
    ISNULL((SELECT SUM(pr.Amount) FROM dbo.AAP_Reference_PaymentRecord pr WHERE pr.Reference_School_ID = rs.Reference_School_ID AND pr.PaidDate BETWEEN @From AND @To), 0) AS PaidAmount
FROM dbo.AAP_Reference_School rs
INNER JOIN dbo.SchoolInfo s ON rs.SchoolID = s.SchoolID
WHERE rs.ReferenceID = @RefID
ORDER BY s.SchoolName
""", con))
            {
                cmd.Parameters.AddWithValue("@RefID", detail);
                cmd.Parameters.AddWithValue("@From", from.Date);
                cmd.Parameters.AddWithValue("@To", to);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var comm = M(reader["CommissionAmount"]);
                    var paid = M(reader["PaidAmount"]);
                    dto.Schools.Add(new AuthCommissionSchoolDto
                    {
                        ReferenceSchoolID = I(reader["Reference_School_ID"]),
                        SchoolName = S(reader["SchoolName"]),
                        Percentage = M(reader["Percentage"]),
                        SignupDate = Dt(reader["School_SignUp_Date"]),
                        ExpireDate = Dt(reader["End_Reference_Date"]),
                        TotalServiceCharge = M(reader["TotalServiceCharge"]),
                        CommissionAmount = comm,
                        PaidAmount = paid,
                        DueAmount = comm - paid
                    });
                }
            }

            await using (var cmd = new SqlCommand("""
SELECT pr.ReferencePaymentRecordID, pr.PaidDate, pr.Amount, ISNULL(pr.Paid_By, N'') AS Paid_By,
       ISNULL(pr.Payment_Method, N'') AS Payment_Method, ISNULL(pr.Note, N'') AS Note
FROM dbo.AAP_Reference_PaymentRecord pr
WHERE pr.ReferenceID = @RefID
ORDER BY pr.PaidDate DESC
""", con))
            {
                cmd.Parameters.AddWithValue("@RefID", detail);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    dto.History.Add(new AuthPayHistoryDto
                    {
                        Id = I(reader["ReferencePaymentRecordID"]),
                        PaidDate = Dt(reader["PaidDate"]),
                        Amount = M(reader["Amount"]),
                        PaidBy = S(reader["Paid_By"]),
                        Method = S(reader["Payment_Method"]),
                        Note = S(reader["Note"])
                    });
                }
            }
        }

        return dto;
    }

    public async Task<AuthorityResult> PayCommissionAsync(
        SessionSnapshot session, AuthCommissionPayRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthCommissionPayRequest();
        if (request.ReferenceID <= 0)
            return Fail("al.needRef");
        if (request.Amount <= 0)
            return Fail("al.payAmount");

        var paidDate = request.PaidDate ?? DateTime.Now;
        if (paidDate.TimeOfDay == TimeSpan.Zero)
            paidDate = paidDate.Date + DateTime.Now.TimeOfDay;
        var method = string.IsNullOrWhiteSpace(request.Method) ? "Cash" : request.Method.Trim();
        var paidBy = string.IsNullOrWhiteSpace(request.PaidBy)
            ? (session.UserName ?? "")
            : request.PaidBy.Trim();

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.AAP_Reference_PaymentRecord
    (Reference_PayOrderID, ReferenceID, Reference_School_ID, SchoolID, InvoiceID, Amount, PaidDate, Paid_By, Payment_Method, Note)
VALUES (0, @RefID, 0, 0, 0, @Amount, @Date, @PaidBy, @Method, @Note)
""", con);
        cmd.Parameters.AddWithValue("@RefID", request.ReferenceID);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Date", paidDate);
        cmd.Parameters.AddWithValue("@PaidBy", paidBy);
        cmd.Parameters.AddWithValue("@Method", method);
        cmd.Parameters.AddWithValue("@Note", (request.Note ?? "").Trim());
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("al.paySaved");
    }

    private async Task<List<AuthAssignedSchoolDto>> LoadAssignedAsync(SqlConnection con, int refId, CancellationToken ct)
    {
        var items = new List<AuthAssignedSchoolDto>();
        await using var cmd = new SqlCommand("""
SELECT
    rs.Reference_School_ID,
    s.SchoolID,
    s.SchoolName,
    ISNULL(s.Phone, N'') AS Phone,
    rs.Percentage,
    rs.School_SignUp_Date,
    rs.End_Reference_Date,
    ISNULL((SELECT SUM(rc.Commission_Amount) FROM dbo.AAP_Reference_Commission rc WHERE rc.Reference_School_ID = rs.Reference_School_ID), 0) AS TotalCommission,
    ISNULL((SELECT SUM(pr.Amount) FROM dbo.AAP_Reference_PaymentRecord pr WHERE pr.Reference_School_ID = rs.Reference_School_ID), 0) AS PaidCommission
FROM dbo.AAP_Reference_School rs
INNER JOIN dbo.SchoolInfo s ON rs.SchoolID = s.SchoolID
WHERE rs.ReferenceID = @RefID
ORDER BY rs.Reference_School_ID DESC
""", con);
        cmd.Parameters.AddWithValue("@RefID", refId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var expire = Dt(reader["End_Reference_Date"]);
            items.Add(new AuthAssignedSchoolDto
            {
                ReferenceSchoolID = I(reader["Reference_School_ID"]),
                SchoolID = I(reader["SchoolID"]),
                SchoolName = S(reader["SchoolName"]),
                Phone = S(reader["Phone"]),
                Percentage = M(reader["Percentage"]),
                SignupDate = Dt(reader["School_SignUp_Date"]),
                ExpireDate = expire,
                TotalCommission = M(reader["TotalCommission"]),
                PaidCommission = M(reader["PaidCommission"]),
                Expired = expire.HasValue && expire.Value.Date < DateTime.Today
            });
        }
        return items;
    }
}
