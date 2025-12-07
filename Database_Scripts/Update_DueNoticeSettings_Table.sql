-- ================================================
-- Database Update Script for Due Notice Settings
-- Purpose: Change from IsHidden to IsEnabled logic
-- ================================================

-- Step 1: Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo_DueNoticeSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SchoolInfo_DueNoticeSettings](
        [SettingID] [int] IDENTITY(1,1) NOT NULL,
        [SchoolID] [int] NOT NULL,
        [IsEnabled] [bit] NOT NULL DEFAULT 0,
        [HideUntilDate] [datetime] NULL,
        [Reason] [nvarchar](500) NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [CreatedBy] [int] NULL,
        CONSTRAINT [PK_SchoolInfo_DueNoticeSettings] PRIMARY KEY CLUSTERED ([SettingID] ASC)
    )
    
    PRINT 'Table SchoolInfo_DueNoticeSettings created successfully.'
END
ELSE
BEGIN
    PRINT 'Table SchoolInfo_DueNoticeSettings already exists.'
    
    -- Step 2: Check if IsHidden column exists (old structure)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo_DueNoticeSettings]') AND name = 'IsHidden')
    BEGIN
        PRINT 'Updating table structure from IsHidden to IsEnabled...'
        
        -- Add IsEnabled column if not exists
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo_DueNoticeSettings]') AND name = 'IsEnabled')
        BEGIN
            ALTER TABLE [dbo].[SchoolInfo_DueNoticeSettings]
            ADD [IsEnabled] [bit] NOT NULL DEFAULT 0
            
            PRINT 'IsEnabled column added.'
        END
        
        -- Convert data: IsHidden = 1 means notification was hidden, so IsEnabled = 0
        -- IsHidden = 0 means notification was shown, so we don't enable it by default (keep IsEnabled = 0)
        -- This ensures all institutions start with notification disabled by default
        UPDATE [dbo].[SchoolInfo_DueNoticeSettings]
        SET [IsEnabled] = 0  -- Set all to disabled by default
        WHERE [IsHidden] IS NOT NULL
        
        PRINT 'Data converted - All notifications set to disabled by default.'
        
        -- Drop IsHidden column
        ALTER TABLE [dbo].[SchoolInfo_DueNoticeSettings]
        DROP COLUMN [IsHidden]
        
        PRINT 'IsHidden column removed.'
    END
    ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo_DueNoticeSettings]') AND name = 'IsEnabled')
    BEGIN
        -- Add IsEnabled column if it doesn't exist
        ALTER TABLE [dbo].[SchoolInfo_DueNoticeSettings]
        ADD [IsEnabled] [bit] NOT NULL DEFAULT 0
        
        PRINT 'IsEnabled column added to existing table.'
    END
    ELSE
    BEGIN
        PRINT 'Table structure is already up to date with IsEnabled column.'
    END
END

-- Step 3: Create index for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo_DueNoticeSettings]') AND name = N'IX_SchoolInfo_DueNoticeSettings_SchoolID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SchoolInfo_DueNoticeSettings_SchoolID]
    ON [dbo].[SchoolInfo_DueNoticeSettings] ([SchoolID])
    INCLUDE ([IsEnabled], [HideUntilDate], [CreatedDate])
    
    PRINT 'Index created for better query performance.'
END

-- Step 4: Show current settings status
PRINT ''
PRINT '=== Current Settings Status ==='
SELECT 
    s.SchoolName,
    CASE WHEN dns.IsEnabled = 1 THEN 'Enabled' ELSE 'Disabled' END AS NotificationStatus,
    dns.HideUntilDate,
    dns.Reason,
    dns.CreatedDate
FROM SchoolInfo s
LEFT JOIN SchoolInfo_DueNoticeSettings dns ON s.SchoolID = dns.SchoolID
WHERE dns.SettingID IS NOT NULL
ORDER BY dns.CreatedDate DESC

PRINT ''
PRINT '=== Update Complete ==='
PRINT 'Default behavior: All institutions have due notice DISABLED by default.'
PRINT 'Authority can enable notification for specific institutions from Institution_Details page.'
