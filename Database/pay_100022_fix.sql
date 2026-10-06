/* =====================================================================================
   pay_100022_fix.sql     REPAIR TEMPLATE - ROLLS BACK BY DEFAULT (@Commit = 0)      2026-09-29
   -------------------------------------------------------------------------------------
   What happened: the payment for StudentID 172754 was collected on Payment_Collection.aspx
   after the web server restarted. The browser page was still open, the login cookie survived,
   but the server-side Session was empty, so SchoolID = 0 and RegistrationID = 0 were used:
     * Income_MoneyReceipt  -> SchoolID 0, RegistrationID 0, number from school 0's own
                               counter (MAX of school 0 + 1 = 100022)
     * Income_PaymentRecord -> SchoolID 0, RegistrationID 0
     * Income_PayOrder      -> correctly marked paid (updated by PayOrderID only)
     * Account              -> Total_Income added to the chosen account (correct)
     * Account_Log          -> SchoolID 0, RegistrationID 0, Details NULL (operator name missing)
   Nothing is half-saved: the money WAS collected and applied. The receipt is only filed under
   the wrong school. So the right direction is to COMPLETE it (move it to the student's school),
   not to reverse it. Reversing would make September/October due again although the parent paid.

   What this script does (inside ONE transaction; idempotent - a repaired receipt is not found again):
     1. picks the orphan receipt(s): SchoolID 0/NULL. Default = only receipt 100022 of StudentID 172754;
        set @AllOrphans = 1 to repair every school-0 receipt listed by pay_100022_check_readonly.sql (R6).
        A receipt is only repaired if ALL its pay orders and accounts belong to the student's school.
     2. collector (RegistrationID): @CollectorRegistrationID if given, else the collector of the
        student's latest other receipt in that school, else the school's latest collector.
     3. receipt number: kept if still free in the target school; if that school already has the
        number (school 1240 has an old receipt 100022 from 2025), a new number is issued from the
        school's own counter and the printed number is kept in PrintedReceiptNo ("100022") so the
        parent's paper receipt can still be found. Set @KeepNumberEvenIfDuplicate = 1 to keep 100022
        (only possible if the unique index does not cover this receipt).
     4. moves receipt + payment rows + matching Account_Log rows to the school; rebuilds the
        Account_Log text and serial number. PayOrders and Account balances are already correct.
     5. re-seeds any per-school receipt sequence that is at/behind the school's MAX (ALTER SEQUENCE RESTART).
     6. shows before/after; ROLLBACK unless @Commit = 1.
   RUN: first as is (rollback) and read the result sets; then set @Commit = 1 and run again.
   ===================================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @Commit                    bit = 0,       -- 0 = test (ROLLBACK), 1 = save (COMMIT)
        @StudentID                 int = 172754,
        @ReceiptSN                 int = 100022,
        @AllOrphans                bit = 0,       -- 1 = repair every SchoolID 0/NULL receipt since @OrphanSince
        @OrphanSince               date = '2026-01-01',
        @CollectorRegistrationID   int = NULL,    -- NULL = automatic (see header)
        @KeepNumberEvenIfDuplicate bit = 0,
        @ReseedSequences           bit = 1;

BEGIN TRY
BEGIN TRANSACTION;

/* ---------- 1. candidates ---------- */
IF OBJECT_ID('tempdb..#fix') IS NOT NULL DROP TABLE #fix;
SELECT mr.MoneyReceiptID, mr.StudentID, mr.MoneyReceipt_SN AS OldSN, s.SchoolID AS TargetSchool,
       CAST(NULL AS int) AS NewRegID, CAST(NULL AS int) AS NewSN, CAST(NULL AS nvarchar(200)) AS Skip
INTO #fix
FROM dbo.Income_MoneyReceipt mr WITH (UPDLOCK, HOLDLOCK)
JOIN dbo.Student s ON s.StudentID = mr.StudentID
WHERE ISNULL(mr.SchoolID, 0) = 0
  AND ( (@AllOrphans = 0 AND mr.StudentID = @StudentID AND mr.MoneyReceipt_SN = @ReceiptSN)
     OR (@AllOrphans = 1 AND mr.PaidDate >= @OrphanSince) );

UPDATE f SET Skip = N'student has no school'
FROM #fix f WHERE ISNULL(f.TargetSchool, 0) = 0;
UPDATE f SET Skip = N'a pay order belongs to another school'
FROM #fix f
WHERE f.Skip IS NULL AND EXISTS (SELECT 1 FROM dbo.Income_PaymentRecord p JOIN dbo.Income_PayOrder po ON po.PayOrderID = p.PayOrderID
                                 WHERE p.MoneyReceiptID = f.MoneyReceiptID AND po.SchoolID <> f.TargetSchool);
UPDATE f SET Skip = N'an account belongs to another school'
FROM #fix f
WHERE f.Skip IS NULL AND EXISTS (SELECT 1 FROM dbo.Income_PaymentRecord p JOIN dbo.Account a ON a.AccountID = p.AccountID
                                 WHERE p.MoneyReceiptID = f.MoneyReceiptID AND a.SchoolID <> f.TargetSchool);

/* ---------- 2. collector ---------- */
UPDATE f SET NewRegID = COALESCE(
        (SELECT ad.RegistrationID FROM dbo.Admin ad WHERE ad.RegistrationID = @CollectorRegistrationID),
        (SELECT TOP 1 o.RegistrationID FROM dbo.Income_MoneyReceipt o JOIN dbo.Admin ad ON ad.RegistrationID = o.RegistrationID
          WHERE o.SchoolID = f.TargetSchool AND o.StudentID = f.StudentID AND o.MoneyReceiptID <> f.MoneyReceiptID ORDER BY o.MoneyReceiptID DESC),
        (SELECT TOP 1 o.RegistrationID FROM dbo.Income_MoneyReceipt o JOIN dbo.Admin ad ON ad.RegistrationID = o.RegistrationID
          WHERE o.SchoolID = f.TargetSchool ORDER BY o.MoneyReceiptID DESC))
FROM #fix f WHERE f.Skip IS NULL;
UPDATE #fix SET Skip = N'no collector found - set @CollectorRegistrationID' WHERE Skip IS NULL AND NewRegID IS NULL;

SELECT 'BEFORE: receipts to repair' AS Step, f.*, mr.SchoolID, mr.RegistrationID, mr.PrintedReceiptNo, mr.TotalAmount, mr.PaidDate
FROM #fix f JOIN dbo.Income_MoneyReceipt mr ON mr.MoneyReceiptID = f.MoneyReceiptID ORDER BY f.MoneyReceiptID;

/* ---------- 3. receipt number ---------- */
DECLARE @mid int, @school int, @old int, @sn int;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT MoneyReceiptID, TargetSchool, OldSN FROM #fix WHERE Skip IS NULL ORDER BY MoneyReceiptID;
OPEN c; FETCH NEXT FROM c INTO @mid, @school, @old;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sn = @old;
    IF @KeepNumberEvenIfDuplicate = 0
       AND (EXISTS (SELECT 1 FROM dbo.Income_MoneyReceipt WHERE SchoolID = @school AND MoneyReceipt_SN = @old AND MoneyReceiptID <> @mid)
            OR EXISTS (SELECT 1 FROM #fix WHERE TargetSchool = @school AND NewSN = @old))
    BEGIN
        IF OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NOT NULL
            EXEC dbo.usp_NextMoneyReceiptSN @SchoolID = @school, @SN = @sn OUTPUT;
        ELSE
        BEGIN
            /* MAX+1 over existing receipts AND numbers already handed out in this run (several orphans of one school) */
            SELECT @sn = MAX(v) FROM (
                SELECT MAX(MoneyReceipt_SN) AS v FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK) WHERE SchoolID = @school
                UNION ALL SELECT MAX(NewSN) FROM #fix WHERE TargetSchool = @school) m;
            SET @sn = CASE WHEN ISNULL(@sn, 0) < 100000 THEN 100001 ELSE @sn + 1 END;
        END
    END
    UPDATE #fix SET NewSN = @sn WHERE MoneyReceiptID = @mid;
    FETCH NEXT FROM c INTO @mid, @school, @old;
END
CLOSE c; DEALLOCATE c;

/* ---------- 4. move to the school ---------- */
-- Account_Log rows of these payments (school 0, operator NULL), paired 1:1 with the payment rows
IF OBJECT_ID('tempdb..#log') IS NOT NULL DROP TABLE #log;
;WITH p AS (
    SELECT pr.PaymentRecordID, pr.AccountID, pr.PaidAmount, pr.PayFor, CAST(pr.PaidDate AS date) AS D, f.TargetSchool, f.NewRegID, f.NewSN, pr.RoleID,
           ROW_NUMBER() OVER (PARTITION BY pr.AccountID, pr.PaidAmount, pr.PayFor, CAST(pr.PaidDate AS date) ORDER BY pr.PaymentRecordID) AS rn
    FROM dbo.Income_PaymentRecord pr JOIN #fix f ON f.MoneyReceiptID = pr.MoneyReceiptID
    WHERE f.Skip IS NULL AND ISNULL(pr.SchoolID, 0) = 0),
l AS (
    SELECT al.AccountLogID, al.AccountID, al.Amount, al.Pay_For, al.Activity_Date,
           ROW_NUMBER() OVER (PARTITION BY al.AccountID, al.Amount, al.Pay_For, al.Activity_Date ORDER BY al.AccountLogID) AS rn
    FROM dbo.Account_Log al WITH (UPDLOCK)
    WHERE ISNULL(al.SchoolID, 0) = 0 AND al.MainCategory = N'Student Payment' AND al.Add_Subtraction = 'Add' AND al.Details IS NULL)
SELECT l.AccountLogID, p.TargetSchool, p.NewRegID, p.NewSN, p.PaidAmount, p.RoleID
INTO #log
FROM p JOIN l ON l.AccountID = p.AccountID AND l.Amount = p.PaidAmount AND l.Pay_For = p.PayFor AND l.Activity_Date = p.D AND l.rn = p.rn;

UPDATE mr SET mr.SchoolID = f.TargetSchool,
              mr.RegistrationID = f.NewRegID,
              mr.MoneyReceipt_SN = f.NewSN,
              mr.PrintedReceiptNo = CASE WHEN f.NewSN <> f.OldSN AND mr.PrintedReceiptNo IS NULL THEN CONVERT(nvarchar(20), f.OldSN) ELSE mr.PrintedReceiptNo END
FROM dbo.Income_MoneyReceipt mr JOIN #fix f ON f.MoneyReceiptID = mr.MoneyReceiptID
WHERE f.Skip IS NULL AND ISNULL(mr.SchoolID, 0) = 0;
PRINT CONCAT('receipts moved: ', @@ROWCOUNT);

UPDATE pr SET pr.SchoolID = f.TargetSchool, pr.RegistrationID = f.NewRegID
FROM dbo.Income_PaymentRecord pr JOIN #fix f ON f.MoneyReceiptID = pr.MoneyReceiptID
WHERE f.Skip IS NULL AND ISNULL(pr.SchoolID, 0) = 0;
PRINT CONCAT('payment rows moved: ', @@ROWCOUNT);

;WITH n AS (
    SELECT g.AccountLogID, g.TargetSchool, g.NewRegID, g.NewSN, g.PaidAmount, g.RoleID,
           (SELECT ISNULL(MAX(x.Log_SN), 0) FROM dbo.Account_Log x WITH (UPDLOCK, HOLDLOCK) WHERE x.SchoolID = g.TargetSchool)
           + ROW_NUMBER() OVER (PARTITION BY g.TargetSchool ORDER BY g.AccountLogID) AS NewLogSN
    FROM #log g)
UPDATE al SET al.SchoolID = n.TargetSchool,
              al.RegistrationID = n.NewRegID,
              al.Log_SN = n.NewLogSN,
              al.Details = N'Receipt No: ' + CONVERT(nvarchar(20), n.NewSN) + N'. Collected  ' + ISNULL(r.Role, N'') + N' '
                         + CAST(n.PaidAmount AS varchar(50)) + N' Tk. Operated By: ' + ISNULL(ad.FirstName + N' ' + ad.LastName, N'')
                         + N' (repaired: saved without session on ' + CONVERT(nvarchar(10), al.Activity_Date, 120) + N')'
FROM dbo.Account_Log al
JOIN n ON n.AccountLogID = al.AccountLogID
LEFT JOIN dbo.Income_Roles r ON r.RoleID = n.RoleID
LEFT JOIN dbo.Admin ad ON ad.RegistrationID = n.NewRegID;
PRINT CONCAT('account log rows moved: ', @@ROWCOUNT);

/* ---------- 5. sequences at/behind MAX ---------- */
IF @ReseedSequences = 1
BEGIN
    DECLARE @seq sysname, @sid int, @next bigint, @max int, @sql nvarchar(400);
    DECLARE q CURSOR LOCAL FAST_FORWARD FOR
        SELECT sq.name, TRY_CONVERT(int, SUBSTRING(sq.name, 18, 20)),
               CASE WHEN sq.last_used_value IS NULL THEN CONVERT(bigint, sq.current_value) ELSE CONVERT(bigint, sq.last_used_value) + 1 END
        FROM sys.sequences sq WHERE sq.schema_id = SCHEMA_ID(N'dbo') AND sq.name LIKE N'Seq[_]MoneyReceipt[_]%';
    OPEN q; FETCH NEXT FROM q INTO @seq, @sid, @next;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @max = MAX(MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt WHERE SchoolID = @sid;
        IF @next <= ISNULL(@max, 0)
        BEGIN
            SET @sql = N'ALTER SEQUENCE dbo.' + QUOTENAME(@seq) + N' RESTART WITH ' + CONVERT(nvarchar(20), @max + 1) + N';';
            EXEC (@sql);
            PRINT CONCAT('re-seeded ', @seq, ': next was ', @next, ', MAX ', @max, ' -> restart ', @max + 1);
        END
        FETCH NEXT FROM q INTO @seq, @sid, @next;
    END
    CLOSE q; DEALLOCATE q;
END

/* ---------- 6. verification ---------- */
SELECT 'AFTER: receipts' AS Step, f.MoneyReceiptID, f.Skip, mr.MoneyReceipt_SN, mr.PrintedReceiptNo, mr.SchoolID, mr.RegistrationID,
       ad.FirstName + ' ' + ad.LastName AS CollectedBy, mr.EducationYearID, mr.TotalAmount,
       (SELECT SUM(p.PaidAmount) FROM dbo.Income_PaymentRecord p WHERE p.MoneyReceiptID = mr.MoneyReceiptID) AS SumPaymentRows,
       (SELECT COUNT(*) FROM dbo.Income_PaymentRecord p WHERE p.MoneyReceiptID = mr.MoneyReceiptID AND ISNULL(p.SchoolID, 0) <> mr.SchoolID) AS RowsInOtherSchool,
       (SELECT COUNT(*) FROM dbo.Income_MoneyReceipt d WHERE d.SchoolID = mr.SchoolID AND d.MoneyReceipt_SN = mr.MoneyReceipt_SN) AS CopiesOfThisSN
FROM #fix f JOIN dbo.Income_MoneyReceipt mr ON mr.MoneyReceiptID = f.MoneyReceiptID
LEFT JOIN dbo.Admin ad ON ad.RegistrationID = mr.RegistrationID
ORDER BY f.MoneyReceiptID;

SELECT 'AFTER: account log' AS Step, al.AccountLogID, al.SchoolID, al.RegistrationID, al.Log_SN, al.Amount, al.Pay_For, al.Details
FROM dbo.Account_Log al JOIN #log g ON g.AccountLogID = al.AccountLogID ORDER BY al.AccountLogID;

-- what Payment_Collection "Student Paid Records (Current Session)" will now list for the first student
SELECT 'AFTER: Payment_Collection popup' AS Step, mr.MoneyReceipt_SN, mr.PrintedReceiptNo, mr.TotalAmount, mr.PaidDate, ad.FirstName + ' ' + ad.LastName AS ReceivedBy
FROM dbo.Income_MoneyReceipt mr
JOIN dbo.Student st ON mr.StudentID = st.StudentID AND st.SchoolID = mr.SchoolID
JOIN dbo.Admin ad ON mr.RegistrationID = ad.RegistrationID
WHERE mr.StudentID = (SELECT TOP 1 StudentID FROM #fix ORDER BY MoneyReceiptID)
  AND mr.EducationYearID = (SELECT TOP 1 m2.EducationYearID FROM dbo.Income_MoneyReceipt m2 JOIN #fix f2 ON f2.MoneyReceiptID = m2.MoneyReceiptID ORDER BY f2.MoneyReceiptID)
ORDER BY ISNULL(mr.CollectionDate, mr.PaidDate) DESC;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT '*** COMMITTED ***';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT '*** TEST RUN: ROLLED BACK (nothing saved). Set @Commit = 1 to save. ***';
END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT '*** ERROR - rolled back: ' + ERROR_MESSAGE();
    THROW;
END CATCH
