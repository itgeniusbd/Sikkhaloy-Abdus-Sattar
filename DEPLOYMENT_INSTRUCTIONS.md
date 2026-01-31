# ?? Donor Payment System - Deployment Instructions

## ?? Pre-Deployment Changes

### 1?? **Update ConfirmationBase URL** (MANDATORY)

**File:** `SIKKHALOY V2\Committee\Donor_Dues.aspx.cs`  
**Line:** 21

```csharp
// ? BEFORE (Localhost):
private string ConfirmationBase = "http://localhost:3326";

// ? AFTER (Production):
private string ConfirmationBase = "https://yourschool.sikkhaloy.com";
// Replace 'yourschool' with actual subdomain
```

---

## ??? Database Requirements

### Email Column in CommitteeMember Table

**Already completed!** ?

```sql
-- Verify Email column exists:
SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CommitteeMember' 
AND COLUMN_NAME = 'Email';
```

---

## ?? Payment Gateway Configuration

### Amarpay Settings in SchoolInfo Table

Make sure these fields are filled:

```sql
SELECT 
    SchoolID,
    StoreId,              -- Amarpay Store ID
    SignatureKey,         -- Amarpay Signature Key
    OnlinePaymentEnable,  -- Must be 1 (enabled)
    Email                 -- Institute email (fallback)
FROM SchoolInfo
WHERE SchoolID = @YourSchoolID;
```

**If not filled, update:**

```sql
UPDATE SchoolInfo
SET 
    StoreId = 'your_store_id',
    SignatureKey = 'your_signature_key',
    OnlinePaymentEnable = 1,
    Email = 'school@example.com'
WHERE SchoolID = @YourSchoolID;
```

---

## ?? Files Modified/Created

### ? Modified Files:
1. `SIKKHALOY V2\Committee\Donor_Dues.aspx` - Repeater with Checkboxes
2. `SIKKHALOY V2\Committee\Donor_Dues.aspx.cs` - Payment Logic + Email Handling
3. `SIKKHALOY V2\Committee\Donor_Dues.aspx.designer.cs` - Controls
4. `SIKKHALOY V2\Committee\MemberAdd.aspx` - Email Field Added
5. `SIKKHALOY V2\Committee\MemberAdd.aspx.cs` - Email Handling
6. `SIKKHALOY V2\Committee\MemberAdd.aspx.designer.cs` - Email Controls

### ? Database Scripts:
1. `Database_Scripts\Add_Email_To_CommitteeMember.sql` - Email Column

---

## ?? Testing Checklist (Before Deployment)

### On Localhost:

- [x] Member Add with Email field works
- [x] Donor Login successful
- [x] Dues page loads without error
- [x] Checkboxes visible and clickable
- [x] Select multiple donations
- [x] Total amount calculates correctly
- [x] Pay Now button enabled
- [x] Payment redirect to Amarpay
- [x] Payment successful

### On Production (After Deployment):

- [ ] Update ConfirmationBase URL
- [ ] Member Add with Email works
- [ ] Donor Login works
- [ ] Dues page loads
- [ ] Payment flow complete (Success/Fail/Cancel)
- [ ] Payment record saved in database
- [ ] Donation amounts updated

---

## ?? Security Settings

### IsSandbox Flag

**Development/Testing:**
```csharp
private static readonly bool IsSandbox = true;  // Sandbox mode
```

**Production:**
```csharp
private static readonly bool IsSandbox = false;  // Live mode
```

---

## ?? URL Configuration Summary

### Payment URLs (Auto-configured):

```csharp
// Success URL (after payment)
success_url = ConfirmationBase + "/Default.aspx"

// Fail URL (payment failed)
fail_url = ConfirmationBase + "/Committee/OnlinePayment/Failed.aspx"

// Cancel URL (user cancelled)
cancel_url = ConfirmationBase + "/Committee/OnlinePayment/Cancelled.aspx"
```

**Production Example:**
```
https://yourschool.sikkhaloy.com/Default.aspx
https://yourschool.sikkhaloy.com/Committee/OnlinePayment/Failed.aspx
https://yourschool.sikkhaloy.com/Committee/OnlinePayment/Cancelled.aspx
```

---

## ?? Features Implemented

### ? Donor Payment System:

1. **Email Integration:**
   - Member email (if provided)
   - Fallback to Institute email
   - Required for payment gateway

2. **Checkbox Selection:**
   - Select All functionality
   - Individual selection
   - Row highlighting on select

3. **Payment Flow:**
   - Multiple donations in one payment
   - Real-time amount calculation
   - Secure Amarpay integration
   - Session-based tracking

4. **Error Handling:**
   - Database column check (Email)
   - Session validation
   - Default values fallback
   - SQL exception handling

---

## ?? Common Issues & Solutions

### Issue 1: "Invalid column name 'Email'"
**Solution:** Run the SQL script to add Email column
```sql
ALTER TABLE CommitteeMember ADD Email NVARCHAR(100) NULL;
```

### Issue 2: "Object reference not set" after payment
**Solution:** Update ConfirmationBase to production URL

### Issue 3: "Email is required" error
**Solution:** 
- Add email in Member profile, OR
- Check SchoolInfo.Email is not empty

### Issue 4: Checkbox not visible
**Solution:** Hard refresh browser (Ctrl+F5)

---

## ?? Support Contact

For any deployment issues, contact:
- **Developer:** [Your Contact]
- **System Admin:** [Admin Contact]

---

## ? Final Deployment Steps

1. **Backup Database** (IMPORTANT!)
2. **Update ConfirmationBase URL** in code
3. **Set IsSandbox = false** for production
4. **Build Solution** (Release mode)
5. **Publish to Server**
6. **Verify SchoolInfo settings**
7. **Test payment flow**
8. **Monitor first few transactions**

---

## ?? Deployment Complete!

Once deployed, notify donors that:
- ? Online payment is now available
- ? Multiple donations can be paid at once
- ? Secure payment via Amarpay
- ? Email receipts will be sent

---

**Last Updated:** 2025-01-29  
**Version:** 1.0  
**Status:** Ready for Production ??
