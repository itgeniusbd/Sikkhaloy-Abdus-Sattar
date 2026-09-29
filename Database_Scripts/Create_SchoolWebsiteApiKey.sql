-- Per-school public website API keys (IISAC / school websites)
IF OBJECT_ID(N'dbo.SchoolWebsiteApiKey', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchoolWebsiteApiKey
    (
        SchoolID INT NOT NULL CONSTRAINT PK_SchoolWebsiteApiKey PRIMARY KEY,
        ApiKey NVARCHAR(128) NOT NULL,
        AllowedHosts NVARCHAR(500) NULL,
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_SchoolWebsiteApiKey_UpdatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy NVARCHAR(100) NULL
    );
END
GO
