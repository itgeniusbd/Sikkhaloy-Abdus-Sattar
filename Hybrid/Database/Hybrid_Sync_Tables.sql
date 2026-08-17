-- Optional but recommended. Lets Hybrid Sync API map LocalId -> ServerId
-- and pull changes without destructive queues. Run on EduHybrid; no existing table is altered.

IF OBJECT_ID(N'dbo.Hybrid_EntityMap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hybrid_EntityMap
    (
        LocalId UNIQUEIDENTIFIER NOT NULL,
        EntityType NVARCHAR(64) NOT NULL,
        ServerId INT NOT NULL,
        SchoolID INT NOT NULL,
        DeviceId NVARCHAR(64) NOT NULL,
        CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_Hybrid_EntityMap_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Hybrid_EntityMap PRIMARY KEY (LocalId),
        CONSTRAINT UQ_Hybrid_EntityMap_Type_Server UNIQUE (EntityType, ServerId)
    );

    CREATE INDEX IX_Hybrid_EntityMap_School_Type
        ON dbo.Hybrid_EntityMap (SchoolID, EntityType);
END
GO

IF OBJECT_ID(N'dbo.Hybrid_ChangeLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hybrid_ChangeLog
    (
        ChangeId BIGINT IDENTITY(1, 1) NOT NULL,
        SchoolID INT NOT NULL,
        EducationYearID INT NULL,
        EntityType NVARCHAR(64) NOT NULL,
        ServerId INT NOT NULL,
        LocalId UNIQUEIDENTIFIER NULL,
        Operation NVARCHAR(16) NOT NULL,
        ChangedUtc DATETIME2 NOT NULL CONSTRAINT DF_Hybrid_ChangeLog_ChangedUtc DEFAULT SYSUTCDATETIME(),
        OriginDeviceId NVARCHAR(64) NULL,
        CONSTRAINT PK_Hybrid_ChangeLog PRIMARY KEY (ChangeId)
    );

    CREATE INDEX IX_Hybrid_ChangeLog_School_Change
        ON dbo.Hybrid_ChangeLog (SchoolID, ChangeId);
END
GO
