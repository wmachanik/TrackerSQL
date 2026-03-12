# Post-Migration Code Refactoring Checklist

## Overview
After running the migration with renamed tables and columns, the application code must be updated to match.

## Files to Rename/Update

### 1. POCO Classes (Classes/Poco/)

#### Rename Files:
- [ ] `ContactsUsage.cs` ? `ContactsItemsPredicted.cs`
- [ ] `ContactUsageLine.cs` ? `ContactUsage.cs`
- [ ] `ContactUsageItem.cs` ? Delete (duplicate of ContactsItemUsage)
- [ ] `ContactsItemUsage.cs` ? Keep as-is

#### Update Class Names:
```csharp
// OLD:
public class ContactsUsage { ... }

// NEW:
public class ContactsItemsPredicted { ... }
```

#### Update Property Names in ALL POCOs:
```csharp
// In Contact.cs (or equivalent):
public string MachineSN { get; set; }  // OLD
public string EquipmentSN { get; set; }  // NEW
```

### 2. Repository Classes (Classes/Sql/)

#### Rename Files:
- [ ] `ContactsUsageRepository.cs` ? `ContactsItemsPredictedRepository.cs`
- [ ] `ContactUsageLinesRepository.cs` ? `ContactsUsageRepository.cs`
- [ ] `ContactsItemUsageRepository.cs` ? Keep as-is

#### Update Class Names and TableName:
```csharp
// OLD: ContactsUsageRepository.cs
public class ContactsUsageRepository : RepositoryBase<ContactsUsage>
{
    protected override string TableName => "ContactsUsageTbl";
    protected override string KeyColumn => "ContactID";
}

// NEW: ContactsItemsPredictedRepository.cs
public class ContactsItemsPredictedRepository : RepositoryBase<ContactsItemsPredicted>
{
    protected override string TableName => "ContactsItemsPredictedTbl";
    protected override string KeyColumn => "ContactID";
}
```

```csharp
// OLD: ContactUsageLinesRepository.cs
public class ContactUsageLinesRepository : RepositoryBase<ContactUsageLine>
{
    protected override string TableName => "ContactUsageLinesTbl";
    protected override string KeyColumn => "ContactUsageLineNo";
}

// NEW: ContactsUsageRepository.cs
public class ContactsUsageRepository : RepositoryBase<ContactUsage>
{
    protected override string TableName => "ContactsUsageTbl";
    protected override string KeyColumn => "ContactUsageLineNo";
}
```

### 3. Legacy Controls (Controls/)

#### Files to Update:
- [ ] `Controls/ClientUsageTbl.cs` - Update all SQL references
- [ ] `Controls/ClientUsageLinesTbl.cs` - Update all SQL references

**Option A:** Update SQL strings to use new table names
**Option B:** Delete these files if they're not used (check references first)

### 4. Search and Replace Operations

Run these searches across the entire solution:

#### Table Name References:
```
Search: ContactsUsageTbl
Replace: ContactsItemsPredictedTbl
Note: Be careful - review each match!

Search: ContactUsageLinesTbl
Replace: ContactsUsageTbl
Note: Be careful - review each match!
```

#### Column Name References:
```
Search: MachineSN
Replace: EquipmentSN
```

#### Class References:
```
Search: ContactsUsage (as class name)
Replace: ContactsItemsPredicted
Note: Don't replace ContactsUsageTbl!

Search: ContactUsageLine (as class name)
Replace: ContactUsage
```

### 5. Page Code-Behinds (Pages/*.aspx.cs)

Check these files for usage table references:
- [ ] `Pages/ContactDetails.aspx.cs`
- [ ] `Pages/Contacts.aspx.cs`
- [ ] Any page that displays or edits usage data

### 6. Manager Classes (Managers/)

Check for any managers that use usage repositories:
- [ ] `Managers/OrderDoneManager.cs`
- [ ] Any manager that calculates or displays usage

### 7. Update using Statements

Find and update namespace references:
```csharp
// OLD:
using var repo = new ContactsUsageRepository();
var usage = repo.GetByContactId(contactId);

// NEW:
using var repo = new ContactsItemsPredictedRepository();
var predicted = repo.GetByContactId(contactId);
```

## Testing Checklist

After code changes, test these areas:

- [ ] Contact Details page loads correctly
- [ ] Contact list displays equipment info (EquipmentSN)
- [ ] Usage predictions are calculated correctly
- [ ] Usage history displays correctly
- [ ] Item usage/delivery tracking works
- [ ] Reports that use usage data still work
- [ ] No build errors or warnings

## Build Verification

1. **Clean Solution**
2. **Rebuild All**
3. **Check for Errors**:
   - Missing type references
   - SQL query errors
   - Null reference warnings

## Database Queries to Update

Search for raw SQL strings containing:
- `FROM ContactsUsageTbl` ? `FROM ContactsItemsPredictedTbl`
- `FROM ContactUsageLinesTbl` ? `FROM ContactsUsageTbl`
- `MachineSN` ? `EquipmentSN`

## Priority Order

1. ? **HIGH**: Repository classes (breaks data access)
2. ? **HIGH**: POCO classes (breaks model binding)
3. ?? **MEDIUM**: Page code-behinds (breaks UI)
4. ?? **MEDIUM**: Manager classes (breaks business logic)
5. ?? **LOW**: Legacy Controls (may not be used)

## Notes

- Keep a backup of the code before making changes
- Use Git branches for refactoring
- Test incrementally after each major change
- Consider creating aliases/adapters if immediate full refactor is not feasible
