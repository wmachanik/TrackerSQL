# ? Lookups.aspx Comprehensive Refactoring - PARTIAL COMPLETE

**Date:** 2025-03-26  
**Status:** ?? **Build Successful - Partial Completion**  
**Problem:** Items and Cities tabs still use Access SqlDataSource

---

## ?? What Was Completed

Successfully refactored **3 additional tabs** to use Repository pattern:

1. ? **People** - PersonsRepository 
2. ? **Packaging** - ItemPackagingsRepository
3. ? **RepairStatuses** - RepairStatusesRepository

Combined with previous Phase 1 work:
4. ? **Equipment** - EquipTypesRepository
5. ? **InvoiceTypes** - InvoiceTypesRepository
6. ? **PaymentTerms** - PaymentTermsRepository
7. ? **PriceLevels** - PriceLevelsRepository

**Total: 7 out of 9 tabs complete!**

---

## ? Repositories Enhanced

### 1. PersonsRepository
- Added Insert(), Update(), Delete() methods
- Added GetAll() override with SQL alias (`Person AS PersonName`)
- Maps PeopleTbl ? Person POCO

### 2. ItemPackagingsRepository  
- Added Insert(), Update(), Delete() methods
- Already had GetAll() override with aliases
- Maps ItemPackagingsTbl ? ItemPackaging POCO

### 3. RepairStatusesRepository (NEW!)
- Created complete repository from scratch
- Insert(), Update(), Delete(), GetAll() methods
- Maps RepairStatusesTbl ? RepairStatus POCO

---

## ?? Files Modified

### Repositories (3 files)
- ? `Classes\Sql\PersonsRepository.cs` - Enhanced
- ? `Classes\Sql\ItemPackagingsRepository.cs` - Enhanced  
- ? `Classes\Sql\RepairStatusesRepository.cs` - **Created**

### Pages (2 files)
- ? `Pages\Lookups.aspx` - Updated ObjectDataSource definitions
- ? `Pages\Lookups.aspx.cs` - Updated event handlers

---

## ? Still Problematic - Access Database References

### Items Tab
**Issue:** Uses `sdsItems` (SqlDataSource) pointing to Access database

```aspx
<asp:SqlDataSource ID="sdsItems" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
    ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
    SelectCommand="SELECT ItemTypeID, ItemDesc, SKU... FROM ItemTypeTbl WHERE (ItemDesc LIKE ?) ORDER BY SortOrder, ItemDesc"
    ...
/>
```

**Solution Options:**
1. Change to use SQL Server connection string
2. Refactor to use ItemsRepository (complex - has search functionality)

---

### Cities Tab
**Issue:** Uses `sdsCities` (SqlDataSource) pointing to Access database

```aspx
<asp:SqlDataSource ID="sdsCities" runat="server"
    ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>"
    ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>"
    SelectCommand="SELECT [ID], [Area] FROM [AreaTbl] ORDER BY [Area]"
    ...
/>
```

Plus `odsAreaDays` uses `AreaPrepDaysTbl` (legacy Table Class)

**Solution Options:**
1. Change to use SQL Server connection string
2. Refactor to use AreasRepository + AreaPrepDaysRepository

---

## ?? Quick Fix to Make Page Load

### Option A: Change Connection String in Web.config

Update `Tracker08ConnectionString` to point to SQL Server instead of Access:

```xml
<!-- OLD (Access - disabled) -->
<add name="Tracker08ConnectionString" 
     connectionString="Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\SRC\Data\QuaffeeTracker08.mdbx" 
     providerName="System.Data.OleDb"/>

<!-- NEW (SQL Server) -->
<add name="Tracker08ConnectionString" 
     connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\SRC\Data\TrackerDB.mdf;Integrated Security=True" 
     providerName="System.Data.SqlClient"/>
```

**Pros:** Immediate fix, page will load  
**Cons:** Mixed approach, not fully refactored

---

### Option B: Complete Repository Refactoring

Refactor Items and Cities tabs to use Repositories:

**Items Tab:**
- Use existing `ItemsRepository`
- Create custom search method in repository
- Update `gvItems_RowCommand` handler
- Remove SqlDataSource, bind in code-behind

**Cities Tab:**
- Use existing `AreasRepository` 
- Create/use `AreaPrepDaysRepository` for the related grid
- Update handlers
- Remove SqlDataSource

**Pros:** Clean, consistent architecture  
**Cons:** More work, complex (Items has search)

---

## ?? Current State

### Working Tabs (7)
1. ? Equipment
2. ? InvoiceTypes
3. ? PaymentTerms
4. ? PriceLevels
5. ? People
6. ? Packaging
7. ? RepairStatuses

### Not Working (2)
1. ? Items - Access SqlDataSource
2. ? Cities - Access SqlDataSource

---

## ?? Recommendation

**Immediate:** Option A - Change connection string to SQL Server for quick testing

**Long-term:** Option B - Complete refactoring for clean architecture

---

## ? Build Status

**Build:** ? Successful  
**Runtime:** ? Will fail on Items/Cities tabs (Access connection error)

---

## ?? Testing Plan

If we implement **Option A** (change connection string):

1. Update Web.config connection string
2. Test all 9 tabs
3. Verify CRUD operations work on all tabs

If we implement **Option B** (complete refactoring):

1. Refactor Items tab (complex - has search)
2. Refactor Cities tab (medium - has related grid)
3. Test all 9 tabs
4. Verify CRUD operations

---

## ?? Next Steps

**Choose one:**

### Quick Win (30 minutes)
- Change Tracker08ConnectionString in Web.config to SQL Server
- Test all tabs
- Move forward with other work

### Complete Solution (2-3 hours)
- Refactor Items tab to use ItemsRepository
- Refactor Cities tab to use AreasRepository  
- Remove all SqlDataSource references
- Fully clean architecture

**Which approach do you prefer?**

