# MigrationRunner Menu Options - Updated

## ? Fixed Menu Key Conflict

**RESOLVED**: The menu key conflict has been fixed:

### Current Menu Options:

| Key | Command | Description |
|-----|---------|-------------|
| **X** | `DropAllTablesCommand` | Drop ALL tables in database (DANGER: Deletes all data!) |
| **&** | `PostMigrationVerificationCommand` | ?? Post-migration data validation (with % success per table) |

## Complete Menu Structure

```
== Migration Runner (menu v2: options 1..12, A..D, M/MS/N/!, O, R, X, &, Z, $) ==

Select an option:
   1) Export Access schema (per-table JSON)
   2) Review/Edit migration plan (per table)
   3) Export plan summary CSV
   4) Export plan CSV (interactive: All/Tables/Columns/Normalize/Assignments)
   5) Import plan CSV (interactive: All/Tables/Columns/Normalize/Assignments)
   6) Bulk rename (dry-run, all tables)
   7) Bulk rename (apply, all tables)
   8) Create full plan review (JSON + Markdown)
   9) Export Before/After CSV (side-by-side types, keys, FKs)
  10) Export Human Review CSV (per-table sections, Excel-friendly)
  11) Import Human Review CSV (plan edits + PK/FK/Identity constraints)
  12) Validate plan (relations, business rules, preflight, source vs target counts)
   A) Generate CREATE TABLE DDL (SQL)
   B) Generate FK constraints DDL (SQL)
   C) Apply latest DDL scripts to target DB
   D) Open SQL output folder
   !) Custom migrate Orders + Recurring tables (normalization)
   M) Generate data migration script (SQL)
  MS) Stage Access data to SQL [AccessSrc] schema
   N) Apply data migration script (excluding normalized tables) with enhanced error handling
   O) [Post-migration verification or other command]
   R) Reset/Clean migration tables (drop Orders, Recurring, Orphan tables)
   X) Drop ALL tables in database (DANGER: Deletes all data!)
   &) ?? Post-migration data validation (with % success per table)
   Z) Full migration pipeline (A?B?C?D?M?MS?N?! sequence)
   $) Create tables (no FKs), load data, then apply FK constraints (ask-once)
   0) Exit
```

## Key Features:

### **Option X - Drop ALL Tables** ??
- **Purpose**: Nuclear reset - drops all user tables
- **Safety**: Triple confirmation required
- **Preserves**: System tables, ASP.NET membership, WebPages
- **Use Case**: Clean slate for fresh migration after fixing ignored tables

### **Option & - Post-Migration Verification** ??
- **Purpose**: Comprehensive data validation with statistics
- **Features**: Table-by-table success percentages, orphan detection
- **Reports**: Detailed migration success metrics
- **Use Case**: Verify migration completed successfully

## Menu Organization:

1. **Numbered Options (1-12)**: Schema and plan management
2. **Letters (A-D)**: DDL generation and application  
3. **Symbols (!,M,MS,N)**: Data migration and normalization
4. **Letters (O-R)**: Verification and cleanup
5. **Symbols (X,&)**: Dangerous operations and validation
6. **Letters (Z,$)**: Pipeline automation
7. **Zero (0)**: Exit

## Menu Key Sorting Logic:

The `SortKey` method ensures proper ordering:
- Numbers (1-12) appear first
- Letters appear in alphabetical order
- Special symbols are positioned logically:
  - `!` appears after `N` 
  - `$` appears after `Z`
  - `&` appears after `X` (right after dangerous operations)

## Usage Examples:

### After Dropping Tables (Option X):
```
X ? A ? C ? $ ? &
```
*(Drop ? Generate Schema ? Apply Schema ? Full Migration ? Verify)*

### Regular Migration Flow:
```
A ? C ? M ? N ? ! ? &
```
*(Schema ? Data ? Normalize ? Verify)*

### Quick Full Pipeline:
```
$ ? &
```
*(One-shot pipeline ? Verify)*

## Notes:

- **Option X** requires exact phrases: "DROP ALL TABLES" and "YES DELETE EVERYTHING"
- **Option &** provides detailed statistics and recommendations
- Both options support batch mode via `RunRangeState` for automation
- Menu is automatically sorted for consistent presentation

---

**? Problem Solved**: No more menu key conflicts! Both dangerous operations (X) and verification (&) have unique, logical positions in the menu.