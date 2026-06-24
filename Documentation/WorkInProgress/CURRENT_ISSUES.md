# Current Migration Issues

**Last Updated:** 2025-03-26  
**Purpose:** Track active problems discovered during code migration

---

## Active Issues

### Issue #1: Column Name Ambiguity in SentRemindersLogTbl

**Status:** ?? Documented, Not Yet Fixed  
**Priority:** Medium  
**Discovered:** 2025-03-26  
**Impact:** Inconsistent naming

**Description:**
The `SentRemindersLogTbl` table class uses `CustomerID` as the column name, but it references `ContactsTbl.ContactID`. This creates confusion and inconsistency.

**Files Affected:**
- `Controls\SentRemindersLogTbl.cs` - Uses CustomerID property
- Database table `SentRemindersLogTbl` - Column is named CustomerID (not yet renamed to ContactID)

**Current Behavior:**
```csharp
public long CustomerID { get; set; }  // References ContactsTbl.ContactID
```

**Expected Behavior:**
```csharp
public long ContactID { get; set; }  // Consistent with ContactsTbl
```

**Recommended Fix:**
1. Update `SentRemindersLogTbl.cs` class:
   - Rename property `CustomerID` ? `ContactID`
   - Update all SQL queries to use `ContactID`
2. Consider adding database migration to rename column
3. Update any code that uses `SentRemindersLogTbl.CustomerID`

**Blocker Status:** Not blocking - can work around
**Related TODO Items:** Priority 2, item 2.9 (indirectly)

---

### Issue #2: DataSet Generated Code May Be Stale

**Status:** ?? Needs Investigation  
**Priority:** High  
**Discovered:** 2025-03-26  
**Impact:** Potential runtime errors

**Description:**
Several DataSet files in `DataSets\` folder are generated code that may reference old table/column names. These are typed datasets from an older .NET pattern.

**Files Affected:**
- `DataSets\CustomersDataSet.cs` - Large generated file
- `DataSets\TrackerDataSet.cs` - Large generated file
- `DataSets\CustomersCls.cs` - May be generated
- `DataSets\TrackerDataSetTableAdapters\*.cs` - All generated

**Investigation Needed:**
1. Are these files still used?
2. Can they be replaced with POCO + Repository pattern?
3. If needed, how to regenerate with new names?
4. What's the migration path?

**Potential Solutions:**
- **Option A:** Regenerate DataSets with new schema (complex)
- **Option B:** Phase out DataSets, use POCOs instead (recommended)
- **Option C:** Leave as-is if not actively used (risky)

**Recommendation:**
Conduct code analysis to determine usage, then decide on approach. POCOs + Repositories are already complete and modern.

**Blocker Status:** May block Priority 3 work
**Related TODO Items:** 3.28, 3.29, 3.30

---

### Issue #3: Page Aspx Markup May Reference Old Names

**Status:** ?? Needs Verification  
**Priority:** Medium  
**Discovered:** 2025-03-26  
**Impact:** Possible binding errors

**Description:**
Web Forms .aspx markup files may reference old table class names in ObjectDataSource, GridView bindings, etc.

**Example:**
```aspx
<asp:ObjectDataSource ID="odsCustomers" runat="server"
    TypeName="TrackerSQL.Controls.CustomersTbl"
    SelectMethod="GetAllCustomers" />
```

Should become:
```aspx
<asp:ObjectDataSource ID="odsCustomers" runat="server"
    TypeName="TrackerSQL.Controls.ContactsTbl"
    SelectMethod="GetAllContacts" />
```

**Files Likely Affected:**
- All .aspx files in `Pages\` folder
- All .aspx files in `Tools\` folder
- All .aspx files in `Administration\` folder
- Master pages

**Verification Needed:**
```powershell
# Search .aspx files for old class names
Get-ChildItem -Recurse -Include *.aspx | Select-String "CustomersTbl|ClientUsageTbl|ItemTypeTbl"
```

**Recommended Fix:**
1. When updating .aspx.cs code-behind, also check .aspx markup
2. Update TypeName attributes
3. Update method names if changed
4. Test data binding works

**Blocker Status:** Not blocking but required for page functionality
**Related TODO Items:** All Priority 2 web page items

---

## Resolved Issues

*(None yet - move completed issues here from Active section)*

---

## Issue Template

Use this template when documenting new issues:

```markdown
### Issue #N: [Brief Title]

**Status:** ?? [Active/Investigating/Blocked]  
**Priority:** [High/Medium/Low]  
**Discovered:** YYYY-MM-DD  
**Impact:** [Description of impact]

**Description:**
[Detailed description of the problem]

**Files Affected:**
- File1.cs - Description
- File2.cs - Description

**Current Behavior:**
[Code example or description]

**Expected Behavior:**
[What should happen instead]

**Recommended Fix:**
1. Step 1
2. Step 2
3. Step 3

**Blocker Status:** [Blocking/Not blocking - details]
**Related TODO Items:** [List relevant TODO items]
```

---

## How to Update This Document

### When Discovering New Issue

1. Add to **Active Issues** section
2. Assign next sequential issue number
3. Fill in all template fields
4. Link to relevant TODO items
5. Update Last Updated date

### When Resolving Issue

1. Mark status as ? Resolved
2. Add resolution date
3. Document solution applied
4. Move to **Resolved Issues** section
5. Update related TODO items if needed

### When Issue Becomes Blocker

1. Update status to ?? Blocked
2. Document what is blocked
3. Update priority if needed
4. Notify team/lead
5. Consider workaround

---

## Issue Priority Guidelines

**High Priority:**
- Blocks multiple TODO items
- Causes runtime errors
- Affects core functionality
- Security concern

**Medium Priority:**
- Affects specific features
- Creates inconsistency
- Needs investigation
- May become blocker

**Low Priority:**
- Minor inconsistency
- Documentation issue
- Nice-to-fix
- No immediate impact

---

## Quick Links

**Related Documents:**
- [MIGRATION_TODO.md](MIGRATION_TODO.md) - See what's affected
- [AI_CONTEXT.md](../AI_CONTEXT.md) - Check patterns
- [TABLE_SCHEMA_REFERENCE.md](../TABLE_SCHEMA_REFERENCE.md) - Verify names

**Getting Help:**
- Check Excel file: `../../Migrations/Data/TableMigrationReport-10-Mar-26.xlsx`
- Review migration notes in `../../Migrations/` folder
- Search existing code for patterns

---

## Statistics

**Active Issues:** 3  
**Resolved Issues:** 0  
**Blocked Tasks:** 0

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-26 | Initial issues document created |

---

**Found a new issue? Document it here immediately!** ??
