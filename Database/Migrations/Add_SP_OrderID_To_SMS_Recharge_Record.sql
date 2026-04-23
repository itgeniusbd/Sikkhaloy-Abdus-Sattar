-- Add SP_OrderID column to SMS_Recharge_Record table
-- This stores the ShurjoPay order ID to prevent duplicate recharge on callback retry

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'dbo.SMS_Recharge_Record') 
               AND name = 'SP_OrderID')
BEGIN
    ALTER TABLE SMS_Recharge_Record
    ADD SP_OrderID NVARCHAR(100) NULL

    PRINT 'SP_OrderID column added successfully to SMS_Recharge_Record table'
END
ELSE
BEGIN
    PRINT 'SP_OrderID column already exists in SMS_Recharge_Record table'
END
GO
