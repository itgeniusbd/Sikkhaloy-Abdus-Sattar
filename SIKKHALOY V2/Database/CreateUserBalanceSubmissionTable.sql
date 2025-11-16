-- টেবিল তৈরি করুন যেখানে ইউজার থেকে অথরিটিতে টাকা জমা/প্রদানের রেকর্ড রাখা হবে

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_Balance_Submission]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[User_Balance_Submission](
 [SubmissionID] [int] IDENTITY(1,1) NOT NULL,
 [SchoolID] [int] NOT NULL,
      [RegistrationID] [int] NOT NULL,
        [SubmissionAmount] [decimal](18, 2) NOT NULL,
   [SubmissionDate] [datetime] NOT NULL,
  [ReceivedBy] [nvarchar](100) NULL,
        [ReceiverPhone] [nvarchar](15) NULL,
        [PaymentMethod] [nvarchar](50) NULL,
        [Remarks] [nvarchar](500) NULL,
 [CreatedDate] [datetime] NOT NULL DEFAULT (GETDATE()),
 [CreatedBy] [int] NULL,
     CONSTRAINT [PK_User_Balance_Submission] PRIMARY KEY CLUSTERED 
    (
  [SubmissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- ReceiverPhone column যুক্ত করুন যদি না থাকে
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User_Balance_Submission]') AND name = 'ReceiverPhone')
BEGIN
    ALTER TABLE [dbo].[User_Balance_Submission]
 ADD [ReceiverPhone] [nvarchar](15) NULL
  PRINT 'ReceiverPhone column added successfully!'
END
GO

-- SubmissionTime column মুছে ফেলুন যদি থাকে (আর প্রয়োজন নেই - CreatedDate ব্যবহার করা হবে)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User_Balance_Submission]') AND name = 'SubmissionTime')
BEGIN
    ALTER TABLE [dbo].[User_Balance_Submission]
    DROP COLUMN [SubmissionTime]
    PRINT 'SubmissionTime column removed - using CreatedDate instead!'
END
GO

-- Index তৈরি করুন দ্রুত সার্চের জন্য
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_Balance_Submission_School_User')
BEGIN
  CREATE NONCLUSTERED INDEX [IX_User_Balance_Submission_School_User] 
  ON [dbo].[User_Balance_Submission] ([SchoolID], [RegistrationID])
    INCLUDE ([SubmissionAmount], [SubmissionDate], [CreatedDate])
END
GO

-- Comment যোগ করুন
EXEC sys.sp_addextendedproperty 
    @name=N'MS_Description', 
    @value=N'ইউজার থেকে অথরিটিতে টাকা জমা/প্রদানের রেকর্ড রাখার টেবিল (OTP সহ, CreatedDate এ date ও time সংরক্ষিত)' , 
  @level0type=N'SCHEMA',
    @level0name=N'dbo', 
    @level1type=N'TABLE',
    @level1name=N'User_Balance_Submission'
GO

PRINT 'User_Balance_Submission table updated - SubmissionTime removed, using CreatedDate for both date and time!'
