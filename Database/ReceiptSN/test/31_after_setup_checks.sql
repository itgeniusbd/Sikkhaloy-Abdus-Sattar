/* LocalDB ONLY. Run after 30_bootstrap_live_dups.sql + receipt_sn_setup.sql. */
USE ReceiptSnDupTest; SET NOCOUNT ON;
GO
IF DB_NAME() <> N'ReceiptSnDupTest' BEGIN RAISERROR(N'wrong db', 16, 1); SET NOEXEC ON; END
GO
DECLARE @r TABLE (n int IDENTITY, test nvarchar(80), result nvarchar(200));
DECLARE @cut int, @sid int, @sn int;
DECLARE @x TABLE (id decimal(18,0));
SELECT @cut = TRY_CONVERT(int, REPLACE(SUBSTRING(filter_definition, CHARINDEX(N'[MoneyReceiptID]>(', filter_definition) + 18, 20), N')', N''))
FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND name = N'UX_Income_MoneyReceipt_School_SN';
INSERT @r VALUES (N'cutoff in unique index filter (expect 1166495)', CONVERT(nvarchar(20), @cut));

/* 1) historical duplicates still there */
INSERT @r SELECT N'historical duplicate pairs still present (expect 6)', CONVERT(nvarchar(10), COUNT(*))
FROM (SELECT 1 x FROM dbo.Income_MoneyReceipt GROUP BY SchoolID, MoneyReceipt_SN HAVING COUNT(*) > 1) d;

/* 2) new duplicate of a number above the cutoff -> must be rejected */
SELECT TOP (1) @sid = SchoolID, @sn = MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID > @cut AND SchoolID = 1144 ORDER BY MoneyReceiptID DESC;
BEGIN TRY
    INSERT dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN, PaymentBy) VALUES (@sid, @sn, N'new-dup-test');
    INSERT @r VALUES (N'new duplicate (1144,' + CONVERT(nvarchar(11), @sn) + N') above cutoff', N'*** ACCEPTED (FAIL) ***');
END TRY
BEGIN CATCH
    INSERT @r VALUES (N'new duplicate (1144,' + CONVERT(nvarchar(11), @sn) + N') above cutoff', N'rejected, error ' + CONVERT(nvarchar(10), ERROR_NUMBER()));
END CATCH
/* same via the old MAX+1 path racing: two rows with the same new number */
BEGIN TRY
    BEGIN TRAN;
    INSERT dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN, PaymentBy) VALUES (1162, dbo.F_MoneyReceipt_SN(1162), N'oldcode-a');
    INSERT dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN, PaymentBy) SELECT 1162, MoneyReceipt_SN, N'oldcode-b' FROM dbo.Income_MoneyReceipt WHERE PaymentBy = N'oldcode-a';
    COMMIT;
    INSERT @r VALUES (N'two new rows with the same fresh number', N'*** ACCEPTED (FAIL) ***');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    INSERT @r VALUES (N'two new rows with the same fresh number', N'rejected, error ' + CONVERT(nvarchar(10), ERROR_NUMBER()));
END CATCH

/* 3) known limitation: a NEW row reusing a HISTORICAL number (row below the cutoff) is not
      blocked by the filtered index; the sequence never issues such a number (starts > MAX) */
BEGIN TRAN;
    INSERT dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN, PaymentBy) VALUES (1144, 166376, N'limit-test');
    INSERT @r VALUES (N'limitation: new row reusing historical (1144,166376)', N'accepted by index (not issued by usp)');
ROLLBACK;

/* 4) usp / dbo.MoneyReceipt with the historical duplicates present */
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',1144;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',1144;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',1162;
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',1185;
INSERT @r SELECT N'MoneyReceipt school ' + CONVERT(nvarchar(11), m.SchoolID) + N' id ' + CONVERT(nvarchar(11), m.MoneyReceiptID),
       N'SN ' + CONVERT(nvarchar(11), m.MoneyReceipt_SN) + N' (prev max ' +
       CONVERT(nvarchar(11), (SELECT MAX(p.MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt p WHERE p.SchoolID = m.SchoolID AND p.MoneyReceiptID < m.MoneyReceiptID)) + N')'
FROM dbo.Income_MoneyReceipt m WHERE m.MoneyReceiptID IN (SELECT id FROM @x) ORDER BY m.MoneyReceiptID;
DELETE @x;
/* self-heal: old code (MAX+1) inserts after the sequence exists, then the usp */
INSERT dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN, PaymentBy) VALUES (1144, dbo.F_MoneyReceipt_SN(1144), N'oldcode');
INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',1144;
INSERT @r SELECT N'self-heal after old-code insert ' + CONVERT(nvarchar(11), (SELECT MoneyReceipt_SN FROM dbo.Income_MoneyReceipt WHERE PaymentBy = N'oldcode')),
       N'usp issued ' + CONVERT(nvarchar(11), MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt WHERE MoneyReceiptID IN (SELECT id FROM @x);
DELETE @x;

/* 5) SchoolID 0 (Authority, not a school) */
INSERT @r SELECT N'Seq_MoneyReceipt_0 exists?', CASE WHEN OBJECT_ID(N'dbo.Seq_MoneyReceipt_0', N'SO') IS NULL THEN N'no' ELSE N'YES' END;
BEGIN TRY
    INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'Institution','2026-09-28',0;
    INSERT @r VALUES (N'MoneyReceipt with SchoolID 0', N'accepted');
END TRY
BEGIN CATCH
    INSERT @r VALUES (N'MoneyReceipt with SchoolID 0', LEFT(N'error ' + CONVERT(nvarchar(10), ERROR_NUMBER()) + N': ' + ERROR_MESSAGE(), 200));
END CATCH

/* 6) duplicates at the end = the six historical ones only */
INSERT @r SELECT N'duplicate pairs at the end (expect 6, all ids <= cutoff)',
       CONVERT(nvarchar(10), COUNT(*)) + N' pairs, max id ' + CONVERT(nvarchar(11), MAX(LastID))
FROM (SELECT MAX(MoneyReceiptID) LastID FROM dbo.Income_MoneyReceipt GROUP BY SchoolID, MoneyReceipt_SN HAVING COUNT(*) > 1) d;
SELECT test, result FROM @r ORDER BY n;
SELECT name, is_unique, filter_definition FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND index_id > 0 ORDER BY index_id;
GO
