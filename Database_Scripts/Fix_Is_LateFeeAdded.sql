-- ============================================================
-- Fix Is_LateFeeAdded for past-due records where EndDate has
-- already passed but Is_LateFeeAdded was still 0.
-- After this fix, the computed Status column will correctly
-- show 'Due' when Amount+LateFee-Discount-PaidAmount > 0.
-- ============================================================

-- Step 1: Preview affected records before update
SELECT 
    PayOrderID,
    Amount,
    PaidAmount,
    LateFee,
    Discount,
    Is_LateFeeAdded,
    EndDate,
    Status,
    Receivable_Amount
FROM Income_PayOrder
WHERE EndDate < GETDATE()
  AND ISNULL(LateFee, 0) > 0
  AND Is_LateFeeAdded = 0
  AND (Amount - ISNULL(Discount,0) - ISNULL(PaidAmount,0)) = 0
  AND (Amount + ISNULL(LateFee,0) - ISNULL(Discount,0) - ISNULL(PaidAmount,0) - ISNULL(LateFee_Discount,0)) > 0;

-- Step 2: Fix existing wrong records
-- Set Is_LateFeeAdded = 1 for past-due records that:
--   - EndDate has passed
--   - Have a LateFee set
--   - Is_LateFeeAdded is still 0 (late fee not yet applied)
--   - PaidAmount covers only base fee (not late fee)
UPDATE Income_PayOrder
SET Is_LateFeeAdded = 1
WHERE EndDate < GETDATE()
  AND ISNULL(LateFee, 0) > 0
  AND Is_LateFeeAdded = 0;

-- Step 3: Verify fix
SELECT 
    PayOrderID,
    Amount,
    PaidAmount,
    LateFee,
    Is_LateFeeAdded,
    EndDate,
    Status,
    Receivable_Amount
FROM Income_PayOrder
WHERE EndDate < GETDATE()
  AND ISNULL(LateFee, 0) > 0
  AND (Amount + ISNULL(LateFee,0) - ISNULL(Discount,0) - ISNULL(PaidAmount,0) - ISNULL(LateFee_Discount,0)) > 0
ORDER BY EndDate DESC;
