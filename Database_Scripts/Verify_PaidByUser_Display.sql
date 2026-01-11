-- Verify: Show exact data that should appear on Invoice_List.aspx page
-- Run this to see what the webpage should display

SELECT TOP 10
    InvoiceReceipt_SN AS 'Receipt',
    TotalAmount AS 'Total Amount',
    PaymentBy AS 'Payment By',
    PaidByUser AS 'Paid By User',  -- This column should show username
    Collected_By AS 'Collected By',
    Payment_Method AS 'Payment Method',
    CONVERT(VARCHAR, PaidDate, 106) AS 'Paid Date'
FROM AAP_Invoice_Receipt
WHERE SchoolID = 1263  -- Replace with your SchoolID from session
ORDER BY PaidDate DESC
GO

-- Expected Result:
-- Receipt | Total Amount | Payment By | Paid By User | Collected By | Payment Method | Paid Date
-- --------|--------------|------------|--------------|--------------|----------------|------------
-- 103467  | 2090         | Principal  | System       | Abdus sattar | bKash          | 06 Jan 2026
