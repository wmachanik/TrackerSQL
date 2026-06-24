# Contributing to TrackerSQL

## CRITICAL: City → Area Refactoring (2026-05-11)

A comprehensive refactoring has renamed all "City" terminology to "Area" throughout the codebase. **This is mandatory for all contributions.**

### Must Know Before Contributing

When you see references to:
- ❌ `CityTbl` → Use `AreasTbl`
- ❌ `CityTblDAL` → Use `AreasRepository`
- ❌ `CityPrepDaysTbl` → Use `AreaPrepDaysTbl`
- ❌ `cityId` variable → Use `areaId`

See `Documentation/AI_CONTEXT.md` section "CRITICAL REFACTORING: City → Area Namespace Rename" for complete mapping.

### Code Review Checklist

Before submitting code, verify:
- [ ] No references to legacy `CityTbl`, `CityPrepDaysTbl`, or `CityTblDAL`
- [ ] All area-related queries use `AreasTbl` and `AreaPrepDaysTbl`
- [ ] Variable names use "area" not "city"
- [ ] New repositories use `AreasRepository` for prep rule lookups
- [ ] No hardcoded SQL referencing old table names

### Breaking Changes Since Last Session

**If you see SQL errors about "Invalid object name 'CityPrepDaysTbl'":**
- This means code is still using old table names
- Update all SQL queries to use `AreasTbl` and `AreaPrepDaysTbl`
- Check `DateCalculator.cs` and `RecurringOrdersRepository.cs` as examples of correct usage

---

## SPELLING STANDARD: "Recurring" NOT "Reoccurring" (2026-05-15)

All references to recurring dates, orders, and operations use the correct spelling: **"Recurring"** (NOT "Reoccurring").

### What Changed

Column renames across all related tables:
- ❌ `LastReoccurringDate` → ✅ `LastRecurringDate`
- ❌ `DoReoccuringOrders` → ✅ `DoRecurringOrders` (gradual - still in progress for backwards compatibility)

Property names in POCOs:
- ❌ `LastReoccurringDate` → ✅ `LastRecurringDate`

### What to Watch For

When contributing or reviewing code:
- [ ] Use "Recurring" in all new property names and variable names
- [ ] Use "Recurring" in all new database column names
- [ ] Update existing references to use "Recurring" spelling during refactoring
- [ ] Check `RecurringOrdersRepository.cs` and `RecurringOrder.cs` POCO for examples
- [ ] Verify SQL column names in queries match target database schema

### Breaking Changes

**If you see compilation errors with "Reoccurring":**
- Legacy class names like `ReoccuringOrderDAL` and `ReoccuringOrderTbl` are deprecated
- Use `RecurringOrdersRepository` instead
- Update column references from `DoReoccuringOrders` to `DoRecurringOrders`

---

## MANDATORY: Repository Pattern ONLY (No OleDb, No SqlDataSource)

All data access **MUST use the Repository pattern**. NO exceptions.

### HARD RULES

1. ❌ **NO Access Database** - Migrating FROM Access TO SQL Server only
2. ❌ **NO OleDb Classes** - Use SqlClient only
3. ❌ **NO SqlDataSource Controls** - Use Repository manual binding
4. ❌ **NO Legacy ObjectDataSource** - Use Repository classes with manual binding
5. ❌ **NO TrackerDb Direct Usage** - Only use TrackerSQLDb in Repositories
6. ✅ **USE Repository Pattern** - All data access through repositories in `Classes/Sql/`

### When Migrating a Page

1. Identify all data access in code-behind and markup
2. Create or identify the required Repository class
3. Remove all `SqlDataSource` and `ObjectDataSource` controls
4. Update GridView/DropDownList binding in code-behind
5. Use `DataSource = repo.GetAll(); DataBind();` pattern
6. Test manually with both read and write operations

---

## CRITICAL: Delete Legacy Controls Classes (2026-05-25)

Legacy table wrapper classes in the `Controls/` folder are being removed. **Do NOT create new ones or use existing ones.**

### Classes to Delete

- ❌ `Controls/ContactType.cs` - **SCHEDULED FOR DELETION**
  - Reason: 100% legacy Access code using `TrackerDb` and old table names
  - Replacement: Use `ContactTypesRepository` from `Classes/Sql/`
  - Uses: Only referenced in deprecated `btnSetClientType_Click` handler

### Classes Still Active (For Now)

The following legacy classes remain temporarily but **should NOT be used for new code**:
- `Controls/ReoccuringOrderDAL.cs` - Use `RecurringOrdersRepository` instead
- `Controls/NextPreperationDateByAreaTbl.cs` - Use `NextPreperationDateByAreasRepository` instead (if exists) or create
- `Controls/ClientUsageLinesTbl.cs` - Use `ContactsItemSvcSummaryRepository` instead
- `Controls/ItemUsageTbl.cs` - Use `ContactsItemUsageRepository` instead
- Other `*Tbl.cs` classes in Controls folder

### Migration Strategy

**For each legacy Controls class found:**
1. Check if there's a modern Repository equivalent in `Classes/Sql/`
2. If yes: Update calling code to use Repository instead
3. If no: Create new Repository class following established patterns
4. Delete the legacy Controls class after all references updated
5. Document the deletion in commit message

### Example: ContactType Deletion

**Before (Legacy):**
```csharp
// DON'T USE THIS
var contactType = new TrackerSQL.Controls.ContactType();
var allContacts = contactType.GetAllContacts("CompanyName");
```

**After (Modern):**
```csharp
// USE THIS
var repo = new ContactTypesRepository();
var allContacts = repo.GetAll();
```

---

## LOGGING STANDARD: SystemConstants.LogTypes (2026-05-25)

The correct namespace for logging constants is: **`SystemConstants.LogTypes`**

### What Changed

❌ **WRONG:** `SystemConstants.Log.Types.System`  
❌ **WRONG:** `SystemConstants.LogTypes.System`  (typo: "Log.Types" instead of "LogTypes")  
✅ **CORRECT:** `SystemConstants.LogTypes.System`

### When Logging

Always use the correct syntax:

```csharp
// CORRECT
AppLogger.WriteLog(SystemConstants.LogTypes.System, "Message");
AppLogger.WriteLog(SystemConstants.LogTypes.Database, "Message");
AppLogger.WriteLog(SystemConstants.LogTypes.Error, "Message");

// WRONG - Don't do this
AppLogger.WriteLog(SystemConstants.Log.Types.System, "Message");  // ❌ Log.Types doesn't exist
AppLogger.WriteLog(SystemConstants.LogTypes.System, "Message");   // ❌ Typo/autocorrect error
```

### Code Review Checklist

When reviewing code with logging:
- [ ] Check `AppLogger.WriteLog()` calls use `SystemConstants.LogTypes`
- [ ] Verify no typos like `Log.Types` or `LogType` (singular)
- [ ] Confirm valid log type: `System`, `Database`, `Error`, etc.

### IntelliSense Fix

If your IDE autocompletes to `SystemConstants.Log.Types`:
1. This is a namespace resolution issue
2. Manually type `SystemConstants.LogTypes` instead
3. The correct class is `LogTypes` (not `Log` → `Types`)

---

## SYSTEM TOOLS CLEANUP (2026-05-25)

The System Tools page (`Tools/SystemTools.aspx`) provides 11 system administration utilities. Several need cleanup or removal.

### Tools Cleanup Status

#### ✅ KEEP - Working & Needed

- **System Data** - Essential system configuration tool
- **Log Viewer** - Useful for diagnostics
- **Messages Editor** - Resource management (role-restricted)
- **Email Diagnostics** - SMTP testing
- **Holiday/Closure Dates** - Business calendar management
- **Disable Inactive Clients** - Recently modernized (Repository pattern)
- **Set Last Order Date** - Recurring order maintenance

#### ⚠️ FIX REQUIRED - Known Issues

**Reset Prep/Delivery Date**
- **Issue:** Shows success message but no count of updated records
- **Fix:** Add count of records updated to status message
- **Expected:** "SystemTools: Prep/Delivery dates reset. [X] records updated."

**Move Delivery Date**
- **Issue:** Crashes (likely legacy Controls class issue)
- **Legacy Class:** `NextPreperationDateByAreaTbl` - deprecated
- **Fix Options:**
  1. Rewrite using modern repository (if exists)
  2. Create new repository if doesn't exist
  3. Or remove if not critical

#### ❌ REMOVE - Not Needed

**Merge Customers From QB**
- **Issue:** QuickBooks sync no longer needed (based on current business)
- **Action:** Remove button and page entirely
- **Files to Delete:**
  - `Tools/MergeCustomersFromQB.aspx`
  - `Tools/MergeCustomersFromQB.aspx.cs`

#### ⚠️ UNCLEAR - Needs Clarification

**Set Client Type** (Currently Deprecated)
- **Current Status:** Marked `[Obsolete]`, shows "feature disabled" message
- **Purpose:** Unclear - automatically updates contact types based on usage patterns
- **Options:**
  1. Remove entirely (if not needed by business)
  2. Rename to "Set Contact Type" (terminology alignment)
  3. Rewrite with modern repositories (if needed)
- **Decision:** User to determine based on business need

---

## System Tools Migration - Phase 2 Plan

### Priority 1: Remove Merge QB Tool

```
Steps:
1. Delete button from SystemTools.aspx (lines 76-79)
2. Delete page files:
   - Tools/MergeCustomersFromQB.aspx
   - Tools/MergeCustomersFromQB.aspx.cs
3. Commit with message:
   "Remove: Delete MergeCustomersFromQB tool - no longer needed"
```

### Priority 2: Fix Reset Prep/Delivery Date

**Current Code:**
```csharp
new TrackerTools().SetNextPreperationDateByArea();
this.ltrlStatus.Text = "SystemTools: Prep/Delivery dates reset.";
```

**Improved Code:**
```csharp
int recordsUpdated = new TrackerTools().SetNextPreperationDateByArea();
this.ltrlStatus.Text = $"SystemTools: Prep/Delivery dates reset. {recordsUpdated} records updated.";
```

**Note:** Requires `SetNextPreperationDateByArea()` to return row count

### Priority 3: Fix Move Delivery Date Crash

**Problem:** Uses `NextPreperationDateByAreaTbl` (legacy Controls class)

**Solution:**
- Option A: Create `NextPreperationDateByAreasRepository` (modern)
- Option B: Fix legacy class to use SQL Server names
- Option C: Remove tool if not critical

### Priority 4: Clarify Set Client Type

**Decision Needed:**
- Is this feature used by business? Y/N
- Should it be renamed "Set Contact Type"? Y/N
- Should handler be rewritten or removed?

---

## Standard Table Name Mappings (Quick Reference)

### Common Changes for System Tools Pages

| Legacy Access Name | SQL Server Name | Legacy Class | New Repository |
|---|---|---|---|
| `CustomersTbl` | `ContactsTbl` | `CustomersTbl` | `ContactsRepository` |
| `CustomersAccInfoTbl` | `ContactsAccInfoTbl` | - | `ContactsAccInfoRepository` |
| `CityTbl` | `AreasTbl` | - | `AreasRepository` |
| `CityPrepDaysTbl` | `AreaPrepDaysTbl` | - | `AreaPrepDaysRepository` |
| `ItemTypeTbl` | `ItemsTbl` | - | `ItemsRepository` |
| `ItemUsageTbl` | `ContactsItemUsageTbl` | - | `ContactsItemUsageRepository` |
| `ClientUsageLinesTbl` | `ContactsItemSvcSummaryTbl` | - | `ContactsItemSvcSummaryRepository` |
| `CustomerTypeTbl` | `ContactTypesTbl` | - | `ContactTypesRepository` |
| `ReoccuringOrderTbl` | `RecurringOrdersTbl` | `ReoccuringOrderDAL` | `RecurringOrdersRepository` |
| `NextRoastDateByCityTbl` | `NextPreperationDateByAreasTbl` | `NextPreperationDateByAreaTbl` | `NextPreperationDateByAreasRepository` (needs creation) |
| `PackagingTbl` | `ItemPackagingsTbl` | - | `ItemPackagingsRepository` |
| `PrepTypesTbl` | `ItemPrepTypesTbl` | - | `ItemPrepTypesRepository` |
| `MachineConditionsTbl` | `EquipConditionsTbl` | - | `EquipConditionsRepository` |
| `ReoccuranceTypeTbl` | `RecurranceTypesTbl` | - | `RecurranceTypesRepository` |

---

## Commit Message Template for System Tools Cleanup

```
Refactor: Clean up System Tools page - remove QB integration, fix messaging

Changes:
- Removed: MergeCustomersFromQB tool (no longer needed)
- Fixed: Reset Prep/Delivery Date now shows count of updated records
- Fixed: Move Delivery Date (migrate to modern repository)
- Deprecated: Set Client Type (unclear purpose - marked for review)

Verified:
- All remaining tools tested and working
- Logging uses SystemConstants.LogTypes (not Log.Types)
- No legacy Access database calls in active handlers

Testing:
- Tested all 7 active system tools
- Verified button visibility and role restrictions
- Confirmed error handling and user messages

Files Modified:
- Tools/SystemTools.aspx - Removed QB button
- Tools/SystemTools.aspx.cs - Improved status messages
- Tools/MoveDeliveryDate.aspx.cs - Migrated to repository (if applicable)

Files Deleted:
- Tools/MergeCustomersFromQB.aspx
- Tools/MergeCustomersFromQB.aspx.cs

Task: System Tools Modernization - Phase 2
```

---

**These guidelines are MANDATORY for all System Tools contributions. No exceptions.**

**Last Updated:** 2026-05-25  
**Version:** 1.5  
**Status:** ACTIVE