-- ============================================================
-- PARTHO PRITOM (0207) — Receipt 100008 / 950 Tk মুছুন
-- SSMS: Edu database select → Ctrl+A → F5 (পুরো ফাইল)
-- ============================================================

USE Edu;

SET NOCOUNT OFF;

DECLARE @MoneyReceiptID INT;

-- রেকর্ড খুঁজুন (ID hardcode নয় — SN + Student দিয়ে)
SELECT @MoneyReceiptID = mr.MoneyReceiptID
FROM Income_MoneyReceipt mr
INNER JOIN Student s ON mr.StudentID = s.StudentID
WHERE s.ID = N'0207'
  AND mr.MoneyReceipt_SN = 100008
  AND mr.TotalAmount = 950;

IF @MoneyReceiptID IS NULL
BEGIN
    PRINT N'*** Receipt 100008 (950 Tk) পাওয়া যায়নি — হয় আগেই মুছে গেছে, নয় ভিন্ন DB/server ***';
    PRINT N'Student 0207-এর সব receipt:';
    SELECT mr.MoneyReceiptID, mr.MoneyReceipt_SN, mr.TotalAmount,
           CONVERT(VARCHAR(11), mr.PaidDate, 106) AS PaidDate,
           s.StudentsName
    FROM Income_MoneyReceipt mr
    INNER JOIN Student s ON mr.StudentID = s.StudentID
    WHERE s.ID = N'0207'
    ORDER BY mr.PaidDate DESC;
    RETURN;
END

PRINT N'পাওয়া গেছে MoneyReceiptID = ' + CAST(@MoneyReceiptID AS NVARCHAR(20));

-- মুছার আগে দেখুন
SELECT mr.MoneyReceiptID, mr.MoneyReceipt_SN, mr.TotalAmount, mr.PaidDate,
       s.ID AS StudentID, s.StudentsName
FROM Income_MoneyReceipt mr
INNER JOIN Student s ON mr.StudentID = s.StudentID
WHERE mr.MoneyReceiptID = @MoneyReceiptID;

SELECT pr.PaymentRecordID, pr.PayFor, pr.PaidAmount
FROM Income_PaymentRecord pr
WHERE pr.MoneyReceiptID = @MoneyReceiptID;

BEGIN TRANSACTION;

    UPDATE po
    SET po.PaidAmount = po.PaidAmount - x.PaidTotal,
        po.NumberOfPayment = CASE
            WHEN po.NumberOfPayment > x.Cnt THEN po.NumberOfPayment - x.Cnt
            ELSE 0
        END
    FROM Income_PayOrder po
    INNER JOIN (
        SELECT PayOrderID, SUM(PaidAmount) AS PaidTotal, COUNT(*) AS Cnt
        FROM Income_PaymentRecord
        WHERE MoneyReceiptID = @MoneyReceiptID
        GROUP BY PayOrderID
    ) x ON po.PayOrderID = x.PayOrderID;

    DELETE FROM Income_PaymentRecord WHERE MoneyReceiptID = @MoneyReceiptID;
    PRINT N'Income_PaymentRecord মোছা: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' row';

    DELETE FROM Income_MoneyReceipt WHERE MoneyReceiptID = @MoneyReceiptID;
    PRINT N'Income_MoneyReceipt মোছা: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' row';

COMMIT TRANSACTION;

-- verify — 0 row = সফল
SELECT mr.MoneyReceiptID, mr.MoneyReceipt_SN
FROM Income_MoneyReceipt mr
WHERE mr.MoneyReceiptID = @MoneyReceiptID;

PRINT N'সম্পন্ন। এখন ওয়েবসাইটে Ctrl+F5 দিয়ে refresh করুন।';
