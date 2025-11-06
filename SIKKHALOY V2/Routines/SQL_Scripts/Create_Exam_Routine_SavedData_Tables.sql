-- =============================================
-- Exam Routine Bangla - Save/Load Feature
-- ??????? ????? ???? ???? Script
-- =============================================

-- Main table to store routine metadata
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Routine_SavedData]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Exam_Routine_SavedData](
        [RoutineID] [int] IDENTITY(1,1) NOT NULL,
   [SchoolID] [int] NOT NULL,
        [RoutineName] [nvarchar](200) NOT NULL,
        [TitleText] [nvarchar](500) NULL,
        [SubtitleText] [nvarchar](500) NULL,
        [ExamInfoText] [nvarchar](500) NULL,
        [InstructionText] [nvarchar](max) NULL,
        [NotesText] [nvarchar](max) NULL,
        [SignatureText] [nvarchar](200) NULL,
        [ClassColumnCount] [int] NOT NULL DEFAULT 1,
        [RowCount] [int] NOT NULL DEFAULT 1,
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedDate] [datetime] NULL,
        [CreatedBy] [int] NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Exam_Routine_SavedData] PRIMARY KEY CLUSTERED ([RoutineID] ASC)
    )
END
GO

-- Table to store individual routine rows (dates)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Routine_Rows]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Exam_Routine_Rows](
[RowID] [int] IDENTITY(1,1) NOT NULL,
        [RoutineID] [int] NOT NULL,
        [RowIndex] [int] NOT NULL,
     [ExamDate] [date] NULL,
        [DayName] [nvarchar](50) NULL,
        [ExamTime] [nvarchar](50) NULL,
        CONSTRAINT [PK_Exam_Routine_Rows] PRIMARY KEY CLUSTERED ([RowID] ASC),
        CONSTRAINT [FK_Exam_Routine_Rows_SavedData] FOREIGN KEY([RoutineID])
      REFERENCES [dbo].[Exam_Routine_SavedData] ([RoutineID])
          ON DELETE CASCADE
)
END
GO

-- Table to store class columns
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Routine_ClassColumns]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Exam_Routine_ClassColumns](
        [ColumnID] [int] IDENTITY(1,1) NOT NULL,
        [RoutineID] [int] NOT NULL,
        [ColumnIndex] [int] NOT NULL,
        [ClassID] [int] NOT NULL,
 CONSTRAINT [PK_Exam_Routine_ClassColumns] PRIMARY KEY CLUSTERED ([ColumnID] ASC),
    CONSTRAINT [FK_Exam_Routine_ClassColumns_SavedData] FOREIGN KEY([RoutineID])
         REFERENCES [dbo].[Exam_Routine_SavedData] ([RoutineID])
            ON DELETE CASCADE
    )
END
GO

-- Table to store individual cell data (subjects and times for each class)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exam_Routine_CellData]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Exam_Routine_CellData](
        [CellID] [int] IDENTITY(1,1) NOT NULL,
        [RoutineID] [int] NOT NULL,
        [RowIndex] [int] NOT NULL,
 [ColumnIndex] [int] NOT NULL,
        [SubjectID] [int] NULL,
        [SubjectText] [nvarchar](200) NULL,
        [TimeText] [nvarchar](50) NULL,
        CONSTRAINT [PK_Exam_Routine_CellData] PRIMARY KEY CLUSTERED ([CellID] ASC),
        CONSTRAINT [FK_Exam_Routine_CellData_SavedData] FOREIGN KEY([RoutineID])
     REFERENCES [dbo].[Exam_Routine_SavedData] ([RoutineID])
  ON DELETE CASCADE
    )
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Exam_Routine_SavedData_SchoolID')
BEGIN
CREATE NONCLUSTERED INDEX [IX_Exam_Routine_SavedData_SchoolID]
    ON [dbo].[Exam_Routine_SavedData] ([SchoolID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Exam_Routine_Rows_RoutineID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Exam_Routine_Rows_RoutineID]
    ON [dbo].[Exam_Routine_Rows] ([RoutineID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Exam_Routine_ClassColumns_RoutineID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Exam_Routine_ClassColumns_RoutineID]
    ON [dbo].[Exam_Routine_ClassColumns] ([RoutineID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Exam_Routine_CellData_RoutineID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Exam_Routine_CellData_RoutineID]
    ON [dbo].[Exam_Routine_CellData] ([RoutineID])
END
GO

PRINT 'Exam Routine Bangla database tables created successfully!'
