using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed class EducationYearService
{
    private readonly EduConnectionFactory _connections;

    public EducationYearService(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<EducationYearResult> CreateAsync(
        SessionSnapshot session, SaveEducationYearRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0)
            return Fail("sess.needName");
        if (request is null || request.EndDate.Date < request.StartDate.Date)
            return Fail("sess.needRange");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (await NameExistsAsync(con, session.SchoolID, name, null, cancellationToken))
            return Fail("sess.exists");

        var sn = await NextSerialAsync(con, session.SchoolID, cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.Education_Year (SchoolID, RegistrationID, EducationYear, Status, StartDate, EndDate, SN)
VALUES (@SchoolID, @RegistrationID, @EducationYear, N'False', @StartDate, @EndDate, @SN);
SELECT CAST(SCOPE_IDENTITY() AS INT);
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@EducationYear", name);
        cmd.Parameters.AddWithValue("@StartDate", request.StartDate.Date);
        cmd.Parameters.AddWithValue("@EndDate", request.EndDate.Date);
        cmd.Parameters.AddWithValue("@SN", sn);
        var id = await cmd.ExecuteScalarAsync(cancellationToken);
        return new EducationYearResult
        {
            Succeeded = true,
            EducationYearID = id is null or DBNull ? 0 : Convert.ToInt32(id)
        };
    }

    public async Task<EducationYearResult> UpdateAsync(
        SessionSnapshot session, int yearId, SaveEducationYearRequest? request, CancellationToken cancellationToken)
    {
        var name = (request?.Name ?? "").Trim();
        if (yearId <= 0)
            return Fail("sess.needYear");
        if (name.Length == 0)
            return Fail("sess.needName");
        if (request is null || request.EndDate.Date < request.StartDate.Date)
            return Fail("sess.needRange");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsAsync(con, session.SchoolID, yearId, cancellationToken))
            return Fail("sess.needYear");

        var rename = !await NameExistsAsync(con, session.SchoolID, name, yearId, cancellationToken);
        await using var cmd = new SqlCommand(rename
            ? """
UPDATE dbo.Education_Year
SET EducationYear = @EducationYear, StartDate = @StartDate, EndDate = @EndDate
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
"""
            : """
UPDATE dbo.Education_Year
SET StartDate = @StartDate, EndDate = @EndDate
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@EducationYear", name);
        cmd.Parameters.AddWithValue("@StartDate", request.StartDate.Date);
        cmd.Parameters.AddWithValue("@EndDate", request.EndDate.Date);
        cmd.Parameters.AddWithValue("@EducationYearID", yearId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return new EducationYearResult { Succeeded = true, EducationYearID = yearId };
    }

    public async Task<EducationYearResult> DeleteAsync(
        SessionSnapshot session, int yearId, CancellationToken cancellationToken)
    {
        if (yearId <= 0)
            return Fail("sess.needYear");
        if (yearId == session.EducationYearID)
            return Fail("sess.current");

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        if (!await OwnsAsync(con, session.SchoolID, yearId, cancellationToken))
            return Fail("sess.needYear");
        if (await HasStudentsAsync(con, session.SchoolID, yearId, cancellationToken))
            return Fail("sess.inUse");
        if (await IsAssignedAsync(con, session.SchoolID, yearId, cancellationToken))
            return Fail("sess.current");

        await using var cmd = new SqlCommand("""
DELETE FROM dbo.Education_Year
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@EducationYearID", yearId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return new EducationYearResult { Succeeded = true, EducationYearID = yearId };
    }

    private static async Task<int> NextSerialAsync(SqlConnection con, int schoolId, CancellationToken cancellationToken)
    {
        try
        {
            await using var fn = new SqlCommand("SELECT dbo.F_EducationYear_SN(@SchoolID)", con);
            fn.Parameters.AddWithValue("@SchoolID", schoolId);
            var value = await fn.ExecuteScalarAsync(cancellationToken);
            if (value is not null and not DBNull)
                return Convert.ToInt32(value);
        }
        catch (SqlException)
        {
        }

        await using var cmd = new SqlCommand(
            "SELECT ISNULL(MAX(SN), 0) + 1 FROM dbo.Education_Year WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var next = await cmd.ExecuteScalarAsync(cancellationToken);
        return next is null or DBNull ? 1 : Convert.ToInt32(next);
    }

    private static async Task<bool> OwnsAsync(
        SqlConnection con, int schoolId, int yearId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(
            "SELECT 1 FROM dbo.Education_Year WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@EducationYearID", yearId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static async Task<bool> NameExistsAsync(
        SqlConnection con, int schoolId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        var sql = exceptId is null
            ? "SELECT 1 FROM dbo.Education_Year WHERE SchoolID = @SchoolID AND EducationYear = @EducationYear"
            : "SELECT 1 FROM dbo.Education_Year WHERE SchoolID = @SchoolID AND EducationYear = @EducationYear AND EducationYearID <> @EducationYearID";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@EducationYear", name);
        if (exceptId is not null)
            cmd.Parameters.AddWithValue("@EducationYearID", exceptId.Value);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static async Task<bool> HasStudentsAsync(
        SqlConnection con, int schoolId, int yearId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 1 FROM dbo.StudentsClass
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@EducationYearID", yearId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static async Task<bool> IsAssignedAsync(
        SqlConnection con, int schoolId, int yearId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 1 FROM dbo.Education_Year_User
WHERE EducationYearID = @EducationYearID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@EducationYearID", yearId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is not null and not DBNull;
    }

    private static EducationYearResult Fail(string error) => new() { Succeeded = false, Error = error };
}
