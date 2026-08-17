-- Restore Edu backup as EduHybrid with data files on F: (C: does not have room
-- for a second ~24 GB copy next to live Edu).

USE master;
GO

IF DB_ID(N'EduHybrid') IS NOT NULL
BEGIN
    ALTER DATABASE EduHybrid SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EduHybrid;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.master_files WHERE physical_name LIKE N'F:\SQLData\%')
BEGIN
    -- folder is created by the restore host; this is documentation only
    PRINT 'Ensure folder F:\SQLData exists before restore.';
END
GO

RESTORE DATABASE EduHybrid
FROM DISK = N'F:\Edu.BAK'
WITH
    MOVE N'IISAC' TO N'F:\SQLData\EduHybrid.mdf',
    MOVE N'IISAC_log' TO N'F:\SQLData\EduHybrid_log.ldf',
    REPLACE,
    STATS = 10;
GO
