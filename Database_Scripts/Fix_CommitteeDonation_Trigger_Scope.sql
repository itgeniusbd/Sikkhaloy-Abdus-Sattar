SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.Tr_CommitteeDonation', N'TR') IS NOT NULL
BEGIN
    EXEC(N'
ALTER TRIGGER [dbo].[Tr_CommitteeDonation]
ON [dbo].[CommitteeDonation]
AFTER INSERT, DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CommitteeMember
    SET TotalDonation = T.Amount, PaidDonation = T.PaidAmount
    FROM CommitteeMember
    INNER JOIN (
        SELECT d.CommitteeMemberId, SUM(d.PaidAmount) AS PaidAmount, SUM(d.Amount) AS Amount
        FROM dbo.CommitteeDonation d
        WHERE d.CommitteeMemberId IN (
            SELECT CommitteeMemberId FROM INSERTED
            UNION
            SELECT CommitteeMemberId FROM DELETED)
        GROUP BY d.CommitteeMemberId
    ) AS T ON CommitteeMember.CommitteeMemberId = T.CommitteeMemberId;
END
');
END
GO
