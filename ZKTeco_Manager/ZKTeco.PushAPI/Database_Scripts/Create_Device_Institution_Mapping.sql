-- =============================================
-- Device-Institution Mapping Table
-- Purpose: Map ZKTeco Device Serial Numbers to Institutions
-- =============================================
-- This table is crucial for identifying which device belongs to which institution

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Device_Institution_Mapping]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Device_Institution_Mapping](
        [MappingID] [int] IDENTITY(1,1) NOT NULL,
        [SchoolID] [int] NOT NULL,
        [DeviceSerialNumber] [nvarchar](50) NOT NULL,
        [DeviceName] [nvarchar](100) NULL,
        [DeviceLocation] [nvarchar](200) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [LastPushTime] [datetime] NULL,
        [Remarks] [nvarchar](500) NULL,
        CONSTRAINT [PK_Device_Institution_Mapping] PRIMARY KEY CLUSTERED 
        (
            [MappingID] ASC
        )
    ) ON [PRIMARY]

    PRINT 'Table Device_Institution_Mapping created successfully!'
END
ELSE
BEGIN
    PRINT 'Table Device_Institution_Mapping already exists.'
END
GO

-- Create unique index on DeviceSerialNumber (one device can only belong to one institution)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Device_Institution_Mapping_DeviceSN')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Device_Institution_Mapping_DeviceSN] 
    ON [dbo].[Device_Institution_Mapping] ([DeviceSerialNumber] ASC)
    WHERE ([IsActive] = 1)
    
    PRINT 'Unique index on DeviceSerialNumber created successfully!'
END
GO

-- Create index for SchoolID lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Device_Institution_Mapping_SchoolID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Device_Institution_Mapping_SchoolID] 
    ON [dbo].[Device_Institution_Mapping] ([SchoolID] ASC, [IsActive] ASC)
    INCLUDE ([DeviceSerialNumber], [LastPushTime])
    
    PRINT 'Index on SchoolID created successfully!'
END
GO

-- Add foreign key constraint (optional)
-- IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Device_Institution_Mapping_SchoolInfo')
-- BEGIN
--     ALTER TABLE [dbo].[Device_Institution_Mapping]  WITH CHECK ADD  
--     CONSTRAINT [FK_Device_Institution_Mapping_SchoolInfo] FOREIGN KEY([SchoolID])
--     REFERENCES [dbo].[SchoolInfo] ([SchoolID])
--     ON DELETE CASCADE
--     
--     PRINT 'Foreign key constraint added successfully!'
-- END
-- GO

PRINT ''
PRINT 'Device_Institution_Mapping table setup completed!'
PRINT 'Now you can map device serial numbers to institutions.'
GO

-- Sample data (for testing - replace with your actual data)
-- INSERT INTO Device_Institution_Mapping (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation)
-- VALUES 
-- (1, 'CKLT123456', 'Main Gate Device', 'School Main Entrance'),
-- (1, 'CKLT789012', 'Office Device', 'Admin Office'),
-- (2, 'CKLT345678', 'Gate Device', 'Institution Gate')

