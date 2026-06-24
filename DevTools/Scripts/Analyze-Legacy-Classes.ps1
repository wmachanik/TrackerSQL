# Analyzes legacy Controls\*Tbl.cs classes and matches them to POCOs/Repositories
# Created: 2026-05-15
# Updated: 2026-05-15 - Added naming convention mappings
# Purpose: Create migration mapping without AI, accounting for table/column renames

param(
    [string]$RootPath = "C:\SRC\ASP.net\TrackerSQL",
    [string]$OutputFile = "DevTools\Documentation\Legacy_To_Poco_Mapping.csv"
)

Write-Host "Legacy Class Analysis (with naming convention mapping)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# Naming convention mappings from migration CSV
function Get-MigratedPocoName {
    param([string]$LegacyName)

    # Remove Tbl suffix first
    $name = $LegacyName -replace 'Tbl$', ''

    # Apply systematic renames based on migration CSV
    $mappings = @{
        'Customers' = 'Contact'           # CustomersTbl -> Contact
        'ClientUsage' = 'ContactsItemsPredicted'  # ClientUsageTbl -> ContactsItemsPredicted
        'ClientUsageLines' = 'ContactsUsage'      # ClientUsageLinesTbl -> ContactsUsage
        'ItemUsage' = 'ContactsItemUsage'         # ItemUsageTbl -> ContactsItemUsage
        'City' = 'Area'                   # CityTbl -> Area
        'CityPrepDays' = 'AreaPrepDays'  # CityPrepDaysTbl -> AreaPrepDays
        'ItemType' = 'Item'               # ItemTypeTbl -> Item
        'Packaging' = 'ItemPackaging'     # PackagingTbl -> ItemPackaging
        'PrepTypes' = 'ItemPrepType'      # PrepTypesTbl -> ItemPrepType
        'MachineConditions' = 'EquipCondition'  # MachineConditionsTbl -> EquipCondition
        'Reoccuring' = 'Recurring'        # ReoccuringOrderTbl -> Recurring
        'ReoccuranceType' = 'RecurranceType'  # ReoccuranceTypeTbl -> RecurranceType
        'Persons' = 'Person'              # PersonsTbl -> Person (singular)
        'HolidayClosure' = 'HolidayClosure'  # No change
        'NextRoastDateByCity' = 'NextPreperationDateByArea'  # NextRoastDateByCityTbl -> NextPreperationDateByArea
        'ServiceTypes' = 'ItemServiceType'  # ServiceTypesTbl -> ItemServiceType
        'ClientAwayPeriod' = 'ContactsAwayPeriod'  # ClientAwayPeriodTbl -> ContactsAwayPeriod
        'CustomersAccInfo' = 'ContactsAccInfo'     # CustomersAccInfoTbl -> ContactsAccInfo
        'CustomerType' = 'ContactType'             # CustomerTypeTbl -> ContactType
        'EquipType' = 'EquipType'                  # No change
        'ItemGroup' = 'ItemGroup'                  # No change
        'CustomersAway' = 'ContactsAwayPeriod'     # CustomersAwayTbl -> ContactsAwayPeriod
        'CustomerTrackedServiceItems' = 'ContactTrackedServiceItem'  # CustomerTrackedServiceItemsTbl -> ContactTrackedServiceItem
    }

    # Check for exact match in mappings
    foreach ($key in $mappings.Keys) {
        if ($name -eq $key) {
            return $mappings[$key]
        }
    }

    # If no exact match, apply general rules
    # Customer* -> Contact*
    $name = $name -replace '^Customer', 'Contact'
    $name = $name -replace '^Client', 'Contact'

    # City* -> Area*
    $name = $name -replace '^City', 'Area'

    # Machine* -> Equip*
    $name = $name -replace '^Machine', 'Equip'

    # Reoccur* -> Recurr*
    $name = $name -replace 'Reoccur', 'Recurr'

    # ItemType* -> Item* (but not ItemGroup, ItemPackaging, etc.)
    if ($name -eq 'ItemType') {
        $name = 'Item'
    }

    return $name
}

# Find all legacy table classes
$legacyClasses = Get-ChildItem -Path "$RootPath\Controls" -Filter "*Tbl.cs" -File

# Find all POCO classes
$pocoClasses = Get-ChildItem -Path "$RootPath\Classes\Poco" -Filter "*.cs" -File

# Find all Repository classes
$repoClasses = Get-ChildItem -Path "$RootPath\Classes\Sql" -Filter "*Repository.cs" -File

$results = @()

foreach ($legacy in $legacyClasses) {
    $legacyName = $legacy.BaseName
    $content = Get-Content $legacy.FullName -Raw

    # Get migrated POCO name using naming convention mappings
    $pocoName = Get-MigratedPocoName -LegacyName $legacyName

    # Also try plural/singular variations
    $pocoNamePlural = "${pocoName}s"
    $pocoNameSingular = $pocoName -replace 's$', ''

    # Check if POCO exists (try multiple variations)
    $pocoExists = $pocoClasses | Where-Object { 
        $_.BaseName -eq $pocoName -or 
        $_.BaseName -eq $pocoNamePlural -or 
        $_.BaseName -eq $pocoNameSingular 
    }

    # If found with variation, use that name
    if ($pocoExists) {
        $pocoName = $pocoExists.BaseName
    }

    # Check if Repository exists (try multiple variations)
    $repoName = "${pocoName}Repository"
    $repoNamePlural = "${pocoNamePlural}Repository"
    $repoNameSingular = "${pocoNameSingular}Repository"

    $repoExists = $repoClasses | Where-Object { 
        $_.BaseName -eq $repoName -or 
        $_.BaseName -eq $repoNamePlural -or
        $_.BaseName -eq $repoNameSingular
    }

    # If found with variation, use that name
    if ($repoExists) {
        $repoName = $repoExists.BaseName
    }

    # Count methods in legacy class
    $methodMatches = [regex]::Matches($content, 'public\s+\w+\s+\w+\s*\(')
    $methodCount = $methodMatches.Count

    # Categorize methods
    $insertMethods = ([regex]::Matches($content, 'public\s+\w+\s+Insert\w*\(')).Count
    $updateMethods = ([regex]::Matches($content, 'public\s+\w+\s+Update\w*\(')).Count
    $deleteMethods = ([regex]::Matches($content, 'public\s+\w+\s+Delete\w*\(')).Count
    $getMethods = ([regex]::Matches($content, 'public\s+\w+\s+Get\w*\(')).Count
    $customMethods = $methodCount - ($insertMethods + $updateMethods + $deleteMethods + $getMethods)

    # Extract custom method names
    $customMethodNames = @()
    foreach ($match in $methodMatches) {
        $methodSignature = $match.Value
        if ($methodSignature -notmatch 'Insert|Update|Delete|Get') {
            $methodName = ($methodSignature -replace 'public\s+\w+\s+', '' -replace '\s*\(.*', '')
            $customMethodNames += $methodName
        }
    }

    $results += [PSCustomObject]@{
        LegacyClass = $legacyName
        PocoName = $pocoName
        PocoExists = if ($pocoExists) { "YES" } else { "NO" }
        RepoName = $repoName
        RepoExists = if ($repoExists) { "YES" } else { "NO" }
        TotalMethods = $methodCount
        InsertMethods = $insertMethods
        UpdateMethods = $updateMethods
        DeleteMethods = $deleteMethods
        GetMethods = $getMethods
        CustomMethods = $customMethods
        CustomMethodNames = ($customMethodNames -join "; ")
        Priority = if (-not $repoExists) { "HIGH" } elseif ($customMethods -gt 0) { "MEDIUM" } else { "LOW" }
    }
}

# Export to CSV
$results | Export-Csv -Path $OutputFile -NoTypeInformation

Write-Host "Analysis Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Results Summary:" -ForegroundColor Yellow
Write-Host "  Legacy Classes Found: $($legacyClasses.Count)"
Write-Host "  POCOs Exist: $(($results | Where-Object { $_.PocoExists -eq 'YES' }).Count)"
Write-Host "  Repos Exist: $(($results | Where-Object { $_.RepoExists -eq 'YES' }).Count)"
Write-Host "  Need Repos: $(($results | Where-Object { $_.RepoExists -eq 'NO' }).Count)"
Write-Host "  Have Custom Logic: $(($results | Where-Object { $_.CustomMethods -gt 0 }).Count)"
Write-Host ""
Write-Host "Report saved to: $OutputFile" -ForegroundColor Green

# Display high priority items
$highPriority = $results | Where-Object { $_.Priority -eq "HIGH" } | Sort-Object CustomMethods -Descending
if ($highPriority.Count -gt 0) {
    Write-Host ""
    Write-Host "HIGH PRIORITY (Missing Repositories):" -ForegroundColor Red
    $highPriority | ForEach-Object {
        Write-Host "  - $($_.LegacyClass) -> $($_.RepoName) (Custom: $($_.CustomMethods))"
    }
}

# Display medium priority items
$mediumPriority = $results | Where-Object { $_.Priority -eq "MEDIUM" } | Sort-Object CustomMethods -Descending | Select-Object -First 5
if ($mediumPriority.Count -gt 0) {
    Write-Host ""
    Write-Host "MEDIUM PRIORITY (Has Repo, Custom Logic):" -ForegroundColor Yellow
    $mediumPriority | ForEach-Object {
        Write-Host "  - $($_.LegacyClass) -> $($_.RepoName)"
        Write-Host "    Custom Methods: $($_.CustomMethodNames)"
    }
}
