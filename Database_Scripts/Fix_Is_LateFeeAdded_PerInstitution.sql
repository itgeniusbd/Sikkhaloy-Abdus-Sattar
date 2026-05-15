-- ============================================================
-- Institution-wise Fix for Is_LateFeeAdded
-- 
-- Usage: @SchoolID = 0 ???? ?? ??????????, ????????? ID ???? ???? ??? ??????????
--
-- Rule:
--   Is_LateFeeAdded = 1  ? EndDate ??? ?????? AND on-time payment ???
--   Is_LateFeeAdded = 0  ? EndDate-?? ??? full payment ?????? (LateFee apply ??)
-- ============================================================

DECLARE @SchoolID INT = 0;  -- ? ????? SchoolID ???, 0 = ?? ??????????

-- ============================================================
-- Step 1: ??????? Is_LateFeeAdded=1 ????? rows ??? ???
-- ?? rows-? EndDate-?? ??? full base-fee payment ???????
-- ============================================================
UPDATE Income_PayOrder
SET Is_LateFeeAdded = 0
WHERE ISNULL(LateFee, 0) > 0
  AND Is_LateFeeAdded = 1
  AND (@SchoolID = 0 OR SchoolID = @SchoolID)
  AND EXISTS (
    SELECT 1
    FROM Income_PaymentRecord pr
    WHERE pr.PayOrderID = Income_PayOrder.PayOrderID
      AND pr.PaidDate <= Income_PayOrder.EndDate
      AND pr.PaidAmount >= (Income_PayOrder.Amount - ISNULL(Income_PayOrder.Discount, 0))
  );

PRINT 'Step 1 Done: Is_LateFeeAdded=1 ? 0 fixed rows: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ============================================================
-- Step 2: Late payment ????? rows-? Is_LateFeeAdded=1 ???
-- EndDate ??? ?????? ?????? on-time full payment ??? ? LateFee apply ???
-- ============================================================
UPDATE Income_PayOrder
SET Is_LateFeeAdded = 1
WHERE EndDate < GETDATE()
  AND ISNULL(LateFee, 0) > 0
  AND Is_LateFeeAdded = 0
  AND (@SchoolID = 0 OR SchoolID = @SchoolID)
  AND NOT EXISTS (
    SELECT 1
    FROM Income_PaymentRecord pr
    WHERE pr.PayOrderID = Income_PayOrder.PayOrderID
      AND pr.PaidDate <= Income_PayOrder.EndDate
      AND pr.PaidAmount >= (Income_PayOrder.Amount - ISNULL(Income_PayOrder.Discount, 0))
  );

PRINT 'Step 2 Done: Is_LateFeeAdded=0 ? 1 fixed rows: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ============================================================
-- Step 3: Result ????? — ???? selected ????????????
-- ============================================================
SELECT
    po.PayOrderID,
    po.SchoolID,
    si.SchoolName,
    s.ID          AS StudentID,
    s.StudentsName,
    po.PayFor,
    po.Amount,
    po.PaidAmount,
    po.LateFee,
    po.Is_LateFeeAdded,
    po.EndDate,
    po.Status,
    po.Receivable_Amount AS Due
FROM Income_PayOrder po
INNER JOIN Student    s  ON po.StudentID = s.StudentID
INNER JOIN SchoolInfo si ON po.SchoolID  = si.SchoolID
WHERE po.Receivable_Amount > 0
  AND po.EndDate < GETDATE()
  AND (@SchoolID = 0 OR po.SchoolID = @SchoolID)
ORDER BY po.EndDate DESC;
