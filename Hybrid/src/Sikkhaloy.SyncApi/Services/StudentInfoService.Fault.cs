using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class StudentInfoService
{
    public async Task<List<StudentPortalFaultReportDto>> ListFaultReportsAsync(
        SessionSnapshot session, string? studentCode, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var items = new List<StudentPortalFaultReportDto>();
        var code = (studentCode ?? "").Trim();
        if (code.Length == 0)
            return items;

        var fromDate = from?.Date ?? new DateTime(1753, 1, 1);
        var toDate = to?.Date ?? new DateTime(9999, 12, 31);

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        var placement = await ResolveStudentByCodeAsync(con, session, code, cancellationToken);
        if (placement is null)
            return items;

        await using var cmd = new SqlCommand("""
SELECT sf.StudentFaultID, sf.Fault_Title, sf.Fault, sf.Fault_Date, ISNULL(r.UserName, N'') AS UserName
FROM dbo.Student_Fault AS sf
LEFT JOIN dbo.Registration AS r ON sf.RegistrationID = r.RegistrationID
WHERE sf.SchoolID = @SchoolID
  AND sf.EducationYearID = @EducationYearID
  AND sf.StudentClassID = @StudentClassID
  AND CAST(sf.Fault_Date AS date) BETWEEN @FromDate AND @ToDate
ORDER BY sf.Fault_Date DESC, sf.StudentFaultID DESC
""", con) { CommandTimeout = 20 };
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        cmd.Parameters.AddWithValue("@StudentClassID", placement.Value.StudentClassID);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StudentPortalFaultReportDto
            {
                StudentFaultID = ToInt(reader["StudentFaultID"]),
                Title = NullString(reader["Fault_Title"]) ?? "",
                Body = NullString(reader["Fault"]) ?? "",
                Date = ReadDate(reader["Fault_Date"]),
                PostBy = NullString(reader["UserName"]) ?? ""
            });
        }

        return items;
    }

    public async Task<StudentInfoResult> SaveFaultReportAsync(
        SessionSnapshot session, SaveStudentFaultReportRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
            return Fail("report.faultNeedTitle");
        var title = (request.Title ?? "").Trim();
        var body = (request.Body ?? "").Trim();
        if (title.Length == 0)
            return Fail("report.faultNeedTitle");
        if (body.Length == 0)
            return Fail("report.faultNeedBody");

        var code = (request.StudentCode ?? "").Trim();
        if (code.Length == 0)
            return Fail("report.needId");

        var faultDate = request.Date?.Date ?? DateTime.Today;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            var placement = await ResolveStudentByCodeAsync(con, session, code, cancellationToken);
            if (placement is null)
                return Fail("report.notFound");

            var registrationId = session.RegistrationID > 0 ? session.RegistrationID : placement.Value.StudentID;
            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Student_Fault
    (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, ClassID, Fault_Title, Fault, Fault_Date, InsertDate)
VALUES
    (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @StudentClassID, @ClassID, @Fault_Title, @Fault, @Fault_Date, @InsertDate)
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@StudentID", placement.Value.StudentID);
            cmd.Parameters.AddWithValue("@StudentClassID", placement.Value.StudentClassID);
            cmd.Parameters.AddWithValue("@ClassID", placement.Value.ClassID);
            cmd.Parameters.AddWithValue("@Fault_Title", title);
            cmd.Parameters.AddWithValue("@Fault", body);
            cmd.Parameters.AddWithValue("@Fault_Date", faultDate);
            cmd.Parameters.AddWithValue("@InsertDate", DateTime.Today);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return new StudentInfoResult { Succeeded = true };
        }
        catch (SqlException)
        {
            return Fail("report.faultFailed");
        }
    }

    public async Task<StudentInfoResult> UpdateFaultReportAsync(
        SessionSnapshot session, UpdateStudentFaultReportRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.StudentFaultID <= 0)
            return Fail("report.faultNeedTitle");
        var title = (request.Title ?? "").Trim();
        var body = (request.Body ?? "").Trim();
        if (title.Length == 0)
            return Fail("report.faultNeedTitle");
        if (body.Length == 0)
            return Fail("report.faultNeedBody");

        var faultDate = request.Date?.Date ?? DateTime.Today;
        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
UPDATE dbo.Student_Fault
SET Fault_Title = @Fault_Title, Fault = @Fault, Fault_Date = @Fault_Date
WHERE StudentFaultID = @StudentFaultID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@Fault_Title", title);
        cmd.Parameters.AddWithValue("@Fault", body);
        cmd.Parameters.AddWithValue("@Fault_Date", faultDate);
        cmd.Parameters.AddWithValue("@StudentFaultID", request.StudentFaultID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? new StudentInfoResult { Succeeded = true }
            : Fail("report.notFound");
    }

    public async Task<StudentInfoResult> DeleteFaultReportAsync(
        SessionSnapshot session, int studentFaultId, CancellationToken cancellationToken)
    {
        if (studentFaultId <= 0)
            return Fail("report.notFound");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.Student_Fault WHERE StudentFaultID = @StudentFaultID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@StudentFaultID", studentFaultId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0
            ? new StudentInfoResult { Succeeded = true }
            : Fail("report.notFound");
    }

    public async Task<StudentInfoResult> SaveFaultReportsBulkAsync(
        SessionSnapshot session, SaveStudentFaultReportsBulkRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.Items.Count == 0)
            return Fail("report.faultNeedBody");

        var lines = request.Items
            .Select(x => new { Title = (x.Title ?? "").Trim(), Body = (x.Body ?? "").Trim() })
            .Where(x => x.Title.Length > 0 && x.Body.Length > 0)
            .ToList();
        if (lines.Count == 0)
            return Fail("report.faultNeedBody");

        var code = (request.StudentCode ?? "").Trim();
        if (code.Length == 0)
            return Fail("report.needId");

        var faultDate = request.Date?.Date ?? DateTime.Today;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(cancellationToken);
            var placement = await ResolveStudentByCodeAsync(con, session, code, cancellationToken);
            if (placement is null)
                return Fail("report.notFound");

            var registrationId = session.RegistrationID > 0 ? session.RegistrationID : placement.Value.StudentID;
            await using var tx = (SqlTransaction)await con.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var line in lines)
                {
                    await using var cmd = new SqlCommand("""
INSERT INTO dbo.Student_Fault
    (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, ClassID, Fault_Title, Fault, Fault_Date, InsertDate)
VALUES
    (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @StudentClassID, @ClassID, @Fault_Title, @Fault, @Fault_Date, @InsertDate)
""", con, tx);
                    cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
                    cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
                    cmd.Parameters.AddWithValue("@StudentID", placement.Value.StudentID);
                    cmd.Parameters.AddWithValue("@StudentClassID", placement.Value.StudentClassID);
                    cmd.Parameters.AddWithValue("@ClassID", placement.Value.ClassID);
                    cmd.Parameters.AddWithValue("@Fault_Title", line.Title);
                    cmd.Parameters.AddWithValue("@Fault", line.Body);
                    cmd.Parameters.AddWithValue("@Fault_Date", faultDate);
                    cmd.Parameters.AddWithValue("@InsertDate", DateTime.Today);
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

                await tx.CommitAsync(cancellationToken);
                return new StudentInfoResult { Succeeded = true, Count = lines.Count };
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (SqlException)
        {
            return Fail("report.faultFailed");
        }
    }

    private static async Task<(int StudentID, int StudentClassID, int ClassID)?> ResolveStudentByCodeAsync(
        SqlConnection con, SessionSnapshot session, string code, CancellationToken cancellationToken)
    {
        await using var find = new SqlCommand("""
SELECT TOP 1 Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID
FROM dbo.StudentsClass
INNER JOIN dbo.Student ON StudentsClass.StudentID = Student.StudentID
WHERE StudentsClass.EducationYearID = @EducationYearID
  AND StudentsClass.SchoolID = @SchoolID
  AND Student.ID = @ID
""", con);
        find.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
        find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        find.Parameters.AddWithValue("@ID", code);
        await using var reader = await find.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return (ToInt(reader["StudentID"]), ToInt(reader["StudentClassID"]), ToInt(reader["ClassID"]));
    }
}
