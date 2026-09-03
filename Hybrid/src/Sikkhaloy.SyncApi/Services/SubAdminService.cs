using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Access;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SubAdminService
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly EduConnectionFactory _connections;

    public SubAdminService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<CreateSubAdminResult> CreateAsync(
        SessionSnapshot session, CreateSubAdminRequest? request, CancellationToken cancellationToken)
    {
        request ??= new CreateSubAdminRequest();
        var firstName = (request.FirstName ?? "").Trim();
        var lastName = (request.LastName ?? "").Trim();
        var designation = (request.Designation ?? "").Trim();
        var userName = (request.UserName ?? "").Trim();
        var password = request.Password ?? "";
        var confirm = request.ConfirmPassword ?? "";
        var email = (request.Email ?? "").Trim();
        var question = "Username";
        var answer = userName;

        if (firstName.Length == 0 || lastName.Length == 0 || designation.Length == 0
            || userName.Length == 0 || password.Length == 0 || email.Length == 0)
            return Fail("sub.required");
        if (userName.Any(char.IsWhiteSpace))
            return Fail("sub.userSpace");
        if (userName.Length is < 8 or > 30)
            return Fail("sub.userLen");
        if (password.Length is < 8 or > 30)
            return Fail("sub.passLen");
        if (!string.Equals(password, confirm, StringComparison.Ordinal))
            return Fail("sub.passMatch");
        if (!EmailPattern.IsMatch(email))
            return Fail("sub.emailInvalid");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            if (await UserExistsAsync(con, tx, userName, cancellationToken))
            {
                await tx.RollbackAsync(cancellationToken);
                return Fail("sub.userExists");
            }

            var created = await CreateMembershipUserAsync(
                con, tx, userName, password, email, question, answer, cancellationToken);
            if (!created.Succeeded)
            {
                await tx.RollbackAsync(cancellationToken);
                return created;
            }

            await AddToRoleAsync(con, tx, userName, "Sub-Admin", cancellationToken);

            var registrationId = await InsertRegistrationAsync(con, tx, session.SchoolID, userName, cancellationToken);
            await InsertAstAsync(con, tx, registrationId, session.SchoolID, userName, password, answer, cancellationToken);
            await InsertEducationYearAsync(con, tx, registrationId, session.SchoolID, session.EducationYearID, cancellationToken);
            await InsertAdminAsync(con, tx, registrationId, session.SchoolID, firstName, lastName, designation, cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new CreateSubAdminResult
            {
                Succeeded = true,
                UserName = userName,
                RegistrationID = registrationId
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    private static CreateSubAdminResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<bool> UserExistsAsync(
        SqlConnection con, SqlTransaction tx, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT 1
WHERE EXISTS (
    SELECT 1 FROM dbo.aspnet_Users AS u
    INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
    WHERE u.LoweredUserName = LOWER(@UserName))
   OR EXISTS (SELECT 1 FROM dbo.Registration WHERE UserName = @UserName)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static async Task<CreateSubAdminResult> CreateMembershipUserAsync(
        SqlConnection con,
        SqlTransaction tx,
        string userName,
        string password,
        string email,
        string question,
        string answer,
        CancellationToken cancellationToken)
    {
        var salt = MembershipPasswordVerifier.NewSalt();
        var hashedPassword = MembershipPasswordVerifier.Hash(password, salt);
        var hashedAnswer = MembershipPasswordVerifier.Hash(answer, salt);
        var now = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        await using var cmd = new SqlCommand("dbo.aspnet_Membership_CreateUser", con, tx)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ApplicationName", "/");
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@Password", hashedPassword);
        cmd.Parameters.AddWithValue("@PasswordSalt", salt);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@PasswordQuestion", question);
        cmd.Parameters.AddWithValue("@PasswordAnswer", hashedAnswer);
        cmd.Parameters.AddWithValue("@IsApproved", true);
        cmd.Parameters.AddWithValue("@CurrentTimeUtc", now);
        cmd.Parameters.AddWithValue("@CreateDate", now);
        cmd.Parameters.AddWithValue("@UniqueEmail", 0);
        cmd.Parameters.AddWithValue("@PasswordFormat", 1);
        var userIdParam = cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier);
        userIdParam.Direction = ParameterDirection.InputOutput;
        userIdParam.Value = userId;
        var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
        returnParam.Direction = ParameterDirection.ReturnValue;

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 2812)
        {
            await InsertMembershipManuallyAsync(
                con, tx, userId, userName, hashedPassword, salt, email, question, hashedAnswer, now, cancellationToken);
            return new CreateSubAdminResult { Succeeded = true, UserName = userName };
        }

        var code = returnParam.Value is int i ? i : Convert.ToInt32(returnParam.Value ?? 0);
        return code switch
        {
            0 => new CreateSubAdminResult { Succeeded = true, UserName = userName },
            6 => Fail("sub.userExists"),
            7 => Fail("sub.emailExists"),
            _ => Fail("sub.failed")
        };
    }

    private static async Task InsertMembershipManuallyAsync(
        SqlConnection con,
        SqlTransaction tx,
        Guid userId,
        string userName,
        string hashedPassword,
        string salt,
        string email,
        string question,
        string hashedAnswer,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        await using var appCmd = new SqlCommand(
            "SELECT ApplicationId FROM dbo.aspnet_Applications WHERE LoweredApplicationName = N'/'", con, tx);
        var appIdObj = await appCmd.ExecuteScalarAsync(cancellationToken);
        if (appIdObj is null or DBNull)
            throw new InvalidOperationException("Membership application '/' was not found.");
        var appId = (Guid)appIdObj;
        var lockout = new DateTime(1754, 1, 1);

        await using var userCmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
VALUES (@ApplicationId, @UserId, @UserName, LOWER(@UserName), 0, @Now)
""", con, tx);
        userCmd.Parameters.AddWithValue("@ApplicationId", appId);
        userCmd.Parameters.AddWithValue("@UserId", userId);
        userCmd.Parameters.AddWithValue("@UserName", userName);
        userCmd.Parameters.AddWithValue("@Now", utcNow);
        await userCmd.ExecuteNonQueryAsync(cancellationToken);

        await using var memCmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Membership
    (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, Email, LoweredEmail,
     PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate,
     LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
     FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)
VALUES
    (@ApplicationId, @UserId, @Password, 1, @PasswordSalt, @Email, LOWER(@Email),
     @PasswordQuestion, @PasswordAnswer, 1, 0, @Now, @Now,
     @Now, @Lockout, 0, @Lockout,
     0, @Lockout)
""", con, tx);
        memCmd.Parameters.AddWithValue("@ApplicationId", appId);
        memCmd.Parameters.AddWithValue("@UserId", userId);
        memCmd.Parameters.AddWithValue("@Password", hashedPassword);
        memCmd.Parameters.AddWithValue("@PasswordSalt", salt);
        memCmd.Parameters.AddWithValue("@Email", email);
        memCmd.Parameters.AddWithValue("@PasswordQuestion", question);
        memCmd.Parameters.AddWithValue("@PasswordAnswer", hashedAnswer);
        memCmd.Parameters.AddWithValue("@Now", utcNow);
        memCmd.Parameters.AddWithValue("@Lockout", lockout);
        await memCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task AddToRoleAsync(
        SqlConnection con, SqlTransaction tx, string userName, string roleName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.aspnet_Roles AS r ON r.ApplicationId = a.ApplicationId AND r.LoweredRoleName = LOWER(@RoleName)
WHERE u.LoweredUserName = LOWER(@UserName)
  AND NOT EXISTS (
      SELECT 1 FROM dbo.aspnet_UsersInRoles AS ur
      WHERE ur.UserId = u.UserId AND ur.RoleId = r.RoleId)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@RoleName", roleName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> InsertRegistrationAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Registration (SchoolID, UserName, Validation, Category, CreateDate)
VALUES (@SchoolID, @UserName, N'Valid', N'Sub-Admin', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        var id = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(id);
    }

    private static async Task InsertAstAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, string userName, string password, string answer, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
INSERT INTO dbo.AST (RegistrationID, SchoolID, UserName, Category, Password, PasswordAnswer)
VALUES (@RegistrationID, @SchoolID, @UserName, N'Sub-Admin', @Password, @PasswordAnswer)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@Password", password);
        cmd.Parameters.AddWithValue("@PasswordAnswer", answer);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertEducationYearAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, int educationYearId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Education_Year_User (RegistrationID, EducationYearID, SchoolID)
VALUES (@RegistrationID, @EducationYearID, @SchoolID)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertAdminAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, string firstName, string lastName, string designation, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Admin (RegistrationID, SchoolID, FirstName, LastName, Designation)
VALUES (@RegistrationID, @SchoolID, @FirstName, @LastName, @Designation)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@FirstName", firstName);
        cmd.Parameters.AddWithValue("@LastName", lastName);
        cmd.Parameters.AddWithValue("@Designation", designation);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubAdminAccountDto>> ListAccountsAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT Registration.RegistrationID,
       LTRIM(RTRIM(ISNULL(Admin.FirstName, N'') + N' ' + ISNULL(Admin.LastName, N''))) AS Name,
       ISNULL(Admin.Designation, N'') AS Designation,
       Admin.Phone,
       aspnet_Membership.Email,
       Registration.UserName,
       ISNULL(Registration.Validation, N'') AS Validation,
       aspnet_Membership.IsApproved,
       aspnet_Membership.IsLockedOut,
       aspnet_Membership.CreateDate,
       aspnet_Membership.LastLoginDate,
       aspnet_Membership.LastPasswordChangedDate
FROM dbo.aspnet_Users
INNER JOIN dbo.aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId
INNER JOIN dbo.aspnet_Applications AS a
    ON a.ApplicationId = aspnet_Users.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.Registration ON aspnet_Users.UserName = Registration.UserName
INNER JOIN dbo.Admin ON Registration.RegistrationID = Admin.RegistrationID
WHERE Registration.Category = N'Sub-Admin'
  AND Registration.SchoolID = @SchoolID
ORDER BY Name
""";

        var items = new List<SubAdminAccountDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SubAdminAccountDto
            {
                RegistrationID = Convert.ToInt32(reader["RegistrationID"]),
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                Phone = reader["Phone"] as string,
                Email = reader["Email"] as string,
                UserName = reader["UserName"]?.ToString() ?? "",
                Validation = reader["Validation"]?.ToString() ?? "",
                IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"]),
                CreateDate = ReadDate(reader["CreateDate"]),
                LastLoginDate = ReadDate(reader["LastLoginDate"]),
                LastPasswordChangedDate = ReadDate(reader["LastPasswordChangedDate"])
            });
        }

        return items;
    }

    public async Task<SubAdminStatusResult> SetApprovedAsync(
        SessionSnapshot session, SetSubAdminApprovedRequest? request, CancellationToken cancellationToken)
    {
        request ??= new SetSubAdminApprovedRequest();
        var userName = (request.UserName ?? "").Trim();
        if (userName.Length == 0)
            return StatusFail("subact.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var userId = await FindSchoolSubAdminUserIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (userId is null)
            return StatusFail("subact.needUser");

        await using var cmd = new SqlCommand("""
UPDATE dbo.aspnet_Membership
SET IsApproved = @IsApproved
WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@IsApproved", request.IsApproved);
        cmd.Parameters.AddWithValue("@UserId", userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return await ReadStatusAsync(con, userId.Value, cancellationToken);
    }

    public async Task<SubAdminStatusResult> UnlockAsync(
        SessionSnapshot session, UnlockSubAdminRequest? request, CancellationToken cancellationToken)
    {
        request ??= new UnlockSubAdminRequest();
        var userName = (request.UserName ?? "").Trim();
        if (userName.Length == 0)
            return StatusFail("subact.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var userId = await FindSchoolSubAdminUserIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (userId is null)
            return StatusFail("subact.needUser");

        await using var cmd = new SqlCommand("""
UPDATE dbo.aspnet_Membership
SET IsLockedOut = 0,
    FailedPasswordAttemptCount = 0,
    FailedPasswordAttemptWindowStart = CONVERT(datetime, '17540101', 112),
    FailedPasswordAnswerAttemptCount = 0,
    FailedPasswordAnswerAttemptWindowStart = CONVERT(datetime, '17540101', 112),
    LastLockoutDate = CONVERT(datetime, '17540101', 112)
WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@UserId", userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return await ReadStatusAsync(con, userId.Value, cancellationToken);
    }

    private static SubAdminStatusResult StatusFail(string error) =>
        new() { Succeeded = false, Error = error };

    private static async Task<Guid?> FindSchoolSubAdminUserIdAsync(
        SqlConnection con, int schoolId, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 u.UserId
FROM dbo.aspnet_Users AS u
INNER JOIN dbo.aspnet_Applications AS a
    ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
INNER JOIN dbo.Registration AS r ON r.UserName = u.UserName
WHERE u.LoweredUserName = LOWER(@UserName)
  AND r.SchoolID = @SchoolID
  AND r.Category = N'Sub-Admin'
""", con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is Guid id ? id : value is null or DBNull ? null : (Guid?)value;
    }

    private static async Task<SubAdminStatusResult> ReadStatusAsync(
        SqlConnection con, Guid userId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT IsApproved, IsLockedOut
FROM dbo.aspnet_Membership
WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return StatusFail("subact.needUser");

        return new SubAdminStatusResult
        {
            Succeeded = true,
            IsApproved = Convert.ToBoolean(reader["IsApproved"]),
            IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"])
        };
    }

    private static DateTime? ReadDate(object value)
    {
        if (value is null or DBNull)
            return null;
        var date = Convert.ToDateTime(value);
        return date.Year < 1900 ? null : date;
    }
}
