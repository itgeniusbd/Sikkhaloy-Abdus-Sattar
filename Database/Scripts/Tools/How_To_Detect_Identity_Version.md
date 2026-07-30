# ASP.NET Identity সংস্করণ কীভাবে বুঝবেন

## ১) PasswordHash দেখে (সবচেয়ে দ্রুত)

SSMS-এ যেকোনো `PasswordHash` কপি করে Base64 decode করুন।

| প্রথম byte (version) | Hash size | মানে |
|----------------------|-----------|------|
| **0x00** | সাধারণত **49 bytes** | **ASP.NET Identity 2.x** (PBKDF2-SHA1) |
| **0x01** | সাধারণত **61+ bytes** | **ASP.NET Core Identity 3+** (PBKDF2-SHA256) |

আপনার হ্যাশ:
`ACGMLlK84ntgQsygFbVQmybUcV9i/edzBS14wxi5r2r0Cn95IFH3sSbK1QNV2Ji+xg==`
→ **49 bytes, version 0x00** → **Identity 2.x**

### PowerShell one-liner

```powershell
$h='YOUR_BASE64_HASH_HERE'
$b=[Convert]::FromBase64String($h)
"Length=$($b.Length); Version=0x$('{0:X2}' -f $b[0])"
```

### Python one-liner

```python
import base64
b=base64.b64decode("YOUR_BASE64_HASH_HERE")
print(len(b), hex(b[0]))
```

---

## ২) ডাটাবেইজ টেবিল স্ট্রাকচার দেখে

`AspNetUsers` টেবিলে কলাম চেক করুন:

| কলাম | Identity 2 | Core Identity 3+ |
|------|------------|------------------|
| `LockoutEndDateUtc` | ✅ আছে | ❌ নেই |
| `LockoutEnd` | ❌ নেই | ✅ আছে (DateTimeOffset) |
| `NormalizedUserName` | ❌ সাধারণত নেই | ✅ আছে |
| `ConcurrencyStamp` | ❌ সাধারণত নেই | ✅ আছে |

```sql
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;
```

---

## ৩) প্রজেক্ট সোর্স/প্যাকেজ দেখে

Visual Studio → **References / NuGet Packages**:

| Package | সংস্করণ |
|---------|---------|
| `Microsoft.AspNet.Identity.Core` | **Identity 2** (Web Forms / MVC 5 / Web API 2) |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | **Core Identity 3+** |

**Namespace:**
- Identity 2: `Microsoft.AspNet.Identity`
- Core: `Microsoft.AspNetCore.Identity`

---

## ৪) নতুন PasswordHash জেনারেট (Identity 2)

```powershell
# One-time: small console app in temp folder, or use script in this repo
dotnet run --project Database/Scripts/Tools/IdentityHashGen
```

Or run `GenerateIdentity2PasswordHash.csx` with dotnet-script.

Default test password used in SQL script: **Test@123**
