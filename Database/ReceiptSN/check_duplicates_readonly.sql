/* =====================================================================
   check_duplicates_readonly.sql            (SELECT-ONLY, changes nothing)
   Run on the live DB (Edu) BEFORE receipt_sn_setup.sql.
   Result set 2 = historical duplicates. receipt_sn_setup.sql keeps them
   (no renumbering) and builds the unique index only for rows with
   MoneyReceiptID above the largest MoneyReceiptID in result set 2; if that
   value is higher than @ReviewedDuplicateCutoff in the setup (Step 3), the
   index is refused until the new duplicates are reviewed.
   ===================================================================== */
SET NOCOUNT ON;

/* 1) Summary */
SELECT COUNT(*)                                             AS TotalReceipts,
       COUNT(DISTINCT SchoolID)                             AS Schools,
       SUM(CASE WHEN MoneyReceipt_SN IS NULL THEN 1 ELSE 0 END) AS NullSN_Rows,
       SUM(CASE WHEN SchoolID IS NULL THEN 1 ELSE 0 END)        AS NullSchool_Rows
FROM dbo.Income_MoneyReceipt WITH (NOLOCK);

/* 2) Duplicate (SchoolID, MoneyReceipt_SN) pairs, all schools */
SELECT SchoolID, MoneyReceipt_SN, COUNT(*) AS Copies,
       MIN(MoneyReceiptID) AS FirstMoneyReceiptID, MAX(MoneyReceiptID) AS LastMoneyReceiptID,
       MIN(ISNULL(CollectionDate, PaidDate)) AS FirstDate, MAX(ISNULL(CollectionDate, PaidDate)) AS LastDate
FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
WHERE SchoolID IS NOT NULL AND MoneyReceipt_SN IS NOT NULL
GROUP BY SchoolID, MoneyReceipt_SN
HAVING COUNT(*) > 1
ORDER BY SchoolID, MoneyReceipt_SN;

/* 3) Every row involved in a duplicate, with student, collector, amounts, payment rows */
;WITH d AS (
    SELECT SchoolID, MoneyReceipt_SN
    FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
    WHERE SchoolID IS NOT NULL AND MoneyReceipt_SN IS NOT NULL
    GROUP BY SchoolID, MoneyReceipt_SN
    HAVING COUNT(*) > 1
)
SELECT mr.SchoolID, mr.MoneyReceipt_SN, mr.MoneyReceiptID, mr.PaidDate, mr.CollectionDate,
       mr.TotalAmount, mr.PrintedReceiptNo, mr.PaymentBy, mr.EducationYearID,
       s.ID AS StudentCode, s.StudentsName,
       mr.RegistrationID, LTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N'')) AS CollectedBy,
       (SELECT COUNT(*) FROM dbo.Income_PaymentRecord pr WITH (NOLOCK) WHERE pr.MoneyReceiptID = mr.MoneyReceiptID) AS PaymentRows,
       (SELECT SUM(pr.PaidAmount) FROM dbo.Income_PaymentRecord pr WITH (NOLOCK) WHERE pr.MoneyReceiptID = mr.MoneyReceiptID) AS PaymentSum
FROM d
JOIN dbo.Income_MoneyReceipt mr WITH (NOLOCK) ON mr.SchoolID = d.SchoolID AND mr.MoneyReceipt_SN = d.MoneyReceipt_SN
LEFT JOIN dbo.Student s WITH (NOLOCK) ON s.StudentID = mr.StudentID
LEFT JOIN dbo.Admin   a WITH (NOLOCK) ON a.RegistrationID = mr.RegistrationID AND a.SchoolID = mr.SchoolID
ORDER BY mr.SchoolID, mr.MoneyReceipt_SN, mr.MoneyReceiptID;

/* 4) Duplicates per school (how many pairs each school has) */
SELECT x.SchoolID, COUNT(*) AS DuplicatePairs, SUM(x.Copies - 1) AS ExtraRows
FROM (SELECT SchoolID, MoneyReceipt_SN, COUNT(*) AS Copies
      FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
      WHERE SchoolID IS NOT NULL AND MoneyReceipt_SN IS NOT NULL
      GROUP BY SchoolID, MoneyReceipt_SN HAVING COUNT(*) > 1) x
GROUP BY x.SchoolID ORDER BY DuplicatePairs DESC;

/* 5) Current state of the objects the setup script touches (definition of dbo.MoneyReceipt:
      compare with the one in receipt_sn_setup.sql; the setup keeps the same parameters/result) */
SELECT OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') AS usp_Next_ObjectId,
       OBJECT_ID(N'dbo.MoneyReceipt', N'P')           AS MoneyReceipt_ObjectId,
       (SELECT COUNT(*) FROM sys.sequences WHERE name LIKE N'Seq[_]MoneyReceipt[_]%') AS ExistingSequences;
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.MoneyReceipt')) AS Current_MoneyReceipt_Definition;
SELECT i.name, i.is_unique, i.has_filter, i.filter_definition,
       STUFF((SELECT N', ' + c.name + CASE WHEN ic.is_descending_key = 1 THEN N' DESC' ELSE N'' END
              FROM sys.index_columns ic JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
              WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
              ORDER BY ic.key_ordinal FOR XML PATH('')), 1, 2, N'') AS KeyColumns
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND i.index_id > 0
ORDER BY i.index_id;

/* 6) SchoolID 0 receipts: 0 is the SchoolID of Authority / Sub-Authority logins, not a school.
      The setup creates no Seq_MoneyReceipt_0, and after setup a collection with SchoolID 0
      fails with error 51000 instead of creating a receipt that belongs to no school. */
SELECT (SELECT COUNT(*) FROM dbo.SchoolInfo WITH (NOLOCK) WHERE SchoolID = 0) AS SchoolInfo_rows_with_SchoolID_0,
       COUNT(*) AS Receipts_SchoolID_0, MIN(MoneyReceipt_SN) AS MinSN, MAX(MoneyReceipt_SN) AS MaxSN,
       MIN(ISNULL(CollectionDate, PaidDate)) AS FirstDate, MAX(ISNULL(CollectionDate, PaidDate)) AS LastDate
FROM dbo.Income_MoneyReceipt WITH (NOLOCK) WHERE SchoolID = 0;
SELECT mr.RegistrationID, r.UserName, r.Category, r.SchoolID AS LoginSchoolID, COUNT(*) AS Receipts,
       MAX(ISNULL(mr.CollectionDate, mr.PaidDate)) AS LastDate
FROM dbo.Income_MoneyReceipt mr WITH (NOLOCK)
LEFT JOIN dbo.Registration r WITH (NOLOCK) ON r.RegistrationID = mr.RegistrationID
WHERE mr.SchoolID = 0
GROUP BY mr.RegistrationID, r.UserName, r.Category, r.SchoolID
ORDER BY Receipts DESC;
