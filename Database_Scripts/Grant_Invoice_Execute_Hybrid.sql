-- Grant EXECUTE on invoice scalar functions used by Hybrid SyncApi (IIS APPPOOL\HybridApi)
-- Error: EXECUTE permission was denied on the object 'Invoice_SerialNumber'
-- Also covers related invoice helpers Hybrid calls next.

USE [Edu];
GO

DECLARE @HybridUser SYSNAME = N'IIS APPPOOL\HybridApi';

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @HybridUser)
BEGIN
    RAISERROR(N'User not found in Edu.', 16, 1);
    RETURN;
END

DECLARE @sql NVARCHAR(MAX) = N'';

IF OBJECT_ID(N'dbo.Invoice_SerialNumber', N'FN') IS NOT NULL
    OR OBJECT_ID(N'dbo.Invoice_SerialNumber', N'FS') IS NOT NULL
    OR OBJECT_ID(N'dbo.Invoice_SerialNumber') IS NOT NULL
    SET @sql += N'GRANT EXECUTE ON OBJECT::dbo.Invoice_SerialNumber TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13);

IF OBJECT_ID(N'dbo.F_InvoiceReceipt_SN') IS NOT NULL
    SET @sql += N'GRANT EXECUTE ON OBJECT::dbo.F_InvoiceReceipt_SN TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13);

IF OBJECT_ID(N'dbo.fn_GetBillableCommitteeCount') IS NOT NULL
    SET @sql += N'GRANT EXECUTE ON OBJECT::dbo.fn_GetBillableCommitteeCount TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13);

IF LEN(@sql) = 0
BEGIN
    RAISERROR(N'No invoice functions found to grant.', 16, 1);
    RETURN;
END

EXEC sp_executesql @sql;
PRINT N'OK: invoice EXECUTE granted for ' + @HybridUser;
GO