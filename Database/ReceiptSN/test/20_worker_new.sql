USE ReceiptSnTest; SET NOCOUNT ON;
DECLARE @i int = 0, @err int = 0;
DECLARE @x TABLE (id decimal(18,0));
WHILE @i < $(N)
BEGIN
    BEGIN TRY
        BEGIN TRAN;
        INSERT @x EXEC dbo.MoneyReceipt 1,1,1,1,N'$(TAG)','2026-09-28',1208;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @err += 1; PRINT ERROR_MESSAGE();
    END CATCH
    SET @i += 1;
END
SELECT '$(TAG)' AS worker, @i AS attempts, @err AS errors;
