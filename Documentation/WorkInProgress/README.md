# Work In Progress - Code Migration

## Purpose

This folder contains **active working documents** related to the ongoing code migration from Access to SQL Server naming conventions. Documents here track current work, issues being resolved, and TODO lists.

---

## Folder Contents

### Active Documents

1. **MIGRATION_TODO.md** - Master TODO list for code migration
2. **CURRENT_ISSUES.md** - Known issues being worked on
3. **COMPLETED_TASKS.md** - Archive of completed work

### Migration Notes (from root Migrations folder)

Reference these for recent fixes:
- `../Migrations/GITHUB_SUBMISSION_COMPLETE.md`
- `../Migrations/REGEX_FIX_BROKEN_PROPER_FIX.md`
- `../Migrations/AMBIGUOUS_COLUMN_NAMES_FIX.md`
- `../Migrations/TWO_FIXES_NEED_REGENERATE.md`

---

## Document Organization

### MIGRATION_TODO.md
**Primary working document** - Contains prioritized list of all code files that need updating, organized by:
- Priority (Easy ? Medium ? Hard)
- Type (Table Classes, Web Pages, Managers, etc.)
- Status (Not Started, In Progress, Complete)
- Complexity (Simple rename, Moderate changes, Complex refactoring)

### CURRENT_ISSUES.md
**Active problem tracking** - Documents issues discovered during migration:
- Problem description
- Impact
- Proposed solution
- Status
- Related files

### COMPLETED_TASKS.md
**Achievement log** - Archive of completed tasks:
- What was done
- Date completed
- Files changed
- Any notes or gotchas discovered

---

## Workflow

### When Starting Work on a File

1. Check **MIGRATION_TODO.md** for priority
2. Mark task as "In Progress"
3. Update **CURRENT_ISSUES.md** if problems found
4. Reference **AI_CONTEXT.md** for patterns
5. Reference **TABLE_SCHEMA_REFERENCE.md** for names

### When Completing Work on a File

1. Mark task as "Complete" in **MIGRATION_TODO.md**
2. Move details to **COMPLETED_TASKS.md**
3. Update **CURRENT_ISSUES.md** if issues resolved
4. Test thoroughly
5. Commit with clear message

### When Finding Issues

1. Document in **CURRENT_ISSUES.md** immediately
2. Note which files are affected
3. Propose solution if possible
4. Link to TODO items affected

---

## Quick Links

**Main Documentation:**
- [Project Overview](../PROJECT_OVERVIEW.md)
- [AI Context Guide](../AI_CONTEXT.md)
- [Table Schema Reference](../TABLE_SCHEMA_REFERENCE.md)
- [Migration Guide](../MIGRATION_GUIDE.md)
- [Code Structure](../CODE_STRUCTURE.md)

**Master Reference:**
- Excel File: `../../Migrations/Data/TableMigrationReport-10-Mar-26.xlsx`

---

## Status Overview

**Last Updated:** 2025-03-26

### Migration Progress

| Category | Total Files | Completed | In Progress | Not Started | % Complete |
|----------|------------|-----------|-------------|-------------|------------|
| **POCOs** | ~30 | 30 | 0 | 0 | **100%** ? |
| **Repositories** | ~25 | 25 | 0 | 0 | **100%** ? |
| **Table Classes** | ~50 | 0 | 0 | 50 | **0%** ? |
| **Web Pages** | ~30 | 2 | 0 | 28 | **7%** ?? |
| **Managers** | ~10 | 5 | 0 | 5 | **50%** ?? |
| **Data Access** | ~20 | 0 | 0 | 20 | **0%** ? |
| **TOTAL** | ~165 | 62 | 0 | 103 | **38%** ?? |

### Priority Areas

**High Priority (Blocks other work):**
- Table Classes using old table names

**Medium Priority (User-facing):**
- Web Pages (.aspx.cs files)
- Managers (business logic)

**Low Priority (Internal):**
- Data Access Layer classes
- Helper utilities

---

## Tips for Working in This Folder

### Keep Documents Updated

? **Do:**
- Update TODO status as you work
- Document issues immediately
- Move completed tasks to archive
- Cross-reference related items
- Date all entries

? **Don't:**
- Leave stale status (always update)
- Forget to archive completed work
- Skip documenting issues
- Work without checking TODO first

### Use Consistent Status Markers

- ? **Complete** - Done and tested
- ?? **In Progress** - Currently being worked on
- ?? **Blocked** - Waiting on something
- ?? **Has Issues** - Problems discovered
- ? **Not Started** - Not yet begun
- ?? **Planned** - Scheduled for future

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial Work In Progress folder created |

---

**This is your active workspace for the migration. Keep it updated and organized!** ??
