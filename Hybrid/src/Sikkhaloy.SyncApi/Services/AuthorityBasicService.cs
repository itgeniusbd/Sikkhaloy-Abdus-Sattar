using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityBasicService
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly string[] SmsProviders = ["BanglaPhone", "GreenWeb", "Novocom"];
    private const int MaxLogoBytes = 1_500_000;
    private const string SmsPurpose = "Authority Client SMS";

    private readonly EduConnectionFactory _connections;
    private readonly SmsOfficeService _sms;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopes;

    public AuthorityBasicService(
        EduConnectionFactory connections,
        SmsOfficeService sms,
        IConfiguration config,
        IServiceScopeFactory scopes)
    {
        _connections = connections;
        _sms = sms;
        _config = config;
        _scopes = scopes;
    }

    private static void Guard(SessionSnapshot session)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");
    }

    private static AuthorityResult Fail(string error) => new() { Succeeded = false, Error = error };
    private static AuthorityResult Ok(string? message = null, int id = 0) =>
        new() { Succeeded = true, Message = message, Id = id };

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static long L(object? value) => value is null or DBNull ? 0L : Convert.ToInt64(value);
    private static bool B(object? value) =>
        value is bool b ? b : value is null or DBNull ? false : Convert.ToBoolean(value);
    private static DateTime? Dt(object? value) =>
        value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public async Task<SignupLookupsDto> GetSignupLookupsAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var dto = new SignupLookupsDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(
                "SELECT ReferenceID, Reference_Name, Reference_Phone FROM dbo.AAP_Reference ORDER BY Reference_Name", con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var name = S(reader["Reference_Name"]);
                var phone = S(reader["Reference_Phone"]);
                dto.Referrers.Add(new AuthorityOptionDto
                {
                    Id = I(reader["ReferenceID"]),
                    Name = string.IsNullOrWhiteSpace(phone) ? name : $"{name} ({phone})"
                });
            }
        }
        catch
        {
        }
        return dto;
    }

    public async Task<AuthorityResult> CreateSignupUserAsync(
        SessionSnapshot session, SignupUserRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new SignupUserRequest();
        var userName = (request.UserName ?? "").Trim();
        var password = request.Password ?? "";
        var confirm = request.ConfirmPassword ?? "";
        var email = (request.Email ?? "").Trim();
        var question = (request.Question ?? "").Trim();
        var answer = (request.Answer ?? "").Trim();

        if (userName.Length == 0 || password.Length == 0 || email.Length == 0
            || question.Length == 0 || answer.Length == 0)
            return Fail("ab.required");
        if (userName.Any(char.IsWhiteSpace))
            return Fail("ab.userSpace");
        if (userName.Length is < 8 or > 30)
            return Fail("ab.userLen");
        if (password.Length is < 8 or > 30)
            return Fail("ab.passLen");
        if (!string.Equals(password, confirm, StringComparison.Ordinal))
            return Fail("ab.passMatch");
        if (!EmailPattern.IsMatch(email))
            return Fail("ab.emailInvalid");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            if (await UserExistsAsync(con, tx, userName, ct))
            {
                await tx.RollbackAsync(ct);
                return Fail("ab.userExists");
            }

            var created = await CreateMembershipUserAsync(con, tx, userName, password, email, question, answer, ct);
            if (!created.Succeeded)
            {
                await tx.RollbackAsync(ct);
                return created;
            }

            await AddToRoleAsync(con, tx, userName, "Admin", ct);
            await tx.CommitAsync(ct);
            return Ok("ab.userCreated");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    public async Task<AuthorityResult> CreateInstitutionAsync(
        SessionSnapshot session, SignupInstitutionRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new SignupInstitutionRequest();
        var userName = (request.UserName ?? "").Trim();
        var password = request.Password ?? "";
        var answer = request.PasswordAnswer ?? "";
        var schoolName = (request.SchoolName ?? "").Trim();
        var city = (request.City ?? "").Trim();
        var rateText = (request.PerStudentRate ?? "").Trim();

        if (userName.Length == 0 || password.Length == 0 || answer.Length == 0)
            return Fail("ab.needUser");
        if (schoolName.Length == 0 || city.Length == 0 || rateText.Length == 0)
            return Fail("ab.instRequired");

        byte[]? logo = null;
        if (!string.IsNullOrWhiteSpace(request.LogoBase64))
        {
            logo = DecodeImage(request.LogoBase64);
            if (logo is null)
                return Fail("ab.badLogo");
        }

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.SchoolInfo
    (SchoolName, SchoolLogo, Established, Principal, AcadamicStaff, Students, Address, City, State,
     LocalArea, PostalCode, Phone, Email, Website, UserName, Validation, Date, School_SN, Per_Student_Rate)
VALUES
    (@SchoolName, @SchoolLogo, @Established, @Principal, @AcadamicStaff, @Students, @Address, @City, @State,
     @LocalArea, @PostalCode, @Phone, @Email, @Website, @UserName, N'Valid', GETDATE(), dbo.Institution_SerialNumber(), @Rate);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx))
            {
                ins.Parameters.AddWithValue("@SchoolName", schoolName);
                var logoParam = ins.Parameters.Add("@SchoolLogo", SqlDbType.VarBinary);
                logoParam.Value = logo is { Length: > 0 } ? logo : DBNull.Value;
                ins.Parameters.AddWithValue("@Established", (object?)NullIfEmpty(request.Established) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Principal", (object?)NullIfEmpty(request.Principal) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@AcadamicStaff", (object?)NullIfEmpty(request.AcadamicStaff) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Students", (object?)NullIfEmpty(request.Students) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@City", city);
                ins.Parameters.AddWithValue("@State", (object?)NullIfEmpty(request.State) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@LocalArea", (object?)NullIfEmpty(request.LocalArea) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@PostalCode", (object?)NullIfEmpty(request.PostalCode) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Phone", (object?)NullIfEmpty(request.Phone) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Email", (object?)NullIfEmpty(request.Email) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Website", (object?)NullIfEmpty(request.Website) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@UserName", userName);
                ins.Parameters.AddWithValue("@Rate", rateText);
                var schoolId = Convert.ToInt32(await ins.ExecuteScalarAsync(ct));

                int registrationId;
                await using (var reg = new SqlCommand("""
INSERT INTO dbo.Registration (SchoolID, UserName, Validation, Category, CreateDate)
VALUES (@SchoolID, @UserName, N'Valid', N'Admin', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx))
                {
                    reg.Parameters.AddWithValue("@SchoolID", schoolId);
                    reg.Parameters.AddWithValue("@UserName", userName);
                    registrationId = Convert.ToInt32(await reg.ExecuteScalarAsync(ct));
                }

                await using (var admin = new SqlCommand(
                    "INSERT INTO dbo.Admin (RegistrationID, SchoolID) VALUES (@RegistrationID, @SchoolID)", con, tx))
                {
                    admin.Parameters.AddWithValue("@RegistrationID", registrationId);
                    admin.Parameters.AddWithValue("@SchoolID", schoolId);
                    await admin.ExecuteNonQueryAsync(ct);
                }

                await using (var ast = new SqlCommand("""
INSERT INTO dbo.AST (RegistrationID, SchoolID, UserName, Category, Password, PasswordAnswer)
VALUES (@RegistrationID, @SchoolID, @UserName, N'Admin', @Password, @PasswordAnswer)
""", con, tx))
                {
                    ast.Parameters.AddWithValue("@RegistrationID", registrationId);
                    ast.Parameters.AddWithValue("@SchoolID", schoolId);
                    ast.Parameters.AddWithValue("@UserName", userName);
                    ast.Parameters.AddWithValue("@Password", password);
                    ast.Parameters.AddWithValue("@PasswordAnswer", answer);
                    await ast.ExecuteNonQueryAsync(ct);
                }

                await using (var year = new SqlCommand("""
INSERT INTO dbo.Education_Year (SchoolID, RegistrationID, EducationYear, Status, StartDate, EndDate, SN)
VALUES (@SchoolID, @RegistrationID, YEAR(GETDATE()), N'True',
       '01-01-' + CONVERT(varchar(4), YEAR(GETDATE())),
       '12-31-' + CONVERT(varchar(4), YEAR(GETDATE())),
       [dbo].[F_EducationYear_SN](@SchoolID));
INSERT INTO dbo.Education_Year_User (EducationYearID, SchoolID, RegistrationID)
VALUES (SCOPE_IDENTITY(), @SchoolID, @RegistrationID);
""", con, tx))
                {
                    year.Parameters.AddWithValue("@SchoolID", schoolId);
                    year.Parameters.AddWithValue("@RegistrationID", registrationId);
                    await year.ExecuteNonQueryAsync(ct);
                }

                await using (var sms = new SqlCommand("""
INSERT INTO dbo.SMS (SchoolID, SMS_Balance, Masking, Date)
VALUES (@SchoolID, 0, N'Sikkhaloy', GETDATE())
""", con, tx))
                {
                    sms.Parameters.AddWithValue("@SchoolID", schoolId);
                    await sms.ExecuteNonQueryAsync(ct);
                }

                if (request.ReferrerId > 0)
                {
                    decimal.TryParse(request.Commission, out var pct);
                    if (pct < 0) pct = 0;
                    var years = request.DurationYears is 3 or 5 ? request.DurationYears : 2;
                    await using var refer = new SqlCommand("""
IF NOT EXISTS (SELECT 1 FROM dbo.AAP_Reference_School WHERE SchoolID = @SchoolID AND ReferenceID = @RefID)
INSERT INTO dbo.AAP_Reference_School (SchoolID, ReferenceID, Percentage, School_SignUp_Date, End_Reference_Date)
VALUES (@SchoolID, @RefID, @Pct, CAST(GETDATE() AS DATE), DATEADD(YEAR, @Years, CAST(GETDATE() AS DATE)))
""", con, tx);
                    refer.Parameters.AddWithValue("@SchoolID", schoolId);
                    refer.Parameters.AddWithValue("@RefID", request.ReferrerId);
                    refer.Parameters.AddWithValue("@Pct", pct);
                    refer.Parameters.AddWithValue("@Years", years);
                    await refer.ExecuteNonQueryAsync(ct);
                }

                await tx.CommitAsync(ct);
                return Ok("ab.instCreated", schoolId);
            }
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return Fail(ex.Message);
        }
    }

    public async Task<UserInfoListDto> GetUserInfoAsync(
        SessionSnapshot session, string? q, string? validation, string? password, CancellationToken ct)
    {
        Guard(session);
        var dto = new UserInfoListDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        var where = new StringBuilder();
        await using var cmd = new SqlCommand { Connection = con };
        if (!string.IsNullOrWhiteSpace(q))
        {
            And(where);
            where.Append("(Sch.SchoolName LIKE @q OR AdminUser.UserName LIKE @q OR Sch.Phone LIKE @q OR CAST(Sch.SchoolID AS VARCHAR(20)) LIKE @q)");
            cmd.Parameters.AddWithValue("@q", "%" + q.Trim() + "%");
        }
        if (string.Equals(validation, "Valid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(validation, "Invalid", StringComparison.OrdinalIgnoreCase))
        {
            And(where);
            where.Append("Sch.Validation = @val");
            cmd.Parameters.AddWithValue("@val", validation!.Trim());
        }
        if (string.Equals(password, "HasPassword", StringComparison.OrdinalIgnoreCase))
        {
            And(where);
            where.Append("(AdminUser.Password IS NOT NULL AND AdminUser.Password <> N'')");
        }
        else if (string.Equals(password, "NoPassword", StringComparison.OrdinalIgnoreCase))
        {
            And(where);
            where.Append("(AdminUser.Password IS NULL OR AdminUser.Password = N'')");
        }

        var whereSql = where.Length == 0 ? "" : " WHERE " + where;
        var join = """
FROM dbo.SchoolInfo AS Sch
LEFT JOIN dbo.AST AS AdminUser ON AdminUser.SchoolID = Sch.SchoolID AND AdminUser.Category = N'admin'
""";

        await using (var count = new SqlCommand($"""
SELECT COUNT(*) AS TotalCount,
       SUM(CASE WHEN Sch.Validation = N'Valid' THEN 1 ELSE 0 END) AS ValidCount,
       SUM(CASE WHEN Sch.Validation = N'Invalid' THEN 1 ELSE 0 END) AS InvalidCount
{join}
{whereSql}
""", con))
        {
            CopyParameters(cmd, count);
            await using var reader = await count.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Total = I(reader["TotalCount"]);
                dto.Valid = I(reader["ValidCount"]);
                dto.Invalid = I(reader["InvalidCount"]);
            }
        }

        cmd.CommandText = $"""
SELECT Sch.SchoolID, Sch.SchoolName, AdminUser.UserName, AdminUser.Password, Sch.Phone, Sch.Validation,
       ISNULL(STUFF((
           SELECT N', ' + CAST(EducationYear AS nvarchar(20))
           FROM dbo.Education_Year ey
           WHERE ey.SchoolID = Sch.SchoolID AND ey.IsActive = 1
           FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, N''), N'') AS SessionNames
{join}
{whereSql}
ORDER BY Sch.SchoolID
""";
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Rows.Add(new UserInfoRowDto
                {
                    SchoolID = I(reader["SchoolID"]),
                    SchoolName = S(reader["SchoolName"]),
                    UserName = S(reader["UserName"]),
                    Password = S(reader["Password"]),
                    Phone = S(reader["Phone"]),
                    Validation = S(reader["Validation"]),
                    SessionNames = S(reader["SessionNames"])
                });
            }
        }
        return dto;
    }

    public async Task<IReadOnlyList<SchoolUserDto>> GetSchoolUsersAsync(
        SessionSnapshot session, int schoolId, string? category, CancellationToken ct)
    {
        Guard(session);
        var items = new List<SchoolUserDto>();
        if (schoolId <= 0 || string.IsNullOrWhiteSpace(category) || category == "%")
            return items;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT Registration.RegistrationID, Registration.UserName, aspnet_Membership.IsApproved,
       aspnet_Membership.IsLockedOut, aspnet_Membership.Email, AST.Password,
       Registration.Validation, Registration.CreateDate
FROM dbo.aspnet_Users
INNER JOIN dbo.aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId
INNER JOIN dbo.Registration INNER JOIN dbo.AST ON Registration.RegistrationID = AST.RegistrationID
    ON aspnet_Users.UserName = Registration.UserName
WHERE Registration.SchoolID = @SchoolID AND Registration.Category = @Category
ORDER BY Registration.UserName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Category", category.Trim());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new SchoolUserDto
            {
                RegistrationID = I(reader["RegistrationID"]),
                UserName = S(reader["UserName"]),
                Password = S(reader["Password"]),
                IsApproved = B(reader["IsApproved"]),
                IsLockedOut = B(reader["IsLockedOut"]),
                Email = S(reader["Email"]),
                Validation = S(reader["Validation"]),
                CreateDate = Dt(reader["CreateDate"])
            });
        }
        return items;
    }

    public async Task<AuthorityResult> SetApprovedAsync(
        SessionSnapshot session, SetApprovedRequest? request, CancellationToken ct)
    {
        Guard(session);
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0)
            return Fail("ab.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("""
UPDATE m SET m.IsApproved = @IsApproved
FROM dbo.aspnet_Membership AS m
INNER JOIN dbo.aspnet_Users AS u ON u.UserId = m.UserId
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
WHERE u.LoweredUserName = LOWER(@UserName)
""", con))
        {
            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@IsApproved", request!.IsApproved);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using (var sch = new SqlCommand(
            "UPDATE dbo.SchoolInfo SET Validation = @Validation WHERE UserName = @UserName", con))
        {
            sch.Parameters.AddWithValue("@UserName", userName);
            sch.Parameters.AddWithValue("@Validation", request.IsApproved ? "Valid" : "Invalid");
            await sch.ExecuteNonQueryAsync(ct);
        }

        return Ok("ab.saved");
    }

    public async Task<AuthorityResult> UnlockUserAsync(
        SessionSnapshot session, UnlockUserRequest? request, CancellationToken ct)
    {
        Guard(session);
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0)
            return Fail("ab.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE m SET m.IsLockedOut = 0,
             m.FailedPasswordAttemptCount = 0,
             m.FailedPasswordAnswerAttemptCount = 0
FROM dbo.aspnet_Membership AS m
INNER JOIN dbo.aspnet_Users AS u ON u.UserId = m.UserId
INNER JOIN dbo.aspnet_Applications AS a ON a.ApplicationId = u.ApplicationId AND a.LoweredApplicationName = N'/'
WHERE u.LoweredUserName = LOWER(@UserName)
""", con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.unlocked");
    }

    public async Task<IReadOnlyList<TestimonialRowDto>> GetTestimonialsAsync(
        SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var items = new List<TestimonialRowDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("""
SELECT Public_Testimonial.TestimonialID, Public_Testimonial.Testimonial_Text, Public_Testimonial.Is_Show,
       Public_Testimonial.Show_SN, SchoolInfo.SchoolName, Public_Testimonial.Insert_Date
FROM dbo.Public_Testimonial
INNER JOIN dbo.SchoolInfo ON Public_Testimonial.SchoolID = SchoolInfo.SchoolID
ORDER BY Public_Testimonial.Show_SN
""", con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new TestimonialRowDto
                {
                    TestimonialID = I(reader["TestimonialID"]),
                    ShowSn = I(reader["Show_SN"]),
                    SchoolName = S(reader["SchoolName"]),
                    Text = S(reader["Testimonial_Text"]),
                    InsertDate = Dt(reader["Insert_Date"]),
                    IsShow = B(reader["Is_Show"])
                });
            }
        }
        catch
        {
        }
        return items;
    }

    public async Task<AuthorityResult> SaveTestimonialAsync(
        SessionSnapshot session, SaveTestimonialRequest? request, CancellationToken ct)
    {
        Guard(session);
        if (request is null || request.TestimonialID <= 0)
            return Fail("ab.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Public_Testimonial
SET Testimonial_Text = @Text, Show_SN = @ShowSn
WHERE TestimonialID = @ID
""", con);
        cmd.Parameters.AddWithValue("@Text", request.Text ?? "");
        cmd.Parameters.AddWithValue("@ShowSn", request.ShowSn);
        cmd.Parameters.AddWithValue("@ID", request.TestimonialID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.saved");
    }

    public async Task<AuthorityResult> SetTestimonialShowAsync(
        SessionSnapshot session, SetTestimonialShowRequest? request, CancellationToken ct)
    {
        Guard(session);
        if (request is null || request.TestimonialID <= 0)
            return Fail("ab.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "UPDATE dbo.Public_Testimonial SET Is_Show = @IsShow WHERE TestimonialID = @ID", con);
        cmd.Parameters.AddWithValue("@IsShow", request.IsShow);
        cmd.Parameters.AddWithValue("@ID", request.TestimonialID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.saved");
    }

    public async Task<AttSignupPageDto> GetAttendanceSignupAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var dto = new AttSignupPageDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var cmd = new SqlCommand("""
SELECT UserName, SchoolName + N' (' + UserName + N')' AS DisplayName
FROM dbo.SchoolInfo
WHERE Validation = N'Valid'
  AND SchoolID NOT IN (SELECT SchoolID FROM dbo.Attendance_Device_Setting)
ORDER BY SchoolName
""", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Available.Add(new AuthorityOptionDto
                {
                    Extra = S(reader["UserName"]),
                    Name = S(reader["DisplayName"])
                });
            }
        }

        await using (var cmd = new SqlCommand("""
SELECT SchoolInfo.SchoolID, SchoolInfo.SchoolName, Attendance_Device_Setting.UserName,
       Attendance_Device_Setting.Password, Attendance_Device_Setting.IsActive
FROM dbo.Attendance_Device_Setting
INNER JOIN dbo.SchoolInfo ON Attendance_Device_Setting.SchoolID = SchoolInfo.SchoolID
ORDER BY SchoolInfo.SchoolName
""", con))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Registered.Add(new AttDeviceRowDto
                {
                    SchoolID = I(reader["SchoolID"]),
                    SchoolName = S(reader["SchoolName"]),
                    UserName = S(reader["UserName"]),
                    Password = S(reader["Password"]),
                    IsActive = B(reader["IsActive"])
                });
            }
        }
        return dto;
    }

    public async Task<AuthorityResult> RegisterAttendanceAsync(
        SessionSnapshot session, AttRegisterRequest? request, CancellationToken ct)
    {
        Guard(session);
        var userName = (request?.UserName ?? "").Trim();
        var password = request?.Password ?? "";
        if (userName.Length == 0 || userName == "0" || password.Length == 0)
            return Fail("ab.attNeed");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Attendance_Device_Setting (SchoolID, UserName, Password, SettingKey)
SELECT SchoolID, UserName, @Password, N'123456'
FROM dbo.SchoolInfo
WHERE UserName = @UserName
""", con);
        cmd.Parameters.AddWithValue("@Password", password);
        cmd.Parameters.AddWithValue("@UserName", userName);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0 ? Ok("ab.attOk") : Fail("ab.attFail");
    }

    public async Task<AuthorityResult> SetAttendancePasswordAsync(
        SessionSnapshot session, AttPasswordRequest? request, CancellationToken ct)
    {
        Guard(session);
        var userName = (request?.UserName ?? "").Trim();
        var password = request?.Password ?? "";
        if (userName.Length == 0 || password.Length == 0)
            return Fail("ab.attNeed");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "UPDATE dbo.Attendance_Device_Setting SET Password = @Password WHERE UserName = @UserName", con);
        cmd.Parameters.AddWithValue("@Password", password);
        cmd.Parameters.AddWithValue("@UserName", userName);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.saved");
    }

    public async Task<AuthorityResult> SetAttendanceActiveAsync(
        SessionSnapshot session, AttActiveRequest? request, CancellationToken ct)
    {
        Guard(session);
        if (request is null || request.SchoolID <= 0)
            return Fail("ab.failed");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "UPDATE dbo.Attendance_Device_Setting SET IsActive = @IsActive WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@IsActive", request.IsActive);
        cmd.Parameters.AddWithValue("@SchoolID", request.SchoolID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.saved");
    }

    public async Task<SmsSettingPageDto> GetSmsSettingAsync(SessionSnapshot session, CancellationToken ct)
    {
        Guard(session);
        var dto = new SmsSettingPageDto { Providers = [.. SmsProviders] };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 SmsProvider, SmsProviderMultiple, SmsSendInterval, SmsProcessingUnit
FROM dbo.SikkhaloySetting
""", con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.SmsProvider = S(reader["SmsProvider"]);
                dto.SmsProviderMultiple = S(reader["SmsProviderMultiple"]);
                dto.SmsSendInterval = I(reader["SmsSendInterval"]);
                dto.SmsProcessingUnit = I(reader["SmsProcessingUnit"]);
            }
        }
        catch
        {
        }

        try
        {
            await using var pending = new SqlCommand("SELECT COUNT(Attendance_SMSID) FROM dbo.Attendance_SMS", con);
            dto.PendingSms = I(await pending.ExecuteScalarAsync(ct));
        }
        catch
        {
        }
        return dto;
    }

    public async Task<AuthorityResult> SaveSmsSettingAsync(
        SessionSnapshot session, SaveSmsSettingRequest? request, CancellationToken ct)
    {
        Guard(session);
        request ??= new SaveSmsSettingRequest();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE dbo.SikkhaloySetting
SET SmsProvider = @SmsProvider,
    SmsProviderMultiple = @SmsProviderMultiple,
    SmsSendInterval = @SmsSendInterval,
    SmsProcessingUnit = @SmsProcessingUnit
""", con);
        cmd.Parameters.AddWithValue("@SmsProvider", request.SmsProvider ?? "");
        cmd.Parameters.AddWithValue("@SmsProviderMultiple", request.SmsProviderMultiple ?? "");
        cmd.Parameters.AddWithValue("@SmsSendInterval", request.SmsSendInterval);
        cmd.Parameters.AddWithValue("@SmsProcessingUnit", request.SmsProcessingUnit);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("ab.smsSaved");
    }

    public async Task<IReadOnlyList<SmsSenderRowDto>> GetSmsSenderRecordsAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken ct)
    {
        Guard(session);
        var items = new List<SmsSenderRowDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("""
SELECT AttendanceSmsSenderId, AppStartTime, AppCloseTime, TotalEventCall, TotalSmsSend, TotalSmsFailed
FROM dbo.Attendance_SMS_Sender
WHERE (@StartDate IS NULL OR CAST(AppStartTime AS DATE) >= @StartDate)
  AND (@EndDate IS NULL OR CAST(AppStartTime AS DATE) <= @EndDate)
ORDER BY AttendanceSmsSenderId DESC
""", con);
            cmd.Parameters.AddWithValue("@StartDate", (object?)from?.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object?)to?.Date ?? DBNull.Value);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new SmsSenderRowDto
                {
                    Id = I(reader["AttendanceSmsSenderId"]),
                    AppStartTime = Dt(reader["AppStartTime"]),
                    AppCloseTime = Dt(reader["AppCloseTime"]),
                    TotalEventCall = I(reader["TotalEventCall"]),
                    TotalSmsSend = I(reader["TotalSmsSend"]),
                    TotalSmsFailed = I(reader["TotalSmsFailed"])
                });
            }
        }
        catch
        {
        }
        return items;
    }

    public async Task<SmsFailedPageDto> GetFailedSmsAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, string? reason, int schoolId, CancellationToken ct)
    {
        Guard(session);
        var dto = new SmsFailedPageDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        try
        {
            await using (var schools = new SqlCommand(
                "SELECT SchoolID, SchoolName FROM dbo.SchoolInfo ORDER BY SchoolName", con))
            await using (var reader = await schools.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    dto.Schools.Add(new AuthorityOptionDto
                    {
                        Id = I(reader["SchoolID"]),
                        Name = S(reader["SchoolName"])
                    });
                }
            }
        }
        catch
        {
        }

        try
        {
            var filter = FailedWhere(from, to, reason, schoolId, out var args);
            await using (var stats = new SqlCommand($"""
SELECT COUNT(*) AS TotalFailed,
       SUM(CASE WHEN CAST(InsertDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayFailed,
       SUM(CASE WHEN InsertDate >= DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END) AS ThisWeekFailed
FROM dbo.Attendance_SMS_Failed
INNER JOIN dbo.SchoolInfo ON Attendance_SMS_Failed.SchoolID = SchoolInfo.SchoolID
{filter}
""", con))
            {
                foreach (var p in args)
                    stats.Parameters.Add(p);
                await using var reader = await stats.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    dto.TotalFailed = I(reader["TotalFailed"]);
                    dto.TodayFailed = I(reader["TodayFailed"]);
                    dto.ThisWeekFailed = I(reader["ThisWeekFailed"]);
                }
            }

            await using (var cmd = new SqlCommand($"""
SELECT TOP 400 Attendance_SMS_Failed.AttendanceSmsFailedId, Attendance_SMS_Failed.SchoolID,
       ISNULL(CONVERT(varchar(15), Attendance_SMS_Failed.ScheduleTime, 100), '') AS ScheduleTime,
       ISNULL(CONVERT(varchar(15), Attendance_SMS_Failed.CreateTime, 100), '') AS CreateTime,
       ISNULL(CONVERT(varchar(15), Attendance_SMS_Failed.SentTime, 100), '') AS SentTime,
       Attendance_SMS_Failed.AttendanceDate, Attendance_SMS_Failed.SMS_Text, Attendance_SMS_Failed.MobileNo,
       Attendance_SMS_Failed.AttendanceStatus, Attendance_SMS_Failed.SMS_TimeOut, Attendance_SMS_Failed.FailedReson,
       Attendance_SMS_Failed.InsertDate, SchoolInfo.SchoolName
FROM dbo.Attendance_SMS_Failed
INNER JOIN dbo.SchoolInfo ON Attendance_SMS_Failed.SchoolID = SchoolInfo.SchoolID
{filter}
ORDER BY AttendanceSmsFailedId DESC
""", con))
            {
                foreach (var p in args)
                    cmd.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    dto.Rows.Add(new SmsFailedRowDto
                    {
                        Id = I(reader["AttendanceSmsFailedId"]),
                        SchoolID = I(reader["SchoolID"]),
                        SchoolName = S(reader["SchoolName"]),
                        SmsText = S(reader["SMS_Text"]),
                        MobileNo = S(reader["MobileNo"]),
                        AttendanceStatus = S(reader["AttendanceStatus"]),
                        FailedReason = S(reader["FailedReson"]),
                        ScheduleTime = S(reader["ScheduleTime"]),
                        CreateTime = S(reader["CreateTime"]),
                        SentTime = S(reader["SentTime"]),
                        AttendanceDate = Dt(reader["AttendanceDate"]),
                        InsertDate = Dt(reader["InsertDate"]),
                        SmsTimeOut = S(reader["SMS_TimeOut"])
                    });
                }
            }
        }
        catch
        {
        }
        return dto;
    }

    public async Task<ClientSmsPageDto> GetClientSmsAsync(
        SessionSnapshot session, string? q, string? validation, CancellationToken ct)
    {
        Guard(session);
        var dto = new ClientSmsPageDto { LocalMode = _sms.IsLocal, Gateway = await ReadGatewaySummaryAsync(ct) };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        var where = new StringBuilder("WHERE 1=1");
        await using var cmd = new SqlCommand { Connection = con };
        if (!string.IsNullOrWhiteSpace(q))
        {
            where.Append(" AND (SchoolName LIKE @q OR UserName LIKE @q OR Phone LIKE @q OR CAST(SchoolID AS VARCHAR(20)) LIKE @q)");
            cmd.Parameters.AddWithValue("@q", "%" + q.Trim() + "%");
        }
        if (string.Equals(validation, "Valid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(validation, "Invalid", StringComparison.OrdinalIgnoreCase))
        {
            where.Append(" AND Validation = @val");
            cmd.Parameters.AddWithValue("@val", validation!.Trim());
        }

        cmd.CommandText = $"""
SELECT SchoolID, SchoolName, UserName, Phone, Validation, Date
FROM dbo.SchoolInfo
{where}
ORDER BY SchoolID DESC
""";
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var phone = S(reader["Phone"]);
                var valid = S(reader["Validation"]);
                dto.Rows.Add(new ClientSmsRowDto
                {
                    SchoolID = I(reader["SchoolID"]),
                    SchoolName = S(reader["SchoolName"]),
                    UserName = S(reader["UserName"]),
                    Phone = phone,
                    Validation = valid,
                    StatusText = valid == "Valid" ? "Active" : "Deactive",
                    PhoneCount = ParsePhones(phone).Count,
                    Date = Dt(reader["Date"])
                });
            }
        }

        dto.Total = dto.Rows.Count;
        dto.Active = dto.Rows.Count(x => x.Validation == "Valid");
        dto.Deactive = dto.Total - dto.Active;
        dto.Balance = await ReadPlatformBalanceAsync(con, ct);
        return dto;
    }

    public async Task<SendClientSmsResult> SendClientSmsAsync(
        SessionSnapshot session, SendClientSmsRequest? request, CancellationToken ct)
    {
        Guard(session);
        var text = (request?.Text ?? "").Trim();
        if (text.Length == 0)
            return new SendClientSmsResult { Error = "ab.needText" };
        var ids = request?.SchoolIds ?? [];
        if (ids.Count == 0)
            return new SendClientSmsResult { Error = "ab.needSelect" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var platformId = await GetPlatformSmsSchoolIdAsync(con, ct);
        var schools = new List<(int Id, string Name, string Phone)>();
        await using (var cmd = new SqlCommand($"""
SELECT SchoolID, SchoolName, Phone
FROM dbo.SchoolInfo
WHERE SchoolID IN ({string.Join(",", ids.Select((_, i) => "@id" + i))})
""", con))
        {
            for (var i = 0; i < ids.Count; i++)
                cmd.Parameters.AddWithValue("@id" + i, ids[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                schools.Add((I(reader["SchoolID"]), S(reader["SchoolName"]), S(reader["Phone"])));
        }

        var sent = 0;
        var failed = 0;
        var skipped = 0;
        var details = new List<string>();
        foreach (var school in schools)
        {
            var phones = ParsePhones(school.Phone);
            if (phones.Count == 0)
            {
                skipped++;
                details.Add(school.Name + " — no phone.");
                continue;
            }

            foreach (var phone in phones)
            {
                if (!IsValidBdMobile(phone))
                {
                    failed++;
                    details.Add(school.Name + " (" + phone + ") — Invalid Mobile Number");
                    continue;
                }

                var call = await _sms.TrySendAsync(phone, text, ct);
                if (string.IsNullOrWhiteSpace(call.Body))
                {
                    failed++;
                    details.Add(school.Name + " (" + phone + ") — " + (call.Error ?? "not sent"));
                    continue;
                }

                var count = SmsCount(text);
                var smsId = Guid.NewGuid();
                await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, @Purpose, @Status, GETDATE(), @Resp)
""", con))
                {
                    ins.Parameters.AddWithValue("@ID", smsId);
                    ins.Parameters.AddWithValue("@Phone", phone);
                    ins.Parameters.AddWithValue("@Text", text);
                    ins.Parameters.AddWithValue("@Len", text.Length);
                    ins.Parameters.AddWithValue("@Count", count);
                    ins.Parameters.AddWithValue("@Purpose", SmsPurpose);
                    ins.Parameters.AddWithValue("@Status", call.Local ? "Local" : "Sent");
                    ins.Parameters.AddWithValue("@Resp", call.Body);
                    await ins.ExecuteNonQueryAsync(ct);
                }

                await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, EducationYearID)
VALUES (@ID, @SchoolID, 0)
""", con))
                {
                    other.Parameters.AddWithValue("@ID", smsId);
                    other.Parameters.AddWithValue("@SchoolID", platformId);
                    try { await other.ExecuteNonQueryAsync(ct); }
                    catch { /* optional link row */ }
                }

                sent++;
            }
        }

        var balance = await ReadPlatformBalanceAsync(con, ct);
        var summary = sent > 0 && _sms.IsLocal
            ? "Dev Mode: logged, not sent to mobile. "
            : "";
        summary += $"Sent: {sent} | Failed: {failed}";
        if (skipped > 0)
            summary += $" | No phone: {skipped}";

        return new SendClientSmsResult
        {
            Succeeded = sent > 0,
            Sent = sent,
            Failed = failed,
            Skipped = skipped,
            Balance = balance,
            Message = summary,
            Error = sent > 0 ? null : (details.FirstOrDefault() ?? "ab.smsFail"),
            LocalMode = _sms.IsLocal,
            Details = details
        };
    }

    private static string FailedWhere(
        DateTime? from, DateTime? to, string? reason, int schoolId, out List<SqlParameter> args)
    {
        args = [];
        var sql = "WHERE 1=1";
        if (from is DateTime start)
        {
            sql += " AND CAST(AttendanceDate AS DATE) >= @StartDate";
            args.Add(new SqlParameter("@StartDate", start.Date));
        }
        if (to is DateTime end)
        {
            sql += " AND CAST(AttendanceDate AS DATE) <= @EndDate";
            args.Add(new SqlParameter("@EndDate", end.Date));
        }
        if (!string.IsNullOrWhiteSpace(reason))
        {
            sql += " AND FailedReson = @Reason";
            args.Add(new SqlParameter("@Reason", reason.Trim()));
        }
        if (schoolId > 0)
        {
            sql += " AND Attendance_SMS_Failed.SchoolID = @SchoolID";
            args.Add(new SqlParameter("@SchoolID", schoolId));
        }
        return sql;
    }

    private async Task<string> ReadGatewaySummaryAsync(CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand(
                "SELECT TOP 1 SmsProvider, SmsProviderMultiple FROM dbo.SikkhaloySetting", con);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                return "Single: " + S(reader["SmsProvider"]) + " | Multiple: " + S(reader["SmsProviderMultiple"]);
        }
        catch
        {
        }
        return "Unknown";
    }

    private async Task<int> ReadPlatformBalanceAsync(SqlConnection con, CancellationToken ct)
    {
        var schoolId = await GetPlatformSmsSchoolIdAsync(con, ct);
        await using var cmd = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        return I(await cmd.ExecuteScalarAsync(ct));
    }

    private async Task<int> GetPlatformSmsSchoolIdAsync(SqlConnection con, CancellationToken ct)
    {
        var configured = _config["AuthorityPlatformSmsSchoolId"];
        if (int.TryParse(configured, out var id) && id > 0)
            return id;
        try
        {
            await using var cmd = new SqlCommand("SELECT TOP 1 SchoolID FROM dbo.SMS ORDER BY SchoolID", con);
            var value = await cmd.ExecuteScalarAsync(ct);
            if (value is not null and not DBNull)
                return Convert.ToInt32(value);
        }
        catch
        {
        }
        return 1;
    }

    private static List<string> ParsePhones(string phoneField)
    {
        var phones = new List<string>();
        if (string.IsNullOrWhiteSpace(phoneField))
            return phones;
        foreach (var part in phoneField.Split([',', ';', '/', '|'], StringSplitOptions.RemoveEmptyEntries))
        {
            var number = part.Trim();
            if (number.Length > 0 && !phones.Contains(number, StringComparer.Ordinal))
                phones.Add(number);
        }
        return phones;
    }

    private static int SmsCount(string text)
    {
        var unicode = text.Any(ch => ch > 127);
        var size = unicode ? 70 : 160;
        return Math.Max(1, (int)Math.Ceiling(text.Length / (double)size));
    }

    private static bool IsValidBdMobile(string phone)
    {
        var digits = new string((phone ?? "").Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits.StartsWith("01", StringComparison.Ordinal)
               || digits.Length == 13 && digits.StartsWith("8801", StringComparison.Ordinal);
    }

    private static byte[]? DecodeImage(string raw)
    {
        var comma = raw.IndexOf(',');
        var payload = comma >= 0 && raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? raw[(comma + 1)..]
            : raw;
        try
        {
            var bytes = Convert.FromBase64String(payload);
            if (bytes.Length == 0 || bytes.Length > MaxLogoBytes)
                return null;
            return bytes;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static void And(StringBuilder where)
    {
        if (where.Length > 0)
            where.Append(" AND ");
    }

    private static void CopyParameters(SqlCommand source, SqlCommand target)
    {
        foreach (SqlParameter p in source.Parameters)
            target.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);
    }
}
