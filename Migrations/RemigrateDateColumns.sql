-- Re-migrate ContactsItemsPredictedTbl with dates from AccessSrc.ClientUsageTbl
-- Run this after Import-AccessClientUsageDates.ps1 has successfully imported dates

USE OtterDb;
GO

PRINT 'Step 1: Checking for orphan ContactIDs in AccessSrc.ClientUsageTbl...';
SELECT 
    COUNT(*) AS OrphanCount
FROM AccessSrc.ClientUsageTbl src
WHERE NOT EXISTS (
    SELECT 1 FROM ContactsTbl c WHERE c.ContactID = src.CustomerId
);
GO

PRINT 'Step 2: Clearing ContactsItemsPredictedTbl...';
DELETE FROM ContactsItemsPredictedTbl;
PRINT '  Deleted ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows';
GO

PRINT 'Step 3: Migrating data with dates (only valid ContactIDs)...';
INSERT INTO [ContactsItemsPredictedTbl]
(
    [ContactID], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], 
    [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], 
    [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
)
SELECT
    src.[CustomerId] AS [ContactID], 
    src.[LastCupCount], 
    src.[NextCoffeeBy],  -- Already a proper datetime from PowerShell import
    src.[NextCleanOn],   -- Already a proper datetime from PowerShell import
    src.[NextFilterEst], -- Already a proper datetime from PowerShell import
    src.[NextDescaleEst],-- Already a proper datetime from PowerShell import
    src.[NextServiceEst],-- Already a proper datetime from PowerShell import
    src.[DailyConsumption], 
    src.[FilterAveCount], 
    src.[DescaleAveCount], 
    src.[ServiceAveCount], 
    src.[CleanAveCount]
FROM [AccessSrc].[ClientUsageTbl] src
INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerId  -- Only migrate valid ContactIDs
WHERE src.CustomerId IS NOT NULL;

PRINT '  Inserted ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows';
GO

PRINT 'Step 4: Verifying results...';
SELECT 
    COUNT(*) AS TotalRows,
    SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS NextCoffeeBy_WithDates,
    SUM(CASE WHEN NextCleanOn IS NOT NULL THEN 1 ELSE 0 END) AS NextCleanOn_WithDates,
    SUM(CASE WHEN NextFilterEst IS NOT NULL THEN 1 ELSE 0 END) AS NextFilterEst_WithDates,
    SUM(CASE WHEN NextDescaleEst IS NOT NULL THEN 1 ELSE 0 END) AS NextDescaleEst_WithDates,
    SUM(CASE WHEN NextServiceEst IS NOT NULL THEN 1 ELSE 0 END) AS NextServiceEst_WithDates
FROM ContactsItemsPredictedTbl;
GO

PRINT 'Step 5: Sample data (first 10 rows)...';
SELECT TOP 10
    ContactID,
    LastCupCount,
    CONVERT(VARCHAR(10), NextCoffeeBy, 120) AS NextCoffeeBy,
    CONVERT(VARCHAR(10), NextCleanOn, 120) AS NextCleanOn,
    CONVERT(VARCHAR(10), NextFilterEst, 120) AS NextFilterEst,
    CONVERT(VARCHAR(10), NextDescaleEst, 120) AS NextDescaleEst,
    CONVERT(VARCHAR(10), NextServiceEst, 120) AS NextServiceEst,
    DailyConsumption
FROM ContactsItemsPredictedTbl
ORDER BY ContactID;
GO

PRINT '';
PRINT '========================================';
PRINT 'Migration complete!';
PRINT 'Dates are now populated in ContactsItemsPredictedTbl.';
PRINT '========================================';
