-- Script to find duplicate phone numbers in CommitteeMember table
-- Run this to identify members with duplicate phone numbers

-- Find duplicate phone numbers
SELECT 
    LTRIM(RTRIM(SmsNumber)) AS PhoneNumber,
    COUNT(*) AS DuplicateCount,
    STRING_AGG(CAST(CommitteeMemberId AS VARCHAR), ', ') AS MemberIds,
    STRING_AGG(MemberName, ', ') AS MemberNames
FROM CommitteeMember
WHERE SmsNumber IS NOT NULL AND SmsNumber != ''
GROUP BY LTRIM(RTRIM(SmsNumber))
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- If you're using SQL Server 2016 or earlier (doesn't support STRING_AGG), use this instead:
/*
SELECT 
    LTRIM(RTRIM(SmsNumber)) AS PhoneNumber,
    COUNT(*) AS DuplicateCount
FROM CommitteeMember
WHERE SmsNumber IS NOT NULL AND SmsNumber != ''
GROUP BY LTRIM(RTRIM(SmsNumber))
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- Then for each duplicate, see the details:
SELECT CommitteeMemberId, MemberName, SmsNumber, Address, InsertDate
FROM CommitteeMember
WHERE LTRIM(RTRIM(SmsNumber)) = 'PUT_PHONE_NUMBER_HERE'
ORDER BY InsertDate;
*/

-- Optional: Add a unique constraint to prevent future duplicates at database level
-- UNCOMMENT the lines below if you want to enforce uniqueness in database
/*
-- First, clean up duplicates manually, then run:
ALTER TABLE CommitteeMember 
ADD CONSTRAINT UQ_CommitteeMember_Phone_School 
UNIQUE (SchoolID, SmsNumber);
*/
