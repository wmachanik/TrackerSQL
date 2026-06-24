# Generates missing Repository classes with standard CRUD
# Created: 2026-05-15
# Purpose: Create repos from POCOs without AI

param(
    [Parameter(Mandatory=$true)]
    [string]$PocoName,

    [string]$TableName = "",
    [string]$PrimaryKeyColumn = "",
    [switch]$DryRun
)

$RootPath = "C:\SRC\ASP.net\TrackerSQL"
$PocoPath = "$RootPath\Classes\Poco\$PocoName.cs"
$RepoPath = "$RootPath\Classes\Sql\${PocoName}Repository.cs"

# Validate POCO exists
if (-not (Test-Path $PocoPath)) {
    Write-Host "ERROR: POCO not found: $PocoPath" -ForegroundColor Red
    exit 1
}

# Read POCO to extract properties
$pocoContent = Get-Content $PocoPath -Raw

# Extract namespace
$namespaceMatch = [regex]::Match($pocoContent, 'namespace\s+([\w.]+)')
$namespace = if ($namespaceMatch.Success) { $namespaceMatch.Groups[1].Value } else { "TrackerSQL.Models" }

# Extract class name
$classMatch = [regex]::Match($pocoContent, 'public\s+class\s+(\w+)')
$className = if ($classMatch.Success) { $classMatch.Groups[1].Value } else { $PocoName }

# Extract properties
$propertyMatches = [regex]::Matches($pocoContent, 'public\s+(\w+(?:<\w+>)?)\s+(\w+)\s*{\s*get;\s*set;\s*}')
$properties = @()
foreach ($match in $propertyMatches) {
    $properties += @{
        Type = $match.Groups[1].Value
        Name = $match.Groups[2].Value
    }
}

# Guess table name if not provided
if (-not $TableName) {
    $TableName = "${PocoName}Tbl"
}

# Guess primary key if not provided
if (-not $PrimaryKeyColumn) {
    # Look for property ending in ID
    $pkProp = $properties | Where-Object { $_.Name -like "*ID" } | Select-Object -First 1
    if ($pkProp) {
        $PrimaryKeyColumn = $pkProp.Name
    } else {
        Write-Host "WARNING: Could not guess primary key. Using 'ID'" -ForegroundColor Yellow
        $PrimaryKeyColumn = "ID"
    }
}

Write-Host "Generating Repository for $PocoName" -ForegroundColor Cyan
Write-Host "  Table: $TableName"
Write-Host "  Primary Key: $PrimaryKeyColumn"
Write-Host "  Properties: $($properties.Count)"
Write-Host ""

# Generate repository code
$repoCode = @"
using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Repository for $PocoName entity
    /// Auto-generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    public class ${PocoName}Repository : RepositoryBase<$PocoName>
    {
        protected override string TableName => "$TableName";
        protected override string KeyColumn => "$PrimaryKeyColumn";

        /// <summary>
        /// Maps a DataReader row to a $PocoName object
        /// </summary>
        protected override $PocoName Map(IDataReader reader)
        {
            return DbMapper.Map<$PocoName>(reader);
        }

        /// <summary>
        /// Gets all $PocoName records
        /// </summary>
        /// <param name="sortColumn">Column to sort by (optional)</param>
        public List<$PocoName> GetAll(string sortColumn = null)
        {
            string sql = ``$"SELECT * FROM {TableName}"```;
            if (!string.IsNullOrEmpty(sortColumn))
            {
                sql += ``$" ORDER BY {sortColumn}"```;
            }

            return ExecuteQuery(sql);
        }

        /// <summary>
        /// Gets a single $PocoName by ID
        /// </summary>
        public $PocoName GetById(int id)
        {
            string sql = ``$"SELECT * FROM {TableName} WHERE {KeyColumn} = @Id"```;
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            return ExecuteQuerySingle(sql, parameters);
        }

        /// <summary>
        /// Inserts a new $PocoName record
        /// </summary>
        public int Insert($PocoName entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // TODO: Customize INSERT statement based on POCO properties
            // This is a template - adjust column names as needed
            string sql = @"
                INSERT INTO $TableName (
                    -- Add column names here
                )
                VALUES (
                    -- Add parameter names here (@Param1, @Param2, etc.)
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

        /// <summary>
        /// Updates an existing $PocoName record
        /// </summary>
        public bool Update($PocoName entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // TODO: Customize UPDATE statement based on POCO properties
            string sql = @"
                UPDATE $TableName
                SET 
                    -- Add column = @Param pairs here
                WHERE $PrimaryKeyColumn = @Id";

            var parameters = new List<DBParameter>
            {
                // TODO: Add parameters based on POCO properties
                // Don't forget to add the ID parameter
            };

            int result = ExecuteNonQuery(sql, parameters);
            return result > 0;
        }

        /// <summary>
        /// Deletes a $PocoName record by ID
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

        // TODO: Add custom methods from legacy ${PocoName}Tbl.cs here
        // Example:
        // public int GetCustomCount()
        // {
        //     string sql = "SELECT COUNT(*) FROM $TableName WHERE CustomCondition";
        //     return ExecuteScalar<int>(sql);
        // }
    }
}
"@

if ($DryRun) {
    Write-Host "DRY RUN - Preview of generated code:" -ForegroundColor Yellow
    Write-Host $repoCode
    Write-Host ""
    Write-Host "To execute, run without -DryRun flag" -ForegroundColor Yellow
} else {
    # Check if file already exists
    if (Test-Path $RepoPath) {
        Write-Host "WARNING: Repository already exists: $RepoPath" -ForegroundColor Yellow
        $response = Read-Host "Overwrite? (y/n)"
        if ($response -ne 'y') {
            Write-Host "Aborted." -ForegroundColor Red
            exit 0
        }
    }

    # Write file
    Set-Content -Path $RepoPath -Value $repoCode -Encoding UTF8
    Write-Host "SUCCESS: Repository created at $RepoPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Open $RepoPath"
    Write-Host "  2. Fill in TODO sections (INSERT/UPDATE statements)"
    Write-Host "  3. Copy custom methods from Controls\${PocoName}Tbl.cs"
    Write-Host "  4. Build and test"
}
