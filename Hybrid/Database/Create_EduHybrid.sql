-- Empty EduHybrid on a new SQL Server (schema only: tables, views, SPs, triggers).
-- No school/student/invoice data. Do not run against live Edu.
--
-- In SSMS (SQLCMD Mode is NOT required):
--   1. Connect to the server.
--   2. Open Hybrid\Database\EduHybrid_Schema_Only.sql
--   3. Execute (F5).
-- If EduHybrid already exists the script stops. Delete that empty/failed
-- database first only if you want a fresh schema rebuild.
--
-- After create, the database is empty: add an Authority/Admin membership
-- user before Hybrid login will work. SQL Agent jobs (monthly invoice, etc.)
-- are server-level and are not in this script.
--
-- Optional: publish the dacpac instead of the .sql
--   SqlPackage /Action:Publish /SourceFile:EduHybrid.dacpac
--     /TargetServerName:YOUR_SERVER /TargetDatabaseName:EduHybrid
--     /TargetTrustServerCertificate:True /p:CreateNewDatabase=True

IF DB_ID(N'EduHybrid') IS NULL
BEGIN
    CREATE DATABASE EduHybrid;
    PRINT N'Created empty database EduHybrid. Now run EduHybrid_Schema_Only.sql in SQLCMD mode.';
END
ELSE
    PRINT N'EduHybrid already exists. Do not re-run the schema script (it will abort).';
GO
