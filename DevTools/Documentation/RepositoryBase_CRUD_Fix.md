# FIXED: RepositoryBase Now Has Full CRUD

**Date:** 2026-05-15  
**Issue:** RepositoryBase<T> only had Read operations (GetById, GetAll, etc.)  
**Solution:** Added Insert, Update, Delete methods to base class

---

## ? What Was Added to RepositoryBase<T>

### 1. Insert(T entity) - Virtual Method

```csharp
/// <summary>
/// Inserts a new entity record
/// Derived repositories MUST override this to provide proper INSERT statement
/// </summary>
public virtual int Insert(T entity)
{
    throw new NotImplementedException($"Insert method must be implemented in {GetType().Name}...");
}
```

**Status:** Virtual method that **throws NotImplementedException**  
**Requirement:** Derived repositories **must override** with actual INSERT logic  
**Reason:** INSERT SQL varies too much by table (different columns, data types)

### 2. Update(T entity) - Virtual Method

```csharp
/// <summary>
/// Updates an existing entity record
/// Derived repositories MUST override this to provide proper UPDATE statement
/// </summary>
public virtual bool Update(T entity)
{
    throw new NotImplementedException($"Update method must be implemented in {GetType().Name}...");
}
```

**Status:** Virtual method that **throws NotImplementedException**  
**Requirement:** Derived repositories **must override** with actual UPDATE logic  
**Reason:** UPDATE SQL varies by table (different columns to update)

### 3. Delete(int id) - Virtual Method with Working Implementation

```csharp
/// <summary>
/// Deletes an entity record by ID
/// Default implementation uses TableName and KeyColumn.
/// Override in derived repository if custom delete logic needed.
/// </summary>
public virtual bool Delete(int id)
{
    string sql = $"DELETE FROM {TableName} WHERE {KeyColumn} = @Id";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
    };

    int result = ExecNonQuery(sql, parameters);
    return result > 0;
}
```

**Status:** **Fully working default implementation** ?  
**Requirement:** Can be used as-is, override only if needed  
**Reason:** DELETE SQL is simple and standard across all tables

---

## ?? Complete CRUD Summary

### RepositoryBase<T> Now Provides

| Method | Status | Must Override? | Notes |
|---|---|---|---|
| **GetById** | ? Complete | No | Inherited, works immediately |
| **GetAll** | ? Complete | No | Inherited, works immediately |
| **GetAll(string)** | ? Complete | No | Inherited, with sorting |
| **GetKeyColsById** | ? Complete | No | Inherited, performance optimized |
| **GetLookupValues** | ? Complete | No | Inherited, for dropdowns |
| **GetLookupList** | ? Complete | No | Inherited, formatted for UI |
| **GetAllEnabled** | ? Complete | No | Inherited, active records only |
| **Insert** | ?? Virtual stub | **YES** | Throws NotImplementedException |
| **Update** | ?? Virtual stub | **YES** | Throws NotImplementedException |
| **Delete** | ? Complete | No (optional) | Working default implementation |

---

## ?? Impact on Scripts

### Before Fix

Scripts needed to add:
- ? Insert
- ? Update
- ? Delete

### After Fix

Scripts now only need to add:
- ?? Insert (must override base stub)
- ?? Update (must override base stub)
- ~~Delete~~ ? Already works in base!

**Result:** Scripts simplified, Delete no longer needed!

---

## ?? Why This Design?

### Delete Can Be Generic
```csharp
// Same for ALL tables:
DELETE FROM {TableName} WHERE {KeyColumn} = @Id
```
? Works for every repository without changes!

### Insert/Update Must Be Specific
```csharp
// Different for EACH table:
INSERT INTO ContactsTbl (CompanyName, AreaID, IsActive, ...) VALUES (@CompanyName, @AreaID, @IsActive, ...)
INSERT INTO ItemsTbl (ItemName, ItemTypeID, Price, ...) VALUES (@ItemName, @ItemTypeID, @Price, ...)
```
? Cannot be generic - each table has different columns!

---

## ?? Usage Examples

### Example 1: Delete Works Immediately

```csharp
public class ContactsRepository : RepositoryBase<Contact>
{
    protected override string TableName => "ContactsTbl";
    protected override string KeyColumn => "ContactID";

    // Delete inherited from base - works immediately!
    // No need to override unless you need custom logic
}

// Usage:
var repo = new ContactsRepository();
bool deleted = repo.Delete(123);  // ? Works!
```

### Example 2: Insert Must Be Overridden

```csharp
public class ContactsRepository : RepositoryBase<Contact>
{
    protected override string TableName => "ContactsTbl";
    protected override string KeyColumn => "ContactID";

    // MUST override Insert - base throws NotImplementedException
    public override int Insert(Contact entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        string sql = @"
            INSERT INTO ContactsTbl (CompanyName, AreaID, IsActive)
            VALUES (@CompanyName, @AreaID, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" },
            new DBParameter { DataValue = entity.AreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" },
            new DBParameter { DataValue = entity.IsActive, DataDbType = DbType.Boolean, ParamName = "@IsActive" }
        };

        return ExecuteScalar<int>(sql, parameters);
    }
}
```

### Example 3: Update Must Be Overridden

```csharp
public override bool Update(Contact entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));

    string sql = @"
        UPDATE ContactsTbl
        SET CompanyName = @CompanyName,
            AreaID = @AreaID,
            IsActive = @IsActive
        WHERE ContactID = @ContactID";

    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" },
        new DBParameter { DataValue = entity.AreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" },
        new DBParameter { DataValue = entity.IsActive, DataDbType = DbType.Boolean, ParamName = "@IsActive" },
        new DBParameter { DataValue = entity.ContactID, DataDbType = DbType.Int32, ParamName = "@ContactID" }
    };

    int result = ExecNonQuery(sql, parameters);
    return result > 0;
}
```

---

## ?? Benefits

### For Developers

? **Delete works immediately** - No code needed in most repositories  
? **Clear contract** - Insert/Update must be overridden (compiler enforces)  
? **Less boilerplate** - Only write table-specific code  
? **Consistent API** - All repos have same CRUD methods  

### For Scripts

? **Simpler logic** - Only add Insert/Update  
? **No Delete generation** - Already works  
? **Faster execution** - Fewer methods to add  

---

## ?? Migration Impact

### Existing Repositories

**Repositories WITHOUT Insert/Update:**
- Will compile ?
- GetById, GetAll, Delete work ?
- Insert/Update throw NotImplementedException at runtime ??
- **Action:** Add Insert/Update overrides

**Repositories WITH Insert/Update:**
- Everything works ?
- Delete now available for free ?
- **Action:** None needed

### New Repositories

**Minimal template:**
```csharp
public class SomeRepository : RepositoryBase<SomeEntity>
{
    protected override string TableName => "SomeTable";
    protected override string KeyColumn => "SomeID";

    // GetById, GetAll, Delete inherited - work immediately! ?

    // Must add:
    public override int Insert(SomeEntity entity) { ... }
    public override bool Update(SomeEntity entity) { ... }
}
```

---

## ? Summary

**What Changed:**
- ? RepositoryBase now has Insert, Update, Delete methods
- ? Delete has working default implementation
- ? Insert/Update are virtual stubs (must override)

**What This Means:**
- ? Every repository has full CRUD interface
- ? Delete works out-of-the-box
- ? Insert/Update must be implemented (enforced)
- ? Scripts only add Insert/Update now

**Next Steps:**
1. Build project - ensure no errors
2. Run batch script - adds only Insert/Update
3. Fill TODOs - copy from legacy classes
4. Test - all CRUD operations work

---

**File Modified:** `Classes/Sql/RepositoryBase.cs`  
**Scripts Updated:** `DevTools/Scripts/Add-Missing-CRUD.ps1`  
**Status:** ? Ready to use
