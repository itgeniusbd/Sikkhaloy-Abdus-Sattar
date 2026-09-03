using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Inventory;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class InventoryService
{
    public async Task<IReadOnlyList<InventoryStudentHitDto>> SuggestSaleStudentsAsync(
        SessionSnapshot session, string? query, CancellationToken ct)
    {
        var code = (query ?? "").Trim();
        if (code.Length == 0) return [];
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT TOP 12 s.StudentID, s.ID, s.StudentsName, ISNULL(c.Class, N'') AS ClassName,
       ISNULL(sc.RollNo, N'') AS RollNo, ISNULL(s.SMSPhoneNo, N'') AS Phone
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
LEFT JOIN dbo.CreateClass AS c ON sc.ClassID = c.ClassID
WHERE s.Status = N'Active' AND sc.SchoolID = @SchoolID AND sc.EducationYearID = @YearID
  AND sc.Class_Status IS NULL
  AND (s.ID LIKE @ID + N'%' OR (LEN(@ID) >= 2 AND s.StudentsName LIKE N'%' + @ID + N'%'))
ORDER BY CASE WHEN s.ID LIKE @ID + N'%' THEN 0 ELSE 1 END, s.ID
""", con);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
        return await ReadStudentHitsAsync(cmd, ct);
    }

    public async Task<InventoryCustomerDto?> CustomerFromStudentAsync(SessionSnapshot session, string? id, CancellationToken ct)
    {
        var code = (id ?? "").Trim();
        if (code.Length == 0) return null;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        InventoryStudentHitDto? hit = null;
        await using (var cmd = new SqlCommand("""
SELECT TOP 1 s.StudentID, s.ID, s.StudentsName, ISNULL(c.Class, N'') AS ClassName,
       ISNULL(sc.RollNo, N'') AS RollNo, ISNULL(s.SMSPhoneNo, N'') AS Phone
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON sc.StudentID = s.StudentID
LEFT JOIN dbo.CreateClass AS c ON sc.ClassID = c.ClassID
WHERE sc.SchoolID = @SchoolID AND sc.Class_Status IS NULL
  AND (s.ID = @ID OR s.ID = @Unpadded)
ORDER BY CASE WHEN sc.EducationYearID = @YearID THEN 0 ELSE 1 END
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@YearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@ID", code);
            var unpadded = code.TrimStart('0');
            cmd.Parameters.AddWithValue("@Unpadded", unpadded.Length == 0 ? "0" : unpadded);
            var rows = await ReadStudentHitsAsync(cmd, ct);
            hit = rows.FirstOrDefault();
        }
        if (hit is null) return null;
        return await UpsertStudentCustomerAsync(con, session.SchoolID, hit, ct);
    }

    public async Task<IReadOnlyList<InventoryCustomerDto>> SearchCustomersAsync(
        SessionSnapshot session, string? name, string? phone, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var n = (name ?? "").Trim();
        var p = (phone ?? "").Trim();
        if (n.Length == 0 && p.Length == 0) return [];
        await using var cmd = new SqlCommand("""
SELECT TOP 20 c.CustomerID, c.Name, c.Phone, ISNULL(c.StudentID, 0) AS StudentID,
       ISNULL(c.StudentCode, N'') AS StudentCode, ISNULL(c.ClassName, N'') AS ClassName,
       ISNULL(d.Due, 0) AS Due
FROM dbo.Inv_Customer AS c
LEFT JOIN (
    SELECT CustomerID, SUM(Total - ISNULL(PaidAmount, 0)) AS Due
    FROM dbo.Inv_Sale WHERE SchoolID = @SchoolID GROUP BY CustomerID
) AS d ON d.CustomerID = c.CustomerID
WHERE c.SchoolID = @SchoolID
  AND (@Name = N'' OR c.Name LIKE N'%' + @Name + N'%')
  AND (@Phone = N'' OR ISNULL(c.Phone, N'') LIKE N'%' + @Phone + N'%')
ORDER BY c.Name
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Name", n);
        cmd.Parameters.AddWithValue("@Phone", p);
        return await ReadCustomersAsync(cmd, ct);
    }

    public async Task<InventoryResult> SaveWalkInCustomerAsync(
        SessionSnapshot session, SaveInventoryCustomerRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        var phone = (request?.Phone ?? "").Trim();
        if (name.Length == 0) return Fail("inv.needCustomerName");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureSchemaAsync(con, ct);
        var id = await ScalarAsync(con, null, """
INSERT INTO dbo.Inv_Customer (SchoolID, Name, Phone)
VALUES (@SchoolID, @Name, @Phone);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
        {
            p.AddWithValue("@SchoolID", session.SchoolID);
            p.AddWithValue("@Name", name);
            p.AddWithValue("@Phone", (object?)NullIfEmpty(phone) ?? DBNull.Value);
        }, ct);
        return Ok(id, "inv.customerSaved");
    }

    private static async Task<string?> GetCustomerNameAsync(SqlConnection con, int schoolId, int customerId, CancellationToken ct)
    {
        if (customerId <= 0) return null;
        await using var cmd = new SqlCommand("""
SELECT Name FROM dbo.Inv_Customer WHERE CustomerID = @ID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", customerId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        return NullIfEmpty(S(await cmd.ExecuteScalarAsync(ct)));
    }

    private static async Task<InventoryCustomerDto> UpsertStudentCustomerAsync(
        SqlConnection con, int schoolId, InventoryStudentHitDto hit, CancellationToken ct)
    {
        var existing = await ScalarAsync(con, null, """
SELECT TOP 1 CustomerID FROM dbo.Inv_Customer WHERE SchoolID = @SchoolID AND StudentID = @StudentID
""", p => { p.AddWithValue("@SchoolID", schoolId); p.AddWithValue("@StudentID", hit.StudentID); }, ct);
        if (existing > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.Inv_Customer SET Name = @Name, Phone = @Phone, StudentCode = @Code, ClassName = @Class
WHERE CustomerID = @ID
""", con);
            upd.Parameters.AddWithValue("@Name", hit.Name);
            upd.Parameters.AddWithValue("@Phone", (object?)NullIfEmpty(hit.Phone) ?? DBNull.Value);
            upd.Parameters.AddWithValue("@Code", hit.ID);
            upd.Parameters.AddWithValue("@Class", (object?)NullIfEmpty(hit.ClassName) ?? DBNull.Value);
            upd.Parameters.AddWithValue("@ID", existing);
            await upd.ExecuteNonQueryAsync(ct);
            return await CustomerWithDueAsync(con, schoolId, new InventoryCustomerDto
            {
                CustomerID = existing,
                Name = hit.Name,
                Phone = hit.Phone,
                StudentID = hit.StudentID,
                StudentCode = hit.ID,
                ClassName = hit.ClassName
            }, ct);
        }

        var id = await ScalarAsync(con, null, """
INSERT INTO dbo.Inv_Customer (SchoolID, Name, Phone, StudentID, StudentCode, ClassName)
VALUES (@SchoolID, @Name, @Phone, @StudentID, @Code, @Class);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", p =>
        {
            p.AddWithValue("@SchoolID", schoolId);
            p.AddWithValue("@Name", hit.Name);
            p.AddWithValue("@Phone", (object?)NullIfEmpty(hit.Phone) ?? DBNull.Value);
            p.AddWithValue("@StudentID", hit.StudentID);
            p.AddWithValue("@Code", hit.ID);
            p.AddWithValue("@Class", (object?)NullIfEmpty(hit.ClassName) ?? DBNull.Value);
        }, ct);
        return await CustomerWithDueAsync(con, schoolId, new InventoryCustomerDto
        {
            CustomerID = id,
            Name = hit.Name,
            Phone = hit.Phone,
            StudentID = hit.StudentID,
            StudentCode = hit.ID,
            ClassName = hit.ClassName
        }, ct);
    }

    private static async Task<InventoryCustomerDto> CustomerWithDueAsync(
        SqlConnection con, int schoolId, InventoryCustomerDto dto, CancellationToken ct)
    {
        dto.Due = await ScalarDecAsync(con, null, """
SELECT ISNULL(SUM(Total - ISNULL(PaidAmount, 0)), 0)
FROM dbo.Inv_Sale WHERE SchoolID = @SchoolID AND CustomerID = @ID
""", p => { p.AddWithValue("@SchoolID", schoolId); p.AddWithValue("@ID", dto.CustomerID); }, ct);
        return dto;
    }

    private static async Task<List<InventoryStudentHitDto>> ReadStudentHitsAsync(SqlCommand cmd, CancellationToken ct)
    {
        var rows = new List<InventoryStudentHitDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new InventoryStudentHitDto
            {
                StudentID = I(reader["StudentID"]),
                ID = S(reader["ID"]),
                Name = S(reader["StudentsName"]),
                ClassName = NullS(reader["ClassName"]),
                RollNo = NullS(reader["RollNo"]),
                Phone = NullS(reader["Phone"])
            });
        }
        return rows;
    }

    private static async Task<List<InventoryCustomerDto>> ReadCustomersAsync(SqlCommand cmd, CancellationToken ct)
    {
        var rows = new List<InventoryCustomerDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new InventoryCustomerDto
            {
                CustomerID = I(reader["CustomerID"]),
                Name = S(reader["Name"]),
                Phone = NullS(reader["Phone"]),
                StudentID = I(reader["StudentID"]),
                StudentCode = NullS(reader["StudentCode"]),
                ClassName = NullS(reader["ClassName"]),
                Due = Dec(reader["Due"])
            });
        }
        return rows;
    }

    private static async Task EnsureCustomerSchemaAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.Inv_Customer', N'U') IS NULL
CREATE TABLE dbo.Inv_Customer (
    CustomerID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    StudentID INT NULL,
    StudentCode NVARCHAR(50) NULL,
    ClassName NVARCHAR(80) NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Customer_Insert DEFAULT (GETDATE())
);
IF COL_LENGTH(N'dbo.Inv_Sale', N'CustomerID') IS NULL
    ALTER TABLE dbo.Inv_Sale ADD CustomerID INT NULL;
IF COL_LENGTH(N'dbo.Inv_Sale', N'PaidAmount') IS NULL
    ALTER TABLE dbo.Inv_Sale ADD PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Sale_Paid DEFAULT (0);
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inv_Sale') AND name = N'AccountID' AND is_nullable = 0)
    ALTER TABLE dbo.Inv_Sale ALTER COLUMN AccountID INT NULL;
IF COL_LENGTH(N'dbo.Inv_Sale', N'PaidAmount') IS NOT NULL
    EXEC(N'UPDATE dbo.Inv_Sale SET PaidAmount = Total WHERE ExtraIncomeID IS NOT NULL AND ISNULL(PaidAmount, 0) = 0 AND Total > 0');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Customer_School' AND object_id = OBJECT_ID(N'dbo.Inv_Customer'))
    CREATE INDEX IX_Inv_Customer_School ON dbo.Inv_Customer (SchoolID, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Sale_Customer' AND object_id = OBJECT_ID(N'dbo.Inv_Sale'))
    CREATE INDEX IX_Inv_Sale_Customer ON dbo.Inv_Sale (SchoolID, CustomerID, DocDate);
IF COL_LENGTH(N'dbo.Inv_Sale', N'FeePayOrderID') IS NULL
    ALTER TABLE dbo.Inv_Sale ADD FeePayOrderID INT NULL;
""", con);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
