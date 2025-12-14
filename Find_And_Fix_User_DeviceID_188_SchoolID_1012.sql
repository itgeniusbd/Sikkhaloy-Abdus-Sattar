-- ========================================
-- Find MUHAMMAD KAZI ABDUS SATTI in SchoolID 1012
-- and assign DeviceID = 188
-- ========================================

PRINT '=========================================='
PRINT 'Finding User: MUHAMMAD KAZI ABDUS SATTI'
PRINT 'SchoolID: 1012'
PRINT 'Device Punch ID: 188'
PRINT '=========================================='
PRINT ''

-- 1. Check if user exists in SchoolID 1012
PRINT 'Step 1: Searching for MUHAMMAD KAZI ABDUS SATTI in SchoolID 1012...'
PRINT ''

SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID AS CurrentDeviceID,
    SchoolID,
    Job_Status,
    RegistrationID
FROM VW_Emp_Info
WHERE SchoolID = 1012
  AND (
      FirstName LIKE '%MUHAMMAD%' 
      OR FirstName LIKE '%KAZI%'
      OR LastName LIKE '%SATTI%'
      OR LastName LIKE '%ABDUS%'
      OR (FirstName + ' ' + LastName) LIKE '%MUHAMMAD%KAZI%'
  )

-- 2. If not found, search more broadly
PRINT ''
PRINT 'Step 2: Searching in all SchoolIDs...'
PRINT ''

SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    SchoolID,
    Job_Status
FROM VW_Emp_Info
WHERE FirstName LIKE '%MUHAMMAD%' 
  AND LastName LIKE '%SATTI%'

-- 3. Check who has DeviceID = 188 in SchoolID 1012
PRINT ''
PRINT 'Step 3: Who has DeviceID = 188 in SchoolID 1012?'
PRINT ''

SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    SchoolID,
    Job_Status
FROM VW_Emp_Info
WHERE DeviceID = 188
  AND SchoolID = 1012

-- If nobody, show message
IF NOT EXISTS (SELECT 1 FROM VW_Emp_Info WHERE DeviceID = 188 AND SchoolID = 1012)
BEGIN
    PRINT '  ?? NO employee with DeviceID = 188 in SchoolID 1012'
    PRINT ''
END

-- 4. Show all employees in SchoolID 1012 with DeviceID
PRINT ''
PRINT 'Step 4: All employees in SchoolID 1012 with DeviceID assigned:'
PRINT ''

SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    Job_Status,
    Phone,
    RegistrationID
FROM VW_Emp_Info
WHERE SchoolID = 1012
  AND DeviceID IS NOT NULL
  AND Job_Status = 'Active'
ORDER BY DeviceID

-- 5. Show employees WITHOUT DeviceID in SchoolID 1012
PRINT ''
PRINT 'Step 5: Employees WITHOUT DeviceID in SchoolID 1012 (to assign):'
PRINT ''

SELECT TOP 20
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    Job_Status,
    Phone,
    RegistrationID
FROM VW_Emp_Info
WHERE SchoolID = 1012
  AND DeviceID IS NULL
  AND Job_Status = 'Active'
ORDER BY EmployeeID

PRINT ''
PRINT '=========================================='
PRINT 'INSTRUCTIONS:'
PRINT '=========================================='
PRINT ''
PRINT 'Option 1: If you found yourself in the results above'
PRINT '---------------------------------------------------------'
PRINT '  Run this (replace YOUR_EMPLOYEE_ID):'
PRINT ''
PRINT '  UPDATE Employee_Info'
PRINT '  SET DeviceID = 188'
PRINT '  WHERE EmployeeID = YOUR_EMPLOYEE_ID AND SchoolID = 1012'
PRINT ''
PRINT ''
PRINT 'Option 2: If you are NOT in the database'
PRINT '---------------------------------------------------------'
PRINT '  You need to be added as an employee first in EDUCATION.COM system'
PRINT '  Then assign DeviceID = 188 to your record'
PRINT ''
PRINT ''
PRINT 'Option 3: Create new employee for testing'
PRINT '---------------------------------------------------------'
PRINT '  This will create a test employee with DeviceID = 188'
PRINT '  (Uncomment and run the INSERT below)'
PRINT ''
PRINT '  -- INSERT INTO Employee_Info'
PRINT '  -- (SchoolID, FirstName, LastName, DeviceID, Job_Status, RegistrationID)'
PRINT '  -- VALUES'
PRINT '  -- (1012, ''MUHAMMAD KAZI'', ''ABDUS SATTI'', 188, ''Active'', 1012)'
PRINT ''
PRINT '=========================================='
