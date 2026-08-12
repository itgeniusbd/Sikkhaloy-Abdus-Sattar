-- Add Authority menu: Reset Institution Data
-- Place near Institution Details / User Info under Institutions category

DECLARE @LinkCategoryID INT;
DECLARE @SubCategoryID  INT;
DECLARE @Ascending      INT;

SELECT TOP 1
    @LinkCategoryID = LinkCategoryID,
    @SubCategoryID = SubCategoryID,
    @Ascending = Ascending
FROM Authority_Link_Pages
WHERE PageURL LIKE '%Institution_Details%' OR PageURL LIKE '%UserInfo%';

IF @LinkCategoryID IS NULL
BEGIN
    SELECT TOP 1 @LinkCategoryID = LinkCategoryID
    FROM Authority_Link_Category
    WHERE Category LIKE '%Institution%' OR Category LIKE '%Basic%';

    SET @SubCategoryID = NULL;
    SET @Ascending = 0;
END

SET @Ascending = ISNULL(@Ascending, 0) + 1;

IF EXISTS (SELECT 1 FROM Authority_Link_Pages WHERE PageURL LIKE '%Reset_Institution_Data%')
BEGIN
    UPDATE Authority_Link_Pages
    SET LinkCategoryID = @LinkCategoryID,
        SubCategoryID = @SubCategoryID,
        PageTitle = N'Reset Institution Data',
        PageURL = N'~/Authority/Institutions/Reset_Institution_Data.aspx',
        Ascending = @Ascending
    WHERE PageURL LIKE '%Reset_Institution_Data%';

    PRINT 'Reset Institution Data menu updated.';
END
ELSE
BEGIN
    INSERT INTO Authority_Link_Pages (LinkCategoryID, SubCategoryID, PageTitle, PageURL, Ascending)
    VALUES (
        @LinkCategoryID,
        @SubCategoryID,
        N'Reset Institution Data',
        N'~/Authority/Institutions/Reset_Institution_Data.aspx',
        @Ascending
    );

    PRINT 'Reset Institution Data menu added.';
END

SELECT * FROM Authority_Link_Pages WHERE PageURL LIKE '%Reset_Institution_Data%';
