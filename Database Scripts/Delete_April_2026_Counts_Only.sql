-- =============================================
-- DELETE ONLY April 2026 Student Count Data
-- Keep invoices, delete only counts
-- =============================================

PRINT '========================================'
PRINT 'DELETING APRIL 2026 STUDENT COUNTS'
PRINT '========================================'
PRINT ''

-- Show before
PRINT 'BEFORE DELETE:'
SELECT COUNT(*) AS SchoolRecords FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
SELECT COUNT(*) AS ClassRecords FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
PRINT ''

-- Delete April 2026 student counts
DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
PRINT 'School records deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
PRINT 'Class records deleted: ' + CAST(@@ROWCOUNT AS VARCHAR)

PRINT ''
PRINT 'AFTER DELETE:'
SELECT COUNT(*) AS SchoolRecords FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
SELECT COUNT(*) AS ClassRecords FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026

PRINT ''
PRINT '========================================'
PRINT 'COMPLETE!'
PRINT '========================================'
PRINT 'April 2026 student count data deleted'
PRINT 'Invoices remain unchanged'
PRINT ''
PRINT 'Next: Generate correct April counts using Generate button'
