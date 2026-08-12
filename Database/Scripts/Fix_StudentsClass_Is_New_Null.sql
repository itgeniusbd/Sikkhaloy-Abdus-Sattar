/*
================================================================================
  Fix StudentsClass.Is_New NULL (Find / Pay Order hide these students)

  Why:
    Find_Students.aspx & Pay_Order.aspx filter:
      StudentsClass.Is_New LIKE @Is_New   (All Student = '%')
    In SQL Server:  NULL LIKE '%'  → UNKNOWN → row excluded

    Class Based Students_List does NOT filter Is_New → student still visible.

  Cause:
    Tr_StudentsClass_Insert normally sets Is_New on INSERT.
    If that trigger was DISABLED during class change / re-admit, Is_New stays NULL.

  Trigger logic (mirrored here):
    Is_New = 1 if student has only 1 active class row
    Is_New = 0 if student has more than 1 active class row
    Active = Class_Status IS NULL OR Class_Status = N'Re-Admitted'
================================================================================
*/
USE [Edu];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SchoolID   INT = NULL;   -- NULL = all schools; e.g. 1316
DECLARE @DryRun     BIT = 1;
DECLARE @AutoCommit BIT = 0;

PRINT '=== Diagnose sample (School 1316 / ID 22024) ===';
SELECT
    s.SchoolID,
    s.ID,
    s.StudentsName,
    s.Status,
    sc.StudentClassID,
    sc.ClassID,
    cc.Class,
    sc.EducationYearID,
    sc.Is_New,
    sc.Class_Status,
    sc.SectionID,
    sc.SubjectGroupID,
    sc.ShiftID,
    CASE
        WHEN sc.Is_New IS NULL THEN N'MISSING_FROM_FIND_PAYORDER (Is_New NULL)'
        ELSE N'OK for Is_New filter'
    END AS FindPayOrder_Impact
FROM dbo.Student AS s
INNER JOIN dbo.StudentsClass AS sc
    ON s.StudentID = sc.StudentID
   AND s.SchoolID = sc.SchoolID
LEFT JOIN dbo.CreateClass AS cc
    ON sc.ClassID = cc.ClassID
WHERE s.SchoolID = 1316
  AND s.ID = N'22024';

PRINT '=== Count rows with Is_New NULL ===';
SELECT
    sc.SchoolID,
    COUNT(*) AS Null_Is_New_Rows
FROM dbo.StudentsClass AS sc
WHERE sc.Is_New IS NULL
  AND (@SchoolID IS NULL OR sc.SchoolID = @SchoolID)
  AND (sc.Class_Status IS NULL OR sc.Class_Status = N'Re-Admitted')
GROUP BY sc.SchoolID
ORDER BY Null_Is_New_Rows DESC;

SELECT
    COUNT(*) AS Total_Null_Is_New
FROM dbo.StudentsClass AS sc
WHERE sc.Is_New IS NULL
  AND (@SchoolID IS NULL OR sc.SchoolID = @SchoolID)
  AND (sc.Class_Status IS NULL OR sc.Class_Status = N'Re-Admitted');

IF @DryRun = 1
BEGIN
    PRINT '=== DRY RUN. Set @DryRun=0, @AutoCommit=1 to APPLY Is_New fix. ===';
    RETURN;
END

BEGIN TRAN;

;WITH ActiveClass AS (
    SELECT
        sc.StudentClassID,
        sc.StudentID,
        CAST(
            CASE
                WHEN COUNT(*) OVER (PARTITION BY sc.StudentID) > 1 THEN 0
                ELSE 1
            END AS bit
        ) AS New_Is_New
    FROM dbo.StudentsClass AS sc
    WHERE (sc.Class_Status IS NULL OR sc.Class_Status = N'Re-Admitted')
      AND (@SchoolID IS NULL OR sc.SchoolID = @SchoolID)
)
UPDATE sc
SET sc.Is_New = a.New_Is_New
FROM dbo.StudentsClass AS sc
INNER JOIN ActiveClass AS a
    ON a.StudentClassID = sc.StudentClassID
WHERE sc.Is_New IS NULL;

DECLARE @Updated INT = @@ROWCOUNT;

SELECT @Updated AS Updated_Rows;

SELECT
    s.SchoolID,
    s.ID,
    s.StudentsName,
    sc.Is_New,
    sc.Class_Status,
    cc.Class
FROM dbo.Student AS s
INNER JOIN dbo.StudentsClass AS sc
    ON s.StudentID = sc.StudentID
LEFT JOIN dbo.CreateClass AS cc
    ON sc.ClassID = cc.ClassID
WHERE s.SchoolID = 1316
  AND s.ID = N'22024';

IF @AutoCommit = 1
BEGIN
    COMMIT TRAN;
    PRINT '=== COMMITTED. Re-check Find / Pay Order for ID 22024. ===';
END
ELSE
BEGIN
    ROLLBACK TRAN;
    PRINT '=== ROLLED BACK. Re-run with @AutoCommit=1 to save. ===';
END
GO
