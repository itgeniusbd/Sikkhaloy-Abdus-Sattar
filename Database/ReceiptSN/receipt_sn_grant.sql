/* =====================================================================
   receipt_sn_grant.sql  - EXECUTE permission for the receipt-number procedure
   Fixes   : Hybrid fee collection (ফি আদায়) error
             "The EXECUTE permission was denied on the object 'usp_NextMoneyReceiptSN',
              database 'Edu', schema 'dbo'."
   Target  : live SQL Server, database Edu (the database the Hybrid SyncApi connects to).
   Nature  : IDEMPOTENT. Only GRANT statements; changes no data, no procedure, no index.
             @Execute = 0 (default) = PREVIEW ONLY (read-only).
             Set @Execute = 1 and run again to grant.

   Why only EXECUTE on dbo.usp_NextMoneyReceiptSN:
     - The Hybrid SyncApi (IIS app pool "HybridApi", Windows auth) calls
       dbo.usp_NextMoneyReceiptSN directly. Its database user was given per-object
       grants only (see Database_Scripts\Grant_*_Hybrid.sql), so the new procedure
       from receipt_sn_setup.sql is denied.
     - The procedure is WITH EXECUTE AS OWNER: inside it, the per-school sequences
       (dbo.Seq_MoneyReceipt_<SchoolID>), CREATE/ALTER SEQUENCE, sp_getapplock and
       the read of dbo.Income_MoneyReceipt run as the owner (dbo). So no SELECT/UPDATE
       on sequences and no CREATE SEQUENCE / ALTER permission is needed for the app.
     - The INSERT into dbo.Income_MoneyReceipt is done by the app itself and already
       worked before (the old MAX+1 path), so no table grant is needed.
     - SIKKHALOY V2 never calls the procedure directly: it calls dbo.MoneyReceipt, which
       calls dbo.usp_NextMoneyReceiptSN. Both are owned by dbo, so ownership chaining
       skips the permission check on the inner procedure. V2 keeps working without
       this grant; the script still grants it to the V2 app-pool users if they exist
       and are not db_owner (harmless, future-proof).
     - dbo.usp_SkipMoneyReceiptSN is an admin tool (run in SSMS). It is NOT granted.
   ===================================================================== */
USE [Edu];
GO
SET NOCOUNT ON;

DECLARE @Execute bit = 0;          -- 0 = preview only, 1 = grant

/* App database users that may need the grant. Users that do not exist in Edu are skipped.
   'IIS APPPOOL\HybridApi' = Hybrid SyncApi (required).  The other two = SIKKHALOY V2 / old API. */
DECLARE @Users TABLE (UserName sysname PRIMARY KEY, Purpose nvarchar(60));
INSERT INTO @Users (UserName, Purpose) VALUES
    (N'IIS APPPOOL\HybridApi',        N'Hybrid SyncApi (calls the procedure directly)'),
    (N'IIS APPPOOL\SIKKHALOY',        N'SIKKHALOY V2 web (via dbo.MoneyReceipt)'),
    (N'IIS APPPOOL\SIKKHALOY API',    N'SIKKHALOY API');
/* any other Hybrid app-pool user (e.g. renamed pool) */
INSERT INTO @Users (UserName, Purpose)
SELECT dp.name, N'auto-detected Hybrid app-pool user'
FROM sys.database_principals dp
WHERE dp.type IN ('U', 'S', 'G') AND dp.name LIKE N'IIS APPPOOL\%Hybrid%'
  AND NOT EXISTS (SELECT 1 FROM @Users u WHERE u.UserName = dp.name);

/* ---------------- 1) objects ---------------- */
PRINT N'== 1) receipt-SN objects in ' + DB_NAME() + N' ==';
SELECT
    CASE WHEN OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NOT NULL THEN 'OK' ELSE 'MISSING (run receipt_sn_setup.sql first)' END AS usp_NextMoneyReceiptSN,
    CASE WHEN OBJECT_ID(N'dbo.usp_SkipMoneyReceiptSN', N'P') IS NOT NULL THEN 'OK' ELSE 'MISSING' END AS usp_SkipMoneyReceiptSN,
    CASE WHEN OBJECT_DEFINITION(OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN')) LIKE N'%EXECUTE AS OWNER%' THEN 'EXECUTE AS OWNER' ELSE 'caller' END AS ProcRunsAs,
    (SELECT COUNT(*) FROM sys.sequences WHERE schema_id = SCHEMA_ID(N'dbo') AND name LIKE N'Seq[_]MoneyReceipt[_]%') AS SchoolSequences,
    (SELECT SUSER_SNAME(owner_sid) FROM sys.databases WHERE database_id = DB_ID()) AS DatabaseOwnerLogin,
    CASE WHEN (SELECT SUSER_SNAME(owner_sid) FROM sys.databases WHERE database_id = DB_ID()) IS NULL
         THEN 'ORPHANED - EXECUTE AS OWNER will fail; fix with ALTER AUTHORIZATION ON DATABASE::Edu TO [sa]'
         ELSE 'OK' END AS DatabaseOwnerState;

/* ---------------- 2) app users: roles + current permission ---------------- */
PRINT N'== 2) app users: roles and current EXECUTE permission (BEFORE) ==';
DECLARE @Perm TABLE (UserName sysname, Purpose nvarchar(60), ExistsInDb bit, Roles nvarchar(400),
                     ExplicitGrant nvarchar(60), CanExecNext nvarchar(30), CanExecMoneyReceipt nvarchar(30));
DECLARE @u sysname, @p nvarchar(60), @canNext int, @canMr int, @imp bit;
DECLARE uc CURSOR LOCAL FAST_FORWARD FOR SELECT UserName, Purpose FROM @Users;
OPEN uc; FETCH NEXT FROM uc INTO @u, @p;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @canNext = NULL, @canMr = NULL, @imp = 0;
    IF DATABASE_PRINCIPAL_ID(@u) IS NOT NULL
    BEGIN
        BEGIN TRY
            EXECUTE AS USER = @u;
            SET @imp = 1;
            SET @canNext = HAS_PERMS_BY_NAME(N'dbo.usp_NextMoneyReceiptSN', N'OBJECT', N'EXECUTE');
            SET @canMr   = HAS_PERMS_BY_NAME(N'dbo.MoneyReceipt', N'OBJECT', N'EXECUTE');
            REVERT;
            SET @imp = 0;
        END TRY
        BEGIN CATCH
            IF @imp = 1 REVERT;   -- could not impersonate (e.g. Windows account not resolvable): shown as '?'
            SET @imp = 0;
        END CATCH
    END
    INSERT INTO @Perm
    SELECT @u, @p,
           CASE WHEN DATABASE_PRINCIPAL_ID(@u) IS NULL THEN 0 ELSE 1 END,
           ISNULL(STUFF((SELECT N', ' + r.name FROM sys.database_role_members m
                         JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
                         WHERE m.member_principal_id = DATABASE_PRINCIPAL_ID(@u) ORDER BY r.name
                         FOR XML PATH('')), 1, 2, N''), N''),
           ISNULL((SELECT TOP (1) pe.state_desc FROM sys.database_permissions pe
                   WHERE pe.grantee_principal_id = DATABASE_PRINCIPAL_ID(@u) AND pe.class = 1
                     AND pe.major_id = OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN') AND pe.permission_name = N'EXECUTE'), N'-'),
           CASE @canNext WHEN 1 THEN N'YES' WHEN 0 THEN N'NO  <-- needs grant' ELSE N'?' END,
           CASE @canMr   WHEN 1 THEN N'YES' WHEN 0 THEN N'NO' ELSE N'?' END;
    FETCH NEXT FROM uc INTO @u, @p;
END
CLOSE uc; DEALLOCATE uc;
SELECT UserName, Purpose, CASE ExistsInDb WHEN 1 THEN 'yes' ELSE 'no (skipped)' END AS UserInEdu,
       Roles, ExplicitGrant AS ExplicitGrantOnProc, CanExecNext AS CanExec_usp_NextMoneyReceiptSN,
       CanExecMoneyReceipt AS CanExec_dbo_MoneyReceipt
FROM @Perm ORDER BY ExistsInDb DESC, UserName;

/* all IIS app-pool users in Edu (to spot a differently named Hybrid pool) */
SELECT dp.name AS IisAppPoolUser, dp.type_desc,
       ISNULL(STUFF((SELECT N', ' + r.name FROM sys.database_role_members m
                     JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
                     WHERE m.member_principal_id = dp.principal_id ORDER BY r.name
                     FOR XML PATH('')), 1, 2, N''), N'') AS Roles
FROM sys.database_principals dp
WHERE dp.name LIKE N'IIS APPPOOL\%' OR dp.name LIKE N'NT AUTHORITY\%' OR dp.name LIKE N'%$'
ORDER BY dp.name;

/* ---------------- 3) grant ---------------- */
IF OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN', N'P') IS NULL
BEGIN
    PRINT N'dbo.usp_NextMoneyReceiptSN does not exist: nothing to grant (Hybrid uses its MAX+1 fallback).';
    RETURN;
END

IF @Execute = 0
BEGIN
    PRINT N'PREVIEW ONLY. Users marked "NO  <-- needs grant" above will get:';
    SELECT N'GRANT EXECUTE ON OBJECT::dbo.usp_NextMoneyReceiptSN TO ' + QUOTENAME(UserName) + N';' AS WillRun
    FROM @Perm WHERE ExistsInDb = 1 AND CanExecNext <> N'YES';
    PRINT N'Set @Execute = 1 at the top and run the script again.';
    RETURN;
END

DECLARE @sql nvarchar(max), @n int = 0;
DECLARE gc CURSOR LOCAL FAST_FORWARD FOR
    SELECT UserName FROM @Perm WHERE ExistsInDb = 1 AND CanExecNext <> N'YES';
OPEN gc; FETCH NEXT FROM gc INTO @u;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'GRANT EXECUTE ON OBJECT::dbo.usp_NextMoneyReceiptSN TO ' + QUOTENAME(@u) + N';';
    EXEC sys.sp_executesql @sql;
    PRINT N'  granted: ' + @sql;
    SET @n += 1;
    FETCH NEXT FROM gc INTO @u;
END
CLOSE gc; DEALLOCATE gc;
PRINT N'GRANT statements run: ' + CONVERT(nvarchar(11), @n) + N' (0 = everything was already in place).';

/* ---------------- 4) verify ---------------- */
PRINT N'== 4) AFTER ==';
SELECT pu.UserName,
       CASE WHEN x.CanExec = 1 THEN 'YES' WHEN x.CanExec = 0 THEN 'NO' ELSE '?' END AS CanExec_usp_NextMoneyReceiptSN_After
FROM @Perm pu
CROSS APPLY (SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.database_permissions pe
                                      WHERE pe.grantee_principal_id = DATABASE_PRINCIPAL_ID(pu.UserName)
                                        AND pe.class = 1 AND pe.major_id = OBJECT_ID(N'dbo.usp_NextMoneyReceiptSN')
                                        AND pe.permission_name = N'EXECUTE' AND pe.state IN ('G', 'W'))
                             OR pu.CanExecNext = N'YES' THEN 1 ELSE 0 END AS CanExec) x
WHERE pu.ExistsInDb = 1
ORDER BY pu.UserName;
GO
