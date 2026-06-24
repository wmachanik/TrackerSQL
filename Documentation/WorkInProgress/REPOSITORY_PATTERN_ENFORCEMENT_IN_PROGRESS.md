# ??? MANDATORY REPOSITORY PATTERN - ENFORCEMENT IN PROGRESS

**Date:** 2025-03-26  
**Status:** ?? **PARTIAL - COMPLETION REQUIRED**  
**Rule:** ZERO SqlDataSource/ObjectDataSource allowed (except ASP.NET membership)

---

## ? What Was Accomplished

### 1. **PROJECT_OVERVIEW.md Updated**
Added **CRITICAL PROJECT RULE** section at the top:
- Repository Pattern is MANDATORY
- SqlDataSource/ObjectDataSource PROHIBITED  
- NO EXCEPTIONS

### 2. **Lookups.aspx - DataSourceID Removed from Controls**
? **Items Tab:**
- Removed `DataSourceID="sdsItems"` from `gvItems`
- Removed `DataSourceID="sdsServiceTypes"` from all ServiceType dropdowns
- Removed `DataSourceID="odsAllItems"` from all Replacement dropdowns
- Removed `DataSourceID="odsItemUnits"` from all Units dropdowns
- Added paging/sorting event handlers: `OnPageIndexChanging`, `OnSorting`
- Fixed `DataValueField` from `ItemTypeID` ? `ItemID`

? **People Tab:**
- Removed `DataSourceID="sdsUserNames"` from SecurityUsername dropdowns
  - NOTE: sdsUserNames queries ASP.NET membership DB - may keep for now

? **Cities Tab:**
- Removed `DataSourceID="sdsCities"` from `gvCities`
- Added paging/sorting event handlers

### 3. **DetailsView in EmptyDataTemplate**
- Removed `DataSourceID="sdsItems"` from `dvItemIns`
- Removed DataSourceID from all DetailsView dropdowns

---

## ? What Still Needs to Be Done

### Step 1: Delete Remaining SqlDataSource Controls

**In Pages/Lookups.aspx (around line 1290-1312):**

DELETE these lines:
```aspx
<asp:SqlDataSource ID="sdsServiceTypes" runat="server" ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    ProviderName="<%$ ConnectionStrings:TrackerDataSQL.ProviderName %>"
    SelectCommand="SELECT [ServiceTypeId], [ServiceType] FROM [ServiceTypesTbl]"></asp:SqlDataSource>

<asp:SqlDataSource ID="sdsReplacementItems" runat="server" ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    ProviderName="<%$ ConnectionStrings:TrackerDataSQL.ProviderName %>"
    SelectCommand="SELECT [ItemID] AS ItemTypeID, [ItemDesc] FROM [ItemsTbl] ORDER BY [ItemDesc]"></asp:SqlDataSource>

<asp:SqlDataSource ID="sdsCities" runat="server" OnSelecting="sdsCities_Selecting"
    ConflictDetection="CompareAllValues" ConnectionString="<%$ ConnectionStrings:TrackerDataSQL %>"
    DeleteCommand="DELETE FROM [AreaTbl] WHERE [ID] = @ID" InsertCommand="INSERT INTO AreaTbl(Area) VALUES (@Area)"
    OldValuesParameterFormatString="original_{0}" ProviderName="<%$ ConnectionStrings:TrackerDataSQL.ProviderName %>"
    SelectCommand="SELECT [ID], [Area] FROM [AreaTbl] ORDER BY [Area]"
    UpdateCommand="UPDATE [AreaTbl] SET [Area] = @Area WHERE [ID] = @ID">
    <DeleteParameters>
        <asp:Parameter Name="ID" Type="Int32" />
    </DeleteParameters>
    <InsertParameters>
        <asp:Parameter Name="Area" Type="String" />
    </InsertParameters>
    <UpdateParameters>
        <asp:Parameter Name="Area" Type="String" />
        <asp:Parameter Name="ID" Type="Int32" />
    </UpdateParameters>
</asp:SqlDataSource>
```

**Also search for and delete:**
- `sdsItems` (SqlDataSource for Items grid - should be around line 1224-1265)
- `odsAllItems` (ObjectDataSource ? legacy ItemTypeTbl - around line 1266-1270)

---

### Step 2: Search for `sdsItems` in Lookups.aspx

The Items tab SqlDataSource wasn't found in the earlier replacements. It likely still exists.

**Manual search needed:**
1. Open Pages/Lookups.aspx
2. Search for: `<asp:SqlDataSource ID="sdsItems"`
3. Delete the entire `<asp:SqlDataSource>` block including all parameters
4. Do the same for `odsAllItems`

---

### Step 3: Create Missing Repositories

**ServiceTypesRepository** doesn't exist yet:

```csharp
// File: Classes/Sql/ServiceTypesRepository.cs
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerDotNet.Classes.Poco;

namespace TrackerDotNet.Classes.Sql
{
    public class ServiceTypesRepository : RepositoryBase<ServiceType>
    {
        protected override string TableName => "ServiceTypesTbl";
        protected override string KeyColumn => "ServiceTypeId";
        protected override string CoreColumns => "ServiceTypeId, ServiceType";
        protected override string LookupColumns => "ServiceTypeId, ServiceType";

        public void Insert(ServiceType serviceType)
        {
            const string sql = "INSERT INTO ServiceTypesTbl (ServiceType) VALUES (@ServiceType)";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ServiceType", DataValue = serviceType.ServiceType, DataDbType = DbType.String }
            };
            
            ExecNonQuery(sql, parameters);
        }

        public void Update(ServiceType serviceType)
        {
            const string sql = "UPDATE ServiceTypesTbl SET ServiceType = @ServiceType WHERE ServiceTypeId = @ServiceTypeId";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ServiceType", DataValue = serviceType.ServiceType, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ServiceTypeId", DataValue = serviceType.ServiceTypeId, DataDbType = DbType.Int32 }
            };
            
            ExecNonQuery(sql, parameters);
        }

        public void Delete(int serviceTypeId)
        {
            const string sql = "DELETE FROM ServiceTypesTbl WHERE ServiceTypeId = @ServiceTypeId";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ServiceTypeId", DataValue = serviceTypeId, DataDbType = DbType.Int32 }
            };
            
            ExecNonQuery(sql, parameters);
        }
    }
}
```

**ServiceType POCO** (check if exists, if not create):

```csharp
// File: Classes/Poco/ServiceType.cs
namespace TrackerDotNet.Classes.Poco
{
    public class ServiceType
    {
        public int ServiceTypeId { get; set; }
        public string ServiceType { get; set; }
    }
}
```

**AreasRepository** for Cities (or continue using AreaTbl):

Check if this exists or needs to be created.

---

### Step 4: Update Code-Behind (Lookups.aspx.cs)

**Add using statements:**
```csharp
using TrackerDotNet.Classes.Sql;
using TrackerDotNet.Classes.Poco;
```

**Page_Load - Bind all grids:**
```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
    {
        BindItemsGrid();
        BindCitiesGrid();
        BindDropdowns();
    }
}
```

**Items Grid Binding:**
```csharp
private void BindItemsGrid()
{
    var repo = new ItemsRepository();
    var searchTerm = Session["SearchItemContains"]?.ToString() ?? "%";
    
    var items = repo.GetAll("SortOrder, ItemDesc"); // Apply search filter in GetAll method
    gvItems.DataSource = items;
    gvItems.DataBind();
}

protected void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
{
    gvItems.PageIndex = e.NewPageIndex;
    BindItemsGrid();
}

protected void gvItems_Sorting(object sender, GridViewSortEventArgs e)
{
    // Implement sorting logic
    BindItemsGrid();
}
```

**Cities Grid Binding:**
```csharp
private void BindCitiesGrid()
{
    // Use AreasRepository or create AreaRepository
    // For now, manual SQL might be needed
    gvCities.DataSource = GetCities(); // Implement this method
    gvCities.DataBind();
}

protected void gvCities_PageIndexChanging(object sender, GridViewPageEventArgs e)
{
    gvCities.PageIndex = e.NewPageIndex;
    BindCitiesGrid();
}

protected void gvCities_Sorting(object sender, GridViewSortEventArgs e)
{
    BindCitiesGrid();
}
```

**Dropdown Binding (call from Page_Load):**
```csharp
private void BindDropdowns()
{
    BindServiceTypeDropdowns();
    BindReplacementDropdowns();
    BindUnitsDropdowns();
    BindSecurityUsernamesDropdowns();
}

private void BindServiceTypeDropdowns()
{
    var repo = new ServiceTypesRepository();
    var serviceTypes = repo.GetAll("ServiceType");
    
    // Bind to all ServiceType dropdowns in Items tab
    // This is complex - need to find all ddlServiceType controls in GridView rows
}

private void BindReplacementDropdowns()
{
    var repo = new ItemsRepository();
    var items = repo.GetAll("ItemDesc");
    
    // Bind to all ddlReplacement controls
}

private void BindUnitsDropdowns()
{
    var repo = new ItemUnitsRepository();
    var units = repo.GetAll("UnitOfMeasure");
    
    // Bind to all ddlUnits controls
}

private void BindSecurityUsernamesDropdowns()
{
    // Query ASP.NET membership database
    // May keep sdsUserNames for this
}
```

**CRUD Operations for Items:**
```csharp
protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
{
    var repo = new ItemsRepository();
    
    if (e.CommandName.Equals("AddItem"))
    {
        // Extract values from footer row
        GridViewRow footerRow = gvItems.FooterRow;
        var tbxItem = (TextBox)footerRow.FindControl("tbxItem");
        var tbxSKU = (TextBox)footerRow.FindControl("tbxSKU");
        // ... extract all values
        
        var newItem = new Item
        {
            ItemDesc = tbxItem.Text,
            SKU = tbxSKU.Text,
            // ... set all properties
        };
        
        repo.Insert(newItem);
        BindItemsGrid();
    }
    else if (e.CommandName.Equals("Update"))
    {
        // Handle update
    }
    else if (e.CommandName.Equals("Delete"))
    {
        // Handle delete
    }
}
```

---

### Step 5: Remove `sdsCities_Selecting` Event Handler

In Lookups.aspx.cs, find and **delete**:
```csharp
protected void sdsCities_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
{
    // This is no longer needed
}
```

---

## ?? Checklist to Complete 100% Compliance

### ASPX Cleanup
- [ ] **Delete** `sdsItems` SqlDataSource (search entire file)
- [ ] **Delete** `odsAllItems` ObjectDataSource  
- [ ] **Delete** `sdsServiceTypes` SqlDataSource
- [ ] **Delete** `sdsReplacementItems` SqlDataSource
- [ ] **Delete** `sdsCities` SqlDataSource
- [ ] **Keep (for now)** `sdsUserNames` - ASP.NET membership database
- [ ] **Keep** All Repository-based ObjectDataSource controls:
  - `odsPeople` ? PersonsRepository ?
  - `odsEquipTypes` ? EquipTypesRepository ?
  - `odsInvoiceTypes` ? InvoiceTypesRepository ?
  - `odsPaymentTerms` ? PaymentTermsRepository ?
  - `odsPriceLevels` ? PriceLevelsRepository ?
  - `odsPackaging` ? ItemPackagingsRepository ?
  - `odsRepairStatuses` ? RepairStatusesRepository ?
  - `odsAreaDays` ? AreaPrepDaysTbl (legacy, refactor in Phase 2)

### Repository Creation
- [ ] Create `ServiceTypesRepository.cs`
- [ ] Create `ServiceType.cs` POCO
- [ ] Create `AreasRepository.cs` (or `CitiesRepository.cs`)
- [ ] Create `Area.cs` (or `Area.cs`) POCO

### Code-Behind Updates
- [ ] Add `BindItemsGrid()` method
- [ ] Add `BindCitiesGrid()` method
- [ ] Add `BindDropdowns()` and sub-methods
- [ ] Implement `gvItems_PageIndexChanging`
- [ ] Implement `gvItems_Sorting`
- [ ] Implement `gvCities_PageIndexChanging`
- [ ] Implement `gvCities_Sorting`
- [ ] Update `gvItems_RowCommand` for CRUD operations
- [ ] Update `gvCities_OnRowCommand` for CRUD operations
- [ ] Remove `sdsCities_Selecting` event handler

### Testing
- [ ] **Items Tab** - Load, search, page, sort, add, edit, delete
- [ ] **Cities Tab** - Load, page, sort, add, edit, delete, select (for AreaDays)
- [ ] **All Dropdowns** - Populate correctly on page load and during grid operations
- [ ] **Build** - No compilation errors
- [ ] **Runtime** - No missing DataSource errors

---

## ?? Critical Note

**The ASPX file still has DataSourceID references removed, but the SqlDataSource controls themselves are still in the file.**

This will cause **runtime errors** until:
1. The SqlDataSource controls are deleted from the ASPX
2. The code-behind is updated to bind grids manually
3. Dropdowns are populated in code-behind

**DO NOT run the application until all steps are completed!**

---

## ?? Related Documents

- [ARCHITECTURE_RULES.md](../ARCHITECTURE_RULES.md) - The mandatory rule
- [PROJECT_OVERVIEW.md](../PROJECT_OVERVIEW.md) - Now includes rule at top
- [ITEMS_TAB_TABLE_NAME_FIX.md](ITEMS_TAB_TABLE_NAME_FIX.md) - Related fix

---

## ?? Next Actions

**Priority 1 (BLOCKING):**
1. Manually delete the 5 SqlDataSource controls from Lookups.aspx
2. Delete `odsAllItems` ObjectDataSource

**Priority 2 (REQUIRED):**
3. Create `ServiceTypesRepository` and `ServiceType` POCO
4. Create `AreasRepository`/`CitiesRepository`

**Priority 3 (IMPLEMENTATION):**
5. Update Lookups.aspx.cs with all binding methods
6. Implement CRUD event handlers
7. Test thoroughly

**Estimated Time:** 2-3 hours of focused work

---

**Status:** ?? **INCOMPLETE - DO NOT MERGE OR RUN UNTIL FINISHED**
