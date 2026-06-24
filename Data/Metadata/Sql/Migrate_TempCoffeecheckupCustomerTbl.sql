-- Migration script for table: TempCoffeecheckupCustomerTbl
DELETE FROM [TempCoffeecheckupCustomerTbl];
SET IDENTITY_INSERT [TempCoffeecheckupCustomerTbl] ON;
INSERT INTO [TempCoffeecheckupCustomerTbl] (
[TCCID], [ContactID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [AreaID], [EmailAddress], [AltEmailAddress], [ContactTypeID], [EquipTypeID], [TypicallySecToo], [PreferredAgentID], [SalesAgentID], [UsesFilter], [Enabled], [AlwaysSendChkUp], [ReminderCount], [NextPreperationDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]
)
SELECT
[TCCID], [CustomerID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [CityID], [EmailAddress], [AltEmailAddress], [CustomerTypeID], [EquipTypeID], [TypicallySecToo], [PreferredAgentID], [SalesAgentID], [UsesFilter], [enabled], [AlwaysSendChkUp], [ReminderCount], dbo.SafeDateConvert([NextPreperationDate]) AS [NextPreperationDate], dbo.SafeDateConvert([NextDeliveryDate]) AS [NextDeliveryDate], dbo.SafeDateConvert([NextCoffee]) AS [NextCoffee], dbo.SafeDateConvert([NextClean]) AS [NextClean], dbo.SafeDateConvert([NextFilter]) AS [NextFilter], dbo.SafeDateConvert([NextDescal]) AS [NextDescal], dbo.SafeDateConvert([NextService]) AS [NextService], [RequiresPurchOrder]
FROM [AccessSrc].[TempCoffeecheckupCustomerTbl];
SET IDENTITY_INSERT [TempCoffeecheckupCustomerTbl] OFF;

