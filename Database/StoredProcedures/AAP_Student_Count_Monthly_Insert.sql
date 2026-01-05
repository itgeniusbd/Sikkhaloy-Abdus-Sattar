-- ==========================================
-- Stored Procedure: AAP_Student_Count_Monthly_Insert
-- Database: Edu
-- Server: DESKTOP-3UN61QI
-- Updated: 2026-01-04 - Added Month Parameter & Duplicate Check
-- ==========================================

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AAP_Student_Count_Monthly_Insert]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[AAP_Student_Count_Monthly_Insert]
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Insert monthly student count data with optional month parameter
-- Parameters:	@TargetMonth - Optional. If NULL, uses last day of CURRENT month
-- Schedule:    Runs on 28th of every month to insert that month's data
-- =============================================
CREATE PROCEDURE [dbo].[AAP_Student_Count_Monthly_Insert]
    @TargetMonth DATE = NULL  -- Optional parameter for specific month
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If no month specified, use last day of CURRENT month (not previous)
    IF @TargetMonth IS NULL
    BEGIN
        SET @TargetMonth = EOMONTH(GETDATE());  -- Current month এর শেষ দিন
    END
    ELSE
    BEGIN
        -- Ensure we use the last day of the specified month
        SET @TargetMonth = EOMONTH(@TargetMonth);
    END
    
    -- Check if data already exists for this month
    IF EXISTS (SELECT 1 FROM AAP_Student_Count_Monthly WHERE Month = @TargetMonth)
    BEGIN
        PRINT 'Data for ' + FORMAT(@TargetMonth, 'MMMM yyyy') + ' already exists. Skipping insert.';
        RETURN;
    END
    
    -- Insert StudentClass data with specified month
    INSERT INTO AAP_StudentClass_Count_Monthly
        (SchoolID, EducationYearID, ClassID, Active_Student, Reject_Countable, Reject_Uncountable, Month)
    SELECT 
        SchoolID, 
        EducationYearID, 
        ClassID, 
        ActiveStudent, 
        Reject_Countable, 
        Reject_Uncountable,
        @TargetMonth AS Month
    FROM VW_Payment_Monthly_StudentClass;
    
    -- Insert Student data with specified month
    INSERT INTO AAP_Student_Count_Monthly
        (SchoolID, Active_Student, Reject_Countable, Reject_Uncountable, Month)
    SELECT 
        SchoolID, 
        ActiveStudent, 
        Reject_Countable, 
        Reject_Uncountable,
        @TargetMonth AS Month
    FROM VW_Payment_Monthly_Stu;
    
    PRINT 'Successfully inserted data for ' + FORMAT(@TargetMonth, 'MMMM yyyy');
END
GO

