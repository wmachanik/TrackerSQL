# TrackerSQL Documentation - AI-Focused Reference

## 🔴 CRITICAL: READ HARD RULES FIRST

**Before making ANY changes, you MUST read:**

1. **[HARD_PROJECT_RULES.md](HARD_PROJECT_RULES.md)** (MANDATORY)
1b. **[WEBFORMS_UI_STANDARDS.md](WEBFORMS_UI_STANDARDS.md)** — status at bottom; UpdatePanel default; retrofit when editing
2. **[CONTRIBUTING.md](../CONTRIBUTING.md)** - City → Area Refactoring section (2026-05-11)

**These are NON-NEGOTIABLE rules:**
1. NO Microsoft Access Database (use SQL Server only)
2. NO SqlDataSource Controls (use Repository Pattern only)
3. **NO references to legacy "City*" classes** (use "Area*" terminology)

**Violating these rules will require complete code rewrite!**

---

## 🆕 Recent Critical Changes (2026-05-11)

### City → Area Refactoring Complete

A systematic refactoring has replaced all "City*" terminology with "Area*" across the entire codebase. This is a **breaking change** that affects:

- Table names: `CityTbl` → `AreasTbl`, `CityPrepDaysTbl` → `AreaPrepDaysTbl`
- Class names: `CityTblDAL` → **DEPRECATED**, use `AreasRepository` instead
- Variable names: `cityId` → `areaId`, `City` → `Area`
- SQL queries: All must use `AreasTbl` and `AreaPrepDaysTbl`

**⚠️ IF YOU SEE "City" TERMINOLOGY IN CODE:**
- It's legacy/outdated
- Replace with "Area" equivalent
- Reference: `AI_CONTEXT.md` section "CRITICAL: City → Area Refactoring (2026-05-11)"

---

## Purpose

This folder contains **comprehensive documentation specifically designed for AI assistants** (GitHub Copilot, Claude, ChatGPT, and other LLMs) to understand and work effectively with the TrackerSQL migration project.

**Last Updated:** 2026-05-11  
**Documentation Version:** 1.2

---

## Quick Start for AI Assistants

### First Time Working on This Project?

**Read these in order:**

0. **`HARD_PROJECT_RULES.md`** (MANDATORY - 10 min read)
   - **READ THIS FIRST!**
   - Non-negotiable project rules
   - What is absolutely forbidden
   - What is required instead

0. **`[../CONTRIBUTING.md](../CONTRIBUTING.md)`** (MANDATORY - 5 min read)
   - **City → Area Refactoring section** (2026-05-11)
   - Breaking changes explained
   - Code review checklist

1. **`PROJECT_OVERVIEW.md`** (5-10 min read)
   - Get the big picture
   - Understand what TrackerSQL does
   - Learn about the migration context

2. **`AI_CONTEXT.md`** (15-20 min read - EXPANDED for refactoring)
   - Code patterns and conventions
   - C# 7.3 language constraints
   - Common scenarios and solutions
   - **NEW: Complete City → Area refactoring reference**

3. **`ARCHITECTURE_RULES.md`** (10 min read)
   - Repository Pattern details
   - Code organization standards
   - Design patterns to follow

4. **`TABLE_SCHEMA_REFERENCE.md`** (Reference as needed)
   - Complete table and column mappings
   - Use when writing database code
   - Verify naming conventions

5. **`MIGRATION_GUIDE.md`** (Reference as needed)
   - Migration procedures and processes
   - Use when working on migration tasks

6. **`CODE_STRUCTURE.md`** (Reference as needed)
   - Codebase organization
   - File locations and patterns
   - Use when navigating the project

---

## Document Descriptions

### 🔴 AI_CONTEXT.md - **EXPANDED with Refactoring Documentation**

**Purpose:** Specific guidance for AI code generation and assistance

**Key New Section:**
- **CRITICAL: City → Area Refactoring (2026-05-11)**
  - Complete before/after code examples
  - Breaking changes with workarounds
  - How to identify remaining legacy code
  - Complete mapping reference table
  - Files affected by the change

**Existing Contents:**
- Code patterns and conventions
- Standard implementations
- C# 7.3 language constraints
- Common scenarios with examples
- Anti-patterns to avoid
- Best practices

**Use this when:**
- Writing new code
- Modifying existing code
- Generating code suggestions
- Answering coding questions
- Debugging issues related to areas/delivery schedules
- **NEW: Updating legacy City* code to Area* equivalents**

**Most Important Parts:**
- Database Access Pattern
- Date/Time Handling (always use `TimeZoneUtils.Now()`)
- **CRITICAL: City → Area Refactoring section**
- Null Handling Pattern
- Table/Column Name Mappings

---

### 📋 PROJECT_OVERVIEW.md

**Purpose:** Executive summary and project context

**Contents:**
- Project purpose and business context
- Technology stack details
- Repository structure
- Migration status and timeline
- Quick reference for all aspects of the project

**Use this when:**
- Starting work on the project
- Need high-level context
- Explaining the project to others
- Understanding project scope

**Key Sections:**
- Executive Summary
- Repository Structure
- Project Components
- Database Migration Strategy
- Naming Convention Changes
- Current Migration Status

---

### 📊 TABLE_SCHEMA_REFERENCE.md

**Purpose:** Complete database schema reference

**Contents:**
- All table name mappings (Access → SQL Server)
- Column-level mappings for each table
- Data type conversions
- Foreign key relationships
- Verification queries

**Use this when:**
- Writing SQL queries
- Creating/modifying table classes
- Verifying column names
- Understanding relationships
- Working with data access code

**Critical Reference:**
This is the **authoritative source** for table and column names. Always check here before using table/column names in code.

---

### 🔄 MIGRATION_GUIDE.md

**Purpose:** Database migration procedures and strategy

**Contents:**
- Migration types (direct copy, rename, normalization)
- Migration process phases
- Tools and scripts
- Naming convention changes
- Post-migration code updates
- Verification and rollback procedures

---

### 🏗️ ARCHITECTURE_RULES.md

**Purpose:** Code architecture and design patterns

**Contents:**
- Repository Pattern details
- Code organization standards
- Design patterns to follow
- Class hierarchies
- Inheritance patterns

---

### 📁 CODE_STRUCTURE.md

**Purpose:** Codebase organization and architecture

**Contents:**
- Solution structure
- Project organization
- Namespace organization
- File naming conventions
- Data access architecture
- Web pages structure

---

## Usage Scenarios

### Scenario 1: Writing a New Database Query

**Read:**
1. `AI_CONTEXT.md` → Database Access Pattern
2. `TABLE_SCHEMA_REFERENCE.md` → Look up table/column names
3. `CODE_STRUCTURE.md` → Understand TrackerDb class

**Pattern to follow:**
```csharp
TrackerDb db = new TrackerDb();
db.AddWhereParams((object)contactId, DbType.Int32);
IDataReader reader = db.ExecuteSQLGetDataReader("SELECT * FROM ContactsTbl WHERE ContactID = ?");
// Process results
db.Close();
```

### Scenario 2: Creating a New Table Class

**Read:**
1. `AI_CONTEXT.md` → Scenario 1: Creating a New Table Class
2. `TABLE_SCHEMA_REFERENCE.md` → Get exact column names
3. `CODE_STRUCTURE.md` → Table Class Pattern

**Use template from AI_CONTEXT.md and fill in table-specific details**

### Scenario 3: Updating Legacy Column Names

**Read:**
1. `TABLE_SCHEMA_REFERENCE.md` → Find old → new mappings
2. `MIGRATION_GUIDE.md` → Post-Migration Code Updates
3. `AI_CONTEXT.md` → Common Migration Issues

**Search for old names, replace with new names per reference**

### 🆕 Scenario 4: Fixing City/Area-Related Code

**Read:**
1. **`AI_CONTEXT.md` → CRITICAL: City → Area Refactoring section**
2. **`../CONTRIBUTING.md` → City → Area Refactoring section**
3. Examples: `DateCalculator.cs`, `RecurringOrdersRepository.cs`, `AreasRepository.cs`

**Common fixes:**
- Replace `CityTblDAL` with `AreasRepository`
- Replace `CityPrepDaysTbl` type with `PrepRule`
- Replace `cityId` variables with `areaId`
- Update SQL: `CityTbl` → `AreasTbl`, `CityPrepDaysTbl` → `AreaPrepDaysTbl`

### Scenario 5: Understanding Business Logic

**Read:**
1. `PROJECT_OVERVIEW.md` → Business Context
2. `CODE_STRUCTURE.md` → Business logic location
3. Source files in `Classes\` folder

### Scenario 6: Fixing a Date Handling Bug

**Read:**
1. `AI_CONTEXT.md` → Date/Time Handling Pattern
2. `AI_CONTEXT.md` → Common Migration Issues

**Remember:** ALWAYS use `TimeZoneUtils.Now()` instead of `DateTime.Now`

---

## Documentation Maintenance

### When to Update These Docs

**Update CONTRIBUTING.md when:**
- New refactoring completed
- Breaking changes introduced
- Code review standards change

**Update AI_CONTEXT.md when:**
- New code patterns established
- Common issues identified
- Best practices evolved
- Language version changes
- **Breaking refactorings completed** (like City → Area)

**Update TABLE_SCHEMA_REFERENCE.md when:**
- Tables added/removed
- Columns added/renamed
- Relationships change
- Schema migrations occur

**Update MIGRATION_GUIDE.md when:**
- Migration phases complete
- Migration procedures change
- New tools added
- Issues resolved

**Update CODE_STRUCTURE.md when:**
- Folder structure changes
- New namespaces added
- Architecture changes
- New patterns established

### Version Control

Each document has a version history table at the bottom:

```markdown
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial creation |
| 1.1 | 2025-XX-XX | [description] |
```

Update version number and add entry when making significant changes.

---

## Additional Resources

### Other Documentation in Repository

**`Docs\MigrationPlaybook_TrackerDotNet_to_TrackerSQL.md`**
- Original migration playbook
- Junior developer focused
- Detailed phase-by-phase guide

**`Migrations\README_MIGRATION.md`**
- Quick migration execution guide
- Table and column renaming focus

**`Migrations\README_FIX.md`**
- Recent bug fixes
- Quick fix procedures

**`Data\Metadata\README.md`**
- Naming conventions reference
- Access → SQL Server mappings

### Migration Data Files

**`Migrations\Data\TableMigrationReport-10-Mar-26.xlsx`**
- **MASTER REFERENCE** for migration
- Complete table and column mappings
- Migration type for each table
- Row-by-row transformation details

**Use the Excel file when:**
- Documentation is incomplete
- Need detailed transformation rules
- Verifying specific table migrations
- Understanding original Access schema

### Generated SQL Scripts

**Location:** `Data\Metadata\PlanEdits\Sql\`

**Files:**
- `CreateTables_LATEST.sql` - Table creation
- `DataMigration_LATEST.sql` - Data migration
- `AddForeignKeys_LATEST.sql` - Foreign keys

**Use these to:**
- Understand target schema
- See exact SQL being used
- Verify migrations
- Debug issues

---

## Tips for AI Assistants

### Do's ✅

- **Always reference TABLE_SCHEMA_REFERENCE.md** for table/column names
- **Always use AI_CONTEXT.md patterns** for new code
- **Always check AI_CONTEXT.md City → Area section** when you see area-related code
- **Always use `TimeZoneUtils.Now()`** instead of `DateTime.Now`
- **Always use parameterized queries** via TrackerDb
- **Always close database connections** with `trackerDb.Close()`
- **Always log operations** with AppLogger
- **Always handle DBNull** when reading from database
- **Always use "Area*" terminology, never "City*"**

### Don'ts ❌

- **Don't assume column names** - always verify
- **Don't use Access-era names** (Customer*, Machine*, Roast*)
- **Don't use legacy City* classes** (use AreasRepository instead)
- **Don't use C# 8+ features** (nullable reference types, switch expressions, etc.)
- **Don't skip error handling** and logging
- **Don't use string interpolation** in SQL queries (use parameters)
- **Don't use DateTime.Now** (use TimeZoneUtils.Now())

### When Uncertain

**Priority order:**
1. Check these documentation files
2. Check AI_CONTEXT.md **City → Area Refactoring** section
3. Check the Excel file (`TableMigrationReport-10-Mar-26.xlsx`)
4. Search existing code for patterns (prefer Area* patterns)
5. Ask the user for clarification

### Code Review Checklist

Before suggesting code, verify:
- [ ] Table names use SQL Server names (Contact*, not Customer*)
- [ ] Column names use SQL Server names (ContactID, not CustomerID)
- [ ] ✅ **Area-related code uses "Area*" terminology, not "City*"**
- [ ] Uses `TimeZoneUtils.Now()` for dates
- [ ] Uses parameterized queries (no SQL injection)
- [ ] Closes database connections
- [ ] Handles DBNull properly
- [ ] Includes error handling
- [ ] Includes logging
- [ ] Uses C# 7.3 compatible syntax only
- [ ] Follows existing code patterns

---

## Quick Reference Card

**For rapid context loading:**

```
PROJECT: TrackerSQL - ASP.NET Web Forms ? SQL Server Migration
TECH STACK: .NET Framework 4.8, C# 7.3, ASP.NET Web Forms
DATABASE: Migrating from Access to SQL Server
STATUS: Schema ? Data ? Code ?? (in progress)

KEY CONVENTIONS:
- Use Contact* (not Customer*)
- Use Equipment*/Equip* (not Machine*)
- Use Prep* (not Roast*)
- Use Area* (not Area*)
- Use TimeZoneUtils.Now() (not DateTime.Now)
- Use parameterized queries always
- Check TABLE_SCHEMA_REFERENCE.md for all names

MASTER REFERENCE: Migrations\Data\TableMigrationReport-10-Mar-26.xlsx
```

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-03-26 | AI Documentation Initiative | Initial README creation, all docs v1.0 |
| 1.1 | 2025-XX-XX | Project Team | Various updates and refinements |
| 1.2 | 2026-05-11 | AI Assistant | City → Area refactoring documentation, updated all references |

---

**Welcome to TrackerSQL!** This documentation will help you understand and work effectively with the codebase. Happy coding! 🚀

**LATEST UPDATE (2026-05-11):** City → Area refactoring is complete. Refer to AI_CONTEXT.md for complete reference.



