-- ============================================================
-- MIGRATION FIX: Employee SubCategory Feature
-- Run this script on the LIVE server database (sikkhaloy.com)
-- ============================================================

-- ?? STEP 1: Create Employee_SubCategory table ??????????????
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'Employee_SubCategory'
)
BEGIN
    CREATE TABLE Employee_SubCategory (
        SubCategoryID   INT IDENTITY(1,1) PRIMARY KEY,
        SchoolID        INT NOT NULL,
        EmployeeType    NVARCHAR(20) NOT NULL,
        SubCategoryName NVARCHAR(100) NOT NULL,
        CreateDate      DATETIME DEFAULT GETDATE()
    );
    PRINT 'Employee_SubCategory table created.';
END
ELSE
    PRINT 'Employee_SubCategory table already exists — skipped.';
GO

-- ?? STEP 2: Add SubCategoryID column to Employee_Info ??????
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee_Info' AND COLUMN_NAME = 'SubCategoryID'
)
BEGIN
    ALTER TABLE Employee_Info ADD SubCategoryID INT NULL;
    PRINT 'SubCategoryID column added to Employee_Info.';
END
ELSE
    PRINT 'SubCategoryID column already exists in Employee_Info — skipped.';
GO

-- ?? STEP 3: FK (safe) ??????????????????????????????????????
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employee_Info_SubCategory')
BEGIN
    ALTER TABLE Employee_Info
    ADD CONSTRAINT FK_Employee_Info_SubCategory
    FOREIGN KEY (SubCategoryID) REFERENCES Employee_SubCategory(SubCategoryID)
    ON DELETE SET NULL ON UPDATE CASCADE;
    PRINT 'Foreign key created.';
END
ELSE
    PRINT 'Foreign key already exists — skipped.';
GO

-- ?? STEP 4: Ensure Employee_Payorder_NameID exists ?????????
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee_Info' AND COLUMN_NAME = 'Employee_Payorder_NameID'
)
BEGIN
    ALTER TABLE Employee_Info ADD Employee_Payorder_NameID INT NULL;
    PRINT 'Employee_Payorder_NameID added.';
END
ELSE
    PRINT 'Employee_Payorder_NameID already exists — skipped.';
GO

-- ?? STEP 5: Recreate VW_Emp_Info ???????????????????????????
-- The view is a UNION of Teacher + Staff joined with Employee_Info
-- SubCategoryID & SubCategoryName added via LEFT JOIN Employee_SubCategory

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'VW_Emp_Info')
    DROP VIEW VW_Emp_Info;
GO

CREATE VIEW VW_Emp_Info AS
-- Teachers
SELECT
    ei.EmployeeID,
    ei.SchoolID,
    ei.RegistrationID,
    t.FirstName,
    t.LastName,
    t.Designation,
    ei.EmployeeType,
    ei.Permanent_Temporary,
    t.Phone,
    ei.Bank_AccNo,
    ei.Salary,
    ei.Job_Status,
    ei.DeviceID,
    ei.RFID,
    ei.Work_Time_Basis,
    ei.Time_Basis_Type,
    t.Image,
    ei.ID,
    ei.Employee_Payorder_NameID,
    ei.SubCategoryID,
    sc.SubCategoryName,
    t.FatherName
FROM Employee_Info ei
INNER JOIN Teacher t ON ei.EmployeeID = t.EmployeeID
LEFT JOIN Employee_SubCategory sc ON ei.SubCategoryID = sc.SubCategoryID

UNION ALL

-- Staff
SELECT
    ei.EmployeeID,
    ei.SchoolID,
    ei.RegistrationID,
    s.FirstName,
    s.LastName,
    s.Designation,
    ei.EmployeeType,
    ei.Permanent_Temporary,
    s.Phone,
    ei.Bank_AccNo,
    ei.Salary,
    ei.Job_Status,
    ei.DeviceID,
    ei.RFID,
    ei.Work_Time_Basis,
    ei.Time_Basis_Type,
    s.Image,
    ei.ID,
    ei.Employee_Payorder_NameID,
    ei.SubCategoryID,
    sc.SubCategoryName,
    s.FatherName
FROM Employee_Info ei
INNER JOIN Staff_Info s ON ei.EmployeeID = s.EmployeeID
LEFT JOIN Employee_SubCategory sc ON ei.SubCategoryID = sc.SubCategoryID;
GO

PRINT 'VW_Emp_Info recreated successfully.';
GO

-- ?? STEP 6: Verify ?????????????????????????????????????????
SELECT TOP 3 EmployeeID, FirstName, LastName, SubCategoryID, SubCategoryName
FROM VW_Emp_Info;
GO

PRINT '== Migration completed successfully ==';
GO


-- ?? STEP 1: Create Employee_SubCategory table ??????????????
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'Employee_SubCategory'
)
BEGIN
    CREATE TABLE Employee_SubCategory (
        SubCategoryID   INT IDENTITY(1,1) PRIMARY KEY,
        SchoolID        INT NOT NULL,
        EmployeeType    NVARCHAR(20) NOT NULL,   -- 'Teacher' or 'Staff'
        SubCategoryName NVARCHAR(100) NOT NULL,
        CreateDate      DATETIME DEFAULT GETDATE()
    );
    PRINT 'Employee_SubCategory table created.';
END
ELSE
    PRINT 'Employee_SubCategory table already exists — skipped.';
GO

-- ?? STEP 2: Add SubCategoryID column to Employee_Info ??????
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee_Info' AND COLUMN_NAME = 'SubCategoryID'
)
BEGIN
    ALTER TABLE Employee_Info
    ADD SubCategoryID INT NULL;
    PRINT 'SubCategoryID column added to Employee_Info.';
END
ELSE
    PRINT 'SubCategoryID column already exists in Employee_Info — skipped.';
GO

-- ?? STEP 3: Add FK (optional, safe) ????????????????????????
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Employee_Info_SubCategory'
)
BEGIN
    ALTER TABLE Employee_Info
    ADD CONSTRAINT FK_Employee_Info_SubCategory
    FOREIGN KEY (SubCategoryID) REFERENCES Employee_SubCategory(SubCategoryID)
    ON DELETE SET NULL ON UPDATE CASCADE;
    PRINT 'Foreign key FK_Employee_Info_SubCategory created.';
END
ELSE
    PRINT 'Foreign key already exists — skipped.';
GO

-- ?? STEP 3b: Ensure Employee_Payorder_NameID exists (needed for VW_Emp_Info) ??
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee_Info' AND COLUMN_NAME = 'Employee_Payorder_NameID'
)
BEGIN
    ALTER TABLE Employee_Info ADD Employee_Payorder_NameID INT NULL;
    PRINT 'Employee_Payorder_NameID column added to Employee_Info.';
END
ELSE
    PRINT 'Employee_Payorder_NameID already exists — skipped.';
GO

-- ?? STEP 4: Update VW_Emp_Info view ????????????????????????
-- Safely add SubCategoryID & SubCategoryName to existing view
-- NOTE: This recreates the view — column list matches existing usage across all pages

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'VW_Emp_Info')
    DROP VIEW VW_Emp_Info;
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
    -- Teacher image lives in Teacher table; Staff image lives in Staff_Info table
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

PRINT 'VW_Emp_Info view recreated with SubCategoryID and SubCategoryName.';

-- ?? STEP 5: Verify ?????????????????????????????????????????
SELECT 'Employee_SubCategory' AS TableName,
       COUNT(*) AS RowCount FROM Employee_SubCategory
UNION ALL
SELECT 'VW_Emp_Info columns added' AS TableName,
       COUNT(*) AS ColCount
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee_Info'
  AND COLUMN_NAME = 'SubCategoryID';
GO

PRINT '== Migration completed successfully ==';
GO
