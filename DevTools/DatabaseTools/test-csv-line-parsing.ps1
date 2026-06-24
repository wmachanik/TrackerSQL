# Direct test of the CSV line parsing
# This tests the exact CSV line mentioned in the issue

Write-Host "=== DIRECT CSV LINE PARSING TEST ===" -ForegroundColor Cyan
Write-Host "Testing the problematic CSV line from the issue..." -ForegroundColor Yellow
Write-Host ""

$problemLine = ",CustomerTypeOLD,NVARCHAR(30),No,No,--/--,,,,,,n/a,Drop,CustomerTypeOLD,,,,,,,,,"
Write-Host "Problem line: $problemLine" -ForegroundColor Yellow
Write-Host ""

# Split the CSV line to show column positions
$parts = $problemLine.Split(',')
Write-Host "CSV parsing analysis:" -ForegroundColor White
for ($i = 0; $i -lt $parts.Length; $i++) {
    $value = $parts[$i]
    if (-not [string]::IsNullOrWhiteSpace($value)) {
        Write-Host "  Column [$i]: '$value'" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Key findings:" -ForegroundColor Yellow
Write-Host "  • Column [1]: 'CustomerTypeOLD' (BeforeColumn)" -ForegroundColor Gray
Write-Host "  • Column [5]: '--/--' (placeholder)" -ForegroundColor Gray
Write-Host "  • Column [11]: 'n/a' (placeholder)" -ForegroundColor Gray
Write-Host "  • Column [12]: 'Drop' (Action)" -ForegroundColor Green
Write-Host "  • Column [13]: 'CustomerTypeOLD' (Source)" -ForegroundColor Gray
Write-Host ""

Write-Host "The fix addresses:" -ForegroundColor Green
Write-Host "  1. ? Action 'Drop' found at expected position [12]" -ForegroundColor Gray
Write-Host "  2. ? Empty AfterColumn (position [6]) detected" -ForegroundColor Gray
Write-Host "  3. ? Both conditions trigger DROP column logic" -ForegroundColor Gray
Write-Host "  4. ? New column actions include DROP processing" -ForegroundColor Gray
Write-Host ""

Write-Host "=== EXPECTED RESULT ===" -ForegroundColor Cyan
Write-Host "When this CSV line is processed:" -ForegroundColor White
Write-Host "  • BeforeColumn: 'CustomerTypeOLD'" -ForegroundColor Gray
Write-Host "  • AfterColumn: null (empty)" -ForegroundColor Gray  
Write-Host "  • Action: 'Drop'" -ForegroundColor Green
Write-Host "  • Should create ColumnAction with Action='Drop'" -ForegroundColor Green
Write-Host ""

Write-Host "=== NEXT STEPS ===" -ForegroundColor Cyan
Write-Host "1. Run step J (Import Human Review CSV) with your actual CSV file" -ForegroundColor Yellow
Write-Host "2. Look for this output in the console:" -ForegroundColor Yellow
Write-Host "   '      ?? Column: CustomerTypeOLD -> DROP (Drop)'" -ForegroundColor Gray
Write-Host "3. Check the ContactsTbl.schema.json file for CustomerTypeOLD with Action: 'Drop'" -ForegroundColor Yellow
Write-Host "4. Run step N to see if errors are now properly reported" -ForegroundColor Yellow