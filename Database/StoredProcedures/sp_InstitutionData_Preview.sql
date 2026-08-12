-- Preview row counts that would be affected by SESSION / FULL / PURGE
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[sp_InstitutionData_Preview]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_InstitutionData_Preview];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE [dbo].[sp_InstitutionData_Preview]
    @SchoolID        INT,
    @Mode            VARCHAR(20),          -- SESSION / FULL / PURGE
    @EducationYearID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    SET @Mode = UPPER(LTRIM(RTRIM(ISNULL(@Mode, ''))));

    IF @SchoolID IS NULL OR @SchoolID <= 0
    BEGIN
        RAISERROR(N'Invalid SchoolID.', 16, 1);
        RETURN;
    END

    IF @Mode NOT IN ('SESSION', 'FULL', 'PURGE')
    BEGIN
        RAISERROR(N'Mode must be SESSION, FULL or PURGE.', 16, 1);
        RETURN;
    END

    IF @Mode = 'SESSION' AND (@EducationYearID IS NULL OR @EducationYearID <= 0)
    BEGIN
        RAISERROR(N'EducationYearID required for SESSION.', 16, 1);
        RETURN;
    END

    DECLARE @SchoolName NVARCHAR(500) =
        (SELECT TOP 1 SchoolName FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID);

    DECLARE @ActiveUsers INT = 0;
    IF OBJECT_ID(N'dbo.User_Active_Sessions', N'U') IS NOT NULL
        SELECT @ActiveUsers = COUNT(*)
        FROM dbo.User_Active_Sessions
        WHERE SchoolID = @SchoolID
          AND LastActivity >= DATEADD(MINUTE, -30, GETDATE());

    DECLARE @Counts TABLE (
        TableName SYSNAME NOT NULL,
        RowCnt    BIGINT NOT NULL
    );

    DECLARE @tbl SYSNAME, @sql NVARCHAR(MAX), @cnt BIGINT;

    DECLARE @Tables TABLE (Ord INT IDENTITY(1,1), TableName SYSNAME);
    INSERT INTO @Tables (TableName) VALUES
    (N'StudentsClass'), (N'Student'), (N'StudentRecord'),
    (N'Attendance_Record'), (N'Attendance_Student'), (N'Attendance_Leave'),
    (N'Income_PayOrder'), (N'Income_PaymentRecord'), (N'Income_MoneyReceipt'),
    (N'Exam_Obtain_Marks'), (N'Exam_Result_of_Student'), (N'Exam_Result_of_Subject'),
    (N'Exam_Name'), (N'Teacher'), (N'Subject'), (N'CreateClass'),
    (N'Account_Log'), (N'AccountIN_Record'), (N'AccountOUT_Record'),
    (N'Expenditure'), (N'Extra_Income'),
    (N'SMS_Send_Record'), (N'SMS_OtherInfo'),
    (N'Employee_Info'), (N'Employee_Payorder'),
    (N'Education_Year'), (N'Registration'), (N'AST'), (N'SMS'), (N'SchoolInfo');

    DECLARE @i INT = 1, @max INT;
    SELECT @max = MAX(Ord) FROM @Tables;

    WHILE @i <= @max
    BEGIN
        SELECT @tbl = TableName FROM @Tables WHERE Ord = @i;

        IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.' + @tbl, 'SchoolID') IS NOT NULL
        BEGIN
            IF @Mode = 'SESSION'
               AND COL_LENGTH(N'dbo.' + @tbl, 'EducationYearID') IS NOT NULL
               AND @tbl NOT IN (N'SchoolInfo', N'Registration', N'AST', N'SMS', N'Teacher', N'Subject', N'CreateClass', N'Student')
            BEGIN
                SET @sql = N'SELECT @c = COUNT_BIG(*) FROM dbo.' + QUOTENAME(@tbl) +
                           N' WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID';
                BEGIN TRY
                    EXEC sp_executesql @sql,
                        N'@SchoolID INT, @EducationYearID INT, @c BIGINT OUTPUT',
                        @SchoolID = @SchoolID, @EducationYearID = @EducationYearID, @c = @cnt OUTPUT;
                    IF @cnt > 0 INSERT INTO @Counts VALUES (@tbl, @cnt);
                END TRY BEGIN CATCH END CATCH;
            END
            ELSE IF @Mode IN ('FULL', 'PURGE')
            BEGIN
                -- FULL keeps SchoolInfo/Registration(Admin)/SMS/one year — still show counts of operational tables
                IF @Mode = 'FULL' AND @tbl IN (N'SchoolInfo')
                BEGIN
                    SET @i = @i + 1;
                    CONTINUE;
                END

                SET @sql = N'SELECT @c = COUNT_BIG(*) FROM dbo.' + QUOTENAME(@tbl) +
                           N' WHERE SchoolID = @SchoolID';
                BEGIN TRY
                    EXEC sp_executesql @sql,
                        N'@SchoolID INT, @c BIGINT OUTPUT',
                        @SchoolID = @SchoolID, @c = @cnt OUTPUT;
                    IF @cnt > 0 INSERT INTO @Counts VALUES (@tbl, @cnt);
                END TRY BEGIN CATCH END CATCH;
            END
        END

        SET @i = @i + 1;
    END

    -- Membership users for PURGE
    IF @Mode = 'PURGE'
    BEGIN
        DECLARE @mem INT = 0;
        SELECT @mem = COUNT(*)
        FROM dbo.aspnet_Users AU
        WHERE EXISTS (
            SELECT 1 FROM dbo.Registration R
            WHERE R.SchoolID = @SchoolID AND LOWER(LTRIM(RTRIM(R.UserName))) = LOWER(LTRIM(RTRIM(AU.UserName)))
        )
        OR EXISTS (
            SELECT 1 FROM dbo.SchoolInfo S
            WHERE S.SchoolID = @SchoolID AND LOWER(LTRIM(RTRIM(S.UserName))) = LOWER(LTRIM(RTRIM(AU.UserName)))
        );
        IF @mem > 0 INSERT INTO @Counts VALUES (N'aspnet_Users (login)', @mem);
    END

    SELECT
        N'Preview' AS Status,
        @SchoolID AS SchoolID,
        ISNULL(@SchoolName, N'') AS SchoolName,
        @Mode AS Mode,
        @EducationYearID AS EducationYearID,
        @ActiveUsers AS ActiveUsers,
        (SELECT ISNULL(SUM(RowCnt), 0) FROM @Counts) AS TotalRows;

    SELECT TableName, RowCnt
    FROM @Counts
    ORDER BY RowCnt DESC, TableName;
END
GO
