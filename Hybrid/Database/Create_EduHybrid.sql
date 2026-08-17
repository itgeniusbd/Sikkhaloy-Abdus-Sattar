-- Creates an empty EduHybrid database. Schema/data must still come from Edu
-- (backup/restore, Copy Database, or generate scripts). Hybrid Sync API uses
-- Initial Catalog=EduHybrid and will not write to the live Edu database.

IF DB_ID(N'EduHybrid') IS NULL
BEGIN
    CREATE DATABASE EduHybrid;
END
GO

-- Do not restore onto C: next to live Edu (~24 GB). Use Restore_EduHybrid_To_F.sql
-- so files land on F:\SQLData.
