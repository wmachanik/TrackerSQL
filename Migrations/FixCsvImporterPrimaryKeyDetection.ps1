# Fix CSV Importer to properly detect primary keys from the "After Key" column in the CSV
# This fixes the issue where ContactUsageLinesTbl, ContactsItemsPredictedTbl, and ContactsItemUsageTbl
# have wrong primary keys because the importer only looked at naming conventions, not the CSV data

$importerFile = "Migrations\MigrationRunner\PlanHumanReviewImporter.cs"

Write-Host "Reading $importerFile..." -ForegroundColor Cyan
$content = Get-Content $importerFile -Raw

# Step 1: Add IsPrimaryKey and IsIdentity properties to ColumnMapping class
Write-Host "Step 1: Adding IsPrimaryKey and IsIdentity properties to ColumnMapping class..." -ForegroundColor Yellow

$columnMappingClassOld = @'
    public class ColumnMapping
    {
        public string BeforeColumn { get; set; }
        public string AfterColumn { get; set; }
        public string Action { get; set; }
        public string NormalizationTarget { get; set; } // "Header" or "Lines"
    }
'@

$columnMappingClassNew = @'
    public class ColumnMapping
    {
        public string BeforeColumn { get; set; }
        public string AfterColumn { get; set; }
        public string Action { get; set; }
        public string NormalizationTarget { get; set; } // "Header" or "Lines"
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
    }
'@

if ($content -match [regex]::Escape($columnMappingClassOld)) {
    $content = $content -replace [regex]::Escape($columnMappingClassOld), $columnMappingClassNew
    Write-Host "  ? Added IsPrimaryKey and IsIdentity properties" -ForegroundColor Green
} else {
    Write-Host "  ? ColumnMapping class pattern not found or already updated" -ForegroundColor Yellow
}

# Step 2: Update the column parser to read After Key and After Auto columns
Write-Host "Step 2: Updating column parser to read After Key (position 8) and After Auto (position 9)..." -ForegroundColor Yellow

# Find the section where columns are parsed and add afterKey and afterAuto
$parserOld = @'
                                    var beforeCol = columnParts[1]?.Trim(); // Before Col name
                                    var afterCol = columnParts[6]?.Trim();  // After Col Name
                                    var colAction = columnParts[12]?.Trim(); // Action
'@

$parserNew = @'
                                    var beforeCol = columnParts[1]?.Trim(); // Before Col name
                                    var afterCol = columnParts[6]?.Trim();  // After Col Name
                                    var afterKey = columnParts[8]?.Trim();  // After Key (PK, FK, No)
                                    var afterAuto = columnParts[9]?.Trim(); // After Auto (Yes/No for IDENTITY)
                                    var colAction = columnParts[12]?.Trim(); // Action
'@

if ($content -match [regex]::Escape($parserOld)) {
    $content = $content -replace [regex]::Escape($parserOld), $parserNew
    Write-Host "  ? Added afterKey and afterAuto column reading" -ForegroundColor Green
} else {
    Write-Host "  ? Column parser pattern not found or already updated" -ForegroundColor Yellow
}

# Step 3: Update ColumnMapping creation to include IsPrimaryKey and IsIdentity
Write-Host "Step 3: Updating ColumnMapping object creation to include PK and Identity flags..." -ForegroundColor Yellow

$mappingCreationOld = @'
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = beforeCol,
                                            AfterColumn = afterCol,
                                            Action = colAction
                                        };
                                        
                                        mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;
                                        
                                        Console.WriteLine($"      ? Column: {beforeCol} -> {afterCol ?? "DROP"} ({colAction})");
'@

$mappingCreationNew = @'
                                        // Detect primary key and identity from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = beforeCol,
                                            AfterColumn = afterCol,
                                            Action = colAction,
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity
                                        };
                                        
                                        mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;
                                        
                                        var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        Console.WriteLine($"      ? Column: {beforeCol} -> {afterCol ?? "DROP"} ({colAction}){pkMarker}{identityMarker}");
'@

if ($content -match [regex]::Escape($mappingCreationOld)) {
    $content = $content -replace [regex]::Escape($mappingCreationOld), $mappingCreationNew
    Write-Host "  ? Updated ColumnMapping creation with PK and Identity detection" -ForegroundColor Green
} else {
    Write-Host "  ? ColumnMapping creation pattern not found or already updated" -ForegroundColor Yellow
}

# Step 4: Update CreateConstraintsForTable to use IsPrimaryKey from CSV
Write-Host "Step 4: Updating CreateConstraintsForTable to prioritize CSV 'After Key' data..." -ForegroundColor Yellow

$constraintsOld = @'
            // Look for ID columns in the column mappings to find the actual primary key
            if (mapping.ColumnMappings?.Any() == true)
            {
                // Find columns that are likely primary keys (end with "ID")
                var idColumns = mapping.ColumnMappings
                    .Where(cm => !string.IsNullOrEmpty(cm.BeforeColumn) && 
                                !string.IsNullOrEmpty(cm.AfterColumn) &&
                                cm.BeforeColumn.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // For normalized tables, look for the table-specific ID (e.g., OrderID for OrdersTbl)
                // Priority order: 1) Table-specific ID, 2) Generic "ID", 3) Others
                var tableBaseName = mapping.BeforeTable.Replace("Tbl", "");
                var expectedPkName = tableBaseName + "ID"; // e.g., "OrderID" for "OrdersTbl"
                
                var pkColumn = idColumns.FirstOrDefault(cm => 
                    string.Equals(cm.BeforeColumn, expectedPkName, StringComparison.OrdinalIgnoreCase)) ??
                    idColumns.FirstOrDefault(cm => 
                        string.Equals(cm.BeforeColumn, "ID", StringComparison.OrdinalIgnoreCase)) ??
                    idColumns.FirstOrDefault(); // Fallback to any ID column

                if (pkColumn != null)
                {
                    var finalPkName = pkColumn.AfterColumn;
                    Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={finalPkName} (mapped from {pkColumn.BeforeColumn})");
                    
                    constraintTable.PrimaryKey.Add(finalPkName);
                    constraintTable.IdentityColumns.Add(finalPkName);
                }
                else
                {
                    Console.WriteLine($"    ? No obvious primary key found for {targetTable}");
                }
            }
'@

$constraintsNew = @'
            // FIRST: Look for columns explicitly marked as PK in the CSV (most reliable)
            var pkColumns = mapping.ColumnMappings?
                .Where(cm => cm.IsPrimaryKey && !string.IsNullOrEmpty(cm.AfterColumn))
                .ToList();

            if (pkColumns?.Any() == true)
            {
                foreach (var pkCol in pkColumns)
                {
                    constraintTable.PrimaryKey.Add(pkCol.AfterColumn);
                    
                    // Add to identity columns if marked as identity
                    if (pkCol.IsIdentity)
                    {
                        constraintTable.IdentityColumns.Add(pkCol.AfterColumn);
                    }
                    
                    Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={pkCol.AfterColumn} (from CSV After Key=PK, Identity={pkCol.IsIdentity})");
                }
            }
            // FALLBACK: Look for ID columns in the column mappings (legacy logic)
            else if (mapping.ColumnMappings?.Any() == true)
            {
                // Find columns that are likely primary keys (end with "ID")
                var idColumns = mapping.ColumnMappings
                    .Where(cm => !string.IsNullOrEmpty(cm.BeforeColumn) && 
                                !string.IsNullOrEmpty(cm.AfterColumn) &&
                                cm.BeforeColumn.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // For normalized tables, look for the table-specific ID (e.g., OrderID for OrdersTbl)
                // Priority order: 1) Table-specific ID, 2) Generic "ID", 3) Others
                var tableBaseName = mapping.BeforeTable.Replace("Tbl", "");
                var expectedPkName = tableBaseName + "ID"; // e.g., "OrderID" for "OrdersTbl"
                
                var pkColumn = idColumns.FirstOrDefault(cm => 
                    string.Equals(cm.BeforeColumn, expectedPkName, StringComparison.OrdinalIgnoreCase)) ??
                    idColumns.FirstOrDefault(cm => 
                        string.Equals(cm.BeforeColumn, "ID", StringComparison.OrdinalIgnoreCase)) ??
                    idColumns.FirstOrDefault(); // Fallback to any ID column

                if (pkColumn != null)
                {
                    var finalPkName = pkColumn.AfterColumn;
                    Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={finalPkName} (heuristic fallback from {pkColumn.BeforeColumn})");
                    
                    constraintTable.PrimaryKey.Add(finalPkName);
                    constraintTable.IdentityColumns.Add(finalPkName);
                }
                else
                {
                    Console.WriteLine($"    ? No obvious primary key found for {targetTable}");
                }
            }
'@

if ($content -match [regex]::Escape($constraintsOld)) {
    $content = $content -replace [regex]::Escape($constraintsOld), $constraintsNew
    Write-Host "  ? Updated CreateConstraintsForTable to use CSV PK data" -ForegroundColor Green
} else {
    Write-Host "  ? CreateConstraintsForTable pattern not found or already updated" -ForegroundColor Yellow
}

# Write updated content
Write-Host ""
Write-Host "Writing updated file..." -ForegroundColor Cyan
Set-Content -Path $importerFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "? CSV Importer Primary Key Detection Fixed!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Changes made:" -ForegroundColor Cyan
Write-Host "  1. Added IsPrimaryKey and IsIdentity properties to ColumnMapping" -ForegroundColor White
Write-Host "  2. Updated CSV parser to read 'After Key' column (position 8)" -ForegroundColor White
Write-Host "  3. Updated CSV parser to read 'After Auto' column (position 9)" -ForegroundColor White
Write-Host "  4. Modified CreateConstraintsForTable to prioritize CSV PK data" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Build the solution to verify changes compile" -ForegroundColor White
Write-Host "  2. Run MigrationRunner and select option 'Z' (Import from CSV)" -ForegroundColor White
Write-Host "  3. Verify PlanConstraints.json now has correct PKs for:" -ForegroundColor White
Write-Host "     - ContactUsageLinesTbl (PK = ContactUsageLineNo)" -ForegroundColor White
Write-Host "     - ContactsItemsPredictedTbl (PK = ContactsItemsPredictedID)" -ForegroundColor White
Write-Host "     - ContactsItemUsageTbl (PK = ContactItemUsageLineNo)" -ForegroundColor White
Write-Host "  4. Regenerate CREATE TABLE script (option M)" -ForegroundColor White
Write-Host "  5. Regenerate data migration script (option N)" -ForegroundColor White
Write-Host ""
