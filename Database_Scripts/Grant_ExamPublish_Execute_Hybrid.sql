-- Grant EXECUTE on Exam Result Publish stored procedures (Hybrid SyncApi)
-- Target user (from Edu principals): IIS APPPOOL\HybridApi
-- Old WebForms uses IIS APPPOOL\SIKKHALOY — this script does NOT change that login.

USE [Edu];
GO

DECLARE @HybridUser SYSNAME = N'IIS APPPOOL\HybridApi';

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @HybridUser)
BEGIN
    RAISERROR(N'User not found in Edu. Change @HybridUser to the Hybrid API login.', 16, 1);
    RETURN;
END

DECLARE @sql NVARCHAR(MAX) =
    N'GRANT EXECUTE ON OBJECT::dbo.SP_Exam_Subject TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13) +
    N'GRANT EXECUTE ON OBJECT::dbo.SP_Exam_Student TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13) +
    N'GRANT EXECUTE ON OBJECT::dbo.SP_Exam_Attendance TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13) +
    N'GRANT EXECUTE ON OBJECT::dbo.HighestMark_Position TO ' + QUOTENAME(@HybridUser) + N';' + CHAR(13) +
    N'GRANT EXECUTE ON OBJECT::dbo.SP_Cumulative_HighestMark_Position TO ' + QUOTENAME(@HybridUser) + N';';

EXEC sp_executesql @sql;
PRINT N'OK: EXECUTE granted for ' + @HybridUser;
GO