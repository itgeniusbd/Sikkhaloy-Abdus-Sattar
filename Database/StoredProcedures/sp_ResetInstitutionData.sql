-- ============================================================
-- sp_ResetInstitutionData
-- FULL   : wipe operational data -> near new-signup state
-- SESSION: wipe one EducationYear / session data only
-- PURGE  : FULL wipe + delete SchoolInfo, logins, membership, SMS, years, invoices
-- ============================================================

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[sp_ResetInstitutionData]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[sp_ResetInstitutionData];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE [dbo].[sp_ResetInstitutionData]
    @SchoolID          INT,
    @Mode              VARCHAR(20),           -- 'FULL' or 'SESSION' or 'PURGE'
    @EducationYearID   INT = NULL,            -- required for SESSION
    @ConfirmSchoolID   INT,                   -- must equal @SchoolID
    @TotalRowsEstimate BIGINT = NULL,         -- from preview (for UI progress)
    @DeletedRows       INT = 0 OUTPUT,
    @Message           NVARCHAR(500) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET ANSI_NULLS ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET ARITHABORT ON;
    -- Must stay OFF: we intentionally skip FK failures and continue deleting.
    -- With XACT_ABORT ON, one failed DELETE dooms the whole transaction
    -- ("cannot be committed and cannot support operations that write to the log file").
    SET XACT_ABORT OFF;

    SET @DeletedRows = 0;
    SET @Message = NULL;
    SET @Mode = UPPER(LTRIM(RTRIM(ISNULL(@Mode, ''))));

    /* Live UI progress (polled by Reset_Institution_Data_API?action=progress) */
    DECLARE @HasProgress BIT = CASE WHEN OBJECT_ID(N'dbo.Institution_Reset_Progress', N'U') IS NOT NULL THEN 1 ELSE 0 END;

    IF @SchoolID IS NULL OR @SchoolID <= 0
    BEGIN
        SET @Message = N'Invalid SchoolID.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF @ConfirmSchoolID <> @SchoolID
    BEGIN
        SET @Message = N'Confirm SchoolID does not match.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID)
    BEGIN
        SET @Message = N'School not found.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF @Mode NOT IN ('FULL', 'SESSION', 'PURGE')
    BEGIN
        SET @Message = N'Mode must be FULL, SESSION or PURGE.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF @Mode = 'SESSION'
    BEGIN
        IF @EducationYearID IS NULL OR @EducationYearID <= 0
        BEGIN
            SET @Message = N'EducationYearID is required for SESSION mode.';
            RAISERROR(@Message, 16, 1);
            RETURN;
        END
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Education_Year
            WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
        )
        BEGIN
            SET @Message = N'Session not found for this school.';
            RAISERROR(@Message, 16, 1);
            RETURN;
        END
    END

    /* Start progress row for UI polling */
    IF @HasProgress = 1
    BEGIN
        DELETE FROM dbo.Institution_Reset_Progress WHERE SchoolID = @SchoolID;
        INSERT INTO dbo.Institution_Reset_Progress
            (SchoolID, Mode, EducationYearID, TotalRows, DeletedRows, Status, Message, UpdatedAt)
        VALUES
            (@SchoolID, @Mode, @EducationYearID, ISNULL(@TotalRowsEstimate, 0), 0, N'Running', NULL, SYSUTCDATETIME());
    END

    DECLARE @KeepEducationYearID INT = NULL;
    DECLARE @AdminRegistrationID INT = NULL;
    DECLARE @AdminUserName NVARCHAR(256) = NULL;
    DECLARE @Pass INT;
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @tbl SYSNAME;
    DECLARE @schema SYSNAME;
    DECLARE @rc INT;

    SELECT TOP 1
        @AdminRegistrationID = R.RegistrationID,
        @AdminUserName = R.UserName
    FROM dbo.Registration R
    WHERE R.SchoolID = @SchoolID AND R.Category = N'Admin'
    ORDER BY R.RegistrationID;

    IF @Mode = 'FULL'
    BEGIN
        SELECT TOP 1 @KeepEducationYearID = EducationYearID
        FROM dbo.Education_Year
        WHERE SchoolID = @SchoolID
        ORDER BY
            CASE WHEN Status = N'True' OR Status = '1' THEN 0 ELSE 1 END,
            CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
            EducationYearID DESC;
    END

    DECLARE @TriggersDisabled BIT = 0;
    DECLARE @ActiveUsers INT = 0;

    -- Live users on this school (last 30 minutes)
    IF OBJECT_ID(N'dbo.User_Active_Sessions', N'U') IS NOT NULL
    BEGIN
        SELECT @ActiveUsers = COUNT(*)
        FROM dbo.User_Active_Sessions
        WHERE SchoolID = @SchoolID
          AND LastActivity >= DATEADD(MINUTE, -30, GETDATE());
    END

    --------------------------------------------------------
    -- SESSION MODE: short locks, NO trigger disable, NO long TRAN
    -- (Trigger disable / long TRAN was hanging the whole server)
    --------------------------------------------------------
    IF @Mode = 'SESSION'
    BEGIN
        BEGIN TRY
            SET DEADLOCK_PRIORITY LOW;
            SET LOCK_TIMEOUT 15000; -- 15 sec wait then skip/retry path

            DECLARE @SessionDel TABLE (Ord INT IDENTITY(1,1), TableName SYSNAME);
            INSERT INTO @SessionDel (TableName) VALUES
            (N'Account_Log'),
            (N'Exam_Obtain_Marks'),
            (N'Exam_Result_of_Subject'),
            (N'Exam_Result_of_Student'),
            (N'Exam_Publish_Sub_Countable_Mark'),
            (N'Exam_Publish_Setting'),
            (N'Exam_Cumulative_Subject'),
            (N'Exam_Cumulative_Student'),
            (N'Exam_Cumulative_FullMarks'),
            (N'Exam_Cumulative_ExamList'),
            (N'Exam_Cumulative_Setting'),
            (N'Exam_Full_Marks'),
            (N'Exam_Grading_Assign'),
            (N'Exam_SubExam_Name'),
            (N'Exam_Name'),
            (N'WeeklyExam'),
            (N'Attendance_Schedule_AssignStudent'),
            (N'Attendance_Schedule_ChangeRecord'),
            (N'Attendance_Schedule_Day'),
            (N'Attendance_Record'),
            (N'Attendance_Record_Device'),
            (N'Attendance_Student'),
            (N'Attendance_Leave'),
            (N'Attendance_SMS'),
            (N'Attendance_SMS_Failed'),
            (N'Attendance_Monthly_Report'),
            (N'Attendance_Fine'),
            (N'Attendance_Schedule'),
            (N'Income_PaymentRecord'),
            (N'Income_MoneyReceipt'),
            (N'Income_Discount_Record'),
            (N'Income_LateFee_Change_Record'),
            (N'Income_LateFee_Discount_Record'),
            (N'Income_PayOrder'),
            (N'Income_Assign_Role'),
            (N'AccountIN_Record'),
            (N'AccountOUT_Record'),
            (N'Expenditure'),
            (N'Extra_Income'),
            (N'StudentRecord'),
            (N'Student_Fault'),
            (N'StudentsClass'),
            (N'RoutineForClass'),
            (N'SMS_OtherInfo'),
            (N'Employee_Holiday');

            DECLARE @sOrd INT = 1, @sMax INT, @batch INT, @batchRows INT;
            SELECT @sMax = MAX(Ord) FROM @SessionDel;

            WHILE @sOrd <= @sMax
            BEGIN
                SELECT @tbl = TableName FROM @SessionDel WHERE Ord = @sOrd;
                IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.' + @tbl, 'SchoolID') IS NOT NULL
                   AND COL_LENGTH(N'dbo.' + @tbl, 'EducationYearID') IS NOT NULL
                BEGIN
                    -- Batched delete: commit each batch so other users are not blocked long
                    SET @batch = 0;
                    WHILE @batch < 500
                    BEGIN
                        SET @batch = @batch + 1;
                        SET @sql = N'
                            DELETE TOP (2000)
                            FROM dbo.' + QUOTENAME(@tbl) + N' WITH (ROWLOCK, READPAST)
                            WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID;';
                        BEGIN TRY
                            EXEC sp_executesql @sql,
                                N'@SchoolID INT, @EducationYearID INT',
                                @SchoolID = @SchoolID, @EducationYearID = @EducationYearID;
                            SET @batchRows = @@ROWCOUNT;
                            SET @DeletedRows = @DeletedRows + @batchRows;
                            IF @HasProgress = 1 AND (@batchRows > 0 OR (@DeletedRows % 5000) = 0)
                                UPDATE dbo.Institution_Reset_Progress
                                SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                                WHERE SchoolID = @SchoolID AND Status = N'Running';
                            IF @batchRows = 0 BREAK;
                        END TRY
                        BEGIN CATCH
                            -- lock timeout / FK: try once more without READPAST, then move on
                            BEGIN TRY
                                SET @sql = N'DELETE TOP (500) FROM dbo.' + QUOTENAME(@tbl) +
                                           N' WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID;';
                                EXEC sp_executesql @sql,
                                    N'@SchoolID INT, @EducationYearID INT',
                                    @SchoolID = @SchoolID, @EducationYearID = @EducationYearID;
                                SET @batchRows = @@ROWCOUNT;
                                SET @DeletedRows = @DeletedRows + @batchRows;
                                IF @HasProgress = 1 AND @batchRows > 0
                                    UPDATE dbo.Institution_Reset_Progress
                                    SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                                    WHERE SchoolID = @SchoolID AND Status = N'Running';
                                IF @batchRows = 0 BREAK;
                            END TRY
                            BEGIN CATCH
                                BREAK; -- leave remaining for multi-pass
                            END CATCH
                        END CATCH
                    END
                END
                SET @sOrd = @sOrd + 1;
            END

            -- Light multi-pass for remaining SchoolID+Year tables (still no outer TRAN)
            SET @Pass = 0;
            WHILE @Pass < 10
            BEGIN
                SET @Pass = @Pass + 1;
                DECLARE @didWork BIT = 0;

                DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                SELECT t.TABLE_SCHEMA, t.TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                  AND t.TABLE_SCHEMA = 'dbo'
                  AND t.TABLE_NAME NOT IN (
                        N'SchoolInfo', N'Education_Year', N'Education_Year_User',
                        N'Registration', N'Admin', N'AST', N'SMS', N'Account',
                        N'AAP_Invoice', N'AAP_Invoice_Category', N'AAP_Invoice_Receipt',
                        N'AAP_Invoice_Payment_Record', N'AAP_Invoice_OnlinePayment',
                        N'AAP_Reference', N'AAP_Reference_School', N'AAP_Reference_Commission',
                        N'AAP_Reference_PaymentRecord', N'AAP_Reference_PayOrder', N'AAP_Reference_Target',
                        N'aspnet_Applications', N'aspnet_Membership', N'aspnet_Users',
                        N'aspnet_UsersInRoles', N'aspnet_Roles', N'aspnet_Profile',
                        N'Authority_Info', N'Authority_Link_Category', N'Authority_Link_SubCategory',
                        N'Authority_Link_Pages', N'Authority_Link_Users'
                  )
                  AND EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                          AND c.COLUMN_NAME = 'SchoolID'
                  )
                  AND EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                          AND c.COLUMN_NAME = 'EducationYearID'
                  );

                OPEN cur;
                FETCH NEXT FROM cur INTO @schema, @tbl;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @sql = N'DELETE TOP (2000) FROM ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) +
                               N' WITH (ROWLOCK, READPAST) WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID';
                    BEGIN TRY
                        EXEC sp_executesql @sql,
                            N'@SchoolID INT, @EducationYearID INT',
                            @SchoolID = @SchoolID, @EducationYearID = @EducationYearID;
                        SET @rc = @@ROWCOUNT;
                        IF @rc > 0
                        BEGIN
                            SET @DeletedRows = @DeletedRows + @rc;
                            SET @didWork = 1;
                        END
                    END TRY
                    BEGIN CATCH
                        -- ignore lock/FK for this table this pass
                    END CATCH

                    FETCH NEXT FROM cur INTO @schema, @tbl;
                END
                CLOSE cur;
                DEALLOCATE cur;

                IF @didWork = 0 BREAK;
            END

            BEGIN TRY
                IF @AdminRegistrationID IS NOT NULL
                    DELETE FROM dbo.Education_Year_User
                    WHERE SchoolID = @SchoolID
                      AND EducationYearID = @EducationYearID
                      AND RegistrationID <> @AdminRegistrationID;
                ELSE
                    DELETE FROM dbo.Education_Year_User
                    WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
            END TRY BEGIN CATCH END CATCH;

            -- Clear active sessions for this school/year so hang risk drops
            BEGIN TRY
                IF OBJECT_ID(N'dbo.User_Active_Sessions', N'U') IS NOT NULL
                    DELETE FROM dbo.User_Active_Sessions WHERE SchoolID = @SchoolID;
            END TRY BEGIN CATCH END CATCH;

            SET @Message = N'Session data deleted successfully'
                + CASE WHEN @ActiveUsers > 0
                       THEN N' (note: ' + CAST(@ActiveUsers AS NVARCHAR(10)) + N' live user(s) were online; used short batched deletes).'
                       ELSE N'.' END;

            IF @HasProgress = 1
                UPDATE dbo.Institution_Reset_Progress
                SET DeletedRows = @DeletedRows, Status = N'Done', Message = @Message, UpdatedAt = SYSUTCDATETIME()
                WHERE SchoolID = @SchoolID;

            SELECT
                N'Success' AS Status,
                @SchoolID AS SchoolID,
                @Mode AS Mode,
                @EducationYearID AS EducationYearID,
                @KeepEducationYearID AS KeptEducationYearID,
                @DeletedRows AS DeletedRows,
                @Message AS Message;
        END TRY
        BEGIN CATCH
            SET @Message = ERROR_MESSAGE();
            IF @HasProgress = 1
                UPDATE dbo.Institution_Reset_Progress
                SET DeletedRows = @DeletedRows, Status = N'Error', Message = @Message, UpdatedAt = SYSUTCDATETIME()
                WHERE SchoolID = @SchoolID;
            SELECT
                N'Error' AS Status,
                @SchoolID AS SchoolID,
                @Mode AS Mode,
                @EducationYearID AS EducationYearID,
                ERROR_LINE() AS ErrorLine,
                @Message AS Message;
        END CATCH

        RETURN;
    END

    --------------------------------------------------------
    -- FULL / PURGE only below (NO long transaction — avoids server hang)
    --------------------------------------------------------
    BEGIN TRY
        SET DEADLOCK_PRIORITY LOW;
        SET LOCK_TIMEOUT 15000;

        BEGIN
            ------------------------------------------------
            -- FULL/PURGE: batched deletes (auto-commit each batch)
            ------------------------------------------------

            -- Account_Log first (no trigger)
            BEGIN TRY
                WHILE 1 = 1
                BEGIN
                    DELETE TOP (3000) FROM dbo.Account_Log WITH (ROWLOCK, READPAST) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1
                        UPDATE dbo.Institution_Reset_Progress
                        SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                        WHERE SchoolID = @SchoolID AND Status = N'Running';
                END
            END TRY BEGIN CATCH END CATCH;

            -- Brief trigger disable ONLY around account/income ledger deletes
            BEGIN TRY
                DISABLE TRIGGER ALL ON dbo.AccountIN_Record;
                DISABLE TRIGGER ALL ON dbo.AccountOUT_Record;
                DISABLE TRIGGER ALL ON dbo.Income_PaymentRecord;
                DISABLE TRIGGER ALL ON dbo.Income_PayOrder;
                IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL DISABLE TRIGGER ALL ON dbo.Income_MoneyReceipt;
                IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL DISABLE TRIGGER ALL ON dbo.Expenditure;
                IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL DISABLE TRIGGER ALL ON dbo.Extra_Income;
                SET @TriggersDisabled = 1;

                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.AccountIN_Record WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.AccountOUT_Record WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Income_PaymentRecord WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Income_MoneyReceipt WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Income_PayOrder WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Expenditure WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Extra_Income WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
            END TRY BEGIN CATCH END CATCH;

            -- Re-enable ledger triggers immediately so other schools are not blocked
            BEGIN TRY
                ENABLE TRIGGER ALL ON dbo.AccountIN_Record;
                ENABLE TRIGGER ALL ON dbo.AccountOUT_Record;
                ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord;
                ENABLE TRIGGER ALL ON dbo.Income_PayOrder;
                IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Income_MoneyReceipt;
                IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Expenditure;
                IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Extra_Income;
                SET @TriggersDisabled = 0;
            END TRY BEGIN CATCH END CATCH;

            -- Helper macro style: delete if table/column exists
            DECLARE @Del TABLE (Ord INT IDENTITY(1,1), SqlText NVARCHAR(MAX));

            INSERT INTO @Del (SqlText) VALUES
            -- Attendance children first
            (N'DELETE FROM dbo.Attendance_Schedule_AssignStudent WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Schedule_ChangeRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Schedule_Day WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Record_Device WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Student WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Leave WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_SMS WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_SMS_Failed WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Monthly_Report WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Fine WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Schedule WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Device_DataUpdateList WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Device_Setting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Leave_Type WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_SMS_Sender WHERE SchoolID=@SchoolID'),

            -- Device
            (N'DELETE FROM dbo.Device_Commands WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Device_Finger_Print_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Device_Institution_Mapping WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Up_RFID WHERE SchoolID=@SchoolID'),

            -- Exam
            (N'DELETE FROM dbo.Exam_Obtain_Marks WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Result_of_Subject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Result_of_Student WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Publish_Sub_Countable_Mark WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Publish_Setting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Subject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Student WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_FullMarks WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_ExamList WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Setting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Full_Marks WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Grading_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Grading_System WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Grade_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_SubExam_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_CellData WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_ClassColumns WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_Rows WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_SavedData WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.WeeklyExam WHERE SchoolID=@SchoolID'),

            -- Income / Expense
            (N'DELETE FROM dbo.Income_PaymentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_MoneyReceipt WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_Discount_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_LateFee_Change_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_LateFee_Discount_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_PayOrder WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_Assign_Role WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_Roles WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Expenditure WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Expense_SubCategory WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Expense_CategoryName WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Extra_Income WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Extra_IncomeCategory WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Temp_Online_PaymentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Temp_Online_DonationPaymentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.User_Balance_Submission WHERE SchoolID=@SchoolID'),

            -- Account
            (N'DELETE FROM dbo.Account_Log WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AccountIN_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AccountOUT_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Account WHERE SchoolID=@SchoolID'),

            -- Employee / payroll
            (N'DELETE FROM dbo.Employee_Payorder_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Daily WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Monthly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Weekly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Work_Basis WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Allowance_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Allowance_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Allowance WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Bonus_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Bonus WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Deduction_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Deduction_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Deduction WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Fine_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Fine WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Attendance_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Attendance_Report WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Attendance_Schedule_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Leave WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Holiday WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Info WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_SubCategory WHERE SchoolID=@SchoolID'),

            -- Committee
            (N'DELETE FROM dbo.CommitteePaymentRecord WHERE SchoolId=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMoneyReceipt WHERE SchoolId=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeDonation WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMember_Billing WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMember WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMemberType WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeDonationCategory WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeDonationTemplate WHERE SchoolID=@SchoolID'),

            -- Student / notices
            (N'DELETE FROM dbo.StudentNoticeClass WHERE StudentNoticeId IN (SELECT StudentNoticeId FROM dbo.StudentNotice WHERE SchoolId=@SchoolID)'),
            (N'DELETE FROM dbo.StudentNotice WHERE SchoolId=@SchoolID'),
            (N'DELETE FROM dbo.NoticeBoard WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.StudentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student_Fault WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student_Act_Deactivate_Log WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student_Image WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.StudentsClass WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student WHERE SchoolID=@SchoolID'),

            -- Teacher / staff / subject
            (N'DELETE FROM dbo.TeacherSubject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.TecherSubject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Achievements WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Additional WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Career_Objective WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_EducationInfo WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_JobInfo WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Language WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Skill WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Staff_Info WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SubjectForGroup WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.TechnoSubject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Subject WHERE SchoolID=@SchoolID'),

            -- Class structure
            (N'DELETE FROM dbo.[Join] WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateSection WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateShift WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateSubjectGroup WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateClass WHERE SchoolID=@SchoolID'),

            -- Routine
            (N'DELETE FROM dbo.RoutineForClass WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineTemporary WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineTime WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineDay WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineInfo WHERE SchoolID=@SchoolID'),

            -- SMS history (keep SMS wallet row later)
            (N'DELETE FROM dbo.SMS_OtherInfo WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Send_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Recharge_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Group_Phone_Number WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Group_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Template WHERE SchoolID=@SchoolID'),

            -- Counts / settings
            (N'DELETE FROM dbo.AAP_StudentClass_Count_Monthly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AAP_Student_Count_Monthly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AAP_Auto_Process_Log WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SchoolInfo_DueNoticeSettings WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SikkhaitySetting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.User_Active_Sessions WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.WordOfTheDay WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Link_Users WHERE SchoolID=@SchoolID');

            -- Teacher child rows by join (may not have SchoolID column)
            BEGIN TRY
                DELETE X FROM dbo.Teacher_Achievements X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Additional X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Career_Objective X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_EducationInfo X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_JobInfo X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Language X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Skill X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
            END TRY BEGIN CATCH END CATCH;

            DECLARE @Ord INT, @maxOrd INT, @delSql NVARCHAR(MAX), @loopN INT, @loopRows INT;
            SELECT @Ord = MIN(Ord), @maxOrd = MAX(Ord) FROM @Del;
            WHILE @Ord <= @maxOrd
            BEGIN
                SELECT @sql = SqlText FROM @Del WHERE Ord = @Ord;
                -- Convert plain DELETE into batched TOP deletes when possible
                IF @sql LIKE N'DELETE FROM dbo.%WHERE SchoolID=@SchoolID'
                   OR @sql LIKE N'DELETE FROM dbo.%WHERE SchoolId=@SchoolID'
                BEGIN
                    SET @tbl = NULL;
                    SET @tbl = SUBSTRING(@sql, CHARINDEX(N'dbo.', @sql) + 4, 200);
                    SET @tbl = LEFT(@tbl, CHARINDEX(N' ', @tbl + N' ') - 1);
                    SET @tbl = REPLACE(REPLACE(@tbl, N'[', N''), N']', N'');
                    IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NOT NULL
                    BEGIN
                        SET @loopN = 0;
                        WHILE @loopN < 400
                        BEGIN
                            SET @loopN = @loopN + 1;
                            SET @delSql = N'DELETE TOP (2000) FROM dbo.' + QUOTENAME(@tbl) +
                                          N' WITH (ROWLOCK, READPAST) WHERE SchoolID = @SchoolID';
                            BEGIN TRY
                                EXEC sp_executesql @delSql, N'@SchoolID INT', @SchoolID=@SchoolID;
                                SET @loopRows = @@ROWCOUNT;
                                SET @DeletedRows = @DeletedRows + @loopRows;
                                IF @loopRows = 0 BREAK;
                            END TRY
                            BEGIN CATCH
                                BEGIN TRY
                                    EXEC sp_executesql @sql, N'@SchoolID INT', @SchoolID=@SchoolID;
                                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                                END TRY BEGIN CATCH END CATCH;
                                BREAK;
                            END CATCH
                        END
                    END
                END
                ELSE
                BEGIN
                    BEGIN TRY
                        EXEC sp_executesql @sql, N'@SchoolID INT', @SchoolID=@SchoolID;
                        SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    END TRY
                    BEGIN CATCH
                        -- continue; multi-pass below will retry leftovers
                    END CATCH
                END
                SET @Ord = @Ord + 1;
            END

            -- Multi-pass: any remaining dbo tables with SchoolID except keep-list
            SET @Pass = 0;
            WHILE @Pass < 25
            BEGIN
                SET @Pass = @Pass + 1;
                DECLARE @didWork2 BIT = 0;

                DECLARE cur2 CURSOR LOCAL FAST_FORWARD FOR
                SELECT t.TABLE_SCHEMA, t.TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                  AND t.TABLE_SCHEMA = 'dbo'
                  AND t.TABLE_NAME NOT IN (
                        N'SchoolInfo', N'Education_Year', N'Education_Year_User',
                        N'Registration', N'Admin', N'AST', N'SMS',
                        N'AAP_Invoice', N'AAP_Invoice_Category', N'AAP_Invoice_Receipt',
                        N'AAP_Invoice_Payment_Record', N'AAP_Invoice_OnlinePayment',
                        N'AAP_Reference', N'AAP_Reference_School', N'AAP_Reference_Commission',
                        N'AAP_Reference_PaymentRecord', N'AAP_Reference_PayOrder', N'AAP_Reference_Target',
                        N'aspnet_Applications', N'aspnet_Membership', N'aspnet_Users',
                        N'aspnet_UsersInRoles', N'aspnet_Roles', N'aspnet_Profile',
                        N'aspnet_Paths', N'aspnet_PersonalizationAllUsers', N'aspnet_PersonalizationPerUser',
                        N'aspnet_SchemaVersions', N'aspnet_WebEvent_Events',
                        N'Authority_Info', N'Authority_Link_Category', N'Authority_Link_SubCategory',
                        N'Authority_Link_Pages', N'Authority_Link_Users',
                        N'Link_Category', N'Link_SubCategory', N'Link_Pages'
                  )
                  AND EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                          AND c.COLUMN_NAME IN ('SchoolID', 'SchoolId')
                  );

                OPEN cur2;
                FETCH NEXT FROM cur2 INTO @schema, @tbl;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    DECLARE @col SYSNAME =
                        CASE WHEN EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_SCHEMA=@schema AND TABLE_NAME=@tbl AND COLUMN_NAME='SchoolID'
                        ) THEN 'SchoolID' ELSE 'SchoolId' END;

                    SET @sql = N'
                        BEGIN TRY
                            DELETE FROM ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) + N'
                            WHERE ' + QUOTENAME(@col) + N' = @SchoolID;
                            SET @rc = @@ROWCOUNT;
                        END TRY
                        BEGIN CATCH
                            SET @rc = 0;
                        END CATCH';
                    BEGIN TRY
                        EXEC sp_executesql
                            @sql,
                            N'@SchoolID INT, @rc INT OUTPUT',
                            @SchoolID = @SchoolID,
                            @rc = @rc OUTPUT;
                        IF @rc > 0
                        BEGIN
                            SET @DeletedRows = @DeletedRows + @rc;
                            SET @didWork2 = 1;
                            IF @HasProgress = 1
                                UPDATE dbo.Institution_Reset_Progress
                                SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                                WHERE SchoolID = @SchoolID AND Status = N'Running';
                        END
                    END TRY
                    BEGIN CATCH
                    END CATCH

                    FETCH NEXT FROM cur2 INTO @schema, @tbl;
                END
                CLOSE cur2;
                DEALLOCATE cur2;

                IF @didWork2 = 0 BREAK;
            END

            -- Extra education years (keep one)
            IF @KeepEducationYearID IS NOT NULL
            BEGIN
                DELETE FROM dbo.Education_Year_User
                WHERE SchoolID = @SchoolID AND EducationYearID <> @KeepEducationYearID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE FROM dbo.Education_Year
                WHERE SchoolID = @SchoolID AND EducationYearID <> @KeepEducationYearID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- Keep only admin on remaining year
                IF @AdminRegistrationID IS NOT NULL
                BEGIN
                    DELETE FROM dbo.Education_Year_User
                    WHERE SchoolID = @SchoolID
                      AND EducationYearID = @KeepEducationYearID
                      AND RegistrationID <> @AdminRegistrationID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.Education_Year_User
                        WHERE SchoolID = @SchoolID
                          AND EducationYearID = @KeepEducationYearID
                          AND RegistrationID = @AdminRegistrationID
                    )
                    BEGIN
                        INSERT INTO dbo.Education_Year_User (EducationYearID, SchoolID, RegistrationID)
                        VALUES (@KeepEducationYearID, @SchoolID, @AdminRegistrationID);
                    END
                END
            END

            -- Remove non-admin registrations for this school
            IF OBJECT_ID('tempdb..#NonAdminUsers') IS NOT NULL DROP TABLE #NonAdminUsers;
            SELECT RegistrationID, UserName
            INTO #NonAdminUsers
            FROM dbo.Registration
            WHERE SchoolID = @SchoolID
              AND (@AdminRegistrationID IS NULL OR RegistrationID <> @AdminRegistrationID);

            DELETE LU FROM dbo.Link_Users LU
            INNER JOIN #NonAdminUsers N ON LU.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            DELETE EYU FROM dbo.Education_Year_User EYU
            INNER JOIN #NonAdminUsers N ON EYU.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            DELETE A FROM dbo.AST A
            INNER JOIN #NonAdminUsers N ON A.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            -- Membership cleanup for non-admin usernames
            DELETE UIR
            FROM dbo.aspnet_UsersInRoles UIR
            INNER JOIN dbo.aspnet_Users U ON U.UserId = UIR.UserId
            INNER JOIN #NonAdminUsers N ON N.UserName = U.UserName;

            DELETE M
            FROM dbo.aspnet_Membership M
            INNER JOIN dbo.aspnet_Users U ON U.UserId = M.UserId
            INNER JOIN #NonAdminUsers N ON N.UserName = U.UserName;

            DELETE U
            FROM dbo.aspnet_Users U
            INNER JOIN #NonAdminUsers N ON N.UserName = U.UserName;

            DELETE R FROM dbo.Registration R
            INNER JOIN #NonAdminUsers N ON R.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            IF @Mode = 'FULL'
            BEGIN
                -- Ensure SMS wallet exists and is zeroed
                IF EXISTS (SELECT 1 FROM dbo.SMS WHERE SchoolID = @SchoolID)
                    UPDATE dbo.SMS SET SMS_Balance = 0 WHERE SchoolID = @SchoolID;
                ELSE
                    INSERT INTO dbo.SMS (SchoolID, SMS_Balance, Masking, Date)
                    VALUES (@SchoolID, 0, N'Sikkhaloy', GETDATE());

                SET @Message = N'Institution reset to new-signup state successfully.';
            END
            ELSE IF @Mode = 'PURGE'
            BEGIN
                ------------------------------------------------
                -- PURGE: remove identity / login / school row
                ------------------------------------------------

                -- Platform invoices for this school
                BEGIN TRY
                    DELETE FROM dbo.AAP_Invoice_Payment_Record WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Invoice_OnlinePayment WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Invoice_Receipt WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Invoice WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                END TRY BEGIN CATCH END CATCH;

                -- Referrer assignment & commission for this school
                BEGIN TRY
                    DELETE FROM dbo.AAP_Reference_Commission WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Reference_PaymentRecord WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Reference_PayOrder WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Reference_School WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                END TRY BEGIN CATCH END CATCH;

                -- All sessions
                DELETE FROM dbo.Education_Year_User WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE FROM dbo.Education_Year WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- SMS wallet
                DELETE FROM dbo.SMS WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                ------------------------------------------------
                -- Collect ALL usernames BEFORE deleting identity rows
                -- (Registration / SchoolInfo / AST / known admin)
                ------------------------------------------------
                IF OBJECT_ID('tempdb..#PurgeUserNames') IS NOT NULL DROP TABLE #PurgeUserNames;
                CREATE TABLE #PurgeUserNames (UserName NVARCHAR(256) NOT NULL PRIMARY KEY);

                INSERT INTO #PurgeUserNames (UserName)
                SELECT DISTINCT LTRIM(RTRIM(UserName))
                FROM (
                    SELECT UserName FROM dbo.Registration WHERE SchoolID = @SchoolID AND UserName IS NOT NULL AND LTRIM(RTRIM(UserName)) <> N''
                    UNION
                    SELECT UserName FROM dbo.AST WHERE SchoolID = @SchoolID AND UserName IS NOT NULL AND LTRIM(RTRIM(UserName)) <> N''
                    UNION
                    SELECT UserName FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID AND UserName IS NOT NULL AND LTRIM(RTRIM(UserName)) <> N''
                    UNION
                    SELECT @AdminUserName WHERE @AdminUserName IS NOT NULL AND LTRIM(RTRIM(@AdminUserName)) <> N''
                ) X;

                IF OBJECT_ID('tempdb..#AllSchoolUsers') IS NOT NULL DROP TABLE #AllSchoolUsers;
                SELECT RegistrationID, LTRIM(RTRIM(UserName)) AS UserName
                INTO #AllSchoolUsers
                FROM dbo.Registration
                WHERE SchoolID = @SchoolID;

                DELETE LU FROM dbo.Link_Users LU
                INNER JOIN #AllSchoolUsers U ON LU.RegistrationID = U.RegistrationID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE FROM dbo.Admin WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE FROM dbo.AST WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- Membership cleanup (match UserName OR LoweredUserName)
                IF OBJECT_ID('tempdb..#PurgeUserIds') IS NOT NULL DROP TABLE #PurgeUserIds;
                SELECT DISTINCT AU.UserId
                INTO #PurgeUserIds
                FROM dbo.aspnet_Users AU
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);

                BEGIN TRY
                    DELETE P FROM dbo.aspnet_Profile P INNER JOIN #PurgeUserIds U ON P.UserId = U.UserId;
                    DELETE P FROM dbo.aspnet_PersonalizationPerUser P INNER JOIN #PurgeUserIds U ON P.UserId = U.UserId;
                END TRY BEGIN CATCH END CATCH;

                DELETE UIR
                FROM dbo.aspnet_UsersInRoles UIR
                INNER JOIN #PurgeUserIds U ON UIR.UserId = U.UserId;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE M
                FROM dbo.aspnet_Membership M
                INNER JOIN #PurgeUserIds U ON M.UserId = U.UserId;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE AU
                FROM dbo.aspnet_Users AU
                INNER JOIN #PurgeUserIds U ON AU.UserId = U.UserId;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE R FROM dbo.Registration R
                WHERE R.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                BEGIN TRY
                    DELETE FROM dbo.SchoolInfo_DueNoticeSettings WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                END TRY BEGIN CATCH END CATCH;

                -- Capture SchoolInfo.UserName again just before delete (safety)
                INSERT INTO #PurgeUserNames (UserName)
                SELECT DISTINCT LTRIM(RTRIM(UserName))
                FROM dbo.SchoolInfo
                WHERE SchoolID = @SchoolID
                  AND UserName IS NOT NULL
                  AND LTRIM(RTRIM(UserName)) <> N''
                  AND NOT EXISTS (
                        SELECT 1 FROM #PurgeUserNames P WHERE LOWER(P.UserName) = LOWER(LTRIM(RTRIM(SchoolInfo.UserName)))
                  );

                DELETE FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- Final orphan membership sweep for collected usernames
                DELETE UIR
                FROM dbo.aspnet_UsersInRoles UIR
                INNER JOIN dbo.aspnet_Users AU ON AU.UserId = UIR.UserId
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);

                DELETE M
                FROM dbo.aspnet_Membership M
                INNER JOIN dbo.aspnet_Users AU ON AU.UserId = M.UserId
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);

                DELETE AU
                FROM dbo.aspnet_Users AU
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                IF EXISTS (SELECT 1 FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID)
                BEGIN
                    SET @Message = N'Failed to delete SchoolInfo row. Check remaining FK references.';
                    RAISERROR(@Message, 16, 1);
                END

                SET @Message = N'Institution permanently deleted (including login & SchoolInfo).';
            END
        END

        IF @HasProgress = 1
            UPDATE dbo.Institution_Reset_Progress
            SET DeletedRows = @DeletedRows, Status = N'Done', Message = @Message, UpdatedAt = SYSUTCDATETIME()
            WHERE SchoolID = @SchoolID;

        SELECT
            N'Success' AS Status,
            @SchoolID AS SchoolID,
            @Mode AS Mode,
            @EducationYearID AS EducationYearID,
            @KeepEducationYearID AS KeptEducationYearID,
            @DeletedRows AS DeletedRows,
            @Message AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SET @Message = ERROR_MESSAGE();
        IF @HasProgress = 1
            UPDATE dbo.Institution_Reset_Progress
            SET DeletedRows = @DeletedRows, Status = N'Error', Message = @Message, UpdatedAt = SYSUTCDATETIME()
            WHERE SchoolID = @SchoolID;
        SELECT
            N'Error' AS Status,
            @SchoolID AS SchoolID,
            @Mode AS Mode,
            @EducationYearID AS EducationYearID,
            ERROR_LINE() AS ErrorLine,
            @Message AS Message;
    END CATCH

    -- Safety: re-enable ledger triggers if still disabled
    IF @TriggersDisabled = 1
    BEGIN
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.AccountIN_Record; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.AccountOUT_Record; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.Income_PayOrder; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Income_MoneyReceipt; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Expenditure; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Extra_Income; END TRY BEGIN CATCH END CATCH;
    END
END
GO

