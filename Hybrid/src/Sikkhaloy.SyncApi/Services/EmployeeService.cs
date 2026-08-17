using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Employees;

namespace Sikkhaloy.SyncApi.Services;

public sealed class EmployeeService
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly EduConnectionFactory _connections;

    public EmployeeService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<IReadOnlyList<EmployeeListDto>> ListAsync(
        SessionSnapshot session, string? type, string? status, string? query, CancellationToken cancellationToken)
    {
        var employeeType = NormalizeType(type);
        var jobStatus = NormalizeStatus(status);
        var search = (query ?? "").Trim();

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        try
        {
            return await QueryListAsync(con, session.SchoolID, employeeType, jobStatus, search, true, cancellationToken);
        }
        catch (SqlException)
        {
            try
            {
                return await QueryListAsync(con, session.SchoolID, employeeType, jobStatus, search, false, cancellationToken);
            }
            catch (SqlException)
            {
                return await QueryUnionAsync(con, session.SchoolID, employeeType, jobStatus, search, cancellationToken);
            }
        }
    }

    public async Task<EmployeeResult> CreateTeacherAsync(
        SessionSnapshot session, CreateTeacherRequest? request, CancellationToken cancellationToken)
    {
        request ??= new CreateTeacherRequest();
        var firstName = (request.FirstName ?? "").Trim();
        var lastName = (request.LastName ?? "").Trim();
        var designation = (request.Designation ?? "").Trim();
        var phone = (request.Phone ?? "").Trim();
        var gender = (request.Gender ?? "").Trim();
        var userName = (request.UserName ?? "").Trim();
        var password = request.Password ?? "";
        var confirm = request.ConfirmPassword ?? "";
        var email = (request.Email ?? "").Trim();
        var question = (request.Question ?? "").Trim();
        var answer = (request.Answer ?? "").Trim();

        if (firstName.Length == 0 || lastName.Length == 0 || designation.Length == 0
            || phone.Length == 0 || gender.Length == 0)
            return Fail("emp.needInfo");
        if (request.AbsDeducted && request.AbsDeduction <= 0)
            return Fail("emp.needAbsAmount");
        if (userName.Length == 0 || password.Length == 0 || email.Length == 0
            || question.Length == 0 || answer.Length == 0)
            return Fail("emp.needLogin");
        if (question.StartsWith("Select ", StringComparison.OrdinalIgnoreCase))
            return Fail("sub.question");
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

            await AddToRoleAsync(con, tx, userName, "Teacher", cancellationToken);
            var registrationId = await InsertRegistrationAsync(con, tx, session.SchoolID, userName, cancellationToken);
            await InsertAstAsync(con, tx, registrationId, session.SchoolID, userName, password, answer, cancellationToken);
            await InsertEducationYearAsync(con, tx, registrationId, session.SchoolID, session.EducationYearID, cancellationToken);

            var sn = await NextTeacherSnAsync(con, tx, session.SchoolID, cancellationToken);
            await using var teacherCmd = new SqlCommand("""
INSERT INTO dbo.Teacher
    (TeacherRegistrationID, RegistrationID, SchoolID, FirstName, LastName, Date, T_SN,
     Designation, FatherName, Gender, Age, DateofBirth, Religion, Nationality,
     Address, City, PostalCode, State, Phone, Email)
VALUES
    (@TeacherRegistrationID, @RegistrationID, @SchoolID, @FirstName, @LastName, GETDATE(), @T_SN,
     @Designation, @FatherName, @Gender, @Age, @DateofBirth, @Religion, @Nationality,
     @Address, @City, @PostalCode, @State, @Phone, @Email);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
            teacherCmd.Parameters.AddWithValue("@TeacherRegistrationID", registrationId);
            teacherCmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            teacherCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            teacherCmd.Parameters.AddWithValue("@FirstName", firstName);
            teacherCmd.Parameters.AddWithValue("@LastName", lastName);
            teacherCmd.Parameters.AddWithValue("@T_SN", sn);
            teacherCmd.Parameters.AddWithValue("@Designation", designation);
            Add(teacherCmd, "@FatherName", request.FatherName);
            teacherCmd.Parameters.AddWithValue("@Gender", gender);
            Add(teacherCmd, "@Age", request.Age);
            Add(teacherCmd, "@DateofBirth", request.DateOfBirth?.Date);
            Add(teacherCmd, "@Religion", request.Religion);
            Add(teacherCmd, "@Nationality", string.IsNullOrWhiteSpace(request.Nationality) ? "BANGLADESHI" : request.Nationality);
            Add(teacherCmd, "@Address", request.Address);
            Add(teacherCmd, "@City", request.City);
            Add(teacherCmd, "@PostalCode", request.PostalCode);
            Add(teacherCmd, "@State", request.State);
            teacherCmd.Parameters.AddWithValue("@Phone", phone);
            teacherCmd.Parameters.AddWithValue("@Email", email);
            var teacherId = Convert.ToInt32(await teacherCmd.ExecuteScalarAsync(cancellationToken));

            var empCode = await EmployeeCodeAsync(con, tx, "dbo.Employee_Teacher_ID", teacherId, "T", cancellationToken);
            var employeeId = await InsertEmployeeInfoAsync(
                con, tx, session, empCode, "Teacher", request.PermanentTemporary,
                request.Salary, request.AbsDeducted, request.AbsDeduction,
                request.LateCountAsAbs, request.LateDays, cancellationToken);

            await using var linkCmd = new SqlCommand("""
UPDATE dbo.Teacher SET EmployeeID = @EmployeeID
WHERE TeacherID = @TeacherID AND SchoolID = @SchoolID
""", con, tx);
            linkCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            linkCmd.Parameters.AddWithValue("@TeacherID", teacherId);
            linkCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await linkCmd.ExecuteNonQueryAsync(cancellationToken);

            await MarkDeviceUpdateAsync(con, tx, session, "Add new Teacher", cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return new EmployeeResult
            {
                Succeeded = true,
                EmployeeID = employeeId,
                PersonID = teacherId,
                UserName = userName
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    public async Task<EmployeeResult> CreateStaffAsync(
        SessionSnapshot session, CreateStaffRequest? request, CancellationToken cancellationToken)
    {
        request ??= new CreateStaffRequest();
        var firstName = (request.FirstName ?? "").Trim();
        var lastName = (request.LastName ?? "").Trim();
        var designation = (request.Designation ?? "").Trim();
        var gender = (request.Gender ?? "").Trim();
        if (firstName.Length == 0 || lastName.Length == 0 || designation.Length == 0 || gender.Length == 0)
            return Fail("emp.needInfo");
        if (request.AbsDeducted && request.AbsDeduction <= 0)
            return Fail("emp.needAbsAmount");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var sn = await NextStaffSnAsync(con, tx, session.SchoolID, cancellationToken);
            await using var staffCmd = new SqlCommand("""
INSERT INTO dbo.Staff_Info
    (SchoolID, RegistrationID, FirstName, LastName, Gender, FatherName, Designation,
     Address, Phone, Staff_SN, Religion)
VALUES
    (@SchoolID, @RegistrationID, @FirstName, @LastName, @Gender, @FatherName, @Designation,
     @Address, @Phone, @Staff_SN, @Religion);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
            staffCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            staffCmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            staffCmd.Parameters.AddWithValue("@FirstName", firstName);
            staffCmd.Parameters.AddWithValue("@LastName", lastName);
            staffCmd.Parameters.AddWithValue("@Gender", gender);
            Add(staffCmd, "@FatherName", request.FatherName);
            staffCmd.Parameters.AddWithValue("@Designation", designation);
            Add(staffCmd, "@Address", request.Address);
            Add(staffCmd, "@Phone", request.Phone);
            staffCmd.Parameters.AddWithValue("@Staff_SN", sn);
            Add(staffCmd, "@Religion", request.Religion);
            var staffId = Convert.ToInt32(await staffCmd.ExecuteScalarAsync(cancellationToken));

            var empCode = await EmployeeCodeAsync(con, tx, "dbo.Employee_Staff_ID", staffId, "S", cancellationToken);
            var employeeId = await InsertEmployeeInfoAsync(
                con, tx, session, empCode, "Staff", request.PermanentTemporary,
                request.Salary, request.AbsDeducted, request.AbsDeduction,
                request.LateCountAsAbs, request.LateDays, cancellationToken);

            await using var linkCmd = new SqlCommand("""
UPDATE dbo.Staff_Info SET EmployeeID = @EmployeeID
WHERE StaffID = @StaffID AND SchoolID = @SchoolID
""", con, tx);
            linkCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
            linkCmd.Parameters.AddWithValue("@StaffID", staffId);
            linkCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await linkCmd.ExecuteNonQueryAsync(cancellationToken);

            await MarkDeviceUpdateAsync(con, tx, session, "Add new Staff", cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return new EmployeeResult
            {
                Succeeded = true,
                EmployeeID = employeeId,
                PersonID = staffId
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    public async Task<EmployeeResult> UpdateAsync(
        SessionSnapshot session, int employeeId, UpdateEmployeeRequest? request, CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
            return Fail("emp.needId");
        request ??= new UpdateEmployeeRequest();

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsEmployeeAsync(con, session.SchoolID, employeeId, cancellationToken))
            return Fail("emp.needId");

        var code = (request.ID ?? "").Trim();
        if (code.Length > 0)
        {
            await using var exists = new SqlCommand("""
SELECT 1 FROM dbo.Employee_Info
WHERE SchoolID = @SchoolID AND ID = @ID AND EmployeeID <> @EmployeeID
""", con);
            exists.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            exists.Parameters.AddWithValue("@ID", code);
            exists.Parameters.AddWithValue("@EmployeeID", employeeId);
            if (await exists.ExecuteScalarAsync(cancellationToken) is not null and not DBNull)
                return Fail("emp.idExists");
        }

        await using var cmd = new SqlCommand("""
UPDATE dbo.Employee_Info
SET ID = CASE WHEN @ID = N'' THEN ID ELSE @ID END,
    EmployeeType = CASE WHEN @EmployeeType = N'' THEN EmployeeType ELSE @EmployeeType END,
    Salary = CASE WHEN @HasSalary = 1 THEN @Salary ELSE Salary END,
    Bank_AccNo = CASE WHEN @HasBank = 1 THEN @BankAccNo ELSE Bank_AccNo END
WHERE EmployeeID = @EmployeeID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@ID", code);
        cmd.Parameters.AddWithValue("@EmployeeType", (request.EmployeeType ?? "").Trim());
        cmd.Parameters.AddWithValue("@HasSalary", request.Salary.HasValue ? 1 : 0);
        cmd.Parameters.AddWithValue("@Salary", request.Salary ?? 0);
        cmd.Parameters.AddWithValue("@HasBank", request.BankAccNo is not null ? 1 : 0);
        Add(cmd, "@BankAccNo", request.BankAccNo);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (code.Length > 0)
            await MarkDeviceUpdateAsync(con, null, session, "Update Employee ID", cancellationToken);
        return new EmployeeResult { Succeeded = true, EmployeeID = employeeId };
    }

    public async Task<EmployeeResult> SetJobStatusAsync(
        SessionSnapshot session, int employeeId, SetJobStatusRequest? request, CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
            return Fail("emp.needId");
        var status = string.Equals(request?.JobStatus, "Deactivate", StringComparison.OrdinalIgnoreCase)
            ? "Deactivate"
            : "Active";

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Employee_Info
SET Job_Status = @JobStatus
WHERE EmployeeID = @EmployeeID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@JobStatus", status);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return n > 0
            ? new EmployeeResult { Succeeded = true, EmployeeID = employeeId }
            : Fail("emp.needId");
    }

    public async Task<IReadOnlyList<TeacherAccountDto>> ListTeacherAccountsAsync(
        SessionSnapshot session, string? query, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT Teacher.TeacherID,
       ISNULL(Employee_Info.ID, N'') AS ID,
       LTRIM(RTRIM(ISNULL(Teacher.FirstName, N'') + N' ' + ISNULL(Teacher.LastName, N''))) AS Name,
       ISNULL(Teacher.Designation, N'') AS Designation,
       Teacher.Phone,
       aspnet_Membership.Email,
       Registration.UserName,
       ISNULL(AST.Password, N'') AS Password,
       ISNULL(aspnet_Membership.IsApproved, 0) AS IsApproved,
       ISNULL(aspnet_Membership.IsLockedOut, 0) AS IsLockedOut,
       aspnet_Membership.LastLoginDate
FROM dbo.Teacher
INNER JOIN dbo.Registration ON Teacher.TeacherRegistrationID = Registration.RegistrationID
INNER JOIN dbo.Employee_Info ON Teacher.EmployeeID = Employee_Info.EmployeeID
LEFT OUTER JOIN dbo.AST ON Registration.UserName = AST.UserName
LEFT OUTER JOIN dbo.aspnet_Users ON Registration.UserName = aspnet_Users.UserName
LEFT OUTER JOIN dbo.aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId
WHERE Registration.Category = N'Teacher'
  AND Registration.SchoolID = @SchoolID
  AND Registration.Validation = N'Valid'
ORDER BY Employee_Info.ID, Teacher.TeacherID
""";
        var search = (query ?? "").Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<TeacherAccountDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new TeacherAccountDto
            {
                TeacherID = Convert.ToInt32(reader["TeacherID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                Phone = NullString(reader["Phone"]),
                Email = NullString(reader["Email"]),
                UserName = reader["UserName"]?.ToString() ?? "",
                Password = reader["Password"]?.ToString() ?? "",
                IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"]),
                LastLoginDate = ReadDate(reader["LastLoginDate"])
            };
            if (search.Length == 0
                || Contains(row.Name, search)
                || Contains(row.ID, search)
                || Contains(row.Designation, search)
                || Contains(row.Phone, search)
                || Contains(row.UserName, search))
                items.Add(row);
        }

        return items;
    }

    public async Task<TeacherAccountResult> SetApprovedAsync(
        SessionSnapshot session, SetTeacherApprovedRequest? request, CancellationToken cancellationToken)
    {
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0)
            return AccountFail("emp.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var userId = await FindTeacherUserIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (userId is null)
            return AccountFail("emp.needUser");

        await using var cmd = new SqlCommand("""
UPDATE dbo.aspnet_Membership
SET IsApproved = @IsApproved
WHERE UserId = @UserId
""", con);
        cmd.Parameters.AddWithValue("@IsApproved", request!.IsApproved);
        cmd.Parameters.AddWithValue("@UserId", userId.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return await ReadAccountStatusAsync(con, userId.Value, cancellationToken);
    }

    public async Task<TeacherAccountResult> UnlockAsync(
        SessionSnapshot session, UnlockTeacherRequest? request, CancellationToken cancellationToken)
    {
        var userName = (request?.UserName ?? "").Trim();
        if (userName.Length == 0)
            return AccountFail("emp.needUser");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var userId = await FindTeacherUserIdAsync(con, session.SchoolID, userName, cancellationToken);
        if (userId is null)
            return AccountFail("emp.needUser");

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
        return await ReadAccountStatusAsync(con, userId.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<TeacherPickDto>> ListActiveTeachersAsync(
        SessionSnapshot session, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT Teacher.TeacherID,
       LTRIM(RTRIM(ISNULL(Teacher.FirstName, N'') + N' ' + ISNULL(Teacher.LastName, N''))) AS Name
FROM dbo.Teacher
INNER JOIN dbo.Employee_Info ON Teacher.EmployeeID = Employee_Info.EmployeeID
WHERE Teacher.SchoolID = @SchoolID
  AND Employee_Info.Job_Status = N'Active'
ORDER BY Name
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var items = new List<TeacherPickDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TeacherPickDto
            {
                TeacherID = Convert.ToInt32(reader["TeacherID"]),
                Name = reader["Name"]?.ToString() ?? ""
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<TeacherSubjectRowDto>> ListTeacherSubjectsAsync(
        SessionSnapshot session, int teacherId, int classId, CancellationToken cancellationToken)
    {
        if (teacherId <= 0 || classId <= 0)
            return [];

        const string sql = """
SELECT DISTINCT Subject.SubjectID, Subject.SubjectName, Subject.SN,
       CAST(CASE WHEN T_S.SubjectID IS NULL THEN 0 ELSE 1 END AS BIT) AS Assigned
FROM dbo.Subject
INNER JOIN dbo.SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID
LEFT OUTER JOIN (
    SELECT SubjectID
    FROM dbo.TecherSubject
    WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID AND ClassID = @ClassID
) AS T_S ON Subject.SubjectID = T_S.SubjectID
WHERE Subject.SchoolID = @SchoolID
  AND SubjectForGroup.ClassID = @ClassID
ORDER BY Subject.SN, Subject.SubjectName
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@TeacherID", teacherId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        var items = new List<TeacherSubjectRowDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TeacherSubjectRowDto
            {
                SubjectID = Convert.ToInt32(reader["SubjectID"]),
                SubjectName = reader["SubjectName"]?.ToString() ?? "",
                Assigned = Convert.ToBoolean(reader["Assigned"])
            });
        }

        return items;
    }

    public async Task<EmployeeResult> ToggleTeacherSubjectAsync(
        SessionSnapshot session, int teacherId, ToggleTeacherSubjectRequest? request, CancellationToken cancellationToken)
    {
        request ??= new ToggleTeacherSubjectRequest();
        if (teacherId <= 0)
            return Fail("emp.needTeacher");
        if (request.ClassID <= 0)
            return Fail("emp.needClass");
        if (request.SubjectID <= 0)
            return Fail("emp.needSubject");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (request.Assigned)
        {
            await using var cmd = new SqlCommand("""
IF NOT EXISTS (
    SELECT 1 FROM dbo.TecherSubject
    WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID AND ClassID = @ClassID AND SubjectID = @SubjectID)
INSERT INTO dbo.TecherSubject (SchoolID, RegistrationID, TeacherID, SubjectID, date, ClassID)
VALUES (@SchoolID, @RegistrationID, @TeacherID, @SubjectID, GETDATE(), @ClassID)
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
            cmd.Parameters.AddWithValue("@TeacherID", teacherId);
            cmd.Parameters.AddWithValue("@SubjectID", request.SubjectID);
            cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            await using var cmd = new SqlCommand("""
DELETE FROM dbo.TecherSubject
WHERE SchoolID = @SchoolID AND TeacherID = @TeacherID AND ClassID = @ClassID AND SubjectID = @SubjectID
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@TeacherID", teacherId);
            cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
            cmd.Parameters.AddWithValue("@SubjectID", request.SubjectID);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return new EmployeeResult { Succeeded = true, PersonID = teacherId };
    }

    public async Task<IReadOnlyList<EmployeeIdCardDto>> ListIdCardsAsync(
        SessionSnapshot session, string? type, string? query, CancellationToken cancellationToken)
    {
        var employeeType = NormalizeType(type);
        var search = (query ?? "").Trim();
        const string sql = """
SELECT v.EmployeeID, v.ID, v.EmployeeType,
       LTRIM(RTRIM(ISNULL(v.FirstName, N'') + N' ' + ISNULL(v.LastName, N''))) AS Name,
       ISNULL(v.Designation, N'') AS Designation,
       v.Phone, t.Email, t.DateofBirth,
       s.SchoolName, s.Address, s.Institution_Dialog, s.SchoolLogo,
       CASE WHEN v.EmployeeType = N'Teacher' THEN t.Image ELSE st.Image END AS Photo
FROM dbo.VW_Emp_Info AS v
INNER JOIN dbo.SchoolInfo AS s ON s.SchoolID = v.SchoolID
LEFT JOIN dbo.Teacher AS t ON t.EmployeeID = v.EmployeeID AND v.EmployeeType = N'Teacher'
LEFT JOIN dbo.Staff_Info AS st ON st.EmployeeID = v.EmployeeID AND v.EmployeeType = N'Staff'
WHERE v.SchoolID = @SchoolID
  AND v.Job_Status = N'Active'
  AND (v.EmployeeType LIKE @EmployeeType)
ORDER BY v.ID
""";
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        List<EmployeeIdCardDto> items;
        try
        {
            items = await ReadIdCardsAsync(con, sql, session.SchoolID, employeeType, cancellationToken);
        }
        catch (SqlException)
        {
            items = await ReadIdCardsAsync(con, """
SELECT e.EmployeeID, e.ID, e.EmployeeType,
       LTRIM(RTRIM(ISNULL(p.FirstName, N'') + N' ' + ISNULL(p.LastName, N''))) AS Name,
       ISNULL(p.Designation, N'') AS Designation,
       p.Phone,
       s.SchoolName, s.Address, s.Institution_Dialog, s.SchoolLogo,
       p.Image AS Photo
FROM dbo.Employee_Info AS e
INNER JOIN (
    SELECT EmployeeID, FirstName, LastName, Designation, Phone, Image, SchoolID
    FROM dbo.Teacher
    UNION ALL
    SELECT EmployeeID, FirstName, LastName, Designation, Phone, Image, SchoolID
    FROM dbo.Staff_Info
) AS p ON p.EmployeeID = e.EmployeeID
INNER JOIN dbo.SchoolInfo AS s ON s.SchoolID = e.SchoolID
WHERE e.SchoolID = @SchoolID
  AND e.Job_Status = N'Active'
  AND (e.EmployeeType LIKE @EmployeeType)
ORDER BY e.ID
""", session.SchoolID, employeeType, cancellationToken);
        }

        if (search.Length == 0)
            return items;
        return items.Where(x =>
            Contains(x.Name, search)
            || Contains(x.ID, search)
            || Contains(x.Designation, search)
            || Contains(x.Phone, search)).ToList();
    }

    private static async Task<List<EmployeeIdCardDto>> ReadIdCardsAsync(
        SqlConnection con, string sql, int schoolId, string employeeType, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@EmployeeType", employeeType);
        var items = new List<EmployeeIdCardDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var hasEmail = HasColumn(reader, "Email");
        var hasDob = HasColumn(reader, "DateofBirth");
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EmployeeIdCardDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                Designation = reader["Designation"]?.ToString() ?? "",
                Phone = NullString(reader["Phone"]),
                Email = hasEmail ? NullString(reader["Email"]) : null,
                DateofBirth = hasDob ? ReadDate(reader["DateofBirth"]) : null,
                SchoolName = reader["SchoolName"]?.ToString() ?? "",
                Address = NullString(reader["Address"]),
                InstitutionDialog = NullString(reader["Institution_Dialog"]),
                LogoDataUrl = ToDataUrl(reader["SchoolLogo"] as byte[]),
                PhotoDataUrl = ToDataUrl(reader["Photo"] as byte[])
            });
        }

        return items;
    }

    private static async Task<List<EmployeeListDto>> QueryListAsync(
        SqlConnection con, int schoolId, string employeeType, string jobStatus, string search,
        bool withSubCategory, CancellationToken cancellationToken)
    {
        var subCols = withSubCategory
            ? "ISNULL(SubCategoryID, 0) AS SubCategoryID, ISNULL(SubCategoryName, N'') AS SubCategoryName"
            : "0 AS SubCategoryID, N'' AS SubCategoryName";
        var sql = $"""
SELECT EmployeeID, ID, Bank_AccNo, EmployeeType, Permanent_Temporary, Salary,
       LTRIM(RTRIM(ISNULL(FirstName, N'') + N' ' + ISNULL(LastName, N''))) AS Name,
       FatherName, Designation, Phone, DeviceID, Job_Status,
       {subCols}
FROM dbo.VW_Emp_Info
WHERE SchoolID = @SchoolID
  AND (@Status = N'%' OR Job_Status = @Status)
  AND (EmployeeType LIKE @EmployeeType)
ORDER BY ID
""";
        return await ReadListAsync(con, sql, schoolId, employeeType, jobStatus, search, cancellationToken);
    }

    private static async Task<List<EmployeeListDto>> QueryUnionAsync(
        SqlConnection con, int schoolId, string employeeType, string jobStatus, string search,
        CancellationToken cancellationToken)
    {
        const string sql = """
SELECT e.EmployeeID, e.ID, e.Bank_AccNo, e.EmployeeType, e.Permanent_Temporary, e.Salary,
       LTRIM(RTRIM(ISNULL(t.FirstName, N'') + N' ' + ISNULL(t.LastName, N''))) AS Name,
       t.FatherName, t.Designation, t.Phone, e.DeviceID, e.Job_Status,
       0 AS SubCategoryID, N'' AS SubCategoryName
FROM dbo.Employee_Info AS e
INNER JOIN dbo.Teacher AS t ON t.EmployeeID = e.EmployeeID
WHERE e.SchoolID = @SchoolID AND e.EmployeeType = N'Teacher'
  AND (@Status = N'%' OR e.Job_Status = @Status)
  AND (e.EmployeeType LIKE @EmployeeType)
UNION ALL
SELECT e.EmployeeID, e.ID, e.Bank_AccNo, e.EmployeeType, e.Permanent_Temporary, e.Salary,
       LTRIM(RTRIM(ISNULL(s.FirstName, N'') + N' ' + ISNULL(s.LastName, N''))) AS Name,
       s.FatherName, s.Designation, s.Phone, e.DeviceID, e.Job_Status,
       0 AS SubCategoryID, N'' AS SubCategoryName
FROM dbo.Employee_Info AS e
INNER JOIN dbo.Staff_Info AS s ON s.EmployeeID = e.EmployeeID
WHERE e.SchoolID = @SchoolID AND e.EmployeeType = N'Staff'
  AND (@Status = N'%' OR e.Job_Status = @Status)
  AND (e.EmployeeType LIKE @EmployeeType)
ORDER BY ID
""";
        return await ReadListAsync(con, sql, schoolId, employeeType, jobStatus, search, cancellationToken);
    }

    private static async Task<List<EmployeeListDto>> ReadListAsync(
        SqlConnection con, string sql, int schoolId, string employeeType, string jobStatus, string search,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Status", jobStatus);
        cmd.Parameters.AddWithValue("@EmployeeType", employeeType);
        var items = new List<EmployeeListDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new EmployeeListDto
            {
                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                ID = reader["ID"]?.ToString() ?? "",
                Name = reader["Name"]?.ToString() ?? "",
                FatherName = NullString(reader["FatherName"]),
                Phone = NullString(reader["Phone"]),
                Designation = reader["Designation"]?.ToString() ?? "",
                EmployeeType = reader["EmployeeType"]?.ToString() ?? "",
                JobStatus = reader["Job_Status"]?.ToString() ?? "",
                PermanentTemporary = NullString(reader["Permanent_Temporary"]),
                Salary = reader["Salary"] is DBNull ? 0 : Convert.ToDecimal(reader["Salary"]),
                BankAccNo = NullString(reader["Bank_AccNo"]),
                DeviceID = NullString(reader["DeviceID"]),
                SubCategoryName = NullString(reader["SubCategoryName"])
            };
            if (search.Length == 0
                || Contains(row.ID, search)
                || Contains(row.Name, search)
                || Contains(row.Designation, search)
                || Contains(row.Phone, search))
                items.Add(row);
        }

        return items;
    }

    private static async Task<int> InsertEmployeeInfoAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, string id, string employeeType,
        string? permanentTemporary, decimal salary, bool absDeducted, decimal absDeduction,
        bool lateCountAsAbs, int? lateDays, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Employee_Info
    (SchoolID, RegistrationID, ID, EmployeeType, Permanent_Temporary, Work_Time_Basis, Time_Basis_Type,
     Salary, IS_Abs_Deducted, Abs_Deduction, IS_Late_Count_As_Abs, Late_Days, Job_Status)
VALUES
    (@SchoolID, @RegistrationID, @ID, @EmployeeType, @Permanent_Temporary, N'Time Basis', N'Monthly',
     @Salary, @IS_Abs_Deducted, @Abs_Deduction, @IS_Late_Count_As_Abs, @Late_Days, N'Active');
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@ID", id);
        cmd.Parameters.AddWithValue("@EmployeeType", employeeType);
        cmd.Parameters.AddWithValue("@Permanent_Temporary",
            string.Equals(permanentTemporary, "Temporary", StringComparison.OrdinalIgnoreCase) ? "Temporary" : "Permanent");
        cmd.Parameters.AddWithValue("@Salary", salary);
        cmd.Parameters.AddWithValue("@IS_Abs_Deducted", absDeducted);
        cmd.Parameters.AddWithValue("@Abs_Deduction", absDeducted ? absDeduction : 0);
        cmd.Parameters.AddWithValue("@IS_Late_Count_As_Abs", lateCountAsAbs);
        Add(cmd, "@Late_Days", lateCountAsAbs ? lateDays : null);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task MarkDeviceUpdateAsync(
        SqlConnection con, SqlTransaction? tx, SessionSnapshot session, string description, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.Attendance_Device_DataUpdateList', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Attendance_Device_DataUpdateList
        WHERE SchoolID = @SchoolID AND UpdateType = N'New Employee')
    INSERT INTO dbo.Attendance_Device_DataUpdateList (SchoolID, RegistrationID, UpdateType, UpdateDescription)
    VALUES (@SchoolID, @RegistrationID, N'New Employee', @UpdateDescription)
END
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@UpdateDescription", description);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> NextTeacherSnAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("SELECT dbo.Teacher_SerialNumber(@SchoolID)", con, tx);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            if (value is not null and not DBNull)
                return ToInt(value, 1);
        }
        catch (SqlException)
        {
        }

        await using var fallback = new SqlCommand(
            "SELECT ISNULL(MAX(T_SN), 0) + 1 FROM dbo.Teacher WHERE SchoolID = @SchoolID", con, tx);
        fallback.Parameters.AddWithValue("@SchoolID", schoolId);
        return ToInt(await fallback.ExecuteScalarAsync(cancellationToken), 1);
    }

    private static async Task<int> NextStaffSnAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("SELECT dbo.Staff_SerialNumber(@SchoolID)", con, tx);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            if (value is not null and not DBNull)
                return ToInt(value, 1);
        }
        catch (SqlException)
        {
        }

        await using var fallback = new SqlCommand(
            "SELECT ISNULL(MAX(Staff_SN), 0) + 1 FROM dbo.Staff_Info WHERE SchoolID = @SchoolID", con, tx);
        fallback.Parameters.AddWithValue("@SchoolID", schoolId);
        return ToInt(await fallback.ExecuteScalarAsync(cancellationToken), 1);
    }

    private static async Task<string> EmployeeCodeAsync(
        SqlConnection con, SqlTransaction tx, string functionName, int personId, string prefix, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand($"SELECT {functionName}(@ID)", con, tx);
            cmd.Parameters.AddWithValue("@ID", personId);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            if (value is not null and not DBNull)
            {
                var text = Convert.ToString(value)?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }
        catch (SqlException)
        {
        }

        return prefix + personId;
    }

    private static async Task<bool> OwnsEmployeeAsync(
        SqlConnection con, int schoolId, int employeeId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "SELECT 1 FROM dbo.Employee_Info WHERE EmployeeID = @EmployeeID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        return await cmd.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
    }

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
        return await cmd.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
    }

    private static async Task<EmployeeResult> CreateMembershipUserAsync(
        SqlConnection con, SqlTransaction tx, string userName, string password, string email,
        string question, string answer, CancellationToken cancellationToken)
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
            return new EmployeeResult { Succeeded = true, UserName = userName };
        }

        var code = returnParam.Value is int i ? i : Convert.ToInt32(returnParam.Value ?? 0);
        return code switch
        {
            0 => new EmployeeResult { Succeeded = true, UserName = userName },
            6 => Fail("sub.userExists"),
            7 => Fail("sub.emailExists"),
            _ => Fail("emp.failed")
        };
    }

    private static async Task InsertMembershipManuallyAsync(
        SqlConnection con, SqlTransaction tx, Guid userId, string userName, string hashedPassword,
        string salt, string email, string question, string hashedAnswer, DateTime utcNow,
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
VALUES (@SchoolID, @UserName, N'Valid', N'Teacher', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task InsertAstAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, string userName,
        string password, string answer, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.AST', N'U') IS NOT NULL
INSERT INTO dbo.AST (RegistrationID, SchoolID, UserName, Category, Password, PasswordAnswer)
VALUES (@RegistrationID, @SchoolID, @UserName, N'Teacher', @Password, @PasswordAnswer)
""", con, tx);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@Password", password);
        cmd.Parameters.AddWithValue("@PasswordAnswer", answer);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertEducationYearAsync(
        SqlConnection con, SqlTransaction tx, int registrationId, int schoolId, int educationYearId,
        CancellationToken cancellationToken)
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

    private static async Task<Guid?> FindTeacherUserIdAsync(
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
  AND r.Category = N'Teacher'
""", con);
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is Guid id ? id : value is null or DBNull ? null : (Guid?)value;
    }

    private static async Task<TeacherAccountResult> ReadAccountStatusAsync(
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
            return AccountFail("emp.needUser");

        return new TeacherAccountResult
        {
            Succeeded = true,
            IsApproved = Convert.ToBoolean(reader["IsApproved"]),
            IsLockedOut = Convert.ToBoolean(reader["IsLockedOut"])
        };
    }

    private static string NormalizeType(string? type)
    {
        type = (type ?? "").Trim();
        if (string.Equals(type, "Teacher", StringComparison.OrdinalIgnoreCase))
            return "Teacher";
        if (string.Equals(type, "Staff", StringComparison.OrdinalIgnoreCase))
            return "Staff";
        return "%";
    }

    private static string NormalizeStatus(string? status)
    {
        status = (status ?? "").Trim();
        if (string.Equals(status, "Deactivate", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Deactivated", StringComparison.OrdinalIgnoreCase))
            return "Deactivate";
        if (string.Equals(status, "%", StringComparison.Ordinal) || string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            return "%";
        return "Active";
    }

    private static void Add(SqlCommand cmd, string name, object? value)
    {
        cmd.Parameters.AddWithValue(name, value is null or "" ? DBNull.Value : value);
    }

    private static bool HasColumn(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string? NullString(object value)
    {
        var text = value is DBNull ? null : value?.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static DateTime? ReadDate(object value)
    {
        if (value is null or DBNull)
            return null;
        var date = Convert.ToDateTime(value);
        return date.Year < 1900 ? null : date;
    }

    private static bool Contains(string? source, string search) =>
        !string.IsNullOrWhiteSpace(source)
        && source.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static string? ToDataUrl(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return null;
        var mime = bytes.Length >= 8 && bytes[0] == 0x89 ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    private static int ToInt(object? value, int fallback)
    {
        if (value is null or DBNull)
            return fallback;
        if (value is int i)
            return i;
        if (value is long l)
            return (int)l;
        if (value is decimal d)
            return (int)d;
        return int.TryParse(Convert.ToString(value), out var n) ? n : fallback;
    }

    private static EmployeeResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static TeacherAccountResult AccountFail(string error) =>
        new() { Succeeded = false, Error = error };
}
