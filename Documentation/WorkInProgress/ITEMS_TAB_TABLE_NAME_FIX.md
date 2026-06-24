# ?? Fixed: Items Tab Table Name Error

**Date:** 2025-03-26  
**Issue:** `Invalid object name 'ItemTypeTbl'`  
**Status:** ? **FIXED**  

---

## ?? Problem

When loading `Lookups.aspx`, the page crashed with:

```
Invalid object name 'ItemTypeTbl'.
```

**Root Cause:**
The SqlDataSource `sdsItems` was referencing the **old Access database table name** `ItemTypeTbl`, but the SQL Server database uses `ItemsTbl` with different column names.

---

## ?? Table Name Mapping

| Access Database | SQL Server Database |
|----------------|-------------------|
| `ItemTypeTbl` | `ItemsTbl` |
| `ItemTypeID` (PK) | `ItemID` (PK) |
| `ServiceTypeId` | `ItemServiceTypeID` |
| `ReplacementID` | `ReplacementItemID` |

---

## ? Changes Made

### 1. Updated `sdsItems` SqlDataSource

**File:** `Pages/Lookups.aspx`

**Changed:**
- Table name: `ItemTypeTbl` ? `ItemsTbl`
- Primary key: `ItemTypeID` ? `ItemID`
- Column: `ServiceTypeId` ? `ItemServiceTypeID` (with alias `AS ServiceTypeId`)
- Column: `ReplacementID` ? `ReplacementItemID` (with alias `AS Replacement`)

**Before:**
```sql
SELECT ItemTypeID, ..., ServiceTypeId, ... FROM ItemTypeTbl WHERE ...
UPDATE ItemTypeTbl SET ... WHERE (ItemTypeID = @original_ItemTypeID)
DELETE FROM ItemTypeTbl WHERE ItemTypeID = @original_ItemTypeID
```

**After:**
```sql
SELECT ItemID, ..., ItemServiceTypeID AS ServiceTypeId, ... FROM ItemsTbl WHERE ...
UPDATE ItemsTbl SET ... WHERE (ItemID = @original_ItemID)
DELETE FROM ItemsTbl WHERE ItemID = @original_ItemID
```

---

### 2. Updated GridView DataKeyNames

**Before:**
```aspx
<asp:GridView ... DataKeyNames="ItemTypeID">
    <asp:BoundField DataField="ItemTypeID" ... />
```

**After:**
```aspx
<asp:GridView ... DataKeyNames="ItemID">
    <asp:BoundField DataField="ItemID" ... />
```

---

### 3. Updated DetailsView DataKeyNames

**Before:**
```aspx
<asp:DetailsView ... DataKeyNames="ItemTypeID" ...>
```

**After:**
```aspx
<asp:DetailsView ... DataKeyNames="ItemID" ...>
```

---

### 4. Updated `sdsReplacementItems` SqlDataSource

**Before:**
```sql
SELECT [ItemTypeID], [ItemDesc] FROM [ItemTypeTbl] ORDER BY [ItemDesc]
```

**After:**
```sql
SELECT [ItemID] AS ItemTypeID, [ItemDesc] FROM [ItemsTbl] ORDER BY [ItemDesc]
```

**Note:** Used alias `AS ItemTypeID` to maintain compatibility with existing dropdown bindings.

---

## ?? Why This Happened

During the Access ? SQL Server migration:
1. Table was renamed from `ItemTypeTbl` to `ItemsTbl`
2. Columns were renamed to match new naming conventions
3. The SqlDataSource in Lookups.aspx **was not updated** to use the new names

This is exactly why we created the **ARCHITECTURE_RULES.md** to mandate Repository pattern and eliminate SqlDataSource!

---

## ? Verification

**Build Status:** ? Successful

**Expected Result:**
- Items tab should now load without errors
- Grid should display items from `ItemsTbl`
- CRUD operations should work correctly

---

## ?? Still Remaining (Per Architecture Rules)

Following our new **mandatory Repository Pattern rule**, these should be refactored in Phase 2:

### Items Tab - Remaining Legacy DataSources

1. **`sdsItems`** (SqlDataSource)
   - Status: ? Fixed table name, ?? Still uses SqlDataSource
   - Action: Refactor to use `ItemsRepository` in Phase 2

2. **`sdsReplacementItems`** (SqlDataSource)
   - Status: ? Fixed table name, ?? Still uses SqlDataSource
   - Action: Populate dropdown in code-behind using `ItemsRepository`

3. **`sdsServiceTypes`** (SqlDataSource)
   - Status: ?? Still uses SqlDataSource
   - Action: Create `ServiceTypesRepository` and populate in code-behind

4. **`odsAllItems`** (ObjectDataSource ? legacy `ItemTypeTbl`)
   - Status: ? Still points to legacy Table Class
   - Action: Remove and populate dropdown using `ItemsRepository`

5. **`odsItemUnits`** (referenced in ASPX but removed from data sources)
   - Status: ? Already removed
   - Action: None needed

---

## ?? Next Steps

### Immediate (Testing)
1. **Test Items tab** - Verify page loads
2. **Test CRUD operations** - Add, Edit, Update, Delete items
3. **Test search** - Verify search functionality works
4. **Test dropdowns** - ServiceType, Replacement, Units dropdowns

### Phase 2 (Full Repository Compliance)
1. Remove `sdsItems` SqlDataSource
2. Bind grid in code-behind using `ItemsRepository`
3. Remove all remaining SqlDataSource controls
4. Follow **ARCHITECTURE_RULES.md** mandate

---

## ?? Related Documents

- [ARCHITECTURE_RULES.md](../ARCHITECTURE_RULES.md) - Mandatory Repository Pattern
- [TABLE_SCHEMA_REFERENCE.md](../TABLE_SCHEMA_REFERENCE.md) - Table/column mappings
- [LOOKUPS_COMPLETE_ALL_TABS.md](LOOKUPS_COMPLETE_ALL_TABS.md) - Overall refactoring status

---

## ?? Lesson Learned

**This error demonstrates exactly why SqlDataSource is problematic:**

? **SqlDataSource Problems:**
- Table names hardcoded in markup
- No compile-time checking
- Runtime errors only discovered when page loads
- Hard to find/update all references

? **Repository Pattern Benefits:**
- Table names in ONE place (Repository class)
- Compile-time type checking
- Errors caught during build
- Easy to refactor

**This is a perfect example of why our new ARCHITECTURE_RULES.md mandate is critical!**

---

## ? Summary

**Issue:** Items tab crashed due to incorrect table name  
**Root Cause:** SqlDataSource referenced Access table names instead of SQL Server  
**Fix:** Updated table/column names to match SQL Server schema  
**Build:** ? Successful  
**Testing:** Ready for testing  

**Next:** Follow ARCHITECTURE_RULES.md and refactor to Repository Pattern in Phase 2! ???
