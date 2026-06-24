# Quick test to run the CSV import
cd "Migrations\MigrationRunner"

# Build the project
Write-Host "Building MigrationRunner..."
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nRunning CSV import test..."
    
    # Run with option 11 (Import Human Review CSV) 
    $input = "11`n`nq`n"
    $input | dotnet run --configuration Release --no-build
} else {
    Write-Host "Build failed!"
}