# Updated: Naming Convention Support in Analysis Script

**Date:** 2026-05-15  
**Issue:** Script was missing POCOs/Repos due to naming changes (Customer?Contact, City?Area, etc.)  
**Solution:** Added systematic mapping based on migration CSV

---

## What Was Fixed

### Problem

The `Analyze-Legacy-Classes.ps1` script was reporting POCOs and Repositories as "missing" when they actually existed under renamed names:

**Example:**
- Legacy: `CustomersTbl.cs`
- Script looked for: `Customers.cs` or `CustomersRepository.cs`
- Actually exists as: `Contact.cs` and `ContactsRepository.cs`
- **Result:** Falsely reported as missing ?

### Solution

Updated script now includes a `Get-MigratedPocoName` function that applies systematic renames:

```powershell
function Get-MigratedPocoName {
    param([string]$LegacyName)

    # Applies mappings like:
    # Customers ? Contact
    # City ? Area
    # ItemType ? Item
    # Machine ? Equip
    # Reoccur ? Recurr
}
```

---

## Naming Convention Mappings Applied

The script now automatically handles these renames from `TableMigrationReport-14-May-26.csv`:

### Major Renames

| Legacy Pattern | Modern Pattern | Examples |
|---|---|---|
| `Customer*` | `Contact*` | CustomersTbl ? Contact |
| `Client*` | `Contact*` | ClientUsageTbl ? ContactsItemsPredicted |
| `City*` | `Area*` | CityTbl ? Area |
| `ItemType*` | `Item*` | ItemTypeTbl ? Item |
| `Machine*` | `Equip*` | MachineConditionsTbl ? EquipCondition |
| `Reoccur*` | `Recurr*` | ReoccuringOrderTbl ? RecurringOrder |

### Specific Table Mappings

| Legacy Class | POCO Name | Repository Name |
|---|---|---|
| `CustomersTbl` | `Contact` | `ContactsRepository` |
| `CustomersAccInfoTbl` | `ContactsAccInfo` | `ContactsAccInfoRepository` |
| `CustomerTypeTbl` | `ContactType` | `ContactTypesRepository` |
| `ClientUsageTbl` | `ContactsItemsPredicted` | (via ContactsRepository) |
| `CityTbl` | `Area` | `AreasRepository` |
| `CityPrepDaysTbl` | `AreaPrepDays` | `AreaPrepDaysRepository` |
| `ItemTypeTbl` | `Item` | `ItemsRepository` |
| `PackagingTbl` | `ItemPackaging` | `ItemPackagingsRepository` |
| `PrepTypesTbl` | `ItemPrepType` | `ItemPrepTypesRepository` |
| `MachineConditionsTbl` | `EquipCondition` | `EquipConditionsRepository` |
| `ReoccuringOrderTbl` | `RecurringOrder` | `RecurringOrdersRepository` |

---

## Files Updated

1. ? **`DevTools/Scripts/Analyze-Legacy-Classes.ps1`**
   - Added `Get-MigratedPocoName` function
   - Applies systematic renames
   - Tries plural/singular variations
   - Correctly identifies existing POCOs and Repos

2. ? **`DevTools/Documentation/Naming_Convention_Reference.md`** (NEW)
   - Complete naming convention reference
   - All table/column mappings
   - Examples and patterns
   - Verification checklist

3. ? **`DevTools/QUICK_START_REPOSITORY_MIGRATION.md`**
   - Added naming convention warning
   - References new documentation

---

## How to Use

### Run Updated Analysis

```powershell
cd C:\SRC\ASP.net\TrackerSQL
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```

**Now correctly identifies:**
- ? `CustomersTbl` ? finds `Contact.cs` and `ContactsRepository.cs`
- ? `CityTbl` ? finds `Area.cs` and `AreasRepository.cs`
- ? `ItemTypeTbl` ? finds `Item.cs` and `ItemsRepository.cs`
- ? `ReoccuringOrderTbl` ? finds `RecurringOrder.cs` and `RecurringOrdersRepository.cs`

### Review Results

```powershell
code DevTools\Documentation\Legacy_To_Poco_Mapping.csv
```

**Columns:**
- `LegacyClass` - Original name (e.g., CustomersTbl)
- `PocoName` - Mapped POCO name (e.g., Contact)
- `PocoExists` - YES/NO (now accurate!)
- `RepoName` - Mapped Repository name (e.g., ContactsRepository)
- `RepoExists` - YES/NO (now accurate!)
- `CustomMethods` - Count of custom business methods
- `CustomMethodNames` - List of custom method names
- `Priority` - HIGH/MEDIUM/LOW

---

## What This Means for Migration

### Before (Incorrect)

```csv
LegacyClass,PocoName,PocoExists,RepoName,RepoExists,Priority
CustomersTbl,Customers,NO,CustomersRepository,NO,HIGH
CityTbl,City,NO,CityRepository,NO,HIGH
```

**Result:** Script thinks you need to create 50+ POCOs and Repos ?

### After (Correct)

```csv
LegacyClass,PocoName,PocoExists,RepoName,RepoExists,Priority
CustomersTbl,Contact,YES,ContactsRepository,YES,LOW
CityTbl,Area,YES,AreasRepository,YES,LOW
```

**Result:** Script correctly identifies existing POCOs and Repos ?

---

## Next Steps

### 1. Re-run Analysis

```powershell
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```

### 2. Review NEW Results

The CSV will now show:
- **HIGH Priority:** Truly missing repos (create these)
- **MEDIUM Priority:** Repos exist but have custom methods (copy logic)
- **LOW Priority:** Repos exist with all standard CRUD (done!)

### 3. Generate Only What's Actually Missing

```powershell
# Get truly missing repos
$missing = Import-Csv "DevTools\Documentation\Legacy_To_Poco_Mapping.csv" | 
    Where-Object { $_.RepoExists -eq "NO" }

# Generate each one
foreach ($item in $missing) {
    Write-Host "Creating $($item.RepoName)..." -ForegroundColor Cyan
    .\DevTools\Scripts\Generate-Repository.ps1 -PocoName $item.PocoName
}
```

---

## Reference Documentation

**For complete naming conventions:**  
?? `DevTools/Documentation/Naming_Convention_Reference.md`

**Key sections:**
- Table Name Mappings
- Column Name Mappings
- POCO Class Naming
- Examples of Correct Mappings
- Common Mistakes to Avoid
- Quick Reference Table

---

## Testing the Fix

### Example: CustomersTbl

```powershell
# Before fix
# Script looked for: Customers.cs, CustomersRepository.cs
# Found: NO, NO
# Priority: HIGH (incorrect!)

# After fix
# Script applies mapping: Customers ? Contact
# Looks for: Contact.cs, ContactsRepository.cs
# Found: YES, YES
# Priority: LOW (correct!)
```

### Example: CityPrepDaysTbl

```powershell
# Before fix
# Script looked for: CityPrepDays.cs, CityPrepDaysRepository.cs
# Found: NO, NO
# Priority: HIGH (incorrect!)

# After fix
# Script applies mapping: City ? Area
# Looks for: AreaPrepDays.cs, AreaPrepDaysRepository.cs
# Found: YES, YES
# Priority: LOW (correct!)
```

---

## Summary

? **Fixed:** Analysis script now accounts for systematic table/column renames  
? **Added:** Complete naming convention reference documentation  
? **Result:** Accurate identification of missing vs. existing POCOs and Repositories  
? **Benefit:** Won't waste time creating POCOs/Repos that already exist  

**Run the updated script now:**

```powershell
.\DevTools\Scripts\Analyze-Legacy-Classes.ps1
```

**Expected:** Much lower "missing" count, more accurate priorities! ??
