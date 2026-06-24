# TrackerDb to TrackerSQLDb Quick Reference Guide

**Purpose:** Fast lookup for common migration patterns  
**Created:** 2025-05-14

---

## Quick Comparison

| Aspect | OLD (TrackerDb) | NEW (TrackerSQLDb) |
|--------|-----------------|-------------------|
| **Class** | `TrackerDb` | `TrackerSQLDb` |
| **Using** | Manual `.Close()` | `using` statement |
| **Parameters** | `?` placeholders | `@ParamName` named |
| **Add Params** | `.AddParams()` / `.AddWhereParams()` | `List<DBParameter>` |
| **Execute Non-Query** | `.ExecuteNonQuerySQL(sql)` | `.ExecuteNonQuery(sql, params)` |
| **Execute Reader** | `.ExecuteSQLGetDataReader(sql)` | `.ExecuteReader(sql, params)` |
| **Connection** | OleDb (Access) | SqlClient (SQL Server) |

---

## Search & Replace Patterns

### Step 1: Change SQL Placeholders
```
FIND:    VALUES (?, ?, ?)
REPLACE: VALUES (@Param1, @Param2, @Param3)

FIND:    WHERE ID = ?
REPLACE: WHERE ID = @ID

FIND:    SET Field = ?
REPLACE: SET Field = @Field
```

### Step 2: Replace Class Usage
```
FIND:    TrackerDb trackerDb = new TrackerDb();
REPLACE: using (var db = new TrackerSQLDb())
         {

FIND:    trackerDb.Close();
REPLACE: } // Auto-dispose with using
```

### Step 3: Update Method Calls
```
FIND:    trackerDb.ExecuteNonQuerySQL(
REPLACE: db.ExecuteNonQuery(sql, parameters)

FIND:    trackerDb.ExecuteSQLGetDataReader(
REPLACE: db.ExecuteReader(sql, parameters)
```

---

## Common Code Transformations

### 1. Simple INSERT

**BEFORE:**
```csharp
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddParams((object)field1, DbType.String);
trackerDb.AddParams((object)field2, DbType.Int32);
string result = trackerDb.ExecuteNonQuerySQL("INSERT INTO Table (Field1, Field2) VALUES (?, ?)");
trackerDb.Close();
return result;
```

**AFTER:**
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "INSERT INTO Table (Field1, Field2) VALUES (@Field1, @Field2)";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = field1, DataDbType = DbType.String, ParamName = "@Field1" },
        new DBParameter { DataValue = field2, DataDbType = DbType.Int32, ParamName = "@Field2" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
    return result < 0 ? "ERROR" : string.Empty;
}
```

---

### 2. SELECT with Parameters

**BEFORE:**
```csharp
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddWhereParams((object)id, DbType.Int32);
IDataReader reader = trackerDb.ExecuteSQLGetDataReader("SELECT * FROM Table WHERE ID = ?");
if (reader != null)
{
    while (reader.Read()) { /* process */ }
    reader.Close();
}
trackerDb.Close();
```

**AFTER:**
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "SELECT * FROM Table WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };

    using (IDataReader reader = db.ExecuteReader(sql, parameters))
    {
        if (reader != null)
        {
            while (reader.Read()) { /* process */ }
        }
    }
}
```

---

### 3. UPDATE

**BEFORE:**
```csharp
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddParams((object)newValue, DbType.String);
trackerDb.AddWhereParams((object)id, DbType.Int32);
string result = trackerDb.ExecuteNonQuerySQL("UPDATE Table SET Field = ? WHERE ID = ?");
trackerDb.Close();
return result;
```

**AFTER:**
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "UPDATE Table SET Field = @Field WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = newValue, DataDbType = DbType.String, ParamName = "@Field" },
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
    return result < 0 ? "ERROR" : string.Empty;
}
```

---

### 4. DELETE

**BEFORE:**
```csharp
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddWhereParams((object)id, DbType.Int32);
string result = trackerDb.ExecuteNonQuerySQL("DELETE FROM Table WHERE ID = ?");
trackerDb.Close();
return result;
```

**AFTER:**
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "DELETE FROM Table WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
    return result < 0 ? "ERROR" : string.Empty;
}
```

---

### 5. SELECT Without Parameters

**BEFORE:**
```csharp
TrackerDb trackerDb = new TrackerDb();
IDataReader reader = trackerDb.ExecuteSQLGetDataReader("SELECT * FROM Table");
if (reader != null)
{
    while (reader.Read()) { /* process */ }
    reader.Close();
}
trackerDb.Close();
```

**AFTER:**
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "SELECT * FROM Table";
    using (IDataReader reader = db.ExecuteReader(sql))
    {
        if (reader != null)
        {
            while (reader.Read()) { /* process */ }
        }
    }
}
```

---

## Parameter Creation Shortcuts

### Single Parameter
```csharp
var parameters = new List<DBParameter>
{
    new DBParameter { DataValue = value, DataDbType = DbType.String, ParamName = "@Name" }
};
```

### Multiple Parameters
```csharp
var parameters = new List<DBParameter>
{
    new DBParameter { DataValue = value1, DataDbType = DbType.String, ParamName = "@Field1" },
    new DBParameter { DataValue = value2, DataDbType = DbType.Int32, ParamName = "@Field2" },
    new DBParameter { DataValue = value3, DataDbType = DbType.Boolean, ParamName = "@Field3" }
};
```

### With Null Handling
```csharp
var parameters = new List<DBParameter>
{
    new DBParameter { 
        DataValue = string.IsNullOrEmpty(value) ? (object)DBNull.Value : value, 
        DataDbType = DbType.String, 
        ParamName = "@Field" 
    }
};
```

---

## Common DbTypes

```csharp
DbType.String      // string
DbType.Int32       // int
DbType.Int64       // long
DbType.Byte        // byte
DbType.Boolean     // bool
DbType.DateTime    // DateTime (with time)
DbType.Date        // DateTime (date only)
DbType.Decimal     // decimal
DbType.Double      // double
DbType.Single      // float
```

---

## Error Handling Pattern

### Simple
```csharp
int result = db.ExecuteNonQuery(sql, parameters);
if (result < 0)
{
    return "ERROR: Operation failed";
}
return string.Empty; // Success
```

### With Logging
```csharp
int result = db.ExecuteNonQuery(sql, parameters);
if (result < 0)
{
    AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
        $"Failed to insert/update/delete: {details}");
    return "ERROR: Operation failed";
}

AppLogger.WriteLog(SystemConstants.LogTypes.System, 
    $"Successfully inserted/updated/deleted: {details}");
return string.Empty; // Success
```

---

## Migration Workflow

1. **Identify** TrackerDb usage
   ```powershell
   # Run PowerShell script
   .\DevTools\Scripts\Find-TrackerDbUsages.ps1
   ```

2. **Plan** method migration order
   - Start with simple INSERT/UPDATE/DELETE
   - Then do SELECT methods
   - Save complex queries for last

3. **Migrate** one method at a time
   - Copy old code to comment block
   - Write new code below
   - Test immediately
   - Remove old code when confirmed working

4. **Test** thoroughly
   - Unit test if available
   - Manual test in application
   - Verify data integrity

5. **Commit** when complete
   ```git
   git add Controls\FileName.cs
   git commit -m "Migrated FileName.cs from TrackerDb to TrackerSQLDb"
   ```

---

## Common Mistakes to Avoid

1. **Forgetting to rename parameters in SQL**
   ```csharp
   ? WRONG:  "WHERE ID = ?"
   ? CORRECT: "WHERE ID = @ID"
   ```

2. **Not disposing connections**
   ```csharp
   ? WRONG:  var db = new TrackerSQLDb(); ... db.Close();
   ? CORRECT: using (var db = new TrackerSQLDb()) { ... }
   ```

3. **Parameter order mismatch**
   ```csharp
   ? WRONG:  SQL has @Field1, @Field2, but parameters list has Field2, Field1
   ? CORRECT: Match parameter names to SQL placeholders
   ```

4. **Forgetting to pass parameters**
   ```csharp
   ? WRONG:  db.ExecuteReader(sql)  // No parameters
   ? CORRECT: db.ExecuteReader(sql, parameters)
   ```

5. **Not wrapping reader in using**
   ```csharp
   ? WRONG:  IDataReader reader = db.ExecuteReader(sql);
   ? CORRECT: using (IDataReader reader = db.ExecuteReader(sql)) { ... }
   ```

---

## Visual Studio Tips

### Find All TrackerDb Usages
1. Ctrl+Shift+F (Find in Files)
2. Search for: `new TrackerDb\(\)`
3. Use Regular Expressions
4. Search in: Entire Solution

### Bulk Rename Parameters
1. Find: `VALUES \(\?, \?, \?\)`
2. Replace: `VALUES (@Param1, @Param2, @Param3)`
3. Use Regular Expressions
4. Manually update @ParamName for each

### Format SQL Strings
```csharp
// Use verbatim strings for multiline SQL
string sql = @"
    SELECT * 
    FROM Table 
    WHERE ID = @ID 
    ORDER BY Name";
```

---

## Testing Checklist

For each migrated file:
- [ ] Code compiles without errors
- [ ] All methods tested individually
- [ ] Integration tests pass
- [ ] No performance regression
- [ ] Logging works correctly
- [ ] Error handling tested
- [ ] NULL values handled correctly
- [ ] Data integrity verified

---

## References

- [Full Migration Plan](../Documentation/TrackerDb_to_TrackerSQLDb_Migration_Plan.md)
- [Migration Template](../Documentation/Migration_Template.md)
- [HARD_PROJECT_RULES.md](../../Documentation/HARD_PROJECT_RULES.md)
- [PROJECT_OVERVIEW.md](../../Documentation/PROJECT_OVERVIEW.md)

---

**Need help?** Check the full migration plan or template for detailed examples!
