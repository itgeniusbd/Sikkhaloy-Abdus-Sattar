using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Committee;

namespace Sikkhaloy.SyncApi.Services;

public sealed class CommitteeService
{
    private readonly EduConnectionFactory _connections;

    public CommitteeService(EduConnectionFactory connections) => _connections = connections;

    public async Task<CommitteeLookupsDto> GetLookupsAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new CommitteeLookupsDto();
        await using var con = _connections.Create();
        await con.OpenAsync(ct);

        dto.Types.AddRange(await OptionsAsync(con, """
SELECT CommitteeMemberTypeId, CommitteeMemberType
FROM dbo.CommitteeMemberType WHERE SchoolID = @SchoolID ORDER BY CommitteeMemberType
""", session.SchoolID, ct));

        dto.Categories.AddRange(await OptionsAsync(con, """
SELECT CommitteeDonationCategoryId, DonationCategory
FROM dbo.CommitteeDonationCategory WHERE SchoolID = @SchoolID ORDER BY DonationCategory
""", session.SchoolID, ct));

        dto.Members.AddRange(await OptionsAsync(con, """
SELECT CommitteeMemberId, MemberName
FROM dbo.CommitteeMember WHERE SchoolID = @SchoolID ORDER BY MemberName
""", session.SchoolID, ct));

        dto.Years.AddRange(await OptionsAsync(con, """
SELECT EducationYearID, EducationYear
FROM dbo.Education_Year WHERE SchoolID = @SchoolID ORDER BY EducationYearID
""", session.SchoolID, ct));

        await using (var cmd = new SqlCommand("""
SELECT AccountID, AccountName, ISNULL(AccountBalance, 0) AS AccountBalance, ISNULL(Default_Status, N'') AS Default_Status
FROM dbo.Account WHERE SchoolID = @SchoolID
ORDER BY CASE WHEN Default_Status = N'True' THEN 0 ELSE 1 END, AccountName
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Accounts.Add(new CommitteeAccountDto
                {
                    AccountID = I(reader["AccountID"]),
                    AccountName = S(reader["AccountName"]),
                    Balance = Dec(reader["AccountBalance"]),
                    IsDefault = string.Equals(S(reader["Default_Status"]), "True", StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        return dto;
    }

    public async Task<IReadOnlyList<CommitteeMemberTypeDto>> GetMemberTypesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT CommitteeMemberTypeId, CommitteeMemberType, InsertDate
FROM dbo.CommitteeMemberType WHERE SchoolID = @SchoolID ORDER BY CommitteeMemberType
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = new List<CommitteeMemberTypeDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new CommitteeMemberTypeDto
            {
                CommitteeMemberTypeId = I(reader["CommitteeMemberTypeId"]),
                CommitteeMemberType = S(reader["CommitteeMemberType"]),
                InsertDate = Dt(reader["InsertDate"])
            });
        }
        return rows;
    }

    public async Task<CommitteeResult> SaveMemberTypeAsync(SessionSnapshot session, SaveCommitteeMemberTypeRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0) return Fail("cm.needType");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (await NameExistsAsync(con, "SELECT 1 FROM dbo.CommitteeMemberType WHERE SchoolID = @SchoolID AND CommitteeMemberType = @Name AND CommitteeMemberTypeId <> @Id",
                session.SchoolID, name, request!.CommitteeMemberTypeId, ct))
            return Fail("cm.typeExists");

        if (request.CommitteeMemberTypeId > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.CommitteeMemberType SET CommitteeMemberType = @Name
WHERE CommitteeMemberTypeId = @Id AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@Name", name);
            upd.Parameters.AddWithValue("@Id", request.CommitteeMemberTypeId);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(ct);
            return Ok("cm.typeUpdated");
        }

        await using var ins = new SqlCommand("""
INSERT INTO dbo.CommitteeMemberType (SchoolID, RegistrationID, CommitteeMemberType)
VALUES (@SchoolID, @RegID, @Name)
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@RegID", session.RegistrationID);
        ins.Parameters.AddWithValue("@Name", name);
        await ins.ExecuteNonQueryAsync(ct);
        return Ok("cm.typeAdded");
    }

    public async Task<CommitteeResult> DeleteMemberTypeAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("""
DELETE FROM dbo.CommitteeMemberType WHERE CommitteeMemberTypeId = @Id AND SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("cm.typeDeleted");
        }
        catch (SqlException)
        {
            return Fail("cm.typeUsed");
        }
    }

    public async Task<IReadOnlyList<CommitteeMemberDto>> GetMembersAsync(SessionSnapshot session, int typeId, string? q, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT m.CommitteeMemberId, m.CommitteeMemberTypeId, t.CommitteeMemberType, m.MemberName, m.ReferenceBy,
       m.SmsNumber, m.Email, m.Address, ISNULL(m.Status, N'Active') AS Status,
       ISNULL(m.TotalDonation, 0) AS TotalDonation, ISNULL(m.PaidDonation, 0) AS PaidDonation,
       ISNULL(m.DueDonation, 0) AS DueDonation
FROM dbo.CommitteeMember m
INNER JOIN dbo.CommitteeMemberType t ON m.CommitteeMemberTypeId = t.CommitteeMemberTypeId
WHERE m.SchoolID = @SchoolID
  AND (@TypeId = 0 OR m.CommitteeMemberTypeId = @TypeId)
  AND (@Q = N'' OR m.MemberName LIKE N'%' + @Q + N'%' OR m.SmsNumber LIKE N'%' + @Q + N'%')
ORDER BY m.MemberName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@TypeId", typeId);
        cmd.Parameters.AddWithValue("@Q", (q ?? "").Trim());
        var rows = new List<CommitteeMemberDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(ReadMember(reader));
        return rows;
    }

    public async Task<CommitteeResult> SaveMemberAsync(SessionSnapshot session, SaveCommitteeMemberRequest? request, CancellationToken ct)
    {
        var name = (request?.MemberName ?? "").Trim();
        var phone = (request?.SmsNumber ?? "").Trim();
        if (name.Length == 0) return Fail("cm.needName");
        if (request!.CommitteeMemberTypeId <= 0) return Fail("cm.needType");
        if (phone.Length == 0) return Fail("cm.needPhone");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (await PhoneTakenAsync(con, session.SchoolID, phone, request.CommitteeMemberId, ct))
            return Fail("cm.phoneExists");

        var status = string.Equals(request.Status, "Inactive", StringComparison.OrdinalIgnoreCase) ? "Inactive" : "Active";
        if (request.CommitteeMemberId > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.CommitteeMember
SET CommitteeMemberTypeId = @TypeId, MemberName = @Name, ReferenceBy = @Ref, SmsNumber = @Phone,
    Email = @Email, Address = @Address, Status = @Status
WHERE CommitteeMemberId = @Id AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@TypeId", request.CommitteeMemberTypeId);
            upd.Parameters.AddWithValue("@Name", name);
            upd.Parameters.AddWithValue("@Ref", (object?)NullIfEmpty(request.ReferenceBy) ?? DBNull.Value);
            upd.Parameters.AddWithValue("@Phone", phone);
            upd.Parameters.AddWithValue("@Email", (object?)NullIfEmpty(request.Email) ?? DBNull.Value);
            upd.Parameters.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
            upd.Parameters.AddWithValue("@Status", status);
            upd.Parameters.AddWithValue("@Id", request.CommitteeMemberId);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(ct);
            return Ok("cm.memberUpdated", request.CommitteeMemberId);
        }

        await using var ins = new SqlCommand("""
INSERT INTO dbo.CommitteeMember (CommitteeMemberTypeId, RegistrationID, SchoolID, MemberName, ReferenceBy, SmsNumber, Email, Address, Status)
VALUES (@TypeId, @RegID, @SchoolID, @Name, @Ref, @Phone, @Email, @Address, N'Active');
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con);
        ins.Parameters.AddWithValue("@TypeId", request.CommitteeMemberTypeId);
        ins.Parameters.AddWithValue("@RegID", session.RegistrationID);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@Name", name);
        ins.Parameters.AddWithValue("@Ref", (object?)NullIfEmpty(request.ReferenceBy) ?? DBNull.Value);
        ins.Parameters.AddWithValue("@Phone", phone);
        ins.Parameters.AddWithValue("@Email", (object?)NullIfEmpty(request.Email) ?? DBNull.Value);
        ins.Parameters.AddWithValue("@Address", (object?)NullIfEmpty(request.Address) ?? DBNull.Value);
        var id = await ins.ExecuteScalarAsync(ct);
        return Ok("cm.memberAdded", I(id));
    }

    public async Task<IReadOnlyList<DonationCategoryDto>> GetCategoriesAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT CommitteeDonationCategoryId, DonationCategory, InsertDate
FROM dbo.CommitteeDonationCategory WHERE SchoolID = @SchoolID ORDER BY DonationCategory
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var rows = new List<DonationCategoryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new DonationCategoryDto
            {
                CommitteeDonationCategoryId = I(reader["CommitteeDonationCategoryId"]),
                DonationCategory = S(reader["DonationCategory"]),
                InsertDate = Dt(reader["InsertDate"])
            });
        }
        return rows;
    }

    public async Task<CommitteeResult> SaveCategoryAsync(SessionSnapshot session, SaveDonationCategoryRequest? request, CancellationToken ct)
    {
        var name = (request?.Name ?? "").Trim();
        if (name.Length == 0) return Fail("cm.needCategory");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        if (await NameExistsAsync(con, "SELECT 1 FROM dbo.CommitteeDonationCategory WHERE SchoolID = @SchoolID AND DonationCategory = @Name AND CommitteeDonationCategoryId <> @Id",
                session.SchoolID, name, request!.CommitteeDonationCategoryId, ct))
            return Fail("cm.categoryExists");

        if (request.CommitteeDonationCategoryId > 0)
        {
            await using var upd = new SqlCommand("""
UPDATE dbo.CommitteeDonationCategory SET DonationCategory = @Name
WHERE CommitteeDonationCategoryId = @Id AND SchoolID = @SchoolID
""", con);
            upd.Parameters.AddWithValue("@Name", name);
            upd.Parameters.AddWithValue("@Id", request.CommitteeDonationCategoryId);
            upd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await upd.ExecuteNonQueryAsync(ct);
            return Ok("cm.categoryUpdated");
        }

        await using var ins = new SqlCommand("""
INSERT INTO dbo.CommitteeDonationCategory (SchoolID, RegistrationID, DonationCategory)
VALUES (@SchoolID, @RegID, @Name)
""", con);
        ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        ins.Parameters.AddWithValue("@RegID", session.RegistrationID);
        ins.Parameters.AddWithValue("@Name", name);
        await ins.ExecuteNonQueryAsync(ct);
        return Ok("cm.categoryAdded");
    }

    public async Task<CommitteeResult> DeleteCategoryAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand("""
DELETE FROM dbo.CommitteeDonationCategory WHERE CommitteeDonationCategoryId = @Id AND SchoolID = @SchoolID
""", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await cmd.ExecuteNonQueryAsync(ct);
            return Ok("cm.categoryDeleted");
        }
        catch (SqlException)
        {
            return Fail("cm.categoryUsed");
        }
    }

    public async Task<IReadOnlyList<DonorSuggestDto>> SuggestDonorsAsync(SessionSnapshot session, string? q, CancellationToken ct)
    {
        var prefix = (q ?? "").Trim();
        if (prefix.Length == 0) return [];
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
SELECT TOP (8) CommitteeMemberId, MemberName, SmsNumber
FROM dbo.CommitteeMember
WHERE SchoolID = @SchoolID AND (MemberName LIKE @Q + N'%' OR SmsNumber LIKE @Q + N'%')
ORDER BY MemberName
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@Q", prefix);
        var rows = new List<DonorSuggestDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new DonorSuggestDto
            {
                CommitteeMemberId = I(reader["CommitteeMemberId"]),
                MemberName = S(reader["MemberName"]),
                SmsNumber = S(reader["SmsNumber"])
            });
        }
        return rows;
    }

    public async Task<CommitteeResult> AddDonationAsync(SessionSnapshot session, AddDonationRequest? request, CancellationToken ct)
    {
        if (request is null || request.CommitteeMemberId <= 0) return Fail("cm.needDonor");
        if (request.CommitteeDonationCategoryId <= 0) return Fail("cm.needCategory");
        if (request.Amount <= 0) return Fail("cm.needAmount");
        if (request.PaidAmount < 0) return Fail("cm.badPaid");
        if (request.PaidAmount > request.Amount) return Fail("cm.paidOver");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            int donationId;
            await using (var ins = new SqlCommand("""
INSERT INTO dbo.CommitteeDonation (SchoolID, RegistrationID, CommitteeMemberId, CommitteeDonationCategoryId, Amount, Description, PromiseDate)
VALUES (@SchoolID, @RegID, @MemberId, @CatId, @Amount, @Desc, @Promise);
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con, tx))
            {
                ins.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                ins.Parameters.AddWithValue("@RegID", session.RegistrationID);
                ins.Parameters.AddWithValue("@MemberId", request.CommitteeMemberId);
                ins.Parameters.AddWithValue("@CatId", request.CommitteeDonationCategoryId);
                ins.Parameters.AddWithValue("@Amount", request.Amount);
                ins.Parameters.AddWithValue("@Desc", (object?)NullIfEmpty(request.Description) ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Promise", (object?)request.PromiseDate ?? DBNull.Value);
                donationId = I(await ins.ExecuteScalarAsync(ct));
            }

            var receiptId = 0;
            if (request.PaidAmount > 0)
            {
                if (request.AccountId <= 0) { await tx.RollbackAsync(ct); return Fail("cm.needAccount"); }
                receiptId = await InsertReceiptAsync(con, tx, session, request.CommitteeMemberId, request.AccountId, request.PaidDate ?? DateTime.Today, ct);
                await InsertPaymentAsync(con, tx, session, donationId, receiptId, request.PaidAmount, ct);
                await UpdateReceiptTotalAsync(con, tx, receiptId, request.PaidAmount, ct);
            }

            await tx.CommitAsync(ct);
            return new CommitteeResult
            {
                Succeeded = true,
                Message = receiptId > 0 ? "cm.donationPaid" : "cm.donationAdded",
                Id = donationId,
                ReceiptId = receiptId
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<DonationListDto> GetDonationsAsync(SessionSnapshot session, int memberId, int categoryId, string? paid, CancellationToken ct)
    {
        var status = (paid ?? "%").Trim();
        if (status is not "%" and not "1" and not "0") status = "%";
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new DonationListDto();
        await using (var sum = new SqlCommand("""
SELECT SUM(ISNULL(d.Amount, 0)) AS Total, SUM(ISNULL(d.PaidAmount, 0)) AS Paid, SUM(ISNULL(d.Due, 0)) AS Due
FROM dbo.CommitteeDonation d
WHERE d.SchoolID = @SchoolID
  AND (@MemberId = 0 OR d.CommitteeMemberId = @MemberId)
  AND (@CatId = 0 OR d.CommitteeDonationCategoryId = @CatId)
  AND (@Paid = N'%' OR (@Paid = N'1' AND ISNULL(d.Due, 0) <= 0) OR (@Paid = N'0' AND ISNULL(d.Due, 0) > 0))
""", con))
        {
            AddDonationFilters(sum, session.SchoolID, memberId, categoryId, status);
            await using var reader = await sum.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                dto.Total = Dec(reader["Total"]);
                dto.Paid = Dec(reader["Paid"]);
                dto.Due = Dec(reader["Due"]);
            }
        }

        await using var cmd = new SqlCommand("""
SELECT d.CommitteeDonationId, d.CommitteeDonationCategoryId, d.CommitteeMemberId, c.DonationCategory,
       d.Amount, ISNULL(d.PaidAmount, 0) AS PaidAmount, ISNULL(d.Due, 0) AS Due, d.Description,
       d.InsertDate, d.PromiseDate, m.MemberName, m.SmsNumber, t.CommitteeMemberType
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeDonationCategory c ON d.CommitteeDonationCategoryId = c.CommitteeDonationCategoryId
INNER JOIN dbo.CommitteeMember m ON d.CommitteeMemberId = m.CommitteeMemberId
INNER JOIN dbo.CommitteeMemberType t ON m.CommitteeMemberTypeId = t.CommitteeMemberTypeId
WHERE d.SchoolID = @SchoolID
  AND (@MemberId = 0 OR d.CommitteeMemberId = @MemberId)
  AND (@CatId = 0 OR d.CommitteeDonationCategoryId = @CatId)
  AND (@Paid = N'%' OR (@Paid = N'1' AND ISNULL(d.Due, 0) <= 0) OR (@Paid = N'0' AND ISNULL(d.Due, 0) > 0))
ORDER BY d.CommitteeDonationId DESC
""", con);
        AddDonationFilters(cmd, session.SchoolID, memberId, categoryId, status);
        await using var rows = await cmd.ExecuteReaderAsync(ct);
        while (await rows.ReadAsync(ct))
        {
            var paidAmt = Dec(rows["PaidAmount"]);
            dto.Rows.Add(new DonationRowDto
            {
                CommitteeDonationId = I(rows["CommitteeDonationId"]),
                CommitteeDonationCategoryId = I(rows["CommitteeDonationCategoryId"]),
                CommitteeMemberId = I(rows["CommitteeMemberId"]),
                MemberName = S(rows["MemberName"]),
                MemberType = S(rows["CommitteeMemberType"]),
                SmsNumber = S(rows["SmsNumber"]),
                DonationCategory = S(rows["DonationCategory"]),
                Amount = Dec(rows["Amount"]),
                PaidAmount = paidAmt,
                Due = Dec(rows["Due"]),
                Description = S(rows["Description"]),
                InsertDate = Dt(rows["InsertDate"]),
                PromiseDate = Dt(rows["PromiseDate"]),
                CanDelete = paidAmt == 0
            });
        }
        return dto;
    }

    public async Task<CommitteeResult> UpdateDonationAsync(SessionSnapshot session, UpdateDonationRequest? request, CancellationToken ct)
    {
        if (request is null || request.CommitteeDonationId <= 0) return Fail("cm.needDonation");
        if (request.CommitteeDonationCategoryId <= 0) return Fail("cm.needCategory");
        if (request.Amount <= 0) return Fail("cm.needAmount");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
UPDATE dbo.CommitteeDonation
SET CommitteeDonationCategoryId = @CatId,
    Amount = CASE WHEN ISNULL(PaidAmount, 0) > @Amount THEN Amount ELSE @Amount END,
    Description = @Desc
WHERE CommitteeDonationId = @Id AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@CatId", request.CommitteeDonationCategoryId);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Desc", (object?)NullIfEmpty(request.Description) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", request.CommitteeDonationId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        await cmd.ExecuteNonQueryAsync(ct);
        return Ok("cm.donationUpdated");
    }

    public async Task<CommitteeResult> DeleteDonationAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
DELETE FROM dbo.CommitteeDonation
WHERE CommitteeDonationId = @Id AND SchoolID = @SchoolID AND ISNULL(PaidAmount, 0) = 0
""", con);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0 ? Ok("cm.donationDeleted") : Fail("cm.donationPaidLock");
    }

    public async Task<CollectPageDto> GetCollectAsync(SessionSnapshot session, int memberId, CancellationToken ct)
    {
        var dto = new CollectPageDto();
        if (memberId <= 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var mem = new SqlCommand("""
SELECT m.CommitteeMemberId, m.CommitteeMemberTypeId, t.CommitteeMemberType, m.MemberName, m.ReferenceBy,
       m.SmsNumber, m.Email, m.Address, ISNULL(m.Status, N'Active') AS Status,
       ISNULL(m.TotalDonation, 0) AS TotalDonation, ISNULL(m.PaidDonation, 0) AS PaidDonation,
       ISNULL(m.DueDonation, 0) AS DueDonation
FROM dbo.CommitteeMember m
INNER JOIN dbo.CommitteeMemberType t ON m.CommitteeMemberTypeId = t.CommitteeMemberTypeId
WHERE m.SchoolID = @SchoolID AND m.CommitteeMemberId = @Id
""", con))
        {
            mem.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            mem.Parameters.AddWithValue("@Id", memberId);
            await using var reader = await mem.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                dto.Member = ReadMember(reader);
        }

        await using (var due = new SqlCommand("""
SELECT d.CommitteeDonationId, c.DonationCategory, d.Description, d.Amount,
       ISNULL(d.PaidAmount, 0) AS PaidAmount, ISNULL(d.Due, 0) AS Due
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeDonationCategory c ON d.CommitteeDonationCategoryId = c.CommitteeDonationCategoryId
WHERE d.SchoolID = @SchoolID AND d.CommitteeMemberId = @Id AND ISNULL(d.Due, 0) > 0
ORDER BY d.CommitteeDonationId
""", con))
        {
            due.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            due.Parameters.AddWithValue("@Id", memberId);
            await using var reader = await due.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var dueAmt = Dec(reader["Due"]);
                dto.Dues.Add(new DonationDueDto
                {
                    CommitteeDonationId = I(reader["CommitteeDonationId"]),
                    DonationCategory = S(reader["DonationCategory"]),
                    Description = S(reader["Description"]),
                    Amount = Dec(reader["Amount"]),
                    PaidAmount = Dec(reader["PaidAmount"]),
                    Due = dueAmt,
                    CollectAmount = dueAmt
                });
            }
        }

        await using (var rec = new SqlCommand("""
SELECT CommitteeMoneyReceiptId, CommitteeMoneyReceiptSn, ISNULL(TotalAmount, 0) AS TotalAmount, PaidDate
FROM dbo.CommitteeMoneyReceipt
WHERE SchoolId = @SchoolID AND EducationYearId = @YearId AND CommitteeMemberId = @Id
ORDER BY PaidDate DESC
""", con))
        {
            rec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            rec.Parameters.AddWithValue("@YearId", session.EducationYearID);
            rec.Parameters.AddWithValue("@Id", memberId);
            await using var reader = await rec.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                dto.Receipts.Add(new MemberReceiptDto
                {
                    CommitteeMoneyReceiptId = I(reader["CommitteeMoneyReceiptId"]),
                    CommitteeMoneyReceiptSn = I(reader["CommitteeMoneyReceiptSn"]),
                    TotalAmount = Dec(reader["TotalAmount"]),
                    PaidDate = Dt(reader["PaidDate"])
                });
            }
        }
        return dto;
    }

    public async Task<CommitteeResult> CollectAsync(SessionSnapshot session, CollectDonationRequest? request, CancellationToken ct)
    {
        if (request is null || request.CommitteeMemberId <= 0) return Fail("cm.needDonor");
        if (request.AccountId <= 0) return Fail("cm.needAccount");
        var lines = request.Lines.Where(x => x.CommitteeDonationId > 0 && x.PaidAmount > 0).ToList();
        if (lines.Count == 0) return Fail("cm.needCollect");

        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            foreach (var line in lines)
            {
                await using var due = new SqlCommand("""
SELECT ISNULL(Due, 0) FROM dbo.CommitteeDonation
WHERE SchoolID = @SchoolID AND CommitteeDonationId = @Id AND CommitteeMemberId = @MemberId
""", con, tx);
                due.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                due.Parameters.AddWithValue("@Id", line.CommitteeDonationId);
                due.Parameters.AddWithValue("@MemberId", request.CommitteeMemberId);
                var dueAmt = Dec(await due.ExecuteScalarAsync(ct));
                if (line.PaidAmount > dueAmt)
                {
                    await tx.RollbackAsync(ct);
                    return Fail("cm.paidOver");
                }
            }

            var receiptId = await InsertReceiptAsync(con, tx, session, request.CommitteeMemberId, request.AccountId, request.PaidDate ?? DateTime.Today, ct);
            decimal total = 0;
            foreach (var line in lines)
            {
                await InsertPaymentAsync(con, tx, session, line.CommitteeDonationId, receiptId, line.PaidAmount, ct);
                total += line.PaidAmount;
            }
            await UpdateReceiptTotalAsync(con, tx, receiptId, total, ct);
            await tx.CommitAsync(ct);
            return new CommitteeResult { Succeeded = true, Message = "cm.collected", ReceiptId = receiptId };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<PaymentRecordListDto> GetPaymentsAsync(
        SessionSnapshot session, int yearId, int categoryId, int memberId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        var dto = new PaymentRecordListDto();
        const string filter = """
WHERE r.SchoolId = @SchoolID
  AND (@YearId = 0 OR r.EducationYearId = @YearId)
  AND (@MemberId = 0 OR r.CommitteeMemberId = @MemberId)
  AND CAST(r.PaidDate AS date) BETWEEN ISNULL(@From, '1000-01-01') AND ISNULL(@To, '3000-01-01')
  AND (@CatId = 0 OR r.CommitteeMoneyReceiptId IN (
        SELECT p.CommitteeMoneyReceiptId
        FROM dbo.CommitteePaymentRecord p
        INNER JOIN dbo.CommitteeDonation d ON p.CommitteeDonationId = d.CommitteeDonationId
        WHERE d.CommitteeDonationCategoryId = @CatId AND d.SchoolID = @SchoolID))
""";
        await using (var sum = new SqlCommand($"""
SELECT COALESCE(SUM(r.TotalAmount), 0) AS Total
FROM dbo.CommitteeMoneyReceipt r
{filter}
""", con))
        {
            AddPaymentFilters(sum, session.SchoolID, yearId, categoryId, memberId, from, to);
            dto.Total = Dec(await sum.ExecuteScalarAsync(ct));
        }

        await using var cmd = new SqlCommand($"""
SELECT r.CommitteeMoneyReceiptId, r.CommitteeMoneyReceiptSn, ISNULL(r.TotalAmount, 0) AS TotalAmount, r.PaidDate,
       m.MemberName, m.SmsNumber, t.CommitteeMemberType, a.AccountName
FROM dbo.CommitteeMoneyReceipt r
INNER JOIN dbo.CommitteeMember m ON r.CommitteeMemberId = m.CommitteeMemberId
INNER JOIN dbo.CommitteeMemberType t ON m.CommitteeMemberTypeId = t.CommitteeMemberTypeId
INNER JOIN dbo.Account a ON r.AccountId = a.AccountID
{filter}
ORDER BY r.PaidDate DESC, r.CommitteeMoneyReceiptId DESC
""", con);
        AddPaymentFilters(cmd, session.SchoolID, yearId, categoryId, memberId, from, to);
        var rows = new List<PaymentRecordRowDto>();
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new PaymentRecordRowDto
                {
                    CommitteeMoneyReceiptId = I(reader["CommitteeMoneyReceiptId"]),
                    CommitteeMoneyReceiptSn = I(reader["CommitteeMoneyReceiptSn"]),
                    MemberName = S(reader["MemberName"]),
                    MemberType = S(reader["CommitteeMemberType"]),
                    SmsNumber = S(reader["SmsNumber"]),
                    AccountName = S(reader["AccountName"]),
                    TotalAmount = Dec(reader["TotalAmount"]),
                    PaidDate = Dt(reader["PaidDate"])
                });
            }
        }

        if (rows.Count > 0)
        {
            var ids = string.Join(",", rows.Select(x => x.CommitteeMoneyReceiptId));
            await using var lines = new SqlCommand($"""
SELECT p.CommitteeMoneyReceiptId, c.DonationCategory, d.Description, p.PaidAmount
FROM dbo.CommitteePaymentRecord p
INNER JOIN dbo.CommitteeDonation d ON p.CommitteeDonationId = d.CommitteeDonationId
INNER JOIN dbo.CommitteeDonationCategory c ON d.CommitteeDonationCategoryId = c.CommitteeDonationCategoryId
WHERE p.CommitteeMoneyReceiptId IN ({ids})
""", con);
            var map = rows.ToDictionary(x => x.CommitteeMoneyReceiptId);
            await using var reader = await lines.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = I(reader["CommitteeMoneyReceiptId"]);
                if (!map.TryGetValue(id, out var row)) continue;
                var cat = S(reader["DonationCategory"]);
                var desc = S(reader["Description"]);
                var amt = Dec(reader["PaidAmount"]);
                var part = string.IsNullOrWhiteSpace(desc) ? $"{cat} : {amt:0.##}" : $"{cat}, {desc} : {amt:0.##}";
                row.Details = string.IsNullOrWhiteSpace(row.Details) ? part : row.Details + "\n" + part;
            }
        }

        dto.Rows = rows;
        return dto;
    }

    public async Task<UnpaidReceiptDto> GetUnpaidAsync(SessionSnapshot session, string? sn, CancellationToken ct)
    {
        var dto = new UnpaidReceiptDto();
        var receiptSn = (sn ?? "").Trim();
        if (receiptSn.Length == 0) return dto;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using (var cmd = new SqlCommand("""
SELECT r.CommitteeMoneyReceiptId, r.CommitteeMoneyReceiptSn, ISNULL(r.TotalAmount, 0) AS TotalAmount, r.PaidDate,
       m.MemberName, m.SmsNumber, m.Address, ISNULL(m.TotalDonation, 0) AS TotalDonation, ISNULL(m.PaidDonation, 0) AS PaidDonation,
       t.CommitteeMemberType, a.AccountName, ISNULL(ad.FirstName, N'') + N' ' + ISNULL(ad.LastName, N'') AS ReceivedBy
FROM dbo.CommitteeMoneyReceipt r
INNER JOIN dbo.CommitteeMember m ON r.CommitteeMemberId = m.CommitteeMemberId
INNER JOIN dbo.CommitteeMemberType t ON m.CommitteeMemberTypeId = t.CommitteeMemberTypeId
INNER JOIN dbo.Account a ON r.AccountId = a.AccountID
LEFT JOIN dbo.Admin ad ON r.RegistrationId = ad.RegistrationID
WHERE r.SchoolId = @SchoolID AND CAST(r.CommitteeMoneyReceiptSn AS nvarchar(50)) = @Sn
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Sn", receiptSn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return dto;
            dto.Found = true;
            dto.CommitteeMoneyReceiptId = I(reader["CommitteeMoneyReceiptId"]);
            dto.CommitteeMoneyReceiptSn = I(reader["CommitteeMoneyReceiptSn"]);
            dto.MemberName = S(reader["MemberName"]);
            dto.MemberType = S(reader["CommitteeMemberType"]);
            dto.SmsNumber = S(reader["SmsNumber"]);
            dto.Address = S(reader["Address"]);
            dto.TotalDonation = Dec(reader["TotalDonation"]);
            dto.PaidDonation = Dec(reader["PaidDonation"]);
            dto.AccountName = S(reader["AccountName"]);
            dto.TotalAmount = Dec(reader["TotalAmount"]);
            dto.PaidDate = Dt(reader["PaidDate"]);
            dto.ReceivedBy = S(reader["ReceivedBy"]).Trim();
        }

        dto.Lines.AddRange(await ReceiptLinesAsync(con, session.SchoolID, dto.CommitteeMoneyReceiptId, ct));
        return dto;
    }

    public async Task<CommitteeResult> UnpaidAsync(SessionSnapshot session, string? sn, CancellationToken ct)
    {
        var receiptSn = (sn ?? "").Trim();
        if (receiptSn.Length == 0) return Fail("cm.needReceipt");
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync(ct);
        try
        {
            await using (var delPay = new SqlCommand("""
DELETE FROM dbo.CommitteePaymentRecord
FROM dbo.CommitteeMoneyReceipt r
INNER JOIN dbo.CommitteePaymentRecord p ON r.CommitteeMoneyReceiptId = p.CommitteeMoneyReceiptId
WHERE r.SchoolId = @SchoolID AND CAST(r.CommitteeMoneyReceiptSn AS nvarchar(50)) = @Sn
""", con, tx))
            {
                delPay.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                delPay.Parameters.AddWithValue("@Sn", receiptSn);
                await delPay.ExecuteNonQueryAsync(ct);
            }

            await using var delRec = new SqlCommand("""
DELETE FROM dbo.CommitteeMoneyReceipt
WHERE SchoolId = @SchoolID AND CAST(CommitteeMoneyReceiptSn AS nvarchar(50)) = @Sn
""", con, tx);
            delRec.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            delRec.Parameters.AddWithValue("@Sn", receiptSn);
            var n = await delRec.ExecuteNonQueryAsync(ct);
            await tx.CommitAsync(ct);
            return n > 0 ? Ok("cm.unpaidDone") : Fail("cm.receiptMissing");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<DonationReceiptDto?> GetReceiptAsync(SessionSnapshot session, int id, CancellationToken ct)
    {
        if (id <= 0) return null;
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        DonationReceiptDto? dto = null;
        await using (var cmd = new SqlCommand("""
SELECT r.CommitteeMoneyReceiptId, r.CommitteeMoneyReceiptSn, ISNULL(r.TotalAmount, 0) AS TotalAmount, r.PaidDate,
       m.MemberName, m.SmsNumber, m.Address, t.CommitteeMemberType, a.AccountName, y.EducationYear,
       ISNULL(ad.FirstName, N'') + N' ' + ISNULL(ad.LastName, N'') AS ReceivedBy, r.CommitteeMemberId
FROM dbo.CommitteeMoneyReceipt r
INNER JOIN dbo.CommitteeMember m ON r.CommitteeMemberId = m.CommitteeMemberId
LEFT JOIN dbo.CommitteeMemberType t ON m.CommitteeMemberTypeId = t.CommitteeMemberTypeId
LEFT JOIN dbo.Account a ON r.AccountId = a.AccountID
LEFT JOIN dbo.Education_Year y ON r.EducationYearId = y.EducationYearID
LEFT JOIN dbo.Admin ad ON r.RegistrationId = ad.RegistrationID
WHERE r.SchoolId = @SchoolID AND r.CommitteeMoneyReceiptId = @Id
""", con))
        {
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;
            dto = new DonationReceiptDto
            {
                CommitteeMoneyReceiptId = I(reader["CommitteeMoneyReceiptId"]),
                CommitteeMoneyReceiptSn = I(reader["CommitteeMoneyReceiptSn"]),
                MemberName = S(reader["MemberName"]),
                MemberType = S(reader["CommitteeMemberType"]),
                SmsNumber = S(reader["SmsNumber"]),
                Address = S(reader["Address"]),
                AccountName = S(reader["AccountName"]),
                EducationYear = S(reader["EducationYear"]),
                TotalAmount = Dec(reader["TotalAmount"]),
                PaidDate = Dt(reader["PaidDate"]),
                ReceivedBy = S(reader["ReceivedBy"]).Trim()
            };
            var memberId = I(reader["CommitteeMemberId"]);
            await reader.CloseAsync();
            dto.Lines.AddRange(await ReceiptLinesAsync(con, session.SchoolID, id, ct));
            dto.CurrentDues.AddRange(await CurrentDuesAsync(con, session.SchoolID, memberId, ct));
        }
        return dto;
    }

    private static async Task<int> InsertReceiptAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, int memberId, int accountId, DateTime paidDate, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.CommitteeMoneyReceipt (RegistrationId, SchoolId, CommitteeMemberId, EducationYearId, AccountId, CommitteeMoneyReceiptSn, PaidDate)
VALUES (@RegID, @SchoolID, @MemberId, @YearId, @AccountId, dbo.F_CommitteeMoneyReceiptSn(@SchoolID), @PaidDate);
SELECT CAST(SCOPE_IDENTITY() AS int);
""", con, tx);
        cmd.Parameters.AddWithValue("@RegID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@MemberId", memberId);
        cmd.Parameters.AddWithValue("@YearId", session.EducationYearID);
        cmd.Parameters.AddWithValue("@AccountId", accountId);
        cmd.Parameters.AddWithValue("@PaidDate", paidDate);
        return I(await cmd.ExecuteScalarAsync(ct));
    }

    private static async Task InsertPaymentAsync(
        SqlConnection con, SqlTransaction tx, SessionSnapshot session, int donationId, int receiptId, decimal paid, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
INSERT INTO dbo.CommitteePaymentRecord (SchoolId, RegistrationId, CommitteeDonationId, CommitteeMoneyReceiptId, PaidAmount)
VALUES (@SchoolID, @RegID, @DonationId, @ReceiptId, @Paid)
""", con, tx);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@RegID", session.RegistrationID);
        cmd.Parameters.AddWithValue("@DonationId", donationId);
        cmd.Parameters.AddWithValue("@ReceiptId", receiptId);
        cmd.Parameters.AddWithValue("@Paid", paid);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpdateReceiptTotalAsync(SqlConnection con, SqlTransaction tx, int receiptId, decimal total, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
UPDATE dbo.CommitteeMoneyReceipt SET TotalAmount = @Total WHERE CommitteeMoneyReceiptId = @Id
""", con, tx);
        cmd.Parameters.AddWithValue("@Total", total);
        cmd.Parameters.AddWithValue("@Id", receiptId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<List<ReceiptLineDto>> ReceiptLinesAsync(SqlConnection con, int schoolId, int receiptId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT c.DonationCategory, d.Description, p.PaidAmount
FROM dbo.CommitteePaymentRecord p
INNER JOIN dbo.CommitteeDonation d ON p.CommitteeDonationId = d.CommitteeDonationId
INNER JOIN dbo.CommitteeDonationCategory c ON d.CommitteeDonationCategoryId = c.CommitteeDonationCategoryId
WHERE p.SchoolId = @SchoolID AND p.CommitteeMoneyReceiptId = @Id
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Id", receiptId);
        var rows = new List<ReceiptLineDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new ReceiptLineDto
            {
                DonationCategory = S(reader["DonationCategory"]),
                Description = S(reader["Description"]),
                PaidAmount = Dec(reader["PaidAmount"])
            });
        }
        return rows;
    }

    private static async Task<List<ReceiptLineDto>> CurrentDuesAsync(SqlConnection con, int schoolId, int memberId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT c.DonationCategory, d.Description, ISNULL(d.Due, 0) AS Due
FROM dbo.CommitteeDonation d
INNER JOIN dbo.CommitteeDonationCategory c ON d.CommitteeDonationCategoryId = c.CommitteeDonationCategoryId
WHERE d.SchoolID = @SchoolID AND d.CommitteeMemberId = @Id AND ISNULL(d.Due, 0) > 0
  AND d.PromiseDate IS NOT NULL AND CAST(d.PromiseDate AS date) < CAST(GETDATE() AS date)
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Id", memberId);
        var rows = new List<ReceiptLineDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new ReceiptLineDto
            {
                DonationCategory = S(reader["DonationCategory"]),
                Description = S(reader["Description"]),
                Due = Dec(reader["Due"])
            });
        }
        return rows;
    }

    private static async Task<bool> PhoneTakenAsync(SqlConnection con, int schoolId, string phone, int exceptId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT COUNT(*) FROM dbo.CommitteeMember
WHERE SchoolID = @SchoolID AND LTRIM(RTRIM(SmsNumber)) = @Phone AND CommitteeMemberId <> @Id
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Phone", phone);
        cmd.Parameters.AddWithValue("@Id", exceptId);
        return I(await cmd.ExecuteScalarAsync(ct)) > 0;
    }

    private static async Task<bool> NameExistsAsync(SqlConnection con, string sql, int schoolId, string name, int exceptId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Id", exceptId);
        return await cmd.ExecuteScalarAsync(ct) is not null and not DBNull;
    }

    private static async Task<List<CommitteeOptionDto>> OptionsAsync(SqlConnection con, string sql, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var rows = new List<CommitteeOptionDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            rows.Add(new CommitteeOptionDto { Id = I(reader[0]), Name = S(reader[1]) });
        return rows;
    }

    private static CommitteeMemberDto ReadMember(SqlDataReader reader) => new()
    {
        CommitteeMemberId = I(reader["CommitteeMemberId"]),
        CommitteeMemberTypeId = I(reader["CommitteeMemberTypeId"]),
        MemberType = S(reader["CommitteeMemberType"]),
        MemberName = S(reader["MemberName"]),
        ReferenceBy = S(reader["ReferenceBy"]),
        SmsNumber = S(reader["SmsNumber"]),
        Email = S(reader["Email"]),
        Address = S(reader["Address"]),
        Status = S(reader["Status"]),
        TotalDonation = Dec(reader["TotalDonation"]),
        PaidDonation = Dec(reader["PaidDonation"]),
        DueDonation = Dec(reader["DueDonation"])
    };

    private static void AddDonationFilters(SqlCommand cmd, int schoolId, int memberId, int categoryId, string paid)
    {
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@MemberId", memberId);
        cmd.Parameters.AddWithValue("@CatId", categoryId);
        cmd.Parameters.AddWithValue("@Paid", paid);
    }

    private static void AddPaymentFilters(SqlCommand cmd, int schoolId, int yearId, int categoryId, int memberId, DateTime? from, DateTime? to)
    {
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@YearId", yearId);
        cmd.Parameters.AddWithValue("@CatId", categoryId);
        cmd.Parameters.AddWithValue("@MemberId", memberId);
        cmd.Parameters.AddWithValue("@From", (object?)from?.Date ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@To", (object?)to?.Date ?? DBNull.Value);
    }

    private static CommitteeResult Ok(string message, int id = 0) =>
        new() { Succeeded = true, Message = message, Id = id };

    private static CommitteeResult Fail(string error) => new() { Error = error };

    private static string S(object? value) => value is null or DBNull ? "" : value.ToString() ?? "";
    private static int I(object? value) => value is null or DBNull ? 0 : Convert.ToInt32(value);
    private static decimal Dec(object? value) => value is null or DBNull ? 0 : Convert.ToDecimal(value);
    private static DateTime? Dt(object? value) => value is DateTime d ? d : value is null or DBNull ? null : Convert.ToDateTime(value);
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
