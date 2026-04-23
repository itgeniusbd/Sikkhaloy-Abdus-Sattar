-- Add SMS Recharge page to navigation menu
-- Run this script once to add the SMS Recharge link to the SMS section navigation

-- First, find the SMS category LinkCategoryID
-- (Adjust the Category name if it differs in your database)
DECLARE @SMSCategoryID INT
SELECT @SMSCategoryID = LinkCategoryID 
FROM Link_Category 
WHERE Category = N'SMS'

IF @SMSCategoryID IS NOT NULL
BEGIN
    -- Check if the link already exists
    IF NOT EXISTS (
        SELECT 1 FROM Link_Pages 
        WHERE PageURL = N'~/SMS/SMS_Recharge.aspx'
    )
    BEGIN
        -- Get max Ascending value in this category
        DECLARE @MaxAsc INT
        SELECT @MaxAsc = ISNULL(MAX(Ascending), 0) 
        FROM Link_Pages 
        WHERE LinkCategoryID = @SMSCategoryID

        INSERT INTO Link_Pages (LinkCategoryID, SubCategoryID, PageTitle, PageURL, Ascending)
        VALUES (@SMSCategoryID, NULL, N'SMS Recharge', N'~/SMS/SMS_Recharge.aspx', @MaxAsc + 1)

        PRINT 'SMS Recharge link added to navigation successfully.'
    END
    ELSE
    BEGIN
        PRINT 'SMS Recharge link already exists.'
    END
END
ELSE
BEGIN
    PRINT 'SMS category not found. Check Link_Category table for the correct category name.'
    -- List available categories for reference:
    SELECT LinkCategoryID, Category FROM Link_Category ORDER BY Ascending
END
