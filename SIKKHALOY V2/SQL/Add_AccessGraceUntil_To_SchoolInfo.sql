-- ============================================================
-- Invoice Expiry Access Control
-- SchoolInfo table-? AccessGraceUntil column ??? ????
-- Run this script once in your Edu database
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.SchoolInfo') 
    AND name = 'AccessGraceUntil'
)
BEGIN
    ALTER TABLE SchoolInfo
    ADD AccessGraceUntil DATETIME NULL;

    PRINT 'AccessGraceUntil column added to SchoolInfo table.';
END
ELSE
BEGIN
    PRINT 'AccessGraceUntil column already exists.';
END
GO

-- ============================================================
-- Check ???? query:
-- ============================================================
-- SELECT SchoolID, SchoolName, AccessGraceUntil FROM SchoolInfo ORDER BY SchoolID
-- GO
