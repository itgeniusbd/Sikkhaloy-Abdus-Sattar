-- ============================================================
-- ShurjoPay Online Payment Table
-- Sikkhaloy V3 - IT Genius
-- Run this script once in your Edu database
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = 'AAP_Invoice_OnlinePayment'
)
BEGIN
    CREATE TABLE AAP_Invoice_OnlinePayment (
        PaymentID       INT IDENTITY(1,1) PRIMARY KEY,
        SchoolID        INT NOT NULL,
        SP_OrderID      NVARCHAR(100) NOT NULL,
        SP_TrxID        NVARCHAR(200) NULL,
        SP_Method       NVARCHAR(100) NULL,
        Amount          DECIMAL(18,2) NOT NULL DEFAULT 0,
        SP_Code         NVARCHAR(20)  NULL,
        SP_Message      NVARCHAR(500) NULL,
        PaymentDate     DATETIME      NULL,
        CreatedDate     DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_SP_OrderID UNIQUE (SP_OrderID)
    );

    CREATE INDEX IX_OnlinePayment_SchoolID ON AAP_Invoice_OnlinePayment (SchoolID);
    CREATE INDEX IX_OnlinePayment_OrderID  ON AAP_Invoice_OnlinePayment (SP_OrderID);

    PRINT 'Table AAP_Invoice_OnlinePayment created successfully.';
END
ELSE
BEGIN
    PRINT 'Table AAP_Invoice_OnlinePayment already exists.';
END
GO
