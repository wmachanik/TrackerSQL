# Repository Pattern - Standard Methods & Naming Conventions

**Date:** 2025-01-15  
**Status:** ACTIVE - MANDATORY  
**Version:** 1.0

---

## Purpose

This document establishes **mandatory naming conventions and standard methods** for all Repository classes to:

1. ? **Prevent code duplication** across repositories
2. ? **Ensure consistency** in lookup/dropdown formatting
3. ? **Provide predictable APIs** for all data access
4. ? **Enforce standard** "disabled item" formatting (`_ItemName`)

---

## Standard Repository Methods

### **All repositories MUST inherit from `RepositoryBase<T>`**

```csharp
public class ItemsRepository : RepositoryBase<Item>
{
    protected override string TableName => "ItemsTbl";
    protected override string KeyColumn => "ItemID";
    
    // Optional: Override for performance
    protected override string CoreColumns => "ItemID, ItemDesc, ItemEnabled";
    protected override string LookupColumns => "ItemID, ItemDesc, ItemEnabled";
}
```

---

## **MANDATORY Standard Methods**

Every repository inherits these methods from `RepositoryBase<T>`:

### **1. GetAll() - Get All Records**

```csharp
// Get all records, unsorted
List<Item> items = repo.GetAll();

// Get all records with sorting
List<Item> items = repo.GetAll("ItemDesc");
```

**Use When:**
- Loading grid data
- Getting full dataset
- Need all columns

**Standard Signature:**
```csharp
public virtual List<T> GetAll()
public virtual List<T> GetAll(string sortBy)
```

---

### **2. GetLookupList() - Get Formatted Dropdown List**

```csharp
// Get formatted list for dropdowns (disabled items prefixed with "_")
List<Item> items = repo.GetLookupList();
```

**Use When:**
- Populating dropdowns
- Building lookup lists
- Need ID + Display Text only
- Need disabled items marked

**Standard Behavior:**
- ? Returns only `LookupColumns` (ID + Display fields)
- ? **Disabled items prefixed with `"_"`** automatically
- ? Sorts enabled first, then disabled
- ? Both groups alphabetically sorted

**Example Output:**
```
ID  Display Text
--  ------------
1   Bread
2   Coffee
3   Milk
4   _Old Product (Disabled)
5   _Legacy Item (Disabled)
```

**Standard Signature:**
```csharp
public virtual List<T> GetLookupList()
```

---

### **3. GetAllEnabled() - Get Only Active/Enabled Records**

```csharp
// Get only enabled records
List<Item> activeItems = repo.GetAllEnabled("ItemDesc");
```

**Use When:**
- Need only active items
- Filtering out disabled records
- Building active-only dropdowns

**Standard Signature:**
```csharp
public virtual List<T> GetAllEnabled(string sortBy = null)
```

---

### **4. GetById() - Get Single Record by ID**

```csharp
// Get full record by ID
Item item = repo.GetById(5);
```

**Use When:**
- Loading single record
- Need all columns for one entity

**Standard Signature:**
```csharp
public virtual T GetById(int id)
```

---

### **5. GetKeyColsById() - Get Minimal Columns by ID**

```csharp
// Get only key/core columns (performance optimization)
Item item = repo.GetKeyColsById(5);
```

**Use When:**
- Only need ID + name fields
- Performance optimization
- Lookup scenarios

**Uses:**
- `CoreColumns` property if overridden
- Falls back to `*` if not specified

**Standard Signature:**
```csharp
public virtual T GetKeyColsById(int id)
```

---

## **ILookupEntity Interface**

### **Purpose**

Enables standard formatting for entities used in dropdowns.

### **Implementation Required For:**

? **MUST implement** if entity:
- Has an "Enabled" or "Active" field
- Used in dropdowns/lookups
- Needs disabled items marked

? **Not needed** if entity:
- No enable/disable concept
- Never used in dropdowns
- Simple reference tables

### **Interface Definition**

```csharp
public interface ILookupEntity
{
    int GetId();              // Returns the primary key
    string GetDisplayText();  // Returns the text to display
    bool? IsEnabled();        // Returns enabled state (null = no concept of enabled)
}
```

### **Example Implementation**

```csharp
public class Item : ILookupEntity
{
    public int ItemID { get; set; }
    public string ItemDesc { get; set; }
    public bool? ItemEnabled { get; set; }

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

## **Standard Disabled Item Formatting**

### **The "_" Prefix Rule**

**MANDATORY:** All disabled items in lookups MUST be prefixed with `"_"` (underscore).

**Why:**
1. ? **Consistent visual indicator** - Users see disabled items clearly
2. ? **Sorts to bottom** - Underscore sorts after letters in most collations
3. ? **Standard across app** - Same formatting everywhere

**Example:**
```
Active Items:
- Bread
- Coffee
- Milk

Disabled Items:
- _Old Product
- _Retired Item
```

### **LookupFormatter.FormatLookupText()**

**Central method for all formatting:**

```csharp
public static class LookupFormatter
{
    public static string FormatLookupText(string text, bool? isEnabled)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Prefix with "_" if disabled
        if (isEnabled.HasValue && !isEnabled.Value)
            return "_" + text;

        return text;
    }
}
```

**Use this method EVERYWHERE disabled formatting is needed!**

---

## **Naming Conventions**

### **Repository Class Names**

**Pattern:** `{EntityName}Repository`

? **Correct:**
- `ItemsRepository`
- `PersonsRepository`
- `ServiceTypesRepository`

? **Incorrect:**
- `ItemRepo` (missing "Repository")
- `ItemManager` (not "Repository")
- `ItemsTbl` (that's the legacy table class)

---

### **Repository Method Names**

**Standard Names (inherited from base - DO NOT override unless necessary):**

| Method | Purpose | When to Override |
|--------|---------|-----------------|
| `GetAll()` | Get all records | Never (base implementation sufficient) |
| `GetAll(string sortBy)` | Get all sorted | Never |
| `GetLookupList()` | Get formatted dropdown list | Only if custom formatting needed |
| `GetAllEnabled()` | Get only enabled | Only if "Enabled" column has different name |
| `GetById()` | Get single by ID | Rarely (base is sufficient) |

**Custom Method Naming:**

If you need repository-specific methods:

? **Good Patterns:**
```csharp
GetByCustomerId(int customerId)
GetActiveByDate(DateTime date)
GetSummaryData()
Search(string searchTerm)
```

? **Bad Patterns:**
```csharp
Get_Customer_Items()  // No underscores
getAllItemsForCustomer()  // Wrong casing
FetchItems()  // Use "Get" not "Fetch"
```

---

### **Column Name Standards**

**Override in derived repository:**

```csharp
protected override string TableName => "ItemsTbl";
protected override string KeyColumn => "ItemID";

// Optional performance optimization
protected override string CoreColumns => "ItemID, ItemDesc, ItemEnabled";
protected override string LookupColumns => "ItemID, ItemDesc, ItemEnabled";
```

**CoreColumns:**
- Minimal columns needed for key lookups
- Used by `GetKeyColsById()`
- Performance optimization

**LookupColumns:**
- ID + Display fields only
- Used by `GetLookupValues()` and `GetLookupList()`
- Should include enabled/disabled column if applicable

---

## **Code Examples**

### **Example 1: Using GetLookupList() in Code-Behind**

```csharp
protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
{
    if (e.Row.RowType == DataControlRowType.DataRow || 
        e.Row.RowType == DataControlRowType.Footer)
    {
        var ddl = e.Row.FindControl("ddlServiceType") as DropDownList;
        if (ddl != null)
        {
            var repo = new ServiceTypesRepository();
            
            // ? STANDARD WAY - Gets formatted list with disabled marked
            var serviceTypes = repo.GetLookupList();
            
            ddl.DataSource = serviceTypes;
            ddl.DataTextField = "FormattedDisplayText";  // Uses "_Disabled Item" formatting
            ddl.DataValueField = "ServiceTypeId";
            ddl.DataBind();
        }
    }
}
```

### **Example 2: Using GetAllEnabled() for Active-Only Dropdown**

```csharp
protected void LoadActiveItemsOnly()
{
    var repo = new ItemsRepository();
    
    // ? Get only enabled items
    var activeItems = repo.GetAllEnabled("ItemDesc");
    
    ddlActiveItems.DataSource = activeItems;
    ddlActiveItems.DataTextField = "ItemDesc";  // No formatting needed (all enabled)
    ddlActiveItems.DataValueField = "ItemID";
    ddlActiveItems.DataBind();
}
```

### **Example 3: Custom Repository Method**

```csharp
public class ItemsRepository : RepositoryBase<Item>
{
    protected override string TableName => "ItemsTbl";
    protected override string KeyColumn => "ItemID";
    protected override string LookupColumns => "ItemID, ItemDesc, ItemEnabled";

    // ? Custom method - follows naming convention
    public List<Item> GetByServiceType(int serviceTypeId, string sortBy = null)
    {
        var list = new List<Item>();
        string sql = $"SELECT * FROM {TableName} WHERE ItemServiceTypeID = @ServiceTypeId";
        
        if (!string.IsNullOrWhiteSpace(sortBy))
            sql += " ORDER BY " + sortBy;
            
        var param = new List<DBParameter> 
        { 
            new DBParameter 
            { 
                ParamName = "@ServiceTypeId", 
                DataValue = serviceTypeId, 
                DataDbType = DbType.Int32 
            } 
        };
        
        using (var db = new TrackerSQLDb())
        using (var rdr = db.ExecuteReader(sql, param))
        {
            while (rdr.Read()) 
                list.Add(DbMapper.Map<Item>(rdr));
        }
        
        return list;
    }
}
```

---

## **Checklist for New Repositories**

When creating a new repository, ensure:

- [ ] ? Inherits from `RepositoryBase<T>`
- [ ] ? Named `{Entity}Repository`
- [ ] ? Overrides `TableName` property
- [ ] ? Overrides `KeyColumn` property
- [ ] ? Overrides `LookupColumns` if entity used in dropdowns
- [ ] ? Overrides `CoreColumns` if performance optimization needed
- [ ] ? POCO implements `ILookupEntity` if has enabled/disabled state
- [ ] ? POCO has `FormattedDisplayText` property if used in dropdowns
- [ ] ? Standard methods (`GetAll`, `GetById`) NOT overridden unless necessary
- [ ] ? Custom methods follow `GetByX()` naming pattern

---

## **Benefits of This Standard**

? **Consistency:**
- Same method names across all repositories
- Predictable API for all developers

? **No Duplication:**
- Lookup formatting logic in ONE place (`LookupFormatter`)
- Disabled item prefix ("_") enforced globally

? **Maintainability:**
- Change formatting rules in one place
- All repositories inherit improvements

? **Discoverability:**
- IntelliSense shows standard methods
- New developers know what's available

? **Type Safety:**
- Compile-time checking
- No magic strings

---

## **Violation Examples (DO NOT DO THIS)**

? **Duplicating Lookup Logic:**
```csharp
// BAD - Duplicates formatting in every repository
public List<Item> GetLookupList()
{
    var items = GetAll();
    foreach (var item in items)
    {
        if (!item.ItemEnabled)
            item.ItemDesc = "_" + item.ItemDesc;  // ? WRONG!
    }
    return items;
}
```

? **Use Standard Method:**
```csharp
// GOOD - Uses inherited method
var items = repo.GetLookupList();  // ? Formatting handled by base class
```

---

? **Non-Standard Method Names:**
```csharp
// BAD
public List<Item> FetchAllActiveItems() { }  // ? Use "Get" not "Fetch"
public List<Item> getAllItems() { }          // ? Wrong casing
public List<Item> Get_Items() { }            // ? No underscores
```

? **Standard Names:**
```csharp
// GOOD
public List<Item> GetAllEnabled() { }        // ? Standard name
public List<Item> GetByCustomerId(int id) { }  // ? Descriptive, follows pattern
```

---

## **Summary**

| Concept | Standard | Example |
|---------|----------|---------|
| **Repository Name** | `{Entity}Repository` | `ItemsRepository` |
| **Base Class** | `RepositoryBase<T>` | `public class ItemsRepository : RepositoryBase<Item>` |
| **Get All** | `GetAll(sortBy)` | `repo.GetAll("ItemDesc")` |
| **Get Dropdown List** | `GetLookupList()` | `repo.GetLookupList()` |
| **Get Enabled Only** | `GetAllEnabled(sortBy)` | `repo.GetAllEnabled("ItemDesc")` |
| **Get By ID** | `GetById(id)` | `repo.GetById(5)` |
| **Disabled Prefix** | `"_"` before name | `"_Old Product"` |
| **Lookup Interface** | `ILookupEntity` | `public class Item : ILookupEntity` |
| **Format Helper** | `LookupFormatter` | `LookupFormatter.FormatLookupText(text, enabled)` |

---

**THESE STANDARDS ARE MANDATORY. NO EXCEPTIONS.**

**Last Updated:** 2025-01-15  
**Version:** 1.0  
**Status:** ACTIVE
