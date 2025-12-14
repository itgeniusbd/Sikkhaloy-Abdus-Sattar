-- Create Device_Commands Table for storing device configuration commands
-- ?? table ???????? command ??????? ???? ??????? ???

-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Device_Commands')
BEGIN
    CREATE TABLE [dbo].[Device_Commands] (
        [CommandID] INT IDENTITY(1,1) PRIMARY KEY,
        [DeviceSerialNumber] NVARCHAR(100) NOT NULL,
        [Command] NVARCHAR(MAX) NOT NULL,
        [CommandType] NVARCHAR(50) NULL, -- 'DATETIME', 'CONFIG', 'RESTART', etc.
        [CommandStatus] NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Sent', 'Completed', 'Failed'
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ProcessedDate] DATETIME NULL,
        [ResponseData] NVARCHAR(MAX) NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedBy] NVARCHAR(100) NULL,
        [SchoolID] INT NULL
    )

    -- Create indexes for better performance
    CREATE INDEX IX_Device_Commands_Serial ON Device_Commands(DeviceSerialNumber)
    CREATE INDEX IX_Device_Commands_Status ON Device_Commands(CommandStatus)
    CREATE INDEX IX_Device_Commands_Date ON Device_Commands(CreatedDate DESC)

    PRINT 'Device_Commands table created successfully!'
END
ELSE
BEGIN
    PRINT 'Device_Commands table already exists!'
END
GO

-- Insert sample command for testing (Optional - Comment out if not needed)
/*
INSERT INTO Device_Commands (DeviceSerialNumber, Command, CommandType, CommandStatus, CreatedBy)
VALUES 
('YOUR_DEVICE_SN', 'C:1:SET OPTION ~TimeZone=6', 'DATETIME', 'Pending', 'System'),
('YOUR_DEVICE_SN', 'C:2:DATA UPDATE ATTLOG Stamp=' + CAST(CAST(GETDATE() AS FLOAT) AS NVARCHAR), 'CONFIG', 'Pending', 'System')
*/

-- View all commands
SELECT * FROM Device_Commands ORDER BY CreatedDate DESC

-- View pending commands only
SELECT * FROM Device_Commands WHERE CommandStatus = 'Pending' ORDER BY CreatedDate ASC

-- Clean up old completed commands (older than 30 days)
-- DELETE FROM Device_Commands WHERE CommandStatus IN ('Completed', 'Sent') AND ProcessedDate < DATEADD(DAY, -30, GETDATE())
