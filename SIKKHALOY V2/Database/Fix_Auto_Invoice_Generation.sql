-- Fix auto + manual invoice generation issues
-- 1) StudentCount NULL হলে Active_Student ব্যবহার
-- 2) Auto SP: EOMONTH match + committee count + শুধু unpaid duplicate block

UPDATE AAP_Student_Count_Monthly
SET StudentCount = Active_Student
WHERE StudentCount IS NULL OR StudentCount = 0;

GO

-- ডায়াগনোস্টিক: ভুল MonthName সহ April ইনভয়েস (EOMONTH = May কিন্তু Invoice_For = April)
-- SELECT SchoolID, InvoiceID, MonthName, Invoice_For, IsPaid, TotalAmount, PaidAmount
-- FROM AAP_Invoice
-- WHERE InvoiceCategoryID = 1
--   AND EOMONTH(MonthName) = '2026-05-31'
--   AND Invoice_For LIKE '%April%';

GO

-- Re-deploy updated procedure (run full file from Database/StoredProcedures/AAP_Auto_Generate_Monthly_Invoice.sql)
