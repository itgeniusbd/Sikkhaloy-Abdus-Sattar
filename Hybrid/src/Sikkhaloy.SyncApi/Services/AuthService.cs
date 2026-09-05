using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class AuthService
{
    private static readonly HashSet<string> OfficeRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Sub-Admin"
    };

    private static readonly HashSet<string> AuthorityRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authority",
        "Sub-Authority"
    };

    private readonly EduConnectionFactory _connections;

    public AuthService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await LoginCoreAsync(request, cancellationToken);
        }
        catch (Exception)
        {
            return Fail("login.failed");
        }
    }

    private async Task<LoginResponse> LoginCoreAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Fail("login.required");
        }

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        var membership = await ReadMembershipAsync(con, userName, cancellationToken);
        if (membership is null)
            return Fail("login.userNotFound");
        if (!membership.Value.IsApproved)
            return Fail("login.notApproved");
        if (membership.Value.IsLockedOut)
            return Fail("login.locked");
        if (!MembershipPasswordVerifier.Verify(request.Password, membership.Value.Password, membership.Value.Salt, membership.Value.Format))
            return Fail("login.badPassword");

        var role = await ReadRoleAsync(con, userName, cancellationToken);
        if (role is null || (!OfficeRoles.Contains(role) && !AuthorityRoles.Contains(role) && !IsStudentRole(role)))
            return Fail("login.role");

        var deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
            ? Guid.NewGuid().ToString("N")
            : request.DeviceId;

        if (AuthorityRoles.Contains(role))
        {
            var authority = await ReadAuthorityProfileAsync(con, userName, cancellationToken);
            if (authority is null)
                return Fail("login.noAuthority");

            return new LoginResponse
            {
                Succeeded = true,
                Session = new SessionSnapshot
                {
                    UserName = userName,
                    Role = role,
                    SchoolID = 0,
                    SchoolName = "Sikkhaloy.com",
                    RegistrationID = authority.Value.RegistrationID,
                    EducationYearID = 0,
                    DeviceId = deviceId,
                    DisplayName = string.IsNullOrWhiteSpace(authority.Value.DisplayName)
                        ? userName
                        : authority.Value.DisplayName
                }
            };
        }

        if (IsStudentRole(role))
        {
            var student = await ReadStudentProfileAsync(con, userName, cancellationToken);
            if (student is null)
                return Fail("login.noYear");

            return new LoginResponse
            {
                Succeeded = true,
                Session = new SessionSnapshot
                {
                    UserName = userName,
                    Role = role,
                    SchoolID = student.Value.SchoolID,
                    SchoolName = student.Value.SchoolName,
                    RegistrationID = student.Value.RegistrationID,
                    EducationYearID = student.Value.EducationYearID,
                    DeviceId = deviceId,
                    DisplayName = string.IsNullOrWhiteSpace(student.Value.DisplayName)
                        ? userName
                        : student.Value.DisplayName,
                    StudentID = student.Value.StudentID,
                    StudentClassID = student.Value.StudentClassID,
                    ClassID = student.Value.ClassID,
                    StudentCode = student.Value.StudentCode,
                    ClassName = student.Value.ClassName,
                    SectionName = student.Value.SectionName
                }
            };
        }

        var profile = await ReadProfileAsync(con, userName, cancellationToken);
        if (profile is null)
            return Fail("login.noYear");

        return new LoginResponse
        {
            Succeeded = true,
            Session = new SessionSnapshot
            {
                UserName = userName,
                Role = role,
                SchoolID = profile.Value.SchoolID,
                SchoolName = profile.Value.SchoolName,
                RegistrationID = profile.Value.RegistrationID,
                EducationYearID = profile.Value.EducationYearID,
                DeviceId = deviceId,
                DisplayName = string.IsNullOrWhiteSpace(profile.Value.DisplayName)
                    ? userName
                    : profile.Value.DisplayName
            }
        };
    }

    private static LoginResponse Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<(string Password, string Salt, int Format, bool IsApproved, bool IsLockedOut)?> ReadMembershipAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT m.Password, m.PasswordSalt, m.PasswordFormat, m.IsApproved, m.IsLockedOut
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Membership AS m ON u.UserId = m.UserId
INNER JOIN dbo.aspnet_Applications AS a ON u.ApplicationId = a.ApplicationId
WHERE u.LoweredUserName = LOWER(@UserName)
  AND a.LoweredApplicationName = N'/'";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        var password = Str(reader, 0);
        var salt = Str(reader, 1);
        if (password.Length == 0 || salt.Length == 0)
            return null;

        return (
            password,
            salt,
            Int(reader, 2),
            Flag(reader, 3, whenNull: true),
            Flag(reader, 4, whenNull: false));
    }

    private static async Task<string?> ReadRoleAsync(SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT r.RoleName
FROM dbo.aspnet_UsersInRoles AS ur
INNER JOIN dbo.aspnet_Roles AS r ON ur.RoleId = r.RoleId
INNER JOIN dbo.aspnet_Users AS u ON ur.UserId = u.UserId
WHERE u.LoweredUserName = LOWER(@UserName)";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        string? best = null;
        var bestRank = 99;
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = Str(reader, 0);
            var rank = RoleRank(name);
            if (rank < bestRank)
            {
                best = name;
                bestRank = rank;
            }
        }

        return best;
    }

    private static int RoleRank(string name)
    {
        if (string.Equals(name, "Authority", StringComparison.OrdinalIgnoreCase)) return 0;
        if (string.Equals(name, "Sub-Authority", StringComparison.OrdinalIgnoreCase)) return 1;
        if (string.Equals(name, "Admin", StringComparison.OrdinalIgnoreCase)) return 2;
        if (string.Equals(name, "Sub-Admin", StringComparison.OrdinalIgnoreCase)) return 3;
        if (string.Equals(name, "Student", StringComparison.OrdinalIgnoreCase)) return 8;
        return 99;
    }

    private static bool IsStudentRole(string role) =>
        string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase);

    private static async Task<(int SchoolID, string SchoolName, int RegistrationID, int EducationYearID, string DisplayName, int StudentID, int StudentClassID, int ClassID, string StudentCode, string ClassName, string SectionName)?> ReadStudentProfileAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT TOP 1
    Registration.SchoolID,
    SchoolInfo.SchoolName,
    Registration.RegistrationID,
    StudentsClass.EducationYearID,
    Student.StudentsName,
    Student.StudentID,
    StudentsClass.StudentClassID,
    StudentsClass.ClassID,
    Student.ID,
    ISNULL(CreateClass.Class, N'') AS ClassName,
    ISNULL(CreateSection.Section, N'') AS SectionName
FROM dbo.Registration
INNER JOIN dbo.SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID
INNER JOIN dbo.Student ON Student.StudentRegistrationID = Registration.RegistrationID
INNER JOIN dbo.StudentsClass ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.Education_Year_User ON Education_Year_User.RegistrationID = Registration.RegistrationID
    AND Education_Year_User.EducationYearID = StudentsClass.EducationYearID
LEFT JOIN dbo.CreateClass ON CreateClass.ClassID = StudentsClass.ClassID
LEFT JOIN dbo.CreateSection ON CreateSection.SectionID = StudentsClass.SectionID
WHERE Registration.UserName = @UserName
  AND Registration.Validation = N'Valid'
ORDER BY CASE WHEN Education_Year_User.EducationYearID IS NULL THEN 1 ELSE 0 END,
         StudentsClass.EducationYearID DESC
""";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var row = ReadStudentRow(reader);
        return row.EducationYearID <= 0 ? null : row;
    }

    private static async Task<(int SchoolID, string SchoolName, int RegistrationID, int EducationYearID, string DisplayName, int StudentID, int StudentClassID, int ClassID, string StudentCode, string ClassName, string SectionName)?> ReadStudentClassForYearAsync(
        SqlConnection con, SessionSnapshot session, int educationYearId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT TOP 1
    Registration.SchoolID,
    SchoolInfo.SchoolName,
    Registration.RegistrationID,
    StudentsClass.EducationYearID,
    Student.StudentsName,
    Student.StudentID,
    StudentsClass.StudentClassID,
    StudentsClass.ClassID,
    Student.ID,
    ISNULL(CreateClass.Class, N'') AS ClassName,
    ISNULL(CreateSection.Section, N'') AS SectionName
FROM dbo.Registration
INNER JOIN dbo.SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID
INNER JOIN dbo.Student ON Student.StudentRegistrationID = Registration.RegistrationID
INNER JOIN dbo.StudentsClass ON StudentsClass.StudentID = Student.StudentID
LEFT JOIN dbo.CreateClass ON CreateClass.ClassID = StudentsClass.ClassID
LEFT JOIN dbo.CreateSection ON CreateSection.SectionID = StudentsClass.SectionID
WHERE Registration.UserName = @UserName
  AND Registration.Validation = N'Valid'
  AND Registration.SchoolID = @SchoolID
  AND Student.StudentID = @StudentID
  AND StudentsClass.EducationYearID = @EducationYearID
ORDER BY StudentsClass.StudentClassID DESC
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", session.UserName);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@StudentID", session.StudentID);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var row = ReadStudentRow(reader);
        return row.EducationYearID <= 0 ? null : row;
    }

    private static async Task SaveYearUserAsync(
        SqlConnection con, int schoolId, int registrationId, int educationYearId, CancellationToken cancellationToken)
    {
        const string updateSql = """
UPDATE dbo.Education_Year_User
SET EducationYearID = @EducationYearID
WHERE SchoolID = @SchoolID AND RegistrationID = @RegistrationID
""";
        await using (var update = new SqlCommand(updateSql, con))
        {
            update.Parameters.AddWithValue("@SchoolID", schoolId);
            update.Parameters.AddWithValue("@RegistrationID", registrationId);
            update.Parameters.AddWithValue("@EducationYearID", educationYearId);
            if (await update.ExecuteNonQueryAsync(cancellationToken) > 0)
                return;
        }

        const string insertSql = """
INSERT INTO dbo.Education_Year_User (RegistrationID, EducationYearID, SchoolID)
VALUES (@RegistrationID, @EducationYearID, @SchoolID)
""";
        await using var insert = new SqlCommand(insertSql, con);
        insert.Parameters.AddWithValue("@RegistrationID", registrationId);
        insert.Parameters.AddWithValue("@EducationYearID", educationYearId);
        insert.Parameters.AddWithValue("@SchoolID", schoolId);
        await insert.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<(int SchoolID, string SchoolName, int RegistrationID, int EducationYearID, string DisplayName)?> ReadProfileAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1
    Registration.SchoolID,
    SchoolInfo.SchoolName,
    Registration.RegistrationID,
    COALESCE(Education_Year_User.EducationYearID, CurrentYear.EducationYearID) AS EducationYearID,
    LTRIM(RTRIM(ISNULL(Admin.FirstName, N'') + N' ' + ISNULL(Admin.LastName, N''))) AS DisplayName
FROM dbo.Registration
INNER JOIN dbo.SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID
LEFT JOIN dbo.Education_Year_User ON Registration.RegistrationID = Education_Year_User.RegistrationID
    AND Education_Year_User.SchoolID = Registration.SchoolID
LEFT JOIN dbo.Admin ON Admin.RegistrationID = Registration.RegistrationID AND Admin.SchoolID = Registration.SchoolID
OUTER APPLY (
    SELECT TOP 1 EducationYearID
    FROM dbo.Education_Year
    WHERE SchoolID = Registration.SchoolID
    ORDER BY CASE WHEN Status IN (N'True', N'Active', N'Current') THEN 0 ELSE 1 END,
             ISNULL(SN, EducationYearID) DESC, EducationYearID DESC
) AS CurrentYear
WHERE Registration.UserName = @UserName
  AND Registration.Validation = N'Valid'
ORDER BY CASE WHEN Education_Year_User.EducationYearID IS NULL THEN 1 ELSE 0 END";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var yearId = Int(reader, "EducationYearID");
        if (yearId <= 0)
            return null;

        return (
            Int(reader, "SchoolID"),
            Str(reader, "SchoolName"),
            Int(reader, "RegistrationID"),
            yearId,
            Str(reader, "DisplayName"));
    }

    private static async Task<(int RegistrationID, string DisplayName)?> ReadAuthorityProfileAsync(
        SqlConnection con, string userName, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP 1 Registration.RegistrationID,
       LTRIM(RTRIM(ISNULL(Authority_Info.Name, N''))) AS DisplayName
FROM dbo.Authority_Info
INNER JOIN dbo.Registration ON Authority_Info.RegistrationID = Registration.RegistrationID
WHERE Registration.UserName = @UserName
  AND Registration.Validation = N'Valid'";

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return (
            Int(reader, "RegistrationID"),
            Str(reader, "DisplayName"));
    }

    public async Task<LoginResponse> EnterSchoolAsync(SessionSnapshot authority, int schoolId, int educationYearId, CancellationToken cancellationToken)
    {
        if (!authority.IsAuthority)
            return Fail("auth.forbidden");
        if (schoolId <= 0)
            return Fail("auth.noSchool");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        string? userName = null;
        await using (var find = new SqlCommand("""
SELECT TOP 1 r.UserName
FROM dbo.SchoolInfo s
INNER JOIN dbo.Registration r ON r.SchoolID = s.SchoolID AND r.UserName = s.UserName
WHERE s.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
  AND r.Category IN (N'Admin', N'Sub-Admin')
ORDER BY CASE WHEN r.Category = N'Admin' THEN 0 ELSE 1 END, r.RegistrationID
""", con))
        {
            find.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await find.ExecuteScalarAsync(cancellationToken);
            userName = value is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            await using var fallback = new SqlCommand("""
SELECT TOP 1 r.UserName
FROM dbo.Registration r
WHERE r.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
  AND r.Category = N'Admin'
ORDER BY r.RegistrationID
""", con);
            fallback.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await fallback.ExecuteScalarAsync(cancellationToken);
            userName = value is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
        }

        if (string.IsNullOrWhiteSpace(userName))
            return Fail("auth.noAdmin");

        var role = await ReadRoleAsync(con, userName, cancellationToken);
        if (role is null || (!OfficeRoles.Contains(role) && !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(role, "Sub-Admin", StringComparison.OrdinalIgnoreCase)))
        {
            role = "Admin";
        }

        var profile = await ReadProfileAsync(con, userName, cancellationToken);
        if (profile is null || profile.Value.SchoolID != schoolId)
            return Fail("login.noYear");

        var yearId = profile.Value.EducationYearID;
        if (educationYearId > 0)
        {
            await using var year = new SqlCommand("""
SELECT EducationYearID FROM dbo.Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
""", con);
            year.Parameters.AddWithValue("@SchoolID", schoolId);
            year.Parameters.AddWithValue("@EducationYearID", educationYearId);
            var found = await year.ExecuteScalarAsync(cancellationToken);
            if (found is null or DBNull)
                return Fail("login.noYear");
            yearId = educationYearId;
        }

        var deviceId = string.IsNullOrWhiteSpace(authority.DeviceId)
            ? Guid.NewGuid().ToString("N")
            : authority.DeviceId;

        return new LoginResponse
        {
            Succeeded = true,
            Session = new SessionSnapshot
            {
                UserName = userName,
                Role = OfficeRoles.Contains(role) ? role : "Admin",
                SchoolID = profile.Value.SchoolID,
                SchoolName = profile.Value.SchoolName,
                RegistrationID = profile.Value.RegistrationID,
                EducationYearID = yearId,
                DeviceId = deviceId,
                DisplayName = string.IsNullOrWhiteSpace(profile.Value.DisplayName)
                    ? userName
                    : profile.Value.DisplayName
            }
        };
    }

    public async Task<LoginResponse> SwitchYearAsync(SessionSnapshot session, int educationYearId, CancellationToken cancellationToken)
    {
        if (educationYearId <= 0)
            return Fail("login.noYear");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);

        const string existsSql = @"
SELECT EducationYear
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID";
        await using (var exists = new SqlCommand(existsSql, con))
        {
            exists.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            exists.Parameters.AddWithValue("@EducationYearID", educationYearId);
            var name = await exists.ExecuteScalarAsync(cancellationToken);
            if (name is null or DBNull)
                return Fail("login.noYear");
        }

        if (session.IsStudent)
        {
            var student = await ReadStudentClassForYearAsync(con, session, educationYearId, cancellationToken);
            if (student is null)
                return Fail("login.noYear");

            await SaveYearUserAsync(con, session.SchoolID, session.RegistrationID, educationYearId, cancellationToken);

            return new LoginResponse
            {
                Succeeded = true,
                Session = new SessionSnapshot
                {
                    UserName = session.UserName,
                    Role = session.Role,
                    SchoolID = student.Value.SchoolID,
                    SchoolName = student.Value.SchoolName,
                    RegistrationID = student.Value.RegistrationID,
                    EducationYearID = educationYearId,
                    DeviceId = session.DeviceId,
                    DisplayName = string.IsNullOrWhiteSpace(student.Value.DisplayName)
                        ? session.UserName
                        : student.Value.DisplayName,
                    StudentID = student.Value.StudentID,
                    StudentClassID = student.Value.StudentClassID,
                    ClassID = student.Value.ClassID,
                    StudentCode = student.Value.StudentCode,
                    ClassName = student.Value.ClassName,
                    SectionName = student.Value.SectionName
                }
            };
        }

        const string updateSql = @"
UPDATE dbo.Education_Year_User
SET EducationYearID = @EducationYearID
WHERE SchoolID = @SchoolID AND RegistrationID = @RegistrationID";
        await using (var update = new SqlCommand(updateSql, con))
        {
            update.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            update.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            update.Parameters.AddWithValue("@EducationYearID", educationYearId);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        var profile = await ReadProfileAsync(con, session.UserName, cancellationToken);
        if (profile is null)
            return Fail("login.noYear");

        return new LoginResponse
        {
            Succeeded = true,
            Session = new SessionSnapshot
            {
                UserName = session.UserName,
                Role = session.Role,
                SchoolID = profile.Value.SchoolID,
                SchoolName = profile.Value.SchoolName,
                RegistrationID = profile.Value.RegistrationID,
                EducationYearID = educationYearId,
                DeviceId = session.DeviceId,
                DisplayName = string.IsNullOrWhiteSpace(profile.Value.DisplayName)
                    ? session.UserName
                    : profile.Value.DisplayName
            }
        };
    }

    private static (int SchoolID, string SchoolName, int RegistrationID, int EducationYearID, string DisplayName, int StudentID, int StudentClassID, int ClassID, string StudentCode, string ClassName, string SectionName) ReadStudentRow(SqlDataReader reader) =>
    (
        Int(reader, "SchoolID"),
        Str(reader, "SchoolName"),
        Int(reader, "RegistrationID"),
        Int(reader, "EducationYearID"),
        Str(reader, "StudentsName"),
        Int(reader, "StudentID"),
        Int(reader, "StudentClassID"),
        Int(reader, "ClassID"),
        Str(reader, "ID"),
        Str(reader, "ClassName"),
        Str(reader, "SectionName"));

    private static string Str(SqlDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return "";
        return Convert.ToString(reader.GetValue(ordinal))?.Trim() ?? "";
    }

    private static string Str(SqlDataReader reader, string name) =>
        Str(reader, reader.GetOrdinal(name));

    private static int Int(SqlDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return 0;
        return Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static int Int(SqlDataReader reader, string name) =>
        Int(reader, reader.GetOrdinal(name));

    private static bool Flag(SqlDataReader reader, int ordinal, bool whenNull)
    {
        if (reader.IsDBNull(ordinal))
            return whenNull;
        var value = reader.GetValue(ordinal);
        return value switch
        {
            bool flag => flag,
            byte n => n != 0,
            short n => n != 0,
            int n => n != 0,
            long n => n != 0,
            _ => Convert.ToBoolean(value)
        };
    }
}
