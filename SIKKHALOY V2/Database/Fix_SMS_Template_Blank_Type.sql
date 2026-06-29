-- Fix templates saved with blank TemplateType (dropdown postback bug)
-- Run once per school if templates show Active but SMS uses default message.

-- School 1012 (Sikkhaloy School & Madrasha)
UPDATE SMS_Template SET TemplateType = 'Passed', UpdatedDate = GETDATE()
WHERE TemplateID = 1064 AND SchoolID = 1012 AND LTRIM(RTRIM(ISNULL(TemplateType, ''))) = '';

UPDATE SMS_Template SET TemplateType = 'Failed', UpdatedDate = GETDATE()
WHERE TemplateID = 1065 AND SchoolID = 1012 AND LTRIM(RTRIM(ISNULL(TemplateType, ''))) = '';

UPDATE SMS_Template SET TemplateType = 'DonorDue', UpdatedDate = GETDATE()
WHERE TemplateID = 1067 AND SchoolID = 1012 AND LTRIM(RTRIM(ISNULL(TemplateType, ''))) = '';

UPDATE SMS_Template SET TemplateType = 'DonorPayment', UpdatedDate = GETDATE()
WHERE TemplateID = 1068 AND SchoolID = 1012 AND LTRIM(RTRIM(ISNULL(TemplateType, ''))) = '';

-- Generic fix: infer type from message placeholders where type is blank
UPDATE SMS_Template SET TemplateType = 'DonorPayment', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Donor'
  AND (MessageTemplate LIKE '%{ReceiptNo}%' OR MessageTemplate LIKE '%{Amount}%');

UPDATE SMS_Template SET TemplateType = 'DonorDue', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Donor'
  AND MessageTemplate LIKE '%{TotalDue}%';

UPDATE SMS_Template SET TemplateType = 'Due', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Due';

-- Attendance: infer type from placeholders where type is blank
UPDATE SMS_Template SET TemplateType = 'Exit', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Attendance'
  AND MessageTemplate LIKE '%{ExitTime}%';

UPDATE SMS_Template SET TemplateType = 'LateAbs', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Attendance'
  AND MessageTemplate LIKE '%{LateMinutes}%'
  AND (MessageTemplate LIKE '%Absent%' OR MessageTemplate LIKE N'%অনুপস্থিত%');

UPDATE SMS_Template SET TemplateType = 'Late', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Attendance'
  AND MessageTemplate LIKE '%{LateMinutes}%';

UPDATE SMS_Template SET TemplateType = 'Entry', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Attendance'
  AND MessageTemplate LIKE '%{EntryTime}%';

UPDATE SMS_Template SET TemplateType = 'Absent', UpdatedDate = GETDATE()
WHERE LTRIM(RTRIM(ISNULL(TemplateType, ''))) = ''
  AND TemplateCategory = 'Attendance'
  AND (MessageTemplate LIKE '%Absent%' OR MessageTemplate LIKE N'%অনুপস্থিত%');
