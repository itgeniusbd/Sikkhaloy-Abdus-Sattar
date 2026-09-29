using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Institution;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class WebsitePublicService
{
    private const int StudentLimit = 48;
    private readonly EduConnectionFactory _connections;
    private readonly SmsOfficeService _sms;

    public WebsitePublicService(EduConnectionFactory connections, SmsOfficeService sms)
    {
        _connections = connections;
        _sms = sms;
    }

    public async Task<WebsiteHomeDto?> GetHomeAsync(int schoolId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0)
            return null;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var school = await ReadSchoolAsync(con, schoolId, cancellationToken);
        if (school is null)
            return null;

        var year = await ReadActiveYearAsync(con, schoolId, cancellationToken);
        return new WebsiteHomeDto
        {
            School = school,
            Stats = await ReadStatsAsync(con, schoolId, year.Id, year.Name, cancellationToken),
            Teachers = await ReadTeachersAsync(con, schoolId, cancellationToken),
            Staff = await ReadStaffAsync(con, schoolId, cancellationToken),
            Students = await ReadStudentsAsync(con, schoolId, year.Id, cancellationToken),
            Birthdays = await ReadBirthdaysAsync(con, schoolId, year.Id, cancellationToken),
            Notices = await ReadGeneralNoticesAsync(con, schoolId, cancellationToken)
        };
    }

    public async Task<byte[]?> GetStudentPhotoAsync(int schoolId, int studentId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0 || studentId <= 0)
            return null;

        const string sql = """
SELECT TOP 1 si.Image
FROM dbo.Student AS s
INNER JOIN dbo.StudentsClass AS sc ON sc.StudentID = s.StudentID
LEFT JOIN dbo.Student_Image AS si ON si.StudentImageID = s.StudentImageID
WHERE s.StudentID = @StudentID AND sc.SchoolID = @SchoolID
""";
        return await ReadImageAsync(sql, schoolId, "@StudentID", studentId, cancellationToken);
    }

    public async Task<byte[]?> GetTeacherPhotoAsync(int schoolId, int employeeId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0 || employeeId <= 0)
            return null;

        const string sql = """
SELECT TOP 1 t.Image
FROM dbo.Teacher AS t
INNER JOIN dbo.Employee_Info AS e ON e.EmployeeID = t.EmployeeID
WHERE t.EmployeeID = @EmployeeID AND e.SchoolID = @SchoolID
""";
        return await ReadImageAsync(sql, schoolId, "@EmployeeID", employeeId, cancellationToken);
    }

    public async Task<byte[]?> GetStaffPhotoAsync(int schoolId, int employeeId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0 || employeeId <= 0)
            return null;

        const string sql = """
SELECT TOP 1 s.Image
FROM dbo.Staff_Info AS s
INNER JOIN dbo.Employee_Info AS e ON e.EmployeeID = s.EmployeeID
WHERE s.EmployeeID = @EmployeeID AND e.SchoolID = @SchoolID
""";
        return await ReadImageAsync(sql, schoolId, "@EmployeeID", employeeId, cancellationToken);
    }

    private async Task<byte[]?> ReadImageAsync(
        string sql, int schoolId, string idName, int id, CancellationToken cancellationToken)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue(idName, id);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is byte[] bytes && bytes.Length > 0 ? bytes : null;
    }

    private static async Task<WebsiteSchoolDto?> ReadSchoolAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT SchoolID, SchoolName, Institution_Dialog, Established, Principal,
       Address, City, State, LocalArea, PostalCode, Phone, Email, Website,
       CASE WHEN SchoolLogo IS NULL OR DATALENGTH(SchoolLogo) = 0 THEN 0 ELSE 1 END AS HasLogo
FROM dbo.SchoolInfo
WHERE SchoolID = @SchoolID
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var address = JoinAddress(
            ReadString(reader, "Address"),
            ReadString(reader, "LocalArea"),
            ReadString(reader, "City"),
            ReadString(reader, "State"),
            ReadString(reader, "PostalCode"));
        var hasLogo = Convert.ToInt32(reader["HasLogo"]) == 1;
        return new WebsiteSchoolDto
        {
            SchoolID = Convert.ToInt32(reader["SchoolID"]),
            SchoolName = reader["SchoolName"]?.ToString()?.Trim() ?? "",
            Dialog = ReadString(reader, "Institution_Dialog"),
            Established = ReadString(reader, "Established"),
            Principal = ReadString(reader, "Principal"),
            Address = address,
            Phone = ReadString(reader, "Phone"),
            Email = ReadString(reader, "Email"),
            Website = ReadString(reader, "Website"),
            LogoUrl = hasLogo ? $"/api/public/institutes/{schoolId}/logo" : null
        };
    }

    private static async Task<YearInfo> ReadActiveYearAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            var year = await ReadYearAsync(con, """
SELECT TOP 1 EducationYearID, EducationYear
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID AND IsActive = 1
ORDER BY SN DESC
""", schoolId, cancellationToken);
            if (year.Id > 0)
                return year;
        }
        catch (SqlException)
        {
        }

        return await ReadYearAsync(con, """
SELECT TOP 1 EducationYearID, EducationYear
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID
ORDER BY SN DESC
""", schoolId, cancellationToken);
    }

    private static async Task<YearInfo> ReadYearAsync(
        SqlConnection con, string sql, int schoolId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new YearInfo(0, null);
        return new YearInfo(Convert.ToInt32(reader["EducationYearID"]), ReadString(reader, "EducationYear"));
    }

    private static async Task<WebsiteStatsDto> ReadStatsAsync(
        SqlConnection con, int schoolId, int yearId, string? yearName, CancellationToken cancellationToken)
    {
        var stats = new WebsiteStatsDto { EducationYear = yearName };
        stats.Students = await CountAsync(con, yearId > 0
            ? """
SELECT COUNT(*)
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON s.StudentID = sc.StudentID
WHERE sc.SchoolID = @SchoolID AND sc.EducationYearID = @YearID AND s.Status = N'Active'
"""
            : """
SELECT COUNT(*)
FROM dbo.Student
WHERE SchoolID = @SchoolID AND Status = N'Active'
""", schoolId, yearId, cancellationToken);

        stats.Teachers = await CountEmployeesAsync(con, schoolId, "Teacher", cancellationToken);
        stats.Staff = await CountEmployeesAsync(con, schoolId, "Staff", cancellationToken);

        if (yearId > 0)
        {
            await using var cmd = new SqlCommand("""
SELECT ISNULL(sc.ClassID, 0) AS ClassID, ISNULL(cc.Class, N'') AS ClassName, COUNT(*) AS Total
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON s.StudentID = sc.StudentID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
WHERE sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @YearID
  AND s.Status = N'Active'
GROUP BY sc.ClassID, cc.Class
ORDER BY sc.ClassID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@YearID", yearId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var name = ReadString(reader, "ClassName") ?? "";
                if (name.Length == 0)
                    name = "Class " + Convert.ToInt32(reader["ClassID"]);
                stats.Classes.Add(new WebsiteClassCountDto
                {
                    ClassID = Convert.ToInt32(reader["ClassID"]),
                    Name = name,
                    Count = Convert.ToInt32(reader["Total"])
                });
            }
        }

        return stats;
    }

    private static async Task<int> CountEmployeesAsync(
        SqlConnection con, int schoolId, string type, CancellationToken cancellationToken)
    {
        try
        {
            return await CountAsync(con, """
SELECT COUNT(*)
FROM dbo.Employee_Info
WHERE SchoolID = @SchoolID AND Job_Status = N'Active' AND EmployeeType = @Type
""", schoolId, 0, cancellationToken, type);
        }
        catch (SqlException)
        {
            var table = type == "Staff" ? "dbo.Staff_Info" : "dbo.Teacher";
            return await CountAsync(con, $"SELECT COUNT(*) FROM {table} WHERE SchoolID = @SchoolID", schoolId, 0, cancellationToken);
        }
    }

    private static async Task<int> CountAsync(
        SqlConnection con, string sql, int schoolId, int yearId, CancellationToken cancellationToken, string? type = null)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        if (sql.Contains("@YearID", StringComparison.Ordinal))
            cmd.Parameters.AddWithValue("@YearID", yearId);
        if (type is not null)
            cmd.Parameters.AddWithValue("@Type", type);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task<List<WebsitePersonDto>> ReadTeachersAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            return await ReadPeopleAsync(con, """
SELECT v.EmployeeID AS PersonID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(v.Designation, N'') AS Title,
       N'' AS Meta,
       CASE WHEN t.Image IS NULL OR DATALENGTH(t.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.VW_Emp_Info AS v
LEFT JOIN dbo.Teacher AS t ON t.EmployeeID = v.EmployeeID AND v.EmployeeType = N'Teacher'
WHERE v.SchoolID = @SchoolID
  AND v.Job_Status = N'Active'
  AND v.EmployeeType = N'Teacher'
ORDER BY v.ID
""", schoolId, 0, "teacher", cancellationToken);
        }
        catch (SqlException)
        {
            return await ReadPeopleAsync(con, """
SELECT e.EmployeeID AS PersonID,
       LTRIM(RTRIM(ISNULL(t.FirstName, N'') + N' ' + ISNULL(t.LastName, N''))) AS Name,
       ISNULL(t.Designation, N'') AS Title,
       N'' AS Meta,
       CASE WHEN t.Image IS NULL OR DATALENGTH(t.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.Employee_Info AS e
INNER JOIN dbo.Teacher AS t ON t.EmployeeID = e.EmployeeID
WHERE e.SchoolID = @SchoolID
  AND e.Job_Status = N'Active'
  AND e.EmployeeType = N'Teacher'
ORDER BY e.ID
""", schoolId, 0, "teacher", cancellationToken);
        }
    }

    private static async Task<List<WebsitePersonDto>> ReadStaffAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            return await ReadPeopleAsync(con, """
SELECT v.EmployeeID AS PersonID,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(v.Designation, N'') AS Title,
       N'' AS Meta,
       CASE WHEN st.Image IS NULL OR DATALENGTH(st.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.VW_Emp_Info AS v
LEFT JOIN dbo.Staff_Info AS st ON st.EmployeeID = v.EmployeeID AND v.EmployeeType = N'Staff'
WHERE v.SchoolID = @SchoolID
  AND v.Job_Status = N'Active'
  AND v.EmployeeType = N'Staff'
ORDER BY v.ID
""", schoolId, 0, "staff", cancellationToken);
        }
        catch (SqlException)
        {
            return await ReadPeopleAsync(con, """
SELECT e.EmployeeID AS PersonID,
       LTRIM(RTRIM(ISNULL(s.FirstName, N'') + N' ' + ISNULL(s.LastName, N''))) AS Name,
       ISNULL(s.Designation, N'') AS Title,
       N'' AS Meta,
       CASE WHEN s.Image IS NULL OR DATALENGTH(s.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.Employee_Info AS e
INNER JOIN dbo.Staff_Info AS s ON s.EmployeeID = e.EmployeeID
WHERE e.SchoolID = @SchoolID
  AND e.Job_Status = N'Active'
  AND e.EmployeeType = N'Staff'
ORDER BY e.ID
""", schoolId, 0, "staff", cancellationToken);
        }
    }

    private static async Task<List<WebsitePersonDto>> ReadStudentsAsync(
        SqlConnection con, int schoolId, int yearId, CancellationToken cancellationToken)
    {
        if (yearId <= 0)
            return [];

        return await ReadPeopleAsync(con, $"""
SELECT TOP ({StudentLimit}) s.StudentID AS PersonID,
       ISNULL(s.StudentsName, N'') AS Name,
       ISNULL(cc.Class, N'') AS Title,
       ISNULL(sc.RollNo, N'') AS Meta,
       CASE WHEN si.Image IS NULL OR DATALENGTH(si.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON s.StudentID = sc.StudentID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.Student_Image AS si ON si.StudentImageID = s.StudentImageID
WHERE sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @YearID
  AND s.Status = N'Active'
ORDER BY CASE WHEN si.Image IS NULL OR DATALENGTH(si.Image) = 0 THEN 1 ELSE 0 END,
         sc.ClassID,
         CASE WHEN ISNUMERIC(REPLACE(REPLACE(ISNULL(sc.RollNo, N''), '$', ''), ',', '')) = 1
              THEN CAST(REPLACE(REPLACE(sc.RollNo, '$', ''), ',', '') AS FLOAT) ELSE 0 END
""", schoolId, yearId, "student", cancellationToken);
    }

    private static async Task<List<WebsitePersonDto>> ReadBirthdaysAsync(
        SqlConnection con, int schoolId, int yearId, CancellationToken cancellationToken)
    {
        var list = new List<WebsitePersonDto>();
        if (yearId > 0)
        {
            try
            {
                list.AddRange(await ReadPeopleAsync(con, """
SELECT s.StudentID AS PersonID,
       ISNULL(s.StudentsName, N'') AS Name,
       ISNULL(cc.Class, N'') AS Title,
       ISNULL(sc.RollNo, N'') AS Meta,
       CASE WHEN si.Image IS NULL OR DATALENGTH(si.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON s.StudentID = sc.StudentID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.Student_Image AS si ON si.StudentImageID = s.StudentImageID
WHERE sc.SchoolID = @SchoolID
  AND sc.EducationYearID = @YearID
  AND s.Status = N'Active'
  AND s.DateofBirth IS NOT NULL
  AND MONTH(s.DateofBirth) = MONTH(GETDATE())
  AND DAY(s.DateofBirth) = DAY(GETDATE())
ORDER BY sc.ClassID, s.StudentsName
""", schoolId, yearId, "student", cancellationToken));
            }
            catch (SqlException)
            {
            }
        }

        try
        {
            list.AddRange(await ReadPeopleAsync(con, """
SELECT e.EmployeeID AS PersonID,
       LTRIM(RTRIM(ISNULL(t.FirstName, N'') + N' ' + ISNULL(t.LastName, N''))) AS Name,
       ISNULL(t.Designation, N'') AS Title,
       N'' AS Meta,
       CASE WHEN t.Image IS NULL OR DATALENGTH(t.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.Employee_Info AS e
INNER JOIN dbo.Teacher AS t ON t.EmployeeID = e.EmployeeID
WHERE e.SchoolID = @SchoolID
  AND e.Job_Status = N'Active'
  AND e.EmployeeType = N'Teacher'
  AND t.DateofBirth IS NOT NULL
  AND MONTH(t.DateofBirth) = MONTH(GETDATE())
  AND DAY(t.DateofBirth) = DAY(GETDATE())
ORDER BY t.FirstName
""", schoolId, 0, "teacher", cancellationToken));
        }
        catch (SqlException)
        {
        }

        try
        {
            list.AddRange(await ReadPeopleAsync(con, """
SELECT e.EmployeeID AS PersonID,
       LTRIM(RTRIM(ISNULL(s.FirstName, N'') + N' ' + ISNULL(s.LastName, N''))) AS Name,
       ISNULL(s.Designation, N'') AS Title,
       N'' AS Meta,
       CASE WHEN s.Image IS NULL OR DATALENGTH(s.Image) = 0 THEN 0 ELSE 1 END AS HasPhoto
FROM dbo.Employee_Info AS e
INNER JOIN dbo.Staff_Info AS s ON s.EmployeeID = e.EmployeeID
WHERE e.SchoolID = @SchoolID
  AND e.Job_Status = N'Active'
  AND e.EmployeeType = N'Staff'
  AND s.DateofBirth IS NOT NULL
  AND MONTH(s.DateofBirth) = MONTH(GETDATE())
  AND DAY(s.DateofBirth) = DAY(GETDATE())
ORDER BY s.FirstName
""", schoolId, 0, "staff", cancellationToken));
        }
        catch (SqlException)
        {
        }

        return list;
    }

    private static async Task<List<WebsiteNoticeDto>> ReadGeneralNoticesAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        var list = new List<WebsiteNoticeDto>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 20 n.StudentNoticeId,
       LTRIM(RTRIM(ISNULL(n.NoticeTitle, N''))) AS Title,
       ISNULL(n.Notice, N'') AS Body,
       n.InsertDate
FROM dbo.StudentNotice AS n
WHERE n.SchoolId = @SchoolID
  AND n.IsHomeWork = 0
  AND n.InsertDate >= DATEADD(DAY, -7, GETDATE())
  AND LTRIM(RTRIM(ISNULL(n.NoticeTitle, N''))) <> N''
ORDER BY n.InsertDate DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new WebsiteNoticeDto
                {
                    Id = Convert.ToInt32(reader["StudentNoticeId"]),
                    Title = reader["Title"]?.ToString()?.Trim() ?? "",
                    Body = reader["Body"]?.ToString()?.Trim(),
                    Date = reader["InsertDate"] is DateTime dt ? dt.ToString("yyyy-MM-dd") : null
                });
            }
        }
        catch (SqlException)
        {
        }

        return list;
    }

    private static async Task<List<WebsitePersonDto>> ReadPeopleAsync(
        SqlConnection con, string sql, int schoolId, int yearId, string kind, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        if (sql.Contains("@YearID", StringComparison.Ordinal))
            cmd.Parameters.AddWithValue("@YearID", yearId);

        var list = new List<WebsitePersonDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = Convert.ToInt32(reader["PersonID"]);
            var hasPhoto = Convert.ToInt32(reader["HasPhoto"]) == 1;
            var meta = ReadString(reader, "Meta");
            list.Add(new WebsitePersonDto
            {
                Id = id,
                Name = reader["Name"]?.ToString()?.Trim() ?? "",
                Title = ReadString(reader, "Title"),
                Meta = string.IsNullOrWhiteSpace(meta) ? null : meta,
                PhotoUrl = hasPhoto
                    ? $"/api/public/website/{schoolId}/{PersonFolder(kind)}/{id}/photo"
                    : null,
                Kind = kind
            });
        }

        return list;
    }

    private static string PersonFolder(string kind)
        => kind == "teacher" ? "teachers" : kind == "staff" ? "staff" : "students";

    private static string? JoinAddress(params string?[] parts)
    {
        var kept = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;
            foreach (var piece in part.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (piece.Length == 0)
                    continue;
                if (piece.All(char.IsDigit) && kept.Any(item => item.Contains(piece, StringComparison.Ordinal)))
                    continue;
                var key = piece.Split([' ', '.', '/', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(word => new string(word.Where(char.IsLetter).ToArray()))
                    .FirstOrDefault(word => word.Length > 3);
                if (key is { Length: > 0 } && !seen.Add(key))
                    continue;
                kept.Add(piece);
            }
        }
        return kept.Count == 0 ? null : string.Join(", ", kept);
    }

    private static string? ReadString(SqlDataReader reader, string column)
    {
        if (!HasColumn(reader, column))
            return null;
        var value = reader[column];
        var text = value is DBNull ? null : value?.ToString()?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool HasColumn(SqlDataReader reader, string column)
    {
        for (var i = 0; i < reader.FieldCount; i += 1)
        {
            if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private readonly record struct YearInfo(int Id, string? Name);
}
