-- ========================================
-- QUICK FIX: Assign DeviceID 188 to yourself
-- SchoolID: 1012
-- ========================================

PRINT '=========================================='
PRINT 'Quick Fix for DeviceID 188 Assignment'
PRINT '=========================================='
PRINT ''

-- Option 1: Find your employee record and update
PRINT 'OPTION 1: If you already exist in Employee_Info table'
PRINT '------------------------------------------------------'
PRINT ''
PRINT 'Step 1: Find yourself by name or phone:'
PRINT ''

-- Search by partial name (modify as needed)
SELECT 
    EmployeeID,
    FirstName,
    LastName,
    DeviceID AS CurrentDeviceID,
    SchoolID,
    Job_Status,
    Phone
FROM VW_Emp_Info
WHERE SchoolID = 1012
  AND Job_Status = 'Active'
  AND (
      FirstName LIKE '%????? ???%'  -- Replace with your name
      OR LastName LIKE '%????? ???%'
      OR Phone LIKE '%????? ???%'    -- Replace with your phone
  )

PRINT ''
PRINT 'If you found yourself above, copy your EmployeeID and run:'
PRINT ''
PRINT '  -- Replace YOUR_EMPLOYEE_ID below'
PRINT '  UPDATE Employee_Info'
PRINT '  SET DeviceID = 188'
PRINT '  WHERE EmployeeID = YOUR_EMPLOYEE_ID'
PRINT '  AND SchoolID = 1012'
PRINT ''
PRINT ''

-- Option 2: Show any employee from SchoolID 1012 to modify
PRINT 'OPTION 2: Pick any existing employee and assign DeviceID 188'
PRINT '--------------------------------------------------------------'
PRINT ''

SELECT TOP 10
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    SchoolID,
    Job_Status,
    RegistrationID
FROM VW_Emp_Info
WHERE SchoolID = 1012
  AND Job_Status = 'Active'
ORDER BY EmployeeID

PRINT ''
PRINT 'Pick one EmployeeID from above and run:'
PRINT ''
PRINT '  UPDATE Employee_Info'
PRINT '  SET DeviceID = 188'
PRINT '  WHERE EmployeeID = [CHOSEN_EMPLOYEE_ID]'
PRINT ''
PRINT ''

-- Option 3: Create new test employee
PRINT 'OPTION 3: Create new employee with DeviceID 188 (for testing)'
PRINT '---------------------------------------------------------------'
PRINT ''
PRINT 'This will add MUHAMMAD KAZI ABDUS SATTI as employee:'
PRINT ''
PRINT '-- Uncomment and run this:'
PRINT '/*'
PRINT 'DECLARE @NewEmployeeID INT'
PRINT ''
PRINT 'INSERT INTO Employee_Info ('
PRINT '    SchoolID, RegistrationID, FirstName, LastName, '
PRINT '    DeviceID, Job_Status, CreatedDate'
PRINT ') VALUES ('
PRINT '    1012,              -- Your SchoolID'
PRINT '    1012,              -- Your RegistrationID'
PRINT '    ''MUHAMMAD KAZI'',  -- First Name'
PRINT '    ''ABDUS SATTI'',    -- Last Name'
PRINT '    188,               -- DeviceID from device'
PRINT '    ''Active'',         -- Job Status'
PRINT '    GETDATE()          -- Created Date'
PRINT ')'
PRINT ''
PRINT 'SET @NewEmployeeID = SCOPE_IDENTITY()'
PRINT ''
PRINT 'PRINT ''Employee created with ID: '' + CAST(@NewEmployeeID AS VARCHAR(10))'
PRINT '*/'
PRINT ''
PRINT ''

-- Verify current situation
PRINT '=========================================='
PRINT 'CURRENT SITUATION:'
PRINT '=========================================='
PRINT ''
PRINT 'Employees with DeviceID 188 in SchoolID 1012:'

IF EXISTS (SELECT 1 FROM VW_Emp_Info WHERE DeviceID = 188 AND SchoolID = 1012)
BEGIN
    SELECT 
        EmployeeID,
        FirstName + ' ' + LastName AS FullName,
        DeviceID,
        SchoolID
    FROM VW_Emp_Info
    WHERE DeviceID = 188 
      AND SchoolID = 1012
END
ELSE
BEGIN
    PRINT '  ?? NONE - This is why attendance is not working!'
END

PRINT ''
PRINT 'Employees with DeviceID 188 in OTHER schools:'
SELECT 
    EmployeeID,
    FirstName + ' ' + LastName AS FullName,
    DeviceID,
    SchoolID,
    (SELECT SchoolName FROM SchoolInfo WHERE SchoolID = VW_Emp_Info.SchoolID) AS SchoolName
FROM VW_Emp_Info
WHERE DeviceID = 188 
  AND SchoolID != 1012

PRINT ''
PRINT '=========================================='
PRINT 'RECOMMENDED ACTION:'
PRINT '=========================================='
PRINT ''
PRINT '1. Choose ONE option from above'
PRINT '2. Run the UPDATE or INSERT query'
PRINT '3. Verify by running this:'
PRINT ''
PRINT '   SELECT * FROM VW_Emp_Info '
PRINT '   WHERE DeviceID = 188 AND SchoolID = 1012'
PRINT ''
PRINT '4. Then punch on device again'
PRINT '5. Check dashboard - attendance should appear!'
PRINT ''
PRINT '=========================================='
