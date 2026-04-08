-- =============================================
-- CHECK & DELETE Inactive Institutions from March 2026
-- =============================================

PRINT '========================================='
PRINT 'CHECKING INACTIVE INSTITUTIONS'
PRINT '========================================='
PRINT ''

-- Step 1: Show which institutions are INACTIVE
PRINT 'INACTIVE INSTITUTIONS (will be deleted):'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    ISNULL(SI.IS_ServiceChargeActive, 0) AS ServiceChargeActive,
    SCM.Active_Student,
    'WILL BE DELETED' AS Action
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
AND ISNULL(SI.IS_ServiceChargeActive, 0) = 0 -- Inactive institutions
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='

-- Step 2: Show which institutions will REMAIN
PRINT 'ACTIVE INSTITUTIONS (will be kept):'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    SI.IS_ServiceChargeActive,
    SCM.Active_Student,
    'WILL KEEP' AS Action
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
AND SI.IS_ServiceChargeActive = 1 -- Active institutions
ORDER BY SI.SchoolID

PRINT ''
PRINT '========================================='
PRINT 'READY TO DELETE INACTIVE INSTITUTIONS'
PRINT 'Review the lists above and then uncomment deletion lines below'
PRINT '========================================='

-- Step 3: DELETE inactive institutions (UNCOMMENT TO EXECUTE)
/*
DELETE FROM AAP_Student_Count_Monthly 
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
AND SchoolID IN (
    SELECT SI.SchoolID 
    FROM SchoolInfo SI 
    WHERE ISNULL(SI.IS_ServiceChargeActive, 0) = 0
)

DELETE FROM AAP_StudentClass_Count_Monthly 
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
AND SchoolID IN (
    SELECT SI.SchoolID 
    FROM SchoolInfo SI 
    WHERE ISNULL(SI.IS_ServiceChargeActive, 0) = 0
)

PRINT '? Deleted inactive institutions from March 2026'
*/

-- After deletion, verify
/*
PRINT ''
PRINT 'AFTER DELETION - Remaining institutions:'
SELECT 
    SI.SchoolID,
    SI.SchoolName,
    SCM.Active_Student
FROM AAP_Student_Count_Monthly SCM
INNER JOIN SchoolInfo SI ON SCM.SchoolID = SI.SchoolID
WHERE MONTH(SCM.Month) = 3 AND YEAR(SCM.Month) = 2026
ORDER BY SI.SchoolID
*/
