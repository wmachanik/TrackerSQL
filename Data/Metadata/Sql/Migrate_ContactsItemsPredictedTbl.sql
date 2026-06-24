-- Migration script for table: ContactsItemsPredictedTbl
use [OtterDb]
DELETE FROM [OtterDb].[dbo].[ContactsItemsPredictedTbl];
-- SET IDENTITY_INSERT [ContactsItemsPredictedTbl] ON;
INSERT INTO [OtterDb].[dbo].[ContactsItemsPredictedTbl] (
[ContactID], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
)
SELECT
[CustomerId], [LastCupCount], dbo.SafeDateConvert([NextCoffeeBy]) AS [NextCoffeeBy], dbo.SafeDateConvert([NextCleanOn]) AS [NextCleanOn], dbo.SafeDateConvert([NextFilterEst]) AS [NextFilterEst], dbo.SafeDateConvert([NextDescaleEst]) AS [NextDescaleEst], dbo.SafeDateConvert([NextServiceEst]) AS [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
FROM [AccessSrc].[ClientUsageTbl];
-- SET IDENTITY_INSERT [ContactsItemsPredictedTbl] OFF;
