# Monthly Student Count & Invoice Auto-Generation Setup Guide

## সারাংশ (Summary)

এই সিস্টেম **অটোমেটিকভাবে** প্রতি মাসের ১ তারিখে:
1. ✅ সব institution এর student count করবে
2. ✅ Service Charge invoice তৈরি করবে
3. ✅ যদি অটো কাজ না করে, UI তে button click করে manual করা যাবে

---

## Installation Steps

### Step 1: Database Setup (প্রথম ধাপ)

1. **SQL Server Management Studio (SSMS) খুলুন**
2. আপনার database select করুন
3. `Database Scripts/Monthly_Auto_Process.sql` file টি open করুন
4. **পুরো script টি run করুন** (F5 press করুন)

এটি তৈরি করবে:
- ✅ `sp_Generate_Monthly_Student_Count` - Student count করার procedure
- ✅ `sp_Generate_Monthly_Invoices` - Invoice তৈরি করার procedure  
- ✅ `sp_Monthly_Auto_Process` - দুটিই একসাথে চালানোর procedure
- ✅ `AAP_Auto_Process_Log` - Log সংরক্ষণের table

---

### Step 2: Testing (পরীক্ষা করুন)

#### Test 1: Current Month এর জন্য Student Count তৈরি করুন

```sql
DECLARE @Count INT, @Msg NVARCHAR(500)

EXEC sp_Generate_Monthly_Student_Count 
    @TargetMonth = NULL, -- NULL = Current month
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT

PRINT 'Result: ' + @Msg
PRINT 'Institutions processed: ' + CAST(@Count AS NVARCHAR)
```

#### Test 2: Current Month এর জন্য Invoice তৈরি করুন

```sql
DECLARE @Count INT, @Msg NVARCHAR(500)

EXEC sp_Generate_Monthly_Invoices 
    @TargetMonth = NULL, -- NULL = Current month
    @IssueDate = NULL, -- NULL = 1st of month
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT

PRINT 'Result: ' + @Msg
PRINT 'Invoices created: ' + CAST(@Count AS NVARCHAR)
```

#### Test 3: Full Auto Process চালান

```sql
-- Current month এর জন্য
EXEC sp_Monthly_Auto_Process @TargetMonth = NULL

-- অথবা নির্দিষ্ট মাসের জন্য (যেমন: March 2026)
EXEC sp_Monthly_Auto_Process @TargetMonth = '2026-03-01'
```

#### Test 4: Logs দেখুন

```sql
SELECT TOP 20 
    LogID,
    ProcessDate,
    ProcessMonth,
    ProcessType,
    LogMessage
FROM AAP_Auto_Process_Log 
ORDER BY ProcessDate DESC
```

---

### Step 3: SQL Server Agent Job Setup (অটোমেশন সেটআপ)

এটি **প্রতি মাসের ১ তারিখ রাত ২টায় অটোমেটিক চলবে**।

#### Option A: SSMS GUI দিয়ে (সহজ পদ্ধতি)

1. SSMS তে **SQL Server Agent** expand করুন
2. **Jobs** এ right-click → **New Job**
3. **General Tab**:
   - Name: `Monthly Student Count and Invoice Generation`
   - Enabled: ✓ (check করুন)
   - Description: `Automatically generates student count and invoices on 1st of every month`

4. **Steps Tab** → **New**:
   - Step name: `Run Auto Process`
   - Type: `Transact-SQL script (T-SQL)`
   - Database: আপনার database select করুন
   - Command:
     ```sql
     EXEC sp_Monthly_Auto_Process @TargetMonth = NULL
     ```
   - Click **OK**

5. **Schedules Tab** → **New**:
   - Name: `Monthly on 1st at 2AM`
   - Schedule type: `Recurring`
   - Frequency: 
     - Occurs: `Monthly`
     - Day: `1` (1st day of month)
   - Daily frequency:
     - Occurs once at: `02:00:00` (রাত ২টা)
   - Click **OK**

6. Main dialog এ **OK** click করুন

#### Option B: SQL Script দিয়ে (দ্রুত পদ্ধতি)

```sql
USE msdb;
GO

-- Create job
EXEC dbo.sp_add_job
    @job_name = N'Monthly Student Count and Invoice Generation',
    @enabled = 1,
    @description = N'Automatically generates student count and invoices on 1st of every month';

-- Add job step
EXEC dbo.sp_add_jobstep
    @job_name = N'Monthly Student Count and Invoice Generation',
    @step_name = N'Run Auto Process',
    @subsystem = N'TSQL',
    @command = N'EXEC sp_Monthly_Auto_Process @TargetMonth = NULL',
    @database_name = N'YourDatabaseName', -- ⚠️ আপনার database name দিন
    @retry_attempts = 3,
    @retry_interval = 5;

-- Schedule to run on 1st of every month at 2:00 AM
EXEC dbo.sp_add_schedule
    @schedule_name = N'Monthly on 1st at 2AM',
    @freq_type = 16, -- Monthly
    @freq_interval = 1, -- 1st day of month
    @active_start_time = 020000; -- 2:00 AM

-- Attach schedule to job
EXEC dbo.sp_attach_schedule
    @job_name = N'Monthly Student Count and Invoice Generation',
    @schedule_name = N'Monthly on 1st at 2AM';

-- Add job to local server
EXEC dbo.sp_add_jobserver
    @job_name = N'Monthly Student Count and Invoice Generation',
    @server_name = N'(local)';

PRINT 'SQL Agent Job created successfully!'
```

**⚠️ গুরুত্বপূর্ণ**: `@database_name = N'YourDatabaseName'` এই লাইনে আপনার আসল database name দিন।

---

### Step 4: Manual Testing (Job পরীক্ষা করুন)

Job তৈরি হওয়ার পর manually run করে দেখুন:

1. SSMS → **SQL Server Agent** → **Jobs**
2. `Monthly Student Count and Invoice Generation` job খুঁজুন
3. Right-click → **Start Job at Step...**
4. Run হওয়ার পর **View History** দেখুন

---

## কীভাবে কাজ করে (How It Works)

### Automatic Process (অটোমেটিক)

```
প্রতি মাসের ১ তারিখ রাত ২টায়:
  ↓
1. Student Count Generate হবে
  ↓
2. Invoice Generate হবে
  ↓
3. Log table এ result save হবে
```

### Manual Process (যদি অটো fail করে)

আপনার UI তে ইতিমধ্যে আছে:
1. **"Generate Student Count" button** → Student count তৈরি করবে
2. **Month select + Submit button** → Invoice তৈরি করবে

---

## Features (বৈশিষ্ট্য)

### ✅ Automatic Features

1. **Duplicate Prevention**: একই মাসে দুইবার generate হবে না
2. **Error Handling**: যদি error হয় log এ save হবে
3. **Retry Logic**: 3 বার পর্যন্ত retry করবে
4. **Committee Count**: Committee members ও billing এ যুক্ত হবে
5. **Active Only**: শুধু active institutions এর invoice হবে
6. **Fixed/Variable Rate**: Institution এর rate অনুযায়ী calculate হবে

### ✅ Manual Backup

যদি কোনো মাসে automatic process fail করে:
1. UI তে login করুন
2. **Create Invoice** page এ যান
3. **"Generate Student Count"** button click করুন
4. মাস select করুন এবং **Generate** click করুন
5. তারপর normal process এ invoice তৈরি করুন

---

## Monitoring (পর্যবেক্ষণ)

### Log দেখুন

```sql
-- Last 30 days logs
SELECT 
    FORMAT(ProcessDate, 'dd MMM yyyy hh:mm tt') AS ProcessTime,
    FORMAT(ProcessMonth, 'MMM yyyy') AS ForMonth,
    ProcessType,
    LogMessage
FROM AAP_Auto_Process_Log 
WHERE ProcessDate >= DATEADD(DAY, -30, GETDATE())
ORDER BY ProcessDate DESC
```

### Job History দেখুন

1. SSMS → **SQL Server Agent** → **Jobs**
2. Right-click on job → **View History**
3. Success/Failure দেখুন

### Email Notification Setup (Optional)

যদি চান job fail হলে email পাবেন:

```sql
-- Configure Database Mail first, then:
EXEC msdb.dbo.sp_update_job 
    @job_name = N'Monthly Student Count and Invoice Generation',
    @notify_level_email = 2, -- On failure
    @notify_email_operator_name = N'YourOperatorName'
```

---

## Troubleshooting (সমস্যা সমাধান)

### সমস্যা 1: Job চলছে না

**সমাধান**:
1. SQL Server Agent Service চালু আছে কিনা check করুন
   - Services → SQL Server Agent → Start
2. Job enabled আছে কিনা check করুন
3. Job history দেখুন error message এর জন্য

### সমস্যা 2: Student Count empty আসছে

**সমাধান**:
1. Education Year active আছে কিনা check করুন
   ```sql
   SELECT * FROM Education_Year 
   WHERE GETDATE() BETWEEN StartDate AND EndDate
   ```
2. StudentsClass table এ data আছে কিনা check করুন

### সমস্যা 3: Invoice তৈরি হচ্ছে না

**সমাধান**:
1. Student count আছে কিনা check করুন
2. Service Charge category আছে কিনা check করুন
   ```sql
   SELECT * FROM AAP_Invoice_Category 
   WHERE InvoiceCategory = N'Service Charge'
   ```
3. Institution active আছে কিনা check করুন
   ```sql
   SELECT SchoolID, SchoolName, IS_ServiceChargeActive
   FROM SchoolInfo 
   WHERE IS_ServiceChargeActive = 0
   ```

---

## Manual Commands (প্রয়োজনে ব্যবহার করুন)

### নির্দিষ্ট মাসের জন্য generate করুন

```sql
-- Example: March 2026 এর জন্য
EXEC sp_Monthly_Auto_Process @TargetMonth = '2026-03-01'

-- Example: Previous month এর জন্য
DECLARE @LastMonth DATE = DATEADD(MONTH, -1, GETDATE())
EXEC sp_Monthly_Auto_Process @TargetMonth = @LastMonth
```

### Job manually run করুন

```sql
EXEC msdb.dbo.sp_start_job 
    @job_name = N'Monthly Student Count and Invoice Generation'
```

### Job disable করুন (temporarily)

```sql
EXEC msdb.dbo.sp_update_job 
    @job_name = N'Monthly Student Count and Invoice Generation',
    @enabled = 0
```

### Job enable করুন

```sql
EXEC msdb.dbo.sp_update_job 
    @job_name = N'Monthly Student Count and Invoice Generation',
    @enabled = 1
```

---

## Best Practices (সুপারিশ)

1. ✅ প্রতি মাসে **2-3 তারিখে** manually check করুন যে process সফল হয়েছে কিনা
2. ✅ **Log table** নিয়মিত দেখুন
3. ✅ **Database backup** নিয়মিত নিন
4. ✅ Test environment এ প্রথমে test করুন
5. ✅ Production এ deploy করার আগে manual test করুন

---

## Support Contact

কোনো সমস্যা হলে:
1. Log table check করুন
2. Job history check করুন  
3. Manual process দিয়ে test করুন
4. Database administrator কে জানান

---

## Version History

- **v1.0** (2025): Initial release with automatic student count and invoice generation
