-- =============================================
-- Migration: School-specific leave types for gate pass
-- Table: Attendance_Leave_Type
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance_Leave_Type]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[Attendance_Leave_Type](
        [LeaveTypeID] [int] IDENTITY(1,1) NOT NULL,
        [SchoolID] [int] NOT NULL,
        [LeaveTypeName] [nvarchar](100) NOT NULL,
        [SortOrder] [int] NOT NULL CONSTRAINT [DF_Attendance_Leave_Type_SortOrder] DEFAULT (0),
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_Attendance_Leave_Type_IsActive] DEFAULT (1),
        [CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_Attendance_Leave_Type_CreatedDate] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Attendance_Leave_Type] PRIMARY KEY CLUSTERED ([LeaveTypeID] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_Attendance_Leave_Type_SchoolID]
        ON [dbo].[Attendance_Leave_Type]([SchoolID], [IsActive], [SortOrder]);
END
GO
