# TrackerDb Migration Template
# Use this as a guide when migrating individual files from TrackerDb to TrackerSQLDb

## File: [FileName.cs]
**Status:** Not Started / In Progress / Completed  
**Migrated By:** [Your Name]  
**Date:** [YYYY-MM-DD]

---

## Methods to Migrate

### Method 1: [MethodName]

**OLD CODE:**
```csharp
public string InsertSomething(SomeTbl obj)
{
    string empty = string.Empty;
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddParams((object)obj.Field1, DbType.String);
    trackerDb.AddParams((object)obj.Field2, DbType.Int32);
    string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO SomeTbl (Field1, Field2) VALUES (?, ?)");
    trackerDb.Close();
    return str;
}
```

**NEW CODE:**
```csharp
public string InsertSomething(SomeTbl obj)
{
    using (var db = new TrackerSQLDb())
    {
        string sql = @"
            INSERT INTO SomeTbl (Field1, Field2) 
            VALUES (@Field1, @Field2)";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = obj.Field1, DataDbType = DbType.String, ParamName = "@Field1" },
            new DBParameter { DataValue = obj.Field2, DataDbType = DbType.Int32, ParamName = "@Field2" }
        };

        int result = db.ExecuteNonQuery(sql, parameters);

        if (result < 0)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
                $"Failed to insert SomeTbl: {obj.Field1}");
            return "ERROR: Insert failed";
        }

        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
            $"Inserted SomeTbl: {obj.Field1}");
        return string.Empty; // Success
    }
}
```

**Changes Made:**
- [ ] Replaced `TrackerDb` with `TrackerSQLDb`
- [ ] Changed `?` to named parameters `@ParamName`
- [ ] Converted parameters to `List<DBParameter>`
- [ ] Wrapped in `using` statement
- [ ] Updated error handling
- [ ] Added logging

**Testing:**
- [ ] Unit test passed
- [ ] Integration test passed
- [ ] Manual test passed

---

### Method 2: [MethodName]

**OLD CODE:**
```csharp
public List<SomeTbl> GetAll(string sortBy)
{
    List<SomeTbl> all = new List<SomeTbl>();
    string strSQL = "SELECT * FROM SomeTbl" + (!string.IsNullOrEmpty(sortBy) ? " ORDER BY " + sortBy : "");
    TrackerDb trackerDb = new TrackerDb();
    IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
    if (dataReader != null)
    {
        while (dataReader.Read())
        {
            all.Add(new SomeTbl()
            {
                Field1 = dataReader["Field1"] == DBNull.Value ? string.Empty : dataReader["Field1"].ToString(),
                Field2 = dataReader["Field2"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["Field2"])
            });
        }
        dataReader.Close();
    }
    trackerDb.Close();
    return all;
}
```

**NEW CODE:**
```csharp
public List<SomeTbl> GetAll(string sortBy)
{
    List<SomeTbl> all = new List<SomeTbl>();
    string strSQL = "SELECT * FROM SomeTbl" + (!string.IsNullOrEmpty(sortBy) ? " ORDER BY " + sortBy : "");

    using (var db = new TrackerSQLDb())
    {
        using (IDataReader dataReader = db.ExecuteReader(strSQL))
        {
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    all.Add(new SomeTbl()
                    {
                        Field1 = dataReader["Field1"] == DBNull.Value ? string.Empty : dataReader["Field1"].ToString(),
                        Field2 = dataReader["Field2"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["Field2"])
                    });
                }
            }
        }
    }

    return all;
}
```

**Changes Made:**
- [ ] Replaced `TrackerDb` with `TrackerSQLDb`
- [ ] Changed `ExecuteSQLGetDataReader()` to `ExecuteReader()`
- [ ] Wrapped both db and reader in `using` statements
- [ ] Removed explicit `.Close()` calls

**Testing:**
- [ ] Unit test passed
- [ ] Integration test passed
- [ ] Manual test passed

---

### Method 3: [MethodName with Parameters]

**OLD CODE:**
```csharp
public SomeTbl GetById(int id)
{
    SomeTbl result = null;
    string strSQL = "SELECT * FROM SomeTbl WHERE SomeID = ?";
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddWhereParams((object)id, DbType.Int32);
    IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
    if (dataReader != null && dataReader.Read())
    {
        result = new SomeTbl()
        {
            SomeID = Convert.ToInt32(dataReader["SomeID"]),
            Field1 = dataReader["Field1"].ToString()
        };
        dataReader.Close();
    }
    trackerDb.Close();
    return result;
}
```

**NEW CODE:**
```csharp
public SomeTbl GetById(int id)
{
    SomeTbl result = null;
    string strSQL = "SELECT * FROM SomeTbl WHERE SomeID = @SomeID";

    using (var db = new TrackerSQLDb())
    {
        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@SomeID" }
        };

        using (IDataReader dataReader = db.ExecuteReader(strSQL, parameters))
        {
            if (dataReader != null && dataReader.Read())
            {
                result = new SomeTbl()
                {
                    SomeID = Convert.ToInt32(dataReader["SomeID"]),
                    Field1 = dataReader["Field1"].ToString()
                };
            }
        }
    }

    return result;
}
```

**Changes Made:**
- [ ] Replaced `TrackerDb` with `TrackerSQLDb`
- [ ] Changed `?` to `@SomeID`
- [ ] Created `List<DBParameter>` for parameter
- [ ] Passed parameters to `ExecuteReader()`
- [ ] Wrapped in `using` statements

**Testing:**
- [ ] Unit test passed
- [ ] Integration test passed
- [ ] Manual test passed

---

### Method 4: [Update Method]

**OLD CODE:**
```csharp
public string Update(SomeTbl obj, int originalId)
{
    string str = string.Empty;
    if (originalId > 0)
    {
        TrackerDb trackerDb = new TrackerDb();
        trackerDb.AddParams((object)obj.Field1, DbType.String);
        trackerDb.AddParams((object)obj.Field2, DbType.Int32);
        trackerDb.AddWhereParams((object)originalId, DbType.Int32);
        str = trackerDb.ExecuteNonQuerySQL("UPDATE SomeTbl SET Field1 = ?, Field2 = ? WHERE SomeID = ?");
        trackerDb.Close();
    }
    return str;
}
```

**NEW CODE:**
```csharp
public string Update(SomeTbl obj, int originalId)
{
    if (originalId <= 0)
    {
        return "ERROR: Invalid ID";
    }

    using (var db = new TrackerSQLDb())
    {
        string sql = @"
            UPDATE SomeTbl 
            SET Field1 = @Field1, Field2 = @Field2 
            WHERE SomeID = @OriginalId";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = obj.Field1, DataDbType = DbType.String, ParamName = "@Field1" },
            new DBParameter { DataValue = obj.Field2, DataDbType = DbType.Int32, ParamName = "@Field2" },
            new DBParameter { DataValue = originalId, DataDbType = DbType.Int32, ParamName = "@OriginalId" }
        };

        int result = db.ExecuteNonQuery(sql, parameters);

        if (result < 0)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
                $"Failed to update SomeTbl ID {originalId}");
            return "ERROR: Update failed";
        }

        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
            $"Updated SomeTbl ID {originalId}");
        return string.Empty; // Success
    }
}
```

**Changes Made:**
- [ ] Replaced `TrackerDb` with `TrackerSQLDb`
- [ ] Changed `?` to named parameters
- [ ] Created `List<DBParameter>` for all parameters
- [ ] Improved validation at start
- [ ] Added logging
- [ ] Wrapped in `using` statement

**Testing:**
- [ ] Unit test passed
- [ ] Integration test passed
- [ ] Manual test passed

---

### Method 5: [Delete Method]

**OLD CODE:**
```csharp
public string DeleteById(int id)
{
    string empty = string.Empty;
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddWhereParams((object)id, DbType.Int32);
    string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM SomeTbl WHERE SomeID = ?");
    trackerDb.Close();
    return str;
}
```

**NEW CODE:**
```csharp
public string DeleteById(int id)
{
    using (var db = new TrackerSQLDb())
    {
        string sql = "DELETE FROM SomeTbl WHERE SomeID = @SomeID";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@SomeID" }
        };

        int result = db.ExecuteNonQuery(sql, parameters);

        if (result < 0)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
                $"Failed to delete SomeTbl ID {id}");
            return "ERROR: Delete failed";
        }

        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
            $"Deleted SomeTbl ID {id}");
        return string.Empty; // Success
    }
}
```

**Changes Made:**
- [ ] Replaced `TrackerDb` with `TrackerSQLDb`
- [ ] Changed `?` to `@SomeID`
- [ ] Created `List<DBParameter>` for parameter
- [ ] Added logging
- [ ] Wrapped in `using` statement

**Testing:**
- [ ] Unit test passed
- [ ] Integration test passed
- [ ] Manual test passed

---

## Common Patterns Reference

### Pattern 1: Simple INSERT
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "INSERT INTO TableName (Col1, Col2) VALUES (@Col1, @Col2)";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = value1, DataDbType = DbType.String, ParamName = "@Col1" },
        new DBParameter { DataValue = value2, DataDbType = DbType.Int32, ParamName = "@Col2" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
}
```

### Pattern 2: Simple SELECT
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "SELECT * FROM TableName WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };

    using (IDataReader reader = db.ExecuteReader(sql, parameters))
    {
        if (reader != null && reader.Read())
        {
            // Process data
        }
    }
}
```

### Pattern 3: UPDATE
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "UPDATE TableName SET Col1 = @Col1 WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = value, DataDbType = DbType.String, ParamName = "@Col1" },
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
}
```

### Pattern 4: DELETE
```csharp
using (var db = new TrackerSQLDb())
{
    string sql = "DELETE FROM TableName WHERE ID = @ID";
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@ID" }
    };
    int result = db.ExecuteNonQuery(sql, parameters);
}
```

---

## Data Type Mapping

| .NET Type | DbType | Example |
|-----------|--------|---------|
| `string` | `DbType.String` | `new DBParameter { DataValue = "text", DataDbType = DbType.String, ParamName = "@Name" }` |
| `int` | `DbType.Int32` | `new DBParameter { DataValue = 123, DataDbType = DbType.Int32, ParamName = "@ID" }` |
| `long` | `DbType.Int64` | `new DBParameter { DataValue = 123L, DataDbType = DbType.Int64, ParamName = "@BigID" }` |
| `byte` | `DbType.Byte` | `new DBParameter { DataValue = (byte)1, DataDbType = DbType.Byte, ParamName = "@Flag" }` |
| `bool` | `DbType.Boolean` | `new DBParameter { DataValue = true, DataDbType = DbType.Boolean, ParamName = "@IsActive" }` |
| `DateTime` | `DbType.DateTime` or `DbType.Date` | `new DBParameter { DataValue = DateTime.Now, DataDbType = DbType.DateTime, ParamName = "@Created" }` |
| `decimal` | `DbType.Decimal` | `new DBParameter { DataValue = 99.99m, DataDbType = DbType.Decimal, ParamName = "@Price" }` |
| `double` | `DbType.Double` | `new DBParameter { DataValue = 99.99, DataDbType = DbType.Double, ParamName = "@Value" }` |

---

## File Status Tracker

- [ ] All methods identified
- [ ] All methods migrated
- [ ] All tests passed
- [ ] Code reviewed
- [ ] Committed to repository
- [ ] Marked as COMPLETED in migration plan

---

## Notes / Special Cases

[Document any special considerations, edge cases, or complex logic here]

---

## References

- [TrackerDb_to_TrackerSQLDb_Migration_Plan.md](../Documentation/TrackerDb_to_TrackerSQLDb_Migration_Plan.md)
- [HARD_PROJECT_RULES.md](../../Documentation/HARD_PROJECT_RULES.md)
- [PROJECT_OVERVIEW.md](../../Documentation/PROJECT_OVERVIEW.md)
