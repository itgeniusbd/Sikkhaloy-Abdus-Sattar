-- =============================================
-- CLEANUP & FIX - March 2026 (ACTIVE INSTITUTIONS ONLY)
-- =============================================

PRINT '========================================='
PRINT 'STEP 1: CHECKING CURRENT STATUS'
PRINT '========================================='
PRINT ''

-- Show inactive institutions that will be deleted
PRINT 'INACTIVE INSTITUTIONS (will be deleted):'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    ISNULL(SCM.Active_Student, 0) AS Students
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
AND ISNULL(SI.IS_ServiceChargeActive, 0) = 0
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'STEP 2: DELETING INACTIVE INSTITUTIONS'
PRINT '========================================='
PRINT ''

-- Delete inactive institutions
DELETE FROM AAP_Student_Count_Monthly 
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
AND SchoolID IN (
    SELECT SchoolID FROM SchoolInfo 
    WHERE ISNULL(IS_ServiceChargeActive, 0) = 0
)

DECLARE @DeletedSchools INT = @@ROWCOUNT
PRINT '? Deleted ' + CAST(@DeletedSchools AS NVARCHAR) + ' inactive institutions from AAP_Student_Count_Monthly'

DELETE FROM AAP_StudentClass_Count_Monthly 
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
AND SchoolID IN (
    SELECT SchoolID FROM SchoolInfo 
    WHERE ISNULL(IS_ServiceChargeActive, 0) = 0
)

DECLARE @DeletedClasses INT = @@ROWCOUNT
PRINT '? Deleted ' + CAST(@DeletedClasses AS NVARCHAR) + ' class records from AAP_StudentClass_Count_Monthly'
PRINT ''

PRINT '========================================='
PRINT 'STEP 3: FINAL VERIFICATION'
PRINT '========================================='
PRINT ''

-- Show remaining ACTIVE institutions
PRINT 'REMAINING ACTIVE INSTITUTIONS:'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    SI.IS_ServiceChargeActive,
    SCM.StudentCount,
    SCM.Active_Student,
    CONVERT(VARCHAR(10), SCM.Month, 120) AS Month
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'CLEANUP COMPLETE!'
PRINT '========================================='
PRINT ''
PRINT 'Summary:'
PRINT '  - Inactive institutions deleted: ' + CAST(@DeletedSchools AS NVARCHAR)
PRINT '  - Class records deleted: ' + CAST(@DeletedClasses AS NVARCHAR)
PRINT '  - Remaining active institutions: ' + CAST((SELECT COUNT(*) FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026) AS NVARCHAR)
PRINT ''
PRINT '? Next: Refresh browser and check Create Invoice page'
PRINT '? Only ACTIVE (blue) institutions should appear now'
