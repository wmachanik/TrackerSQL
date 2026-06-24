# AreaPrepDaysTbl.cs Migration Example

**File:** Controls\AreaPrepDaysTbl.cs  
**Status:** Example/Template  
**Purpose:** Complete worked example of TrackerDb to TrackerSQLDb migration  
**Date:** 2025-05-14

---

## Overview

This file shows the complete migration of `AreaPrepDaysTbl.cs` from the old `TrackerDb` (OleDb/Access) pattern to the new `TrackerSQLDb` (SQL Server) pattern.

Use this as a reference when migrating other files.

---

## Methods Migrated

1. `GetAllByAreaId()` - SELECT with parameter
2. `InsertAreaPrepDay()` - INSERT
3. `UpdateAreaPrepDay()` - UPDATE
4. `DeleteByAreaPrepDayID()` - DELETE

---

## Method 1: GetAllByAreaId() - SELECT with WHERE clause

### OLD CODE ?
```csharp
public List<AreaPrepDaysTbl> GetAllByAreaId(int pAreaID)
{
    string strSQL = "SELECT AreaPrepDaysID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM AreaPrepDaysTbl WHERE AreaID = ? ORDER BY PrepDayOfWeekID";
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddWhereParams((object)pAreaID, DbType.Int32);
    List<AreaPrepDaysTbl> allByAreaId = new List<AreaPrepDaysTbl>();
    IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
    if (dataReader != null)
    {
        while (dataReader.Read())
            allByAreaId.Add(new AreaPrepDaysTbl()
            {
                AreaID = pAreaID,
                AreaPrepDaysID = dataReader["AreaPrepDaysID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaPrepDaysID"]),
                PrepDayOfWeekID = dataReader["PrepDayOfWeekID"] == DBNull.Value ? (byte)0 : Convert.ToByte(dataReader["PrepDayOfWeekID"]),
                DeliveryDelayDays = dataReader["DeliveryDelayDays"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryDelayDays"]),
                DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryOrder"])
            });
        dataReader.Close();
    }
    trackerDb.Close();
    return allByAreaId;
}
```

### NEW CODE ?
```csharp
public List<AreaPrepDaysTbl> GetAllByAreaId(int pAreaID)
{
    string strSQL = @"
        SELECT AreaPrepDaysID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder 
        FROM AreaPrepDaysTbl 
        WHERE AreaID = @AreaID 
        ORDER BY PrepDayOfWeekID";

    List<AreaPrepDaysTbl> allByAreaId = new List<AreaPrepDaysTbl>();

    using (var db = new TrackerSQLDb())
    {
        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = pAreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" }
        };

        using (IDataReader dataReader = db.ExecuteReader(strSQL, parameters))
        {
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    allByAreaId.Add(new AreaPrepDaysTbl()
                    {
                        AreaID = pAreaID,
                        AreaPrepDaysID = dataReader["AreaPrepDaysID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaPrepDaysID"]),
                        PrepDayOfWeekID = dataReader["PrepDayOfWeekID"] == DBNull.Value ? (byte)0 : Convert.ToByte(dataReader["PrepDayOfWeekID"]),
                        DeliveryDelayDays = dataReader["DeliveryDelayDays"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryDelayDays"]),
                        DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : (int)Convert.ToInt16(dataReader["DeliveryOrder"])
                    });
                }
            }
        }
    }

    return allByAreaId;
}
```

### Changes Made ?
1. ? Replaced `TrackerDb` with `TrackerSQLDb`
2. ? Changed SQL placeholder `?` to named parameter `@AreaID`
3. ? Created `List<DBParameter>` for the parameter
4. ? Passed parameters to `ExecuteReader()`
5. ? Wrapped db in `using` statement
6. ? Wrapped dataReader in `using` statement
7. ? Removed explicit `.Close()` calls
8. ? Kept all DBNull handling logic unchanged

### Key Points ??
- The data reading logic stays **exactly the same**
- Only the connection and parameter passing changed
- `using` statements ensure proper cleanup
- Named parameters are more explicit than `?`

---

## Method 2: InsertAreaPrepDay() - INSERT

### OLD CODE ?
```csharp
public string InsertAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl)
{
    string empty = string.Empty;
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddParams((object)objAreaPrepDaysTbl.AreaID, DbType.Int32);
    trackerDb.AddParams((object)objAreaPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
    trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
    trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryOrder, DbType.Int32);
    string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (?, ?, ?, ?)");
    trackerDb.Close();
    return str;
}
```

### NEW CODE ?
```csharp
public string InsertAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl)
{
    using (var db = new TrackerSQLDb())
    {
        string sql = @"
            INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) 
            VALUES (@AreaID, @PrepDayOfWeekID, @DeliveryDelayDays, @DeliveryOrder)";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = objAreaPrepDaysTbl.AreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" },
            new DBParameter { DataValue = objAreaPrepDaysTbl.PrepDayOfWeekID, DataDbType = DbType.Byte, ParamName = "@PrepDayOfWeekID" },
            new DBParameter { DataValue = objAreaPrepDaysTbl.DeliveryDelayDays, DataDbType = DbType.Int32, ParamName = "@DeliveryDelayDays" },
            new DBParameter { DataValue = objAreaPrepDaysTbl.DeliveryOrder, DataDbType = DbType.Int32, ParamName = "@DeliveryOrder" }
        };

        int result = db.ExecuteNonQuery(sql, parameters);

        if (result < 0)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
                $"Failed to insert AreaPrepDay for AreaID: {objAreaPrepDaysTbl.AreaID}");
            return "ERROR: Failed to insert area prep day";
        }

        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
            $"Inserted AreaPrepDay for AreaID: {objAreaPrepDaysTbl.AreaID}, PrepDay: {objAreaPrepDaysTbl.PrepDayOfWeekID}");

        return string.Empty; // Success
    }
}
```

### Changes Made ?
1. ? Replaced `TrackerDb` with `TrackerSQLDb`
2. ? Changed `?` placeholders to `@AreaID`, `@PrepDayOfWeekID`, etc.
3. ? Created `List<DBParameter>` with all 4 parameters
4. ? Changed `ExecuteNonQuerySQL()` to `ExecuteNonQuery(sql, parameters)`
5. ? Added error checking on result
6. ? Added logging for both success and failure
7. ? Wrapped in `using` statement
8. ? Removed explicit `.Close()` call

### Key Points ??
- **Parameter Order:** Matches SQL column order, but names are explicit
- **Error Handling:** Now checks `result < 0` for errors
- **Logging:** Added system and error logging
- **Return Value:** Empty string on success, error message on failure

---

## Method 3: UpdateAreaPrepDay() - UPDATE

### OLD CODE ?
```csharp
public string UpdateAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl, int origAreaPrepDaysID)
{
    string str = string.Empty;
    if (origAreaPrepDaysID > 0)
    {
        TrackerDb trackerDb = new TrackerDb();
        trackerDb.AddParams((object)objAreaPrepDaysTbl.AreaID, DbType.Int32);
        trackerDb.AddParams((object)objAreaPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
        trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
        trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryOrder, DbType.Int32);
        trackerDb.AddWhereParams((object)origAreaPrepDaysID, DbType.Int32);
        str = trackerDb.ExecuteNonQuerySQL("UPDATE AreaPrepDaysTbl SET AreaID = ?, PrepDayOfWeekID = ?, DeliveryDelayDays = ?, DeliveryOrder = ? WHERE (AreaPrepDaysID = ?)");
        trackerDb.Close();
    }
    return str;
}
```

### NEW CODE ?
```csharp
public string UpdateAreaPrepDay(AreaPrepDaysTbl objAreaPrepDaysTbl, int origAreaPrepDaysID)
{
    if (origAreaPrepDaysID <= 0)
    {
        return "ERROR: Invalid AreaPrepDaysID";
    }

    using (var db = new TrackerSQLDb())
    {
        string sql = @"
            UPDATE AreaPrepDaysTbl 
            SET AreaID = @AreaID, 
                PrepDayOfWeekID = @PrepDayOfWeekID, 
                DeliveryDelayDays = @DeliveryDelayDays, 
                DeliveryOrder = @DeliveryOrder 
            WHERE AreaPrepDaysID = @AreaPrepDaysID";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = objAreaPrepDaysTbl.AreaID, DataDbType = DbType.Int32, ParamName = "@AreaID" },
            new DBParameter { DataValue = objAreaPrepDaysTbl.PrepDayOfWeekID, DataDbType = DbType.Byte, ParamName = "@PrepDayOfWeekID" },
            new DBParameter { DataValue = objAreaPrepDaysTbl.DeliveryDelayDays, DataDbType = DbType.Int32, ParamName = "@DeliveryDelayDays" },
            new DBParameter { DataValue = objAreaPrepDaysTbl.DeliveryOrder, DataDbType = DbType.Int32, ParamName = "@DeliveryOrder" },
            new DBParameter { DataValue = origAreaPrepDaysID, DataDbType = DbType.Int32, ParamName = "@AreaPrepDaysID" }
        };

        int result = db.ExecuteNonQuery(sql, parameters);

        if (result < 0)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
                $"Failed to update AreaPrepDay ID: {origAreaPrepDaysID}");
            return "ERROR: Failed to update area prep day";
        }

        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
            $"Updated AreaPrepDay ID: {origAreaPrepDaysID}, AreaID: {objAreaPrepDaysTbl.AreaID}");

        return string.Empty; // Success
    }
}
```

### Changes Made ?
1. ? Replaced `TrackerDb` with `TrackerSQLDb`
2. ? Changed all `?` to named parameters
3. ? Created `List<DBParameter>` with 5 parameters (4 SET + 1 WHERE)
4. ? Improved validation at the start
5. ? Added error checking and logging
6. ? Wrapped in `using` statement
7. ? Made SQL more readable with multiline formatting

### Key Points ??
- **5 Parameters:** 4 for SET clause, 1 for WHERE clause
- **Validation:** Early return if ID is invalid
- **Parameter Names:** Match SQL placeholders exactly
- **WHERE Parameter:** Named `@AreaPrepDaysID` to match column name

---

## Method 4: DeleteByAreaPrepDayID() - DELETE

### OLD CODE ?
```csharp
public string DeleteByAreaPrepDayID(int pAreaPrepDayID)
{
    string empty = string.Empty;
    TrackerDb trackerDb = new TrackerDb();
    trackerDb.AddWhereParams((object)pAreaPrepDayID, DbType.Int32, "@AreaPrepDayID");
    string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM AreaPrepDaysTbl WHERE (AreaPrepDaysID = ?)");
    trackerDb.Close();
    return str;
}
```

### NEW CODE ?
```csharp
public string DeleteByAreaPrepDayID(int pAreaPrepDayID)
{
    using (var db = new TrackerSQLDb())
    {
        string sql = "DELETE FROM AreaPrepDaysTbl WHERE AreaPrepDaysID = @AreaPrepDaysID";

        var parameters = new List<DBParameter>
        {
            new DBParameter { DataValue = pAreaPrepDayID, DataDbType = DbType.Int32, ParamName = "@AreaPrepDaysID" }
        };

        int result = db.ExecuteNonQuery(sql, parameters);

        if (result < 0)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Error, 
                $"Failed to delete AreaPrepDay ID: {pAreaPrepDayID}");
            return "ERROR: Failed to delete area prep day";
        }

        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
            $"Deleted AreaPrepDay ID: {pAreaPrepDayID}");

        return string.Empty; // Success
    }
}
```

### Changes Made ?
1. ? Replaced `TrackerDb` with `TrackerSQLDb`
2. ? Changed `?` to `@AreaPrepDaysID`
3. ? Created `List<DBParameter>` with single parameter
4. ? Added error checking and logging
5. ? Wrapped in `using` statement
6. ? Simplified SQL (removed unnecessary parentheses)

### Key Points ??
- **Single Parameter:** DELETE usually has one WHERE parameter
- **Logging:** Important for audit trail on deletions
- **Simple SQL:** No need for extra parentheses in WHERE clause

---

## Before & After Summary

| Aspect | Before (TrackerDb) | After (TrackerSQLDb) |
|--------|-------------------|---------------------|
| **Connection** | Manual creation & close | `using` auto-dispose |
| **Parameters** | `.AddParams()` / `.AddWhereParams()` | `List<DBParameter>` |
| **SQL** | `?` placeholders | `@ParamName` named |
| **Execute** | `.ExecuteNonQuerySQL()` | `.ExecuteNonQuery(sql, params)` |
| **Reader** | `.ExecuteSQLGetDataReader()` | `.ExecuteReader(sql, params)` |
| **Error Handling** | String return only | Check result + logging |
| **Cleanup** | `.Close()` calls | Automatic via `using` |

---

## Testing Checklist ?

After migration, verify:
- [ ] Code compiles without errors
- [ ] `GetAllByAreaId()` returns correct records
- [ ] `InsertAreaPrepDay()` successfully inserts
- [ ] `UpdateAreaPrepDay()` updates correct record
- [ ] `DeleteByAreaPrepDayID()` deletes correct record
- [ ] Error logging appears in logs when expected
- [ ] Success logging appears in logs
- [ ] No memory leaks (connections auto-dispose)
- [ ] Performance is same or better
- [ ] NULL values handled correctly

---

## Common Issues & Solutions

### Issue 1: Parameter Name Mismatch
**Problem:** SQL says `@AreaID` but parameter list has `@ID`  
**Solution:** Make sure parameter names match exactly

### Issue 2: Parameter Order Wrong
**Problem:** Parameters in different order than SQL expects  
**Solution:** Use named parameters - order doesn't matter!

### Issue 3: Forgot `using` on reader
**Problem:** Memory leak from not closing reader  
**Solution:** Always wrap `IDataReader` in `using` statement

### Issue 4: Connection not disposed
**Problem:** SQL Server connection pool exhausted  
**Solution:** Always wrap `TrackerSQLDb` in `using` statement

---

## Next Steps

Use this example as a template for:
1. Other `*Tbl.cs` files in Controls folder
2. Any DAL (Data Access Layer) files
3. Any code-behind files with database access

Remember:
- Start simple (SELECT first)
- Test each method individually
- Add logging for troubleshooting
- Use `using` statements for cleanup

---

## References

- [Migration Plan](TrackerDb_to_TrackerSQLDb_Migration_Plan.md)
- [Quick Reference](Quick_Reference_TrackerDb_Migration.md)
- [Migration Template](Migration_Template.md)

**Good luck with your migrations!** ??
