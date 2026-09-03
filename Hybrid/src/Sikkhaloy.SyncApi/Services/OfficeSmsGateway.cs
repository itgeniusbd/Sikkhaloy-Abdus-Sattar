using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Sikkhaloy.SyncApi.Services;

/// <summary>Routes outbound SMS through SikkhaloySetting provider (V2 SmsService parity).</summary>
public sealed class OfficeSmsGateway
{
    private const string NovocomHost = "https://sms.novocom-bd.com/api/v2/";
    private const string NovocomApiKey = "NmOEbTPV33xPQ3iTZczN6Uc99jMs/p/oljruf6NzJyI=";
    private const string NovocomClientId = "621e744e-91cd-4ddd-8312-99c7a4cd8736";
    private const string NovocomSenderId = "8809658016341";

    private const string BanglaPhoneHost = "http://loopsitbd.powersms.net.bd/httpapi/";
    private const string BanglaPhoneUser = "Sikkhaloy";
    private const string BanglaPhonePassword = "Sikkhaloy@SMS_345";

    private const string GreenWebHost = "https://api.greenweb.com.bd/";
    private const string GreenWebApiKey = "90282210471675095047ee665e3d0ba098844814cab35e133dc4";

    private readonly EduConnectionFactory _connections;
    private string? _provider;

    public OfficeSmsGateway(EduConnectionFactory connections) => _connections = connections;

    public async Task<GatewayCall> SendAsync(string phone, string text, CancellationToken ct)
    {
        var provider = await ReadProviderAsync(ct);
        try
        {
            return provider switch
            {
                "Novocom" => await SendNovocomAsync(phone, text, ct),
                "GreenWeb" => await SendGreenWebAsync(phone, text, ct),
                _ => await SendBanglaPhoneAsync(phone, text, ct)
            };
        }
        catch (Exception ex)
        {
            return new GatewayCall(null, ex.Message);
        }
    }

    private async Task<string> ReadProviderAsync(CancellationToken ct)
    {
        if (_provider != null)
            return _provider;
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand(
                "SELECT TOP 1 SmsProvider FROM dbo.SikkhaloySetting", con);
            var value = (await cmd.ExecuteScalarAsync(ct))?.ToString()?.Trim();
            _provider = string.IsNullOrWhiteSpace(value) ? "Novocom" : value;
        }
        catch
        {
            _provider = "Novocom";
        }
        return _provider;
    }

    private static async Task<GatewayCall> SendNovocomAsync(string phone, string text, CancellationToken ct)
    {
        var number = NormalizeNovocomPhone(phone);
        var unicode = RequiresUnicode(text);
        var query = string.Join("&", new Dictionary<string, string>
        {
            ["ApiKey"] = NovocomApiKey,
            ["ClientId"] = NovocomClientId,
            ["SenderId"] = NovocomSenderId,
            ["MobileNumbers"] = number,
            ["Message"] = text ?? "",
            ["Is_Unicode"] = unicode ? "true" : "false"
        }.Select(p => p.Key + "=" + Uri.EscapeDataString(p.Value)));

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var response = await http.GetAsync(NovocomHost + "SendSMS?" + query, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
            return new GatewayCall(null, "Empty response from Novocom SMS service.");

        if (!body.TrimStart().StartsWith('{') && !body.TrimStart().StartsWith('['))
            return new GatewayCall(null, "Novocom SMS error: " + TrimErr(body));

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var errorCode = JsonInt(root, "ErrorCode");
        if (errorCode is > 0)
            return new GatewayCall(null, NovocomError(root));

        if (!root.TryGetProperty("Data", out var data) || data.ValueKind != JsonValueKind.Array || data.GetArrayLength() == 0)
            return new GatewayCall(null, "Novocom response has no message data.");

        var first = data[0];
        var messageErrorCode = JsonInt(first, "MessageErrorCode");
        if (messageErrorCode is > 0)
        {
            var desc = JsonText(first, "MessageErrorDescription");
            return new GatewayCall(null, string.IsNullOrWhiteSpace(desc)
                ? "Sms sending was failed."
                : "Sms sending was failed. Because: " + desc);
        }

        var messageId = JsonText(first, "MessageId");
        if (string.IsNullOrWhiteSpace(messageId))
            return new GatewayCall(null, "Novocom did not return a MessageId.");

        return new GatewayCall(messageId, null);
    }

    private static async Task<GatewayCall> SendBanglaPhoneAsync(string phone, string text, CancellationToken ct)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        var safe = SafePlusText(text);
        var body = "userId=" + Uri.EscapeDataString(BanglaPhoneUser)
                   + "&password=" + Uri.EscapeDataString(BanglaPhonePassword)
                   + "&smsText=" + UrlEncodeBangla(safe)
                   + "&commaSeperatedReceiverNumbers=" + Uri.EscapeDataString(phone);
        using var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
        using var response = await http.PostAsync(BanglaPhoneHost + "sendsms", content, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (BanglaPhoneIsError(json, out var gatewayMessage))
            return new GatewayCall(null, gatewayMessage ?? ("SMS gateway error (" + (int)response.StatusCode + ")."));
        if (!response.IsSuccessStatusCode)
            return new GatewayCall(null, "SMS gateway HTTP " + (int)response.StatusCode
                + (string.IsNullOrWhiteSpace(json) ? "" : ": " + TrimErr(json)));
        return new GatewayCall(string.IsNullOrWhiteSpace(json) ? "Sent" : json, null);
    }

    private static async Task<GatewayCall> SendGreenWebAsync(string phone, string text, CancellationToken ct)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        var safe = SafePlusText(text);
        var body = "token=" + Uri.EscapeDataString(GreenWebApiKey)
                   + "&to=" + Uri.EscapeDataString(phone)
                   + "&message=" + Uri.EscapeDataString(safe);
        using var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
        using var response = await http.PostAsync(GreenWebHost + "api.php?json", content, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode || json.Contains("\"isError\":true", StringComparison.OrdinalIgnoreCase))
            return new GatewayCall(null, string.IsNullOrWhiteSpace(json) ? "GreenWeb SMS failed." : TrimErr(json));
        return new GatewayCall(string.IsNullOrWhiteSpace(json) ? "Sent" : json, null);
    }

    private static string NormalizeNovocomPhone(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new InvalidOperationException("Invalid mobile number.");

        var digits = Regex.Replace(number.Trim(), @"[^\d]", "");
        if (digits.StartsWith("880", StringComparison.Ordinal) && digits.Length == 13)
            return digits;
        if (digits.StartsWith("01", StringComparison.Ordinal) && digits.Length == 11)
            return "88" + digits;
        if (digits.Length == 10 && digits.StartsWith('1'))
            return "880" + digits;
        throw new InvalidOperationException("Invalid mobile number format for Novocom: " + number);
    }

    private static bool RequiresUnicode(string? message) =>
        !string.IsNullOrEmpty(message) && message.Any(c => c > 127);

    private static string SafePlusText(string text) =>
        text.Replace("A+", "A Plus", StringComparison.OrdinalIgnoreCase).Replace("+", " Plus ");

    private static string UrlEncodeBangla(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        var bytes = Encoding.UTF8.GetBytes(text);
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9')
                || b is (byte)'-' or (byte)'_' or (byte)'.' or (byte)'~')
                sb.Append((char)b);
            else if (b == ' ')
                sb.Append('+');
            else
            {
                sb.Append('%');
                sb.Append(b.ToString("X2"));
            }
        }
        return sb.ToString();
    }

    private static bool BanglaPhoneIsError(string json, out string? message)
    {
        message = null;
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            message = JsonText(root, "message") ?? JsonText(root, "Message");
            if (!root.TryGetProperty("isError", out var err))
                return false;
            return err.ValueKind == JsonValueKind.True
                   || (err.ValueKind == JsonValueKind.String
                       && string.Equals(err.GetString(), "true", StringComparison.OrdinalIgnoreCase));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string NovocomError(JsonElement root)
    {
        var description = JsonText(root, "ErrorDescription");
        if (!string.IsNullOrWhiteSpace(description))
            return "Sms sending was failed. Because: " + description;
        return "Sms sending was failed.";
    }

    private static int? JsonInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;

    private static string? JsonText(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? JsonText(value) : null;

    private static string? JsonText(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => value.GetRawText()
    };

    private static string TrimErr(string text)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= 240 ? trimmed : trimmed[..240];
    }

    public readonly record struct GatewayCall(string? Body, string? Error);
}
