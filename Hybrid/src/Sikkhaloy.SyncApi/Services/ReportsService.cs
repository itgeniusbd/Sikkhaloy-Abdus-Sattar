using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class ReportsService
{
    private readonly EduConnectionFactory _connections;

    public ReportsService(EduConnectionFactory connections) => _connections = connections;

    private static decimal ToDec(object? value) => value is DBNull or null ? 0 : Convert.ToDecimal(value);
    private static int ToInt(object? value) => value is DBNull or null ? 0 : Convert.ToInt32(value);
    private static string Text(object? value) => value is DBNull or null ? "" : value.ToString() ?? "";
    private static DateTime Day(object? value) => value is DBNull or null ? DateTime.MinValue : Convert.ToDateTime(value);
    private static string Like(string? value) => string.IsNullOrWhiteSpace(value) || value == "0" ? "%" : value.Trim();

    private static void AddDates(SqlCommand cmd, DateTime? from, DateTime? to)
    {
        var fromP = cmd.Parameters.Add("@From", SqlDbType.Date);
        fromP.Value = from.HasValue ? from.Value.Date : DBNull.Value;
        var toP = cmd.Parameters.Add("@To", SqlDbType.Date);
        toP.Value = to.HasValue ? to.Value.Date : DBNull.Value;
    }

    private static void AddSchool(SqlCommand cmd, SessionSnapshot session) =>
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);

    // Tuition current-due is EndDate before today. Inventory due is collectable the same day.
    private const string CurrentDueWhen = """
(
  po.EndDate < CAST(GETDATE() AS date)
  OR EXISTS (
    SELECT 1 FROM dbo.Income_Roles AS invr
    WHERE invr.RoleID = po.RoleID AND invr.Role = N'Inventory Sale'
  )
)
""";

    // All-session account reports: legacy rows may have NULL/wrong AccountID (old education years).
    private const string AccountLogScopeCorrelated = """
(
  AccountID = Account.AccountID
  OR (AccountID IS NULL AND Account.AccountID = (SELECT MIN(a2.AccountID) FROM Account a2 WHERE a2.SchoolID = @SchoolID))
  OR (SELECT COUNT(*) FROM Account a3 WHERE a3.SchoolID = @SchoolID) = 1
)
""";

    private const string AccountLogScopeParam = """
(
  AccountID = @AccountID
  OR (AccountID IS NULL AND @AccountID = (SELECT MIN(a2.AccountID) FROM Account a2 WHERE a2.SchoolID = @SchoolID))
  OR (SELECT COUNT(*) FROM Account a3 WHERE a3.SchoolID = @SchoolID) = 1
)
""";

    private async Task<decimal> ScalarAsync(SqlConnection con, string sql, Action<SqlCommand> bind, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            bind(cmd);
            return ToDec(await cmd.ExecuteScalarAsync(ct));
        }
        catch
        {
            return 0;
        }
    }

    public async Task<AccountsSummaryDto> GetSummaryAsync(SessionSnapshot session, CancellationToken ct)
    {
        var headTask = LoadSummaryHeadAsync(session, ct);
        var usersTask = WithConnectionAsync(con => QueryNamesAsync(con, """
SELECT RegistrationID AS Id, Name, Income AS Amount, Expense AS Amount2
FROM (
    SELECT User_T.RegistrationID,
           User_T.Name,
           ISNULL(EX_In_T.Other_Income, 0) + ISNULL(Com_In_T.CommitteeDonation, 0) + ISNULL(Stu_P_T.Student_Income, 0) AS Income,
           ISNULL(Ex_T.Expenditure, 0) + ISNULL(Emp_P_T.Employee_Paid, 0) AS Expense
    FROM (
        SELECT DISTINCT Registration.RegistrationID,
               ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name
        FROM Registration
        INNER JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
        WHERE Registration.SchoolID = @SchoolID
    ) AS User_T
    LEFT JOIN (SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income FROM Extra_Income WHERE SchoolID = @SchoolID GROUP BY RegistrationID) EX_In_T ON User_T.RegistrationID = EX_In_T.RegistrationID
    LEFT JOIN (SELECT RegistrationID, ISNULL(SUM(TotalAmount), 0) AS CommitteeDonation FROM CommitteeMoneyReceipt WHERE SchoolID = @SchoolID GROUP BY RegistrationID) Com_In_T ON User_T.RegistrationID = Com_In_T.RegistrationID
    LEFT JOIN (SELECT RegistrationID, ISNULL(SUM(PaidAmount), 0) AS Student_Income FROM Income_PaymentRecord WHERE SchoolID = @SchoolID GROUP BY RegistrationID) Stu_P_T ON User_T.RegistrationID = Stu_P_T.RegistrationID
    LEFT JOIN (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Expenditure FROM Expenditure WHERE SchoolID = @SchoolID GROUP BY RegistrationID) Ex_T ON User_T.RegistrationID = Ex_T.RegistrationID
    LEFT JOIN (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Employee_Paid FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID GROUP BY RegistrationID) Emp_P_T ON User_T.RegistrationID = Emp_P_T.RegistrationID
) T
WHERE T.Income <> 0 OR T.Expense <> 0
""", session, ct, commandTimeout: ReportCommandTimeout), ct);
        var accountsTask = WithConnectionAsync(con => QueryNamesAsync(con, """
SELECT AccountID AS Id, AccountName AS Name, AccountBalance AS Amount, 0 AS Amount2
FROM Account WHERE SchoolID = @SchoolID
""", session, ct, commandTimeout: ReportCommandTimeout), ct);
        var incomeMainTask = WithConnectionAsync(con => QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, SUM(Total) AS Amount, 0 AS Amount2
FROM (
    SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Total
    FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
    WHERE Income_PaymentRecord.SchoolID = @SchoolID
    GROUP BY Income_Roles.Role
    UNION ALL
    SELECT Extra_IncomeCategory.Extra_Income_CategoryName, SUM(Extra_Income.Extra_IncomeAmount)
    FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
    WHERE Extra_Income.SchoolID = @SchoolID
    GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName
) t GROUP BY Category ORDER BY Category
""", session, ct, commandTimeout: ReportCommandTimeout), ct);
        var incomeDonationTask = WithConnectionAsync(con => QueryNamesAsync(con, """
SELECT 0 AS Id, CommitteeDonationCategory.DonationCategory AS Name, SUM(CommitteePaymentRecord.PaidAmount) AS Amount, 0 AS Amount2
FROM CommitteePaymentRecord
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
WHERE CommitteePaymentRecord.SchoolId = @SchoolID
GROUP BY CommitteeDonationCategory.DonationCategory
""", session, ct, safe: true, commandTimeout: ReportCommandTimeout), ct);
        var expenseTask = WithConnectionAsync(con => QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, SUM(Amount) AS Amount, 0 AS Amount2 FROM (
    SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Amount
    FROM Employee_Payorder_Records
    INNER JOIN Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID
    INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
    WHERE Employee_Payorder_Records.SchoolID = @SchoolID
    GROUP BY Employee_Payorder_Name.Payorder_Name
    UNION ALL
    SELECT Expense_CategoryName.CategoryName, SUM(Expenditure.Amount)
    FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
    WHERE Expenditure.SchoolID = @SchoolID
    GROUP BY Expense_CategoryName.CategoryName
) t GROUP BY Category ORDER BY Category
""", session, ct, commandTimeout: ReportCommandTimeout), ct);

        var sessionsTask = LoadSessionsAsync(session, ct);
        await Task.WhenAll(headTask, usersTask, accountsTask, incomeMainTask, incomeDonationTask, expenseTask, sessionsTask);

        var dto = await headTask;
        dto.Users = await usersTask;
        dto.Accounts = await accountsTask;
        dto.IncomeCategories = await incomeMainTask;
        dto.IncomeCategories.AddRange(await incomeDonationTask);
        dto.ExpenseCategories = await expenseTask;
        dto.Sessions = await sessionsTask;
        return dto;
    }

    private const int ReportCommandTimeout = 120;
    private static void SetReportTimeout(SqlCommand cmd) => cmd.CommandTimeout = ReportCommandTimeout;

    private async Task<AccountsSummaryDto> LoadSummaryHeadAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new AccountsSummaryDto();
        const string headSql = """
SELECT
    ISNULL((SELECT SUM(Extra_IncomeAmount) FROM Extra_Income WHERE SchoolID = @SchoolID), 0)
  + ISNULL((SELECT SUM(PaidAmount) FROM Income_PaymentRecord WHERE SchoolID = @SchoolID), 0)
  + ISNULL((SELECT SUM(TotalAmount) FROM CommitteeMoneyReceipt WHERE SchoolId = @SchoolID), 0) AS TotalIncome,
    ISNULL((SELECT SUM(Amount) FROM Expenditure WHERE SchoolID = @SchoolID), 0)
  + ISNULL((SELECT SUM(Amount) FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID), 0) AS TotalExpense,
    ISNULL((SELECT SUM(AccountBalance) FROM Account WHERE SchoolID = @SchoolID), 0) AS AccountTotal
""";
        const string headFallbackSql = """
SELECT
    ISNULL((SELECT SUM(Extra_IncomeAmount) FROM Extra_Income WHERE SchoolID = @SchoolID), 0)
  + ISNULL((SELECT SUM(PaidAmount) FROM Income_PaymentRecord WHERE SchoolID = @SchoolID), 0) AS TotalIncome,
    ISNULL((SELECT SUM(Amount) FROM Expenditure WHERE SchoolID = @SchoolID), 0)
  + ISNULL((SELECT SUM(Amount) FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID), 0) AS TotalExpense,
    ISNULL((SELECT SUM(AccountBalance) FROM Account WHERE SchoolID = @SchoolID), 0) AS AccountTotal
""";
        try
        {
            await using var cmd = new SqlCommand(headSql, con);
            SetReportTimeout(cmd);
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.TotalIncome = ToDec(reader["TotalIncome"]);
                dto.TotalExpense = ToDec(reader["TotalExpense"]);
                dto.AccountTotal = ToDec(reader["AccountTotal"]);
                dto.Net = dto.TotalIncome - dto.TotalExpense;
            }
        }
        catch
        {
            await using var cmd = new SqlCommand(headFallbackSql, con);
            SetReportTimeout(cmd);
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.TotalIncome = ToDec(reader["TotalIncome"]);
                dto.TotalExpense = ToDec(reader["TotalExpense"]);
                dto.AccountTotal = ToDec(reader["AccountTotal"]);
                dto.Net = dto.TotalIncome - dto.TotalExpense;
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT
       ISNULL(SUM(CASE WHEN Is_Active = 1 THEN Amount ELSE 0 END), 0) AS TotalFee,
       ISNULL(SUM(CASE WHEN Is_Active = 1 THEN LateFeeCountable ELSE 0 END), 0) AS LateFee,
       ISNULL(SUM(CASE WHEN Is_Active = 1 THEN Total_Discount ELSE 0 END), 0) AS Concession,
       ISNULL(SUM(CASE WHEN Is_Active = 1 THEN PaidAmount ELSE 0 END), 0) AS Paid,
       ISNULL(SUM(CASE WHEN Is_Active = 1 THEN Receivable_Amount ELSE 0 END), 0) AS Unpaid,
       ISNULL(SUM(CASE WHEN Is_Active = 1 AND EndDate < GETDATE() THEN Receivable_Amount ELSE 0 END), 0) AS PresentDue,
       ISNULL(SUM(CASE WHEN StartDate > GETDATE() THEN PaidAmount ELSE 0 END), 0) AS Advance
FROM Income_PayOrder
WHERE SchoolID = @SchoolID
""", con))
        {
            SetReportTimeout(cmd);
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Payorder = ToDec(reader["TotalFee"]);
                dto.LateFee = ToDec(reader["LateFee"]);
                dto.Concession = ToDec(reader["Concession"]);
                dto.Paid = ToDec(reader["Paid"]);
                dto.Unpaid = ToDec(reader["Unpaid"]);
                dto.PresentDue = ToDec(reader["PresentDue"]);
                dto.Advance = ToDec(reader["Advance"]);
            }
        }

        return dto;
    }

    private async Task<T> WithConnectionAsync<T>(Func<SqlConnection, Task<T>> work, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await work(con);
    }

    private async Task<List<SessionReportDto>> LoadSessionsAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var items = await ReadSessionListAsync(con, session, ct);
        if (items.Count == 0)
            return items;

        var payMap = new Dictionary<int, (decimal Payorder, decimal LateFee, decimal Concession, decimal Paid, decimal Unpaid)>();
        await using (var pay = new SqlCommand("""
SELECT EducationYearID,
       ISNULL(SUM(Amount), 0) AS Payorder,
       ISNULL(SUM(LateFeeCountable), 0) AS LateFee,
       ISNULL(SUM(Total_Discount), 0) AS Concession,
       ISNULL(SUM(PaidAmount), 0) AS Paid,
       ISNULL(SUM(Receivable_Amount), 0) AS Unpaid
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND Is_Active = 1
GROUP BY EducationYearID
""", con))
        {
            SetReportTimeout(pay);
            AddSchool(pay, session);
            await using var reader = await pay.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                payMap[ToInt(reader["EducationYearID"])] = (
                    ToDec(reader["Payorder"]),
                    ToDec(reader["LateFee"]),
                    ToDec(reader["Concession"]),
                    ToDec(reader["Paid"]),
                    ToDec(reader["Unpaid"]));
            }
        }

        var monthMap = await LoadAllSessionMonthsAsync(con, session, items, ct);
        foreach (var year in items)
        {
            if (payMap.TryGetValue(year.EducationYearID, out var pay))
            {
                year.Payorder = pay.Payorder;
                year.LateFee = pay.LateFee;
                year.Concession = pay.Concession;
                year.Paid = pay.Paid;
                year.Unpaid = pay.Unpaid;
            }
            if (monthMap.TryGetValue(year.EducationYearID, out var months))
                year.Months = months;
        }

        return items;
    }

    private async Task<List<SessionReportDto>> ReadSessionListAsync(SqlConnection con, SessionSnapshot session, CancellationToken ct)
    {
        const string sessionSql = """
SELECT ey.EducationYearID, ey.EducationYear, ey.StartDate, ey.EndDate,
       ISNULL(SUM(t.Income), 0) AS Income,
       ISNULL(SUM(t.Expense), 0) AS Expense
FROM Education_Year ey
LEFT JOIN (
    SELECT EducationYearID, Extra_IncomeAmount AS Income, CAST(0 AS money) AS Expense
    FROM Extra_Income WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearID, PaidAmount, 0 FROM Income_PaymentRecord WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearID, 0, Amount FROM Expenditure WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearID, 0, Amount FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearId, TotalAmount, 0 FROM CommitteeMoneyReceipt WHERE SchoolId = @SchoolID
) t ON ey.EducationYearID = t.EducationYearID
WHERE ey.SchoolID = @SchoolID
GROUP BY ey.EducationYearID, ey.EducationYear, ey.StartDate, ey.EndDate
HAVING ISNULL(SUM(t.Income), 0) <> 0 OR ISNULL(SUM(t.Expense), 0) <> 0
ORDER BY ey.StartDate DESC
""";
        const string fallbackSql = """
SELECT ey.EducationYearID, ey.EducationYear, ey.StartDate, ey.EndDate,
       ISNULL(SUM(t.Income), 0) AS Income,
       ISNULL(SUM(t.Expense), 0) AS Expense
FROM Education_Year ey
LEFT JOIN (
    SELECT EducationYearID, Extra_IncomeAmount AS Income, CAST(0 AS money) AS Expense
    FROM Extra_Income WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearID, PaidAmount, 0 FROM Income_PaymentRecord WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearID, 0, Amount FROM Expenditure WHERE SchoolID = @SchoolID
    UNION ALL
    SELECT EducationYearID, 0, Amount FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID
) t ON ey.EducationYearID = t.EducationYearID
WHERE ey.SchoolID = @SchoolID
GROUP BY ey.EducationYearID, ey.EducationYear, ey.StartDate, ey.EndDate
HAVING ISNULL(SUM(t.Income), 0) <> 0 OR ISNULL(SUM(t.Expense), 0) <> 0
ORDER BY ey.StartDate DESC
""";
        return await ReadSessionRowsAsync(con, session, sessionSql, ct)
               ?? await ReadSessionRowsAsync(con, session, fallbackSql, ct)
               ?? [];
    }

    private async Task<List<SessionReportDto>?> ReadSessionRowsAsync(
        SqlConnection con, SessionSnapshot session, string sql, CancellationToken ct)
    {
        var items = new List<SessionReportDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            SetReportTimeout(cmd);
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var income = ToDec(reader["Income"]);
                var expense = ToDec(reader["Expense"]);
                items.Add(new SessionReportDto
                {
                    EducationYearID = ToInt(reader["EducationYearID"]),
                    YearName = Text(reader["EducationYear"]),
                    StartDate = Day(reader["StartDate"]),
                    EndDate = Day(reader["EndDate"]),
                    Income = income,
                    Expense = expense,
                    Net = income - expense
                });
            }
            return items;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Dictionary<int, List<NameAmountDto>>> LoadAllSessionMonthsAsync(
        SqlConnection con, SessionSnapshot session, IReadOnlyList<SessionReportDto> years, CancellationToken ct)
    {
        var map = new Dictionary<int, List<NameAmountDto>>();
        if (years.Count == 0)
            return map;

        var start = years.Min(y => y.StartDate.Date);
        var endEx = years.Max(y => y.EndDate.Date).AddDays(1);
        var rows = await QueryMonthTotalsAsync(con, session, start, endEx, includeCommittee: true, ct)
                   ?? await QueryMonthTotalsAsync(con, session, start, endEx, includeCommittee: false, ct)
                   ?? [];

        foreach (var year in years)
        {
            var from = year.StartDate.Date;
            var to = year.EndDate.Date;
            var months = new List<NameAmountDto>();
            foreach (var row in rows)
            {
                if (row.Month < from || row.Month > to)
                    continue;
                months.Add(new NameAmountDto
                {
                    Name = row.Month.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Amount = row.Income,
                    Amount2 = row.Expense
                });
            }
            if (months.Count > 0)
                map[year.EducationYearID] = months;
        }

        return map;
    }

    private async Task<List<(DateTime Month, decimal Income, decimal Expense)>?> QueryMonthTotalsAsync(
        SqlConnection con, SessionSnapshot session, DateTime start, DateTime endEx, bool includeCommittee, CancellationToken ct)
    {
        var committee = includeCommittee
            ? """
    UNION ALL
    SELECT DATEFROMPARTS(YEAR(PaidDate), MONTH(PaidDate), 1), TotalAmount, 0
    FROM CommitteeMoneyReceipt
    WHERE SchoolId = @SchoolID AND PaidDate >= @Start AND PaidDate < @EndEx
"""
            : "";
        var sql = $"""
SELECT MonthStart, SUM(Income) AS Income, SUM(Expense) AS Expense
FROM (
    SELECT DATEFROMPARTS(YEAR(Extra_IncomeDate), MONTH(Extra_IncomeDate), 1) AS MonthStart,
           Extra_IncomeAmount AS Income, CAST(0 AS money) AS Expense
    FROM Extra_Income
    WHERE SchoolID = @SchoolID AND Extra_IncomeDate >= @Start AND Extra_IncomeDate < @EndEx
    UNION ALL
    SELECT DATEFROMPARTS(YEAR(PaidDate), MONTH(PaidDate), 1), PaidAmount, 0
    FROM Income_PaymentRecord
    WHERE SchoolID = @SchoolID AND PaidDate >= @Start AND PaidDate < @EndEx
    UNION ALL
    SELECT DATEFROMPARTS(YEAR(ExpenseDate), MONTH(ExpenseDate), 1), 0, Amount
    FROM Expenditure
    WHERE SchoolID = @SchoolID AND ExpenseDate >= @Start AND ExpenseDate < @EndEx
    UNION ALL
    SELECT DATEFROMPARTS(YEAR(Paid_date), MONTH(Paid_date), 1), 0, Amount
    FROM Employee_Payorder_Records
    WHERE SchoolID = @SchoolID AND Paid_date >= @Start AND Paid_date < @EndEx
    {committee}
) t
GROUP BY MonthStart
ORDER BY MonthStart
""";
        var rows = new List<(DateTime Month, decimal Income, decimal Expense)>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            SetReportTimeout(cmd);
            AddSchool(cmd, session);
            cmd.Parameters.Add("@Start", SqlDbType.DateTime).Value = start;
            cmd.Parameters.Add("@EndEx", SqlDbType.DateTime).Value = endEx;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add((Day(reader["MonthStart"]), ToDec(reader["Income"]), ToDec(reader["Expense"])));
            }
            return rows;
        }
        catch
        {
            return null;
        }
    }

    public async Task<MonthBasedDto> GetMonthBasedAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, int classId, string? sectionId, string? roleIds,
        bool students, bool money, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new MonthBasedDto { SchoolName = session.SchoolName };
        if (money)
            await FillMoneyAsync(con, session, from, to, dto, ct);
        if (students && classId > 0)
            await FillStudentsAsync(con, session, classId, sectionId, roleIds, dto, ct);
        return dto;
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListMonthRolesAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT DISTINCT Income_Roles.RoleID AS Id, Income_Roles.Role AS Name, 0 AS Amount, 0 AS Amount2
FROM Income_PayOrder
INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID
WHERE Income_PayOrder.SchoolID = @SchoolID
  AND Income_PayOrder.EducationYearID = @YearID
  AND (@ClassID = 0 OR StudentsClass.ClassID = @ClassID)
ORDER BY Income_Roles.Role
""", session, ct, extra: c =>
        {
            c.Parameters.AddWithValue("@YearID", session.EducationYearID);
            c.Parameters.AddWithValue("@ClassID", classId);
        });
    }

    private async Task FillMoneyAsync(SqlConnection con, SessionSnapshot session, DateTime? from, DateTime? to, MonthBasedDto dto, CancellationToken ct)
    {
        var income = await ReadPeriodRowsAsync(con, """
SELECT PeriodDate, Category, SUM(Amount) AS Amount FROM (
    SELECT CAST(Income_PaymentRecord.PaidDate AS DATE) AS PeriodDate, Income_Roles.Role AS Category, Income_PaymentRecord.PaidAmount AS Amount
    FROM Income_PaymentRecord
    INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
    WHERE Income_PaymentRecord.SchoolID = @SchoolID
      AND CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    UNION ALL
    SELECT Extra_Income.Extra_IncomeDate, Extra_IncomeCategory.Extra_Income_CategoryName, Extra_Income.Extra_IncomeAmount
    FROM Extra_Income
    INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
    WHERE Extra_Income.SchoolID = @SchoolID
      AND Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
) t
GROUP BY PeriodDate, Category
""", session, from, to, ct);
        income.AddRange(await ReadPeriodRowsAsync(con, """
SELECT CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS PeriodDate, CommitteeDonationCategory.DonationCategory AS Category,
       SUM(CommitteePaymentRecord.PaidAmount) AS Amount
FROM CommitteePaymentRecord
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
INNER JOIN CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId
WHERE CommitteeMoneyReceipt.SchoolID = @SchoolID
  AND CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY CAST(CommitteeMoneyReceipt.PaidDate AS DATE), CommitteeDonationCategory.DonationCategory
""", session, from, to, ct, safe: true));

        var expense = await ReadPeriodRowsAsync(con, """
SELECT PeriodDate, Category, SUM(Amount) AS Amount FROM (
    SELECT CAST(Expenditure.ExpenseDate AS DATE) AS PeriodDate, Expense_CategoryName.CategoryName AS Category, Expenditure.Amount
    FROM Expenditure
    INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
    WHERE Expenditure.SchoolID = @SchoolID
      AND Expenditure.ExpenseDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    UNION ALL
    SELECT CAST(Employee_Payorder_Records.Paid_date AS DATE), Employee_Payorder_Name.Payorder_Name, Employee_Payorder_Records.Amount
    FROM Employee_Payorder_Records
    INNER JOIN Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID
    INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
    WHERE Employee_Payorder_Records.SchoolID = @SchoolID
      AND Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
) t
GROUP BY PeriodDate, Category
""", session, from, to, ct);

        dto.IncomeDaily = PivotMatrix(income.Select(x => (x.Category, FormatDay(x.Day), x.Amount)));
        dto.IncomeMonthly = PivotMatrix(income.Select(x => (x.Category, FormatMonth(x.Day), x.Amount)));
        dto.ExpenseDaily = PivotMatrix(expense.Select(x => (x.Category, FormatDay(x.Day), x.Amount)));
        dto.ExpenseMonthly = PivotMatrix(expense.Select(x => (x.Category, FormatMonth(x.Day), x.Amount)));
    }

    private async Task FillStudentsAsync(
        SqlConnection con, SessionSnapshot session, int classId, string? sectionId, string? roleIds, MonthBasedDto dto, CancellationToken ct)
    {
        dto.ClassName = Text(await ScalarTextAsync(con, """
SELECT Class FROM CreateClass WHERE ClassID = @ClassID AND SchoolID = @SchoolID
""", session, ct, c => c.Parameters.AddWithValue("@ClassID", classId)));

        var start = new DateTime(DateTime.Today.Year, 1, 1);
        var end = new DateTime(DateTime.Today.Year, 12, 31);
        await using (var yearCmd = new SqlCommand("""
SELECT StartDate, EndDate FROM Education_Year WHERE EducationYearID = @YearID AND SchoolID = @SchoolID
""", con))
        {
            AddSchool(yearCmd, session);
            yearCmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            await using var yearReader = await yearCmd.ExecuteReaderAsync(ct);
            if (await yearReader.ReadAsync(ct))
            {
                start = Day(yearReader["StartDate"]);
                end = Day(yearReader["EndDate"]);
                if (end < start) end = start;
            }
        }

        var months = new List<string>();
        for (var day = new DateTime(start.Year, start.Month, 1); day <= end; day = day.AddMonths(1))
            months.Add(FormatMonth(day));
        dto.Months = months;

        var ids = ParseRoleIds(roleIds);
        var section = string.IsNullOrWhiteSpace(sectionId) ? "%" : sectionId.Trim();
        var map = new Dictionary<int, MonthStudentRowDto>();

        await using (var stu = new SqlCommand("""
SELECT StudentsClass.StudentID, StudentsClass.RollNo, Student.ID, Student.StudentsName
FROM StudentsClass
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @YearID
  AND StudentsClass.ClassID = @ClassID
  AND StudentsClass.Class_Status IS NULL
  AND Student.Status = N'Active'
  AND (@SectionID = N'%' OR StudentsClass.SectionID LIKE @SectionID)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END, Student.ID
""", con))
        {
            AddSchool(stu, session);
            stu.Parameters.AddWithValue("@YearID", session.EducationYearID);
            stu.Parameters.AddWithValue("@ClassID", classId);
            stu.Parameters.AddWithValue("@SectionID", section);
            await using var reader = await stu.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var studentId = ToInt(reader["StudentID"]);
                map[studentId] = new MonthStudentRowDto
                {
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    RollNo = Text(reader["RollNo"])
                };
            }
        }

        var paySql = """
SELECT Income_PayOrder.StudentID, MIN(Income_PayOrder.EndDate) AS MonthDate, SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS Paid
FROM Income_PayOrder
INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID
WHERE Income_PayOrder.SchoolID = @SchoolID
  AND Income_PayOrder.EducationYearID = @YearID
  AND Income_PayOrder.Is_Active = 1
  AND StudentsClass.ClassID = @ClassID
  AND (@SectionID = N'%' OR StudentsClass.SectionID LIKE @SectionID)
""";
        if (ids.Count > 0)
            paySql += " AND Income_PayOrder.RoleID IN (" + string.Join(",", ids.Select((_, i) => "@R" + i)) + ")";
        paySql += """
 GROUP BY Income_PayOrder.StudentID, YEAR(Income_PayOrder.EndDate), MONTH(Income_PayOrder.EndDate)
""";

        await using (var pay = new SqlCommand(paySql, con))
        {
            AddSchool(pay, session);
            pay.Parameters.AddWithValue("@YearID", session.EducationYearID);
            pay.Parameters.AddWithValue("@ClassID", classId);
            pay.Parameters.AddWithValue("@SectionID", section);
            for (var i = 0; i < ids.Count; i++)
                pay.Parameters.AddWithValue("@R" + i, ids[i]);
            await using var reader = await pay.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var studentId = ToInt(reader["StudentID"]);
                if (!map.TryGetValue(studentId, out var row)) continue;
                var month = FormatMonth(Day(reader["MonthDate"]));
                if (!dto.Months.Contains(month, StringComparer.OrdinalIgnoreCase)) continue;
                var paid = ToDec(reader["Paid"]);
                row.Months[month] = paid;
                row.Total += paid;
            }
        }

        dto.Students = map.Values.ToList();
        foreach (var month in dto.Months)
            dto.MonthTotals[month] = dto.Students.Sum(s => s.Months.TryGetValue(month, out var amt) ? amt : 0);
        dto.GrandTotal = dto.Students.Sum(s => s.Total);
    }

    private async Task<List<(string Category, DateTime Day, decimal Amount)>> ReadPeriodRowsAsync(
        SqlConnection con, string sql, SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct, bool safe = false)
    {
        var items = new List<(string Category, DateTime Day, decimal Amount)>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            AddSchool(cmd, session);
            AddDates(cmd, from, to);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                items.Add((Text(reader["Category"]), Day(reader["PeriodDate"]), ToDec(reader["Amount"])));
        }
        catch when (safe)
        {
        }
        return items;
    }

    private async Task<string> ScalarTextAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null)
    {
        await using var cmd = new SqlCommand(sql, con);
        AddSchool(cmd, session);
        extra?.Invoke(cmd);
        return Text(await cmd.ExecuteScalarAsync(ct));
    }

    private static MonthMatrixDto PivotMatrix(IEnumerable<(string Row, string Column, decimal Amount)> rows)
    {
        var columns = new SortedSet<string>(PeriodComparer);
        var map = new Dictionary<string, MonthRoleRowDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var (row, column, amount) in rows)
        {
            if (string.IsNullOrWhiteSpace(column)) continue;
            columns.Add(column);
            if (!map.TryGetValue(row, out var item))
            {
                item = new MonthRoleRowDto { Role = row };
                map[row] = item;
            }
            item.Months[column] = item.Months.TryGetValue(column, out var prev) ? prev + amount : amount;
            item.Total += amount;
        }

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columns)
            totals[column] = map.Values.Sum(x => x.Months.TryGetValue(column, out var amt) ? amt : 0);
        return new MonthMatrixDto
        {
            Columns = columns.ToList(),
            Rows = map.Values.OrderBy(x => x.Role, StringComparer.OrdinalIgnoreCase).ToList(),
            ColumnTotals = totals,
            GrandTotal = map.Values.Sum(x => x.Total)
        };
    }

    private static List<int> ParseRoleIds(string? roleIds) =>
        (roleIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => int.TryParse(x, out var n) && n > 0)
            .Select(int.Parse)
            .Distinct()
            .ToList();

    private static readonly Comparer<string> PeriodComparer = Comparer<string>.Create((a, b) =>
    {
        if (DateTime.TryParse(a, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var da)
            && DateTime.TryParse(b, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var db))
            return da.CompareTo(db);
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    });

    private static string FormatDay(DateTime day) => day.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
    private static string FormatMonth(DateTime day) => day.ToString("MMM yyyy", CultureInfo.InvariantCulture);

    public async Task<IncomeExpenseReportDto> GetIncomeReportAsync(SessionSnapshot session, DateTime? from, DateTime? to, string? category, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var like = Like(category);
        var groups = await IncomeCategoryTotalsAsync(con, session, from, to, like, ct);
        var lines = await QueryLinesAsync(con, """
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, Income_Roles.Role AS Category,
       '[' + Student.ID + '] ' + Student.StudentsName + ', Class: ' + CreateClass.Class + ', For: ' + Income_PaymentRecord.PayFor AS Details,
       Income_PaymentRecord.PaidAmount AS Amount, CAST(Income_PaymentRecord.PaidDate AS DATE) AS [Date]
FROM Income_PaymentRecord
INNER JOIN Registration ON Income_PaymentRecord.RegistrationID = Registration.RegistrationID
INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
INNER JOIN Student ON Income_PaymentRecord.StudentID = Student.StudentID
INNER JOIN StudentsClass ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID
INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT JOIN Admin ON Admin.RegistrationID = Registration.RegistrationID
LEFT JOIN Account ON Income_PaymentRecord.AccountID = Account.AccountID
WHERE Income_PaymentRecord.SchoolID = @SchoolID
  AND CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Income_Roles.Role LIKE @Category
UNION ALL
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')',
       ISNULL(Account.AccountName, 'N/A'), Extra_IncomeCategory.Extra_Income_CategoryName,
       Extra_Income.Extra_IncomeFor, Extra_Income.Extra_IncomeAmount, Extra_Income.Extra_IncomeDate
FROM Extra_Income
INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
INNER JOIN Registration ON Extra_Income.RegistrationID = Registration.RegistrationID
LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
LEFT JOIN Account ON Extra_Income.AccountID = Account.AccountID
WHERE Extra_Income.SchoolID = @SchoolID
  AND Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Extra_IncomeCategory.Extra_Income_CategoryName LIKE @Category
ORDER BY [Date]
""", session, from, to, like, ct);
        lines.AddRange(await QueryLinesAsync(con, """
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, CommitteeDonationCategory.DonationCategory AS Category,
       CommitteeDonation.Description AS Details, CommitteePaymentRecord.PaidAmount AS Amount,
       CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS [Date]
FROM CommitteeMoneyReceipt
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
INNER JOIN Registration ON CommitteeMoneyReceipt.RegistrationId = Registration.RegistrationID
LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
LEFT JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID
WHERE CommitteeMoneyReceipt.SchoolID = @SchoolID
  AND CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND CommitteeDonationCategory.DonationCategory LIKE @Category
""", session, from, to, like, ct, safe: true));

        return Pack(groups, lines);
    }

    public async Task<IncomeExpenseReportDto> GetExpenseReportAsync(SessionSnapshot session, DateTime? from, DateTime? to, string? category, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var like = Like(category);
        var groups = await ExpenseCategoryTotalsAsync(con, session, from, to, like, ct);
        var lines = await QueryLinesAsync(con, """
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, Expense_CategoryName.CategoryName AS Category,
       Expenditure.ExpenseFor AS Details, Expenditure.Amount, Expenditure.ExpenseDate AS [Date]
FROM Expenditure
INNER JOIN Registration ON Expenditure.RegistrationID = Registration.RegistrationID
INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
INNER JOIN Admin ON Admin.RegistrationID = Registration.RegistrationID
LEFT JOIN Account ON Expenditure.AccountID = Account.AccountID
WHERE Expenditure.SchoolID = @SchoolID
  AND Expenditure.ExpenseDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Expense_CategoryName.CategoryName LIKE @Category
UNION ALL
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')',
       ISNULL(Account.AccountName, 'N/A'), Employee_Payorder_Name.Payorder_Name,
       Employee_Payorder_Records.Paid_For, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date
FROM Registration
INNER JOIN Admin ON Admin.RegistrationID = Registration.RegistrationID
INNER JOIN Employee_Payorder_Records ON Registration.RegistrationID = Employee_Payorder_Records.RegistrationID
INNER JOIN Employee_Payorder ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Records.Employee_PayorderID
INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
LEFT JOIN Account ON Employee_Payorder_Records.AccountID = Account.AccountID
WHERE Employee_Payorder_Records.SchoolID = @SchoolID
  AND Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Employee_Payorder_Name.Payorder_Name LIKE @Category
ORDER BY [Date]
""", session, from, to, like, ct);
        return Pack(groups, lines);
    }

    public async Task<NetReportDto> GetNetAsync(SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var income = await GetIncomeReportAsync(session, from, to, "%", ct);
        var expense = await GetExpenseReportAsync(session, from, to, "%", ct);
        var online = await ScalarAsync(con, """
SELECT ISNULL(SUM(t.TotalAmount), 0) FROM (
    SELECT a.PaidAmount AS TotalAmount
    FROM Income_PaymentRecord a
    INNER JOIN Account b ON a.AccountID = b.AccountID AND a.SchoolID = b.SchoolID
    WHERE b.AccountName = 'Online Payment' AND a.SchoolID = @SchoolID
      AND CAST(a.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    UNION ALL
    SELECT p.PaidAmount
    FROM CommitteeMoneyReceipt c
    INNER JOIN CommitteePaymentRecord p ON c.CommitteeMoneyReceiptId = p.CommitteeMoneyReceiptId
    INNER JOIN Account d ON c.AccountId = d.AccountID AND c.SchoolId = d.SchoolID
    WHERE d.AccountName = 'Online Payment' AND c.SchoolId = @SchoolID
      AND CAST(c.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
) t
""", c => { AddSchool(c, session); AddDates(c, from, to); }, ct);

        var classIncome = await QueryNamesAsync(con, """
SELECT StudentsClass.ClassID AS Id, CreateClass.Class AS Name, SUM(Income_PaymentRecord.PaidAmount) AS Amount, 0 AS Amount2
FROM Income_PaymentRecord
INNER JOIN StudentsClass ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID
INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE Income_PaymentRecord.SchoolID = @SchoolID
  AND CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY StudentsClass.ClassID, CreateClass.Class
ORDER BY StudentsClass.ClassID
""", session, ct, extra: c => AddDates(c, from, to));
        var invClassIncome = await QueryNamesAsync(con, """
SELECT sc.ClassID AS Id, cc.Class AS Name, SUM(ei.Extra_IncomeAmount) AS Amount, 0 AS Amount2
FROM Extra_Income ei
INNER JOIN Inv_Sale s ON s.ExtraIncomeID = ei.Extra_IncomeID AND s.SchoolID = ei.SchoolID
INNER JOIN Inv_Customer c ON c.CustomerID = s.CustomerID AND c.SchoolID = s.SchoolID
INNER JOIN StudentsClass sc ON sc.StudentID = c.StudentID
  AND sc.EducationYearID = s.EducationYearID
  AND sc.Class_Status IS NULL
INNER JOIN CreateClass cc ON cc.ClassID = sc.ClassID
WHERE ei.SchoolID = @SchoolID
  AND CAST(ei.Extra_IncomeDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND ISNULL(c.StudentID, 0) > 0
  AND ISNULL(ei.Extra_IncomeAmount, 0) > 0
GROUP BY sc.ClassID, cc.Class
""", session, ct, extra: c => AddDates(c, from, to), safe: true);
        classIncome = MergeNamedAmounts(classIncome, invClassIncome);

        var donations = await QueryNamesAsync(con, """
SELECT 0 AS Id, CommitteeDonationCategory.DonationCategory AS Name, SUM(CommitteePaymentRecord.PaidAmount) AS Amount, 0 AS Amount2
FROM CommitteeMoneyReceipt
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
WHERE CommitteeMoneyReceipt.SchoolId = @SchoolID
  AND CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY CommitteeDonationCategory.DonationCategory
""", session, ct, extra: c => AddDates(c, from, to), safe: true);

        return new NetReportDto
        {
            Income = income.Total,
            Expense = expense.Total,
            Online = online,
            CashInHand = income.Total - expense.Total - online,
            IncomeCategories = income.Groups.Select(g => new NameAmountDto { Name = g.Category, Amount = g.Total }).ToList(),
            ExpenseCategories = expense.Groups.Select(g => new NameAmountDto { Name = g.Category, Amount = g.Total }).ToList(),
            ClassIncome = classIncome,
            Donations = donations,
            IncomeDetails = income.Groups,
            ExpenseDetails = expense.Groups
        };
    }

    public async Task<CurrentDueDto> GetCurrentDueAsync(SessionSnapshot session, int classId, string? sectionId, string? roleId, string? studentId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new CurrentDueDto
        {
            InstitutionDue = await ScalarAsync(con, $"""
SELECT ISNULL(SUM(CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
    THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
    ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END), 0)
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND {CurrentDueWhen} AND s.Status = N'Active'
""", c =>
            {
                AddSchool(c, session);
                c.Parameters.AddWithValue("@YearID", session.EducationYearID);
            }, ct)
        };

        if (!string.IsNullOrWhiteSpace(studentId))
        {
            await using var byId = new SqlCommand($"""
SELECT po.StudentID, s.ID, s.StudentsName, sc.RollNo, s.SMSPhoneNo, CreateClass.Class,
       SUM(CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
           THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
           ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END) AS Due
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
INNER JOIN StudentsClass sc ON s.StudentID = sc.StudentID AND sc.EducationYearID = @YearID AND sc.Class_Status IS NULL
INNER JOIN CreateClass ON sc.ClassID = CreateClass.ClassID
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND {CurrentDueWhen} AND s.Status = N'Active' AND s.ID = @ID
GROUP BY po.StudentID, s.ID, s.StudentsName, sc.RollNo, s.SMSPhoneNo, CreateClass.Class
""", con);
            AddSchool(byId, session);
            byId.Parameters.AddWithValue("@YearID", session.EducationYearID);
            byId.Parameters.AddWithValue("@ID", studentId.Trim());
            dto.Students = await ReadDueRowsAsync(byId, ct);
            return dto;
        }

        if (classId <= 0)
            return dto;

        await using var cmd = new SqlCommand($"""
SELECT po.StudentID, s.ID, s.StudentsName, sc.RollNo, s.SMSPhoneNo,
       SUM(CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
           THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
           ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END) AS Due
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
INNER JOIN StudentsClass sc ON s.StudentID = sc.StudentID AND sc.EducationYearID = @YearID AND sc.ClassID = @ClassID AND sc.Class_Status IS NULL
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND {CurrentDueWhen} AND s.Status = N'Active'
  AND sc.SectionID LIKE @SectionID AND CAST(po.RoleID AS NVARCHAR(50)) LIKE @RoleID
GROUP BY s.StudentsName, s.ID, po.StudentID, s.SMSPhoneNo, sc.RollNo
ORDER BY CASE WHEN ISNUMERIC(sc.RollNo) = 1 THEN CAST(REPLACE(REPLACE(sc.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
        cmd.Parameters.AddWithValue("@RoleID", Like(roleId));
        dto.Students = await ReadDueRowsAsync(cmd, ct);
        return dto;
    }

    public async Task<CurrentDueStudentDetailDto> GetDueDetailsAsync(SessionSnapshot session, string studentCode, string? roleId, CancellationToken ct)
    {
        var dto = new CurrentDueStudentDetailDto { ID = (studentCode ?? "").Trim() };
        if (string.IsNullOrWhiteSpace(dto.ID))
            return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var like = Like(roleId);
        await using var cmd = new SqlCommand($"""
SELECT po.StudentID, Student.ID, Student.StudentsName, StudentsClass.RollNo, Student.SMSPhoneNo, CreateClass.Class,
       Income_Roles.Role, po.PayFor, po.Amount, po.LateFee, ISNULL(po.Discount, 0) AS Discount, po.PaidAmount, po.EndDate,
       CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
            THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
            ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END AS Due
FROM Income_PayOrder po
INNER JOIN Income_Roles ON po.RoleID = Income_Roles.RoleID
INNER JOIN Student ON po.StudentID = Student.StudentID
INNER JOIN StudentsClass ON po.StudentClassID = StudentsClass.StudentClassID
INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE po.SchoolID = @SchoolID AND po.Status = N'Due' AND po.Is_Active = 1 AND {CurrentDueWhen}
  AND Student.ID = @ID AND Student.Status = N'Active' AND CAST(po.RoleID AS NVARCHAR(50)) LIKE @RoleID
ORDER BY po.EndDate
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@ID", dto.ID);
        cmd.Parameters.AddWithValue("@RoleID", like);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var due = ToDec(reader["Due"]);
            if (due <= 0) continue;
            if (dto.StudentID == 0)
            {
                dto.StudentID = ToInt(reader["StudentID"]);
                dto.ID = Text(reader["ID"]);
                dto.Name = Text(reader["StudentsName"]);
                dto.RollNo = Text(reader["RollNo"]);
                dto.Phone = Text(reader["SMSPhoneNo"]);
                dto.ClassName = Text(reader["Class"]);
            }
            dto.Lines.Add(new CurrentDueLineDto
            {
                Role = Text(reader["Role"]),
                PayFor = Text(reader["PayFor"]),
                Amount = ToDec(reader["Amount"]),
                LateFee = ToDec(reader["LateFee"]),
                Discount = ToDec(reader["Discount"]),
                Paid = ToDec(reader["PaidAmount"]),
                Due = due,
                EndDate = Day(reader["EndDate"])
            });
            dto.Due += due;
        }
        var labels = await LoadInventoryPayForLabelsAsync(con, session, dto.Lines.Select(x => x.PayFor), ct);
        foreach (var line in dto.Lines)
            line.PayFor = PayForWithItems(line.PayFor, labels);
        return dto;
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListDueRolesAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        if (classId <= 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, $"""
SELECT DISTINCT ir.RoleID AS Id, ir.Role AS Name, 0 AS Amount, 0 AS Amount2
FROM Income_PayOrder po
INNER JOIN Income_Roles ir ON po.RoleID = ir.RoleID
INNER JOIN StudentsClass sc ON po.StudentID = sc.StudentID AND sc.EducationYearID = @YearID AND sc.ClassID = @ClassID AND sc.Class_Status IS NULL
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND {CurrentDueWhen}
ORDER BY ir.Role
""", session, ct, extra: c =>
        {
            c.Parameters.AddWithValue("@YearID", session.EducationYearID);
            c.Parameters.AddWithValue("@ClassID", classId);
        });
    }

    public async Task<PayorderReportDto> GetPayorderAsync(SessionSnapshot session, DateTime? from, DateTime? to, int roleId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var (fromDate, toDate) = await ResolvePayorderDatesAsync(con, session, from, to, ct);

        var dto = new PayorderReportDto();
        const string payWhere = """
SchoolID = @SchoolID AND Is_Active = 1 AND EndDate >= @From AND EndDate <= @To
""";
        const string payWhereJoin = """
Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.Is_Active = 1
  AND Income_PayOrder.EndDate >= @From AND Income_PayOrder.EndDate <= @To
""";

        await using (var tot = new SqlCommand($"""
SELECT ISNULL(SUM(Amount), 0), ISNULL(SUM(LateFeeCountable), 0), ISNULL(SUM(Total_Discount), 0), ISNULL(SUM(PaidAmount), 0),
       ISNULL(SUM(CASE WHEN Status = 'Paid' THEN 0
            WHEN EndDate < CAST(GETDATE() AS DATE) THEN ISNULL(Amount, 0) + ISNULL(LateFee, 0) - ISNULL(Discount, 0) - ISNULL(PaidAmount, 0) - ISNULL(LateFee_Discount, 0)
            ELSE ISNULL(Amount, 0) - ISNULL(Discount, 0) - ISNULL(PaidAmount, 0) END), 0)
FROM Income_PayOrder
WHERE {payWhere}
""", con))
        {
            SetReportTimeout(tot);
            AddSchool(tot, session);
            tot.Parameters.AddWithValue("@From", fromDate);
            tot.Parameters.AddWithValue("@To", toDate);
            await using var reader = await tot.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Payorder = ToDec(reader[0]);
                dto.LateFee = ToDec(reader[1]);
                dto.Concession = ToDec(reader[2]);
                dto.Paid = ToDec(reader[3]);
                dto.Unpaid = ToDec(reader[4]);
            }
        }

        await using var roles = new SqlCommand($"""
SELECT Income_Roles.RoleID, Income_Roles.Role,
       SUM(Income_PayOrder.Amount) AS Fee, SUM(LateFeeCountable) AS LateFee,
       SUM(Total_Discount) AS Concession, SUM(Income_PayOrder.PaidAmount) AS Paid, SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder
INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE {payWhereJoin}
GROUP BY Income_Roles.Role, Income_Roles.RoleID
ORDER BY Income_Roles.Role
""", con);
        SetReportTimeout(roles);
        AddSchool(roles, session);
        roles.Parameters.AddWithValue("@From", fromDate);
        roles.Parameters.AddWithValue("@To", toDate);
        var roleMap = new Dictionary<int, PayorderRoleDto>();
        await using (var reader = await roles.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var role = new PayorderRoleDto
                {
                    RoleID = ToInt(reader["RoleID"]),
                    Role = Text(reader["Role"]),
                    Fee = ToDec(reader["Fee"]),
                    LateFee = ToDec(reader["LateFee"]),
                    Concession = ToDec(reader["Concession"]),
                    Paid = ToDec(reader["Paid"]),
                    Unpaid = ToDec(reader["Unpaid"])
                };
                dto.Roles.Add(role);
                roleMap[role.RoleID] = role;
            }
        }

        await using var payFor = new SqlCommand($"""
SELECT RoleID, PayFor, SUM(Amount), SUM(LateFeeCountable), SUM(Total_Discount), SUM(PaidAmount), SUM(Receivable_Amount)
FROM Income_PayOrder
WHERE {payWhere}
GROUP BY RoleID, PayFor
ORDER BY RoleID, MAX(EndDate)
""", con);
        SetReportTimeout(payFor);
        AddSchool(payFor, session);
        payFor.Parameters.AddWithValue("@From", fromDate);
        payFor.Parameters.AddWithValue("@To", toDate);
        await using (var reader = await payFor.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var rid = ToInt(reader["RoleID"]);
                if (!roleMap.TryGetValue(rid, out var role)) continue;
                role.PayFors.Add(new PayorderRoleDto
                {
                    Role = Text(reader["PayFor"]),
                    Fee = ToDec(reader[2]),
                    LateFee = ToDec(reader[3]),
                    Concession = ToDec(reader[4]),
                    Paid = ToDec(reader[5]),
                    Unpaid = ToDec(reader[6])
                });
            }
        }

        return dto;
    }

    private async Task<(DateTime From, DateTime To)> ResolvePayorderDatesAsync(
        SqlConnection con, SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
        if (from.HasValue && to.HasValue)
            return (from.Value.Date, to.Value.Date);
        if (from.HasValue)
            return (from.Value.Date, from.Value.Date);
        if (to.HasValue)
            return (to.Value.Date, to.Value.Date);

        await using var cmd = new SqlCommand("""
SELECT StartDate, EndDate FROM Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            var start = Day(reader["StartDate"]);
            var end = Day(reader["EndDate"]);
            if (start != DateTime.MinValue && end != DateTime.MinValue)
                return (start.Date, end.Date);
        }

        var today = DateTime.Today;
        return (new DateTime(today.Year, today.Month, 1), today);
    }

    public async Task<PaidDetailsDto> GetPaidAsync(SessionSnapshot session, string? yearId, int classId, string? groupId, string? sectionId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new PaidDetailsDto
        {
            Total = await ScalarAsync(con, """
SELECT ISNULL(SUM(Income_PaymentRecord.PaidAmount), 0)
FROM Income_PaymentRecord
INNER JOIN Income_MoneyReceipt ON Income_PaymentRecord.MoneyReceiptID = Income_MoneyReceipt.MoneyReceiptID
INNER JOIN StudentsClass ON Income_MoneyReceipt.StudentClassID = StudentsClass.StudentClassID
WHERE StudentsClass.SchoolID = @SchoolID
  AND (StudentsClass.ClassID = @ClassID OR @ClassID = 0)
  AND Income_MoneyReceipt.EducationYearID LIKE @YearID
  AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @GroupID
  AND CAST(Income_MoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
""", c =>
            {
                AddSchool(c, session);
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@YearID", Like(yearId));
                c.Parameters.AddWithValue("@SectionID", Like(sectionId));
                c.Parameters.AddWithValue("@GroupID", Like(groupId));
                AddDates(c, from, to);
            }, ct)
        };

        await using var cmd = new SqlCommand("""
SELECT Student.ID, StudentsClass.RollNo, Student.StudentsName, CreateClass.Class, CreateSection.Section,
       Income_MoneyReceipt.MoneyReceipt_SN, CAST(Income_MoneyReceipt.PaidDate AS DATE) AS PaidDate,
       Income_MoneyReceipt.MoneyReceiptID, Income_MoneyReceipt.TotalAmount
FROM Income_MoneyReceipt
INNER JOIN StudentsClass ON Income_MoneyReceipt.StudentClassID = StudentsClass.StudentClassID
INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
LEFT JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE StudentsClass.SchoolID = @SchoolID
  AND (StudentsClass.ClassID = @ClassID OR @ClassID = 0)
  AND Income_MoneyReceipt.EducationYearID LIKE @YearID
  AND StudentsClass.SectionID LIKE @SectionID
  AND StudentsClass.SubjectGroupID LIKE @GroupID
  AND CAST(Income_MoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
ORDER BY StudentsClass.ClassID, CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS int) ELSE 0 END
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@YearID", Like(yearId));
        cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
        cmd.Parameters.AddWithValue("@GroupID", Like(groupId));
        AddDates(cmd, from, to);
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Receipts.Add(new PaidReceiptDto
                {
                    MoneyReceiptID = ToInt(reader["MoneyReceiptID"]),
                    ReceiptNo = Text(reader["MoneyReceipt_SN"]),
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    RollNo = Text(reader["RollNo"]),
                    ClassName = Text(reader["Class"]),
                    Section = Text(reader["Section"]),
                    TotalAmount = ToDec(reader["TotalAmount"]),
                    PaidDate = Day(reader["PaidDate"])
                });
            }
        }

        foreach (var rec in dto.Receipts)
        {
            await using var det = new SqlCommand("""
SELECT Income_Roles.Role, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidAmount, Education_Year.EducationYear
FROM Income_PaymentRecord
INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID
WHERE Income_PaymentRecord.MoneyReceiptID = @ID
""", con);
            det.Parameters.AddWithValue("@ID", rec.MoneyReceiptID);
            await using var reader = await det.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                rec.Details.Add($"{Text(reader["Role"])} For {Text(reader["PayFor"])} ({Text(reader["EducationYear"])}) :  {ToDec(reader["PaidAmount"]):0.##}");
        }

        return dto;
    }

    public async Task<MyAccountsDto> GetMyAccountsAsync(SessionSnapshot session, int registrationId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var regId = registrationId > 0 ? registrationId : session.RegistrationID;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new MyAccountsDto();
        await using (var cmd = new SqlCommand("""
SELECT User_T.Name, User_T.Designation,
       ISNULL(EX_In_T.Other_Income, 0) + ISNULL(Stu_P_T.Student_Income, 0) + ISNULL(Com_In_T.CommitteeDonation, 0) AS Income,
       ISNULL(Ex_T.Expenditure, 0) + ISNULL(Emp_P_T.Employee_Paid, 0) AS Expense,
       ISNULL(Sub_T.TotalSubmitted, 0) AS Submitted
FROM (
    SELECT Registration.RegistrationID, Admin.Designation,
           ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name
    FROM Registration LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
    WHERE Registration.SchoolID = @SchoolID AND Registration.RegistrationID = @RegID
) User_T
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income FROM Extra_Income
    WHERE SchoolID = @SchoolID AND Extra_IncomeDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) EX_In_T ON User_T.RegistrationID = EX_In_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(PaidAmount), 0) AS Student_Income FROM Income_PaymentRecord
    WHERE SchoolID = @SchoolID AND CAST(PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Stu_P_T ON User_T.RegistrationID = Stu_P_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Expenditure FROM Expenditure
    WHERE SchoolID = @SchoolID AND ExpenseDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Ex_T ON User_T.RegistrationID = Ex_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Employee_Paid FROM Employee_Payorder_Records
    WHERE SchoolID = @SchoolID AND Paid_date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Emp_P_T ON User_T.RegistrationID = Emp_P_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationId, ISNULL(SUM(TotalAmount), 0) AS CommitteeDonation FROM CommitteeMoneyReceipt
    WHERE SchoolId = @SchoolID AND CAST(PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationId
) Com_In_T ON User_T.RegistrationID = Com_In_T.RegistrationId
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(SubmissionAmount), 0) AS TotalSubmitted FROM User_Balance_Submission
    WHERE SchoolID = @SchoolID AND SubmissionDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Sub_T ON User_T.RegistrationID = Sub_T.RegistrationID
""", con))
        {
            AddSchool(cmd, session);
            cmd.Parameters.AddWithValue("@RegID", regId);
            AddDates(cmd, from, to);
            try
            {
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    dto.UserName = Text(reader["Name"]);
                    dto.Designation = Text(reader["Designation"]);
                    dto.Income = ToDec(reader["Income"]);
                    dto.Expense = ToDec(reader["Expense"]);
                    dto.Submitted = ToDec(reader["Submitted"]);
                    dto.Balance = dto.Income - dto.Expense;
                    dto.Remaining = dto.Balance - dto.Submitted;
                }
            }
            catch
            {
                await using var fallback = new SqlCommand("""
SELECT User_T.Name, User_T.Designation,
       ISNULL(EX_In_T.Other_Income, 0) + ISNULL(Stu_P_T.Student_Income, 0) AS Income,
       ISNULL(Ex_T.Expenditure, 0) + ISNULL(Emp_P_T.Employee_Paid, 0) AS Expense
FROM (
    SELECT Registration.RegistrationID, Admin.Designation,
           ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name
    FROM Registration LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
    WHERE Registration.SchoolID = @SchoolID AND Registration.RegistrationID = @RegID
) User_T
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income FROM Extra_Income
    WHERE SchoolID = @SchoolID AND Extra_IncomeDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) EX_In_T ON User_T.RegistrationID = EX_In_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(PaidAmount), 0) AS Student_Income FROM Income_PaymentRecord
    WHERE SchoolID = @SchoolID AND CAST(PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Stu_P_T ON User_T.RegistrationID = Stu_P_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Expenditure FROM Expenditure
    WHERE SchoolID = @SchoolID AND ExpenseDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Ex_T ON User_T.RegistrationID = Ex_T.RegistrationID
LEFT JOIN (
    SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Employee_Paid FROM Employee_Payorder_Records
    WHERE SchoolID = @SchoolID AND Paid_date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY RegistrationID
) Emp_P_T ON User_T.RegistrationID = Emp_P_T.RegistrationID
""", con);
                AddSchool(fallback, session);
                fallback.Parameters.AddWithValue("@RegID", regId);
                AddDates(fallback, from, to);
                await using var reader = await fallback.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    dto.UserName = Text(reader["Name"]);
                    dto.Designation = Text(reader["Designation"]);
                    dto.Income = ToDec(reader["Income"]);
                    dto.Expense = ToDec(reader["Expense"]);
                    dto.Balance = dto.Income - dto.Expense;
                    dto.Remaining = dto.Balance;
                }
            }
        }

        var income = await GetIncomeReportAsync(session, from, to, "%", ct);
        var expense = await GetExpenseReportAsync(session, from, to, "%", ct);
        dto.IncomeGroups = income.Groups.Select(g => new ReportGroupDto
        {
            Category = g.Category,
            Total = g.Lines.Where(l => l.UserName.Contains($"({ExtractUser(dto.UserName)})") || string.IsNullOrWhiteSpace(dto.UserName)).Sum(l => l.Amount),
            Lines = g.Lines.Where(l => UserMatches(l.UserName, dto.UserName)).ToList()
        }).Where(g => g.Lines.Count > 0).ToList();
        dto.ExpenseGroups = expense.Groups.Select(g => new ReportGroupDto
        {
            Category = g.Category,
            Total = g.Lines.Where(l => UserMatches(l.UserName, dto.UserName)).Sum(l => l.Amount),
            Lines = g.Lines.Where(l => UserMatches(l.UserName, dto.UserName)).ToList()
        }).Where(g => g.Lines.Count > 0).ToList();
        return dto;
    }

    public async Task<BalanceRemainingDto> GetMyRemainingBalanceAsync(
        SessionSnapshot session, int registrationId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var regId = registrationId > 0 ? registrationId : session.RegistrationID;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT
    (ISNULL((SELECT SUM(Extra_IncomeAmount) FROM Extra_Income
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               AND Extra_IncomeDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0)
   + ISNULL((SELECT SUM(PaidAmount) FROM Income_PaymentRecord
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               AND CAST(PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0)
   + ISNULL((SELECT SUM(TotalAmount) FROM CommitteeMoneyReceipt
             WHERE SchoolId = @SchoolID AND RegistrationId = @RegID
               AND CAST(PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0))
  - (ISNULL((SELECT SUM(Amount) FROM Expenditure
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               AND ExpenseDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0)
   + ISNULL((SELECT SUM(Amount) FROM Employee_Payorder_Records
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               AND Paid_date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0))
  - ISNULL((SELECT SUM(SubmissionAmount) FROM User_Balance_Submission
            WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
              AND SubmissionDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0)
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@RegID", regId);
        AddDates(cmd, from, to);
        var value = await cmd.ExecuteScalarAsync(ct);
        return new BalanceRemainingDto
        {
            Remaining = value is null or DBNull ? 0 : ToDec(value)
        };
    }

    public async Task<List<AccountDetailDto>> GetAccountDetailsAsync(SessionSnapshot session, string? accountId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var items = new List<AccountDetailDto>();
        await using var cmd = new SqlCommand($"""
SELECT AccountID, AccountName, AccountBalance,
       ISNULL((SELECT SUM(Amount) FROM Account_Log WHERE SchoolID = @SchoolID AND {AccountLogScopeCorrelated} AND Add_Subtraction = 'Add' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0) AS Total_In,
       ISNULL((SELECT SUM(Amount) FROM Account_Log WHERE SchoolID = @SchoolID AND {AccountLogScopeCorrelated} AND Add_Subtraction = 'Subtraction' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0) AS Total_Ex,
       (SELECT TOP 1 Balance_Before FROM Account_Log WHERE SchoolID = @SchoolID AND AccountID = Account.AccountID AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000') ORDER BY Insert_Date, Insert_Time) AS Balance_Before,
       (SELECT TOP 1 Balance_After FROM Account_Log WHERE SchoolID = @SchoolID AND AccountID = Account.AccountID AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000') ORDER BY Insert_Date DESC, Insert_Time DESC) AS Balance_After
FROM Account
WHERE SchoolID = @SchoolID AND CAST(AccountID AS NVARCHAR(50)) LIKE @AccountID
""", con);
        AddSchool(cmd, session);
        AddDates(cmd, from, to);
        cmd.Parameters.AddWithValue("@AccountID", Like(accountId));
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var add = ToDec(reader["Total_In"]);
                var sub = ToDec(reader["Total_Ex"]);
                if (add == 0 && sub == 0) continue;
                items.Add(new AccountDetailDto
                {
                    AccountID = ToInt(reader["AccountID"]),
                    AccountName = Text(reader["AccountName"]),
                    Balance = ToDec(reader["AccountBalance"]),
                    TotalAdd = add,
                    TotalSub = sub,
                    Opening = ToDec(reader["Balance_Before"]),
                    Closing = ToDec(reader["Balance_After"])
                });
            }
        }

        foreach (var acc in items)
        {
            acc.Adds = await LogDetailCatsAsync(con, session, acc.AccountID, from, to, "Add", "In", true, ct);
            acc.AddAdjust = await LogDetailCatsAsync(con, session, acc.AccountID, from, to, "Add", "In", false, ct);
            acc.Subs = await LogDetailCatsAsync(con, session, acc.AccountID, from, to, "Subtraction", "Ex", true, ct);
            acc.SubAdjust = await LogDetailCatsAsync(con, session, acc.AccountID, from, to, "Subtraction", "Ex", false, ct);
        }

        return items;
    }

    public async Task<AccountsLogDto> GetLogAsync(SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
        from ??= DateTime.Today;
        to ??= DateTime.Today;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return new AccountsLogDto
        {
            IncomeTotal = await ScalarAsync(con, "SELECT ISNULL(SUM(Amount), 0) FROM Account_Log WHERE SchoolID = @SchoolID AND In_Ex_type = 'In' AND Insert_Up_De = 'In' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')", c => { AddSchool(c, session); AddDates(c, from, to); }, ct),
            ExpenseTotal = await ScalarAsync(con, "SELECT ISNULL(SUM(Amount), 0) FROM Account_Log WHERE SchoolID = @SchoolID AND In_Ex_type = 'Ex' AND Insert_Up_De = 'In' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')", c => { AddSchool(c, session); AddDates(c, from, to); }, ct),
            AdjustTotal = await ScalarAsync(con, "SELECT ISNULL(SUM(Amount), 0) FROM Account_Log WHERE SchoolID = @SchoolID AND Insert_Up_De <> 'In' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')", c => { AddSchool(c, session); AddDates(c, from, to); }, ct),
            Income = await LogGroupsAsync(con, session, from, to,
                "In_Ex_type = 'In' AND Insert_Up_De = 'In'",
                "In_Ex_type = 'In' AND Insert_Up_De = 'In' AND Add_Subtraction = N'Add' AND ClassOrOtherCategory NOT LIKE '%Updated%' AND ClassOrOtherCategory NOT LIKE '%Deleted%'", ct),
            Expense = await LogGroupsAsync(con, session, from, to,
                "In_Ex_type = 'Ex' AND Insert_Up_De = 'In'",
                "In_Ex_type = 'Ex' AND Insert_Up_De = 'In' AND Add_Subtraction = N'Subtraction' AND ClassOrOtherCategory NOT LIKE '%Updated%' AND ClassOrOtherCategory NOT LIKE '%Deleted%'", ct),
            Adjust = await LogGroupsAsync(con, session, from, to,
                "Insert_Up_De <> 'In'",
                "Insert_Up_De <> 'In'", ct)
        };
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListIncomeCategoriesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, 0 AS Amount, 0 AS Amount2 FROM (
    SELECT Income_Roles.Role AS Category FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID WHERE Income_PaymentRecord.SchoolID = @SchoolID
    UNION SELECT Extra_IncomeCategory.Extra_Income_CategoryName FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID WHERE Extra_Income.SchoolID = @SchoolID
) t GROUP BY Category ORDER BY Category
""", session, ct);
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListExpenseCategoriesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, 0 AS Amount, 0 AS Amount2 FROM (
    SELECT Employee_Payorder_Name.Payorder_Name AS Category
    FROM Employee_Payorder_Records
    INNER JOIN Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID
    INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
    WHERE Employee_Payorder_Records.SchoolID = @SchoolID
    UNION
    SELECT Expense_CategoryName.CategoryName FROM Expenditure
    INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
    WHERE Expenditure.SchoolID = @SchoolID
) t GROUP BY Category ORDER BY Category
""", session, ct);
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListSectionsAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT DISTINCT CreateSection.SectionID AS Id, CreateSection.Section AS Name, 0 AS Amount, 0 AS Amount2
FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID
WHERE [Join].ClassID = @ClassID
""", session, ct, extra: c => c.Parameters.AddWithValue("@ClassID", classId));
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListGroupsAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT DISTINCT CreateSubjectGroup.SubjectGroupID AS Id, CreateSubjectGroup.SubjectGroup AS Name, 0 AS Amount, 0 AS Amount2
FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID
WHERE [Join].ClassID = @ClassID
""", session, ct, extra: c => c.Parameters.AddWithValue("@ClassID", classId));
    }

    private async Task<List<NameAmountDto>> IncomeCategoryTotalsAsync(SqlConnection con, SessionSnapshot session, DateTime? from, DateTime? to, string like, CancellationToken ct)
    {
        var items = await QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, SUM(Income) AS Amount, 0 AS Amount2 FROM (
    SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Income
    FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
    WHERE Income_PaymentRecord.SchoolID = @SchoolID AND CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY Income_Roles.Role
    UNION ALL
    SELECT Extra_IncomeCategory.Extra_Income_CategoryName, SUM(Extra_Income.Extra_IncomeAmount)
    FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
    WHERE Extra_Income.SchoolID = @SchoolID AND Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName
) t WHERE Category LIKE @Category GROUP BY Category
""", session, ct, extra: c => { AddDates(c, from, to); c.Parameters.AddWithValue("@Category", like); });
        items.AddRange(await QueryNamesAsync(con, """
SELECT 0 AS Id, CommitteeDonationCategory.DonationCategory AS Name, SUM(CommitteePaymentRecord.PaidAmount) AS Amount, 0 AS Amount2
FROM CommitteeMoneyReceipt
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
WHERE CommitteeMoneyReceipt.SchoolId = @SchoolID
  AND CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND CommitteeDonationCategory.DonationCategory LIKE @Category
GROUP BY CommitteeDonationCategory.DonationCategory
""", session, ct, extra: c => { AddDates(c, from, to); c.Parameters.AddWithValue("@Category", like); }, safe: true));
        return items.OrderBy(x => x.Name).ToList();
    }

    private async Task<List<NameAmountDto>> ExpenseCategoryTotalsAsync(SqlConnection con, SessionSnapshot session, DateTime? from, DateTime? to, string like, CancellationToken ct) =>
        await QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, SUM(Amount) AS Amount, 0 AS Amount2 FROM (
    SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Amount
    FROM Employee_Payorder_Records
    INNER JOIN Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID
    INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
    WHERE Employee_Payorder_Records.SchoolID = @SchoolID AND Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY Employee_Payorder_Name.Payorder_Name
    UNION ALL
    SELECT Expense_CategoryName.CategoryName, SUM(Expenditure.Amount)
    FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
    WHERE Expenditure.SchoolID = @SchoolID AND Expenditure.ExpenseDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
    GROUP BY Expense_CategoryName.CategoryName
) t WHERE Category LIKE @Category GROUP BY Category
""", session, ct, extra: c => { AddDates(c, from, to); c.Parameters.AddWithValue("@Category", like); });

    private static IncomeExpenseReportDto Pack(List<NameAmountDto> groups, List<ReportLineDto> lines)
    {
        var map = groups.ToDictionary(g => g.Name, g => new ReportGroupDto { Category = g.Name, Total = g.Amount }, StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            if (!map.TryGetValue(line.Category, out var group))
            {
                group = new ReportGroupDto { Category = line.Category };
                map[line.Category] = group;
            }
            group.Lines.Add(line);
            if (group.Total == 0) group.Total += line.Amount;
        }
        var list = map.Values.OrderBy(x => x.Category).ToList();
        return new IncomeExpenseReportDto { Groups = list, Total = list.Sum(x => x.Total) };
    }

    private async Task<List<NameAmountDto>> QueryNamesAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null, bool safe = false, int commandTimeout = 30)
    {
        var items = new List<NameAmountDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = commandTimeout;
            if (sql.Contains("@SchoolID", StringComparison.OrdinalIgnoreCase))
                AddSchool(cmd, session);
            extra?.Invoke(cmd);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new NameAmountDto
                {
                    Id = ToInt(reader["Id"]),
                    Name = Text(reader["Name"]),
                    Amount = ToDec(reader["Amount"]),
                    Amount2 = ToDec(reader["Amount2"])
                });
            }
        }
        catch when (safe)
        {
        }
        return items;
    }

    private static List<NameAmountDto> MergeNamedAmounts(List<NameAmountDto> primary, List<NameAmountDto> extra)
    {
        if (extra.Count == 0) return primary;
        var map = new Dictionary<string, NameAmountDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in primary.Concat(extra))
        {
            var key = row.Id > 0 ? "id:" + row.Id : "n:" + row.Name;
            if (map.TryGetValue(key, out var exist))
            {
                exist.Amount += row.Amount;
                exist.Amount2 += row.Amount2;
            }
            else
            {
                map[key] = new NameAmountDto
                {
                    Id = row.Id,
                    Name = row.Name,
                    Amount = row.Amount,
                    Amount2 = row.Amount2
                };
            }
        }
        return map.Values.OrderBy(x => x.Id).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private async Task<List<ReportLineDto>> QueryLinesAsync(SqlConnection con, string sql, SessionSnapshot session, DateTime? from, DateTime? to, string category, CancellationToken ct, bool safe = false)
    {
        var items = new List<ReportLineDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            AddSchool(cmd, session);
            AddDates(cmd, from, to);
            cmd.Parameters.AddWithValue("@Category", category);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new ReportLineDto
                {
                    UserName = Text(reader["UserName"]),
                    AccountName = Text(reader["AccountName"]),
                    Category = Text(reader["Category"]),
                    Details = Text(reader["Details"]),
                    Amount = ToDec(reader["Amount"]),
                    Date = Day(reader["Date"])
                });
            }
        }
        catch when (safe)
        {
        }
        return items;
    }

    private static string Col(System.Data.Common.DbDataReader reader, string name)
    {
        try { return Text(reader[name]); }
        catch { return ""; }
    }

    private static async Task<List<CurrentDueRowDto>> ReadDueRowsAsync(SqlCommand cmd, CancellationToken ct)
    {
        var items = new List<CurrentDueRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new CurrentDueRowDto
            {
                StudentID = ToInt(reader["StudentID"]),
                ID = Text(reader["ID"]),
                Name = Text(reader["StudentsName"]),
                RollNo = Text(reader["RollNo"]),
                Phone = Text(reader["SMSPhoneNo"]),
                ClassName = Col(reader, "Class"),
                Due = ToDec(reader["Due"])
            });
        }
        return items;
    }

    private async Task<Dictionary<string, string>> LoadInventoryPayForLabelsAsync(
        SqlConnection con, SessionSnapshot session, IEnumerable<string?> invoices, CancellationToken ct)
    {
        var keys = invoices
            .Select(x => (x ?? "").Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (keys.Count == 0) return map;
        try
        {
            var inList = string.Join(",", keys.Select((_, i) => $"@Inv{i}"));
            await using var cmd = new SqlCommand($"""
SELECT s.InvoiceNo, i.Name
FROM dbo.Inv_Sale AS s
INNER JOIN dbo.Inv_SaleLine AS l ON l.SaleID = s.SaleID
INNER JOIN dbo.Inv_Item AS i ON i.ItemID = l.ItemID
WHERE s.SchoolID = @SchoolID AND s.InvoiceNo IN ({inList})
ORDER BY s.InvoiceNo, l.SaleLineID
""", con);
            AddSchool(cmd, session);
            for (var i = 0; i < keys.Count; i++)
                cmd.Parameters.AddWithValue($"@Inv{i}", keys[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var inv = Text(reader["InvoiceNo"]);
                var name = Text(reader["Name"]);
                if (inv.Length == 0 || name.Length == 0) continue;
                if (map.TryGetValue(inv, out var prev))
                {
                    if (!prev.Split([", "], StringSplitOptions.None).Contains(name, StringComparer.OrdinalIgnoreCase))
                        map[inv] = prev + ", " + name;
                }
                else
                    map[inv] = name;
            }
        }
        catch
        {
        }
        return map;
    }

    private static string PayForWithItems(string? payFor, Dictionary<string, string> labels)
    {
        var key = (payFor ?? "").Trim();
        if (key.Length == 0 || !labels.TryGetValue(key, out var items) || string.IsNullOrWhiteSpace(items))
            return payFor ?? "";
        var parts = items.Split([", "], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 6)
            items = string.Join(", ", parts.Take(6)) + ", …";
        return $"{key} ({items})";
    }

    private async Task<List<AccountDetailCatDto>> LogDetailCatsAsync(SqlConnection con, SessionSnapshot session, int accountId, DateTime? from, DateTime? to, string addSub, string inEx, bool matchType, CancellationToken ct)
    {
        var op = matchType ? "=" : "<>";
        if (matchType)
        {
            return (await QueryNamesAsync(con, $"""
SELECT 0 AS Id, SubCategory AS Name, SUM(Amount) AS Amount, 0 AS Amount2
FROM Account_Log
WHERE SchoolID = @SchoolID AND {AccountLogScopeParam} AND Add_Subtraction = @AddSub AND In_Ex_type {op} @InEx
  AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY SubCategory
""", session, ct, extra: c =>
            {
                c.Parameters.AddWithValue("@AccountID", accountId);
                c.Parameters.AddWithValue("@AddSub", addSub);
                c.Parameters.AddWithValue("@InEx", inEx);
                AddDates(c, from, to);
            })).Select(x => new AccountDetailCatDto { Name = x.Name, Amount = x.Amount }).ToList();
        }

        const string badgeExpr = """
CASE
  WHEN Insert_Up_De = 'De' AND Add_Subtraction = 'Add' THEN 'deleted'
  WHEN Insert_Up_De = 'De' AND Add_Subtraction = 'Subtraction'
       AND (ClassOrOtherCategory LIKE '%Student Payment%' OR Details LIKE '%Receipt No:%') THEN 'unpaid'
  WHEN Insert_Up_De = 'De' THEN 'deleted'
  WHEN Insert_Up_De = 'Up' THEN 'adjust'
  ELSE 'adjust'
END
""";
        var items = new List<AccountDetailCatDto>();
        await using var cmd = new SqlCommand($"""
SELECT SubCategory AS Name, SUM(Amount) AS Amount, {badgeExpr} AS Badge
FROM Account_Log
WHERE SchoolID = @SchoolID AND {AccountLogScopeParam} AND Add_Subtraction = @AddSub AND In_Ex_type {op} @InEx
  AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY SubCategory, {badgeExpr}
ORDER BY SUM(Amount) DESC
""", con);
        AddSchool(cmd, session);
        cmd.Parameters.AddWithValue("@AccountID", accountId);
        cmd.Parameters.AddWithValue("@AddSub", addSub);
        cmd.Parameters.AddWithValue("@InEx", inEx);
        AddDates(cmd, from, to);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new AccountDetailCatDto
            {
                Name = Text(reader["Name"]),
                Amount = ToDec(reader["Amount"]),
                Badge = Text(reader["Badge"])
            });
        }

        return items;
    }

    private async Task<List<NameAmountDto>> LogCatsAsync(SqlConnection con, SessionSnapshot session, int accountId, DateTime? from, DateTime? to, string addSub, string inEx, bool matchType, CancellationToken ct)
    {
        var op = matchType ? "=" : "<>";
        return await QueryNamesAsync(con, $"""
SELECT 0 AS Id, SubCategory AS Name, SUM(Amount) AS Amount, 0 AS Amount2
FROM Account_Log
WHERE SchoolID = @SchoolID AND {AccountLogScopeParam} AND Add_Subtraction = @AddSub AND In_Ex_type {op} @InEx
  AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY SubCategory
""", session, ct, extra: c =>
        {
            c.Parameters.AddWithValue("@AccountID", accountId);
            c.Parameters.AddWithValue("@AddSub", addSub);
            c.Parameters.AddWithValue("@InEx", inEx);
            AddDates(c, from, to);
        });
    }

    private static string AliasedLogWhere(string where) =>
        where.Replace("ClassOrOtherCategory", "AL.ClassOrOtherCategory", StringComparison.Ordinal)
            .Replace("Add_Subtraction", "AL.Add_Subtraction", StringComparison.Ordinal)
            .Replace("Insert_Up_De", "AL.Insert_Up_De", StringComparison.Ordinal)
            .Replace("In_Ex_type", "AL.In_Ex_type", StringComparison.Ordinal);

    private async Task<List<ReportGroupDto>> LogGroupsAsync(SqlConnection con, SessionSnapshot session, DateTime? from, DateTime? to, string groupWhere, string lineWhere, CancellationToken ct)
    {
        var groups = new List<ReportGroupDto>();
        var aliasedLineWhere = AliasedLogWhere(lineWhere);
        await using var cmd = new SqlCommand($"""
SELECT ClassOrOtherCategory, ISNULL(SUM(Amount), 0) AS Total
FROM Account_Log
WHERE SchoolID = @SchoolID AND {groupWhere} AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY ClassOrOtherCategory ORDER BY ClassOrOtherCategory
""", con);
        AddSchool(cmd, session);
        AddDates(cmd, from, to);
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
                groups.Add(new ReportGroupDto { Category = Text(reader[0]), Total = ToDec(reader[1]) });
        }

        foreach (var group in groups)
        {
            await using var lines = new SqlCommand($"""
SELECT AL.Log_SN, AL.SubCategory, AL.Amount, AL.Details, COALESCE(OpReg.UserName, Reg.UserName) AS UserName,
       AL.Insert_Date, AL.Activity_Date, CONVERT(varchar(15), AL.Insert_Time, 100) AS Insert_Time
FROM Account_Log AL
LEFT JOIN Registration Reg ON AL.RegistrationID = Reg.RegistrationID
OUTER APPLY (
    SELECT TRY_CAST(LTRIM(RTRIM(SUBSTRING(AL.Details, NULLIF(CHARINDEX('ID =', AL.Details), 0) + 4, 10))) AS INT) AS OpRegID
) P
LEFT JOIN Registration OpReg ON OpReg.RegistrationID = P.OpRegID
WHERE AL.SchoolID = @SchoolID AND AL.ClassOrOtherCategory = @Cat AND {aliasedLineWhere}
  AND AL.Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
ORDER BY AL.Insert_Date, AL.Insert_Time, AL.Log_SN
""", con);
            AddSchool(lines, session);
            AddDates(lines, from, to);
            lines.Parameters.AddWithValue("@Cat", group.Category);
            try
            {
                await using var reader = await lines.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    group.Lines.Add(new ReportLineDto
                    {
                        Sn = ToInt(reader["Log_SN"]),
                        SubCategory = Text(reader["SubCategory"]),
                        Amount = ToDec(reader["Amount"]),
                        Details = Text(reader["Details"]),
                        UserName = Text(reader["UserName"]),
                        Date = Day(reader["Insert_Date"]),
                        ActivityDate = reader["Activity_Date"] is DBNull ? null : Convert.ToDateTime(reader["Activity_Date"]),
                        Time = Text(reader["Insert_Time"])
                    });
                }
            }
            catch
            {
                await using var simple = new SqlCommand($"""
SELECT Log_SN, SubCategory, Amount, Details, '' AS UserName, Insert_Date, Activity_Date, CONVERT(varchar(15), Insert_Time, 100) AS Insert_Time
FROM Account_Log AL
WHERE AL.SchoolID = @SchoolID AND AL.ClassOrOtherCategory = @Cat AND {aliasedLineWhere}
  AND AL.Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
ORDER BY AL.Insert_Date, AL.Insert_Time, AL.Log_SN
""", con);
                AddSchool(simple, session);
                AddDates(simple, from, to);
                simple.Parameters.AddWithValue("@Cat", group.Category);
                await using var reader = await simple.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    group.Lines.Add(new ReportLineDto
                    {
                        Sn = ToInt(reader["Log_SN"]),
                        SubCategory = Text(reader["SubCategory"]),
                        Amount = ToDec(reader["Amount"]),
                        Details = Text(reader["Details"]),
                        Date = Day(reader["Insert_Date"]),
                        ActivityDate = reader["Activity_Date"] is DBNull ? null : Convert.ToDateTime(reader["Activity_Date"]),
                        Time = Text(reader["Insert_Time"])
                    });
                }
            }
        }

        return groups;
    }

    private static bool UserMatches(string userName, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return true;
        var user = ExtractUser(fullName);
        return userName.Contains(fullName, StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(user) && userName.Contains($"({user})", StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractUser(string name)
    {
        var start = name.LastIndexOf('(');
        var end = name.LastIndexOf(')');
        return start >= 0 && end > start ? name[(start + 1)..end] : name;
    }
}
