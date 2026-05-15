-- ============================================================
-- CORRECTED Fix for Is_LateFeeAdded
-- 
-- Rule:
--   Is_LateFeeAdded = 1  → EndDate পার হয়েছে AND on-time payment নেই
--   Is_LateFeeAdded = 0  → EndDate-এর আগে full payment হয়েছে (LateFee apply না)
-- ============================================================

-- Step 1: ভুলভাবে Is_LateFeeAdded=1 হওয়া rows ঠিক করো
-- যে rows-এ EndDate-এর আগে full base-fee payment হয়েছিল
UPDATE Income_PayOrder
SET Is_LateFeeAdded = 0
WHERE ISNULL(LateFee, 0) > 0
  AND Is_LateFeeAdded = 1
  AND EXISTS (
    SELECT 1
    FROM Income_PaymentRecord pr
    WHERE pr.PayOrderID = Income_PayOrder.PayOrderID
      AND pr.PaidDate <= Income_PayOrder.EndDate
      AND pr.PaidAmount >= (Income_PayOrder.Amount - ISNULL(Income_PayOrder.Discount, 0))
  );

-- Step 2: Late payment হওয়া rows-এ Is_LateFeeAdded=1 করো
-- EndDate পার হয়েছে কিন্তু on-time full payment নেই → LateFee apply হবে
UPDATE Income_PayOrder
SET Is_LateFeeAdded = 1
WHERE EndDate < GETDATE()
  AND ISNULL(LateFee, 0) > 0
  AND Is_LateFeeAdded = 0
  AND NOT EXISTS (
    SELECT 1
    FROM Income_PaymentRecord pr
    WHERE pr.PayOrderID = Income_PayOrder.PayOrderID
      AND pr.PaidDate <= Income_PayOrder.EndDate
      AND pr.PaidAmount >= (Income_PayOrder.Amount - ISNULL(Income_PayOrder.Discount, 0))
  );

-- Step 3: Verify result
SELECT
    po.PayOrderID,
    s.ID AS StudentID,
    po.PayFor,
    po.Amount,
    po.PaidAmount,
    po.LateFee,
    po.Is_LateFeeAdded,
    po.EndDate,
    po.Status,
    po.Receivable_Amount AS Due
FROM Income_PayOrder po
INNER JOIN Student s ON po.StudentID = s.StudentID
WHERE po.Receivable_Amount > 0
  AND po.EndDate < GETDATE()
ORDER BY po.EndDate DESC;
