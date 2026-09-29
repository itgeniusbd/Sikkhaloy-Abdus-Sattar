/* LocalDB ONLY. Schema state used to prove a re-run of the setup changes nothing. */
SET NOCOUNT ON;
SELECT 'IDX' k, name, CONVERT(nvarchar(10), is_unique) u, ISNULL(filter_definition, N'-') f,
       (SELECT STRING_AGG(CONVERT(nvarchar(10), used_page_count), ',') FROM sys.dm_db_partition_stats p WHERE p.object_id = i.object_id AND p.index_id = i.index_id) pages
FROM sys.indexes i WHERE object_id = OBJECT_ID(N'dbo.Income_MoneyReceipt') AND index_id > 0 ORDER BY index_id;
SELECT 'SEQ' k, name, CONVERT(nvarchar(30), create_date, 121) c, CONVERT(nvarchar(30), modify_date, 121) m,
       CONVERT(nvarchar(20), current_value) cv, CONVERT(nvarchar(20), ISNULL(last_used_value, -1)) lu
FROM sys.sequences ORDER BY name;
SELECT 'PROC' k, name, CHECKSUM(OBJECT_DEFINITION(object_id)) def_checksum FROM sys.procedures ORDER BY name;
SELECT 'ROWS' k, COUNT(*) n, CHECKSUM_AGG(CHECKSUM(MoneyReceiptID, SchoolID, MoneyReceipt_SN)) h FROM dbo.Income_MoneyReceipt;
