# ?? Quick Start Guide - Due Invoice Notification Feature

## Step-by-Step Installation & Usage Guide

---

## ?? Prerequisites (?????????)

- ? SQL Server database access
- ? .NET Framework 4.7.2 or 4.8
- ? Bootstrap 4+ loaded in your project
- ? jQuery loaded
- ? Admin/Authority permissions

---

## ?? Installation Steps (????????? ???)

### Step 1: Database Setup

1. Open SQL Server Management Studio
2. Connect to your database
3. Open and execute this file:
   ```
   Database_Scripts\Create_SchoolInfo_DueNoticeSettings_Table.sql
   ```
4. Verify table created:
   ```sql
   SELECT * FROM SchoolInfo_DueNoticeSettings
   ```

**Expected Output:** Empty table with these columns:
- SettingID
- SchoolID
- IsHidden
- HideUntilDate
- Reason
- CreatedDate
- CreatedBy

---

### Step 2: Test the Feature

#### A. Test Due Invoice Notification on Admin Dashboard

1. Login to the system
2. Navigate to: `/Profile/Admin.aspx`
3. ??? ?????? invoice ????, ???? modal popup ??????:
   - ?????? ???????? ??????
   - ??? ?????? ??????
   - Due Invoice ????? ????

**Expected Behavior:**
- Modal automatically shows on page load
- Shows only if there are unpaid invoices
- Shows only if notification is not hidden

---

#### B. Test Hide Notification Feature (Authority Panel)

1. Login as Authority
2. Navigate to: `/Authority/Auth_Profile.aspx`
3. Click on any institution
4. Go to "Due Invoice Settings" tab
5. Test scenarios:

##### **Scenario 1: Permanently Hide**
- ? Check "?????? ????? ??????????? ?????"
- ? Leave date field empty
- ? Click "??????? ????"
- **Result:** Notification hidden permanently

##### **Scenario 2: Temporarily Hide**
- ? Check "?????? ????? ??????????? ?????"
- ? Select future date (e.g., 31 Jan 2025)
- ? Add reason: "??,??? ???? ?????? ?????"
- ? Click "??????? ????"
- **Result:** Notification hidden until selected date

##### **Scenario 3: Re-enable Notification**
- ? Uncheck "?????? ????? ??????????? ?????"
- **Result:** Notification enabled immediately

---

#### C. Verify Hiding Works

1. After hiding notification in Authority panel
2. Login to that institution's admin panel
3. Navigate to `/Profile/Admin.aspx`
4. **Expected:** No due invoice modal should appear

---

## ?? Testing Checklist

### ? Functionality Tests

- [ ] Modal shows when there are unpaid invoices
- [ ] Modal doesn't show when all invoices are paid
- [ ] Modal doesn't show when notification is hidden
- [ ] Checkbox toggles hide panel visibility
- [ ] Save button saves settings correctly
- [ ] Date picker works properly
- [ ] Current status displays correctly
- [ ] Auto-expiry works (after hide date passes)
- [ ] Re-enable works by unchecking

### ? UI/UX Tests

- [ ] Modal is responsive (mobile/desktop)
- [ ] Bengali text displays correctly
- [ ] Icons show properly
- [ ] Colors match the theme
- [ ] Date format is user-friendly
- [ ] Success/Error messages display

### ? Database Tests

```sql
-- Test 1: Insert hide setting
INSERT INTO SchoolInfo_DueNoticeSettings (SchoolID, IsHidden, HideUntilDate, Reason)
VALUES (1, 1, '2025-02-28', 'Test hide')

-- Test 2: Check if hidden
SELECT * FROM SchoolInfo_DueNoticeSettings WHERE SchoolID = 1 AND IsHidden = 1

-- Test 3: Update/Deactivate
UPDATE SchoolInfo_DueNoticeSettings SET IsHidden = 0 WHERE SchoolID = 1

-- Test 4: Clean up
DELETE FROM SchoolInfo_DueNoticeSettings WHERE SchoolID = 1
```

---

## ?? Common Use Cases

### Use Case 1: Institution Paid Partial Amount
**Scenario:** ?????????? ??,??? ???? ?????? ?????, ?????? ???? ?????? ????

**Solution:**
1. Authority panel-? ???
2. Institution Details ? Due Invoice Settings
3. Hide notification for 30 days
4. Reason: "??,??? ???? ?????? ?????"

### Use Case 2: Payment Agreement Made
**Scenario:** ???????????? ???? ??????? ??????? agree ??? ???????

**Solution:**
1. Hide notification until next payment date
2. Add reason: "????? ???????? ???????? ?????? ??????"

### Use Case 3: Technical Issue with Invoice
**Scenario:** Invoice calculation-? ?????? ???, fix ??? ??????

**Solution:**
1. Permanently hide until fixed
2. Reason: "Invoice calculation ??? ??? ?????"
3. Re-enable after fixing

---

## ?? Admin Queries (Useful SQL)

### See all schools with hidden notifications:
```sql
SELECT 
    s.SchoolID,
    s.SchoolName,
    dns.IsHidden,
    dns.HideUntilDate,
    dns.Reason,
    dns.CreatedDate,
    CASE 
        WHEN dns.HideUntilDate IS NULL THEN 'Permanent'
        WHEN dns.HideUntilDate > GETDATE() THEN 'Active'
        ELSE 'Expired'
    END AS Status
FROM SchoolInfo_DueNoticeSettings dns
INNER JOIN SchoolInfo s ON dns.SchoolID = s.SchoolID
WHERE dns.IsHidden = 1
ORDER BY dns.CreatedDate DESC
```

### Find schools with expiring hide dates:
```sql
SELECT 
    s.SchoolName,
    dns.HideUntilDate,
    DATEDIFF(day, GETDATE(), dns.HideUntilDate) AS DaysRemaining
FROM SchoolInfo_DueNoticeSettings dns
INNER JOIN SchoolInfo s ON dns.SchoolID = s.SchoolID
WHERE dns.IsHidden = 1
AND dns.HideUntilDate IS NOT NULL
AND dns.HideUntilDate > GETDATE()
AND DATEDIFF(day, GETDATE(), dns.HideUntilDate) <= 7
ORDER BY dns.HideUntilDate
```

### Cleanup expired settings automatically:
```sql
UPDATE SchoolInfo_DueNoticeSettings 
SET IsHidden = 0 
WHERE IsHidden = 1 
AND HideUntilDate IS NOT NULL 
AND HideUntilDate < GETDATE()
```

---

## ?? Troubleshooting

### Problem: Modal not showing
**Possible Causes:**
1. No unpaid invoices exist
2. Notification is hidden
3. JavaScript error

**Debug Steps:**
```javascript
// Open browser console and check:
console.log($('#dueInvoiceModal').length); // Should be 1
console.log($('#dueRecordCount').text()); // Should show count
$('#dueInvoiceModal').modal('show'); // Manually trigger
```

### Problem: Checkbox not saving
**Check:**
1. Database connection string correct?
2. SchoolID in QueryString?
3. Check browser console for errors
4. Verify table exists

**SQL Debug:**
```sql
-- Check if setting exists
SELECT * FROM SchoolInfo_DueNoticeSettings WHERE SchoolID = YOUR_SCHOOL_ID
```

### Problem: Date picker not working
**Solutions:**
1. Verify Bootstrap datepicker is loaded:
   ```javascript
   console.log($.fn.datepicker); // Should not be undefined
   ```
2. Check if CSS class "datepicker" is applied
3. Re-initialize after UpdatePanel:
   ```javascript
   Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
       $('.datepicker').datepicker({
           format: 'dd M yyyy',
           autoclose: true
       });
   });
   ```

---

## ?? Browser Compatibility

Tested and working on:
- ? Chrome (Latest)
- ? Firefox (Latest)
- ? Edge (Latest)
- ? Safari (Latest)
- ? Mobile browsers

---

## ?? Security Considerations

1. **Permission Check:** Only Authority users should access Institution_Details.aspx
2. **SQL Injection:** All queries use parameterized commands ?
3. **Session Validation:** SchoolID verified from session
4. **XSS Protection:** User inputs are sanitized

---

## ?? Performance Tips

1. **Index exists** on SchoolID and IsHidden columns
2. **TOP 1** used in queries for efficiency
3. **Cached queries** for repeated checks
4. **Async operations** where possible

---

## ?? Best Practices

### For Authority Users:
1. ? Always add a reason when hiding
2. ? Set specific dates rather than permanent hide
3. ? Review hidden notifications weekly
4. ? Re-enable after issue is resolved

### For Developers:
1. ? Test on different screen sizes
2. ? Check browser console for errors
3. ? Verify database transactions complete
4. ? Keep documentation updated

---

## ?? Support & Contact

**Documentation:** See `Documentation\DueInvoiceNotification_Feature_Documentation.md`

**Need Help?** Contact your system administrator.

---

## ? Success Checklist

Before going live, ensure:

- [ ] Database table created
- [ ] All files compiled without errors
- [ ] Modal shows correctly
- [ ] Hide feature works
- [ ] Date expiry works
- [ ] Re-enable works
- [ ] Tested on production-like data
- [ ] Authority users trained
- [ ] Documentation provided

---

**Ready to use! ??**

Your Due Invoice Notification System is now fully functional!

---

**Version:** 1.0  
**Last Updated:** January 2025  
**Status:** ? Production Ready
