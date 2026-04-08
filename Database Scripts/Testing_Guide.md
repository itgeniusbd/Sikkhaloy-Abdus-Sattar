# Testing & Monitoring Guide
## কীভাবে পরীক্ষা করবেন এবং অটো process কাজ করছে কিনা বুঝবেন

---

## ✅ Part 1: Manual Testing (এখনই পরীক্ষা করুন)

### Method 1: UI থেকে Manual Test (সবচেয়ে সহজ)

1. **Application restart করুন** (debugging বন্ধ করে আবার চালান)
2. **Create Invoice page** এ যান
3. **"Generate Student Count"** button click করুন
4. Month field এ **"March 2026"** বা **"Mar 2026"** লিখুন
5. **Generate** button click করুন
6. Success message দেখবেন এবং dropdown এ March 2026 আসবে

### Method 2: SQL থেকে Direct Test

SSMS এ এই command run করুন:

```sql
-- Test 1: Current month এর জন্য
DECLARE @Count INT, @Msg NVARCHAR(500)
EXEC sp_Generate_Monthly_Student_Count 
    @TargetMonth = NULL,
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT
PRINT 'Count: ' + CAST(@Count AS NVARCHAR(10))
PRINT 'Message: ' + @Msg
```

```sql
-- Test 2: March 2026 এর জন্য
DECLARE @Count INT, @Msg NVARCHAR(500)
EXEC sp_Generate_Monthly_Student_Count 
    @TargetMonth = '2026-03-01',
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT
PRINT 'Count: ' + CAST(@Count AS NVARCHAR(10))
PRINT 'Message: ' + @Msg
```

```sql
-- Test 3: Full process (Student Count + Invoice)
EXEC sp_Monthly_Auto_Process @TargetMonth = '2026-03-01'
```

---

## 🔍 Part 2: অটো Process Monitoring (কাজ করছে কিনা check করুন)

### Step 1: SQL Agent Job Check করুন

```sql
-- Check if job exists
SELECT 
    name AS JobName,
    enabled AS IsEnabled,
    date_created AS CreatedDate,
    date_modified AS ModifiedDate
FROM msdb.dbo.sysjobs 
WHERE name LIKE '%Monthly%'
```

**Expected Result:**
- যদি job তৈরি থাকে তাহলে 1 row দেখাবে
- `IsEnabled` = 1 হতে হবে
- যদি কিছু না আসে = job এখনও তৈরি হয়নি

### Step 2: Job Schedule Check করুন

```sql
-- Check job schedule
SELECT 
    j.name AS JobName,
    s.name AS ScheduleName,
    CASE s.freq_type
        WHEN 1 THEN 'Once'
        WHEN 4 THEN 'Daily'
        WHEN 8 THEN 'Weekly'
        WHEN 16 THEN 'Monthly'
        WHEN 32 THEN 'Monthly (relative)'
    END AS Frequency,
    s.freq_interval AS DayOfMonth,
    RIGHT('0' + CAST(s.active_start_time / 10000 AS VARCHAR), 2) + ':' +
    RIGHT('0' + CAST((s.active_start_time % 10000) / 100 AS VARCHAR), 2) AS StartTime,
    s.enabled AS ScheduleEnabled
FROM msdb.dbo.sysjobs j
INNER JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
INNER JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
WHERE j.name LIKE '%Monthly%'
```

**Expected Result:**
```
Frequency: Monthly
DayOfMonth: 1 (1st day of month)
StartTime: 02:00 (2 AM)
ScheduleEnabled: 1
```

### Step 3: Job History দেখুন

```sql
-- Last 10 executions
SELECT TOP 10
    j.name AS JobName,
    h.step_name AS StepName,
    CONVERT(VARCHAR(20), 
        CAST(CAST(h.run_date AS VARCHAR(8)) AS DATETIME), 120) AS RunDate,
    RIGHT('0' + CAST(h.run_time / 10000 AS VARCHAR), 2) + ':' +
    RIGHT('0' + CAST((h.run_time % 10000) / 100 AS VARCHAR), 2) AS RunTime,
    CASE h.run_status
        WHEN 0 THEN '❌ Failed'
        WHEN 1 THEN '✅ Succeeded'
        WHEN 2 THEN '⚠️ Retry'
        WHEN 3 THEN '⏹️ Canceled'
        WHEN 4 THEN '🏃 In Progress'
    END AS Status,
    h.run_duration AS DurationSeconds,
    LEFT(h.message, 200) AS Message
FROM msdb.dbo.sysjobhistory h
INNER JOIN msdb.dbo.sysjobs j ON h.job_id = j.job_id
WHERE j.name LIKE '%Monthly%'
ORDER BY h.run_date DESC, h.run_time DESC
```

### Step 4: Process Logs দেখুন

```sql
-- Last 30 days logs
SELECT 
    LogID,
    FORMAT(ProcessDate, 'dd MMM yyyy hh:mm tt') AS ProcessTime,
    CASE WHEN ProcessMonth IS NOT NULL 
        THEN FORMAT(ProcessMonth, 'MMM yyyy') 
        ELSE 'N/A' 
    END AS ForMonth,
    ProcessType,
    CASE 
        WHEN LogMessage LIKE '%Success%' THEN '✅ ' + LogMessage
        WHEN LogMessage LIKE '%error%' OR LogMessage LIKE '%Error%' THEN '❌ ' + LogMessage
        WHEN LogMessage LIKE '%Skipped%' THEN '⚠️ ' + LogMessage
        WHEN LogMessage LIKE '%already exists%' THEN '⚡ ' + LogMessage
        ELSE '📝 ' + LogMessage
    END AS Status
FROM AAP_Auto_Process_Log 
WHERE ProcessDate >= DATEADD(DAY, -30, GETDATE())
ORDER BY ProcessDate DESC
```

### Step 5: Data Verification (আসলেই data তৈরি হয়েছে কিনা)

```sql
-- Check last 6 months student count
SELECT 
    FORMAT(Month, 'MMM yyyy') AS MonthName,
    COUNT(DISTINCT SchoolID) AS TotalInstitutions,
    SUM(ISNULL(StudentCount, 0)) AS TotalStudents,
    SUM(ISNULL(Active_Student, 0)) AS ActiveStudents
FROM AAP_Student_Count_Monthly
WHERE Month >= DATEADD(MONTH, -6, GETDATE())
GROUP BY Month
ORDER BY Month DESC
```

```sql
-- Check invoices generated
SELECT 
    FORMAT(MonthName, 'MMM yyyy') AS InvoiceMonth,
    COUNT(*) AS TotalInvoices,
    SUM(TotalAmount) AS TotalAmount,
    SUM(CASE WHEN IsPaid = 1 THEN 1 ELSE 0 END) AS PaidInvoices,
    SUM(CASE WHEN IsPaid = 0 OR IsPaid IS NULL THEN 1 ELSE 0 END) AS UnpaidInvoices
FROM AAP_Invoice
WHERE InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
AND MonthName >= DATEADD(MONTH, -6, GETDATE())
GROUP BY MonthName
ORDER BY MonthName DESC
```

---

## 🎯 Part 3: অটো Process এর Status বুঝুন

### Scenario 1: Job এখনও তৈরি হয়নি ❌

**Symptom:**
```sql
SELECT * FROM msdb.dbo.sysjobs WHERE name LIKE '%Monthly%'
-- Returns: 0 rows
```

**Solution:**
1. `Monthly_Auto_Process.sql` file এর শেষে job creation script আছে
2. Comment (`/* */`) সরিয়ে দিন
3. `@database_name` এ আপনার database name দিন
4. Run করুন

```sql
USE msdb;
GO

EXEC dbo.sp_add_job
    @job_name = N'Monthly Student Count and Invoice Generation',
    @enabled = 1;

EXEC dbo.sp_add_jobstep
    @job_name = N'Monthly Student Count and Invoice Generation',
    @step_name = N'Run Auto Process',
    @command = N'EXEC sp_Monthly_Auto_Process',
    @database_name = N'Edu'; -- আপনার database name

EXEC dbo.sp_add_schedule
    @schedule_name = N'Monthly on 1st at 2AM',
    @freq_type = 16,
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC dbo.sp_attach_schedule
    @job_name = N'Monthly Student Count and Invoice Generation',
    @schedule_name = N'Monthly on 1st at 2AM';

EXEC dbo.sp_add_jobserver
    @job_name = N'Monthly Student Count and Invoice Generation';
```

### Scenario 2: Job আছে কিন্তু চলছে না ⚠️

**Symptom:**
- Job exists কিন্তু history empty

**Check SQL Server Agent Service:**
```sql
EXEC xp_servicecontrol 'QueryState', 'SQLServerAGENT'
```

**Expected:** RUNNING

**যদি Stopped থাকে:**
1. Windows Services (`services.msc`) খুলুন
2. "SQL Server Agent" খুঁজুন
3. Right-click → Start
4. Properties → Startup type → Automatic করুন

### Scenario 3: Job চলছে কিন্তু fail হচ্ছে ❌

**Check Error:**
```sql
SELECT TOP 5
    FORMAT(msdb.dbo.agent_datetime(run_date, run_time), 'dd MMM yyyy hh:mm tt') AS ExecutionTime,
    step_name,
    message
FROM msdb.dbo.sysjobhistory
WHERE job_id = (SELECT job_id FROM msdb.dbo.sysjobs WHERE name LIKE '%Monthly%')
AND run_status = 0 -- Failed
ORDER BY run_date DESC, run_time DESC
```

**Common Errors & Solutions:**

| Error Message | Solution |
|---------------|----------|
| "Invalid object name 'AAP_Student_Count_Monthly'" | Table নেই - database ভুল select হয়েছে |
| "No active education year found" | Education_Year table এ active year add করুন |
| "Service Charge category not found" | AAP_Invoice_Category এ 'Service Charge' add করুন |
| "Login timeout" / "Connection failed" | Database connection string check করুন |

### Scenario 4: Job সফল কিন্তু data নেই 🤔

**Check:**
```sql
-- Do you have students?
SELECT COUNT(*) FROM Student WHERE Status = 'Active'

-- Do you have student-class mapping?
SELECT COUNT(*) FROM StudentsClass

-- Do you have education year?
SELECT * FROM Education_Year WHERE GETDATE() BETWEEN StartDate AND EndDate
```

---

## 📊 Part 4: Quick Dashboard

### এক নজরে সব কিছু দেখুন:

```sql
PRINT '========================================='
PRINT '   MONTHLY AUTO PROCESS DASHBOARD'
PRINT '========================================='
PRINT ''

-- 1. Job Status
PRINT '1. JOB STATUS:'
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name LIKE '%Monthly%')
BEGIN
    SELECT 
        '✅ Job exists: ' + name + 
        CASE WHEN enabled = 1 THEN ' (Enabled)' ELSE ' ❌ (Disabled)' END AS Status
    FROM msdb.dbo.sysjobs WHERE name LIKE '%Monthly%'
END
ELSE
    PRINT '❌ Job NOT created yet'

PRINT ''

-- 2. SQL Agent Service
PRINT '2. SQL AGENT SERVICE:'
EXEC xp_servicecontrol 'QueryState', 'SQLServerAGENT'

PRINT ''

-- 3. Last Execution
PRINT '3. LAST EXECUTION:'
SELECT TOP 1
    '📅 ' + FORMAT(msdb.dbo.agent_datetime(run_date, run_time), 'dd MMM yyyy hh:mm tt') + ' - ' +
    CASE run_status WHEN 1 THEN '✅ Success' ELSE '❌ Failed' END AS LastRun
FROM msdb.dbo.sysjobhistory
WHERE job_id = (SELECT job_id FROM msdb.dbo.sysjobs WHERE name LIKE '%Monthly%')
AND step_id = 0
ORDER BY run_date DESC, run_time DESC

PRINT ''

-- 4. Recent Student Counts
PRINT '4. RECENT STUDENT COUNTS:'
SELECT TOP 3
    '📊 ' + FORMAT(Month, 'MMM yyyy') + ': ' + 
    CAST(COUNT(*) AS NVARCHAR) + ' institutions, ' +
    CAST(SUM(ISNULL(StudentCount, 0)) AS NVARCHAR) + ' students' AS Summary
FROM AAP_Student_Count_Monthly
GROUP BY Month
ORDER BY Month DESC

PRINT ''

-- 5. Recent Invoices
PRINT '5. RECENT INVOICES:'
SELECT TOP 3
    '💰 ' + FORMAT(MonthName, 'MMM yyyy') + ': ' + 
    CAST(COUNT(*) AS NVARCHAR) + ' invoices, Total: ' +
    CAST(SUM(TotalAmount) AS NVARCHAR) + ' BDT' AS Summary
FROM AAP_Invoice
WHERE InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = 'Service Charge')
GROUP BY MonthName
ORDER BY MonthName DESC

PRINT ''
PRINT '========================================='
```

---

## 🔔 Part 5: Email Notifications Setup (Optional)

যদি চান job fail হলে email পাবেন:

### Step 1: Configure Database Mail

```sql
-- Enable Database Mail
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'Database Mail XPs', 1;
RECONFIGURE;

-- Create mail profile (একবারই করতে হবে)
EXECUTE msdb.dbo.sysmail_add_profile_sp
    @profile_name = 'AutoProcess Mail',
    @description = 'Profile for automatic process notifications';

-- Add mail account
EXECUTE msdb.dbo.sysmail_add_account_sp
    @account_name = 'SMTP Account',
    @email_address = 'your-email@gmail.com',
    @display_name = 'Auto Process',
    @mailserver_name = 'smtp.gmail.com',
    @port = 587,
    @enable_ssl = 1,
    @username = 'your-email@gmail.com',
    @password = 'your-app-password';

-- Associate account with profile
EXECUTE msdb.dbo.sysmail_add_profileaccount_sp
    @profile_name = 'AutoProcess Mail',
    @account_name = 'SMTP Account',
    @sequence_number = 1;
```

### Step 2: Create Operator

```sql
EXEC msdb.dbo.sp_add_operator
    @name = N'Admin',
    @enabled = 1,
    @email_address = N'admin@yourcompany.com';
```

### Step 3: Configure Job Notifications

```sql
EXEC msdb.dbo.sp_update_job
    @job_name = N'Monthly Student Count and Invoice Generation',
    @notify_level_email = 2, -- On failure
    @notify_email_operator_name = N'Admin';
```

---

## 📝 Checklist: প্রতি মাসে Check করুন

- [ ] **2 বা 3 তারিখে** log table check করুন
- [ ] Student count হয়েছে কিনা verify করুন
- [ ] Invoice generate হয়েছে কিনা check করুন
- [ ] Count সংখ্যা সঠিক কিনা compare করুন
- [ ] Job history তে error আছে কিনা দেখুন

**Quick Check Command:**
```sql
-- Run this on 2nd or 3rd of every month
EXEC sp_Monthly_Auto_Process @TargetMonth = NULL
-- If it says "already exists" = Auto process worked!
```

---

## 🆘 Troubleshooting Quick Guide

| Problem | Quick Fix |
|---------|-----------|
| Job না থাকলে | Job creation script run করুন |
| SQL Agent stopped | Services থেকে start করুন |
| Data generate হচ্ছে না | Education Year active আছে কিনা check করুন |
| Invoice না হলে | Student count আগে আছে কিনা check করুন |
| UI error | Application restart করুন |

---

**🎉 সফল হলে আপনি দেখবেন:**
1. ✅ `AAP_Auto_Process_Log` table এ success message
2. ✅ `AAP_Student_Count_Monthly` table এ নতুন data
3. ✅ `AAP_Invoice` table এ নতুন invoices
4. ✅ SQL Agent job history তে success status
