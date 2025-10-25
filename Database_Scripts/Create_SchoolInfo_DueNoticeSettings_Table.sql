-- =============================================
-- Database Table for Due Invoice Notice Settings
-- =============================================
-- This table stores settings for hiding due invoice notifications
-- for specific schools temporarily or permanently

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo_DueNoticeSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SchoolInfo_DueNoticeSettings](
        [SettingID] [int] IDENTITY(1,1) NOT NULL,
        [SchoolID] [int] NOT NULL,
        [IsHidden] [bit] NOT NULL DEFAULT 0,
        [HideUntilDate] [datetime] NULL,
        [Reason] [nvarchar](500) NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [CreatedBy] [int] NULL,
        CONSTRAINT [PK_SchoolInfo_DueNoticeSettings] PRIMARY KEY CLUSTERED 
        (
            [SettingID] ASC
        )
    ) ON [PRIMARY]

    PRINT 'Table SchoolInfo_DueNoticeSettings created successfully!'
END
ELSE
BEGIN
    PRINT 'Table SchoolInfo_DueNoticeSettings already exists.'
END
GO

-- Create index for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SchoolInfo_DueNoticeSettings_SchoolID_IsHidden')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SchoolInfo_DueNoticeSettings_SchoolID_IsHidden] 
    ON [dbo].[SchoolInfo_DueNoticeSettings] ([SchoolID] ASC, [IsHidden] ASC)
    INCLUDE ([HideUntilDate], [CreatedDate])
    
    PRINT 'Index created successfully!'
END
GO

-- Add foreign key constraint (optional, depends on your schema)
-- IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SchoolInfo_DueNoticeSettings_SchoolInfo')
-- BEGIN
--     ALTER TABLE [dbo].[SchoolInfo_DueNoticeSettings]  WITH CHECK ADD  
--     CONSTRAINT [FK_SchoolInfo_DueNoticeSettings_SchoolInfo] FOREIGN KEY([SchoolID])
--     REFERENCES [dbo].[SchoolInfo] ([SchoolID])
--     ON DELETE CASCADE
--     
--     PRINT 'Foreign key constraint added successfully!'
-- END
-- GO

PRINT 'SchoolInfo_DueNoticeSettings table setup completed!'
GO

-- Sample data (for testing)
-- INSERT INTO SchoolInfo_DueNoticeSettings (SchoolID, IsHidden, HideUntilDate, Reason, CreatedBy)
-- VALUES (1, 1, '2025-02-28', '??????? ????? ??,??? ????', 1)
