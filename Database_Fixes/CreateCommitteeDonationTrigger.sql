-- ? Create TRIGGER to automatically update CommitteeDonation PaidAmount
-- This trigger recalculates PaidAmount from CommitteePaymentRecord table
-- Run this SQL in your database

USE [dbo_EDUCATION]
GO

-- Drop existing trigger if exists
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_CommitteePaymentRecord_UpdateDonation')
    DROP TRIGGER trg_CommitteePaymentRecord_UpdateDonation
GO

-- Create trigger for INSERT
CREATE TRIGGER trg_CommitteePaymentRecord_UpdateDonation
ON CommitteePaymentRecord
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update donations that were affected by INSERT/UPDATE
    UPDATE CD
    SET CD.PaidAmount = ISNULL((
        SELECT SUM(CPR.PaidAmount)
        FROM CommitteePaymentRecord CPR
        WHERE CPR.CommitteeDonationId = CD.CommitteeDonationId
          AND CPR.SchoolId = CD.SchoolID
    ), 0)
    FROM CommitteeDonation CD
    WHERE CD.CommitteeDonationId IN (
        SELECT DISTINCT CommitteeDonationId FROM inserted
        UNION
        SELECT DISTINCT CommitteeDonationId FROM deleted
    );
    
    -- Update CommitteeMember totals
    UPDATE CM
    SET 
        CM.TotalDonation = ISNULL((
            SELECT SUM(CD.Amount)
            FROM CommitteeDonation CD
            WHERE CD.CommitteeMemberId = CM.CommitteeMemberId
              AND CD.SchoolID = CM.SchoolID
        ), 0),
        CM.PaidDonation = ISNULL((
            SELECT SUM(CD.PaidAmount)
            FROM CommitteeDonation CD
            WHERE CD.CommitteeMemberId = CM.CommitteeMemberId
              AND CD.SchoolID = CM.SchoolID
        ), 0),
        CM.DueDonation = ISNULL((
            SELECT SUM(CD.Due)
            FROM CommitteeDonation CD
            WHERE CD.CommitteeMemberId = CM.CommitteeMemberId
              AND CD.SchoolID = CM.SchoolID
        ), 0)
    FROM CommitteeMember CM
    INNER JOIN CommitteeDonation CD ON CM.CommitteeMemberId = CD.CommitteeMemberId AND CM.SchoolID = CD.SchoolID
    WHERE CD.CommitteeDonationId IN (
        SELECT DISTINCT CommitteeDonationId FROM inserted
        UNION
        SELECT DISTINCT CommitteeDonationId FROM deleted
    );
END
GO

-- ? Fix existing data inconsistencies
PRINT 'Fixing existing data...'

-- Recalculate all PaidAmount from CommitteePaymentRecord
UPDATE CD
SET CD.PaidAmount = ISNULL((
    SELECT SUM(CPR.PaidAmount)
    FROM CommitteePaymentRecord CPR
    WHERE CPR.CommitteeDonationId = CD.CommitteeDonationId
      AND CPR.SchoolId = CD.SchoolID
), 0)
FROM CommitteeDonation CD;

-- Recalculate CommitteeMember totals
UPDATE CM
SET 
    CM.TotalDonation = ISNULL((
        SELECT SUM(CD.Amount)
        FROM CommitteeDonation CD
        WHERE CD.CommitteeMemberId = CM.CommitteeMemberId
          AND CD.SchoolID = CM.SchoolID
    ), 0),
    CM.PaidDonation = ISNULL((
        SELECT SUM(CD.PaidAmount)
        FROM CommitteeDonation CD
        WHERE CD.CommitteeMemberId = CM.CommitteeMemberId
          AND CD.SchoolID = CM.SchoolID
    ), 0),
    CM.DueDonation = ISNULL((
        SELECT SUM(CD.Due)
        FROM CommitteeDonation CD
        WHERE CD.CommitteeMemberId = CM.CommitteeMemberId
          AND CD.SchoolID = CM.SchoolID
    ), 0)
FROM CommitteeMember CM;

PRINT 'Trigger created and data fixed successfully!'
GO

-- Test query to verify
SELECT 
    CD.CommitteeDonationId,
    CD.Amount,
    CD.PaidAmount AS CurrentPaidAmount,
    ISNULL((
        SELECT SUM(CPR.PaidAmount)
        FROM CommitteePaymentRecord CPR
        WHERE CPR.CommitteeDonationId = CD.CommitteeDonationId
    ), 0) AS CalculatedPaidAmount,
    CD.Due
FROM CommitteeDonation CD
WHERE CD.SchoolID = 1002 -- Change to your SchoolID
  AND CD.CommitteeMemberId = 1563 -- Change to your test donor
ORDER BY CD.CommitteeDonationId;
