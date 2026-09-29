-- Persist student portal notice / SMS / report "read" state per school + student + year.
-- The Sync API also creates this table on first inbox-reads request if it is missing.

IF OBJECT_ID(N'dbo.Student_Portal_Inbox_Read', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Inbox_Read (
        SchoolID int NOT NULL,
        StudentID int NOT NULL,
        EducationYearID int NOT NULL,
        NoticeReads nvarchar(max) NOT NULL CONSTRAINT DF_StudentPortalInbox_Notice DEFAULT (N'[]'),
        SmsReads nvarchar(max) NOT NULL CONSTRAINT DF_StudentPortalInbox_Sms DEFAULT (N'[]'),
        ReportReads nvarchar(max) NOT NULL CONSTRAINT DF_StudentPortalInbox_Report DEFAULT (N'[]'),
        UpdatedAt datetime NOT NULL CONSTRAINT DF_StudentPortalInbox_Updated DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Inbox_Read PRIMARY KEY (SchoolID, StudentID, EducationYearID)
    );

    PRINT 'Table Student_Portal_Inbox_Read created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Student_Portal_Inbox_Read already exists.';
END
GO
