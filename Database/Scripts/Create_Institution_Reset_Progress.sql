-- Progress table for live row counts during institution reset
USE [Edu];
GO

IF OBJECT_ID(N'dbo.Institution_Reset_Progress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Institution_Reset_Progress
    (
        SchoolID         INT            NOT NULL CONSTRAINT PK_Institution_Reset_Progress PRIMARY KEY,
        Mode             VARCHAR(20)    NOT NULL,
        EducationYearID  INT            NULL,
        TotalRows        BIGINT         NOT NULL CONSTRAINT DF_InstResetProg_Total DEFAULT (0),
        DeletedRows      BIGINT         NOT NULL CONSTRAINT DF_InstResetProg_Deleted DEFAULT (0),
        Status           NVARCHAR(20)   NOT NULL,  -- Running / Done / Error
        Message          NVARCHAR(500)  NULL,
        UpdatedAt        DATETIME2(0)   NOT NULL CONSTRAINT DF_InstResetProg_Updated DEFAULT (SYSUTCDATETIME())
    );
END
GO
