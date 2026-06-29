-- Client SMS sidebar link (same placement as SMS Setting under Basic Option)

-- Run against Education database



DECLARE @LinkCategoryID INT;

DECLARE @SubCategoryID  INT;

DECLARE @Ascending      INT;

DECLARE @SmsAscending   INT;



SELECT TOP 1

    @LinkCategoryID = LinkCategoryID,

    @SubCategoryID = SubCategoryID,

    @SmsAscending = Ascending

FROM Authority_Link_Pages

WHERE PageURL LIKE '%SmsSetting%';



IF @LinkCategoryID IS NULL

BEGIN

    SELECT TOP 1 @LinkCategoryID = LinkCategoryID

    FROM Authority_Link_Category

    WHERE Category LIKE '%Basic%';



    SET @SubCategoryID = NULL;

    SET @SmsAscending = 0;

END



SET @Ascending = ISNULL(@SmsAscending, 0) + 1;



IF EXISTS (SELECT 1 FROM Authority_Link_Pages WHERE PageURL LIKE '%Bulk_Client_SMS%')

BEGIN

    UPDATE Authority_Link_Pages

    SET LinkCategoryID = @LinkCategoryID,

        SubCategoryID = @SubCategoryID,

        PageTitle = N'Client SMS',

        PageURL = N'~/Authority/Bulk_Client_SMS.aspx',

        Ascending = @Ascending

    WHERE PageURL LIKE '%Bulk_Client_SMS%';



    PRINT 'Client SMS menu item updated (SubCategoryID aligned with SMS Setting).';

END

ELSE

BEGIN

    INSERT INTO Authority_Link_Pages (LinkCategoryID, SubCategoryID, PageTitle, PageURL, Ascending)

    VALUES (

        @LinkCategoryID,

        @SubCategoryID,

        N'Client SMS',

        N'~/Authority/Bulk_Client_SMS.aspx',

        @Ascending

    );



    PRINT 'Client SMS menu item added successfully.';

END



SELECT * FROM Authority_Link_Pages WHERE PageURL LIKE '%Bulk_Client_SMS%';

