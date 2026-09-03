using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;

namespace Sikkhaloy.SyncApi.Services;

public sealed class BalanceSubmissionService
{
    private static readonly ConcurrentDictionary<string, OtpEntry> OtpCache = new();
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    private readonly EduConnectionFactory _connections;
    private readonly PaymentSmsService _sms;
    private readonly ReportsService _reports;

    public BalanceSubmissionService(EduConnectionFactory connections, PaymentSmsService sms, ReportsService reports)
    {
        _connections = connections;
        _sms = sms;
        _reports = reports;
    }

    public Task<BalanceRemainingDto> GetRemainingAsync(
        SessionSnapshot session, DateTime? from, DateTime? to, CancellationToken cancellationToken) =>
        _reports.GetMyRemainingBalanceAsync(session, session.RegistrationID, from, to, cancellationToken);

    public async Task<AccountsResult> SendOtpAsync(SessionSnapshot session, BalanceSubmitOtpRequest request, CancellationToken cancellationToken)
    {
        var phone = NormalizePhone(request.Phone);
        if (!IsValidBdMobile(phone))
            return new AccountsResult { Error = "rpt.invalidPhone" };

        var key = CacheKey(session);
        if (OtpCache.TryGetValue(key, out var existing)
            && DateTime.UtcNow - existing.SentAt < ResendCooldown)
        {
            var wait = (int)Math.Ceiling((ResendCooldown - (DateTime.UtcNow - existing.SentAt)).TotalSeconds);
            return new AccountsResult { Error = "rpt.otpWait", Count = wait };
        }

        var otp = Random.Shared.Next(100000, 999999).ToString();
        var school = string.IsNullOrWhiteSpace(session.SchoolName) ? "School" : session.SchoolName.Trim();
        var message = $"Your OTP for balance submission is: {otp}. Valid for 5 minutes. - {school}";
        var sent = await _sms.SendPlainSmsAsync(session, phone, message, "Balance Submission OTP", cancellationToken);
        if (!sent.Succeeded)
            return sent;

        OtpCache[key] = new OtpEntry(otp, phone, DateTime.UtcNow);
        return new AccountsResult { Succeeded = true };
    }

    public async Task<AccountsResult> SubmitAsync(SessionSnapshot session, BalanceSubmitRequest request, CancellationToken cancellationToken)
    {
        var phone = NormalizePhone(request.Phone);
        if (!IsValidBdMobile(phone))
            return new AccountsResult { Error = "rpt.invalidPhone" };

        var otp = (request.Otp ?? "").Trim();
        if (otp.Length != 6)
            return new AccountsResult { Error = "rpt.otpInvalid" };

        var key = CacheKey(session);
        if (!OtpCache.TryGetValue(key, out var entry))
            return new AccountsResult { Error = "rpt.otpMissing" };

        if (!string.Equals(entry.Phone, phone, StringComparison.Ordinal))
            return new AccountsResult { Error = "rpt.phoneChanged" };

        if (DateTime.UtcNow - entry.SentAt > OtpLifetime)
        {
            OtpCache.TryRemove(key, out _);
            return new AccountsResult { Error = "rpt.otpExpired" };
        }

        if (!string.Equals(entry.Otp, otp, StringComparison.Ordinal))
            return new AccountsResult { Error = "rpt.otpInvalid" };

        if (request.Amount <= 0)
            return new AccountsResult { Error = "rpt.amountInvalid" };

        var remaining = await _reports.GetMyRemainingBalanceAsync(
            session, session.RegistrationID, request.PeriodFrom, request.PeriodTo, cancellationToken);
        if (remaining.Remaining <= 0)
            return new AccountsResult { Error = "rpt.noBalance" };

        if (request.Amount > remaining.Remaining)
            return new AccountsResult { Error = "rpt.amountExceeds", Count = (int)Math.Truncate(remaining.Remaining) };

        var method = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod.Trim();
        var receivedBy = string.IsNullOrWhiteSpace(request.ReceivedBy) ? null : request.ReceivedBy.Trim();
        var remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks.Trim();
        var submitDate = request.SubmissionDate.Date;
        if (submitDate.Year < 2000)
            submitDate = DateTime.Today;

        await using var con = _connections.Create();
        await con.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.User_Balance_Submission
    (SchoolID, RegistrationID, SubmissionAmount, SubmissionDate, ReceivedBy, ReceiverPhone, PaymentMethod, Remarks, CreatedBy)
VALUES
    (@SchoolID, @RegistrationID, @Amount, @Date, @ReceivedBy, @Phone, @Method, @Remarks, @CreatedBy)
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegistrationID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Date", submitDate);
        cmd.Parameters.AddWithValue("@ReceivedBy", (object?)receivedBy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Phone", phone);
        cmd.Parameters.AddWithValue("@Method", method);
        cmd.Parameters.AddWithValue("@Remarks", (object?)remarks ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedBy", session.RegistrationID);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        OtpCache.TryRemove(key, out _);
        return new AccountsResult { Succeeded = true };
    }

    private static string CacheKey(SessionSnapshot session) => $"{session.SchoolID}:{session.RegistrationID}";

    private static string NormalizePhone(string? phone) => (phone ?? "").Trim();

    private static bool IsValidBdMobile(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits.StartsWith("01", StringComparison.Ordinal)
               || digits.Length == 13 && digits.StartsWith("8801", StringComparison.Ordinal);
    }

    private sealed record OtpEntry(string Otp, string Phone, DateTime SentAt);
}
