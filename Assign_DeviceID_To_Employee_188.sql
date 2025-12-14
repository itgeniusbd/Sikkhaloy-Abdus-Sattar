-- ========================================
-- Configure Employee DeviceID for User 188
-- ========================================

-- 1. Check if employee exists
SELECT 
    EmployeeID,
    FirstName,
    LastName,
    DeviceID,
    SchoolID,
    Job_Status
FROM VW_Emp_Info
WHERE FirstName LIKE '%MUHAMMAD%'
  AND LastName LIKE '%SATTI%'
  OR EmployeeID = 188

-- 2. If employee found, check their table
-- VW_Emp_Info is a VIEW, we need to update base table
-- Usually it's Employee_Info or similar

-- Find the base table
SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    SchoolID
FROM Employee_Info
WHERE FirstName LIKE '%MUHAMMAD%' 
  OR EmployeeID = 188

-- 3. Update DeviceID for User 188
-- IMPORTANT: Change EmployeeID to match actual employee
UPDATE Employee_Info
SET DeviceID = 188
WHERE EmployeeID = 188  -- Change this to actual EmployeeID if different

-- Or if you need to find by name:
-- UPDATE Employee_Info
-- SET DeviceID = 188
-- WHERE FirstName LIKE '%MUHAMMAD%' AND LastName LIKE '%SATTI%'

-- 4. Verify the update
SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    SchoolID,
    Job_Status
FROM VW_Emp_Info
WHERE DeviceID = 188

-- 5. Check if more employees need DeviceID
SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS Name,
    DeviceID,
    SchoolID
FROM VW_Emp_Info
WHERE SchoolID = 1012  -- Change to your SchoolID
  AND Job_Status = 'Active'
  AND DeviceID IS NULL
ORDER BY EmployeeID

PRINT 'INSTRUCTIONS:'
PRINT '1. Find the employee who punched (User 188)'
PRINT '2. Set their DeviceID = 188 in Employee_Info table'
PRINT '3. Or create new employee with DeviceID = 188'
PRINT '4. Then test punch again'
