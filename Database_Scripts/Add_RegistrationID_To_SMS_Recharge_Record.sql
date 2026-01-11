-- Add RegistrationID column to SMS_Recharge_Record table
-- This will track which user recharged the SMS

-- Check if column exists, if not then add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'dbo.SMS_Recharge_Record') 
               AND name = 'RegistrationID')
BEGIN
    ALTER TABLE SMS_Recharge_Record
    ADD RegistrationID INT NULL
    
    PRINT 'RegistrationID column added successfully to SMS_Recharge_Record table'
END
ELSE
BEGIN
    PRINT 'RegistrationID column already exists in SMS_Recharge_Record table'
END
GO

-- Add foreign key constraint (optional, for data integrity)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_SMS_Recharge_Record_Registration')
BEGIN
    ALTER TABLE SMS_Recharge_Record
    ADD CONSTRAINT FK_SMS_Recharge_Record_Registration
    FOREIGN KEY (RegistrationID) REFERENCES Registration(RegistrationID)
    
    PRINT 'Foreign key constraint added successfully'
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists'
END
GO
