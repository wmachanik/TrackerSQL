# Quick debug script to check CSV parsing
$csvPath = "..\..\Data\Metadata\PlanEdits\TableMigrationReport-Dec-1.csv"
$content = Get-Content $csvPath

Write-Host "Looking for normalization header lines..."

for ($i = 0; $i -lt $content.Length; $i++) {
    $line = $content[$i].Trim()
    if ($line.StartsWith("Table:,Before,After Header Tbl")) {
        Write-Host "Found normalization header at line $($i + 1): $line"
        
        # Check next few lines
        for ($j = 1; $j -le 10; $j++) {
            if (($i + $j) -lt $content.Length) {
                Write-Host "  Line $($i + $j + 1): $($content[$i + $j])"
            }
        }
        Write-Host ""
    }
}