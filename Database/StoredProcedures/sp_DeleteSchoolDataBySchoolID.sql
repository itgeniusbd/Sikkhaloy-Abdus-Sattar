-- ==========================================
-- Stored Procedure: sp_DeleteSchoolDataBySchoolID
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Backup Date: 2025-11-03 00:18:43
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DeleteSchoolDataBySchoolID]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_DeleteSchoolDataBySchoolID]
END
GO


-- নতুন Stored Procedure তৈরি করুন
CREATE PROCEDURE sp_DeleteSchoolDataBySchoolID
    @SchoolID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ✅ Parameter থেকে সরাসরি School ID নিন
    DECLARE @School_ID INT = 1091  -- এখানে parameter ব্যবহার করুন
    DECLARE @TotalRowsDeleted INT = 0
    
    BEGIN TRANSACTION
    
    BEGIN TRY
        -- AAP Tables
        DELETE FROM [Edu].[dbo].[AAP_Invoice_Payment_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[AAP_Invoice_Receipt] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[AAP_Invoice] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[AAP_StudentClass_Count_Monthly] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Account Tables
        DELETE FROM [Edu].[dbo].[Account_Log] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[AccountIN_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[AccountOUT_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Account] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- আপনার বাকি সব DELETE statements...
        -- (আপনার কোডের বাকি অংশ এখানে থাকবে)
        
        DELETE FROM [Edu].[dbo].[Attendance_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Record_Device] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Device_DataUpdateList] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Device_Setting] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Fine] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Leave] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Monthly_Report] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Schedule_AssignStudent] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Schedule_ChangeRecord] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Schedule_Day] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Schedule] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_SMS] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_SMS_Failed] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Attendance_Student] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CommitteePaymentRecord] WHERE [SchoolId] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CommitteeMoneyReceipt] WHERE [SchoolId] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CommitteeDonation] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CommitteeMember] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CommitteeMemberType] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CommitteeDonationCategory] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CreateSection] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CreateShift] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CreateSubjectGroup] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[CreateClass] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Device_Finger_Print_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Employee Tables
        DELETE FROM [Edu].[dbo].[Employee_Allowance_Records] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Allowance_Assign] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Allowance] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Attendance_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Attendance_Report] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Attendance_Schedule_Assign] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Bonus_Records] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Bonus] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Deduction_Records] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Deduction_Assign] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Deduction] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Fine_Records] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Fine] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Holiday] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Leave] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Payorder_Records] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Payorder_Daily] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Payorder_Monthly] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Payorder_Weekly] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Payorder_Work_Basis] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Payorder] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Employee_Info] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Exam Tables
        DELETE FROM [Edu].[dbo].[Exam_Obtain_Marks] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Result_of_Subject] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Result_of_Student] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Cumulative_Subject] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Cumulative_Student] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Cumulative_FullMarks] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Cumulative_ExamList] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Cumulative_Setting] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Cumulative_Name] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Full_Marks] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Grading_Assign] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Publish_Sub_Countable_Mark] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Publish_Setting] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Exam_Name] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Income and Expenditure
        DELETE FROM [Edu].[dbo].[Expenditure] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Extra_Income] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_PaymentRecord] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_MoneyReceipt] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_Discount_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_LateFee_Change_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_LateFee_Discount_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_PayOrder] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_Assign_Role] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Income_Roles] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Other Tables
        DELETE FROM [Edu].[dbo].[Join] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[NoticeBoard] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[StudentNoticeClass] 
        WHERE [StudentNoticeId] IN (SELECT [StudentNoticeId] FROM [Edu].[dbo].[StudentNotice] WHERE [SchoolId] = @School_ID)
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[StudentNotice] WHERE [SchoolId] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Routine
        DELETE FROM [Edu].[dbo].[RoutineForClass] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[RoutineTime] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[RoutineDay] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[RoutineInfo] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[RoutineTemporary] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- SMS
        DELETE FROM [Edu].[dbo].[SMS_OtherInfo] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[SMS_Recharge_Record] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[SMS] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Staff
        DELETE FROM [Edu].[dbo].[Staff_Info] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Student
        DELETE FROM [Edu].[dbo].[StudentRecord] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[WeeklyExam] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Student_Fault] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Student_Act_Deactivate_Log] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[StudentsClass] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[Student] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        -- Subject
        DELETE FROM [Edu].[dbo].[SubjectForGroup] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        DELETE FROM [Edu].[dbo].[TecherSubject] WHERE [SchoolID] = @School_ID
        SET @TotalRowsDeleted = @TotalRowsDeleted + @@ROWCOUNT
        
        COMMIT TRANSACTION
        
        PRINT 'All data deleted successfully for SchoolID: ' + CAST(@School_ID AS VARCHAR(10))
        PRINT 'Total Rows Deleted: ' + CAST(@TotalRowsDeleted AS VARCHAR(10))
        
        SELECT 'Success' AS Status, 
               @TotalRowsDeleted AS TotalRowsDeleted, 
               @School_ID AS SchoolID,
               'All school data deleted successfully' AS Message
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorLine INT = ERROR_LINE()
        
        PRINT 'Error at line: ' + CAST(@ErrorLine AS VARCHAR(10))
        PRINT 'Error message: ' + @ErrorMessage
        
        SELECT 'Error' AS Status,
               @ErrorMessage AS ErrorMessage,
               @ErrorLine AS ErrorLine,
               ERROR_NUMBER() AS ErrorNumber,
               @School_ID AS SchoolID
        
    END CATCH
END

GO
