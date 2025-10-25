# Due Invoice Notification System - Feature Documentation

## বৈশিষ্ট্য সংক্ষিপ্ত বিবরণ (Feature Overview)

এই ফিচারটি প্রতিষ্ঠানের বকেয়া invoice এর নোটিফিকেশন ম্যানেজমেন্ট সিস্টেম।

### ✨ মূল বৈশিষ্ট্য (Key Features):

1. **Auto Due Invoice Notification** - Admin পেজ লোড হওয়ার সময় স্বয়ংক্রিয়ভাবে বকেয়া নোটিশ দেখায়
2. **Hide Notification Option** - Authority প্রতিষ্ঠানের জন্য সাময়িকভাবে নোটিশ বন্ধ রাখতে পারে
3. **Date-based Auto Enable** - নির্দিষ্ট তারিখের পর স্বয়ংক্রিয়ভাবে নোটিশ আবার চালু হয়
4. **Reason Tracking** - কেন নোটিশ বন্ধ করা হয়েছে তার কারণ সংরক্ষণ

---

## 📁 পরিবর্তিত ফাইলসমূহ (Modified Files)

### 1. **Admin.aspx** (`SIKKHALOY V2\Profile\Admin.aspx`)
   - Bootstrap Modal পপআপ যোগ করা হয়েছে
   - বকেয়া রেকর্ডের সংখ্যা এবং মোট টাকা দেখায়
   - Due Invoice পেজে যাওয়ার লিংক সহ

### 2. **Admin.aspx.cs** (`SIKKHALOY V2\Profile\Admin.aspx.cs`)
   **যোগকৃত Methods:**
   - `CheckAndShowDueInvoiceNotification()` - বকেয়া চেক করে পপআপ দেখায়
   - `IsDueNoticeHidden()` - নোটিশ hide করা আছে কিনা চেক করে
   - `DeactivateExpiredDueNoticeSetting()` - expired settings deactivate করে

### 3. **Institution_Details.aspx** (`SIKKHALOY V2\Authority\Institutions\Institution_Details.aspx`)
   - নতুন "Due Invoice Settings" tab যোগ করা হয়েছে
   - Hide notification checkbox
   - Date picker এবং reason text box
   - Current status display

### 4. **Institution_Details.aspx.cs** (`SIKKHALOY V2\Authority\Institutions\Institution_Details.aspx.cs`)
   **যোগকৃত Methods:**
   - `LoadDueNoticeSettings()` - page load এ settings load করে
   - `HideDueNoticeCheckBox_CheckedChanged()` - checkbox event handler
   - `SaveDueSettingsButton_Click()` - settings save করে
   - `RemoveDueNoticeHideSetting()` - hide setting remove করে

---

## 🗄️ Database Setup

### Required Table: `SchoolInfo_DueNoticeSettings`

**Table Structure:**
```sql
CREATE TABLE SchoolInfo_DueNoticeSettings (
    SettingID INT IDENTITY(1,1) PRIMARY KEY,
    SchoolID INT NOT NULL,
    IsHidden BIT NOT NULL DEFAULT 0,
    HideUntilDate DATETIME NULL,
    Reason NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL
)
```

**Installation Steps:**
1. Database এ SQL script চালান: `Database_Scripts\Create_SchoolInfo_DueNoticeSettings_Table.sql`
2. Table এবং index স্বয়ংক্রিয়ভাবে তৈরি হবে

---

## 🚀 কিভাবে ব্যবহার করবেন (How to Use)

### Authority এর জন্য:

1. **Auth_Profile.aspx** থেকে কোন প্রতিষ্ঠানে ক্লিক করুন
2. **Institution_Details.aspx** পেজে যান
3. **"Due Invoice Settings"** tab-এ ক্লিক করুন
4. **"বকেয়া নোটিশ সাময়িকভাবে লুকান"** checkbox চেক করুন
5. (ঐচ্ছিক) তারিখ এবং কারণ লিখুন
6. **"সংরক্ষণ করুন"** বাটনে ক্লিক করুন

### Options:

#### স্থায়ীভাবে Hide করতে:
- শুধু checkbox চেক করুন
- তারিখ খালি রাখুন
- Save করুন

#### সাময়িকভাবে Hide করতে:
- Checkbox চেক করুন
- তারিখ নির্বাচন করুন (যেমন: 31 Jan 2025)
- Save করুন
- নির্দিষ্ট তারিখের পর স্বয়ংক্রিয়ভাবে notification চালু হবে

#### আবার Enable করতে:
- Checkbox uncheck করুন
- স্বয়ংক্রিয়ভাবে database থেকে remove হয়ে যাবে

---

## 🔍 কিভাবে কাজ করে (How It Works)

### Flow Diagram:

```
Admin.aspx Page Load
    ↓
CheckAndShowDueInvoiceNotification()
    ↓
IsDueNoticeHidden()?
    ↓
    ├── Yes → Return (No notification)
    │
    └── No → Query Due Invoices
            ↓
            Has Due Amount?
            ↓
            ├── Yes → Show Modal Popup
            │
            └── No → No Action
```

### Hide Logic:

```
IsDueNoticeHidden()
    ↓
Query SchoolInfo_DueNoticeSettings
    ↓
    ├── No Record → Return False (Show notification)
    │
    ├── HideUntilDate = NULL → Return True (Hide permanently)
    │
    └── HideUntilDate Set → Check Date
            ↓
            ├── Date Not Passed → Return True (Hide)
            │
            └── Date Passed → Deactivate Setting
                             → Return False (Show)
```

---

## 📊 Database Query Examples

### Check if notification is hidden for a school:
```sql
SELECT TOP 1 * 
FROM SchoolInfo_DueNoticeSettings 
WHERE SchoolID = 123 
AND IsHidden = 1 
ORDER BY CreatedDate DESC
```

### Get all schools with hidden notifications:
```sql
SELECT s.SchoolName, dns.HideUntilDate, dns.Reason, dns.CreatedDate
FROM SchoolInfo_DueNoticeSettings dns
INNER JOIN SchoolInfo s ON dns.SchoolID = s.SchoolID
WHERE dns.IsHidden = 1
ORDER BY dns.CreatedDate DESC
```

### Find expired hide settings:
```sql
SELECT * 
FROM SchoolInfo_DueNoticeSettings 
WHERE IsHidden = 1 
AND HideUntilDate IS NOT NULL 
AND HideUntilDate < GETDATE()
```

---

## 🎨 UI Screenshots Description

### Admin Dashboard Modal:
- সুন্দর Bootstrap modal popup
- বাংলা টেক্সট সহ
- বকেয়া রেকর্ড সংখ্যা badge-এ
- মোট বকেয়া পরিমাণ highlighted
- Due Invoice পেজে যাওয়ার button

### Institution Details - Due Invoice Settings Tab:
- Info alert box
- Checkbox for hide/show
- Date picker (Bootstrap datepicker)
- Reason textarea
- Save button
- Current status display panel

---

## ⚠️ Important Notes

1. **Database Table প্রথমে তৈরি করুন** - SQL script চালান
2. **Session Variables** - নিশ্চিত করুন যে `SchoolID` session-এ আছে
3. **Date Format** - "dd MMM yyyy" format ব্যবহার করুন (যেমন: 31 Jan 2025)
4. **Auto Expiry** - System স্বয়ংক্রিয়ভাবে expired settings deactivate করে
5. **Bootstrap Required** - Modal এর জন্য Bootstrap 4+ দরকার

---

## 🔧 Troubleshooting

### Notification দেখাচ্ছে না:
- Check করুন database table আছে কিনা
- Session["SchoolID"] set আছে কিনা verify করুন
- Browser console-এ JavaScript error আছে কিনা দেখুন

### Checkbox কাজ করছে না:
- AutoPostBack="True" আছে কিনা চেক করুন
- UpdatePanel ঠিকমত configure করা আছে কিনা দেখুন

### Date picker কাজ করছে না:
- Bootstrap datepicker JavaScript loaded আছে কিনা চেক করুন
- CSS class "datepicker" apply করা আছে কিনা দেখুন

---

## 📝 Future Enhancements (ভবিষ্যত উন্নতি)

1. ✅ Email notification when hide period expires
2. ✅ Hide notification for specific invoice categories
3. ✅ Bulk hide/show for multiple schools
4. ✅ Report showing all hidden notifications
5. ✅ SMS alert before hide period expires

---

## 👨‍💻 Developer Information

**Version:** 1.0
**Created:** January 2025
**Framework:** .NET Framework 4.7.2 / 4.8
**Database:** SQL Server

---

## 📞 Support

কোন সমস্যা হলে বা প্রশ্ন থাকলে যোগাযোগ করুন।

---

**Developed with ❤️ for SIKKHALOY Education Management System**
