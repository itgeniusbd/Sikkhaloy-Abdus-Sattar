-- Student / guardian complaints and suggestions sent to Admin or Sub-Admin.
-- The Sync API also creates these tables on first request if they are missing.

IF OBJECT_ID(N'dbo.Student_Portal_Feedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback (
        FeedbackID int IDENTITY(1,1) NOT NULL,
        SchoolID int NOT NULL,
        EducationYearID int NOT NULL,
        StudentID int NOT NULL,
        StudentClassID int NOT NULL,
        Kind nvarchar(20) NOT NULL,
        Title nvarchar(200) NOT NULL,
        Body nvarchar(max) NOT NULL,
        ToRegistrationID int NOT NULL,
        ToName nvarchar(200) NOT NULL,
        ToDesignation nvarchar(200) NOT NULL CONSTRAINT DF_StudentPortalFeedback_Desig DEFAULT (N''),
        ToCategory nvarchar(20) NOT NULL,
        Status nvarchar(20) NOT NULL CONSTRAINT DF_StudentPortalFeedback_Status DEFAULT (N'Pending'),
        AssignedByRegistrationID int NULL,
        AssignedByName nvarchar(200) NULL,
        AssignedByDesignation nvarchar(200) NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedback_Created DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback PRIMARY KEY (FeedbackID)
    );
    CREATE INDEX IX_Student_Portal_Feedback_School ON dbo.Student_Portal_Feedback (SchoolID, CreatedAt DESC);
    CREATE INDEX IX_Student_Portal_Feedback_Student ON dbo.Student_Portal_Feedback (SchoolID, StudentID, CreatedAt DESC);
    CREATE INDEX IX_Student_Portal_Feedback_To ON dbo.Student_Portal_Feedback (SchoolID, ToRegistrationID, CreatedAt DESC);
    PRINT 'Table Student_Portal_Feedback created.';
END
GO

IF OBJECT_ID(N'dbo.Student_Portal_Feedback_Read', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback_Read (
        FeedbackID int NOT NULL,
        RegistrationID int NOT NULL,
        ReadAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedbackRead_At DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback_Read PRIMARY KEY (FeedbackID, RegistrationID)
    );
    PRINT 'Table Student_Portal_Feedback_Read created.';
END
GO

IF OBJECT_ID(N'dbo.Student_Portal_Feedback', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Student_Portal_Feedback', N'Status') IS NULL
BEGIN
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_StudentPortalFeedback_Status DEFAULT (N'Pending');
    PRINT 'Column Status added to Student_Portal_Feedback.';
END
GO

IF OBJECT_ID(N'dbo.Student_Portal_Feedback', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Student_Portal_Feedback', N'AssignedByRegistrationID') IS NULL
BEGIN
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD AssignedByRegistrationID int NULL,
        AssignedByName nvarchar(200) NULL,
        AssignedByDesignation nvarchar(200) NULL;
    PRINT 'AssignedBy columns added to Student_Portal_Feedback.';
END
GO

IF OBJECT_ID(N'dbo.Student_Portal_Feedback', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Student_Portal_Feedback', N'ProcessingByRegistrationID') IS NULL
BEGIN
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD ProcessingByRegistrationID int NULL,
        ProcessingByName nvarchar(200) NULL,
        ProcessingByDesignation nvarchar(200) NULL,
        ProcessingAt datetime NULL,
        SolvedByRegistrationID int NULL,
        SolvedByName nvarchar(200) NULL,
        SolvedByDesignation nvarchar(200) NULL,
        SolvedAt datetime NULL;
    PRINT 'Status-by columns added to Student_Portal_Feedback.';
END
GO

IF OBJECT_ID(N'dbo.Student_Portal_Feedback_Reply', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback_Reply (
        ReplyID int IDENTITY(1,1) NOT NULL,
        FeedbackID int NOT NULL,
        SchoolID int NOT NULL,
        FromRegistrationID int NOT NULL,
        FromName nvarchar(200) NOT NULL,
        FromDesignation nvarchar(200) NOT NULL CONSTRAINT DF_StudentPortalFeedbackReply_Desig DEFAULT (N''),
        FromCategory nvarchar(20) NOT NULL,
        Body nvarchar(max) NOT NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedbackReply_At DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback_Reply PRIMARY KEY (ReplyID)
    );
    CREATE INDEX IX_Student_Portal_Feedback_Reply_Fb ON dbo.Student_Portal_Feedback_Reply (FeedbackID, CreatedAt);
    PRINT 'Table Student_Portal_Feedback_Reply created.';
END
GO

IF OBJECT_ID(N'dbo.Student_Portal_Feedback_Reply_Read', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback_Reply_Read (
        ReplyID int NOT NULL,
        RegistrationID int NOT NULL,
        ReadAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedbackReplyRead_At DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback_Reply_Read PRIMARY KEY (ReplyID, RegistrationID)
    );
    PRINT 'Table Student_Portal_Feedback_Reply_Read created.';
END
GO
