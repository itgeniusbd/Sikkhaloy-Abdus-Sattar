using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Institution;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class WebsitePublicService
{
    private static readonly Regex MobileRx = new(@"^01[3-9]\d{8}$", RegexOptions.CultureInvariant);

    public async Task<WebsiteAdmissionFormDto?> GetAdmissionFormAsync(int schoolId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0)
            return null;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await ReadSchoolAsync(con, schoolId, cancellationToken) is null)
            return null;

        var year = await ReadActiveYearAsync(con, schoolId, cancellationToken);
        return new WebsiteAdmissionFormDto
        {
            LastStudentId = await ReadLastStudentCodeAsync(con, schoolId, cancellationToken),
            ActiveEducationYearId = year.Id,
            Sessions = await ReadLookupsAsync(con, """
SELECT EducationYearID AS ItemID, EducationYear AS ItemName
FROM dbo.Education_Year
WHERE SchoolID = @SchoolID
ORDER BY SN DESC
""", schoolId, cancellationToken),
            Classes = await ReadLookupsAsync(con, """
SELECT ClassID AS ItemID, Class AS ItemName
FROM dbo.CreateClass
WHERE SchoolID = @SchoolID
ORDER BY SN
""", schoolId, cancellationToken),
            FeeRoles = await ReadFeeRolesAsync(con, schoolId, cancellationToken)
        };
    }

    public async Task<WebsiteAdmissionOptionsDto?> GetAdmissionOptionsAsync(
        int schoolId, int classId, int educationYearId, int groupId, int sectionId, int shiftId, CancellationToken cancellationToken)
    {
        if (schoolId <= 0 || classId <= 0)
            return null;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await ReadSchoolAsync(con, schoolId, cancellationToken) is null)
            return null;

        var year = educationYearId > 0 ? educationYearId : (await ReadActiveYearAsync(con, schoolId, cancellationToken)).Id;
        return new WebsiteAdmissionOptionsDto
        {
            LastRollNo = await ReadLastRollAsync(con, schoolId, classId, year, sectionId, cancellationToken),
            Groups = await ReadClassLookupsAsync(con, """
SELECT DISTINCT j.SubjectGroupID AS ItemID, g.SubjectGroup AS ItemName
FROM dbo.[Join] AS j
INNER JOIN dbo.CreateSubjectGroup AS g ON g.SubjectGroupID = j.SubjectGroupID
WHERE j.ClassID = @ClassID
ORDER BY g.SubjectGroup
""", classId, 0, 0, cancellationToken),
            Sections = await ReadClassLookupsAsync(con, """
SELECT DISTINCT j.SectionID AS ItemID, s.Section AS ItemName
FROM dbo.[Join] AS j
INNER JOIN dbo.CreateSection AS s ON s.SectionID = j.SectionID
WHERE j.ClassID = @ClassID
  AND (@GroupID = 0 OR j.SubjectGroupID = @GroupID)
ORDER BY s.Section
""", classId, groupId, 0, cancellationToken),
            Shifts = await ReadClassLookupsAsync(con, """
SELECT DISTINCT j.ShiftID AS ItemID, sh.Shift AS ItemName
FROM dbo.[Join] AS j
INNER JOIN dbo.CreateShift AS sh ON sh.ShiftID = j.ShiftID
WHERE j.ClassID = @ClassID
  AND (@GroupID = 0 OR j.SubjectGroupID = @GroupID)
  AND (@SectionID = 0 OR j.SectionID = @SectionID)
ORDER BY sh.Shift
""", classId, groupId, sectionId, cancellationToken),
            Subjects = await ReadSubjectsAsync(con, schoolId, classId, groupId, cancellationToken),
            Fees = await ReadSuggestedFeesAsync(con, schoolId, classId, year, cancellationToken)
        };
    }

    public async Task<WebsiteAdmitResult> AdmitStudentAsync(
        int schoolId, WebsiteAdmitRequest request, CancellationToken cancellationToken)
    {
        var name = (request.StudentsName ?? "").Trim();
        var phone = (request.SMSPhoneNo ?? "").Trim();
        var father = (request.FathersName ?? "").Trim();
        if (name.Length == 0)
            return Fail("Student name is required.");
        if (!MobileRx.IsMatch(phone))
            return Fail("SMS mobile must be 11 digits (01XXXXXXXXX).");
        if (father.Length == 0)
            return Fail("Father's name is required.");
        if (request.EducationYearID <= 0)
            return Fail("Session year is required.");
        if (request.ClassID <= 0)
            return Fail("Class is required.");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var school = await ReadSchoolAsync(con, schoolId, cancellationToken);
        if (school is null)
            return Fail("Institution was not found.");

        var registrationId = await ReadAdminRegistrationAsync(con, schoolId, cancellationToken);
        if (registrationId <= 0)
            return Fail("No valid Admin registration was found for this school.");

        var className = await ReadClassNameAsync(con, schoolId, request.ClassID, cancellationToken);

        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var lastId = await ReadLastStudentCodeAsync(con, schoolId, cancellationToken, tx);
            var studentCode = await NextFreeStudentCodeAsync(con, schoolId, lastId, cancellationToken, tx);
            var imageId = await InsertStudentImageAsync(con, tx, request.PhotoDataUrl, cancellationToken);
            var dob = ParseDate(request.DateofBirth);
            var blood = BlankIfUnknown(request.BloodGroup);

            const string insertStudent = """
INSERT INTO dbo.Student
    (RegistrationID, SchoolID, StudentImageID, ID, SMSPhoneNo, StudentsName, Gender, DateofBirth,
     FathersName, MothersName, BloodGroup, Religion, Status, AdmissionDate,
     StudentEmailAddress, Legal_Identity, StudentsLocalAddress, StudentPermanentAddress,
     PrevSchoolName, PrevClass, PrevExamYear, PrevExamGrade,
     FatherOccupation, FatherPhoneNumber, MotherOccupation, MotherPhoneNumber,
     GuardianName, GuardianRelationshipwithStudent, GuardianPhoneNumber)
VALUES
    (@RegistrationID, @SchoolID, @StudentImageID, @ID, @SMSPhoneNo, @StudentsName, @Gender, @DateofBirth,
     @FathersName, @MothersName, @BloodGroup, @Religion, N'Active', GETDATE(),
     @StudentEmailAddress, @LegalIdentity, @StudentsLocalAddress, @StudentPermanentAddress,
     @PrevSchoolName, @PrevClass, @PrevExamYear, @PrevExamGrade,
     @FatherOccupation, @FatherPhoneNumber, @MotherOccupation, @MotherPhoneNumber,
     @GuardianName, @GuardianRelationship, @GuardianPhoneNumber);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""";
            int studentId;
            await using (var cmd = new SqlCommand(insertStudent, con, tx))
            {
                cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@StudentImageID", (object?)imageId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID", studentCode);
                cmd.Parameters.AddWithValue("@SMSPhoneNo", phone);
                cmd.Parameters.AddWithValue("@StudentsName", name);
                cmd.Parameters.AddWithValue("@Gender", string.IsNullOrWhiteSpace(request.Gender) ? "Male" : request.Gender.Trim());
                cmd.Parameters.AddWithValue("@DateofBirth", (object?)dob ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FathersName", father);
                cmd.Parameters.AddWithValue("@MothersName", Db(request.MothersName));
                cmd.Parameters.AddWithValue("@BloodGroup", Db(blood));
                cmd.Parameters.AddWithValue("@Religion", Db(request.Religion));
                cmd.Parameters.AddWithValue("@StudentEmailAddress", Db(request.StudentEmailAddress));
                cmd.Parameters.AddWithValue("@LegalIdentity", Db(request.LegalIdentity));
                cmd.Parameters.AddWithValue("@StudentsLocalAddress", Db(request.StudentsLocalAddress));
                cmd.Parameters.AddWithValue("@StudentPermanentAddress", Db(request.StudentPermanentAddress));
                cmd.Parameters.AddWithValue("@PrevSchoolName", Db(request.PrevSchoolName));
                cmd.Parameters.AddWithValue("@PrevClass", Db(request.PrevClass));
                cmd.Parameters.AddWithValue("@PrevExamYear", Db(request.PrevExamYear));
                cmd.Parameters.AddWithValue("@PrevExamGrade", Db(request.PrevExamGrade));
                cmd.Parameters.AddWithValue("@FatherOccupation", Db(request.FatherOccupation));
                cmd.Parameters.AddWithValue("@FatherPhoneNumber", Db(request.FatherPhoneNumber));
                cmd.Parameters.AddWithValue("@MotherOccupation", Db(request.MotherOccupation));
                cmd.Parameters.AddWithValue("@MotherPhoneNumber", Db(request.MotherPhoneNumber));
                cmd.Parameters.AddWithValue("@GuardianName", Db(request.GuardianName));
                cmd.Parameters.AddWithValue("@GuardianRelationship", Db(request.GuardianRelationshipwithStudent));
                cmd.Parameters.AddWithValue("@GuardianPhoneNumber", Db(request.GuardianPhoneNumber));
                studentId = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            }

            const string insertClass = """
INSERT INTO dbo.StudentsClass
    (SchoolID, RegistrationID, StudentID, ClassID, SectionID, ShiftID, SubjectGroupID, RollNo, EducationYearID, Date, Is_New)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, GETDATE(), 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""";
            int studentClassId;
            await using (var cmd = new SqlCommand(insertClass, con, tx))
            {
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@ClassID", request.ClassID);
                cmd.Parameters.AddWithValue("@SectionID", IdOrZero(request.SectionID));
                cmd.Parameters.AddWithValue("@ShiftID", IdOrZero(request.ShiftID));
                cmd.Parameters.AddWithValue("@SubjectGroupID", IdOrZero(request.SubjectGroupID));
                cmd.Parameters.AddWithValue("@RollNo", Db(request.RollNo));
                cmd.Parameters.AddWithValue("@EducationYearID", request.EducationYearID);
                studentClassId = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            }

            foreach (var subject in request.Subjects ?? [])
            {
                if (subject.SubjectID <= 0)
                    continue;
                var type = string.Equals(subject.SubjectType, "Optional", StringComparison.OrdinalIgnoreCase)
                    ? "Optional"
                    : "Compulsory";
                const string insertSubject = """
IF NOT EXISTS (
    SELECT 1 FROM dbo.StudentRecord
    WHERE StudentID = @StudentID AND SchoolID = @SchoolID
      AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND SubjectID = @SubjectID)
BEGIN
    INSERT INTO dbo.StudentRecord
        (SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType)
    VALUES
        (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)
END
""";
                await using var cmd = new SqlCommand(insertSubject, con, tx);
                cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@StudentClassID", studentClassId);
                cmd.Parameters.AddWithValue("@SubjectID", subject.SubjectID);
                cmd.Parameters.AddWithValue("@EducationYearID", request.EducationYearID);
                cmd.Parameters.AddWithValue("@SubjectType", type);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            var payments = await InsertPayOrdersAsync(
                con, tx, schoolId, registrationId, studentId, studentClassId,
                request.ClassID, request.EducationYearID, request.Payments, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            var smsSent = false;
            var smsLocal = false;
            string? smsError = null;
            if (request.SendSms)
            {
                (smsSent, smsLocal, smsError) = await TrySendAdmissionSmsAsync(
                    con, schoolId, request.EducationYearID, studentId, name, studentCode,
                    className, request.RollNo, phone, school.SchoolName, cancellationToken);
            }

            return new WebsiteAdmitResult
            {
                Succeeded = true,
                StudentID = studentId,
                StudentClassID = studentClassId,
                StudentCode = studentCode,
                LastStudentId = lastId,
                ClassName = className,
                RollNo = request.RollNo,
                SchoolName = school.SchoolName,
                SchoolAddress = school.Address,
                SchoolPhone = school.Phone,
                AdmissionDate = DateTime.Now,
                SmsSent = smsSent,
                SmsLocal = smsLocal,
                SmsError = smsError,
                Payments = payments
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    private static async Task<List<WebsiteLookupDto>> ReadLookupsAsync(
        SqlConnection con, string sql, int schoolId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        return await ReadLookupRowsAsync(cmd, cancellationToken);
    }

    private static async Task<List<WebsiteLookupDto>> ReadClassLookupsAsync(
        SqlConnection con, string sql, int classId, int groupId, int sectionId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            if (sql.Contains("@GroupID", StringComparison.Ordinal))
                cmd.Parameters.AddWithValue("@GroupID", groupId);
            if (sql.Contains("@SectionID", StringComparison.Ordinal))
                cmd.Parameters.AddWithValue("@SectionID", sectionId);
            return await ReadLookupRowsAsync(cmd, cancellationToken);
        }
        catch (SqlException)
        {
            return [];
        }
    }

    private static async Task<List<WebsiteLookupDto>> ReadLookupRowsAsync(SqlCommand cmd, CancellationToken cancellationToken)
    {
        var list = new List<WebsiteLookupDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = Convert.ToInt32(reader["ItemID"]);
            var name = reader["ItemName"]?.ToString()?.Trim() ?? "";
            if (id > 0 && name.Length > 0)
                list.Add(new WebsiteLookupDto { Id = id, Name = name });
        }
        return list;
    }

    private static async Task<List<WebsiteSubjectOptionDto>> ReadSubjectsAsync(
        SqlConnection con, int schoolId, int classId, int groupId, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
SELECT s.SubjectID, s.SubjectName, ISNULL(g.SubjectType, N'Compulsory') AS SubjectType
FROM dbo.Subject AS s
INNER JOIN dbo.SubjectForGroup AS g ON g.SubjectID = s.SubjectID
WHERE s.SchoolID = @SchoolID
  AND g.ClassID = @ClassID
  AND g.SubjectGroupID = @GroupID
ORDER BY g.SubjectType, s.SubjectName
""";
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@GroupID", groupId);
            var list = new List<WebsiteSubjectOptionDto>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var type = reader["SubjectType"]?.ToString()?.Trim() ?? "Compulsory";
                list.Add(new WebsiteSubjectOptionDto
                {
                    SubjectID = Convert.ToInt32(reader["SubjectID"]),
                    SubjectName = reader["SubjectName"]?.ToString()?.Trim() ?? "",
                    SubjectType = type,
                    Selected = string.Equals(type, "Compulsory", StringComparison.OrdinalIgnoreCase)
                });
            }
            return list;
        }
        catch (SqlException)
        {
            return [];
        }
    }

    private static async Task<string?> ReadLastStudentCodeAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken, SqlTransaction? tx = null)
    {
        const string sql = "SELECT TOP 1 ID FROM dbo.Student WHERE SchoolID = @SchoolID ORDER BY StudentID DESC";
        await using var cmd = tx is null ? new SqlCommand(sql, con) : new SqlCommand(sql, con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        var text = value is null or DBNull ? null : Convert.ToString(value)?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static async Task<string?> ReadLastRollAsync(
        SqlConnection con, int schoolId, int classId, int yearId, int sectionId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT TOP 1 sc.RollNo
FROM dbo.StudentsClass AS sc
INNER JOIN dbo.Student AS s ON s.StudentID = sc.StudentID
WHERE sc.SchoolID = @SchoolID
  AND sc.ClassID = @ClassID
  AND sc.EducationYearID = @YearID
  AND (@SectionID = 0 OR sc.SectionID = @SectionID)
  AND s.Status = N'Active'
ORDER BY sc.StudentClassID DESC
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@YearID", yearId);
        cmd.Parameters.AddWithValue("@SectionID", sectionId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        var text = value is null or DBNull ? null : Convert.ToString(value)?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static async Task<int> ReadAdminRegistrationAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        const string sql = """
SELECT TOP 1 RegistrationID
FROM dbo.Registration
WHERE SchoolID = @SchoolID AND Validation = N'Valid' AND Category = N'Admin'
ORDER BY RegistrationID
""";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task<string> NextFreeStudentCodeAsync(
        SqlConnection con, int schoolId, string? lastId, CancellationToken cancellationToken, SqlTransaction tx)
    {
        var code = NextStudentCode(lastId);
        for (var i = 0; i < 5000; i += 1)
        {
            if (!await StudentCodeExistsAsync(con, schoolId, code, cancellationToken, tx))
                return code;
            code = NextStudentCode(code);
        }
        throw new InvalidOperationException("Could not find a free student ID after the last entry.");
    }

    private static async Task<bool> StudentCodeExistsAsync(
        SqlConnection con, int schoolId, string code, CancellationToken cancellationToken, SqlTransaction tx)
    {
        const string sql = "SELECT TOP 1 StudentID FROM dbo.Student WHERE SchoolID = @SchoolID AND ID = @ID";
        await using var cmd = new SqlCommand(sql, con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@ID", code);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null && value is not DBNull;
    }

    private static string NextStudentCode(string? lastId)
    {
        var last = (lastId ?? "").Trim();
        if (last.Length == 0)
            return "1";

        var i = last.Length - 1;
        while (i >= 0 && char.IsDigit(last[i]))
            i -= 1;
        if (i == last.Length - 1)
            return last + "1";

        var prefix = last[..(i + 1)];
        var digits = last[(i + 1)..];
        if (!BigInteger.TryParse(digits, out var number))
            return last + "1";
        return prefix + (number + 1).ToString().PadLeft(digits.Length, '0');
    }

    private static async Task<int?> InsertStudentImageAsync(
        SqlConnection con, SqlTransaction tx, string? photoDataUrl, CancellationToken cancellationToken)
    {
        var bytes = DecodePhoto(photoDataUrl) ?? [];

        const string sql = """
INSERT INTO dbo.Student_Image (Image, Guardian_Photo)
VALUES (@Image, @Guardian);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""";
        await using var cmd = new SqlCommand(sql, con, tx);
        cmd.Parameters.AddWithValue("@Image", bytes);
        cmd.Parameters.AddWithValue("@Guardian", Array.Empty<byte>());
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    private static byte[]? DecodePhoto(string? photoDataUrl)
    {
        if (string.IsNullOrWhiteSpace(photoDataUrl))
            return null;
        var text = photoDataUrl.Trim();
        var comma = text.IndexOf(',');
        if (text.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > 0)
            text = text[(comma + 1)..];
        try
        {
            var bytes = Convert.FromBase64String(text);
            return bytes.Length is > 0 and <= 2_000_000 ? bytes : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static DateTime? ParseDate(string? value)
    {
        var text = (value ?? "").Trim();
        if (text.Length == 0)
            return null;
        string[] formats = ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy"];
        if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact.Date;
        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed.Date : null;
    }

    private static string? BlankIfUnknown(string? value)
    {
        var text = (value ?? "").Trim();
        return text.Length == 0 || text.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ? null : text;
    }

    public async Task<WebsiteAdmitResult> AddAdmissionPaymentsAsync(
        int schoolId, WebsiteAdmitPaymentsRequest request, CancellationToken cancellationToken)
    {
        if (request.StudentID <= 0)
            return Fail("Student is required.");
        if (request.ClassID <= 0 || request.EducationYearID <= 0)
            return Fail("Class and session are required.");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var school = await ReadSchoolAsync(con, schoolId, cancellationToken);
        if (school is null)
            return Fail("Institution was not found.");

        var registrationId = await ReadAdminRegistrationAsync(con, schoolId, cancellationToken);
        if (registrationId <= 0)
            return Fail("No valid Admin registration was found for this school.");

        string studentCode;
        int studentClassId;
        await using (var find = new SqlCommand("""
SELECT TOP 1 s.ID, sc.StudentClassID, sc.RollNo
FROM dbo.Student AS s
INNER JOIN dbo.StudentsClass AS sc ON sc.StudentID = s.StudentID
WHERE s.StudentID = @StudentID AND s.SchoolID = @SchoolID
  AND sc.EducationYearID = @YearID
ORDER BY sc.StudentClassID DESC
""", con))
        {
            find.Parameters.AddWithValue("@StudentID", request.StudentID);
            find.Parameters.AddWithValue("@SchoolID", schoolId);
            find.Parameters.AddWithValue("@YearID", request.EducationYearID);
            await using var reader = await find.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return Fail("Student class was not found.");
            studentCode = reader["ID"]?.ToString()?.Trim() ?? "";
            studentClassId = Convert.ToInt32(reader["StudentClassID"]);
        }

        var className = await ReadClassNameAsync(con, schoolId, request.ClassID, cancellationToken);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
        try
        {
            var payments = await InsertPayOrdersAsync(
                con, tx, schoolId, registrationId, request.StudentID, studentClassId,
                request.ClassID, request.EducationYearID, request.Payments, cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return new WebsiteAdmitResult
            {
                Succeeded = true,
                StudentID = request.StudentID,
                StudentClassID = studentClassId,
                StudentCode = studentCode,
                ClassName = className,
                SchoolName = school.SchoolName,
                SchoolAddress = school.Address,
                SchoolPhone = school.Phone,
                AdmissionDate = DateTime.Now,
                Payments = payments
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return Fail(ex.Message);
        }
    }

    private static object IdOrZero(int? id) => id is > 0 ? id.Value : 0;

    private static object Db(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static WebsiteAdmitResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static async Task<string> ReadClassNameAsync(
        SqlConnection con, int schoolId, int classId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "SELECT TOP 1 Class FROM dbo.CreateClass WHERE ClassID = @ClassID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@ClassID", classId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? "" : Convert.ToString(value)?.Trim() ?? "";
    }

    private static async Task<List<WebsiteFeeRoleDto>> ReadFeeRolesAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT RoleID, Role, NumberOfPay
FROM dbo.Income_Roles
WHERE SchoolID = @SchoolID
ORDER BY Role
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            var list = new List<WebsiteFeeRoleDto>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new WebsiteFeeRoleDto
                {
                    RoleID = Convert.ToInt32(reader["RoleID"]),
                    Role = reader["Role"]?.ToString()?.Trim() ?? "",
                    NumberOfPay = reader["NumberOfPay"] is DBNull ? 0 : Convert.ToInt32(reader["NumberOfPay"])
                });
            }
            return list;
        }
        catch (SqlException)
        {
            return [];
        }
    }

    private static async Task<List<WebsiteSuggestedFeeDto>> ReadSuggestedFeesAsync(
        SqlConnection con, int schoolId, int classId, int yearId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT a.AssignRoleID, a.RoleID, r.Role, a.PayFor, a.Amount, a.LateFee, a.StartDate, a.EndDate
FROM dbo.Income_Assign_Role AS a
INNER JOIN dbo.Income_Roles AS r ON r.RoleID = a.RoleID
WHERE a.SchoolID = @SchoolID AND a.ClassID = @ClassID
  AND (@YearID = 0 OR a.EducationYearID = @YearID)
  AND a.Amount > 0
ORDER BY r.Role, a.PayFor
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@YearID", yearId);
            var list = new List<WebsiteSuggestedFeeDto>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new WebsiteSuggestedFeeDto
                {
                    AssignRoleID = Convert.ToInt32(reader["AssignRoleID"]),
                    RoleID = Convert.ToInt32(reader["RoleID"]),
                    Role = reader["Role"]?.ToString()?.Trim() ?? "",
                    PayFor = reader["PayFor"]?.ToString()?.Trim() ?? "",
                    Amount = reader["Amount"] is DBNull ? 0 : Convert.ToDecimal(reader["Amount"]),
                    LateFee = reader["LateFee"] is DBNull ? 0 : Convert.ToDecimal(reader["LateFee"]),
                    StartDate = reader["StartDate"] is DBNull ? null : Convert.ToDateTime(reader["StartDate"]).Date,
                    EndDate = reader["EndDate"] is DBNull ? null : Convert.ToDateTime(reader["EndDate"]).Date
                });
            }
            return list;
        }
        catch (SqlException)
        {
            return [];
        }
    }

    private static async Task<List<WebsiteAdmitPayment>> InsertPayOrdersAsync(
        SqlConnection con, SqlTransaction tx, int schoolId, int registrationId, int studentId, int studentClassId,
        int classId, int yearId, List<WebsiteAdmitPayment>? payments, CancellationToken cancellationToken)
    {
        var created = new List<WebsiteAdmitPayment>();
        foreach (var item in payments ?? [])
        {
            var amount = item.Amount;
            var discount = item.Discount < 0 ? 0 : item.Discount;
            if (discount > amount)
                discount = amount;
            if (item.RoleID <= 0 || amount <= 0)
                continue;
            var payFor = (item.PayFor ?? "").Trim();
            if (payFor.Length == 0)
                payFor = (item.Role ?? "").Trim();
            if (payFor.Length == 0)
                continue;

            var role = (item.Role ?? "").Trim();
            if (role.Length == 0)
            {
                await using var roleCmd = new SqlCommand(
                    "SELECT TOP 1 Role FROM dbo.Income_Roles WHERE RoleID = @RoleID AND SchoolID = @SchoolID", con, tx);
                roleCmd.Parameters.AddWithValue("@RoleID", item.RoleID);
                roleCmd.Parameters.AddWithValue("@SchoolID", schoolId);
                role = Convert.ToString(await roleCmd.ExecuteScalarAsync(cancellationToken))?.Trim() ?? "";
            }

            var start = item.StartDate?.Date ?? DateTime.Today;
            var end = item.EndDate?.Date ?? start.AddMonths(1);

            const string sql = """
INSERT INTO dbo.Income_PayOrder
    (SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, AssignRoleID, EducationYearID,
     Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor, StartDate, EndDate, CreatedDate)
VALUES
    (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SCID, @Assign, @YearID,
     @Amount, 0, @LateFee, @Discount, 0, @RoleID, @PayFor, @Start, @End, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);
""";
            await using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            cmd.Parameters.AddWithValue("@ClassID", classId);
            cmd.Parameters.AddWithValue("@SCID", studentClassId);
            cmd.Parameters.AddWithValue("@Assign", item.AssignRoleID > 0 ? item.AssignRoleID : DBNull.Value);
            cmd.Parameters.AddWithValue("@YearID", yearId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Discount", discount);
            cmd.Parameters.AddWithValue("@LateFee", item.LateFee);
            cmd.Parameters.AddWithValue("@RoleID", item.RoleID);
            cmd.Parameters.AddWithValue("@PayFor", payFor);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);
            await cmd.ExecuteScalarAsync(cancellationToken);
            created.Add(new WebsiteAdmitPayment
            {
                RoleID = item.RoleID,
                Role = role,
                PayFor = payFor,
                Amount = amount,
                Discount = discount,
                AssignRoleID = item.AssignRoleID,
                LateFee = item.LateFee,
                StartDate = start,
                EndDate = end
            });
        }
        return created;
    }

    private async Task<(bool Sent, bool Local, string? Error)> TrySendAdmissionSmsAsync(
        SqlConnection con, int schoolId, int yearId, int studentId, string studentName, string studentCode,
        string className, string? rollNo, string phone, string schoolName, CancellationToken cancellationToken)
    {
        try
        {
            var template = await ReadAdmissionSmsTemplateAsync(con, schoolId, cancellationToken);
            var text = string.IsNullOrWhiteSpace(template)
                ? $"Congratulation!! {studentName} You have been Online admitted into class: {className}. ID: {studentCode}. Roll No: {rollNo} Please save this information for future. Regards: {schoolName}"
                : template
                    .Replace("{StudentName}", studentName)
                    .Replace("{ID}", studentCode)
                    .Replace("{Class}", className)
                    .Replace("{RollNo}", rollNo ?? "")
                    .Replace("{AdmissionDate}", DateTime.Now.ToString("dd MMM yyyy"))
                    .Replace("{SchoolName}", schoolName);

            var count = SmsCount(text);
            if (!_sms.IsLocal)
            {
                var balance = await ReadSmsBalanceAsync(con, schoolId, cancellationToken);
                if (balance < count)
                    return (false, false, "SMS balance is not enough.");
            }

            var call = await _sms.TrySendAsync(phone, text, cancellationToken);
            if (!string.IsNullOrWhiteSpace(call.Error) || string.IsNullOrWhiteSpace(call.Body))
                return (false, call.Local, call.Error ?? "Admission SMS was not sent.");

            var smsId = Guid.NewGuid();
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES
    (@ID, @Phone, @Text, @Len, @Count, N'Admission', @Status, GETDATE(), @Resp)
""", con))
            {
                ins.Parameters.AddWithValue("@ID", smsId);
                ins.Parameters.AddWithValue("@Phone", phone);
                ins.Parameters.AddWithValue("@Text", text);
                ins.Parameters.AddWithValue("@Len", text.Length);
                ins.Parameters.AddWithValue("@Count", count);
                ins.Parameters.AddWithValue("@Status", call.Local ? "Local" : "Sent");
                ins.Parameters.AddWithValue("@Resp", call.Body);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID)
VALUES (@ID, @SchoolID, @SID, @TID, @YearID)
""", con))
            {
                other.Parameters.AddWithValue("@ID", smsId);
                other.Parameters.AddWithValue("@SchoolID", schoolId);
                other.Parameters.AddWithValue("@SID", studentId);
                other.Parameters.AddWithValue("@TID", DBNull.Value);
                other.Parameters.AddWithValue("@YearID", yearId);
                await other.ExecuteNonQueryAsync(cancellationToken);
            }

            if (!call.Local)
            {
                await using var dec = new SqlCommand(
                    "UPDATE dbo.SMS SET SMS_Balance = SMS_Balance - @Count WHERE SchoolID = @SchoolID AND SMS_Balance >= @Count", con);
                dec.Parameters.AddWithValue("@Count", count);
                dec.Parameters.AddWithValue("@SchoolID", schoolId);
                await dec.ExecuteNonQueryAsync(cancellationToken);
            }

            return (true, call.Local, null);
        }
        catch (Exception ex)
        {
            return (false, _sms.IsLocal, ex.Message);
        }
    }

    private static async Task<string?> ReadAdmissionSmsTemplateAsync(
        SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND IsActive = 1
  AND TemplateType IN (N'AdmissionConfirm', N'AdmissionWelcome')
ORDER BY CASE WHEN TemplateType = N'AdmissionConfirm' THEN 0 ELSE 1 END
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await cmd.ExecuteScalarAsync(cancellationToken);
            var text = value is null or DBNull ? null : Convert.ToString(value);
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (SqlException)
        {
            return null;
        }
    }

    private static async Task<int> ReadSmsBalanceAsync(SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static int SmsCount(string text)
    {
        var unicode = text.Any(ch => ch > 127);
        var size = unicode ? 70 : 160;
        return Math.Max(1, (int)Math.Ceiling(text.Length / (double)size));
    }
}
