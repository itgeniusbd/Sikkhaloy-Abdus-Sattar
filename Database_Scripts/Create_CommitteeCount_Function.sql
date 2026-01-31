-- =============================================
-- Function to Get Billable Committee Member Count
-- Purpose: Calculate active committee member count for billing
-- Created: 2025-01-29
-- =============================================

USE [Edu]  -- Change to your database name
GO

-- Drop function if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fn_GetBillableCommitteeCount]') AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT'))
    DROP FUNCTION [dbo].[fn_GetBillableCommitteeCount]
GO

CREATE FUNCTION [dbo].[fn_GetBillableCommitteeCount]
(
    @SchoolID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT = 0
    
    SELECT @Count = ISNULL(SUM(MemberCount), 0)
    FROM (
        SELECT COUNT(CM.CommitteeMemberId) as MemberCount
        FROM CommitteeMember_Billing CMB
        INNER JOIN CommitteeMember CM 
            ON CMB.CommitteeMemberTypeId = CM.CommitteeMemberTypeId 
            AND CMB.SchoolID = CM.SchoolID
        WHERE CMB.SchoolID = @SchoolID
          AND CMB.IsIncluded = 1
          AND CMB.IsActive = 1
          AND ISNULL(CM.Status, 'Active') = 'Active'
        GROUP BY CMB.CommitteeMemberTypeId
    ) AS CategoryCounts
    
    RETURN @Count
END
GO

-- Test the function
-- SELECT dbo.fn_GetBillableCommitteeCount(1) as CommitteeCount

PRINT 'Function fn_GetBillableCommitteeCount created successfully!'
GO
