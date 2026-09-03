using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Sms;

namespace Sikkhaloy.SyncApi.Services;

public sealed class SmsTemplateService
{
    private const string SettingCategory = "_Setting";
    private const string DonorPaymentLangType = "DonorPaymentLang";

    private readonly EduConnectionFactory _connections;

    public SmsTemplateService(EduConnectionFactory connections) => _connections = connections;

    public async Task<IReadOnlyList<SmsTemplateDto>> ListAsync(SessionSnapshot session, string? category, CancellationToken ct)
    {
        var cat = (category ?? "").Trim();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            return await ReadTemplateListAsync(con, session.SchoolID, cat, useCategory: true, ct);
        }
        catch (SqlException)
        {
            return await ReadTemplateListAsync(con, session.SchoolID, cat, useCategory: false, ct);
        }
    }

    private static async Task<List<SmsTemplateDto>> ReadTemplateListAsync(
        SqlConnection con, int schoolId, string category, bool useCategory, CancellationToken ct)
    {
        var sql = useCategory
            ? """
SELECT TemplateID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, ISNULL(IsActive, 1) AS IsActive, CreatedDate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND ISNULL(TemplateCategory, N'') <> @SettingCat
"""
            : """
SELECT TemplateID, TemplateName, TemplateType AS TemplateCategory, TemplateType, MessageTemplate, 1 AS IsActive, NULL AS CreatedDate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID
""";
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (useCategory)
                sql += " AND TemplateCategory = @Category";
            else
                sql += " AND " + LegacyCategoryFilter(category);
        }
        sql += useCategory ? " ORDER BY CreatedDate DESC" : " ORDER BY TemplateID DESC";
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        if (useCategory)
            cmd.Parameters.AddWithValue("@SettingCat", SettingCategory);
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (useCategory)
                cmd.Parameters.AddWithValue("@Category", category);
            else if (category is not ("Donor" or "ExamResult" or "Attendance"))
                cmd.Parameters.AddWithValue("@Category", category);
        }
        var rows = new List<SmsTemplateDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(ReadRow(reader));
        return rows;
    }

    private static string LegacyCategoryFilter(string category) => category switch
    {
        "Donor" => "TemplateType IN (N'DonorDue', N'DonorPayment')",
        "ExamResult" => "TemplateType IN (N'Passed', N'Failed')",
        "Attendance" => "TemplateType IN (N'Entry', N'Exit', N'Late', N'Absent')",
        _ => "TemplateType = @Category"
    };

    public async Task<SmsTemplateDto?> GetAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return null;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT TemplateID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, ISNULL(IsActive, 1) AS IsActive, CreatedDate
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND TemplateID = @Id AND TemplateCategory <> @SettingCat
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@SettingCat", SettingCategory);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadRow(reader) : null;
    }

    public async Task<SmsTemplateResult> SaveAsync(SessionSnapshot session, SaveSmsTemplateRequest? request, CancellationToken ct)
    {
        if (request is null)
            return Fail("sms.tplNeed");
        var name = (request.TemplateName ?? "").Trim();
        var category = (request.TemplateCategory ?? "").Trim();
        var type = (request.TemplateType ?? "").Trim();
        var message = (request.MessageTemplate ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(message))
            return Fail("sms.tplNeed");
        if (category == SettingCategory)
            return Fail("sms.tplInvalid");

        var validation = ValidateTemplate(category, type, message);
        if (validation is not null)
            return Fail(validation);

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (request.TemplateID > 0)
        {
            await using var cmd = new SqlCommand("""
UPDATE dbo.SMS_Template
SET TemplateName = @Name, TemplateCategory = @Category, TemplateType = @Type,
    MessageTemplate = @Message, IsActive = @Active, UpdatedDate = GETDATE()
WHERE TemplateID = @Id AND SchoolID = @SchoolID AND TemplateCategory <> @SettingCat
""", con);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Category", category);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.Parameters.AddWithValue("@Message", message);
            cmd.Parameters.AddWithValue("@Active", request.IsActive);
            cmd.Parameters.AddWithValue("@Id", request.TemplateID);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@SettingCat", SettingCategory);
            var n = await cmd.ExecuteNonQueryAsync(ct);
            return n > 0
                ? new SmsTemplateResult { Succeeded = true, TemplateID = request.TemplateID }
                : Fail("sms.tplMissing");
        }

        await using (var cmd = new SqlCommand("""
INSERT INTO dbo.SMS_Template (SchoolID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, IsActive, CreatedDate, UpdatedDate)
VALUES (@SchoolID, @Name, @Category, @Type, @Message, @Active, GETDATE(), GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Category", category);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.Parameters.AddWithValue("@Message", message);
            cmd.Parameters.AddWithValue("@Active", request.IsActive);
            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            return new SmsTemplateResult { Succeeded = true, TemplateID = id };
        }
    }

    public async Task<SmsTemplateResult> DeleteAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return Fail("sms.tplMissing");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.SMS_Template
WHERE TemplateID = @Id AND SchoolID = @SchoolID AND TemplateCategory <> @SettingCat
""", con);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@SettingCat", SettingCategory);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0 ? new SmsTemplateResult { Succeeded = true } : Fail("sms.tplMissing");
    }

    public async Task<CommitteePaymentSmsLangDto> GetDonorPaymentLangAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return new CommitteePaymentSmsLangDto { Lang = await ReadDonorPaymentLangAsync(con, session.SchoolID, ct) };
    }

    public async Task<SmsTemplateResult> SaveDonorPaymentLangAsync(SessionSnapshot session, CommitteePaymentSmsLangDto? request, CancellationToken ct)
    {
        var lang = NormalizeLang(request?.Lang);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var find = new SqlCommand("""
SELECT TOP 1 TemplateID FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND TemplateCategory = @SettingCat AND TemplateType = @Type
""", con))
        {
            find.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            find.Parameters.AddWithValue("@SettingCat", SettingCategory);
            find.Parameters.AddWithValue("@Type", DonorPaymentLangType);
            var existing = await find.ExecuteScalarAsync(ct);
            if (existing is not null and not DBNull)
            {
                await using var upd = new SqlCommand("""
UPDATE dbo.SMS_Template SET MessageTemplate = @Lang, UpdatedDate = GETDATE()
WHERE TemplateID = @Id AND SchoolID = @SchoolID
""", con);
                upd.Parameters.AddWithValue("@Lang", lang);
                upd.Parameters.AddWithValue("@Id", Convert.ToInt32(existing));
                upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await upd.ExecuteNonQueryAsync(ct);
                return new SmsTemplateResult { Succeeded = true };
            }
        }
        await using var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Template (SchoolID, TemplateName, TemplateCategory, TemplateType, MessageTemplate, IsActive, CreatedDate, UpdatedDate)
VALUES (@SchoolID, N'Setting', @SettingCat, @Type, @Lang, 0, GETDATE(), GETDATE())
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@SettingCat", SettingCategory);
        ins.Parameters.AddWithValue("@Type", DonorPaymentLangType);
        ins.Parameters.AddWithValue("@Lang", lang);
        await ins.ExecuteNonQueryAsync(ct);
        return new SmsTemplateResult { Succeeded = true };
    }

    public async Task<string?> ResolveDonorPaymentTemplateAsync(int schoolId, string lang, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await ReadDonorPaymentTemplateAsync(con, schoolId, lang, ct);
    }

    public async Task<string> ResolveDonorPaymentLangAsync(int schoolId, string? requestedLang, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(requestedLang))
            return NormalizeLang(requestedLang);
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        return await ReadDonorPaymentLangAsync(con, schoolId, ct);
    }

    private static async Task<string> ReadDonorPaymentLangAsync(SqlConnection con, int schoolId, CancellationToken ct)
    {
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TOP 1 MessageTemplate FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID AND TemplateCategory = @SettingCat AND TemplateType = @Type
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@SettingCat", SettingCategory);
            cmd.Parameters.AddWithValue("@Type", DonorPaymentLangType);
            var value = (await cmd.ExecuteScalarAsync(ct))?.ToString();
            return NormalizeLang(value);
        }
        catch
        {
            return "bn";
        }
    }

    private static async Task<string?> ReadDonorPaymentTemplateAsync(SqlConnection con, int schoolId, string lang, CancellationToken ct)
    {
        var rows = new List<(string Name, string Message, bool Active)>();
        try
        {
            await using var cmd = new SqlCommand("""
SELECT TemplateName, MessageTemplate, ISNULL(IsActive, 1) AS IsActive
FROM dbo.SMS_Template
WHERE SchoolID = @SchoolID
  AND TemplateCategory = N'Donor'
  AND (
        TemplateType = N'DonorPayment'
        OR TemplateType LIKE N'%DonorPayment%'
      )
  AND (
        MessageTemplate LIKE N'%{ReceiptNo}%'
        OR MessageTemplate LIKE N'%{Amount}%'
        OR MessageTemplate LIKE N'%{PaymentDetails}%'
      )
ORDER BY CASE WHEN ISNULL(IsActive, 1) = 1 THEN 0 ELSE 1 END, CreatedDate DESC
""", con);
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add((
                    reader["TemplateName"]?.ToString() ?? "",
                    reader["MessageTemplate"]?.ToString() ?? "",
                    reader["IsActive"] is not DBNull && Convert.ToBoolean(reader["IsActive"])));
            }
        }
        catch
        {
            return null;
        }

        if (rows.Count == 0) return null;
        var wantEn = lang == "en";
        foreach (var row in rows.Where(x => x.Active))
        {
            var tag = LangTag(row.Name);
            if (wantEn && tag == "en") return row.Message;
            if (!wantEn && tag == "bn") return row.Message;
        }
        foreach (var row in rows.Where(x => x.Active))
        {
            if (LangTag(row.Name) is null) return row.Message;
        }
        return rows.FirstOrDefault(x => x.Active).Message ?? rows[0].Message;
    }

    private static string? LangTag(string name)
    {
        var n = (name ?? "").ToLowerInvariant();
        if (n.Contains("[en]") || n.Contains("(en)") || n.Contains("english") || n.Contains("ইংরেজি"))
            return "en";
        if (n.Contains("[bn]") || n.Contains("(bn)") || n.Contains("bangla") || n.Contains("bengali") || n.Contains("বাংলা"))
            return "bn";
        return null;
    }

    private static string NormalizeLang(string? lang) =>
        string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "bn";

    private static string? ValidateTemplate(string category, string type, string message)
    {
        if (category == "Donor")
        {
            var hasPayment = message.Contains("{Amount}") || message.Contains("{ReceiptNo}") || message.Contains("{PaymentDetails}");
            var hasDue = message.Contains("{TotalDue}") || message.Contains("{DueDetails}");
            if (type == "DonorDue" && hasPayment && !hasDue) return "sms.tplDonorDueType";
            if (type == "DonorPayment" && hasDue && !hasPayment) return "sms.tplDonorPayType";
        }
        return null;
    }

    private static SmsTemplateDto ReadRow(SqlDataReader reader) => new()
    {
        TemplateID = Convert.ToInt32(reader["TemplateID"]),
        TemplateName = reader["TemplateName"]?.ToString() ?? "",
        TemplateCategory = reader["TemplateCategory"]?.ToString() ?? "",
        TemplateType = reader["TemplateType"]?.ToString() ?? "",
        MessageTemplate = reader["MessageTemplate"]?.ToString() ?? "",
        IsActive = reader["IsActive"] is not DBNull && Convert.ToBoolean(reader["IsActive"]),
        CreatedDate = reader["CreatedDate"] is DBNull ? null : Convert.ToDateTime(reader["CreatedDate"])
    };

    private static SmsTemplateResult Fail(string error) => new() { Error = error };
}
