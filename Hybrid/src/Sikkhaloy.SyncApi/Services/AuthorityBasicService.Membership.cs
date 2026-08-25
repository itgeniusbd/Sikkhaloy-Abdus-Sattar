using System.Data;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityBasicService
{
    internal static async Task<bool> UserExistsAsync(
        SqlConnection con, SqlTransaction tx, string userName, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT 1
WHERE EXISTS (
    SELECT 1 FROM dbo.aspnet_Users AS u
    INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
    WHERE u.LoweredUserName = LOWER(@UserName))
   OR EXISTS (SELECT 1 FROM dbo.Registration WHERE UserName = @UserName)
   OR EXISTS (SELECT 1 FROM dbo.SchoolInfo WHERE UserName = @UserName)
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    internal static async Task<AuthorityResult> CreateMembershipUserAsync(
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
            return Ok();
        }

        var code = returnParam.Value is int i ? i : Convert.ToInt32(returnParam.Value ?? 0);
        return code switch
        {
            0 => Ok(),
            6 => Fail("ab.userExists"),
            7 => Fail("ab.emailExists"),
            _ => Fail("ab.failed")
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

    internal static async Task AddToRoleAsync(
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
}
