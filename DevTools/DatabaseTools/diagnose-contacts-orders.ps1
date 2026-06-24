# Diagnose Orders/Contacts FK Issue
# This script will run diagnostics to understand why all orders are being skipped during migration

param(
    [string]$ConnectionString = $null
)

Write-Host "=== ORDERS/CONTACTS FK DIAGNOSTIC TOOL ===" -ForegroundColor Cyan
Write-Host "This will help diagnose why all orders are being skipped during migration" -ForegroundColor Yellow
Write-Host ""

# Get connection string
if (-not $ConnectionString) {
    Write-Host "Please provide SQL Server connection string:" -ForegroundColor Green
    Write-Host "Example: Server=.\SQLEXPRESS;Database=TrackerData;Integrated Security=True;" -ForegroundColor Gray
    $ConnectionString = Read-Host "Connection String"
}

if (-not $ConnectionString) {
    Write-Error "Connection string is required"
    exit 1
}

try {
    # Test connection
    Write-Host "Testing connection..." -ForegroundColor Yellow
    $conn = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $conn.Open()
    Write-Host "? Connection successful" -ForegroundColor Green
    
    # Run diagnostic script
    Write-Host ""
    Write-Host "Running diagnostic queries..." -ForegroundColor Yellow
    
    $diagnosticSql = Get-Content "diagnose-contacts-orders.sql" -Raw
    
    $cmd = New-Object System.Data.SqlClient.SqlCommand($diagnosticSql, $conn)
    $cmd.CommandTimeout = 300  # 5 minutes
    
    # Execute and capture output
    $reader = $cmd.ExecuteReader()
    
    # Process all result sets
    do {
        if ($reader.HasRows) {
            Write-Host ""
            Write-Host "Query Results:" -ForegroundColor Cyan
            
            # Get column names
            $columns = @()
            for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                $columns += $reader.GetName($i)
            }
            
            # Display header
            $header = $columns -join "`t"
            Write-Host $header -ForegroundColor White
            Write-Host ("-" * $header.Length) -ForegroundColor Gray
            
            # Display rows
            while ($reader.Read()) {
                $row = @()
                for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                    $value = if ($reader.IsDBNull($i)) { "<NULL>" } else { $reader.GetValue($i).ToString() }
                    $row += $value
                }
                Write-Host ($row -join "`t")
            }
        }
    } while ($reader.NextResult())
    
    $reader.Close()
    $conn.Close()
    
    Write-Host ""
    Write-Host "=== DIAGNOSTIC COMPLETE ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "NEXT STEPS based on common issues:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. IF ContactsTbl is empty:" -ForegroundColor White
    Write-Host "   Run: cd Migrations\MigrationRunner" -ForegroundColor Gray  
    Write-Host "        dotnet run --configuration Release -- (select Z for full pipeline)" -ForegroundColor Gray
    Write-Host "   Or:  dotnet run --configuration Release -- (select M, then N for data migration)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. IF CustomerID ranges don't match:" -ForegroundColor White
    Write-Host "   - The Access CustomerIDs don't exist as ContactIDs in the target" -ForegroundColor Gray
    Write-Host "   - May need to check if CustomersTbl -> ContactsTbl migration succeeded" -ForegroundColor Gray
    Write-Host "   - Or verify ID remapping during migration" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. IF AccessSrc.OrdersTbl is missing:" -ForegroundColor White
    Write-Host "   Run: dotnet run --configuration Release -- (select MS for staging)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "4. IF Column mapping issue:" -ForegroundColor White
    Write-Host "   - Check if CSV import correctly mapped CustomerID -> ContactID" -ForegroundColor Gray
    Write-Host "   - Verify OrdersTbl.schema.json has correct ColumnActions" -ForegroundColor Gray

} catch {
    Write-Error "Error running diagnostic: $_"
    if ($conn -and $conn.State -eq 'Open') {
        $conn.Close()
    }
    exit 1
}

Write-Host ""
Write-Host "For more help, check the Migration_OrphanedOrders table for specific failing ContactIDs" -ForegroundColor Cyan