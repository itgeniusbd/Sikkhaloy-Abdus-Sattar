using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityInvoiceService
{
    private readonly EduConnectionFactory _connections;

    public AuthorityInvoiceService(EduConnectionFactory connections) => _connections = connections;

    private static void Guard(SessionSnapshot session)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");
    }

    private static AuthorityResult Fail(string error) => new() { Succeeded = false, Error = error };
    private static AuthorityResult Ok(string? message = null, int id = 0) =>
        new() { Succeeded = true, Message = message, Id = id };

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static bool B(object? value) =>
        value is bool b ? b : value is null or DBNull ? false : Convert.ToBoolean(value);
    private static decimal M(object? value) =>
        value is null or DBNull ? 0m : Convert.ToDecimal(value);
    private static DateTime? Dt(object? value) =>
        value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);

    private static DateTime? ParseDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var formats = new[]
        {
            "yyyy-MM-dd", "dd MMM yyyy", "d MMM yyyy", "dd/MM/yyyy", "MM/dd/yyyy",
            "MMM yyyy", "MMMM yyyy", "yyyy-MM"
        };
        if (DateTime.TryParseExact(text.Trim(), formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var exact))
            return exact;
        return DateTime.TryParse(text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var any)
            ? any
            : null;
    }

    private static DateTime MonthEnd(DateTime value) =>
        new(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));

    private static decimal Percent(decimal paid, decimal receivable) =>
        receivable <= 0 ? 0 : Math.Round(paid * 100m / receivable, 2);

    public async Task<AuthAccountsPageDto> GetAccountsAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthAccountsPageDto { Summary = await LoadCollectSummaryAsync(0, ct) };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using (var cmd = new SqlCommand(
                """
                SELECT
                  (SELECT COUNT(DISTINCT SchoolID) FROM AAP_Invoice WHERE IsPaid = 0) AS UnpaidSchools,
                  (SELECT COUNT(*) FROM AAP_Invoice WHERE IsPaid = 0) AS UnpaidInvoices
                """, con))
            await using (var reader = await cmd.ExecuteReaderAsync(ct))
            {
                if (await reader.ReadAsync(ct))
                {
                    dto.UnpaidSchools = I(reader["UnpaidSchools"]);
                    dto.UnpaidInvoices = I(reader["UnpaidInvoices"]);
                }
            }

            await using var recent = new SqlCommand(
                """
                SELECT TOP 25 i.InvoiceID, i.SchoolID, s.SchoolName, i.Invoice_SN, c.InvoiceCategory,
                       i.Invoice_For, i.MonthName, i.IssuDate, i.Unit, i.UnitPrice, i.TotalAmount,
                       i.Discount, i.PaidAmount, i.Due
                FROM AAP_Invoice i
                INNER JOIN SchoolInfo s ON i.SchoolID = s.SchoolID
                INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
                WHERE i.IsPaid = 0
                ORDER BY i.InvoiceID DESC
                """, con);
            await using var r2 = await recent.ExecuteReaderAsync(ct);
            while (await r2.ReadAsync(ct))
                dto.RecentUnpaid.Add(ReadInvoiceLine(r2));
        }
        catch
        {
        }
        return dto;
    }

    public async Task<AuthProgressPageDto> GetProgressAsync(SessionSnapshot session, string? filter, CancellationToken ct)
    {
        Guard(session);
        var like = filter is "1" or "0" ? filter : "%";
        var dto = new AuthProgressPageDto { Filter = like };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using (var cmd = new SqlCommand(
                """
                SELECT COUNT(v.SchoolID) AS Total_Institution,
                       SUM(v.ActiveStudent + v.Reject_Countable) AS Total_Countable,
                       SUM(v.ActiveStudent) AS ActiveStudent,
                       SUM(v.Reject_Countable) AS Reject_Countable,
                       SUM(v.Reject_Uncountable) AS Reject_Uncountable,
                       SUM(CASE WHEN s.Fixed = 0 THEN (v.ActiveStudent + v.Reject_Countable) * s.Per_Student_Rate
                                ELSE s.Fixed END - s.Discount) AS Service_Charge
                FROM VW_Payment_Monthly_Stu v
                INNER JOIN SchoolInfo s ON v.SchoolID = s.SchoolID
                WHERE CAST(s.IS_ServiceChargeActive AS varchar(10)) LIKE @f
                """, con))
            {
                cmd.Parameters.AddWithValue("@f", like);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    dto.Institutions = I(reader["Total_Institution"]);
                    dto.TotalCountable = I(reader["Total_Countable"]);
                    dto.ActiveStudent = I(reader["ActiveStudent"]);
                    dto.RejectCountable = I(reader["Reject_Countable"]);
                    dto.RejectUncountable = I(reader["Reject_Uncountable"]);
                    dto.ServiceCharge = M(reader["Service_Charge"]);
                }
            }

            await using var grid = new SqlCommand(
                """
                SELECT v.SchoolID, s.SchoolName, v.ActiveStudent, v.Reject_Countable, v.Reject_Uncountable,
                       s.Per_Student_Rate, s.IS_ServiceChargeActive, s.Discount, s.Fixed, s.Free_SMS,
                       v.ActiveStudent + v.Reject_Countable AS Countable_Stu,
                       CASE WHEN s.Fixed = 0 THEN (v.ActiveStudent + v.Reject_Countable) * s.Per_Student_Rate
                            ELSE s.Fixed END - s.Discount AS Service_Charge
                FROM VW_Payment_Monthly_Stu v
                INNER JOIN SchoolInfo s ON v.SchoolID = s.SchoolID
                WHERE CAST(s.IS_ServiceChargeActive AS varchar(10)) LIKE @f
                ORDER BY s.SchoolName
                """, con);
            grid.Parameters.AddWithValue("@f", like);
            await using var r2 = await grid.ExecuteReaderAsync(ct);
            while (await r2.ReadAsync(ct))
            {
                dto.Rows.Add(new AuthProgressRowDto
                {
                    SchoolID = I(r2["SchoolID"]),
                    SchoolName = S(r2["SchoolName"]),
                    ActiveStudent = I(r2["ActiveStudent"]),
                    RejectCountable = I(r2["Reject_Countable"]),
                    RejectUncountable = I(r2["Reject_Uncountable"]),
                    FreeSms = I(r2["Free_SMS"]),
                    Countable = I(r2["Countable_Stu"]),
                    PerStudent = M(r2["Per_Student_Rate"]),
                    Fixed = M(r2["Fixed"]),
                    Discount = M(r2["Discount"]),
                    ServiceCharge = M(r2["Service_Charge"]),
                    PaymentActive = B(r2["IS_ServiceChargeActive"])
                });
            }
        }
        catch (Exception ex)
        {
            dto.Error = ex.Message;
        }
        return dto;
    }

    public async Task<AuthCollectPageDto> GetCollectAsync(
        SessionSnapshot session, int categoryId, string? month, string? detail, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthCollectPageDto
        {
            CategoryId = categoryId,
            Categories = await LoadCategoriesAsync(ct),
            Summary = await LoadCollectSummaryAsync(categoryId, ct)
        };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand(
            """
            SELECT YEAR(MonthName) AS Year, DATENAME(month, MonthName) AS Month, MONTH(MonthName) AS MonthNo,
                   COUNT(InvoiceID) AS InvoiceCount, SUM(Unit) AS Unit_Count,
                   SUM(TotalAmount) AS TotalAmount, SUM(Discount) AS Discount,
                   SUM(TotalAmount - Discount) AS Receivable, SUM(PaidAmount) AS PaidAmount, SUM(Due) AS Due
            FROM AAP_Invoice
            WHERE (@CatId = 0 OR InvoiceCategoryID = @CatId)
            GROUP BY YEAR(MonthName), DATENAME(month, MonthName), MONTH(MonthName)
            ORDER BY YEAR(MonthName), MONTH(MonthName)
            """, con))
        {
            cmd.Parameters.AddWithValue("@CatId", categoryId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var rec = M(reader["Receivable"]);
                var paid = M(reader["PaidAmount"]);
                dto.Months.Add(new AuthCollectMonthDto
                {
                    Year = I(reader["Year"]),
                    MonthNo = I(reader["MonthNo"]),
                    Month = $"{S(reader["Month"])} {I(reader["Year"])}",
                    InvoiceCount = I(reader["InvoiceCount"]),
                    UnitCount = M(reader["Unit_Count"]),
                    TotalAmount = M(reader["TotalAmount"]),
                    Discount = M(reader["Discount"]),
                    Receivable = rec,
                    PaidAmount = paid,
                    Due = M(reader["Due"]),
                    CollectPercent = Percent(paid, rec)
                });
            }
        }

        await using (var months = new SqlCommand(
            """
            SELECT DISTINCT CONVERT(varchar(7), MonthName, 120) AS Month
            FROM AAP_Invoice
            WHERE (@CatId = 0 OR InvoiceCategoryID = @CatId)
            ORDER BY Month DESC
            """, con))
        {
            months.Parameters.AddWithValue("@CatId", categoryId);
            await using var reader = await months.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                dto.DetailMonths.Add(S(reader["Month"]));
        }

        var monthKey = string.IsNullOrWhiteSpace(month) ? (dto.DetailMonths.FirstOrDefault() ?? "") : month.Trim();
        var paidOnly = string.Equals(detail, "paid", StringComparison.OrdinalIgnoreCase);
        dto.PaidRows = await LoadInvoiceDetailsAsync(con, categoryId, monthKey, paid: true, ct);
        dto.DueRows = await LoadInvoiceDetailsAsync(con, categoryId, monthKey, paid: false, ct);
        if (paidOnly)
            dto.DueRows = [];
        return dto;
    }

    public async Task<AuthManagePageDto> GetManageAsync(
        SessionSnapshot session, string? q, string? validation, string? payment, CancellationToken ct)
    {
        Guard(session);
        var dto = new AuthManagePageDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var sql = new StringBuilder(
            """
            SELECT Per_Student_Rate, School_SN, SchoolID, SchoolName, Date, Address, Phone, Free_SMS,
                   Fixed, Discount, IS_ServiceChargeActive,
                   CAST(CASE WHEN Validation = 'Valid' THEN 1 ELSE 0 END AS BIT) AS Validation, UserName
            FROM SchoolInfo
            WHERE 1 = 1
            """);
        await using var cmd = new SqlCommand { Connection = con };
        if (!string.IsNullOrWhiteSpace(q))
        {
            sql.Append(" AND (SchoolName LIKE @q OR UserName LIKE @q OR Phone LIKE @q OR CAST(SchoolID AS varchar(20)) LIKE @q)");
            cmd.Parameters.AddWithValue("@q", "%" + q.Trim() + "%");
        }
        if (validation is "Valid" or "Invalid")
        {
            sql.Append(" AND Validation = @v");
            cmd.Parameters.AddWithValue("@v", validation);
        }
        if (string.Equals(payment, "Active", StringComparison.OrdinalIgnoreCase))
            sql.Append(" AND IS_ServiceChargeActive = 1");
        else if (string.Equals(payment, "Inactive", StringComparison.OrdinalIgnoreCase))
            sql.Append(" AND IS_ServiceChargeActive = 0");
        sql.Append(" ORDER BY School_SN");
        cmd.CommandText = sql.ToString();
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var row = new AuthManageRowDto
                {
                    SchoolID = I(reader["SchoolID"]),
                    SchoolSn = I(reader["School_SN"]),
                    SchoolName = S(reader["SchoolName"]),
                    UserName = S(reader["UserName"]),
                    Phone = S(reader["Phone"]),
                    Address = S(reader["Address"]),
                    Date = Dt(reader["Date"]),
                    FreeSms = I(reader["Free_SMS"]),
                    PerStudent = M(reader["Per_Student_Rate"]),
                    Discount = M(reader["Discount"]),
                    Fixed = M(reader["Fixed"]),
                    PaymentActive = B(reader["IS_ServiceChargeActive"]),
                    Valid = B(reader["Validation"])
                };
                dto.Rows.Add(row);
            }
        }

        dto.Total = dto.Rows.Count;
        dto.Valid = dto.Rows.Count(x => x.Valid);
        dto.Invalid = dto.Rows.Count(x => !x.Valid);
        dto.PaymentActive = dto.Rows.Count(x => x.PaymentActive);
        await AttachCommitteeAsync(con, dto.Rows, ct);
        return dto;
    }

    public async Task<AuthorityResult> SaveManageAsync(
        SessionSnapshot session, AuthManageSaveRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new AuthManageSaveRequest();
        if (request.Rows.Count == 0)
            return Fail("ai.needRows");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureCommitteeBillingTableAsync(con, ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            foreach (var row in request.Rows)
            {
                await using (var cmd = new SqlCommand(
                    """
                    UPDATE SchoolInfo
                    SET Free_SMS = @Free, Discount = @Discount, Fixed = @Fixed,
                        IS_ServiceChargeActive = @Pay, Validation = @Valid, Per_Student_Rate = @Rate
                    WHERE SchoolID = @SchoolID
                    """, con, tx))
                {
                    cmd.Parameters.AddWithValue("@Free", row.FreeSms);
                    cmd.Parameters.AddWithValue("@Discount", row.Discount);
                    cmd.Parameters.AddWithValue("@Fixed", row.Fixed);
                    cmd.Parameters.AddWithValue("@Pay", row.PaymentActive);
                    cmd.Parameters.AddWithValue("@Valid", row.Valid ? "Valid" : "Invalid");
                    cmd.Parameters.AddWithValue("@Rate", row.PerStudent);
                    cmd.Parameters.AddWithValue("@SchoolID", row.SchoolID);
                    await cmd.ExecuteNonQueryAsync(ct);
                }

                await using (var device = new SqlCommand(
                    "UPDATE Attendance_Device_Setting SET IsActive = @IsActive WHERE SchoolID = @SchoolID", con, tx))
                {
                    device.Parameters.AddWithValue("@IsActive", row.Valid);
                    device.Parameters.AddWithValue("@SchoolID", row.SchoolID);
                    try { await device.ExecuteNonQueryAsync(ct); } catch { }
                }

                foreach (var cat in row.Committee)
                {
                    await using var bill = new SqlCommand(
                        """
                        IF EXISTS (SELECT 1 FROM CommitteeMember_Billing WHERE SchoolID = @SchoolID AND CommitteeMemberTypeId = @TypeId)
                            UPDATE CommitteeMember_Billing
                            SET IsIncluded = @Inc, IsActive = @Act, UpdatedDate = GETDATE()
                            WHERE SchoolID = @SchoolID AND CommitteeMemberTypeId = @TypeId
                        ELSE
                            INSERT INTO CommitteeMember_Billing (SchoolID, CommitteeMemberTypeId, IsIncluded, IsActive)
                            VALUES (@SchoolID, @TypeId, @Inc, @Act)
                        """, con, tx);
                    bill.Parameters.AddWithValue("@SchoolID", row.SchoolID);
                    bill.Parameters.AddWithValue("@TypeId", cat.TypeId);
                    bill.Parameters.AddWithValue("@Inc", cat.Included);
                    bill.Parameters.AddWithValue("@Act", cat.Active);
                    try { await bill.ExecuteNonQueryAsync(ct); } catch { }
                }
            }

            await tx.CommitAsync(ct);
            return Ok("ai.manageSaved");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    public async Task<AuthOnlinePayPageDto> GetOnlinePayAsync(
        SessionSnapshot session, string? type, int schoolId, string? method, DateTime? from, DateTime? to, CancellationToken ct)
    {
        Guard(session);
        var start = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var end = (to ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);
        var kind = type is "Online" or "Offline" ? type : "All";
        var payMethod = method ?? "";
        var dto = new AuthOnlinePayPageDto
        {
            Type = kind,
            SchoolID = schoolId,
            Method = payMethod,
            From = start,
            To = end.Date,
            Schools = await LoadAllSchoolsAsync(ct)
        };

        const string union = """
            SELECT SchoolID, SchoolName, Amount, PayMethod, CollectionType, CollectedBy, Reference, PaymentDate
            FROM (
                SELECT p.SchoolID, ISNULL(s.SchoolName, 'Unknown') AS SchoolName, p.Amount,
                       ISNULL(p.SP_Method, 'ShurjoPay') AS PayMethod, 'Online' AS CollectionType,
                       'ShurjoPay' AS CollectedBy, ISNULL(p.SP_TrxID, p.SP_OrderID) AS Reference, p.PaymentDate
                FROM AAP_Invoice_OnlinePayment p
                LEFT JOIN SchoolInfo s ON p.SchoolID = s.SchoolID
                WHERE p.PaymentDate BETWEEN @From AND @To
                  AND (@SchoolID = 0 OR p.SchoolID = @SchoolID)
                  AND (@Method = '' OR p.SP_Method LIKE '%' + @Method + '%')
                  AND (@Type = 'All' OR @Type = 'Online')
                UNION ALL
                SELECT r.SchoolID, ISNULL(s.SchoolName, 'Unknown'), r.TotalAmount,
                       ISNULL(r.Payment_Method, 'Cash'), 'Offline',
                       ISNULL(r.Collected_By, r.PaymentBy),
                       CAST(r.InvoiceReceipt_SN AS nvarchar(50)), r.PaidDate
                FROM AAP_Invoice_Receipt r
                LEFT JOIN SchoolInfo s ON r.SchoolID = s.SchoolID
                WHERE r.PaidDate BETWEEN @From AND @To
                  AND (@SchoolID = 0 OR r.SchoolID = @SchoolID)
                  AND (@Method = '' OR ISNULL(r.Payment_Method, '') LIKE '%' + @Method + '%')
                  AND (@Type = 'All' OR @Type = 'Offline')
                  AND ISNULL(r.Collected_By, '') NOT LIKE '%ShurjoPay%'
            ) AS Combined
            """;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using (var cmd = new SqlCommand(union + " ORDER BY PaymentDate DESC", con))
            {
                AddOnlineParams(cmd, start, end, schoolId, payMethod, kind);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    dto.Rows.Add(new AuthOnlinePayRowDto
                    {
                        SchoolID = I(reader["SchoolID"]),
                        SchoolName = S(reader["SchoolName"]),
                        Amount = M(reader["Amount"]),
                        Method = S(reader["PayMethod"]),
                        Type = S(reader["CollectionType"]),
                        CollectedBy = S(reader["CollectedBy"]),
                        Reference = S(reader["Reference"]),
                        PaymentDate = Dt(reader["PaymentDate"])
                    });
                }
            }

            await using var sum = new SqlCommand(
                $"""
                SELECT ISNULL(SUM(Amount), 0) AS TotalAmount, COUNT(*) AS TotalCount,
                       COUNT(DISTINCT SchoolID) AS InstitutionCount,
                       ISNULL(SUM(CASE WHEN CollectionType = 'Online' THEN Amount ELSE 0 END), 0) AS OnlineAmount,
                       ISNULL(SUM(CASE WHEN CollectionType = 'Offline' THEN Amount ELSE 0 END), 0) AS OfflineAmount,
                       ISNULL(SUM(CASE WHEN CollectionType = 'Online' THEN 1 ELSE 0 END), 0) AS OnlineCount,
                       ISNULL(SUM(CASE WHEN CollectionType = 'Offline' THEN 1 ELSE 0 END), 0) AS OfflineCount
                FROM ({union}) AS T
                """, con);
            AddOnlineParams(sum, start, end, schoolId, payMethod, kind);
            await using var r2 = await sum.ExecuteReaderAsync(ct);
            if (await r2.ReadAsync(ct))
            {
                dto.TotalAmount = M(r2["TotalAmount"]);
                dto.TotalCount = I(r2["TotalCount"]);
                dto.InstitutionCount = I(r2["InstitutionCount"]);
                dto.OnlineAmount = M(r2["OnlineAmount"]);
                dto.OfflineAmount = M(r2["OfflineAmount"]);
                dto.OnlineCount = I(r2["OnlineCount"]);
                dto.OfflineCount = I(r2["OfflineCount"]);
            }
        }
        catch
        {
        }
        return dto;
    }

    private async Task<AuthCollectSummaryDto> LoadCollectSummaryAsync(int categoryId, CancellationToken ct)
    {
        var dto = new AuthCollectSummaryDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            """
            SELECT COUNT(InvoiceID) AS InvoiceCount, SUM(Unit) AS Unit_Count,
                   SUM(TotalAmount) AS TotalAmount, SUM(Discount) AS Discount,
                   SUM(TotalAmount - Discount) AS Receivable, SUM(PaidAmount) AS PaidAmount, SUM(Due) AS Due
            FROM AAP_Invoice
            WHERE (@CatId = 0 OR InvoiceCategoryID = @CatId)
            """, con);
        cmd.Parameters.AddWithValue("@CatId", categoryId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            dto.InvoiceCount = I(reader["InvoiceCount"]);
            dto.UnitCount = M(reader["Unit_Count"]);
            dto.TotalAmount = M(reader["TotalAmount"]);
            dto.Discount = M(reader["Discount"]);
            dto.Receivable = M(reader["Receivable"]);
            dto.PaidAmount = M(reader["PaidAmount"]);
            dto.Due = M(reader["Due"]);
            dto.CollectPercent = Percent(dto.PaidAmount, dto.Receivable);
        }
        return dto;
    }

    private static async Task<List<AuthInvoiceLineDto>> LoadInvoiceDetailsAsync(
        SqlConnection con, int categoryId, string month, bool paid, CancellationToken ct)
    {
        var list = new List<AuthInvoiceLineDto>();
        await using var cmd = new SqlCommand(
            """
            SELECT i.InvoiceID, i.SchoolID, s.SchoolName, i.Invoice_SN, c.InvoiceCategory, i.Invoice_For,
                   i.MonthName, i.IssuDate, i.EndDate, i.Unit, i.UnitPrice, i.TotalAmount, i.Discount,
                   i.PaidAmount, i.Due
            FROM AAP_Invoice i
            INNER JOIN SchoolInfo s ON i.SchoolID = s.SchoolID
            INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
            WHERE i.IsPaid = @Paid
              AND (@CatId = 0 OR i.InvoiceCategoryID = @CatId)
              AND (@Month = '' OR CONVERT(varchar(7), i.MonthName, 120) = @Month)
            ORDER BY s.SchoolName, c.InvoiceCategory
            """, con);
        cmd.Parameters.AddWithValue("@Paid", paid);
        cmd.Parameters.AddWithValue("@CatId", categoryId);
        cmd.Parameters.AddWithValue("@Month", month ?? "");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(ReadInvoiceLine(reader));
        return list;
    }

    private static AuthInvoiceLineDto ReadInvoiceLine(SqlDataReader reader) => new()
    {
        InvoiceID = I(reader["InvoiceID"]),
        SchoolID = I(reader["SchoolID"]),
        SchoolName = S(reader["SchoolName"]),
        InvoiceSn = S(reader["Invoice_SN"]),
        Category = S(reader["InvoiceCategory"]),
        InvoiceFor = S(reader["Invoice_For"]),
        MonthName = Dt(reader["MonthName"]),
        IssueDate = Dt(reader["IssuDate"]),
        EndDate = Has(reader, "EndDate") ? Dt(reader["EndDate"]) : null,
        Unit = M(reader["Unit"]),
        UnitPrice = M(reader["UnitPrice"]),
        TotalAmount = M(reader["TotalAmount"]),
        Discount = M(reader["Discount"]),
        PaidAmount = M(reader["PaidAmount"]),
        Due = M(reader["Due"]),
        PayAmount = M(reader["Due"])
    };

    private static bool Has(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
            if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private async Task<List<AuthorityOptionDto>> LoadCategoriesAsync(CancellationToken ct)
    {
        var list = new List<AuthorityOptionDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "SELECT InvoiceCategoryID, InvoiceCategory FROM AAP_Invoice_Category ORDER BY InvoiceCategory", con);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(new AuthorityOptionDto { Id = I(reader["InvoiceCategoryID"]), Name = S(reader["InvoiceCategory"]) });
        return list;
    }

    private async Task<List<AuthorityOptionDto>> LoadAllSchoolsAsync(CancellationToken ct)
    {
        var list = new List<AuthorityOptionDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "SELECT SchoolID, CAST(SchoolID AS nvarchar(20)) + ' - ' + SchoolName AS Name FROM SchoolInfo ORDER BY SchoolID DESC", con);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(new AuthorityOptionDto { Id = I(reader["SchoolID"]), Name = S(reader["Name"]) });
        return list;
    }

    private static async Task AttachCommitteeAsync(SqlConnection con, List<AuthManageRowDto> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return;
        var map = rows.ToDictionary(x => x.SchoolID);
        try
        {
            var ids = string.Join(",", map.Keys);
            await using var cmd = new SqlCommand(
                $"""
                SELECT CMT.SchoolID, CMT.CommitteeMemberTypeId, CMT.CommitteeMemberType,
                       COUNT(CM.CommitteeMemberId) AS MemberCount,
                       COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END) AS ActiveMemberCount,
                       COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Inactive' THEN 1 END) AS InactiveMemberCount,
                       ISNULL(CMB.IsIncluded, 0) AS IsIncluded, ISNULL(CMB.IsActive, 1) AS IsActive
                FROM CommitteeMemberType CMT
                LEFT JOIN CommitteeMember CM ON CMT.CommitteeMemberTypeId = CM.CommitteeMemberTypeId AND CM.SchoolID = CMT.SchoolID
                LEFT JOIN CommitteeMember_Billing CMB ON CMT.CommitteeMemberTypeId = CMB.CommitteeMemberTypeId AND CMB.SchoolID = CMT.SchoolID
                WHERE CMT.SchoolID IN ({ids})
                GROUP BY CMT.SchoolID, CMT.CommitteeMemberTypeId, CMT.CommitteeMemberType, CMB.IsIncluded, CMB.IsActive
                ORDER BY CMT.CommitteeMemberType
                """, con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var schoolId = I(reader["SchoolID"]);
                if (!map.TryGetValue(schoolId, out var row)) continue;
                var item = new AuthCommitteeBillDto
                {
                    TypeId = I(reader["CommitteeMemberTypeId"]),
                    TypeName = S(reader["CommitteeMemberType"]),
                    MemberCount = I(reader["MemberCount"]),
                    ActiveCount = I(reader["ActiveMemberCount"]),
                    InactiveCount = I(reader["InactiveMemberCount"]),
                    Included = B(reader["IsIncluded"]),
                    Active = B(reader["IsActive"])
                };
                row.Committee.Add(item);
                if (item.Included && item.Active)
                    row.CommitteeTotal += item.ActiveCount;
            }
        }
        catch
        {
        }
    }

    private static async Task EnsureCommitteeBillingTableAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(
            """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
            BEGIN
                CREATE TABLE CommitteeMember_Billing (
                    BillingId INT IDENTITY(1,1) PRIMARY KEY,
                    SchoolID INT NOT NULL,
                    CommitteeMemberTypeId INT NOT NULL,
                    IsIncluded BIT NOT NULL DEFAULT 0,
                    IsActive BIT NOT NULL DEFAULT 1,
                    CreatedDate DATETIME DEFAULT GETDATE(),
                    UpdatedDate DATETIME DEFAULT GETDATE(),
                    CONSTRAINT UC_School_Category UNIQUE (SchoolID, CommitteeMemberTypeId)
                )
            END
            IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeMember_Billing')
               AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CommitteeMember_Billing') AND name = 'IsActive')
                ALTER TABLE CommitteeMember_Billing ADD IsActive BIT NOT NULL DEFAULT 1
            """, con);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void AddOnlineParams(SqlCommand cmd, DateTime from, DateTime to, int schoolId, string method, string type)
    {
        cmd.Parameters.AddWithValue("@From", from);
        cmd.Parameters.AddWithValue("@To", to);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Method", method);
        cmd.Parameters.AddWithValue("@Type", type);
    }
}
