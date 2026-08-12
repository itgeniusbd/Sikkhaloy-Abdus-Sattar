/*
================================================================================
  Enable Expenditure triggers + backfill missing expense Account_Log rows
  (same root cause: sp_ResetInstitutionData left DISABLE TRIGGER behind)

  Balance formula:
    AccountBalance = (Total_IN + Total_Income + Deleted_Expense)
                   - (Total_OUT + Total_Expense + Deleted_Income)
    Expense INSERT trigger does: Total_Expense += Amount  → balance decreases

  HOW TO RUN (LIVE):
    1) Step 0 enables triggers
    2) DryRun=1 → see Missing_Amount
    3) DryRun=0, AutoCommit=1 → save
================================================================================
*/
USE [Edu];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SchoolID   INT  = NULL;            -- NULL = all schools; or 1280 for one
DECLARE @FromDate   DATE = '2026-08-11';
DECLARE @ToDate     DATE = '2026-08-11';
DECLARE @DryRun     BIT  = 1;
DECLARE @AutoCommit BIT  = 0;

PRINT '=== Step 0: Enable expense / out ledger triggers ===';

ENABLE TRIGGER ALL ON dbo.Expenditure;
ENABLE TRIGGER ALL ON dbo.AccountOUT_Record;
ENABLE TRIGGER ALL ON dbo.AccountIN_Record;
ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord;
ENABLE TRIGGER ALL ON dbo.Extra_Income;
ENABLE TRIGGER ALL ON dbo.Income_PayOrder;

SELECT
    OBJECT_NAME(t.parent_id) AS TableName,
    t.name AS TriggerName,
    t.is_disabled
FROM sys.triggers t
WHERE t.parent_id IN (
    OBJECT_ID(N'dbo.Expenditure'),
    OBJECT_ID(N'dbo.AccountOUT_Record'),
    OBJECT_ID(N'dbo.AccountIN_Record')
)
ORDER BY TableName, TriggerName;

IF EXISTS (
    SELECT 1 FROM sys.triggers t
    WHERE t.is_disabled = 1
      AND t.parent_id = OBJECT_ID(N'dbo.Expenditure')
)
BEGIN
    RAISERROR(N'ABORT: Expenditure triggers still disabled.', 16, 1);
    RETURN;
END

PRINT '=== Step 1: Find expenses missing from Account_Log ===';

;WITH Exp AS (
    SELECT
        e.ExpenseID,
        e.SchoolID,
        e.RegistrationID,
        e.EducationYearID,
        e.Amount,
        e.ExpenseFor,
        CAST(e.ExpenseDate AS date) AS ExpenseDate,
        e.AccountID,
        e.ExpenseCategoryID,
        c.CategoryName,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(ad.FirstName, N'') + N' ' + ISNULL(ad.LastName, N''))), N''), reg.UserName) AS OperatedBy,
        ROW_NUMBER() OVER (
            PARTITION BY e.SchoolID, e.Amount, c.CategoryName, CAST(e.ExpenseDate AS date), e.AccountID
            ORDER BY e.ExpenseID
        ) AS rn
    FROM dbo.Expenditure AS e
    INNER JOIN dbo.Expense_CategoryName AS c
        ON e.ExpenseCategoryID = c.ExpenseCategoryID
    INNER JOIN dbo.Registration AS reg
        ON e.RegistrationID = reg.RegistrationID
    LEFT JOIN dbo.Admin AS ad
        ON ad.RegistrationID = e.RegistrationID
    WHERE (@SchoolID IS NULL OR e.SchoolID = @SchoolID)
      AND CAST(e.ExpenseDate AS date) BETWEEN @FromDate AND @ToDate
      AND e.AccountID IS NOT NULL
      AND c.CategoryName IS NOT NULL
),
LogExp AS (
    SELECT
        al.AccountLogID,
        al.SchoolID,
        al.Amount,
        al.SubCategory,
        al.AccountID,
        al.Activity_Date,
        ROW_NUMBER() OVER (
            PARTITION BY al.SchoolID, al.Amount, al.SubCategory, al.Activity_Date, al.AccountID
            ORDER BY al.AccountLogID
        ) AS rn
    FROM dbo.Account_Log AS al
    WHERE (@SchoolID IS NULL OR al.SchoolID = @SchoolID)
      AND al.In_Ex_type = 'Ex'
      AND al.Insert_Up_De = 'In'
      AND al.MainCategory = N'Expense'
      AND (
            al.Activity_Date BETWEEN @FromDate AND @ToDate
         OR al.Insert_Date BETWEEN @FromDate AND @ToDate
      )
)
SELECT
    x.*
INTO #MissingExpense
FROM Exp AS x
LEFT JOIN LogExp AS l
    ON l.SchoolID = x.SchoolID
   AND ABS(l.Amount - x.Amount) < 0.01
   AND l.SubCategory = x.CategoryName
   AND l.Activity_Date = x.ExpenseDate
   AND l.AccountID = x.AccountID
   AND l.rn = x.rn
WHERE l.AccountLogID IS NULL;

SELECT
    COUNT(*) AS Missing_Expense_Rows,
    ISNULL(SUM(Amount), 0) AS Missing_Expense_Amount
FROM #MissingExpense;

SELECT SchoolID, COUNT(*) AS RowsCnt, SUM(Amount) AS Amount
FROM #MissingExpense
GROUP BY SchoolID
ORDER BY Amount DESC;

SELECT *
FROM #MissingExpense
ORDER BY SchoolID, ExpenseDate, ExpenseID;

IF @DryRun = 1
BEGIN
    PRINT '=== DRY RUN only. Set @DryRun=0, @AutoCommit=1 to SAVE. ===';
    DROP TABLE IF EXISTS #MissingExpense;
    RETURN;
END

PRINT '=== Step 2: APPLY (mirrors Tr_Expenditure_Insert) ===';

BEGIN TRAN;

DECLARE
    @ExpenseID INT,
    @SchID INT,
    @RegistrationID INT,
    @EducationYearID INT,
    @Amount FLOAT,
    @ExpenseFor NVARCHAR(256),
    @ExpenseDate DATE,
    @AccountID INT,
    @ExpenseCategoryID INT,
    @CategoryName NVARCHAR(128),
    @OperatedBy NVARCHAR(128),
    @Balance_Before FLOAT,
    @Balance_After FLOAT,
    @Inserted INT = 0;

DECLARE curEx CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        ExpenseID, SchoolID, RegistrationID, EducationYearID, Amount, ExpenseFor,
        ExpenseDate, AccountID, ExpenseCategoryID, CategoryName, OperatedBy
    FROM #MissingExpense
    ORDER BY ExpenseDate, ExpenseID;

OPEN curEx;
FETCH NEXT FROM curEx INTO
    @ExpenseID, @SchID, @RegistrationID, @EducationYearID, @Amount, @ExpenseFor,
    @ExpenseDate, @AccountID, @ExpenseCategoryID, @CategoryName, @OperatedBy;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @Balance_Before = AccountBalance
    FROM dbo.Account WITH (UPDLOCK, ROWLOCK)
    WHERE AccountID = @AccountID;

    UPDATE dbo.Account
    SET Total_Expense = Total_Expense + @Amount
    WHERE AccountID = @AccountID;

    SELECT @Balance_After = AccountBalance
    FROM dbo.Account
    WHERE AccountID = @AccountID;

    INSERT INTO dbo.Account_Log (
        AccountID, SchoolID, RegistrationID, EducationYearID, Amount,
        Add_Subtraction, Pay_For, MainCategory, ClassOrOtherCategory, SubCategory,
        Details, Log_SN, Balance_Before, Balance_After, Activity_Date,
        In_Ex_type, Insert_Up_De, Insert_Date, Insert_Time
    )
    VALUES (
        @AccountID, @SchID, @RegistrationID, @EducationYearID, @Amount,
        N'Subtraction', @ExpenseFor, N'Expense', N'Expense', @CategoryName,
        N'Expense Amount inputted ' + CAST(@Amount AS varchar(50))
            + N' Tk. ' + ISNULL(N'Expense Reason: ' + @ExpenseFor, N'')
            + N' Operated By: ' + ISNULL(@OperatedBy, N''),
        dbo.Account_Log_SerialNumber(@SchID),
        @Balance_Before, @Balance_After, @ExpenseDate,
        'Ex', 'In', @ExpenseDate, CAST('00:00:00' AS time(7))
    );

    SET @Inserted += 1;

    FETCH NEXT FROM curEx INTO
        @ExpenseID, @SchID, @RegistrationID, @EducationYearID, @Amount, @ExpenseFor,
        @ExpenseDate, @AccountID, @ExpenseCategoryID, @CategoryName, @OperatedBy;
END

CLOSE curEx;
DEALLOCATE curEx;

SELECT
    @Inserted AS Inserted_Expense_Log_Rows,
    (SELECT COUNT(*) FROM #MissingExpense) AS Expected_Rows;

/* Sample check for school 1280 Cash */
IF @SchoolID = 1280 OR @SchoolID IS NULL
BEGIN
    SELECT AccountID, AccountName, AccountBalance, Total_Expense, Total_Income
    FROM dbo.Account
    WHERE SchoolID = ISNULL(@SchoolID, 1280)
    ORDER BY AccountName;
END

IF @AutoCommit = 1
BEGIN
    COMMIT TRAN;
    PRINT '=== COMMITTED. Expense backfill saved. New expenses will now reduce balance. ===';
END
ELSE
BEGIN
    ROLLBACK TRAN;
    PRINT '=== ROLLED BACK. Re-run with @AutoCommit=1 to save. ===';
END

DROP TABLE IF EXISTS #MissingExpense;
GO
