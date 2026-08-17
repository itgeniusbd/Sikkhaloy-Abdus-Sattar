using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class ReportsService
{
    public async Task<SessionFilterDto> GetSessionFiltersAsync(
        SessionSnapshot session, int yearId, int classId, string? roleId, string? kind, DateTime? from, DateTime? to, CancellationToken ct)
    {
        yearId = Year(session, yearId);
        var extra = KindWhere(kind);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new SessionFilterDto
        {
            Classes = await QueryNamesAsync(con, $"""
SELECT DISTINCT CreateClass.ClassID AS Id, CreateClass.Class AS Name, 0 AS Amount, 0 AS Amount2
FROM CreateClass INNER JOIN Income_PayOrder ON CreateClass.ClassID = Income_PayOrder.ClassID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID {extra}
ORDER BY CreateClass.ClassID
""", session, ct, extra: c => BindYear(c, yearId))
        };
        if (classId > 0)
        {
            dto.Sections = await QueryNamesAsync(con, """
SELECT DISTINCT CreateSection.SectionID AS Id, CreateSection.Section AS Name, 0 AS Amount, 0 AS Amount2
FROM StudentsClass
INNER JOIN Income_PayOrder ON StudentsClass.StudentClassID = Income_PayOrder.StudentClassID
INNER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE Income_PayOrder.Is_Active = 1 AND StudentsClass.SchoolID = @SchoolID
  AND StudentsClass.EducationYearID = @EducationYearID AND StudentsClass.ClassID = @ClassID
""", session, ct, extra: c => { BindYear(c, yearId); c.Parameters.AddWithValue("@ClassID", classId); });
            dto.Roles = await QueryNamesAsync(con, $"""
SELECT DISTINCT Income_Roles.RoleID AS Id, Income_Roles.Role AS Name, 0 AS Amount, 0 AS Amount2
FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND (Income_PayOrder.ClassID = @ClassID OR @ClassID = 0) {extra}
""", session, ct, extra: c => { BindYear(c, yearId); c.Parameters.AddWithValue("@ClassID", classId); });
            dto.PayFors = await QueryNamesAsync(con, """
SELECT 0 AS Id, PayFor AS Name, 0 AS Amount, 0 AS Amount2
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID
  AND RoleID LIKE @RoleID AND Is_Active = 1
  AND EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY PayFor ORDER BY MAX(EndDate)
""", session, ct, extra: c =>
            {
                BindYear(c, yearId);
                AddDates(c, from, to);
                c.Parameters.AddWithValue("@ClassID", classId);
                c.Parameters.AddWithValue("@RoleID", Like(roleId));
            });
        }
        else
        {
            dto.Roles = await QueryNamesAsync(con, $"""
SELECT DISTINCT Income_Roles.RoleID AS Id, Income_Roles.Role AS Name, 0 AS Amount, 0 AS Amount2
FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID {extra}
""", session, ct, extra: c => BindYear(c, yearId));
        }
        return dto;
    }

    public async Task<SessionClassReportDto> GetSessionClassAsync(
        SessionSnapshot session, int yearId, DateTime? from, DateTime? to, int classId, int roleId, CancellationToken ct)
    {
        yearId = Year(session, yearId);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new SessionClassReportDto();
        await using (var cmd = new SqlCommand("""
SELECT EducationYear, StartDate, EndDate FROM Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con))
        {
            AddSchool(cmd, session);
            BindYear(cmd, yearId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.YearName = Text(reader["EducationYear"]);
                dto.StartDate = Day(reader["StartDate"]);
                dto.EndDate = Day(reader["EndDate"]);
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT COUNT(DISTINCT StudentID) AS Total_Stu, SUM(Amount) AS TotalFee, SUM(LateFeeCountable) AS TotalLateFee,
       SUM(Total_Discount) AS TotalDiscount, SUM(ISNULL(PaidAmount, 0)) AS TotalPaid, SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND Is_Active = 1
  AND EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
""", con))
        {
            AddSchool(cmd, session);
            BindYear(cmd, yearId);
            AddDates(cmd, from, to);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Students = ToInt(reader["Total_Stu"]);
                dto.Payorder = ToDec(reader["TotalFee"]);
                dto.LateFee = ToDec(reader["TotalLateFee"]);
                dto.Concession = ToDec(reader["TotalDiscount"]);
                dto.Paid = ToDec(reader["TotalPaid"]);
                dto.Unpaid = ToDec(reader["Unpaid"]);
            }
        }

        dto.Classes = await ReadClassRowsAsync(con, """
SELECT Income_PayOrder.ClassID AS Id, CreateClass.Class AS Name, COUNT(DISTINCT Income_PayOrder.StudentID) AS Students,
       SUM(Income_PayOrder.Amount) AS Fee, SUM(Income_PayOrder.LateFeeCountable) AS LateFee,
       SUM(Income_PayOrder.Total_Discount) AS Concession, SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS Paid,
       SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM Income_PayOrder INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Is_Active = 1
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY Income_PayOrder.ClassID, CreateClass.Class
ORDER BY Income_PayOrder.ClassID
""", session, yearId, from, to, 0, 0, ct);

        if (classId > 0)
        {
            dto.Roles = await ReadClassRowsAsync(con, """
SELECT Income_PayOrder.RoleID AS Id, Income_Roles.Role AS Name, COUNT(DISTINCT Income_PayOrder.StudentID) AS Students,
       SUM(Income_PayOrder.Amount) AS Fee, SUM(Income_PayOrder.LateFeeCountable) AS LateFee,
       SUM(Income_PayOrder.Total_Discount) AS Concession, SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS Paid,
       SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Is_Active = 1 AND Income_PayOrder.ClassID = @ClassID
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY Income_PayOrder.RoleID, Income_Roles.Role
ORDER BY Income_PayOrder.RoleID
""", session, yearId, from, to, classId, 0, ct);
        }

        if (classId > 0 && roleId > 0)
        {
            dto.PayFors = await ReadClassRowsAsync(con, """
SELECT 0 AS Id, PayFor AS Name, COUNT(DISTINCT StudentID) AS Students, SUM(Amount) AS Fee,
       SUM(LateFeeCountable) AS LateFee, SUM(Total_Discount) AS Concession,
       SUM(ISNULL(PaidAmount, 0)) AS Paid, SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND Is_Active = 1
  AND ClassID = @ClassID AND RoleID = @RoleID
  AND EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
GROUP BY PayFor ORDER BY MAX(EndDate)
""", session, yearId, from, to, classId, roleId, ct);
        }

        return dto;
    }

    public Task<SessionStudentReportDto> GetSessionStudentsAsync(
        SessionSnapshot session, int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken ct) =>
        GetSessionStudentCoreAsync(session, "students", yearId, classId, sectionId, roleId, "%", from, to, ct);

    public Task<SessionStudentReportDto> GetSessionPaidAsync(
        SessionSnapshot session, int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken ct) =>
        GetSessionStudentCoreAsync(session, "paid", yearId, classId, sectionId, roleId, payFor, from, to, ct);

    public Task<SessionStudentReportDto> GetSessionDueAsync(
        SessionSnapshot session, int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken ct) =>
        GetSessionStudentCoreAsync(session, "due", yearId, classId, sectionId, roleId, payFor, from, to, ct);

    public Task<SessionStudentReportDto> GetSessionConcessionAsync(
        SessionSnapshot session, int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken ct) =>
        GetSessionStudentCoreAsync(session, "concession", yearId, classId, sectionId, roleId, "%", from, to, ct);

    public async Task<SessionPaidDueDto> GetSessionPaidDueAsync(
        SessionSnapshot session, string? status, string? classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var yearId = session.EducationYearID;
        var classLike = string.IsNullOrWhiteSpace(classId) ? "%" : classId.Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new SessionPaidDueDto();
        await using (var cmd = new SqlCommand("""
SELECT COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu,
       SUM(Income_PayOrder.Amount) AS TotalFee,
       SUM(Income_PayOrder.LateFeeCountable) AS TotalLateFee,
       SUM(Income_PayOrder.Total_Discount) AS TotalDiscount,
       SUM(ISNULL(Income_PayOrder.Amount, 0) - ISNULL(Income_PayOrder.Total_Discount, 0)) AS Receivable_Amount,
       SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS TotalPaid,
       SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM Income_PayOrder
INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID
INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.PayFor LIKE @PayFor AND Income_Roles.RoleID LIKE @RoleID
  AND (@ClassID = '%' OR CAST(Income_PayOrder.ClassID AS nvarchar(20)) LIKE @ClassID)
  AND ISNULL(CAST(StudentsClass.SectionID AS nvarchar(20)), N'0') LIKE @SectionID
  AND Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Is_Active = 1
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
""", con))
        {
            AddSchool(cmd, session);
            BindYear(cmd, yearId);
            AddDates(cmd, from, to);
            cmd.Parameters.AddWithValue("@PayFor", Like(payFor));
            cmd.Parameters.AddWithValue("@RoleID", Like(roleId));
            cmd.Parameters.AddWithValue("@ClassID", classLike);
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Students = ToInt(reader["Total_Stu"]);
                dto.Fee = ToDec(reader["TotalFee"]);
                dto.LateFee = ToDec(reader["TotalLateFee"]);
                dto.Concession = ToDec(reader["TotalDiscount"]);
                dto.Receivable = ToDec(reader["Receivable_Amount"]);
                dto.Paid = ToDec(reader["TotalPaid"]);
                dto.Unpaid = ToDec(reader["Unpaid"]);
            }
        }

        var students = new List<SessionPaidDueStudentDto>();
        await using (var cmd = new SqlCommand("""
SELECT Income_PayOrder.StudentClassID, Student.ID, Student.StudentsName, StudentsClass.RollNo, CreateClass.Class,
       SUM(Income_PayOrder.PaidAmount) AS Total_Paid, SUM(Income_PayOrder.Receivable_Amount) AS Unpaid
FROM Income_PayOrder
INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID
INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID
INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID
WHERE Income_PayOrder.PayFor LIKE @PayFor AND Income_PayOrder.Status LIKE @Status
  AND Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Income_PayOrder.Is_Active = 1
  AND ISNULL(CAST(StudentsClass.SectionID AS nvarchar(20)), N'0') LIKE @SectionID
  AND Income_PayOrder.RoleID LIKE @RoleID
  AND (@ClassID = '%' OR CAST(Income_PayOrder.ClassID AS nvarchar(20)) LIKE @ClassID)
GROUP BY Income_PayOrder.StudentClassID, Student.ID, CreateClass.ClassID, CreateClass.Class, Student.StudentsName, StudentsClass.RollNo
ORDER BY CreateClass.ClassID,
         CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END
""", con))
        {
            AddSchool(cmd, session);
            BindYear(cmd, yearId);
            AddDates(cmd, from, to);
            cmd.Parameters.AddWithValue("@PayFor", Like(payFor));
            cmd.Parameters.AddWithValue("@Status", Like(status));
            cmd.Parameters.AddWithValue("@SectionID", Like(sectionId));
            cmd.Parameters.AddWithValue("@RoleID", Like(roleId));
            cmd.Parameters.AddWithValue("@ClassID", classLike);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                students.Add(new SessionPaidDueStudentDto
                {
                    StudentClassID = ToInt(reader["StudentClassID"]),
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    RollNo = Text(reader["RollNo"]),
                    ClassName = Text(reader["Class"]),
                    Paid = ToDec(reader["Total_Paid"]),
                    Unpaid = ToDec(reader["Unpaid"])
                });
            }
        }

        foreach (var row in students)
        {
            await using var cmd = new SqlCommand("""
SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.Amount, Income_PayOrder.LateFee,
       Income_PayOrder.Total_Discount, Income_PayOrder.PaidAmount, Income_PayOrder.Receivable_Amount, Income_PayOrder.Status
FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE Income_PayOrder.StudentClassID = @StudentClassID AND Income_PayOrder.RoleID LIKE @RoleID
  AND Income_PayOrder.PayFor LIKE @PayFor AND Income_PayOrder.Status LIKE @Status
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
ORDER BY EndDate
""", con);
            AddDates(cmd, from, to);
            cmd.Parameters.AddWithValue("@StudentClassID", row.StudentClassID);
            cmd.Parameters.AddWithValue("@RoleID", Like(roleId));
            cmd.Parameters.AddWithValue("@PayFor", Like(payFor));
            cmd.Parameters.AddWithValue("@Status", Like(status));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                row.Lines.Add(new SessionPaidDueLineDto
                {
                    Role = Text(reader["Role"]),
                    PayFor = Text(reader["PayFor"]),
                    Amount = ToDec(reader["Amount"]),
                    LateFee = ToDec(reader["LateFee"]),
                    Concession = ToDec(reader["Total_Discount"]),
                    Paid = ToDec(reader["PaidAmount"]),
                    Due = ToDec(reader["Receivable_Amount"]),
                    Status = Text(reader["Status"])
                });
            }
        }

        dto.Rows = students;
        return dto;
    }

    private async Task<SessionStudentReportDto> GetSessionStudentCoreAsync(
        SessionSnapshot session, string kind, int yearId, int classId, string? sectionId, string? roleId, string? payFor,
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        yearId = Year(session, yearId);
        var section = Like(sectionId);
        var role = Like(roleId);
        var forLike = Like(payFor);
        var extra = kind switch
        {
            "paid" => "AND Income_PayOrder.PaidAmount <> 0",
            "due" => "AND Income_PayOrder.Receivable_Amount <> 0",
            "concession" => "AND (Income_PayOrder.LateFeeCountable + ISNULL(Income_PayOrder.Discount, 0)) <> 0",
            _ => ""
        };
        var payForSql = kind is "paid" or "due" ? "AND Income_PayOrder.PayFor LIKE @PayFor" : "";
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new SessionStudentReportDto();

        var totalSql = kind switch
        {
            "paid" => """
SELECT COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, 0 AS PayCount, 0 AS TotalFee, 0 AS TotalLateFee, 0 AS TotalDiscount,
       SUM(Income_PayOrder.PaidAmount) AS TotalPaid, 0 AS Unpaid,
       ROUND((SUM(Income_PayOrder.PaidAmount) * 100) / NULLIF(SUM(Income_PayOrder.PaidAmount) + SUM(Income_PayOrder.Receivable_Amount), 0), 2) AS Percentage
""",
            "due" => """
SELECT COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, COUNT(DISTINCT Income_PayOrder.PayOrderID) AS PayCount,
       0 AS TotalFee, 0 AS TotalLateFee, 0 AS TotalDiscount, 0 AS TotalPaid, SUM(Income_PayOrder.Receivable_Amount) AS Unpaid, 0 AS Percentage
""",
            "concession" => """
SELECT COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, 0 AS PayCount, SUM(Income_PayOrder.Amount) AS TotalFee, 0 AS TotalLateFee,
       SUM(Income_PayOrder.LateFeeCountable + ISNULL(Income_PayOrder.Discount, 0)) AS TotalDiscount, 0 AS TotalPaid, 0 AS Unpaid,
       ROUND(SUM(Income_PayOrder.Total_Discount) * 100 / NULLIF(SUM(Income_PayOrder.Amount), 0), 2) AS Percentage
""",
            _ => """
SELECT COUNT(DISTINCT Income_PayOrder.StudentID) AS Total_Stu, 0 AS PayCount, SUM(Income_PayOrder.Amount) AS TotalFee,
       SUM(Income_PayOrder.LateFeeCountable) AS TotalLateFee,
       SUM(Income_PayOrder.LateFeeCountable + ISNULL(Income_PayOrder.Discount, 0)) AS TotalDiscount,
       SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS TotalPaid, SUM(Income_PayOrder.Receivable_Amount) AS Unpaid, 0 AS Percentage
"""
        };

        await using (var cmd = new SqlCommand($"""
{totalSql}
FROM Income_PayOrder INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Is_Active = 1
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Income_PayOrder.RoleID LIKE @RoleID
  AND (Income_PayOrder.ClassID = @ClassID OR @ClassID = 0)
  AND ISNULL(CAST(StudentsClass.SectionID AS nvarchar(20)), N'0') LIKE @SectionID
  {payForSql} {extra}
""", con))
        {
            BindStudent(cmd, session, yearId, classId, section, role, forLike, from, to, kind is "paid" or "due");
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Students = ToInt(reader["Total_Stu"]);
                dto.PayorderCount = ToInt(reader["PayCount"]);
                dto.Payorder = ToDec(reader["TotalFee"]);
                dto.LateFee = ToDec(reader["TotalLateFee"]);
                dto.Concession = ToDec(reader["TotalDiscount"]);
                dto.Paid = ToDec(reader["TotalPaid"]);
                dto.Unpaid = ToDec(reader["Unpaid"]);
                dto.Percentage = ToDec(reader["Percentage"]);
            }
        }

        var rowSql = kind switch
        {
            "paid" => """
SELECT Student.ID, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo, 0 AS PayCount,
       0 AS TotalFee, 0 AS TotalLateFee, 0 AS TotalDiscount, SUM(Income_PayOrder.PaidAmount) AS TotalPaid, 0 AS Unpaid,
       ROUND((SUM(Income_PayOrder.PaidAmount) * 100) / NULLIF(SUM(Income_PayOrder.PaidAmount) + SUM(Income_PayOrder.Receivable_Amount), 0), 2) AS Percentage,
       Income_PayOrder.ClassID
""",
            "due" => """
SELECT Student.ID, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo,
       COUNT(DISTINCT Income_PayOrder.PayOrderID) AS PayCount, 0 AS TotalFee, 0 AS TotalLateFee, 0 AS TotalDiscount,
       0 AS TotalPaid, SUM(Income_PayOrder.Receivable_Amount) AS Unpaid, 0 AS Percentage, Income_PayOrder.ClassID
""",
            "concession" => """
SELECT Student.ID, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo, 0 AS PayCount,
       SUM(Income_PayOrder.Amount) AS TotalFee, 0 AS TotalLateFee, SUM(Income_PayOrder.Total_Discount) AS TotalDiscount,
       0 AS TotalPaid, 0 AS Unpaid,
       ROUND(SUM(Income_PayOrder.Total_Discount) * 100 / NULLIF(SUM(Income_PayOrder.Amount), 0), 2) AS Percentage,
       Income_PayOrder.ClassID
""",
            _ => """
SELECT Student.ID, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo, 0 AS PayCount,
       SUM(Income_PayOrder.Amount) AS TotalFee, SUM(Income_PayOrder.LateFeeCountable) AS TotalLateFee,
       SUM(Income_PayOrder.LateFeeCountable + ISNULL(Income_PayOrder.Discount, 0)) AS TotalDiscount,
       SUM(ISNULL(Income_PayOrder.PaidAmount, 0)) AS TotalPaid, SUM(Income_PayOrder.Receivable_Amount) AS Unpaid,
       0 AS Percentage, Income_PayOrder.ClassID
"""
        };

        await using (var cmd = new SqlCommand($"""
{rowSql}
FROM Income_PayOrder
INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID
INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID
INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE Income_PayOrder.SchoolID = @SchoolID AND Income_PayOrder.EducationYearID = @EducationYearID
  AND Income_PayOrder.Is_Active = 1
  AND Income_PayOrder.EndDate BETWEEN ISNULL(@From, '1-1-1000') AND ISNULL(@To, '1-1-3000')
  AND Income_PayOrder.RoleID LIKE @RoleID {payForSql}
  AND (Income_PayOrder.ClassID = @ClassID OR @ClassID = 0)
  AND ISNULL(CAST(StudentsClass.SectionID AS nvarchar(20)), N'0') LIKE @SectionID
  {extra}
GROUP BY CreateClass.Class, Student.StudentsName, Student.ID, Income_PayOrder.StudentID, Income_PayOrder.ClassID, StudentsClass.RollNo
ORDER BY Income_PayOrder.ClassID, StudentsClass.RollNo
""", con))
        {
            BindStudent(cmd, session, yearId, classId, section, role, forLike, from, to, kind is "paid" or "due");
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Rows.Add(new SessionStudentRowDto
                {
                    ID = Text(reader["ID"]),
                    Name = Text(reader["StudentsName"]),
                    ClassName = Text(reader["Class"]),
                    RollNo = Text(reader["RollNo"]),
                    PayorderCount = ToInt(reader["PayCount"]),
                    Fee = ToDec(reader["TotalFee"]),
                    LateFee = ToDec(reader["TotalLateFee"]),
                    Concession = ToDec(reader["TotalDiscount"]),
                    Paid = ToDec(reader["TotalPaid"]),
                    Unpaid = ToDec(reader["Unpaid"]),
                    Percentage = ToDec(reader["Percentage"])
                });
            }
        }

        return dto;
    }

    private async Task<List<SessionClassRowDto>> ReadClassRowsAsync(
        SqlConnection con, string sql, SessionSnapshot session, int yearId, DateTime? from, DateTime? to, int classId, int roleId, CancellationToken ct)
    {
        var items = new List<SessionClassRowDto>();
        await using var cmd = new SqlCommand(sql, con);
        AddSchool(cmd, session);
        BindYear(cmd, yearId);
        AddDates(cmd, from, to);
        if (sql.Contains("@ClassID", StringComparison.Ordinal))
            cmd.Parameters.AddWithValue("@ClassID", classId);
        if (sql.Contains("@RoleID", StringComparison.Ordinal))
            cmd.Parameters.AddWithValue("@RoleID", roleId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new SessionClassRowDto
            {
                Id = ToInt(reader["Id"]),
                Name = Text(reader["Name"]),
                Students = ToInt(reader["Students"]),
                Fee = ToDec(reader["Fee"]),
                LateFee = ToDec(reader["LateFee"]),
                Concession = ToDec(reader["Concession"]),
                Paid = ToDec(reader["Paid"]),
                Unpaid = ToDec(reader["Unpaid"])
            });
        }
        return items;
    }

    private void BindStudent(SqlCommand cmd, SessionSnapshot session, int yearId, int classId, string section, string role, string payFor, DateTime? from, DateTime? to, bool withPayFor)
    {
        AddSchool(cmd, session);
        BindYear(cmd, yearId);
        AddDates(cmd, from, to);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SectionID", section);
        cmd.Parameters.AddWithValue("@RoleID", role);
        if (withPayFor)
            cmd.Parameters.AddWithValue("@PayFor", payFor);
    }

    private static int Year(SessionSnapshot session, int yearId) => yearId > 0 ? yearId : session.EducationYearID;
    private static void BindYear(SqlCommand cmd, int yearId) => cmd.Parameters.AddWithValue("@EducationYearID", yearId);
    private static string KindWhere(string? kind) => kind switch
    {
        "paid" => "AND Income_PayOrder.PaidAmount <> 0",
        "due" => "AND Income_PayOrder.Receivable_Amount <> 0",
        "concession" => "AND Income_PayOrder.Total_Discount <> 0",
        _ => ""
    };
}
