USE ReceiptSnTest; SET NOCOUNT ON;
DECLARE @i int = 0, @err int = 0, @Sn int;
WHILE @i < $(N)
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        SELECT @Sn = ISNULL(MAX(MoneyReceipt_SN), 100000) FROM dbo.Income_MoneyReceipt WITH (UPDLOCK, HOLDLOCK) WHERE SchoolID = 1208;
        IF (@Sn < 100000) SET @Sn = 100000;
        INSERT INTO dbo.Income_MoneyReceipt (SchoolID, PaymentBy, MoneyReceipt_SN, CollectionDate) VALUES (1208, N'OLDHYBRID', @Sn + 1, GETDATE());
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @err += 1; PRINT ERROR_MESSAGE();
    END CATCH
    SET @i += 1;
END
SELECT 'OLDHYBRID' AS worker, @i AS attempts, @err AS errors;
