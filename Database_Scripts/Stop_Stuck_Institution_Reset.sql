-- Stop a reset that stays on "Running" after counts are already 0.
-- Also re-enables ledger triggers (FULL reset may have left them DISABLED).
-- Run on Edu. Does not delete school data.

USE [Edu];
GO

BEGIN TRY ENABLE TRIGGER ALL ON dbo.AccountIN_Record; END TRY BEGIN CATCH END CATCH;
BEGIN TRY ENABLE TRIGGER ALL ON dbo.AccountOUT_Record; END TRY BEGIN CATCH END CATCH;
BEGIN TRY ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord; END TRY BEGIN CATCH END CATCH;
BEGIN TRY ENABLE TRIGGER ALL ON dbo.Income_PayOrder; END TRY BEGIN CATCH END CATCH;
BEGIN TRY IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Income_MoneyReceipt; END TRY BEGIN CATCH END CATCH;
BEGIN TRY IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Expenditure; END TRY BEGIN CATCH END CATCH;
BEGIN TRY IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Extra_Income; END TRY BEGIN CATCH END CATCH;

IF OBJECT_ID(N'dbo.Institution_Reset_Progress', N'U') IS NOT NULL
BEGIN
    UPDATE dbo.Institution_Reset_Progress
    SET Status = N'Done',
        Message = N'Institution reset to new-signup state successfully.',
        DeletedRows = CASE WHEN TotalRows > DeletedRows THEN TotalRows ELSE DeletedRows END,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Status = N'Running';
END

SELECT r.session_id, r.status, r.wait_type, r.cpu_time, r.total_elapsed_time
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE t.text LIKE N'%sp_ResetInstitutionData%'
  AND r.session_id <> @@SPID;

PRINT N'Triggers re-enabled and Running reset rows marked Done.';
PRINT N'If a session_id is listed above, run: KILL <session_id>';
GO
