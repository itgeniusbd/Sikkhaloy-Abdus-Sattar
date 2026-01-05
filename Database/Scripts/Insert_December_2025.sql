-- ==========================================
-- Manual Script: Insert December 2025 Data
-- Purpose: One-time script to insert December 2025 student count
-- Usage: Run this in SQL Server Management Studio
-- ==========================================

-- First, update the stored procedure (run the AAP_Student_Count_Monthly_Insert.sql file)
-- Then run this script

-- Insert December 2025 data
EXEC AAP_Student_Count_Monthly_Insert @TargetMonth = '2025-12-31';

-- Verify the data was inserted
SELECT 
    FORMAT(Month, 'MMM yyyy') AS MonthName,
    COUNT(*) as SchoolCount,
    SUM(Active_Student) as TotalActive,
    SUM(Reject_Countable) as TotalRejectedCountable,
    SUM(Reject_Uncountable) as TotalRejectedUncountable
FROM AAP_Student_Count_Monthly
WHERE YEAR(Month) = 2025 AND MONTH(Month) = 12
GROUP BY Month;

PRINT 'December 2025 data inserted successfully!';
GO
