# TrackerDb to TrackerSQLDb Migration Plan

**Created:** 2025-05-14  
**Purpose:** Complete migration from legacy OleDb `TrackerDb` to SQL Server `TrackerSQLDb`  
**Status:** Planning Phase

---

## Overview

This document tracks the migration of all legacy `TrackerDb` (OleDb/Access) calls to the new `TrackerSQLDb` (SQL Server) pattern across the TrackerSQL codebase.

### Migration Patterns

#### OLD PATTERN (TrackerDb - OleDb/Access):
```csharp
string empty = string.Empty;
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddParams((object)objAreaPrepDaysTbl.AreaID, DbType.Int32);
trackerDb.AddParams((object)objAreaPrepDaysTbl.PrepDayOfWeekID, DbType.Byte);
trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryDelayDays, DbType.Int32);
trackerDb.AddParams((object)objAreaPrepDaysTbl.DeliveryOrder, DbType.Int32);
string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (?, ?, ?, ?)");
trackerDb.Close();
```

#### NEW PATTERN (TrackerSQLDb - SQL Server):
```csharp
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
        return "ERROR: Failed to insert area prep day";
    }
    return string.Empty; // Success
}
```

#### For SELECT queries (OLD):
```csharp
string strSQL = "SELECT * FROM SomeTable WHERE SomeID = ?";
TrackerDb trackerDb = new TrackerDb();
trackerDb.AddWhereParams((object)someId, DbType.Int32);
IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
if (dataReader != null)
{
    while (dataReader.Read())
    {
        // Process data
    }
    dataReader.Close();
}
trackerDb.Close();
```

#### For SELECT queries (NEW):
```csharp
string strSQL = "SELECT * FROM SomeTable WHERE SomeID = @SomeID";

using (var db = new TrackerSQLDb())
{
    var parameters = new List<DBParameter>
    {
        new DBParameter { DataValue = someId, DataDbType = DbType.Int32, ParamName = "@SomeID" }
    };

    using (IDataReader dataReader = db.ExecuteReader(strSQL, parameters))
    {
        if (dataReader != null)
        {
            while (dataReader.Read())
            {
                // Process data
            }
        }
    }
}
```

---

## Key Differences

| Aspect | OLD (TrackerDb) | NEW (TrackerSQLDb) |
|--------|-----------------|-------------------|
| **Parameters** | `?` placeholders | Named `@ParamName` |
| **Parameter Adding** | `.AddParams()` and `.AddWhereParams()` | `List<DBParameter>` |
| **Execution** | `.ExecuteNonQuerySQL()` | `.ExecuteNonQuery(sql, parameters)` |
| **Reader** | `.ExecuteSQLGetDataReader()` | `.ExecuteReader(sql, parameters)` |
| **Cleanup** | `.Close()` | `using` statement (auto-dispose) |
| **Connection** | OleDb (Access) | SqlClient (SQL Server) |

---

## Files Requiring Migration

### TIER 1: Controls\*Tbl.cs Files (Legacy Data Access)

These files use the old `TrackerDb` pattern and need to be migrated:

#### 1. ? **COMPLETED**: 
None yet

#### 2. ?? **IN PROGRESS**:

| File | Methods Affected | Priority | Notes |
|------|------------------|----------|-------|
| `Controls\AreaPrepDaysTbl.cs` | `GetAllByAreaId()`, `InsertAreaPrepDay()`, `UpdateAreaPrepDay()`, `DeleteByAreaPrepDayID()` | HIGH | Example file - good template |

#### 3. ? **NOT STARTED** (HIGH PRIORITY):

| File | Estimated Methods | Priority | Notes |
|------|------------------|----------|-------|
| `Controls\ItemTypeTbl.cs` | 10+ methods | HIGH | Complex, many dependencies |
| `Controls\PersonsTbl.cs` | 5+ methods | HIGH | People/Delivery management |
| `Controls\CustomersTbl.cs` | 15+ methods | CRITICAL | Core customer operations |
| `Controls\OrdersTbl.cs` | 10+ methods | CRITICAL | Order management |
| `Controls\OrderDataControl.cs` | 5+ methods | HIGH | Order data processing |
| `Controls\OrderDetailDAL.cs` | 5+ methods | HIGH | Order details |
| `Controls\TempOrdersDAL.cs` | 8+ methods | MEDIUM | Temporary orders |
| `Controls\TempOrdersHeaderTbl.cs` | 5+ methods | MEDIUM | Temp order headers |
| `Controls\TempOrdersLinesTbl.cs` | 5+ methods | MEDIUM | Temp order lines |
| `Controls\ReoccuringOrderDAL.cs` | 10+ methods | HIGH | Recurring orders |
| `Controls\CustomersAwayTbl.cs` | 5+ methods | MEDIUM | Away periods |
| `Controls\CustomersAccInfoTbl.cs` | 5+ methods | MEDIUM | Account info |
| `Controls\RepairsTbl.cs` | 8+ methods | MEDIUM | Repairs management |
| `Controls\RepairStatusesTbl.cs` | 5+ methods | MEDIUM | Repair statuses |
| `Controls\ItemUsageTbl.cs` | 5+ methods | HIGH | Item usage tracking |
| `Controls\ClientUsageTbl.cs` | 5+ methods | HIGH | Client usage prediction |
| `Controls\ClientUsageLinesTbl.cs` | 5+ methods | MEDIUM | Usage line details |
| `Controls\PriceLevelsTbl.cs` | 3+ methods | LOW | Price levels |
| `Controls\PaymentTermsTbl.cs` | 3+ methods | LOW | Payment terms |
| `Controls\InvoiceTypeTbl.cs` | 3+ methods | LOW | Invoice types |
| `Controls\EquipTypeTbl.cs` | 3+ methods | LOW | Equipment types |
| `Controls\MachineConditionsTbl.cs` | 3+ methods | LOW | Machine conditions |
| `Controls\CustomerTypeTbl.cs` | 3+ methods | LOW | Customer types |
| `Controls\PackagingTbl.cs` | 3+ methods | LOW | Packaging types |
| `Controls\ItemGroupTbl.cs` | 5+ methods | MEDIUM | Item grouping |
| `Controls\ItemUnitsTbl.cs` | 3+ methods | LOW | Item units |
| `Controls\SysDataTbl.cs` | 5+ methods | MEDIUM | System data |
| `Controls\SentRemindersLogTbl.cs` | 5+ methods | MEDIUM | Reminder logging |
| `Controls\LogTbl.cs` | 3+ methods | LOW | General logging |
| `Controls\SectionTypesTbl.cs` | 3+ methods | LOW | Section types |
| `Controls\TempCoffeeCheckup.cs` | 5+ methods | MEDIUM | Coffee checkup temp |

#### 4. ? **NOT STARTED** (MEDIUM PRIORITY):

| File | Estimated Methods | Notes |
|------|------------------|-------|
| `Controls\HolidayClosureProvider.cs` | 8+ methods | Holiday management |
| `Controls\NextPrepDateByAreaTbl.cs` | 5+ methods | Prep date scheduling |
| `Controls\AreaTblDAL.cs` | 5+ methods | Area data access |

#### 5. ?? **SPECIAL CASES**:

| File | Status | Notes |
|------|--------|-------|
| `Controls\ActiveDeliveryData.cs` | REVIEW | May use datasets |
| `Controls\CompanyNames.cs` | REVIEW | May be simple lookups |
| `Controls\ContactEmailDetails.cs` | REVIEW | May be view-only |
| `Controls\CustomerSummaryDAL.cs` | REVIEW | Summary queries |

---

## Migration Checklist Per File

For each file, follow this checklist:

### Pre-Migration
- [ ] Review all methods in the file
- [ ] Identify all `TrackerDb` usages
- [ ] List all SQL queries
- [ ] Note any special parameter handling
- [ ] Check for transaction requirements

### Migration Steps
- [ ] Replace `TrackerDb` with `TrackerSQLDb`
- [ ] Change `?` placeholders to `@ParamName` in SQL
- [ ] Convert `AddParams()`/`AddWhereParams()` to `List<DBParameter>`
- [ ] Update method calls:
  - `ExecuteNonQuerySQL()` ? `ExecuteNonQuery(sql, parameters)`
  - `ExecuteSQLGetDataReader()` ? `ExecuteReader(sql, parameters)`
- [ ] Wrap in `using` statement for auto-dispose
- [ ] Update error handling if needed
- [ ] Add proper logging

### Post-Migration
- [ ] Build and verify no compilation errors
- [ ] Test all affected methods
- [ ] Update unit tests if any
- [ ] Mark file as COMPLETED in this document
- [ ] Document any issues or special cases

---

## Priority Order for Migration

### Phase 1: Critical Path (Week 1)
1. `CustomersTbl.cs` - Core customer operations
2. `OrdersTbl.cs` - Core order operations
3. `ItemTypeTbl.cs` - Item management
4. `OrderDataControl.cs` - Order data processing

### Phase 2: High Impact (Week 2)
1. `ReoccuringOrderDAL.cs` - Recurring orders
2. `ItemUsageTbl.cs` - Usage tracking
3. `PersonsTbl.cs` - People management
4. `OrderDetailDAL.cs` - Order details
5. `ClientUsageTbl.cs` - Usage predictions

### Phase 3: Supporting Tables (Week 3)
1. Temp order files (3 files)
2. Customer-related files (3 files)
3. Repair-related files (2 files)
4. Item-related files (3 files)

### Phase 4: Lookup Tables (Week 4)
1. All remaining `*Tbl.cs` files with simple CRUD
2. Logging and system tables
3. Configuration tables

---

## Testing Strategy

### Per-File Testing
1. **Unit Test** - Test each migrated method individually
2. **Integration Test** - Test method interactions
3. **Data Integrity** - Verify data reads/writes correctly
4. **Performance** - Compare query performance

### Overall Testing
1. **Smoke Test** - Basic functionality after each phase
2. **Regression Test** - Ensure no breaking changes
3. **End-to-End Test** - Full user workflows
4. **Load Test** - Performance under load

---

## Known Issues / Special Cases

### Issue 1: Parameter Order
- **Problem**: Old code relied on parameter position
- **Solution**: Named parameters are explicit and safer
- **Action**: Verify parameter names match SQL placeholders

### Issue 2: DBNull Handling
- **Problem**: Both patterns handle DBNull differently
- **Solution**: Maintain existing null checks
- **Action**: No change needed in data reading logic

### Issue 3: Transaction Support
- **Problem**: Some methods may need transactions
- **Solution**: Use `TrackerSQLDb.BeginTransaction()`
- **Action**: Identify methods needing transactions

### Issue 4: Connection Pooling
- **Problem**: Old code manually closes connections
- **Solution**: `using` statements auto-manage connections
- **Action**: Remove explicit `.Close()` calls

---

## Migration Template

Use this template for each file migration:

```csharp
// OLD PATTERN
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

// NEW PATTERN
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

---

## Progress Tracking

| Phase | Files | Completed | In Progress | Not Started | % Complete |
|-------|-------|-----------|-------------|-------------|------------|
| **Phase 1** | 4 | 0 | 1 | 3 | 0% |
| **Phase 2** | 5 | 0 | 0 | 5 | 0% |
| **Phase 3** | 11 | 0 | 0 | 11 | 0% |
| **Phase 4** | 12 | 0 | 0 | 12 | 0% |
| **TOTAL** | **32** | **0** | **1** | **31** | **0%** |

---

## Related Documentation

- [HARD_PROJECT_RULES.md](../../Documentation/HARD_PROJECT_RULES.md) - Project rules
- [MIGRATION_TODO.md](../../Documentation/WorkInProgress/MIGRATION_TODO.md) - Page migration
- [PROJECT_OVERVIEW.md](../../Documentation/PROJECT_OVERVIEW.md) - Project overview
- [TABLE_SCHEMA_REFERENCE.md](../../Documentation/TABLE_SCHEMA_REFERENCE.md) - Schema reference

---

## Notes

1. **Repository Pattern Preferred**: Where possible, consider creating Repository classes instead of just migrating `*Tbl.cs` files
2. **POCO Models**: Use existing POCO models in `Classes\Poco\` folder
3. **Error Handling**: Add proper error logging with `AppLogger`
4. **SQL Naming**: Ensure SQL table/column names match new SQL Server schema
5. **Testing**: Test thoroughly - this is core data access code

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-05-14 | Initial migration plan created |

---

**Next Steps:**
1. Start with `AreaPrepDaysTbl.cs` as example
2. Create migration branch
3. Migrate Phase 1 files first
4. Test thoroughly before moving to Phase 2
