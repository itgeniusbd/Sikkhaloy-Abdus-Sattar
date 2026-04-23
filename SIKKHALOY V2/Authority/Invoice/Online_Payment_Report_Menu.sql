-- Run this script to add "Online Payment Report" to the Authority navigation menu
-- This adds the page under the same category as "Paid Invoice" (SIKKHALOY INVOICE)

-- Step 1: Find the correct LinkCategoryID for "SIKKHALOY INVOICE"
-- SELECT * FROM Authority_Link_Category WHERE Category LIKE '%Invoice%'

-- Step 2: Find the SubCategoryID (if any) used by Paid Invoice
-- SELECT * FROM Authority_Link_Pages WHERE PageURL LIKE '%Paid_Invoice%'

-- Step 3: Insert the new page link
-- Replace @LinkCategoryID and @SubCategoryID with actual values from steps above

DECLARE @LinkCategoryID INT;
DECLARE @SubCategoryID  INT;
DECLARE @MaxAscending   INT;

-- Get LinkCategoryID for the Invoice category (same as Paid Invoice page)
SELECT @LinkCategoryID = LinkCategoryID
FROM Authority_Link_Pages
WHERE PageURL LIKE '%Invoice/Paid_Invoice%';

-- Get SubCategoryID (if Paid Invoice is under a sub-category)
SELECT @SubCategoryID = SubCategoryID
FROM Authority_Link_Pages
WHERE PageURL LIKE '%Invoice/Paid_Invoice%';

-- Get next Ascending value
SELECT @MaxAscending = ISNULL(MAX(Ascending), 0) + 1
FROM Authority_Link_Pages
WHERE LinkCategoryID = @LinkCategoryID;

-- Insert the new page
INSERT INTO Authority_Link_Pages (LinkCategoryID, SubCategoryID, PageTitle, PageURL, Ascending)
VALUES (
    @LinkCategoryID,
    @SubCategoryID,
    'Online Payment Report',
    '~/Authority/Invoice/Online_Payment_Report.aspx',
    @MaxAscending
);

PRINT 'Online Payment Report menu item added successfully!';
PRINT 'LinkCategoryID: ' + CAST(ISNULL(@LinkCategoryID, 0) AS VARCHAR);
PRINT 'SubCategoryID: ' + CAST(ISNULL(@SubCategoryID, 0) AS VARCHAR);

-- Verify
SELECT * FROM Authority_Link_Pages WHERE PageURL LIKE '%Online_Payment_Report%';
