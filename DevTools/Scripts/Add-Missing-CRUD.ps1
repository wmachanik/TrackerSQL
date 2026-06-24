# Adds missing standard CRUD methods to existing repositories
# Adds missing standard CRUD methods to existing repositories
# Created: 2026-05-15
# Updated: 2026-05-15 - RepositoryBase now has full CRUD (Insert/Update/Delete as virtual methods)
# Purpose: Add Insert and Update overrides where missing (Delete already in base with working implementation)

param(
    [string]$RepositoryPath,
    [switch]$DryRun,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Test-MethodExists {
    param([string]$Content, [string]$MethodName)
    return $Content -match "public\s+\w+\s+$MethodName\s*\("
}

function Get-PocoTypeFromRepo {
    param([string]$Content)

    # Extract from: public class SomeRepository : RepositoryBase<SomeType>
    if ($Content -match 'class\s+\w+Repository\s*:\s*RepositoryBase<(\w+)>') {
        return $matches[1]
    }
    return $null
}

function Get-TableNameFromRepo {
    param([string]$Content)

    # Extract from: protected override string TableName => "SomeTableName";
    if ($Content -match 'TableName\s*=>\s*"(\w+)"') {
        return $matches[1]
    }
    return $null
}

function Get-KeyColumnFromRepo {
    param([string]$Content)

    # Extract from: protected override string KeyColumn => "SomeID";
    if ($Content -match 'KeyColumn\s*=>\s*"(\w+)"') {
        return $matches[1]
    }
    return $null
}

function Add-InsertMethod {
    param([string]$Content, [string]$PocoType, [string]$TableName)

    $method = @"

        /// <summary>
        /// Inserts a new $PocoType record
        /// TODO: Fill in column names and parameters
        /// </summary>
        public int Insert($PocoType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // TODO: Customize INSERT statement based on POCO properties
            // Copy from legacy Controls\*Tbl.cs Insert method
            string sql = @"
                INSERT INTO $TableName (
                    -- TODO: Add column names here
                )
                VALUES (
                    -- TODO: Add parameter names here (@Param1, @Param2, etc.)
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                // TODO: Add parameters based on POCO properties
                // Example:
                // new DBParameter { DataValue = entity.PropertyName, DataDbType = DbType.String, ParamName = "@PropertyName" }
            };

            return ExecuteScalar<int>(sql, parameters);
        }
"@

    $lastBrace = $Content.LastIndexOf('}')
    return $Content.Insert($lastBrace, $method)
}

function Add-UpdateMethod {
    param([string]$Content, [string]$PocoType, [string]$TableName, [string]$KeyColumn)

    $method = @"

        /// <summary>
        /// Updates an existing $PocoType record
        /// TODO: Fill in SET clause and parameters
        /// </summary>
        public bool Update($PocoType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // TODO: Customize UPDATE statement based on POCO properties
            // Copy from legacy Controls\*Tbl.cs Update method
            string sql = @"
                UPDATE $TableName
                SET 
                    -- TODO: Add column = @Param pairs here
                WHERE $KeyColumn = @Id";

            var parameters = new List<DBParameter>
            {
                // TODO: Add parameters based on POCO properties
                // Don't forget to add the ID parameter at the end
            };

            int result = ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
"@

    $lastBrace = $Content.LastIndexOf('}')
    return $Content.Insert($lastBrace, $method)
}

function Add-DeleteMethod {
    param([string]$Content, [string]$PocoType)

    $method = @"

        /// <summary>
        /// Deletes a $PocoType record by ID
        /// </summary>
        public bool Delete(int id)
        {
            string sql = ``$"DELETE FROM {TableName} WHERE {KeyColumn} = @Id"```;
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            int result = ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
"@

    $lastBrace = $Content.LastIndexOf('}')
    return $Content.Insert($lastBrace, $method)
}

# Main script
if (-not $RepositoryPath) {
    Write-Host "ERROR: -RepositoryPath required" -ForegroundColor Red
    Write-Host "Usage: .\Add-Missing-CRUD.ps1 -RepositoryPath 'Classes\Sql\SomeRepository.cs'" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $RepositoryPath)) {
    Write-Host "ERROR: Repository not found: $RepositoryPath" -ForegroundColor Red
    exit 1
}

Write-Host "Analyzing: $RepositoryPath" -ForegroundColor Cyan
Write-Host ""

$content = Get-Content $RepositoryPath -Raw
$originalContent = $content

# Extract metadata
$pocoType = Get-PocoTypeFromRepo -Content $content
$tableName = Get-TableNameFromRepo -Content $content
$keyColumn = Get-KeyColumnFromRepo -Content $content

if (-not $pocoType) {
    Write-Host "ERROR: Could not determine POCO type from repository" -ForegroundColor Red
    exit 1
}

Write-Host "Repository Info:" -ForegroundColor Yellow
Write-Host "  POCO Type: $pocoType"
Write-Host "  Table: $tableName"
Write-Host "  Key Column: $keyColumn"
Write-Host ""

# Check which methods are missing
$missingMethods = @()
$existingMethods = @()

# NOTE: RepositoryBase<T> now has Insert, Update, Delete as virtual methods
# But they throw NotImplementedException, so we still need to check if they're overridden
# We check for actual implementation (not just the base throw statement)
$methods = @{
    "Insert" = { Add-InsertMethod -Content $content -PocoType $pocoType -TableName $tableName }
    "Update" = { Add-UpdateMethod -Content $content -PocoType $pocoType -TableName $tableName -KeyColumn $keyColumn }
    # Delete has a working implementation in base, but may need override for custom logic
}

# Note: Delete is now fully implemented in RepositoryBase with working default behavior
# Only add Insert and Update (which must be overridden)

foreach ($methodName in $methods.Keys) {
    $exists = Test-MethodExists -Content $content -MethodName $methodName

    if ($exists) {
        $existingMethods += $methodName
        Write-Host "  ✓ $methodName exists" -ForegroundColor Green
    } else {
        $missingMethods += $methodName
        Write-Host "  ✗ $methodName MISSING" -ForegroundColor Red
    }
}

Write-Host ""

if ($missingMethods.Count -eq 0) {
    Write-Host "All standard CRUD methods already exist! ✓" -ForegroundColor Green
    exit 0
}

Write-Host "Adding $($missingMethods.Count) missing method(s)..." -ForegroundColor Yellow
Write-Host ""

# Add missing methods
foreach ($methodName in $missingMethods) {
    $scriptBlock = $methods[$methodName]
    $content = & $scriptBlock
    Write-Host "  + Added $methodName" -ForegroundColor Cyan
}

if ($DryRun) {
    Write-Host ""
    Write-Host "=== DRY RUN - Preview ===" -ForegroundColor Yellow
    Write-Host $content
    Write-Host ""
    Write-Host "Run without -DryRun to save changes" -ForegroundColor Yellow
} else {
    # Backup original
    $backupPath = "$RepositoryPath.bak"
    Copy-Item $RepositoryPath $backupPath -Force

    # Save updated content
    Set-Content -Path $RepositoryPath -Value $content -Encoding UTF8

    Write-Host ""
    Write-Host "✓ Repository updated successfully!" -ForegroundColor Green
    Write-Host "  Backup: $backupPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Open $RepositoryPath"
    Write-Host "  2. Fill in TODO sections for Insert/Update methods"
    Write-Host "  3. Use legacy Controls\*Tbl.cs as reference"
    Write-Host "  4. Build and test"
}
