# Minimal patch to fix CSV importer to read primary key from CSV "After Key" column
# This is a surgical fix that adds PK detection without restructuring the entire file

$file = "Migrations\MigrationRunner\PlanHumanReviewImporter.cs"
$content = Get-Content $file -Raw

Write-Host "Patching $file to detect primary keys from CSV..." -ForegroundColor Cyan

# Step 1: Add properties to ColumnMapping class
$pattern1 = '(\s+public string NormalizationTarget \{ get; set; \} // "Header" or "Lines"\s+)\}'
$replacement1 = '$1    public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
    }'

if ($content -match $pattern1) {
    $content = $content -replace $pattern1, $replacement1
    Write-Host "? Added IsPrimaryKey and IsIdentity properties" -ForegroundColor Green
} else {
    Write-Host "? Pattern 1 not found - may already be patched" -ForegroundColor Yellow
}

# Step 2: Add reading of afterKey and afterAuto in parser (after line with afterCol)
$pattern2 = '(\s+var afterCol = columnParts\[6\]\?\.Trim\(\);\s+// After Col Name\s+)(\s+var colAction = columnParts\[12\])'
$replacement2 = '$1                                    var afterKey = columnParts[8]?.Trim();  // After Key (PK, FK, No)
                                    var afterAuto = columnParts[9]?.Trim(); // After Auto (Yes/No)
$2'

if ($content -match $pattern2) {
    $content = $content -replace $pattern2, $replacement2
    Write-Host "? Added afterKey and afterAuto reading" -ForegroundColor Green
} else {
    Write-Host "? Pattern 2 not found - may already be patched" -ForegroundColor Yellow
}

# Step 3: Add PK/Identity detection before ColumnMapping creation
$pattern3 = '(\s+// Default column action\s+if \(string\.IsNullOrEmpty\(colAction\)\)\s+\{\s+colAction = string\.Equals\(beforeCol, afterCol[^\}]+\}\s+)(var columnMapping = new ColumnMapping)'
$replacement3 = '$1
                                        // Detect primary key and identity from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        
                                        $2'

if ($content -match $pattern3) {
    $content = $content -replace $pattern3, $replacement3
    Write-Host "? Added PK/Identity detection" -ForegroundColor Green
} else {
    Write-Host "? Pattern 3 not found - may already be patched" -ForegroundColor Yellow
}

# Step 4: Update ColumnMapping initialization to include new properties
$pattern4 = '(var columnMapping = new ColumnMapping\s+\{\s+BeforeColumn = beforeCol,\s+AfterColumn = afterCol,\s+Action = colAction)\s+\};'
$replacement4 = '$1,
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity
                                        };'

if ($content -match $pattern4) {
    $content = $content -replace $pattern4, $replacement4
    Write-Host "? Updated ColumnMapping initialization" -ForegroundColor Green
} else {
    Write-Host "? Pattern 4 not found - may already be patched" -ForegroundColor Yellow
}

# Step 5: Update Console.WriteLine to show PK markers
$pattern5 = 'Console\.WriteLine\(\$"      ? Column: \{beforeCol\} -> \{afterCol \?\? ""DROP""\} \(\{colAction\}\)"\);'
$replacement5 = 'var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        Console.WriteLine($"      ? Column: {beforeCol} -> {afterCol ?? \"DROP\"} ({colAction}){pkMarker}{identityMarker}");'

if ($content -match $pattern5) {
    $content = $content -replace $pattern5, $replacement5
    Write-Host "? Updated console output" -ForegroundColor Green
} else {
    Write-Host "? Pattern 5 not found - may already be patched" -ForegroundColor Yellow
}

# Step 6: Update CreateConstraintsForTable to check IsPrimaryKey FIRST
$pattern6 = '(\s+// Look for ID columns in the column mappings to find the actual primary key\s+)(if \(mapping\.ColumnMappings\?\.Any\(\) == true\))'
$replacement6 = '$1// FIRST: Look for columns explicitly marked as PK in the CSV (most reliable)
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
            // FALLBACK: Look for ID columns (legacy heuristic logic)
            else $2'

if ($content -match $pattern6) {
    $content = $content -replace $pattern6, $replacement6
    Write-Host "? Updated CreateConstraintsForTable" -ForegroundColor Green
} else {
    Write-Host "? Pattern 6 not found - may already be patched" -ForegroundColor Yellow
}

# Step 7: Update the heuristic fallback message
$pattern7 = 'Console\.WriteLine\(\$"    ? Creating constraints for \{targetTable\}: PK=\{finalPkName\} \(mapped from \{pkColumn\.BeforeColumn\}\)"\);'
$replacement7 = 'Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={finalPkName} (heuristic fallback from {pkColumn.BeforeColumn})");'

if ($content -match $pattern7) {
    $content = $content -replace $pattern7, $replacement7
    Write-Host "? Updated fallback message" -ForegroundColor Green
} else {
    Write-Host "? Pattern 7 not found - may already be patched" -ForegroundColor Yellow
}

# Save the file
Set-Content $file -Value $content -NoNewline

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "Patching complete!" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Build the solution to verify the patch worked" -ForegroundColor White
Write-Host "2. If build succeeds, run MigrationRunner option Z to re-import CSV" -ForegroundColor White
Write-Host "3. Check PlanConstraints.json for correct primary keys" -ForegroundColor White
Write-Host "4. Regenerate SQL scripts (options M and N)" -ForegroundColor White
Write-Host ""
Write-Host "If build fails, run: git checkout -- $file" -ForegroundColor Cyan
Write-Host ""
