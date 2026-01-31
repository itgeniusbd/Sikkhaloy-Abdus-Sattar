# ?? SMS Bangla Encoding Troubleshooting Guide

## ?? ??????: ???? "?????" ????????

??? SMS ? ???? ????? text ?? ???????? **"?????"** ??????, ????? ????? steps follow ????:

---

## ?? Troubleshooting Checklist

### ? **Step 1: Application Restart ????**

#### **Option A: IIS Restart (??? IIS ??????? ????)**
```powershell
# PowerShell (Run as Administrator)
iisreset /restart
```

#### **Option B: Application Pool Restart**
1. IIS Manager ?????
2. Application Pools ? ???
3. ????? app ?? pool ??????? ????
4. Right click ? **Recycle** ????

#### **Option C: Visual Studio ????**
1. Stop Debugging (Shift + F5)
2. Clean Solution (Build ? Clean Solution)
3. Rebuild Solution (Build ? Rebuild Solution)
4. Start Debugging (F5)

---

### ? **Step 2: DLL Cache Clear ????**

```powershell
# PowerShell
cd "F:\SIKKHALOY-V3\SIKKHALOY V2\bin"
Remove-Item SmsService.dll -Force
Remove-Item SmsService.pdb -Force
```

Then rebuild:
```powershell
cd "F:\SIKKHALOY-V3"
# Rebuild the solution
```

---

### ? **Step 3: Browser Cache Clear ????**

1. Chrome/Edge: `Ctrl + Shift + Delete`
2. Clear cached images and files
3. Close and reopen browser
4. **Hard Refresh**: `Ctrl + F5`

---

### ? **Step 4: SMS Gateway Settings Check ????**

#### **Green Web API Settings:**
SMS Gateway dashboard ? check ????:
- ? **Unicode SMS** enable ??? ????
- ? **Character Encoding** = UTF-8
- ? **API Permission** ??? ??? ????

#### **Contact Green Web Support:**
- Email: support@greenweb.com.bd
- Phone: +880 1844532340
- ????: "I need to enable Unicode/Bangla SMS support for my API"

---

### ? **Step 5: Database ? SMS Record Check ????**

```sql
-- Check recent SMS records
SELECT TOP 10 
    SMS_Send_ID,
    PhoneNumber,
    TextSMS,
    Status,
    SMS_Response,
    Date
FROM SMS_Send_Record
ORDER BY Date DESC
```

**Check ????:**
- `TextSMS` column ? ????? text ??? ??? ????
- ??? database ? ??? ???? ?????? mobile ? ??? ????, ????? Gateway ?? ??????

---

### ? **Step 6: Manual Test ????**

#### **Option A: Postman ????? Direct API Call**

```http
POST https://api.greenweb.com.bd/api.php?json
Content-Type: application/x-www-form-urlencoded; charset=utf-8

token=90282210471675095047ee665e3d0ba098844814cab35e133dc4
&to=01XXXXXXXXX
&message=???????: ???? ??? ?????? ???????
```

#### **Option B: cURL Command**

```bash
curl -X POST "https://api.greenweb.com.bd/api.php?json" \
  -H "Content-Type: application/x-www-form-urlencoded; charset=utf-8" \
  -d "token=90282210471675095047ee665e3d0ba098844814cab35e133dc4" \
  -d "to=01XXXXXXXXX" \
  -d "message=???????: ????? ??????"
```

---

### ? **Step 7: SMS Provider Change ???? (Temporary Test)**

`SikkhaloySetting` table ?:
```sql
UPDATE SikkhaloySetting
SET SmsProvider = 'BanglaPhone'  -- Change from GreenWeb
```

Then test ???? - ??? BanglaPhone ? ??? ??? ????? GreenWeb ?? configuration issue?

---

## ?? Code Verification

### **Current Code Status:**

#### ? **SmsProviderGreenWeb.cs** - UPDATED
```csharp
var smsText = Uri.EscapeDataString(safeMassage);  // ? Handles Unicode
var data = Encoding.UTF8.GetBytes(urlEncodedData); // ? UTF-8 encoding
request.ContentType = "application/x-www-form-urlencoded; charset=utf-8"; // ? Charset specified
```

#### ? **Key Changes:**
1. ? `Uri.EscapeDataString()` - Properly handles Unicode (?????)
2. ? `Encoding.UTF8.GetBytes()` - UTF-8 byte encoding
3. ? `charset=utf-8` - Content-Type header updated
4. ? `A+` ? `A Plus` - Symbol handling

---

## ?? Testing Steps

### **Test 1: Simple Bangla SMS**
```
Message: ???????
Expected: ???????
```

### **Test 2: Mixed English + Bangla**
```
Message: Dear, ?????? ???????. Thank you.
Expected: Dear, ?????? ???????. Thank you.
```

### **Test 3: Donor Due Message**
```
Message: Dear, ??? ?????? ???????? ??? ????. ????? ?????? ??????: 9000 ????
Expected: Same as input (no ?????)
```

---

## ?? Debug Mode Enable ????

### **Add Debugging in Donor_Present_Due.aspx.cs:**

```csharp
// Already added - Check Visual Studio Output window
System.Diagnostics.Debug.WriteLine("=== SMS DEBUG ===");
System.Diagnostics.Debug.WriteLine("Original Message: " + Msg);
System.Diagnostics.Debug.WriteLine("UTF-8 Bytes: " + BitConverter.ToString(Encoding.UTF8.GetBytes(Msg)));
```

**View Output:**
1. Visual Studio ? View ? Output (Ctrl + W, O)
2. Select "Debug" from dropdown
3. Run SMS send
4. Check console output

---

## ?? Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| "?????" in SMS | Encoding issue | ? Already fixed - Restart app |
| "?????" in DB too | Database collation | Change column to `NVARCHAR` |
| Gateway error | API not configured | Contact Green Web support |
| Works in test, fails in prod | Cached DLL | Clear bin folder + rebuild |
| A+ becomes "A Plus" | Expected behavior | This is correct for SMS |

---

## ?? If Still Not Working

### **Last Resort Steps:**

#### **1. Check SMS Provider Configuration:**
```sql
SELECT SmsProvider, SmsProviderMultiple 
FROM SikkhaloySetting
```

**Should be:**
- `SmsProvider` = `GreenWeb` OR `BanglaPhone`
- `SmsProviderMultiple` = `GreenWeb`

#### **2. Re-register SMS Gateway IP:**
- Green Web ?? dashboard ? login ????
- API Settings ? Whitelist IPs
- ????? server ?? IP add ????

#### **3. Check SMS Balance:**
```sql
SELECT SMS_Balance FROM SMS WHERE SchoolID = @YourSchoolID
```

#### **4. Test with Different Gateway:**
- Temporarily use **BanglaPhone** provider
- If works ? GreenWeb configuration issue
- If not works ? Code issue

---

## ?? Support Contacts

### **Green Web SMS:**
- Website: https://greenweb.com.bd
- Email: support@greenweb.com.bd
- Phone: +880 1844532340

### **BanglaPhone (Alternative):**
- Website: http://powersms.net.bd
- Support: support@powersms.net.bd

---

## ? Success Verification

### **How to confirm it's working:**

1. ? Database ? `TextSMS` column ? ????? properly save ?????
2. ? Mobile ? SMS receive ?????
3. ? Mobile ? ????? text properly display ????? (no ?????)
4. ? Debug output ? UTF-8 bytes ??? ???

### **Expected Debug Output:**
```
=== SMS DEBUG ===
Original Message: Dear, ?????? ???????. ????? ??????: 9000 ????
UTF-8 Bytes: 44-65-61-72-2C-20-E0-A6-86-E0-A6-AC-...
```

---

## ?? Next Steps

1. ? Application restart ???? (IIS/AppPool)
2. ? Browser cache clear ????
3. ? Test SMS ?????
4. ? Mobile ? check ????
5. ? ??? ???? ?????? ? Green Web support contact ????

---

## ?? Notes

### **Important:**
- `Uri.EscapeDataString()` .NET Framework 4.7.2+ ?? Unicode properly handle ???
- Green Web API must support Unicode SMS (check with them)
- Some SMS gateways charge extra for Unicode SMS
- Bangla SMS typically counts as 2-3 SMS credits per message

### **Alternative if GreenWeb doesn't support Unicode:**
```sql
-- Switch to BanglaPhone temporarily
UPDATE SikkhaloySetting
SET SmsProvider = 'BanglaPhone'
```

---

**??? ?? ?? steps follow ???? ???? ??? ?? ???, ?????:**
1. Green Web ?? email ???? Unicode support ?? ????
2. ???? BanglaPhone provider use ????
3. ???? alternative SMS gateway consider ???? ???? Bangla support ???

**Good Luck!** ??
