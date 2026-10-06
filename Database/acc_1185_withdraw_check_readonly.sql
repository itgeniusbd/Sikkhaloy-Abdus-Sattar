/* =====================================================================================
   acc_1185_withdraw_check_readonly.sql      (SELECT ONLY - safe on live)
   Why don't the CASH withdrawals of 27-31 Aug 2026 show on Accounts/Reports/AccountDetails.aspx?

   Facts (from code + triggers):
   * Deposit_Withdraw.aspx inserts into AccountOUT_Record / AccountIN_Record.
   * Tr_AccountOUT_Record_INSERT: Account.Total_OUT += amount (AccountBalance drops) and writes Account_Log
       Add_Subtraction='Subtraction', In_Ex_type='Ex', Main/Sub/ClassOrOtherCategory='Withdraw',
       Activity_Date = AccountOUT_Date (the date typed on the form), Insert_Date = default (the day it was entered).
     (Deposit: 'Add' / 'In' / 'Deposit', Activity_Date = AccountIN_Date.)
   * The OLD AccountDetails report filters every section AND opening/closing on Account_Log.Insert_Date,
     so a withdrawal dated 27-31 Aug but entered in September is counted in September, not August.
   * The NEW report filters on Activity_Date and derives opening/closing from the current balance.
   ===================================================================================== */
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

DECLARE @SchoolID    int           = 1185;
DECLARE @From        date          = '2026-08-01';
DECLARE @To          date          = '2026-08-31';
DECLARE @AccountName nvarchar(100) = NULL;     -- e.g. N'CASH'; NULL = all accounts (report "All Account")

DECLARE @acc TABLE (AccountID int PRIMARY KEY, AccountName nvarchar(200), AccountBalance float);
INSERT @acc SELECT AccountID, AccountName, AccountBalance FROM dbo.Account
WHERE SchoolID = @SchoolID AND (@AccountName IS NULL OR AccountName = @AccountName);

/* R1: withdrawals (AccountOUT_Record) dated in the period OR entered in the period */
SELECT 'R1 withdraw rows' AS R, o.AccountOUT_ID, a.AccountName, o.AccountOUT_Amount, o.AccountOUT_Date AS WithdrawDate,
       o.Insert_Date AS EnteredOn, o.EducationYearID, o.RegistrationID, o.Out_Details,
       CASE WHEN CAST(o.Insert_Date AS date) BETWEEN @From AND @To THEN 'yes' ELSE 'NO (entered ' + CONVERT(varchar(11), o.Insert_Date, 106) + ')' END AS InOldReport,
       CASE WHEN o.AccountOUT_Date BETWEEN @From AND @To THEN 'yes' ELSE 'no' END AS InNewReport
FROM dbo.AccountOUT_Record o JOIN @acc a ON a.AccountID = o.AccountID
WHERE o.SchoolID = @SchoolID AND (o.AccountOUT_Date BETWEEN @From AND @To OR CAST(o.Insert_Date AS date) BETWEEN @From AND @To)
ORDER BY o.AccountOUT_Date, o.AccountOUT_ID;

/* R2: deposits (AccountIN_Record), same test */
SELECT 'R2 deposit rows' AS R, i.AccountIN_ID, a.AccountName, i.AccountIN_Amount, i.AccountIN_Date AS DepositDate,
       i.Insert_Date AS EnteredOn, i.EducationYearID, i.IN_Details,
       CASE WHEN CAST(i.Insert_Date AS date) BETWEEN @From AND @To THEN 'yes' ELSE 'NO' END AS InOldReport,
       CASE WHEN i.AccountIN_Date BETWEEN @From AND @To THEN 'yes' ELSE 'no' END AS InNewReport
FROM dbo.AccountIN_Record i JOIN @acc a ON a.AccountID = i.AccountID
WHERE i.SchoolID = @SchoolID AND (i.AccountIN_Date BETWEEN @From AND @To OR CAST(i.Insert_Date AS date) BETWEEN @From AND @To)
ORDER BY i.AccountIN_Date, i.AccountIN_ID;

/* R3: the Account_Log rows the triggers wrote for those deposits / withdrawals (the report's only source table) */
SELECT 'R3 Account_Log deposit/withdraw' AS R, l.AccountLogID, a.AccountName, l.MainCategory, l.Add_Subtraction, l.In_Ex_type,
       l.Amount, l.Activity_Date, l.Insert_Date, l.Insert_Time, l.Balance_Before, l.Balance_After, l.EducationYearID, l.Details
FROM dbo.Account_Log l JOIN @acc a ON a.AccountID = l.AccountID
WHERE l.SchoolID = @SchoolID AND (l.MainCategory LIKE N'%Deposit%' OR l.MainCategory LIKE N'%Withdraw%')
  AND (l.Activity_Date BETWEEN @From AND @To OR l.Insert_Date BETWEEN @From AND @To)
ORDER BY l.Activity_Date, l.AccountLogID;

/* R3b: withdraw/deposit records with NO Account_Log row (would mean the balance was not logged) */
SELECT 'R3b records without log' AS R, 'Withdraw' AS Kind, o.AccountOUT_ID AS RecID, o.AccountOUT_Amount AS Amount, o.AccountOUT_Date AS D
FROM dbo.AccountOUT_Record o JOIN @acc a ON a.AccountID = o.AccountID
WHERE o.SchoolID = @SchoolID AND o.AccountOUT_Date BETWEEN @From AND @To
  AND NOT EXISTS (SELECT 1 FROM dbo.Account_Log l WHERE l.SchoolID = @SchoolID AND l.AccountID = o.AccountID AND l.MainCategory = 'Withdraw'
                  AND l.Amount = o.AccountOUT_Amount AND l.Activity_Date = o.AccountOUT_Date)
UNION ALL
SELECT 'R3b records without log', 'Deposit', i.AccountIN_ID, i.AccountIN_Amount, i.AccountIN_Date
FROM dbo.AccountIN_Record i JOIN @acc a ON a.AccountID = i.AccountID
WHERE i.SchoolID = @SchoolID AND i.AccountIN_Date BETWEEN @From AND @To
  AND NOT EXISTS (SELECT 1 FROM dbo.Account_Log l WHERE l.SchoolID = @SchoolID AND l.AccountID = i.AccountID AND l.MainCategory = 'Deposit'
                  AND l.Amount = i.AccountIN_Amount AND l.Activity_Date = i.AccountIN_Date);

/* R4: header figures - OLD report (Insert_Date + first/last log running balance) vs NEW report (Activity_Date, balance-derived) */
;WITH L AS (
    SELECT l.AccountID, l.Amount, l.Add_Subtraction, l.Insert_Date, l.Insert_Time, l.Activity_Date, l.Balance_Before, l.Balance_After,
           CASE l.Add_Subtraction WHEN 'Add' THEN l.Amount WHEN 'Subtraction' THEN -l.Amount ELSE 0 END AS Signed
    FROM dbo.Account_Log l JOIN @acc a ON a.AccountID = l.AccountID WHERE l.SchoolID = @SchoolID)
SELECT 'R4 header old vs new' AS R, a.AccountName, CAST(a.AccountBalance AS decimal(18,2)) AS CurrentBalance,
       o.Old_Total_Add, o.Old_Total_Sub,
       (SELECT TOP (1) Balance_Before FROM L WHERE L.AccountID = a.AccountID AND Insert_Date BETWEEN @From AND @To ORDER BY Insert_Date, Insert_Time) AS Old_Opening,
       (SELECT TOP (1) Balance_After  FROM L WHERE L.AccountID = a.AccountID AND Insert_Date BETWEEN @From AND @To ORDER BY Insert_Date DESC, Insert_Time DESC) AS Old_Closing,
       n.New_Total_Add, n.New_Total_Sub,
       a.AccountBalance - ISNULL(f.Net_From, 0) AS New_Opening,
       a.AccountBalance - ISNULL(f.Net_After_To, 0) AS New_Closing,
       (a.AccountBalance - ISNULL(f.Net_From, 0)) + ISNULL(n.New_Total_Add, 0) - ISNULL(n.New_Total_Sub, 0) - (a.AccountBalance - ISNULL(f.Net_After_To, 0)) AS New_Check_ShouldBe0,
       t.SumAllLogs, CAST(a.AccountBalance - ISNULL(t.SumAllLogs, 0) AS decimal(18,2)) AS Balance_minus_Logs_ShouldBe0
FROM @acc a
OUTER APPLY (SELECT SUM(CASE WHEN Add_Subtraction = 'Add' THEN Amount END) AS Old_Total_Add, SUM(CASE WHEN Add_Subtraction = 'Subtraction' THEN Amount END) AS Old_Total_Sub
             FROM L WHERE L.AccountID = a.AccountID AND Insert_Date BETWEEN @From AND @To) o
OUTER APPLY (SELECT SUM(CASE WHEN Add_Subtraction = 'Add' THEN Amount END) AS New_Total_Add, SUM(CASE WHEN Add_Subtraction = 'Subtraction' THEN Amount END) AS New_Total_Sub
             FROM L WHERE L.AccountID = a.AccountID AND Activity_Date BETWEEN @From AND @To) n
OUTER APPLY (SELECT SUM(CASE WHEN Activity_Date >= @From THEN Signed END) AS Net_From, SUM(CASE WHEN Activity_Date > @To THEN Signed END) AS Net_After_To
             FROM L WHERE L.AccountID = a.AccountID) f
OUTER APPLY (SELECT SUM(Signed) AS SumAllLogs FROM L WHERE L.AccountID = a.AccountID) t
WHERE o.Old_Total_Add IS NOT NULL OR o.Old_Total_Sub IS NOT NULL OR n.New_Total_Add IS NOT NULL OR n.New_Total_Sub IS NOT NULL
ORDER BY a.AccountName;

/* R5: section rows OLD (Insert_Date) vs NEW (Activity_Date) - shows the Withdraw row appearing under Subtract */
;WITH S AS (
    SELECT a.AccountName,
           CASE WHEN l.Add_Subtraction = 'Add' AND l.In_Ex_type = 'In' THEN '1 Add Balance'
                WHEN l.Add_Subtraction = 'Add' THEN '2 Add by Adjustment'
                WHEN l.Add_Subtraction = 'Subtraction' AND l.In_Ex_type = 'Ex' THEN '3 Subtract Balance'
                ELSE '4 Subtract by Adjustment' END AS Section,
           l.SubCategory AS Category,
           CASE WHEN l.Insert_Date   BETWEEN @From AND @To THEN l.Amount END AS OldAmt,
           CASE WHEN l.Activity_Date BETWEEN @From AND @To THEN l.Amount END AS NewAmt
    FROM dbo.Account_Log l JOIN @acc a ON a.AccountID = l.AccountID
    WHERE l.SchoolID = @SchoolID AND (l.Insert_Date BETWEEN @From AND @To OR l.Activity_Date BETWEEN @From AND @To))
SELECT 'R5 sections old vs new' AS R, AccountName, Section, Category, SUM(OldAmt) AS Old_Report, SUM(NewAmt) AS New_Report,
       ISNULL(SUM(NewAmt), 0) - ISNULL(SUM(OldAmt), 0) AS Diff
FROM S GROUP BY AccountName, Section, Category
ORDER BY AccountName, Section, Category;

/* R6: back-dated entries = the rows whose month differs between Insert_Date and Activity_Date (why old report != new) */
SELECT 'R6 back/forward-dated in period' AS R, a.AccountName, l.MainCategory, l.SubCategory, COUNT(*) AS Rows, SUM(l.Amount) AS Amount,
       MIN(l.Activity_Date) AS FirstActivity, MAX(l.Insert_Date) AS LastEntered
FROM dbo.Account_Log l JOIN @acc a ON a.AccountID = l.AccountID
WHERE l.SchoolID = @SchoolID
  AND ((l.Activity_Date BETWEEN @From AND @To AND l.Insert_Date NOT BETWEEN @From AND @To)
    OR (l.Insert_Date BETWEEN @From AND @To AND l.Activity_Date NOT BETWEEN @From AND @To))
GROUP BY a.AccountName, l.MainCategory, l.SubCategory
ORDER BY Amount DESC;

/* R7: closing (new) -> current balance: everything dated after @To, by category */
SELECT 'R7 after period (closing -> current)' AS R, a.AccountName, l.Add_Subtraction, l.MainCategory, COUNT(*) AS Rows, SUM(l.Amount) AS Amount
FROM dbo.Account_Log l JOIN @acc a ON a.AccountID = l.AccountID
WHERE l.SchoolID = @SchoolID AND l.Activity_Date > @To
GROUP BY a.AccountName, l.Add_Subtraction, l.MainCategory
ORDER BY a.AccountName, l.Add_Subtraction, Amount DESC;

/* R8: Account_Log rows of these accounts filed under another SchoolID (excluded by the report) */
SELECT 'R8 logs under other SchoolID' AS R, l.SchoolID, a.AccountName, COUNT(*) AS Rows, SUM(l.Amount) AS Amount
FROM dbo.Account_Log l JOIN @acc a ON a.AccountID = l.AccountID
WHERE l.SchoolID <> @SchoolID
GROUP BY l.SchoolID, a.AccountName;
