using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Employees;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SalaryService
{
    private readonly EduConnectionFactory _connections;

    public SalaryService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public Task<IReadOnlyList<SalaryNameDto>> ListNamesAsync(
        SessionSnapshot session, string kind, CancellationToken cancellationToken) =>
        QueryNamesAsync(session, kind, cancellationToken);

    public async Task<SalaryResult> CreateNameAsync(
        SessionSnapshot session, string kind, SaveSalaryNameRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("sal.needName");
        var map = Map(kind);
        if (map is null)
            return Fail("sal.failed");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await NameExistsAsync(con, map.Value.Table, map.Value.NameCol, session.SchoolID, name, null, map.Value.IdCol, cancellationToken))
            return Fail("sal.exists");

        await using var cmd = new SqlCommand($"""
INSERT INTO dbo.{map.Value.Table} (SchoolID, RegistrationID, {map.Value.NameCol})
VALUES (@SchoolID, @RegistrationID, @Name);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Name", name);
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        return new SalaryResult { Succeeded = true, Id = id };
    }

    public async Task<SalaryResult> UpdateNameAsync(
        SessionSnapshot session, string kind, int id, SaveSalaryNameRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        if (id <= 0)
            return Fail("sal.needItem");
        if (name.Length == 0)
            return Fail("sal.needName");
        var map = Map(kind);
        if (map is null)
            return Fail("sal.failed");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await NameExistsAsync(con, map.Value.Table, map.Value.NameCol, session.SchoolID, name, id, map.Value.IdCol, cancellationToken))
            return Fail("sal.exists");

        await using var cmd = new SqlCommand($"""
UPDATE dbo.{map.Value.Table}
SET {map.Value.NameCol} = @Name
WHERE {map.Value.IdCol} = @Id AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0 ? new SalaryResult { Succeeded = true, Id = id } : Fail("sal.needItem");
    }

    public async Task<SalaryResult> DeleteNameAsync(
        SessionSnapshot session, string kind, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return Fail("sal.needItem");
        var map = Map(kind);
        if (map is null)
            return Fail("sal.failed");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await InUseAsync(con, kind, session.SchoolID, id, cancellationToken))
            return Fail("sal.inUse");

        await using var cmd = new SqlCommand($"""
DELETE FROM dbo.{map.Value.Table}
WHERE {map.Value.IdCol} = @Id AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0 ? new SalaryResult { Succeeded = true, Id = id } : Fail("sal.needItem");
    }

    public async Task<IReadOnlyList<SalaryAssignRowDto>> ListAssignAsync(
        SessionSnapshot session, string kind, int nameId, string? type, CancellationToken cancellationToken)
    {
        var employeeType = NormalizeType(type);
        var isAllowance = string.Equals(kind, "allowance", StringComparison.OrdinalIgnoreCase);
        var joinTable = isAllowance ? "Employee_Allowance_Assign" : "Employee_Deduction_Assign";
        var idCol = isAllowance ? "AllowanceID" : "DeductionID";
        var amountCol = isAllowance ? "AllowanceAmount" : "DeductionAmount";
        var sql = $"""
SELECT v.EmployeeID, v.ID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(v.Designation, N'') AS Designation, v.EmployeeType, v.Phone, ISNULL(v.Salary, 0) AS Salary,
       CAST(CASE WHEN a.{idCol} IS NULL THEN 0 ELSE 1 END AS bit) AS Assigned,
       ISNULL(a.{amountCol}, 0) AS Amount,
       ISNULL(a.Fixed_Percetage, N'Fixed') AS FixedOrPercentage
FROM dbo.VW_Emp_Info AS v
LEFT JOIN dbo.{joinTable} AS a ON a.EmployeeID = v.EmployeeID AND a.{idCol} = @NameId
WHERE v.SchoolID = @SchoolID AND v.Job_Status = N'Active' AND v.EmployeeType LIKE @EmployeeType
ORDER BY v.ID
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@NameId", nameId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EmployeeType", employeeType);
        var items = new List<SalaryAssignRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SalaryAssignRowDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                Phone = NullString(reader["Phone"]),
                Salary = ToDec(reader["Salary"]),
                Assigned = Convert.ToBoolean(reader["Assigned"]),
                Amount = ToDec(reader["Amount"]),
                FixedOrPercentage = reader["FixedOrPercentage"]?.ToString() == "Percentage" ? "Percentage" : "Fixed"
            });
        }

        return items;
    }

    public async Task<SalaryResult> SaveAssignAsync(
        SessionSnapshot session, string kind, SaveSalaryAssignRequest? request, CancellationToken cancellationToken)
    {
        request ??= new SaveSalaryAssignRequest();
        if (request.NameId <= 0)
            return Fail("sal.needItem");
        var isAllowance = string.Equals(kind, "allowance", StringComparison.OrdinalIgnoreCase);

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var item in request.Items)
            {
                if (item.Assigned)
                {
                    var mode = item.FixedOrPercentage == "Percentage" ? "Percentage" : "Fixed";
                    var amount = mode == "Percentage" ? Math.Min(item.Amount, 100) : item.Amount;
                    if (amount <= 0)
                        continue;
                    if (isAllowance)
                        await UpsertAllowanceAsync(con, tx, session, request.NameId, item.EmployeeID, amount, mode, cancellationToken);
                    else
                        await UpsertDeductionAsync(con, tx, session, request.NameId, item.EmployeeID, amount, mode, cancellationToken);
                }
                else if (isAllowance)
                {
                    await using var del = new SqlCommand("""
DELETE FROM dbo.Employee_Allowance_Assign
WHERE EmployeeID = @EmployeeID AND AllowanceID = @NameId AND SchoolID = @SchoolID
""", con, tx);
                    del.Parameters.AddWithValue("@EmployeeID", item.EmployeeID);
                    del.Parameters.AddWithValue("@NameId", request.NameId);
                    del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    await del.ExecuteNonQueryAsync(cancellationToken);
                }
                else
                {
                    await using var del = new SqlCommand("""
DELETE FROM dbo.Employee_Deduction_Assign
WHERE EmployeeID = @EmployeeID AND DeductionID = @NameId AND SchoolID = @SchoolID
""", con, tx);
                    del.Parameters.AddWithValue("@EmployeeID", item.EmployeeID);
                    del.Parameters.AddWithValue("@NameId", request.NameId);
                    del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    await del.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await tx.CommitAsync(cancellationToken);
            return new SalaryResult { Succeeded = true, Id = request.NameId };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyList<PayorderEmployeeDto>> ListPayorderEmployeesAsync(
        SessionSnapshot session, string? type, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT e.EmployeeID, e.ID,
       LTRIM(RTRIM(ISNULL(e.FirstName, N'') + N' ' + ISNULL(e.LastName, N''))) AS Name,
       ISNULL(e.Designation, N'') AS Designation, e.EmployeeType, e.Phone, ISNULL(e.Salary, 0) AS Salary,
       e.Bank_AccNo, ISNULL(e.Employee_Payorder_NameID, 0) AS PayorderNameId,
       p.Payorder_Name
FROM dbo.VW_Emp_Info AS e
LEFT JOIN dbo.Employee_Payorder_Name AS p ON p.Employee_Payorder_NameID = e.Employee_Payorder_NameID
WHERE e.SchoolID = @SchoolID AND e.Job_Status = N'Active' AND e.EmployeeType LIKE @EmployeeType
ORDER BY e.ID
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EmployeeType", NormalizeType(type));
        var items = new List<PayorderEmployeeDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PayorderEmployeeDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                Phone = NullString(reader["Phone"]),
                Salary = ToDec(reader["Salary"]),
                BankAccNo = NullString(reader["Bank_AccNo"]),
                PayorderNameId = Convert.ToInt32(reader["PayorderNameId"]),
                PayorderName = NullString(reader["Payorder_Name"])
            });
        }

        return items;
    }

    public async Task<SalaryResult> AssignPayorderAsync(
        SessionSnapshot session, AssignPayorderRequest? request, CancellationToken cancellationToken)
    {
        request ??= new AssignPayorderRequest();
        if (request.PayorderNameId <= 0)
            return Fail("sal.needPayorder");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        foreach (var employeeId in request.EmployeeIDs.Distinct())
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.Employee_Info
SET Employee_Payorder_NameID = @PayorderNameId
WHERE SchoolID = @SchoolID AND EmployeeID = @EmployeeID
""", con);
            cmd.Parameters.AddWithValue("@PayorderNameId", request.PayorderNameId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return new SalaryResult { Succeeded = true, Count = request.EmployeeIDs.Count };
    }

    public async Task<IReadOnlyList<SalaryMonthDto>> ListMonthsAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
SELECT StartDate, EndDate FROM dbo.Education_Year
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return [];

        var start = Convert.ToDateTime(reader["StartDate"]);
        var end = Convert.ToDateTime(reader["EndDate"]);
        var months = new List<SalaryMonthDto>();
        var cursor = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);
        while (cursor <= last)
        {
            months.Add(new SalaryMonthDto
            {
                Date = cursor,
                Name = cursor.ToString("MMM yyyy", CultureInfo.InvariantCulture)
            });
            cursor = cursor.AddMonths(1);
        }

        return months;
    }

    public async Task<SalaryResult> GenerateAsync(
        SessionSnapshot session, GenerateSalaryRequest? request, CancellationToken cancellationToken)
    {
        request ??= new GenerateSalaryRequest();
        if (request.PayorderNameId <= 0)
            return Fail("sal.needPayorder");
        if (string.IsNullOrWhiteSpace(request.MonthName) || request.EmployeeIDs.Count == 0)
            return Fail("sal.needGenerate");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var count = 0;
        foreach (var employeeId in request.EmployeeIDs.Distinct())
        {
            await using var cmd = new SqlCommand("dbo.Emp_Salary_Monthly", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            cmd.Parameters.AddWithValue("@Employee_Payorder_NameID", request.PayorderNameId);
            cmd.Parameters.AddWithValue("@Get_date", request.MonthDate.Date);
            cmd.Parameters.AddWithValue("@MonthName", request.MonthName.Trim());
            var outId = cmd.Parameters.Add("@GeT_Employee_PayorderID", SqlDbType.Int);
            outId.Direction = ParameterDirection.InputOutput;
            outId.Value = 0;
            try
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                if (outId.Value is int generated && generated > 0)
                    count++;
            }
            catch (SqlException ex) when (ex.Number is 2812)
            {
                return Fail("sal.needSp");
            }
        }

        return new SalaryResult { Succeeded = true, Count = count };
    }

    public async Task<IReadOnlyList<MonthlyPayorderDto>> ListMonthlyAsync(
        SessionSnapshot session, int payorderNameId, string monthName, string? type, CancellationToken cancellationToken)
    {
        if (payorderNameId <= 0 || string.IsNullOrWhiteSpace(monthName))
            return [];

        const string sql = """
SELECT m.MonthlyPayorderID, p.Employee_PayorderID, p.EmployeeID, v.ID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(v.Designation, N'') AS Designation, v.Bank_AccNo, v.EmployeeType, v.Phone,
       ISNULL(p.PayorderAmount, 0) AS PayorderAmount,
       ISNULL(m.WorkingDays, 0) AS WorkingDays, ISNULL(m.PerDays, 0) AS PerDays,
       ISNULL(m.AbsDays, 0) AS AbsDays, ISNULL(m.LateDays, 0) AS LateDays,
       ISNULL(m.LeaveDays, 0) AS LeaveDays, ISNULL(m.FineCountDays, 0) AS FineCountDays,
       ISNULL(p.Allowance, 0) AS Allowance, ISNULL(p.Bonus, 0) AS Bonus,
       ISNULL(p.GrossSalary, 0) AS GrossSalary, ISNULL(p.Diduction, 0) AS Diduction,
       ISNULL(p.Fine, 0) AS Fine, ISNULL(m.FineAmount, 0) AS FineAmount,
       ISNULL(p.InTotalSalary, 0) AS InTotalSalary,
       ISNULL(p.PaidAmount, 0) AS PaidAmount, ISNULL(p.Due, 0) AS Due,
       ISNULL(p.PaidStatus, N'') AS PaidStatus
FROM dbo.Employee_Payorder_Monthly AS m
INNER JOIN dbo.Employee_Payorder AS p ON p.Employee_PayorderID = m.Employee_PayorderID
INNER JOIN dbo.VW_Emp_Info AS v ON v.EmployeeID = p.EmployeeID
WHERE p.Employee_Payorder_NameID = @PayorderNameId
  AND m.MonthName = @MonthName
  AND p.SchoolID = @SchoolID
  AND v.EmployeeType LIKE @EmployeeType
ORDER BY v.ID
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@PayorderNameId", payorderNameId);
        cmd.Parameters.AddWithValue("@MonthName", monthName);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EmployeeType", NormalizeType(type));
        var items = new List<MonthlyPayorderDto>();
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new MonthlyPayorderDto
                {
                    MonthlyPayorderID = Convert.ToInt32(reader["MonthlyPayorderID"]),
                    EmployeePayorderID = Convert.ToInt32(reader["Employee_PayorderID"]),
                    EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                    ID = reader["ID"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    Designation = reader["Designation"]?.ToString() ?? "",
                    BankAccNo = NullString(reader["Bank_AccNo"]),
                    EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                    Phone = NullString(reader["Phone"]),
                    PayorderAmount = ToDec(reader["PayorderAmount"]),
                    WorkingDays = Convert.ToInt32(reader["WorkingDays"]),
                    PresentDays = Convert.ToInt32(reader["PerDays"]),
                    AbsDays = Convert.ToInt32(reader["AbsDays"]),
                    LateDays = Convert.ToInt32(reader["LateDays"]),
                    LeaveDays = Convert.ToInt32(reader["LeaveDays"]),
                    FineCountDays = Convert.ToInt32(reader["FineCountDays"]),
                    Allowance = ToDec(reader["Allowance"]),
                    Bonus = ToDec(reader["Bonus"]),
                    GrossSalary = ToDec(reader["GrossSalary"]),
                    Deduction = ToDec(reader["Diduction"]),
                    Fine = ToDec(reader["Fine"]),
                    AttendanceFine = ToDec(reader["FineAmount"]),
                    NetSalary = ToDec(reader["InTotalSalary"]),
                    PaidAmount = ToDec(reader["PaidAmount"]),
                    Due = ToDec(reader["Due"]),
                    PaidStatus = reader["PaidStatus"]?.ToString() ?? ""
                });
            }
        }

        foreach (var row in items)
            await FillLinesAsync(con, session.SchoolID, row, cancellationToken);
        return items;
    }

    public async Task<SalaryResult> UpdateBonusFineAsync(
        SessionSnapshot session, UpdateBonusFineRequest? request, CancellationToken cancellationToken)
    {
        request ??= new UpdateBonusFineRequest();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        foreach (var item in request.Items)
        {
            await using (var fine = new SqlCommand("""
UPDATE dbo.Employee_Payorder_Monthly
SET FineAmount = @FineAmount
WHERE Employee_PayorderID = @Employee_PayorderID AND EmployeeID = @EmployeeID AND SchoolID = @SchoolID
""", con))
            {
                fine.Parameters.AddWithValue("@FineAmount", item.AttendanceFine);
                fine.Parameters.AddWithValue("@Employee_PayorderID", item.EmployeePayorderID);
                fine.Parameters.AddWithValue("@EmployeeID", item.EmployeeID);
                fine.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await fine.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var bonus in item.Bonuses)
                await UpsertAmountAsync(con, session, "Employee_Bonus_Records", "Bonus_RecordsID", "BonusID", "Bonus_Amount",
                    bonus.Id, item.EmployeeID, item.EmployeePayorderID, bonus.Amount, cancellationToken);
            foreach (var fineLine in item.Fines)
                await UpsertAmountAsync(con, session, "Employee_Fine_Records", "Fine_RecordsID", "FineID", "Fine_Amount",
                    fineLine.Id, item.EmployeeID, item.EmployeePayorderID, fineLine.Amount, cancellationToken);

            try
            {
                await RecalcPayorderAsync(con, session.SchoolID, item.EmployeePayorderID, cancellationToken);
            }
            catch (SqlException)
            {
                // Totals may be computed columns or handled by database triggers.
            }
        }

        return new SalaryResult { Succeeded = true, Count = request.Items.Count };
    }

    public Task<SalaryResult> DeletePayorderAsync(
        SessionSnapshot session, int employeePayorderId, CancellationToken cancellationToken) =>
        DeletePayordersAsync(session, new DeleteMonthlyPayordersRequest { EmployeePayorderIds = [employeePayorderId] }, cancellationToken);

    public async Task<SalaryResult> DeletePayordersAsync(
        SessionSnapshot session, DeleteMonthlyPayordersRequest? request, CancellationToken cancellationToken)
    {
        var ids = (request?.EmployeePayorderIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
            return Fail("sal.needDelete");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var inSql = string.Join(",", ids.Select((_, i) => "@P" + i));
            var allowed = new List<int>();
            await using (var check = new SqlCommand($"""
SELECT Employee_PayorderID
FROM dbo.Employee_Payorder
WHERE SchoolID = @SchoolID AND ISNULL(PaidAmount, 0) = 0
  AND Employee_PayorderID IN ({inSql})
""", con, tx))
            {
                check.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                for (var i = 0; i < ids.Count; i++)
                    check.Parameters.AddWithValue("@P" + i, ids[i]);
                await using var reader = await check.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    allowed.Add(Convert.ToInt32(reader[0]));
            }

            if (allowed.Count == 0)
                return Fail(ids.Count == 1 ? "sal.paidLock" : "sal.needDelete");

            var delIn = string.Join(",", allowed.Select((_, i) => "@D" + i));
            await using var cmd = new SqlCommand($"""
DELETE FROM dbo.Employee_Fine_Records WHERE SchoolID = @SchoolID AND Employee_PayorderID IN ({delIn});
DELETE FROM dbo.Employee_Deduction_Records WHERE SchoolID = @SchoolID AND Employee_PayorderID IN ({delIn});
DELETE FROM dbo.Employee_Bonus_Records WHERE SchoolID = @SchoolID AND Employee_PayorderID IN ({delIn});
DELETE FROM dbo.Employee_Allowance_Records WHERE SchoolID = @SchoolID AND Employee_PayorderID IN ({delIn});
DELETE FROM dbo.Employee_Payorder_Monthly WHERE SchoolID = @SchoolID AND Employee_PayorderID IN ({delIn});
DELETE FROM dbo.Employee_Payorder WHERE SchoolID = @SchoolID AND Employee_PayorderID IN ({delIn});
""", con, tx);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            for (var i = 0; i < allowed.Count; i++)
                cmd.Parameters.AddWithValue("@D" + i, allowed[i]);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return new SalaryResult
            {
                Succeeded = true,
                Count = allowed.Count,
                Id = allowed.Count == 1 ? allowed[0] : 0
            };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<AccountOptionDto>> ListAccountsAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT AccountID, AccountName, ISNULL(AccountBalance, 0) AS AccountBalance, ISNULL(Default_Status, N'') AS Default_Status
FROM dbo.Account
WHERE SchoolID = @SchoolID AND AccountBalance <> 0
ORDER BY CASE WHEN Default_Status = N'True' THEN 0 ELSE 1 END, AccountName
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<AccountOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AccountOptionDto
            {
                AccountID = Convert.ToInt32(reader["AccountID"]),
                Name = $"{reader["AccountName"]} ({ToDec(reader["AccountBalance"]):0.##} tk)",
                Balance = ToDec(reader["AccountBalance"])
            });
        }

        return items;
    }

    public async Task<SalaryResult> PayAsync(
        SessionSnapshot session, PaySalaryRequest? request, CancellationToken cancellationToken)
    {
        request ??= new PaySalaryRequest();
        if (request.AccountID <= 0)
            return Fail("sal.needAccount");
        var items = request.Items.Where(x => x.Amount > 0).ToList();
        if (items.Count == 0)
            return Fail("sal.needPay");

        var total = items.Sum(x => x.Amount);
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var balCmd = new SqlCommand(
            "SELECT ISNULL(AccountBalance, 0) FROM dbo.Account WHERE AccountID = @AccountID AND SchoolID = @SchoolID", con);
        balCmd.Parameters.AddWithValue("@AccountID", request.AccountID);
        balCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var balance = ToDec(await balCmd.ExecuteScalarAsync(cancellationToken) ?? 0);
        if (balance < total)
            return Fail("sal.needBalance");

        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var paidCount = 0;
            foreach (var item in items)
            {
                await using var dueCmd = new SqlCommand("""
SELECT ISNULL(Due, ISNULL(InTotalSalary, 0) - ISNULL(PaidAmount, 0))
FROM dbo.Employee_Payorder
WHERE Employee_PayorderID = @Id AND SchoolID = @SchoolID AND EmployeeID = @EmployeeID
""", con, tx);
                dueCmd.Parameters.AddWithValue("@Id", item.EmployeePayorderID);
                dueCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                dueCmd.Parameters.AddWithValue("@EmployeeID", item.EmployeeID);
                var due = ToDec(await dueCmd.ExecuteScalarAsync(cancellationToken) ?? 0);
                if (item.Amount > due)
                    continue;

                var paidFor = $"Salary Paid To: {item.Name}. Pay For: {request.MonthName}. Paid Amount: {item.Amount} Tk.";
                await using var ins = new SqlCommand("""
INSERT INTO dbo.Employee_Payorder_Records
    (Employee_PayorderID, SchoolID, RegistrationID, EducationYearID, EmployeeID, AccountID, Amount, Paid_For, Paid_date)
VALUES (@Employee_PayorderID, @SchoolID, @RegistrationID, @EducationYearID, @EmployeeID, @AccountID, @Amount, @Paid_For, @Paid_date)
""", con, tx);
                ins.Parameters.AddWithValue("@Employee_PayorderID", item.EmployeePayorderID);
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
                ins.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                ins.Parameters.AddWithValue("@EmployeeID", item.EmployeeID);
                ins.Parameters.AddWithValue("@AccountID", request.AccountID);
                ins.Parameters.AddWithValue("@Amount", item.Amount);
                ins.Parameters.AddWithValue("@Paid_For", paidFor);
                ins.Parameters.AddWithValue("@Paid_date", request.PaidDate.Date);
                await ins.ExecuteNonQueryAsync(cancellationToken);

                await using var upd = new SqlCommand("""
UPDATE dbo.Employee_Payorder
SET PaidAmount = ISNULL(PaidAmount, 0) + @PaidAmount
WHERE Employee_PayorderID = @Id AND SchoolID = @SchoolID
""", con, tx);
                upd.Parameters.AddWithValue("@PaidAmount", item.Amount);
                upd.Parameters.AddWithValue("@Id", item.EmployeePayorderID);
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await upd.ExecuteNonQueryAsync(cancellationToken);
                paidCount++;
            }

            if (paidCount == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("sal.needPay");
            }

            await tx.CommitAsync(cancellationToken);
            return new SalaryResult { Succeeded = true, Count = paidCount };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyList<PaidRecordDto>> ListPaidRecordsAsync(
        SessionSnapshot session, int employeeId, int employeePayorderId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT r.Employee_Payorder_RecordID, r.Amount, r.Paid_date, a.AccountName
FROM dbo.Employee_Payorder_Records AS r
LEFT JOIN dbo.Account AS a ON a.AccountID = r.AccountID
WHERE r.SchoolID = @SchoolID AND r.EmployeeID = @EmployeeID AND r.Employee_PayorderID = @Employee_PayorderID
ORDER BY r.Paid_date
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        cmd.Parameters.AddWithValue("@Employee_PayorderID", employeePayorderId);
        var items = new List<PaidRecordDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PaidRecordDto
            {
                RecordID = Convert.ToInt32(reader["Employee_Payorder_RecordID"]),
                Amount = ToDec(reader["Amount"]),
                PaidDate = reader["Paid_date"] is DBNull ? null : Convert.ToDateTime(reader["Paid_date"]),
                AccountName = NullString(reader["AccountName"])
            });
        }

        return items;
    }

    public async Task<SalaryResult> DeletePaidRecordAsync(
        SessionSnapshot session, int recordId, CancellationToken cancellationToken)
    {
        if (recordId <= 0)
            return Fail("sal.needItem");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            await using var read = new SqlCommand("""
SELECT Employee_PayorderID, EmployeeID, Amount, AccountID
FROM dbo.Employee_Payorder_Records
WHERE Employee_Payorder_RecordID = @Id AND SchoolID = @SchoolID
""", con, tx);
            read.Parameters.AddWithValue("@Id", recordId);
            read.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await read.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                await reader.CloseAsync();
                await tx.RollbackAsync(cancellationToken);
                return Fail("sal.needItem");
            }

            await reader.CloseAsync();

            await using var del = new SqlCommand("""
DELETE FROM dbo.Employee_Payorder_Records
WHERE Employee_Payorder_RecordID = @Id AND SchoolID = @SchoolID
""", con, tx);
            del.Parameters.AddWithValue("@Id", recordId);
            del.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await del.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new SalaryResult { Succeeded = true, Id = recordId };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyList<PaidDueRowDto>> ListPaidDueAsync(
        SessionSnapshot session, IReadOnlyList<int>? payorderNameIds, CancellationToken cancellationToken)
    {
        var filter = payorderNameIds is { Count: > 0 };
        var sql = """
SELECT p.EmployeeID, v.ID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       m.MonthName, m.MonthStartDate,
       SUM(ISNULL(p.PaidAmount, 0)) AS Paid, SUM(ISNULL(p.Due, 0)) AS Due
FROM dbo.Employee_Payorder AS p
INNER JOIN dbo.VW_Emp_Info AS v ON v.EmployeeID = p.EmployeeID
INNER JOIN dbo.Employee_Payorder_Monthly AS m ON m.Employee_PayorderID = p.Employee_PayorderID
WHERE p.SchoolID = @SchoolID AND p.EducationYearID = @EducationYearID
""";
        if (filter)
            sql += " AND p.Employee_Payorder_NameID IN (" + string.Join(",", payorderNameIds!.Select((_, i) => "@R" + i)) + ")";
        sql += """
 GROUP BY p.EmployeeID, v.ID, v.FirstName, v.LastName, m.MonthName, m.MonthStartDate
 ORDER BY m.MonthStartDate, v.ID
""";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        if (filter)
        {
            for (var i = 0; i < payorderNameIds!.Count; i++)
                cmd.Parameters.AddWithValue("@R" + i, payorderNameIds[i]);
        }

        var items = new List<PaidDueRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PaidDueRowDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                MonthName = reader["MonthName"]?.ToString() ?? "",
                MonthStartDate = reader["MonthStartDate"] is DBNull or null
                    ? default
                    : Convert.ToDateTime(reader["MonthStartDate"]),
                Paid = ToDec(reader["Paid"]),
                Due = ToDec(reader["Due"])
            });
        }

        return items;
    }

    private async Task<IReadOnlyList<SalaryNameDto>> QueryNamesAsync(
        SessionSnapshot session, string kind, CancellationToken cancellationToken)
    {
        var map = Map(kind);
        if (map is null)
            return [];

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand($"""
SELECT {map.Value.IdCol} AS Id, {map.Value.NameCol} AS Name, CreateDate
FROM dbo.{map.Value.Table}
WHERE SchoolID = @SchoolID
ORDER BY {map.Value.NameCol}
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<SalaryNameDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SalaryNameDto
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"]?.ToString() ?? "",
                Created = reader["CreateDate"] is DBNull ? null : Convert.ToDateTime(reader["CreateDate"])
            });
        }

        return items;
    }

    private static async Task UpsertAllowanceAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, int allowanceId, int employeeId,
        decimal amount, string mode, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF EXISTS (SELECT 1 FROM dbo.Employee_Allowance_Assign WHERE SchoolID = @SchoolID AND EmployeeID = @EmployeeID AND AllowanceID = @AllowanceID)
UPDATE dbo.Employee_Allowance_Assign
SET RegistrationID = @RegistrationID, AllowanceAmount = @Amount, Fixed_Percetage = @Mode
WHERE SchoolID = @SchoolID AND EmployeeID = @EmployeeID AND AllowanceID = @AllowanceID
ELSE
INSERT INTO dbo.Employee_Allowance_Assign (SchoolID, RegistrationID, AllowanceID, EmployeeID, AllowanceAmount, Fixed_Percetage)
VALUES (@SchoolID, @RegistrationID, @AllowanceID, @EmployeeID, @Amount, @Mode)
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@AllowanceID", allowanceId);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@Mode", mode);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpsertDeductionAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, int deductionId, int employeeId,
        decimal amount, string mode, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF EXISTS (SELECT 1 FROM dbo.Employee_Deduction_Assign WHERE SchoolID = @SchoolID AND EmployeeID = @EmployeeID AND DeductionID = @DeductionID)
UPDATE dbo.Employee_Deduction_Assign
SET RegistrationID = @RegistrationID, DeductionAmount = @Amount, Fixed_Percetage = @Mode
WHERE SchoolID = @SchoolID AND EmployeeID = @EmployeeID AND DeductionID = @DeductionID
ELSE
INSERT INTO dbo.Employee_Deduction_Assign (SchoolID, RegistrationID, DeductionID, EmployeeID, DeductionAmount, Fixed_Percetage)
VALUES (@SchoolID, @RegistrationID, @DeductionID, @EmployeeID, @Amount, @Mode)
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@DeductionID", deductionId);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@Mode", mode);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpsertAmountAsync(
        SqlConnection con, SessionSnapshot session, string table, string recordIdCol, string fkCol, string amountCol,
        int fkId, int employeeId, int payorderId, decimal amount, CancellationToken cancellationToken)
    {
        if (amount > 0)
        {
            await using var cmd = new SqlCommand($"""
IF EXISTS (SELECT 1 FROM dbo.{table} WHERE SchoolID = @SchoolID AND {fkCol} = @Fk AND Employee_PayorderID = @PayorderId)
UPDATE dbo.{table} SET {amountCol} = @Amount
WHERE SchoolID = @SchoolID AND {fkCol} = @Fk AND Employee_PayorderID = @PayorderId
ELSE
INSERT INTO dbo.{table} (SchoolID, RegistrationID, {fkCol}, EmployeeID, Employee_PayorderID, {amountCol})
VALUES (@SchoolID, @RegistrationID, @Fk, @EmployeeID, @PayorderId, @Amount)
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@Fk", fkId);
            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            cmd.Parameters.AddWithValue("@PayorderId", payorderId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            await using var cmd = new SqlCommand($"""
DELETE FROM dbo.{table}
WHERE SchoolID = @SchoolID AND {fkCol} = @Fk AND Employee_PayorderID = @PayorderId
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Fk", fkId);
            cmd.Parameters.AddWithValue("@PayorderId", payorderId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task RecalcPayorderAsync(
        SqlConnection con, int schoolId, int payorderId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
;WITH t AS (
    SELECT p.Employee_PayorderID,
           ISNULL((SELECT SUM(r.AllowanceAmount) FROM dbo.Employee_Allowance_Records AS r WHERE r.Employee_PayorderID = p.Employee_PayorderID), 0) AS Allowance,
           ISNULL((SELECT SUM(r.Bonus_Amount) FROM dbo.Employee_Bonus_Records AS r WHERE r.Employee_PayorderID = p.Employee_PayorderID), 0) AS Bonus,
           ISNULL((SELECT SUM(r.Deduction_Amount) FROM dbo.Employee_Deduction_Records AS r WHERE r.Employee_PayorderID = p.Employee_PayorderID), 0) AS Diduction,
           ISNULL((SELECT SUM(r.Fine_Amount) FROM dbo.Employee_Fine_Records AS r WHERE r.Employee_PayorderID = p.Employee_PayorderID), 0)
             + ISNULL((SELECT TOP 1 m.FineAmount FROM dbo.Employee_Payorder_Monthly AS m WHERE m.Employee_PayorderID = p.Employee_PayorderID), 0) AS Fine
    FROM dbo.Employee_Payorder AS p
    WHERE p.Employee_PayorderID = @Id AND p.SchoolID = @SchoolID
)
UPDATE p
SET Allowance = t.Allowance,
    Bonus = t.Bonus,
    Diduction = t.Diduction,
    Fine = t.Fine,
    GrossSalary = ISNULL(p.PayorderAmount, 0) + t.Allowance + t.Bonus,
    InTotalSalary = ISNULL(p.PayorderAmount, 0) + t.Allowance + t.Bonus - t.Diduction - t.Fine,
    Due = ISNULL(p.PayorderAmount, 0) + t.Allowance + t.Bonus - t.Diduction - t.Fine - ISNULL(p.PaidAmount, 0),
    PaidStatus = CASE
        WHEN ISNULL(p.PaidAmount, 0) = 0 THEN N'Due'
        WHEN ISNULL(p.PayorderAmount, 0) + t.Allowance + t.Bonus - t.Diduction - t.Fine - ISNULL(p.PaidAmount, 0) <= 0 THEN N'Paid'
        ELSE N'Partial'
    END
FROM dbo.Employee_Payorder AS p
INNER JOIN t ON t.Employee_PayorderID = p.Employee_PayorderID
""", con);
        cmd.Parameters.AddWithValue("@Id", payorderId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task FillLinesAsync(
        SqlConnection con, int schoolId, MonthlyPayorderDto row, CancellationToken cancellationToken)
    {
        row.Allowances = await ReadLinesAsync(con, """
SELECT ISNULL(r.AllowanceID, 0) AS Id,
       ISNULL(NULLIF(LTRIM(RTRIM(a.AllowanceName)), N''), N'—') AS Name,
       ISNULL(r.AllowanceAmount, 0) AS Amount
FROM dbo.Employee_Allowance_Records AS r
LEFT JOIN dbo.Employee_Allowance AS a ON a.AllowanceID = r.AllowanceID
WHERE r.Employee_PayorderID = @Id
""", schoolId, row.EmployeePayorderID, cancellationToken);

        row.Deductions = await ReadLinesAsync(con, """
SELECT ISNULL(r.DeductionID, 0) AS Id,
       ISNULL(NULLIF(LTRIM(RTRIM(d.DeductionName)), N''), N'—') AS Name,
       ISNULL(r.Deduction_Amount, 0) AS Amount
FROM dbo.Employee_Deduction_Records AS r
LEFT JOIN dbo.Employee_Deduction AS d ON d.DeductionID = r.DeductionID
WHERE r.Employee_PayorderID = @Id
""", schoolId, row.EmployeePayorderID, cancellationToken);

        row.Bonuses = await ReadLinesAsync(con, """
SELECT b.BonusID AS Id, b.BonusName AS Name, ISNULL(t.Bonus_Amount, 0) AS Amount
FROM dbo.Employee_Bonus AS b
LEFT JOIN dbo.Employee_Bonus_Records AS t
    ON t.BonusID = b.BonusID AND t.Employee_PayorderID = @Id
WHERE b.SchoolID = @SchoolID
UNION ALL
SELECT ISNULL(r.BonusID, 0) AS Id,
       ISNULL(NULLIF(LTRIM(RTRIM(b.BonusName)), N''), N'—') AS Name,
       ISNULL(r.Bonus_Amount, 0) AS Amount
FROM dbo.Employee_Bonus_Records AS r
LEFT JOIN dbo.Employee_Bonus AS b ON b.BonusID = r.BonusID
WHERE r.Employee_PayorderID = @Id
  AND NOT EXISTS (
      SELECT 1 FROM dbo.Employee_Bonus AS x
      WHERE x.BonusID = r.BonusID AND x.SchoolID = @SchoolID)
""", schoolId, row.EmployeePayorderID, cancellationToken);

        row.Fines = await ReadLinesAsync(con, """
SELECT f.FineID AS Id, f.FineName AS Name, ISNULL(t.Fine_Amount, 0) AS Amount
FROM dbo.Employee_Fine AS f
LEFT JOIN dbo.Employee_Fine_Records AS t
    ON t.FineID = f.FineID AND t.Employee_PayorderID = @Id
WHERE f.SchoolID = @SchoolID
UNION ALL
SELECT ISNULL(r.FineID, 0) AS Id,
       ISNULL(NULLIF(LTRIM(RTRIM(f.FineName)), N''), N'—') AS Name,
       ISNULL(r.Fine_Amount, 0) AS Amount
FROM dbo.Employee_Fine_Records AS r
LEFT JOIN dbo.Employee_Fine AS f ON f.FineID = r.FineID
WHERE r.Employee_PayorderID = @Id
  AND NOT EXISTS (
      SELECT 1 FROM dbo.Employee_Fine AS x
      WHERE x.FineID = r.FineID AND x.SchoolID = @SchoolID)
""", schoolId, row.EmployeePayorderID, cancellationToken);
    }

    private static async Task<List<SalaryLineDto>> ReadLinesAsync(
        SqlConnection con, string sql, int schoolId, int payorderId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Id", payorderId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var items = new List<SalaryLineDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SalaryLineDto
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"]?.ToString() ?? "",
                Amount = ToDec(reader["Amount"])
            });
        }

        return items;
    }

    private static async Task<bool> NameExistsAsync(
        SqlConnection con, string table, string nameCol, int schoolId, string name, int? exceptId, string idCol,
        CancellationToken cancellationToken)
    {
        var sql = exceptId is null
            ? $"SELECT 1 FROM dbo.{table} WHERE SchoolID = @SchoolID AND {nameCol} = @Name"
            : $"SELECT 1 FROM dbo.{table} WHERE SchoolID = @SchoolID AND {nameCol} = @Name AND {idCol} <> @Id";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Name", name);
        if (exceptId is not null)
            cmd.Parameters.AddWithValue("@Id", exceptId.Value);
        return await cmd.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
    }

    private static async Task<bool> InUseAsync(
        SqlConnection con, string kind, int schoolId, int id, CancellationToken cancellationToken)
    {
        var sql = kind.ToLowerInvariant() switch
        {
            "allowance" => "SELECT 1 FROM dbo.Employee_Allowance_Assign WHERE SchoolID = @SchoolID AND AllowanceID = @Id",
            "deduction" => """
SELECT 1 WHERE EXISTS (SELECT 1 FROM dbo.Employee_Deduction_Assign WHERE DeductionID = @Id)
   OR EXISTS (SELECT 1 FROM dbo.Employee_Deduction_Records WHERE DeductionID = @Id)
""",
            "payorder" => "SELECT 1 FROM dbo.Employee_Payorder WHERE SchoolID = @SchoolID AND Employee_Payorder_NameID = @Id",
            _ => null
        };
        if (sql is null)
            return false;

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
    }

    private static (string Table, string IdCol, string NameCol)? Map(string kind) =>
        kind.ToLowerInvariant() switch
        {
            "allowance" => ("Employee_Allowance", "AllowanceID", "AllowanceName"),
            "deduction" => ("Employee_Deduction", "DeductionID", "DeductionName"),
            "bonus" => ("Employee_Bonus", "BonusID", "BonusName"),
            "fine" => ("Employee_Fine", "FineID", "FineName"),
            "payorder" => ("Employee_Payorder_Name", "Employee_Payorder_NameID", "Payorder_Name"),
            _ => null
        };

    private static string NormalizeType(string? type)
    {
        type = (type ?? "").Trim();
        if (string.Equals(type, "Teacher", StringComparison.OrdinalIgnoreCase))
            return "Teacher";
        if (string.Equals(type, "Staff", StringComparison.OrdinalIgnoreCase))
            return "Staff";
        return "%";
    }

    private static string? NullString(object value)
    {
        var text = value is DBNull ? null : value?.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static decimal ToDec(object? value) =>
        value is null or DBNull ? 0 : Convert.ToDecimal(value);

    private static SalaryResult Fail(string error) => new() { Succeeded = false, Error = error };
}
