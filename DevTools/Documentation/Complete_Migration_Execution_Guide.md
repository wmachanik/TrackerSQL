# Complete TrackerDb Migration - Execution Guide

**Created:** 2025-05-14  
**Status:** Ready for Execution  
**Goal:** Migrate all 66 files from TrackerDb to TrackerSQLDb

---

## ?? Mission: Complete Migration

**Objective:** Remove all TrackerDb (OleDb/Access) dependencies from the project

**Scope:** 66 files, 1,746 usage patterns

**Timeline:** 3 weeks (can be shortened with automation)

---

## ?? Quick Start - Automated Migration

### Step 1: Generate Missing POCO Classes

```powershell
cd C:\SRC\ASP.net\TrackerSQL

# Analyze what POCOs are missing
.\DevTools\Scripts\Generate-PocoClasses.ps1

# Generate missing POCO classes
.\DevTools\Scripts\Generate-PocoClasses.ps1 -GenerateFiles
```

**Expected Output:** Creates POCO classes for any table classes that don't have them

---

### Step 2: Run Dry Run Migration (Phase 1)

```powershell
# Test migration on Phase 1 files (Critical)
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only -DryRun
```

**Expected Output:** Shows what changes will be made without modifying files

---

### Step 3: Execute Phase 1 Migration

```powershell
# Migrate Phase 1 files
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only
```

**Files Affected:**
- `Controls\CustomersTbl.cs` (168 usages)
- `Controls\ItemTypeTbl.cs` (99 usages)
- `Controls\OrderDataControl.cs` (14 usages)

**What Happens:**
1. ? Creates backup of each file
2. ? Replaces `new TrackerDb()` with `using (var db = new TrackerSQLDb())`
3. ? Replaces `.Close()` with closing braces
4. ?? Marks parameter conversion spots with TODO comments
5. ? Generates migration log

---

### Step 4: Manual Parameter Conversion (Critical!)

After automation, you **MUST** manually update:

1. **SQL Parameter Placeholders:**
   ```csharp
   // OLD
   "INSERT INTO Table (Field) VALUES (?)"

   // NEW
   "INSERT INTO Table (Field) VALUES (@Field)"
   ```

2. **Parameter Lists:**
   ```csharp
   // OLD
   trackerDb.AddParams((object)value, DbType.String);

   // NEW
   var parameters = new List<DBParameter>
   {
       new DBParameter { DataValue = value, DataDbType = DbType.String, ParamName = "@Field" }
   };
   ```

3. **Method Calls:**
   ```csharp
   // OLD
   trackerDb.ExecuteNonQuerySQL(sql);

   // NEW
   db.ExecuteNonQuery(sql, parameters);
   ```

**Use Quick Reference:** `DevTools\Documentation\Quick_Reference_TrackerDb_Migration.md`

---

### Step 5: Test Phase 1 Files

```powershell
# Build solution
msbuild C:\SRC\ASP.net\TrackerSQL\TrackerSQL.sln /t:Build

# Run tests (if available)
# Test manually by running the application
```

**Testing Checklist per File:**
- [ ] Code compiles without errors
- [ ] Unit tests pass (if available)
- [ ] Manual testing of functionality
- [ ] No regression in behavior
- [ ] Performance is acceptable

---

### Step 6: Commit Phase 1

```bash
git add Controls\CustomersTbl.cs
git add Controls\ItemTypeTbl.cs
git add Controls\OrderDataControl.cs
git commit -m "Phase 1: Migrated critical files from TrackerDb to TrackerSQLDb

- CustomersTbl.cs: Migrated 168 usages
- ItemTypeTbl.cs: Migrated 99 usages
- OrderDataControl.cs: Migrated 14 usages
- All tests passing
- Functionality verified

Part of complete TrackerDb removal initiative"
```

---

### Step 7: Repeat for Remaining Phases

**Phase 2 (High Impact):**
```powershell
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase2Only
```

**Phase 3 (Supporting Tables):**
```powershell
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase3Only
```

**Phase 4 (Lookup Tables):**
```powershell
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase4Only
```

**All Phases at Once (Advanced):**
```powershell
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1
```

---

## ?? Repository Pattern Migration (Recommended)

For better architecture, consider creating Repository classes instead of just migrating table classes:

### Example: CustomersTbl.cs ? ContactsRepository.cs

Instead of migrating `CustomersTbl.cs` directly, create:

**`Classes\Sql\ContactsRepository.cs`:**
```csharp
using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactsRepository : RepositoryBase<Contact>
    {
        protected override string TableName => "ContactsTbl";
        protected override string KeyColumn => "ContactID";

        public List<Contact> GetAll(string sortBy = "CompanyName")
        {
            string sql = $"SELECT * FROM {TableName}";
            if (!string.IsNullOrEmpty(sortBy))
                sql += $" ORDER BY {sortBy}";

            List<Contact> contacts = new List<Contact>();

            using (var db = new TrackerSQLDb())
            {
                using (IDataReader reader = db.ExecuteReader(sql))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            contacts.Add(MapFromReader(reader));
                        }
                    }
                }
            }

            return contacts;
        }

        public Contact GetById(int contactId)
        {
            string sql = $"SELECT * FROM {TableName} WHERE {KeyColumn} = @ContactID";

            using (var db = new TrackerSQLDb())
            {
                var parameters = new List<DBParameter>
                {
                    new DBParameter { DataValue = contactId, DataDbType = DbType.Int32, ParamName = "@ContactID" }
                };

                using (IDataReader reader = db.ExecuteReader(sql, parameters))
                {
                    if (reader != null && reader.Read())
                    {
                        return MapFromReader(reader);
                    }
                }
            }

            return null;
        }

        protected override Contact MapFromReader(IDataReader reader)
        {
            return new Contact
            {
                ContactID = reader["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ContactID"]),
                CompanyName = reader["CompanyName"] == DBNull.Value ? string.Empty : reader["CompanyName"].ToString(),
                // ... map all other properties
            };
        }
    }
}
```

**Benefits:**
- ? Clean separation of concerns
- ? Reusable across pages
- ? Testable
- ? Follows project architecture rules
- ? Type-safe with POCO classes

---

## ?? Migration Phases in Detail

### Phase 1: Critical Path (3-4 files)

**Priority:** HIGHEST  
**Impact:** Maximum  
**Estimated Time:** 2-3 days

Files:
1. `Controls\CustomersTbl.cs` - Customer operations (168 usages)
2. `Controls\ItemTypeTbl.cs` - Item management (99 usages)
3. `Controls\OrderDataControl.cs` - Order processing (14 usages)

**Why First:**
- Core business operations
- Used by many other files
- Highest risk if broken

---

### Phase 2: High Impact (5 files)

**Priority:** HIGH  
**Impact:** High  
**Estimated Time:** 2-3 days

Files:
1. `Controls\ReoccuringOrderDAL.cs` (61 usages)
2. `Controls\PersonsTbl.cs` (49 usages)
3. `Controls\ItemUsageTbl.cs` (45 usages)
4. `Controls\ClientUsageTbl.cs` (42 usages)
5. `Controls\OrderDetailDAL.cs` (36 usages)

**Why Second:**
- Important business logic
- Dependencies on Phase 1
- Medium complexity

---

### Phase 3: Supporting Tables (20 files)

**Priority:** MEDIUM  
**Impact:** Medium  
**Estimated Time:** 3-4 days

Includes:
- Temp order management
- Customer account info
- Equipment tracking
- Repair management
- System data

**Why Third:**
- Supporting functionality
- Lower risk
- Can be done in parallel

---

### Phase 4: Lookup Tables (20+ files)

**Priority:** LOW  
**Impact:** Low  
**Estimated Time:** 2-3 days

Includes:
- Invoice types
- Payment terms
- Price levels
- Section types
- Transaction types

**Why Last:**
- Simple CRUD operations
- Low complexity
- Minimal dependencies

---

## ??? Tools & Scripts Available

### Analysis Tools
1. **Find-TrackerDbUsages.ps1** - Finds all TrackerDb usage
2. **Generate-PocoClasses.ps1** - Creates missing POCO classes

### Migration Tools
3. **Migrate-TrackerDbFile.ps1** - Migrates single file
4. **Migrate-AllTrackerDbFiles.ps1** - Batch migration

### Documentation
5. Complete migration docs in `DevTools\Documentation\`

---

## ?? Success Metrics

### Code Quality
- [ ] Zero `new TrackerDb()` references
- [ ] All using `TrackerSQLDb` with proper disposal
- [ ] Named parameters throughout
- [ ] Error handling and logging added

### Testing
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] Manual testing complete
- [ ] Performance acceptable

### Project Goals
- [ ] TrackerDb.cs marked obsolete
- [ ] All 66 files migrated
- [ ] Repository pattern adopted where applicable
- [ ] Production deployment successful

---

## ?? Important Warnings

### DO NOT Skip Manual Steps!

The automation scripts do **NOT** handle:
1. ? SQL parameter placeholder conversion (? ? @ParamName)
2. ? Parameter list creation (AddParams ? List<DBParameter>)
3. ? Error handling and logging
4. ? DataReader using statements
5. ? Method signature updates

**You MUST manually update these after running automation!**

### Testing is Critical

This is **core data access code**. Any bugs could:
- ? Corrupt data
- ? Cause data loss
- ? Break application functionality
- ? Create security vulnerabilities

**Test thoroughly at every step!**

---

## ?? Recommended Timeline

### Week 1: Foundation
- **Day 1:** Generate POCOs, dry run Phase 1
- **Day 2:** Execute Phase 1 migration
- **Day 3:** Manual parameter updates for Phase 1
- **Day 4:** Test Phase 1 thoroughly
- **Day 5:** Commit Phase 1, start Phase 2

### Week 2: Main Migration
- **Day 1-2:** Phase 2 migration and testing
- **Day 3:** Phase 3 migration (first 10 files)
- **Day 4:** Phase 3 migration (remaining 10 files)
- **Day 5:** Phase 3 testing

### Week 3: Cleanup & Verification
- **Day 1:** Phase 4 migration
- **Day 2:** Phase 4 testing
- **Day 3:** Final regression testing
- **Day 4:** Code review and cleanup
- **Day 5:** Production deployment

---

## ?? Final Steps

### After All Files Migrated

1. **Mark TrackerDb Obsolete:**
   ```csharp
   [Obsolete("Use TrackerSQLDb instead. TrackerDb uses OleDb/Access and is deprecated.")]
   public class TrackerDb
   ```

2. **Update Documentation:**
   - Mark migration plan as 100% complete
   - Update progress tracker
   - Document lessons learned

3. **Production Deployment:**
   - Deploy to staging
   - Full regression testing
   - Deploy to production
   - Monitor for 24-48 hours

4. **Celebrate! ??**
   - Document the success
   - Share lessons learned
   - Consider removing TrackerDb.cs entirely in future

---

## ?? Getting Help

### If You Get Stuck

1. Check `Quick_Reference_TrackerDb_Migration.md`
2. Review `AreaPrepDaysTbl_Migration_Example.md`
3. Search the migration plan
4. Check TODO comments in code

### Common Issues

**Issue:** Parameters not being passed
**Solution:** See Quick Reference - Parameter Creation section

**Issue:** SQL errors at runtime
**Solution:** Check SQL has @ParamName not ?

**Issue:** Connection leaks
**Solution:** Ensure all `using` statements are in place

---

## ? Pre-Migration Checklist

Before starting:
- [ ] All documentation read
- [ ] Scripts tested with -DryRun
- [ ] Git branch created
- [ ] Backup of current codebase
- [ ] Team notified
- [ ] Test environment prepared

---

## ?? Ready to Begin?

Run this command to start:

```powershell
cd C:\SRC\ASP.net\TrackerSQL

# Step 1: Check for missing POCOs
.\DevTools\Scripts\Generate-PocoClasses.ps1 -GenerateFiles

# Step 2: Dry run Phase 1
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only -DryRun

# Step 3: Execute Phase 1 (after reviewing dry run)
.\DevTools\Scripts\Migrate-AllTrackerDbFiles.ps1 -Phase1Only

# Step 4: Manual updates (see TODO comments in files)

# Step 5: Test, commit, repeat for next phase
```

**Good luck!** ??

---

**Remember:** 
- Start small (Phase 1)
- Test frequently
- Commit often
- Ask for help when needed
- Celebrate progress!
