# Schema Drop Action Verification

## Test the Schema File Reading

Run this PowerShell to verify the schema file is correct:

```powershell
$json = Get-Content "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema\ItemUsageTbl.schema.json" | ConvertFrom-Json
$clientUsageLineNoAction = $json.Plan.ColumnActions | Where-Object { $_.Source -eq "ClientUsageLineNo" }
Write-Host "Source: $($clientUsageLineNoAction.Source)"
Write-Host "Target: $($clientUsageLineNoAction.Target)"
Write-Host "Action: $($clientUsageLineNoAction.Action)"
Write-Host "Expression: $($clientUsageLineNoAction.Expression)"
```

Expected output:
```
Source: ClientUsageLineNo
Target: ContactItemUsageLineNo
Action: Drop
Expression: Let SQL Server auto-generate new identity values since Access has composite PK with duplicates
```

## Check the Generated SQL

Run this to see what the generated SQL actually contains:

```powershell
Get-Content "C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\Sql\DataMigration_LATEST.sql" | 
    Select-String -Pattern "ContactsItemUsageTbl" -Context 3,10 | 
    Select-Object -First 5
```

## The Real Problem

I suspect the issue is that the **DataMigration_LATEST.sql was NOT regenerated** after the schema changes.

**Proof:** Look at the generated SQL - it still shows:
```sql
SET IDENTITY_INSERT [ContactsItemUsageTbl] ON;
INSERT INTO [ContactsItemUsageTbl]
(
    [ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]
```

If the "Drop" action was working, it should:
1. **NOT** include `SET IDENTITY_INSERT ON` (because `useIdentityInsert` would be FALSE)
2. **NOT** include the identity column in the INSERT list (which it doesn't - only 7 columns)

**BUT**: It's still doing `SET IDENTITY_INSERT ON` even though the identity column is NOT in the INSERT!

This is a logic bug!

## The Logic Bug

In `DmlScriptGenerator.cs` lines 181-183:

```csharp
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              tm.Columns.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
var useIdentityInsert = includeIdentityInInsert;
```

But then at line 176:
```csharp
var targetColsLocal = cols.Select(c => Qi(c.Target)).ToList();
```

The problem: `useIdentityInsert` is calculated BEFORE filtering `cols` by `existingTargets`!

## Solution

The calculation of `includeIdentityInInsert` should use `cols` (the filtered list) instead of `tm.Columns` (the raw list).

Change line 182:
```csharp
// OLD:
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              tm.Columns.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));

// NEW:
var includeIdentityInInsert = !string.IsNullOrWhiteSpace(chosenIdentity) &&
                              cols.Any(c => c.Target != null && c.Target.Equals(chosenIdentity, StringComparison.OrdinalIgnoreCase));
```

This way, if the identity column was dropped (not in `cols`), then `includeIdentityInInsert` will be FALSE!
