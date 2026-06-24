# Repository Standards Implementation - COMPLETE

**Date:** 2025-01-15  
**Status:** ? COMPLETE  
**Version:** 1.0

---

## What Was Done

### **Problem Identified**

User correctly identified potential code duplication issues:

1. ? **Risk:** Multiple repositories might implement GetAll/GetLookupList differently
2. ? **Risk:** Disabled item formatting ("_prefix") could be inconsistent
3. ? **Risk:** No naming standards for repository methods
4. ? **Risk:** Developers might not know which standard methods exist

### **Solution Implemented**

? **Created Standard Interface:** `ILookupEntity`
? **Created Formatter Class:** `LookupFormatter`  
? **Enhanced RepositoryBase:** Added `GetLookupList()` and `GetAllEnabled()`  
? **Updated Item POCO:** Implements `ILookupEntity`  
? **Documented Standards:** Created `REPOSITORY_STANDARDS.md`  
? **Updated Rules:** Referenced new standards in `HARD_PROJECT_RULES.md`

---

## Files Created/Modified

### **1. Created: `Classes/Poco/ILookupEntity.cs`**

**Purpose:** Standard interface for entities used in lookups/dropdowns

```csharp
public interface ILookupEntity
{
    int GetId();              // Returns primary key
    string GetDisplayText();  // Returns display text
    bool? IsEnabled();        // Returns enabled state (null = N/A)
}
```

**Also includes `LookupFormatter` helper class:**

```csharp
public static class LookupFormatter
{
    public static string FormatLookupText(string text, bool? isEnabled)
    {
        // Prefixes disabled items with "_" automatically
    }
}
```

---

### **2. Modified: `Classes/Sql/RepositoryBase.cs`**

**Added standard methods:**

1. **`GetLookupList()`**
   - Gets formatted dropdown list
   - Disabled items prefixed with "_"
   - Sorts enabled first, then disabled
   - Both groups alphabetically sorted

2. **`GetAllEnabled(string sortBy)`**
   - Gets only enabled/active records
   - Filters WHERE Enabled = 1
   - Optional sorting

**Enhanced:**
- Added `using System.Linq;` for LINQ support
- Added `using TrackerDotNet.Classes.Poco;` for `ILookupEntity`

---

### **3. Modified: `Classes/Poco/Item.cs`**

**Implemented `ILookupEntity` interface:**

```csharp
public class Item : ILookupEntity
{
    // Existing properties...
    
    // ILookupEntity implementation
    public int GetId() => ItemID;
    public string GetDisplayText() => ItemDesc ?? string.Empty;
    public bool? IsEnabled() => ItemEnabled;

    // Convenience property for formatted display
    public string FormattedDisplayText => 
        LookupFormatter.FormatLookupText(ItemDesc, ItemEnabled);
}
```

---

### **4. Created: `Documentation/REPOSITORY_STANDARDS.md`**

**Comprehensive documentation including:**

- ? Standard method names (GetAll, GetLookupList, GetAllEnabled, GetById)
- ? Naming conventions for repositories and methods
- ? ILookupEntity interface usage
- ? Disabled item formatting rules ("_" prefix)
- ? Code examples for all patterns
- ? DO/DON'T violation examples
- ? Checklist for new repositories

**Status:** MANDATORY documentation - all developers must read

---

### **5. Modified: `Documentation/HARD_PROJECT_RULES.md`**

**Added reference to new standards:**

```markdown
## Where These Rules Are Documented

1. This File: HARD_PROJECT_RULES.md (you are here)
2. Repository Standards: REPOSITORY_STANDARDS.md (naming conventions & standard methods)
3. Architecture: ARCHITECTURE_RULES.md
4. Project Overview: PROJECT_OVERVIEW.md
5. Implementation Guide: REPOSITORY_ENFORCEMENT_PHASE1_COMPLETE.md
```

---

## Standard Methods Available

### **In RepositoryBase<T>** (all repositories inherit these):

| Method | Purpose | Example |
|--------|---------|---------|
| `GetAll()` | Get all records | `repo.GetAll()` |
| `GetAll(string sortBy)` | Get all sorted | `repo.GetAll("ItemDesc")` |
| `GetLookupList()` | **NEW:** Get formatted dropdown list | `repo.GetLookupList()` |
| `GetAllEnabled(sortBy)` | **NEW:** Get only enabled records | `repo.GetAllEnabled("ItemDesc")` |
| `GetById(int id)` | Get single by ID | `repo.GetById(5)` |
| `GetKeyColsById(int id)` | Get minimal columns by ID | `repo.GetKeyColsById(5)` |
| `GetLookupValues(sortBy)` | Get lookup values (no formatting) | `repo.GetLookupValues("ItemDesc")` |

---

## Disabled Item Formatting Standard

### **The Rule**

? **ALL disabled items in lookups MUST be prefixed with `"_"` (underscore)**

### **Why**

1. ? Consistent visual indicator
2. ? Sorts to bottom (underscore sorts after letters)
3. ? Standard across entire app

### **Example Output**

```
Dropdown List:
--------------
Bread
Coffee
Milk
_Discontinued Product
_Legacy Item
```

### **How It Works**

**Automatic formatting via `GetLookupList()`:**

```csharp
// In code-behind
var repo = new ItemsRepository();
var items = repo.GetLookupList();  // ? Disabled items auto-prefixed with "_"

ddl.DataSource = items;
ddl.DataTextField = "FormattedDisplayText";  // Uses formatted text
ddl.DataValueField = "ItemID";
ddl.DataBind();
```

---

## Benefits

### **1. No Code Duplication**

? Lookup formatting logic in **ONE place** (`LookupFormatter`)  
? Disabled item prefix enforced **globally**  
? Change format rules in **one place** - all repos inherit

### **2. Consistency**

? Same method names across **all repositories**  
? Predictable API for **all developers**  
? Same disabled item formatting **everywhere**

### **3. Maintainability**

? Change formatting rules in one place  
? All repositories inherit improvements automatically  
? No hunting for duplicate code

### **4. Discoverability**

? IntelliSense shows standard methods  
? New developers know what's available  
? Clear naming conventions

### **5. Type Safety**

? Compile-time checking  
? No magic strings  
? Interface enforcement

---

## Next Steps for Other POCOs

### **Entities That SHOULD Implement ILookupEntity:**

? **Person** - Has `Enabled` field  
? **ServiceType** - Has `Enabled` field  
? **PaymentTerm** - Has `Enabled` field  
? **PriceLevel** - Has `Enabled` field  
? **InvoiceType** - Has `Enabled` field  
? **RepairStatus** - Has `Enabled` field

### **Entities That DON'T NEED ILookupEntity:**

? **ItemUnit** - No enabled/disabled concept  
? **EquipType** - No enabled/disabled concept (currently)  
? Simple lookup tables with no enable/disable

---

## Code Example: Using New Standards

### **Before (Old Way - Duplicated Logic):**

```csharp
// ? BAD - Formatting logic duplicated in every page
protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
{
    var ddl = e.Row.FindControl("ddlReplacement") as DropDownList;
    if (ddl != null)
    {
        var repo = new ItemsRepository();
        var items = repo.GetAll("ItemDesc");
        
        // ? Duplicated formatting logic!
        foreach (var item in items)
        {
            if (!item.ItemEnabled.HasValue || !item.ItemEnabled.Value)
                item.ItemDesc = "_" + item.ItemDesc;
        }
        
        ddl.DataSource = items;
        ddl.DataTextField = "ItemDesc";
        ddl.DataValueField = "ItemID";
        ddl.DataBind();
    }
}
```

### **After (New Way - Uses Standard Method):**

```csharp
// ? GOOD - Uses standard method, no duplication
protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
{
    var ddl = e.Row.FindControl("ddlReplacement") as DropDownList;
    if (ddl != null)
    {
        var repo = new ItemsRepository();
        
        // ? ONE line - formatting handled by base class!
        var items = repo.GetLookupList();
        
        ddl.DataSource = items;
        ddl.DataTextField = "FormattedDisplayText";  // Uses formatted text
        ddl.DataValueField = "ItemID";
        ddl.DataBind();
    }
}
```

---

## Checklist for Developers

When creating a new repository:

- [ ] ? Inherits from `RepositoryBase<T>`
- [ ] ? Named `{Entity}Repository`
- [ ] ? Overrides `TableName` property
- [ ] ? Overrides `KeyColumn` property
- [ ] ? Overrides `LookupColumns` if entity used in dropdowns
- [ ] ? POCO implements `ILookupEntity` if has enabled/disabled state
- [ ] ? POCO has `FormattedDisplayText` property if used in dropdowns
- [ ] ? Uses standard methods (don't override unless necessary)

---

## Build Status

? **Build:** Successful  
? **Standards:** Documented  
? **Implementation:** Complete  
? **Examples:** Provided

---

## Summary

### **What We Achieved**

1. ? **Standard interface** for lookup entities (`ILookupEntity`)
2. ? **Central formatter** for disabled items (`LookupFormatter`)
3. ? **Enhanced base class** with `GetLookupList()` and `GetAllEnabled()`
4. ? **Comprehensive documentation** of all standards
5. ? **Example implementation** (Item POCO)
6. ? **Updated hard rules** to reference new standards

### **Benefits Delivered**

- ? **No code duplication** - Format logic in ONE place
- ? **Consistent formatting** - "_" prefix enforced globally
- ? **Standard methods** - Same API across all repositories
- ? **Clear documentation** - Developers know what to do
- ? **Type safety** - Interface enforcement

---

## Documentation Location

**Primary:** `Documentation/REPOSITORY_STANDARDS.md`  
**Reference:** `Documentation/HARD_PROJECT_RULES.md` (updated with link)

---

**STANDARDS ARE NOW MANDATORY AND ENFORCED!** ??

**Last Updated:** 2025-01-15  
**Status:** COMPLETE  
**Version:** 1.0
