-- Speeds /sms/records by SchoolID + date range (no CAST on Date).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_SMS_OtherInfo_SchoolID'
      AND object_id = OBJECT_ID(N'dbo.SMS_OtherInfo'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SMS_OtherInfo_SchoolID
    ON dbo.SMS_OtherInfo (SchoolID)
    INCLUDE (StudentID, TeacherID, SMS_NumberID, CommitteeMemberId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_SMS_Send_Record_Date'
      AND object_id = OBJECT_ID(N'dbo.SMS_Send_Record'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SMS_Send_Record_Date
    ON dbo.SMS_Send_Record ([Date] DESC)
    INCLUDE (PhoneNumber, TextCount, SMSCount, PurposeOfSMS, [Status]);
END
GO
