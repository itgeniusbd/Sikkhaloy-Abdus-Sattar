/* =====================================================================
   receipt_sn_setup.sql   — per-school, never-reused money receipt numbers
   Target : SQL Server 2016 SP1+ (live: SQL 2019), database Edu.
   Nature : IDEMPOTENT and ADDITIVE. Safe to run more than once.
            - changes NO existing rows (receipt numbers stay exactly as they are)
            - creates dbo.usp_NextMoneyReceiptSN, dbo.usp_SkipMoneyReceiptSN
            - creates one SEQUENCE per school: dbo.Seq_MoneyReceipt_<SchoolID>
              starting at that school's MAX(MoneyReceipt_SN)+1 (min 100001)
            - creates UNIQUE index UX_Income_MoneyReceipt_School_SN; if old duplicates
              exist it covers only rows above the largest duplicate MoneyReceiptID
              (historical numbers stay), plus lookup index IX_Income_MoneyReceipt_School_SN
            - alters dbo.MoneyReceipt (same parameters/result) to use the new procedure
            - grants EXECUTE on dbo.usp_NextMoneyReceiptSN to the app users (Step 6:
              IIS APPPOOL\HybridApi etc.; without it Hybrid fee collection is denied)
   Run    : in SSMS on the live DB, preferably at a quiet time (index build).
            Run check_duplicates_readonly.sql first; set @ReviewedDuplicateCutoff
            in Step 3 if its duplicate list changed.
   ===================================================================== */
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT OFF;
GO
IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NULL
BEGIN
    RAISERROR(N'dbo.Income_MoneyReceipt not found. Wrong database? Nothing was changed.', 16, 1) WITH NOWAIT;
    SET NOEXEC ON;
END
GO
PRINT N'Step 1/6: dbo.usp_NextMoneyReceiptSN';
GO
/* ---------------------------------------------------------------------
   dbo.usp_NextMoneyReceiptSN  @SchoolID, @SN OUTPUT
   Returns the next receipt number of one school from its own SEQUENCE
   dbo.Seq_MoneyReceipt_<SchoolID> (CACHE 50).
   * First call for a school creates the sequence at MAX(MoneyReceipt_SN)+1
     (minimum 100001, same as the old F_MoneyReceipt_SN).
   * Works inside the caller's transaction (opens its own only if none).
   * Self-heal: if the drawn number already exists in Income_MoneyReceipt
     (old MAX+1 code / manual insert) or the sequence is >= 1000 behind MAX,
     the sequence is restarted past MAX and past every value already issued.
   * A number drawn inside a transaction that later rolls back is consumed
     (sequences are not transactional): that leaves a gap, never a duplicate.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_NextMoneyReceiptSN
    @SchoolID int,
    @SN       int OUTPUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    IF @SchoolID IS NULL OR @SchoolID <= 0
        THROW 51000, N'usp_NextMoneyReceiptSN: @SchoolID must be greater than 0.', 1;

    DECLARE @SeqName  sysname       = N'Seq_MoneyReceipt_' + CONVERT(nvarchar(11), @SchoolID);
    DECLARE @Seq      nvarchar(300) = N'dbo.' + QUOTENAME(@SeqName);
    DECLARE @LockRes  nvarchar(255) = N'MoneyReceiptSN:' + CONVERT(nvarchar(11), @SchoolID);
    DECLARE @NextSql  nvarchar(400) = N'SET @v = NEXT VALUE FOR ' + @Seq + N';';
    DECLARE @OwnTran  bit = 0, @rc int, @Max int, @Used bigint, @Start bigint, @sql nvarchar(max);

    IF @@TRANCOUNT = 0
    BEGIN
        BEGIN TRANSACTION;
        SET @OwnTran = 1;
    END;

    BEGIN TRY
        /* 1) lazy, race-safe creation (normally done once by the setup script) */
        IF OBJECT_ID(@Seq, N'SO') IS NULL
        BEGIN
            EXEC @rc = sys.sp_getapplock @Resource = @LockRes, @LockMode = N'Exclusive',
                                         @LockOwner = N'Transaction', @LockTimeout = 15000;
            IF @rc < 0
                THROW 51001, N'usp_NextMoneyReceiptSN: timeout while creating the receipt counter. Please try again.', 1;

            IF OBJECT_ID(@Seq, N'SO') IS NULL
            BEGIN
                SELECT @Max = MAX(MoneyReceipt_SN)
                FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK)
                WHERE SchoolID = @SchoolID;

                SET @Start = CASE WHEN ISNULL(@Max, 0) < 100000 THEN 100001 ELSE @Max + 1 END;
                SET @sql = N'CREATE SEQUENCE ' + @Seq + N' AS int START WITH ' + CONVERT(nvarchar(20), @Start)
                         + N' INCREMENT BY 1 MINVALUE 1 NO MAXVALUE NO CYCLE CACHE 50;';
                EXEC (@sql);
            END;
        END;

        /* 2) draw */
        EXEC sys.sp_executesql @NextSql, N'@v int OUTPUT', @v = @SN OUTPUT;

        /* 3) self-heal check (index seek on SchoolID, MoneyReceipt_SN; NOLOCK = never blocks,
              also sees an uncommitted row of old code that took the same number) */
        SELECT @Max = MAX(MoneyReceipt_SN)
        FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
        WHERE SchoolID = @SchoolID;

        IF EXISTS (SELECT 1 FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
                   WHERE SchoolID = @SchoolID AND MoneyReceipt_SN = @SN)
           OR ISNULL(@Max, 0) - @SN >= 1000
        BEGIN
            EXEC @rc = sys.sp_getapplock @Resource = @LockRes, @LockMode = N'Exclusive',
                                         @LockOwner = N'Transaction', @LockTimeout = 15000;
            IF @rc < 0
                THROW 51002, N'usp_NextMoneyReceiptSN: timeout while repairing the receipt counter. Please try again.', 1;

            /* freeze the sequence (schema lock until the transaction ends) so no other
               session can draw between reading its state and restarting it */
            SET @sql = N'ALTER SEQUENCE ' + @Seq + N' CACHE 50;';
            EXEC (@sql);

            SELECT @Max = MAX(MoneyReceipt_SN)
            FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK)
            WHERE SchoolID = @SchoolID;

            SELECT @Used = CONVERT(bigint, ISNULL(last_used_value, current_value))
            FROM sys.sequences WHERE object_id = OBJECT_ID(@Seq);

            SET @Start = CASE WHEN ISNULL(@Max, 100000) > ISNULL(@Used, 100000)
                              THEN ISNULL(@Max, 100000) ELSE @Used END + 1;
            IF @Start < 100001 SET @Start = 100001;

            SET @sql = N'ALTER SEQUENCE ' + @Seq + N' RESTART WITH ' + CONVERT(nvarchar(20), @Start) + N';';
            EXEC (@sql);
            EXEC sys.sp_executesql @NextSql, N'@v int OUTPUT', @v = @SN OUTPUT;
        END;

        IF @OwnTran = 1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @OwnTran = 1 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
PRINT N'Step 2/6: dbo.usp_SkipMoneyReceiptSN';
GO
/* ---------------------------------------------------------------------
   dbo.usp_SkipMoneyReceiptSN  @SchoolID = NULL (all schools), @Skip = 50
   Manual tool for AFTER an incident in which the database itself went back
   in time (restore from backup, VM snapshot revert, lost writes): the
   sequences went back too, so push every counter @Skip past both the
   current MAX and the last issued value. Never changes existing rows.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_SkipMoneyReceiptSN
    @SchoolID int = NULL,
    @Skip     int = 50
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    IF @Skip IS NULL OR @Skip < 0 OR @Skip > 100000
        THROW 51010, N'usp_SkipMoneyReceiptSN: @Skip must be between 0 and 100000.', 1;

    DECLARE @Out TABLE (SchoolID int, OldLastIssued bigint, MaxSN int, NewNext bigint);
    DECLARE @rc int, @sid int, @res nvarchar(255), @Seq nvarchar(300), @Max int, @Used bigint, @Start bigint, @sql nvarchar(max);

    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
        SELECT TRY_CONVERT(int, SUBSTRING(s.name, 18, 20))
        FROM sys.sequences s
        WHERE s.schema_id = SCHEMA_ID(N'dbo') AND s.name LIKE N'Seq[_]MoneyReceipt[_]%'
          AND TRY_CONVERT(int, SUBSTRING(s.name, 18, 20)) IS NOT NULL
          AND (@SchoolID IS NULL OR s.name = N'Seq_MoneyReceipt_' + CONVERT(nvarchar(11), @SchoolID));
    OPEN c;
    FETCH NEXT FROM c INTO @sid;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Seq = N'dbo.' + QUOTENAME(N'Seq_MoneyReceipt_' + CONVERT(nvarchar(11), @sid));
        BEGIN TRANSACTION;
            SET @res = N'MoneyReceiptSN:' + CONVERT(nvarchar(11), @sid);
            EXEC @rc = sys.sp_getapplock @Resource = @res, @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 30000;
            IF @rc < 0 BEGIN ROLLBACK TRANSACTION; CLOSE c; DEALLOCATE c; THROW 51011, N'usp_SkipMoneyReceiptSN: lock timeout, try again when no collection is running.', 1; END;
            SET @sql = N'ALTER SEQUENCE ' + @Seq + N' CACHE 50;';
            EXEC (@sql);
            SELECT @Max = MAX(MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK) WHERE SchoolID = @sid;
            SELECT @Used = CONVERT(bigint, ISNULL(last_used_value, CONVERT(bigint, current_value) - 1))
            FROM sys.sequences WHERE object_id = OBJECT_ID(@Seq);
            SET @Start = CASE WHEN ISNULL(@Max, 100000) > ISNULL(@Used, 100000) THEN ISNULL(@Max, 100000) ELSE @Used END + @Skip + 1;
            SET @sql = N'ALTER SEQUENCE ' + @Seq + N' RESTART WITH ' + CONVERT(nvarchar(20), @Start) + N';';
            EXEC (@sql);
        COMMIT TRANSACTION;
        INSERT @Out VALUES (@sid, @Used, @Max, @Start);
        FETCH NEXT FROM c INTO @sid;
    END;
    CLOSE c; DEALLOCATE c;
    SELECT * FROM @Out ORDER BY SchoolID;
END;
GO
PRINT N'Step 3/6: unique index (SchoolID, MoneyReceipt_SN) + lookup index';
GO
/* ---------------------------------------------------------------------
   Historical duplicates are NOT renumbered (parents hold the paper receipts).
   * No duplicates      -> plain unique index (new databases, EduHybrid schema).
   * Only old duplicates -> filtered unique index that covers every row with
     MoneyReceiptID > <cutoff>, cutoff = largest MoneyReceiptID of any duplicate
     (a literal, written into the filter). New receipts are always above it.
   * A duplicate with MoneyReceiptID > @ReviewedDuplicateCutoff is NEW (not yet
     reviewed): the index is refused and the rows are listed.
   * Index already there (with or without cutoff): left unchanged.
   Live Edu, check_duplicates_readonly.sql of 28 Sep 2026: 6 pairs (schools 0,
   1144 x3, 1162, 1185), largest MoneyReceiptID involved = 1166495.
   Set @ReviewedDuplicateCutoff = NULL to accept whatever duplicates exist now.
   --------------------------------------------------------------------- */
DECLARE @ReviewedDuplicateCutoff int = 1166495;

DECLARE @filter nvarchar(max), @Cutoff int, @pairs int, @ix nvarchar(max),
        @online nvarchar(40) = CASE WHEN CONVERT(int, SERVERPROPERTY('EngineEdition')) IN (3, 5, 8) THEN N' WITH (ONLINE = ON)' ELSE N'' END;

SELECT @filter = filter_definition FROM sys.indexes
WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND name = N'UX_Income_MoneyReceipt_School_SN';

IF @@ROWCOUNT > 0
    PRINT N'  UX_Income_MoneyReceipt_School_SN already exists (' + ISNULL(@filter, N'no filter') + N'): left unchanged.';
ELSE
BEGIN
    SELECT SchoolID, MoneyReceipt_SN, COUNT(*) AS Copies, MAX(MoneyReceiptID) AS LastID
    INTO #dup
    FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
    WHERE SchoolID IS NOT NULL AND MoneyReceipt_SN IS NOT NULL
    GROUP BY SchoolID, MoneyReceipt_SN
    HAVING COUNT(*) > 1;
    SET @pairs = @@ROWCOUNT;
    SELECT @Cutoff = MAX(LastID) FROM #dup;

    IF @pairs > 0 AND @Cutoff > ISNULL(@ReviewedDuplicateCutoff, @Cutoff)
    BEGIN
        PRINT N'  *** NEW duplicate receipt numbers (MoneyReceiptID above the reviewed cutoff '
              + CONVERT(nvarchar(11), @ReviewedDuplicateCutoff) + N'): unique index REFUSED. ***';
        PRINT N'  *** Everything else is installed. Review the rows listed below, then set          ***';
        PRINT N'  *** @ReviewedDuplicateCutoff in Step 3 (or fix the rows) and run the script again. ***';
        SELECT mr.SchoolID, mr.MoneyReceipt_SN, mr.MoneyReceiptID, mr.PaidDate, mr.CollectionDate, mr.TotalAmount, mr.StudentID
        FROM dbo.Income_MoneyReceipt mr WITH (NOLOCK)
        JOIN #dup d ON d.SchoolID = mr.SchoolID AND d.MoneyReceipt_SN = mr.MoneyReceipt_SN
        WHERE d.LastID > @ReviewedDuplicateCutoff
        ORDER BY mr.SchoolID, mr.MoneyReceipt_SN, mr.MoneyReceiptID;
    END
    ELSE
    BEGIN
        SET @ix = N'CREATE UNIQUE NONCLUSTERED INDEX UX_Income_MoneyReceipt_School_SN'
                + N' ON dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN)'
                + N' WHERE MoneyReceipt_SN IS NOT NULL AND SchoolID IS NOT NULL'
                + CASE WHEN @pairs > 0 THEN N' AND MoneyReceiptID > ' + CONVERT(nvarchar(11), @Cutoff) ELSE N'' END
                + @online + N';';
        BEGIN TRY
            EXEC (@ix);
            IF @pairs = 0
                PRINT N'  UX_Income_MoneyReceipt_School_SN created: the database now rejects duplicate receipt numbers.';
            ELSE
                PRINT N'  UX_Income_MoneyReceipt_School_SN created for MoneyReceiptID > ' + CONVERT(nvarchar(11), @Cutoff)
                      + N': new duplicates are rejected; ' + CONVERT(nvarchar(11), @pairs)
                      + N' historical duplicate pair(s) keep their numbers.';
        END TRY
        BEGIN CATCH
            PRINT N'  unique index NOT created (' + ERROR_MESSAGE() + N'). Everything else is installed; run check_duplicates_readonly.sql and re-run this script.';
        END CATCH
    END
    DROP TABLE #dup;
END

/* lookup index for usp_NextMoneyReceiptSN (MAX / EXISTS per school): a unique index
   filtered on MoneyReceiptID cannot serve those queries, so it is needed unless an
   index on (SchoolID, MoneyReceipt_SN) without a MoneyReceiptID filter exists */
IF NOT EXISTS (SELECT 1 FROM sys.indexes i
               JOIN sys.index_columns k1 ON k1.object_id = i.object_id AND k1.index_id = i.index_id AND k1.key_ordinal = 1
               JOIN sys.index_columns k2 ON k2.object_id = i.object_id AND k2.index_id = i.index_id AND k2.key_ordinal = 2
               WHERE i.object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt')
                 AND k1.column_id = COLUMNPROPERTY(i.object_id, N'SchoolID', 'ColumnId')
                 AND k2.column_id = COLUMNPROPERTY(i.object_id, N'MoneyReceipt_SN', 'ColumnId')
                 AND ISNULL(i.filter_definition, N'') NOT LIKE N'%MoneyReceiptID%')
BEGIN
    BEGIN TRY
        SET @ix = N'CREATE NONCLUSTERED INDEX IX_Income_MoneyReceipt_School_SN ON dbo.Income_MoneyReceipt (SchoolID, MoneyReceipt_SN DESC)' + @online + N';';
        EXEC (@ix);
        PRINT N'  lookup index IX_Income_MoneyReceipt_School_SN created (non-unique).';
    END TRY
    BEGIN CATCH
        PRINT N'  lookup index not created: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT N'Step 4/6: one sequence per school (start = MAX+1, existing sequences are left alone)';
GO
DECLARE @sid int, @Seq nvarchar(300), @Max int, @Start bigint, @sql nvarchar(max), @res nvarchar(255), @rc int,
        @created int = 0, @existing int = 0;
DECLARE sc CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT SchoolID FROM dbo.Income_MoneyReceipt WITH (NOLOCK)
    WHERE SchoolID IS NOT NULL AND SchoolID > 0;   -- SchoolID 0 = Authority logins, not a school: no sequence
OPEN sc;
FETCH NEXT FROM sc INTO @sid;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Seq = N'dbo.' + QUOTENAME(N'Seq_MoneyReceipt_' + CONVERT(nvarchar(11), @sid));
    IF OBJECT_ID(@Seq, N'SO') IS NOT NULL
        SET @existing += 1;
    ELSE
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;
                SET @res = N'MoneyReceiptSN:' + CONVERT(nvarchar(11), @sid);
                EXEC @rc = sys.sp_getapplock @Resource = @res, @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 30000;
                IF @rc >= 0 AND OBJECT_ID(@Seq, N'SO') IS NULL
                BEGIN
                    SELECT @Max = MAX(MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK) WHERE SchoolID = @sid;
                    SET @Start = CASE WHEN ISNULL(@Max, 0) < 100000 THEN 100001 ELSE @Max + 1 END;
                    SET @sql = N'CREATE SEQUENCE ' + @Seq + N' AS int START WITH ' + CONVERT(nvarchar(20), @Start)
                             + N' INCREMENT BY 1 MINVALUE 1 NO MAXVALUE NO CYCLE CACHE 50;';
                    EXEC (@sql);
                    SET @created += 1;
                END
            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            PRINT N'  school ' + CONVERT(nvarchar(11), @sid) + N': sequence not created now (' + ERROR_MESSAGE() + N'); it will be created on first use.';
        END CATCH
    END
    FETCH NEXT FROM sc INTO @sid;
END
CLOSE sc; DEALLOCATE sc;
PRINT N'  sequences created: ' + CONVERT(nvarchar(11), @created) + N', already existing: ' + CONVERT(nvarchar(11), @existing)
    + N'. Schools without receipts get theirs on the first collection.';
GO
PRINT N'Step 5/6: dbo.MoneyReceipt -> usp_NextMoneyReceiptSN (fallback F_MoneyReceipt_SN)';
GO
/* ---------------------------------------------------------------------
   dbo.MoneyReceipt  (used by every SIKKHALOY V2 collection path)
   Same parameters and same result (SELECT SCOPE_IDENTITY()) as before;
   only the receipt number source changed. Falls back to the old
   F_MoneyReceipt_SN if usp_NextMoneyReceiptSN is not present.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[MoneyReceipt]
    @StudentID        INT,
    @RegistrationID   INT,
    @StudentClassID   INT,
    @EducationYearID  INT,
    @PaymentBy        NVARCHAR(128),
    @PaidDate         DATETIME,
    @SchoolID         INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MoneyReceipt_SN INT
    IF OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NOT NULL
        EXEC dbo.usp_NextMoneyReceiptSN @SchoolID = @SchoolID, @SN = @MoneyReceipt_SN OUTPUT
    ELSE
        SET @MoneyReceipt_SN = [dbo].[F_MoneyReceipt_SN](@SchoolID)

    INSERT INTO Income_MoneyReceipt
        (StudentID, RegistrationID, StudentClassID, PaidDate, EducationYearID, PaymentBy, SchoolID, MoneyReceipt_SN, CollectionDate)
    VALUES
        (@StudentID, @RegistrationID, @StudentClassID, @PaidDate, @EducationYearID, @PaymentBy, @SchoolID, @MoneyReceipt_SN, GETDATE())

    SELECT SCOPE_IDENTITY()
END
GO
PRINT N'Step 6/6: EXECUTE on dbo.usp_NextMoneyReceiptSN for the app database users';
GO
/* ---------------------------------------------------------------------
   The Hybrid SyncApi calls dbo.usp_NextMoneyReceiptSN directly, and its database user
   (IIS APPPOOL\HybridApi) only has per-object grants, so without this step every Hybrid
   fee collection fails with "The EXECUTE permission was denied on the object
   'usp_NextMoneyReceiptSN'". Only EXECUTE is needed: the procedure is WITH EXECUTE AS OWNER,
   so its sequences / CREATE SEQUENCE / table reads run as dbo.
   SIKKHALOY V2 reaches the procedure through dbo.MoneyReceipt (ownership chaining, no check);
   its pool users are granted too if present (harmless). Missing users are skipped.
   Re-runnable; the same grant on its own: receipt_sn_grant.sql (with a permission preview).
   --------------------------------------------------------------------- */
DECLARE @GrantUsers TABLE (UserName sysname PRIMARY KEY);
INSERT INTO @GrantUsers (UserName) VALUES
    (N'IIS APPPOOL\HybridApi'), (N'IIS APPPOOL\SIKKHALOY'), (N'IIS APPPOOL\SIKKHALOY API');
INSERT INTO @GrantUsers (UserName)
SELECT dp.name FROM sys.database_principals dp
WHERE dp.type IN ('U', 'S', 'G') AND dp.name LIKE N'IIS APPPOOL\%Hybrid%'
  AND NOT EXISTS (SELECT 1 FROM @GrantUsers g WHERE g.UserName = dp.name);

DECLARE @gu sysname, @gsql nvarchar(max), @granted int = 0, @skipped int = 0;
DECLARE gcur CURSOR LOCAL FAST_FORWARD FOR SELECT UserName FROM @GrantUsers;
OPEN gcur;
FETCH NEXT FROM gcur INTO @gu;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF DATABASE_PRINCIPAL_ID(@gu) IS NULL
        SET @skipped += 1;
    ELSE IF OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NOT NULL
    BEGIN
        BEGIN TRY
            SET @gsql = N'GRANT EXECUTE ON OBJECT::dbo.usp_NextMoneyReceiptSN TO ' + QUOTENAME(@gu) + N';';
            EXEC sys.sp_executesql @gsql;
            SET @granted += 1;
            PRINT N'  EXECUTE granted to ' + @gu;
        END TRY
        BEGIN CATCH
            PRINT N'  grant to ' + @gu + N' failed: ' + ERROR_MESSAGE();
        END CATCH
    END
    FETCH NEXT FROM gcur INTO @gu;
END
CLOSE gcur; DEALLOCATE gcur;
PRINT N'  app users granted: ' + CONVERT(nvarchar(11), @granted) + N', not in this database (skipped): ' + CONVERT(nvarchar(11), @skipped)
    + N'. If the Hybrid API runs under another login, run receipt_sn_grant.sql with that user.';
GO
/* ---------------- verification (read-only) ---------------- */
SELECT
    CASE WHEN OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NOT NULL THEN 'OK' ELSE 'MISSING' END AS usp_NextMoneyReceiptSN,
    CASE WHEN OBJECT_ID(N'dbo.usp_SkipMoneyReceiptSN', N'P') IS NOT NULL THEN 'OK' ELSE 'MISSING' END AS usp_SkipMoneyReceiptSN,
    CASE WHEN OBJECT_DEFINITION(OBJECT_ID(N'dbo.MoneyReceipt')) LIKE N'%usp_NextMoneyReceiptSN%' THEN 'OK' ELSE 'OLD' END AS MoneyReceipt_uses_usp,
    ISNULL((SELECT CASE WHEN filter_definition LIKE N'%MoneyReceiptID%' THEN N'OK ' + filter_definition ELSE N'OK' END
            FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt')
              AND name = N'UX_Income_MoneyReceipt_School_SN'), N'NOT CREATED') AS UniqueIndex,
    (SELECT COUNT(DISTINCT SchoolID) FROM dbo.Income_MoneyReceipt WITH (NOLOCK) WHERE SchoolID > 0) AS SchoolsWithReceipts,
    (SELECT COUNT(*) FROM sys.sequences WHERE schema_id = SCHEMA_ID(N'dbo') AND name LIKE N'Seq[_]MoneyReceipt[_]%') AS Sequences,
    ISNULL(STUFF((SELECT N', ' + dp.name FROM sys.database_permissions pe
                  JOIN sys.database_principals dp ON dp.principal_id = pe.grantee_principal_id
                  WHERE pe.class = 1 AND pe.major_id = OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN')
                    AND pe.permission_name = N'EXECUTE' AND pe.state IN ('G', 'W')
                  FOR XML PATH('')), 1, 2, N''), N'NONE (Hybrid fee collection needs IIS APPPOOL\HybridApi)') AS ExecuteGrantedTo;

/* per school: next number that will be issued vs current MAX (NextToIssue must be > MaxSN;
   'will self-heal' only appears if old code inserted after the sequence was created) */
SELECT TOP (50) x.SchoolID, x.MaxSN, x.NextToIssue,
       CASE WHEN x.NextToIssue > ISNULL(x.MaxSN, 0) THEN 'OK' ELSE 'will self-heal on next use' END AS State
FROM (
    SELECT TRY_CONVERT(int, SUBSTRING(s.name, 18, 20)) AS SchoolID,
           CASE WHEN s.last_used_value IS NULL THEN CONVERT(bigint, s.current_value)
                ELSE CONVERT(bigint, s.last_used_value) + 1 END AS NextToIssue,
           (SELECT MAX(m.MoneyReceipt_SN) FROM dbo.Income_MoneyReceipt m WITH (NOLOCK)
            WHERE m.SchoolID = TRY_CONVERT(int, SUBSTRING(s.name, 18, 20))) AS MaxSN
    FROM sys.sequences s
    WHERE s.schema_id = SCHEMA_ID(N'dbo') AND s.name LIKE N'Seq[_]MoneyReceipt[_]%'
) x
ORDER BY CASE WHEN x.NextToIssue > ISNULL(x.MaxSN, 0) THEN 1 ELSE 0 END, x.SchoolID;
GO
SET NOEXEC OFF;
GO
