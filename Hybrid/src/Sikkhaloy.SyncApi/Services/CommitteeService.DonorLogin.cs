using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Committee;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class CommitteeService
{
    public async Task<DonorLoginPageDto> GetDonorLoginPageAsync(SessionSnapshot session, int typeId, string? q, CancellationToken ct)
    {
        var search = (q ?? "").Trim();
        var dto = new DonorLoginPageDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var pending = new SqlCommand("""
SELECT cm.CommitteeMemberId, cm.MemberName,
       LTRIM(RTRIM(ISNULL(cm.SmsNumber, N''))) AS SmsNumber,
       ISNULL(cm.Address, N'') AS Address, cmt.CommitteeMemberType
FROM dbo.CommitteeMember cm
INNER JOIN dbo.CommitteeMemberType cmt ON cm.CommitteeMemberTypeId = cmt.CommitteeMemberTypeId
WHERE cm.SchoolID = @SchoolID
  AND cm.CommitteeMemberId IS NOT NULL
  AND (@TypeId = 0 OR cm.CommitteeMemberTypeId = @TypeId)
  AND (@Q = N'' OR cm.MemberName LIKE N'%' + @Q + N'%' OR cm.SmsNumber LIKE N'%' + @Q + N'%')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.Registration r
      WHERE r.SchoolID = cm.SchoolID AND r.CommitteeMemberId = cm.CommitteeMemberId)
ORDER BY cm.InsertDate DESC
""", con))
        {
            pending.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            pending.Parameters.AddWithValue("@TypeId", typeId);
            pending.Parameters.AddWithValue("@Q", search);
            await using var reader = await pending.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Pending.Add(new DonorLoginPendingRowDto
                {
                    CommitteeMemberId = I(reader["CommitteeMemberId"]),
                    MemberName = S(reader["MemberName"]),
                    SmsNumber = S(reader["SmsNumber"]),
                    MemberType = S(reader["CommitteeMemberType"]),
                    Address = S(reader["Address"])
                });
            }
        }

        await using (var created = new SqlCommand("""
SELECT cm.CommitteeMemberId, cm.MemberName, ISNULL(cm.SmsNumber, N'') AS SmsNumber,
       r.UserName, ISNULL(a.Password, N'') AS Password, r.CreateDate
FROM dbo.CommitteeMember cm
INNER JOIN dbo.Registration r ON r.SchoolID = cm.SchoolID AND r.CommitteeMemberId = cm.CommitteeMemberId
LEFT JOIN dbo.AST a ON r.RegistrationID = a.RegistrationID
WHERE cm.SchoolID = @SchoolID
  AND cm.CommitteeMemberId IS NOT NULL
  AND (@TypeId = 0 OR cm.CommitteeMemberTypeId = @TypeId)
  AND (@Q = N'' OR cm.MemberName LIKE N'%' + @Q + N'%' OR cm.SmsNumber LIKE N'%' + @Q + N'%')
ORDER BY r.CreateDate DESC
""", con))
        {
            created.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            created.Parameters.AddWithValue("@TypeId", typeId);
            created.Parameters.AddWithValue("@Q", search);
            await using var reader = await created.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Created.Add(new DonorLoginCreatedRowDto
                {
                    CommitteeMemberId = I(reader["CommitteeMemberId"]),
                    MemberName = S(reader["MemberName"]),
                    SmsNumber = S(reader["SmsNumber"]),
                    UserName = S(reader["UserName"]),
                    Password = S(reader["Password"]),
                    CreateDate = Dt(reader["CreateDate"])
                });
            }
        }

        return dto;
    }

    public async Task<DonorLoginCreateResult> CreateDonorLoginsAsync(SessionSnapshot session, DonorLoginCreateRequest? request, CancellationToken ct)
    {
        var ids = (request?.CommitteeMemberIds ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return new DonorLoginCreateResult { Error = "cm.bulkEditNeedSelect" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var created = 0;
        var skipped = 0;

        foreach (var memberId in ids)
        {
            var member = await LoadDonorLoginMemberAsync(con, session.SchoolID, memberId, ct);
            if (member is null || string.IsNullOrWhiteSpace(member.Value.Phone))
            {
                skipped++;
                continue;
            }

            if (await DonorRegistrationExistsAsync(con, session.SchoolID, memberId, ct))
            {
                skipped++;
                continue;
            }

            await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
            try
            {
                var userName = await GenerateDonorUserNameAsync(con, tx, session.SchoolID, ct);
                var password = Random.Shared.Next(100000, 999999).ToString();
                var email = $"{userName}@sikkhaloy.local";
                await EnsureMembershipRoleAsync(con, tx, "Donor", ct);
                await InsertDonorMembershipAsync(con, tx, userName, password, email, ct);
                await AddDonorToRoleAsync(con, tx, userName, "Donor", ct);
                var registrationId = await InsertDonorRegistrationAsync(con, tx, session.SchoolID, userName, memberId, ct);
                await InsertDonorAstAsync(con, tx, registrationId, session.SchoolID, userName, password, member.Value.Phone, ct);
                await InsertDonorEducationYearAsync(con, tx, registrationId, session.SchoolID, session.EducationYearID, ct);
                await tx.CommitAsync(ct);
                created++;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                skipped++;
            }
        }

        if (created == 0)
            return new DonorLoginCreateResult { Error = "cm.donorLoginNotCreated", Skipped = skipped };
        return new DonorLoginCreateResult
        {
            Succeeded = true,
            Created = created,
            Skipped = skipped,
            Message = "cm.donorLoginCreated"
        };
    }

    public async Task<DonorLoginSmsResult> SendDonorLoginSmsAsync(SessionSnapshot session, DonorLoginSmsRequest? request, CancellationToken ct)
    {
        var ids = (request?.CommitteeMemberIds ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return new DonorLoginSmsResult { Error = "cm.bulkEditNeedSelect" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var balance = await ReadSmsBalanceAsync(con, session.SchoolID, ct);
        var school = await SessionSchool.ResolveNameAsync(session, con, ct);
        var jobs = new List<(string Phone, string Message, int Count)>();

        foreach (var id in ids)
        {
            await using var cmd = new SqlCommand("""
SELECT cm.MemberName, ISNULL(cm.SmsNumber, N'') AS SmsNumber, r.UserName, ISNULL(a.Password, N'') AS Password
FROM dbo.CommitteeMember cm
INNER JOIN dbo.Registration r ON r.SchoolID = cm.SchoolID AND r.CommitteeMemberId = cm.CommitteeMemberId
LEFT JOIN dbo.AST a ON r.RegistrationID = a.RegistrationID
WHERE cm.SchoolID = @SchoolID AND cm.CommitteeMemberId = @Id
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) continue;
            var name = S(reader["MemberName"]);
            var phone = S(reader["SmsNumber"]);
            var user = S(reader["UserName"]);
            var pass = S(reader["Password"]);
            var msg = $" সম্মানিত দাতা {name},আপনার লগিন ইউজার আইডি: {user},ও পাসওয়ার্ড: {pass}. ভবিষ্যতের জন্য সংরক্ষণ করুন, ধন্যবাদ:, {school}";
            jobs.Add((phone, msg, IsValidBdMobile(phone) ? SmsCount(msg) : 0));
        }

        var needed = jobs.Sum(x => x.Count);
        if (needed == 0)
            return new DonorLoginSmsResult { Error = "acc.smsNoPhone", Failed = ids.Count };
        if (balance < needed)
            return new DonorLoginSmsResult { Error = "acc.smsLow", Message = balance.ToString() };

        var sent = 0;
        var failed = 0;
        foreach (var job in jobs)
        {
            if (job.Count <= 0 || string.IsNullOrWhiteSpace(job.Phone))
            {
                failed++;
                continue;
            }
            var resp = await _gateway.SendAsync(job.Phone, job.Message, ct);
            if (!string.IsNullOrWhiteSpace(resp.Error))
            {
                failed++;
                continue;
            }
            var smsId = Guid.NewGuid();
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES (@ID, @Phone, @Text, @Len, @Count, N'Donor Login Info', @Status, GETDATE(), @Resp)
""", con))
            {
                ins.Parameters.AddWithValue("@ID", smsId);
                ins.Parameters.AddWithValue("@Phone", job.Phone);
                ins.Parameters.AddWithValue("@Text", job.Message);
                ins.Parameters.AddWithValue("@Len", job.Message.Length);
                ins.Parameters.AddWithValue("@Count", job.Count);
                ins.Parameters.AddWithValue("@Status", _local.IsLocal ? "Local" : "Sent");
                ins.Parameters.AddWithValue("@Resp", resp.Body ?? "");
                await ins.ExecuteNonQueryAsync(ct);
            }
            sent++;
        }

        return new DonorLoginSmsResult
        {
            Succeeded = sent > 0,
            Sent = sent,
            Failed = failed,
            Message = sent > 0 ? "cm.donorLoginSmsOk" : null,
            Error = sent == 0 ? "cm.fail" : null
        };
    }

    private static async Task<(string Name, string Phone)?> LoadDonorLoginMemberAsync(SqlConnection con, int schoolId, int memberId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT MemberName, LTRIM(RTRIM(ISNULL(SmsNumber, N''))) AS SmsNumber
FROM dbo.CommitteeMember
WHERE SchoolID = @SchoolID AND CommitteeMemberId = @Id
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Id", memberId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return (S(reader["MemberName"]), S(reader["SmsNumber"]));
    }

    private static async Task<bool> DonorRegistrationExistsAsync(SqlConnection con, int schoolId, int memberId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT 1 FROM dbo.Registration
WHERE SchoolID = @SchoolID AND CommitteeMemberId = @MemberId
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@MemberId", memberId);
        return await cmd.ExecuteScalarAsync(ct) is not null and not DBNull;
    }

    private static async Task<string> GenerateDonorUserNameAsync(SqlConnection con, SqlTransaction tx, int schoolId, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var userName = schoolId + Random.Shared.Next(100000, 999999).ToString();
            if (!await UserNameTakenAsync(con, tx, userName, ct))
                return userName;
        }
        return schoolId + DateTime.Now.ToString("HHmmssfff");
    }

    private static async Task<bool> UserNameTakenAsync(SqlConnection con, SqlTransaction tx, string userName, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.aspnet_Users AS u
    INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
    WHERE u.LoweredUserName = LOWER(@UserName))
   OR EXISTS (SELECT 1 FROM dbo.Registration WHERE UserName = @UserName)
THEN 1 ELSE 0 END
""", con, tx);
        cmd.Parameters.AddWithValue("@UserName", userName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
    }

    private static async Task EnsureMembershipRoleAsync(SqlConnection con, SqlTransaction tx, string roleName, CancellationToken ct)
    {
        await using var check = new SqlCommand("""
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.aspnet_Roles AS r
    INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = r.ApplicationId AND a.LoweredApplicationName = N'/'
    WHERE r.LoweredRoleName = LOWER(@RoleName))
THEN 1 ELSE 0 END
""", con, tx);
        check.Parameters.AddWithValue("@RoleName", roleName);
        if (Convert.ToInt32(await check.ExecuteScalarAsync(ct)) == 1)
            return;

        await using var appCmd = new SqlCommand(
            "SELECT ApplicationId FROM dbo.aspnet_Applications WHERE LoweredApplicationName = N'/'", con, tx);
        var appIdObj = await appCmd.ExecuteScalarAsync(ct);
        if (appIdObj is null or DBNull)
            throw new InvalidOperationException("Membership application '/' was not found.");
        var appId = (Guid)appIdObj;

        await using var ins = new SqlCommand("""
INSERT INTO dbo.aspnet_Roles (ApplicationId, RoleId, RoleName, LoweredRoleName, Description)
VALUES (@ApplicationId, @RoleId, @RoleName, LOWER(@RoleName), @RoleName)
""", con, tx);
        ins.Parameters.AddWithValue("@ApplicationId", appId);
        ins.Parameters.AddWithValue("@RoleId", Guid.NewGuid());
        ins.Parameters.AddWithValue("@RoleName", roleName);
        await ins.ExecuteNonQueryAsync(ct);
    }

    private static async Task InsertDonorMembershipAsync(
        SqlConnection con, SqlTransaction tx, string userName, string password, string email, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var salt = MembershipPasswordVerifier.NewSalt();
        var hashedPassword = MembershipPasswordVerifier.Hash(password, salt);
        var hashedAnswer = MembershipPasswordVerifier.Hash(password, salt);
        var utcNow = DateTime.UtcNow;
        var lockout = new DateTime(1754, 1, 1);

        await using var appCmd = new SqlCommand(
            "SELECT ApplicationId FROM dbo.aspnet_Applications WHERE LoweredApplicationName = N'/'", con, tx);
        var appIdObj = await appCmd.ExecuteScalarAsync(ct);
        if (appIdObj is null or DBNull)
            throw new InvalidOperationException("Membership application '/' was not found.");
        var appId = (Guid)appIdObj;

        await using var userCmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
VALUES (@ApplicationId, @UserId, @UserName, LOWER(@UserName), 0, @Now)
""", con, tx);
        userCmd.Parameters.AddWithValue("@ApplicationId", appId);
        userCmd.Parameters.AddWithValue("@UserId", userId);
        userCmd.Parameters.AddWithValue("@UserName", userName);
        userCmd.Parameters.AddWithValue("@Now", utcNow);
        await userCmd.ExecuteNonQueryAsync(ct);

        await using var memCmd = new SqlCommand("""
INSERT INTO dbo.aspnet_Membership
    (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, Email, LoweredEmail,
     PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate,
     LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
     FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)
VALUES
    (@ApplicationId, @UserId, @Password, 1, @PasswordSalt, @Email, LOWER(@Email),
     N'Donor', @PasswordAnswer, 1, 0, @Now, @Now,
     @Now, @Lockout, 0, @Lockout,
     0, @Lockout)
""", con, tx);
        memCmd.Parameters.AddWithValue("@ApplicationId", appId);
        memCmd.Parameters.AddWithValue("@UserId", userId);
        memCmd.Parameters.AddWithValue("@Password", hashedPassword);
        memCmd.Parameters.AddWithValue("@PasswordSalt", salt);
        memCmd.Parameters.AddWithValue("@Email", email);
        memCmd.Parameters.AddWithValue("@PasswordAnswer", hashedAnswer);
        memCmd.Parameters.AddWithValue("@Now", utcNow);
        memCmd.Parameters.AddWithValue("@Lockout", lockout);
        await memCmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task AddDonorToRoleAsync(
        SqlConnection con, SqlTransaction tx, string userName, string roleName, CancellationToken ct)
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
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> InsertDonorRegistrationAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, string userName, int memberId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Registration (SchoolID, UserName, CommitteeMemberId, Validation, Category, CreateDate)
VALUES (@SchoolID, @UserName, @MemberId, N'Valid', N'Donor', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@MemberId", memberId);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }

    private static async Task InsertDonorAstAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, string userName,
        string password, string smsNumber, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
INSERT INTO dbo.AST (RegistrationID, SchoolID, UserName, Password, SmsNumber, Category)
VALUES (@RegistrationID, @SchoolID, @UserName, @Password, @SmsNumber, N'Donor')
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@Password", password);
        cmd.Parameters.AddWithValue("@SmsNumber", smsNumber);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task InsertDonorEducationYearAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, int educationYearId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Education_Year_User (RegistrationID, EducationYearID, SchoolID)
VALUES (@RegistrationID, @EducationYearID, @SchoolID)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@EducationYearID", educationYearId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
