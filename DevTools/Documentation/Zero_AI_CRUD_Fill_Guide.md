# Zero-AI CRUD Fill-In Guide

**Purpose:** Step-by-step guide to fill in repository CRUD methods by copy-paste  
**Time:** 10-15 minutes per repository  
**AI Calls:** 0

---

## The Process (3 Steps Per Repository)

### Step 1: Open Both Files (30 seconds)

```
Legacy:  Controls\CustomersTbl.cs
New:     Classes\Sql\ContactsRepository.cs
```

**Side-by-side in Visual Studio:**
- Right-click legacy file ? "New Vertical Tab Group"
- Open repo file in other tab group

### Step 2: Fill INSERT Method (5 minutes)

#### In Legacy Class, Find:

```csharp
public string Insert...(...) 
{
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddParams((object)customer.CompanyName, DbType.String);
    trackerDb.AddParams((object)customer.ContactFirstName, DbType.String);
    trackerDb.AddParams((object)customer.AreaID, DbType.Int32);
    // ... more AddParams

    string sql = "INSERT INTO CustomersTbl (CompanyName, ContactFirstName, AreaID, ...) VALUES (?, ?, ?, ...)";
    string result = trackerDb.ExecuteNonQuerySQL(sql);
    trackerDb.Close();
    return result;
}
```

#### Copy to Repo Template:

```csharp
public int Insert(Contact entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));

    // 1. COPY SQL, change table name, change ? to @ParamName
    string sql = @"
        INSERT INTO ContactsTbl (
            CompanyName, 
            ContactFirstName, 
            AreaID
            -- add rest of columns
        )
        VALUES (
            @CompanyName, 
            @ContactFirstName, 
            @AreaID
            -- add rest of @params
        );
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    // 2. CONVERT AddParams to DBParameter list
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" },
        new DBParameter { DataValue = entity.ContactFirstName, DataDbType = DbType.String, ParamName = "@ContactFirstName" },
        new DBParameter { DataValue = entity.AreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" },
        // add rest from AddParams calls
    };

    return ExecuteScalar<int>(sql, parameters);
}
```

**Pattern:**
- Each `trackerDb.AddParams((object)X, DbType.Y)` becomes:
- `new DBParameter { DataValue = entity.X, DataDbType = DbType.Y, ParamName = "@X" }`

### Step 3: Fill UPDATE Method (5 minutes)

Same process as INSERT:

```csharp
// LEGACY
trackerDb.AddParams((object)customer.CompanyName, DbType.String);
// ... all other fields
trackerDb.AddParams((object)customer.CustomerID, DbType.Int32); // ID is LAST
string sql = "UPDATE CustomersTbl SET CompanyName = ?, ... WHERE CustomerID = ?";

// NEW
string sql = @"
    UPDATE ContactsTbl 
    SET CompanyName = @CompanyName,
        -- all other fields
    WHERE ContactID = @ContactID";

var parameters = new List<DBParameter>
{
    new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" },
    // all other fields
    new DBParameter { DataValue = entity.ContactID, DataDbType = DbType.Int32, ParamName = "@ContactID" }
};

int result = ExecuteNonQuery(sql, parameters);
return result > 0;
```

**DELETE is already done in template!** ?

---

## Quick Reference: Conversion Patterns

### Pattern 1: Simple Value

```csharp
// LEGACY
trackerDb.AddParams((object)customer.CompanyName, DbType.String);

// NEW
new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" }
```

### Pattern 2: Nullable Value

```csharp
// LEGACY
trackerDb.AddParams((object)customer.Notes ?? DBNull.Value, DbType.String);

// NEW
new DBParameter { DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String, ParamName = "@Notes" }
```

### Pattern 3: DateTime

```csharp
// LEGACY
trackerDb.AddParams((object)customer.CreatedDate, DbType.DateTime);

// NEW
new DBParameter { DataValue = entity.CreatedDate, DataDbType = DbType.DateTime, ParamName = "@CreatedDate" }
```

### Pattern 4: Boolean

```csharp
// LEGACY
trackerDb.AddParams((object)customer.IsActive, DbType.Boolean);

// NEW
new DBParameter { DataValue = entity.IsActive, DataDbType = DbType.Boolean, ParamName = "@IsActive" }
```

### Pattern 5: Decimal/Money

```csharp
// LEGACY
trackerDb.AddParams((object)customer.CreditLimit, DbType.Decimal);

// NEW
new DBParameter { DataValue = entity.CreditLimit, DataDbType = DbType.Decimal, ParamName = "@CreditLimit" }
```

---

## Common Mistakes to Avoid

### ? Mistake 1: Wrong Parameter Order

```csharp
// WRONG - params don't match SQL order
VALUES (@CompanyName, @AreaID)
parameters: [AreaID, CompanyName]  // Wrong order!

// RIGHT
VALUES (@CompanyName, @AreaID)
parameters: [CompanyName, AreaID]  // Matches!
```

### ? Mistake 2: Forgot to Rename Table

```csharp
// WRONG
INSERT INTO CustomersTbl  // Old name!

// RIGHT
INSERT INTO ContactsTbl   // New name!
```

### ? Mistake 3: Forgot @ParamName

```csharp
// WRONG
VALUES (?, ?, ?)  // Still using ?

// RIGHT
VALUES (@CompanyName, @AreaID, @IsActive)
```

---

## Time-Saving Tips

### Tip 1: Use Find/Replace

In legacy SQL:
1. Copy SQL string
2. Find: `?` 
3. Replace with parameter name (do manually, one by one)

### Tip 2: Line Up AddParams

```csharp
// In legacy class, make a list:
AddParams: CompanyName, String
AddParams: ContactFirstName, String  
AddParams: AreaID, Int32
// etc.

// Then convert each line to DBParameter
```

### Tip 3: Check Count

```csharp
// Count ? in SQL: 5
// Count AddParams: 5
// Count DBParameter: should be 5
// If mismatch = ERROR!
```

---

## Complete Example: ContactsRepository.Insert

### Legacy (CustomersTbl.cs)

```csharp
public string InsertCustomer(CustomersTbl customer)
{
    TrackerDb db = new TrackerDb();
    db.AddParams((object)customer.CompanyName, DbType.String);
    db.AddParams((object)customer.ContactFirstName, DbType.String);
    db.AddParams((object)customer.ContactLastName, DbType.String);
    db.AddParams((object)customer.AreaID, DbType.Int32);
    db.AddParams((object)customer.IsActive, DbType.Boolean);

    string sql = "INSERT INTO CustomersTbl (CompanyName, ContactFirstName, ContactLastName, AreaID, IsActive) VALUES (?, ?, ?, ?, ?)";
    string result = db.ExecuteNonQuerySQL(sql);
    db.Close();
    return result;
}
```

### New (ContactsRepository.cs)

```csharp
public int Insert(Contact entity)
{
    if (entity == null) throw new ArgumentNullException(nameof(entity));

    string sql = @"
        INSERT INTO ContactsTbl (
            CompanyName, 
            ContactFirstName, 
            ContactLastName, 
            AreaID, 
            IsActive
        )
        VALUES (
            @CompanyName, 
            @ContactFirstName, 
            @ContactLastName, 
            @AreaID, 
            @IsActive
        );
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = entity.CompanyName, DataDbType = DbType.String, ParamName = "@CompanyName" },
        new DBParameter { DataValue = entity.ContactFirstName, DataDbType = DbType.String, ParamName = "@ContactFirstName" },
        new DBParameter { DataValue = entity.ContactLastName, DataDbType = DbType.String, ParamName = "@ContactLastName" },
        new DBParameter { DataValue = entity.AreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" },
        new DBParameter { DataValue = entity.IsActive, DataDbType = DbType.Boolean, ParamName = "@IsActive" }
    };

    return ExecuteScalar<int>(sql, parameters);
}
```

**Time:** 5 minutes  
**AI Calls:** 0

---

## Checklist Per Repository

- [ ] Open legacy class (Controls\*Tbl.cs)
- [ ] Open new repo (Classes\Sql\*Repository.cs)
- [ ] Find INSERT method in legacy
- [ ] Copy SQL to repo template
- [ ] Rename table name (Customer ? Contact)
- [ ] Replace ? with @ParamName
- [ ] Convert AddParams to DBParameter list
- [ ] Verify parameter count matches
- [ ] Find UPDATE method in legacy
- [ ] Repeat same process
- [ ] DELETE already done
- [ ] Build and verify (dotnet build)

**Repeat for next repository!**

---

## Next: Custom Methods

After CRUD is done, see:
- `DevTools\Documentation\Minimal_AI_Repository_Migration_Plan.md` (Phase 4)
- Simple custom methods: copy-paste (0 AI)
- Complex custom methods: use AI (1-2 calls each)
