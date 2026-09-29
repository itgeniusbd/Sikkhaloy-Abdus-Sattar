USE ReceiptSnTest; SET NOCOUNT ON;
SELECT PaymentBy, COUNT(*) AS rows_, MIN(MoneyReceipt_SN) mn, MAX(MoneyReceipt_SN) mx FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID > $(FROMID) GROUP BY PaymentBy;
SELECT COUNT(*) AS dup_pairs FROM (SELECT SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceipt_SN IS NOT NULL GROUP BY SchoolID, MoneyReceipt_SN HAVING COUNT(*)>1) d;
;WITH s AS (SELECT MoneyReceipt_SN, LAG(MoneyReceipt_SN) OVER (ORDER BY MoneyReceipt_SN) p FROM dbo.Income_MoneyReceipt WHERE SchoolID=1208 AND MoneyReceiptID > $(FROMID))
SELECT COUNT(*) AS gaps, ISNULL(SUM(MoneyReceipt_SN - p - 1),0) AS missing_numbers FROM s WHERE MoneyReceipt_SN - p > 1;
SELECT MAX(MoneyReceiptID) AS maxid FROM dbo.Income_MoneyReceipt;
