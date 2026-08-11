# TrackerSQL Project - AI Context Documentation

## STOP! READ THIS FIRST

### HARD PROJECT RULES (NON-NEGOTIABLE)

**Before making ANY changes to this project, you MUST read:**

**[HARD_PROJECT_RULES.md](HARD_PROJECT_RULES.md)** 

**Summary of HARD RULES:**

1. **NO Microsoft Access Database**
   - We are migrating FROM Access TO SQL Server
   - Use TrackerDataSQL connection string ONLY
   - NO OleDb classes allowed

2. **NO SqlDataSource Controls**
   - Use Repository Pattern ONLY
   - Manual data binding in code-behind
   - Exception: sdsUserNames (ASP.NET membership only)

**Violating these rules will require complete code rewrite. Read the full rules document!**

---

## CRITICAL PROJECT RULE - READ FIRST

### **MANDATORY: Repository Pattern Only**

**ALL data access in this project MUST use the Repository pattern.**

? **ABSOLUTELY PROHIBITED:**
- SqlDataSource controls in ASPX pages
- ObjectDataSource controls (ALL of them, including those pointing to repositories)
- Direct database access in code-behind
- Inline SQL in markup

? **REQUIRED:**
- Repository classes in `Classes/Sql/` inheriting from `RepositoryBase<T>`
- POCO models in `Classes/Poco/`
- Manual data binding in code-behind (no `DataSourceID`)
- Standard CRUD operations through repositories

**?? Full details:** See [ARCHITECTURE_RULES.md](ARCHITECTURE_RULES.md)

**?? NO EXCEPTIONS to this rule!**

---

## Quick Reference Card
**Last Updated:** 2025-03-26  
**Project Type:** ASP.NET Web Forms Migration Project  
**Technology Stack:**
- **Framework:** .NET Framework 4.8
- **Language:** C# 7.3
- **Web Platform:** ASP.NET Web Forms 4.8
- **Source Database:** Microsoft Access (.mdb/.accdb)
- **Target Database:** SQL Server / SQL Express
- **Data Access:** Repository Pattern (mandatory)
- IIS hosting

Key folders:
•	/Pages or /Default.aspx → UI entry points
•	/App_Code → shared business logic (if used)
•	/Services → service or business logic layer
•	/Data or /DAL → database access
•	/Scripts → JavaScript
•	/Styles → CSS

## AI Codebase Context (IMPORTANT)
This project includes a full AI-readable version of the codebase:
📄 documentation/repomix-output.xml
This file allows AI tools to:
•	Understand the full project structure
•	Analyze dependencies
•	Suggest refactoring or improvements across files

### Dev tooling location (standard)
All helper scripts and generated cleanup notes **must** be stored under `DevTools/` going forward:
- PowerShell scripts: `DevTools/Scripts/`
- Dev notes / one-off cleanup docs: `DevTools/Documentation/`

Do not add `.ps1` or ad-hoc cleanup `.md` files to the repository root.


---

## Executive Summary

### What Is This Project?

TrackerSQL is a **dual-purpose migration project** consisting of two related applications:

1. **TrackerSQL Web Application** (Main Application)
   - Legacy ASP.NET Web Forms application
   - ~~Currently uses Microsoft Access database via OleDb~~ **MIGRATED to SQL Server**
   - Uses **Repository Pattern** for all data access
   - **Status:** Work in Progress

2. **MigrationRunner** (Migration Utility)
   - Console application (.NET Framework 4.8)
   - Handles schema extraction and data migration
   - Generates DDL and DML scripts
   - **Status:** Working and Operational

### Business Context

TrackerSQL is a customer relationship and order management system for a coffee roasting business. It tracks:
- Customer contacts and delivery schedules
- Coffee bean inventory and orders
- Equipment tracking and maintenance
- Automated reminder/checkup emails
- Usage predictions and recurring orders

The migration is necessary because:
- Access database has scalability limitations
- 32-bit OleDb/Jet dependencies are problematic
- Need for better performance and reliability
- Modern deployment requirements (64-bit, cloud-ready)

---

## Repository Structure

```
C:\SRC\ASP.net\TrackerSQL\
?
??? TrackerSQL.csproj              # Main web application project
??? Web.config                     # Web application configuration
?
??? Migrations\                    # Migration tooling and metadata
?   ??? MigrationRunner\           # Console app for database migration
?   ?   ??? MigrationRunner.csproj # .NET 4.8 console application
?   ?   ??? Program.cs             # Entry point with interactive menu
?   ?   ??? AccessSchemaReader.cs  # Reads Access database schema
?   ?   ??? DdlScriptGenerator.cs  # Generates CREATE TABLE scripts
?   ?   ??? DmlScriptGenerator.cs  # Generates INSERT scripts
?   ?   ??? FkScriptGenerator.cs   # Generates foreign key scripts
?   ?   ??? Metadata\              # Migration configuration files
?   ?
?   ??? Data\                      # Migration planning data
?   ?   ??? TableMigrationReport-10-Mar-26.xlsx  # MASTER REFERENCE
?   ?   ??? TableMigrationReport-10-Mar-26.csv   # Same data, CSV format
?   ?   ??? [older versions]
?   ?
?   ??? README_MIGRATION.md        # Migration execution guide
?   ??? README_FIX.md              # Recent fixes documentation
?   ??? Scripts\                   # SQL helper scripts
?
??? Data\
?   ??? Metadata\                  # Shared metadata files
?       ??? README.md              # Naming conventions reference
?       ??? PlanEdits\
?           ??? Sql\               # Generated SQL scripts
?           ?   ??? CreateTables_LATEST.sql
?           ?   ??? DataMigration_LATEST.sql
?           ?   ??? AddForeignKeys_LATEST.sql
?           ??? PlanColumns.csv    # Column mapping configuration
?           ??? PlanConstraints.json
?
??? Documentation\                 # **AI-FOCUSED DOCUMENTATION** (THIS FOLDER)
?   ??? PROJECT_OVERVIEW.md        # This file
?   ??? MIGRATION_GUIDE.md         # Detailed migration strategy
?   ??? TABLE_SCHEMA_REFERENCE.md  # Complete table documentation
?   ??? CODE_STRUCTURE.md          # Codebase organization
?   ??? AI_CONTEXT.md              # AI assistant guidelines
?   ??? MIGRATION_NAMING_ALIGNMENT.md
?   ??? Migration\                 # Playbooks / plans (former Docs\)
?   ??? Archive\                   # Historical session notes
?
??? Controls\                      # Data access classes
?   ??? SentRemindersLogTbl.cs     # Reminder tracking
?   ??? ContactsThatMayNeedNextWeek.cs
?   ??? [~50 more table classes]
?
??? Classes\                       # Business logic and utilities
?   ??? TrackerDb.cs               # Database abstraction layer
?   ??? SystemConstants.cs         # Application constants
?   ??? TimeZoneUtils.cs           # Time zone handling
?   ??? [other utilities]
?
??? DataSets\                      # Data access objects
??? Emails\                        # Email generation
??? Pages\                         # ASP.NET web pages (.aspx)
??? Tools\                         # Utility pages
```

---

## Project Components

### 1. Main Web Application (TrackerSQL.csproj)

**Technology:**
- ASP.NET Web Forms 4.8
- .NET Framework 4.8
- C# (likely 7.3, though not explicitly set in main project)

**Key Characteristics:**
- Legacy web application using Web Forms paradigm
- Currently uses OleDb to connect to Access database
- Data access pattern: Individual table classes (e.g., `ContactsTbl.cs`)
- Uses `TrackerDb.cs` as database abstraction layer

**Migration Status:**
- ?? **Partially Migrated** - Some code still references Access
- Schema is defined in SQL Server
- Data has been migrated
- Code refactoring is ongoing

**Current WIP Resume Snapshot (2026-04-23):**
- The recurring-order page migration has started but is not finished
- Active recurring files are:
  - `Pages/RecurringOrders.aspx`
  - `Pages/RecurringOrders.aspx.cs`
  - `Pages/RecurringOrderDetails.aspx`
  - `Pages/RecurringOrderDetails.aspx.cs`
  - `Classes/Sql/RecurringOrdersRepository.cs`
  - `Classes/Poco/RecurringOrder.cs`
  - `Classes/Poco/RecurringOrderItem.cs`
  - `Classes/Poco/RecurringOrderSummary.cs`
- Current page work has already moved away from markup data sources and toward SQL repository binding
- Known blocker: recurring item data appears to have been migrated incorrectly during normalization
- Legacy source table: `AccessSrc.ReoccuringOrderTbl`
- Target normalized tables: `RecurringOrdersTbl` and `RecurringOrderItemsTbl`
- Missing item names in the recurring grid likely indicate incomplete or incorrect population of `RecurringOrderItemsTbl`
- To resume this work, use `Documentation/WorkInProgress/MIGRATION_TODO.md` as the step-by-step handoff checklist together with this overview

### 2. MigrationRunner (Migrations\MigrationRunner\MigrationRunner.csproj)

**Technology:**
- Console Application
- .NET Framework 4.8
- C# 7.3 (explicitly set)
- Platform Target: x64

**Purpose:**
- Extract schema from Access database
- Generate SQL Server DDL scripts
- Generate data migration DML scripts
- Apply bulk renames based on configuration
- Verify migration integrity

**Status:**
- ? **Working and Operational**
- Successfully generates migration scripts
- Has been used to migrate database

**Key Features:**
- Interactive menu-driven interface
- Reads Access schema via OleDb
- Configurable via JSON and CSV metadata files
- Supports table renaming and normalization
- Handles identity column preservation
- Foreign key generation

---

## Database Migration Strategy

### Three Types of Table Migrations

The Excel file `Migrations\Data\TableMigrationReport-10-Mar-26.xlsx` documents three migration patterns:

#### 1. **Direct Copy (Table-to-Table)**
   - Source table copied directly to target with same name
   - Example: `SysDataTbl` ? `SysDataTbl`
   - Columns may be renamed or dropped

#### 2. **Rename Migration**
   - Source table copied to differently-named target
   - Example: `CustomersTbl` ? `ContactsTbl`
   - Example: `AreaTbl` ? `AreasTbl`
   - Reflects terminology standardization

#### 3. **Normalization Migration**
   - Source table split into multiple related tables
   - Example: `OrdersTbl` ? `OrdersTbl` + `OrderLinesTbl`
   - Introduces proper relational structure

### Column-Level Transformations

For each column, the Excel sheet tracks:
- **Column Name Changes** (e.g., `CustomerID` ? `ContactID`)
- **Data Type Changes** (e.g., Access Date/Time ? SQL Server DateTime2)
- **Dropped Columns** (obsolete fields removed)
- **New Columns** (additional fields for SQL Server)

---

## Naming Convention Changes

The migration standardizes terminology across the application:

### Major Terminology Shifts

| Concept | Access Term | SQL Server Term | Rationale |
|---------|-------------|-----------------|-----------|
| **Customers** | Customer*, Client* | Contact* | Unified term for all customer/client entities |
| **Cities** | Area* | Area* | More generic for locations |
| **Coffee Processing** | Roast* | Prep* | Broader than just roasting |
| **Machines** | Machine* | Equipment*/Equip* | Professional terminology |
| **Coffee Types** | ItemType* | Item* | Simplified naming |
| **Packaging** | Packaging* | ItemPackaging* | Clearer relationship |
| **Recurring** | Reoccur* | Recurr* | Correct spelling |

### Examples of Renamed Entities

**Tables:**
- `CustomersTbl` ? `ContactsTbl`
- `ClientUsageTbl` ? `ContactsItemsPredictedTbl`
- `AreaTbl` ? `AreasTbl`
- `ItemTypeTbl` ? `ItemsTbl`
- `PackagingTbl` ? `ItemPackagingsTbl`
- `MachineConditionsTbl` ? `EquipConditionsTbl`

**Columns:**
- `CustomerID` ? `ContactID`
- `PrepDate` ? `PrepDate`
- `MachineSN` ? `EquipmentSN`
- `Abreviation` ? `Abbreviation` (spelling fix)

---

## Key Application Concepts

### Business Domain

**Coffee Roasting Operations:**
- Manage customer relationships and orders
- Track coffee bean inventory
- Schedule roasting/preparation
- Manage deliveries
- Handle equipment tracking

**Customer Management:**
- Contact information
- Delivery schedules
- Usage predictions
- Recurring orders
- Equipment installations

**Email Automation:**
- Automated reminder emails ("checkups")
- Delivery notifications
- Order confirmations

### Important Data Tables

**Core Tables:**
- `ContactsTbl` - Customer/client master data
- `ItemsTbl` - Coffee products and items
- `OrdersTbl` / `OrderLinesTbl` - Order management
- `ContactsItemsPredictedTbl` - Usage predictions
- `EquipConditionsTbl` - Equipment tracking
- `SentRemindersLogTbl` - Email tracking

**Configuration Tables:**
- `SysDataTbl` - System configuration
- `ItemPackagingsTbl` - Packaging types
- `ItemPrepTypesTbl` - Preparation methods
- `AreasTbl` - Delivery areas
- `PeopleTbl` - People

---

## Current Migration Status

### ? Completed
1. Schema extraction from Access database
2. SQL Server table creation scripts generated
3. Data migration scripts generated
4. Database created and populated in SQL Server
5. Foreign key relationships established
6. MigrationRunner tool is fully functional
7. Bulk rename configurations working

### ?? In Progress
1. Web application code refactoring
2. Replacing OleDb calls with SqlClient
3. Updating table/column references to new names
4. Testing web pages with SQL Server backend

### ? Not Started
1. Complete regression testing
2. Performance optimization
3. Production deployment planning
4. Access database retirement

---

## Critical Files for AI Assistants

When assisting with this project, always reference:

### Primary References
1. **`Documentation\TABLE_SCHEMA_REFERENCE.md`** - Complete table and column mappings
2. **`Migrations\Data\TableMigrationReport-10-Mar-26.xlsx`** - Master migration plan
3. **`Data\Metadata\README.md`** - Naming conventions
4. **`Controls\*Tbl.cs`** - Current data access classes

### Migration Scripts
1. **`Data\Metadata\PlanEdits\Sql\CreateTables_LATEST.sql`** - Target schema
2. **`Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql`** - Data migration
3. **`Data\Metadata\PlanEdits\Sql\AddForeignKeys_LATEST.sql`** - FK constraints

### Configuration Files
1. **`Data\Metadata\BulkRenameRules.json`** - Automated renaming
2. **`Data\Metadata\PlanEdits\PlanColumns.csv`** - Column mappings
3. **`Data\Metadata\PlanEdits\PlanConstraints.json`** - Key constraints

---

## Development Guidelines for AI Assistance

### When Modifying Code:

1. **Check Naming Conventions**
   - Use NEW SQL Server names (Contact*, not Customer*)
   - Reference `TABLE_SCHEMA_REFERENCE.md` for mappings

2. **Maintain .NET 4.8 Compatibility**
   - Use C# 7.3 syntax only
   - No nullable reference types
   - No pattern matching enhancements
   - No span/memory types

3. **Follow Existing Patterns**
   - Table classes inherit from base patterns
   - Use `TrackerDb.cs` for database access
   - Use `TimeZoneUtils.Now()` instead of `DateTime.Now`
   - Log with `AppLogger.WriteLog()`

4. **Preserve Migration Context**
   - Don't assume Access field names
   - Check both old and new names in migration period
   - Update comments to reflect SQL Server, not Access

5. **Reuse Shared Repository Logic**
   - If multiple pages need the same lookup ordering / display behavior, put it in the repository instead of copying it into each page
   - Table-specific standard behavior should usually live in the table-specific repository (example: standard `ItemsTbl` dropdown ordering in `Classes/Sql/ItemsRepository.cs`)
   - Keep code-behind focused on binding and page behavior, not re-implementing common lookup rules

6. **Do Not Reintroduce Legacy Aliases In Repository Results**
   - Repository queries should prefer real SQL/project-standard names, not legacy compatibility aliases
   - Do not emit `<TableName>ID AS ID`; keep the standard primary key name such as `AreaID`, `ContactID`, `ItemID`, etc.
   - Do not alias `Area` / `AreaName` back to `Area` in new SQL-backed repository paths
   - If a page still expects old names, fix the page/POCO instead of pushing old terminology back into repository output

7. **CSS Versioning and Cache Busting**
   - **ALWAYS** update version numbers when modifying `Styles/Site.css`
   - Version format: `YYYYMMDD-n`
     - `YYYYMMDD` = Date (e.g., 20260421 = April 21, 2026)
     - `n` = Revision number for that day (1, 2, 3, etc.)
   - **TWO places must match:**
     1. **Site.css header comment**: `VERSION: 20260421-1` and `LAST UPDATED: 2025-04-21 - REV 1`
     2. **Site.Master link tag**: `<link href="~/Styles/Site.css?v=20260421-1" ... />`
   - Add change description to Site.css header changelog
   - This prevents browser caching issues after CSS updates

### Common Pitfalls to Avoid:

? Using `DateTime.Now` ? ? Use `TimeZoneUtils.Now()`  
? Hardcoding column names ? ? Reference constants  
? Using `Customer*` ? ? Use `Contact*`  
? Using `Machine*` ? ? Use `Equipment*` or `Equip*`  
? Modern C# syntax ? ? C# 7.3 only  

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial AI-focused documentation created |

---

## Next Steps for Migration

See `MIGRATION_GUIDE.md` for detailed next steps and `TABLE_SCHEMA_REFERENCE.md` for complete schema documentation.

---

**For AI Assistants:** This document provides high-level context. Always cross-reference with specific documentation files for detailed implementation guidance.
