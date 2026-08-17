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
        var dto = new AccountsSummaryDto
        {
            TotalIncome = await ScalarAsync(con, """
SELECT ISNULL((SELECT SUM(Extra_IncomeAmount) FROM Extra_Income WHERE SchoolID = @SchoolID), 0)
 + ISNULL((SELECT SUM(PaidAmount) FROM Income_PaymentRecord WHERE SchoolID = @SchoolID), 0)
 + ISNULL((SELECT SUM(TotalAmount) FROM CommitteeMoneyReceipt WHERE SchoolId = @SchoolID), 0)
""", c => AddSchool(c, session), ct),
            TotalExpense = await ScalarAsync(con, """
SELECT ISNULL((SELECT SUM(Amount) FROM Expenditure WHERE SchoolID = @SchoolID), 0)
 + ISNULL((SELECT SUM(Amount) FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID), 0)
""", c => AddSchool(c, session), ct),
            AccountTotal = await ScalarAsync(con, "SELECT ISNULL(SUM(AccountBalance), 0) FROM Account WHERE SchoolID = @SchoolID", c => AddSchool(c, session), ct)
        };
        dto.Net = dto.TotalIncome - dto.TotalExpense;

        await using (var cmd = new SqlCommand("""
SELECT ISNULL(SUM(Amount), 0) AS TotalFee,
       ISNULL(SUM(LateFeeCountable), 0) AS LateFee,
       ISNULL(SUM(Total_Discount), 0) AS Concession,
       ISNULL(SUM(PaidAmount), 0) AS Paid,
       ISNULL(SUM(Receivable_Amount), 0) AS Unpaid,
       (SELECT ISNULL(SUM(Receivable_Amount), 0) FROM Income_PayOrder WHERE EndDate < GETDATE() AND SchoolID = @SchoolID AND Is_Active = 1) AS PresentDue,
       (SELECT ISNULL(SUM(PaidAmount), 0) FROM Income_PayOrder WHERE StartDate > GETDATE() AND SchoolID = @SchoolID) AS Advance
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND Is_Active = 1
""", con))
        {
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

        dto.Users = await QueryNamesAsync(con, """
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
""", session, ct);

        dto.Accounts = await QueryNamesAsync(con, """
SELECT AccountID AS Id, AccountName AS Name, AccountBalance AS Amount, 0 AS Amount2
FROM Account WHERE SchoolID = @SchoolID
""", session, ct);

        dto.IncomeCategories = await QueryNamesAsync(con, """
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
""", session, ct);
        dto.IncomeCategories.AddRange(await QueryNamesAsync(con, """
SELECT 0 AS Id, CommitteeDonationCategory.DonationCategory AS Name, SUM(CommitteePaymentRecord.PaidAmount) AS Amount, 0 AS Amount2
FROM CommitteePaymentRecord
INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
WHERE CommitteePaymentRecord.SchoolId = @SchoolID
GROUP BY CommitteeDonationCategory.DonationCategory
""", session, ct, safe: true));

        dto.ExpenseCategories = await QueryNamesAsync(con, """
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
""", session, ct);

        dto.Sessions = await LoadSessionsAsync(con, session, ct);
        return dto;
    }

    private async Task<List<SessionReportDto>> LoadSessionsAsync(SqlConnection con, SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<SessionReportDto>();
        await using var cmd = new SqlCommand("""
SELECT Edu_Year.EducationYearID, Education_Year.EducationYear, Education_Year.StartDate, Education_Year.EndDate,
       ISNULL(Ex_In_T.Ex_In, 0) + ISNULL(Stu_In_T.Stu_In, 0) + ISNULL(Com_In_T.Com_In, 0) AS Income,
       ISNULL(Ex_T.Ex, 0) + ISNULL(Emp_Ex_T.Emp_Ex, 0) AS Expense
FROM (
    SELECT DISTINCT EducationYearID FROM Extra_Income WHERE SchoolID = @SchoolID
    UNION SELECT DISTINCT EducationYearID FROM Income_PaymentRecord WHERE SchoolID = @SchoolID
    UNION SELECT DISTINCT EducationYearID FROM Expenditure WHERE SchoolID = @SchoolID
    UNION SELECT DISTINCT EducationYearID FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID
) AS Edu_Year
INNER JOIN Education_Year ON Edu_Year.EducationYearID = Education_Year.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.Amount), 0) AS Ex
    FROM Education_Year INNER JOIN Expenditure A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.ExpenseDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Ex_T ON Edu_Year.EducationYearID = Ex_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.Extra_IncomeAmount), 0) AS Ex_In
    FROM Education_Year INNER JOIN Extra_Income A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.Extra_IncomeDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Ex_In_T ON Edu_Year.EducationYearID = Ex_In_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.PaidAmount), 0) AS Stu_In
    FROM Education_Year INNER JOIN Income_PaymentRecord A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.PaidDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Stu_In_T ON Edu_Year.EducationYearID = Stu_In_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.Amount), 0) AS Emp_Ex
    FROM Education_Year INNER JOIN Employee_Payorder_Records A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.Paid_date BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Emp_Ex_T ON Edu_Year.EducationYearID = Emp_Ex_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(C_T.TotalAmount), 0) AS Com_In
    FROM Education_Year INNER JOIN CommitteeMoneyReceipt C_T ON Education_Year.SchoolID = C_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND C_T.PaidDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Com_In_T ON Edu_Year.EducationYearID = Com_In_T.EducationYearID
ORDER BY Education_Year.StartDate DESC
""", con);
        AddSchool(cmd, session);
        try
        {
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
        }
        catch
        {
            items.Clear();
            await using var fallback = new SqlCommand("""
SELECT Edu_Year.EducationYearID, Education_Year.EducationYear, Education_Year.StartDate, Education_Year.EndDate,
       ISNULL(Ex_In_T.Ex_In, 0) + ISNULL(Stu_In_T.Stu_In, 0) AS Income,
       ISNULL(Ex_T.Ex, 0) + ISNULL(Emp_Ex_T.Emp_Ex, 0) AS Expense
FROM (
    SELECT DISTINCT EducationYearID FROM Extra_Income WHERE SchoolID = @SchoolID
    UNION SELECT DISTINCT EducationYearID FROM Income_PaymentRecord WHERE SchoolID = @SchoolID
    UNION SELECT DISTINCT EducationYearID FROM Expenditure WHERE SchoolID = @SchoolID
    UNION SELECT DISTINCT EducationYearID FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID
) AS Edu_Year
INNER JOIN Education_Year ON Edu_Year.EducationYearID = Education_Year.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.Amount), 0) AS Ex
    FROM Education_Year INNER JOIN Expenditure A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.ExpenseDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Ex_T ON Edu_Year.EducationYearID = Ex_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.Extra_IncomeAmount), 0) AS Ex_In
    FROM Education_Year INNER JOIN Extra_Income A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.Extra_IncomeDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Ex_In_T ON Edu_Year.EducationYearID = Ex_In_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.PaidAmount), 0) AS Stu_In
    FROM Education_Year INNER JOIN Income_PaymentRecord A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.PaidDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Stu_In_T ON Edu_Year.EducationYearID = Stu_In_T.EducationYearID
LEFT JOIN (
    SELECT Education_Year.EducationYearID, ISNULL(SUM(A_T.Amount), 0) AS Emp_Ex
    FROM Education_Year INNER JOIN Employee_Payorder_Records A_T ON Education_Year.SchoolID = A_T.SchoolID
    WHERE Education_Year.SchoolID = @SchoolID AND A_T.Paid_date BETWEEN Education_Year.StartDate AND Education_Year.EndDate
    GROUP BY Education_Year.EducationYearID
) Emp_Ex_T ON Edu_Year.EducationYearID = Emp_Ex_T.EducationYearID
ORDER BY Education_Year.StartDate DESC
""", con);
            AddSchool(fallback, session);
            await using var reader = await fallback.ExecuteReaderAsync(ct);
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
        }

        foreach (var year in items)
        {
            await using (var pay = new SqlCommand("""
SELECT ISNULL(SUM(Amount), 0), ISNULL(SUM(LateFeeCountable), 0), ISNULL(SUM(Total_Discount), 0),
       ISNULL(SUM(PaidAmount), 0), ISNULL(SUM(Receivable_Amount), 0)
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND EducationYearID = @YearID AND Is_Active = 1
""", con))
            {
                AddSchool(pay, session);
                pay.Parameters.AddWithValue("@YearID", year.EducationYearID);
                await using var reader = await pay.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    year.Payorder = ToDec(reader[0]);
                    year.LateFee = ToDec(reader[1]);
                    year.Concession = ToDec(reader[2]);
                    year.Paid = ToDec(reader[3]);
                    year.Unpaid = ToDec(reader[4]);
                }
            }

            year.Months = await QueryNamesAsync(con, """
SELECT 0 AS Id, M_T.Months AS Name,
       ISNULL(Ex_In_T.Ex_In, 0) + ISNULL(Stu_In_T.Stu_In, 0) AS Amount,
       ISNULL(Ex_T.Ex, 0) + ISNULL(Emp_Ex_T.Emp_Ex, 0) AS Amount2
FROM (
    SELECT FORMAT(Extra_IncomeDate, 'MMM yyyy') AS Months FROM Extra_Income
    WHERE SchoolID = @SchoolID AND Extra_IncomeDate BETWEEN @Start AND @End GROUP BY FORMAT(Extra_IncomeDate, 'MMM yyyy')
    UNION SELECT FORMAT(PaidDate, 'MMM yyyy') FROM Income_PaymentRecord
    WHERE SchoolID = @SchoolID AND CAST(PaidDate AS DATE) BETWEEN @Start AND @End GROUP BY FORMAT(PaidDate, 'MMM yyyy')
    UNION SELECT FORMAT(ExpenseDate, 'MMM yyyy') FROM Expenditure
    WHERE SchoolID = @SchoolID AND ExpenseDate BETWEEN @Start AND @End GROUP BY FORMAT(ExpenseDate, 'MMM yyyy')
    UNION SELECT FORMAT(Paid_date, 'MMM yyyy') FROM Employee_Payorder_Records
    WHERE SchoolID = @SchoolID AND Paid_date BETWEEN @Start AND @End GROUP BY FORMAT(Paid_date, 'MMM yyyy')
) M_T
LEFT JOIN (
    SELECT ISNULL(SUM(Extra_IncomeAmount), 0) AS Ex_In, FORMAT(Extra_IncomeDate, 'MMM yyyy') AS Months
    FROM Extra_Income WHERE SchoolID = @SchoolID AND Extra_IncomeDate BETWEEN @Start AND @End
    GROUP BY FORMAT(Extra_IncomeDate, 'MMM yyyy')
) Ex_In_T ON M_T.Months = Ex_In_T.Months
LEFT JOIN (
    SELECT ISNULL(SUM(PaidAmount), 0) AS Stu_In, FORMAT(PaidDate, 'MMM yyyy') AS Months
    FROM Income_PaymentRecord WHERE SchoolID = @SchoolID AND CAST(PaidDate AS DATE) BETWEEN @Start AND @End
    GROUP BY FORMAT(PaidDate, 'MMM yyyy')
) Stu_In_T ON M_T.Months = Stu_In_T.Months
LEFT JOIN (
    SELECT ISNULL(SUM(Amount), 0) AS Ex, FORMAT(ExpenseDate, 'MMM yyyy') AS Months
    FROM Expenditure WHERE SchoolID = @SchoolID AND ExpenseDate BETWEEN @Start AND @End
    GROUP BY FORMAT(ExpenseDate, 'MMM yyyy')
) Ex_T ON M_T.Months = Ex_T.Months
LEFT JOIN (
    SELECT ISNULL(SUM(Amount), 0) AS Emp_Ex, FORMAT(Paid_date, 'MMM yyyy') AS Months
    FROM Employee_Payorder_Records WHERE SchoolID = @SchoolID AND Paid_date BETWEEN @Start AND @End
    GROUP BY FORMAT(Paid_date, 'MMM yyyy')
) Emp_Ex_T ON M_T.Months = Emp_Ex_T.Months
ORDER BY CONVERT(date, M_T.Months)
""", session, ct, extra: c =>
            {
                c.Parameters.AddWithValue("@Start", year.StartDate);
                c.Parameters.AddWithValue("@End", year.EndDate);
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
            InstitutionDue = await ScalarAsync(con, """
SELECT ISNULL(SUM(CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
    THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
    ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END), 0)
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND po.EndDate < CAST(GETDATE() AS date) AND s.Status = N'Active'
""", c =>
            {
                AddSchool(c, session);
                c.Parameters.AddWithValue("@YearID", session.EducationYearID);
            }, ct)
        };

        if (!string.IsNullOrWhiteSpace(studentId))
        {
            await using var byId = new SqlCommand("""
SELECT po.StudentID, s.ID, s.StudentsName, sc.RollNo, s.SMSPhoneNo, CreateClass.Class,
       SUM(CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
           THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
           ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END) AS Due
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
INNER JOIN StudentsClass sc ON s.StudentID = sc.StudentID AND sc.EducationYearID = @YearID AND sc.Class_Status IS NULL
INNER JOIN CreateClass ON sc.ClassID = CreateClass.ClassID
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND po.EndDate < CAST(GETDATE() AS date) AND s.Status = N'Active' AND s.ID = @ID
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

        await using var cmd = new SqlCommand("""
SELECT po.StudentID, s.ID, s.StudentsName, sc.RollNo, s.SMSPhoneNo,
       SUM(CASE WHEN po.EndDate < DATEADD(day, -1, CAST(GETDATE() AS date))
           THEN ISNULL(po.Amount, 0) + ISNULL(po.LateFee, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) - ISNULL(po.LateFee_Discount, 0)
           ELSE ISNULL(po.Amount, 0) - ISNULL(po.Discount, 0) - ISNULL(po.PaidAmount, 0) END) AS Due
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
INNER JOIN StudentsClass sc ON s.StudentID = sc.StudentID AND sc.EducationYearID = @YearID AND sc.ClassID = @ClassID AND sc.Class_Status IS NULL
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND po.EndDate < CAST(GETDATE() AS date) AND s.Status = N'Active'
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
        await using var cmd = new SqlCommand("""
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
WHERE po.SchoolID = @SchoolID AND po.Status = N'Due' AND po.Is_Active = 1 AND po.EndDate < GETDATE()
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
        return dto;
    }

    public async Task<IReadOnlyList<NameAmountDto>> ListDueRolesAsync(SessionSnapshot session, int classId, CancellationToken ct)
    {
        if (classId <= 0)
            return [];
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await QueryNamesAsync(con, """
SELECT DISTINCT ir.RoleID AS Id, ir.Role AS Name, 0 AS Amount, 0 AS Amount2
FROM Income_PayOrder po
INNER JOIN Income_Roles ir ON po.RoleID = ir.RoleID
INNER JOIN StudentsClass sc ON po.StudentID = sc.StudentID AND sc.EducationYearID = @YearID AND sc.ClassID = @ClassID AND sc.Class_Status IS NULL
WHERE po.SchoolID = @SchoolID AND po.EducationYearID = @YearID AND po.Status = N'Due' AND po.Is_Active = 1
  AND po.EndDate < CAST(GETDATE() AS date)
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
        var dto = new PayorderReportDto();
        await using (var tot = new SqlCommand("""
SELECT ISNULL(SUM(Amount), 0), ISNULL(SUM(LateFeeCountable), 0), ISNULL(SUM(Total_Discount), 0), ISNULL(SUM(PaidAmount), 0),
       ISNULL(SUM(CASE WHEN Status = 'Paid' THEN 0
            WHEN EndDate < GETDATE() - 1 THEN ISNULL(Amount, 0) + ISNULL(LateFee, 0) - ISNULL(Discount, 0) - ISNULL(PaidAmount, 0) - ISNULL(LateFee_Discount, 0)
            ELSE ISNULL(Amount, 0) - ISNULL(Discount, 0) - ISNULL(PaidAmount, 0) END), 0)
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND Is_Active = 1 AND EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
""", con))
        {
            AddSchool(tot, session);
            AddDates(tot, from, to);
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

        await using var roles = new SqlCommand("""
SELECT Income_Roles.RoleID, Income_Roles.Role,
       SUM(Income_PayOrder.Amount) AS Fee, SUM(LateFeeCountable) AS LateFee,
       SUM(Total_Discount) AS Concession, SUM(Income_PayOrder.PaidAmount) AS Paid, SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder
INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.Is_Active = 1
  AND EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY Income_Roles.Role, Income_Roles.RoleID
ORDER BY Income_Roles.Role
""", con);
        AddSchool(roles, session);
        AddDates(roles, from, to);
        await using (var reader = await roles.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Roles.Add(new PayorderRoleDto
                {
                    RoleID = ToInt(reader["RoleID"]),
                    Role = Text(reader["Role"]),
                    Fee = ToDec(reader["Fee"]),
                    LateFee = ToDec(reader["LateFee"]),
                    Concession = ToDec(reader["Concession"]),
                    Paid = ToDec(reader["Paid"]),
                    Unpaid = ToDec(reader["Unpaid"])
                });
            }
        }

        if (roleId > 0)
        {
            var selected = dto.Roles.FirstOrDefault(x => x.RoleID == roleId);
            if (selected is not null)
            {
                await using var payFor = new SqlCommand("""
SELECT PayFor, SUM(Amount), SUM(LateFeeCountable), SUM(Total_Discount), SUM(PaidAmount), SUM(Receivable_Amount)
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND Is_Active = 1 AND RoleID = @RoleID
  AND EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY PayFor ORDER BY MAX(EndDate)
""", con);
                AddSchool(payFor, session);
                AddDates(payFor, from, to);
                payFor.Parameters.AddWithValue("@RoleID", roleId);
                await using var reader = await payFor.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    selected.PayFors.Add(new PayorderRoleDto
                    {
                        Role = Text(reader[0]),
                        Fee = ToDec(reader[1]),
                        LateFee = ToDec(reader[2]),
                        Concession = ToDec(reader[3]),
                        Paid = ToDec(reader[4]),
                        Unpaid = ToDec(reader[5])
                    });
                }
            }
        }

        return dto;
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

    public async Task<List<AccountDetailDto>> GetAccountDetailsAsync(SessionSnapshot session, string? accountId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var items = new List<AccountDetailDto>();
        await using var cmd = new SqlCommand("""
SELECT AccountID, AccountName, AccountBalance,
       ISNULL((SELECT SUM(Amount) FROM Account_Log WHERE SchoolID = @SchoolID AND AccountID = Account.AccountID AND Add_Subtraction = 'Add' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0) AS Total_In,
       ISNULL((SELECT SUM(Amount) FROM Account_Log WHERE SchoolID = @SchoolID AND AccountID = Account.AccountID AND Add_Subtraction = 'Subtraction' AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')), 0) AS Total_Ex,
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
            acc.Adds = await LogCatsAsync(con, session, acc.AccountID, from, to, "Add", "In", true, ct);
            acc.AddAdjust = await LogCatsAsync(con, session, acc.AccountID, from, to, "Add", "In", false, ct);
            acc.Subs = await LogCatsAsync(con, session, acc.AccountID, from, to, "Subtraction", "Ex", true, ct);
            acc.SubAdjust = await LogCatsAsync(con, session, acc.AccountID, from, to, "Subtraction", "Ex", false, ct);
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
            Income = await LogGroupsAsync(con, session, from, to, "In_Ex_type = 'In' AND Insert_Up_De = 'In'", "Add_Subtraction = N'Add' AND ClassOrOtherCategory NOT LIKE '%Updated%' AND ClassOrOtherCategory NOT LIKE '%Deleted%'", ct),
            Expense = await LogGroupsAsync(con, session, from, to, "In_Ex_type = 'Ex' AND Insert_Up_De = 'In'", "1 = 1", ct),
            Adjust = await LogGroupsAsync(con, session, from, to, "Insert_Up_De <> 'In'", "Insert_Up_De <> 'In'", ct)
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

    private async Task<List<NameAmountDto>> QueryNamesAsync(SqlConnection con, string sql, SessionSnapshot session, CancellationToken ct, Action<SqlCommand>? extra = null, bool safe = false)
    {
        var items = new List<NameAmountDto>();
        try
        {
            await using var cmd = new SqlCommand(sql, con);
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

    private async Task<List<NameAmountDto>> LogCatsAsync(SqlConnection con, SessionSnapshot session, int accountId, DateTime? from, DateTime? to, string addSub, string inEx, bool matchType, CancellationToken ct)
    {
        var op = matchType ? "=" : "<>";
        return await QueryNamesAsync(con, $"""
SELECT 0 AS Id, SubCategory AS Name, SUM(Amount) AS Amount, 0 AS Amount2
FROM Account_Log
WHERE SchoolID = @SchoolID AND AccountID = @AccountID AND Add_Subtraction = @AddSub AND In_Ex_type {op} @InEx
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

    private async Task<List<ReportGroupDto>> LogGroupsAsync(SqlConnection con, SessionSnapshot session, DateTime? from, DateTime? to, string groupWhere, string lineWhere, CancellationToken ct)
    {
        var groups = new List<ReportGroupDto>();
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
INNER JOIN Registration Reg ON AL.RegistrationID = Reg.RegistrationID
OUTER APPLY (SELECT TRY_CAST(LTRIM(RTRIM(SUBSTRING(AL.Details, CHARINDEX('ID =', AL.Details) + 4, 10))) AS INT) AS OpRegID) P
LEFT JOIN Registration OpReg ON OpReg.RegistrationID = P.OpRegID
WHERE AL.SchoolID = @SchoolID AND AL.ClassOrOtherCategory = @Cat AND {lineWhere}
  AND AL.Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
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
FROM Account_Log
WHERE SchoolID = @SchoolID AND ClassOrOtherCategory = @Cat AND Insert_Date BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
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
