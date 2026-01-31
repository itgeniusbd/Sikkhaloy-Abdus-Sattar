# ?? Committee Member Active/Inactive Feature

## ?? Overview
Committee Member-??? Active/Inactive ???? ???????? ??????? ??? ??? ??????? ??? ???? level-? control ??? ????:

1. **Individual Member Level** - ??????? member ????????? active/inactive
2. **Category Level** - ???? category ?????? billing ???? active/inactive

---

## ? Features Added

### 1. **Database Changes**

#### CommitteeMember Table:
```sql
- Status column added (NVARCHAR(20), Default: 'Active')
- Values: 'Active' or 'Inactive'
- Indexed for performance
```

#### CommitteeMember_Billing Table:
```sql
- IsActive column added (BIT, Default: 1)
- Controls whether category is active for billing
- Indexed for performance
```

### 2. **Member Management (Committee/MemberAdd.aspx)**

**New Features:**
- ? Status column in GridView
- ? Dropdown to change Active/Inactive
- ? Badge display (Green for Active, Gray for Inactive)
- ? Status filter option
- ? Default status = Active for new members

**Visual Display:**
```
???????????????????????????????????????
? Name: John Doe                      ?
? Status: [Active ?]                  ?
? Badge:  ? Active  (green badge)    ?
???????????????????????????????????????
```

### 3. **Authority Billing Control (Authority/Free_SMS.aspx)**

**Enhanced Display:**
```
??????????????????????????????????????????????????????
? ?? Committee Billing                               ?
??????????????????????????????????????????????????????
? ? President       [5 active] ? ? Active           ?
? ? Secretary       [2 active] ? ? Active           ?
? ? Member          [40 active] ? ? Inactive        ?
??????????????????????????????????????????????????????
? ?? Total Active: 5 members                         ?
? (Only active & selected categories counted)        ?
??????????????????????????????????????????????????????
```

**Two Checkboxes:**
1. **Left Checkbox (?)** - Include in billing calculation
2. **Right Checkbox (?)** - Category active status

---

## ?? Billing Logic

### Previous (Student Only):
```
Total Bill = Active Students × Per Student Rate
```

### Current (Student + Committee):
```
Active Students = Students with Status = 'Active'
Active Committee = Members with:
                   - Member.Status = 'Active' AND
                   - Category.IsIncluded = 1 (checked) AND
                   - Category.IsActive = 1 (active)

Total Bill = (Active Students + Active Committee) × Per Student Rate
```

### Example Calculation:
```
Per Student Rate: ?2

Students:
- Total Students: 500
- Active Students: 480
- Inactive Students: 20

Committee:
Category: President
- Total Members: 5
- Active Members: 5
- IsIncluded: Yes (checked)
- IsActive: Yes
- Count in Billing: 5

Category: Secretary  
- Total Members: 3
- Active Members: 2
- IsIncluded: Yes (checked)
- IsActive: No (inactive)
- Count in Billing: 0

Category: Member
- Total Members: 50
- Active Members: 40
- IsIncluded: No (not checked)
- IsActive: Yes
- Count in Billing: 0

Total Billable Count = 480 + 5 + 0 + 0 = 485
Total Bill = 485 × ?2 = ?970
```

---

## ?? SQL Queries

### Get Active Member Count by Category:
```sql
SELECT 
    CMT.CommitteeMemberType,
    COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END) as ActiveCount,
    COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Inactive' THEN 1 END) as InactiveCount,
    COUNT(CM.CommitteeMemberId) as TotalCount
FROM CommitteeMemberType CMT
LEFT JOIN CommitteeMember CM ON CMT.CommitteeMemberTypeId = CM.CommitteeMemberTypeId
WHERE CMT.SchoolID = @SchoolID
GROUP BY CMT.CommitteeMemberType
```

### Get Billable Committee Member Count:
```sql
SELECT SUM(
    (SELECT COUNT(*) 
     FROM CommitteeMember 
     WHERE CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
       AND SchoolID = @SchoolID
       AND ISNULL(Status, 'Active') = 'Active')
) as BillableCount
FROM CommitteeMember_Billing CMB 
WHERE CMB.SchoolID = @SchoolID 
  AND CMB.IsIncluded = 1 
  AND CMB.IsActive = 1
```

### Complete Billing Count Query:
```sql
-- Student count (active only)
DECLARE @StudentCount INT = (
    SELECT COUNT(*) 
    FROM Student 
    WHERE SchoolID = @SchoolID 
      AND Status = 'Active'
)

-- Committee member count (active members, included & active categories only)
DECLARE @CommitteeCount INT = ISNULL((
    SELECT SUM(
        (SELECT COUNT(*) 
         FROM CommitteeMember 
         WHERE CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
           AND SchoolID = @SchoolID
           AND ISNULL(Status, 'Active') = 'Active')
    )
    FROM CommitteeMember_Billing CMB 
    WHERE CMB.SchoolID = @SchoolID 
      AND CMB.IsIncluded = 1 
      AND CMB.IsActive = 1
), 0)

-- Total billable count
DECLARE @TotalBillable INT = @StudentCount + @CommitteeCount

-- Calculate bill amount
DECLARE @PerStudentRate DECIMAL(10,2) = (
    SELECT Per_Student_Rate 
    FROM SchoolInfo 
    WHERE SchoolID = @SchoolID
)

DECLARE @TotalBill DECIMAL(10,2) = @TotalBillable * @PerStudentRate

SELECT 
    @StudentCount as StudentCount,
    @CommitteeCount as CommitteeCount,
    @TotalBillable as TotalBillableCount,
    @PerStudentRate as PerStudentRate,
    @TotalBill as TotalBillAmount
```

---

## ?? Implementation in Code

### C# Helper Method for Billing:
```csharp
public class BillingCalculator
{
    private string connectionString;

    public BillingCalculator(string connString)
    {
        this.connectionString = connString;
    }

    public BillingInfo CalculateMonthlyBilling(int schoolID)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            
            // Get student count
            int studentCount = GetActiveStudentCount(conn, schoolID);
            
            // Get committee member count
            int committeeCount = GetBillableCommitteeMemberCount(conn, schoolID);
            
            // Get per student rate
            decimal perStudentRate = GetPerStudentRate(conn, schoolID);
            
            // Calculate total
            int totalBillable = studentCount + committeeCount;
            decimal totalBill = totalBillable * perStudentRate;
            
            return new BillingInfo
            {
                SchoolID = schoolID,
                StudentCount = studentCount,
                CommitteeCount = committeeCount,
                TotalBillableCount = totalBillable,
                PerStudentRate = perStudentRate,
                TotalBillAmount = totalBill,
                BillingDate = DateTime.Now
            };
        }
    }

    private int GetActiveStudentCount(SqlConnection conn, int schoolID)
    {
        string query = "SELECT COUNT(*) FROM Student WHERE SchoolID = @SchoolID AND Status = 'Active'";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@SchoolID", schoolID);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private int GetBillableCommitteeMemberCount(SqlConnection conn, int schoolID)
    {
        string query = @"
            SELECT ISNULL(SUM(
                (SELECT COUNT(*) 
                 FROM CommitteeMember 
                 WHERE CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
                   AND SchoolID = @SchoolID
                   AND ISNULL(Status, 'Active') = 'Active')
            ), 0)
            FROM CommitteeMember_Billing CMB 
            WHERE CMB.SchoolID = @SchoolID 
              AND CMB.IsIncluded = 1 
              AND CMB.IsActive = 1";
        
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@SchoolID", schoolID);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private decimal GetPerStudentRate(SqlConnection conn, int schoolID)
    {
        string query = "SELECT Per_Student_Rate FROM SchoolInfo WHERE SchoolID = @SchoolID";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@SchoolID", schoolID);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
    }
}

public class BillingInfo
{
    public int SchoolID { get; set; }
    public int StudentCount { get; set; }
    public int CommitteeCount { get; set; }
    public int TotalBillableCount { get; set; }
    public decimal PerStudentRate { get; set; }
    public decimal TotalBillAmount { get; set; }
    public DateTime BillingDate { get; set; }
}
```

---

## ?? Usage Instructions

### For Committee Administrators:

#### Managing Individual Members:
1. **Navigate to:** Committee ? Add Member
2. **Find the member** you want to update
3. **Click Edit** button
4. **Change Status** dropdown:
   - Select "Active" to include in billing
   - Select "Inactive" to exclude from billing
5. **Click Update**

#### Viewing Member Status:
- Active members show **green badge**
- Inactive members show **gray badge**
- Status column shows current state

### For Authority Users:

#### Managing Category Billing:
1. **Navigate to:** Authority ? Free SMS
2. **Find the school** in the list
3. **Committee Member Bill** column shows:
   - List of all categories
   - Active member count for each
   - Two checkboxes per category:
     - **Left:** Include in billing
     - **Right:** Category active status

4. **Make Changes:**
   - Check/uncheck categories to include/exclude
   - Check/uncheck active status
   - Total automatically updates

5. **Save Changes:**
   - Click "?? Update All Changes" button
   - Confirmation message appears

---

## ?? Reports & Analytics

### Member Status Report:
```sql
SELECT 
    SI.SchoolName,
    CMT.CommitteeMemberType as Category,
    COUNT(CM.CommitteeMemberId) as TotalMembers,
    COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END) as ActiveMembers,
    COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Inactive' THEN 1 END) as InactiveMembers,
    CAST(COUNT(CASE WHEN ISNULL(CM.Status, 'Active') = 'Active' THEN 1 END) * 100.0 / 
         NULLIF(COUNT(CM.CommitteeMemberId), 0) AS DECIMAL(5,2)) as ActivePercentage
FROM SchoolInfo SI
INNER JOIN CommitteeMemberType CMT ON SI.SchoolID = CMT.SchoolID
LEFT JOIN CommitteeMember CM ON CMT.CommitteeMemberTypeId = CM.CommitteeMemberTypeId
GROUP BY SI.SchoolName, CMT.CommitteeMemberType
ORDER BY SI.SchoolName, CMT.CommitteeMemberType
```

### Billing Impact Report:
```sql
SELECT 
    SI.SchoolName,
    SI.Per_Student_Rate,
    
    -- Student counts
    (SELECT COUNT(*) FROM Student WHERE SchoolID = SI.SchoolID) as TotalStudents,
    (SELECT COUNT(*) FROM Student WHERE SchoolID = SI.SchoolID AND Status = 'Active') as ActiveStudents,
    
    -- Committee counts
    (SELECT SUM(
        (SELECT COUNT(*) FROM CommitteeMember 
         WHERE CommitteeMemberTypeId = CMT.CommitteeMemberTypeId 
           AND SchoolID = SI.SchoolID)
    ) FROM CommitteeMemberType CMT WHERE CMT.SchoolID = SI.SchoolID) as TotalCommittee,
    
    (SELECT SUM(
        (SELECT COUNT(*) FROM CommitteeMember 
         WHERE CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
           AND SchoolID = SI.SchoolID
           AND ISNULL(Status, 'Active') = 'Active')
    ) FROM CommitteeMember_Billing CMB 
     WHERE CMB.SchoolID = SI.SchoolID 
       AND CMB.IsIncluded = 1 
       AND CMB.IsActive = 1) as BillableCommittee,
    
    -- Billing calculation
    (SELECT COUNT(*) FROM Student WHERE SchoolID = SI.SchoolID AND Status = 'Active') +
    ISNULL((SELECT SUM(
        (SELECT COUNT(*) FROM CommitteeMember 
         WHERE CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
           AND SchoolID = SI.SchoolID
           AND ISNULL(Status, 'Active') = 'Active')
    ) FROM CommitteeMember_Billing CMB 
     WHERE CMB.SchoolID = SI.SchoolID 
       AND CMB.IsIncluded = 1 
       AND CMB.IsActive = 1), 0) as TotalBillable,
    
    -- Total bill amount
    (
        (SELECT COUNT(*) FROM Student WHERE SchoolID = SI.SchoolID AND Status = 'Active') +
        ISNULL((SELECT SUM(
            (SELECT COUNT(*) FROM CommitteeMember 
             WHERE CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
               AND SchoolID = SI.SchoolID
               AND ISNULL(Status, 'Active') = 'Active')
        ) FROM CommitteeMember_Billing CMB 
         WHERE CMB.SchoolID = SI.SchoolID 
           AND CMB.IsIncluded = 1 
           AND CMB.IsActive = 1), 0)
    ) * SI.Per_Student_Rate as TotalBillAmount

FROM SchoolInfo SI
ORDER BY SI.SchoolName
```

---

## ? Testing Checklist

### Database Testing:
- [ ] Status column exists in CommitteeMember table
- [ ] IsActive column exists in CommitteeMember_Billing table
- [ ] Default value 'Active' works for new members
- [ ] Indexes created successfully

### Member Management Testing:
- [ ] Status dropdown appears in edit mode
- [ ] Can change member status to Active/Inactive
- [ ] Badge color changes based on status
- [ ] New members default to Active
- [ ] Status persists after save

### Billing Control Testing:
- [ ] Category checkboxes appear correctly
- [ ] Active member count displays accurately
- [ ] Category active checkbox works
- [ ] Total count calculates correctly
- [ ] Only counts active members in active categories
- [ ] Save button stores both settings
- [ ] Data persists after page refresh

### Billing Calculation Testing:
- [ ] Active students counted correctly
- [ ] Active committee members counted correctly
- [ ] Only included categories counted
- [ ] Only active categories counted
- [ ] Only active members counted
- [ ] Total bill calculation accurate

---

## ?? Migration Guide

### For Existing Installations:

1. **Backup Database** (CRITICAL!)
   ```sql
   BACKUP DATABASE [Edu] TO DISK = 'C:\Backup\Edu_Before_Status_Update.bak'
   ```

2. **Run SQL Script:**
   ```sql
   -- Execute: Database_Scripts\Create_CommitteeMember_Billing_Table.sql
   ```

3. **Verify Changes:**
   ```sql
   -- Check CommitteeMember table
   SELECT * FROM sys.columns 
   WHERE object_id = OBJECT_ID('CommitteeMember') 
     AND name = 'Status'
   
   -- Check CommitteeMember_Billing table  
   SELECT * FROM sys.columns 
   WHERE object_id = OBJECT_ID('CommitteeMember_Billing') 
     AND name = 'IsActive'
   ```

4. **Set Default Values:**
   ```sql
   -- Set all existing members to Active (if not already set)
   UPDATE CommitteeMember 
   SET Status = 'Active' 
   WHERE Status IS NULL OR Status = ''
   
   -- Set all existing categories to Active (if not already set)
   UPDATE CommitteeMember_Billing 
   SET IsActive = 1 
   WHERE IsActive IS NULL
   ```

5. **Deploy Application Files**
   - MemberAdd.aspx
   - MemberAdd.aspx.cs
   - Free_SMS.aspx
   - Free_SMS.aspx.cs

6. **Test Functionality**
   - Test member status change
   - Test category active/inactive
   - Test billing calculation
   - Verify reports

---

## ?? Support & Troubleshooting

### Common Issues:

**Issue 1: Status column not found**
```
Solution: Run the SQL script to add Status column to CommitteeMember table
```

**Issue 2: Category active checkbox not saving**
```
Solution: Verify IsActive column exists in CommitteeMember_Billing table
```

**Issue 3: Inactive members still counted in billing**
```
Solution: Check billing calculation query uses Status = 'Active' filter
```

**Issue 4: Category shows wrong member count**
```
Solution: Refresh the page, verify query includes Status filter
```

---

## ?? Deployment Complete!

### Files Modified:
1. ? `Database_Scripts\Create_CommitteeMember_Billing_Table.sql`
2. ? `SIKKHALOY V2\Authority\Free_SMS.aspx`
3. ? `SIKKHALOY V2\Authority\Free_SMS.aspx.cs`
4. ? `SIKKHALOY V2\Committee\MemberAdd.aspx`
5. ? `SIKKHALOY V2\Committee\MemberAdd.aspx.cs`

### Documentation Created:
1. ? `COMMITTEE_BILLING_FEATURE.md` (Original feature docs)
2. ? `COMMITTEE_ACTIVE_INACTIVE_FEATURE.md` (This document)

### Build Status:
? **Build Successful!**

---

**Feature Complete!** ??

Committee Member-??? ??? ???? level-? control ??? ????:
1. Individual member level (Active/Inactive)
2. Category level (Include in billing + Active/Inactive)

??? billing calculation ???????? accurate ???! ??
