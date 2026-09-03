using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Committee;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class CommitteeService
{
    private const string OverdueFilter = """
(ISNULL(d.Due, 0) > 0) AND (d.PromiseDate < CAST(GETDATE() AS date) OR d.PromiseDate IS NULL)
""";

    public async Task<DonorDueSummaryDto> GetDonorDueSummaryAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand($"""
SELECT ISNULL(SUM(d.Due), 0) AS TotalDue
FROM dbo.CommitteeDonation d
WHERE d.SchoolID = @SchoolID AND {OverdueFilter}
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var total = Dec(await cmd.ExecuteScalarAsync(ct));
        return new DonorDueSummaryDto { TotalDue = total };
    }

    public async Task<IReadOnlyList<CommitteeOptionDto>> GetDonorDueCategoriesAsync(SessionSnapshot session, int typeId, CancellationToken ct)
    {
        if (typeId <= 0) return [];
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand($"""
SELECT DISTINCT cdc.CommitteeDonationCategoryId, cdc.DonationCategory
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeDonationCategory cdc ON d.CommitteeDonationCategoryId = cdc.CommitteeDonationCategoryId
INNER JOIN dbo.CommitteeMember cm ON d.CommitteeMemberId = cm.CommitteeMemberId
WHERE d.SchoolID = @SchoolID AND {OverdueFilter}
  AND cm.CommitteeMemberTypeId = @TypeId
ORDER BY cdc.DonationCategory
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@TypeId", typeId);
        return await OptionsAsync(con, cmd, ct);
    }

    public async Task<DonorDueByTypeListDto> GetDonorDueByTypeAsync(SessionSnapshot session, int typeId, int categoryId, CancellationToken ct)
    {
        var dto = new DonorDueByTypeListDto();
        if (typeId <= 0) return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand($"""
SELECT cm.CommitteeMemberId, cm.MemberName, cmt.CommitteeMemberType, cm.SmsNumber, SUM(d.Due) AS Due
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeMember cm ON d.CommitteeMemberId = cm.CommitteeMemberId
INNER JOIN dbo.CommitteeMemberType cmt ON cm.CommitteeMemberTypeId = cmt.CommitteeMemberTypeId
WHERE d.SchoolID = @SchoolID AND {OverdueFilter}
  AND cm.CommitteeMemberTypeId = @TypeId
  AND (@CatId = 0 OR d.CommitteeDonationCategoryId = @CatId)
GROUP BY cm.CommitteeMemberId, cm.MemberName, cmt.CommitteeMemberType, cm.SmsNumber
ORDER BY cm.MemberName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@TypeId", typeId);
        cmd.Parameters.AddWithValue("@CatId", categoryId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var due = Dec(reader["Due"]);
            dto.TypeTotalDue += due;
            dto.Rows.Add(new DonorDueRowDto
            {
                CommitteeMemberId = I(reader["CommitteeMemberId"]),
                MemberName = S(reader["MemberName"]),
                MemberType = S(reader["CommitteeMemberType"]),
                SmsNumber = S(reader["SmsNumber"]),
                Due = due
            });
        }
        return dto;
    }

    public async Task<DonorDueMemberDetailDto> GetDonorDueByNameAsync(SessionSnapshot session, string? q, CancellationToken ct)
    {
        var search = (q ?? "").Trim();
        var dto = new DonorDueMemberDetailDto();
        if (search.Length == 0) return dto;

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var head = new SqlCommand($"""
SELECT TOP (1) cm.CommitteeMemberId, cm.MemberName, cmt.CommitteeMemberType, cm.SmsNumber, cm.Address,
       SUM(d.Due) AS TotalDue
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeMember cm ON d.CommitteeMemberId = cm.CommitteeMemberId
INNER JOIN dbo.CommitteeMemberType cmt ON cm.CommitteeMemberTypeId = cmt.CommitteeMemberTypeId
WHERE d.SchoolID = @SchoolID AND {OverdueFilter}
  AND (cm.MemberName LIKE N'%' + @Q + N'%' OR cm.SmsNumber LIKE N'%' + @Q + N'%')
GROUP BY cm.CommitteeMemberId, cm.MemberName, cmt.CommitteeMemberType, cm.SmsNumber, cm.Address
ORDER BY cm.MemberName
""", con))
        {
            head.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            head.Parameters.AddWithValue("@Q", search);
            await using var reader = await head.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return dto;
            dto.Found = true;
            dto.CommitteeMemberId = I(reader["CommitteeMemberId"]);
            dto.MemberName = S(reader["MemberName"]);
            dto.MemberType = S(reader["CommitteeMemberType"]);
            dto.SmsNumber = S(reader["SmsNumber"]);
            dto.Address = S(reader["Address"]);
            dto.TotalDue = Dec(reader["TotalDue"]);
        }

        dto.Lines.AddRange(await ReadDonorDueLinesAsync(con, session.SchoolID, search, 0, ct));
        return dto;
    }

    public async Task<IReadOnlyList<DonorDueViewBlockDto>> GetDonorDueViewAsync(SessionSnapshot session, DonorDueViewRequest? request, CancellationToken ct)
    {
        var ids = (request?.CommitteeMemberIds ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0) return [];

        var blocks = new List<DonorDueViewBlockDto>();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        foreach (var id in ids)
        {
            await using var mem = new SqlCommand("""
SELECT MemberName, SmsNumber FROM dbo.CommitteeMember
WHERE SchoolID = @SchoolID AND CommitteeMemberId = @Id
""", con);
            mem.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            mem.Parameters.AddWithValue("@Id", id);
            var name = "";
            var phone = "";
            await using (var reader = await mem.ExecuteReaderAsync(ct))
            {
                if (!await reader.ReadAsync(ct)) continue;
                name = S(reader["MemberName"]);
                phone = S(reader["SmsNumber"]);
            }

            var lines = await ReadDonorDueLinesAsync(con, session.SchoolID, null, id, ct, request?.CategoryId ?? 0);
            if (lines.Count == 0) continue;
            blocks.Add(new DonorDueViewBlockDto
            {
                CommitteeMemberId = id,
                MemberName = name,
                SmsNumber = phone,
                TotalDue = lines.Sum(x => x.Due),
                Lines = lines
            });
        }
        return blocks;
    }

    public async Task<DonorDueSmsResult> SendDonorDueSmsAsync(SessionSnapshot session, DonorDueSmsRequest? request, CancellationToken ct)
    {
        var ids = (request?.CommitteeMemberIds ?? []).Where(x => x > 0).Distinct().ToList();
        if (ids.Count == 0)
            return new DonorDueSmsResult { Error = "cm.bulkEditNeedSelect" };

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var template = await ReadDonorDueTemplateAsync(con, session.SchoolID, ct);
        var school = await SessionSchool.ResolveNameAsync(session, con, ct);
        var balance = await ReadSmsBalanceAsync(con, session.SchoolID, ct);

        var jobs = new List<(int MemberId, string Phone, string Message, int Count)>();
        foreach (var id in ids)
        {
            var member = await LoadDonorMemberAsync(con, session.SchoolID, id, ct);
            if (member is null)
            {
                jobs.Add((id, "", "", 0));
                continue;
            }
            var lines = await ReadDonorDueLinesAsync(con, session.SchoolID, null, id, ct, request?.CategoryId ?? 0);
            var totalDue = lines.Sum(x => x.Due);
            var dueDetails = string.Join(", ", lines.Select(x =>
                $"{x.DonationCategory}: {x.Description} - {FmtDue(x.Due)} Tk"));
            var msg = BuildDonorDueMessage(template, member.Value.Name, totalDue, dueDetails, school);
            var phone = member.Value.Phone;
            jobs.Add((id, phone, msg, IsValidBdMobile(phone) ? SmsCount(msg) : 0));
        }

        var needed = jobs.Sum(x => x.Count);
        if (needed == 0)
            return new DonorDueSmsResult { Error = "acc.smsNoPhone", Failed = ids.Count };
        if (balance < needed)
            return new DonorDueSmsResult { Error = "acc.smsLow", Message = balance.ToString() };

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
VALUES (@ID, @Phone, @Text, @Len, @Count, N'Donor Due SMS', @Status, GETDATE(), @Resp)
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
            await using (var other = new SqlCommand("""
INSERT INTO dbo.SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID, CommitteeMemberId)
VALUES (@ID, @SchoolID, NULL, NULL, @YearID, @MemberId)
""", con))
            {
                other.Parameters.AddWithValue("@ID", smsId);
                other.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                other.Parameters.AddWithValue("@YearID", session.EducationYearID);
                other.Parameters.AddWithValue("@MemberId", job.MemberId);
                await other.ExecuteNonQueryAsync(ct);
            }
            sent++;
        }

        return new DonorDueSmsResult
        {
            Succeeded = sent > 0,
            Sent = sent,
            Failed = failed,
            Message = sent > 0 ? "cm.donorDueSmsOk" : null,
            Error = sent == 0 ? "cm.fail" : null
        };
    }

    private async Task<List<DonorDueLineDto>> ReadDonorDueLinesAsync(
        SqlConnection con, int schoolId, string? search, int memberId, CancellationToken ct, int categoryId = 0)
    {
        await using var cmd = new SqlCommand($"""
SELECT cdc.DonationCategory, d.Description, d.Amount, ISNULL(d.PaidAmount, 0) AS PaidAmount,
       ISNULL(d.Due, 0) AS Due, d.PromiseDate
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeDonationCategory cdc ON d.CommitteeDonationCategoryId = cdc.CommitteeDonationCategoryId
INNER JOIN dbo.CommitteeMember cm ON d.CommitteeMemberId = cm.CommitteeMemberId
WHERE d.SchoolID = @SchoolID AND {OverdueFilter}
  AND (@MemberId = 0 OR d.CommitteeMemberId = @MemberId)
  AND (@Q = N'' OR cm.MemberName LIKE N'%' + @Q + N'%' OR cm.SmsNumber LIKE N'%' + @Q + N'%')
  AND (@CatId = 0 OR d.CommitteeDonationCategoryId = @CatId)
ORDER BY d.PromiseDate
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@MemberId", memberId);
        cmd.Parameters.AddWithValue("@Q", (search ?? "").Trim());
        cmd.Parameters.AddWithValue("@CatId", categoryId);
        var rows = new List<DonorDueLineDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new DonorDueLineDto
            {
                DonationCategory = S(reader["DonationCategory"]),
                Description = S(reader["Description"]),
                Amount = Dec(reader["Amount"]),
                PaidAmount = Dec(reader["PaidAmount"]),
                Due = Dec(reader["Due"]),
                PromiseDate = Dt(reader["PromiseDate"])
            });
        }
        return rows;
    }

    private static async Task<List<CommitteeOptionDto>> OptionsAsync(SqlConnection con, SqlCommand cmd, CancellationToken ct)
    {
        var rows = new List<CommitteeOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new CommitteeOptionDto { Id = I(reader[0]), Name = S(reader[1]) });
        return rows;
    }

    private static async Task<(string Name, string Phone)?> LoadDonorMemberAsync(SqlConnection con, int schoolId, int memberId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT MemberName, SmsNumber FROM dbo.CommitteeMember
WHERE SchoolID = @SchoolID AND CommitteeMemberId = @Id
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Id", memberId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return (S(reader["MemberName"]), S(reader["SmsNumber"]));
    }

    private static async Task<int> ReadSmsBalanceAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT TOP 1 SMS_Balance FROM dbo.SMS WHERE SchoolID = @SchoolID", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null or DBNull ? 0 : Convert.ToInt32(value);
    }

    private static async Task<string?> ReadDonorDueTemplateAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID
  AND (
        TemplateType = N'DonorDue'
        OR TemplateType LIKE N'%DonorDue%'
        OR ISNULL(TemplateCategory, N'') = N'Donor'
      )
  AND (MessageTemplate LIKE N'%{TotalDue}%' OR MessageTemplate LIKE N'%{DueDetails}%')
ORDER BY CASE WHEN ISNULL(IsActive, 1) = 1 THEN 0 ELSE 1 END, CreatedDate DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            return (await cmd.ExecuteScalarAsync(ct))?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string BuildDonorDueMessage(string? template, string name, decimal totalDue, string dueDetails, string school)
    {
        if (!string.IsNullOrWhiteSpace(template))
        {
            var details = (dueDetails ?? "").Trim().TrimStart(',').TrimEnd(',').Trim();
            var msg = template
                .Replace("{DonorName}", name)
                .Replace("{TotalDue}", FmtDue(totalDue))
                .Replace("{SchoolName}", school);
            if (!string.IsNullOrWhiteSpace(details))
                msg = msg.Replace("{DueDetails}", details);
            else
                msg = msg.Replace(", {DueDetails}", "").Replace("{DueDetails}", "");
            return msg;
        }
        return $"সম্মানিত দাতা, {name}. আস্সালামু আলাইকুম, আপনার বকেয়া ডোনেশন: {FmtDue(totalDue)} টাকা. ধন্যবাদ, {school}";
    }

    private static string FmtDue(decimal amount) =>
        amount == decimal.Truncate(amount) ? decimal.Truncate(amount).ToString("0") : amount.ToString("0.##");

    private static int SmsCount(string text)
    {
        var len = text.Length;
        if (len <= 160) return 1;
        return (int)Math.Ceiling(len / 153.0);
    }

    private static bool IsValidBdMobile(string phone)
    {
        var digits = new string((phone ?? "").Where(char.IsDigit).ToArray());
        return digits.Length is 11 or 13 && (digits.StartsWith("01") || digits.StartsWith("8801"));
    }
}
