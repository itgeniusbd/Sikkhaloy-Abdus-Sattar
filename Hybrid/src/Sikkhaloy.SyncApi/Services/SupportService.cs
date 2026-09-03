using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Support;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SupportService
{
    private readonly EduConnectionFactory _connections;

    public SupportService(EduConnectionFactory connections) => _connections = connections;

    public async Task<SupportPageDto> GetPageAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new SupportPageDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var titles = new SqlCommand("""
SELECT SupportTitleID, Support_Title
FROM dbo.Public_Support_Title
ORDER BY ISNULL(SN, 999), Support_Title
""", con))
        await using (var reader = await titles.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Titles.Add(new SupportTitleDto
                {
                    SupportTitleID = I(reader["SupportTitleID"]),
                    SupportTitle = S(reader["Support_Title"])
                });
            }
        }

        await using var tickets = new SqlCommand("""
SELECT s.SupportID, ISNULL(t.Support_Title, N'') AS Support_Title, s.Message, s.Sent_Date, ISNULL(s.Is_Read, 0) AS Is_Read
FROM dbo.Public_Support s
LEFT JOIN dbo.Public_Support_Title t ON s.SupportTitleID = t.SupportTitleID
WHERE s.SchoolID = @SchoolID AND s.RegistrationID = @RegistrationID
ORDER BY s.Sent_Date DESC, s.SupportID DESC
""", con);
        tickets.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        tickets.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        await using (var reader = await tickets.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                dto.Tickets.Add(new SupportTicketDto
                {
                    SupportID = I(reader["SupportID"]),
                    Subject = S(reader["Support_Title"]),
                    Message = S(reader["Message"]),
                    IsRead = Convert.ToBoolean(reader["Is_Read"]),
                    SentDate = reader["Sent_Date"] is DateTime d ? d : null
                });
            }
        }

        return dto;
    }

    public async Task<SupportResult> SubmitAsync(SessionSnapshot session, SubmitSupportRequest? request, CancellationToken ct)
    {
        var titleId = request?.SupportTitleID ?? 0;
        var message = (request?.Message ?? "").Trim();
        if (titleId <= 0)
            return Fail("sup.needSubject");
        if (message.Length == 0)
            return Fail("sup.needMessage");
        if (message.Length > 4000)
            message = message[..4000];

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        await using (var check = new SqlCommand(
            "SELECT COUNT(*) FROM dbo.Public_Support_Title WHERE SupportTitleID = @Id", con))
        {
            check.Parameters.AddWithValue("@Id", titleId);
            if (I(await check.ExecuteScalarAsync(ct)) == 0)
                return Fail("sup.needSubject");
        }

        await using var ins = new SqlCommand("""
INSERT INTO dbo.Public_Support (SchoolID, RegistrationID, SupportTitleID, Message)
VALUES (@SchoolID, @RegistrationID, @SupportTitleID, @Message)
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        ins.Parameters.AddWithValue("@SupportTitleID", titleId);
        ins.Parameters.AddWithValue("@Message", message);
        await ins.ExecuteNonQueryAsync(ct);
        return new SupportResult { Succeeded = true, Message = "sup.ok" };
    }

    private static SupportResult Fail(string error) => new() { Error = error };

    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);

    private static string S(object? value) => value is null or DBNull ? "" : Convert.ToString(value) ?? "";
}
