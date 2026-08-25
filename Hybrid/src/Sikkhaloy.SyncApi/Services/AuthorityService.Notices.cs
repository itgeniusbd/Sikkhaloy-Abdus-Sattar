using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Authority;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class AuthorityService
{
    public Task<List<AuthNoticeDto>> ListActiveNoticesAsync(CancellationToken ct) =>
        QueryNoticesAsync("WHERE GETDATE() BETWEEN Show_Date AND End_Date", swallow: true, ct);

    public async Task<List<AuthNoticeDto>> ListNoticesAsync(SessionSnapshot session, CancellationToken ct)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");
        return await QueryNoticesAsync("", swallow: false, ct);
    }

    public async Task<AuthorityResult> SaveNoticeAsync(SessionSnapshot session, AuthNoticeSaveRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority)
            return Fail("auth.forbidden");
        var title = (request?.Title ?? "").Trim();
        if (title.Length == 0 || !DateTime.TryParse(request?.From, out var from) || !DateTime.TryParse(request?.To, out var to))
            return Fail("an.needTitle");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if ((request?.Id ?? 0) > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.Notice_Admin
SET Notice_Title = @Title, Notice = @Notice, Show_Date = @From, End_Date = @To
WHERE AdminNoticeID = @Id
""", con);
            upd.Parameters.AddWithValue("@Id", request!.Id);
            upd.Parameters.AddWithValue("@Title", title);
            upd.Parameters.AddWithValue("@Notice", (request.Notice ?? "").Trim());
            upd.Parameters.AddWithValue("@From", from.Date);
            upd.Parameters.AddWithValue("@To", to.Date);
            await upd.ExecuteNonQueryAsync(ct);
        }
        else
        {
            await using var ins = new SqlCommand("""
INSERT INTO dbo.Notice_Admin (Notice_Title, Notice, Show_Date, End_Date, RegistrationID)
VALUES (@Title, @Notice, @From, @To, @Reg)
""", con);
            ins.Parameters.AddWithValue("@Title", title);
            ins.Parameters.AddWithValue("@Notice", (request?.Notice ?? "").Trim());
            ins.Parameters.AddWithValue("@From", from.Date);
            ins.Parameters.AddWithValue("@To", to.Date);
            ins.Parameters.AddWithValue("@Reg", session.RegistrationID);
            await ins.ExecuteNonQueryAsync(ct);
        }
        return new AuthorityResult { Succeeded = true, Message = "an.saved" };
    }

    public async Task<AuthorityResult> DeleteNoticeAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (!session.IsAuthority)
            return Fail("auth.forbidden");
        if (id <= 0)
            return Fail("an.needTitle");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("DELETE FROM dbo.Notice_Admin WHERE AdminNoticeID = @Id", con);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
        return new AuthorityResult { Succeeded = true, Message = "an.deleted" };
    }

    public async Task<AuthUnreadDto> GetUnreadAsync(SessionSnapshot session, CancellationToken ct)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");
        return new AuthUnreadDto { Count = await CountUnreadAsync(ct) };
    }

    public async Task<AuthMessagePageDto> GetMessagesAsync(SessionSnapshot session, CancellationToken ct)
    {
        if (!session.IsAuthority)
            throw new InvalidOperationException("auth.forbidden");
        var dto = new AuthMessagePageDto { Unread = await CountUnreadAsync(ct) };
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var support = new SqlCommand("""
SELECT Public_Support.SupportID, Public_Support_Title.Support_Title,
       Admin.FirstName + ' ' + ISNULL(Admin.LastName, '') AS Name,
       SchoolInfo.SchoolName, Public_Support.Message, Public_Support.Sent_Date, Public_Support.Is_Read
FROM dbo.Public_Support
INNER JOIN dbo.Public_Support_Title ON Public_Support.SupportTitleID = Public_Support_Title.SupportTitleID
INNER JOIN dbo.SchoolInfo ON Public_Support.SchoolID = SchoolInfo.SchoolID
INNER JOIN dbo.Admin ON Public_Support.RegistrationID = Admin.RegistrationID
ORDER BY Public_Support.Sent_Date DESC
""", con);
            await using (var reader = await support.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    dto.Support.Add(new AuthSupportRowDto
                    {
                        Id = I(reader["SupportID"]),
                        SchoolName = S(reader["SchoolName"]),
                        Name = S(reader["Name"]).Trim(),
                        Subject = S(reader["Support_Title"]),
                        Message = S(reader["Message"]),
                        SentDate = Dt(reader["Sent_Date"]),
                        IsRead = Convert.ToBoolean(reader["Is_Read"])
                    });
                }
            }
        }
        catch
        {
        }

        try
        {
            await using var contact = new SqlCommand("""
SELECT ContactUsID, Name, Email, MobileNo, Subject, Message, Sent_Date, Is_Read
FROM dbo.Public_Contact_US
ORDER BY ContactUsID DESC
""", con);
            await using var reader = await contact.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Contact.Add(new AuthContactRowDto
                {
                    Id = I(reader["ContactUsID"]),
                    Name = S(reader["Name"]),
                    Email = S(reader["Email"]),
                    Mobile = S(reader["MobileNo"]),
                    Subject = S(reader["Subject"]),
                    Message = S(reader["Message"]),
                    SentDate = Dt(reader["Sent_Date"]),
                    IsRead = Convert.ToBoolean(reader["Is_Read"])
                });
            }
        }
        catch
        {
        }

        return dto;
    }

    public async Task<AuthorityResult> MarkMessageReadAsync(SessionSnapshot session, AuthMessageReadRequest? request, CancellationToken ct)
    {
        if (!session.IsAuthority)
            return Fail("auth.forbidden");
        var id = request?.Id ?? 0;
        if (id <= 0)
            return Fail("msg.empty");
        var kind = (request?.Kind ?? "").Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (kind.Equals("contact", StringComparison.OrdinalIgnoreCase))
        {
            await using var cmd = new SqlCommand("UPDATE dbo.Public_Contact_US SET Is_Read = 1 WHERE ContactUsID = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        else
        {
            await using var cmd = new SqlCommand("UPDATE dbo.Public_Support SET Is_Read = 1 WHERE SupportID = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        return new AuthorityResult { Succeeded = true };
    }

    public async Task<AuthorityResult> DeleteContactAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (!session.IsAuthority)
            return Fail("auth.forbidden");
        if (id <= 0)
            return Fail("msg.empty");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("DELETE FROM dbo.Public_Contact_US WHERE ContactUsID = @Id", con);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
        return new AuthorityResult { Succeeded = true, Message = "an.deleted" };
    }

    private async Task<int> CountUnreadAsync(CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand("""
SELECT
  (SELECT COUNT(*) FROM dbo.Public_Support WHERE Is_Read = 0) +
  (SELECT COUNT(*) FROM dbo.Public_Contact_US WHERE Is_Read = 0)
""", con);
            return I(await cmd.ExecuteScalarAsync(ct));
        }
        catch
        {
            return 0;
        }
    }

    private async Task<List<AuthNoticeDto>> QueryNoticesAsync(string where, bool swallow, CancellationToken ct)
    {
        var list = new List<AuthNoticeDto>();
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            foreach (var extra in new[] { ", Insert_Date", "" })
            {
                try
                {
                    list.Clear();
                    await using var cmd = new SqlCommand($"""
SELECT AdminNoticeID, Notice_Title, Notice, Show_Date, End_Date{extra}
FROM dbo.Notice_Admin
{where}
ORDER BY AdminNoticeID DESC
""", con);
                    await using var reader = await cmd.ExecuteReaderAsync(ct);
                    while (await reader.ReadAsync(ct))
                        list.Add(ReadNotice(reader));
                    return list;
                }
                catch
                {
                    if (extra.Length == 0 && !swallow)
                        throw;
                }
            }
        }
        catch when (swallow)
        {
        }
        return list;
    }

    private static AuthNoticeDto ReadNotice(SqlDataReader reader) => new()
    {
        Id = I(reader["AdminNoticeID"]),
        Title = S(reader["Notice_Title"]),
        Notice = S(reader["Notice"]),
        ShowDate = Dt(reader["Show_Date"]),
        EndDate = Dt(reader["End_Date"]),
        InsertDate = Dt(NameOrNull(reader, "Insert_Date"))
    };

    private static object NameOrNull(SqlDataReader reader, string name)
    {
        try { return reader[name]; }
        catch { return DBNull.Value; }
    }
}
