# RoastDate → PrepDate Migration List

**Date Created:** 2026-05-13  
**Priority:** HIGH - This naming change affects core business logic across multiple pages  
**Context:** System is moving to more generic "prep" terminology (not coffee-specific)

---

## Summary

All references to `RoastDate` must be updated to `PrepDate` throughout the codebase. This is a **column rename** in the SQL Server migration:

- **Legacy (Access):** `RoastDate` 
- **SQL Server:** `PrepDate`
- **UI Labels:** Should become "Prep Date" instead of "Roast Date"
- **Reason:** Generic terminology for broader use beyond coffee

---

## 🔴 CRITICAL - High Priority Files

### 1. **Pages/OrderDetail.aspx** ⚠️ NEEDS MIGRATION
**Status:** Not migrated yet  
**Severity:** CRITICAL - Core order entry page

**Changes needed:**
- Line ~115: Change `HeaderText="Roast Date"` → `HeaderText="Prep Date"`
- Line ~116: Change `SortExpression="RoastDate"` → `SortExpression="PrepDate"`
- Line ~117: Change `Bind("RoastDate"` → `Bind("PrepDate"`
- Line ~120: Change `Bind("RoastDate"` → `Bind("PrepDate"`
- Line ~124: Change `Bind("RoastDate"` → `Bind("PrepDate"`

**Repository:** Uses `OrdersRepository` (already correct - returns `PrepDate`)

**Code-behind file:** `Pages/OrderDetail.aspx.cs` - Check if it references `RoastDate` in C# code

---

### 2. **Pages/NewOrder.aspx** ⚠️ NEEDS MIGRATION
**Status:** Not migrated yet  
**Severity:** CRITICAL - New order entry page

**Changes needed:**
- Line ~70: Change `HeaderText="Roast Date"` → `HeaderText="Prep Date"`
- Line ~71: Change `SortExpression="RoastDate"` → `SortExpression="PrepDate"`
- Line ~72: Change `Bind("RoastDate"` → `Bind("PrepDate"`
- Line ~75: Change `Bind("RoastDate"` → `Bind("PrepDate"`
- Line ~81: Change `Bind("RoastDate"` → `Bind("PrepDate"`

**Status:** Still uses legacy bindings - needs conversion to Repository pattern first

**Code-behind file:** `Pages/NewOrder.aspx.cs` - Check for C# references to `RoastDate`

---

### 3. **Pages/DeliverySheet.aspx.cs** ⚠️ NEEDS REVIEW
**Status:** Not migrated yet  
**Severity:** HIGH - Delivery operations

**Changes needed:**
- Line ~858: Change log message: `"Changed roast date to"` → `"Changed prep date to"`
- Check for variable names: `ddlActiveRoastDates` → consider renaming to `ddlActivePrepDates`
- Search full file for other `RoastDate` references

**Note:** This page likely has dropdown controls and business logic around prep date selection

---

## 🟡 MEDIUM Priority - Business Logic & Utilities

### 4. **Classes/DateCalculator.cs** ⚠️ NEEDS REFACTORING
**Status:** Partially needs updates  
**Severity:** MEDIUM - Supporting utility

**Changes needed:**
- Line ~147: Method name: `CalculateRoastDateFromDelivery` → `CalculatePrepDateFromDelivery` (semantic change)
- Line ~149: Variable: `roastDate` → `prepDate` (throughout method)
- Line ~154-155: Log message: "RoastDateCalculated" and "ErrorCalculatingRoastDate" → "PrepDateCalculated" / "ErrorCalculatingPrepDate"

**Impact:** Any code calling this method must be updated to use new name

**Note:** Consider if this method should be moved to a more generic location (not just for roasting)

---

### 5. **Classes/TrackerTools.cs** ⚠️ NEEDS REFACTORING
**Status:** Partially needs updates  
**Severity:** MEDIUM - Supporting utility

**Changes needed (terminology updates):**
- Line ~88: Method: `GetDaysToRoastDate()` → `GetDaysToPrepDate()`
- Line ~95: Method: `GetDaysToRoastDate()` overload → `GetDaysToPrepDate()` overload
- Line ~107: Method: `NumDaysTillNextRoast()` → `NumDaysTillNextPrep()`
- Line ~111: Method: `NumDaysTillNextRoast()` overload → `NumDaysTillNextPrep()` overload
- Line ~115: Method: `GetClosestNextRoastDate()` → `GetClosestNextPreperationDate()`
- Line ~119: Method: `GetClosestNextRoastDate()` overload → `GetClosestNextPreperationDate()` overload
- Line ~123: Method: `RoastDateIsBtw()` → `PrepDateIsBtw()`
- Line ~126: Method: `RoastDateIsBtw()` overload → `PrepDateIsBtw()` overload
- Line ~132: Method: `IsNextRoastDateByAreaTodays()` → `IsNextPreperationDateByAreaTodays()`

**Note:** These are terminology changes, not just column renames - method names should be more generic

**Impact:** ALL pages and code calling these methods must be updated

---

## 🔵 LOWER Priority - Legacy Controls (Pending Modernization)

### 6. **Controls/NextRoastDateByCityTbl.cs** 
**Status:** Legacy control (should be deprecated)  
**Severity:** LOW - Legacy control pending replacement

**Issue:** Class/control name references legacy "RoastDate" and "CityTbl"
**Should be:** `NextPreperationDateByAreasTbl` or migrated to Repository pattern

**Action:** Don't update - plan for removal as part of larger control modernization

---

## 📄 Reference Files (No Changes Needed)

These files contain data migration scripts or app_data files - they're reference only:

1. `Data/Metadata/PlanEdits/Sql/DataMigration_LATEST.sql` - Migration scripts (reference)
2. `App_Data/SetClientType_06082025_1304.txt` - Archive/backup (no action)

---

## ✅ Already Completed

- ✅ `Pages/PreperationSummary.aspx` - PrepDate now used correctly
- ✅ `Pages/PreperationSummary.aspx.cs` - Repository updated with PrepDate
- ✅ `Classes/Sql/PreperationSummaryRepository.cs` - Uses PrepDate from OrdersTbl

---

## 📋 MIGRATION CHECKLIST

When updating each file:

- [ ] Update all `RoastDate` column references to `PrepDate`
- [ ] Update all `"Roast Date"` UI labels to `"Prep Date"`
- [ ] Update method/variable names to use "Prep" instead of "Roast"
- [ ] Update log messages and error messages
- [ ] Update SQL query bindings: `Bind("RoastDate"` → `Bind("PrepDate"`
- [ ] Check code-behind C# files for hardcoded `RoastDate` strings
- [ ] Update any dropdown control IDs that reference roast date
- [ ] Verify OrdersRepository already returns `PrepDate` (it does ✅)
- [ ] Build and test the page
- [ ] Verify no "Invalid column name 'RoastDate'" errors

---

## 🔄 Related Changes to Watch For

### UI Control Names That May Need Updating

- `ddlActiveRoastDates` → `ddlActivePrepDates`
- `tbxRoastDate` → `tbxPrepDate`
- `lblRoastDate` → `lblPrepDate`
- `btnRoastDate*` → `btnPrepDate*`

### Database/Repository Pattern Notes

The `OrdersRepository.cs` already correctly uses: