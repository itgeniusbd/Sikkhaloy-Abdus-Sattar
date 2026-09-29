-- Live Edu: Hybrid reset fails with
--   EXECUTE permission was denied on the object 'sp_InstitutionData_Preview'
-- because DROP/CREATE of the proc drops object-level grants.
-- Run this on the live Edu database (SSMS), then retry preview/reset.
-- Does not change data.

USE [Edu];
GO

IF OBJECT_ID(N'dbo.sp_InstitutionData_Preview', N'P') IS NULL
    PRINT N'Missing dbo.sp_InstitutionData_Preview — deploy Database/StoredProcedures/sp_InstitutionData_Preview.sql first.';
ELSE
    GRANT EXECUTE ON OBJECT::dbo.sp_InstitutionData_Preview TO PUBLIC;

IF OBJECT_ID(N'dbo.sp_ResetInstitutionData', N'P') IS NULL
    PRINT N'Missing dbo.sp_ResetInstitutionData — deploy Database/StoredProcedures/sp_ResetInstitutionData.sql first.';
ELSE
    GRANT EXECUTE ON OBJECT::dbo.sp_ResetInstitutionData TO PUBLIC;

IF OBJECT_ID(N'dbo.Institution_Reset_Progress', N'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.Institution_Reset_Progress TO PUBLIC;
END

PRINT N'Granted EXECUTE on reset procedures to PUBLIC.';
GO
