# Final comprehensive fix for CSV import PK detection
# This script makes all remaining changes needed

$file = "Migrations\MigrationRunner\PlanHumanReviewImporter.cs"
Write-Host "Applying remaining fixes to $file..." -ForegroundColor Cyan

# Read the file
$content = Get-Content $file -Raw

# Change 3: Add PK/Identity/FK detection in column mapping creation
Write-Host "Change 3: Adding PK/Identity/FK detection..." -ForegroundColor Yellow
$pattern = '(\s+// Default column action\s+if \(string\.IsNullOrEmpty\(colAction\)\)\s+\{\s+colAction = [^\}]+\}\s+)(var columnMapping = new ColumnMapping\s+\{\s+BeforeColumn = beforeCol,\s+AfterColumn = afterCol,\s+Action = colAction\s+\};)'
$replacement = @'
$1
                                        // Detect primary key, identity, and foreign key from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        var isForeignKey = (afterKey ?? "").StartsWith("FK", StringComparison.OrdinalIgnoreCase);
                                        
                                        $2
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity,
                                            IsForeignKey = isForeignKey
                                        };
'@

$content = $content -replace $pattern, $replacement

# Change 4: Update console output
Write-Host "Change 4: Updating console output..." -ForegroundColor Yellow
$pattern2 = 'mapping\.ColumnMappings\.Add\(columnMapping\);\s+columnCount\+\+;\s+Console\.WriteLine\(\$"      ? Column: \{beforeCol\} -> \{afterCol \?\? ""DROP""\} \(\{colAction\}\)"\);'
$replacement2 = @'
mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;
                                        
                                        var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        var fkMarker = isForeignKey ? " [FK]" : "";
                                        Console.WriteLine($"      ? Column: {beforeCol} -> {afterCol ?? \"DROP\"} ({colAction}){pkMarker}{identityMarker}{fkMarker}");
'@

$content = $content -replace $pattern2, $replacement2

# Change 6: Update CreateConstraintsForTable 
Write-Host "Change 6: Updating CreateConstraintsForTable..." -ForegroundColor Yellow
$pattern3 = '// Look for ID columns in the column mappings to find the actual primary key\s+if \(mapping\.ColumnMappings\?\.Any\(\) == true\)'
$replacement3 = @'
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
            // FALLBACK: Look for ID columns (legacy heuristic logic)
            else if (mapping.ColumnMappings?.Any() == true)
'@

$content = $content -replace $pattern3, $replacement3

# Change: Exclude FKs from heuristic
Write-Host "Excluding FKs from heuristic PK detection..." -ForegroundColor Yellow
$pattern4 = '(var idColumns = mapping\.ColumnMappings\s+\.Where\(cm => !string\.IsNullOrEmpty\(cm\.BeforeColumn\) &&\s+!string\.IsNullOrEmpty\(cm\.AfterColumn\) &&\s+cm\.BeforeColumn\.EndsWith\("ID", StringComparison\.OrdinalIgnoreCase\)\))'
$replacement4 = @'
var idColumns = mapping.ColumnMappings
                    .Where(cm => !string.IsNullOrEmpty(cm.BeforeColumn) && 
                                !string.IsNullOrEmpty(cm.AfterColumn) &&
                                cm.BeforeColumn.EndsWith("ID", StringComparison.OrdinalIgnoreCase) &&
                                !cm.IsForeignKey)
'@

$content = $content -replace $pattern4, $replacement4

# Save
Set-Content $file -Value $content -NoNewline

Write-Host ""
Write-Host "? All changes applied!" -ForegroundColor Green
Write-Host "Next: Build the solution and run option 11 to re-import CSV" -ForegroundColor Cyan
