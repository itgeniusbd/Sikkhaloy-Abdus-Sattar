# Quick Reference - Monthly Auto Process

## ?? Quick Start (????? ???? ????)

### 1?? Install (?????? ????)
```sql
-- Run this file in SSMS
Database Scripts/Monthly_Auto_Process.sql
```

### 2?? Test (??????? ????)
```sql
-- Test for current month
EXEC sp_Monthly_Auto_Process @TargetMonth = NULL
```

### 3?? Schedule (???????? ????)
```sql
-- Create auto job (change YourDatabaseName)
USE msdb;
EXEC dbo.sp_add_job @job_name = N'Monthly Student Count and Invoice Generation', @enabled = 1;
EXEC dbo.sp_add_jobstep @job_name = N'Monthly Student Count and Invoice Generation', 
    @step_name = N'Run Auto Process', @command = N'EXEC sp_Monthly_Auto_Process', 
    @database_name = N'YourDatabaseName';
EXEC dbo.sp_add_schedule @schedule_name = N'Monthly on 1st at 2AM', 
    @freq_type = 16, @freq_interval = 1, @active_start_time = 020000;
EXEC dbo.sp_attach_schedule @job_name = N'Monthly Student Count and Invoice Generation', 
    @schedule_name = N'Monthly on 1st at 2AM';
EXEC dbo.sp_add_jobserver @job_name = N'Monthly Student Count and Invoice Generation';
```

---

## ?? Common Commands (?????? ??????)

### Generate for Current Month
```sql
EXEC sp_Monthly_Auto_Process @TargetMonth = NULL
```

### Generate for Specific Month
```sql
EXEC sp_Monthly_Auto_Process @TargetMonth = '2026-03-01'
```

### View Logs
```sql
SELECT TOP 10 * FROM AAP_Auto_Process_Log ORDER BY ProcessDate DESC
```

### Check Job Status
```sql
SELECT name, enabled, date_created 
FROM msdb.dbo.sysjobs 
WHERE name LIKE '%Monthly%'
```

### Manual Run Job
```sql
EXEC msdb.dbo.sp_start_job @job_name = N'Monthly Student Count and Invoice Generation'
```

---

## ? How It Works (?????? ??? ???)

```
????? ????? ? ????? ??? ?????:

???????????????????????????????????
?  SQL Server Agent Job Starts    ?
???????????????????????????????????
            ?
            ?
???????????????????????????????????
?  1. Generate Student Count      ?
?     - Class-wise count          ?
?     - School-wise total         ?
?     - Include committee         ?
???????????????????????????????????
            ?
            ?
???????????????????????????????????
?  2. Generate Invoices           ?
?     - Service Charge category   ?
?     - Active institutions only  ?
?     - Calculate with committee  ?
???????????????????????????????????
            ?
            ?
???????????????????????????????????
?  3. Save Logs                   ?
?     - AAP_Auto_Process_Log      ?
?     - Success/Error messages    ?
???????????????????????????????????
```

---

## ?? Manual Backup (??? ??? fail ???)

### UI ???? (Web Application):
1. Login ? **Create Invoice** page
2. Click **"Generate Student Count"** button
3. Select month ? Click **Generate**
4. Select month from dropdown
5. Select institutions ? Click **Submit**

### SQL ???? (Database):
```sql
-- Step 1: Generate student count
DECLARE @Count INT, @Msg NVARCHAR(500)
EXEC sp_Generate_Monthly_Student_Count 
    @TargetMonth = '2026-03-01',
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT
PRINT @Msg

-- Step 2: Generate invoices
EXEC sp_Generate_Monthly_Invoices 
    @TargetMonth = '2026-03-01',
    @GeneratedCount = @Count OUTPUT,
    @ErrorMessage = @Msg OUTPUT
PRINT @Msg
```

---

## ?? Monitoring (??????????)

### Last 7 Days Activity
```sql
SELECT 
    FORMAT(ProcessDate, 'dd MMM hh:mm tt') AS Time,
    FORMAT(ProcessMonth, 'MMM yyyy') AS Month,
    ProcessType,
    CASE 
        WHEN LogMessage LIKE '%Success%' THEN '? Success'
        WHEN LogMessage LIKE '%error%' THEN '? Error'
        ELSE '?? Warning'
    END AS Status,
    LogMessage
FROM AAP_Auto_Process_Log 
WHERE ProcessDate >= DATEADD(DAY, -7, GETDATE())
ORDER BY ProcessDate DESC
```

### Job Execution History
```sql
SELECT TOP 10
    h.run_date,
    h.run_time,
    CASE h.run_status 
        WHEN 0 THEN '? Failed'
        WHEN 1 THEN '? Succeeded'
        WHEN 2 THEN '?? Retry'
        WHEN 3 THEN '?? Canceled'
    END AS Status,
    h.message
FROM msdb.dbo.sysjobhistory h
INNER JOIN msdb.dbo.sysjobs j ON h.job_id = j.job_id
WHERE j.name LIKE '%Monthly%'
ORDER BY h.run_date DESC, h.run_time DESC
```

---

## ?? Troubleshooting (?????? ??????)

| Problem | Solution |
|---------|----------|
| Job not running | Check SQL Server Agent service is started |
| No student count | Verify Education Year is active for the month |
| No invoices | Check if student count exists for that month |
| Duplicate invoices | Process already ran - check logs |
| Wrong amounts | Verify Per_Student_Rate in SchoolInfo table |

### Quick Checks
```sql
-- Check if SQL Agent is running
EXEC xp_servicecontrol 'QueryState', 'SQLServerAGENT'

-- Check Education Year
SELECT * FROM Education_Year 
WHERE GETDATE() BETWEEN StartDate AND EndDate

-- Check Student Count for current month
SELECT COUNT(*) AS TotalSchools
FROM AAP_Student_Count_Monthly
WHERE MONTH(Month) = MONTH(GETDATE()) 
AND YEAR(Month) = YEAR(GETDATE())

-- Check if Service Charge category exists
SELECT * FROM AAP_Invoice_Category 
WHERE InvoiceCategory = N'Service Charge'
```

---

## ?? Emergency Commands

### Stop Running Job
```sql
EXEC msdb.dbo.sp_stop_job @job_name = N'Monthly Student Count and Invoice Generation'
```

### Disable Auto Job
```sql
EXEC msdb.dbo.sp_update_job 
    @job_name = N'Monthly Student Count and Invoice Generation',
    @enabled = 0
```

### Delete a Month's Data (Use with caution!)
```sql
-- Delete invoices for a specific month
DELETE FROM AAP_Invoice 
WHERE MONTH(MonthName) = 3 AND YEAR(MonthName) = 2026
AND InvoiceCategoryID = (SELECT InvoiceCategoryID FROM AAP_Invoice_Category WHERE InvoiceCategory = N'Service Charge')

-- Delete student count for a specific month
DELETE FROM AAP_Student_Count_Monthly 
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026

DELETE FROM AAP_StudentClass_Count_Monthly 
WHERE MONTH(Month) = 3 AND YEAR(Month) = 2026
```

---

## ?? Checklist (????????)

????? ????? ? ?? ? ?????? verify ????:

- [ ] Job successfully ran (Log check ????)
- [ ] Student count generated (Check AAP_Student_Count_Monthly)
- [ ] Invoices created (Check AAP_Invoice)
- [ ] Counts are correct (Compare with UI)
- [ ] No errors in logs (Check AAP_Auto_Process_Log)

---

## ?? Quick Commands Summary

```sql
-- Install
-- Run Monthly_Auto_Process.sql file

-- Test
EXEC sp_Monthly_Auto_Process @TargetMonth = NULL

-- View Logs
SELECT * FROM AAP_Auto_Process_Log ORDER BY ProcessDate DESC

-- Manual Run
EXEC msdb.dbo.sp_start_job @job_name = N'Monthly Student Count and Invoice Generation'

-- Check Status
SELECT name, enabled FROM msdb.dbo.sysjobs WHERE name LIKE '%Monthly%'
```

---

**?? For detailed documentation, see:** `Database Scripts/Setup_Guide.md`
