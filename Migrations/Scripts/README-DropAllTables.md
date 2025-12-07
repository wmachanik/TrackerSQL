# Drop All Tables Functionality

The MigrationRunner now includes powerful table cleanup functionality to help with migration testing and database resets.

## Available Options

### Option X: Drop ALL Tables (MigrationRunner Console)
The safest and most comprehensive way to drop all tables.

**Location**: MigrationRunner Console Application  
**Command**: `X`  
**Safety Level**: ????? (Triple confirmation required)

#### Usage:
1. Open command prompt/PowerShell
2. Navigate to `Migrations\MigrationRunner`
3. Run: `dotnet run` (or use the compiled exe)
4. Select option `X` from the menu
5. Follow the safety confirmations:
   - Type exactly: `DROP ALL TABLES`
   - Type exactly: `YES DELETE EVERYTHING`

#### Features:
- ? **Triple safety confirmation** - prevents accidental execution
- ? **Connection testing** - verifies database connectivity before proceeding
- ? **Smart preservation** - keeps ASP.NET membership and system tables
- ? **Progress reporting** - shows detailed progress during execution
- ? **Error handling** - provides helpful troubleshooting information
- ? **Table counting** - shows exactly how many tables will be affected
- ? **Batch mode support** - can be used in automated pipelines

#### What it preserves:
- ASP.NET membership tables (`aspnet_%`)
- WebPages framework tables (`webpages_%`)
- Entity Framework migration history (`__MigrationHistory`)
- System and metadata tables

#### What it removes:
- All user-created tables
- All foreign key constraints
- All table data

#### Next Steps After Using Option X:
```bash
# Recommended workflow after dropping all tables:
1. Run Option A (Generate CREATE TABLE DDL)
2. Run Option C (Apply DDL scripts)  
3. Run Option $ (Full migration pipeline)
```

### Alternative: Direct SQL Script
**Location**: `Migrations\Scripts\DropAllTables.sql`  
**Safety Level**: ?? (Manual execution required)

#### Usage:
1. Update the database name in the script: `USE [YourDatabaseName]`
2. Execute via SSMS or command line:
   ```bash
   sqlcmd -S .\SQLEXPRESS -d YourDatabase -i "Migrations\Scripts\DropAllTables.sql"
   ```

## When to Use

### ? Good Use Cases:
- **Testing migrations** - Clean slate for testing migration scripts
- **Development resets** - Starting over with fresh schema
- **Migration troubleshooting** - Fixing corrupt migration states
- **Schema redesign** - Major structural changes requiring clean start

### ? Avoid Using When:
- **Production databases** - Never use on production data
- **Shared development** - Check with team before cleaning shared databases  
- **Partial resets** - Use Option R for targeted cleanup instead
- **Data preservation needed** - This deletes ALL data permanently

## Troubleshooting

### Permission Issues:
- Ensure you have `ALTER` and `DROP` permissions on the database
- Check that your login has `db_ddladmin` or `db_owner` roles

### Connection Issues:
- Verify SQL Server service is running
- Test connection strings in MigrationConfig.json
- Check firewall settings for SQL Server

### Foreign Key Constraints:
- The script automatically handles FK constraints by disabling them first
- If you see constraint errors, there may be cross-schema dependencies

### Active Connections:
- Close all applications using the database
- Use Activity Monitor in SSMS to check for blocking connections

## Safety Features

### Multiple Confirmation Layers:
1. **Menu Warning** - Clear warning text before option selection
2. **First Confirmation** - Must type exact phrase "DROP ALL TABLES"
3. **Final Confirmation** - Must type exact phrase "YES DELETE EVERYTHING"
4. **Connection Test** - Verifies database connectivity before proceeding

### Smart Preservation:
The system automatically preserves important system tables while removing all user data tables.

### Detailed Logging:
All operations are logged with:
- Table counts (before/after)
- Error messages with troubleshooting hints
- Success confirmations
- Timing information

## Integration with Migration Pipeline

The Drop All Tables functionality integrates seamlessly with the full migration pipeline:

```
X (Drop All) ? A (Create Schema) ? C (Apply DDL) ? $ (Full Migration)
```

This workflow ensures a completely clean migration from scratch.

## Configuration

The Drop All Tables command uses the same configuration as other MigrationRunner commands:

- **Connection String**: From `MigrationConfig.json` or interactive prompt
- **Batch Mode**: Respects `RunRangeState` for automated execution
- **Working Directory**: Uses standard MigrationRunner working paths

## Examples

### Interactive Development Workflow:
```bash
cd Migrations\MigrationRunner
dotnet run
# Select: X
# Type: DROP ALL TABLES  
# Type: YES DELETE EVERYTHING
# Select: A (Generate CREATE TABLE DDL)
# Select: $ (Full migration pipeline)
```

### Automated Testing Workflow:
```bash
# In CI/CD pipeline or test script
# (Configuration would be automated via RunRangeState)
```

## Related Commands

- **Option R**: Clean specific migration tables (safer, targeted cleanup)
- **Option A**: Generate CREATE TABLE DDL (first step after cleanup)
- **Option $**: Full migration pipeline (complete end-to-end migration)
- **Option Z**: Full migration sequence (includes all steps)

---

**?? IMPORTANT REMINDER**: Always backup your database before using any drop table functionality. This operation is **IRREVERSIBLE** and will **DELETE ALL DATA**.