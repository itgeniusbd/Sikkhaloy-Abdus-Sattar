-- Create Exam_Routine_Bangla table for storing exam routine data

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Routine_Bangla]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Exam_Routine_Bangla](
     [RoutineID] [int] IDENTITY(1,1) NOT NULL,
  [SchoolID] [int] NOT NULL,
        [RegistrationID] [int] NOT NULL,
 [ClassID] [int] NOT NULL,
    [EducationYearID] [int] NOT NULL,
        
        -- Date and Time fields
 [ExamDate] [nvarchar](50) NULL,
        [DayName] [nvarchar](50) NULL,
        [ExamTime] [nvarchar](50) NULL,
        
     -- Subject and Time for 5 periods
     [Subject1] [nvarchar](200) NULL,
        [Time1] [nvarchar](50) NULL,
        [Subject2] [nvarchar](200) NULL,
        [Time2] [nvarchar](50) NULL,
        [Subject3] [nvarchar](200) NULL,
        [Time3] [nvarchar](50) NULL,
        [Subject4] [nvarchar](200) NULL,
        [Time4] [nvarchar](50) NULL,
        [Subject5] [nvarchar](200) NULL,
        [Time5] [nvarchar](50) NULL,
        
        -- Header information
        [Title] [nvarchar](500) NULL,
        [Subtitle] [nvarchar](500) NULL,
 [ExamInfo] [nvarchar](500) NULL,
      [Instructions] [nvarchar](MAX) NULL,
        [Notes] [nvarchar](MAX) NULL,
    [Signature] [nvarchar](200) NULL,
        
        [CreatedDate] [datetime] NULL,
        [UpdatedDate] [datetime] NULL,
        
        CONSTRAINT [PK_Exam_Routine_Bangla] PRIMARY KEY CLUSTERED 
        (
     [RoutineID] ASC
        )
    )
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Routine_Bangla]') AND name = N'IX_Exam_Routine_School_Class')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Exam_Routine_School_Class]
    ON [dbo].[Exam_Routine_Bangla] ([SchoolID], [ClassID], [EducationYearID])
END
GO

PRINT 'Exam_Routine_Bangla table created successfully!'
