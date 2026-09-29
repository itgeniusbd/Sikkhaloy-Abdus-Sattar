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

    private static string SqlDateRange(string column) => $"""
  AND {column} >= ISNULL(@From, CONVERT(date, '17530101'))
  AND {column} < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
""";

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
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var hasReceipt = await TableExistsAsync(con, "CommitteeMoneyReceipt", ct);
        var hasPay = await TableExistsAsync(con, "CommitteePaymentRecord", ct);
        return await LoadSummaryFastAsync(con, session, hasReceipt, hasPay, ct);
    }

    private const int ReportCommandTimeout = 120;
    private const int SummaryCommandTimeout = 60;
    private static void SetReportTimeout(SqlCommand cmd) => cmd.CommandTimeout = ReportCommandTimeout;

    private static async Task<bool> TableExistsAsync(SqlConnection con, string table, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@t, 'U') IS NOT NULL THEN 1 ELSE 0 END", con);
        cmd.CommandTimeout = 5;
        cmd.Parameters.AddWithValue("@t", "dbo." + table);
        return ToInt(await cmd.ExecuteScalarAsync(ct)) == 1;
    }

    private async Task<AccountsSummaryDto> LoadSummaryFastAsync(
        SqlConnection con, SessionSnapshot session, bool hasCommitteeReceipt, bool hasCommitteePay, CancellationToken ct)
    {
        var committeeInc = hasCommitteeReceipt
            ? """
INSERT INTO #inc (RegistrationID, EducationYearID, MonthStart, Category, Amount)
SELECT RegistrationID, EducationYearId,
       CASE WHEN PaidDate IS NULL THEN NULL ELSE DATEFROMPARTS(YEAR(PaidDate), MONTH(PaidDate), 1) END,
       NULL, TotalAmount
FROM dbo.CommitteeMoneyReceipt
WHERE SchoolId = @SchoolID;

"""
            : "";
        var donationCats = hasCommitteePay
            ? """
UNION ALL
SELECT CommitteeDonationCategory.DonationCategory, CommitteePaymentRecord.PaidAmount
FROM dbo.CommitteePaymentRecord
INNER JOIN dbo.CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN dbo.CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
WHERE CommitteePaymentRecord.SchoolId = @SchoolID
"""
            : "";

        var sql =
            """
SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#inc') IS NOT NULL DROP TABLE #inc;
IF OBJECT_ID('tempdb..#exp') IS NOT NULL DROP TABLE #exp;
CREATE TABLE #inc (
    RegistrationID int NULL,
    EducationYearID int NULL,
    MonthStart date NULL,
    Category nvarchar(250) NULL,
    Amount money NOT NULL
);
CREATE TABLE #exp (
    RegistrationID int NULL,
    EducationYearID int NULL,
    MonthStart date NULL,
    Category nvarchar(250) NULL,
    Amount money NOT NULL
);

INSERT INTO #inc (RegistrationID, EducationYearID, MonthStart, Category, Amount)
SELECT ipr.RegistrationID, ipr.EducationYearID,
       CASE WHEN ipr.PaidDate IS NULL THEN NULL ELSE DATEFROMPARTS(YEAR(ipr.PaidDate), MONTH(ipr.PaidDate), 1) END,
       r.Role, ipr.PaidAmount
FROM dbo.Income_PaymentRecord ipr
LEFT JOIN dbo.Income_Roles r ON r.RoleID = ipr.RoleID
WHERE ipr.SchoolID = @SchoolID;

INSERT INTO #inc (RegistrationID, EducationYearID, MonthStart, Category, Amount)
SELECT ei.RegistrationID, ei.EducationYearID,
       CASE WHEN ei.Extra_IncomeDate IS NULL THEN NULL ELSE DATEFROMPARTS(YEAR(ei.Extra_IncomeDate), MONTH(ei.Extra_IncomeDate), 1) END,
       c.Extra_Income_CategoryName, ei.Extra_IncomeAmount
FROM dbo.Extra_Income ei
LEFT JOIN dbo.Extra_IncomeCategory c ON c.Extra_IncomeCategoryID = ei.Extra_IncomeCategoryID
WHERE ei.SchoolID = @SchoolID;
"""
            + committeeInc
            + """
INSERT INTO #exp (RegistrationID, EducationYearID, MonthStart, Category, Amount)
SELECT e.RegistrationID, e.EducationYearID,
       CASE WHEN e.ExpenseDate IS NULL THEN NULL ELSE DATEFROMPARTS(YEAR(e.ExpenseDate), MONTH(e.ExpenseDate), 1) END,
       c.CategoryName, e.Amount
FROM dbo.Expenditure e
LEFT JOIN dbo.Expense_CategoryName c ON c.ExpenseCategoryID = e.ExpenseCategoryID
WHERE e.SchoolID = @SchoolID;

INSERT INTO #exp (RegistrationID, EducationYearID, MonthStart, Category, Amount)
SELECT r.RegistrationID, r.EducationYearID,
       CASE WHEN r.Paid_date IS NULL THEN NULL ELSE DATEFROMPARTS(YEAR(r.Paid_date), MONTH(r.Paid_date), 1) END,
       n.Payorder_Name, r.Amount
FROM dbo.Employee_Payorder_Records r
LEFT JOIN dbo.Employee_Payorder po ON po.Employee_PayorderID = r.Employee_PayorderID
LEFT JOIN dbo.Employee_Payorder_Name n ON n.Employee_Payorder_NameID = po.Employee_Payorder_NameID
WHERE r.SchoolID = @SchoolID;

SELECT
    ISNULL((SELECT SUM(Amount) FROM #inc), 0) AS TotalIncome,
    ISNULL((SELECT SUM(Amount) FROM #exp), 0) AS TotalExpense,
    ISNULL((SELECT SUM(AccountBalance) FROM dbo.Account WHERE SchoolID = @SchoolID), 0) AS AccountTotal;

SELECT
    EducationYearID,
    CAST(CASE WHEN GROUPING(EducationYearID) = 1 THEN 1 ELSE 0 END AS bit) AS IsTotal,
    ISNULL(SUM(CASE WHEN Is_Active = 1 THEN Amount ELSE 0 END), 0) AS Payorder,
    ISNULL(SUM(CASE WHEN Is_Active = 1 THEN LateFeeCountable ELSE 0 END), 0) AS LateFee,
    ISNULL(SUM(CASE WHEN Is_Active = 1 THEN Total_Discount ELSE 0 END), 0) AS Concession,
    ISNULL(SUM(CASE WHEN Is_Active = 1 THEN PaidAmount ELSE 0 END), 0) AS Paid,
    ISNULL(SUM(CASE WHEN Is_Active = 1 THEN Receivable_Amount ELSE 0 END), 0) AS Unpaid,
    ISNULL(SUM(CASE WHEN Is_Active = 1 AND EndDate < GETDATE() THEN Receivable_Amount ELSE 0 END), 0) AS PresentDue,
    ISNULL(SUM(CASE WHEN StartDate > GETDATE() THEN PaidAmount ELSE 0 END), 0) AS Advance
FROM dbo.Income_PayOrder
WHERE SchoolID = @SchoolID
GROUP BY GROUPING SETS ((EducationYearID), ());

SELECT AccountID AS Id, AccountName AS Name, AccountBalance AS Amount, CAST(0 AS money) AS Amount2
FROM dbo.Account WHERE SchoolID = @SchoolID;

SELECT User_T.RegistrationID AS Id, User_T.Name, ISNULL(i.Amt, 0) AS Amount, ISNULL(e.Amt, 0) AS Amount2
FROM (
    SELECT DISTINCT Registration.RegistrationID,
           ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name
    FROM dbo.Registration
    INNER JOIN dbo.Admin ON Registration.RegistrationID = Admin.RegistrationID
    WHERE Registration.SchoolID = @SchoolID
) AS User_T
LEFT JOIN (SELECT RegistrationID, SUM(Amount) AS Amt FROM #inc GROUP BY RegistrationID) i ON User_T.RegistrationID = i.RegistrationID
LEFT JOIN (SELECT RegistrationID, SUM(Amount) AS Amt FROM #exp GROUP BY RegistrationID) e ON User_T.RegistrationID = e.RegistrationID
WHERE ISNULL(i.Amt, 0) <> 0 OR ISNULL(e.Amt, 0) <> 0;

SELECT 0 AS Id, Category AS Name, SUM(Amount) AS Amount, CAST(0 AS money) AS Amount2
FROM (
    SELECT Category, Amount FROM #inc WHERE Category IS NOT NULL AND LTRIM(RTRIM(Category)) <> N''
"""
            + donationCats
            + """
) t
GROUP BY Category
ORDER BY Category;

SELECT 0 AS Id, Category AS Name, SUM(Amount) AS Amount, CAST(0 AS money) AS Amount2
FROM #exp
WHERE Category IS NOT NULL AND LTRIM(RTRIM(Category)) <> N''
GROUP BY Category
ORDER BY Category;

SELECT ey.EducationYearID, ey.EducationYear, ey.StartDate, ey.EndDate,
       ISNULL(i.Income, 0) AS Income, ISNULL(e.Expense, 0) AS Expense
FROM dbo.Education_Year ey
LEFT JOIN (SELECT EducationYearID, SUM(Amount) AS Income FROM #inc GROUP BY EducationYearID) i ON ey.EducationYearID = i.EducationYearID
LEFT JOIN (SELECT EducationYearID, SUM(Amount) AS Expense FROM #exp GROUP BY EducationYearID) e ON ey.EducationYearID = e.EducationYearID
WHERE ey.SchoolID = @SchoolID
  AND (ISNULL(i.Income, 0) <> 0 OR ISNULL(e.Expense, 0) <> 0)
ORDER BY ey.StartDate DESC;

SELECT MonthStart, SUM(Income) AS Income, SUM(Expense) AS Expense
FROM (
    SELECT MonthStart, Amount AS Income, CAST(0 AS money) AS Expense FROM #inc WHERE MonthStart IS NOT NULL
    UNION ALL
    SELECT MonthStart, CAST(0 AS money), Amount FROM #exp WHERE MonthStart IS NOT NULL
) t
GROUP BY MonthStart
ORDER BY MonthStart;
""";

        var dto = new AccountsSummaryDto();
        var payMap = new Dictionary<int, (decimal Payorder, decimal LateFee, decimal Concession, decimal Paid, decimal Unpaid)>();
        await using var cmd = new SqlCommand(sql, con);
        cmd.CommandTimeout = SummaryCommandTimeout;
        AddSchool(cmd, session);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            dto.TotalIncome = ToDec(reader["TotalIncome"]);
            dto.TotalExpense = ToDec(reader["TotalExpense"]);
            dto.AccountTotal = ToDec(reader["AccountTotal"]);
            dto.Net = dto.TotalIncome - dto.TotalExpense;
        }

        await reader.NextResultAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var payorder = ToDec(reader["Payorder"]);
            var late = ToDec(reader["LateFee"]);
            var concession = ToDec(reader["Concession"]);
            var paid = ToDec(reader["Paid"]);
            var unpaid = ToDec(reader["Unpaid"]);
            if (reader.GetBoolean(reader.GetOrdinal("IsTotal")))
            {
                dto.Payorder = payorder;
                dto.LateFee = late;
                dto.Concession = concession;
                dto.Paid = paid;
                dto.Unpaid = unpaid;
                dto.PresentDue = ToDec(reader["PresentDue"]);
                dto.Advance = ToDec(reader["Advance"]);
            }
            else
            {
                payMap[ToInt(reader["EducationYearID"])] = (payorder, late, concession, paid, unpaid);
            }
        }

        await reader.NextResultAsync(ct);
        dto.Accounts = await ReadNameRowsAsync(reader, ct);
        await reader.NextResultAsync(ct);
        dto.Users = await ReadNameRowsAsync(reader, ct);
        await reader.NextResultAsync(ct);
        dto.IncomeCategories = await ReadNameRowsAsync(reader, ct);
        await reader.NextResultAsync(ct);
        dto.ExpenseCategories = await ReadNameRowsAsync(reader, ct);

        await reader.NextResultAsync(ct);
        var sessions = new List<SessionReportDto>();
        while (await reader.ReadAsync(ct))
        {
            var income = ToDec(reader["Income"]);
            var expense = ToDec(reader["Expense"]);
            sessions.Add(new SessionReportDto
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

        await reader.NextResultAsync(ct);
        var months = new List<(DateTime Month, decimal Income, decimal Expense)>();
        while (await reader.ReadAsync(ct))
            months.Add((Day(reader["MonthStart"]), ToDec(reader["Income"]), ToDec(reader["Expense"])));

        foreach (var year in sessions)
        {
            if (payMap.TryGetValue(year.EducationYearID, out var pay))
            {
                year.Payorder = pay.Payorder;
                year.LateFee = pay.LateFee;
                year.Concession = pay.Concession;
                year.Paid = pay.Paid;
                year.Unpaid = pay.Unpaid;
            }
            var from = year.StartDate.Date;
            var to = year.EndDate.Date;
            var list = new List<NameAmountDto>();
            foreach (var row in months)
            {
                if (row.Month < from || row.Month > to)
                    continue;
                list.Add(new NameAmountDto
                {
                    Name = row.Month.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Amount = row.Income,
                    Amount2 = row.Expense
                });
            }
            if (list.Count > 0)
                year.Months = list;
        }

        dto.Sessions = sessions;
        return dto;
    }

    private async Task<List<NameAmountDto>> ReadNameRowsAsync(SqlDataReader reader, CancellationToken ct)
    {
        var items = new List<NameAmountDto>();
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
        return items;
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
       Income_PaymentRecord.PaidAmount AS Amount, CAST(Income_PaymentRecord.PaidDate AS DATE) AS [Date],
       COALESCE(NULLIF(LTRIM(RTRIM(mr.PrintedReceiptNo)), N''), CAST(mr.MoneyReceipt_SN AS nvarchar(20))) AS InvoiceNo,
       N'money' AS InvoiceKind
FROM Income_PaymentRecord
INNER JOIN Registration ON Income_PaymentRecord.RegistrationID = Registration.RegistrationID
INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
INNER JOIN Student ON Income_PaymentRecord.StudentID = Student.StudentID
INNER JOIN StudentsClass ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID
INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT JOIN Admin ON Admin.RegistrationID = Registration.RegistrationID
LEFT JOIN Account ON Income_PaymentRecord.AccountID = Account.AccountID
LEFT JOIN Income_MoneyReceipt mr ON mr.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID
WHERE Income_PaymentRecord.SchoolID = @SchoolID
  AND Income_PaymentRecord.PaidDate >= ISNULL(@From, CONVERT(date, '17530101'))
  AND Income_PaymentRecord.PaidDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
  AND Income_Roles.Role LIKE @Category
UNION ALL
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')',
       ISNULL(Account.AccountName, 'N/A'), Extra_IncomeCategory.Extra_Income_CategoryName,
       Extra_Income.Extra_IncomeFor, Extra_Income.Extra_IncomeAmount, Extra_Income.Extra_IncomeDate,
       CAST(NULL AS nvarchar(20)), CAST(NULL AS nvarchar(20))
FROM Extra_Income
INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
INNER JOIN Registration ON Extra_Income.RegistrationID = Registration.RegistrationID
LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
LEFT JOIN Account ON Extra_Income.AccountID = Account.AccountID
WHERE Extra_Income.SchoolID = @SchoolID
  AND Extra_Income.Extra_IncomeDate >= ISNULL(@From, CONVERT(date, '17530101'))
  AND Extra_Income.Extra_IncomeDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
  AND Extra_IncomeCategory.Extra_Income_CategoryName LIKE @Category
ORDER BY [Date]
""", session, from, to, like, ct);
        lines.AddRange(await QueryLinesAsync(con, """
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, CommitteeDonationCategory.DonationCategory AS Category,
       CommitteeDonation.Description AS Details, CommitteePaymentRecord.PaidAmount AS Amount,
       CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS [Date],
       CAST(CommitteeMoneyReceipt.CommitteeMoneyReceiptSn AS nvarchar(20)) AS InvoiceNo,
       N'committee' AS InvoiceKind
FROM CommitteeMoneyReceipt
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
INNER JOIN Registration ON CommitteeMoneyReceipt.RegistrationId = Registration.RegistrationID
LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
LEFT JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID
WHERE CommitteeMoneyReceipt.SchoolID = @SchoolID
  AND CommitteeMoneyReceipt.PaidDate >= ISNULL(@From, CONVERT(date, '17530101'))
  AND CommitteeMoneyReceipt.PaidDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
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
       Expenditure.ExpenseFor AS Details, Expenditure.Amount, Expenditure.ExpenseDate AS [Date],
       CAST(Expenditure.ExpenseID AS nvarchar(20)) AS InvoiceNo, N'expense' AS InvoiceKind
FROM Expenditure
INNER JOIN Registration ON Expenditure.RegistrationID = Registration.RegistrationID
INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
INNER JOIN Admin ON Admin.RegistrationID = Registration.RegistrationID
LEFT JOIN Account ON Expenditure.AccountID = Account.AccountID
WHERE Expenditure.SchoolID = @SchoolID
  AND Expenditure.ExpenseDate >= ISNULL(@From, CONVERT(date, '17530101'))
  AND Expenditure.ExpenseDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
  AND Expense_CategoryName.CategoryName LIKE @Category
UNION ALL
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')',
       ISNULL(Account.AccountName, 'N/A'), Employee_Payorder_Name.Payorder_Name,
       Employee_Payorder_Records.Paid_For, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date,
       CAST(Employee_Payorder_Records.Employee_Payorder_RecordID AS nvarchar(20)), N'pay'
FROM Registration
INNER JOIN Admin ON Admin.RegistrationID = Registration.RegistrationID
INNER JOIN Employee_Payorder_Records ON Registration.RegistrationID = Employee_Payorder_Records.RegistrationID
INNER JOIN Employee_Payorder ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Records.Employee_PayorderID
INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
LEFT JOIN Account ON Employee_Payorder_Records.AccountID = Account.AccountID
WHERE Employee_Payorder_Records.SchoolID = @SchoolID
  AND Employee_Payorder_Records.Paid_date >= ISNULL(@From, CONVERT(date, '17530101'))
  AND Employee_Payorder_Records.Paid_date < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
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
        var hasCommittee = await TableExistsAsync(con, "CommitteeMoneyReceipt", ct);
        var hasSubmit = await TableExistsAsync(con, "User_Balance_Submission", ct);
        var dto = new MyAccountsDto();

        var committeeIncome = hasCommittee ? $"""
     + ISNULL((SELECT SUM(TotalAmount) FROM CommitteeMoneyReceipt
               WHERE SchoolId = @SchoolID AND RegistrationId = @RegID
                 {SqlDateRange("PaidDate")}), 0)
""" : "";
        var submittedExpr = hasSubmit ? $"""
ISNULL((SELECT SUM(SubmissionAmount) FROM User_Balance_Submission
        WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
          {SqlDateRange("SubmissionDate")}), 0)
""" : "CAST(0 AS decimal(18,2))";

        await using (var cmd = new SqlCommand($"""
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name,
       Admin.Designation,
       ISNULL((SELECT SUM(PaidAmount) FROM Income_PaymentRecord
               WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
                 {SqlDateRange("PaidDate")}), 0)
     + ISNULL((SELECT SUM(Extra_IncomeAmount) FROM Extra_Income
               WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
                 {SqlDateRange("Extra_IncomeDate")}), 0)
     {committeeIncome} AS Income,
       ISNULL((SELECT SUM(Amount) FROM Expenditure
               WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
                 {SqlDateRange("ExpenseDate")}), 0)
     + ISNULL((SELECT SUM(Amount) FROM Employee_Payorder_Records
               WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
                 {SqlDateRange("Paid_date")}), 0) AS Expense,
       {submittedExpr} AS Submitted
FROM Registration
LEFT JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
WHERE Registration.SchoolID = @SchoolID AND Registration.RegistrationID = @RegID
""", con))
        {
            cmd.CommandTimeout = SummaryCommandTimeout;
            AddSchool(cmd, session);
            cmd.Parameters.AddWithValue("@RegID", regId);
            AddDates(cmd, from, to);
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

        void BindReg(SqlCommand c) => c.Parameters.AddWithValue("@RegID", regId);

        var incomeLines = await QueryLinesAsync(con, $"""
SELECT CAST(N'' AS nvarchar(300)) AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, Income_Roles.Role AS Category,
       '[' + Student.ID + '] ' + Student.StudentsName + ', Class: ' + CreateClass.Class + ', For: ' + Income_PaymentRecord.PayFor AS Details,
       Income_PaymentRecord.PaidAmount AS Amount, CAST(Income_PaymentRecord.PaidDate AS DATE) AS [Date],
       COALESCE(NULLIF(LTRIM(RTRIM(mr.PrintedReceiptNo)), N''), CAST(mr.MoneyReceipt_SN AS nvarchar(20))) AS InvoiceNo,
       N'money' AS InvoiceKind
FROM Income_PaymentRecord
INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
INNER JOIN Student ON Income_PaymentRecord.StudentID = Student.StudentID
INNER JOIN StudentsClass ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID
INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
LEFT JOIN Account ON Income_PaymentRecord.AccountID = Account.AccountID
LEFT JOIN Income_MoneyReceipt mr ON mr.MoneyReceiptID = Income_PaymentRecord.MoneyReceiptID
WHERE Income_PaymentRecord.SchoolID = @SchoolID
  AND Income_PaymentRecord.RegistrationID = @RegID
  {SqlDateRange("Income_PaymentRecord.PaidDate")}
UNION ALL
SELECT CAST(N'' AS nvarchar(300)),
       ISNULL(Account.AccountName, 'N/A'), Extra_IncomeCategory.Extra_Income_CategoryName,
       Extra_Income.Extra_IncomeFor, Extra_Income.Extra_IncomeAmount, Extra_Income.Extra_IncomeDate,
       CAST(NULL AS nvarchar(20)), CAST(NULL AS nvarchar(20))
FROM Extra_Income
INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
LEFT JOIN Account ON Extra_Income.AccountID = Account.AccountID
WHERE Extra_Income.SchoolID = @SchoolID
  AND Extra_Income.RegistrationID = @RegID
  {SqlDateRange("Extra_Income.Extra_IncomeDate")}
ORDER BY [Date] DESC
""", session, from, to, "%", ct, extra: BindReg);

        if (hasCommittee)
        {
            incomeLines.AddRange(await QueryLinesAsync(con, $"""
SELECT CAST(N'' AS nvarchar(300)) AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, CommitteeDonationCategory.DonationCategory AS Category,
       CommitteeDonation.Description AS Details, CommitteePaymentRecord.PaidAmount AS Amount,
       CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS [Date],
       CAST(CommitteeMoneyReceipt.CommitteeMoneyReceiptSn AS nvarchar(20)) AS InvoiceNo,
       N'committee' AS InvoiceKind
FROM CommitteeMoneyReceipt
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
LEFT JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID
WHERE CommitteeMoneyReceipt.SchoolID = @SchoolID
  AND CommitteeMoneyReceipt.RegistrationId = @RegID
  {SqlDateRange("CommitteeMoneyReceipt.PaidDate")}
""", session, from, to, "%", ct, safe: true, extra: BindReg));
        }

        var expenseLines = await QueryLinesAsync(con, $"""
SELECT CAST(N'' AS nvarchar(300)) AS UserName,
       ISNULL(Account.AccountName, 'N/A') AS AccountName, Expense_CategoryName.CategoryName AS Category,
       Expenditure.ExpenseFor AS Details, Expenditure.Amount, Expenditure.ExpenseDate AS [Date],
       CAST(Expenditure.ExpenseID AS nvarchar(20)) AS InvoiceNo, N'expense' AS InvoiceKind
FROM Expenditure
INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
LEFT JOIN Account ON Expenditure.AccountID = Account.AccountID
WHERE Expenditure.SchoolID = @SchoolID
  AND Expenditure.RegistrationID = @RegID
  {SqlDateRange("Expenditure.ExpenseDate")}
UNION ALL
SELECT CAST(N'' AS nvarchar(300)),
       ISNULL(Account.AccountName, 'N/A'), Employee_Payorder_Name.Payorder_Name,
       Employee_Payorder_Records.Paid_For, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date,
       CAST(Employee_Payorder_Records.Employee_Payorder_RecordID AS nvarchar(20)), N'pay'
FROM Employee_Payorder_Records
INNER JOIN Employee_Payorder ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Records.Employee_PayorderID
INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
LEFT JOIN Account ON Employee_Payorder_Records.AccountID = Account.AccountID
WHERE Employee_Payorder_Records.SchoolID = @SchoolID
  AND Employee_Payorder_Records.RegistrationID = @RegID
  {SqlDateRange("Employee_Payorder_Records.Paid_date")}
ORDER BY [Date] DESC
""", session, from, to, "%", ct, extra: BindReg);

        dto.IncomeGroups = GroupMyLines(incomeLines);
        dto.ExpenseGroups = GroupMyLines(expenseLines);
        return dto;
    }

    public async Task<BalanceRemainingDto> GetMyRemainingBalanceAsync(
        SessionSnapshot session, int registrationId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var regId = registrationId > 0 ? registrationId : session.RegistrationID;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand($"""
SELECT
    (ISNULL((SELECT SUM(Extra_IncomeAmount) FROM Extra_Income
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               {SqlDateRange("Extra_IncomeDate")}), 0)
   + ISNULL((SELECT SUM(PaidAmount) FROM Income_PaymentRecord
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               {SqlDateRange("PaidDate")}), 0)
   + ISNULL((SELECT SUM(TotalAmount) FROM CommitteeMoneyReceipt
             WHERE SchoolId = @SchoolID AND RegistrationId = @RegID
               {SqlDateRange("PaidDate")}), 0))
  - (ISNULL((SELECT SUM(Amount) FROM Expenditure
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               {SqlDateRange("ExpenseDate")}), 0)
   + ISNULL((SELECT SUM(Amount) FROM Employee_Payorder_Records
             WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
               {SqlDateRange("Paid_date")}), 0))
  - ISNULL((SELECT SUM(SubmissionAmount) FROM User_Balance_Submission
            WHERE SchoolID = @SchoolID AND RegistrationID = @RegID
              {SqlDateRange("SubmissionDate")}), 0)
""", con);
        cmd.CommandTimeout = SummaryCommandTimeout;
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
        var all = new List<AccountDetailDto>();
        await using (var cmd = new SqlCommand("""
SELECT AccountID, AccountName, AccountBalance
FROM Account
WHERE SchoolID = @SchoolID
ORDER BY AccountName
""", con))
        {
            AddSchool(cmd, session);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                all.Add(new AccountDetailDto
                {
                    AccountID = ToInt(reader["AccountID"]),
                    AccountName = Text(reader["AccountName"]),
                    Balance = ToDec(reader["AccountBalance"])
                });
            }
        }

        if (all.Count == 0) return [];
        var minId = all.Min(a => a.AccountID);
        var single = all.Count == 1;
        var filterId = int.TryParse(accountId, out var parsed) && parsed > 0 ? parsed : 0;
        var items = filterId > 0 ? all.Where(a => a.AccountID == filterId).ToList() : all;
        if (items.Count == 0) return [];
        var map = items.ToDictionary(a => a.AccountID);

        const string accountFilter = """
  AND (
        @SingleAccount = 1
        OR @FilterAccount = 0
        OR ISNULL(AccountID, @MinAccountID) = @AccountID
      )
""";

        void BindAccountFilter(SqlCommand cmd)
        {
            cmd.CommandTimeout = SummaryCommandTimeout;
            AddSchool(cmd, session);
            AddDates(cmd, from, to);
            cmd.Parameters.AddWithValue("@MinAccountID", minId);
            cmd.Parameters.AddWithValue("@SingleAccount", single ? 1 : 0);
            cmd.Parameters.AddWithValue("@FilterAccount", filterId > 0 ? 1 : 0);
            cmd.Parameters.AddWithValue("@AccountID", filterId);
        }

        await using (var cmd = new SqlCommand($"""
SELECT ISNULL(AccountID, @MinAccountID) AS AccountID,
       Add_Subtraction, In_Ex_type, Insert_Up_De, SubCategory,
       SUM(Amount) AS Amount,
       MAX(CASE WHEN ClassOrOtherCategory LIKE N'%Student Payment%' OR Details LIKE N'%Receipt No:%' THEN 1 ELSE 0 END) AS IsReceipt
FROM Account_Log
WHERE SchoolID = @SchoolID
  {SqlDateRange("Insert_Date")}
  {accountFilter}
GROUP BY ISNULL(AccountID, @MinAccountID), Add_Subtraction, In_Ex_type, Insert_Up_De, SubCategory
""", con))
        {
            BindAccountFilter(cmd);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["AccountID"]);
                if (!map.TryGetValue(id, out var acc)) continue;
                var addSub = Text(reader["Add_Subtraction"]);
                var inEx = Text(reader["In_Ex_type"]);
                var upDe = Text(reader["Insert_Up_De"]);
                var name = Text(reader["SubCategory"]);
                var amount = ToDec(reader["Amount"]);
                var isReceipt = ToInt(reader["IsReceipt"]) == 1;
                if (addSub.Equals("Add", StringComparison.OrdinalIgnoreCase))
                    acc.TotalAdd += amount;
                else if (addSub.Equals("Subtraction", StringComparison.OrdinalIgnoreCase))
                    acc.TotalSub += amount;

                if (string.IsNullOrWhiteSpace(name) || amount == 0) continue;
                var isAdd = addSub.Equals("Add", StringComparison.OrdinalIgnoreCase);
                var isSub = addSub.Equals("Subtraction", StringComparison.OrdinalIgnoreCase);
                if (isAdd && inEx.Equals("In", StringComparison.OrdinalIgnoreCase))
                    AddDetailCat(acc.Adds, name, amount, "");
                else if (isAdd)
                    AddDetailCat(acc.AddAdjust, name, amount, AccountLogBadge(upDe, addSub, isReceipt));
                else if (isSub && inEx.Equals("Ex", StringComparison.OrdinalIgnoreCase))
                    AddDetailCat(acc.Subs, name, amount, "");
                else if (isSub)
                    AddDetailCat(acc.SubAdjust, name, amount, AccountLogBadge(upDe, addSub, isReceipt));
            }
        }

        await using (var cmd = new SqlCommand($"""
SELECT AccountID, Opening, Closing
FROM (
    SELECT ISNULL(AccountID, @MinAccountID) AS AccountID,
           FIRST_VALUE(Balance_Before) OVER (PARTITION BY ISNULL(AccountID, @MinAccountID) ORDER BY Insert_Date, Insert_Time, AccountLogID) AS Opening,
           FIRST_VALUE(Balance_After) OVER (PARTITION BY ISNULL(AccountID, @MinAccountID) ORDER BY Insert_Date DESC, Insert_Time DESC, AccountLogID DESC) AS Closing,
           ROW_NUMBER() OVER (PARTITION BY ISNULL(AccountID, @MinAccountID) ORDER BY Insert_Date, Insert_Time, AccountLogID) AS rn
    FROM Account_Log
    WHERE SchoolID = @SchoolID
      {SqlDateRange("Insert_Date")}
      {accountFilter}
) t
WHERE rn = 1
""", con))
        {
            BindAccountFilter(cmd);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = ToInt(reader["AccountID"]);
                if (!map.TryGetValue(id, out var acc)) continue;
                acc.Opening = ToDec(reader["Opening"]);
                acc.Closing = ToDec(reader["Closing"]);
            }
        }

        return items.Where(a => a.TotalAdd != 0 || a.TotalSub != 0).ToList();
    }

    private static void AddDetailCat(List<AccountDetailCatDto> list, string name, decimal amount, string badge)
    {
        var hit = list.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Badge == badge);
        if (hit is not null) { hit.Amount += amount; return; }
        list.Add(new AccountDetailCatDto { Name = name, Amount = amount, Badge = badge });
    }

    private static string AccountLogBadge(string insertUpDe, string addSub, bool isReceipt)
    {
        if (insertUpDe.Equals("De", StringComparison.OrdinalIgnoreCase) && addSub.Equals("Add", StringComparison.OrdinalIgnoreCase))
            return "deleted";
        if (insertUpDe.Equals("De", StringComparison.OrdinalIgnoreCase) && addSub.Equals("Subtraction", StringComparison.OrdinalIgnoreCase) && isReceipt)
            return "unpaid";
        if (insertUpDe.Equals("De", StringComparison.OrdinalIgnoreCase))
            return "deleted";
        if (insertUpDe.Equals("Up", StringComparison.OrdinalIgnoreCase))
            return "adjust";
        return "adjust";
    }

    public async Task<AccountsLogDto> GetLogAsync(SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
        from ??= DateTime.Today;
        to ??= DateTime.Today;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var rows = new List<LogRow>();
        await using (var cmd = new SqlCommand($"""
SELECT AL.Log_SN, AL.ClassOrOtherCategory, AL.SubCategory, AL.Amount, AL.Details,
       ISNULL(Reg.UserName, N'') AS UserName,
       AL.Insert_Date, AL.Activity_Date, CONVERT(varchar(15), AL.Insert_Time, 100) AS Insert_Time,
       AL.In_Ex_type, AL.Insert_Up_De, AL.Add_Subtraction
FROM Account_Log AL
LEFT JOIN Registration Reg ON AL.RegistrationID = Reg.RegistrationID
WHERE AL.SchoolID = @SchoolID
  {SqlDateRange("AL.Insert_Date")}
ORDER BY AL.ClassOrOtherCategory, AL.Insert_Date, AL.Insert_Time, AL.Log_SN
""", con))
        {
            cmd.CommandTimeout = SummaryCommandTimeout;
            AddSchool(cmd, session);
            AddDates(cmd, from, to);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new LogRow(
                    ToInt(reader["Log_SN"]),
                    Text(reader["ClassOrOtherCategory"]),
                    Text(reader["SubCategory"]),
                    ToDec(reader["Amount"]),
                    Text(reader["Details"]),
                    Text(reader["UserName"]),
                    Day(reader["Insert_Date"]),
                    reader["Activity_Date"] is DBNull ? null : Convert.ToDateTime(reader["Activity_Date"]),
                    Text(reader["Insert_Time"]),
                    Text(reader["In_Ex_type"]).Trim(),
                    Text(reader["Insert_Up_De"]).Trim(),
                    Text(reader["Add_Subtraction"]).Trim()));
            }
        }

        static bool IsIncomeTotal(LogRow r) =>
            r.InEx.Equals("In", StringComparison.OrdinalIgnoreCase)
            && r.UpDe.Equals("In", StringComparison.OrdinalIgnoreCase);
        static bool IsExpenseTotal(LogRow r) =>
            r.InEx.Equals("Ex", StringComparison.OrdinalIgnoreCase)
            && r.UpDe.Equals("In", StringComparison.OrdinalIgnoreCase);
        static bool IsAdjust(LogRow r) =>
            !r.UpDe.Equals("In", StringComparison.OrdinalIgnoreCase);
        static bool IsIncomeLine(LogRow r) =>
            IsIncomeTotal(r)
            && r.AddSub.Equals("Add", StringComparison.OrdinalIgnoreCase)
            && r.Category.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) < 0
            && r.Category.IndexOf("Deleted", StringComparison.OrdinalIgnoreCase) < 0;
        static bool IsExpenseLine(LogRow r) =>
            IsExpenseTotal(r)
            && r.AddSub.Equals("Subtraction", StringComparison.OrdinalIgnoreCase)
            && r.Category.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) < 0
            && r.Category.IndexOf("Deleted", StringComparison.OrdinalIgnoreCase) < 0;

        return new AccountsLogDto
        {
            IncomeTotal = rows.Where(IsIncomeTotal).Sum(r => r.Amount),
            ExpenseTotal = rows.Where(IsExpenseTotal).Sum(r => r.Amount),
            AdjustTotal = rows.Where(IsAdjust).Sum(r => r.Amount),
            Income = PackLogGroups(rows.Where(IsIncomeLine)),
            Expense = PackLogGroups(rows.Where(IsExpenseLine)),
            Adjust = PackLogGroups(rows.Where(IsAdjust))
        };
    }

    private readonly record struct LogRow(
        int Sn, string Category, string SubCategory, decimal Amount, string Details,
        string UserName, DateTime Date, DateTime? ActivityDate, string Time,
        string InEx, string UpDe, string AddSub);

    private static List<ReportGroupDto> PackLogGroups(IEnumerable<LogRow> rows) =>
        rows.GroupBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ReportGroupDto
            {
                Category = g.First().Category,
                Total = g.Sum(x => x.Amount),
                Lines = g.Select(x => new ReportLineDto
                {
                    Sn = x.Sn,
                    SubCategory = x.SubCategory,
                    Amount = x.Amount,
                    Details = x.Details,
                    UserName = x.UserName,
                    Date = x.Date,
                    ActivityDate = x.ActivityDate,
                    Time = x.Time
                }).ToList()
            }).ToList();

    public async Task<IReadOnlyList<NameAmountDto>> ListIncomeCategoriesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var items = await QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, 0 AS Amount, 0 AS Amount2 FROM (
    SELECT Role AS Category FROM dbo.Income_Roles WHERE SchoolID = @SchoolID AND Role IS NOT NULL AND LTRIM(Role) <> N''
    UNION
    SELECT Extra_Income_CategoryName FROM dbo.Extra_IncomeCategory WHERE SchoolID = @SchoolID AND Extra_Income_CategoryName IS NOT NULL AND LTRIM(Extra_Income_CategoryName) <> N''
) t
ORDER BY Category
""", session, ct);
        items.AddRange(await QueryNamesAsync(con, """
SELECT 0 AS Id, DonationCategory AS Name, 0 AS Amount, 0 AS Amount2
FROM dbo.CommitteeDonationCategory
WHERE SchoolID = @SchoolID AND DonationCategory IS NOT NULL AND LTRIM(DonationCategory) <> N''
""", session, ct, safe: true));
        return items
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListExpenseCategoriesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT 0 AS Id, Category AS Name, 0 AS Amount, 0 AS Amount2 FROM (
    SELECT Payorder_Name AS Category FROM dbo.Employee_Payorder_Name WHERE SchoolID = @SchoolID AND Payorder_Name IS NOT NULL AND LTRIM(Payorder_Name) <> N''
    UNION
    SELECT CategoryName FROM dbo.Expense_CategoryName WHERE SchoolID = @SchoolID AND CategoryName IS NOT NULL AND LTRIM(CategoryName) <> N''
) t
ORDER BY Category
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
    WHERE Income_PaymentRecord.SchoolID = @SchoolID
      AND Income_PaymentRecord.PaidDate >= ISNULL(@From, CONVERT(date, '17530101'))
      AND Income_PaymentRecord.PaidDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
    GROUP BY Income_Roles.Role
    UNION ALL
    SELECT Extra_IncomeCategory.Extra_Income_CategoryName, SUM(Extra_Income.Extra_IncomeAmount)
    FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
    WHERE Extra_Income.SchoolID = @SchoolID
      AND Extra_Income.Extra_IncomeDate >= ISNULL(@From, CONVERT(date, '17530101'))
      AND Extra_Income.Extra_IncomeDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
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
  AND CommitteeMoneyReceipt.PaidDate >= ISNULL(@From, CONVERT(date, '17530101'))
  AND CommitteeMoneyReceipt.PaidDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
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
    WHERE Employee_Payorder_Records.SchoolID = @SchoolID
      AND Employee_Payorder_Records.Paid_date >= ISNULL(@From, CONVERT(date, '17530101'))
      AND Employee_Payorder_Records.Paid_date < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
    GROUP BY Employee_Payorder_Name.Payorder_Name
    UNION ALL
    SELECT Expense_CategoryName.CategoryName, SUM(Expenditure.Amount)
    FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
    WHERE Expenditure.SchoolID = @SchoolID
      AND Expenditure.ExpenseDate >= ISNULL(@From, CONVERT(date, '17530101'))
      AND Expenditure.ExpenseDate < DATEADD(day, 1, ISNULL(@To, CONVERT(date, '99991230')))
    GROUP BY Expense_CategoryName.CategoryName
) t WHERE Category LIKE @Category GROUP BY Category
""", session, ct, extra: c => { AddDates(c, from, to); c.Parameters.AddWithValue("@Category", like); });

    private static List<ReportGroupDto> GroupMyLines(List<ReportLineDto> lines) =>
        lines.GroupBy(l => l.Category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ReportGroupDto
            {
                Category = g.First().Category,
                Total = g.Sum(x => x.Amount),
                Lines = g.OrderByDescending(x => x.Date).ToList()
            }).ToList();

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

    private async Task<List<ReportLineDto>> QueryLinesAsync(SqlConnection con, string sql, SessionSnapshot session, DateTime? from, DateTime? to, string category, CancellationToken ct, bool safe = false, Action<SqlCommand>? extra = null)
    {
        var items = new List<ReportLineDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = SummaryCommandTimeout;
            AddSchool(cmd, session);
            AddDates(cmd, from, to);
            cmd.Parameters.AddWithValue("@Category", category);
            extra?.Invoke(cmd);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new ReportLineDto
                {
                    UserName = Text(reader["UserName"]),
                    AccountName = Text(reader["AccountName"]),
                    Category = Text(reader["Category"]),
                    Details = Text(reader["Details"]),
                    ReceiptNo = Col(reader, "InvoiceNo"),
                    ReceiptKind = Col(reader, "InvoiceKind"),
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
