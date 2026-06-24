# Item POCO Property Name Mismatch Fix - UPDATED (NO ALIASES)

**Date:** 2025-01-15  
**Status:** ✅ UPDATED  
**Issue:** DataBinding exception - Property name mismatch

---

## Problem

Runtime error when loading Lookups.aspx Items tab:

```
System.Web.HttpException: DataBinding: 'TrackerDotNet.Classes.Poco.Item' does not contain a property with the name 'ServiceTypeId'.
at ASP.pages_lookups_aspx.__DataBinding__control153 in Lookups.aspx:line 117
```

---

## Root Cause

**ASPX Binding vs POCO Property Name Mismatch:**

| ASPX Binding Expression | Actual POCO Property | Status |
|------------------------|---------------------|---------|
| `<%# Bind("ServiceTypeId") %>` | `ItemServiceTypeID` | ❌ MISMATCH |
| `<%# Bind("Replacement") %>` | `ReplacementItemID` | ❌ MISMATCH |
| `<%# Bind("UoMID") %>` | `ItemUnitID` | ❌ MISMATCH |

**Why the mismatch:**
- The ASPX markup was using shortened/aliased property names
- The Item POCO was refactored to use full property names matching the database columns
- The ASPX markup was not updated to match

---

## Solution Implemented (DEPRECATED)

**Initial approach (deprecated): added alias properties to `Item` POCO for backward compatibility.**

```csharp
public class Item : ILookupEntity
{
    // Primary properties
    public int? ItemServiceTypeID { get; set; }
    public int? ReplacementItemID { get; set; }
    public int? ItemUnitID { get; set; }
    
    // ✅ ADDED: Alias properties for backward compatibility with ASPX binding
    public int? ServiceTypeId 
    { 
        get => ItemServiceTypeID; 
        set => ItemServiceTypeID = value; 
    }
    
    public int? Replacement 
    { 
        get => ReplacementItemID; 
        set => ReplacementItemID = value; 
    }
    
    public int? UoMID 
    { 
        get => ItemUnitID; 
        set => ItemUnitID = value; 
    }
}
```

---

## Why This Approach (DEPRECATED)

### **Option 1: Change ASPX** ❌ Not chosen
- Update all `<%# Bind("ServiceTypeId") %>` to `<%# Bind("ItemServiceTypeID") %>`
- Update all `<%# Bind("Replacement") %>` to `<%# Bind("ReplacementItemID") %>`
- Update all `<%# Bind("UoMID") %>` to `<%# Bind("ItemUnitID") %>`

**Cons:**
- More changes required
- Higher risk of missing instances
- More testing needed

### **Option 2: Add Alias Properties** ❌ NOT ALLOWED (Policy)
- Add get/set properties that map to the real properties
- ASPX binding works with both old and new names
- Code-behind already uses correct property names

**Pros:**
- ✅ Minimal changes (only Item.cs)
- ✅ Backward compatible
- ✅ No ASPX changes needed
- ✅ Code-behind already correct

---

## Affected ASPX Bindings

### **ServiceTypeId → ItemServiceTypeID**

**Locations in Lookups.aspx:**
```aspx
Line 100: <asp:TemplateField HeaderText="Type" SortExpression="ServiceTypeId">
Line 104:     SelectedValue='<%# Bind("ServiceTypeId") %>'   <!-- EditItemTemplate -->
Line 111:     SelectedValue='<%# Bind("ServiceTypeId") %>'   <!-- FooterTemplate -->
Line 118:     SelectedValue='<%# Bind("ServiceTypeId") %>'   <!-- ItemTemplate -->
```

**Also in EmptyDataTemplate (DetailsView):**
```aspx
Line 221: <asp:TemplateField HeaderText="ServiceType" SortExpression="ServiceTypeId">
Line 224:     SelectedValue='<%# Bind("ServiceTypeId") %>'   <!-- EditItemTemplate -->
Line 228:     SelectedValue='<%# Bind("ServiceTypeId") %>'   <!-- InsertItemTemplate -->
Line 232:     SelectedValue='<%# Bind("ServiceTypeId") %>'   <!-- ItemTemplate -->
```

---

### **Replacement → ReplacementItemID**

**Locations in Lookups.aspx:**
```aspx
Line 123: <asp:TemplateField HeaderText="Replcment" SortExpression="Replacement">
Line 127:     SelectedValue='<%# Bind("Replacement") %>'     <!-- EditItemTemplate -->
Line 134:     SelectedValue='<%# Bind("Replacement") %>'     <!-- FooterTemplate -->
Line 141:     SelectedValue='<%# Bind("Replacement") %>'     <!-- ItemTemplate -->
```

**Also in EmptyDataTemplate (DetailsView):**
```aspx
Line 237: <asp:TemplateField HeaderText="Replacement">
Line 241:     SelectedValue='<%# Bind("Replacement") %>'     <!-- EditItemTemplate -->
Line 246:     SelectedValue='<%# Bind("Replacement") %>'     <!-- InsertItemTemplate -->
Line 250:     SelectedValue='<%# Bind("Replacement") %>'     <!-- ItemTemplate -->
Line 254:     SelectedValue='<%# Bind("Replacement") %>'     <!-- FooterTemplate -->
```

---

### **UoMID → ItemUnitID**

**Locations in Lookups.aspx:**
```aspx
Line 168: <asp:TemplateField HeaderText="UoM" SortExpression="UoMID">
Line 172:     SelectedValue='<%# Bind("UoMID") %>'           <!-- EditItemTemplate -->
Line 179:     SelectedValue='<%# Bind("UoMID") %>'           <!-- FooterTemplate -->
Line 186:     SelectedValue='<%# Bind("UoMID") %>'           <!-- ItemTemplate -->
```

---

## Code-Behind Status

✅ **No changes needed in code-behind** - Already using correct property names:

```csharp
var newItem = new Item
{
    ItemServiceTypeID = Convert.ToInt32(control6.SelectedValue),  // ✅ Correct
    ReplacementItemID = Convert.ToInt32(control7.SelectedValue),  // ✅ Correct
    ItemUnitID = Convert.ToInt32(control11.SelectedValue)         // ✅ Correct
};
```

---

## Testing Checklist

After restarting the application:

- [ ] Load Lookups.aspx Items tab
- [ ] Verify grid displays without errors
- [ ] Test Edit mode (Edit button)
- [ ] Verify dropdowns populate:
  - [ ] Service Type dropdown
  - [ ] Replacement dropdown
  - [ ] Unit of Measure dropdown
- [ ] Test Add New Item (Footer row)
- [ ] Verify selected values display correctly
- [ ] Test Update existing item
- [ ] Verify data saves correctly

---

## Similar Patterns to Check

**Other POCOs that might have similar issues:**

1. **Area POCO** ✅ Already has aliases:
   - `ID` → `AreaID`
   - `Area` → `AreaName`

2. **Check these POCOs for similar issues:**
   - Person
   - ServiceType
   - PaymentTerm
   - PriceLevel
   - InvoiceType

---

---

## Updated Decision (Policy: NO ALIAS PROPERTIES)

Per `Documentation/NO_ALIAS_PROPERTIES_POLICY.md` (and repo standards docs), **POCO alias properties are not allowed** because they create ambiguity and drift from the canonical schema.

### What we do instead

1. **Update ASPX binding expressions** to use canonical POCO/database property names:
   - `ItemServiceTypeID`
   - `ReplacementItemID`
   - `ItemUnitID`
2. **Remove alias properties** from `Classes/Poco/Item.cs`.

### Status in workspace

- `Classes/Poco/Item.cs`: alias properties removed.
- `Pages/Lookups.aspx`: bindings already updated to canonical names in the Items tab.

---

## Summary

### **Changes Made:**

| File | Change |
|------|--------|
| `Classes\Poco\Item.cs` | ✅ Added 3 alias properties: `ServiceTypeId`, `Replacement`, `UoMID` |
| `Pages\Lookups.aspx` | ✅ No changes (aliases maintain compatibility) |
| `Pages\Lookups.aspx.cs` | ✅ No changes (already using correct names) |

### **Result:**

✅ **ASPX binding works** - Aliases allow old property names  
✅ **Code-behind works** - Uses full property names  
✅ **Repository works** - Uses full property names  
✅ **Backward compatible** - Both names work  

---

## Lesson Learned

**When refactoring POCOs:**

1. ✅ **Check ASPX binding expressions** - Not just code-behind
2. ✅ **Search for `<%# Bind("PropertyName") %>`** - Find all data binding
3. ✅ **Consider alias properties** - Easier than updating ASPX
4. ✅ **Test runtime binding** - Not just compile-time

**Future pattern:** When creating new POCOs from legacy code:
1. Keep alias properties if ASPX uses shortened names
2. Document which properties are aliases vs primary
3. Use primary property names in new code
4. Let aliases handle backward compatibility

---

**Property Name Mismatch Fixed!** 🎯

**Status:** ✅ READY TO TEST (Restart debugger)  
**Last Updated:** 2025-01-15  
**Version:** 1.0
