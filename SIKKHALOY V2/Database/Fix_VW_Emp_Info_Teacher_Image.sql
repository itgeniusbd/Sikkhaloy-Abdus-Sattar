-- ============================================================
-- FIX: VW_Emp_Info recreate with Teacher & Staff_Info JOINs
-- Problem : Teacher images missing on server because the view
--           was accidentally dropped and recreated without
--           joining Teacher/Staff_Info tables. Image column
--           was only reading Employee_Info.Image (always NULL
--           for Teachers whose images are stored in Teacher table).
-- Run this : on the LIVE SERVER SQL database (SSMS or Azure portal)
-- ============================================================

PRINT 'Recreating VW_Emp_Info with Teacher + Staff_Info JOINs...';

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'VW_Emp_Info')
BEGIN
    DROP VIEW VW_Emp_Info;
    PRINT 'Old VW_Emp_Info dropped.';
END
GO

CREATE VIEW VW_Emp_Info AS
SELECT
    ei.EmployeeID,
    ei.SchoolID,
    ei.RegistrationID,
    COALESCE(t.FirstName,   si.FirstName)   AS FirstName,
    COALESCE(t.LastName,    si.LastName)    AS LastName,
    COALESCE(t.FatherName,  si.FatherName)  AS FatherName,
    COALESCE(t.Designation, si.Designation) AS Designation,
    ei.EmployeeType,
    ei.Permanent_Temporary,
    COALESCE(t.Phone,  si.Phone)            AS Phone,
    ei.Bank_AccNo,
    ei.Salary,
    ei.Job_Status,
    ei.DeviceID,
    ei.RFID,
    ei.Work_Time_Basis,
    ei.Time_Basis_Type,
    -- Teacher images are stored in Teacher.Image
    -- Staff   images are stored in Staff_Info.Image
    COALESCE(t.Image, si.Image)             AS Image,
    ei.ID,
    ei.Employee_Payorder_NameID,
    ei.SubCategoryID,
    sc.SubCategoryName
FROM Employee_Info ei
LEFT JOIN Teacher              t  ON ei.EmployeeID = t.EmployeeID
LEFT JOIN Staff_Info           si ON ei.EmployeeID = si.EmployeeID
LEFT JOIN Employee_SubCategory sc ON ei.SubCategoryID = sc.SubCategoryID;
GO

PRINT 'VW_Emp_Info recreated successfully.';

-- ?? Quick verification ??????????????????????????????????????
SELECT
    ei_type.EmployeeType,
    COUNT(*)                                    AS TotalEmployees,
    SUM(CASE WHEN v.Image IS NOT NULL THEN 1 ELSE 0 END) AS WithImage,
    SUM(CASE WHEN v.Image IS NULL     THEN 1 ELSE 0 END) AS WithoutImage
FROM VW_Emp_Info v
JOIN Employee_Info ei_type ON v.EmployeeID = ei_type.EmployeeID
WHERE v.Job_Status = 'Active'
GROUP BY ei_type.EmployeeType;
GO
