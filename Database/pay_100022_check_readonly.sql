/* =====================================================================================
   pay_100022_check_readonly.sql            SELECT ONLY - changes nothing        2026-09-29
   Case: StudentID 172754 (StudentClassID 341081), receipt 100022, 28-Sep-2026.
   Shows the receipt everywhere it lives, explains why each screen does / doesn't show
   it, lists every other "orphan" or half-saved receipt, and checks the receipt counters.
   Run on Edu (live is fine: NOLOCK reads only). Change the parameters for other cases.
   ===================================================================================== */
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
DECLARE @StudentID int = 172754,
        @ReceiptSN int = 100022,
        @HalfSavedSince date = '2026-09-28',   -- window for the half-saved check
        @OrphanSince   date = '2026-01-01';    -- window for the orphan (school 0 / wrong school) check

DECLARE @StudentSchool int = (SELECT SchoolID FROM dbo.Student WHERE StudentID = @StudentID);
SELECT 'R1 student' AS Section, s.StudentID, s.ID, s.StudentsName, s.SchoolID AS StudentSchoolID, s.Status
FROM dbo.Student s WHERE s.StudentID = @StudentID;

/* R2: every receipt of this student + receipt 100022 in school 0 / the student's school.
   Diagnosis columns tell which screen can see it:
     Student Report (MoneyReceiptSQL) : StudentID + EducationYearID + StudentClassID (no SchoolID!)
     Payment_Collection "Paid Records": mr.SchoolID = session school AND Admin(RegistrationID) exists
                                        AND EducationYearID = session year                        */
SELECT 'R2 receipts' AS Section, mr.MoneyReceiptID, mr.MoneyReceipt_SN, mr.PrintedReceiptNo, mr.SchoolID AS ReceiptSchoolID,
       s.SchoolID AS StudentSchoolID, mr.RegistrationID, ad.FirstName + ' ' + ad.LastName AS CollectedBy,
       mr.EducationYearID, ey.EducationYear, mr.StudentClassID, mr.PaidDate, mr.CollectionDate, mr.TotalAmount,
       pr.nRows AS PaymentRows, pr.SumPaid, pr.PrSchools,
       CASE WHEN ISNULL(mr.SchoolID, 0) <> s.SchoolID THEN 'WRONG SCHOOL (' + ISNULL(CONVERT(varchar(11), mr.SchoolID), 'NULL') + ')' ELSE 'ok' END AS SchoolCheck,
       CASE WHEN ad.RegistrationID IS NULL THEN 'NO ADMIN for RegistrationID -> hidden in Payment_Collection' ELSE 'ok' END AS CollectorCheck,
       CASE WHEN ABS(ISNULL(pr.SumPaid, 0) - ISNULL(mr.TotalAmount, 0)) > 0.001 THEN 'TOTAL <> SUM(payment rows)' ELSE 'ok' END AS TotalCheck,
       dupl.OtherReceiptWithSameSN
FROM dbo.Income_MoneyReceipt mr
LEFT JOIN dbo.Student s ON s.StudentID = mr.StudentID
LEFT JOIN dbo.Admin ad ON ad.RegistrationID = mr.RegistrationID
LEFT JOIN dbo.Education_Year ey ON ey.EducationYearID = mr.EducationYearID
OUTER APPLY (SELECT COUNT(*) nRows, SUM(p.PaidAmount) SumPaid,
                    STUFF((SELECT DISTINCT ',' + ISNULL(CONVERT(varchar(11), p2.SchoolID), 'NULL') FROM dbo.Income_PaymentRecord p2 WHERE p2.MoneyReceiptID = mr.MoneyReceiptID FOR XML PATH('')), 1, 1, '') PrSchools
             FROM dbo.Income_PaymentRecord p WHERE p.MoneyReceiptID = mr.MoneyReceiptID) pr
OUTER APPLY (SELECT TOP 1 CONVERT(varchar(11), o.MoneyReceiptID) + ' (student ' + CONVERT(varchar(11), o.StudentID) + ', ' + CONVERT(varchar(10), o.PaidDate, 120) + ')' AS OtherReceiptWithSameSN
             FROM dbo.Income_MoneyReceipt o WHERE o.SchoolID = s.SchoolID AND o.MoneyReceipt_SN = mr.MoneyReceipt_SN AND o.MoneyReceiptID <> mr.MoneyReceiptID) dupl
WHERE mr.StudentID = @StudentID
   OR (mr.MoneyReceipt_SN = @ReceiptSN AND (ISNULL(mr.SchoolID, 0) IN (0, @StudentSchool)))
ORDER BY mr.MoneyReceiptID;

/* R3: payment rows of receipt 100022 (and every receipt of the student since @HalfSavedSince) */
SELECT 'R3 payment rows' AS Section, mr.MoneyReceipt_SN, pr.PaymentRecordID, pr.MoneyReceiptID, pr.SchoolID, pr.RegistrationID, pr.EducationYearID,
       pr.StudentClassID, r.Role, pr.PayFor, pr.PaidAmount, pr.PaidDate, pr.PayOrderID, po.SchoolID AS PayOrderSchool,
       pr.AccountID, a.AccountName, a.SchoolID AS AccountSchool
FROM dbo.Income_PaymentRecord pr
JOIN dbo.Income_MoneyReceipt mr ON mr.MoneyReceiptID = pr.MoneyReceiptID
LEFT JOIN dbo.Income_Roles r ON r.RoleID = pr.RoleID
LEFT JOIN dbo.Income_PayOrder po ON po.PayOrderID = pr.PayOrderID
LEFT JOIN dbo.Account a ON a.AccountID = pr.AccountID
WHERE pr.StudentID = @StudentID AND (pr.PaidDate >= @HalfSavedSince OR mr.MoneyReceipt_SN = @ReceiptSN)
ORDER BY pr.PaymentRecordID;

/* R4: the student's Sept/Oct (and any touched since @HalfSavedSince) pay orders: PaidAmount must equal SUM(payment rows).
   Due = what Payment_Collection's due list uses; Due = 0 => the row is not listed. */
SELECT 'R4 pay orders' AS Section, po.PayOrderID, po.SchoolID, po.EducationYearID, r.Role, po.PayFor, po.Amount, po.LateFee, po.Discount, po.LateFee_Discount,
       po.PaidAmount, x.SumPaidRows, po.NumberOfPayment, po.LastPaidDate,
       ISNULL(po.Amount,0) + ISNULL(po.LateFee,0) - ISNULL(po.Discount,0) - ISNULL(po.LateFee_Discount,0) - ISNULL(po.PaidAmount,0) AS Due,
       CASE WHEN ABS(ISNULL(po.PaidAmount,0) - ISNULL(x.SumPaidRows,0)) > 0.001 THEN 'MISMATCH PaidAmount vs payment rows' ELSE 'ok' END AS PaidCheck
FROM dbo.Income_PayOrder po
LEFT JOIN dbo.Income_Roles r ON r.RoleID = po.RoleID
OUTER APPLY (SELECT SUM(p.PaidAmount) SumPaidRows FROM dbo.Income_PaymentRecord p WHERE p.PayOrderID = po.PayOrderID) x
WHERE po.StudentID = @StudentID
  AND (po.PayFor LIKE N'%September%' OR po.PayFor LIKE N'%October%' OR po.LastPaidDate >= @HalfSavedSince)
ORDER BY po.EducationYearID, po.EndDate, po.RoleID;

/* R5: accounts ledger (Account_Log) rows written by the insert trigger for these payments.
   A session-less save writes SchoolID 0, RegistrationID 0 and Details NULL (operator name NULL). */
SELECT 'R5 account log' AS Section, al.AccountLogID, al.SchoolID, al.RegistrationID, al.AccountID, al.Amount, al.Pay_For, al.SubCategory,
       al.Details, al.Log_SN, al.Activity_Date, al.Insert_Date, al.Insert_Time
FROM dbo.Account_Log al
WHERE al.MainCategory = N'Student Payment'
  AND al.Activity_Date >= @HalfSavedSince
  AND (al.SchoolID IN (0, @StudentSchool) OR al.SchoolID IS NULL)
  AND al.AccountID IN (SELECT pr.AccountID FROM dbo.Income_PaymentRecord pr WHERE pr.StudentID = @StudentID AND pr.PaidDate >= @HalfSavedSince)
  AND (al.SchoolID = 0 OR al.Details LIKE N'%' + CONVERT(nvarchar(11), @ReceiptSN) + N'%')
ORDER BY al.AccountLogID;

/* R6: ORPHAN receipts in all schools since @OrphanSince: school 0 / NULL, collector 0 / not in Admin,
   receipt school <> student school, or payment rows in another school. (Same bug: session lost.) */
SELECT 'R6 orphan receipts' AS Section, mr.MoneyReceiptID, mr.MoneyReceipt_SN, mr.SchoolID AS ReceiptSchoolID, s.SchoolID AS StudentSchoolID,
       mr.RegistrationID, mr.StudentID, s.ID AS StudentCode, mr.EducationYearID, mr.PaidDate, mr.TotalAmount,
       (SELECT COUNT(*) FROM dbo.Income_PaymentRecord p WHERE p.MoneyReceiptID = mr.MoneyReceiptID) AS PaymentRows,
       (SELECT COUNT(*) FROM dbo.Income_PaymentRecord p JOIN dbo.Income_PayOrder po ON po.PayOrderID = p.PayOrderID
         WHERE p.MoneyReceiptID = mr.MoneyReceiptID AND po.SchoolID <> s.SchoolID) AS PayOrdersOfOtherSchool,
       CASE WHEN ISNULL(mr.SchoolID, 0) = 0 THEN 'SCHOOL 0 (session lost) - repair with pay_100022_fix.sql'
            WHEN mr.SchoolID <> s.SchoolID THEN 'receipt school <> student school'
            WHEN ISNULL(mr.RegistrationID, 0) = 0 THEN 'collector 0'
            WHEN NOT EXISTS (SELECT 1 FROM dbo.Admin ad WHERE ad.RegistrationID = mr.RegistrationID) THEN 'collector not in Admin (hidden in Paid Records popup only)'
            ELSE 'payment rows in another school' END AS Reason
FROM dbo.Income_MoneyReceipt mr
LEFT JOIN dbo.Student s ON s.StudentID = mr.StudentID
WHERE mr.PaidDate >= @OrphanSince
  AND (ISNULL(mr.SchoolID, 0) = 0 OR ISNULL(mr.RegistrationID, 0) = 0 OR mr.SchoolID <> s.SchoolID
       OR NOT EXISTS (SELECT 1 FROM dbo.Admin ad WHERE ad.RegistrationID = mr.RegistrationID)
       OR EXISTS (SELECT 1 FROM dbo.Income_PaymentRecord p WHERE p.MoneyReceiptID = mr.MoneyReceiptID AND ISNULL(p.SchoolID, 0) <> ISNULL(mr.SchoolID, 0)))
ORDER BY mr.MoneyReceiptID;

/* R7: HALF-SAVED receipts since @HalfSavedSince (all schools): no payment rows, TotalAmount NULL/0 or <> sum,
   or a pay order whose PaidAmount does not match its payment rows. (Expected: none - saves are one transaction.) */
SELECT 'R7 half-saved receipts' AS Section, mr.MoneyReceiptID, mr.MoneyReceipt_SN, mr.SchoolID, mr.StudentID, mr.PaidDate, mr.TotalAmount, x.nRows, x.SumPaid
FROM dbo.Income_MoneyReceipt mr
OUTER APPLY (SELECT COUNT(*) nRows, SUM(p.PaidAmount) SumPaid FROM dbo.Income_PaymentRecord p WHERE p.MoneyReceiptID = mr.MoneyReceiptID) x
WHERE mr.PaidDate >= @HalfSavedSince
  AND (x.nRows = 0 OR ISNULL(mr.TotalAmount, 0) = 0 OR ABS(ISNULL(x.SumPaid, 0) - ISNULL(mr.TotalAmount, 0)) > 0.001)
ORDER BY mr.MoneyReceiptID;

SELECT 'R7b pay orders <> payment rows' AS Section, po.PayOrderID, po.SchoolID, po.StudentID, po.PayFor, po.PaidAmount, x.SumPaidRows, po.LastPaidDate
FROM dbo.Income_PayOrder po
OUTER APPLY (SELECT SUM(p.PaidAmount) SumPaidRows FROM dbo.Income_PaymentRecord p WHERE p.PayOrderID = po.PayOrderID) x
WHERE po.LastPaidDate >= @HalfSavedSince
  AND ABS(ISNULL(po.PaidAmount, 0) - ISNULL(x.SumPaidRows, 0)) > 0.001
ORDER BY po.SchoolID, po.PayOrderID;

/* R8: receipt counters. Is the receipt-SN setup installed? Next number per school vs MAX (must be > MAX). */
SELECT 'R8 setup' AS Section,
       CASE WHEN OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NULL THEN 'NOT installed (old MAX+1 numbering)' ELSE 'installed' END AS usp_NextMoneyReceiptSN,
       CASE WHEN OBJECT_DEFINITION(OBJECT_ID(N'dbo.MoneyReceipt')) LIKE N'%usp_NextMoneyReceiptSN%' THEN 'uses sequence' ELSE 'old F_MoneyReceipt_SN' END AS MoneyReceipt_proc,
       (SELECT COUNT(*) FROM sys.sequences WHERE name LIKE N'Seq[_]MoneyReceipt[_]%') AS Sequences,
       ISNULL((SELECT filter_definition FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND name = N'UX_Income_MoneyReceipt_School_SN'), 'no unique index') AS UniqueIndex;

SELECT 'R8b sequence behind MAX' AS Section, x.SchoolID, x.SequenceName, x.NextToIssue, x.MaxSN,
       CASE WHEN x.NextToIssue <= ISNULL(x.MaxSN, 0) THEN 'BEHIND (would self-heal on next use; fix script re-seeds)' ELSE 'ok' END AS State
FROM (SELECT TRY_CONVERT(int, SUBSTRING(sq.name, 18, 20)) AS SchoolID, sq.name AS SequenceName,
             CASE WHEN sq.last_used_value IS NULL THEN CONVERT(bigint, sq.current_value) ELSE CONVERT(bigint, sq.last_used_value) + 1 END AS NextToIssue,
             (SELECT MAX(m.MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt m WHERE m.SchoolID = TRY_CONVERT(int, SUBSTRING(sq.name, 18, 20))) AS MaxSN
      FROM sys.sequences sq WHERE sq.name LIKE N'Seq[_]MoneyReceipt[_]%') x
WHERE x.NextToIssue <= ISNULL(x.MaxSN, 0)
ORDER BY x.SchoolID;

/* R9: duplicate receipt numbers per school (historical pairs are known; anything new is listed here too) */
SELECT 'R9 duplicate SN' AS Section, mr.SchoolID, mr.MoneyReceipt_SN, COUNT(*) AS Copies,
       STRING_AGG(CONVERT(varchar(11), mr.MoneyReceiptID) + ' (' + CONVERT(varchar(10), mr.PaidDate, 120) + ')', ', ') AS Receipts
FROM dbo.Income_MoneyReceipt mr
WHERE mr.MoneyReceipt_SN IS NOT NULL
GROUP BY mr.SchoolID, mr.MoneyReceipt_SN
HAVING COUNT(*) > 1
ORDER BY MAX(mr.MoneyReceiptID) DESC;
