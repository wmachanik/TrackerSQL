-- Fix invalid date values in AccessSrc tables before data migration
-- These tables have date conversion errors preventing migration

USE [OtterDb];
GO

PRINT 'Fixing bad dates in AccessSrc.ClientUsageTbl...';

-- ContactsUsageTbl (from ClientUsageTbl)
UPDATE [AccessSrc].[ClientUsageTbl]
SET [NextCoffeeBy] = NULL
WHERE TRY_CONVERT(DATETIME, [NextCoffeeBy]) IS NULL
  AND [NextCoffeeBy] IS NOT NULL;

UPDATE [AccessSrc].[ClientUsageTbl]
SET [NextCleanOn] = NULL
WHERE TRY_CONVERT(DATETIME, [NextCleanOn]) IS NULL
  AND [NextCleanOn] IS NOT NULL;

UPDATE [AccessSrc].[ClientUsageTbl]
SET [NextFilterEst] = NULL
WHERE TRY_CONVERT(DATETIME, [NextFilterEst]) IS NULL
  AND [NextFilterEst] IS NOT NULL;

UPDATE [AccessSrc].[ClientUsageTbl]
SET [NextDescaleEst] = NULL
WHERE TRY_CONVERT(DATETIME, [NextDescaleEst]) IS NULL
  AND [NextDescaleEst] IS NOT NULL;

UPDATE [AccessSrc].[ClientUsageTbl]
SET [NextServiceEst] = NULL
WHERE TRY_CONVERT(DATETIME, [NextServiceEst]) IS NULL
  AND [NextServiceEst] IS NOT NULL;

PRINT 'Fixing bad dates in AccessSrc.RepairsTbl...';

-- RepairsTbl
UPDATE [AccessSrc].[RepairsTbl]
SET [DateLogged] = NULL
WHERE TRY_CONVERT(DATETIME, [DateLogged]) IS NULL
  AND [DateLogged] IS NOT NULL;

UPDATE [AccessSrc].[RepairsTbl]
SET [LastStatusChange] = NULL
WHERE TRY_CONVERT(DATETIME, [LastStatusChange]) IS NULL
  AND [LastStatusChange] IS NOT NULL;

PRINT 'Fixing bad dates in AccessSrc.TempCoffeecheckupCustomerTbl...';

-- TempCoffeecheckupCustomerTbl
UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextPrepDate] = NULL
WHERE TRY_CONVERT(DATETIME, [NextPrepDate]) IS NULL
  AND [NextPrepDate] IS NOT NULL;

UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextDeliveryDate] = NULL
WHERE TRY_CONVERT(DATETIME, [NextDeliveryDate]) IS NULL
  AND [NextDeliveryDate] IS NOT NULL;

UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextCoffee] = NULL
WHERE TRY_CONVERT(DATETIME, [NextCoffee]) IS NULL
  AND [NextCoffee] IS NOT NULL;

UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextClean] = NULL
WHERE TRY_CONVERT(DATETIME, [NextClean]) IS NULL
  AND [NextClean] IS NOT NULL;

UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextFilter] = NULL
WHERE TRY_CONVERT(DATETIME, [NextFilter]) IS NULL
  AND [NextFilter] IS NOT NULL;

UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextDescal] = NULL
WHERE TRY_CONVERT(DATETIME, [NextDescal]) IS NULL
  AND [NextDescal] IS NOT NULL;

UPDATE [AccessSrc].[TempCoffeecheckupCustomerTbl]
SET [NextService] = NULL
WHERE TRY_CONVERT(DATETIME, [NextService]) IS NULL
  AND [NextService] IS NOT NULL;

PRINT 'Date fixes complete!';
PRINT '';
PRINT 'Now re-run Option N (Apply Data Migration) to migrate these tables.';
