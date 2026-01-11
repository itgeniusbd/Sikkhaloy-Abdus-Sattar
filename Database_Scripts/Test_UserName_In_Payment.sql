-- Test: Check which user is logged in and what username will be saved
-- This shows what the new payment records should have

-- Check 1: What's in the Registration table for current user?
SELECT 
    RegistrationID,
    UserName,
    Category,
    Validation,
    CreateDate
FROM Registration
WHERE RegistrationID = 1  -- Replace with your Session["RegistrationID"]
GO

-- Check 2: Show the SQL that will be executed when payment is made
-- This is what the modified InsertCommand does:
SELECT 
    1263 AS SchoolID,  -- @SchoolID
    1 AS RegistrationID,  -- @RegistrationID from session
    2090 AS TotalAmount,  -- @TotalAmount
    GETDATE() AS PaidDate,  -- @PaidDate
    'Principal' AS PaymentBy,  -- @PaymentBy from textbox
    'Abdus Sattar' AS Collected_By,  -- @Collected_By from textbox
    'bKash' AS Payment_Method,  -- @Payment_Method from textbox
    Registration.UserName AS PaidByUser  -- This is the username that will be saved
FROM Registration 
WHERE RegistrationID = 1  -- Your session RegistrationID
GO

-- Check 3: Show recent payment records
SELECT TOP 5
    InvoiceReceipt_SN,
    TotalAmount,
    PaymentBy,
    PaidByUser,  -- This should show actual username now
    PaidDate
FROM AAP_Invoice_Receipt
ORDER BY PaidDate DESC
GO
