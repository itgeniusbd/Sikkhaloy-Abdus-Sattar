using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class AccountsService
{
    private readonly EduConnectionFactory _connections;
    private readonly PaymentSmsService _sms;
    private readonly InventoryService _inventory;

    public AccountsService(EduConnectionFactory connections, PaymentSmsService sms, InventoryService inventory)
    {
        _connections = connections;
        _sms = sms;
        _inventory = inventory;
    }

    private static AccountsResult Fail(string error) => new() { Error = error };
    private static AccountsResult Ok(int id = 0, int saved = 0, int failed = 0, string? receipt = null) =>
        new() { Succeeded = true, Id = id, Count = saved, Saved = saved, Failed = failed, ReceiptNo = receipt };

    private static decimal ToDec(object value) => value is DBNull or null ? 0 : Convert.ToDecimal(value);
    private static int ToInt(object value) => value is DBNull or null ? 0 : Convert.ToInt32(value);
    private static string? NullString(object value)
    {
        var text = value is DBNull or null ? null : value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static decimal DueOf(decimal amount, decimal lateFee, decimal discount, decimal lateDisc, decimal paid, DateTime endDate) =>
        amount + (endDate.Date < DateTime.Today ? lateFee : 0) - discount - lateDisc - paid;

    public async Task<IReadOnlyList<PaymentRoleDto>> ListRolesAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT RoleID, Role, NumberOfPay, Description
FROM dbo.Income_Roles
WHERE SchoolID = @SchoolID
ORDER BY Role
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<PaymentRoleDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PaymentRoleDto
            {
                RoleID = ToInt(reader["RoleID"]),
                Role = reader["Role"]?.ToString() ?? "",
                NumberOfPay = ToInt(reader["NumberOfPay"]),
                Description = NullString(reader["Description"])
            });
        }
        return items;
    }

    public async Task<AccountsResult> CreateRoleAsync(SessionSnapshot session, SavePaymentRoleRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Role ?? "").Trim();
        if (name.Length == 0)
            return Fail("acc.needRole");
        var pays = request?.NumberOfPay ?? 0;
        if (pays <= 0)
            return Fail("acc.needPays");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var check = new SqlCommand("SELECT RoleID FROM dbo.Income_Roles WHERE SchoolID = @SchoolID AND Role = @Role", con))
        {
            check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            check.Parameters.AddWithValue("@Role", name);
            if (await check.ExecuteScalarAsync(cancellationToken) is not null)
                return Fail("acc.roleExists");
        }
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Income_Roles (SchoolID, RegistrationID, Role, NumberOfPay, Description, Date)
VALUES (@SchoolID, @RegistrationID, @Role, @Pays, @Desc, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Role", name);
        cmd.Parameters.AddWithValue("@Pays", pays);
        cmd.Parameters.AddWithValue("@Desc", (object?)request?.Description ?? DBNull.Value);
        return Ok(Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)));
    }

    public async Task<AccountsResult> UpdateRoleAsync(SessionSnapshot session, int id, SavePaymentRoleRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Role ?? "").Trim();
        if (id <= 0 || name.Length == 0)
            return Fail("acc.needRole");
        var pays = request?.NumberOfPay ?? 0;
        if (pays <= 0)
            return Fail("acc.needPays");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Income_Roles
SET Role = @Role, NumberOfPay = @Pays, Description = @Desc
WHERE RoleID = @ID AND SchoolID = @SchoolID
  AND NOT EXISTS (SELECT 1 FROM dbo.Income_Roles x WHERE x.SchoolID = @SchoolID AND x.Role = @Role AND x.RoleID <> @ID)
""", con);
        cmd.Parameters.AddWithValue("@Role", name);
        cmd.Parameters.AddWithValue("@Pays", pays);
        cmd.Parameters.AddWithValue("@Desc", (object?)request?.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0 ? Ok(id) : Fail("acc.roleExists");
    }

    public async Task<AccountsResult> DeleteRoleAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("acc.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var exists = new SqlCommand(
            "SELECT RoleID FROM dbo.Income_Roles WHERE RoleID = @ID AND SchoolID = @SchoolID", con))
        {
            exists.Parameters.AddWithValue("@ID", id);
            exists.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            if (await exists.ExecuteScalarAsync(cancellationToken) is null)
                return Fail("acc.failed");
        }
        try
        {
            await using (var ctx = new SqlCommand("SET CONTEXT_INFO @Ctx", con))
            {
                ctx.Parameters.Add("@Ctx", SqlDbType.VarBinary, 128).Value = BitConverter.GetBytes(session.RegistrationID);
                await ctx.ExecuteNonQueryAsync(cancellationToken);
            }
            await using var cmd = new SqlCommand(
                "DELETE FROM dbo.Income_Roles WHERE RoleID = @ID AND SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException)
        {
            return Fail("acc.roleUsed");
        }
        await using (var still = new SqlCommand(
            "SELECT RoleID FROM dbo.Income_Roles WHERE RoleID = @ID AND SchoolID = @SchoolID", con))
        {
            still.Parameters.AddWithValue("@ID", id);
            still.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            if (await still.ExecuteScalarAsync(cancellationToken) is not null)
                return Fail("acc.roleUsed");
        }
        return Ok(id);
    }

    public async Task<IReadOnlyList<AssignedRoleDto>> ListAssignedAsync(
        SessionSnapshot session, int classId, int roleId, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT a.AssignRoleID, a.RoleID, r.Role, r.NumberOfPay, a.PayFor, a.Amount, a.LateFee, a.StartDate, a.EndDate
FROM dbo.Income_Assign_Role AS a
INNER JOIN dbo.Income_Roles AS r ON a.RoleID = r.RoleID
WHERE a.SchoolID = @SchoolID AND a.EducationYearID = @YearID AND a.ClassID = @ClassID
  AND (@RoleID = 0 OR a.RoleID = @RoleID)
ORDER BY a.StartDate, a.PayFor
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@RoleID", roleId);
        var items = new List<AssignedRoleDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AssignedRoleDto
            {
                AssignRoleID = ToInt(reader["AssignRoleID"]),
                RoleID = ToInt(reader["RoleID"]),
                Role = reader["Role"]?.ToString() ?? "",
                NumberOfPay = ToInt(reader["NumberOfPay"]),
                PayFor = reader["PayFor"]?.ToString() ?? "",
                Amount = ToDec(reader["Amount"]),
                LateFee = ToDec(reader["LateFee"]),
                StartDate = Convert.ToDateTime(reader["StartDate"]).Date,
                EndDate = Convert.ToDateTime(reader["EndDate"]).Date
            });
        }
        return items;
    }

    public async Task<AccountsResult> AssignRoleAsync(SessionSnapshot session, SaveAssignedRoleRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ClassID <= 0 || request.RoleID <= 0)
            return Fail("acc.needClassRole");
        var payFor = request.PayFor.Trim();
        if (payFor.Length == 0)
            return Fail("acc.needAssign");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        int limit;
        await using (var lim = new SqlCommand("SELECT NumberOfPay FROM dbo.Income_Roles WHERE RoleID = @ID AND SchoolID = @SchoolID", con))
        {
            lim.Parameters.AddWithValue("@ID", request.RoleID);
            lim.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            limit = ToInt(await lim.ExecuteScalarAsync(cancellationToken) ?? 0);
        }
        int count;
        await using (var cnt = new SqlCommand("""
SELECT COUNT(*) FROM dbo.Income_Assign_Role
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID = @ClassID AND RoleID = @RoleID
""", con))
        {
            cnt.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cnt.Parameters.AddWithValue("@YearID", session.EducationYearID);
            cnt.Parameters.AddWithValue("@ClassID", request.ClassID);
            cnt.Parameters.AddWithValue("@RoleID", request.RoleID);
            count = ToInt(await cnt.ExecuteScalarAsync(cancellationToken) ?? 0);
        }
        if (count >= limit)
            return Fail("acc.overLimit");
        int dup;
        await using (var existing = new SqlCommand("""
SELECT COUNT(*) FROM dbo.Income_Assign_Role
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID = @ClassID AND RoleID = @RoleID AND PayFor = @PayFor
""", con))
        {
            existing.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            existing.Parameters.AddWithValue("@YearID", session.EducationYearID);
            existing.Parameters.AddWithValue("@ClassID", request.ClassID);
            existing.Parameters.AddWithValue("@RoleID", request.RoleID);
            existing.Parameters.AddWithValue("@PayFor", payFor);
            dup = ToInt(await existing.ExecuteScalarAsync(cancellationToken) ?? 0);
        }
        if (dup > 0)
            return Fail("acc.dupPayFor");
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Income_Assign_Role (SchoolID, RegistrationID, RoleID, ClassID, EducationYearID, PayFor, Amount, LateFee, StartDate, EndDate, Date)
VALUES (@SchoolID, @RegistrationID, @RoleID, @ClassID, @YearID, @PayFor, @Amount, @LateFee, @Start, @End, GETDATE())
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@RoleID", request.RoleID);
        cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@PayFor", payFor);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@LateFee", request.LateFee);
        cmd.Parameters.AddWithValue("@Start", request.StartDate.Date);
        cmd.Parameters.AddWithValue("@End", request.EndDate.Date);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<AssignableRolesDto> ListAssignableAsync(
        SessionSnapshot session, IReadOnlyList<int> classIds, CancellationToken cancellationToken)
    {
        var result = new AssignableRolesDto();
        var ids = classIds.Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return result;
        var inList = string.Join(",", ids);
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var roles = new List<PaymentRoleDto>();
        await using (var cmd = new SqlCommand("""
SELECT RoleID, Role, NumberOfPay, Description
FROM dbo.Income_Roles
WHERE SchoolID = @SchoolID
ORDER BY Role
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                roles.Add(new PaymentRoleDto
                {
                    RoleID = ToInt(reader["RoleID"]),
                    Role = reader["Role"]?.ToString() ?? "",
                    NumberOfPay = ToInt(reader["NumberOfPay"]),
                    Description = NullString(reader["Description"])
                });
            }
        }

        var assigned = new List<(int RoleID, int ClassID, string PayFor)>();
        await using (var cmd = new SqlCommand($"""
SELECT RoleID, ClassID, PayFor
FROM dbo.Income_Assign_Role
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND ClassID IN ({inList})
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                assigned.Add((
                    ToInt(reader["RoleID"]),
                    ToInt(reader["ClassID"]),
                    reader["PayFor"]?.ToString()?.Trim() ?? ""));
            }
        }

        var byRole = assigned.ToLookup(x => x.RoleID);
        foreach (var role in roles)
        {
            var rows = byRole[role.RoleID].ToList();
            var byClass = rows.ToLookup(x => x.ClassID);
            var limit = role.NumberOfPay <= 1 ? 1 : role.NumberOfPay;
            var remaining = ids.Select(classId =>
            {
                var used = byClass[classId].Count();
                return Math.Max(0, limit - used);
            }).ToList();
            var status = new RoleAssignStatusDto
            {
                RoleID = role.RoleID,
                Role = role.Role,
                NumberOfPay = limit,
                Description = role.Description,
                MaxRemaining = remaining.Count == 0 ? 0 : remaining.Max(),
                ClassesNeeding = remaining.Count(x => x > 0),
                SelectedClassCount = ids.Count,
                Assigned = rows
                    .Where(x => x.PayFor.Length > 0)
                    .GroupBy(x => x.PayFor, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new AssignedPayForDto
                    {
                        PayFor = g.First().PayFor,
                        ClassCount = g.Select(x => x.ClassID).Distinct().Count()
                    })
                    .OrderBy(x => x.PayFor, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
            if (role.NumberOfPay <= 1)
                result.OneTime.Add(status);
            else
                result.Multi.Add(status);
        }
        return result;
    }

    public async Task<AccountsResult> BulkAssignAsync(SessionSnapshot session, BulkAssignRoleRequest? request, CancellationToken cancellationToken)
    {
        var classIds = request?.ClassIDs.Where(x => x > 0).Distinct().ToList() ?? [];
        var items = request?.Items
            .Where(x => x.RoleID > 0 && !string.IsNullOrWhiteSpace(x.PayFor))
            .ToList() ?? [];
        if (classIds.Count == 0)
            return Fail("acc.needClasses");
        if (items.Count == 0)
            return Fail("acc.needAssign");
        var saved = 0;
        var skipped = 0;
        var failed = 0;
        foreach (var classId in classIds)
        {
            foreach (var item in items)
            {
                var one = await AssignRoleAsync(session, new SaveAssignedRoleRequest
                {
                    ClassID = classId,
                    RoleID = item.RoleID,
                    PayFor = item.PayFor,
                    Amount = item.Amount,
                    LateFee = item.LateFee,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate
                }, cancellationToken);
                if (one.Succeeded)
                    saved++;
                else if (one.Error is "acc.overLimit" or "acc.dupPayFor")
                    skipped++;
                else
                    failed++;
            }
        }
        if (saved == 0)
            return Fail(skipped > 0 ? "acc.nothingNew" : (failed > 0 ? "acc.overLimit" : "acc.failed"));
        return Ok(saved: saved, failed: skipped);
    }

    public async Task<AccountsResult> UpdateAssignedAsync(SessionSnapshot session, UpdateAssignedRoleRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.AssignRoleID <= 0)
            return Fail("acc.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Income_Assign_Role
SET PayFor = @PayFor, Amount = @Amount, LateFee = @LateFee, StartDate = @Start, EndDate = @End
WHERE AssignRoleID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@PayFor", request.PayFor.Trim());
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@LateFee", request.LateFee);
        cmd.Parameters.AddWithValue("@Start", request.StartDate.Date);
        cmd.Parameters.AddWithValue("@End", request.EndDate.Date);
        cmd.Parameters.AddWithValue("@ID", request.AssignRoleID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0 ? Ok() : Fail("acc.failed");
    }

    public async Task<AccountsResult> DeleteAssignedAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("DELETE FROM dbo.Income_Assign_Role WHERE AssignRoleID = @ID AND SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0 ? Ok() : Fail("acc.failed");
        }
        catch (SqlException)
        {
            return Fail("acc.roleUsed");
        }
    }

    public async Task<IReadOnlyList<PayOrderStudentDto>> ListPayOrderStudentsAsync(
        SessionSnapshot session, int classId, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT sc.StudentID, sc.StudentClassID, sc.ClassID, s.ID, s.StudentsName, sc.RollNo, ISNULL(sc.Is_New, 1) AS IsNew
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
WHERE sc.SchoolID = @SchoolID AND sc.EducationYearID = @YearID AND s.Status = N'Active'
  AND (@ClassID = 0 OR sc.ClassID = @ClassID)
ORDER BY CASE WHEN ISNUMERIC(sc.RollNo) = 1 THEN CAST(sc.RollNo AS INT) ELSE 0 END, s.ID
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        var items = new List<PayOrderStudentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PayOrderStudentDto
            {
                StudentID = ToInt(reader["StudentID"]),
                StudentClassID = ToInt(reader["StudentClassID"]),
                ClassID = ToInt(reader["ClassID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                RollNo = NullString(reader["RollNo"]),
                IsNew = ToInt(reader["IsNew"]) != 0
            });
        }
        return items;
    }

    public async Task<AccountsResult> CreatePayOrdersAsync(SessionSnapshot session, CreatePayOrdersRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.Items.Count == 0)
            return Fail("acc.needRows");
        if (request.StudentClassIDs.Count == 0 && request.StudentIDs.Count > 0)
            request.StudentClassIDs = await ResolveStudentClassIdsAsync(session, request.StudentIDs, cancellationToken);
        if (request.StudentClassIDs.Count == 0)
            return Fail("acc.needRows");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        var failed = 0;
        foreach (var studentClassId in request.StudentClassIDs.Distinct())
        {
            int studentId, classId;
            await using (var find = new SqlCommand("""
SELECT StudentID, ClassID FROM dbo.StudentsClass
WHERE StudentClassID = @ID AND SchoolID = @SchoolID AND EducationYearID = @YearID
""", con))
            {
                find.Parameters.AddWithValue("@ID", studentClassId);
                find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                find.Parameters.AddWithValue("@YearID", session.EducationYearID);
                await using var reader = await find.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    continue;
                studentId = ToInt(reader["StudentID"]);
                classId = ToInt(reader["ClassID"]);
            }

            foreach (var item in request.Items)
            {
                if (item.RoleID <= 0 || string.IsNullOrWhiteSpace(item.PayFor))
                {
                    failed++;
                    continue;
                }
                int limit;
                await using (var lim = new SqlCommand("SELECT NumberOfPay FROM dbo.Income_Roles WHERE RoleID = @ID", con))
                {
                    lim.Parameters.AddWithValue("@ID", item.RoleID);
                    limit = ToInt(await lim.ExecuteScalarAsync(cancellationToken) ?? 0);
                }
                int count;
                await using (var cnt = new SqlCommand("""
SELECT COUNT(*) FROM dbo.Income_PayOrder
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND StudentClassID = @SCID AND RoleID = @RoleID
""", con))
                {
                    cnt.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    cnt.Parameters.AddWithValue("@YearID", session.EducationYearID);
                    cnt.Parameters.AddWithValue("@SCID", studentClassId);
                    cnt.Parameters.AddWithValue("@RoleID", item.RoleID);
                    count = ToInt(await cnt.ExecuteScalarAsync(cancellationToken) ?? 0);
                }
                if (count >= limit)
                {
                    failed++;
                    continue;
                }
                await using var cmd = new SqlCommand("""
INSERT INTO dbo.Income_PayOrder
    (SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, AssignRoleID, EducationYearID,
     Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor, StartDate, EndDate, CreatedDate)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SCID, @Assign, @YearID,
     @Amount, 0, @LateFee, @Discount, 0, @RoleID, @PayFor, @Start, @End, GETDATE())
""", con);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@ClassID", classId);
                cmd.Parameters.AddWithValue("@SCID", studentClassId);
                cmd.Parameters.AddWithValue("@Assign", item.AssignRoleID > 0 ? item.AssignRoleID : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
                cmd.Parameters.AddWithValue("@Amount", item.Amount);
                cmd.Parameters.AddWithValue("@LateFee", item.LateFee);
                cmd.Parameters.AddWithValue("@Discount", item.Discount);
                cmd.Parameters.AddWithValue("@RoleID", item.RoleID);
                cmd.Parameters.AddWithValue("@PayFor", item.PayFor.Trim());
                cmd.Parameters.AddWithValue("@Start", item.StartDate.Date);
                cmd.Parameters.AddWithValue("@End", item.EndDate.Date);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                saved++;
            }
        }
        return saved == 0 ? Fail("acc.payorderNone") : Ok(saved: saved, failed: failed);
    }

    public async Task<IReadOnlyList<UnpaidPayOrderDto>> ListUnpaidAsync(
        SessionSnapshot session, int classId, int roleId, DateTime? endDate, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT po.PayOrderID, po.RoleID, po.StudentID, po.ClassID, s.ID, s.StudentsName,
       ISNULL(c.Class, N'') AS ClassName, r.Role, po.PayFor, po.Amount, po.StartDate, po.EndDate
FROM dbo.Income_PayOrder AS po
INNER JOIN dbo.Student AS s ON po.StudentID = s.StudentID
INNER JOIN dbo.Income_Roles AS r ON po.RoleID = r.RoleID
LEFT JOIN dbo.CreateClass AS c ON po.ClassID = c.ClassID
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.PaidAmount <= 0
  AND (@ClassID = 0 OR po.ClassID = @ClassID)
  AND (@RoleID = 0 OR po.RoleID = @RoleID)
  AND (@EndDate IS NULL OR po.EndDate <= @EndDate)
ORDER BY s.ID, po.EndDate
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@RoleID", roleId);
        cmd.Parameters.AddWithValue("@EndDate", (object?)endDate?.Date ?? DBNull.Value);
        var items = new List<UnpaidPayOrderDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new UnpaidPayOrderDto
            {
                PayOrderID = ToInt(reader["PayOrderID"]),
                RoleID = ToInt(reader["RoleID"]),
                StudentID = ToInt(reader["StudentID"]),
                ClassID = ToInt(reader["ClassID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                ClassName = NullString(reader["ClassName"]),
                Role = reader["Role"]?.ToString() ?? "",
                PayFor = reader["PayFor"]?.ToString() ?? "",
                Amount = ToDec(reader["Amount"]),
                StartDate = Convert.ToDateTime(reader["StartDate"]).Date,
                EndDate = Convert.ToDateTime(reader["EndDate"]).Date
            });
        }
        return items;
    }

    public async Task<AccountsResult> RemovePayOrdersAsync(SessionSnapshot session, RemovePayOrderRequest? request, CancellationToken cancellationToken)
    {
        var ids = request?.PayOrderIDs.Where(x => x > 0).Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return Fail("acc.needRows");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        var saved = 0;
        try
        {
            foreach (var id in ids)
            {
                await using (var disc = new SqlCommand("DELETE FROM dbo.Income_Discount_Record WHERE PayOrderID = @ID AND SchoolID = @SchoolID", con, tx))
                {
                    disc.Parameters.AddWithValue("@ID", id);
                    disc.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    try { await disc.ExecuteNonQueryAsync(cancellationToken); } catch (SqlException) { }
                }
                await using var cmd = new SqlCommand("""
DELETE FROM dbo.Income_PayOrder
WHERE PayOrderID = @ID AND SchoolID = @SchoolID AND PaidAmount <= 0
""", con, tx);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                saved += await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
        return saved == 0 ? Fail("acc.failed") : Ok(saved: saved);
    }

    public async Task<AccountsResult> ChangePayOrderDateAsync(SessionSnapshot session, ChangePayOrderDateRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.AssignRoleID <= 0)
            return Fail("acc.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        string payFor;
        int roleId, classId;
        await using (var find = new SqlCommand("""
SELECT RoleID, ClassID, PayFor FROM dbo.Income_Assign_Role
WHERE AssignRoleID = @ID AND SchoolID = @SchoolID
""", con))
        {
            find.Parameters.AddWithValue("@ID", request.AssignRoleID);
            find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await find.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return Fail("acc.failed");
            roleId = ToInt(reader["RoleID"]);
            classId = ToInt(reader["ClassID"]);
            payFor = reader["PayFor"]?.ToString() ?? "";
        }
        await using (var upd = new SqlCommand("""
UPDATE dbo.Income_Assign_Role SET StartDate = @Start, EndDate = @End
WHERE AssignRoleID = @ID AND SchoolID = @SchoolID
""", con))
        {
            upd.Parameters.AddWithValue("@Start", request.StartDate.Date);
            upd.Parameters.AddWithValue("@End", request.EndDate.Date);
            upd.Parameters.AddWithValue("@ID", request.AssignRoleID);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(cancellationToken);
        }
        await using var po = new SqlCommand("""
UPDATE dbo.Income_PayOrder
SET StartDate = @Start, EndDate = @End,
    Is_LateFeeAdded = CASE WHEN @End >= CAST(GETDATE() AS DATE) THEN 0 ELSE Is_LateFeeAdded END
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND RoleID = @RoleID AND ClassID = @ClassID AND PayFor = @PayFor
""", con);
        po.Parameters.AddWithValue("@Start", request.StartDate.Date);
        po.Parameters.AddWithValue("@End", request.EndDate.Date);
        po.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        po.Parameters.AddWithValue("@YearID", session.EducationYearID);
        po.Parameters.AddWithValue("@RoleID", roleId);
        po.Parameters.AddWithValue("@ClassID", classId);
        po.Parameters.AddWithValue("@PayFor", payFor);
        await po.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<IReadOnlyList<CashAccountDto>> ListCashAccountsAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT AccountID, AccountName, ISNULL(AccountBalance, 0) AS AccountBalance, ISNULL(Default_Status, N'') AS Default_Status
FROM dbo.Account
WHERE SchoolID = @SchoolID
ORDER BY CASE WHEN Default_Status = N'True' THEN 0 ELSE 1 END, AccountName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<CashAccountDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CashAccountDto
            {
                AccountID = ToInt(reader["AccountID"]),
                AccountName = reader["AccountName"]?.ToString() ?? "",
                Balance = ToDec(reader["AccountBalance"]),
                IsDefault = string.Equals(reader["Default_Status"]?.ToString(), "True", StringComparison.OrdinalIgnoreCase)
            });
        }
        return items;
    }

    public async Task<AccountsResult> CreateCashAccountAsync(SessionSnapshot session, SaveCashAccountRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.AccountName ?? "").Trim();
        if (name.Length == 0)
            return Fail("acc.needAccount");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Account (AccountName, RegistrationID, SchoolID)
VALUES (@Name, @RegistrationID, @SchoolID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        try
        {
            return Ok(Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)));
        }
        catch (SqlException)
        {
            return Fail("acc.accountExists");
        }
    }

    public async Task<AccountsResult> SetDefaultAccountAsync(SessionSnapshot session, int accountId, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var clear = new SqlCommand("UPDATE dbo.Account SET Default_Status = N'False' WHERE SchoolID = @SchoolID", con))
        {
            clear.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await clear.ExecuteNonQueryAsync(cancellationToken);
        }
        await using var cmd = new SqlCommand("UPDATE dbo.Account SET Default_Status = N'True' WHERE AccountID = @ID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@ID", accountId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0 ? Ok(accountId) : Fail("acc.failed");
    }

    public async Task<AccountsResult> DepositAsync(SessionSnapshot session, AccountMoveRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.AccountID <= 0 || request.Amount <= 0)
            return Fail("acc.needAmount");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.AccountIN_Record (AccountID, SchoolID, RegistrationID, AccountIN_Amount, IN_Details, EducationYearID, AccountIN_Date)
VALUES (@AccountID, @SchoolID, @RegistrationID, @Amount, @Details, @YearID, @Date)
""", con);
        cmd.Parameters.AddWithValue("@AccountID", request.AccountID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Details", (object?)request.Details ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Date", request.Date.Date);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<AccountsResult> WithdrawAsync(SessionSnapshot session, AccountMoveRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.AccountID <= 0 || request.Amount <= 0)
            return Fail("acc.needAmount");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        decimal balance;
        await using (var bal = new SqlCommand("SELECT ISNULL(AccountBalance, 0) FROM dbo.Account WHERE AccountID = @ID AND SchoolID = @SchoolID", con))
        {
            bal.Parameters.AddWithValue("@ID", request.AccountID);
            bal.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            balance = ToDec(await bal.ExecuteScalarAsync(cancellationToken) ?? 0);
        }
        if (request.Amount > balance)
            return Fail("acc.overBalance");
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.AccountOUT_Record (AccountID, SchoolID, RegistrationID, AccountOUT_Amount, Out_Details, EducationYearID, AccountOUT_Date)
VALUES (@AccountID, @SchoolID, @RegistrationID, @Amount, @Details, @YearID, @Date)
""", con);
        cmd.Parameters.AddWithValue("@AccountID", request.AccountID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Details", (object?)request.Details ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Date", request.Date.Date);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<IReadOnlyList<AccountMoveDto>> ListDepositsAsync(SessionSnapshot session, int accountId, CancellationToken cancellationToken) =>
        await ListMovesAsync(session, accountId, true, cancellationToken);

    public async Task<IReadOnlyList<AccountMoveDto>> ListWithdrawsAsync(SessionSnapshot session, int accountId, CancellationToken cancellationToken) =>
        await ListMovesAsync(session, accountId, false, cancellationToken);

    private async Task<IReadOnlyList<AccountMoveDto>> ListMovesAsync(SessionSnapshot session, int accountId, bool deposit, CancellationToken cancellationToken)
    {
        var sql = deposit
            ? """
SELECT AccountIN_ID AS Id, AccountIN_Date AS MoveDate, AccountIN_Amount AS Amount, IN_Details AS Details
FROM dbo.AccountIN_Record
WHERE SchoolID = @SchoolID AND AccountID = @AccountID AND EducationYearID = @YearID
ORDER BY AccountIN_Date DESC
"""
            : """
SELECT AccountOUT_ID AS Id, AccountOUT_Date AS MoveDate, AccountOUT_Amount AS Amount, Out_Details AS Details
FROM dbo.AccountOUT_Record
WHERE SchoolID = @SchoolID AND AccountID = @AccountID AND EducationYearID = @YearID
ORDER BY AccountOUT_Date DESC
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@AccountID", accountId);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        var items = new List<AccountMoveDto>();
        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new AccountMoveDto
                {
                    Id = ToInt(reader["Id"]),
                    Date = Convert.ToDateTime(reader["MoveDate"]).Date,
                    Amount = ToDec(reader["Amount"]),
                    Details = NullString(reader["Details"])
                });
            }
        }
        catch (SqlException)
        {
            return items;
        }
        return items;
    }

    public async Task<IReadOnlyList<FeeSuggestDto>> SuggestStudentsAsync(SessionSnapshot session, string? query, CancellationToken cancellationToken)
    {
        var code = (query ?? "").Trim();
        if (code.Length == 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT TOP 8 s.ID, s.StudentsName, ISNULL(c.Class, N'') AS ClassName
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
LEFT JOIN dbo.CreateClass AS c ON sc.ClassID = c.ClassID
WHERE s.Status = N'Active' AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @YearID
  AND s.ID LIKE @ID + N'%'
ORDER BY s.ID
""", con);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        var items = new List<FeeSuggestDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new FeeSuggestDto
            {
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["StudentsName"]?.ToString() ?? "",
                ClassName = NullString(reader["ClassName"])
            });
        }
        return items;
    }

    public async Task<FeeStudentBundleDto> GetStudentBundleAsync(SessionSnapshot session, string id, CancellationToken cancellationToken)
    {
        var bundle = new FeeStudentBundleDto();
        var code = (id ?? "").Trim();
        if (code.Length == 0)
            return bundle;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        bundle.Student = await ReadStudentAsync(con, session, code, cancellationToken);
        if (bundle.Student is null)
            return bundle;
        await EnsureInventoryFeePayOrdersAsync(con, session, bundle.Student.StudentID, cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT po.PayOrderID, po.RoleID, po.EducationYearID, r.Role, po.PayFor, ISNULL(y.EducationYear, N'') AS YearName,
       ISNULL(c.Class, N'') AS ClassName,
       po.Amount, po.LateFee, po.Discount, po.LateFee_Discount, po.PaidAmount, po.StartDate, po.EndDate
FROM dbo.Income_PayOrder AS po
INNER JOIN dbo.Income_Roles AS r ON po.RoleID = r.RoleID
LEFT JOIN dbo.Education_Year AS y ON po.EducationYearID = y.EducationYearID
LEFT JOIN dbo.CreateClass AS c ON po.ClassID = c.ClassID
WHERE po.SchoolID = @SchoolID AND po.StudentID = @StudentID AND po.Status = N'Due'
ORDER BY po.EndDate
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@StudentID", bundle.Student.StudentID);
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var end = Convert.ToDateTime(reader["EndDate"]).Date;
                var amount = ToDec(reader["Amount"]);
                var late = ToDec(reader["LateFee"]);
                var disc = ToDec(reader["Discount"]);
                var lateDisc = ToDec(reader["LateFee_Discount"]);
                var paid = ToDec(reader["PaidAmount"]);
                var due = DueOf(amount, late, disc, lateDisc, paid, end);
                if (due <= 0)
                    continue;
                var yearId = ToInt(reader["EducationYearID"]);
                var row = new DueRowDto
                {
                    PayOrderID = ToInt(reader["PayOrderID"]),
                    RoleID = ToInt(reader["RoleID"]),
                    EducationYearID = yearId,
                    Role = reader["Role"]?.ToString() ?? "",
                    PayFor = reader["PayFor"]?.ToString() ?? "",
                    YearName = NullString(reader["YearName"]),
                    ClassName = NullString(reader["ClassName"]),
                    Amount = amount,
                    LateFee = end < DateTime.Today ? late : 0,
                    StoredLateFee = late,
                    Discount = disc,
                    LateFeeDiscount = lateDisc,
                    PaidAmount = paid,
                    Due = due,
                    PayNow = 0,
                    Selected = false,
                    StartDate = Convert.ToDateTime(reader["StartDate"]).Date,
                    EndDate = end,
                    Overdue = end < DateTime.Today,
                    CurrentYear = yearId == session.EducationYearID
                };
                if (string.Equals(row.Role, InventoryService.SaleCategory, StringComparison.OrdinalIgnoreCase))
                    bundle.InventoryDues.Add(row);
                else if (row.CurrentYear)
                    bundle.CurrentDues.Add(row);
                else
                    bundle.OtherDues.Add(row);
            }
        }
        bundle.CurrentDue = bundle.CurrentDues.Sum(x => x.Due);
        await LoadBundleReceiptsAsync(con, session, bundle, cancellationToken);
        return bundle;
    }

    private static async Task EnsureInventoryFeePayOrdersAsync(
        SqlConnection con, SessionSnapshot session, int studentId, CancellationToken ct)
    {
        if (studentId <= 0) return;
        if (await ScalarIntAsync(con, "SELECT CASE WHEN COL_LENGTH(N'dbo.Inv_Sale', N'FeePayOrderID') IS NULL THEN 0 ELSE 1 END", ct) <= 0)
            return;
        var roleId = await ScalarIntAsync(con, """
SELECT TOP 1 RoleID FROM dbo.Income_Roles WHERE SchoolID = @SchoolID AND Role = @Role
""", ct, p =>
        {
            p.AddWithValue("@SchoolID", session.SchoolID);
            p.AddWithValue("@Role", InventoryService.SaleCategory);
        });
        if (roleId <= 0)
        {
            roleId = await ScalarIntAsync(con, """
INSERT INTO dbo.Income_Roles (SchoolID, RegistrationID, Role, NumberOfPay, Description, Date)
VALUES (@SchoolID, @RegistrationID, @Role, 1, N'Inventory due', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", ct, p =>
            {
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@Role", InventoryService.SaleCategory);
            });
        }

        var pending = new List<(int SaleId, decimal Due, string Invoice, DateTime Date, int YearId, int ClassId, int StudentClassId)>();
        await using (var find = new SqlCommand("""
SELECT s.SaleID, s.Total - ISNULL(s.PaidAmount, 0) AS Due,
       ISNULL(NULLIF(s.InvoiceNo, N''), N'S-' + CAST(s.SaleID AS nvarchar(20))) AS InvoiceNo,
       s.DocDate, s.EducationYearID, ISNULL(sc.ClassID, 0) AS ClassID, ISNULL(sc.StudentClassID, 0) AS StudentClassID
FROM dbo.Inv_Sale AS s
INNER JOIN dbo.Inv_Customer AS c ON c.CustomerID = s.CustomerID
LEFT JOIN dbo.StudentsClass AS sc ON sc.StudentID = c.StudentID AND sc.SchoolID = s.SchoolID
    AND sc.EducationYearID = s.EducationYearID AND sc.Class_Status IS NULL
WHERE s.SchoolID = @SchoolID AND c.StudentID = @StudentID
  AND ISNULL(s.FeePayOrderID, 0) = 0
  AND s.Total > ISNULL(s.PaidAmount, 0)
""", con))
        {
            find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            find.Parameters.AddWithValue("@StudentID", studentId);
            await using var reader = await find.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                pending.Add((
                    ToInt(reader["SaleID"]),
                    ToDec(reader["Due"]),
                    reader["InvoiceNo"]?.ToString() ?? "",
                    Convert.ToDateTime(reader["DocDate"]).Date,
                    ToInt(reader["EducationYearID"]),
                    ToInt(reader["ClassID"]),
                    ToInt(reader["StudentClassID"])));
            }
        }

        foreach (var row in pending)
        {
            var payOrderId = await ScalarIntAsync(con, """
INSERT INTO dbo.Income_PayOrder
    (SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, EducationYearID,
     Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor,
     StartDate, EndDate, CreatedDate, NumberOfPayment, Is_Active, Is_LateFeeAdded)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SCID, @YearID,
     @Amount, 0, 0, 0, 0, @RoleID, @PayFor,
     @Date, @Date, GETDATE(), 0, 1, 0);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", ct, p =>
            {
                p.AddWithValue("@SchoolID", session.SchoolID);
                p.AddWithValue("@RegistrationID", session.RegistrationID);
                p.AddWithValue("@StudentID", studentId);
                p.AddWithValue("@ClassID", row.ClassId > 0 ? row.ClassId : DBNull.Value);
                p.AddWithValue("@SCID", row.StudentClassId > 0 ? row.StudentClassId : DBNull.Value);
                p.AddWithValue("@YearID", row.YearId > 0 ? row.YearId : session.EducationYearID);
                p.AddWithValue("@Amount", (double)row.Due);
                p.AddWithValue("@RoleID", roleId);
                p.AddWithValue("@PayFor", row.Invoice);
                p.AddWithValue("@Date", row.Date);
            });
            if (payOrderId <= 0) continue;
            await using var upd = new SqlCommand("UPDATE dbo.Inv_Sale SET FeePayOrderID = @PID WHERE SaleID = @ID", con);
            upd.Parameters.AddWithValue("@PID", payOrderId);
            upd.Parameters.AddWithValue("@ID", row.SaleId);
            await upd.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task ApplyInventoryFeePaymentsAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, List<CollectPaymentItem> items, CancellationToken ct)
    {
        if (items.Count == 0) return;
        var rows = string.Join(", ", Enumerable.Range(0, items.Count).Select(i => $"(@PID{i}, @PA{i})"));
        await using var cmd = new SqlCommand($"""
IF COL_LENGTH(N'dbo.Inv_Sale', N'FeePayOrderID') IS NOT NULL
UPDATE s
SET PaidAmount = CASE WHEN s.PaidAmount + v.Paid > s.Total THEN s.Total ELSE s.PaidAmount + v.Paid END
FROM dbo.Inv_Sale AS s
INNER JOIN (VALUES {rows}) AS v(PayOrderID, Paid) ON s.FeePayOrderID = v.PayOrderID
WHERE s.SchoolID = @SchoolID
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        for (var i = 0; i < items.Count; i++)
        {
            cmd.Parameters.AddWithValue($"@PID{i}", items[i].PayOrderID);
            cmd.Parameters.AddWithValue($"@PA{i}", items[i].PaidAmount);
        }
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> ScalarIntAsync(
        SqlConnection con, string sql, CancellationToken ct, Action<SqlParameterCollection>? bind = null)
    {
        await using var cmd = new SqlCommand(sql, con);
        bind?.Invoke(cmd.Parameters);
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task LoadBundleReceiptsAsync(
        SqlConnection con, SessionSnapshot session, FeeStudentBundleDto bundle, CancellationToken cancellationToken)
    {
        var studentId = bundle.Student!.StudentID;
        try
        {
            await using (var rec = new SqlCommand("""
SELECT TOP 50 mr.MoneyReceiptID, CAST(mr.MoneyReceipt_SN AS nvarchar(20)) AS ReceiptNo, ISNULL(mr.TotalAmount, 0) AS TotalAmount,
       mr.PaidDate, ISNULL(mr.CollectionDate, mr.PaidDate) AS CollectionDate, mr.EducationYearID,
       ISNULL(y.EducationYear, N'') AS YearName, ISNULL(mr.PrintedReceiptNo, N'') AS PrintedReceiptNo
FROM dbo.Income_MoneyReceipt AS mr
LEFT JOIN dbo.Education_Year AS y ON mr.EducationYearID = y.EducationYearID
WHERE mr.SchoolID = @SchoolID AND mr.StudentID = @StudentID
  AND (
        mr.EducationYearID = @YearID
        OR EXISTS (
            SELECT 1 FROM dbo.Income_PaymentRecord AS pr
            WHERE pr.MoneyReceiptID = mr.MoneyReceiptID
              AND pr.SchoolID = @SchoolID
              AND pr.EducationYearID = @YearID
        )
  )
ORDER BY ISNULL(mr.CollectionDate, mr.PaidDate) DESC, mr.MoneyReceiptID DESC
""", con))
            {
                rec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                rec.Parameters.AddWithValue("@StudentID", studentId);
                rec.Parameters.AddWithValue("@YearID", session.EducationYearID);
                await using var recReader = await rec.ExecuteReaderAsync(cancellationToken);
                while (await recReader.ReadAsync(cancellationToken))
                    bundle.Receipts.Add(MapReceipt(recReader, true));
            }

            await using (var prev = new SqlCommand("""
SELECT TOP 50 mr.MoneyReceiptID, CAST(mr.MoneyReceipt_SN AS nvarchar(20)) AS ReceiptNo, ISNULL(mr.TotalAmount, 0) AS TotalAmount,
       mr.PaidDate, ISNULL(mr.CollectionDate, mr.PaidDate) AS CollectionDate, mr.EducationYearID,
       ISNULL(y.EducationYear, N'') AS YearName, ISNULL(mr.PrintedReceiptNo, N'') AS PrintedReceiptNo
FROM dbo.Income_MoneyReceipt AS mr
LEFT JOIN dbo.Education_Year AS y ON mr.EducationYearID = y.EducationYearID
WHERE mr.SchoolID = @SchoolID AND mr.StudentID = @StudentID
  AND mr.EducationYearID <> @YearID
  AND mr.EducationYearID = (
        SELECT MAX(x.EducationYearID) FROM dbo.Income_MoneyReceipt AS x
        WHERE x.StudentID = mr.StudentID AND x.SchoolID = @SchoolID AND x.EducationYearID <> @YearID
  )
  AND NOT EXISTS (
        SELECT 1 FROM dbo.Income_PaymentRecord AS pr
        WHERE pr.MoneyReceiptID = mr.MoneyReceiptID
          AND pr.SchoolID = @SchoolID
          AND pr.EducationYearID = @YearID
  )
ORDER BY ISNULL(mr.CollectionDate, mr.PaidDate) DESC, mr.MoneyReceiptID DESC
""", con))
            {
                prev.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                prev.Parameters.AddWithValue("@StudentID", studentId);
                prev.Parameters.AddWithValue("@YearID", session.EducationYearID);
                await using var prevReader = await prev.ExecuteReaderAsync(cancellationToken);
                while (await prevReader.ReadAsync(cancellationToken))
                    bundle.PreviousReceipts.Add(MapReceipt(prevReader, true));
            }
        }
        catch (SqlException)
        {
            await using var rec = new SqlCommand("""
SELECT TOP 50 MoneyReceiptID, CAST(MoneyReceipt_SN AS nvarchar(20)) AS ReceiptNo, ISNULL(TotalAmount, 0) AS TotalAmount,
       PaidDate, EducationYearID
FROM dbo.Income_MoneyReceipt
WHERE SchoolID = @SchoolID AND StudentID = @StudentID
  AND EducationYearID = @YearID
ORDER BY PaidDate DESC, MoneyReceiptID DESC
""", con);
            rec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            rec.Parameters.AddWithValue("@StudentID", studentId);
            rec.Parameters.AddWithValue("@YearID", session.EducationYearID);
            await using var recReader = await rec.ExecuteReaderAsync(cancellationToken);
            while (await recReader.ReadAsync(cancellationToken))
                bundle.Receipts.Add(MapReceipt(recReader, false));
        }
    }

    private static ReceiptListDto MapReceipt(SqlDataReader reader, bool hasStamp)
    {
        var paid = Convert.ToDateTime(reader["PaidDate"]);
        return new ReceiptListDto
        {
            MoneyReceiptID = ToInt(reader["MoneyReceiptID"]),
            ReceiptNo = reader["ReceiptNo"]?.ToString() ?? "",
            TotalAmount = ToDec(reader["TotalAmount"]),
            PaidDate = paid,
            CollectionDate = hasStamp ? Convert.ToDateTime(reader["CollectionDate"]) : paid,
            EducationYearID = ToInt(reader["EducationYearID"]),
            YearName = hasStamp ? NullString(reader["YearName"]) : null,
            PrintedReceiptNo = hasStamp ? NullString(reader["PrintedReceiptNo"]) : null
        };
    }

    private static async Task<FeeStudentDto?> ReadStudentAsync(
        SqlConnection con, SessionSnapshot session, string code, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 s.StudentID, sc.StudentClassID, sc.ClassID, sc.EducationYearID, s.ID, s.StudentsName,
       c.Class, sc.RollNo, s.SMSPhoneNo, s.FathersName, s.Status, ISNULL(y.EducationYear, N'') AS EducationYear,
       ISNULL(sec.Section, N'') AS Section, ISNULL(sh.Shift, N'') AS Shift
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
LEFT JOIN dbo.CreateClass AS c ON sc.ClassID = c.ClassID
LEFT JOIN dbo.Education_Year AS y ON sc.EducationYearID = y.EducationYearID
LEFT JOIN dbo.CreateSection AS sec ON sc.SectionID = sec.SectionID
LEFT JOIN dbo.CreateShift AS sh ON sc.ShiftID = sh.ShiftID
WHERE sc.SchoolID = @SchoolID AND sc.Class_Status IS NULL
  AND (s.ID = @ID OR s.ID = @Unpadded)
ORDER BY CASE WHEN sc.EducationYearID = @YearID THEN 0 ELSE 1 END,
         CASE WHEN s.ID = @ID THEN 0 ELSE 1 END
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ID", code);
        var unpadded = code.TrimStart('0');
        cmd.Parameters.AddWithValue("@Unpadded", unpadded.Length == 0 ? "0" : unpadded);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new FeeStudentDto
        {
            StudentID = ToInt(reader["StudentID"]),
            StudentClassID = ToInt(reader["StudentClassID"]),
            ClassID = ToInt(reader["ClassID"]),
            EducationYearID = ToInt(reader["EducationYearID"]),
            ID = reader["ID"]?.ToString() ?? "",
            Name = reader["StudentsName"]?.ToString() ?? "",
            ClassName = NullString(reader["Class"]),
            RollNo = NullString(reader["RollNo"]),
            Phone = NullString(reader["SMSPhoneNo"]),
            FathersName = NullString(reader["FathersName"]),
            Status = NullString(reader["Status"]),
            EducationYear = NullString(reader["EducationYear"]),
            Section = NullString(reader["Section"]),
            Shift = NullString(reader["Shift"])
        };
    }

    public async Task<AccountsResult> CollectAsync(SessionSnapshot session, CollectPaymentRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0 || request.AccountID <= 0)
            return Fail("acc.needPay");
        var items = request.Items.Where(x => x.PayOrderID > 0 && x.PaidAmount > 0).ToList();
        if (items.Count == 0)
            return Fail("acc.needRows");
        var paidDate = request.PaidDate ?? DateTime.Now;
        if (request.PaidDate is { } picked)
        {
            if (picked.Date > DateTime.Today)
                return Fail("acc.futureDate");
            if (picked.TimeOfDay == TimeSpan.Zero)
                paidDate = picked.Date.Add(DateTime.Now.TimeOfDay);
        }
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var opts = new SqlCommand("SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_WARNINGS ON; SET ANSI_PADDING ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON;", con, tx))
                await opts.ExecuteNonQueryAsync(cancellationToken);
            var payOrderIds = items.Select(x => x.PayOrderID).Distinct().ToList();
            var orders = await LoadPayOrdersBatchAsync(con, tx, session.SchoolID, payOrderIds, cancellationToken);
            foreach (var item in items)
            {
                if (!orders.TryGetValue(item.PayOrderID, out var due) || item.PaidAmount > due.Due)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("acc.overDue");
                }
            }

            int receiptId;
            var receiptNo = "";
            var receiptClassId = request.StudentClassID;
            foreach (var item in items)
            {
                var order = orders[item.PayOrderID];
                if (order.EducationYearID == session.EducationYearID && order.StudentClassID > 0)
                {
                    receiptClassId = order.StudentClassID;
                    break;
                }
            }
            if (receiptClassId <= 0)
            {
                foreach (var item in items)
                {
                    var order = orders[item.PayOrderID];
                    if (order.StudentClassID > 0)
                    {
                        receiptClassId = order.StudentClassID;
                        break;
                    }
                }
            }
            await using (var rec = new SqlCommand("""
DECLARE @Sn int;
SELECT @Sn = ISNULL(MAX(MoneyReceipt_SN), 100000)
FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK)
WHERE SchoolID = @SchoolID;
IF (@Sn < 100000) SET @Sn = 100000;
INSERT INTO dbo.Income_MoneyReceipt
    (StudentID, RegistrationID, StudentClassID, PaidDate, EducationYearID, PaymentBy, SchoolID, MoneyReceipt_SN, CollectionDate)
VALUES
    (@SID, @RID, @SCID, @PaidDate, @YearID, N'Institution', @SchoolID, @Sn + 1, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int), CAST(@Sn + 1 AS nvarchar(20));
""", con, tx))
            {
                rec.Parameters.AddWithValue("@SID", request.StudentID);
                rec.Parameters.AddWithValue("@RID", session.RegistrationID);
                rec.Parameters.AddWithValue("@SCID", receiptClassId);
                rec.Parameters.AddWithValue("@YearID", session.EducationYearID);
                rec.Parameters.AddWithValue("@PaidDate", paidDate);
                rec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await using var reader = await rec.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    await reader.CloseAsync();
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("acc.receiptFail");
                }
                receiptId = ToInt(reader[0]);
                receiptNo = reader[1]?.ToString() ?? "";
            }
            if (receiptId <= 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("acc.receiptFail");
            }

            decimal total = 0;
            await using (var ins = new SqlCommand(BuildPaymentInsertSql(items.Count), con, tx))
            {
                ins.Parameters.AddWithValue("@SID", request.StudentID);
                ins.Parameters.AddWithValue("@RID", session.RegistrationID);
                ins.Parameters.AddWithValue("@Date", paidDate);
                ins.Parameters.AddWithValue("@MID", receiptId);
                ins.Parameters.AddWithValue("@SchID", session.SchoolID);
                ins.Parameters.AddWithValue("@AccID", request.AccountID);
                for (var i = 0; i < items.Count; i++)
                {
                    var order = orders[items[i].PayOrderID];
                    ins.Parameters.AddWithValue($"@Role{i}", order.RoleID);
                    ins.Parameters.AddWithValue($"@PID{i}", items[i].PayOrderID);
                    ins.Parameters.AddWithValue($"@PA{i}", items[i].PaidAmount);
                    ins.Parameters.AddWithValue($"@PF{i}", order.PayFor);
                    ins.Parameters.AddWithValue($"@SCID{i}", order.StudentClassID);
                    ins.Parameters.AddWithValue($"@EID{i}", order.EducationYearID);
                    total += items[i].PaidAmount;
                }
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var upd = new SqlCommand(BuildPayOrderUpdateSql(items.Count), con, tx))
            {
                upd.Parameters.AddWithValue("@Date", paidDate);
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                for (var i = 0; i < items.Count; i++)
                {
                    upd.Parameters.AddWithValue($"@PID{i}", items[i].PayOrderID);
                    upd.Parameters.AddWithValue($"@PA{i}", items[i].PaidAmount);
                }
                await upd.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var tot = new SqlCommand("UPDATE dbo.Income_MoneyReceipt SET TotalAmount = @T WHERE MoneyReceiptID = @MID", con, tx))
            {
                tot.Parameters.AddWithValue("@T", total);
                tot.Parameters.AddWithValue("@MID", receiptId);
                await tot.ExecuteNonQueryAsync(cancellationToken);
            }
            await ApplyInventoryFeePaymentsAsync(con, tx, session.SchoolID, items, cancellationToken);
            await tx.CommitAsync(cancellationToken);

            if (request.SendSms)
            {
                var details = string.Concat(items.Select(x =>
                {
                    var order = orders[x.PayOrderID];
                    return $", {order.Role}: {order.PayFor}";
                }));
                _ = _sms.TrySendAfterCollectAsync(session, request.StudentID, "", "", "", total, receiptNo,
                    details, CancellationToken.None);
            }
            return new AccountsResult { Succeeded = true, Id = receiptId, Saved = items.Count, ReceiptNo = receiptNo };
        }
        catch (Exception ex)
        {
            try { await tx.RollbackAsync(cancellationToken); } catch { }
            return new AccountsResult { Error = string.IsNullOrWhiteSpace(ex.Message) ? "acc.failed" : ex.Message };
        }
    }

    private sealed class PayOrderPayInfo
    {
        public int RoleID { get; init; }
        public int EducationYearID { get; init; }
        public int StudentClassID { get; init; }
        public string PayFor { get; init; } = "";
        public string Role { get; init; } = "";
        public decimal Due { get; init; }
    }

    private static async Task<Dictionary<int, PayOrderPayInfo>> LoadPayOrdersBatchAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, IReadOnlyList<int> payOrderIds, CancellationToken cancellationToken)
    {
        var map = new Dictionary<int, PayOrderPayInfo>();
        if (payOrderIds.Count == 0)
            return map;
        var names = new List<string>(payOrderIds.Count);
        await using var cmd = new SqlCommand { Connection = con, Transaction = tx };
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        for (var i = 0; i < payOrderIds.Count; i++)
        {
            var name = "@p" + i;
            names.Add(name);
            cmd.Parameters.AddWithValue(name, payOrderIds[i]);
        }
        cmd.CommandText = $"""
SELECT po.PayOrderID, po.RoleID, po.PayFor, po.EducationYearID, po.StudentClassID,
       ISNULL(r.Role, N'') AS Role,
       po.Amount, po.LateFee, po.Discount, po.LateFee_Discount, po.PaidAmount, po.EndDate
FROM dbo.Income_PayOrder AS po
LEFT JOIN dbo.Income_Roles AS r ON po.RoleID = r.RoleID
WHERE po.SchoolID = @SchoolID AND po.PayOrderID IN ({string.Join(", ", names)})
""";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            map[ToInt(reader["PayOrderID"])] = new PayOrderPayInfo
            {
                RoleID = ToInt(reader["RoleID"]),
                EducationYearID = ToInt(reader["EducationYearID"]),
                StudentClassID = ToInt(reader["StudentClassID"]),
                PayFor = reader["PayFor"]?.ToString() ?? "",
                Role = reader["Role"]?.ToString() ?? "",
                Due = DueOf(
                    ToDec(reader["Amount"]),
                    ToDec(reader["LateFee"]),
                    ToDec(reader["Discount"]),
                    ToDec(reader["LateFee_Discount"]),
                    ToDec(reader["PaidAmount"]),
                    Convert.ToDateTime(reader["EndDate"]).Date)
            };
        }
        return map;
    }

    private static string BuildPaymentInsertSql(int count)
    {
        var rows = string.Join(", ", Enumerable.Range(0, count).Select(i =>
            $"(@SID, @RID, @Role{i}, @PID{i}, @PA{i}, @PF{i}, @Date, @MID, @SCID{i}, @EID{i}, @SchID, @AccID)"));
        return $"""
INSERT INTO dbo.Income_PaymentRecord
    (StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, PayFor, PaidDate, MoneyReceiptID, StudentClassID, EducationYearID, SchoolID, AccountID)
VALUES {rows}
""";
    }

    private static string BuildPayOrderUpdateSql(int count)
    {
        var rows = string.Join(", ", Enumerable.Range(0, count).Select(i => $"(@PID{i}, @PA{i})"));
        return $"""
UPDATE po
SET PaidAmount = po.PaidAmount + v.Paid,
    LastPaidDate = @Date,
    NumberOfPayment = po.NumberOfPayment + 1,
    Is_LateFeeAdded = CASE WHEN po.EndDate < GETDATE() AND ISNULL(po.LateFee, 0) > 0 THEN 1 ELSE po.Is_LateFeeAdded END
FROM dbo.Income_PayOrder AS po
INNER JOIN (VALUES {rows}) AS v(PayOrderID, Paid) ON po.PayOrderID = v.PayOrderID
WHERE po.SchoolID = @SchoolID
""";
    }

    private static async Task<Dictionary<int, decimal>> LoadDuesBatchAsync(
        SqlConnection con, SqlTransaction tx, IReadOnlyList<int> payOrderIds, CancellationToken cancellationToken)
    {
        var dues = new Dictionary<int, decimal>();
        if (payOrderIds.Count == 0)
            return dues;
        var names = new List<string>(payOrderIds.Count);
        await using var cmd = new SqlCommand { Connection = con, Transaction = tx };
        for (var i = 0; i < payOrderIds.Count; i++)
        {
            var name = "@p" + i;
            names.Add(name);
            cmd.Parameters.AddWithValue(name, payOrderIds[i]);
        }
        cmd.CommandText = $"""
SELECT PayOrderID, Amount, LateFee, Discount, LateFee_Discount, PaidAmount, EndDate
FROM dbo.Income_PayOrder WHERE PayOrderID IN ({string.Join(", ", names)})
""";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = ToInt(reader["PayOrderID"]);
            dues[id] = DueOf(
                ToDec(reader["Amount"]),
                ToDec(reader["LateFee"]),
                ToDec(reader["Discount"]),
                ToDec(reader["LateFee_Discount"]),
                ToDec(reader["PaidAmount"]),
                Convert.ToDateTime(reader["EndDate"]).Date);
        }
        return dues;
    }

    private static async Task<decimal> GetDueAsync(SqlConnection con, SqlTransaction tx, int payOrderId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT Amount, LateFee, Discount, LateFee_Discount, PaidAmount, EndDate
FROM dbo.Income_PayOrder WHERE PayOrderID = @ID
""", con, tx);
        cmd.Parameters.AddWithValue("@ID", payOrderId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return 0;
        return DueOf(
            ToDec(reader["Amount"]),
            ToDec(reader["LateFee"]),
            ToDec(reader["Discount"]),
            ToDec(reader["LateFee_Discount"]),
            ToDec(reader["PaidAmount"]),
            Convert.ToDateTime(reader["EndDate"]).Date);
    }

    public async Task<AccountsResult> AddMoreAsync(SessionSnapshot session, AddMorePayOrderRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentID <= 0 || request.RoleID <= 0 || request.Amount <= 0)
            return Fail("acc.needAssign");
        var payFor = string.IsNullOrWhiteSpace(request.PayFor) ? DateTime.Today.ToString("MMMM") : request.PayFor.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Income_PayOrder
    (SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, EducationYearID,
     Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor, StartDate, EndDate, CreatedDate)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SCID, @YearID,
     @Amount, 0, 0, @Discount, 0, @RoleID, @PayFor, GETDATE(), GETDATE(), GETDATE())
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@StudentID", request.StudentID);
        cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
        cmd.Parameters.AddWithValue("@SCID", request.StudentClassID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Discount", request.Discount < 0 ? 0 : request.Discount);
        cmd.Parameters.AddWithValue("@RoleID", request.RoleID);
        cmd.Parameters.AddWithValue("@PayFor", payFor);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Ok();
    }

    public async Task<AccountsResult> SaveConcessionAsync(SessionSnapshot session, SaveConcessionRequest? request, CancellationToken cancellationToken)
    {
        var items = request?.Items ?? [];
        if (items.Count == 0)
            return Fail("acc.needRows");
        var reason = string.IsNullOrWhiteSpace(request?.Reason) ? "Concession" : request!.Reason!.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var saved = 0;
        foreach (var item in items)
        {
            decimal oldDisc, oldLate, amount, late, paid;
            int studentId, studentClassId, yearId;
            await using (var find = new SqlCommand("""
SELECT Discount, LateFee_Discount, Amount, LateFee, PaidAmount, StudentID, StudentClassID, EducationYearID
FROM dbo.Income_PayOrder WHERE PayOrderID = @ID AND SchoolID = @SchoolID
""", con))
            {
                find.Parameters.AddWithValue("@ID", item.PayOrderID);
                find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await using var reader = await find.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    continue;
                oldDisc = ToDec(reader["Discount"]);
                oldLate = ToDec(reader["LateFee_Discount"]);
                amount = ToDec(reader["Amount"]);
                late = ToDec(reader["LateFee"]);
                paid = ToDec(reader["PaidAmount"]);
                studentId = ToInt(reader["StudentID"]);
                studentClassId = ToInt(reader["StudentClassID"]);
                yearId = ToInt(reader["EducationYearID"]);
            }
            if (item.Discount > amount || item.LateFeeDiscount > late)
                return Fail("acc.overConcession");
            if (item.Discount + item.LateFeeDiscount + paid > amount + late)
                return Fail("acc.overConcession");
            if (item.Discount != oldDisc)
            {
                await using var disc = new SqlCommand("""
INSERT INTO dbo.Income_Discount_Record
    (SchoolID, RegistrationID, EducationYearID, StudentID, PayOrderID, Reason, PreviousAmount, PostAmount, Date, StudentClassID)
VALUES
    (@SchoolID, @RegistrationID, @YearID, @StudentID, @PID, @Reason, @Prev, @Post, GETDATE(), @SCID)
""", con);
                disc.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                disc.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                disc.Parameters.AddWithValue("@YearID", yearId);
                disc.Parameters.AddWithValue("@StudentID", studentId);
                disc.Parameters.AddWithValue("@PID", item.PayOrderID);
                disc.Parameters.AddWithValue("@Reason", reason);
                disc.Parameters.AddWithValue("@Prev", oldDisc);
                disc.Parameters.AddWithValue("@Post", item.Discount);
                disc.Parameters.AddWithValue("@SCID", studentClassId);
                await disc.ExecuteNonQueryAsync(cancellationToken);
            }
            if (item.LateFeeDiscount != oldLate)
            {
                await using var lateCmd = new SqlCommand("""
INSERT INTO dbo.Income_LateFee_Discount_Record
    (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, PayOrderID, PreviousAmount, PostAmount, Date, Reason)
VALUES
    (@SchoolID, @RegistrationID, @YearID, @StudentID, @SCID, @PID, @Prev, @Post, GETDATE(), @Reason)
""", con);
                lateCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                lateCmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                lateCmd.Parameters.AddWithValue("@YearID", yearId);
                lateCmd.Parameters.AddWithValue("@StudentID", studentId);
                lateCmd.Parameters.AddWithValue("@SCID", studentClassId);
                lateCmd.Parameters.AddWithValue("@PID", item.PayOrderID);
                lateCmd.Parameters.AddWithValue("@Prev", oldLate);
                lateCmd.Parameters.AddWithValue("@Post", item.LateFeeDiscount);
                lateCmd.Parameters.AddWithValue("@Reason", reason);
                await lateCmd.ExecuteNonQueryAsync(cancellationToken);
            }
            await using var upd = new SqlCommand("""
UPDATE dbo.Income_PayOrder
SET Discount = @Discount, LateFee_Discount = @LateDisc,
    LateFee = CASE WHEN @SetLate = 1 THEN @LateFee ELSE LateFee END
WHERE PayOrderID = @ID AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@Discount", item.Discount);
            upd.Parameters.AddWithValue("@LateDisc", item.LateFeeDiscount);
            upd.Parameters.AddWithValue("@SetLate", item.SetLateFee ? 1 : 0);
            upd.Parameters.AddWithValue("@LateFee", item.LateFee);
            upd.Parameters.AddWithValue("@ID", item.PayOrderID);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            saved += await upd.ExecuteNonQueryAsync(cancellationToken);
        }
        return saved == 0 ? Fail("acc.failed") : Ok(saved: saved);
    }

    public async Task<ReceiptDetailDto?> GetReceiptAsync(SessionSnapshot session, string receiptNo, CancellationToken cancellationToken)
    {
        var sn = (receiptNo ?? "").Trim();
        if (sn.Length == 0)
            return null;
        var unpadded = sn.TrimStart('0');
        if (unpadded.Length == 0)
            unpadded = "0";
        int.TryParse(unpadded, out var snInt);
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT TOP 1 mr.MoneyReceiptID, CAST(mr.MoneyReceipt_SN AS nvarchar(20)) AS ReceiptNo, mr.PaidDate,
       ISNULL(mr.CollectionDate, mr.PaidDate) AS CollectionDate, ISNULL(mr.TotalAmount, 0) AS TotalAmount,
       mr.StudentID, mr.StudentClassID, mr.EducationYearID, ISNULL(mr.PrintedReceiptNo, N'') AS PrintedReceiptNo,
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS ReceivedBy,
       (
           SELECT TOP 1 acc.AccountName
           FROM dbo.Income_PaymentRecord AS pr
           INNER JOIN dbo.Account AS acc ON pr.AccountID = acc.AccountID
           WHERE pr.MoneyReceiptID = mr.MoneyReceiptID AND pr.SchoolID = mr.SchoolID
       ) AS AccountName
FROM dbo.Income_MoneyReceipt AS mr
LEFT JOIN dbo.Admin AS a ON mr.RegistrationID = a.RegistrationID
WHERE mr.SchoolID = @SchoolID
  AND (
        (@SNInt > 0 AND mr.MoneyReceipt_SN = @SNInt)
     OR (@SNInt > 0 AND mr.MoneyReceiptID = @SNInt)
     OR CAST(mr.MoneyReceipt_SN AS nvarchar(20)) = @SN
     OR CAST(mr.MoneyReceiptID AS nvarchar(20)) = @SN
     OR CAST(mr.MoneyReceipt_SN AS nvarchar(20)) = @Unpadded
     OR CAST(mr.MoneyReceiptID AS nvarchar(20)) = @Unpadded
     OR LTRIM(RTRIM(ISNULL(mr.PrintedReceiptNo, N''))) IN (@SN, @Unpadded)
  )
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@SN", sn);
        cmd.Parameters.AddWithValue("@Unpadded", unpadded);
        cmd.Parameters.AddWithValue("@SNInt", snInt);
        ReceiptDetailDto? dto = null;
        int studentId = 0, studentClassId = 0, receiptClassId = 0;
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            studentId = ToInt(reader["StudentID"]);
            studentClassId = ToInt(reader["StudentClassID"]);
            receiptClassId = studentClassId;
            dto = new ReceiptDetailDto
            {
                MoneyReceiptID = ToInt(reader["MoneyReceiptID"]),
                ReceiptNo = reader["ReceiptNo"]?.ToString() ?? "",
                PaidDate = Convert.ToDateTime(reader["PaidDate"]),
                CollectionDate = Convert.ToDateTime(reader["CollectionDate"]),
                TotalAmount = ToDec(reader["TotalAmount"]),
                ReceivedBy = NullString(reader["ReceivedBy"]),
                AccountName = NullString(reader["AccountName"]),
                PrintedReceiptNo = NullString(reader["PrintedReceiptNo"])
            };
        }
        await using (var stu = new SqlCommand("""
;WITH chain AS (
    SELECT sc.StudentClassID, sc.ClassID, sc.SectionID, sc.RollNo, sc.EducationYearID, sc.[Date], 0 AS hops
    FROM dbo.StudentsClass AS sc
    WHERE sc.StudentClassID = @SCID AND sc.StudentID = @SID AND sc.SchoolID = @SchoolID
    UNION ALL
    SELECT prev.StudentClassID, prev.ClassID, prev.SectionID, prev.RollNo, prev.EducationYearID, prev.[Date], c.hops + 1
    FROM chain AS c
    INNER JOIN dbo.StudentsClass AS prev
        ON prev.New_StudentClassID = c.StudentClassID AND prev.StudentID = @SID AND prev.SchoolID = @SchoolID
    WHERE @PaidDate < ISNULL(c.[Date], @PaidDate) AND c.hops < 8
)
SELECT TOP 1 s.StudentID, ch.StudentClassID, ch.ClassID, ch.EducationYearID, s.ID, s.StudentsName,
       c.Class, ch.RollNo, s.SMSPhoneNo, s.FathersName, ISNULL(sec.Section, N'') AS Section
FROM chain AS ch
INNER JOIN dbo.Student AS s ON s.StudentID = @SID
LEFT JOIN dbo.CreateClass AS c ON ch.ClassID = c.ClassID
LEFT JOIN dbo.CreateSection AS sec ON ch.SectionID = sec.SectionID
ORDER BY ch.hops DESC
OPTION (MAXRECURSION 8)
""", con))
        {
            stu.Parameters.AddWithValue("@SID", studentId);
            stu.Parameters.AddWithValue("@SCID", studentClassId);
            stu.Parameters.AddWithValue("@PaidDate", dto!.PaidDate);
            stu.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await stu.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                dto!.Student = new FeeStudentDto
                {
                    StudentID = ToInt(reader["StudentID"]),
                    StudentClassID = ToInt(reader["StudentClassID"]),
                    ClassID = ToInt(reader["ClassID"]),
                    EducationYearID = ToInt(reader["EducationYearID"]),
                    ID = reader["ID"]?.ToString() ?? "",
                    Name = reader["StudentsName"]?.ToString() ?? "",
                    ClassName = NullString(reader["Class"]),
                    RollNo = NullString(reader["RollNo"]),
                    Phone = NullString(reader["SMSPhoneNo"]),
                    FathersName = NullString(reader["FathersName"]),
                    Section = NullString(reader["Section"])
                };
            }
        }
        if (dto.Student is null)
        {
            await using var fallback = new SqlCommand("""
SELECT TOP 1 s.StudentID, sc.StudentClassID, sc.ClassID, sc.EducationYearID, s.ID, s.StudentsName,
       c.Class, sc.RollNo, s.SMSPhoneNo, s.FathersName, ISNULL(sec.Section, N'') AS Section
FROM dbo.Student AS s
INNER JOIN dbo.StudentsClass AS sc ON s.StudentID = sc.StudentID
LEFT JOIN dbo.CreateClass AS c ON sc.ClassID = c.ClassID
LEFT JOIN dbo.CreateSection AS sec ON sc.SectionID = sec.SectionID
WHERE s.StudentID = @SID AND s.SchoolID = @SchoolID
ORDER BY CASE WHEN sc.StudentClassID = @SCID THEN 0 ELSE 1 END
""", con);
            fallback.Parameters.AddWithValue("@SID", studentId);
            fallback.Parameters.AddWithValue("@SCID", studentClassId);
            fallback.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var fbReader = await fallback.ExecuteReaderAsync(cancellationToken);
            if (await fbReader.ReadAsync(cancellationToken))
            {
                dto.Student = new FeeStudentDto
                {
                    StudentID = ToInt(fbReader["StudentID"]),
                    StudentClassID = ToInt(fbReader["StudentClassID"]),
                    ClassID = ToInt(fbReader["ClassID"]),
                    EducationYearID = ToInt(fbReader["EducationYearID"]),
                    ID = fbReader["ID"]?.ToString() ?? "",
                    Name = fbReader["StudentsName"]?.ToString() ?? "",
                    ClassName = NullString(fbReader["Class"]),
                    RollNo = NullString(fbReader["RollNo"]),
                    Phone = NullString(fbReader["SMSPhoneNo"]),
                    FathersName = NullString(fbReader["FathersName"]),
                    Section = NullString(fbReader["Section"])
                };
            }
        }
        if (dto.Student is not null && dto.Student.StudentClassID != receiptClassId)
        {
            await using var fix = new SqlCommand("""
UPDATE mr SET StudentClassID = old.StudentClassID
FROM dbo.Income_MoneyReceipt AS mr
INNER JOIN dbo.StudentsClass AS cur
    ON mr.StudentClassID = cur.StudentClassID AND cur.StudentID = mr.StudentID AND cur.SchoolID = mr.SchoolID
INNER JOIN dbo.StudentsClass AS old
    ON old.New_StudentClassID = cur.StudentClassID AND old.StudentID = mr.StudentID AND old.SchoolID = mr.SchoolID
WHERE mr.StudentID = @SID AND mr.SchoolID = @SchoolID
  AND mr.PaidDate < ISNULL(cur.[Date], mr.PaidDate);
UPDATE pr SET StudentClassID = old.StudentClassID
FROM dbo.Income_PaymentRecord AS pr
INNER JOIN dbo.StudentsClass AS cur
    ON pr.StudentClassID = cur.StudentClassID AND cur.StudentID = pr.StudentID AND cur.SchoolID = pr.SchoolID
INNER JOIN dbo.StudentsClass AS old
    ON old.New_StudentClassID = cur.StudentClassID AND old.StudentID = pr.StudentID AND old.SchoolID = pr.SchoolID
WHERE pr.StudentID = @SID AND pr.SchoolID = @SchoolID
  AND pr.PaidDate < ISNULL(cur.[Date], pr.PaidDate);
""", con);
            fix.Parameters.AddWithValue("@SID", studentId);
            fix.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await fix.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (var lines = new SqlCommand("""
SELECT pr.PayOrderID, r.Role, pr.PayFor, ISNULL(y.EducationYear, N'') AS YearName, pr.PaidAmount,
       ISNULL(po.Amount, 0) AS Amount,
       ISNULL(po.Discount, 0) + ISNULL(po.LateFee_Discount, 0) AS Discount,
       CASE
           WHEN po.EndDate < DATEADD(DAY, -1, GETDATE())
           THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
           ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0)
       END AS Due
FROM dbo.Income_PaymentRecord AS pr
INNER JOIN dbo.Income_Roles AS r ON pr.RoleID = r.RoleID
LEFT JOIN dbo.Income_PayOrder AS po ON pr.PayOrderID = po.PayOrderID
LEFT JOIN dbo.Education_Year AS y ON pr.EducationYearID = y.EducationYearID
WHERE pr.SchoolID = @SchoolID AND pr.MoneyReceiptID = @MID
ORDER BY po.EndDate
""", con))
        {
            lines.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            lines.Parameters.AddWithValue("@MID", dto!.MoneyReceiptID);
            await using var lineReader = await lines.ExecuteReaderAsync(cancellationToken);
            while (await lineReader.ReadAsync(cancellationToken))
            {
                dto.Lines.Add(new ReceiptLineDto
                {
                    PayOrderID = ToInt(lineReader["PayOrderID"]),
                    Role = lineReader["Role"]?.ToString() ?? "",
                    PayFor = lineReader["PayFor"]?.ToString() ?? "",
                    YearName = NullString(lineReader["YearName"]),
                    Amount = ToDec(lineReader["Amount"]),
                    Discount = ToDec(lineReader["Discount"]),
                    PaidAmount = ToDec(lineReader["PaidAmount"]),
                    Due = ToDec(lineReader["Due"])
                });
            }
        }
        if (studentId > 0)
        {
            await using var dues = new SqlCommand("""
SELECT r.Role, po.PayFor, ISNULL(y.EducationYear, N'') AS YearName, po.EndDate, po.PaidAmount,
       po.Amount, po.LateFee, po.Discount, po.LateFee_Discount
FROM dbo.Income_PayOrder AS po
INNER JOIN dbo.Income_Roles AS r ON po.RoleID = r.RoleID
LEFT JOIN dbo.Education_Year AS y ON po.EducationYearID = y.EducationYearID
WHERE po.SchoolID = @SchoolID AND po.StudentID = @SID AND po.Status = N'Due'
  AND po.EndDate < GETDATE()
ORDER BY po.EndDate
""", con);
            dues.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            dues.Parameters.AddWithValue("@SID", studentId);
            await using var dueReader = await dues.ExecuteReaderAsync(cancellationToken);
            while (await dueReader.ReadAsync(cancellationToken))
            {
                var end = Convert.ToDateTime(dueReader["EndDate"]).Date;
                var due = DueOf(
                    ToDec(dueReader["Amount"]),
                    ToDec(dueReader["LateFee"]),
                    ToDec(dueReader["Discount"]),
                    ToDec(dueReader["LateFee_Discount"]),
                    ToDec(dueReader["PaidAmount"]),
                    end);
                if (due <= 0)
                    continue;
                dto.RemainingDues.Add(new ReceiptDueLineDto
                {
                    Role = dueReader["Role"]?.ToString() ?? "",
                    PayFor = dueReader["PayFor"]?.ToString() ?? "",
                    YearName = NullString(dueReader["YearName"]),
                    EndDate = end,
                    PaidAmount = ToDec(dueReader["PaidAmount"]),
                    Due = due
                });
            }
        }
        return dto;
    }

    public async Task<AccountsResult> UpdatePrintedReceiptAsync(SessionSnapshot session, PrintedReceiptRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.MoneyReceiptID <= 0)
            return Fail("acc.needReceipt");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Income_MoneyReceipt
SET PrintedReceiptNo = @No
WHERE MoneyReceiptID = @MID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@No", (object?)request.PrintedReceiptNo?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MID", request.MoneyReceiptID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) == 0 ? Fail("acc.failed") : Ok(request.MoneyReceiptID);
    }

    public async Task<AccountsResult> UnpaidReceiptAsync(SessionSnapshot session, int moneyReceiptId, CancellationToken cancellationToken)
    {
        if (moneyReceiptId <= 0)
            return Fail("acc.needReceipt");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var ctx = new SqlCommand("SET CONTEXT_INFO @Ctx", con, tx))
            {
                var bytes = BitConverter.GetBytes(session.RegistrationID);
                ctx.Parameters.Add("@Ctx", SqlDbType.VarBinary, 128).Value = bytes;
                await ctx.ExecuteNonQueryAsync(cancellationToken);
            }
            await using (var upd = new SqlCommand("""
UPDATE po
SET po.PaidAmount = po.PaidAmount - pr.PaidAmount,
    po.NumberOfPayment = 0,
    po.LastPaidDate = NULL
FROM dbo.Income_PayOrder AS po
INNER JOIN dbo.Income_PaymentRecord AS pr ON po.PayOrderID = pr.PayOrderID
WHERE pr.MoneyReceiptID = @MID AND pr.SchoolID = @SchoolID
""", con, tx))
            {
                upd.Parameters.AddWithValue("@MID", moneyReceiptId);
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await upd.ExecuteNonQueryAsync(cancellationToken);
            }
            await using (var delPay = new SqlCommand("DELETE FROM dbo.Income_PaymentRecord WHERE MoneyReceiptID = @MID AND SchoolID = @SchoolID", con, tx))
            {
                delPay.Parameters.AddWithValue("@MID", moneyReceiptId);
                delPay.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await delPay.ExecuteNonQueryAsync(cancellationToken);
            }
            await using (var delMr = new SqlCommand("DELETE FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID = @MID AND SchoolID = @SchoolID", con, tx))
            {
                delMr.Parameters.AddWithValue("@MID", moneyReceiptId);
                delMr.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                if (await delMr.ExecuteNonQueryAsync(cancellationToken) == 0)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return Fail("acc.failed");
                }
            }
            await tx.CommitAsync(cancellationToken);
            return Ok(moneyReceiptId);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<ExtraIncomeCategoryDto>> ListExtraCategoriesAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT Extra_IncomeCategoryID, Extra_Income_CategoryName
FROM dbo.Extra_IncomeCategory
WHERE SchoolID = @SchoolID
ORDER BY Extra_Income_CategoryName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<ExtraIncomeCategoryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ExtraIncomeCategoryDto
            {
                ExtraIncomeCategoryID = ToInt(reader["Extra_IncomeCategoryID"]),
                Name = reader["Extra_Income_CategoryName"]?.ToString() ?? ""
            });
        }
        return items;
    }

    public async Task<AccountsResult> CreateExtraCategoryAsync(SessionSnapshot session, string name, CancellationToken cancellationToken)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0)
            return Fail("acc.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var check = new SqlCommand("""
SELECT Extra_IncomeCategoryID FROM dbo.Extra_IncomeCategory
WHERE SchoolID = @SchoolID AND Extra_Income_CategoryName = @Name
""", con))
        {
            check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            check.Parameters.AddWithValue("@Name", name);
            if (await check.ExecuteScalarAsync(cancellationToken) is not null)
                return Fail("acc.categoryExists");
        }
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Extra_IncomeCategory (SchoolID, RegistrationID, Extra_Income_CategoryName)
VALUES (@SchoolID, @RegistrationID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Name", name);
        return Ok(Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)));
    }

    public async Task<(IReadOnlyList<ExtraIncomeDto> Items, decimal Total)> ListExtraIncomeAsync(
        SessionSnapshot session, int categoryId, DateTime? from, DateTime? to, string? receiptNo, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT e.Extra_IncomeID, e.Extra_IncomeCategoryID, c.Extra_Income_CategoryName, e.Extra_IncomeFor,
       e.Extra_IncomeAmount, e.Extra_IncomeDate
FROM dbo.Extra_Income AS e
INNER JOIN dbo.Extra_IncomeCategory AS c ON e.Extra_IncomeCategoryID = c.Extra_IncomeCategoryID
WHERE e.SchoolID = @SchoolID AND e.EducationYearID = @YearID
  AND (@Cat = 0 OR e.Extra_IncomeCategoryID = @Cat)
  AND (@From IS NULL OR CAST(e.Extra_IncomeDate AS DATE) >= @From)
  AND (@To IS NULL OR CAST(e.Extra_IncomeDate AS DATE) <= @To)
  AND (CAST(e.Extra_IncomeID AS nvarchar(20)) LIKE @Rid)
ORDER BY e.Extra_IncomeDate DESC, e.Extra_IncomeID DESC
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Cat", categoryId);
        var fromP = cmd.Parameters.Add("@From", SqlDbType.Date);
        fromP.Value = from?.Date ?? (object)DBNull.Value;
        var toP = cmd.Parameters.Add("@To", SqlDbType.Date);
        toP.Value = to?.Date ?? (object)DBNull.Value;
        cmd.Parameters.AddWithValue("@Rid", string.IsNullOrWhiteSpace(receiptNo) ? "%" : receiptNo.Trim());
        var items = new List<ExtraIncomeDto>();
        decimal total = 0;
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var amount = ToDec(reader["Extra_IncomeAmount"]);
            total += amount;
            items.Add(new ExtraIncomeDto
            {
                ExtraIncomeID = ToInt(reader["Extra_IncomeID"]),
                ExtraIncomeCategoryID = ToInt(reader["Extra_IncomeCategoryID"]),
                Category = reader["Extra_Income_CategoryName"]?.ToString() ?? "",
                Details = NullString(reader["Extra_IncomeFor"]),
                Amount = amount,
                Date = Convert.ToDateTime(reader["Extra_IncomeDate"]).Date
            });
        }
        return (items, total);
    }

    public async Task<AccountsResult> CreateExtraIncomeAsync(SessionSnapshot session, SaveExtraIncomeRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ExtraIncomeCategoryID <= 0 || request.Amount <= 0 || request.AccountID <= 0)
            return Fail("acc.needAmount");
        var paid = request.Date == default ? DateTime.Today : request.Date.Date;
        if (paid > DateTime.Today)
            return Fail("acc.futureDate");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Extra_Income
    (SchoolID, RegistrationID, Extra_IncomeCategoryID, Extra_IncomeAmount, Extra_IncomeFor, AccountID, EducationYearID, Extra_IncomeDate)
VALUES
    (@SchoolID, @RegistrationID, @Cat, @Amount, @Details, @AccountID, @YearID, @Date);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Cat", request.ExtraIncomeCategoryID);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Details", (object?)request.Details ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AccountID", request.AccountID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@Date", paid);
        return Ok(Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)));
    }

    public async Task<AccountsResult> UpdateExtraIncomeAsync(SessionSnapshot session, SaveExtraIncomeRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ExtraIncomeID <= 0 || request.Amount <= 0)
            return Fail("acc.needAmount");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await SetContextAsync(con, session.RegistrationID, cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Extra_Income
SET Extra_IncomeAmount = @Amount, Extra_IncomeFor = @Details
WHERE Extra_IncomeID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Details", (object?)request.Details ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID", request.ExtraIncomeID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0 ? Ok(request.ExtraIncomeID) : Fail("acc.empty");
    }

    public async Task<AccountsResult> DeleteExtraIncomeAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("acc.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await SetContextAsync(con, session.RegistrationID, cancellationToken);
        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.Extra_Income WHERE Extra_IncomeID = @ID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0 ? Ok(id) : Fail("acc.empty");
    }

    public async Task<AccountsResult> UpdateExtraCategoryAsync(SessionSnapshot session, SaveExtraCategoryRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        if (request is null || request.ExtraIncomeCategoryID <= 0 || name.Length == 0)
            return Fail("acc.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var check = new SqlCommand("""
SELECT Extra_IncomeCategoryID FROM dbo.Extra_IncomeCategory
WHERE SchoolID = @SchoolID AND Extra_Income_CategoryName = @Name AND Extra_IncomeCategoryID <> @ID
""", con))
        {
            check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            check.Parameters.AddWithValue("@Name", name);
            check.Parameters.AddWithValue("@ID", request.ExtraIncomeCategoryID);
            if (await check.ExecuteScalarAsync(cancellationToken) is not null)
                return Fail("acc.categoryExists");
        }
        await using var cmd = new SqlCommand("""
UPDATE dbo.Extra_IncomeCategory SET Extra_Income_CategoryName = @Name
WHERE Extra_IncomeCategoryID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@ID", request.ExtraIncomeCategoryID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0 ? Ok(request.ExtraIncomeCategoryID) : Fail("acc.empty");
    }

    public async Task<AccountsResult> DeleteExtraCategoryAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("acc.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand(
                "DELETE FROM dbo.Extra_IncomeCategory WHERE Extra_IncomeCategoryID = @ID AND SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return n > 0 ? Ok(id) : Fail("acc.empty");
        }
        catch (SqlException)
        {
            return Fail("acc.categoryUsed");
        }
    }

    private static async Task SetContextAsync(SqlConnection con, int registrationId, CancellationToken cancellationToken)
    {
        var bytes = new byte[128];
        BitConverter.GetBytes(registrationId).CopyTo(bytes, 0);
        await using var cmd = new SqlCommand("SET CONTEXT_INFO @Ctx", con);
        cmd.Parameters.Add("@Ctx", SqlDbType.Binary, 128).Value = bytes;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ExtraIncomeDto?> GetExtraIncomeAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT e.Extra_IncomeID, e.Extra_IncomeCategoryID, c.Extra_Income_CategoryName, e.Extra_IncomeFor,
       e.Extra_IncomeAmount, e.Extra_IncomeDate,
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS ReceivedBy
FROM dbo.Extra_Income AS e
INNER JOIN dbo.Extra_IncomeCategory AS c ON e.Extra_IncomeCategoryID = c.Extra_IncomeCategoryID
LEFT JOIN dbo.Admin AS a ON e.RegistrationID = a.RegistrationID
WHERE e.Extra_IncomeID = @ID AND e.SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new ExtraIncomeDto
        {
            ExtraIncomeID = ToInt(reader["Extra_IncomeID"]),
            ExtraIncomeCategoryID = ToInt(reader["Extra_IncomeCategoryID"]),
            Category = reader["Extra_Income_CategoryName"]?.ToString() ?? "",
            Details = NullString(reader["Extra_IncomeFor"]),
            Amount = ToDec(reader["Extra_IncomeAmount"]),
            Date = Convert.ToDateTime(reader["Extra_IncomeDate"]).Date,
            ReceivedBy = NullString(reader["ReceivedBy"])
        };
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> ListExpenseCategoriesAsync(SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT ExpenseCategoryID, CategoryName
FROM dbo.Expense_CategoryName
WHERE SchoolID = @SchoolID
ORDER BY CategoryName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<ExpenseCategoryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ExpenseCategoryDto
            {
                ExpenseCategoryID = ToInt(reader["ExpenseCategoryID"]),
                Name = reader["CategoryName"]?.ToString() ?? ""
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<ExpenseSubCategoryDto>> ListExpenseSubCategoriesAsync(
        SessionSnapshot session, int categoryId, CancellationToken cancellationToken)
    {
        if (categoryId <= 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("""
SELECT ExpenseSubCategoryID, ExpenseCategoryID, SubCategoryName
FROM dbo.Expense_SubCategory
WHERE SchoolID = @SchoolID AND ExpenseCategoryID = @Cat
ORDER BY ExpenseSubCategoryID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Cat", categoryId);
            var items = new List<ExpenseSubCategoryDto>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new ExpenseSubCategoryDto
                {
                    ExpenseSubCategoryID = ToInt(reader["ExpenseSubCategoryID"]),
                    ExpenseCategoryID = ToInt(reader["ExpenseCategoryID"]),
                    Name = reader["SubCategoryName"]?.ToString() ?? ""
                });
            }
            return items;
        }
        catch (SqlException)
        {
            return [];
        }
    }

    public async Task<AccountsResult> CreateExpenseCategoryAsync(SessionSnapshot session, string? name, CancellationToken cancellationToken)
    {
        var text = (name ?? "").Trim();
        if (text.Length == 0)
            return Fail("acc.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var check = new SqlCommand("""
SELECT ExpenseCategoryID FROM dbo.Expense_CategoryName
WHERE SchoolID = @SchoolID AND CategoryName = @Name
""", con))
        {
            check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            check.Parameters.AddWithValue("@Name", text);
            if (await check.ExecuteScalarAsync(cancellationToken) is not null)
                return Fail("acc.categoryExists");
        }
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Expense_CategoryName (CategoryName, RegistrationID, SchoolID)
VALUES (@Name, @RegistrationID, @SchoolID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@Name", text);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        return Ok(Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)));
    }

    public async Task<AccountsResult> UpdateExpenseCategoryAsync(SessionSnapshot session, SaveExpenseCategoryRequest? request, CancellationToken cancellationToken)
    {
        var text = (request?.Name ?? "").Trim();
        if (request is null || request.ExpenseCategoryID <= 0 || text.Length == 0)
            return Fail("acc.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var check = new SqlCommand("""
SELECT ExpenseCategoryID FROM dbo.Expense_CategoryName
WHERE SchoolID = @SchoolID AND CategoryName = @Name AND ExpenseCategoryID <> @ID
""", con))
        {
            check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            check.Parameters.AddWithValue("@Name", text);
            check.Parameters.AddWithValue("@ID", request.ExpenseCategoryID);
            if (await check.ExecuteScalarAsync(cancellationToken) is not null)
                return Fail("acc.categoryExists");
        }
        await using var cmd = new SqlCommand("""
UPDATE dbo.Expense_CategoryName SET CategoryName = @Name
WHERE ExpenseCategoryID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Name", text);
        cmd.Parameters.AddWithValue("@ID", request.ExpenseCategoryID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0 ? Ok(request.ExpenseCategoryID) : Fail("acc.empty");
    }

    public async Task<AccountsResult> DeleteExpenseCategoryAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("acc.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand(
                "DELETE FROM dbo.Expense_CategoryName WHERE ExpenseCategoryID = @ID AND SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return n > 0 ? Ok(id) : Fail("acc.empty");
        }
        catch (SqlException)
        {
            return Fail("acc.categoryUsed");
        }
    }

    public async Task<AccountsResult> CreateExpenseSubCategoryAsync(SessionSnapshot session, SaveExpenseSubCategoryRequest? request, CancellationToken cancellationToken)
    {
        var text = (request?.Name ?? "").Trim();
        if (request is null || request.ExpenseCategoryID <= 0 || text.Length == 0)
            return Fail("acc.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("""
IF NOT EXISTS (
    SELECT 1 FROM dbo.Expense_SubCategory
    WHERE ExpenseCategoryID = @Cat AND SchoolID = @SchoolID AND SubCategoryName = @Name)
INSERT INTO dbo.Expense_SubCategory (ExpenseCategoryID, SubCategoryName, SchoolID, RegistrationID)
VALUES (@Cat, @Name, @SchoolID, @RegistrationID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
            cmd.Parameters.AddWithValue("@Cat", request.ExpenseCategoryID);
            cmd.Parameters.AddWithValue("@Name", text);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            var id = await cmd.ExecuteScalarAsync(cancellationToken);
            return id is int or long or decimal ? Ok(Convert.ToInt32(id)) : Ok();
        }
        catch (SqlException)
        {
            return Fail("acc.failed");
        }
    }

    public async Task<AccountsResult> UpdateExpenseSubCategoryAsync(SessionSnapshot session, SaveExpenseSubCategoryRequest? request, CancellationToken cancellationToken)
    {
        var text = (request?.Name ?? "").Trim();
        if (request is null || request.ExpenseSubCategoryID <= 0 || text.Length == 0)
            return Fail("acc.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Expense_SubCategory SET SubCategoryName = @Name
WHERE ExpenseSubCategoryID = @ID AND SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@Name", text);
            cmd.Parameters.AddWithValue("@ID", request.ExpenseSubCategoryID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return n > 0 ? Ok(request.ExpenseSubCategoryID) : Fail("acc.empty");
        }
        catch (SqlException)
        {
            return Fail("acc.failed");
        }
    }

    public async Task<AccountsResult> DeleteExpenseSubCategoryAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("acc.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand(
                "DELETE FROM dbo.Expense_SubCategory WHERE ExpenseSubCategoryID = @ID AND SchoolID = @SchoolID", con);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return n > 0 ? Ok(id) : Fail("acc.empty");
        }
        catch (SqlException)
        {
            return Fail("acc.subUsed");
        }
    }

    public async Task<(IReadOnlyList<ExpenseDto> Items, decimal Total, int TotalCount)> ListExpenseAsync(
        SessionSnapshot session, int categoryId, int subCategoryId, DateTime? from, DateTime? to, string? receiptNo,
        int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var skip = (page - 1) * pageSize;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        const string whereSql = """
WHERE e.SchoolID = @SchoolID AND e.EducationYearID = @YearID
  AND (@Cat = 0 OR e.ExpenseCategoryID = @Cat)
  AND (@Sub = 0 OR e.ExpenseSubCategoryID = @Sub)
  AND (@From IS NULL OR CAST(e.ExpenseDate AS DATE) >= @From)
  AND (@To IS NULL OR CAST(e.ExpenseDate AS DATE) <= @To)
  AND (CAST(e.ExpenseID AS nvarchar(20)) LIKE @Rid)
""";
        const string fromSql = """
FROM dbo.Expenditure AS e
INNER JOIN dbo.Expense_CategoryName AS c ON e.ExpenseCategoryID = c.ExpenseCategoryID
LEFT JOIN dbo.Expense_SubCategory AS s ON e.ExpenseSubCategoryID = s.ExpenseSubCategoryID
""";

        void BindFilters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@Cat", categoryId);
            cmd.Parameters.AddWithValue("@Sub", subCategoryId);
            var fromP = cmd.Parameters.Add("@From", SqlDbType.Date);
            fromP.Value = from?.Date ?? (object)DBNull.Value;
            var toP = cmd.Parameters.Add("@To", SqlDbType.Date);
            toP.Value = to?.Date ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@Rid", string.IsNullOrWhiteSpace(receiptNo) ? "%" : receiptNo.Trim());
        }

        decimal total = 0;
        var totalCount = 0;
        try
        {
            await using (var sum = new SqlCommand($"SELECT ISNULL(SUM(e.Amount), 0), COUNT(1) {fromSql} {whereSql}", con))
            {
                BindFilters(sum);
                await using var sumReader = await sum.ExecuteReaderAsync(cancellationToken);
                if (await sumReader.ReadAsync(cancellationToken))
                {
                    total = ToDec(sumReader[0]);
                    totalCount = ToInt(sumReader[1]);
                }
            }

            await using var cmd = new SqlCommand($"""
SELECT e.ExpenseID, e.ExpenseCategoryID, e.ExpenseSubCategoryID, c.CategoryName,
       ISNULL(s.SubCategoryName, N'') AS SubCategoryName, e.ExpenseFor, e.Amount, e.ExpenseDate
{fromSql}
{whereSql}
ORDER BY e.ExpenseID DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
""", con);
            BindFilters(cmd);
            cmd.Parameters.AddWithValue("@Skip", skip);
            cmd.Parameters.AddWithValue("@Take", pageSize);
            var items = new List<ExpenseDto>();
            await using var listReader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await listReader.ReadAsync(cancellationToken))
            {
                var amount = ToDec(listReader["Amount"]);
                items.Add(ReadExpenseRow(listReader, amount));
            }
            return (items, total, totalCount);
        }
        catch (SqlException)
        {
            const string fallbackWhere = """
WHERE e.SchoolID = @SchoolID AND e.EducationYearID = @YearID
  AND (@Cat = 0 OR e.ExpenseCategoryID = @Cat)
  AND (@From IS NULL OR CAST(e.ExpenseDate AS DATE) >= @From)
  AND (@To IS NULL OR CAST(e.ExpenseDate AS DATE) <= @To)
  AND (CAST(e.ExpenseID AS nvarchar(20)) LIKE @Rid)
""";
            const string fallbackFrom = """
FROM dbo.Expenditure AS e
INNER JOIN dbo.Expense_CategoryName AS c ON e.ExpenseCategoryID = c.ExpenseCategoryID
""";
            await using (var sum = new SqlCommand($"SELECT ISNULL(SUM(e.Amount), 0), COUNT(1) {fallbackFrom} {fallbackWhere}", con))
            {
                BindFilters(sum);
                await using var sumReader = await sum.ExecuteReaderAsync(cancellationToken);
                if (await sumReader.ReadAsync(cancellationToken))
                {
                    total = ToDec(sumReader[0]);
                    totalCount = ToInt(sumReader[1]);
                }
            }

            await using var fallback = new SqlCommand($"""
SELECT e.ExpenseID, e.ExpenseCategoryID, c.CategoryName, e.ExpenseFor, e.Amount, e.ExpenseDate
{fallbackFrom}
{fallbackWhere}
ORDER BY e.ExpenseID DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
""", con);
            BindFilters(fallback);
            fallback.Parameters.AddWithValue("@Skip", skip);
            fallback.Parameters.AddWithValue("@Take", pageSize);
            var items = new List<ExpenseDto>();
            await using var fbReader = await fallback.ExecuteReaderAsync(cancellationToken);
            while (await fbReader.ReadAsync(cancellationToken))
            {
                var amount = ToDec(fbReader["Amount"]);
                items.Add(new ExpenseDto
                {
                    ExpenseID = ToInt(fbReader["ExpenseID"]),
                    ExpenseCategoryID = ToInt(fbReader["ExpenseCategoryID"]),
                    Category = fbReader["CategoryName"]?.ToString() ?? "",
                    Details = NullString(fbReader["ExpenseFor"]),
                    Amount = amount,
                    Date = Convert.ToDateTime(fbReader["ExpenseDate"]).Date
                });
            }
            return (items, total, totalCount);
        }
    }

    public async Task<AccountsResult> CreateExpenseAsync(SessionSnapshot session, SaveExpenseRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ExpenseCategoryID <= 0 || request.Amount <= 0 || request.AccountID <= 0)
            return Fail("acc.needAmount");
        var paid = request.Date == default ? DateTime.Today : request.Date.Date;
        if (paid > DateTime.Today)
            return Fail("acc.futureDate");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using (var bal = new SqlCommand(
            "SELECT ISNULL(AccountBalance, 0) FROM dbo.Account WHERE AccountID = @ID AND SchoolID = @SchoolID", con))
        {
            bal.Parameters.AddWithValue("@ID", request.AccountID);
            bal.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var value = await bal.ExecuteScalarAsync(cancellationToken);
            if (value is null)
                return Fail("acc.needPay");
            if (ToDec(value) < request.Amount)
                return Fail("acc.overBalance");
        }
        try
        {
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Expenditure
    (RegistrationID, ExpenseCategoryID, ExpenseSubCategoryID, Amount, ExpenseFor, ExpenseDate, SchoolID, EducationYearID, AccountID)
VALUES
    (@RegistrationID, @Cat, @Sub, @Amount, @Details, @Date, @SchoolID, @YearID, @AccountID);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@Cat", request.ExpenseCategoryID);
            cmd.Parameters.AddWithValue("@Sub", (object?)request.ExpenseSubCategoryID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Amount", request.Amount);
            cmd.Parameters.AddWithValue("@Details", (object?)request.Details ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Date", paid);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@AccountID", request.AccountID);
            return Ok(Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)));
        }
        catch (SqlException)
        {
            return Fail("acc.overBalance");
        }
    }

    public async Task<AccountsResult> UpdateExpenseAsync(SessionSnapshot session, SaveExpenseRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.ExpenseID <= 0 || request.ExpenseCategoryID <= 0 || request.Amount <= 0)
            return Fail("acc.needAmount");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await _inventory.PrepareSchemaAsync(con, cancellationToken);
        var reapply = await IsInventoryPurchaseCategoryAsync(con, session.SchoolID, request.ExpenseCategoryID, cancellationToken);
        await SetContextAsync(con, session.RegistrationID, cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var inv = await _inventory.SyncExpenseInventoryInTxAsync(
                con, tx, session, request.ExpenseID, request.Amount, reapply, cancellationToken);
            if (!inv.Succeeded)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail(inv.Error ?? "acc.failed");
            }
            var details = request.Details;
            if (reapply && details is not null &&
                details.StartsWith("Supplier Payment:", StringComparison.OrdinalIgnoreCase))
            {
                var dot = details.IndexOf('.', "Supplier Payment:".Length);
                if (dot > 0)
                    details = details[..(dot + 1)] + $" {request.Amount:0.##} Tk.";
            }
            await using var cmd = new SqlCommand("""
UPDATE dbo.Expenditure
SET Amount = @Amount, ExpenseFor = @Details, ExpenseCategoryID = @Cat, ExpenseSubCategoryID = @Sub
WHERE ExpenseID = @ID AND SchoolID = @SchoolID
""", con, tx);
            cmd.Parameters.AddWithValue("@Amount", request.Amount);
            cmd.Parameters.AddWithValue("@Details", (object?)details ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Cat", request.ExpenseCategoryID);
            cmd.Parameters.AddWithValue("@Sub", (object?)request.ExpenseSubCategoryID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ID", request.ExpenseID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
            if (n <= 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("acc.empty");
            }
            await tx.CommitAsync(cancellationToken);
            return Ok(request.ExpenseID);
        }
        catch (SqlException)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail("acc.overBalance");
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AccountsResult> DeleteExpenseAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("acc.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await _inventory.PrepareSchemaAsync(con, cancellationToken);
        await SetContextAsync(con, session.RegistrationID, cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var inv = await _inventory.SyncExpenseInventoryInTxAsync(con, tx, session, id, 0, false, cancellationToken);
            if (!inv.Succeeded)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail(inv.Error ?? "acc.failed");
            }
            await using var cmd = new SqlCommand(
                "DELETE FROM dbo.Expenditure WHERE ExpenseID = @ID AND SchoolID = @SchoolID", con, tx);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
            if (n <= 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("acc.empty");
            }
            await tx.CommitAsync(cancellationToken);
            return Ok(id);
        }
        catch (SqlException)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail("acc.failed");
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<bool> IsInventoryPurchaseCategoryAsync(
        SqlConnection con, int schoolId, int categoryId, CancellationToken cancellationToken)
    {
        if (categoryId <= 0) return false;
        await using var cmd = new SqlCommand("""
SELECT CategoryName FROM dbo.Expense_CategoryName
WHERE ExpenseCategoryID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", categoryId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var name = (await cmd.ExecuteScalarAsync(cancellationToken))?.ToString() ?? "";
        return string.Equals(name.Trim(), InventoryService.PurchaseCategory, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ExpenseDto?> GetExpenseAsync(SessionSnapshot session, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return null;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT e.ExpenseID, e.ExpenseCategoryID, e.ExpenseSubCategoryID, c.CategoryName,
       ISNULL(s.SubCategoryName, N'') AS SubCategoryName, e.ExpenseFor, e.Amount, e.ExpenseDate,
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS ReceivedBy
FROM dbo.Expenditure AS e
INNER JOIN dbo.Expense_CategoryName AS c ON e.ExpenseCategoryID = c.ExpenseCategoryID
LEFT JOIN dbo.Expense_SubCategory AS s ON e.ExpenseSubCategoryID = s.ExpenseSubCategoryID
LEFT JOIN dbo.Admin AS a ON e.RegistrationID = a.RegistrationID
WHERE e.ExpenseID = @ID AND e.SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            var row = ReadExpenseRow(reader, ToDec(reader["Amount"]));
            row.ReceivedBy = NullString(reader["ReceivedBy"]);
            return row;
        }
        catch (SqlException)
        {
            await using var fallback = new SqlCommand("""
SELECT e.ExpenseID, e.ExpenseCategoryID, c.CategoryName, e.ExpenseFor, e.Amount, e.ExpenseDate,
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS ReceivedBy
FROM dbo.Expenditure AS e
INNER JOIN dbo.Expense_CategoryName AS c ON e.ExpenseCategoryID = c.ExpenseCategoryID
LEFT JOIN dbo.Admin AS a ON e.RegistrationID = a.RegistrationID
WHERE e.ExpenseID = @ID AND e.SchoolID = @SchoolID
""", con);
            fallback.Parameters.AddWithValue("@ID", id);
            fallback.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await fallback.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            return new ExpenseDto
            {
                ExpenseID = ToInt(reader["ExpenseID"]),
                ExpenseCategoryID = ToInt(reader["ExpenseCategoryID"]),
                Category = reader["CategoryName"]?.ToString() ?? "",
                Details = NullString(reader["ExpenseFor"]),
                Amount = ToDec(reader["Amount"]),
                Date = Convert.ToDateTime(reader["ExpenseDate"]).Date,
                ReceivedBy = NullString(reader["ReceivedBy"])
            };
        }
    }

    private static ExpenseDto ReadExpenseRow(SqlDataReader reader, decimal amount) => new()
    {
        ExpenseID = ToInt(reader["ExpenseID"]),
        ExpenseCategoryID = ToInt(reader["ExpenseCategoryID"]),
        ExpenseSubCategoryID = reader["ExpenseSubCategoryID"] is DBNull ? null : ToInt(reader["ExpenseSubCategoryID"]),
        Category = reader["CategoryName"]?.ToString() ?? "",
        SubCategory = NullString(reader["SubCategoryName"]),
        Details = NullString(reader["ExpenseFor"]),
        Amount = amount,
        Date = Convert.ToDateTime(reader["ExpenseDate"]).Date
    };

    private async Task<List<int>> ResolveStudentClassIdsAsync(
        SessionSnapshot session, IEnumerable<string> studentIds, CancellationToken cancellationToken)
    {
        var codes = studentIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (codes.Count == 0)
            return [];

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var ids = new List<int>();
        foreach (var code in codes)
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 sc.StudentClassID
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON s.StudentID = sc.StudentID
WHERE sc.SchoolID = @SchoolID AND sc.EducationYearID = @YearID AND s.ID = @ID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@ID", code);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            if (value is int id && id > 0)
                ids.Add(id);
            else if (value is not null and not DBNull)
                ids.Add(Convert.ToInt32(value));
        }

        return ids;
    }
}
