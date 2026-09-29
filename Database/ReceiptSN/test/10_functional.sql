USE ReceiptSnTest;
SET NOCOUNT ON;
DECLARE @r TABLE (step nvarchar(60), SchoolID int, SN int);
DECLARE @id int, @sn int;

/* first calls through the V2 procedure (lazy/pre-created sequences) */
DECLARE @x TABLE (id decimal(18,0));
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',1208;
INSERT @x EXEC dbo.MoneyReceipt 2,1,1,1,N'Institution','2026-09-28',1208;
INSERT @x EXEC dbo.MoneyReceipt 3,1,1,1,N'Institution','2026-09-28',1208;
INSERT @r SELECT 'V2 SP consecutive (expect 101901..101903)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',7;
INSERT @r SELECT 'school 7 (expect 100006)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',99;
INSERT @r SELECT 'school 99 old small SN (expect 100001)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',500;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',500;
INSERT @r SELECT 'new school 500 lazy create (expect 100001,100002)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;

/* self-heal 1: old code (MAX+1) inserts two receipts after the sequence exists */
INSERT dbo.Income_MoneyReceipt(SchoolID, MoneyReceipt_SN, PaidDate) VALUES (1208, dbo.F_MoneyReceipt_SN(1208), GETDATE());
INSERT dbo.Income_MoneyReceipt(SchoolID, MoneyReceipt_SN, PaidDate) VALUES (1208, dbo.F_MoneyReceipt_SN(1208), GETDATE());
INSERT @r SELECT 'old-code rows (101904,101905)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE SchoolID = 1208 AND MoneyReceipt_SN > 101903;
INSERT @x EXEC dbo.MoneyReceipt 4,1,1,1,N'Institution','2026-09-28',1208;
INSERT @x EXEC dbo.MoneyReceipt 5,1,1,1,N'Institution','2026-09-28',1208;
INSERT @r SELECT 'after heal (expect 101906,101907)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;

/* self-heal 2: manual far-ahead number */
INSERT dbo.Income_MoneyReceipt(SchoolID, MoneyReceipt_SN, PaidDate) VALUES (7, 105000, GETDATE());
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',7;
INSERT @r SELECT 'school 7 after manual 105000 (expect 105001)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;

/* rolled-back transaction consumes a number */
BEGIN TRAN;
  INSERT @x EXEC dbo.MoneyReceipt 6,1,1,1,N'Institution','2026-09-28',1208;
  SELECT @sn = MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
ROLLBACK;
INSERT @r VALUES ('inside rolled-back tran (expect 101908, then gone)', 1208, @sn);
DELETE @x;
INSERT @x EXEC dbo.MoneyReceipt 7,1,1,1,N'Institution','2026-09-28',1208;
INSERT @r SELECT 'next after rollback (expect 101909 = gap of 1)', SchoolID, MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;

/* direct usp call with no outer transaction */
EXEC dbo.usp_NextMoneyReceiptSN 1208, @sn OUTPUT;
INSERT @r VALUES ('usp direct, no tran (expect 101910, not inserted)', 1208, @sn);

/* invalid school */
BEGIN TRY EXEC dbo.usp_NextMoneyReceiptSN 0, @sn OUTPUT; END TRY
BEGIN CATCH INSERT @r VALUES (LEFT('error ok: ' + ERROR_MESSAGE(), 60), 0, NULL); END CATCH

SELECT * FROM @r;
SELECT name, start_value, current_value, last_used_value, cache_size FROM sys.sequences ORDER BY name;
SELECT SchoolID, MoneyReceipt_SN, COUNT(*) c FROM dbo.Income_MoneyReceipt WHERE MoneyReceipt_SN IS NOT NULL GROUP BY SchoolID, MoneyReceipt_SN HAVING COUNT(*) > 1;
