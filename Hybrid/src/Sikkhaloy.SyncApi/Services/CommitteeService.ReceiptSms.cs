using System.Text;
using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Sms;

namespace Sikkhaloy.SyncApi.Services;

public sealed partial class CommitteeService
{
    public Task<AccountsResult> SendDonorReceiptSmsAsync(SessionSnapshot session, int receiptId, CancellationToken ct) =>
        SendDonorReceiptSmsAsync(session, receiptId, null, ct);

    public async Task<AccountsResult> SendDonorReceiptSmsAsync(
        SessionSnapshot session, int receiptId, DonorReceiptSmsRequest? request, CancellationToken ct)
    {
        if (receiptId <= 0)
            return new AccountsResult { Error = "acc.needReceipt" };

        var lang = await _templates.ResolveDonorPaymentLangAsync(session.SchoolID, request?.Lang, ct);

        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        string donorName = "", phone = "", receiptNo = "";
        int memberId = 0;
        decimal amount = 0;
        var details = new StringBuilder();

        await using (var cmd = new SqlCommand("""
SELECT r.CommitteeMoneyReceiptSn, ISNULL(r.TotalAmount, 0) AS TotalAmount, r.CommitteeMemberId,
       m.MemberName, ISNULL(m.SmsNumber, N'') AS SmsNumber
FROM dbo.CommitteeMoneyReceipt r
INNER JOIN dbo.CommitteeMember m ON r.CommitteeMemberId = m.CommitteeMemberId
WHERE r.SchoolId = @SchoolID AND r.CommitteeMoneyReceiptId = @Id
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Id", receiptId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return new AccountsResult { Error = "cm.receiptMissing" };
            receiptNo = I(reader["CommitteeMoneyReceiptSn"]).ToString();
            amount = Dec(reader["TotalAmount"]);
            memberId = I(reader["CommitteeMemberId"]);
            donorName = S(reader["MemberName"]);
            phone = S(reader["SmsNumber"]);
        }

        var lines = await ReceiptLinesAsync(con, session.SchoolID, receiptId, ct);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.DonationCategory)) continue;
            details.Append(", ").Append(line.DonationCategory);
            if (!string.IsNullOrWhiteSpace(line.Description))
                details.Append(": ").Append(line.Description);
        }

        var dues = await CurrentDuesAsync(con, session.SchoolID, memberId, ct);
        var currentDue = dues.Sum(x => x.Due);

        phone = phone.Trim();
        if (!IsValidBdMobile(phone))
            return new AccountsResult { Error = "acc.smsNoPhone" };

        var school = await SessionSchool.ResolveNameAsync(session, con, ct);
        var template = await _templates.ResolveDonorPaymentTemplateAsync(session.SchoolID, lang, ct);
        var msg = BuildDonorPaymentMessage(template, lang, donorName, amount, receiptNo, details.ToString(), currentDue, school);
        var count = SmsCount(msg);
        var balance = await ReadSmsBalanceAsync(con, session.SchoolID, ct);
        if (balance < count)
            return new AccountsResult { Error = "acc.smsLow", Count = balance };

        var resp = await _gateway.SendAsync(phone, msg, ct);
        if (!string.IsNullOrWhiteSpace(resp.Error))
            return new AccountsResult { Error = "acc.smsFail" };

        var smsId = Guid.NewGuid();
        await using (var ins = new SqlCommand("""
INSERT INTO dbo.SMS_Send_Record
    (SMS_Send_ID, PhoneNumber, TextSMS, TextCount, SMSCount, PurposeOfSMS, Status, Date, SMS_Response)
VALUES (@ID, @Phone, @Text, @Len, @Count, N'Donor Payment', @Status, GETDATE(), @Resp)
""", con))
        {
            ins.Parameters.AddWithValue("@ID", smsId);
            ins.Parameters.AddWithValue("@Phone", phone);
            ins.Parameters.AddWithValue("@Text", msg);
            ins.Parameters.AddWithValue("@Len", msg.Length);
            ins.Parameters.AddWithValue("@Count", count);
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
            other.Parameters.AddWithValue("@MemberId", memberId);
            await other.ExecuteNonQueryAsync(ct);
        }

        return new AccountsResult { Succeeded = true, Saved = count };
    }

    private static string BuildDonorPaymentMessage(
        string? template, string lang, string donorName, decimal amount, string receiptNo, string paymentDetails, decimal currentDue, string school)
    {
        var paid = FmtDue(amount);
        var dueText = FmtDue(currentDue);
        var payFor = (paymentDetails ?? "").Trim().TrimStart(',').Trim();
        if (!string.IsNullOrWhiteSpace(template))
        {
            var msg = template
                .Replace("{DonorName}", donorName)
                .Replace("{Amount}", paid)
                .Replace("{ReceiptNo}", receiptNo)
                .Replace("{CurrentDue}", dueText)
                .Replace("{SchoolName}", school);
            if (!string.IsNullOrWhiteSpace(payFor))
                msg = msg.Replace("{PaymentDetails}", payFor);
            else
                msg = msg.Replace(", {PaymentDetails}", "").Replace("{PaymentDetails}", "");
            return msg;
        }

        if (lang == "en")
        {
            var message = $"Congrats! {donorName}. You've Paid: {paid} Tk. Receipt No: {receiptNo}";
            if (!string.IsNullOrWhiteSpace(payFor))
                message += $". Details: {payFor}";
            if (currentDue > 0)
                message += $". Current Due: {dueText} Tk";
            message += $". Regards, {school}";
            return message;
        }

        var bn = $"অভিনন্দন! {donorName}. আপনি: {paid} টাকা পরিশোধ করেছেন, রিসিট নম্বর: {receiptNo}";
        if (!string.IsNullOrWhiteSpace(payFor))
            bn += payFor;
        if (currentDue > 0)
            bn += $". বর্তমান বকেয়া: {dueText} টাকা";
        bn += $". ধন্যবাদ, {school}";
        return bn;
    }
}
