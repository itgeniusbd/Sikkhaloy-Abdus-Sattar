/*
================================================================================
  Backfill Account_Log for payments taken while ledger triggers were DISABLED
  (e.g. after sp_ResetInstitutionData left DISABLE TRIGGER behind)

  What this fixes:
    - Income.aspx / Net.aspx show Income_PaymentRecord totals
    - Account_Log / Cash balance missed rows because Tr_Income_PaymentRecord_INSERT
      (and Extra_Income INSERT) were off

  HOW TO RUN (LIVE):
    1) First ENABLE all ledger triggers (script Step 0 does this)
    2) Set @SchoolID / @FromDate / @ToDate
    3) Keep @DryRun = 1, @AutoCommit = 0  → review missing rows/amount (~20060)
    4) Set @DryRun = 0, keep @AutoCommit = 0 → trial apply then ROLLBACK (see verify)
    5) Set @DryRun = 0, @AutoCommit = 1 → real save
    6) Check Account Log + Cash balance on site

  Safe to re-run: only inserts rows that still have no matching Account_Log.
================================================================================
*/
USE [Edu];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ===================== PARAMETERS ===================== */
DECLARE @SchoolID   INT  = 1002;          -- change if needed; NULL = ALL schools
DECLARE @FromDate   DATE = '2026-08-11';  -- inclusive
DECLARE @ToDate     DATE = '2026-08-11';  -- inclusive
DECLARE @DryRun     BIT  = 1;             -- 1 = preview only, 0 = build/apply inside TRAN
DECLARE @AutoCommit BIT  = 0;             -- 0 = ROLLBACK after verify (safe), 1 = COMMIT

PRINT '=== Step 0: Ensure ledger triggers are ENABLED ===';

ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord;
ENABLE TRIGGER ALL ON dbo.AccountIN_Record;
ENABLE TRIGGER ALL ON dbo.AccountOUT_Record;
ENABLE TRIGGER ALL ON dbo.Income_PayOrder;
IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL
    ENABLE TRIGGER ALL ON dbo.Extra_Income;
IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL
    ENABLE TRIGGER ALL ON dbo.Expenditure;

SELECT
    OBJECT_NAME(t.parent_id) AS TableName,
    t.name AS TriggerName,
    t.is_disabled
FROM sys.triggers t
WHERE t.parent_id IN (
    OBJECT_ID(N'dbo.Income_PaymentRecord'),
    OBJECT_ID(N'dbo.AccountIN_Record'),
    OBJECT_ID(N'dbo.AccountOUT_Record'),
    OBJECT_ID(N'dbo.Income_PayOrder'),
    OBJECT_ID(N'dbo.Extra_Income'),
    OBJECT_ID(N'dbo.Expenditure')
)
ORDER BY TableName, TriggerName;

IF EXISTS (
    SELECT 1
    FROM sys.triggers t
    WHERE t.is_disabled = 1
      AND t.parent_id IN (
          OBJECT_ID(N'dbo.Income_PaymentRecord'),
          OBJECT_ID(N'dbo.AccountIN_Record'),
          OBJECT_ID(N'dbo.AccountOUT_Record'),
          OBJECT_ID(N'dbo.Income_PayOrder'),
          OBJECT_ID(N'dbo.Extra_Income'),
          OBJECT_ID(N'dbo.Expenditure')
      )
)
BEGIN
    RAISERROR(N'ABORT: some ledger triggers are still disabled. Enable them first.', 16, 1);
    RETURN;
END

PRINT '=== Step 1: Find MISSING student payments (Income_PaymentRecord) ===';

;WITH Pay AS (
    SELECT
        pr.PaymentRecordID,
        pr.SchoolID,
        pr.RegistrationID,
        pr.EducationYearID,
        pr.PaidAmount,
        pr.StudentClassID,
        pr.RoleID,
        pr.PayFor,
        CAST(pr.PaidDate AS date) AS PaidDate,
        CAST(pr.PaidDate AS time(7)) AS PaidTime,
        pr.AccountID,
        pr.MoneyReceiptID,
        /* keep as INT for clean match with Account_Log Details parse */
        TRY_CAST(mr.MoneyReceipt_SN AS INT) AS MoneyReceipt_SN,
        ir.Role AS Situation_Role,
        cc.Class AS Category_Class,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(ad.FirstName, N'') + N' ' + ISNULL(ad.LastName, N''))), N''), reg.UserName) AS OperatedBy,
        ROW_NUMBER() OVER (
            PARTITION BY pr.SchoolID, TRY_CAST(mr.MoneyReceipt_SN AS INT), pr.PaidAmount, ir.Role
            ORDER BY pr.PaymentRecordID
        ) AS rn
    FROM dbo.Income_PaymentRecord AS pr
    INNER JOIN dbo.Income_MoneyReceipt AS mr
        ON pr.MoneyReceiptID = mr.MoneyReceiptID
    INNER JOIN dbo.Income_Roles AS ir
        ON pr.RoleID = ir.RoleID
    INNER JOIN dbo.Registration AS reg
        ON pr.RegistrationID = reg.RegistrationID
    LEFT JOIN dbo.Admin AS ad
        ON ad.RegistrationID = pr.RegistrationID
    LEFT JOIN dbo.StudentsClass AS sc
        ON pr.StudentClassID = sc.StudentClassID
    LEFT JOIN dbo.CreateClass AS cc
        ON sc.ClassID = cc.ClassID
    WHERE (@SchoolID IS NULL OR pr.SchoolID = @SchoolID)
      AND CAST(pr.PaidDate AS date) BETWEEN @FromDate AND @ToDate
      AND pr.AccountID IS NOT NULL
      AND ir.Role IS NOT NULL
      AND cc.Class IS NOT NULL
),
LogRows AS (
    /* Details example: 'Receipt No: 152351. Collected  Tuition Fee ...'
       Must extract ONLY the digits before '.' — old parse kept '. Collected...' and matched NOTHING,
       so every payment looked missing (74860 instead of ~20060). */
    SELECT
        al.AccountLogID,
        al.SchoolID,
        al.Amount,
        al.SubCategory,
        sn.MoneyReceipt_SN,
        ROW_NUMBER() OVER (
            PARTITION BY al.SchoolID, sn.MoneyReceipt_SN, al.Amount, al.SubCategory
            ORDER BY al.AccountLogID
        ) AS rn
    FROM dbo.Account_Log AS al
    CROSS APPLY (
        SELECT
            TRY_CAST(
                LEFT(
                    rest.Txt,
                    NULLIF(PATINDEX(N'%[^0-9]%', rest.Txt + N'x'), 0) - 1
                ) AS INT
            ) AS MoneyReceipt_SN
        FROM (
            SELECT LTRIM(SUBSTRING(
                al.Details,
                CHARINDEX(N'Receipt No:', al.Details) + LEN(N'Receipt No:'),
                40
            )) AS Txt
        ) AS rest
    ) AS sn
    WHERE (@SchoolID IS NULL OR al.SchoolID = @SchoolID)
      AND al.In_Ex_type = 'In'
      AND al.Insert_Up_De = 'In'
      AND al.MainCategory = N'Student Payment'
      AND al.Details LIKE N'%Receipt No:%'
      AND (
            al.Activity_Date BETWEEN @FromDate AND @ToDate
         OR al.Insert_Date BETWEEN @FromDate AND @ToDate
      )
      AND sn.MoneyReceipt_SN IS NOT NULL
)
SELECT
    p.PaymentRecordID,
    p.SchoolID,
    p.MoneyReceipt_SN,
    p.PaidDate,
    p.PaidTime,
    p.Situation_Role,
    p.Category_Class,
    p.PaidAmount,
    p.AccountID,
    p.OperatedBy,
    p.PayFor,
    p.RegistrationID,
    p.EducationYearID,
    p.StudentClassID,
    p.RoleID,
    p.MoneyReceiptID
INTO #MissingStudentPay
FROM Pay AS p
LEFT JOIN LogRows AS l
    ON l.SchoolID = p.SchoolID
   AND l.MoneyReceipt_SN = p.MoneyReceipt_SN
   AND ABS(l.Amount - p.PaidAmount) < 0.01
   AND l.SubCategory = p.Situation_Role
   AND l.rn = p.rn
WHERE l.AccountLogID IS NULL;

SELECT
    COUNT(*) AS Missing_Rows,
    ISNULL(SUM(PaidAmount), 0) AS Missing_Amount
FROM #MissingStudentPay;

SELECT *
FROM #MissingStudentPay
ORDER BY PaidDate, MoneyReceipt_SN, PaymentRecordID;

PRINT '=== Step 2: Find MISSING Extra_Income (if any) ===';

;WITH Extra AS (
    SELECT
        ei.Extra_IncomeID,
        ei.SchoolID,
        ei.RegistrationID,
        ei.EducationYearID,
        ei.Extra_IncomeCategoryID,
        ei.Extra_IncomeAmount,
        ei.Extra_IncomeFor,
        CAST(ei.Extra_IncomeDate AS date) AS Extra_IncomeDate,
        ei.AccountID,
        eic.Extra_Income_CategoryName,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(ad.FirstName, N'') + N' ' + ISNULL(ad.LastName, N''))), N''), reg.UserName) AS OperatedBy,
        ROW_NUMBER() OVER (
            PARTITION BY ei.SchoolID, ei.Extra_IncomeAmount, eic.Extra_Income_CategoryName, CAST(ei.Extra_IncomeDate AS date)
            ORDER BY ei.Extra_IncomeID
        ) AS rn
    FROM dbo.Extra_Income AS ei
    INNER JOIN dbo.Extra_IncomeCategory AS eic
        ON ei.Extra_IncomeCategoryID = eic.Extra_IncomeCategoryID
    INNER JOIN dbo.Registration AS reg
        ON ei.RegistrationID = reg.RegistrationID
    LEFT JOIN dbo.Admin AS ad
        ON ad.RegistrationID = ei.RegistrationID
    WHERE (@SchoolID IS NULL OR ei.SchoolID = @SchoolID)
      AND ei.Extra_IncomeDate BETWEEN @FromDate AND @ToDate
      AND ei.AccountID IS NOT NULL
),
LogExtra AS (
    SELECT
        al.AccountLogID,
        al.SchoolID,
        al.Amount,
        al.SubCategory,
        al.Activity_Date,
        ROW_NUMBER() OVER (
            PARTITION BY al.SchoolID, al.Amount, al.SubCategory, al.Activity_Date
            ORDER BY al.AccountLogID
        ) AS rn
    FROM dbo.Account_Log AS al
    WHERE (@SchoolID IS NULL OR al.SchoolID = @SchoolID)
      AND al.In_Ex_type = 'In'
      AND al.Insert_Up_De = 'In'
      AND al.MainCategory = N'Other Income'
      AND al.Activity_Date BETWEEN @FromDate AND @ToDate
)
SELECT
    e.*
INTO #MissingExtra
FROM Extra AS e
LEFT JOIN LogExtra AS l
    ON l.SchoolID = e.SchoolID
   AND l.Amount = e.Extra_IncomeAmount
   AND l.SubCategory = e.Extra_Income_CategoryName
   AND l.Activity_Date = e.Extra_IncomeDate
   AND l.rn = e.rn
WHERE l.AccountLogID IS NULL;

SELECT
    COUNT(*) AS Missing_Extra_Rows,
    ISNULL(SUM(Extra_IncomeAmount), 0) AS Missing_Extra_Amount
FROM #MissingExtra;

SELECT *
FROM #MissingExtra
ORDER BY Extra_IncomeDate, Extra_IncomeID;

IF @DryRun = 1
BEGIN
    PRINT '=== DRY RUN only. Set @DryRun = 0 to APPLY. ===';
    DROP TABLE IF EXISTS #MissingStudentPay;
    DROP TABLE IF EXISTS #MissingExtra;
    RETURN;
END

PRINT '=== Step 3: APPLY backfill (mirrors Tr_Income_PaymentRecord_INSERT / Tr_Extra_Income_INSERT) ===';

BEGIN TRAN;

DECLARE
    @PaymentRecordID INT,
    @SchID INT,
    @RegistrationID INT,
    @EducationYearID INT,
    @PaidAmount FLOAT,
    @StudentClassID INT,
    @RoleID INT,
    @PayFor NVARCHAR(50),
    @PaidDate DATE,
    @PaidTime TIME(7),
    @AccountID INT,
    @MoneyReceiptID INT,
    @MoneyReceipt_SN INT,
    @Situation_Role NVARCHAR(50),
    @Category_Class NVARCHAR(50),
    @OperatedBy NVARCHAR(128),
    @Balance_Before FLOAT,
    @Balance_After FLOAT,
    @InsertedStudent INT = 0,
    @InsertedExtra INT = 0;

DECLARE curPay CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        PaymentRecordID, SchoolID, RegistrationID, EducationYearID, PaidAmount,
        StudentClassID, RoleID, PayFor, PaidDate, PaidTime, AccountID, MoneyReceiptID,
        MoneyReceipt_SN, Situation_Role, Category_Class, OperatedBy
    FROM #MissingStudentPay
    ORDER BY PaidDate, PaidTime, PaymentRecordID;

OPEN curPay;
FETCH NEXT FROM curPay INTO
    @PaymentRecordID, @SchID, @RegistrationID, @EducationYearID, @PaidAmount,
    @StudentClassID, @RoleID, @PayFor, @PaidDate, @PaidTime, @AccountID, @MoneyReceiptID,
    @MoneyReceipt_SN, @Situation_Role, @Category_Class, @OperatedBy;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @Balance_Before = AccountBalance
    FROM dbo.Account WITH (UPDLOCK, ROWLOCK)
    WHERE AccountID = @AccountID;

    UPDATE dbo.Account
    SET Total_Income = Total_Income + @PaidAmount
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
        @AccountID, @SchID, @RegistrationID, @EducationYearID, @PaidAmount,
        N'Add', @PayFor, N'Student Payment', @Category_Class, @Situation_Role,
        N'Receipt No: ' + CAST(@MoneyReceipt_SN AS nvarchar(20))
            + N'. Collected  ' + @Situation_Role + N' '
            + CAST(@PaidAmount AS varchar(50)) + N' Tk. Operated By: ' + ISNULL(@OperatedBy, N''),
        dbo.Account_Log_SerialNumber(@SchID),
        @Balance_Before, @Balance_After, @PaidDate,
        'In', 'In', @PaidDate, @PaidTime
    );

    SET @InsertedStudent += 1;

    FETCH NEXT FROM curPay INTO
        @PaymentRecordID, @SchID, @RegistrationID, @EducationYearID, @PaidAmount,
        @StudentClassID, @RoleID, @PayFor, @PaidDate, @PaidTime, @AccountID, @MoneyReceiptID,
        @MoneyReceipt_SN, @Situation_Role, @Category_Class, @OperatedBy;
END

CLOSE curPay;
DEALLOCATE curPay;

/* Extra_Income backfill */
DECLARE
    @Extra_IncomeID INT,
    @Extra_IncomeCategoryID INT,
    @Extra_IncomeAmount FLOAT,
    @Extra_IncomeFor NVARCHAR(256),
    @Extra_IncomeDate DATE,
    @Extra_Income_CategoryName NVARCHAR(128);

DECLARE curEx CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        Extra_IncomeID, SchoolID, RegistrationID, EducationYearID, Extra_IncomeCategoryID,
        Extra_IncomeAmount, Extra_IncomeFor, Extra_IncomeDate, AccountID,
        Extra_Income_CategoryName, OperatedBy
    FROM #MissingExtra
    ORDER BY Extra_IncomeDate, Extra_IncomeID;

OPEN curEx;
FETCH NEXT FROM curEx INTO
    @Extra_IncomeID, @SchID, @RegistrationID, @EducationYearID, @Extra_IncomeCategoryID,
    @Extra_IncomeAmount, @Extra_IncomeFor, @Extra_IncomeDate, @AccountID,
    @Extra_Income_CategoryName, @OperatedBy;

WHILE @@FETCH_STATUS = 0
BEGIN
    UPDATE dbo.Extra_IncomeCategory
    SET Total_Extra_Income = Total_Extra_Income + @Extra_IncomeAmount
    WHERE Extra_IncomeCategoryID = @Extra_IncomeCategoryID;

    SELECT @Balance_Before = AccountBalance
    FROM dbo.Account WITH (UPDLOCK, ROWLOCK)
    WHERE AccountID = @AccountID;

    UPDATE dbo.Account
    SET Total_Income = Total_Income + @Extra_IncomeAmount
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
        @AccountID, @SchID, @RegistrationID, @EducationYearID, @Extra_IncomeAmount,
        N'Add', @Extra_IncomeFor, N'Other Income', N'Other Income', @Extra_Income_CategoryName,
        N'Others payment inputted ' + CAST(@Extra_IncomeAmount AS varchar(50))
            + N' Tk. ' + ISNULL(N'Payment For : ' + @Extra_IncomeFor, N'')
            + N' Operated By: ' + ISNULL(@OperatedBy, N''),
        dbo.Account_Log_SerialNumber(@SchID),
        @Balance_Before, @Balance_After, @Extra_IncomeDate,
        'In', 'In', @Extra_IncomeDate, CAST('00:00:00' AS time(7))
    );

    SET @InsertedExtra += 1;

    FETCH NEXT FROM curEx INTO
        @Extra_IncomeID, @SchID, @RegistrationID, @EducationYearID, @Extra_IncomeCategoryID,
        @Extra_IncomeAmount, @Extra_IncomeFor, @Extra_IncomeDate, @AccountID,
        @Extra_Income_CategoryName, @OperatedBy;
END

CLOSE curEx;
DEALLOCATE curEx;

PRINT '=== Step 4: Verify after apply ===';

DECLARE @IncomeTotal FLOAT =
(
    SELECT ISNULL(SUM(PaidAmount), 0)
    FROM dbo.Income_PaymentRecord
    WHERE (@SchoolID IS NULL OR SchoolID = @SchoolID)
      AND CAST(PaidDate AS date) BETWEEN @FromDate AND @ToDate
)
+
(
    SELECT ISNULL(SUM(Extra_IncomeAmount), 0)
    FROM dbo.Extra_Income
    WHERE (@SchoolID IS NULL OR SchoolID = @SchoolID)
      AND Extra_IncomeDate BETWEEN @FromDate AND @ToDate
);

DECLARE @LogTotal FLOAT =
(
    SELECT ISNULL(SUM(Amount), 0)
    FROM dbo.Account_Log
    WHERE (@SchoolID IS NULL OR SchoolID = @SchoolID)
      AND In_Ex_type = 'In'
      AND Insert_Up_De = 'In'
      AND Insert_Date BETWEEN @FromDate AND @ToDate
);

SELECT
    @InsertedStudent AS Inserted_Student_Log_Rows,
    @InsertedExtra AS Inserted_Extra_Log_Rows,
    @IncomeTotal AS Income_Page_Style_Total,
    @LogTotal AS AccountLog_Income_Total,
    @IncomeTotal - @LogTotal AS Remaining_Diff;

SELECT AccountID, AccountName, AccountBalance, Total_Income
FROM dbo.Account
WHERE (@SchoolID IS NULL OR SchoolID = @SchoolID)
ORDER BY AccountName;

IF @AutoCommit = 1
BEGIN
    COMMIT TRAN;
    PRINT '=== COMMITTED. Backfill saved. ===';
END
ELSE
BEGIN
    ROLLBACK TRAN;
    PRINT '=== ROLLED BACK (safe mode). Re-run with @DryRun=0 and @AutoCommit=1 to save. ===';
END

DROP TABLE IF EXISTS #MissingStudentPay;
DROP TABLE IF EXISTS #MissingExtra;
GO
